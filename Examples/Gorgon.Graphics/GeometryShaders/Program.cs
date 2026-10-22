
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
// Created: June 3, 2018 11:04:26 AM
// 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
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
/// This example shows how to use a geometry shader with the Gorgon core graphics library
///
/// A geometry shader is used to generate primitive data based on incoming primitive vertices (e.g. a set of vertices for a triangle, a single point, etc...). Using this, 
/// we can dynamically build geometry on the GPU
///
/// This example will take a single triangle, and build a fully 3D pyramid along with some animation in the geometry shader. It will also show how an application can build
/// geometry without using an input layout, or vertex buffer
/// </summary>
internal class Program
    : IDisposable
{
    /// <summary>
    /// The data used to render the scene.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct RenderData
    {
        /// <summary>
        /// The world matrix.
        /// </summary>
        public Matrix4x4 World;
        /// <summary>
        /// The projection matrix.
        /// </summary>
        public Matrix4x4 Projection;
        /// <summary>
        /// The offset of the top point in the geometry.
        /// </summary>
        public float Offset;
        /// <summary>
        /// Handle for the texture.
        /// </summary>
        public int TextureHandle;
        /// <summary>
        /// Handle for the texture sampler.
        /// </summary>
        public int SamplerHandle;
    }

    // The application window.
    private Form _window;
    // The graphics interface for the application.
    private GorgonGraphics _graphics;
    // The application swap chain used to display our graphics.
    private GorgonSwapChain _swap;
    // The camera for the view.
    private GorgonPerspectiveCamera _camera;
    // The draw call used to draw the primitives.
    private GorgonDrawCall? _drawCall;
    // The pipline state builder.
    private GorgonGraphicsPsoFactory _psoFactory;
    // The texture to apply to the primitives.
    private IGorgonTextureView<GorgonTexture>? _texture;
    // The depth/stencil view.
    private GorgonDepthStencilView _depthStencil;
    // The angle of rotation.
    private float _angle;
    // Per frame rendering data.
    private RenderData _renderData;

    /// <summary>
    /// Function to build the draw call.
    /// </summary>
    /// <param name="texture">The texture to use.</param>
    private void BuildDrawCall(IGorgonTextureView<GorgonTexture> texture)
    {
        string shaderCode = Resources.Shaders;
        CompileFlags flags = Debugger.IsAttached ? CompileFlags.Debug : CompileFlags.OptimizationLevel3;

        using GorgonShaderCompiler compiler = new(_graphics);

        GorgonShader vertexShader = compiler.Compile(shaderCode, "VsMain", ShaderType.VertexShader, flags: flags);
        GorgonShader pixelShader = compiler.Compile(shaderCode, "PsMain", ShaderType.PixelShader, flags: flags);
        GorgonShader geometryShader = compiler.Compile(shaderCode, "GsMain", ShaderType.GeometryShader, flags: flags);

        GorgonGraphicsPsoBuilder builder = new(_graphics);

        builder.OutputFormat(_swap.Format)
               .DepthStencilState(GorgonDepthStencilState.DepthEnabled, _depthStencil.Format)
               .PixelShader(pixelShader)
               .GeometryShader(geometryShader);

        GorgonGraphicsPso pso = _psoFactory.CreateOrGetPso("Geometry Shader PSO", vertexShader, builder);

        _renderData.TextureHandle = texture.GetViewHandle();

        _drawCall = new GorgonDrawCall(3, pso);
        _drawCall.AssignTexture(new GorgonUsedTexture(texture, ShaderStage.Pixel, TextureUsage.ReadOnly));
    }

    /// <summary>
    /// Function to handle idle time for the application.
    /// </summary>
    /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
    private bool Idle()
    {
        Debug.Assert(_drawCall is not null, "No draw call defined");

        // Rotate our pyramid.
        _angle += 45.0f * GorgonTiming.Delta;

        if (_angle > 360.0f)
        {
            _angle -= 360.0f;
        }

        _renderData.Offset = _angle.ToRadians().FastSin().Abs();

        // Send the animated variables to their respective shaders.
        ref Matrix4x4 world = ref _renderData.World;

        world = Matrix4x4.CreateRotationY(_angle.ToRadians());
        world.SetRow(3, new Vector4(0, 0, 2.0f, 1.0f));

        _swap.WaitForFrameLatency();

        GorgonCommandList list = _graphics.GetCommandList("Geometry Shaders List");

        // Set up our rendering commands.
        // The presenter tells the list that our swap chain will be presenting at the end of the command submission.
        list.AddPresenter(_swap)
            .SetRenderTarget(_swap, _depthStencil)
            .SetScissorRectangle(new GorgonRectangle(0, 0, _swap.Width, _swap.Height))
            .SetViewport(new GorgonViewport(0, 0, _swap.Width, _swap.Height))
            .ClearRenderTarget(_swap, GorgonColors.CornFlowerBlue)
            .ClearDepthStencil(_depthStencil, 1.0f, 0)
            .WriteConstant(0, _renderData)
            .Draw(_drawCall);

        GorgonExample.BlitLogo(list);

        _graphics.Submit(list);

        return true;
    }

    /// <summary>
    /// Function used to initialize the application.
    /// </summary>
    private static void Initialize()
    {
        /*
            // Load the shaders from a file on disc.
            LoadShaders();

            // Load the texture.
            _texture = GorgonTexture2DView.FromFile(_graphics,
                                                    Path.Combine(GorgonExample.GetResourcePath(@"Textures\GeometryShader\").FullName, "GSTexture.png"),
                                                    new GorgonCodecPng());

            // Create our builders so we can compose a draw call and pipeline state.
            _drawCallBuilder = new GorgonDrawCallBuilder();
            _psoFactory = new GorgonPipelineStateBuilder(_graphics);

            // Set our swap chain as the active rendering target and the depth/stencil buffer.
            _graphics.SetRenderTarget(_swap.RenderTargetView, _depthStencil);

            // Create a constant buffer so we can adjust the positioning of the data.
            _camera = new GorgonPerspectiveCamera(_graphics, new Vector2(_swap.Width, _swap.Height), 0.125f, 1000.0f)
            {
                Fov = 65.0f
            };

            _vsConstants = GorgonConstantBufferView.CreateConstantBuffer(_graphics,
                                                                         new GorgonConstantBufferInfo((Unsafe.SizeOf<Matrix4x4>() * 2) + Unsafe.SizeOf<Vector4>())
                                                                         {
                                                                             Name = "WorldProjection CBuffer"
                                                                         });
            _vsConstants.Buffer.SetData(in _camera.GetProjectionMatrix(), copyMode: CopyMode.Discard);
            _vsConstants.Buffer.SetData(in _worldMatrix, 64, CopyMode.NoOverwrite);

            // Create a draw call so we actually have something we can draw.
            _drawCall = _drawCallBuilder.VertexRange(0, 3)
                                        .PipelineState(_psoFactory.PixelShader(_pixelShader)
                                                                        .VertexShader(_bufferless)
                                                                        .GeometryShader(_geometryShader)
                                                                        .DepthStencilState(GorgonDepthStencilState.DepthEnabled))
                                        .ShaderResource(ShaderType.Pixel, _texture)
                                        .ConstantBuffer(ShaderType.Vertex, _vsConstants)
                                        .ConstantBuffer(ShaderType.Geometry, _vsConstants)
                                        .Build();

        }
        finally
        {
            
        }*/
    }

    /// <summary>
    /// Function to dispose of unmanaged and managed objects.
    /// </summary>
    /// <param name="disposing"><b>true</b> to dispose of both managed and unmanaged objects, <b>false</b> to only dispose unmanaged.</param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _texture?.Dispose();
            _psoFactory.Dispose();
            _depthStencil.Dispose();
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

        _depthStencil.Dispose();

        _swap.Resize(form.ClientSize.Width, form.ClientSize.Height);
        _depthStencil = GorgonDepthStencilView.CreateDepthStencilView(_graphics, "Geometry Shader Depth Buffer", BufferFormat.D24_UNorm_S8_UInt, _swap.Width, _swap.Height);
        _camera.ViewDimensions = new Vector2(_swap.Width, _swap.Height);
        _renderData.Projection = _camera.GetProjectionMatrix();
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
        IGorgonImage? image = null;

        try
        {
            GorgonCodecPng pngCodec = new();

            image = pngCodec.FromFile(Path.Combine(GorgonExample.GetResourcePath(@"Textures\GeometryShader\").FullName, "GSTexture.png"));
            _texture = IGorgonTextureView<GorgonTexture>.CreateTexture(_graphics, "Geometry Shader Texture", image);

            image.Dispose();
            image = null;

            BuildDrawCall(_texture);

            GorgonExample.LoadResources(_graphics);
            GorgonExample.EndInit();

            GorgonExample.Loop.Run(Idle);

            _renderData = new RenderData()
            {
                Projection = _camera.GetProjectionMatrix(),
                TextureHandle = _texture.GetViewHandle(),
                SamplerHandle = _renderData.SamplerHandle = GorgonSampler.Linear(_graphics).GetViewHandle()
            };

            _window.Resize += Window_Resize;

            Application.Run(_window);
        }
        finally
        {
            image?.Dispose();
            _window.Resize -= Window_Resize;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Program"/> class.
    /// </summary>
    /// <param name="window">The window to render into.</param>
    /// <param name="graphics">The graphics interface to use.</param>
    public Program(Form window, GorgonGraphics graphics)
    {
        _window = window;
        _graphics = graphics;
        _swap = new GorgonSwapChain(graphics, "Geometry Shader SwapChain", _window.Handle, new GorgonSwapChainInfo(_window.ClientSize.Width, _window.ClientSize.Height, BufferFormat.R8G8B8A8_UNorm));
        _depthStencil = GorgonDepthStencilView.CreateDepthStencilView(_graphics, "Geometry Shader Depth Buffer", BufferFormat.D24_UNorm_S8_UInt, _swap.Width, _swap.Height);
        _psoFactory = new GorgonGraphicsPsoFactory(_graphics);
        _camera = new GorgonPerspectiveCamera(new Vector2(_swap.Width, _swap.Height), 0.125f, 1000.0f)
        {
            // Vertical FOV for 16:9 aspect ratio at 80 degrees.
            Fov = 51f
        };
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            // Create the main application form.
            FormMain mainForm = GorgonExample.Initialize(ExampleConfig.Default.Resolution, "Geometry Shader");
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

            // We'll need a depth buffer for this example, or else our pyramid will look weird when rotating as back faces will appear through front faces.
            // So, first we should check for support of a proper depth/stencil format.  That said, if we don't have this format, then we're likely not running hardware from the last decade or more.
            if (!graphics.FormatSupport[BufferFormat.D24_UNorm_S8_UInt].IsDepthStencilFormat)
            {
                GorgonDialogs.Error(mainForm, "A 24 bit depth buffer is required for this example.");
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
