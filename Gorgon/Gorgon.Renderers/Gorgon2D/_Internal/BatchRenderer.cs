#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: June 6, 2018 8:56:13 PM
// 
#endregion

using System.Runtime.CompilerServices;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.Renderers.Geometry;

namespace Gorgon.Renderers;

/// <summary>
/// The renderer for renderable objects.
/// </summary>
internal sealed class BatchRenderer
    : IDisposable
{
    #region Constants.
    /// <summary>
    /// Maximum number of renderables that can be buffered prior to sending to the GPU.
    /// We have 10,922 renderables (16 bit Index Buffer - 65536 bytes / 6 indices per renderable = ~10922).
    /// </summary>
    public const int MaxSpriteCount = 10922;
    // The maximum number of vertices we can render.
    private const int MaxVertexCount = MaxSpriteCount * 4;
    // The maximum number of indices we can render.
    private const int MaxIndexCount = MaxSpriteCount * 6;
    #endregion

    #region Variables.
    // Start with a modest cache size.
    private Gorgon2DVertex[] _vertexCache = new Gorgon2DVertex[4096];
    // The current vertex index.
    private int _currentVertexIndex;
    // The number of vertices allocated in the cache.
    private int _allocatedVertexCount;
    // The offset in the vertex buffer to start rendering at (in bytes).
    private int _vertexBufferByteOffset;
    // The starting index to render.
    private int _indexStart;
    // The number of indices queued.
    private int _indexCount;
    // The index of the vertex in the vertex buffer.
    private int _vertexBufferIndex;
    // The vertex index offset used by the index buffer to offset within the vertex buffer.
    private int _indexBufferBaseVertexIndex;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the graphics interface that built this object.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <summary>
    /// Property to return the transformer to use when updating the renderable.
    /// </summary>
    public SpriteTransformer SpriteTransformer
    {
        get;
    }

    /// <summary>
    /// Property to return the transformer to use when updating the renderable.
    /// </summary>
    public TextSpriteTransformer TextSpriteTransformer
    {
        get;
    }

    /// <summary>
    /// Property to return the vertex buffer used by the object renderer.
    /// </summary>
    public GorgonVertexBufferBinding VertexBuffer
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the index buffer used by the object renderer.
    /// </summary>
    public GorgonIndexBuffer IndexBuffer
    {
        get;
        private set;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to create the vertex/index buffers for the renderer.
    /// </summary>
    private void CreateBuffers()
    {
        // We don't need to update the index buffer ever.  So we can set up the indices right now.            
        using var indices = new GorgonNativeBuffer<int>(MaxSpriteCount * 6);
        int indexOffset = 0;
        ushort index = 0;
        for (int i = 0; i < MaxSpriteCount; ++i)
        {
            indices[indexOffset++] = index;
            indices[indexOffset++] = (index + 1);
            indices[indexOffset++] = (index + 2);
            indices[indexOffset++] = (index + 1);
            indices[indexOffset++] = (index + 3);
            indices[indexOffset++] = (index + 2);

            index += 4;
        }

        VertexBuffer = GorgonVertexBufferBinding.CreateVertexBuffer<Gorgon2DVertex>(Graphics,
                                                                                     new GorgonVertexBufferInfo(Gorgon2DVertex.SizeInBytes * (MaxSpriteCount * 4))
                                                                                     {
                                                                                         Usage = ResourceUsage.Dynamic,
                                                                                         Binding = VertexIndexBufferBinding.None                                                                                             
                                                                                     });

        IndexBuffer = new GorgonIndexBuffer(Graphics,
                                            new GorgonIndexBufferInfo(indices.Length)
                                            {
                                                Usage = ResourceUsage.Immutable,
                                                Binding = VertexIndexBufferBinding.None
                                            },
                                            indices.ToSpan());
    }

    /// <summary>
    /// Function to expand the vertex cache if it is full.
    /// </summary>
    /// <param name="increaseBy">The number of vertices to increase by.</param>
    private void ExpandCache(int increaseBy)
    {
        int newSize;

        if (increaseBy <= 0)
        {
            newSize = _vertexCache.Length + (_vertexCache.Length / 8);
        }
        else
        {
            newSize = _vertexCache.Length + increaseBy + (increaseBy / 4);
        }

        // Move to the next 
        newSize = ((newSize + 63) & ~63) * 4;

        Array.Resize(ref _vertexCache, newSize);
    }

    /// <summary>
    /// Function queue a renderable item for rendering.
    /// </summary>
    /// <param name="renderable">The renderable to queue.</param>
    public void QueueRenderable(BatchRenderable renderable)
    {
        Gorgon2DVertex[] vertices = renderable.Vertices;
        int vertexCount = renderable.ActualVertexCount;
        int lastVertex = _allocatedVertexCount + _currentVertexIndex + vertexCount;

        // Ensure we actually have the room to cache the renderable vertices.
        if (lastVertex >= _vertexCache.Length)
        {
            ExpandCache(lastVertex - _vertexCache.Length);
        }

        unsafe
        {
            fixed (void* destPtr = &_vertexCache[_currentVertexIndex])
            fixed (void* srcPtr = &vertices[0])
            {
                Unsafe.CopyBlock(destPtr, srcPtr, (uint)(Gorgon2DVertex.SizeInBytes * vertexCount));
            }
        }

        if (renderable.IndexCount != 0)
        {
            _indexCount += renderable.IndexCount;
        }

        _allocatedVertexCount += vertexCount;
        _currentVertexIndex += vertexCount;
    }

    /// <summary>
    /// Function to flush the cache data into the buffers and render the renderables.
    /// </summary>
    /// <param name="drawCall">The draw call that will be used to render the renderables.</param>
    /// <param name="blendFactor">The factor used to modulate the pixel shader, render target or both.</param>
    /// <param name="blendMask">The mask used to define which samples get updated in the active render targets.</param>
    /// <param name="stencilRef">The stencil reference value used when performing a stencil test.</param>
    public void RenderBatches(GorgonDrawCall drawCall, GorgonColor blendFactor, int blendMask, int stencilRef)
    {
        GorgonVertexBuffer vertexBuffer = VertexBuffer.VertexBuffer;
        int cacheIndex = _currentVertexIndex - _allocatedVertexCount;

        while (_allocatedVertexCount > 0)
        {
            int vertexCount = _allocatedVertexCount.Min(MaxVertexCount);
            int byteCount = Gorgon2DVertex.SizeInBytes * vertexCount;

            // If we've exceeded our vertex or index buffer size, we need to reset from the beginning.
            if ((_vertexBufferByteOffset + byteCount) >= vertexBuffer.SizeInBytes)
            {
                _indexStart = 0;
                _vertexBufferByteOffset = 0;
                _vertexBufferIndex = 0;
                _indexBufferBaseVertexIndex = 0;
            }

            CopyMode copyMode = _vertexBufferByteOffset == 0 ? CopyMode.Discard : CopyMode.NoOverwrite;

            // Copy a chunk of the cache.
            vertexBuffer.SetData<Gorgon2DVertex>(_vertexCache.AsSpan(cacheIndex, vertexCount), _vertexBufferByteOffset, copyMode);

            drawCall.VertexStartIndex = _vertexBufferIndex;
            drawCall.VertexCount = vertexCount;
            Graphics.Submit(drawCall, blendFactor, blendMask, stencilRef);

            // Move to the next block of bytes to render.
            _vertexBufferByteOffset += byteCount;
            _vertexBufferIndex += vertexCount;
            _allocatedVertexCount -= vertexCount;
            _indexBufferBaseVertexIndex += vertexCount;
            cacheIndex += vertexCount;
        }

        // Invalidate the cache.
        _currentVertexIndex = 0;
        _allocatedVertexCount = 0;
        _indexCount = 0;
    }

    /// <summary>
    /// Function to flush the cache data into the buffers and render the renderables.
    /// </summary>
    /// <param name="drawCall">The draw call that will be used to render the renderables.</param>
    /// <param name="blendFactor">The factor used to modulate the pixel shader, render target or both.</param>
    /// <param name="blendMask">The mask used to define which samples get updated in the active render targets.</param>
    /// <param name="stencilRef">The stencil reference value used when performing a stencil test.</param>
    public void RenderBatches(GorgonDrawIndexCall drawCall, GorgonColor blendFactor, int blendMask, int stencilRef)
    {
        GorgonVertexBuffer vertexBuffer = VertexBuffer.VertexBuffer;
        int cacheIndex = _currentVertexIndex - _allocatedVertexCount;

        while (_allocatedVertexCount > 0)
        {
            int vertexCount = _allocatedVertexCount.Min(MaxVertexCount);
            int indexCount = _indexCount.Min(MaxIndexCount);
            int byteCount = Gorgon2DVertex.SizeInBytes * vertexCount;

            // If we've exceeded our vertex or index buffer size, we need to reset from the beginning.
            if (((_vertexBufferByteOffset + byteCount) >= vertexBuffer.SizeInBytes)
                || ((_indexStart + _indexCount) >= MaxIndexCount))
            {
                _indexStart = 0;
                _vertexBufferByteOffset = 0;
                _vertexBufferIndex = 0;
                _indexBufferBaseVertexIndex = 0;
            }

            CopyMode copyMode = _vertexBufferByteOffset == 0 ? CopyMode.Discard : CopyMode.NoOverwrite;

            // Copy a chunk of the cache.
            vertexBuffer.SetData<Gorgon2DVertex>(_vertexCache.AsSpan(cacheIndex, vertexCount), _vertexBufferByteOffset, copyMode);

            drawCall.BaseVertexIndex = _indexBufferBaseVertexIndex;
            drawCall.IndexStart = _indexStart;
            drawCall.IndexCount = indexCount;
            Graphics.Submit(drawCall, blendFactor, blendMask, stencilRef);

            // Move to the next block of bytes to render.
            _vertexBufferByteOffset += byteCount;
            _vertexBufferIndex += vertexCount;
            _allocatedVertexCount -= vertexCount;
            cacheIndex += vertexCount;
            _indexStart += indexCount;
            _indexCount -= indexCount;
        }

        // Invalidate the cache.
        _currentVertexIndex = 0;
        _allocatedVertexCount = 0;
        _indexCount = 0;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        VertexBuffer.VertexBuffer?.Dispose();
        IndexBuffer?.Dispose();
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchRenderer"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface that uses this renderer.</param>
    public BatchRenderer(GorgonGraphics graphics)
    {
        Graphics = graphics;
        SpriteTransformer = new SpriteTransformer();
        TextSpriteTransformer = new TextSpriteTransformer();
        CreateBuffers();
    }
    #endregion
}
