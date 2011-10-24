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
// Created: Thursday, July 21, 2011 3:15:20 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Native;
using GorgonLibrary.Collections;
using GI = SlimDX.DXGI;
using D3D = SlimDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A collection of video devices.
	/// </summary>
	public class GorgonVideoDeviceCollection
		: GorgonBaseNamedObjectCollection<GorgonVideoDevice>
	{
		#region Constants.
		#endregion

		#region Variables.
		private GorgonGraphics _graphics = null;		// Graphics interface.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a video device by its index.
		/// </summary>
		public GorgonVideoDevice this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return a video device by its name.
		/// </summary>
		public GorgonVideoDevice this[string name]
		{
			get
			{
				return GetItem(name);
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		public override bool IsReadOnly
		{
			get
			{
				return true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clear the items from the collection.
		/// </summary>
		protected override void ClearItems()
		{
			foreach (var item in this)
				((IDisposable)item).Dispose();

			base.ClearItems();
		}

		/// <summary>
		/// Function to clear the collection.
		/// </summary>
		internal void Clear()
		{
			ClearItems();
		}

		/// <summary>
		/// Function to enumerate the video devices attached to the computer.
		/// </summary>
		public void Refresh()
		{
			ClearItems();
			
			int adapterCount = _graphics.GIFactory.GetAdapterCount1();

			Gorgon.Log.Print("Enumerating video devices...", Diagnostics.GorgonLoggingLevel.Simple);

			for (int i = 0; i < adapterCount; i++)
			{
				Gorgon.Log.Print("Creating DXGI adapter interface...", Diagnostics.GorgonLoggingLevel.Verbose);
				GI.Adapter1 adapter = _graphics.GIFactory.GetAdapter1(i);

				// Only use local adapters.
				if (adapter.Description1.Flags != GI.AdapterFlags.Remote)
				{
					GorgonVideoDevice device = new GorgonVideoDevice(adapter);

					if ((device.SupportsFeatureLevels(_graphics.MaxFeatureLevel)) && (device.HardwareFeatureLevels != DeviceFeatureLevel.Unsupported))
					{
						this.AddItem(device);

						// Get an instance of the Direct 3D device object.
						device.GetDevice(_graphics.MaxFeatureLevel);

						Gorgon.Log.Print("Device found: {0}", Diagnostics.GorgonLoggingLevel.Simple ,device.Name);
						Gorgon.Log.Print("===================================================================", Diagnostics.GorgonLoggingLevel.Verbose);
						Gorgon.Log.Print("Supports feature level: {0}", Diagnostics.GorgonLoggingLevel.Verbose, device.HardwareFeatureLevels);
						Gorgon.Log.Print("Limited to feature level: {0}", Diagnostics.GorgonLoggingLevel.Verbose, device.SupportedFeatureLevels);
						Gorgon.Log.Print("Video memory: {0}", Diagnostics.GorgonLoggingLevel.Verbose, device.DedicatedVideoMemory.FormatMemory());
						Gorgon.Log.Print("System memory: {0}", Diagnostics.GorgonLoggingLevel.Verbose, device.DedicatedSystemMemory.FormatMemory());
						Gorgon.Log.Print("Shared memory: {0}", Diagnostics.GorgonLoggingLevel.Verbose, device.SharedSystemMemory.FormatMemory());
						Gorgon.Log.Print("Device ID: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, device.DeviceID.FormatHex());
						Gorgon.Log.Print("Sub-system ID: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, device.SubSystemID.FormatHex());
						Gorgon.Log.Print("Vendor ID: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, device.VendorID.FormatHex());
						Gorgon.Log.Print("Revision: {0}", Diagnostics.GorgonLoggingLevel.Verbose, device.Revision);
						Gorgon.Log.Print("Unique ID: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, device.UUID.FormatHex());
						Gorgon.Log.Print("===================================================================", Diagnostics.GorgonLoggingLevel.Verbose);

						// Get the outputs for the device.
						device.Outputs.Refresh();
					}						
				}
			}

			Gorgon.Log.Print("Found {0} video devices.", Diagnostics.GorgonLoggingLevel.Simple, Count);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoDeviceCollection"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface to use.</param>
		internal GorgonVideoDeviceCollection(GorgonGraphics graphics)
			: base(false)
		{
			if (graphics == null)
				throw new ArgumentNullException("graphics");

			_graphics = graphics;
			Refresh();
		}
		#endregion
	}
}
