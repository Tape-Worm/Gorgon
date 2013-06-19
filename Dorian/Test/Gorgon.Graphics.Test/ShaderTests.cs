using System;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GorgonLibrary.IO;
using GorgonLibrary.Graphics.Test.Properties;

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
                _framework.Graphics.Output.SetRenderTarget(null);

                var texture = _framework.Graphics.Textures.CreateTexture("Test",
                                                                         new GorgonTexture2DSettings()
                                                                             {
                                                                                 AllowUnorderedAccessViews = true,
                                                                                 Format = BufferFormat.R8G8B8A8,
                                                                                 ShaderViewFormat = BufferFormat.R8G8B8A8_UIntNormal,
                                                                                 Width = 256,
                                                                                 Height = 256
                                                                             });

                var view = ((GorgonTexture2D)_framework.Screen).CreateUnorderedAccessView(BufferFormat.R8G8B8A8_UIntNormal);
                

                _framework.Graphics.Shaders.ComputeShader.Current = compShader;
                _framework.Graphics.Shaders.ComputeShader.UnorderedAccessViews[0] = view;
                _framework.Graphics.Shaders.ComputeShader.Dispatch(32, 32, 16);

                _framework.Graphics.Output.SetRenderTarget(_framework.Screen);

                _framework.Graphics.Shaders.ComputeShader.UnorderedAccessViews[0] = null;
                _framework.Graphics.Shaders.PixelShader.Resources[0] = texture;

                Assert.IsTrue(_framework.Run() == DialogResult.Yes);
            }
        }

		[TestMethod]
		public void TestGeometryShader()
		{
			_framework.CreateTestScene(BaseShaders, BaseShaders, true);

			using(var geoShader = _framework.Graphics.Shaders.CreateShader<GorgonGeometryShader>("GeoShader", "TestGS", BaseShaders,
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
