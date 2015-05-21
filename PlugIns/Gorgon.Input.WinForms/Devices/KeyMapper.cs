#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Friday, July 15, 2011 6:47:05 AM
// 
#endregion

using System.Collections.Generic;
using System.Windows.Forms;

namespace Gorgon.Input.WinForms
{
	/// <summary>
	/// Used to map win forms keys to gorgon keyboard keys.
	/// </summary>
	internal class KeyMapper
	{
		#region Properties.
		/// <summary>
		/// Property to return the keyboard mappings.
		/// </summary>
		public IDictionary<Keys, KeyboardKeys> KeyMapping
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create the keyboard mappings.
		/// </summary>
		private void CreateMappings()
		{
			KeyMapping.Add(Keys.None, KeyboardKeys.None);
			KeyMapping.Add(Keys.LButton, KeyboardKeys.LButton);
			KeyMapping.Add(Keys.RButton, KeyboardKeys.RButton);
			KeyMapping.Add(Keys.Cancel, KeyboardKeys.Cancel);
			KeyMapping.Add(Keys.MButton, KeyboardKeys.MButton);
			KeyMapping.Add(Keys.XButton1, KeyboardKeys.XButton1);
			KeyMapping.Add(Keys.XButton2, KeyboardKeys.XButton2);
			KeyMapping.Add(Keys.Back, KeyboardKeys.Back);
			KeyMapping.Add(Keys.Tab, KeyboardKeys.Tab);
			KeyMapping.Add(Keys.LineFeed, KeyboardKeys.LineFeed);
			KeyMapping.Add(Keys.Clear, KeyboardKeys.Clear);
			KeyMapping.Add(Keys.Enter, KeyboardKeys.Enter);
			KeyMapping.Add(Keys.ShiftKey, KeyboardKeys.ShiftKey);
			KeyMapping.Add(Keys.ControlKey, KeyboardKeys.ControlKey);
			KeyMapping.Add(Keys.Menu, KeyboardKeys.Menu);
			KeyMapping.Add(Keys.Pause, KeyboardKeys.Pause);
			KeyMapping.Add(Keys.CapsLock, KeyboardKeys.CapsLock);
			KeyMapping.Add(Keys.KanaMode, KeyboardKeys.KanaMode);
			KeyMapping.Add(Keys.JunjaMode, KeyboardKeys.JunjaMode);
			KeyMapping.Add(Keys.FinalMode, KeyboardKeys.FinalMode);
			KeyMapping.Add(Keys.KanjiMode, KeyboardKeys.KanjiMode);
			KeyMapping.Add(Keys.Escape, KeyboardKeys.Escape);
			KeyMapping.Add(Keys.IMEConvert, KeyboardKeys.IMEConvert);
			KeyMapping.Add(Keys.IMENonconvert, KeyboardKeys.IMENonconvert);
			KeyMapping.Add(Keys.IMEAccept, KeyboardKeys.IMEAccept);
			KeyMapping.Add(Keys.IMEModeChange, KeyboardKeys.IMEModeChange);
			KeyMapping.Add(Keys.Space, KeyboardKeys.Space);
			KeyMapping.Add(Keys.PageUp, KeyboardKeys.PageUp);
			KeyMapping.Add(Keys.PageDown, KeyboardKeys.PageDown);
			KeyMapping.Add(Keys.End, KeyboardKeys.End);
			KeyMapping.Add(Keys.Home, KeyboardKeys.Home);
			KeyMapping.Add(Keys.Left, KeyboardKeys.Left);
			KeyMapping.Add(Keys.Up, KeyboardKeys.Up);
			KeyMapping.Add(Keys.Right, KeyboardKeys.Right);
			KeyMapping.Add(Keys.Down, KeyboardKeys.Down);
			KeyMapping.Add(Keys.Select, KeyboardKeys.Select);
			KeyMapping.Add(Keys.Print, KeyboardKeys.Print);
			KeyMapping.Add(Keys.Execute, KeyboardKeys.Execute);
			KeyMapping.Add(Keys.PrintScreen, KeyboardKeys.PrintScreen);
			KeyMapping.Add(Keys.Insert, KeyboardKeys.Insert);
			KeyMapping.Add(Keys.Delete, KeyboardKeys.Delete);
			KeyMapping.Add(Keys.Help, KeyboardKeys.Help);
			KeyMapping.Add(Keys.D0, KeyboardKeys.D0);
			KeyMapping.Add(Keys.D1, KeyboardKeys.D1);
			KeyMapping.Add(Keys.D2, KeyboardKeys.D2);
			KeyMapping.Add(Keys.D3, KeyboardKeys.D3);
			KeyMapping.Add(Keys.D4, KeyboardKeys.D4);
			KeyMapping.Add(Keys.D5, KeyboardKeys.D5);
			KeyMapping.Add(Keys.D6, KeyboardKeys.D6);
			KeyMapping.Add(Keys.D7, KeyboardKeys.D7);
			KeyMapping.Add(Keys.D8, KeyboardKeys.D8);
			KeyMapping.Add(Keys.D9, KeyboardKeys.D9);
			KeyMapping.Add(Keys.A, KeyboardKeys.A);
			KeyMapping.Add(Keys.B, KeyboardKeys.B);
			KeyMapping.Add(Keys.C, KeyboardKeys.C);
			KeyMapping.Add(Keys.D, KeyboardKeys.D);
			KeyMapping.Add(Keys.E, KeyboardKeys.E);
			KeyMapping.Add(Keys.F, KeyboardKeys.F);
			KeyMapping.Add(Keys.G, KeyboardKeys.G);
			KeyMapping.Add(Keys.H, KeyboardKeys.H);
			KeyMapping.Add(Keys.I, KeyboardKeys.I);
			KeyMapping.Add(Keys.J, KeyboardKeys.J);
			KeyMapping.Add(Keys.K, KeyboardKeys.K);
			KeyMapping.Add(Keys.L, KeyboardKeys.L);
			KeyMapping.Add(Keys.M, KeyboardKeys.M);
			KeyMapping.Add(Keys.N, KeyboardKeys.N);
			KeyMapping.Add(Keys.O, KeyboardKeys.O);
			KeyMapping.Add(Keys.P, KeyboardKeys.P);
			KeyMapping.Add(Keys.Q, KeyboardKeys.Q);
			KeyMapping.Add(Keys.R, KeyboardKeys.R);
			KeyMapping.Add(Keys.S, KeyboardKeys.S);
			KeyMapping.Add(Keys.T, KeyboardKeys.T);
			KeyMapping.Add(Keys.U, KeyboardKeys.U);
			KeyMapping.Add(Keys.V, KeyboardKeys.V);
			KeyMapping.Add(Keys.W, KeyboardKeys.W);
			KeyMapping.Add(Keys.X, KeyboardKeys.X);
			KeyMapping.Add(Keys.Y, KeyboardKeys.Y);
			KeyMapping.Add(Keys.Z, KeyboardKeys.Z);
			KeyMapping.Add(Keys.LWin, KeyboardKeys.LWin);
			KeyMapping.Add(Keys.RWin, KeyboardKeys.RWin);
			KeyMapping.Add(Keys.Apps, KeyboardKeys.Apps);
			KeyMapping.Add(Keys.Sleep, KeyboardKeys.Sleep);
			KeyMapping.Add(Keys.NumPad0, KeyboardKeys.NumPad0);
			KeyMapping.Add(Keys.NumPad1, KeyboardKeys.NumPad1);
			KeyMapping.Add(Keys.NumPad2, KeyboardKeys.NumPad2);
			KeyMapping.Add(Keys.NumPad3, KeyboardKeys.NumPad3);
			KeyMapping.Add(Keys.NumPad4, KeyboardKeys.NumPad4);
			KeyMapping.Add(Keys.NumPad5, KeyboardKeys.NumPad5);
			KeyMapping.Add(Keys.NumPad6, KeyboardKeys.NumPad6);
			KeyMapping.Add(Keys.NumPad7, KeyboardKeys.NumPad7);
			KeyMapping.Add(Keys.NumPad8, KeyboardKeys.NumPad8);
			KeyMapping.Add(Keys.NumPad9, KeyboardKeys.NumPad9);
			KeyMapping.Add(Keys.Multiply, KeyboardKeys.Multiply);
			KeyMapping.Add(Keys.Add, KeyboardKeys.Add);
			KeyMapping.Add(Keys.Separator, KeyboardKeys.Separator);
			KeyMapping.Add(Keys.Subtract, KeyboardKeys.Subtract);
			KeyMapping.Add(Keys.Decimal, KeyboardKeys.Decimal);
			KeyMapping.Add(Keys.Divide, KeyboardKeys.Divide);
			KeyMapping.Add(Keys.F1, KeyboardKeys.F1);
			KeyMapping.Add(Keys.F2, KeyboardKeys.F2);
			KeyMapping.Add(Keys.F3, KeyboardKeys.F3);
			KeyMapping.Add(Keys.F4, KeyboardKeys.F4);
			KeyMapping.Add(Keys.F5, KeyboardKeys.F5);
			KeyMapping.Add(Keys.F6, KeyboardKeys.F6);
			KeyMapping.Add(Keys.F7, KeyboardKeys.F7);
			KeyMapping.Add(Keys.F8, KeyboardKeys.F8);
			KeyMapping.Add(Keys.F9, KeyboardKeys.F9);
			KeyMapping.Add(Keys.F10, KeyboardKeys.F10);
			KeyMapping.Add(Keys.F11, KeyboardKeys.F11);
			KeyMapping.Add(Keys.F12, KeyboardKeys.F12);
			KeyMapping.Add(Keys.F13, KeyboardKeys.F13);
			KeyMapping.Add(Keys.F14, KeyboardKeys.F14);
			KeyMapping.Add(Keys.F15, KeyboardKeys.F15);
			KeyMapping.Add(Keys.F16, KeyboardKeys.F16);
			KeyMapping.Add(Keys.F17, KeyboardKeys.F17);
			KeyMapping.Add(Keys.F18, KeyboardKeys.F18);
			KeyMapping.Add(Keys.F19, KeyboardKeys.F19);
			KeyMapping.Add(Keys.F20, KeyboardKeys.F20);
			KeyMapping.Add(Keys.F21, KeyboardKeys.F21);
			KeyMapping.Add(Keys.F22, KeyboardKeys.F22);
			KeyMapping.Add(Keys.F23, KeyboardKeys.F23);
			KeyMapping.Add(Keys.F24, KeyboardKeys.F24);
			KeyMapping.Add(Keys.NumLock, KeyboardKeys.NumLock);
			KeyMapping.Add(Keys.Scroll, KeyboardKeys.Scroll);
			KeyMapping.Add(Keys.LShiftKey, KeyboardKeys.LShiftKey);
			KeyMapping.Add(Keys.RShiftKey, KeyboardKeys.RShiftKey);
			KeyMapping.Add(Keys.LControlKey, KeyboardKeys.LControlKey);
			KeyMapping.Add(Keys.RControlKey, KeyboardKeys.RControlKey);
			KeyMapping.Add(Keys.LMenu, KeyboardKeys.LMenu);
			KeyMapping.Add(Keys.RMenu, KeyboardKeys.RMenu);
			KeyMapping.Add(Keys.BrowserBack, KeyboardKeys.BrowserBack);
			KeyMapping.Add(Keys.BrowserForward, KeyboardKeys.BrowserForward);
			KeyMapping.Add(Keys.BrowserRefresh, KeyboardKeys.BrowserRefresh);
			KeyMapping.Add(Keys.BrowserStop, KeyboardKeys.BrowserStop);
			KeyMapping.Add(Keys.BrowserSearch, KeyboardKeys.BrowserSearch);
			KeyMapping.Add(Keys.BrowserFavorites, KeyboardKeys.BrowserFavorites);
			KeyMapping.Add(Keys.BrowserHome, KeyboardKeys.BrowserHome);
			KeyMapping.Add(Keys.VolumeMute, KeyboardKeys.VolumeMute);
			KeyMapping.Add(Keys.VolumeDown, KeyboardKeys.VolumeDown);
			KeyMapping.Add(Keys.VolumeUp, KeyboardKeys.VolumeUp);
			KeyMapping.Add(Keys.MediaNextTrack, KeyboardKeys.MediaNextTrack);
			KeyMapping.Add(Keys.MediaPreviousTrack, KeyboardKeys.MediaPreviousTrack);
			KeyMapping.Add(Keys.MediaStop, KeyboardKeys.MediaStop);
			KeyMapping.Add(Keys.MediaPlayPause, KeyboardKeys.MediaPlayPause);
			KeyMapping.Add(Keys.LaunchMail, KeyboardKeys.LaunchMail);
			KeyMapping.Add(Keys.SelectMedia, KeyboardKeys.SelectMedia);
			KeyMapping.Add(Keys.LaunchApplication1, KeyboardKeys.LaunchApplication1);
			KeyMapping.Add(Keys.LaunchApplication2, KeyboardKeys.LaunchApplication2);
			KeyMapping.Add(Keys.Oem1, KeyboardKeys.Oem1);
			KeyMapping.Add(Keys.Oemplus, KeyboardKeys.Oemplus);
			KeyMapping.Add(Keys.Oemcomma, KeyboardKeys.Oemcomma);
			KeyMapping.Add(Keys.OemMinus, KeyboardKeys.OemMinus);
			KeyMapping.Add(Keys.OemPeriod, KeyboardKeys.OemPeriod);
			KeyMapping.Add(Keys.OemQuestion, KeyboardKeys.OemQuestion);
			KeyMapping.Add(Keys.Oemtilde, KeyboardKeys.Oemtilde);
			KeyMapping.Add(Keys.Oem4, KeyboardKeys.Oem4);
			KeyMapping.Add(Keys.OemPipe, KeyboardKeys.OemPipe);
			KeyMapping.Add(Keys.Oem6, KeyboardKeys.Oem6);
			KeyMapping.Add(Keys.Oem7, KeyboardKeys.Oem7);
			KeyMapping.Add(Keys.Oem8, KeyboardKeys.Oem8);
			KeyMapping.Add(Keys.Oem102, KeyboardKeys.Oem102);
			KeyMapping.Add(Keys.ProcessKey, KeyboardKeys.ProcessKey);
			KeyMapping.Add(Keys.Packet, KeyboardKeys.Packet);
			KeyMapping.Add(Keys.Attn, KeyboardKeys.Attn);
			KeyMapping.Add(Keys.Crsel, KeyboardKeys.Crsel);
			KeyMapping.Add(Keys.Exsel, KeyboardKeys.Exsel);
			KeyMapping.Add(Keys.EraseEof, KeyboardKeys.EraseEof);
			KeyMapping.Add(Keys.Play, KeyboardKeys.Play);
			KeyMapping.Add(Keys.Zoom, KeyboardKeys.Zoom);
			KeyMapping.Add(Keys.NoName, KeyboardKeys.NoName);
			KeyMapping.Add(Keys.Pa1, KeyboardKeys.Pa1);
			KeyMapping.Add(Keys.OemClear, KeyboardKeys.OemClear);
			KeyMapping.Add(Keys.KeyCode, KeyboardKeys.KeyCode);
			KeyMapping.Add(Keys.Shift, KeyboardKeys.Shift);
			KeyMapping.Add(Keys.Control, KeyboardKeys.Control);
			KeyMapping.Add(Keys.Alt, KeyboardKeys.Alt);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="KeyMapper"/> class.
		/// </summary>
		public KeyMapper()
		{
			KeyMapping = new Dictionary<Keys, KeyboardKeys>();
			CreateMappings();
		}
		#endregion
	}
}
