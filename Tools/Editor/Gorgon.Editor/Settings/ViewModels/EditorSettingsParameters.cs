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
// Created: April 20, 2019 10:23:20 AM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// The parameters for the <see cref="IEditorSettings"/> view model.
/// </summary>
internal class EditorSettingsParameters
    : IViewModelInjection<IHostServices>
{
    /// <summary>
    /// Property to set or return the categories for the settings.
    /// </summary>
    public IEnumerable<ISettingsCategory> Categories
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the list of plug ins for the fixed plug in list category.
    /// </summary>
    public ISettingsPlugInsList PlugInsList
    {
        get;
        set;
    }

    /// <summary>Property to return the services passed from host application.</summary>
    public IHostServices HostServices
    {
        get;
        set;
    }
}
