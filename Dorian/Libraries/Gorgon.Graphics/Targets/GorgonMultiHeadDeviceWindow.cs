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
// Created: Wednesday, August 31, 2011 9:48:00 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A specialized device window for handling multi-head device windows.
	/// </summary>
	/// <remarks>Multi-head device windows are only for multi-head video devices.  They create multiple swap chains for the same device and can drive fullscreen presentation for multiple heads while sharing data.
	/// This is different from a <see cref="GorgonLibrary.Graphics.GorgonDeviceWindow">GorgonDeviceWindow</see> in that the device windows cannot share data automatically on multiple monitor systems.
	/// </remarks>
	public abstract  class GorgonMultiHeadDeviceWindow
		: GorgonDeviceWindow
	{
		#region Variables.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the number of heads being utilized.
		/// </summary>
		public int HeadCount
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return a list of settings for each head.
		/// </summary>
		public new GorgonMultiHeadSettings Settings
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the device window.
		/// </summary>
		/// <remarks>Use this method to apply changes the <see cref="GorgonLibrary.Graphics.GorgonDeviceWindowSettings">dimensions, format, fullscreen/windowed state and depth information</see> for the device window.
		/// <para>The <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateNominator">RefreshRateNominator</see> and the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateDenominator">RefreshRateDenominator</see>
		/// of the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindow.Settings">Settings</see> property are not relevant when in windowed mode.</para>
		/// 	<para>Device windows bound to child controls or device windows with extra <see cref="GorgonLibrary.Graphics.GorgonSwapChain">swap chains</see> attached to them cannot go full screen, setting the <see cref="P:GorgonDeviceWindowSettings.Windowed"/> setting to TRUE will throw an exception.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentException">Thrown when the window is a child control, or when there are extra swap chains belonging to this device window and setting the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.IsWindowed">GorgonDeviceWindowSettings.IsWindowed</see> property to FALSE.
		/// </exception>
		public override void UpdateSettings()
		{
			// Force to full screen.
			foreach (var setting in Settings.Settings)
				setting.IsWindowed = false;

			// What here?
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMultiHeadDeviceWindow"/> class.
		/// </summary>
		/// <param name="graphics">The graphics instance that owns this render target.</param>
		/// <param name="name">The name.</param>
		/// <param name="settings">Device window settings.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// 	<para>Thrown when the <paramref name="device"/> and <paramref name="output"/> parameters are NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		/// <remarks>A fullscreen video mode must have a a Windows Form object as the bound window.
		/// <para>The <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateNominator">RefreshRateNominator</see> and the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateDenominator">RefreshRateDenominator</see>
		/// of the <see cref="GorgonLibrary.Graphics.GorgonVideoMode">GorgonVideoMode</see> type are not relevant when fullScreen is set to FALSE.</para>
		/// </remarks>
		protected GorgonMultiHeadDeviceWindow(GorgonGraphics graphics, string name, GorgonMultiHeadSettings multiHeadSettings)
			: base(graphics, name, multiHeadSettings.Settings.ElementAt(0))
		{
			HeadCount = multiHeadSettings.Settings.Count();
			Settings = multiHeadSettings;			
		}
		#endregion
	}
}
