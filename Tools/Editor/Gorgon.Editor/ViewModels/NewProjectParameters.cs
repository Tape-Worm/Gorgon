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
// Created: September 17, 2018 8:44:42 AM
// 
#endregion

using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// Parameters for the <see cref="INewProject"/> view model.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="NewProjectParameters"/> class.
/// </remarks>
/// <param name="hostServices">The services from the host application.</param>
/// <param name="viewModelFactory">The view model factory for creating view models.</param>
/// <param name="settings">The settings for the editor.</param>
/// <param name="messageDisplay">The message display service to use.</param>
/// <param name="busyService">The busy state service to use.</param>
internal class NewProjectParameters(IHostContentServices hostServices, ViewModelFactory viewModelFactory)
        : ViewModelCommonParameters(hostServices, viewModelFactory)
{
    /// <summary>
    /// Property to set or return the directory locator service.
    /// </summary>
    public IDirectoryLocateService DirectoryLocator
    {
        get;
        set;
    }
}
