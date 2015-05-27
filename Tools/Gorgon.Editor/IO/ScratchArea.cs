#region MIT.
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
// Created: Wednesday, March 11, 2015 12:28:51 AM
// 
#endregion

using System;
using System.IO;
using System.Linq;
using Gorgon.Diagnostics;
using Gorgon.Editor.Properties;
using Gorgon.IO;

namespace Gorgon.Editor
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

#warning Add event to notify when the scratch area changes so that items like the file system service can mark the current file as changed.
	/// <summary>
	/// Used to control the scratch file area for the application.
	/// </summary>
	/// <remarks>
	/// This interface performs file manipulation for the application.  When the editor is run, a new scratch 
	/// area is created on the local disk drive to allow for manipulation of content.  The editor settings 
	/// will contain the path to the scratch area on the drive.
	/// <para>
	/// Files/folders added to the editor will be placed in this scratch area to allow for modification as 
	/// most (if not all) of the pack file provider types that Gorgon comes with are read-only.  
	/// </para>
	/// <para>
	/// Upon saving, the files are gathered up from the scratch area, and placed within the packed file being 
	/// saved by the editor (depending on the current writer plug-in).
	/// </para>
	/// <para>
	/// When a packed file is being loaded, its contents are copied into the scratch area to allow for modification. 
	/// Again, this is because most of the file system providers for Gorgon are read-only.
	/// </para>
	/// <para>
	/// For security, a scratch area cannot be created at the root of the operating system drive, and must be created 
	/// under a sub directory that is writable by the current user.  Also, the scratch area cannot be created 
	/// under "system" folders.  System folders are typically critical operating system folders such as System32, 
	/// Program Files, etc... This is necessary because the contents of the scratch area are very volatile and if 
	/// something were to go wrong, damage to those areas would cause major problems, so this will help mitigate 
	/// potential issues.
	/// </para>
	/// </remarks>
	sealed class ScratchArea
		: IScratchArea
	{
		#region Variables
		// A list of system directories.
		private readonly string[] _systemDirs;
		// The log file for the application.
		private readonly GorgonLogFile _log;
		// The application settings.
		private readonly IEditorSettings _settings;
		// The folder file system that will hold the scratch data.
		private readonly GorgonFileSystem _fileSystem;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the GUID for the scratch area.
		/// </summary>
		public Guid ID
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the scratch file directory path.
		/// </summary>
		public string ScratchDirectory
		{
			get
			{
				return string.IsNullOrWhiteSpace(_settings.ScratchPath) ? string.Empty : _settings.ScratchPath;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if the path is a root path or system location.
		/// </summary>
		/// <param name="path">Path to evaluate.</param>
		/// <returns><c>true</c> if a system location or root directory.</returns>
		private bool IsSystemLocation(string path)
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
		/// Function to clean everything from the scratch directory.
		/// </summary>
		private void CleanAll()
		{
			if (string.IsNullOrWhiteSpace(ScratchDirectory))
			{
				return;
			}

			// If someone is attempting to be clever and trying to mess up our configuration by hand, 
			// this will ensure that we don't try to destroy a system area.
			if (CanAccessScratch(ScratchDirectory) != ScratchAccessibility.Accessible)
			{
				return;
			}

			var scratchInfo = new DirectoryInfo(ScratchDirectory);

			if (!scratchInfo.Exists)
			{
				return;
			}

			// Get all the directories in the scratch area.
			var directories = scratchInfo.GetDirectories("Gorgon.Editor.*", SearchOption.TopDirectoryOnly)
			                             .Where(item => (((item.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
			                                             && ((item.Attributes & FileAttributes.NotContentIndexed) == FileAttributes.NotContentIndexed)))
			                             .ToArray();

			_log.Print("Scratch Area: Cleaning {0} stale scratch areas.", LoggingLevel.Intermediate, directories.Length);

			foreach (var directory in directories)
			{
				try
				{
					_log.Print("Scratch Area: Cleaning stale path {0}", LoggingLevel.Intermediate, directory.FullName);
					directory.Delete(true);
				}
				catch (Exception ex)
				{
					// If we can't delete, then something else is amiss and we'll have to try again later.
					// So just eat the exception and move on to the next directory.
					GorgonApplication.Log.LogException(ex);
				}
			}

		}

		/// <summary>
		/// Function to create the scratch area.
		/// </summary>
		/// <param name="directoryPath">Path to the directory that will contain the scratch area.</param>
		private void CreateScratchArea(string directoryPath)
		{
			// Create a new ID for the scratch area.
			ID = Guid.NewGuid();

			_settings.ScratchPath = directoryPath;

			// Get the full path to the scratch area including its ID.
			directoryPath = directoryPath + ("Gorgon.Editor." + ID.ToString("N")).FormatDirectory(Path.DirectorySeparatorChar);

			// The write path is auto-mounted into the file system, so we don't need to worry about issuing a 
			// separate mount call.
			_fileSystem.WriteLocation = directoryPath;

			// Mark the directory as hidden and non-indexable.
			// This is to keep people from stumbling upon the directory too easily as direct interference 
			// will mess up the editor.
			var directoryInfo = new DirectoryInfo(directoryPath);

			if (((directoryInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
				|| ((directoryInfo.Attributes & FileAttributes.NotContentIndexed) != FileAttributes.NotContentIndexed))
			{
				directoryInfo.Attributes = FileAttributes.NotContentIndexed | FileAttributes.Hidden;
			}

			if (!directoryInfo.Exists)
			{
				directoryInfo.Create();
			}

			_log.Print("Scratch Area: Using '{0}' as the scratch area.", LoggingLevel.Simple, directoryPath);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ScratchArea"/> class.
		/// </summary>
		/// <param name="log">The applicationlog.</param>
		/// <param name="settings">The application settings.</param>
		/// <param name="scratchFileSystem">The proxy object for the file system used to manipulate the scratch files.</param>
		public ScratchArea(GorgonLogFile log, GorgonFileSystem scratchFileSystem, IEditorSettings settings)
		{
			_log = log;
			_settings = settings;
			_fileSystem = scratchFileSystem;

			// Get our list of system directories.
			_systemDirs = ((Environment.SpecialFolder[])Enum.GetValues(typeof(Environment.SpecialFolder)))
				.Select(Environment.GetFolderPath)
				.Where(item => !string.IsNullOrWhiteSpace(item)
				               && !string.Equals(item, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), StringComparison.OrdinalIgnoreCase)
				               && !string.Equals(item, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), StringComparison.OrdinalIgnoreCase)
				               && !string.Equals(item, Environment.GetFolderPath(Environment.SpecialFolder.Personal), StringComparison.OrdinalIgnoreCase))
				.ToArray();
		}
		#endregion

		#region IScratchArea Implementation.
		#region Events.
		/// <summary>
		/// Event fired when a file or directory is updated in the scratch area.
		/// </summary>
		public event EventHandler<ScratchUpdatedEventArgs> ScratchUpdated;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if the scratch area is accessible.
		/// </summary>
		/// <param name="path">Path to the scratch area.</param>
		/// <returns>A value from <see cref="ScratchAccessibility"/> indicating the level of accessibility to the scratch area on the drive.</returns>
		public ScratchAccessibility CanAccessScratch(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return ScratchAccessibility.CannotWrite;
			}

			try
			{
				// If the path we've got is relative, then get the absolute path.
				if (!Path.IsPathRooted(path))
				{
					path = Path.GetFullPath(path).FormatDirectory(Path.DirectorySeparatorChar);
				}

				// Ensure that the device exists or is ready.
				var root = Path.GetPathRoot(path);

				if ((string.IsNullOrWhiteSpace(root))
					|| (!Directory.Exists(root)))
				{
					_log.Print("Scratch Area: Cannot write because the root of the path does not exist.", LoggingLevel.Verbose);
					return ScratchAccessibility.CannotWrite;
				}

				// Do not allow access to a system location.
				if (IsSystemLocation(path))
				{
					_log.Print("Scratch Area: Cannot write because the path '{0}' points to an operating system critical directory.", LoggingLevel.Verbose, path);
					return ScratchAccessibility.SystemArea;
				}

				var directoryInfo = new DirectoryInfo(path);
				if (!directoryInfo.Exists)
				{
					// If we created the directory, then hide it.
					directoryInfo.Create();
					_log.Print("Scratch Area: Directory '{0}' created for the scratch area.", LoggingLevel.Intermediate, path);
					return ScratchAccessibility.Accessible;
				}

				directoryInfo.GetAccessControl();

				// Ensure that we can actually write to this directory.
				var testWrite = new FileInfo(path + "TestWrite.tst");
				using (Stream stream = testWrite.OpenWrite())
				{
					stream.WriteByte(127);
				}
				testWrite.Delete();
			}
			catch (Exception ex)
			{
				GorgonApplication.Log.LogException(ex);
				return ScratchAccessibility.CannotWrite;
			}

			return ScratchAccessibility.Accessible;
		}

		/// <summary>
		/// Function to set the path to the directory that will hold the scratch data.
		/// </summary>
		/// <param name="directoryPath">Path to the scratch directory.</param>
		/// <returns>A value from the <see cref="ScratchAccessibility"/> enumeration.</returns>
		/// <remarks>
		/// This method will create a directory under the path specified in <paramref name="directoryPath"/> that will contain 
		/// the files and folders for the editor content. The directory specified must be writable by the current user and cannot 
		/// be a system directory (e.g. C:\windows\system32\) and cannot be the root of the operating system drive.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryPath"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="directoryPath"/> parameter is empty.</exception>
		public ScratchAccessibility SetScratchDirectory(string directoryPath)
		{
			if (directoryPath == null)
			{
				throw new ArgumentNullException("directoryPath");
			}

			if (string.IsNullOrWhiteSpace(directoryPath))
			{
				throw new ArgumentException(Resources.GOREDIT_ERR_PARAMETER_MUST_NOT_BE_EMPTY, "directoryPath");
			}

			try
			{
				// GetFullPath will exception if we don't have access to the directory via security permissions.
				// So we can just catch the exception and let the application know that we cannot write to the 
				// area.
				directoryPath = Path.GetFullPath(directoryPath).FormatDirectory(Path.DirectorySeparatorChar);
			}
			catch (Exception ex)
			{
				GorgonApplication.Log.LogException(ex);
				return ScratchAccessibility.CannotWrite;
			}

			// Determine our access to the path provided.
			ScratchAccessibility result = CanAccessScratch(directoryPath);

			if (result != ScratchAccessibility.Accessible)
			{
				return result;
			}

			// Attempt to clean up the previous directory (if one was assigned).
			if (!string.IsNullOrWhiteSpace(_fileSystem.WriteLocation))
			{
				CleanUp();
			}

			CreateScratchArea(directoryPath);

			return result;
		}

		/// <summary>
		/// Function to clean up the scratch area data.
		/// </summary>
		/// <param name="cleanEverything">[Optional] <c>true</c> to remove this scratch area and stale scratch area IDs from the folder, <c>false</c> to clean up the current scratch area only.</param>
		/// <returns>
		/// <c>true</c> if the call was successful and the data deleted.  <c>false</c> if not.
		/// </returns>
		/// <remarks>
		/// This will delete the files and folders under the scratch area path, and will delete the scratch area folder (<see cref="ScratchDirectory" />\<see cref="ID" />).
		/// </remarks>
		public bool CleanUp(bool cleanEverything = false)
		{
			try
			{
				if (cleanEverything)
				{
					CleanAll();
					return true;
				}

				if ((string.IsNullOrWhiteSpace(ScratchDirectory))
					|| (string.IsNullOrWhiteSpace(_fileSystem.WriteLocation))
					|| (CanAccessScratch(ScratchDirectory) != ScratchAccessibility.Accessible))
				{
					_fileSystem.Clear();
					return true;
				}

				var directory = new DirectoryInfo(_fileSystem.WriteLocation);

				// We're already gone, so leave.
				if (!directory.Exists)
				{
					_fileSystem.Clear();
					return true;
				}

				_log.Print("Scratch Area: Cleaning up '{0}'...", LoggingLevel.Verbose, directory.FullName);

				// Blow it away.
				directory.Delete(true);

				// Disable the writing for the file system.
				_fileSystem.Clear();

				return true;
			}
			catch (Exception ex)
			{
				GorgonApplication.Log.LogException(ex);
				return false;
			}
		}

		/// <summary>
		/// Function to copy the packed file system to the scratch area.
		/// </summary>
		/// <param name="fileSystem">File system to copy.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileSystem"/> parameter is NULL (Nothing in VB.Net).</exception>
		public void CopyFileSystem(GorgonFileSystem fileSystem)
		{
			if (fileSystem == null)
			{
				throw new ArgumentNullException("fileSystem");
			}

			if (!string.IsNullOrWhiteSpace(_fileSystem.WriteLocation))
			{
				var scratchDir = new DirectoryInfo(_fileSystem.WriteLocation);

				// If for some reason the scratch area is gone, then re-create it.
				if (scratchDir.Exists)
				{
					CleanUp();
				}
			}

			// Refresh the scratch area.
			CreateScratchArea(ScratchDirectory);

			// Get the lists of directories and files from the file system.
			try
			{
				// At this point we should have a clean scratch area, so all files will exist in the packed file.
				// Unpack the file structure so we can work with it.
				var directories = fileSystem.FindDirectories("*", true);
				var files = fileSystem.FindFiles("*", true);

				// Create our directories.
				foreach (var directory in directories)
				{
					_fileSystem.CreateDirectory(directory.FullPath);
				}

				// Copy our files.
				foreach (var file in files)
				{
					using (var inputStream = file.OpenStream(false))
					{
						using (var outputStream = _fileSystem.OpenStream(file.FullPath, true))
						{
							inputStream.CopyTo(outputStream);
						}
					}
				}
			}
			catch
			{
				// Recreate the scratch area if we have an exception.  This will ensure we don't 
				// get bad data.
				CleanUp();
				CreateScratchArea(ScratchDirectory);

				throw;
			}
		}
		#endregion
		#endregion
	}
}
