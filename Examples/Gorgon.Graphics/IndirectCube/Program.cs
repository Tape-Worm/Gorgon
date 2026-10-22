//
// Gorgon
// Copyright (C) 2026 Michael Winsor
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
// Created: October 22, 2026 3:47:14 PM
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Gorgon.Math;
using Gorgon.Timing;
using Gorgon.UI.WindowsForms;

namespace Gorgon.Examples;

/// <summary>
/// This example shows how to draw with an execute call using the Gorgon core graphics library
///
/// An execute call draws a list of commands stored in a buffer, instead of drawing with a single draw call. Each command holds the arguments for a draw, and can hold execute
/// constants that are sent to the shaders for that command only. Here, the commands are written once by the application on the CPU
///
/// This example builds a cube out of a stack of slices. Each command draws one slice, and its execute constant tells the geometry shader which slice to build. The geometry shader
/// bends the slices to make the cube wobble like jello. Press any key (except Escape), or click the mouse, to make it wobble again
/// </summary>
/// <remarks>
/// NOTE FROM Tape_Worm:
///
/// This is an LLM generated sample by Claude. I wanted to test Claude's capabilities to see how well it could do an example program with the API. And frankly, it did good, real 
/// good. The code is easy to read and understand.
/// </remarks>
internal partial class Program
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
        /// The time, in seconds, used to animate the wobble.
        /// </summary>
        public float Time;
        /// <summary>
        /// The strength of the wobble.
        /// </summary>
        public float Amplitude;
        /// <summary>
        /// The number of slices in the stack.
        /// </summary>
        public int SliceCount;
        /// <summary>
        /// Handle for the texture.
        /// </summary>
        public int TextureHandle;
        /// <summary>
        /// Handle for the texture sampler.
        /// </summary>
        public int SamplerHandle;
    }

    // The number of slices in the cube.
    private const int SliceCount = 8;
    // The strength of the wobble when it starts.
    private const float StartAmplitude = 0.35f;
    // How quickly the wobble settles down.
    private const float Damping = 1.5f;
    // The application window.
    private readonly Form _window;
    // The graphics interface for the application.
    private readonly GorgonGraphics _graphics;
    // The application swap chain used to display our graphics.
    private readonly GorgonSwapChain _swap;
    // The camera for the view.
    private readonly GorgonPerspectiveCamera _camera;
    // The buffer holding the commands to execute.
    private GorgonExecuteBuffer? _commands;
    // The execute call used to draw the slices.
    private GorgonExecuteCall? _executeCall;
    // The pipline state builder.
    private readonly GorgonGraphicsPsoFactory _psoFactory;
    // The texture to apply to the cube.
    private IGorgonTextureView<GorgonTexture>? _texture;
    // The depth/stencil view.
    private GorgonDepthStencilView _depthStencil;
    // The angle of rotation.
    private float _angle;
    // Per frame rendering data.
    private RenderData _renderData;

    /// <summary>
    /// Function to build the execute call, and write the commands for each slice.
    /// </summary>
    /// <param name="texture">The texture to use.</param>
    private void BuildExecuteCall(IGorgonTextureView<GorgonTexture> texture)
    {
        string shaderCode = Resources.Shaders;
        CompileFlags flags = Debugger.IsAttached ? CompileFlags.Debug : CompileFlags.OptimizationLevel3;

        using GorgonShaderCompiler compiler = new(_graphics);

        GorgonShader vertexShader = compiler.Compile(shaderCode, "VsMain", ShaderType.VertexShader, flags: flags);
        GorgonShader pixelShader = compiler.Compile(shaderCode, "PsMain", ShaderType.PixelShader, flags: flags);
        GorgonShader geometryShader = compiler.Compile(shaderCode, "GsMain", ShaderType.GeometryShader, flags: flags);

        GorgonGraphicsPsoBuilder builder = new(_graphics);

        // Each command sends a single point to the geometry shader, which builds the slice from it.
        builder.PrimitiveType(PrimitiveType.PointList)
               .OutputFormat(_swap.Format)
               .DepthStencilState(GorgonDepthStencilState.DepthEnabled, _depthStencil.Format)
               .PixelShader(pixelShader)
               .GeometryShader(geometryShader);

        GorgonGraphicsPso pso = _psoFactory.CreateOrGetPso("Indirect Cube PSO", vertexShader, builder);

        _commands = new GorgonExecuteBuffer(_graphics, "Indirect Cube Commands", new GorgonExecuteBufferInfo(SliceCount, Unsafe.SizeOf<SliceCommand>()));

        // The first value in each command is an execute constant, so we start at constant 0 and use 1 constant.
        _executeCall = GorgonExecuteCall.CreateCpuRendering(pso, _commands, constantStart: 0, constantCount: 1);
        _executeCall.AssignTexture(new GorgonUsedTexture(texture, ShaderStage.Pixel, TextureUsage.ReadOnly));

        // The commands never change, so we only need to write them once.
        Span<SliceCommand> commands = stackalloc SliceCommand[SliceCount];

        for (int i = 0; i < commands.Length; ++i)
        {
            commands[i] = new SliceCommand
            {
                SliceIndex = (uint)i,
                VertexCount = 1,
                InstanceCount = 1
            };
        }

        GorgonCommandList list = _graphics.GetCommandList("Indirect Cube Init List");
        list.CopyRange(commands, _commands);
        _graphics.Submit(list);
    }

    /// <summary>
    /// Function to handle idle time for the application.
    /// </summary>
    /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
    private bool Idle()
    {
        Debug.Assert(_executeCall is not null, "No execute call defined");

        // Rotate our cube.
        _angle += 30.0f * GorgonTiming.Delta;

        if (_angle > 360.0f)
        {
            _angle -= 360.0f;
        }

        // Let the wobble settle down over time.
        _renderData.Time += GorgonTiming.Delta;
        _renderData.Amplitude *= MathF.Exp(-Damping * GorgonTiming.Delta);

        ref Matrix4x4 world = ref _renderData.World;

        world = Matrix4x4.CreateRotationY(_angle.ToRadians());
        world.SetRow(3, new Vector4(0, -0.25f, 2.0f, 1.0f));

        _swap.WaitForFrameLatency();

        GorgonCommandList list = _graphics.GetCommandList("Indirect Cube List");

        // Set up our rendering commands.
        // The presenter tells the list that our swap chain will be presenting at the end of the command submission.
        list.AddPresenter(_swap)
            .SetRenderTarget(_swap, _depthStencil)
            .SetScissorRectangle(new GorgonRectangle(0, 0, _swap.Width, _swap.Height))
            .SetViewport(new GorgonViewport(0, 0, _swap.Width, _swap.Height))
            .ClearRenderTarget(_swap, GorgonColors.CornFlowerBlue)
            .ClearDepthStencil(_depthStencil, 1.0f, 0)
            .WriteConstant(0, _renderData)
            .Execute(_executeCall);

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
            _executeCall?.Dispose();
            _commands?.Dispose();
            _texture?.Dispose();
            _psoFactory.Dispose();
            _depthStencil.Dispose();
            _swap.Dispose();
        }
    }

    /// <summary>
    /// Function called when a key is pressed on the window.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void Window_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            _window.Close();
            return;
        }

        TriggerWobble();
    }

    /// <summary>
    /// Function called when a mouse button is pressed on the window.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void Window_MouseDown(object? sender, MouseEventArgs e) => TriggerWobble();

    /// <summary>
    /// Function to start the cube wobbling.
    /// </summary>
    private void TriggerWobble() => _renderData.Amplitude = StartAmplitude;

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
        _depthStencil = GorgonDepthStencilView.CreateDepthStencilView(_graphics, "Indirect Cube Depth Buffer", BufferFormat.D24_UNorm_S8_UInt, _swap.Width, _swap.Height);
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

            image = pngCodec.FromFile(Path.Combine(GorgonExample.GetResourcePath(@"Textures\GlassCube\").FullName, "Glass.png"));
            _texture = IGorgonTextureView<GorgonTexture>.CreateTexture(_graphics, "Indirect Cube Texture", image);

            image.Dispose();
            image = null;

            BuildExecuteCall(_texture);

            _renderData.Projection = _camera.GetProjectionMatrix();
            _renderData.Amplitude = StartAmplitude;
            _renderData.SliceCount = SliceCount;
            _renderData.TextureHandle = _texture.GetViewHandle();
            _renderData.SamplerHandle = GorgonSampler.Linear(_graphics).GetViewHandle();

            GorgonExample.LoadResources(_graphics);
            GorgonExample.EndInit();

            _window.Resize += Window_Resize;
            _window.KeyDown += Window_KeyDown;
            _window.MouseDown += Window_MouseDown;

            GorgonExample.Loop.Run(Idle);

            Application.Run(_window);
        }
        finally
        {
            image?.Dispose();
            _window.MouseDown -= Window_MouseDown;
            _window.KeyDown -= Window_KeyDown;
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
        _swap = new GorgonSwapChain(graphics, "Indirect Cube SwapChain", _window.Handle, new GorgonSwapChainInfo(_window.ClientSize.Width, _window.ClientSize.Height, BufferFormat.R8G8B8A8_UNorm));
        _depthStencil = GorgonDepthStencilView.CreateDepthStencilView(_graphics, "Indirect Cube Depth Buffer", BufferFormat.D24_UNorm_S8_UInt, _swap.Width, _swap.Height);
        _psoFactory = new GorgonGraphicsPsoFactory(_graphics);
        _camera = new GorgonPerspectiveCamera(new Vector2(_swap.Width, _swap.Height), 0.125f, 1000.0f)
        {
            Fov = 65.0f
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
            FormMain mainForm = GorgonExample.Initialize(ExampleConfig.Default.Resolution, "Indirect Cube");
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

            IReadOnlyList<GorgonVideoAdapterInfo> adapters = factory.EnumerateAdapters();

            if (adapters.Count == 0)
            {
                GorgonDialogs.Error(mainForm, "No valid Direct 3D 12 GPU was found on the system. Please see the log for details.");
                return;
            }

            using GorgonGraphics graphics = factory.CreateGraphics(adapters[0]);

            if (!graphics.FormatSupport[BufferFormat.R8G8B8A8_UNorm].IsDisplayFormat)
            {
                GorgonDialogs.Error(mainForm, "We should not see this error.");
                Application.Exit();
                return;
            }

            // We need a depth buffer so the back faces of the cube don't show through the front faces.
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
