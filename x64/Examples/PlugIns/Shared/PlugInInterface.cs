#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Tuesday, December 11, 2007 9:56:58 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary.Graphics;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.Example
{
	// This DLL is shared between any hosts and plug-ins.  From here the plug-ins know what they're supposed to
	// implement.  And the hosts know what interface to expect from a plug-in.  While using a proxy like this 
	// seems messy, it's a necessary evil.

	/// <summary>
	/// Object representing a sprite export plug-in.
	/// </summary>
	/// <remarks>Our plug-in will remarkably export a sprite to an image file.  Crazy!</remarks>
	public abstract class SpriteExportPlugIn
		: PlugInEntryPoint
	{
		#region Methods.
		/// <summary>
		/// Function to create the sprite exporter plug-in.
		/// </summary>
		/// <returns>A new instance of the sprite exporter.</returns>
		internal SpriteExporter Create()
		{
			return CreateImplementation(null) as SpriteExporter;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="SpriteExportPlugIn"/> class.
		/// </summary>
		/// <param name="name">Name of the plug-in.</param>
		/// <param name="path">Path to the plug-in.</param>
		public SpriteExportPlugIn(string name, string path)
			: base(name, path, PlugInType.UserDefined)
		{
		}
		#endregion
	}

	/// <summary>
	/// Abstract object for a sprite exporter that will export the sprite.
	/// </summary>
	public abstract class SpriteExporter
	{
		#region Methods.
		/// <summary>
		/// Function to save the image.
		/// </summary>
		/// <param name="image">Image to save.</param>
		/// <param name="path">Path for the image.</param>
		protected abstract void Save(Image image, string path);

		/// <summary>
		/// Function to load a sprite exporter plug-in.
		/// </summary>
		/// <param name="path">Path to the plug-in.</param>
		/// <returns>A new sprite exporter.</returns>
		public static SpriteExporter LoadPlugIn(string path)
		{
			SpriteExportPlugIn plugIn = null;		// Plug-in.

			PlugInFactory.Load(path);
				
			// Get the first plug-in.
			// We can actually host several plug-ins within a single DLL if necessary.
			plugIn = PlugInFactory.PlugIns[0] as SpriteExportPlugIn;

			if (plugIn == null)
				throw new PlugInCannotCreateException("SpriteExporter");

			return plugIn.Create();
		}

		/// <summary>
		/// Function to export the sprite to a PNG file.
		/// </summary>
		/// <param name="sprite"></param>
		/// <param name="path"></param>
		public void Export(Sprite sprite, string path)
		{
			RenderImage spriteImage = null;			// Image to save.
			RenderTarget lastTarget = null;			// Last render target.

			try
			{
				if (sprite == null)
					throw new ArgumentNullException("sprite");
				if (string.IsNullOrEmpty(path))
					throw new ArgumentNullException("path");

				spriteImage = new RenderImage("SpriteTempImage", (int)sprite.ScaledWidth, (int)sprite.ScaledHeight, ImageBufferFormats.BufferRGB888A8);
				spriteImage.Clear(System.Drawing.Color.Transparent);

				// Preserve the last render target.
				lastTarget = Gorgon.CurrentRenderTarget;
				Gorgon.CurrentRenderTarget = spriteImage;
				sprite.SetAxis(0, 0);
				sprite.SetPosition(0, 0);
				sprite.Draw();
				Gorgon.CurrentRenderTarget = lastTarget;

				// Write out the image.
				Save(spriteImage.Image, path);
			}
			catch
			{
				throw;
			}
			finally
			{
				if (spriteImage != null)
					spriteImage.Dispose();
				spriteImage = null;
			}
		}
		#endregion
	}
}
