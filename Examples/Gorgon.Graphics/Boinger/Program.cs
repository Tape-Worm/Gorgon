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
// Created: March 2, 2017 7:46:37 PM
// 
#endregion

using System;
using System.Numerics;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.GdiPlus;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Renderers.Cameras;
using Gorgon.Renderers.Geometry;
using Gorgon.Timing;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Examples
{
    /// <summary>
	/// This is an example of using the base graphics API.  It's very similar to how Direct 3D 11 works, but with some enhancements
	/// to deal with poor error support and other "gotchas" that tend to pop up.  It also has some time saving functionality to
	/// deal with mundane tasks like setting up a swap chain, pixel shaders, etc...
	/// 
	/// This example is a recreation of the Amiga "Boing" demo (https://www.youtube.com/watch?v=8EpOq5H8wUI).
	/// 
	/// 
	/// Before I go any further: This example is NOT a good example of how to write a 3D application.  
	/// A good 3D renderer is a monster to write, this example just shows a user the flexibility of Gorgon and that it's capable 
	/// of rendering 3D with the lower API level.  Any funky 3D only tricks or complicated scene graph mechanisms that you might 
	/// expect are up to the developer to figure out and write.
	/// 
	/// Anyway, on with the show....
	/// 
	/// In this example we create a swap chain, and set up the application for 3D rendering and build 2 types of objects:  
	/// * 2 planes (1 for the floor and another for the rear wall)
	/// * A sphere.  
	/// 
	/// Once the initialization is done, we render the objects.  We transform the sphere using a world matrix for its rotation,
	/// translation and scaling.  Note that there's a shadow under the sphere, this is just the same sphere drawn again without
	/// a texture and a diffuse hardcoded shader (see shader.hlsl).  To get the shadow in there, we turn off depth-writing, which
	/// enables us to render the shadow without it interferring with any geometry but still respecting the depth buffer.
	/// 
	/// One thing to note is the use of the 2D renderer for drawing text.  I had 2 options here:
	/// 1. Draw the text manually myself.  And, there was no way in hell I was doing that.
	/// 2. Use the 2D renderer.
	/// 
	/// You'll note that in the render loop, before we render the text, we call _2D.Begin().  This sets up the initial state for
	/// 2D rendering.  Then we call the 2D functions to render a little window, and some text. And finally, we call _2D.End() and
	/// that renders the batched 2D commands.
	/// 
	/// This example is considered advanced, and a firm understanding of a graphics API like Direct 3D 11.2 is recommended.
	/// It's also very "low level", in that there's not a whole lot that's done for you by the API.  It's a very manual process to 
	/// get everything initialized and thus there's a lot of set up code.  This is unlike the 2D renderer, which takes very little
	/// effort to get up and running (technically, you barely have to touch the base graphics library to get the 2D renderer doing
	/// something useful).
	/// </summary>
	internal static class Program
    {
        #region Variables.
        // Format for our depth/stencil buffer.
        private static BufferFormat _depthFormat;
        // Main application form.
        private static FormMain _mainForm;
        // The graphics interface for the application.
        private static GorgonGraphics _graphics;
        // Our primary swap chain.
        private static GorgonSwapChain _swap;
        // The depth buffer.
        private static GorgonDepthStencil2DView _depthBuffer;
        // Our primary vertex shader.
        private static GorgonVertexShader _vertexShader;
        // Our primary pixel shader.	
        private static GorgonPixelShader _pixelShader;
        // Input layout.
        private static GorgonInputLayout _inputLayout;
        // Our texture.    
        private static GorgonTexture2DView _texture;
        // Our world/view/project matrix buffer.
        private static GorgonConstantBufferView _wvpBuffer;
        // Buffer holding our material.
        private static GorgonConstantBufferView _materialBuffer;
        // 2D interface for rendering our text.
        private static Gorgon2D _2D;
        // The camera.
        private static GorgonPerspectiveCamera _camera;
        // Our walls.
        private static Plane[] _planes;
        // Our sphere.
        private static Sphere _sphere;
        // Horizontal bounce.
        private static bool _bounceH;
        // Vertical bounce.
        private static bool _bounceV = true;
        // Ball rotation.
        private static float _rotate = -45.0f;
        // Rotation speed.
        private static float _rotateSpeed = 1.0f;
        // Drop speed.
        private static float _dropSpeed = 0.01f;
        // The selected video mode.
        private static GorgonVideoMode _selectedVideoMode;
        // The draw call used to render the plane(s).
        private static readonly GorgonDrawIndexCall[] _drawCalls = new GorgonDrawIndexCall[3];
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the ball position.
        /// </summary>
        private static void UpdateBall()
        {
            Vector3 position = _sphere.Position;

            if (_bounceV)
            {
                _dropSpeed += 9.8f * GorgonTiming.Delta;
            }
            else
            {
                _dropSpeed -= 9.8f * GorgonTiming.Delta;
            }

            if (!_bounceH)
            {
                position.X -= 4.0f * GorgonTiming.Delta;
            }
            else
            {
                position.X += 4.0f * GorgonTiming.Delta;
            }

            if (!_bounceV)
            {
                position.Y += 4.0f * GorgonTiming.Delta * (_dropSpeed / 20f);
            }
            else
            {
                position.Y -= 4.0f * GorgonTiming.Delta * (_dropSpeed / 20f);
            }

            if (position.X is < (-2.3f) or > 2.3f)
            {
                _bounceH = !_bounceH;
                if (_bounceH)
                {
                    position.X = -2.3f;
                }
                else
                {
                    position.X = 2.3f;
                }
            }

            if (position.Y is > 2.0f or < (-2.5f))
            {
                _bounceV = !_bounceV;
                if (!_bounceV)
                {
                    position.Y = -2.5f;
                    _dropSpeed = 20f;
                }
            }

            _sphere.Rotation = new Vector3(0, _rotate, -12.0f);
            _sphere.Position = position;

            _rotate += 90.0f * GorgonTiming.Delta * (_rotateSpeed.Sin() * 1.5f);
            _rotateSpeed += GorgonTiming.Delta / 1.25f;
        }

        /// <summary>
        /// Function to send model data to the GPU for rendering.
        /// </summary>
        /// <param name="model">The model containing the data to render.</param>
        private static void RenderModel(Model model)
        {
            // Send the transform for the model to the GPU so we can update its position and rotation.
            model.UpdateTransform();
            UpdateWVP(in model.WorldMatrix);

            GorgonColor color = model.Material.Diffuse;
            _materialBuffer.Buffer.SetData(in color, copyMode: CopyMode.Discard);

            GorgonDrawIndexCall drawCall = model == _sphere ? _drawCalls[2] : (model == _planes[0] ? _drawCalls[0] : _drawCalls[1]);

            // Finally, send the draw call to the GPU.
            _graphics.Submit(drawCall);
        }

        /// <summary>
		/// Function to handle idle time for the application.
		/// </summary>
		/// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
		private static bool Idle()
        {
            // Animate the ball.
            UpdateBall();

            // Clear to our gray color and clear out the depth buffer.
            _swap.RenderTargetView.Clear(Color.FromArgb(173, 173, 173));
            //_depthBuffer.Clear(0.0f, 0);
            _depthBuffer.Clear(0.0f, 0);

            // Render the back and floor planes.
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < _planes.Length; ++i)
            {
                RenderModel(_planes[i]);
            }

            // Render the ball.
            _sphere.Material.Diffuse = GorgonColor.White;
            RenderModel(_sphere);

            // Remember the position and rotation so we can restore them later.
            Vector3 spherePosition = _sphere.Position;
            Vector3 sphereRotation = _sphere.Rotation;

            // Offset the position of the ball so we can fake a shadow under the ball.
            _sphere.Position = new Vector3(spherePosition.X + 0.25f, spherePosition.Y - 0.125f, spherePosition.Z + 0.5f);
            // Scale on the z-axis so the ball "shadow" has no real depth, and on the x & y to make it look slightly bigger.
            _sphere.Scale = new Vector3(1.155f, 1.155f, 0.001f);
            // Reset the rotation so we don't rotate our flattened ball "shadow" (it'd look real weird if it rotated).
            _sphere.Rotation = Vector3.Zero;
            // Render as black with alpha of 0.5 to simulate a shadow.
            _sphere.Material.Diffuse = new GorgonColor(0, 0, 0, 0.5f);

            // Render the shadow.
            RenderModel(_sphere);

            // Restore our original positioning so we can render the ball in the correct place on the next frame.
            _sphere.Position = spherePosition;
            // Reset scale on the z-axis so the ball so it'll be normal for the next frame.
            _sphere.Scale = Vector3.One;
            // Reset the rotation so it'll be in the correct place on the next frame.
            _sphere.Rotation = sphereRotation;

            // Draw our text.
            // Use this to show how incredibly slow and terrible my 3D code is.
            GorgonExample.DrawStatsAndLogo(_2D);

            // Now we flip our buffers.
            // We need to this or we won't see anything.
            _swap.Present(1);

            return true;
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
        private static void UpdateWVP(in Matrix4x4 world)
        {
            // Build our world/view/projection matrix to send to
            // the shader.
            ref readonly Matrix4x4 viewMatrix = ref _camera.GetViewMatrix();
            ref readonly Matrix4x4 projMatrix = ref _camera.GetProjectionMatrix();

            var temp = Matrix4x4.Multiply(world, viewMatrix);
            var wvp = Matrix4x4.Multiply(temp, projMatrix);

            // Direct 3D 11 requires that we transpose our matrix 
            // before sending it to the shader.
            wvp = Matrix4x4.Transpose(wvp);

            // Update the constant buffer.
            _wvpBuffer.Buffer.SetData(in wvp);
        }

        /// <summary>
        /// Function to create the primary graphics interface.
        /// </summary>
        /// <returns>A new graphics interface, or <b>null</b> if a suitable video device was not found on the system.</returns>
        /// <remarks>
        /// <para>
        /// This method will create a new graphics interface for our application to use. It will select the video device with the most suitable depth buffer available, and if it cannot find a suitable 
        /// device, it will indicate that by returning <b>null</b>.
        /// </para>
        /// </remarks>
        private static GorgonGraphics CreateGraphicsInterface()
        {
            GorgonGraphics graphics = null;
            BufferFormat[] depthFormats =
            {
                BufferFormat.D32_Float,
                BufferFormat.D32_Float_S8X24_UInt,
                BufferFormat.D24_UNorm_S8_UInt,
                BufferFormat.D16_UNorm
            };

            // Find out which devices we have installed in the system.
            IReadOnlyList<IGorgonVideoAdapterInfo> deviceList = GorgonGraphics.EnumerateAdapters();

            if (deviceList.Count == 0)
            {
                GorgonDialogs.ErrorBox(_mainForm, "There are no suitable video adapters available in the system. This example is unable to continue and will now exit.");
                return null;
            }

            int depthFormatIndex = 0;
            int selectedDeviceIndex = 0;
            IGorgonVideoAdapterInfo selectedDevice = null;

            _selectedVideoMode = new GorgonVideoMode(ExampleConfig.Default.Resolution.Width, ExampleConfig.Default.Resolution.Height, BufferFormat.R8G8B8A8_UNorm);

            while (selectedDeviceIndex < deviceList.Count)
            {
                // Reset back to a 32 bit floating point depth.
                _depthFormat = depthFormats[depthFormatIndex++];

                // Destroy the previous interface.
                graphics?.Dispose();

                // Create the main graphics interface.
                graphics = new GorgonGraphics(deviceList[selectedDeviceIndex++]);

                // Validate depth buffer for this device.
                // Odds are good that if this fails, you should probably invest in a better video card.  Preferably something created after 2005.
                if (!graphics.FormatSupport[_depthFormat].IsDepthBufferFormat)
                {
                    continue;
                }

                selectedDevice = graphics.VideoAdapter;
                break;
            }

            // If, somehow, we are on a device from the dark ages, then we can't continue.
            if (selectedDevice is not null)
            {
                return graphics;
            }

            GorgonDialogs.ErrorBox(_mainForm, $"The selected video device ('{deviceList[0].Name}') does not support a 32, 24 or 16 bit depth buffer.");
            return null;
        }

        /// <summary>
        /// Function to build or rebuild the depth buffer.
        /// </summary>
        /// <param name="width">The width of the depth buffer.</param>
        /// <param name="height">The height of the depth buffer.</param>
	    private static void BuildDepthBuffer(int width, int height)
        {
            _graphics.SetDepthStencil(null);
            _depthBuffer?.Dispose();
            _depthBuffer = GorgonDepthStencil2DView.CreateDepthStencil(_graphics,
                                                                       new GorgonTexture2DInfo("Depth Buffer")
                                                                       {
                                                                           Usage = ResourceUsage.Default,
                                                                           Width = width,
                                                                           Height = height,
                                                                           Format = _depthFormat
                                                                       });
        }

        /// <summary>
        /// Function to initialize the GPU resource objects.
        /// </summary>
	    private static void InitializeGpuResources()
        {
            _graphics = CreateGraphicsInterface();

            // If we couldn't create the graphics interface, then leave.
            if (_graphics is null)
            {
                return;
            }

            // Create a 1280x800 window with a depth buffer.
            // We can modify the resolution in the config file for the application, but like other Gorgon examples, the default is 1280x800.
            _swap = new GorgonSwapChain(_graphics,
                                        _mainForm,
                                        new GorgonSwapChainInfo("Main")
                                        {
                                            // Set up for 32 bit RGBA normalized display.
                                            Format = BufferFormat.R8G8B8A8_UNorm,
                                            Width = ExampleConfig.Default.Resolution.Width,
                                            Height = ExampleConfig.Default.Resolution.Height
                                        });

            // Build the depth buffer for our swap chain.
            BuildDepthBuffer(_swap.Width, _swap.Height);


            if (!ExampleConfig.Default.IsWindowed)
            {
                // Get the output for the main window.
                var currentScreen = Screen.FromControl(_mainForm);
                IGorgonVideoOutputInfo output = _graphics.VideoAdapter.Outputs[currentScreen.DeviceName];

                // If we've asked for full screen mode, then locate the correct video mode and set us up.
                _selectedVideoMode = new GorgonVideoMode(ExampleConfig.Default.Resolution.Width, ExampleConfig.Default.Resolution.Height, BufferFormat.R8G8B8A8_UNorm);
                _swap.EnterFullScreen(in _selectedVideoMode, output);
            }

            // Handle resizing because the projection matrix and depth buffer needs to be updated to reflect the new view size.
            _swap.SwapChainResizing += Swap_BeforeResized;
            _swap.SwapChainResized += Swap_AfterResized;

            // Set the current render target output so we can see something.
            _graphics.SetRenderTarget(_swap.RenderTargetView, _depthBuffer);

            // Create our shaders.
            // Our vertex shader.  This is a simple shader, it just processes a vertex by multiplying it against
            // the world/view/projection matrix and spits it back out.
            _vertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics, Resources.Shader, "BoingerVS");

            // Our main pixel shader.  This is a very simple shader, it just reads a texture and spits it back out.  Has no
            // diffuse capability.
            _pixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics, Resources.Shader, "BoingerPS");

            // Create the vertex input layout.
            // We need to create a layout for our vertex type because the shader won't know how to interpret the data we're sending it otherwise.  
            // This is why we need a vertex shader before we even create the layout.
            _inputLayout = GorgonInputLayout.CreateUsingType<GorgonVertexPosUv>(_graphics, _vertexShader);

            // Resources are stored as System.Drawing.Bitmap files, so we need to convert into an IGorgonImage so we can upload it to a texture.
            // We also will generate mip-map levels for this image so that scaling the texture will look better. 
            using (IGorgonImage image = Resources.Texture.ToGorgonImage())
            {
                _texture = image.ToTexture2D(_graphics,
                                             new GorgonTexture2DLoadOptions
                                             {
                                                 Usage = ResourceUsage.Immutable,
                                                 Name = "Texture"
                                             })
                                .GetShaderResourceView();
            }

            // Create our constant buffer.			
            // Our constant buffers are how we send data to our shaders.  This one in particular will be responsible for sending our world/view/projection matrix 
            // to the vertex shader.  
            _wvpBuffer = GorgonConstantBufferView.CreateConstantBuffer(_graphics,
                                                                       new GorgonConstantBufferInfo("WVPBuffer")
                                                                       {
                                                                           Usage = ResourceUsage.Dynamic,
                                                                           SizeInBytes = Unsafe.SizeOf<Matrix4x4>()
                                                                       });
            // This one will hold our material information.
            _materialBuffer = GorgonConstantBufferView.CreateConstantBuffer(_graphics,
                                                                            new GorgonConstantBufferInfo("MaterialBuffer")
                                                                            {
                                                                                Usage = ResourceUsage.Dynamic,
                                                                                SizeInBytes = Unsafe.SizeOf<GorgonColor>()
                                                                            });
            GorgonColor defaultMaterialColor = GorgonColor.White;
            _materialBuffer.Buffer.SetData(in defaultMaterialColor);

            GorgonExample.LoadResources(_graphics);
        }

        /// <summary>
        /// Function to initialize the states for the objects to draw.
        /// </summary>
	    private static void InitializeStates()
        {
            var drawBuilder = new GorgonDrawIndexCallBuilder();
            var stateBuilder = new GorgonPipelineStateBuilder(_graphics);
            GorgonSamplerState sampler = new GorgonSamplerStateBuilder(_graphics)
                                         .Filter(SampleFilter.MinMagMipPoint)
                                         .Wrapping(TextureWrap.Wrap, TextureWrap.Wrap)
                                         .Build();


            // Initialize our draw call so we can render the objects.
            // All objects are using triangle lists, so we must tell the draw call that's what we need to render.
            for (int i = 0; i < 2; ++i)
            {
                _planes[i].Material.TextureSampler = sampler;
                _drawCalls[i] = drawBuilder.VertexBuffer(_inputLayout, _planes[i].VertexBufferBindings[0])
                                           .IndexBuffer(_planes[i].IndexBuffer, 0, _planes[i].IndexBuffer.IndexCount)
                                           .SamplerState(ShaderType.Pixel, sampler)
                                           .ShaderResource(ShaderType.Pixel, _planes[i].Material.Texture)
                                           .ConstantBuffer(ShaderType.Vertex, _wvpBuffer)
                                           .ConstantBuffer(ShaderType.Pixel, _materialBuffer)
                                           .PipelineState(stateBuilder.DepthStencilState(GorgonDepthStencilState.DepthStencilEnabledGreaterEqual)
                                                                      .PixelShader(_pixelShader)
                                                                      .VertexShader(_vertexShader))
                                           .Build();
            }

            // For our sphere, we can just reuse the builder(s) since only a small part of the resources have changed.
            _sphere.Material.TextureSampler = sampler;
            _drawCalls[2] = drawBuilder.VertexBuffer(_inputLayout, _sphere.VertexBufferBindings[0])
                                       .ShaderResource(ShaderType.Pixel, _sphere.Material.Texture)
                                       .IndexBuffer(_sphere.IndexBuffer, 0, _sphere.IndexBuffer.IndexCount)
                                       .Build();

            // Set up our camera.
            _camera = new GorgonPerspectiveCamera(_graphics, new DX.Size2F(_swap.Width, _swap.Height), 500.0f, 0.125f)
            {
                Fov = 75,
                Position = new Vector3(0, 0, -2.2f)
            };
        }

        /// <summary>
        /// Function to initialize the application.
        /// </summary>
        private static void Initialize()
        {
            try
            {
                // Create our form.
                _mainForm = GorgonExample.Initialize(new DX.Size2(ExampleConfig.Default.Resolution.Width, ExampleConfig.Default.Resolution.Height), "Boinger");

                // Add a keybinding to switch to full screen or windowed.
                _mainForm.KeyDown += MainForm_KeyDown;

                // Set up the swap chain, buffers, and texture(s).
                InitializeGpuResources();

                // Create our planes.
                // Here's where we create the 2 planes for our rear wall and floor.  We set the texture size to texel units because that's how the video card expects 
                // them.  However, it's a little hard to eyeball 0.67798223f by looking at the texture image display, so we use the ToTexel function to determine our 
                // texel size.
                DX.Size2F textureSize = _texture.ToTexel(new DX.Size2(511, 511));

                // And here we set up the planes with a material, and initial positioning.
                _planes = new[]
                          {
                              new Plane(_graphics, _inputLayout, new Vector2(3.5f), new DX.RectangleF(0, 0, textureSize.Width, textureSize.Height))
                              {
                                  Material = new Material
                                             {
                                                 Diffuse = GorgonColor.White, Texture = _texture
                                             },
                                  Position = new Vector3(0, 0, 3.0f)
                              },
                              new Plane(_graphics, _inputLayout, new Vector2(3.5f), new DX.RectangleF(0, 0, textureSize.Width, textureSize.Height))
                              {
                                  Material = new Material
                                             {
                                                 Diffuse = GorgonColor.White, Texture = _texture
                                             },
                                  Position = new Vector3(0, -3.5f, 3.5f),
                                  Rotation = new Vector3(90.0f, 0, 0)
                              }
                          };

                // Create our sphere.
                // Again, here we're using texels to align the texture coordinates to the other image packed into the texture (atlasing).  
                Vector2 textureOffset = _texture.ToTexel(new Vector2(516, 0));
                // This is to scale our texture coordinates because the actual image is much smaller (255x255) than the full texture (1024x512).
                textureSize = _texture.ToTexel(new DX.Size2(255, 255));
                // Give the sphere a place to live.
                _sphere = new Sphere(_graphics, _inputLayout, 1.0f, textureOffset, textureSize)
                {
                    Position = new Vector3(2.2f, 1.5f, 2.5f),
                    Material = new Material
                    {
                        Diffuse = GorgonColor.White,
                        Texture = _texture
                    }
                };

                // Initialize the states used to draw the objects.
                InitializeStates();

                // Initialize 2D rendering.
                _2D = new Gorgon2D(_graphics);

                // I know, there's a lot in here.  Thing is, if this were Direct 3D 11 code, it'd probably MUCH 
                // more code and that's even before creating our planes and sphere.
            }
            finally
            {
                GorgonExample.EndInit();
            }
        }

        /// <summary>
        /// Handles the KeyDown event of the _mainForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        /// <exception cref="NotSupportedException"></exception>
        private static void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!e.Alt) || (e.KeyCode != Keys.Enter))
            {
                return;
            }

            if (!_swap.IsWindowed)
            {
                _swap.ExitFullScreen();
            }
            else
            {
                // Get the output for the main window.
                var currentScreen = Screen.FromControl(_mainForm);
                IGorgonVideoOutputInfo output = _graphics.VideoAdapter.Outputs[currentScreen.DeviceName];

                _swap.EnterFullScreen(in _selectedVideoMode, output);
            }
        }

        /// <summary>
        /// Handles the BeforeResize event of the swap chain.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SwapChainResizingEventArgs" /> instance containing the event data.</param>
        private static void Swap_BeforeResized(object sender, SwapChainResizingEventArgs e) => _graphics.SetDepthStencil(null);

        /// <summary>
        /// Handles the Resized event of the _swap control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SwapChainResizedEventArgs" /> instance containing the event data.</param>
        /// <exception cref="NotSupportedException"></exception>
        private static void Swap_AfterResized(object sender, SwapChainResizedEventArgs e)
        {
            // This method allows us to restore projection matrix after the swap chain has been resized.  If we didn't do this, we'd have a weird looking (e.g. distorted)
            // image because the old projection matrix would be in place for the previous swap chain size.
            //
            // This is also the place to re-apply any custom viewports, or scissor rectangles.

            // Reset our projection matrix to match our new size.
            _camera.ViewDimensions = new DX.Size2F(e.Size.Width, e.Size.Height);
            BuildDepthBuffer(e.Size.Width, e.Size.Height);
            _graphics.SetDepthStencil(_depthBuffer);
        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
#if NET5_0_OR_GREATER
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Initialize();
                GorgonApplication.Run(_mainForm, Idle);
            }
            catch (Exception ex)
            {
                GorgonExample.HandleException(ex);
            }
            finally
            {
                GorgonExample.UnloadResources();

                _2D.Dispose();

                // Always call dispose so we can free the native memory allocated for the backing graphics API.
                _sphere?.Dispose();

                if (_planes is not null)
                {
                    foreach (Plane plane in _planes)
                    {
                        plane?.Dispose();
                    }
                }

                _texture?.Dispose();
                _wvpBuffer?.Dispose();
                _materialBuffer?.Dispose();
                _vertexShader?.Dispose();
                _pixelShader?.Dispose();
                _inputLayout?.Dispose();
                _swap?.Dispose();
                _graphics?.Dispose();
            }
        }
    }
}
