using System.Drawing;
using System.Windows.Forms;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SlimMath;

namespace GorgonLibrary.Renderers
{
    /// <summary>
    /// Summary description for PerspectiveTest
    /// </summary>
    [TestClass]
    public class PrimitivesTest
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
        public void TestEllipse()
        {
            _form.Show();
            _form.ClientSize = new Size(1280, 800);
            _form.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width / 2 - 640, Screen.PrimaryScreen.WorkingArea.Height / 2 - 400);
            _form.BringToFront();
            _form.WindowState = FormWindowState.Minimized;
            _form.WindowState = FormWindowState.Normal;

            GorgonEllipse ellipse = _renderer.Renderables.CreateEllipse("Test",
                                                                        new Vector2(320, 400),
                                                                        new Vector2(100, 100),
                                                                        Color.Blue,
                                                                        true,
                                                                        32);

            Gorgon.Run(_form, () =>
            {
                _renderer.Clear(Color.Black);

                ellipse.Color = Color.Blue;
                ellipse.IsFilled = true;
                ellipse.Position = new Vector2(480, 400);
                ellipse.Draw();

                ellipse.Color = Color.Green;
                ellipse.IsFilled = false;
                ellipse.Position = new Vector2(800, 400);
                ellipse.Draw();

                _renderer.Render();

                return true;
            });

            Assert.IsTrue(_form.TestResult == DialogResult.Yes);
        }

        [TestMethod]
        public void TestPolygon()
        {
            _form.Show();
            _form.ClientSize = new Size(1280, 800);
            _form.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width / 2 - 640, Screen.PrimaryScreen.WorkingArea.Height / 2 - 400);
            _form.BringToFront();
            _form.WindowState = FormWindowState.Minimized;
            _form.WindowState = FormWindowState.Normal;

            GorgonPolygon polygon = _renderer.Renderables.CreatePolygon("Test", new Vector2(320, 240), Color.RosyBrown);

            polygon.SetVertexData(new GorgonPolygonPoint[]
                                  {
                                      new GorgonPolygonPoint(new Vector2(0, 20), GorgonColor.White), 
                                      new GorgonPolygonPoint(new Vector2(20, 0), GorgonColor.White), 
                                      new GorgonPolygonPoint(new Vector2(40, 20), GorgonColor.White),
                                      new GorgonPolygonPoint(new Vector2(10, 40), GorgonColor.White), 
                                      new GorgonPolygonPoint(new Vector2(30, 40), GorgonColor.White)
                                  });

            polygon.SetIndexData(new []
                                 {
                                     0, 1, 2,
                                     2, 3, 0,
                                     3, 2, 4
                                 });

            GorgonPolygon polygon2 = _renderer.Renderables.CreatePolygon("Test", new Vector2(320, 240), Color.GreenYellow);

            polygon2.PolygonType = PolygonType.Line;
            polygon2.SetVertexData(new GorgonPolygonPoint[]
                                  {
                                      new GorgonPolygonPoint(new Vector2(0, 20), GorgonColor.White), 
                                      new GorgonPolygonPoint(new Vector2(20, 0), GorgonColor.White), 
                                      new GorgonPolygonPoint(new Vector2(40, 20), GorgonColor.White),
                                      new GorgonPolygonPoint(new Vector2(10, 40), GorgonColor.White), 
                                      new GorgonPolygonPoint(new Vector2(30, 40), GorgonColor.White)
                                  });

            polygon2.SetIndexData(new[]
                                 {
                                     0, 1,
                                     1, 2,
                                     2, 4,
                                     4, 3,
                                     3, 0
                                 });

            Gorgon.Run(_form, () =>
            {
                _renderer.Clear(Color.Black);

                polygon.Draw();
                polygon2.Draw();

                _renderer.Render();

                return true;
            });

            Assert.IsTrue(_form.TestResult == DialogResult.Yes);
        }
    }
}
