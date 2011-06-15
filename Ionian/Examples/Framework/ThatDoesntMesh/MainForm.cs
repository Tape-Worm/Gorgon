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
using GorgonLibrary.Graphics.Utilities;
using GorgonLibrary.InputDevices;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Main application form.
	/// </summary>
	public partial class MainForm 
		: GorgonApplicationWindow
	{
		#region Variables.
		private TextSprite _text = null;					// Text for instruction.
		private Random _rnd = new Random();					// Random number generator.
		private SpriteMesh _mesh = null;					// Sprite mesh.
		private Image _nebulaImage = null;					// Nebula image.
		private Vector2D _point = Vector2D.Zero;			// Translation point.
		private Vector2D _swirl = Vector2D.Zero;			// Swirl.
		private float _angle = 0.0f;						// Angle.
		private PreciseTimer _timer = new PreciseTimer();	// Timer.
		private float _disturbance = 50.0f;					// Brush disturbance.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform logic updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnLogicUpdate(FrameEventArgs e)
		{
			base.OnLogicUpdate(e);

			int col, row;		
			int mCol = 0;
			int mRow = 0;
			Vector2D mousePos = Input.Mouse.Position;

			_angle += 180.0f * e.FrameDeltaTime;

			if (_angle >= 359.999f)
				_angle = 0.0f;

			mCol = (int)((mousePos.X / _mesh.Scale.X) / _mesh.ColumnWidth);
			mRow = (int)((mousePos.Y / _mesh.Scale.Y) / _mesh.RowHeight);

			// Every 25 milliseconds we will advance the scene revert.
			if (_timer.Milliseconds > 25)
			{
				for (int y = 0; y < _mesh.Rows; y++)
				{
					for (int x = 0; x < _mesh.Columns; x++)
					{
						_point = _mesh.GetVertexPosition(x, y);
						Vector2D dest = new Vector2D(x * _mesh.ColumnWidth, y * _mesh.RowHeight);
						Vector2D diff = Vector2D.Subtract(dest, _point);
						float r = 0, g = 0, b = 0;

						// Reset the colors.
						r = _mesh.GetVertexColor(x, y).R;
						g = _mesh.GetVertexColor(x, y).G;
						b = _mesh.GetVertexColor(x, y).B;

						r += MathUtility.Round(384.0f * e.FrameDeltaTime);
						g += MathUtility.Round(384.0f * e.FrameDeltaTime);
						b += MathUtility.Round(384.0f * e.FrameDeltaTime);

						if (r > 255)
							r = 255;
						if (g > 255)
							g = 255;
						if (b > 255)
							b = 255;

						_mesh.SetVertexColor(x, y, Drawing.Color.FromArgb((int)r, (int)g, (int)b));

						if (diff.X < -0.5f)
							_point.X -= 5.0f * e.FrameDeltaTime;
						if (diff.X > 0.5f)
							_point.X += 5.0f * e.FrameDeltaTime;
						if (diff.Y < -0.5f)
							_point.Y -= 5.0f * e.FrameDeltaTime;
						if (diff.Y > 0.5f)
							_point.Y += 5.0f * e.FrameDeltaTime;
						_mesh.SetVertexPosition(x, y, _point.X, _point.Y);
					}
				}
				_timer.Reset();
			}

			for (int y = -6; y < 7; y++)
			{
				for (int x = -6; x < 7; x++)
				{
					col = mCol + x;
					row = mRow + y;
					if ((col >= 0) && (row >= 0) && (col < _mesh.Columns) && (row < _mesh.Rows))
					{
						_point.X = col * _mesh.ColumnWidth;
						_point.Y = row * _mesh.RowHeight;
						_point = Vector2D.Add(_point, new Vector2D(_rnd.Next((int)-_mesh.ColumnWidth, (int)_mesh.ColumnWidth) * e.FrameDeltaTime * _disturbance, _rnd.Next((int)-_mesh.RowHeight, (int)_mesh.RowHeight) * e.FrameDeltaTime * _disturbance));
						_swirl.X = (MathUtility.Cos(MathUtility.Radians(_angle))) - (MathUtility.Sin(MathUtility.Radians(_angle)));
						_swirl.Y = (MathUtility.Cos(MathUtility.Radians(_angle))) + (MathUtility.Sin(MathUtility.Radians(_angle)));
						_mesh.SetVertexPosition(col, row, _point.X + _swirl.X, _point.Y + _swirl.Y);
						Drawing.Color baseColor = _mesh.GetVertexColor(col, row);
						_mesh.SetVertexColor(col, row, Drawing.Color.FromArgb(_rnd.Next((int)(baseColor.R / 1.15f), 255), _rnd.Next((int)(baseColor.G / 1.15f), 255), _rnd.Next(baseColor.B / 2, 255)));
					}
				}
			}

			_text.Text = "Mouse wheel - increase/decrease the disturbance of the brush.\n\nDisturbance: " + _disturbance.ToString("0.0");
		}

		/// <summary>
		/// Function to perform rendering updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnFrameUpdate(FrameEventArgs e)
		{
			base.OnFrameUpdate(e);
			_mesh.Draw();
			_text.Draw();
		}

		/// <summary>
		/// Function called when a mouse scroll wheel is scrolled.
		/// </summary>
		/// <param name="e">Mouse event parameters.</param>
		protected override void OnMouseWheelScrolled(MouseInputEventArgs e)
		{
			base.OnMouseWheelScrolled(e);

			if (e.WheelDelta < 0)
				_disturbance -= 10.0f;
			if (e.WheelDelta > 0)
				_disturbance += 10.0f;

			if (_disturbance < 50.0f)
				_disturbance = 50.0f;
			if (_disturbance > 250.0f)
				_disturbance = 250.0f;
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
		}

		/// <summary>
		/// Function to provide initialization for our example.
		/// </summary>
		protected override void Initialize()
		{
			float scale = 0.0f;

			Gorgon.GlobalStateSettings.GlobalSmoothing = Smoothing.MagnificationSmooth;

			_nebulaImage = Image.FromFile(ResourcePath + @"\..\Images\eagle_nebula_heic0506b_f.jpg");
			_mesh = new SpriteMesh("SpriteMesh", _nebulaImage, _nebulaImage.Height / 10, _nebulaImage.Width / 10);
			scale = (640.0f / _nebulaImage.Width) * ((Gorgon.Screen.Width + (Gorgon.Screen.Width / 10.0f)) / 640.0f);			
			_mesh.SetScale(scale,scale);
			_mesh.Columns = (int)(_mesh.ScaledWidth / 10);
			_mesh.Rows = (int)(_mesh.ScaledHeight / 10);

			_text = new TextSprite("Instructions", string.Empty, this.FrameworkFont, Drawing.Color.White);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public MainForm()
			: base(@".\ThatDoesntMesh.xml")
		{
			InitializeComponent();
		}
		#endregion
	}
}