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
    public class TextTest
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
        public void TestText()
        {
            _form.Show();
            _form.ClientSize = new Size(1280, 800);
            _form.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width / 2 - 640, Screen.PrimaryScreen.WorkingArea.Height / 2 - 400);
            _form.BringToFront();
            _form.WindowState = FormWindowState.Minimized;
            _form.WindowState = FormWindowState.Normal;

            GorgonText text = _renderer.Renderables.CreateText("Test",
                                                               _graphics.Fonts.DefaultFont,
                                                               "The quick brown fox jumps over the lazy dog.\n1234567890 !@#$%^&*() ABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyz");

            Gorgon.Run(_form, () =>
            {
                _renderer.Clear(Color.Black);

                text.Draw();

                _renderer.Render();

                return true;
            });

            Assert.IsTrue(_form.TestResult == DialogResult.Yes);
        }
    }
}
