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
using System.Windows.Forms;
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
		: ContentObject
	{
		#region Variables.
		private DefaultContentPanel _container = null;			// Our container control.
		private Gorgon2D _2D = null;							// 2D renderer for the content.
		private GorgonTexture2D _logo = null;					// Logo.
		private bool _disposed = false;							// Flag to indicate that the object was disposed.
		private RectangleF[] _blurStates = null;				// Images for blur states.
		private RectangleF _sourceState = RectangleF.Empty;		// Source image state.
		private RectangleF _destState = RectangleF.Empty;		// Destination image state.
		private float _alphaDelta = 0.0f;						// Alpha delta value.
		private float _alpha = 0.0f;							// Alpha value.
		private string _name = string.Empty;					// Name of the document.
		#endregion

		#region Properties.
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
		/// Handles the Click event of the checkPulse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void checkPulse_Click(object sender, EventArgs e)
		{
			Program.Settings.AnimateStartPage = _container.checkPulse.Checked;
			_container.numericPulseRate.Enabled = _container.checkPulse.Checked;
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericPulseRate control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericPulseRate_ValueChanged(object sender, EventArgs e)
		{
			Program.Settings.StartPageAnimationPulseRate = (float)_container.numericPulseRate.Value;
			if (_alphaDelta < 0)
			{
				_alphaDelta = -Program.Settings.StartPageAnimationPulseRate;
			}
			else
			{
				_alphaDelta = Program.Settings.StartPageAnimationPulseRate;
			}			
		}

        /// <summary>
        /// Function to persist the content to the file system.
        /// </summary>
        protected override void OnPersist()
        {            
        }

		/// <summary>
		/// Function called when the content window is closed.
		/// </summary>
		/// <returns>
		/// TRUE to continue closing the window, FALSE to cancel the close.
		/// </returns>
		protected override bool OnClose()
		{
			return true;
		}

		/// <summary>
		/// Function to draw the default content page.
		/// </summary>
		public override void Draw()
		{
			var defaultTarget = _2D.DefaultTarget;
			RectangleF logoBounds = RectangleF.Empty;
			SizeF logoSize = _logo.Settings.Size;
			float aspect = 0.0f;
						
			_2D.Clear(_container.BackColor);

			logoSize.Height = 256;

			if (defaultTarget.Settings.Width < logoSize.Width)
				logoBounds.Width = logoSize.Width * defaultTarget.Settings.Width / logoSize.Width;
			else
				logoBounds.Width = logoSize.Width;

			aspect = logoSize.Height / logoSize.Width;
			logoBounds.Height = logoBounds.Width * aspect;						
			logoBounds.X = defaultTarget.Settings.Width / 2.0f - logoBounds.Width / 2.0f;
			logoBounds.Y = defaultTarget.Settings.Height / 2.0f - logoBounds.Height / 2.0f;

			_2D.Drawing.SmoothingMode = SmoothingMode.Smooth;
			_2D.Drawing.BlendingMode = BlendingMode.Modulate;

			if ((_container.checkPulse.Checked) && (_alphaDelta != 0))
			{
				_2D.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, 1.0f - _alpha), _logo, _sourceState);
				_2D.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, _alpha), _logo, _destState);

				_2D.Drawing.BlendingMode = BlendingMode.Additive;
				_2D.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, 0.25f), _logo, _blurStates[2]);

				_alpha += _alphaDelta * GorgonTiming.ScaledDelta;

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
						_alphaDelta = -_alphaDelta;
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
						_alphaDelta = -_alphaDelta;
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
		public override Control InitializeContent()
		{		
			_container = new DefaultContentPanel();
			_container.BackColor = DarkFormsRenderer.DarkBackground;
			_container.checkPulse.Checked = Program.Settings.AnimateStartPage;
			_alphaDelta = Program.Settings.StartPageAnimationPulseRate;
			_container.numericPulseRate.Value = (decimal)_alphaDelta.Abs();
			_container.checkPulse.Click += checkPulse_Click;
			_container.numericPulseRate.ValueChanged += numericPulseRate_ValueChanged;

			_2D = Program.Graphics.Output.Create2DRenderer(_container.panelDisplay);

			// Create the logo for display.
			_logo = Program.Graphics.Textures.FromMemory<GorgonTexture2D>("Logo", Properties.Resources.Gorgon_2_x_Logo_Blurry, new GorgonCodecDDS());

			var textureCoordinates = new RectangleF(Vector2.Zero, _logo.ToTexel(new Vector2(_logo.Settings.Width, _logo.Settings.Height / 3)));

			// Set up coordinates for our blurred images.
			_blurStates = new[] {
					textureCoordinates,																			// No blur.
					new RectangleF(new Vector2(0, textureCoordinates.Height), textureCoordinates.Size),			// Medium blur.
					new RectangleF(new Vector2(0, textureCoordinates.Height * 2), textureCoordinates.Size),		// Max blur.
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
		{
			HasChanges = false;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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

					if (_container != null)
					{
						_container.Dispose();						
					}
				}

				_container = null;
				_2D = null;
				_logo = null;
				_disposed = true;
			}
		}
		#endregion
	}
}
