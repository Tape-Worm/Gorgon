using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using GorgonLibrary.Graphics.Test.Properties;
using GorgonLibrary.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GorgonLibrary.Math;
using GorgonLibrary.Diagnostics;
using SlimMath;

namespace GorgonLibrary.Graphics.Test
{
    [TestClass]
    public class TextureTesting
    {
        #region Variables.
	    private GraphicsFramework _framework;
	    private string _shaders;
        #endregion

        #region Properties.
		/// <summary>
		/// Property to return the shaders for the test.
		/// </summary>
	    private string Shaders
	    {
		    get
		    {
			    if (string.IsNullOrWhiteSpace(_shaders))
			    {
				    _shaders = Encoding.UTF8.GetString(Resources.TextureShaders);
			    }

			    return _shaders;
		    }
	    }
        #endregion

        #region Methods.
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

        /// <summary>
        /// Test for failure on staging textures.
        /// </summary>
        [TestMethod]
        public void Test1DStagingFail()
        {
            GorgonTexture1D texture = null;

            try
            {
                texture = _framework.Graphics.Textures.CreateTexture<GorgonTexture1D>("Test1D",
                                                 new GorgonTexture1DSettings
                                                 {
                                                     Width = 256,
                                                     ArrayCount = 1,
                                                     Format = BufferFormat.R8,
                                                     MipCount = 1,
                                                     AllowUnorderedAccessViews = false,
                                                     Usage = BufferUsage.Staging
                                                 });

                texture.CreateShaderView(BufferFormat.R8_Int);
            }
            finally
            {
                if (texture != null)
                {
                    texture.Dispose();
                }
            }
        }

        /// <summary>
        /// Test for failure on staging textures.
        /// </summary>
        [TestMethod]
        public void Test2DStagingFail()
        {
            GorgonTexture2D texture = null;

            try
            {
                texture = _framework.Graphics.Textures.CreateTexture<GorgonTexture2D>("Test2D",
                                                 new GorgonTexture2DSettings
                                                 {
                                                     Width = 256,
                                                     Height = 256,
                                                     ArrayCount = 1,
                                                     Format = BufferFormat.R8G8B8A8,
                                                     MipCount = 1,
                                                     AllowUnorderedAccessViews = false,
                                                     Usage = BufferUsage.Staging
                                                 });

                texture.CreateShaderView(BufferFormat.R8G8B8A8_Int);
            }
            finally
            {
                if (texture != null)
                {
                    texture.Dispose();
                }
            }
        }

        /// <summary>
        /// Test for failure on staging textures.
        /// </summary>
        [TestMethod]
        public void Test3DStagingFail()
        {
            GorgonTexture3D texture = null;

            try
            {
                texture = _framework.Graphics.Textures.CreateTexture<GorgonTexture3D>("Test3D",
                                                 new GorgonTexture3DSettings
                                                 {
                                                     Width = 256,
                                                     Height = 256,
                                                     Depth = 64,
                                                     Format = BufferFormat.R8G8B8A8,
                                                     MipCount = 1,
                                                     AllowUnorderedAccessViews = false,
                                                     Usage = BufferUsage.Staging
                                                 });
                texture.CreateShaderView(BufferFormat.R8G8B8A8_Int);
            }
            finally
            {
                if (texture != null)
                {
                    texture.Dispose();
                }
            }
        }

        /// <summary>
        /// Test for failure on staging textures.
        /// </summary>
        [TestMethod]
        public void Test2ViewsSameShaderStage()
        {
            GorgonTexture2D texture = null;

			_framework.CreateTestScene(Shaders, Shaders, true);

            try
            {
                using(var data = new GorgonImageData(new GorgonTexture2DSettings
                    {
                        Width = 256,
                        Height = 256,
                        ArrayCount = 1,
                        Format = BufferFormat.R8G8B8A8,
                        MipCount = 1,
                        ShaderViewFormat = BufferFormat.R8G8B8A8_Int,
                        AllowUnorderedAccessViews = false,
                        Usage = BufferUsage.Default
                    }))
                {
                    for (int i = 0; i < 5000; i++)
                    {
                        data[0].Data.Position = ((GorgonRandom.RandomInt32(0, 256) * data[0].PitchInformation.RowPitch)
                                                + GorgonRandom.RandomInt32(0, 256) * 4);
						data[0].Data.Write((int)((GorgonRandom.RandomSingle() * 2.0f - 1.0f) * (Int32.MaxValue - 2)));
                    }

                    texture = _framework.Graphics.Textures.CreateTexture<GorgonTexture2D>("Test2D", data);
                }

                GorgonTextureShaderView view = texture.CreateShaderView(BufferFormat.R8G8B8A8_UIntNormal);
                 
                _framework.Graphics.Shaders.PixelShader.Resources[0] = texture;
                _framework.Graphics.Shaders.PixelShader.Resources[1] = view;

                Assert.IsTrue(_framework.Run() == DialogResult.Yes);
            }
            finally
            {
                if (texture != null)
                {
                    texture.Dispose();
                }
            }
        }

        /// <summary>
        /// Test automatic shader resource views.
        /// </summary>
        [TestMethod]
        public void TestAutoSRV()
        {
            GorgonTexture1D _1D = null;
            GorgonTexture2D _2D = null;
            GorgonTexture3D _3D = null;

            try
            {
                _1D = _framework.Graphics.Textures.CreateTexture<GorgonTexture1D>("Test1D",
                                                 new GorgonTexture1DSettings
                                                     {
                                                         Width = 256,
                                                         ArrayCount = 1,
                                                         Format = BufferFormat.R8_UIntNormal,
                                                         MipCount = 1,
                                                         ShaderViewFormat = BufferFormat.Unknown,
                                                         AllowUnorderedAccessViews = false,
                                                         Usage = BufferUsage.Default
                                                     });

                Assert.IsNotNull((GorgonShaderView)_1D);
				Assert.AreEqual(_1D.Settings.Format, ((GorgonShaderView)_1D).Format);

                _2D = _framework.Graphics.Textures.CreateTexture<GorgonTexture2D>("Test2D",
                                                 new GorgonTexture2DSettings
                                                 {
                                                     Width = 256,
                                                     Height = 256,
                                                     ArrayCount = 1,
                                                     Format = BufferFormat.R8G8B8A8_UIntNormal,
                                                     MipCount = 1,
                                                     ShaderViewFormat = BufferFormat.Unknown,
                                                     AllowUnorderedAccessViews = false,
                                                     Usage = BufferUsage.Default
                                                 });

				Assert.IsNotNull((GorgonShaderView)_2D);
                Assert.AreEqual(_2D.Settings.Format, ((GorgonShaderView)_2D).Format);

                _3D = _framework.Graphics.Textures.CreateTexture<GorgonTexture3D>("Test3D",
                                                 new GorgonTexture3DSettings
                                                 {
                                                     Width = 256,
                                                     Height = 256,
                                                     Depth =  64,
                                                     Format = BufferFormat.R8G8B8A8_UIntNormal,
                                                     MipCount = 1,
                                                     ShaderViewFormat = BufferFormat.Unknown,
                                                     AllowUnorderedAccessViews = false,
                                                     Usage = BufferUsage.Default
                                                 });

				Assert.IsNotNull((GorgonShaderView)_3D);
				Assert.AreEqual(_3D.Settings.Format, ((GorgonShaderView)_3D).Format);
            }
            finally
            {
                if (_1D != null)
                {
                    _1D.Dispose();
                }

                if (_2D != null)
                {
                    _2D.Dispose();
                }

                if (_3D != null)
                {
                    _3D.Dispose();
                }
            }
        }

		[TestMethod]
		public void TestUAVPSBinding()
		{
			bool firstStep = true;
			var uavOutputPS = Encoding.UTF8.GetString(Resources.UAVTexture);
			/*var texture = _framework.Graphics.Textures.Create2DTextureFromGDIImage("Test", Properties.Resources.Glass, new GorgonGDIOptions
				{
					AllowUnorderedAccess = true,
					//Format = BufferFormat.R8G8B8A8_UIntNormal,
					ViewFormat = BufferFormat.R8G8B8A8_UIntNormal
				});*/
            var texture = _framework.Graphics.Textures.FromFile<GorgonTexture2D>("Test", @"..\..\..\..\Resources\D3D\Glass.png", new GorgonCodecPNG
                {
                    AllowUnorderedAccess = true,
                    Format = BufferFormat.R8G8B8A8_UIntNormal
                });

			var uav = texture.CreateUnorderedAccessView(BufferFormat.R32_UInt);
			var uavShaderPS = _framework.Graphics.Shaders.CreateShader<GorgonPixelShader>("UAV", "TestUAV", uavOutputPS, true);

			var newRT = _framework.Graphics.Output.CreateRenderTarget2D("RT", new GorgonRenderTarget2DSettings
				{
					Width = 256,
					Height = 256,
					Format = BufferFormat.R8G8B8A8_UIntNormal
				});

			_framework.CreateTestScene(Shaders, uavOutputPS, true);
			_framework.Graphics.Output.SetRenderTarget(newRT);

			_framework.IdleFunc = () =>
				{
					if (!firstStep)
					{
						if (_framework.Graphics.Shaders.PixelShader.Resources.GetResource<GorgonTexture2D>(0) != texture)
						{
							//_framework.Graphics.Shaders.PixelShader.SetUnorderedAccessView(1, null);
							//_framework.Graphics.Shaders.PixelShader.SetUnorderedAccessViewTest(_framework.Screen, 1, null);
                            _framework.Graphics.Output.SetRenderTarget(_framework.Screen);
							_framework.Graphics.Shaders.PixelShader.Current = _framework.PixelShader;
							_framework.Graphics.Shaders.PixelShader.Resources[0] = texture;
							//firstStep = true;
						}
					}
					else
					{
						_framework.Graphics.Shaders.PixelShader.Resources[0] = null;
						_framework.Graphics.Shaders.PixelShader.Current = uavShaderPS;
						_framework.Graphics.Shaders.PixelShader.SetUnorderedAccessView(1, uav);
						firstStep = false;
					}
				};

			Assert.IsTrue(_framework.Run() == DialogResult.Yes);
		}
        #endregion
    }
}
