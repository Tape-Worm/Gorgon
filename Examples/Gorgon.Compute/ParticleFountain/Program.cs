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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: April 15, 2025 2:29:16 PM
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Timing;
using Gorgon.UI.WindowsForms;

namespace Gorgon.Examples;

/// <summary>
/// The main application class
/// </summary>
internal class Program
    : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Vertex
    {
        /// <summary>
        /// The position of the vertex in object space.
        /// </summary>
        public Vector4 Position;
        /// <summary>
        /// The diffuse color of the vertex.
        /// </summary>
        public GorgonColor Diffuse;
        /// <summary>
        /// The texture coordinates for the vertex.
        /// </summary>
        public Vector2 UV;
    }

    /// <summary>
    /// GPU data for rendering the group layer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct GroundRenderData
    {
        /// <summary>
        /// The size of the render target.
        /// </summary>
        public Vector2 TargetSize;

        /// <summary>
        /// The vertex buffer handle for the ground.
        /// </summary>
        public int GroundVertexBufferHandle;

        /// <summary>
        /// The handle for the ground texture sampler to use.
        /// </summary>
        public int GroundSamplerHandle;

        /// <summary>
        /// The handle for the reflection texture sampler to use.
        /// </summary>
        public int ReflectionSamplerHandle;

        /// <summary>
        /// The handle for the ground texture to use.
        /// </summary>
        public int GroundTextureHandle;

        /// <summary>
        /// The handle for the reflection texture to use.
        /// </summary>
        public int ReflectionTextureHandle;
    }

    /// <summary>
    /// GPU data for rendering the group layer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct ParticleUpdateData
    {
        /// <summary>
        /// The buffer handle for the particles.
        /// </summary>
        public int ParticleBufferHandle;
        /// <summary>
        /// The total number of particles.
        /// </summary>
        public int ParticleCount;
        /// <summary>
        /// The seed value for randomness.
        /// </summary>
        public int RandomSeed;
        /// <summary>
        /// The current running time.
        /// </summary>
        public float RunningTime;
        /// <summary>
        /// The initial diffuse color for the particles.
        /// </summary>
        public GorgonColor InitialDiffuse;
    }

    /// <summary>
    /// GPU data for rendering the group layer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct ParticleRenderData
    {
        /// <summary>
        /// The view matrix from the camera.
        /// </summary>
        public Matrix4x4 ViewMatrix;

        /// <summary>
        /// The projection matrix from the camera.
        /// </summary>
        public Matrix4x4 ProjectionMatrix;

        /// <summary>
        /// The buffer handle for the particles.
        /// </summary>
        public int ParticleBufferHandle;

        /// <summary>
        /// The handle for the particle texture sampler to use.
        /// </summary>
        public int SamplerHandle;

        /// <summary>
        /// The handle for the particle texture to use.
        /// </summary>
        public int TextureHandle;
    }

    /// <summary>
    /// The size of a single particle, in bytes.
    /// </summary>
    private const int ParticleSizeInBytes = 112;

    // The total number of particles supported.
    private const int MaxParticles = 131_072;

    // The form for the application.
    private readonly Form _window;
    // The primary graphics interface.
    private readonly GorgonGraphics _graphics;
    // The main swap chain.
    private readonly GorgonSwapChain _swap;
    // The depth buffer.
    private GorgonDepthStencilView _depthBuffer;
    // The render target for the reflection.
    private GorgonRenderTargetView _reflectionTarget;
    // The pipeline state object factories.
    private readonly GorgonGraphicsPsoFactory _psoFactory;
    private readonly GorgonComputePsoFactory _computePsoFactory;
    // Draw call used to draw the ground.
    private GorgonDrawCallCommon[] _drawCalls = [];
    // The dispatch call used to update the particles in the compute shader.
    private GorgonDispatchCall? _dispatchCall;
    // The timer used to throttle updates to the FPS counter.
    private readonly GorgonTimer _fpsTimer = new();
    // The data to tell the renderer how to draw the ground.
    private GroundRenderData _groundRenderData;
    // The data to tell the renderer how to update a particle.
    private ParticleUpdateData _particleUpdateData;
    // The data to tell the renderer how to draw a particle.
    private ParticleRenderData _particleRenderData;
    // The camera.
    private readonly GorgonPerspectiveCamera _camera;
    // The textures used by the application.
    private IGorgonTextureView<GorgonTexture>[] _textures = new IGorgonTextureView<GorgonTexture>[2];
    // The old cursor position when the camera is activated.
    private GorgonPoint _oldMousePosition;
    // The angle around the particle fountain's vertical axis.
    private float _orbitYaw;
    // The angle above the ground.
    private float _orbitPitch;
    // The diffuse colors for the particles.
    private readonly GorgonColor[] _colors = [new GorgonColor(0.59f, 0.52f, 0.20f, 1.0f), GorgonColors.Red, GorgonColors.Green, GorgonColors.Blue, GorgonColors.Cyan, GorgonColors.Purple, GorgonColors.Yellow, GorgonColors.White];
    private int _colorIndex;
    // The current particle count.
    private int _particleCount = 1024;

    /// <summary>
    /// Function to dispose of unmanaged and managed objects.
    /// </summary>
    /// <param name="disposing"><b>true</b> to dispose of both managed and unmanaged objects, <b>false</b> to only dispose unmanaged.</param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (IGorgonTextureView<GorgonTexture> texture in _textures)
            {
                texture.Dispose();
            }

            _computePsoFactory.Dispose();
            _psoFactory.Dispose();            
            _depthBuffer.Dispose();
            _reflectionTarget.Dispose();
            _swap.Dispose();
        }
    }

    /// <summary>
    /// Function to update the world/view/projection matrix.
    /// </summary>
    /// <param name="world">The world matrix to update.</param>
    /// <remarks>
    /// <para>
    /// This is what sends the transformation information for the model plus any view space transforms (projection & view) to the GPU so the shader can transform the vertices in the 
    /// model and project them into 2D space on your render target.
    /// </para>
    /// </remarks>
    private Matrix4x4 UpdateWVP(in Matrix4x4 world)
    {
        // Build our world/view/projection matrix to send to
        // the shader.
        ref readonly Matrix4x4 viewMatrix = ref _camera.GetViewMatrix();
        ref readonly Matrix4x4 projMatrix = ref _camera.GetProjectionMatrix();

        Matrix4x4 temp = Matrix4x4.Multiply(world, viewMatrix);
        Matrix4x4 wvp = Matrix4x4.Multiply(temp, projMatrix);

        // Direct 3D requires that we transpose our matrix 
        // before sending it to the shader.
        return Matrix4x4.Transpose(wvp);
    }

    /// <summary>
    /// Function to display the frames per second on the window title bar.
    /// </summary>
    private void UpdateFpsCounter()
    {
        if (_fpsTimer.Milliseconds < 250)
        {
            return;
        }

        _window.Text = $"FPS: {GorgonTiming.AverageFPS:0.0}, Delta: {(GorgonTiming.AverageDelta * 1000):0.000} msec.";

        _fpsTimer.Reset();
    }

    /// <summary>
    /// Function to handle idle time for the application.
    /// </summary>
    /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
    private bool Idle()
    {
        Matrix4x4 wvp = UpdateWVP(Matrix4x4.Identity);

        _particleRenderData.ProjectionMatrix = Matrix4x4.Transpose(_camera.GetProjectionMatrix());
        _particleUpdateData.InitialDiffuse = _colors[_colorIndex];
        _particleUpdateData.ParticleCount = _particleCount;
        _particleUpdateData.RandomSeed = GorgonRandom.RandomInt32();
        _particleUpdateData.RunningTime = GorgonTiming.SecondsSinceStart;

        _swap.WaitForFrameLatency();

        GorgonIndexedDrawCall groundDraw = (GorgonIndexedDrawCall)_drawCalls[0];
        GorgonDrawCall particleDraw = (GorgonDrawCall)_drawCalls[1];
        GorgonDrawCall particleLightsDraw = (GorgonDrawCall)_drawCalls[2];
        GorgonDrawCall reflectionDraw = (GorgonDrawCall)_drawCalls[3];

        reflectionDraw.VertexCount = particleDraw.VertexCount = _particleCount * 240;
        particleLightsDraw.VertexCount = _particleCount * 30;

        GorgonCommandList list = _graphics.GetCommandList("Particle Fountian Gfx CommandList");

        list.AddPresenter(_swap)
            .SetScissorRectangle(new GorgonRectangle(0, 0, _swap.Width, _swap.Height))
            .SetViewport(new GorgonViewport(0, 0, _swap.Width, _swap.Height))
            .ClearRenderTarget(_swap, GorgonColors.Black)
            .ClearRenderTarget(_reflectionTarget, GorgonColors.Black)
            .ClearDepthStencil(_depthBuffer, 1.0f, 0);

        Debug.Assert(_dispatchCall is not null, "No dispatch call found.");

        _dispatchCall.ThreadXGroupCount = (_particleCount * 5 + 63) / 64;

        list.WriteConstant(0, _particleUpdateData)
            .Dispatch(_dispatchCall);

        _particleRenderData.ViewMatrix = Matrix4x4.Transpose(Matrix4x4.Multiply(Matrix4x4.CreateReflection(new Plane(Vector3.UnitY, 0)), _camera.GetViewMatrix()));

        list.SetRenderTarget(_reflectionTarget)
            .WriteConstant(0, _particleRenderData)
            .Draw(reflectionDraw);
                
        list.WriteConstant(0, wvp)
            .WriteConstant(1, _groundRenderData)
            .SetRenderTarget(_swap, _depthBuffer)
            .Draw(groundDraw);

        _particleRenderData.ViewMatrix = Matrix4x4.Transpose(_camera.GetViewMatrix());

        list.WriteConstant(0, _particleRenderData)            
            .Draw(particleLightsDraw);

        list.Draw(particleDraw);

        GorgonExample.BlitLogo(list);

        _graphics.Submit(list);

        UpdateFpsCounter();

        return true;
    }

    /// <summary>
    /// Function called when the window is resized.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void Window_Resize(object? sender, EventArgs e)
    {
        Form? form = (Form?)sender;

        if ((form is null) || (form.WindowState == FormWindowState.Minimized))
        {
            return;
        }

        _swap.Resize(form.ClientSize.Width, form.ClientSize.Height);
        _reflectionTarget = BuildReflectionTarget(_swap.Width, _swap.Height, _reflectionTarget);
        _depthBuffer = BuildDepthBuffer(_swap.Width, _swap.Height, _depthBuffer);
        _groundRenderData.ReflectionTextureHandle = _reflectionTarget.TextureView?.GetViewHandle() ?? throw new Exception("No texture view was found on the render target.");
        _groundRenderData.TargetSize = new Vector2(_swap.Width, _swap.Height);
        _drawCalls[0].AssignTextures([new GorgonUsedTexture(_textures[0], ShaderStage.Pixel, TextureUsage.ReadOnly),
                                                              new GorgonUsedTexture(_reflectionTarget.TextureView, ShaderStage.Pixel, TextureUsage.ReadOnly)]);
    }

    /// <summary>
    /// Function to handle mouse movement.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The mouse event parameters.</param>
    private void Window_MouseMove(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
        {
            return;
        }

        Cursor.Position = _window.PointToScreen(new GorgonPoint(_swap.Width / 2, _swap.Height / 2));

        float deltaX = e.X - (_swap.Width / 2);
        float deltaY = e.Y - (_swap.Height / 2);

        _orbitYaw += deltaX * 0.001f;
        _orbitPitch = (_orbitPitch + deltaY * 0.001f).Clamp(0.05f, 1.5f);

        float distanceToFountain = Vector3.Distance(_camera.Position, Vector3.UnitY);
        float groundDistance = distanceToFountain * _orbitPitch.Cos();

        _camera.Position = Vector3.UnitY + new Vector3(groundDistance * _orbitYaw.Sin(), distanceToFountain * _orbitPitch.Sin(), groundDistance * _orbitYaw.Cos());
        _camera.LookAt(Vector3.UnitY);
    }

    /// <summary>
    /// Function called when the mouse wheel is moved.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The mouse event parameters.</param>
    private void Window_MouseWheel(object? sender, MouseEventArgs e)
    {
        if (e.Delta > 0)
        {
            _particleCount <<= 1;

            if (_particleCount > MaxParticles)
            {
                _particleCount = MaxParticles;
            }
        }
        else
        {
            _particleCount >>= 1;

            if (_particleCount < 1)
            {
                _particleCount = 1;
            }
        }
    }

    /// <summary>
    /// Function to handle the mouse button presses.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The mouse event parameters.</param>
    private void Window_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
        {
            return;
        }

        _oldMousePosition = Cursor.Position;
        Cursor.Hide();
        Cursor.Position = _window.PointToScreen(new GorgonPoint(_swap.Width / 2, _swap.Height / 2));        
    }

    /// <summary>
    /// Function called when a mouse button is released.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The mouse event parameters.</param>
    private void Window_MouseUp(object? sender, MouseEventArgs e)
    {
        switch (e.Button)
        {
            case MouseButtons.Left:
                _colorIndex++;

                if (_colorIndex >= _colors.Length)
                {
                    _colorIndex = 0;
                }
                break;
            case MouseButtons.Right:
                Cursor.Position = _oldMousePosition;
                Cursor.Show();
                break;
        }
    }

    /// <summary>
    /// Function to build or rebuild the depth buffer.
    /// </summary>
    /// <param name="width">The width of the depth buffer.</param>
    /// <param name="height">The height of the depth buffer.</param>
    /// <param name="previous">The previous depth buffer.</param>
    /// <returns>The depth/stencil buffer.</returns>
    private GorgonDepthStencilView BuildDepthBuffer(int width, int height, GorgonDepthStencilView? previous)
    {
        previous?.Dispose();
        return GorgonDepthStencilView.CreateDepthStencilView(_graphics, "Particle Fountain Depth/Stencil Buffer", BufferFormat.D24_UNorm_S8_UInt, width, height);
    }

    /// <summary>
    /// Function to build or rebuild the reflection render target.
    /// </summary>
    /// <param name="width">The width of the render target.</param>
    /// <param name="height">The height of the render target.</param>
    /// <param name="previous">The previous render target.</param>
    /// <returns>The render target.</returns>
    private GorgonRenderTargetView BuildReflectionTarget(int width, int height, GorgonRenderTargetView? previous)
    {
        previous?.Dispose();
        return GorgonRenderTargetView.Create2DRenderTarget(_graphics, "Particle Fountain Reflection Target", _swap.Format, width, height);
    }

    /// <summary>
    /// Function to create the ground on which the fountain will sit.
    /// </summary>
    /// <returns>The buffers holding the ground view vertices and indices.</returns>
    private (GorgonStructuredBufferView vertexData, GorgonIndexBuffer indexData) BuildGround()
    {
        const int divisions = 8;
        const float rectSize = 256.0f;
        const float tileSize = 32.0f;

        int[] indices = new int[(divisions * divisions) * 6];
        Vertex[] vertices = new Vertex[(divisions + 1) * (divisions + 1)];
        int indexCount = 0;
        float increment = rectSize / divisions;
        float uvIncrement = tileSize / divisions;
        float vx;
        float vz = rectSize * -0.5f;
        float ux;
        float uy = 0;

        for (int z = 0; z <= divisions; ++z)
        {
            vx = rectSize * -0.5f;
            ux = 0;

            for (int x = 0; x <= divisions; ++x)
            {
                vertices[x + z * (divisions + 1)] = new Vertex()
                {
                    Position = new Vector4(vx, 0, vz, 1.0f),
                    Diffuse = new GorgonColor(0.2f, 0.2f, 0.2f),
                    UV = new Vector2(ux, uy)
                };

                vx += increment;
                ux += uvIncrement;                
            }

            vz += increment;
            uy += uvIncrement;
        }

        // Populate index buffer.
        for (int z = 0; z < divisions; ++z)
        {
            for (int x = 0; x < divisions; x++)
            {
                int vertexIndex = x + z * (divisions + 1);

                indices[indexCount] = vertexIndex;
                indices[indexCount + 1] = vertexIndex + (divisions + 1);
                indices[indexCount + 2] = vertexIndex + 1;
                indices[indexCount + 3] = vertexIndex + (divisions + 1);
                indices[indexCount + 4] = vertexIndex + (divisions + 2);
                indices[indexCount + 5] = vertexIndex + 1;

                indexCount += 6;
            }
        }

        GorgonStructuredBufferView vertexBuffer = GorgonStructuredBufferView.CreateStructuredBuffer<Vertex>(_graphics, "Particle Fountain", vertices.Length);
        GorgonIndexBuffer indexBuffer = new(_graphics, "Particle Fountain IB", new GorgonIndexBufferInfo(sizeof(int) * indices.Length, true));

        GorgonCommandList list = _graphics.GetCommandList("Particle Fountain Upload")
                                          .CopyRange(indices, indexBuffer)
                                          .CopyRange(vertices, vertexBuffer.Buffer);

        _graphics.Submit(list);
        _graphics.WaitForGpu();

        return (vertexBuffer, indexBuffer);
    }

    /// <summary>
    /// Function to build the dispatch call used to update the particles.
    /// </summary>
    /// <param name="particleBuffer">The buffer containing the particle data.</param>
    /// <returns>The dispatch call used to update the particle data.</returns>
    private GorgonDispatchCall GetDispatchCall(GorgonStructuredBufferRwView particleBuffer)
    {
        using GorgonShaderCompiler compiler = new(_graphics);
        compiler.AddInclude(new GorgonShaderInclude("ParticleShaderCommon", Resources.ParticleShaderCommon));
        GorgonShader computeShader = compiler.Compile(Resources.ComputeParticles, "ComputeParticles", ShaderType.ComputeShader, flags: (_graphics.IsInDebugMode ? CompileFlags.Debug : CompileFlags.OptimizationLevel3));

        GorgonComputePso particlePso = _computePsoFactory.CreateOrGetPso("Particle Fountain Compute PSO", computeShader);

        GorgonDispatchCall result = new(particlePso, (_particleCount * 5 + 63) / 64, 1, 1);
        result.AssignBuffer(new GorgonUsedBuffer(particleBuffer, ShaderStage.Compute, BufferUsage.ReadWrite));

        return result;
    }

    /// <summary>
    /// Function to build the draw call used to render the ground.
    /// </summary>
    /// <param name="vertices">The vertices for the ground plane.</param>
    /// <param name="indices">The indices for the ground plane.</param>
    /// <param name="texture">The texture for the draw call.</param>
    /// <param name="reflectionTarget">The render target for the reflection.</param>
    /// <returns>The fully constructed draw call used to render the ground.</returns>
    private GorgonIndexedDrawCall GetGroundDrawCall(GorgonStructuredBufferView vertices, GorgonIndexBuffer indices, IGorgonTextureView<GorgonTexture> texture, IGorgonTextureView<GorgonTextureCommon> reflectionTarget)
    {
        using GorgonShaderCompiler compiler = new(_graphics);
        compiler.AddInclude(new GorgonShaderInclude("ParticleShaderCommon", Resources.ParticleShaderCommon));
        GorgonShader vertexShader = compiler.Compile(Resources.GroundShader, "VertexShader", ShaderType.VertexShader, flags: (_graphics.IsInDebugMode ? CompileFlags.Debug : CompileFlags.OptimizationLevel3));
        GorgonShader pixelShader = compiler.Compile(Resources.GroundShader, "PixelShader", ShaderType.PixelShader, flags: (_graphics.IsInDebugMode ? CompileFlags.Debug : CompileFlags.OptimizationLevel3));

        GorgonGraphicsPsoBuilder builder = new(_graphics);

        builder.BlendState(GorgonBlendState.NoBlending)
               .RasterizerState(GorgonRasterState.Default)
               .OutputFormat(_swap.Format)
               .DepthStencilState(GorgonDepthStencilState.DepthEnabled, _depthBuffer.Format)
               .PrimitiveType(PrimitiveType.TriangleList)
               .PixelShader(pixelShader);

        GorgonGraphicsPso groundPso = _psoFactory.CreateOrGetPso("Particle Fountain Ground PSO", vertexShader, builder);

        GorgonIndexedDrawCall result = new((int)(indices.SizeInBytes / sizeof(int)), indices, groundPso);
        result.AssignBuffer(new GorgonUsedBuffer(vertices, ShaderStage.Vertex, BufferUsage.ReadOnly));
        result.AssignTextures([new GorgonUsedTexture(texture, ShaderStage.Pixel, TextureUsage.ReadOnly), 
                                                       new GorgonUsedTexture(reflectionTarget, ShaderStage.Pixel, TextureUsage.ReadOnly)]);        

        return result;
    }

    /// <summary>
    /// Function to build the draw call used to render the particles.
    /// </summary>
    /// <param name="particles">The buffer containing the particles.</param>
    /// <param name="texture">The texture for the draw call.</param>
    /// <returns>The fully constructed draw call used to render the ground.</returns>
    private GorgonDrawCall GetParticleDrawCall(GorgonStructuredBufferView particles, IGorgonTextureView<GorgonTexture> texture)
    {
        using GorgonShaderCompiler compiler = new(_graphics);
        compiler.AddInclude(new GorgonShaderInclude("ParticleShaderCommon", Resources.ParticleShaderCommon));
        GorgonShader vertexShader = compiler.Compile(Resources.ParticleShader, "VertexShader", ShaderType.VertexShader, flags: (_graphics.IsInDebugMode ? CompileFlags.Debug : CompileFlags.OptimizationLevel3));
        GorgonShader pixelShader = compiler.Compile(Resources.ParticleShader, "PixelShader", ShaderType.PixelShader, flags: (_graphics.IsInDebugMode ? CompileFlags.Debug : CompileFlags.OptimizationLevel3));

        GorgonGraphicsPsoBuilder builder = new(_graphics);

        builder.BlendState(GorgonBlendState.AdditiveBlend)
               .DepthStencilState(GorgonDepthStencilState.DepthEnabledNoWrite, _depthBuffer.Format)
               .RasterizerState(GorgonRasterState.NoCulling)
               .OutputFormat(_swap.Format)
               .PrimitiveType(PrimitiveType.TriangleList)
               .PixelShader(pixelShader);

        GorgonGraphicsPso particlePso = _psoFactory.CreateOrGetPso("Particle Fountain Particle PSO", vertexShader, builder);

        GorgonDrawCall result = new(_particleCount * 240, particlePso);
        result.AssignBuffer(new GorgonUsedBuffer(particles, ShaderStage.Vertex, BufferUsage.ReadOnly));
        result.AssignTexture(new GorgonUsedTexture(texture, ShaderStage.Pixel, TextureUsage.ReadOnly));

        return result;
    }

    /// <summary>
    /// Function to build the draw call used to render the particle lights.
    /// </summary>
    /// <param name="particles">The buffer containing the particles.</param>
    /// <param name="texture">The texture for the draw call.</param>
    /// <returns>The fully constructed draw call used to render the ground.</returns>
    private GorgonDrawCall GetParticleLightDrawCall(GorgonDrawCallCommon particleDrawCall, GorgonStructuredBufferView particles, IGorgonTextureView<GorgonTexture> texture)
    {
        using GorgonShaderCompiler compiler = new(_graphics);
        compiler.AddInclude(new GorgonShaderInclude("ParticleShaderCommon", Resources.ParticleShaderCommon));
        GorgonShader vertexShader = compiler.Compile(Resources.ParticleShader, "LightVertexShader", ShaderType.VertexShader, flags: (_graphics.IsInDebugMode ? CompileFlags.Debug : CompileFlags.OptimizationLevel3));

        GorgonGraphicsPsoBuilder builder = new(_graphics);

        builder.ResetTo(particleDrawCall.Pso)
               .DepthStencilState(GorgonDepthStencilState.Default, BufferFormat.Unknown)
               .RasterizerState(GorgonRasterState.NoCulling)
               .OutputFormat(_swap.Format)
               .PrimitiveType(PrimitiveType.TriangleList);

        GorgonGraphicsPso particlePso = _psoFactory.CreateOrGetPso("Particle Fountain Particle Lights PSO", vertexShader, builder);

        GorgonDrawCall result = new(_particleCount * 30, particlePso);
        result.AssignBuffer(new GorgonUsedBuffer(particles, ShaderStage.Vertex, BufferUsage.ReadOnly));
        result.AssignTexture(new GorgonUsedTexture(texture, ShaderStage.Pixel, TextureUsage.ReadOnly));

        return result;
    }

    /// <summary>
    /// Function to build the draw call used to render the particle reflection.
    /// </summary>
    /// <param name="particles">The buffer containing the particles.</param>
    /// <param name="texture">The texture for the draw call.</param>
    /// <returns>The fully constructed draw call used to render the ground.</returns>
    private GorgonDrawCall GetReflectionDrawCall(GorgonStructuredBufferView particles, IGorgonTextureView<GorgonTexture> texture)
    {
        using GorgonShaderCompiler compiler = new(_graphics);
        compiler.AddInclude(new GorgonShaderInclude("ParticleShaderCommon", Resources.ParticleShaderCommon));
        GorgonShader vertexShader = compiler.Compile(Resources.ParticleShader, "VertexShader", ShaderType.VertexShader, flags: (_graphics.IsInDebugMode ? CompileFlags.Debug : CompileFlags.OptimizationLevel3));
        GorgonShader pixelShader = compiler.Compile(Resources.ParticleShader, "PixelShader", ShaderType.PixelShader, flags: (_graphics.IsInDebugMode ? CompileFlags.Debug : CompileFlags.OptimizationLevel3));

        GorgonGraphicsPsoBuilder builder = new(_graphics);

        builder.BlendState(GorgonBlendState.AdditiveBlend)
               .DepthStencilState(GorgonDepthStencilState.Default, BufferFormat.Unknown)
               .RasterizerState(GorgonRasterState.NoCulling)
               .OutputFormat(_swap.Format)
               .PrimitiveType(PrimitiveType.TriangleList)
               .PixelShader(pixelShader);

        GorgonGraphicsPso particlePso = _psoFactory.CreateOrGetPso("Particle Fountain Particle Reflection PSO", vertexShader, builder);

        GorgonDrawCall result = new(_particleCount * 240, particlePso);
        result.AssignBuffer(new GorgonUsedBuffer(particles, ShaderStage.Vertex, BufferUsage.ReadOnly));
        result.AssignTexture(new GorgonUsedTexture(texture, ShaderStage.Pixel, TextureUsage.ReadOnly));

        return result;
    }

    /// <summary>
    /// Function to load the textures used by the application.
    /// </summary>
    /// <returns>The ground texture for now.</returns>
    private IGorgonTextureView<GorgonTexture>[] LoadTextures()
    {
        GorgonCodecPng png = new();
        using IGorgonImage groundImage = png.FromFile(Path.Combine(GorgonExample.GetResourcePath(Path.Combine("Textures", "ParticleFountain")).FullName, "Marble Black.png"));
        using IGorgonImage particleImage = png.FromFile(Path.Combine(GorgonExample.GetResourcePath(Path.Combine("Textures", "ParticleFountain")).FullName, "Particle.png"));

        return [IGorgonTextureView<GorgonTexture>.CreateTexture(_graphics, "Ground Texture", groundImage),
                                            IGorgonTextureView<GorgonTexture>.CreateTexture(_graphics, "Particle Texture", particleImage)];
    }

    /// <summary>
    /// Function to initialize the particle data.
    /// </summary>
    /// <returns>A buffer containing the particle data.</returns>
    private GorgonGpuBuffer InitializeParticles()
    {
        GorgonGpuBuffer result = new(_graphics, "Particle Buffer", new GorgonGpuBufferInfo(MaxParticles * 5 * ParticleSizeInBytes)
        {
            Alignment = ParticleSizeInBytes,
            HasReadWriteAccess = true
        });

        byte[] empty = new byte[(int)result.SizeInBytes];
        Array.Clear(empty);

        GorgonCommandList list = _graphics.GetCommandList("Particle Fountain Upload")
                                          .CopyRange(empty, result);

        _graphics.Submit(list);
        _graphics.WaitForGpu();

        return result;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to begin execution of the application.
    /// </summary>
    public void Run()
    {
        GorgonGpuBuffer? particleBuffer = null;
        GorgonStructuredBufferRwView? particles = null;
        GorgonStructuredBufferView? particlesReadOnly = null;
        GorgonStructuredBufferView? vertices = null;
        GorgonIndexBuffer? indices = null;

        try
        {
            GorgonExample.LoadResources(_graphics);

            particleBuffer = InitializeParticles();
            particles = particleBuffer.GetStructuredBufferReadWriteView(ParticleSizeInBytes);
            particlesReadOnly = particleBuffer.GetStructuredBufferView(ParticleSizeInBytes);

            (vertices, indices) = BuildGround();

            _textures = LoadTextures();

            Debug.Assert(_reflectionTarget.TextureView is not null, "No texture view assigned to reflection target.");

            _drawCalls = new GorgonDrawCallCommon[4];

            _drawCalls[0] = GetGroundDrawCall(vertices, indices, _textures[0], _reflectionTarget.TextureView);
            _drawCalls[1] = GetParticleDrawCall(particlesReadOnly, _textures[1]);
            _drawCalls[2] = GetParticleLightDrawCall(_drawCalls[1], particlesReadOnly, _textures[1]);
            _drawCalls[3] = GetReflectionDrawCall(particlesReadOnly, _textures[1]);

            _dispatchCall = GetDispatchCall(particles);

            _particleUpdateData = new()
            {
                ParticleBufferHandle = particles.GetViewHandle(),
                ParticleCount = _particleCount
            };

            _groundRenderData = new GroundRenderData()
            {
                GroundVertexBufferHandle = vertices.GetViewHandle(),
                GroundSamplerHandle = GorgonSampler.LinearWrapping(_graphics).GetSamplerHandle(),
                GroundTextureHandle = _textures[0].GetViewHandle(),
                ReflectionTextureHandle = _reflectionTarget.TextureView.GetViewHandle(),
                TargetSize = new Vector2(_swap.Width, _swap.Height)
            };

            _particleRenderData = new ParticleRenderData()
            {
                ParticleBufferHandle = particlesReadOnly.GetViewHandle(),
                SamplerHandle = GorgonSampler.Linear(_graphics).GetSamplerHandle(),
                TextureHandle = _textures[1].GetViewHandle()
            };

            _camera.Position = new Vector3(-4.33925f, 3.99127f, -3.35433f);
            _camera.LookAt(Vector3.UnitY);
            Vector3 offset = Vector3.Subtract(_camera.Position, Vector3.UnitY);
            _orbitPitch = (offset.Y / offset.Length()).ASin();
            _orbitYaw = offset.X.ATan(offset.Z);

            GorgonExample.EndInit();

            GorgonExample.Loop.Run(Idle);

            _window.Resize += Window_Resize;
            _window.MouseUp += Window_MouseUp;
            _window.MouseDown += Window_MouseDown;
            _window.MouseMove += Window_MouseMove;
            _window.MouseWheel += Window_MouseWheel;

            Application.Run(_window);
        }
        finally
        {
            _window.MouseUp -= Window_MouseUp;
            _window.MouseDown -= Window_MouseDown;
            _window.MouseMove -= Window_MouseMove;
            _window.MouseWheel -= Window_MouseWheel;
            _window.Resize -= Window_Resize;
            indices?.Dispose();
            vertices?.Dispose();
            particlesReadOnly?.Dispose();
            particles?.Dispose();
            particleBuffer?.Dispose();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Program"/> type.
    /// </summary>
    /// <param name="window">The window to render into.</param>
    /// <param name="graphics">The graphics interface to use.</param>
    public Program(Form window, GorgonGraphics graphics)
    {
        _window = window;
        _graphics = graphics;
        _swap = new GorgonSwapChain(_graphics, "Particle Fountain SwapChain", _window.Handle, new GorgonSwapChainInfo(_window.Width, _window.Height, BufferFormat.R8G8B8A8_UNorm));
        _depthBuffer = BuildDepthBuffer(_swap.Width, _swap.Height, null);
        _reflectionTarget = BuildReflectionTarget(_swap.Width, _swap.Height, null);
        _psoFactory = new GorgonGraphicsPsoFactory(_graphics);
        _computePsoFactory = new GorgonComputePsoFactory(_graphics);
        _camera = new GorgonPerspectiveCamera(new Vector2(_swap.Width, _swap.Height))
        {
            Fov = 45.0f
        };
    }

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        try
        {
            // Create the main application form.
            FormMain mainForm = GorgonExample.Initialize(ExampleConfig.Default.Resolution, "Particle Fountain");
            GorgonExample.ResourceBaseDirectory = new DirectoryInfo(Path.GetFullPath(ExampleConfig.Default.ResourceLocation));            

            // Create the factory used to create our graphics functionality.
            // This is required to create the primary graphics object used for accessing the GPU.
#if DEBUG
            using GorgonGraphicsFactory factory = new(GorgonGraphicsDebugFlags.SynchronizedCommandQueueValidation
                | GorgonGraphicsDebugFlags.GpuBasedValidation
                | GorgonGraphicsDebugFlags.GpuBasedStateTrackingValidation
                | GorgonGraphicsDebugFlags.ObjectTracking, log: GorgonExample.Log);
#else
            using GorgonGraphicsFactory factory = new(log: GorgonExample.Log);
#endif     

            // Now we create and enumerate the list of video devices installed in the computer.
            // We must do this in order to tell Gorgon which video device we intend to use. Note that this method may be quite slow 
            //
            // If no suitable device was found (no Direct 3D 12 support) in the computer, this method will return an empty list. However, if it succeeds, then the devices list 
            // will be populated with an IGorgonVideoDeviceInfo for each suitable video device in the system.
            //
            // Using this method, we could also enumerate the WARP software rasterizer, and/of the D3D Reference device. These devices are typically used to determine if there's
            // a driver error, and can be terribly slow to render (reference moreso than WARP). It is recommended that these only be used in diagnostic scenarios only.
            IReadOnlyList<GorgonVideoAdapterInfo> adapters = factory.EnumerateAdapters();

            if (adapters.Count == 0)
            {
                GorgonDialogs.Error(mainForm, "No valid Direct 3D 12 GPU was found on the system. Please see the log for details.");
                return;
            }

            using GorgonGraphics graphics = factory.CreateGraphics(adapters[0]);

            // Check to ensure that we can support the format required for our swap chain.
            // If a video device can't support this format, then the odds are good it won't render anything. Since we're asking for a very common display format, this will 
            // succeed nearly 100% of the time. Regardless, it's good form to the check for a working display format prior to setting up the swap chain.
            //
            // This is also used to determine if a format can be used for other objects (e.g. a texture, render target, etc...) And like the swap chain format, it is also best 
            // practice to check if the object you're creating supports the desired format.
            if (!graphics.FormatSupport[BufferFormat.R8G8B8A8_UNorm].IsDisplayFormat)
            {
                // We should never see this unless you've got some very esoteric hardware.
                GorgonDialogs.Error(mainForm, "We should not see this error.");
                Application.Exit();
                return;
            }
            
            if (!graphics.FormatSupport[BufferFormat.D24_UNorm_S8_UInt].IsDepthStencilFormat)
            {
                // We should never see this unless you've got some very esoteric hardware.
                GorgonDialogs.Error(mainForm, $"The depth/stencil format {BufferFormat.D24_UNorm_S8_UInt} is not supported.");
                Application.Exit();
                return;
            }            

            using Program program = new(mainForm, graphics);

            program.Run();
        }
        catch (Exception ex)
        {
            GorgonExample.HandleException(ex);
        }
        finally
        {
            GorgonExample.ShutDown();
        }
    }
}
