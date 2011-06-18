using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.UI;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics;

namespace Tester
{
	public partial class Form1 : Form
	{
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				//Gorgon.Initialize(this);

				//Gorgon.SetMode(this);

				Gorgon.Go((timingData) =>
				{
					Gorgon.Screen.Clear();
					Gorgon.Screen.FilledCircle(Gorgon.Screen.Width / 2, Gorgon.Screen.Height / 2, 25.0f, Color.Black);
					Gorgon.Screen.Update();
					return true;
				}
				);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
			}
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
