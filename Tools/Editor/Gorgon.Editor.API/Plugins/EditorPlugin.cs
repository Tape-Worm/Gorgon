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
// Created: October 12, 2018 12:48:16 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.ViewModels;
using Gorgon.Plugins;

namespace Gorgon.Editor.Plugins
{
    /// <summary>
    /// The type of plug in.
    /// </summary>
    public enum PluginType
    {
		/// <summary>
        /// Plug in type is not known.
        /// </summary>
		Unknown = 0,
        /// <summary>
        /// Plug in is used to write out editor files.
        /// </summary>
        Writer = 1,
        /// <summary>
        /// Plug in is used to build content.
        /// </summary>
        Content = 2,
		/// <summary>
        /// Plug in is a pack file reader.
        /// </summary>
		Reader = 3,
        /// <summary>
        /// Plug in is used for a utility.
        /// </summary>
        Tool = 4,
		/// <summary>
        /// Plug in is used to import content.
        /// </summary>
		ContentImporter = 5
    }

    /// <summary>
    /// The base class for an editor plug in.
    /// </summary>
    public abstract class EditorPlugin
        : GorgonPlugin
    {
        #region Variables.
        // Empty string array for IsPluginAvailable.
        private readonly IReadOnlyList<string> _defaultPluginAvailablity = Array.Empty<string>();
        #endregion

        #region Properties.
		/// <summary>
        /// Property to return the common services for the application.
        /// </summary>
		protected IViewModelInjection CommonServices
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the type of this plug in.
        /// </summary>
        public abstract PluginType PluginType
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the settings interface for this plug in.
        /// </summary>
        /// <returns>The settings interface view model.</returns>
        /// <remarks>
        /// <para>
        /// Implementors who wish to supply customizable settings for their plug ins from the main "Settings" area in the application can override this method and return a new view model based on 
        /// the base <see cref="ISettingsCategoryViewModel"/> type. Returning <b>null</b> will mean that the plug in does not have settings that can be managed externally.
        /// </para>
        /// <para>
        /// Plug ins must register the view associated with their settings panel via the <see cref="ViewFactory.Register{T}(Func{System.Windows.Forms.Control})"/> method when the plug in first loaded, 
        /// or else the panel will not show in the main settings area.
        /// </para>
        /// </remarks>
        protected virtual ISettingsCategoryViewModel OnGetSettings() => null;

        /// <summary>
        /// Function to determine if this plug in is usable or not.
        /// </summary>
        /// <returns>A list of string values containing information about why this plug in may not be usable at this time.</returns>
        /// <remarks>
        /// <para>
        /// Plug in developers should override this method if the plug in has dependencies, and those dependencies cannot be located. This can also be used by a developer to disable the plug in should it 
        /// be necessary to do so.
        /// </para>
        /// <para>
        /// This method should never return <b>null</b>, only an empty array if no problems are present.
        /// </para>
        /// </remarks>
        protected virtual IReadOnlyList<string> OnGetPluginAvailability() => _defaultPluginAvailablity;


        /// <summary>
        /// Function to retrieve the settings interface for the plug in.
        /// </summary>
        /// <returns>The base settings view model.</returns>
        /// <remarks>
        /// <para>
        /// This will return a view model that can be used to modify plug in settings on the main settings area in the application. If this method returns <b>null</b>, then the settings will not show in 
        /// the main settings area of the application.
        /// </para>
        /// </remarks>
        public ISettingsCategoryViewModel GetPluginSettings() => OnGetSettings();

        /// <summary>
        /// Function to determine if this plug in is usable or not.
        /// </summary>
        /// <returns>A list of string values containing information about why this plug in may not be usable at this time, or an empty array if the plug in can be used without issue.</returns>
        /// <remarks>
        /// <para>
        /// Use this to determine if the plug in is able to be used in the application. If it is not available, it may be that a dependency is broken, or some other issue is keeping the plug in from 
        /// working correctly.
        /// </para>
        /// </remarks>
        public IReadOnlyList<string> IsPluginAvailable() => OnGetPluginAvailability();

		/// <summary>
        /// Function to assign the application common services to the plug in.
        /// </summary>
        /// <param name="commonServices">The common services to pass to the plug in.</param>
        public void AssignCommonServices(IViewModelInjection commonServices) => CommonServices = commonServices;
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPlugin"/> class.
        /// </summary>
        /// <param name="description">Optional description of the plugin.</param>
        protected EditorPlugin(string description)
            : base(description)
        {
        }
        #endregion
    }
}
