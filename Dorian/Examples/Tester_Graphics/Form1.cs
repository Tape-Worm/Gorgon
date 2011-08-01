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
		GorgonDeviceWindow _dev = null;

		private bool Idle(GorgonFrameRate timing)
		{
			_dev.RunTest();
			return true;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			try
			{
				if (e.KeyCode == Keys.F1)
				{
					_dev.Update(_dev.TargetInformation, _dev.DepthStencilFormat, _dev.IsWindowed);
				}

				if (e.KeyCode == Keys.Back)
				{
					_dev.Dispose();
					_dev = null;
				}
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try 
			{
				Gorgon.Initialize(this);
				GorgonPlugInFactory.SearchPaths.Add(@"..\..\..\..\PlugIns\bin\debug");
				GorgonPlugInFactory.LoadPlugInAssembly("Gorgon.Graphics.D3D9.dll");

				//this.Location = new Point(Screen.AllScreens[1].Bounds.Width / 2 + Screen.AllScreens[1].Bounds.Left, Screen.AllScreens[1].Bounds.Height / 2 + Screen.AllScreens[1].Bounds.Top);
				Gorgon.UnfocusedSleepTime = 10;
				Gorgon.AllowBackground = true;
				_gfx = GorgonGraphics.CreateGraphics("GorgonLibrary.Graphics.GorgonD3D9");
				_dev = _gfx.CreateDeviceWindow("Test", new GorgonVideoMode(640, 480, GorgonBufferFormat.R8G8B8A8_UIntNorm, 60, 1), GorgonBufferFormat.D24_UIntNorm_S8_UInt, false);
				_dev.SetupTest();

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

			try
			{
				if (_dev != null)
					_dev.Dispose();

				if (_gfx != null)
					_gfx.Dispose();

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
