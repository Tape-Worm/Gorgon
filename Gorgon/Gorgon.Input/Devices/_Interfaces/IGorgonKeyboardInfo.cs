
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
// Created: Thursday, September 10, 2015 10:04:24 PM
// 

namespace Gorgon.Input;

/// <summary>
/// Provides capability information about a keyboard device
/// </summary>
public interface IGorgonKeyboardInfo
    : IGorgonRawInputDeviceInfo
{
    /// <summary>
    /// Property to return the total number of keys present on the keyboard.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value may or may not be accurate depending on the implementation. That is, for some systems, this will be an estimate, and for others this will be accurate.
    /// </para> 
    /// </remarks>
    int KeyCount
    {
        get;
    }

    /// <summary>
    /// Property to return the number of LED indicators on the keyboard.
    /// </summary>
    int IndicatorCount
    {
        get;
    }

    /// <summary>
    /// Property to return the number of function keys on the keyboard.
    /// </summary>
    int FunctionKeyCount
    {
        get;
    }

    /// <summary>
    /// Property to return the type of keyboard.
    /// </summary>
    KeyboardType KeyboardType
    {
        get;
    }
}
