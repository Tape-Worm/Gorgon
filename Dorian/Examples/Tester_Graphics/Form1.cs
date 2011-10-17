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
		GorgonGraphics _graphics = null;
		GorgonSwapChain _swapChain = null;
		private bool _running = true;
		GorgonTimer _timer = new GorgonTimer(true);
		Form2 form2 = null;
		int framecounter = 0;

		private bool Idle(GorgonFrameRate timing)
		{			
			Text = "FPS: " + timing.FPS.ToString() + " DT:" + timing.FrameDelta.ToString();

			return true;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
		}

		protected override void OnLoad(EventArgs e)
		{			
			base.OnLoad(e);

			try
			{
				this.panelDX.Visible = false;
				Gorgon.Initialize(this);

				Gorgon.PlugIns.SearchPaths.Add(@"..\..\..\..\PlugIns\bin\debug");

				GorgonFrameRate.UseHighResolutionTimer = false;

				Gorgon.UnfocusedSleepTime = 10;
				Gorgon.AllowBackground = true;

				ClientSize = new System.Drawing.Size(640, 480);

				_graphics = new GorgonGraphics();
				_swapChain = _graphics.CreateSwapChain("Swap", new GorgonSwapChainSettings());

				Gorgon.Go(Idle);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
				Application.Exit();				
			}

		}

		void form2_FormClosing(object sender, FormClosingEventArgs e)
		{
			form2 = null;
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			try
			{				
				Gorgon.Terminate();
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
			}
		}

		public Form1()
		{
			InitializeComponent();
		}
	}
}
