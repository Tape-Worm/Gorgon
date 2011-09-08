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
// Created: Saturday, July 30, 2011 1:27:24 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// A D3D9 implementation of the device window.
	/// </summary>
	internal class D3D9DeviceWindow
		: GorgonDeviceWindow
	{		
		#region Variables.
		private GorgonD3D9Graphics _graphics = null;						// Direct 3D9 specific instance of the graphics object.
		private bool _disposed = false;										// Flag to indicate that the object was disposed.
		private PresentParameters[] _presentParams = null;					// Presentation parameters.
		private bool _deviceIsLost = true;									// Flag to indicate that the device is in a lost state.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a list of unmanaged objects.
		/// </summary>
		public UnmanagedObjectsCollection UnmanagedObjects
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the Direct3D 9 device.
		/// </summary>
		public Device D3DDevice
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the target is ready to receive rendering data.
		/// </summary>
		public override bool IsReady
		{
			get 
			{
				Form window = Settings.BoundWindow as Form;

				if (D3DDevice == null)
					return false;

				if (window == null)
					window = Settings.BoundForm;

				// If the device is in a lost state, then try to reset it.
				if (_deviceIsLost)
				{
					Result result = this.D3DDevice.TestCooperativeLevel();
					if (result == ResultCode.DeviceNotReset)
					{
						RemoveEventHandlers();
						ResetDevice();
						AddEventHandlers();
					}
				}

				return ((!_deviceIsLost) && (window.WindowState != FormWindowState.Minimized) && (Settings.BoundWindow.ClientSize.Height > 0));
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the swap chain, and surfaces for the window.
		/// </summary>
		/// <param name="settings">Settings for the device window/head.</param>
		private void GetData()
		{
			GorgonSwapChainSettings settings = Settings;

			for (int i = 0; i < _presentParams.Length; i++)
			{				
				if (i > 0)
					settings = Settings.HeadSettings[i - 1];

				if (SwapChains[i] == null)
					SwapChains[i] = new D3D9SwapChain(_graphics, this, Name + "_SwapChain", settings, D3DDevice.GetSwapChain(i));
			}

			//CurrentHead = 0;
		}

		/// <summary>
		/// Function to release any instances of objects gathered with GetData().
		/// </summary>
		private void FreeData()
		{
			for (int i = 0; i < HeadCount; i++)
			{
				SwapChains[i].Dispose();
				SwapChains[i] = null;
			}

			Surface = null;
			DepthStencilSurface = null;
		}

		/// <summary>
		/// Function to perform a device reset.
		/// </summary>
		private void ResetDevice()
		{
			Form window = Settings.BoundWindow as Form;
			bool inWindowedMode = _presentParams[0].Windowed;

			_deviceIsLost = true;
			Gorgon.Log.Print("Device has not been reset.", Diagnostics.GorgonLoggingLevel.Verbose);

			FreeData();
			OnBeforeDeviceReset();			
			
			// HACK: For some reason when the window is maximized and then eventually
			// set to full screen, the mouse cursor screws up and we can click right through
			// our window into whatever is behind it.  Setting it to normal size prior to
			// resetting it seems to work.
			if (window != null)
			{
				if ((window.WindowState != FormWindowState.Normal) && (!Settings.IsWindowed))
				{
					if (window.WindowState == FormWindowState.Minimized)
					{
						window.WindowState = FormWindowState.Normal;
						Settings.Dimensions = window.ClientSize;
					}
					else
					{
						Settings.Dimensions = new Size(Settings.Output.DefaultVideoMode.Width, Settings.Output.DefaultVideoMode.Height);
						Settings.Format = Settings.Output.DefaultVideoMode.Format;
						Settings.RefreshRateDenominator = 1;
						Settings.RefreshRateNumerator = Settings.Output.DefaultVideoMode.RefreshRateNumerator;
						window.WindowState = FormWindowState.Normal;
					}
				}

				if ((Settings.IsWindowed) && (!Settings.Output.DesktopDimensions.Contains(window.DesktopLocation)))
				{
					window.DesktopLocation = Settings.Output.DesktopDimensions.Location;
					window.Size = Settings.Output.DesktopDimensions.Size;
					Settings.Dimensions = window.ClientSize;
				} 
			}	
			
			SetPresentationParameters();
			D3DDevice.Reset(_presentParams[0]);
			AdjustWindow(inWindowedMode);
			// Ensure that the device is indeed restored, multiple monitor configurations will set the primary device into a lost state after a reset.
			_deviceIsLost = ((D3DDevice.TestCooperativeLevel() == ResultCode.DeviceLost) || (D3DDevice.TestCooperativeLevel() == ResultCode.DeviceNotReset));
			if (!_deviceIsLost)
			{
				// Force focus back to the focus window.
				_graphics.FocusWindow.Focus();
				
				// Get the surfaces.
				GetData();
				
				OnAfterDeviceReset();
			}
			Gorgon.Log.Print("IDirect3DDevice9 interface has been reset.", Diagnostics.GorgonLoggingLevel.Verbose);
		}		

		/// <summary>
		/// Function to set the presentation parameters for the device window.
		/// </summary>
		private void SetPresentationParameters()
		{			
			_presentParams[0] = D3DConvert.Convert(Settings);

			Gorgon.Log.Print("Direct3D master head presentation parameters:", Diagnostics.GorgonLoggingLevel.Verbose);
			Gorgon.Log.Print("\tAutoDepthStencilFormat: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[0].AutoDepthStencilFormat);
			Gorgon.Log.Print("\tBackBufferCount: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[0].BackBufferCount);
			Gorgon.Log.Print("\tBackBufferFormat: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[0].BackBufferFormat);
			Gorgon.Log.Print("\tBackBufferWidth: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[0].BackBufferWidth);
			Gorgon.Log.Print("\tBackBufferHeight: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[0].BackBufferHeight);
			Gorgon.Log.Print("\tDeviceWindowHandle: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[0].DeviceWindowHandle.FormatHex());
			Gorgon.Log.Print("\tEnableAutoDepthStencil: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[0].EnableAutoDepthStencil);
			Gorgon.Log.Print("\tFullScreenRefreshRateInHertz: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[0].FullScreenRefreshRateInHertz);
			Gorgon.Log.Print("\tMultisample: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[0].Multisample);
			Gorgon.Log.Print("\tMultisampleQuality: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[0].MultisampleQuality);
			Gorgon.Log.Print("\tPresentationInterval: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[0].PresentationInterval);
			Gorgon.Log.Print("\tPresentFlags: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[0].PresentFlags);
			Gorgon.Log.Print("\tSwapEffect: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[0].SwapEffect);
			Gorgon.Log.Print("\tWindowed: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[0].Windowed);

			for (int i = 1; i <= Settings.HeadSettings.Count; i++)
			{
				_presentParams[i] = D3DConvert.Convert(Settings.HeadSettings[i - 1]);

				Gorgon.Log.Print("Direct3D head '{0}' presentation parameters:", Diagnostics.GorgonLoggingLevel.Verbose, Settings.HeadSettings[i - 1].Output.Name);
				Gorgon.Log.Print("\tAutoDepthStencilFormat: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[i].AutoDepthStencilFormat);
				Gorgon.Log.Print("\tBackBufferCount: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[i].BackBufferCount);
				Gorgon.Log.Print("\tBackBufferFormat: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[i].BackBufferFormat);
				Gorgon.Log.Print("\tBackBufferWidth: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[i].BackBufferWidth);
				Gorgon.Log.Print("\tBackBufferHeight: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[i].BackBufferHeight);
				Gorgon.Log.Print("\tDeviceWindowHandle: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[i].DeviceWindowHandle.FormatHex());
				Gorgon.Log.Print("\tEnableAutoDepthStencil: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[i].EnableAutoDepthStencil);
				Gorgon.Log.Print("\tFullScreenRefreshRateInHertz: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[i].FullScreenRefreshRateInHertz);
				Gorgon.Log.Print("\tMultisample: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[i].Multisample);
				Gorgon.Log.Print("\tMultisampleQuality: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[i].MultisampleQuality);
				Gorgon.Log.Print("\tPresentationInterval: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[i].PresentationInterval);
				Gorgon.Log.Print("\tPresentFlags: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[i].PresentFlags);
				Gorgon.Log.Print("\tSwapEffect: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[i].SwapEffect);
				Gorgon.Log.Print("\tWindowed: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[i].Windowed);
			}
		}

		/// <summary>
		/// Function to adjust the window for windowed/full screen.
		/// </summary>
		/// <param name="inWindowedMode">TRUE to indicate that we're already in windowed mode when switching to fullscreen.</param>
		private void AdjustWindow(bool inWindowedMode)
		{
			Form window = Settings.BoundWindow as Form;

			if (window == null)
				return;

			// If switching to windowed mode, then restore the form.  Otherwise, record the current state.
			if (Settings.IsWindowed)
			{
				if (!inWindowedMode)
					WindowState.Restore(true, false);
				else
					WindowState.Restore(true, true);
			}

			window.Visible = true;
			window.Enabled = true;

			if ((window.ClientSize.Width != Settings.Width) || (window.ClientSize.Height != Settings.Height))
				window.ClientSize = new System.Drawing.Size(Settings.Width, Settings.Height);

			if (!Settings.IsWindowed)
			{
				window.DesktopLocation = Settings.Output.DesktopDimensions.Location;
				window.FormBorderStyle = FormBorderStyle.None;
				window.TopMost = true;
			}
		}

		/// <summary>
		/// Function called when the device has been put into a lost state.
		/// </summary>
		protected override void OnBeforeDeviceReset()
		{
			base.OnBeforeDeviceReset();
			UnmanagedObjects.DeviceLost();
		}

		/// <summary>
		/// Function called after the device has been reset from a lost state.
		/// </summary>
		protected override void OnAfterDeviceReset()
		{
			UnmanagedObjects.DeviceReset();
			base.OnAfterDeviceReset();
		}

		/// <summary>
		/// Function to perform an update on the resources required by the render target.
		/// </summary>
		protected override void UpdateResources()
		{
			Result result = default(Result);

			if (D3DDevice == null)
				return;

			if ((Settings.HeadSettings.Count > 0) && (Settings.Device.Outputs.Count != Settings.HeadSettings.Count + 1))
				throw new GorgonException(GorgonResult.CannotCreate, "Each subordinate head in a multi head device window must have settings.");

			result = D3DDevice.TestCooperativeLevel();

			if ((result.IsSuccess) || (result == ResultCode.DeviceNotReset))
				ResetDevice();
		}

		/// <summary>
		/// Function to create any resources required by the render target.
		/// </summary>
		protected override void CreateResources()
		{
			CreateFlags flags = CreateFlags.Multithreaded | CreateFlags.FpuPreserve | CreateFlags.HardwareVertexProcessing;
			int index = ((D3D9VideoOutput)Settings.Output).AdapterIndex;

			// Set for multi-head.
			if (Settings.HeadSettings.Count > 0)
				flags |= CreateFlags.AdapterGroupDevice;

			// When we first create, record the last set of window modifications, after that, all bets are off and we own this window and will do as we need.
			WindowState.Update();
			AdjustWindow(true);
			_presentParams = new PresentParameters[Settings.HeadSettings.Count + 1];
			SetPresentationParameters();			

			// Attempt to create a pure device, if that fails, then create a hardware vertex device, anything else will fail.
			try
			{				
				D3DDevice = new Device(_graphics.D3D, index, _graphics.DeviceType, _graphics.FocusWindow.Handle, flags | CreateFlags.PureDevice, _presentParams);
				Gorgon.Log.Print("IDirect3D9 Pure Device interface created.", Diagnostics.GorgonLoggingLevel.Verbose);
			}
			catch(SlimDXException)
			{
				D3DDevice = new Device(_graphics.D3D, index, _graphics.DeviceType, _graphics.FocusWindow.Handle, flags, _presentParams);
				Gorgon.Log.Print("IDirect3D9 Device interface created.", Diagnostics.GorgonLoggingLevel.Verbose);
			}

			// Get the surfaces.
			GetData();

			_deviceIsLost = false;
		}

		/// <summary>
		/// Function to clear out any outstanding resources when the object is disposed.
		/// </summary>
		protected override void CleanUpResources()
		{
			base.CleanUpResources();

			UnmanagedObjects.Clear();

			FreeData();

			// Remove link to the focus window if we're removing this device.
			if (_graphics.FocusWindow == Settings.BoundForm)
				_graphics.FocusWindow = null;

			if (D3DDevice != null)
				D3DDevice.Dispose();
			D3DDevice = null;
			Gorgon.Log.Print("IDirect3DDevice9 interface destroyed.", Diagnostics.GorgonLoggingLevel.Verbose);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_deviceIsLost = true;

					CleanUpResources();					
				}

				_disposed = true;
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to set the current render target.
		/// </summary>
		/// <param name="target">Target to set.</param>
		protected override void SetRenderTargetImpl(GorgonRenderTarget target)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			D3D9Surface bufferSurface = target.Surface as D3D9Surface; 
			D3D9Surface depthSurface = target.DepthStencilSurface as D3D9Surface;

			D3DDevice.SetRenderTarget(0, bufferSurface.D3DSurface);

			if (depthSurface != null)
				D3DDevice.DepthStencilSurface = depthSurface.D3DSurface;
		}

		/// <summary>
		/// Function to create a swap chain.
		/// </summary>
		/// <param name="name">Name of the swap chain.</param>
		/// <param name="settings">Settings for the swap chain.</param>
		/// <returns>
		/// A new Gorgon swap chain.
		/// </returns>
		protected override GorgonSwapChain CreateSwapChainImpl(string name, GorgonSwapChainSettings settings)
		{
			if (Settings.HeadSettings.Count > 0)
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create additional swap chains with full screen multi-head device windows.");

			if (!Settings.IsWindowed)
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create a swap chain while in full screen mode.");

			return new D3D9SwapChain(_graphics, this, name, settings);
		}

		/// <summary>
		/// Function to clear a target.
		/// </summary>
		/// <param name="color">Color to clear with.</param>
		/// <param name="depthValue">Depth buffer value to clear with.</param>
		/// <param name="stencilValue">Stencil value to clear with.</param>
		/// <remarks>This will only clear a depth/stencil buffer if one has been attached to the target, otherwise it will do nothing.
		/// <para>Pass NULL (Nothing in VB.Net) to <paramref name="color"/>, <paramref name="depthValue"/> or <paramref name="stencilValue"/> to exclude that buffer from being cleared.</para>
		/// </remarks>
		internal void ClearTarget(GorgonColor? color, float? depthValue, int? stencilValue)
		{
			ClearFlags flags = ClearFlags.None;

			if (color != null)
				flags |= ClearFlags.Target;
			if ((depthValue != null) && (HasDepthBuffer))
				flags |= ClearFlags.ZBuffer;
			if ((stencilValue != null) && (HasStencilBuffer))
				flags |= ClearFlags.Stencil;

			D3DDevice.Clear(flags, (color == null ? new Color4() : D3DConvert.Convert(color.Value)), (depthValue == null ? 1.0f : depthValue.Value), (stencilValue == null ? 0 : stencilValue.Value));
		}

		/// <summary>
		/// Function to clear a target.
		/// </summary>
		/// <param name="color">Color to clear with.</param>
		/// <param name="depthValue">Depth buffer value to clear with.</param>
		/// <param name="stencilValue">Stencil value to clear with.</param>
		/// <remarks>This will only clear a depth/stencil buffer if one has been attached to the target, otherwise it will do nothing.
		/// <para>Pass NULL (Nothing in VB.Net) to <paramref name="color"/>, <paramref name="depthValue"/> or <paramref name="stencilValue"/> to exclude that buffer from being cleared.</para>
		/// </remarks>
		public override void Clear(GorgonColor? color, float? depthValue, int? stencilValue)
		{
			GorgonRenderTarget previousTarget = CurrentTarget;

			if (CurrentTarget != this)
			{
				previousTarget = CurrentTarget;
				CurrentTarget = this;
			}
			
			ClearTarget(color, depthValue, stencilValue);

			if (CurrentTarget != previousTarget)
				CurrentTarget = previousTarget;
		}

		/// <summary>
		/// Function to display the contents of the device window swap chain.
		/// </summary>
		public override void Display()
		{
			if (D3DDevice == null) 
				return;

			Result result = default(Result);

			Configuration.ThrowOnError = false;
			if (_deviceIsLost)
				result = ResultCode.DeviceLost;
			else
				result = D3DDevice.Present();
			Configuration.ThrowOnError = true;

			if (result == ResultCode.DeviceLost)
			{
				if (!_deviceIsLost)
					Gorgon.Log.Print("Device is in a lost state.", Diagnostics.GorgonLoggingLevel.Verbose);

				_deviceIsLost = true;

				// Test to see if we can reset yet.
				result = D3DDevice.TestCooperativeLevel();

				if (result == ResultCode.DeviceNotReset)
				{
					RemoveEventHandlers();
					ResetDevice();
					AddEventHandlers();
				}
				else
				{
					if (result == ResultCode.DriverInternalError)
						throw new GorgonException(GorgonResult.DriverError, "There was an internal driver error.  Cannot reset the device.");

					if (result.IsSuccess)
						_deviceIsLost = false;
				}
			}
		}

		#region Remove this shit.
		public Test<D3D9DeviceWindow> _test = null;

		/// <summary>
		/// </summary>
		public override void SetupTest()
		{
			_test = new Test<D3D9DeviceWindow>(this, D3DDevice);
		}

		/// <summary>
		/// </summary>
		public override void RunTest(float dt)
		{
			if (_test != null)
			{
				if (CurrentHead == 0)
					_test.Run(dt, Settings);
				else
					_test.Run(dt, Settings.HeadSettings[CurrentHead - 1]);
			}
		}

		/// <summary>
		/// </summary>
		public override void CleanUpTest()
		{
			if (_test != null)
				_test.ShutDown();
		}
		#endregion
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDeviceWindow"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="graphics">The graphics instance that owns this render target.</param>
		/// <param name="settings">Device window settings.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		public D3D9DeviceWindow(GorgonD3D9Graphics graphics, string name, GorgonDeviceWindowSettings settings)
			: base(graphics, name, settings)
		{
			_graphics = graphics as GorgonD3D9Graphics;
			UnmanagedObjects = new UnmanagedObjectsCollection();
		}
		#endregion
	}
}
