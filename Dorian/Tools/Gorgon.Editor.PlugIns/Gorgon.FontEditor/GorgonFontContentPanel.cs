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
using System.Linq;
using System.Windows.Forms;
using Fetze.WinFormsColor;
using GorgonLibrary.Animation;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.FontEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.Input;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using GorgonLibrary.Renderers;
using GorgonLibrary.UI;
using SlimMath;

// TODO: Add texture clipping for glyphs.
// TODO: Add spacing/kerning interfaces for glyphs.

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
    /// <summary>
    /// Control for displaying font data.
    /// </summary>
    partial class GorgonFontContentPanel 
		: ContentPanel
    {
        #region Variables.
        private Vector2 _panelMidPoint = Vector2.Zero;
        private GorgonKeyboard _rawKeyboard;
	    private GorgonPointingDevice _rawMouse;
		private bool _disposed;
	    private GorgonTexture2D _pattern;
	    private float _currentZoom = -1;
	    private int _currentTextureIndex;
	    private float _indexTransition;
	    private GorgonFontContent _content;
	    private GorgonText _text;
	    private GorgonSprite _patternSprite;
	    private Dictionary<GorgonGlyph, RectangleF> _glyphRegions;
	    private Vector2 _textureOffset = Vector2.Zero;
	    private RectangleF _textureRegion = RectangleF.Empty;
	    private GorgonGlyph _hoverGlyph;
	    private Vector2 _selectorBackGroundPos = Vector2.Zero;
	    private GorgonGlyph _selectedGlyph;
	    private GorgonTexture2D[] _textures;
	    private GorgonRenderTarget2D _editGlyph;
	    private GorgonAnimationController<GorgonSprite> _editGlyphAnimation;
		private GorgonAnimationController<GorgonSprite> _editBackgroundAnimation;
        private List<GorgonSprite> _textureSprites;
	    private GorgonSprite _glyphSprite;
	    private GorgonSprite _glyphBackgroundSprite;
        private IEnumerable<GorgonSprite> _sortedTextures;
        private DrawState _nextState = DrawState.DrawFontTextures;
	    private Point _lastScrollPoint = Point.Empty;
	    private Clipper _glyphClipper;
	    private ZoomWindow _zoomWindow;
	    private GorgonFont _zoomFont;
	    private Vector2 _mousePosition;
	    private GorgonGlyph _newGlyph;
        #endregion

        #region Methods.
		/// <summary>
		/// Handles the Load event of the GorgonFontContentPanel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void GorgonFontContentPanel_Load(object sender, EventArgs e)
		{
			try
			{
				_rawKeyboard = RawInput.CreateKeyboard(panelTextures);
				_rawKeyboard.Enabled = true;
				_rawKeyboard.KeyUp += GorgonFontContentPanel_KeyUp;

				Point lastPosition = Cursor.Position;
				_rawMouse = RawInput.CreatePointingDevice(panelTextures);
				_rawMouse.Acquired = false;
				_rawMouse.Enabled = true;
				_rawMouse.PointingDeviceMove += ClipMouseMove;
                _rawMouse.PointingDeviceDown += ClipMouseDown;
                _rawMouse.PointingDeviceUp += ClipMouseUp;
                _rawMouse.PointingDeviceWheelMove += ClipMouseWheel;
				Cursor.Position = lastPosition;

				_glyphClipper = new Clipper(_content.Renderer, panelTextures)
				                {
					                DefaultCursor = Cursors.Cross,
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
                _zoomWindow.ZoomAmount -= 0.5f;
            }
            else if (pointingDeviceEventArgs.WheelDelta > 0)
            {
                _zoomWindow.ZoomAmount += 0.5f;
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
            _zoomWindow.Position = _mousePosition;

            var zoomPosition = new Vector2(_mousePosition.X + 4,
                                           _mousePosition.Y + 4);

            if ((zoomPosition.X + _zoomWindow.ZoomWindowSize.X) > panelTextures.ClientSize.Width - 4)
            {
                zoomPosition.X = _mousePosition.X - _zoomWindow.ZoomWindowSize.X - 4;
            }

            if ((zoomPosition.Y + _zoomWindow.ZoomWindowSize.Y) > panelTextures.ClientSize.Height - 4)
            {
                zoomPosition.Y = _mousePosition.Y - _zoomWindow.ZoomWindowSize.Y - 4;
            }

            /*var zoomPosition = new Vector2(panelTextures.ClientSize.Width - _zoomWindow.ZoomWindowSize.X, 0);

            if ((position.Y < _zoomWindow.ZoomWindowSize.Y) && (position.X > panelTextures.ClientSize.Width * 0.5f))
            {
                zoomPosition.Y = panelTextures.ClientSize.Height - _zoomWindow.ZoomWindowSize.Y;
            }

            if ((position.X >= zoomPosition.X) && (position.Y > panelTextures.ClientSize.Height * 0.5f))
            {
                zoomPosition.X = 0;
            }*/

            _zoomWindow.ZoomWindowLocation = zoomPosition;
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

			_glyphClipper.OnMouseMove(pointingDeviceEventArgs);

            UpdateMagnifierWindow();
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

			_content.CurrentState = DrawState.GlyphEdit;

			panelTextures.Cursor = DefaultCursor;
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

				_newGlyph = new GorgonGlyph(_newGlyph.Character,
				                            _newGlyph.Texture,
				                            Rectangle.Truncate(_glyphClipper.ClipRegion),
				                            _newGlyph.Offset,
				                            _newGlyph.Advance);

				// If we've customized this glyph before, remove it from the customization list.
				GorgonGlyph existingCustomGlyph =
					_content.Font.Settings.Glyphs.FirstOrDefault(item => item.Character == _newGlyph.Character);

				while (existingCustomGlyph != null)
				{
					_content.Font.Settings.Glyphs.Remove(existingCustomGlyph);

					existingCustomGlyph = _content.Font.Settings.Glyphs.FirstOrDefault(item => item.Character == _newGlyph.Character);
				}

				EndGlyphClipping();

				_content.Font.Settings.Glyphs.Add(_newGlyph);
				_content.UpdateFont();
                _content.DisableProperty("FontTextureSize", true);
            }
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				_newGlyph = null;
				panelTextures.Focus();
				ValidateControls();

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

				EndGlyphClipping();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				_newGlyph = null;
				panelTextures.Focus();
				ValidateControls();
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
						if ((_selectedGlyph == null)
						    || (_content.CurrentState == DrawState.ToGlyphEdit)
						    || (_content.CurrentState == DrawState.GlyphEdit))
						{
							return;
						}

						buttonEditGlyph.PerformClick();
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
							case DrawState.ClipGlyph:
								buttonGlyphClipCancel.PerformClick();
								break;
							case DrawState.GlyphEdit:
							case DrawState.ToGlyphEdit:
								buttonEditGlyph.PerformClick();
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

		    Dependency dependency = _content.Dependencies.FirstOrDefault(item => item.DependencyObject == glyph.Texture);

		    if (dependency != null)
		    {
			    _content.Dependencies[dependency.Path, dependency.Type] = null;
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
                                             string.Format(Resources.GORFNT_REMOVE_TEXTURE_PROMPT,
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

                _content.UpdateFont();
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
		/// <param name="textureFile">The file selected to load.</param>
	    private GorgonTexture2D LoadTexture(GorgonFileSystemFileEntry textureFile)
	    {
			GorgonTexture2D texture;
			var transform = GlyphTextureTransform.None;

			using (var fileStream = textureFile.OpenStream(false))
			{
				using (var imageContent = _content.ImageEditor.ImportContent(imageFileBrowser.Files[0].FullPath,
				                                                             fileStream,
				                                                             0,
				                                                             0,
				                                                             true,
				                                                             BufferFormat.R8G8B8A8_UIntNormal))
				{
					// We can only use 2D content.
					if (imageContent.Image.Settings.ImageType != ImageType.Image2D)
					{
						GorgonDialogs.ErrorBox(ParentForm, string.Format(Resources.GORFNT_IMAGE_NOT_2D, imageContent.Name));
						return null;
					}

					// If the size is mismatched with the font textures then ask the user if they wish to resize the 
					// texture.
					if ((imageContent.Image.Settings.Width != _content.Font.Settings.TextureSize.Width)
					    || (imageContent.Image.Settings.Height != _content.Font.Settings.TextureSize.Height))
					{
						ConfirmationResult result = GorgonDialogs.ConfirmBox(ParentForm,
						                                                     string.Format(
						                                                                   Resources.GORFNT_IMAGE_SIZE_MISMATCH_MSG,
						                                                                   imageContent.Image.Settings.Width,
						                                                                   imageContent.Image.Settings.Height,
						                                                                   _content.Font.Settings.TextureSize.Width,
						                                                                   _content.Font.Settings.TextureSize.Height),
						                                                     null,
						                                                     true);

						if (result == ConfirmationResult.Cancel)
						{
							return null;
						}

						transform = result == ConfirmationResult.Yes ? GlyphTextureTransform.Scaled : GlyphTextureTransform.Clipped;

						// Resize or clip the image.
						imageContent.Image.Resize(_content.Font.Settings.TextureSize.Width,
						                          _content.Font.Settings.TextureSize.Height,
						                          result == ConfirmationResult.No,
						                          ImageFilter.Point);
					}


					// Remove any array indices from the texture, we don't need them for font glyphs.
					var settings = (GorgonTexture2DSettings)imageContent.Image.Settings.Clone();
					settings.ArrayCount = 1;

					texture = _content.Graphics.Textures.CreateTexture<GorgonTexture2D>(imageContent.Name, imageContent.Image, settings);

					var dependency = new Dependency(textureFile.FullPath, GorgonFontContent.GlyphTextureType)
					                        {
						                        DependencyObject = texture
					                        };

					dependency.Properties[GorgonFontContent.GlyphTextureTransformProp] =
						new DependencyProperty(GorgonFontContent.GlyphTextureTransformProp, transform.ToString());

					if (transform != GlyphTextureTransform.None)
					{
						var converter = new SizeConverter();

						dependency.Properties[GorgonFontContent.GlyphTextureSizeProp] =
							new DependencyProperty(GorgonFontContent.GlyphTextureSizeProp, converter.ConvertToInvariantString(settings.Size));
					}

					_content.Dependencies[dependency.Path, GorgonFontContent.GlyphTextureType] = dependency;
				}
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
							  ZoomAmount = 4.0f,
							  ZoomWindowSize = new Vector2(256, 256),
                              BackgroundTexture = _pattern,
                              Position = cursorPosition
			              };
            
			if (_zoomFont == null)
			{
				_zoomFont = _content.Graphics.Fonts.CreateFont("MagnifierCaptionFont",
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

            UpdateMagnifierWindow();

			ValidateControls();
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

		        imageFileBrowser.FileExtensions.Clear();
		        foreach (var extension in _content.ImageEditor.FileExtensions)
		        {
		            imageFileBrowser.FileExtensions[extension.Extension] = extension;
		        }
		        imageFileBrowser.FileExtensions["*"] = new GorgonFileExtension("*", Resources.GORFNT_DLG_ALL_FILES);

		        imageFileBrowser.FileView = GorgonFontEditorPlugIn.Settings.LastTextureImportDialogView;
			    imageFileBrowser.DefaultExtension = GorgonFontEditorPlugIn.Settings.LastTextureExtension;

			    if (imageFileBrowser.ShowDialog(ParentForm) == DialogResult.OK)
			    {
				    GorgonTexture2D texture = null;
				    GorgonFileSystemFileEntry file = imageFileBrowser.Files[0];
				    Dependency dependency = _content.Dependencies.FirstOrDefault(item =>
				                                                                 item.DependencyObject == _selectedGlyph.Texture 
																				 && string.Equals(item.Path,
				                                                                               file.FullPath,
				                                                                               StringComparison.OrdinalIgnoreCase));

				    // This texture is already assigned to the glyph, skip the load procedure.
				    if (dependency != null)
				    {
					    menuItemSetGlyph.Enabled = true;
						menuItemSetGlyph.PerformClick();
					    return;
				    }

				    if (_content.Dependencies.Contains(file.FullPath, GorgonFontContent.GlyphTextureType))
				    {
					    texture = _content.Dependencies[file.FullPath, GorgonFontContent.GlyphTextureType].DependencyObject as GorgonTexture2D;
				    }

					// We have no pre-loaded texture, so go get it.
				    if (texture == null)
				    {
					    texture = LoadTexture(file);
				    }

					_newGlyph = _content.Font.Settings.Glyphs.FirstOrDefault(item => item.Character == _selectedGlyph.Character);

					while (_newGlyph != null)
					{
						RemoveGlyphTexture(_newGlyph);
						_content.Font.Settings.Glyphs.Remove(_newGlyph);

						_newGlyph = _content.Font.Settings.Glyphs.FirstOrDefault(item => item.Character == _selectedGlyph.Character);
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

					GorgonFontEditorPlugIn.Settings.LastTextureImportPath = file.Directory.FullPath;

					// Jump to the glyph clipper.
				    menuItemSetGlyph.Enabled = true;
					menuItemSetGlyph.PerformClick();
			    }

			    GorgonFontEditorPlugIn.Settings.LastTextureImportDialogView = imageFileBrowser.FileView;
			    GorgonFontEditorPlugIn.Settings.LastTextureExtension = imageFileBrowser.DefaultExtension;
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
				textPreviewText.Text = Resources.GORFNT_DEFAULT_TEXT;
			    return;
			}

			GorgonFontEditorPlugIn.Settings.SampleText = formattedText;

			formattedText = formattedText.Replace("\\n", "\n");
			formattedText = formattedText.Replace("\\t", "\t");

			_text.Text = formattedText;
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
			formValueComponentEditDialog componentEdit = null;

			try
			{
				componentEdit = new formValueComponentEditDialog
				                {
					                ValueComponents = 2,
					                DecimalPlaces = 0,
					                MaxValue = 8,
					                MinValue = -8,
					                Value1 = GorgonFontEditorPlugIn.Settings.ShadowOffset.X,
					                Value2 = GorgonFontEditorPlugIn.Settings.ShadowOffset.Y,
					                Text = Resources.GORFNT_DLG_SHADOW_OFFSET_TEXT
				                };
				componentEdit.ValueChanged += componentEdit_ValueChanged;

				if (componentEdit.ShowDialog() == DialogResult.OK)
				{
					_text.ShadowOffset = GorgonFontEditorPlugIn.Settings.ShadowOffset = new Point((int)componentEdit.Value1, (int)componentEdit.Value2);
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
				if (componentEdit != null)
				{
					componentEdit.ValueChanged -= componentEdit_ValueChanged;
					componentEdit.Dispose();
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
			var dialog = (formValueComponentEditDialog)sender;
						
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
			formAlphaPicker picker = null;

			try
			{
				picker = new formAlphaPicker
				         {
					         Text = Resources.GORFNT_DLG_SHADOW_ALPHA_TEXT,
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
					         Text = Resources.GORFNT_DLG_BACKGROUND_COLOR_TEXT,
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
					         Text = Resources.GORFNT_DLG_FOREGROUND_COLOR_TEXT,
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
				// If we can't use the image editor plug-in, then don't allow us to edit the glyph image dimensions.
				if (_currentTextureIndex < 0)
				{
					_currentTextureIndex = 0;
				}

				if (_currentTextureIndex >= _textures.Length)
				{
					_currentTextureIndex = _textures.Length - 1;
				}

	            buttonPrevTexture.Enabled = _currentTextureIndex > 0 &&
	                                        ((_content.CurrentState == DrawState.DrawFontTextures) ||
	                                         _content.CurrentState == DrawState.NextTexture ||
	                                         _content.CurrentState == DrawState.PrevTexture);
				buttonNextTexture.Enabled = _currentTextureIndex < _textures.Length - 1 && ((_content.CurrentState == DrawState.DrawFontTextures) ||
											 _content.CurrentState == DrawState.NextTexture ||
											 _content.CurrentState == DrawState.PrevTexture);

				dropDownZoom.Enabled = (_content.CurrentState == DrawState.DrawFontTextures);

				labelTextureCount.Text = string.Format("{0}: {1}/{2}", Resources.GORFNT_LABEL_TEXTURE_COUNT, _currentTextureIndex + 1, _textures.Length);

				panelToolbar.Visible = _content.CurrentState != DrawState.ClipGlyph;
				panelGlyphSet.Visible = _content.CurrentState == DrawState.ClipGlyph;

                // There is some strangeness when hiding the panels, the z-order looks to be getting messed up, this corrects that.
                panelGlyphSet.SendToBack();
                panelGlyphSet.BringToFront();
                panelInnerDisplay.SendToBack();
				panelInnerDisplay.BringToFront();

                buttonEditGlyph.Enabled = _selectedGlyph != null;

                if ((!buttonEditGlyph.Enabled) && (buttonEditGlyph.Checked))
                {
                    buttonEditGlyph.Checked = false;
                }

                buttonGlyphTools.Enabled =
                    buttonGlyphKern.Enabled =
                    buttonGlyphSizeSpace.Enabled = buttonEditGlyph.Enabled && _content.CurrentState == DrawState.GlyphEdit;


	            menuItemLoadGlyphImage.Visible =
		            menuItemSetGlyph.Visible =
		            menuItemRemoveGlyphImage.Visible = buttonGlyphTools.Visible = _content.ImageEditor != null;
				menuItemSetGlyph.Enabled = menuItemRemoveGlyphImage.Enabled = (_selectedGlyph != null) &&
                                                    (_selectedGlyph.IsExternalTexture) &&
                                                    (_content.CurrentState == DrawState.GlyphEdit);

                if ((_content.CurrentState == DrawState.ToGlyphEdit)
                    || (_content.CurrentState == DrawState.GlyphEdit)
                    || (_content.CurrentState == DrawState.ClipGlyph))
                {
                    buttonEditGlyph.Checked = true;
                    buttonEditGlyph.Text = Resources.GORFNT_BUTTON_END_EDIT_GLYPH;
                    buttonEditGlyph.Image = Resources.stop_16x16;

                    // Disable the edit button until we're done clipping.
                    if (_content.CurrentState == DrawState.ClipGlyph)
                    {
                        buttonEditGlyph.Enabled = false;
                    }
                }
                else
                {
                    buttonEditGlyph.Checked = false;
                    buttonEditGlyph.Text = Resources.GORFNT_BUTTON_EDIT_GLYPH;
                    buttonEditGlyph.Image = Resources.edit_16x16;
                }

				UpdateGlyphInfo();
            }
            else
            {
				buttonPrevTexture.Enabled = false;
				buttonNextTexture.Enabled = false;
                labelTextureCount.Text = string.Format("{0}: 0/0", Resources.GORFNT_LABEL_TEXTURE_COUNT);
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
                            string.Format("{0}: {1}x{2}  {3}: {4} (U+{5}), {6}: {7}x{8}-{9}x{10} ({11}: {12}, {13}: {14})",
                                          Resources.GORFNT_LABEL_MOUSE_POS,
                                          _mousePosition.X,
                                          _mousePosition.Y,
                                          Resources.GORFNT_LABEL_SELECTED_GLYPH,
                                          _selectedGlyph.Character,
                                          ((ushort)_selectedGlyph.Character).FormatHex(),
                                          Resources.GORFNT_LABEL_GLYPHREGION,
                                          _glyphClipper.ClipRegion.Left,
										  _glyphClipper.ClipRegion.Top,
										  _glyphClipper.ClipRegion.Right,
										  _glyphClipper.ClipRegion.Bottom,
                                          Resources.GORFNT_LABEL_GLYPHREGION_WIDTH,
										  _glyphClipper.ClipRegion.Width,
                                          Resources.GORFNT_LABEL_GLYPHREGION_HEIGHT,
										  _glyphClipper.ClipRegion.Height);
                        break;
                    case DrawState.DrawFontTextures:
		                labelSelectedGlyphInfo.Text = string.Format("{0}: {1} (U+{2})",
																    Resources.GORFNT_LABEL_SELECTED_GLYPH,
		                                                            _selectedGlyph.Character,
		                                                            ((ushort)_selectedGlyph.Character).FormatHex())
		                                                    .Replace("&", "&&");
                        break;
                    default:
					    labelSelectedGlyphInfo.Text = string.Format("{0}: {1} (U+{2}) {3}: {4}, {5} {6}: {7},{8} {9}: A={10}, B={11}, C={12}",
						    Resources.GORFNT_LABEL_SELECTED_GLYPH,
						    _selectedGlyph.Character,
						    ((ushort)_selectedGlyph.Character).FormatHex(),
						    Resources.GORFNT_LABEL_GLYPH_LOCATION,
						    _selectedGlyph.GlyphCoordinates.X,
						    _selectedGlyph.GlyphCoordinates.Y,
						    Resources.GORFNT_LABEL_GLYPH_SIZE,
						    _selectedGlyph.GlyphCoordinates.Width,
						    _selectedGlyph.GlyphCoordinates.Height,
						    Resources.GORFNT_LABEL_ADVANCE,
						    _selectedGlyph.Advance.X,
						    _selectedGlyph.Advance.Y,
						    _selectedGlyph.Advance.Z).Replace("&", "&&");
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

                labelHoverGlyphInfo.Text = string.Format("{0}: {1} (U+{2}) {3}: {4}, {5} {6}: {7},{8} {9}: A={10}, B={11}, C={12}",
					Resources.GORFNT_LABEL_HOVER_GLYPH,
                    _hoverGlyph.Character,
                    ((ushort)_hoverGlyph.Character).FormatHex(),
					Resources.GORFNT_LABEL_GLYPH_LOCATION,
                    _hoverGlyph.GlyphCoordinates.X,
                    _hoverGlyph.GlyphCoordinates.Y,
					Resources.GORFNT_LABEL_GLYPH_SIZE,
                    _hoverGlyph.GlyphCoordinates.Width,
                    _hoverGlyph.GlyphCoordinates.Height,
					Resources.GORFNT_LABEL_ADVANCE,
                    _hoverGlyph.Advance.X,
                    _hoverGlyph.Advance.Y,
					_hoverGlyph.Advance.Z).Replace("&", "&&");
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
		        _content.CurrentState = DrawState.PrevTexture;
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

				dropDownZoom.Text = string.Format("{0}: {1}", Resources.GORFNT_MENU_ZOOM, selectedItem.Text);

				if (menuItemToWindow.Checked)
				{
					CalculateZoomToWindow();
				}

				UpdateGlyphRegions();

                _sortedTextures = null;
				

				switch (_content.CurrentState)
				{
					case DrawState.DrawFontTextures:
						GorgonTexture2D texture = _textures[_currentTextureIndex];

						Size newSize = texture.Settings.Size;
						newSize.Width = (int)(newSize.Width * _currentZoom);
						newSize.Height = (int)(newSize.Height * _currentZoom);

						CheckForScroll(newSize);
						break;
					default:
						UpdateGlyphEditor();
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
			            break;
					case DrawState.ClipGlyph:
						// TODO: Do clipping stuff.
			            break;
					case DrawState.GlyphEdit:
	                    if (!_textureRegion.Contains(e.Location))
	                    {
	                        buttonEditGlyph.PerformClick();
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
		/// Function to calculate the current zoom factor when scaling to the size of the window.
		/// </summary>
	    private void CalculateZoomToWindow()
	    {
			// Get the current zoom level.
			if (!menuItemToWindow.Checked)
			{
				return;
			}

			GorgonTexture2D currentTexture = _textures[_currentTextureIndex];

			try
			{
				Vector2 zoomValue = currentTexture.Settings.Size;

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

			GorgonTexture2D currentTexture = _textures[_currentTextureIndex];

			// Recalculate 
			var glyphs = from GorgonGlyph fontGlyph in _content.Font.Glyphs
						 where fontGlyph.Texture == currentTexture && !char.IsWhiteSpace(fontGlyph.Character)
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

                _panelMidPoint = new Vector2(panelTextures.ClientSize.Width / 2.0f, panelTextures.ClientSize.Height / 2.0f);

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
						if ((_editGlyph != null) && (_content.CurrentState != DrawState.DrawFontTextures))
						{
							_content.CurrentState = DrawState.DrawFontTextures;
                            _nextState = DrawState.DrawFontTextures;
						}
					}
				}

				// Disable any animations for the font textures.
				_indexTransition = _currentTextureIndex;

				if (menuItemToWindow.Checked)
				{
					CalculateZoomToWindow();
				}

				// Turn on scrolling if we need it.
			    switch (_content.CurrentState)
			    {
					case DrawState.DrawFontTextures:
						GorgonTexture2D texture = _textures[_currentTextureIndex];

						var fontTextureSize = texture.Settings.Size;
						fontTextureSize.Width = (int)(fontTextureSize.Width * _currentZoom);
						fontTextureSize.Height = (int)(fontTextureSize.Height * _currentZoom);

						CheckForScroll(fontTextureSize);
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
					    break;
					case DrawState.FromGlyphEdit:
					case DrawState.NextTexture:
					case DrawState.PrevTexture:
					case DrawState.DrawFontTextures:
						if (_editGlyph != null)
						{
							_editGlyph.Dispose();
							_editGlyph = null;
						}
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
		/// Function to localize the text of the controls on the form.
		/// </summary>
		protected override void LocalizeControls()
		{
			Text = Resources.GORFNT_TITLE;
			buttonEditGlyph.Text = Resources.GORFNT_BUTTON_EDIT_GLYPH;
			buttonGlyphSizeSpace.Text = Resources.GORFNT_BUTTON_EDIT_ADVANCE_TIP;
			buttonGlyphKern.Text = Resources.GORFNT_BUTTON_EDIT_KERNING_TIP;
			buttonGlyphTools.ToolTipText = Resources.GORFNT_BUTTON_GLYPH_TOOLS;
			menuItemSetGlyph.Text = Resources.GORFNT_MENU_SET_GLYPH;
			menuItemLoadGlyphImage.Text = Resources.GORFNT_MENU_LOAD_GLYPH_IMAGE;
			menuTextColor.Text = Resources.GORFNT_MENU_DISPLAY_COLORS;
			itemSampleTextForeground.Text = Resources.GORFNT_MENU_FOREGROUND_COLOR;
			itemSampleTextBackground.Text = Resources.GORFNT_MENU_BACKGROUND_COLOR;
			menuShadow.Text = Resources.GORFNT_MENU_SHADOW_SETTINGS;
			itemPreviewShadowEnable.Text = Resources.GORFNT_MENU_ENABLE_SHADOW;
			itemShadowOpacity.Text = Resources.GORFNT_MENU_SHADOW_OPACITY;
			itemShadowOffset.Text = Resources.GORFNT_MENU_SHADOW_OFFSET;
			toolStripLabel1.Text = string.Format("{0}:", Resources.GORFNT_LABEL_PREVIEW_TEXT);
			labelTextureCount.Text = string.Format("{0}: 0/0", Resources.GORFNT_LABEL_TEXTURE_COUNT);
			dropDownZoom.Text = string.Format("{0}: {1}", Resources.GORFNT_MENU_ZOOM, Resources.GORFNT_MENU_TO_WINDOW);
			menuItemToWindow.Text = Resources.GORFNT_MENU_TO_WINDOW;
			menuItem1600.Text = 16.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem800.Text = 8.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem400.Text = 4.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem200.Text = 2.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem100.Text = 1.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem75.Text = 0.75.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem50.Text = 0.5.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem25.Text = 0.25.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
		}

		/// <summary>
		/// Function called when a property is changed on the related content.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="value">New value assigned to the property.</param>
		protected override void OnContentPropertyChanged(string propertyName, object value)
		{
			base.OnContentPropertyChanged(propertyName, value);

			// Refresh the font display.
			TextureDisplayResize(this, EventArgs.Empty);
		}

		/// <summary>
		/// Function called when the content has changed.
		/// </summary>
		public override void RefreshContent()
		{
			base.RefreshContent();

			_content = Content as GorgonFontContent;

            Debug.Assert(_content != null, "The content is not font content.");

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

			_text = _content.Renderer.Renderables.CreateText("Sample.Text", _content.Font, string.Empty);
			_text.Color = Color.Black;
			_text.WordWrap = true;
			_text.TextRectangle = new RectangleF(PointF.Empty, panelText.ClientSize);
			_text.ShadowOpacity = GorgonFontEditorPlugIn.Settings.ShadowOpacity;
			_text.ShadowOffset = GorgonFontEditorPlugIn.Settings.ShadowOffset;
			_text.ShadowEnabled = itemPreviewShadowEnable.Checked;

            _pattern = _content.Graphics.Textures.CreateTexture<GorgonTexture2D>("Background.Pattern", Resources.Pattern);

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

			textPreviewText.Text = GorgonFontEditorPlugIn.Settings.SampleText;
			textPreviewText.Select();
			textPreviewText.Select(0, textPreviewText.Text.Length);

			// Create the glyph sprite.
			_glyphSprite = _content.Renderer.Renderables.CreateSprite("GlyphSprite",
																	  new GorgonSpriteSettings
																	  {
																		  InitialPosition = Vector2.Zero,
																		  Size = new Vector2(1),
																		  TextureRegion = new RectangleF(0, 0, 1, 1)
																	  });
			_glyphSprite.BlendingMode = BlendingMode.Modulate;

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


			_editBackgroundAnimation = new GorgonAnimationController<GorgonSprite>();
			animation = _editBackgroundAnimation.Add("GlyphBGColor", animationTime);
			animation.Tracks["Color"].InterpolationMode = TrackInterpolationMode.Spline;
			animation.Tracks["Color"].KeyFrames.Add(new GorgonKeyGorgonColor(0.0f, new GorgonColor(0.125f, 0.125f, 0, 0)));
			animation.Tracks["Color"].KeyFrames.Add(new GorgonKeyGorgonColor(animation.Length, new GorgonColor(0.125f, 0.125f, 0.7f, 1.0f)));

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

			_text.Color = new GorgonColor(GorgonFontEditorPlugIn.Settings.TextColor);		

			panelText.AutoScrollMinSize = new Size((int)System.Math.Ceiling(_text.Size.X - (SystemInformation.VerticalScrollBarWidth * 1.5f)), (int)System.Math.Ceiling(_text.Size.Y));

			if (panelText.VerticalScroll.Visible)
			{
				textPosition.Y = panelText.AutoScrollPosition.Y;
			}

			_text.Position = textPosition;
			_text.Draw();
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

				if ((_content.CurrentState != DrawState.GlyphEdit)
				    && ((_content.CurrentState != DrawState.ToGlyphEdit)))
				{
					InitializeGlyphEditTransition(true);
				}
				else
				{
					InitializeGlyphEditTransition(false);
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
					buttonEditGlyph.Checked = true;
					InitializeGlyphEditTransition(true);
					return;
				}

				if ((_sortedTextures == null)
				    || (_textures.Length < 2))
				{
					return;
				}

				// Move to the font texture that we clicked on.
				foreach (GorgonSprite backSprite in _sortedTextures.Reverse())
				{
					var spriteSpace = new RectangleF(backSprite.Position - (Vector2.Modulate(backSprite.Anchor, backSprite.Scale)),
						                                backSprite.ScaledSize);

					if (!spriteSpace.Contains(e.Location))
					{
						continue;
					}

					int arrayIndex = Array.IndexOf(_textures, backSprite.Texture);

					if (arrayIndex == -1)
					{
						continue;
					}

					_sortedTextures = null;
					_content.CurrentState = arrayIndex < _currentTextureIndex ? DrawState.PrevTexture : DrawState.NextTexture;
                    _nextState = DrawState.DrawFontTextures;
					_currentTextureIndex = arrayIndex;
					UpdateGlyphRegions();

					return;
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
		/// Function to create the glyph texture.
		/// </summary>
		/// <param name="currentTexture">The currently selected texture.</param>
		/// <param name="textureSize">Size of the glyph texture.</param>
		/// <param name="glyphRegion">The glyph region on the source texture.</param>
	    private void CreateGlyphTexture(GorgonTexture2D currentTexture, Size textureSize, Rectangle glyphRegion)
	    {
			_editGlyph = _content.Graphics.Output.CreateRenderTarget("Glyph",
																	 new GorgonRenderTarget2DSettings
																	 {
																		 Width = textureSize.Width,
																		 Height = textureSize.Height,
																		 Format = BufferFormat.R8G8B8A8_UIntNormal
																	 });

			_content.Renderer.Target = _editGlyph;
			_content.Renderer.Drawing.SmoothingMode = SmoothingMode.None;
			_content.Renderer.Clear(GorgonColor.Transparent);
			_content.Renderer.Drawing.Blit(currentTexture,
										   new RectangleF(Vector2.Zero, textureSize),
										   new RectangleF(currentTexture.ToTexel(glyphRegion.Location),
														  currentTexture.ToTexel(glyphRegion.Size)));

			_content.Renderer.Target = null;
	    }

		/// <summary>
		/// Function to update the selected texture region.
		/// </summary>
		/// <param name="currentTexture">The currently selected texture.</param>
	    private void UpdateTextureRegion(GorgonTexture2D currentTexture)
		{
		    var textureMidpoint = new Vector2((currentTexture.Settings.Width * _currentZoom / 2.0f),
		                                      (currentTexture.Settings.Height * _currentZoom) / 2.0f);

		    Vector2.Subtract(ref _panelMidPoint, ref textureMidpoint, out _textureOffset);

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
			GorgonTexture2D currentTexture = _selectedGlyph.Texture;

			CheckForScroll(Size.Empty);

			UpdateTextureRegion(currentTexture);

			var textureRegion = GetGlyphEditRegion();
		    var glyphRegion = _selectedGlyph.GlyphCoordinates;

		    if (_editGlyph != null)
		    {
			    _editGlyph.Dispose();
			    _editGlyph = null;
		    }

		    CreateGlyphTexture(currentTexture, textureRegion.Size, glyphRegion);
		    _glyphSprite.Texture = _editGlyph;
		    if (_editGlyphAnimation.CurrentAnimation == null)
		    {
			    _glyphSprite.ScaledSize = Size.Round(textureRegion.Size);
			    _glyphBackgroundSprite.Size = panelTextures.ClientSize;
			    _glyphBackgroundSprite.TextureRegion = new RectangleF(Vector2.Zero, _pattern.ToTexel(panelTextures.ClientSize));
		    }
		    _content.CurrentState = DrawState.GlyphEdit;
            _nextState = DrawState.FromGlyphEdit;
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
				_panelMidPoint = new Vector2(clientSize.Width * 0.5f, clientSize.Height * 0.5f);
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
		/// <returns>The rectangle containing the glyph.</returns>
	    private Rectangle GetGlyphEditRegion()
	    {
			var glyphRegion = _selectedGlyph.GlyphCoordinates;
			var panelSize = new Vector2(panelTextures.Size);

			float scale = (panelSize.X > panelSize.Y
							  ? panelSize.Y / glyphRegion.Height
							  : panelSize.X / glyphRegion.Width);

			var textureRegion = new Rectangle((int)_textureRegion.X,
			                                  (int)_textureRegion.Y,
			                                  (int)(glyphRegion.Width * scale).Min(panelSize.X),
											  (int)(glyphRegion.Height * scale).Min(panelSize.Y));

			textureRegion.Y = (int)(_panelMidPoint.Y - (textureRegion.Height * 0.5f));

			textureRegion.X = (int)(_panelMidPoint.X - (textureRegion.Width * 0.5f));

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
				sprite.Position = new Vector2(_panelMidPoint.X + delta * _content.FontTextureSize.Width * 2.0f * _currentZoom,
											  _panelMidPoint.Y);

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
		/// Function to initialize the editor transition.
		/// </summary>
		/// <param name="editOn">TRUE when editing a glyph, FALSE when exiting the editor.</param>
	    private void InitializeGlyphEditTransition(bool editOn)
	    {
			// Move back to the selected texture index.
			if (_textures[_currentTextureIndex] != _selectedGlyph.Texture)
			{
				_currentTextureIndex = Array.IndexOf(_textures, _selectedGlyph.Texture);
				UpdateGlyphRegions();
			}

			GorgonTexture2D currentTexture = _selectedGlyph.Texture;
			var glyphRegion = _selectedGlyph.GlyphCoordinates;
			var textureRegion = GetGlyphEditRegion();
			
			var glyphScreenRegion = _glyphRegions[_selectedGlyph];

		    if (!editOn)
		    {
				// When we return from the editor screen, restore the scrollbars.
			    Size newSize = currentTexture.Settings.Size;

			    newSize.Width = (int)(newSize.Width * _currentZoom);
				newSize.Height = (int)(newSize.Height * _currentZoom);

				CheckForScroll(newSize);

				scrollHorizontal.Value = _lastScrollPoint.X.Max(0).Min(scrollHorizontal.Maximum);
				scrollVertical.Value = _lastScrollPoint.Y.Max(0).Min(scrollVertical.Maximum);

				// The panel may have been resized, we'll need to update our region.
			    if ((panelVertScroll.Visible)
			        || (panelHorzScroll.Visible))
			    {
				    UpdateTextureRegion(currentTexture);
					textureRegion = GetGlyphEditRegion();
			    }
		    }

			glyphScreenRegion.X += _textureRegion.X;
			glyphScreenRegion.Y += _textureRegion.Y;

		    if ((_editGlyph != null) && (editOn))
			{
				_editGlyph.Dispose();
				_editGlyph = null;
			}

			// Create the target.
			if (_editGlyph == null)
			{
				CreateGlyphTexture(currentTexture, textureRegion.Size, glyphRegion);
			}

			_editBackgroundAnimation.Stop();
			_editGlyphAnimation.Stop();

			var transitionAnimation = _editGlyphAnimation["TransitionTo"];

		    transitionAnimation.Tracks["ScaledSize"].KeyFrames[0] = new GorgonKeyVector2(0.0f, glyphScreenRegion.Size);
			transitionAnimation.Tracks["Position"].KeyFrames[0] = new GorgonKeyVector2(0.0f, glyphScreenRegion.Location);
			transitionAnimation.Tracks["ScaledSize"].KeyFrames[1] = new GorgonKeyVector2(transitionAnimation.Length,
			                                                                                textureRegion.Size);
			transitionAnimation.Tracks["Position"].KeyFrames[1] = new GorgonKeyVector2(transitionAnimation.Length,
			                                                                            textureRegion.Location);

		    _glyphSprite.Texture = _editGlyph;

		    _glyphBackgroundSprite.Scale = new Vector2(1);
		    _glyphBackgroundSprite.Position = Vector2.Zero;
			_glyphBackgroundSprite.Size = panelTextures.ClientSize;
		    _glyphBackgroundSprite.TextureRegion = new RectangleF(Vector2.Zero, _pattern.ToTexel(panelTextures.ClientSize));

			if (editOn)
			{
				_editBackgroundAnimation[0].Speed = 1.0f;
				transitionAnimation.Speed = 1.0f;

				buttonEditGlyph.Text = Resources.GORFNT_BUTTON_END_EDIT_GLYPH;
				buttonEditGlyph.Image = Resources.stop_16x16;
			}
			else
			{
				_editBackgroundAnimation[0].Speed = -1.0f;
				transitionAnimation.Speed = -1.0f;
				buttonEditGlyph.Text = Resources.GORFNT_BUTTON_EDIT_GLYPH;
				buttonEditGlyph.Image = Resources.edit_16x16;
			}

		    _editGlyphAnimation.Play(_glyphSprite, "TransitionTo");
			_editBackgroundAnimation.Play(_glyphBackgroundSprite, "GlyphBGColor");
			_content.CurrentState = editOn ? DrawState.ToGlyphEdit : DrawState.FromGlyphEdit;
            _nextState = DrawState.GlyphEdit;
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
			
			if (_content.CurrentState == DrawState.ToGlyphEdit)
			{
				_lastScrollPoint.X = scrollHorizontal.Value;
				_lastScrollPoint.Y = scrollVertical.Value;

				CheckForScroll(Size.Empty);
			}

			_content.CurrentState = _content.CurrentState == DrawState.ToGlyphEdit
				                        ? DrawState.GlyphEdit
				                        : DrawState.DrawFontTextures;
			_nextState = _content.CurrentState;
			ValidateControls();
		}

		/// <summary>
		/// Function to draw the glyph editor screen.
		/// </summary>
	    public void DrawGlyphEdit()
		{
			DrawFontTexture();

			Rectangle region = GetGlyphEditRegion();
			
			_glyphSprite.Position = region.Location;
			_glyphSprite.ScaledSize = region.Size;
			_glyphBackgroundSprite.Position = Vector2.Zero;
			_glyphBackgroundSprite.ScaledSize = panelTextures.ClientSize;
			_glyphBackgroundSprite.TextureRegion = new RectangleF(Vector2.Zero, _pattern.ToTexel(panelTextures.ClientSize));

			_glyphBackgroundSprite.Draw();
			// TODO: Make an option to show or hide the alpha.
			_glyphSprite.Draw();
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
                sprite.Position = _panelMidPoint;
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

			MouseWheel += PanelDisplay_MouseWheel;
	    }
        #endregion
	}
}
