using System.Text;
using System.Security.Cryptography;
using System.IO;
using System;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;

namespace SaveSystem
{
    /// <summary>
    /// Encrypts and Decrypts string or byte[]
    /// </summary>
    public class Cryptography
    {
        private const string DEFAULT_KEY = "<ö$pw%#42§>";

        private string key;
        public string Key { get { return key; } }

        #region CONSTRUCTOR
        public Cryptography()
        {
            key = DEFAULT_KEY;
        }
        public Cryptography(string key)
        {
            this.key = key;
        }
        #endregion

        #region ENCRYPT
        public string Encrypt<T>(T toEncrypt)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(toEncrypt.GetType());
                MemoryStream memStrm = new MemoryStream();
                UTF8Encoding utf8e = new UTF8Encoding();
                XmlTextWriter xmlSink = new XmlTextWriter(memStrm, utf8e);
                xmlSerializer.Serialize(xmlSink, toEncrypt);
                byte[] utf8EncodedData = memStrm.ToArray();
                return EncryptString(utf8e.GetString(utf8EncodedData));
            }
            catch (Exception exc)
            {
                Debug.LogError("Error while encrypting: Read ERRORS-file.\n" + exc);
                return "";
            }
        }
        #endregion

        #region DECRYPT
        public T Decrypt<T>(string toDecrypt)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

                using (TextReader textReader = new StringReader(DecryptString(toDecrypt)))
                {
                    return (T)xmlSerializer.Deserialize(textReader);
                }
            }
            catch (Exception exc)
            {
                Debug.LogError("Error while decrypting: Read ERRORS-file.\n" + exc);
                return default(T);
            }
        }
        #endregion

        #region CRYPTOGRAPHY PROCESS
        private string EncryptString(string toEncrypt)
        {
            byte[] input = Encoding.UTF8.GetBytes(toEncrypt);
            byte[] output = Encrypt(input, key);
            return Convert.ToBase64String(output);
        }
        public string DecryptString(string toDecrypt)
        {
            byte[] input = Convert.FromBase64String(toDecrypt);
            byte[] output = Decrypt(input, key);
            return Encoding.UTF8.GetString(output);
        }

        #region TRANSFORM
        private static byte[] Transform(byte[] input, ICryptoTransform CryptoTransform)
        {
            MemoryStream memStream = new MemoryStream();
            CryptoStream cryptStream = new CryptoStream(memStream, CryptoTransform, CryptoStreamMode.Write);

            cryptStream.Write(input, 0, input.Length);
            cryptStream.FlushFinalBlock();

            memStream.Position = 0;
            byte[] result = new byte[Convert.ToInt32(memStream.Length)];
            memStream.Read(result, 0, Convert.ToInt32(result.Length));

            memStream.Close();
            cryptStream.Close();

            return result;
        }
        #endregion

        #region ENCRYPT
        private static byte[] Encrypt(byte[] input, string password)
        {
            try
            {
                TripleDESCryptoServiceProvider service = new TripleDESCryptoServiceProvider();
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

                byte[] key = md5.ComputeHash(Encoding.ASCII.GetBytes(password));
                byte[] iv = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

                return Transform(input, service.CreateEncryptor(key, iv));
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }
        #endregion

        #region DECRYPT
        private static byte[] Decrypt(byte[] input, string password)
        {
            try
            {
                TripleDESCryptoServiceProvider service = new TripleDESCryptoServiceProvider();
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

                byte[] key = md5.ComputeHash(Encoding.ASCII.GetBytes(password));
                byte[] iv = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

                return Transform(input, service.CreateDecryptor(key, iv));
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }
        #endregion
        #endregion
    }
}
