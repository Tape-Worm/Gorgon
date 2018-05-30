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
// Created: March 2, 2017 12:11:47 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DX = SharpDX;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.Timing;
using Gorgon.UI;
using SharpDX.Direct3D;

namespace Gorgon.Graphics.Example
{
	/// <summary>
	/// This is an example of using the core graphics API.  
	/// 
	/// It will show how to create and initialize the graphics API.  We do this by creating a GorgonGraphics object and a 
	/// GorgonSwapChain object that is used to display graphical data.
	/// 
	/// GorgonGraphics
	/// ===========================================================================================================================
	/// 
	/// This is the primary object used to send data to the GPU. This object can be used to define which video device can be used 
	/// for rendering, and multiple GorgonGraphics objects can allow an application to use multiple video devices at once time
	/// (note: This is not the same as multiple head outputs on a video device).
	/// 
	/// Graphics objects created with Gorgon must pass in an instance of the GorgonGraphics object for the specific video device 
	/// when creating them. This associates the data created by these objects to the video device used for rendering.
	/// 
	/// The GorgonGraphics object can also be used to force a specific feature level for a video device. This allows Gorgon to 
	/// be compatible with a wide range of video devices that don't support Direct3D 11.1 (at this point, this option is kind of 
	/// moot as the vast majority of video devices these days are more than capable of D3D 11.1). If the feature level is not 
	/// defined, then the maximum feature level supported by the device is used.
	/// 
	/// To initialize the GorgonGraphics object, an application needs to pass in an IGorgonVideoDeviceInfo object associated 
	/// with the desired video device. This will be retrieved from the IGorgonVideoDeviceList object which is merely a collection 
	/// of available video devices installed on the computer. 
	/// 
	/// Since this example does not focus on rendering data, this object is merely used to get things going. 
	/// 
	/// GorgonSwapChain
	/// ===========================================================================================================================
	/// 
	/// A swap chain allows us to send our graphics data to the screen.  We can create multiple swap chains and even make them all 
	/// full screen (provided they are different monitors).
	///
	/// A GorgonSwapChain requries an IGorgonSwapChainInfo object that will contain the swap chain settings required for initializing 
	/// a swap chain. Because a swap chain uses multiple buffers to present graphical data to the screen, a buffer size is required 
	/// and is typically set to the same size as the client area on the window that will be bound to the swap chain.  This buffer 
	/// size will resized when the window is resized, or can be resized manually through a method on the object. 
	/// 
	/// Swap chains can also enter/exit full screen exclusive mode, and in this example we use Alt+Enter to switch between full 
	/// screen and windowed mode. Gorgon also has functionality to set up a borderless full screen window (which is preferable to an 
	/// exclusive full screen mode).
	/// 
	/// For this example we're just going to assign a window (although this could be any type that inherits from 
	/// System.Windows.Forms.Control) to the swap chain via its constructor, and set its initial size to the client size of the 
	/// window. 
	/// 
	/// When setting up a swap chain, it is important to know which buffer format to use for the back buffers.  To this end, the 
	/// GorgonGraphics.VideoDevice has a method called GetBufferFormatSupport that will indicate which formats are valid for use as 
	/// a swap chain format. Well behaved applications should check with this method prior to setting up the swap chain.
	/// 
	/// Finally, to see something on the screen an application needs to call the Present method on the swap chain. This flips the 
	/// current backbuffer frame to the window and can have a presentation interval to lock down the presentation to the refresh 
	/// rate for the current video mode (this does not apply to windowed mode, and as such is not necessary for this application).
	/// </summary>
	internal static class Program
	{
		#region Variables.
		// Main application form.
		private static FormMain _mainForm;                                      
		// The graphics interface for the application.
		private static GorgonGraphics _graphics;
		// Our primary swap chain.
		private static GorgonSwapChain _swap;
		// The color to clear our swap chain with.
		private static GorgonColor _clearColor = new GorgonColor(0, 0, 0);
		// Which color channel are we animating? (R = 0, G = 1, B = 2).
		private static int _channel;
		// The value to apply to a specific color channel.
		private static float _channelValue;
		// The direction of the color channel animation. (1 = Incrementing values, -1 = decrementing values).
		private static int _direction = 1;
		// Indicates how to cycle through the available channels (1 = Incrementing from R -> G -> B, -1 = B -> G -> R).
		private static int _channelDirection = 1;
        // Defines which regions on the swap chain to clear.
        private static readonly DX.Rectangle[] _clearRegions = new DX.Rectangle[2];
        // Clearing pattern values (0 = full swap chain, 1 upper left/lower right only, 2 upper right, lower left only)
	    private static int _clearPattern;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to handle idle time for the application.
		/// </summary>
		/// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
		private static bool Idle()
		{
		    // This will clear the swap chain to the specified color.  
		    // For our example, we'll cycle through multiple colors so we don't end up with a boring old screen with a static color. This will also prove that our 
		    // swap chain is working and rendering data to the window.

		    // Set up the clear to clear the upper left and lower right of the swap chain:
		    switch (_clearPattern)
		    {
                case 1:
                    _clearRegions[0] = new DX.Rectangle(0, 0, _swap.Width / 2, _swap.Height / 2);
                    _clearRegions[1] = new DX.Rectangle(_swap.Width / 2, _swap.Height / 2, _swap.Width / 2, _swap.Height / 2);
                    _swap.RenderTargetView.Clear(_clearColor, _clearRegions);
                    break;
                case 2:
                    _clearRegions[0] = new DX.Rectangle(_swap.Width / 2, 0, _swap.Width / 2, _swap.Height / 2);
                    _clearRegions[1] = new DX.Rectangle(0, _swap.Height / 2, _swap.Width / 2, _swap.Height / 2);
                    _swap.RenderTargetView.Clear(_clearColor, _clearRegions);
                    break;
		        default:
		            _swap.RenderTargetView.Clear(_clearColor);
		            break;
		    }

			// This specifies how much color to apply to the channel.
			// We're using the GorgonTiming.Delta property here to retrieve the number of seconds that it takes to draw a single frame. This allows us to smooth 
			// the animation speed by basing it on the frame rate of the device. If we didn't do this, the colors would cycle way too quickly.
			_channelValue = GorgonTiming.Delta * _direction * 0.4f;

			switch (_channel)
			{
				case 0:
					_channelValue = _clearColor.Red + _channelValue;
					_clearColor = new GorgonColor(_channelValue, 0, 0);
					break;
				case 1:
					_channelValue = _clearColor.Green + _channelValue;
					_clearColor = new GorgonColor(0, _channelValue, 0);
					break;
				case 2:
					_channelValue = _clearColor.Blue + _channelValue;
					_clearColor = new GorgonColor(0, 0, _channelValue);
					break;
			}

			// If we've exceeded the min/max amount of color for the channel, move on to the next.
			if (_channelValue > 1.0f)
			{
				_direction = -1;
				_channelValue = 1.0f;
			}

			if (_channelValue < 0.0f)
			{
				_direction = 1;
				_channelValue = 0.0f;
				_channel += _channelDirection;
			}

			// Flip directions and set to the middle channel.
			if (_channel > 2)
			{
				_channel = 1;
				_channelDirection = -1;

			    ++_clearPattern;
			    if (_clearPattern > 2)
			    {
			        _clearPattern = 0;
			    }
			}

			if (_channel < 0)
			{
				_channel = 1;
				_channelDirection = 1;

			    ++_clearPattern;
			    if (_clearPattern > 2)
			    {
			        _clearPattern = 0;
			    }
			}

		    _mainForm.Text = $"FPS: {GorgonTiming.AverageFPS:#0.0} DT: {GorgonTiming.Delta:#0.000}ms";
            TestDrawing();

			// Now we flip our buffers on the swap chain.  
			// We need to this or we won't see anything at all except the standard window background color. Clearly, we don't want that. 
			// This method will take the current frame back buffer and flip it to the front buffer (the window). If we had more than one swap chain tied to multiple 
			// windows, then we'd need to do this for every swap chain.
            _swap.Present();
			
			return true;
		}

        #region Bad Stuff.
#warning Get rid of me.
	    /// <summary>
	    /// The vertex of the blitter used to blit textures to the current render target.
	    /// </summary>
	    [StructLayout(LayoutKind.Sequential, Pack = 1)]
	    internal struct BltVertex
	    {
	        /// <summary>
	        /// The size of the vertex, in bytes.
	        /// </summary>
	        public static readonly int Size = Unsafe.SizeOf<BltVertex>();

	        /// <summary>
	        /// The position of the vertex.
	        /// </summary>
	        [InputElement(0, "SV_POSITION")]
	        public DX.Vector4 Position;

	        /// <summary>
	        /// The texture coordinate for the vertex.
	        /// </summary>
	        [InputElement(1, "TEXCOORD")]
	        public DX.Vector2 Uv;

	        /// <summary>
	        /// The color of the vertex.
	        /// </summary>
	        [InputElement(2, "COLOR")]
	        public GorgonColor Color;
	    }

	    private static GorgonVertexShader _vShader;
	    private static GorgonVertexShader _passThru;
	    private static GorgonPixelShader _pShader;
	    private static GorgonPixelShader _pShader2;
	    private static GorgonGeometryShader _gsSo;
	    private static GorgonInputLayout _layout;
	    private static GorgonVertexBufferBinding _vbBinding;
	    private static GorgonVertexBufferBinding _vbBinding2;
	    private static GorgonStreamOutBinding _soBinding;
	    private static GorgonIndexBuffer _iBuffer;
	    private static GorgonConstantBufferView _cBuffer;
	    private static GorgonTexture2DView _texture;
	    private static GorgonTexture2DView _texture2;
	    private static GorgonDepthStencil2DView _depth;
	    private static GorgonDrawIndexCall _drawCall;
	    private static GorgonDrawIndexCall _drawCallSo;
	    private static GorgonDrawIndexCall _drawCall2;
	    private static GorgonStreamOutCall _soCall;
        private static DX.Matrix _projMatrix = DX.Matrix.Identity;
	    private static float _yRot;

	    private static void Transform(float xoffset)
	    {
            var position = new DX.Vector4(xoffset, 0, 1.0f, 1.0f);
                /*new DX.Vector4(GorgonRandom.RandomSingle(-1.0f, 1.0f), 
                                          GorgonRandom.RandomSingle(-1.0f, 1.0f), 
                                          GorgonRandom.RandomSingle(1.0f, 2.0f),
                                          1.0f);*/

	        DX.Matrix.RotationY(_yRot.ToRadians(), out DX.Matrix worldMatrix);
	        worldMatrix.Row4 = position;
	        DX.Matrix.Multiply(ref worldMatrix, ref _projMatrix, out DX.Matrix wProj);

	        _cBuffer.Buffer.SetData(ref wProj);
	        //_cBuffer.Buffer.SetData(ref wProj);
	    }
        
	    private static void TestDrawing()
	    {
            _depth.Clear(1.0f, 0);

	        Transform(0.5f);
	        _graphics.Submit(_drawCall2);
	        //for (int i = 0; i < 100000; ++i)
	        //{
	            Transform(0);
	            _graphics.Submit(_drawCall);
	        //}

	        // Send data into the output buffers.
	        _graphics.Submit(_drawCallSo);
            _graphics.SubmitStreamOut(_soCall);

	        _yRot += GorgonTiming.Delta * 45.0f;

	        if (_yRot > 360.0f)
	        {
	            _yRot = _yRot - 360.0f;
	        }
	    }

	    private static void TestInit()
	    {
            _depth = GorgonDepthStencil2DView.CreateDepthStencil(_graphics, new GorgonTexture2DInfo
                                                                            {
                                                                                Format = BufferFormat.D24_UNorm_S8_UInt,
                                                                                Binding = TextureBinding.DepthStencil,
                                                                                Usage = ResourceUsage.Default,
                                                                                Width = _swap.Width,
                                                                                Height = _swap.Height
                                                                            });
            _texture = GorgonTexture2DView.FromFile(_graphics, @"..\..\..\..\..\Resources\Textures\MiniTri\Gorgon.MiniTri.png", new GorgonCodecPng());
	        _texture2 = GorgonTexture2DView.FromFile(_graphics, @"..\..\..\..\..\Resources\Textures\GlassCube\Glass.png", new GorgonCodecPng());
	        using (StreamReader reader = new StreamReader(@"..\..\..\..\..\Gorgon\Gorgon.Graphics.Core\Resources\GraphicsShaders.hlsl"))
	        {
	            string shaderCode = reader.ReadToEnd();
	            _vShader = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics, shaderCode, "GorgonBltVertexShader", true);
	            _pShader = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics, shaderCode, "GorgonBltPixelShader", true);
	            _pShader2 = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics, shaderCode, "GorgonBltPixelShaderBwTest", true);
	            _passThru = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics, shaderCode, "PassThruVS", true);
	        }

	        DX.Matrix.PerspectiveFovLH((65.0f).ToRadians(), (float)_swap.Width / _swap.Height, 0.125f, 1000.0f, out _projMatrix);
            _cBuffer = GorgonConstantBufferView.CreateConstantBuffer(_graphics, new GorgonConstantBufferInfo
                                                                                {
                                                                                    SizeInBytes = DX.Matrix.SizeInBytes,
                                                                                    Usage = ResourceUsage.Dynamic
                                                                                }, 0, 1);
            _cBuffer.Buffer.SetData(ref _projMatrix);

	        _layout = GorgonInputLayout.CreateUsingType<BltVertex>(_graphics, _vShader);

	        using (var vertexData = new GorgonNativeBuffer<BltVertex>(3))
	        using (var indexData = new GorgonNativeBuffer<ushort>(3))
	        {
	            vertexData[0] = new BltVertex
	                            {
	                                Color = new GorgonColor(1.0f, 0, 0),
	                                Position = new DX.Vector4(0, 0.5f, 0.0f, 1.0f),
	                                Uv = _texture.Texture.ToTexel(new DX.Point(128, 3))
	                            };
	            vertexData[1] = new BltVertex
	                            {
	                                Color = new GorgonColor(0.0f, 1.0f, 0),
	                                Position = new DX.Vector4(0.5f, -0.5f, 0.0f, 1.0f),
	                                Uv = _texture.Texture.ToTexel(new DX.Point(230, 252))
	                            };
	            vertexData[2] = new BltVertex
	                            {
	                                Color = new GorgonColor(0.0f, 0, 1.0f),
	                                Position = new DX.Vector4(-0.5f, -0.5f, 0.0f, 1.0f),
	                                Uv = _texture.Texture.ToTexel(new DX.Point(23, 252))
	                            };
	            indexData[0] = 0;
	            indexData[1] = 1;
	            indexData[2] = 2;

	            _vbBinding = GorgonVertexBufferBinding.CreateVertexBuffer(_graphics,
	                                                                      new GorgonVertexBufferInfo
	                                                                      {
	                                                                          Usage = ResourceUsage.Default,
	                                                                          SizeInBytes = vertexData.SizeInBytes,
	                                                                          Binding = VertexIndexBufferBinding.None
	                                                                      },
	                                                                      vertexData);

	            _iBuffer = new GorgonIndexBuffer(_graphics,
	                                             new GorgonIndexBufferInfo
	                                             {
	                                                 Usage = ResourceUsage.Default,
	                                                 Binding = VertexIndexBufferBinding.None,
	                                                 Use16BitIndices = true,
	                                                 IndexCount = 3
	                                             }, indexData.Cast<byte>());
	        }

	        _vbBinding2 = GorgonVertexBufferBinding.CreateVertexBuffer<BltVertex>(_graphics,
	                                                                              new GorgonVertexBufferInfo("SO_Vtx_Buffer")
	                                                                              {
	                                                                                  SizeInBytes = 3 * Unsafe.SizeOf<BltVertex>(),
	                                                                                  Usage = ResourceUsage.Default,
	                                                                                  Binding = VertexIndexBufferBinding.StreamOut
	                                                                              });
            _soBinding = new GorgonStreamOutBinding(_vbBinding2.VertexBuffer);

	        using (var baseGsShader = GorgonShaderFactory.Compile<GorgonGeometryShader>(_graphics,
	                                                                                    @"
                                                                                        struct DS_OUTPUT
                                                                                        {
	                                                                                        float4 position : SV_Position;
                                                                                            float2 uv : TEXCOORD;
	                                                                                        float4 colour : COLOR;
                                                                                        };

                                                                                        [maxvertexcount(3)]
                                                                                        void GsMain( triangle DS_OUTPUT input[3], inout TriangleStream<DS_OUTPUT> TriangleOutputStream )
                                                                                        {
                                                                                            for (int i = 0; i < 3; ++i)
                                                                                            {
                                                                                                DS_OUTPUT newPoint = input[i];
                                                                                                newPoint.colour = float4(1, 1, 1, 0.5f);
                                                                                                
                                                                                                TriangleOutputStream.Append( newPoint );
                                                                                            }

                                                                                            TriangleOutputStream.RestartStrip();
                                                                                        }
                                                                                        ",
	                                                                                    "GsMain",
	                                                                                    true))
	        {
	            _gsSo = baseGsShader.ToStreamOut(_layout.ToStreamOutLayout());
	        }


	        _graphics.SetRenderTarget(_swap.RenderTargetView, _depth);

            var builder = new GorgonDrawIndexCallBuilder();
            var rsBuilder = new GorgonRasterStateBuilder();
	        var sampleBuilder = new GorgonSamplerStateBuilder(_graphics);
            var psoBuilder = new GorgonPipelineStateBuilder(_graphics);

	        _drawCall = builder.VertexBuffer(_layout, _vbBinding)
	                           .IndexBuffer(_iBuffer, 0, 3)
	                           .ConstantBuffer(ShaderType.Vertex, _cBuffer)
	                           .SamplerState(ShaderType.Pixel, sampleBuilder.Filter(SampleFilter.MinMagMipPoint))
	                           .ShaderResource(ShaderType.Pixel, _texture)
	                           .PipelineState(psoBuilder
	                                          .DepthStencilState(GorgonDepthStencilState.DepthEnabled)
	                                          .BlendState(GorgonBlendState.NoBlending)
	                                          .PrimitiveType(PrimitiveType.TriangleList)
	                                          .PixelShader(_pShader)
	                                          .VertexShader(_vShader)
	                                          .RasterState(GorgonRasterState.NoCulling))
	                           .Build();

	        _drawCall2 = builder.ShaderResource(ShaderType.Pixel, _texture2)
	                            .Build();

	        _drawCallSo = builder.PipelineState(psoBuilder.PixelShader(null)
	                                                      .GeometryShader(_gsSo)
	                                                      .RasterState(GorgonRasterState.Default))
	                             .ConstantBuffer(ShaderType.Vertex, null)
	                             .ShaderResource(ShaderType.Pixel, null)
	                             .StreamOutBuffer(in _soBinding)
	                             .Build();
            
	        var soCallBuilder = new GorgonStreamOutCallBuilder();
	        var soPsoBuilder = new GorgonStreamOutPipelineStateBuilder(_graphics);

	        _soCall = soCallBuilder.PipelineState(soPsoBuilder.BlendState(GorgonBlendState.Default)
	                                                          .DepthStencilState(GorgonDepthStencilState.DepthEnabled)
	                                                          .RasterState(GorgonRasterState.Default)
	                                                          .VertexShader(_vShader)
	                                                          .PixelShader(_pShader))
	                               .ConstantBuffer(ShaderType.Vertex, _cBuffer)
	                               .VertexBuffer(_layout, _vbBinding2)
	                               .SamplerState(GorgonSamplerState.Default)
	                               .ShaderResource(_texture2)
	                               .Build();
	    }
        #endregion

	    /// <summary>
	    /// Function to initialize the application.
	    /// </summary>
	    private static void Initialize()
	    {
	        // First, create our form.
	        _mainForm = new FormMain
	                    {
	                        ClientSize = new Size(640, 480)
	                    };
	        _mainForm.Show();

	        _mainForm.KeyUp += MainForm_KeyUp;

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
	            GorgonDialogs.ErrorBox(_mainForm, "This example requires a video adapter capable of feature set 12 or better.");
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
	                                    new GorgonSwapChainInfo("Main Swap Chain")
	                                    {
	                                        Format = BufferFormat.R8G8B8A8_UNorm,
	                                        Width = _mainForm.ClientSize.Width,
	                                        Height = _mainForm.ClientSize.Height
	                                    });

	        TestInit();
	    }

	    /// <summary>
        /// Handles the KeyUp event of the MainForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private static void MainForm_KeyUp(object sender, KeyEventArgs e)
	    {
	        if ((e.KeyCode != Keys.Enter)
	            || (!e.Alt)
	            || (_swap == null))
	        {
	            return;
	        }
            
            if (!_swap.IsWindowed)
	        {
                _swap.ExitFullScreen();
	            return;
	        }

	        IGorgonVideoOutputInfo output = _graphics.VideoAdapter.Outputs[_mainForm.Handle];

	        if (output == null)
	        {
	            return;
	        }

            // Find an appropriate video mode.
            var searchMode = new GorgonVideoMode(_mainForm.ClientSize.Width, _mainForm.ClientSize.Height, BufferFormat.R8G8B8A8_UNorm);
	        output.VideoModes.FindNearestVideoMode(output, in searchMode, out GorgonVideoMode nearestMode);
            // To enter full screen borderless window mode, call EnterFullScreen with no parameters.
            _swap.EnterFullScreen(in nearestMode, output);
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
			    _mainForm.KeyUp -= MainForm_KeyUp;

				// Always clean up when you're done.
				// Since Gorgon uses Direct 3D 11.4, we must be careful to dispose of any objects that implement IDisposable. 
			    // Failure to do so can lead to warnings from the Direct 3D runtime when running in DEBUG mode.
				_swap?.Dispose();
				_graphics?.Dispose();
			}
		}
	}
}
