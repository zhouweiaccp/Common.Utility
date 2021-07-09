using DotNetCommon.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNetCommon.Encrypt
{
    /// <summary>
    /// AES加解密
    /// </summary>
    public static class AESEncrypt
    {
        /// <summary>
        /// AES加密字符串
        /// </summary>
        /// <param name="encryptString">待加密字符串</param>
        /// <param name="encryptKey">加密密钥ASCII码，最长32位</param>
        /// <exception cref="Exception"></exception>
        /// <returns>加密结果字符串(16进制表示法)</returns>
        public static string Encrypt(string encryptString, string encryptKey)
        {
            if (string.IsNullOrEmpty(encryptString)) return encryptString;
            if (string.IsNullOrWhiteSpace(encryptKey)) throw new Exception("秘钥格式不正确!");
            if (encryptKey.Length > 32) throw new Exception("秘钥长度最大32位!");
            var (key, iv) = getKeyIv(encryptKey);
            using var rijndaelProvider = new RijndaelManaged
            {
                Key = key,
                IV = iv,
                Padding = PaddingMode.PKCS7
            };
            using ICryptoTransform rijndaelEncrypt = rijndaelProvider.CreateEncryptor();
            byte[] inputData = Encoding.UTF8.GetBytes(encryptString);
            byte[] encryptedData = rijndaelEncrypt.TransformFinalBlock(inputData, 0, inputData.Length);
            StringBuilder ret = new StringBuilder();
            foreach (byte b in encryptedData)
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        /// <summary>
        /// AES解密字符串
        /// </summary>
        /// <param name="decryptString">待解密的字符串</param>
        /// <param name="decryptKey">加密密钥ASCII码，最长32位</param>
        /// <returns>解密后的字符串</returns>
        public static string Decrypt(string decryptString, string decryptKey)
        {
            if (string.IsNullOrEmpty(decryptString)) return decryptString;
            if (string.IsNullOrWhiteSpace(decryptKey)) throw new Exception("秘钥格式不正确!");
            if (decryptKey.Length > 32) throw new Exception("秘钥长度最大32位!");
            var (key, iv) = getKeyIv(decryptKey);
            using var rijndaelProvider = new RijndaelManaged
            {
                Key = key,
                IV = iv,
                Padding = PaddingMode.PKCS7
            };
            using ICryptoTransform rijndaelDecrypt = rijndaelProvider.CreateDecryptor();

            int len;
            len = decryptString.Length / 2;
            byte[] inputByteArray = new byte[len];
            int x, i;
            for (x = 0; x < len; x++)
            {
                i = Convert.ToInt32(decryptString.Substring(x * 2, 2), 16);
                inputByteArray[x] = (byte)i;
            }

            byte[] inputData = inputByteArray;
            byte[] decryptedData = rijndaelDecrypt.TransformFinalBlock(inputData, 0, inputData.Length);
            return Encoding.UTF8.GetString(decryptedData);
        }

        /// <summary>
        /// 加密流,返回临时文件的路径
        /// </summary>
        /// <param name="stream">需要加密的流(自动将指针位置调整到0)</param>
        /// <param name="encryptKey">加密密钥,必须为8位ASCII码,否则不成功</param>
        /// <param name="tmpfiledir">临时文件目录</param>
        /// <exception cref="Exception"></exception>
        public static string Encrypt(Stream stream, string encryptKey, string tmpfiledir = null)
        {
            if (stream == null) throw new Exception("流不能为空!");
            if (string.IsNullOrWhiteSpace(encryptKey)) throw new Exception("秘钥格式不正确!");
            if (encryptKey.Length > 32) throw new Exception("秘钥长度最大32位!");
            stream.Position = 0;
            var (key, iv) = getKeyIv(encryptKey);
            string temppath = "";
            if (tmpfiledir.IsNullOrEmptyOrWhiteSpace())
            {
                temppath = System.IO.Path.GetTempPath();
                temppath = Path.Combine(temppath, "DotNetCommon.EncrptTemp");
            }
            else
            {
                temppath = tmpfiledir;
            }
            Directory.CreateDirectory(temppath);
            var guid = DateTime.Now.ToString("yyyyMMdd") + "_" + Guid.NewGuid().ToString();
            temppath = Path.Combine(temppath, guid);
            var fileStream = new FileStream(temppath, FileMode.Create);

            using var rijndaelProvider = new RijndaelManaged
            {
                Key = key,
                IV = iv,
                Padding = PaddingMode.PKCS7
            };
            using var encrypto = rijndaelProvider.CreateEncryptor();
            var cStream = new CryptoStream(fileStream, encrypto, CryptoStreamMode.Write);
            var bs = new byte[1024];
            var len = 0;
            do
            {
                len = stream.Read(bs, 0, bs.Length);
                cStream.Write(bs, 0, len);
            } while (len > 0);
            cStream.FlushFinalBlock();
            fileStream.Flush();
            fileStream.Close();
            return temppath;
        }

        /// <summary>
        /// 解密流，返回临时文件的路径
        /// </summary>
        /// <param name="stream">需要解密的流(自动将指针位置调整到0)</param>
        /// <param name="decryptKey">解密密钥</param>
        /// <param name="tmpfiledir">临时文件目录</param>
        public static string Decrypt(Stream stream, string decryptKey, string tmpfiledir = null)
        {
            if (stream == null) throw new Exception("流不能为空!");
            if (string.IsNullOrWhiteSpace(decryptKey)) throw new Exception("秘钥格式不正确!");
            if (decryptKey.Length > 32) throw new Exception("秘钥长度最大32位!");
            stream.Position = 0;
            var (key, iv) = getKeyIv(decryptKey);
            string temppath = "";
            if (tmpfiledir.IsNullOrEmptyOrWhiteSpace())
            {
                temppath = System.IO.Path.GetTempPath();
                temppath = Path.Combine(temppath, "DotNetCommon.EncrptTemp");
            }
            else
            {
                temppath = tmpfiledir;
            }
            Directory.CreateDirectory(temppath);
            var guid = DateTime.Now.ToString("yyyyMMdd") + "_" + Guid.NewGuid().ToString();
            temppath = Path.Combine(temppath, guid);
            var fileStream = new FileStream(temppath, FileMode.Create);

            using var rijndaelProvider = new RijndaelManaged
            {
                Key = key,
                IV = iv,
                Padding = PaddingMode.PKCS7
            };
            using var encrypto = rijndaelProvider.CreateDecryptor();
            var cStream = new CryptoStream(fileStream, encrypto, CryptoStreamMode.Write);
            var bs = new byte[1024];
            var len = 0;
            do
            {
                len = stream.Read(bs, 0, bs.Length);
                cStream.Write(bs, 0, len);
            } while (len > 0);
            cStream.FlushFinalBlock();
            fileStream.Flush();
            fileStream.Close();
            return temppath;
        }

        private static (byte[] key, byte[] iv) getKeyIv(string key)
        {
            key = key.PadRight(32, ' ');
            var keyByte = Encoding.ASCII.GetBytes(key.Substring(0, 32));
            var ivByte = new byte[16];
            keyByte.ToList().CopyTo(0, ivByte, 0, 16);
            return (keyByte, ivByte);
        }
    }
}
