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
// Created: March 2, 2017 7:46:50 PM
// 
#endregion

using System;
using System.IO;
using Drawing = System.Drawing;
using System.Windows.Forms;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Example.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.UI;
using SharpDX.Direct3D;

namespace Gorgon.Graphics.Example
{
    /// <summary>
	/// This is an example based on the MiniTri example that will draw a single triangle with a texture.
	/// 
	/// Like the MiniTri example, we'll be drawing a single triangle, but instead of using a single color per vertex on the triangle, we'll be applying a texture to the triangle. 
	/// 
	/// To map the location within the texture to a point in the triangle, we'll change our vertex structure to use a Vector2 called UV. This is the UV coordinates for mapping the texture to the vertex. 
	/// This value is in Texel space, and has a range of 0.0f - 1.0f, where 0.0 is the top/left, and 1.0f is the right/bottom. Larger values than 1.0f will either be clamped (the default), wrapped, or 
	/// have a border color drawn.
	/// 
	/// To assign a texture, we modify our draw call to assign the texture's resource view to the PixelShaderResources on the draw call.
	/// 
	/// To properly sample the texture in the pixel shader, we'll also create a texture sampler which defines how the texture data should be read when processing in the pixel shader. This example uses 
	/// default sampling which uses a bilinear filter to smooth the texture when it is zoomed in or out. Like the texture resource view, we assign this sampler to the PixelShaderSamplers on the draw 
	/// call. 
	/// 
	/// One final note: The textures and samplers are assigned to slots. These slots must correspond to the slots declared in the pixel shader. So, for example, if we have declared a texture at slot 4 
	/// in out pixel shader, then slot 4 on the PixelShaderResources must contain the texture, likewise for samplers.
	/// </summary>
	static class Program
	{
		#region Variables.
		// Main application form.
		private static formMain _mainForm;                                      
		// The graphics interface for the application.
		private static GorgonGraphics _graphics;
		// Our primary swap chain.
		private static GorgonSwapChain _swap;
		// The shader used to draw our pixels when rendering.
		private static GorgonPixelShader _pixelShader;
		// The shader used to transform our vertices when rendering.
		private static GorgonVertexShader _vertexShader;
		// The layout defining how a single vertex is laid out in memory.
		private static GorgonInputLayout _inputLayout;
		// The vertex buffer that will hold our vertices.
		private static GorgonVertexBuffer _vertexBuffer;
		// The buffer used to send data over to our shaders when rendering.
		// In this case, we will be sending a projection matrix so we know to project the vertices from a 3D space into a 2D space.
		private static GorgonConstantBuffer _constantBuffer;
		// The texture to apply to the triangle.
		private static GorgonTexture _texture;
		// This defines the data to send to the GPU.  
		// A draw call tells the GPU what to draw, and what special states to apply when rendering. This will be submitted to our GorgonGraphics object so that the 
		// GPU can queue up the data for rendering.
		private static GorgonDrawCall _drawCall;
		#endregion

		#region Methods.
		/// <summary>
		/// Property to return the path to the resources for the example.
		/// </summary>
		/// <param name="resourceItem">The directory or file to use as a resource.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="resourceItem"/> was NULL (<i>Nothing</i> in VB.Net) or empty.</exception>
		private static string GetResourcePath(string resourceItem)
		{
			string path = Settings.Default.ResourceLocation;

			if (string.IsNullOrEmpty(resourceItem))
			{
				throw new ArgumentException(@"The resource was not specified.", nameof(resourceItem));
			}

			path = path.FormatDirectory(Path.DirectorySeparatorChar);

			// If this is a directory, then sanitize it as such.
			if (resourceItem.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				path += resourceItem.FormatDirectory(Path.DirectorySeparatorChar);
			}
			else
			{
				// Otherwise, format the file name.
				path += resourceItem.FormatPath(Path.DirectorySeparatorChar);
			}

			// Ensure that we have an absolute path.
			return Path.GetFullPath(path);
		}
		
		/// <summary>
		/// Function to handle idle time for the application.
		/// </summary>
		/// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
		private static bool Idle()
		{
			// This will clear the swap chain to the specified color.  
			_swap.RenderTargetView.Clear(Drawing.Color.CornflowerBlue);

			// Draw our triangle.
			_graphics.Submit(_drawCall);

			// Now we flip our buffers on the swap chain.  
			// We need to this or we won't see anything at all except the standard window background color. Clearly, we don't want that. 
			// This method will take the current frame back buffer and flip it to the front buffer (the window). If we had more than one swap chain tied to multiple 
			// windows, then we'd need to do this for every swap chain.
			_swap.Present(1);
			
			return true;
		}

		/// <summary>
		/// Function to create a new vertex and pixel shader for use when rendering our triangle.
		/// </summary>
		private static void CreateShaders()
		{
			// We compile the vertex shader program into byte code for use by the GPU.  When we do this we have to provide the shader source code, and the name of the function 
			// that represents the entry point for the vertex shader.
			_vertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics.VideoDevice, Resources.MiniTriShaders, "MiniTriVS");

			// Next, we'll compile the pixel shader.
			_pixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics.VideoDevice, Resources.MiniTriShaders, "MiniTriPS");
		}

		/// <summary>
		/// Function to create a new vertex buffer and fill it with the vertices that represent our triangle.
		/// </summary>
		private static void CreateVertexBuffer()
		{
			// Define the points that make up our triangle.
			// We'll push it back half a unit along the Z-Axis so that we can see it.
			var vertices = new[]
						   {
							   // Note that we're assigning the texture coordinates in pixel space. The ToTexel function on the texture will convert these into 
							   // texel space for us.
				               new MiniTriVertex(new DX.Vector3(0, 0.5f, 1.0f), _texture.ToTexel(new DX.Point(128, 3))),
				               new MiniTriVertex(new DX.Vector3(0.5f, -0.5f, 1.0f), _texture.ToTexel(new DX.Point(230, 252))),
				               new MiniTriVertex(new DX.Vector3(-0.5f, -0.5f, 1.0f), _texture.ToTexel(new DX.Point(23, 252))),
			               };

			// Create the vertex buffer.
			//
			// This will be responsible for sending vertex data to the GPU. The buffer size is specified in bytes, so we need to ensure it has enough room to hold all 
			// 3 vertices.
			_vertexBuffer = new GorgonVertexBuffer("MiniTri Vertex Buffer", _graphics, new GorgonVertexBufferInfo
			                                                                           {
				                                                                           Usage = D3D11.ResourceUsage.Default,
																						   SizeInBytes = MiniTriVertex.SizeInBytes * vertices.Length
			                                                                           });

			// Send the vertex data into the buffer.
			_vertexBuffer.Update(vertices);
		}

		/// <summary>
		/// Function to create a new constant buffer so we can upload data to the shaders on the GPU.
		/// </summary>
		private static void CreateConstantBuffer()
		{
			// Our projection matrix.

			// Build our projection matrix using a 65 degree field of view and an aspect ratio that matches our current window aspect ratio.
			// Note that we depth a depth range from 0.001f up to 1000.0f.  This provides a near and far plane for clipping.  
			// These clipping values must have the world transformed vertex data inside of it or else it will not render. Note that the near/far plane is not a 
			// linear range and Z accuracy can get worse the further from the near plane that you get (particularly with depth buffers).
			DX.Matrix.PerspectiveFovLH(65.0f.ToRadians(), _mainForm.ClientSize.Width / (float)_mainForm.ClientSize.Height, 0.125f, 1000f, out DX.Matrix projectionMatrix);

			// Create our constant buffer.
			//
			// The data we pass into here will apply the projection transformation to our vertex data so we can transform from 3D space into 2D space.
			_constantBuffer = new GorgonConstantBuffer("MiniTri WVP Constant Buffer", _graphics, new GorgonConstantBufferInfo
			                                                                                     {
				                                                                                     Usage = D3D11.ResourceUsage.Default,
																									 SizeInBytes = DX.Matrix.SizeInBytes
			                                                                                     });

			_constantBuffer.Update(ref projectionMatrix);
		}

		/// <summary>
		/// Function to load the texture to apply to the triangle.
		/// </summary>
		private static void LoadTexture()
		{
			// In order to load the texture, we must first load it as image data using a PNG codec.
			// This allows us to manipulate the image data in memory prior to uploading it to the GPU as a texture.
			IGorgonImageCodec pngCodec = new GorgonCodecPng();

			using (IGorgonImage image = pngCodec.LoadFromFile(GetResourcePath(@"Textures\MiniTri\Gorgon.MiniTri.png")))
			{
				// Upload the image data into the texture.
				_texture = image.ToTexture("Gorgon.MiniTri.Texture", _graphics);
			}
		}

		/// <summary>
		/// Function to initialize the application.
		/// </summary>
		private static void Initialize()
		{
			// Create our form and center on the primary monitor.
			_mainForm = new formMain();

			_mainForm.Location = new Drawing.Point(Screen.PrimaryScreen.WorkingArea.Width / 2 - _mainForm.Width / 2,
				Screen.PrimaryScreen.WorkingArea.Height / 2 - _mainForm.Height / 2);

			// First we create and enumerate the list of video devices installed in the computer.
			// We must do this in order to tell Gorgon which video device we intend to use. Note that this method may be quite slow (particularly when running DEBUG versions of 
			// Direct 3D). To counter this, this object and its Enumerate method are thread safe so this can be run in the background while keeping the main UI responsive.
			IGorgonVideoDeviceList devices = new GorgonVideoDeviceList();

			// If no suitable device was found (no Direct 3D 11.1 support) in the computer, this method will throw an exception. However, if it succeeds, then the devices object 
			// will be populated with the IGorgonVideoDeviceInfo for each video device in the system.
			//
			// Using this method, we could also enumerate the WARP software rasterizer, and/of the D3D Reference device (only if the DEBUG functionality provided by the Windows 
			// SDK is installed). These devices are typically used to determine if there's a driver error, and can be terribly slow to render (reference moreso than WARP). It is 
			// recommended that these only be used in diagnostic scenarios only.
			devices.Enumerate();

			// Now we create the main graphics interface with the first applicable video device.
			_graphics = new GorgonGraphics(devices[0]);

			// Check to ensure that we can support the format required for our swap chain.
			// If a video device can't support this format, then the odds are good it won't render anything. Since we're asking for a very common display format, this will 
			// succeed nearly 100% of the time (unless you've somehow gotten an ancient video device to work with Direct 3D 11.1). Regardless, it's good form to the check for a 
			// working display format prior to setting up the swap chain.
			//
			// This method is also used to determine if a format can be used for other objects (e.g. a texture, render target, etc...) Like the swap chain format, this is also a 
			// best practice to check if the object you're creating supports the desired format.
			if ((_graphics.VideoDevice.GetBufferFormatSupport(DXGI.Format.R8G8B8A8_UNorm) & D3D11.FormatSupport.Display) != D3D11.FormatSupport.Display)
			{
				// We should never see this unless you've performed some form of black magic.
				GorgonDialogs.ErrorBox(_mainForm, "We should not see this error.");
				return;
			}

			// Finally, create a swap chain to display our output.
			// In this case we're setting up our swap chain to bind with our main window, and we use its client size to determine the width/height of the swap chain back buffers.
			// This width/height does not need to be the same size as the window, but, except for some scenarios, that would produce undesirable image quality.
			_swap = new GorgonSwapChain("Main Swap Chain",
			                            _graphics,
			                            _mainForm,
			                            new GorgonSwapChainInfo
			                            {
				                            Format = DXGI.Format.R8G8B8A8_UNorm,
				                            Width = _mainForm.ClientSize.Width,
				                            Height = _mainForm.ClientSize.Height
			                            })
			        {
				        DoNotAutoResizeBackBuffer = true
			        };

			// Create the shaders used to render the triangle.
			// These shaders provide transformation and coloring for the output pixel data.
			CreateShaders();

			// Set up our input layout.
			//
			// We'll be using this to describe to Direct 3D how the elements of a vertex is laid out in memory. 
			// In order to provide synchronization between the layout on the CPU side and the GPU side, we have to pass the vertex shader because it will contain the vertex 
			// layout to match with our C# input layout.
			_inputLayout = GorgonInputLayout.CreateUsingType<MiniTriVertex>(_graphics.VideoDevice, _vertexShader);

			// Load our texture so that we can apply it to our triangle.
			//
			// We load this first so we can use some functionality present on the texture to calculate the texture space coordinates required to render with the texture.
			LoadTexture();

			// Set up the triangle vertices.
			CreateVertexBuffer();

			// Set up the constant buffer.
			//
			// This is used (but could be used for more) to transform the vertex data from 3D space into 2D space.
			CreateConstantBuffer();

		    // This defines where to send the pixel data when rendering. For now, this goes to our swap chain.
            _graphics.SetRenderTarget(_swap.RenderTargetView);

			// Create our draw call.
			//
			// This will pass all the necessary information to the GPU to render the triangle
			_drawCall = new GorgonDrawCall
			            {
				            // This defines what type of primitive data to render. 
				            // For this, and most other examples, this will be a list of individual triangles. However, this could also be a strip of joined triangles, 
				            //lines, points, etc...
				            PrimitiveTopology = PrimitiveTopology.TriangleList,
				            // Our triangle has 3 points, obviously.
				            VertexCount = 3,
				            // This will bind the vertex buffer to the GPU so it can be read from when rendering.
				            VertexBuffers = new GorgonVertexBufferBindings(_inputLayout)
				                            {
					                            [0] = new GorgonVertexBufferBinding(_vertexBuffer, MiniTriVertex.SizeInBytes)
				                            },
				            // Bind the constant buffer with our projection matrix to the GPU.
				            VertexShaderConstantBuffers =
				            {
					            [0] = _constantBuffer
				            },
				            // Bind the texture to this draw call.
				            // We put this texture in slot 0, and this must be synchronized with the 
				            // pixel shader. Hence why the declaration of the texture in the shader 
				            // has a t0 semantic.
				            PixelShaderResourceViews =
				            {
					            [0] = _texture.DefaultShaderResourceView
				            },
				            // Bind the texture sampler to this draw call.
				            PixelShaderSamplers =
				            {
					            // Like a texture resource, we have to assign this to the slot referenced in the 
					            // pixel shader.
					            [0] = GorgonSamplerState.Default
				            },

				            // Define the current state of the rendering pipeline.
				            // This will tell the GPU how to render the data. Here we assign the shaders we created, but we could assign things like 
				            // blending information, depth information, etc...
				            PipelineState = _graphics.GetPipelineState(new GorgonPipelineStateInfo
				                                               {
					                                               PixelShader = _pixelShader,
					                                               VertexShader = _vertexShader,
					                                               // For items facing away from us, don't render.
					                                               RasterState = GorgonRasterState.NoCulling
				                                               })
			            };
		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			try
			{
				// Initialize the application.
				Initialize();

				// Now begin running the application idle loop.
				GorgonApplication.Run(_mainForm, Idle);
			}
			catch (Exception ex)
			{
				ex.Catch(_ => GorgonDialogs.ErrorBox(null, _), GorgonApplication.Log);
			}
			finally
			{
				// Always clean up when you're done.
				// Since Gorgon uses Direct 3D 11.1, which allocate objects that use native memory and COM objects, we must be careful to dispose of any objects that implement 
				// IDisposable. Failure to do so can lead to warnings from the Direct 3D runtime when running in DEBUG mode.
				_texture?.Dispose();
				_constantBuffer?.Dispose();
				_vertexBuffer?.Dispose();
				_inputLayout?.Dispose();
				_vertexShader?.Dispose();
				_pixelShader?.Dispose();
				_swap?.Dispose();
				_graphics?.Dispose();
			}
		}
	}
}
