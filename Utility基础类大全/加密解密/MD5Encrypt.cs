using DotNetCommon.Encrypt;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DotNetCommon.Extensions;

namespace DotNetCommon.Encrypt
{
    /// <summary>
    /// MD5加密类
    /// </summary>
    public static class MD5Encrypt
    {
        /// <summary>
        /// MD5加密字符串（32位大写）
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <returns>加密后的字符串(16进制表示法)</returns>
        public static string MD5(string source)
        {
            if (source.IsNullOrEmpty()) return source;
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.UTF8.GetBytes(source);
            string result = BitConverter.ToString(md5.ComputeHash(bytes));
            return result.Replace("-", "");
        }

        /// <summary>
        /// MD5加密流（32位大写）
        /// </summary>
        /// <param name="stream">要操作的流(自动将指针位置调整到0)</param>
        /// <returns>加密后的字符串(16进制表示法)</returns>
        public static string MD5(Stream stream)
        {
            if (stream == null) throw new Exception("流不能为null!");
            stream.Position = 0;
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string result = BitConverter.ToString(md5.ComputeHash(stream));
            return result.Replace("-", "");
        }

        /// <summary>
        /// MD5加密文件
        /// </summary>
        /// <param name="absPath">要操作的文件绝对路径</param>
        /// <returns>加密后的字符串(16进制表示法)</returns>
        public static string MD5File(string absPath)
        {
            if (absPath.IsNullOrEmptyOrWhiteSpace()) return absPath;
            using var stream = new FileStream(absPath, FileMode.Open);
            return MD5(stream);
        }
    }
}
