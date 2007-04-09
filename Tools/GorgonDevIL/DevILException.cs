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
// Created: Sunday, August 06, 2006 1:29:41 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// DevIL error codes.
	/// </summary>
	public enum DevILErrors
	{
		/// <summary>Could not load the image.</summary>
		CannotLoadImage = 0x7FFF0001,
		/// <summary>Invalid bitmap object.</summary>
		InvalidBitmap = 0x7FFF0002,
		/// <summary>Could not save the image.</summary>
		CannotSaveImage = 0x7FFF0003
	}

	/// <summary>
	/// Cannot load image exception.
	/// </summary>
	public class DevILCannotLoadImageException : SharpException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="description">Description of the error.</param>
		/// <param name="errorCode">Error code.</param>
		public DevILCannotLoadImageException(string description, DevILErrors errorCode)
			: base(description, (int)errorCode)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="imageName">Name of the image.</param>
		/// <param name="devilError">Devil error code.</param>
		public DevILCannotLoadImageException(string imageName, string devilError)
			: this("Unable to load the image '" + imageName + "'.", DevILErrors.CannotLoadImage)
		{
		}
		#endregion
	}

	/// <summary>
	/// Invalid bitmap exception.
	/// </summary>
	public class DevILInvalidBitmapException : SharpException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="description">Description of the error.</param>
		/// <param name="errorCode">Error code.</param>
		public DevILInvalidBitmapException(string description, DevILErrors errorCode)
			: base(description, (int)errorCode)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public DevILInvalidBitmapException()
			: this("Bitmap passed was invalid.", DevILErrors.InvalidBitmap)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot save image exception.
	/// </summary>
	public class DevILCannotSaveImageException : SharpException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="description">Description of the error.</param>
		/// <param name="errorCode">Error code.</param>
		public DevILCannotSaveImageException(string description, DevILErrors errorCode)
			: base(description, (int)errorCode)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="imageName">Name of the image.</param>
		/// <param name="devilError">Devil error code.</param>
		public DevILCannotSaveImageException(string imageName, string devilError)
			: this("Unable to save the image '" + imageName + "' (" + devilError + ").", DevILErrors.CannotLoadImage)
		{
		}
		#endregion
	}
}
