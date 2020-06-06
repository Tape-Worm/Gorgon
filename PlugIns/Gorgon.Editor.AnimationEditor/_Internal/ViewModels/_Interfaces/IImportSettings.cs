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
// Created: April 24, 2019 11:12:27 AM
// 
#endregion

using System.Collections.ObjectModel;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.AnimationEditor
{
    /// <summary>
    /// The view model for the importer plug in settings.
    /// </summary>
    internal interface IImportSettings
        : ISettingsCategory
    {
        /// <summary>
        /// Property to return the list of selected codecs.
        /// </summary>
        ObservableCollection<CodecSetting> SelectedCodecs
        {
            get;
        }

        /// <summary>
        /// Propery to return the paths to the codec plug ins.
        /// </summary>
        ObservableCollection<CodecSetting> CodecPlugInPaths
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

        /// <summary>
        /// Property to return the command for loading a plug in assembly.
        /// </summary>
        IEditorCommand<object> LoadPlugInAssemblyCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to unloading a plug in assembly.
        /// </summary>
        IEditorCommand<object> UnloadPlugInAssembliesCommand
        {
            get;
        }
    }
}
