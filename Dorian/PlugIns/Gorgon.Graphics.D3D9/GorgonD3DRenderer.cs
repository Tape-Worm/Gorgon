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
// Created: Tuesday, July 19, 2011 5:09:59 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using Microsoft.Win32;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// The interface for the Direct 3D9 renderer.
	/// </summary>
	class GorgonD3D9Renderer
		: GorgonRenderer
	{
		#region Constants.
		/// <summary>
		/// Key for the reference rasterizer setting.
		/// </summary>
		public const string RefRastKey = "UseReferenceRasterizer";
		#endregion

		#region Variables.
		/// <summary>
		/// Property to return the primary Direct 3D interface.
		/// </summary>
		public Direct3D D3D
		{
			get;
			private set;
		}
		#endregion

		#region Properties.

		#endregion

		#region Methods.
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
		/// Function to return a list of driver capabilities for a given driver.
		/// </summary>
		/// <param name="index">Index of the driver to evaluate.</param>
		/// <returns>
		/// An enumerable list of driver capabilities.
		/// </returns>
		protected override IEnumerable<KeyValuePair<string, string>> GetDriverCapabilities(int index)
		{
			Dictionary<string, string> driverCaps = new Dictionary<string, string>();



			return driverCaps;
		}

		/// <summary>
		/// Function to perform the actual initialization for the renderer from a plug-in.
		/// </summary>
		protected override void InitializeRenderer()
		{
#if DEBUG
			Configuration.EnableObjectTracking = true;
#else
            Configuration.EnableObjectTracking = false;
#endif
			// We don't need exceptions with these errors.
			Configuration.AddResultWatch(ResultCode.DeviceLost, SlimDX.ResultWatchFlags.AlwaysIgnore);
			Configuration.AddResultWatch(ResultCode.DeviceNotReset, SlimDX.ResultWatchFlags.AlwaysIgnore);

			D3D = new Direct3D();
			D3D.CheckWhql = false;

			// Log any performance warnings.
			if (IsD3DDebug())
				Gorgon.Log.Print("[*WARNING*] The Direct 3D runtime is currently set to DEBUG mode.  Performance will be hindered. [*WARNING*]", Diagnostics.GorgonLoggingLevel.Intermediate);
		}

		/// <summary>
		/// Function to perform the actual shut down for the renderer from a plug-in.
		/// </summary>
		protected override void ShutdownRenderer()
		{
			if (D3D != null)
				D3D.Dispose();
			D3D = null;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonD3D9Renderer"/> class.
		/// </summary>
		internal GorgonD3D9Renderer()
			: base("Gorgon Direct 3D® 9 Renderer")
		{
			CustomSettings[RefRastKey] = "0";
			// TODO: Move this to where it belongs, like in device creation or some such.
/*			if (CustomSettings[RefRastKey] != "0")
				Gorgon.Log.Print("[*WARNING*] The D3D device will be a REFERENCE device.  Performance will be greatly hindered. [*WARNING*]", Diagnostics.GorgonLoggingLevel.All);*/
		}
		#endregion
	}
}
