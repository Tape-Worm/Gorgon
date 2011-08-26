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
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Input
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
		private bool _disposed = false;			// Flag to indicate whether the object is disposed or not.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the input interface owner for this device.
		/// </summary>
		protected GorgonInputDeviceFactory DeviceFactory
		{
			get;
			private set;
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
		public virtual bool Acquired
		{
			get
			{
				return _acquired;
			}
			set
			{
				_acquired = value;

				if (_enabled)
				{
					if (value)
						BindDevice();
					else
						UnbindDevice();
				}
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
				_exclusive = value;
				if (_enabled)
					BindDevice();
			}
		}

		/// <summary>
		/// Property to set or return whether to allow this device to keep sending data even if the window is not focused.
		/// </summary>
		public virtual bool AllowBackground
		{
			get
			{
				return _background;
			}
			set
			{
				_background = value;
				if (_enabled)
					BindDevice();
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
				Acquired = value;

				if (value)
					BindDevice();
				else
					UnbindDevice();

				_enabled = value;
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
		/// Function called if the bound window gets focus.
		/// </summary>
		protected virtual void OnBoundWindowFocused()
		{
			if ((DeviceFactory.AutoReacquireDevices) && (Exclusive))
				Acquired = true;
		}

		/// <summary>
		/// Function called if the bound window loses focus.
		/// </summary>
		protected virtual void OnBoundWindowUnfocused()
		{
			if (Exclusive)
				Acquired = false;
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
			if ((!BoundControl.IsDisposed) && (!BoundControl.Disposing))
				Gorgon.Log.Print("Unbinding input device object {1} from window 0x{0}.", GorgonLoggingLevel.Intermediate, GorgonHexFormatter.Format(BoundControl.Handle), GetType().Name);
			else
				Gorgon.Log.Print("Owner window was disposed.", GorgonLoggingLevel.Intermediate);

			if ((BoundTopLevelForm != null) && (!BoundTopLevelForm.IsDisposed) && (!BoundTopLevelForm.Disposing))
			{
				BoundTopLevelForm.Activated -= new EventHandler(BoundForm_Activated);
				BoundTopLevelForm.Deactivate -= new EventHandler(BoundForm_Deactivate);
			}
			if ((BoundControl != null) && (!BoundControl.IsDisposed) && (!BoundControl.Disposing))
			{
				BoundControl.LostFocus -= new EventHandler(BoundWindow_LostFocus);
				BoundControl.GotFocus -= new EventHandler(BoundWindow_GotFocus);
			}
		}

		/// <summary>
		/// Function called when the device is bound to a window.
		/// </summary>
		/// <param name="window">Window that was bound.</param>
		/// <remarks>Implementors will override this function to handle specific events that may affect the input device.</remarks>
		protected virtual void OnWindowBound(Control window)
		{
		}

		/// <summary>
		/// Function to bind the device to a window.
		/// </summary>
		/// <param name="boundWindow">Window to bind with.</param>
		/// <remarks>Passing NULL (Nothing in VB.Net) to <paramref name="boundWindow"/> will use the <see cref="GorgonLibrary.Gorgon.ApplicationForm">main Gorgon Application Form</see>.  If there is no Form bound with the application, then an exception will be thrown.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the boundWindow parameter and the Gorgon.ApplicationForm property are both NULL (Nothing in VB.Net).
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
				if (Gorgon.ApplicationForm == null)
					throw new ArgumentException("There is no application window to bind with.", "boundWindow");

				boundWindow = Gorgon.ApplicationForm;
			}

			Gorgon.Log.Print("Binding input device object {1} to window 0x{0}.", GorgonLoggingLevel.Intermediate, GorgonHexFormatter.Format(boundWindow.Handle), GetType().Name);

			BoundControl = boundWindow;
			BoundTopLevelForm = Gorgon.GetTopLevelForm(BoundControl);

			if (BoundTopLevelForm == null)
				throw new ArgumentException("Cannot bind to the window, no parent form was found.", "boundWindow");

			BoundTopLevelForm.Activated += new EventHandler(BoundForm_Activated);
			BoundTopLevelForm.Deactivate += new EventHandler(BoundForm_Deactivate);
			BoundControl.LostFocus += new EventHandler(BoundWindow_LostFocus);
			BoundControl.GotFocus += new EventHandler(BoundWindow_GotFocus);

			OnWindowBound(boundWindow);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputDevice"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <param name="deviceName">Name of the input device.</param>
		/// <param name="boundWindow">The window to bind this device with.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</remarks>
		protected internal GorgonInputDevice(GorgonInputDeviceFactory owner, string deviceName, Control boundWindow)
			: base(deviceName)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");

			DeviceFactory = owner;
			Bind(boundWindow);
			UUID = Guid.Empty.ToString();
		}
		#endregion

		#region IDisposable Implementation.
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					UnbindWindow();

					if (_enabled)
						UnbindDevice();
					_acquired = false;
					_enabled = false;

					if (DeviceFactory.Devices.ContainsKey(UUID))
						DeviceFactory.Devices.Remove(UUID);
				}
				_disposed = true;
			}
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
