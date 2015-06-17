#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Thursday, March 07, 2013 9:51:37 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Editor.FontEditorPlugIn.Properties;
using Gorgon.Graphics;
using Gorgon.Input;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Timing;
using Gorgon.UI;

namespace Gorgon.Editor.FontEditorPlugIn
{
    /// <summary>
    /// Control for displaying font data.
    /// </summary>
    partial class GorgonFontContentPanel 
		: ContentPanel
	{
		#region Value Types.
		/// <summary>
		/// Kerning pair combo box item.
		/// </summary>
	    private struct KernPairComboItem
			: IEquatable<KernPairComboItem>
		{
			#region Variables.
			// The kerning pair for this item.
			private readonly GorgonKerningPair _pair;
			#endregion

			#region Methods.
			/// <summary>
			/// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
			/// </summary>
			/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
			/// <returns>
			///   <b>true</b> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <b>false</b>.
			/// </returns>
			public override bool Equals(object obj)
			{
				if (obj is KernPairComboItem)
				{
					return ((KernPairComboItem)obj).Equals(this);
				}
				return base.Equals(obj);
			}

			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <returns>
			/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
			/// </returns>
			public override int GetHashCode()
			{
				return _pair.GetHashCode();
			}

			/// <summary>
			/// Returns a <see cref="System.String" /> that represents this instance.
			/// </summary>
			/// <returns>
			/// A <see cref="System.String" /> that represents this instance.
			/// </returns>
			public override string ToString()
			{
				return _pair.RightCharacter.ToString(CultureInfo.CurrentUICulture);
			}

			/// <summary>
			/// Operator to convert a kerning pair value to a kerning combo item.
			/// </summary>
			/// <param name="pair">Kerning pair to convert.</param>
			/// <returns>The combo item.</returns>
			public static implicit operator KernPairComboItem(GorgonKerningPair pair)
			{
				return new KernPairComboItem(pair);
			}

			/// <summary>
			/// Operator to convert a kern combo item to a kerning pair.
			/// </summary>
			/// <param name="pair">Kerning combo item to convert..</param>
			/// <returns>The kerning pair.</returns>
			public static implicit operator GorgonKerningPair(KernPairComboItem pair)
			{
				return pair._pair;
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="KernPairComboItem"/> struct.
			/// </summary>
			/// <param name="pair">The kerning pair for this item.</param>
			private KernPairComboItem(GorgonKerningPair pair)
			{
				_pair = pair;
			}
			#endregion

			#region IEquatable<KernPairComboItem> Members
			/// <summary>
			/// Indicates whether the current object is equal to another object of the same type.
			/// </summary>
			/// <param name="other">An object to compare with this object.</param>
			/// <returns>
			/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
			/// </returns>
			public bool Equals(KernPairComboItem other)
			{
				return _pair.Equals(other._pair);
			}
			#endregion
		}
		#endregion

		#region Variables.
		private GorgonKeyboard _rawKeyboard;
	    private GorgonPointingDevice _rawMouse;
		private bool _disposed;
	    private GorgonTexture2D _pattern;
	    private float _currentZoom = -1;
	    private int _currentTextureIndex;
	    private float _indexTransition;
	    private readonly GorgonFontContent _content;
	    private GorgonText _text;
	    private GorgonSprite _patternSprite;
	    private Dictionary<GorgonGlyph, RectangleF> _glyphRegions;
	    private Vector2 _textureOffset = Vector2.Zero;
	    private RectangleF _textureRegion = RectangleF.Empty;
	    private GorgonGlyph _hoverGlyph;
	    private Vector2 _selectorBackGroundPos = Vector2.Zero;
	    private GorgonGlyph _selectedGlyph;
	    private GorgonTexture2D[] _textures;
	    private GorgonAnimationController<GorgonSprite> _editGlyphAnimation;
		private GorgonAnimationController<GorgonSprite> _editBackgroundAnimation;
        private List<GorgonSprite> _textureSprites;
	    private GorgonSprite _glyphSprite;
	    private GorgonSprite _glyphKernSprite;
	    private GorgonSprite _glyphBackgroundSprite;
        private IEnumerable<GorgonSprite> _sortedTextures;
        private DrawState _nextState = DrawState.DrawFontTextures;
	    private Point _lastScrollPoint = Point.Empty;
	    private Clipper _glyphClipper;
	    private ZoomWindow _zoomWindow;
	    private GorgonFont _zoomFont;
	    private Vector2 _mousePosition;
	    private GorgonGlyph _newGlyph;
	    private GorgonGlyph _kernGlyph;
        private Font _kernComboFont;
	    private BlendingMode _previewBlendMode = BlendingMode.Modulate;
        #endregion

		#region Properties.
		/// <summary>
		/// Property to return the mid point for the render panel.
		/// </summary>
	    private Vector2 PanelMidPoint
	    {
		    get
		    {
			    return new Vector2(panelTextures.ClientSize.Width * 0.5f, panelTextures.ClientSize.Height * 0.5f);
		    }
	    }

		/// <summary>
		/// Property to return the currently selected texture.
		/// </summary>
	    private GorgonTexture2D CurrentTexture
	    {
		    get
		    {
			    if ((_currentTextureIndex < 0)
			        || (_currentTextureIndex >= _textures.Length))
			    {
				    return null;
			    }

			    return _textures[_currentTextureIndex];
		    }
	    }
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Click event of the buttonSave control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonSave_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				_content.Commit();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboBlendMode control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void comboBlendMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			_previewBlendMode = string.Equals(comboBlendMode.Text, Resources.GORFNT_TEXT_BLEND_MOD, StringComparison.CurrentCultureIgnoreCase)
				                    ? BlendingMode.Modulate
				                    : BlendingMode.Additive;

			GorgonFontEditorPlugIn.Settings.BlendMode = comboBlendMode.Text;
		}

	    /// <summary>
        /// Handles the Click event of the buttonRevert control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonRevert_Click(object sender, EventArgs e)
        {
            if (GorgonDialogs.ConfirmBox(ParentForm, Resources.GORFNT_DLG_CONFIRM_REVERT) == ConfirmationResult.No)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            try
            {
                _content.Revert();
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
		/// Handles the Load event of the GorgonFontContentPanel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void GorgonFontContentPanel_Load(object sender, EventArgs e)
		{
			try
			{
				Point lastPosition = Cursor.Position;
				_rawMouse = RawInput.CreatePointingDevice(panelTextures);
				_rawMouse.Enabled = true;
				_rawMouse.Acquired = false;
				_rawMouse.PointingDeviceMove += ClipMouseMove;
                _rawMouse.PointingDeviceDown += ClipMouseDown;
                _rawMouse.PointingDeviceUp += ClipMouseUp;
                _rawMouse.PointingDeviceWheelMove += ClipMouseWheel;
				Cursor.Position = lastPosition;

				_rawKeyboard = RawInput.CreateKeyboard(panelTextures);
				_rawKeyboard.Enabled = true;
				_rawKeyboard.KeyUp += GorgonFontContentPanel_KeyUp;
				_rawKeyboard.KeyDown += GorgonFontContentPanel_KeyDown;

				_glyphClipper = new Clipper(_content.Renderer, panelTextures)
				                {
					                DefaultCursor = Cursors.Cross
				                };

				// Default scrolling to off.
				CheckForScroll(Size.Empty);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
				_content.CloseContent();
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonEditPreviewText control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonEditPreviewText_Click(object sender, EventArgs e)
		{
			FormEditPreviewText textEditor = null;
			try
			{
				textEditor = new FormEditPreviewText
				             {
					             PreviewText = textPreviewText.Text
				             };

				if (textEditor.ShowDialog(ParentForm) != DialogResult.OK)
				{
					return;
				}

				GorgonFontEditorPlugIn.Settings.SampleText = textPreviewText.Text = textEditor.PreviewText;
				textPreviewText.SelectAll();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				if (textEditor != null)
				{
					textEditor.Dispose();
				}
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonSearchGlyph control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonSearchGlyph_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				if ((string.IsNullOrEmpty(comboSearchGlyph.Text)) || (!_content.Font.Glyphs.Contains(comboSearchGlyph.Text[0])))
				{
					return;
				}

				if ((_selectedGlyph != null)
				    && (_selectedGlyph.Character == comboSearchGlyph.Text[0]))
				{
					return;
				}

				_selectedGlyph = _content.Font.Glyphs[comboSearchGlyph.Text[0]];

				if (_selectedGlyph.Texture == _textures[_currentTextureIndex])
				{
					return;
				}

				_currentTextureIndex = Array.IndexOf(_textures, _selectedGlyph.Texture);

				if (_currentTextureIndex == -1)
				{
					return;
				}

				_content.CurrentState = DrawState.NextTexture;
				_nextState = DrawState.DrawFontTextures;
				ActiveControl = panelTextures;
				UpdateGlyphRegions();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the TextUpdate event of the comboSearchGlyph control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void comboSearchGlyph_TextUpdate(object sender, EventArgs e)
		{
			ValidateControls();
		}

		/// <summary>
		/// Handles the KeyDown event of the textSearchGlyph control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
		private void comboSearchGlyph_KeyDown(object sender, KeyEventArgs e)
		{
			
			try
			{
				if ((string.IsNullOrEmpty(comboSearchGlyph.Text))
					|| (e.KeyCode != Keys.Enter))
				{
					return;
				}

				buttonSearchGlyph.PerformClick();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the KeyDown event of the GorgonFontContentPanel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyboardEventArgs"/> instance containing the event data.</param>
	    private void GorgonFontContentPanel_KeyDown(object sender, KeyboardEventArgs e)
	    {
			switch (e.Key)
			{
				case KeyboardKeys.Space:
					if (_content.CurrentState != DrawState.ClipGlyph)
					{
						return;
					}

					var args = new PointingDeviceEventArgs(PointingDeviceButtons.Left,
					                                       PointingDeviceButtons.None,
					                                       _mousePosition,
					                                       0,
					                                       Vector2.Zero,
					                                       0,
					                                       1);

					if (_glyphClipper.DragMode == ClipSelectionDragMode.None)
					{
						ClipMouseDown(sender, args);
					}
					else
					{
						ClipMouseUp(sender, args);
					}
					break;
				case KeyboardKeys.NumPad3:
					if (_content.CurrentState != DrawState.ClipGlyph)
					{
						return;
					}

					if (ActiveControl != panelTextures)
					{
						panelTextures.Focus();
					}

					if (e.Shift)
					{
						_mousePosition.X += 100;
						_mousePosition.Y += 100;
					}
					else if (e.Ctrl)
					{
						_mousePosition.X += 10;
						_mousePosition.Y += 10;
					}
					else
					{
						_mousePosition.X += 1;
						_mousePosition.Y += 1;
					}

					MoveClipperDrag(new PointingDeviceEventArgs(PointingDeviceButtons.Left,
					                                            PointingDeviceButtons.None,
					                                            _mousePosition,
					                                            0,
					                                            Vector2.Zero,
					                                            0,
					                                            1));
					Cursor.Position = panelTextures.PointToScreen(new Point((int)_mousePosition.X, (int)_mousePosition.Y));
					break;
				case KeyboardKeys.NumPad1:
					if (_content.CurrentState != DrawState.ClipGlyph)
					{
						return;
					}

					if (ActiveControl != panelTextures)
					{
						panelTextures.Focus();
					}

					if (e.Shift)
					{
						_mousePosition.X -= 100;
						_mousePosition.Y += 100;
					}
					else if (e.Ctrl)
					{
						_mousePosition.X -= 10;
						_mousePosition.Y += 10;
					}
					else
					{
						_mousePosition.X -= 1;
						_mousePosition.Y += 1;
					}

					MoveClipperDrag(new PointingDeviceEventArgs(PointingDeviceButtons.Left,
					                                            PointingDeviceButtons.None,
					                                            _mousePosition,
					                                            0,
					                                            Vector2.Zero,
					                                            0,
					                                            1));
					Cursor.Position = panelTextures.PointToScreen(new Point((int)_mousePosition.X, (int)_mousePosition.Y));
					break;
				case KeyboardKeys.NumPad9:
					if (_content.CurrentState != DrawState.ClipGlyph)
					{
						return;
					}

					if (ActiveControl != panelTextures)
					{
						panelTextures.Focus();
					}

					if (e.Shift)
					{
						_mousePosition.X += 100;
						_mousePosition.Y -= 100;
					}
					else if (e.Ctrl)
					{
						_mousePosition.X += 10;
						_mousePosition.Y -= 10;
					}
					else
					{
						_mousePosition.X += 1;
						_mousePosition.Y -= 1;
					}

					MoveClipperDrag(new PointingDeviceEventArgs(PointingDeviceButtons.Left,
					                                            PointingDeviceButtons.None,
					                                            _mousePosition,
					                                            0,
					                                            Vector2.Zero,
					                                            0,
					                                            1));
					Cursor.Position = panelTextures.PointToScreen(new Point((int)_mousePosition.X, (int)_mousePosition.Y));
					break;
				case KeyboardKeys.NumPad7:
					if (_content.CurrentState != DrawState.ClipGlyph)
					{
						return;
					}

					if (ActiveControl != panelTextures)
					{
						panelTextures.Focus();
					}

					if (e.Shift)
					{
						_mousePosition.X -= 100;
						_mousePosition.Y -= 100;
					}
					else if (e.Ctrl)
					{
						_mousePosition.X -= 10;
						_mousePosition.Y -= 10;
					}
					else
					{
						_mousePosition.X -= 1;
						_mousePosition.Y -= 1;
					}

					MoveClipperDrag(new PointingDeviceEventArgs(PointingDeviceButtons.Left,
					                                            PointingDeviceButtons.None,
					                                            _mousePosition,
					                                            0,
					                                            Vector2.Zero,
					                                            0,
					                                            1));
					Cursor.Position = panelTextures.PointToScreen(new Point((int)_mousePosition.X, (int)_mousePosition.Y));
					break;
				case KeyboardKeys.NumPad6:
				case KeyboardKeys.Right:
					if (_content.CurrentState != DrawState.ClipGlyph)
					{
						return;
					}

					if (ActiveControl != panelTextures)
					{
						panelTextures.Focus();
					}

					if (e.Shift)
					{
						_mousePosition.X += 100;
					}
					else if (e.Ctrl)
					{
						_mousePosition.X += 10;
					}
					else
					{
						_mousePosition.X += 1;
					}

					MoveClipperDrag(new PointingDeviceEventArgs(PointingDeviceButtons.Left,
					                                            PointingDeviceButtons.None,
					                                            _mousePosition,
					                                            0,
					                                            Vector2.Zero,
					                                            0,
					                                            1));
					Cursor.Position = panelTextures.PointToScreen(new Point((int)_mousePosition.X, (int)_mousePosition.Y));
					break;
				case KeyboardKeys.NumPad4:
				case KeyboardKeys.Left:
					if (_content.CurrentState != DrawState.ClipGlyph)
					{
						return;
					}

					if (ActiveControl != panelTextures)
					{
						panelTextures.Focus();
					}


					if (e.Shift)
					{
						_mousePosition.X -= 100;
					}
					else if (e.Ctrl)
					{
						_mousePosition.X -= 10;
					}
					else
					{
						_mousePosition.X -= 1;
					}

					MoveClipperDrag(new PointingDeviceEventArgs(PointingDeviceButtons.Left,
					                                            PointingDeviceButtons.None,
					                                            _mousePosition,
					                                            0,
					                                            Vector2.Zero,
					                                            0,
					                                            1));
					Cursor.Position = panelTextures.PointToScreen(new Point((int)_mousePosition.X, (int)_mousePosition.Y));
					break;
				case KeyboardKeys.NumPad2:
				case KeyboardKeys.Down:
					if (_content.CurrentState != DrawState.ClipGlyph)
					{
						return;
					}

					if (ActiveControl != panelTextures)
					{
						panelTextures.Focus();
					}

					if (e.Shift)
					{
						_mousePosition.Y += 100;
					}
					else if (e.Ctrl)
					{
						_mousePosition.Y += 10;
					}
					else
					{
						_mousePosition.Y += 1;
					}

					MoveClipperDrag(new PointingDeviceEventArgs(PointingDeviceButtons.Left,
					                                            PointingDeviceButtons.None,
					                                            _mousePosition,
					                                            0,
					                                            Vector2.Zero,
					                                            0,
					                                            1));
					Cursor.Position = panelTextures.PointToScreen(new Point((int)_mousePosition.X, (int)_mousePosition.Y));
					break;
				case KeyboardKeys.NumPad8:
				case KeyboardKeys.Up:
					if (_content.CurrentState != DrawState.ClipGlyph)
					{
						return;
					}

					if (ActiveControl != panelTextures)
					{
						panelTextures.Focus();
					}

					if (e.Shift)
					{
						_mousePosition.Y -= 100;
					}
					else if (e.Ctrl)
					{
						_mousePosition.Y -= 10;
					}
					else
					{
						_mousePosition.Y -= 1;
					}

					MoveClipperDrag(new PointingDeviceEventArgs(PointingDeviceButtons.Left,
					                                            PointingDeviceButtons.None,
					                                            _mousePosition,
					                                            0,
					                                            Vector2.Zero,
					                                            0,
					                                            1));
					Cursor.Position = panelTextures.PointToScreen(new Point((int)_mousePosition.X, (int)_mousePosition.Y));
					break;
			}
	    }

	    /// <summary>
		/// Function to enable or disable the clipper numeric control limits.
		/// </summary>
		/// <param name="enable"><b>true</b> to enable the control limits, <b>false</b> to turn off the limits.</param>
	    private void EnableClipNumericLimits(bool enable)
	    {
			if ((!enable)
				|| (_glyphClipper == null)
				|| (_newGlyph == null))
			{
				numericGlyphHeight.Maximum = numericGlyphWidth.Maximum = numericGlyphLeft.Maximum = numericGlyphTop.Maximum = Int32.MaxValue;
				numericGlyphHeight.Minimum = numericGlyphWidth.Minimum = numericGlyphLeft.Minimum = numericGlyphTop.Minimum = 0;
				return;
			}

			RectangleF clipRegion = _glyphClipper.ClipRegion;

			numericGlyphLeft.Maximum = (decimal)(_newGlyph.Texture.Settings.Width - clipRegion.Width);
			numericGlyphTop.Maximum = (decimal)(_newGlyph.Texture.Settings.Height - clipRegion.Height);

			numericGlyphWidth.Maximum = (decimal)(_newGlyph.Texture.Settings.Width - clipRegion.Left);
			numericGlyphHeight.Maximum = (decimal)(_newGlyph.Texture.Settings.Height - clipRegion.Top);
			numericGlyphWidth.Minimum = 1;
			numericGlyphHeight.Minimum = 1;
	    }

        /// <summary>
        /// Function called when the mouse wheel is moved.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="pointingDeviceEventArgs">The <see cref="PointingDeviceEventArgs"/> instance containing the event data.</param>
        private void ClipMouseWheel(object sender, PointingDeviceEventArgs pointingDeviceEventArgs)
        {
            if ((_content.CurrentState != DrawState.ClipGlyph)
                || (_glyphClipper == null)
                || (_zoomWindow == null))
            {
                return;
            }

	        if (ActiveControl != panelTextures)
	        {
		        panelTextures.Focus();
	        }

	        if (pointingDeviceEventArgs.WheelDelta < 0)
	        {
				if ((numericZoomAmount.Value - 0.5M) >= numericZoomAmount.Minimum)
		        {
			        numericZoomAmount.Value -= 0.5M;
		        }
	        }
            else if (pointingDeviceEventArgs.WheelDelta > 0)
            {
	            if (numericZoomAmount.Value + 0.5M <= numericZoomAmount.Maximum)
	            {
		            numericZoomAmount.Value += 0.5M;
	            }
            }
        }

        /// <summary>
        /// Function called when a mouse button is released.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="pointingDeviceEventArgs">The <see cref="PointingDeviceEventArgs"/> instance containing the event data.</param>
        private void ClipMouseUp(object sender, PointingDeviceEventArgs pointingDeviceEventArgs)
        {
            try
            {
                // Ensure focus on the texture display.
	            if (ActiveControl != panelTextures)
	            {
		            panelTextures.Focus();
	            }

	            if ((_newGlyph == null)
                    || (_glyphClipper == null))
                {
                    return;
                }

	            if (!_glyphClipper.OnMouseUp(pointingDeviceEventArgs))
	            {
		            return;
	            }

	            RectangleF clipRectangle = _glyphClipper.ClipRegion;

	            numericGlyphTop.Value = (decimal)clipRectangle.Top;
	            numericGlyphLeft.Value = (decimal)clipRectangle.Left;
	            numericGlyphWidth.Value = (decimal)clipRectangle.Width;
	            numericGlyphHeight.Value = (decimal)clipRectangle.Height;
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
            finally
            {
				EnableClipNumericLimits(true);
                UpdateGlyphInfo();
            }
        }

        /// <summary>
        /// Function called when a mouse button is pressed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="pointingDeviceEventArgs">The <see cref="PointingDeviceEventArgs"/> instance containing the event data.</param>
        private void ClipMouseDown(object sender, PointingDeviceEventArgs pointingDeviceEventArgs)
        {
            try
            {
                // Ensure focus on the texture display.
	            if (ActiveControl != panelTextures)
	            {
		            panelTextures.Focus();
	            }

	            if ((_newGlyph == null)
                    || (_glyphClipper == null))
                {
                    return;
                }

                if (!_glyphClipper.OnMouseDown(pointingDeviceEventArgs))
                {
                    return;
                }

				EnableClipNumericLimits(false);
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
            finally
            {
                UpdateGlyphInfo();
            }
        }

        /// <summary>
        /// Function to draw the magnification window.
        /// </summary>
        private void UpdateMagnifierWindow()
        {
	        Vector2 zoomPosition;
            _zoomWindow.Position = _mousePosition;

	        if (!checkZoomSnap.Checked)
	        {
		        zoomPosition = new Vector2(_mousePosition.X + 4,
		                                   _mousePosition.Y + 4);

		        if ((zoomPosition.X + _zoomWindow.ZoomWindowSize.X) > panelTextures.ClientSize.Width - 4)
		        {
			        zoomPosition.X = _mousePosition.X - _zoomWindow.ZoomWindowSize.X - 4;
		        }

		        if ((zoomPosition.Y + _zoomWindow.ZoomWindowSize.Y) > panelTextures.ClientSize.Height - 4)
		        {
			        zoomPosition.Y = _mousePosition.Y - _zoomWindow.ZoomWindowSize.Y - 4;
		        }
	        }
	        else
	        {
		        zoomPosition = new Vector2(panelTextures.ClientSize.Width - _zoomWindow.ZoomWindowSize.X, 0);

		        if ((_mousePosition.Y < _zoomWindow.ZoomWindowSize.Y) &&
		            (_mousePosition.X > panelTextures.ClientSize.Width * 0.5f))
		        {
			        zoomPosition.Y = panelTextures.ClientSize.Height - _zoomWindow.ZoomWindowSize.Y;
		        }

		        if ((_mousePosition.X >= zoomPosition.X) && (_mousePosition.Y > panelTextures.ClientSize.Height * 0.5f))
		        {
			        zoomPosition.X = 0;
		        }
	        }

	        _zoomWindow.ZoomWindowLocation = zoomPosition;
        }

		/// <summary>
		/// Function to move the clipper when in a drag operation.
		/// </summary>
		/// <param name="pointingDeviceEventArgs">Event parameters specifying movement.</param>
	    private void MoveClipperDrag(PointingDeviceEventArgs pointingDeviceEventArgs)
	    {
			if (_glyphClipper.OnMouseMove(pointingDeviceEventArgs))
			{
				RectangleF clipRectangle = _glyphClipper.ClipRegion;

				numericGlyphTop.Value = (decimal)clipRectangle.Top;
				numericGlyphLeft.Value = (decimal)clipRectangle.Left;
				numericGlyphWidth.Value = (decimal)clipRectangle.Width;
				numericGlyphHeight.Value = (decimal)clipRectangle.Height;
			}

			UpdateMagnifierWindow();
	    }

        /// <summary>
		/// Function called when the mouse is moved in clipping mode.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="pointingDeviceEventArgs">The <see cref="PointingDeviceEventArgs"/> instance containing the event data.</param>
	    private void ClipMouseMove(object sender, PointingDeviceEventArgs pointingDeviceEventArgs)
		{
	        if ((_content.CurrentState != DrawState.ClipGlyph)
				|| (_zoomWindow == null)
				|| (_glyphClipper == null))
	        {
		        return;
	        }

	        _mousePosition = pointingDeviceEventArgs.Position;

			MoveClipperDrag(pointingDeviceEventArgs);
		}

		/// <summary>
		/// Function to end the glyph clipping process.
		/// </summary>
	    private void EndGlyphClipping()
	    {
			panelTextures.MouseClick += GorgonFontContentPanel_MouseClick;
			panelTextures.MouseDoubleClick += panelTextures_MouseDoubleClick;
			panelTextures.MouseDown += panelTextures_MouseDown;
			panelTextures.MouseMove += panelTextures_MouseMove;

			if (_rawMouse != null)
			{
				_rawMouse.Acquired = false;
			}

            // Turn off any drag operation that is active.
		    if (_glyphClipper != null)
		    {
		        _glyphClipper.DragMode = ClipSelectionDragMode.None;
		    }

			_content.CurrentState = DrawState.GlyphEdit;

			panelTextures.Cursor = DefaultCursor;

            ValidateControls();

		    panelInnerDisplay.BorderStyle = BorderStyle.None;
	    }

		/// <summary>
		/// Handles the Click event of the buttonGlyphClipOK control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonGlyphClipOK_Click(object sender, EventArgs e)
		{
			try
			{
				Cursor.Current = Cursors.WaitCursor;

				if (_newGlyph == null)
				{
					return;
				}

				// If we have a custom advance for this glyph, then remove it.
				if (_content.Font.Settings.Advances.ContainsKey(_newGlyph.Character))
				{
					_content.Font.Settings.Advances.Remove(_newGlyph.Character);
				}

				_newGlyph = new GorgonGlyph(_newGlyph.Character,
				                            _newGlyph.Texture,
				                            Rectangle.Truncate(_glyphClipper.ClipRegion),
				                            _newGlyph.Offset,
											(int)_glyphClipper.ClipRegion.Width);

				// If we've customized this glyph before, remove it from the customization list.
				GorgonGlyph existingCustomGlyph =
					_content.Font.Settings.Glyphs.FirstOrDefault(item => item.Character == _newGlyph.Character);

				while (existingCustomGlyph != null)
				{
					if (existingCustomGlyph.Texture != _newGlyph.Texture)
					{
						RemoveGlyphTexture(existingCustomGlyph);
					}

					_content.Font.Settings.Glyphs.Remove(existingCustomGlyph);

					existingCustomGlyph = _content.Font.Settings.Glyphs.FirstOrDefault(item => item.Character == _newGlyph.Character);
				}

				_content.Font.Settings.Glyphs.Add(_newGlyph);
				_content.UpdateFontGlyphs();
                _content.DisableProperty("FontTextureSize", true);

				_selectedGlyph = _newGlyph;

				GetGlyphAdvancementAndOffset();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				_newGlyph = null;
				panelTextures.Focus();
                EndGlyphClipping();

				Cursor.Current = Cursors.Default;
			}
		}


		/// <summary>
		/// Handles the Click event of the buttonGlyphClipCancel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonGlyphClipCancel_Click(object sender, EventArgs e)
		{
			try
			{
				if (_newGlyph != null)
				{
					// Remove the texture if this is the only glyph using it.
					RemoveGlyphTexture(_newGlyph);
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				_newGlyph = null;
				panelTextures.Focus();
                EndGlyphClipping();
			}
		}

	    /// <summary>
		/// Handles the KeyUp event of the GorgonFontContentPanel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
		private void GorgonFontContentPanel_KeyUp(object sender, KeyboardEventArgs e)
		{
			Debug.Assert(ParentForm != null, "No parent form!");

			if ((ParentForm.ActiveControl != this) 
				|| (!panelTextures.Focused))
			{
				return;
			}

		    if (_content.CurrentState == DrawState.ClipGlyph)
		    {
				// Disable control validate when clipping and we're using the keyboard.
				// This avoids ugly flashing on the panel with the numeric controls.
			    switch (e.Key)
			    {
				    case KeyboardKeys.Up:
					case KeyboardKeys.Down:
					case KeyboardKeys.Left:
					case KeyboardKeys.Right:
					case KeyboardKeys.NumPad1:
					case KeyboardKeys.NumPad2:
					case KeyboardKeys.NumPad3:
					case KeyboardKeys.NumPad4:
					case KeyboardKeys.NumPad6:
					case KeyboardKeys.NumPad7:
					case KeyboardKeys.NumPad8:
					case KeyboardKeys.NumPad9:
					case KeyboardKeys.Space:
					    return;
			    }
		    }

		    try
			{
				switch (e.Key)
				{
					case KeyboardKeys.Left:
						buttonPrevTexture.PerformClick();
						break;
					case KeyboardKeys.Right:
						buttonNextTexture.PerformClick();
						break;
					case KeyboardKeys.Enter:
						if (_selectedGlyph == null)
						{
							return;
						}

						switch (_content.CurrentState)
						{
							case DrawState.DrawFontTextures:
								buttonEditGlyph.PerformClick();
								break;
							case DrawState.ClipGlyph:
								buttonGlyphClipOK.PerformClick();
								break;
						}
						
						break;
					case KeyboardKeys.F4:
						if (!e.Alt)
						{
							ParentForm.Close();
						}
						break;
					case KeyboardKeys.Escape:
						if (_selectedGlyph == null)
						{
							return;
						}

						switch (_content.CurrentState)
						{
                            case DrawState.KernPair:
                                buttonGlyphKern.PerformClick();
						        break;
							case DrawState.ClipGlyph:
								buttonGlyphClipCancel.PerformClick();
								break;
							case DrawState.GlyphEdit:
							case DrawState.ToGlyphEdit:
								buttonGoHome.PerformClick();
								break;
						}
						break;
				}

			}
			finally
			{
				ValidateControls();
			}
		}

		/// <summary>
		/// Function to unload an external glyph texture.
		/// </summary>
		/// <param name="glyph">Glyph that is attached to the texture.</param>
	    private void RemoveGlyphTexture(GorgonGlyph glyph)
	    {
		    if ((glyph == null)
				|| (glyph.Texture == null))
		    {
			    return;
		    }

			// If any other glyphs are attached to this texture, then leave.
		    if (_content.Font.Settings.Glyphs.Any(item => (glyph != item) && (item.Texture == glyph.Texture)))
		    {
			    return;
		    }

			Dependency dependency = _content.Dependencies.FirstOrDefault(item => _content.Dependencies.GetCachedDependencyObject<GorgonTexture2D>(item) == glyph.Texture);

		    if (dependency != null)
		    {
			    _content.Dependencies[dependency.EditorFile, dependency.Type] = null;
		    }

			glyph.Texture.Dispose();

            _content.DisableProperty("FontTextureSize", _content.Font.Settings.Glyphs.Count > 0);
	    }

        /// <summary>
		/// Handles the Click event of the menuItemRemoveGlyphImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void menuItemRemoveGlyphImage_Click(object sender, EventArgs e)
        {
            try
            {
                if (GorgonDialogs.ConfirmBox(ParentForm,
                                             string.Format(Resources.GORFNT_DLG_REMOVE_TEXTURE_PROMPT,
                                                           string.Format("{0} (0x{1})",
                                                                         _selectedGlyph.Character,
                                                                         ((ushort)_selectedGlyph.Character).FormatHex()),
                                                                         _selectedGlyph.Texture.Name))
                    == ConfirmationResult.No)
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                // Ensure that the selected glyph actually exists in the glyph overrides.
                GorgonGlyph currentGlyph = _content.Font.Settings.Glyphs.FirstOrDefault(item => item.Character == _selectedGlyph.Character);

                // Destroy the texture if it's not used by any other glyph.
                while (currentGlyph != null)
                {
					RemoveGlyphTexture(currentGlyph);

                    _content.Font.Settings.Glyphs.Remove(currentGlyph);

					currentGlyph = _content.Font.Settings.Glyphs.FirstOrDefault(item => item.Character == _selectedGlyph.Character);
                }

                _content.UpdateFontGlyphs();
                _content.DisableProperty("FontTextureSize", _content.Font.Settings.Glyphs.Count > 0);
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
            finally
            {
                ValidateControls();
                Cursor.Current = Cursors.Default;
            }
        }

		/// <summary>
		/// Function to load a texture.
		/// </summary>
		/// <param name="file">File containing the texture.</param>
		/// <param name="stream">Stream containing the texture data.</param>
	    private GorgonTexture2D LoadTexture(EditorFile file, Stream stream)
	    {
			GorgonTexture2D texture;

			using (var imageContent = _content.ImageEditor.ImportContent(file, stream))
			{
				// We can only use 2D content.
				if (imageContent.Image.Settings.ImageType != ImageType.Image2D)
				{
					GorgonDialogs.ErrorBox(ParentForm, string.Format(Resources.GORFNT_ERR_IMAGE_NOT_2D, imageContent.Name));
					return null;
				}

				var formatInfo = GorgonBufferFormatInfo.GetInfo(imageContent.Image.Settings.Format);

				// If the size is mismatched with the font textures then ask the user if they wish to resize the 
				// texture.
				if ((imageContent.Image.Settings.Width != _content.FontTextureSize.Width)
				    || (imageContent.Image.Settings.Height != _content.FontTextureSize.Height))
				{
					if (formatInfo.IsCompressed)
					{
						GorgonDialogs.ErrorBox(ParentForm,
						                       string.Format(Resources.GORFNT_CANNOT_RESIZE_BC_IMAGE,
						                                     _content.FontTextureSize.Width,
						                                     _content.FontTextureSize.Height,
						                                     imageContent.Image.Settings.Width,
						                                     imageContent.Image.Settings.Height));
						return null;
					}

					// If the image is larger than the font texture size, then ask if clipping is required.
					if ((imageContent.Image.Settings.Width > _content.FontTextureSize.Width)
					    || (imageContent.Image.Settings.Height > _content.FontTextureSize.Height))
					{
						ConfirmationResult result = GorgonDialogs.ConfirmBox(ParentForm,
						                                                     string.Format(
						                                                                   Resources.GORFNT_ERR_IMAGE_SIZE_MISMATCH,
						                                                                   imageContent.Image.Settings.Width,
						                                                                   imageContent.Image.Settings.Height,
						                                                                   _content.Font.Settings.TextureSize.Width,
						                                                                   _content.Font.Settings.TextureSize.Height),
						                                                     null);

						if (result == ConfirmationResult.No)
						{
							return null;
						}
					}

					// Resize or clip the image.
					imageContent.Image.Resize(_content.FontTextureSize.Width,
					                          _content.FontTextureSize.Height,
					                          true,
					                          ImageFilter.Point);
				}

				// Remove any array indices from the texture, we don't need them for font glyphs.
				var settings = (GorgonTexture2DSettings)imageContent.Image.Settings.Clone();
				settings.ArrayCount = 1;

				texture = ContentObject.Graphics.Textures.CreateTexture<GorgonTexture2D>(imageContent.EditorFile.FilePath, imageContent.Image, settings);

				// Attach the dependency to link the texture to this font.
				var dependency = new Dependency(imageContent.EditorFile, GorgonFontContent.GlyphTextureType);

				var converter = new SizeConverter();

				dependency.Properties[GorgonFontContent.GlyphTextureSizeProp] =
					new DependencyProperty(GorgonFontContent.GlyphTextureSizeProp, converter.ConvertToInvariantString(settings.Size));

				_content.Dependencies[dependency.EditorFile, GorgonFontContent.GlyphTextureType] = dependency;
				_content.Dependencies.CacheDependencyObject(dependency.EditorFile, GorgonFontContent.GlyphTextureType, texture);
			}

			return texture;
	    }

		/// <summary>
		/// Handles the Click event of the menuItemSetGlyph control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void menuItemSetGlyph_Click(object sender, EventArgs e)
		{
			if (_content.CurrentState != DrawState.GlyphEdit)
			{
				return;
			}

			if (_newGlyph == null)
			{
				_newGlyph = new GorgonGlyph(_selectedGlyph.Character,
				                            _selectedGlyph.Texture,
				                            _selectedGlyph.GlyphCoordinates,
				                            _selectedGlyph.Offset,
				                            _selectedGlyph.Advance);
			}

			_content.CurrentState = DrawState.ClipGlyph;

			// Turn on scrollbars if necessary.
			CheckForScroll(_newGlyph.Texture.Settings.Size);

			// Disable previous events.
			panelTextures.MouseClick -= GorgonFontContentPanel_MouseClick;
			panelTextures.MouseDoubleClick -= panelTextures_MouseDoubleClick;
			panelTextures.MouseDown -= panelTextures_MouseDown;
			panelTextures.MouseMove -= panelTextures_MouseMove;

			var cursorPosition = new Point(_newGlyph.GlyphCoordinates.Right, _newGlyph.GlyphCoordinates.Bottom);

			_rawMouse.Acquired = true;
			_mousePosition = _rawMouse.Position = cursorPosition;
			Cursor.Position = panelTextures.PointToScreen(cursorPosition);

			_glyphClipper.ClipRegion = _newGlyph.GlyphCoordinates;
			_glyphClipper.TextureSize = _newGlyph.Texture.Settings.Size;

			_glyphClipper.Offset = Vector2.Zero;
			_glyphClipper.SelectorPattern = _pattern;

			_zoomWindow = new ZoomWindow(_content.Renderer, _newGlyph.Texture)
			              {
				              Clipper = _glyphClipper,
							  ZoomAmount = GorgonFontEditorPlugIn.Settings.ZoomWindowScaleFactor,
							  ZoomWindowSize = new Vector2(GorgonFontEditorPlugIn.Settings.ZoomWindowSize, GorgonFontEditorPlugIn.Settings.ZoomWindowSize),
                              BackgroundTexture = _pattern,
                              Position = cursorPosition
			              };
            
			if (_zoomFont == null)
			{
				_zoomFont = ContentObject.Graphics.Fonts.CreateFont("MagnifierCaptionFont",
				                                               new GorgonFontSettings
				                                               {
					                                               AntiAliasingMode = FontAntiAliasMode.AntiAlias,
					                                               Characters = _zoomWindow.ZoomWindowText + ":.01234567890x ",
					                                               FontFamilyName = "Segoe UI",
					                                               FontHeightMode = FontHeightMode.Points,
					                                               Size = 10.0f,
					                                               TextureSize = new Size(64, 64)
				                                               });
			}

			_zoomWindow.ZoomWindowFont = _zoomFont;

		    EnableClipNumericLimits(false);

			numericGlyphTop.Value = _newGlyph.GlyphCoordinates.Top;
			numericGlyphLeft.Value = _newGlyph.GlyphCoordinates.Left;
			numericGlyphWidth.Value = _newGlyph.GlyphCoordinates.Width;
			numericGlyphHeight.Value = _newGlyph.GlyphCoordinates.Height;
			numericZoomWindowSize.Value = GorgonFontEditorPlugIn.Settings.ZoomWindowSize;
			numericZoomAmount.Value = (decimal)GorgonFontEditorPlugIn.Settings.ZoomWindowScaleFactor;
			checkZoomSnap.Checked = GorgonFontEditorPlugIn.Settings.ZoomWindowSnap;

			EnableClipNumericLimits(true);

            UpdateMagnifierWindow();

			ValidateControls();
            panelInnerDisplay.BorderStyle = BorderStyle.FixedSingle;
		}

        /// <summary>
		/// Handles the Click event of the menuItemLoadGlyphImage control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void menuItemLoadGlyphImage_Click(object sender, EventArgs e)
		{
		    try
		    {
			    _newGlyph = null;

			    if (!string.IsNullOrWhiteSpace(GorgonFontEditorPlugIn.Settings.LastTextureImportPath))
			    {
				    imageFileBrowser.StartDirectory = GorgonFontEditorPlugIn.Settings.LastTextureImportPath;
			    }

		        imageFileBrowser.FileTypes.Clear();
				imageFileBrowser.FileTypes.Add(_content.ImageEditor.ContentType);

		        imageFileBrowser.FileView = GorgonFontEditorPlugIn.Settings.LastTextureImportDialogView;

			    if (imageFileBrowser.ShowDialog(ParentForm) == DialogResult.OK)
			    {
				    GorgonTexture2D texture = null;
				    EditorFile file = imageFileBrowser.Files[0];
				    Dependency dependency = _content.Dependencies.FirstOrDefault(item =>
				                                                                 _content.Dependencies.GetCachedDependencyObject<GorgonTexture2D>(item) == _selectedGlyph.Texture 
																				 && string.Equals(item.EditorFile.FilePath,
				                                                                               file.FilePath,
				                                                                               StringComparison.OrdinalIgnoreCase));

				    // This texture is already assigned to the glyph, skip the load procedure.
				    if (dependency != null)
				    {
					    menuItemSetGlyph.Enabled = true;
						menuItemSetGlyph.PerformClick();
					    return;
				    }

					// Get the texture if it's already loaded in the cache.
				    _content.Dependencies.TryGetValue(file, GorgonFontContent.GlyphTextureType, out dependency);
				    if (dependency != null)
				    {
					    texture = _content.Dependencies.GetCachedDependencyObject<GorgonTexture2D>(dependency);
				    }

					// We have no pre-loaded texture, so go get it.
				    if (texture == null)
				    {
					    using (Stream stream = imageFileBrowser.OpenFile())
					    {
						    texture = LoadTexture(file, stream);
					    }
				    }

					// If we don't load a texture, then leave.
				    if (texture == null)
				    {
					    return;
				    }

					var textureRegion = new Rectangle(0, 0, texture.Settings.Width, texture.Settings.Height);
				    var glyphRegion = _selectedGlyph.GlyphCoordinates;

					// Ensure the glyph fits on the loaded texture.
				    if (!textureRegion.Contains(glyphRegion))
				    {
					    glyphRegion.X = 0;
					    glyphRegion.Y = 0;

					    if (glyphRegion.Right > textureRegion.Right)
					    {
						    glyphRegion.Width = textureRegion.Width;
					    }

					    if (glyphRegion.Bottom > textureRegion.Bottom)
					    {
						    glyphRegion.Height = textureRegion.Height;
					    }
				    }

					_newGlyph = new GorgonGlyph(_selectedGlyph.Character,
												texture,
												glyphRegion,
												_selectedGlyph.Offset,
												_selectedGlyph.Advance);

					GorgonFontEditorPlugIn.Settings.LastTextureImportPath = Path.GetDirectoryName(file.FilePath).FormatDirectory('/');

					// Jump to the glyph clipper.
				    menuItemSetGlyph.Enabled = true;
					menuItemSetGlyph.PerformClick();
			    }

			    GorgonFontEditorPlugIn.Settings.LastTextureImportDialogView = imageFileBrowser.FileView;
		    }
		    catch (Exception ex)
		    {
		        GorgonDialogs.ErrorBox(ParentForm, ex);
		    }
		    finally
		    {
		        ValidateControls();
		    }
		}

        /// <summary>
        /// Function to check the text preview scroll state.
        /// </summary>
        private void CheckTextPreviewScroll()
        {
            if ((_text == null)
				|| (_content == null)
				|| (_content.Font == null))
            {
                return;
            }

            if (_text.Size.Y <= panelText.ClientSize.Height)
            {
                scrollTextVertical.Value = 0;
                panelTextVertScroll.Visible = false;
                return;
            }
            
            panelTextVertScroll.Visible = true;
            scrollTextVertical.Maximum =(int)((_text.Size.Y - panelText.ClientSize.Height) + (scrollTextVertical.LargeChange)).Max(0);
            scrollTextVertical.LargeChange = (int)_content.Font.LineHeight.Max(1);
            scrollTextVertical.SmallChange = (int)(_content.Font.LineHeight / 4.0f).Max(1);
        }

		/// <summary>
		/// Handles the TextChanged event of the textPreviewText control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void textPreviewText_TextChanged(object sender, EventArgs e)
		{
			string formattedText = textPreviewText.Text;

			if (_text == null)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(textPreviewText.Text))
			{
				textPreviewText.Text = Resources.GORFNT_DEFAULT_PREVIEW_TEXT;
			    return;
			}

			GorgonFontEditorPlugIn.Settings.SampleText = formattedText;

			_text.Text = formattedText;

		    CheckTextPreviewScroll();
		}
		
		/// <summary>
		/// Function to populate the search character autocomplete list.
		/// </summary>
	    private void PopulateSearchCharacterList()
		{
			comboSearchGlyph.Items.Clear();

			if ((_content == null)
			    || (_content.Font == null))
			{
				return;
			}

			comboSearchGlyph.Items.AddRange(_content.Characters.Where(item => !char.IsWhiteSpace(item))
			                                        .Select(item => (object)item.ToString(CultureInfo.CurrentUICulture))
			                                        .ToArray());
	    }

		/// <summary>
		/// Handles the MouseClick event of the GorgonFontContentPanel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void GorgonFontContentPanel_MouseClick(object sender, MouseEventArgs e)
		{
			Focus();
		}

		/// <summary>
		/// Handles the MouseWheel event of the PanelDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void PanelDisplay_MouseWheel(object sender, MouseEventArgs e)
		{
			if ((_content == null)
				|| (_content.CurrentState != DrawState.DrawFontTextures))
			{
				return;
			}

		    if ((e.Delta > 0)
		        && (buttonNextTexture.Enabled))
		    {
		        buttonNextTexture.PerformClick();
		    }

		    if ((e.Delta < 0)
		        && (buttonPrevTexture.Enabled))
		    {
		        buttonPrevTexture.PerformClick();
		    }
		}

		/// <summary>
		/// Handles the Click event of the itemShadowOffset control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemShadowOffset_Click(object sender, EventArgs e)
		{
			ValueComponentDialog valueEditor = null;

			try
			{
				valueEditor = new ValueComponentDialog
				                {
					                ValueComponents = 2,
					                DecimalPlaces = 0,
					                MaxValue = 8,
					                MinValue = -8,
					                Value1 = GorgonFontEditorPlugIn.Settings.ShadowOffset.X,
					                Value2 = GorgonFontEditorPlugIn.Settings.ShadowOffset.Y,
					                Text = Resources.GORFNT_DLG_SHADOW_OFFSET_CAPTION
				                };
				valueEditor.ValueChanged += componentEdit_ValueChanged;

				if (valueEditor.ShowDialog() == DialogResult.OK)
				{
					_text.ShadowOffset = GorgonFontEditorPlugIn.Settings.ShadowOffset = new Point((int)valueEditor.Value1, (int)valueEditor.Value2);
				}
				else
				{
					_text.ShadowOffset = GorgonFontEditorPlugIn.Settings.ShadowOffset;
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				if (valueEditor != null)
				{
					valueEditor.ValueChanged -= componentEdit_ValueChanged;
					valueEditor.Dispose();
				}
			}
		}

		/// <summary>
		/// Handles the ValueChanged event of the componentEdit control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void componentEdit_ValueChanged(object sender, EventArgs e)
		{
			var dialog = (ValueComponentDialog)sender;
						
			_text.ShadowOffset = new Point((int)dialog.Value1, (int)dialog.Value2);
			DrawText();
			_content.Renderer.Render();
		}

		/// <summary>
		/// Handles the Click event of the itemShadowOpacity control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemShadowOpacity_Click(object sender, EventArgs e)
		{
			AlphaChannelDialog picker = null;

			try
			{
				picker = new AlphaChannelDialog
				         {
					         Text = Resources.GORFNT_DLG_SHADOW_ALPHA_CAPTION,
					         SelectedAlphaValue = GorgonFontEditorPlugIn.Settings.ShadowOpacity
				         };
				if (picker.ShowDialog() == DialogResult.OK)
				{
					GorgonFontEditorPlugIn.Settings.ShadowOpacity = _text.ShadowOpacity = picker.SelectedAlphaValue;
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
			}
			finally
			{
				if (picker != null)
					picker.Dispose();
			}
		}

		/// <summary>
		/// Handles the Click event of the itemPreviewShadowEnable control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemPreviewShadowEnable_Click(object sender, EventArgs e)
		{
			if (_text == null)
			{
				return;
			}

			_text.ShadowEnabled = GorgonFontEditorPlugIn.Settings.ShadowEnabled = itemPreviewShadowEnable.Checked;
			ValidateControls();
		}

		/// <summary>
		/// Handles the Click event of the itemSampleTextBackground control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemSampleTextBackground_Click(object sender, EventArgs e)
		{
			ColorPickerDialog picker = null;

			try
			{
				picker = new ColorPickerDialog
				         {
					         Text = Resources.GORFNT_DLG_BACKGROUND_COLOR_CAPTION,
					         AlphaEnabled = false,
					         OldColor = Color.FromArgb(GorgonFontEditorPlugIn.Settings.BackgroundColor)
				         };

				if (picker.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				panelText.BackColor = picker.SelectedColor;
				GorgonFontEditorPlugIn.Settings.BackgroundColor = picker.SelectedColor.ToArgb();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
			}
			finally
			{
				if (picker != null)
				{
					picker.Dispose();
				}
				ValidateControls();
			}	
		}

		/// <summary>
		/// Handles the Click event of the buttonTextColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonTextColor_Click(object sender, EventArgs e)
		{
			ColorPickerDialog picker = null;

			try
			{
				picker = new ColorPickerDialog
				         {
					         Text = Resources.GORFNT_DLG_FOREGROUND_COLOR_CAPTION,
					         OldColor = Color.FromArgb(GorgonFontEditorPlugIn.Settings.TextColor)
				         };

				if (picker.ShowDialog() == DialogResult.OK)
				{
					GorgonFontEditorPlugIn.Settings.TextColor = picker.SelectedColor.ToArgb();
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
			}
			finally
			{
				if (picker != null)
					picker.Dispose();
			}
		}

		/// <summary>
		/// Function to perform validation of the form controls.
		/// </summary>
		private void ValidateControls()
		{
            if ((_content != null) && (_content.Font != null))
            {
	            buttonSave.Enabled = buttonRevert.Enabled = _content.HasChanges;

				// If we can't use the image editor plug-in, then don't allow us to edit the glyph image dimensions.
				if (_currentTextureIndex < 0)
				{
					_currentTextureIndex = 0;
				}

				if (_currentTextureIndex >= _textures.Length)
				{
					_currentTextureIndex = _textures.Length - 1;
				}

	            labelFindGlyph.Enabled = comboSearchGlyph.Enabled = _content.CurrentState == DrawState.DrawFontTextures;
	            buttonSearchGlyph.Enabled = comboSearchGlyph.Text.Length > 0 && _content.CurrentState == DrawState.DrawFontTextures;

	            sepGlyphEditing.Visible = buttonSearchGlyph.Visible =
	                                      labelFindGlyph.Visible =
	                                      comboSearchGlyph.Visible = _content.CurrentState == DrawState.DrawFontTextures
	                                                                 || _content.CurrentState == DrawState.NextTexture ||
	                                                                 _content.CurrentState == DrawState.PrevTexture
	                                                                 || _content.CurrentState == DrawState.FromGlyphEdit;

	            buttonPrevTexture.Enabled = _currentTextureIndex > 0 &&
	                                        ((_content.CurrentState == DrawState.DrawFontTextures) ||
	                                         _content.CurrentState == DrawState.NextTexture ||
	                                         _content.CurrentState == DrawState.PrevTexture);
				buttonNextTexture.Enabled = _currentTextureIndex < _textures.Length - 1 && ((_content.CurrentState == DrawState.DrawFontTextures) ||
											 _content.CurrentState == DrawState.NextTexture ||
											 _content.CurrentState == DrawState.PrevTexture);

				dropDownZoom.Enabled = (_content.CurrentState == DrawState.DrawFontTextures);

				labelTextureCount.Text = string.Format("{0}: {1}/{2}", Resources.GORFNT_TEXT_TEXTURE, _currentTextureIndex + 1, _textures.Length);

				panelToolbar.Visible = _content.CurrentState != DrawState.ClipGlyph;

	            panelGlyphEdit.Visible = _content.CurrentState == DrawState.ClipGlyph
	                                     || _content.CurrentState == DrawState.GlyphEdit
										 || _content.CurrentState == DrawState.ToGlyphEdit
										 || _content.CurrentState == DrawState.KernPair;

	            panelGlyphAdvance.Visible = _content.CurrentState == DrawState.GlyphEdit
	                                        || _content.CurrentState == DrawState.ToGlyphEdit;

	            numericOffsetX.Enabled =
		            numericOffsetY.Enabled =
		            numericGlyphAdvance.Enabled = _content.CurrentState == DrawState.GlyphEdit
		                                                                && _selectedGlyph != null;

	            panelGlyphClip.Visible = _content.CurrentState == DrawState.ClipGlyph;
                panelKerningPairs.Visible = _content.CurrentState == DrawState.KernPair;

                if ((panelKerningPairs.Visible)
                    && (comboSecondGlyph.SelectedItem != null)
                    && (comboSecondGlyph.Items.Count > 0))
                {
                    GorgonKerningPair kernValue = (KernPairComboItem)comboSecondGlyph.SelectedItem;
                    int kernAmount;

                    // If we don't have a kerning value applied, then allow us to add it.
                    if (!_content.Font.Settings.KerningPairs.TryGetValue(kernValue, out kernAmount))
                    {
                        _content.Font.KerningPairs.TryGetValue(kernValue, out kernAmount);
                    }

                    // Check current kerning values.
                    buttonKernOK.Enabled = kernAmount != numericKerningOffset.Value;
                    buttonKernCancel.Enabled = true;
                }
                else
                {
                    buttonKernOK.Enabled = false;
                    buttonKernCancel.Enabled = false;
                }

                buttonGoHome.Enabled = buttonEditGlyph.Enabled = _selectedGlyph != null;

				buttonGlyphKern.Enabled = _content.UseKerningPairs && (_content.CurrentState == DrawState.GlyphEdit || _content.CurrentState == DrawState.KernPair);
	            buttonGlyphTools.Enabled = _content.CurrentState == DrawState.GlyphEdit;

	            sepGlyphSpacing.Visible =
					buttonGlyphTools.Visible = (_content.CurrentState == DrawState.GlyphEdit || _content.CurrentState == DrawState.ToGlyphEdit || _content.CurrentState == DrawState.KernPair);
	            buttonGlyphKern.Visible = (_content.CurrentState == DrawState.GlyphEdit ||
	                                       _content.CurrentState == DrawState.ToGlyphEdit ||
	                                       _content.CurrentState == DrawState.KernPair);

	            buttonGlyphKern.Checked = _content.CurrentState == DrawState.KernPair && buttonGlyphKern.Enabled &&
	                                      buttonGlyphKern.Visible;

				// Disable the reset buttons when we don't have a change, and also when we're using custom glyph textures.
				// This is done because the custom glyph does not have a source value to use when determining offset/advancement.
	            buttonResetGlyphAdvance.Enabled = numericGlyphAdvance.Enabled && _selectedGlyph != null &&
	                                              _content.Font.Settings.Advances.ContainsKey(_selectedGlyph.Character)
												  && !_content.Font.Settings.Glyphs.Contains(_selectedGlyph);

				buttonResetGlyphOffset.Enabled = numericGlyphAdvance.Enabled && _selectedGlyph != null &&
												  _content.Font.Settings.Offsets.ContainsKey(_selectedGlyph.Character)
												  && !_content.Font.Settings.Glyphs.Contains(_selectedGlyph);


	            if (buttonGlyphTools.Visible)
	            {
		            menuItemLoadGlyphImage.Visible =
			            menuItemSetGlyph.Visible =
			            menuItemRemoveGlyphImage.Visible = buttonGlyphTools.Visible = _content.ImageEditor != null;
		            menuItemSetGlyph.Enabled = menuItemRemoveGlyphImage.Enabled = (_selectedGlyph != null) &&
		                                                                          (_selectedGlyph.IsExternalTexture) &&
		                                                                          (_content.CurrentState ==
		                                                                           DrawState.GlyphEdit);
	            }

	            if ((_content.CurrentState == DrawState.ToGlyphEdit)
                    || (_content.CurrentState == DrawState.GlyphEdit)
                    || (_content.CurrentState == DrawState.ClipGlyph)
					|| (_content.CurrentState == DrawState.KernPair))
	            {
	                buttonGoHome.Enabled = true;
	                buttonGoHome.Visible = true;
	                buttonEditGlyph.Visible = false;

                    // Disable the edit button until we're done clipping.
                    if (_content.CurrentState == DrawState.ClipGlyph)
                    {
                        buttonGoHome.Enabled = false;
                    }
                }
                else
	            {
	                buttonGoHome.Visible = false;
	                buttonEditGlyph.Visible = true;
                }

				UpdateGlyphInfo();
            }
            else
            {
				buttonPrevTexture.Enabled = false;
				buttonNextTexture.Enabled = false;
                labelTextureCount.Text = string.Format("{0}: 0/0", Resources.GORFNT_TEXT_TEXTURE);
            }

			itemShadowOffset.Enabled = itemShadowOpacity.Enabled = GorgonFontEditorPlugIn.Settings.ShadowEnabled;
		}

        /// <summary>
        /// Function to update the glyph information.
        /// </summary>
        private void UpdateGlyphInfo()
        {
	        if (_selectedGlyph != null) 
            {
                switch (_content.CurrentState)
                {
                    case DrawState.ClipGlyph:
                        if (_rawMouse == null)
                        {
                            labelSelectedGlyphInfo.Text = string.Empty;
                            break;
                        }

                        labelSelectedGlyphInfo.Text =
                            string.Format("{0}: {1}x{2}  {3}: {4} (U+{5}), {6}: {7}x{8}-{9}x{10} ({11}: {12}, {13})",
                                          Resources.GORFNT_TEXT_CURSOR_POSITION,
                                          _mousePosition.X,
                                          _mousePosition.Y,
                                          Resources.GORFNT_TEXT_SELECTED_GLYPH,
                                          _selectedGlyph.Character,
                                          ((ushort)_selectedGlyph.Character).FormatHex(),
                                          Resources.GORFNT_TEXT_REGION,
                                          _glyphClipper.ClipRegion.Left,
										  _glyphClipper.ClipRegion.Top,
										  _glyphClipper.ClipRegion.Right,
										  _glyphClipper.ClipRegion.Bottom,
                                          Resources.GORFNT_TEXT_SIZE,
										  _glyphClipper.ClipRegion.Width,
										  _glyphClipper.ClipRegion.Height);
                        break;
                    case DrawState.DrawFontTextures:
		                labelSelectedGlyphInfo.Text = string.Format("{0}: {1} (U+{2})",
																    Resources.GORFNT_TEXT_SELECTED_GLYPH,
		                                                            _selectedGlyph.Character,
		                                                            ((ushort)_selectedGlyph.Character).FormatHex())
		                                                    .Replace("&", "&&");
                        break;
                    default:
					    labelSelectedGlyphInfo.Text = string.Format("{0}: {1} (U+{2}) {3}: {4}, {5} {6}: {7}, {8} {9}: {10}",
						    Resources.GORFNT_TEXT_SELECTED_GLYPH,
						    _selectedGlyph.Character,
						    ((ushort)_selectedGlyph.Character).FormatHex(),
						    Resources.GORFNT_TEXT_LOCATION,
						    _selectedGlyph.GlyphCoordinates.X,
						    _selectedGlyph.GlyphCoordinates.Y,
						    Resources.GORFNT_TEXT_SIZE,
						    _selectedGlyph.GlyphCoordinates.Width,
						    _selectedGlyph.GlyphCoordinates.Height,
						    Resources.GORFNT_TEXT_ADVANCEMENT,
						    _selectedGlyph.Advance).Replace("&", "&&");
		                _hoverGlyph = null;
                        break;
                }
            }
            else
            {
                labelSelectedGlyphInfo.Text = string.Empty;
            }

            if (_hoverGlyph != null)
            {
                if (_selectedGlyph != null)
                {
                    separatorGlyphInfo.Visible = true;
                }

                labelHoverGlyphInfo.Text = string.Format("{0}: {1} (U+{2}) {3}: {4}, {5} {6}: {7},{8} {9}: {10}",
					Resources.GORFNT_TEXT_GLYPH,
                    _hoverGlyph.Character,
                    ((ushort)_hoverGlyph.Character).FormatHex(),
					Resources.GORFNT_TEXT_LOCATION,
                    _hoverGlyph.GlyphCoordinates.X,
                    _hoverGlyph.GlyphCoordinates.Y,
					Resources.GORFNT_TEXT_SIZE,
                    _hoverGlyph.GlyphCoordinates.Width,
                    _hoverGlyph.GlyphCoordinates.Height,
					Resources.GORFNT_TEXT_ADVANCEMENT,
                    _hoverGlyph.Advance).Replace("&", "&&");
            }
            else
            {
                labelHoverGlyphInfo.Text = string.Empty;
                separatorGlyphInfo.Visible = false;
            }
        }

		/// <summary>
		/// Handles the Resize event of the panelText control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void panelText_Resize(object sender, EventArgs e)
		{
		    if (_text == null)
		    {
		        return;
		    }

		    _text.TextRectangle = new RectangleF(PointF.Empty, panelText.ClientSize);
            CheckTextPreviewScroll();
		}

		/// <summary>
		/// Handles the ButtonClick event of the buttonGlyphTools control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonGlyphTools_ButtonClick(object sender, EventArgs e)
		{
			try
			{
				if ((_content == null)
				    || (_content.CurrentState != DrawState.GlyphEdit)
				    || (_content.ImageEditor == null))
				{
					return;
				}

				if (menuItemSetGlyph.Enabled)
				{
					menuItemSetGlyph.PerformClick();
					return;
				}

				menuItemLoadGlyphImage.PerformClick();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
			}
		}

        /// <summary>
		/// Handles the Click event of the buttonNextTexture control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonNextTexture_Click(object sender, EventArgs e)
		{
		    try
		    {
		        _sortedTextures = null;
		        _currentTextureIndex++;
			    if (GorgonFontEditorPlugIn.Settings.ShowAnimations)
			    {
				    _content.CurrentState = DrawState.NextTexture;
				    _nextState = DrawState.DrawFontTextures;
			    }
			    ActiveControl = panelTextures;
		        UpdateGlyphRegions();
		    }
		    catch (Exception ex)
		    {
		        GorgonDialogs.ErrorBox(ParentForm, ex);
		    }
		    finally
		    {
		        ValidateControls();
		    }
		}

		/// <summary>
		/// Handles the Click event of the buttonPrevTexture control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonPrevTexture_Click(object sender, EventArgs e)
		{
		    try
		    {
		        _sortedTextures = null;
		        _currentTextureIndex--;
			    if (GorgonFontEditorPlugIn.Settings.ShowAnimations)
			    {
				    _content.CurrentState = DrawState.PrevTexture;
				    _nextState = DrawState.DrawFontTextures;
			    }
			    ActiveControl = panelTextures;
		        UpdateGlyphRegions();
		    }
		    catch (Exception ex)
		    {
		        GorgonDialogs.ErrorBox(ParentForm, ex);
		    }
		    finally
		    {
		        ValidateControls();
		        
		    }
		}

		/// <summary>
		/// Handles the Click event of the zoomItem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void zoomItem_Click(object sender, EventArgs e)
		{
			var selectedItem = (ToolStripMenuItem)sender;

			try
			{
				var items = dropDownZoom.DropDownItems.Cast<ToolStripItem>().Where(item => item is ToolStripMenuItem);

			    // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
				foreach (ToolStripMenuItem control in items)
				{
					if (control != sender)
					{
						control.Checked = false;
					}
				}

				_currentZoom = float.Parse(selectedItem.Tag.ToString(),
				                           NumberStyles.Float,
				                           NumberFormatInfo.InvariantInfo);

				dropDownZoom.Text = string.Format("{0}: {1}", Resources.GORFNT_TEXT_ZOOM, selectedItem.Text);

				if (menuItemToWindow.Checked)
				{
					CalculateZoomToWindow();
				}

				UpdateGlyphRegions();

                _sortedTextures = null;
				
				switch (_content.CurrentState)
				{
					case DrawState.DrawFontTextures:
                        // Do nothing for this mode.
						break;
					default:
						UpdateGlyphEditor();
						_nextState = DrawState.FromGlyphEdit;
						break;
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
			}
		}

        /// <summary>
        /// Handles the MouseDown event of the panelTextures control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void panelTextures_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
	            switch (_content.CurrentState)
	            {
					case DrawState.NextTexture:
					case DrawState.PrevTexture:
					case DrawState.FromGlyphEdit:
					case DrawState.ToGlyphEdit:
					case DrawState.ClipGlyph:
	                    break;
                    case DrawState.KernPair:
	                    if (ActiveControl != panelTextures)
	                    {
	                        panelTextures.Focus();
	                    }
			            break;
					case DrawState.GlyphEdit:
			            if (!_textureRegion.Contains(e.Location))
			            {
				            buttonEditGlyph.PerformClick();
			            }
			            else
			            {
				            if (ActiveControl != this)
				            {
					            panelTextures.Focus();
				            }
			            }
	                    break;
		            default:
						// If we're inside the texture, find the glyph that we're hovering over.
						_selectedGlyph = null;
						if (_textureRegion.Contains(e.Location))
						{
							foreach (var glyph in _glyphRegions)
							{
								var rect = glyph.Value;
								rect.X += _textureOffset.X;
								rect.Y += _textureOffset.Y;

								if (!rect.Contains(e.Location))
								{
									continue;
								}

								_selectedGlyph = glyph.Key;
								break;
							}
						}

			            break;
	            }
            }
            finally
            {
                ValidateControls();
            }
        }

		/// <summary>
		/// Handles the MouseMove event of the panelTextures control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelTextures_MouseMove(object sender, MouseEventArgs e)
		{
            // If we're inside the texture, find the glyph that we're hovering over.
			switch (_content.CurrentState)
			{
				case DrawState.FromGlyphEdit:
				case DrawState.ToGlyphEdit:
				case DrawState.GlyphEdit:
				case DrawState.ClipGlyph:
                case DrawState.KernPair:
					break;
				default:
					_hoverGlyph = null;

					if (_textureRegion.Contains(e.Location))
					{
						foreach (var glyph in _glyphRegions)
						{
							var rect = glyph.Value;
							rect.X += _textureOffset.X;
							rect.Y += _textureOffset.Y;

							if (!rect.Contains(e.Location))
							{
								continue;
							}

							_hoverGlyph = glyph.Key;
							break;
						}
					}

					UpdateGlyphInfo();
					break;
			}
		}

        /// <summary>
        /// Handles the Scroll event of the clipper control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ClipperOffsetScroll(object sender, EventArgs e)
        {
            if ((_content == null)
                || (_content.CurrentState != DrawState.ClipGlyph))
            {
                return;
            }

            _glyphClipper.Offset = new Vector2(-scrollHorizontal.Value, -scrollVertical.Value);
        }

		/// <summary>
		/// Handles the ValueChanged event of the numericZoomWindowSize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericZoomWindowSize_ValueChanged(object sender, EventArgs e)
		{
			if ((_content == null) 
				|| (_content.CurrentState != DrawState.ClipGlyph)
				|| (_zoomWindow == null))
			{
				return;
			}

			_zoomWindow.ZoomWindowSize = new Vector2((float)numericZoomWindowSize.Value, (float)numericZoomWindowSize.Value);
			UpdateMagnifierWindow();
			
			GorgonFontEditorPlugIn.Settings.ZoomWindowSize = (int)numericZoomWindowSize.Value;
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericZoomAmount control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericZoomAmount_ValueChanged(object sender, EventArgs e)
		{
			if ((_content == null) 
				|| (_content.CurrentState != DrawState.ClipGlyph)
				|| (_zoomWindow == null))
			{
				return;
			}

			_zoomWindow.ZoomAmount = (float)numericZoomAmount.Value;
			UpdateMagnifierWindow();

			GorgonFontEditorPlugIn.Settings.ZoomWindowScaleFactor = _zoomWindow.ZoomAmount;
		}

		/// <summary>
		/// Handles the Leave event of the numericGlyphLeft control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericGlyphLeft_Leave(object sender, EventArgs e)
		{
			if ((_content == null) 
				|| (_content.CurrentState != DrawState.ClipGlyph)
				|| (_glyphClipper == null)
				|| (_glyphClipper.DragMode != ClipSelectionDragMode.None)
                || (_rawKeyboard == null))
			{
				return;
			}

			_rawKeyboard.KeyDown += GorgonFontContentPanel_KeyDown;
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericKerningOffset control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericKerningOffset_ValueChanged(object sender, EventArgs e)
		{
			ValidateControls();
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericGlyphAdvance control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericGlyphAdvance_ValueChanged(object sender, EventArgs e)
		{
			if ((_content == null)
				|| (_content.CurrentState != DrawState.GlyphEdit))
			{
				return;
			}

			try
			{
			    _selectedGlyph.Advance = (int)numericGlyphAdvance.Value;
				_content.Font.Settings.Advances[_selectedGlyph.Character] = _selectedGlyph.Advance;

				_content.UpdateFontGlyphAdvance(_selectedGlyph.Advance, false);

				_text.Refresh();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericOffsetX control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericOffsetX_ValueChanged(object sender, EventArgs e)
		{
			if ((_content == null)
			    || (_content.CurrentState != DrawState.GlyphEdit))
			{
				return;
			}

			try
			{
				_selectedGlyph.Offset = new Point((int)numericOffsetX.Value, (int)numericOffsetY.Value);
				_content.Font.Settings.Offsets[_selectedGlyph.Character] = _selectedGlyph.Offset;

				_content.UpdateFontGlyphOffset(_selectedGlyph.Offset, false);

				_text.Refresh();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Enter event of the numericGlyphLeft control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericGlyphLeft_Enter(object sender, EventArgs e)
		{
			if ((_content == null) 
				|| (_content.CurrentState != DrawState.ClipGlyph)
				|| (_glyphClipper == null)
				|| (_glyphClipper.DragMode != ClipSelectionDragMode.None)
                || (_rawKeyboard == null))
			{
				return;
			}

			_rawKeyboard.KeyDown -= GorgonFontContentPanel_KeyDown;

			// Snap the cursor when we enter one of the numeric areas.
			if ((sender == numericGlyphLeft)
				|| (sender == numericGlyphTop))
			{
				_mousePosition = _glyphClipper.ClipRegion.Location;
				UpdateMagnifierWindow();
			}
			else
			{
				RectangleF clipRectangle = _glyphClipper.ClipRegion;

				_mousePosition = new Vector2(clipRectangle.Right, clipRectangle.Bottom);
				UpdateMagnifierWindow();
			}
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericGlyphLeft control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericGlyphLeft_ValueChanged(object sender, EventArgs e)
		{
			if ((_content == null) 
				|| (_content.CurrentState != DrawState.ClipGlyph)
				|| (_glyphClipper == null)
				|| (_glyphClipper.DragMode != ClipSelectionDragMode.None))
			{
				return;
			}

			try
			{
				var clipRegion = new RectangleF((float)numericGlyphLeft.Value,
				                                (float)numericGlyphTop.Value,
				                                (float)numericGlyphWidth.Value,
				                                (float)numericGlyphHeight.Value);

				_glyphClipper.ClipRegion = clipRegion;

				// Readjust the limits.
				numericGlyphLeft.Maximum = _newGlyph.Texture.Settings.Width - numericGlyphWidth.Value;
				numericGlyphTop.Maximum = _newGlyph.Texture.Settings.Height - numericGlyphHeight.Value;

				numericGlyphWidth.Maximum = _newGlyph.Texture.Settings.Width - numericGlyphLeft.Value;
				numericGlyphHeight.Maximum = _newGlyph.Texture.Settings.Height - numericGlyphTop.Value;

				if (sender == numericGlyphLeft)
				{
					_mousePosition.X = clipRegion.Left;
				}

				if (sender == numericGlyphTop)
				{
					_mousePosition.Y = clipRegion.Top;
				}

				if (sender == numericGlyphWidth)
				{
					_mousePosition.X = clipRegion.Right;
				}

				if (sender == numericGlyphHeight)
				{
					_mousePosition.Y = clipRegion.Bottom;
				}

				// Snap the magnifier to the mouse.
				UpdateMagnifierWindow();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
		}

		/// <summary>
		/// Handles the Click event of the checkZoomSnap control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void checkZoomSnap_Click(object sender, EventArgs e)
		{
			if ((_content == null) 
				|| (_content.CurrentState != DrawState.ClipGlyph)
				|| (_zoomWindow == null))
			{
				return;
			}

			UpdateMagnifierWindow();

			GorgonFontEditorPlugIn.Settings.ZoomWindowSnap = checkZoomSnap.Checked;
		}

		/// <summary>
		/// Function to update the kerning glyph.
		/// </summary>
	    private void UpdateKernGlyph()
	    {
			_kernGlyph = _content.Font.Glyphs.FirstOrDefault(item => item.Character == comboSecondGlyph.Text[0]);

			if (_kernGlyph == null)
			{
				if (buttonGlyphKern.Checked)
				{
					buttonGlyphKern.PerformClick();
				}
				return;
			}

			_glyphKernSprite.Texture = _kernGlyph.Texture;
			_glyphKernSprite.TextureRegion = _kernGlyph.TextureCoordinates;
			_glyphKernSprite.Size = _kernGlyph.GlyphCoordinates.Size;
	    }

		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboSecondGlyph control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void comboSecondGlyph_SelectedIndexChanged(object sender, EventArgs e)
		{
			if ((_content == null)
			    || (_content.CurrentState != DrawState.KernPair))
			{
				return;
			}

			try
			{
				if (comboSecondGlyph.SelectedItem == null)
				{
					_kernGlyph = null;
					return;
				}

				UpdateKernGlyph();

			    if (_kernGlyph == null)
			    {
			        return;
			    }

                // Get the current kerning value.
                GetCurrentKerningValue();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
			}
		}

		/// <summary>
		/// Function to calculate the current zoom factor when scaling to the size of the window.
		/// </summary>
	    private void CalculateZoomToWindow()
	    {
			// Get the current zoom level.
			if (!menuItemToWindow.Checked)
			{
				return;
			}

			try
			{
				Vector2 zoomValue = CurrentTexture.Settings.Size;

				if (panelTextures.ClientSize.Width != 0)
				{
					zoomValue.X = panelTextures.ClientSize.Width / zoomValue.X;
				}
				else
				{
					zoomValue.X = 1e-6f;
				}

				if (panelTextures.ClientSize.Height != 0)
				{
					zoomValue.Y = panelTextures.ClientSize.Height / zoomValue.Y;
				}
				else
				{
					zoomValue.Y = 1e-6f;
				}

				_currentZoom = (zoomValue.Y < zoomValue.X) ? zoomValue.Y : zoomValue.X;
			}
			finally
			{
				ValidateControls();
			}
	    }

		/// <summary>
		/// Function to update the glyph regions.
		/// </summary>
	    private void UpdateGlyphRegions()
	    {
			if (_currentTextureIndex < 0)
			{
				_currentTextureIndex = 0;
			}

			if (_currentTextureIndex >= _textures.Length)
			{
				_currentTextureIndex = _textures.Length - 1;

				// If for some reason we don't have textures, then leave.
				if (_currentTextureIndex < 0)
				{
					return;
				}
			}

			// Recalculate 
			var glyphs = from GorgonGlyph fontGlyph in _content.Font.Glyphs
						 where fontGlyph.Texture == CurrentTexture && !char.IsWhiteSpace(fontGlyph.Character)
						 select fontGlyph;

			// Find the regions for the glyph.
			_hoverGlyph = null;
			_glyphRegions = new Dictionary<GorgonGlyph, RectangleF>();


			foreach (var glyph in glyphs)
			{
				var glyphRect = new RectangleF((glyph.GlyphCoordinates.Left - 1) * _currentZoom,
												(glyph.GlyphCoordinates.Top - 1) * _currentZoom,
												(glyph.GlyphCoordinates.Width + 1) * _currentZoom,
												(glyph.GlyphCoordinates.Height + 1) * _currentZoom);

				_glyphRegions[glyph] = glyphRect;
			}


		    if (_content.CurrentState != DrawState.DrawFontTextures)
		    {
		        return;
		    }

		    Size newSize = CurrentTexture.Settings.Size;
            newSize.Width = (int)(newSize.Width * _currentZoom);
            newSize.Height = (int)(newSize.Height * _currentZoom);

            CheckForScroll(newSize);
	    }

        /// <summary>
        /// Handles the Scroll event of the scrollTextVertical control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ScrollEventArgs"/> instance containing the event data.</param>
        private void scrollTextVertical_Scroll(object sender, ScrollEventArgs e)
        {
            scrollTextVertical.Value = e.NewValue;
            _content.DrawPreviewText();
        }

		/// <summary>
		/// Handles the Scroll event of the scrollVertical control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ScrollEventArgs"/> instance containing the event data.</param>
		private void scrollVertical_Scroll(object sender, ScrollEventArgs e)
		{
			var scroller = (ScrollBar)sender;

			if (scroller == scrollVertical)
			{
				scrollVertical.Value = e.NewValue;
			}

			if (scroller == scrollHorizontal)
			{
				scrollHorizontal.Value = e.NewValue;
			}

			switch (_content.CurrentState)
			{
				case DrawState.DrawFontTextures:
				case DrawState.ClipGlyph:
					_content.Draw();
					break;
			}
		}

		/// <summary>
		/// Handles the Resize event of the texture display panel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void TextureDisplayResize(object sender, EventArgs e)
		{
		    try
		    {
		        _sortedTextures = null;

				if ((_content == null) || (_content.Font == null))
				{
					_textures = new GorgonTexture2D[0];
					return;
				}

				// Update the texture list.
				_textures = (from glyph in _content.Font.Glyphs
							 select glyph.Texture).Distinct().ToArray();

				// If we had a previously selected glyph, then try and find it in the updated glyphs from
				// the new font.  Otherwise, deselect it.
				if (_selectedGlyph != null)
				{
					_selectedGlyph = _content.Font.Glyphs.FirstOrDefault(item => item.Character == _selectedGlyph.Character);

					// We found the glyph, let's see if we can't focus on the texture it was on.
					if (_selectedGlyph != null)
					{
						_currentTextureIndex = Array.IndexOf(_textures, _selectedGlyph.Texture);
					}
					else
					{
						// If we're editing a glyph that doesn't exist, then we need to stop editing immediately.
						if (_content.CurrentState != DrawState.DrawFontTextures)
						{
							_content.CurrentState = DrawState.DrawFontTextures;
                            _nextState = DrawState.DrawFontTextures;
						}
					}
				}

			    _currentTextureIndex = _currentTextureIndex.Max(0).Min(_textures.Length - 1);

				// Disable any animations for the font textures.
				_indexTransition = _currentTextureIndex;

		        switch (_content.CurrentState)
		        {
		            case DrawState.DrawFontTextures:
				        if (menuItemToWindow.Checked)
				        {
					        CalculateZoomToWindow();
				        }
		                break;
                    case DrawState.ClipGlyph:
                        if (_selectedGlyph == null)
                        {
                            _content.CurrentState = DrawState.GlyphEdit;
                            break;
                        }

                        CheckForScroll(_selectedGlyph.Texture.Settings.Size);
		                break;
		        }

				UpdateGlyphRegions();
                InitializeTextureSprites();

				_text.Font = _content.Font;

                CheckTextPreviewScroll();

				// Kill any transition.
				if (_editGlyphAnimation.CurrentAnimation != null)
				{
					_editGlyphAnimation.CurrentAnimation.Time = _editGlyphAnimation.CurrentAnimation.Speed > 0
						                                            ? _editGlyphAnimation.CurrentAnimation.Length
						                                            : 0;
					_editBackgroundAnimation.CurrentAnimation.Time = _editBackgroundAnimation.CurrentAnimation.Speed > 0
						                                                 ? _editBackgroundAnimation.CurrentAnimation.Length
						                                                 : 0;
					_editBackgroundAnimation.Update();
					_editGlyphAnimation.Update();
					_editBackgroundAnimation.Stop();
					_editGlyphAnimation.Stop();
				}

			    switch (_content.CurrentState)
			    {
					case DrawState.ToGlyphEdit:
					case DrawState.GlyphEdit:
						UpdateGlyphEditor();
						_nextState = DrawState.FromGlyphEdit;
					    break;
					case DrawState.KernPair:
						UpdateGlyphEditor();
						UpdateKernGlyph();
					    break;
					case DrawState.FromGlyphEdit:
					case DrawState.NextTexture:
					case DrawState.PrevTexture:
					case DrawState.DrawFontTextures:
						_content.CurrentState = DrawState.DrawFontTextures;
						_nextState = DrawState.DrawFontTextures;

					    break;
			    }
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
			}
		}

        /// <summary>
        /// Function called when the settings for the content plug-in have changed.
        /// </summary>
        /// <remarks>
        /// Plug-in implementors should implement this method to facilitate the updating of the UI when a plug-in setting has changed.  This 
        /// only applies to plug-ins that implement 
        /// <see cref="Gorgon.Editor.IPlugInSettingsUI" />.
        /// </remarks>
        protected override void OnEditorSettingsChanged()
        {
            if (_content == null)
            {
                return;
            }

            textPreviewText.Text = GorgonFontEditorPlugIn.Settings.SampleText;
			comboBlendMode.Text = comboBlendMode.Items.Contains(GorgonFontEditorPlugIn.Settings.BlendMode)
						  ? GorgonFontEditorPlugIn.Settings.BlendMode
						  : comboBlendMode.Items[0].ToString();

            itemPreviewShadowEnable.Checked = GorgonFontEditorPlugIn.Settings.ShadowEnabled;
            itemPreviewShadowEnable_Click(this, EventArgs.Empty);

            if (_content.CurrentState != DrawState.ClipGlyph)
            {
                return;
            }

            checkZoomSnap.Checked = GorgonFontEditorPlugIn.Settings.ZoomWindowSnap;
            checkZoomSnap_Click(this, EventArgs.Empty);
            numericZoomAmount.Value = (decimal)GorgonFontEditorPlugIn.Settings.ZoomWindowScaleFactor;
            numericZoomWindowSize.Value = GorgonFontEditorPlugIn.Settings.ZoomWindowSize;
        }

		/// <summary>
		/// Function to localize the text of the controls on the form.
		/// </summary>
		protected override void LocalizeControls()
		{
			Text = Resources.GORFNT_TITLE;
		    menuItemRemoveGlyphImage.Text = Resources.GORFNT_ACC_TEXT_RESET_GLYPH_IMAGE;
		    labelGlyphOffsetLeft.Text = string.Format("{0}:", Resources.GORFNT_TEXT_LEFT_OFFSET);
		    labelGlyphOffsetTop.Text = string.Format("{0}:", Resources.GORFNT_TEXT_TOP_OFFSET);
		    labelGlyphAdvance.Text = string.Format("{0}:", Resources.GORFNT_TEXT_ADVANCEMENT);
		    buttonResetGlyphOffset.Text = Resources.GORFNT_TEXT_RESET_OFFSET;
            tipButtons.SetToolTip(buttonResetGlyphOffset, Resources.GORFNT_TIP_RESET_OFFSET);
		    buttonResetGlyphAdvance.Text = Resources.GORFNT_TEXT_RESET_ADVANCE;
            tipButtons.SetToolTip(buttonResetGlyphAdvance, Resources.GORFNT_TIP_RESET_ADVANCE);
		    buttonKernOK.Text = Resources.GORFNT_ACC_TEXT_UPDATE;
		    buttonKernCancel.Text = Resources.GORFNT_ACC_TEXT_RESET;
		    labelKerningSecondaryGlyph.Text = string.Format("{0}:", Resources.GORFNT_TEXT_SECONDARY_GLYPH);
		    labelKerningOffset.Text = string.Format("{0}:", Resources.GORFNT_TEXT_KERNING_OFFSET);
		    buttonGlyphClipOK.Text = Resources.GORFNT_ACC_TEXT_UPDATE;
		    tipButtons.SetToolTip(buttonGlyphClipOK, Resources.GORFNT_TIP_ACCEPT_GLYPH_REGION);
            buttonGlyphClipCancel.Text = Resources.GORFNT_ACC_TEXT_RESET;
            tipButtons.SetToolTip(buttonGlyphClipCancel, Resources.GORFNT_TIP_CANCEL_GLYPH_REGION);
		    labelZoomWindowSize.Text = string.Format("{0}:", Resources.GORFNT_TEXT_ZOOM_WINSIZE);
		    labelZoomAmount.Text = string.Format("{0}:", Resources.GORFNT_TEXT_ZOOM_AMOUNT);
		    checkZoomSnap.Text = Resources.GORFNT_TEXT_SNAP_ZOOM;
		    labelGlyphLeft.Text = string.Format("{0}:", Resources.GORFNT_TEXT_LEFT);
		    labelGlyphTop.Text = string.Format("{0}:", Resources.GORFNT_TEXT_TOP);
            labelGlyphWidth.Text = string.Format("{0}:", Resources.GORFNT_TEXT_WIDTH);
		    labelGlyphHeight.Text = string.Format("{0}:", Resources.GORFNT_TEXT_HEIGHT);
		    buttonEditPreviewText.Text = Resources.GORFNT_TEXT_EDIT_PREVIEW_TEXT;
		    labelFindGlyph.Text = string.Format("{0}:", Resources.GORFNT_TEXT_FIND_GLYPH);
		    buttonSearchGlyph.Text = Resources.GORFNT_TEXT_SEARCH_GLYPH;
			buttonEditGlyph.Text = Resources.GORFNT_TEXT_EDIT_SELECTED_GLYPH;
		    buttonGoHome.Text = Resources.GORFNT_TEXT_GO_BACK;
		    buttonGoHome.ToolTipText = Resources.GORFNT_TIP_GO_BACK;
			buttonGlyphKern.Text = Resources.GORFNT_TIP_EDIT_KERNING;
			buttonGlyphTools.ToolTipText = Resources.GORFNT_TEXT_EDIT_GLYPH_IMAGE;
			buttonRevert.Text = Resources.GORFNT_TEXT_REVERT;
			buttonSave.Text = Resources.GORFNT_TEXT_SAVE;
			menuItemSetGlyph.Text = Resources.GORFNT_ACC_TEXT_UPDATE_GLYPH_RGN;
			menuItemLoadGlyphImage.Text = Resources.GORFNT_ACC_TEXT_LOAD_GLYPH_IMAGE;
			menuTextColor.Text = Resources.GORFNT_TIP_DISPLAY_COLORS;
			itemSampleTextForeground.Text = Resources.GORFNT_ACC_TEXT_FOREGROUND_COLOR;
			itemSampleTextBackground.Text = Resources.GORFNT_ACC_TEXT_BACKGROUND_COLOR;
			menuShadow.Text = Resources.GORFNT_TEXT_EDIT_PREVIEW_TEXT_SHADOW;
			itemPreviewShadowEnable.Text = Resources.GORFNT_ACC_TEXT_ENABLE_SHADOW;
			itemShadowOpacity.Text = Resources.GORFNT_ACC_TEXT_SHADOW_OPACITY;
			itemShadowOffset.Text = Resources.GORFNT_ACC_TEXT_SHADOW_OFFSET;
			labelPreviewText.Text = string.Format("{0}:", Resources.GORFNT_TEXT_PREVIEW_TEXT);
			labelTextureCount.Text = string.Format("{0}: 0/0", Resources.GORFNT_TEXT_TEXTURE);
			dropDownZoom.Text = string.Format("{0}: {1}", Resources.GORFNT_TEXT_ZOOM, Resources.GORFNT_TEXT_TO_WINDOW);
			menuItemToWindow.Text = Resources.GORFNT_TEXT_TO_WINDOW;
			menuItem1600.Text = 16.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem800.Text = 8.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem400.Text = 4.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem200.Text = 2.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem100.Text = 1.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem75.Text = 0.75.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem50.Text = 0.5.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem25.Text = 0.25.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			labelBlendMode.Text = Resources.GORFNT_TEXT_BLEND_MODE;
		}

		/// <summary>
		/// Function to set the font used for the kerning combo.
		/// </summary>
	    private void SetKerningComboFont()
		{
            Debug.Assert(ParentForm != null, "No parent form!!");

			comboSecondGlyph.Font = Font;

			// Recreate the kerning combo font.
			if (_kernComboFont != null)
			{
				_kernComboFont.Dispose();
			}

			FontFamily family = FontFamily.Families.First(item =>
														  string.Equals(_content.FontFamily,
																		item.Name,
																		StringComparison.OrdinalIgnoreCase));

			var fontStyles = (FontStyle[])Enum.GetValues(typeof(FontStyle));

			FontStyle currentStyle = family.IsStyleAvailable(FontStyle.Regular) ? FontStyle.Regular : fontStyles[0];

			_kernComboFont = new Font(_content.FontFamily,
			                          _content.FontSize.Min(24).Max(10),
			                          currentStyle,
			                          _content.FontHeightMode == FontHeightMode.Pixels ? GraphicsUnit.Pixel : GraphicsUnit.Point);
			comboSecondGlyph.Font = _kernComboFont;

		    comboSecondGlyph.MaxDropDownItems = ((ParentForm.ClientSize.Height / 2) / (_kernComboFont.Height)).Max(8);
		}

		/// <summary>
		/// Function called when a property is changed on the related content.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="value">New value assigned to the property.</param>
		protected override void OnContentPropertyChanged(string propertyName, object value)
		{
			base.OnContentPropertyChanged(propertyName, value);

			try
			{
				switch (propertyName)
				{
						// Skip the resize functionality when updating the glyph advancement, offset and kerning.
					case "SelectedGlyphAdvance":
					case "SelectedGlyphOffset":
						break;
					case "Revert":
					case "Characters":
						PopulateSearchCharacterList();
						TextureDisplayResize(this, EventArgs.Empty);
						break;
					default:
						TextureDisplayResize(this, EventArgs.Empty);
						break;
				}

				GetGlyphAdvancementAndOffset();
				SetKerningComboFont();

				// Get the current kerning value for the font face (if we've not customized it already).
				if (_content == null)
				{
					return;
				}

				if (_content.CurrentState != DrawState.KernPair)
				{
					return;
				}

				// If we're editing the kerning for a glyph, and we've disabled kerning, then 
				// reset the editor state.
				if (_content.UseKerningPairs)
				{
					GetCurrentKerningValue();
				}
				else
				{
					buttonGlyphKern.Checked = false;
					buttonGlyphKern_Click(this, EventArgs.Empty);
				}
			}
			finally
			{
				ValidateControls();
			}
		}

		/// <summary>
		/// Function to refresh the UI by a change to the font properties.
		/// </summary>
		/// <param name="propertyName">Name of the property to refresh from.</param>
		/// <param name="value">The current value of the property.</param>
	    public void RefreshByProperty(string propertyName, object value)
	    {
		    OnContentPropertyChanged(propertyName, value);
	    }

		/// <summary>
		/// Function called when the content has changed.
		/// </summary>
		public override void RefreshContent()
		{
			base.RefreshContent();

            Debug.Assert(_content != null, "The content is not font content.");

			// Populate the character search list.
			PopulateSearchCharacterList();

			TextureDisplayResize(this, EventArgs.Empty);

			ValidateControls();
		}

		/// <summary>
		/// Function to create the resources for the content.
		/// </summary>
		public void CreateResources()
		{
			const float animationTime = 0.5f;		// Time for the animation.

			itemPreviewShadowEnable.Checked = GorgonFontEditorPlugIn.Settings.ShadowEnabled;
			panelText.BackColor = Color.FromArgb(GorgonFontEditorPlugIn.Settings.BackgroundColor);
			comboBlendMode.Items.Add(Resources.GORFNT_TEXT_BLEND_MOD);
			comboBlendMode.Items.Add(Resources.GORFNT_TEXT_BLEND_ADD);
			comboBlendMode.Text = comboBlendMode.Items.Contains(GorgonFontEditorPlugIn.Settings.BlendMode)
						  ? GorgonFontEditorPlugIn.Settings.BlendMode
						  : comboBlendMode.Items[0].ToString();
			textPreviewText.Text = GorgonFontEditorPlugIn.Settings.SampleText;
			textPreviewText.Select();

			_text = _content.Renderer.Renderables.CreateText("Sample.Text", _content.Font, string.Empty);
			_text.Color = Color.Black;
			_text.WordWrap = true;
			_text.TextRectangle = new RectangleF(PointF.Empty, panelText.ClientSize);
			_text.ShadowOpacity = GorgonFontEditorPlugIn.Settings.ShadowOpacity;
			_text.ShadowOffset = GorgonFontEditorPlugIn.Settings.ShadowOffset;
			_text.ShadowEnabled = itemPreviewShadowEnable.Checked;
		    _text.AllowColorCodes = true;
			_text.Text = textPreviewText.Text;

            _pattern = ContentObject.Graphics.Textures.CreateTexture<GorgonTexture2D>("Background.Pattern", Resources.Pattern);

            _patternSprite = _content.Renderer.Renderables.CreateSprite("Pattern", new GorgonSpriteSettings
            {
                Color = new GorgonColor(1, 0, 0, 0.4f),
                Texture = _pattern,
                TextureRegion = new RectangleF(0, 0, 1, 1),
                Size = _pattern.Settings.Size
            });

            // Set the sprite up for wrapping the texture.
            _patternSprite.TextureSampler.HorizontalWrapping = TextureAddressing.Wrap;
            _patternSprite.TextureSampler.VerticalWrapping = TextureAddressing.Wrap;


			// Create the glyph sprite.
			_glyphSprite = _content.Renderer.Renderables.CreateSprite("GlyphSprite", new GorgonSpriteSettings());
			_glyphSprite.BlendingMode = BlendingMode.Modulate;

			_glyphKernSprite = _content.Renderer.Renderables.CreateSprite("GlyphKernSprite", new GorgonSpriteSettings());
			_glyphKernSprite.BlendingMode = BlendingMode.Modulate;

			_glyphBackgroundSprite = _content.Renderer.Renderables.CreateSprite("GlyphBackgroundSprite",
			                                                                    new GorgonSpriteSettings
			                                                                    {
				                                                                    InitialPosition = Vector2.Zero,
				                                                                    Size = new Vector2(1),
				                                                                    Color = GorgonColor.Transparent,
				                                                                    Texture = _pattern
			                                                                    });

			_glyphBackgroundSprite.TextureSampler.HorizontalWrapping = TextureAddressing.Wrap;
			_glyphBackgroundSprite.TextureSampler.VerticalWrapping = TextureAddressing.Wrap;

			// Create initial animations.
			_editGlyphAnimation = new GorgonAnimationController<GorgonSprite>();
			var animation = _editGlyphAnimation.Add("TransitionTo", animationTime);
			animation.Tracks["Position"].InterpolationMode = TrackInterpolationMode.Spline;
			animation.Tracks["ScaledSize"].InterpolationMode = TrackInterpolationMode.Spline;
			animation.Tracks["Position"].KeyFrames.Add(new GorgonKeyVector2(0, Vector2.Zero));
			animation.Tracks["Position"].KeyFrames.Add(new GorgonKeyVector2(animation.Length, Vector2.Zero));
			animation.Tracks["ScaledSize"].KeyFrames.Add(new GorgonKeyVector2(0, new Vector2(1)));
			animation.Tracks["ScaledSize"].KeyFrames.Add(new GorgonKeyVector2(animation.Length, new Vector2(1)));
			animation.Tracks["Opacity"].KeyFrames.Add(new GorgonKeySingle(0, 0));
			animation.Tracks["Opacity"].KeyFrames.Add(new GorgonKeySingle(animation.Length, 1.0f));


			_editBackgroundAnimation = new GorgonAnimationController<GorgonSprite>();
			animation = _editBackgroundAnimation.Add("GlyphBGColor", animationTime);
			animation.Tracks["Color"].InterpolationMode = TrackInterpolationMode.Spline;
			animation.Tracks["Color"].KeyFrames.Add(new GorgonKeyGorgonColor(0.0f, GorgonColor.Transparent));
			animation.Tracks["Color"].KeyFrames.Add(new GorgonKeyGorgonColor(animation.Length, new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f)));

            _textureSprites = new List<GorgonSprite>();
		}

		/// <summary>
		/// Function to draw the sample text for the font.
		/// </summary>
		public void DrawText()
		{
			Vector2 textPosition = Vector2.Zero;			

			_content.Renderer.Clear(panelText.BackColor);

			_content.Renderer.Drawing.SmoothingMode = SmoothingMode.Smooth;

			_text.BlendingMode = _previewBlendMode;
			_text.Color = new GorgonColor(GorgonFontEditorPlugIn.Settings.TextColor);		

			if (scrollTextVertical.Visible)
			{
			    textPosition.Y = -scrollTextVertical.Value;
			}

			_text.Position = textPosition;
			_text.Draw();
		}

        /// <summary>
        /// Handles the Click event of the buttonKernCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonKernCancel_Click(object sender, EventArgs e)
        {
            if ((_content == null)
                || (_content.CurrentState != DrawState.KernPair)
                || (_content.Font == null)
                || (comboSecondGlyph.SelectedItem == null))
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                GorgonKerningPair pair = (KernPairComboItem)comboSecondGlyph.SelectedItem;

                if (_content.Font.Settings.KerningPairs.ContainsKey(pair))
                {
                    _content.Font.Settings.KerningPairs.Remove(pair);
                }

                _content.UpdateFontGlyphs();

                int actualKernValue;

                // Reset the drop down.
                _content.Font.KerningPairs.TryGetValue(pair, out actualKernValue);
                numericKerningOffset.Value = actualKernValue;
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
            finally
            {
                ValidateControls();
                Cursor.Current = Cursors.Default;
            }
        }

		/// <summary>
		/// Handles the Click event of the buttonKernOK control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonKernOK_Click(object sender, EventArgs e)
		{
			if ((_content == null)
				|| (_content.CurrentState != DrawState.KernPair)
				|| (_content.Font == null)
                || (comboSecondGlyph.SelectedItem == null))
			{
				return;
			}

			Cursor.Current = Cursors.WaitCursor;
			try
			{
				GorgonKerningPair pair = (KernPairComboItem)comboSecondGlyph.SelectedItem;

				_content.Font.Settings.KerningPairs[pair] = (int)numericKerningOffset.Value;

				_content.UpdateFontGlyphs();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}

        /// <summary>
        /// Function to retrieve the current kerning value.
        /// </summary>
        private void GetCurrentKerningValue()
        {
            // Try to see if we already have a kerning amount defined.
            GorgonKerningPair pair = (KernPairComboItem)comboSecondGlyph.SelectedItem;

            int kernAmount;
            if (!_content.Font.Settings.KerningPairs.TryGetValue(pair, out kernAmount))
            {
                _content.Font.KerningPairs.TryGetValue(pair, out kernAmount);
            }

            numericKerningOffset.Value = kernAmount;
        }

        /// <summary>
        /// Handles the Click event of the buttonGlyphKern control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonGlyphKern_Click(object sender, EventArgs e)
        {
            if ((_content == null)
                || ((_content.CurrentState != DrawState.GlyphEdit) 
                    && (_content.CurrentState != DrawState.KernPair))
                || (_selectedGlyph == null)
                || (ParentForm == null))
            {
                return;
            }

            try
            {
                if (_content.CurrentState == DrawState.KernPair)
                {
                    _content.CurrentState = DrawState.GlyphEdit;
                    return;
                }

				_content.CurrentState = DrawState.KernPair;

				SetKerningComboFont();

	            // Rebuild the glyph list.
                comboSecondGlyph.Items.Clear();

                foreach(char secondChar in _content.Font.Settings.Characters.Where(item => !char.IsWhiteSpace(item)))
                {
                    var pair = new GorgonKerningPair(_selectedGlyph.Character, secondChar);
                    comboSecondGlyph.Items.Add((KernPairComboItem)pair);

                    if ((_content.Font.Settings.KerningPairs.ContainsKey(pair))
						|| (_content.Font.KerningPairs.ContainsKey(pair)))
                    {
                        comboSecondGlyph.SelectedItem = pair;
                    }
                }

                if ((string.IsNullOrEmpty(comboSecondGlyph.Text))
					&& (comboSecondGlyph.Items.Count > 0))
                {
	                comboSecondGlyph.SelectedIndex = 0;
                }

                GetCurrentKerningValue();
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
            finally
            {
                ValidateControls();
            }
        }

        /// <summary>
        /// Handles the Click event of the buttonGoHome control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonGoHome_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedGlyph == null)
                {
                    return;
                }

                if (_textures[_currentTextureIndex] != _selectedGlyph.Texture)
                {
                    _currentTextureIndex = Array.IndexOf(_textures, _selectedGlyph.Texture);
                    _content.CurrentState = DrawState.NextTexture;
                    _nextState = DrawState.ToGlyphEdit;
                    UpdateGlyphRegions();
                    return;
                }

                InitializeGlyphEditTransition(false);
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
            finally
            {
                ValidateControls();
            }
        }

        /// <summary>
		/// Handles the Click event of the buttonEditGlyph control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonEditGlyph_Click(object sender, EventArgs e)
		{
			try
			{
			    if (_selectedGlyph == null)
			    {
			        return;
			    }

			    if (_textures[_currentTextureIndex] != _selectedGlyph.Texture)
			    {
                    _currentTextureIndex = Array.IndexOf(_textures, _selectedGlyph.Texture);
                    _content.CurrentState = DrawState.NextTexture;
                    _nextState = DrawState.ToGlyphEdit;
                    UpdateGlyphRegions();
			        return;
			    }

				InitializeGlyphEditTransition(true);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonResetGlyphOffset control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonResetGlyphOffset_Click(object sender, EventArgs e)
		{
			if ((_content == null)
				|| (_content.CurrentState != DrawState.GlyphEdit)
				|| (_selectedGlyph == null)
				|| (!_content.Font.Settings.Offsets.ContainsKey(_selectedGlyph.Character)))
			{
				return;
			}

			Cursor.Current = Cursors.WaitCursor;
			try
			{
				_content.Font.Settings.Offsets.Remove(_selectedGlyph.Character);
				_content.UpdateFontGlyphOffset(Point.Empty, true);
				_text.Font = _content.Font;
				_text.Refresh();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonResetGlyphAdvance control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonResetGlyphAdvance_Click(object sender, EventArgs e)
		{
			if ((_content == null)
				|| (_content.CurrentState != DrawState.GlyphEdit)
				|| (_selectedGlyph == null)
				|| (!_content.Font.Settings.Advances.ContainsKey(_selectedGlyph.Character)))
			{
				return;
			}

			Cursor.Current = Cursors.WaitCursor;
			try
			{
				_content.Font.Settings.Advances.Remove(_selectedGlyph.Character);
				_content.UpdateFontGlyphAdvance(0, true);
				_text.Refresh();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the MouseEnter event of the panelTextures control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void panelTextures_MouseEnter(object sender, EventArgs e)
		{
			if ((_rawMouse == null)
				|| (_content == null)
				|| (_content.CurrentState != DrawState.ClipGlyph))
			{
				return;
			}

			_rawMouse.Acquired = true;
		}

		/// <summary>
		/// Handles the MouseLeave event of the panelTextures control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void panelTextures_MouseLeave(object sender, EventArgs e)
		{
			if ((_rawMouse == null)
				|| (_content == null)
				|| (_content.CurrentState != DrawState.ClipGlyph)
                || (_glyphClipper == null))
			{
				return;
			}

            // Turn off dragging.
		    _glyphClipper.DragMode = ClipSelectionDragMode.None;

			_rawMouse.Acquired = false;
		}

		/// <summary>
		/// Handles the MouseDoubleClick event of the panelTextures control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelTextures_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			try
			{
				if (_content.CurrentState != DrawState.DrawFontTextures)
				{
					return;
				}

				if ((_textureRegion.Contains(e.Location))
					&& (_selectedGlyph != null))
				{
					InitializeGlyphEditTransition(true);
					return;
				}

				if ((_sortedTextures == null)
				    || (_textures.Length < 2))
				{
					return;
				}

				// Move to the font texture that we clicked on.
				var currentSprite = (from backSprite in _sortedTextures.Reverse()
				                    let region =
					                    new RectangleF(backSprite.Position - (Vector2.Modulate(backSprite.Anchor, backSprite.Scale)),
					                                   backSprite.ScaledSize)
				                    let textureIndex = Array.IndexOf(_textures, backSprite.Texture)
				                    where region.Contains(e.Location) && textureIndex != -1
				                    select new
				                           {
					                           Sprite = backSprite,
					                           TextureIndex = textureIndex
				                           }).FirstOrDefault();

				if (currentSprite == null)
				{
					return;
				}

				_sortedTextures = null;
				if (GorgonFontEditorPlugIn.Settings.ShowAnimations)
				{
					_content.CurrentState = currentSprite.TextureIndex < _currentTextureIndex ? DrawState.PrevTexture : DrawState.NextTexture;
					_nextState = DrawState.DrawFontTextures;
				}

				_currentTextureIndex = currentSprite.TextureIndex;
				UpdateGlyphRegions();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
			}
		}

		/// <summary>
		/// Function to update the selected texture region.
		/// </summary>
		/// <param name="currentTexture">The currently selected texture.</param>
	    private void UpdateTextureRegion(GorgonTexture2D currentTexture)
		{
			var textureMidpoint = new Vector2((currentTexture.Settings.Width * _currentZoom) / 2.0f,
		                                      (currentTexture.Settings.Height * _currentZoom) / 2.0f);

			_textureOffset = Vector2.Subtract(PanelMidPoint, textureMidpoint);

			if (panelHorzScroll.Visible)
			{
				_textureOffset.X = -scrollHorizontal.Value;
			}

			if (panelVertScroll.Visible)
			{
				_textureOffset.Y = -scrollVertical.Value;
			}

			_textureRegion = new RectangleF(_textureOffset, ((Vector2)currentTexture.Settings.Size) * _currentZoom);
	    }

	    /// <summary>
	    /// Function to update the glyph editor area on the screen after a resize.
	    /// </summary>
	    private void UpdateGlyphEditor()
	    {
			CheckForScroll(Size.Empty);

			UpdateTextureRegion(_selectedGlyph.Texture);

			_glyphSprite.Texture = _selectedGlyph.Texture;
			_glyphSprite.TextureRegion = _selectedGlyph.TextureCoordinates;
		    _glyphSprite.Size = _selectedGlyph.GlyphCoordinates.Size;
		}

        /// <summary>
        /// Function to initialize the texture sprites.
        /// </summary>
        private void InitializeTextureSprites()
        {
            _textureSprites.Clear();

            // Create our sprites.
            var sprites = from texture in _textures
                          let spriteSettings = new GorgonSpriteSettings
                                               {
                                                   InitialPosition = Vector2.Zero,
                                                   Anchor = new Vector2(_content.Font.Settings.TextureSize.Width / 2.0f,
                                                                        _content.Font.Settings.TextureSize.Height / 2.0f),
                                                   Texture = texture,
                                                   TextureRegion = new RectangleF(0, 0, 1, 1),
                                                   Size = _content.Font.Settings.TextureSize
                                               }
                          select _content.Renderer.Renderables.CreateSprite(texture.Name, spriteSettings);

            _textureSprites.AddRange(sprites);
        }

		/// <summary>
		/// Function to check and see if a scroll bar is required.
		/// </summary>
		/// <param name="proposedSize">The requested size of the object to display in the panel.</param>
	    private void CheckForScroll(Size proposedSize)
	    {
			// Turn off resizing until we get the window straightened out.
			panelTextures.Resize -= TextureDisplayResize;

			try
			{
				Size clientSize = panelInnerDisplay.ClientSize;

				scrollHorizontal.Scroll -= scrollVertical_Scroll;
				scrollVertical.Scroll -= scrollVertical_Scroll;

				if (proposedSize.Width > panelTextures.ClientSize.Width)
				{
					panelHorzScroll.Visible = true;
					clientSize.Height = clientSize.Height - panelHorzScroll.ClientSize.Height;
				}
				else
				{
					scrollHorizontal.Value = 0;
					panelHorzScroll.Visible = false;
				}

				if (proposedSize.Height > panelTextures.ClientSize.Height)
				{
					panelVertScroll.Visible = true;
					clientSize.Width = clientSize.Width - panelVertScroll.ClientSize.Width;
					
				}
				else
				{
					scrollVertical.Value = 0;
					panelVertScroll.Visible = false;
				}

				if (panelHorzScroll.Visible)
				{
					scrollHorizontal.Maximum = ((proposedSize.Width - clientSize.Width) + (scrollHorizontal.LargeChange)).Max(0);
					scrollHorizontal.Scroll += scrollVertical_Scroll;
				}

				if (panelVertScroll.Visible)
				{
					scrollVertical.Maximum = ((proposedSize.Height - clientSize.Height) + (scrollVertical.LargeChange)).Max(0);
					scrollVertical.Scroll += scrollVertical_Scroll;
				}

				panelTextures.ClientSize = clientSize;
			}
			finally
			{
				// Re-enable resizing.
				panelTextures.Resize += TextureDisplayResize;
			}
	    }

		/// <summary>
		/// Function to retrieve the size of the glyph edit region.
		/// </summary>
		/// <param name="glyph">The glyph to retrieve a region for.</param>
		/// <param name="scaleRegion">The region to scale into.</param>
		/// <param name="scale">The scale of the glyph compared to the render panel.</param>
		/// <returns>The rectangle containing the glyph.</returns>
	    private static Rectangle GetGlyphEditRegion(GorgonGlyph glyph, Vector2 scaleRegion, out float scale)
		{
			var glyphDimensions = new Vector2(glyph.GlyphCoordinates.Size.Width + glyph.Offset.X,
			                                  glyph.GlyphCoordinates.Size.Height + glyph.Offset.Y);

			scale = scaleRegion.Y / glyphDimensions.Y;

			var textureRegion = new Rectangle(0,
			                                  0,
			                                  (int)(glyphDimensions.X * scale),
											  (int)(glyphDimensions.Y * scale));

			textureRegion.Y = (int)((scaleRegion.Y * 0.5f) - (textureRegion.Height * 0.5f));
			textureRegion.X = (int)((scaleRegion.X * 0.5f) - (textureRegion.Width * 0.5f));

			return textureRegion;
	    }

		/// <summary>
		/// Function to retrieve the scaled glyph bounds.
		/// </summary>
		/// <param name="glyph">Glyph to evaluate.</param>
		/// <param name="editRegion">The size of the edit region.</param>
		/// <param name="scale">Current scale level.</param>
		/// <returns>The scaled glyph bounds.</returns>
	    private static Rectangle GetScaledGlyphBounds(GorgonGlyph glyph, Rectangle editRegion, float scale)
	    {
			var glyphDimensions = glyph.GlyphCoordinates.Size;

			var textureRegion = new Rectangle((int)(editRegion.Left + (glyph.Offset.X * scale)),
											  (int)(editRegion.Top + (glyph.Offset.Y * scale)),
											  (int)(glyphDimensions.Width * scale),
											  (int)(glyphDimensions.Height * scale));

			return textureRegion;
	    }

		/// <summary>
		/// Funciton to calculate the position and scale of the texture sprites.
		/// </summary>
		private void ArrangeTextureSprites()
		{
			if ((_indexTransition.EqualsEpsilon(_currentTextureIndex, 0.01f))
				&& ((_content.CurrentState == DrawState.PrevTexture)
				|| (_content.CurrentState == DrawState.NextTexture)))
			{
				_content.CurrentState = DrawState.DrawFontTextures;

				if (_nextState == DrawState.ToGlyphEdit)
				{
					buttonEditGlyph.PerformClick();
					return;
				}

				ValidateControls();
			}

			for (int i = 0; i < _textureSprites.Count; ++i)
			{
				GorgonSprite sprite = _textureSprites[i];

				if ((i == _currentTextureIndex)
					&& (_content.CurrentState != DrawState.NextTexture)
					&& (_content.CurrentState != DrawState.PrevTexture))
				{
					sprite.Scale = new Vector2(1);
					continue;
				}

				float delta = (i - _indexTransition) / _textureSprites.Count;
				float deltaAbs = delta.Abs();

				sprite.Scale = new Vector2((1.0f - deltaAbs * 0.5f) * _currentZoom);
				sprite.Position = new Vector2(PanelMidPoint.X + delta * _content.FontTextureSize.Width * 2.0f * _currentZoom,
											  PanelMidPoint.Y);

				sprite.Color = new GorgonColor(0.753f,
											   0.753f,
											   0.753f,
											   (1.0f - deltaAbs));

				sprite.SmoothingMode = SmoothingMode.Smooth;
			}

			if ((_content.CurrentState != DrawState.NextTexture)
				&& (_content.CurrentState != DrawState.PrevTexture))
			{
				_sortedTextures = _textureSprites.Where(item => item.Texture != _textures[_currentTextureIndex])
												 .OrderBy(item => item.Scale.X);
			}
			else
			{
				_sortedTextures = _textureSprites.OrderBy(item => item.Scale.X);
			}
		}

		/// <summary>
		/// Function to retrieve the current glyph advancement and offset values.
		/// </summary>
	    private void GetGlyphAdvancementAndOffset()
	    {
			numericOffsetX.ValueChanged -= numericOffsetX_ValueChanged;
			numericOffsetY.ValueChanged -= numericOffsetX_ValueChanged;
			numericGlyphAdvance.ValueChanged -= numericGlyphAdvance_ValueChanged;

			try
			{
				if (_selectedGlyph == null)
				{
					numericOffsetX.Value = 0;
					numericOffsetY.Value = 0;
					numericGlyphAdvance.Value = 0;
					return;
				}

				// Set the numeric values.
				numericOffsetX.Value = _selectedGlyph.Offset.X.Min(2048).Max(-2048);
				numericOffsetY.Value = _selectedGlyph.Offset.Y.Min(2048).Max(-2048);
				numericGlyphAdvance.Value = _selectedGlyph.Advance.Min(2048).Max(-2048);
			}
			finally
			{
				numericOffsetX.ValueChanged += numericOffsetX_ValueChanged;
				numericOffsetY.ValueChanged += numericOffsetX_ValueChanged;
				numericGlyphAdvance.ValueChanged += numericGlyphAdvance_ValueChanged;
			}
	    }

	    /// <summary>
		/// Function to initialize the editor transition.
		/// </summary>
		/// <param name="editOn"><b>true</b> when editing a glyph, <b>false</b> when exiting the editor.</param>
	    private void InitializeGlyphEditTransition(bool editOn)
	    {
			// Move back to the selected texture index.
			if (_textures[_currentTextureIndex] != _selectedGlyph.Texture)
			{
				_currentTextureIndex = Array.IndexOf(_textures, _selectedGlyph.Texture);
				UpdateGlyphRegions();
			}

			float scale;
			var textureRegion = GetGlyphEditRegion(_selectedGlyph, panelTextures.ClientSize, out scale);

			var glyphScreenRegion = _glyphRegions[_selectedGlyph];

		    if (!editOn)
		    {
				panelTextures.Resize -= TextureDisplayResize;
				panelGlyphEdit.Visible = false;
				panelGlyphAdvance.Visible = false;
				UpdateTextureRegion(_selectedGlyph.Texture);
				panelTextures.Resize += TextureDisplayResize;

				// When we return from the editor screen, restore the scrollbars.
			    Size newSize = _selectedGlyph.Texture.Settings.Size;

			    newSize.Width = (int)(newSize.Width * _currentZoom);
				newSize.Height = (int)(newSize.Height * _currentZoom);

				CheckForScroll(newSize);

				scrollHorizontal.Value = _lastScrollPoint.X.Max(0).Min(scrollHorizontal.Maximum);
				scrollVertical.Value = _lastScrollPoint.Y.Max(0).Min(scrollVertical.Maximum);

				// The panel may have been resized, we'll need to update our region.
			    if ((panelVertScroll.Visible)
			        || (panelHorzScroll.Visible))
			    {
				    UpdateTextureRegion(_selectedGlyph.Texture);
					textureRegion = GetGlyphEditRegion(_selectedGlyph, panelTextures.ClientSize, out scale);
			    }
		    }

			glyphScreenRegion.X += _textureRegion.X;
			glyphScreenRegion.Y += _textureRegion.Y;

			_editBackgroundAnimation.Stop();
			_editGlyphAnimation.Stop();

		    _glyphSprite.Texture = _selectedGlyph.Texture;
		    _glyphSprite.TextureRegion = _selectedGlyph.TextureCoordinates;
		    _glyphSprite.Size = _selectedGlyph.GlyphCoordinates.Size;

		    _glyphBackgroundSprite.Scale = new Vector2(1);
		    _glyphBackgroundSprite.Position = Vector2.Zero;
			_glyphBackgroundSprite.Size = panelTextures.ClientSize;
		    _glyphBackgroundSprite.TextureRegion = new RectangleF(Vector2.Zero, _pattern.ToTexel(panelTextures.ClientSize));

			if (editOn)
			{
				_lastScrollPoint.X = scrollHorizontal.Value;
				_lastScrollPoint.Y = scrollVertical.Value;

				CheckForScroll(Size.Empty);

				panelTextures.Resize -= TextureDisplayResize;
				panelGlyphEdit.Visible = true;
				panelGlyphAdvance.Visible = true;
				panelTextures.Resize += TextureDisplayResize;
				UpdateTextureRegion(_selectedGlyph.Texture);

				GetGlyphAdvancementAndOffset();
			}

		    if (GorgonFontEditorPlugIn.Settings.ShowAnimations)
		    {
				Rectangle destination = GetScaledGlyphBounds(_selectedGlyph, textureRegion, scale);
				var transitionAnimation = _editGlyphAnimation["TransitionTo"];

				transitionAnimation.Tracks["ScaledSize"].KeyFrames[0] = new GorgonKeyVector2(0.0f, glyphScreenRegion.Size);
				transitionAnimation.Tracks["Position"].KeyFrames[0] = new GorgonKeyVector2(0.0f, glyphScreenRegion.Location);
				transitionAnimation.Tracks["Opacity"].KeyFrames[0] = new GorgonKeySingle(0.0f, 0.0f);
				transitionAnimation.Tracks["ScaledSize"].KeyFrames[1] = new GorgonKeyVector2(transitionAnimation.Length,
																								destination.Size);
				transitionAnimation.Tracks["Position"].KeyFrames[1] = new GorgonKeyVector2(transitionAnimation.Length,
																							destination.Location);
				transitionAnimation.Tracks["Opacity"].KeyFrames[1] = new GorgonKeySingle(transitionAnimation.Length, 1.0f);

				if (editOn)
				{
					_editBackgroundAnimation[0].Speed = 1.0f;
					transitionAnimation.Speed = 1.0f;
				}
				else
				{
					_editBackgroundAnimation[0].Speed = -1.0f;
					transitionAnimation.Speed = -1.0f;
				}

			    _editGlyphAnimation.Play(_glyphSprite, "TransitionTo");
			    _editBackgroundAnimation.Play(_glyphBackgroundSprite, "GlyphBGColor");
			    _content.CurrentState = editOn ? DrawState.ToGlyphEdit : DrawState.FromGlyphEdit;
			    _nextState = DrawState.GlyphEdit;

			    return;
		    }

			_glyphBackgroundSprite.Color = GorgonColor.White;
			if (editOn)
			{
				_content.CurrentState = _nextState = DrawState.GlyphEdit;
			}
			else
			{
				_content.CurrentState = _nextState = DrawState.DrawFontTextures;
			}
	    }

		/// <summary>
		/// Function to draw the glyph edit transition.
		/// </summary>
	    public void DrawGlyphEditTransition()
		{
			DrawFontTexture();

			_glyphBackgroundSprite.Draw();
			_glyphSprite.Draw();

			_editBackgroundAnimation.Update();
			_editGlyphAnimation.Update();

			if (_editBackgroundAnimation.CurrentAnimation != null)
			{
				return;
			}		

			_content.CurrentState = _content.CurrentState == DrawState.ToGlyphEdit
				                        ? DrawState.GlyphEdit
				                        : DrawState.DrawFontTextures;
			_nextState = _content.CurrentState;

            // Refresh the glyph regions.
		    if (_content.CurrentState == DrawState.DrawFontTextures)
		    {
		        UpdateGlyphRegions();
		    }

			ValidateControls();
		}

		/// <summary>
		/// Function to draw the glyph editor screen.
		/// </summary>
	    public void DrawGlyphEdit()
		{
			UpdateTextureRegion(_selectedGlyph.Texture);

			float scale;
		    Vector2 scaleArea = _content.CurrentState != DrawState.KernPair
		                            ? panelTextures.ClientSize
		                            : new Vector2(panelTextures.ClientSize.Width * 0.5f, panelTextures.ClientSize.Height);

			Rectangle region = GetGlyphEditRegion(_selectedGlyph, scaleArea, out scale);

            // Put the left glyph near to center, but offset by its width.
            if (_content.CurrentState == DrawState.KernPair)
            {
                region.X = (int)PanelMidPoint.X - region.Width;
            }
            
            Rectangle glyphBounds = GetScaledGlyphBounds(_selectedGlyph, region, scale);
            region.Width = (int)((_selectedGlyph.Offset.X + _selectedGlyph.Advance) * scale);


            _glyphBackgroundSprite.Position = Vector2.Zero;
            _glyphBackgroundSprite.ScaledSize = panelTextures.ClientSize;
            _glyphBackgroundSprite.TextureRegion = new RectangleF(Vector2.Zero, _pattern.ToTexel(panelTextures.ClientSize));
            _glyphBackgroundSprite.Draw();
			
			_glyphSprite.Position = glyphBounds.Location;
			_glyphSprite.ScaledSize = glyphBounds.Size;
			
			_content.Renderer.Drawing.FilledRectangle(region, new GorgonColor(Color.Gray, 0.35f));

			_glyphSprite.Draw();

			if (_content.CurrentState == DrawState.GlyphEdit)
			{
				var advanceLine = new RectangleF(region.Left + (_selectedGlyph.Offset.X * scale),
				                                 (region.Bottom - scale),
				                                 _selectedGlyph.Advance * scale,
				                                 scale);

				_content.Renderer.Drawing.FilledRectangle(advanceLine, new GorgonColor(Color.Green, 0.5f));
			}
			else
			{
				// We need the same scale so we can accurately measure the distance.
				float dummy;
				var kernValue = (float)numericKerningOffset.Value;
				var offset = (int)((_content.OutlineSize + _selectedGlyph.Advance  + kernValue) * scale) + region.Left;
			    Rectangle leftRegion = region;

				region = GetGlyphEditRegion(_kernGlyph, scaleArea, out dummy);
				region.X = offset;
			    region.Y = leftRegion.Y;
			    region.Height = leftRegion.Height;
				glyphBounds = GetScaledGlyphBounds(_kernGlyph, region, scale);

				region.Width = (int)((_kernGlyph.Offset.X + _kernGlyph.Advance) * scale);

				_glyphKernSprite.Position = glyphBounds.Location;
				_glyphKernSprite.ScaledSize = glyphBounds.Size;

				_content.Renderer.Drawing.FilledRectangle(region, new GorgonColor(Color.Gray, 0.35f));

				_glyphKernSprite.Draw();
			}
		}

		/// <summary>
		/// Function to draw the glyph clipping.
		/// </summary>
	    public void DrawGlyphClip()
		{
			if (_newGlyph == null)
			{
				return;
			}

		    GorgonTexture2D currentTexture = _newGlyph.Texture;

			_content.Renderer.Drawing.TextureSampler.HorizontalWrapping = TextureAddressing.Wrap;
			_content.Renderer.Drawing.TextureSampler.VerticalWrapping = TextureAddressing.Wrap;
			
			// Draw the background.
		    _content.Renderer.Drawing.FilledRectangle(panelTextures.ClientRectangle, GorgonColor.White, _pattern, _pattern.ToTexel(panelTextures.ClientRectangle));

			BlendingMode previousMode = _content.Renderer.Drawing.BlendingMode;

			_content.Renderer.Drawing.BlendingMode = BlendingMode.Modulate;

            _content.Renderer.Drawing.FilledRectangle(new RectangleF(0, 0, currentTexture.Settings.Width, currentTexture.Settings.Height), new GorgonColor(0, 0, 0, 0.3f));

            _content.Renderer.Drawing.DrawLine(new Vector2(0, currentTexture.Settings.Height),
                                               new Vector2(currentTexture.Settings.Width,
                                                           currentTexture.Settings.Height),
											   GorgonColor.Black);

            _content.Renderer.Drawing.DrawLine(new Vector2(currentTexture.Settings.Width, 0),
                                               new Vector2(currentTexture.Settings.Width,
                                                           currentTexture.Settings.Height),
											   GorgonColor.Black);

            _content.Renderer.Drawing.Blit(currentTexture, new Vector2(-scrollHorizontal.Value, -scrollVertical.Value));


			// Draw selected area.
			_glyphClipper.Draw();
			
			_content.Renderer.Drawing.BlendingMode = BlendingMode.Inverted;
			_content.Renderer.Drawing.DrawLine(new Vector2(_mousePosition.X, 0), new Vector2(_mousePosition.X, panelTextures.ClientSize.Height), GorgonColor.White);
			_content.Renderer.Drawing.DrawLine(new Vector2(0, _mousePosition.Y), new Vector2(panelTextures.ClientSize.Width, _mousePosition.Y), GorgonColor.White);

			_zoomWindow.Draw();

			_content.Renderer.Drawing.BlendingMode = previousMode;
			_content.Renderer.Drawing.TextureSampler.HorizontalWrapping = TextureAddressing.Clamp;
			_content.Renderer.Drawing.TextureSampler.VerticalWrapping = TextureAddressing.Clamp;
			
			UpdateGlyphInfo();
	    }

        /// <summary>
        /// Function to draw the font texture(s).
        /// </summary>
        public void DrawFontTexture()
        {
            GorgonTexture2D currentTexture = _textures[_currentTextureIndex];
            
            if (_textureSprites.Count > 1)
            {
                if ((_sortedTextures == null)
					|| (_content.CurrentState == DrawState.NextTexture)
					|| (_content.CurrentState == DrawState.PrevTexture))
                {
                    ArrangeTextureSprites();

                    Debug.Assert(_sortedTextures != null, "No sorted textures.");
                }

                foreach (GorgonSprite backSprite in _sortedTextures)
                {
	                if (panelVertScroll.Visible)
	                {
		                backSprite.Position = new Vector2(backSprite.Position.X, -scrollVertical.Value + (backSprite.Anchor.Y * backSprite.Scale.Y));
	                }

                    float color = backSprite.Color.Red * (1.0f - backSprite.Color.Alpha) * 0.5f;
                    _content.Renderer.Drawing.FilledRectangle(new RectangleF(backSprite.Position - (Vector2.Modulate(backSprite.Anchor, backSprite.Scale)), backSprite.ScaledSize),
                                                              new GorgonColor(color, color, color, backSprite.Opacity));
                    backSprite.Draw();
                }

				if ((_content.CurrentState == DrawState.NextTexture)
					|| (_content.CurrentState == DrawState.PrevTexture))
	            {
		            _indexTransition += (_currentTextureIndex - _indexTransition) * GorgonTiming.Delta * 4.0f;
					return;
	            }

		        _indexTransition = _currentTextureIndex;
            }

            UpdateTextureRegion(currentTexture);

            // Fill the background so we can clearly see the first page.
            _content.Renderer.Drawing.FilledRectangle(_textureRegion, Color.Black);
           
            // If we're animating into/out of or are currently at the glyph editor, then overlay the background textures.
            if ((_content.CurrentState == DrawState.ToGlyphEdit)
                || (_content.CurrentState == DrawState.FromGlyphEdit)
                || (_content.CurrentState == DrawState.GlyphEdit))
            {
                _content.Renderer.Drawing.FilledRectangle(panelTextures.ClientRectangle,
                                                          new GorgonColor(Color.Black, _glyphBackgroundSprite.Opacity * 0.5f));
            }

            GorgonSprite sprite = _textureSprites[_currentTextureIndex];
            sprite.SmoothingMode = SmoothingMode.None;
            sprite.Color = GorgonColor.White;
            sprite.Scale = new Vector2(_currentZoom);

            // If the scroller is active, then position it based on that.
            if ((!panelHorzScroll.Visible)
                && (!panelVertScroll.Visible))
            {
                sprite.Position = PanelMidPoint;
            }
            else
            {
                sprite.Position = new Vector2(_textureRegion.Left + sprite.ScaledSize.X / 2.0f,
                                              _textureRegion.Top + sprite.ScaledSize.Y / 2.0f);
            }

            sprite.Draw();

			if (_content.CurrentState != DrawState.GlyphEdit)
			{
				// Draw the borders around each glyph.
				foreach (var glyph in _glyphRegions)
				{
					var glyphRect = glyph.Value;
					glyphRect.X += _textureOffset.X;
					glyphRect.Y += _textureOffset.Y;

					_content.Renderer.Drawing.DrawRectangle(glyphRect, Color.Red);
				}
			}

			// If the editor is visible, then don't draw the selected texture.
			if (_content.CurrentState == DrawState.GlyphEdit)
			{
				return;
			}

			RectangleF rect;

            _selectorBackGroundPos.X += (GorgonTiming.Delta * 16.0f) / _pattern.Settings.Width;
            _selectorBackGroundPos.Y += (GorgonTiming.Delta * 16.0f) / _pattern.Settings.Height;

            if (_selectorBackGroundPos.X > 1.0f)
            {
                _selectorBackGroundPos.X = 0.0f;
            }

            if (_selectorBackGroundPos.Y > 1.0f)
            {
                _selectorBackGroundPos.Y = 0.0f;
            }

			if (_hoverGlyph != null)
			{
				rect = _glyphRegions[_hoverGlyph];
				rect.X += _textureOffset.X;
				rect.Y += _textureOffset.Y;

				_patternSprite.Position = rect.Location;
				_patternSprite.Size = rect.Size;
				_patternSprite.Color = new GorgonColor(1, 0, 0, 0.4f);
				_patternSprite.TextureRegion = new RectangleF(_selectorBackGroundPos.X,
							                                    _selectorBackGroundPos.Y,
							                                    rect.Width / _pattern.Settings.Width,
							                                    rect.Height / _pattern.Settings.Height);
				_patternSprite.Draw();
			}

			if ((_selectedGlyph == null) || (_selectedGlyph.Texture != currentTexture))
			{
				return;
			}

			rect = _glyphRegions[_selectedGlyph];
			rect.X += _textureOffset.X;
			rect.Y += _textureOffset.Y;

			_patternSprite.Position = rect.Location;
			_patternSprite.Size = rect.Size;
			_patternSprite.Color = new GorgonColor(0.25f, 0.25f, 1.0f, 0.4f);
			_patternSprite.TextureRegion = new RectangleF(_selectorBackGroundPos.X,
			                                              _selectorBackGroundPos.Y,
			                                              rect.Width / _pattern.Settings.Width,
			                                              rect.Height / _pattern.Settings.Height);
			_patternSprite.Draw();
	    }
		#endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFontContentPanel"/> class.
        /// </summary>
        public GorgonFontContentPanel()
        {
            InitializeComponent();    
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFontContentPanel"/> class.
        /// </summary>
        public GorgonFontContentPanel(GorgonFontContent content, GorgonInputServiceFactory input)
            : base(content, input)
        {
            _content = content;

            InitializeComponent();

			MouseWheel += PanelDisplay_MouseWheel;
        }
        #endregion
	}
}
