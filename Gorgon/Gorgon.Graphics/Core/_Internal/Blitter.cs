// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: October 20, 2025 5:57:46 PM
//

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Gorgon.Graphics.Core;

public unsafe class Blitter
    : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Vertex
    {
        public static readonly int SizeInBytes = sizeof(Vertex);

        public Vector2 Position;
        public Vector2 UV;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RenderData
    {
        public Matrix4x4 Wvp;
        public int VertexBufferHandle;
        public int TextureHandle;
        public int SamplerHandle;
    }

    private Vector2 _viewDimensions;
    private readonly GorgonCommandList _commandList;
    private GorgonDrawCall? _drawCall;
    private GorgonGraphicsPso _pso;
    private readonly GorgonIndexBuffer _indexBuffer;
    private readonly GorgonStructuredBufferView _vertexBuffer;
    private GorgonBlendState _blendState = GorgonBlendState.NoBlending;
    private readonly (IGorgonTextureView<GorgonTextureCommon>, ShaderStage, TextureUsage)[] _texture = new (IGorgonTextureView<GorgonTextureCommon>, ShaderStage, TextureUsage)[1];
    private Matrix4x4 _projection;    

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _pso.Dispose();
        }
    }

    /// <summary>
    /// Function to build the pipeline state object for the blitter.
    /// </summary>
    /// <param name="blendState">The requested blend state.</param>
    [MemberNotNull(nameof(_pso))]
    private void BuildPSO(GorgonBlendState blendState)
    {
        _pso?.Dispose();

        Span<BufferFormat> outputFormats = stackalloc BufferFormat[_commandList.RenderTargets.Length];        

        if (_commandList.RenderTargets.Length > 1)
        {
            for (int i = 0; i < outputFormats.Length; ++i)
            {
                outputFormats[i] = _commandList.RenderTargets[i]?.Format ?? BufferFormat.Unknown;
            }
        }
        else
        {
            outputFormats[0] = _commandList.RenderTargets[0]?.Format ?? BufferFormat.Unknown;
        }

        GorgonGraphicsPsoBuilder psoFactory = new(_commandList.Graphics);
        _pso = psoFactory.PixelShader(_commandList.Graphics.SharedResources.BlitterPixelShader)
                         .OutputFormats(outputFormats)
                         .BlendState(blendState)
                         .Build($"Gorgon Blitter PSO - {_commandList.Name}", _commandList.Graphics.SharedResources.BlitterVertexShader);

        _blendState = blendState;
    }

    /// <summary>Function to update the projection matrix.</summary>
    /// <param name="projectionMatrix">The instance of the matrix to update.</param>
    private void UpdateProjectionMatrix(ref Matrix4x4 projectionMatrix)
    {
        const float zRange = 1.0f;
        const float left = 0;
        const float top = 0;
        float right = _commandList.Viewports[0].Width;
        float bottom = _commandList.Viewports[0].Height;

        projectionMatrix = Matrix4x4.Identity;
        projectionMatrix.M11 = 2.0f / (right - left);
        projectionMatrix.M22 = 2.0f / (top - bottom);
        projectionMatrix.M33 = zRange;
        projectionMatrix.M41 = (left + right) / (left - right);
        projectionMatrix.M42 = (top + bottom) / (bottom - top);

        _viewDimensions = new Vector2(_commandList.Viewports[0].Width, _commandList.Viewports[0].Height);
    }

    private bool CheckPsoState(GorgonBlendState blendState)
    {
        if (((_pso.OutputFormats.Length != _commandList.RenderTargets.Length) || (!_blendState.Equals(blendState)))
            || (!_pso.Multisample.Equals(_commandList.RenderTargets[0]?.Texture.MultisampleInfo)))
        {
            return true;
        }

        for (int i = 0; i < _commandList.RenderTargets.Length; i++)
        {
            if (_pso.OutputFormats[i] != _commandList.RenderTargets[i]?.Format)
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Draw(IGorgonTextureView<GorgonTextureCommon> texture, GorgonRectangleF destination, GorgonRectangleF textureCoordinates, GorgonSampler sampler, GorgonBlendState blendState)
    {
        _drawCall ??= new GorgonDrawCall(_indexBuffer, 6, _pso)
        {
            IndexBuffer = _indexBuffer,
            UsedBuffers = [(_vertexBuffer.Buffer, ShaderStage.Vertex, BufferUsage.VertexBuffer)]
        };

        if (CheckPsoState(blendState))
        {
            BuildPSO(blendState);
            _blendState = blendState;
            _drawCall.Pso = _pso;
        }

        _texture[0] = (texture, ShaderStage.Pixel, TextureUsage.ReadOnly);
        _drawCall.UsedTextures = _texture;

        if ((_commandList.Viewports[0].Width != _viewDimensions.X)
            || (_commandList.Viewports[0].Height != _viewDimensions.Y))
        {
            UpdateProjectionMatrix(ref _projection);
        }

        RenderData data = new()
        {
            Wvp = _projection,
            VertexBufferHandle = _vertexBuffer.GetViewHandle(),
            TextureHandle = texture.GetViewHandle(),
            SamplerHandle = sampler.GetViewHandle()
        };

        Span<Vertex> vertices = [
            new Vertex
                {
                    Position = destination.TopLeft,
                    UV = textureCoordinates.TopLeft
                },
                new Vertex
                {
                    Position = destination.TopRight,
                    UV = textureCoordinates.TopRight
                },
                new Vertex
                {
                    Position = destination.BottomRight,
                    UV = textureCoordinates.BottomRight
                },
                new Vertex
                {
                    Position = destination.BottomLeft,
                    UV = textureCoordinates.BottomLeft
                }
        ];

        GorgonGpuUploadMemory uploader = new(_commandList.Graphics, _vertexBuffer.Buffer.SizeInBytes);
        uploader.WriteRange(vertices);

        _commandList.UploadGpuMemoryToBuffer(in uploader, _vertexBuffer.Buffer)
                    .WriteConstant(0, in data)
                    .Draw(_drawCall);
    }

    public Blitter(GorgonCommandList commandList)
    {
        _commandList = commandList;

        _indexBuffer = new GorgonIndexBuffer(_commandList.Graphics, "Gorgon Blitter Index Buffer", new GorgonIndexBufferInfo(sizeof(short) * 6, false));
        _vertexBuffer = GorgonStructuredBufferView.CreateStructuredBuffer<Vertex>(commandList.Graphics, "Gorgon Blitter Vertex Buffer", 4);

        BuildPSO(_blendState);

        Span<short> indices = [
            0, 1, 2, 2, 3, 0
        ];

        commandList.CopyRange(indices, _indexBuffer);
    }
}

