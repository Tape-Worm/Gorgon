
// 
// Gorgon
// Copyright (C) 2017 Michael Winsor
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
// Created: March 4, 2017 10:22:14 AM
// 


using System.Numerics;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Renderers.Cameras;
using Gorgon.Renderers.Geometry;
using Gorgon.Timing;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Examples;

/// <summary>
/// The main window for our example
/// </summary>
public partial class Form : System.Windows.Forms.Form
{

    // The target delta time.
    private const float TargetDelta = 1 / 60.0f;



    // The primary graphics interface.
    private GorgonGraphics _graphics;
    // The swap chain for displaying the graphics.
    private GorgonSwapChain _swap;
    // The layout for a cube vertex.
    private GorgonInputLayout _inputLayout;
    // The constant buffer that will hold our world/view/projection matrix.
    private GorgonConstantBufferView _wvpBuffer;
    // The pixel shader used to draw to the swap chain target.
    private GorgonPixelShader _pixelShader;
    // The vertex shader used to transform the vertices.
    private GorgonVertexShader _vertexShader;
    // The texture to apply to the cube.
    private GorgonTexture2DView _texture;
    // The draw call used to submit data to the GPU, using a pixellated texture look.
    private GorgonDrawIndexCall _drawCallPixel;
    // The draw call used to submit data to the GPU, using a smooth look.
    private GorgonDrawIndexCall _drawCallSmooth;
    // The current draw call.
    private GorgonDrawIndexCall _drawCall;
    // The cube to draw.
    private Cube _cube;
    // The camera for viewing the cube.
    private GorgonPerspectiveCamera _camera;
    // The rotation to apply to the cube, in degrees.
    private Vector3 _rotation;
    // The current time.
    private float _accumulator;
    // The timer used for updating the text block.
    private IGorgonTimer _timer;



    /// <summary>Handles the Resize event of the AfterSwapChain control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SwapChainResizedEventArgs" /> instance containing the event data.</param>
    private void AfterSwapChain_Resize(object sender, SwapChainResizedEventArgs e)
    {
        if (_camera is null)
        {
            return;
        }

        _camera.ViewDimensions = e.Size.ToSize2F();
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
    private void UpdateWVP(ref readonly Matrix4x4 world)
    {
        // Get the view and projection from the camera.
        // These values are cached and returned as read only references for performance.
        ref readonly Matrix4x4 viewMatrix = ref _camera.GetViewMatrix();
        ref readonly Matrix4x4 projMatrix = ref _camera.GetProjectionMatrix();

        // Build our world/vi ew/projection matrix to send to
        // the shader.
        Matrix4x4 temp = Matrix4x4.Multiply(world, viewMatrix);
        // Direct 3D 11 requires that we transpose our matrix 
        // before sending it to the shader.
        Matrix4x4 wvp = Matrix4x4.Transpose(Matrix4x4.Multiply(temp, projMatrix));

        // Update the constant buffer.
        _wvpBuffer.Buffer.SetData(in wvp);
    }

    /// <summary>
    /// Function to handle idle time for the application.
    /// </summary>
    /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
    private bool Idle()
    {
        int jitter1 = GorgonRandom.RandomInt32(1, 3);
        int jitter2 = GorgonRandom.RandomInt32(1, 3);
        int jitter3 = GorgonRandom.RandomInt32(1, 3);

        if (_timer.Milliseconds > 500)
        {
            LabelFPS.Text = $"FPS: {GorgonTiming.AverageFPS:0.0} Frame Delta: {GorgonTiming.AverageDelta * 1000: 0.0##} msec.";
            _timer.Reset();
        }

        // Do nothing here.  When we need to update, we will.
        _swap.RenderTargetView.Clear(GorgonColor.White);

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

            if (_drawCall == _drawCallPixel)
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

        // Send our world matrix to the constant buffer so the vertex shader can update the vertices.
        UpdateWVP(in _cube.WorldMatrix);

        // And, as always, send the cube to the GPU for rendering.
        _graphics.Submit(_drawCall);

        GorgonExample.BlitLogo(_graphics);

        _swap.Present(1);
        return true;
    }

    /// <summary>
    /// Function to provide initialization for our example.
    /// </summary>
    private void Initialize()
    {
#if DEBUG
        GorgonGraphics.IsDebugEnabled = true;
        GorgonGraphics.IsObjectTrackingEnabled = true;
#endif
        // Load the texture.
        _texture = GorgonTexture2DView.FromFile(_graphics,
                                                Path.Combine(GorgonExample.GetResourcePath(@"Textures\GlassCube\").FullName, "Glass.png"),
                                                new GorgonCodecPng());

        // Create our shaders.
        _vertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics, Resources.GlassCubeShaders, "GlassCubeVS");
        _pixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics, Resources.GlassCubeShaders, "GlassCubePS");

        // Create the input layout for a cube vertex.
        _inputLayout = GorgonInputLayout.CreateUsingType<GorgonVertexPosUv>(_graphics, _vertexShader);

        // Create our constant buffer so we can send our transformation information to the shader.
        _wvpBuffer = GorgonConstantBufferView.CreateConstantBuffer(_graphics, new GorgonConstantBufferInfo(Unsafe.SizeOf<Matrix4x4>())
        {
            Name = "GlassCube WVP Constant Buffer"
        });

        // Pull the camera back 1.5 units on the Z axis. Otherwise, we'd end up inside of the cube.
        _camera = new GorgonPerspectiveCamera(_graphics, new DX.Size2F(ClientSize.Width, ClientSize.Height), 0.1f, 10.0f, "GlassCube Camera")
        {
            Fov = 60.0f,
            Position = new Vector3(0.0f, 0.0f, -1.5f)
        };

        _cube = new Cube(_graphics, _inputLayout);

        // Assign our swap chain as the primary render target.
        _graphics.SetRenderTarget(_swap.RenderTargetView);

        // Set up the pipeline to draw the cube.
        GorgonDrawIndexCallBuilder drawCallBuilder = new();
        GorgonPipelineStateBuilder stateBuilder = new(_graphics);

        // This draw call will use point filtering on the texture.
        _drawCall = _drawCallPixel = drawCallBuilder.VertexBuffer(_inputLayout, _cube.VertexBuffer[0])
                                                    .IndexBuffer(_cube.IndexBuffer, 0, _cube.IndexBuffer.IndexCount)
                                                    .ShaderResource(ShaderType.Pixel, _texture)
                                                    .ConstantBuffer(ShaderType.Vertex, _wvpBuffer)
                                                    .SamplerState(ShaderType.Pixel, GorgonSamplerState.PointFiltering)
                                                    .PipelineState(stateBuilder.PixelShader(_pixelShader)
                                                                               .VertexShader(_vertexShader)
                                                                               .RasterState(GorgonRasterState.NoCulling))
                                                    .Build();

        // And this one will use bilinear filtering.
        _drawCallSmooth = drawCallBuilder.SamplerState(ShaderType.Pixel, GorgonSamplerState.Default).Build();

        GorgonExample.LoadResources(_graphics);

        _timer = new GorgonTimerQpc();
    }

    /// <summary>Handles the Click event of the CheckSmoothing control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void CheckSmoothing_Click(object sender, EventArgs e) => _drawCall = _drawCall == _drawCallPixel ? _drawCallSmooth : _drawCallPixel;

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

        _drawCall = _drawCall == _drawCallPixel ? _drawCallSmooth : _drawCallPixel;
        CheckSmoothing.Checked = _drawCall == _drawCallSmooth;
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
        GorgonExample.UnloadResources();

        _cube?.Dispose();
        _texture?.Dispose();
        _wvpBuffer?.Dispose();
        _inputLayout?.Dispose();
        _pixelShader?.Dispose();
        _vertexShader?.Dispose();
        _swap?.Dispose();
        _graphics?.Dispose();
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

            // Initialize Gorgon as we have in the other examples.
            // Find out which devices we have installed in the system.
            IReadOnlyList<IGorgonVideoAdapterInfo> deviceList = GorgonGraphics.EnumerateAdapters();

            if (deviceList.Count == 0)
            {
                GorgonDialogs.ErrorBox(this, "There are no suitable video adapters available in the system. This example is unable to continue and will now exit.");
                GorgonApplication.Quit();
                return;
            }

            _graphics = new GorgonGraphics(deviceList[0]);

            // Build our swap chain.
            _swap = new GorgonSwapChain(_graphics,
                                        this,
                                        new GorgonSwapChainInfo(ClientSize.Width, ClientSize.Height, BufferFormat.R8G8B8A8_UNorm)
                                        {
                                            Name = "GlassCube SwapChain",
                                            // We don't need this, as it's the default, but it's included for 
                                            // completeness.
                                            StretchBackBuffer = true
                                        });

            // Register an event to tell us when the swap chain was resized.
            // We need to do this in order to resize our projection matrix & viewport to match our client area.
            // Also, when a swap chain is resized, it is unbound from the pipeline, and needs to be reassigned to the draw call.
            _swap.SwapChainResized += AfterSwapChain_Resize;
            ;

            // Initialize the app.
            Initialize();

            // Assign idle event.
            GorgonApplication.IdleMethod = Idle;
        }
        catch (Exception ex)
        {
            GorgonExample.HandleException(ex);
            GorgonApplication.Quit();
        }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public Form() => InitializeComponent();
}
