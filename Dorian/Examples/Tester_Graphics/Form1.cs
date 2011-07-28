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
using GorgonLibrary.Collections;
using GorgonLibrary.Graphics;

namespace Tester_Graphics
{
	public partial class Form1 : Form
	{
		GorgonGraphics _gfx = null;

		private bool Idle(GorgonFrameRate timing)
		{
			return true;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try 
			{
				Gorgon.Initialize(this);
				GorgonPlugInFactory.SearchPaths.Add(@"..\..\..\..\PlugIns\bin\debug");
				GorgonPlugInFactory.LoadPlugInAssembly("Gorgon.Graphics.D3D9.dll");

				_gfx = GorgonGraphics.CreateGraphics("GorgonLibrary.Graphics.GorgonD3D9", new GorgonNamedValue<string>[] {new GorgonNamedValue<string>("UseReferenceRasterizer", "1")});

				Gorgon.Go(Idle);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
				Close();
			}

		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			Gorgon.Stop();

			if (_gfx != null)
				_gfx.Dispose();

			Gorgon.Terminate();
		}

		public Form1()
		{
			InitializeComponent();
		}
	}
}
