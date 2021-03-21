#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: June 3, 2018 11:04:26 AM
// 
#endregion

using System;
using System.Numerics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Timing;
using Gorgon.UI;
using DX = SharpDX;
using Gorgon.Renderers.Cameras;

namespace Gorgon.Examples
{
    /// <summary>
    /// This example shows how to use a geometry shader with the Gorgon core graphics library.
    ///
    /// A geometry shader is used to generate primitive data based on incoming primitive vertices (e.g. a set of vertices for a triangle, a single point, etc...). Using this, 
    /// we can dynamically build geometry on the GPU.
    ///
    /// This example will take a single triangle, and build a fully 3D pyramid along with some animation in the geometry shader. It will also show how an application can build
    /// geometry without using an input layout, or vertex buffer.
    /// </summary>
    static class Program
    {
        #region Variables.
        // The main application form.
        private static FormMain _mainForm;
        // The graphics interface for the application.
        private static GorgonGraphics _graphics;
        // The application swap chain used to display our graphics.
        private static GorgonSwapChain _swap;
        // The camera for the view.
        private static GorgonPerspectiveCamera _camera;
        // The world matrix for moving our objects.
        private static Matrix4x4 _worldMatrix = Matrix4x4.Identity;
        // A vertex shader that will generate our data to pass on to the geometry shader.
        private static GorgonVertexShader _bufferless;
        // Our vertex shader used to send data to the render target.
        private static GorgonVertexShader _vertexShader;
        // Our pixel shader used to send data to the render target.
        private static GorgonPixelShader _pixelShader;
        // Our geometry shader used to generate primitives.
        private static GorgonGeometryShader _geometryShader;
        // The vertex shader constants.
        private static GorgonConstantBufferView _vsConstants;
        // The draw call used to draw the primitives.
        private static GorgonDrawCall _drawCall;
        // The draw call builder.
        private static GorgonDrawCallBuilder _drawCallBuilder;
        // The pipline state builder.
        private static GorgonPipelineStateBuilder _pipeStateBuilder;
        // The texture to apply to the primitives.
        private static GorgonTexture2DView _texture;
        // The depth/stencil view.
        private static GorgonDepthStencil2DView _depthStencil;
        // The angle of rotation.
        private static float _angle;
        // The height offset.
        private static float _heightOffset;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the world project matrix.
        /// </summary>
        private static void UpdatedWorldProjection()
        {
            // Update the animated offset for the center point.  We need to put this into a Vector4 because our data needs to be 
            // aligned to a 16 byte boundary for constant buffers.
            var offset = new Vector4(_heightOffset, 0, 0, 0);

            _worldMatrix = Matrix4x4.CreateRotationY(_angle.ToRadians());
            _worldMatrix.SetRow(3, new Vector4(0, 0, 2.0f, 1.0f));

            // We've put our world matrix and center point offset inside of the same buffer since they're both updated once per
            // frame.
            _vsConstants.Buffer.SetData(in _worldMatrix, 64, CopyMode.NoOverwrite);
            _vsConstants.Buffer.SetData(in offset, 128, CopyMode.NoOverwrite);
        }

        /// <summary>
        /// Handles the AfterSwapChainResized event of the Swap control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SwapChainResizedEventArgs"/> instance containing the event data.</param>
        private static void Swap_AfterSwapChainResized(object sender, SwapChainResizedEventArgs e)
        {
            // We need to recreate the depth/stencil here to match the updated size of the render target (the depth/stencil and render targets must be the same size).
            _depthStencil = GorgonDepthStencil2DView.CreateDepthStencil(_graphics,
                                                                        new GorgonTexture2DInfo
                                                                        {
                                                                            Format = BufferFormat.D24_UNorm_S8_UInt,
                                                                            Binding = TextureBinding.DepthStencil,
                                                                            Usage = ResourceUsage.Default,
                                                                            Width = _swap.Width,
                                                                            Height = _swap.Height
                                                                        });
            _graphics.SetDepthStencil(_depthStencil);

            // When we resize, the projection matrix will go out of date, so we need to update our constant buffer with an updated projection.
            _camera.ViewDimensions = e.Size.ToSize2F();            
            ref readonly Matrix4x4 projection = ref _camera.GetProjectionMatrix();            
            _vsConstants.Buffer.SetData(in projection);
        }

        /// <summary>
        /// Handles the BeforeSwapChainResized event of the Swap control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SwapChainResizingEventArgs"/> instance containing the event data.</param>
        private static void Swap_BeforeSwapChainResized(object sender, SwapChainResizingEventArgs e)
        {
            // Before we go about resizing the render target, we need to ensure the depth stencil is removed so that we don't end up with a depth/stencil still 
            // attached that's not matched to the size of the render target.
            _graphics.SetDepthStencil(null);
            _depthStencil?.Dispose();
        }

        /// <summary>
        /// Function to load the shaders for the application.
        /// </summary>
        private static void LoadShaders()
        {
            string shaderCode = Resources.Shaders;

            _bufferless = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics, shaderCode, "BufferlessVs", Debugger.IsAttached);
            _vertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics, shaderCode, "VsMain", Debugger.IsAttached);
            _pixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics, shaderCode, "PsMain", Debugger.IsAttached);
            _geometryShader = GorgonShaderFactory.Compile<GorgonGeometryShader>(_graphics, shaderCode, "GsMain", Debugger.IsAttached);
        }

        /// <summary>
        /// Function to handle idle time for the application.
        /// </summary>
        /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
        private static bool Idle()
        {
            // Rotate our pyramid.
            _angle += 45.0f * GorgonTiming.Delta;

            if (_angle > 360.0f)
            {
                _angle -= 360.0f;
            }


            // This will allow us to animate the center point of our pyramid.
            _heightOffset = _angle.ToRadians().FastSin().Abs();

            // Send the animated variables to their respective shaders.
            UpdatedWorldProjection();

            // Clear our render target.
            _swap.RenderTargetView.Clear(Color.CornflowerBlue);
            _depthStencil.ClearDepth(1.0f);

            _graphics.Submit(_drawCall);

            GorgonExample.BlitLogo(_graphics);

            // Send the contents of the swap chain buffers to the screen.
            _swap.Present(1);

            return true;
        }

        /// <summary>
        /// Function used to initialize the application.
        /// </summary>
        private static void Initialize()
        {
            GorgonExample.ResourceBaseDirectory = new DirectoryInfo(ExampleConfig.Default.ResourceLocation);

            // Build the form so we can actually show something.
            _mainForm = GorgonExample.Initialize(new DX.Size2(1280, 800), "Geometry Shaders");

            try
            {
                // Now we create and enumerate the list of video devices installed in the computer.
                // We must do this in order to tell Gorgon which video device we intend to use. Note that this method may be quite slow (particularly when running DEBUG versions of 
                // Direct 3D). To counter this, this object and its Enumerate method are thread safe so this can be run in the background while keeping the main UI responsive.
                //
                // If no suitable device was found (no Direct 3D 11.2 support) in the computer, this method will return an empty list. However, if it succeeds, then the devices list 
                // will be populated with an IGorgonVideoDeviceInfo for each suitable video device in the system.
                //
                // Using this method, we could also enumerate the WARP software rasterizer, and/of the D3D Reference device (only if the DEBUG functionality provided by the Windows 
                // SDK is installed). These devices are typically used to determine if there's a driver error, and can be terribly slow to render (reference moreso than WARP). It is 
                // recommended that these only be used in diagnostic scenarios only.
                IReadOnlyList<IGorgonVideoAdapterInfo> devices = GorgonGraphics.EnumerateAdapters(log: GorgonApplication.Log);

                if (devices.Count == 0)
                {
                    GorgonDialogs.ErrorBox(_mainForm, "This example requires a video adapter that supports Direct3D 11.2 or better.");
                    GorgonApplication.Quit();
                    return;
                }

                // Now we create the main graphics interface with the first applicable video device.
                _graphics = new GorgonGraphics(devices[0], log: GorgonApplication.Log);

                // Check to ensure that we can support the format required for our swap chain.
                // If a video device can't support this format, then the odds are good it won't render anything. Since we're asking for a very common display format, this will 
                // succeed nearly 100% of the time. Regardless, it's good form to the check for a working display format prior to setting up the swap chain.
                //
                // This is also used to determine if a format can be used for other objects (e.g. a texture, render target, etc...) And like the swap chain format, it is also best 
                // practice to check if the object you're creating supports the desired format.
                if (!_graphics.FormatSupport[BufferFormat.R8G8B8A8_UNorm].IsDisplayFormat)
                {
                    // We should never see this unless you've got some very esoteric hardware.
                    GorgonDialogs.ErrorBox(_mainForm, "We should not see this error.");
                    return;
                }

                // Finally, create a swap chain to display our output.
                // In this case we're setting up our swap chain to bind with our main window, and we use its client size to determine the width/height of the swap chain back buffers.
                // This width/height does not need to be the same size as the window, but, except for some scenarios, that would produce undesirable image quality.
                _swap = new GorgonSwapChain(_graphics,
                                            _mainForm,
                                            new GorgonSwapChainInfo(_mainForm.ClientSize.Width, _mainForm.ClientSize.Height, BufferFormat.R8G8B8A8_UNorm)
                                            {
                                                Name = "Main Swap Chain"
                                            });

                // Assign events so we can update our projection with our window size.
                _swap.SwapChainResizing += Swap_BeforeSwapChainResized;
                _swap.SwapChainResized += Swap_AfterSwapChainResized;

                // We'll need a depth buffer for this example, or else our pyramid will look weird when rotating as back faces will appear through front faces.
                // So, first we should check for support of a proper depth/stencil format.  That said, if we don't have this format, then we're likely not running hardware from the last decade or more.
                if (!_graphics.FormatSupport[BufferFormat.D24_UNorm_S8_UInt].IsDepthBufferFormat)
                {
                    GorgonDialogs.ErrorBox(_mainForm, "A 24 bit depth buffer is required for this example.");
                    return;
                }

                _depthStencil = GorgonDepthStencil2DView.CreateDepthStencil(_graphics,
                                                                            new GorgonTexture2DInfo
                                                                            {
                                                                                Format = BufferFormat.D24_UNorm_S8_UInt,
                                                                                Binding = TextureBinding.DepthStencil,
                                                                                Usage = ResourceUsage.Default,
                                                                                Width = _swap.Width,
                                                                                Height = _swap.Height
                                                                            });

                // Load the shaders from a file on disc.
                LoadShaders();

                // Load the texture.
                _texture = GorgonTexture2DView.FromFile(_graphics,
                                                        Path.Combine(GorgonExample.GetResourcePath(@"Textures\GeometryShader\").FullName, "GSTexture.png"),
                                                        new GorgonCodecPng());

                // Create our builders so we can compose a draw call and pipeline state.
                _drawCallBuilder = new GorgonDrawCallBuilder();
                _pipeStateBuilder = new GorgonPipelineStateBuilder(_graphics);

                // Set our swap chain as the active rendering target and the depth/stencil buffer.
                _graphics.SetRenderTarget(_swap.RenderTargetView, _depthStencil);

                // Create a constant buffer so we can adjust the positioning of the data.
                _camera = new GorgonPerspectiveCamera(_graphics, new DX.Size2F(_swap.Width, _swap.Height), 0.125f, 1000.0f)
                {
                    Fov = 65.0f
                };
                
                _vsConstants = GorgonConstantBufferView.CreateConstantBuffer(_graphics,
                                                                             new GorgonConstantBufferInfo((Unsafe.SizeOf<Matrix4x4>() * 2) + Unsafe.SizeOf<Vector4>())
                                                                             {
                                                                                 Name = "WorldProjection CBuffer"                                                                                 
                                                                             });
                _vsConstants.Buffer.SetData(in _camera.GetProjectionMatrix(), copyMode: CopyMode.Discard);
                _vsConstants.Buffer.SetData(in _worldMatrix, 64, CopyMode.NoOverwrite);

                // Create a draw call so we actually have something we can draw.
                _drawCall = _drawCallBuilder.VertexRange(0, 3)
                                            .PipelineState(_pipeStateBuilder.PixelShader(_pixelShader)
                                                                            .VertexShader(_bufferless)
                                                                            .GeometryShader(_geometryShader)
                                                                            .DepthStencilState(GorgonDepthStencilState.DepthEnabled))
                                            .ShaderResource(ShaderType.Pixel, _texture)
                                            .ConstantBuffer(ShaderType.Vertex, _vsConstants)
                                            .ConstantBuffer(ShaderType.Geometry, _vsConstants)
                                            .Build();

                GorgonExample.LoadResources(_graphics);
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
            try
            {
#if NET5_0_OR_GREATER
                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
#endif
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

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

                // Always clean up when you're done.
                // Since Gorgon uses Direct 3D 11.2, we must be careful to dispose of any objects that implement IDisposable. 
                // Failure to do so can lead to warnings from the Direct 3D runtime when running in DEBUG mode.
                _depthStencil?.Dispose();
                _texture?.Dispose();
                _geometryShader?.Dispose();
                _pixelShader?.Dispose();
                _vertexShader?.Dispose();
                _bufferless?.Dispose();
                _swap?.Dispose();
                _graphics?.Dispose();
            }
        }
        #endregion
    }
}
