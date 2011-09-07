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
// Created: Thursday, August 04, 2011 8:20:02 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	///  A swap chain render target that can be attached to a window.
	/// </summary>
	public abstract class GorgonSwapChain
		: GorgonWindowTarget<GorgonSwapChainSettings>, IDeviceWindowChild
	{
		#region Variables.
		private bool _disposed = false;			// Flag to indicate that the object is disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the target has a depth buffer attached to it.
		/// </summary>
		/// <value></value>
		public override bool HasDepthBuffer
		{
			get 
			{
				switch (Settings.DepthStencilFormat)
				{
					case GorgonBufferFormat.D24_Float_S8_UInt:
					case GorgonBufferFormat.D24_UIntNormal_X4S4_UInt:
					case GorgonBufferFormat.D15_UIntNormal_S1_UInt:
					case GorgonBufferFormat.D24_UIntNormal_S8_UInt:
					case GorgonBufferFormat.D32_Float:
					case GorgonBufferFormat.D32_UIntNormal:
					case GorgonBufferFormat.D32_Float_Lockable:
					case GorgonBufferFormat.D24_UIntNormal_X8:
					case GorgonBufferFormat.D16_UIntNormal_Lockable:
					case GorgonBufferFormat.D16_UIntNormal:
						return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Property to return whether the target has a stencil buffer attaached to it.
		/// </summary>
		/// <value></value>
		public override bool HasStencilBuffer
		{
			get 
			{
				switch (Settings.DepthStencilFormat)
				{
					case GorgonBufferFormat.D32_Float_S8X24_UInt:
					case GorgonBufferFormat.D24_Float_S8_UInt:
					case GorgonBufferFormat.D24_UIntNormal_X4S4_UInt:
					case GorgonBufferFormat.D15_UIntNormal_S1_UInt:
					case GorgonBufferFormat.D24_UIntNormal_S8_UInt:
						return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Property to return whether the device window is ready for rendering.
		/// </summary>
		public override bool IsReady
		{
			get 
			{
				return DeviceWindow.IsReady;
			}
		}
		#endregion

		#region Methods.
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
					Gorgon.Log.Print("Swap chain {0} destroyed.", GorgonLoggingLevel.Simple, Name);
				}
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to update the render target after changes have been made to its settings.
		/// </summary>
		public override void UpdateSettings()
		{
			DeviceWindow.ValidateSwapChainSettings(Settings);
			UpdateResources();
		}		
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSwapChain"/> class.
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
		protected GorgonSwapChain(GorgonGraphics graphics, GorgonDeviceWindow deviceWindow, string name, GorgonSwapChainSettings settings)
			: base(graphics, name, settings)
		{
			if (deviceWindow == null)
				throw new ArgumentNullException("deviceWindow");

			DeviceWindow = deviceWindow;
		}
		#endregion

		#region IDeviceWindowChild Members
		/// <summary>
		/// Property to return the device window that created this swap chain.
		/// </summary>
		public GorgonDeviceWindow DeviceWindow
		{
			get;
			private set;
		}
		#endregion
	}
}
