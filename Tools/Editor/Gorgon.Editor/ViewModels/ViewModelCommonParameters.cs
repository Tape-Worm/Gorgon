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
// Created: September 17, 2018 8:10:15 AM
// 
#endregion

using System;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// Common injection parameters for all view models.
    /// </summary>
    internal abstract class ViewModelCommonParameters
        : IViewModelInjection
    {
        /// <summary>
        /// Property to set or return the serivce used to show busy states.
        /// </summary>
        public IBusyStateService BusyState
        {
            get;
        }

        /// <summary>
        /// Property to set or return the service used to show message dialogs.
        /// </summary>
        public IMessageDisplayService MessageDisplay
        {
            get;
        }

        /// <summary>
        /// Property to set or return the current project.
        /// </summary>
        public IProject Project
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the view model factory.
        /// </summary>
        public ViewModelFactory ViewModelFactory
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelCommonParameters"/> class.
        /// </summary>
        /// <param name="viewModelFactory">The view model factory for creating view models.</param>
        /// <param name="messageDisplay">The message display service to use.</param>
        /// <param name="busyService">The busy state service to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        public ViewModelCommonParameters(ViewModelFactory viewModelFactory, IMessageDisplayService messageDisplay, IBusyStateService busyService)
        {
            ViewModelFactory = viewModelFactory ?? throw new ArgumentNullException(nameof(viewModelFactory));
            MessageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
            BusyState = busyService ?? throw new ArgumentNullException(nameof(busyService));
        }
    }
}
