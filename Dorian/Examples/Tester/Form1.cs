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
using GorgonLibrary.PlugIns;
using GorgonLibrary.Graphics;
using GorgonLibrary.Input;

namespace Tester
{
	public partial class Form1 : Form
	{
		GorgonLibrary.Input.Raw.GorgonRawInputFactory input = null;
		GorgonPointingDevice mouse = null;

		private bool Idle(GorgonFrameRate timing)
		{
			labelMouse.Text = mouse.Position.X.ToString() + "x" + mouse.Position.Y.ToString();
			return true;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				Gorgon.Initialize(this);
				GorgonPlugInFactory.LoadPlugInAssembly(@"..\..\..\TesterPlugIn\Bin\Debug\TesterPlugIn.dll");
				GorgonPlugIn plugIn = GorgonPlugInFactory.PlugIns["TesterPlugIn.EntryPoint"];
				input = new GorgonLibrary.Input.Raw.GorgonRawInputFactory();
				mouse = input.CreatePointingDevice();
				mouse.Exclusive = true;				
				mouse.PositionRange = new RectangleF(50,50, 100, 100);

				Gorgon.Go(Idle);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			mouse.Dispose();
			Gorgon.Terminate();
		}

		public Form1()
		{
			InitializeComponent();
		}
	}
}
