#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: April 18, 2019 8:48:54 AM
// 
#endregion

using System;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.PlugIns
{
    /// <summary>
    /// Base class for a plug in that provides basic utility functionality.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tool plug ins, unlike content plug ins, are independent plug ins that provide their own UI (if necessary) that do various jobs in the editor. They will be placed into a tab on the ribbon 
    /// called "Tools" and provide global functionality across the editor and don't necessarily have to restrict themselves to one type of content.
    /// </para>
    /// <para>
    /// A typical use of a tool plug in would be to display a dialog that allows mass actions on content files.  For example, a dialog that sequentially renames a list of content files so that they're 
    /// postfixed with "original_name (number)" would be a use case for a tool plug in.
    /// </para>
    /// <para>
    /// Because these plug ins are independent, there is no interaction with the editor UI beyond providing information to display a button on the ribbon.
    /// </para>
    /// </remarks>
    public abstract class ToolPlugIn
		: EditorPlugIn
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

		/// <summary>
        /// Property to return the button to display on the ribbon.
        /// </summary>
		public abstract IToolPlugInRibbonButton Button
        {
            get;
        }

        /// <summary>Property to return the type of this plug in.</summary>
        public sealed override PlugInType PlugInType => PlugInType.Tool;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to provide initialization for the plugin.
        /// </summary>
        /// <param name="pluginService">The plug in service for tool plug ins.</param>
        /// <remarks>
        /// <para>
        /// This method is only called when the plugin is loaded at startup.
        /// </para>
        /// </remarks>
        protected virtual void OnInitialize(IToolPlugInService pluginService)
        {
        }

        /// <summary>
        /// Function to provide clean up for the plugin.
        /// </summary>
        protected virtual void OnShutdown()
        {
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
        /// <param name="log">The debug log used by the application.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginService"/>, or the <paramref name="graphicsContext"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This method is only called when the plugin is loaded at startup.
        /// </para>
        /// </remarks>
        public void Initialize(IToolPlugInService pluginService, IGraphicsContext graphicsContext, IGorgonLog log)
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
        /// <summary>Initializes a new instance of the <see cref="ToolPlugIn"/> class.</summary>
        /// <param name="description">Optional description of the plugin.</param>
        protected ToolPlugIn(string description)
			: base(description)
        {

        }
		#endregion

    }
}
