
// 
// Gorgon
// Copyright (C) 2015 Michael Winsor
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
// Created: Thursday, September 10, 2015 10:00:57 PM
// 

namespace Gorgon.Input;

/// <summary>
/// Provides capability information about a mouse device
/// </summary>
public interface IGorgonMouseInfo
    : IGorgonRawInputDeviceInfo
{
    /// <summary>
    /// Property to return the sampling rate for the mouse.
    /// </summary>
    int SampleRate
    {
        get;
    }

    /// <summary>
    /// Property to return the number of buttons on the mouse.
    /// </summary>
    int ButtonCount
    {
        get;
    }

    /// <summary>
    /// Property to return whether the mouse supports a horizontal wheel or not.
    /// </summary>
    bool HasHorizontalWheel
    {
        get;
    }

    /// <summary>
    /// Property to return the mouse ID.
    /// </summary>
    int MouseID
    {
        get;
    }
}
