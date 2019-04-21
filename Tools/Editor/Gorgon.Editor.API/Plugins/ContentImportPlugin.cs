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
// Created: December 17, 2018 10:01:27 PM
// 
#endregion

using System;
using System.IO;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.IO;

namespace Gorgon.Editor.Plugins
{
    /// <summary>
    /// A plugin type performs a custom import for content.
    /// </summary>
    public abstract class ContentImportPlugin
        : EditorPlugin
    {
        #region Constants.
        /// <summary>
        /// An attribute name for the file metadata to indicate that this item was imported.
        /// </summary>
        public const string ImportOriginalFileNameAttr = "ImportOriginalName";
        #endregion

        #region Variables.
        // Flag to indicate that the plugin is initialized.
        private int _initialized;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the graphics context for the application.
        /// </summary>
        protected IGraphicsContext GraphicsContext
        {
            get;
            private set;
        }

        /// <summary>Property to return the type of this plug in.</summary>
        public override PluginType PluginType => PluginType.ContentImporter;
        #endregion

        #region Methods.
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
        /// <param name="sourceFile">The file being imported.</param>
        /// <param name="fileSystem">The file system containing the file being imported.</param>
        /// <returns>A new <see cref="IEditorContentImporter"/> object.</returns>
        protected abstract IEditorContentImporter OnCreateImporter(FileInfo sourceFile, IGorgonFileSystem fileSystem);

        /// <summary>
        /// Function to determine if the content plugin can open the specified file.
        /// </summary>
        /// <param name="file">The content file to evaluate.</param>
        /// <returns><b>true</b> if the plugin can open the file, or <b>false</b> if not.</returns>
        protected abstract bool OnCanOpenContent(FileInfo file);

        /// <summary>
        /// Function to determine if the content plugin can open the specified file.
        /// </summary>
        /// <param name="file">The content file to evaluate.</param>
        /// <returns><b>true</b> if the plugin can open the file, or <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/> parameter is <b>null</b>.</exception>
        public bool CanOpenContent(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            return OnCanOpenContent(file);
        }


        /// <summary>
        /// Function to open a content object from this plugin.
        /// </summary>        
        /// <param name="sourceFile">The file being imported.</param>
        /// <param name="fileSystem">The file system that contains the file being imported.</param>
        /// <returns>A new <see cref="IEditorContent"/> object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sourceFile"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <see cref="OnCreateImporter"/> method returns <b>null</b>.</exception>
        public IEditorContentImporter CreateImporter(FileInfo sourceFile, IGorgonFileSystem fileSystem)
        {
            if (sourceFile == null)
            {
                throw new ArgumentNullException(nameof(sourceFile));
            }

            IEditorContentImporter importer = OnCreateImporter(sourceFile, fileSystem);

            if (importer == null)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GOREDIT_ERR_NO_CONTENT_IMPORTER_FROM_PLUGIN, Name));
            }

            return importer;
        }


        /// <summary>
        /// Function to perform any required clean up for the plugin.
        /// </summary>
        public void Shutdown()
        {
            CommonServices = null;
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
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginService"/>, or the <paramref name="graphicsContext"/> parameter is <b>null</b>.</exception> 
        /// <remarks>
        /// <para>
        /// This method is only called when the plugin is loaded at startup.
        /// </para>
        /// </remarks>
        public void Initialize(IContentPluginService pluginService, IGraphicsContext graphicsContext)
        {
            if (pluginService == null)
            {
                throw new ArgumentNullException(nameof(pluginService));
            }

            if (Interlocked.Exchange(ref _initialized, 1) == 1)
            {
                return;
            }

            CommonServices.Log.Print($"Initializing {Name}...", LoggingLevel.Simple);
			            
            GraphicsContext = graphicsContext ?? throw new ArgumentNullException(nameof(graphicsContext));

            OnInitialize(pluginService);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ContentImportPlugin class.</summary>
        /// <param name="description">Optional description of the plugin.</param>
        protected ContentImportPlugin(string description)
            : base(description)
        {
        }
        #endregion
    }
}
