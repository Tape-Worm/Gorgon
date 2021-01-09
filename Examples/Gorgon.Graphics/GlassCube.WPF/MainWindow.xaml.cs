using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Numerics;
using Gorgon.Math;
using Gorgon.Graphics.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Timing;
using Gorgon.Graphics.Wpf;
using Gorgon.Core;

namespace Gorgon.Examples
{
    /// <summary>
    /// GlassCube WPF
    /// 
    /// This is a WPF version of the GlassCube example. The purpose here is to show how to interoperate Gorgon with WPF.
    /// 
    /// For the most part, the code for rendering is exactly the same as the original WinForms code. But, unlike the WinForms version, no swap chain is required 
    /// to display the image. Instead, WPF uses an Image control, with a D3D11Image as its image source. This setup gives us a render target that we can render 
    /// into like normal.
    /// 
    /// To enable WPF rendering, place an Image control in your XAML (inside of a grid, canvas, whatever, doesn't matter). Then, add a reference to the 
    /// Microsoft.Wpf.Interop.DirectX.dll (included in the Dependencies directory) assembly, and assign the D3D11Image to the ImageSource property of the Image 
    /// control. Ensure that your Image control has a name (x:Name attribute on the XAML). To see an example setup for this, look at the MainWindow.xaml file.
    /// 
    /// Once that is done, we create an instance of the GorgonWpfTarget object after we initialize the GorgonGraphics object and assign the Image control via the  
    /// constructor. 
    /// 
    /// Because WPF handles things quite differently than WinForms, we do not use the GorgonApplication object. So, to get your code handling idle CPU time for 
    /// rendering we cannot just assign the Idle property on GorgonApplication. Instead, we assign the rendering method by calling the Run method on the 
    /// GorgonWPFTarget. 
    /// 
    /// When rendering, just pass the GorgonWPFTarget.RenderTargetView to the GorgonGraphics.SetRenderTarget method and you're off the races.
    /// 
    /// And that is all that's required to get Gorgon working with WPF.
    /// 
    /// Now, for the bad news. There are several limitations due to the nature of WPF:
    /// * This example, and anything using the Microsoft.Wpf.Interop.DirectX assembly MUST be compiled as x64. The assembly is compiled to x64 by default. If x86 
    ///   is required, developers can grab the source for the assembly from https://github.com/Microsoft/WPFDXInterop and compile it as x86. Then replace the x64 
    ///   reference with the x86 reference in the Gorgon.Graphics.WPF project and then recompile that as x86. This, however, is not recommended and unsupported.
    ///   
    /// * No exclusive full screen support. However, windowed full screen should still be possible if the Window is setup correctly.
    /// 
    /// * Only the 32 bit BGRA format is supported for the render target. This is enforced by WPF.
    /// 
    /// * A maximum frame rate of 60 frames per second is enforced by WPF. There is no stable way around this at this time.
    /// </summary>
    public partial class MainWindow 
        : Window
    {
        #region Constants.
        // The target delta time.
        private const float TargetDelta = 1 / 60.0f;
        #endregion

        #region Variables.
        // The primary graphics interface.
        private GorgonGraphics _graphics;
        // The WPF render target. This is where we'll send our rendering.
        private GorgonWpfTarget _target;
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
        // The view matrix that acts as the camera.
        private Matrix4x4 _viewMatrix;
        // The projection matrix to transform from 3D space into 2D space.
        private Matrix4x4 _projectionMatrix;
        // The rotation to apply to the cube, in degrees.
        private Vector3 _rotation;
        // The speed of rotation.
        private Vector3 _rotationSpeed = new Vector3(1, 1, 1);
        // The current time.
        private float _accumulator;
        // The timer used for updating the text block.
        private IGorgonTimer _timer;
        #endregion

        #region Methods.
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
            world.Multiply(in _viewMatrix, out Matrix4x4 temp);
            temp.Multiply(in _projectionMatrix, out Matrix4x4 wvp);

            // Direct 3D 11 requires that we transpose our matrix 
            // before sending it to the shader.
            wvp.Transpose(out wvp);

            // Update the constant buffer.
            _wvpBuffer.Buffer.SetData(in wvp);
        }

        /// <summary>
		/// Function to handle idle time for the application.
		/// </summary>
		/// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
		private bool Idle()
        {
            if ((BlockFps.IsVisible) && (_timer.Milliseconds > 500))
            {
                BlockFps.Text = $"FPS: {GorgonTiming.AverageFPS:0.0} Frame Delta: {GorgonTiming.AverageDelta * 1000: 0.0##} msec.";
                _timer.Reset();
            }

            // Do nothing here.  When we need to update, we will.
            _target.RenderTargetView.Clear(GorgonColor.BlackTransparent);

            // In order to get our rendering to show up in WPF, we need to render to the render target view provided to us by the 
            // WPF render target.
            _graphics.SetRenderTarget(_target.RenderTargetView);

            // Use a fixed step timing to animate the cube.
            _accumulator += GorgonTiming.Delta;

            while (_accumulator >= TargetDelta)
            {
                // Spin the cube.
                _rotation.X += GorgonRandom.RandomSingle(45, 90) * TargetDelta * (_rotationSpeed.X.FastSin());
                _rotation.Y += GorgonRandom.RandomSingle(45, 90) * TargetDelta * (_rotationSpeed.Y.FastSin());
                _rotation.Z += GorgonRandom.RandomSingle(45, 90) * TargetDelta * (_rotationSpeed.Z.FastSin());

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

                _rotationSpeed.X += TargetDelta / 6f;
                _rotationSpeed.Y += TargetDelta / 6f;
                _rotationSpeed.Z += TargetDelta / 6f;

                if (_rotationSpeed.X > 6.28319f)
                {
                    _rotationSpeed.X = 0;
                }

                if (_rotationSpeed.Y > 6.28319f)
                {
                    _rotationSpeed.Y = 0;
                }

                if (_rotationSpeed.Z > 6.28319f)
                {
                    _rotationSpeed.Z = 0;
                }

                _cube.RotateXYZ(_rotation.X, _rotation.Y, _rotation.Z);
                _accumulator -= TargetDelta;
            }

            ref readonly Matrix4x4 matrix = ref _cube.WorldMatrix;

            // Send our world matrix to the constant buffer so the vertex shader can update the vertices.
            UpdateWVP(in matrix);

            // And, as always, send the cube to the GPU for rendering.
            _graphics.Submit(_drawCall);

            GorgonExample.BlitLogo(_graphics);

            // You will notice that we do not need to call a Present method here anymore. This is handled entirely by the 
            // GorgonWPFTarget object on your behalf.
            //
            // To force a render, users may call GorgonGraphics.Flush().

            return true;
        }

        /// <summary>
        /// Function to provide initialization for our example.
        /// </summary>
        private void Initialize()
        {
            // Load the texture.
            _texture = GorgonTexture2DView.FromFile(_graphics,
                                                    System.IO.Path.Combine(GorgonExample.GetResourcePath(@"Textures\GlassCube\").FullName, "Glass.png"),
                                                    new GorgonCodecPng());

            // Create our shaders.
            _vertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics, Properties.Resources.GlassCubeShaders, "GlassCubeVS");
            _pixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics, Properties.Resources.GlassCubeShaders, "GlassCubePS");

            // Create the input layout for a cube vertex.
            _inputLayout = GorgonInputLayout.CreateUsingType<GlassCubeVertex>(_graphics, _vertexShader);

            // We use this as our initial world transform.
            // Since it's an identity, it will put the cube in the default orientation defined by the vertices.
            Matrix4x4 dummyMatrix = Matrix4x4.Identity;

            // Create our constant buffer so we can send our transformation information to the shader.
            _wvpBuffer = GorgonConstantBufferView.CreateConstantBuffer(_graphics, in dummyMatrix, "GlassCube WVP Constant Buffer");

            // Create a new projection matrix so we can transform from 3D to 2D space.            
            MatrixFactory.CreatePerspectiveFovLH(60.0f.ToRadians(), (float)(ActualWidth / ActualHeight), 0.1f, 1000.0f, out _projectionMatrix);
            // Pull the camera back 1.5 units on the Z axis. Otherwise, we'd end up inside of the cube.
            MatrixFactory.CreateTranslation(new Vector3(0, 0, 1.5f), out _viewMatrix);

            _cube = new Cube(_graphics, _inputLayout);

            // Assign our swap chain as the primary render target.
            //_graphics.SetRenderTarget(_swap.RenderTargetView);

            // Set up the pipeline to draw the cube.
            var drawCallBuilder = new GorgonDrawIndexCallBuilder();
            var stateBuilder = new GorgonPipelineStateBuilder(_graphics);

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

        /// <summary>Handles the Loaded event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                GorgonExample.ResourceBaseDirectory = new System.IO.DirectoryInfo(Properties.Settings.Default.ResourceLocation);                

                // Initialize Gorgon as we have in the other examples.
                // Find out which devices we have installed in the system.
                IReadOnlyList<IGorgonVideoAdapterInfo> deviceList = GorgonGraphics.EnumerateAdapters();

                if (deviceList.Count == 0)
                {
                    MessageBox.Show("There are no suitable video adapters available in the system. This example is unable to continue and will now exit.", 
                                    "Error", 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Error);

                    Close();
                    return;
                }

                _graphics = new GorgonGraphics(deviceList[0]);

                // Unlike the Windows Forms version of this example, we don't need to use a swap chain. Instead, we use this 
                // render target type to get our rendering to a WPF surface.
                //
                // The D3DImage object we're passing in is a standard WPF Image control with a D3D11Image object assigned to 
                // its ImageSource property.
                _target = new GorgonWpfTarget(_graphics, new GorgonWpfTargetInfo(D3DImage, "WPF Render Target"));
                                
                Initialize();

                // This is where we kick off our rendering. And again, unlike the Windows Forms version of the example, we 
                // do not need to assign the Idle method to the GorgonApplication class. Instead we pass it to the Run method 
                // on the GorgonWPFTarget. This will call the Idle method when WPF requires rendering to be performed. 
                _target.Run(Idle);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        /// <summary>Handles the Closing event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs" /> instance containing the event data.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Always clean up after yourself.
            GorgonExample.UnloadResources();

            _cube?.Dispose();
            _texture?.Dispose();
            _wvpBuffer?.Dispose();
            _inputLayout?.Dispose();
            _pixelShader?.Dispose();
            _vertexShader?.Dispose();
            _target?.Dispose();
            _graphics?.Dispose();
        }

        /// <summary>
        /// Function to update the texture smoothing on the cube.
        /// </summary>
        private void ChangeTextureSmoothing()
        {
            _drawCall = _drawCall == _drawCallPixel ? _drawCallSmooth : _drawCallPixel;
            CheckTextureSmooth.IsChecked = _drawCall != _drawCallPixel;
        }

        /// <summary>Handles the KeyDown event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            base.OnKeyDown(e);

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

        /// <summary>Handles the Click event of the Button control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void Button_Click(object sender, RoutedEventArgs e) => Close();

        /// <summary>Handles the Click event of the CheckTextureSmooth control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void CheckTextureSmooth_Click(object sender, RoutedEventArgs e) => ChangeTextureSmoothing();
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="MainWindow" /> class.</summary>
        public MainWindow() => InitializeComponent();
        #endregion        
    }
}
