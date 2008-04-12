#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Tuesday, April 01, 2008 5:40:50 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Security.Cryptography;
using SharpUtilities.Utility;
using SharpUtilities.IO;
using SharpUtilities.Collections;
using GorgonLibrary.PlugIns;
using GorgonLibrary.Serialization;

namespace GorgonLibrary.FileSystems
{
    /// <summary>
	/// Object representing a packed file system encrypted with 3DES encryption.
    /// </summary>
	[FileSystemInfo("Triple DES File System", false, true, true, "GORPACK1.3DES")]
    public class Gorgon3DESFileSystem
        : FileSystem
    {
        #region Variables.
        private int _maxKeyBytes = 24;                          // Maximum key size in bytes.
        private TripleDESCryptoServiceProvider _des = null;     // Triple DES provider.
		private long _fileOffset = 0;					        // Offset within the archive of the file data.
		private Stream _fileStream = null;				        // File stream for packed file.
        private byte[] _privKey = null;                         // Private encryption key.
        private byte[] _fileKey = null;                         // File encryption key.
        private byte[] _IV = null;                              // Initialization vector.
        private Random _rnd = new Random();                     // Random data.
		private bool _streamIsRoot = false;				// Flag to indicate that the root of the file system is from a stream.
		private long _fileSystemOffset = 0;				// Offset within the file system.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the header for the file system.
		/// </summary>
		protected override string FileSystemHeader
		{
			get 
			{
				return "GORPACK1.3DES";
			}
		}

		/// <summary>
		/// Property to return whether the root of the file system is a stream or not.
		/// </summary>
		/// <value></value>
		public override bool IsRootInStream
		{
			get
			{
				return _streamIsRoot;
			}
		}

		/// <summary>
		/// Property to return the offset of the file system within the stream.
		/// </summary>
		public override long FileSystemStreamOffset
		{
			get
			{
				if (!IsRootInStream)
					return -1;

				return _fileSystemOffset;
			}
			set
			{
				if (!IsRootInStream)
					return;

				_fileSystemOffset = value;
			}
		}

		/// <summary>
		/// Function to read the file system index.
		/// </summary>
		/// <param name="fileSystemStream">Stream from which to read to the index.</param>
		private void ReadIndex(Stream fileSystemStream)
		{
			BinaryReaderEx reader = null;               // Binary reader.			
			string xmlData = string.Empty;				// String to contain the XML data.
			string header = string.Empty;               // Header text.
			int indexSize = 0;							// Size of the directory index.
			byte[] authCompare;                         // Authorization bytes to compare.
			int encryptedSize = 0;                      // Encrypted data size.

			try
			{
				reader = new BinaryReaderEx(fileSystemStream, true);

				// Read the header.
				header = reader.ReadString();
				if (string.Compare(header, FileSystemHeader, true) != 0)
					throw new Exception("Invalid pack file format.");

				// Load in our authorization data.
				encryptedSize = reader.ReadInt32();
				authCompare = TransformData(reader.ReadBytes(encryptedSize), _privKey, true);

				if (authCompare.Length != AuthenticationData.Data.Length)
					throw new FileSystemAccessDeniedException(Name);

				// Ensure the data is the same.
				for (int i = 0; i < authCompare.Length; i++)
				{
					if (authCompare[i] != AuthenticationData.Data[i])
						throw new FileSystemAccessDeniedException(Name);
				}

				// Try to load the file key.
				encryptedSize = reader.ReadInt32();
				_fileKey = TransformData(reader.ReadBytes(encryptedSize), _privKey, true);

				// Get the index size (encrypted).
				indexSize = reader.ReadInt32();

				// Get the XML.
				xmlData = Encoding.UTF8.GetString(TransformData(reader.ReadBytes(indexSize), _fileKey, true));

				// Load into the XML document object.
				FileIndexXML.LoadXml(xmlData);

				// Get the file offset.				
				_fileOffset = fileSystemStream.Position - _fileSystemOffset;
			}
			finally
			{
				if (reader != null)
					reader.Close();
				reader = null;
			}
		}

		/// <summary>
		/// Function to assign the root of this file system.
		/// </summary>
		/// <param name="fileSystemStream">The file stream that will contain the file system.</param>
		/// <remarks>Due to the nature of a file stream, the file system within the stream must be a packed file system.</remarks>
		/// <exception cref="GorgonLibrary.FileSystems.FileSystemRootIsInvalidException">The path to the root is invalid.</exception>
		public override void AssignRoot(Stream fileSystemStream)
		{
			if (fileSystemStream == null)
				throw new ArgumentNullException("fileSystemStream");

			base.AssignRoot(fileSystemStream);
			InitializeIndex("[Stream]->" + Provider.Name + "." + Name);

			// Set up stream binding information.
			_fileStream = fileSystemStream;
			_fileSystemOffset = fileSystemStream.Position;
			_streamIsRoot = true;

			try
			{
				ReadIndex(fileSystemStream);
			}
			catch (Exception ex)
			{
				_fileStream = null;
				_fileSystemOffset = 0;
				_streamIsRoot = false;
				throw new FileSystemRootIsInvalidException("File stream [" + fileSystemStream.ToString() + "]", ex);
			}

			// Validate the index XML.
			ValidateIndexXML();
		}

		/// <summary>
		/// Function to assign the root of this file system.
		/// </summary>
		/// <param name="path">Path to the root of the file system.</param>
		/// <remarks>Path can be a folder that contains the file system XML index for a folder file system or a file (typically
		/// ending with extension .gorPack) for a packed file system.</remarks>
		/// <exception cref="GorgonLibrary.FileSystems.FileSystemRootIsInvalidException">The path to the root is invalid.</exception>
		public override void AssignRoot(string path)
		{
			FileStream stream = null;					// File stream.

			if (path == null)
				path = string.Empty;

			// Append default extension.
			if (Path.GetExtension(path) == string.Empty)
				path = Path.ChangeExtension(path, ".gorPack");

			InitializeIndex(path);
			
			// Check for the archive file.
			if (!File.Exists(Root))
				throw new FileSystemRootIsInvalidException(Root);

            if (AuthenticationData == null)
                throw new InvalidAuthenticationDataException(Name);

            try
            {
				// Disable the root/stream binding.
				_fileSystemOffset = 0;
				_fileStream = null;
				_streamIsRoot = false;
				
				// Open the archive file.
	            stream = File.OpenRead(Root);
				ReadIndex(stream);
            }
            catch (FileSystemAccessDeniedException adex)
            {
                throw adex;
            }
            catch (Exception ex)
            {
                throw new FileSystemRootIsInvalidException(path, ex);
            }
			finally
			{
				// Clean up.
				if (stream != null)
					stream.Dispose();
				stream = null;
			}

			// Validate the index XML.
			ValidateIndexXML();
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function to encode object data.
        /// </summary>
        /// <param name="file">File to re-encode.</param>
        /// <param name="data">Data to encode.</param>
        /// <returns>A new file system entry.</returns>
        private void EncodeData(FileSystemFile file, byte[] data)
        {
            byte[] encryptedData = null;			// Encrypted data.

            try
            {
                // Get compressed data.
                encryptedData = TransformData(data, _fileKey, false);

                file.Data = encryptedData;
            }
            catch (Exception ex)
            {
                throw new CannotCreateException("Cannot encode " + file.FullPath + " to the file system.", ex);
            }
        }
                
        /// <summary>
        /// Function to transform a block of data.
        /// </summary>
        /// <param name="data">Data to encrypt.</param>
        /// <param name="key">Private key.</param>
        /// <param name="decrypt">TRUE to decrypt the data, FALSE to encrypt it</param>
        /// <returns>The block of encrypted data.</returns>
        private byte[] TransformData(byte[] data, byte[] key, bool decrypt)
        {
            MemoryStream encryptedStream = null;    // Encrypted data stream.
            ICryptoTransform transform = null;      // Encryption transform.
            CryptoStream cryptStream = null;        // Crypto stream.
            byte[] encryptedData = null;            // Encrypted data.

            try
            {
                // Create new transformation.
                if (!decrypt)
                    transform = _des.CreateEncryptor(key, _IV);
                else
                    transform = _des.CreateDecryptor(key, _IV);

                encryptedStream = new MemoryStream();
                cryptStream = new CryptoStream(encryptedStream, transform, CryptoStreamMode.Write);
                cryptStream.Write(data, 0, data.Length);
                cryptStream.FlushFinalBlock();

                encryptedStream.Position = 0;
                encryptedData = new byte[(int)encryptedStream.Length];
                encryptedStream.Read(encryptedData, 0, encryptedData.Length);

                return encryptedData;
            }
            finally
            {
                if (cryptStream != null)
                    cryptStream.Dispose();
                if (transform != null)
                    transform.Dispose();
                if (encryptedStream != null)
                    encryptedStream.Dispose();
            }
        }

		/// <summary>
		/// Function to load an object from the file system.
		/// </summary>
		/// <param name="file">File to read.</param>
		/// <returns>The raw binary data for the file.</returns>
		protected override byte[] DecodeData(FileSystemFile file)
		{
			try
			{
				// If not compressed, then leave.
				if (!file.IsEncrypted)
					return file.Data;
				
				return TransformData(file.Data, _fileKey, true);
			}
			catch (Exception ex)
			{
				throw new CannotLoadException("Cannot load " + file.FullPath + " from the file system.", ex);
			}
		}

        /// <summary>
        /// Function to make the encryption key based on our authorization data.
        /// </summary>
        protected override void InitializeSecurity()
        {
            byte[] oldKey = _fileKey;       // Old file key.

            // Clear the key if we have no auth data.
            if (AuthenticationData == null)
            {
                _fileKey = null;
                return;
            }

            // Now create the key used to encrypt the files.
            byte[] newKey = new byte[_maxKeyBytes];

            // Let's create the key based on our authorization data - we'll use every 2nd byte of the password and fill in the odd bytes with a random byte.
            int authDataLength = AuthenticationData.Data.Length / 2;
            if (authDataLength > newKey.Length / 2)
                authDataLength = newKey.Length / 2;

            // Generate the key.
            for (int i = 0; i < newKey.Length; i += 2)
            {
                if (i < authDataLength)
                {
                    newKey[i] = AuthenticationData.Data[i];
                    newKey[i + 1] = (byte)_rnd.Next(i, 255);
                }
                else
                {
                    newKey[i] = (byte)_rnd.Next(i, 255);
                    newKey[i + 1] = (byte)_rnd.Next(i, 255);
                }
            }

            // We have to re-encode each file.
            foreach (FileSystemFile file in this.Paths.GetFiles())
            {
                byte[] fileData = null;     // File data.

                _fileKey = oldKey;
                fileData = DecodeData(file);
                _fileKey = newKey;
                EncodeData(file, fileData);
            }

            _fileKey = newKey;
        }

        /// <summary>
		/// Function to encode object data.
		/// </summary>
		/// <param name="path">Path to place the file into.</param>
		/// <param name="filePath">File path.</param>
		/// <param name="data">Data to encode.</param>
		/// <returns>A new file system entry.</returns>
		protected override FileSystemFile EncodeData(FileSystemPath path, string filePath, byte[] data)
		{
			byte[] encryptedData = null;			// Encrypted data.
			FileSystemFile file = null;				// File.

			try
			{
				// Get compressed data.
				encryptedData = TransformData(data, _fileKey, false);

                file = path.Files.Add(filePath, encryptedData, encryptedData.Length, 0, DateTime.Now, true);

				// Add the entry.
				return file;
			}
			catch (Exception ex)
			{
				throw new CannotCreateException("Cannot encode " + filePath + " to the file system.", ex);
			}
		}

		/// <summary>
		/// Function to load an object from the file system.
		/// </summary>
		/// <param name="file">File system entry for the object.</param>
		protected override void Load(FileSystemFile file)
		{
			BinaryReaderEx reader = null;				// Binary data reader.

			try
			{
				// Open the archive for reading.
				if (IsRootInStream)
					reader = new BinaryReaderEx(_fileStream, true);
				else
					reader = new BinaryReaderEx(File.Open(Root, FileMode.Open, FileAccess.Read, FileShare.Read), false);				

				// Move to the data.
				if (!IsRootInStream)
					reader.BaseStream.Position = _fileOffset + file.Offset;
				else
					reader.BaseStream.Position = _fileSystemOffset + _fileOffset + file.Offset;

				file.Data = reader.ReadBytes(file.Size);

				// Close the file to get around concurrency issues.
				if (reader != null)
					reader.Close();
				reader = null;

				if ((file.Data == null) || (file.Data.Length != file.Size))
					throw new Exception("Failure reading file system file.");

				// Fire the event.
				OnDataLoad(this, new FileSystemDataIOEventArgs(file));
			}
			catch (Exception ex)
			{
				throw new CannotLoadException("Cannot load " + file.FullPath + " from the file system.", ex);
			}
			finally
			{
				if (reader != null)
					reader.Close();
				reader = null;
			}
		}

		/// <summary>
		/// Function called when a save operation begins.
		/// </summary>
		/// <param name="filePath">Path to the file system location.</param>
		protected override void SaveInitialize(string filePath)
		{
			// Append the file system extension.
			if ((Path.GetExtension(filePath) == string.Empty) && (!IsRootInStream))
				filePath += ".gorPack";

			// Open the stream for writing.
			if (!IsRootInStream)
				_fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
			else
				_fileStream.Position = _fileSystemOffset;
		}

		/// <summary>
		/// Function called when the save function is complete.
		/// </summary>
		/// <remarks>This function is called at the end of the save function, regardless of whether the save was successful or not.</remarks>
		protected override void SaveFinalize()
		{
			if (IsRootInStream)
			{
				_fileStream.Position = _fileSystemOffset;
				return;
			}

			if (_fileStream != null)
				_fileStream.Dispose();
			_fileStream = null;
		}

		/// <summary>
		/// Function to save the file index.
		/// </summary>
		/// <param name="filePath">Root of the file system on the disk.</param>
		protected override void SaveIndex(string filePath)
		{
			byte[] encrypted = null;		// Encrypted index data.
			BinaryWriterEx writer = null;	// Binary writer.
			
			try
			{
                // Let's encrypt our private authentication data.
                if (AuthenticationData != null)
                    encrypted = TransformData(AuthenticationData.Data, _privKey, false);
                else
                    throw new InvalidAuthenticationDataException(Name);

				writer = new BinaryWriterEx(_fileStream, true);
				writer.Write(FileSystemHeader);
                // Write out encrypted authorization data.
                writer.Write(encrypted.Length);
                writer.Write(encrypted, 0, encrypted.Length);

                // Encrypt and write the key.
                byte[] encryptedKey = TransformData(_fileKey, _privKey, false);
                writer.Write(encryptedKey.Length);
                writer.Write(encryptedKey, 0, encryptedKey.Length);

                // Save the index file.
                encrypted = TransformData(Encoding.UTF8.GetBytes(FileIndexXML.OuterXml), _fileKey, false);
                writer.Write(encrypted.Length);
                writer.Write(encrypted);
			}
			finally			
			{
				if (writer != null)
					writer.Close();
				writer = null;
			}
		}

		/// <summary>
		/// Function to save the file data.
		/// </summary>
		/// <param name="filePath">Root of the file system on the disk.</param>
		/// <param name="file">File to save.</param>
		protected override void SaveFileData(string filePath, FileSystemFile file)
		{
			_fileStream.Write(file.Data, 0, file.Data.Length);
		}

        /// <summary>
        /// Function used to create a user friendly authorization interface.
        /// </summary>
        /// <param name="owner">Form that would potentially own any dialogs we create.</param>
        /// <returns>A code to indicate the status.</returns>
        /// <remarks>You'd typically use this to create a login screen or file browser or whatever to define the authorization for the user.</remarks>
        public override int CreateAuthorization(System.Windows.Forms.Form owner)
        {
            formChangePassword pwdForm = null;      // Password form.

            try
            {
                pwdForm = new formChangePassword();
                pwdForm.AuthorizationData = AuthenticationData;
                if (pwdForm.ShowDialog(owner) == System.Windows.Forms.DialogResult.OK)
                    AuthenticationData = pwdForm.AuthorizationData;
                else
                    return 0;
            }
            catch (Exception ex)
            {
                UI.ErrorBox(owner, "Error changing the password.", ex);
                return 0;
            }

            return 1;
        }

        /// <summary>
        /// Function user to get the authorization from the user.
        /// </summary>
        /// <param name="owner">Form that would potentially own any dialogs we create.</param>
        /// <returns>A code to indicate the status.</returns>
        /// <remarks>You'd typically use this to create a login screen or file browser or whatever to define the authorization for the user.</remarks>
        public override int GetAuthorization(System.Windows.Forms.Form owner)
        {
            formGetPassword pwdForm = null;     // Password form.

            try
            {
                pwdForm = new formGetPassword();
                if (pwdForm.ShowDialog(owner) == System.Windows.Forms.DialogResult.OK)
                    AuthenticationData = pwdForm.AuthorizationData;
                else
                {
                    AuthenticationData = null;
                    return 0;
                }
            }
            catch (Exception ex)
            {
                UI.ErrorBox(owner, "Error getting the authorization from the user.", ex);
                return 0;
            }
            finally
            {
                if (pwdForm != null)
                    pwdForm.Dispose();
                pwdForm = null;
            }

            return 1;
        }

		/// <summary>
		/// Function to save the file system to a stream.
		/// </summary>
		/// <param name="fileSystemStream">Stream to save into.</param>
		public override void Save(Stream fileSystemStream)
		{
			if (fileSystemStream == null)
				throw new ArgumentNullException("fileSystemStream");

			_streamIsRoot = true;
			_fileSystemOffset = fileSystemStream.Position;
			_fileStream = fileSystemStream;

			base.Save(string.Empty);
		}

		/// <summary>
		/// Function to save the file system.
		/// </summary>
		/// <param name="filePath">Path to save the file system into.</param>
		public override void Save(string filePath)
		{
			_streamIsRoot = false;
			_fileStream = null;

			base.Save(filePath);
		}
		#endregion

        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of this file system.</param>
        /// <param name="provider">File system provider.</param>
		internal Gorgon3DESFileSystem(string name, FileSystemProvider provider)
            : base(name, provider)
        {
            KeySizes[] keySizes;        // Key sizes.

            // Let's encrypt the password to the file system.
            string privKey = "This is a private key, you should use your own - do not use this one.";
            string ivKey = "This will be encrypted too.";

            _des = new TripleDESCryptoServiceProvider();
            keySizes = _des.LegalKeySizes;
            _maxKeyBytes = keySizes[0].MaxSize / 8;

            // Create a new private key and IV.
            _privKey = new byte[_maxKeyBytes];
            _IV = new byte[8];

            for (int i = 0; i < _privKey.Length; i++)
                _privKey[i] = Convert.ToByte(privKey[i]);

            for (int i = 0; i < _IV.Length; i++)
                _IV[i] = Convert.ToByte(ivKey[i]);
        }
        #endregion
    }
}
