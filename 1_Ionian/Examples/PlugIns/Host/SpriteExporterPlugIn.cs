#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: TOBEREPLACED
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary.Graphics;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.Example
{
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

            PlugInFactory.Load(path, false);

            // Get the first plug-in.
            // We can actually host several plug-ins within a single DLL if necessary.
            plugIn = PlugInFactory.PlugIns[0] as SpriteExportPlugIn;

			if (plugIn == null)
				throw new ApplicationException("Could not create the plug-in.");

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
            finally
            {
                if (spriteImage != null)
                    spriteImage.Dispose();
                spriteImage = null;
            }
        }
        #endregion
    }

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
}
