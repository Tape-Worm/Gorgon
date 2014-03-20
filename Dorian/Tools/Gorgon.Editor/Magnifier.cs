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
// Created: Thursday, March 20, 2014 2:19:42 PM
// 
#endregion

using System;
using System.Drawing;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.Math;
using GorgonLibrary.Renderers;
using SlimMath;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Interface used to magnify a texture in a window.
	/// </summary>
	public class ZoomWindow
	{
		#region Variables.
		// Sprite used to perform magnification.
		private readonly GorgonSprite _sprite;
		// Renderer used to render the magnification sprite.
		private readonly Gorgon2D _renderer;
		// Texture to magnify.
		private readonly GorgonTexture2D _texture;
		// Zoom amount.
		private float _zoom;
		// Zoom window size.
		private Vector2 _windowSize;
		// Size of the zoomed region, in pixels.
		private Vector2 _zoomSize;
		// Caption text.
		private string _zoomWindowText;
		// Height of the caption.
		private float _captionHeight = 16.0f;
		// The font for the caption.
		private GorgonFont _zoomFont;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the location of the zoom window.
		/// </summary>
		public Vector2 ZoomWindowLocation
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size of the zoom window.
		/// </summary>
		/// <remarks>The minimum window size is 128x128.</remarks>
		public Vector2 ZoomWindowSize
		{
			get
			{
				return _windowSize;
			}
			set
			{
				if (value.X < 128)
				{
					value.X = 128;
				}

				if (value.Y < 128)
				{
					value.Y = 128;
				}

				_windowSize = value;

				UpdateTextureCoordinates();
			}
		}

		/// <summary>
		/// Property to set or return the position on the texture to magnify.
		/// </summary>
		public Vector2 Position
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the amount to zoom.
		/// </summary>
		/// <remarks>The minimum value is 1 and the maximum value is 32.</remarks>
		public float ZoomAmount
		{
			get
			{
				return _zoom;
			}
			set
			{
				if (value < 1)
				{
					value = 1;
				}

				if (value > 32)
				{
					value = 32;
				}
					
				_zoom = value;

				UpdateTextureCoordinates();
			}
		}

		/// <summary>
		/// Property to set or return the font to use for the zoom window caption.
		/// </summary>
		public GorgonFont ZoomWindowFont
		{
			get
			{
				return _zoomFont;
			}
			set
			{
				_zoomFont = value ?? _renderer.Graphics.Fonts.DefaultFont;

				CalculateCaptionHeight();
			}
		}

		/// <summary>
		/// Property to set or return the zoom window caption text.
		/// </summary>
		public string ZoomWindowText
		{
			get
			{
				return _zoomWindowText;
			}
			set
			{
				if (string.IsNullOrWhiteSpace(_zoomWindowText))
				{
					_zoomWindowText = Resources.GOREDIT_ZOOMWINDOW_TEXT;
				}

				_zoomWindowText = value;

				CalculateCaptionHeight();
			}
		}

		/// <summary>
		/// Property to set or return the clipper bound to this magnifier.
		/// </summary>
		public Clipper Clipper
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to calculate the height of the caption.
		/// </summary>
		private void CalculateCaptionHeight()
		{
			Vector2 size = _renderer.Drawing.MeasureString(ZoomWindowFont,
			                                               string.Format("{0}: {1:0.0}x", ZoomWindowText, _zoom),
			                                               false,
			                                               new SizeF(ZoomWindowSize.X - 2, 30));

			_captionHeight = size.Y.FastFloor() + 2;
		}

		/// <summary>
		/// Function to update the texture coordinates.
		/// </summary>
		private void UpdateTextureCoordinates()
		{
			_sprite.Size = new Vector2(_windowSize.X - 2, _windowSize.Y - 17);

			Vector2 textureSize = _sprite.Size;
			Vector2.Divide(ref textureSize, _zoom, out _zoomSize);

			_sprite.TextureSize = _texture.ToTexel(_zoomSize);

			CalculateCaptionHeight();
		}

		/// <summary>
		/// Function to draw the magnification window.
		/// </summary>
		public void Draw()
		{
			var texturePosition = new Vector2(Position.X - _zoomSize.X * 0.5f, Position.Y - _zoomSize.Y * 0.5f);
			var clientRect = new RectangleF(ZoomWindowLocation.X + 1,
			                                ZoomWindowLocation.Y + _captionHeight,
			                                _sprite.Size.X,
											_sprite.Size.Y);

			_renderer.Drawing.BlendingMode = BlendingMode.Modulate;

			//Vector2.Divide(ref texturePosition, _zoom, out texturePosition);
			
			_renderer.Drawing.FilledRectangle(new RectangleF(ZoomWindowLocation, ZoomWindowSize), GorgonColor.Black);
			_renderer.Drawing.FilledRectangle(new RectangleF(ZoomWindowLocation.X + 1, ZoomWindowLocation.Y + 1, ZoomWindowSize.X - 2, _captionHeight - 2), GorgonColor.White);
			_renderer.Drawing.DrawString(ZoomWindowFont ?? _renderer.Graphics.Fonts.DefaultFont,
										 string.Format("{0}: {1:0.0}x", ZoomWindowText, _zoom),
			                             ZoomWindowLocation,
			                             GorgonColor.Black);
		
			_sprite.TextureOffset = _texture.ToTexel(texturePosition);
			_sprite.Position = clientRect.Location;
			_sprite.Draw();

			if (Clipper == null)
			{
				return;
			}

			Vector2 prevPosition = Clipper.SelectionSprite.Position;
			Vector2 prevSize = Clipper.SelectionSprite.Size;

			var clipArea = new RectangleF((Clipper.ClipRegion.X - texturePosition.X) * _zoom + clientRect.X,
			                              (Clipper.ClipRegion.Y - texturePosition.Y) * _zoom + clientRect.Y,
			                              Clipper.ClipRegion.Width * _zoom,
			                              Clipper.ClipRegion.Height * _zoom);

			clipArea = RectangleF.Intersect(clipArea, clientRect);

			Clipper.SelectionSprite.Size = clipArea.Size;
			Clipper.SelectionSprite.Position = clipArea.Location;
			Clipper.SelectionSprite.Draw();

			_renderer.Drawing.DrawRectangle(new RectangleF(clipArea.X, clipArea.Y, clipArea.Width, clipArea.Height - 1), new GorgonColor(Clipper.SelectorColor, 1.0f));

			Clipper.SelectionSprite.Size = prevSize;
			Clipper.SelectionSprite.Position = prevPosition;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ZoomWindow"/> class.
		/// </summary>
		/// <param name="renderer">The renderer to use for magnification.</param>
		/// <param name="texture">The texture to zoom.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="renderer"/> or the <paramref name="texture"/> parameter is NULL (Nothing in VB.Net).</exception>
		public ZoomWindow(Gorgon2D renderer, GorgonTexture2D texture)
		{
			if (renderer == null)
			{
				throw new ArgumentNullException("renderer");
			}

			if (texture == null)
			{
				throw new ArgumentNullException("texture");
			}

			_windowSize = new Vector2(128, 128);
			Position = Vector2.Zero;
			_zoom = 2.0f;
			_renderer = renderer;
			_texture = texture;

			_sprite = _renderer.Renderables.CreateSprite("Zoomer",
			                                             new GorgonSpriteSettings
			                                             {
				                                             Texture = _texture,
				                                             Size = _windowSize,
				                                             InitialPosition = ZoomWindowLocation,
				                                             TextureRegion = new RectangleF(0, 0, 1, 1)
			                                             });

			_sprite.TextureSampler.HorizontalWrapping = TextureAddressing.Border;
			_sprite.TextureSampler.VerticalWrapping = TextureAddressing.Border;
			_sprite.TextureSampler.BorderColor = GorgonColor.Black;
			_zoomWindowText = Resources.GOREDIT_ZOOMWINDOW_TEXT;
			_zoomFont = renderer.Graphics.Fonts.DefaultFont;

			UpdateTextureCoordinates();
		}
		#endregion
	}
}
