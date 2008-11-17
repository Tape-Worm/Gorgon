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
		#region Variables.
		Renderer _renderer = null;
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
			if (_renderer != null)
				_renderer.Dispose();

			Gorgon.Terminate();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			Visible = true;
			Gorgon.Initialize();
			Gorgon.SetMode(this);

			_renderer = Renderer.Load(@"..\..\..\..\PlugIns\bin\debug\GorgonSlimDXRenderer.dll", "Gorgon.SlimDXRenderer");

			Gorgon.Idle += new FrameEventHandler(Gorgon_Idle);
			Gorgon.Go();
		}

		void Gorgon_Idle(object sender, FrameEventArgs e)
		{
			Gorgon.Screen.Clear();
		}
		#endregion

		#region Constructor/Destructor.
		public Form1()
		{
			InitializeComponent();
		}
		#endregion
	}
}
