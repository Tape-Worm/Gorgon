﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: January 15, 2021 2:48:43 PM
// 

using System.Numerics;
using System.Runtime.CompilerServices;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Core.Properties;

namespace Gorgon.Renderers.Debug;

/// <summary>
/// Provides a visual wireframe box to display a bounding box for debug purposes
/// </summary>
public class GorgonAABBVisual
    : IGorgonGraphicsObject, IDisposable
{

    // The builders used to build draw calls.
    private readonly GorgonDrawCallBuilder _builder = new();
    private GorgonPipelineStateBuilder _psoBuilder;
    // The draw call to submit to the graphics interface.
    private GorgonDrawCall _drawCall;
    // The layout of the vertices.
    private GorgonInputLayout _inputLayout;
    // The vertex buffer.
    private GorgonVertexBufferBinding _vertexBuffer;
    // The constant buffer.
    private GorgonConstantBufferView _constantBuffer;
    // The vertices that make up the box.
    private readonly Vector3[] _lineVertices = new Vector3[24];
    // The vertex shader.
    private GorgonVertexShader _vertexShader;
    // The pixel shader.
    private GorgonPixelShader _pixelShader;

    /// <summary>Property to return the graphics interface that built this object.</summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <summary>
    /// Function to build the draw call.
    /// </summary>
    /// <param name="depthStencilState">The depth/stencil state to apply.</param>
    private void BuildDrawCall(GorgonDepthStencilState depthStencilState) => _drawCall = _builder.ConstantBuffer(ShaderType.Vertex, _constantBuffer)
                            .VertexBuffer(_inputLayout, _vertexBuffer)
                            .VertexRange(0, _lineVertices.Length)
                            .PipelineState(_psoBuilder.VertexShader(_vertexShader)
                                                     .PixelShader(_pixelShader)
                                                     .RasterState(GorgonRasterState.NoCulling)
                                                     .PrimitiveType(PrimitiveType.LineList)
                                                     .DepthStencilState(depthStencilState)
                                                     .Build())
                            .Build();

    /// <summary>
    /// Function to initialize the visual.
    /// </summary>
    private void Initialize()
    {
        GorgonVertexBuffer vertexBuffer = new(Graphics, new GorgonVertexBufferInfo(_lineVertices.Length * Unsafe.SizeOf<Vector3>())
        {
            Name = "AABB Visual VertexBuffer",
            Usage = ResourceUsage.Dynamic
        });

        _vertexBuffer = new GorgonVertexBufferBinding(vertexBuffer, Unsafe.SizeOf<Vector3>());

        _constantBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, new GorgonConstantBufferInfo(Unsafe.SizeOf<Matrix4x4>())
        {
            Name = "AABB Visual ConstantBufer",
            Usage = ResourceUsage.Dynamic
        });

        _vertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(Graphics, Resources.GraphicsShaders, "GorgonDebugVertexShader", GorgonGraphics.IsDebugEnabled);
        _pixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics, Resources.GraphicsShaders, "GorgonDebugPixelShader", GorgonGraphics.IsDebugEnabled);

        _inputLayout = new GorgonInputLayout(Graphics, "AABB Line Vertex Input Layout", _vertexShader,
        [
            new GorgonInputElement("SV_POSITION", Gorgon.Graphics.BufferFormat.R32G32B32_Float, 0)
        ]);

        _psoBuilder = new GorgonPipelineStateBuilder(Graphics);

        BuildDrawCall(null);
    }

    /// <summary>
    /// Function to build the AABB visual.
    /// </summary>
    /// <param name="aabb">The axis aligned bounding box to visualize.</param>
    private void BuildBox(ref readonly GorgonBoundingBox aabb)
    {
        // Left
        _lineVertices[0] = aabb.BottomLeftBack;
        _lineVertices[2] = _lineVertices[1] = aabb.TopLeftBack;
        _lineVertices[4] = _lineVertices[3] = aabb.TopLeftFront;
        _lineVertices[6] = _lineVertices[5] = aabb.BottomLeftFront;
        _lineVertices[7] = _lineVertices[0];

        // Right
        _lineVertices[8] = aabb.BottomRightBack;
        _lineVertices[10] = _lineVertices[9] = aabb.TopRightBack;
        _lineVertices[12] = _lineVertices[11] = aabb.TopRightFront;
        _lineVertices[14] = _lineVertices[13] = aabb.BottomRightFront;
        _lineVertices[15] = _lineVertices[8];

        // Bottom
        _lineVertices[16] = aabb.BottomLeftBack;
        _lineVertices[17] = aabb.BottomRightBack;
        _lineVertices[18] = aabb.BottomLeftFront;
        _lineVertices[19] = aabb.BottomRightFront;

        // Top
        _lineVertices[20] = aabb.TopLeftBack;
        _lineVertices[21] = aabb.TopRightBack;
        _lineVertices[22] = aabb.TopLeftFront;
        _lineVertices[23] = aabb.TopRightFront;
    }

    /// <summary>
    /// Function to draw the AABB visualization.
    /// </summary>
    /// <param name="aabb">The axis aligned bounding box to visualize.</param>
    /// <param name="viewMatrix">The current view matrix.</param>
    /// <param name="projectionMatrix">The current projection matrix.</param>
    /// <param name="depthStencilState">[Optional] The depth/stencil state to apply when drawing.</param>
    public void Draw(ref readonly GorgonBoundingBox aabb, ref readonly Matrix4x4 viewMatrix, ref readonly Matrix4x4 projectionMatrix, GorgonDepthStencilState depthStencilState = null)
    {
        if ((_drawCall is not null) && (_drawCall.PipelineState.DepthStencilState != depthStencilState))
        {
            BuildDrawCall(depthStencilState);
        }

        BuildBox(in aabb);

        _vertexBuffer.VertexBuffer.SetData<Vector3>(_lineVertices);

        Matrix4x4 viewProj = Matrix4x4.Multiply(viewMatrix, projectionMatrix);
        viewProj = Matrix4x4.Transpose(viewProj);
        _constantBuffer.Buffer.SetData(in viewProj);

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

    /// <summary>Initializes a new instance of the <see cref="GorgonAABBVisual" /> class.</summary>
    /// <param name="graphics">The graphics interface to update.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
    public GorgonAABBVisual(GorgonGraphics graphics)
    {
        Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        Initialize();
    }
}
