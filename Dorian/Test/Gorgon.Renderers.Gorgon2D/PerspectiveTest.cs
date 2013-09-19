using System;
using System.Drawing;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
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
		public void TestOrthoCamera()
		{
			_form.Show();
			_form.ClientSize = new Size(1280, 800);
			_form.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width / 2 - 640, Screen.PrimaryScreen.WorkingArea.Height / 2 - 400);
			_form.BringToFront();
			_form.WindowState = FormWindowState.Minimized;
			_form.WindowState = FormWindowState.Normal;

			var camera = _renderer.CreateCamera<Gorgon2DOrthoCamera>("TestCam",
				new RectangleF(Vector2.Zero, _form.ClientSize),
				0.0f,
				1.0f);

			var camera2 = _renderer.CreateCamera<Gorgon2DPerspectiveCamera>("TestCam2",
				new RectangleF(-1, -1, 2, 2),
				0.01f,
				100.0f);

			using (var texture = _graphics.Textures.FromFile<GorgonTexture2D>("Test", @"..\..\..\..\Resources\Images\Ship.png", new GorgonCodecPNG()))
			{
				//camera.ViewDimensions = new RectangleF(0, 0, 1, 1);
				camera.ViewDimensions = new RectangleF(-1, -1, 2, 2);
				/*camera.ViewDimensions = new RectangleF(
					new Vector2(_screen.Settings.Width / -2.0f, _screen.Settings.Height / -2.0f),
					new Vector2(_screen.Settings.Width / 2.0f, _screen.Settings.Height / 2.0f));*/
				//camera.Anchor = new Vector2(0.5f, 0.5f);
				//camera.Zoom = new Vector2(2);
				//camera.Anchor = new Vector2(640, 400);
				//camera.Angle = 45.0f;

				var size = new Vector2(texture.Settings.Width * (camera.ViewDimensions.Width / _screen.Settings.Width),
					texture.Settings.Height * (camera.ViewDimensions.Height / _screen.Settings.Height));
				
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

				_renderer.Camera = camera;

				sprite.Depth = 0.0f;

				sprite.Position = (Vector2)camera.Project(new Vector2(320, 200));

				Gorgon.Run(_form, () =>
				{
					var projectedSpace = -(Vector2)camera.Project(new Vector3(_form.PointToClient(Cursor.Position), 0), false);
					var projectedViewSpace = (Vector2)camera.Project(new Vector2(320, 200));
					var unprojectedSpace = (Vector2)camera.Unproject(projectedSpace, false);
					var unprojectedViewSpace = (Vector2)camera.Unproject(projectedViewSpace);

					_renderer.Clear(Color.Black);

					_renderer.Camera = camera;
					
					sprite.Draw();
					
					//Vector2 projectedSpace = camera.Project(new Vector2(320, 200));

					camera.Position = projectedSpace;
					camera2.Draw();
					camera.Draw();

                    _renderer.DefaultCamera.Draw();

					_renderer.Camera = null;
					_renderer.Drawing.DrawEllipse(new RectangleF(310, 190, 20, 20), Color.Firebrick, 64, new Vector2(1));
					_renderer.Drawing.DrawString(_graphics.Fonts.DefaultFont,
						string.Format("X:{0:0.00}, Y:{1:0.00}\nU:{2:0.00}, V:{3:0.00}\nI:{4:0.00}, J:{5:0.00}\nK:{8:0.00}, L:{9:00}\nSize X:{6:0.00}, Size Y:{7:0.00}", projectedSpace.X, projectedSpace.Y, unprojectedSpace.X, unprojectedSpace.Y, projectedViewSpace.X, projectedViewSpace.Y, size.X, size.Y, unprojectedViewSpace.X, unprojectedViewSpace.Y),
						Vector2.Zero,
						Color.Yellow);


					_renderer.Render();

					return true;
				});

				Assert.IsTrue(_form.TestResult == DialogResult.Yes);
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

            var camera = _renderer.CreateCamera<Gorgon2DPerspectiveCamera>("TestCam",
                new RectangleF(Vector2.Zero, _form.ClientSize),
				0.01f,
                1000.0f);

			var camera2 = _renderer.CreateCamera<Gorgon2DPerspectiveCamera>("TestCam2",
				new RectangleF(-1, -1, 2, 2),
				0.1f,
				1.0f);
			
            using(var texture = _graphics.Textures.FromFile<GorgonTexture2D>("Test", @"..\..\..\..\Resources\Images\Ship.png", new GorgonCodecPNG()))
            {
				//camera.ViewDimensions = new RectangleF(0, 0, 1, 1);
				camera.ViewDimensions = new RectangleF(-1, -1, 2, 2);
				//camera.ViewDimensions = new RectangleF(-640, -400, 1280, 800);

				var size = new Vector2(texture.Settings.Width * (camera.ViewDimensions.Width / _screen.Settings.Width) * 12.80f,
					texture.Settings.Height * (camera.ViewDimensions.Height / _screen.Settings.Height) * 12.80f);

                var sprite = _renderer.Renderables.CreateSprite("Test",
                    new GorgonSpriteSettings
                    {
                        Texture = texture,
                        Size = size,
                        TextureRegion =
                            new RectangleF(Vector2.Zero,
                                texture.ToTexel(texture.Settings.Width, texture.Settings.Height))
                    });

                sprite.Anchor = size / 2.0f;

				
                _renderer.Camera = camera;

				sprite.Depth = 0.0001f;
				//sprite.Position = (Vector2)camera.Project(new Vector3(320, 200, sprite.Depth));

	            //camera.Anchor = new Vector2(_screen.Settings.Width / 2.0f, _screen.Settings.Height / 2.0f);
	            //_screen.AfterSwapChainResized += (sender, args) => camera.Anchor = new Vector2(_screen.Settings.Width / 2.0f, _screen.Settings.Height / 2.0f);

	            _form.MouseWheel += (sender, args) =>
	            {
		            sprite.Depth += args.Delta / 32000.0f;
	            };


	            float camDepth = 0.01f;
	            _form.KeyPreview = true;
				_form.Focus();
				_form.KeyPress += (sender, args) =>
	            {
		            if (args.KeyChar == 'w')
		            {
			            camDepth -= 12.0f * GorgonTiming.Delta;
		            }

		            if (args.KeyChar == 's')
		            {
						camDepth += 12.0f * GorgonTiming.Delta;
		            }
	            };

                Gorgon.Run(_form, () =>
                    {
						Vector2 cursorPos = _form.PointToClient(Cursor.Position);
						Vector3 projectedSpace = camera.Project(new Vector3(cursorPos, 0), false);
						Vector3 projectedViewSpace = camera.Project(new Vector3(320, 200, 0.01f));
						Vector3 unprojectedSpace = camera.Unproject(projectedSpace, false);
						Vector3 unprojectedViewSpace = camera.Unproject(projectedViewSpace);

                        //_renderer.DefaultCamera.Position = cursorPos;

						_renderer.Clear(Color.Black);

	                    projectedSpace.X = -projectedSpace.X;
						projectedSpace.Y = -projectedSpace.Y;
	                    projectedSpace.Z = camDepth;

						_renderer.Camera = camera;
						sprite.Draw();
                    
	                    camera.Position = projectedSpace;

						camera2.Draw();
                        camera.Draw();
                        _renderer.DefaultCamera.Draw();

						_renderer.Camera = null;
						_renderer.Drawing.DrawEllipse(new RectangleF(310, 190, 20, 20), Color.Firebrick, 64, new Vector2(1));
						_renderer.Drawing.DrawString(_graphics.Fonts.DefaultFont,
							string.Format("X:{0:0.00}, Y:{1:0.00}\nU:{2:0.00}, V:{3:0.00}\nI:{4:0.00}, J:{5:0.00}\nK:{8:0.00}, L:{9:00}\nSize X:{6:0.00}, Size Y:{7:0.00}, Depth: {10:0.000}", 
							projectedSpace.X, projectedSpace.Y, unprojectedSpace.X, unprojectedSpace.Y, projectedViewSpace.X, projectedViewSpace.Y, size.X, size.Y, 
							unprojectedViewSpace.X, unprojectedViewSpace.Y, sprite.Depth),
							Vector2.Zero,
							Color.Yellow);

                        _renderer.Render();

                        return true;
                    });

                Assert.IsTrue(_form.TestResult == DialogResult.Yes);
            }
        }
    }
}
