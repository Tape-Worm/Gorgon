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
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.UI.ViewModels;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// Common injection parameters for all view models.
    /// </summary>
    internal class ViewModelCommonParameters
        : IViewModelInjection<IHostContentServices>
    {
        /// <summary>
        /// Property to set or return the current project.
        /// </summary>
        public IProject Project
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the project manager.
        /// </summary>
        public ProjectManager ProjectManager
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the settings for the application.
        /// </summary>
        public Editor.EditorSettings EditorSettings
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

        /// <summary>Property to return the services passed from host application.</summary>
        public IHostContentServices HostServices
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelCommonParameters"/> class.
        /// </summary>
        /// <param name="hostServices">The services from the host application.</param>
        /// <param name="viewModelFactory">The view model factory for creating view models.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="hostServices"/>, or the <paramref name="viewModelFactory"/> parameter is <b>null</b>.</exception>
        public ViewModelCommonParameters(IHostContentServices hostServices, ViewModelFactory viewModelFactory)
        {
            HostServices = hostServices ?? throw new ArgumentNullException(nameof(hostServices));
            ViewModelFactory = viewModelFactory ?? throw new ArgumentNullException(nameof(viewModelFactory));
        }
    }
}
