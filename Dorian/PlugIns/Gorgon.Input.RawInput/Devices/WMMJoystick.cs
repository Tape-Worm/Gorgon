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
// Created: Friday, June 24, 2011 10:02:29 AM
// 
#endregion

using System.Runtime.InteropServices;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Native;
using GorgonLibrary.Math;
using GorgonLibrary.Input;

namespace GorgonLibrary.Input.Raw
{
	/// <summary>
	/// Windows Multimedia based joystick interface.
	/// </summary>
	internal class WMMJoystick
		: GorgonJoystick
	{
		#region Classes.
		/// <summary>
		/// Defines the button states for the joystick.
		/// </summary>
		public class WMMJoystickButtons
			: WMMJoystick.JoystickButtons
		{
			#region Methods.
			/// <summary>
			/// Function to set the button state.
			/// </summary>
			/// <param name="index">Index of the button.</param>
			/// <param name="state">State to set.</param>
			public void SetButtonState(int index, bool state)
			{
				this[index] = new JoystickButtonState(this[index].Name, state);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="WMMJoystickButtons"/> class.
			/// </summary>
			/// <param name="buttonCount">The button count.</param>
			public WMMJoystickButtons(int buttonCount)
			{
				for (int i = 0; i < buttonCount; i++)
					DefineButton("Button " + i.ToString());
			}
			#endregion
		}

		/// <summary>
		/// Defines the capabilities of the joystick.
		/// </summary>
		public class WMMJoystickCapabilities
			: WMMJoystick.JoystickCapabilities
		{
			#region Methods.
			/// <summary>
			/// Function to retrieve the joystick capabilities.
			/// </summary>
			/// <param name="joystickID">ID of the joystick.</param>
			private void GetCaps(int joystickID)
			{
				JOYCAPS caps = default(JOYCAPS);									// Joystick capabilities.
				int error = 0;
				JoystickCapabilityFlags capsFlags = JoystickCapabilityFlags.None;	// Extra capability flags.

				error = Win32API.joyGetDevCaps(joystickID, ref caps, Marshal.SizeOf(typeof(JOYCAPS)));

				// If the joystick is disconnected then leave.
				if (error == 0xA7)
					return;

				// If it's any other error, then throw an exception.
				if (error > 0)
					throw new GorgonException(GorgonResult.CannotRead, "Cannot read capabilities from joystick ID (" + joystickID.ToString() + ").\nError code:" + error.ToString());

				// Gather device info.
				if ((caps.Capabilities & JoystickCaps.HasZ) == JoystickCaps.HasZ)
				{
					capsFlags |= JoystickCapabilityFlags.SupportsThrottle;
					ThrottleAxisRange = new GorgonMinMax((int)caps.MinimumZ, (int)caps.MaximumZ);
				}
				if ((caps.Capabilities & JoystickCaps.HasRudder) == JoystickCaps.HasRudder)
				{
					capsFlags |= JoystickCapabilityFlags.SupportsRudder;
					RudderAxisRange = new GorgonMinMax((int)caps.MinimumRudder, (int)caps.MaximumRudder);
				}
				if ((caps.Capabilities & JoystickCaps.HasPOV) == JoystickCaps.HasPOV)
				{
					capsFlags |= JoystickCapabilityFlags.SupportsPOV;
					if ((caps.Capabilities & JoystickCaps.POV4Directions) == JoystickCaps.POV4Directions)
						capsFlags |= JoystickCapabilityFlags.SupportsDiscreetPOV;
					if ((caps.Capabilities & JoystickCaps.POVContinuousDegreeBearings) == JoystickCaps.POVContinuousDegreeBearings)
						capsFlags |= JoystickCapabilityFlags.SupportsContinuousPOV;
				}

				if ((caps.Capabilities & JoystickCaps.HasU) == JoystickCaps.HasU)
				{
					capsFlags |= JoystickCapabilityFlags.SupportsSecondaryXAxis;
					SecondaryXAxisRange = new GorgonMinMax((int)caps.Axis5Minimum, (int)caps.Axis5Maximum);
				}

				if ((caps.Capabilities & JoystickCaps.HasV) == JoystickCaps.HasV)
				{
					capsFlags |= JoystickCapabilityFlags.SupportsSecondaryYAxis;
					SecondaryYAxisRange = new GorgonMinMax((int)caps.Axis6Minimum, (int)caps.Axis6Maximum);
				}

				ExtraCapabilities = capsFlags;
				AxisCount = (int)caps.AxisCount;
				ButtonCount = (int)caps.ButtonCount;
				ManufacturerID = caps.ManufacturerID;
				ProductID = caps.ProductID;

				// Get primary axis ranges.  Force the range to split into halfs going from negative to positive so that 0 is our center.
				XAxisRange = new GorgonMinMax(-((int)caps.MaximumX / 2) - 1, ((int)caps.MaximumX / 2)); 
				YAxisRange = new GorgonMinMax(-((int)caps.MaximumY / 2) - 1, ((int)caps.MaximumY / 2));
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="WMMJoystickCapabilities"/> class.
			/// </summary>
			public WMMJoystickCapabilities(int joystickID)
				: base()
			{
				// Return an empty capabilities set.
				if (joystickID < 0)
					return;

				GetCaps(joystickID);
			}
			#endregion
		}
		#endregion

		#region Variables.
		private int _joystickID = 0;				// ID of the joystick.
		WMMJoystickButtons _buttonStates = null;	// Button states.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the device is acquired or not.
		/// </summary>		
		public override bool Acquired
		{
			get
			{
				return base.Acquired;
			}
			set
			{
				base.Acquired = value;

				if (value)
					DeviceLost = false;
			}
		}

		/// <summary>
		/// Property to set or return whether the window has exclusive access or not.  For joysticks, this is always TRUE.
		/// </summary>		
		public override bool Exclusive
		{
			get
			{
				return true;
			}
			set
			{				
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to shift the value to fall within the axis range.
		/// </summary>
		/// <param name="currentValue">Value to shift.</param>
		/// <param name="axisRange">Range to evaluate.</param>
		/// <returns>The shifted value.</returns>
		private int CenterValue(int currentValue, GorgonMinMax axisRange)
		{
			return currentValue - (axisRange.Range / 2);
		}

		/// <summary>
		/// Function to retrieve the state of the joystick.
		/// </summary>
		/// <returns></returns>
		private JOYINFOEX GetWin32JoystickData()
		{
			JOYINFOEX joyInfo = new JOYINFOEX();		// Joystick information.
			int error = 0;								// Error code.

			// Set up joystick info.
			joyInfo.Size = Marshal.SizeOf(typeof(JOYINFOEX));
			joyInfo.Flags = JoystickInfoFlags.ReturnButtons | JoystickInfoFlags.ReturnX | JoystickInfoFlags.ReturnY;

			// Determine which data we want to return.
			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsThrottle) == JoystickCapabilityFlags.SupportsThrottle)
				joyInfo.Flags |= JoystickInfoFlags.ReturnZ;
			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsSecondaryXAxis) == JoystickCapabilityFlags.SupportsSecondaryXAxis)
				joyInfo.Flags |= JoystickInfoFlags.ReturnAxis5;
			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsSecondaryYAxis) == JoystickCapabilityFlags.SupportsSecondaryYAxis)
				joyInfo.Flags |= JoystickInfoFlags.ReturnAxis6;
			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsRudder) == JoystickCapabilityFlags.SupportsRudder)
			    joyInfo.Flags |= JoystickInfoFlags.ReturnRudder;
			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsPOV) == JoystickCapabilityFlags.SupportsPOV)
			{
				joyInfo.Flags |= JoystickInfoFlags.ReturnPOV;
				if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsContinuousPOV) == JoystickCapabilityFlags.SupportsContinuousPOV)
					joyInfo.Flags |= JoystickInfoFlags.ReturnPOVContinuousDegreeBearings;
			}

			error = Win32API.joyGetPosEx(_joystickID, ref joyInfo);
			
			// If the joystick is disconnected then leave.
			if (error == 0xA7)
			{
				IsConnected = false;
				return default(JOYINFOEX);
			}
			
			// If it's any other error, then throw an exception.
			if (error > 0)
				throw new GorgonException(GorgonResult.CannotRead, "Cannot read data from joystick ID (" + _joystickID.ToString() + ").\nError code:" + error.ToString());

			IsConnected = true;

			return joyInfo;
		}

		/// <summary>
		/// Function to initalize the data for the joystick.
		/// </summary>
		protected override JoystickCapabilities GetCapabilities()
		{
			return new WMMJoystick.WMMJoystickCapabilities(_joystickID);
		}

		/// <summary>
		/// Function to retrieve the buttons for the joystick/gamepad.
		/// </summary>
		/// <returns>
		/// The list of buttons for the joystick/gamepad.
		/// </returns>
		/// <remarks>Implementors must implement this method so the object can get the list of buttons for the device.</remarks>
		protected override GorgonJoystick.JoystickButtons GetButtons()
		{
			_buttonStates = new WMMJoystickButtons(Capabilities.ButtonCount);
			return _buttonStates;
		}

		/// <summary>
		/// Function to poll the joystick for data.
		/// </summary>
		protected override void PollJoystick()
		{
			JOYINFOEX joyInfo = new JOYINFOEX();		// Joystick information.

			joyInfo = GetWin32JoystickData();

			// Do nothing if we're not connected.
			if (!IsConnected)
				return;

			// Get primary axis data.  Center between the range.
			X = CenterValue((int)joyInfo.X, Capabilities.XAxisRange);
			Y = -CenterValue((int)joyInfo.Y, Capabilities.YAxisRange);

			// Get secondary axis data.
			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsSecondaryXAxis) == JoystickCapabilityFlags.SupportsSecondaryXAxis)
				SecondaryX = CenterValue((int)joyInfo.Axis5, Capabilities.SecondaryXAxisRange);
			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsSecondaryYAxis) == JoystickCapabilityFlags.SupportsSecondaryYAxis)
				SecondaryY = -CenterValue((int)joyInfo.Axis6, Capabilities.SecondaryYAxisRange);

			// Get throttle/rudder info.
			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsThrottle) == JoystickCapabilityFlags.SupportsThrottle)
				Throttle = CenterValue((int)joyInfo.Z, Capabilities.ThrottleAxisRange);
			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsRudder) == JoystickCapabilityFlags.SupportsRudder)
				Rudder = CenterValue((int)joyInfo.Rudder, Capabilities.RudderAxisRange);
						
			// Get POV data.
			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsPOV) == JoystickCapabilityFlags.SupportsPOV)
				POV = (int)joyInfo.POV;

			// Update buttons.
			for (int i = 0; i < _buttonStates.Count; i++)
			{
				if ((joyInfo.Buttons & (JoystickButton)(1 << i)) != 0)
					_buttonStates.SetButtonState(i, true);
				else
					_buttonStates.SetButtonState(i, false);
			}
		}

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected override void UnbindDevice()
		{
		}

		/// <summary>
		/// Function to bind the input device.
		/// </summary>
		protected override void BindDevice()
		{
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="WMMJoystick"/> class.
		/// </summary>
		/// <param name="owner">The input factory that owns this device.</param>
		/// <param name="ID">The ID of the joystick.</param>
		/// <param name="name">The name of the joystick.</param>
		/// <param name="boundWindow">The window to bind the joystick with.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</remarks>
		internal WMMJoystick(GorgonRawInputFactory owner, int ID, string name, Control boundWindow)
			: base(owner, name, boundWindow)
		{
			_joystickID = ID;
			Initialize();
			Gorgon.Log.Print("Windows multimedia joystick device ID 0x{0} interface created.", LoggingLevel.Verbose, ID.ToString("x").PadLeft(8, '0'));
		}
		#endregion
	}
}
