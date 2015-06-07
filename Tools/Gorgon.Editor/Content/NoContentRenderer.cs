#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Tuesday, March 3, 2015 1:09:52 AM
// 
#endregion

using System.Drawing;
using Gorgon.Editor.Properties;
using Gorgon.Graphics;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Timing;
using SlimMath;

namespace Gorgon.Editor
{
	/// <summary>
	/// Renderer used to display the no content default panel.
	/// </summary>
	class NoContentRenderer
		: EditorContentRenderer2D
	{
		#region Variables.
		// Flag to indicate that we should drop the logo down from the top.
		private static bool _dropDown = true;
		// The logo texture.
		private GorgonTexture2D _logo;
		// Images for blur states.
		private RectangleF[] _blurStates;
		// Source image state.
		private RectangleF _sourceState = RectangleF.Empty;
		// Destination image state.
		private RectangleF _destState = RectangleF.Empty;
		// Sprite used to dislpay the logo.
		private GorgonSprite _logoSprite;
		// Alpha value.
		private float _alpha;
		// Alpha delta.
		private float _delta;
		// Vertical position of the sprite.
		private float _yPos;
		// Time to start moving at.
		private float _startMoveTime;
		// Half of the total height for the swap chain.
		private Size _halfScreen;
		// Starting speed for drop animation.
		private float _startSpeed;
		// Drop speed.
		private float _dropSpeed;
		// Percentage of logo distance on the screen when the screen is resized.
		private float _yPosPercent;
		// Starting position of the logo.
		private float _startPosition;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to switch the states for the logo animation blurring/fading.
		/// </summary>
		/// <param name="reverse"><b>true</b> to set the state to reverse animation, <b>false</b> to set to forward animation.</param>
		private void SwitchState(bool reverse)
		{
			RectangleF state1 = reverse ? _blurStates[1] : _blurStates[0];
			RectangleF state2 = reverse ? _blurStates[2] : _blurStates[1];

			if ((_destState == state1) && (_sourceState == state2))
			{
				_alpha = reverse ? 0.0f : 1.0f;
				_destState = reverse ? _blurStates[0] : _blurStates[1];
				_sourceState = reverse ? _blurStates[1] : _blurStates[2];
			}
			else
			{
				_alpha = reverse ? 1.0f : 0.0f;

				// Switch direction.
				_delta = -_delta;
			}
		}

		/// <summary>
		/// Function to draw the logo with animation.
		/// </summary>
		private void DrawAnimated()
		{
			_logoSprite.Position = new Vector2(_halfScreen.Width, _yPos);
			_logoSprite.SmoothingMode = SmoothingMode.Smooth;
			_logoSprite.BlendingMode = BlendingMode.Modulate;

			MoveLogo();

			_logoSprite.Opacity = 1.0f - _alpha;
			_logoSprite.TextureRegion = _sourceState;
			_logoSprite.Draw();

			_logoSprite.Opacity = _alpha;
			_logoSprite.Position = new Vector2(_halfScreen.Width, _halfScreen.Height + (_halfScreen.Height - _yPos));
			_logoSprite.TextureRegion = _destState;
			_logoSprite.Draw();

			_logoSprite.Opacity = 0.25f;
			_logoSprite.Position = new Vector2(_halfScreen.Width, _halfScreen.Height);
			_logoSprite.TextureRegion = _blurStates[2];
			_logoSprite.BlendingMode = BlendingMode.Additive;
			_logoSprite.Draw();

			_alpha += _delta * GorgonTiming.Delta;
			
			if (_alpha > 1.0f)
			{
				SwitchState(true);
			}
			
			if (_alpha < 0.0f)
			{
				SwitchState(false);
			}
		}

		/// <summary>
		/// Function to draw the logo with no animation.
		/// </summary>
		private void DrawNoAnimation()
		{
			_logoSprite.Opacity = 1.0f;
			_logoSprite.TextureRegion = _blurStates[0];
			_logoSprite.Draw();

			_logoSprite.Opacity = 0.25f;
			_logoSprite.TextureRegion = _blurStates[2];
			_logoSprite.Draw();
		}

		/// <summary>
		/// Function to clean up any resources provided to the renderer.
		/// </summary>
		protected override void OnCleanUpResources()
		{
			if (_logo != null)
			{
				_logo.Dispose();
			}
			_logo = null;

			// Turn off the drop down animation if we reload this renderer.
			_dropDown = false;
		}

		/// <summary>
		/// Function to create any resources that may be required by the renderer.
		/// </summary>
		protected override void OnCreateResources()
		{
			// Create the logo for display.
			_logo = Graphics.Textures.FromMemory<GorgonTexture2D>("Logo", Resources.Gorgon_2_x_Logo_Blurry, new GorgonCodecDDS());

			var textureCoordinates = new RectangleF(Vector2.Zero, _logo.ToTexel(new Vector2(_logo.Settings.Width, _logo.Settings.Height / 3)));

			// Set up coordinates for our blurred images.
			_blurStates = new[]
			              {
				              textureCoordinates, // No blur.
				              new RectangleF(new Vector2(0, textureCoordinates.Height), textureCoordinates.Size), // Medium blur.
				              new RectangleF(new Vector2(0, textureCoordinates.Height * 2), textureCoordinates.Size) // Max blur.
			              };

			_sourceState = _blurStates[2];
			_destState = _blurStates[1];

			_logoSprite = Renderer.Renderables.CreateSprite("LogoSprite",
			                                                new GorgonSpriteSettings
			                                                {
																Size = new Vector2(_logo.Settings.Width, _logo.Settings.Height / 3),
																Texture = _logo,
																TextureRegion = _blurStates[0]
			                                                });

			CalculateLogoSize();
		}

		/// <summary>
		/// Function called before the swap chain is resized.
		/// </summary>
		/// <remarks>
		/// Use this method to unbind resources before the swap chain is resized.
		/// </remarks>
		protected override void OnBeforeSwapChainResize()
		{
			base.OnBeforeSwapChainResize();

			if (!_dropDown)
			{
				return;
			}

			_yPosPercent = _yPos / (_halfScreen.Height - _startPosition);
		}

		/// <summary>
		/// Function called after the swap chain is resized.
		/// </summary>
		/// <param name="args">Arguments for the resize event.</param>
		/// <remarks>
		/// Use this method to rebind and reinitialize (if necessary) items when the swap chain is finished resizing.
		/// </remarks>
		protected override void OnAfterSwapChainResize(GorgonAfterSwapChainResizedEventArgs args)
		{
			base.OnAfterSwapChainResize(args);
			
			CalculateLogoSize();

			if (!_dropDown)
			{
				return;
			}

			// Place at the approximate relative location that we were at prior to the resizing.
			_yPos = (_halfScreen.Height - _startPosition) * _yPosPercent;
		}

		/// <summary>
		/// Function to calculate the size of the logo based on the current swap chain width and height.
		/// </summary>
		private void CalculateLogoSize()
		{
			if (_logoSprite == null)
			{
				return;
			}

			_halfScreen = new Size(SwapChain.Settings.Width / 2, SwapChain.Settings.Height / 2);

			var scale = new Vector2(1);

			if (SwapChain.Settings.Size.Width < _logo.Settings.Size.Width)
			{ 
				scale.X = (float)SwapChain.Settings.Size.Width / _logo.Settings.Size.Width;
				scale.Y = scale.X;
			}

			_logoSprite.Scale = scale;
			_logoSprite.Anchor = new Vector2(_logoSprite.Size.X / 2.0f, _logoSprite.Size.Y / 2.0f);

			// Place out of sight.
			if ((_delta.EqualsEpsilon(0.0f))
			    || (!_dropDown))
			{
				// Force to the center of the screen.
				_yPos = _halfScreen.Height;
			}
			else if (_startMoveTime.EqualsEpsilon(0.0f))
			{
				_startSpeed = _dropSpeed = (_logo.Settings.Height / 3) * (_delta / 0.125f).Max(0.5f);
				_startPosition = _yPos = -_logoSprite.Anchor.Y * 1.25f;
			}
		}

		/// <summary>
		/// Function to move the logo down.
		/// </summary>
		private void MoveLogo()
		{
			if (!_dropDown)
			{
				return;
			}
			
			if (_startMoveTime.EqualsEpsilon(0))
			{
				_startMoveTime = GorgonTiming.SecondsSinceStart;
			}

			float slowDistance = _halfScreen.Height / 3;

			float posDelta = (_halfScreen.Height - _yPos);
			_dropSpeed = _startSpeed * GorgonTiming.Delta;

			if (posDelta < slowDistance)
			{
				_dropSpeed = _startSpeed * GorgonTiming.Delta * (posDelta / slowDistance);
			}

			_yPos += _dropSpeed;

			_dropDown = _yPos.FastCeiling() < _halfScreen.Height;

			if (_dropDown)
			{
				return;
			}

			_yPos = _halfScreen.Height;
			RenderInterval = 2;
		}

		/// <summary>
		/// Function to perform the actual rendering of graphics to the control surface.
		/// </summary>
		public override void Draw()
		{
			if (_delta.EqualsEpsilon(0.0f))
			{
				DrawNoAnimation();
				return;
			}

			DrawAnimated();
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="NoContentRenderer"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface.</param>
		/// <param name="settings">The settings interface for the application.</param>
		/// <param name="defaultContent">The default content.</param>
		public NoContentRenderer(GorgonGraphics graphics, IEditorSettings settings, IContentData defaultContent)
			: base(graphics, defaultContent)
		{
			ClearColor = null;

			// Set for 30 FPS vsync.
			RenderInterval = 1;
			_delta = settings.StartPageAnimationPulseRate > 0 ? settings.StartPageAnimationPulseRate.Min(1) : 0;

			if (_delta > 0)
			{
				// Scale the delta so that our range is 0..1 in the editor settings, but more reasonable values here.
				_delta = _delta * 0.5f;
			}
		}
		#endregion
	}
}
