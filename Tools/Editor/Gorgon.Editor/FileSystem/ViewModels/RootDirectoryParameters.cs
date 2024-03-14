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
// Created: December 2, 2019 8:32:27 AM
// 
#endregion

using Gorgon.Editor.PlugIns;
using Gorgon.IO;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// Parameters for the <see cref="RootDirectory"/> view model.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="RootDirectoryParameters"/> class.</remarks>
/// <param name="hostServices">The services from the host application.</param>
/// <param name="factory">The factory used to create view models.</param>
/// <exception cref="ArgumentNullException">Thrown when any of the required parameters are <b>null</b>.</exception>
internal class RootDirectoryParameters(IHostContentServices hostServices, ViewModelFactory factory)
        : ViewModelCommonParameters(hostServices, factory)
{
    #region Properties.
    /// <summary>
    /// Property to set or return the virtual root directory.
    /// </summary>
    public IGorgonVirtualDirectory RootDirectory
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the path to the directory on the physical file system that represents the root of the virtual file system.
    /// </summary>
    public string Path
    {
        get;
        set;
    }

    #endregion
    #region Constructor.
    #endregion
}
