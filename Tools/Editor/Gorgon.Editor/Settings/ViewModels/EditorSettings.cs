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
// Created: April 20, 2019 10:22:08 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The view model for the editor settings panel.
    /// </summary>
    internal class EditorSettingsVm
        : ViewModelBase<EditorSettingsParameters>, IEditorSettingsVm
    {
        #region Variables.
		// The message display service.
        private IMessageDisplayService _messageDisplay;
		// The currently selected category.
        private ISettingsCategoryViewModel _current;
        #endregion

        #region Properties.
        /// <summary>Property to return the current category ID being used.</summary>
        public ISettingsCategoryViewModel CurrentCategory
        {
            get => _current;
            private set
            {
                if (_current == value)
                {
                    return;
                }

                OnPropertyChanging();
                _current = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the list of categories available.</summary>
        public ObservableCollection<ISettingsCategoryViewModel> Categories
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the list of plug ins for the fixed plug in list category.
        /// </summary>
        public ISettingsPluginsList PluginsList
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <exception cref="ArgumentMissingException">MessageDisplay - injectionParameters
        /// or
        /// Categories - injectionParameters</exception>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(EditorSettingsParameters injectionParameters)
        {
            _messageDisplay = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(injectionParameters.MessageDisplay), nameof(injectionParameters));
            Categories = new ObservableCollection<ISettingsCategoryViewModel>(injectionParameters.Categories 
				?? throw new ArgumentMissingException(nameof(injectionParameters.Categories), nameof(injectionParameters)));
            PluginsList = injectionParameters.PluginsList ?? throw new ArgumentMissingException(nameof(injectionParameters.PluginsList), nameof(injectionParameters));
        }
        #endregion

        #region Constructor/Finalizer.

        #endregion
    }
}
