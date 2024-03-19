
// 
// Gorgon
// Copyright (C) 2023 Michael Winsor
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
// Created: February 19, 2023 1:30:47 PM
// 


using System.Numerics;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Renderers.Cameras;
using Gorgon.Renderers.Geometry;
using Gorgon.Timing;
using DX = SharpDX;

namespace Gorgon.Examples;

/// <summary>
/// GlassCube Avalonia
/// 
/// This is an Avalonia version of the GlassCube example. The purpose here is to show how to interoperate Gorgon with Avalonia
/// 
/// For the most part, the code for rendering is exactly the same as the original WinForms code. But, unlike the WinForms version, we add a new Control to 
/// the Window called the GorgonAvaloniaSwapChainControl. 
/// 
/// To enable Avalonia rendering, place the GorgonAvaloniaSwapChainControl control in your XAML (inside of a grid, canvas, whatever, doesn't matter). Once 
/// the application starts we create a new GorgonGraphics instance and pass it to the GorgonAvaloniaSwapChainControl.RunAsync method along with the Idle
/// method that we normally use. 
/// 
/// Because Avalonia handles things quite differently than WinForms, we do not use the GorgonApplication object. So, to get your code handling idle CPU time 
/// for rendering we cannot just assign the Idle property on GorgonApplication. The Idle is instead passed to the control directly via the RunAsync method. 
/// This RunAsync method is, as you can see, asynchronous, so it should be awaited
/// 
/// When we are rendering in the idle method, we just set the current render target to the target passed to the idle method and things will start appearing 
/// in amongst the controls of the Avalonia window
/// 
/// Now, for the bad news. There are several limitations due to the nature of Avalonia:
///   
/// * No exclusive full screen support. However, windowed full screen should still be possible if the Window is setup correctly
/// 
/// * Only the 32 bit BGRA format is supported for the render target. This is enforced by Avalonia
/// 
/// * A maximum frame rate of 60 frames per second is enforced by Avalonia. There is no stable way around this at this time
/// </summary>
public partial class MainWindow : Window
{

    // The target delta time.
    private const float TargetDelta = 1 / 60.0f;



    // The primary graphics interface.
    private GorgonGraphics _graphics;
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


    /// <summary>
    /// Function to update the texture smoothing on the cube.
    /// </summary>
    private void ChangeTextureSmoothing()
    {
        _drawCall = _drawCall == _drawCallPixel ? _drawCallSmooth : _drawCallPixel;
        CheckTextureSmooth.IsChecked = _drawCall != _drawCallPixel;
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
    /// Function to render the graphics.
    /// </summary>
    /// <param name="rtv">The swap chain render target.</param>
    /// <returns><b>true</b> to continue rendering, <b>false</b> to stop.</returns>
    private bool Idle(GorgonRenderTarget2DView rtv)
    {
        _graphics.SetRenderTarget(rtv);

        int jitter1 = GorgonRandom.RandomInt32(1, 3);
        int jitter2 = GorgonRandom.RandomInt32(1, 3);
        int jitter3 = GorgonRandom.RandomInt32(1, 3);

        if (_timer.Milliseconds > 500)
        {
            BlockFps.Text = $"FPS: {GorgonTiming.AverageFPS:0.0} Frame Delta: {GorgonTiming.AverageDelta * 1000: 0.0##} msec.";
            _timer.Reset();
        }

        // Do nothing here.  When we need to update, we will.
        rtv.Clear(GorgonColor.BlackTransparent);

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

        // Flush the command queue before giving control back to Avalonia.
        _graphics.Flush();
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
        _vertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics, Properties.Resources.GlassCubeShaders, "GlassCubeVS");
        _pixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics, Properties.Resources.GlassCubeShaders, "GlassCubePS");

        // Create the input layout for a cube vertex.
        _inputLayout = GorgonInputLayout.CreateUsingType<GorgonVertexPosUv>(_graphics, _vertexShader);

        // Create our constant buffer so we can send our transformation information to the shader.
        _wvpBuffer = GorgonConstantBufferView.CreateConstantBuffer(_graphics, new GorgonConstantBufferInfo(Unsafe.SizeOf<Matrix4x4>())
        {
            Name = "GlassCube WVP Constant Buffer"
        });

        // Pull the camera back 1.5 units on the Z axis. Otherwise, we'd end up inside of the cube.
        _camera = new GorgonPerspectiveCamera(_graphics, new DX.Size2F((float)ClientSize.Width, (float)ClientSize.Height), 0.1f, 10.0f, "GlassCube Camera")
        {
            Fov = 60.0f,
            Position = new Vector3(0.0f, 0.0f, -1.5f)
        };

        _cube = new Cube(_graphics, _inputLayout);

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


    /// <summary>
    /// Function called when the window is loaded.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The parameters for the event.</param>
    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        GorgonExample.ResourceBaseDirectory = new DirectoryInfo(ExampleConfig.Default.ResourceLocation);
        IReadOnlyList<IGorgonVideoAdapterInfo> adapters = GorgonGraphics.EnumerateAdapters();

        _graphics = new GorgonGraphics(adapters[0]);

        Initialize();

        // Begin running the application.
        await GorgonControl.RunAsync(_graphics, Idle);
    }

    /// <summary>
    /// Function called when a key is pressed.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }

        // Switch the sampler states when we hit the 'F' key (F - Filtering).
        if (e.Key != Key.F)
        {
            return;
        }

        ChangeTextureSmoothing();
    }

    /// <summary>
    /// Function called when a mouse button is pressed.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void Window_PointerPressed(object sender, PointerPressedEventArgs e) => BeginMoveDrag(e);

    /// <summary>
    /// Function called when the texture smooth checkbox is checked or unchecked.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">Event parameters.</param>
    private void CheckTextureSmooth_Click(object sender, RoutedEventArgs e) => ChangeTextureSmoothing();

    /// <summary>
    /// Function called when the close button is clicked.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">Event parameters.</param>
    private void ButtonClose_Click(object sender, RoutedEventArgs e) => Close();

    /// <summary>
    /// Function called when the Gorgon render control is detached from the logical tree.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    /// <remarks>
    /// <para>
    /// We use this event to perform any required clean up for the control.
    /// </para>
    /// </remarks>
    private void GorgonControl_DetachedFromLogicalTree(object sender, Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
    {
        // Always clean up after yourself.
        GorgonExample.UnloadResources();

        _cube?.Dispose();
        _texture?.Dispose();
        _wvpBuffer?.Dispose();
        _inputLayout?.Dispose();
        _pixelShader?.Dispose();
        _vertexShader?.Dispose();
        _graphics?.Dispose();
    }

    /// <summary>
    /// Function called when the Gorgon render control is resized.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void GorgonControl_Resized(object sender, RoutedEventArgs e)
    {
        if (_camera is null)
        {
            return;
        }

        // When our control is resized, we need the view to resize to avoid distortion.
        _camera.ViewDimensions = new DX.Size2F((float)ClientSize.Width, (float)ClientSize.Height);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow() => InitializeComponent();
}
