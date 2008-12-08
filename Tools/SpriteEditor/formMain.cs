#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Monday, May 07, 2007 4:53:17 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Linq;
using Drawing = System.Drawing;
using GorgonLibrary;
using GorgonLibrary.InputDevices;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Tools.Controls;
using ControlExtenders;
using Dialogs;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Main interface form for the sprite editor.
	/// </summary>
	public partial class formMain 
		: Form
	{
		#region Consts.
		// Sprite project file header version string.
		private const string _spriteProjectHeaderVersion = "SPRPROJ1.0";
		#endregion

		#region Variables.
		private Input _input = null;										// Input devices.
		private float _logoOpacity;											// Opacity.
		private Sprite _gorgonLogo = null;									// Gorgon logo.
		private bool _showLogo = true;										// Flag to show the logo.
		private Color _spriteBGColor = Color.DarkBlue;						// Sprite background color.
		private RenderTargetManager _renderTargetManager = null;			// Render target manager control.
		private Sprite _currentImage = null;								// Current image.
		private Image _patternImage = null;									// Pattern image.
		private Sprite _patternSprite = null;								// Pattern sprite.
		private Vector2D _mousePos = Vector2D.Zero;							// Mouse position.
		private RenderImage _backBuffer = null;								// Back buffer.
		private Sprite _backBufferSprite = null;							// Back buffer sprite.
		private Sprite _tempImageSprite = null;								// Temporary image sprite.
		private Random _rnd = new Random();									// Random number generator.
		private bool _spriteManagerVisible;									// Flag to indicate that the sprite manager is visible.
		private FormWindowState _lastState;									// Last window state.
		private FormBorderStyle _lastStyle;									// Last window style.
		private bool _imageManagerVisible;									// Flag to indicate whether the image manager is visible.
		private bool _targetManagerVisible;									// Flag to indicate whether the target manager is visible.
		private Viewport _clipView = null;									// Clipping viewport.
		private bool _constrainX;											// Flag to indicate a horizontal constraint.
		private bool _constrainY;											// Flag to indicate a vertical constraint.
		private Clipper _clipper = null;									// Clipper object.
		private Zoomer _zoomer = null;										// Zoomer object.
		private DockExtender _docker = null;								// Dock extender host.
		private string _projectName = string.Empty;							// Project name.
		private string _projectPath = string.Empty;							// Path to the project.
		private bool _projectChanged = false;								// Flag to indicate that the project has changes.
		private SpriteManager _spriteManager = null;						// Sprite manager.
		private ImageManager _imageManager = null;							// Image manager.
		private Drawing.Color _axisColor;									// Axis cross color.
		private static formMain _me = null;									// Form main.
		private bool _inAxisDrag = false;									// Flag to indicate we're in axis-dragging mode.
		private Vector2D _dragAxisStart = Vector2D.Zero;					// Axis drag start position.
		private bool _stopRendering = false;								// Flag to temporarily stop rendering.
		private static Font _mainfont = null;								// Main font.
		private bool _showBoundingBox = false;								// Flag to indicate whether to show the bounding box for the sprite.
		private bool _showBoundingCircle = false;							// Flag to indicate whether to show the bounding circle for the sprite.
		private Sprite _displaySprite = null;								// Sprite used for display - no animation.
		private Drawing.Color _clipBGColor = Drawing.Color.Transparent;		// Clipper background color.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the main font.
		/// </summary>
		internal static Font MainFont
		{
			get
			{
				return _mainfont;
			}
		}

		/// <summary>
		/// Property to set or return whether we should temporarily stop rendering.
		/// </summary>
		public bool StopRendering
		{
			get
			{
				return _stopRendering;
			}
			set
			{
				_stopRendering = value;
			}
		}

		/// <summary>
		/// Property to return this instance of form main.
		/// </summary>
		public static formMain Me
		{
			get
			{
				return _me;
			}
		}

		/// <summary>
		/// Property to return the sprite manager.
		/// </summary>
		public SpriteManager SpriteManager
		{
			get
			{
				return _spriteManager;
			}
		}

		/// <summary>
		/// Property to return the scrollbar offsets.
		/// </summary>
		public Vector2D ScrollOffset
		{
			get
			{
				return new Vector2D(scrollHorizontal.Value, scrollVertical.Value);
			}			
		}

		/// <summary>
		/// Property to return the image manager instance.
		/// </summary>
		public ImageManager ImageManager
		{
			get
			{
				return _imageManager;
			}
		}

		/// <summary>
		/// Property to return the render target manager interface.
		/// </summary>
		public RenderTargetManager RenderTargetManager
		{
			get
			{
				return _renderTargetManager;
			}
			set
			{
				_renderTargetManager = value;				
			}
		}

		/// <summary>
		/// Property to return the back buffer render image.
		/// </summary>
		public RenderImage BackBuffer
		{
			get
			{
				return _backBuffer;
			}
		}

		/// <summary>
		/// Property to return the clipper object.
		/// </summary>
		public Clipper Clipper
		{
			get
			{
				return _clipper;
			}
		}

		/// <summary>
		/// Property to return the dimensions of the display area minus the scroll bar width/height.
		/// </summary>
		public Vector2D DisplayDimensions
		{
			get
			{
				return new Vector2D(panelGorgon.Size.Width - scrollVertical.Width, panelGorgon.Size.Height - scrollHorizontal.Height);
			}
		}

		/// <summary>
		/// Property to set or return whether the project has changed or not.
		/// </summary>
		public bool ProjectChanged
		{
			get
			{
				if (_spriteManager.Sprites.HasChanges)
					_projectChanged = true;

				return _projectChanged;
			}
			set
			{
				_projectChanged = value;
				ValidateForm();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Click event of the buttonBackgroundColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonBackgroundColor_Click(object sender, EventArgs e)
		{
			ColorPicker picker = null;

			try
			{
				picker = new ColorPicker();
				picker.Color = _clipBGColor;

				if (picker.ShowDialog(this) == DialogResult.OK)
				{
					_clipBGColor = picker.Color;
					Settings.Root = "Clipping";
					Settings.SetSetting("ClipperBGColor", _clipBGColor.ToArgb().ToString());
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Error setting the background color.", ex);
			}
			finally
			{
				Settings.Root = null;
				if (picker != null)
					picker.Dispose();
				picker = null;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemAbout control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemAbout_Click(object sender, EventArgs e)
		{
			ABOOT aboutForm = null;			// About form.

			try
			{
				aboutForm = new ABOOT();
				aboutForm.ShowDialog(this);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the KeyDown event of the Keyboard control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:GorgonLibrary.Input.KeyboardInputEventArgs"/> instance containing the event data.</param>
		private void Keyboard_KeyDown(object sender, KeyboardInputEventArgs e)
		{
			// Do nothing if the fixed textboxes are focused and we're in clip mode.
			if (((textFixedHeight.Focused) || (textFixedWidth.Focused) || (textWindowSize.Focused)) && (_clipper.IsClippingStarted))
				return;

			// Enter keyboard mode.
			switch (e.Key)
			{
				case KeyboardKeys.OemQuestion:
					if ((_clipper.IsClippingStarted) && (_clipper.KeyboardMode) && (e.Shift))
						_clipper.IsHelpShowing = !_clipper.IsHelpShowing;					
					break;
				case KeyboardKeys.F1:
					if ((_clipper.IsClippingStarted) && (_clipper.KeyboardMode))
						_clipper.IsHelpShowing = !_clipper.IsHelpShowing;
					break;
				case KeyboardKeys.Space:
					if (!_clipper.KeyboardMode)
						buttonKeyboardMode.Checked = true;
					else
						buttonKeyboardMode.Checked = false;

					buttonKeyboardMode_Click(this, EventArgs.Empty);

					break;
				case KeyboardKeys.Enter:
					if (_clipper.KeyboardMode)
						_clipper.SetClipPoint(new Vector2D(_mousePos.X + scrollHorizontal.Value, _mousePos.Y + scrollVertical.Value));
					break;
				case KeyboardKeys.Escape:
					// Stop the clipping.
					if (_clipper.IsInSelection)
						_clipper.CancelSelection();
					else
					{
						if (_clipper.IsClippingStarted)
							ExitClipping();
					}
					break;
				case KeyboardKeys.F:
					if (_clipper.IsClippingStarted)
					{
						buttonFollowCursor.Checked = !buttonFollowCursor.Checked;
						buttonFollowCursor_Click(this, EventArgs.Empty);
					}
					break;
				case KeyboardKeys.Left:
				case KeyboardKeys.NumPad4:
					// Holding shift will move by 100 pixels, ctrl will move by 10 pixels.
					if (_clipper.KeyboardMode)
					{
						if (e.Shift)
							_mousePos.X -= 100.0f;
						else
						{
							if (e.Ctrl)
								_mousePos.X -= 10.0f;
							else
								_mousePos.X -= 1.0f;
						}
					}
					break;
				case KeyboardKeys.Right:
				case KeyboardKeys.NumPad6:
					// Holding shift will move by 100 pixels, ctrl will move by 10 pixels.
					if (_clipper.KeyboardMode)
					{
						if (e.Shift)
							_mousePos.X += 100.0f;
						else
						{
							if (e.Ctrl)
								_mousePos.X += 10.0f;
							else
								_mousePos.X += 1.0f;
						}
					}
					break;
				case KeyboardKeys.Up:
				case KeyboardKeys.NumPad8:
					// Holding shift will move by 100 pixels, ctrl will move by 10 pixels.
					if (_clipper.KeyboardMode)
					{
						if (e.Shift)
							_mousePos.Y -= 100.0f;
						else
						{
							if (e.Ctrl)
								_mousePos.Y -= 10.0f;
							else
								_mousePos.Y -= 1.0f;
						}
					}
					break;
				case KeyboardKeys.Down:
				case KeyboardKeys.NumPad2:
					// Holding shift will move by 100 pixels, ctrl will move by 10 pixels.
					if ((_clipper.KeyboardMode) && (!textFixedHeight.Focused) && (!textFixedWidth.Focused) && (!textWindowSize.Focused))
					{
						if (e.Shift)
							_mousePos.Y += 100.0f;
						else
						{
							if (e.Ctrl)
								_mousePos.Y += 10.0f;
							else
								_mousePos.Y += 1.0f;
						}
					}
					break;
				case KeyboardKeys.NumPad7:
				case KeyboardKeys.Home:
					// Holding shift will move by 100 pixels, ctrl will move by 10 pixels.
					if (_clipper.KeyboardMode)
					{
						if (e.Shift)
						{
							_mousePos.Y -= 100.0f;
							_mousePos.X -= 100.0f;
						}
						else
						{
							if (e.Ctrl)
							{
								_mousePos.Y -= 10.0f;
								_mousePos.X -= 10.0f;
							}
							else
							{
								_mousePos.Y -= 1.0f;
								_mousePos.X -= 1.0f;
							}
						}
					}
					break;
				case KeyboardKeys.NumPad1:
				case KeyboardKeys.End:
					// Holding shift will move by 100 pixels, ctrl will move by 10 pixels.
					if (_clipper.KeyboardMode)
					{
						if (e.Shift)
						{
							_mousePos.Y += 100.0f;
							_mousePos.X -= 100.0f;
						}
						else
						{
							if (e.Ctrl)
							{
								_mousePos.Y += 10.0f;
								_mousePos.X -= 10.0f;
							}
							else
							{
								_mousePos.Y += 1.0f;
								_mousePos.X -= 1.0f;
							}
						}
					}
					break;
				case KeyboardKeys.NumPad9:
				case KeyboardKeys.PageUp:
					// Holding shift will move by 100 pixels, ctrl will move by 10 pixels.
					if (_clipper.KeyboardMode)
					{
						if (e.Shift)
						{
							_mousePos.Y -= 100.0f;
							_mousePos.X += 100.0f;
						}
						else
						{
							if (e.Ctrl)
							{
								_mousePos.Y -= 10.0f;
								_mousePos.X += 10.0f;
							}
							else
							{
								_mousePos.Y -= 1.0f;
								_mousePos.X += 1.0f;
							}
						}
					}
					break;
				case KeyboardKeys.NumPad3:
				case KeyboardKeys.PageDown:
					// Holding shift will move by 100 pixels, ctrl will move by 10 pixels.
					if (_clipper.KeyboardMode)
					{
						if (e.Shift)
						{
							_mousePos.Y += 100.0f;
							_mousePos.X += 100.0f;
						}
						else
						{
							if (e.Ctrl)
							{
								_mousePos.Y += 10.0f;
								_mousePos.X += 10.0f;
							}
							else
							{
								_mousePos.Y += 1.0f;
								_mousePos.X += 1.0f;
							}
						}
					}
					break;
			}			
		}
				
		/// <summary>
		/// Function to exit the sprite clipping mode.
		/// </summary>
		private void ExitClipping()
		{
			// Restore the interface.
			_input.Keyboard.KeyDown -= new KeyboardInputEvent(Keyboard_KeyDown);
			_input.Keyboard.Enabled = false;
			_clipper.EndClipping();
			labelMiscInfo.Visible = false;
			stripSpriteClip.Visible = false;
			labelName.Visible = false;
			labelSpriteStart.Visible = false;
			labelSpriteSize.Visible = false;
			labelX.Visible = false;
			menuMain.Enabled = true;
			menuMain.Visible = true;			
			WindowState = _lastState;
			_spriteManager.Visible = _spriteManagerVisible;
			_imageManager.Visible = _imageManagerVisible;
			_renderTargetManager.Visible = _targetManagerVisible;
			panelGorgon.Cursor = Cursors.Default;
			_constrainX = false;
			_constrainY = false;
			scrollHorizontal.Cursor = Cursors.Default;
			scrollVertical.Cursor = Cursors.Default;

			// Update the current sprite.
			_spriteManager.RefreshList();

			panelGorgon.Focus();
		}

		/// <summary>
		/// Function to create a new project.
		/// </summary>
		/// <returns></returns>
		private bool NewProject()
		{
			try
			{
				Gorgon.Stop();

				ValidateForm();
				if (_projectChanged)
				{
					ConfirmationResult result = ConfirmationResult.None;		// Confirmation result.

					result = UI.ConfirmBox(this, "There are unsaved changes to this project.\nSave them now?", true, false);

					// Return if we cancel.
					if (result == ConfirmationResult.Cancel)
						return false;

					// Save the file if yes is chosen.
					if (result == ConfirmationResult.Yes)
						menuItemSaveAll_Click(this, EventArgs.Empty);
				}

				_projectPath = string.Empty;
				_projectName = "Untitled";

				// Remove sprites.
				_spriteManager.Clear();

				// Remove images.
				_imageManager.Clear();

				// Remove targets.
				_renderTargetManager.Clear();

				// Project is not changed.
				_projectChanged = false;

				return true;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Error clearing the data.", ex);
			}
			finally
			{
				ValidateForm();

				Gorgon.Go();
			}

			return false;
		}

		/// <summary>
		/// Handles the Click event of the menuItemNewProject control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemNewProject_Click(object sender, EventArgs e)
		{
			NewProject();
		}

		/// <summary>
		/// Function to display the gorgon logo.
		/// </summary>
		private void DisplayLogo(float frameTime)
		{
			VideoMode mode;			// Video mode.			

			try
			{
				// Hide controls.
				scrollHorizontal.Visible = false;
				panelVScroll.Visible = false;

				Gorgon.CurrentRenderTarget = null;
				Gorgon.Screen.BackgroundColor = Drawing.Color.FromArgb(248, 245, 222);

				// Get the current video mode.
				mode = Gorgon.CurrentVideoMode;

				// Draw the logo in the center.
				if (_logoOpacity < 255)
				{
					_logoOpacity += 85.33f * frameTime;
					_gorgonLogo.Opacity = (byte)(_logoOpacity);
				}
				else
					_gorgonLogo.Opacity = 255;

				_gorgonLogo.SetPosition(mode.Width / 2.0f, mode.Height / 2.0f);
				_gorgonLogo.Smoothing = Smoothing.Smooth;
				_gorgonLogo.UniformScale = mode.Width / 568.0f;
				_gorgonLogo.Draw();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Function to draw the pattern in the background.
		/// </summary>
		private void DrawPattern()
		{
			if (_currentImage == null)
				return;

			// Draw pattern.
			Gorgon.CurrentClippingViewport = _clipView;
			if (_clipBGColor.A < 255)
			{
				for (int y = 0; y < _backBuffer.Height; y += (int)_patternSprite.Height)
				{
					for (int x = 0; x < _backBuffer.Width; x += (int)_patternSprite.Width)
					{
						_patternSprite.SetPosition(x, y);
						_patternSprite.Draw();
					}
				}
			}

			if (_clipBGColor.A > 0)
			{
				Gorgon.CurrentRenderTarget.BeginDrawing();
				Gorgon.CurrentRenderTarget.BlendingMode = BlendingModes.Modulated;
				Gorgon.CurrentRenderTarget.FilledRectangle(0, 0, Gorgon.CurrentRenderTarget.Width - 1, Gorgon.CurrentRenderTarget.Height - 1, _clipBGColor);
				Gorgon.CurrentRenderTarget.BlendingMode = BlendingModes.None;
				Gorgon.CurrentRenderTarget.EndDrawing();
			}
			Gorgon.CurrentClippingViewport = null;
		}

		/// <summary>
		/// Function to display the current sprite.
		/// </summary>
		private void DisplayCurrentSprite(float frameTime)
		{
			Vector2D newPosition = Vector2D.Zero;		// New position.
			Vector2D oldScale = Vector2D.Unit;			// Old scale.
			Vector2D oldAxis = Vector2D.Zero;			// Old axis.
			Vector2D newScale = Vector2D.Unit;			// New scale.
			Vector2D panelSize = Vector2D.Zero;			// Panel size.

			// Hide controls.
			scrollHorizontal.Visible = false;
			panelVScroll.Visible = false;

			Gorgon.Screen.BackgroundColor = _spriteBGColor;
			
			panelSize.X = panelGorgon.Size.Width;
			panelSize.Y = panelGorgon.Size.Height;

			if (_spriteManager.CurrentSprite == null)
				_displaySprite = null;
			else
			{
				// Clone the current sprite and remove its animations.
				if (_displaySprite != _spriteManager.CurrentSprite.Sprite)
				{
					if (panelSize.X < _spriteManager.CurrentSprite.Sprite.Width)
						newScale.X = (panelSize.X / _spriteManager.CurrentSprite.Sprite.Width);
					if (panelSize.Y < _spriteManager.CurrentSprite.Sprite.Height)
						newScale.Y = (panelSize.Y / _spriteManager.CurrentSprite.Sprite.Height);

					// Square it.
					if (newScale.Y < newScale.X)
						newScale.X = newScale.Y;
					if (newScale.X < newScale.Y)
						newScale.Y = newScale.X;

					_displaySprite = _spriteManager.CurrentSprite.Sprite.Clone() as Sprite;
					_displaySprite.Animations.Clear();
					_displaySprite.Rotation = 0.0f;
					_displaySprite.Scale = newScale;
					_displaySprite.Position = new Vector2D(MathUtility.Round((panelSize.X / 2.0f) - (_displaySprite.ScaledWidth / 2.0f)),
						MathUtility.Round((panelSize.Y / 2.0f) - (_displaySprite.ScaledHeight / 2.0f)));
				}
			}

			// Set axis.
			if (_displaySprite != null)
			{
				oldAxis = _displaySprite.Axis;
				_displaySprite.Axis = Vector2D.Zero;
			}

			// If bound to a render target, then update the temp image.
			if ((_spriteManager.CurrentSprite.Sprite.Image != null) && (_spriteManager.CurrentSprite.Sprite.Image.ImageType == ImageType.RenderTarget))
				DrawTempImage(_spriteManager.CurrentSprite.Sprite.Image.RenderImage);
						
			if (_showLogo)
			{
				_gorgonLogo.SetPosition(Gorgon.CurrentVideoMode.Width / 2.0f, Gorgon.CurrentVideoMode.Height / 2.0f);
				_gorgonLogo.Smoothing = Smoothing.Smooth;
				_gorgonLogo.UniformScale = Gorgon.CurrentVideoMode.Width / 568;
				_gorgonLogo.Opacity = 64;
				_gorgonLogo.Draw();
			}

			if ((_showBoundingBox) || (_showBoundingCircle))
			{
				Gorgon.Screen.BeginDrawing();
				if (_showBoundingBox)
					Gorgon.Screen.Rectangle(_displaySprite.AABB.X, _displaySprite.AABB.Y,
						_displaySprite.AABB.Width, _displaySprite.AABB.Height, Drawing.Color.Red);
				if (_showBoundingCircle)
					Gorgon.Screen.Circle(_displaySprite.BoundingCircle.Center.X, _displaySprite.BoundingCircle.Center.Y, _displaySprite.BoundingCircle.Radius, Drawing.Color.Cyan);
				Gorgon.Screen.EndDrawing();
			}
						
			_displaySprite.Draw();

			// Draw a pointer to the hot-spot.
			Gorgon.Screen.BeginDrawing();
			newPosition = MathUtility.Round(Vector2D.Add(_displaySprite.FinalPosition, oldAxis));
			if (_axisColor == Drawing.Color.Red)
				_axisColor = Drawing.Color.Yellow;
			else
				_axisColor = Drawing.Color.Red;
			Gorgon.Screen.VerticalLine(newPosition.X, newPosition.Y - 5, 5, _axisColor);
			Gorgon.Screen.VerticalLine(newPosition.X, newPosition.Y + 1, 5, _axisColor);
			Gorgon.Screen.HorizontalLine(newPosition.X - 5, newPosition.Y, 5, _axisColor);
			Gorgon.Screen.HorizontalLine(newPosition.X + 1, newPosition.Y, 5, _axisColor);			
			Gorgon.Screen.EndDrawing();

			// Set axis.
			if (_displaySprite != null)
				_displaySprite.Axis = oldAxis;
		}

		/// <summary>
		/// Handles the AreaSelected event of the _clipper control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void clipper_AreaSelected(object sender, EventArgs e)
		{
			Drawing.RectangleF clipRegion = _clipper.ClipRectangle;		// Clipping region.

			clipRegion.Offset(-8.0f, -8.0f);
			_spriteManager.CurrentSprite.SetRegion(clipRegion);			
			ExitClipping();
		}

		/// <summary>
		/// Function to draw a temporary image within a render target image.
		/// </summary>
		/// <param name="image">Render target to draw into.</param>
		private void DrawTempImage(RenderImage image)
		{
			Drawing.Color color = Drawing.Color.Black;		// Rectangle color.			

			Gorgon.CurrentRenderTarget = image;

			image.Clear(Drawing.Color.FromArgb(255, 0, 255));

			_tempImageSprite.Width = image.Width;
			_tempImageSprite.Height = image.Height;
			_tempImageSprite.SetAxis(image.Width / 2.0f, image.Height / 2.0f);
			_tempImageSprite.SetPosition(image.Width / 2.0f, image.Height / 2.0f);

			_tempImageSprite.BlendingMode = BlendingModes.Modulated;
			_tempImageSprite.Rotation = 0.0f;
			_tempImageSprite.SetSpriteVertexColor(VertexLocations.LowerRight, Drawing.Color.FromArgb((byte)_rnd.Next(192) + 63, (byte)_rnd.Next(192) + 63, (byte)_rnd.Next(192) + 63, (byte)_rnd.Next(192) + 63));
			_tempImageSprite.Draw();

			_tempImageSprite.Rotation = 90.0f;
			_tempImageSprite.SetSpriteVertexColor(VertexLocations.LowerRight, Drawing.Color.FromArgb((byte)_rnd.Next(192) + 63, (byte)_rnd.Next(192) + 63, (byte)_rnd.Next(192) + 63, (byte)_rnd.Next(192) + 63));
			_tempImageSprite.Draw();

			_tempImageSprite.Rotation = 180.0f;
			_tempImageSprite.SetSpriteVertexColor(VertexLocations.LowerRight, Drawing.Color.FromArgb((byte)_rnd.Next(192) + 63, (byte)_rnd.Next(192) + 63, (byte)_rnd.Next(192) + 63, (byte)_rnd.Next(192) + 63));
			_tempImageSprite.Draw();

			_tempImageSprite.Rotation = 270.0f;
			_tempImageSprite.SetSpriteVertexColor(VertexLocations.LowerRight, Drawing.Color.FromArgb((byte)_rnd.Next(192) + 63, (byte)_rnd.Next(192) + 63, (byte)_rnd.Next(192) + 63, (byte)_rnd.Next(192) + 63));
			_tempImageSprite.Draw();

			image.BlendingMode = BlendingModes.Modulated;
			image.FilledCircle(image.Width / 2.0f, image.Height / 2.0f, image.Width / 4.0f, Drawing.Color.FromArgb(128, Drawing.Color.White));
			image.BlendingMode = BlendingModes.None;

			Gorgon.CurrentRenderTarget = null;
		}

		/// <summary>
		/// Function to display the currently selected image.
		/// </summary>
		/// <param name="frameTime">Time to render the frame.</param>
		private void DisplayCurrentImage(float frameTime)
		{
			Image image = null;						// Current image.
			int newWidth = 0;						// New scroller width.
			int newHeight = 0;						// New scroller height.
			bool imageChanged = false;				// Image changed flag.
			Vector2D position = Vector2D.Zero;		// Cursor position.

			// Constrain the axes.
			if (_clipper.IsClippingStarted)
			{
				if (_input.Keyboard.KeyStates[KeyboardKeys.ShiftKey] == KeyState.Down)
					_constrainX = true;
				else
					_constrainX = false;

				if (_input.Keyboard.KeyStates[KeyboardKeys.ControlKey] == KeyState.Down)
					_constrainY = true;
				else
					_constrainY = false;
			}

			// Limit the cursor.
			if (_mousePos.X <= 8)
				_mousePos.X = 8;
			if (_mousePos.Y <= 8)
				_mousePos.Y = 8;
			if (_mousePos.X >= Gorgon.CurrentVideoMode.Width - 10)
				_mousePos.X = Gorgon.CurrentVideoMode.Width - 10;
			if (_mousePos.Y >= Gorgon.CurrentVideoMode.Height - 10)
				_mousePos.Y = Gorgon.CurrentVideoMode.Height - 10;

			// Set cursor position.
			position = _mousePos;
			position.X += scrollHorizontal.Value - 8;
			position.Y += scrollVertical.Value - 8;

			Gorgon.Screen.BackgroundColor = Drawing.Color.FromArgb(35,68,122);

			// Get image.
			if (!_clipper.IsClippingStarted)
				image = _imageManager.SelectedImage;
			else
			{
				image = _spriteManager.CurrentSprite.Sprite.Image;

				if ((image != null) && (image.ImageType == ImageType.RenderTarget))
					DrawTempImage(image.RenderImage);
			}

			// Create the current image sprite.
			if (_currentImage == null)
			{
				_currentImage = new Sprite("CurrentImage", image, new Vector2D(image.Width, image.Height));
				imageChanged = true;
			}
			else
			{
				_currentImage.Image = image;

				if (_currentImage.Image != null)
				{				
					_currentImage.Width = image.Width;
					_currentImage.Height = image.Height;
				}
			}

			Gorgon.CurrentRenderTarget = _backBuffer;
			_backBuffer.Clear(Drawing.Color.FromArgb(35, 68, 122));

			// Draw the background.
			DrawPattern();

			scrollHorizontal.Visible = true;
			panelVScroll.Visible = true;

			// Update the scroll bars if the image changed.
			if (image != null)
			{
				if (image.Width <= Gorgon.CurrentVideoMode.Width)
				{
					scrollHorizontal.Value = 0;
					scrollHorizontal.Enabled = false;
				}
				else
				{
					scrollHorizontal.Enabled = true;
					newWidth = image.Width - Gorgon.CurrentVideoMode.Width + scrollHorizontal.LargeChange + scrollVertical.Width;
					if (scrollHorizontal.Maximum != newWidth)
					{
						scrollHorizontal.Maximum = newWidth;
						if (imageChanged)
							scrollHorizontal.Value = 0;
					}
				}

				if (image.Height <= Gorgon.CurrentVideoMode.Height)
				{
					scrollVertical.Value = 0;
					scrollVertical.Enabled = false;
				}
				else
				{
					scrollVertical.Enabled = true;
					newHeight = image.Height - Gorgon.CurrentVideoMode.Height + scrollVertical.LargeChange + scrollHorizontal.Height - 1;
					if (scrollVertical.Maximum != newHeight)
					{
						scrollVertical.Maximum = newHeight;
						if (imageChanged)
							scrollVertical.Value = 0;
					}
				}
			}
			else
			{
				scrollHorizontal.Enabled = true;
				scrollVertical.Enabled = true;
				newHeight = Gorgon.CurrentDriver.MaximumTextureHeight - Gorgon.CurrentVideoMode.Height + scrollVertical.LargeChange + 1;
				newWidth = Gorgon.CurrentDriver.MaximumTextureWidth - Gorgon.CurrentVideoMode.Width + scrollHorizontal.LargeChange + 1;

				if (scrollHorizontal.Maximum != newWidth)
					scrollHorizontal.Maximum = newWidth;
				if (scrollVertical.Maximum != newHeight)
					scrollVertical.Maximum = newHeight;
			}

			// Set the position.
			_currentImage.SetPosition(-scrollHorizontal.Value + 8, -scrollVertical.Value + 8);

			// Blit.
			if (image != null)
			{
				Gorgon.CurrentClippingViewport = _clipView;
				_currentImage.Draw();
				Gorgon.CurrentClippingViewport = null;
			}

			// Draw selection cursor.
			if ((panelGorgon.Focused) || (!_clipper.KeyboardMode))
				_clipper.DrawCursor(_mousePos, frameTime);

			// Flip the buffer to the front.
			Gorgon.CurrentRenderTarget = null;
			_backBufferSprite.Draw();

			// Show help window.
			// Update status bar.
			if (_clipper.IsClippingStarted)
			{
				if ((_clipper.IsHelpShowing) && (_clipper.KeyboardMode))
					_clipper.DrawHelp();

				_zoomer.Draw(_mousePos);

				if ((!_clipper.IsInSelection) && (!_clipper.IsFixedDimensions))
					labelSpriteStart.Text = position.X.ToString("0") + ", " + position.Y.ToString("0");
				else
				{
					Drawing.RectangleF dimensions = Drawing.RectangleF.Empty;		// Dimensions.

					dimensions = _clipper.GetClipDimensions(_mousePos);
					dimensions.Offset(-8.0f, -8.0f);

					// Update the labels.
					labelSpriteStart.Text = dimensions.X.ToString("0") + ", " + dimensions.Y.ToString("0");
					labelSpriteSize.Text = string.Format("{0:0}", dimensions.Right - 1.0f) + ", " + string.Format("{0:0}", dimensions.Bottom - 1.0f) + " (Width: " + dimensions.Width + ", Height: " + dimensions.Height + ")";
				}
			}
		}

		/// <summary>
		/// Handles the Idle event of the Gorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Graphics.FrameEventArgs"/> instance containing the event data.</param>
		private void Gorgon_Idle(object sender, FrameEventArgs e)
		{	
			Gorgon.Screen.Clear();

			if ((_spriteManager.CurrentSprite != null) && (!_clipper.IsClippingStarted) && (!_stopRendering))
				DisplayCurrentSprite(e.FrameDeltaTime);
			else
			{
				if (((_imageManager.SelectedImage == null) && (!_clipper.IsClippingStarted)) || (_stopRendering))
					DisplayLogo(e.FrameDeltaTime);
				else
					DisplayCurrentImage(e.FrameDeltaTime);
			}
		}


		/// <summary>
		/// Handles the DeviceLost event of the Gorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Gorgon_DeviceLost(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
		}

		/// <summary>
		/// Handles the DeviceReset event of the Gorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Gorgon_DeviceReset(object sender, EventArgs e)
		{
			try
			{
				// Re-adjust the back buffer render target.
				_backBuffer.SetDimensions(Gorgon.CurrentVideoMode.Width, Gorgon.CurrentVideoMode.Height);
				_backBufferSprite.SetImageRegion(0, 0, Gorgon.CurrentVideoMode.Width, Gorgon.CurrentVideoMode.Height);
				_clipView = new Viewport(8, 8, Gorgon.CurrentVideoMode.Width - 17, Gorgon.CurrentVideoMode.Height - 17);
				panelGorgon.BringToFront();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Function to load the settings for the application.
		/// </summary>
		private void GetSettings()
		{
			try
			{
				if (!Settings.Load("Config.XML"))
				{
					Settings.Root = "Paths";
					Settings.SetSetting("LastSpriteOpenPath", @".\");
					Settings.SetSetting("LastImageOpenPath", @".\");
					Settings.SetSetting("LastProjectOpenPath", @".\");
					Settings.Root = null;
					Settings.Root = "ImageManager";
					Settings.SetSetting("ImageEditorName", "Microsoft Paint");
					Settings.SetSetting("ImageEditorPath", Environment.SystemDirectory + @"\mspaint.exe");
					Settings.SetSetting("Visible", "true");
					Settings.Root = null;
					Settings.Root = "SpriteManager";
					Settings.SetSetting("Visible", "true");
					Settings.Root = null;
					menuItemSpriteManager.Checked = true;
					menuItemImageManager.Checked = true;
					return;
				}

				// Save position and height.
				Settings.Root = "MainWindow";

				// Save main window settings.
				Left = Convert.ToInt32(Settings.GetSetting("Left", Left.ToString()));
				Top = Convert.ToInt32(Settings.GetSetting("Top", Top.ToString()));
				Width = Convert.ToInt32(Settings.GetSetting("Width", Width.ToString()));
				Height = Convert.ToInt32(Settings.GetSetting("Height", Height.ToString()));

				// Set window state.
				WindowState = (FormWindowState)Enum.Parse(typeof(FormWindowState), Settings.GetSetting("State", WindowState.ToString()));

				Settings.Root = null;

				Settings.Root = "SpriteManager";
				menuItemSpriteManager.Checked = string.Compare(Settings.GetSetting("Visible", "false"), "true", true) == 0;
				panelSpriteManager.Width = Convert.ToInt32(Settings.GetSetting("SplitPosition", splitSpriteManager.SplitPosition.ToString()));
				Settings.Root = null;

				Settings.Root = "ImageManager";
				menuItemImageManager.Checked = string.Compare(Settings.GetSetting("Visible", "false"), "true", true) == 0;
				panelImageManager.Width = Convert.ToInt32(Settings.GetSetting("SplitPosition", splitImageManager.SplitPosition.ToString()));
				Settings.Root = null;

				Settings.Root = "TargetManager";
				menuItemTargetManager.Checked = string.Compare(Settings.GetSetting("Visible", "false"), "true", true) == 0;
				panelRenderTargetManager.Width = Convert.ToInt32(Settings.GetSetting("SplitPosition", splitRenderTargetManager.SplitPosition.ToString()));

				Settings.Root = "Clipping";
				_clipBGColor = Color.FromArgb(Convert.ToInt32(Settings.GetSetting("ClipperBGColor", "0")));

				Settings.Root = null;
				_showLogo = (string.Compare(Settings.GetSetting("ShowLogo", "True"), "true", true) == 0);
				_spriteBGColor = Color.FromArgb(255, Color.FromArgb(Convert.ToInt32(Settings.GetSetting("BGColor", "-16777077"))));


				_showBoundingBox = (string.Compare(Settings.GetSetting("ShowBoundingBox", "True"), "true", true) == 0);
				_showBoundingCircle = (string.Compare(Settings.GetSetting("ShowBoundingCircle", "True"), "true", true) == 0);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to read the settings file.", ex);
			}
			finally
			{
				Settings.Root = null;
			}
		}

		/// <summary>
		/// Handles the Docking event of the _spriteManager control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void _spriteManager_Docking(object sender, EventArgs e)
		{
			splitSpriteManager.BringToFront();
			panelMain.BringToFront();
			panelMain.Refresh();
		}

		/// <summary>
		/// Handles the Docking event of the _imageManager control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void _imageManager_Docking(object sender, EventArgs e)
		{
			splitImageManager.BringToFront();
			panelMain.BringToFront();
			panelMain.Refresh();
		}

		/// <summary>
		/// Handles the Docking event of the _renderTargetManager control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void _renderTargetManager_Docking(object sender, EventArgs e)
		{
			splitRenderTargetManager.BringToFront();
			panelMain.BringToFront();
			panelMain.Refresh();
		}

		/// <summary>
		/// Handles the SplitterMoved event of the splitImageManager control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.Windows.Forms.SplitterEventArgs"/> instance containing the event data.</param>
		private void splitImageManager_SplitterMoved(object sender, SplitterEventArgs e)
		{
			// Save the distance.
			Settings.Root = "ImageManager";
			Settings.SetSetting("SplitPosition", splitImageManager.SplitPosition.ToString());
			Settings.Root = null;

			// Restart rendering.
			Gorgon.Go();
		}

		/// <summary>
		/// Handles the SplitterMoving event of the splitImageManager control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.Windows.Forms.SplitterEventArgs"/> instance containing the event data.</param>
		private void splitImageManager_SplitterMoving(object sender, SplitterEventArgs e)
		{
			// Stop rendering until we finish moving.
			Gorgon.Stop();
		}

		/// <summary>
		/// Handles the SplitterMoved event of the splitRenderTargetManager control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.Windows.Forms.SplitterEventArgs"/> instance containing the event data.</param>
		private void splitRenderTargetManager_SplitterMoved(object sender, SplitterEventArgs e)
		{
			// Save the distance.
			Settings.Root = "TargetManager";
			Settings.SetSetting("SplitPosition", splitRenderTargetManager.SplitPosition.ToString());
			Settings.Root = null;
		}

		/// <summary>
		/// Handles the SplitterMoving event of the splitRenderTargetManager control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.Windows.Forms.SplitterEventArgs"/> instance containing the event data.</param>
		private void splitRenderTargetManager_SplitterMoving(object sender, SplitterEventArgs e)
		{
			// Stop rendering until we finish moving.
			Gorgon.Stop();
		}

		/// <summary>
		/// Handles the Click event of the menuItemExit control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemExit_Click(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Handles the MouseMove event of the splitMain_Panel1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void panelGorgon_MouseMove(object sender, MouseEventArgs e)
		{
			Vector2D spritePos = Vector2D.Zero;						// Sprite position.
			Drawing.Rectangle axisRect = Drawing.Rectangle.Empty;	// Drag rectangle.

			if (!_clipper.KeyboardMode)
			{
				if (!_constrainX)
					_mousePos.X = e.X;
				if (!_constrainY)
					_mousePos.Y = e.Y;
			}

			if ((_spriteManager.CurrentSprite != null) && (!_clipper.IsClippingStarted) && (!_spriteManager.CurrentSprite.HasKeysForTrack("Axis")) && (_displaySprite != null))
			{
				spritePos = Vector2D.Add(_displaySprite.FinalPosition, _displaySprite.Axis);
				axisRect = new Drawing.Rectangle((int)(spritePos.X - 6), (int)(spritePos.Y - 6), 12, 12);

				if (_inAxisDrag)
					_spriteManager.CurrentSprite.Sprite.Axis = _displaySprite.Axis = Vector2D.Subtract(e.Location, _displaySprite.Position);
				else
				{
					// Change the cursor to indicate.
					if (axisRect.Contains(e.Location))
						panelGorgon.Cursor = Cursors.Hand;
					else
						panelGorgon.Cursor = Cursors.Default;
				}
			}
		}

		/// <summary>
		/// Handles the SplitterMoved event of the splitMain control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.SplitterEventArgs"/> instance containing the event data.</param>
		private void splitSpriteManager_SplitterMoved(object sender, SplitterEventArgs e)
		{
			// Save the distance.
			Settings.Root = "SpriteManager";
			Settings.SetSetting("SplitPosition", splitSpriteManager.SplitPosition.ToString());
			Settings.Root = null;

			// Restart rendering.
			Gorgon.Go();
		}

		/// <summary>
		/// Handles the Click event of the buttonKeyboardMode control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonKeyboardMode_Click(object sender, EventArgs e)
		{
			_clipper.KeyboardMode = buttonKeyboardMode.Checked;

			if (buttonKeyboardMode.Checked)
			{
				panelGorgon.Cursor = Cursors.Default;
				labelMiscInfo.Text = "Keyboard mode enabled, press '?' to show key list.";
			}
			else
			{
				panelGorgon.Cursor = Cursors.Cross;
				labelMiscInfo.Text = "Mouse mode enabled.  Press ESC to stop selection/clipping.";
			}
		}

		/// <summary>
		/// Handles the MouseUp event of the panelGorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void panelGorgon_MouseUp(object sender, MouseEventArgs e)
		{
			// Stop drag.
			if (_inAxisDrag)
			{
				_inAxisDrag = false;

				// Update display.
				_spriteManager.CurrentSprite.Changed = true;
				_spriteManager.RefreshList();
				_spriteManager.RefreshPropertyGrid();
			}
		}

		/// <summary>
		/// Handles the MouseDown event of the splitMain_Panel1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void panelGorgon_MouseDown(object sender, MouseEventArgs e)
		{
			Vector2D spritePos = Vector2D.Zero;							// Sprite position.
			Drawing.Rectangle axisRect = Drawing.Rectangle.Empty;		// Axis drag rectangle.

			if ((!_clipper.KeyboardMode) && (_clipper.IsClippingStarted))
			{
				// Cancel.
				if (e.Button == System.Windows.Forms.MouseButtons.Right)
				{
					// Exit the selection rectangle if we're in it.  Otherwise, leave the entire clipping process.
					if (_clipper.IsInSelection)
						_clipper.CancelSelection();
					else
						ExitClipping();
					return;
				}

				// Set the clipping point.
				_clipper.SetClipPoint(new Vector2D(_mousePos.X + scrollHorizontal.Value, _mousePos.Y + scrollVertical.Value));
			}

			// Begin axis drag.
			if (_spriteManager.CurrentSprite != null)
			{
				if ((!_clipper.IsClippingStarted) && (!_inAxisDrag) && (e.Button == System.Windows.Forms.MouseButtons.Left) && (!_spriteManager.CurrentSprite.HasKeysForTrack("Axis")) && (_displaySprite != null))
				{
					// Axis drag rectangle.
					spritePos = Vector2D.Add(_displaySprite.FinalPosition, _displaySprite.Axis);
					axisRect = new Drawing.Rectangle((int)(spritePos.X - 6), (int)(spritePos.Y - 6), 12, 12);

					// If we're inside the drag rectangle, begin axis drag operation.
					if (axisRect.Contains(e.Location))
					{
						_inAxisDrag = true;
						_dragAxisStart = new Vector2D(e.X, e.Y);
					}
				}
			}
			else
				_inAxisDrag = false;

			panelGorgon.Focus();
		}

		/// <summary>
		/// Handles the Click event of the buttonFixedSize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonFixedSize_Click(object sender, EventArgs e)
		{
			_clipper.IsFixedDimensions = buttonFixedSize.Checked;
			ValidateForm();
		}

		/// <summary>
		/// Handles the Validating event of the textFixedHeight control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void textFixedHeight_Validating(object sender, CancelEventArgs e)
		{
			Vector2D size = Vector2D.Unit;		// Fixed dimensions.

			size = _clipper.FixedSize;

			if (!Utilities.IsNumeric(textFixedHeight.Text))
				e.Cancel = true;
			else
			{
				float.TryParse(textFixedHeight.Text, out size.Y);

				if ((size.Y <= 1.0f) || (size.Y >= (float)Gorgon.CurrentDriver.MaximumTextureHeight))
				{
					e.Cancel = true;
					return;
				}

				_clipper.FixedSize = size;
			}
		}

		/// <summary>
		/// Handles the Validating event of the textWindowSize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void textWindowSize_Validating(object sender, CancelEventArgs e)
		{
			float windowSize = 0.0f;			// Window size.

			windowSize = _zoomer.ZoomerWindowSize;

			if (!Utilities.IsNumeric(textWindowSize.Text))
				e.Cancel = true;
			else
			{
				float.TryParse(textWindowSize.Text, out windowSize);

				if ((windowSize<128.0f) || (windowSize>512.0f))
				{
					e.Cancel = true;
					return;
				}

				_zoomer.ZoomerWindowSize = windowSize;
			}
		}

		/// <summary>
		/// Handles the Leave event of the textWindowSize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void textWindowSize_Leave(object sender, EventArgs e)
		{
			textWindowSize_Validating(this, new CancelEventArgs());
		}

		/// <summary>
		/// Handles the Validating event of the textFixedWidth control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void textFixedWidth_Validating(object sender, CancelEventArgs e)
		{
			Vector2D size = Vector2D.Unit;		// Fixed dimensions.

			size = _clipper.FixedSize;

			if (!Utilities.IsNumeric(textFixedWidth.Text))
				e.Cancel = true;
			else
			{
				float.TryParse(textFixedWidth.Text, out size.X);

				if ((size.X <= 1.0f) || (size.X >= (float)Gorgon.CurrentDriver.MaximumTextureWidth))
				{
					e.Cancel = true;
					return;
				}

				_clipper.FixedSize = size;
			}
		}

		/// <summary>
		/// Handles the Leave event of the textFixedHeight control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void textFixedHeight_Leave(object sender, EventArgs e)
		{
			textFixedHeight_Validating(this, new CancelEventArgs());
		}

		/// <summary>
		/// Handles the Leave event of the textFixedWidth control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void textFixedWidth_Leave(object sender, EventArgs e)
		{
			textFixedWidth_Validating(this, new CancelEventArgs());
		}

		/// <summary>
		/// Handles the KeyDown event of the textFixedWidth control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void textFixedWidth_KeyDown(object sender, KeyEventArgs e)
		{
			CancelEventArgs args = new CancelEventArgs();
			if (e.KeyCode == Keys.Enter)
			{
				textFixedWidth_Validating(this, args);
				e.Handled = true;
				if (!args.Cancel)
					panelGorgon.Focus();
			}
		}

		/// <summary>
		/// Handles the KeyDown event of the textFixedHeight control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void textFixedHeight_KeyDown(object sender, KeyEventArgs e)
		{
			CancelEventArgs args = new CancelEventArgs();
			if (e.KeyCode == Keys.Enter)
			{
				textFixedHeight_Validating(this, args);
				e.Handled = true;
				if (!args.Cancel)
					panelGorgon.Focus();
			}
		}

		/// <summary>
		/// Handles the KeyDown event of the textWindowSize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void textWindowSize_KeyDown(object sender, KeyEventArgs e)
		{
			CancelEventArgs args = new CancelEventArgs();

			if (e.KeyCode == Keys.Enter)
			{
				textWindowSize_Validating(this, args);
				e.Handled = true;
				if (!args.Cancel)
					panelGorgon.Focus();
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonFollowCursor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonFollowCursor_Click(object sender, EventArgs e)
		{
			_zoomer.IsZoomerFollowingCursor = buttonFollowCursor.Checked;
		}

		/// <summary>
		/// Handles the Click event of the menuItemOpen control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemOpenProject_Click(object sender, EventArgs e)
		{
			try
			{
				Settings.Root = "Paths";
				dialogOpen.Title = "Open sprite project...";
				dialogOpen.InitialDirectory = Settings.GetSetting("LastProjectOpenPath", @".\");
				dialogOpen.Filter = "Gorgon Sprite Project (*.sprProj)|*.sprProj";
				dialogOpen.Multiselect = false;

				if (dialogOpen.ShowDialog(this) == DialogResult.OK)
				{
					// Confirm so we can save our changes.
					if (!NewProject())
						return;

					// Open the project.
					OpenProject(dialogOpen.FileName);
					Settings.SetSetting("LastProjectOpenPath", Path.GetDirectoryName(dialogOpen.FileName));
					_projectChanged = false;
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Error opening the project file.", ex);
			}
			finally
			{
				Settings.Root = null;
				Cursor.Current = Cursors.Default;
				ValidateForm();
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemSaveAs control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemSaveSpriteAs_Click(object sender, EventArgs e)
		{
			try
			{
				SaveSpriteAsDialog(_spriteManager.CurrentSprite, dialogSave);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to save the sprite.", ex);
			}
			finally
			{
				Settings.Root = null;
				_projectChanged = true;
				Cursor.Current = Cursors.Default;
				ValidateForm();
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemSave control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemSaveSprite_Click(object sender, EventArgs e)
		{
			try
			{
				Cursor.Current = Cursors.WaitCursor;
				// Save.
				_spriteManager.CurrentSprite.Save(null, _spriteManager.CurrentSprite.IsXML);
				_spriteManager.CurrentSprite.Changed = false;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to save the sprite.", ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateForm();
			}
		}

		/// <summary>
		/// Function to show the save sprite as.. dialog.
		/// </summary>
		/// <param name="doc">Sprite to save.</param>
		public void SaveSpriteAsDialog(SpriteDocument doc, SaveFileDialog dialog)
		{
			Settings.Root = "Paths";
			dialog.InitialDirectory = Settings.GetSetting("LastSpriteOpenPath", @".\");
			dialog.Filter = "Gorgon Sprite (*.gorSprite)|*.gorSprite|Gorgon XML sprite (*.xml)|*.xml|All files (*.*)|*.*";
			if (!doc.IsXML)
				dialog.FilterIndex = 1;
			else
				dialog.FilterIndex = 2;
			dialog.Title = "Save sprite (" + doc.Name + ") as.";
			if (doc.Sprite.Filename == string.Empty)
				dialog.FileName = doc.Name + ".gorSprite";
			else
				dialog.FileName = doc.Sprite.Filename;

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				Cursor.Current = Cursors.WaitCursor;
				doc.Save(dialog.FileName, dialog.FilterIndex != 1);
				Settings.SetSetting("LastSpriteOpenPath", Path.GetDirectoryName(dialog.FileName));
			}

			doc.Changed = false;
		}

		/// <summary>
		/// Function to save all sprites.
		/// </summary>
		private void SaveAllSprites()
		{
			// Save the sprites.				
			foreach (SpriteDocument doc in _spriteManager.Sprites)
			{
				if (doc.Sprite.Filename == string.Empty)
				{
					SaveSpriteAsDialog(doc, dialogSave);
					Settings.Root = null;
				}
				else
				{
					doc.Save(doc.Sprite.Filename, doc.IsXML);
					doc.Changed = false;
				}
			}

			_spriteManager.RefreshList();
		}

		/// <summary>
		/// Handles the Click event of the menuItemSaveAll control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemSaveAll_Click(object sender, EventArgs e)
		{
			try
			{
				Cursor.Current = Cursors.WaitCursor;

				// Save all the sprites.
				SaveAllSprites();

				// Save the project.
				if (_projectPath == string.Empty)
					menuItemSaveProjectAs_Click(this, EventArgs.Empty);
				else
					SaveProject(_projectPath);				
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to save the sprite.", ex);
			}
			finally
			{
				Settings.Root = null;
				Cursor.Current = Cursors.Default;
				ValidateForm();
			}
		}

		/// <summary>
		/// Handles the SplitterMoving event of the splitSpriteManager control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.Windows.Forms.SplitterEventArgs"/> instance containing the event data.</param>
		private void splitSpriteManager_SplitterMoving(object sender, SplitterEventArgs e)
		{
			// Stop rendering until we finish moving.
			Gorgon.Stop();
		}

		/// <summary>
		/// Handles the Click event of the menuItemSaveProject control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemSaveProject_Click(object sender, EventArgs e)
		{
			try
			{
				SaveProject(_projectPath);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "There was an error trying to save the project.", ex);
			}
			finally
			{
				ValidateForm();
			}
		}

		/// <summary>
		/// Function to open a sprite project.
		/// </summary>
		/// <param name="fileName">Filename and path of the sprite project.</param>
		private void OpenProject(string fileName)
		{
			XmlDocument projectDocument = new XmlDocument();		// XML document.
			XmlNode header = null;									// Header node.
			XmlNodeList nodeList = null;							// List of nodes.

			if (Path.GetExtension(fileName) == string.Empty)
				fileName += ".sprProj";


			// Open the document.			
			projectDocument.Load(fileName);
			Directory.SetCurrentDirectory(Path.GetDirectoryName(fileName));

			// Get header.
			header = projectDocument.SelectSingleNode("//Header");
			if ((header == null) || (string.Compare(header.InnerText, _spriteProjectHeaderVersion, true) != 0))
				throw new ApplicationException("The sprite project file is corrupt.");

			// Load dependencies first.

			// Load images.
			nodeList = projectDocument.SelectNodes("//Images/Path");

			// Begin loading.
			foreach (XmlNode node in nodeList)
			{
				try
				{
					if (Path.GetDirectoryName(node.InnerText) == @".")
						Image.FromFile(Path.GetDirectoryName(fileName) + @"\" + Path.GetFileName(node.InnerText));
					else
						Image.FromFile(Path.GetFullPath(node.InnerText));
				}
				catch (Exception ex)
				{
					UI.ErrorBox(this, "There was an error trying to load the dependency image '" + node.InnerText + "'.", ex);
				}
			}

			// Load render targets.
			nodeList = projectDocument.SelectNodes("//Targets/RenderTarget");

			// Begin loading.
			RenderImage target = null;
			foreach (XmlNode node in nodeList)
			{
				string targetName = string.Empty;		// Render target name.

				try
				{
					targetName = node.SelectSingleNode("//Name").InnerText;

					if ((targetName != string.Empty) && (!RenderTargetCache.Targets.Contains(targetName)))
					{
						target = new RenderImage(targetName, Convert.ToInt32(node.SelectSingleNode("//Width").InnerText),
								Convert.ToInt32(node.SelectSingleNode("//Height").InnerText), (ImageBufferFormats)Enum.Parse(typeof(ImageBufferFormats), node.SelectSingleNode("//Format").InnerText),
								string.Compare(node.SelectSingleNode("//UseDepth").InnerText, "true", true) == 0, string.Compare(node.SelectSingleNode("//UseStencil").InnerText, "true", true) == 0);
					}
				}
				catch (Exception ex)
				{
					UI.ErrorBox(this, "There was an error trying to create the dependency render target '" + targetName + "'.", ex);
				}
			}

			// Load images.
			nodeList = projectDocument.SelectNodes("//Sprites/Path");

			// Begin loading.
			foreach (XmlNode node in nodeList)
			{
				try
				{
					XmlAttribute includeInAnims = null;		// Include in animations flag.
					SpriteDocument doc = null;				// Sprite document.

					if (Path.GetDirectoryName(node.InnerText) == @".")
						doc = _spriteManager.Sprites.Load(Path.GetDirectoryName(fileName) + @"\" + Path.GetFileName(node.InnerText));
					else
						doc = _spriteManager.Sprites.Load(node.InnerText);

					if (node.Attributes.Count > 0)
					{
						includeInAnims = node.Attributes["IncludeInAnims"];
						if (includeInAnims != null)
						{
							if (string.Compare(includeInAnims.Value, "true", true) == 0)
								doc.IncludeInAnimations = true;
							else
								doc.IncludeInAnimations = false;
						}
					}
				}
				catch (Exception ex)
				{
					UI.ErrorBox(this, "There was an error trying to load the sprite '" + node.InnerText + "'.", ex);
				}
			}

			// Refresh the image manager list.
			_imageManager.RefreshList();
			// Refresh the sprite manager list.
			_spriteManager.RefreshList();
			// Refresh render target manager list.
			_renderTargetManager.RefreshList();

			// Select the first sprite if we can.
            if (_spriteManager.Items.Count != 0)
            {
                _spriteManager.Items[0].Selected = true;
                _spriteManager.UpdatePropertyGrid();
            }

			_projectName = Path.GetFileNameWithoutExtension(fileName);
			_projectPath = fileName;
		}

		/// <summary>
		/// Function to return the relative path compared to a given path.
		/// </summary>
		/// <param name="relativeTo">Path that we're relative to.</param>
		/// <param name="path">Absolute path to get relative path from.</param>
		/// <returns>The relative path.</returns>
		private string GetRelativePath(string relativeTo, string path)
		{
			string resultPath = string.Empty;					// Result path.

			if (string.IsNullOrEmpty(relativeTo))
				throw new ArgumentNullException("relativeTo");

			if (string.IsNullOrEmpty(path))
				return ".";

			// Make into proper paths.
			if (!relativeTo.EndsWith(Path.DirectorySeparatorChar.ToString()))
				relativeTo += Path.DirectorySeparatorChar;
			if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
				path += Path.DirectorySeparatorChar;

			// Convert the paths to URI format.
			if (!relativeTo.StartsWith("file://", StringComparison.InvariantCultureIgnoreCase))
				relativeTo = "file://" + relativeTo;
			if (!path.StartsWith("file://", StringComparison.InvariantCultureIgnoreCase))
				path = "file://" + Path.GetFullPath(path);

			// URIs for calculation.
			Uri relativeURI = new Uri(relativeTo, UriKind.Absolute);
			Uri pathURI = new Uri(path, UriKind.Absolute);

			resultPath = relativeURI.MakeRelativeUri(pathURI).ToString();

			if (resultPath == string.Empty)
				return @".\";
			return resultPath.Replace("/", Path.DirectorySeparatorChar.ToString());
		}

		/// <summary>
		/// Function to save the project.
		/// </summary>
		/// <param name="fileName">Name and path of the project.</param>
		/// <returns>TRUE if successful, FALSE if not.</returns>
		private void SaveProject(string fileName)
		{
			XmlDocument projectDocument = new XmlDocument();		// XML document.
			XmlElement element = null;								// XML element.			
			XmlElement root = null;									// Root XML element.
			XmlElement support = null;								// Supporting data XML element.	
			XmlElement subSupport = null;							// Sub support element.
			string sourceDir = string.Empty;						// Source directory.
			string destDir = string.Empty;							// Destination directory.


			// If the sprites have changes, ask to save them.
			if (SpriteManager.Sprites.HasChanges)
			{
				if (UI.ConfirmBox(this, "There have been changes made to the sprites.\nWould you like to save these changes?") == ConfirmationResult.Yes)
					SaveAllSprites();
			}

			if (Path.GetExtension(fileName) == string.Empty)
				fileName += ".sprProj";

			// Begin project document.
			projectDocument.AppendChild(projectDocument.CreateProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\""));
			root = projectDocument.CreateElement("SpriteProject");
			projectDocument.AppendChild(root);

			// Create header node.
			support = projectDocument.CreateElement("Header");
			support.InnerText = _spriteProjectHeaderVersion;
			root.AppendChild(support);

			// Add supporting data paths.
			support = projectDocument.CreateElement("Images");
			root.AppendChild(support);

			// Create image count.
			var imageList = ImageCache.Images.Where((image) => !image.IsResource);
			int imageCount = imageList.Count();

			element = projectDocument.CreateElement("Count");
			element.InnerText = imageCount.ToString();
			support.AppendChild(element);

			// Add each image.			
			foreach (Image image in imageList)
			{
				element = projectDocument.CreateElement("Path");
				element.InnerText = GetRelativePath(Path.GetDirectoryName(fileName), Path.GetDirectoryName(image.Filename)) + Path.GetFileName(image.Filename);
				support.AppendChild(element);
			}

			support = projectDocument.CreateElement("Targets");
			root.AppendChild(support);

			// Create count.
			var targetList = RenderTargetCache.Targets.Where(target => ValidRenderTarget(target.Name));
			int targetCount = targetList.Count();

			element = projectDocument.CreateElement("Count");
			element.InnerText = targetCount.ToString();
			support.AppendChild(element);

			// Add each image.
			foreach (RenderImage target in targetList)
			{
				subSupport = projectDocument.CreateElement("RenderTarget");
				support.AppendChild(subSupport);

				element = projectDocument.CreateElement("Name");
				element.InnerText = target.Name;
				subSupport.AppendChild(element);
				element = projectDocument.CreateElement("Width");
				element.InnerText = target.Width.ToString();
				subSupport.AppendChild(element);
				element = projectDocument.CreateElement("Height");
				element.InnerText = target.Width.ToString();
				subSupport.AppendChild(element);
				element = projectDocument.CreateElement("Format");
				element.InnerText = target.Format.ToString();
				subSupport.AppendChild(element);
				element = projectDocument.CreateElement("UseDepth");
				element.InnerText = target.UseDepthBuffer.ToString();
				subSupport.AppendChild(element);
				element = projectDocument.CreateElement("UseStencil");
				element.InnerText = target.UseStencilBuffer.ToString();
				subSupport.AppendChild(element);
			}

			support = projectDocument.CreateElement("Sprites");
			root.AppendChild(support);

			// Create count.
			var documents = SpriteManager.Sprites.Where((doc) => !string.IsNullOrEmpty(doc.Sprite.Filename));
			int spriteCount = documents.Count();

			element = projectDocument.CreateElement("Count");
			element.InnerText = SpriteManager.Sprites.Count.ToString();
			support.AppendChild(element);

			foreach (SpriteDocument doc in documents)
			{
				XmlAttribute animVisible;		// Animation visible.
				
				element = projectDocument.CreateElement("Path");
				animVisible = projectDocument.CreateAttribute("IncludeInAnims");
				animVisible.Value = doc.IncludeInAnimations.ToString();
				element.Attributes.Append(animVisible);
				element.InnerText = GetRelativePath(Path.GetDirectoryName(fileName), Path.GetDirectoryName(doc.Sprite.Filename)) + Path.GetFileName(doc.Sprite.Filename);
				support.AppendChild(element);
			}


			// Save the XML document.
			projectDocument.Save(fileName);

			// We're OK.
			_projectName = Path.GetFileNameWithoutExtension(fileName);
			if (!_spriteManager.Sprites.HasChanges)
				_projectChanged = false;
		}

		/// <summary>
		/// Handles the Click event of the menuItemSaveProjectAs control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemSaveProjectAs_Click(object sender, EventArgs e)
		{
			try
			{
				Settings.Root = "Paths";
				dialogSave.Title = "Save project as...";
				dialogSave.InitialDirectory = Settings.GetSetting("LastProjectOpenPath", @".\");
				dialogSave.Filter = "Sprite project (*.sprProj)|*.sprProj";
				dialogSave.FileName = _projectName + ".sprProj";

				if (dialogSave.ShowDialog(this) == DialogResult.OK)
				{
					Settings.Root = "Paths";
					Settings.SetSetting("LastProjectOpenPath", Path.GetDirectoryName(dialogSave.FileName));
					_projectPath = dialogSave.FileName;
					Settings.Root = null;

					SaveProject(dialogSave.FileName);
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "There was an error trying to save the project.", ex);
				_projectPath = string.Empty;
			}
			finally
			{
				ValidateForm();
				Settings.Root = null;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemPrefs control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemPrefs_Click(object sender, EventArgs e)
		{
			formPreferences prefs = null;		// Preferences.

			try
			{
				prefs = new formPreferences();
				prefs.GetSettings();
				if (prefs.ShowDialog() == DialogResult.OK)
				{
					_imageManager.GetSettings();

					// Update other settings.
					Settings.Root = null;
					_showLogo = (string.Compare(Settings.GetSetting("ShowLogo", "True"), "true", true) == 0);
					_spriteBGColor = Color.FromArgb(255, Color.FromArgb(Convert.ToInt32(Settings.GetSetting("BGColor", "-16777077"))));
					_showBoundingBox = (string.Compare(Settings.GetSetting("ShowBoundingBox", "True"), "true", true) == 0);
					_showBoundingCircle = (string.Compare(Settings.GetSetting("ShowBoundingCircle", "True"), "true", true) == 0);
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "There was an error trying to set the preferences.", ex);
			}
			finally
			{
				Settings.Root = null;

				if (prefs != null)
					prefs.Dispose();
				prefs = null;
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			if ((_clipper.IsClippingStarted) && (e.Delta != 0))
			{
				if (e.Delta > 0)
					_zoomer.ZoomAmount += 0.1f;
				else
				{
					_zoomer.ZoomAmount -= 0.1f;
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			string[] args = null;							// Command line arguments.
			string applicationDirectory = string.Empty;		// Application dir.
			base.OnLoad(e);

			try
			{
				// Leave design mode out of this.
				if (!DesignMode)
				{
					args = Environment.GetCommandLineArgs();

					// Initialize Gorgon.
					Gorgon.Initialize(true, true);

					// Load the input plug-in.
					if (args.Length > 0)
						applicationDirectory = Path.GetDirectoryName(args[0]);
					else
						applicationDirectory = Environment.CurrentDirectory;
					_input = Input.LoadInputPlugIn(applicationDirectory + @"\GorgonInput.dll", "Gorgon.RawInput");

					// Initialize and bind the main panel.
					_input.Bind(panelGorgon);
					_input.Keyboard.Enabled = false;
					_input.Mouse.Enabled = false;

					// Set the video mode.
					Gorgon.SetMode(panelGorgon);

					// Create main font.
					_mainfont = new Font("Arial9pt", "Arial", 9.0f, true, true);

					// Load the logo.
					Image.FromResource("GorgonLogo2", Properties.Resources.ResourceManager);
					_gorgonLogo = new Sprite("GorgonLogoSprite", ImageCache.Images["GorgonLogo2"]);
					_gorgonLogo.UniformScale = 2.0f;
					_gorgonLogo.SetAxis(_gorgonLogo.Width / 2.0f, _gorgonLogo.Height / 2.0f);
					_logoOpacity = 0.0f;

					// Load the pattern sprite + image.
					_patternImage = Image.FromResource("Pattern", Properties.Resources.ResourceManager);
					_patternSprite = new Sprite("Pattern", _patternImage);
					_patternSprite.BlendingMode = BlendingModes.None;
					_patternSprite.AlphaMaskFunction = CompareFunctions.Always;

					// Create the backbuffer render target.			
					_backBuffer = new RenderImage("___BACKBUFFER___", Gorgon.CurrentVideoMode.Width, Gorgon.CurrentVideoMode.Height, Image.ValidateFormat(ImageBufferFormats.BufferRGB888X8, ImageType.RenderTarget));
					_backBufferSprite = new Sprite("__BACKBUFFERSPRITE__", _backBuffer);

					// Create temp sprite.
					_tempImageSprite = new Sprite("TempSprite");
					_tempImageSprite.Color = Drawing.Color.Transparent;

					// Create the clipper.
					_clipper = new Clipper(this);
					_clipper.AreaSelected += new EventHandler(clipper_AreaSelected);

					// Create the zoomer.
					_zoomer = new Zoomer(this);

					Gorgon.Idle += new FrameEventHandler(Gorgon_Idle);
					Gorgon.DeviceReset += new EventHandler(Gorgon_DeviceReset);
					Gorgon.DeviceLost += new EventHandler(Gorgon_DeviceLost);

					// Create a clipper for the back buffer.
					_clipView = new Viewport(0, 0, Gorgon.CurrentVideoMode.Width - scrollVertical.Width, Gorgon.CurrentVideoMode.Height - scrollHorizontal.Height);

					Gorgon.Go();

					// Get the settings.
					GetSettings();
					_clipper.GetSettings();
					_zoomer.GetSettings();
					_imageManager.GetSettings();
					_imageManager.RefreshList();
					_renderTargetManager.RefreshList();
					_spriteManager.GetSettings();

					// Set default project name.
					_projectName = "Untitled";

					try
					{
						if ((args.Length > 1) && (!string.IsNullOrEmpty(args[1])))
							OpenProject(args[1]);
					}
					catch (Exception ex)
					{
						UI.ErrorBox(this, "There was an error opening the file '" + args[1] + "'.", ex);
					}

					ValidateForm();
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "There was an error while trying to start the application.", ex);
				Application.Exit();
			}
		}
	
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			string message = string.Empty;								// Confirmation message.
			ConfirmationResult result = ConfirmationResult.None;		// Confirmation result.

			if (!DesignMode)
			{
				try
				{
					// If we're in clipping mode, then just go back to the main interface.
					if (_clipper.IsClippingStarted)
					{
						_clipper.CancelSelection();
						ExitClipping();
						e.Cancel = true;
						return;
					}

					// Check for changes.
					if ((_spriteManager.Sprites.HasChanges) || (_projectChanged))
					{
						result = UI.ConfirmBox(this, "The project has changes that have not been saved.\nWould you like to save these changes?", true, false);

						// Save the sprites.
						if (result == ConfirmationResult.Yes)
							menuItemSaveAll_Click(this, EventArgs.Empty);

						// If we cancel, stop the exit process.
						if (result == ConfirmationResult.Cancel)
						{
							e.Cancel = true;
							return;
						}
					}
				}
				catch (Exception ex)
				{
					UI.ErrorBox(this, "Could not save the project file.", ex);
				}
			}

			base.OnFormClosing(e);

			if (!DesignMode)
			{				
				try
				{
					_spriteManager.SaveSettings();
					_imageManager.SaveSettings();

					if (_clipper.IsClippingStarted)
						ExitClipping();

					// Save position and height.
					Settings.Root = "MainWindow";
					// Save main window settings.
					if (WindowState != FormWindowState.Minimized)
						Settings.SetSetting("State", WindowState.ToString());

					if (WindowState == FormWindowState.Normal)
					{
						Settings.SetSetting("Left", Left.ToString());
						Settings.SetSetting("Top", Top.ToString());
						Settings.SetSetting("Width", Width.ToString());
						Settings.SetSetting("Height", Height.ToString());
					}
					Settings.Root = null;

					// Save the settings
					Settings.Save("Config.XML");

					// Remove the event.
					if (_clipper != null)
						_clipper.AreaSelected -= new EventHandler(clipper_AreaSelected);

					_renderTargetManager.Docking -= new EventHandler(_renderTargetManager_Docking);
					_spriteManager.Docking -= new EventHandler(_spriteManager_Docking);
					_imageManager.Docking -= new EventHandler(_imageManager_Docking);
				}
				catch (Exception ex)
				{
					UI.ErrorBox(this, "Could not save the settings file.", ex);
				}

				// Remove input system.
				if (_input != null)
					_input.Dispose();
				_input = null;

				// Close the application.
				Gorgon.Terminate();
			}
		}

		/// <summary>
		/// Function to enter the sprite clipping mode.
		/// </summary>
		public void EnterClipping()
		{
			_input.Keyboard.Enabled = true;
			_input.Keyboard.AllowBackground = false;
			_input.Keyboard.Exclusive = true;

			_input.Keyboard.KeyDown += new KeyboardInputEvent(Keyboard_KeyDown);
						
			_lastState = WindowState;
			_lastStyle = FormBorderStyle;
			_spriteManagerVisible = _spriteManager.Visible;
			_imageManagerVisible = _imageManager.Visible;
			_targetManagerVisible = _renderTargetManager.Visible;

			// Get fixed info.
			buttonFixedSize.Checked = _clipper.IsFixedDimensions;
			textFixedHeight.Text = _clipper.FixedSize.Y.ToString("0.0");
			textFixedWidth.Text = _clipper.FixedSize.X.ToString("0.0");
			textWindowSize.Text = _zoomer.ZoomerWindowSize.ToString("0.0");

			// Configure the interface.
			scrollHorizontal.Value = 0;
			scrollVertical.Value = 0;
			stripSpriteClip.Visible = true;
			_clipper.KeyboardMode = false;
			buttonKeyboardMode.Checked = false;
			labelMiscInfo.Text = "Mouse mode enabled.";
			labelMiscInfo.Visible = true;
			panelSpriteManager.Visible = false;
			menuMain.Enabled = false;
			menuMain.Visible = false;
			WindowState = FormWindowState.Maximized;
			_spriteManager.Visible = false;
			_imageManager.Visible = false;
			_renderTargetManager.Visible = false;
			panelGorgon.Cursor = Cursors.Cross;
			scrollHorizontal.Cursor = Cursors.Arrow;
			scrollVertical.Cursor = Cursors.Arrow;
			labelName.Text = "Sprite: " + _spriteManager.CurrentSprite.Name;
			labelName.Visible = true;
			labelSpriteStart.Visible = true;

			// Force focus to the main panel.
			panelGorgon.Focus();

			// Begin clipping.
			_clipper.StartClipping();
		}

		/// <summary>
		/// Function to refresh all panels and the main form.
		/// </summary>
		public void RefreshAll()
		{
			_spriteManager.RefreshList();
			_imageManager.RefreshList();
			_renderTargetManager.RefreshList();

			ValidateForm();
		}

		/// <summary>
		/// Function to validate the form.
		/// </summary>
		public void ValidateForm()
		{
			menuItemNewProject.Enabled = true;
			menuItemOpenProject.Enabled = true;
			menuItemSaveSpriteAs.Enabled = false;
			menuItemSaveSprite.Enabled = false;

			// Update manager status.
			if (_spriteManager != null)
				_spriteManager.Visible = menuItemSpriteManager.Checked;
			if (_imageManager != null)
				_imageManager.Visible = menuItemImageManager.Checked;
			if (_renderTargetManager != null)
				_renderTargetManager.Visible = menuItemTargetManager.Checked;

			// Display fixed size interface.
			if ((_clipper != null) && (_clipper.IsClippingStarted))
			{
				menuItemNewProject.Enabled = false;
				menuItemOpenProject.Enabled = false;

				labelSpriteStart.Visible = true;

				if (_clipper.IsFixedDimensions)
				{
					textFixedWidth.Enabled = true;
					textFixedHeight.Enabled = true;
					labelSpriteSize.Visible = true;
					labelX.Visible = true;
				}
				else
				{
					textFixedWidth.Enabled = false;
					textFixedHeight.Enabled = false;
					if (!_clipper.IsInSelection)
					{
						labelX.Visible = false;
						labelSpriteSize.Visible = false;
					}
					else
					{
						labelX.Visible = true;
						labelSpriteSize.Visible = true;
					}
				}
			}
			else
			{				
				labelSpriteStart.Visible = false;
				labelX.Visible = false;
				labelSpriteSize.Visible = false;
			}

			if ((_zoomer != null) && (_zoomer.IsZoomerFollowingCursor))
				buttonFollowCursor.Checked = true;
			else
				buttonFollowCursor.Checked = false;

			// If the sprites have changed, mark the project as changed.
			if (_spriteManager.Sprites.HasChanges)
				_projectChanged = true;

			if (_projectChanged)
			{
				menuItemSaveAll.Enabled = true;
				if (_projectPath != string.Empty)
					menuItemSaveProject.Enabled = true;
				else
					menuItemSaveProject.Enabled = false;
			}
			else
			{
				menuItemSaveAll.Enabled = false;
				menuItemSaveProject.Enabled = false;
			}

			// Check for updated sprites.
			if (_spriteManager.CurrentSprite != null)
			{
				menuItemSaveSpriteAs.Enabled = true;

				Text = "Sprite Editor - " + _projectName + " - " + _spriteManager.CurrentSprite.Name;
				if (_projectChanged)
				{
					Text += "*";
					if (_spriteManager.CurrentSprite.Sprite.Filename != string.Empty)
						menuItemSaveSprite.Enabled = true;
				}
			}
			else
			{
				Text = "Sprite Editor - " + _projectName;
				if (_projectChanged)
					Text += "*";
			}
		}

		/// <summary>
		/// Function to help filter out render targets when building lists.
		/// </summary>
		/// <param name="targetName">Name of the render target.</param>
		/// <returns>TRUE if valid, FALSE if not.</returns>
		public bool ValidRenderTarget(string targetName)
		{
			RenderImage target = null;		// Render target.

			// Ensure that the target is loaded and it's a render image.
			if (!RenderTargetCache.Targets.Contains(targetName))
				return false;

			if (!(RenderTargetCache.Targets[targetName] is RenderImage))
				return false;

			target = (RenderImage)RenderTargetCache.Targets[targetName];

			// Check against internal render targets first.
			if (target == _backBuffer)
				return false;

			return true;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formMain()
		{
			InitializeComponent();

			_me = this;

			// Create dock extenders.
			_docker = new DockExtender(containerMain.ContentPanel);

			// Create sprite manager floater.
			_spriteManager = new SpriteManager();
			_spriteManager.Dock = DockStyle.Fill;
			panelSpriteManager.Controls.Add(_spriteManager);
			_spriteManager.Docking += new EventHandler(_spriteManager_Docking);
			_spriteManager.BindToMainForm(this, menuItemSpriteManager);
			_spriteManager.BindToDocker(_docker, panelSpriteManager, splitSpriteManager);

			// Create image manager.
			_imageManager = new ImageManager();
			_imageManager.Dock = DockStyle.Fill;
			panelImageManager.Controls.Add(_imageManager);
			_imageManager.Docking += new EventHandler(_imageManager_Docking);
			_imageManager.BindToMainForm(this, menuItemImageManager);
			_imageManager.BindToDocker(_docker, panelImageManager, splitImageManager);

			// Create render target manager.
			_renderTargetManager = new RenderTargetManager();
			_renderTargetManager.Dock = DockStyle.Fill;
			panelRenderTargetManager.Controls.Add(_renderTargetManager);
			_renderTargetManager.Docking += new EventHandler(_renderTargetManager_Docking);
			_renderTargetManager.BindToMainForm(this, menuItemTargetManager);
			_renderTargetManager.BindToDocker(_docker, panelRenderTargetManager, splitRenderTargetManager);

			// Initial axis cross color.
			_axisColor = Drawing.Color.Red;
		}
		#endregion
	}
}