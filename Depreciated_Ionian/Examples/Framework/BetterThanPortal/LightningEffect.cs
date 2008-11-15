#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: TOBEREPLACED
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using GorgonLibrary;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Event arguments for the lightning flash.
	/// </summary>
	public class LightningFlashEventArgs
	{
		#region Variables.
		private Color _lightColor;			// Lightning color.
		private BlendingModes _mode;		// Blending mode for the lightning.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the color of the lightning flash state.
		/// </summary>
		public Color Color
		{
			get
			{
				return _lightColor;
			}
		}

		/// <summary>
		/// Property to return the blending mode for the flash state.
		/// </summary>
		public BlendingModes BlendMode
		{
			get
			{
				return _mode;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="LightningFlashEventArgs"/> class.
		/// </summary>
		/// <param name="flashColor">Color of the flash.</param>
		/// <param name="flashBlend">The flash blending mode.</param>
		public LightningFlashEventArgs(Color flashColor, BlendingModes flashBlend)
		{
			_lightColor = flashColor;
			_mode = flashBlend;
		}
		#endregion
	}

	/// <summary>
	/// Event handler for lightning flashes.
	/// </summary>
	/// <param name="sender">Object that sent the event.</param>
	/// <param name="e">Event parameters.</param>
	public delegate void LightningFlashEventHandler(object sender, LightningFlashEventArgs e);
	
	/// <summary>
	/// Object to perform a lightning flash.
	/// </summary>
	public class LightningEffect
	{
		#region Variables.
		private PreciseTimer _lightningTimer;					// Timer used to handle the lightning flash.
		private PreciseTimer _flashTimer;						// Flashing timer.
		private bool _flashActive = false;						// Flag to indicate whether the flash is active.
		private Random _rnd;									// Random number generator.
		private Color _baseColor;								// Base color when flash is gone.
		private Color _tintColor;								// Color that will tint the objects when the flash is active.
		private BlendingModes _flashMode;						// Mode used when the flash is active.
		private LightningFlashEventArgs _eventArgs;				// Event parameters.
		#endregion

		#region Events.
		/// <summary>
		/// Event fired when lightning is flashing.
		/// </summary>
		public event LightningFlashEventHandler LightningEvent;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the timing for the lightning animation.
		/// </summary>
		public void Update()
		{
			if (_lightningTimer.Seconds > 15)
			{
				if (_flashTimer.Milliseconds > ((float)_rnd.NextDouble() + 1.0f) * 35.0f)
				{
					_flashActive = !_flashActive;
					_flashTimer.Reset();
				}

				if (_flashActive)
					_eventArgs = new LightningFlashEventArgs(_tintColor, _flashMode);
				else
					_eventArgs = new LightningFlashEventArgs(_baseColor, BlendingModes.None);

				if (LightningEvent != null)
					LightningEvent(this, _eventArgs);
			}

			// Turn off the flash.
			if (_lightningTimer.Seconds > 15 + _rnd.Next(2, 15))
			{
				_flashActive = false;
				_lightningTimer.Reset();
				_eventArgs = new LightningFlashEventArgs(_baseColor, BlendingModes.None);

				if (LightningEvent != null)
					LightningEvent(this, _eventArgs);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="LightningEffect"/> class.
		/// </summary>
		/// <param name="baseColor">Original color.</param>
		/// <param name="lightningColor">Color of the lightning.</param>
		/// <param name="lightningMode">Blending mode used when lightning is active.</param>
		public LightningEffect(Color baseColor, Color lightningColor, BlendingModes lightningMode)
		{
			_rnd = new Random();
			_lightningTimer = new PreciseTimer();
			_flashTimer = new PreciseTimer();
			_baseColor = baseColor;
			_flashMode = lightningMode;
			_tintColor = lightningColor;
			_eventArgs = new LightningFlashEventArgs(baseColor, BlendingModes.None);
		}
		#endregion
	}
}
