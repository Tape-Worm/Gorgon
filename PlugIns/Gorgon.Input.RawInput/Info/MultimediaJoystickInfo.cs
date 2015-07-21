using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Input.Raw.Properties;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// Information for a Windows Multimedia Joystick interface.
	/// </summary>
	class MultimediaJoystickInfo
		: IMultimediaJoystickInfo
	{
		#region Variables.
		// The size of the JOYCAPS struct, in bytes.
		// We have to use Marshal.SizeOf because the structure contains marshalling information, which our pointer types don't support directly.
		private readonly int _capsSize = Marshal.SizeOf(typeof(JOYCAPS));
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="MultimediaJoystickInfo"/> class.
		/// </summary>
		/// <param name="uuid">The unique identifier for the joystick.</param>
		/// <param name="name">The name of the joystick.</param>
		/// <param name="joystickID">The joystick identifier.</param>
		public MultimediaJoystickInfo(Guid uuid, string name, int joystickID)
		{
			UUID = uuid;
			Name = name;
			ID = joystickID;
			VibrationMotorRanges = new GorgonRange[0];
			AxisRanges = new GorgonJoystickAxisRangeList(new KeyValuePair<int, GorgonRange>[0]);
		}
		#endregion

		#region IMultimediaJoystickInfo Members
		#region Properties.
		/// <inheritdoc/>
		public int ID
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the capability flags for the joystick.
		/// </summary>
		/// <param name="joyCaps">Windows multimedia joystick capabilities value.</param>
		/// <returns>The joystick capability flags.</returns>
		private static JoystickCapabilityFlags GetCapabilities(ref JOYCAPS joyCaps)
		{
			JoystickCapabilityFlags result = JoystickCapabilityFlags.None; 

			// Gather device info.
			if ((joyCaps.Capabilities & JoystickCaps.HasZ) == JoystickCaps.HasZ)
			{
				result |= JoystickCapabilityFlags.SupportsThrottle;
			}

			if ((joyCaps.Capabilities & JoystickCaps.HasRudder) == JoystickCaps.HasRudder)
			{
				result |= JoystickCapabilityFlags.SupportsRudder;
			}

			if ((joyCaps.Capabilities & JoystickCaps.HasPOV) == JoystickCaps.HasPOV)
			{
				result |= JoystickCapabilityFlags.SupportsPOV;
				if ((joyCaps.Capabilities & JoystickCaps.POV4Directions) == JoystickCaps.POV4Directions)
				{
					result |= JoystickCapabilityFlags.SupportsDiscreetPOV;
				}

				if ((joyCaps.Capabilities & JoystickCaps.POVContinuousDegreeBearings) == JoystickCaps.POVContinuousDegreeBearings)
				{
					result |= JoystickCapabilityFlags.SupportsContinuousPOV;
				}
			}

			if ((joyCaps.Capabilities & JoystickCaps.HasU) == JoystickCaps.HasU)
			{
				result |= JoystickCapabilityFlags.SupportsSecondaryXAxis;
			}

			if ((joyCaps.Capabilities & JoystickCaps.HasV) != JoystickCaps.HasV)
			{
				return result;
			}

			result |= JoystickCapabilityFlags.SupportsSecondaryYAxis;

			return result;
		}

		/// <summary>
		/// Function to retrieve the ranges for all axes.
		/// </summary>
		/// <returns>The list of axis ranges mapped to the appropriate axes.</returns>
		private IList<KeyValuePair<int, GorgonRange>> GetRanges(ref JOYCAPS joyCaps)
		{
			// For X & Y axes, shift the range so that 0 indicates that the joystick is centered.
			// This also keeps it equivalent to the values returned in the XInput plug in so we can have consistency.
			List<KeyValuePair<int, GorgonRange>> result = new List<KeyValuePair<int, GorgonRange>>
			                                              {
				                                              new KeyValuePair<int, GorgonRange>((int)JoystickAxis.XAxis,
				                                                                                 new GorgonRange(-((int)joyCaps.MaximumX / 2) - 1, ((int)joyCaps.MaximumX / 2))),
				                                              new KeyValuePair<int, GorgonRange>((int)JoystickAxis.YAxis,
				                                                                                 new GorgonRange(-((int)joyCaps.MaximumY / 2) - 1, ((int)joyCaps.MaximumY / 2)))
			                                              };
			
			if ((Capabilities & JoystickCapabilityFlags.SupportsThrottle) == JoystickCapabilityFlags.SupportsThrottle)
			{
				result.Add(new KeyValuePair<int, GorgonRange>((int)JoystickAxis.Throttle, new GorgonRange((int)joyCaps.MinimumZ, (int)joyCaps.MaximumZ)));
			}

			if ((Capabilities & JoystickCapabilityFlags.SupportsRudder) == JoystickCapabilityFlags.SupportsRudder)
			{
				result.Add(new KeyValuePair<int, GorgonRange>((int)JoystickAxis.Rudder, new GorgonRange((int)joyCaps.MinimumRudder, (int)joyCaps.MaximumRudder)));
			}

			// For X & Y axes, shift the range so that 0 indicates that the joystick is centered.
			if ((Capabilities & JoystickCapabilityFlags.SupportsSecondaryXAxis) == JoystickCapabilityFlags.SupportsSecondaryXAxis)
			{
				result.Add(new KeyValuePair<int, GorgonRange>((int)JoystickAxis.XAxis2, new GorgonRange(-((int)joyCaps.Axis5Minimum / 2) - 1, (int)joyCaps.Axis5Maximum / 2)));
			}

			if ((Capabilities & JoystickCapabilityFlags.SupportsSecondaryYAxis) == JoystickCapabilityFlags.SupportsSecondaryYAxis)
			{
				result.Add(new KeyValuePair<int, GorgonRange>((int)JoystickAxis.YAxis2, new GorgonRange(-((int)joyCaps.Axis6Minimum / 2) - 1, (int)joyCaps.Axis6Maximum / 2)));
			}

			return result;
		}

		/// <inheritdoc/>
		public void GetCapsData()
		{
			JOYCAPS caps = default(JOYCAPS);									// Joystick capabilities.

			// NOTE: We have to use Marshal.SizeOf here because DirectAccess.SizeOf does -not- check data marshalling attributes
			//       when determining the size of a structure (this is a performance decision).  So, for structures that have 
			//       marshalled strings and/or fixed size arrays that need marshalling attributes (like MarshalAsAttribute), then 
			//		 we -must- use Marshal.SizeOf, otherwise we should use DirectAccess.SizeOf.
			int error = Win32API.joyGetDevCaps(ID, ref caps, Marshal.SizeOf(typeof(JOYCAPS)));

			// If it's any other error, then throw an exception.
			if (error > 0)
			{
				throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORINP_RAW_ERR_CANNOT_GET_JOYSTICK_CAPS, ID, error.FormatHex()));
			}

			// Get capabilities.
			Capabilities = GetCapabilities(ref caps);
			
			// Get axis ranges.
			AxisRanges = new GorgonJoystickAxisRangeList(GetRanges(ref caps));
			ButtonCount = (int)caps.ButtonCount;
			ManufacturerID = caps.ManufacturerID;
			ProductID = caps.ProductID;
		}
		#endregion
		#endregion

		#region IGorgonJoystickInfo Members
		/// <inheritdoc/>
		public bool IsConnected
		{
			get
			{
				JOYCAPS caps = default(JOYCAPS);
				return Win32API.joyGetDevCaps(ID, ref caps, _capsSize) != 0xA7;
			}
		}

		/// <inheritdoc/>
		public int ButtonCount
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public int ManufacturerID
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public int ProductID
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public IGorgonJoystickAxisRangeList AxisRanges
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public IReadOnlyList<GorgonRange> VibrationMotorRanges
		{
			get;
		}

		/// <inheritdoc/>
		public JoystickCapabilityFlags Capabilities
		{
			get;
			private set;
		}
		#endregion

		#region IGorgonInputDeviceInfo Members
		/// <inheritdoc/>
		public Guid UUID
		{
			get;
		}

		/// <inheritdoc/>
		public string HumanInterfaceDevicePath => "GamingDevice";

		/// <inheritdoc/>
		public string ClassName => "Gaming device";

		/// <inheritdoc/>
		public InputDeviceType InputDeviceType => InputDeviceType.Joystick;

		#endregion

		#region IGorgonNamedObject Members
		/// <inheritdoc/>
		public string Name
		{
			get;
		}
		#endregion
	}
}
