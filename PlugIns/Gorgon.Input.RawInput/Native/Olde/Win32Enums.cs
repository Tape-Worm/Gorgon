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
// Created: Friday, June 24, 2011 10:04:58 AM
// 
#endregion

using System;

namespace Gorgon.Native
{
    // ReSharper disable InconsistentNaming
	#region Enumerations
	/// <summary>
	/// Enumeration containing the joystick information flags.
	/// </summary>
	[Flags]
	enum JoystickInfoFlags
	{
		/// <summary></summary>
		All = (ReturnX | ReturnY | ReturnZ | ReturnRudder | ReturnAxis5 | ReturnAxis6 | ReturnPOV | ReturnButtons),
		/// <summary></summary>
		ReturnX = 0x00000001,
		/// <summary></summary>
		ReturnY = 0x00000002,
		/// <summary></summary>
		ReturnZ = 0x00000004,
		/// <summary></summary>
		ReturnRudder = 0x00000008,
		/// <summary></summary>
		ReturnAxis5 = 0x00000010,
		/// <summary></summary>
		ReturnAxis6 = 0x00000020,
		/// <summary></summary>
		ReturnPOV = 0x00000040,
		/// <summary></summary>
		ReturnButtons = 0x00000080,
		/// <summary></summary>
		ReturnRawData = 0x00000100,
		/// <summary></summary>
		ReturnPOVContinuousDegreeBearings = 0x00000200,
		/// <summary></summary>
		ReturnCentered = 0x00000400,
		/// <summary></summary>
		UseDeadzone = 0x00000800,
		/// <summary></summary>
		CalibrationReadAlways = 0x00010000,
		/// <summary></summary>
		CalibrationReadXYOnly = 0x00020000,
		/// <summary></summary>
		CalibrationRead3 = 0x00040000,
		/// <summary></summary>
		CalibrationRead4 = 0x00080000,
		/// <summary></summary>
		CalibrationReadXOnly = 0x00100000,
		/// <summary></summary>
		CalibrationReadYOnly = 0x00200000,
		/// <summary></summary>
		CalibrationRead5 = 0x00400000,
		/// <summary></summary>
		CalibrationRead6 = 0x00800000,
		/// <summary></summary>
		CalibrationReadZOnly = 0x01000000,
		/// <summary></summary>
		CalibrationReadRudderOnly = 0x02000000,
		/// <summary></summary>
		CalibrationReadAxis5Only = 0x04000000,
		/// <summary></summary>
		CalibrationReadaxis6Only = 0x08000000
	}

	/// <summary>
	/// Enumeration for joystick buttons.
	/// </summary>
	[Flags]
	enum JoystickButton
		: uint
	{
		/// <summary></summary>
		Button1 = 0x0001,
		/// <summary></summary>
		Button2 = 0x0002,
		/// <summary></summary>
		Button3 = 0x0003,
		/// <summary></summary>
		Button4 = 0x0004,
		/// <summary></summary>
		Button1Changed = 0x0100,
		/// <summary></summary>
		Button2Changed = 0x0200,
		/// <summary></summary>
		Button3Changed = 0x0400,
		/// <summary></summary>
		Button4Changed = 0x0800,
		/// <summary></summary>
		Button5 = 0x00000010,
		/// <summary></summary>
		Button6 = 0x00000020,
		/// <summary></summary>
		Button7 = 0x00000040,
		/// <summary></summary>
		Button8 = 0x00000080,
		/// <summary></summary>
		Button9 = 0x00000100,
		/// <summary></summary>
		Button10 = 0x00000200,
		/// <summary></summary>
		Button11 = 0x00000400,
		/// <summary></summary>
		Button12 = 0x00000800,
		/// <summary></summary>
		Button13 = 0x00001000,
		/// <summary></summary>
		Button14 = 0x00002000,
		/// <summary></summary>
		Button15 = 0x00004000,
		/// <summary></summary>
		Button16 = 0x00008000,
		/// <summary></summary>
		Button17 = 0x00010000,
		/// <summary></summary>
		Button18 = 0x00020000,
		/// <summary></summary>
		Button19 = 0x00040000,
		/// <summary></summary>
		Button20 = 0x00080000,
		/// <summary></summary>
		Button21 = 0x00100000,
		/// <summary></summary>
		Button22 = 0x00200000,
		/// <summary></summary>
		Button23 = 0x00400000,
		/// <summary></summary>
		Button24 = 0x00800000,
		/// <summary></summary>
		Button25 = 0x01000000,
		/// <summary></summary>
		Button26 = 0x02000000,
		/// <summary></summary>
		Button27 = 0x04000000,
		/// <summary></summary>
		Button28 = 0x08000000,
		/// <summary></summary>
		Button29 = 0x10000000,
		/// <summary></summary>
		Button30 = 0x20000000,
		/// <summary></summary>
		Button31 = 0x40000000,
		/// <summary></summary>
		Button32 = 0x80000000
	}

	/// <summary>
	/// Enumeration for joystick capabilities.
	/// </summary>
	[Flags]
	enum JoystickCaps
	{
		/// <summary>Has a Z axis.</summary>
		HasZ = 0x0001,
		/// <summary>Has a rudder axis.</summary>
		HasRudder = 0x0002,
		/// <summary>Has a 5th axis.</summary>
		HasU = 0x0004,
		/// <summary>Has a 6th axis.</summary>
		HasV = 0x0008,
		/// <summary>Has a POV hat.</summary>
		HasPOV = 0x0010,
		/// <summary>Has a 4 direction POV hat.</summary>
		POV4Directions = 0x0020,
		/// <summary>Has a continuous degree bearing POV hat.</summary>
		POVContinuousDegreeBearings = 0x0040
	}
	#endregion
    // ReSharper restore InconsistentNaming
}
