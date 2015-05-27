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
// Created: Sunday, March 16, 2014 12:08:06 AM
// 
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Input;
using Gorgon.Renderers;
using SlimMath;

namespace Gorgon.Editor
{
	/// <summary>
	/// Current dragging mode.
	/// </summary>
	public enum ClipSelectionDragMode
	{
		/// <summary>
		/// No dragging.
		/// </summary>
		None = 0,
		/// <summary>
		/// Selection is being moved.
		/// </summary>
		Move = 1,
		/// <summary>
		/// Selection is being resized.
		/// </summary>
		Resize = 2
	}

	/// <summary>
	/// An interface that allows clipping of regions from a texture.
	/// </summary>
	/// <remarks>Use this to show a UI on a texture to allow users to clip regions from the texture.</remarks>
	public class Clipper
	{
		#region Constants.
		// Top drag bar index.
		private const int DragTop = 0;
		// Bottom drag bar index.
		private const int DragBottom = 1;
		// Left drag bar index.
		private const int DragLeft = 2;
		// Right drag bar index.
		private const int DragRight = 3;
		// NW corner drag node index.
		private const int DragNW = 4;
		// NE corner drag node index.
		private const int DragNE = 5;
		// SW corner drag node index.
		private const int DragSW = 6;
		// SE corner drag node index.
		private const int DragSE = 7;
		#endregion

		#region Variables.
		// Renderer to use when displaying the clipper UI.
		private readonly Gorgon2D _renderer;
		// Size of the texture being clipped.
		private Size _textureSize;
		// Region used to display the texture.
		private RectangleF _textureDisplayRegion;
		// Scale of the texture.
		private Vector2 _scale;
		// The region assigned to the selector.
		private RectangleF _selectorRegion;
		// The current clipping region.
		private RectangleF? _clipRegion;
		// Areas that can be dragged.
		private readonly RectangleF[] _dragAreas;
		// The currently selected drag node.
		private int _selectedDragNode = -1;
		// Control being rendered into.
		private readonly Control _renderControl;
		// Offset of the cursor from the top-left drag corner, or the top-left corner of the selected drag node.
		private Vector2 _dragOffset;
		// Size of the draggable node areas.
		private Size _dragNodeSize;
		// Half size of the draggable node areas.
		private SizeF _dragNodeHalfSize;
		// Default cursor for the control.
		private Cursor _defaultCursor;
        // Offset of the texture relative to the control size.
	    private Vector2 _offset;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the sprite that represents our selection region.
		/// </summary>
		internal GorgonSprite SelectionSprite
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return whether bounds checking should be applied when clipping.
		/// </summary>
		/// <remarks>When this is set to <c>true</c>, the clipped with limit the selection rectangle to the size of the texture (and its <see cref="Offset"/>).  Otherwise, 
		/// it will allow for unconstrained clipping.</remarks>
		public bool NoBounds
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to hide the nodes when drawing the selection rectangle.
		/// </summary>
		public bool HideNodes
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the default cursor for the clipper.
		/// </summary>
		public Cursor DefaultCursor
		{
			get
			{
				if (_defaultCursor != null)
				{
					return _defaultCursor;
				}

				return Cursors.Default;
			}
			set
			{
				_defaultCursor = value;
			}
		}

		/// <summary>
		/// Property to set or return the current dragging mode operation.
		/// </summary>
		public ClipSelectionDragMode DragMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size of the texture for clipping.
		/// </summary>
		public Size TextureSize
		{
			get
			{
				return _textureSize;
			}
			set
			{
				if (value.Width < 1)
				{
					value.Width = 1;
				}

				if (value.Height < 1)
				{
					value.Height = 1;
				}

				if (value.Width >= _renderer.Graphics.Textures.MaxWidth)
				{
					value.Width = _renderer.Graphics.Textures.MaxWidth - 1;
				}

				if (value.Height >= _renderer.Graphics.Textures.MaxHeight)
				{
					value.Height = _renderer.Graphics.Textures.MaxHeight - 1;
				}

				_textureSize = value;

				UpdateRegions();
			}
		}

		/// <summary>
		/// Property to set or return the width and height of the dragging nodes.
		/// </summary>
		public Size DragNodeSize
		{
			get
			{
				return _dragNodeSize;
			}
			set
			{
				if (_dragNodeSize.Width < 2)
				{
					_dragNodeSize.Width = 2;
				}

				if (_dragNodeSize.Height < 2)
				{
					_dragNodeSize.Height = 2;
				}

				_dragNodeSize = value;
				_dragNodeHalfSize = new Size(_dragNodeSize.Width / 2, _dragNodeSize.Height / 2);
			}
		}

		/// <summary>
		/// Property to set or return the color for the drag nodes.
		/// </summary>
		public GorgonColor DragNodeColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the color for the drag nodes when the mouse is hovered over them.
		/// </summary>
		public GorgonColor DragNodeHoverColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the scaling amount used for the texture.
		/// </summary>
		public Vector2 Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_scale = value;

				UpdateRegions();
			}
		}

		/// <summary>
		/// Property to set or return the translation amount of the texture if the texture has been offset on the display area.
		/// </summary>
		public Vector2 Offset
		{
		    get
		    {
		        return _offset;
		    }
		    set
		    {
                if (_clipRegion != null)
                {
                    _clipRegion = ClipRegion;
                }

                _offset = value;

                UpdateRegions();
		    }
		}

		/// <summary>
		/// Property to set or return the pattern to draw on the selection box.
		/// </summary>
		public GorgonTexture2D SelectorPattern
		{
			get
			{
				return SelectionSprite.Texture;
			}
			set
			{
				SelectionSprite.Texture = value;
				SelectionSprite.TextureRegion = new RectangleF(0, 0, 1, 1);
			}
		}

		/// <summary>
		/// Property to set or return the current clipping region.
		/// </summary>
		public RectangleF ClipRegion
		{
			get
			{
				return new RectangleF((_selectorRegion.X - Offset.X) / _scale.X,
				                      (_selectorRegion.Y - Offset.Y) / _scale.Y,
				                      _selectorRegion.Width / Scale.X,
				                      _selectorRegion.Height / Scale.Y);
			}
			set
			{
				_clipRegion = value;

				UpdateRegions();
			}
		}

		/// <summary>
		/// Property to set or return the current selector color.
		/// </summary>
		public GorgonColor SelectorColor
		{
			get
			{
				return SelectionSprite.Color;
			}
			set
			{
				SelectionSprite.Color = value;
			}
		}

		/// <summary>
		/// Property to set or return the speed at which the selector box animates.
		/// </summary>
		public float SelectorAnimateSpeed
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to constrain the selection rectangle.
		/// </summary>
		private void Constrain()
		{
			if (NoBounds)
			{
				return;
			}

			_selectorRegion = RectangleF.Intersect(_selectorRegion, _textureDisplayRegion);
		}

		/// <summary>
		/// Function to update the clipping selection area node regions.
		/// </summary>
		private void UpdateNodeRegions()
		{
			// Top.
			_dragAreas[DragTop] = new RectangleF(_selectorRegion.X + _dragNodeHalfSize.Width,
												 _selectorRegion.Y - _dragNodeHalfSize.Height,
												 _selectorRegion.Width - DragNodeSize.Width,
												 DragNodeSize.Height);

			// Bottom
			_dragAreas[DragBottom] = new RectangleF(_selectorRegion.X + _dragNodeHalfSize.Width,
													_selectorRegion.Bottom - _dragNodeHalfSize.Height,
													_selectorRegion.Width - DragNodeSize.Width,
													DragNodeSize.Height);

			// Left.
			_dragAreas[DragLeft] = new RectangleF(_selectorRegion.X - _dragNodeHalfSize.Width,
												  _selectorRegion.Y + _dragNodeHalfSize.Height,
												  DragNodeSize.Width,
												  _selectorRegion.Height - DragNodeSize.Height);

			// Right
			_dragAreas[DragRight] = new RectangleF(_selectorRegion.Right - _dragNodeHalfSize.Width,
												   _selectorRegion.Y + _dragNodeHalfSize.Height,
												   DragNodeSize.Width,
												   _selectorRegion.Height - DragNodeSize.Height);

			// NW corner.
			_dragAreas[DragNW] = new RectangleF(_selectorRegion.X - _dragNodeHalfSize.Width,
												_selectorRegion.Y - _dragNodeHalfSize.Height,
												DragNodeSize.Width,
												DragNodeSize.Height);

			// NE corner.
			_dragAreas[DragNE] = new RectangleF(_selectorRegion.Right - _dragNodeHalfSize.Width,
												_selectorRegion.Y - _dragNodeHalfSize.Height,
												DragNodeSize.Width,
												DragNodeSize.Height);

			// SW corner.
			_dragAreas[DragSW] = new RectangleF(_selectorRegion.X - _dragNodeHalfSize.Width,
												_selectorRegion.Bottom - _dragNodeHalfSize.Height,
												DragNodeSize.Width,
												DragNodeSize.Height);

			// SE corner.
			_dragAreas[DragSE] = new RectangleF(_selectorRegion.Right - _dragNodeHalfSize.Width,
												_selectorRegion.Bottom - _dragNodeHalfSize.Height,
												DragNodeSize.Width,
												DragNodeSize.Height);
		}

		/// <summary>
		/// Function to update the clipping region on the UI.
		/// </summary>
		private void UpdateClipRegion()
		{
			if (_clipRegion == null)
			{
				return;
			}

			_selectorRegion = new RectangleF(_textureDisplayRegion.X + (_clipRegion.Value.X * _scale.X),
			                                 _textureDisplayRegion.Y + (_clipRegion.Value.Y * _scale.Y),
			                                 _clipRegion.Value.Width * _scale.X,
			                                 _clipRegion.Value.Height * _scale.Y);

			// Limit to the size of the texture area.
			Constrain();

			UpdateNodeRegions();
		}

		/// <summary>
		/// Function to update regions.
		/// </summary>
		private void UpdateRegions()
		{
			_textureDisplayRegion = new RectangleF(Offset.X,
			                                       Offset.Y,
			                                       _textureSize.Width * _scale.X,
			                                       _textureSize.Height * _scale.Y);

			_selectorRegion = RectangleF.Empty;

			UpdateClipRegion();
		}

		/// <summary>
		/// Function set the correct cursor when the mouse hovers over a node.
		/// </summary>
		private void SetCursor()
		{
			switch (_selectedDragNode)
			{
				case DragTop:
				case DragBottom:
					_renderControl.Cursor = Cursors.SizeNS;
					break;
				case DragLeft:
				case DragRight:
					_renderControl.Cursor = Cursors.SizeWE;
					break;
				case DragNW:
				case DragSE:
					_renderControl.Cursor = Cursors.SizeNWSE;
					break;
				case DragSW:
				case DragNE:
					_renderControl.Cursor = Cursors.SizeNESW;
					break;
				default:
					_renderControl.Cursor = DefaultCursor;
					break;
			}
		}

		/// <summary>
		/// Function to perform a hit test on the nodes for dragging.
		/// </summary>
		/// <param name="cursorPosition">Current client area relative mouse cursor position.</param>
		private void HitTestNodes(Point cursorPosition)
		{
			_selectedDragNode = -1;

			if (HideNodes)
			{
				return;
			}

			for (int i = 0; i < _dragAreas.Length; ++i)
			{
				// ReSharper disable once InvertIf
				if (_dragAreas[i].Contains(cursorPosition))
				{
					_selectedDragNode = i;
					break;
				}
			}
		}

		/// <summary>
		/// Function to move the selection rectangle.
		/// </summary>
		/// <param name="cursorPosition">Current client relative cursor position.</param>
		private void MoveSelection(Point cursorPosition)
		{
			_selectorRegion.X = cursorPosition.X + _dragOffset.X;
			_selectorRegion.Y = cursorPosition.Y + _dragOffset.Y;

			if (NoBounds)
			{
				UpdateNodeRegions();
				return;
			}

			if (_selectorRegion.X < _textureDisplayRegion.X)
			{
				_selectorRegion.X = _textureDisplayRegion.X;
			}

			if (_selectorRegion.Y < _textureDisplayRegion.Y)
			{
				_selectorRegion.Y = _textureDisplayRegion.Y;
			}

			if (_selectorRegion.Right > _textureDisplayRegion.Right)
			{
				_selectorRegion.X = (_textureDisplayRegion.Width - _selectorRegion.Width) + _textureDisplayRegion.X;
			}

			if (_selectorRegion.Bottom > _textureDisplayRegion.Bottom)
			{
				_selectorRegion.Y = (_textureDisplayRegion.Height - _selectorRegion.Height) + _textureDisplayRegion.Y;
			}

			UpdateNodeRegions();
		}

		/// <summary>
		/// Function to update a dragging node.
		/// </summary>
		/// <param name="cursorPosition">Current client relative cursor position.</param>
		private void MoveNode(PointF cursorPosition)
		{
			if (_selectedDragNode == -1)
			{
				return;
			}

			switch (_selectedDragNode)
			{
				case DragTop:
					if (cursorPosition.Y > _selectorRegion.Bottom - _dragNodeHalfSize.Height)
					{
						break;
					}

					_selectorRegion = RectangleF.FromLTRB(_selectorRegion.X,
														  cursorPosition.Y - _dragOffset.Y,
														  _selectorRegion.Right,
														  _selectorRegion.Bottom);
					break;
				case DragLeft:
					if (cursorPosition.X > _selectorRegion.Right - _dragNodeHalfSize.Width)
					{
						break;
					}

					_selectorRegion = RectangleF.FromLTRB(cursorPosition.X - _dragOffset.X,
					                                      _selectorRegion.Y,
					                                      _selectorRegion.Right,
					                                      _selectorRegion.Bottom);
					break;
				case DragRight:
					if (cursorPosition.X < _selectorRegion.Left + _dragNodeHalfSize.Width)
					{
						break;
					}

					_selectorRegion = RectangleF.FromLTRB(_selectorRegion.X,
														  _selectorRegion.Y,
														  cursorPosition.X - _dragOffset.X,
														  _selectorRegion.Bottom);
					break;
				case DragBottom:
					if (cursorPosition.Y < _selectorRegion.Top + _dragNodeHalfSize.Height)
					{
						break;
					}

					_selectorRegion = RectangleF.FromLTRB(_selectorRegion.X,
														  _selectorRegion.Y,
														  _selectorRegion.Right,
														  cursorPosition.Y - _dragOffset.Y);
					break;
				case DragNW:
					if ((cursorPosition.X > _selectorRegion.Right - _dragNodeHalfSize.Width)
						|| (cursorPosition.Y > _selectorRegion.Bottom - _dragNodeHalfSize.Height))
					{
						break;
					}

					_selectorRegion = RectangleF.FromLTRB(cursorPosition.X - _dragOffset.X,
														  cursorPosition.Y - _dragOffset.Y,
														  _selectorRegion.Right,
														  _selectorRegion.Bottom);
					break;
				case DragNE:
					if ((cursorPosition.X < _selectorRegion.Left + _dragNodeHalfSize.Width)
						|| (cursorPosition.Y > _selectorRegion.Bottom - _dragNodeHalfSize.Height))
					{
						break;
					}

					_selectorRegion = RectangleF.FromLTRB(_selectorRegion.Left,
														  cursorPosition.Y - _dragOffset.Y,
														  cursorPosition.X - _dragOffset.X,
														  _selectorRegion.Bottom);
					break;
				case DragSW:
					if ((cursorPosition.X > _selectorRegion.Right - _dragNodeHalfSize.Width)
						|| (cursorPosition.Y < _selectorRegion.Top + _dragNodeHalfSize.Height))
					{
						break;
					}

					_selectorRegion = RectangleF.FromLTRB(cursorPosition.X - _dragOffset.X,
														  _selectorRegion.Y,
														  _selectorRegion.Right,
														  cursorPosition.Y - _dragOffset.Y);
					break;
				case DragSE:
					if ((cursorPosition.X < _selectorRegion.Left + _dragNodeHalfSize.Width)
						|| (cursorPosition.Y < _selectorRegion.Top + _dragNodeHalfSize.Height))
					{
						break;
					}

					_selectorRegion = RectangleF.FromLTRB(_selectorRegion.X,
														  _selectorRegion.Y,
														  cursorPosition.X - _dragOffset.X,
														  cursorPosition.Y - _dragOffset.Y);
					break;
			}

			Constrain();

			UpdateNodeRegions();
		}

		/// <summary>
		/// Function to convert raw input pointing device args to win forms event args.
		/// </summary>
		/// <param name="args">Raw input device args.</param>
		/// <returns>Winforms event args.</returns>
		private static MouseEventArgs Convert(PointingDeviceEventArgs args)
		{
			var buttons = MouseButtons.None;

			if (args.Buttons.HasFlag(PointingDeviceButtons.Button1))
			{
				buttons |= MouseButtons.Left;
			}

			if (args.Buttons.HasFlag(PointingDeviceButtons.Button2))
			{
				buttons |= MouseButtons.Right;
			}

			if (args.Buttons.HasFlag(PointingDeviceButtons.Button3))
			{
				buttons |= MouseButtons.Middle;
			}

			return new MouseEventArgs(buttons, args.ClickCount, (int)args.Position.X, (int)args.Position.Y, args.WheelDelta);
		}

        /// <summary>
        /// Function to call in the mouse up event of the control being rendered into.
        /// </summary>
        /// <param name="e">Event parameters.</param>
        /// <returns><c>true</c> if handled, <c>false</c> if not.</returns>
        public bool OnMouseUp(PointingDeviceEventArgs e)
        {
            return OnMouseUp(Convert(e));
        }

		/// <summary>
		/// Function to call in the mouse up event of the control being rendered into.
		/// </summary>
		/// <param name="e">Event parameters.</param>
		/// <returns><c>true</c> if handled, <c>false</c> if not.</returns>
		public bool OnMouseUp(MouseEventArgs e)
		{
			if ((_clipRegion == null)
			    || (DragMode == ClipSelectionDragMode.None)
				|| (e.Button != MouseButtons.Left))
			{
				return false;
			}

			if (DragMode == ClipSelectionDragMode.Resize)
			{
				Constrain();
				UpdateNodeRegions();
			}

			DragMode = ClipSelectionDragMode.None;
			_dragOffset = Vector2.Zero;

			return true;
		}
        
        /// <summary>
        /// Function to call in the mouse down event of the control being rendered into.
        /// </summary>
        /// <param name="e">Event parameters.</param>
        /// <returns><c>true</c> if handled, <c>false</c> if not.</returns>
        public bool OnMouseDown(PointingDeviceEventArgs e)
	    {
	        return OnMouseDown(Convert(e));
	    }

		/// <summary>
		/// Function to call in the mouse down event of the control being rendered into.
		/// </summary>
		/// <param name="e">Event parameters.</param>
		/// <returns><c>true</c> if handled, <c>false</c> if not.</returns>
		public bool OnMouseDown(MouseEventArgs e)
		{
			if (_clipRegion == null)
			{
				return false;
			}

			HitTestNodes(e.Location);

			if ((e.Button != MouseButtons.Left)
			    || (DragMode != ClipSelectionDragMode.None)
			    || (((!_textureDisplayRegion.Contains(e.Location)) && (!NoBounds))
			        && (_selectedDragNode == -1)))
			{
				return false;
			}

			// Create a new clip selection if we click on the outside of the current selection area (and nodes).
			if ((!_selectorRegion.Contains(e.Location)) && (_selectedDragNode == -1))
		    {
				DragMode = ClipSelectionDragMode.Resize;
				_selectorRegion = new RectangleF(e.Location, new SizeF(DragNodeSize.Width, DragNodeSize.Height));
			    _selectedDragNode = DragSE;
				UpdateNodeRegions();
		        return true;
		    }

			if (_selectedDragNode == -1) 
			{
				_dragOffset = new Vector2(_selectorRegion.Left - e.Location.X, _selectorRegion.Top - e.Location.Y);
				DragMode = ClipSelectionDragMode.Move;
				
				return true;
			}

			DragMode = ClipSelectionDragMode.Resize;

			return true;
		}

		/// <summary>
		/// Function to call in the mouse move event of the control being rendered into.
		/// </summary>
		/// <param name="e">Event parameters.</param>
		/// <returns><c>true</c> if handled, <c>false</c> if not.</returns>
		public bool OnMouseMove(PointingDeviceEventArgs e)
		{
			return OnMouseMove(Convert(e));
		}
		
		/// <summary>
		/// Function to call in the mouse move event of the control being rendered into.
		/// </summary>
		/// <param name="e">Event parameters.</param>
		/// <returns><c>true</c> if handled, <c>false</c> if not.</returns>
		public bool OnMouseMove(MouseEventArgs e)
		{
			if (_clipRegion == null)
			{
				return false;
			}

			switch (DragMode)
			{
				case ClipSelectionDragMode.Move:
					_renderControl.Cursor = Cursors.SizeAll;

					MoveSelection(e.Location);
					return true;
				case ClipSelectionDragMode.Resize:
					SetCursor();

					MoveNode(e.Location);
					return true;
				default:
					HitTestNodes(e.Location);

					if (_selectedDragNode != -1)
					{
						// We've selected a dragging node, so change the cursor accordingly.
						SetCursor();
					}
					else
					{
						// If we're inside the selection area
						_renderControl.Cursor = _selectorRegion.Contains(e.Location) ? Cursors.SizeAll : DefaultCursor;
					}
					break;
			}

			return false;
		}

		/// <summary>
		/// Function to draw the clipper UI.
		/// </summary>
		public void Draw()
		{
			if (_clipRegion == null)
			{
				return;
			}

			// Animate the sprite texture.
			SelectionSprite.Size = _selectorRegion.Size;
			SelectionSprite.Position = _selectorRegion.Location;

			if (SelectionSprite.Texture != null)
			{
				Vector2 textureOffset = SelectionSprite.TextureOffset;
				float offset = SelectorAnimateSpeed * GorgonTiming.Delta;
				
				textureOffset.X += offset;
				textureOffset.Y += offset;

				if (textureOffset.X > 1)
				{
					textureOffset.X = textureOffset.X - 1.0f;
				}

				if (textureOffset.Y > 1)
				{
					textureOffset.Y = textureOffset.Y - 1.0f;
				}

				SelectionSprite.TextureRegion = new RectangleF(textureOffset,
				                                                new Vector2(_selectorRegion.Width /
				                                                            SelectionSprite.Texture.Settings.Width,
				                                                            _selectorRegion.Height /
				                                                            SelectionSprite.Texture.Settings.Height));
			}

			SelectionSprite.Draw();

			// Draw our nodes.
			if (HideNodes)
			{
				return;
			}

			_renderer.Drawing.FilledRectangle(_dragAreas[DragNW], _selectedDragNode == DragNW ? DragNodeHoverColor : DragNodeColor);
			_renderer.Drawing.FilledRectangle(_dragAreas[DragNE], _selectedDragNode == DragNE ? DragNodeHoverColor : DragNodeColor);
			_renderer.Drawing.FilledRectangle(_dragAreas[DragSW], _selectedDragNode == DragSW ? DragNodeHoverColor : DragNodeColor);
			_renderer.Drawing.FilledRectangle(_dragAreas[DragSE], _selectedDragNode == DragSE ? DragNodeHoverColor : DragNodeColor);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Clipper"/> class.
		/// </summary>
		/// <param name="renderer">The renderer to display the clipper UI.</param>
		/// <param name="renderControl">The control that will be rendered into.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="renderer"/> or the <paramref name="renderControl"/> parameter is NULL (Nothing in VB.Net).</exception>
		public Clipper(Gorgon2D renderer, Control renderControl)
		{
			if (renderer == null)
			{
				throw new ArgumentNullException("renderer");
			}

			if (renderControl == null)
			{
				throw new ArgumentNullException("renderControl");
			}

			_scale = new Vector2(1);
			_dragAreas = new RectangleF[8]; 
			SelectorAnimateSpeed = 0.5f;
			DragNodeSize = new Size(8, 8);
			DragNodeColor = new GorgonColor(0, 1, 1, 0.5f);
			DragNodeHoverColor = new GorgonColor(1, 0, 0, 0.5f);

			_renderControl = renderControl;
			_renderer = renderer;

			_textureSize = new Size(1, 1);

			SelectionSprite = renderer.Renderables.CreateSprite("SelectionSprite",
																  new GorgonSpriteSettings
																  {
																	  Color = new GorgonColor(0, 0, 0.8f, 0.5f),
																	  Size = new Vector2(1)
																  });

			SelectionSprite.TextureSampler.HorizontalWrapping = TextureAddressing.Wrap;
			SelectionSprite.TextureSampler.VerticalWrapping = TextureAddressing.Wrap;
		}
		#endregion
	}
}
