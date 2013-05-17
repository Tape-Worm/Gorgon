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
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct SBStruct
		{
			public float x;
			public float y;
			public float z;
		}

		private GraphicsFramework _framework;
		private string _tbShaders;
		private string _sbShaders;

		[TestInitialize]
		public void Init()
		{
			_framework = new GraphicsFramework();
			_tbShaders = Encoding.UTF8.GetString(Resources.TypedBufferShaders);
			_sbShaders = Encoding.UTF8.GetString(Resources.StructuredBufferShaders);
		}

		[TestCleanup]
		public void CleanUp()
		{
			if (_framework != null)
			{
				_framework.Dispose();
			}
		}

		[TestMethod]
		public void BindStructBuffer()
		{
			_framework.CreateTestScene(_sbShaders, _sbShaders, false);

			using (var buffer = _framework.Graphics.Shaders.CreateStructuredBuffer(new GorgonStructuredBufferSettings()
				{
					AllowCPUWrite = false,
					ElementCount = 1,
					ElementSize = 48,
					IsOutput = false,
					StructuredBufferType = StructuredBufferType.Standard
				}))
			{
				var stream = new GorgonDataStream(48);

				try
				{
					_framework.Graphics.Shaders.VertexShader.Resources.SetShaderBuffer(0, buffer);
					
					_framework.MaxTimeout = 10000;

					_framework.IdleFunc = () =>
						{
							stream.Position = 0;

							for (int i = 0; i < 4; i++)
							{
								var rnd = new Vector3(GorgonRandom.RandomSingle() * GorgonTiming.Delta,
								                          GorgonRandom.RandomSingle() * GorgonTiming.Delta,
								                          (GorgonRandom.RandomSingle() * 5.0f - 2.5f) * GorgonTiming.Delta);
								var value = new SBStruct
									{
										x = rnd.X,
										y = rnd.Y,
										z = rnd.Z
									};

								stream.Write(value);
							}

							stream.Position = 0;

							buffer.Update(stream);
						}; 

					buffer.Update(stream);

					_framework.Run();
				}
				finally
				{
					stream.Dispose();
				}
			}
		}

		[TestMethod]
		public void BindTypedBuffer()
		{
			_framework.CreateTestScene(_tbShaders, _tbShaders, true);

			var values = new Vector4[256];

			for (int i = 0; i < values.Length; i++)
			{
				values[i] = new Vector4(GorgonRandom.RandomSingle(), GorgonRandom.RandomSingle(), GorgonRandom.RandomSingle(), 1.0f);
			}

			using(var buffer = _framework.Graphics.Shaders.CreateTypedBuffer<Vector4>(values, BufferFormat.R32G32B32A32_Float, false))
			{
				_framework.Graphics.Shaders.PixelShader.Resources.SetShaderBuffer(0, buffer);

				Assert.IsTrue(_framework.Run() == DialogResult.Yes);
			}
			
		}
	}
}
