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
// Created: Wednesday, July 27, 2011 5:21:59 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using SlimDX;
using SlimDX.Direct3D9;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// The Direct 3D9 graphics interface.
	/// </summary>
	internal class GorgonD3D9Graphics
		: GorgonGraphics
	{
		#region Constants.
		/// <summary>
		/// Key for the reference rasterizer setting.
		/// </summary>
		public const string RefRastKey = "UseReferenceRasterizer";
		#endregion

		#region Variables.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the D3D runtime is in debug mode or not.
		/// </summary>
		/// <returns>TRUE if in debug, FALSE if in retail.</returns>
		private bool IsD3DDebug()
		{
			RegistryKey regKey = null;		// Registry key.
			int keyValue = 0;				// Registry key value.

			try
			{
				// Get the registry setting.
				regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Direct3D");

				if (regKey == null)
					return false;

				// Get value.
				keyValue = Convert.ToInt32(regKey.GetValue("LoadDebugRuntime", (int)0));
				if (keyValue != 0)
					return true;
			}
#if DEBUG
			catch (Exception ex)
#else
			catch
#endif
			{
#if DEBUG
				Gorgon.Log.Print("Exception retrieving registry settings for D3D debug mode:", Diagnostics.GorgonLoggingLevel.Intermediate);
				Gorgon.Log.Print("{0}", Diagnostics.GorgonLoggingLevel.Intermediate, ex.Message);
#endif
			}
			finally
			{
				if (regKey != null)
					regKey.Close();
				regKey = null;
			}

			return false;
		}

		/// <summary>
		/// Property to return whether we're using the reference rasterizer.
		/// </summary>
		public DeviceType DeviceType
		{
			get
			{
				if (string.Compare(CustomSettings[RefRastKey], "0", true) != 0)
					return DeviceType.Reference;
				else
					return DeviceType.Hardware;
			}
		}

		/// <summary>
		/// Property to return the primary Direct 3D interface.
		/// </summary>
		public Direct3D D3D
		{
			get;
			private set;
		}	
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the video device and output that we're currently using.
		/// </summary>
		/// <param name="window">Window to use.</param>
		/// <param name="fullScreen">TRUE if we're going full screen, FALSE if not.</param>
		/// <returns>The video device and output that we're using.</returns>
		private Tuple<GorgonVideoDevice, GorgonVideoOutput> GetDevice(Control window, bool fullScreen)
		{
			IntPtr monitor = Win32API.GetMonitor(window);

			return (from device in VideoDevices						
					where ((D3D9VideoOutput)device.Outputs[0]).Handle == monitor
					select new Tuple<GorgonVideoDevice, GorgonVideoOutput>(device, device.Outputs[0])).Single();
		}

		/// <summary>
		/// Function to create a device window in back end API.
		/// </summary>
		/// <param name="name">Name of the window.</param>
		/// <param name="window">Window to bind to the device window.</param>
		/// <param name="mode">Video mode to set.</param>
		/// <param name="depthStencilFormat">Format for the depth/stencil buffer.</param>
		/// <param name="fullScreen">TRUE to use fullscreen mode, FALSE to use windowed.</param>
		/// <returns>
		/// A device window.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		///   <para>-or-</para>
		///   <para>Thrown when the <paramref name="window"/> parameter is NULL (Nothing in VB.Net).</para>
		///   </exception>
		/// <exception cref="System.ArgumentException">Thrown if the name parameter is an empty string.
		///   <para>-or-</para>
		///   <para>Thrown if the <paramref name="mode"/> is a video mode that cannot be used.</para>
		///   </exception>
		protected override GorgonDeviceWindow CreateDeviceWindowImpl(string name, System.Windows.Forms.Control window, GorgonVideoMode mode, GorgonBufferFormat depthStencilFormat, bool fullScreen)
		{
			Tuple<GorgonVideoDevice, GorgonVideoOutput> deviceOutput = GetDevice(window, fullScreen);
			return new D3D9DeviceWindow(this, name, deviceOutput.Item1, deviceOutput.Item2, window, mode, depthStencilFormat, fullScreen);
		}

		/// <summary>
		/// Function to initialize the graphics interface.
		/// </summary>
		protected override void InitializeGraphics()
		{

#if DEBUG
			Configuration.EnableObjectTracking = true;
#else
            Configuration.EnableObjectTracking = false;
#endif
			// We don't need exceptions with these errors.
			Configuration.AddResultWatch(ResultCode.DeviceLost, SlimDX.ResultWatchFlags.AlwaysIgnore);
			Configuration.AddResultWatch(ResultCode.DeviceNotReset, SlimDX.ResultWatchFlags.AlwaysIgnore);

			Gorgon.Log.Print("Creating IDirect3D9 interface.", Diagnostics.GorgonLoggingLevel.Verbose);
			D3D = new Direct3D();
			D3D.CheckWhql = false;

			// Log any performance warnings.
			if (IsD3DDebug())
				Gorgon.Log.Print("[*WARNING*] The Direct 3D runtime is currently set to DEBUG mode.  Performance will be hindered. [*WARNING*]", Diagnostics.GorgonLoggingLevel.Intermediate);
		}

		/// <summary>
		/// Function to create the renderer interface.
		/// </summary>
		/// <returns>
		/// The renderer interface.
		/// </returns>
		protected override GorgonRenderer CreateRenderer()
		{
			return new GorgonD3D9Renderer(this);
		}

		/// <summary>
		/// Function to perform clean up in the plug-in.
		/// </summary>
		protected override void CleanUpGraphics()
		{
			Gorgon.Log.Print("Destroying IDirect3D9 interface.", Diagnostics.GorgonLoggingLevel.Verbose);
			if (D3D != null)
				D3D.Dispose();
			D3D = null;
		}

		/// <summary>
		/// Function to return a list of all video devices installed on the system.
		/// </summary>
		/// <returns>
		/// An enumerable list of video devices.
		/// </returns>
		protected override IEnumerable<KeyValuePair<string, GorgonVideoDevice>> GetVideoDevices()
		{
			Version minShaderModelVersion = new Version(3, 0);
			IDictionary<string, GorgonVideoDevice> devices = null;
			bool isHardWare = false;
			string deviceName = string.Empty;

			devices = new Dictionary<string, GorgonVideoDevice>();

			if (CustomSettings[RefRastKey] != "0")
				Gorgon.Log.Print("[*WARNING*] The D3D device will be a REFERENCE device.  Performance will be greatly hindered. [*WARNING*]", Diagnostics.GorgonLoggingLevel.All);

			for (int i = 0; i < D3D.Adapters.Count; i++)
			{
				// Get device capabilities.
				Capabilities caps = D3D.GetDeviceCaps(i, DeviceType);

				if (DeviceType == SlimDX.Direct3D9.DeviceType.Reference)
					isHardWare = true;
				else
					isHardWare = (((caps.DeviceCaps & DeviceCaps.HWRasterization) == DeviceCaps.HWRasterization) && ((caps.DeviceCaps & DeviceCaps.HWTransformAndLight) == DeviceCaps.HWTransformAndLight));

				if ((isHardWare) && (caps.PixelShaderVersion == minShaderModelVersion) && (caps.VertexShaderVersion == minShaderModelVersion))
				{
					int deviceCount = 1;
					deviceName = D3D.Adapters[i].Details.Description.Trim();

					// Check for an existing name (for dual head devices).
					while (devices.ContainsKey(deviceName.ToLower()))
					{
						// If we're only at one device, then use the name + device name, otherwise, include the count.
						if (deviceCount == 1)
							deviceName = deviceName + " [" + D3D.Adapters[i].Details.DeviceName + "]";
						else
							deviceName = deviceName + " #" + deviceCount.ToString() + " [" + D3D.Adapters[i].Details.DeviceName + "]";
						deviceCount++;
					}

					D3D9VideoDevice device = new D3D9VideoDevice(deviceName, devices.Count, D3D, DeviceType, D3D.Adapters[i], caps);
					devices.Add(deviceName.ToLower(), device);
				}
			}

			return devices;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonD3D9Graphics"/> class.
		/// </summary>
		public GorgonD3D9Graphics()	
			: base("Gorgon Direct3D 9® Graphics")
		{
			CustomSettings[RefRastKey] = "0";
		}
		#endregion
	}
}
