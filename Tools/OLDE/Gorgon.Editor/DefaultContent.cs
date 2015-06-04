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

using System.Drawing;
using System.IO;
using Gorgon.Diagnostics;
using Gorgon.Editor.Properties;
using Gorgon.Graphics;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Renderers;
using SlimMath;

namespace Gorgon.Editor
{
	/// <summary>
	/// Default content object.
	/// </summary>
	class DefaultContent
		: ContentObject
	{
		#region Variables.
		private ContentPanel _container;					// Our container control.
		private Gorgon2D _2D;								// 2D renderer for the content.
		private GorgonTexture2D _logo;						// Logo.
		private bool _disposed;								// Flag to indicate that the object was disposed.
		private RectangleF[] _blurStates;					// Images for blur states.
		private RectangleF _sourceState = RectangleF.Empty;	// Source image state.
		private RectangleF _destState = RectangleF.Empty;	// Destination image state.
		private float _alpha;								// Alpha value.
		private float _delta;								// Alpha delta.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the alpha delta time value.
		/// </summary>
		public float AlphaDelta
		{
			get
			{
				return _delta;
			}
			set
			{
				// If the current delta is less than 0, then make the value less than 0.
				if (_delta < 0)
				{
					value = -value;
				}

				_delta = value;
			}
		}

        /// <summary>
        /// Property to return whether this content has properties that can be manipulated in the properties tab.
        /// </summary>
        public override bool HasProperties
        {
            get 
            {
                return false;
            }
        }

		/// <summary>
		/// Property to return the type of content.
		/// </summary>
		public override string ContentType
		{
			get
			{
				return "Default Content";
			}		
		}

		/// <summary>
		/// Property to return whether the content uses a renderer.
		/// </summary>
		public override bool HasRenderer
		{
			get
			{
				return true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function called when editor or plug-in settings are updated.
		/// </summary>
		protected internal override void OnEditorSettingsUpdated()
		{
			if (Program.Settings.AnimateStartPageLogo)
			{
				_delta = _delta < 0 ? -Program.Settings.StartPageAnimationPulseRate : Program.Settings.StartPageAnimationPulseRate;
			}
			else
			{
				_delta = 0.0f;
			}
		}

		/// <summary>
		/// Function to persist the content data into a stream.
		/// </summary>
		/// <param name="stream">Stream that will receive the data for the content.</param>
		protected override void OnPersist(Stream stream)
		{
			// No saving is required for this content.	
		}

		/// <summary>
		/// Function to read the content data from a stream.
		/// </summary>
		/// <param name="stream">Stream containing the content data.</param>
		protected override void OnRead(Stream stream)
		{
			// No need to read this content.
		}

		/// <summary>
		/// Function to draw the default content page.
		/// </summary>
		public override void Draw()
		{
			RectangleF logoBounds = RectangleF.Empty;
			SizeF logoSize = _logo.Settings.Size;
			SizeF screenSize = _container.ClientSize;

			_2D.Clear(_container.BackColor);

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

			_2D.Drawing.SmoothingMode = SmoothingMode.Smooth;
			_2D.Drawing.BlendingMode = BlendingMode.Modulate;

			if (!_delta.EqualsEpsilon(0.0f))
			{
				_2D.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, 1.0f - _alpha), _logo, _sourceState);
				_2D.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, _alpha), _logo, _destState);

				_2D.Drawing.BlendingMode = BlendingMode.Additive;
				_2D.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, 0.25f), _logo, _blurStates[2]);

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
				_2D.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, 1.0f), _logo, _blurStates[0]);
				_2D.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, 0.25f), _logo, _blurStates[2]);
			}

			// Display our logo.
			_2D.Render(2);
		}

		/// <summary>
		/// Function to perform initialization on the content.
		/// </summary>
		/// <returns>A control to embed into the container interface.</returns>
        protected override ContentPanel OnInitialize()
		{
			_delta = Program.Settings.AnimateStartPageLogo ? Program.Settings.StartPageAnimationPulseRate.Abs() : 0;

			_container = new ContentPanel(this)
			             {
				             CaptionVisible = false
			             };

			_2D = Graphics.Output.Create2DRenderer(_container.PanelDisplay);

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
						
			return _container;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultContent"/> class.
		/// </summary>
		public DefaultContent()
			: base(null)
		{
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_logo != null)
					{
						_logo.Dispose();
					}

					if (_2D != null)
					{
						_2D.Dispose();
					}
				}
			}

			_container = null;
			_2D = null;
			_logo = null;
			_disposed = true;

			base.Dispose(disposing);
		}
		#endregion
	}
}
