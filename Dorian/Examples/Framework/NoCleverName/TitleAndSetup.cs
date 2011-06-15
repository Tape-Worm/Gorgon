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
using Drawing = System.Drawing;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.InputDevices;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Object representing the title and the set up menu.
	/// </summary>
	public class TitleAndSetup
	{
		#region Variables.
		private Font _font = null;				// Font to use for drawing.
		private TextSprite _text = null;		// Text to display.
		private bool _atTitle = false;			// Flag to indicate that we're at the title level.
		private Input _input = null;			// Input interface.
		private int _maxSticks = 0;				// Maximum stick count.
		private Joystick _selectedStick = null;	// Selected stick.
		#endregion

		#region Events.
		/// <summary>
		/// Event fired when we want to close the title screen.
		/// </summary>
		public event EventHandler TitleClosed;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether we're at the title level or not.
		/// </summary>
		public bool AtTitle
		{
			get
			{
				return _atTitle;
			}
		}

		/// <summary>
		/// Property to return the selected joystick.
		/// </summary>
		public Joystick SelectedStick
		{
			get
			{
				return _selectedStick;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to close the title screen.
		/// </summary>
		private void CloseTitle()
		{
			if (TitleClosed != null)
				TitleClosed(this, EventArgs.Empty);
		}

		/// <summary>
		/// Handles the KeyDown event of the Keyboard control.
		/// </summary>
		/// <param name="e">The <see cref="GorgonLibrary.InputDevices.KeyboardInputEventArgs"/> instance containing the event data.</param>
		public void KeyDown(KeyboardInputEventArgs e)
		{
			switch (e.Key)
			{
				case KeyboardKeys.J:
					if ((_atTitle) && (_maxSticks > 0))
					{
						if (_maxSticks > 1)
							_atTitle = false;
						else
						{
							_selectedStick = _input.Joysticks[0];
							CloseTitle();
						}
					}
					break;
				case KeyboardKeys.M:
					if ((_atTitle) && (_maxSticks > 0))
						CloseTitle();

					if ((!_atTitle) && (_maxSticks > 0))
					{
						_selectedStick = null;
						_atTitle = true;
					}
					break;
				default:
					// Get key mapping.
					if ((!_atTitle) && (_maxSticks > 0))
					{
						int stickIndex = -1;		// Stick index.

						// Convert the index.
						int.TryParse(_input.Keyboard.KeyMappings[e.Key].Character.ToString(), out stickIndex);

						if ((stickIndex >= 0) && (stickIndex < _maxSticks))
						{
							_selectedStick = _input.Joysticks[stickIndex];
							CloseTitle();
						}
					}

					if ((_atTitle) && (_maxSticks < 1))
						CloseTitle();
					break;
			}
		}

		/// <summary>
		/// Function to draw the 'title' screen.
		/// </summary>
		public void DrawTitle()
		{
			Gorgon.Screen.Clear(Drawing.Color.Black);

			if (_atTitle)
			{
				_text.Text = "Gorgon Example Game.\n\nBy Michael Winsor (Tape_Worm)\nFeel free to rip this off however you see fit.";
				_text.Alignment = Alignment.Center;
				_text.Draw();

				if (_maxSticks > 0)
				{
					_text.Text = "Use the (m)ouse or (j)oystick/gamepad";
					_text.Alignment = Alignment.LowerCenter;
					_text.Draw();

					// Check for a button press on ANY joystick, whichever stick
					// had a button press is the one we'll use.
					for (int i = 0; i < _maxSticks; i++)
					{
						for (int button = 0; button < _input.Joysticks[i].ButtonCount; button++)
						{
							if (_input.Joysticks[i].Button[button])
							{
								_selectedStick = _input.Joysticks[i];
								CloseTitle();
							}
						}
					}
				}
				else
				{
					_text.Text = "Press any key to continue.";
					_text.Alignment = Alignment.LowerCenter;
					_text.Draw();
				}
			}
			else
			{
				StringBuilder stickList = null;				// Stick list.

				stickList = new StringBuilder();
				_text.Text = "Select a joystick/gamepad:\n";
				_text.Alignment = Alignment.UpperLeft;
				_text.WordWrap = true;

				// List the joysticks.
				for (int i = 0; i < _maxSticks; i++)
				{
					if (stickList.Length > 0)
						stickList.Append("\n");
					stickList.Append(i.ToString());
					stickList.Append(")\t");
					stickList.Append(_input.Joysticks[i].Name);
				}

				stickList.Append("\nM)\tMain menu");

				_text.Text += stickList.ToString();

				_text.Draw();				
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="TitleAndSetup"/> class.
		/// </summary>
		/// <param name="textFont">The text font.</param>
		public TitleAndSetup(Font textFont, Input input)
		{
			_font = textFont;
			_text = new TextSprite("TitleText", string.Empty, _font, Drawing.Color.White);
			_text.WordWrap = true;
			_input = input;
			if (_input.Joysticks != null)
				_maxSticks = _input.Joysticks.Count;
			if (_maxSticks > 5)
				_maxSticks = 5;
			_atTitle = true;

			if (_maxSticks > 0)
			{
				// Enable the joysticks.
				for (int i = 0; i < _maxSticks; i++)
					_input.Joysticks[i].Enabled = true;
			}
		}
		#endregion
	}
}
