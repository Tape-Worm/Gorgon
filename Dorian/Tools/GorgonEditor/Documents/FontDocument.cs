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
using System.Windows.Forms;
using SlimMath;
using GorgonLibrary.Graphics;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Font document.
	/// </summary>
	class FontDocument
		: Document
	{
		#region Variables.
		private GorgonTexture2D _ibar = null;				// I-Bar texture.
		private controlFontDisplay _fontWindow = null;				// Font window.
		private bool _disposed = false;						// Flag to indicate that the object was disposed.
		private GorgonRenderTarget _target = null;			// Render target.
		private GorgonFont _font = null;					// Font.
		private Vector2 _lastBoundSize = Vector2.Zero;		// Last bounding area.
		private GorgonFontSettings _fontSettings = null;	// Font settings.
		private GorgonSwapChain _textArea = null;			// Text area.
		private GorgonText _text = null;					// Text sprite.
		private bool _showIBar = true;						// I-Bar flag.
		private GorgonTimer _iBarTimer = null;				// I-Bar timer.
		private Vector2 _dropOffset = Vector2.Zero;			// Drop shadow offset.
		private bool _dropShadowEnabled = false;			// Flag for the drop shadow.
		#endregion

		#region Properties
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
		/// Function to set the base color for the font glyphs.
		/// </summary>
		[Category("Appearance"), Description("Sets a base color for the glyphs on the texture."), Editor(typeof(ColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(RGBATypeConverter))]
		public Color BaseColor
		{
			get
			{
				return _fontSettings.BaseColors[0];
			}
			set
			{
				if (_fontSettings.BaseColors[0].ToColor() != value)
				{
					_fontSettings.BaseColors = new GorgonColor[] { value };
					DispatchUpdateNotification();
				}
			}
		}

		/// <summary>
		/// Property to set or return the outline size.
		/// </summary>
		[Category("Appearance"), Description("Sets the size, in pixels, of an outline to apply to each glyph.  Please note that the outline color must have an alpha greater than 0 before the outline is applied."), DefaultValue(0)]
		public int OutlineSize
		{
			get
			{
				return _fontSettings.OutlineSize;
			}
			set
			{
				if (value < 0)
					value = 0;
				if (value > 16)
					value = 16;

				if (_fontSettings.OutlineSize != value)
				{
					_fontSettings.OutlineSize = value;
					DispatchUpdateNotification();
				}
			}
		}

		/// <summary>
		/// Function to set the base color for the font glyphs.
		/// </summary>
		[Category("Appearance"), Description("Sets the outline color for the glyphs with outlines.  Note that the outline size must be greater than 0 before it is applied to the glyph."), DefaultValue(0), Editor(typeof(ColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(RGBATypeConverter))]
		public Color OutlineColor
		{
			get
			{
				return _fontSettings.OutlineColor;
			}
			set
			{
				if (_fontSettings.OutlineColor.ToColor() != value)
				{
					_fontSettings.OutlineColor = value ;
					DispatchUpdateNotification();
				}
			}
		}

		/// <summary>
		/// Property to set or return the font style.
		/// </summary>
		[Editor(typeof(FontStyleEditor), typeof(UITypeEditor)), Category("Appearance"), Description("Sets whether the font should be bolded, underlined, italicized, or strike through"), DefaultValue(FontStyle.Regular)]
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
		[Editor(typeof(FontFamilyEditor), typeof(UITypeEditor)), RefreshProperties(System.ComponentModel.RefreshProperties.Repaint), Category("Design"), Description("The font family to use for the font.")]
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
		[Category("Design"), Description("The size of the font.  This value may be in Points or in Pixels depending on the font height mode."), DefaultValue(9.0f)]
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
					CheckTextureSize();
					DispatchUpdateNotification();
				}
			}
		}

		/// <summary>
		/// Property to set or return the texture size for the font.
		/// </summary>
		[Category("Design"), Description("The size of the textures used to hold the sprite glyphs."), DefaultValue(typeof(Size), "256,256")]
		public Size FontTextureSize
		{
			get
			{
				return _fontSettings.TextureSize;
			}
			set
			{
				if (value.Width < 16)
					value.Width = 16;
				if (value.Height < 16)
					value.Height = 16;
				if (value.Width >= Program.Graphics.Textures.MaxWidth)
					value.Width = Program.Graphics.Textures.MaxWidth - 1;
				if (value.Height >= Program.Graphics.Textures.MaxHeight)
					value.Height = Program.Graphics.Textures.MaxHeight - 1;

				if (_fontSettings.TextureSize != value)
				{
					_fontSettings.TextureSize = value;
					CheckTextureSize();
					DispatchUpdateNotification();
				}
			}
		}

		/// <summary>
		/// Property to set or return the anti-alias mode.
		/// </summary>
		[Category("Appearance"), Description("Sets the type of anti-aliasing to use for the font.  High Quality (HQ) produces the best output, but doesn't get applied to fonts at certain sizes.  Non HQ, will attempt to anti-alias regardless, but the result will be worse."), DefaultValue(FontAntiAliasMode.AntiAliasHQ)]
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
		[DisplayName("FontHeightMode"), Category("Design"), Description("Sets whether to use points or pixels to determine the size of the font."), DefaultValue(typeof(FontHeightMode), "Points")]
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
					CheckTextureSize();
					DispatchUpdateNotification();
				}
			}
		}

		/// <summary>
		/// Property to return the type of document.
		/// </summary>
		public override string DocumentType
		{
			get
			{
				return "Font";
			}
		}

		/// <summary>
		/// Property to set or return the list of characters that need to be turned into glyphs.
		/// </summary>
		[Category("Design"), Description("Sets the list of characters to convert into glyphs."), TypeConverter(typeof(CharacterSetTypeConverter)), Editor(typeof(CharacterSetEditor), typeof(UITypeEditor))]
		public IEnumerable<char> Characters
		{
			get
			{
				return _fontSettings.Characters;
			}
			set
			{
				if ((value == null) || (value.Count() == 0))
					value = new char[] {' '};

				if (_fontSettings.Characters != value)
				{
					_fontSettings.Characters = value;
					DispatchUpdateNotification();
				}
			}
		}

		/// <summary>
		/// Property to set or return the contrast of the glyphs.
		/// </summary>
		[Category("Appearance"), Description("Sets the contrast for anti-aliased glyphs."), DefaultValue(4)]
		public int TextContrast
		{
			get
			{
				return _fontSettings.TextContrast;
			}
			set
			{
				if (value < 0)
					value = 0;
				if (value > 12)
					value = 12;

				if (value != _fontSettings.TextContrast)
				{
					_fontSettings.TextContrast = value;
					DispatchUpdateNotification();
				}
			}
		}

		/// <summary>
		/// Property to set or return the text color for the example text.
		/// </summary>
		[Category("Example Text"), Description("Sets the color for the example text."), DefaultValue(0xFF000000), TypeConverter(typeof(RGBATypeConverter)), Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public Color TextColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the text should have a shadow or not.
		/// </summary>
		[Category("Example Text"), Description("Sets whether a drop shadow should be added to the text."), DefaultValue(false), RefreshProperties(RefreshProperties.All)]
		public bool DropShadow
		{
			get
			{
				return _dropShadowEnabled;
			}
			set
			{
				_dropShadowEnabled = value;
				TypeDescriptor["DropShadowOffset"].IsReadOnly = !_dropShadowEnabled;
			}
		}

		/// <summary>
		/// Property to set or return the offset of the shadow.
		/// </summary>
		[Category("Example Text"), Description("Sets the offset, in pixels, of the drop shadow."), TypeConverter(typeof(PointFConverter))]
		public PointF DropShadowOffset
		{
			get
			{
				return _dropOffset;
			}
			set
			{
				_dropOffset = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to limit the font texture size based on the font size.
		/// </summary>
		private void CheckTextureSize()
		{
			float fontSize = FontSize;
			if (UsePointSize == FontHeightMode.Points)
				fontSize = (float)System.Math.Ceiling(GorgonFontSettings.GetFontHeight(fontSize, OutlineSize));
			else
				fontSize += OutlineSize;

			if ((fontSize > FontTextureSize.Width) || (fontSize > FontTextureSize.Height))
				FontTextureSize = new Size((int)fontSize, (int)fontSize);
		}

		/// <summary>
		/// Function to initialize graphics and other items for the document.
		/// </summary>
		protected override void LoadGraphics()
		{
			_textArea = Program.Graphics.Output.CreateSwapChain("TextArea.SwapChain", new GorgonSwapChainSettings()
				{
					Window = _fontWindow.panelText,
					Format = BufferFormat.R8G8B8A8_UIntNormal
				});

			_ibar = Program.Graphics.Textures.FromGDIBitmap("IBar", Properties.Resources.IBar);
		}

		/// <summary>
		/// Function to remove graphics and other items for the document.
		/// </summary>
		protected override void UnloadGraphics()
		{
			if (_textArea != null)
				_textArea.Dispose();
			if (_target != null)
				_target.Dispose();
			_textArea = null;
			_target = null;
		}

		/// <summary>
		/// Function to draw to the control.
		/// </summary>
		/// <param name="timing">Timing data.</param>
		/// <returns>
		/// Number of vertical retraces to wait.
		/// </returns>
		protected override int Draw(Diagnostics.GorgonFrameRate timing)
		{
			if (_font == null)
				return 3;

			Renderer.Drawing.SmoothingMode = Renderers.SmoothingMode.Smooth;

			GorgonTexture2D texture = _font.Textures[_fontWindow.CurrentTexture];
			IEnumerable<GorgonGlyph> glyphs = null;
			Rectangle bounds = new Rectangle(0, 0, (int)System.Math.Ceiling(texture.Settings.Width * _fontWindow.Zoom), (int)System.Math.Ceiling(texture.Settings.Height * _fontWindow.Zoom));

			glyphs = from GorgonGlyph glyph in _font.Glyphs
						where glyph.Texture == texture && !char.IsWhiteSpace(glyph.Character)
						select glyph;


			_fontWindow.panelDisplay.AutoScrollMinSize = bounds.Size;
								
			Renderer.Target = _target;
			_target.Clear(Color.Black);

			foreach (var glyph in glyphs)
				Renderer.Drawing.DrawRectangle(new RectangleF(glyph.GlyphCoordinates.Left - 1, glyph.GlyphCoordinates.Top - 1, glyph.GlyphCoordinates.Width + 1, glyph.GlyphCoordinates.Height + 1), Color.Red);
			Renderer.Drawing.Blit(texture, Vector2.Zero);
				
			Renderer.Target = null;

			if ((_fontWindow.panelDisplay.HorizontalScroll.Visible) || (_fontWindow.panelDisplay.VerticalScroll.Visible))
				Renderer.Drawing.Blit(_target, _fontWindow.panelDisplay.AutoScrollPosition, new Vector2(_fontWindow.Zoom, _fontWindow.Zoom));
			else
				Renderer.Drawing.Blit(_target, new Vector2(_fontWindow.panelDisplay.ClientSize.Width / 2 - bounds.Size.Width / 2, _fontWindow.panelDisplay.ClientSize.Height / 2 - bounds.Size.Height / 2), new Vector2(_fontWindow.Zoom, _fontWindow.Zoom));				

			// Draw preview text.
			if ((!string.IsNullOrEmpty(_fontWindow.EditText)) || (_fontWindow.EditMode))
			{
				Vector2 position = Vector2.Zero;
				IList<string> lines = _fontWindow.EditText.Split(new char[] {'\n'});
				_textArea.Clear(Color.White);
				_text.ShadowEnabled = DropShadow;
				_text.ShadowOffset = DropShadowOffset;
				_text.Color = TextColor;
				_text.TextRectangle = new RectangleF(Vector2.Zero, _fontWindow.ClientSize);
				Renderer.Target = _textArea;

				if (_iBarTimer.Milliseconds > 500)
				{
					_iBarTimer.Reset();
					_showIBar = !_showIBar;
				}

				foreach (var line in lines)
				{
					_text.Position = position;
					_text.Text = line;
					_text.Draw();
					position.Y += _font.FontHeight;
				}

				// Draw i-bar.
				if ((_fontWindow.EditMode) && (_fontWindow.panelText.Focused) && (_showIBar))
				{
					float ibarAspect = (float)_ibar.Settings.Width / (float)_ibar.Settings.Height;
					Vector2 size = _text.MeasureText(lines[lines.Count - 1], false, 0);
					Renderer.Drawing.FilledRectangle(new RectangleF(size.X, position.Y - _font.FontHeight, _font.FontHeight * ibarAspect, _font.FontHeight), Color.White, _ibar, new RectangleF(Vector2.Zero, _ibar.Settings.Size));
				}

				Renderer.Target = null;
				
				_textArea.Flip();
			}

			Renderer.Drawing.SmoothingMode = Renderers.SmoothingMode.None;
			return 3;
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
			GorgonFont newFont = null;

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

			try
			{
				newFont = Program.Graphics.Textures.CreateFont(Name, new GorgonFontSettings()
					{
						AntiAliasingMode = _fontSettings.AntiAliasingMode,
						BaseColors = _fontSettings.BaseColors,
						Brush = _fontSettings.Brush,
						Characters = _fontSettings.Characters,
						DefaultCharacter = _fontSettings.DefaultCharacter,
						FontFamilyName = _fontSettings.FontFamilyName,
						FontHeightMode = _fontSettings.FontHeightMode,
						FontStyle = _fontSettings.FontStyle,
						OutlineColor = _fontSettings.OutlineColor,
						OutlineSize = _fontSettings.OutlineSize,
						PackingSpacing = _fontSettings.PackingSpacing,
						Size = _fontSettings.Size,
						TextContrast = _fontSettings.TextContrast,
						TextureSize = _fontSettings.TextureSize
					});
			}
			catch
			{
				// Restore the settings.
				if (_font != null)
					_fontSettings = _font.Settings;

				throw;
			}			


			if (_font != null)
				_font.Dispose();

			_fontWindow.CurrentFont = _font = newFont;
			
			if (_target != null)
				_target.Dispose();

			_target = Program.Graphics.Output.CreateRenderTarget("Surface", new GorgonRenderTargetSettings()
				{
					Width = _fontSettings.TextureSize.Width,
					Height = _fontSettings.TextureSize.Height,
					Format = BufferFormat.R8G8B8A8_UIntNormal					
				});

			_text = Renderer.Renderables.CreateText("Text");
			_text.Font = _font;			
			_text.Color = Color.Black;
			NeedsSave = true;
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
			_iBarTimer = new GorgonTimer();
			_fontWindow = new controlFontDisplay();
			_fontSettings = new GorgonFontSettings();
			DropShadow = false;
			_dropOffset = new Vector2(1, 1);
			TextColor = Color.Black;
			HasProperties = true;
			_fontWindow.Dock = System.Windows.Forms.DockStyle.Fill;
			UpdateControl(_fontWindow, _fontWindow.panelDisplay);
		}
		#endregion
	}
}
