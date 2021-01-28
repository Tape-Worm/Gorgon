#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: January 15, 2021 2:48:43 PM
// 
#endregion

using System;
using System.Runtime.CompilerServices;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Memory;
using DX = SharpDX;

namespace Gorgon.Renderers.Debug
{
    /// <summary>
    /// Provides a visual wireframe box to display a frustum for debug purposes.
    /// </summary>
    public class GorgonFrustumVisual
        : IGorgonGraphicsObject, IDisposable
    {
        #region Variables.        
        // The builder used to build draw calls.
        private readonly GorgonDrawCallBuilder _builder = new GorgonDrawCallBuilder();
        // The draw call to submit to the graphics interface.
        private GorgonDrawCall _drawCall;
        // The layout of the vertices.
        private GorgonInputLayout _inputLayout;
        // The vertex buffer.
        private GorgonVertexBufferBinding _vertexBuffer;
        // The constant buffer.
        private GorgonConstantBufferView _constantBuffer;
        // The vertices that make up the box.
        private readonly DX.Vector3[] _lineVertices = new DX.Vector3[24];
        // The vertex shader.
        private GorgonVertexShader _vertexShader;
        // The pixel shader.
        private GorgonPixelShader _pixelShader;
        #endregion

        #region Properties.
        /// <summary>Property to return the graphics interface that built this object.</summary>
        public GorgonGraphics Graphics
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to initialize the visual.
        /// </summary>
        private void Initialize()
        {
            var vertexBuffer = new GorgonVertexBuffer(Graphics, new GorgonVertexBufferInfo("Frustum Visual VertexBuffer")
            {
                SizeInBytes = _lineVertices.Length * Unsafe.SizeOf<DX.Vector3>(),
                Usage = ResourceUsage.Dynamic
            });

            _vertexBuffer = new GorgonVertexBufferBinding(vertexBuffer, Unsafe.SizeOf<DX.Vector3>());

            _constantBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, new GorgonConstantBufferInfo("Frustum Visual ConstantBufer")
            {
                SizeInBytes = Unsafe.SizeOf<DX.Matrix>(),
                Usage = ResourceUsage.Dynamic
            });

            _vertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(Graphics, Resources.GraphicsShaders, "GorgonDebugVertexShader", GorgonGraphics.IsDebugEnabled);
            _pixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics, Resources.GraphicsShaders, "GorgonDebugPixelShader", GorgonGraphics.IsDebugEnabled);

            _inputLayout = new GorgonInputLayout(Graphics, "Frustum Line Vertex Input Layout", _vertexShader, new[]
            {
                new GorgonInputElement("SV_POSITION", Gorgon.Graphics.BufferFormat.R32G32B32_Float, 0)
            });

            var psoBuilder = new GorgonPipelineStateBuilder(Graphics);

            _drawCall = _builder.ConstantBuffer(ShaderType.Vertex, _constantBuffer)
                                .VertexBuffer(_inputLayout, _vertexBuffer)
                                .VertexRange(0, _lineVertices.Length)
                                .PipelineState(psoBuilder.VertexShader(_vertexShader)
                                                         .PixelShader(_pixelShader)
                                                         .RasterState(GorgonRasterState.NoCulling)
                                                         .PrimitiveType(PrimitiveType.LineList)
                                                         .Build())
                                .Build();
        }

        /// <summary>
        /// Function to build the frustum visual.
        /// </summary>
        /// <param name="frustum">The frustum to visualize.</param>
        private void BuildBox(ref DX.BoundingFrustum frustum)
        {
            DX.Vector3[] corners = GorgonArrayPool<DX.Vector3>.SharedTiny.Rent(8);
                        
            try
            {
                // 0 - Near, right, bottom
                // 1 - Near, right, top
                // 2 - Near, left, top
                // 3 - Near, left, bottom
                // 4 - Far, right, bottom
                // 5 - Far, right, top
                // 6 - Far, left, top
                // 7 - Far, left, bottom

                frustum.GetCorners(corners);

                // Left            
                _lineVertices[0] = corners[2]; 
                _lineVertices[2] = _lineVertices[1] = corners[6];
                _lineVertices[4] = _lineVertices[3] = corners[7]; 
                _lineVertices[6] = _lineVertices[5] = corners[3];
                _lineVertices[7] = _lineVertices[0];

                // Right
                _lineVertices[8] = corners[1];
                _lineVertices[10] = _lineVertices[9] = corners[5];
                _lineVertices[12] = _lineVertices[11] = corners[4];
                _lineVertices[14] = _lineVertices[13] = corners[0];                
                _lineVertices[15] = _lineVertices[8];

                // Bottom
                _lineVertices[16] = corners[7];
                _lineVertices[17] = corners[4];
                _lineVertices[18] = corners[3];
                _lineVertices[19] = corners[0];

                // Top
                _lineVertices[20] = corners[6];
                _lineVertices[21] = corners[5];
                _lineVertices[22] = corners[2];
                _lineVertices[23] = corners[1];
            }
            finally
            {
                GorgonArrayPool<DX.Vector3>.SharedTiny.Return(corners);
            }
        }

        /// <summary>
        /// Function to draw the frustum visualization.
        /// </summary>
        /// <param name="frustum">The frustum to visualize.</param>
        /// <param name="viewMatrix">The current view matrix.</param>
        /// <param name="projectionMatrix">The current projection matrix.</param>
        public void Draw(DX.BoundingFrustum frustum, ref DX.Matrix viewMatrix, ref DX.Matrix projectionMatrix)
        {
            BuildBox(ref frustum);

            _vertexBuffer.VertexBuffer.SetData<DX.Vector3>(_lineVertices);

            DX.Matrix.Multiply(ref projectionMatrix, ref viewMatrix, out DX.Matrix viewProj);
            DX.Matrix.Transpose(ref viewProj, out viewProj);
            
            _constantBuffer.Buffer.SetData(ref viewProj);

            Graphics.Submit(_drawCall);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _vertexShader?.Dispose();
            _pixelShader?.Dispose();
            _constantBuffer?.Dispose();
            _vertexBuffer.VertexBuffer?.Dispose();
            _inputLayout?.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="GorgonFrustumVisual" /> class.</summary>
        /// <param name="graphics">The graphics interface to update.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
        public GorgonFrustumVisual(GorgonGraphics graphics)
        {
            Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            Initialize();
        }
        #endregion
    }
}
