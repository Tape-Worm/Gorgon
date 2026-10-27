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
using System.Numerics;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;

namespace Gorgon.Examples;

/// <summary>
/// Provides a visual wireframe box to display a frustum for debug purposes.
/// </summary>
public class GorgonFrustumVisual
    : IDisposable
{
    // The draw call to submit to the graphics interface.
    private readonly GorgonDrawCall _drawCall;
    // The vertex buffer.
    private readonly GorgonStructuredBufferView _vertexBuffer;
    // The vertices that make up the box.
    private readonly Vector4[] _lineVertices = new Vector4[24];

    /// <summary>Property to return the graphics interface that built this object.</summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    ///// <summary>
    ///// Function to initialize the visual.
    ///// </summary>
    //private void Initialize()
    //{
    //    var vertexBuffer = new GorgonVertexBuffer(Graphics, new GorgonVertexBufferInfo(_lineVertices.Length * Unsafe.SizeOf<Vector3>())
    //    {
    //        Name = "Frustum Visual VertexBuffer",                
    //        Usage = ResourceUsage.Dynamic
    //    });

    //    _vertexBuffer = new GorgonVertexBufferBinding(vertexBuffer, Unsafe.SizeOf<Vector3>());

    //    _constantBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, new GorgonConstantBufferInfo(Unsafe.SizeOf<Matrix4x4>())
    //    {
    //        Name = "Frustum Visual ConstantBufer",
    //        Usage = ResourceUsage.Dynamic
    //    });

    //    _vertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(Graphics, Resources.GraphicsShaders, "GorgonDebugVertexShader", GorgonGraphics.IsDebugEnabled);
    //    _pixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics, Resources.GraphicsShaders, "GorgonDebugPixelShader", GorgonGraphics.IsDebugEnabled);

    //    _inputLayout = new GorgonInputLayout(Graphics, "Frustum Line Vertex Input Layout", _vertexShader, new[]
    //    {
    //        new GorgonInputElement("SV_POSITION", Gorgon.Graphics.BufferFormat.R32G32B32_Float, 0)
    //    });

    //    var psoBuilder = new GorgonPipelineStateBuilder(Graphics);

    //    _drawCall = _builder.ConstantBuffer(ShaderType.Vertex, _constantBuffer)
    //                        .VertexBuffer(_inputLayout, _vertexBuffer)
    //                        .VertexRange(0, _lineVertices.Length)
    //                        .PipelineState(psoBuilder.VertexShader(_vertexShader)
    //                                                 .PixelShader(_pixelShader)
    //                                                 .RasterState(GorgonRasterState.NoCulling)
    //                                                 .PrimitiveType(PrimitiveType.LineList)
    //                                                 .Build())
    //                        .Build();
    //}

    /// <summary>
    /// Function to build the frustum visual.
    /// </summary>
    /// <param name="frustum">The frustum to visualize.</param>
    private void BuildBox(GorgonBoundingFrustum frustum)
    {
        // Left            
        _lineVertices[0] = new Vector4(frustum.Corners[FrustumCorner.BottomLeftFar], 1.0f);
        _lineVertices[2] = _lineVertices[1] = new Vector4(frustum.Corners[FrustumCorner.TopLeftFar], 1.0f);
        _lineVertices[4] = _lineVertices[3] = new Vector4(frustum.Corners[FrustumCorner.TopLeftNear], 1.0f);
        _lineVertices[6] = _lineVertices[5] = new Vector4(frustum.Corners[FrustumCorner.BottomLeftNear], 1.0f);
        _lineVertices[7] = _lineVertices[0];

        // Right
        _lineVertices[8] = new Vector4(frustum.Corners[FrustumCorner.BottomRightFar], 1.0f);
        _lineVertices[10] = _lineVertices[9] = new Vector4(frustum.Corners[FrustumCorner.TopRightFar], 1.0f);
        _lineVertices[12] = _lineVertices[11] = new Vector4(frustum.Corners[FrustumCorner.TopRightNear], 1.0f);
        _lineVertices[14] = _lineVertices[13] = new Vector4(frustum.Corners[FrustumCorner.BottomRightNear], 1.0f);
        _lineVertices[15] = _lineVertices[8];

        // Bottom
        _lineVertices[16] = new Vector4(frustum.Corners[FrustumCorner.BottomLeftFar], 1.0f);
        _lineVertices[17] = new Vector4(frustum.Corners[FrustumCorner.BottomRightFar], 1.0f);
        _lineVertices[18] = new Vector4(frustum.Corners[FrustumCorner.BottomLeftNear], 1.0f);
        _lineVertices[19] = new Vector4(frustum.Corners[FrustumCorner.BottomRightNear], 1.0f);

        // Top
        _lineVertices[20] = new Vector4(frustum.Corners[FrustumCorner.TopLeftFar], 1.0f);
        _lineVertices[21] = new Vector4(frustum.Corners[FrustumCorner.TopRightFar], 1.0f);
        _lineVertices[22] = new Vector4(frustum.Corners[FrustumCorner.TopLeftNear], 1.0f);
        _lineVertices[23] = new Vector4(frustum.Corners[FrustumCorner.TopRightNear], 1.0f);
    }

    /// <summary>
    /// Function to draw the frustum visualization.
    /// </summary>
    /// <param name="list">The command list to use for drawing.</param>
    /// <param name="frustum">The frustum to visualize.</param>
    /// <param name="viewMatrix">The current view matrix.</param>
    /// <param name="projectionMatrix">The current projection matrix.</param>
    public void Draw(GorgonCommandList list, GorgonBoundingFrustum frustum, in Matrix4x4 viewMatrix, in Matrix4x4 projectionMatrix)
    {
        BuildBox(frustum);

        GorgonGpuUploadMemory upload = new(Graphics, _vertexBuffer.Buffer.SizeInBytes);
        upload.WriteRange(_lineVertices);

        Matrix4x4 viewProj = Matrix4x4.Multiply(viewMatrix, projectionMatrix);

        DebugRenderData data = new()
        {
            ViewProjection = Matrix4x4.Transpose(viewProj),
            VertexBufferHandle = _vertexBuffer.GetViewHandle()
        };

        list.UploadGpuMemoryToBuffer(in upload, _vertexBuffer.Buffer)
            .WriteConstant(15, in data)
            .Draw(_drawCall);
    }

    /// <inheritdoc cref="GorgonAABBVisual.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _vertexBuffer.Dispose();
        }
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        //_vertexShader?.Dispose();
        //_pixelShader?.Dispose();
        //_constantBuffer?.Dispose();
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Initializes a new instance of the <see cref="GorgonFrustumVisual" /> class.</summary>
    /// <param name="graphics">The graphics interface to update.</param>
    /// <param name="psoFactory">The PSO factory used to cache/create PSOs.</param>
    public GorgonFrustumVisual(GorgonGraphics graphics, GorgonGraphicsPsoFactory psoFactory) 
    {
        Graphics = graphics;

        _vertexBuffer = GorgonStructuredBufferView.CreateStructuredBuffer<Vector4>(Graphics, "Gorgon Debug Frustum Visual VB", _lineVertices.Length);

        using GorgonShaderCompiler compiler = new(Graphics);
        GorgonShader vertexShader = compiler.Compile(Resources.Shaders, "GorgonDebugVertexShader", ShaderType.VertexShader);
        GorgonShader pixelShader = compiler.Compile(Resources.Shaders, "GorgonDebugPixelShader", ShaderType.PixelShader);

        GorgonGraphicsPsoBuilder psoBuilder = new(Graphics);
        psoBuilder.BlendState(GorgonBlendState.NoBlending)
                  .DepthStencilState(GorgonDepthStencilState.Default, BufferFormat.Unknown)
                  .RasterizerState(GorgonRasterState.NoCulling)
                  .OutputFormat(BufferFormat.R8G8B8A8_UNorm)
                  .PixelShader(pixelShader)
                  .PrimitiveType(PrimitiveType.LineList);

        GorgonGraphicsPso pso = psoFactory.CreateOrGetPso("Gorgon Debug Frustum PSO", vertexShader, psoBuilder);

        _drawCall = new GorgonDrawCall(_lineVertices.Length, pso);
        _drawCall.AssignBuffer(new GorgonUsedBuffer(_vertexBuffer, ShaderStage.Vertex, BufferUsage.ReadOnly));
    }
}
