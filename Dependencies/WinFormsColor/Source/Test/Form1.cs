using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    public partial class Form1 : Form
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            label1.Text = $"{colorPickerPanel1.Width}x{colorPickerPanel1.Height}";
        }


        public Form1()
        {
            InitializeComponent();
        }
    }
}
