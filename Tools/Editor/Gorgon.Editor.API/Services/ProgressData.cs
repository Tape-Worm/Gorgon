
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
// Created: May 27, 2020 3:36:45 PM
// 


namespace Gorgon.Editor.Services;

/// <summary>
/// Value type used to relay progress back to a subscriber
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="ProgressData"/> struct.</remarks>
/// <param name="current">The current item.</param>
/// <param name="total">The total number of items.</param>
public readonly struct ProgressData(int current, int total)
{
    /// <summary>
    /// The current value being processed.
    /// </summary>
    public readonly int Current = current;
    /// <summary>
    /// The total number of items to process.
    /// </summary>
    public readonly int Total = total;
}
