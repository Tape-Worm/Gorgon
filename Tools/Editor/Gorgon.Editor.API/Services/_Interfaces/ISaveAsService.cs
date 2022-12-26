#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: December 12, 2021 1:32:34 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor;
using Gorgon.Editor.Content;

namespace Gorgon.Editor.Services;

/// <summary>
/// A service used to present a save as dialog for the project file system.
/// </summary>
public interface ISaveAsService
{
    /// <summary>
    /// Function to present a means of providing a path for a save as operation.
    /// </summary>
    /// <param name="currentFileName">The current file name.</param>
    /// <param name="filesOfType">The extension to search for.</param>
    /// <param name="typeKey">[Optional] The key to check in the file metadata for the file type.</param>
    /// <returns>The selected file path to save the file as.</returns>
    string SaveAs(string currentFileName, string filesOfType, string typeKey = CommonEditorConstants.ContentTypeAttr);
}
