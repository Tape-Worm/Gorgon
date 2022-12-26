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
// Created: September 15, 2018 4:19:08 PM
// 
#endregion

using System;

namespace Gorgon.Editor.UI;

/// <summary>
/// Arguments used for the 
/// </summary>
public class ProgressPanelUpdateArgs
    : EventArgs
{
    /// <summary>
    /// Property to set or return the method to execute when the operation is cancelled from the panel.
    /// </summary>
    public Action CancelAction
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the title for the panel.
    /// </summary>
    public string Title
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the message for the panel.
    /// </summary>
    public string Message
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the percentage complete as a normalized (0..1) value.
    /// </summary>
    public float PercentageComplete
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether to use a marquee for the progress bar.
    /// </summary>
    public bool IsMarquee
    {
        get;
        set;
    }
}
