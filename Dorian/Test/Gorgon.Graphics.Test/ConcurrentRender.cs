using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private GorgonRenderCommands _commands;
        private GorgonGraphics _deferred;
        private List<Task<GorgonRenderCommands>> _tasks;
            
        [StructLayout(LayoutKind.Sequential)]
        struct WVPBuffer
        {
            public Matrix World;
            public Matrix Projection;
            public Matrix View;
        }

        private WVPBuffer _wvp;

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

        async private Task<GorgonRenderCommands> UpdateDeferred(GorgonConstantBuffer constantBuffer)
        {
            _tasks.Add(new Task<GorgonRenderCommands>(() =>
                {
                    try
                    {
                        Matrix.Translation(2.0f, 0, 6.0f, out _wvp.World);
                        Matrix.Transpose(ref _wvp.World, out _wvp.World);

                        constantBuffer.Update(_framework.Graphics, ref _wvp);

                        _framework.Graphics.Output.DrawIndexed(0, 0, 6);
                    }
                    catch (Exception ex)
                    {
                        UI.GorgonDialogs.ErrorBox(null, ex);
                    }

                    return null;
                }));

            _tasks.Add(new Task<GorgonRenderCommands>(() =>
                {
                    try
                    {
                        Matrix.Translation(-2.0f, 0, 6.0f, out _wvp.World);
                        Matrix.Transpose(ref _wvp.World, out _wvp.World);

                        constantBuffer.Update(_deferred, ref _wvp);

                        _deferred.Output.DrawIndexed(0, 0, 6);
                    }
                    catch (Exception ex)
                    {
                        UI.GorgonDialogs.ErrorBox(null, ex);
                    }

                    return _deferred.FinalizeDeferred();
                }));

            for (int i = 0; i < _tasks.Count; i++)
            {
                _tasks[i].Start();
            }

            int taskCounter = 0;
            while (taskCounter < 2)
            {
                var task = await Task.WhenAny(_tasks);

                _tasks.Remove(task);

                taskCounter++;
            }

            return _tasks[1].Result;
        }

        [TestMethod]
        public void ConcurrentRendering()
        {
            _tasks = new List<Task<GorgonRenderCommands>>();

            _wvp.World = Matrix.Identity;
            _wvp.Projection = Matrix.Identity;
            _wvp.View = Matrix.Identity;

            _framework.CreateTestScene(BaseShaders, BaseShaders, true);

            using(var constantBuffer = _framework.Graphics.Buffers.CreateConstantBuffer("WVP",
                                                                                        new GorgonConstantBufferSettings
                                                                                            {
                                                                                            SizeInBytes  = DirectAccess.SizeOf<WVPBuffer>()
                                                                                            }))
            {
                _deferred = _framework.Graphics.CreateDeferredGraphics();
                _framework.Graphics.Shaders.VertexShader.ConstantBuffers[0] = constantBuffer;

                _framework.Screen.BeforeSwapChainResized += (sender, e) =>
                    {
                        if (_commands != null)
                        {
                            _commands.Dispose();
                        }

                        if (_deferred == null)
                        {
                            return;
                        }
                        _deferred.Dispose();
                        _deferred = null;
                    };

                _framework.Screen.AfterSwapChainResized += (sender, e) =>
                    {
                        if (_deferred == null)
                        {
                            _deferred = _framework.Graphics.CreateDeferredGraphics();
                        }

                        Matrix.PerspectiveFovLH(80.0f.Radians(), (float)_framework.Screen.Settings.Width / _framework.Screen.Settings.Height, 0.1f, 100.0f, out _wvp.Projection);
                        Matrix.Transpose(ref _wvp.Projection, out _wvp.Projection);

                        _deferred.Shaders.VertexShader.ConstantBuffers[0] = constantBuffer;
                        _deferred.Shaders.VertexShader.Current = _framework.VertexShader;
                        _deferred.Shaders.PixelShader.Current = _framework.PixelShader;

                        _deferred.Input.IndexBuffer = _framework.Indices;
                        _deferred.Input.Layout = _framework.Layout;
                        _deferred.Input.PrimitiveType = PrimitiveType.TriangleList;
                        _deferred.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_framework.Vertices, 40);

                        _deferred.Output.SetRenderTarget(_framework.Screen, _framework.Screen.DepthStencilBuffer);

                        _deferred.Rasterizer.SetViewport(new GorgonViewport(0, 0, _framework.Screen.Settings.Width, _framework.Screen.Settings.Height, 0, 1));
                    };

                _framework.IdleFunc = () =>
                    {
                        using(_commands = UpdateDeferred(constantBuffer).Result)
                        {
                            _framework.Graphics.ExecuteDeferred(_commands);
                        }

                        return true;
                    };

                Assert.IsTrue(_framework.Run() == DialogResult.Yes);
            }
        }
    }
}
