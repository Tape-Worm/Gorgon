using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestGDIShapes
{
	public partial class Form1 : Form
	{
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			e.Graphics.DrawRectangle(Pens.Black, new Rectangle(20, 20, 100, 100));
			e.Graphics.FillRectangle(Brushes.Black, new Rectangle(140, 20, 100, 100));
		}

		public Form1()
		{
			InitializeComponent();
		}
	}
}
