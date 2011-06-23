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

namespace Tester
{
	public partial class Form1 : Form
	{
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				Gorgon.Initialize(this);
				GorgonPlugInFactory.LoadPlugInAssembly(@"..\..\..\TesterPlugIn\Bin\Debug\TesterPlugIn.dll");
				GorgonPlugIn plugIn = GorgonPlugInFactory.PlugIns["TesterPlugIn.EntryPoint"];

				plugIn.GetType().GetMethod("ShowShit").Invoke(plugIn, null);
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
