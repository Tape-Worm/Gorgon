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
// Created: April 20, 2019 10:23:20 AM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Diagnostics;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.ViewModels;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The parameters for the <see cref="IEditorSettingsVm"/> view model.
    /// </summary>
    internal class EditorSettingsParameters
		: IViewModelInjection
    {
		/// <summary>
        /// Property to return the categories for the settings.
        /// </summary>
        public IEnumerable<ISettingsCategoryViewModel> Categories
        {
            get;
        }

        /// <summary>
        /// Property to return the list of plug ins for the fixed plug in list category.
        /// </summary>
        public ISettingsPlugInsList PlugInsList
        {
            get;
        }

        /// <summary>Property to return the logging interface for debug logging.</summary>
        public IGorgonLog Log => Program.Log;

        /// <summary>Property to return the serivce used to show busy states.</summary>
        public IBusyStateService BusyService
        {
            get;
            private set;
        }

        /// <summary>Property to return the service used to show message dialogs.</summary>
        public IMessageDisplayService MessageDisplay
        {
            get;
            private set;
        }

        /// <summary>Initializes a new instance of the <see cref="EditorSettingsParameters"/> class.</summary>
        /// <param name="factory">The view model factory.</param>
        /// <param name="categories">The list of settings categories.</param>
        /// <param name="pluginsList">The list of plug ins for the fixed plugin list category.</param>
        /// <param name="messageDisplay">The message display service.</param>
        /// <param name="busyService">The busy state service.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is <b>null</b>.</exception>
        public EditorSettingsParameters(IEnumerable<ISettingsCategoryViewModel> categories, ISettingsPlugInsList pluginsList, IMessageDisplayService messageDisplay, IBusyStateService busyService)
        {
            MessageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
            Categories = categories ?? throw new ArgumentNullException(nameof(categories));
            PlugInsList = pluginsList ?? throw new ArgumentNullException(nameof(pluginsList));
            BusyService = busyService ?? throw new ArgumentNullException(nameof(busyService));
        }
    }
}
