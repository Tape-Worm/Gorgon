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
					window = ParentWindow;

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
						UpdateTargetInformation(new GorgonVideoMode(window.ClientSize.Width, window.ClientSize.Height, Mode.Format, Mode.RefreshRateNumerator, Mode.RefreshRateDenominator), DepthStencilFormat);
					}
					else
					{
						UpdateTargetInformation(VideoOutput.DefaultVideoMode, DepthStencilFormat);
						window.WindowState = FormWindowState.Normal;
					}
				}

				if ((IsWindowed) && (!VideoOutput.DesktopDimensions.Contains(window.DisplayRectangle)))
				{
					window.Location = VideoOutput.DesktopDimensions.Location;
					window.Size = VideoOutput.DesktopDimensions.Size;
					UpdateTargetInformation(new GorgonVideoMode(window.ClientSize.Width, window.ClientSize.Height, Mode.Format, Mode.RefreshRateNumerator, Mode.RefreshRateDenominator), DepthStencilFormat);
				} 
			}	
			
			SetPresentationParameters();
			D3DDevice.Reset(_presentParams[0]);
			AdjustWindow(inWindowedMode);
			_deviceIsLost = false;

			OnAfterDeviceReset();
		}

		/// <summary>
		/// Function to set the presentation parameters for the device window.
		/// </summary>
		private void SetPresentationParameters()
		{
			Form window = BoundWindow as Form;			

			if (Mode.Format == GorgonBackBufferFormat.Unknown)
			{
				UpdateTargetInformation(new GorgonVideoMode(Mode.Width, 
						Mode.Height, 
						VideoOutput.DefaultVideoMode.Format, 
						VideoOutput.DefaultVideoMode.RefreshRateNumerator, 
						VideoOutput.DefaultVideoMode.RefreshRateDenominator), DepthStencilFormat);
			}

			// If we're maximized, then use the desktop resolution.
			if ((window != null) && (window.WindowState == FormWindowState.Maximized))
				UpdateTargetInformation(VideoOutput.DefaultVideoMode, DepthStencilFormat);
	
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
						UpdateTargetInformation(new GorgonVideoMode(Mode.Width, Mode.Height, Mode.Format, refresh, 1), DepthStencilFormat);
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
				UpdateTargetInformation(new GorgonVideoMode(BoundWindow.ClientSize.Width, BoundWindow.ClientSize.Height, Mode.Format, Mode.RefreshRateNumerator, Mode.RefreshRateDenominator), DepthStencilFormat);
			
			_presentParams = new PresentParameters[] {
				new PresentParameters() {
					AutoDepthStencilFormat = SlimDX.Direct3D9.Format.Unknown,
					BackBufferCount = 3,
					BackBufferFormat = D3DConvert.ConvertBackBufferFormat(Mode.Format),
					BackBufferHeight = Mode.Height,
					BackBufferWidth = Mode.Width,
					DeviceWindowHandle = BoundWindow.Handle,
					EnableAutoDepthStencil = false,
					Windowed = IsWindowed,
					FullScreenRefreshRateInHertz = Mode.RefreshRateNumerator,
					Multisample = MultisampleType.None,
					MultisampleQuality = 0,
					PresentationInterval = PresentInterval.Immediate,
					PresentFlags = PresentFlags.None,
					SwapEffect = SwapEffect.Discard
			}};

			if (!VideoOutput.SupportsBackBufferFormat(Mode.Format, IsWindowed))
				throw new GorgonException(GorgonResult.FormatNotSupported, "Cannot use the specified format '" + Mode.Format.ToString() + "' with the display format '" + VideoOutput.DefaultVideoMode.Format.ToString() + "'.");

			if (!VideoOutput.SupportsDepthFormat(Mode.Format, DepthStencilFormat, IsWindowed))
				throw new GorgonException(GorgonResult.FormatNotSupported, "Cannot use the specified depth/stencil format '" + DepthStencilFormat.ToString() + "'.");

			GorgonMSAAQualityLevel? msaaQualityLevel = VideoDevice.SupportsMultiSampleQualityLevel(GorgonMSAALevel.NonMasked, Mode.Format, IsWindowed);

			if (msaaQualityLevel != null)
			{
				_presentParams[0].Multisample = D3DConvert.Convert(msaaQualityLevel.Value.Level);
				_presentParams[0].MultisampleQuality = msaaQualityLevel.Value.Quality - 1;
			}

			// Set up the depth buffer if necessary.
			if (DepthStencilFormat != GorgonDepthBufferFormat.Unknown)
			{				
				_presentParams[0].AutoDepthStencilFormat = D3DConvert.ConvertDepthBufferFormat(DepthStencilFormat);
				_presentParams[0].EnableAutoDepthStencil = true;
			}
			
			if (IsWindowed)
			{
				// These parameters are meaningless in windowed mode.
				_presentParams[0].PresentationInterval = PresentInterval.Immediate;
				_presentParams[0].FullScreenRefreshRateInHertz = 0;
			}

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
		/// Function to perform an update on the render target.
		/// </summary>
		protected override void UpdateRenderTarget()
		{
			Result result = default(Result);

			if (D3DDevice == null)
				return;

			result = D3DDevice.TestCooperativeLevel();

			if ((result.IsSuccess) || (result == ResultCode.DeviceNotReset))
				ResetDevice();
		}

		/// <summary>
		/// Function to create the render target.
		/// </summary>
		protected override void CreateRenderTarget()
		{
			CreateFlags flags = CreateFlags.Multithreaded | CreateFlags.FpuPreserve | CreateFlags.HardwareVertexProcessing;

			AdjustWindow(true);
			SetPresentationParameters();			

			// Attempt to create a pure device, if that fails, then create a hardware vertex device, anything else will fail.
			try
			{
				D3DDevice = new Device(_graphics.D3D, ((D3D9VideoDevice)VideoDevice).AdapterIndex, _graphics.DeviceType, ParentWindow.Handle, flags | CreateFlags.PureDevice, _presentParams);												
			}
			catch(SlimDXException)
			{
				D3DDevice = new Device(_graphics.D3D, ((D3D9VideoDevice)VideoDevice).AdapterIndex, _graphics.DeviceType, ParentWindow.Handle, flags, _presentParams);					
			}

			_deviceIsLost = false;
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
					RemoveEventHandlers();

					if (D3DDevice != null)
						D3DDevice.Dispose();
					D3DDevice = null;					
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
		public override void RunTest()
		{
			_test.Run();
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
		/// <param name="advanced">Advanceed settings.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="device"/> and <paramref name="output"/> parameters are NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		public D3D9DeviceWindow(GorgonD3D9Graphics graphics, string name, GorgonVideoDevice device, GorgonVideoOutput output, GorgonDeviceWindowSettings settings, GorgonDeviceWindowAdvancedSettings advanced)
			: base(graphics, name, device, output, settings, advanced)
		{
			_graphics = graphics as GorgonD3D9Graphics;
		}
		#endregion
	}
}
