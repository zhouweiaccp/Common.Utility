using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Common
{
    public static class ShutdownHelper
    {
        private enum RestartOptions
        {
            LogOff = 0,
            PowerOff = 8,
            Reboot = 2,
            ShutDown = 1,
            Suspend = -1,
            Hibernate = -2
            //EWX_FORCE = 4 
        }

        private struct LUID
        {
            public int LowPart;
            public int HighPart;
        }

        private struct LUID_AND_ATTRIBUTES
        {

            public LUID pLuid;

            public int Attributes;
        }

        private struct TOKEN_PRIVILEGES
        {

            public int PrivilegeCount;

            public LUID_AND_ATTRIBUTES Privileges;
        }

        private const int TOKEN_ADJUST_PRIVILEGES = 0x20;
        private const int TOKEN_QUERY = 0x8;
        private const int SE_PRIVILEGE_ENABLED = 0x2;
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        private const int EWX_FORCE = 4;
        [DllImport("kernel32", EntryPoint = "LoadLibraryA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr LoadLibrary(string lpLibFileName);
        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int FreeLibrary(IntPtr hLibModule);
        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        [DllImport("Powrprof", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int SetSuspendState(int Hibernate, int ForceCritical, int DisableWakeEvent);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);
        [DllImport("advapi32.dll", EntryPoint = "LookupPrivilegeValueA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int LookupPrivilegeValue(string lpSystemName, string lpName, ref LUID lpLuid);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int AdjustTokenPrivileges(IntPtr TokenHandle, int DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState, int BufferLength, ref TOKEN_PRIVILEGES PreviousState, ref int ReturnLength);
        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int ExitWindowsEx(int uFlags, int dwReserved);
        [DllImport("kernel32", EntryPoint = "FormatMessageA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, int Arguments);

        private static void ExitWindows(RestartOptions how, bool force)
        {
            switch (how)
            {
                case RestartOptions.Suspend:
                    SuspendSystem(false, force);
                    break;
                case RestartOptions.Hibernate:
                    SuspendSystem(true, force);
                    break;
                default:
                    ExitWindows(Convert.ToInt32(how), force);
                    break;
            }
        }

        private static void ExitWindows(int how, bool force)
        {
            EnableToken("SeShutdownPrivilege");
            if (force) how = how | EWX_FORCE;
            if ((ExitWindowsEx(how, 0) == 0)) throw new PrivilegeException(FormatError(Marshal.GetLastWin32Error()));
        }

        private static void EnableToken(string privilege)
        {
            if (!CheckEntryPoint("advapi32.dll", "AdjustTokenPrivileges")) return;
            IntPtr tokenHandle = IntPtr.Zero;
            var privilegeLUID = new LUID();
            var newPrivileges = new TOKEN_PRIVILEGES();
            TOKEN_PRIVILEGES tokenPrivileges = default(TOKEN_PRIVILEGES);
            if ((OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref  tokenHandle)) == 0) throw new PrivilegeException(FormatError(Marshal.GetLastWin32Error()));
            if ((LookupPrivilegeValue("", privilege, ref privilegeLUID)) == 0) throw new PrivilegeException(FormatError(Marshal.GetLastWin32Error()));
            tokenPrivileges.PrivilegeCount = 1;
            tokenPrivileges.Privileges.Attributes = SE_PRIVILEGE_ENABLED;
            tokenPrivileges.Privileges.pLuid = privilegeLUID;
            int Size = 4;
            if ((AdjustTokenPrivileges(tokenHandle, 0, ref tokenPrivileges, 4 + (12 * tokenPrivileges.PrivilegeCount), ref newPrivileges, ref Size)) == 0) throw new PrivilegeException(FormatError(Marshal.GetLastWin32Error()));
        }

        private static void SuspendSystem(bool hibernate, bool force)
        {
            if (!CheckEntryPoint("powrprof.dll", "SetSuspendState")) throw new PlatformNotSupportedException("The SetSuspendState method is not supported on this system!");
            SetSuspendState(Convert.ToInt32((hibernate ? 1 : 0)), Convert.ToInt32((force ? 1 : 0)), 0);
        }

        private static bool CheckEntryPoint(string library, string method)
        {
            IntPtr libPtr = LoadLibrary(library);
            if (!libPtr.Equals(IntPtr.Zero))
            {
                if (!GetProcAddress(libPtr, method).Equals(IntPtr.Zero))
                {
                    FreeLibrary(libPtr);
                    return true;
                }
                FreeLibrary(libPtr);
            }
            return false;
        }

        private static string FormatError(int number)
        {
            var Buffer = new StringBuilder(255);
            FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, IntPtr.Zero, number, 0, Buffer, Buffer.Capacity, 0);
            return Buffer.ToString();
        }
        //注销 
        public static void LogOff()
        {
            
            ExitWindows(RestartOptions.LogOff, false);
        }
        //关闭电源 
        public static void PowerOff()
        {
            
            ExitWindows(RestartOptions.PowerOff, false);
        }
        //重启计算机 
        public static void Reboot()
        {
            
            ExitWindows(RestartOptions.Reboot, false);
        }
        //关闭系统 
        public static void ShutDown()
        {
            
            ExitWindows(RestartOptions.ShutDown, false);
        }
        //待机 
        public static void Suspend()
        {
           
            ExitWindows(RestartOptions.Suspend, false);
        }
        //休眠 
        public static void Hibernate()
        {
            
            ExitWindows(RestartOptions.Hibernate, false);
        }

    }

    public class PrivilegeException : Exception
    {

        public PrivilegeException()
            : base()
        {
        }

        public PrivilegeException(string message)
            : base(message)
        {
        }
    }
}
