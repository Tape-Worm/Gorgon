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
// Created: Sunday, August 06, 2006 1:17:23 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Tao.DevIl;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Object to use DevIL to load/save a GDI+ bitmap.
	/// </summary>
	public static class GorgonDevIL
	{
		#region Methods.
		/// <summary>
		/// Function to retrieve error codes.
		/// </summary>
		/// <returns>A comma seperated list of error codes.</returns>
		private static string GetErrors()
		{
			string errorCode = string.Empty;		// Error code.
			int error = Il.IL_NO_ERROR;				// No error.

			error = Il.ilGetError();

			// Loop through errors until there aren't any.
			while (error != Il.IL_NO_ERROR)
			{				
				if (errorCode != string.Empty)
					errorCode += ", ";
				errorCode += Ilu.iluErrorString(error);
				error = Il.ilGetError();
			}

			return errorCode;
		}

		/// <summary>
		/// Function to save an image.
		/// </summary>
		/// <param name="filename">Filename to save the image to.</param>
		/// <param name="image">Image to save.</param>
		public static void SaveBitmap(string filename, Bitmap image)
		{
			int width;						// Image width.
			int height;						// Image height.
			BitmapData bitmapLock = null;	// Data for locked bitmap.
			int imageID = -1;				// Bound image ID.

			try
			{
				if (image == null)
					throw new DevILInvalidBitmapException();

				// Initialize DevIL.
				Il.ilInit();

				Il.ilGenImages(1, out imageID);
				Il.ilBindImage(imageID);

				// Get width and height of the image.
				width = image.Width;
				height = image.Height;

				image.RotateFlip(RotateFlipType.RotateNoneFlipY);
				
				bitmapLock = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

				// Copy the data.
				if (!Il.ilTexImage(width, height, 1, 4, Il.IL_BGRA, Il.IL_UNSIGNED_BYTE, bitmapLock.Scan0))
					throw new DevILCannotSaveImageException(filename,GetErrors());
				
				image.UnlockBits(bitmapLock);
				bitmapLock = null;

				image.RotateFlip(RotateFlipType.RotateNoneFlipY);

				if (!Il.ilEnable(Il.IL_FILE_OVERWRITE))
					throw new DevILCannotSaveImageException(filename, GetErrors());
				if (!Il.ilSaveImage(filename))
					throw new DevILCannotSaveImageException(filename, GetErrors());
			}
			catch
			{
				// Clean up if an error is encountered.
				if ((image != null) && (bitmapLock != null))
					image.UnlockBits(bitmapLock);
				throw;
			}
			finally
			{
				// Remove the image.
				if (imageID != -1)
					Il.ilDeleteImages(1, ref imageID);

				// Shut down DevIL.
				Il.ilShutDown();
			}
		}

		/// <summary>
		/// Function to load a DevIL image into a GDI+ bitmap.
		/// </summary>
		/// <param name="filename">Name of the file to load.</param>
		/// <returns>A bitmap containing the image.</returns>
		public static Bitmap LoadBitmap(string filename)
		{
			int width;						// Image width.
			int height;						// Image height.
			Bitmap newBitmap = null;		// New bitmap object.
			BitmapData bitmapLock = null;	// Data for locked bitmap.
			int imageID = -1;				// Bound image ID.

			try
			{
				// Initialize DevIL.
				Il.ilInit();

				Il.ilGenImages(1, out imageID);
				Il.ilBindImage(imageID);

				if (!Il.ilLoadImage(filename))
					throw new DevILCannotLoadImageException(filename, GetErrors());

				// Get image dimensions.
				width = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
				height = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);

				// Create new bitmap.
				newBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
				bitmapLock = newBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

				// Copy the data.
				Il.ilConvertImage(Il.IL_BGRA, Il.IL_UNSIGNED_BYTE);
				Il.ilCopyPixels(0, 0, 0, width, height, 1, Il.IL_BGRA, Il.IL_UNSIGNED_BYTE, bitmapLock.Scan0);

				newBitmap.UnlockBits(bitmapLock);
				bitmapLock = null;
			}
			catch
			{
				// Clean up if an error is encountered.
				if (newBitmap != null)
				{
					if (bitmapLock != null)
						newBitmap.UnlockBits(bitmapLock);

					newBitmap.Dispose();
				}
				throw;
			}
			finally
			{
				// Remove the image.
				if (imageID != -1)
					Il.ilDeleteImages(1, ref imageID);

				// Shut down DevIL.
				Il.ilShutDown();
			}

			return newBitmap;
		}
		#endregion
	}
}
