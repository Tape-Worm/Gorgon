#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
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
