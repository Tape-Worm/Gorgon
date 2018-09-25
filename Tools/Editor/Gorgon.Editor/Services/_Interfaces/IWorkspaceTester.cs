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
// Created: August 27, 2018 9:39:56 PM
// 
#endregion

using System.IO;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A locator for finding the most suitable workspace for the project.
    /// </summary>
    internal interface IWorkspaceTester
    {
        /// <summary>
        /// Function to test a location for accessibility.
        /// </summary>
        /// <param name="path">The path to the workspace.</param>
        /// <returns>A tuple containing a <see cref="bool"/> flag to indicate whether the <b>true</b> if the area is accessible or not, and a string containing the reason why it is not acceptable.</returns>
        (bool isAcceptable, string reason) TestForAccessibility(DirectoryInfo path);

        /// <summary>
        /// Function to find the most suitable location for the workspace directory.
        /// </summary>
        /// <returns>The directory that is most suitable to use for a workspace.</returns>
        DirectoryInfo FindBestDirectory();
    }
}