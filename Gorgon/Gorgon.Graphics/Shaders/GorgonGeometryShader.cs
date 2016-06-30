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

using Gorgon.Core;
using Gorgon.Graphics.Properties;
using Compiler = SharpDX.D3DCompiler;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
    /// <summary>
    /// A geometry shader object.
    /// </summary>
    /// <remarks>Use geometry shaders to create geometry (vertex data) on the GPU.</remarks>
    public class GorgonGeometryShader
        : GorgonShader
    {
        #region Variables.
        private bool _disposed;			// Flag to indicate that the object was disposed.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the Direct3D pixel shader.
        /// </summary>
        internal D3D11.GeometryShader D3DShader
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Disassociate any shaders after we've destroyed them.
                    if (Graphics.Shaders.GeometryShader.Current == this)
                    {
                        Graphics.Shaders.GeometryShader.Current = null;
                    }

                    if (D3DShader != null)
                    {
                        D3DShader.Dispose();
                    }

                    D3DShader = null;
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

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
            
            D3DShader = new D3D11.GeometryShader(Graphics.VideoDevice.D3DDevice(), byteCode)
                {
#if DEBUG
                    DebugName = $"Gorgon Geometry Shader '{Name}'"
#endif
                };
        }
        #endregion

        #region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGeometryShader"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		/// <param name="name">The name of the pixel shader.</param>
		/// <param name="entryPoint">The entry point method for the shader.</param>
        internal GorgonGeometryShader(GorgonGraphics graphics, string name, string entryPoint)
			: base(graphics, name, ShaderType.Geometry, entryPoint)
		{
			if (graphics.VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_10_0)
			{
				throw new GorgonException(GorgonResult.CannotCreate,
				                          string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_10_0));
			}
		}
        #endregion
    }
}
