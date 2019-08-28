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
// Created: April 20, 2019 9:22:57 PM
// 
#endregion

using System;
using Gorgon.Diagnostics;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.UI.ViewModels
{
    /// <summary>
    /// Common services to inject into view models across the application, including plug ins.
    /// </summary>
    public class ViewModelInjection
        : IViewModelInjection
    {
        /// <summary>
        /// Property to return the log for the application.
        /// </summary>
        public IGorgonLog Log
        {
            get;
        }

        /// <summary>
        /// Property to return the busy state service for the application.
        /// </summary>
        public IBusyStateService BusyService
        {
            get;
        }

        /// <summary>
        /// Property to return the message display service for the application.
        /// </summary>
        public IMessageDisplayService MessageDisplay
        {
            get;
        }

        /// <summary>Initializes a new instance of the <see cref="ViewModelInjection"/> class.</summary>
        /// <param name="copy">The objects to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="copy"/> parameter is <b>null</b>.</exception>
        protected ViewModelInjection(IViewModelInjection copy)
        {
            if (copy == null)
            {
                throw new ArgumentNullException(nameof(copy));
            }

            Log = copy.Log;
            BusyService = copy.BusyService;
            MessageDisplay = copy.MessageDisplay;
        }

        /// <summary>Initializes a new instance of the <see cref="ViewModelInjection"/> class.</summary>
        /// <param name="log">The log for the application.</param>
        /// <param name="busyService">The busy service for the application.</param>
        /// <param name="messageService">The message display service for the application..</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        public ViewModelInjection(IGorgonLog log, IBusyStateService busyService, IMessageDisplayService messageService)
        {
            Log = log ?? throw new ArgumentNullException(nameof(log));
            BusyService = busyService ?? throw new ArgumentNullException(nameof(busyService));
            MessageDisplay = messageService ?? throw new ArgumentNullException(nameof(messageService));
        }
    }
}
