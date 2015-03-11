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
// Created: Monday, March 9, 2015 12:16:11 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor
{
#warning Extract out some of this functionality into an "editor" file object type and have it returned by Load.  Maybe make it so that "current" file does not exist in here?  Also, put the "HasChanges" property on the file object, not in the service and hook the scratch area changed event to that file.
	/// <summary>
	/// The file system service used to manipulate items in the file system.
	/// </summary>
	/// <remarks>
	/// This object is used to create, load and save file systems.
	/// </remarks>
	class EditorFileSystemService 
		: IFileSystemService
	{
		#region Variables.
		// The application log file.
		private readonly GorgonLogFile _log;
		// The scratch file interface.
		private readonly IScratchArea _scratchArea;
		// The plug-in registry holding the file system providers available.
		private readonly IPlugInRegistry _plugInRegistry;
		// The list of file system providers indexed by file extension.
		private readonly IDictionary<GorgonFileExtension, GorgonFileSystemProvider> _providers;
		// Application settings.
		private readonly IEditorSettings _settings;
		// The file system used for packed files.
		private readonly GorgonFileSystem _packedFileSystem;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the applicable file system provider for the file being loaded.
		/// </summary>
		/// <param name="extension">The file extension to look up.</param>
		/// <returns>The file system provider if found, NULL (Nothing in VB.Net) if not.</returns>
		private GorgonFileSystemProvider GetProviderByExtension(string extension)
		{
			var fileExtension = new GorgonFileExtension(extension);
			GorgonFileSystemProvider provider;

			_providers.TryGetValue(fileExtension, out provider);

			return provider;
		}

		/// <summary>
		/// Function to search all loaded providers to find one that will load the requested file.
		/// </summary>
		/// <param name="path">Path to the packed file to load.</param>
		/// <returns>The file system provider if found, NULL (Nothing in VB.Net) if not.</returns>
		private GorgonFileSystemProvider SearchForProvider(string path)
		{
			return _providers.Values.FirstOrDefault(item => item.CanReadFile(path));
		}

		/// <summary>
		/// Function to retrieve a provider based on the file in the path provided.
		/// </summary>
		/// <param name="path">Path to the packed file to load.</param>
		/// <returns>The file system provider if found, NULL (Nothing in VB.Net) if not.</returns>
		private GorgonFileSystemProvider GetProvider(string path)
		{
			string extension = Path.GetExtension(path);

			// Ensure that we have an extension.
			if (string.IsNullOrWhiteSpace(extension))
			{
				// Search for the provider.
				 return SearchForProvider(path);
			}

			GorgonFileSystemProvider provider = GetProviderByExtension(extension);

			if ((provider == null)
				|| (!provider.CanReadFile(path)))
			{
				return SearchForProvider(path);
			}
			
			return provider;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorFileSystemService"/> class.
		/// </summary>
		/// <param name="log">The application log file.</param>
		/// <param name="scratchArea">The scratch file area used to hold a copy of the file being edited.</param>
		/// <param name="packFileSystem">The file system used to read packed files.</param>
		/// <param name="plugInRegistry">The plug-in registry holding the file system providers we need.</param>
		/// <param name="settings">The application settings.</param>
		public EditorFileSystemService(GorgonLogFile log, GorgonFileSystem packFileSystem, IScratchArea scratchArea, IPlugInRegistry plugInRegistry, IEditorSettings settings)
		{
			_log = log;
			_scratchArea = scratchArea;
			_providers = new Dictionary<GorgonFileExtension, GorgonFileSystemProvider>(new GorgonFileExtensionComparer());
			_settings = settings;
			_packedFileSystem = packFileSystem;
			_plugInRegistry = plugInRegistry;
			CurrentFile = string.Empty;
			CurrentFilePath = string.Empty;

			// Notification for disabled plug-ins.
			_plugInRegistry.PlugInDisabled += (sender, args) =>
			                                  {
												  // We're only interested in file system provider plug-ins here.
												  // TODO: And writers when they're implemented.
				                                  if (!(args.PlugIn is GorgonFileSystemProviderPlugIn))
				                                  {
					                                  return;
				                                  }

												  // Reload the file system providers if we've disabled one.
				                                  LoadFileSystemProviders();
			                                  };
		}
		#endregion

		#region IFileSystemService Implementation.
		#region Events.
		/// <summary>
		/// Event fired when a file is loaded.
		/// </summary>
		public event EventHandler FileLoaded;
		/// <summary>
		/// Event fired when a file is saved.
		/// </summary>
		public event EventHandler FileSaved;
		/// <summary>
		/// Event fired when a file is unloaded.
		/// </summary>
		public event EventHandler FileUnloaded;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a string of file types supported for reading.
		/// </summary>
		/// <remarks>This property will return a string formatted for the open file dialog extension property.</remarks>
		public string ReadFileTypes
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the name of the currently loaded file.
		/// </summary>
		public string CurrentFile
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the full path to the current file.
		/// </summary>
		public string CurrentFilePath
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether any file system providers are loaded into the application or not.
		/// </summary>
		public bool HasFileSystemProviders
		{
			get
			{
				return _providers.Count > 0;
			}
		}

		/// <summary>
		/// Property to return the default file type for a writer.
		/// </summary>
		public string WriterDefaultFileType
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the current writer extension index for the currently loaded file.
		/// </summary>
		/// <remarks>
		/// This is useful to determining the (1 based) index to assign to a save file dialog if we currently have a file open.
		/// <para>This value will be 0 if no file is currently loaded, or if no provider could be found for the currently loaded file.</para>
		/// </remarks>
		public int WriterCurrentExtensionIndex
		{
			get
			{
				// TODO: Change this to use the file system writer plug-ins.
				/*if ((_providers.Count == 0)
					|| (string.IsNullOrWhiteSpace(CurrentFile)))
				{
					return 0;
				}

				string fileExtension = Path.GetExtension(CurrentFile);

				if (string.IsNullOrWhiteSpace(fileExtension))
				{
					return 0;
				}

				var extension = new GorgonFileExtension(Path.GetExtension(CurrentFile));
				return _providers.Keys.TakeWhile(currentExt => !extension.Equals(currentExt)).Count();*/
				return 1;
			}
		}

		/// <summary>
		/// Property to return whether there have been changes to this file system or not.
		/// </summary>
		public bool HasChanges
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if the application can read the packed file or not.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		/// <returns>TRUE if the file can be read, FALSE if not.</returns>
		/// <remarks>
		/// This method will check against the internal list of file system providers to see if the extension is known. If the file cannot be located by extension, 
		/// or the file could not be read, then all providers will be tested to determine if the file can be read.
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
		public bool CanReadFile(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException(Resources.GOREDIT_ERR_PARAMETER_MUST_NOT_BE_EMPTY, "path");
			}

			try
			{
				return GetProvider(path) != null;
			}
			catch (Exception ex)
			{
				// Exceptions for this method are logged, but will just indicate that we cannot load the file.
				GorgonException.Catch(ex);
				return false;
			}
		}

		/// <summary>
		/// Function to unload the currently loaded file.
		/// </summary>
		public void UnloadCurrentFile()
		{
			// Reset the scratch area.
			try
			{
				_scratchArea.CleanUp();
			}
			catch(Exception ex)
			{
				// If we can't clean up the scratch area for some reason (e.g. explorer has "helped" us by locking the folder), then 
				// we'll just leave it for now.  It'll be expunged on exit or when the application loads up again.
				_log.Print("FileSystemService: Exception generated while performing scratch area clean up.  Error: {0}", LoggingLevel.Simple, ex.Message);
			}
			_scratchArea.SetScratchDirectory(_scratchArea.ScratchDirectory);

			if (string.IsNullOrWhiteSpace(CurrentFilePath))
			{
				return;
			}

			_log.Print("FileSystemService: Unloading the file '{0}'", LoggingLevel.Verbose, CurrentFilePath);

			CurrentFilePath = string.Empty;
			CurrentFile = string.Empty;
			HasChanges = false;

			if (FileUnloaded != null)
			{
				FileUnloaded(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Function to load a file from the physical file system.
		/// </summary>
		/// <param name="path">Path to the file to load.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the file could not be read by any of the known providers.</exception>
		/// <exception cref="System.IO.FileNotFoundException">Thrown when the file in the <paramref name="path"/> could not be found.</exception>
		public void LoadFile(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException(Resources.GOREDIT_ERR_PARAMETER_MUST_NOT_BE_EMPTY, "path");
			}

			path = Path.GetFullPath(path);

			if (!File.Exists(path))
			{
				throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, path));
			}

			if (GetProvider(path) == null)
			{
				throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_CANNOT_LOCATE_PROVIDER, path));
			}

			try
			{
				_packedFileSystem.Mount(path);

				// Copy to the scratch area.
				_scratchArea.CopyFileSystem(_packedFileSystem);

				// Note the current file.
				CurrentFile = Path.GetFileName(path);
				CurrentFilePath = path;
				HasChanges = false;

				// Once the copy is complete, notify anyone who's listening that we've loaded a new file.
				if (FileLoaded != null)
				{
					FileLoaded(this, EventArgs.Empty);
				}
				
				// TODO: Find an applicable writer plug-in for this file.

				_settings.LastEditorFile = path;
			}
			finally
			{
				// Unload previous mount points (we should only have 1, but just cover our asses regardless).
				if (_packedFileSystem.MountPoints.Count > 0)
				{
					while (_packedFileSystem.MountPoints.Count > 0)
					{
						_packedFileSystem.Unmount(_packedFileSystem.MountPoints[0]);
					}
				}
			}
		}

		/// <summary>
		/// Function to load the file system providers available to the application.
		/// </summary>
		public void LoadFileSystemProviders()
		{
			var readerFileTypeList = new StringBuilder();
			var extensionList = new StringBuilder();
			var allSupportedList = new StringBuilder();

			// Retrieve all of our file system providers for this file system.
			_providers.Clear();
			_packedFileSystem.Clear();
			_packedFileSystem.Providers.UnloadAll();

			// Load only those providers in our plug-in list (disabled plug-ins will not be in here).
			foreach (var plugIn in _plugInRegistry.FileSystemPlugIns)
			{
				_packedFileSystem.Providers.LoadProvider(plugIn.Value.Name);
			}

			_log.Print("FileSystemService: Building file system provider list...", LoggingLevel.Verbose);

			// Get a list of the extensions for the file system providers.
			foreach (var provider in _packedFileSystem.Providers)
			{
				if (readerFileTypeList.Length > 0)
				{
					readerFileTypeList.Append("|");
				}

				var firstExtension = provider.PreferredExtensions.First();

				readerFileTypeList.Append(firstExtension.ToDialogDescription());
				readerFileTypeList.Append("|");

				extensionList.Length = 0;

				foreach (var extension in provider.PreferredExtensions)
				{
					if (_providers.ContainsKey(extension))
					{
						_log.Print("FileSystemService: Extension '{0}' is already assigned to a provider, skipping...", LoggingLevel.Verbose);
						continue;
					}

					_log.Print("FileSystemService: Extension '{0} - {1}' assigned to provider {2} ({3})",
					           LoggingLevel.Verbose,
					           extension.Extension,
					           extension.Description,
							   provider.Name,
							   provider.Description);

					_providers[extension] = provider;

					if (extensionList.Length > 0)
					{
						extensionList.Append(";");
					}

					extensionList.Append("*.");
					extensionList.Append(extension.Extension);
				}

				if (allSupportedList.Length > 0)
				{
					allSupportedList.Append(";");
				}

				allSupportedList.Append(extensionList);
				readerFileTypeList.Append(extensionList);
			}

			// If we have more than 1 provider, then show the all supported list.
			if ((_providers.Count > 1)
				&& (allSupportedList.Length > 0))
			{
				readerFileTypeList.Append("|");
				readerFileTypeList.AppendFormat(Resources.GOREDIT_DLG_ALL_SUPPORTED_CONTENT_FILES, allSupportedList);
				readerFileTypeList.Append("|");
				readerFileTypeList.Append(allSupportedList);
			}

			if (readerFileTypeList.Length > 0)
			{
				readerFileTypeList.Append("|");
			}

			readerFileTypeList.Append(Resources.GOREDIT_DLG_ALL_FILES);
			readerFileTypeList.Append("|*.*");

			ReadFileTypes = readerFileTypeList.ToString();
		}
		#endregion
		#endregion
	}
}
