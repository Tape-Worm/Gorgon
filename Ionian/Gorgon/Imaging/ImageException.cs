#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Tuesday, October 31, 2006 4:55:33 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Could not get image information.
	/// </summary>
	public class ImageInformationNotFoundException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public ImageInformationNotFoundException(string message, Exception ex)
			: base(message, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		public ImageInformationNotFoundException(string message)
			: base(message, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Image size exception.
	/// </summary>
	public class ImageSizeException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="ex">Source exception.</param>
		public ImageSizeException(string message, Exception ex)
			: base(message, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		public ImageSizeException(string message)
			: this(message, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="imageName">Name of the image.</param>
		/// <param name="width">Requested width.</param>
		/// <param name="height">Requested height.</param>
		/// <param name="maxWidth">Maximum width the hardware will allow.</param>
		/// <param name="maxHeight">Maximum height the hardware will allow.</param>
		/// <param name="ex">Source exception.</param>
		public ImageSizeException(string imageName, int width, int height, int maxWidth, int maxHeight, Exception ex)
			: this("Image '" + imageName + "' is outside of the range of acceptable image sizes. (1,1) -> (" + width.ToString() + ", " + height.ToString() + ").\nMaximum size is (" + maxWidth.ToString() + ", " + maxHeight.ToString() + ".", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="imageName">Name of the image.</param>
		/// <param name="width">Requested width.</param>
		/// <param name="height">Requested height.</param>
		/// <param name="maxWidth">Maximum width the hardware will allow.</param>
		/// <param name="maxHeight">Maximum height the hardware will allow.</param>
		public ImageSizeException(string imageName, int width, int height, int maxWidth, int maxHeight)
			: this("Image '" + imageName + "' is outside of the range of acceptable image sizes. (1,1) -> (" + width.ToString() + ", " + height.ToString() + ").\nMaximum size is (" + maxWidth.ToString() + ", " + maxHeight.ToString() + ".", null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Image already loaded.
	/// </summary>
	public class ImageAlreadyLoadedException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="imageName">Name of the image.</param>
		/// <param name="ex">Source exception.</param>
		public ImageAlreadyLoadedException(string imageName, Exception ex)
			: base("The image '" + imageName + "' has already been loaded.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="imageName">Name of the image.</param>
		public ImageAlreadyLoadedException(string imageName)
			: this(imageName, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Image not valid.
	/// </summary>
	public class ImageNotValidException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public ImageNotValidException(Exception ex)
			: base("The image is not valid.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public ImageNotValidException()
			: this(null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Image shader not valid.
	/// </summary>
	public class ImageShaderNotValidException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public ImageShaderNotValidException(Exception ex)
			: base("The image shader is not a valid shader.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public ImageShaderNotValidException()
			: this(null)
		{
		}
		#endregion
	}
}
