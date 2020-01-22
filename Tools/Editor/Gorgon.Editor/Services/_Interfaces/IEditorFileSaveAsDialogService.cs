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
// Created: September 24, 2018 1:02:38 PM
// 
#endregion

using Gorgon.Editor.PlugIns;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// Provides a service for showing a dialog used to save editor files.
    /// </summary>
    internal interface IEditorFileSaveAsDialogService
        : IFileDialogService
    {
        /// <summary>
        /// Property to return the available file system providers.
        /// </summary>
        IFileSystemProviders Providers
        {
            get;
        }

        /// <summary>
        /// Property to return the settings for the application.
        /// </summary>
        EditorSettings Settings
        {
            get;
        }

        /// <summary>
        /// Property to set or return the currently active file writer plugin.
        /// </summary>
        OLDE_FileWriterPlugIn CurrentWriter
        {
            get;
            set;
        }
    }
}
