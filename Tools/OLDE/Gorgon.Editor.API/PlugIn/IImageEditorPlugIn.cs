#region MIT.
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Wednesday, October 23, 2013 11:24:40 PM
// 
#endregion

using System.IO;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Interface that must be applied to image editors for other plug-ins to utilize the image editing plug-in.
	/// </summary>
	/// <remarks>All image editing plug-ins must implement this on their plug-in entry objects.</remarks>
    public interface IImageEditorPlugIn
    {
		/// <summary>
		/// Property to return the content type for this image editor plug-in.
		/// </summary>
		string ContentType
		{
			get;
		}

        /// <summary>
        /// Property to return the name of the image editor plug-in.
        /// </summary>
	    string Name
	    {
	        get;
	    }

        /// <summary>
        /// Property to return the description of the image editor plug-in.
        /// </summary>
	    string Description
	    {
	        get;
	    }

        /// <summary>
        /// Property to return the file extensions supported by the image editor.
        /// </summary>
	    GorgonFileExtensionCollection FileExtensions
	    {
	        get;
	    }

		/// <summary>
		/// Function to import content from a file system file.
		/// </summary>
		/// <param name="editorFile">The editor file to load.</param>
		/// <param name="imageDataStream">The stream containing the image data.</param>
		/// <returns>An image editor content object.</returns>
		IImageEditorContent ImportContent(EditorFile editorFile, Stream imageDataStream);
    }
}
