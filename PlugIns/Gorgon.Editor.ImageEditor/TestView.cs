using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Editor.UI.Views;
using Gorgon.Graphics;

namespace Gorgon.Editor.ImageEditor
{
    public partial class TestView 
        : ContentBaseControl
    {
        private bool Idle()
        {
            SwapChain.RenderTargetView.Clear(new GorgonColor(255, 0, 255));

            SwapChain.Present();

            return true;
        }

        protected override void OnLoad(EventArgs e)
        {
            IdleMethod = Idle;
        }

        public TestView()
        {
            InitializeComponent();
        }
    }
}
