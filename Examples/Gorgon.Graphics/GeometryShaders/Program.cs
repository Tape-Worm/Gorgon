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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Drawing = System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Core;
using DX = SharpDX;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Example.Properties;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Timing;
using Gorgon.UI;

namespace Gorgon.Graphics.Example
{
    /// <summary>
    /// TODO: A description of what we're showing.
    ///
    /// 
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
        // The projection matrix.
        private static DX.Matrix _projection = DX.Matrix.Identity;
        // The world matrix for moving our objects.
        private static DX.Matrix _worldMatrix = DX.Matrix.Identity;
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
        /// <param name="angle">The angle of rotation, in degrees.</param>
        private static void UpdatedWorldProjection()
        {
            DX.Vector4 offset = new DX.Vector4(_heightOffset, 0, 0, 0);
            DX.Matrix.RotationY(_angle.ToRadians(), out _worldMatrix);
            _worldMatrix.Row4 = new DX.Vector4(0, 0, 2.0f, 1.0f);
            _vsConstants.Buffer.SetData(ref _worldMatrix, 64, CopyMode.NoOverwrite);
            _vsConstants.Buffer.SetData(ref offset, 128, CopyMode.NoOverwrite);
        }

        /// <summary>
        /// Handles the AfterSwapChainResized event of the Swap control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="AfterSwapChainResizedEventArgs"/> instance containing the event data.</param>
        private static void Swap_AfterSwapChainResized(object sender, AfterSwapChainResizedEventArgs e)
        {
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

            DX.Matrix.PerspectiveFovLH((65.0f).ToRadians(), (float)_swap.Width / _swap.Height, 0.125f, 1000.0f, out _projection);
            _vsConstants.Buffer.SetData(ref _projection);
        }

        /// <summary>
        /// Handles the BeforeSwapChainResized event of the Swap control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="BeforeSwapChainResizedEventArgs"/> instance containing the event data.</param>
        private static void Swap_BeforeSwapChainResized(object sender, BeforeSwapChainResizedEventArgs e)
        {
            _graphics.SetDepthStencil(null);
            _depthStencil?.Dispose();
        }

        /// <summary>
        /// Function to load the shaders for the application.
        /// </summary>
        private static void LoadShaders()
        {
            string shaderPath = Path.Combine(GorgonApplication.StartupPath.FullName, "Shaders.hlsl");
            string shaderCode;

            using (var reader = new StreamReader(shaderPath, Encoding.ASCII))
            {
                shaderCode = reader.ReadToEnd();
            }

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
            _angle += 45.0f * GorgonTiming.Delta;

            if (_angle > 360.0f)
            {
                _angle -= 360.0f;
            }


            _heightOffset = _angle.ToRadians().FastSin().Abs();

            // Send the new angle into the shader.
            UpdatedWorldProjection();

            // Clear our render target.
            _swap.RenderTargetView.Clear(Drawing.Color.CornflowerBlue);
            _depthStencil.ClearDepth(1.0f);

            _graphics.Submit(_drawCall);

            // Send the contents of the swap chain buffers to the screen.
            _swap.Present(1);

            return true;
        }

        /// <summary>
        /// Function used to initialize the application.
        /// </summary>
        private static void Initialize()
        {
            Cursor.Current = Cursors.WaitCursor;

            // Build the form so we can actually show something.
            _mainForm = new FormMain
                        {
                            ClientSize = new Drawing.Size(1280, 800)
                        };

            // Now we create and enumerate the list of video devices installed in the computer.
            // We must do this in order to tell Gorgon which video device we intend to use. Note that this method may be quite slow (particularly when running DEBUG versions of 
            // Direct 3D). To counter this, this object and its Enumerate method are thread safe so this can be run in the background while keeping the main UI responsive.
            //
            // If no suitable device was found (no Direct 3D 11.4 support) in the computer, this method will return an empty list. However, if it succeeds, then the devices list 
            // will be populated with an IGorgonVideoDeviceInfo for each suitable video device in the system.
            //
            // Using this method, we could also enumerate the WARP software rasterizer, and/of the D3D Reference device (only if the DEBUG functionality provided by the Windows 
            // SDK is installed). These devices are typically used to determine if there's a driver error, and can be terribly slow to render (reference moreso than WARP). It is 
            // recommended that these only be used in diagnostic scenarios only.
            IReadOnlyList<IGorgonVideoAdapterInfo> devices = GorgonGraphics.EnumerateAdapters(log: GorgonApplication.Log);

            if (devices.Count == 0)
            {
                GorgonDialogs.ErrorBox(_mainForm, "This example requires a video adapter that supports Direct3D 11.4 or better.");
                GorgonApplication.Quit();
                return;
            }

            try
            {
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
                                            new GorgonSwapChainInfo("Main Swap Chain")
                                            {
                                                Format = BufferFormat.R8G8B8A8_UNorm,
                                                Width = _mainForm.ClientSize.Width,
                                                Height = _mainForm.ClientSize.Height
                                            });

                // Assign events so we can update our projection with our window size.
                _swap.BeforeSwapChainResized += Swap_BeforeSwapChainResized;
                _swap.AfterSwapChainResized += Swap_AfterSwapChainResized;

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
                _texture = GorgonTexture2DView.FromFile(_graphics, Path.Combine(Settings.Default.ResourceLocation, "GSTexture.png"), new GorgonCodecPng());

                // Create our builders so we can compose a draw call and pipeline state.
                _drawCallBuilder = new GorgonDrawCallBuilder();
                _pipeStateBuilder = new GorgonPipelineStateBuilder(_graphics);

                // Create a constant buffer so we can adjust the positioning of the data.
                DX.Matrix.PerspectiveFovLH((65.0f).ToRadians(), (float)_swap.Width / _swap.Height, 0.125f, 1000.0f, out _projection);
                _vsConstants = GorgonConstantBufferView.CreateConstantBuffer(_graphics, new GorgonConstantBufferInfo("WorldProjection CBuffer")
                                                                                        {
                                                                                            SizeInBytes = DX.Matrix.SizeInBytes * 2 + DX.Vector4.SizeInBytes
                                                                                        });
                _vsConstants.Buffer.SetData(ref _projection, copyMode: CopyMode.Discard);
                _vsConstants.Buffer.SetData(ref _worldMatrix, 64, CopyMode.NoOverwrite);

                // Create a draw call so we actually have something we can draw.
                _drawCall = _drawCallBuilder.VertexRange(0, 3)
                                            .PipelineState(_pipeStateBuilder.PixelShader(_pixelShader)
                                                                            .VertexShader(_bufferless)
                                                                            .GeometryShader(_geometryShader)
                                                                            .RasterState(GorgonRasterState.NoCulling)
                                                                            .DepthStencilState(GorgonDepthStencilState.DepthEnabled))
                                            .ShaderResource(ShaderType.Pixel, _texture)
                                            .ConstantBuffer(ShaderType.Vertex, _vsConstants)
                                            .ConstantBuffer(ShaderType.Geometry, _vsConstants)
                                            .Build();

                _graphics.SetRenderTarget(_swap.RenderTargetView, _depthStencil);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
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
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Initialize();

                GorgonApplication.Run(_mainForm, Idle);
            }
            catch (Exception ex)
            {
                ex.Catch(_ => GorgonDialogs.ErrorBox(null, _), GorgonApplication.Log);
            }
            finally
            {
                // Always clean up when you're done.
                // Since Gorgon uses Direct 3D 11.4, we must be careful to dispose of any objects that implement IDisposable. 
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
