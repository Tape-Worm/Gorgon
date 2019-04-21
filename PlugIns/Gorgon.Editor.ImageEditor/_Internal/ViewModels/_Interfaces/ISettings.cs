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
// Created: April 20, 2019 2:21:44 PM
// 
#endregion

using System.Collections.ObjectModel;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ImageEditor
{
	/// <summary>
    /// Settings view model for image codecs.
    /// </summary>
    internal interface ISettings
		: ISettingsCategoryViewModel
    {
        /// <summary>
        /// Property to set or return the to the directory that was last used for importing/exporting.
        /// </summary>
        string LastImportExportPath
        {
            get;
            set;
        }

        /// <summary>
        /// Propery to return the paths to the codec plug ins.
        /// </summary>
        ObservableCollection<(string name, string path)> CodecPluginPaths
        {
            get;            
        }

		/// <summary>
        /// Property to return the command for writing setting data.
        /// </summary>
		IEditorCommand<object> WriteSettingsCommand
        {
            get;
        }
    }
}
