﻿using System.Security;
using System.Security.Cryptography;
using Gorgon.Core;
using Gorgon.Properties;
using Microsoft.IO;

namespace Gorgon.Security;

/// <summary>
/// Encryption functionality that will encrypt or decrypt data based on an AES 256 crypto provider
/// </summary>
public class Aes256Encryption
    : IEncryption
{

    // The stream pool used to retrieve memory streams.
    private static readonly RecyclableMemoryStreamManager _streamManager = new(new RecyclableMemoryStreamManager.Options
    {
        MaximumSmallPoolFreeBytes = int.MaxValue / 2,
        MaximumLargePoolFreeBytes = int.MaxValue
    });

    // The initialization vector and key.
    private readonly (byte[] IV, byte[] Key) _ivKey;



    /// <summary>
    /// Function to extract the symmetric key from the key data passed to the constructor.
    /// </summary>
    /// <param name="keyData">Key data to extract.</param>
    /// <returns>A tuple containing the initialization vector, and the key.</returns>
    private (byte[] IV, byte[] Key) GetSymmetricKey(byte[] keyData)
    {
        if ((_ivKey.IV is not null) && (_ivKey.Key is not null) && (_ivKey.IV.Length > 0) && (_ivKey.Key.Length > 0))
        {
            return _ivKey;
        }

        using MemoryStream stream = _streamManager.GetStream(keyData);
        using var reader = new BinaryReader(stream, Encoding.Default, false);
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

    /// <summary>
    /// Function to decrypt an encrypted byte array.
    /// </summary>
    /// <param name="data">The encrypted data stored in a byte array.</param>
    /// <returns>The decrypted data as a byte array.</returns>
    /// <exception cref="SecurityException">Thrown if no symmetric key was provided, or is invalid.</exception>
    public byte[] Decrypt(byte[] data)
    {
        if ((_ivKey.IV is null) || (_ivKey.Key is null) || (_ivKey.IV.Length == 0) || (_ivKey.Key.Length == 0))
        {
            throw new SecurityException(Resources.GOR_ERR_ENCRYPTION_KEY_NOT_VALID);
        }

        if ((data is null) || (data.Length == 0))
        {
            return [];
        }

        using var aes = Aes.Create();
        using ICryptoTransform transform = aes.CreateDecryptor(_ivKey.Key, _ivKey.IV);
        using RecyclableMemoryStream stream = _streamManager.GetStream();
        using var writer = new CryptoStream(stream, transform, CryptoStreamMode.Write);
        writer.Write(data, 0, data.Length);

        if (!writer.HasFlushedFinalBlock)
        {
            writer.FlushFinalBlock();
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Function to encrypt a byte array.
    /// </summary>
    /// <param name="data">The data to encrypt, stored in a byte array.</param>
    /// <returns>The encrypted data as a byte array.</returns>
    /// <exception cref="SecurityException">Thrown if no symmetric key was provided, or is invalid.</exception>
    public byte[] Encrypt(byte[] data)
    {
        if ((_ivKey.IV is null) || (_ivKey.Key is null) || (_ivKey.IV.Length == 0) || (_ivKey.Key.Length == 0))
        {
            throw new SecurityException(Resources.GOR_ERR_ENCRYPTION_KEY_NOT_VALID);
        }

        if ((data is null) || (data.Length == 0))
        {
            return [];
        }

        using var aes = Aes.Create();
        using ICryptoTransform transform = aes.CreateEncryptor(_ivKey.Key, _ivKey.IV);
        using RecyclableMemoryStream stream = _streamManager.GetStream();
        using var writer = new CryptoStream(stream, transform, CryptoStreamMode.Write);
        writer.Write(data, 0, data.Length);
        writer.FlushFinalBlock();

        return stream.ToArray();
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
            return [];
        }

        textEncoding ??= Encoding.UTF8;

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
        if ((value is null) || (value.Length == 0))
        {
            return string.Empty;
        }

        textEncoding ??= Encoding.UTF8;

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
        using var rndGen = RandomNumberGenerator.Create();
        if (salt is null)
        {
            salt = new byte[32];
            rndGen.GetBytes(salt);
        }

        using var aes = Aes.Create();
        using var hashGen = new Rfc2898DeriveBytes(password, salt, 100, HashAlgorithmName.SHA3_256);
        return (hashGen.GetBytes(aes.BlockSize / 8), hashGen.GetBytes(aes.KeySize / 8));
    }



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
        ArgumentNullException.ThrowIfNull(keyPairFileData);

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
        ArgumentNullException.ThrowIfNull(iv);

        ArgumentNullException.ThrowIfNull(key);

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

}
