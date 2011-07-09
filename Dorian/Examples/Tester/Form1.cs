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
				byte[] keyCheck = {   0,  36,   0,   0,   4, 128,   0,   0, 148,   0,   0,   0,   6,   2,   0,   0,   0,  36,   0,   0,  82,  83,  65,  49,   0,   4,   0,   0,   1,   0,   1,   0,  73,  50,   7, 196, 181, 135,  85, 211,  46, 233, 245,   4, 114, 109, 101, 139,  19, 173,  93,  13, 179, 101, 174, 156, 139, 185, 147, 140,  15, 101,  39, 154,  97, 204,  34, 106, 104,  31, 122,  91,  81,  60,  12, 192, 116, 183, 165,  24, 168,  79, 200, 249,  72,  65,  21,  30,  45, 194,  28, 118, 148,  50, 196, 194, 195, 161,  44,  55,  47,  40, 161, 218, 138, 207,  25,  47,  39, 211,  75, 218, 201, 202, 210, 101, 138,  20,  23, 176, 217, 155, 138, 123,  75, 213,  47, 222,  99, 226, 139, 169, 248, 193, 116,   7, 219, 229,  61,  23,  40, 108,  68, 152, 179, 127,  82, 246, 177, 143,   1, 130,  76, 144, 168, 198, 219, 228, 128, 165};
				Gorgon.Initialize(this);
				GorgonPlugInFactory.SearchPaths.Add(@"..\..\..\..\PlugIns\bin\debug");
				GorgonDialogs.InfoBox(GorgonPlugInFactory.IsPlugInSigned(@"Gorgon.HID.RawInput.dll", keyCheck).ToString());
					
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
