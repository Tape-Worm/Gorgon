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
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Memory;

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
    private GorgonIndexedDrawCall? _drawCall;
    private GorgonGraphicsPso _pso;
    private readonly GorgonIndexBuffer _indexBuffer;
    private readonly GorgonStructuredBufferView _vertexBuffer;
    private GorgonBlendState _blendState = GorgonBlendState.NoBlending;
    private Matrix4x4 _projection;
    private readonly GorgonGraphicsPsoFactory _psoCache;

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _psoCache.Dispose();
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
        }
    }

    /// <summary>
    /// Function to build the pipeline state object for the blitter.
    /// </summary>
    /// <param name="blendState">The requested blend state.</param>
    [MemberNotNull(nameof(_pso))]
    private void BuildPSO(GorgonBlendState blendState)
    {
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

        GorgonGraphicsPso? pso = _psoCache.FindPso(p =>
        {
            if (!p.BlendStates[0].Equals(blendState))
            {
                return false;
            }

            if (_commandList.RenderTargets.Length != p.OutputFormats.Length)
            {
                return false;
            }

            if (p.Multisample.Equals(_commandList.RenderTargets[0]?.Texture.MultisampleInfo ?? GorgonMultisampleInfo.NoMultisampling))
            {
                return false;
            }

            for (int i = 0; i < p.OutputFormats.Length; ++i)
            {
                if ((_commandList.RenderTargets[i]?.Format ?? BufferFormat.Unknown) != p.OutputFormats[i])
                {
                    return false;
                }
            }

            return true;
        });

        if (pso is not null)
        {
            _pso = pso;
            _blendState = blendState;
            return;
        }

        GorgonGraphicsPsoBuilder psoFactory = new(_commandList.Graphics);
        psoFactory.PixelShader(_commandList.Graphics.SharedResources.BlitterPixelShader)
                  .OutputFormats(outputFormats)
                  .Multisample(_commandList.RenderTargets[0]?.Texture.MultisampleInfo ?? GorgonMultisampleInfo.NoMultisampling)
                  .BlendState(blendState);

        string name = $"Gorgon Blitter PSO - {_commandList.Name}:{Guid.NewGuid():N}";
        _pso = _psoCache.CreateOrGetPso(name, _commandList.Graphics.SharedResources.BlitterVertexShader, psoFactory);

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
        if (CheckPsoState(blendState))
        {
            BuildPSO(blendState);
            _blendState = blendState;
            _drawCall = null;
        }

        if ((_drawCall is null) || (_drawCall.UsedTextures[0].Texture != texture.Texture))
        {
            if (_drawCall is null)
            {            
                _drawCall = new GorgonIndexedDrawCall()
                {
                    IndexBuffer = _indexBuffer,
                    IndexCount = 6,
                    Pso = _pso
                };
            }
            else
            {
                _drawCall.Pso = _pso;
            }

            _drawCall.AssignBuffer(new GorgonDrawCallBuffer(_vertexBuffer, ShaderStage.Vertex, BufferUsage.VertexBuffer));
            _drawCall.AssignTexture(new GorgonDrawCallTexture(texture, ShaderStage.Pixel, TextureUsage.ReadOnly));
        }

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
        _psoCache = new GorgonGraphicsPsoFactory(_commandList.Graphics);

        BuildPSO(_blendState);

        Span<short> indices = [
            0, 1, 2, 2, 3, 0
        ];

        commandList.CopyRange(indices, _indexBuffer);
    }
}

