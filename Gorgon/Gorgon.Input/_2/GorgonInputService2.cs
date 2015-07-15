using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gorgon.Input
{
	/// <inheritdoc/>
	public abstract class GorgonInputService2
		: IGorgonInputService
	{
		#region Variables.

		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to acquire or unacquire a device.
		/// </summary>
		/// <param name="device">The device being acquire or unacquired.</param>
		/// <param name="acquisitionState"><b>true</b> if the device is being acquired, <b>false</b> if the device is being unacquired.</param>
		/// <remarks>
		/// Plug in implementors will use this method to perform any set up or tear down of functionality required when the device becomes acquired or unacquired respectively. This method will be called when the 
		/// <see cref="IGorgonInputDevice.IsAcquired"/> property is set.
		/// </remarks>
		protected abstract internal void AcquireDevice(IGorgonInputDevice device, bool acquisitionState);

		/// <summary>
		/// Function to register a device when it binds with a window.
		/// </summary>
		/// <param name="device">The device that is being bound to the window.</param>
		/// <param name="deviceInfo">Information about the device being bound to the window.</param>
		/// <remarks>
		/// Plug in implementors will use this method to ensure that required functionality is present when a device is bound to a window. This method will be called when the <see cref="IGorgonInputDevice.BindWindow"/> 
		/// method is called.
		/// </remarks>
		protected abstract internal void RegisterDevice(IGorgonInputDevice device, IGorgonInputDeviceInfo2 deviceInfo);

		/// <summary>
		/// Function to unregister a device when it unbinds from a window.
		/// </summary>
		/// <param name="device">The device that is being unbound from the window.</param>
		/// <param name="deviceInfo">Information about the device being unbound from the window.</param>
		/// <remarks>
		/// Plug in implementors will use this method to ensure that any clean up required for functionality is present when a device is unbound from a window. This method will be called when the 
		/// <see cref="IGorgonInputDevice.UnbindWindow"/>  method is called.
		/// </remarks>
		protected abstract internal void UnregisterDevice(IGorgonInputDevice device, IGorgonInputDeviceInfo2 deviceInfo);

		/// <summary>
		/// Function to perform enumeration of keyboard devices.
		/// </summary>
		/// <returns>A list of <see cref="IGorgonKeyboardInfo2"/> items containing information about each keyboard.</returns>
		/// <remarks>
		/// <para>
		/// This is intended for use by input plug in developers. Developers may enumerate their devices in whichever mechanism is provided by the underlying input system and return a list of 
		/// <see cref="IGorgonKeyboardInfo2"/> items.
		/// </para>
		/// </remarks>
		protected abstract IReadOnlyList<IGorgonKeyboardInfo2> OnEnumerateKeyboards();

		/// <summary>
		/// Function to perform enumeration of mice or other pointing devices.
		/// </summary>
		/// <returns>A list of <see cref="IGorgonMouseInfo2"/> items containing information about each mouse.</returns>
		/// <remarks>
		/// <para>
		/// This is intended for use by input plug in developers. Developers may enumerate their devices in whichever mechanism is provided by the underlying input system and return a list of 
		/// <see cref="IGorgonMouseInfo2"/> items.
		/// </para>
		/// </remarks>
		protected abstract IReadOnlyList<IGorgonMouseInfo2> OnEnumerateMice();

		/// <summary>
		/// Function to perform enumeration of joysticks or other gaming devices.
		/// </summary>
		/// <returns>A list of <see cref="IGorgonMouseInfo2"/> items containing information about each joystick.</returns>
		/// <remarks>
		/// <para>
		/// This is intended for use by input plug in developers. Developers may enumerate their devices in whichever mechanism is provided by the underlying input system and return a list of 
		/// <see cref="IGorgonJoystickInfo2"/> items.
		/// </para>
		/// </remarks>
		protected abstract IReadOnlyList<IGorgonJoystickInfo2> OnEnumerateJoysticks();
		#endregion

		#region Constructor/Finalizer.

		#endregion

		#region IGorgonInputService Members
		/// <inheritdoc/>
		public IReadOnlyList<IGorgonKeyboardInfo2> EnumerateKeyboards()
		{
			return OnEnumerateKeyboards();
		}

		/// <inheritdoc/>
		public IReadOnlyList<IGorgonMouseInfo2> EnumerateMice()
		{
			return OnEnumerateMice();
		}

		/// <inheritdoc/>
		public IReadOnlyList<IGorgonJoystickInfo2> EnumerateJoysticks()
		{
			return OnEnumerateJoysticks();
		}
		#endregion
	}
}
