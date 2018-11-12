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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Plugins;

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
        /// <summary>Property to return the type of this plug in.</summary>
        public override PluginType PluginType => PluginType.Content;

        /// <summary>
        /// Property to return whether or not the plugin is capable of creating content.
        /// </summary>
        public abstract bool CanCreateContent
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to provide initialization for the plugin.
        /// </summary>
        /// <param name="pluginService">The plugin service used to access other plugins.</param>
        /// <param name="log">The logging interface for debug messages.</param>
        /// <remarks>
        /// <para>
        /// This method is only called when the plugin is loaded at startup.
        /// </para>
        /// </remarks>
        protected virtual void OnInitialize(IContentPluginService pluginService, IGorgonLog log)
        {
        }

        /// <summary>
        /// Function to provide clean up for the plugin.
        /// </summary>
        /// <param name="log">The logging interface for debug messages.</param>
        protected virtual void OnShutdown(IGorgonLog log)
        {

        }

        /// <summary>
        /// Function to open a content object from this plugin.
        /// </summary>
        /// <param name="file">The file that contains the content.</param>
        /// <param name="injector">Parameters for injecting dependency objects.</param>
        /// <param name="log">The logging interface to use.</param>
        /// <returns>A new <see cref="IEditorContent"/> object.</returns>
        protected abstract Task<IEditorContent> OnOpenContentAsync(IContentFile file, IViewModelInjection injector, IGorgonLog log);

        /// <summary>
        /// Function to open a content object from this plugin.
        /// </summary>        
        /// <param name="file">The file that contains the content.</param>
        /// <param name="injector">Parameters for injecting dependency objects.</param>
        /// <param name="log">The logging interface to use.</param>
        /// <returns>A new <see cref="IEditorContent"/> object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/>, or the <paramref name="injector"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <see cref="OnOpenContentAsync"/> method returns <b>null</b>.</exception>
        public async Task<IEditorContent> OpenContentAsync(IContentFile file, IViewModelInjection injector, IGorgonLog log)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (injector == null)
            {
                throw new ArgumentNullException(nameof(injector));
            }

            IEditorContent content = await OnOpenContentAsync(file, injector, log ?? GorgonLog.NullLog);

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
        /// <param name="log">The logging interface for debug messages.</param>
        public void Shutdown(IGorgonLog log)
        {
            int initalizedFlag = Interlocked.Exchange(ref _initialized, 0);

            if (initalizedFlag == 0)
            {
                return;
            }

            OnShutdown(log ?? GorgonLog.NullLog);
        }

        /// <summary>
        /// Function to perform any required initialization for the plugin.
        /// </summary>
        /// <param name="pluginService">The plugin service used to access other plugins.</param>                
        /// <param name="log">The logging interface for debug messages.</param>
        /// <remarks>
        /// <para>
        /// This method is only called when the plugin is loaded at startup.
        /// </para>
        /// </remarks>
        public void Initialize(IContentPluginService pluginService, IGorgonLog log)
        {
            if (Interlocked.Exchange(ref _initialized, 1) == 1)
            {
                return;
            }

            if (log == null)
            {
                log = GorgonLog.NullLog;
            }

            log.Print($"Initializing {Name}...", LoggingLevel.Simple);

            OnInitialize(pluginService, log);
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
