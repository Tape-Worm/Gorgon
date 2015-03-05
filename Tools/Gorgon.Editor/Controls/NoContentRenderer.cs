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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using GorgonLibrary.Renderers;
using SlimMath;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Renderer used to display the no content default panel.
	/// </summary>
	class NoContentRenderer
		: EditorContentRenderer2D
	{
		#region Variables.
		// The logo texture.
		private GorgonTexture2D _logo;
		// Images for blur states.
		private RectangleF[] _blurStates;
		// Source image state.
		private RectangleF _sourceState = RectangleF.Empty;
		// Destination image state.
		private RectangleF _destState = RectangleF.Empty;
		// Alpha value.
		private float _alpha;
		// Alpha delta.
		private float _delta;								
		#endregion

		#region Methods.
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
		}

		/// <summary>
		/// Function to perform the actual rendering of graphics to the control surface.
		/// </summary>
		public override void Draw()
		{
			RectangleF logoBounds = RectangleF.Empty;
			SizeF logoSize = _logo.Settings.Size;
			SizeF screenSize = SwapChain.Settings.Size;

			logoSize.Height = 256;

			if (screenSize.Width < logoSize.Width)
			{
				logoBounds.Width = logoSize.Width * screenSize.Width / logoSize.Width;
			}
			else
			{
				logoBounds.Width = logoSize.Width;
			}

			float aspect = logoSize.Height / logoSize.Width;
			logoBounds.Height = logoBounds.Width * aspect;
			logoBounds.X = screenSize.Width / 2.0f - logoBounds.Width / 2.0f;
			logoBounds.Y = screenSize.Height / 2.0f - logoBounds.Height / 2.0f;

			Renderer.Drawing.SmoothingMode = SmoothingMode.Smooth;
			Renderer.Drawing.BlendingMode = BlendingMode.Modulate;

			if (!_delta.EqualsEpsilon(0.0f))
			{
				Renderer.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, 1.0f - _alpha), _logo, _sourceState);
				Renderer.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, _alpha), _logo, _destState);

				Renderer.Drawing.BlendingMode = BlendingMode.Additive;
				Renderer.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, 0.25f), _logo, _blurStates[2]);

				_alpha += _delta * GorgonTiming.ScaledDelta;

				if (_alpha > 1.0f)
				{
					if ((_destState == _blurStates[1]) && (_sourceState == _blurStates[2]))
					{
						_alpha = 0.0f;
						_destState = _blurStates[0];
						_sourceState = _blurStates[1];
					}
					else
					{
						_alpha = 1.0f;
						_delta = -_delta;
					}
				}
				else if (_alpha < 0.0f)
				{
					if ((_destState == _blurStates[0]) && (_sourceState == _blurStates[1]))
					{
						_alpha = 1.0f;
						_destState = _blurStates[1];
						_sourceState = _blurStates[2];
					}
					else
					{
						_alpha = 0.0f;
						_delta = -_delta;
					}
				}
			}
			else
			{
				Renderer.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, 1.0f), _logo, _blurStates[0]);
				Renderer.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, 0.25f), _logo, _blurStates[2]);
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="NoContentRenderer"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface.</param>
		/// <param name="settings">The settings interface for the application.</param>
		/// <param name="defaultContent">The default content.</param>
		public NoContentRenderer(GorgonGraphics graphics, IEditorSettings settings, IContent defaultContent)
			: base(graphics, defaultContent)
		{
			ClearColor = null;

			// Set for 30 FPS vsync.
			RenderInterval = 2;
			_delta = (settings.AnimateStartPageLogo && settings.StartPageAnimationPulseRate > 0) ? settings.StartPageAnimationPulseRate : 0;
		}
		#endregion
	}
}
