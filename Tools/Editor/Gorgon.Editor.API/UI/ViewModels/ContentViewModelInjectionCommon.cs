﻿#region MIT
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
// Created: November 10, 2018 11:07:56 PM
// 
#endregion

using System;
using Gorgon.Editor.Content;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// Common content view model parameters.
    /// </summary>
    public class ContentViewModelInjectionCommon
        : ViewModelInjectionCommon, IContentViewModelInjection
    {
        /// <summary>Property to return the content file.</summary>
        public IContentFile File
        {
            get;            
        }

        /// <summary>Initializes a new instance of the ContentViewModelInjectionCommon class.</summary>
        /// <param name="file">The file that contains the content.</param>
        /// <param name="messageService">The message display service.</param>
        /// <param name="busyService">The busy state service.</param>
        /// <exception cref="ArgumentNullException">Thrown any of the parameters are <b>null</b></exception>
        public ContentViewModelInjectionCommon(IContentFile file, IMessageDisplayService messageService, IBusyStateService busyService)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));
            MessageDisplay = messageService ?? throw new ArgumentNullException(nameof(messageService));
            BusyService = busyService ?? throw new ArgumentNullException(nameof(busyService));
        }
    }
}
