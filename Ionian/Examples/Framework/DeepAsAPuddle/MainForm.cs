#region MIT.
// 
// Examples.
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
// Created: Thursday, October 02, 2008 10:46:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.Framework;
using GorgonLibrary.Graphics;
using GorgonLibrary.InputDevices;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Main application form.
	/// </summary>
	public partial class MainForm 
		: GorgonApplicationWindow
	{
		#region Enum
		/// <summary>
		/// Enumeration containing animation sets.
		/// </summary>
		public enum AnimationSet
		{
			/// <summary>
			/// Walk up animation.
			/// </summary>
			WalkUp = 0,
			/// <summary>
			/// Walk left animation.
			/// </summary>
			WalkLeft = 1,
			/// <summary>
			/// Turn left animation.
			/// </summary>
			TurnLeft = 2,
			/// <summary>
			/// Walk left animation 2.
			/// </summary>
			WalkLeftAgain = 3
		}
		#endregion

		#region Variables.
		private Random _rnd = new Random();					// Random number generator.
		private TextSprite _text = null;					// Text sprite.
		private Sprite _iceTiles = null;					// Ice tiles.
		private Sprite _icicle = null;						// Icicle.
		private Sprite _guy = null;							// Guy.
		private Image _spriteImage = null;					// Image used for the sprites.
		private int _tileCountX = 0;						// Horizontal tile count.
		private int _tileCountY = 0;						// Vertical tile count.
		private Vector2D _position = Vector2D.Zero;			// Position for guy.
		private AnimationSet _animSet;						// Animation set.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to draw the background.
		/// </summary>
		private void DrawBackground()
		{
			for (int y = 0; y < _tileCountY + 1; y++)
			{
				for (int x = 0; x < _tileCountX + 1; x++)
				{
					_iceTiles.SetPosition(x * _iceTiles.ScaledWidth, y * _iceTiles.ScaledHeight);
					_iceTiles.Draw();
				}
			}
		}

		/// <summary>
		/// Function to perform logic updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnLogicUpdate(FrameEventArgs e)
		{
			base.OnLogicUpdate(e);

			switch (_animSet)
			{
				case AnimationSet.WalkUp:
					_position.Y -= (_guy.ScaledHeight / 4.0f) * e.FrameDeltaTime;

					if ((_position.Y < _icicle.Position.Y + 16) && (_guy.Depth < _icicle.Depth))
					{
						_animSet = AnimationSet.TurnLeft;
						_guy.Animations["Guy_Walk_Up"].AnimationState = AnimationState.Stopped;
						_guy.Animations["Guy_Walk_Up"].Reset();
						_guy.Animations["Guy_Turn_Left"].AnimationState = AnimationState.Playing;
						if (CursorSprite.Depth > 0.0f)
						{
							if (CursorSprite.Depth > _guy.Depth)
								CursorSprite.Depth = _icicle.Depth + 0.11f;
							if (CursorSprite.Depth < _guy.Depth)
								CursorSprite.Depth = _icicle.Depth + 0.09f;
						}
						_guy.Depth = _icicle.Depth + 0.1f;
					}
					else
						_guy.Animations["Guy_Walk_Up"].Advance(e.FrameDeltaTime * 1000.0f);
					_text.Text = string.Empty;
					break;
				case AnimationSet.TurnLeft:
					if (_guy.Animations["Guy_Turn_Left"].CurrentTime >= _guy.Animations["Guy_Turn_Left"].Length)
					{
						_guy.Animations["Guy_Turn_Left"].Reset();
						_guy.Animations["Guy_Turn_Left"].AnimationState = AnimationState.Stopped;
						_guy.Animations["Guy_Walk_Left"].AnimationState = AnimationState.Playing;
						_animSet = AnimationSet.WalkLeft;
					}
					else
						_guy.Animations["Guy_Turn_Left"].Advance(e.FrameDeltaTime * 1000.0f);
					_text.Text = string.Empty;
					break;
				case AnimationSet.WalkLeft:
					_position.X -= (_guy.ScaledWidth / 2.5f) * e.FrameDeltaTime;
					_guy.Animations["Guy_Walk_Left"].Advance(e.FrameDeltaTime * 1000.0f);

					if (_position.X < (_guy.ScaledWidth * -2.0f))
					{
						_animSet = AnimationSet.WalkLeftAgain;
						if (CursorSprite.Depth > 0.0f)
						{
							if (CursorSprite.Depth > _guy.Depth)
								CursorSprite.Depth = _icicle.Depth - 0.11f;
							if (CursorSprite.Depth < _guy.Depth)
								CursorSprite.Depth = _icicle.Depth - 0.09f;
						}
						_guy.Depth = _icicle.Depth - 0.1f;
						_position.Y = _icicle.Position.Y + (_icicle.ScaledHeight / 2.0f);
						_position.X = Gorgon.Screen.Width + _guy.ScaledWidth;
						_guy.Animations["Guy_Walk_Left"].Reset();
					}
					_text.Text = "We're above the icicle base, so the character will walk behind the icicle by adjusting its depth.\n" +
						"Icicle depth:" + _icicle.Depth.ToString() + "\nCharacter depth:" + _guy.Depth.ToString() + "\nCursor depth:" + CursorSprite.Depth.ToString() + "\n\n";
					break;
				case AnimationSet.WalkLeftAgain:
					_position.X -= (_guy.ScaledWidth / 2.5f) * e.FrameDeltaTime;
					_guy.Animations["Guy_Walk_Left"].Advance(e.FrameDeltaTime * 1000.0f);

					if (_position.X < (_guy.ScaledWidth * -2.0f))
					{
						_guy.Animations["Guy_Walk_Left"].AnimationState = AnimationState.Stopped;
						_guy.Animations["Guy_Walk_Left"].Reset();
						_guy.Animations["Guy_Walk_Up"].AnimationState = AnimationState.Playing;
						_position = new Vector2D((Gorgon.Screen.Width / 2) + _guy.ScaledWidth, Gorgon.Screen.Height + _guy.ScaledHeight);
						_animSet = AnimationSet.WalkUp;
					}
					_text.Text = "We're below the icicle base, so the character will walk in front the icicle by adjusting its depth.\nIcicle depth:"
							+ _icicle.Depth.ToString() + "\nCharacter depth:" + _guy.Depth.ToString() + "\nCursor depth:" + CursorSprite.Depth.ToString() + "\n\n";
					break;
			}

		}

		/// <summary>
		/// Function to perform rendering updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnFrameUpdate(FrameEventArgs e)
		{
			base.OnFrameUpdate(e);

			DrawBackground();

			_guy.Position = MathUtility.Round(_position);
			_guy.Draw();

			// Note how we're drawing the icicle AFTER the character, but when the character is below the icicle, it won't overwrite it.
			_icicle.SetPosition(((_tileCountX - 2) / 2) * _iceTiles.ScaledWidth, ((_tileCountY - 2) / 2) * _iceTiles.ScaledHeight);
			_icicle.Draw();
						
			_text.Text += "Left mouse button - Put the cursor behind the character.\nRight mouse button - Put the cursor in front of the character.\nMiddle mouse button - Reset the cursor depth.\nESC - Close the application.\nCTRL + F - Show frame statistics.";
			_text.Draw();
		}

		/// <summary>
		/// Function called when a mouse button is pushed down.
		/// </summary>
		/// <param name="e">Mouse event parameters.</param>
		protected override void OnMouseButtonDown(MouseInputEventArgs e)
		{
			base.OnMouseButtonDown(e);

			if (e.Buttons == GorgonLibrary.InputDevices.MouseButtons.Button1)
				CursorSprite.Depth = _guy.Depth + 0.01f;
			
			if (e.Buttons == GorgonLibrary.InputDevices.MouseButtons.Button2)
				CursorSprite.Depth = _guy.Depth - 0.01f;

			if (e.Buttons == GorgonLibrary.InputDevices.MouseButtons.Button3)
				CursorSprite.Depth = 0;
		}

		/// <summary>
		/// Function called when the video device is set to a lost state.
		/// </summary>
		protected override void OnDeviceLost()
		{
			base.OnDeviceLost();
		}

		/// <summary>
		/// Function called when the video device has recovered from a lost state.
		/// </summary>
		protected override void OnDeviceReset()
		{
			base.OnDeviceReset();

			_tileCountX = (int)(Gorgon.Screen.Width / _iceTiles.ScaledWidth);
			_tileCountY = (int)(Gorgon.Screen.Height / _iceTiles.ScaledHeight);
		}

		/// <summary>
		/// Function to provide initialization for our example.
		/// </summary>
		protected override void Initialize()
		{			
			_spriteImage = Image.FromFileSystem(FileSystems[ApplicationName], "/Images/0_GuyAnimation.png");
			_guy = Sprite.FromFileSystem(FileSystems[ApplicationName], "/Sprites/Guy.gorSprite");
			_iceTiles = Sprite.FromFileSystem(FileSystems[ApplicationName], "/Sprites/Snow.gorSprite");
			_icicle = Sprite.FromFileSystem(FileSystems[ApplicationName], "/Sprites/Icicle.gorSprite");

			_icicle.UniformScale = _iceTiles.UniformScale = _guy.UniformScale = (Gorgon.Screen.Width / 320.0f) * 2.0f;

			_tileCountX = (int)(Gorgon.Screen.Width / _iceTiles.ScaledWidth);
			_tileCountY = (int)(Gorgon.Screen.Height / _iceTiles.ScaledHeight);

			_iceTiles.Depth = 0.9f;
			_icicle.Depth = 0.2f;
			_guy.Depth = 0.1f;

			_guy.SetPosition((Gorgon.Screen.Width / 2) + _guy.ScaledWidth, Gorgon.Screen.Height - _guy.ScaledHeight);
			_position = _guy.Position;			
			_guy.Animations["Guy_Walk_Up"].Enabled = true;
			_guy.Animations["Guy_Walk_Up"].AnimationState = AnimationState.Playing;

			_text = new TextSprite("Sprite", "", this.FrameworkFont, Drawing.Color.Black);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public MainForm()
			: base(@".\DeepAsAPuddle.xml")
		{
			InitializeComponent();
		}
		#endregion
	}
}