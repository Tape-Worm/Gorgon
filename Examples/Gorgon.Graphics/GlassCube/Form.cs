
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
// Created: March 4, 2017 10:22:14 AM
// 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
/// The main window for our example
/// </summary>
public partial class Form : System.Windows.Forms.Form
{
    /// <summary>
    /// Data to tell the GPU what to render, how to render it.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct RenderData
    {
        /// <summary>
        /// The world/view/projection matrix used to transform our cube vertices.
        /// </summary>
        public Matrix4x4 WorldViewProjection;
        /// <summary>
        /// The handle to the vertex buffer containing the cube geometry.
        /// </summary>
        public int VertexBufferHandle;
        /// <summary>
        /// The handle to the texture sampler used to sample texels from the texture.
        /// </summary>
        public int SamplerHandle;
        /// <summary>
        /// The handle to the texture view to apply to the geometry.
        /// </summary>
        public int TextureHandle;
    }

    // The target delta time.
    private const float TargetDelta = 1 / 60.0f;

    // The cube to draw.
    private Cube? _cube;
    // The graphics object factory.
    private GorgonGraphicsFactory? _factory;
    // The primary graphics interface.    
    private GorgonGraphics? _graphics;
    // The swap chain for displaying the graphics.
    private GorgonSwapChain? _swap;
    private IGorgonTextureView<GorgonTexture>? _texture;
    // The factory used to cache and deliver pipeline state objects.
    private GorgonGraphicsPsoFactory? _psoFactory;
    // The draw call used to submit data to the GPU.
    private GorgonIndexedDrawCall? _drawCall;
    // The rotation to apply to the cube, in degrees.
    private Vector3 _rotation;
    // The current time.
    private float _accumulator;
    // The timer used for updating the text block.
    private readonly GorgonTimer _timer = new();
    // The projection matrix. Updates when the view is altered.
    private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;

    /// <summary>Handles the Resize event of the AfterSwapChain control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SwapChainResizedEventArgs" /> instance containing the event data.</param>
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if ((_swap is null) || (WindowState == FormWindowState.Minimized))
        {
            return;
        }

        _swap.Resize(ClientSize.Width, ClientSize.Height);
        _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded((80.0f).ToRadians(), (float)_swap.Width / _swap.Height, 0.1f, 1000f);
    }

    /// <summary>
    /// Function to update the world/view/projection matrix.
    /// </summary>
    /// <param name="world">The world matrix to update.</param>
    /// <param name="renderData">The render data to update.</param>
    /// <remarks>
    /// <para>
    /// This is what sends the transformation information for the model plus any view space transforms (projection & view) to the GPU so the shader can transform the vertices in the 
    /// model and project them into 2D space on your render target.
    /// </para>
    /// </remarks>
    private void UpdateWVP(ref readonly Matrix4x4 world, ref RenderData renderData)
    {
        ref Matrix4x4 wvp = ref renderData.WorldViewProjection;
        Matrix4x4 view = Matrix4x4.Multiply(world, Matrix4x4.CreateTranslation(0, 0, 1.5f));
        wvp = Matrix4x4.Transpose(Matrix4x4.Multiply(view, _projectionMatrix));
    }

    /// <summary>
    /// Function to handle idle time for the application.
    /// </summary>
    /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
    private bool Idle()
    {
        Debug.Assert(_graphics is not null && _swap is not null && _texture is not null && _cube is not null && _drawCall is not null, "Not initialized.");

        if (_timer.Milliseconds > 500)
        {
            LabelFPS.Text = $"FPS: {GorgonTiming.AverageFPS:0.0} Frame Delta: {GorgonTiming.AverageDelta * 1000: 0.0##} msec.";
            _timer.Reset();
        }

        int jitter1 = GorgonRandom.RandomInt32(1, 3);
        int jitter2 = GorgonRandom.RandomInt32(1, 3);
        int jitter3 = GorgonRandom.RandomInt32(1, 3);

        RenderData renderData = new()
        {
            SamplerHandle = CheckSmoothing.Checked ? GorgonSampler.Linear(_graphics).GetViewHandle() : GorgonSampler.Default(_graphics).GetViewHandle(),
            TextureHandle = _texture.GetViewHandle(),
            VertexBufferHandle = _cube.VertexBuffer.GetViewHandle()
        };

        // Use a fixed step timing to animate the cube.
        _accumulator += GorgonTiming.Delta;

        while (_accumulator >= TargetDelta)
        {
            // Spin the cube.
            _rotation.X += 25 * TargetDelta;
            _rotation.Y += 25 * TargetDelta;
            _rotation.Z += 25 * TargetDelta;

            if (_rotation.X >= 360.0f)
            {
                _rotation.X -= 360.0f;
            }

            if (_rotation.Y >= 360.0f)
            {
                _rotation.Y -= 360.0f;
            }

            if (_rotation.Z >= 360.0f)
            {
                _rotation.Z -= 360.0f;
            }

            if (!CheckSmoothing.Checked)
            {
                _cube.RotateXYZ(((int)_rotation.X / jitter1) * jitter1,
                                ((int)_rotation.Y / jitter2) * jitter2,
                                ((int)_rotation.Z / jitter3) * jitter3);
            }
            else
            {
                _cube.RotateXYZ(_rotation.X, _rotation.Y, _rotation.Z);
            }

            _accumulator -= TargetDelta;
        }

        UpdateWVP(in _cube.WorldMatrix, ref renderData);

        _swap.WaitForFrameLatency();

        GorgonCommandList list = _graphics.GetCommandList("Main command list")
                                          .AddPresenter(_swap)
                                          .ClearRenderTarget(_swap, GorgonColors.White)
                                          .SetScissorRectangle(new GorgonRectangle(0, 0, _swap.Width, _swap.Height))
                                          .SetViewport(new GorgonViewport(0, 0, _swap.Width, _swap.Height))
                                          .SetRenderTarget(_swap)
                                          .WriteConstant(0, in renderData);

        list.Draw(_drawCall);

        GorgonExample.BlitLogo(list);

        _graphics.Submit(list);

        return true;
    }

    /// <summary>
    /// Function to provide initialization for our example.
    /// </summary>
    /// <param name="graphics">The graphics interface to use.</param>
    /// <param name="swap">The swap chain to use.</param>
    private void Initialize(GorgonGraphics graphics, GorgonSwapChain swap)
    {
        // Load the texture.
        GorgonCodecPng pngCodec = new();
        using IGorgonImage image = pngCodec.FromFile(Path.Combine(GorgonExample.GetResourcePath(@"Textures\GlassCube\").FullName, "Glass.png"));
        _texture = IGorgonTextureView<GorgonTexture>.CreateTexture(graphics, "Cube Texture", image);

        // Create the pixel shader used to render our cube, and the vertex shader used to transform the cube geometry.
        using GorgonShaderCompiler compiler = new(graphics);
        GorgonShader _vertexShader = compiler.Compile(Resources.CubeShaders, "GlassCubeVS", ShaderType.VertexShader);
        GorgonShader _pixelShader = compiler.Compile(Resources.CubeShaders, "GlassCubePS", ShaderType.PixelShader);

        _psoFactory = new GorgonGraphicsPsoFactory(graphics);
        GorgonGraphicsPsoBuilder psoBuilder = new(graphics);
        psoBuilder.OutputFormat(swap.Format)
                  .PrimitiveType(PrimitiveType.TriangleList)
                  .BlendState(GorgonBlendState.Default, 0)
                  .RasterizerState(GorgonRasterState.NoCulling)
                  .PixelShader(_pixelShader);
        GorgonGraphicsPso cubePso = _psoFactory.CreateOrGetPso("Cube PSO", _vertexShader, psoBuilder);

        _cube = new Cube(graphics);

        _drawCall = new GorgonIndexedDrawCall(_cube.IndexBuffer.IndexCount, _cube.IndexBuffer, cubePso);
        _drawCall.AssignTexture(new GorgonUsedTexture(_texture, ShaderStage.Pixel, TextureUsage.ReadOnly));
        _drawCall.AssignBuffer(new GorgonUsedBuffer(_cube.VertexBuffer, ShaderStage.Vertex, BufferUsage.None));

        _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded((80.0f).ToRadians(), (float)swap.Width / swap.Height, 0.1f, 1000f);

        GorgonExample.LoadResources(graphics);
        GorgonExample.EndInit();

        _timer.Reset();
    }

    /// <summary>Processes a dialog box key.</summary>
    /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys">Keys</see> values that represents the key to process.</param>
    /// <returns>
    ///   <span class="keyword">
    ///     <span class="languageSpecificText">
    ///       <span class="cs">true</span>
    ///       <span class="vb">True</span>
    ///       <span class="cpp">true</span>
    ///     </span>
    ///   </span>
    ///   <span class="nu">
    ///     <span class="keyword">true</span> (<span class="keyword">True</span> in Visual Basic)</span> if the keystroke was processed and consumed by the control; otherwise, <span class="keyword"><span class="languageSpecificText"><span class="cs">false</span><span class="vb">False</span><span class="cpp">false</span></span></span><span class="nu"><span class="keyword">false</span> (<span class="keyword">False</span> in Visual Basic)</span> to allow further processing.
    /// </returns>
    protected override bool ProcessDialogKey(Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            Close();
            return true;
        }

        // Switch the sampler states when we hit the 'F' key (F - Filtering).
        if (keyData != Keys.F)
        {
            return base.ProcessDialogKey(keyData);
        }

        CheckSmoothing.Checked = !CheckSmoothing.Checked;
        return true;
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
    /// </summary>
    /// <param name="e">A <see cref="FormClosingEventArgs"></see> that contains the event data.</param>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        // Always clean up after yourself.
        GorgonExample.ShutDown();

        _psoFactory?.Dispose();
        _cube?.Dispose();
        _texture?.Dispose();
        _swap?.Dispose();
        _graphics?.Dispose();
        _factory?.Dispose();
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs"></see> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        try
        {
            GorgonExample.ResourceBaseDirectory = new DirectoryInfo(ExampleConfig.Default.ResourceLocation);
            ClientSize = LogicalToDeviceUnits(new Size(ExampleConfig.Default.Resolution.X, ExampleConfig.Default.Resolution.Y));
            CenterToScreen();

#if DEBUG
            _factory = new(GorgonGraphicsDebugFlags.SynchronizedCommandQueueValidation
                | GorgonGraphicsDebugFlags.GpuBasedValidation
                | GorgonGraphicsDebugFlags.GpuBasedStateTrackingValidation
                | GorgonGraphicsDebugFlags.ObjectTracking, log: GorgonExample.Log);
#else
            _factory = new(log: GorgonExample.Log);
#endif     

            // Ensure that we have an appropriate GPU that we can use.
            IReadOnlyList<GorgonVideoAdapterInfo> adapters = _factory.EnumerateAdapters();

            if (adapters.Count == 0)
            {
                GorgonDialogs.Error(this, "There are no suitable video adapters available in the system. This example is unable to continue and will now exit.");
                Application.Exit();
                return;
            }

            _graphics = _factory.CreateGraphics(adapters[0]);

            // Build our swap chain.
            _swap = new GorgonSwapChain(_graphics,
                                        "GlassCube SwapChain",
                                        Handle,
                                        new GorgonSwapChainInfo(new GorgonSwapChainInfo(ClientSize.Width, ClientSize.Height, BufferFormat.R8G8B8A8_UNorm)));

            // Initialize the app.
            Initialize(_graphics, _swap);

            // Assign idle event.
            GorgonExample.Loop.Run(Idle);
        }
        catch (Exception ex)
        {
            GorgonExample.HandleException(ex);
            Application.Exit();
        }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public Form() => InitializeComponent();
}
