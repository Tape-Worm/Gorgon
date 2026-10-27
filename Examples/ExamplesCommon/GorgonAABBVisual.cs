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
using System.Collections.Generic;
using System.Numerics;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;

namespace Gorgon.Examples;

/// <summary>
/// Provides a visual wireframe box to display a bounding box for debug purposes.
/// </summary>
public partial class GorgonAABBVisual
    : IDisposable
{
    // The pipeline state object builder.
    private readonly GorgonGraphicsPsoBuilder _psoBuilder;
    // The pipeline state object factory.
    private readonly GorgonGraphicsPsoFactory _psoFactory;
    // The currently active pipeline state object.
    private GorgonGraphicsPso? _pso;
    // The last known depth/stencil state.
    private readonly Dictionary<GorgonDepthStencilState, GorgonGraphicsPso> _psoCache = [];
    // The draw call to submit to the graphics interface.
    private GorgonDrawCall? _drawCall;
    // The vertex buffer.
    private readonly GorgonStructuredBufferView _vertexBuffer;
    // The vertices that make up the box.
    private readonly Vector4[] _lineVertices = new Vector4[24];
    // The vertex shader.
    private readonly GorgonShader _vertexShader;

    /// <summary>Property to return the graphics interface that built this object.</summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <summary>
    /// Function to build a new PSO.
    /// </summary>
    /// <param name="depthStencilState">The depth/stencil state.</param>
    /// <param name="depthBufferFormat">The format of the depth/stencil buffer.</param>
    /// <returns>A new graphics PSO.</returns>
    private GorgonGraphicsPso BuildPso(GorgonDepthStencilState depthStencilState, BufferFormat depthBufferFormat)
    {
        if (_psoCache.TryGetValue(depthStencilState, out GorgonGraphicsPso? pso))
        {
            return pso;
        }

        _psoBuilder.DepthStencilState(depthStencilState, depthBufferFormat);
        pso = _psoFactory.CreateOrGetPso("Gorgon Debug AABB Visualization PSO", _vertexShader, _psoBuilder);
        _psoCache[depthStencilState] = pso;

        return pso;
    }

    /// <summary>
    /// Function to build the draw call.
    /// </summary>
    /// <param name="depthStencilState">The depth/stencil state to apply.</param>
    /// <param name="depthBufferFormat">The format of the depth/stencil buffer.</param>
    /// <returns>The draw call to use.</returns>
    private GorgonDrawCall BuildDrawCall(GorgonDepthStencilState depthStencilState, BufferFormat depthBufferFormat)
    {
        if ((_pso is null) || (_pso.DepthStencilState != depthStencilState))
        {
            _pso = BuildPso(depthStencilState, depthBufferFormat);
        }

        if (_drawCall is not null)
        {
            _drawCall.Pso = _pso;
            return _drawCall;
        }

        _drawCall = new GorgonDrawCall(_lineVertices.Length, _pso);
        _drawCall.AssignBuffer(new GorgonUsedBuffer(_vertexBuffer, ShaderStage.Vertex, BufferUsage.ReadOnly));
        return _drawCall;
    }

    /// <summary>
    /// I think we all know what this is for by now don't we?
    /// </summary>
    /// <param name="disposing">Seriously.</param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _vertexBuffer.Dispose();
        }
    }

    /// <summary>
    /// Function to build the AABB visual.
    /// </summary>
    /// <param name="aabb">The axis aligned bounding box to visualize.</param>
    private void BuildBox(in GorgonBoundingBox aabb)
    {
        // Left
        _lineVertices[0] = new Vector4(aabb.BottomLeftBack, 1.0f);
        _lineVertices[2] = _lineVertices[1] = new Vector4(aabb.TopLeftBack, 1.0f);
        _lineVertices[4] = _lineVertices[3] = new Vector4(aabb.TopLeftFront, 1.0f);
        _lineVertices[6] = _lineVertices[5] = new Vector4(aabb.BottomLeftFront, 1.0f);
        _lineVertices[7] = _lineVertices[0];

        // Right
        _lineVertices[8] = new Vector4(aabb.BottomRightBack, 1.0f);
        _lineVertices[10] = _lineVertices[9] = new Vector4(aabb.TopRightBack, 1.0f);
        _lineVertices[12] = _lineVertices[11] = new Vector4(aabb.TopRightFront, 1.0f);
        _lineVertices[14] = _lineVertices[13] = new Vector4(aabb.BottomRightFront, 1.0f);
        _lineVertices[15] = _lineVertices[8];

        // Bottom
        _lineVertices[16] = new Vector4(aabb.BottomLeftBack, 1.0f);
        _lineVertices[17] = new Vector4(aabb.BottomRightBack, 1.0f);
        _lineVertices[18] = new Vector4(aabb.BottomLeftFront, 1.0f);
        _lineVertices[19] = new Vector4(aabb.BottomRightFront, 1.0f);

        // Top
        _lineVertices[20] = new Vector4(aabb.TopLeftBack, 1.0f);
        _lineVertices[21] = new Vector4(aabb.TopRightBack, 1.0f);
        _lineVertices[22] = new Vector4(aabb.TopLeftFront, 1.0f);
        _lineVertices[23] = new Vector4(aabb.TopRightFront, 1.0f);
    }

    /// <summary>
    /// Function to draw the AABB visualization.
    /// </summary>
    /// <param name="list">The command list to use for drawing.</param>
    /// <param name="aabb">The axis aligned bounding box to visualize.</param>
    /// <param name="viewMatrix">The current view matrix.</param>
    /// <param name="projectionMatrix">The current projection matrix.</param>
    /// <param name="depthStencilState">[Optional] The depth/stencil state to apply when drawing.</param>
    public void Draw(GorgonCommandList list, in GorgonBoundingBox aabb, in Matrix4x4 viewMatrix, in Matrix4x4 projectionMatrix, GorgonDepthStencilState? depthStencilState = null)
    {
        BuildBox(in aabb);
        GorgonDrawCall drawCall = BuildDrawCall(depthStencilState ?? GorgonDepthStencilState.Default, list.DepthStencil?.Format ?? BufferFormat.Unknown);

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
            .Draw(drawCall);
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Initializes a new instance of the <see cref="GorgonAABBVisual" /> class.</summary>
    /// <param name="graphics">The graphics interface to update.</param>
    /// <param name="psoFactory">The PSO factory used to cache/create PSOs.</param>
    public GorgonAABBVisual(GorgonGraphics graphics, GorgonGraphicsPsoFactory psoFactory)
    {
        Graphics = graphics;
        _psoFactory = psoFactory;

        _vertexBuffer = GorgonStructuredBufferView.CreateStructuredBuffer<Vector4>(Graphics, "Gorgon Debug AABB Visual VB", _lineVertices.Length);

        using GorgonShaderCompiler compiler = new(Graphics);
        _vertexShader = compiler.Compile(Resources.Shaders, "GorgonDebugVertexShader", ShaderType.VertexShader);
        GorgonShader pixelShader = compiler.Compile(Resources.Shaders, "GorgonDebugPixelShader", ShaderType.PixelShader);

        _psoBuilder = new(Graphics);
        _psoBuilder.BlendState(GorgonBlendState.NoBlending)
                   .RasterizerState(GorgonRasterState.NoCulling)
                   .OutputFormat(BufferFormat.R8G8B8A8_UNorm)
                   .PixelShader(pixelShader)
                   .PrimitiveType(PrimitiveType.LineList);
    }
}
