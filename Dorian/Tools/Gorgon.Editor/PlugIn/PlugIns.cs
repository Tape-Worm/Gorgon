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
// Created: Monday, September 23, 2013 12:06:20 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.IO;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// The primary plug-in interface.
	/// </summary>
	static class PlugIns
	{
		#region Variables.
		private readonly static Dictionary<string, FileWriterPlugIn> _writerPlugIns;
		private readonly static Dictionary<string, GorgonFileSystemProviderPlugIn> _readerPlugIns;
		private readonly static Dictionary<string, ContentPlugIn> _contentPlugIns;
		private readonly static Dictionary<FileExtension, ContentPlugIn> _contentFiles;
		private readonly static List<DisabledPlugIn> _disabled;
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to load all the plug-in assemblies from the plug-in directory.
		/// </summary>
		/// <param name="plugInDirectory">Plug-in directory that contains the assemblies.</param>
		/// <param name="callback">The function to call back to when loading the assembly.</param>
		/// <returns>The list of plug-ins loaded.</returns>
		private static IList<GorgonPlugIn> LoadAssemblies(DirectoryInfo plugInDirectory, Action<string> callback)
		{
			var results = new List<GorgonPlugIn>();
			IEnumerable<FileInfo> assemblies = plugInDirectory.GetFiles("*.dll", SearchOption.AllDirectories);

			foreach (var assembly in assemblies)
			{
				Program.LogFile.Print("Loading plug-in assembly \"{0}\".", LoggingLevel.Verbose, assembly.FullName);

				if (!Gorgon.PlugIns.IsPlugInAssembly(assembly.FullName))
				{
					Program.LogFile.Print("Assembly \"{0}\" is not a valid plug-in assembly.", LoggingLevel.Verbose, assembly.FullName);
					continue;
				}

				if (callback != null)
				{
					callback(assembly.FullName);
				}

				// Load the DLL and return the list of plugins from it.
				AssemblyName name = Gorgon.PlugIns.LoadPlugInAssembly(assembly.FullName);
				IEnumerable<GorgonPlugIn> plugIns = Gorgon.PlugIns.EnumeratePlugIns(name);
				
				results.AddRange(plugIns.Where(item => item is EditorPlugIn || item is GorgonFileSystemProviderPlugIn));
			}

			return results;
		}

		/// <summary>
		/// Function to retrieve the content file types available in the plug-in.
		/// </summary>
		/// <param name="plugIn">Plug-in containing the content types.</param>
		private static void GetContentFileTypes(ContentPlugIn plugIn)
		{
			if (plugIn.FileExtensions.Count == 0)
			{
				return;
			}

			// Associate the content file type with the plug-in.
			foreach (var extension in plugIn.FileExtensions)
			{
				_contentFiles[extension] = plugIn;
			}
		}

		/// <summary>
		/// Function to retrieve the list of available content extensions.
		/// </summary>
		/// <returns>A list of content file name extensions.</returns>
		public static IEnumerable<FileExtension> GetContentExtensions()
		{
			return _contentFiles.Keys;
		}

		/// <summary>
		/// Function to return a related plug-in for the given content file.
		/// </summary>
		/// <param name="fileName">Name of the content file.</param>
		/// <returns>The plug-in used to access the file.</returns>
		public static ContentPlugIn GetContentPlugInForFile(string fileName)
		{
			string extension = Path.GetExtension(fileName);

			return !CanOpenContent(extension) ? null : _contentFiles[new FileExtension(extension, null)];
		}

		/// <summary>
		/// Function to determine if a certain type of content can be opened by a plug-in.
		/// </summary>
		/// <param name="fileName">Filename of the content.</param>
		/// <returns>TRUE if the content can be opened, FALSE if not.</returns>
		public static bool CanOpenContent(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
			{
				return false;
			}

			string extension = Path.GetExtension(fileName);

			return !string.IsNullOrWhiteSpace(extension) && _contentFiles.ContainsKey(new FileExtension(extension, null));
		}

		/// <summary>
		/// Function to load the plug-ins for the application.
		/// </summary>
		/// <param name="callback">The function to call back to when a plug-in is loaded.</param>
		/// <returns>The number of plug-ins loaded.</returns>
		public static int LoadPlugIns(Action<string> callback)
		{
			var plugInDirectory = new DirectoryInfo(Program.Settings.PlugInDirectory);

			if (!plugInDirectory.Exists)
			{
				plugInDirectory.Create();
			}

			IList<GorgonPlugIn> plugIns = LoadAssemblies(plugInDirectory, callback);

			if (plugIns.Count == 0)
			{
				return 0;
			}

			// Process each plug-in.
			foreach (var plugIn in plugIns)
			{
				// Check for a file system provider first.
				var fileReader = plugIn as GorgonFileSystemProviderPlugIn;

				if (fileReader != null)
				{
					Program.LogFile.Print("Found a file system provider plug-in: \"{0}\".", LoggingLevel.Verbose, fileReader.Name);

					_readerPlugIns[fileReader.Name] = fileReader;
					continue;
				}

				var editorPlugIn = (EditorPlugIn)plugIn;
				var validationData = editorPlugIn.ValidatePlugIn();

				if (!string.IsNullOrWhiteSpace(validationData))
				{
					Program.LogFile.Print("Found a {0} plug-in: \"{1}\".  But it is disabled for the following reasons:", LoggingLevel.Verbose, editorPlugIn.PlugInType, editorPlugIn.Description);
					Program.LogFile.Print("{0}", LoggingLevel.Verbose, validationData);

					_disabled.Add(new DisabledPlugIn(editorPlugIn, validationData));
					continue;
				}

				Program.LogFile.Print("Found a {0} plug-in: \"{1}\".", LoggingLevel.Verbose, editorPlugIn.PlugInType, editorPlugIn.Description);

				// Categorize the editor plug-ins.
				switch (editorPlugIn.PlugInType)
				{
					case PlugInType.Content:
						var contentPlugIn = editorPlugIn as ContentPlugIn;

						if (contentPlugIn != null)
						{
							_contentPlugIns[editorPlugIn.Name] = contentPlugIn;

							GetContentFileTypes(contentPlugIn);
						}
						break;
					case PlugInType.FileWriter:
						var writerPlugIn = editorPlugIn as FileWriterPlugIn;

						if (writerPlugIn != null)
						{
							_writerPlugIns[editorPlugIn.Name] = writerPlugIn;
						}
						break;
					default:
						Program.LogFile.Print("Found a {0} plug-in: \"{1}\".  But it is disabled for the following reasons:", LoggingLevel.Verbose, editorPlugIn.PlugInType, editorPlugIn.Description);
						Program.LogFile.Print("Plug-in type is unknown.", LoggingLevel.Verbose);

						_disabled.Add(new DisabledPlugIn(editorPlugIn, "Unknown plug-in type."));
						break;
				}
			}

			return plugIns.Count;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="PlugIns"/> class.
		/// </summary>
		static PlugIns()
		{
			_contentPlugIns = new Dictionary<string, ContentPlugIn>();
			_readerPlugIns = new Dictionary<string, GorgonFileSystemProviderPlugIn>();
			_writerPlugIns = new Dictionary<string, FileWriterPlugIn>();
			_contentFiles = new Dictionary<FileExtension, ContentPlugIn>();
			_disabled = new List<DisabledPlugIn>();
		}
		#endregion
	}
}
