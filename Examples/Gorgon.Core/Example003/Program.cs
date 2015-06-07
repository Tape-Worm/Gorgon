﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Tuesday, September 18, 2012 8:00:02 PM
// 
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Timing;
using Gorgon.UI;

namespace Gorgon.Examples
{
	/// <summary>
	/// Entry point class.
	/// </summary>
	/// <remarks>This example is tiny bit more advanced.  It'll show how to use an application context with Gorgon and how to 
	/// dynamically switch idle loops on the fly.</remarks>
	static class Program
	{
		#region Variables.
		private static readonly Random _rnd = new Random();	// Random number generator.
		private static int _lastX;						    // Last horizontal coordinate.
		private static int _lastY;						    // Last vertical coordinate.
		private static float _lastTime;					    // Last time we drew.
		private static int _color;						    // Color for the bars.
		private static int _component;					    // Color component.
		#endregion

		#region Methods.
		/// <summary>
		/// Function that's called during idle time.
		/// </summary>
		/// <returns><b>true</b> to continue execution, <b>false</b> to stop.</returns>
		/// <remarks>This is the secondary default idle loop.</remarks>
		public static bool NewIdle()
		{
			var form = (formMain)GorgonApplication.ApplicationContext.MainForm;		// Get our main form from the context.
			
			// Draw some bars every 16 ms.
			if (GorgonTiming.MillisecondsSinceStart - _lastTime >= 16.6f)
			{
				Color newColor = Color.Transparent;

				switch (_component)
				{
					case 0:
						newColor = Color.FromArgb(_color, 0, 0);
						break;
					case 1:
						newColor = Color.FromArgb(0, _color, 0);
						break;
					case 2:
						newColor = Color.FromArgb(0, 0, _color);
						break;
				}

				_lastTime = GorgonTiming.MillisecondsSinceStart;

				form.Draw(_lastX, _lastY, _lastX, (form.GraphicsSize.Height - 1) - (_lastY), newColor);

				_color += 3;
				_lastX++;

				if (_color >= 255)
				{
					_color = 0;
					_component++;
					if (_component > 2)
						_component = 0;
				}

				if (_lastX >= form.GraphicsSize.Width)
					_lastX = 0;

				_lastY = _rnd.Next(0, form.GraphicsSize.Height / 4);
			}

			form.Flip();

			form.DrawFPS("Secondary Idle Loop - FPS: " + GorgonTiming.FPS.ToString("0.0"));

			return true;
		}

		/// <summary>
		/// Function that's called during idle time.
		/// </summary>
		/// <returns><b>true</b> to continue execution, <b>false</b> to stop.</returns>
		/// <remarks>This is the default idle loop.</remarks>
		public static bool Idle()
		{
			var form = (formMain)GorgonApplication.ApplicationContext.MainForm;		// Get our main form from the context.

			int x = _rnd.Next(0, form.GraphicsSize.Width - 1);
			int y = _rnd.Next(0, form.GraphicsSize.Height - 1);

			// Draw a connected line on the form every 256 milliseconds.
			// This will run continously until the application has ended.
			if (GorgonTiming.MillisecondsSinceStart - _lastTime >= 256)
			{
				_lastTime = GorgonTiming.MillisecondsSinceStart;
				form.Draw(_lastX, _lastY, x, y, Color.FromArgb(_rnd.Next(0, 255), _rnd.Next(0, 255), _rnd.Next(0, 255)));
				_lastX = x;
				_lastY = y;
			}
			else
				form.Draw(x, y, x, y, Color.FromArgb(_rnd.Next(0, 255), _rnd.Next(0, 255), _rnd.Next(0, 255)));

			// Flip the buffer.
			form.Flip();

			form.DrawFPS("Primary Idle Loop - FPS: " + GorgonTiming.FPS.ToString("0.0"));

			return true;
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				// Get the initial time.
				_lastTime = GorgonTiming.MillisecondsSinceStart;

				// Run the application context with an idle loop.
				//
				// Here we specify that we want to run an application context and an idle loop.  The idle loop 
				// will kick in after the main form displays.
				GorgonApplication.Run(new Context(), Idle);
			}
			catch (Exception ex)
			{
				ex.Catch(_ => GorgonDialogs.ErrorBox(null, _), GorgonApplication.Log);
			}
		}
		#endregion
	}
}
