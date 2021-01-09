#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: March 4, 2017 10:22:14 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Numerics;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Examples.Properties;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Timing;
using Gorgon.UI;

namespace Gorgon.Examples
{
    /// <summary>
    /// The main window for our example.
    /// </summary>
    public partial class Form : System.Windows.Forms.Form
    {
        #region Constants.
        // The target delta time.
        private const float TargetDelta = 1 / 60.0f;
        #endregion

        #region Variables.
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
        private void UpdateWVP(ref Matrix4x4 world)
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
        /// Function called after the swap chain is resized.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void AfterSwapChainResized(object sender, EventArgs eventArgs) => MatrixFactory.CreatePerspectiveFovLH(60.0f.ToRadians(), (float)ClientSize.Width / ClientSize.Height, 0.1f, 1000.0f, out _projectionMatrix);

        /// <summary>
		/// Function to handle idle time for the application.
		/// </summary>
		/// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
		private bool Idle()
        {
            // Do nothing here.  When we need to update, we will.
            _swap.RenderTargetView.Clear(GorgonColor.White);

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

            ref Matrix4x4 matrix = ref _cube.WorldMatrix;

            // Send our world matrix to the constant buffer so the vertex shader can update the vertices.
            UpdateWVP(ref matrix);

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
            _inputLayout = GorgonInputLayout.CreateUsingType<GlassCubeVertex>(_graphics, _vertexShader);

            // We use this as our initial world transform.
            // Since it's an identity, it will put the cube in the default orientation defined by the vertices.
            Matrix4x4 dummyMatrix = Matrix4x4.Identity;

            // Create our constant buffer so we can send our transformation information to the shader.
            _wvpBuffer = GorgonConstantBufferView.CreateConstantBuffer(_graphics, in dummyMatrix, "GlassCube WVP Constant Buffer");

            // Create a new projection matrix so we can transform from 3D to 2D space.
            MatrixFactory.CreatePerspectiveFovLH(60.0f.ToRadians(), (float)ClientSize.Width / ClientSize.Height, 0.1f, 1000.0f, out _projectionMatrix);
            // Pull the camera back 1.5 units on the Z axis. Otherwise, we'd end up inside of the cube.
            MatrixFactory.CreateTranslation(new Vector3(0, 0, 1.5f), out _viewMatrix);

            _cube = new Cube(_graphics, _inputLayout);

            // Assign our swap chain as the primary render target.
            _graphics.SetRenderTarget(_swap.RenderTargetView);

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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="System.Windows.Forms.KeyEventArgs"></see> that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }

            // Switch the sampler states when we hit the 'F' key (F - Filtering).
            if (e.KeyCode != Keys.F)
            {
                return;
            }

            _drawCall = _drawCall == _drawCallPixel ? _drawCallSmooth : _drawCallPixel;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
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
        /// <param name="e">An <see cref="System.EventArgs"></see> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                GorgonExample.ResourceBaseDirectory = new DirectoryInfo(Settings.Default.ResourceLocation);

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
                                            new GorgonSwapChainInfo("GlassCube SwapChain")
                                            {
                                                Format = BufferFormat.R8G8B8A8_UNorm,
                                                Width = ClientSize.Width,
                                                Height = ClientSize.Height,
                                                // We don't need this, as it's the default, but it's included for 
                                                // completeness.
                                                StretchBackBuffer = true
                                            });

                // Register an event to tell us when the swap chain was resized.
                // We need to do this in order to resize our projection matrix & viewport to match our client area.
                // Also, when a swap chain is resized, it is unbound from the pipeline, and needs to be reassigned to the draw call.
                _swap.SwapChainResized += AfterSwapChainResized;

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
        #endregion

        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        public Form() => InitializeComponent();
        #endregion
    }
}
