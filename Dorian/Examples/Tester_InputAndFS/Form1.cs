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
using GorgonLibrary.Input;
using GorgonLibrary.FileSystem;

namespace Tester
{
	public partial class Form1 : Form
	{
		GorgonInputDeviceFactory input = null;
		GorgonInputDeviceFactory winput = null;
		GorgonInputDeviceFactory xinput = null;
		GorgonPointingDevice mouse = null;
		GorgonKeyboard keyboard = null;
		GorgonFileSystem fileSystem = null;
		GorgonJoystick joystick = null;
		GorgonTimer pulseTimer = null;
		bool pulse = false;
		Random rnd = new Random();
		string mouseInfo = string.Empty;

		private bool Idle(GorgonFrameRate timing)
		{
			labelMouse.Text = mouseInfo;

			if (joystick != null)
			{
				joystick.Poll();

				labelMouse.Text += string.Format("Left Stick: {0}x{1} ({8})\nRight stick:{2}x{3} ({9})\nRudder:{4}\nThrottle:{5}\nPOV: {6}\nPOV Direction: {7}\n", 
						joystick.X, joystick.Y, joystick.SecondaryX, joystick.SecondaryY, joystick.Rudder, joystick.Throttle, joystick.POV, joystick.Direction.POV, joystick.Direction.X|joystick.Direction.Y, joystick.Direction.SecondaryX|joystick.Direction.SecondaryY);

				for (int i = 0; i < joystick.Capabilities.ButtonCount; i++)
					labelMouse.Text += "Button :" + joystick.Button[i].Name + " " + joystick.Button[i].IsPressed.ToString() + "\n";

				if (((joystick.Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsVibration) == JoystickCapabilityFlags.SupportsVibration) && (joystick.Button["A"].IsPressed))
				{
					pulseTimer = new GorgonTimer();
				}

				if (((joystick.Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsVibration) == JoystickCapabilityFlags.SupportsVibration) && (pulseTimer == null))
				{
					float normalize1 = (float)joystick.Rudder;
					float normalize2 = (float)joystick.Throttle;

					normalize1 = (float)Math.Pow(normalize1 + 1.0f, 2);
					normalize2 = (float)Math.Pow(normalize2 + 1.0f, 2);

					if (normalize1 > 65535.0f)
						normalize1 = 65535.0f;
					if (normalize2 > 65535.0f)
						normalize2 = 65535.0f;

					if (joystick.Rudder > 0)
						joystick.Vibrate(0, (int)normalize1);
					else
						joystick.Vibrate(0, 0);

					if (joystick.Throttle > 0)
						joystick.Vibrate(1, (int)normalize2);
					else
						joystick.Vibrate(1, 0);
				}

				if (((joystick.Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsVibration) == JoystickCapabilityFlags.SupportsVibration) && (joystick.Button["B"].IsPressed))
				{
					joystick.Vibrate(0, 0);
					joystick.Vibrate(1, 0);
					pulseTimer = null;
				}

				if (pulseTimer != null)
				{
					if (pulseTimer.Milliseconds > 1000)
					{
						pulse = !pulse;
						pulseTimer.Reset();
					}

					if (pulse)
					{
						joystick.Vibrate(0, rnd.Next(joystick.Capabilities.VibrationMotorRanges[0].Minimum, joystick.Capabilities.VibrationMotorRanges[0].Maximum));
						joystick.Vibrate(1, 0);
					}
					else
					{
						joystick.Vibrate(0, 0);
						joystick.Vibrate(1, rnd.Next(joystick.Capabilities.VibrationMotorRanges[0].Minimum, joystick.Capabilities.VibrationMotorRanges[0].Maximum));
					}
				}

			}

			return true;
		}

		private void CreateJoysticks()
		{
			xinput = GorgonInputDeviceFactory.CreateInputDeviceFactory("GorgonLibrary.Input.GorgonXInputPlugIn");

			foreach (GorgonInputDeviceInfo name in xinput.JoystickDevices)
			{
				if (name.IsConnected)
				{
					joystick = xinput.CreateJoystick(name, this.panel1);
					break;
				}
			}

			if (input != null)
			{
				if (joystick == null)
				{
					foreach (GorgonInputDeviceInfo name in input.JoystickDevices)
					{
						if (name.IsConnected)
						{
							joystick = input.CreateJoystick(name, this.panel1);
							break;
						}
					}
				}
			}


			if (joystick != null)
			{
				joystick.DeadZone.X = new GorgonLibrary.Math.GorgonMinMax(-2500, 2500);
				joystick.DeadZone.Y = new GorgonLibrary.Math.GorgonMinMax(-2500, 2500);
				joystick.DeadZone.SecondaryX = new GorgonLibrary.Math.GorgonMinMax(-2500, 2500);
				joystick.DeadZone.SecondaryY = new GorgonLibrary.Math.GorgonMinMax(-2500, 2500);
			}
		}
		
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try 
			{
				Gorgon.Initialize();
				GorgonPlugInFactory.SearchPaths.Add(@"..\..\..\..\PlugIns\bin\debug");				
				GorgonPlugInFactory.LoadPlugInAssembly(@"Gorgon.Input.Raw.dll");
				GorgonPlugInFactory.LoadPlugInAssembly(@"Gorgon.Input.XInput.dll");
				GorgonPlugInFactory.LoadPlugInAssembly(@"Gorgon.Input.WinForms.dll");
				GorgonPlugInFactory.LoadPlugInAssembly(@"Gorgon.FileSystem.Zip.dll");
				GorgonPlugInFactory.LoadPlugInAssembly(@"Gorgon.FileSystem.BZ2Packfile.dll");
				input = GorgonInputDeviceFactory.CreateInputDeviceFactory("GorgonLibrary.Input.GorgonRawPlugIn");
				winput = GorgonInputDeviceFactory.CreateInputDeviceFactory("GorgonLibrary.Input.GorgonWinFormsPlugIn");
				
				//mouse = input.CreatePointingDevice();
				//keyboard = input.CreateKeyboard();
				
				mouse = winput.CreatePointingDevice(this.panel1);
				mouse.PointingDeviceMove += new EventHandler<PointingDeviceEventArgs>(mouse_MouseMove);
				mouse.PointingDeviceDown += new EventHandler<PointingDeviceEventArgs>(mouse_MouseDown);
				mouse.PointingDeviceUp += new EventHandler<PointingDeviceEventArgs>(mouse_MouseUp);
				mouse.PointingDeviceWheelMove += new EventHandler<PointingDeviceEventArgs>(mouse_MouseWheelMove);
				//panel1.MouseDown += new MouseEventHandler(Form1_MouseDown);
				//panel1.MouseUp += new MouseEventHandler(Form1_MouseUp);
				//panel1.MouseMove += new MouseEventHandler(Form1_MouseMove);
				keyboard = winput.CreateKeyboard(this);
				keyboard.KeyDown += new EventHandler<KeyboardEventArgs>(keyboard_KeyDown);

/*				fileSystem = new GorgonFileSystem();
				fileSystem.AddProvider("GorgonLibrary.FileSystem.GorgonZipFileSystemProvider");
				fileSystem.AddProvider("GorgonLibrary.FileSystem.GorgonBZ2FileSystemProvider");
				fileSystem.Mount(System.IO.Path.GetPathRoot(Application.ExecutablePath) + @"unpak\", "/FS");
				fileSystem.Mount(System.IO.Path.GetPathRoot(Application.ExecutablePath) + @"unpak\ParilTest.zip", "/Zip");
				fileSystem.Mount(System.IO.Path.GetPathRoot(Application.ExecutablePath) + @"unpak\BZipFileSystem.gorPack");

				System.IO.Stream stream = fileSystem.GetFile("/Shaders/Blur.fx").OpenStream(false);
				byte[] streamFile = new byte[stream.Length];
				stream.Read(streamFile, 0, (int)stream.Length);
				byte[] file = fileSystem.GetFile("/Shaders/Cloak.fx").Read();*/

				CreateJoysticks();
				
				Gorgon.Go(Idle);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
				Close();
			}
		}
		
		void mouse_MouseWheelMove(object sender, PointingDeviceEventArgs e)
		{
			mouseInfo = e.Position.X.ToString() + "x" + e.Position.Y.ToString() + "\nWheel: " + e.WheelPosition.ToString() + "\nButton:" + e.Buttons.ToString() + "\n\n";
		}

		void mouse_MouseUp(object sender, PointingDeviceEventArgs e)
		{
			if ((e.Buttons == PointingDeviceButtons.Left) && (e.ShiftButtons == PointingDeviceButtons.Right))			
				mouseInfo = e.Position.X.ToString() + "x" + e.Position.Y.ToString() + "\nWheel: " + e.WheelPosition.ToString() + "\nButton:" + e.Buttons.ToString() + " - UP\n\n";
			if (e.DoubleClick)
			{
				if (xinput != null)
				{
					xinput.Dispose();
					xinput = null;
					joystick = null;
				}
				else
				{
					CreateJoysticks();
				}
			}
		}

		void mouse_MouseDown(object sender, PointingDeviceEventArgs e)
		{
			mouseInfo = e.Position.X.ToString() + "x" + e.Position.Y.ToString() + "\nWheel: " + e.WheelPosition.ToString() + "\nButton:" + e.Buttons.ToString() + " - DOWN\n\n";
		}

		/// <summary>
		/// Handles the MouseMove event of the mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Input.PointingDeviceEventArgs"/> instance containing the event data.</param>
		void mouse_MouseMove(object sender, PointingDeviceEventArgs e)
		{
			mouseInfo = e.Position.X.ToString() + "x" + e.Position.Y.ToString() + "\nWheel: " + e.WheelPosition.ToString() + "\nButton:" + e.Buttons.ToString() + "\n\n";
		}	

		void keyboard_KeyDown(object sender, KeyboardEventArgs e)
		{
			GorgonDialogs.InfoBox(this, e.Key.ToString());
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

/*			if (input != null)
				input.Dispose();
			if (winput != null)
				winput.Dispose();
			if (xinput != null)
				xinput.Dispose();*/
			Gorgon.Terminate();
		}

		public Form1()
		{
			InitializeComponent();
		}
	}
}
