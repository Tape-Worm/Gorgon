using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.Graphics;

namespace Tester
{
	public partial class Form1 : Form
	{
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			Gorgon.Initialize();

			Gorgon.SetMode(this);

			Gorgon.Idle += new FrameEventHandler(Gorgon_Idle);

			Gorgon.Go();
		}

		void Gorgon_Idle(object sender, FrameEventArgs e)
		{
			Gorgon.Screen.Clear();
			Gorgon.Screen.FilledCircle(Gorgon.Screen.Width / 2, Gorgon.Screen.Height / 2, 25.0f, Color.Black);
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			Gorgon.Terminate();
		}

		public Form1()
		{
			InitializeComponent();
		}
	}
}
