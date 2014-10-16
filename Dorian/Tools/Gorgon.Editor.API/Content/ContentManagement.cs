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
// Created: Tuesday, September 24, 2013 10:37:50 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Windows.Forms;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor
{
    /// <summary>
    /// Interface for content management.
    /// </summary>
    static class ContentManagement
    {
        #region Variables.
        private readonly static Dictionary<GorgonFileExtension, ContentPlugIn> _contentFiles;
	    private readonly static HashSet<string> _availablePlugIns;
	    private static ContentObject _currentContentObject;
	    private static bool _contentChanged ;
	    private static Type _defaultContentType;
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
		/// Property to set or return the application graphics interface.
		/// </summary>
	    public static GorgonGraphics Graphics
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the type of the default content object.
		/// </summary>
	    public static Type DefaultContentType
		{
			get
			{
				return _defaultContentType;
			}
			set
			{
				if ((value != null)
				    && (!value.IsSubclassOf(typeof(ContentObject))))
				{
					throw new GorgonException(GorgonResult.CannotBind, string.Format(APIResources.GOREDIT_ERR_DEFAULT_TYPE_NOT_CONTENT, value.FullName));
				}

				_defaultContentType = value;
			}
		}

		/// <summary>
		/// Property to return whether the current content has been changed or not.
		/// </summary>
	    public static bool Changed
	    {
		    get
		    {
			    return ((Current != null) && (_contentChanged));
			}
	    }

		/// <summary>
		/// Property to set or return the method to call if a dependency could not be loaded.
		/// </summary>
	    public static Action<string, IList<string>> DependencyNotFound
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the method to call after a content pane is unloaded.
		/// </summary>
	    public static Action ContentPanelUnloadAction
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the method to call after the content object is initialized.
		/// </summary>
	    public static Action<ContentPanel> ContentInitializedAction
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the method to call to enumerate the properties for a content pane.
		/// </summary>
		/// <remarks>The parameter of the method indicates whether properties are used by the content or not.</remarks>
	    public static Action<bool> ContentEnumerateProperties
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the method to call when content is renamed.
		/// </summary>
		/// <remarks>This only takes effect if properties are available for public use on the content.  The parameter for the method is as follows: <c>string newName</c>, <c>bool overrideExtension</c></remarks>
	    public static Action<string, bool> ContentRenamed
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the method to call when the current content is persisted back to the file system.
		/// </summary>
	    public static Action ContentSaved
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the method to call when a content property has its enabled/disabled state changed.
		/// </summary>
	    public static Action ContentPropertyStateChanged
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the method to call when a dependency needs to be loaded for content..
		/// </summary>
	    public static Func<string, GorgonFileSystemFileEntry> OnGetDependency
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to return the currently active content.
		/// </summary>
	    public static ContentObject Current
	    {
		    get;
		    private set;
	    }
        #endregion

        #region Methods.
		/// <summary>
		/// Function called during idle time for rendering.
		/// </summary>
		/// <returns>TRUE to continue rendering, FALSE to stop.</returns>
	    private static bool IdleLoop()
	    {
		    if (_currentContentObject != null)
		    {
			    _currentContentObject.Draw();
		    }
		    return true;
	    }

		/// <summary>
		/// Function called when a property on the content has been changed.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event parameters.</param>
		private static void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			_contentChanged = true;
		}

		/// <summary>
		/// Function to load the content and its related UI.
		/// </summary>
		/// <param name="contentObject">Content to load.</param>
		/// <param name="isDefault">TRUE if this is the default content, FALSE if not.</param>
	    private static void LoadContent(ContentObject contentObject, bool isDefault)
	    {
			// Unload any content that's currently active.
			UnloadCurrentContent();

			// Initialize content resources.
			ContentPanel contentWindow = contentObject.InitializeContent();

			_currentContentObject = contentObject;

			// Do not count the default content pane as being "content".
			// This "Current" content being null will be our indicator that no content is currently active.
			if (!isDefault)
			{
				Current = contentObject;
			}

			if (contentWindow == null)
			{
				return;
			}

			if (ContentInitializedAction != null)
			{
				contentWindow.Dock = DockStyle.Fill;
				ContentInitializedAction(contentWindow);
			}

			if (ContentEnumerateProperties != null)
			{
				// Enumerate properties for the content.
				ContentEnumerateProperties(contentObject.HasProperties);
			}

			if (contentObject.HasProperties)
			{
				contentObject.PropertyChanged += OnPropertyChanged;
			}

			// Force focus to the content window.
			if (contentWindow.Parent != null)
			{
				contentWindow.Focus();
			}

			if (contentObject.HasRenderer)
			{
				Gorgon.ApplicationIdleLoopMethod = IdleLoop;
			}
	    }

		/// <summary>
		/// Function to tell the content that the editor settings have changed.
		/// </summary>
	    public static void EditorSettingsUpdated()
	    {
			if (_currentContentObject == null)
			{
				return;
			}

			_currentContentObject.OnEditorSettingsUpdated();

			if (_currentContentObject.ContentControl != null)
			{
				_currentContentObject.ContentControl.OnEditorSettingsChanged();
			}
	    }

		/// <summary>
        /// Function to retrieve the list of available content extensions.
        /// </summary>
        /// <returns>A list of content file name extensions.</returns>
        public static IEnumerable<GorgonFileExtension> GetContentExtensions()
        {
            return _contentFiles.Keys;
        }

        /// <summary>
        /// Function to return a related plug-in for the given content file.
        /// </summary>
        /// <param name="fileExtension">Name of the content file.</param>
        /// <returns>The plug-in used to access the file.</returns>
        public static ContentPlugIn GetContentPlugInForFile(string fileExtension)
        {
	        if (string.IsNullOrWhiteSpace(fileExtension))
	        {
		        return null;
	        }

            if (fileExtension.IndexOf('.') > 0)
            {
                fileExtension = Path.GetExtension(fileExtension);
            }

	        ContentPlugIn plugIn;

	        _contentFiles.TryGetValue(new GorgonFileExtension(fileExtension, null), out plugIn);

	        return plugIn;
        }

		/// <summary>
		/// Function to determine if a file can be opened for viewing/editing.
		/// </summary>
		/// <param name="file">File to evaluate.</param>
		/// <returns>TRUE if the file can be opened, FALSE if not.</returns>
	    public static bool CanOpenContent(EditorFile file)
	    {
			if (file == null)
			{
				return false;
			}

			// Check to see if the plug-in exists.
			return !string.IsNullOrWhiteSpace(file.PlugInType) && _availablePlugIns.Contains(file.PlugInType);
	    }

		/// <summary>
		/// Function to unload any current content.
		/// </summary>
	    public static void UnloadCurrentContent()
	    {
			if (_currentContentObject == null)
			{
				return;
			}

			// Turn off the public facing content object.
			Current = null;

			// Turn off any idle time activity during the load.
			Gorgon.ApplicationIdleLoopMethod = null;

			if (ContentPanelUnloadAction != null)
			{
				ContentPanelUnloadAction();
			}

			// Close the content object.  This should preserve any changes.
			_currentContentObject.PropertyChanged -= OnPropertyChanged;
			_currentContentObject.Dispose();
			_currentContentObject = null;
	    }

		/// <summary>
		/// Function to load a content object into the content pane in the interface.
		/// </summary>
		/// <param name="contentObject">Content object to load into the interface.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="contentObject"/> is NULL (Nothing in VB.Net).</exception>
	    public static void LoadContentPane(ContentObject contentObject)
	    {
			if (contentObject == null)
			{
				throw new ArgumentNullException("contentObject");
			}

			LoadContent(contentObject, false);
	    }
		
		/// <summary>
		/// Function load the default content pane into the interface.
		/// </summary>
		/// <exception cref="GorgonException">Thrown when the <see cref="DefaultContentType"/> property is set to NULL (Nothing in VB.Net).</exception>
	    public static void LoadDefaultContentPane()
	    {
			if (DefaultContentType == null)
			{
				throw new GorgonException(GorgonResult.CannotCreate, APIResources.GOREDIT_ERR_DEFAULT_TYPE_UNKNOWN);
			}

			// We already have the default pane loaded.
		    if ((Current == null) && (_currentContentObject != null))
		    {
			    return;
		    }

			var defaultContent = (ContentObject)Activator.CreateInstance(DefaultContentType,
			                                                             BindingFlags.CreateInstance,
			                                                             null,
			                                                             null,
			                                                             null,
			                                                             null);

			LoadContent(defaultContent, true);
	    }

        /// <summary>
        /// Function to create a new content object.
        /// </summary>
        /// <param name="plugIn">The plug-in used to create the object.</param>
        /// <returns>The content object or NULL if the content object was not created.</returns>
        public static ContentObject Create(ContentPlugIn plugIn)
        {
            if (plugIn == null)
            {
                throw new ArgumentNullException("plugIn");
            }

            // Perform any set up required on the content.
            ContentSettings settings = plugIn.GetContentSettings();

            if (settings != null)
            {
                if (!settings.PerformSetup())
                {
                    return null;
                }

                settings.CreateContent = true;
            }

            ContentObject content = plugIn.CreateContentObject(settings);

            if (content.HasProperties)
            {
                content.SetDefaults();
            }

			content.GetRegisteredImageEditor(DefaultImageEditorPlugIn);

	        if ((content.ImageEditor != null)
	            && (string.IsNullOrWhiteSpace(DefaultImageEditorPlugIn)))
	        {
		        DefaultImageEditorPlugIn = content.ImageEditor.Name;
	        }

            return content;
        }

		/// <summary>
		/// Function to load dependencies for a file.
		/// </summary>
		/// <param name="content">The content that is being loaded.</param>
		/// <param name="filePath">Path to the file that may contain dependencies.</param>
		/// <param name="missing">A list of dependencies that are missing.</param>
	    private static void LoadDependencies(ContentObject content, string filePath, List<string> missing)
	    {
			foreach (Dependency dependencyFile in EditorMetaDataFile.Dependencies[filePath])
			{
				GorgonFileSystemFileEntry externalFile = OnGetDependency(dependencyFile.Path);

				if (externalFile == null)
				{
					throw new FileNotFoundException(string.Format(APIResources.GOREDIT_ERR_CANNOT_FIND_DEPENDENCY_FILE,
						                                                        dependencyFile.Path));
				}

				// If the dependency file contains any dependencies, then we need to load it them prior to loading the actual dependency.
				if ((EditorMetaDataFile.Dependencies.ContainsKey(dependencyFile.Path))
					&& (EditorMetaDataFile.Dependencies[dependencyFile.Path].Count > 0))
				{
					LoadDependencies(content, dependencyFile.Path, missing);
				}

				// Read the external content.
				using (Stream dependencyStream = externalFile.OpenStream(false))
				{
					DependencyLoadResult result = content.LoadDependencyFile(dependencyFile, dependencyStream);
					switch (result.State)
					{
						case DependencyLoadState.FatalError:
							throw new GorgonException(GorgonResult.CannotRead,
							                          string.Format(APIResources.GOREDIT_ERR_CANNOT_LOAD_DEPENDENCY, dependencyFile.Path, result.Message));
						case DependencyLoadState.ErrorContinue:
							missing.Add(string.Format(APIResources.GOREDIT_DLG_CANNOT_LOAD_DEPENDENCY, dependencyFile.Path, result.Message));
							break;
						default:
							content.Dependencies[dependencyFile.Path, dependencyFile.Type] = dependencyFile;
							break;
					}
					
				}
			}
	    }

		/// <summary>
		/// Function to load content data from the file system.
		/// </summary>
		/// <param name="file">The file system file that contains the content data.</param>
		/// <param name="plugIn">The plug-in used to open the file.</param>
	    public static void Load(GorgonFileSystemFileEntry file, ContentPlugIn plugIn)
	    {
		    if (file == null)
		    {
		        throw new ArgumentNullException("file");
		    }

		    ContentSettings settings = plugIn.GetContentSettings();        // Get default settings.

		    if (settings != null)
		    {
                // Assign the name from the file.
			    settings.Filename = file.Name;
                settings.Name = Path.GetFileNameWithoutExtension(file.Name);
		        settings.CreateContent = false;
		    }

            ContentObject content = plugIn.CreateContentObject(settings);

			content.GetRegisteredImageEditor(DefaultImageEditorPlugIn);

			if ((string.IsNullOrWhiteSpace(DefaultImageEditorPlugIn))
			    && (content.ImageEditor != null))
			{
				DefaultImageEditorPlugIn = content.ImageEditor.Name;
			}

            Debug.Assert(_currentContentObject != null, "Content should not be NULL!");

			// Indicate that this content is linked to another piece of content.
			content.HasOwner = EditorMetaDataFile.HasFileLinks(file);

			// Load the content dependencies if any exist.
			// Check for dependencies.
			if ((EditorMetaDataFile.Dependencies.ContainsKey(file.FullPath))
				&& (EditorMetaDataFile.Dependencies[file.FullPath].Count > 0))
			{
				var missingDependencies = new List<string>();

				try
				{
					LoadDependencies(content, file.FullPath, missingDependencies);
				}
				catch
				{
					// If we have a major failure, ensure that any dependencies that were loaded 
					// are cleaned up (if they require it).
					var cleanUp = from dependency in EditorMetaDataFile.Dependencies
					              from dependencyObject in dependency.Value
					              let disposable = dependencyObject.DependencyObject as IDisposable
					              where disposable != null
					              select new
					                     {
						                     Dependency = dependencyObject,
						                     Disposer = disposable
					                     };

					foreach (var dependency in cleanUp)
					{
						dependency.Dependency.DependencyObject = null;
						dependency.Disposer.Dispose();
					}

					throw;
				}

				if ((missingDependencies.Count > 0)
					&& (DependencyNotFound != null))
				{
					DependencyNotFound(file.Name, missingDependencies);
				}
			}

            LoadContentPane(content);

            // Load in the content data.
		    using(Stream stream = file.OpenStream(false))
		    {
		        content.Read(stream);
		    }

		    _contentChanged = false;
	    }

		/// <summary>
		/// Function to save the current content.
		/// </summary>
		/// <param name="file">The file that will contain the content data.</param>
	    public static void Save(GorgonFileSystemFileEntry file)
	    {
		    if (Current == null)
		    {
			    return;
		    }

		    if (file == null)
		    {
		        throw new ArgumentNullException("file");
		    }
			
			// Finalize the content after persisting to the file store.
			string newName = Current.OnBeforePersist();

			if ((!string.IsNullOrWhiteSpace(newName))
			    && (!Current.HasOwner))
			{
				// Ensure that this file doesn't already exist.
				if (ScratchArea.ScratchFiles.GetFile(file.Directory.FullPath + newName) != null)
				{
					throw new GorgonException(GorgonResult.AccessDenied,
					                          string.Format(APIResources.GOREDIT_ERR_FILE_ALREADY_EXISTS,
					                                        APIResources.GOREDIT_TEXT_FILE.ToLower(CultureInfo.CurrentUICulture),
					                                        newName));
				}
			}

			// Write the content out to the scratch file system.
			using (var contentStream = file.OpenStream(true))
			{
				Current.Persist(contentStream);
			}

			if ((Current.Dependencies.Count == 0)
				&& (EditorMetaDataFile.Dependencies.ContainsKey(file.FullPath)))
			{
				EditorMetaDataFile.Dependencies.Remove(file.FullPath);
				EditorMetaDataFile.Save();
			}
			else
			{
				if (Current.Dependencies.Count > 0)
				{
					EditorMetaDataFile.Dependencies[file.FullPath] = Current.Dependencies;
					EditorMetaDataFile.Save();
				}
			}

			if ((!string.IsNullOrWhiteSpace(newName))
				&& (!Current.HasOwner))
			{
				// Rename the content.
				ContentRenamed(newName, true);
			}

			_contentChanged = false;

			// Indicate that we've saved the content.
			if (ContentSaved != null)
			{
				ContentSaved();
			}
	    }

	    /// <summary>
        /// Function used to initialize the file types for the content types.
        /// </summary>
        public static void InitializeContentFileTypes()
        {
            // Get content extensions.
            foreach (var contentPlugIn in PlugIns.ContentPlugIns)
            {
                if (contentPlugIn.Value.FileExtensions.Count == 0)
                {
                    continue;
                }

				string plugInType = contentPlugIn.Value.GetType().FullName;

				if (!_availablePlugIns.Contains(plugInType))
				{
					_availablePlugIns.Add(plugInType);
				}

                // Associate the content file type with the plug-in.
                foreach (var extension in contentPlugIn.Value.FileExtensions)
                {
                    _contentFiles[extension] = contentPlugIn.Value;
                }
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes the <see cref="ContentManagement"/> class.
        /// </summary>
        static ContentManagement()
        {
            _contentFiles = new Dictionary<GorgonFileExtension, ContentPlugIn>();
			_availablePlugIns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
        #endregion
    }
}
