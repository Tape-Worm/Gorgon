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
// Created: Wednesday, November 21, 2007 12:32:50 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Drawing = System.Drawing;
using GorgonLibrary;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a font to be used with Gorgon.
	/// </summary>
	public class Font
		: NamedObject, IDisposable
	{
		#region Variables.
		private bool _disposed = false;											// Flag to indicate that we're already disposed.
		private string _charList = string.Empty;								// Character list.
		private float _fontSize = 9.0f;											// Font size.
		private SortedList<char, Glyph> _glyphData = null;						// Glyph data.
		private Drawing.Size _maxImageSize = Drawing.Size.Empty;				// Backing image size.
		private bool _antiAlias = false;										// Flag to enable anti-aliasing on the font.
		private int _widthPadding = 0;											// Glyph width padding.
		private int _heightPadding = 0;											// Glyph height padding.
		private int _leftPadding = 0;											// Glyph left padding.
		private int _topPadding = 0;											// Glyph top padding.
		private bool _bold = false;												// Bold flag.
		private bool _underline = false;										// Underlined flag.
		private bool _italics = false;											// Italics flag.
		private bool _strikeout = false;										// Strike out flag.
		private Drawing.Color _color1;											// Font base color.
		private Drawing.Color _color2;											// Font gradient color.
		private float _gradientAngle = 0.0f;									// Angle of the gradient.
		private Drawing.Brush _userBrush;										// User-defined brush.
		private float _outlineWidth = 0.0f;										// Outline width.
		private Drawing.Color _outlineColor = Drawing.Color.Black;				// Outline color.
		private bool _needUpdate;												// Flag indicate that the font needs to be updated.
		private Drawing.FontFamily _family = null;								// Font family.
		private int _lineHeight;												// Font line height.
		private string _invalidChars = string.Empty;							// Characters.
		private int _imageCount = 0;											// Number of images.
		private int _ascent = 0;												// Ascent of the font (in pixels).
		private int _descent = 0;												// Descent of the font (in pixels).
		private string _familyName = string.Empty;								// Font family name.
		private int _characterHeight = 0;										// Maximum character height in pixels.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the font needs to be updated or not.
		/// </summary>
		internal bool NeedsUpdate
		{
			get
			{
				return _needUpdate;
			}
		}

		/// <summary>
		/// Property to set or return the font family name.
		/// </summary>
		public string FamilyName
		{
			get
			{
				if (_family == null)
					return _familyName;
				else
					return _family.Name;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");
				_family = null;
				_familyName = value;
				_needUpdate = true;
			}
		}

		/// <summary>
		/// Property to return the number of resources the font consumes.
		/// </summary>
		/// <remarks>Resources mean the number of backing images that are required to hold the glyphs.  Too many images can lead to texture thrashing on the card and really hinder performance.  
		/// This property will allow the user to monitor how many resources the font is using and allow a chance to adjust by manipulating the <see cref="FontSize"/> or raising the <see cref="MaxFontImageWidth"/> 
		/// and/or the <see cref="MaxFontImageWidth"/>.<para>This value should never be less than 1 and ideally only return 1.</para></remarks>
		public int ResourceCount
		{
			get
			{
				return _imageCount;
			}
		}

		/// <summary>
		/// Property to return the ascent of the font (in pixels).
		/// </summary>
		public int Ascent
		{
			get
			{
				if (_needUpdate)
					Refresh();
				return _ascent;
			}
		}

		/// <summary>
		/// Property to return the descent of the font (in pixels).
		/// </summary>
		public int Descent
		{
			get
			{
				if (_needUpdate)
					Refresh();
				return _descent;
			}
		}

		/// <summary>
		/// Property to return the line height of the font (in pixels).
		/// </summary>
		public int LineHeight
		{
			get
			{
				if (_needUpdate)
					Refresh();
				return _lineHeight;
			}
		}

		/// <summary>
		/// Property to return the maximum character height.
		/// </summary>
		/// <remarks>This is the height of the character itself, not the cell of the character.</remarks>
		public int CharacterHeight
		{
			get
			{
				if (_needUpdate)
					Refresh();
				return _characterHeight;
			}
		}

		/// <summary>
		/// Property to set or return the outline color.
		/// </summary>
		public Drawing.Color OutlineColor
		{
			get
			{
				return _outlineColor;
			}
			set
			{
				_outlineColor = value;
				_needUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the outline size.
		/// </summary>
		public float OutlineWidth
		{
			get
			{
				return _outlineWidth;
			}
			set
			{
				if (value < 0.0f)
					value = 0.0f;
				if (value > 4.0f)
					value = 4.0f;

				_outlineWidth = value;
				_needUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return a user defined brush used to paint the font.
		/// </summary>
		/// <remarks>This property will make a clone of the original brush and will use that clone internally.<para>Please note that the user brush will override any color/gradient settings on the font.</para></remarks>
		public Drawing.Brush UserBrush
		{
			get
			{
				return _userBrush;
			}
			set
			{
				if (value != null)
					_userBrush = value.Clone() as Drawing.Brush;
				else
				{
					if (_userBrush != null)
						_userBrush.Dispose();
					_userBrush = null;
				}

				_needUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the angle of the gradient.
		/// </summary>
		public float GradientAngle
		{
			get
			{
				return _gradientAngle;
			}
			set
			{
				_gradientAngle = value;
				_needUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the base color for the font glyphs.
		/// </summary>
		public Drawing.Color BaseColor
		{
			get
			{
				return _color1;
			}
			set
			{
				_color1 = value;
				_needUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the gradient color for the font glyphs.
		/// </summary>
		public Drawing.Color GradientColor
		{
			get
			{
				return _color2;
			}
			set
			{
				_color2 = value;
				_needUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the font is bolded.
		/// </summary>
		public bool Bold
		{
			get
			{
				return _bold;
			}
			set
			{
				_bold = value;
				_needUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the font is underlined.
		/// </summary>
		public bool Underline
		{
			get
			{
				return _underline;
			}
			set
			{
				_underline = value;
				_needUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the font is italicized.
		/// </summary>
		public bool Italic
		{
			get
			{
				return _italics;
			}
			set
			{
				_italics = value;
				_needUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the font is striked out.
		/// </summary>
		public bool Strikeout
		{
			get
			{
				return _strikeout;
			}
			set
			{
				_strikeout = value;
				_needUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the amount of offset of the left side of a glyph on the backing image.
		/// </summary>
		public int GlyphLeftOffset
		{
			get
			{
				return _leftPadding;
			}
			set
			{
				_leftPadding = value;
				_needUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the amount of offset of the top side of a glyph on the backing image.
		/// </summary>
		public int GlyphTopOffset
		{
			get
			{
				return _topPadding;
			}
			set
			{
				_topPadding = value;
				_needUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the amount of padding between the glyph width on the backing image.
		/// </summary>
		public int GlyphWidthPadding
		{
			get
			{
				return _widthPadding;
			}
			set
			{
				_widthPadding = value;
				_needUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the amount of padding between the glyph heights on the backing image.
		/// </summary>
		public int GlyphHeightPadding
		{
			get
			{
				return _heightPadding;
			}
			set
			{
				_heightPadding = value;
				_needUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the font will be anti-aliased or not.
		/// </summary>
		public bool AntiAlias
		{
			get
			{
				return _antiAlias;
			}
			set
			{
				_antiAlias = value;

				_needUpdate = true;
			}
		}
		
		/// <summary>
		/// Property to set or return the font image width.
		/// </summary>
		public int MaxFontImageWidth
		{
			get
			{
				return _maxImageSize.Width;
			}
			set
			{
				if ((!Gorgon.CurrentDriver.SupportNonPowerOfTwoTexture) || (Gorgon.CurrentDriver.SupportNonPowerOfTwoTextureConditional))
					value = Image.ResizePowerOf2(value, _maxImageSize.Height).Width;

				if ((value < 1) || (value > Gorgon.CurrentDriver.MaximumTextureWidth))
					throw new ImageSizeException("Font image width must be between 1 - " + Gorgon.CurrentDriver.MaximumTextureWidth.ToString() + ".");

				_maxImageSize = new Drawing.Size(value, _maxImageSize.Height);

				_needUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the font image width.
		/// </summary>
		public int MaxFontImageHeight
		{
			get
			{
				return _maxImageSize.Height;
			}
			set
			{
				if ((!Gorgon.CurrentDriver.SupportNonPowerOfTwoTexture) || (Gorgon.CurrentDriver.SupportNonPowerOfTwoTextureConditional))
					value = Image.ResizePowerOf2(_maxImageSize.Width, value).Height;

				if ((value < 1) || (value > Gorgon.CurrentDriver.MaximumTextureHeight))
					throw new ImageSizeException("Font image height must be between 1 - " + Gorgon.CurrentDriver.MaximumTextureHeight.ToString() + ".");

				_maxImageSize = new Drawing.Size(_maxImageSize.Width, value);

				_needUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the character list.
		/// </summary>
		public string CharacterList
		{
			get
			{
				return _charList;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
					value = " ";

				// ALWAYS include a space.
				if (value.IndexOf(" ") == -1)
					value = " " + value;

				_charList = value;

				_needUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the font size.
		/// </summary>
		public float FontSize
		{
			get
			{
				return _fontSize;
			}
			set
			{
				if (value < 0.0f)
					value = 0.0f;

				_fontSize = value;

				_needUpdate = true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the width of a character.
		/// </summary>
		/// <param name="context">Context from which the character will be measured.</param>
		/// <param name="font">Font to use.</param>
		/// <param name="c">Character to measure.</param>
		/// <returns>The width of the character cell.</returns>
		private int GetCharWidth(Drawing.Bitmap context, Drawing.Font font,char c)
		{
			using (Drawing.Graphics g = Drawing.Graphics.FromImage(context))
				return (int)Math.Ceiling(g.MeasureString(c.ToString(), font).Width);
		}

		/// <summary>
		/// Function to draw the font.
		/// </summary>
		/// <param name="glyph">Glyph data.</param>
		/// <param name="gdiFont">GDI font to use.</param>
		/// <param name="fontData">Font bitmap to use.</param>
		private void DrawFont(Glyph glyph, Drawing.Font gdiFont, Drawing.Bitmap fontData)
		{
			Drawing.Brush brush = null;		// Font brush.

			using (Drawing.Graphics g = Drawing.Graphics.FromImage(fontData))
			{				
				if (_antiAlias)
				{
					g.TextContrast = 1;
					g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias;					

					if (gdiFont.Size <= 18.0f)
					{
						g.TextContrast = 0;
						g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit;
					}
				}
				else
				{
					g.TextContrast = 0;
					g.TextRenderingHint = Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
					g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
				}

				// Draw each character.
				if (_userBrush == null)
				{
					if (_color1 != _color2)
						brush = new Drawing.Drawing2D.LinearGradientBrush(glyph.GlyphDimensions, _color1, _color2, _gradientAngle, true);
					else
						brush = new Drawing.SolidBrush(_color1);
				}
				else
					brush = _userBrush;
				
				// Get character position on the image.
				Drawing.PointF charPos = glyph.GlyphDimensions.Location;

				// Adjust height.  For some reason, some fonts appear shifted down by 1 pixel.
				charPos.X += _leftPadding;
				charPos.Y += _topPadding;

				// If we have an outline specified, then draw the text using paths.
				if ((_outlineWidth > 0.0f) && (_outlineColor.A > 0))
				{
					using (Drawing.Pen outlinePen = new Drawing.Pen(_outlineColor, _outlineWidth))
					{
						using (Drawing.Drawing2D.GraphicsPath outlinePath = new Drawing.Drawing2D.GraphicsPath())
						{
							if (_antiAlias)
								g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

							// Create outline.
							outlinePath.AddString(glyph.GlyphCharacter.ToString(), gdiFont.FontFamily, (int)gdiFont.Style, (g.DpiY / 72.0f) * gdiFont.Size, charPos, null);

							// Draw outline.
							g.FillPath(brush, outlinePath);
							g.DrawPath(outlinePen, outlinePath);
						}
					}
				}
				else
					g.DrawString(glyph.GlyphCharacter.ToString(), gdiFont, brush, charPos);
									
				if ((brush != null) && (_userBrush == null))
					brush.Dispose();
				brush = null;
			}

			if ((brush != null) && (_userBrush == null))
				brush.Dispose();
			brush = null;
		}

		/// <summary>
		/// Function to create an image to hold the glyphs.
		/// </summary>
		/// <param name="dims">Dimensions of the image.</param>
		/// <returns>An image to hold the character glyphs.</returns>
		private Image CreateGlyphImage(Drawing.Size dims)
		{
			ImageBufferFormats fontImageFormat;			// Font image format.

			// Copy the font data into our font image.
			if (Image.ValidateFormat(ImageBufferFormats.BufferRGB888A8, ImageType.Normal) == ImageBufferFormats.BufferRGB888A8)
				fontImageFormat = ImageBufferFormats.BufferRGB888A8;
			else
			{
				if (Image.ValidateFormat(ImageBufferFormats.BufferRGB555A1, ImageType.Normal) == ImageBufferFormats.BufferRGB332A8)
					fontImageFormat = ImageBufferFormats.BufferRGB332A8;
				else
				{
					if (Image.ValidateFormat(ImageBufferFormats.BufferRGB444A4, ImageType.Normal) == ImageBufferFormats.BufferRGB444A4)
						fontImageFormat = ImageBufferFormats.BufferRGB444A4;
					else
						if (Image.ValidateFormat(ImageBufferFormats.BufferRGB332A8, ImageType.Normal) == ImageBufferFormats.BufferRGB555A1)
							fontImageFormat = ImageBufferFormats.BufferRGB555A1;
						else
							throw new FontImageFormatNotValidException();
				}
			}

			// Create the backing image.
			return new Image(Name + FontSize.ToString("0.0") + ".BackingImage." + _imageCount.ToString(), ImageType.Normal, dims.Width, dims.Height, fontImageFormat, false, true);
		}

		/// <summary>
		/// Function to update the font.
		/// </summary>
		/// <param name="chars">Characters to add.</param>
		private void UpdateFont(string chars)
		{
			Drawing.Font gdiFont = null;				// GDI+ font to use.
			Drawing.Size newSize = Drawing.Size.Empty;	// New backing image size.
			GlyphTree tree = null;						// Glyph tree.
			Drawing.FontStyle style;					// Font style.
			Image fontImage = null;						// Image for the font glyphs.

			if (!_needUpdate)
				return;

			style = Drawing.FontStyle.Regular;

			if (_bold)
				style |= Drawing.FontStyle.Bold;
			if (_underline)
				style |= Drawing.FontStyle.Underline;
			if (_italics)
				style |= Drawing.FontStyle.Italic;
			if (_strikeout)
				style |= Drawing.FontStyle.Strikeout;

			// Grab from family information if we have it.
			if (_family == null)
				gdiFont = new Drawing.Font(_familyName, _fontSize, style, Drawing.GraphicsUnit.Point);
			else
				gdiFont = new Drawing.Font(_family, _fontSize, style, Drawing.GraphicsUnit.Point);

			// Calculate required image size, try to fit at least 16x16 glyphs on here.
			int lineHeight = (int)Math.Ceiling(((double)gdiFont.Height * (double)gdiFont.FontFamily.GetLineSpacing(style)) / (double)gdiFont.FontFamily.GetEmHeight(style));
			
			newSize.Width = (lineHeight + _leftPadding + _widthPadding) * 16;
			newSize.Height = (lineHeight + _topPadding + _heightPadding) * 16;

			if (newSize.Width > _maxImageSize.Width)
				newSize.Width = _maxImageSize.Width;
			if (newSize.Height > _maxImageSize.Height)
				newSize.Height = _maxImageSize.Height;

			// Resize to power of two if the card requires it.
			if ((!Gorgon.CurrentDriver.SupportNonPowerOfTwoTexture) || (Gorgon.CurrentDriver.SupportNonPowerOfTwoTextureConditional))
				newSize = Image.ResizePowerOf2(newSize.Width, newSize.Height);

			// Ensure the backing image will fit into the valid texture dimensions allowed by the video card.
			if (newSize.Width > Gorgon.CurrentDriver.MaximumTextureWidth)
				newSize.Width = Gorgon.CurrentDriver.MaximumTextureWidth;
			if (newSize.Height > Gorgon.CurrentDriver.MaximumTextureHeight)
				newSize.Height = Gorgon.CurrentDriver.MaximumTextureHeight;
			if (newSize.Width < 1)
				newSize.Width = 1;
			if (newSize.Height < 1)
				newSize.Height = 1;

			// Create the image.
			fontImage = CreateGlyphImage(newSize);

			// Create our font image.
			using (Drawing.Bitmap fontData = new Drawing.Bitmap(newSize.Width, newSize.Height, Drawing.Imaging.PixelFormat.Format32bppArgb))
			{				
				// Calculate actual text cell height.
				using (Drawing.Graphics g = Drawing.Graphics.FromImage(fontData))
				{
					g.Clear(Drawing.Color.Transparent);

					g.PageUnit = Drawing.GraphicsUnit.Pixel;

					// Perform calculations.
					_lineHeight = (int)Math.Ceiling(((double)gdiFont.GetHeight(g) * (double)gdiFont.FontFamily.GetLineSpacing(style)) / (double)gdiFont.FontFamily.GetEmHeight(style));
					_ascent = (int)Math.Ceiling(((double)gdiFont.FontFamily.GetCellAscent(style) * (double)gdiFont.GetHeight(g)) / (double)gdiFont.FontFamily.GetEmHeight(style));
					_descent = (int)Math.Ceiling(((double)gdiFont.FontFamily.GetCellDescent(style) * (double)gdiFont.GetHeight(g)) / (double)gdiFont.FontFamily.GetEmHeight(style));

					if (_lineHeight < gdiFont.Height)
						_lineHeight = gdiFont.Height;
				}

				tree = new GlyphTree(newSize);

				foreach (char c in chars)
				{
					Glyph newGlyph;						// New glyph.
					Drawing.RectangleF newCoords;		// Scaled coordinates.
					Drawing.RectangleF glyphCoords;		// Unscaled coordinates.
					int charWidth = 0;					// Character width.

					if (!_glyphData.ContainsKey(c))
					{
						charWidth = GetCharWidth(fontData, gdiFont, c);
						glyphCoords = tree.Add(new Drawing.Size(charWidth + _widthPadding + _leftPadding, _lineHeight + _heightPadding + _topPadding));
						if (glyphCoords != Drawing.Rectangle.Empty)
						{
							if (glyphCoords.Width < 0.1f)
								glyphCoords = tree.Add(new Drawing.Size(GetCharWidth(fontData, gdiFont, ' ') + _widthPadding + _leftPadding, _lineHeight + _heightPadding + _topPadding));
							newCoords = glyphCoords;
							newCoords.X = ((newCoords.X - 0.5f) / (float)newSize.Width);
							newCoords.Y = ((newCoords.Y - 0.5f) / (float)newSize.Height);
							newCoords.Width = ((newCoords.Width) / (float)newSize.Width);
							newCoords.Height = ((newCoords.Height) / (float)newSize.Height);

							newGlyph = new Glyph(c, gdiFont, fontImage, glyphCoords, newCoords);

							// Ensure that only glyphs that have a width or spaces are added.  We require spaces.
							if ((newGlyph.Size.Width >= 0.1f) || (c == ' '))
								_glyphData.Add(c, newGlyph);

							// Draw the glyph.
							DrawFont(newGlyph, gdiFont, fontData);

							if (_invalidChars.IndexOf(c) > -1)
								_invalidChars = _invalidChars.Remove(_invalidChars.IndexOf(c), 1);
						}
						else
						{
							if (_invalidChars.IndexOf(c) < 0)
								_invalidChars += c.ToString();
						}
					}
				}

				// Copy the source bitmap.
				fontImage.Copy(fontData);
			}

			_needUpdate = false;
		}

		/// <summary>
		/// Function to release the resources.
		/// </summary>
		internal void ReleaseResources()
		{
			// Destroy image resources.
			foreach (KeyValuePair<char, Glyph> glyph in _glyphData)
			{
				if (glyph.Value.GlyphImage != null)
					glyph.Value.GlyphImage.Dispose();
			}

			_needUpdate = true;
		}

		/// <summary>
		/// Function to refresh the font data.
		/// </summary>
		public void Refresh()
		{	
			if (_needUpdate)
			{
				_imageCount = 1;
				_glyphData.Clear();

				UpdateFont(_charList);

				// Add any invalid characters.
				while (_invalidChars.Length > 0)
				{
					_needUpdate = true;
					UpdateFont(_invalidChars);
					_imageCount++;
				}

				// Reset character height.
				_characterHeight = 0;

				foreach (KeyValuePair<char, Glyph> glyph in _glyphData)
				{
					if (glyph.Value.Size.Height > _characterHeight)
						_characterHeight = glyph.Value.Size.Height;
				}

				// Use line height if we can't find the maximum character height.
				if (_characterHeight == 0)
					_characterHeight = LineHeight;
			}
		}

		/// <summary>
		/// Function to retrieve the glyph for the specified character.
		/// </summary>
		/// <param name="c">Character to retrieve glyph from.</param>
		/// <returns>The glyph associated with the character.</returns>
		public Glyph GetGlyph(char c)
		{
			try
			{
				if (_glyphData.ContainsKey(c))
					return _glyphData[c];
				else
					return _glyphData[' '];
			}
			catch (Exception ex)
			{
				throw new FontGlyphNotFoundException(c, ex);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Font"/> class.
		/// </summary>
		/// <param name="fontName">Name of the font to use.</param>
		/// <param name="family">Font family.</param>
		/// <param name="fontSize">Font size in points.</param>
		/// <param name="antiAlias">TRUE to anti-alias the font, FALSE to draw without anti-aliasing.</param>
		/// <param name="bold">TRUE to set the font to bold, FALSE to leave as is.</param>
		/// <param name="underline">TRUE to set the font to underlined, FALSE to leave as is.</param>
		/// <param name="italic">TRUE to set the font to italics, FALSE to leave as is.</param>
		public Font(string fontName, string family, float fontSize, bool antiAlias, bool bold, bool underline, bool italic)
			: base(fontName)
		{
			if (FontCache.Fonts.Contains(fontName))
				throw new FontAlreadyLoadedException(fontName);

			_familyName = family;
			_antiAlias = antiAlias;
			_bold = bold;
			_underline = underline;
			_italics = italic;
			_fontSize = fontSize;
			_color1 = Drawing.Color.White;
			_color2 = Drawing.Color.White;
			_glyphData = new SortedList<char, Glyph>();

			// Default to a 1024x1024 image.
			_maxImageSize = new Drawing.Size(512, 512);
			
			// Initialize the character list.
			for (int i = 32; i < 128; i++)
				_charList += Convert.ToChar(i);

			_needUpdate = true;

			// Add to cache.
			FontCache.Fonts.Add(this);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Font"/> class.
		/// </summary>
		/// <param name="fontName">Name of the font to use.</param>
		/// <param name="family">Font family.</param>
		/// <param name="fontSize">Font size in points.</param>
		/// <param name="antiAlias">TRUE to anti-alias the font, FALSE to draw without anti-aliasing.</param>
		/// <param name="bold">TRUE to set the font to bold, FALSE to leave as is.</param>
		/// <param name="underline">TRUE to set the font to underlined, FALSE to leave as is.</param>
		public Font(string fontName, string family, float fontSize, bool antiAlias, bool bold, bool underline)
			: this(fontName, family, fontSize, antiAlias, bold, underline, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Font"/> class.
		/// </summary>
		/// <param name="fontName">Name of the font to use.</param>
		/// <param name="family">Font family.</param>
		/// <param name="fontSize">Font size in points.</param>
		/// <param name="antiAlias">TRUE to anti-alias the font, FALSE to draw without anti-aliasing.</param>
		/// <param name="bold">TRUE to set the font to bold, FALSE to leave as is.</param>
		public Font(string fontName, string family, float fontSize, bool antiAlias, bool bold)
			: this(fontName, family, fontSize, antiAlias, bold, false, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Font"/> class.
		/// </summary>
		/// <param name="fontName">Name of the font to use.</param>
		/// <param name="family">Font family.</param>
		/// <param name="fontSize">Font size in points.</param>
		/// <param name="antiAlias">TRUE to anti-alias the font, FALSE to draw without anti-aliasing.</param>
		public Font(string fontName, string family, float fontSize, bool antiAlias)
			: this(fontName, family, fontSize, antiAlias, false, false, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Font"/> class.
		/// </summary>
		/// <param name="fontName">Name of the font to use.</param>
		/// <param name="family">Font family.</param>
		/// <param name="fontSize">Font size in points.</param>
		public Font(string fontName, string family, float fontSize)
			: this(fontName, family, fontSize, true, false, false, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Font"/> class.
		/// </summary>
		/// <param name="fontName">Name of the font to use.</param>
		/// <param name="fontFamily">Family that the font belongs to.</param>
		/// <param name="fontSize">Font size in points.</param>
		/// <param name="antiAlias">TRUE to anti-alias the font, FALSE to draw without anti-aliasing.</param>
		/// <param name="bold">TRUE to set the font to bold, FALSE to leave as is.</param>
		/// <param name="underline">TRUE to set the font to underlined, FALSE to leave as is.</param>
		/// <param name="italic">TRUE to set the font to italics, FALSE to leave as is.</param>
		public Font(string fontName, Drawing.FontFamily fontFamily, float fontSize, bool antiAlias, bool bold, bool underline, bool italic)
			: this(fontName, fontFamily.Name, fontSize, antiAlias, bold, underline, italic)
		{
			_family = fontFamily;
			_needUpdate = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Font"/> class.
		/// </summary>
		/// <param name="fontName">Name of the font to use.</param>
		/// <param name="fontFamily">Family that the font belongs to.</param>
		/// <param name="fontSize">Font size in points.</param>
		/// <param name="antiAlias">TRUE to anti-alias the font, FALSE to draw without anti-aliasing.</param>
		/// <param name="bold">TRUE to set the font to bold, FALSE to leave as is.</param>
		/// <param name="underline">TRUE to set the font to underlined, FALSE to leave as is.</param>
		public Font(string fontName, Drawing.FontFamily fontFamily, float fontSize, bool antiAlias, bool bold, bool underline)
			: this(fontName, fontFamily, fontSize, antiAlias, bold, underline, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Font"/> class.
		/// </summary>
		/// <param name="fontName">Name of the font to use.</param>
		/// <param name="fontFamily">Family that the font belongs to.</param>
		/// <param name="fontSize">Font size in points.</param>
		/// <param name="antiAlias">TRUE to anti-alias the font, FALSE to draw without anti-aliasing.</param>
		/// <param name="bold">TRUE to set the font to bold, FALSE to leave as is.</param>
		public Font(string fontName, Drawing.FontFamily fontFamily, float fontSize, bool antiAlias, bool bold)
			: this(fontName, fontFamily, fontSize, antiAlias, bold, false, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Font"/> class.
		/// </summary>
		/// <param name="fontName">Name of the font to use.</param>
		/// <param name="fontFamily">Family that the font belongs to.</param>
		/// <param name="fontSize">Font size in points.</param>
		/// <param name="antiAlias">TRUE to anti-alias the font, FALSE to draw without anti-aliasing.</param>
		public Font(string fontName, Drawing.FontFamily fontFamily, float fontSize, bool antiAlias)
			: this(fontName, fontFamily, fontSize, antiAlias, false, false, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Font"/> class.
		/// </summary>
		/// <param name="fontName">Name of the font to use.</param>
		/// <param name="fontFamily">Family that the font belongs to.</param>
		/// <param name="fontSize">Font size in points.</param>
		public Font(string fontName, Drawing.FontFamily fontFamily, float fontSize)
			: this(fontName, fontFamily, fontSize, true, false, false, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Font"/> class.
		/// </summary>
		/// <param name="fontName">Name of the font to use.</param>
		/// <param name="font">GDI+ font to copy settings from.</param>
		public Font(string fontName, Drawing.Font font)
			: this(fontName, font.FontFamily, font.Size, true, false, false, false)
		{
			// Copy settings.
			Bold = font.Bold;
			Underline = font.Underline;
			Italic = font.Italic;
			Strikeout = font.Strikeout;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!_disposed)
				{
					if (_userBrush != null)
						_userBrush.Dispose();

					// Destroy images.
					foreach (KeyValuePair<char, Glyph> glyph in _glyphData)
					{
						if (glyph.Value.GlyphImage != null)
							glyph.Value.GlyphImage.Dispose();
					}

					// Remove this font from the cache.
					if (FontCache.Fonts.Contains(Name))
						FontCache.Fonts.Remove(Name);
				}

				_userBrush = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
