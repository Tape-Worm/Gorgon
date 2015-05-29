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
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Properties;
using Gorgon.Graphics;
using Gorgon.IO;
using Gorgon.PlugIns;

namespace Gorgon.Editor
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
		private readonly static List<DisabledPlugIn> _disabled;
		#endregion

        #region Properties.

		/// <summary>
		/// Property to set or return the default image editor plug-in setting.
		/// </summary>
		public static string DefaultImageEditorPlugIn
		{
			get;
			set;
		}

		/// <summary>
        /// Property to set or return the graphics interface used by the editor.
        /// </summary>
	    public static GorgonGraphics Graphics
	    {
	        get;
	        set;
	    }

        /// <summary>
        /// Property to set or return the path to the plug-ins.
        /// </summary>
	    public static string PlugInPath
	    {
	        get;
	        set;
	    }

        /// <summary>
        /// Property to set or return the list of disabled plug-ins disabled by the user.
        /// </summary>
	    public static IList<string> UserDisabledPlugIns
	    {
	        get;
	        set;
	    }

        /// <summary>
        /// Property to return the list of writer plug-ins.
        /// </summary>
	    public static IReadOnlyDictionary<string, FileWriterPlugIn> WriterPlugIns
	    {
	        get
	        {
	            return _writerPlugIns;
	        }
	    }

        /// <summary>
        /// Property to return the list of reader plug-ins.
        /// </summary>
	    public static IReadOnlyDictionary<string, GorgonFileSystemProviderPlugIn> ReaderPlugIns
	    {
	        get
	        {
	            return _readerPlugIns;
	        }
	    }

        /// <summary>
        /// Property to return the list of content plug-ins.
        /// </summary>
	    public static IReadOnlyDictionary<string, ContentPlugIn> ContentPlugIns
	    {
	        get
	        {
	            return _contentPlugIns;
	        }
	    }
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
				EditorLogging.Print("Loading plug-in assembly \"{0}\".", assembly.FullName);

				if (!GorgonApplication.PlugIns.IsPlugInAssembly(assembly.FullName))
				{
					EditorLogging.Print("Assembly \"{0}\" is not a valid plug-in assembly.", assembly.FullName);
					continue;
				}

				if (callback != null)
				{
					callback(assembly.FullName);
				}

				// Load the DLL and return the list of plugins from it.
				AssemblyName name = GorgonApplication.PlugIns.LoadPlugInAssembly(assembly.FullName);
				IEnumerable<GorgonPlugIn> plugIns = GorgonApplication.PlugIns.EnumeratePlugIns(name);
				
				results.AddRange(plugIns.Where(item => item is EditorPlugIn || item is GorgonFileSystemProviderPlugIn));
			}

			return results;
		}

        /// <summary>
        /// Function to determine if a plug-in has been disabled.
        /// </summary>
        /// <param name="plugIn">Plug-in to check.</param>
        /// <returns><c>true</c> if the plug-in has been disabled, <c>false</c> if not.</returns>
	    public static bool IsDisabled(GorgonPlugIn plugIn)
        {
            return _disabled.Contains(new DisabledPlugIn(plugIn, String.Empty));
        }

        /// <summary>
        /// Function to return the reason that a plug-in was disabled.
        /// </summary>
        /// <param name="plugIn">Plug-in to look up.</param>
        /// <returns>A string containing the reason that a plug-in was disabled.</returns>
	    public static string GetDisabledReason(GorgonPlugIn plugIn)
        {
            var disabled = new DisabledPlugIn(plugIn, String.Empty);
            int index = _disabled.IndexOf(disabled);

            return index > -1 ? _disabled[index].Reason : String.Empty;
        }

		/// <summary>
		/// Function to load the plug-ins for the application.
		/// </summary>
		/// <param name="callback">The function to call back to when a plug-in is loaded.</param>
		/// <returns>The number of plug-ins loaded.</returns>
		public static int LoadPlugIns(Action<string> callback)
		{
			var plugInDirectory = new DirectoryInfo(PlugInPath);

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
				if (UserDisabledPlugIns.Any(item => String.Equals(item, plugIn.Name, StringComparison.OrdinalIgnoreCase)))
				{
					EditorLogging.Print("Found plug-in: \"{0}\".  But it is disabled by the user.", plugIn.Description);

					_disabled.Add(new DisabledPlugIn(plugIn, APIResources.GOREDIT_TEXT_DISABLED_BY_USER));
					
					continue;
				}

				// Check for a file system provider first.
				var fileReader = plugIn as GorgonFileSystemProviderPlugIn;

				if (fileReader != null)
				{
					EditorLogging.Print("Found a file system provider plug-in: \"{0}\".", fileReader.Name);


					_readerPlugIns[fileReader.Name] = fileReader;
					continue;
				}

				var editorPlugIn = (EditorPlugIn)plugIn;

				var validationData = editorPlugIn.ValidatePlugIn();

				if (!String.IsNullOrWhiteSpace(validationData))
				{
					EditorLogging.Print("Found a {0} plug-in: \"{1}\".  But it is disabled for the following reasons:",
					                      editorPlugIn.PlugInType,
					                      editorPlugIn.Description);
					EditorLogging.Print("{0}", validationData);

					_disabled.Add(new DisabledPlugIn(plugIn, validationData));
					continue;
				}

				EditorLogging.Print("Found a {0} plug-in: \"{1}\".", editorPlugIn.PlugInType, editorPlugIn.Description);

				// Categorize the editor plug-ins.
				switch (editorPlugIn.PlugInType)
				{
					case PlugInType.Content:
						var contentPlugIn = editorPlugIn as ContentPlugIn;

						if (contentPlugIn != null)
						{
							_contentPlugIns[editorPlugIn.Name] = contentPlugIn;
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
				        EditorLogging.Print("Found a {0} plug-in: \"{1}\".  But it is disabled for the following reasons:",
				                            editorPlugIn.PlugInType,
				                            editorPlugIn.Description);
						EditorLogging.Print("Plug-in type is unknown.", LoggingLevel.Verbose);

						_disabled.Add(new DisabledPlugIn(editorPlugIn, APIResources.GOREDIT_TEXT_UNKNOWN_PLUG_IN_TYPE));
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
			
			_disabled = new List<DisabledPlugIn>();
		}
		#endregion
    }
}
