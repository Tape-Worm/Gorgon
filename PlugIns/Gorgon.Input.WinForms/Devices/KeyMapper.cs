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
		public IDictionary<Keys, KeyboardKey> KeyMapping
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create the keyboard mappings.
		/// </summary>
		private void CreateMappings()
		{
			KeyMapping.Add(Keys.None, KeyboardKey.None);
			KeyMapping.Add(Keys.LButton, KeyboardKey.LButton);
			KeyMapping.Add(Keys.RButton, KeyboardKey.RButton);
			KeyMapping.Add(Keys.Cancel, KeyboardKey.Cancel);
			KeyMapping.Add(Keys.MButton, KeyboardKey.MButton);
			KeyMapping.Add(Keys.XButton1, KeyboardKey.XButton1);
			KeyMapping.Add(Keys.XButton2, KeyboardKey.XButton2);
			KeyMapping.Add(Keys.Back, KeyboardKey.Back);
			KeyMapping.Add(Keys.Tab, KeyboardKey.Tab);
			KeyMapping.Add(Keys.LineFeed, KeyboardKey.LineFeed);
			KeyMapping.Add(Keys.Clear, KeyboardKey.Clear);
			KeyMapping.Add(Keys.Enter, KeyboardKey.Enter);
			KeyMapping.Add(Keys.ShiftKey, KeyboardKey.ShiftKey);
			KeyMapping.Add(Keys.ControlKey, KeyboardKey.ControlKey);
			KeyMapping.Add(Keys.Menu, KeyboardKey.Menu);
			KeyMapping.Add(Keys.Pause, KeyboardKey.Pause);
			KeyMapping.Add(Keys.CapsLock, KeyboardKey.CapsLock);
			KeyMapping.Add(Keys.KanaMode, KeyboardKey.KanaMode);
			KeyMapping.Add(Keys.JunjaMode, KeyboardKey.JunjaMode);
			KeyMapping.Add(Keys.FinalMode, KeyboardKey.FinalMode);
			KeyMapping.Add(Keys.KanjiMode, KeyboardKey.KanjiMode);
			KeyMapping.Add(Keys.Escape, KeyboardKey.Escape);
			KeyMapping.Add(Keys.IMEConvert, KeyboardKey.IMEConvert);
			KeyMapping.Add(Keys.IMENonconvert, KeyboardKey.IMENonconvert);
			KeyMapping.Add(Keys.IMEAccept, KeyboardKey.IMEAccept);
			KeyMapping.Add(Keys.IMEModeChange, KeyboardKey.IMEModeChange);
			KeyMapping.Add(Keys.Space, KeyboardKey.Space);
			KeyMapping.Add(Keys.PageUp, KeyboardKey.PageUp);
			KeyMapping.Add(Keys.PageDown, KeyboardKey.PageDown);
			KeyMapping.Add(Keys.End, KeyboardKey.End);
			KeyMapping.Add(Keys.Home, KeyboardKey.Home);
			KeyMapping.Add(Keys.Left, KeyboardKey.Left);
			KeyMapping.Add(Keys.Up, KeyboardKey.Up);
			KeyMapping.Add(Keys.Right, KeyboardKey.Right);
			KeyMapping.Add(Keys.Down, KeyboardKey.Down);
			KeyMapping.Add(Keys.Select, KeyboardKey.Select);
			KeyMapping.Add(Keys.Print, KeyboardKey.Print);
			KeyMapping.Add(Keys.Execute, KeyboardKey.Execute);
			KeyMapping.Add(Keys.PrintScreen, KeyboardKey.PrintScreen);
			KeyMapping.Add(Keys.Insert, KeyboardKey.Insert);
			KeyMapping.Add(Keys.Delete, KeyboardKey.Delete);
			KeyMapping.Add(Keys.Help, KeyboardKey.Help);
			KeyMapping.Add(Keys.D0, KeyboardKey.D0);
			KeyMapping.Add(Keys.D1, KeyboardKey.D1);
			KeyMapping.Add(Keys.D2, KeyboardKey.D2);
			KeyMapping.Add(Keys.D3, KeyboardKey.D3);
			KeyMapping.Add(Keys.D4, KeyboardKey.D4);
			KeyMapping.Add(Keys.D5, KeyboardKey.D5);
			KeyMapping.Add(Keys.D6, KeyboardKey.D6);
			KeyMapping.Add(Keys.D7, KeyboardKey.D7);
			KeyMapping.Add(Keys.D8, KeyboardKey.D8);
			KeyMapping.Add(Keys.D9, KeyboardKey.D9);
			KeyMapping.Add(Keys.A, KeyboardKey.A);
			KeyMapping.Add(Keys.B, KeyboardKey.B);
			KeyMapping.Add(Keys.C, KeyboardKey.C);
			KeyMapping.Add(Keys.D, KeyboardKey.D);
			KeyMapping.Add(Keys.E, KeyboardKey.E);
			KeyMapping.Add(Keys.F, KeyboardKey.F);
			KeyMapping.Add(Keys.G, KeyboardKey.G);
			KeyMapping.Add(Keys.H, KeyboardKey.H);
			KeyMapping.Add(Keys.I, KeyboardKey.I);
			KeyMapping.Add(Keys.J, KeyboardKey.J);
			KeyMapping.Add(Keys.K, KeyboardKey.K);
			KeyMapping.Add(Keys.L, KeyboardKey.L);
			KeyMapping.Add(Keys.M, KeyboardKey.M);
			KeyMapping.Add(Keys.N, KeyboardKey.N);
			KeyMapping.Add(Keys.O, KeyboardKey.O);
			KeyMapping.Add(Keys.P, KeyboardKey.P);
			KeyMapping.Add(Keys.Q, KeyboardKey.Q);
			KeyMapping.Add(Keys.R, KeyboardKey.R);
			KeyMapping.Add(Keys.S, KeyboardKey.S);
			KeyMapping.Add(Keys.T, KeyboardKey.T);
			KeyMapping.Add(Keys.U, KeyboardKey.U);
			KeyMapping.Add(Keys.V, KeyboardKey.V);
			KeyMapping.Add(Keys.W, KeyboardKey.W);
			KeyMapping.Add(Keys.X, KeyboardKey.X);
			KeyMapping.Add(Keys.Y, KeyboardKey.Y);
			KeyMapping.Add(Keys.Z, KeyboardKey.Z);
			KeyMapping.Add(Keys.LWin, KeyboardKey.LWin);
			KeyMapping.Add(Keys.RWin, KeyboardKey.RWin);
			KeyMapping.Add(Keys.Apps, KeyboardKey.Apps);
			KeyMapping.Add(Keys.Sleep, KeyboardKey.Sleep);
			KeyMapping.Add(Keys.NumPad0, KeyboardKey.NumPad0);
			KeyMapping.Add(Keys.NumPad1, KeyboardKey.NumPad1);
			KeyMapping.Add(Keys.NumPad2, KeyboardKey.NumPad2);
			KeyMapping.Add(Keys.NumPad3, KeyboardKey.NumPad3);
			KeyMapping.Add(Keys.NumPad4, KeyboardKey.NumPad4);
			KeyMapping.Add(Keys.NumPad5, KeyboardKey.NumPad5);
			KeyMapping.Add(Keys.NumPad6, KeyboardKey.NumPad6);
			KeyMapping.Add(Keys.NumPad7, KeyboardKey.NumPad7);
			KeyMapping.Add(Keys.NumPad8, KeyboardKey.NumPad8);
			KeyMapping.Add(Keys.NumPad9, KeyboardKey.NumPad9);
			KeyMapping.Add(Keys.Multiply, KeyboardKey.Multiply);
			KeyMapping.Add(Keys.Add, KeyboardKey.Add);
			KeyMapping.Add(Keys.Separator, KeyboardKey.Separator);
			KeyMapping.Add(Keys.Subtract, KeyboardKey.Subtract);
			KeyMapping.Add(Keys.Decimal, KeyboardKey.Decimal);
			KeyMapping.Add(Keys.Divide, KeyboardKey.Divide);
			KeyMapping.Add(Keys.F1, KeyboardKey.F1);
			KeyMapping.Add(Keys.F2, KeyboardKey.F2);
			KeyMapping.Add(Keys.F3, KeyboardKey.F3);
			KeyMapping.Add(Keys.F4, KeyboardKey.F4);
			KeyMapping.Add(Keys.F5, KeyboardKey.F5);
			KeyMapping.Add(Keys.F6, KeyboardKey.F6);
			KeyMapping.Add(Keys.F7, KeyboardKey.F7);
			KeyMapping.Add(Keys.F8, KeyboardKey.F8);
			KeyMapping.Add(Keys.F9, KeyboardKey.F9);
			KeyMapping.Add(Keys.F10, KeyboardKey.F10);
			KeyMapping.Add(Keys.F11, KeyboardKey.F11);
			KeyMapping.Add(Keys.F12, KeyboardKey.F12);
			KeyMapping.Add(Keys.F13, KeyboardKey.F13);
			KeyMapping.Add(Keys.F14, KeyboardKey.F14);
			KeyMapping.Add(Keys.F15, KeyboardKey.F15);
			KeyMapping.Add(Keys.F16, KeyboardKey.F16);
			KeyMapping.Add(Keys.F17, KeyboardKey.F17);
			KeyMapping.Add(Keys.F18, KeyboardKey.F18);
			KeyMapping.Add(Keys.F19, KeyboardKey.F19);
			KeyMapping.Add(Keys.F20, KeyboardKey.F20);
			KeyMapping.Add(Keys.F21, KeyboardKey.F21);
			KeyMapping.Add(Keys.F22, KeyboardKey.F22);
			KeyMapping.Add(Keys.F23, KeyboardKey.F23);
			KeyMapping.Add(Keys.F24, KeyboardKey.F24);
			KeyMapping.Add(Keys.NumLock, KeyboardKey.NumLock);
			KeyMapping.Add(Keys.Scroll, KeyboardKey.Scroll);
			KeyMapping.Add(Keys.LShiftKey, KeyboardKey.LShiftKey);
			KeyMapping.Add(Keys.RShiftKey, KeyboardKey.RShiftKey);
			KeyMapping.Add(Keys.LControlKey, KeyboardKey.LControlKey);
			KeyMapping.Add(Keys.RControlKey, KeyboardKey.RControlKey);
			KeyMapping.Add(Keys.LMenu, KeyboardKey.LMenu);
			KeyMapping.Add(Keys.RMenu, KeyboardKey.RMenu);
			KeyMapping.Add(Keys.BrowserBack, KeyboardKey.BrowserBack);
			KeyMapping.Add(Keys.BrowserForward, KeyboardKey.BrowserForward);
			KeyMapping.Add(Keys.BrowserRefresh, KeyboardKey.BrowserRefresh);
			KeyMapping.Add(Keys.BrowserStop, KeyboardKey.BrowserStop);
			KeyMapping.Add(Keys.BrowserSearch, KeyboardKey.BrowserSearch);
			KeyMapping.Add(Keys.BrowserFavorites, KeyboardKey.BrowserFavorites);
			KeyMapping.Add(Keys.BrowserHome, KeyboardKey.BrowserHome);
			KeyMapping.Add(Keys.VolumeMute, KeyboardKey.VolumeMute);
			KeyMapping.Add(Keys.VolumeDown, KeyboardKey.VolumeDown);
			KeyMapping.Add(Keys.VolumeUp, KeyboardKey.VolumeUp);
			KeyMapping.Add(Keys.MediaNextTrack, KeyboardKey.MediaNextTrack);
			KeyMapping.Add(Keys.MediaPreviousTrack, KeyboardKey.MediaPreviousTrack);
			KeyMapping.Add(Keys.MediaStop, KeyboardKey.MediaStop);
			KeyMapping.Add(Keys.MediaPlayPause, KeyboardKey.MediaPlayPause);
			KeyMapping.Add(Keys.LaunchMail, KeyboardKey.LaunchMail);
			KeyMapping.Add(Keys.SelectMedia, KeyboardKey.SelectMedia);
			KeyMapping.Add(Keys.LaunchApplication1, KeyboardKey.LaunchApplication1);
			KeyMapping.Add(Keys.LaunchApplication2, KeyboardKey.LaunchApplication2);
			KeyMapping.Add(Keys.Oem1, KeyboardKey.Oem1);
			KeyMapping.Add(Keys.Oemplus, KeyboardKey.Oemplus);
			KeyMapping.Add(Keys.Oemcomma, KeyboardKey.Oemcomma);
			KeyMapping.Add(Keys.OemMinus, KeyboardKey.OemMinus);
			KeyMapping.Add(Keys.OemPeriod, KeyboardKey.OemPeriod);
			KeyMapping.Add(Keys.OemQuestion, KeyboardKey.OemQuestion);
			KeyMapping.Add(Keys.Oemtilde, KeyboardKey.Oemtilde);
			KeyMapping.Add(Keys.Oem4, KeyboardKey.Oem4);
			KeyMapping.Add(Keys.OemPipe, KeyboardKey.OemPipe);
			KeyMapping.Add(Keys.Oem6, KeyboardKey.Oem6);
			KeyMapping.Add(Keys.Oem7, KeyboardKey.Oem7);
			KeyMapping.Add(Keys.Oem8, KeyboardKey.Oem8);
			KeyMapping.Add(Keys.Oem102, KeyboardKey.Oem102);
			KeyMapping.Add(Keys.ProcessKey, KeyboardKey.ProcessKey);
			KeyMapping.Add(Keys.Packet, KeyboardKey.Packet);
			KeyMapping.Add(Keys.Attn, KeyboardKey.Attn);
			KeyMapping.Add(Keys.Crsel, KeyboardKey.Crsel);
			KeyMapping.Add(Keys.Exsel, KeyboardKey.Exsel);
			KeyMapping.Add(Keys.EraseEof, KeyboardKey.EraseEof);
			KeyMapping.Add(Keys.Play, KeyboardKey.Play);
			KeyMapping.Add(Keys.Zoom, KeyboardKey.Zoom);
			KeyMapping.Add(Keys.NoName, KeyboardKey.NoName);
			KeyMapping.Add(Keys.Pa1, KeyboardKey.Pa1);
			KeyMapping.Add(Keys.OemClear, KeyboardKey.OemClear);
			KeyMapping.Add(Keys.KeyCode, KeyboardKey.KeyCode);
			KeyMapping.Add(Keys.Shift, KeyboardKey.Shift);
			KeyMapping.Add(Keys.Control, KeyboardKey.Control);
			KeyMapping.Add(Keys.Alt, KeyboardKey.Alt);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="KeyMapper"/> class.
		/// </summary>
		public KeyMapper()
		{
			KeyMapping = new Dictionary<Keys, KeyboardKey>();
			CreateMappings();
		}
		#endregion
	}
}
