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
// Created: Wednesday, August 31, 2011 10:03:47 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D9;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// Direct 3D 9 implementation of the multi-head device window.
	/// </summary>
	class D3D9MultiHeadDeviceWindow
		: GorgonMultiHeadDeviceWindow
	{
		#region Variables.
		private GorgonD3D9Graphics _graphics = null;						// Direct 3D9 specific instance of the graphics object.
		private bool _disposed = false;										// Flag to indicate that the object was disposed.
		private PresentParameters[] _presentParams = null;					// Presentation parameters.
		private bool _deviceIsLost = true;									// Flag to indicate that the device is in a lost state.
		private D3D9DeviceWindow _deviceWindowProxy = null;					// The proxy for manipulating the device window.
		#endregion

		#region Properties.		
		/// <summary>
		/// Property to return the swap back buffer surfaces for each head.
		/// </summary>
		public Surface[] SwapSurfaces
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the depth/stencil surfaces for each head.
		/// </summary>
		public Surface[] DepthStencilSurfaces
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
				foreach (var setting in Settings.Settings)
				{
					Form window = setting.BoundWindow as Form;

					if (D3DDevice == null)
						return false;

					if (window == null)
						window = setting.BoundForm;

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

					if ((_deviceIsLost) || (window.WindowState == FormWindowState.Minimized) || (setting.BoundWindow.ClientSize.Height < 1))
						return false;
				}

				return true;				
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform a device reset.
		/// </summary>
		private void ResetDevice()
		{
			// TODO: Currently there is a bug in SlimDX where the Reset() method only takes one presentation parameter object, it's supposed to take multiple.
			//Form window = Settings.BoundWindow as Form;
			//bool inWindowedMode = _presentParams[0].Windowed;

			//_deviceIsLost = true;
			//Gorgon.Log.Print("Device has not been reset.", Diagnostics.GorgonLoggingLevel.Verbose);

			//OnBeforeDeviceReset();			
			
			//// HACK: For some reason when the window is maximized and then eventually
			//// set to full screen, the mouse cursor screws up and we can click right through
			//// our window into whatever is behind it.  Setting it to normal size prior to
			//// resetting it seems to work.
			//if (window != null)
			//{
			//    if ((window.WindowState != FormWindowState.Normal) && (!Settings.IsWindowed))
			//    {
			//        if (window.WindowState == FormWindowState.Minimized)
			//        {
			//            window.WindowState = FormWindowState.Normal;
			//            Settings.Dimensions = window.ClientSize;
			//        }
			//        else
			//        {
			//            Settings.Dimensions = new Size(Settings.Output.DefaultVideoMode.Width, Settings.Output.DefaultVideoMode.Height);
			//            Settings.Format = Settings.Output.DefaultVideoMode.Format;
			//            Settings.RefreshRateDenominator = 1;
			//            Settings.RefreshRateNumerator = Settings.Output.DefaultVideoMode.RefreshRateNumerator;
			//            window.WindowState = FormWindowState.Normal;
			//        }
			//    }

			//    if ((Settings.IsWindowed) && (!Settings.Output.DesktopDimensions.Contains(window.DesktopLocation)))
			//    {
			//        window.DesktopLocation = Settings.Output.DesktopDimensions.Location;
			//        window.Size = Settings.Output.DesktopDimensions.Size;
			//        Settings.Dimensions = window.ClientSize;
			//    } 
			//}	
			
			//SetPresentationParameters();
			//D3DDevice.Reset(_presentParams[0]);
			//AdjustWindow(inWindowedMode);
			//// Ensure that the device is indeed restored, multiple monitor configurations will set the primary device into a lost state after a reset.
			//_deviceIsLost = ((D3DDevice.TestCooperativeLevel() == ResultCode.DeviceLost) || (D3DDevice.TestCooperativeLevel() == ResultCode.DeviceNotReset));
			//if (!_deviceIsLost)
			//{
			//    // Force focus back to the focus window.
			//    _graphics.FocusWindow.Focus();
			//    OnAfterDeviceReset();
			//}
			//Gorgon.Log.Print("IDirect3DDevice9 interface has been reset.", Diagnostics.GorgonLoggingLevel.Verbose);
		}		

		/// <summary>
		/// Function to set the presentation parameters for the device window.
		/// </summary>
		private void SetPresentationParameters()
		{
			foreach (var setting in Settings.Settings)
			{
				int index = ((D3D9VideoOutput)setting.Output).HeadIndex;

				_presentParams[index] = D3DConvert.Convert(setting);
				// Turn of the auto depth buffer for the multi-head device.
				_presentParams[index].EnableAutoDepthStencil = false;
				_presentParams[index].AutoDepthStencilFormat = Format.Unknown;

				Gorgon.Log.Print("Direct3D presentation parameters for head {0}:", Diagnostics.GorgonLoggingLevel.Verbose, index);
				Gorgon.Log.Print("\tAutoDepthStencilFormat: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[index].AutoDepthStencilFormat);
				Gorgon.Log.Print("\tBackBufferCount: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[index].BackBufferCount);
				Gorgon.Log.Print("\tBackBufferFormat: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[index].BackBufferFormat);
				Gorgon.Log.Print("\tBackBufferWidth: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[index].BackBufferWidth);
				Gorgon.Log.Print("\tBackBufferHeight: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[index].BackBufferHeight);
				Gorgon.Log.Print("\tDeviceWindowHandle: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[index].DeviceWindowHandle.FormatHex());
				Gorgon.Log.Print("\tEnableAutoDepthStencil: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[index].EnableAutoDepthStencil);
				Gorgon.Log.Print("\tFullScreenRefreshRateInHertz: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[index].FullScreenRefreshRateInHertz);
				Gorgon.Log.Print("\tMultisample: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[index].Multisample);
				Gorgon.Log.Print("\tMultisampleQuality: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[index].MultisampleQuality);
				Gorgon.Log.Print("\tPresentationInterval: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[index].PresentationInterval);
				Gorgon.Log.Print("\tPresentFlags: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[index].PresentFlags);
				Gorgon.Log.Print("\tSwapEffect: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[index].SwapEffect);
				Gorgon.Log.Print("\tWindowed: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[index].Windowed);
			}
		}

		/// <summary>
		/// Function to adjust the window for windowed/full screen.
		/// </summary>
		private void AdjustWindow()
		{
			foreach (var setting in Settings.Settings)
			{
				setting.BoundForm.Visible = true;
				setting.BoundForm.Enabled = true;

				if ((setting.BoundForm.ClientSize.Width != setting.Width) || (setting.BoundForm.ClientSize.Height != setting.Height))
					setting.BoundForm.ClientSize = new System.Drawing.Size(setting.Width, setting.Height);

				setting.BoundForm.DesktopLocation = setting.Output.DesktopDimensions.Location;
				setting.BoundForm.FormBorderStyle = FormBorderStyle.None;
				setting.BoundForm.TopMost = true;
			}
		}

		/// <summary>
		/// Function to set the currently active head for rendering.
		/// </summary>
		/// <param name="headIndex">Index of the head.</param>
		protected override void SetCurrentHead(int headIndex)
		{
			D3DDevice.SetRenderTarget(0, SwapSurfaces[headIndex]);
			if (Settings.Settings[headIndex].DepthStencilFormat != GorgonBufferFormat.Unknown)
				D3DDevice.DepthStencilSurface = DepthStencilSurfaces[headIndex];
			else
				D3DDevice.DepthStencilSurface = null;
		}

		/// <summary>
		/// Function to perform an update on the resources required by the render target.
		/// </summary>
		protected override void UpdateResources()
		{
			Result result = default(Result);

			if (D3DDevice == null)
				return;

			result = D3DDevice.TestCooperativeLevel();

			if ((result.IsSuccess) || (result == ResultCode.DeviceNotReset))
				ResetDevice();
		}

		/// <summary>
		/// Function to clean up the surfaces allocated by the device.
		/// </summary>
		private void DestroySurfaces()
		{
			if ((SwapSurfaces != null) && (SwapSurfaces.Length > 0))
			{
				Gorgon.Log.Print("Destroying swap chain back buffer surfaces.", Diagnostics.GorgonLoggingLevel.Verbose);
				for (int i = 0; i < SwapSurfaces.Length; i++)
				{
					if (SwapSurfaces[i] != null)
						SwapSurfaces[i].Dispose();
				}
			}

			if ((DepthStencilSurfaces != null) && (DepthStencilSurfaces.Length > 0))
			{
				Gorgon.Log.Print("Destroying depth/stencil buffer surfaces.", Diagnostics.GorgonLoggingLevel.Verbose);
				for (int i = 0; i < DepthStencilSurfaces.Length; i++)
				{
					if (DepthStencilSurfaces[i] != null)
						DepthStencilSurfaces[i].Dispose();
				}
			}

			DepthStencilSurfaces = null;
			SwapSurfaces = null;
		}

		/// <summary>
		/// Function to retrieve a copy of the multi-head swap chain back buffers and depth/stencil buffers.
		/// </summary>
		private void GetSurfaces()
		{
			SwapChain swapChain = null;
			GorgonDeviceWindowSettings setting = null;

			try
			{
				DestroySurfaces();

				SwapSurfaces = new Surface[HeadCount];
				DepthStencilSurfaces = new Surface[HeadCount];
								
				for (int i = 0; i < HeadCount; i++)
				{
					Gorgon.Log.Print("Retrieving back buffer surfaces and creating depth/stencil surfaces for head {0}.", Diagnostics.GorgonLoggingLevel.Verbose, i);

					swapChain = D3DDevice.GetSwapChain(i);
					setting = Settings.Settings[i];

					SwapSurfaces[i] = swapChain.GetBackBuffer(0);

					// Create a depth buffer for the swap chain.
					if (setting.DepthStencilFormat != GorgonBufferFormat.Unknown)					
						DepthStencilSurfaces[i] = Surface.CreateDepthStencil(D3DDevice, setting.Width, setting.Height, D3DConvert.Convert(setting.DepthStencilFormat), D3DConvert.Convert(setting.MSAAQualityLevel.Level), (setting.MSAAQualityLevel.Level != GorgonMSAALevel.None ? setting.MSAAQualityLevel.Quality - 1 : 0), false);

					swapChain.Dispose();
					swapChain = null;
				}
			}
			finally
			{
				if (swapChain != null)
					swapChain.Dispose();
				swapChain = null;
			}
		}

		/// <summary>
		/// Function to create any resources required by the render target.
		/// </summary>
		protected override void CreateResources()
		{		
			CreateFlags flags = CreateFlags.Multithreaded | CreateFlags.FpuPreserve | CreateFlags.HardwareVertexProcessing | CreateFlags.AdapterGroupDevice;
			int index = ((D3D9VideoDevice)Settings.Device).AdapterIndex;

			// When we first create, record the last set of window modifications, after that, all bets are off and we own this window and will do as we need.
			WindowState.Update();
			AdjustWindow();
			_presentParams = new PresentParameters[Settings.Settings.Count()];
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

			GetSurfaces();

			// Create our proxy object to pass off responsibility for creating our child objects.
			_deviceWindowProxy = new D3D9DeviceWindow(_graphics, this);

			_deviceIsLost = false;
		}

		/// <summary>
		/// Function to clear out any outstanding resources when the object is disposed.
		/// </summary>
		protected override void CleanUpResources()
		{
			base.CleanUpResources();

			// Destroy our proxy window.
			_deviceWindowProxy.DisposeProxy();

			DestroySurfaces();

			// Remove link to the focus window if we're removing this device.
			if (_graphics.FocusWindow == Settings.Settings[0].BoundForm)
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
		/// Function to display the contents of the swap chain.
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
		private Test _test = null;

		/// <summary>
		/// </summary>
		public override void SetupTest()
		{
			_test = new Test(this, D3DDevice);
		}

		/// <summary>
		/// </summary>
		public override void RunTest(float dt)
		{
			_test.Run(dt);
		}

		/// <summary>
		/// </summary>
		public override void CleanUpTest()
		{
			_test.ShutDown();
		}
		#endregion
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="D3D9MultiHeadDeviceWindow"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="graphics">The graphics instance that owns this render target.</param>
		/// <param name="settings">Device window settings.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		public D3D9MultiHeadDeviceWindow(GorgonD3D9Graphics graphics, string name, GorgonMultiHeadSettings settings)
			: base(graphics, name, settings)
		{
			_graphics = graphics as GorgonD3D9Graphics;			
		}
		#endregion
	}
}
