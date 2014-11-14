using System.Text;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using GorgonLibrary.Graphics.Test.Properties;
using SlimMath;

namespace GorgonLibrary.Graphics.Test
{
	[TestClass]
	public class ShaderTests
	{
		private GraphicsFramework _framework;
		private string _shaders;

		/// <summary>
		/// Property to return the shaders for the test.
		/// </summary>
		private string BaseShaders
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_shaders))
				{
					_shaders = Encoding.UTF8.GetString(Resources.ShaderTests);
				}

				return _shaders;
			}
		}

		/// <summary>
		/// Test clean up code.
		/// </summary>
		[TestCleanup]
		public void CleanUp()
		{
			if (_framework == null)
			{
				return;
			}

			_framework.Dispose();
			_framework = null;
		}

		/// <summary>
		/// Test initialization code.
		/// </summary>
		[TestInitialize]
		public void Init()
		{
			_framework = new GraphicsFramework();
		}

        [TestMethod]
        public void TestComputeShader()
        {
            _framework.CreateTestScene(BaseShaders, BaseShaders, true, true);

            using(var compShader = _framework.Graphics.Shaders.CreateShader<GorgonComputeShader>("CompShader",
                                                                                               "TestCS",
                                                                                               BaseShaders))
            {
	            var cbuffer = _framework.Graphics.Buffers.CreateConstantBuffer("CBuffer", new GorgonConstantBufferSettings
		            {
			            SizeInBytes = 16
		            });
				var cbuffer2 = _framework.Graphics.Buffers.CreateConstantBuffer("CBuffer2", new GorgonConstantBufferSettings
				{
					SizeInBytes = 16
				});



                var view = ((GorgonTexture2D)_framework.Screen).GetUnorderedAccessView(BufferFormat.R8G8B8A8_UIntNormal);

	            _framework.Screen.AfterSwapChainResized += (sender, e) =>
		            {
			            view = ((GorgonTexture2D)_framework.Screen).GetUnorderedAccessView(BufferFormat.R8G8B8A8_UIntNormal);
						using(var data = new GorgonDataStream(16))
						{
							data.Write(new Vector2(e.Width, e.Height));
							data.Position = 0;
							cbuffer.Update(data);
						}
		            };

				var texture = _framework.Graphics.Textures.CreateTexture<GorgonTexture2D>("Test", Resources.Glass,
																						  new GorgonGDIOptions
																						  {
																							  AllowUnorderedAccess = true,
																							  Format =
																								  BufferFormat.R8G8B8A8_UIntNormal
																						  });
				var test = new Vector3(GorgonRandom.RandomSingle(0, 180.0f), GorgonRandom.RandomSingle(0, 180.0f), GorgonRandom.RandomSingle(0, 180.0f));
	            var speed = new Vector3(GorgonRandom.RandomSingle(5, 45), GorgonRandom.RandomSingle(5, 45), GorgonRandom.RandomSingle(5, 45));
	            var testDir = new Vector3(1);
		        _framework.IdleFunc = () =>
			        {
				        //_framework.Screen.Clear(GorgonColor.White);
				        _framework.Graphics.Output.SetRenderTarget(null);
				        _framework.Graphics.Shaders.PixelShader.Resources[0] = null;

				        _framework.Graphics.Shaders.ComputeShader.Current = compShader;
				        _framework.Graphics.Shaders.ComputeShader.ConstantBuffers[0] = cbuffer;
				        _framework.Graphics.Shaders.ComputeShader.ConstantBuffers[1] = cbuffer2;
				        _framework.Graphics.Shaders.ComputeShader.UnorderedAccessViews[0] = view;
				        _framework.Graphics.Shaders.ComputeShader.Dispatch((_framework.Screen.Settings.Width + 10) / 10,
				                                                            (_framework.Screen.Settings.Height + 10) / 10, 1);

				        _framework.Graphics.Shaders.ComputeShader.UnorderedAccessViews[0] = null;
				        _framework.Graphics.Output.SetRenderTarget(_framework.Screen);

				        _framework.Graphics.Shaders.PixelShader.Resources[0] = texture;

				        test += Vector3.Modulate(testDir * GorgonTiming.Delta, speed);
						
						/*if ((test >= 1.0f) || (test <= 0.0f))
						{
							testDir *= -1.0f;
						}*/

						if ((test.X > 359.9f) || (test.X <= 0.0f))
						{
							test.X = test.X - 359.9f * -testDir.X;
							speed.X = GorgonRandom.RandomSingle(5, 180.0f);
							testDir.X *= -1.0f;
						}

						if ((test.Y > 359.9f) || (test.Y <= 0.0f))
						{
							test.Y = test.Y - 359.9f * -testDir.X;
							speed.Y = GorgonRandom.RandomSingle(5, 180);
							testDir.Y *= -1.0f;
						}

						if ((test.Z > 359.9f) || (test.Z <= 0.0f))
						{
							test.Z = test.Z - 359.9f * -testDir.Y;
							speed.Z = GorgonRandom.RandomSingle(5, 180);
							testDir.Z *= -1.0f;
						}

				        var animVal = new Vector3(test.X.Radians().Cos(), test.Y.Radians().Sin(), test.Z.Radians().Cos() * test.Z.Radians().Cos());
						cbuffer2.Update(ref animVal);

						return false;
			        };

				Assert.IsTrue(_framework.Run() == DialogResult.Yes);
            }
        }

		[TestMethod]
		public void TestHSDSShaders()
		{
			float anim = 0;
			float tessFactor = 1.0f;

			_framework.CreateTestScene(BaseShaders, BaseShaders, true, true);
			_framework.Graphics.Input.PrimitiveType = PrimitiveType.PatchListWith4ControlPoints;
			_framework.Graphics.Rasterizer.States = GorgonRasterizerStates.WireFrame;

			using (var cbuffer = _framework.Graphics.Buffers.CreateConstantBuffer("cbuff", new GorgonConstantBufferSettings
				{
					SizeInBytes = 16
				}))
			using (var vsShader = _framework.Graphics.Shaders.CreateShader<GorgonVertexShader>("TessVS", "TestVSTess", BaseShaders))
			using (var hsShader = _framework.Graphics.Shaders.CreateShader<GorgonHullShader>("HullShader", "TestHS", BaseShaders))
			using (var dsShader = _framework.Graphics.Shaders.CreateShader<GorgonDomainShader>("DomShader",
																							   "TestDS",
																							   BaseShaders))
			{
				var texture = _framework.Graphics.Textures.CreateTexture<GorgonTexture2D>("Test", Resources.Glass, new GorgonGDIOptions());
				_framework.Graphics.Shaders.PixelShader.Resources[0] = texture;

				_framework.Graphics.Shaders.VertexShader.Current = vsShader;
				_framework.Graphics.Shaders.HullShader.Current = hsShader;
				_framework.Graphics.Shaders.DomainShader.Current = dsShader;
				_framework.Graphics.Shaders.DomainShader.ConstantBuffers[0] = cbuffer;
				_framework.Graphics.Shaders.HullShader.ConstantBuffers[0] = cbuffer;

				_framework.IdleFunc = () =>
					{
						anim += GorgonTiming.Delta;
						tessFactor += GorgonTiming.Delta * 64.0f;

						if (anim > 1.0f)
						{
							anim -= 1.0f;
						}

						if (tessFactor > 64.0f)
						{
							tessFactor = 1.0f;
						}
						
						Vector2 animVal = new Vector2(anim, tessFactor);
						cbuffer.Update(ref animVal);

						return false;
					};

				Assert.IsTrue(_framework.Run() == DialogResult.Yes);
			}
		}

		[TestMethod]
		public void TestGeometryShaderStreamOut()
		{
			_framework.CreateTestScene(BaseShaders, BaseShaders, true);

			using (var geoShader = _framework.Graphics.Shaders.CreateShader("GeoShader", "TestGS", BaseShaders, new GorgonStreamOutputElement[]
				{
					new GorgonStreamOutputElement(0, "SV_POSITION", 0, 0, 4, 0), 	
					new GorgonStreamOutputElement(0, "COLOR", 0, 0, 4, 0), 	
					new GorgonStreamOutputElement(0, "TEXCOORD", 0, 0, 2, 0), 
				}))
			{
				var vertexBuffer = _framework.Graphics.Buffers.CreateVertexBuffer("SOVB", new GorgonBufferSettings
					{
						SizeInBytes = 65536,
						IsOutput = true
					});
				var texture = _framework.Graphics.Textures.CreateTexture<GorgonTexture2D>("Test", Resources.Glass,
																						  new GorgonGDIOptions
																						  {
																							  AllowUnorderedAccess = true,
																							  Format = BufferFormat.R8G8B8A8_UIntNormal
																						  });

				_framework.Graphics.Shaders.GeometryShader.Current = geoShader;
				_framework.Graphics.Shaders.GeometryShader.SetStreamOutputBuffer(new GorgonOutputBufferBinding(vertexBuffer));

				_framework.Graphics.Output.DrawIndexed(0, 0, 6);

				_framework.Graphics.Shaders.GeometryShader.SetStreamOutputBuffer(GorgonOutputBufferBinding.Empty);
				_framework.Graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(vertexBuffer, 40);
				_framework.Graphics.Shaders.GeometryShader.Current = null;
				_framework.Graphics.Shaders.PixelShader.Resources[0] = texture;
				
				_framework.IdleFunc = () =>
					{
						_framework.Graphics.Output.DrawAuto();

						return true;
					};

				Assert.IsTrue(_framework.Run() == DialogResult.Yes);
			}
		}

		[TestMethod]
		public void TestGeometryShader()
		{
			_framework.CreateTestScene(BaseShaders, BaseShaders, true);

			using(var geoShader = _framework.Graphics.Shaders.CreateShader<GorgonGeometryShader>("GeoShader", "TestGS", BaseShaders,null,
				                                                                               true))
			{

				var texture = _framework.Graphics.Textures.CreateTexture<GorgonTexture2D>("Test", Resources.Glass,
				                                                                          new GorgonGDIOptions
					                                                                          {
						                                                                          AllowUnorderedAccess = true,
						                                                                          Format =
							                                                                          BufferFormat.R8G8B8A8_UIntNormal
					                                                                          });

				_framework.Graphics.Shaders.GeometryShader.Current = geoShader;
			    _framework.Graphics.Shaders.PixelShader.Resources[0] = texture;
				//_framework.Graphics.Rasterizer.States = GorgonRasterizerStates.WireFrame;

                Assert.IsTrue(_framework.Run() == DialogResult.Yes);
			}
		}
	}
}
