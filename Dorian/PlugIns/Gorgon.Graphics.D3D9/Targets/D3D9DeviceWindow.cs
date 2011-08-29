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
		private IEnumerable<GorgonDeviceWindowSettings> _settings = null;	// Settings for the device window.
		private bool _masterDevice = false;									// Flag to indicate this is the master of the multi-head group.
		#endregion

		#region Properties.
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

				return ((!_deviceIsLost) && (window.WindowState != FormWindowState.Minimized) && (Settings.BoundWindow.ClientSize.Height > 0));
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform a device reset.
		/// </summary>
		private void ResetDevice()
		{
			Form window = Settings.BoundWindow as Form;
			bool inWindowedMode = _presentParams[0].Windowed;

			_deviceIsLost = true;
			Gorgon.Log.Print("Device has not been reset.", Diagnostics.GorgonLoggingLevel.Verbose);

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

				if ((Settings.IsWindowed) && (!Settings.Output.DesktopDimensions.Contains(window.DisplayRectangle)))
				{
					window.Location = Settings.Output.DesktopDimensions.Location;
					window.Size = Settings.Output.DesktopDimensions.Size;
					Settings.Dimensions = window.ClientSize;
				} 
			}	
			
			SetPresentationParameters();
			// TODO: This is a bug in SlimDX, need to wait until they fix it for multi-head.			
			D3DDevice.Reset(_presentParams[0]);
			AdjustWindow(inWindowedMode);
			// Ensure that the device is indeed restored, multiple monitor configurations will set the primary device into a lost state after a reset.
			_deviceIsLost = ((D3DDevice.TestCooperativeLevel() == ResultCode.DeviceLost) || (D3DDevice.TestCooperativeLevel() == ResultCode.DeviceNotReset));
			OnAfterDeviceReset();
			Gorgon.Log.Print("IDirect3DDevice9 interface has been reset.", Diagnostics.GorgonLoggingLevel.Verbose);
		}

		/// <summary>
		/// Function to set the presentation parameters for the device window.
		/// </summary>
		private void SetPresentationParameters()
		{
			int counter = 0;
			foreach (var setting in _settings)
			{
				Form window = setting.BoundWindow as Form;
				Format _depthFormat = SlimDX.Direct3D9.Format.Unknown;

				if (setting.DisplayFunction == GorgonDisplayFunction.Copy)
					setting.BackBufferCount = 1;

				// If we didn't specify the back buffer format, then set the default.
				if (setting.DisplayMode.Format == GorgonBufferFormat.Unknown)
					setting.Format = GorgonBufferFormat.X8_R8G8B8_UIntNormal;

				// If we're maximized, then use the desktop resolution.
				if ((window != null) && (window.WindowState == FormWindowState.Maximized))
					setting.Dimensions = new Size(setting.Output.DefaultVideoMode.Width, setting.Output.DefaultVideoMode.Height);

				// If we are not windowed, don't allow an unknown video mode.
				if (!setting.IsWindowed)
				{
					int count = 0;

					// If we've not specified a refresh rate, then find the lowest refresh on the output.
					if ((setting.DisplayMode.RefreshRateDenominator == 0) || (setting.DisplayMode.RefreshRateNumerator == 0))
					{
						if (setting.Output.VideoModes.Count(item => item.Width == setting.Width &&
															item.Height == setting.Height &&
															item.Format == setting.Format) > 0)
						{
							var refresh = (from mode in setting.Output.VideoModes
										   where ((mode.Width == setting.Width) && (mode.Height == setting.Height) && (mode.Format == setting.Format))
										   select mode.RefreshRateNumerator).Min();
							setting.RefreshRateNumerator = refresh;
							setting.RefreshRateDenominator = 1;
						}
					}

					count = setting.Output.VideoModes.Count(item => item == setting.DisplayMode);

					if (count == 0)
						throw new GorgonException(GorgonResult.CannotBind, "Unable to set the video mode.  The mode '" + setting.Width.ToString() + "x" +
							setting.Height.ToString() + " Format: " + setting.Format.ToString() + " Refresh Rate: " +
							setting.RefreshRateNumerator.ToString() + "/" + setting.RefreshRateDenominator.ToString() +
							"' is not a valid full screen video mode for this video output.");
				}

				if (!setting.Output.SupportsBackBufferFormat(setting.Format, setting.IsWindowed))
					throw new GorgonException(GorgonResult.FormatNotSupported, "Cannot use the specified format '" + setting.Format.ToString() + "' with the display format '" + setting.Output.DefaultVideoMode.Format.ToString() + "'.");

				// Set up the depth buffer if necessary.
				if (setting.DepthStencilFormat != GorgonBufferFormat.Unknown)
				{
					if (!setting.Output.SupportsDepthFormat(setting.Format, setting.DepthStencilFormat, setting.IsWindowed))
						throw new GorgonException(GorgonResult.FormatNotSupported, "Cannot use the specified depth/stencil format '" + setting.DepthStencilFormat.ToString() + "'.");
					_depthFormat = D3DConvert.Convert(setting.DepthStencilFormat);
				}

				// We can only enable multi-sampling when we have a discard swap effect and have non-lockable depth buffers.
				if (setting.MSAAQualityLevel.Level != GorgonMSAALevel.None)
				{
					if ((setting.DisplayFunction == GorgonDisplayFunction.Discard) && (_depthFormat != SlimDX.Direct3D9.Format.D32SingleLockable)
						&& (_depthFormat != SlimDX.Direct3D9.Format.D16Lockable))
					{
						int? qualityLevel = setting.Device.GetMultiSampleQuality(setting.MSAAQualityLevel.Level, setting.Format, setting.IsWindowed);

						if ((qualityLevel == null) || (qualityLevel < setting.MSAAQualityLevel.Quality))
							throw new ArgumentException("The device cannot support the quality level '" + setting.MSAAQualityLevel.Level.ToString() + "' at a quality of '" + setting.MSAAQualityLevel.Quality.ToString() + "'");
					}
					else
						throw new ArgumentException("Cannot use multi sampling with device windows that don't have a Discard display function and/or use lockable depth/stencil buffers.");
				}

				// Turn off vsync and refresh rates when in windowed mode.
				if (setting.IsWindowed)
				{
					if (setting.VSyncInterval != GorgonVSyncInterval.None)
						setting.VSyncInterval = GorgonVSyncInterval.None;
					if (setting.RefreshRateNumerator > 0)
						setting.RefreshRateNumerator = 0;
				}

				_presentParams[counter] = new PresentParameters() {
									AutoDepthStencilFormat = _depthFormat,
									BackBufferCount = setting.BackBufferCount,
									BackBufferFormat = D3DConvert.Convert(setting.Format),
									BackBufferHeight = setting.Height,
									BackBufferWidth = setting.Width,
									DeviceWindowHandle = setting.BoundWindow.Handle,
									EnableAutoDepthStencil = (_depthFormat != SlimDX.Direct3D9.Format.Unknown),					
									Windowed = setting.IsWindowed,
									FullScreenRefreshRateInHertz = setting.RefreshRateNumerator,
									Multisample = D3DConvert.Convert(setting.MSAAQualityLevel.Level),
									MultisampleQuality =  (setting.MSAAQualityLevel.Level != GorgonMSAALevel.None ? setting.MSAAQualityLevel.Quality - 1 : 0),
									PresentationInterval = D3DConvert.Convert(setting.VSyncInterval),
									PresentFlags = (setting.WillUseVideo ? PresentFlags.Video : PresentFlags.None),
									SwapEffect = D3DConvert.Convert(setting.DisplayFunction)
								};

				Gorgon.Log.Print("Direct3D presentation parameters:", Diagnostics.GorgonLoggingLevel.Verbose);
				Gorgon.Log.Print("\tAutoDepthStencilFormat: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[counter].AutoDepthStencilFormat);
				Gorgon.Log.Print("\tBackBufferCount: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[counter].BackBufferCount);
				Gorgon.Log.Print("\tBackBufferFormat: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[counter].BackBufferFormat);
				Gorgon.Log.Print("\tBackBufferWidth: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[counter].BackBufferWidth);
				Gorgon.Log.Print("\tBackBufferHeight: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[counter].BackBufferHeight);
				Gorgon.Log.Print("\tDeviceWindowHandle: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[counter].DeviceWindowHandle.FormatHex());
				Gorgon.Log.Print("\tEnableAutoDepthStencil: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[counter].EnableAutoDepthStencil);
				Gorgon.Log.Print("\tFullScreenRefreshRateInHertz: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[counter].FullScreenRefreshRateInHertz);
				Gorgon.Log.Print("\tMultisample: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[counter].Multisample);
				Gorgon.Log.Print("\tMultisampleQuality: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[counter].MultisampleQuality);
				Gorgon.Log.Print("\tPresentationInterval: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[counter].PresentationInterval);
				Gorgon.Log.Print("\tPresentFlags: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[counter].PresentFlags);
				Gorgon.Log.Print("\tSwapEffect: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[counter].SwapEffect);
				Gorgon.Log.Print("\tWindowed: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams[counter].Windowed);
				counter++;
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
			else
			{
				// Only update if we're switching back from windowed mode.
				if ((_presentParams == null) || (inWindowedMode))
					WindowState.Update();
			}

			window.Visible = true;
			window.Enabled = true;

			if ((window.ClientSize.Width != Settings.Width) || (window.ClientSize.Height != Settings.Height))
				window.ClientSize = new System.Drawing.Size(Settings.Width, Settings.Height);

			if (!Settings.IsWindowed)
			{
				window.Location = new Point(0, 0);
				window.FormBorderStyle = FormBorderStyle.None;
				window.TopMost = true;
			}
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
		/// Function to create any resources required by the render target.
		/// </summary>
		protected override void CreateResources()
		{
			CreateFlags flags = CreateFlags.Multithreaded | CreateFlags.FpuPreserve | CreateFlags.HardwareVertexProcessing;

			AdjustWindow(true);
			SetPresentationParameters();			

			// Attempt to create a pure device, if that fails, then create a hardware vertex device, anything else will fail.
			try
			{
				D3DDevice = new Device(_graphics.D3D, ((D3D9VideoDevice)Settings.Device).AdapterIndex, _graphics.DeviceType, Settings.BoundForm.Handle, flags | CreateFlags.PureDevice, _presentParams);
				Gorgon.Log.Print("IDirect3D9 Pure Device interface created.", Diagnostics.GorgonLoggingLevel.Verbose);
			}
			catch(SlimDXException)
			{
				D3DDevice = new Device(_graphics.D3D, ((D3D9VideoDevice)Settings.Device).AdapterIndex, _graphics.DeviceType, Settings.BoundForm.Handle, flags, _presentParams);
				Gorgon.Log.Print("IDirect3D9 Device interface created.", Diagnostics.GorgonLoggingLevel.Verbose);
			}

			_deviceIsLost = false;
		}

		/// <summary>
		/// Function to perform the creation of the multi-head window resource.
		/// </summary>
		/// <param name="settings">Settings for each multi-head window.</param>
		/// <param name="deviceWindows"></param>
		protected override void CreateMultiHeadResource(IEnumerable<GorgonDeviceWindowSettings> settings, IEnumerable<GorgonDeviceWindow> deviceWindows)
		{
			CreateFlags flags = CreateFlags.Multithreaded | CreateFlags.FpuPreserve | CreateFlags.HardwareVertexProcessing | CreateFlags.AdapterGroupDevice;

			_masterDevice = true;
			_settings = settings;
			_presentParams = new PresentParameters[settings.Count()];
			AdjustWindow(true);
			SetPresentationParameters();

			// Attempt to create a pure device, if that fails, then create a hardware vertex device, anything else will fail.
			try
			{
				D3DDevice = new Device(_graphics.D3D, ((D3D9VideoDevice)Settings.Device).AdapterIndex, _graphics.DeviceType, settings.ElementAt(0).BoundForm.Handle, flags | CreateFlags.PureDevice, _presentParams);
				Gorgon.Log.Print("IDirect3D9 Pure Device interface created.", Diagnostics.GorgonLoggingLevel.Verbose);
			}
			catch (SlimDXException)
			{
				D3DDevice = new Device(_graphics.D3D, ((D3D9VideoDevice)Settings.Device).AdapterIndex, _graphics.DeviceType, settings.ElementAt(0).BoundForm.Handle, flags, _presentParams);
				Gorgon.Log.Print("IDirect3D9 Device interface created.", Diagnostics.GorgonLoggingLevel.Verbose);
			}

			for (int i = 1; i < deviceWindows.Count(); i++)
			{
				D3D9DeviceWindow window = deviceWindows.ElementAt(i) as D3D9DeviceWindow;
				window.D3DDevice = D3DDevice;
			}

			_deviceIsLost = false;
		}

		/// <summary>
		/// Function to clear out any outstanding resources when the object is disposed.
		/// </summary>
		protected override void CleanUpResources()
		{
			base.CleanUpResources();

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
		/// Initializes a new instance of the <see cref="GorgonDeviceWindow"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="graphics">The graphics instance that owns this render target.</param>
		/// <param name="settings">Device window settings.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="device"/> and <paramref name="output"/> parameters are NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		public D3D9DeviceWindow(GorgonD3D9Graphics graphics, string name, GorgonDeviceWindowSettings settings)
			: base(graphics, name, settings.Device, settings.Output, settings)
		{
			_graphics = graphics as GorgonD3D9Graphics;
			_settings = new[] { settings };
			_presentParams = new PresentParameters[1];
		}
		#endregion
	}
}
