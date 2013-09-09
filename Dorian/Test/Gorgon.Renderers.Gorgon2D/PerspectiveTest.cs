using System;
using System.Drawing;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Test;
using GorgonLibrary.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SlimMath;

namespace GorgonLibrary.Renderers
{
    /// <summary>
    /// Summary description for PerspectiveTest
    /// </summary>
    [TestClass]
    public class PerspectiveTest
    {
        private TestForm _form;
        private GorgonGraphics _graphics;
        private Gorgon2D _renderer;
        private GorgonSwapChain _screen;

        [TestInitialize]
        public void TestInitialize()
        {
            _form = new TestForm();
            _graphics = new GorgonGraphics();
            _renderer = _graphics.Output.Create2DRenderer(_form.panelDisplay);
            _screen = (GorgonSwapChain)_renderer.DefaultTarget;
        }

        [TestCleanup]
        public void CleanUp()
        {
            if (_renderer != null)
            {
                _renderer.Dispose();
                _renderer = null;
            }
            if (_screen != null)
            {
                _screen.Dispose();
                _screen = null;
            }
            if (_graphics != null)
            {
                _graphics.Dispose();
                _graphics = null;
            }
            if (_form != null)
            {
                _form.Dispose();
                _form = null;
            }
        }

        [TestMethod]
        public void TestPerspectiveCamera()
        {
            _form.Show();
            _form.ClientSize = new Size(1280,800);
            _form.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width / 2 - 640, Screen.PrimaryScreen.WorkingArea.Height / 2 - 400);
            _form.BringToFront();
            _form.WindowState = FormWindowState.Minimized;
            _form.WindowState = FormWindowState.Normal;

            var camera = _renderer.CreateCamera<GorgonPerspectiveCamera>("TestCam",
                _form.ClientSize,
                1000.0f);

            using(var texture = _graphics.Textures.FromFile<GorgonTexture2D>("Test", @"..\..\..\..\Resources\Images\Ship.png", new GorgonCodecPNG()))
            {
                var sprite = _renderer.Renderables.CreateSprite("Test",
                    new GorgonSpriteSettings
                    {
                        //Texture = texture,
                        Size = new Vector2(texture.Settings.Width, texture.Settings.Height),
                        TextureRegion =
                            new RectangleF(Vector2.Zero,
                                texture.ToTexel(texture.Settings.Width, texture.Settings.Height))
                    });

                sprite.Depth = 3.0f;
                _renderer.Camera = camera;

                //sprite.Position = new Vector2(0.5f, 0.5f);

                Gorgon.Run(_form,
                    () =>
                    {
                        _renderer.Clear(Color.Black);

                        sprite.Draw();

                        _renderer.Render();

                        return true;
                    });

                Assert.IsTrue(_form.TestResult == DialogResult.Yes);
            }
        }
    }
}
