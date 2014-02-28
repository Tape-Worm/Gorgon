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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Fetze.WinFormsColor;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.FontEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
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
			private RectangleF _nodeBounds;				// Bounding area for the node.
			private Vector2 _nodePosition;				// Position for the node.
			private GorgonColor _nodeColor;				// Color of the node.
			private RectangleF _drawBounds;				// Bounding area for the view.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to set or return the cursor associated with this node point.
			/// </summary>
			public Cursor NodeCursor
			{
				get;
				private set;
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to set the node position.
			/// </summary>
			/// <param name="nodePosition">Position of the node.</param>
			public void SetNodePosition(Vector2 nodePosition)
			{
				_nodePosition = nodePosition;
				_nodeBounds = new RectangleF(_nodePosition.X - 4, _nodePosition.Y - 4, 8, 8);
			}

			/// <summary>
			/// Function to perform a hit test on the node.
			/// </summary>
			/// <param name="cursorPosition">Position of the mouse cursor.</param>
			/// <returns>TRUE if the mouse is over the node, FALSE if not.</returns>
			public bool HitTest(Point cursorPosition)
			{
				bool result = _nodeBounds.Contains(cursorPosition); 
				_nodeColor =  result ? new GorgonColor(1, 0, 0, 0.5f) : new GorgonColor(0, 1, 1, 0.5f);
				return result;
			}

			/// <summary>
			/// Function to render the node point.
			/// </summary>
			/// <param name="renderer">Renderer used to render the node point.</param>
			public void Draw(Gorgon2D renderer)
			{
				RectangleF bounds = _nodeBounds;

				// Ensure that we can see the nodes.
				if ((!_drawBounds.Contains(bounds.Location))
					|| (!_drawBounds.Contains(new PointF(bounds.Right, bounds.Bottom))))
				{
					bounds.Inflate(2,2);
				}

				renderer.Drawing.FilledEllipse(bounds, _nodeColor);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="CornerNode"/> class.
			/// </summary>
			/// <param name="cursor">The cursor.</param>
			/// <param name="areaBounds">The area to display the node within.</param>
			public CornerNode(Cursor cursor, RectangleF areaBounds)
			{
				NodeCursor = cursor;
				_nodePosition = Vector2.Zero;
				_nodeColor = new GorgonColor(0, 1, 1, 0.5f);
				_drawBounds = areaBounds;
			}
			#endregion
	    }

		/// <summary>
		/// Selection area interface.
		/// </summary>
	    private class SelectionArea
	    {
			#region Variables.
			private readonly Panel _container;									// Container being drawn in.
			private readonly Gorgon2D _renderer;								// Renderer to draw with.
			private readonly GorgonSprite _selectionSprite;						// Selection sprite.
			private Vector2 _textureSize;										// Size of the texture that is being selected.
			private RectangleF _drawRegion = RectangleF.Empty;					// Region to draw.
			private Rectangle _selectionRegion = Rectangle.Empty;				// Selection area.
			private RectangleF _textureRegion = RectangleF.Empty;				// The screen space texture area.
			private Vector2 _dragOffset = Vector2.Zero;							// Offset for dragging.
			private Vector2 _imageScale = Vector2.Zero;							// Image scaling.
			private readonly CornerNode[] _corners = new CornerNode[4];			// Corner nodes.
			private int _dragNodeIndex = -1;									// The current node index that we're dragging.
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
			/// Property to return whether a node is being dragged.
			/// </summary>
			public bool IsDraggingNode
			{
				get
				{
					return _dragNodeIndex != -1;
				}
			}

			/// <summary>
			/// Property to return the texture region in client space.
			/// </summary>
			public RectangleF TextureRegion
			{
				get
				{
					return _textureRegion;
				}
			}

			/// <summary>
			/// Property to set or return the region for selection.
			/// </summary>
			public Rectangle SelectionRegion
			{
				get
				{
					return _selectionRegion;
				}
				set
				{
					_selectionRegion = value;
					
					_drawRegion = new RectangleF((_selectionRegion.X * _imageScale.X) + _textureRegion.X,
												 (_selectionRegion.Y * _imageScale.Y) + _textureRegion.Y,
												 _selectionRegion.Width * _imageScale.X,
												 _selectionRegion.Height * _imageScale.Y);

					UpdateNodes();
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to update the corner codes.
			/// </summary>
			private void UpdateNodes()
			{
				// Set the new positions.
				_corners[0].SetNodePosition(_drawRegion.Location);
				_corners[1].SetNodePosition(new Vector2(_drawRegion.Right, _drawRegion.Top));
				_corners[2].SetNodePosition(new Vector2(_drawRegion.Right, _drawRegion.Bottom));
				_corners[3].SetNodePosition(new Vector2(_drawRegion.Left, _drawRegion.Bottom));
			}

			/// <summary>
			/// Function to move the entire as a result of a click in the body of the selection rectangle.
			/// </summary>
			/// <param name="mousePosition">The coordinates of the mouse cursor in the client area.</param>
			private void MoveSelection(Point mousePosition)
			{
				if (!IsDragging)
				{
					return;
				}

				_drawRegion = new RectangleF(mousePosition.X + _dragOffset.X, mousePosition.Y + _dragOffset.Y, _drawRegion.Width, _drawRegion.Height);

				if (_drawRegion.X < _textureRegion.X)
				{
					_drawRegion.X = _textureRegion.X;
				}

				if (_drawRegion.Y < _textureRegion.Y)
				{
					_drawRegion.Y = _textureRegion.Y;
				}

				if (_drawRegion.Right > _textureRegion.Right)
				{
					_drawRegion.X = (_textureRegion.Width - _drawRegion.Width) + _textureRegion.X;
				}

				if (_drawRegion.Bottom > _textureRegion.Bottom)
				{
					_drawRegion.Y = (_textureRegion.Height - _drawRegion.Height) + _textureRegion.Y;
				}

				UpdateFromDrawingRegion();
				UpdateNodes();
			}

			/// <summary>
			/// Function to set the texture parameters for the selection region.
			/// </summary>
			/// <param name="texture">The texture to select from.</param>
			/// <param name="selectedRegion">The initial selected region.</param>
			public void SetTexture(GorgonTexture2D texture, Rectangle selectedRegion)
			{
				float imageRatio;
				Vector2 imagePosition;
				Vector2 imageSize = _container.Size;

				// Get the texture size in client space for the container.
				if (texture.Settings.Width > texture.Settings.Height)
				{
					imageRatio = (float)texture.Settings.Height / texture.Settings.Width;
					imageSize.Y *= imageRatio;
					imagePosition = new Vector2(0, _container.Size.Height / 2.0f - imageSize.Y / 2.0f);
				}
				else
				{
					imageRatio = (float)texture.Settings.Width / texture.Settings.Height;
					imageSize.X *= imageRatio;
					imagePosition = new Vector2(_container.Size.Width / 2.0f - imageSize.X / 2.0f, 0);
				}

				_textureRegion = new RectangleF(imagePosition, imageSize);
				_textureSize = texture.Settings.Size;

				_imageScale = new Vector2(_textureRegion.Width / _textureSize.X, _textureRegion.Height / _textureSize.Y);

				SelectionRegion = Rectangle.Intersect(selectedRegion,
				                                      new Rectangle(0, 0, texture.Settings.Width, texture.Settings.Height));
			}

			/// <summary>
			/// Function to begin a dragging operation on the main selection region.
			/// </summary>
			/// <param name="mousePosition">Mouse position.</param>
			private void StartDrag(Point mousePosition)
			{
				IsDragging = true;
				_dragOffset = new Vector2(_drawRegion.X - mousePosition.X, _drawRegion.Y - mousePosition.Y);
				
				MouseMove(mousePosition, MouseButtons.Left);
			}

			/// <summary>
			/// Function to end a drag operation on the selection area.
			/// </summary>
			private void EndDrag()
			{
				IsDragging = false;
			}

			/// <summary>
			/// Function to retrieve the selected corner node.
			/// </summary>
			/// <param name="mousePosition">The position of the mouse cursor.</param>
			/// <returns>The node if selected, NULL if none are selected.</returns>
			private CornerNode GetSelectedNode(Point mousePosition)
			{
				CornerNode hitNode = null;

				// ReSharper disable once ForCanBeConvertedToForeach
				// ReSharper disable once LoopCanBeConvertedToQuery
				for (int i = 0; i < _corners.Length; ++i)
				{
					// ReSharper disable once InvertIf
					if (_corners[i].HitTest(mousePosition))
					{
						hitNode = _corners[i];
						break;
					}
				}

				return hitNode;
			}

			/// <summary>
			/// Function to update the selection based on the drawn rectangle.
			/// </summary>
			private void UpdateFromDrawingRegion()
			{
				_selectionRegion = Rectangle.Intersect(Rectangle.FromLTRB((int)((_drawRegion.X - _textureRegion.X) / _imageScale.X),
				                                                          (int)((_drawRegion.Y - _textureRegion.Y) / _imageScale.Y),
				                                                          (int)((_drawRegion.Right - _textureRegion.X) / _imageScale.X),
				                                                          (int)((_drawRegion.Bottom - _textureRegion.Y) / _imageScale.Y)),
				                                       new Rectangle(0,
				                                                     0,
				                                                     (int)_textureSize.X,
				                                                     (int)_textureSize.Y));
			}

			/// <summary>
			/// Function to drag the selected node around by the mouse cursor.
			/// </summary>
			/// <param name="mousePosition">Position of the mouse cursor.</param>
			private void MoveNode(Point mousePosition)
			{
				if (_dragNodeIndex == -1)
				{
					return;
				}

				// Update the drawing region based on the selected node.
				switch (_dragNodeIndex)
				{
					case 0:
						if ((mousePosition.X <= _drawRegion.Right - 4)
						    && (mousePosition.Y <= _drawRegion.Bottom - 4))
						{
							_drawRegion = RectangleF.FromLTRB(mousePosition.X, mousePosition.Y, _drawRegion.Right, _drawRegion.Bottom);
						}
						break;
					case 1:
						if ((mousePosition.X >= _drawRegion.Left + 4)
						    && (mousePosition.Y <= _drawRegion.Bottom - 4))
						{
							_drawRegion = RectangleF.FromLTRB(_drawRegion.X, mousePosition.Y, mousePosition.X, _drawRegion.Bottom);
						}
						break;
					case 2:
						if ((mousePosition.X >= _drawRegion.Left + 4)
						    && (mousePosition.Y >= _drawRegion.Top + 4))
						{
							_drawRegion = RectangleF.FromLTRB(_drawRegion.X, _drawRegion.Y, mousePosition.X, mousePosition.Y);
						}
						break;
					case 3:
						if ((mousePosition.X <= _drawRegion.Right - 4)
						    && (mousePosition.Y >= _drawRegion.Top + 4))
						{
							_drawRegion = RectangleF.FromLTRB(mousePosition.X, _drawRegion.Y, _drawRegion.Right, mousePosition.Y);
						}
						break;
				}

				_drawRegion = RectangleF.Intersect(_textureRegion, _drawRegion);
				UpdateFromDrawingRegion();
				UpdateNodes();
			}

			/// <summary>
			/// Function handle a mouse down in the selection region.
			/// </summary>
			/// <param name="mousePosition">Mouse cursor position.</param>
			/// <param name="buttons">Button that was pressed.</param>
			/// <returns>TRUE if handled here, FALSE if not.</returns>
			public bool MouseDown(Point mousePosition, MouseButtons buttons)
			{
				if (buttons != MouseButtons.Left)
				{
					return false;
				}

				if (!IsDragging)
				{
					CornerNode hitNode = GetSelectedNode(mousePosition);

					if (hitNode != null)
					{
						_dragNodeIndex = Array.IndexOf(_corners, hitNode);

						if (_dragNodeIndex != -1)
						{
							return true;
						}
					}
				}

				if (!_drawRegion.Contains(mousePosition))
				{
					return false;
				}

				StartDrag(mousePosition);
				return true;
			}

			/// <summary>
			/// Function handle a mouse up in the selection region.
			/// </summary>
			/// <param name="buttons">Button that was pressed.</param>
			/// <returns>TRUE if handled here, FALSE if not.</returns>
			public bool MouseUp(MouseButtons buttons)
			{
				if (buttons != MouseButtons.Left)
				{
					return false;
				}

				if (_dragNodeIndex != -1)
				{
					_dragNodeIndex = -1;
					return true;
				}

				if (!IsDragging)
				{
					return false;
				}

				EndDrag();
				return true;
			}

			/// <summary>
			/// Function called when the mouse moves.
			/// </summary>
			/// <param name="mousePosition">Position of the mouse cursor.</param>
			/// <param name="buttons">Currently pressed mouse buttons.</param>
			public void MouseMove(Point mousePosition, MouseButtons buttons)
			{
				// Only select the nodes when we're not dragging the main selection around.
				if ((!IsDragging) && (_dragNodeIndex == -1))
				{
					CornerNode hitNode = GetSelectedNode(mousePosition);

					// Change the cursor.
					if (hitNode != null)
					{
						if (_container.Cursor == Cursors.Default)
						{
							_container.Cursor = hitNode.NodeCursor;
						}
						return;
					}

					_container.Cursor = Cursors.Default;
				}

				if (buttons != MouseButtons.Left)
				{
					return;
				}

				if (_dragNodeIndex != -1)
				{
					// Mode the node to match the cursor.
					MoveNode(mousePosition);
					return;
				}

				MoveSelection(mousePosition);
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

				_selectionSprite.Size = _drawRegion.Size;
				_selectionSprite.Position = region.Location;
				_selectionSprite.TextureRegion = new RectangleF(textureOffset,
																new Vector2(
																	_drawRegion.Width / _selectionSprite.Texture.Settings.Width,
																	_drawRegion.Height / _selectionSprite.Texture.Settings.Height));

				_selectionSprite.Draw();

				// Draw node corners.
				// ReSharper disable once ForCanBeConvertedToForeach
				for (int i = 0; i < _corners.Length; ++i)
				{
					_corners[i].Draw(_renderer);
				}
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="SelectionArea"/> class.
			/// </summary>
			/// <param name="renderer">The renderer used to draw the selection area.</param>
			/// <param name="selectionTexture">The selection texture.</param>
			/// <param name="container">The container for the drawing area.</param>
			public SelectionArea(Gorgon2D renderer, GorgonTexture2D selectionTexture, Panel container)
			{
				_renderer = renderer;
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

				// Use clock wise orientation the corners.
				_corners[0] = new CornerNode(Cursors.SizeNWSE, container.ClientRectangle);
				_corners[1] = new CornerNode(Cursors.SizeNESW, container.ClientRectangle);
				_corners[2] = new CornerNode(Cursors.SizeNWSE, container.ClientRectangle);
				_corners[3] = new CornerNode(Cursors.SizeNESW, container.ClientRectangle);

				_container = container;
			}
			#endregion
	    }
		#endregion

		#region Variables.
		// Primary selected color attribute for color selection panels.
		private static ColorPickerPanel.PrimaryAttrib _primaryAttr = ColorPickerPanel.PrimaryAttrib.Hue;

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
		private readonly StringBuilder _infoText = new StringBuilder(256);	// Text for the info label.
        #endregion

		#region Properties.
		/// <summary>
		/// Property to return the path to an existing texture brush.
		/// </summary>
	    public string TextureBrushPath
	    {
		    get;
			set;
	    }

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

				if (_texture != null)
				{
					_textureRegion = _textureBrush.TextureRegion == null
						                 ? new RectangleF(0, 0, _texture.Settings.Width, _texture.Settings.Height)
						                 : _texture.ToPixel(_textureBrush.TextureRegion.Value);

					_textureWrapMode = _textureBrush.WrapMode;
				}
				else
				{
					_textureWrapMode = WrapMode.Tile;
					_textureRegion = new RectangleF(0, 0, 1, 1);
				}
			}
	    }
        #endregion

        #region Methods.
		/// <summary>
		/// Function to disable the limits on the numeric fields.
		/// </summary>
	    private void DisableNumericLimits()
		{
			numericWidth.Maximum = Int32.MaxValue;
			numericHeight.Maximum = Int32.MaxValue;
			numericX.Maximum = Int32.MaxValue;
			numericY.Maximum = Int32.MaxValue;
		}

		/// <summary>
		/// Function to enable the limits on the numeric fields.
		/// </summary>
	    private void EnableNumericLimits()
	    {
			if (_texture == null)
			{
				return;
			}

			numericWidth.Maximum = _texture.Settings.Width - _selectionArea.SelectionRegion.X;
			numericHeight.Maximum = _texture.Settings.Height - _selectionArea.SelectionRegion.Y;
			numericX.Maximum = _texture.Settings.Width - _selectionArea.SelectionRegion.Width;
			numericY.Maximum = _texture.Settings.Height - _selectionArea.SelectionRegion.Height;
		}

		/// <summary>
		/// Handles the MouseDown event of the panelTextureDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelTextureDisplay_MouseDown(object sender, MouseEventArgs e)
		{
			try
			{
				if (_selectionArea == null)
				{
					return;
				}

				if (_selectionArea.MouseDown(e.Location, e.Button))
				{
					DisableNumericLimits();
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateCommands();
				UpdateLabelInfo();
			}
		}

		/// <summary>
		/// Function to update the information label.
		/// </summary>
	    private void UpdateLabelInfo()
		{
			_infoText.Length = 0;

			if (_selectionArea == null)
			{
				labelInfo.Text = _infoText.ToString();
				return;
			}

			_infoText.AppendFormat(Resources.GORFNT_PROP_VALUE_SELECTLABEL,
			                       _selectionArea.SelectionRegion.X,
			                       _selectionArea.SelectionRegion.Y,
			                       _selectionArea.SelectionRegion.Right,
			                       _selectionArea.SelectionRegion.Bottom,
			                       _selectionArea.SelectionRegion.Width,
			                       _selectionArea.SelectionRegion.Height);

			labelInfo.Text = _infoText.ToString();
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

			if ((!_selectionArea.IsDragging) && (!_selectionArea.IsDraggingNode))
			{
				return;
			}

			Rectangle numericValues = Rectangle.Intersect(Rectangle.Round(_selectionArea.SelectionRegion),
			                                              new Rectangle(0, 0, _texture.Settings.Width, _texture.Settings.Height));

			numericX.Value = numericValues.X;
			numericY.Value = numericValues.Y;
			numericWidth.Value = numericValues.Width;
			numericHeight.Value = numericValues.Height;
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

			try
			{
				if (_selectionArea.MouseUp(e.Button))
				{
					EnableNumericLimits();
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				UpdateLabelInfo();
				ValidateCommands();
			}
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
			try
			{
				if ((_selectionArea == null) || (_selectionArea.IsDragging) || (_selectionArea.IsDraggingNode))
				{
					return;
				}

				_selectionArea.SelectionRegion = new Rectangle((int)numericX.Value,
				                                               (int)numericY.Value,
				                                               (int)numericWidth.Value,
				                                               (int)numericHeight.Value);
				EnableNumericLimits();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateCommands();
				UpdateLabelInfo();
			}
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

					break;
			}
	    }

		/// <summary>
		/// Handles the Click event of the buttonOpen control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonOpen_Click(object sender, EventArgs e)
		{
			DisableNumericLimits();

			numericWidth.ValueChanged -= numericX_ValueChanged;
			numericHeight.ValueChanged -= numericX_ValueChanged;
			numericX.ValueChanged -= numericX_ValueChanged;
			numericY.ValueChanged -= numericX_ValueChanged;

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
					// Don't load the same image.
					if ((!string.IsNullOrWhiteSpace(TextureBrushPath)) &&
					    (string.Equals(TextureBrushPath, imageFileBrowser.Files[0].FullPath)))
					{
						return;
					}

					// Don't try to re-load a texture in use elsewhere on the font.
					if (_currentContent.Dependencies.Contains(imageFileBrowser.Files[0].FullPath))
					{
						GorgonDialogs.ErrorBox(this, string.Format(Resources.GORFNT_TEXTURE_IN_USE, imageFileBrowser.Files[0].FullPath));
						return;
					}

					Cursor.Current = Cursors.WaitCursor;

					// Load the image.
					using (Stream stream = imageFileBrowser.Files[0].OpenStream(false))
					{
						using (IImageEditorContent imageContent = _currentContent.ImageEditor.ImportContent(imageFileBrowser.Files[0].Name, 
																											stream,
						                                                                                    0,
						                                                                                    0,
						                                                                                    false,
						                                                                                    BufferFormat.R8G8B8A8_UIntNormal))
						{
							if (imageContent.Image.Settings.ImageType != ImageType.Image2D)
							{
								GorgonDialogs.ErrorBox(ParentForm, string.Format(Resources.GORFNT_IMAGE_NOT_2D, imageContent.Name));
								return;
							}

							// If we've loaded a texture previously and haven't committed it yet, then get rid of it.
							if (_texture != _originalTexture)
							{
								_texture.Dispose();
							}

							var settings = (GorgonTexture2DSettings)imageContent.Image.Settings.Clone();
							_texture = _graphics.Textures.CreateTexture<GorgonTexture2D>(imageContent.Name,
							                                                             imageContent.Image,
							                                                             settings);
							// Reset the texture brush settings.
							numericWidth.Value = settings.Width;
							numericHeight.Value = settings.Height;
							numericX.Value = 0;
							numericY.Value = 0;
							comboWrapMode.Text = Resources.GORFNT_PROP_VALUE_TILE;

							// Function to retrieve the transformation and node point values from the image.
							_selectionArea.SetTexture(_texture, new Rectangle(0, 0, settings.Width, settings.Height));

							TextureBrushPath = imageFileBrowser.Files[0].FullPath;
						}
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
				EnableNumericLimits();

				numericWidth.ValueChanged += numericX_ValueChanged;
				numericHeight.ValueChanged += numericX_ValueChanged;
				numericX.ValueChanged += numericX_ValueChanged;
				numericY.ValueChanged += numericX_ValueChanged;

				Cursor.Current = Cursors.Default;
				UpdateLabelInfo();
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
			try
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
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
				DialogResult = DialogResult.None;
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
	        Text = Resources.GORFNT_DLG_BRUSH_EDITOR_TEXT;

	        buttonOK.Text = Resources.GORFNT_BUTTON_OK_TEXT;
	        buttonCancel.Text = Resources.GORFNT_BUTTON_CANCEL_TEXT;

	        labelBrushType.Text = Resources.GORFNT_PROP_VALUE_BRUSH_TYPE;
	        labelTexHeight.Text = Resources.GORFNT_PROP_VALUE_HEIGHT;
			labelTexWidth.Text = Resources.GORFNT_PROP_VALUE_WIDTH;
			labelTexLeft.Text = Resources.GORFNT_PROP_VALUE_LEFT;
			labelTexTop.Text = Resources.GORFNT_PROP_VALUE_TOP;
	        labelWrapMode.Text = Resources.GORFNT_PROP_VALUE_WRAP_MODE;
	        buttonOpen.Text = Resources.GORFNT_PROP_VALUE_OPEN_TEXTURE;

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

			_renderer.Drawing.Blit(_texture,
								   _selectionArea.TextureRegion,
								   new RectangleF(0, 0, 1, 1));

			_selectionArea.Draw();
			
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

						_selectionArea = new SelectionArea(_renderer, _defaultTexture, panelTextureDisplay);
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

			// Remember our previous selection for the primary color attribute.
			_primaryAttr = colorSolidBrush.PrimaryAttribute;

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

			    colorSolidBrush.PrimaryAttribute = _primaryAttr;
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

						    Rectangle region = Rectangle.Intersect(new Rectangle(0, 0, _texture.Settings.Width, _texture.Settings.Height),
						                                           Rectangle.Round(_textureRegion));

							_selectionArea.SetTexture(_texture, region);
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
