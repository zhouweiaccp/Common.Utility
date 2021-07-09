using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DotNetCommon.Extensions;

namespace DotNetCommon.Encrypt
{
    /// <summary>
    /// Sha256加密算法，类似MD5
    /// </summary>
    public static class Sha256
    {
        /// <summary>
        /// MD5加密字符串（32位大写）
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string SHA256Hash(string source)
        {
            if (source.IsNullOrEmpty()) return source;
            byte[] data = Encoding.UTF8.GetBytes(source);
            SHA256 shaM = new SHA256Managed();
            var hashBytes = shaM.ComputeHash(data);
            return Base64UrlSafe.Encode(hashBytes);
        }

        /// <summary>
        /// Sha256加密流（32位大写）
        /// </summary>
        /// <param name="stream">要操作的流(自动将指针位置调整到0)</param>
        /// <returns>加密后的字符串</returns>
        public static string SHA256Hash(Stream stream)
        {
            if (stream == null) throw new Exception("流不能为null!");
            stream.Position = 0;
            SHA256 shaM = new SHA256Managed();
            var hashBytes = shaM.ComputeHash(stream);
            return Base64UrlSafe.Encode(hashBytes);
        }

        /// <summary>
        /// Sha256加密文件
        /// </summary>
        /// <param name="absPath">要操作的文件绝对路径</param>
        /// <returns>加密后的字符串</returns>
        public static string SHA256HashFile(string absPath)
        {
            if (absPath.IsNullOrEmptyOrWhiteSpace()) return absPath;
            using var stream = new FileStream(absPath, FileMode.Open);
            return SHA256Hash(stream);
        }
    }
}
