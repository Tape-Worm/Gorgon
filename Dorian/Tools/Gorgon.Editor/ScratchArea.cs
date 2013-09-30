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
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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
		/// Settings for import.
		/// </summary>
		private class ImportSettings
		{
			#region Properties.
			/// <summary>
			/// The process form to use.
			/// </summary>
			public formProcess ProcessForm;

			/// <summary>
			/// Files to copy.
			/// </summary>
			public IList<Tuple<string, string>> Files;
			/// <summary>
			/// Directories to import.
			/// </summary>
			public IList<Tuple<string, string>> Directories;

			/// <summary>
			/// The file counter.
			/// </summary>
			public int FileCounter;

			/// <summary>
			/// The current conflict result.
			/// </summary>
			public ConfirmationResult ConflictResult;

			/// <summary>
			/// The cancellation token.
			/// </summary>
			public CancellationToken CancelToken;

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
			#endregion
		}

        /// <summary>
        /// Settings for export.
        /// </summary>
        private class ExportSettings
        {
            #region Properties.
	        /// <summary>
	        /// The process form to use.
	        /// </summary>
	        public formProcess ProcessForm;

	        /// <summary>
	        /// The total number of files.
	        /// </summary>
	        public int TotalFiles;

	        /// <summary>
	        /// The file counter.
	        /// </summary>
	        public int FileCount;

	        /// <summary>
	        /// The cancellation token.
	        /// </summary>
	        public CancellationToken CancelToken;

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
	        /// The current conflict result.
	        /// </summary>
	        public ConfirmationResult ConflictResult;
	        #endregion
        }
        #endregion

        #region Variables.
        private readonly static string[] _systemDirs;
        private static Guid _scratchID = Guid.NewGuid();
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the method called to determine if a file is able to be imported or not.
		/// </summary>
		public static Func<GorgonFileSystemFileEntry, bool> CanImportFunction
		{
			get;
			set;
		}

        /// <summary>
        /// Property to set or return the method that is called when there's a file conflict (file already exists) when importing/exporting.
        /// </summary>
        /// <remarks>This will return a confirmation result so that a dialog can be used to determine the results of the action.</remarks>
        public static Func<string, int, ConfirmationResult> ImportExportFileConflictFunction
        {
            get;
            set;
        }

		/// <summary>
		/// Property to set or return the method that is called if a file copy operation throws an exception.
		/// </summary>
		public static Action<Exception> ImportExportFileCopyExceptionAction
		{
			get;
			set;
		}

        /// <summary>
        /// Property to set or return the method that is called when the file import/export is completed or cancelled.
        /// </summary>
        /// <remarks>The first parameter will be TRUE if the export is cancelled, the 2nd indicates TRUE for import, FALSE for export, the third is the number of files imported or exported, and the fourth is the total number of files.</remarks>
        public static Action<bool, bool, int, int> ImportExportFileCompleteAction
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
        /// Property to set or return the function to call when a file that is being copied already exists.
        /// </summary>
        /// <remarks>The first parameter is the destination file name, the 2nd is the number of files being copied and the return value is a confirmation dialog result.</remarks>
	    public static Func<string, int, ConfirmationResult> CopyFileConflictFunction
	    {
	        get;
	        set;
	    }

        /// <summary>
        /// Property to set or return the function to call when a directory that is being copied already exists.
        /// </summary>
        /// <remarks>The first parameter is the destination directory name, the 2nd is the number of directories being copied and the return value is a confirmation dialog result.</remarks>
        public static Func<string, int, ConfirmationResult> CopyDirectoryConflictFunction
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
		/// Function to retrieve the directory/file names for import.
		/// </summary>
		/// <param name="parentDirectory">Parent directory for the children.</param>
		/// <param name="destination">The destination directory.</param>
		/// <param name="settings">Settings for the files.</param>
		private static void GetImportNames(DirectoryInfo parentDirectory, string destination, ImportSettings settings)
		{
			var directories = (from directory in parentDirectory.EnumerateDirectories()
							   where (((directory.Attributes & FileAttributes.Encrypted) != FileAttributes.Encrypted)
									  && ((directory.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
									  && ((directory.Attributes & FileAttributes.System) != FileAttributes.System))
							   select
								   new Tuple<DirectoryInfo, string>(directory, (destination + directory.Name).FormatDirectory('/')));

			var files = (from file in parentDirectory.EnumerateFiles()
						 where (((file.Attributes & FileAttributes.Encrypted) != FileAttributes.Encrypted)
								&& ((file.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
								&& ((file.Attributes & FileAttributes.System) != FileAttributes.System))
						 select new Tuple<string, string>(file.FullName, destination + file.Name.FormatFileName()));

			// Get directories.
			foreach (var directory in directories.Distinct())
			{
				if (settings.IsCancelled)
				{
					return;
				}

				GetImportNames(directory.Item1, directory.Item2, settings);

				settings.Directories.Add(new Tuple<string, string>(directory.Item1.FullName, directory.Item2));
			}

			// Get files.
			foreach (var file in files)
			{
				if (settings.IsCancelled)
				{
					return;
				}

				settings.Files.Add(file);
			}
		}

		/// <summary>
		/// Function to retrieve the file information for the import procedure.
		/// </summary>
		/// <param name="importFiles">Files to import.</param>
		/// <param name="destination">Destination directory for the files.</param>
		/// <param name="settings">Settings for the file import.</param>
		private static void GetFileInfoThread(IEnumerable<string> importFiles, GorgonFileSystemDirectory destination, ImportSettings settings)
		{
			var paths = from path in importFiles
						let Info = new DirectoryInfo(path)
						where (((Info.Attributes & FileAttributes.Encrypted) != FileAttributes.Encrypted)
							   && ((Info.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
							   && ((Info.Attributes & FileAttributes.System) != FileAttributes.System))
						select new
						{
							Info,
							DestPath = ((Info.Attributes & FileAttributes.Directory) == FileAttributes.Directory) ? 
										(destination.FullPath + Info.Name).FormatDirectory('/')
										: destination.FullPath.FormatDirectory('/') + Info.Name.FormatFileName()
						};

			// Get information.
			foreach (var file in paths.Where(item => !item.DestPath.StartsWith(ScratchFiles.WriteLocation, StringComparison.OrdinalIgnoreCase)))
			{
				if (settings.IsCancelled)
				{
					return;
				}

				if ((file.Info.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
				{
					settings.Directories.Add(new Tuple<string, string>(file.Info.FullName, file.DestPath.FormatDirectory('/')));
					GetImportNames(file.Info, file.DestPath, settings);
				}
				else
				{
					settings.Files.Add(new Tuple<string, string>(file.Info.FullName, file.DestPath));
				}
			}
		}

		/// <summary>
		/// Function to perform the file import.
		/// </summary>
		/// <param name="settings">Settings for the import.</param>
		private static void ImportFilesThread(ImportSettings settings)
		{
			// Begin copy procedure.
			var result = ConfirmationResult.None;
			decimal totalEntries = settings.Files.Count + settings.Directories.Count;

			// Create the directory layout first.
			foreach (string directory in settings.Directories.Select(item => item.Item2))
			{
			    if (settings.IsCancelled)
			    {
			        return;
			    }

			    settings.ProcessForm.UpdateStatusText(string.Format(Resources.GOREDIT_IMPORT_STATUS_CREATING,
			                                                        directory.Ellipses(45, true)));

			    ScratchFiles.CreateDirectory(directory);

			    settings.FileCounter++;

			    settings.ProcessForm.SetProgress((int)((settings.FileCounter / totalEntries) * 100M));
			}

			// Copy the files in.
		    for (int i = 0; i < settings.Files.Count; i++)
		    {
		        if (settings.IsCancelled)
		        {
		            return;
		        }

                Tuple<string, string> file = settings.Files[i];
		        var progressValue = (int)(((i + settings.Directories.Count + 1) / totalEntries) * 100M);

		        settings.ProcessForm.UpdateStatusText(string.Format(Resources.GOREDIT_IMPORT_STATUS_COPYING,
		                                                            file.Item2.Ellipses(45, true)));

		        try
		        {
		            // Find out if this file already exists.
		            var fileEntry = ScratchFiles.GetFile(file.Item2);

		            if (fileEntry != null)
		            {
		                if ((result & ConfirmationResult.ToAll) != ConfirmationResult.ToAll)
		                {
		                    result = ImportExportFileConflictFunction(fileEntry.FullPath, settings.Files.Count);
		                }

		                // Stop copying.
		                if (result == ConfirmationResult.Cancel)
		                {
		                    return;
		                }

		                // Skip this file.
		                if ((result & ConfirmationResult.No) == ConfirmationResult.No)
		                {
		                    continue;
		                }

		                // Ensure that the file isn't open.
		                if (!CanImportFunction(fileEntry))
		                {
		                    continue;
		                }
		            }

		            // Copy file data.
		            using(var inputStream = File.Open(file.Item1, FileMode.Open, FileAccess.Read, FileShare.Read))
		            {
		                using(var outputStream = ScratchFiles.OpenStream(file.Item2, true))
		                {
		                    inputStream.CopyTo(outputStream);
		                }
		            }

		            // Increment the file counter.
		            settings.FileCounter++;
		        }
		        catch (Exception ex)
		        {
		            ImportExportFileCopyExceptionAction(ex);
		        }
		        finally
		        {
		            settings.ProcessForm.SetProgress(progressValue);
		        }
		    }
		}

		/// <summary>
		/// Function to perform the file copy operation for the import.
		/// </summary>
		/// <param name="settings">Settings for the import.</param>
		/// <returns>The number of files and directories imported.</returns>
		private static int ImportData(ImportSettings settings)
		{
			using (var processForm = new formProcess(ProcessType.FileImporter))
			{
				using (var tokenSource = new CancellationTokenSource(Int32.MaxValue))
				{
					settings.ProcessForm = processForm;
					settings.CancelToken = tokenSource.Token;

					processForm.Task = Task.Factory.StartNew(() =>
					{
						ImportFilesThread(settings);

						ImportExportFileCompleteAction(settings.IsCancelled, true, settings.FileCounter + settings.Directories.Count,
													   settings.Files.Count + settings.Directories.Count);
					},
					settings.CancelToken);
					
					if (processForm.ShowDialog() == DialogResult.Cancel)
					{
						tokenSource.Cancel();
					}
				}
			}

			return settings.Directories.Count + settings.FileCounter;
		}

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
            for (int i = 0; i < directory.Files.Count; i++)
            {
                var file = directory.Files[i];

                if (settings.IsCancelled)
                {
                    return;
                }

                if (FileManagement.IsBlocked(file))
                {
                    continue;
                }

                string newFileName = string.Format("{0}{1}", newDirectory, file.Name.FormatFileName());

                if (File.Exists(newFileName))
                {
                    if ((settings.ConflictResult & ConfirmationResult.ToAll) != ConfirmationResult.ToAll)
                    {
                        settings.ConflictResult = ImportExportFileConflictFunction(newFileName, settings.TotalFiles);
                    }

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

                if (Export(file, newDirectory, true))
                {
                    settings.FileCount++;
                }

                settings.ProcessForm.SetProgress((int)(((i + 1) / (decimal)settings.TotalFiles) * 100M));
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
        /// Function to copy a directory from one location to another.
        /// </summary>
        /// <param name="directory">Directory to copy.</param>
        /// <param name="destination">Destination directory.</param>
        /// <returns>The new file system directory or NULL if the directory was not copied.</returns>
        public static GorgonFileSystemDirectory Copy(GorgonFileSystemDirectory directory, string destination)
        {
            Debug.Assert(CopyDirectoryConflictFunction != null, "Copy directory conflict action is not assigned for copy.");
            Debug.Assert(CopyFileConflictFunction != null, "Copy file conflict action is not assigned for copy.");

            GorgonFileSystemFileEntry result;

            if (directory == null)
            {
                throw new ArgumentNullException("directory");
            }

            if (string.IsNullOrWhiteSpace(destination))
            {
                throw new ArgumentException(Resources.GOREDIT_PARAMETER_MUST_NOT_BE_EMPTY, "destination");
            }

            // Don't copy ourselves,
            string destPath = Path.GetDirectoryName(destination);
            
            if (string.IsNullOrWhiteSpace(destPath))
            {
                destPath = ScratchFiles.RootDirectory.FullPath + directory.Name;
            }
            else
            {
                destPath = destPath.FormatDirectory('/') + directory.Name;
            }
            
            // Get the total number of files and directories that will be copied.
            int directoryCount = directory.GetDirectoryCount();
            int fileCount = directory.GetFileCount();

            // If we have a root directory, always assume yes for the merge.
            var overwriteResult = destPath != "/" ? ConfirmationResult.None : (ConfirmationResult.Yes | ConfirmationResult.ToAll);

            // Enumerate sub directories.
            foreach (var subDirectory in directory.Directories)
            {
                string newDirectoryPath = (destPath + subDirectory.Name).FormatDirectory('/');

                // Determine if the directory already exists.
                GorgonFileSystemDirectory newSubDirectory = ScratchFiles.GetDirectory(newDirectoryPath);

                if (newSubDirectory == null)
                {
                    newSubDirectory = ScratchFiles.CreateDirectory(newDirectoryPath);
                }
                else
                {
                    // TODO: overwrite result is going to be forgotten on subsequent recursion levels.  Employ settings like the async stuff (perhaps make this all async).
                    if ((overwriteResult & ConfirmationResult.ToAll) != ConfirmationResult.ToAll)
                    {
                        overwriteResult = CopyDirectoryConflictFunction(newDirectoryPath, directoryCount);
                    }

                    if ((overwriteResult & ConfirmationResult.No) == ConfirmationResult.No)
                    {
                        int nameCounter = 0;

                        // Rename the directory if we don't want to merge.
                        while (newSubDirectory != null)
                        {
                            destPath = string.Format("{0} ({1})", destPath.Substring(0, destPath.Length - 1), ++nameCounter)
                                      .FormatDirectory('/');

                            newSubDirectory = ScratchFiles.GetDirectory(destPath);
                        }
                    }
                    
                    // Do nothing if we cancel.
                    if (overwriteResult == ConfirmationResult.Cancel)
                    {
                        return null;
                    }
                }

                // Copy the contents of this sub directory.
                Copy(subDirectory, newDirectoryPath);
            }

            // For files, always prompt the user.
            overwriteResult = ConfirmationResult.None;

            // Copy the files.
            foreach (var file in directory.Files)
            {
                string newFilePath = destPath + file.Name;
                GorgonFileSystemFileEntry newFile = ScratchFiles.GetFile(newFilePath);

                // If the new file
                if (newFile != null)
                {
                    if ((overwriteResult & ConfirmationResult.ToAll) != ConfirmationResult.ToAll)
                    {
                        overwriteResult = CopyFileConflictFunction(newFilePath, fileCount);
                    }

                    if (overwriteResult == ConfirmationResult.Cancel)
                    {
                        return null;
                    }
                }
            }

            using (Stream inputStream = file.OpenStream(false))
            {
                string newPath = destination.FullPath + file.Name;

                if ((ScratchFiles.GetFile(newPath) != null) && (forceOverwrite))
                {
                    ConfirmationResult copyResult = CopyFileConflictFunction(newPath, 1);

                    if (copyResult == ConfirmationResult.No)
                    {
                        return null;
                    }

                    ScratchFiles.DeleteFile(newPath);
                }

                // Create the 0 byte file.
                result = ScratchFiles.WriteFile(newPath, null);

                // Dump the source data into it.
                using (Stream outputStream = result.OpenStream(true))
                {
                    inputStream.CopyTo(outputStream);
                }
            }

            return result;
        }

        /// <summary>
        /// Function to copy a file from one location to another.
        /// </summary>
        /// <param name="file">File to copy.</param>
        /// <param name="destination">Destination file name or directory.</param>
        /// <param name="forceOverwrite">TRUE to force an overwrite if the file already exists, FALSE to prompt the user.</param>
        /// <returns>The new file system file or NULL if the file was not copied.</returns>
	    public static GorgonFileSystemFileEntry Copy(GorgonFileSystemFileEntry file, string destination, bool forceOverwrite)
        {
            Debug.Assert(CopyFileConflictFunction != null, "Copy file conflict action is not assigned for copy.");

            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            if (string.IsNullOrWhiteSpace(destination))
            {
                throw new ArgumentException(Resources.GOREDIT_PARAMETER_MUST_NOT_BE_EMPTY, "destination");
            }

            // Do not copy if this is a blocked file.
            if (FileManagement.IsBlocked(file))
            {
                return null;
            }

            string destDirectoryPath = Path.GetDirectoryName(destination);
            string destFileName = Path.GetFileName(destination);

            // If we didn't supply a destination path, then create one.
            if (string.IsNullOrWhiteSpace(destDirectoryPath))
            {
                destDirectoryPath = ScratchFiles.RootDirectory.FullPath;
            }
            else
            {
                destDirectoryPath = destDirectoryPath.FormatDirectory('/');
            }

            if (string.IsNullOrWhiteSpace(destFileName))
            {
                destFileName = file.Name;
            }
            else
            {
                destFileName = destFileName.FormatFileName();
            }

            string newPath = destDirectoryPath + destFileName;
            GorgonFileSystemFileEntry existingFile = ScratchFiles.GetFile(newPath);

            if (existingFile != null)
            {
                if (!forceOverwrite)
                {
                    ConfirmationResult copyResult = CopyFileConflictFunction(newPath, 1);

                    // If we chose not to overwrite it, then adjust the name of the file.
                    if (copyResult == ConfirmationResult.No)
                    {
                        int nameCounter = 0;

                        while (existingFile != null)
                        {
                            newPath = string.Format("{0}{1} ({2}){3}",
                                                    destDirectoryPath,
                                                    Path.GetFileNameWithoutExtension(destFileName),
                                                    ++nameCounter,
                                                    Path.GetExtension(destFileName));

                            existingFile = ScratchFiles.GetFile(newPath);
                        }
                    }

                    if (copyResult == ConfirmationResult.Cancel)
                    {
                        return null;
                    }
                }
            }

            // If we try to copy this file on top of itself, then don't bother.  It'd be redundant.
            if (string.Equals(newPath, file.FullPath, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Create the 0 byte file.
            GorgonFileSystemFileEntry result = ScratchFiles.WriteFile(newPath, null);

            using(Stream inputStream = file.OpenStream(false))
            {
                // Dump the source data into it.
                using(Stream outputStream = result.OpenStream(true))
                {
                    inputStream.CopyTo(outputStream);
                }
            }

            return result;
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
            Debug.Assert(ImportExportFileCompleteAction != null, "No file completion action assigned in export.");
            Debug.Assert(ImportExportFileConflictFunction != null, "No file conflict action assigned in export.");
			Debug.Assert(ImportExportFileCopyExceptionAction != null, "No file exception action assigned in export");

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

                        ImportExportFileCompleteAction(settings.IsCancelled, false, settings.FileCount, settings.TotalFiles);
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
        /// <returns>TRUE if the export was successul, FALSE if not.</returns>
        public static bool Export(GorgonFileSystemFileEntry file, string destinationPath, bool overwriteIfExists)
        {
            Debug.Assert(ImportExportFileConflictFunction != null, "No file conflict action assigned in single file Export");
			Debug.Assert(ImportExportFileCopyExceptionAction != null, "No file exception action assigned in single file Export");

            if ((file == null)
                || (string.IsNullOrWhiteSpace(destinationPath)))
            {
                return false;
            }

            // Do not export our blocked files list.
            if (FileManagement.IsBlocked(file))
            {
                return false;
            }

            destinationPath = Path.GetFullPath(destinationPath);
            string directory = destinationPath.FormatDirectory(Path.DirectorySeparatorChar);
            string fileName = file.Name.FormatFileName();

            if ((string.IsNullOrWhiteSpace(fileName))
                || (string.IsNullOrWhiteSpace(directory)))
            {
                return false;
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string newPath = string.Format("{0}{1}", directory, fileName);

            if ((!overwriteIfExists) && (File.Exists(newPath)))
            {
                ImportExportFileConflictFunction(newPath, 1);
            }

	        try
	        {
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
	        catch (Exception ex)
	        {
		        ImportExportFileCopyExceptionAction(ex);
	            return false;
	        }

            return true;
        }

		/// <summary>
		/// Function to perform an import of files and directories into the file system.
		/// </summary>
		/// <param name="importFiles">Files to import.</param>
		/// <param name="destination">Destination directory for the files/directories.</param>
		/// <returns>The number of files and directories imported.</returns>
		public static int Import(IEnumerable<string> importFiles, GorgonFileSystemDirectory destination)
		{
			ImportSettings settings;

			Debug.Assert(ImportExportFileCompleteAction != null, "No file completion action assigned in import.");
			Debug.Assert(ImportExportFileConflictFunction != null, "No file conflict action assigned in import.");
			Debug.Assert(ImportExportFileCopyExceptionAction != null, "No file exception action assigned in import.");
            Debug.Assert(CanImportFunction != null, "No file validation action assigned in import.");

			// Bring up the progress form.
			using (var processForm = new formProcess(ProcessType.FileInfo))
			{
				using (var tokenSource = new CancellationTokenSource(Int32.MaxValue))
				{
					settings = new ImportSettings
					{
						CancelToken = tokenSource.Token,
						ConflictResult = ConfirmationResult.None,
						FileCounter = 0,
						Files = new List<Tuple<string, string>>(),
						Directories = new List<Tuple<string, string>>(),
						ProcessForm = processForm
					};

					processForm.Task = Task.Factory.StartNew(() => GetFileInfoThread(importFiles, destination, settings),
					settings.CancelToken);

					// ReSharper disable once InvertIf
					if (processForm.ShowDialog() == DialogResult.Cancel)
					{
						tokenSource.Cancel();
						return 0;
					}
				}
			}

			// If we have nothing to import, then we should leave.
			if ((settings.Files.Count != 0) || (settings.Directories.Count != 0))
			{
				// Don't just assume we want this.
				return GorgonDialogs.ConfirmBox(null, string.Format(Resources.GOREDIT_IMPORT_CONFIRM, settings.Files.Count,
				                                                    settings.Directories.Count)) == ConfirmationResult.No
					       ? 0
					       : ImportData(settings);
			}

			GorgonDialogs.InfoBox(null, Resources.GOREDIT_IMPORT_NO_DATA);
			return 0;
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
