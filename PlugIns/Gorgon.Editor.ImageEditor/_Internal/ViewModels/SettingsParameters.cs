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
// Created: April 20, 2019 5:20:34 PM
// 
#endregion

using System;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI.ViewModels;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// Parameters to pass to the <see cref="ISettings"/> view model.
    /// </summary>
    internal class SettingsParameters
        : ViewModelInjection
    {
        #region Properties.
        /// <summary>
        /// Property to return the settings for the image editor plugin.
        /// </summary>
        public ImageEditorSettings Settings
        {
            get;
        }

		/// <summary>
        /// Property to return the content plug in service.
        /// </summary>
		public IContentPluginService PluginService
        {
            get;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ImageContentVmParameters class.</summary>
        /// <param name="settings">The settings for the image editor.</param>
        /// <param name="pluginService">The service used to manage content and importer plug ins.</param>
        /// <param name="commonServices">Common application services.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        public SettingsParameters(ImageEditorSettings settings, IContentPluginService pluginService, IViewModelInjection commonServices)
			: base(commonServices)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            PluginService = pluginService ?? throw new ArgumentNullException(nameof(pluginService));
        }
        #endregion
    }
}
