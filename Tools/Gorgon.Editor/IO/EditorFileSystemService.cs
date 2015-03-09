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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor
{
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
		private GorgonLogFile _log;
		// The scratch file interface.
		private IScratchArea _scratchArea;
		// The plug-in registry holding the file system providers available.
		private IPlugInRegistry _plugInRegistry;
		// The list of file system providers indexed by file extension.
		private readonly IDictionary<GorgonFileExtension, GorgonFileSystemProvider> _providers;
		// Application settings.
		private IEditorSettings _settings;
		// The file system used for packed files.
		private readonly GorgonFileSystem _packedFileSystem;
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
		#endregion

		#region Methods.
		/// <summary>
		/// Function to load a file from the physical file system.
		/// </summary>
		/// <param name="path">Path to the file to load.</param>
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


		}

		/// <summary>
		/// Function to build the file system provider list.
		/// </summary>
		public void GetFileSystemProviders()
		{
			var readerFileTypeList = new StringBuilder();
			var extensionList = new StringBuilder();
			var allSupportedList = new StringBuilder();

			// Retrieve all of our file system providers for this file system.
			_packedFileSystem.Clear();
			_packedFileSystem.Providers.UnloadAll();
			_packedFileSystem.Providers.LoadAllProviders();

			_log.Print("Building file system provider list...", LoggingLevel.Verbose);

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
						_log.Print("Extension '{0}' is already assigned to a provider, skipping...", LoggingLevel.Verbose);
						continue;
					}

					_log.Print("Extension '{0} - {1}' assigned to provider {2} ({3})",
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

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorFileSystemService"/> class.
		/// </summary>
		/// <param name="log">The application log file.</param>
		/// <param name="scratchArea">The scratch file area used to hold a copy of the file being edited.</param>
		/// <param name="packFileSystem">The file system used to read packed files.</param>
		/// <param name="plugInRegistry">The plug-in registry holding the file system providers we need.</param>
		/// <param name="settings">The application settings.</param>
		public EditorFileSystemService(GorgonLogFile log, IProxyObject<GorgonFileSystem> packFileSystem, IScratchArea scratchArea, IPlugInRegistry plugInRegistry, IEditorSettings settings)
		{
			_log = log;
			_scratchArea = scratchArea;
			_providers = new Dictionary<GorgonFileExtension, GorgonFileSystemProvider>(new GorgonFileExtensionComparer());
			_plugInRegistry = plugInRegistry;
			_settings = settings;
			_packedFileSystem = packFileSystem.Item;
		}
		#endregion
	}
}
