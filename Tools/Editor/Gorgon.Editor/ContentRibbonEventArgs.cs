
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: November 10, 2018 10:13:20 PM
// 

using Krypton.Ribbon;

namespace Gorgon.Editor;

/// <summary>
/// Event parameters used to indicate when a content ribbon is added to or removed from the project view
/// </summary>
/// <remarks>Initializes a new instance of the ContentRibbonAddedEventArgs class.</remarks>
/// <param name="ribbon">The ribbon.</param>
internal class ContentRibbonEventArgs(KryptonRibbon ribbon)
        : EventArgs
{
    /// <summary>
    /// Property to return the ribbon that was added.
    /// </summary>
    public KryptonRibbon Ribbon
    {
        get;
    } = ribbon;
}
