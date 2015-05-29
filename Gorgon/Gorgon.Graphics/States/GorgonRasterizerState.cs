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

using System.Drawing;
using Gorgon.Core;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Defines how a triangle should be culled.
	/// </summary>
	public enum CullingMode
	{
		/// <summary>
		/// Unknown.
		/// </summary>
		Unknown = 0,
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
		/// Unknown.
		/// </summary>
		Unknown = 0,
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
		: IEquatableByRef<GorgonRasterizerStates>
	{
		#region Variables.
		/// <summary>
		/// Wireframe, with no culling.
		/// </summary>
		public static readonly GorgonRasterizerStates WireFrame = new GorgonRasterizerStates
		{
			CullingMode = CullingMode.None,
			FillMode = FillMode.Wireframe,
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
		/// Back face culling states.
		/// </summary>
		/// <remarks>This is the default state for the rasterizer states.</remarks>
		public static readonly GorgonRasterizerStates CullBackFace = new GorgonRasterizerStates
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
        /// Front face culling states.
        /// </summary>
	    public static readonly GorgonRasterizerStates CullFrontFace = new GorgonRasterizerStates
		    {
                CullingMode = CullingMode.Front,
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
        /// Default raster states with no culling.
        /// </summary>
        public static readonly GorgonRasterizerStates NoCulling = new GorgonRasterizerStates
	        {
            CullingMode = CullingMode.None,
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
		/// <remarks>The default value is <c>false</c>.</remarks>
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
		/// <remarks>The default value is <c>true</c>.</remarks>
		public bool IsDepthClippingEnabled;

		/// <summary>
		/// Property to set or return whether to enable scissor testing.
		/// </summary>
		/// <remarks>When this value is set to <c>true</c> any pixels outside the active scissor rectangle are culled.
		/// <para>The default value is <c>false</c>.</para>
		/// </remarks>
		public bool IsScissorTestingEnabled;

		/// <summary>
		/// Property to set or return whether multisampling is enabled or not.
		/// </summary>
		/// <remarks>This must be set to <c>true</c> in order to activate multisampling.
		/// <para>The default value is <c>false</c>.</para>
		/// </remarks>
		public bool IsMultisamplingEnabled;

		/// <summary>
		/// Property to set or return whether antialiasing should be used when drawing lines.
		/// </summary>
		/// <remarks>This value is only valid if <see cref="Gorgon.Graphics.GorgonRasterizerStates.IsMultisamplingEnabled">IsMultisamplingEnabled</see> is equal to <c>false</c>.
		/// <para>The default value is <c>false</c>.</para>
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
                // ReSharper disable NonReadonlyFieldInGetHashCode
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
                // ReSharper restore NonReadonlyFieldInGetHashCode
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
		    {
		        return Equals((GorgonRasterizerStates)obj);
		    }

		    return base.Equals(obj);
		}

		/// <summary>
		/// Function to compare two sets of rasterizer states for equality.
		/// </summary>
		/// <param name="left">Left states to compare.</param>
		/// <param name="right">Right states to compare.</param>
		/// <returns><c>true</c> if equal, <c>false</c> if not.</returns>
		public static bool Equals(ref GorgonRasterizerStates left, ref GorgonRasterizerStates right)
		{
			// ReSharper disable CompareOfFloatsByEqualityOperator
			return ((left.CullingMode == right.CullingMode) && (left.DepthBias == right.DepthBias)
		            && (left.DepthBiasClamp == right.DepthBiasClamp) && (left.FillMode == right.FillMode) &&
		            (left.IsAntialiasedLinesEnabled == right.IsAntialiasedLinesEnabled)
		            && (left.IsDepthClippingEnabled == right.IsDepthClippingEnabled)
		            && (left.IsFrontFacingTriangleCounterClockwise == right.IsFrontFacingTriangleCounterClockwise) &&
		            (left.IsMultisamplingEnabled == right.IsMultisamplingEnabled)
		            && (left.IsScissorTestingEnabled == right.IsScissorTestingEnabled)
		            && (left.SlopeScaledDepthBias == right.SlopeScaledDepthBias));
			// ReSharper restore CompareOfFloatsByEqualityOperator
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
			return Equals(ref left, ref right);
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
			return !Equals(ref left, ref right);
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
			return Equals(ref this, ref other);
		}
		#endregion

		#region IEquatableByRef<GorgonRasterizerStates> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(ref GorgonRasterizerStates other)
		{
			return Equals(ref this, ref other);
		}
		#endregion
	}
	#endregion

	/// <summary>
	/// Render states for the rasterizer.
	/// </summary>
	public sealed class GorgonRasterizerRenderState
		: GorgonState<GorgonRasterizerStates>
	{
		#region Variables.
		private Rectangle[] _clipRects;						// Clipping rectangles for scissor testing.
		private GorgonViewport[] _viewPorts;				// Viewports.
		private DX.Rectangle[] _dxRects;					// DirectX rectangles for scissor testing.
		private DX.ViewportF[] _dxViewports;				// DirectX viewports.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the maximum number of viewport and scissor test rectangles allowed for the device.
		/// </summary>
		public int MaxViewportScissorTestCount
		{
			get
			{
				return 16;
			}
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function to reset the rasterizer state.
        /// </summary>
        internal override void Reset()
        {
            base.Reset();

            States = GorgonRasterizerStates.CullBackFace;
            
            Graphics.Context.Rasterizer.SetViewports(null);
            Graphics.Context.Rasterizer.SetScissorRectangles(null);

            _dxRects = null;
            _clipRects = null;
            _viewPorts = null;
            _dxViewports = null;
        }

		/// <summary>
		/// Function to apply the state to the current rendering context.
		/// </summary>
		/// <param name="stateObject">State to apply.</param>
		internal override void ApplyState(D3D.DeviceChild stateObject)
		{
			Graphics.Context.Rasterizer.State = (D3D.RasterizerState)stateObject;
		}

		/// <summary>
		/// Function to retrieve the D3D state object.
		/// </summary>
		/// <param name="stateType">The state type information.</param>
		/// <returns>The D3D state object.</returns>
		internal override D3D.DeviceChild GetStateObject(ref GorgonRasterizerStates stateType)
		{
			var desc = new D3D.RasterizerStateDescription
				{
					CullMode = (D3D.CullMode)States.CullingMode,
					DepthBias = States.DepthBias,
					DepthBiasClamp = States.DepthBiasClamp,
					FillMode = (D3D.FillMode)States.FillMode,
					IsAntialiasedLineEnabled = States.IsAntialiasedLinesEnabled,
					IsDepthClipEnabled = States.IsDepthClippingEnabled,
					IsFrontCounterClockwise = States.IsFrontFacingTriangleCounterClockwise,
					IsMultisampleEnabled = States.IsMultisamplingEnabled,
					IsScissorEnabled = States.IsScissorTestingEnabled
				};

			var state = new D3D.RasterizerState(Graphics.D3DDevice, desc)
				{
					DebugName = "Gorgon Rasterizer State #" + StateCacheCount
				};

			return state;
		}

        /// <summary>
        /// Function to return a viewport by index.
        /// </summary>
        /// <param name="index">Index of the viewport.</param>
        /// <returns>The viewport or NULL if no viewport was set at the index.</returns>
        public GorgonViewport? GetViewport(int index)
        {
            if ((_viewPorts == null) || (_viewPorts.Length == 0) || (index < 0) || (index >= _viewPorts.Length))
            {
                return null;
            }

            return _viewPorts[index];
        }

        /// <summary>
        /// Function to return all the viewports.
        /// </summary>
        /// <returns>An array containing all the viewports.</returns>
        /// <remarks>This will return an array with all of the viewports bound to the pipeline.</remarks>
        public GorgonViewport[] GetViewports()
        {
            var result = new GorgonViewport[_viewPorts == null ? 0 : _viewPorts.Length];

            if ((_viewPorts != null) && (_viewPorts.Length > 0))
            {
                _viewPorts.CopyTo(result, 0);
            }

            return result;
        }

		/// <summary>
		/// Function to set a list of viewports.
		/// </summary>
		/// <remarks>This will clip/scale the output to the the constraints defined in the viewports.
		/// <para>Viewports must have a width and height greater than 0.</para>
        /// <para>Viewports must be set all at once, any viewports not defined in the <paramref name="viewPorts"/> parameter will be disabled.  Passing NULL to the <paramref name="viewPorts"/> parameter 
        /// will disable all viewports.</para>
        /// <para>Which viewport is in use is determined by the <c>SV_ViewportArrayIndex</c> HLSL semantic output by a geometry shader.  If no geometry shader is bound, or the 
        /// geometry shader does not make use of the <c>SV_ViewportArrayIndex</c> semantic, then only the first viewport is used.</para>
        /// <para>On only the first scissor test rectangle will be used on devices with a feature level of SM2_a_b.  This is because they cannot set the SV_ViewportArrayIndex semantic in 
        /// a geometry shader because these devices do not support geometry shaders.</para>
        /// </remarks>
		public void SetViewports(GorgonViewport[] viewPorts)
		{
			if ((viewPorts == null) || (viewPorts.Length == 0))
			{
				Graphics.Context.Rasterizer.SetViewports(null);
			    _dxViewports = null;
			    _viewPorts = null;
				return;
			}

		    _viewPorts = viewPorts;

            if ((_dxViewports == null)
                || (_dxViewports.Length != viewPorts.Length))
            {
                _dxViewports = new DX.ViewportF[_viewPorts.Length];
            }

            for (int i = 0; i < _viewPorts.Length; i++)
            {
                _dxViewports[i] = _viewPorts[i].Convert();
            }

			Graphics.Context.Rasterizer.SetViewports(_dxViewports);
		}


		/// <summary>
		/// Function to set a single viewport.
		/// </summary>
		/// <param name="viewPort">Viewport to set.</param>
		/// <remarks>Viewports must have a width and height greater than 0.</remarks>
		public void SetViewport(GorgonViewport viewPort)
		{
            if ((_viewPorts == null) || (_viewPorts.Length != 1))
            {
                _viewPorts = new[]
                    {
                        viewPort
                    };
                _dxViewports = new[]
                    {
                        viewPort.Convert()
                    };
            }
            else
            {
                if (GorgonViewport.Equals(ref _viewPorts[0], ref viewPort))
                {
                    return;
                }

                _viewPorts[0] = viewPort;
                _dxViewports[0] = viewPort.Convert();
            }

			Graphics.Context.Rasterizer.SetViewport(viewPort.Left, viewPort.Top, viewPort.Width, viewPort.Height, viewPort.MinimumZ, viewPort.MaximumZ);			
		}

		/// <summary>
		/// Function to set a scissor rectangle clipping rectangle.
		/// </summary>
		/// <param name="rectangle">Rectangle to set.</param>
		/// <remarks>Scissor rectangles define a 2D area on the render target that can be used for clipping.  That is, all pixels outside of the rectangle will be discarded.
        /// <para>To use scissor rectangles, set the <see cref="Gorgon.Graphics.GorgonRasterizerStates.IsScissorTestingEnabled">IsScissorTestingEnabled</see> 
		/// state to <c>true</c>. If the state is set to <c>false</c>, this value will have no effect.</para>
        /// <para>This method will only set the first scissor test rectangle.</para>
		/// </remarks>
		public void SetScissorRectangle(Rectangle rectangle)
		{
			if ((_clipRects == null) || (_clipRects.Length != 1))
			{
				_clipRects = new[]
					{
						rectangle
					};
			    _dxRects = new[]
			        {
                        DX.Rectangle.Empty
			        };
			}
			else
			{
                if (_clipRects[0] == rectangle)
                {
                    return;
                }

				_clipRects[0] = rectangle;
                _dxRects[0] = new DX.Rectangle(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
			}

			Graphics.Context.Rasterizer.SetScissorRectangle(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
		}

		/// <summary>
		/// Function to return a scissor test rectangle by index.
		/// </summary>
		/// <param name="index">Index of the scissor test rectangle.</param>
		/// <returns>The scissor test rectangle or NULL if no scissor test rectangle was set at the index.</returns>
		public Rectangle? GetScissorRectangle(int index)
		{
			if ((_clipRects == null) || (_clipRects.Length == 0) || (index < 0) || (index >= _clipRects.Length))
			{
				return null;
			}

			return _clipRects[index];
		}

		/// <summary>
		/// Function to return all the scissor test rectangles.
		/// </summary>
		/// <returns>An array containing all the scissor test rectangles.</returns>
		/// <remarks>This will return a array of all scissor test rectangles currently bound to the pipeline.</remarks>
		public Rectangle[] GetScissorRectangles()
		{
		    var result = new Rectangle[_clipRects == null ? 0 : _clipRects.Length];

            if ((_clipRects != null)
                && (_clipRects.Length > 0))
            {
                _clipRects.CopyTo(result, 0);
            }

		    return result;
		}

		/// <summary>
		/// Function to set a list of scissor rectangle clipping rectangles.
		/// </summary>
		/// <param name="rectangles">An array containing the scissor testing rectangles.</param>
        /// <remarks>Scissor rectangles define a 2D area on the render target that can be used for clipping.  That is, all pixels outside of the rectangle will be discarded.
        /// <para>To use scissor rectangles, set the <see cref="Gorgon.Graphics.GorgonRasterizerStates.IsScissorTestingEnabled">IsScissorTestingEnabled</see> 
		/// state to <c>true</c>. If the state is set to <c>false</c>, then setting a scissor test rectangle will have no effect.</para>
        /// <para>Scissor test rectangles must be set all at once, any viewports not defined in the <paramref name="rectangles"/> parameter will be disabled.  Passing NULL (Nothing in VB.Net) to the 
        /// <paramref name="rectangles"/> parameter will disable all scissor test rectangles.</para>
        /// <para>Which scissor rectangle is in use is determined by the <c>SV_ViewportArrayIndex</c> HLSL semantic output by a geometry shader.  If no geometry shader is bound, or the 
        /// geometry shader does not make use of the <c>SV_ViewportArrayIndex</c> semantic, then only the first rectangle is used.</para>
        /// <para>Each scissor test rectangle corresponds to a <see cref="Gorgon.Graphics.GorgonRasterizerRenderState.SetViewports">viewport</see> in an array of viewports.</para>
        /// <para>Only the first scissor test rectangle will be used on devices with a feature level of SM2_a_b.  This is because they cannot set the SV_ViewportArrayIndex semantic in 
        /// a geometry shader because these devices do not support geometry shaders.</para>
		/// </remarks>
		public void SetScissorRectangles(Rectangle[] rectangles)
		{
			if ((rectangles == null) || (rectangles.Length == 0))
			{
				Graphics.Context.Rasterizer.SetScissorRectangles(null);
				_clipRects = null;
				_dxRects = null;
				return;
			}

			_clipRects = rectangles;
			if ((_dxRects == null) || (_dxRects.Length != _clipRects.Length))
			{
				_dxRects = new DX.Rectangle[_clipRects.Length];
			}

			for (int i = 0; i < _clipRects.Length; i++)
			{
				var clipRect = _clipRects[i];
				_dxRects[i] = new DX.Rectangle(clipRect.Left, clipRect.Top, clipRect.Right, clipRect.Bottom);
			}

			Graphics.Context.Rasterizer.SetScissorRectangles(_dxRects);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRasterizerRenderState"/> class.
		/// </summary>
		internal GorgonRasterizerRenderState(GorgonGraphics graphics)
			: base(graphics)
		{
		}
		#endregion
	}
}
