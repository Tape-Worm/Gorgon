// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: January 8, 2025 9:37:51 PM
//

using Gorgon.Input.Devices;

namespace Gorgon.Input;

/// <summary>
/// The flags to indicate how to register input devices.
/// </summary>
[Flags]
public enum InputFlags
{
    /// <summary>
    /// No devices will be registered.
    /// </summary>
    None = 0,
    /// <summary>
    /// Register keyboard devices.
    /// </summary>
    Keyboard = 1,
    /// <summary>
    /// Register mouse devices.
    /// </summary>
    Mouse = 2,
    /// <summary>
    /// Register gaming devices.
    /// </summary>
    GamingDevices = 4,
    /// <summary>
    /// Mouse functionality will be exclusive to the application.  When this value is used, no window messages will be generated for mouse input. This implies <see cref="Mouse"/>.
    /// </summary>
    ExclusiveMouse = Mouse | 256,
    /// <summary>
    /// Keyboard functionality will be exclusive to the application.  When this value is used, no window messages will be generated for keyboard input. This implies <see cref="Keyboard"/>.
    /// </summary>
    ExclusiveKeyboard = Keyboard | 512,
    /// <summary>
    /// Only register keyboard and mouse devices.
    /// </summary>
    KeyboardAndMouse = Keyboard | Mouse,
    /// <summary>
    /// Only register keyboard and mouse devices.
    /// </summary>
    ExclusiveKeyboardAndMouse = ExclusiveKeyboard | ExclusiveMouse,
    /// <summary>
    /// Register all devices.
    /// </summary>
    AllDevices = Keyboard | Mouse | GamingDevices,
    /// <summary>
    /// Register all devices as exclusive (keyboard and mouse only).
    /// </summary>
    AllExclusiveDevices = ExclusiveKeyboard | ExclusiveMouse | GamingDevices
}

/// <summary>
/// Functionality to read input devices such as keyboards, mice and gaming devices (joysticks, gamepads, etc...).
/// </summary>
/// <remarks>
/// <para>
/// The Gorgon input system reads inputs from multiple sources (e.g. mice, keyboards, gamepads, etc...) and provides a means to provide a complete input stream for parsing by the various device types. 
/// Because all the data is aggregated, and each input event is timestamped, the input system can provide a complete picture of the input state at any given time without concern for missing any input 
/// events. 
/// </para>
/// <para>
/// This system is meant for polling, which means users will poll the input system within the application idle loop to retrieve the current input state, and then parse the input data via one of the input 
/// device objects to get the current state, even if the input device hasn't read any readings within some period of time. 
/// </para>
/// <para>
/// The interface also provides functionality to detect when a device has been attached or detached and will let users provide a callback for each event. With this, developers can easily know when a device, 
/// for example a wireless XBOX 360 controller, has been detached and let the user of their application know that the device is disconnected.
/// </para>
/// <para>
/// Finally, the system supports detection of input events from the system using a callback method, which allows developers to monitor exactly when events come in from the operating system, and handle them 
/// accordingly. This can be used in conjection with, or instead of polling.
/// </para>
/// </remarks>
public interface IGorgonInput
    : IDisposable
{
    /// <summary>
    /// Property to return the devices that are being monitored by the input system.
    /// </summary>
    /// <remarks>
    /// Use the <see cref="Enable"/> or <see cref="Disable"/> methods to disable or enable devices monitored by the input system.
    /// </remarks>
    InputFlags Devices
    {
        get;
    }

    /// <summary>
    /// Property to return the list of gaming devices (e.g. joysticks and gamepads) attached to the system.
    /// </summary>
    /// <remarks>
    /// The items in this list can be used to create an <see cref="IGorgonGamingDeviceInfo"/> object tied to a specific gaming device.
    /// </remarks>
    IReadOnlyList<IGorgonGamingDeviceInfo> GamingDevices
    {
        get;
    }

    /// <summary>
    /// Property to return the list of keyboards attached to the system.
    /// </summary>
    /// <remarks>
    /// The items in this list can be used to create an <see cref="IGorgonKeyboard"/> object optionally tied to a specific keyboard device.
    /// </remarks>
    IReadOnlyList<IGorgonKeyboardInfo> Keyboards
    {
        get;
    }

    /// <summary>
    /// Property to return the list of mice attached to the system.
    /// </summary>
    /// <remarks>
    /// The items in this list can be used to create an <see cref="IGorgonMouse"/> object optionally tied to a specific mouse device.
    /// </remarks>
    IReadOnlyList<IGorgonMouseInfo> Mice
    {
        get;
    }

    /// <summary>
    /// Function to disable the input system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applications should call this method to release the hold on exclusive input devices, so that regular window messages can be received and processed by the application. Or, if the user wishes to 
    /// change which devices to monitor.
    /// </para>
    /// <para>
    /// When the input system is disabled, no input data is received until it is reenabled. Also, if a mouse, keyboard, or gaming device object is bound to a particular item from the <see cref="Keyboards"/>, 
    /// <see cref="Mice"/>, or <see cref="GamingDevices"/> lists, then those objects will be invalid after this method is called and will have to be recreated after the <see cref="Enable"/> method is called.
    /// </para>
    /// <para>
    /// To enable the input system again, use the <see cref="Enable"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="Enable"/>
    /// <seealso cref="Devices"/>
    /// <seealso cref="Keyboards"/>
    /// <seealso cref="Mice"/>
    /// <seealso cref="GamingDevices"/>
    void Disable();

    /// <summary>
    /// Function to enable the input system.
    /// </summary>
    /// <param name="flags">The flags to indicate which devices to register and how they should be registered with the system.</param>
    /// <remarks>
    /// <para>
    /// Applications should call this method to enable the input system after it has been disabled. This will allow the application to receive raw input messages from the system again, or can be used to 
    /// change which devices are monitored via the <paramref name="flags"/> parameter (a call to <see cref="Disable"/> must be made prior to changing the device bindings).
    /// </para>
    /// <para>
    /// The <paramref name="flags"/> can be set to indicate which devices are read by the input system, and whether or not these devices are exclusive (keyboard/mouse only). Passing 
    /// <see cref="InputFlags.None"/> to this method effectively tells the system to ignore any input data.
    /// </para>
    /// <para>
    /// When a keyboard or mouse is set as exclusive, then the application will no longer respond to windows messages from these devices. This will cause the mouse to appear to freeze (the cursor at least), 
    /// and the keyboard will stop responding to the various system keypresses for a window (e.g. Alt+F4 will be disabled). Regardless of whether these devices are exclusive or not, the application will 
    /// no longer receive messages from them when the application is not in focus. Note that joysticks and gamepads cannot be set as exclusive and messages from these devices will always be received 
    /// regardless of whether the application is in the foreground or not.
    /// </para>
    /// <para>
    /// Because a call to <see cref="Disable"/> will invalidate any <see cref="IGorgonKeyboard"/>, <see cref="IGorgonMouse"/>, or <see cref="IGorgonGamingDevice"/> object created using an object from the 
    /// <see cref="Keyboards"/>, <see cref="Mice"/>, or <see cref="GamingDevices"/> lists, those objects will have to be recreated after this method is called.  This does not apply to a 
    /// <see cref="IGorgonKeyboard"/>, or <see cref="IGorgonMouse"/> object that is not tied to a specific device (i.e. created with a parameterless constructor).
    /// </para>
    /// <para>
    /// To disable the input system again, use the <see cref="Disable"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="Disable"/>
    /// <seealso cref="Devices"/>
    /// <seealso cref="Keyboards"/>
    /// <seealso cref="Mice"/>
    /// <seealso cref="GamingDevices"/>
    /// <seealso cref="IGorgonKeyboard"/>
    /// <seealso cref="IGorgonMouse"/>
    /// <seealso cref="IGorgonGamingDevice"/>
    void Enable(InputFlags flags);

    /// <summary>
    /// Function to retrieve the current input event readings.
    /// </summary>
    /// <param name="deviceType">The type of device(s) to retrieve readings for.</param>
    /// <param name="events">A buffer to hold a series of <see cref="GorgonInputEvent"/> readings for the specified device(s).</param>
    /// <returns>The number of events read.</returns>
    /// <remarks>
    /// <para>
    /// This will retrieve a series of input events from the input system, filtered by the <paramref name="deviceType"/> value. These events will be copied into the <paramref name="events"/> buffer. The 
    /// buffer can then be passed to an input device, such as <see cref="GorgonMouse"/> for parsing. The buffer is used to ensure that no events are missed during the polling process.
    /// </para>
    /// <para>
    /// The <paramref name="events"/> buffer will contain a snapshot of all input events that have occurred since the last time this method was called. If the buffer is not large enough to hold all the 
    /// events, then the buffer will only hold the last <c>n</c> events, where <c>n</c> is the size of the buffer. The last item in the list will be the last input event that occurred, and the buffer should 
    /// have at least 1 element. If the buffer is empty, then this method will return 0. The user may choose create the buffer with any size they wish, the method will fill it up to the size of the buffer, 
    /// or until there are no more events to read. For best results, and accurate readings, the buffer should be as large as can be afforded.
    /// </para>
    /// <para>
    /// The <paramref name="deviceType"/> parameter can filter for multiple types of devices by OR'ing the desired device types together. For example, to retrieve events for both mice and keyboards, the 
    /// <paramref name="deviceType"/> parameter would be set to <c>GorgonInputDeviceType.Mouse | GorgonInputDeviceType.Keyboard</c>.
    /// </para>
    /// <para>
    /// When this method is called, it will empty the current readings from the input system. This means that the <paramref name="events"/> buffer will be filled with the current readings, and subsequent 
    /// reads will be be empty until new input events are received from the computer.
    /// </para>
    /// <para>
    /// This method is thread safe, but it is recommended that the user call this method from a single thread to avoid potential data loss from other threads emptying the internal readings buffer.
    /// </para>
    /// <para>
    /// If an application wishes to respond to input events as they arrive in the system instead of polling, then use the <see cref="RegisterInputEventCallback"/> method.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// If any events are older than 1/2 a second since the last time this method was called, then they will be discarded. This is to ensure that the buffer does not grow too large over time.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <example>
    /// This example shows how to poll for input data, and parse it into usable device information.
    /// <code language="csharp">
    /// <![CDATA[
    /// // Where our current input readings are stored.
    /// private GorgonInputEventBuffer _buffer = new();
    /// 
    /// // The devices.
    /// private IGorgonMouse _mouse = new GorgonMouse();
    /// private IGorgonKeyboard _keyboard = new GorgonKeyboard();
    /// 
    /// // The input system.
    /// private IGorgonInput _input;
    /// 
    /// private bool Idle()
    /// {
    ///    // Get our data into the event buffer.
    ///    _input.GetInput(InputDeviceType.Mouse | InputDeviceType.Keyboard , _events);
    ///    
    ///    // Let the devices read the data.
    ///    _mouse.ParseData(_buffer);
    ///    _keyboard.ParseData(_buffer);
    ///    
    ///    // TODO: Do something with the _mouse and _keyboard data.
    /// }
    /// 
    /// private void Dispose(bool disposing)
    /// {
    ///    if (disposing)
    ///    {
    ///       _input?.Dispose();
    ///    }
    /// }
    /// 
    /// public void Dispose()
    /// {
    ///    Dispose(true);
    ///    GC.SuppressFinalize(this);
    /// }
    /// 
    /// Constructor()
    /// {
    ///    // Create an input system that will read the mouse and keyboard non-exclusively.
    ///    _input = new GorgonInput.CreateInput(InputFlags.Mouse | InputFlags.Keyboard, GorgonApplication.Log);
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="GorgonMouse"/>
    /// <seealso cref="GorgonKeyboard"/>
    /// <seealso cref="GorgonInputEventBuffer"/>
    /// <seealso cref="RegisterInputEventCallback"/>
    int GetInput(InputDeviceType deviceType, GorgonInputEventBuffer events);

    /// <summary>
    /// Function to register a callbacks that are executed when a device is added or removed from the system.
    /// </summary>
    /// <param name="deviceAttached">The method to call when a device is attached, or <b>NULL</b> to remove the callback.</param>
    /// <param name="deviceDetached">The method to call when a device is detached, or <b>NULL</b> to remove the callback.</param>
    /// <remarks>
    /// <para>
    /// To disable a callback, pass <b>null</b> to either the <paramref name="deviceAttached"/>, and/or the <paramref name="deviceDetached"/> parameters.
    /// </para>
    /// <para>
    /// <note type="Important">
    /// <para>
    /// The callback methods are called on the input background thread, UI functionality must be synchronized before updating any UI elements.
    /// </para>
    /// <para>
    /// For best performance, do not run any long running operations in the callback methods. 
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    void RegisterDeviceChangeCallbacks(Action<IGorgonInputDeviceInfo>? deviceAttached, Action<IGorgonInputDeviceInfo>? deviceDetached);

    /// <summary>
    /// Function to register a callback that is executed when an input event arrives.
    /// </summary>
    /// <param name="inputEvent">The method to call when an input arrives.</param>
    /// <remarks>
    /// <para>
    /// Applications can use this to set up an event driven system to handle the input events rather than polling using <see cref="GetInput(InputDeviceType, GorgonInputEventBuffer)"/>. For example, an 
    /// application may want to handle keyboard events, but poll for gaming device data. To do this, the user can pass a method that would do something like this:
    /// <code language="csharp">
    /// <![CDATA[
    /// bool KeyboardEvent(GorgonInputEvent keyEvent)
    /// {
    ///    // Only process keyboard events.
    ///    if (keyEvent.DeviceType != InputDeviceType.Keyboard)
    ///    {
    ///       return false;
    ///    }
    ///    
    ///    // Do keyboard event processing: OnKeyDown, OnKeyUp, etc...
    ///    
    ///    // Return true so that the system knows that we've already used this event.
    ///    // By returning true, we make it so that the event does not get returned 
    ///    // to the buffer in _input.GetInput.
    ///    return true;
    /// }
    /// 
    /// bool Idle()
    /// {
    ///    _input.GetInput(eventBuffer);
    ///    
    ///    _gamingDevice.ParseData(eventBuffer);
    /// }
    /// 
    /// void Setup()
    /// {
    ///    _input.RegisterInputEventCallback(KeyboardEvent);
    /// }
    /// ]]>
    /// </code>
    /// </para>
    /// <para>
    /// <note type="important">
    /// Be aware that any events processed using the callback (by returning <b>true</b> from the callback) will be consumed and will not be available for polling.
    /// </note>
    /// </para>
    /// <para>
    /// To disable a callback, pass <b>null</b> to the <paramref name="inputEvent"/> parameter.
    /// </para>
    /// <para>
    /// The <paramref name="inputEvent"/> method recieves a <see cref="GorgonInputEvent"/> as its parameter, and should return <b>true</b> if the event is processed by the application, or <b>false</b> if not.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// The callback method is called on the input background thread, UI functionality must be synchronized before updating any UI elements.
    /// </para>
    /// <para>
    /// For best performance, do not run any long running operations in the callback method. 
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonInputEvent"/>
    /// <seealso cref="GetInput"/>
    void RegisterInputEventCallback(Func<GorgonInputEvent, bool> inputEvent);
}