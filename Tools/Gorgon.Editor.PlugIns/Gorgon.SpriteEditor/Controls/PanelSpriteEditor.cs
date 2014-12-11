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
		Edit = 1
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
		/// Handles the Click event of the buttonClip control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonClip_Click(object sender, EventArgs e)
		{
			if (SpriteEditorMode == SpriteEditorMode.Edit)
			{
				SpriteEditorMode = SpriteEditorMode.View;
				ValidateControls();
				return;
			}

			_clipper.ClipRegion = new RectangleF(_content.TextureRegion.Location, _content.Size);
			_clipper.TextureSize = _content.Texture.Settings.Size;

			_zoomWindow = new ZoomWindow(_content.Renderer, _content.Texture)
			              {
				              Clipper = _clipper,
							  BackgroundTexture = _content.BackgroundTexture,
							  Position = _content.Texture.ToPixel(_content.Sprite.TextureOffset),
							  ZoomAmount = 3.0f,
							  ZoomWindowSize = new Vector2(256, 256)
			              };

			SpriteEditorMode = SpriteEditorMode.Edit;
			ValidateControls();
		}

		/// <summary>
		/// Handles the Click event of the buttonCenter control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonCenter_Click(object sender, EventArgs e)
		{
			scrollHorizontal.Value = 0;
			scrollVertical.Value = 0;
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
			_content.Draw();
		}

		/// <summary>
		/// Function to validate the controls on the panel.
		/// </summary>
		private void ValidateControls()
		{
			buttonSave.Enabled = buttonRevert.Enabled = _content.HasChanges;
			buttonClip.Enabled = _content != null && _content.Texture != null;
			dropDownZoom.Enabled = SpriteEditorMode == SpriteEditorMode.View;
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
				foreach (ToolStripMenuItem control in items)
				{
					if (control != sender)
					{
						control.Checked = false;
					}
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

			menuItemToWindow.Text = Resources.GORSPR_TEXT_TO_WINDOW;
			menuItem1600.Text = 16.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem800.Text = 8.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem400.Text = 4.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem200.Text = 2.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem100.Text = 1.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem75.Text = 0.75.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem50.Text = 0.5.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);
			menuItem25.Text = 0.25.ToString("P0", CultureInfo.CurrentUICulture.NumberFormat);

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
		/// Function to calculate the sprite position.
		/// </summary>
		private void CalculateSpritePosition()
		{
			_halfSprite = (Point)(new Vector2(_content.Sprite.ScaledSize.X / 2.0f, _content.Sprite.ScaledSize.Y / 2.0f));
			_halfScreen = (Point)(new Vector2(panelSprite.ClientSize.Width / 2.0f, panelSprite.ClientSize.Height / 2.0f));

			_spritePosition = new Vector2(_halfScreen.X - _halfSprite.X - (scrollHorizontal.Value * _zoom), _halfScreen.Y - _halfSprite.Y - (scrollVertical.Value * _zoom));
		}

		/// <summary>
		/// Function to draw the sprite texture.
		/// </summary>
		private void DrawSpriteTexture()
		{
			Vector2 spriteSize;
			Vector2 imageScale;
			Vector2 imagePosition;

			switch (SpriteEditorMode)
			{
				case SpriteEditorMode.Edit:
					spriteSize = _content.Sprite.Size;
					_content.TextureSprite.Opacity = 1.0f;
					break;
				default:
					spriteSize = new Vector2(_content.Sprite.Size.X * _zoom, _content.Sprite.Size.Y * _zoom);
					_content.TextureSprite.Opacity = 0.35f;
					break;
			}

			imageScale = new Vector2(spriteSize.X / _content.TextureRegion.Width, spriteSize.Y / _content.TextureRegion.Height);
			imagePosition = new Vector2(_spritePosition.X - _content.TextureRegion.X * imageScale.X, _spritePosition.Y - _content.TextureRegion.Y * imageScale.Y);
			
			_content.TextureSprite.ScaledSize = (Point)Vector2.Modulate(imageScale, _content.TextureSprite.Size);
			_content.TextureSprite.Position = (Point)imagePosition;
			_content.TextureSprite.SmoothingMode = SmoothingMode.Smooth;

			_content.TextureSprite.Draw();
		}

		/// <summary>
		/// Function to draw the sprite editor functionality.
		/// </summary>
		private void DrawSpriteEdit()
		{
			_content.Sprite.Scale = new Vector2(1);			

			CalculateSpritePosition();

			DrawSpriteTexture();

			// Offset the clipping rectangle so that it appears in the correct place on the screen.
			_clipper.Offset = _spritePosition - _content.TextureRegion.Location;
			_clipper.Draw();

			_zoomWindow.Draw();
		}

		/// <summary>
		/// Function to draw the current sprite.
		/// </summary>
		private void DrawSprite()
		{
			_anchorAnim.Animate();

			_content.Sprite.Scale = new Vector2(_zoom);

			CalculateSpritePosition();

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

			_content.Sprite.Scale = new Vector2(1);

			float anchorZoom = _zoom.Max(0.75f).Min(1.5f);
			_anchorSprite.Scale = new Vector2(anchorZoom);
			_anchorSprite.Color = _anchorAnim.Color;

			var anchorTrans = new Vector2((_content.Anchor.X * _zoom), _content.Anchor.Y * _zoom);

			Vector2.Add(ref _spritePosition, ref anchorTrans, out anchorTrans);
			
			_anchorSprite.Position = anchorTrans;
			_anchorSprite.Draw();

			_content.Renderer.Drawing.DrawLine(new Vector2(0, _halfScreen.Y), new Vector2(panelSprite.ClientSize.Width, _halfScreen.Y), Color.DeepPink);
			_content.Renderer.Drawing.DrawLine(new Vector2(_halfScreen.X, 0), new Vector2(_halfScreen.X, panelSprite.ClientSize.Height), Color.DeepPink);
		}

		/// <summary>
		/// Function called when a property is changed on the related content.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="value">New value assigned to the property.</param>
		protected override void OnContentPropertyChanged(string propertyName, object value)
		{
			base.OnContentPropertyChanged(propertyName, value);

			SetUpScrolling();
			ValidateControls();
		}

		/// <summary>
		/// Function to draw the
		/// </summary>
		public void Draw()
		{
			switch (SpriteEditorMode)
			{
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
					           SelectorPattern = _content.BackgroundTexture
				           };
			}

			ValidateControls();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="PanelSpriteEditor"/> class.
		/// </summary>
		public PanelSpriteEditor(GorgonSpriteContent spriteContent, GorgonInputFactory input)
			: base(spriteContent, input)
		{
			_content = spriteContent;
			_plugIn = (GorgonSpriteEditorPlugIn)_content.PlugIn;

			InitializeComponent();
		}
		#endregion
	}
}
