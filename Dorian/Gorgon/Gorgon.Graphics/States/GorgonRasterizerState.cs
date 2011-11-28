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
// Created: Tuesday, November 22, 2011 5:44:00 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Defines how a triangle should be culled.
	/// </summary>
	public enum GorgonCullingMode
	{
		/// <summary>
		/// No culling.
		/// </summary>
		None = 1,
		/// <summary>
		/// Front facing should be culled.
		/// </summary>
		Front = 2,
		/// <summary>
		/// Back facing should be culled.
		/// </summary>
		Back = 3
	}

	/// <summary>
	/// Defines how a triangle should be filled.
	/// </summary>
	public enum GorgonFillMode
	{
		/// <summary>
		/// Wireframe triangles.
		/// </summary>
		Wireframe = 2,
		/// <summary>
		/// Solid triangles.
		/// </summary>
		Solid = 3
	}

	/// <summary>
	/// Render states for the rasterizer.
	/// </summary>
	public class GorgonRasterizerState
		: GorgonStateObject<D3D.RasterizerState>
	{
		#region Variables.
		private bool _disposed = false;							// Flag to indicate that the object was disposed of.
		private D3D.RasterizerStateDescription _desc;			// Rasterizer state description.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the triangle culling mode for the rasterizer.
		/// </summary>
		public GorgonCullingMode CullingMode
		{
			get
			{
				return (GorgonCullingMode)_desc.CullMode;
			}
			set
			{
				if (_desc.CullMode != (D3D.CullMode)value)
				{
					_desc.CullMode = (D3D.CullMode)value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the triangle filling mode.
		/// </summary>
		public GorgonFillMode FillMode
		{
			get
			{
				return (GorgonFillMode)_desc.FillMode;
			}
			set
			{
				if (_desc.FillMode != (D3D.FillMode)value)
				{
					_desc.FillMode = (D3D.FillMode)value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return whether a triangle uses clockwise or counterclockwise vertices to determine whether it is front or back facing respectively.
		/// </summary>
		public bool IsFrontFacingTriangleCounterClockwise
		{
			get
			{
				return _desc.IsFrontCounterClockwise;
			}
			set
			{
				if (_desc.IsFrontCounterClockwise != value)
				{
					_desc.IsFrontCounterClockwise = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return a value to add to a pixel when comparing depth.
		/// </summary>
		public int DepthBias
		{
			get
			{
				return _desc.DepthBias;
			}
			set
			{
				if (_desc.DepthBias != value)
				{
					_desc.DepthBias = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the maximum depth bias for a pixel.
		/// </summary>
		public float DepthBiasClamp
		{
			get
			{
				return _desc.DepthBiasClamp;
			}
			set
			{
				if (_desc.DepthBiasClamp != value)
				{
					_desc.DepthBiasClamp = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the scalar value for a pixel slope.
		/// </summary>
		public float SlopeScaledDepthBias
		{
			get
			{
				return _desc.SlopeScaledDepthBias;
			}
			set
			{
				if (_desc.SlopeScaledDepthBias != value)
				{
					_desc.SlopeScaledDepthBias = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return whether the hardware should clip the Z value.
		/// </summary>
		public bool IsDepthClippingEnabled
		{
			get
			{
				return _desc.IsDepthClipEnabled;
			}
			set
			{
				if (_desc.IsDepthClipEnabled != value)
				{
					_desc.IsDepthClipEnabled = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return whether to enable scissor testing.
		/// </summary>
		/// <remarks>When this value is set to TRUE any pixels outside the active scissor rectangle are culled.</remarks>
		public bool IsScissorTestingEnabled
		{
			get
			{				
				return _desc.IsScissorEnabled;
			}
			set
			{
				if (_desc.IsScissorEnabled != value)
				{
					_desc.IsScissorEnabled = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return whether multisampling is enabled or not.
		/// </summary>
		public bool IsMultisamplingEnabled
		{
			get
			{
				return _desc.IsMultisampleEnabled;
			}
			set
			{
				if (_desc.IsMultisampleEnabled != value)
				{
					_desc.IsMultisampleEnabled = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return whether antialiasing should be used when drawing lines.
		/// </summary>
		/// <remarks>This value is only valid if <see cref="P:GorgonLibrary.GorgonGraphics.GorgonRasterizerState.IsMultisamplingEnabled">IsMultisamplingEnabled</see> is equal to FALSE.</remarks>
		public bool IsAntialiasedLinesEnabled
		{
			get
			{
				return _desc.IsAntialiasedLineEnabled;
			}
			set
			{
				if (_desc.IsAntialiasedLineEnabled != value)
				{
					_desc.IsAntialiasedLineEnabled = value;
					HasChanged = true;
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to apply any changes immediately if this state is the current state.
		/// </summary>
		protected override void ApplyImmediate()
		{
			if ((Graphics != null) && (Graphics.RasterizerState == this))
				Graphics.Context.Rasterizer.State = Convert();
		}

		/// <summary>
		/// Function to convert this state object into a rasterizer state.
		/// </summary>
		/// <returns>The new rasterizer state.</returns>
		protected internal override D3D.RasterizerState Convert()
		{			
			if ((HasChanged) || (State == null))
			{
				if (State != null)
					State.Dispose();
				State = new D3D.RasterizerState(Graphics.VideoDevice.D3DDevice, _desc);
				HasChanged = false;
			}

			return State;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRasterizerState"/> class.
		/// </summary>
		internal GorgonRasterizerState(GorgonGraphics graphics)
			: base(graphics)
		{
			FillMode = GorgonFillMode.Solid;
			CullingMode = GorgonCullingMode.Back;
			IsDepthClippingEnabled = true;
		}
		#endregion

		#region IDisposable Members
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
					// Unassign this state.
					if (Graphics.RasterizerState == this)
						Graphics.RasterizerState = null;
				}
				_disposed = true;
			}

			base.Dispose(disposing);
		}
		#endregion
	}
}
