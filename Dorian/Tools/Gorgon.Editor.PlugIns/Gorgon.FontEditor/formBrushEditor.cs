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
// Created: Wednesday, November 13, 2013 8:58:39 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fetze.WinFormsColor;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.FontEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using GorgonLibrary.Renderers;
using GorgonLibrary.UI;
using SlimMath;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
    /// <summary>
    /// An editor that will create a brush for a font.
    /// </summary>
    partial class formBrushEditor 
        : ZuneForm
	{
		#region Classes.
		/// <summary>
		/// A wrap mode combo box item.
		/// </summary>
		private class WrapModeComboItem
		{
			#region Variables.
			private readonly string _text = string.Empty;			// Item text.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the wrap mode.
			/// </summary>
			public WrapMode WrapMode
			{
				get;
				private set;
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Returns a <see cref="System.String" /> that represents this instance.
			/// </summary>
			/// <returns>
			/// A <see cref="System.String" /> that represents this instance.
			/// </returns>
			public override string ToString()
			{
				return _text;
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="WrapModeComboItem"/> class.
			/// </summary>
			/// <param name="wrapMode">Type of the brush.</param>
			/// <param name="text">The text.</param>
			public WrapModeComboItem(WrapMode wrapMode, string text)
			{
				WrapMode = wrapMode;
				_text = text;
			}
			#endregion
		}

		/// <summary>
		/// A brush type combo box item.
		/// </summary>
	    private class BrushTypeComboItem
	    {
			#region Variables.
			private readonly string _text = string.Empty;			// Item text.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the brush type.
			/// </summary>
			public GlyphBrushType BrushType
			{
				get;
				private set;
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Returns a <see cref="System.String" /> that represents this instance.
			/// </summary>
			/// <returns>
			/// A <see cref="System.String" /> that represents this instance.
			/// </returns>
			public override string ToString()
			{
				return _text;
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="BrushTypeComboItem"/> class.
			/// </summary>
			/// <param name="brushType">Type of the brush.</param>
			/// <param name="text">The text.</param>
			public BrushTypeComboItem(GlyphBrushType brushType, string text)
			{
				BrushType = brushType;
				_text = text;
			}
			#endregion
	    }

		/// <summary>
		/// A value type for the corner nodes.
		/// </summary>
	    private class CornerNode
	    {
			#region Variables.
			private Rectangle _nodeBounds;				// Bounding area for the node.
			private Point _nodePosition;				// Position for the node.
			private GorgonColor _nodeColor;				// Color of the node.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to set or return the cursor associated with this node point.
			/// </summary>
			public Cursor NodeCursor
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the coordinates for the node.
			/// </summary>
			public Point NodeCoordinates
			{
				get
				{
					return _nodePosition;
				}
				set
				{
					_nodePosition = value;

					if (value == Point.Empty)
					{
						_nodeBounds = Rectangle.Empty;
						return;
					}

					_nodeBounds = new Rectangle(value.X - 4, value.Y - 4, 8, 8);
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to perform a hit test on the node.
			/// </summary>
			/// <param name="cursorPosition">Position of the mouse cursor.</param>
			public void HitTest(Point cursorPosition)
			{
				_nodeColor = _nodeBounds.Contains(cursorPosition) ? new GorgonColor(1, 0, 0, 0.5f) : new GorgonColor(0, 1, 1, 0.5f);
			}

			/// <summary>
			/// Function to render the node point.
			/// </summary>
			/// <param name="renderer">Renderer used to render the node point.</param>
			public void Draw(Gorgon2D renderer)
			{
				if (_nodePosition == Point.Empty)
				{
					return;
				}

				renderer.Drawing.FilledEllipse(_nodeBounds, _nodeColor);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="CornerNode"/> class.
			/// </summary>
			/// <param name="cursor">The cursor.</param>
			public CornerNode(Cursor cursor)
			{
				NodeCursor = cursor;
				NodeCoordinates = Point.Empty;
				_nodeColor = new GorgonColor(0, 1, 1, 0.5f);
			}
			#endregion
	    }

		/// <summary>
		/// Selection area interface.
		/// </summary>
	    private class SelectionArea
	    {
			#region Variables.
			private readonly GorgonSprite _selectionSprite;						// Selection sprite.
			private Vector2 _textureSize;										// Size of the texture that is being selected.
			private RectangleF _drawRegion = RectangleF.Empty;					// Region to draw.
			private RectangleF _selectionRegion = RectangleF.Empty;				// Selection area.
			private RectangleF _textureRegion = RectangleF.Empty;				// The screen space texture area.
			private Vector2 _dragOffset = Vector2.Zero;							// Offset for dragging.
			private Vector2 _imageScale = Vector2.Zero;							// Image scaling.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return whether we're in the middle of a drag operation or not.
			/// </summary>
			public bool IsDragging
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to set or return the region for selection.
			/// </summary>
			public RectangleF SelectionRegion
			{
				get
				{
					return _selectionRegion;
				}
				set
				{
					_selectionRegion = value;
					UpdateDrawRegion();
				}
			}

			/// <summary>
			/// Property to set or return the region for the texture on the screen.
			/// </summary>
			public RectangleF TextureRegion
			{
				get
				{
					return _textureRegion;
				}
				set
				{
					_textureRegion = value;
					UpdateDrawRegion();
				}
			}

			/// <summary>
			/// Property to set or return the size of the texture.
			/// </summary>
			public Vector2 TextureSize
			{
				get
				{
					return _textureSize;
				}
				set
				{
					_textureSize = value;
					UpdateDrawRegion();
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to update the drawing region for the selection rectangle.
			/// </summary>
			private void UpdateDrawRegion()
			{
				if ((_textureSize.X.EqualsEpsilon(0))
					|| (_textureSize.Y.EqualsEpsilon(0)))
				{
					return;
				}

				_imageScale = new Vector2(TextureRegion.Width / _textureSize.X, TextureRegion.Height / _textureSize.Y);

				_drawRegion = new RectangleF((SelectionRegion.X * _imageScale.X) + TextureRegion.X,
											 (SelectionRegion.Y * _imageScale.Y) + TextureRegion.Y,
											 SelectionRegion.Width * _imageScale.X,
											 SelectionRegion.Height * _imageScale.Y);

				_drawRegion = RectangleF.Intersect(_textureRegion, _drawRegion);
			}

			/// <summary>
			/// Function to determine if the mouse is within the bounds of the selection area.
			/// </summary>
			/// <param name="mousePosition">Mouse cursor position.</param>
			/// <returns>TRUE if the cursor is in the hit test area, FALSE if not.</returns>
			public bool HitTest(Point mousePosition)
			{
				return _drawRegion.Contains(mousePosition);
			}

			/// <summary>
			/// Function to begin a dragging operation on the main selection region.
			/// </summary>
			/// <param name="mousePosition">Mouse position.</param>
			public void StartDrag(Point mousePosition)
			{
				IsDragging = true;
				_dragOffset = new Vector2(_drawRegion.X - mousePosition.X, _drawRegion.Y - mousePosition.Y);
			}

			/// <summary>
			/// Function to end a drag operation on the selection area.
			/// </summary>
			public void EndDrag()
			{
				IsDragging = false;
			}

			/// <summary>
			/// Function called when the mouse moves.
			/// </summary>
			/// <param name="mousePosition">Position of the mouse cursor.</param>
			/// <param name="buttons">Currently pressed mouse buttons.</param>
			public void MouseMove(Point mousePosition, MouseButtons buttons)
			{
				if ((buttons == MouseButtons.Left) && (IsDragging))
				{
					_selectionRegion = new RectangleF(((mousePosition.X + _dragOffset.X) / _imageScale.X).FastFloor(),
					                                 ((mousePosition.Y + _dragOffset.Y) / _imageScale.Y).FastFloor(),
					                                 _selectionRegion.Width.FastFloor(),
					                                 _selectionRegion.Height.FastFloor());

					// Constrain selection area.
					if (_selectionRegion.X < 0)
					{
						_selectionRegion.X = 0;
					}

					if (_selectionRegion.Y < 0)
					{
						_selectionRegion.Y = 0;
					}

					if (_selectionRegion.Right > _textureSize.X)
					{
						_selectionRegion = RectangleF.FromLTRB(_textureSize.X - _selectionRegion.Width,
						                                  _selectionRegion.Y,
						                                  _textureSize.X,
						                                  _selectionRegion.Bottom);
					}

					if (_selectionRegion.Bottom > _textureSize.Y)
					{
						_selectionRegion = RectangleF.FromLTRB(_selectionRegion.X,
						                                  _textureSize.Y - _selectionRegion.Height,
						                                  _selectionRegion.Right,
						                                  _textureSize.Y);
					}

					UpdateDrawRegion();
				}
			}

			/// <summary>
			/// Function to draw the selection area.
			/// </summary>
			public void Draw()
			{
				RectangleF region = _drawRegion;

				// Animate the sprite texture.
				Vector2 textureOffset = _selectionSprite.TextureOffset;
				textureOffset.X += 0.25f * GorgonTiming.Delta;
				textureOffset.Y += 0.25f * GorgonTiming.Delta;

				if (textureOffset.X > 1)
				{
					textureOffset.X = textureOffset.X - 1.0f;
				}

				if (textureOffset.Y > 1)
				{
					textureOffset.Y = textureOffset.Y - 1.0f;
				}

				region = RectangleF.Intersect(region, _textureRegion);

				_selectionSprite.Size = region.Size;
				_selectionSprite.Position = region.Location;
				_selectionSprite.TextureRegion = new RectangleF(textureOffset,
																new Vector2(
																	_drawRegion.Width / _selectionSprite.Texture.Settings.Width,
																	_drawRegion.Height / _selectionSprite.Texture.Settings.Height));

				_selectionSprite.Draw();
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="SelectionArea"/> class.
			/// </summary>
			/// <param name="renderer">The renderer used to draw the selection area.</param>
			/// <param name="selectionTexture">The selection texture.</param>
			public SelectionArea(Gorgon2D renderer, GorgonTexture2D selectionTexture)
			{
				_selectionSprite = renderer.Renderables.CreateSprite("SelectionSprite",
				                                                      new GorgonSpriteSettings
				                                                      {
					                                                      Texture = selectionTexture,
					                                                      Color = new GorgonColor(0, 0, 1, 0.4f),
					                                                      Size = new Vector2(32, 32),
					                                                      TextureRegion = new RectangleF(0, 0, 1, 1)
				                                                      });

				_selectionSprite.TextureSampler.HorizontalWrapping = TextureAddressing.Wrap;
				_selectionSprite.TextureSampler.VerticalWrapping = TextureAddressing.Wrap;
			}
			#endregion
	    }
		#endregion

		#region Variables.
		private readonly GorgonFontContent _currentContent;					// Current content object.
	    private readonly GorgonGraphics _graphics;							// The graphics interface.
	    private GorgonSwapChain _swapChain;									// The swap chain to use for displaying images.
	    private readonly Gorgon2D _renderer;								// Renderer for interface.
	    private GorgonGlyphTextureBrush _textureBrush;						// The current texture brush.
	    private GorgonTexture2D _defaultTexture;							// Default texture.
	    private readonly Func<bool> _previousIdle;							// Previously active idle function.
	    private Gorgon2DStateRecall _lastState;								// Last 2D state.
	    private GorgonTexture2D _originalTexture;							// Original brush texture.
	    private GorgonTexture2D _texture;									// Current texture assigned to the brush.
	    private RectangleF _textureRegion = new RectangleF(0, 0, 1, 1);		// Current texture region.
	    private WrapMode _textureWrapMode = WrapMode.Tile;					// Current wrap mode.
		private GlyphBrushType _originalBrushType = GlyphBrushType.Solid;	// Original brush type.
	    private SelectionArea _selectionArea;								// Selection area.
	    private RectangleF _imageRegion = RectangleF.Empty;					// Image region.
        #endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the current brush type.
		/// </summary>
	    public GlyphBrushType BrushType
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the current solid brush.
		/// </summary>
	    public GorgonGlyphSolidBrush SolidBrush
	    {
			get;
			set;
	    }

		/// <summary>
		/// Property to set or return the current hatch pattern brush.
		/// </summary>
	    public GorgonGlyphHatchBrush PatternBrush
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the current gradient brush.
		/// </summary>
	    public GorgonGlyphLinearGradientBrush GradientBrush
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the current texture brush.
		/// </summary>
	    public GorgonGlyphTextureBrush TextureBrush
	    {
			get
			{
				return _textureBrush;
			}
			set
			{
				if (value == null)
				{
					return;
				}

				_textureBrush = value;
				_texture = _originalTexture = value.Texture;

				_textureRegion = _textureBrush.TextureRegion == null
					                 ? new RectangleF(0, 0, _texture.Settings.Width, _texture.Settings.Height)
					                 : _texture.ToPixel(_textureBrush.TextureRegion.Value);

				_textureWrapMode = _textureBrush.WrapMode;
			}
	    }
        #endregion

        #region Methods.
		/// <summary>
		/// Handles the MouseDown event of the panelTextureDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelTextureDisplay_MouseDown(object sender, MouseEventArgs e)
		{
			if (_selectionArea == null)
			{
				return;
			}

			if (!_selectionArea.HitTest(e.Location))
			{
				return;
			}

			if (e.Button != MouseButtons.Left)
			{
				return;
			}

			numericX.ValueChanged -= numericX_ValueChanged;
			numericY.ValueChanged -= numericX_ValueChanged;
			numericWidth.ValueChanged -= numericX_ValueChanged;
			numericHeight.ValueChanged -= numericX_ValueChanged;

			_selectionArea.StartDrag(e.Location);
		}

		/// <summary>
		/// Handles the MouseMove event of the panelTextureDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelTextureDisplay_MouseMove(object sender, MouseEventArgs e)
		{
			if (_selectionArea == null)
			{
				return;
			}

			_selectionArea.MouseMove(e.Location, e.Button);

			if (!_selectionArea.IsDragging)
			{
				return;
			}

			Rectangle numericValues =
				Rectangle.Round(RectangleF.Intersect(new RectangleF(0, 0, _texture.Settings.Width, _texture.Settings.Height),
				                                     _selectionArea.SelectionRegion));

			numericX.Value = numericValues.X;
			numericY.Value = numericValues.Y;
		}

		/// <summary>
		/// Handles the MouseUp event of the panelTextureDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelTextureDisplay_MouseUp(object sender, MouseEventArgs e)
		{
			if (_selectionArea == null)
			{
				return;
			}

			if (e.Button != MouseButtons.Left)
			{
				return;
			}

			_selectionArea.EndDrag();

			numericX.ValueChanged += numericX_ValueChanged;
			numericY.ValueChanged += numericX_ValueChanged;
			numericWidth.ValueChanged += numericX_ValueChanged;
			numericHeight.ValueChanged += numericX_ValueChanged;

			ValidateCommands();
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboWrapMode control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void comboWrapMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			ValidateCommands();
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericX control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericX_ValueChanged(object sender, EventArgs e)
		{
			UpdateSelectionArea();
			ValidateCommands();
		}

		/// <summary>
		/// Function to validate the command states.
		/// </summary>
	    private void ValidateCommands()
	    {
			switch (BrushType)
			{
				case GlyphBrushType.Solid:
					buttonOK.Enabled = ((colorSolidBrush.SelectedColor != colorSolidBrush.OldColor)
					                    || (BrushType != _originalBrushType));
					break;
				case GlyphBrushType.Texture:
					var wrapItem = (WrapModeComboItem)comboWrapMode.SelectedItem;

					buttonOK.Enabled = ((_selectionArea != null) && (_texture != null) && ((_texture != _originalTexture)
					                                           || (_selectionArea.SelectionRegion != _textureRegion)
					                                           || (wrapItem.WrapMode != _textureWrapMode)
					                                           || (BrushType != _originalBrushType)));

					numericHeight.Enabled =
						numericWidth.Enabled = numericX.Enabled = numericY.Enabled = comboWrapMode.Enabled = _texture != null;

					// Update the numeric controls to limit based on the current position and size.
					if (_texture != null)
					{
						numericWidth.Maximum = _texture.Settings.Width - numericX.Value;
						numericHeight.Maximum = _texture.Settings.Height - numericY.Value;
						numericX.Maximum = _texture.Settings.Width - numericWidth.Value;
						numericY.Maximum = _texture.Settings.Height - numericHeight.Value;
					}
					break;
			}
	    }

		/// <summary>
		/// Function to update the selection area.
		/// </summary>
	    private void UpdateSelectionArea()
	    {
		    if (_selectionArea == null)
		    {
			    return;
		    }

			_selectionArea.SelectionRegion = new RectangleF((float)numericX.Value,
															(float)numericY.Value,
															(float)numericWidth.Value,
															(float)numericHeight.Value);
		}

		/// <summary>
		/// Function to retrieve the image transformation values and node points.
		/// </summary>
	    private void GetImageValues()
		{
			float imageRatio;
			Vector2 imagePosition;
			Vector2 imageSize = panelTextureDisplay.Size;

			if (_texture.Settings.Width > _texture.Settings.Height)
			{
				imageRatio = (float)_texture.Settings.Height / _texture.Settings.Width;
				imageSize.Y *= imageRatio;
				imagePosition = new Vector2(0, panelTextureDisplay.Height / 2.0f - imageSize.Y / 2.0f);
			}
			else
			{
				imageRatio = (float)_texture.Settings.Width / _texture.Settings.Height;
				imageSize.X *= imageRatio;
				imagePosition = new Vector2(panelTextureDisplay.Width / 2.0f - imageSize.X / 2.0f, 0);
			}

			_imageRegion = new RectangleF(imagePosition, imageSize);

			_selectionArea.TextureSize = new Vector2(_texture.Settings.Width, _texture.Settings.Height);
			_selectionArea.TextureRegion = _imageRegion;
			UpdateSelectionArea();
		}

		/// <summary>
		/// Handles the Click event of the buttonOpen control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonOpen_Click(object sender, EventArgs e)
		{
			try
			{
				imageFileBrowser.FileExtensions.Clear();
				foreach (var extension in _currentContent.ImageEditor.FileExtensions)
				{
					imageFileBrowser.FileExtensions[extension.Extension] = extension;
				}

				imageFileBrowser.FileExtensions["*"] = new GorgonFileExtension("*", Resources.GORFNT_DLG_ALL_FILES);

				imageFileBrowser.FileView = GorgonFontEditorPlugIn.Settings.LastTextureImportDialogView;
				imageFileBrowser.DefaultExtension = GorgonFontEditorPlugIn.Settings.LastTextureExtension;

				if (imageFileBrowser.ShowDialog(ParentForm) == DialogResult.OK)
				{
					Cursor.Current = Cursors.WaitCursor;

					// Load the image.
					using (IImageEditorContent imageContent = _currentContent.ImageEditor.ImportContent(imageFileBrowser.Files[0],
					                                                                                    0,
					                                                                                    0,
					                                                                                    false,
					                                                                                    BufferFormat
						                                                                                    .R8G8B8A8_UIntNormal))
					{
						if (imageContent.Image.Settings.ImageType != ImageType.Image2D)
						{
							GorgonDialogs.ErrorBox(ParentForm, string.Format(Resources.GORFNT_IMAGE_NOT_2D, imageContent.Name));
							return;
						}

						var settings = (GorgonTexture2DSettings)imageContent.Image.Settings.Clone();
						_texture = _graphics.Textures.CreateTexture<GorgonTexture2D>(imageContent.Name,
						                                                             imageContent.Image,
						                                                             settings);
						// Reset the texture brush settings.
						numericWidth.Maximum = settings.Width;
						numericWidth.Value = settings.Width;
						numericHeight.Maximum = settings.Height;
						numericHeight.Value = settings.Height;
						numericX.Maximum = settings.Width - 1;
						numericX.Value = 0;
						numericY.Maximum = settings.Height - 1;
						numericY.Value = 0;
						comboWrapMode.Text = Resources.GORFNT_PROP_VALUE_TILE;

						// Function to retrieve the transformation and node point values from the image.
						GetImageValues();
					}
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
				Cursor.Current = Cursors.Default;
				ValidateCommands();
			}
		}

		/// <summary>
        /// Handles the Click event of the buttonOK control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            switch (BrushType)
            {
                case GlyphBrushType.Solid:
		            SolidBrush = new GorgonGlyphSolidBrush
		                         {
			                         Color = colorSolidBrush.SelectedColor
		                         };
                    break;
				case GlyphBrushType.Texture:
		            var item = (WrapModeComboItem)comboWrapMode.SelectedItem;
		            var region = new RectangleF((float)numericX.Value,
		                                        (float)numericY.Value,
		                                        (float)numericWidth.Value,
		                                        (float)numericHeight.Value);

		            _originalTexture = _texture;
		            TextureBrush = new GorgonGlyphTextureBrush(_texture)
		                           {
			                           WrapMode = item.WrapMode,
			                           TextureRegion = _texture.ToTexel(region)
		                           };
		            break;
            }
        }

        /// <summary>
        /// Handles the ColorChanged event of the colorSolidBrush control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void colorSolidBrush_ColorChanged(object sender, EventArgs e)
        {
			ValidateCommands();
        }

		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboBrushType control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void comboBrushType_SelectedIndexChanged(object sender, EventArgs e)
		{
			var item = comboBrushType.SelectedItem as BrushTypeComboItem;

			if (item == null)
			{
				return;
			}

			BrushType = item.BrushType;
			switch (item.BrushType)
			{
				case GlyphBrushType.Solid:
					tabBrushEditor.SelectedTab = pageSolid;
					break;
				case GlyphBrushType.Texture:
					tabBrushEditor.SelectedTab = pageTexture;
					break;
			}

			ValidateCommands();
		}

        /// <summary>
		/// Function to localize the form.
		/// </summary>
	    private void Localize()
	    {
		    comboBrushType.Items.Clear();
			comboBrushType.Items.Add(new BrushTypeComboItem(GlyphBrushType.Solid, Resources.GORFNT_PROP_VALUE_SOLID_BRUSH));
			comboBrushType.Items.Add(new BrushTypeComboItem(GlyphBrushType.LinearGradient, Resources.GORFNT_PROP_VALUE_GRADIENT_BRUSH));
			comboBrushType.Items.Add(new BrushTypeComboItem(GlyphBrushType.Hatched, Resources.GORFNT_PROP_VALUE_PATTERN_BRUSH));
			comboBrushType.Items.Add(new BrushTypeComboItem(GlyphBrushType.Texture, Resources.GORFNT_PROP_VALUE_TEXTURE_BRUSH));

	        comboBrushType.SelectedItem = 0;

			comboWrapMode.Items.Clear();
			comboWrapMode.Items.Add(new WrapModeComboItem(WrapMode.Tile, Resources.GORFNT_PROP_VALUE_TILE));
			comboWrapMode.Items.Add(new WrapModeComboItem(WrapMode.Clamp, Resources.GORFNT_PROP_VALUE_CLAMP));
			comboWrapMode.Items.Add(new WrapModeComboItem(WrapMode.TileFlipX, Resources.GORFNT_PROP_VALUE_TILEFLIPX));
			comboWrapMode.Items.Add(new WrapModeComboItem(WrapMode.TileFlipY, Resources.GORFNT_PROP_VALUE_TILEFLIPY));
			comboWrapMode.Items.Add(new WrapModeComboItem(WrapMode.TileFlipXY, Resources.GORFNT_PROP_VALUE_TILEFLIPBOTH));

	        comboWrapMode.SelectedIndex = 0;
	    }

		/// <summary>
		/// Function to execute during idle time.
		/// </summary>
		/// <returns>TRUE to continue executing, FALSE to stop.</returns>
	    private bool Idle()
		{
			_renderer.Drawing.TextureSampler.HorizontalWrapping = TextureAddressing.Wrap;
			_renderer.Drawing.TextureSampler.VerticalWrapping = TextureAddressing.Wrap;

			_renderer.Drawing.Blit(_defaultTexture,
			                       new RectangleF(0, 0, panelTextureDisplay.Width, panelTextureDisplay.Height),
								   _defaultTexture.ToTexel(panelTextureDisplay.ClientRectangle));

			_renderer.Drawing.TextureSampler.HorizontalWrapping = TextureAddressing.Clamp;
			_renderer.Drawing.TextureSampler.VerticalWrapping = TextureAddressing.Clamp;

			if (_texture == null)
			{
				_renderer.Render(2);
				return true;
			}

			float imageRatio;
			Vector2 imagePosition;
			Vector2 imageSize = panelTextureDisplay.Size;

			if (_texture.Settings.Width > _texture.Settings.Height)
			{
				imageRatio = (float)_texture.Settings.Height / _texture.Settings.Width;
				imageSize.Y *= imageRatio;
				imagePosition = new Vector2(0, panelTextureDisplay.Height / 2.0f - imageSize.Y / 2.0f); 
			}
			else
			{
				imageRatio = (float)_texture.Settings.Width / _texture.Settings.Height;
				imageSize.X *= imageRatio;
				imagePosition = new Vector2(panelTextureDisplay.Width / 2.0f - imageSize.X / 2.0f, 0);
			}

			_renderer.Drawing.Blit(_texture,
								   new RectangleF(imagePosition.X, imagePosition.Y, imageSize.X, imageSize.Y),
								   new RectangleF(0, 0, 1, 1));

			_selectionArea.Draw();

			// Draw handles.
			/*var region = new RectangleF(_selectionSprite.Position, _selectionSprite.Size);

			_renderer.Drawing.FilledEllipse(new RectangleF(region.X - 4, region.Y - 4, 8, 8), new GorgonColor(0, 1, 1, 0.5f));
			_renderer.Drawing.FilledEllipse(new RectangleF(region.Right - 4, region.Y - 4, 8, 8), new GorgonColor(0, 1, 1, 0.5f));
			_renderer.Drawing.FilledEllipse(new RectangleF(region.Right - 4, region.Bottom - 4, 8, 8), new GorgonColor(0, 1, 1, 0.5f));
			_renderer.Drawing.FilledEllipse(new RectangleF(region.X - 4, region.Bottom - 4, 8, 8), new GorgonColor(0, 1, 1, 0.5f));*/


			_renderer.Render(2);
			return true;
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the tabBrushEditor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void tabBrushEditor_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (tabBrushEditor.SelectedTab == pageTexture)
				{
					if (_swapChain == null)
					{
						_swapChain = _graphics.Output.CreateSwapChain("ImageDisplay",
						                                              new GorgonSwapChainSettings
						                                              {
							                                              Window = panelTextureDisplay,
							                                              Format = BufferFormat.R8G8B8A8_UIntNormal
						                                              });

						_lastState = _renderer.Begin2D();

						_selectionArea = new SelectionArea(_renderer, _defaultTexture);
					}

					_renderer.Target = _swapChain;

					Gorgon.ApplicationIdleLoopMethod = Idle;
				}
				else
				{
					Gorgon.ApplicationIdleLoopMethod = null;
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateCommands();
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
	    protected override void OnFormClosing(FormClosingEventArgs e)
	    {
		    base.OnFormClosing(e);

			Gorgon.AllowBackground = false;
			Gorgon.ApplicationIdleLoopMethod = _previousIdle;

			if (_swapChain != null)
			{
				_renderer.End2D(_lastState);
				_swapChain.Dispose();
				_swapChain = null;
			}

			if (_defaultTexture != null)
			{
				_defaultTexture.Dispose();
				_defaultTexture = null;
			}

			// Only destroy the current texture if we have no intention of using it outside of here.
			if ((_texture == null)
				|| (_texture == _originalTexture))
			{
				return;
			}

			_texture.Dispose();
			_texture = null;
	    }


	    /// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	    protected override void OnLoad(EventArgs e)
	    {
		    base.OnLoad(e);

		    try
		    {
			    Localize();

			    // Force to the first tab page.
			    tabBrushEditor.SelectedTab = pageSolid;

			    // TODO: Make Primaryattribute static or put them into a configuration value.
			    colorSolidBrush.PrimaryAttribute = ColorPickerPanel.PrimaryAttrib.Red;
			    colorSolidBrush.OldColor = SolidBrush.Color;
			    colorSolidBrush.SelectedColor = SolidBrush.Color;

			    _originalBrushType = BrushType;

			    switch (BrushType)
			    {
				    case GlyphBrushType.LinearGradient:
					    comboBrushType.Text = Resources.GORFNT_PROP_VALUE_GRADIENT_BRUSH;
					    break;
				    case GlyphBrushType.Hatched:
					    comboBrushType.Text = Resources.GORFNT_PROP_VALUE_PATTERN_BRUSH;
					    break;
				    case GlyphBrushType.Texture:
					    comboBrushType.Text = Resources.GORFNT_PROP_VALUE_TEXTURE_BRUSH;
					    tabBrushEditor.SelectedTab = pageTexture;

					    WrapModeComboItem selectedItem = comboWrapMode.Items.Cast<WrapModeComboItem>()
					                                                  .FirstOrDefault(item =>
					                                                                  item.WrapMode == TextureBrush.WrapMode);

					    if (_texture != null)
					    {
						    numericWidth.Maximum = _texture.Settings.Width;
						    numericWidth.Value = (decimal)_textureRegion.Width;
						    numericHeight.Maximum = _texture.Settings.Height;
						    numericHeight.Value = (decimal)_textureRegion.Height;
						    numericX.Maximum = (decimal)_texture.Settings.Width - 1;
						    numericX.Value = (decimal)_textureRegion.X;
						    numericY.Maximum = (decimal)_texture.Settings.Height - 1;
						    numericY.Value = (decimal)_textureRegion.Y;

							GetImageValues();
					    }

					    if (selectedItem != null)
					    {
						    comboWrapMode.Text = selectedItem.ToString();
					    }
					    break;
				    default:
					    comboBrushType.Text = Resources.GORFNT_PROP_VALUE_SOLID_BRUSH;
					    break;
			    }
		    }
		    catch (Exception ex)
		    {
			    GorgonDialogs.ErrorBox(this, ex);
			    Close();
		    }
		    finally
		    {
			    ValidateCommands();
		    }
	    }
	    #endregion

        #region Constructor/Destructor.
	    /// <summary>
	    /// Initializes a new instance of the <see cref="formBrushEditor"/> class.
	    /// </summary>
	    public formBrushEditor()
	    {
		    InitializeComponent();
	    }

	    /// <summary>
        /// Initializes a new instance of the <see cref="formBrushEditor"/> class.
        /// </summary>
        /// <param name="fontContent">The current font content interface.</param>
        public formBrushEditor(GorgonFontContent fontContent)
			: this()
	    {
		    _currentContent = fontContent;
			_previousIdle = Gorgon.ApplicationIdleLoopMethod;
			Gorgon.ApplicationIdleLoopMethod = null;
		    Gorgon.AllowBackground = true;		// Enable background rendering because the application will appear to have lost focus when this dialog is present.
		    _graphics = fontContent.Graphics;
		    _renderer = fontContent.Renderer;
		    _defaultTexture = _graphics.Textures.CreateTexture<GorgonTexture2D>("DefaultPattern", Resources.Pattern);
			SolidBrush = new GorgonGlyphSolidBrush();
	        GradientBrush = new GorgonGlyphLinearGradientBrush
	                        {
		                        Angle = 0.0f,
		                        StartColor = GorgonColor.Black,
		                        EndColor = GorgonColor.White,
		                        GammaCorrection = false,
		                        GradientRegion = new RectangleF(0, 0, 64, 64)
	                        };
	        PatternBrush = new GorgonGlyphHatchBrush
	                       {
		                       ForegroundColor = GorgonColor.White,
		                       BackgroundColor = GorgonColor.Black,
		                       HatchStyle = HatchStyle.BackwardDiagonal
	                       };
        }
        #endregion
    }
}
