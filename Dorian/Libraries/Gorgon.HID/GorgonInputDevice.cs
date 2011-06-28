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

namespace GorgonLibrary.HID
{
	/// <summary>
	/// Abstract class for input devices.
	/// </summary>
	public abstract class GorgonInputDevice
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		private bool _bound;					// Flag to indicate whether the device was bound.
		private bool _acquired;					// Flag to indicate whether the device is in an acquired state.
		private bool _exclusive;				// Flag to indicate whether the device has exclusive access to the device.
		private bool _background;				// Flag to indicate whether the device will poll in the background.
		private bool _disposed = false;			// Flag to indicate whether the object is disposed or not.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the input interface owner for this device.
		/// </summary>
		protected GorgonInputDeviceFactory InputInterface
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the window that the device is bound with.
		/// </summary>
		public Control BoundWindow
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the top level form that the device is bound with.
		/// </summary>
		public Form BoundForm
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

				if (_bound)
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
				if (_bound)
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
				if (_bound)
					BindDevice();
			}
		}

		/// <summary>
		/// Property to return whether the device is bound or not.
		/// </summary>
		public bool Enabled
		{
			get
			{
				return _bound;
			}
			set
			{
				Acquired = value;

				if (value)
					BindDevice();
				else
					UnbindDevice();
				
				_bound = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Activated event of the _parentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void ParentForm_Activated(object sender, EventArgs e)
		{
			Owner_GotFocus(this, e);
		}

		/// <summary>
		/// Handles the Deactivate event of the _parentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void ParentForm_Deactivate(object sender, EventArgs e)
		{
			Owner_LostFocus(this, e);
		}

		/// <summary>
		/// Handles the LostFocus event of the owner control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void Owner_LostFocus(object sender, EventArgs e)
		{
			if (Exclusive)
				Acquired = false;
		}

		/// <summary>
		/// Handles the GotFocus event of the Owner control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void Owner_GotFocus(object sender, EventArgs e)
		{
			if (InputInterface.AutoReacquireDevices)
			{
				if (Exclusive)
					Acquired = true;
			}
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
			if ((!BoundWindow.IsDisposed) && (!BoundWindow.Disposing))
				Gorgon.Log.Print("Unbinding input device object {1} from window 0x{0}.", GorgonLoggingLevel.Intermediate, GorgonUtility.FormatHex(BoundWindow.Handle), GetType().Name);
			else
				Gorgon.Log.Print("Owner window was disposed.", GorgonLoggingLevel.Intermediate);

			if ((BoundForm != null) && (!BoundForm.IsDisposed) && (!BoundForm.Disposing))
			{
				BoundForm.Activated -= new EventHandler(ParentForm_Activated);
				BoundForm.Deactivate -= new EventHandler(ParentForm_Deactivate);
			}
			if ((BoundWindow != null) && (!BoundWindow.IsDisposed) && (!BoundWindow.Disposing))
			{				
				BoundWindow.LostFocus -= new EventHandler(Owner_LostFocus);
				BoundWindow.GotFocus -= new EventHandler(Owner_GotFocus);
			}
		}

		/// <summary>
		/// Function called when the device is bound to a window.
		/// </summary>
		/// <param name="window">Window that was bound.</param>
		/// <remarks>Implementors will override this function to assign events to the window in their child objects.</remarks>
		protected virtual void OnWindowBound(Control window)
		{
		}

		/// <summary>
		/// Function to bind the device to a window.
		/// </summary>
		/// <param name="boundWindow">Window to bind with.</param>
		public void BindWindow(Control boundWindow)
		{
			Control parentWindow = null;

			if (BoundWindow != null)
			{
				UnbindWindow();
				BoundWindow = null;
				BoundForm = null;
			}

			if (boundWindow == null)
			{
				if (Gorgon.ApplicationWindow == null)
					throw new ArgumentException("There is no application window to bind with.", "boundWindow");

				boundWindow = Gorgon.ApplicationWindow;
			}

			Gorgon.Log.Print("Binding input device object {1} to window 0x{0}.", GorgonLoggingLevel.Intermediate, GorgonUtility.FormatHex(boundWindow.Handle), GetType().Name);

			BoundWindow = boundWindow;			
			parentWindow = boundWindow;
			BoundForm = boundWindow as Form;

			while ((BoundForm == null) && (parentWindow != null))
			{
				parentWindow = parentWindow.Parent;
				BoundForm = parentWindow as Form;
			}

			if (BoundForm == null)
				throw new ArgumentException("Cannot bind to the window, no parent form was found.", "boundWindow");

			BoundForm.Activated += new EventHandler(ParentForm_Activated);
			BoundForm.Deactivate += new EventHandler(ParentForm_Deactivate);
			BoundWindow.LostFocus += new EventHandler(Owner_LostFocus);
			BoundWindow.GotFocus += new EventHandler(Owner_GotFocus);

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
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</remarks>
		protected internal GorgonInputDevice(GorgonInputDeviceFactory owner, string deviceName, Control boundWindow)
			: base(deviceName)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");
			InputInterface = owner;

			BindWindow(boundWindow);
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

					if (_bound)
						UnbindDevice();
					_acquired = false;
					_bound = false;
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
