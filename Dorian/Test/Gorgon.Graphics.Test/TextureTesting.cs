using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using GorgonLibrary.Graphics.Test.Properties;
using GorgonLibrary.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GorgonLibrary.Math;
using GorgonLibrary.Diagnostics;

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

        [TestMethod]
        public void TestTextureCopy()
        {
            var shader = _framework.Graphics.Shaders.FromMemory<GorgonPixelShader>("PS",
                "TestPSGeneric",
                Resources.TextureShaders);

            var destTexture = _framework.Graphics.Textures.CreateTexture("Texture",
                new GorgonTexture2DSettings
                {
                    ArrayCount = 1,
                    Format = BufferFormat.R8G8B8A8_UIntNormal,
                    MipCount = 4,
                    Height = 64,
                    Width = 64,
                    Usage = BufferUsage.Default
                });

            var sourceTexture = _framework.Graphics.Textures.CreateTexture<GorgonTexture2D>("Source", Resources.Glass, new GorgonGDIOptions
            {
                MipCount = 4
            });

            _framework.CreateTestScene(Shaders, Shaders, true);
            _framework.Graphics.Shaders.PixelShader.Current = shader;
            _framework.Graphics.Shaders.PixelShader.Resources[1] = destTexture;

            destTexture.CopySubResource(sourceTexture);
            destTexture.CopySubResource(sourceTexture, new Rectangle(0, 0, 128, 128));
            destTexture.CopySubResource(sourceTexture, new Rectangle(0, 0, 128, 128), 4, 4);
            destTexture.CopySubResource(sourceTexture, new Rectangle(0, 0, 64, 64), 8, 8);
            destTexture.CopySubResource(sourceTexture, new Rectangle(0, 0, 64, 64), -4, -4);
            destTexture.CopySubResource(sourceTexture, new Rectangle(32, 32, 32, 32), 32, 32);
            destTexture.CopySubResource(sourceTexture, new Rectangle(16, 16, 16, 16), 0, 0);
            destTexture.CopySubResource(sourceTexture, new Rectangle(0, 0, 64, 64), 0, 1, 128);
            destTexture.CopySubResource(sourceTexture, new Rectangle(0, 0, 32, 32), 0, 2, 192);
            destTexture.CopySubResource(sourceTexture, new Rectangle(0, 0, 16, 16), 0, 3, 224);

            /*_framework.IdleFunc = () =>
            {
                return false;
            };*/

            Assert.IsTrue(_framework.Run() == DialogResult.Yes);
        }

        [TestMethod]
        public void TestTextureUpdate()
        {
            float diver = 1.0f;

            var shader = _framework.Graphics.Shaders.FromMemory<GorgonPixelShader>("PS",
                "TestPSUpdateSub",
                Resources.TextureShaders);

            var texture = _framework.Graphics.Textures.CreateTexture("Texture",
                new GorgonTexture2DSettings
                {
                    ArrayCount = 1,
                    Format = BufferFormat.R8G8B8A8_UIntNormal,
                    MipCount = 4,
                    Height = 256,
                    Width = 256,
                    Usage = BufferUsage.Default
                });

            var dynTexture = _framework.Graphics.Textures.CreateTexture("DynTexture",
                new GorgonTexture2DSettings
                {
                    ArrayCount = 1,
                    Format = BufferFormat.R8G8B8A8_UIntNormal,
                    MipCount = 1,
                    Height = 256,
                    Width = 256,
                    Usage = BufferUsage.Dynamic
                });

            var imageData = GorgonImageData.CreateFromGDIImage(Resources.Glass,
                ImageType.Image2D,
                new GorgonGDIOptions
                {
                    MipCount = 4
                });

            _framework.CreateTestScene(Shaders, Shaders, true);
            _framework.Graphics.Shaders.PixelShader.Current = shader;
            _framework.Graphics.Shaders.PixelShader.Resources[1] = texture;

            texture.UpdateSubResource(imageData.Buffers[0],
                new Rectangle
                {
                    Width = 128,
                    Height = 128,
                    X = 0,
                    Y = 0
                });

            texture.UpdateSubResource(imageData.Buffers[1],
                new Rectangle
                {
                    Width = 64,
                    Height = 64,
                    X = 128,
                    Y = 0
                });

	        GorgonTexture2D testIntFormat = _framework.Graphics.Textures.CreateTexture("asas",
	                                                                                   new GorgonTexture2DSettings
	                                                                                   {
																						   Format = BufferFormat.R8G8B8A8_Int,
																						   Usage = BufferUsage.Dynamic,
																						   Width = 64,
																						   Height = 64
	                                                                                   });

            _framework.IdleFunc = () =>
            {
                _framework.Graphics.Shaders.PixelShader.Resources[2] = null;

                using(var lockData = dynTexture.Lock(BufferLockFlags.Write | BufferLockFlags.Discard))
                {
                    for (int i = 0; i < 4096; ++i)
                    {
                        int y = GorgonRandom.RandomInt32(0, imageData.Buffers[0].Height);
                        int x = GorgonRandom.RandomInt32(0, imageData.Buffers[0].Width);

                        // 95417E

                        imageData.Buffers[0].Data.Position = (y * imageData.Buffers[0].PitchInformation.RowPitch)
                                                     + (x * 4);

                        lockData.Data.Position = (y * lockData.PitchInformation.RowPitch)
                                                 + (x * dynTexture.FormatInformation.SizeInBytes);

                        var color = new GorgonColor(imageData.Buffers[0].Data.ReadInt32());

                        color = new GorgonColor(color.Red / diver, color.Green / diver, color.Blue / diver);

                        lockData.Data.Write(color.ToARGB());
                        //lockData.Data.Write(0xFF00FF00);

                        /*lockData.Data.Write(Color.FromArgb(color.ToARGB()).R);
                    lockData.Data.Write(Color.FromArgb(color.ToARGB()).G);
                    lockData.Data.Write(Color.FromArgb(color.ToARGB()).B);
                    lockData.Data.Write(Color.FromArgb(color.ToARGB()).A);*/
                    }
                }

                diver += 0.5f * GorgonTiming.Delta;

                if (diver > 32.0f)
                {
                    diver = 1.0f;
                }

	            _framework.Graphics.Shaders.PixelShader.Resources[2] = dynTexture;

                return false;
            };

            

            Assert.IsTrue(_framework.Run() == DialogResult.Yes);
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
                texture = _framework.Graphics.Textures.CreateTexture("Test1D",
                                                 new GorgonTexture1DSettings
                                                 {
                                                     Width = 256,
                                                     ArrayCount = 1,
                                                     Format = BufferFormat.R8,
                                                     MipCount = 1,
                                                     AllowUnorderedAccessViews = false,
                                                     Usage = BufferUsage.Staging
                                                 });

                texture.GetShaderView(BufferFormat.R8_Int);
            }
            finally
            {
                if (texture != null)
                {
                    texture.Dispose();
                }
            }
        }

	    [TestMethod]
	    public void TestDynamicUnorderedView()
	    {
		    GorgonTexture2D texture = null;

		    try
		    {
				texture = _framework.Graphics.Textures.CreateTexture("Test2D",
												 new GorgonTexture2DSettings
												 {
													 Width = 256,
													 Height = 256,
													 ArrayCount = 1,
													 Format = BufferFormat.R8G8B8A8,
													 MipCount = 1,
													 AllowUnorderedAccessViews = true,
													 Usage = BufferUsage.Dynamic
												 });

			    texture.GetUnorderedAccessView(BufferFormat.R8G8B8A8_Int);
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
                texture = _framework.Graphics.Textures.CreateTexture("Test2D",
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

                texture.GetShaderView(BufferFormat.R8G8B8A8_Int);
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
                texture = _framework.Graphics.Textures.CreateTexture("Test3D",
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
                texture.GetShaderView(BufferFormat.R8G8B8A8_Int);
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
                        data.Buffers[0].Data.Position = ((GorgonRandom.RandomInt32(0, 256) * data.Buffers[0].PitchInformation.RowPitch)
                                                + GorgonRandom.RandomInt32(0, 256) * 4);
						data.Buffers[0].Data.Write((int)((GorgonRandom.RandomSingle() * 2.0f - 1.0f) * (Int32.MaxValue - 2)));
                    }

                    texture = _framework.Graphics.Textures.CreateTexture<GorgonTexture2D>("Test2D", data);
                }

                GorgonTextureShaderView view = texture.GetShaderView(BufferFormat.R8G8B8A8_UIntNormal);
                 
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
                _1D = _framework.Graphics.Textures.CreateTexture("Test1D",
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

                _2D = _framework.Graphics.Textures.CreateTexture("Test2D",
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

                _3D = _framework.Graphics.Textures.CreateTexture("Test3D",
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
	    public void TestLoadSingleFrameGIF()
	    {
			var shader = _framework.Graphics.Shaders.FromMemory<GorgonPixelShader>("PS",
				"TestPSGeneric",
				Resources.TextureShaders);

			var texture = _framework.Graphics.Textures.FromFile<GorgonTexture2D>("TestImage", @"d:\images\rain_test_single.gif", new GorgonCodecGIF());

			_framework.CreateTestScene(Shaders, Shaders, true);

			_framework.Graphics.Shaders.PixelShader.Current = shader;
			_framework.Graphics.Shaders.PixelShader.Resources[1] = texture;

			Assert.IsTrue(_framework.Run() == DialogResult.Yes);
	    }

		[TestMethod]
		public void TestUAVPSBinding()
		{
			int step = 0;
			var uavOutputPS = Encoding.UTF8.GetString(Resources.UAVTexture);
			var texture = _framework.Graphics.Textures.CreateTexture<GorgonTexture2D>("Test", Resources.Glass, new GorgonGDIOptions
				{
					AllowUnorderedAccess = true,
					Format = BufferFormat.R8G8B8A8_UIntNormal
				});
            /*var texture = _framework.Graphics.Textures.FromFile<GorgonTexture2D>("Test", @"..\..\..\..\Resources\D3D\Glass.png", new GorgonCodecPNG
                {
                    AllowUnorderedAccess = true,
                    Format = BufferFormat.R8G8B8A8_UIntNormal
                });*/

			var uav = texture.GetUnorderedAccessView(BufferFormat.R32_UInt);
			var uavShaderPS = _framework.Graphics.Shaders.CreateShader<GorgonPixelShader>("UAV", "TestUAV", uavOutputPS, null, true);
			
			_framework.CreateTestScene(uavOutputPS, uavOutputPS, true);

			var newRT = _framework.Graphics.Output.CreateRenderTarget("RT", new GorgonRenderTarget2DSettings
			{
				Width = _framework.Screen.Settings.Width,
				Height =_framework.Screen.Settings.Height,
				Format = BufferFormat.R8G8B8A8_UIntNormal,
			});

			_framework.Graphics.Output.SetRenderTarget(newRT);

			_framework.IdleFunc = () =>
				{
					switch (step)
					{
						case 0:
							_framework.Graphics.Output.SetUnorderedAccessViews(1, uav);
							_framework.Graphics.Shaders.PixelShader.Resources[0] = null;
							_framework.Graphics.Shaders.PixelShader.Current = uavShaderPS;
							break;
						/*case 1:
							_framework.Graphics.Output.SetUnorderedAccessViews(1, null);
							_framework.Graphics.Shaders.PixelShader.Current = _framework.PixelShader;
							_framework.Graphics.Shaders.PixelShader.Resources[0] = texture;
							break;*/
						case 1:
							_framework.Graphics.Shaders.PixelShader.Current = _framework.PixelShader;
							//_framework.Graphics.Output.SetRenderTarget(_framework.Screen, null, 1, null);
							_framework.Graphics.Rasterizer.SetViewport(new GorgonViewport(0, 0, _framework.Screen.Settings.Width, _framework.Screen.Settings.Height));
							_framework.Graphics.Output.SetRenderTarget(_framework.Screen);
							_framework.Graphics.Shaders.PixelShader.Resources[0] = texture;
							break;
					}

					step++;

					if (step > 2)
					{
						step = 3;
					}

					return false;
				};

			Assert.IsTrue(_framework.Run() == DialogResult.Yes);
		}
        #endregion
    }
}
