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
// Created: Sunday, September 22, 2013 7:33:11 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.IO;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Accessibility for the scratch area.
	/// </summary>
	enum ScratchAccessibility
	{
		/// <summary>
		/// Scratch area is accessible and writable.
		/// </summary>
		Accessible = 0,
		/// <summary>
		/// Scratch is pointed at a system area.
		/// </summary>
		SystemArea = 1,
		/// <summary>
		/// Scratch area is read only or there are insufficient permissions to the scratch area.
		/// </summary>
		CannotWrite = 2,
		/// <summary>
		/// When changing the scratch area, this indicates that the user canceled the selection.
		/// </summary>
		Canceled = 3
	}

	/// <summary>
	/// The scratch area interface.
	/// </summary>
	/// <remarks>This interface is used to maintain the scratch area.</remarks>
	static class ScratchArea
    {
        #region Classes.
        /// <summary>
        /// Settings for export.
        /// </summary>
        private class ExportSettings
        {
            #region Properties.
            /// <summary>
            /// Property to set or return the process form to use.
            /// </summary>
            public formProcess ProcessForm
            {
                get;
                set;
            }

            /// <summary>
            /// Property to set or return the total number of files.
            /// </summary>
            public int TotalFiles
            {
                get;
                set;
            }

            /// <summary>
            /// Property to set or return the file counter.
            /// </summary>
            public int FileCount
            {
                get;
                set;
            }

            /// <summary>
            /// Property to set or return the cancellation token.
            /// </summary>
            public CancellationToken CancelToken
            {
                get;
                set;
            }

            /// <summary>
            /// Property to set or return whether the operation is cancelled or not.
            /// </summary>
            public bool IsCancelled
            {
                get
                {
                    return ConflictResult == ConfirmationResult.Cancel || CancelToken.IsCancellationRequested;
                }
            }

            /// <summary>
            /// Property to set or return the current conflict result.
            /// </summary>
            public ConfirmationResult ConflictResult
            {
                get;
                set;
            }
            #endregion
        }
        #endregion

        #region Variables.
        private readonly static string[] _systemDirs;
        private static Guid _scratchID = Guid.NewGuid();
		#endregion

		#region Properties.
        /// <summary>
        /// Property to set or return the method that is called when there's a file conflict (file already exists) when exporting.
        /// </summary>
        /// <remarks>This will return a confirmation result so that a dialog can be used to determine the results of the action.</remarks>
        public static Func<string, int, ConfirmationResult> ExportFileConflictFunction
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the method that is called when the file export is completed or cancelled.
        /// </summary>
        /// <remarks>The first parameter will be TRUE if the export is cancelled, the second is the number of files exported, and the third is the total number of files.</remarks>
        public static Action<bool, int, int> ExportFileCompleteAction
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the function to call when a file that is being created already exists.
        /// </summary>
        /// <remarks>The first parameter is the file name, the 2nd is the file type and the return value is a confirmation dialog result.</remarks>
	    public static Func<string, string, ConfirmationResult> CreateFileConflictFunction
	    {
	        get;
	        set;
	    }

        /// <summary>
		/// Property to return the file system for the scratch files.
		/// </summary>
		public static GorgonFileSystem ScratchFiles
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if the path is a root path or system location.
		/// </summary>
		/// <param name="path">Path to evaluate.</param>
		/// <returns>TRUE if a system location or root directory.</returns>
		private static bool IsSystemLocation(string path)
		{
			var sysRoot = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
			var info = new DirectoryInfo(Path.GetFullPath(path));

			// Don't allow the root of the system drive as a scratch area.
			if ((info.Parent == null) && (string.Equals(sysRoot, info.FullName, StringComparison.OrdinalIgnoreCase)))
			{
				return true;
			}

			// Ensure the system files are not accessible.
			return (_systemDirs.Any(item => string.Equals(path, item, StringComparison.OrdinalIgnoreCase)));
		}

        /// <summary>
        /// Function to export files and directories from the file system.
        /// </summary>
        /// <param name="directory">The directory containing the files to copy.</param>
        /// <param name="destinationPath">The path to export into.</param>
        /// <param name="settings">Settings to apply to the export.</param>
        /// <returns>A result to indicate that the action is cancelled.</returns>
        private static void ExportFilesAndDirectoriesThread(GorgonFileSystemDirectory directory, string destinationPath, ExportSettings settings)
        {
            destinationPath = Path.GetFullPath(destinationPath).FormatDirectory(Path.DirectorySeparatorChar);
            string newDirectory = destinationPath;

            if (settings.IsCancelled)
            {
                return;
            }

            if (!Directory.Exists(newDirectory))
            {
                Directory.CreateDirectory(newDirectory);
            }

            // Copy the files to the directory.
            foreach (var file in directory.Files)
            {
                if (settings.IsCancelled)
                {
                    return;
                }

                if (FileManagement.IsBlocked(file))
                {
                    continue;
                }

                string newFileName = string.Format("{0}{1}", newDirectory, file.Name.FormatFileName());

                if (((settings.ConflictResult & ConfirmationResult.ToAll) != ConfirmationResult.ToAll)
                    && (File.Exists(newFileName)))
                {
                    settings.ConflictResult = ExportFileConflictFunction(newFileName, settings.TotalFiles);

                    if ((settings.ConflictResult & ConfirmationResult.No) == ConfirmationResult.No)
                    {
                        continue;
                    }

                    if (settings.ConflictResult == ConfirmationResult.Cancel)
                    {
                        return;
                    }
                }

                // Update the process dialog.
                settings.ProcessForm.UpdateStatusText(string.Format(Resources.GOREDIT_IMPORT_EXPORT_PROCESS_LABEL,
                                                                    newFileName.Ellipses(45, true)));

                Export(file, newDirectory, true);

                settings.FileCount++;

                settings.ProcessForm.SetProgress((int)((settings.FileCount / (decimal)settings.TotalFiles) * 100M));
            }

            // Copy any sub directories.
            foreach (var subDirectory in directory.Directories)
            {
                if (settings.IsCancelled)
                {
                    return;
                }

                newDirectory = string.Format("{0}{1}", destinationPath, subDirectory.Name);

                // Recursively copy everything.
                ExportFilesAndDirectoriesThread(subDirectory, newDirectory, settings);
            }
        }

        /// <summary>
        /// Function to create a new file in the scratch area.
        /// </summary>
        /// <param name="fileName">The name of the file being created.</param>
        /// <param name="fileType">The type of file.</param>
        /// <param name="directory">The directory that will hold the file.</param>
        /// <returns>The file system entry for the file.</returns>
	    public static GorgonFileSystemFileEntry CreateFile(string fileName, string fileType, GorgonFileSystemDirectory directory)
	    {
            string filePath = directory.FullPath + fileName;
            GorgonFileSystemFileEntry file = ScratchFiles.GetFile(filePath);

            if (string.IsNullOrWhiteSpace(fileType))
            {
                fileType = Resources.GOREDIT_FILE_DEFAULT_TYPE;
            }

            Debug.Assert(CreateFileConflictFunction != null, "No file conflict method assigned in create file.");

            // This file exists, ask the user what they want to do.
            if ((file != null) && (CreateFileConflictFunction(fileName, fileType) == ConfirmationResult.Yes))
            {
                // Delete the old file.
                ScratchFiles.DeleteFile(file);
            }

            // Write the file as 0 bytes.
            return ScratchFiles.WriteFile(directory.FullPath + fileName, null);
	    }

        /// <summary>
        /// Function to export a directory to an external location.
        /// </summary>
        /// <param name="directory">Directory to export.</param>
        /// <param name="destinationPath">Path to export the files and directories into.</param>
        public static void Export(GorgonFileSystemDirectory directory, string destinationPath)
        {
            Debug.Assert(ExportFileCompleteAction != null, "No file completion action assigned in export.");
            Debug.Assert(ExportFileConflictFunction != null, "No file conflict action assigned in export.");

            // Bring up the progress form.
            using (var processForm = new formProcess(ProcessType.FileExporter))
            {
                using (var tokenSource = new CancellationTokenSource(Int32.MaxValue))
                {
                    var settings = new ExportSettings
                    {
                        CancelToken = tokenSource.Token,
                        ConflictResult = ConfirmationResult.None,
                        FileCount = 0,
                        TotalFiles = ScratchFiles.FindFiles(directory.FullPath, "*", true).Count(item => !FileManagement.IsBlocked(item)),
                        ProcessForm = processForm
                    };

                    processForm.Task = Task.Factory.StartNew(() =>
                    {
                        ExportFilesAndDirectoriesThread(directory,
                                                        destinationPath,
                                                        settings);

                        ExportFileCompleteAction(settings.IsCancelled, settings.FileCount, settings.TotalFiles);
                    },
                    settings.CancelToken);

                    if (processForm.ShowDialog() == DialogResult.Cancel)
                    {
                        tokenSource.Cancel();
                    }
                }
            }
        }

        /// <summary>
        /// Function to export a file to an external location
        /// </summary>
        /// <param name="file">File to export.</param>
        /// <param name="destinationPath">Path to export the file into.</param>
        /// <param name="overwriteIfExists">TRUE to overwrite the file if it already exists, FALSE to prompt.</param>
        public static void Export(GorgonFileSystemFileEntry file, string destinationPath, bool overwriteIfExists)
        {
            Debug.Assert(ExportFileConflictFunction != null, "No file conflict action assigned in single file Export");

            if ((file == null)
                || (string.IsNullOrWhiteSpace(destinationPath)))
            {
                return;
            }

            // Do not export our blocked files list.
            if (FileManagement.IsBlocked(file))
            {
                return;
            }

            destinationPath = Path.GetFullPath(destinationPath);
            string directory = destinationPath.FormatDirectory(Path.DirectorySeparatorChar);
            string fileName = file.Name.FormatFileName();

            if ((string.IsNullOrWhiteSpace(fileName))
                || (string.IsNullOrWhiteSpace(directory)))
            {
                return;
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string newPath = string.Format("{0}{1}", directory, fileName);

            if ((!overwriteIfExists) && (File.Exists(newPath)))
            {
                ExportFileConflictFunction(newPath, 1);
            }

            // Open the destination file for writing.
            using (var outStream = new FileStream(string.Format("{0}{1}", directory, fileName),
                                            FileMode.Create,
                                            FileAccess.Write,
                                            FileShare.None))
            {
                // Copy the file data.
                using (var inStream = file.OpenStream(false))
                {
                    inStream.CopyTo(outStream);
                }
            }
        }

        /// <summary>
		/// Function to remove all old scratch area directories from the scratch path.
		/// </summary>
		public static void CleanOldScratchAreas()
		{
			if (string.IsNullOrWhiteSpace(Program.Settings.ScratchPath))
			{
				return;
			}

			var scratchInfo = new DirectoryInfo(Program.Settings.ScratchPath);

			if (!scratchInfo.Exists)
			{
				return;
			}

			// Get all the directories in the scratch area.
			var directories = scratchInfo.GetDirectories("Gorgon.Editor.*", SearchOption.TopDirectoryOnly)
				.Where(item => (((item.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
					&& ((item.Attributes & FileAttributes.NotContentIndexed) == FileAttributes.NotContentIndexed)));

			foreach (var directory in directories)
			{
				try
				{
					directory.Delete(true);
				}
				catch (Exception ex)
				{
					// If we can't delete, then something else is amiss and we'll have to try again later.
					// So just eat the exception and move on to the next directory.
					GorgonException.Catch(ex);
				}
			}
		}

		/// <summary>
		/// Function to destroy the current scratch area.
		/// </summary>
		/// <returns>TRUE if the clean up operation was successful, FALSE if not.</returns>
		public static bool DestroyScratchArea()
		{
			if (Program.Settings == null)
			{
				throw new ApplicationException(Resources.GOREDIT_NO_APP_SETTINGS);
			}

			try
			{
				var scratchLocation = new StringBuilder();

				scratchLocation.Append(Program.Settings.ScratchPath);
				scratchLocation.Append(_scratchID.ToString("N").FormatDirectory(Path.DirectorySeparatorChar));

				// Use the write location of the currently mounted file system if possible.
				if ((ScratchFiles != null) && (!string.IsNullOrWhiteSpace(ScratchFiles.WriteLocation)))
				{
					ScratchFiles.Clear();

					scratchLocation.Length = 0;
					scratchLocation.Append(ScratchFiles.WriteLocation);
				}

				string scratchPath = scratchLocation.ToString();

				// The scratch directory is gone, nothing to clean up.
				if (!Directory.Exists(scratchPath))
				{
					return true;
				}

				Program.LogFile.Print("Cleaning up scratch area at \"{0}\".", LoggingLevel.Simple, scratchPath);

				var directory = new DirectoryInfo(scratchPath);

				// Wipe out everything in this directory and the directory proper.				
				directory.Delete(true);
			}
			catch (Exception ex)
			{
				GorgonException.Log = Program.LogFile;
				GorgonException.Catch(ex);
				GorgonException.Log = Gorgon.Log;

				return false;
			}

			return true;
		}

		/// <summary>
		/// Function to allow the user to select a new scratch file location.
		/// </summary>
		/// <returns>A code indicating the accessibility to the scratch area on the drive.</returns>
		public static ScratchAccessibility SetScratchLocation()
		{
			FolderBrowserDialog dialog = null;

			try
			{
				dialog = new FolderBrowserDialog
				{
					Description = Resources.GOREDIT_SCRATCH_PATH_DLG_DESC
				};

				if (Directory.Exists(Program.Settings.ScratchPath))
				{
					dialog.SelectedPath = Program.Settings.ScratchPath;
				}
				dialog.ShowNewFolderButton = true;

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return ScratchAccessibility.Canceled;
				}

				// We chose poorly.
				string selectedPath = dialog.SelectedPath.FormatDirectory(Path.DirectorySeparatorChar);
				if (!IsSystemLocation(selectedPath))
				{
					Program.Settings.ScratchPath = selectedPath;
					return ScratchAccessibility.Accessible;
				}

				GorgonDialogs.ErrorBox(null, Resources.GOREDIT_CANNOT_USESYS_SCRATCH);

				return ScratchAccessibility.SystemArea;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
				return ScratchAccessibility.CannotWrite;
			}
			finally
			{
				if (dialog != null)
				{
					dialog.Dispose();
				}
			}
		}

		/// <summary>
		/// Function to determine if the scratch area is accessible.
		/// </summary>
		/// <param name="scratchPath">Path to the scratch area.</param>
		/// <returns>A code indicating the level of accessibility to the scratch area on the drive.</returns>
		public static ScratchAccessibility CanAccessScratch(string scratchPath)
		{
			string directoryName = Path.GetFullPath(scratchPath).FormatDirectory(Path.DirectorySeparatorChar);

			// Ensure that the device exists or is ready.
			var root = Path.GetPathRoot(directoryName);

			if ((string.IsNullOrWhiteSpace(root)) || !Directory.Exists(root))
			{
				return ScratchAccessibility.CannotWrite;
			}

			try
			{
				// Do not allow access to a system location.
				if (IsSystemLocation(directoryName))
				{
					return ScratchAccessibility.SystemArea;
				}

				var directoryInfo = new DirectoryInfo(directoryName);
				if (!directoryInfo.Exists)
				{
					// If we created the directory, then hide it.
					directoryInfo.Create();
					return ScratchAccessibility.Accessible;
				}

				directoryInfo.GetAccessControl();

				// Ensure that we can actually write to this directory.
				var testWrite = new FileInfo(directoryName + "TestWrite.tst");
				using (Stream stream = testWrite.OpenWrite())
				{
					stream.WriteByte(127);
				}
				testWrite.Delete();
			}
			catch (Exception ex)
			{
				GorgonException.Log = Program.LogFile;
				GorgonException.Catch(ex);
				GorgonException.Log = Gorgon.Log;
				return ScratchAccessibility.CannotWrite;
			}

			return ScratchAccessibility.Accessible;
		}

		/// <summary>
		/// Function to intialize the scratch area.
		/// </summary>
		public static void InitializeScratch()
		{
			_scratchID = Guid.NewGuid();
			ScratchFiles.Clear();
			ScratchFiles.WriteLocation = Program.Settings.ScratchPath + ("Gorgon.Editor." + _scratchID.ToString("N")).FormatDirectory(Path.DirectorySeparatorChar);

			// Set the directory to hidden.  We don't want users really messing around in here if we can help it.
			var scratchDir = new DirectoryInfo(ScratchFiles.WriteLocation);
			if (((scratchDir.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
				|| ((scratchDir.Attributes & FileAttributes.NotContentIndexed) != FileAttributes.NotContentIndexed))
			{
				scratchDir.Attributes = FileAttributes.NotContentIndexed | FileAttributes.Hidden;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="ScratchArea"/> class.
		/// </summary>
		static ScratchArea()
		{
			_systemDirs = ((Environment.SpecialFolder[])Enum.GetValues(typeof(Environment.SpecialFolder)))
							.Select(Environment.GetFolderPath)
							.ToArray();
			ScratchFiles = new GorgonFileSystem();
		}
		#endregion
	}
}
