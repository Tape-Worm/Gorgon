#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Tuesday, March 12, 2013 9:23:18 PM
// 
#endregion

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Gorgon.Core;
using Gorgon.Editor.GorPackWriterPlugIn.Properties;
using Gorgon.IO;

namespace Gorgon.Editor.GorPackWriterPlugIn
{
    /// <summary>
    /// Font editor plug-in interface.
    /// </summary>
    public class GorgonGorPackWriterPlugIn
        : FileWriterPlugIn
	{
		#region Variables.
        private static readonly object _syncLock = new object();
        private XDocument _fat;
        private string _tempPath = string.Empty;
		private int _compressionRatio;
		private CancellationToken _token = default(CancellationToken);
		private byte[] _writeBuffer;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the plug-in supports writing compressed files.
		/// </summary>
		public override bool SupportsCompression => true;

	    #endregion

		#region Methods.
        /// <summary>
        /// Function to create a new path node.
        /// </summary>
        /// <param name="directory">Directory to retrieve information from.</param>
        /// <returns>A new node element with the directory information.</returns>
        private static XElement CreatePathNode(GorgonFileSystemDirectory directory)
        {
            return new XElement("Path", 
                                new XAttribute("Name", (directory.Name == "/") ? @"\" : directory.Name), 
                                new XAttribute("FullPath", directory.FullPath.FormatDirectory(Path.DirectorySeparatorChar)));
        }

        /// <summary>
        /// Function to create a new file node.
        /// </summary>
        /// <param name="file">File to retrieve information from.</param>
        /// <param name="position">Position of the file in the packed data.</param>
        /// <param name="size">Size of the compressed file in the packed data.</param>
		/// <param name="compressedSize">Compressed size of the file.</param>
        /// <returns>A new node element with the file information.</returns>
        private static XElement CreateFileNode(GorgonFileSystemFileEntry file, long position, long size, long compressedSize)
        {
            return new XElement("File",
                                new XElement("Filename", file.BaseFileName),
                                new XElement("Extension", file.Extension),
                                new XElement("Offset", position),
                                new XElement("Size", size),
                                new XElement("CompressedSize", compressedSize),
                                new XElement("FileDate", file.CreateDate.ToString(CultureInfo.InvariantCulture.DateTimeFormat)),
                                new XElement("Encrypted", false),
                                new XElement("Comment", "Gorgon.Editor"));
        }

		/// <summary>
		/// Function to compress data.
		/// </summary>
		/// <param name="inStream">Input stream.</param>
		/// <param name="outStream">Output stream.</param>
		private void CompressData(Stream inStream, Stream outStream)
		{
			if (_writeBuffer == null)
			{
				_writeBuffer = new byte[81920];
			}

			Debug.Assert(outStream != null, "outStream != null");

			using (var bzStream = new BZip2OutputStream(outStream, _compressionRatio))
			{
				long streamSize = inStream.Length;
				bzStream.IsStreamOwner = false;

				while (streamSize > 0)
				{
					if (_token.IsCancellationRequested)
					{
						return;
					}

					int readSize = inStream.Read(_writeBuffer, 0, 81920);

					if (_token.IsCancellationRequested)
					{
						return;
					}

					if (readSize > 0)
					{
						if (_token.IsCancellationRequested)
						{
							return;
						}

						bzStream.Write(_writeBuffer, 0, readSize);

						if (_token.IsCancellationRequested)
						{
							return;
						}
					}

					streamSize -= readSize;
				}
			}			
		}

        /// <summary>
        /// Function to compress the files in a given directory.
        /// </summary>
        /// <param name="output">The output data stream.</param>
        /// <param name="directory">Directory containing the files to compress.</param>
        /// <param name="rootNode">Root node for the file allocation table.</param>
        private void CompressFiles(Stream output, GorgonFileSystemDirectory directory, XElement rootNode)
        {
	        // If we have sub directories, then create those entries.
	        foreach (var subDir in directory.Directories)
	        {
		        var dirNode = CreatePathNode(subDir);

		        if (_token.IsCancellationRequested)
		        {
			        return;
		        }

		        CompressFiles(output, subDir, dirNode);

		        if (_token.IsCancellationRequested)
		        {
			        return;
		        }

		        // Attach to the root.
		        rootNode.Add(dirNode);
	        }

	        // Compress the files.
	        foreach (var fileEntry in directory.Files)
	        {
		        // Load the file into a buffer for compression.
		        long fileStart = output.Position;
		        long fileSize;
		        long compressedSize = 0;


		        UpdateStatus(string.Format(Resources.GORPKW_SAVING_MSG, fileEntry.FullPath.Ellipses(45, true)), 0);

		        if (_token.IsCancellationRequested)
		        {
			        return;
		        }

		        using (var sourceData = ScratchFileSystem.OpenStream(fileEntry, false))
		        {
			        // Load the data into memory.
			        using (var fileData = new GorgonDataStream((int)sourceData.Length))
			        {
				        if (_token.IsCancellationRequested)
				        {
					        return;
				        }

				        sourceData.CopyTo(fileData);

				        if (_token.IsCancellationRequested)
				        {
					        return;
				        }

				        fileSize = fileData.Length;
				        fileData.Position = 0;

				        using (var compressedData = new MemoryStream())
				        {
					        if (_token.IsCancellationRequested)
					        {
						        return;
					        }

					        CompressData(fileData, compressedData);

					        if (_token.IsCancellationRequested)
					        {
						        return;
					        }

					        compressedData.Position = 0;							
					        fileData.Position = 0;

					        // Write the compressed data out to our blob file.
					        if (compressedData.Length < fileSize)
					        {
						        if (_token.IsCancellationRequested)
						        {
							        return;
						        }

						        compressedData.CopyTo(output);

						        if (_token.IsCancellationRequested)
						        {
							        return;
						        }

						        compressedSize = compressedData.Length;
					        }
					        else
					        {
						        if (_token.IsCancellationRequested)
						        {
							        return;
						        }
								
						        // We didn't compress anything, so just dump the file.
						        fileData.CopyTo(output);

						        if (_token.IsCancellationRequested)
						        {
							        return;
						        }
					        }							
				        }
			        }
		        }

		        // Add to our directory.
		        rootNode.Add(CreateFileNode(fileEntry, fileStart, fileSize, compressedSize));
	        }
        }

	    /// <summary>
        /// Function to build the file system.
        /// </summary>
        public void BuildFileSystem()
        {            
            // Create our root directory.
            var root = CreatePathNode(ScratchFileSystem.RootDirectory);

            // Initialize our file allocation table.
            _fat = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), 
                                 new XElement("FileSystem",
                                     new XElement("Header", "GORFS1.0"),
                                     root));

			if (_token.IsCancellationRequested)
			{
				return;
			}

            // Create a temporary blob file to hold our compressed data.
            using (var outputFile = File.Open(_tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                // Get all files in the scratch file system.
                CompressFiles(outputFile, ScratchFileSystem.RootDirectory, root);
            }
        }

        /// <summary>
        /// Function to determine if a plug-in can be used.
        /// </summary>
        /// <returns>
        /// A string containing a list of reasons why the plug-in is not valid for use, or an empty string if the control is not valid for use.
        /// </returns>        
        protected override string ValidatePlugIn()
        {
            return null;
        }

		/// <summary>
		/// Function to write the file to the specified path.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		/// <param name="token">Token used to cancel the task.</param>
        protected override void WriteFile(string path, CancellationToken token)
        {
            // Don't allow other threads in here.
            lock (_syncLock)
            {
				_token = token;

				_tempPath = ScratchFileSystem.WriteLocation + Path.GetFileName(Path.GetTempFileName());

				// Calculate compression ratio.  9 is the max compression level for bzip 2.
				_compressionRatio = (int)System.Math.Round(Compression * 9.0f, 0, MidpointRounding.AwayFromZero);

                try
                {
					// Build file system.
                    BuildFileSystem();

					if (_token.IsCancellationRequested)
					{
						return;
					}

					// Turn off cancellation at this point.
					CanCancel(false);

					if (_token.IsCancellationRequested)
					{
						return;
					}

                    // Write out our file.
                    using (var outputFile = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {                        
                        // Combine the file system parts into a single packed file.
                        using (var writer = new GorgonBinaryWriter(outputFile, true))
                        {
                            // Write the file header.
                            writer.Write("GORPACK1.SharpZip.BZ2");
                                                        
                            // Copy FAT.
							using (var fatData = new GorgonDataStream(Encoding.UTF8.GetBytes(_fat.ToStringWithDeclaration())))
                            {
                                using (var compressData = new MemoryStream())
                                {
									CompressData(fatData, compressData);
                                    compressData.Position = 0;

                                    writer.Write((int)compressData.Length);
                                    compressData.CopyTo(outputFile);
                                }
                            }
                        }

                        // Copy the file data.
                        using (var fileData = File.Open(_tempPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            fileData.CopyTo(outputFile);
                        }
                    }
                }
                finally
                {
                    // Destroy the temp data file.
                    File.Delete(_tempPath);
                }
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonGorPackWriterPlugIn"/> class.
        /// </summary>
        public GorgonGorPackWriterPlugIn()
            : base(Resources.GORPKW_DESC)
        {
			FileExtensions.Add(new GorgonFileExtension("gorPack", Resources.GORPKW_GORPACK_FILE_EXT_DESC));
        }
        #endregion
    }
}
