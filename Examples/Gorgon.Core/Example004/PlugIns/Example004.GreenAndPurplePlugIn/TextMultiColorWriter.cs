
// 
// Gorgon
// Copyright (C) 2013 Michael Winsor
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
// Created: Tuesday, January 1, 2013 7:44:21 PM
// 


namespace Gorgon.Examples;

/// <summary>
/// Our writer that uses multiple colors
/// 
/// So now we can implement our green/purple text color writer by passing in the color 
/// on the constructor from the plug in interface
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TextMultiColorWriter"/> class
/// </remarks>
/// <param name="color">The color to use when printing the text.</param>
internal class TextMultiColorWriter(ConsoleColor color)
        : TextColorWriter
{
    /// <summary>
    /// We'll use this property to advertise the text color.
    /// </summary>
    public override ConsoleColor TextColor
    {
        get;
    } = color;
}
