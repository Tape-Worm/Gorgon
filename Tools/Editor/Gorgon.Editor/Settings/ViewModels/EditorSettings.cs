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
using System.Collections.ObjectModel;
using System.Linq;
using Gorgon.Editor.Properties;
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
        public ISettingsPlugInsList PlugInsList
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the command to execute to select a category.
        /// </summary>
        public IEditorCommand<string> SetCategoryCommand
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to change the current category.
        /// </summary>
        /// <param name="id">The ID of the current category.</param>
        private void DoSetCategory(string id)
        {
            try
            {
                if (Categories.Count == 0)
                {
                    return;
                }

                if (!Guid.TryParse(id, out Guid guid))
                {
                    CurrentCategory = Categories[0];
                    return;
                }

                CurrentCategory = Categories.FirstOrDefault(item => guid == item.ID) ?? Categories[0];
            }
            catch (Exception ex)
            {
                _messageDisplay.ShowError(ex, Resources.GOREDIT_ERR_SELECT_SETTING_CATEGORY);
            }
        }

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
            PlugInsList = injectionParameters.PlugInsList ?? throw new ArgumentMissingException(nameof(injectionParameters.PlugInsList), nameof(injectionParameters));
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ViewModels.EditorSettingsVm"/> class.</summary>
        public EditorSettingsVm() => SetCategoryCommand = new EditorCommand<string>(DoSetCategory);
        #endregion
    }
}
