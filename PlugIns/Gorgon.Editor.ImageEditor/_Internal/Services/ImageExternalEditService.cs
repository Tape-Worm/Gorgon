#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: February 27, 2019 1:06:53 PM
// 
#endregion

using System;
using System.Diagnostics;
using System.IO;
using Gorgon.Diagnostics;
using Gorgon.Editor.ImageEditor.Native;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// Functionality to edit image data using an external program.
    /// </summary>
    internal class ImageExternalEditService
        : IImageExternalEditService
    {
        #region Classes.
        /// <summary>
        /// Data manipulated by the file system watcher should the file be altered in any way.
        /// </summary>
        private class WatchValues
        {
            /// <summary>
            /// The path to the file being edited.
            /// </summary>
            public string FilePath;
            /// <summary>
            /// A flag to indicate whether anything has changed on the file.
            /// </summary>
            public bool HasChanges;
        }
        #endregion

        #region Variables.
        // The log used for debug logging messages.
        private readonly IGorgonLog _log;
        #endregion

        #region Properties.

        #endregion

        #region Methods.
        /// <summary>
        /// Function to locate the application that can edit the specified file.
        /// </summary>
        /// <param name="workingFile">The file to edit.</param>
        /// <returns>The path to the executable.</returns>
        private string GetExecutable(IGorgonVirtualFile workingFile)
        {
            _log.Print($"Retrieving associated executable for files of type {workingFile.Extension}.", LoggingLevel.Verbose);

            string exePath = Win32API.GetAssociatedExecutable(workingFile.PhysicalFile.FullPath);

            if (string.IsNullOrWhiteSpace(exePath))
            {
                _log.Print($"No executable found for files of type {workingFile.Extension}.", LoggingLevel.Verbose);
                return null;
            }

            _log.Print($"Found executable path {exePath}.", LoggingLevel.Verbose);

            return exePath;
        }

        /// <summary>
        /// Function to create a file system watcher for the specified file.
        /// </summary>
        /// <param name="values">Values used to determine if there are any changes to the file.</param>
        /// <returns>The file system watcher used to determine if the file changes or not.</returns>
        private FileSystemWatcher CreateWatcher(WatchValues values)
        {
            FileSystemWatcher result;

            void HandleFileSystemChange(object sender, FileSystemEventArgs e)
            {
                if (!string.Equals(values.FilePath, e.FullPath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                values.HasChanges = true;
            }

            result = new FileSystemWatcher
            {
                NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.Security,
                Path = Path.GetDirectoryName(values.FilePath).FormatDirectory(Path.DirectorySeparatorChar),                
                Filter = "*" + Path.GetExtension(values.FilePath),
                IncludeSubdirectories = false,
                EnableRaisingEvents = true
            };

            result.Created += HandleFileSystemChange;
            result.Changed += HandleFileSystemChange;

            return result;
        }

        /// <summary>Function to edit the image data in an external application.</summary>
        /// <param name="workingFile">The file containing the image data to edit.</param>
        /// <returns>
        ///   <b>true</b> if the image data was changed, <b>false</b> if not.</returns>
        /// <exception cref="IOException">Thrown if no associated executable was found for the file type.</exception>
        public bool EditImage(IGorgonVirtualFile workingFile)
        {
            string exePath = GetExecutable(workingFile);

            if ((string.IsNullOrWhiteSpace(exePath)) || (!File.Exists(exePath)))
            {
                throw new IOException(string.Format(Resources.GORIMG_ERR_NO_EXTERNAL_EDITOR, workingFile.Name));
            }

            GorgonApplication.MainForm.Visible = false;

            var startInfo = new ProcessStartInfo(exePath)
            {
                ErrorDialog = true,
                ErrorDialogParentHandle = GorgonApplication.MainForm.Handle,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Maximized,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(workingFile.PhysicalFile.FullPath).FormatDirectory(Path.DirectorySeparatorChar),
                Arguments = $"\"{workingFile.PhysicalFile.FullPath}\""
            };

            Process externalProcess = null;

            var values = new WatchValues()
            {
                FilePath = workingFile.PhysicalFile.FullPath,
                HasChanges = false
            };

            FileSystemWatcher watcher = CreateWatcher(values);

            try
            {
                _log.Print($"Launching {startInfo.FileName} to edit {workingFile.FullPath}", LoggingLevel.Verbose);

                externalProcess = Process.Start(startInfo);
                externalProcess.WaitForExit();

                if (!values.HasChanges)
                {
                    _log.Print($"{startInfo.FileName} closed. No changes detected on {workingFile.FullPath}.", LoggingLevel.Verbose);
                    return false;
                }

                _log.Print($"{startInfo.FileName} closed. Changes detected on {workingFile.FullPath}.", LoggingLevel.Verbose);
            }
            finally
            {
                watcher?.Dispose();
                externalProcess?.Dispose();
                GorgonApplication.MainForm.Visible = true;
            }            

            return true;
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="ImageExternalEditService"/> class.</summary>
        /// <param name="log">The log used for debug messages.</param>
        public ImageExternalEditService(IGorgonLog log) => _log = log ?? GorgonLog.NullLog;
        #endregion
    }
}
