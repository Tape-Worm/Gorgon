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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Gorgon.Editor.Properties;
using Gorgon.Graphics;
using Gorgon.IO;

namespace Gorgon.Editor
{
    /// <summary>
    /// Interface for content management.
    /// </summary>
    static class ContentManagement
    {
        #region Variables.
	    private static ContentObject _currentContentObject;
	    private static Type _defaultContentType;
        #endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the application graphics interface.
		/// </summary>
	    public static GorgonGraphics Graphics
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the current content file.
		/// </summary>
	    public static GorgonFileSystemFileEntry ContentFile
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
		/// Property to set or return the method to call when content is saved.
		/// </summary>
	    public static Action ContentSaved
		{
			get;
			set;
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
		/// <remarks>This only takes effect if properties are available for public use on the content.  The parameter for the method is as follows: <c>string newName</c>.</remarks>
	    public static Action<string> ContentRenamed
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
		/// Property to set or return the method to call when a property has been changed on the content.
		/// </summary>
	    public static Action ContentPropertyChanged
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
		/// Function to close the current content.
		/// </summary>
		/// <param name="content">The content that's asking to be closed.</param>
		/// <remarks>This will close the current content without asking to save, ensure that data is saved before calling this method.</remarks>
	    private static void CloseCurrentContent(ContentObject content)
	    {
		    if ((Current == null)
				|| (Current != content))
		    {
			    return;
		    }

			LoadDefaultContentPane();
	    }

		/// <summary>
		/// Function to rename the current content.
		/// </summary>
		/// <param name="content">The content that's being renamed.</param>
		/// <param name="newName">The new name for the content.</param>
	    private static void RenameCurrentContent(ContentObject content, string newName)
	    {
			if ((ContentRenamed == null)
				|| (Current == null)
				|| (Current != content))
			{
				return;
			}

			ContentRenamed(newName);
	    }

		/// <summary>
		/// Function to refresh a property for the content.
		/// </summary>
		/// <param name="content">Content that owns the property being refreshed.</param>
	    private static void ContentPropertyRefreshed(ContentObject content)
	    {

			if ((ContentPropertyStateChanged == null)
				|| (Current == null)
				|| (Current != content))
			{
				return;
			}

			ContentPropertyStateChanged();
	    }

		/// <summary>
		/// Function called during idle time for rendering.
		/// </summary>
		/// <returns><c>true</c> to continue rendering, <c>false</c> to stop.</returns>
	    private static bool IdleLoop()
	    {
		    if (_currentContentObject != null)
		    {
			    _currentContentObject.Draw();
		    }
		    return true;
	    }

		/// <summary>
		/// Function to load the content and its related UI.
		/// </summary>
		/// <param name="contentObject">Content to load.</param>
		/// <param name="isDefault"><c>true</c> if this is the default content, <c>false</c> if not.</param>
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

			// Force focus to the content window.
			if (contentWindow.Parent != null)
			{
				contentWindow.Focus();
			}

			if (contentObject.HasRenderer)
			{
				GorgonApplication.ApplicationIdleLoopMethod = IdleLoop;
			}
	    }

		/// <summary>
		/// Function to create a content object instance.
		/// </summary>
		/// <param name="plugIn">The plug-in to use when creating the content.</param>
		/// <param name="settings">Settings to pass to the content.</param>
		/// <param name="editorFile">Editor file that holds the content.</param>
		/// <param name="recordDefaults"><c>true</c> to record the default property values for the content, <c>false</c> to treat as new property values.</param>
		/// <returns>The new content object.</returns>
	    private static ContentObject CreateContentObjectInstance(ContentPlugIn plugIn, ContentSettings settings, EditorFile editorFile, bool recordDefaults)
	    {
			ContentObject content = plugIn.CreateContentObject(settings);

			content.OnCloseCurrent = CloseCurrentContent;
			content.OnRenameContent = RenameCurrentContent;
			content.OnPropertyRefreshed = ContentPropertyRefreshed;
			content.OnChanged = ContentPropertyChanged;
			content.OnReload = contentItem =>
			                   {
				                   if ((Current == null)
				                       || (contentItem != Current))
				                   {
					                   return;
				                   }
								   
								   Load(editorFile, ContentFile, plugIn, true);
			                   };
			content.OnCommit = contentItem =>
			                   {
				                   if ((Current == null)
				                       || (contentItem != Current))
				                   {
					                   return;
				                   }

								   // Save the content and its metadata.
								   Save();
			                   };

			if ((content.HasProperties)
				&& (recordDefaults))
			{
				content.SetDefaults();
			}

			content.ImageEditor = plugIn.GetRegisteredImageEditor();

			if ((content.ImageEditor != null)
				&& (string.IsNullOrWhiteSpace(PlugIns.DefaultImageEditorPlugIn)))
			{
				PlugIns.DefaultImageEditorPlugIn = content.ImageEditor.Name;
			}

			content.EditorFile = editorFile;

			if (content.EditorFile != null)
			{
				// Indicate that this content is linked to another piece of content.
				content.HasOwner = EditorMetaDataFile.HasFileLinks(editorFile);
			}

			return content;
	    }

		/// <summary>
		/// Function to include a file in the editor.
		/// </summary>
		/// <param name="file">File entry to include.</param>
		/// <returns>An editor file object linking the file to the the system.</returns>
		public static EditorFile IncludeItem(GorgonFileSystemFileEntry file)
		{
			ContentPlugIn plugIn = GetContentPlugInForFile(file.Extension);

			string plugInType = string.Empty;

			if (plugIn != null)
			{
				plugInType = plugIn.GetType().FullName;
			}


			var fileItem = new EditorFile(file.FullPath)
			{
				PlugInType = plugInType
			};

			if (plugIn != null)
			{
				using (Stream fileStream = file.OpenStream(false))
				{
					plugIn.GetEditorFileAttributes(fileStream, fileItem.Attributes);
				}
			}

			EditorMetaDataFile.Files[fileItem.FilePath] = fileItem;

			return fileItem;
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
			return PlugIns.ContentPlugIns.Where(item => item.Value.FileExtensions.Count > 0).SelectMany(item => item.Value.FileExtensions);
		}

		/// <summary>
		/// Function to return a related plug-in for the given editor file.
		/// </summary>
		/// <param name="file">Editor file to evaluate.</param>
		/// <returns>The plug-in for the editor file.</returns>
	    public static ContentPlugIn GetContentPlugInForFile(EditorFile file)
	    {
			if ((file == null)
				|| (string.IsNullOrWhiteSpace(file.PlugInType)))
			{
				return null;
			}

			ContentPlugIn plugIn;

			PlugIns.ContentPlugIns.TryGetValue(file.PlugInType, out plugIn);

			return plugIn;
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

	        return (from plugInItem in PlugIns.ContentPlugIns
	                where plugInItem.Value.FileExtensions.Count > 0
	                      && plugInItem.Value.FileExtensions.Contains(new GorgonFileExtension(fileExtension))
	                select plugInItem.Value).FirstOrDefault();
        }

		/// <summary>
		/// Function to determine if a file can be opened for viewing/editing.
		/// </summary>
		/// <param name="file">File to evaluate.</param>
		/// <returns><c>true</c> if the file can be opened, <c>false</c> if not.</returns>
	    public static bool CanOpenContent(EditorFile file)
	    {
			if (file == null)
			{
				return false;
			}

			// Check to see if the plug-in exists.
			return !string.IsNullOrWhiteSpace(file.PlugInType) && PlugIns.ContentPlugIns.ContainsKey(file.PlugInType);
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
			if (Current != null)
			{
				Current.OnCloseCurrent = null;
				Current.OnRenameContent = null;
				Current.OnPropertyRefreshed = null;
				Current.OnCommit = null;
				Current.OnChanged = null;
				Current.OnReload = null;
			}

			Current = null;

			// Turn off any idle time activity during the load.
			GorgonApplication.ApplicationIdleLoopMethod = null;

			if (ContentPanelUnloadAction != null)
			{
				ContentPanelUnloadAction();
			}

			// Close the content object.  This should preserve any changes.
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

			ContentFile = null;

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

	        return CreateContentObjectInstance(plugIn, settings, null, true);
        }

		/// <summary>
		/// Function to load dependencies for a file.
		/// </summary>
		/// <param name="content">The content that is being loaded.</param>
		/// <param name="file">The file that contains the dependencies.</param>
		/// <param name="missing">A list of dependencies that are missing.</param>
	    private static void LoadDependencies(ContentObject content, EditorFile file, ICollection<string> missing)
	    {
			foreach (Dependency dependencyFile in file.DependsOn)
			{
				GorgonFileSystemFileEntry externalFile = OnGetDependency(dependencyFile.EditorFile.FilePath);

				if (externalFile == null)
				{
					throw new FileNotFoundException(string.Format(APIResources.GOREDIT_ERR_CANNOT_FIND_DEPENDENCY_FILE,
																				dependencyFile.EditorFile.FilePath));
				}

				// If the dependency file contains any dependencies, then we need to load it them prior to loading the actual dependency.
				if (dependencyFile.EditorFile.DependsOn.Count > 0)
				{
					LoadDependencies(content, dependencyFile.EditorFile, missing);
				}

				// Read the external content.
				using (Stream dependencyStream = externalFile.OpenStream(false))
				{
					DependencyLoadResult result = content.LoadDependencyFile(dependencyFile, dependencyStream);
					switch (result.State)
					{
						case DependencyLoadState.FatalError:
							throw new GorgonException(GorgonResult.CannotRead,
							                          string.Format(APIResources.GOREDIT_ERR_CANNOT_LOAD_DEPENDENCY, dependencyFile.EditorFile.FilePath, result.Message));
						case DependencyLoadState.ErrorContinue:
							missing.Add(string.Format(APIResources.GOREDIT_DLG_CANNOT_LOAD_DEPENDENCY, dependencyFile.EditorFile.FilePath, result.Message));
							break;
					}
				}
			}
	    }

		/// <summary>
		/// Function to load content data from the file system.
		/// </summary>
		/// <param name="editorFile">The editor file data.</param>
		/// <param name="file">The file system file that contains the content data.</param>
		/// <param name="plugIn">The plug-in used to open the file.</param>
		/// <param name="reload"><c>true</c> to just reload the file, <c>false</c> to do a complete load of the content.</param>
	    public static void Load(EditorFile editorFile, GorgonFileSystemFileEntry file, ContentPlugIn plugIn, bool reload = false)
		{
			ContentObject content;

		    if (file == null)
		    {
		        throw new ArgumentNullException("file");
		    }

			if (!reload)
			{
				ContentSettings settings = plugIn.GetContentSettings(); // Get default settings.

				if (settings != null)
				{
					// Assign the name from the file.
					settings.Name = file.Name;
					settings.CreateContent = false;
				}

				content = CreateContentObjectInstance(plugIn, settings, editorFile, false);

				Debug.Assert(_currentContentObject != null, "Content should not be NULL!");
			}
			else
			{
				content = Current;
			}

			// Load the content dependencies if any exist.
			// Check for dependencies.
			if ((editorFile != null)
				&& (editorFile.DependsOn.Count > 0))
			{
				var missingDependencies = new List<string>();

				try
				{
					LoadDependencies(content, content.EditorFile, missingDependencies);
				}
				catch
				{
					content.Dependencies.Clear();
					throw;
				}

				if ((missingDependencies.Count > 0)
					&& (DependencyNotFound != null))
				{
					DependencyNotFound(file.Name, missingDependencies);
				}
			}

			if (!reload)
			{
				LoadContentPane(content);
			}

			// Load in the content data.
		    using(Stream stream = file.OpenStream(false))
		    {
		        content.Read(stream);
		    }

			ContentFile = file;

			content.OnContentReady();
	    }

		/// <summary>
		/// Function to save the current content.
		/// </summary>
		/// <param name="persistMetaData">[Optional] <c>true</c> to persist the meta data for the file, <c>false</c> to leave it.</param>
	    public static void Save(bool persistMetaData = true)
	    {
		    if ((Current == null)
				|| (ContentFile == null))
		    {
			    return;
		    }
			
			// Write the content out to the scratch file system.
			using (var contentStream = ContentFile.OpenStream(true))
			{
				Current.Persist(contentStream);
			}

			// Save any metadata.
			if (!persistMetaData)
			{
				return;
			}

			EditorMetaDataFile.Save();

			if (ContentSaved == null)
			{
				return;
			}

			ContentSaved();
	    }
        #endregion
    }
}
