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
		GorgonDeviceWindow _dev2 = null;		

		private bool Idle(GorgonFrameRate timing)
		{
			_dev.RunTest();
			//_dev2.RunTest();
			return true;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			try
			{
				if (e.KeyCode == Keys.F1)
				{
					_dev.Update(_dev.Mode, _dev.DepthStencilFormat, _dev.IsWindowed);
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
				Close();
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try 
			{
				this.panelDX.Visible = false;
				Gorgon.Initialize(this);
				GorgonPlugInFactory.SearchPaths.Add(@"..\..\..\..\PlugIns\bin\debug");
				GorgonPlugInFactory.LoadPlugInAssembly("Gorgon.Graphics.D3D9.dll");
								
				Gorgon.UnfocusedSleepTime = 10;
				Gorgon.AllowBackground = true;
				ClientSize = new System.Drawing.Size(640, 480);
				_gfx = GorgonGraphics.CreateGraphics("GorgonLibrary.Graphics.GorgonD3D9");
				//_dev = _gfx.CreateDeviceWindow("Test", new GorgonVideoMode(640, 480, GorgonBufferFormat.R8G8B8A8_UIntNorm, 60, 1), GorgonBufferFormat.D24_UIntNorm_S8_UInt, false);
				//_dev = _gfx.CreateDeviceWindow("Test", GorgonBufferFormat.D24_UIntNorm_S8_UInt, true);
				_dev = _gfx.CreateDeviceWindow("Test", new GorgonDeviceWindowSettings() { DisplayMode = new GorgonVideoMode(640, 480, GorgonBackBufferFormat.A8R8G8B8_UIntNormal), Windowed = true, DepthStencilFormat = GorgonDepthBufferFormat.D16_UIntNormal});
				_dev.SetupTest();

				//form2 = new Form2();				
				//form2.Show();
				//form2.Location = new Point(Screen.AllScreens[1].Bounds.Width / 2 + Screen.AllScreens[1].Bounds.Left, Screen.AllScreens[1].Bounds.Height / 2 + Screen.AllScreens[1].Bounds.Top);

				//_dev2 = _gfx.CreateDeviceWindow("Test2", form2, new GorgonVideoMode(640, 480, GorgonBufferFormat.R8G8B8A8_UIntNorm, 60, 1), GorgonBufferFormat.D24_UIntNorm_S8_UInt, true);
				//_dev2.SetupTest();
				
				Gorgon.Go(Idle);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
				Application.Exit();				
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
