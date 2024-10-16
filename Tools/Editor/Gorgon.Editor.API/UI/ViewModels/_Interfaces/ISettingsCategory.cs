﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: April 19, 2019 1:51:11 PM
// 

using Gorgon.Core;
using Gorgon.Editor.PlugIns;

namespace Gorgon.Editor.UI;

/// <summary>
/// A view model for a settings category
/// </summary>
/// <remarks>
/// <para>
/// This is a view model that is used provide a settings interface for a plug-in. The editor will pick these up and present your custom settings view (along with the view model) so that users can make 
/// changes to plug-in settings. These settings objects are returned to the host editor application by way of the <see cref="EditorPlugIn.OnGetSettings"/> method
/// </para>
/// </remarks>
public interface ISettingsCategory
    : IViewModel, IGorgonNamedObject
{
    /// <summary>
    /// Property to return the ID for the panel.
    /// </summary>
    Guid ID
    {
        get;
    }
}
