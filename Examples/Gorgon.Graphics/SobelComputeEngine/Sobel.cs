#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: August 3, 2017 10:18:21 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Native;
using SharpDX.Direct3D11;

namespace Gorgon.Graphics.Example
{
    /// <summary>
    /// The sobel edge detection shader.
    /// </summary>
    class Sobel
        : IDisposable
    {
        #region Variables.
        // The compute engine to use.
        private readonly GorgonComputeEngine _compute;
        // The shader for sobel edge detection.
        private readonly GorgonComputeShader _shader;
        // The sobel constant data.
        private readonly GorgonConstantBuffer _sobelData;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to process a texture into the output texture.
        /// </summary>
        /// <param name="texture">The texture to process.</param>
        /// <param name="outputTexture">The output texture that will receive the processed texture.</param>
        /// <param name="thickness">The thickness of the sobel lines.</param>
        /// <param name="threshold">The threshold used to determine an edge.</param>
        public void Process(GorgonTextureView texture, GorgonTextureUav outputTexture, int thickness, float threshold)
        {
            if ((texture == null)
                || (outputTexture == null))
            {
                return;
            }

            GorgonPointerAlias data = _sobelData.Lock(MapMode.WriteDiscard);
            data.Write((float)thickness);
            data.Write(4, threshold);
            _sobelData.Unlock(ref data);

            _compute.ConstantBuffers[0] = _sobelData;
            _compute.ShaderResourceViews[0] = texture;
            _compute.UnorderedAccessViews[0] = new GorgonUavBinding(outputTexture);

            // Send 32 threads per group.
            _compute.Execute(_shader, (int)(texture.Width / 32.0f).FastCeiling(), (int)(texture.Height / 32.0f).FastCeiling(), 1);

            // Always call unbind after we're done so that we can be sure the state won't conflict.
            _compute.Unbind();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _sobelData?.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Sobel"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface to use.</param>
        /// <param name="sobelShader">The shader for sobel edge detection.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="sobelShader"/> parameter is <b>null</b>.</exception>
        public Sobel(GorgonGraphics graphics, GorgonComputeShader sobelShader)
        {
            _compute = new GorgonComputeEngine(graphics);
            _shader = sobelShader ?? throw new ArgumentNullException(nameof(sobelShader));
            _sobelData = new GorgonConstantBuffer("SobelData", graphics, new GorgonConstantBufferInfo
                                                                         {
                                                                             Usage = ResourceUsage.Dynamic,
                                                                             SizeInBytes = 16
                                                                         });
        }
        #endregion
    }
}
