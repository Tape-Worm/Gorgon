#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: January 22, 2020 10:44:21 PM
// 
#endregion

using Gorgon.Diagnostics;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.PlugIns
{
    /// <summary>
    /// Services from the host application for content plug ins.
    /// </summary>
    internal class HostContentServices
        : IHostContentServices
    {
        /// <summary>Property to set or return the service that allows a content plug in to access other content plug ins.</summary>
        public IContentPlugInService ContentPlugInService
        {
            get;
            set;
        }

        /// <summary>Property to set or return the service that allows a conetnt plug in to access tool content plug ins.</summary>
        public IToolPlugInService ToolPlugInService
        {
            get;
            set;
        }

        /// <summary>Property to set or return the service used to browse through directories on the file system.</summary>
        public IFileSystemFolderBrowseService FolderBrowser
        {
            get;
            set;
        }

        /// <summary>Property to set or return the log for debug messages.</summary>
        public IGorgonLog Log
        {
            get;
            set;
        }

        /// <summary>Property to set or return the serivce used to show busy states.</summary>
        public IBusyStateService BusyService
        {
            get;
            set;
        }

        /// <summary>Property to set or return the service used to show message dialogs.</summary>
        public IMessageDisplayService MessageDisplay
        {
            get;
            set;
        }

        /// <summary>Property to set or return the graphics and the 2D renderer used by the host application.</summary>
        public IGraphicsContext GraphicsContext
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the service used to send and retrieve data to and from the clipboard.
        /// </summary>
        public IClipboardService ClipboardService
        {
            get;
            set;
        }

        /// <summary>Property to set or return the service used to pick colors.</summary>
        public IColorPickerService ColorPicker
        {
            get;
            set;
        }
    }
}
