using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using System.Xml.Serialization;

namespace PurpleTreeSoftware.Network.Common
{
    public static class helper
    {
        /// <summary>
        /// Serialises a class to xml
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataToSerialize"></param>
        /// <returns></returns>
        public static string Serialize<T>(T dataToSerialize)
        {
            try
            {
                var stringwriter = new System.IO.StringWriter();
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(stringwriter, dataToSerialize);
                return stringwriter.ToString();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Deseerialises an xml string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlText"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string xmlText)
        {
            try
            {
                var stringReader = new System.IO.StringReader(xmlText);
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stringReader);
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Decrypts data
        /// </summary>
        /// <param name="data">The data</param>
        /// <param name="passPhrase">The passphrase to use for decryption</param>
        /// <returns></returns>
        public static string Decrypt(IBuffer data, string passPhrase)
        {
            IBuffer iv = null;

            //Create SymmetricKeyAlgorithmProvider
            var symetric = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);

            //Create hash for passPhrase to create symmetric Key
            IBuffer keyBuffer = Hash(passPhrase);
            //Create symmetric key
            CryptographicKey key = symetric.CreateSymmetricKey(keyBuffer);

            //Decrypt data
            IBuffer bufferDecrypted = CryptographicEngine.Decrypt(key, data, iv);
            //Convert binary to string
            return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, bufferDecrypted);
        }

        /// <summary>
        /// Encrypts a string
        /// </summary>
        /// <param name="text">The string to encrypt</param>
        /// <param name="passPhrase">The passphrase to use for encryption</param>
        /// <returns>The encrypted data</returns>
        public static byte[] Encrypt(string text, string passPhrase)
        {
            IBuffer iv = null;

            //Create SymmetricKeyAlgorithmProvider 
            var symetric = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);

            //Create hash for passPhrase to create symmetric Key
            IBuffer keyBuffer = Hash(passPhrase);
            //Create symmetric
            CryptographicKey key = symetric.CreateSymmetricKey(keyBuffer);

            //Convert texto to binary, for encrypt
            IBuffer data = CryptographicBuffer.ConvertStringToBinary(text, BinaryStringEncoding.Utf8);

            //Encrypt data
            //Encrypt method return IBuffer
            return CryptographicEngine.Encrypt(key, data, iv).ToArray();
        }

        /// <summary>
        /// Creates a hash
        /// </summary>
        /// <param name="text">The string to hash</param>
        /// <returns>The cryptographic hash</returns>
        private static IBuffer Hash(string text)
        {
            //Create HashAlgorithmProvider 
            var hash = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);

            //Create Hash object
            CryptographicHash cryptographicHash = hash.CreateHash();

            //Convert string to binary
            IBuffer data = CryptographicBuffer.ConvertStringToBinary(text, BinaryStringEncoding.Utf8);

            //Append data to generate Hash
            cryptographicHash.Append(data);

            //Generate Hash
            return cryptographicHash.GetValueAndReset();
        }

    }
}
