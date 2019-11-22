#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Monday, June 27, 2011 9:33:12 AM
// 
#endregion

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.IO.Providers;
using Gorgon.IO.Zip.Properties;
using ICSharpCode.SharpZipLib.Zip;

namespace Gorgon.IO.Zip
{
    /// <summary>
    /// A file system provider for zip files.
    /// </summary>
    internal class ZipProvider
        : GorgonFileSystemProvider
    {
        #region Variables.
        /// <summary>
        /// Header bytes for a zip file.
        /// </summary>
        public static IEnumerable<byte> ZipHeader = new byte[] { 0x50, 0x4B, 0x3, 0x4 };
        #endregion

        #region Methods.
        /// <summary>
        /// Function to enumerate the files and directories from a physical location and map it to a virtual location.
        /// </summary>
        /// <param name="physicalLocation">The physical location containing files and directories to enumerate.</param>
        /// <param name="mountPoint">A <see cref="IGorgonVirtualDirectory"/> that the directories and files from the physical file system will be mounted into.</param>		
        /// <returns>A <see cref="GorgonPhysicalFileSystemData"/> object containing information about the directories and files contained within the physical file system.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalLocation"/>, or the <paramref name="mountPoint"/> parameters are <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalLocation"/> parameter is empty.</exception>
        /// <remarks>
        /// <para>
        /// This will return a <see cref="GorgonPhysicalFileSystemData"/> representing the paths to directories and <see cref="IGorgonPhysicalFileInfo"/> objects under the virtual file system. Each file 
        /// system file and directory is mapped from its <paramref name="physicalLocation"/> on the physical file system to a <paramref name="mountPoint"/> on the virtual file system. For example, if the 
        /// mount point is set to <c>/MyMount/</c>, and the physical location of a file is <c>c:\SourceFileSystem\MyDirectory\MyTextFile.txt</c>, then the returned value should be 
        /// <c>/MyMount/MyDirectory/MyTextFile.txt</c>.
        /// </para>
        /// <para>
        /// Implementors of a <see cref="GorgonFileSystemProvider"/> plug in can override this method to read the list of files from another type of file system, like a Zip file.
        /// </para>
        /// <para>
        /// Implementors of a <see cref="GorgonFileSystemProvider"/> should override this method to read the list of directories and files from another type of file system, like a Zip file. 
        /// The default functionality will only enumerate directories and files from the operating system file system.
        /// </para>
        /// </remarks>
        protected override GorgonPhysicalFileSystemData OnEnumerate(string physicalLocation, IGorgonVirtualDirectory mountPoint)
        {
            var directories = new List<string>();
            var files = new List<IGorgonPhysicalFileInfo>();

            using (var zipStream = new ZipInputStream(File.Open(physicalLocation, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                ZipEntry entry;

                while ((entry = zipStream.GetNextEntry()) != null)
                {
                    if (!entry.IsDirectory)
                    {
                        string directoryName = Path.GetDirectoryName(entry.Name).FormatDirectory('/');

                        directoryName = mountPoint.FullPath + directoryName;

                        if (string.IsNullOrWhiteSpace(directoryName))
                        {
                            directoryName = "/";
                        }


                        if (!directories.Contains(directoryName))
                        {
                            directories.Add(directoryName);
                        }

                        files.Add(new ZipPhysicalFileInfo(entry, physicalLocation, mountPoint));
                    }
                    else
                    {
                        directories.Add((mountPoint.FullPath + entry.Name).FormatDirectory('/'));
                    }
                }
            }

            return new GorgonPhysicalFileSystemData(directories, files);
        }

        /// <summary>Function to return the physical file system path from a virtual file system path.</summary>
        /// <param name="virtualPath">Virtual path to the file/folder.</param>
        /// <param name="mountPoint">The mount point used to map the physical path.</param>
        /// <returns>The physical file system path.</returns>
        protected override string OnGetPhysicalPath(string virtualPath, GorgonFileSystemMountPoint mountPoint) => mountPoint.PhysicalPath;

        /// <summary>
        /// Function to open a stream to a file on the physical file system from the <see cref="IGorgonVirtualFile"/> passed in.
        /// </summary>
        /// <param name="file">The <see cref="IGorgonVirtualFile"/> that will be used to locate the file that will be opened on the physical file system.</param>
        /// <returns>A <see cref="Stream"/> to the file, or <b>null</b> if the file does not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This will take the <see cref="IGorgonVirtualFile"/> and open its corresponding physical file location as a stream for reading. The stream that is returned will be opened, and as such, it is the 
        /// responsibility of the user to close the stream when finished.
        /// </para>
        /// <para>
        /// If the file does not exist in the physical file system, this method should return <b>null</b>.
        /// </para>
        /// <para>
        /// Implementors of a <see cref="GorgonFileSystemProvider"/> plug in can overload this method to return a stream into a file within their specific native provider (e.g. a Zip file provider will 
        /// return a stream into the zip file positioned at the location of the compressed file within the zip file).
        /// </para>
        /// </remarks>
        protected override GorgonFileSystemStream OnOpenFileStream(IGorgonVirtualFile file) => new ZipFileStream(file, File.Open(file.MountPoint.PhysicalPath, FileMode.Open, FileAccess.Read, FileShare.Read));

        /// <summary>
        /// Function to determine if a physical file system can be read by this provider.
        /// </summary>
        /// <param name="physicalPath">Path to the packed file containing the file system.</param>
        /// <returns><b>true</b> if the provider can read the packed file, <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.</exception>
        /// <remarks>
        /// <para>
        /// This will test a physical file system (e.g. a Zip file) to see if the provider can open it or not. If used with a directory on an operating system file system, this method should always return 
        /// <b>false</b>.
        /// </para>
        /// <para>
        /// When used with a <see cref="IGorgonFileSystemProvider"/> that supports a non operating system based physical file system, such as the <see cref="GorgonFileSystemRamDiskProvider"/>, then this 
        /// method should compare the <paramref name="physicalPath"/> with its <see cref="IGorgonFileSystemProvider.Prefix"/> to ensure that the <see cref="IGorgonFileSystem"/> requesting the provider is using the correct provider.
        /// </para>
        /// <para>
        /// Implementors of a <see cref="GorgonFileSystemProvider"/> should override this method to determine if a packed file can be read by reading the header of the file specified in <paramref name="physicalPath"/>.
        /// </para>
        /// </remarks>
        protected override bool OnCanReadFile(string physicalPath)
        {
            byte[] headerBytes = new byte[4];

            using (FileStream stream = File.Open(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length <= headerBytes.Length)
                {
                    return false;
                }

                stream.Read(headerBytes, 0, headerBytes.Length);
            }

            return headerBytes.SequenceEqual(ZipHeader);
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="ZipProvider"/> class.
        /// </summary>
        public ZipProvider()
            : base(Resources.GORFS_ZIP_DESC) => PreferredExtensions = new GorgonFileExtensionCollection
                                  {
                                      new GorgonFileExtension("Zip", Resources.GORFS_ZIP_FILE_DESC)
                                  };
        #endregion
    }
}
