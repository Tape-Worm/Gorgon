#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: March 24, 2018 4:00:49 PM
// 
#endregion

using System.Text;

namespace Gorgon.Security
{
    /// <summary>
    /// Utility functionality to allow applications the ability to encrypt and decrypt data using an underlying crypto provider.
    /// </summary>
    public interface IEncryption
    {
        /// <summary>
        /// Function to decrypt an encrypted byte array.
        /// </summary>
        /// <param name="data">The encrypted data stored in a byte array.</param>
        /// <returns>The decrypted data as a byte array.</returns>
        byte[] Decrypt(byte[] data);

        /// <summary>
        /// Function to encrypt a byte array.
        /// </summary>
        /// <param name="data">The data to encrypt, stored in a byte array.</param>
        /// <returns>The encrypted data as a byte array.</returns>
        byte[] Encrypt(byte[] data);

        /// <summary>
        /// Function to encrypt a string.
        /// </summary>
        /// <param name="value">String value to encrypt.</param>
        /// <param name="textEncoding">[Optional] The type of text encoding to use.</param>
        /// <returns>The encrypted string as an array of bytes.</returns>
        /// <remarks>
        /// When the <paramref name="textEncoding"/> is set to <b>null</b> then UTF-8 encoding will be used as the default.
        /// </remarks>
        byte[] EncryptString(string value, Encoding textEncoding = null);

        /// <summary>
        /// Function to decrypt a string.
        /// </summary>
        /// <param name="value">A byte array containing the data to decrypt.</param>
        /// <param name="textEncoding">[Optional] The type of text encoding to use.</param>
        /// <returns>The decrypted string.</returns>
        /// <remarks>
        /// When the <paramref name="textEncoding"/> is set to <b>null</b> then UTF-8 encoding will be used as the default.
        /// </remarks>
        string DecryptString(byte[] value, Encoding textEncoding = null);

        /// <summary>
        /// Function to encrypt a string and return it as a base-64 encoded string.
        /// </summary>
        /// <param name="value">String value to encrypt.</param>
        /// <param name="textEncoding">[Optional] The type of text encoding to use.</param>
        /// <returns>The encrypted string as a base-64 encoded string.</returns>
        /// <remarks>
        /// When the <paramref name="textEncoding"/> is set to <b>null</b> then UTF-8 encoding will be used as the default.
        /// </remarks>
        string EncryptBase64String(string value, Encoding textEncoding = null);

        /// <summary>
        /// Function to decrypt a base 64 string.
        /// </summary>
        /// <param name="base64String">A base 64 encoded and encrypted string.</param>
        /// <param name="textEncoding">[Optional] The type of text encoding to use.</param>
        /// <returns>The decrypted string.</returns>
        /// <remarks>
        /// When the <paramref name="textEncoding"/> is set to <b>null</b> then encoding UTF-8 will be used as the default.
        /// </remarks>
        string DecryptBase64String(string base64String, Encoding textEncoding = null);
    }
}