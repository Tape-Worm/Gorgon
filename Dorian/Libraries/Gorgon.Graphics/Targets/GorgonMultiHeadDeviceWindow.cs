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
using System.Windows.Forms;

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
		private int _currentHead = 0;										// Current head.
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

		/// <summary>
		/// Property to set or return which head is active for rendering.
		/// </summary>
		public int CurrentHead
		{
			get
			{
				return _currentHead;
			}
			set
			{
				if (value < 0)
					value = 0;
				if (value >= HeadCount)
					value = HeadCount - 1;

				_currentHead = value;
				SetCurrentHead(value);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set the currently active head for rendering.
		/// </summary>
		/// <param name="headIndex">Index of the head.</param>
		protected abstract void SetCurrentHead(int headIndex);

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
			int headIndex = 0;

			// Force to full screen.
			foreach (var setting in Settings.Settings)
				setting.IsWindowed = false;
						
			UpdateResources();

			foreach (var setting in Settings.Settings)
			{
				Gorgon.Log.Print("Updating multi-head device window '{0}' on head {6} with settings: {1}x{2} Format: {3} Refresh Rate: {4}/{5}.", Diagnostics.GorgonLoggingLevel.Verbose, Name, setting.DisplayMode.Width, setting.DisplayMode.Height, setting.DisplayMode.Format, setting.DisplayMode.RefreshRateNumerator, setting.DisplayMode.RefreshRateDenominator, headIndex);
				Gorgon.Log.Print("'{0}' information:", Diagnostics.GorgonLoggingLevel.Verbose, Name);
				Gorgon.Log.Print("\tLayout: {0}x{1} Format: {2} Refresh Rate: {3}/{4}", Diagnostics.GorgonLoggingLevel.Verbose, setting.DisplayMode.Width, setting.DisplayMode.Height, setting.DisplayMode.Format, setting.DisplayMode.RefreshRateNumerator, setting.DisplayMode.RefreshRateDenominator);
				Gorgon.Log.Print("\tDepth/Stencil: {0} (Format: {1})", Diagnostics.GorgonLoggingLevel.Verbose, setting.DepthStencilFormat != GorgonBufferFormat.Unknown, setting.DepthStencilFormat);
				Gorgon.Log.Print("\tWindowed: {0}", Diagnostics.GorgonLoggingLevel.Verbose, setting.IsWindowed);
				Gorgon.Log.Print("\tMSAA: {0}", Diagnostics.GorgonLoggingLevel.Verbose, setting.MSAAQualityLevel.Level != GorgonMSAALevel.None);
				if (setting.MSAAQualityLevel.Level != GorgonMSAALevel.None)
					Gorgon.Log.Print("\t\tMSAA Quality: {0}  Level: {1}", Diagnostics.GorgonLoggingLevel.Verbose, setting.MSAAQualityLevel.Quality, setting.MSAAQualityLevel.Level);
				Gorgon.Log.Print("\tBackbuffer Count: {0}", Diagnostics.GorgonLoggingLevel.Verbose, setting.BackBufferCount);
				Gorgon.Log.Print("\tDisplay Function: {0}", Diagnostics.GorgonLoggingLevel.Verbose, setting.DisplayFunction);
				Gorgon.Log.Print("\tV-Sync interval: {0}", Diagnostics.GorgonLoggingLevel.Verbose, setting.VSyncInterval);
				Gorgon.Log.Print("\tVideo surface: {0}", Diagnostics.GorgonLoggingLevel.Verbose, setting.WillUseVideo);

				headIndex++;
			}
			Gorgon.Log.Print("Device window '{0}' updated.", Diagnostics.GorgonLoggingLevel.Simple, Name);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMultiHeadDeviceWindow"/> class.
		/// </summary>
		/// <param name="graphics">The graphics instance that owns this render target.</param>
		/// <param name="name">The name.</param>
		/// <param name="multiHeadSettings">Device window settings.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// 	<para>Thrown when the <paramref name="multiHeadSettings"/> parameter is NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		/// <remarks>A fullscreen video mode must have a a Windows Form object as the bound window.
		/// <para>The <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateNominator">RefreshRateNominator</see> and the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateDenominator">RefreshRateDenominator</see>
		/// of the <see cref="GorgonLibrary.Graphics.GorgonVideoMode">GorgonVideoMode</see> type are not relevant when fullScreen is set to FALSE.</para>
		/// </remarks>
		protected GorgonMultiHeadDeviceWindow(GorgonGraphics graphics, string name, GorgonMultiHeadSettings multiHeadSettings)
			: base(graphics, name, multiHeadSettings.Settings[0])
		{
			HeadCount = multiHeadSettings.Settings.Count();
			Settings = multiHeadSettings;			
		}
		#endregion
	}
}
