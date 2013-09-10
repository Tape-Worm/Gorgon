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
                new RectangleF(Vector2.Zero, _form.ClientSize),
				0.01f,
                1000.0f);
			
            using(var texture = _graphics.Textures.FromFile<GorgonTexture2D>("Test", @"..\..\..\..\Resources\Images\Ship.png", new GorgonCodecPNG()))
            {
				var size = new Vector2(texture.Settings.Width, texture.Settings.Height);
				
				size = new Vector2((size.X / _screen.Settings.Width) * 2.0f, (size.Y / _screen.Settings.Height) * 2.0f);
	            //size = size * 4.0f;

                var sprite = _renderer.Renderables.CreateSprite("Test",
                    new GorgonSpriteSettings
                    {
                        Texture = texture,
                        Size = size,
                        TextureRegion =
                            new RectangleF(Vector2.Zero,
                                texture.ToTexel(texture.Settings.Width, texture.Settings.Height))
                    });

				camera.ViewDimensions = new Vector2(1);
				camera.ViewOffset = new Vector2(-1);
	            camera.Zoom = new Vector2(2);
                _renderer.Camera = camera;
	            //camera.AutoUpdate = true;
				//camera.Update();

                //sprite.Position = new Vector2(0.15f, 0.15f);

				sprite.Depth = 0.01f;

	            //camera.Anchor = new Vector2(_screen.Settings.Width / 2.0f, _screen.Settings.Height / 2.0f);
	            //_screen.AfterSwapChainResized += (sender, args) => camera.Anchor = new Vector2(_screen.Settings.Width / 2.0f, _screen.Settings.Height / 2.0f);

	            _form.MouseWheel += (sender, args) =>
	            {
		            //sprite.Depth += args.Delta / 32000.0f;
	            };

                Gorgon.Run(_form, () =>
                    {
                        _renderer.Clear(Color.Black);

						sprite.Position = Vector2.Zero;
						sprite.CullingMode = CullingMode.Back;
                        sprite.Draw();

	                    Vector2 cursor = _form.PointToClient(Cursor.Position);

	                    //cursor.X = cursor.X - _screen.Settings.Width / 2.0f;
	                    //cursor.Y = cursor.Y - _screen.Settings.Height / 2.0f;
	                    //camera.Position = cursor;
	                    camera.Draw();

                        _renderer.Render();

                        return true;
                    });

                Assert.IsTrue(_form.TestResult == DialogResult.Yes);
            }
        }
    }
}
