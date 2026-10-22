
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: July 30, 2017 1:49:31 PM
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
//using Gorgon.Renderers.Cameras;
//using Gorgon.Renderers.Data;
//using Gorgon.Renderers.Debug;
//using Gorgon.Renderers.Geometry;
//using Gorgon.Renderers.Lights;

namespace Gorgon.Examples;

/// <summary>
/// A simple renderer used to display and animate the scene data
/// </summary>
internal class SimpleRenderer
    : IDisposable
{
    /// <summary>
    /// Per-frame data to send to the GPU.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct RenderData
    {
        /// <summary>
        /// The view/projection matrix.
        /// </summary>
        public Matrix4x4 ViewProjection;
        /// <summary>
        /// The position of the camera.
        /// </summary>
        public Vector4 CameraPosition;
        /// <summary>
        /// The handle for the default sampler.
        /// </summary>
        public int SamplerHandle;
        /// <summary>
        /// The handle for the buffer that contains the light data.
        /// </summary>
        public int LightBufferHandle;
    }

    /// <summary>
    /// Per object data to send to the GPU.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct ObjectData
    {
        /// <summary>
        /// The world matrix for the object.
        /// </summary>
        public Matrix4x4 World;
        /// <summary>
        /// The UV offset for the object's texture.
        /// </summary>
        public Vector2 UvOffset;
        /// <summary>
        /// Specular power.
        /// </summary>
        public float SpecularPower;
        /// <summary>
        /// The handle to the object's vertex buffer.
        /// </summary>
        public int VertexBufferHandle;
        /// <summary>
        /// The handle to the object's texture.
        /// </summary>
        public int TextureHandle;
        /// <summary>
        /// The handle to the object's normal map texture.
        /// </summary>
        public int NormalMapHandle;
        /// <summary>
        /// The handle to the object's specular map texture..
        /// </summary>
        public int SpecularHandle;
    }

    // The maximum number of available lights.
    private const int MaxLights = 8;

    // The graphics interface to use.
    private readonly GorgonGraphics _graphics;

    // The swap chain used to display our rendering.
    private readonly GorgonSwapChain _swapChain;
    // The depth/stencil buffer for the scene.
    private GorgonDepthStencilView _depthStencil;
    // The constant buffer to hold the light list.
    private readonly GorgonConstantBufferView _lightBuffer;
    // The meshes to render.
    private readonly List<Mesh> _meshes = [];
    // The lights in the scene.
    private readonly List<GorgonLightCommon> _lights = [];
    // Flag to indicate that the lights buffer needs updating.
    private bool _updateLights = true;
    // The list of draw calls to make.
    private readonly List<GorgonIndexedDrawCall> _drawCalls = [];
    // The frustum for the camera.
    private readonly GorgonBoundingFrustum _frustum = new();
    // Per frame rendering data.
    private RenderData _renderData;
    // The visual for AABBs.
    private readonly GorgonAABBVisual _aabbVisual;

    /// <summary>
    /// Property to return the camera for the renderer.
    /// </summary>
    public GorgonPerspectiveCamera Camera
    {
        get;
    }

    /// <summary>
    /// Property to return the factory cache for the pipeline state objects.
    /// </summary>
    public GorgonGraphicsPsoFactory PsoFactory
    {
        get;
    }

    /// <summary>
    /// Property to set or return the texture to use as a default fallback.
    /// </summary>
    public IGorgonTextureView<GorgonTexture>? DefaultTexture
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether to show the axis aligned bounding boxes for the objects.
    /// </summary>
    public bool ShowAABB
    {
        get;
        set;
    }

    /// <inheritdoc cref="Primitives.Dispose(bool)">
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _lightBuffer.Dispose();
            _depthStencil.Dispose();
            _swapChain.Dispose();
            PsoFactory.Dispose();
        }
    }

    /// <summary>
    /// Function to perform the final set up for the scene.
    /// </summary>
    /// <param name="list">The command list to use.</param>
    private void UpdateLights(GorgonCommandList list)
    {
        if (!_updateLights)
        {
            return;
        }

        Span<GorgonGpuLightData> lights = stackalloc GorgonGpuLightData[MaxLights];

        for (int i = 0; i < MaxLights.Min(_lights.Count); i++)
        {
            lights[i] = _lights[i].GetGpuData();
            list.CopyRange(lights, _lightBuffer.Buffer);
        }

        _updateLights = false;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to render the scene.
    /// </summary>
    public void Render()
    {
        if (Camera.Changes != CameraChange.None)
        {
            ref Matrix4x4 viewProjection = ref _renderData.ViewProjection;
            ref Vector4 camPos = ref _renderData.CameraPosition;

            if ((Camera.Changes & CameraChange.View) == CameraChange.View)
            {
                camPos = new Vector4(Camera.Position, 1.0f);
            }

            ref readonly Matrix4x4 view = ref Camera.GetViewMatrix();
            ref readonly Matrix4x4 proj = ref Camera.GetProjectionMatrix();

            viewProjection = Matrix4x4.Multiply(view, proj);
            _frustum.Update(in viewProjection);
        }

        _swapChain.WaitForFrameLatency();

        GorgonCommandList list = _graphics.GetCommandList("Primitives Commandlist")
                                          .AddPresenter(_swapChain)
                                          .SetRenderTarget(_swapChain, _depthStencil)
                                          .SetScissorRectangle(new GorgonRectangle(0, 0, _swapChain.Width, _swapChain.Height))
                                          .SetViewport(new GorgonViewport(0, 0, _swapChain.Width, _swapChain.Height))
                                          .ClearRenderTarget(_swapChain, GorgonColors.CornFlowerBlue)
                                          .ClearDepthStencil(_depthStencil, 1.0f, 0)
                                          .WriteConstant(0, in _renderData);

        UpdateLights(list);

        int blackHandle = DefaultTexture?.GetViewHandle() ?? -1;

        if (blackHandle == -1)
        {
            throw new KeyNotFoundException("The default texture was not found.");
        }

        for (int i = 0; i < _meshes.Count; ++i)
        {
            Mesh mesh = _meshes[i];

            if ((mesh.Material is null) || (mesh.VertexBuffer is null))
            {
                continue;
            }

            ref readonly GorgonBoundingBox aabb = ref mesh.Aabb;
            GorgonBoundingBox worldaabb = default;

            if (mesh is MoveableMesh movableMesh)
            {
                Matrix4x4 translation;
                scoped ref readonly Matrix4x4 matrix = ref movableMesh.WorldMatrix;

                if (mesh is Sphere or IcoSphere)
                {
                    translation = Matrix4x4.CreateTranslation(movableMesh.Position);
                    matrix = ref translation;
                }

                GorgonBoundingBox.Transform(in aabb, in matrix, out worldaabb);
            }

            if (!GorgonIntersections.FrustumIntersectsBox(_frustum, in worldaabb))
            {
                continue;
            }

            int textureHandle = mesh.Material.Textures[0].GetViewHandle();
            int normalHandle = mesh.Material.Textures[1]?.GetViewHandle() ?? blackHandle;
            int specularHandle = mesh.Material.Textures[2]?.GetViewHandle() ?? blackHandle;

            ObjectData objectData = new()
            {
                World = mesh is MoveableMesh mover ? mover.WorldMatrix : Matrix4x4.Identity,
                SpecularPower = mesh.Material.SpecularPower,
                UvOffset = mesh.Material.TextureOffset,
                TextureHandle = textureHandle,
                NormalMapHandle = normalHandle,
                SpecularHandle = specularHandle,
                VertexBufferHandle = mesh.VertexBuffer.GetViewHandle()
            };

            list.WriteConstant(1, in objectData);
            list.Draw(_drawCalls[i]);

            if (ShowAABB)
            {
                _aabbVisual.Draw(list, in worldaabb, in Camera.GetViewMatrix(), in Camera.GetProjectionMatrix(), mesh.Material.PipelineState.DepthStencilState);
            }
        }

        GorgonExample.BlitLogo(list);

        _graphics.Submit(list);
    }

    /// <summary>
    /// Function to add a light to the scene.
    /// </summary>
    /// <param name="light">The light to add.</param>
    public void AddLight(GorgonLightCommon light)
    {
        _lights.Add(light);
        _updateLights = true;
    }

    /// <summary>
    /// Function to add a mesh to the renderer.
    /// </summary>
    /// <param name="mesh">The mesh to add to the scene.</param>
    public void AddMesh(Mesh mesh)
    {
        if ((mesh.IndexBuffer is null) || (mesh.Material is null) || (mesh.VertexBuffer is null))
        {
            throw new ArgumentException("The mesh is not valid.", nameof(mesh));
        }

        _meshes.Add(mesh);

        GorgonIndexedDrawCall drawCall = new(mesh.IndexCount, mesh.IndexBuffer, mesh.Material.PipelineState);
        drawCall.UsedBuffers.Add(new GorgonUsedBuffer(mesh.VertexBuffer, ShaderStage.Vertex, BufferUsage.ReadOnly));
        drawCall.UsedBuffers.Add(new GorgonUsedBuffer(_lightBuffer, ShaderStage.Pixel, BufferUsage.ConstantBuffer));

        for (int i = 0; i < mesh.Material.Textures.Length; ++i)
        {
            if (mesh.Material.Textures[i] is not null)
            {
                drawCall.UsedTextures.Add(new GorgonUsedTexture(mesh.Material.Textures[i], ShaderStage.Pixel, TextureUsage.ReadOnly));
            }
        }

        drawCall.UsedTextures.Add(new GorgonUsedTexture(DefaultTexture?.Texture ?? throw new NullReferenceException(), ShaderStage.Pixel, TextureUsage.ReadOnly));

        _drawCalls.Add(drawCall);
    }

    /// <summary>
    /// Function to upload mesh data to the GPU.
    /// </summary>
    public void UploadMeshData()
    {
        GorgonCommandList uploadList = _graphics.GetCommandList("Primitives Upload");

        foreach (Mesh mesh in _meshes)
        {
            if (mesh.VertexBuffer is null)
            {
                continue;
            }

            uploadList.CopyRange(mesh.Vertices, mesh.VertexBuffer.Buffer);

            if (mesh.IndexBuffer is not null)
            {
                uploadList.CopyRange(mesh.Indices, mesh.IndexBuffer);
            }
        }

        _graphics.Submit(uploadList);

        // Wait for the GPU so we can avoid a potential race condition.
        _graphics.WaitForGpu();
    }

    /// <summary>
    /// Function to resize the swap chain and associated back buffer.
    /// </summary>
    /// <param name="width">The width of the swap chain.</param>
    /// <param name="height">The height of the swap chain.</param>
    public void ResizeSwapChain(int width, int height)
    {
        _swapChain.Resize(width, height);
        _depthStencil.Dispose();
        _depthStencil = GorgonDepthStencilView.CreateDepthStencilView(_graphics, "Primitives Depth/Stencil Buffer", BufferFormat.D24_UNorm_S8_UInt, _swapChain.Width, _swapChain.Height);
        Camera.ViewDimensions = new Vector2(_swapChain.Width, _swapChain.Height);
        Camera.Fov = (2.0f * ((75.0f / 2).ToRadians().Tan() / Camera.AspectRatio)).ToDegrees();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleRenderer"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface to use for rendering.</param>
    /// <param name="window">The window for the application.</param>
    public SimpleRenderer(GorgonGraphics graphics, Form window)
    {
        _graphics = graphics;
        PsoFactory = new GorgonGraphicsPsoFactory(_graphics);
        _swapChain = new GorgonSwapChain(_graphics, "Primitives Swapchain", window.Handle, new GorgonSwapChainInfo(window.ClientSize.Width, window.ClientSize.Height, BufferFormat.R8G8B8A8_UNorm));
        _depthStencil = GorgonDepthStencilView.CreateDepthStencilView(_graphics, "Primitives Depth/Stencil Buffer", BufferFormat.D24_UNorm_S8_UInt, _swapChain.Width, _swapChain.Height);
        Camera = new GorgonPerspectiveCamera(new Vector2(_swapChain.Width, _swapChain.Height))
        {
            Position = new Vector3(0, 0, -0.5f)
        };
        Camera.Fov = 2.0f * ((75.0f / 2).ToRadians().Tan() / Camera.AspectRatio).ToDegrees();
        _lightBuffer = GorgonConstantBufferView.CreateConstantBuffer(_graphics, "Primitives Light List", MaxLights * GorgonGpuLightData.SizeInBytes);
        _aabbVisual = new GorgonAABBVisual(_graphics, PsoFactory);

        _renderData = new RenderData()
        {
            LightBufferHandle = _lightBuffer.GetViewHandle(),
            SamplerHandle = GorgonSampler.LinearWrapping(_graphics).GetViewHandle()
        };
    }
}
