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
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform a device reset.
		/// </summary>
		private void ResetDevice()
		{
			bool inWindowedMode = _presentParams[0].Windowed;
			
			// HACK: For some reason when the window is maximized and then eventually
			// set to full screen, the mouse cursor screws up and we can click right through
			// our window into whatever is behind it.  Setting it to normal size prior to
			// resetting it seems to work.
			if (BoundWindow is Form)
			{
				Form window = BoundWindow as Form;

				if ((window.WindowState != FormWindowState.Normal) && (!IsWindowed))
				{
					if (window.WindowState == FormWindowState.Minimized)
					{
						window.WindowState = FormWindowState.Normal;
						UpdateTargetInformation(new GorgonVideoMode(window.ClientSize.Width, window.ClientSize.Height, TargetInformation.Format, TargetInformation.RefreshRateNumerator, TargetInformation.RefreshRateDenominator), DepthStencilFormat);
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
					UpdateTargetInformation(new GorgonVideoMode(window.ClientSize.Width, window.ClientSize.Height, TargetInformation.Format, TargetInformation.RefreshRateNumerator, TargetInformation.RefreshRateDenominator), DepthStencilFormat);
				}
			}
	
			_deviceIsLost = true;
			SetPresentationParameters();
			D3DDevice.Reset(_presentParams[0]);
			AdjustWindow(inWindowedMode);
			_deviceIsLost = false;		

		}

		/// <summary>
		/// Function to set the presentation parameters for the device window.
		/// </summary>
		private void SetPresentationParameters()
		{
			Form window = BoundWindow as Form;

			// If we're maximized, then use the desktop resolution.
			if ((window != null) && (window.WindowState == FormWindowState.Maximized))
			    UpdateTargetInformation(VideoOutput.DefaultVideoMode, DepthStencilFormat);

			// If we are not windowed, don't allow an unknown video mode.
			if (!IsWindowed)
			{
				var count = VideoOutput.VideoModes.Count(item => item == TargetInformation);

				if (count == 0)
					throw new GorgonException(GorgonResult.CannotBind, "Unable to set the video mode.  The mode '" + TargetInformation.Width.ToString() + "x" +
						TargetInformation.Height.ToString() + " Format: " + TargetInformation.Format.ToString() + " Refresh Rate: " +
						TargetInformation.RefreshRateNumerator.ToString() + "/" + TargetInformation.RefreshRateDenominator.ToString() +
						"' is not a valid full screen video mode for this video output.");
			}

			// If the window is just a child control, then use the size of the client control instead.
			if (!(BoundWindow is Form))
				UpdateTargetInformation(new GorgonVideoMode(BoundWindow.ClientSize.Width, BoundWindow.ClientSize.Height, TargetInformation.Format, TargetInformation.RefreshRateNumerator, TargetInformation.RefreshRateDenominator), DepthStencilFormat);

			_presentParams = new PresentParameters[] {
				new PresentParameters() {
					AutoDepthStencilFormat = SlimDX.Direct3D9.Format.Unknown,
					BackBufferCount = 3,
					BackBufferFormat = D3DConvert.GetDisplayFormat(TargetInformation.Format, !IsWindowed),
					BackBufferHeight = TargetInformation.Height,
					BackBufferWidth = TargetInformation.Width,
					DeviceWindowHandle = BoundWindow.Handle,
					EnableAutoDepthStencil = false,
					Windowed = IsWindowed,
					FullScreenRefreshRateInHertz = TargetInformation.RefreshRateNumerator,
					Multisample = MultisampleType.None,
					MultisampleQuality = 0,
					PresentationInterval = PresentInterval.Immediate,
					PresentFlags = PresentFlags.None,
					SwapEffect = SwapEffect.Discard
			}};

			// Set up the depth buffer if necessary.
			if (DepthStencilFormat != GorgonBufferFormat.Unknown)
			{				
				_presentParams[0].AutoDepthStencilFormat = D3DConvert.Convert(DepthStencilFormat, false);
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

			if ((window.ClientSize.Width != TargetInformation.Width) || (window.ClientSize.Height != TargetInformation.Height))
				window.ClientSize = new System.Drawing.Size(TargetInformation.Width, TargetInformation.Height);

			if (!IsWindowed)
			{
				window.Location = new Point(0, 0);
				window.FormBorderStyle = FormBorderStyle.None;
				window.TopMost = true;
			}
		}

		/// <summary>
		/// Function to determine if the active device is ready to render.
		/// </summary>
		/// <returns>
		/// TRUE if the device is ready for rendering, FALSE if not.
		/// </returns>
		protected override bool CanRender()
		{
			Form window = BoundWindow as Form;

			if (D3DDevice == null)
				return false;

			if (window == null)
				window = ParentWindow;

			return ((!_deviceIsLost) && (window.WindowState != FormWindowState.Minimized));
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

			if (_deviceIsLost)
				result = ResultCode.DeviceLost;
			else
				result = D3DDevice.Present();

			if (result == ResultCode.DeviceLost)
			{
				_deviceIsLost = true;

				// Test to see if we can reset yet.
				result = D3DDevice.TestCooperativeLevel();

				if (result == ResultCode.DeviceNotReset)
				{
					ResetDevice();
				}
				else
				{
					if (result == ResultCode.DriverInternalError)
						throw new GorgonException(GorgonResult.DriverError, "There was an internal driver error.  Cannot reset the device.");

					_deviceIsLost = false;
				}
			}
		}

		#region Remove this shit.
		private struct Vertex
		{
			public Vector3 Position;
			public int Color;
		}
		private VertexBuffer _vb = null;
		private VertexDeclaration _vdecl = null;
		private Vertex[] triangle = new Vertex[3];
		private float _pos = -0.2f;
		private Diagnostics.GorgonTimer _timer = null;

		/// <summary>
		/// </summary>
		public override void SetupTest()
		{
			_vdecl = new VertexDeclaration(D3DDevice, new VertexElement[] {new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
																		new VertexElement(0, 12, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0)});

			_vb = new VertexBuffer(D3DDevice, 3 * 16, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
			_vb.Lock(0, 0, LockFlags.None).WriteRange(new [] {
		                new Vertex() { Color = Color.Red.ToArgb(), Position = new Vector3(0.0f, 0.5f, 0.0f) },
                new Vertex() { Color = Color.Blue.ToArgb(), Position = new Vector3(0.5f, -0.5f, 0.0f) },
                new Vertex() { Color = Color.Green.ToArgb(), Position = new Vector3(-0.5f, -0.5f, 0.0f) }
            });

			_vb.Unlock();

			D3DDevice.SetRenderState(RenderState.Lighting, false);
			_timer = new Diagnostics.GorgonTimer();
		}

		/// <summary>
		/// </summary>
		public override void RunTest()
		{
			if (_timer.Milliseconds > 50)
			{
				//_pos += 0.01f;
				_timer.Reset();
			}
			if (CanRender())
			{
				D3DDevice.SetRenderState(RenderState.Lighting, false);
				Viewport view = new Viewport(0, 0, this.TargetInformation.Width, this.TargetInformation.Height, 0.0f, 1.0f);
				
				D3DDevice.Viewport = view;
				D3DDevice.SetTransform(TransformState.Projection, Matrix.PerspectiveLH(1.0f, 1.0f, 0.1f, 10.0f));
				D3DDevice.SetTransform(TransformState.World, Matrix.Identity);
				D3DDevice.SetTransform(TransformState.View, Matrix.LookAtLH(new Vector3(0, 0, _pos), new Vector3(0, 0, 1.0f), Vector3.UnitY));

				D3DDevice.BeginScene();
				D3DDevice.Clear(ClearFlags.All, new Color4(0, 0, 0, 0), 1.0f, 0);
				D3DDevice.SetStreamSource(0, _vb, 0, 16);
				D3DDevice.VertexDeclaration = _vdecl;
				D3DDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
				D3DDevice.EndScene();
			}

			Display();
		}

		/// <summary>
		/// </summary>
		public override void CleanUpTest()
		{
			if (_vdecl != null)
				_vdecl.Dispose();
			if (_vb != null)
				_vb.Dispose();
		}
		#endregion
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="D3D9DeviceWindow"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="device">Video device to use.</param>
		/// <param name="output">Video output on the device to use.</param>
		/// <param name="mode">A video mode structure defining the width, height and format of the render target.</param>
		/// <param name="depthStencilFormat">The depth buffer format (if required) for the target.</param>
		/// <param name="fullScreen">TRUE to go full screen, FALSE to stay windowed.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <param name="device"/>, <param name="output"/> or <param name="window"> parameters are NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		/// <remarks>When passing TRUE to <paramref name="fullScreen"/>, then the <paramref name="window"/> parameter must be a Windows Form object.
		/// <para>The <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateNominator">RefreshRateNominator</see> and the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateDenominator">RefreshRateDenominator</see> 
		/// of the <see cref="GorgonLibrary.Graphics.GorgonVideoMode">GorgonVideoMode</see> type are not relevant when <param name="fullScreen"/> is set to FALSE.</para>
		/// </remarks>
		public D3D9DeviceWindow(GorgonD3D9Graphics graphics, string name, GorgonVideoDevice device, GorgonVideoOutput output, Control window, GorgonVideoMode mode, GorgonBufferFormat depthStencilFormat, bool fullScreen)
			: base(graphics, name, device, output, window, mode, depthStencilFormat, fullScreen)
		{
			_graphics = graphics as GorgonD3D9Graphics;
		}
		#endregion
	}
}
