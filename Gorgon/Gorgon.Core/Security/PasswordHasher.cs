
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: November 30, 2020 10:31:30 PM
// 


using System.Security.Cryptography;
using Gorgon.Core;

namespace Gorgon.Security;

/// <summary>
/// Provides functionality for hashing and salting password strings
/// </summary>
/// <remarks>
/// <para>
/// This class provides functionality to create hashed and salted passwords using an SHA1 hash algorithm
/// </para>
/// </remarks>
public static class PasswordHasher
{

    // The number of iterations to use when hashing (for key stretching).
    private const int PasswordHashIterations = 20000;
    // The size, in bytes, of the SHA1 hash.
    private const int HashSize = 20;
    // The length of the salt.
    private const int SaltLength = 64;

    /// <summary>
    /// The maximum unhashed password length.
    /// </summary>
    /// <remarks>
    /// If the password supplied exceeds this value, then it will be cropped to this length before hashing.
    /// </remarks>
    public const int MaxiumumPasswordLength = 256;



    /// <summary>
    /// Function to generate a salt value.
    /// </summary>
    /// <returns>A byte array containing the salt data.</returns>
    public static byte[] GenerateSalt() => RandomNumberGenerator.GetBytes(SaltLength);

    /// <summary>
    /// Function to hash and salt a password and return a base-64 encoded string containing encrypted hash and salt values.
    /// </summary>
    /// <param name="password">The password to hash and salt.</param>
    /// <param name="salt">The salt to use with the hashed password.</param>
    /// <returns>The encrypted hashed and salted password as a base-64 encoded string, or <b>null</b> if the hash and salt operation fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="password"/>, or the <paramref name="salt"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="password"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// This will convert the <paramref name="password"/> to a salted hash value using an SHA1 algorithm. The resulting hash data will be encrypted using AES-256 encryption, and returned
    /// as a base-64 encoded string.
    /// </para>
    /// </remarks>
    public static string HashAndSalt(string password, byte[] salt)
    {
        ArgumentNullException.ThrowIfNull(password);

        ArgumentNullException.ThrowIfNull(salt);

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentEmptyException(nameof(password));
        }

        if (salt.Length == 0)
        {
            return string.Empty;
        }

        if (password.Length > MaxiumumPasswordLength)
        {
            password = password[..MaxiumumPasswordLength];
        }

        using Rfc2898DeriveBytes hashProvider = new(password, salt, PasswordHashIterations, HashAlgorithmName.SHA3_256);
        return Convert.ToBase64String(hashProvider.GetBytes(HashSize));
    }

    /// <summary>
    /// Function to generate a initialization vector and key based on the application password.
    /// </summary>
    /// <param name="password">The password to evaluate.</param>
    /// <param name="salt">The second salt value to base the hash on.</param>
    /// <returns>A tuple containing the initialization vector and key.</returns>
    public static (byte[] IV, byte[] key) GenerateIvKey(string password, byte[] salt)
    {
        using Rfc2898DeriveBytes hashProvider = new(password, salt, PasswordHashIterations, HashAlgorithmName.SHA3_256);
        byte[] key = hashProvider.GetBytes(32);
        byte[] iv = hashProvider.GetBytes(16);

        return (iv, key);
    }

}
