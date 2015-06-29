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
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Input.Properties;
using Gorgon.UI;

namespace Gorgon.Input
{
	/// <summary>
	/// Abstract class for input devices.
	/// </summary>
	public abstract class GorgonInputDevice
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		private bool _enabled;					// Flag to indicate whether the device is enabled.
		private bool _acquired;					// Flag to indicate whether the device is in an acquired state.
		private bool _exclusive;				// Flag to indicate whether the device has exclusive access to the device.
		private bool _background;				// Flag to indicate whether the device will poll in the background.
		private bool _disposed;			        // Flag to indicate whether the object is disposed or not.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the input interface owner for this device.
		/// </summary>
		protected internal GorgonInputService DeviceFactory
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to set or return the factory UUID for this device.
		/// </summary>
		internal string UUID
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the information about this device.
		/// </summary>
		public InputDeviceType DeviceType
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the window that the device is bound with.
		/// </summary>
		public Control BoundControl
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the top level form that the device is bound with.
		/// </summary>
		public Form BoundTopLevelForm
		{
			get;
			protected set;
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
				foreach (var device in DeviceFactory.Devices.Where(device => device.GetType() == GetType()))
				{
					device.Value._exclusive = value;
				}
			
				// Flag the device as exclusive in the factory.
				// We do this so that plug-in implementors can use the information.
				if (value)
				{
					DeviceFactory.ExclusiveDevices |= DeviceType;
				}
				else
				{
					DeviceFactory.ExclusiveDevices &= ~DeviceType;
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
			if ((DeviceFactory.AutoReacquireDevices) && (Exclusive))
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
		/// Function to unbind the device from a window.
		/// </summary>
		protected virtual void UnbindWindow()
		{
		    if ((BoundTopLevelForm != null) && (!BoundTopLevelForm.IsDisposed) && (!BoundTopLevelForm.Disposing))
			{
				BoundTopLevelForm.Activated -= BoundForm_Activated;
				BoundTopLevelForm.Deactivate -= BoundForm_Deactivate;
			}

		    if ((BoundControl == null) || (BoundControl.IsDisposed) || (BoundControl.Disposing))
		    {
		        return;
		    }

		    BoundControl.LostFocus -= BoundWindow_LostFocus;
		    BoundControl.GotFocus -= BoundWindow_GotFocus;

            GorgonApplication.Log.Print("Unbinding input device object {1} from window 0x{0}.", LoggingLevel.Intermediate,
                             BoundControl.Handle.FormatHex(), GetType().Name);
        }

		/// <summary>
		/// Function to bind the device to a window.
		/// </summary>
		/// <param name="boundWindow">Window to bind with.</param>
		/// <remarks>Passing NULL (<i>Nothing</i> in VB.Net) to <paramref name="boundWindow"/> will use the <see cref="GorgonApplication.ApplicationForm">main Gorgon Application Form</see>.  If there is no Form bound with the application, then an exception will be thrown.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the boundWindow parameter and the Gorgon.ApplicationForm property are both NULL (<i>Nothing</i> in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the top level form cannot be determined.</para>
		/// </exception>
		public void Bind(Control boundWindow)
		{
			if (BoundControl != null)
			{
				UnbindWindow();
				BoundControl = null;
				BoundTopLevelForm = null;
			}

			if (boundWindow == null)
			{
			    if (GorgonApplication.ApplicationForm == null)
			    {
			        throw new ArgumentException(Resources.GORINP_NO_WINDOW_TO_BIND, "boundWindow");
			    }

			    boundWindow = GorgonApplication.ApplicationForm;
			}

			GorgonApplication.Log.Print("Binding input device object {1} to window 0x{0}.", LoggingLevel.Intermediate, boundWindow.Handle.FormatHex(), GetType().Name);

			BoundControl = boundWindow;
			BoundTopLevelForm = GorgonApplication.GetTopLevelForm(BoundControl);

		    if (BoundTopLevelForm == null)
		    {
		        throw new ArgumentException(Resources.GORINP_NO_WINDOW_TO_BIND, "boundWindow");
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
		/// <param name="deviceName">Name of the input device.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="owner"/> parameter is NULL (or Nothing in VB.NET).</exception>
		protected internal GorgonInputDevice(GorgonInputService owner, string deviceName)
			: base(deviceName)
		{
		    if (owner == null)
		    {
		        throw new ArgumentNullException("owner");
		    }

		    DeviceFactory = owner;
			UUID = Guid.Empty.ToString();
		}
		#endregion

		#region IDisposable Implementation.
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				UnbindWindow();

				if (_enabled)
				{
					UnbindDevice();
				}

				_acquired = false;
				_enabled = false;

				if (DeviceFactory.Devices.ContainsKey(UUID))
				{
					DeviceFactory.Devices.Remove(UUID);
				}

				if ((_exclusive) && (DeviceFactory.Devices.All(item => item.Value != this && item.Value.GetType() == GetType())))
				{
					DeviceFactory.ExclusiveDevices &= ~DeviceType;
				}
			}

			_disposed = true;
		}

		/// <summary>
		/// Function to perform clean up on the object.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
