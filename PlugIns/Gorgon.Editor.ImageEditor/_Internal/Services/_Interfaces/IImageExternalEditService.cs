
// 
// Gorgon
// Copyright (C) 2019 Michael Winsor
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
// Created: February 27, 2019 1:02:09 PM
// 


using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// Functionality to edit image data using an external program
/// </summary>
internal interface IImageExternalEditService
{
    /// <summary>
    /// Function to edit the image data in an external application.
    /// </summary>
    /// <param name="workingFile">The file containing the image data to edit.</param>
    /// <param name="exePath">Path to the executable.</param>
    /// <returns><b>true</b> if the image data was changed, <b>false</b> if not.</returns>
    bool EditImage(IGorgonVirtualFile workingFile, string exePath);
}
