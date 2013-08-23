using System;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.VisualStyles;
using GorgonLibrary.Graphics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GorgonLibrary.Renderers
{
    [TestClass]
    public class EffectsTest
    {
        private Form _testForm;
        private GorgonGraphics _graphics;
        private Gorgon2D _2D;

        [TestInitialize]
        public void Initialize()
        {
            _graphics = new GorgonGraphics();
            _testForm = new Form
            {
                Text = @"Test 2D"
            };

            _testForm.StartPosition = FormStartPosition.CenterScreen;
            _testForm.ClientSize = new Size(1280, 800);
            _testForm.WindowState = FormWindowState.Minimized;
            _testForm.Show();

            _2D = _graphics.Output.Create2DRenderer(_testForm);
        }

        [TestMethod]
        public void EffectCreation()
        {
            var blur = _2D.Effects.GaussianBlur;
            Assert.IsTrue(blur != null);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _graphics.Dispose();
        }
    }
}
