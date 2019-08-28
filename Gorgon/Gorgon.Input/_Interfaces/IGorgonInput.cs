#region MIT
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Wednesday, September 16, 2015 11:54:29 PM
// 
#endregion

using System.Collections.Generic;

namespace Gorgon.Input
{
    /// <summary>
    /// Defines functionality for an input system that supports keyboards and mice.
    /// </summary>
    /// <remarks>
    /// This object will allow for enumeration of keyboards and mice attached to the system and will allow an application to register these types of devices for use with the application.  
    /// </remarks>
    public interface IGorgonInput
    {
        /// <summary>
        /// Function to retrieve a list of mice.
        /// </summary>
        /// <returns>A read only list containing information about each mouse.</returns>
        IReadOnlyList<IGorgonMouseInfo> EnumerateMice();

        /// <summary>
        /// Function to retrieve a list of keyboards.
        /// </summary>
        /// <returns>A read only list containing information about each keyboard.</returns>
        IReadOnlyList<IGorgonKeyboardInfo> EnumerateKeyboards();
    }
}