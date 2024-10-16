﻿
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
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
// Created: December 5, 2020 3:43:01 PM
// 

namespace Gorgon.UI;

/// <summary>
/// The form used to display the translucent overlay
/// </summary>
public partial class FormOverlay
    : Form
{
    /// <summary>Gets the required creation parameters when the control handle is created.</summary>
    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams baseParams = base.CreateParams;

            const int wsExNoActivate = 0x08000000;

            baseParams.ExStyle |= wsExNoActivate;

            return baseParams;
        }
    }

    /// <summary>Initializes a new instance of the <see cref="FormOverlay" /> class.</summary>
    public FormOverlay() => InitializeComponent();

}
