using System;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.Graphics.Test.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GorgonLibrary.Graphics.Test
{
    [TestClass]
    public class ConcurrentRender
    {
        private GraphicsFramework _framework;
        private string _shaders;

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
                    _shaders = Encoding.UTF8.GetString(Resources.ShaderTests);
                }

                return _shaders;
            }
        }

        [TestMethod]
        public void ConcurrentRendering()
        {
            _framework.CreateTestScene(BaseShaders, BaseShaders, true);

            Assert.IsTrue(_framework.Run() == DialogResult.Yes);
        }
    }
}
