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
using System.Diagnostics;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;

namespace Gorgon.Input
{
	/// <summary>
	/// Base class containing common functionality for all input devices.
	/// </summary>
	public abstract class GorgonInputDevice2
		: IGorgonInputDevice
	{
		#region Variables.
		// Information about the device.
		private readonly IGorgonInputDeviceInfo2 _info;
		// Flag to indicate that the device has been acquired.
		private bool _isAcquired;
		// The input device registrar.
		private IGorgonInputDeviceRegistrar _registrar;
		#endregion

		#region Events.
		/// <inheritdoc/>
		public event EventHandler InputDeviceAcquired;

		/// <inheritdoc/>
		public event EventHandler InputDeviceUnacquired;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the parent form of the bound window (if applicable).
		/// </summary>
		/// <remarks>
		/// If the input device is bound to a child control of a form, then this property will return the form that contains the child control. If the device is bound to the 
		/// top level window for that control, then this value will be the same as <see cref="Window"/>.
		/// </remarks>
		protected Form ParentForm
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the debug log file used with the device object.
		/// </summary>
		protected IGorgonLog Log
		{
			get;
		}

		/// <summary>
		/// Property to return the service that owns this device.
		/// </summary>
		public GorgonInputService2 Service
		{
			get;
		}

		/// <inheritdoc/>
		public Guid UUID
		{
			get;
		}
		
		/// <inheritdoc/>
		public Control Window
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public bool IsAcquired
		{
			get
			{
				return _isAcquired;
			}
			set
			{
				// If the state has not changed, then do nothing.
				if (value == _isAcquired)
				{
					return;
				}

				// If we're not bound yet, then do nothing.
				if ((Window == null) || (Form.ActiveForm != ParentForm) || ((ParentForm != Window) && (ParentForm.ActiveControl != Window)))
				{
					value = false;
				}

				_isAcquired = value;

				if (value)
				{
					InputDeviceAcquired?.Invoke(this, EventArgs.Empty);
				}
				else
				{
					InputDeviceUnacquired?.Invoke(this, EventArgs.Empty);
				}

				OnAcquiredStateChanged();
			}
		}

		/// <inheritdoc/>
		public bool IsExclusive
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public abstract bool IsPolled
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the EnabledChanged event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Window_EnabledChanged(object sender, EventArgs e)
		{
			if ((ParentForm != null) && (!ParentForm.Enabled))
			{
				IsAcquired = false;
				return;
			}

			if ((Window != null) && (!Window.Enabled))
			{
				IsAcquired = false;
			}
		}

		/// <summary>
		/// Handles the VisibleChanged event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Window_VisibleChanged(object sender, EventArgs eventArgs)
		{
			if ((ParentForm != null) && (!ParentForm.Visible))
			{
				IsAcquired = false;
				return;
			}

			if ((Window != null) && (!Window.Visible))
			{
				IsAcquired = false;
			}
		}

		/// <summary>
		/// Handles the Deactivate event of the ParentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ParentForm_Deactivate(object sender, EventArgs eventArgs)
		{
			IsAcquired = false;
		}

		/// <summary>
		/// Handles the FormClosing event of the ParentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="FormClosingEventArgs"/> instance containing the event data.</param>
		private void ParentForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				// Attempt to shut this device down if the parent window is closing.
				UnbindWindow();
			}
			catch (Exception ex)
			{
				// We'll eat this exception because the window is dying anyway, but dump it to the log just to be safe.
				Log.LogException(ex);
			}
		}

		/// <summary>
		/// Function called when the device acquisition has been changed.
		/// </summary>
		/// <remarks>
		/// Devices will overload this method to update the state of the device after its <see cref="IsAcquired"/> property has been changed.
		/// </remarks>
		protected virtual void OnAcquiredStateChanged()
		{
		}

		/// <inheritdoc/>
		public void BindWindow(Control window, bool exclusive = false)
		{
			if (window == null)
			{
				throw new ArgumentNullException(nameof(window));
			}

			// Just in case we get weird with binding.
			if ((window.IsDisposed) || (window.Disposing))
			{
				throw new ObjectDisposedException(window.GetType().FullName);
			}

			// If the window is previously bound, then release us from the other window before attempting to register it with 
			// the application.
			UnbindWindow();

			Log.Print("Binding input device object {1} to window 0x{0}.", LoggingLevel.Intermediate, window.Handle.FormatHex(), GetType().Name);

			Form parentForm = window.FindForm();

			Debug.Assert(parentForm != null, "No parent form for the window!");

			// Register the device with the service.
			IsExclusive = _registrar.RegisterDevice(this, _info, window, exclusive);

			// Get this before we register. That way, our plug ins will be able to use this value 
			// if it's required.
			ParentForm = parentForm;
			Window = window;

			// These events are used to mark the window as unacquired when the app loses focus.
			ParentForm.Deactivate += ParentForm_Deactivate;
			ParentForm.VisibleChanged += Window_VisibleChanged;
			ParentForm.EnabledChanged += Window_EnabledChanged;
			ParentForm.FormClosing += ParentForm_FormClosing;
			Window.LostFocus += ParentForm_Deactivate;
			Window.VisibleChanged += Window_VisibleChanged;
			Window.EnabledChanged += Window_EnabledChanged;
		}

		/// <inheritdoc/>
		public void UnbindWindow()
		{
			// If the window is not assigned, then we never bound the device in the first place.
			if (Window == null)
			{
				return;
			}

			try
			{
				IsAcquired = false;
				IsExclusive = false;

				// Parent form must be assigned by this point, if it wasn't something is really wrong.
				if ((!ParentForm.IsDisposed) && (!ParentForm.Disposing))
				{
					ParentForm.Deactivate -= ParentForm_Deactivate;
					ParentForm.VisibleChanged -= Window_VisibleChanged;
					ParentForm.EnabledChanged -= Window_EnabledChanged;
					ParentForm.FormClosing -= ParentForm_FormClosing;
				}

				if ((!Window.IsDisposed) && (!Window.Disposing))
				{
					Window.LostFocus -= ParentForm_Deactivate;
					Window.VisibleChanged -= Window_VisibleChanged;
					Window.EnabledChanged -= Window_EnabledChanged;

					Log.Print("Unbinding input device object {1} from window 0x{0}.",
							  LoggingLevel.Intermediate,
							  Window.Handle.FormatHex(),
							  GetType().Name);
				}

				// Let the service know that we're done.
				_registrar.UnregisterDevice(this);
			}
			finally
			{
				Window = null;
				ParentForm = null;
			}
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputDevice2"/> class.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="deviceInfo">The device information.</param>
		/// <param name="log">The debug log file to use with this device.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="service"/>, or the <paramref name="deviceInfo"/> parameters are <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		protected GorgonInputDevice2(GorgonInputService2 service, IGorgonInputDeviceInfo2 deviceInfo, IGorgonLog log)
		{
			if (service == null)
			{
				throw new ArgumentNullException(nameof(service));
			}

			if (deviceInfo == null)
			{
				throw new ArgumentNullException(nameof(deviceInfo));
			}

			_registrar = service.Registrar;
			Service = service;
			UUID = Guid.NewGuid();
			_info = deviceInfo;
			Log = log ?? new GorgonLogDummy();
		}
		#endregion
	}
}
