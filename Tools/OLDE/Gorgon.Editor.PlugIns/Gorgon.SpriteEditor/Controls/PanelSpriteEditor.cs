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
// Created: Wednesday, November 12, 2014 12:02:02 AM
// 
#endregion

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.SpriteEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.Input;
using GorgonLibrary.Math;
using GorgonLibrary.Renderers;
using GorgonLibrary.UI;
using SlimMath;

namespace GorgonLibrary.Editor.SpriteEditorPlugIn.Controls
{
	/// <summary>
	/// Mode for the sprite editor.
	/// </summary>
	public enum SpriteEditorMode
	{
		/// <summary>
		/// In view mode.
		/// </summary>
		View = 0,
		/// <summary>
		/// In edit mode.
		/// </summary>
		Edit = 1,
		/// <summary>
		/// The anchor is being dragged into a new position.
		/// </summary>
		AnchorDrag = 2,
		/// <summary>
		/// Automatically clip sprite.
		/// </summary>
		AutoClip = 3
	}

	/// <summary>
	/// Main UI for sprite editing.
	/// </summary>
	partial class PanelSpriteEditor 
		: ContentPanel
	{
		#region Classes.
		/// <summary>
		/// Data used for animating the anchor color.
		/// </summary>
		private class AnchorColorAnim
		{
			#region Variables.
			// Flag to indicate whether the animation has started or not.
			private bool _isStarted;
			// The last recorded time.
			private float _lastTime;
			// Last used color.
			private Vector3 _lastColor = new Vector3(1);
			// Next color.
			private Vector3 _nextColor;
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the color for the animation.
			/// </summary>
			public GorgonColor Color
			{
				get;
				private set;
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to animate the anchor color.
			/// </summary>
			public void Animate()
			{
				if (!_isStarted)
				{
					_lastTime = GorgonTiming.SecondsSinceStart;
					_isStarted = true;
					return;
				}

				float time = (GorgonTiming.SecondsSinceStart - _lastTime) * 4.0f;
				Vector3 result;
				Vector3.Lerp(ref _lastColor, ref _nextColor, time, out result);

				Color = new GorgonColor(result);

				if (time <= 1)
				{
					return;
				}

				_lastTime = GorgonTiming.SecondsSinceStart;
				_lastColor = _nextColor;
				_nextColor = new Vector3(GorgonRandom.RandomSingle(), GorgonRandom.RandomSingle(), GorgonRandom.RandomSingle());
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="AnchorColorAnim"/> class.
			/// </summary>
			public AnchorColorAnim()
			{
				_nextColor = new Vector3(GorgonRandom.RandomSingle(), GorgonRandom.RandomSingle(), GorgonRandom.RandomSingle());
			}
			#endregion
		}
		#endregion

		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		// The sprite being edited.
		private readonly GorgonSpriteContent _content;
		// The plug-in that created the content.
		private GorgonSpriteEditorPlugIn _plugIn;
		// The amount of zoom on the display.
		private float _zoom = 1.0f;
		// The image used for the anchor sprite.
		private GorgonTexture2D _anchorImage;
		// The sprite used to represent the anchor.
		private GorgonSprite _anchorSprite;
		// The animation data for the anchor.
		private AnchorColorAnim _anchorAnim;
		// The sprite position on screen.
		private Vector2 _spritePosition;
		// The half size of the sprite.
		private Point _halfSprite;
		// The half size of the screen.
		private Point _halfScreen;
		// Clipper interface for editing the sprite.
		private Clipper _clipper;
		// Window used to zoom in on the clipping area.
		private ZoomWindow _zoomWindow;
		// Font used for the zoom window.
		private GorgonFont _zoomFont;
		// Delta between the cursor and the anchor icon when dragging.
		private Point _dragDelta;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the sprite editor mode.
		/// </summary>
		public SpriteEditorMode SpriteEditorMode
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the ValueChanged event of the numericZoomAmount control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericZoomAmount_ValueChanged(object sender, EventArgs e)
		{
			GorgonSpriteEditorPlugIn.Settings.ZoomWindowScaleFactor = (float)numericZoomAmount.Value;

			if (_zoomWindow != null)
			{
				_zoomWindow.ZoomAmount = GorgonSpriteEditorPlugIn.Settings.ZoomWindowScaleFactor;
			}

			UpdateZoomWindow(panelSprite.PointToClient(Cursor.Position));
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericZoomWindowSize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericZoomWindowSize_ValueChanged(object sender, EventArgs e)
		{
			GorgonSpriteEditorPlugIn.Settings.ZoomWindowSize = (int)numericZoomWindowSize.Value;

			if (_zoomWindow != null)
			{
				_zoomWindow.ZoomWindowSize = new Vector2(GorgonSpriteEditorPlugIn.Settings.ZoomWindowSize);
			}

			UpdateZoomWindow(panelSprite.PointToClient(Cursor.Position));
		}

		/// <summary>
		/// Handles the Click event of the checkZoomSnap control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void checkZoomSnap_Click(object sender, EventArgs e)
		{
			GorgonSpriteEditorPlugIn.Settings.ZoomWindowSnap = checkZoomSnap.Checked;
			UpdateZoomWindow(panelSprite.PointToClient(Cursor.Position));
		}

		/// <summary>
		/// Function to update the position of the zoom window.
		/// </summary>
		/// <param name="position">The position of the mouse cursor.</param>
		private void UpdateZoomWindow(Vector2 position)
		{
			if ((_clipper == null) || (_zoomWindow == null))
			{
				return;
			}

			Vector2 spritePosition = _clipper.Offset;
	        Vector2 zoomPosition;
			Vector2 texturePos;

			Vector2.Subtract(ref position, ref spritePosition, out texturePos);
			
            _zoomWindow.Position = texturePos;

	        if (!GorgonSpriteEditorPlugIn.Settings.ZoomWindowSnap)
	        {
		        zoomPosition = new Vector2(position.X + 4,
		                                   position.Y + 4);

		        if ((zoomPosition.X + _zoomWindow.ZoomWindowSize.X) > panelSprite.ClientSize.Width - 4)
		        {
			        zoomPosition.X = position.X - _zoomWindow.ZoomWindowSize.X - 4;
		        }

		        if ((zoomPosition.Y + _zoomWindow.ZoomWindowSize.Y) > panelSprite.ClientSize.Height - 4)
		        {
			        zoomPosition.Y = position.Y - _zoomWindow.ZoomWindowSize.Y - 4;
		        }
	        }
	        else
	        {
		        Vector2 mousePosition = panelSprite.PointToClient(Cursor.Position);
		        zoomPosition = new Vector2(panelSprite.ClientSize.Width - _zoomWindow.ZoomWindowSize.X, 0);

		        if ((mousePosition.Y < _zoomWindow.ZoomWindowSize.Y) &&
		            (mousePosition.X > panelSprite.ClientSize.Width * 0.5f))
		        {
			        zoomPosition.Y = panelSprite.ClientSize.Height - _zoomWindow.ZoomWindowSize.Y;
		        }

		        if ((mousePosition.X >= zoomPosition.X) && (mousePosition.Y > panelSprite.ClientSize.Height * 0.5f))
		        {
			        zoomPosition.X = 0;
		        }
	        }

	        _zoomWindow.ZoomWindowLocation = zoomPosition;
        }

		/// <summary>
		/// Function to offset the cursor in edit mode by a specified amount.
		/// </summary>
		/// <param name="offset">Amount to move the cursor.</param>
		private void MoveEditCursor(Point offset)
		{
			if (SpriteEditorMode != SpriteEditorMode.Edit)
			{
				return;
			}

			if (ActiveControl != panelSprite)
			{
				panelSprite.Focus();
			}

			Point mousePosition = panelSprite.PointToClient(Cursor.Position);

			mousePosition.X += offset.X;
			mousePosition.Y += offset.Y;

			panelSprite_MouseMove(this, new MouseEventArgs(MouseButtons.Left, 1, mousePosition.X, mousePosition.Y, 0));

			Cursor.Position = panelSprite.PointToScreen(mousePosition);
		}

		/// <summary>
		/// Handles the PreviewKeyDown event of the panelSprite control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
		private void panelSprite_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			var offset = Point.Empty;
			int offsetAmount = 1;
			
			if (e.Shift)
			{
				offsetAmount = 100;
			}
			else if (e.Control)
			{
				offsetAmount = 10;
			}
			
			switch (e.KeyCode)
			{
				case Keys.NumPad8:
				case Keys.Up:
					offset.Y = -offsetAmount;
					MoveEditCursor(offset);
					break;
				case Keys.NumPad2:
				case Keys.Down:
					offset.Y = offsetAmount;
					MoveEditCursor(offset);
					break;
				case Keys.NumPad4:
				case Keys.Left:
					offset.X = -offsetAmount;
					MoveEditCursor(offset);
					break;
				case Keys.NumPad6:
				case Keys.Right:
					offset.X = offsetAmount;
					MoveEditCursor(offset);
					break;
				case Keys.NumPad1:
					offset.X = -offsetAmount;
					offset.Y = offsetAmount;
					MoveEditCursor(offset);
					break;
				case Keys.NumPad3:
					offset.X = offsetAmount;
					offset.Y = offsetAmount;
					MoveEditCursor(offset);
					break;
				case Keys.NumPad7:
					offset.X = -offsetAmount;
					offset.Y = -offsetAmount;
					MoveEditCursor(offset);
					break;
				case Keys.NumPad9:
					offset.X = offsetAmount;
					offset.Y = -offsetAmount;
					MoveEditCursor(offset);
					break;
				case Keys.Space:
					if (SpriteEditorMode != SpriteEditorMode.Edit)
					{
						return;
					}

					if (ActiveControl != panelSprite)
					{
						panelSprite.Focus();
					}

					Point mousePosition = panelSprite.PointToClient(Cursor.Position);
					var args = new MouseEventArgs(MouseButtons.Left, 1, mousePosition.X, mousePosition.Y, 0);

					if (_clipper.DragMode == ClipSelectionDragMode.None)
					{
						panelSprite_MouseDown(this, args);
					}
					else
					{
						panelSprite_MouseUp(this, args);
					}
					break;
			}
		}

		/// <summary>
		/// Handles the MouseDown event of the panelSprite control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelSprite_MouseDown(object sender, MouseEventArgs e)
		{
			// If we don't have focus, don't ruin our sprite trying to get it back.
			if (!panelSprite.Focused)
			{
				panelSprite.Focus();
				return;
			}

			try
			{
				switch (SpriteEditorMode)
				{
					case SpriteEditorMode.AutoClip:
						Cursor.Current = Cursors.WaitCursor;

						var autoClipper = new AutoClipper((Point)new Vector2(e.X - _content.TextureSprite.Position.X, e.Y - _content.TextureSprite.Position.Y));

						Rectangle clipRect = autoClipper.Clip(_content.Texture);

						// If we didn't get a clip area, then just leave.
						if (clipRect.IsEmpty)
						{
							return;
						}

						// Set the clipper position.
						_clipper.ClipRegion = clipRect;
						_content.Size = clipRect.Size;
						_content.TextureRegion = clipRect;
						break;
					case SpriteEditorMode.Edit:
						_clipper.OnMouseDown(e);
						break;
					case SpriteEditorMode.View:
						var anchorOffset = (Point)Vector2.Modulate(_anchorSprite.Anchor, _anchorSprite.Scale);
						var anchorRect = new Rectangle((Point)(_anchorSprite.Position - anchorOffset),
						                               (Size)_anchorSprite.ScaledSize);

						if (anchorRect.Contains(e.Location))
						{
							SpriteEditorMode = SpriteEditorMode.AnchorDrag;
							_dragDelta = new Point(e.X - anchorRect.X - anchorOffset.X, e.Y - anchorRect.Y - anchorOffset.Y);

							panelSprite.Cursor = Cursors.Hand;
						}
						break;
				}
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
		/// Handles the MouseUp event of the panelSprite control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelSprite_MouseUp(object sender, MouseEventArgs e)
		{
			// If we don't have focus, don't ruin our sprite trying to get it back.
			if (!panelSprite.Focused)
			{
				panelSprite.Focus();
				return;
			}

			try
			{
				switch (SpriteEditorMode)
				{
					case SpriteEditorMode.AnchorDrag:
						SpriteEditorMode = SpriteEditorMode.View;
						_dragDelta = Point.Empty;
						panelSprite.Cursor = Cursors.Default;
						_content.RefreshProperty("Anchor");
						break;
					case SpriteEditorMode.Edit:
						if (!_clipper.OnMouseUp(e))
						{
							return;
						}

						var newRegion = Rectangle.Round(_clipper.ClipRegion);
						_content.Size = newRegion.Size;
						_content.TextureRegion = newRegion;
						_clipper.TextureSize = _content.Texture.Settings.Size;
						_clipper.Scale = new Vector2(1);
						break;
				}
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
		/// Handles the MouseWheel event of the PanelSprite control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void PanelSprite_MouseWheel(object sender, MouseEventArgs e)
		{
			if (SpriteEditorMode != SpriteEditorMode.Edit)
			{
				return;
			}

			if (ActiveControl != panelSprite)
			{
				panelSprite.Focus();
			}

			if (e.Delta < 0)
			{
				if ((numericZoomAmount.Value - 0.5M) >= numericZoomAmount.Minimum)
				{
					numericZoomAmount.Value -= 0.5M;
				}
			}
			else
			{
				if (numericZoomAmount.Value + 0.5M <= numericZoomAmount.Maximum)
				{
					numericZoomAmount.Value += 0.5M;
				}
			}
		}

		/// <summary>
		/// Handles the MouseMove event of the panelSprite control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
		private void panelSprite_MouseMove(object sender, MouseEventArgs e)
		{
			var textureOffset = (Point)new Vector2((e.X - _content.TextureSprite.Position.X) / _zoom, (e.Y - _content.TextureSprite.Position.Y) / _zoom);

			labelMouseInfo.Text = string.Format(Resources.GORSPR_TEXT_MOUSE_INFO,
			                                    textureOffset.X,
			                                    textureOffset.Y,
			                                    _content.TextureRegion.X,
			                                    _content.TextureRegion.Y,
			                                    _content.TextureRegion.Right,
			                                    _content.TextureRegion.Bottom,
			                                    _content.TextureRegion.Width,
			                                    _content.TextureRegion.Height,
												_content.Anchor.X,
												_content.Anchor.Y);

			switch (SpriteEditorMode)
			{
				case SpriteEditorMode.View:
					var anchorRect = new Rectangle((Point)(_anchorSprite.Position - Vector2.Modulate(_anchorSprite.Anchor, _anchorSprite.Scale)),
												   (Size)_anchorSprite.ScaledSize);


					panelSprite.Cursor = anchorRect.Contains(e.Location) ? Cursors.Hand : Cursors.Default;
					break;
				case SpriteEditorMode.Edit:
					_clipper.OnMouseMove(e);
					UpdateZoomWindow(e.Location);
					break;
				case SpriteEditorMode.AnchorDrag:
					_content.Anchor = (Point)new Vector2((e.X - _spritePosition.X - _dragDelta.X) / _zoom, (e.Y - _spritePosition.Y - _dragDelta.Y) / _zoom);
					break;
			}
		}

		/// <summary>
		/// Function to create a zoom window.
		/// </summary>
		private void CreateZoomWindow()
		{
			if (_content.Texture == null)
			{
				return;
			}

			_zoomWindow = new ZoomWindow(_content.Renderer, _content.Texture)
			{
				Clipper = _clipper,
				BackgroundTexture = _content.BackgroundTexture,
				Position = _content.Texture.ToPixel(_content.Sprite.TextureOffset),
				ZoomAmount = GorgonSpriteEditorPlugIn.Settings.ZoomWindowScaleFactor,
				ZoomWindowSize = new Vector2(GorgonSpriteEditorPlugIn.Settings.ZoomWindowSize)
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
		}

		/// <summary>
		/// Handles the Click event of the buttonAutoClip control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonAutoClip_Click(object sender, EventArgs e)
		{
			try
			{

				if (SpriteEditorMode == SpriteEditorMode.AutoClip)
				{
					panelSprite.Cursor = Cursors.Default;
					SpriteEditorMode = SpriteEditorMode.View;
					buttonCenter_Click(this, e);
					return;
				}

				_content.TextureSprite.Scale = new Vector2(1);
				_clipper.NoBounds = true;
				_clipper.ClipRegion = new RectangleF(_content.TextureRegion.Location, _content.Size);
				_clipper.TextureSize = _content.Texture.Settings.Size;
				_clipper.Offset = new Vector2(_halfScreen.X - _content.Size.Width / 2.0f - _content.TextureRegion.X,
											  _halfScreen.Y - _content.Size.Height / 2.0f - _content.TextureRegion.Y);
				_clipper.HideNodes = true;
				_halfSprite = new Point(_content.Size.Width / 2, _content.Size.Height / 2);


				SpriteEditorMode = SpriteEditorMode.AutoClip;

				scrollHorizontal.Value = _content.TextureRegion.X;
				scrollVertical.Value = _content.TextureRegion.Y;

				CalculateSpritePosition();

				panelSprite.Cursor = Cursors.Cross;
				panelSprite.Focus();
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
		/// Handles the Click event of the buttonClip control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonClip_Click(object sender, EventArgs e)
		{
			try
			{
				if (SpriteEditorMode == SpriteEditorMode.Edit)
				{
					_zoomWindow = null;
					panelSprite.Cursor = Cursors.Default;
					panelZoomControls.Visible = false;
					SpriteEditorMode = SpriteEditorMode.View;
					buttonCenter_Click(this, e);
					return;
				}

				// If the texture rectangle and the sprite size are not equal, then inform the user that we
				// may need to make a destructive change to their sprite.
				if ((_content.Size.Width != _content.TextureRegion.Width)
				    || (_content.Size.Height != _content.TextureRegion.Height))
				{
					if (GorgonDialogs.ConfirmBox(ParentForm, Resources.GORSPR_DLG_CONFIRM_SPREDIT) == ConfirmationResult.No)
					{
						return;
					}

					// Reset the texture region to match the size of the sprite.
					_content.TextureRegion = new Rectangle(_content.TextureRegion.Location, _content.Size);
				}

				_content.TextureSprite.Scale = new Vector2(1);
				panelZoomControls.Visible = true;
				_clipper.NoBounds = true;
				_clipper.ClipRegion = new RectangleF(_content.TextureRegion.Location, _content.Size);
				_clipper.TextureSize = _content.Texture.Settings.Size;
				_clipper.Offset = new Vector2(_halfScreen.X - _content.Size.Width / 2.0f - _content.TextureRegion.X,
				                              _halfScreen.Y - _content.Size.Height / 2.0f - _content.TextureRegion.Y);
				_clipper.HideNodes = false;
				_halfSprite = new Point(_content.Size.Width / 2, _content.Size.Height / 2);

				CreateZoomWindow();

				SpriteEditorMode = SpriteEditorMode.Edit;

				scrollHorizontal.Value = _content.TextureRegion.X;
				scrollVertical.Value = _content.TextureRegion.Y;

				CalculateSpritePosition();

				UpdateZoomWindow(Cursor.Position);

				// Force focus on the panel.
				panelSprite.Focus();
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
		/// Handles the Click event of the buttonCenter control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonCenter_Click(object sender, EventArgs e)
		{
			if (SpriteEditorMode == SpriteEditorMode.Edit)
			{
				_halfSprite = new Point(_content.Size.Width / 2, _content.Size.Height / 2);
				scrollHorizontal.Value = _content.TextureRegion.X;
				scrollVertical.Value = _content.TextureRegion.Y;
			}
			else
			{
				scrollHorizontal.Value = 0;
				scrollVertical.Value = 0;
			}

			CalculateSpritePosition();
		}

		/// <summary>
		/// Handles the Resize event of the panelSprite control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void panelSprite_Resize(object sender, EventArgs e)
		{
			if (menuItemToWindow.Checked)
			{
				CalculateToWindow();
			}

			SetUpScrolling();

			_halfScreen = (Point)(new Vector2(panelSprite.ClientSize.Width / 2.0f, panelSprite.ClientSize.Height / 2.0f));

			CalculateSpritePosition();
		}

		/// <summary>
		/// Function to set up scrolling.
		/// </summary>
		private void SetUpScrolling()
		{
			Size panelSize = panelOuter.ClientSize;

			// Turn off the resizing event so we don't get recursive calls.
			panelSprite.Resize -= panelSprite_Resize;
			scrollHorizontal.Scroll -= OnScroll;
			scrollVertical.Scroll -= OnScroll;

			try
			{
				if ((_content == null)
					|| (_content.Sprite == null)
					|| (_content.Texture == null))
				{
					panelHScroll.Visible = panelVScroll.Visible = false;
					panelSprite.ClientSize = panelSprite.ClientSize;
					return;
				}

				scrollHorizontal.LargeChange = 50;
				scrollVertical.LargeChange = 50;

				var size = new Size(ContentObject.Graphics.Textures.MaxWidth / 2 - scrollHorizontal.LargeChange, ContentObject.Graphics.Textures.MaxHeight / 2 - scrollVertical.LargeChange);
				scrollHorizontal.Minimum = -size.Width;
				scrollVertical.Minimum = -size.Height;
				scrollHorizontal.Maximum = size.Width;
				scrollVertical.Maximum = size.Height;

				panelHScroll.Visible = true;
				panelVScroll.Visible = true;

				panelSize.Width = panelSize.Width - panelVScroll.ClientSize.Width;
				panelSize.Height = panelSize.Height - panelHScroll.ClientSize.Height;

				panelSprite.ClientSize = panelSize;
			}
			finally
			{
				// Re-enable resizing.
				panelSprite.Resize += panelSprite_Resize;
				scrollHorizontal.Scroll += OnScroll;
				scrollVertical.Scroll += OnScroll;
			}
		}

		/// <summary>
		/// Function called when a scrollbar is moved.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="ScrollEventArgs"/> instance containing the event data.</param>
		private void OnScroll(object sender, ScrollEventArgs e)
		{
			if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
			{
				scrollVertical.Value = e.NewValue;
			}

			if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
			{
				scrollHorizontal.Value = e.NewValue;
			}

			CalculateSpritePosition();

			_content.Draw();

			if (SpriteEditorMode == SpriteEditorMode.Edit)
			{
				UpdateZoomWindow(panelSprite.PointToClient(Cursor.Position));
			}
		}

		/// <summary>
		/// Function to validate the controls on the panel.
		/// </summary>
		private void ValidateControls()
		{
			buttonSave.Enabled = buttonRevert.Enabled = _content.HasChanges;
			buttonClip.Enabled = _content != null && _content.Texture != null && SpriteEditorMode != SpriteEditorMode.AutoClip;
			// This functionality only works 32 bit images with an alpha component.
			buttonAutoClip.Enabled = _content != null && _content.Texture != null && SpriteEditorMode != SpriteEditorMode.Edit
			                         && (_content.Texture.FormatInformation.Format == BufferFormat.B8G8R8A8_UIntNormal
			                             || _content.Texture.FormatInformation.Format == BufferFormat.B8G8R8A8_UIntNormal_sRGB
			                             || _content.Texture.FormatInformation.Format == BufferFormat.R8G8B8A8_Int
			                             || _content.Texture.FormatInformation.Format == BufferFormat.R8G8B8A8_IntNormal
			                             || _content.Texture.FormatInformation.Format == BufferFormat.R8G8B8A8_UIntNormal
			                             || _content.Texture.FormatInformation.Format == BufferFormat.R8G8B8A8_UIntNormal_sRGB);
			dropDownZoom.Enabled = SpriteEditorMode == SpriteEditorMode.View || SpriteEditorMode == SpriteEditorMode.AnchorDrag;
		}

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
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonRevert control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonRevert_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				if (GorgonDialogs.ConfirmBox(ParentForm, Resources.GORSPR_DLG_REVERT_CONFIRM) == ConfirmationResult.No)
				{
					return;
				}

				_content.Revert();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
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
				foreach (var control in items.Cast<ToolStripMenuItem>().Where(control => control != sender))
				{
					control.Checked = false;
				}

				_zoom = float.Parse(selectedItem.Tag.ToString(),
				                    NumberStyles.Float,
				                    NumberFormatInfo.InvariantInfo);

				dropDownZoom.Text = string.Format("{0}: {1}", Resources.GORSPR_TEXT_ZOOM, selectedItem.Text);

				if (menuItemToWindow.Checked)
				{
					CalculateToWindow();
				}

				SetUpScrolling();

				_content.Sprite.Scale = new Vector2(_zoom);
				
				CalculateSpritePosition();
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
		/// Function to perform localization on the control text properties.
		/// </summary>
		protected override void LocalizeControls()
		{
			Text = Resources.GORSPR_CONTENT_TYPE;

			buttonSave.Text = Resources.GORSPR_TEXT_SAVE;
			buttonRevert.Text = Resources.GORSPR_TEXT_REVERT;

			buttonClip.Text = Resources.GORSPR_TEXT_CLIP_SPRITE;
			buttonAutoClip.Text = Resources.GORSPR_TEXT_AUTO_CLIP;

			menuItemToWindow.Text = Resources.GORSPR_TEXT_TO_WINDOW;
			menuItem1600.Text = 16.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem800.Text = 8.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem400.Text = 4.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem200.Text = 2.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem100.Text = 1.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem75.Text = 0.75.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem50.Text = 0.5.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem25.Text = 0.25.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);

			labelZoomAmount.Text = Resources.GORSPR_TEXT_ZOOM_AMOUNT;
			labelZoomSize.Text = Resources.GORSPR_TEXT_ZOOM_WINDOW_SIZE;
			checkZoomSnap.Text = Resources.GORSPR_TEXT_SNAP_ZOOM;

			labelMouseInfo.Text = string.Format(Resources.GORSPR_TEXT_MOUSE_INFO, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

			numericZoomAmount.Value = (decimal)GorgonSpriteEditorPlugIn.Settings.ZoomWindowScaleFactor;
			numericZoomWindowSize.Value = GorgonSpriteEditorPlugIn.Settings.ZoomWindowSize;
			checkZoomSnap.Checked = GorgonSpriteEditorPlugIn.Settings.ZoomWindowSnap;

			dropDownZoom.Text = string.Format("{0}: {1}", Resources.GORSPR_TEXT_ZOOM, menuItem100.Text);
		}

		/// <summary>
		/// Function to calculate the zoom required to scale the sprite to the window.
		/// </summary>
		private void CalculateToWindow()
		{
			var windowScale = new Vector2(panelSprite.ClientSize.Width / _content.Sprite.Size.X, panelSprite.ClientSize.Height / _content.Sprite.Size.Y);

			_zoom = windowScale.Y < windowScale.X ? windowScale.Y : windowScale.X;
		}

		/// <summary>
		/// Function to calculate the sprite position in view mode.
		/// </summary>
		private void CalculateSpritePosition()
		{
			if ((_content == null)
				|| (_content.Sprite == null))
			{
				return;
			}

			Vector2 imageScale = _content.TextureSprite.Size;

			switch (SpriteEditorMode)
			{
				case SpriteEditorMode.AutoClip:
				case SpriteEditorMode.Edit:
					_content.TextureSprite.Position = new Vector2(_halfScreen.X - _halfSprite.X - scrollHorizontal.Value, _halfScreen.Y - _halfSprite.Y - scrollVertical.Value);
					_clipper.Offset = _content.TextureSprite.Position;
					break;
				default:
					_content.Sprite.Scale = new Vector2(_zoom);

					imageScale = new Vector2(_content.Sprite.ScaledSize.X / _content.TextureRegion.Width, _content.Sprite.ScaledSize.Y / _content.TextureRegion.Height);
					
					_halfSprite = (Point)(new Vector2(_content.Sprite.ScaledSize.X / 2.0f, _content.Sprite.ScaledSize.Y / 2.0f));

					_spritePosition = new Vector2(_halfScreen.X - _halfSprite.X - (scrollHorizontal.Value * _zoom),
					                              _halfScreen.Y - _halfSprite.Y - (scrollVertical.Value * _zoom));

					_content.TextureSprite.Position =
						(Point)new Vector2(_spritePosition.X - _content.TextureRegion.X * imageScale.X, _spritePosition.Y - _content.TextureRegion.Y * imageScale.Y);

					imageScale = Vector2.Modulate(imageScale, _content.TextureSprite.Size);

					break;
			}

			_content.TextureSprite.ScaledSize = (Point)imageScale;
		}

		/// <summary>
		/// Function to draw the sprite texture.
		/// </summary>
		private void DrawSpriteTexture()
		{
			switch (SpriteEditorMode)
			{
				case SpriteEditorMode.AutoClip:
				case SpriteEditorMode.Edit:
					_content.TextureSprite.Opacity = 1.0f;
					break;
				default:
					_content.TextureSprite.Opacity = 0.35f;
					break;
			}

			_content.TextureSprite.SmoothingMode = SmoothingMode.Smooth;
			_content.TextureSprite.Draw();
		}

		/// <summary>
		/// Function to draw the sprite editor functionality.
		/// </summary>
		private void DrawSpriteEdit()
		{
			_content.Sprite.Scale = new Vector2(1);			

			DrawSpriteTexture();

			// Offset the clipping rectangle so that it appears in the correct place on the screen.
			_clipper.Draw();

			Vector2 mousePosition = panelSprite.PointToClient(Cursor.Position);
				 
			_content.Renderer.Drawing.BlendingMode = BlendingMode.Inverted;

			_content.Renderer.Drawing.DrawLine(new Vector2(mousePosition.X, 0), new Vector2(mousePosition.X, panelSprite.ClientSize.Height), Color.White);
			_content.Renderer.Drawing.DrawLine(new Vector2(0, mousePosition.Y), new Vector2(panelSprite.ClientSize.Width, mousePosition.Y), Color.White);

			_content.Renderer.Drawing.BlendingMode = BlendingMode.Modulate;

			if (_zoomWindow != null)
			{
				_zoomWindow.Draw();
			}

			if (panelSprite.Focused)
			{
				return;
			}

			// If the window does not have focus, then draw an overlay.
			_content.Renderer.Drawing.FilledRectangle(panelSprite.ClientRectangle, new GorgonColor(panelSprite.BackColor, 0.5f));
		}

		/// <summary>
		/// Function to draw the current sprite.
		/// </summary>
		private void DrawSprite()
		{
			_anchorAnim.Animate();

			DrawSpriteTexture();

			var spriteRect = new RectangleF(_spritePosition, _content.Sprite.ScaledSize);

			_content.Sprite.Position = _spritePosition;

			// Clear the area behind the sprite so we don't get the texture blending with itself.
			_content.Renderer.Drawing.BlendingMode = BlendingMode.None;
			_content.Renderer.Drawing.TextureSampler.HorizontalWrapping = TextureAddressing.Wrap;
			_content.Renderer.Drawing.TextureSampler.VerticalWrapping = TextureAddressing.Wrap;
			_content.Renderer.Drawing.Blit(_content.BackgroundTexture, spriteRect, _content.BackgroundTexture.ToTexel(spriteRect));

			_content.Sprite.Draw();

			_content.Renderer.Drawing.BlendingMode = BlendingMode.Modulate;
			_content.Renderer.Drawing.DrawRectangle(spriteRect, new GorgonColor(0, 0, 1.0f, 0.5f));

			//_content.Sprite.Scale = new Vector2(1);

			float anchorZoom = _zoom.Max(0.75f).Min(1.5f);
			_anchorSprite.Scale = new Vector2(anchorZoom);
			_anchorSprite.Color = _anchorAnim.Color;

			var anchorTrans = new Vector2((_content.Anchor.X * _zoom), _content.Anchor.Y * _zoom);

			Vector2.Add(ref _spritePosition, ref anchorTrans, out anchorTrans);
			
			_anchorSprite.Position = anchorTrans;
			_anchorSprite.Draw();
		}

		/// <summary>
		/// Function called when a property is changed on the related content.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="value">New value assigned to the property.</param>
		protected override void OnContentPropertyChanged(string propertyName, object value)
		{
			base.OnContentPropertyChanged(propertyName, value);

			switch (propertyName.ToLowerInvariant())
			{
				case "texture":
					if (SpriteEditorMode != SpriteEditorMode.Edit)
					{
						break;
					}

					CreateZoomWindow();

					if (_content.Texture != null)
					{
						_clipper.TextureSize = _content.Texture.Settings.Size;
					}
					break;
				case "size":
					CalculateSpritePosition();

					if (SpriteEditorMode == SpriteEditorMode.Edit)
					{
						_clipper.ClipRegion = new RectangleF(_content.TextureRegion.Location, _content.Size);
						_content.TextureRegion = Rectangle.Round(_clipper.ClipRegion);
						_clipper.Scale = new Vector2(1);
						_clipper.TextureSize = _content.Texture.Settings.Size;
					}
					break;
				case "textureregion":
					CalculateSpritePosition();

					if (SpriteEditorMode == SpriteEditorMode.Edit)
					{
						_content.Size = _content.TextureRegion.Size;
						_clipper.ClipRegion = new RectangleF(_content.TextureRegion.Location, _content.TextureRegion.Size);
						_clipper.Scale = new Vector2(1);
						_clipper.TextureSize = _content.Texture.Settings.Size;
					}

					break;
			}
			
			SetUpScrolling();
			ValidateControls();
		}

		/// <summary>
		/// Function to begin editing after content is created.
		/// </summary>
		public void StartEditMode()
		{
			buttonClip.PerformClick();
		}

		/// <summary>
		/// Function to draw the
		/// </summary>
		public void Draw()
		{
			switch (SpriteEditorMode)
			{
				case SpriteEditorMode.AutoClip:
				case SpriteEditorMode.Edit:
					DrawSpriteEdit();
					break;
				default:
					DrawSprite();
					break;
			}
		}

		/// <summary>
		/// Function called when the content has changed.
		/// </summary>
		public override void RefreshContent()
		{
			base.RefreshContent();

			if (_content == null)
			{
				throw new InvalidCastException(string.Format(Resources.GORSPR_ERR_CONTENT_NOT_SPRITE, Content.Name));
			}

			SetUpScrolling();

			if (_anchorImage == null)
			{
				_anchorImage = ContentObject.Graphics.Textures.CreateTexture<GorgonTexture2D>("AnchorTexture", Resources.anchor_24x24);
				_anchorSprite = _content.Renderer.Renderables.CreateSprite("AnchorSprite", _anchorImage.Settings.Size, _anchorImage);
				_anchorSprite.Anchor = new Vector2(_anchorSprite.Size.X / 2.0f, _anchorSprite.Size.Y / 2.0f);
				_anchorSprite.SmoothingMode = SmoothingMode.Smooth;
				_anchorAnim = new AnchorColorAnim();

				scrollHorizontal.Value = 0;
				scrollVertical.Value = 0;

				_clipper = new Clipper(_content.Renderer, panelSprite)
				           {
					           SelectorPattern = _content.BackgroundTexture,
							   DefaultCursor = Cursors.Cross
				           };

				_halfScreen = (Point)(new Vector2(panelSprite.ClientSize.Width / 2.0f, panelSprite.ClientSize.Height / 2.0f));

				CalculateSpritePosition();
			}

			ValidateControls();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="PanelSpriteEditor"/> class.
		/// </summary>
		public PanelSpriteEditor(GorgonSpriteContent spriteContent)
			: base(spriteContent)
		{
			_content = spriteContent;
			_plugIn = (GorgonSpriteEditorPlugIn)_content.PlugIn;

			InitializeComponent();

			panelSprite.MouseWheel += PanelSprite_MouseWheel;
		}

		#endregion
	}
}
