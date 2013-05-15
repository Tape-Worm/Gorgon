using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GorgonLibrary.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SlimMath;
using GorgonLibrary.Math;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics.Test
{
    [TestClass]
    public class TextureTesting
    {
        #region Value Types.
        /// <summary>
        /// Vertex for testing.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Vertex
        {
            [InputElement(0, "POSITION")]
            public Vector4 Position;
            [InputElement(1, "COLOR")]
            public Vector4 Color;
            [InputElement(2, "TEXCOORD")]
            public Vector2 TexCoord;
        }
        #endregion

        #region Variables.
        private int _maxTime;
        private GorgonPixelShader _pixelShader;
        private GorgonVertexShader _vertexShader;
        private GorgonInputLayout _layout;
        private GorgonVertexBuffer _vertices;
        private GorgonIndexBuffer _indices;
        private GorgonSwapChain _screen;
        private GorgonGraphics _graphics;
        private TestForm _form;
        #endregion

        #region Properties.

        #endregion

        #region Methods.
        /// <summary>
        /// Test clean up code.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            if (_form != null)
            {
                _form.Dispose();
                _form = null;
            }

            if (_graphics != null)
            {
                _graphics.Dispose();
                _graphics = null;
            }
        }

        /// <summary>
        /// Function to create a test scene.
        /// </summary>
        private void CreateTestScene()
        {
            _layout = _graphics.Input.CreateInputLayout("Layout", typeof(Vertex), _vertexShader);

            _vertices = _graphics.Input.CreateVertexBuffer(BufferUsage.Immutable,
                                                           new[]
                                                               {
                                                                   new Vertex
                                                                       {
                                                                           Position = new Vector4(-0.5f, 0.5f, 0.5f, 1.0f),
                                                                           Color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                                                                           TexCoord = new Vector2(0, 0)
                                                                       },
                                                                    new Vertex
                                                                        {
                                                                           Position = new Vector4(0.5f, -0.5f, 0.5f, 1.0f),
                                                                           Color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                                                           TexCoord = new Vector2(1, 1)
                                                                        },
                                                                    new Vertex
                                                                        {
                                                                           Position = new Vector4(-0.5f, -0.5f, 0.5f, 1.0f),
                                                                           Color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                                                                           TexCoord = new Vector2(0, 1)
                                                                        },
                                                                    new Vertex
                                                                        {
                                                                           Position = new Vector4(0.5f, 0.5f, 0.5f, 1.0f),
                                                                           Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                                                                           TexCoord = new Vector2(1, 0)
                                                                        }
                                                               });

            _indices = _graphics.Input.CreateIndexBuffer(BufferUsage.Immutable,
                                                         true,
                                                         new int[]
                                                             {
                                                                 0, 1, 2, 3, 1, 0
                                                             });

            _graphics.Input.Layout = _layout;
            _graphics.Input.PrimitiveType = PrimitiveType.TriangleList;
            _graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_vertices, 40);
            _graphics.Input.IndexBuffer = _indices;
            _graphics.Rasterizer.SetViewport(new GorgonViewport(0, 0, 640, 480, 0, 1.0f));
            _graphics.Rasterizer.States = GorgonRasterizerStates.DefaultStates;
            _graphics.Shaders.VertexShader.Current = _vertexShader;
            _graphics.Shaders.PixelShader.Current = _pixelShader;
            _graphics.Shaders.PixelShader.TextureSamplers[0] = GorgonTextureSamplerStates.DefaultStates;

            _form.TopMost = true;

            _screen = _graphics.Output.CreateSwapChain("Screen",
                                                       new GorgonSwapChainSettings
                                                           {
                                                               Width = 640,
                                                               Height = 480,
                                                               Window = _form,
                                                               IsWindowed = true
                                                           });

            _graphics.Output.RenderTargets[0] = _screen;
        }

        private bool Idle()
        {
            _screen.Clear(GorgonColor.Black);

            _graphics.Output.DrawIndexed(0, 0, 6);

            _screen.Flip(1);

            if ((_maxTime > 0) && (GorgonTiming.MillisecondsSinceStart > _maxTime))
            {
                _form.Close();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Test initialization code.
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            _form = new TestForm();
            _graphics = new GorgonGraphics();

            _vertexShader = _graphics.Shaders.CreateShader<GorgonVertexShader>("VS",
                                                                               "TestVS",
                                                                               Properties.Resources.Shader,
                                                                               true);
            _pixelShader = _graphics.Shaders.CreateShader<GorgonPixelShader>("PS",
                                                                             "TestPS",
                                                                             Properties.Resources.Shader,
                                                                             true);
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
                texture = _graphics.Textures.CreateTexture<GorgonTexture1D>("Test1D",
                                                 new GorgonTexture1DSettings
                                                 {
                                                     Width = 256,
                                                     ArrayCount = 1,
                                                     Format = BufferFormat.R8,
                                                     MipCount = 1,
                                                     UnorderedAccessViewFormat = BufferFormat.Unknown,
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
                texture = _graphics.Textures.CreateTexture<GorgonTexture2D>("Test2D",
                                                 new GorgonTexture2DSettings
                                                 {
                                                     Width = 256,
                                                     Height = 256,
                                                     ArrayCount = 1,
                                                     Format = BufferFormat.R8G8B8A8,
                                                     MipCount = 1,
                                                     UnorderedAccessViewFormat = BufferFormat.Unknown,
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
                texture = _graphics.Textures.CreateTexture<GorgonTexture3D>("Test3D",
                                                 new GorgonTexture3DSettings
                                                 {
                                                     Width = 256,
                                                     Height = 256,
                                                     Depth = 64,
                                                     Format = BufferFormat.R8G8B8A8,
                                                     MipCount = 1,
                                                     UnorderedAccessViewFormat = BufferFormat.Unknown,
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

            CreateTestScene();

            try
            {
                using(var data = new GorgonImageData(new GorgonTexture2DSettings
                    {
                        Width = 256,
                        Height = 256,
                        ArrayCount = 1,
                        Format = BufferFormat.R8G8B8A8,
                        MipCount = 1,
                        ShaderViewFormat = BufferFormat.R8G8B8A8_UIntNormal,
                        UnorderedAccessViewFormat = BufferFormat.Unknown,
                        Usage = BufferUsage.Default
                    }))
                {
                    for (int i = 0; i < 5000; i++)
                    {
                        data[0].Data.Position = ((GorgonRandom.RandomInt32(0, 256) * data[0].PitchInformation.RowPitch)
                                                + GorgonRandom.RandomInt32(0, 256) * 4);
                        data[0].Data.Write(GorgonColor.White.ToRGBA());
                    }

                    texture = _graphics.Textures.CreateTexture<GorgonTexture2D>("Test2D", data);
                }

                GorgonTextureShaderView view = texture.CreateShaderView(BufferFormat.R8G8B8A8_Int);
                 
                _graphics.Shaders.PixelShader.Resources.SetTexture(0, texture);
                _graphics.Shaders.PixelShader.Resources.SetView(1, view);

                _maxTime = 10000;
                Gorgon.Run(_form, Idle);
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
                _1D = _graphics.Textures.CreateTexture<GorgonTexture1D>("Test1D",
                                                 new GorgonTexture1DSettings
                                                     {
                                                         Width = 256,
                                                         ArrayCount = 1,
                                                         Format = BufferFormat.R8_UIntNormal,
                                                         MipCount = 1,
                                                         ShaderViewFormat = BufferFormat.Unknown,
                                                         UnorderedAccessViewFormat = BufferFormat.Unknown,
                                                         Usage = BufferUsage.Default
                                                     });

                Assert.IsNotNull(_1D.DefaultShaderView);
                Assert.AreEqual(_1D.Settings.Format, _1D.DefaultShaderView.Format);

                _2D = _graphics.Textures.CreateTexture<GorgonTexture2D>("Test2D",
                                                 new GorgonTexture2DSettings
                                                 {
                                                     Width = 256,
                                                     Height = 256,
                                                     ArrayCount = 1,
                                                     Format = BufferFormat.R8G8B8A8_UIntNormal,
                                                     MipCount = 1,
                                                     ShaderViewFormat = BufferFormat.Unknown,
                                                     UnorderedAccessViewFormat = BufferFormat.Unknown,
                                                     Usage = BufferUsage.Default
                                                 });

                Assert.IsNotNull(_2D.DefaultShaderView);
                Assert.AreEqual(_2D.Settings.Format, _2D.DefaultShaderView.Format);

                _3D = _graphics.Textures.CreateTexture<GorgonTexture3D>("Test3D",
                                                 new GorgonTexture3DSettings
                                                 {
                                                     Width = 256,
                                                     Height = 256,
                                                     Depth =  64,
                                                     Format = BufferFormat.R8G8B8A8_UIntNormal,
                                                     MipCount = 1,
                                                     ShaderViewFormat = BufferFormat.Unknown,
                                                     UnorderedAccessViewFormat = BufferFormat.Unknown,
                                                     Usage = BufferUsage.Default
                                                 });

                Assert.IsNotNull(_3D.DefaultShaderView);
                Assert.AreEqual(_3D.Settings.Format, _3D.DefaultShaderView.Format);
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
        #endregion
    }
}
