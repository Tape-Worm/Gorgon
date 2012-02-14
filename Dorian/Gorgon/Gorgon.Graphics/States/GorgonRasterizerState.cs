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
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Defines how a triangle should be culled.
	/// </summary>
	public enum CullingMode
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
	public enum FillMode
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

	#region Value Types.
	/// <summary>
	/// Immutable states for the rasterizer.
	/// </summary>
	public struct GorgonRasterizerStates
		: IEquatable<GorgonRasterizerStates>
	{
		#region Variables.
		/// <summary>
		/// Default rasterizer states.
		/// </summary>
		public static readonly GorgonRasterizerStates DefaultStates = new GorgonRasterizerStates()
		{
			CullingMode = CullingMode.Back,
			FillMode = FillMode.Solid,
			IsFrontFacingTriangleCounterClockwise = false,
			DepthBias = 0,
			DepthBiasClamp = 0.0f,
			SlopeScaledDepthBias = 0.0f,
			IsDepthClippingEnabled = true,
			IsAntialiasedLinesEnabled = false,
			IsMultisamplingEnabled = false,
			IsScissorTestingEnabled = false
		};

		/// <summary>
		/// The triangle culling mode for the rasterizer.
		/// </summary>
		/// <remarks>The default value is Back.</remarks>
		public CullingMode CullingMode;

		/// <summary>
		/// Property to set or return the triangle filling mode.
		/// </summary>
		/// <remarks>The default value is Solid.</remarks>
		public FillMode FillMode;

		/// <summary>
		/// Property to set or return whether a triangle uses clockwise or counterclockwise vertices to determine whether it is front or back facing respectively.
		/// </summary>
		/// <remarks>The default value is FALSE.</remarks>
		public bool IsFrontFacingTriangleCounterClockwise;

		/// <summary>
		/// Property to set or return a value to add to a pixel when comparing depth.
		/// </summary>
		/// <remarks>The default value is 0.</remarks>
		public int DepthBias;

		/// <summary>
		/// Property to set or return the maximum depth bias for a pixel.
		/// </summary>
		/// <remarks>The default value is 0.0f.</remarks>
		public float DepthBiasClamp;

		/// <summary>
		/// Property to set or return the scalar value for a pixel slope.
		/// </summary>
		/// <remarks>The default value is 0.0f.</remarks>
		public float SlopeScaledDepthBias;

		/// <summary>
		/// Property to set or return whether the hardware should clip the Z value.
		/// </summary>
		/// <remarks>The default value is TRUE.</remarks>
		public bool IsDepthClippingEnabled;

		/// <summary>
		/// Property to set or return whether to enable scissor testing.
		/// </summary>
		/// <remarks>When this value is set to TRUE any pixels outside the active scissor rectangle are culled.
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool IsScissorTestingEnabled;

		/// <summary>
		/// Property to set or return whether multisampling is enabled or not.
		/// </summary>
		/// <remarks>This must be set to TRUE in order to activate multisampling.
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool IsMultisamplingEnabled;

		/// <summary>
		/// Property to set or return whether antialiasing should be used when drawing lines.
		/// </summary>
		/// <remarks>This value is only valid if <see cref="P:GorgonLibrary.GorgonGraphics.GorgonRasterizerState.RasterizerStates.IsMultisamplingEnabled">IsMultisamplingEnabled</see> is equal to FALSE.
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool IsAntialiasedLinesEnabled;
		#endregion

		#region Methods.
		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return 281.GenerateHash(CullingMode).
						GenerateHash(DepthBias).
						GenerateHash(DepthBiasClamp).
						GenerateHash(FillMode).
						GenerateHash(IsAntialiasedLinesEnabled).
						GenerateHash(IsDepthClippingEnabled).
						GenerateHash(IsFrontFacingTriangleCounterClockwise).
						GenerateHash(IsMultisamplingEnabled).
						GenerateHash(IsScissorTestingEnabled).
						GenerateHash(SlopeScaledDepthBias);
			}
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonRasterizerStates)
				return Equals((GorgonRasterizerStates)obj);

			return base.Equals(obj);
		}

		/// <summary>
		/// Function to compare two sets of rasterizer states for equality.
		/// </summary>
		/// <param name="left">Left states to compare.</param>
		/// <param name="right">Right states to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(ref GorgonRasterizerStates left, ref GorgonRasterizerStates right)
		{
			return ((left.CullingMode == right.CullingMode) && (left.DepthBias == right.DepthBias) && (left.DepthBiasClamp == right.DepthBiasClamp) && (left.FillMode == right.FillMode) &&
					(left.IsAntialiasedLinesEnabled == right.IsAntialiasedLinesEnabled) && (left.IsDepthClippingEnabled == right.IsDepthClippingEnabled) && (left.IsFrontFacingTriangleCounterClockwise == right.IsFrontFacingTriangleCounterClockwise) &&
					(left.IsMultisamplingEnabled == right.IsMultisamplingEnabled) && (left.IsScissorTestingEnabled == right.IsScissorTestingEnabled) && (left.SlopeScaledDepthBias == right.SlopeScaledDepthBias));
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==(GorgonRasterizerStates left, GorgonRasterizerStates right)
		{
			return GorgonRasterizerStates.Equals(ref left, ref right);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=(GorgonRasterizerStates left, GorgonRasterizerStates right)
		{
			return !GorgonRasterizerStates.Equals(ref left, ref right);
		}
		#endregion

		#region IEquatable<RasterizerStates> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonRasterizerStates other)
		{
			return GorgonRasterizerStates.Equals(ref this, ref other);
		}
		#endregion
	}
	#endregion

	/// <summary>
	/// Render states for the rasterizer.
	/// </summary>
	public class GorgonRasterizerRenderState
		: GorgonStateObject<GorgonRasterizerStates>
	{
		#region Methods.
		/// <summary>
		/// Function to apply the state to the appropriate state object.
		/// </summary>
		/// <param name="state">The Direct3D state object to apply.</param>
		protected override void ApplyState(IDisposable state)
		{
			Graphics.Context.Rasterizer.State = (D3D.RasterizerState)state;
		}

		/// <summary>
		/// Function to convert this state object into a rasterizer state.
		/// </summary>
		/// <returns>The new rasterizer state.</returns>
		protected override IDisposable Convert()
		{			
			D3D.RasterizerStateDescription desc = new D3D.RasterizerStateDescription();

			desc.CullMode = (D3D.CullMode)States.CullingMode;
			desc.DepthBias = States.DepthBias;
			desc.DepthBiasClamp = States.DepthBiasClamp;
			desc.FillMode = (D3D.FillMode)States.FillMode;
			desc.IsAntialiasedLineEnabled = States.IsAntialiasedLinesEnabled;
			desc.IsDepthClipEnabled = States.IsDepthClippingEnabled;
			desc.IsFrontCounterClockwise = States.IsFrontFacingTriangleCounterClockwise;
			desc.IsMultisampleEnabled = States.IsMultisamplingEnabled;
			desc.IsScissorEnabled = States.IsScissorTestingEnabled;

			D3D.RasterizerState state = new D3D.RasterizerState(Graphics.D3DDevice, desc);
			state.DebugName = "Gorgon Rasterizer State #" + StateCacheCount.ToString();

			return state;
		}

		/// <summary>
		/// Function to set a list of viewports.
		/// </summary>
		/// <remarks>This will clip/scale the output to the the constraints in the viewport(s).
		/// <para>Viewports must have a width and height greater than 0.</para>
		/// <para>Which viewport to use is determined by the SV_ViewportArrayIndex semantic output by a geometry shader; if a geometry shader does not specify the semantic, then the first viewport in the list will be used.</para>
		/// </remarks>
		public void SetViewport(IEnumerable<GorgonViewport> viewPorts)
		{
			if (viewPorts == null)
			{
				Graphics.Context.Rasterizer.SetViewport(0, 0, 1.0f, 1.0f, 0, 1.0f);
				return;
			}

			var d3dViews = (from viewPort in viewPorts
							where viewPort.IsEnabled
							select viewPort.Convert()).ToArray();

			Graphics.Context.Rasterizer.SetViewports(d3dViews);
		}


		/// <summary>
		/// Function to set a single viewport.
		/// </summary>
		/// <param name="viewPort">Viewport to set.</param>
		/// <remarks>Viewports must have a width and height greater than 0.</remarks>
		public void SetViewport(GorgonViewport viewPort)
		{
			Graphics.Context.Rasterizer.SetViewport(viewPort.Region.X, viewPort.Region.Y, viewPort.Region.Width, viewPort.Region.Height, viewPort.MinimumZ, viewPort.MaximumZ);			
		}

		/// <summary>
		/// Function to set a scissor rectangle clipping region.
		/// </summary>
		/// <param name="region">Region to set.</param>
		public void SetClip(System.Drawing.Rectangle region)
		{
			Graphics.Context.Rasterizer.SetScissorRectangle(region.Left, region.Top, region.Right, region.Bottom);
		}

		/// <summary>
		/// Function to set a list of scissor rectangle clipping regions.
		/// </summary>
		/// <param name="regions">Regions to set.</param>
		/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="regions"/> parameter will use the width/height of the current <see cref="P:GorgonLibrary.Graphics.OutputMerger.RenderTargets">render target</see>.</remarks>
		public void SetClip(IEnumerable<System.Drawing.Rectangle> regions)
		{
			if (regions == null)
			{
				if (Graphics.Output.RenderTargets[0] != null)
					SetClip(new System.Drawing.Rectangle(0, 0, Graphics.Output.RenderTargets[0].Settings.Width, Graphics.Output.RenderTargets[0].Settings.Height));
				return;
			}

			var dxRegions = (from region in regions
							 let dxRegion = new SharpDX.Rectangle(region.Left, region.Top, region.Right, region.Bottom)
							 select dxRegion).ToArray();

			Graphics.Context.Rasterizer.SetScissorRectangles(dxRegions);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRasterizerRenderState"/> class.
		/// </summary>
		internal GorgonRasterizerRenderState(GorgonGraphics graphics)
			: base(graphics, 4096, 10000)
		{
			States = GorgonRasterizerStates.DefaultStates;
		}
		#endregion
	}
}
