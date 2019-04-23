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
// Created: April 19, 2019 1:48:31 PM
// 
#endregion

using System.Collections.ObjectModel;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels
{
	/// <summary>
    /// The view model for the editor settings panel.
    /// </summary>
    internal interface IEditorSettingsVm
		: IViewModel
    {
		/// <summary>
        /// Property to return the current category ID being used.
        /// </summary>
		ISettingsCategoryViewModel CurrentCategory
        {
            get;
        }

		/// <summary>
        /// Property to return the list of categories available.
        /// </summary>
		ObservableCollection<ISettingsCategoryViewModel> Categories
        {
            get;
        }

		/// <summary>
        /// Property to return the list of plug ins for the fixed plug in list category.
        /// </summary>
		ISettingsPlugInsList PlugInsList
        {
            get;
        }

		/// <summary>
        /// Property to return the command to execute to select a category.
        /// </summary>
        IEditorCommand<string> SetCategoryCommand
        {
            get;
        }
    }
}
