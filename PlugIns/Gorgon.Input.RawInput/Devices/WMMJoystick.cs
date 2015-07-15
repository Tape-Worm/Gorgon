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

using Gorgon.Core;
using Gorgon.Input.Raw.Properties;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// Windows Multimedia based joystick interface.
	/// </summary>
	internal class MultimediaJoystick
		: GorgonJoystick
	{
		#region Variables.
		private readonly int _joystickID;				// ID of the joystick.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether the window has exclusive access or not.
		/// </summary>
		public override bool Exclusive
		{
			get
			{
				return false;
			}
			set
			{
				// Intentionally left blank.
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
		private static int CenterValue(int currentValue, GorgonRange axisRange)
		{
			return currentValue - (axisRange.Range / 2);
		}

		/// <summary>
		/// Function to retrieve the state of the joystick.
		/// </summary>
		/// <returns></returns>
		private JOYINFOEX GetWin32JoystickData()
		{
			var joyInfo = new JOYINFOEX
			    {
			        Size = DirectAccess.SizeOf<JOYINFOEX>(),
			        Flags = JoystickInfoFlags.ReturnButtons | JoystickInfoFlags.ReturnX | JoystickInfoFlags.ReturnY
			    };		// Joystick information.

		    // Determine which data we want to return.
		    if ((Info.Capabilities & JoystickCapabilityFlags.SupportsThrottle) == JoystickCapabilityFlags.SupportsThrottle)
		    {
		        joyInfo.Flags |= JoystickInfoFlags.ReturnZ;
		    }

		    if ((Info.Capabilities & JoystickCapabilityFlags.SupportsSecondaryXAxis) == JoystickCapabilityFlags.SupportsSecondaryXAxis)
		    {
		        joyInfo.Flags |= JoystickInfoFlags.ReturnAxis5;
		    }

		    if ((Info.Capabilities & JoystickCapabilityFlags.SupportsSecondaryYAxis) == JoystickCapabilityFlags.SupportsSecondaryYAxis)
		    {
		        joyInfo.Flags |= JoystickInfoFlags.ReturnAxis6;
		    }

		    if ((Info.Capabilities & JoystickCapabilityFlags.SupportsRudder) == JoystickCapabilityFlags.SupportsRudder)
		    {
		        joyInfo.Flags |= JoystickInfoFlags.ReturnRudder;
		    }

		    if ((Info.Capabilities & JoystickCapabilityFlags.SupportsPOV) == JoystickCapabilityFlags.SupportsPOV)
			{
				joyInfo.Flags |= JoystickInfoFlags.ReturnPOV;

			    if ((Info.Capabilities & JoystickCapabilityFlags.SupportsContinuousPOV) == JoystickCapabilityFlags.SupportsContinuousPOV)
			    {
			        joyInfo.Flags |= JoystickInfoFlags.ReturnPOVContinuousDegreeBearings;
			    }
			}

			int error = Win32API.joyGetPosEx(_joystickID, ref joyInfo);
			
			// If the joystick is disconnected then leave.
			if (error == 0xA7)
			{
				return default(JOYINFOEX);
			}
			
			// If it's any other error, then throw an exception.
		    if (error > 0)
		    {
		        throw new GorgonException(GorgonResult.CannotRead,
		                                  string.Format(Resources.GORINP_RAW_ERR_CANNOT_READ_JOYSTICK_DATA, _joystickID,
		                                                error.FormatHex()));
		    }

			return joyInfo;
		}

        /// <summary>
        /// Function called when the <see cref="GorgonInputDevice.Acquired" /> property changes its value.
        /// </summary>
	    protected override void OnAcquisitionChanged()
	    {
	        if (!Acquired)
	        {
	            return;
	        }

	        DeviceLost = true;
	    }

		/// <summary>
		/// Function to poll the joystick for data.
		/// </summary>
		protected override void PollJoystick()
		{
		    JOYINFOEX joyInfo = GetWin32JoystickData();

			// Do nothing if we're not connected.
		    if (!Info.IsConnected)
		    {
		        return;
		    }

		    // Get primary axis data.  Center between the range.
			Axis[JoystickAxis.XAxis] = CenterValue((int)joyInfo.X, Info.AxisRanges[JoystickAxis.XAxis]);
			Axis[JoystickAxis.YAxis] = -CenterValue((int)joyInfo.Y, Info.AxisRanges[JoystickAxis.YAxis]);

			// Get secondary axis data.
		    if ((Info.Capabilities & JoystickCapabilityFlags.SupportsSecondaryXAxis) == JoystickCapabilityFlags.SupportsSecondaryXAxis)
		    {
				Axis[JoystickAxis.XAxis2] = CenterValue((int)joyInfo.Axis5, Info.AxisRanges[JoystickAxis.XAxis2]);
		    }

			if ((Info.Capabilities & JoystickCapabilityFlags.SupportsSecondaryXAxis) == JoystickCapabilityFlags.SupportsSecondaryXAxis)
			{
				Axis[JoystickAxis.YAxis2] = -CenterValue((int)joyInfo.Axis6, Info.AxisRanges[JoystickAxis.YAxis2]);
			}

		    // Get throttle/rudder info.
		    if ((Info.Capabilities & JoystickCapabilityFlags.SupportsThrottle) == JoystickCapabilityFlags.SupportsThrottle)
		    {
				Axis[JoystickAxis.Throttle] = CenterValue((int)joyInfo.Z, Info.AxisRanges[JoystickAxis.Throttle]);
		    }

		    if ((Info.Capabilities & JoystickCapabilityFlags.SupportsRudder) == JoystickCapabilityFlags.SupportsRudder)
		    {
				Axis[JoystickAxis.Rudder] = CenterValue((int)joyInfo.Rudder, Info.AxisRanges[JoystickAxis.Rudder]);
		    }

		    // Get POV data.
		    if ((Info.Capabilities & JoystickCapabilityFlags.SupportsPOV) == JoystickCapabilityFlags.SupportsPOV)
		    {
		        POV = (int)joyInfo.POV;
		    }

		    // Update buttons.
			for (int i = 0; i < Button.Count; i++)
			{
				Button[i] = (joyInfo.Buttons & (JoystickButton)(1 << i)) != 0 ? JoystickButtonState.Down : JoystickButtonState.Up;
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
		/// Initializes a new instance of the <see cref="MultimediaJoystick"/> class.
		/// </summary>
		/// <param name="owner">The input factory that owns this device.</param>
		/// <param name="deviceInfo"><inheritdoc/></param>
		internal MultimediaJoystick(GorgonInputService owner, IMultimediaJoystickInfo deviceInfo)
			: base(owner, deviceInfo)
		{
			_joystickID = deviceInfo.ID;
		}
		#endregion
	}
}
