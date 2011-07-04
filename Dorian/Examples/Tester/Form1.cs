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
using GorgonLibrary.HID;
using GorgonLibrary.FileSystem;

namespace Tester
{
	public partial class Form1 : Form
	{
		GorgonInputDeviceFactory input = null;
		GorgonPointingDevice mouse = null;
		GorgonKeyboard keyboard = null;
		GorgonFileSystem fileSystem = null;
		GorgonCustomHID joystick = null;

		private bool Idle(GorgonFrameRate timing)
		{
			labelMouse.Text = mouse.Position.X.ToString() + "x" + mouse.Position.Y.ToString();

			if (keyboard.KeyStates[KeyboardKeys.A] == KeyState.Down)
				labelMouse.Text += "\r\nAAAAAAAAAAAAAAAAAAAAAAAAAAA!!";

			return true;
		}



		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				Gorgon.Initialize(this);
				GorgonPlugInFactory.SearchPaths.Add(@"..\..\..\..\PlugIns\bin\debug");
				GorgonPlugInFactory.LoadPlugInAssembly(@"Gorgon.HID.RawInput.dll");
				GorgonPlugInFactory.LoadPlugInAssembly(@"Gorgon.FileSystem.Zip.dll");
				GorgonPlugInFactory.LoadPlugInAssembly(@"Gorgon.FileSystem.BZ2Packfile.dll");
				input = GorgonHIDFactory.CreateInputDeviceFactory("GorgonLibrary.HID.GorgonRawInput");
				mouse = input.CreatePointingDevice();
				keyboard = input.CreateKeyboard();

				fileSystem = new GorgonFileSystem();
				fileSystem.AddProvider("GorgonLibrary.FileSystem.GorgonZipFileSystemProvider");
				fileSystem.AddProvider("GorgonLibrary.FileSystem.GorgonBZ2FileSystemProvider");
				fileSystem.Mount(System.IO.Path.GetPathRoot(Application.ExecutablePath) + @"unpak\", "/FS");
				fileSystem.Mount(System.IO.Path.GetPathRoot(Application.ExecutablePath) + @"unpak\ParilTest.zip", "/Zip");
				fileSystem.Mount(System.IO.Path.GetPathRoot(Application.ExecutablePath) + @"unpak\BZipFileSystem.gorPack");

				System.IO.Stream stream = fileSystem.GetFile("/Shaders/Blur.fx").OpenStream(false);
				byte[] streamFile = new byte[stream.Length];
				stream.Read(streamFile, 0, (int)stream.Length);
				byte[] file = fileSystem.GetFile("/Shaders/Cloak.fx").Read();

				//joystick = input.CreateCustomHID(input.CustomHIDs[5].Name);
				//joystick.DataChanged += new EventHandler<GorgonCustomHIDDataChangedEventArgs>(joystick_DataChanged);
				
				Gorgon.Go(Idle);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
				Close();
			}
		}

		/// <summary>
		/// Joystick_s the data changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		void joystick_DataChanged(object sender, GorgonCustomHIDDataChangedEventArgs e)
		{
			byte[] data = e.GetData<byte[]>();

			labelMouse.Text += "\r\n| ";
			for (int i = 0; i < data.Length; i++)
				labelMouse.Text += "0x" + GorgonUtility.FormatHex(data[i]) + " | ";
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (input != null)
				input.Dispose();
			Gorgon.Terminate();
		}

		public Form1()
		{
			InitializeComponent();
		}
	}
}
