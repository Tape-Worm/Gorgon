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

		/// <summary>
		/// Available animations.
		/// </summary>
		public enum Animations
		{
			SpinNScale = 0,
			Bounce = 1,
			Psychedelic = 2
		}
		#endregion

		#region Variables.
		private Random _rnd = new Random();								// Random number generator.
		private TextSprite _text = null;								// Text sprite.
		private Sprite _iceTiles = null;								// Ice tiles.
		private Sprite _icicle = null;									// Icicle.
		private Sprite _guy = null;										// Guy.
		private Image _spriteImage = null;								// Image used for the sprites.
		private int _tileCountX = 0;									// Horizontal tile count.
		private int _tileCountY = 0;									// Vertical tile count.
		private Vector2D _position = Vector2D.Zero;						// Position for guy.
		private AnimationSet _animSet;									// Animation set.
		private RenderImage _screen = null;								// Output screen.
		private Sprite _screenSprite = null;							// Screen sprite.
		private Animations _currentAnimation = Animations.SpinNScale;	// Current animation.
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
		/// Function to update the logic for the background animation.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		private void GuyLogicUpdate(FrameEventArgs e)
		{
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
						"Icicle depth:" + _icicle.Depth.ToString() + "\nCharacter depth:" + _guy.Depth.ToString();
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
							+ _icicle.Depth.ToString() + "\nCharacter depth:" + _guy.Depth.ToString();
					break;
			}
		}

		/// <summary>
		/// Function to draw the guy animation.
		/// </summary>
		private void DrawGuy()
		{
			Gorgon.CurrentRenderTarget = _screen;
			_screen.Clear();
			DrawBackground();

			_guy.Position = MathUtility.Round(_position);
			_guy.Draw();

			// Note how we're drawing the icicle AFTER the character, but when the character is below the icicle, it won't overwrite it.
			_icicle.SetPosition(((_tileCountX - 2) / 2) * _iceTiles.ScaledWidth, ((_tileCountY - 2) / 2) * _iceTiles.ScaledHeight);
			_icicle.Draw();

			_text.Draw();
			Gorgon.CurrentRenderTarget = null;
		}

		/// <summary>
		/// Function to create our animations.
		/// </summary>
		private void CreateAnimations()
		{
			Animation anim = null;

			_screenSprite.Animations.Clear();

			// Scale n Spin animation.			
			anim = new Animation("SpinNScale", 3000.0f);
			_screenSprite.Animations.Add(anim);
			anim.Looped = true;
			anim.Tracks["Color"].AddKey(new KeyColor(0.0f, Drawing.Color.White));
			anim.Tracks["Position"].AddKey(new KeyVector2D(0.0f, new Vector2D(Gorgon.Screen.Width / 2, Gorgon.Screen.Height / 2)));
			anim.Tracks["ScaledWidth"].AddKey(new KeyFloat(0.0f, 3.0f));
			anim.Tracks["ScaledWidth"].AddKey(new KeyFloat(1500.0f, Gorgon.Screen.Width * 1.5f));
			anim.Tracks["ScaledWidth"].AddKey(new KeyFloat(3000.0f, 3.0f));

			anim.Tracks["ScaledHeight"].CopyKeysFrom(anim.Tracks["ScaledWidth"]);

			anim.Tracks["Rotation"].InterpolationMode = InterpolationMode.Spline;
			anim.Tracks["Rotation"].AddKey(new KeyFloat(0.0f, 0.0f));
			anim.Tracks["Rotation"].AddKey(new KeyFloat(1500.0f, 360.0f));
			anim.Tracks["Rotation"].AddKey(new KeyFloat(3000.0f, 0.0f));
			
			// Bounce animation.			
			anim = new Animation("Bounce", 1500.0f);
			_screenSprite.Animations.Add(anim);
			anim.Looped = true;
			anim.Tracks["Color"].AddKey(new KeyColor(0.0f, Drawing.Color.White));

			anim.Tracks["UniformScale"].AddKey(new KeyFloat(0.0f, 0.25f));
			anim.Tracks["UniformScale"].AddKey(new KeyFloat(1500.0f, 1.0f));

			anim.Tracks["Position"].InterpolationMode = InterpolationMode.Spline;			
			anim.Tracks["Position"].AddKey(new KeyVector2D(0.0f, new Vector2D(-(_screenSprite.Width * 0.25f), Gorgon.Screen.Height - (_screenSprite.Height * 0.25f))));
			anim.Tracks["Position"].AddKey(new KeyVector2D(250.0f, new Vector2D(0, Gorgon.Screen.Height / 2)));
			anim.Tracks["Position"].AddKey(new KeyVector2D(500.0f, new Vector2D(Gorgon.Screen.Width / 4, Gorgon.Screen.Height - (_screenSprite.Height * 0.25f))));			
			anim.Tracks["Position"].AddKey(new KeyVector2D(750.0f, new Vector2D(Gorgon.Screen.Width / 2, Gorgon.Screen.Height / 2)));
			anim.Tracks["Position"].AddKey(new KeyVector2D(1000.0f, new Vector2D(Gorgon.Screen.Width - (Gorgon.Screen.Width / 4), Gorgon.Screen.Height - (_screenSprite.Height * 0.5f))));
			anim.Tracks["Position"].AddKey(new KeyVector2D(1250.0f, new Vector2D(Gorgon.Screen.Width, Gorgon.Screen.Height)));
			anim.Tracks["Position"].AddKey(new KeyVector2D(1500.0f, new Vector2D(Gorgon.Screen.Width + (_screenSprite.Width * 0.25f), -_screenSprite.Height)));			

			// Psychedelic animation.			
			anim = new Animation("Psychedelic", 6000.0f);
			_screenSprite.Animations.Add(anim);
			anim.Looped = true;
			anim.Tracks["ScaledWidth"].AddKey(new KeyFloat(0.0f, 384.0f));
			anim.Tracks["ScaledHeight"].AddKey(anim.Tracks["ScaledWidth"].GetKeyAtIndex(0).Clone());
			anim.Tracks["Position"].AddKey(new KeyVector2D(0.0f, new Vector2D(Gorgon.Screen.Width / 2 , Gorgon.Screen.Height / 2)));

			anim.Tracks["Color"].AddKey(new KeyColor(0.0f, Drawing.Color.FromArgb(_rnd.Next(0, 255),_rnd.Next(0, 255),_rnd.Next(0, 255))));
			anim.Tracks["Color"].AddKey(new KeyColor(3000.0f, Drawing.Color.FromArgb(_rnd.Next(0, 255), _rnd.Next(0, 255), _rnd.Next(0, 255))));
			anim.Tracks["Color"].AddKey(new KeyColor(5000.0f, Drawing.Color.White));

			anim.Tracks["Rotation"].InterpolationMode = InterpolationMode.Spline;
			anim.Tracks["Rotation"].AddKey(new KeyFloat(0.0f, 0.0f));
			anim.Tracks["Rotation"].AddKey(new KeyFloat(4750.0f, 0.0f));
			anim.Tracks["Rotation"].AddKey(new KeyFloat(5000.0f, 360.0f));

			KeyFrame cloneKey = anim.Tracks["Color"].GetKeyAtIndex(0).Clone();
			cloneKey.Time = 6000.0f;
			anim.Tracks["Color"].AddKey(cloneKey);

			_screenSprite.Animations[_currentAnimation.ToString()].AnimationState = AnimationState.Playing;
		}

		/// <summary>
		/// Function to perform logic updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnLogicUpdate(FrameEventArgs e)
		{
			base.OnLogicUpdate(e);
						
			GuyLogicUpdate(e);

			_screenSprite.Animations[_currentAnimation.ToString()].Advance(e.FrameDeltaTime * 1000.0f);
		}

		/// <summary>
		/// Function to perform rendering updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnFrameUpdate(FrameEventArgs e)
		{
			base.OnFrameUpdate(e);

			DrawGuy();

			_text.Text = "Mouse button 1 - Jump to next animation.\n" + "ESC - Close the application.\nCTRL + F - Show frame statistics.\n\nCurrent animation: " + 
				_currentAnimation.ToString();			
			_screenSprite.Draw();
			_text.Draw();
		}

		/// <summary>
		/// Function called when a mouse button is pushed down.
		/// </summary>
		/// <param name="e">Mouse event parameters.</param>
		protected override void OnMouseButtonDown(MouseInputEventArgs e)
		{
			base.OnMouseButtonDown(e);

			foreach(Animation anim in _screenSprite.Animations)
			{
				anim.AnimationState = AnimationState.Stopped;
				anim.Reset();
			}

			switch (_currentAnimation)
			{
				case Animations.SpinNScale:
					_currentAnimation = Animations.Bounce;
					break;
				case Animations.Bounce:
					_currentAnimation = Animations.Psychedelic;
					break;
				case Animations.Psychedelic:
					_currentAnimation = Animations.SpinNScale;
					break;
			}

			_screenSprite.Animations[_currentAnimation.ToString()].AnimationState = AnimationState.Playing;
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

			if ((Gorgon.Screen.Width != _screen.Width) || (Gorgon.Screen.Height != _screen.Height))
			{
				_screen.SetDimensions(Gorgon.Screen.Width, Gorgon.Screen.Height);
				CreateAnimations();
			}
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

			_screen = new RenderImage("Screen", Gorgon.Screen.Width, Gorgon.Screen.Height, ImageBufferFormats.BufferRGB888X8, true, false);
			_screenSprite = new Sprite("ScreenSprite", _screen);
			_screenSprite.ScaledWidth = 384;
			_screenSprite.ScaledHeight = 384;
			_screenSprite.Axis = new Vector2D(_screenSprite.Width / 2, _screenSprite.Height / 2);
			CreateAnimations();
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public MainForm()
			: base(@".\AnimatedDebate.xml")
		{
			InitializeComponent();
		}
		#endregion
	}
}