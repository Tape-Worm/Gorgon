#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Sunday, May 06, 2012 2:12:03 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel;
using SlimMath;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Font document.
	/// </summary>
	class FontDocument
		: Document
	{
		#region Variables.
		private bool _disposed = false;						// Flag to indicate that the object was disposed.
		private GorgonFontSettings _fontSettings = null;	// Font settings.
		private GorgonFont _font = null;					// Font.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the font.
		/// </summary>
		[Browsable(false)]
		public GorgonFont Font
		{
			get
			{
				return _font;
			}
			set
			{
				if (value == null)
					return;

				if (_font != value)
				{
					if (_font != null)
					{
						_font.Dispose();
						_font = null;
					}

					_font = value;
					_fontSettings = _font.Settings;
					DispatchUpdateNotification();
				}
			}
		}
		
		/// <summary>
		/// Property to set or return the font style.
		/// </summary>
		[Editor(typeof(FontStylePicker), typeof(UITypeEditor))]
		public FontStyle FontStyle
		{
			get
			{
				return _fontSettings.FontStyle;
			}
			set
			{
				if (_fontSettings.FontStyle != value)
				{
					_fontSettings.FontStyle = value;
					DispatchUpdateNotification();
				}
			}
		}

		/// <summary>
		/// Property to set or return the name of the font family.
		/// </summary>
		[Editor(typeof(FontFamilyPicker), typeof(UITypeEditor)), RefreshProperties(System.ComponentModel.RefreshProperties.Repaint)]
		public string FontFamily
		{
			get
			{
				return _fontSettings.FontFamilyName;
			}
			set
			{
				if (value == null)
					value = string.Empty;

				if (string.Compare(_fontSettings.FontFamilyName, value, true) != 0)
				{
					_fontSettings.FontFamilyName = value;
					DispatchUpdateNotification();
				}
			}
		}

		/// <summary>
		/// Property to set or return the font size.
		/// </summary>
		public float FontSize
		{
			get
			{
				return _fontSettings.Size;
			}
			set
			{
				if (value < 1.0f) 
					value = 1.0f;

				if (_fontSettings.Size != value)
				{
					_fontSettings.Size = value;
					DispatchUpdateNotification();
				}
			}
		}

		/// <summary>
		/// Property to set or return the texture size for the font.
		/// </summary>
		public Size FontTextureSize
		{
			get
			{
				return _fontSettings.TextureSize;
			}
			set
			{
				if (value.Width < 128)
					value.Width = 128;
				if (value.Height < 128)
					value.Height = 128;
				if (value.Width >= Program.Graphics.Textures.MaxWidth)
					value.Width = Program.Graphics.Textures.MaxWidth - 1;
				if (value.Height >= Program.Graphics.Textures.MaxHeight)
					value.Height = Program.Graphics.Textures.MaxHeight - 1;

				if (_fontSettings.TextureSize != value)
				{
					_fontSettings.TextureSize = value;
					DispatchUpdateNotification();
				}
			}
		}

		/// <summary>
		/// Property to set or return the anti-alias mode.
		/// </summary>
		public FontAntiAliasMode FontAntiAliasMode
		{
			get
			{
				return _fontSettings.AntiAliasingMode;
			}
			set
			{
				if (_fontSettings.AntiAliasingMode != value)
				{
					_fontSettings.AntiAliasingMode = value;
					DispatchUpdateNotification();
				}
			}
		}

		/// <summary>
		/// Property to set or return whether the font size should be in point size or pixel size.
		/// </summary>
		[DisplayName("FontHeightMode"), Category("Design")]
		public FontHeightMode UsePointSize
		{
			get
			{
				return _fontSettings.FontHeightMode;
			}
			set
			{
				if (value != _fontSettings.FontHeightMode)
				{
					_fontSettings.FontHeightMode = value;
					DispatchUpdateNotification();
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize graphics and other items for the document.
		/// </summary>
		protected override void LoadGraphics()
		{			
		}

		/// <summary>
		/// Function to remove graphics and other items for the document.
		/// </summary>
		protected override void UnloadGraphics()
		{			
		}

		/// <summary>
		/// Function to draw to the control.
		/// </summary>
		/// <param name="timing">Timing data.</param>
		protected override void Draw(Diagnostics.GorgonFrameRate timing)
		{
			if (_font == null)
				return;

			Renderer.IsBlendingEnabled = false;
			Renderer.IsAlphaTestEnabled = false;
			Renderer.Drawing.Blit(_font.Textures[0], Vector2.Zero);
			Renderer.IsAlphaTestEnabled = true;
			Renderer.IsBlendingEnabled = true;
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_font != null)
						_font.Dispose();
				}

				_font = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to update the font.
		/// </summary>
		public void Update()
		{
			if (!Program.CachedFonts.ContainsKey(FontFamily))
				return;

			Font cachedFont = Program.CachedFonts[FontFamily];
			FontStyle[] styles = Enum.GetValues(typeof(FontStyle)) as FontStyle[];

			// Remove styles that won't work.
			for (int i = 0; i < styles.Length; i++)
			{
				if ((!cachedFont.FontFamily.IsStyleAvailable(styles[i])) && ((_fontSettings.FontStyle & styles[i]) == styles[i]))
					_fontSettings.FontStyle &= ~styles[i];
			}

			// If we're left with regular and the font doesn't support it, then use the next available style.
			if ((!cachedFont.FontFamily.IsStyleAvailable(System.Drawing.FontStyle.Regular)) && (_fontSettings.FontStyle == System.Drawing.FontStyle.Regular))
			{
				for (int i = 0; i < styles.Length; i++)
				{
					if (cachedFont.FontFamily.IsStyleAvailable(styles[i]))
					{
						_fontSettings.FontStyle |= styles[i];
						break;
					}
				}
			}
			
			if (_font == null)
				_font = Program.Graphics.Textures.CreateFont(Name, _fontSettings);
			else
				_font.Update(_fontSettings);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FontDocument"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="allowClose">TRUE to allow the document to close, FALSE to keep open.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		///   
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		public FontDocument(string name, bool allowClose)
			: base(name, allowClose)
		{
			_fontSettings = new GorgonFontSettings();
			HasProperties = true;
		}
		#endregion
	}
}
