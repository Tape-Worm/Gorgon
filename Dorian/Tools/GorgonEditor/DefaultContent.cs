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
// Created: Saturday, March 2, 2013 2:42:57 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using SlimMath;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Math;
using GorgonLibrary.IO;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Default content object.
	/// </summary>
	class DefaultContent
		: IDisposable
	{
		#region Variables.
		private GorgonTexture2D _logo = null;					// Logo.
		private bool _disposed = false;							// Flag to indicate that the object was disposed.
		private RectangleF[] _blurStates = null;				// Images for blur states.
		private RectangleF _sourceState = RectangleF.Empty;		// Source image state.
		private RectangleF _destState = RectangleF.Empty;		// Destination image state.
		private float _alphaDelta = 0.075f;						// Alpha delta value.
		private float _alpha = 0.0f;							// Alpha value.
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to draw the default content page.
		/// </summary>
		/// <returns>The number of vertical retraces to wait.</returns>
		public int Draw()
		{
			var defaultTarget = Program.Renderer.DefaultTarget;
			RectangleF logoBounds = RectangleF.Empty;
			SizeF logoSize = _logo.Settings.Size;
			float aspect = 0.0f;

			logoSize.Height = 256;

			if (defaultTarget.Settings.Width < logoSize.Width)
				logoBounds.Width = logoSize.Width * defaultTarget.Settings.Width / logoSize.Width;
			else
				logoBounds.Width = logoSize.Width;

			aspect = logoSize.Height / logoSize.Width;
			logoBounds.Height = logoBounds.Width * aspect;						
			logoBounds.X = defaultTarget.Settings.Width / 2.0f - logoBounds.Width / 2.0f;
			logoBounds.Y = defaultTarget.Settings.Height / 2.0f - logoBounds.Height / 2.0f;

			Program.Renderer.Drawing.SmoothingMode = SmoothingMode.Smooth;
			Program.Renderer.Drawing.BlendingMode = BlendingMode.Additive;
			Program.Renderer.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, 1.0f - _alpha), _logo, _sourceState);
			Program.Renderer.Drawing.BlendingMode = BlendingMode.Modulate;
			Program.Renderer.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, _alpha), _logo, _destState);

			_alpha += _alphaDelta * GorgonTiming.ScaledDelta;

			if ((_alpha > 1.0f) || (_alpha < 0.0f))
			{
				_alphaDelta = -_alphaDelta;

				if (_alphaDelta < 0)
				{
					_sourceState = _blurStates[GorgonRandom.RandomInt32(3)];
				}
				if (_alphaDelta > 0)
				{
					_sourceState = _blurStates[GorgonRandom.RandomInt32(3)];
				}

				_destState = _blurStates[1];

				if (_alpha > 1.0f)
				{
					_alpha = 1.0f;
				}

				if (_alpha < 0.0f)
				{
					_alpha = 0.0f;
				}
			}

			return 2;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultContent"/> class.
		/// </summary>
		public DefaultContent()
		{
/*			RectangleF textureCoordinates = RectangleF.Empty;
			int actualHeight = Properties.Resources.Gorgon_2_Logo_Full.Height;
			
			_logo = Program.Graphics.Textures.Create2DTextureFromGDIImage("Logo", Properties.Resources.Gorgon_2_Logo_Full, new IO.GorgonGDIOptions()
				{
					UseClipping = true,
					Height = 372
				});

			textureCoordinates = new RectangleF(Vector2.Zero, _logo.ToTexel(new Vector2(_logo.Settings.Width, _logo.Settings.Height / 3)));

			using (var blurTarget = Program.Graphics.Output.CreateRenderTarget("Blur.Medium", new GorgonRenderTargetSettings()
			{
				Width = _logo.Settings.Width,
				Height = actualHeight,
				Format = _logo.Settings.Format
			}))
			{
				blurTarget.Clear(DarkFormsRenderer.MenuHilightForeground);

				var blurEffect = Program.Renderer.Effects.GaussianBlur;

				blurEffect.BlurRenderTargetsSize = new Size(64, 64);
				blurEffect.BlurAmount = 3.2f;
				blurEffect.Render((int pass) =>
					{
						if (pass == 0)
						{
							Program.Renderer.Drawing.Blit(_logo, new RectangleF(0, 0, blurEffect.BlurRenderTargetsSize.Width, blurEffect.BlurRenderTargetsSize.Height), new RectangleF(0, 0, 1, 1));
						}
						else
						{
							Program.Renderer.Target = blurTarget;
							Program.Renderer.Drawing.Blit(blurEffect.BlurredTexture, new RectangleF(0, 0, _logo.Settings.Width, actualHeight), textureCoordinates);
						}
					});

				_logo.CopySubResource(blurTarget.Texture, new Rectangle(0, 0, _logo.Settings.Width, actualHeight), new Vector2(0, actualHeight));

				// Medium blur.
				blurEffect.BlurRenderTargetsSize = new Size(128, 128);
				blurEffect.BlurAmount = 2.7f;
				blurEffect.Render((int pass) =>
				{
					if (pass == 0)
					{
						Program.Renderer.Drawing.Blit(_logo, new RectangleF(0, 0, blurEffect.BlurRenderTargetsSize.Width, blurEffect.BlurRenderTargetsSize.Height), new RectangleF(0, 0, 1, 1));
					}
					else
					{
						Program.Renderer.Target = blurTarget;
						Program.Renderer.Drawing.Blit(blurEffect.BlurredTexture, new RectangleF(0, 0, _logo.Settings.Width, 124), textureCoordinates);
					}
				});

				_logo.CopySubResource(blurTarget.Texture, new Rectangle(0, 0, _logo.Settings.Width, actualHeight), new Vector2(0, actualHeight * 2));
			}*/

			//_logo = Program.Graphics.Textures.Create2DTextureFromGDIImage("Logo", Properties.Resources.Gorgon_2_x_Logo_Blurry);
			_logo = Program.Graphics.Textures.FromMemory<GorgonTexture2D>("Logo", Properties.Resources.Gorgon_2_x_Logo_Blurry, new GorgonCodecDDS());

			var textureCoordinates = new RectangleF(Vector2.Zero, _logo.ToTexel(new Vector2(_logo.Settings.Width, _logo.Settings.Height / 3)));

			_blurStates = new[] {
					textureCoordinates,																			// No blur.
					new RectangleF(new Vector2(0, textureCoordinates.Height), textureCoordinates.Size),			// Medium blur.
					new RectangleF(new Vector2(0, textureCoordinates.Height * 2), textureCoordinates.Size),		// Max blur.
					};

			_sourceState = _blurStates[2];
			_destState = _blurStates[1];
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_logo != null)
					{
						_logo.Dispose();
					}
				}

				_logo = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
