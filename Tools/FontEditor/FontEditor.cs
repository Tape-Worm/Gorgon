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
// Created: Thursday, August 03, 2006 4:13:36 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using Drawing = System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using SharpUtilities;
using SharpUtilities.Native.Win32;
using SharpUtilities.Utility;
using SharpUtilities.Mathematics;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Fonts;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Main font editor form.
	/// </summary>
	public partial class FontEditor : Form
	{
		#region Variables.
		private Drawing.Bitmap _fontBitmap;					// Bitmap for the font.
		private Drawing.Bitmap _bitmapBuffer;				// Buffer for the bitmap.
		private Drawing.Bitmap _atlas;						// Atlas image buffer.
		private Drawing.Font _TTFont;						// Truetype font object.
		private GorgonFont _font;							// Font object.
		private Font.FontCharacter[] _chars;				// Characters.
		private Sprite _gorgonLogo = null;					// Gorgon logo.
		private string _fontFilename = string.Empty;		// Filename of the font.
		private string _fontImagePath = string.Empty;		// Font image path.
		private TextSprite _exampleText = null;				// Example text.
		private float _logoOpacity = 0.0f;					// Opacity.
		private bool _changes = false;						// Flag to indicate that there are changes present.	
		private bool _noEvents = false;						// Flag to turn off events.
		private float _zoomPercent = 1;						// Zoom percentage.
		private BorderSizer _sizer = null;					// Border sizer form.
		private bool _atlasChange = false;					// Atlas changed flag.
		private string _atlasPath = string.Empty;			// Atlas path.
		private FontManager _fonts;							// Font manager.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the zoom percentage.
		/// </summary>
		public float ZoomPercent
		{
			get
			{
				return _zoomPercent;
			}
			set
			{
				if (value <= 0.0f)
					value = 0.25f;
				if (value > 16.0f)
					value = 16.0f;
				_zoomPercent = value;
				labelZoom.Text = "Zoom: " + (int)(_zoomPercent * 100.0f) + "%";
				panelGorgon.Invalidate();

				// Turn off each item.
				foreach (ToolStripMenuItem dropdown in dropdownZoom.DropDownItems)
				{
					dropdown.Checked = false;
					switch (dropdown.Text)
					{
						case "25%":
							if (_zoomPercent == 0.25f)
								dropdown.Checked = true;
							break;
						case "50%":
							if (_zoomPercent == 0.5f)
								dropdown.Checked = true;
							break;
						case "75%":
							if (_zoomPercent == 0.75f)
								dropdown.Checked = true;
							break;
						case "100%":
							if (_zoomPercent == 1.0f)
								dropdown.Checked = true;
							break;
						case "125%":
							if (_zoomPercent == 1.25f)
								dropdown.Checked = true;
							break;
						case "150%":
							if (_zoomPercent == 1.5f)
								dropdown.Checked = true;
							break;
						case "175%":
							if (_zoomPercent == 1.75f)
								dropdown.Checked = true;
							break;
						case "200%":
							if (_zoomPercent == 2.0f)
								dropdown.Checked = true;
							break;
					}					
				}				
				
				ValidateMenu();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Paint event of the labelFontColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
		private void labelFontColor_Paint(object sender, PaintEventArgs e)
		{
			Drawing.Graphics graphics = null;		// Graphics interface.
			Drawing.SolidBrush brush = null;		// Painting brush.

			try
			{
				if (labelFontColor.Image != null)
				{
					brush = new Drawing.SolidBrush(_font.BaseColor);
					graphics = Drawing.Graphics.FromImage(labelFontColor.Image);
					graphics.DrawImage(Properties.Resources.Color, 0, 0);
					graphics.FillRectangle(brush, new Drawing.Rectangle(Drawing.Point.Empty, labelFontColor.Image.Size));
					graphics.DrawRectangle(Drawing.Pens.Black, 0, 0, labelFontColor.Image.Size.Width - 1, labelFontColor.Image.Size.Height - 1);
				}
			}
			finally
			{
				if (brush != null)
					brush.Dispose();
				brush = null;
				if (graphics != null)
					graphics.Dispose();
				graphics = null;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonFontColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonFontColor_Click(object sender, EventArgs e)
		{
			ColorPicker dialogColor = null;		// Color picker.

			try
			{
				dialogColor = new ColorPicker();
				dialogColor.Color = _font.BaseColor;
				if (dialogColor.ShowDialog(this) == DialogResult.OK)
				{
					_font.SetFontColor(dialogColor.Color);
					GenerateFont(true);
					panelGorgon.Invalidate();
					labelFontColor.Owner.Refresh();
					labelFontColor.Invalidate();
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				if (dialogColor != null)
					dialogColor.Dispose();
				dialogColor = null;
			}
		}

		/// <summary>
		/// Function to confirm saving of any changes.
		/// </summary>
		/// <returns>TRUE if we click YES/NO, FALSE if CANCEL is clicked.</returns>
		private bool ConfirmChanges()
		{
			// Warn us of any changes.
			if ((_changes) || (_atlasChange))
			{
				switch (UI.YesNoCancelBox("There have been changes made to the font.  Would you like to save them?"))
				{
					case DialogResult.Yes:
						if (!menuItemSave.Enabled)
							menuitemSaveAs_Click(this,EventArgs.Empty);
						else
							SaveFont(_fontFilename);
						break;
					case DialogResult.Cancel:
						return false;
				}
			}

			_atlasChange = false;
			_changes = false;
			return true;
		}

		/// <summary>
		/// Function to update the scrollbars.
		/// </summary>
		/// <param name="size">Target size.</param>
		private void UpdateScrollBars(Drawing.Size size)
		{
			int maxHScroll = 32000;		// Defaults.
			int maxVScroll = 32000;

			// Set the scrollers.
			if ((size.Width > panelGorgon.ClientSize.Width) || ((scrollVertical.Visible) && (size.Width + scrollVertical.Width > panelGorgon.ClientSize.Width)))
			{
				scrollHorizontal.Visible = true;
				maxHScroll = (size.Width - panelGorgon.ClientSize.Width) + (scrollHorizontal.LargeChange);
			}
			else
			{
				scrollHorizontal.Value = 0;
				scrollHorizontal.Visible = false;
			}

			if ((size.Height > panelGorgon.ClientSize.Height) || ((scrollHorizontal.Visible) && (size.Height + scrollHorizontal.Height > panelGorgon.ClientSize.Height)))
			{
				scrollVertical.Visible = true;
				maxVScroll = (size.Height - panelGorgon.ClientSize.Height) + (scrollVertical.LargeChange);
			}
			else
			{
				scrollVertical.Value = 0;
				scrollVertical.Visible = false;
			}

			if (scrollHorizontal.Visible)
				maxVScroll += scrollHorizontal.Size.Height;

			if (scrollVertical.Visible)
				maxHScroll += scrollVertical.Size.Width;

			scrollHorizontal.Maximum = maxHScroll;
			scrollVertical.Maximum = maxVScroll;

			if ((scrollVertical.Visible) || (scrollHorizontal.Visible))
				labelScrollSpace.Visible = true;
			else
				labelScrollSpace.Visible = false;
		}

		/// <summary>
		/// Function to perform effect compositing.
		/// </summary>
		/// <param name="graphics">Graphics interface for the font bitmap.</param>
		private void CompositeEffects(Drawing.Graphics graphics)
		{
			Drawing.Bitmap tempBuffer1 = null;						// Temporary buffer.
			Drawing.Bitmap tempBuffer2 = null;						// Temporary buffer.
			Drawing.Graphics tempGraphics = null;					// Graphics interface.
			Drawing.Imaging.ImageAttributes attributes = null;		// Image attributes.

			try
			{
				// Copy the font buffer settings.
				tempBuffer1 = new Drawing.Bitmap(_fontBitmap);
				tempGraphics = Drawing.Graphics.FromImage(tempBuffer1);
				attributes = new Drawing.Imaging.ImageAttributes();
				graphics.CompositingMode = CompositingMode.SourceCopy;

				// If we have anti-aliasing enabled, then overwrite the alpha channel with the proper values.
				// Else we'll have blocky fonts.
				if (_font.IsAntiAliased)
				{
					Drawing.Color pixel;				// Pixel color.
					Drawing.Imaging.BitmapData data;	// Internal bitmap data.
					int avg = 0;						// Average.

					unsafe
					{
						// Get the bitmap data.
						data = _fontBitmap.LockBits(new Drawing.Rectangle(0, 0, _fontBitmap.Width, _fontBitmap.Height), Drawing.Imaging.ImageLockMode.ReadWrite, Drawing.Imaging.PixelFormat.Format32bppArgb);

						// Get a pointer to the data.
						int* scanPtr = (int*)(data.Scan0.ToPointer());

						// Convert to alpha.
						for (int i = 0; i < data.Width * data.Height; i++)
						{
							pixel = Drawing.Color.FromArgb(*scanPtr);

							// Skip pure white.
							if (pixel != Drawing.Color.White)
							{
								// Compute average.								
								avg = (pixel.R + pixel.G + pixel.B) / 3;

								// Update alpha channel.
								if (avg < 255)
									*scanPtr = (avg << 24) | 0x00FFFFFF;
							}

							scanPtr++;
						}

						_fontBitmap.UnlockBits(data);
					}
				}

				// Apply any alpha to the font (TextRenderer.DrawText() will -NOT- use alpha, so we have to do it ourselves.)
				if (_font.BaseColor != Drawing.Color.White)
				{
					Drawing.Imaging.ColorMatrix matrix = new Drawing.Imaging.ColorMatrix();

					// Set the constant alpha value.
					matrix.Matrix00 = (float)_font.BaseColor.R / 255.0f;
					matrix.Matrix11 = (float)_font.BaseColor.G / 255.0f;
					matrix.Matrix22 = (float)_font.BaseColor.B / 255.0f;
					matrix.Matrix33 = (float)_font.BaseColor.A / 255.0f;
					matrix.Matrix44 = 1.0f;
					attributes.SetColorMatrix(matrix);

					// Clear the temporary buffer.
					tempGraphics.Clear(Drawing.Color.Black);
					tempGraphics.Clear(Drawing.Color.Transparent);
					tempGraphics.DrawImage(_fontBitmap, new Drawing.Rectangle(0, 0, _fontBitmap.Width, _fontBitmap.Height),
								0, 0, _fontBitmap.Width, _fontBitmap.Height, Drawing.GraphicsUnit.Pixel, attributes);

					// Reset any previous color matrix.					
					attributes.ClearColorMatrix();

					// Send the image back to the main buffer.
					graphics.DrawImage(tempBuffer1, 0, 0);
				}

				// Composite the outline.
				if ((_font.IsOutlined) && (_font.OutlineColor.A > 0))
				{
					Drawing.Point outlinePos = Drawing.Point.Empty;		// Text position.

					// New color matrix to fade/color the image.
					Drawing.Imaging.ColorMatrix matrix = new Drawing.Imaging.ColorMatrix();
					tempGraphics.Clear(Drawing.Color.FromArgb(0, 0, 0, 0));
					matrix.Matrix00 = (float)_font.OutlineColor.R / 255.0f;
					matrix.Matrix11 = (float)_font.OutlineColor.G / 255.0f;
					matrix.Matrix22 = (float)_font.OutlineColor.B / 255.0f;
					matrix.Matrix33 = 1.0f;
					attributes.SetColorMatrix(matrix);

					outlinePos.Y = -_font.OutlineSize;
					// 'Smear' the characters.
					for (int k = 0; k <= (_font.OutlineSize * 2); k++)
					{
						outlinePos.X = -_font.OutlineSize;
						for (int j = 0; j <= (_font.OutlineSize * 2); j++)
						{
							tempGraphics.DrawImage(_fontBitmap, new Drawing.Rectangle(outlinePos.X, outlinePos.Y, _fontBitmap.Width, _fontBitmap.Height),
								0, 0, _fontBitmap.Width, _fontBitmap.Height, Drawing.GraphicsUnit.Pixel, attributes);
							outlinePos.X++;
						}
						outlinePos.Y++;
					}

					// Apply linear alpha.
					matrix.Matrix00 = (float)_font.OutlineColor.R / 255.0f;
					matrix.Matrix11 = (float)_font.OutlineColor.G / 255.0f;
					matrix.Matrix22 = (float)_font.OutlineColor.B / 255.0f;
					matrix.Matrix33 = (float)_font.OutlineColor.A / 255.0f;
					attributes.SetColorMatrix(matrix);

					// Make a composite buffer.
					tempBuffer2 = new Drawing.Bitmap(tempBuffer1);
					tempGraphics.Clear(Drawing.Color.Black);
					tempGraphics.Clear(Drawing.Color.Transparent);
					// Even out alpha blending.
					tempGraphics.DrawImage(tempBuffer2, new Drawing.Rectangle(0, 0, _fontBitmap.Width, _fontBitmap.Height),
						0, 0, _fontBitmap.Width, _fontBitmap.Height, Drawing.GraphicsUnit.Pixel, attributes);

					// Overlay the image.
					tempGraphics.DrawImage(_fontBitmap, 0, 0);
					// Copy back into the font bitmap.
					graphics.DrawImage(tempBuffer1, 0, 0);
					tempBuffer2.Dispose();
					tempBuffer2 = null;
				}

				// Set up shadowing.
				if ((_font.IsShadowed) && (_font.ShadowColor.A > 0))
				{
					Drawing.Point shadowPos = Drawing.Point.Empty;	// Text position.

					// Alter position.
					switch (_font.ShadowDirection)
					{
						case FontShadowDirection.LowerLeft:
							shadowPos.Y += _font.ShadowOffset;
							shadowPos.X -= _font.ShadowOffset;
							break;
						case FontShadowDirection.LowerMiddle:
							shadowPos.Y += _font.ShadowOffset;
							break;
						case FontShadowDirection.LowerRight:
							shadowPos.X += _font.ShadowOffset;
							shadowPos.Y += _font.ShadowOffset;
							break;
						case FontShadowDirection.MiddleLeft:
							shadowPos.X -= _font.ShadowOffset;
							break;
						case FontShadowDirection.MiddleRight:
							shadowPos.X += _font.ShadowOffset;
							break;
						case FontShadowDirection.UpperLeft:
							shadowPos.X -= _font.ShadowOffset;
							shadowPos.Y -= _font.ShadowOffset;
							break;
						case FontShadowDirection.UpperMiddle:
							shadowPos.Y -= _font.ShadowOffset;
							break;
						case FontShadowDirection.UpperRight:
							shadowPos.X += _font.ShadowOffset;
							shadowPos.Y -= _font.ShadowOffset;
							break;
					}

					// New color matrix to fade/color the image.
					Drawing.Imaging.ColorMatrix matrix = new Drawing.Imaging.ColorMatrix();

					tempGraphics.Clear(Drawing.Color.Black);
					tempGraphics.Clear(Drawing.Color.Transparent);
					matrix.Matrix00 = (float)_font.ShadowColor.R / 255.0f;
					matrix.Matrix11 = (float)_font.ShadowColor.G / 255.0f;
					matrix.Matrix22 = (float)_font.ShadowColor.B / 255.0f;
					matrix.Matrix33 = (float)_font.ShadowColor.A / 255.0f;
					attributes.SetColorMatrix(matrix);
					tempGraphics.DrawImage(_fontBitmap, new Drawing.Rectangle(shadowPos.X, shadowPos.Y, _fontBitmap.Width, _fontBitmap.Height),
						0, 0, _fontBitmap.Width, _fontBitmap.Height, Drawing.GraphicsUnit.Pixel, attributes);

					// Overlay the image.
					tempGraphics.DrawImage(_fontBitmap, 0, 0);
					// Copy back into the font bitmap.
					graphics.DrawImage(tempBuffer1, 0, 0);
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				if (attributes != null)
					attributes.Dispose();
				if (tempGraphics != null)
					tempGraphics.Dispose();
				if (tempBuffer1 != null)
					tempBuffer1.Dispose();
				if (tempBuffer2 != null)
					tempBuffer2.Dispose();

				attributes = null;
				tempGraphics = null;
				tempBuffer1 = null;
				tempBuffer2 = null;
			}
		}

		/// <summary>
		/// Function to generate a font.
		/// </summary>
		/// <param name="updateCharDimensions">TRUE to update the dimensions of the characters, FALSE to use existing.</param>
		private void GenerateFont(bool updateCharDimensions)
		{
			Drawing.Graphics graphics = null;				// Graphics interface.
			Drawing.Point fontPos = Drawing.Point.Empty;	// Font position.			

			try
			{
				Gorgon.Stop();
				Cursor = Cursors.WaitCursor;

				// Create the interface.
				UpdateTTFont();
				graphics = Drawing.Graphics.FromImage(_fontBitmap);
				graphics.Clear(Drawing.Color.Black);
				graphics.Clear(Drawing.Color.FromArgb(0, 0, 0, 0));

				// Draw to the image.				
				// Calculate positions and draw.
				graphics.SmoothingMode = Drawing.Drawing2D.SmoothingMode.None;
				if (_font.IsAntiAliased)
					graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
				else
					graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

				// Calculate ranges.				
				int maxHeight = 0;
				fontPos = Drawing.Point.Empty;

				// Calculate character width.
				ABC charWidth = default(ABC);
				int outlineSize = (_font.IsOutlined) ? _font.OutlineSize : 0;
				int shadowSize = (_font.IsShadowed) ? _font.ShadowOffset : 0;
				int yOffset = 0;
				fontPos.X = 0;
				fontPos.Y = 0;
				for (int i = 0; i < _chars.Length; i++)
				{
					yOffset = 0;

					if (updateCharDimensions)
					{
						// Calculate character width.
						charWidth = Utilities.GetCharacterWidth(graphics, _chars[i].Character, _TTFont);

						// Store ABC widths.
						_chars[i].A = charWidth.A;
						_chars[i].B = (uint)((int)charWidth.B + (_font.LeftBorderOffset + _font.RightBorderOffset + (outlineSize * 2)));
						_chars[i].C = charWidth.C;

						// Get character height.
						Drawing.Size charHeight = Drawing.Size.Empty;

						charHeight = Drawing.Size.Round(graphics.MeasureString(_chars[i].Character.ToString(), _TTFont));

						// Store the max height per line.
						if (charHeight.Height > maxHeight)
							maxHeight = charHeight.Height;

						_chars[i].Width = charWidth.B + _font.LeftBorderOffset + _font.RightBorderOffset + (outlineSize * 2);
						_chars[i].Height = maxHeight + _font.TopBorderOffset + _font.BottomBorderOffset + (outlineSize * 2);

						// Alter for shadow position.
						if (_font.IsShadowed)
						{
							switch (_font.ShadowDirection)
							{
								case FontShadowDirection.LowerLeft:
									_chars[i].Width += shadowSize;
									_chars[i].A -= shadowSize;
									_chars[i].B += (uint)shadowSize;
									_chars[i].Height += shadowSize;
									break;
								case FontShadowDirection.LowerMiddle:
									_chars[i].Height += shadowSize;
									break;
								case FontShadowDirection.LowerRight:
									_chars[i].Width += shadowSize;
									_chars[i].C -= shadowSize;
									_chars[i].B += (uint)shadowSize;
									_chars[i].Height += shadowSize;
									break;
								case FontShadowDirection.MiddleLeft:
									_chars[i].A -= shadowSize;
									_chars[i].B += (uint)shadowSize;
									_chars[i].Width += shadowSize;
									break;
								case FontShadowDirection.MiddleRight:
									_chars[i].Width += shadowSize;
									_chars[i].C -= shadowSize;
									_chars[i].B += (uint)shadowSize;
									break;
								case FontShadowDirection.UpperLeft:
									yOffset += shadowSize;
									_chars[i].Height += shadowSize;
									_chars[i].Width += shadowSize;
									_chars[i].A -= shadowSize;
									_chars[i].B += (uint)shadowSize;
									break;
								case FontShadowDirection.UpperMiddle:
									yOffset += shadowSize;
									_chars[i].Height += shadowSize;
									break;
								case FontShadowDirection.UpperRight:
									yOffset += shadowSize;
									_chars[i].Height += shadowSize;
									_chars[i].Width += shadowSize;
									_chars[i].C -= shadowSize;
									_chars[i].B += (uint)shadowSize;
									break;
							}
						}

						if ((fontPos.X + (int)_chars[i].Width + 1) > _font.ImageSize.X)
						{
							fontPos.X = 0;
							fontPos.Y += (int)_chars[i].Height + 1;
							maxHeight = 0;
						}

						_chars[i].Left = fontPos.X;
						_chars[i].Top = fontPos.Y;					
					}
					
					fontPos.X += (int)_chars[i].Width + 1;

					// Text position.
					Drawing.Point textPos = Drawing.Point.Empty;

					textPos.X = ((int)_chars[i].Left - _chars[i].A + _font.LeftBorderOffset + outlineSize);
					textPos.Y = ((int)_chars[i].Top + _font.TopBorderOffset + outlineSize) + yOffset;

					// Draw to buffer.  
					// Note:  This function does NOT support any special functionality like
					// GDI+ would support, including alpha blending.
					TextRenderer.DrawText(graphics, _chars[i].Character.ToString(), _TTFont, textPos, Drawing.Color.White, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
				}
				
				// Composite any effects into the bitmap.
				CompositeEffects(graphics);				

				// Update font characters.
				_font.UpdateCharacters(_chars);
				_font.FontImage = null;
				panelGorgon.Invalidate();

				// Refresh the text if it's being displayed.
				if (menuitemTestText.Checked)
					menuitemTestText_Click(this, EventArgs.Empty);

				// Update the font.
				_sizer.GorgonFont = _font;
				_changes = true;
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				if (graphics != null)
					graphics.Dispose();

				ValidateMenu();
				Cursor = Cursors.Default;
				graphics = null;
			}
		}

		/// <summary>
		/// Function to update the true type font.
		/// </summary>
		private void UpdateTTFont()
		{
			Drawing.FontStyle style;					// Font style.

			style = Drawing.FontStyle.Regular;
			// Create actual font.
			if (_font.IsBold)
				style |= Drawing.FontStyle.Bold;
			if (_font.IsUnderlined)
				style |= Drawing.FontStyle.Underline;
			if (_font.IsItalics)
				style |= Drawing.FontStyle.Italic;

			if (_TTFont != null)
				_TTFont.Dispose();

			_TTFont = new Drawing.Font(_font.SourceFont, _font.SourceFontSize, style);
		}

		/// <summary>
		/// Function to create the font bitmap buffers.
		/// </summary>
		/// <param name="imageSize">Size of the image.</param>
		private void MakeBuffers(Vector2D imageSize)
		{
			if (_fontBitmap != null)
				_fontBitmap.Dispose();
			if (_bitmapBuffer != null)
				_bitmapBuffer.Dispose();
			_fontBitmap = new Drawing.Bitmap((int)imageSize.X, (int)imageSize.Y, Drawing.Imaging.PixelFormat.Format32bppArgb);
			_bitmapBuffer = new Drawing.Bitmap((int)imageSize.X, (int)imageSize.Y, Drawing.Imaging.PixelFormat.Format32bppArgb);
			_font.ImageSize = imageSize;
		}

		/// <summary>
		/// Function to create a new font.
		/// </summary>
		/// <param name="getName">TRUE to ask for the name of the font, FALSE to skip.</param>
		/// <returns>TRUE if successful, FALSE if not or cancelled.</returns>
		private bool NewFont(bool getName)
		{
			FontMetaData newFontName = null;			// New font name.			

			try
			{
				_noEvents = true;
				// Confirm any changes.
				if (!ConfirmChanges())
					return false;

				// Get new font name.
				if (getName)
				{
					newFontName = new FontMetaData();
					newFontName.Text = "New font.";
					newFontName.SourceFont = comboFontList.Text;
					if (textFontSize.Text.Trim() != string.Empty)
					{
						float fontSize = 9.0f;		// Default to 9 units.
						float.TryParse(textFontSize.Text, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out fontSize);
						newFontName.SourceFontSize = fontSize;
					}
					else
						newFontName.SourceFontSize = 9.0f;
					newFontName.IsBold = buttonBold.Checked;
					newFontName.IsItalics = buttonItalics.Checked;
					newFontName.IsUnderlined = buttonUnderlined.Checked;
					newFontName.AntiAliased = buttonAntiAlias.Checked;

					if (newFontName.ShowDialog(this) != DialogResult.OK)
						return false;
				}

				// Bring the opacity down for animation.
				_logoOpacity = 0.0f;

				// Remove the font.
				if (_font.Name != string.Empty)
				{
					// Remove the font image from the list.
					if (_font.FontImage != null)
						Gorgon.ImageManager.Remove(_font.FontImage.Name);

					_font.FontImage = null;
				}

				// Remove example text.
				if (_exampleText != null)
					_exampleText = null;

				// Initialize.
				_font = new GorgonFont();
				if (getName)
				{
					_font.SetFontMetaData(newFontName.SourceFont, newFontName.SourceFontSize, newFontName.IsBold, newFontName.IsItalics, newFontName.IsUnderlined, newFontName.AntiAliased);
					_font.ImageSize = new Vector2D(newFontName.FontImageSize.Width, newFontName.FontImageSize.Height);
					if (ReadKey<bool>("borderOpen", false))
					{
						menuitemBorderSizes.Checked = true;
						_sizer.Show();
					}
				}
				else
				{
					_font.ImageSize = new Vector2D(256, 256);
					_font.SetFontMetaData("Arial", 9.0f, false, false, false, false);
				}

				UpdateTTFont();

				// Create the font bitmap.
				MakeBuffers(_font.ImageSize);
				_exampleText = new TextSprite("ExampleText", string.Empty, _font, Drawing.Color.Black);
				_exampleText.Text = string.Empty;
				_fontFilename = string.Empty;
				_fontImagePath = string.Empty;
				scrollHorizontal.Visible = false;
				scrollVertical.Visible = false;
				labelScrollSpace.Visible = false;

				// If we got a name, then assign it.
				if (getName)
				{
					_font.SetName(newFontName.FontName.Trim());
					GenerateFont(true);
				}
				else
					_changes = false;

				_atlasChange = false;
				ValidateMenu();
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				Gorgon.Go();
				if (newFontName != null)
					newFontName.Dispose();
				newFontName = null;

				_noEvents = false;
			}

			return true;
		}

		/// <summary>
		/// Handles the KeyDown event of the textFontSize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void textFontSize_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				textFontSize_Leave(sender, e);
				e.Handled = true;
			}
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboFontList control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void comboFontList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!_noEvents)
			{
				_font.SetFontMetaData(comboFontList.Text, _font.SourceFontSize, _font.IsBold, _font.IsItalics, _font.IsUnderlined, _font.IsAntiAliased);
				GenerateFont(true);
			}
		}

		/// <summary>
		/// Handles the Leave event of the textFontSize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void textFontSize_Leave(object sender, EventArgs e)
		{
			float size = 9.0f;		// Size.

			try
			{
				if (!Utilities.IsNumeric(textFontSize.Text))
					textFontSize.Text = "9.0";
				else
				{
					float.TryParse(textFontSize.Text, out size);
					if (size <= 0.0f)
						size = 9.0f;
					if (size > 256.0f)
						size = 256.0f;
					textFontSize.Text = string.Format("{0:##0.0}", size);
				}

				_font.SetFontMetaData(comboFontList.Text, size, _font.IsBold, _font.IsItalics, _font.IsUnderlined, _font.IsAntiAliased);
				GenerateFont(true);				
			}
			catch
			{
			}
		}

		/// <summary>
		/// Function to validate menu items.
		/// </summary>
		private void ValidateMenu()
		{			
			menuitemSaveAs.Enabled = false;
			menuItemSave.Enabled = false;
			comboFontList.Enabled = false;
			labelFontSize.Enabled = false;
			textFontSize.Enabled = false;
			buttonBold.Enabled = false;
			buttonUnderlined.Enabled = false;
			buttonItalics.Enabled = false;
			menuitemTestText.Enabled = false;
			menuitemChangeFont.Enabled = false;
			buttonSave.Enabled = false;
			dropdownZoom.Enabled = false;
			buttonZoomIn.Enabled = false;
			buttonZoomOut.Enabled = false;
			menuitemFontImageSize.Enabled = false;
			menuitemBorderSizes.Enabled = false;
			labelOutline.Enabled = false;
			textOutlineSize.Enabled = false;
			buttonOutlineColor.Enabled = false;
			buttonOutline.Enabled = false;
			menuitemChangeName.Enabled = false;
			buttonAntiAlias.Enabled = false;
			buttonShadow.Enabled = false;
			labelShadowOffset.Enabled = false;
			textShadowOffset.Enabled = false;
			buttonShadowColor.Enabled = false;
			menuShadowDirection.Enabled = false;
			menuExport.Enabled = false;
			menuitemRemoveAtlas.Enabled = false;
			menuitemUnbindAtlas.Enabled = false;
			menuitemPlaceOnAtlas.Enabled = false;
			menuitemSaveAtlas.Enabled = false;
			buttonFontColor.Enabled = false;
			menuitemCharacterList.Enabled = false;
			menuitemRemoveAtlas.Enabled = false;
			menuitemViewAtlasImage.Enabled = false;

			if (_atlas != null)
			{
				menuitemRemoveAtlas.Enabled = true;
				menuitemViewAtlasImage.Enabled = true;
				if (menuitemViewAtlasImage.Checked)
				{
					dropdownZoom.Enabled = true;
					buttonZoomIn.Enabled = true;
					buttonZoomOut.Enabled = true;
				}
			}

			if (_fontFilename != string.Empty)
				menuItemSave.Enabled = true;

			Text = "Gorgon Font Editor";
			if ((_font != null) && (_font.Name != string.Empty))
			{
				_noEvents = true;
				menuitemSaveAs.Enabled = true;
				Text += " - " + _font.Name;

				if ((_changes) || (_atlasChange))
					Text += "*";

				if (_fontFilename != string.Empty)
					Text += " - \"" + _fontFilename + "\"";
				comboFontList.Enabled = true;
				labelFontSize.Enabled = true;
				textFontSize.Enabled = true;
				buttonBold.Enabled = true;
				buttonUnderlined.Enabled = true;
				buttonItalics.Enabled = true;
				menuitemTestText.Enabled = true;
				menuExport.Enabled = true;
				menuitemCharacterList.Enabled = true;

				buttonBold.Checked = _font.IsBold;
				buttonItalics.Checked = _font.IsItalics;
				buttonUnderlined.Checked = _font.IsUnderlined;
				comboFontList.Text = _font.SourceFont;
				buttonAntiAlias.Checked = _font.IsAntiAliased;
				buttonOutline.Checked = _font.IsOutlined;
				buttonShadow.Checked = _font.IsShadowed;
				textOutlineSize.Text = _font.OutlineSize.ToString();
				textShadowOffset.Text = _font.ShadowOffset.ToString();
				textFontSize.Text = _font.SourceFontSize.ToString("##0.0");
				menuitemChangeFont.Enabled = true;
				buttonSave.Enabled = true;
				menuitemFontImageSize.Enabled = true;
				menuitemBorderSizes.Enabled = true;
				buttonOutline.Enabled = true;
				buttonAntiAlias.Enabled = true;
				buttonShadow.Enabled = true;
				menuitemChangeName.Enabled = true;
				buttonFontColor.Enabled = true;

				// Enable atlas options
				if (_atlas != null)
				{
					menuitemPlaceOnAtlas.Enabled = true;
					if (_font.ImageIsAtlas)
						menuitemSaveAtlas.Enabled = true;
				}

				if (_font.ImageIsAtlas)
					menuitemUnbindAtlas.Enabled = true;

				if (_font.IsOutlined)
				{
					labelOutline.Enabled = true;
					textOutlineSize.Enabled = true;
					buttonOutlineColor.Enabled = true;
				}

				if (_font.IsShadowed)
				{
					labelShadowOffset.Enabled = true;
					textShadowOffset.Enabled = true;
					buttonShadowColor.Enabled = true;
					menuShadowDirection.Enabled = true;
					menuitemLowerLeft.Checked = false;
					menuitemLowerCenter.Checked = false;
					menuitemLowerRight.Checked = false;
					menuitemLeft.Checked = false;
					menuitemRight.Checked = false;
					menuitemUpperLeft.Checked = false;
					menuitemUpperCenter.Checked = false;
					menuitemUpperRight.Checked = false;

					// Set the shadow direction.
					switch (_font.ShadowDirection)
					{
						case FontShadowDirection.LowerLeft:
							menuitemLowerLeft.Checked = true;
							break;
						case FontShadowDirection.LowerMiddle:
							menuitemLowerCenter.Checked = true;
							break;
						case FontShadowDirection.LowerRight:
							menuitemLowerRight.Checked = true;
							break;
						case FontShadowDirection.MiddleLeft:
							menuitemLeft.Checked = true;
							break;
						case FontShadowDirection.MiddleRight:
							menuitemRight.Checked = true;
							break;
						case FontShadowDirection.UpperLeft:
							menuitemUpperLeft.Checked = true;
							break;
						case FontShadowDirection.UpperMiddle:
							menuitemUpperCenter.Checked = true;
							break;
						case FontShadowDirection.UpperRight:
							menuitemUpperRight.Checked = true;
							break;
					}
				}

				if (!menuitemTestText.Checked)
				{
					if (_zoomPercent != 16.0f)
						buttonZoomIn.Enabled = true;
					if (_zoomPercent != 0.25f)
						buttonZoomOut.Enabled = true;
					dropdownZoom.Enabled = true;
				}
				
				labelShadowColor.Owner.Refresh();
				labelShadowColor.Invalidate();
				labelOutlineColor.Owner.Refresh();
				labelOutlineColor.Invalidate();
				labelFontColor.Owner.Refresh();
				labelFontColor.Invalidate();
				_noEvents = false;
			}
		}

		/// <summary>
		/// Function to save a registry key for the application.
		/// </summary>
		/// <typeparam name="T">Type of data to save.</typeparam>
		/// <param name="keyName">Name of the key to write data into.</param>
		/// <param name="keyValue">Value to write.</param>
		private void SaveKey<T>(string keyName, T keyValue)
		{
			RegistryKey pathKey = null;			// Key that holds the path.

			try
			{
				// Open the key.
				pathKey = Registry.CurrentUser.CreateSubKey("Software\\Tape_Worm\\Gorgon\\FontEditor");

				if (pathKey == null)
					return;

				pathKey.SetValue(keyName, keyValue);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to save the value for '" + keyName + "'.\n" + ex.Message);
			}
			finally
			{
				if (pathKey != null)
					pathKey.Close();
			}
		}

		/// <summary>
		/// Function to read a value from a registry key.
		/// </summary>
		/// <typeparam name="T">Type of data to save.</typeparam>
		/// <param name="keyName">Name of the key to write data into.</param>
		/// <param name="defaultData">Data to use as a default value.</param>
		/// <returns>Value in the key.</returns>
		private T ReadKey<T>(string keyName, T defaultData)
		{
			RegistryKey pathKey = null;			// Key that holds the path.
			T regValue = default(T);			// Registry key value.

			try
			{
				// Open the key.
				pathKey = Registry.CurrentUser.OpenSubKey("Software\\Tape_Worm\\Gorgon\\FontEditor");

				if (pathKey == null)
					return defaultData;

				regValue = (T)Convert.ChangeType(pathKey.GetValue(keyName, defaultData), typeof(T));
				if (regValue == null)
					regValue = defaultData;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to retrieve the value for '" + keyName + "'.\n" + ex.Message);
			}
			finally
			{
				if (pathKey != null)
					pathKey.Close();
			}

			return regValue;
		}

		/// <summary>
		/// Function to display the font.
		/// </summary>
		private void DisplayFont()
		{
			VideoMode mode;				// Video mode.
			bool useAtlas = false;		// Font uses image atlas.

			try
			{
				// Get the current video mode.
				mode = Gorgon.VideoMode;

				Gorgon.Screen.BackgroundColor = Drawing.Color.FromArgb(169, 153, 112);

				if (_font.Name != string.Empty)
				{
					_gorgonLogo.SetPosition(mode.Width / 2.0f, mode.Height / 2.0f);
					_gorgonLogo.Smoothing = Smoothing.Smooth;
					_gorgonLogo.UniformScale = mode.Width / (568 - containerMain.LeftToolStripPanel.Width - containerMain.RightToolStripPanel.Width);
					_gorgonLogo.Opacity = 90;
					_gorgonLogo.Draw();
					useAtlas = _font.ImageIsAtlas;
					_font.ImageIsAtlas = false;
					if (_exampleText.Text == string.Empty)
					{						
						// Set up information.
						_exampleText.AppendText("This is an example of the " + _font.Name + " font.\n");
						if (_font.SourceFont.ToLower() == "unknown")
							_exampleText.AppendText("Font comes from an unknown font face.\n");
						else
							_exampleText.AppendText("Font is based on the " + _font.SourceFont + " font.\n");
						if (_font.SourceFontSize > 0.0f)
							_exampleText.AppendText("The size is " + _font.SourceFontSize.ToString("##0.0") + " points.\n");
						if (_font.IsBold)
							_exampleText.AppendText("Font is bold. ");
						if (_font.IsItalics)
							_exampleText.AppendText("Font uses italics. ");
						if (_font.IsUnderlined)
							_exampleText.AppendText("Font is underlined. ");
						if (_font.IsOutlined)
							_exampleText.AppendText("Font is outlined. ");
						if (_font.IsAntiAliased)
							_exampleText.AppendText("Font is anti-aliased. ");
						if (_font.IsShadowed)
							_exampleText.AppendText("Font is shadowed.");
						_exampleText.AppendText("\n\nthe quick brown fox jumped over the lazy dog");
						_exampleText.AppendText("\nTHE QUICK BROWN FOX JUMPED OVER THE LAZY DOG");
						_exampleText.AppendText("\n1234567890 !@#$%^&*() -_ =+ {[]} \\| ;: '\" ,<>. ?/");
						_exampleText.AppendText("\n\nThis is a test of some text.  More text.  And yet another line of a text.");
						_exampleText.AppendText("\n\n");

						for (int i = 0; i < _font.Count; i++)
							_exampleText.AppendText(_font[i].Character.ToString());
					}

					_exampleText.Color = Drawing.Color.White;

					if ((_font.IsOutlined) || (_font.IsShadowed))
						_exampleText.Color = Drawing.Color.White;

					_exampleText.SetPosition(-scrollHorizontal.Value, -scrollVertical.Value);
					_exampleText.BlendingMode = Blending.Normal;
					_exampleText.Smoothing = Smoothing.None;
					_exampleText.Draw();
					_font.ImageIsAtlas = useAtlas;

					UpdateScrollBars(new Drawing.Size((int)_exampleText.Width, (int)_exampleText.Height));
				}
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, ErrorDialogIcons.Bug, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Function to update the strip layout.
		/// </summary>
		/// <param name="layoutPosition">Position of the layout.</param>
		/// <param name="strip">Strip to use.</param>
		private void UpdateStripLayout(string layoutPosition, ToolStrip strip)
		{
			switch (layoutPosition.ToLower())
			{
				case "top":
					containerMain.TopToolStripPanel.Controls.Add(strip);
					break;
				case "bottom":
					containerMain.BottomToolStripPanel.Controls.Add(strip);
					break;
				case "right":
					containerMain.RightToolStripPanel.Controls.Add(strip);
					break;
				case "left":
					containerMain.LeftToolStripPanel.Controls.Add(strip);
					break;
			}
		}

		/// <summary>
		/// Function to set up the defaults.
		/// </summary>
		private void LoadDefaults()
		{
			Drawing.Text.InstalledFontCollection fonts;		// Font collection.
			string layout = string.Empty;					// Tool strip layout.
			int stripCount = 0;								// Tool strip count.

			// Get font list.
			fonts = new Drawing.Text.InstalledFontCollection();

			// Set the defaults.
			_chars = new Font.FontCharacter[95];

			// Get default character list.
			for (int c = 0; c < 95; c++)
			{
				_chars[c].CharacterOrdinal = c + 32;
				_chars[c].Character = Convert.ToChar(c + 32);
			}

			// Font we're working on.
			_font = new GorgonFont();
			// Default to 256x256.
			_font.ImageSize = new Vector2D(256, 256);
			_font.UpdateCharacters(_chars);
			_font.SetFontMetaData("Arial", 9.0f, false, false, false,false);

			// Fill the font combo.
			comboFontList.Items.Clear();
			foreach (Drawing.FontFamily ttFont in fonts.Families)
				comboFontList.Items.Add(ttFont.Name);

			if (comboFontList.Items.Contains("Arial"))
				comboFontList.Text = "Arial";
			else
				comboFontList.Text = comboFontList.Items[0].ToString();

			_TTFont = new Drawing.Font("Arial", 9.0f, Drawing.FontStyle.Regular);
			if (_fontBitmap != null)
				_fontBitmap.Dispose();
			_fontBitmap = new Drawing.Bitmap((int)_font.ImageSize.X, (int)_font.ImageSize.Y, Drawing.Imaging.PixelFormat.Format32bppArgb);
			_bitmapBuffer = new Drawing.Bitmap((int)_font.ImageSize.X, (int)_font.ImageSize.Y, Drawing.Imaging.PixelFormat.Format32bppArgb);

			// If the gorgon logo isn't here, then load it.
			if (_gorgonLogo == null)
			{
				Gorgon.ImageManager.FromResource("GorgonLogo2", Properties.Resources.ResourceManager);
				_gorgonLogo = new Sprite("GorgonLogoSprite", Gorgon.ImageManager["GorgonLogo2"]);
				_gorgonLogo.UniformScale = 2.0f;
				_gorgonLogo.SetAxis(_gorgonLogo.Width / 2.0f, _gorgonLogo.Height / 2.0f);
				_logoOpacity = 0.0f;
			}

			// Assign the combo box event.
			comboFontList.SelectedIndexChanged += new EventHandler(comboFontList_SelectedIndexChanged);

			// Create sizer.
			_sizer = new BorderSizer();
			AddOwnedForm(_sizer);
			_sizer.BorderUpdated += new EventHandler(_sizer_BorderUpdated);
			_sizer.FormClosing += new FormClosingEventHandler(_sizer_FormClosing);
			_sizer.Left = ReadKey<int>("borderX", this.Left - _sizer.Width - 5);
			_sizer.Top = ReadKey<int>("borderY", this.Top);

			// Remove all strips.
			containerMain.TopToolStripPanel.Controls.Clear();
			containerMain.LeftToolStripPanel.Controls.Clear();
			containerMain.RightToolStripPanel.Controls.Clear();
			containerMain.BottomToolStripPanel.Controls.Clear();

			stripCount = ReadKey<int>("ToolStripCount", 3);

			containerMain.TopToolStripPanel.Controls.Add(stripMenu);
			
			stripFile.Top = ReadKey<int>("FileY", 24);
			stripFile.Left = ReadKey<int>("FileX", 3);			
			stripFont.Top = ReadKey<int>("FontY", 24);
			stripFont.Left = ReadKey<int>("FontX", 82);
			stripZoom.Top = ReadKey<int>("ZoomY", 24);
			stripZoom.Left = ReadKey<int>("ZoomX", 357);
			stripSpecial.Top = ReadKey<int>("SpecialY", 24);
			stripSpecial.Left = ReadKey<int>("SpecialX", 413);
			
			layout = ReadKey<string>("FileLayout", "top");			
			UpdateStripLayout(layout, stripFile);
			layout = ReadKey<string>("FontLayout", "top");
			UpdateStripLayout(layout, stripFont);
			layout = ReadKey<string>("ZoomLayout", "top");
			UpdateStripLayout(layout, stripZoom);
			layout = ReadKey<string>("SpecialLayout", "top");
			UpdateStripLayout(layout, stripSpecial);
			containerMain.BottomToolStripPanel.Controls.Add(stripStatus);
		}

		/// <summary>
		/// Handles the FormClosing event of the _sizer control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.FormClosingEventArgs"/> instance containing the event data.</param>
		private void _sizer_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing)
				SaveKey<bool>("borderOpen", false);
			SaveKey<int>("borderX", _sizer.Left);
			SaveKey<int>("borderY", _sizer.Top);
			menuitemBorderSizes.Checked = false;
		}

		/// <summary>
		/// Handles the BorderUpdated event of the _sizer control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _sizer_BorderUpdated(object sender, EventArgs e)
		{
			GenerateFont(true);
		}

		/// <summary>
		/// Function to display the gorgon logo.
		/// </summary>
		private void DisplayLogo(float frameTime)
		{
			VideoMode mode;			// Video mode.			

			try
			{
				// Get the current video mode.
				mode = Gorgon.VideoMode;

				// Draw the logo in the center.
				if (_logoOpacity < 255)
				{
					_logoOpacity += 85.33f * (frameTime / 1000.0f);
					_gorgonLogo.Opacity = (byte)(_logoOpacity);
				}
				else
					_gorgonLogo.Opacity = 255;
				_gorgonLogo.SetPosition(mode.Width / 2.0f, mode.Height / 2.0f);
				_gorgonLogo.Smoothing = Smoothing.Smooth;
				_gorgonLogo.UniformScale = mode.Width / (568 - containerMain.LeftToolStripPanel.Width - containerMain.RightToolStripPanel.Width) ;
				_gorgonLogo.Draw();
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, ErrorDialogIcons.Bug, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the OnFrameBegin event of the Screen control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.FrameEventArgs"/> instance containing the event data.</param>
		private void Screen_OnFrameBegin(object sender, FrameEventArgs e)
		{
			Gorgon.Screen.BackgroundColor = Drawing.Color.FromArgb(248, 245, 222);

			if (!menuitemTestText.Checked)
				DisplayLogo((float)Gorgon.Screen.TimingData.FrameDrawTime);
			else
				DisplayFont();
		}

		/// <summary>
		/// Function to composite and save the atlas image.
		/// </summary>
		private void SaveAtlas()
		{
			Drawing.Graphics graphics = Drawing.Graphics.FromImage(_atlas);		// Atlas graphics interface.

			try
			{
				// Erase the contents at the offset area.
				graphics.CompositingMode = CompositingMode.SourceCopy;
				// Place the font.				
				graphics.DrawImage(_fontBitmap, (int)_font.ImageOffset.X, (int)_font.ImageOffset.Y);

				// Save.
				GorgonDevIL.SaveBitmap(_atlasPath, _atlas);
				_fontImagePath = _atlasPath;
			}
			catch
			{
				throw;
			}
			finally
			{
				graphics.Dispose();
				graphics = null;
			}
		}

		/// <summary>
		/// Function to save the font.
		/// </summary>
		/// <param name="filename">Filename of the font.</param>
		private void SaveFont(string filename)
		{
			try
			{
				if (!_font.ImageIsAtlas)
				{
					if (_fontImagePath == string.Empty)
						_fontImagePath = Path.GetDirectoryName(filename) + @"\" + Path.GetFileNameWithoutExtension(filename) + ".png";
					else
						_fontImagePath = Path.GetDirectoryName(_fontImagePath) + @"\" + Path.GetFileNameWithoutExtension(filename) + ".png";

					// Save the font image.
					GorgonDevIL.SaveBitmap(_fontImagePath, _fontBitmap);
				}
				else
				{
					// If we have an atlas loaded, save it.
					if ((_atlasPath != string.Empty) && (_atlas != null) && (_atlasChange))
						SaveAtlas();
				}

				_font.Save(filename, _fontImagePath);

				// Set the last path used.
				SaveKey<string>("LastSavePath", Path.GetDirectoryName(filename));
				_fontFilename = filename;
				_changes = false;
				_atlasChange = false;
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, ErrorDialogIcons.Bug, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				ValidateMenu();
			}
		}

		/// <summary>
		/// Function to validate a filename.
		/// </summary>
		/// <param name="filename">Filename to validate.</param>
		/// <returns>Validated filename.</returns>
		private string ValidateFilename(string filename)
		{
			char[] badChars = Path.GetInvalidFileNameChars();		// Invalid filename characters.

			foreach (char ch in badChars)
				filename = filename.Replace(ch, '_');

			return filename;
		}

		/// <summary>
		/// Handles the CheckedChanged event of the buttonBold control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void buttonBold_CheckedChanged(object sender, EventArgs e)
		{
			if (!_noEvents)
			{
				_font.SetFontMetaData(_font.SourceFont, _font.SourceFontSize, buttonBold.Checked, _font.IsItalics, _font.IsUnderlined, _font.IsAntiAliased);
				GenerateFont(true);
			}
		}

		/// <summary>
		/// Handles the CheckedChanged event of the buttonItalics control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void buttonItalics_CheckedChanged(object sender, EventArgs e)
		{
			if (!_noEvents)
			{
				_font.SetFontMetaData(_font.SourceFont, _font.SourceFontSize, _font.IsBold, buttonItalics.Checked, _font.IsUnderlined, _font.IsAntiAliased);
				GenerateFont(true);
			}
		}

		/// <summary>
		/// Handles the CheckedChanged event of the buttonUnderlined control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void buttonUnderlined_CheckedChanged(object sender, EventArgs e)
		{
			if (!_noEvents)
			{
				_font.SetFontMetaData(_font.SourceFont, _font.SourceFontSize, _font.IsBold, _font.IsItalics, buttonUnderlined.Checked, _font.IsAntiAliased);
				GenerateFont(true);
			}
		}

		/// <summary>
		/// Handles the CheckedChanged event of the buttonAntiAlias control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonAntiAlias_CheckedChanged(object sender, EventArgs e)
		{
			if (!_noEvents)
			{
				_font.SetFontMetaData(_font.SourceFont, _font.SourceFontSize, _font.IsBold, _font.IsItalics, _font.IsUnderlined, buttonAntiAlias.Checked);
				GenerateFont(true);
				ValidateMenu();
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonZoomOut control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonZoomOut_Click(object sender, EventArgs e)
		{
			ZoomPercent -= 0.25f;
		}

		/// <summary>
		/// Handles the Click event of the buttonZoomIn control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonZoomIn_Click(object sender, EventArgs e)
		{
			ZoomPercent += 0.25f;
		}

		/// <summary>
		/// Handles the ValueChanged event of the scrollVertical control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void scrollVertical_ValueChanged(object sender, EventArgs e)
		{
			if (((_font.Name != string.Empty) && (!menuitemTestText.Checked)) || (menuitemViewAtlasImage.Checked))
				panelGorgon.Invalidate();
		}

		/// <summary>
		/// Handles the ValueChanged event of the scrollHorizontal control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void scrollHorizontal_ValueChanged(object sender, EventArgs e)
		{
			if (((_font.Name != string.Empty) && (!menuitemTestText.Checked)) || (menuitemViewAtlasImage.Checked))
				panelGorgon.Invalidate();
		}

		/// <summary>
		/// Handles the Paint event of the panelGorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
		private void panelGorgon_Paint(object sender, PaintEventArgs e)
		{
			Drawing.Graphics bufferGraphics = null;					// Buffer graphics interface.
			Drawing.Size fontSize = Drawing.Size.Empty;				// Size of the font bitmap.
			Drawing.Pen linePen = null;								// Line pen.
			Drawing.Brush rectBrush = null;							// Brush used for background.
			Drawing.Font font = null;								// Font for drawing.
			int maxY = 0;											// Maximum Y position.
			string imageDimensions = string.Empty;					// Image dimensions.

			base.OnPaint(e);

			try
			{
				// Display the font bitmap.
				if ((_font.Name != string.Empty) && (!menuitemTestText.Checked) && (!menuitemViewAtlasImage.Checked))
				{
					Gorgon.Stop();
					linePen = new Drawing.Pen(Drawing.Color.Cyan, 2.0f);
					bufferGraphics = Drawing.Graphics.FromImage(_bitmapBuffer);

					// Draw background.
					e.Graphics.Clear(Drawing.Color.FromArgb(255, 183, 211, 255));
					for (int i = 0; i < panelGorgon.ClientSize.Height; i += 4)
						e.Graphics.DrawLine(linePen, 0, i, panelGorgon.ClientSize.Width, i);

					// Copy font to the buffer.
					bufferGraphics.Clear(Drawing.Color.FromArgb(255, 0, 0, 0));
					bufferGraphics.DrawImage(_fontBitmap, 0, 0);

					fontSize = _fontBitmap.Size;
					fontSize = new Drawing.Size((int)(fontSize.Width * ZoomPercent), (int)(fontSize.Height * ZoomPercent));

					// Place the buffer on the screen.
					e.Graphics.InterpolationMode = Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
					e.Graphics.DrawImage(_bitmapBuffer, new Drawing.Rectangle(new Drawing.Point(-scrollHorizontal.Value, -scrollVertical.Value),fontSize));

					// Draw rectangles.
					for (int i = 0; i < _chars.Length; i++)
					{
						if (menuitemCharacterGrid.Checked)
							e.Graphics.DrawRectangle(Drawing.Pens.Red, (int)(((int)_chars[i].Left - 1) * ZoomPercent) - scrollHorizontal.Value, (int)(((int)_chars[i].Top - 1) * ZoomPercent) - scrollVertical.Value, (int)(((int)_chars[i].Width + 1) * ZoomPercent), (int)(((int)_chars[i].Height + 1) * ZoomPercent));
						maxY = (int)((_chars[i].Top + _chars[i].Height) * ZoomPercent);
					}

					if (maxY < fontSize.Height)
						maxY = fontSize.Height;
					linePen.Dispose();
					linePen = new Drawing.Pen(Drawing.Color.Red);
					linePen.DashStyle = DashStyle.DashDotDot;
					e.Graphics.DrawRectangle(linePen, -scrollHorizontal.Value - 1, -scrollVertical.Value - 1, fontSize.Width + 1, maxY + 1);
					
					// Draw image size and required size.
					font = new Drawing.Font("Arial", 12.0f,Drawing.FontStyle.Bold);
					maxY = (int)(maxY / ZoomPercent);
					imageDimensions = "(" + _fontBitmap.Size.Width.ToString() + "x" + _fontBitmap.Size.Height.ToString() + ")";
					e.Graphics.DrawString(imageDimensions, font, Drawing.Brushes.Black, fontSize.Width + 1 - scrollHorizontal.Value, fontSize.Height + 1 - scrollVertical.Value);
					e.Graphics.DrawString(imageDimensions, font, Drawing.Brushes.White, fontSize.Width - scrollHorizontal.Value, fontSize.Height - scrollVertical.Value);
					if (maxY > _fontBitmap.Size.Height)
					{
						imageDimensions = "(" + _fontBitmap.Size.Width.ToString() + "x" + maxY.ToString() + ")";
						e.Graphics.DrawString(imageDimensions, font, Drawing.Brushes.Black, fontSize.Width + 1 - scrollHorizontal.Value, (maxY * ZoomPercent) + 1 - scrollVertical.Value);
						e.Graphics.DrawString(imageDimensions, font, Drawing.Brushes.Red, fontSize.Width - scrollHorizontal.Value, (maxY * ZoomPercent) - scrollVertical.Value);
					}

					UpdateScrollBars(new Drawing.Size(fontSize.Width, (int)(maxY * ZoomPercent) + font.Height));
				}

				// Display the atlas image.
				if (menuitemViewAtlasImage.Checked)
				{
					Gorgon.Stop();
					linePen = new Drawing.Pen(Drawing.Color.Cyan, 2.0f);
					e.Graphics.Clear(Drawing.Color.FromArgb(255, 183, 211, 255));
					for (int i = 0; i < panelGorgon.ClientSize.Height; i += 4)
						e.Graphics.DrawLine(linePen, 0, i, panelGorgon.ClientSize.Width, i);

					rectBrush = new Drawing.SolidBrush(Drawing.Color.FromArgb(255,0,0,0));
					e.Graphics.FillRectangle(rectBrush, -scrollHorizontal.Value, -scrollVertical.Value, (int)(_atlas.Width * ZoomPercent), (int)(_atlas.Height * ZoomPercent));
					e.Graphics.DrawRectangle(Drawing.Pens.Red, -1 - scrollHorizontal.Value, -1 - scrollVertical.Value, (int)(_atlas.Width * ZoomPercent) + 1, (int)(_atlas.Height * ZoomPercent) + 1);

					e.Graphics.DrawImage(_atlas, -scrollHorizontal.Value, -scrollVertical.Value, (int)(_atlas.Width * ZoomPercent), (int)(_atlas.Height * ZoomPercent));
					rectBrush.Dispose();

					// Display where font will be placed.
					rectBrush = new Drawing.SolidBrush(Drawing.Color.FromArgb(128,255,0,255));
					e.Graphics.FillRectangle(rectBrush, (int)((_font.ImageOffset.X * ZoomPercent)- scrollHorizontal.Value), (int)((_font.ImageOffset.Y * ZoomPercent) - scrollVertical.Value), (int)(_fontBitmap.Width * ZoomPercent), (int)(_fontBitmap.Height * ZoomPercent));
					e.Graphics.DrawImage(_fontBitmap, (int)((_font.ImageOffset.X * ZoomPercent) - scrollHorizontal.Value), (int)((_font.ImageOffset.Y * ZoomPercent) - scrollVertical.Value), (int)(_fontBitmap.Width * ZoomPercent), (int)(_fontBitmap.Height * ZoomPercent));
					font = new Drawing.Font("Arial", 12.0f, Drawing.FontStyle.Bold);
					e.Graphics.DrawString(_font.ImageOffset.X.ToString("0") + "x" + _font.ImageOffset.Y.ToString("0"), font, Drawing.Brushes.Black, (int)((_font.ImageOffset.X * ZoomPercent) - scrollHorizontal.Value), (int)((_font.ImageOffset.Y * ZoomPercent) - scrollVertical.Value) - font.Height);
					e.Graphics.DrawString(_font.ImageOffset.X.ToString("0") + "x" + _font.ImageOffset.Y.ToString("0"), font, Drawing.Brushes.White, (int)((_font.ImageOffset.X * ZoomPercent) - scrollHorizontal.Value) - 2, (int)((_font.ImageOffset.Y * ZoomPercent) - scrollVertical.Value) - font.Height - 2);
					UpdateScrollBars(new Drawing.Size((int)(_atlas.Width * ZoomPercent), (int)(_atlas.Height * ZoomPercent)));
				}
			}
			finally
			{
				if (font != null)
					font.Dispose();
				if (rectBrush != null)
					rectBrush.Dispose();
				if (linePen != null)
					linePen.Dispose();
				if (bufferGraphics != null)
					bufferGraphics.Dispose();

				rectBrush = null;
				font = null;
				linePen = null;
				bufferGraphics = null;
			}
		}

		/// <summary>
		/// Handles the Click event of the zoom menu control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void zoomMenu_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem item = (ToolStripMenuItem)sender;		// Sender of the event.

			// Turn off each item.
			foreach (ToolStripMenuItem dropdown in dropdownZoom.DropDownItems)
				dropdown.Checked = false;

			item.Checked = true;
			switch (item.Text)
			{
				case "25%":
					ZoomPercent = 0.25f;
					break;
				case "50%":
					ZoomPercent = 0.50f;
					break;
				case "75%":
					ZoomPercent = 0.75f;
					break;
				case "100%":
					ZoomPercent = 1.0f;
					break;
				case "125%":
					ZoomPercent = 1.25f;
					break;
				case "150%":
					ZoomPercent = 1.50f;
					break;
				case "175%":
					ZoomPercent = 1.75f;
					break;
				case "200%":
					ZoomPercent = 2.0f;
					break;
			}
		}

		/// <summary>
		/// Function to get the toolstrip container.
		/// </summary>
		/// <param name="strip">Strip to use.</param>
		/// <returns>A string representing which container.</returns>
		private string GetStripContainer(ToolStrip strip)
		{
			if (strip.Parent == containerMain.BottomToolStripPanel)
				return "Bottom";
			if (strip.Parent == containerMain.LeftToolStripPanel)
				return "Left";
			if (strip.Parent == containerMain.RightToolStripPanel)
				return "Right";

			return "Top";
		}

		/// <summary>
		/// Handles the Click event of the menuitemNew control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemNew_Click(object sender, EventArgs e)
		{
			NewFont(true);
		}

		/// <summary>
		/// Function to load a font.
		/// </summary>
		/// <param name="filename">Filename of the font to load.</param>
		private void LoadFont(string filename)
		{
			try
			{
				// Destroy previous font.
				if (!NewFont(false))
					return;

				Gorgon.Stop();

				// Begin import.
				Cursor = Cursors.WaitCursor;

				// Load the font data.
				_font.Copy(_fonts.FromFile(filename));

				// Set the last path used.
				SaveKey<string>("LastOpenPath", Path.GetDirectoryName(filename));

				// Copy the image into the bitmap.
				MakeBuffers(_font.ImageSize);
				_fontFilename = filename;
				_chars = new Font.FontCharacter[_font.Count];
				for (int i = 0; i < _font.Count; i++)
					_chars[i] = _font[i];
				GenerateFont(false);

				panelGorgon.Invalidate();
				// Refresh the text if it's being displayed.
				if (menuitemTestText.Checked)
					menuitemTestText_Click(this, EventArgs.Empty);

				_changes = false;
				_atlasChange = false;
			}
			catch
			{
				throw;
			}
			finally
			{
				Gorgon.Go();
				ValidateMenu();
				Cursor = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuitemOpen control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemOpen_Click(object sender, EventArgs e)
		{
			try
			{
				Gorgon.Stop();

				// Bring up open file dialog.
				dialogOpenFile.Title = "Select a Gorgon font to load.";
				dialogOpenFile.InitialDirectory = ReadKey<string>("LastOpenPath", @".\");
				dialogOpenFile.AddExtension = true;
				dialogOpenFile.CheckFileExists = true;
				dialogOpenFile.CheckPathExists = true;
				dialogOpenFile.DefaultExt = "gorFont";
				dialogOpenFile.FileName = "";
				dialogOpenFile.FilterIndex = 0;
				dialogOpenFile.Filter = "Gorgon font. (*.gorFont)|*.gorFont|Gorgon XML font. (*.xml)|*.xml|All files. (*.*)|*.*";

				// Bring up show dialog.
				if (dialogOpenFile.ShowDialog() == DialogResult.OK)
				{
					if (!ConfirmChanges())
						return;
					LoadFont(dialogOpenFile.FileName);
				}
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, ErrorDialogIcons.Bug, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				panelGorgon.Invalidate();
				Gorgon.Go();
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemSave control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemSave_Click(object sender, EventArgs e)
		{
			try
			{
				Gorgon.Stop();
				Cursor = Cursors.WaitCursor;
				SaveFont(_fontFilename);			
			}
			finally
			{
				Gorgon.Go();
				Cursor = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuitemSaveAs control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemSaveAs_Click(object sender, EventArgs e)
		{
			try
			{
				Gorgon.Stop();

				// Bring up open file dialog.
				dialogSaveFile.Title = "Save Gorgon Font.";
				dialogSaveFile.InitialDirectory = ReadKey<string>("LastSavePath", @".\");
				dialogSaveFile.AddExtension = true;
				dialogSaveFile.CheckPathExists = true;
				dialogSaveFile.DefaultExt = "gorFont";
				if (_fontFilename != string.Empty)
				{
					if (Path.GetExtension(_fontFilename).ToLower() == ".xml")
						dialogSaveFile.FilterIndex = 2;

					dialogSaveFile.FileName = Path.GetFileNameWithoutExtension(_fontFilename);
				}
				else
				{
					dialogSaveFile.FilterIndex = 1;
					dialogSaveFile.FileName = ValidateFilename(_font.Name);
				}
				dialogSaveFile.Filter = "Gorgon font. (*.gorFont)|*.gorFont|Gorgon XML font. (*.xml)|*.xml|All files. (*.*)|*.*";

				// Bring up show dialog.
				if (dialogSaveFile.ShowDialog() == DialogResult.OK)
				{
					// If we have a font atlas with changes, ask if we want 
					if ((_font.ImageIsAtlas) && (_atlas != null) && (_atlasPath != string.Empty) && (_atlasChange))
					{
						_fontImagePath = string.Empty;
						if (!UI.YesNoBox("The font has been placed on an image atlas.\nWould you like to bind the font file to the atlas?"))
						{
							_font.ImageIsAtlas = false;
							_font.ImageOffset = SharpUtilities.Mathematics.Vector2D.Zero;
						}
					}

					if (!_font.ImageIsAtlas)
					{
						if (_fontImagePath == string.Empty)
							_fontImagePath = Path.GetDirectoryName(dialogSaveFile.FileName) + @"\" + Path.GetFileNameWithoutExtension(dialogSaveFile.FileName) + ".png";
						else
							_fontImagePath = Path.GetDirectoryName(_fontImagePath) + @"\" + Path.GetFileNameWithoutExtension(dialogSaveFile.FileName) + ".png";
					}

					Cursor = Cursors.WaitCursor;
					SaveFont(dialogSaveFile.FileName);
					SaveKey<string>("LastSavePath", Path.GetDirectoryName(dialogSaveFile.FileName));
				}
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, ErrorDialogIcons.Bug, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				Gorgon.Go();
				Cursor = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuitemExit control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemExit_Click(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Handles the Click event of the menuitemCharacterList control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemCharacterList_Click(object sender, EventArgs e)
		{
			CharacterSelection charSelector = null;		// Character selection interface.
			StringBuilder characters;					// Characters.

			try
			{
				Cursor = Cursors.WaitCursor;
				characters = new StringBuilder(_chars.Length);
				for (int i = 0; i < _chars.Length; i++)
					characters.Append(_chars[i].Character);
				charSelector = new CharacterSelection();
				charSelector.CharacterFont = _TTFont;
				charSelector.Characters = characters.ToString();

				// Show the selection dialog.
				if (charSelector.ShowDialog() == DialogResult.OK)
				{
					// Copy the characters.
					_chars = new Font.FontCharacter[charSelector.Characters.Length];
					for (int i = 0; i < charSelector.Characters.Length; i++)
					{
						_chars[i].CharacterOrdinal = (int)charSelector.Characters[i];
						_chars[i].Character = charSelector.Characters[i];
					}

					// Regenerate the font.
					GenerateFont(true);
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				Cursor = Cursors.Default;
				if (charSelector != null)
					charSelector.Dispose();

				charSelector = null;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuitemChangeFont control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemChangeFont_Click(object sender, EventArgs e)
		{
			try
			{
				dialogFont.Font = _TTFont;
				if (dialogFont.ShowDialog() == DialogResult.OK)
				{
					_font.SetFontMetaData(dialogFont.Font.Name, dialogFont.Font.Size, dialogFont.Font.Bold, dialogFont.Font.Italic, dialogFont.Font.Underline, _font.IsAntiAliased);
					GenerateFont(true);
					ValidateMenu();
				}
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, ErrorDialogIcons.Bug, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				Cursor = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuitemFontImageSize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemFontImageSize_Click(object sender, EventArgs e)
		{
			ImageSize imageSizer = null;		// Font image size interface.

			try
			{
				Gorgon.Stop();
				imageSizer = new ImageSize();
				imageSizer.FontImageSize = new Drawing.Size((int)_font.ImageSize.X, (int)_font.ImageSize.Y);

				if (imageSizer.ShowDialog(this) == DialogResult.OK)
				{
					_font.ImageSize = new Vector2D(imageSizer.FontImageSize.Width, imageSizer.FontImageSize.Height);
					MakeBuffers(_font.ImageSize);
					GenerateFont(true);
				}
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, ErrorDialogIcons.Bug, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				Gorgon.Go();
				ValidateMenu();
				Cursor = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuitemTestText control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemTestText_Click(object sender, EventArgs e)
		{
			Image fontImage = null;					// Font image.

			scrollVertical.Value = 0;
			scrollHorizontal.Value = 0;
			scrollHorizontal.Maximum = 32000;
			scrollVertical.Maximum = 32000;

			try
			{
				Cursor = Cursors.WaitCursor;
				if (menuitemTestText.Checked)
				{
					// Copy into the font image.
					if (_font.FontImage != null)
					{
						Gorgon.ImageManager.Remove(_font.FontImage.Name);
						_font.FontImage = null;
					}

					fontImage = Gorgon.ImageManager.FromBitmap(_font.Name + "_Image", _fontBitmap);
					_font.FontImage = fontImage;
					_font.UpdateCharacters(_chars);
					_exampleText.Font = _font;
					_exampleText.Text = string.Empty;
					Gorgon.Go();
				}
				else
				{
					Gorgon.Stop();
					panelGorgon.Invalidate();
				}
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, ErrorDialogIcons.Bug, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				ValidateMenu();
				Cursor = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuitemBorderSizes control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemBorderSizes_Click(object sender, EventArgs e)
		{
			try
			{
				if (menuitemBorderSizes.Checked)
				{
					_sizer.GorgonFont = _font;
					_sizer.Show();
				}
				else
					_sizer.Hide();
				SaveKey<bool>("borderOpen", menuitemBorderSizes.Checked);
				SaveKey<int>("borderX", _sizer.Left);
				SaveKey<int>("borderY", _sizer.Top);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonSave control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonSave_Click(object sender, EventArgs e)
		{
			if (_fontFilename != string.Empty)
				SaveFont(_fontFilename);
			else
				menuitemSaveAs_Click(sender, e);

			ValidateMenu();
		}

		/// <summary>
		/// Handles the Click event of the menuitemCharacterGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemCharacterGrid_Click(object sender, EventArgs e)
		{
			panelGorgon.Invalidate();
		}

		/// <summary>
		/// Handles the Click event of the menuitemAbout control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemAbout_Click(object sender, EventArgs e)
		{
			ABOOT aboutForm = null;		// About form.

			try
			{
				aboutForm = new ABOOT();
				aboutForm.ShowDialog(this);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				if (aboutForm != null)
					aboutForm.Dispose();
				aboutForm = null;
			}
		}

		/// <summary>
		/// Handles the Leave event of the textOutlineSize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void textOutlineSize_Leave(object sender, EventArgs e)
		{
			int size = 1;		// Size.

			try
			{
				if (!Utilities.IsNumeric(textOutlineSize.Text))
					textOutlineSize.Text = "1";
				else
				{
					int.TryParse(textOutlineSize.Text, out size);
					if (size <= 0)
						size = 1;
					if (size > 4)
						size = 4;
					textOutlineSize.Text = size.ToString();
				}

				_font.SetOutline(_font.IsOutlined, size, _font.OutlineColor);
				GenerateFont(true);
			}
			catch
			{
			}
		}

		/// <summary>
		/// Handles the KeyDown event of the textOutlineSize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void textOutlineSize_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				textOutlineSize_Leave(sender, e);
				e.Handled = true;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonOutline control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void buttonOutline_Click(object sender, EventArgs e)
		{
			if (_noEvents)
				return;

			_font.SetOutline(buttonOutline.Checked, _font.OutlineSize, _font.OutlineColor);
			ValidateMenu();
			GenerateFont(true);
			panelGorgon.Invalidate();
			labelOutlineColor.Owner.Refresh();
			labelOutlineColor.Invalidate();
		}

		/// <summary>
		/// Handles the Paint event of the labelOutlineColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
		private void labelOutlineColor_Paint(object sender, PaintEventArgs e)
		{
			Drawing.Graphics graphics = null;		// Graphics interface.
			Drawing.SolidBrush brush = null;		// Painting brush.

			try
			{
				if (labelOutlineColor.Image != null)
				{
					brush = new Drawing.SolidBrush(_font.OutlineColor);
					graphics = Drawing.Graphics.FromImage(labelOutlineColor.Image);
					graphics.DrawImage(Properties.Resources.Color, 0, 0);
					graphics.FillRectangle(brush, new Drawing.Rectangle(Drawing.Point.Empty, labelOutlineColor.Image.Size));
					graphics.DrawRectangle(Drawing.Pens.Black, 0, 0, labelOutlineColor.Image.Size.Width - 1, labelOutlineColor.Image.Size.Height - 1);
				}
			}
			finally
			{
				if (brush != null)
					brush.Dispose();
				brush = null;
				if (graphics != null)
					graphics.Dispose();
				graphics = null;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonOutlineColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void buttonOutlineColor_Click(object sender, EventArgs e)
		{
			ColorPicker dialogColor = null;		// Color picker.

			try
			{
				dialogColor = new ColorPicker();
				dialogColor.Color = _font.OutlineColor;
				if (dialogColor.ShowDialog(this) == DialogResult.OK)
				{
					_font.SetOutline(_font.IsOutlined, _font.OutlineSize, dialogColor.Color);
					GenerateFont(true);
					panelGorgon.Invalidate();
					labelOutlineColor.Owner.Refresh();
					labelOutlineColor.Invalidate();
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				if (dialogColor != null)
					dialogColor.Dispose();
				dialogColor = null;
			}
		}


		/// <summary>
		/// Handles the Click event of the menuitemChangeName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemChangeName_Click(object sender, EventArgs e)
		{
			FontMetaData metaData = null;			// Meta data screen.

			try
			{
				Gorgon.Stop();

				// Open the mudge import meta data screen.
				metaData = new FontMetaData();
				metaData.OnlyName = true;
				if (metaData.ShowDialog(this) == DialogResult.OK)
				{
					_font.SetName(metaData.FontName);
					_changes = true;
				}
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, ErrorDialogIcons.Bug, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				Gorgon.Go();
				if (metaData != null)
					metaData.Dispose();

				metaData = null;
				ValidateMenu();
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonShadow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonShadow_Click(object sender, EventArgs e)
		{
			if (_noEvents)
				return;

			_font.SetShadow(buttonShadow.Checked, _font.ShadowOffset, _font.ShadowColor, _font.ShadowDirection);
			ValidateMenu();
			GenerateFont(true);
			panelGorgon.Invalidate();
			labelShadowColor.Owner.Refresh();
			labelShadowColor.Invalidate();
		}

		/// <summary>
		/// Handles the Paint event of the labelShadowColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
		private void labelShadowColor_Paint(object sender, PaintEventArgs e)
		{
			Drawing.Graphics graphics = null;		// Graphics interface.
			Drawing.SolidBrush brush = null;		// Painting brush.

			try
			{
				if (labelShadowColor.Image != null)
				{
					brush = new Drawing.SolidBrush(_font.ShadowColor);
					graphics = Drawing.Graphics.FromImage(labelShadowColor.Image);
					graphics.DrawImage(Properties.Resources.Color, 0, 0);
					graphics.FillRectangle(brush, new Drawing.Rectangle(Drawing.Point.Empty, labelShadowColor.Image.Size));
					graphics.DrawRectangle(Drawing.Pens.Black, 0, 0, labelShadowColor.Image.Size.Width - 1, labelShadowColor.Image.Size.Height - 1);
				}
			}
			finally
			{
				if (brush != null)
					brush.Dispose();
				brush = null;
				if (graphics != null)
					graphics.Dispose();
				graphics = null;
			}
		}

		/// <summary>
		/// Handles the Leave event of the textShadowOffset control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void textShadowOffset_Leave(object sender, EventArgs e)
		{
			int offset = 2;		// Size.

			try
			{
				if (!Utilities.IsNumeric(textShadowOffset.Text))
					textShadowOffset.Text = "2";
				else
				{
					int.TryParse(textShadowOffset.Text, out offset);
					if (offset <= 0)
						offset = 1;
					if (offset > 10)
						offset = 10;
					textShadowOffset.Text = offset.ToString();
				}

				_font.SetShadow(_font.IsShadowed, offset, _font.ShadowColor, _font.ShadowDirection);
				GenerateFont(true);
			}
			catch
			{
			}
		}

		/// <summary>
		/// Handles the KeyDown event of the textShadowOffset control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void textShadowOffset_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				textShadowOffset_Leave(sender, e);
				e.Handled = true;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonShadowColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonShadowColor_Click(object sender, EventArgs e)
		{
			ColorPicker dialogColor = null;		// Color picker.

			try
			{
				dialogColor = new ColorPicker();
				dialogColor.Color = _font.ShadowColor;
				if (dialogColor.ShowDialog(this) == DialogResult.OK)
				{
					_font.SetShadow(_font.IsShadowed, _font.ShadowOffset, dialogColor.Color, _font.ShadowDirection);
					GenerateFont(true);
					panelGorgon.Invalidate();
					labelShadowColor.Owner.Refresh();
					labelShadowColor.Invalidate();
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				if (dialogColor != null)
					dialogColor.Dispose();
				dialogColor = null;
			}
		}

		/// <summary>
		/// Handles the Click event of the shadow direction menu items.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void shadowDirection_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem item = (ToolStripMenuItem)sender;		// Menu item that sent the event.
			try
			{
				if (_noEvents)
					return;

				switch (item.Name.ToLower())
				{
					case "menuitemleft":
						_font.SetShadow(_font.IsShadowed, _font.ShadowOffset, _font.ShadowColor, FontShadowDirection.MiddleLeft);
						break;
					case "menuitemright":
						_font.SetShadow(_font.IsShadowed, _font.ShadowOffset, _font.ShadowColor, FontShadowDirection.MiddleRight);
						break;
					case "menuitemupperleft":
						_font.SetShadow(_font.IsShadowed, _font.ShadowOffset, _font.ShadowColor, FontShadowDirection.UpperLeft);
						break;
					case "menuitemuppercenter":
						_font.SetShadow(_font.IsShadowed, _font.ShadowOffset, _font.ShadowColor, FontShadowDirection.UpperMiddle);
						break;
					case "menuitemupperright":
						_font.SetShadow(_font.IsShadowed, _font.ShadowOffset, _font.ShadowColor, FontShadowDirection.UpperRight);
						break;
					case "menuitemlowerleft":
						_font.SetShadow(_font.IsShadowed, _font.ShadowOffset, _font.ShadowColor, FontShadowDirection.LowerLeft);
						break;
					case "menuitemlowercenter":
						_font.SetShadow(_font.IsShadowed, _font.ShadowOffset, _font.ShadowColor, FontShadowDirection.LowerMiddle);
						break;
					case "menuitemlowerright":
						_font.SetShadow(_font.IsShadowed, _font.ShadowOffset, _font.ShadowColor, FontShadowDirection.LowerRight);
						break;
				}
				GenerateFont(true);
				panelGorgon.Invalidate();
				labelShadowColor.Owner.Refresh();
				labelShadowColor.Invalidate();
			}
			catch
			{
			}
			finally
			{
				ValidateMenu();
			}
		}

		/// <summary>
		/// Handles the Click event of the menuitemImportMudge control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemImportMudge_Click(object sender, EventArgs e)
		{
			FontMetaData metaData = null;			// Meta data screen.

			try
			{
				Font mudgeFont = null;					// Mudge font.				

				Gorgon.Stop();

				// Bring up open file dialog.
				dialogOpenFile.Title = "Select a mudge font to import.";
				dialogOpenFile.InitialDirectory = ReadKey<string>("LastImportPath", ReadKey<string>("LastOpenPath", @".\"));
				dialogOpenFile.AddExtension = true;
				dialogOpenFile.DefaultExt = "mudge";
				dialogOpenFile.FileName = "";
				dialogOpenFile.FilterIndex = 0;
				dialogOpenFile.Filter = "Mudge font (*.mudge)|*.mudge|Mudge XML font. (*.xml)|*.xml|All files. (*.*)|*.*";

				// Bring up show dialog.
				if (dialogOpenFile.ShowDialog() == DialogResult.OK)
				{
					if (!ConfirmChanges())
						return;

					// Open the mudge import meta data screen.
					metaData = new FontMetaData();
					metaData.checkUnderline.Enabled = false;
					if (File.Exists(Path.GetDirectoryName(dialogOpenFile.FileName) + @"\" + Path.GetFileNameWithoutExtension(dialogOpenFile.FileName) + ".tga"))
						metaData.FontImageSize = Image.GetImageDimensions(Path.GetDirectoryName(dialogOpenFile.FileName) + @"\" + Path.GetFileNameWithoutExtension(dialogOpenFile.FileName) + ".tga", true);
					else
						metaData.FontImageSize = new Drawing.Size((int)_font.ImageSize.X,(int)_font.ImageSize.Y);

					// Get the name of the font from the filename.
					metaData.FontName = Path.GetFileNameWithoutExtension(dialogOpenFile.FileName);

					if (Path.GetExtension(dialogOpenFile.FileName).ToLower() == ".xml")
					{
						metaData.OnlyName = false;
						if (metaData.ShowDialog(this) != DialogResult.OK)
							return;
					}
					else
						metaData.OnlyName = true;

					// Destroy previous font.
					if (!NewFont(false))
						return;

					// Begin import.
					Cursor = Cursors.WaitCursor;

					// Attempt to load the font.
					mudgeFont = new MudgeFont(metaData.FontName, dialogOpenFile.FileName);
				
					// Clone the font.
					_chars = new Font.FontCharacter[mudgeFont.Count];
					for (int i = 0; i < mudgeFont.Count; i++)
						_chars[i] = mudgeFont[i];
					_font.Copy(mudgeFont);
					_font.SetName(metaData.FontName);
					if (!metaData.OnlyName)
						_font.SetFontMetaData(metaData.SourceFont, metaData.SourceFontSize, metaData.IsBold, metaData.IsUnderlined, false, metaData.AntiAliased);
					_fontFilename = string.Empty;
					_fontImagePath = string.Empty;

					// Set the last path used.
					SaveKey<string>("LastImportPath", Path.GetDirectoryName(dialogOpenFile.FileName));

					// Store the image.
					MakeBuffers(new Vector2D(metaData.FontImageSize.Width, metaData.FontImageSize.Height));
					GenerateFont(true);

					// Refresh the text if it's being displayed.
					if (menuitemTestText.Checked)
						menuitemTestText_Click(this, EventArgs.Empty);

					// Default to 0 border offsets.
					if (!metaData.OnlyName)
						_font.SetBorderOffsets(0, 0, 0, 0);
					else
						_font.SetBorderOffsets(_font.LeftBorderOffset, _font.TopBorderOffset, _font.RightBorderOffset, _font.BottomBorderOffset);
					_changes = true;
				}
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, ErrorDialogIcons.Bug, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				Gorgon.Go();
				if (metaData != null)
					metaData.Dispose();

				metaData = null;
				ValidateMenu();
				Cursor = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuitemExportMudgeFont control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemExportMudgeFont_Click(object sender, EventArgs e)
		{
			try
			{
				Gorgon.Stop();
				
				// Bring up open file dialog.
				dialogSaveFile.Title = "Export to Mudge.";
				dialogSaveFile.InitialDirectory = ReadKey<string>("LastExportPath", ReadKey<string>("LastSavePath", @".\"));
				dialogSaveFile.AddExtension = true;
				dialogSaveFile.DefaultExt = "mudge";
				if (_fontFilename != string.Empty)
					dialogSaveFile.FileName = Path.GetFileNameWithoutExtension(_fontFilename);
				else
					dialogSaveFile.FileName = ValidateFilename(_font.Name);
				dialogSaveFile.FilterIndex = 1;
				dialogSaveFile.Filter = "Mudge font (*.mudge)|*.mudge|Mudge XML font. (*.xml)|*.xml|All files. (*.*)|*.*";

				// Bring up show dialog.
				if (dialogSaveFile.ShowDialog() == DialogResult.OK)
				{
					// Begin import.
					Cursor = Cursors.WaitCursor;
					if (Path.GetExtension(dialogSaveFile.FileName).ToLower() == ".xml")
						MudgeFont.SaveMudgeXML(_font, _fontBitmap, dialogSaveFile.FileName);
					else
						MudgeFont.SaveMudgeBinary(_font, _fontBitmap, dialogSaveFile.FileName);

					// Set the last path used.
					SaveKey<string>("LastExportPath", Path.GetDirectoryName(dialogSaveFile.FileName));
				}
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, ErrorDialogIcons.Bug, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				Gorgon.Go();
				ValidateMenu();
				Cursor = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuitemImportBM control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemImportBM_Click(object sender, EventArgs e)
		{
			try
			{
				Font bmFont = null;					// Mudge font.				

				Gorgon.Stop();

				// Bring up open file dialog.
				dialogOpenFile.Title = "Select a BM font to import.";
				dialogOpenFile.InitialDirectory = ReadKey<string>("LastImportPath", ReadKey<string>("LastOpenPath", @".\"));
				dialogOpenFile.AddExtension = true;
				dialogOpenFile.DefaultExt = "fnt";
				dialogOpenFile.FileName = "";
				dialogOpenFile.FilterIndex = 0;
				dialogOpenFile.Filter = "BM font (*.fnt)|*.fnt|All files. (*.*)|*.*";

				// Bring up show dialog.
				if (dialogOpenFile.ShowDialog() == DialogResult.OK)
				{
					if (!ConfirmChanges())
						return;

					// Destroy previous font.
					if (!NewFont(false))
						return;

					// Get the name of the font from the filename.
					_font.SetName(Path.GetFileNameWithoutExtension(dialogOpenFile.FileName));

					// Begin import.
					Cursor = Cursors.WaitCursor;

					// Attempt to load the font.
					bmFont = new BMFont(_font.Name, dialogOpenFile.FileName);

					_chars = new Font.FontCharacter[bmFont.Count];
					for (int i = 0; i < bmFont.Count; i++)
						_chars[i] = bmFont[i];
					_font.Copy(bmFont);
					_fontFilename = string.Empty;
					_fontImagePath = string.Empty;

					// Set the last path used.
					SaveKey<string>("LastImportPath", Path.GetDirectoryName(dialogOpenFile.FileName));

					// Store the image.
					MakeBuffers(_font.ImageSize);
					GenerateFont(true);

					// Refresh the text if it's being displayed.
					if (menuitemTestText.Checked)
						menuitemTestText_Click(this, EventArgs.Empty);

					// Default to 0 border offsets.
					_font.SetBorderOffsets(_font.LeftBorderOffset, _font.TopBorderOffset, _font.RightBorderOffset, _font.BottomBorderOffset);
					_changes = true;
				}
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, ErrorDialogIcons.Bug, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				Gorgon.Go();
				ValidateMenu();
				Cursor = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuitemOpenAtlas control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemOpenAtlas_Click(object sender, EventArgs e)
		{
			try
			{
				// Confirm.
				if (_atlas != null)
				{
					if (!UI.YesNoBox("There is an atlas already loaded, loading this one will replace it.\nAre you sure this is what you want to do?"))
						return;
					if ((_font != null) && (_font.ImageIsAtlas))
						_changes = true;
					_atlasChange = false;
				}

				// Bring up open file dialog.
				dialogOpenFile.Title = "Select an image to load.";
				dialogOpenFile.InitialDirectory = ReadKey<string>("LastAtlasPath", ReadKey<string>("LastOpenPath", @".\"));
				dialogOpenFile.AddExtension = true;
				dialogOpenFile.DefaultExt = "png";
				dialogOpenFile.FileName = "";
				dialogOpenFile.Filter = "Portable Network Graphics. (*.png)|*.png|Targa. (*.tga)|*.tga|Tagged Image File Format. (*.tif)|*.tif|Bitmap. (*.bmp)|*.bmp|Direct X Surface (*.dds)|*.dds|All files. (*.*)|*.*";

				if (dialogOpenFile.ShowDialog(this) == DialogResult.OK)
				{
					// Load the image into the atlas buffer.
					_atlas = GorgonDevIL.LoadBitmap(dialogOpenFile.FileName);

					// Reset the image offset.
					if (_font != null)
					{
						_font.ImageIsAtlas = false;
						_font.ImageOffset = new SharpUtilities.Mathematics.Vector2D(0, 0);
						_atlasChange = false;
					}

					_atlasPath = dialogOpenFile.FileName;
					SaveKey<string>("LastAtlasPath", Path.GetDirectoryName(dialogOpenFile.FileName));

					if (menuitemViewAtlasImage.Checked)
					{
						// Refresh the atlas.
						menuitemViewAtlasImage.Checked = false;
						menuitemViewAtlasImage.Checked = true;
					}
				}
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, ErrorDialogIcons.Bug, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				ValidateMenu();
			}
		}

		/// <summary>
		/// Handles the Click event of the menuitemRemoveAtlas control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemRemoveAtlas_Click(object sender, EventArgs e)
		{
			if (UI.YesNoBox("Are you sure you want to remove the atlas?"))
			{
				menuitemViewAtlasImage.Checked = false;

				if ((_font != null) && (_font.ImageIsAtlas))
				{
					// Reset the image offset.
					_font.ImageOffset = new SharpUtilities.Mathematics.Vector2D(0, 0);
					_font.ImageIsAtlas = false;
					_changes = true;
				}

				if (_atlas != null)
					_atlas.Dispose();
				_atlas = null;
				_atlasPath = string.Empty;
				panelGorgon.Invalidate();
				ValidateMenu();
			}
		}

		/// <summary>
		/// Handles the CheckedChanged event of the menuitemViewAtlasImage control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemViewAtlasImage_CheckedChanged(object sender, EventArgs e)
		{
			if (_noEvents)
				return;
			if (menuitemTestText.Checked)
			{
				_noEvents = true;
				Gorgon.Stop();
				menuitemTestText.Checked = false;
				_noEvents = false;
			}
			panelGorgon.Invalidate();
			if (!menuitemViewAtlasImage.Checked)
				Gorgon.Go();

			if ((_font != null) && (_font.ImageIsAtlas))
			{
				scrollHorizontal.Value = (int)_font.ImageOffset.X;
				scrollVertical.Value = (int)_font.ImageOffset.Y;
			}
			
			ValidateMenu();
		}


		/// <summary>
		/// Handles the Click event of the menuitemPlaceOnAtlas control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemPlaceOnAtlas_Click(object sender, EventArgs e)
		{
			AtlasEmbedder atlasEmbed = null;		// Atlas embedding mechanism.

			try
			{
				atlasEmbed = new AtlasEmbedder();
				atlasEmbed.FontBitmap = _fontBitmap;
				atlasEmbed.AtlasBitmap = _atlas;
				if (_font.ImageIsAtlas)
					atlasEmbed.Placement = new Drawing.Point((int)_font.ImageOffset.X, (int)_font.ImageOffset.Y);
				if (atlasEmbed.ShowDialog(this) == DialogResult.OK)
				{
					_font.ImageIsAtlas = true;
					_font.ImageOffset = new SharpUtilities.Mathematics.Vector2D(atlasEmbed.Placement.X, atlasEmbed.Placement.Y);
					_atlasChange = true;
					_changes = true;
					panelGorgon.Invalidate();
				}
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, ErrorDialogIcons.Bug, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				if (atlasEmbed != null)
					atlasEmbed.Dispose();
				atlasEmbed = null;
				ValidateMenu();
			}
		}

		/// <summary>
		/// Handles the Click event of the menuitemUnbindAtlas control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemUnbindAtlas_Click(object sender, EventArgs e)
		{
			_font.ImageIsAtlas = false;
			_font.ImageOffset = Vector2D.Zero;
			_changes = true;
			_atlasChange = false;
			if (menuitemViewAtlasImage.Checked)
			{
				menuitemViewAtlasImage.Checked = false;
				menuitemViewAtlasImage.Checked = true;
			}
			ValidateMenu();
		}

		/// <summary>
		/// Handles the Click event of the menuitemSaveAtlas control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemSaveAtlas_Click(object sender, EventArgs e)
		{
			try
			{
				// Bring up open file dialog.
				dialogSaveFile.Title = "Select an image to load.";
				dialogSaveFile.InitialDirectory = ReadKey<string>("LastAtlasPath", ReadKey<string>("LastSavePath", @".\"));
				dialogSaveFile.AddExtension = true;
				dialogSaveFile.DefaultExt = "";
				dialogSaveFile.FileName = Path.GetFileNameWithoutExtension(_atlasPath);
				dialogSaveFile.Filter = "Portable Network Graphics. (*.png)|*.png|Targa. (*.tga)|*.tga|Tagged Image File Format. (*.tif)|*.tif|Bitmap. (*.bmp)|*.bmp|Direct X Surface (*.dds)|*.dds|All files. (*.*)|*.*";

				if (dialogSaveFile.ShowDialog(this) == DialogResult.OK)
				{
					_atlasPath = dialogSaveFile.FileName;
					if (UI.YesNoBox("Do you wish to bind this image atlas to the font?"))
						_fontImagePath = _atlasPath;
					else
					{
						_font.ImageIsAtlas = false;
						_font.ImageOffset = Vector2D.Zero;
						_fontImagePath = string.Empty;
					}

					// Save the image.
					_changes = true;
					GorgonDevIL.SaveBitmap(dialogSaveFile.FileName, _atlas);

					SaveKey<string>("LastAtlasPath", Path.GetDirectoryName(dialogSaveFile.FileName));
					_atlasChange = false;
				}
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, ErrorDialogIcons.Bug, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				ValidateMenu();
			}
		}

		/// <summary>
		/// Handles the CheckedChanged event of the menuitemTestText control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuitemTestText_CheckedChanged(object sender, EventArgs e)
		{
			if (_noEvents)
				return;
			_noEvents = true;
			if (menuitemViewAtlasImage.Checked)
				menuitemViewAtlasImage.Checked = false;
			_noEvents = false;
			panelGorgon.Invalidate();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (DesignMode)
				return;

			// Terminate the gorgon interface.
			try
			{
				e.Cancel = !ConfirmChanges();

				if (!e.Cancel)
				{
					if (_fontBitmap != null)
						_fontBitmap.Dispose();

					if (_bitmapBuffer != null)
						_bitmapBuffer.Dispose();

					if (_atlas != null)
						_atlas.Dispose();

					if (_TTFont != null)
						_TTFont.Dispose();

					SaveKey<int>("FileX", stripFile.Left);
					SaveKey<int>("FileY", stripFile.Top);
					SaveKey<int>("FontX", stripFont.Left);
					SaveKey<int>("FontY", stripFont.Top);
					SaveKey<int>("ZoomX", stripZoom.Left);
					SaveKey<int>("ZoomY", stripZoom.Top);
					SaveKey<int>("SpecialX", stripSpecial.Left);
					SaveKey<int>("SpecialY", stripSpecial.Top);
					SaveKey<string>("FileLayout", GetStripContainer(stripFile));
					SaveKey<string>("FontLayout", GetStripContainer(stripFont));
					SaveKey<string>("ZoomLayout", GetStripContainer(stripZoom));
					SaveKey<string>("SpecialLayout", GetStripContainer(stripSpecial));

					Gorgon.Terminate();
				}
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, ErrorDialogIcons.Bug, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			string[] arguments = null;		// Commandline arguments.

			base.OnLoad(e);

			if (!DesignMode)
			{
				// Initialize Gorgon.
				try
				{
					// Create font manager.
					_fonts = new FontManager();

					Cursor = Cursors.WaitCursor;
					Gorgon.Initialize(true, true);
					Gorgon.SetMode(panelGorgon);

					// Load default data.
					LoadDefaults();

					arguments = Environment.GetCommandLineArgs();

					if (arguments.Length > 1)
					{
						try
						{
							// Attempt to load the font.
							LoadFont(arguments[1]);
						}
						catch (SharpException sEx)
						{
							UI.ErrorBox(this, ErrorDialogIcons.Disk, sEx.Message, sEx.ErrorLog);
						}
					}

					// Assign events.				
					Gorgon.Screen.OnFrameBegin += new FrameEventHandler(Screen_OnFrameBegin);

					// Begin rendering.
					Gorgon.Screen.BackgroundColor = Drawing.Color.FromArgb(248, 245, 222);
					Gorgon.Go();

					labelFontColor.Owner.Refresh();
					labelFontColor.Invalidate();
				}
				catch (SharpException sEx)
				{
					UI.ErrorBox(this, ErrorDialogIcons.Bug, sEx.Message, sEx.ErrorLog);
					Application.Exit();
				}
				catch (Exception ex)
				{
					UI.ErrorBox(this, ex);
					Application.Exit();
				}
				finally
				{
					Cursor = Cursors.Default;
				}
			}
		}

		/// <summary>
		/// Raises the resize event.
		/// </summary>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			if ((_exampleText != null) && (menuitemTestText.Checked))
			{
				if (scrollHorizontal.Visible)
				{
					if (scrollHorizontal.Value >= scrollHorizontal.Maximum)
						scrollHorizontal.Value = scrollHorizontal.Maximum;
				}
				else
					scrollHorizontal.Value = 0;

				if (scrollVertical.Visible)
				{
					if (scrollVertical.Value >= scrollVertical.Maximum)
						scrollVertical.Value = scrollVertical.Maximum;
				}
				else
					scrollVertical.Value = 0;
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor.
		/// </summary>
		public FontEditor()
		{
			InitializeComponent();
		}
		#endregion
	}
}