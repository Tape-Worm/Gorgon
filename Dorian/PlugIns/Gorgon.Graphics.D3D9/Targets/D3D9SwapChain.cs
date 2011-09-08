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
// Created: Saturday, September 03, 2011 7:01:51 PM
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
	/// A direct 3D 9 swap chain interface.
	/// </summary>
	class D3D9SwapChain
		: GorgonSwapChain, IUnmanagedObject
	{
		#region Variables.
		private bool _disposed = false;						// Flag to indicate that the object was disposed.
		private GorgonD3D9Graphics _graphics = null;		// Graphics interface.
		private D3D9DeviceWindow _deviceWindow = null;		// Device window parent.
		private PresentParameters _presentParams = null;	// Presentation parameters.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the D3D swap chain.
		/// </summary>
		public SwapChain D3DSwapChain
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to release the swap chain data.
		/// </summary>
		private void DestroySwapchainData()
		{
			if (D3DSwapChain != null)
				D3DSwapChain.Dispose();

			if (Surface != null)
				Surface.Dispose();
			if (DepthStencilSurface != null)
				DepthStencilSurface.Dispose();

			D3DSwapChain = null;
		}

		/// <summary>
		/// Function to set the presentation parameters for the device window.
		/// </summary>
		private void SetPresentationParameters()
		{
			_presentParams = D3DConvert.Convert(Settings);
			_presentParams.Windowed = true;

			Gorgon.Log.Print("Direct3D presentation parameters:", Diagnostics.GorgonLoggingLevel.Verbose);
			Gorgon.Log.Print("\tAutoDepthStencilFormat: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams.AutoDepthStencilFormat);
			Gorgon.Log.Print("\tBackBufferCount: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams.BackBufferCount);
			Gorgon.Log.Print("\tBackBufferFormat: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams.BackBufferFormat);
			Gorgon.Log.Print("\tBackBufferWidth: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams.BackBufferWidth);
			Gorgon.Log.Print("\tBackBufferHeight: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams.BackBufferHeight);
			Gorgon.Log.Print("\tDeviceWindowHandle: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams.DeviceWindowHandle.FormatHex());
			Gorgon.Log.Print("\tEnableAutoDepthStencil: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams.EnableAutoDepthStencil);
			Gorgon.Log.Print("\tFullScreenRefreshRateInHertz: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams.FullScreenRefreshRateInHertz);
			Gorgon.Log.Print("\tMultisample: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams.Multisample);
			Gorgon.Log.Print("\tMultisampleQuality: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams.MultisampleQuality);
			Gorgon.Log.Print("\tPresentationInterval: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams.PresentationInterval);
			Gorgon.Log.Print("\tPresentFlags: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams.PresentFlags);
			Gorgon.Log.Print("\tSwapEffect: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams.SwapEffect);
			Gorgon.Log.Print("\tWindowed: {0}", Diagnostics.GorgonLoggingLevel.Verbose, _presentParams.Windowed);
		}

		/// <summary>
		/// Cleans up resources.
		/// </summary>
		protected override void CleanUpResources()
		{
			base.CleanUpResources();

			DestroySwapchainData();
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
					_deviceWindow.UnmanagedObjects.Remove(this);
					CleanUpResources();
				}
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to perform an update on the resources required by the render target.
		/// </summary>
		protected override void UpdateResources()
		{
			DestroySwapchainData();
			CreateResources();
		}

		/// <summary>
		/// Function to create any resources required by the render target.
		/// </summary>
		protected override void CreateResources()
		{
			SetPresentationParameters();
			D3DSwapChain = new SwapChain(_deviceWindow.D3DDevice, _presentParams);
			Surface = new D3D9Surface(Name + "_SwapBufferSurface", _deviceWindow, D3DSwapChain.GetBackBuffer(0));
			if (Settings.DepthStencilFormat != GorgonBufferFormat.Unknown)
			{
				Surface depthStencil = SlimDX.Direct3D9.Surface.CreateDepthStencil(_deviceWindow.D3DDevice, Settings.Width, Settings.Height, D3DConvert.Convert(Settings.DepthStencilFormat), D3DConvert.Convert(Settings.MSAAQualityLevel.Level), (Settings.MSAAQualityLevel.Quality > 0 ? Settings.MSAAQualityLevel.Quality - 1 : 0), false);
				DepthStencilSurface = new D3D9Surface(Name + "_SwapDepthStencilSurface", _deviceWindow, depthStencil);
			}
		}

		/// <summary>
		/// Function to clear a target.
		/// </summary>
		/// <param name="color">Color to clear with.</param>
		/// <param name="depthValue">Depth buffer value to clear with.</param>
		/// <param name="stencilValue">Stencil value to clear with.</param>
		public override void Clear(GorgonColor? color, float? depthValue, int? stencilValue)
		{
			GorgonRenderTarget previousTarget = DeviceWindow.CurrentTarget;

			if (previousTarget != this)
				DeviceWindow.CurrentTarget = this;

			_deviceWindow.ClearTarget(color, depthValue, stencilValue);

			if (previousTarget != this)
				DeviceWindow.CurrentTarget = previousTarget;
		}

		/// <summary>
		/// Function to display the contents of the swap chain.
		/// </summary>
		public override void Display()
		{
			if ((IsReady) && (D3DSwapChain != null))
				D3DSwapChain.Present(Present.None);
		}

		#region Delete this shit.
		/// <summary>
		/// </summary>
		public override void SetupTest()
		{
			
		}

		/// <summary>
		/// </summary>
		/// <param name="dt"></param>
		public override void RunTest(float dt)
		{
			_deviceWindow._test.Run(dt, Settings);
		}

		/// <summary>
		/// </summary>
		public override void CleanUpTest()
		{
			
		}
		#endregion
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="D3D9SwapChain"/> class.
		/// </summary>
		/// <param name="graphics">The graphics instance that owns this swap chain.</param>
		/// <param name="deviceWindow">The device window that created this swap chain.</param>
		/// <param name="name">The name.</param>
		/// <param name="settings">Swap chain settings.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		///   <para>-or-</para>
		///   <para>Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</para>
		///   <para>-or-</para>
		///   <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.BoundWindow">settings.BoundWindow</see> parameter is NULL (Nothing in VB.Net).</para>
		///   <para>-or-</para>
		///   <para>Thrown when the <paramref name="deviceWindow"/> parameter is NULL (Nothing in VB.Net).</para>
		///   </exception>
		///   
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.
		///   </exception>
		public D3D9SwapChain(GorgonD3D9Graphics graphics, D3D9DeviceWindow deviceWindow, string name, GorgonSwapChainSettings settings)
			: base(graphics, deviceWindow, name, settings)
		{
			_graphics = graphics;
			_deviceWindow = deviceWindow;
			_deviceWindow.UnmanagedObjects.Add(this);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="D3D9SwapChain"/> class.
		/// </summary>
		/// <param name="graphics">The graphics instance that owns this swap chain.</param>
		/// <param name="deviceWindow">The device window that created this swap chain.</param>
		/// <param name="name">The name.</param>
		/// <param name="settings">Swap chain settings.</param>
		/// <param name="swapChain">Previous instanced swap chain to wrap.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		///   <para>-or-</para>
		///   <para>Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</para>
		///   <para>-or-</para>
		///   <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.BoundWindow">settings.BoundWindow</see> parameter is NULL (Nothing in VB.Net).</para>
		///   <para>-or-</para>
		///   <para>Thrown when the <paramref name="deviceWindow"/> parameter is NULL (Nothing in VB.Net).</para>
		///   </exception>
		///   
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.
		///   </exception>
		public D3D9SwapChain(GorgonD3D9Graphics graphics, D3D9DeviceWindow deviceWindow, string name, GorgonSwapChainSettings settings, SwapChain swapChain)
			: base(graphics, deviceWindow, name, settings)
		{
			SlimDX.Direct3D9.Surface backBuffer = null;
			SlimDX.Direct3D9.Surface depthBuffer = null;

			// Get and create the back buffers.
			backBuffer = swapChain.GetBackBuffer(0);
			if (settings.DepthStencilFormat != GorgonBufferFormat.Unknown)
			{
				depthBuffer = SlimDX.Direct3D9.Surface.CreateDepthStencil(deviceWindow.D3DDevice,
					settings.Width,
					settings.Height,
					D3DConvert.Convert(settings.DepthStencilFormat),
					D3DConvert.Convert(settings.MSAAQualityLevel.Level),
					(settings.MSAAQualityLevel.Level != GorgonMSAALevel.None ? settings.MSAAQualityLevel.Quality - 1 : 0),
					false
				);
			}

			_graphics = graphics;
			_deviceWindow = deviceWindow;
			D3DSwapChain = swapChain;
			Surface = new D3D9Surface(name + "_BackBufferSurface", deviceWindow, backBuffer);
			if (settings.DepthStencilFormat != GorgonBufferFormat.Unknown)
				DepthStencilSurface = new D3D9Surface(name + "_DepthStencilSurface", deviceWindow, depthBuffer);
		}
		#endregion

		#region IUnmanagedObject Members
		/// <summary>
		/// Function called when a device is placed in a lost state.
		/// </summary>
		public void DeviceLost()
		{
			DestroySwapchainData();
		}

		/// <summary>
		/// Function called when a device is reset from a lost state.
		/// </summary>
		public void DeviceReset()
		{
			UpdateResources();
		}
		#endregion
	}
}
