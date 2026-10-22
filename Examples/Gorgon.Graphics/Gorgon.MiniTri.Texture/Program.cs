
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
// Created: March 2, 2017 7:46:50 PM
// 

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;

namespace Gorgon.Examples;

/// <summary>
/// This is an example based on the MiniTri example that will draw a single triangle with a texture
/// 
/// Like the MiniTri example, we'll be drawing a single triangle, but instead of using a single color per vertex on the triangle, we'll be applying a texture to the triangle. 
/// 
/// To map the location within the texture to a point in the triangle, we'll change our vertex structure to use a Vector2 called UV. This is the UV coordinates for mapping the texture to the vertex. 
/// This value is in Texel space, and has a range of 0.0f - 1.0f, where 0.0 is the top/left, and 1.0f is the right/bottom. Larger values than 1.0f will either be clamped (the default), wrapped, or 
/// have a border color drawn
/// 
/// To assign a texture, we pass the texture view handle it to the render data.
/// 
/// To properly sample the texture in the pixel shader, we'll also create a texture sampler which defines how the texture data should be read when processing in the pixel shader. This example uses 
/// default sampling which uses a bilinear filter to smooth the texture when it is zoomed in or out. Like the texture resource view, we assign this sampler to the PixelShaderSamplers on the draw 
/// call. 
/// 
/// One final note: The textures and samplers are passed with rendering data due to Gorgon's bindless rendering system. This allows shaders to directly reference the texture and sampler memory 
/// from the shader itself.
/// </summary>
internal class Program
    : IDisposable
{
    /// <summary>
    /// Data to tell the GPU what to render, how to render it.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct RenderData
    {
        /// <summary>
        /// The world/view/projection matrix used to transform our triangle vertices.
        /// </summary>
        public Matrix4x4 WorldViewProjection;
        /// <summary>
        /// The handle to the vertex buffer containing the triangle geometry.
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

    // The graphics interface for the application.
    private readonly GorgonGraphics _graphics;
    // Our primary swap chain.
    private readonly GorgonSwapChain _swap;
    // The factory for pipeline state objects.
    private readonly GorgonGraphicsPsoFactory _psoFactory;
    // The vertex buffer holding the triangle vertices.
    private readonly GorgonStructuredBufferView _vertexBuffer;
    // The texture to apply to the triangle.
    private readonly IGorgonTextureView<GorgonTexture> _texture;
    // This defines the data to send to the GPU.  
    // A draw call tells the GPU what to draw, and what special states to apply when rendering. This will be submitted to our GorgonGraphics object so that the 
    // GPU can queue up the data for rendering.
    private readonly GorgonDrawCall _drawCall;
    // The data we need to send to the GPU on each frame.
    private RenderData _renderData;

    /// <summary>
    /// Property to return the window for the applicatin.
    /// </summary>
    public FormMain Window
    {
        get;
    }

    /// <summary>
    /// Function to handle idle time for the application.
    /// </summary>
    /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
    private bool Idle()
    {
        // To render anything, we need a command list to instruct the GPU on what needs to be drawn.
        // To retrieve one, we ask the graphics object for a command list.
        //
        // For those worried about garbage, each command list is pooled and reused as needed, there 
        // are objects for the GC to collect.
        GorgonCommandList list = _graphics.GetCommandList("MiniTri Render Commandlist")
                                          // First we tell the command list that we want the swap chain to present its back buffer to the display.
                                          // By adding it as a presenter, the command list will guarantee the swap chain will display its contents                                          
                                          .AddPresenter(_swap)
                                          // If you recall, we have a constant buffer with data used to render the scene. This is how we pass it 
                                          // to the constant buffer at slot 0. If we don't do this, nothing renders.
                                          .WriteConstant(0, _renderData)
                                          // Next we need to tell the GPU how big our view port and clipping regions are.
                                          // For this example, they're the same size as our window.
                                          .SetScissorRectangle(new GorgonRectangle(0, 0, _swap.Width, _swap.Height))
                                          .SetViewport(new GorgonViewport(0, 0, _swap.Width, _swap.Height))
                                          // Now, we will assign the swap chain as the target that will receive our rendering.
                                          // If we had a depth buffer, we could attach it here as well.
                                          .SetRenderTarget(_swap)
                                          // Finally, clear the swap chain to the colour of Cornflower Blue.
                                          .ClearRenderTarget(_swap, GorgonColors.CornFlowerBlue);

        // Our command list is now set up. And now we have to tell it what to draw.
        // Why is this not on the fluent interface. This command represents a capture of the state assigned thus far.
        // This is an effective end to the recording -for this draw call-. Subsequent changes and draw calls can and 
        // should be made on the same list.
        // e.g. 
        // list.Draw(_drawCall);
        // 
        // list.SetRenderTarget(myOtherTarget)
        //     .ClearRenderTarget(myOtherTarget, GorgonColors.Black)
        //     .Draw(otherDrawCall);
        //    
        list.Draw(_drawCall);

        // This is part of the example helper code that just blits Gorgon's logo in the lower left corner.
        GorgonExample.BlitLogo(list);

        // Finally, we tell the GPU to execute the commands recorded into the list by submitting the list:
        _graphics.Submit(list);


        return true;
    }

    /// <summary>
    /// Function to create a new vertex and pixel shader for use when rendering our triangle.
    /// </summary>
    /// <param name="graphics">The graphics interface to use.</param>
    /// <returns>A tuple containing the vertex, and pixel shaders.</returns>
    private (GorgonShader VertexShader, GorgonShader PixelShader) CreateShaders(GorgonGraphics graphics)
    {
        using GorgonShaderCompiler compiler = new(graphics);

        // We compile the vertex shader program into byte code for use by the GPU.  When we do this we have to provide the shader source code, and the name of the function 
        // that represents the entry point for the vertex shader.
        GorgonShader vertexShader = compiler.Compile(Resources.MiniTriShaders, "MiniTriVS", ShaderType.VertexShader);

        // Next, we'll compile the pixel shader.
        GorgonShader pixelShader = compiler.Compile(Resources.MiniTriShaders, "MiniTriPS", ShaderType.PixelShader);

        return (vertexShader, pixelShader);
    }

    /// <summary>
    /// Function to create a new vertex buffer and fill it with the vertices that represent our triangle.
    /// </summary>
    /// <param name="graphics">The graphics interface to use.</param>
    /// <param name="texture">The texture that will be applied to the geometry.</param>
    /// <returns>The structured vertex buffer view for the vertex data.</returns>
    private GorgonStructuredBufferView CreateVertexBuffer(GorgonGraphics graphics, GorgonTexture texture)
    {
        // Define the points that make up our triangle.
        // We'll push it back half a unit along the Z-Axis so that we can see it.
        MiniTriVertex[] vertices = [
                           new MiniTriVertex(new Vector4(0, 0.5f, 1.0f, 1.0f), texture.ToTexel(new GorgonPoint(128, 3))),
                           new MiniTriVertex(new Vector4(0.5f, -0.5f, 1.0f, 1.0f), texture.ToTexel(new GorgonPoint(230, 252))),
                           new MiniTriVertex(new Vector4(-0.5f, -0.5f, 1.0f, 1.0f), texture.ToTexel(new GorgonPoint(23, 252)))
                       ];

        // Create the vertex buffer.
        //
        // This will be responsible for sending vertex data to the GPU. The buffer size is specified in bytes, so we need to ensure it has enough room to hold all 
        // 3 vertices.
        GorgonStructuredBufferView vertexBuffer = GorgonStructuredBufferView.CreateStructuredBuffer<MiniTriVertex>(graphics, "MiniTri Vertex Buffer", vertices.Length);

        // Now we fill the vertex buffer with the vertex data.
        // We do this with an object known as a command list, which we use to instruct the GPU to copy data from system memory, up to the GPUs memory.
        // You'll see the list is fluent, so we can just write out our commands as we go.
        GorgonCommandList uploadList = graphics.GetCommandList("MiniTri Upload List")
                                               .CopyRange(vertices, vertexBuffer.Buffer);

        // Now tell the GPU to execute the commands.
        graphics.Submit(uploadList);

        // We need to wait for the GPU to finish the copy of the data before we continue. 
        // Failure to do so would result in a race condition where our data may not be ready for rendering, and the output may be undesirable.
        graphics.WaitForGpu();

        return vertexBuffer;
    }

    /// <summary>
    /// Function to dispose managed and unmanaged objects.
    /// </summary>
    /// <param name="disposing"><b>true</b> to dispose both managed and umanaged objects, <b>false</b> to only dispose unmanaged.</param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            GorgonExample.UnloadResources();

            _texture.Dispose();
            _vertexBuffer.Dispose();
            _psoFactory.Dispose();
            _swap.Dispose();
            _graphics.Dispose();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Program"/> class.
    /// </summary>
    /// <param name="graphicsFactory">The factory used to create graphics objects.</param>
    private Program(GorgonGraphicsFactory graphicsFactory)
    {
        // Create our form and center on the primary monitor.
        Window = GorgonExample.Initialize(ExampleConfig.Default.Resolution, "Gorgon MiniTri - Now with 100% more textures.");

        try
        {
            GorgonExample.ResourceBaseDirectory = new DirectoryInfo(ExampleConfig.Default.ResourceLocation);

            // First we enumerate the list of video devices installed in the computer.
            // We must do this in order to tell Gorgon which video device we intend to use. Note that this method may be quite slow (particularly when running DEBUG versions of 
            // Direct 3D). To counter this, this object and its Enumerate method are thread safe so this can be run in the background while keeping the main UI responsive.
            // Find out which devices we have installed in the system.

            // If no suitable device was found (no Direct 3D 12.0 tier 2 support) in the computer, this method will throw an exception. However, if it succeeds, then the devices object 
            // will be populated with the IGorgonVideoDeviceInfo for each video device in the system.
            //
            // Using this method, we could also enumerate the software rasterizer. These devices are typically used to determine if there's a driver error, and can be terribly slow to render 
            // It is recommended that these only be used in diagnostic scenarios only.
            IReadOnlyList<GorgonVideoAdapterInfo> deviceList = graphicsFactory.EnumerateAdapters();

            if (deviceList.Count == 0)
            {
                throw new NotSupportedException("There are no suitable video adapters available in the system. This example is unable to continue and will now exit.");
            }

            // Now we create the main graphics interface with the first applicable video device.
            _graphics = graphicsFactory.CreateGraphics(deviceList[0]);

            // Check to ensure that we can support the format required for our swap chain.
            // If a video device can't support this format, then the odds are good it won't render anything. Since we're asking for a very common display format, this will 
            // succeed nearly 100% of the time (unless you've somehow gotten an ancient video device to work with Direct 3D 11.1). Regardless, it's good form to the check for a 
            // working display format prior to setting up the swap chain.
            //
            // This method is also used to determine if a format can be used for other objects (e.g. a texture, render target, etc...) Like the swap chain format, this is also a 
            // best practice to check if the object you're creating supports the desired format.
            if (!_graphics.FormatSupport[BufferFormat.R8G8B8A8_UNorm].IsDisplayFormat)
            {
                // We should never see this unless you've performed some form of black magic.
                throw new Exception("We should not see this error.");
            }

            // Finally, create a swap chain to display our output.
            // In this case we're setting up our swap chain to bind with our main window, and we use its client size to determine the width/height of the swap chain back buffers.
            // This width/height does not need to be the same size as the window, but, except for some scenarios, that would produce undesirable image quality.
            _swap = new GorgonSwapChain(_graphics,
                                        "MiniTri Main Swap Chain",
                                        Window.Handle,
                                        new GorgonSwapChainInfo(Window.ClientSize.Width,
                                                                     Window.ClientSize.Height,
                                                                     BufferFormat.R8G8B8A8_UNorm));

            // Create the shaders used to render the triangle.
            // These shaders provide transformation and coloring for the output pixel data.
            (GorgonShader vertexShader, GorgonShader pixelShader) = CreateShaders(_graphics);

            // Load our texture so that we can apply it to our triangle.
            //
            // We load this first so we can use some functionality present on the texture to calculate the texture space coordinates required to render with the texture.
            GorgonCodecPng pngCodec = new();
            using IGorgonImage image = pngCodec.FromFile(Path.Combine(GorgonExample.GetResourcePath(@"Textures\MiniTri\").FullName, "Gorgon.MiniTri.png"));
            _texture = IGorgonTextureView<GorgonTexture>.CreateTexture(_graphics, "MiniTri Texture", image);

            // Set up the triangle vertices.
            _vertexBuffer = CreateVertexBuffer(_graphics, _texture.Texture);

            // To define the state for the rendering pipeline, a pipeline state object is required. These are handled by a special factory that caches and reuses these objects 
            // as a performance enhancement. 
            _psoFactory = new(_graphics);

            // We now need to create the state to apply when rendering.
            // To do this, we build a pipeline state using a pipeline state builder to define our raster state, depth state, blending states and which shaders will be used.
            GorgonGraphicsPsoBuilder psoBuilder = new(_graphics);
            psoBuilder.PixelShader(pixelShader)
                      .RasterizerState(GorgonRasterState.NoCulling)
                      .BlendState(GorgonBlendState.NoBlending)
                      // This one is required because the pipeline state object needs to know what type of value should be returned from the pixel shader to the
                      // current render target.
                      .OutputFormat(_swap.Format);

            // We now ask the pipeline factory to either build or retrieve an existing pipeline state object that contains our rendering state.
            // You'll note that the vertex shader is passed separately. This is because we must have, at minimum, a vertex shader for a PSO.
            GorgonGraphicsPso pso = _psoFactory.CreateOrGetPso("MiniTri PSO", vertexShader, psoBuilder);

            // Create our draw call.
            //
            // This will pass all the necessary information to the GPU to render the triangle. Namely the state of the graphics pipeline, and the number of vertices we are 
            // going to render.
            _drawCall = new GorgonDrawCall(3, pso);
            // We need to tell the system that we're using this vertex buffer, and where, and in what capacity.
            // This allows the GPU to configure itself for the rendering process. 
            _drawCall.AssignBuffer(new GorgonUsedBuffer(_vertexBuffer, ShaderStage.Vertex, BufferUsage.ReadOnly));

            // Now we can set up our rendering data to pass to the shader.
            // You'll see we're passing the structured view's handle directly.
            // This is because Gorgon uses a technique called bindless rendering (and vertex pulling), which allows the shader to extract vertex data directly from the
            // buffer. The handle is used to tell the shader where in memory that buffer resides. The same applies to the sampler, and the texture view.
            //
            // Normally, this data could be built dynamically in the Idle method, but for the purposes of this example, we can initialize it here.
            _renderData = new RenderData
            {
                WorldViewProjection = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded((65.0f).ToRadians(), (float)_swap.Width / _swap.Height, 0.125f, 1000f),
                VertexBufferHandle = _vertexBuffer.GetViewHandle(),
                SamplerHandle = GorgonSampler.Linear(_graphics).GetViewHandle(),
                TextureHandle = _texture.GetViewHandle()
            };

            GorgonExample.LoadResources(_graphics);

            GorgonExample.Loop.Run(Idle);
        }
        finally
        {
            GorgonExample.EndInit();
        }
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            // The first step is to create a factory that will create the GorgonGraphics object that is used to communicate with the GPU.
            // This also allows us to apply debug settings to determine if the underlying GPU API is behaving correctly.
            //
            // All applications must create this factory in order to access the graphics functionality.
#if DEBUG
            using GorgonGraphicsFactory factory = new(GorgonGraphicsDebugFlags.SynchronizedCommandQueueValidation
                | GorgonGraphicsDebugFlags.GpuBasedValidation
                | GorgonGraphicsDebugFlags.GpuBasedStateTrackingValidation
                | GorgonGraphicsDebugFlags.ObjectTracking, log: GorgonExample.Log);
#else
            using GorgonGraphicsFactory factory = new(log: GorgonExample.Log);
#endif     

            using Program program = new(factory);

            // Now begin running the application idle loop.
            Application.Run(program.Window);
        }
        catch (Exception ex)
        {
            GorgonExample.HandleException(ex);
        }
        finally
        {
            GorgonExample.ShutDown();
        }
    }
}
