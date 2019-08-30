using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Gorgon.Core;
using Gorgon.Properties;

namespace Gorgon.Security
{
    /// <summary>
    /// Encryption functionality that will encrypt or decrypt data based on an AES 256 crypto provider.
    /// </summary>
    public class Aes256Encryption
        : IEncryption
    {
        #region Variables.
        // The initialization vector and key.
        private readonly (byte[] IV, byte[] Key) _ivKey;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to extract the symmetric key from the key data passed to the constructor.
        /// </summary>
        /// <param name="keyData">Key data to extract.</param>
        /// <returns>A tuple containing the initialization vector, and the key.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Once again, this thing has no fucking clue that the reader does not close the stream.")]
        private (byte[] IV, byte[] Key) GetSymmetricKey(byte[] keyData)
        {
            if ((_ivKey.IV != null) && (_ivKey.Key != null) && (_ivKey.IV.Length > 0) && (_ivKey.Key.Length > 0))
            {
                return _ivKey;
            }

            using (var stream = new MemoryStream(keyData))
            {
                using (var reader = new BinaryReader(stream, Encoding.Default, false))
                {
                    int ivLength = reader.ReadInt32();
                    int keyLength = reader.ReadInt32();

                    byte[] iv = new byte[ivLength];
                    byte[] key = new byte[keyLength];

                    if ((iv.Length == 0) || (key.Length == 0))
                    {
                        throw new ArgumentException(Resources.GOR_ERR_ENCRYPTION_KEY_NOT_VALID, nameof(keyData));
                    }

                    reader.Read(iv, 0, iv.Length);
                    reader.Read(key, 0, key.Length);

                    return (iv, key);
                }
            }
        }

        /// <summary>
        /// Function to decrypt an encrypted byte array.
        /// </summary>
        /// <param name="data">The encrypted data stored in a byte array.</param>
        /// <returns>The decrypted data as a byte array.</returns>
        /// <exception cref="SecurityException">Thrown if no symmetric key was provided, or is invalid.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This is fine.")]
        public byte[] Decrypt(byte[] data)
        {
            if ((_ivKey.IV == null) || (_ivKey.Key == null) || (_ivKey.IV.Length == 0) || (_ivKey.Key.Length == 0))
            {
                throw new SecurityException(Resources.GOR_ERR_ENCRYPTION_KEY_NOT_VALID);
            }

            if ((data == null) || (data.Length == 0))
            {
                return Array.Empty<byte>();
            }

            using (var aes = new AesManaged())
            using (ICryptoTransform transform = aes.CreateDecryptor(_ivKey.Key, _ivKey.IV))
            using (var stream = new MemoryStream())
            using (var writer = new CryptoStream(stream, transform, CryptoStreamMode.Write))
            {
                writer.Write(data, 0, data.Length);

                if (!writer.HasFlushedFinalBlock)
                {
                    writer.FlushFinalBlock();
                }

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Function to encrypt a byte array.
        /// </summary>
        /// <param name="data">The data to encrypt, stored in a byte array.</param>
        /// <returns>The encrypted data as a byte array.</returns>
        /// <exception cref="SecurityException">Thrown if no symmetric key was provided, or is invalid.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This is fine.")]
        public byte[] Encrypt(byte[] data)
        {
            if ((_ivKey.IV == null) || (_ivKey.Key == null) || (_ivKey.IV.Length == 0) || (_ivKey.Key.Length == 0))
            {
                throw new SecurityException(Resources.GOR_ERR_ENCRYPTION_KEY_NOT_VALID);
            }

            if ((data == null) || (data.Length == 0))
            {
                return Array.Empty<byte>();
            }

            using (var aes = new AesManaged())
            using (ICryptoTransform transform = aes.CreateEncryptor(_ivKey.Key, _ivKey.IV))
            using (var stream = new MemoryStream())
            using (var writer = new CryptoStream(stream, transform, CryptoStreamMode.Write))
            {
                writer.Write(data, 0, data.Length);
                writer.FlushFinalBlock();

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Function to encrypt a string.
        /// </summary>
        /// <param name="value">String value to encrypt.</param>
        /// <param name="textEncoding">[Optional] The type of text encoding to use.</param>
        /// <returns>The encrypted string as an array of bytes.</returns>
        /// <remarks>
        /// When the <paramref name="textEncoding"/> is set to <b>null</b> then UTF-8 encoding will be used as the default.
        /// </remarks>
        /// <exception cref="SecurityException">Thrown if no symmetric key was provided, or is invalid.</exception>
        public byte[] EncryptString(string value, Encoding textEncoding = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Array.Empty<byte>();
            }

            if (textEncoding == null)
            {
                textEncoding = Encoding.UTF8;
            }

            return Encrypt(textEncoding.GetBytes(value));
        }

        /// <summary>
        /// Function to decrypt a string.
        /// </summary>
        /// <param name="value">A byte array containing the data to decrypt.</param>
        /// <param name="textEncoding">[Optional] The type of text encoding to use.</param>
        /// <returns>The decrypted string.</returns>
        /// <remarks>
        /// When the <paramref name="textEncoding"/> is set to <b>null</b> then UTF-8 encoding will be used as the default.
        /// </remarks>
        /// <exception cref="SecurityException">Thrown if no symmetric key was provided, or is invalid.</exception>
        public string DecryptString(byte[] value, Encoding textEncoding = null)
        {
            if ((value == null) || (value.Length == 0))
            {
                return string.Empty;
            }

            if (textEncoding == null)
            {
                textEncoding = Encoding.UTF8;
            }

            return textEncoding.GetString(Decrypt(value));
        }

        /// <summary>
        /// Function to encrypt a string and return it as a base-64 encoded string.
        /// </summary>
        /// <param name="value">String value to encrypt.</param>
        /// <param name="textEncoding">[Optional] The type of text encoding to use.</param>
        /// <returns>The encrypted string as a base-64 encoded string.</returns>
        /// <remarks>
        /// When the <paramref name="textEncoding"/> is set to <b>null</b> then UTF-8 encoding will be used as the default.
        /// </remarks>
        /// <exception cref="SecurityException">Thrown if no symmetric key was provided, or is invalid.</exception>
        public string EncryptBase64String(string value, Encoding textEncoding = null) => string.IsNullOrEmpty(value) ? string.Empty : Convert.ToBase64String(EncryptString(value, textEncoding));

        /// <summary>
        /// Function to decrypt a base 64 string.
        /// </summary>
        /// <param name="base64String">A base 64 encoded and encrypted string.</param>
        /// <param name="textEncoding">[Optional] The type of text encoding to use.</param>
        /// <returns>The decrypted string.</returns>
        /// <remarks>
        /// When the <paramref name="textEncoding"/> is set to <b>null</b> then encoding UTF-8 will be used as the default.
        /// </remarks>
        /// <exception cref="SecurityException">Thrown if no symmetric key was provided, or is invalid.</exception>
        public string DecryptBase64String(string base64String, Encoding textEncoding = null) => string.IsNullOrEmpty(base64String) ? string.Empty : DecryptString(Convert.FromBase64String(base64String));

        /// <summary>
        /// Function to generate an initialization vector and key.
        /// </summary>
        /// <param name="password">The password to use.</param>
        /// <param name="salt">[Optional] The salt to apply to the password when generating the IV/Key.</param>
        /// <returns>A tuple containing the initialization vector and key.</returns>
        /// <remarks>
        /// <para>
        /// If the <paramref name="salt"/> parameter is omitted, then the code will generate one on your behalf.
        /// </para>
        /// </remarks>
	    public static (byte[] IV, byte[] Key) GenerateIvKey(string password, byte[] salt = null)
        {
            using (var rndGen = RandomNumberGenerator.Create())
            {
                if (salt == null)
                {
                    salt = new byte[32];
                    rndGen.GetBytes(salt);
                }

                using (var aes = new AesManaged())
                {
                    using (var hashGen = new Rfc2898DeriveBytes(password, salt, 100, HashAlgorithmName.SHA512))
                    {
                        return (hashGen.GetBytes(aes.BlockSize / 8), hashGen.GetBytes(aes.KeySize / 8));
                    }
                }
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Aes256Encryption"/> class.
        /// </summary>
        /// <param name="keyPairFileData">A byte array containing a initialization and key.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="keyPairFileData"/> is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="keyPairFileData"/> is empty.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="keyPairFileData"/> does not contain valid information.</para>
        /// </exception>
        public Aes256Encryption(byte[] keyPairFileData)
        {
            if (keyPairFileData == null)
            {
                throw new ArgumentNullException(nameof(keyPairFileData));
            }

            if (keyPairFileData.Length == 0)
            {
                throw new ArgumentEmptyException(nameof(keyPairFileData));
            }

            if (keyPairFileData.Length < sizeof(int) * 2)
            {
                throw new ArgumentException(Resources.GOR_ERR_ENCRYPTION_KEY_NOT_VALID, nameof(keyPairFileData));
            }

            _ivKey = GetSymmetricKey(keyPairFileData);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Aes256Encryption"/> class.
        /// </summary>
        /// <param name="iv">The initialization vector to use.</param>
        /// <param name="key">The key to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="iv"/>, or the <paramref name="key"/> parameters are <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="iv"/>, or the <paramref name="key"/> parameters are empty.</exception>
        public Aes256Encryption(byte[] iv, byte[] key)
        {
            if (iv == null)
            {
                throw new ArgumentNullException(nameof(iv));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (iv.Length == 0)
            {
                throw new ArgumentEmptyException(nameof(iv));
            }

            if (key.Length == 0)
            {
                throw new ArgumentEmptyException(nameof(key));
            }

            _ivKey = (iv, key);
        }
        #endregion
    }
}
