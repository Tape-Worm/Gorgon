using System;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.Graphics.Test.Properties;
using GorgonLibrary.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices;
using SlimMath;
using GorgonLibrary;
using GorgonLibrary.Native;
using GorgonLibrary.Math;

namespace GorgonLibrary.Graphics.Test
{
    [TestClass]
    public class ConcurrentRender
    {
        private GraphicsFramework _framework;
        private string _shaders;

        [StructLayout(LayoutKind.Sequential)]
        struct WVPBuffer
        {
            public Matrix World;
            public Matrix Projection;
            public Matrix View;
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
        
        /// <summary>
        /// Property to return the shaders for the test.
        /// </summary>
        private string BaseShaders
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_shaders))
                {
                    _shaders = Encoding.UTF8.GetString(Resources.Deferred);
                }

                return _shaders;
            }
        }

        private void UpdateMatrix(WVPBuffer wvpData)
        {
            
        }

        [TestMethod]
        public void ConcurrentRendering()
        {
            var wvp = new WVPBuffer();
            wvp.World = Matrix.Identity;
            wvp.Projection = Matrix.Identity;
            wvp.View = Matrix.Identity;

            _framework.CreateTestScene(BaseShaders, BaseShaders, true);

            var deferred = _framework.Graphics.CreateDeferredGraphics();

            Matrix.PerspectiveFovLH(80.0f.Radians(), (float)_framework.Screen.Settings.Width / _framework.Screen.Settings.Height, 0.1f, 100.0f, out wvp.Projection);
            Matrix.Translation(-2.0f, 0, 6.0f, out wvp.World);

            Matrix.Transpose(ref wvp.Projection, out wvp.Projection);
            Matrix.Transpose(ref wvp.World, out wvp.World);

            using(var stream = new GorgonDataStream(DirectAccess.SizeOf<WVPBuffer>()))
            {
                stream.Write(wvp);
                stream.Position = 0;
                using(var constantBuffer = _framework.Graphics.Buffers.CreateConstantBuffer("WVP",
                                                                                          new GorgonConstantBufferSettings
                                                                                              {
                                                                                                SizeInBytes  = DirectAccess.SizeOf<WVPBuffer>()
                                                                                              },
                                                                                          stream))
                {
                    deferred.Shaders.VertexShader.ConstantBuffers[0] = constantBuffer;
                    deferred.Shaders.VertexShader.Current = _framework.VertexShader;
                    deferred.Shaders.PixelShader.Current = _framework.PixelShader;

                    deferred.Input.IndexBuffer = _framework.Indices;
                    deferred.Input.Layout = _framework.Layout;
                    deferred.Input.PrimitiveType = PrimitiveType.TriangleList;
                    deferred.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_framework.Vertices, 40);

                    deferred.Output.SetRenderTarget(_framework.Screen, _framework.Screen.DepthStencilBuffer);

                    deferred.Rasterizer.SetViewport(new GorgonViewport(0, 0, _framework.Screen.Settings.Width, _framework.Screen.Settings.Height, 0, 1));

                    deferred.Output.DrawIndexed(0, 0, 6);

                    _framework.IdleFunc = () =>
                    {
                        using(var commands = deferred.FinalizeDeferred())
                        {
                            _framework.Graphics.ExecuteDeferred(commands);
                        }
                        _framework.Graphics.Output.SetRenderTarget(_framework.Screen);
                        _framework.Graphics.Rasterizer.SetViewport(new GorgonViewport(0, 0, _framework.Screen.Settings.Width, _framework.Screen.Settings.Height, 0, 1));

                        return true;
                    };

                    Assert.IsTrue(_framework.Run() == DialogResult.Yes);
                }
            }

        }
    }
}
