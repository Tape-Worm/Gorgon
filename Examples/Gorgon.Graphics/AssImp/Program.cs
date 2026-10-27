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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Timing;
using Gorgon.UI.WindowsForms;
using TerraFX.Interop.Windows;

namespace Gorgon.Examples;

/// <summary>
/// The main application class
/// </summary>
internal class Program
    : IDisposable
{
    /// <summary>
    /// GPU data for sending the world and WVP matrices.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct MatrixGpuData
    {
        /// <summary>
        /// The world matrix.
        /// </summary>
        public Matrix4x4 WorldMatrix;
        /// <summary>
        /// The world/view/projection matrix.
        /// </summary>
        public Matrix4x4 WvpMatrix;
        /// <summary>
        /// The current camera position in world space.
        /// </summary>
        public Vector3 CameraPosition;
    }

    /// <summary>
    /// GPU data for sending the material data to the GPU.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct RenderData
    {
        /// <summary>
        /// The diffuse color.
        /// </summary>
        public GorgonColor Diffuse;
        /// <summary>
        /// The emissive color.
        /// </summary>
        public GorgonColor Emissive;
        /// <summary>
        /// The specular color.
        /// </summary>
        public GorgonColor Specular;
        /// <summary>
        /// The vertex buffer handle
        /// </summary>
        public int VertexBufferHandle;
        /// <summary>
        /// The handle for the current texture.
        /// </summary>
        public int TextureHandle;
        /// <summary>
        /// The handle to the texture sampler.
        /// </summary>
        public int SamplerHandle;
    }

    // The form for the application.
    private readonly Form _window;
    // The primary graphics interface.
    private readonly GorgonGraphics _graphics;
    // The main swap chain.
    private readonly GorgonSwapChain _swap;
    // The depth buffer.
    private GorgonDepthStencilView _depthBuffer;
    // The pipeline state object factory.
    private readonly GorgonGraphicsPsoFactory _psoFactory;
    // The fallback texture to use when no texture is supplied.
    private IGorgonTextureView<GorgonTexture>? _fallbackTexture;
    // The model to display.
    private Model? _model;
    // The camera.
    private readonly GorgonPerspectiveCamera _camera;
    // The draw calls for the model.
    private readonly Dictionary<Material, GorgonIndexedDrawCall> _drawCall = [];
    // The matrix data to the send to the GPU.
    private MatrixGpuData _matrixGpu = new();

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
    private void UpdateWVP(in Matrix4x4 world)
    {
        // Build our world/view/projection matrix to send to
        // the shader.
        ref readonly Matrix4x4 viewMatrix = ref _camera.GetViewMatrix();
        ref readonly Matrix4x4 projMatrix = ref _camera.GetProjectionMatrix();

        Matrix4x4 temp = Matrix4x4.Multiply(world, viewMatrix);
        Matrix4x4 wvp = Matrix4x4.Multiply(temp, projMatrix);

        ref Matrix4x4 gpuWorld = ref _matrixGpu.WorldMatrix;
        ref Matrix4x4 gpuWvp = ref _matrixGpu.WvpMatrix;

        gpuWorld = world;

        // Direct 3D requires that we transpose our matrix 
        // before sending it to the shader.
        gpuWvp = Matrix4x4.Transpose(wvp);

        _matrixGpu.CameraPosition = _camera.Position;
    }
    

    /// <summary>
    /// Function to handle idle time for the application.
    /// </summary>
    /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
    private bool Idle()
    {
        Debug.Assert(_model is not null && _fallbackTexture is not null, "Model not loaded");

        UpdateWVP(in _model.GetWorldMatrix());

        // Animate the model.
        _model.RotateX = 15.0f;
        _model.RotateY += 22.5f * GorgonTiming.Delta;

        if (_model.RotateY > 360.0f)
        {
            _model.RotateY -= 360.0f;
        }

        _swap.WaitForFrameLatency();

        GorgonCommandList list = _graphics.GetCommandList("Asset Import CommandList");

        list.AddPresenter(_swap)
            .SetRenderTarget(_swap, _depthBuffer)
            .SetScissorRectangle(new GorgonRectangle(0, 0, _swap.Width, _swap.Height))
            .SetViewport(new GorgonViewport(0, 0, _swap.Width, _swap.Height))
            .ClearRenderTarget(_swap, GorgonColors.CornFlowerBlue)
            .ClearDepthStencil(_depthBuffer, 1.0f, 0)
            .WriteConstant(0, _matrixGpu);

        foreach (KeyValuePair<Material, List<(int baseStart, int start, int count)>> mesh in _model.Meshes)
        {
            // Get the appropriate draw call for the meshes.
            // We have sorted our draw calls by material in this case, so for each material 
            // we draw all meshes associated with it.
            //
            // In an actual 3D renderer you'd also want to draw by Z - front to back for 
            // materials without translucency (alpha) and z-writing turned on. And back to
            // front for those items with translucency, with z-writing turned off.
            if (!_drawCall.TryGetValue(mesh.Key, out GorgonIndexedDrawCall? drawCall))
            {
                continue;
            }

            // Prep for rendering the model..
            RenderData renderData = new()
            {
                VertexBufferHandle = _model.VertexData.GetViewHandle(),
                TextureHandle = mesh.Key.Texture?.GetViewHandle() ?? _fallbackTexture.GetViewHandle(),
                SamplerHandle = mesh.Key.Sampler?.GetViewHandle() ?? GorgonSampler.Wrapping(_graphics).GetViewHandle(),
                Diffuse = mesh.Key.Diffuse,
                Emissive = mesh.Key.Emissive,
                Specular = new(mesh.Key.Specular, mesh.Key.SpecularPower)
            };

            list.WriteConstant(1, renderData);

            for (int i = 0; i < mesh.Value.Count; ++i)
            {
                (int baseStart, int start, int count) = mesh.Value[i];
                drawCall.StartIndex = start;
                drawCall.IndexCount = count;
                if (_graphics.Adapter.SupportsExtendedCommandInfo)
                {
                    drawCall.BaseVertex = baseStart;
                }

                list.Draw(drawCall);
            }
        }

        GorgonExample.BlitLogo(list);

        _graphics.Submit(list);

        return true;
    }

    /// <summary>
    /// Function to dispose of unmanaged and managed objects.
    /// </summary>
    /// <param name="disposing"><b>true</b> to dispose of both managed and unmanaged objects, <b>false</b> to only dispose unmanaged.</param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fallbackTexture?.Dispose();
            _model?.Dispose();
            _psoFactory.Dispose();
            _swap.Dispose();
        }
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

        _depthBuffer = BuildDepthBuffer(_swap.Width, _swap.Height, _depthBuffer);
        _swap.Resize(form.ClientSize.Width, form.ClientSize.Height);
        _camera.ViewDimensions = new Vector2(_swap.Width, _swap.Height);
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
        return GorgonDepthStencilView.CreateDepthStencilView(_graphics, "Asset Importer Depth/Stencil Buffer", BufferFormat.D24_UNorm_S8_UInt, width, height);
    }

    /// <summary>
    /// Function to load the assets and other data required for the application.
    /// </summary>
    /// <returns>The model to render and its pipeline state object.</returns>
    [MemberNotNull(nameof(_fallbackTexture))]
    private (Model model, GorgonGraphicsPso pso) LoadAssets()
    {
        string shaderCode = Resources.Shader;
        CompileFlags flags = Debugger.IsAttached ? CompileFlags.Debug : CompileFlags.OptimizationLevel3;

        using GorgonShaderCompiler compiler = new(_graphics);
        GorgonShader vertexShader = compiler.Compile(shaderCode, "ModelVS", ShaderType.VertexShader, flags: flags);
        GorgonShader pixelShader = compiler.Compile(shaderCode, "ModelPS", ShaderType.PixelShader, flags: flags);

        GorgonGraphicsPsoBuilder builder = new(_graphics);

        builder.OutputFormat(_swap.Format)
               .DepthStencilState(GorgonDepthStencilState.DepthEnabled, _depthBuffer.Format)
               .BlendState(GorgonBlendState.Default)
               .PixelShader(pixelShader);

        GorgonGraphicsPso pso = _psoFactory.CreateOrGetPso("Model PSO", vertexShader, builder);

        GorgonCodecPng pngCodec = new();
        using IGorgonImage defaultImage = pngCodec.FromFile(Path.Combine(GorgonExample.GetResourcePath(@"Textures").FullName, "White_2x2.png"));
        _fallbackTexture = IGorgonTextureView<GorgonTexture>.CreateTexture(_graphics, "Default fallback texture (White)", defaultImage);

        Model model = Model.Load(_graphics, Path.Combine(GorgonExample.GetResourcePath(@"Models\AssImp").FullName, "NCC1701A.ms3d"));

        return (model, pso);
    }

    /// <summary>
    /// Function to build up the list of draw calls needed to render the model.
    /// </summary>
    /// <param name="model">The model to render.</param>
    /// <param name="pso">The pipeline state for the model.</param>
    /// <param name="fallbackTexture">The fallback texture.</param>
    private void BuildDraws(Model model, GorgonGraphicsPso pso, IGorgonTextureView<GorgonTexture> fallbackTexture)
    {
        // We'll pre-bake our draw calls so that we can draw per-material/texture. In a real 3D renderer you'd want this be more flexible.
        foreach (Material material in model.Meshes.Keys)
        {
            GorgonIndexedDrawCall draw = new(0, model.IndexData, pso);
            if (material.Texture is not null)
            {
                draw.AssignTexture(new GorgonUsedTexture(material.Texture, ShaderStage.Pixel, TextureUsage.ReadOnly));
            }
            else
            {
                draw.AssignTexture(new GorgonUsedTexture(fallbackTexture, ShaderStage.Pixel, TextureUsage.ReadOnly));
            }

            draw.AssignBuffer(new GorgonUsedBuffer(model.VertexData, ShaderStage.Vertex, BufferUsage.ReadOnly));

            _drawCall[material] = draw;
        }
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
        try
        {
            GorgonExample.LoadResources(_graphics);

            (_model, GorgonGraphicsPso pso) = LoadAssets();

            BuildDraws(_model, pso, _fallbackTexture);

            // Position the camera to center on the model using its AABB.
            _camera.Position = new Vector3(0, _model.MaxY * 0.5f, -(_model.MaxZ - _model.MinZ));            

            GorgonExample.EndInit();

            GorgonExample.Loop.Run(Idle);

            _window.Resize += Window_Resize;

            Application.Run(_window);
        }
        finally
        {
            _window.Resize -= Window_Resize;
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
        _swap = new GorgonSwapChain(_graphics, "Asset Import SwapChain", _window.Handle, new GorgonSwapChainInfo(_window.Width, _window.Height, BufferFormat.R8G8B8A8_UNorm));
        _depthBuffer = BuildDepthBuffer(_swap.Width, _swap.Height, null);
        _psoFactory = new GorgonGraphicsPsoFactory(_graphics);
        _camera = new GorgonPerspectiveCamera(new Vector2(_swap.Width, _swap.Height), 0.001f, 500.0f)
        {
            // Vertical FOV for 16:9 aspect ratio at 80 degrees.
            Fov = 51f,
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
            FormMain mainForm = GorgonExample.Initialize(ExampleConfig.Default.Resolution, "Asset Import");
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
