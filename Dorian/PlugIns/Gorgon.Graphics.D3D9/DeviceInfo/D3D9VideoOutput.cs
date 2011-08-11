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
// Created: Sunday, July 24, 2011 10:02:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// A direct 3D9 video output.
	/// </summary>
	internal class D3D9VideoOutput
		: GorgonVideoOutput
	{
		#region Variables.
		private AdapterInformation _info = null;				// Adapter information.
		private Direct3D _d3d = null;							// Direct 3D interface.
		private DeviceType _deviceType = DeviceType.Hardware;	// Device type.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the handle for the monitor attached to this output.
		/// </summary>
		public IntPtr Handle
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the adapter group index for this output.
		/// </summary>
		public int HeadIndex
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the video modes for this output.
		/// </summary>
		/// <returns>
		/// An enumerable list of video modes.
		/// </returns>
		protected override IEnumerable<GorgonVideoMode> GetVideoModes()
		{			
			Format[] displayModeFormats = {Format.A2R10G10B10, Format.A8R8G8B8, Format.X8R8G8B8};
			List<GorgonVideoMode> modes = new List<GorgonVideoMode>();

			foreach (var d3dformat in displayModeFormats)
			{
				DisplayModeCollection d3dModes = _info.GetDisplayModes(d3dformat);

				foreach (var d3dMode in d3dModes)
					modes.Add(D3DConvert.Convert(d3dMode));
			}

			return modes;
		}

		/// <summary>
		/// Function to determine if a specified display format is supported by the hardware or not.
		/// </summary>
		/// <param name="format">Backbuffer format to check.</param>
		/// <param name="isWindowed">TRUE if in windowed mode, FALSE if not.</param>
		/// <returns>TRUE if supported, FALSE if not.</returns>
		public override bool SupportsBackBufferFormat(GorgonBackBufferFormat format, bool isWindowed)
		{
			if (isWindowed)
				return _d3d.CheckDeviceType(_info.Adapter, _deviceType, D3DConvert.ConvertBackBufferFormat(DefaultVideoMode.Format), D3DConvert.ConvertBackBufferFormat(format), isWindowed);
			else
				return _d3d.CheckDeviceType(_info.Adapter, _deviceType, D3DConvert.ConvertBackBufferFormat(format), D3DConvert.ConvertBackBufferFormat(format), isWindowed);
		}

		/// <summary>
		/// Function to determine if a depth/stencil format can be used with a specific display format.
		/// </summary>
		/// <param name="displayFormat">Display format to use.</param>
		/// <param name="backBufferFormat">The format of the render target.</param>
		/// <param name="depthStencilFormat">Depth/stencil format to check.</param>
		/// <param name="isWindowed">TRUE if using windowed mode, FALSE if not.</param>
		/// <returns>TRUE if the depth stencil type is supported, FALSE if not.</returns>
		public override bool SupportsDepthFormat(GorgonBackBufferFormat displayFormat, GorgonBufferFormat targetFormat, GorgonDepthBufferFormat depthStencilFormat, bool isWindowed)
		{
			Format adapterFormat = (isWindowed ? D3DConvert.ConvertBackBufferFormat(DefaultVideoMode.Format) : D3DConvert.ConvertBackBufferFormat(displayFormat));

			if (_d3d.CheckDeviceFormat(_info.Adapter, _deviceType, adapterFormat, Usage.DepthStencil, ResourceType.Surface, D3DConvert.ConvertDepthBufferFormat(depthStencilFormat)))			
			{
				if (_d3d.CheckDepthStencilMatch(_info.Adapter, _deviceType, adapterFormat, D3DConvert.ConvertFormat(targetFormat), D3DConvert.ConvertDepthBufferFormat(depthStencilFormat)))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Function to determine if a texture format can be used with a specific display format.
		/// </summary>
		/// <param name="displayFormat">Display format to use.</param>
		/// <param name="depthStencilFormat">Texture format to check.</param>
		/// <param name="dynamic">TRUE if using a dynamic texture, FALSE for static textures.</param>
		/// <param name="isWindowed">TRUE if using windowed mode, FALSE if not.</param>
		/// <returns>TRUE if the texture format is supported, FALSE if not.</returns>
		public bool SupportsTextureFormat(GorgonBackBufferFormat displayFormat, GorgonBufferFormat textureFormat, bool dynamic, bool isWindowed)
		{
			// TODO:  Replace dynamic with a flag enumeration for Dynamic, Static or RenderTarget.
			Format adapterFormat = (isWindowed ? D3DConvert.ConvertBackBufferFormat(DefaultVideoMode.Format) : D3DConvert.ConvertBackBufferFormat(displayFormat));
			Usage usage = Usage.None;

			if (dynamic)
				usage = Usage.Dynamic;

			return _d3d.CheckDeviceFormat(_info.Adapter, _deviceType, adapterFormat, usage, ResourceType.Texture, D3DConvert.ConvertFormat(textureFormat));
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="D3D9VideoOutput"/> class.
		/// </summary>
		/// <param name="d3d">Direct 3D interface.</param>
		/// <param name="deviceType">Device type.</param>
		/// <param name="adapter">Adapter information.</param>
		/// <param name="headIndex">Index of the adapter head.</param>
		/// <param name="monitorInfo">Monitor information.</param>
		public D3D9VideoOutput(Direct3D d3d, DeviceType deviceType, AdapterInformation adapter, int headIndex, MONITORINFOEX monitorInfo)
		{
			if (adapter == null)
				throw new ArgumentNullException("adapter");

			_d3d = d3d;
			_info = adapter;
			Handle = adapter.Monitor;
			Rotation = 0;
			IsAttachedToDesktop = ((monitorInfo.Flags & 1) == 1);
			DesktopDimensions = System.Drawing.Rectangle.FromLTRB(monitorInfo.WorkArea.Left, monitorInfo.WorkArea.Top, monitorInfo.WorkArea.Right, monitorInfo.WorkArea.Bottom);
			Name = monitorInfo.DeviceName;
			HeadIndex = headIndex;
			DefaultVideoMode = D3DConvert.Convert(adapter.CurrentDisplayMode);
		}
		#endregion
	}
}
