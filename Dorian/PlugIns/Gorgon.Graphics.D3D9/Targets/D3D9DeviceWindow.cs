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
		private GorgonD3D9Graphics _graphics = null;			// Direct 3D9 specific instance of the graphics object.
		private bool _disposed = false;							// Flag to indicate that the object was disposed.
		private PresentParameters[] _presentParams = null;		// Presentation parameters.
		private bool _deviceIsLost = true;						// Flag to indicate that the device is in a lost state.
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
				Form window = BoundWindow as Form;

				if (D3DDevice == null)
					return false;

				if (window == null)
					window = BoundForm;

				return ((!_deviceIsLost) && (window.WindowState != FormWindowState.Minimized) && (BoundWindow.ClientSize.Height > 0));
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform a device reset.
		/// </summary>
		private void ResetDevice()
		{
			Form window = BoundWindow as Form;
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
				if ((window.WindowState != FormWindowState.Normal) && (!IsWindowed))
				{
					if (window.WindowState == FormWindowState.Minimized)
					{
						window.WindowState = FormWindowState.Normal;
						UpdateTargetInformation(new GorgonVideoMode(window.ClientSize.Width, window.ClientSize.Height, Mode.Format, Mode.RefreshRateNumerator, Mode.RefreshRateDenominator), DepthStencilFormat, MultiSampleAALevel);
					}
					else
					{
						UpdateTargetInformation(VideoOutput.DefaultVideoMode, DepthStencilFormat, MultiSampleAALevel);
						window.WindowState = FormWindowState.Normal;
					}
				}

				if ((IsWindowed) && (!VideoOutput.DesktopDimensions.Contains(window.DisplayRectangle)))
				{
					window.Location = VideoOutput.DesktopDimensions.Location;
					window.Size = VideoOutput.DesktopDimensions.Size;
					UpdateTargetInformation(new GorgonVideoMode(window.ClientSize.Width, window.ClientSize.Height, Mode.Format, Mode.RefreshRateNumerator, Mode.RefreshRateDenominator), DepthStencilFormat, MultiSampleAALevel);
				} 
			}	
			
			SetPresentationParameters();
			D3DDevice.Reset(_presentParams[0]);
			AdjustWindow(inWindowedMode);
			_deviceIsLost = false;
			Settings.DisplayMode = new GorgonVideoMode(_presentParams[0].BackBufferWidth, _presentParams[0].BackBufferHeight, D3DConvert.Convert(_presentParams[0].BackBufferFormat), _presentParams[0].FullScreenRefreshRateInHertz, 1);
			OnAfterDeviceReset();
			Gorgon.Log.Print("IDirect3DDevice9 interface has been reset.", Diagnostics.GorgonLoggingLevel.Verbose);
		}

		/// <summary>
		/// Function to set the presentation parameters for the device window.
		/// </summary>
		private void SetPresentationParameters()
		{
			Form window = BoundWindow as Form;

			if (Settings.AdvancedSettings.DisplayFunction == GorgonDisplayFunction.Copy)
				Settings.AdvancedSettings.BackBufferCount = 1;

			if (Mode.Format == GorgonBufferFormat.Unknown)
			{
				// If we didn't specify the back buffer format, then set the default.
				UpdateTargetInformation(new GorgonVideoMode(Mode.Width, 
						Mode.Height, 
						GorgonBufferFormat.X8_R8G8B8_UIntNormal, 
						VideoOutput.DefaultVideoMode.RefreshRateNumerator, 
						VideoOutput.DefaultVideoMode.RefreshRateDenominator), DepthStencilFormat, MultiSampleAALevel);
			}

			// If we're maximized, then use the desktop resolution.
			if ((window != null) && (window.WindowState == FormWindowState.Maximized))
				UpdateTargetInformation(VideoOutput.DefaultVideoMode, DepthStencilFormat, MultiSampleAALevel);
	
			// If we are not windowed, don't allow an unknown video mode.
			if (!IsWindowed)
			{
				int count = 0;

				// If we've not specified a refresh rate, then find the lowest refresh on the output.
				if ((Mode.RefreshRateDenominator == 0) || (Mode.RefreshRateNumerator == 0))
				{
					if (VideoOutput.VideoModes.Count(item => item.Width == Mode.Width && item.Height == Mode.Height && item.Format == Mode.Format) > 0)
					{					
						var refresh = (from mode in VideoOutput.VideoModes
									  where ((mode.Width == Mode.Width) && (mode.Height == Mode.Height) && (mode.Format == Mode.Format))
									  select mode.RefreshRateNumerator).Min();
						UpdateTargetInformation(new GorgonVideoMode(Mode.Width, Mode.Height, Mode.Format, refresh, 1), DepthStencilFormat, MultiSampleAALevel);
					}
				}					

				count = VideoOutput.VideoModes.Count(item => item == Mode);

				if (count == 0)
					throw new GorgonException(GorgonResult.CannotBind, "Unable to set the video mode.  The mode '" + Mode.Width.ToString() + "x" +
						Mode.Height.ToString() + " Format: " + Mode.Format.ToString() + " Refresh Rate: " +
						Mode.RefreshRateNumerator.ToString() + "/" + Mode.RefreshRateDenominator.ToString() +
						"' is not a valid full screen video mode for this video output.");
			}

			// If the window is just a child control, then use the size of the client control instead.
			if (!(BoundWindow is Form))
				UpdateTargetInformation(new GorgonVideoMode(BoundWindow.ClientSize.Width, BoundWindow.ClientSize.Height, Mode.Format, Mode.RefreshRateNumerator, Mode.RefreshRateDenominator), DepthStencilFormat, MultiSampleAALevel);
			
			_presentParams = new PresentParameters[] {
				new PresentParameters() {
					AutoDepthStencilFormat = SlimDX.Direct3D9.Format.Unknown,
					BackBufferCount = Settings.AdvancedSettings.BackBufferCount,
					BackBufferFormat = D3DConvert.Convert(Mode.Format),
					BackBufferHeight = Mode.Height,
					BackBufferWidth = Mode.Width,
					DeviceWindowHandle = BoundWindow.Handle,
					EnableAutoDepthStencil = false,
					Windowed = IsWindowed,
					FullScreenRefreshRateInHertz = Mode.RefreshRateNumerator,
					Multisample = MultisampleType.None,
					MultisampleQuality = 0,
					PresentationInterval = D3DConvert.Convert(Settings.AdvancedSettings.VSyncInterval),
					PresentFlags = (Settings.AdvancedSettings.WillUseVideo ? PresentFlags.Video : PresentFlags.None),
					SwapEffect = D3DConvert.Convert(Settings.AdvancedSettings.DisplayFunction)
			}};

			if (!VideoOutput.SupportsBackBufferFormat(Mode.Format, IsWindowed))
				throw new GorgonException(GorgonResult.FormatNotSupported, "Cannot use the specified format '" + Mode.Format.ToString() + "' with the display format '" + VideoOutput.DefaultVideoMode.Format.ToString() + "'.");

			// Set up the depth buffer if necessary.
			if (DepthStencilFormat != GorgonBufferFormat.Unknown)
			{
				if (!VideoOutput.SupportsDepthFormat(Mode.Format, DepthStencilFormat, IsWindowed))
					throw new GorgonException(GorgonResult.FormatNotSupported, "Cannot use the specified depth/stencil format '" + DepthStencilFormat.ToString() + "'.");

				_presentParams[0].AutoDepthStencilFormat = D3DConvert.Convert(DepthStencilFormat);
				_presentParams[0].EnableAutoDepthStencil = true;
			}

			// We can only enable multi-sampling when we have a discard swap effect and have non-lockable depth buffers.
			if (MultiSampleAALevel != null) 
			{
				if ((_presentParams[0].SwapEffect == SwapEffect.Discard) && (_presentParams[0].AutoDepthStencilFormat != SlimDX.Direct3D9.Format.D32SingleLockable) 
					&& (_presentParams[0].AutoDepthStencilFormat != SlimDX.Direct3D9.Format.D16Lockable))
				{
					int? qualityLevel = VideoDevice.GetMultiSampleQuality(MultiSampleAALevel.Value.Level, Mode.Format, IsWindowed);

					if ((qualityLevel != null) && (qualityLevel >= MultiSampleAALevel.Value.Quality))
					{
						_presentParams[0].Multisample = D3DConvert.Convert(MultiSampleAALevel.Value.Level);
						_presentParams[0].MultisampleQuality = MultiSampleAALevel.Value.Quality - 1;
					}
					else
						throw new ArgumentException("The device cannot support the quality level '" + MultiSampleAALevel.Value.Level.ToString() + "' at a quality of '" + MultiSampleAALevel.Value.Quality.ToString() + "'");
				}
				else
					throw new ArgumentException("Cannot use multi sampling with device windows that don't have a Discard display function and/or use lockable depth/stencil buffers.");
			}
		
			if (IsWindowed)
			{
				// These parameters are meaningless in windowed mode.
				_presentParams[0].PresentationInterval = PresentInterval.Immediate;
				_presentParams[0].FullScreenRefreshRateInHertz = 0;
			}

			Gorgon.Log.Print("Direct3D presentation parameters:", Diagnostics.GorgonLoggingLevel.Verbose);
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
		}

		/// <summary>
		/// Function to adjust the window for windowed/full screen.
		/// </summary>
		/// <param name="inWindowedMode">TRUE to indicate that we're already in windowed mode when switching to fullscreen.</param>
		private void AdjustWindow(bool inWindowedMode)
		{
			Form window = BoundWindow as Form;

			if (window == null)
				return;

			// If switching to windowed mode, then restore the form.  Otherwise, record the current state.
			if (IsWindowed)
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

			if ((window.ClientSize.Width != Mode.Width) || (window.ClientSize.Height != Mode.Height))
				window.ClientSize = new System.Drawing.Size(Mode.Width, Mode.Height);

			if (!IsWindowed)
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
				D3DDevice = new Device(_graphics.D3D, ((D3D9VideoDevice)VideoDevice).AdapterIndex, _graphics.DeviceType, BoundForm.Handle, flags | CreateFlags.PureDevice, _presentParams);
				Gorgon.Log.Print("IDirect3D9 Pure Device interface created.", Diagnostics.GorgonLoggingLevel.Verbose);
			}
			catch(SlimDXException)
			{
				D3DDevice = new Device(_graphics.D3D, ((D3D9VideoDevice)VideoDevice).AdapterIndex, _graphics.DeviceType, BoundForm.Handle, flags, _presentParams);
				Gorgon.Log.Print("IDirect3D9 Device interface created.", Diagnostics.GorgonLoggingLevel.Verbose);
			}

			_deviceIsLost = false;

			Settings.DisplayMode = new GorgonVideoMode(_presentParams[0].BackBufferWidth, _presentParams[0].BackBufferHeight, D3DConvert.Convert(_presentParams[0].BackBufferFormat), _presentParams[0].FullScreenRefreshRateInHertz, 1);
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
		/// <param name="device">Video device to use.</param>
		/// <param name="output">Video output on the device to use.</param>
		/// <param name="settings">Device window settings.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="device"/> and <paramref name="output"/> parameters are NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		public D3D9DeviceWindow(GorgonD3D9Graphics graphics, string name, GorgonVideoDevice device, GorgonVideoOutput output, GorgonDeviceWindowSettings settings)
			: base(graphics, name, device, output, settings)
		{
			_graphics = graphics as GorgonD3D9Graphics;
		}
		#endregion
	}
}
