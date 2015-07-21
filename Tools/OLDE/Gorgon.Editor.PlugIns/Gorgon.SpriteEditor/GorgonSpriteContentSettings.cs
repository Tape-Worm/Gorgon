#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Tuesday, November 11, 2014 11:49:03 PM
// 
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.IO;
using Gorgon.Renderers;
using Gorgon.UI;

namespace Gorgon.Editor.SpriteEditorPlugIn
{
	/// <summary>
	/// Settings for sprite content.
	/// </summary>
	class GorgonSpriteContentSettings
		: ContentSettings
	{
		#region Variables.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the settings for the sprite.
		/// </summary>
		public GorgonSpriteSettings Settings
		{
			get;
		}

		/// <summary>
		/// Property to return the texture dependency for this sprite.
		/// </summary>
		public Dependency TextureDependency
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize the settings for the content.
		/// </summary>
		/// <returns>
		/// <b>true</b> if the object was set up, <b>false</b> if not.
		/// </returns>
		public override bool PerformSetup()
		{
			FormNewSprite newSprite = null;
			
			try
			{
				newSprite = new FormNewSprite
				            {
					            ImageEditor = PlugIn.GetRegisteredImageEditor()
				            };

				if (newSprite.ShowDialog() != DialogResult.OK)
				{
					if (newSprite.Texture != null)
					{
						newSprite.Texture.Dispose();
					}

					return false;
				}

				Cursor.Current = Cursors.WaitCursor;

				Name = newSprite.SpriteName.FormatFileName();

				Settings.Texture = newSprite.Texture;
				Settings.TextureRegion = newSprite.Texture.ToTexel(new RectangleF(0, 0, 1, 1));
				Settings.Size = new Vector2(1);
				Settings.Color = Color.White;
				
				TextureDependency = newSprite.Dependency;

				return true;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
				return false;
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				if (newSprite != null)
				{
					newSprite.Dispose();
				}
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSpriteContentSettings"/> class.
		/// </summary>
		public GorgonSpriteContentSettings()
		{
			Settings = new GorgonSpriteSettings();
		}
		#endregion
	}
}
