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

namespace GorgonLibrary.Graphics
{
	/// <summary>
	///  A swap chain render target that can be attached to a window.
	/// </summary>
	public abstract class GorgonSwapChain
		: GorgonSwapChainBase
	{
		#region Properties.
		/// <summary>
		/// Property to return the device window that created this swap chain.
		/// </summary>
		public GorgonDeviceWindow DeviceWindow
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		
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
	}
}
