#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Thursday, May 16, 2013 9:23:14 PM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.IO;
using SlimMath;
using GorgonLibrary;
using GorgonLibrary.Math;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Test.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GorgonLibrary.Graphics.Test
{
	/// <summary>
	/// BufferTesting
	/// </summary>
	[TestClass]
	public class BufferTesting
	{
		private GraphicsFramework _framework;
	    private GorgonDataStream _bufferStream;
		private string _tbShaders;
		private string _sbShaders;
	    private string _rbShaders;

		[TestInitialize]
		public void Init()
		{
			_framework = new GraphicsFramework();
			_tbShaders = Encoding.UTF8.GetString(Resources.TypedBufferShaders);
			_sbShaders = Encoding.UTF8.GetString(Resources.StructuredBufferShaders);
            _rbShaders = Encoding.UTF8.GetString(Resources.RawBufferShaders);
		}

		[TestCleanup]
		public void CleanUp()
		{
            if (_bufferStream != null)
            {
                _bufferStream.Dispose();
            }

			if (_framework != null)
			{
				_framework.Dispose();
			}
		}

		[TestMethod]
		public void CreateStagingBuffer()
		{
			using(var gfx = new GorgonGraphics(DeviceFeatureLevel.SM4))
			{
				var buff = gfx.Buffers.CreateConstantBuffer("MyBuff", new GorgonConstantBufferSettings
					{
						SizeInBytes = 6144,
						Usage = BufferUsage.Default
					});

				var stage = buff.GetStagingBuffer();
			}
		}

		[TestMethod]
		public void BindStructBuffer()
		{
		    _framework.CreateTestScene(_sbShaders, _sbShaders, false);

			using (var buffer = _framework.Graphics.Buffers.CreateStructuredBuffer("SB", new GorgonStructuredBufferSettings
			    {
					SizeInBytes = 48,
					StructureSize = 12
				}))
			{
				_bufferStream = new GorgonDataStream(48);

				_framework.Graphics.Shaders.VertexShader.Resources[0] = buffer;
					
				_framework.MaxTimeout = 10000;

				_framework.IdleFunc = () =>
					{
						for (int i = 0; i < 4; i++)
						{
							var rnd = new Vector3(GorgonRandom.RandomSingle() * GorgonTiming.Delta,
								                        GorgonRandom.RandomSingle() * GorgonTiming.Delta,
								                        GorgonRandom.RandomSingle() * GorgonTiming.Delta);

                            _bufferStream.Write(rnd);
						}
                            
                        _bufferStream.Position = 0;
                        // ReSharper disable AccessToDisposedClosure
                        buffer.Update(_bufferStream);
                        // ReSharper restore AccessToDisposedClosure

						return false;
					}; 

				_framework.Run();
			}
		}

        [TestMethod]
        public void CreateGenericBuffer()
        {
            _framework.Graphics.Buffers.CreateBuffer("Test",
                                                        new GorgonBufferSettings
                                                            {
                                                                IsOutput = true,
                                                                SizeInBytes = 6144,
                                                                Usage = BufferUsage.Default
                                                            });
        }

        [TestMethod]
        public void CreateVertexBufferViews()
        {
            var vb = _framework.Graphics.Buffers.CreateVertexBuffer("VB", new GorgonBufferSettings()
            {
				AllowShaderViews = true,
                AllowUnorderedAccessViews = true,
                DefaultShaderViewFormat = BufferFormat.R8_Int,
                SizeInBytes = 48,
                Usage = BufferUsage.Default
            });

            var uav = vb.GetUnorderedAccessView(BufferFormat.R32G32B32A32_Float, 0, 3, false);
        }

		[TestMethod]
		public void CreateIndexBufferViews()
		{
			var ib = _framework.Graphics.Buffers.CreateIndexBuffer("IB", new GorgonIndexBufferSettings()
				{
					AllowUnorderedAccessViews = true,
					AllowShaderViews = true,
					SizeInBytes = 48,
					Usage = BufferUsage.Default
				});

			var uav = ib.GetUnorderedAccessView(BufferFormat.R32_UInt, 0, 8, false);
		}

        [TestMethod]
        public void CreateUnorderedView()
        {
            var structBuffer = _framework.Graphics.Buffers.CreateStructuredBuffer("SB", new GorgonStructuredBufferSettings()
                {
                    Usage = BufferUsage.Default,
                    AllowUnorderedAccessViews = true,
					SizeInBytes = 80,
					StructureSize = 16
                });

            structBuffer.GetUnorderedAccessView(0, 5, UnorderedAccessViewType.Standard);
            structBuffer.GetUnorderedAccessView(0, 5, UnorderedAccessViewType.AppendConsume);
            structBuffer.GetUnorderedAccessView(0, 5, UnorderedAccessViewType.Counter);
        }

        [TestMethod]
        public void BindRawBuffer()
        {
            _framework.CreateTestScene(_rbShaders, _rbShaders, false);

            var values = new byte[256 * 256 * 4];
	        float angle = 0.0f;
			
	        using(var buffer = _framework.Graphics.Buffers.CreateBuffer("RB", new GorgonBufferSettings
		        {
					AllowRawViews = true,
					AllowShaderViews = true,
					DefaultShaderViewFormat = BufferFormat.R32_UInt,
					SizeInBytes = values.Length
		        }))
	        {
		        _framework.Graphics.Shaders.PixelShader.Resources[0] = buffer;

			    _framework.IdleFunc = () =>
				    {
					    using(var stream = new GorgonDataStream(values))
					    {
						    float rads = (angle * GorgonRandom.RandomSingle(8.0f, 32.0f) + 16.0f).Radians();
						    float x = 128 + GorgonRandom.RandomInt32(0, 5) + (rads * rads.Cos());
							float y = 128 + GorgonRandom.RandomInt32(0, 5) + (rads * rads.Sin());

						    if (x > 255)
						    {
							    x = 255;
						    }

						    if (y > 255)
						    {
							    y = 255;
						    }

						    if (x < 0)
						    {
							    x = 0;
						    }

						    if (y < 0)
						    {
							    y = 0;
						    }
							
							int pos = (((int)y * 1024) + ((int)x * 4));

							for (int i = 0; i < pos - 50; i++)
							{
								values[i] = (byte)(values[i] * 0.95f);
							}

							values[pos] = 255;
							values[pos + 3] = values[pos + 2] = values[pos + 1] = (byte)GorgonRandom.RandomInt32(64, 128);

						    angle += GorgonRandom.RandomSingle(1, 180) * GorgonTiming.Delta;
						    if (angle > 360.0f)
						    {
							    angle = 0.0f;
						    }

						    buffer.Update(stream);

							return false;
					    }
				    };
				_framework.MaxTimeout = 15000;
				_framework.Run();
	        }
        }
    }
}
