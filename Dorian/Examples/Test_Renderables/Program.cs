using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using SlimMath;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace Test_Renderables
{
	static class Program
	{
		private static Form1 _form = null;
		private static GorgonGraphics _graphics = null;
		private static Gorgon2D _2D = null;
		private static GorgonTexture2D _texture = null;
		private static GorgonLine _line = null;
		private static GorgonRectangle _rect = null;
		private static GorgonEllipse _ellipse = null;
		private static float _angle = 0;

		static bool Idle()
		{
			_2D.Clear(new GorgonColor(0, 0, 1.0f));

			_line.StartPoint = new Vector2(185, 240);
			_line.EndPoint = new Vector2(185, 50);
			_line.Draw();
			_line.EndPoint = new Vector2(50, 145);
			_line.StartPoint = new Vector2(320, 145);
			_line.Draw();

			//_line.Angle = _angle;

			_line.StartPoint = new Vector2(320, 240);
			_line.EndPoint = new Vector2(50, 50);
			_line.Draw();

			_rect.Draw();

			_ellipse.Size = new Vector2(100, 100);
			_ellipse.Anchor = new Vector2(50, 50);
			_ellipse.Draw();
			_ellipse.Size = new Vector2(80, 80);
			_ellipse.Anchor = new Vector2(40, 40);
			_ellipse.Draw();

			_2D.Drawing.DrawPoint(new Vector2(160, 120), Color.Cyan);

			_angle += 3.0f * GorgonLibrary.Diagnostics.GorgonTiming.ScaledDelta;

			_2D.Render(1);
			return true;
		}

		static void Init()
		{
			_form = new Form1();
			_form.Show();
			_form.ClientSize = new Size(1280, 800);
			_form.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width / 2 - 640, Screen.PrimaryScreen.WorkingArea.Height / 2 - 400);
			_form.FormClosing += new FormClosingEventHandler(_form_FormClosing);

			_graphics = new GorgonGraphics();
			_2D = _graphics.Output.Create2DRenderer(_form);

			_texture = _graphics.Textures.FromFile<GorgonTexture2D>("BallTexture", @"..\..\..\..\Resources\Images\BallDemo.png");
			_line = _2D.Renderables.CreateLine("Line", new Vector2(50, 50), new Vector2(320, 240), GorgonColor.White);			
			_line.Texture = _texture;
			_line.TextureStart = new Vector2(0.625f, 0);
			_line.TextureEnd = new Vector2(0.625f, 0.5f);
			_line.LineThickness = new Vector2(8.0f, 8.0f);
			
			_rect = _2D.Renderables.CreateRectangle("TestRect", new RectangleF(400, 300, 320, 240), GorgonColor.White, true);
			_rect.Texture = _texture;
			_rect.TextureRegion = new RectangleF(0.0f, 0.0f, 0.5f, 0.5f);
			_rect.LineThickness = new Vector2(8, 8);

			
			_ellipse = _2D.Renderables.CreateEllipse("TestEllipse", new Vector2(160, 120), new Vector2(100, 100), GorgonColor.White, false);
			_ellipse.Texture = _texture;
			_ellipse.TextureRegion = new RectangleF(0.5f, 0, 0.5f, 0.5f);
			_ellipse.LineThickness = new Vector2(4, 4);
			
		}

		static void _form_FormClosing(object sender, FormClosingEventArgs e)
		{			
			Gorgon.Quit();
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Init();
				Gorgon.Run(Idle);
			}
			finally
			{
				if (_graphics != null)
					_graphics.Dispose();
			}
		}
	}
}
