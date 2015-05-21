#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Friday, June 14, 2013 10:40:16 PM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using Compiler = SharpDX.D3DCompiler;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
    /// <summary>
    /// A geometry shader object which does stream output.
    /// </summary>
    /// <remarks>Use geometry shaders to create geometry (vertex data) on the GPU and output it to a buffer instead of passing it down the pipeline.</remarks>
    public class GorgonOutputGeometryShader
        : GorgonGeometryShader
    {
        #region Variables.
	    private readonly D3D.StreamOutputElement[] _elements;		// The stream output elements.
	    private readonly int[] _elementSizes;						// Size of the elements.
        #endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of elements attached to this stream out shader.
		/// </summary>
		public IEnumerable<GorgonStreamOutputElement> Elements
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the stream to be sent to the rasterizer.
		/// </summary>
		/// <remarks>If this value is -1, then no rasterization will take place.</remarks>
		public int RasterizedStream
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
        /// Function to create the shader.
        /// </summary>
        /// <param name="byteCode">Byte code for the shader.</param>
        protected override void CreateShader(Compiler.ShaderBytecode byteCode)
        {
            if (D3DShader != null)
            {
                D3DShader.Dispose();
            }
            
            D3DShader = new D3D.GeometryShader(Graphics.D3DDevice, byteCode, _elements, _elementSizes, RasterizedStream)
                {
#if DEBUG
                    DebugName = string.Format("Gorgon Geometry Shader '{0}'", Name)
#endif
                };
        }
        #endregion

        #region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonOutputGeometryShader"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		/// <param name="name">The name of the pixel shader.</param>
		/// <param name="entryPoint">The entry point method for the shader.</param>
		/// <param name="rasterizedStream">Stream number to be rasterized.</param>
		/// <param name="outputElements">A list of elements to describe the layout of the data for output.</param>
		/// <param name="bufferStrides">The size of each buffer.</param>
        internal GorgonOutputGeometryShader(GorgonGraphics graphics, string name, string entryPoint, int rasterizedStream, IList<GorgonStreamOutputElement> outputElements, IList<int> bufferStrides)
			: base(graphics, name, entryPoint)
		{
			if (graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM4)
			{
				throw new GorgonException(GorgonResult.CannotCreate,
				                          string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM4));
			}

			// If the stream to be rasterized is outside of the range for stream output, then set it to no rasterization.
			if ((rasterizedStream < -1) || (rasterizedStream > 3))
			{
				rasterizedStream = -1;
			}

			_elements = new D3D.StreamOutputElement[outputElements.Count];
			_elementSizes = new int[bufferStrides.Count];
			for (int i = 0; i < _elements.Length; i++)
			{
				_elements[i] = outputElements[i].Convert();
			}

			for (int i = 0; i < bufferStrides.Count; i++)
			{
				_elementSizes[i] = bufferStrides[i];
			}

			RasterizedStream = rasterizedStream;
			Elements = outputElements;
		}
        #endregion
    }
}
