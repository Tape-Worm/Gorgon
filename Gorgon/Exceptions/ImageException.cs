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
	/// Error codes for image exceptions.
	/// </summary>
	public enum ImageErrors
	{
		/// <summary>Dimensions of the image are larger than the hardware can support.</summary>
		ImageSizeInvalid = 0x7FFF0005,
		/// <summary>Image was already loaded.</summary>
		ImageAlreadyLoaded = 0x7FFF0006,
		/// <summary>Could not retrieve image information.</summary>
		CannotGetImageInformation = 0x7FFF0009		
	}

	/// <summary>
	/// Base exception for image exceptions.
	/// </summary>
	public abstract class ImageException
		: ResourceException
	{
		#region "Constructor."
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="code">Error code.</param>
		/// <param name="ex">Source exception.</param>
		public ImageException(string message, ImageErrors code, Exception ex)
			: base(message, (int)code, ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Could not get image information exception.
	/// </summary>
	public class CannotGetImageInformationException
		: ImageException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public CannotGetImageInformationException(string message, Exception ex)
			: base(message, ImageErrors.CannotGetImageInformation, ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Image size exception.
	/// </summary>
	public class ImageSizeException
		: ImageException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="ex">Source exception.</param>
		public ImageSizeException(string message, Exception ex)
			: base(message, ImageErrors.ImageSizeInvalid, ex)
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
		#endregion
	}

	/// <summary>
	/// Image already loaded exception.
	/// </summary>
	public class ImageAlreadyLoadedException
		: ImageException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="ex">Source exception.</param>
		public ImageAlreadyLoadedException(string message, Exception ex)
			: base(message, ImageErrors.ImageAlreadyLoaded, ex)
		{
		}
		#endregion
	}
}
