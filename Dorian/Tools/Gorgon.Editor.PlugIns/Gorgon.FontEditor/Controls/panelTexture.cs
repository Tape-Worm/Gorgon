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
// Created: Tuesday, March 11, 2014 9:11:06 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.FontEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.Renderers;
using GorgonLibrary.UI;
using SlimMath;

namespace GorgonLibrary.Editor.FontEditorPlugIn.Controls
{
	/// <summary>
	/// An interface for managing the application of a texture to font glyphs.
	/// </summary>
	public partial class PanelTexture 
		: UserControl
	{
		#region Classes.
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
				_nodeColor = result ? new GorgonColor(1, 0, 0, 0.5f) : new GorgonColor(0, 1, 1, 0.5f);
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
					bounds.Inflate(2, 2);
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

		#region Events.
		/// <summary>
		/// Event fired when the brush has been changed.
		/// </summary>
		public event EventHandler BrushChanged;
		#endregion

		#region Variables.
		private readonly StringBuilder _infoText = new StringBuilder(256);	// Text for the info label.
		private WrapMode _wrapMode = WrapMode.Tile;							// Current wrapping mode.
		private RectangleF _region = RectangleF.Empty;						// The selected texture region.
		private GorgonTexture2D _texture;									// Selected texture.
		private SelectionArea _selectionArea;								// Selection area.
		private GorgonTexture2D _defaultTexture;							// Default texture.
		private GorgonSwapChain _swapChain;									// The swap chain to use for displaying images.
		private Gorgon2DStateRecall _lastState;								// Last 2D state.
		private Gorgon2D _renderer;											// Renderer.
		private GorgonGraphics _graphics;									// Graphics interface.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the image editor.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IImageEditorPlugIn ImageEditor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the path to an existing texture brush.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string TextureBrushPath
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the currently active texture.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GorgonTexture2D Texture
		{
			get
			{
				return _texture;
			}
			set
			{
				if (_graphics == null)
				{
					return;
				}

				if (value != null)
				{
					_texture = _graphics.Textures.CreateTexture(value.Name, value.Settings);
					_texture.Copy(value);
				}
				else
				{
					if (_texture != null)
					{
						_texture.Dispose();
						_texture = null;
					}

					_texture = null;
				}
			}
		}

		/// <summary>
		/// Property to set or return the current wrapping mode.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public WrapMode WrapMode
		{
			get
			{
				if (IsHandleCreated)
				{
					return (WrapModeComboItem)comboWrapMode.SelectedItem;
				}

				return _wrapMode;
			}
			set
			{
				_wrapMode = value;

				if (IsHandleCreated)
				{
					comboWrapMode.SelectedIndex = comboWrapMode.Items.IndexOf(new WrapModeComboItem(value));
				}
			}
		}

		/// <summary>
		/// Property to set or return the region for the selected texture.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public RectangleF TextureRegion
		{
			get
			{
				if ((IsHandleCreated)
					&& (Texture != null))
				{
					return new RectangleF(Texture.ToTexel((float)numericX.Value, (float)numericY.Value),
					                      Texture.ToTexel((float)numericWidth.Value, (float)numericHeight.Value));
				}

				return _region;
			}
			set
			{
				if (_selectionArea == null)
				{
					return;
				}

				_region = Texture == null ? new RectangleF(0, 0, 1, 1) : Texture.ToPixel(value);

				if (!IsHandleCreated)
				{
					return;
				}

				numericX.Value = (decimal)_region.X;
				numericY.Value = (decimal)_region.Y;
				numericWidth.Value = (decimal)_region.Width;
				numericHeight.Value = (decimal)_region.Height;

				_selectionArea.SetTexture(Texture,
										  new Rectangle((int)_region.X, (int)_region.Y, (int)_region.Width, (int)_region.Height));
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function called when the brush is changed.
		/// </summary>
		private void OnBrushChanged()
		{
			if (BrushChanged == null)
			{
				return;
			}

			BrushChanged(this, EventArgs.Empty);
		}

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
				GorgonDialogs.ErrorBox(ParentForm, ex);
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

			if ((_selectionArea == null)
                || (Texture == null))
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

			OnBrushChanged();
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
				GorgonDialogs.ErrorBox(ParentForm, ex);
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
			OnBrushChanged();
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
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateCommands();
				UpdateLabelInfo();
				OnBrushChanged();
			}
		}

		/// <summary>
		/// Function to perform localization for controls.
		/// </summary>
		private void PerformLocalization()
		{
			comboWrapMode.Items.Clear();
			comboWrapMode.Items.Add(new WrapModeComboItem(WrapMode.Tile, Resources.GORFNT_PROP_VALUE_TILE));
			comboWrapMode.Items.Add(new WrapModeComboItem(WrapMode.Clamp, Resources.GORFNT_PROP_VALUE_CLAMP));
			comboWrapMode.Items.Add(new WrapModeComboItem(WrapMode.TileFlipX, Resources.GORFNT_PROP_VALUE_TILEFLIPX));
			comboWrapMode.Items.Add(new WrapModeComboItem(WrapMode.TileFlipY, Resources.GORFNT_PROP_VALUE_TILEFLIPY));
			comboWrapMode.Items.Add(new WrapModeComboItem(WrapMode.TileFlipXY, Resources.GORFNT_PROP_VALUE_TILEFLIPBOTH));

			labelTexHeight.Text = Resources.GORFNT_PROP_VALUE_HEIGHT;
			labelTexWidth.Text = Resources.GORFNT_PROP_VALUE_WIDTH;
			labelTexLeft.Text = Resources.GORFNT_PROP_VALUE_LEFT;
			labelTexTop.Text = Resources.GORFNT_PROP_VALUE_TOP;
			labelWrapMode.Text = Resources.GORFNT_PROP_VALUE_WRAP_MODE;
			buttonOpen.Text = Resources.GORFNT_PROP_VALUE_OPEN_TEXTURE;
		}

		/// <summary>
		/// Function to perform validation for the controls on the panel.
		/// </summary>
		private void ValidateCommands()
		{
		    buttonOpen.Enabled = ImageEditor != null;
			numericHeight.Enabled = numericWidth.Enabled = numericX.Enabled = numericY.Enabled = comboWrapMode.Enabled = Texture != null;
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
				if (!string.IsNullOrWhiteSpace(GorgonFontEditorPlugIn.Settings.LastTextureImportPath))
				{
					imageFileBrowser.StartDirectory = GorgonFontEditorPlugIn.Settings.LastTextureImportPath;
				}

				imageFileBrowser.FileExtensions.Clear();
				foreach (var extension in ImageEditor.FileExtensions)
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

					Cursor.Current = Cursors.WaitCursor;

					// Load the image.
					using (Stream stream = imageFileBrowser.Files[0].OpenStream(false))
					{
						using (IImageEditorContent imageContent = ImageEditor.ImportContent(imageFileBrowser.Files[0].FullPath,
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
							if (_texture != null)
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

							OnBrushChanged();
						}
					}

					GorgonFontEditorPlugIn.Settings.LastTextureImportPath = imageFileBrowser.Files[0].Directory.FullPath;
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

			if (Texture == null)
			{
				_renderer.Render(2);
				return true;
			}

			_renderer.Drawing.Blit(Texture,
								   _selectionArea.TextureRegion,
								   new RectangleF(0, 0, 1, 1));

			_selectionArea.Draw();

			_renderer.Render(2);
			return true;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (DesignMode)
			{
				return;
			}

			try
			{
				PerformLocalization();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateCommands();
			}
		}

		/// <summary>
		/// Function to set up the renderer.
		/// </summary>
		/// <param name="renderer">Renderer to use.</param>
		public void SetupRenderer(Gorgon2D renderer)
		{
			if (DesignMode)
			{
				return;
			}

			Gorgon.ApplicationIdleLoopMethod = null;

			if ((_renderer != null) && (_renderer != renderer))
			{
				if (_lastState != null)
				{
					_renderer.End2D(_lastState);
					_lastState = null;
				}

				if (_texture != null)
				{
					_texture.Dispose();
					_texture = null;
				}

				if (_defaultTexture != null)
				{
					_defaultTexture.Dispose();
					_defaultTexture = null;
				}

				if (_swapChain != null)
				{
					_swapChain.Dispose();
					_swapChain = null;
				}

				_selectionArea = null;
				_graphics = null;
			}

			if (renderer == null)
			{
				return;
			}

			_renderer = renderer;
			_graphics = renderer.Graphics;

			if (_swapChain == null)
			{
				_swapChain = _graphics.Output.CreateSwapChain("ImageDisplay",
				                                             new GorgonSwapChainSettings
				                                             {
					                                             Window = panelTextureDisplay,
					                                             Format = BufferFormat.R8G8B8A8_UIntNormal
				                                             });
			}

			if (_lastState == null)
			{
				_lastState = renderer.Begin2D();
			}

			if (_defaultTexture == null)
			{
				_defaultTexture = _graphics.Textures.CreateTexture<GorgonTexture2D>("DefaultPattern", Resources.Pattern);
			}

			if (_selectionArea == null)
			{
				_selectionArea = new SelectionArea(renderer, _defaultTexture, panelTextureDisplay);
			}

			_region = new RectangleF(0, 0, 1, 1);

			_renderer.Target = _swapChain;

			Gorgon.ApplicationIdleLoopMethod = Idle;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="PanelTexture"/> class.
		/// </summary>
		public PanelTexture()
		{
			InitializeComponent();
		}
		#endregion
	}
}
