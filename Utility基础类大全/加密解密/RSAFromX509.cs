using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DotNetCommon.Extensions;

namespace DotNetCommon.Encrypt
{
    /// <summary>
    /// 该方式使用cer证书和pfx证书进行加解密
    /// </summary>
    public sealed class RSAFromX509
    {
        /// <summary>
        /// 证书私钥文件绝对路径(*.pfx)
        /// </summary>
        public string CertificatePrivateKeyAbsoluteFilePath { get; }

        /// <summary>
        /// 证书公钥文件绝对路径(*.cer)
        /// </summary>
        public string CertificatePublicKeyAbsoluteFilePath { get; }
        /// <summary>
        /// 证书的私钥文件的秘钥
        /// </summary>
        public string CertificatePrivatePassword { get; }

        /// <summary>
        /// 根据公钥文件、私钥文件、私钥文件的访问密码创建对象实例
        /// </summary>
        /// <param name="certificatePublicKeyAbsoluteFilePath"></param>
        /// <param name="certificatePrivateKeyAbsoluteFilePath"></param>
        /// <param name="certificatePrivatePassword"></param>
        public RSAFromX509(string certificatePublicKeyAbsoluteFilePath, string certificatePrivateKeyAbsoluteFilePath, string certificatePrivatePassword)
        {
            CertificatePublicKeyAbsoluteFilePath = certificatePublicKeyAbsoluteFilePath;
            CertificatePrivateKeyAbsoluteFilePath = certificatePrivateKeyAbsoluteFilePath;
            CertificatePrivatePassword = certificatePrivatePassword;
        }

        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="m_strDecryptString">要解密字符串</param>
        /// <returns></returns>
        public string Decrypt(string m_strDecryptString) => Decrypt(CertificatePrivateKeyAbsoluteFilePath, CertificatePrivatePassword, m_strDecryptString);

        /// <summary>
        /// RSA加密方法
        /// </summary>
        /// <param name="m_strEncryptString">要加密字符串</param>
        /// <returns>返回Base64UrlSafe</returns>
        public string Encrypt(string m_strEncryptString) => Encrypt(CertificatePublicKeyAbsoluteFilePath, m_strEncryptString);

        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="m_strDecryptString">待签名的字符串</param>
        /// <returns>返回Base64UrlSafe</returns>
        public string Sign(string m_strDecryptString) => Sign(CertificatePrivateKeyAbsoluteFilePath, CertificatePrivatePassword, m_strDecryptString);

        /// <summary>
        /// 验签
        /// </summary>
        /// <param name="m_strSignString">待验证签名的字符串</param>
        /// <param name="signature">已有的签名</param>
        /// <returns></returns>
        public bool Verify(string m_strSignString, string signature) => Verify(CertificatePublicKeyAbsoluteFilePath, m_strSignString, signature);


        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="certificatePrivateKeyAbsoluteFilePath">私钥证书绝对路径</param>
        /// <param name="certificatePrivatePassword">私钥证书的私钥</param>
        /// <param name="m_strDecryptString">要解密字符串</param>
        /// <returns></returns>
        public static string Decrypt(string certificatePrivateKeyAbsoluteFilePath, string certificatePrivatePassword, string m_strDecryptString)
        {
            if (m_strDecryptString.IsNullOrEmpty()) return m_strDecryptString;
            X509Certificate2 certpfx = GetCertificateFromPfxFile(certificatePrivateKeyAbsoluteFilePath, certificatePrivatePassword);
            string xmlPrivateKey = certpfx.PrivateKey.ToXmlString(true);

            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            provider.FromXmlString(xmlPrivateKey);
            byte[] rgb = Base64UrlSafe.Decode(m_strDecryptString);
            byte[] bytes = provider.Decrypt(rgb, false);
            return new UnicodeEncoding().GetString(bytes);
        }

        /// <summary>
        /// RSA加密方法
        /// </summary>
        /// <param name="certificatePublicKeyAbsoluteFilePath">公钥证书绝对路径</param>
        /// <param name="m_strEncryptString">要加密字符串</param>
        /// <returns>返回Base64UrlSafe</returns>
        public static string Encrypt(string certificatePublicKeyAbsoluteFilePath, string m_strEncryptString)
        {
            if (m_strEncryptString.IsNullOrEmpty()) return m_strEncryptString;
            X509Certificate2 certcer = GetCertFromCerFile(certificatePublicKeyAbsoluteFilePath);
            string xmlPublicKey = certcer.PublicKey.Key.ToXmlString(false);

            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            provider.FromXmlString(xmlPublicKey);
            byte[] bytes = new UnicodeEncoding().GetBytes(m_strEncryptString);

            return Base64UrlSafe.Encode(provider.Encrypt(bytes, false));

        }

        /// <summary>
        /// 指定密钥签名
        /// </summary>
        /// <param name="certificatePrivateKeyAbsoluteFilePath">私钥证书绝对路径</param>
        /// <param name="certificatePrivatePassword">私钥证书的密码</param>
        /// <param name="m_strDecryptString">待签名的字符串</param>
        /// <returns>返回Base64UrlSafe</returns>
        public static string Sign(string certificatePrivateKeyAbsoluteFilePath, string certificatePrivatePassword, string m_strDecryptString)
        {
            if (m_strDecryptString.IsNullOrEmpty()) return m_strDecryptString;
            X509Certificate2 certpfx = GetCertificateFromPfxFile(certificatePrivateKeyAbsoluteFilePath, certificatePrivatePassword);
            string xmlPrivateKey = certpfx.PrivateKey.ToXmlString(true);

            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            provider.FromXmlString(xmlPrivateKey);
            m_strDecryptString = Convert.ToBase64String(new UnicodeEncoding().GetBytes(m_strDecryptString));
            byte[] rgb = Convert.FromBase64String(m_strDecryptString);
            byte[] bytes = provider.SignData(rgb, "SHA1");
            return Base64UrlSafe.Encode(bytes);
        }

        /// <summary>
        /// 指定公钥验签
        /// </summary>
        /// <param name="certificatePublicKeyAbsoluteFilePath">公钥证书绝对路径</param>
        /// <param name="m_strSignString">待验证签名的字符串</param>
        /// <param name="signature">已有的签名</param>
        /// <returns></returns>
        public static bool Verify(string certificatePublicKeyAbsoluteFilePath, string m_strSignString, string signature)
        {
            if (m_strSignString.IsNullOrEmpty())
            {
                if (signature == m_strSignString) return true;
                return false;
            }
            X509Certificate2 certcer = GetCertFromCerFile(certificatePublicKeyAbsoluteFilePath);
            string xmlPublicKey = certcer.PublicKey.Key.ToXmlString(false);

            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            provider.FromXmlString(xmlPublicKey);
            m_strSignString = Base64UrlSafe.Encode(new UnicodeEncoding().GetBytes(m_strSignString));

            byte[] strSignbytes = Base64UrlSafe.Decode(m_strSignString); //new UnicodeEncoding().GetBytes(m_strSignString);
            byte[] signaturebytes = Base64UrlSafe.Decode(signature); //new UnicodeEncoding().GetBytes(signature);

            return provider.VerifyData(strSignbytes, "SHA1", signaturebytes);
        }

        #region 从证书中获取信息
        /// <summary>     
        /// 根据私钥证书得到证书实体，得到实体后可以根据其公钥和私钥进行加解密     
        /// 加解密函数使用DEncrypt的RSACryption类     
        /// </summary>     
        /// <param name="pfxFileName"></param>     
        /// <param name="password"></param>     
        /// <returns></returns>     
        private static X509Certificate2 GetCertificateFromPfxFile(string pfxFileName, string password)
        {
            return new X509Certificate2(pfxFileName, password, X509KeyStorageFlags.Exportable);
        }

        /// <summary>     
        /// 根据公钥证书，返回证书实体     
        /// </summary>     
        /// <param name="cerPath"></param>     
        private static X509Certificate2 GetCertFromCerFile(string cerPath)
        {
            return new X509Certificate2(cerPath);
        }
        #endregion
    }
}
