using System.Collections.Generic;
using Gorgon.Collections;

namespace Gorgon.Input
{
	/// <summary>
	/// Provides access to various input devices attached to the system.
	/// </summary>
	/// <remarks>
	/// <para>
	/// An input service provides access to the various input devices (such as a keyboard, mouse, ,joystick, game pad, etc...) attached to the system. It coordinates data from the native input device data 
	/// sources (e.g. Raw input) and forwards the data to the appropriate devices after it's been converted to a form that Gorgon can interpret.
	/// </para>
	/// <para>
	/// Input services can pick and choose which device types they support. This means that one input service may support XBox controllers through XInput, and another will use the Windows Multimedia interface 
	/// to use other joystick types. And yet another may provide mouse/keyboard support only. 
	/// </para>
	/// <para>
	/// Users may enumerate the input devices present on the system via provided enumeration methods. These methods will return <see cref="Collections.IReadOnlyList{T}"/> types that will contain 
	/// <see cref="IGorgonKeyboardInfo2"/>, <see cref="IGorgonMouseInfo2"/> or <see cref="IGorgonJoystickInfo2"/> types giving information about each device. These values may be passed to the constructors of the  
	/// various input object types to allow selection of a specific device. This includes mice and keyboards. However, not all input services will allow the use of individual mice and keyboards and may only 
	/// allow use of the system devices (i.e. all input is directed from multiple devices into a single interface). If this is the case, the enumeration method will return only one item in its list.
	/// </para>
	/// <para>
	/// <note type="important">
	/// <para>
	/// The input service, and any plug ins deriving from it, are not guaranteed to be thread safe by design. Accessing a single <see cref="GorgonInputService2"/> instance via multiple threads is discouraged 
	/// and not supported.
	/// </para>
	/// </note>
	/// </para>
	/// <para>
	/// <note type="caution">
	/// <para>
	/// While multiple input services can co-exist, care should be taken not to instantiate the same type of input service more than once. Doing so may end up in undefined behaviour due to internal implementation 
	/// details that may conflict. For example, creating a Raw Input service and an XInput service would be fine, but two instances of the Raw Input service would cause problems due to how Raw Input works.
	/// </para> 
	/// </note>
	/// </para>
	/// </remarks>
	public interface IGorgonInputService
	{
		/// <summary>
		/// Property to return the <see cref="IGorgonInputDeviceRegistrar"/> used to register or unregister any input devices.
		/// </summary>
		/// <remarks>
		/// <para>
		/// A registrar is used to keep track of which devices are currently bound to a window handle. This allows for routing of device data to the appropriate instance of the input device. 
		/// </para>
		/// <para>
		/// Plug in implementors are required to provide their own device registration tracking.
		/// </para>
		/// </remarks>
		IGorgonInputDeviceRegistrar Registrar
		{
			get;
		}

		/// <summary>
		/// Property to return the coordinator used to send/receive data to the appropriate <see cref="IGorgonInputDevice"/>.
		/// </summary>
		IGorgonInputDeviceCoordinator Coordinator
		{
			get;
		}

		/// <summary>
		/// Function to enumerate the keyboards attached to the computer.
		/// </summary>
		/// <returns>A <see cref="IReadOnlyList{T}"/> type containing <see cref="IGorgonKeyboardInfo2"/> values with information about each keyboard.</returns>
		/// <remarks>
		/// <para>
		/// This will return information about each keyboard attached to the system. The <see cref="IGorgonKeyboardInfo2"/> values contained within the returned list are used to create 
		/// <see cref="GorgonKeyboard2"/> devices.
		/// </para>
		/// <para>
		/// Not all input services will allow the use of the individual keyboard devices attached to the computer. Some will only allow the use of the system keyboard (i.e. all input from all keyboards 
		/// is directed into a single device object). If this is the case, this method will return a list with only one item.
		/// </para>
		/// </remarks>
		IReadOnlyList<IGorgonKeyboardInfo2> EnumerateKeyboards();

		/// <summary>
		/// Function to enumerate the mice (or other pointing devices) attached to the computer.
		/// </summary>
		/// <returns>A <see cref="IReadOnlyList{T}"/> type containing <see cref="IGorgonMouseInfo2"/> values with information about each mouse.</returns>
		/// <remarks>
		/// <para>
		/// This will return information about each mouse attached to the system. The <see cref="IGorgonMouseInfo2"/> values contained within the returned list are used to create <see cref="GorgonMouse"/> 
		/// devices.
		/// </para>
		/// <para>
		/// Not all input services will allow the use of the individual mice attached to the computer. Some will only allow the use of the system mouse (i.e. all input from all mice is directed into a 
		/// single device object). If this is the case, this method will return a list with only one item.
		/// </para>
		/// </remarks>
		IReadOnlyList<IGorgonMouseInfo2> EnumerateMice();

		/// <summary>
		/// Function to enumerate the joysticks (or other gaming devices such as game pads) attached to the computer.
		/// </summary>
		/// <returns>A <see cref="IReadOnlyList{T}"/> type containing <see cref="IGorgonJoystickInfo2"/> values with information about each joystick.</returns>
		/// <remarks>
		/// <para>
		/// This will return information about each joystick attached to the system. The <see cref="IGorgonJoystickInfo2"/> values contained within the returned list are used to create <see cref="GorgonJoystick2"/> 
		/// devices.
		/// </para>
		/// </remarks>
		IReadOnlyList<IGorgonJoystickInfo2> EnumerateJoysticks();
	}
}