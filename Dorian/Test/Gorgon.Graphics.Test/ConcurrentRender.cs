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
        private GorgonRenderCommands[] _commands;
        private GorgonGraphics[] _deferred;
        private List<Task> _tasks;
            
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

        private void UpdateDeferred(GorgonConstantBuffer constantBuffer)
        {
            _tasks.Add(Task.Run(() =>
                {
                    Matrix.Translation(2.0f, 0, 6.0f, out _wvp.World);
                    Matrix.Transpose(ref _wvp.World, out _wvp.World);

                    constantBuffer.Update(_deferred[0], ref _wvp);

                    _deferred[0].Output.DrawIndexed(0, 0, 6);
                    _commands[0] = _deferred[0].FinalizeDeferred();
                }));

            _tasks.Add(Task.Run(() =>
                {
                    Matrix.Translation(-2.0f, 0, 6.0f, out _wvp.World);
                    Matrix.Transpose(ref _wvp.World, out _wvp.World);

                    constantBuffer.Update(_deferred[1], ref _wvp);

                    _deferred[1].Output.DrawIndexed(0, 0, 6);
                    _commands[1] = _deferred[1].FinalizeDeferred();
                    
                }));

            try
            {
                Action runDeferred = async () =>
                    {
                        var task = await Task.WhenAny(_tasks);

                        _tasks.Remove(task);

                        await task;
                    };

                while (_tasks.Count > 0)
                {
                    runDeferred();
                    
                    System.Threading.Thread.Sleep(250);
                }
            }
            catch (Exception ex)
            {
                UI.GorgonDialogs.ErrorBox(null, ex);
                Gorgon.Quit();
            }
        }

        [TestMethod]
        public void ConcurrentRendering()
        {
            _tasks = new List<Task>();

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
                _commands = new GorgonRenderCommands[2];
                _deferred = new GorgonGraphics[]
                    {
                        _framework.Graphics.CreateDeferredGraphics(),
                        _framework.Graphics.CreateDeferredGraphics()
                    };
                _framework.Graphics.Shaders.VertexShader.ConstantBuffers[0] = constantBuffer;

                _framework.Screen.BeforeSwapChainResized += (sender, e) =>
                    {
                        for (int i = 0; i < _commands.Length; i++)
                        {
                            if (_commands[i] == null)
                            {
                                continue;
                            }

                            _commands[i].Dispose();
                            _commands[i] = null;
                        }

                        if (_deferred == null)
                        {
                            return;
                        }

                        for (int i = 0; i < _deferred.Length; i++)
                        {
                            _deferred[i].Dispose();
                            _deferred[i] = null;
                        }

                        _deferred = null;
                    };

                _framework.Screen.AfterSwapChainResized += (sender, e) =>
                    {
                        if (_deferred == null)
                        {
                            _deferred = new GorgonGraphics[]
                                {
                                    _framework.Graphics.CreateDeferredGraphics(),
                                    _framework.Graphics.CreateDeferredGraphics()
                                };
                        }

                        Matrix.PerspectiveFovLH(80.0f.Radians(), (float)_framework.Screen.Settings.Width / _framework.Screen.Settings.Height, 0.1f, 100.0f, out _wvp.Projection);
                        Matrix.Transpose(ref _wvp.Projection, out _wvp.Projection);

                        foreach (GorgonGraphics deferred in _deferred)
                        {
                            deferred.Shaders.VertexShader.ConstantBuffers[0] = constantBuffer;
                            deferred.Shaders.VertexShader.Current = _framework.VertexShader;
                            deferred.Shaders.PixelShader.Current = _framework.PixelShader;

                            deferred.Input.IndexBuffer = _framework.Indices;
                            deferred.Input.Layout = _framework.Layout;
                            deferred.Input.PrimitiveType = PrimitiveType.TriangleList;
                            deferred.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_framework.Vertices, 40);

                            deferred.Output.SetRenderTarget(_framework.Screen, _framework.Screen.DepthStencilBuffer);

                            deferred.Rasterizer.SetViewport(new GorgonViewport(0,
                                                                        0,
                                                                        _framework.Screen.Settings.Width,
                                                                        _framework.Screen.Settings.Height,
                                                                        0,
                                                                        1));
                        }
                    };

                _framework.IdleFunc = () =>
                    {
                        _tasks.Clear();

                        UpdateDeferred(constantBuffer);

                        for (int i = 0; i < _commands.Length; i++)
                        {
                            if (_commands[i] == null)
                            {
                                continue;
                            }

                            using(_commands[i])
                            {
                                _framework.Graphics.ExecuteDeferred(_commands[i]);
                            }
                            _commands[i] = null;
                        }

                        return true;
                    };

                Assert.IsTrue(_framework.Run() == DialogResult.Yes);
            }
        }
    }
}
