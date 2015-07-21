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
// Created: Friday, June 24, 2011 10:03:50 AM
// 
#endregion

using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Input.Events;
using Gorgon.Input.Properties;
using Gorgon.UI;

namespace Gorgon.Input
{
	/// <summary>
	/// Abstract class for input devices.
	/// </summary>
	public abstract class GorgonInputDevice
		: GorgonNamedObject
	{
		#region Events.
		/// <summary>
		/// Event fired before the device is unbound from its control.
		/// </summary>
		public event EventHandler<GorgonBeforeInputUnbindEventArgs> BeforeUnbind;
		#endregion

		#region Variables.
		// Flag to indicate whether the device is enabled.
		private bool _enabled;
		// Flag to indicate whether the device is in an acquired state.
		private bool _acquired;
		// Flag to indicate whether the device has exclusive access to the device.
		private bool _exclusive;
		// Flag to indicate whether the device will poll in the background.
		private bool _background;
		// Type of exclusivity for exclusive devices.
		private InputDeviceExclusivity _exclusivity;
		// The type of input device
		private InputDeviceType _deviceType;
		private Form _boundTopLevelForm;
		private Control _boundControl;

		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the input interface owner for this device.
		/// </summary>
		public GorgonInputService InputService
		{
			get;
		}

		/// <summary>
		/// Property to return the information about this device.
		/// </summary>
		public InputDeviceType DeviceType
		{
			get
			{
				return _deviceType;
			}
			internal set
			{
				_deviceType = value;

				switch (_deviceType)
				{
					case InputDeviceType.Keyboard:
						_exclusivity = InputDeviceExclusivity.Keyboard;
						break;
					case InputDeviceType.Mouse:
						_exclusivity = InputDeviceExclusivity.Mouse;
						break;
					case InputDeviceType.HumanInterfaceDevice:
						_exclusivity = InputDeviceExclusivity.HumanInterfaceDevice;
						break;
					case InputDeviceType.Joystick:
						_exclusivity = InputDeviceExclusivity.Joystick;
						break;
				}
			}
		}

		/// <summary>
		/// Property to return the window that the device is bound with.
		/// </summary>
		public Control BoundControl
		{
			get
			{
				return _boundControl;
			}
			protected set
			{
				_boundControl = value;
			}
		}

		/// <summary>
		/// Property to return the top level form that the device is bound with.
		/// </summary>
		public Form BoundTopLevelForm
		{
			get
			{
				return _boundTopLevelForm;
			}
			protected set
			{
				_boundTopLevelForm = value;
			}
		}

		/// <summary>
		/// Property to return whether the device is acquired or not.
		/// </summary>
		public bool Acquired
		{
			get
			{
				return _acquired;
			}
			set
			{
				if ((_acquired == value)
					|| (!_enabled))
				{
					return;
				}

				_acquired = value;

				if (value)
				{
					BindDevice();
				}
				else
				{
					UnbindDevice();
				}

                OnAcquisitionChanged();
			}
		}

		/// <summary>
		/// Property to set or return whether the window has exclusive access or not.
		/// </summary>
		public virtual bool Exclusive
		{
			get
			{
			    return _exclusive;
			}
		    set
			{
				if (_exclusive == value)
				{
					return;
				}

				_exclusive = value;

				// Make all devices of the same type exclusive or not exclusive.
				foreach (var device in InputService.Devices.Where(device => device.GetType() == GetType()))
				{
					device.Value._exclusive = value;
				}
			
				// Flag the device as exclusive in the factory.
				// We do this so that plug-in implementors can use the information.
				if (value)
				{
					InputService.ExclusiveDevices |= _exclusivity;
				}
				else
				{
					InputService.ExclusiveDevices &= ~_exclusivity;
				}
				
				if ((_enabled) && (_acquired))
				{
					BindDevice();
				}

                OnExclusiveChanged();
			}
		}

		/// <summary>
		/// Property to set or return whether to allow this device to keep sending data even if the window is not focused.
		/// </summary>
		public bool AllowBackground
		{
			get
			{
				return _background;
			}
			set
			{
				if (_background == value)
				{
					return;
				}

				_background = value;
				if ((_enabled) && (_acquired))
				{
					BindDevice();
				}
			}
		}

		/// <summary>
		/// Property to set or return whether the device is enabled or not.
		/// </summary>
		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if (value == _enabled)
				{
					return;
				}

				_enabled = value;
				_acquired = value;

				if (value)
				{
					BindDevice();
				}
				else
				{
					UnbindDevice();
				}

                OnEnabledChanged();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve a unique string representation of a device UUID.
		/// </summary>
		/// <param name="deviceInfo">Device information.</param>
		/// <returns>The unique name of the device.</returns>
		internal static string GetDeviceUUIDName<T>(IGorgonInputDeviceInfo deviceInfo)
			where T : GorgonInputDevice
		{
			Type deviceType = typeof(T);

			return GetDeviceUUIDName(deviceInfo, deviceType);
		}

		/// <summary>
		/// Function to retrieve a unique string representation of a device UUID.
		/// </summary>
		/// <param name="deviceInfo">Device information.</param>
		/// <param name="deviceType">The type of device.</param>
		/// <returns>The unique name of the device.</returns>
		internal static string GetDeviceUUIDName(IGorgonInputDeviceInfo deviceInfo, Type deviceType)
		{
			string result = Guid.Empty.ToString();

			if (deviceInfo != null)
			{
				result = deviceInfo.UUID.ToString();
			}

			result += "_" + deviceType.FullName;

			return result;
		}

		/// <summary>
		/// Handles the Activated event of the BoundForm.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void BoundForm_Activated(object sender, EventArgs e)
		{
			OnBoundFormActivated();
		}

		/// <summary>
		/// Handles the Deactivate event of the BoundForm.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void BoundForm_Deactivate(object sender, EventArgs e)
		{
			OnBoundFormDeactivated();
		}

		/// <summary>
		/// Handles the LostFocus event of the BoundWindow.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void BoundWindow_LostFocus(object sender, EventArgs e)
		{
			OnBoundWindowUnfocused();
		}

		/// <summary>
		/// Handles the GotFocus event of the BoundWindow.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void BoundWindow_GotFocus(object sender, EventArgs e)
		{
			OnBoundWindowFocused();
		}

        /// <summary>
        /// Function called when the <see cref="GorgonInputDevice.Acquired"/> property changes its value.
        /// </summary>
	    protected virtual void OnAcquisitionChanged()
	    {
	        
	    }

        /// <summary>
        /// Function called when the <see cref="GorgonInputDevice.Enabled"/> property changes its value.
        /// </summary>
	    protected virtual void OnEnabledChanged()
	    {
	        
	    }

        /// <summary>
        /// Function called when the <see cref="GorgonInputDevice.Exclusive"/> property changes its value.
        /// </summary>
	    protected virtual void OnExclusiveChanged()
	    {
	        
	    }

		/// <summary>
		/// Function called if the bound window gets focus.
		/// </summary>
		protected virtual void OnBoundWindowFocused()
		{
			if ((InputService.AutoReacquireDevices) && (Exclusive))
			{
				Acquired = true;
			}
		}

		/// <summary>
		/// Function called if the bound window loses focus.
		/// </summary>
		protected virtual void OnBoundWindowUnfocused()
		{
			if (Exclusive)
			{
				Acquired = false;
			}
		}

		/// <summary>
		/// Function called when the bound form is activated.
		/// </summary>
		protected virtual void OnBoundFormActivated()
		{
			OnBoundWindowFocused();
		}

		/// <summary>
		/// Function called when the bound form is deactivated.
		/// </summary>
		protected virtual void OnBoundFormDeactivated()
		{
			OnBoundWindowUnfocused();
		}

		/// <summary>
		/// Function to bind the input device.
		/// </summary>
		protected abstract void BindDevice();

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected abstract void UnbindDevice();

		/// <summary>
		/// Function to called before the input device is unbound from its control.
		/// </summary>
		protected virtual void OnBeforeUnbind()
		{
			// Ensure that only 1 thread will call this event. 
			// Having multiple threads in here could be disastrous, so this will help mitigate that.
			var beforeUnbind = Interlocked.Exchange(ref BeforeUnbind, null);

			beforeUnbind?.Invoke(this, new GorgonBeforeInputUnbindEventArgs(_boundControl));
		}

		/// <summary>
		/// Function to unbind the device from a window.
		/// </summary>
		private void UnbindWindow()
		{
			Form boundForm = Interlocked.Exchange(ref _boundTopLevelForm, null);

		    if ((boundForm != null) && (!boundForm.IsDisposed) && (!boundForm.Disposing))
			{
				BoundTopLevelForm.Activated -= BoundForm_Activated;
				BoundTopLevelForm.Deactivate -= BoundForm_Deactivate;
			}

			Control boundControl = Interlocked.Exchange(ref _boundControl, null);

		    if ((boundControl == null) || (boundControl.IsDisposed) || (boundControl.Disposing))
		    {
		        return;
		    }

		    boundControl.LostFocus -= BoundWindow_LostFocus;
			boundControl.GotFocus -= BoundWindow_GotFocus;

            GorgonApplication.Log.Print("Unbinding input device object {1} from window 0x{0}.", LoggingLevel.Intermediate,
                             BoundControl.Handle.FormatHex(), GetType().Name);
        }

		/// <summary>
		/// Function to unbind the input device from its window control.
		/// </summary>
		public void Unbind()
		{
			if (_boundControl == null)
			{
				return;
			}

			if (_enabled)
			{
				UnbindDevice();
			}

			_acquired = false;
			_enabled = false;

			OnBeforeUnbind();

			UnbindWindow();
		}

		/// <summary>
		/// Function to bind the device to a window.
		/// </summary>
		/// <param name="boundWindow">Window to bind with.</param>
		/// <remarks>Passing NULL (<i>Nothing</i> in VB.Net) to <paramref name="boundWindow"/> will use the <see cref="GorgonApplication.MainForm">main Gorgon Application Form</see>.  If there is no Form bound with the application, then an exception will be thrown.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the boundWindow parameter and the Gorgon.ApplicationForm property are both NULL (<i>Nothing</i> in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the top level form cannot be determined.</para>
		/// </exception>
		public void Bind(Control boundWindow)
		{
			if (BoundControl != null)
			{
				Unbind();
			}

			if (boundWindow == null)
			{
			    if (GorgonApplication.MainForm == null)
			    {
			        throw new ArgumentException(Resources.GORINP_ERR_NO_WINDOW_TO_BIND, nameof(boundWindow));
			    }

			    boundWindow = GorgonApplication.MainForm;
			}

			GorgonApplication.Log.Print("Binding input device object {1} to window 0x{0}.", LoggingLevel.Intermediate, boundWindow.Handle.FormatHex(), GetType().Name);

			BoundControl = boundWindow;
			BoundTopLevelForm = BoundControl.FindForm();

		    if (BoundTopLevelForm == null)
		    {
		        throw new ArgumentException(Resources.GORINP_ERR_NO_WINDOW_TO_BIND, nameof(boundWindow));
		    }

		    BoundTopLevelForm.Activated += BoundForm_Activated;
			BoundTopLevelForm.Deactivate += BoundForm_Deactivate;
			BoundControl.LostFocus += BoundWindow_LostFocus;
			BoundControl.GotFocus += BoundWindow_GotFocus;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputDevice"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <param name="deviceInfo">Information about the device.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="owner"/> parameter is <b>null</b> (<i>Nothing</i> in VB.NET).</exception>
		protected GorgonInputDevice(GorgonInputService owner, IGorgonInputDeviceInfo deviceInfo)
			: base(deviceInfo.Name)
		{
			if (owner == null)
			{
				throw new ArgumentNullException(nameof(owner));
			}

			InputService = owner;
		}
		#endregion
	}
}
