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
// Created: August 26, 2018 9:49:35 PM
// 
#endregion


using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Editor.UI.Views;

/// <summary>
/// The base user control for editor UI panes.
/// </summary> 
public partial class EditorBaseControl
    : UserControl
{
    #region Properties.
    /// <summary>
    /// Property to return whether or not the control is being used in a designer.
    /// </summary>
    [Browsable(false)]
    public bool IsDesignTime
    {
        get;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="EditorBaseControl"/> class.
    /// </summary>
    public EditorBaseControl()
    {
        IsDesignTime = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        InitializeComponent();
    }
    #endregion
}
