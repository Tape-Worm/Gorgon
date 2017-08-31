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
using Drawing = System.Drawing;
using System.Windows.Forms;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Example.Properties;
using Gorgon.Math;
using Gorgon.UI;
using SharpDX.Direct3D;

namespace Gorgon.Graphics.Example
{
	/// <summary>
	/// This is an example based on the MiniTri example that will draw a single triangle.
	/// 
	/// Here we will use shaders, a vertex buffer, an input layout and a constant buffer to define how to render a multicolored triangle to the window.
	/// 
	/// This example is a little more complex than the Initialization example, and requires more code to set everything up. But, compared to a Direct 
	/// 3D version of this code, it is still much more simple.
	/// 
	/// We render the triangle by submitting a draw call to the GPU via a GorgonDrawCall object that will contain everything the GPU needs to know in 
	/// order to render the triangle. Resources, such as the vertex buffer, and constant buffer are tied to the draw call and the pipeline state, 
	/// which contains things like culling information, pixel shader, and vertex shader will also be assigned to the draw call.
	/// 
	/// Gorgon is smart enough to know if a draw call has been submitted already and will only assign the resources and state once (unless the call is 
	/// changed). This provides a big performance boost. Of course, for a single triangle like the one in this example, this really doesn't matter.
	/// 
	/// The biggest advantage of using this method of submitting draw calls is that, unlike Direct 3D, the calls are stateless. This way we can avoid 
	/// things like state bleed where a previously forgotten state will interfere with a subsequent draw call. It does this by ensuring the draw call 
	/// has all of the information it needs (hence why a draw call has so many fields), and will set that state per call (of course, if a state has 
	/// not changed, it will not set it because there's no need).
	/// </summary>
	internal static class Program
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
		// This defines the data to send to the GPU.  
		// A draw call tells the GPU what to draw, and what special states to apply when rendering. This will be submitted to our GorgonGraphics object so that the 
		// GPU can queue up the data for rendering.
		private static GorgonDrawCall _drawCall;
		#endregion

		#region Methods.
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
			MiniTriVertex[] vertices = {
				               new MiniTriVertex(new DX.Vector3(0, 0.5f, 1.0f), new GorgonColor(1, 0, 0)),
				               new MiniTriVertex(new DX.Vector3(0.5f, -0.5f, 1.0f), new GorgonColor(0, 1, 0)),
				               new MiniTriVertex(new DX.Vector3(-0.5f, -0.5f, 1.0f), new GorgonColor(0, 0, 1))
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
				            // Define the current state of the rendering pipeline.
				            // This will tell the GPU how to render the data. Here we assign the shaders we created, but we could assign things like 
				            // blending information, depth information, etc...
				            PipelineState = _graphics.GetPipelineState(new GorgonPipelineStateInfo
				                                               {
					                                               PixelShader = _pixelShader,
					                                               VertexShader = _vertexShader,
					                                               // For items facing away from us, always render.
					                                               RasterState = GorgonRasterState.NoCulling
				                                               })
			            };
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
