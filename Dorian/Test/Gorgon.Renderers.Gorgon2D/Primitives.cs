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
    }
}
