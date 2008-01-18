#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Friday, July 08, 2005 3:51:24 PM
// 
#endregion

using System;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.IO.Compression;
using SharpUtilities;
using SharpUtilities.Native.Win32;
using SharpUtilities.Utility;
using SharpUtilities.Mathematics;
using SharpUtilities.IO;
using GorgonLibrary;
using GorgonLibrary.FileSystems;
using GorgonLibrary.Graphics;
using GorgonLibrary.Framework;
using GorgonLibrary.Timing;
using GorgonLibrary.InputDevices;
//using GorgonLibrary.GorgonGUI;

namespace Tester2
{
	/// <summary>
	/// Summary description for MainClass.
	/// </summary>
	public class MainClass : IDisposable
	{
        Sprite _sprite = null;
		Sprite _sprite2 = null;
        Sprite _back = null;
		Random _rnd = new Random();
        TestForm _form = null;
/*		GUI _gui = null;
		GUIWindow _window = null;
		GUIWindow _window2 = null;
		GUILabel _label1 = null;
		GUIButton _button = null;*/
//		Sprite _spriteBack = null;
		FileSystem zlibFS = null;
		Sprite _paint = null;
		StaticTextSprite staticText = null;
		TextSprite textInfo = null;
		Vector2D mouse = Vector2D.Zero;
		Font defaultFont = null;

		public void Initialize()
		{		
/*            Gorgon.Images.FromFile(_form.ResourcePath + "UnhappyFace.png");
            Gorgon.Images.FromFile(_form.ResourcePath + "DefaultGUI.png");

            _cursor = Gorgon.Sprites.Create("@Cursor", Gorgon.Images["DefaultGUI"], 29, 19, 16, 26);
            _sprite = Gorgon.Sprites.Create("Unhappy Bitch", Gorgon.Images["UnhappyFace"], 158, 7, 59, 62, 59.0f / 2.0f, 62.0f / 2.0f, 0.0f, 0, 0, 1.0f, 1.0f);
            _sprite.SetPosition(48, 48);

            _cursor.Animations.Create("Animation", 3000.0f);
            _cursor.Animations["Animation"].Looped = true;
            _cursor.Animations["Animation"].TransformTracks.Create("MoveTrack");
            _cursor.Animations["Animation"].TransformTracks["MoveTrack"].CreateKey(0.0f, new Vector2D(_form.Gorgon.Screen.Width / 2.0f, _form.Gorgon.Screen.Height / 2.0f) , Vector2D.Unit, 0.0f);
            _cursor.Animations["Animation"].TransformTracks["MoveTrack"].CreateKey(2000.0f, Vector2D.Zero, Vector2D.Unit, 0.0f);
            _cursor.Animations["Animation"].TransformTracks["MoveTrack"].CreateKey(3000.0f, new Vector2D(_form.Gorgon.Screen.Width / 2.0f, _form.Gorgon.Screen.Height / 2.0f), Vector2D.Unit, 0.0f);
            _cursor.Animations["Animation"].ColorTracks.Create("ColorTrack");
            _cursor.Animations["Animation"].ColorTracks["ColorTrack"].CreateKey(0.0f, Drawing.Color.White);
            _cursor.Animations["Animation"].ColorTracks["ColorTrack"].CreateKey(1500.0f, Drawing.Color.Red);
            _cursor.Animations["Animation"].ColorTracks["ColorTrack"].CreateKey(3000.0f, Drawing.Color.White);
            _cursor.Animations["Animation"].FrameTracks.Create("FrameTrack");
            _cursor.Animations["Animation"].FrameTracks["FrameTrack"].CreateKey(0.0f, _cursor.Image, new Vector2D(29,19), new Vector2D(16, 26));
            _cursor.Animations["Animation"].FrameTracks["FrameTrack"].CreateKey(1500.0f, _sprite.Image, Vector2D.Zero, new Vector2D(123, 113));            

            _cursor.Children.Add(_sprite);

            _sprite.Save(@"D:\unpak\ball.gorSprite");*/
			Gorgon.Screen.BackgroundColor = Drawing.Color.Blue;
			Image.FromFile(_form.ResourcePath + @"\shuttle.png");

			_back = new Sprite("CrappyBack", ImageCache.Images["shuttle"], new Vector2D(96.0f, 1.0f), new Vector2D(800, 600));
			_sprite = new Sprite("Shuttle", ImageCache.Images["shuttle"], new Vector2D(2, 1), new Vector2D(86, 205));
			_sprite.Axis = new Vector2D(_sprite.Width / 2, _sprite.Height / 2);
			_sprite.SetPosition(((float)Gorgon.Screen.Width / 2.0f), (float)Gorgon.Screen.Height - (_sprite.Height / 2.0f));

			_sprite.Animations.Create("LiftOff", 10000.0f);
			_sprite.Animations["LiftOff"].TransformationTrack.CreateKey(0, _sprite.Position, Vector2D.Unit, 0);
			_sprite.Animations["LiftOff"].TransformationTrack.CreateKey(3000.0f, new Vector2D(_sprite.Position.X, ((float)Gorgon.Screen.Height - 150.0f)), Vector2D.Unit, 0);
			_sprite.Animations["LiftOff"].TransformationTrack.CreateKey(5000.0f, new Vector2D(_sprite.Position.X, ((float)Gorgon.Screen.Height - 200.0f)), Vector2D.Unit, 0);
			_sprite.Animations["LiftOff"].TransformationTrack.CreateKey(8500.0f, new Vector2D(_sprite.Position.X + 20, ((float)Gorgon.Screen.Height / 2.0f) - 200.0f), Vector2D.Unit, 10);
			_sprite.Animations["LiftOff"].TransformationTrack.CreateKey(10000.0f, new Vector2D(_sprite.Position.X + 150, -_sprite.Height), Vector2D.Unit, 25);
			_sprite.Animations["LiftOff"].Looped = true;

/*			Shader shaderTest = Shader.FromFile(@"D:\code\current\GorgonLib\Gorgon\Resources\Blur.fx");
			shaderTest.Save(@"D:\shader.fxb", true);
			shaderTest.Dispose();
			shaderTest = Shader.FromFile(@"D:\shader.fxb", true);

			_sprite.Shader = shaderTest;
			_sprite.Shader.Parameters["sourceImage"].SetValue(_sprite.Image);
			_sprite.Shader.Parameters["blurAmount"].SetValue(0.00123f);

			_sprite.Image.Save(@"D:\imageTest.png", ImageFileFormat.PNG);

			_sprite.Save(@"d:\shuttle.xml", true);
			_sprite = Sprite.FromFile(@"d:\shuttle.xml");

/*			_back = new Sprite("Shuttle", Gorgon.Images["shuttle"], new Vector2D(2, 1), new Vector2D(86, 205));
			_back.SetPosition(((float)Gorgon.Screen.Width / 2.0f), (float)Gorgon.Screen.Height - (_back.Height / 2.0f));

			_gui = new GUI();
			_gui.SkinFromFile(@"D:\code\Current\gorgonlib\Resources\GUI Skins\defaultskin.xml");

			_gui.Background = new Sprite("GUIBackground", (Image)null, new Vector2D(32,32));			
			_gui.Background.Color = Drawing.Color.FromArgb(192, 128, 64);
			_gui.Background.Opacity = 129;
			_gui.Background.Width = _form.Gorgon.Screen.Width;
			_gui.Background.Height = _form.Gorgon.Screen.Height;

			_window = _gui.CreateWindow("MyWindow", string.Empty, new Vector2D(100, 100), new Vector2D(256, 256));
			_window2 = _gui.CreateWindow("MyWindow2", "Test the windows 2", new Vector2D(300, 200), new Vector2D(256, 256));
			_label1 = new GUILabel(_gui, "MyLabel", "Test this text!");
			_button = new GUIButton(_gui, "MyButton", "Button testing 1 2 3 ABCDEFGHIJKLMN@OPQ!");
			_button.SetPosition(180.0f, 80);
			_button.Width = 128;
			_button.Height = 32;
			_button.Clicked += new EventHandler(_button_Clicked);
			_window2.Controls.Add(_label1);
			_window2.Controls.Add(_button);
			//_label1.AutoSize = true;
			_label1.Alignment = Alignment.Center;
			_label1.Width = _window2.ClientSize.X;
			_label1.Height = 64;
			_label1.ForeColor = Drawing.Color.Black;

			_button.MouseMove += new MouseInputEvent(_button_MouseMove);
			/*_label1.Border = true;
			_label1.BorderColor = Drawing.Color.Yellow;
			_label1.BackColor = Drawing.Color.Blue;
			_label1.Alignment = Alignment.Center;
			_label1.BackgroundImage = _spriteBack;
			_label1.ScaleBackgroundImage = true;*/

						
			//_sprite = Gorgon.RootLayer.SpriteFromFile(@"D:\unpak\ball.gorSprite");
            //_cursor = Gorgon.RootLayer.SpriteFromFile(@"D:\unpak\cursor.gorSprite");
            //_cursor.SetPosition(_form.Gorgon.Screen.Width / 2.0f, _form.Gorgon.Screen.Height / 2.0f);
            //_cursor.Animations["Animation"].IsStopped = false;			
			//zlibFS = Gorgon.FileSystems["ZLibFile"];

			// Folder system.
			//FontManager fontMgr = new FontManager();
			//Font test = null;
			//test = fontMgr.FromFile(_form.ResourcePath + @"\arial_9pt.gorFont");
			//Gorgon.Shaders.FromFile(_form.ResourcePath + @"\postprocess.fx");
			//_sprite.Shader = Gorgon.Shaders["postprocess"];
			//string textFile = File.ReadAllText(@"D:\code\current\gorgonlib\Gorgon\Device\RenderWindow.cs");

			//defaultFS = new FolderFileSystem("Default");

			//test.FontImage.Save(defaultFS, @"Fonts\" + test.FontImage.Name + ".png");
			//Gorgon.Shaders["postprocess"].Save(defaultFS, @"\Shaders\PostProcess.fx");
			//Gorgon.ImageManager["Shuttle"].Save(defaultFS, @"Shuttle.png");
			//test.Save(defaultFS, @"Fonts\Arial_9pt.gorFont");
			//_sprite.Save(defaultFS, @"Sprites\Shuttle.gorSprite");			
			//_back.Save(defaultFS, @"Sprites\Back.gorSprite");
			//defaultFS.CreatePath("/Fonts/DummyPath");
			//defaultFS.WriteFile("MyTestText.txt", Encoding.UTF8.GetBytes("This is a test of a text file.\nFun!\nMore fun!\n" + textFile));
			//defaultFS.Save(@"D:\unpak\FileSystemTest");

			//defaultFS.Unmount(@"/sprites");

			// ZLib system.
			/*zlibFS.Add<Image>(@"Shuttle.png", Gorgon.ImageManager["Shuttle"]);
			zlibFS.Add<Image>(@"Fonts/" + test.FontImage.Name + ".png", test.FontImage);
			zlibFS.Add<Shader>(@"/Shaders/PostProcess.fx", Gorgon.Shaders["postprocess"]);
			zlibFS.Add<Font>(@"Fonts/Arial_9pt.gorFont", test);
			zlibFS.Add<Sprite>(@"Sprites/Shuttle.gorSprite", _sprite);
			zlibFS.Add<Sprite>(@"Sprites/Back.gorSprite", _back);
			zlibFS.Add("MyTestText.txt", "This is a test of a text file.\nFun!\nMore fun!\n" + textFile);

			// Remove all entries.			
			Gorgon.Shaders.Remove("postprocess");
			_form.Gfx.ImageManager.Remove("Shuttle");
			_form.Gfx.ImageManager.Remove(test.FontImage.Name);
			test = null;
			
			// Load from zlib fs.
			zlibFS.RootPath = @"D:\unpak\ZlibFS.gorPack";
			zlibFS.Mount();
			//_sprite = zlibFS.LoadObject<Sprite>(@"Sprites/Shuttle.gorSprite");
			_back = zlibFS.LoadObject<Sprite>(@"Sprites/Back.gorSprite");
			test = zlibFS.LoadObject<Font>(@"Fonts/Arial_9pt.gorFont");

/*			_sprite.Shader.ActiveTechnique.Parameters["sourceImage"].SetValue<Image>(_sprite.Image);
			_sprite.Shader.ActiveTechnique.Parameters["blurAmount"].SetValue<float>(0.00252f);

			// Load from file FS.			
			defaultFS.Root = @"D:\unpak\FileSystemTest";
			defaultFS.Mount("/Fonts");
			//defaultFS.MoveEntry(defaultFS["MyTestText.txt"], @"/Sprites");

			//defaultFS.MovePath(@"Sprites\", @"\");
			//defaultFS.Save(@"D:\unpak\FileSystemTest");

/*			_sprite = defaultFS.LoadObject<Sprite>(@"Sprites\Shuttle.gorSprite");
			_sprite.Shader.ActiveTechnique.Parameters["sourceImage"].SetValue<Image>(_sprite.Image);
			_sprite.Shader.ActiveTechnique.Parameters["blurAmount"].SetValue<float>(0.00252f);
			_back = defaultFS.LoadObject<Sprite>(@"Sprites\Back.gorSprite");
			test = defaultFS.LoadObject<Font>(@"Fonts\Arial_9pt.gorFont");*/			
			//zlibFS.CopyFileSystem(defaultFS);

/*			FileSystem.Create("FILESYSTEM!!!", "ZLib File System");

			zlibFS = _form.FileSystems["ZLibFile"];
			zlibFS.Root = _form.ResourcePath +  @"tester2.gorPack";
			zlibFS.Mount("/", false);
			zlibFS.Mount("/Sprites");
			zlibFS.Mount("/Fonts");
			zlibFS.Mount("/Shaders");

			_sprite = Sprite.FromFileSystem(zlibFS, @"Sprites/Shuttle.gorSprite");
			//_sprite.Animations[0].Enabled = false;
			_sprite2 = Sprite.FromFileSystem(zlibFS, @"Sprites/Shuttle.gorSprite");
			//_sprite2.Animations[0].Enabled = false;
			_back = Sprite.FromFileSystem(zlibFS, @"Sprites/Back.gorSprite");

			_sprite.Children.Add(_sprite2, new Vector2D(64,64));
			_sprite2.Shader = null;
			_sprite2.InheritScale = false;
			_sprite.UniformScale = 2.0f;

			//_sprite.Shader.Parameters["sourceImage"].SetValue(_sprite.Image);
			//_sprite2.InheritRotation = false;*/
			
/*			_sprite.Shader.Save(@"D:\unpak\Shader.fxb", true);
			Gorgon.Shaders.Remove(_sprite.Shader.Name);
			_sprite.Shader = null;*/
			//_sprite.Shader.ActiveTechnique.Parameters["blurAmount"].SetValue<float>(0.00152f);


/*			Gorgon.Shaders.FromFile(@"D:\unpak\Shader.fxb");
			//_sprite.Smoothing = Smoothing.Smooth;
			_sprite.Shader = Gorgon.Shaders["Shader"];
			_sprite.Shader.Parameters["sourceImage"].SetValue<Image>(_sprite.Image);
			_sprite.Shader.ActiveTechnique = _sprite.Shader.Techniques["Sharpen"];
			_sprite.Save(zlibFS, @"Sprites\Shuttle.gorSprite");
			_sprite.Shader.Save(zlibFS, @"Shaders\Shader.fxb");
			zlibFS.Save(@"d:\unpak\tester2.gorPack");*/

			//_paint = Sprite.FromFile(_form.ResourcePath + @"\Paint.gorSprite");
			//_sprite.Save(@"D:\shuttle.xml", true);

			defaultFont = new Font("Arial", "Arial", 9.0f, true, true);
			defaultFont.MaxFontImageWidth = 512;
			defaultFont.MaxFontImageHeight = 256;
//			defaultFont.OutlineWidth = 1.25f;

			staticText = new StaticTextSprite("Test", System.IO.File.ReadAllText(@"D:\Code\Archived\Other\Docs\LZWEXP.TXT"), defaultFont, Drawing.Color.Yellow);
			staticText.AutoAdjustCRLF = true;
			staticText.Bounds = new Viewport(0, 0, 320, 200);
			staticText.Smoothing = Smoothing.None;
//			staticText.Alignment = Alignment.UpperCenter;
			staticText.UniformScale = 1.0f;
			staticText.Initialize();
			staticText.Axis = new Vector2D((staticText.Width / staticText.Scale.X) / 2.0f, (staticText.Height / staticText.Scale.Y) / 2.0f);
			staticText.SetCharacterVertexColor(VertexLocations.LowerLeft, Drawing.Color.Black);
			staticText.SetCharacterVertexColor(VertexLocations.LowerRight, Drawing.Color.Black);

			textInfo = new TextSprite("Info", string.Empty, defaultFont);
			//textInfo.Text = "Hello, world! --- fff";
			textInfo.Text = "This is a Test of a failing fucking SysTem.\nABCDEFGHIJKLMNOPQRSTUVWXYZ 1234567890\nabcdefghijklmnopqrstuvwxyz !@#$%^&*()-_=+\n<>?,./:\";'{}[]|\\";
			//textInfo.Text = "1234567890\n!@#$%^&*()\n_+-={}[]|\\:;\"'<>,.?/\nabcdefghijklmnopqrstuvwxyz\nABCDEFGHIJKLMNOPQRSTUVWXYZ";

//			Gorgon.FrameStatsVisible = true;
//			Gorgon.InvertFrameStatsTextColor = false;
		}

		void _button_Clicked(object sender, EventArgs e)
		{
			//((GUIButton)sender).Caption = "Fuck!";
		}

		void _button_MouseMove(object sender, MouseInputEventArgs e)
		{
			
		}

		void Keyboard_KeyUp(object sender, KeyboardInputEventArgs e)
		{
		}

		void Keyboard_KeyDown(object sender, KeyboardInputEventArgs e)
		{
		}

		void Mouse_MouseUp(object sender, MouseInputEventArgs e)
		{
		}

		void Mouse_MouseDown(object sender, MouseInputEventArgs e)
		{
		}

		void Mouse_MouseMove(object sender, MouseInputEventArgs e)
		{
			
		}

		public void MouseMove(Forms.MouseEventArgs e)
		{
			mouse = new Vector2D(e.X, e.Y);
		}

		public void DeviceReset()
		{
		}

		float maxTime = 10.0f;
		public void Render(FrameEventArgs e)
		{
			float remainder = maxTime - e.FrameDeltaTime;

			//_gui.Draw();

			//_sprite.Rotation += (4.0f * (remainder / 1000.0f));
			//_sprite2.Rotation -= (1.0f * (remainder / 1000.0f));
			staticText.Position = mouse;
			_sprite.Animations["LiftOff"].Advance(e.FrameDeltaTime * 1000.0f);

			_back.Draw();

			//staticText.UpdateAABB();
			//textInfo.Text = staticText.AABB.ToString();
						
			textInfo.SetPosition(10.0f, 10.0f);
			
			Gorgon.Screen.BeginDrawing();			
			Gorgon.Screen.Rectangle(staticText.AABB.X, staticText.AABB.Y, staticText.AABB.Width, staticText.AABB.Height, Drawing.Color.Red);
			Gorgon.Screen.EndDrawing();

			// Render away.
			//_paint.SetPosition(_rnd.Next(400), _rnd.Next(400));
			_sprite.Draw();

			staticText.Rotation += 11.0f * e.FrameDeltaTime;
			staticText.UpdateAABB();

			staticText.Draw();			
			textInfo.Draw();
			//_paint.Draw();
		}

		public MainClass(TestForm form)
		{
			_form = form;
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~MainClass()
		{
			Dispose(false);
		}

		#region IDisposable Members
		/// <summary>
		/// Function to do clean up.
		/// </summary>
		/// <param name="disposing">TRUE to remove all resources, FALSE to only remove unmanaged.</param>
		protected void Dispose(bool disposing)
		{
			if (disposing)
			{
			}			
		}

		/// <summary>
		/// Function to do clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);			
		}
		#endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
			Forms.Application.EnableVisualStyles();
			Forms.Application.DoEvents();
			Forms.Application.Run(new TestForm()); 				
        }
	}
}


