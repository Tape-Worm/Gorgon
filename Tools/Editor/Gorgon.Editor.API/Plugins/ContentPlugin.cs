#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: October 29, 2018 1:00:20 PM
// 
#endregion

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.IO;

namespace Gorgon.Editor.Plugins
{
    /// <summary>
    /// Defines a plug in used to generate content in the editor.
    /// </summary>
    public abstract class ContentPlugin
        : EditorPlugin
    {
        #region Variables.
        // Flag to indicate that the plugin is initialized.
        private int _initialized;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the file search service.
        /// </summary>
        protected ISearchService<IGorgonNamedObject> FileSearchService
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the graphics context for the application.
        /// </summary>
        protected IGraphicsContext GraphicsContext
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the logging interface for the application.
        /// </summary>
        protected IGorgonLog Log
        {
            get;
            private set;
        }

        /// <summary>Property to return the type of this plug in.</summary>
        public override PluginType PluginType => PluginType.Content;

        /// <summary>
        /// Property to return whether or not the plugin is capable of creating content.
        /// </summary>
        /// <remarks>
        /// When plugin authors return <b>true</b> for this property, they should also override the <see cref="GetDefaultContentAsync(string)"/> method to pre-populate the content with default data.
        /// </remarks>
        public abstract bool CanCreateContent
        {
            get;
        }

        /// <summary>
        /// Property to return the ID for the type of content produced by this plug in.
        /// </summary>
        public abstract string ContentTypeID
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to return the file system used for writing out temporary data.
        /// </summary>
        /// <param name="tempDirectory">The physical directory to store the temporary data into.</param>
        /// <returns>A new writable file system for writing temporary data into.</returns>
        private IGorgonFileSystemWriter<Stream> GetScratchArea(DirectoryInfo tempDirectory)
        {
            string scratchPath = Path.Combine(tempDirectory.FullName, GetType().FullName).FormatDirectory(Path.DirectorySeparatorChar);

            if (!Directory.Exists(scratchPath))
            {
                Directory.CreateDirectory(scratchPath);
            }

            var scratchArea = new GorgonFileSystem(Log);
            scratchArea.Mount(scratchPath);
            return new GorgonFileSystemWriter(scratchArea, scratchPath);
        }

        /// <summary>
        /// Function to provide initialization for the plugin.
        /// </summary>
        /// <param name="pluginService">The plugin service used to access other plugins.</param>
        /// <remarks>
        /// <para>
        /// This method is only called when the plugin is loaded at startup.
        /// </para>
        /// </remarks>
        protected virtual void OnInitialize(IContentPluginService pluginService)
        {
        }

        /// <summary>
        /// Function to provide clean up for the plugin.
        /// </summary>
        protected virtual void OnShutdown()
        {

        }

        /// <summary>
        /// Function to open a content object from this plugin.
        /// </summary>
        /// <param name="file">The file that contains the content.</param>
        /// <param name="fileManager">The file manager used to access other content files.</param>
        /// <param name="injector">Parameters for injecting dependency objects.</param>
        /// <param name="scratchArea">The file system for the scratch area used to write transitory information.</param>
        /// <param name="undoService">The undo service for the plug in.</param>
        /// <returns>A new <see cref="IEditorContent"/> object.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="scratchArea"/> parameter is the file system where temporary files to store transitory information for the plug in is stored. This file system is destroyed when the 
        /// application or plug in is shut down, and is not stored with the project.
        /// </para>
        /// </remarks>
        protected abstract Task<IEditorContent> OnOpenContentAsync(IContentFile file, IContentFileManager fileManager, IViewModelInjection injector, IGorgonFileSystemWriter<Stream> scratchArea, IUndoService undoService);

        /// <summary>
        /// Function to register plug in specific search keywords with the system search.
        /// </summary>
        /// <typeparam name="T">The type of object being searched, must implement <see cref="IGorgonNamedObject"/>.</typeparam>
        /// <param name="searchService">The search service to use for registration.</param>
        protected abstract void OnRegisterSearchKeywords<T>(ISearchService<T> searchService) where T : IGorgonNamedObject;

        /// <summary>
        /// Function to register plug in specific search keywords with the system search.
        /// </summary>
        /// <typeparam name="T">The type of object being searched, must implement <see cref="IGorgonNamedObject"/>.</typeparam>
        /// <param name="searchService">The search service to use for registration.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="searchService"/> parameter is <b>null</b>.</exception>
        public void RegisterSearchKeywords<T>(ISearchService<T> searchService)
            where T : IGorgonNamedObject
        {
            if (searchService == null)
            {
                throw new ArgumentNullException(nameof(searchService));
            }

            OnRegisterSearchKeywords(searchService);
        }

        /// <summary>
        /// Function to retrieve the default content for the plug in.
        /// </summary>
        /// <param name="name">The name of the content (if applicable).</param>
        /// <returns>A byte array containing the default content data.</returns>
        /// <remarks>
        /// <para>
        /// This is used to generate default content data when creating new content.
        /// </para>
        /// <para>
        /// This method will not be called if <see cref="CanCreateContent"/> is <b>false</b>.
        /// </para>
        /// </remarks>
        public virtual Task<byte[]> GetDefaultContentAsync(string name) => Task.FromResult<byte[]>(null);

        /// <summary>
        /// Function to open a content object from this plugin.
        /// </summary>        
        /// <param name="file">The file that contains the content.</param>
        /// <param name="fileManager">The file manager used to access other content files.</param>
        /// <param name="injector">Parameters for injecting dependency objects.</param>
        /// <param name="project">The project information.</param>
        /// <param name="undoService">The undo service for the plugin.</param>
        /// <returns>A new <see cref="IEditorContent"/> object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/>, <paramref name="fileManager"/>, <paramref name="injector"/>, or the <paramref name="project"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <see cref="OnOpenContentAsync"/> method returns <b>null</b>.</exception>
        public async Task<IEditorContent> OpenContentAsync(IContentFile file, IContentFileManager fileManager, IViewModelInjection injector, IProject project, IUndoService undoService)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (fileManager == null)
            {
                throw new ArgumentNullException(nameof(fileManager));
            }

            if (injector == null)
            {
                throw new ArgumentNullException(nameof(injector));
            }

            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            IGorgonFileSystemWriter<Stream> scratchWriter = GetScratchArea(project.TempDirectory);            

            IEditorContent content = await OnOpenContentAsync(file, fileManager, injector, scratchWriter, undoService);

            if (content == null)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GOREDIT_ERR_NO_CONTENT_FROM_PLUGIN, Name));
            }

            // Reset the content state.
            content.ContentState = ContentState.Unmodified;

            return content;
        }

        /// <summary>
        /// Function to perform any required clean up for the plugin.
        /// </summary>
        public void Shutdown()
        {
            int initalizedFlag = Interlocked.Exchange(ref _initialized, 0);

            if (initalizedFlag == 0)
            {
                return;
            }

            OnShutdown();
        }

        /// <summary>
        /// Function to perform any required initialization for the plugin.
        /// </summary>
        /// <param name="pluginService">The plugin service used to access other plugins.</param>                
        /// <param name="graphicsContext">The graphics context for the application.</param>
        /// <param name="log">The debug log used by the application.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginService"/>, or the <paramref name="graphicsContext"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This method is only called when the plugin is loaded at startup.
        /// </para>
        /// </remarks>
        public void Initialize(IContentPluginService pluginService, IGraphicsContext graphicsContext, IGorgonLog log)
        {
            if (pluginService == null)
            {
                throw new ArgumentNullException(nameof(pluginService));
            }

            if (Interlocked.Exchange(ref _initialized, 1) == 1)
            {
                return;
            }

            if (log == null)
            {
                log = GorgonLog.NullLog;
            }

            log.Print($"Initializing {Name}...", LoggingLevel.Simple);

            Log = log;

            GraphicsContext = graphicsContext ?? throw new ArgumentNullException(nameof(graphicsContext));

            OnInitialize(pluginService);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ContentPlugin class.</summary>
        /// <param name="description">Optional description of the plugin.</param>
        protected ContentPlugin(string description)
            : base(description)
        {            
        }
        #endregion
    }
}
