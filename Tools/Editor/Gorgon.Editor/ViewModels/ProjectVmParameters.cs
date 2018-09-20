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
// Created: September 17, 2018 8:50:22 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// Parameters for the <see cref="IProjectVm"/> view model.
    /// </summary>
    internal class ProjectVmParameters
        : ViewModelCommonParameters
    {
        /// <summary>
        /// Property to set or return the metadata manager for the project.
        /// </summary>
        public IMetadataManager MetadataManager
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectVmParameters"/> class.
        /// </summary>
        /// <param name="viewModelFactory">The view model factory for creating view models.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        public ProjectVmParameters(IProject project, IMetadataManager metadataManager, ViewModelFactory viewModelFactory, IMessageDisplayService messageDisplay, IBusyStateService busyService)
            : base(viewModelFactory, messageDisplay, busyService)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
            MetadataManager = metadataManager ?? throw new ArgumentNullException(nameof(metadataManager));
        }
    }
}
