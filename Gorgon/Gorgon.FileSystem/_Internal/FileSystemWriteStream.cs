#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Thursday, September 24, 2015 8:33:14 PM
// 
#endregion

using System.IO;
using Gorgon.IO.Providers;

namespace Gorgon.IO
{
    /// <summary>
    /// A file stream writer that will update the virtual file information when it closes.
    /// </summary>
    internal class FileSystemWriteStream
        : FileStream
    {
        #region Variables.
        // The virtual file entry to update when the stream is closed.
        private VirtualFile _virtualFile;
        // The mount point for the writable area.
        private readonly GorgonFileSystemMountPoint _mountPoint;
        #endregion

        #region Methods.
        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.FileStream"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
        protected override void Dispose(bool disposing)
        {
            if ((disposing) && (_virtualFile != null) && (CanWrite))
            {
                var info = new FileInfo(_virtualFile.PhysicalFile.FullPath);
                _virtualFile.PhysicalFile = new PhysicalFileInfo(info, _virtualFile.FullPath);

                if (_virtualFile.MountPoint != _mountPoint)
                {
                    _virtualFile.MountPoint = _mountPoint;
                }
            }

            _virtualFile = null;

            base.Dispose(disposing);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemWriteStream"/> class.
        /// </summary>
        /// <param name="mountPoint">The mount point where the writable file is stored.</param>
        /// <param name="writePath">The path to the writable file.</param>
        /// <param name="file">The virtual file being written.</param>
        /// <param name="fileMode">The file mode to use to open the file.</param>
        public FileSystemWriteStream(GorgonFileSystemMountPoint mountPoint, string writePath, VirtualFile file, FileMode fileMode)
            : base(writePath,
                   fileMode,
                   fileMode == FileMode.Open ? FileAccess.Read : fileMode == FileMode.OpenOrCreate ? FileAccess.ReadWrite : FileAccess.Write,
                   fileMode == FileMode.Open ? FileShare.Read : FileShare.None)
        {
            _mountPoint = mountPoint;
            _virtualFile = file;
        }
        #endregion
    }
}
