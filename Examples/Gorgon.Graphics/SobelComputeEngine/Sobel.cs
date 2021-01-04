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
using Gorgon.Graphics.Core;
using Gorgon.Math;

namespace Gorgon.Graphics.Example
{
    /// <summary>
    /// The sobel edge detection shader.
    /// </summary>
    internal class Sobel
        : IDisposable
    {
        #region Variables.
        // The compute engine to use.
        private readonly GorgonComputeEngine _compute;
        // The sobel constant data.
        private readonly GorgonConstantBuffer _sobelData;
        // The options to send to the sobel shader.
        private readonly float[] _sobelOptions = new float[2];
        // The dispatch call.
        private GorgonDispatchCall _dispatch;
        // The dispatch call builder.
        private readonly GorgonDispatchCallBuilder _dispatchBuilder;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to process a texture into the output texture.
        /// </summary>
        /// <param name="texture">The texture to process.</param>
        /// <param name="outputTexture">The output texture that will receive the processed texture.</param>
        /// <param name="thickness">The thickness of the sobel lines.</param>
        /// <param name="threshold">The threshold used to determine an edge.</param>
        public void Process(GorgonTexture2DView texture, GorgonTexture2DReadWriteView outputTexture, int thickness, float threshold)
        {
            if ((texture == null)
                || (outputTexture == null))
            {
                return;
            }

            if ((_dispatch == null) || (_dispatch.ShaderResources[0] != texture) || (_dispatch.ReadWriteViews[0].ReadWriteView != outputTexture))
            {
                _dispatch = _dispatchBuilder.ReadWriteView(new GorgonReadWriteViewBinding(outputTexture))
                                            .ShaderResource(texture)
                                            .Build();
            }

            _sobelOptions[0] = thickness;
            _sobelOptions[1] = threshold;
            _sobelData.SetData<float>(_sobelOptions);

            // Send 32 threads per group.
            _compute.Execute(_dispatch, (int)(texture.Width / 32.0f).FastCeiling(), (int)(texture.Height / 32.0f).FastCeiling(), 1);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => _sobelData?.Dispose();
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
            if (sobelShader == null)
            {
                throw new ArgumentNullException(nameof(sobelShader));
            }

            _compute = new GorgonComputeEngine(graphics);
            _sobelData = new GorgonConstantBuffer(graphics,
                                                  new GorgonConstantBufferInfo("SobelData")
                                                  {
                                                      Usage = ResourceUsage.Dynamic,
                                                      SizeInBytes = 16
                                                  });

            _dispatchBuilder = new GorgonDispatchCallBuilder();
            _dispatchBuilder.ConstantBuffer(_sobelData.GetView())
                            .ComputeShader(sobelShader);
        }
        #endregion
    }
}
