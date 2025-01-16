
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
// Created: Tuesday, January 1, 2013 7:35:52 PM
// 

namespace Gorgon.Examples;

/// <summary>
/// Here's our main plug-in entry point
/// 
/// In this plug-in assembly we have 2 plug-ins in the same assembly. 
/// </summary>
internal class GreenPlugIn
    : TextColorPlugIn
{
    /// <summary>
    /// Function to create the writer that will output our text with a specified color.
    /// </summary>
    /// <returns>The writer interface.</returns>
    public override TextColorWriter CreateWriter() => new TextMultiColorWriter(ConsoleColor.Green);

    /// <summary>
    /// Initializes a new instance of the <see cref="GreenPlugIn" /> class.
    /// </summary>
    public GreenPlugIn()
        : base("This plug-in will print green text.")
    {
        // Here we can pass a description of the plug-in back to the application.
        // This is handy when we want friendly names for the plug-in.
    }
}
