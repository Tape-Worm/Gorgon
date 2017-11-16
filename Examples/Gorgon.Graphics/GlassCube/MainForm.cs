﻿#region MIT
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
using System.Windows.Forms;
using DX = SharpDX;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Example;
using Gorgon.Graphics.Example.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Timing;
using Gorgon.UI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace GorgonLibrary.Example
{
    /// <summary>
    /// The main window for our example.
    /// </summary>
    public partial class formMain : Form
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
	    private GorgonConstantBuffer _wvpBuffer;
		// The pixel shader used to draw to the swap chain target.
	    private GorgonPixelShader _pixelShader;
		// The vertex shader used to transform the vertices.
	    private GorgonVertexShader _vertexShader;
		// The texture to apply to the cube.
	    private GorgonTexture _texture;
		// The draw call used to submit data to the GPU.
		private GorgonDrawIndexedCall _drawCall;
		// The cube to draw.
		private Cube _cube;
		// The view matrix that acts as the camera.
	    private DX.Matrix _viewMatrix;
		// The projection matrix to transform from 3D space into 2D space.
	    private DX.Matrix _projectionMatrix;
		// The rotation to apply to the cube, in degrees.
	    private DX.Vector3 _rotation;
		// The speed of rotation.
	    private DX.Vector3 _rotationSpeed = new DX.Vector3(1, 1, 1);
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
        private void UpdateWVP(ref DX.Matrix world)
		{

			// Build our world/view/projection matrix to send to
			// the shader.
			DX.Matrix.Multiply(ref world, ref _viewMatrix, out DX.Matrix temp);
			DX.Matrix.Multiply(ref temp, ref _projectionMatrix, out DX.Matrix wvp);

			// Direct 3D 11 requires that we transpose our matrix 
			// before sending it to the shader.
			DX.Matrix.Transpose(ref wvp, out wvp);

			// Update the constant buffer.
			_wvpBuffer.Update(ref wvp);
		}

		/// <summary>
		/// Function called after the swap chain is resized.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void AfterSwapChainResized(object sender, EventArgs eventArgs)
		{
			DX.Matrix.PerspectiveFovLH(60.0f.ToRadians(), (float)ClientSize.Width / ClientSize.Height, 0.1f, 1000.0f, out _projectionMatrix);
		}

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

	        _cube.GetWorldMatrix(out DX.Matrix worldMatrix);
			// Send our world matrix to the constant buffer so the vertex shader can update the vertices.
			UpdateWVP(ref worldMatrix);

            // And, as always, send the cube to the GPU for rendering.
			_graphics.Submit(_drawCall);

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
            IGorgonImageCodec pngCodec = new GorgonCodecPng();

			// Load the texture.
	        using (IGorgonImage image = pngCodec.LoadFromFile(Program.GetResourcePath(@"Textures\GlassCube\Glass.png")).ConvertToFormat(BufferFormat.R8G8B8A8_UNorm))
	        {
		        _texture = image.ToTexture("GlassCube Texture", _graphics);
	        }
            // Create our shaders.
            _vertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics.VideoDevice, Resources.GlassCubeShaders, "GlassCubeVS");
	        _pixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics.VideoDevice, Resources.GlassCubeShaders, "GlassCubePS");

			// Create the input layout for a cube vertex.
			_inputLayout = GorgonInputLayout.CreateUsingType<GlassCubeVertex>(_graphics.VideoDevice, _vertexShader);

			// Create our constant buffer so we can send our transformation information to the shader.
	        _wvpBuffer = new GorgonConstantBuffer("GlassCube WVP Constant Buffer",
	                                              _graphics,
	                                              new GorgonConstantBufferInfo
	                                              {
		                                              Usage = ResourceUsage.Default,
		                                              SizeInBytes = DX.Matrix.SizeInBytes
	                                              });

			// We use this as our initial world transform.
			// Since it's an identity, it will put the cube in the default orientation defined by the vertices.
	        DX.Matrix dummyMatrix = DX.Matrix.Identity;
	        
			// Create a new projection matrix so we can transform from 3D to 2D space.
			DX.Matrix.PerspectiveFovLH(60.0f.ToRadians(), (float)ClientSize.Width / ClientSize.Height, 0.1f, 1000.0f, out _projectionMatrix);
			// Pull the camera back 1.5 units on the Z axis. Otherwise, we'd end up inside of the cube.
			DX.Matrix.Translation(0, 0, 1.5f, out _viewMatrix);
			
			// Initialize the constant buffer.
			UpdateWVP(ref dummyMatrix);

	        _cube = new Cube(_graphics, _inputLayout);

            // Assign our swap chain as the primary render target.
            _graphics.SetRenderTarget(_swap.RenderTargetView);

			// Set up the pipeline to draw the cube.
            _drawCall = new GorgonDrawIndexedCall
                        {
                            PrimitiveTopology = PrimitiveTopology.TriangleList,
                            IndexBuffer = _cube.IndexBuffer,
                            VertexBuffers = _cube.VertexBuffer,
                            PixelShaderResourceViews =
                            {
                                [0] = _texture.DefaultShaderResourceView
                            },
                            PixelShaderSamplers =
                            {
                                // Start with bilinear filtering on the cube texture.
                                // This will smooth out the appearance of the texture as it is scaled closer or further away 
                                // from our view.
                                [0] = GorgonSamplerState.Default
                            },
                            VertexShaderConstantBuffers =
                            {
                                [0] = _wvpBuffer
                            },
                            IndexStart = 0,
                            IndexCount = _cube.IndexBuffer.Info.IndexCount,
                            PipelineState = _graphics.GetPipelineState(new GorgonPipelineStateInfo
                                                                       {
                                                                           PixelShader = _pixelShader,
                                                                           VertexShader = _vertexShader,
                                                                           // We turn off culling so we can see through the cube.
                                                                           RasterState = GorgonRasterState.NoCulling
                                                                       })
                        };
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"></see> that contains the event data.</param>
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

			if (_drawCall.PixelShaderSamplers[0] == GorgonSamplerState.Default)
	        {
		        _drawCall.PixelShaderSamplers[0] = GorgonSamplerState.PointFiltering;
	        }
	        else
	        {
		        _drawCall.PixelShaderSamplers[0] = GorgonSamplerState.Default;
	        }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

			// Always clean up after yourself.
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
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                // Initialize Gorgon as we have in the other examples.
				IGorgonVideoDeviceList devices = new GorgonVideoDeviceList();
				devices.Enumerate();

				_graphics = new GorgonGraphics(devices[0]);
                
				// Build our swap chain.
				_swap = new GorgonSwapChain("GlassCube SwapChain", _graphics, this, new GorgonSwapChainInfo
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
				_swap.AfterSwapChainResized += AfterSwapChainResized;

                // Initialize the app.
                Initialize();
                
                // Assign idle event.
	            GorgonApplication.IdleMethod = Idle;
            }
            catch (Exception ex)
            {
				ex.Catch(_ => GorgonDialogs.ErrorBox(this, _), GorgonApplication.Log);
                GorgonApplication.Quit();
            }
        }
	    #endregion

        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        public formMain()
        {
            InitializeComponent();
        }
        #endregion
    }
}
