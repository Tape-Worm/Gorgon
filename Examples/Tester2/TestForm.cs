using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpUtilities.Utility;
using SharpUtilities.Native.Win32;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.Framework;

namespace Tester2
{
    public partial class TestForm : GorgonLibrary.Framework.GorgonApplicationWindow
    {
		public enum KeyList
		{
			KeyF1 = 0,
			KeyLeft = 1,
			KeyRight = 2,
			KeyUp = 3,
			KeyDown = 4,
			KeyPageUp = 5,
			KeyPageDown = 6,
			KeyM = 7,
			KeyA = 8,
			KeyD = 9,
			KeyR = 10,
			KeyL = 11,
			KeyG = 12,
			KeyT = 13,
			KeyB = 14,
			KeyShift = 15,
			KeySpace = 16
		}

        public MainClass mainObj = null;
		public bool[] keys = new bool[256];
	
		public TestForm()
			: base(@".\GorgonSettings.xml")
        {
            InitializeComponent();
        }

		protected override void RunLogo()
		{
			//base.RunLogo();
		}

		protected override void Initialize()
		{
			/*ConfigurationSettings.Paths.Clear();
			ConfigurationSettings.Paths.AddPathsRow("ResourcePath", @"..\..\..\..\Resources");
			ConfigurationSettings.Paths.AddPathsRow("PlugInPathX86", @"..\..\..\..\PlugIns\bin\Release");
			ConfigurationSettings.Paths.AddPathsRow("PlugInPathX64", @"..\..\..\..\PlugIns\bin64\Release");
			ConfigurationSettings.FileSystems.AddFileSystemsRow("TesterFileSystem", @"release\GorgonBZip2FileSystem.dll", string.Empty, "Gorgon.BZip2FileSystem", true);
			ConfigurationSettings.WriteXml("GorgonSettings.xml");*/
			/*Gorgon.RenderTargets.CreateRenderWindow("ATestPanel", panel1, 100);
			Gorgon.RenderTargets["ATestPanel"].DefaultWindow.OnFrameBegin += new FrameEventHandler(OnFrameBegin);
			Gorgon.RenderTargets["ATestPanel"].DefaultWindow.OnFrameEnd += new FrameEventHandler(OnFrameEnd);*/

/*			Gorgon.RenderTargets.CreateRenderWindow("ATestPanel", panel1, 100);
			Gorgon.RenderTargets["ATestPanel"].DefaultWindow.OnFrameBegin += new FrameEventHandler(OnFrameBegin);
			Gorgon.RenderTargets["ATestPanel"].DefaultWindow.OnFrameEnd += new FrameEventHandler(OnFrameEnd);

			Gorgon.RenderTargets.CreateRenderWindow("ATestPanel2", panel2, 200);
			//Gorgon.RenderTargets["ATestPanel2"].DefaultWindow.OnFrameBegin += new FrameEventHandler(OnFrameBegin);
			//Gorgon.RenderTargets["ATestPanel2"].DefaultWindow.OnFrameEnd += new FrameEventHandler(OnFrameEnd);
			Gorgon.RenderTargets["ATestPanel2"].Viewports.Create("PanelWindow1", 100, 0, panel2.Width, panel2.Height, 1);
			Gorgon.RenderTargets["ATestPanel2"].Viewports.Create("PanelWindow2", 0, 0, panel2.Width/3, 50, 2);
			Gorgon.RenderTargets["ATestPanel2"].Viewports["PanelWindow1"].OnFrameBegin += new FrameEventHandler(OnFrameBegin);
			Gorgon.RenderTargets["ATestPanel2"].Viewports["PanelWindow1"].OnFrameEnd += new FrameEventHandler(OnFrameEnd);
			Gorgon.RenderTargets["ATestPanel2"].Viewports["PanelWindow2"].OnFrameBegin += new FrameEventHandler(OnFrameBegin);
			Gorgon.RenderTargets["ATestPanel2"].Viewports["PanelWindow2"].OnFrameEnd += new FrameEventHandler(OnFrameEnd);*/

			mainObj = new MainClass(this);
			mainObj.Initialize();
			
			//Win32API.ClipCursor(new Rectangle(this.PointToScreen(new Point(0, 0)),new Size(640,480)));
			Gorgon.Go();			
		}

		protected override void OnDeviceReset()
		{
			mainObj.DeviceReset();
		}

		protected override void OnFrameUpdate(FrameEventArgs e)
		{
			base.OnFrameUpdate(e);
			mainObj.Render(e);
		}

//        protected override void OnLoad(EventArgs e)
//        {
//            base.OnLoad(e);

//            //this.UseD3DReferenceDevice = true;
///*			Gorgon.VSync = true;
//            Gorgon.VSyncInterval = VSyncIntervals.IntervalOne;*/
//            //Gorgon.UseReferenceDevice = true;
//            if (!DesignMode)
//            {
//                try
//                {
//                    if (!Setup())
//                    {
//                        Application.Exit();
//                        return;
//                    }
//                }
//                catch (Exception ex)
//                {
//                    UI.ErrorBox(this, ex);
//                    Application.Exit();
//                }
//            }
//        }

        private void TestForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
			switch (e.KeyCode)
			{
				case Keys.Escape:
					Close();
					break;
				case Keys.Left:
					keys[(int)KeyList.KeyLeft] = true;
					break;
				case Keys.Right:
					keys[(int)KeyList.KeyRight] = true;
					break;
				case Keys.Up:
					keys[(int)KeyList.KeyUp] = true;
					break;
				case Keys.Down:
					keys[(int)KeyList.KeyDown] = true;
					break;
				case Keys.PageUp:
					keys[(int)KeyList.KeyPageUp] = true;
					break;
				case Keys.PageDown:
					keys[(int)KeyList.KeyPageDown] = true;
					break;
				case Keys.Space:
					keys[(int)KeyList.KeySpace] = true;
					break;
				case Keys.ShiftKey:
				case Keys.LShiftKey:
				case Keys.RShiftKey:
					keys[(int)KeyList.KeyShift] = true;
					break;
			}

			if (e.KeyCode == Keys.F1)
				Gorgon.Screen.Windowed = !Gorgon.Screen.Windowed;

			if (e.KeyCode == Keys.F10)
			{
				Gorgon.Go();
				e.Handled = true;
			}
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			switch (e.KeyCode)
			{
				case Keys.Left:
					keys[(int)KeyList.KeyLeft] = false;
					break;
				case Keys.Right:
					keys[(int)KeyList.KeyRight] = false;
					break;
				case Keys.Up:
					keys[(int)KeyList.KeyUp] = false;
					break;
				case Keys.Down:
					keys[(int)KeyList.KeyDown] = false;
					break;
				case Keys.PageUp:
					keys[(int)KeyList.KeyPageUp] = false;
					break;
				case Keys.PageDown:
					keys[(int)KeyList.KeyPageDown] = false;
					break;
				case Keys.Space:
					keys[(int)KeyList.KeySpace] = false;
					break;
				case Keys.ShiftKey:
				case Keys.LShiftKey:
				case Keys.RShiftKey:
					keys[(int)KeyList.KeyShift] = false;
					break;
			}
			if (e.KeyCode == Keys.U)
				Cursor.Clip = Rectangle.Empty;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e); 
			
			if (mainObj != null)
				mainObj.Dispose();
			else
				Gorgon.Terminate();
		}

		/// <summary>
		/// Handles the MouseMove event of the _control control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void _control_MouseMove(object sender, MouseEventArgs e)
		{
			if (mainObj != null)
				mainObj.MouseMove(e);
		}

    }
}