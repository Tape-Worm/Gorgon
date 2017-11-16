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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Example.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.GdiPlus;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.Timing;
using Gorgon.UI;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

namespace Gorgon.Graphics.Example
{
	/// <summary>
	/// A vertex for our boinger objects.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct BoingerVertex
	{
		/// <summary>
		/// Property to return the size of the vertex, in bytes.
		/// </summary>
		public static int Size => DirectAccess.SizeOf<BoingerVertex>();

		/// <summary>
		/// Vertex position.
		/// </summary>
		[InputElement(0, "SV_POSITION")]
		public DX.Vector4 Position;
		/// <summary>
		/// Texture coordinate.
		/// </summary>
		[InputElement(1, "TEXCOORD")]
		public DX.Vector2 UV;

		/// <summary>
		/// Initializes a new instance of the <see cref="BoingerVertex" /> struct.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="uv">The texture coordinate.</param>
		public BoingerVertex(DX.Vector3 position, DX.Vector2 uv)
		{
			Position = new DX.Vector4(position, 1.0f);
			UV = uv;
		}
	}

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
	/// // TODO: This is not accurate anymore.
	/// You'll note that in the render loop, before we render the text, we call _2D.Begin2D().  This takes  a copy of the current 
	/// state and then overwrites that state with the 2D stuff and then we render the text.  Finally, we call _2D.End2D() and that 
	/// restores the previous state (the states are stored in a stack in a LIFO order) so the 3D renderer doesn't get all messed up.
	/// When mixing the renderers like this, it's crucial to ensure that state doesn't get clobbered or else things won't show up 
	/// properly.
	/// 
	/// This example is considered advanced, and a firm understanding of a graphics API like Direct 3D 11.1 is recommended.
	/// It's also very "low level", in that there's not a whole lot that's done for you by the API.  It's a very manual process to 
	/// get everything initialized and thus there's a lot of set up code.  This is unlike the 2D renderer, which takes very little
	/// effort to get up and running (technically, you barely have to touch the base graphics library to get the 2D renderer doing
	/// something useful).
	/// </summary>
	[SuppressMessage("ReSharper", "LocalizableElement")]
	internal static class Program
	{
		#region Variables.
		// Format for our depth/stencil buffer.
		private static BufferFormat _depthFormat;
		// Main application form.
		private static formMain _mainForm;
		// The graphics interface for the application.
		private static GorgonGraphics _graphics;
		// Our primary swap chain.
		private static GorgonSwapChain _swap;
		// Our primary vertex shader.
		private static GorgonVertexShader _vertexShader;
		// Our primary pixel shader.	
		private static GorgonPixelShader _pixelShader;
		// Input layout.
		private static GorgonInputLayout _inputLayout;
		// Our texture.    
		private static GorgonTexture _texture;
		// Our world/view/project matrix buffer.
		private static GorgonConstantBuffer _wvpBuffer;
        // Buffer holding our material.
	    private static GorgonConstantBuffer _materialBuffer;
		// TODO: 2D interface. 
        //private static Gorgon2D _2D;								
        // Our view matrix.
        private static DX.Matrix _viewMatrix = DX.Matrix.Identity;
		// Our projection matrix.
		private static DX.Matrix _projMatrix = DX.Matrix.Identity;
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
		// The current output.
		private static IGorgonVideoOutputInfo _output;
		// The texture sampler state to use.
		private static GorgonSamplerState _samplerState;
		// The default pipeline state.		
		private static GorgonPipelineState _pipelineState;
		// The draw call used to send our data to the GPU.
		private static GorgonDrawIndexedCall _drawCall;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the ball position.
		/// </summary>
		private static void UpdateBall()
		{
			DX.Vector3 position = _sphere.Position;

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

			if ((position.X < -2.3f) || (position.X > 2.3f))
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

			if ((position.Y > 2.0f) || (position.Y < -2.5f))
			{				
				_bounceV = !_bounceV;
				if (!_bounceV)
				{
					position.Y = -2.5f;
					_dropSpeed = 20f;
				}
			}

			_sphere.Rotation = new DX.Vector3(0, _rotate, -12.0f);
			_sphere.Position = position;

			_rotate += 90.0f * GorgonTiming.Delta * (_rotateSpeed.Sin() * 1.5f);
			_rotateSpeed += GorgonTiming.Delta / 1.25f;
		}

		/// <summary>
		/// Function to send model data to the GPU for rendering.
		/// </summary>
		/// <param name="model">The model containing the data to render.</param>
		/// <param name="currentState">The pipeline state to apply.</param>
		private static void RenderModel(Model model, GorgonPipelineState currentState)
		{

			// Send the transform for the model to the GPU so we can update its position and rotation.
			// We're using an "out" and "ref" here because a matrix is a large struct, which loses its performance after 16 bytes.
			// By making a reference to the struct, we can keep the performance high.
			model.GetWorldMatrix(out DX.Matrix worldMatrix);
			UpdateWVP(ref worldMatrix);

		    GorgonColor color = model.Material.Diffuse;
		    GorgonPointerAlias data = _materialBuffer.Lock(MapMode.WriteDiscard);
            data.Write(ref color);
            _materialBuffer.Unlock(ref data);
            
			// Set up the draw call to render this models Index and Vertex buffers along with the current pipeline state.
			_drawCall.IndexStart = 0;
			_drawCall.IndexCount = model.IndexBuffer.Info.IndexCount;
			_drawCall.IndexBuffer = model.IndexBuffer;
			_drawCall.VertexBuffers = model.VertexBufferBindings;
			_drawCall.PixelShaderResourceViews[0] = model.Material.Texture;
			_drawCall.PixelShaderSamplers[0] = model.Material.TextureSampler;
			_drawCall.PipelineState = currentState;

			// Finally, send the draw call to the GPU.
			_graphics.Submit(_drawCall);
		}

		// TODO: This is temporary until we get 2D rendering in place.
		// TODO: Its purpose right now is to ensure that updating the window caption doesn't hurt our framerate (which it really does unless we limit it).
		private static readonly IGorgonTimer _tempTimer = new GorgonTimerMultimedia();

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
            _swap.DepthStencilView.Clear(0.0f, 0);

			// Render the back and floor planes.
			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < _planes.Length; ++i)
			{
				RenderModel(_planes[i], _pipelineState);
			}

		    // Render the ball.
		    _sphere.Material.Diffuse = GorgonColor.White;
            RenderModel(_sphere, _pipelineState);
			
			// Remember the position and rotation so we can restore them later.
			DX.Vector3 spherePosition = _sphere.Position;
			DX.Vector3 sphereRotation = _sphere.Rotation;

			// Offset the position of the ball so we can fake a shadow under the ball.
			_sphere.Position = new DX.Vector3(spherePosition.X + 0.25f, spherePosition.Y - 0.125f, spherePosition.Z + 0.5f);
			// Scale on the z-axis so the ball "shadow" has no real depth, and on the x & y to make it look slightly bigger.
			_sphere.Scale = new DX.Vector3(1.155f, 1.155f, 0.001f);
			// Reset the rotation so we don't rotate our flattened ball "shadow" (it'd look real weird if it rotated).
			_sphere.Rotation = DX.Vector3.Zero;
            // Render as black with alpha of 0.5 to simulate a shadow.
            _sphere.Material.Diffuse = new GorgonColor(0, 0, 0, 0.5f);

			// Render the shadow.
			RenderModel(_sphere, _pipelineState);
			
			// Restore our original positioning so we can render the ball in the correct place on the next frame.
			_sphere.Position = spherePosition;
			// Reset scale on the z-axis so the ball so it'll be normal for the next frame.
			_sphere.Scale = DX.Vector3.One;
			// Reset the rotation so it'll be in the correct place on the next frame.
			_sphere.Rotation = sphereRotation;

			// TODO: This is temporary until we get 2D rendering in place.
			// TODO: This updates the caption of the window with the FPS every second.  We have to limit the rate of updates because it really hits our FPS hard.
			if (_tempTimer.Milliseconds > 999)
			{
				_mainForm.Text = $"FPS: {GorgonTiming.FPS:0.00}, DT: {GorgonTiming.Delta:0.000}";
				_tempTimer.Reset();
			}

			// Draw our text.
			// Use this to show how incredibly slow and terrible my 3D code is.

			// TODO: Disabled until we get 2D rendering up and running again.
			// Tell the 2D renderer to remember the current state of the 3D scene.
			/*_3DState = _2D.Begin2D();

			_2D.Drawing.FilledRectangle(new RectangleF(0, 0, _mainForm.ClientSize.Width - 1.0f, 38.0f), Color.FromArgb(128, 0, 0, 0));
			_2D.Drawing.DrawRectangle(new RectangleF(0, 0, _mainForm.ClientSize.Width, 38.0f), Color.White);
			_2D.Drawing.DrawString(Graphics.Fonts.DefaultFont, 
				"FPS: " + GorgonTiming.AverageFPS.ToString("0.0")
				+ "\nDelta: " + (GorgonTiming.AverageDelta * 1000.0f).ToString("0.0##") + " milliseconds", 
				new Vector2(3.0f, 0.0f), GorgonColor.White);
			// Draw our logo because I'm insecure.
			_2D.Drawing.Blit(Graphics.Textures.GorgonLogo,
					new RectangleF(_mainForm.ClientSize.Width - Graphics.Textures.GorgonLogo.Settings.Width,
						_mainForm.ClientSize.Height - Graphics.Textures.GorgonLogo.Settings.Height,
						Graphics.Textures.GorgonLogo.Settings.Width,
						Graphics.Textures.GorgonLogo.Settings.Height));

			// Note that we're rendering here but not flipping the buffer (the 'false' parameter).  This just delays the page
			// flipping until later.  Technically, we don't need to do this here because it's the last thing we're doing, but
			// if we had more rendering to do after, we'd have to flip manually.
			_2D.Flush();

			// Restore the 3D scene state.
			_2D.End2D(_3DState);*/

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
		private static void UpdateWVP(ref DX.Matrix world)
		{

			// Build our world/view/projection matrix to send to
			// the shader.
			DX.Matrix.Multiply(ref world, ref _viewMatrix, out DX.Matrix temp);
			DX.Matrix.Multiply(ref temp, ref _projMatrix, out DX.Matrix wvp);

			// Direct 3D 11 requires that we transpose our matrix 
			// before sending it to the shader.
			DX.Matrix.Transpose(ref wvp, out wvp);

			// Update the constant buffer.
		    GorgonPointerAlias data = _wvpBuffer.Lock(MapMode.WriteDiscard);
            data.Write(ref wvp);
            _wvpBuffer.Unlock(ref data);
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

			// Find out which devices we have installed in the system.
			IGorgonVideoDeviceList deviceList = new GorgonVideoDeviceList();
			deviceList.Enumerate();

			int selectedDeviceIndex = 0;
			IGorgonVideoAdapter selectedDevice = null;

			while (selectedDeviceIndex < deviceList.Count)
			{
				// Reset back to a 32 bit floating point depth.
				_depthFormat = BufferFormat.D32_Float;

				// Destroy the previous interface.
				graphics?.Dispose();

				// Create the main graphics interface.
				graphics = new GorgonGraphics(deviceList[selectedDeviceIndex++]);

				// Validate depth buffer for this device.
				// Odds are good that if this fails, you should probably invest in a better video card.  Preferably something created after 2005.
				BufferFormatSupport support = graphics.VideoDevice.GetBufferFormatSupport(_depthFormat);

				if ((support & BufferFormatSupport.DepthStencil) == BufferFormatSupport.DepthStencil)
				{
					selectedDevice = graphics.VideoDevice;
					break;
				}

				// Fall back to 32 bit depth-buffer with 8 bit stencil support.
				_depthFormat = BufferFormat.D32_Float_S8X24_UInt;
				support = graphics.VideoDevice.GetBufferFormatSupport(_depthFormat);

				if ((support & BufferFormatSupport.DepthStencil) != BufferFormatSupport.DepthStencil)
				{
					continue;
				}

			    // Fall back to 24 bit depth-buffer with 8 bit stencil support.
			    _depthFormat = BufferFormat.D24_UNorm_S8_UInt;
			    support = graphics.VideoDevice.GetBufferFormatSupport(_depthFormat);

			    if ((support & BufferFormatSupport.DepthStencil) != BufferFormatSupport.DepthStencil)
			    {
			        continue;
			    }

			    // Fall back to 16 bit depth-buffer with no stencil support.
			    _depthFormat = BufferFormat.D16_UNorm;
			    support = graphics.VideoDevice.GetBufferFormatSupport(_depthFormat);

			    if ((support & BufferFormatSupport.DepthStencil) != BufferFormatSupport.DepthStencil)
			    {
			        continue;
			    }

                selectedDevice = graphics.VideoDevice;
				break;
			}

			// If, somehow, we are on a device from the dark ages, then we can't continue.
			if (selectedDevice != null)
			{
				return graphics;
			}

			GorgonDialogs.ErrorBox(_mainForm, $"The selected video device ('{deviceList[0].Name}') does not support a 24 or 16 bit depth buffer.");
			return null;
		}

		/// <summary>
		/// Function to initialize the application.
		/// </summary>
		private static void Initialize()
		{
			// Create our form.
			_mainForm = new formMain
			            {
				            ClientSize = Settings.Default.Resolution
			            };

			// Add a keybinding to switch to full screen or windowed.
			_mainForm.KeyDown += _mainForm_KeyDown;

			_graphics = CreateGraphicsInterface();

			// If we couldn't create the graphics interface, then leave.
			if (_graphics == null)
			{
				return;
			}
			
			// Create a 1280x800 window with a depth buffer.
			// We can modify the resolution in the config file for the application, but like other Gorgon examples, the default is 1280x800.
			_swap = new GorgonSwapChain("Main",
			                            _graphics,
			                            _mainForm,
			                            new GorgonSwapChainInfo
			                            {
				                            // Set up for 32 bit RGBA normalized display.
				                            Format = BufferFormat.R8G8B8A8_UNorm,
				                            Width = Settings.Default.Resolution.Width,
				                            Height = Settings.Default.Resolution.Height,
                                            DepthStencilFormat = _depthFormat
			                            });

			// Center on the primary monitor.
			// This is necessary because we already created the window, so it'll be off center at this point.
			_mainForm.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width / 2 - _mainForm.Width / 2, 
											Screen.PrimaryScreen.WorkingArea.Height / 2 - _mainForm.Height / 2);


			if (!Settings.Default.IsWindowed)
			{
				// If we've asked for full screen mode, then locate the correct video mode and set us up.
				_output = _graphics.VideoDevice.Info.Outputs[Screen.PrimaryScreen.DeviceName];
			    var mode = new GorgonVideoMode(Settings.Default.Resolution.Width, Settings.Default.Resolution.Height, BufferFormat.R8G8B8A8_UNorm);
				_graphics.VideoDevice.FindNearestVideoMode(_output, ref mode, out _selectedVideoMode);
					
				_swap.EnterFullScreen(ref mode, _output);
			}

			// Handle resizing because the projection matrix needs to be updated to reflect the new view size.
			_swap.AfterSwapChainResized += Swap_AfterResized;

            // Set the current render target output so we can see something.
		    _graphics.SetRenderTarget(_swap.RenderTargetView);

            // Initialize our draw call so we can render the objects.
            // All objects are using triangle lists, so we must tell the draw call that's what we need to render.
            _drawCall = new GorgonDrawIndexedCall
			            {
				            PrimitiveTopology = D3D.PrimitiveTopology.TriangleList
			            };

			// TODO: Create the 2D interface for our text.
			//_2D = Graphics.Output.Create2DRenderer(_swap);									

			// Create our shaders.
			// Our vertex shader.  This is a simple shader, it just processes a vertex by multiplying it against
			// the world/view/projection matrix and spits it back out.
			_vertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics.VideoDevice, Resources.Shader, "BoingerVS");
			// Our main pixel shader.  This is a very simple shader, it just reads a texture and spits it back out.  Has no
			// diffuse capability.
			_pixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics.VideoDevice, Resources.Shader, "BoingerPS");

            // Create the vertex input layout.
            // We need to create a layout for our vertex type because the shader won't know how to interpret the data we're sending it otherwise.  
            // This is why we need a vertex shader before we even create the layout.
			_inputLayout = GorgonInputLayout.CreateUsingType<BoingerVertex>(_graphics.VideoDevice, _vertexShader);

			// Resources are stored as System.Drawing.Bitmap files, so we need to convert into an IGorgonImage so we can upload it to a texture.
			// We also will generate mip-map levels for this image so that scaling the texture will look better. 
		    using (IGorgonImage image = Resources.Texture.ToGorgonImage()
		                                         .GenerateMipMaps(GorgonImage.CalculateMaxMipCount(Resources.Texture.Width, Resources.Texture.Height, 0)))
		    {
		        _texture = image.ToTexture("Texture", _graphics, ResourceUsage.Immutable);
		    }

		    // Create a sampler state for sampling our texture data.
		    _samplerState = new GorgonSamplerState(GorgonSamplerState.PointFiltering)
		                    {
		                        AddressU = D3D11.TextureAddressMode.Wrap,
		                        AddressV = D3D11.TextureAddressMode.Wrap
		                    };

			// Set up our view matrix.
			// Move the camera (view matrix) back 2.2 units.  This will give us enough room to see what's
			// going on.
			DX.Matrix.Translation(0, 0, 2.2f, out _viewMatrix);

			// Set up our projection matrix.
			// This matrix is probably the cause of almost EVERY problem you'll ever run into in 3D programming. Basically we're telling the renderer that we 
			// want to have a vertical FOV of 75 degrees, with the aspect ratio based on our form width and height.  The final values indicate how to 
			// distribute Z values across depth (tip: it's not linear).
			_projMatrix = DX.Matrix.PerspectiveFovLH((75.0f).ToRadians(), _mainForm.ClientSize.Width / (float)_mainForm.ClientSize.Height, 500.0f, 0.125f);

			// Create our constant buffer.			
			// Our constant buffers are how we send data to our shaders.  This one in particular will be responsible for sending our world/view/projection matrix 
			// to the vertex shader.  
			_wvpBuffer = new GorgonConstantBuffer("WVPBuffer", _graphics, new GorgonConstantBufferInfo
			                                                             {
				                                                             Usage = ResourceUsage.Dynamic,
																			 SizeInBytes = DX.Matrix.SizeInBytes
			                                                             });
            // This one will hold our material information.
            _materialBuffer = new GorgonConstantBuffer("MaterialBuffer", _graphics, new GorgonConstantBufferInfo
                                                                                    {
                                                                                        Usage = ResourceUsage.Dynamic,
                                                                                        SizeInBytes = DirectAccess.SizeOf<GorgonColor>()
                                                                                    });

			// Create our planes.
			// Here's where we create the 2 planes for our rear wall and floor.  We set the texture size to texel units because that's how the video card expects 
			// them.  However, it's a little hard to eyeball 0.67798223f by looking at the texture image display, so we use the ToTexel function to determine our 
			// texel size.
			DX.Size2F textureSize = _texture.ToTexel(new DX.Size2(511, 511));

			// And here we set up the planes with a material, and initial positioning.
		    _planes = new[]
		              {
		                  new Plane(_graphics, _inputLayout, new DX.Vector2(3.5f), new DX.RectangleF(0, 0, textureSize.Width, textureSize.Height))
		                  {
		                      Material = new Material
		                                 {
		                                     Diffuse = GorgonColor.White,
		                                     Texture = _texture.DefaultShaderResourceView,
		                                     TextureSampler = _samplerState
		                                 },
		                      Position = new DX.Vector3(0, 0, 3.0f)
		                  },
		                  new Plane(_graphics, _inputLayout, new DX.Vector2(3.5f), new DX.RectangleF(0, 0, textureSize.Width, textureSize.Height))
		                  {
		                      Material = new Material
		                                 {
		                                     Diffuse = GorgonColor.White,
		                                     Texture = _texture.DefaultShaderResourceView,
		                                     TextureSampler = _samplerState
		                                 },
		                      Position = new DX.Vector3(0, -3.5f, 3.5f),
		                      Rotation = new DX.Vector3(90.0f, 0, 0)
		                  }
		              };

			// Create our sphere.
			// Again, here we're using texels to align the texture coordinates to the other image packed into the texture (atlasing).  
			DX.Vector2 textureOffset = _texture.ToTexel(new DX.Vector2(516, 0));
			// This is to scale our texture coordinates because the actual image is much smaller (255x255) than the full texture (1024x512).
			textureSize = _texture.ToTexel(new DX.Size2(255, 255));
            // Give the sphere a place to live.
			_sphere = new Sphere(_graphics, _inputLayout, 1.0f, textureOffset, textureSize)
			          {
				          Position = new DX.Vector3(2.2f, 1.5f, 2.5f),
				          Material = new Material
				                     {
                                         Diffuse = GorgonColor.White,
					                     Texture = _texture.DefaultShaderResourceView,
					                     TextureSampler = _samplerState
				                     }
			          };


            // Add resources that are common throughout the application to the draw call.
            _drawCall.VertexShaderConstantBuffers[0] = _wvpBuffer;
		    _drawCall.PixelShaderConstantBuffers[0] = _materialBuffer;

            // Initialize a pipeline state so that the graphics can be rendered using the correct shaders, depth buffer, and blending.
			_pipelineState = _graphics.GetPipelineState(new GorgonPipelineStateInfo
			                                           {
				                                           DepthStencilState = GorgonDepthStencilState.DepthStencilEnabledGe,
				                                           PixelShader = _pixelShader,
				                                           VertexShader = _vertexShader
			                                           });

			// I know, there's a lot in here.  Thing is, if this were Direct 3D 11 code, it'd probably MUCH 
			// more code and that's even before creating our planes and sphere.
		}

		/// <summary>
		/// Handles the KeyDown event of the _mainForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		private static void _mainForm_KeyDown(object sender, KeyEventArgs e)
		{
			if ((!e.Alt) || (e.KeyCode != Keys.Enter) || (_output == null))
			{
				return;
			}

			if (!_swap.IsWindowed)
			{
				_swap.ExitFullScreen();
			}
			else
			{
				_swap.EnterFullScreen(ref _selectedVideoMode, _output);
			}
		}

		/// <summary>
		/// Handles the Resized event of the _swap control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		private static void Swap_AfterResized(object sender, EventArgs e)
		{
			// This method allows us to restore projection matrix after the swap chain has been resized.  If we didn't do this, we'd have a weird looking (e.g. distorted)
            // image because the old projection matrix would be in place for the previous swap chain size.
            //
            // This is also the place to re-apply any custom viewports, or scissor rectangles.

			// Reset our projection matrix to match our new size.
			_projMatrix = DX.Matrix.PerspectiveFovLH((75.0f).ToRadians(), _mainForm.ClientSize.Width / (float)_mainForm.ClientSize.Height, 0.125f, 500.0f);
        }
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			try
			{
				Initialize();
				GorgonApplication.Run(_mainForm, Idle);
			}
			catch (Exception ex)
			{
				ex.Catch(_ => GorgonDialogs.ErrorBox(null, _), GorgonApplication.Log);
			}
			finally
			{
				// Always call dispose so we can free the native memory allocated for the backing graphics API.
				_sphere?.Dispose();

				if (_planes != null)
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
