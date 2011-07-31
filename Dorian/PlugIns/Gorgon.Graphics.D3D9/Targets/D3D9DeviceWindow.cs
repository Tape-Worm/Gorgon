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
		private bool _disposed = false;							// Flag to indicate that the object was disposed.
		private PresentParameters[] _presentParams = null;		// Presentation parameters.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the graphics instance that created this object.
		/// </summary>
		private GorgonD3D9Graphics Graphics
		{
			get;
			set;
		}

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
		/// Function to set the presentation parameters for the device window.
		/// </summary>
		/// <param name="isMultiHead">TRUE if using a multi-head adapter. FALSE if not.</param>
		private void SetPresentationParameters(bool isMultiHead)
		{
			_presentParams = new PresentParameters[VideoDevice.Outputs.Count];

			for (int i = 0; i < VideoDevice.Outputs.Count; i++)
			{
				_presentParams[i] = new PresentParameters();

				_presentParams[i].AutoDepthStencilFormat = D3DConvert.Convert(DepthStencilFormat, false);
				_presentParams[i].BackBufferCount = 3;
				_presentParams[i].BackBufferFormat = D3DConvert.GetDisplayFormat(Mode.Format, !IsWindowed);
				_presentParams[i].BackBufferHeight = Mode.Height;
				_presentParams[i].BackBufferWidth = Mode.Width;
				if (isMultiHead)
					_presentParams[i].DeviceWindowHandle = IntPtr.Zero;
				else
					_presentParams[i].DeviceWindowHandle = BoundWindow.Handle;
				if (DepthStencilFormat != GorgonBufferFormat.Unknown)
					_presentParams[i].EnableAutoDepthStencil = true;
				else
					_presentParams[i].EnableAutoDepthStencil = false;
				if (!IsWindowed)
					_presentParams[i].FullScreenRefreshRateInHertz = Mode.RefreshRateNumerator;
				_presentParams[i].Multisample = MultisampleType.None;
				_presentParams[i].MultisampleQuality = 0;
				_presentParams[i].PresentationInterval = PresentInterval.Immediate;
				_presentParams[i].PresentFlags = PresentFlags.None;
				_presentParams[i].SwapEffect = SwapEffect.Discard;
				_presentParams[i].Windowed = IsWindowed;
			}
		}

		/// <summary>
		/// Function to create the render target.
		/// </summary>
		protected override void CreateRenderTarget()
		{
			CreateFlags flags = CreateFlags.Multithreaded | CreateFlags.HardwareVertexProcessing | CreateFlags.FpuPreserve;
			bool isMultiHead = (((D3D9VideoOutput)VideoOutput).HeadIndex > 0);

			SetPresentationParameters(isMultiHead);

			D3DDevice = new Device(Graphics.D3D, ((D3D9VideoDevice)VideoDevice).AdapterIndex, Graphics.DeviceType, ParentWindow.Handle, flags | CreateFlags.AdapterGroupDevice, _presentParams);			
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
					if (D3DDevice != null)
						D3DDevice.Dispose();
					D3DDevice = null;
				}

				_disposed = true;
			}
		}
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
			: base(name, device, output, window, mode, depthStencilFormat, fullScreen)
		{
			Graphics = graphics;
		}
		#endregion
	}
}
