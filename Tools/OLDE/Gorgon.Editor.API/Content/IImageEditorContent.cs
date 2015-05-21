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
// Created: Wednesday, October 23, 2013 11:29:31 PM
// 
#endregion

using System;
using System.IO;
using Gorgon.Graphics;

namespace Gorgon.Editor
{
	/// <summary>
	/// The interface that defines the content produced by an image editor.
	/// </summary>
	/// <remarks>All image editing plug-ins must implement this on their content objects.</remarks>
	public interface IImageEditorContent
        : INamedObject, IDisposable
	{
		/// <summary>
		/// Property to return the editor file associated with the content.
		/// </summary>
		EditorFile EditorFile
		{
			get;
		}

        /// <summary>
        /// Property to return the image held in the content object.
        /// </summary>
	    GorgonImageData Image
	    {
	        get;
	    }

		/// <summary>
		/// Property to return the content identifier for the image editor.
		/// </summary>
		string ContentType
		{
			get;
		}

		/// <summary>
		/// Function to load data into the image.
		/// </summary>
		/// <param name="stream">Stream containing the data to load.</param>
		/// <remarks>This method is used by the editor to import images into various places.  The <see cref="Image"/> property will remain NULL (Nothing in VB.Net) until this method is called.</remarks>
		void Load(Stream stream);
	}
}
