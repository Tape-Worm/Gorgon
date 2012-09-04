using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using SlimMath;
using GorgonLibrary;
using GorgonLibrary.Math;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace Test_TextureArray
{
	static class Program
	{
		private static GorgonTexture2D _tex = null;
		private static GorgonTexture2D _noDecal = null;
		private static Form1 _form = null;
		private static GorgonGraphics _graphics = null;
		private static Gorgon2D _2D = null;
		private static GorgonSwapChain _swap = null;
		private static GorgonSprite _sprite = null;
		private static GorgonText _text = null;
		private static GorgonPixelShader _shader = null;
		private static float _startTime = 0;
		private static bool _showDecal = true;
		private static float _angle = 0.0f;

		private static bool Idle()
		{
			_2D.Clear(Color.Blue);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Time since startup: " + GorgonTiming.SecondsSinceStart.ToString("0.0") + " seconds", new Vector2(0, 0), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "FPS: " + GorgonTiming.FPS.ToString("0.0"), new Vector2(0, 16), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Frame Delta: " + (GorgonTiming.Delta * 1000.0f).ToString("0.0##") + " ms", new Vector2(0, 32), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Avg. FPS: " + GorgonTiming.AverageFPS.ToString("0.0"), new Vector2(0, 48), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Avg. Frame Delta: " + (GorgonTiming.AverageDelta * 1000.0f).ToString("0.0##") + " ms", new Vector2(0, 64), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Frame Count: " + GorgonTiming.FrameCount.ToString("0"), new Vector2(0, 80), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Highest FPS: " + GorgonTiming.HighestFPS.ToString("0.0"), new Vector2(0, 96), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Lowest FPS: " + GorgonTiming.LowestFPS.ToString("0.0"), new Vector2(0, 112), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "VB Count: " + GorgonRenderStatistics.VertexBufferCount.ToString() + " (" + GorgonRenderStatistics.VertexBufferSize.FormatMemory() + ")", new Vector2(0, 144), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "IB Count: " + GorgonRenderStatistics.IndexBufferCount.ToString() + " (" + GorgonRenderStatistics.IndexBufferSize.FormatMemory() + ")", new Vector2(0, 160), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "CB Count: " + GorgonRenderStatistics.ConstantBufferCount.ToString() + " (" + GorgonRenderStatistics.ConstantBufferSize.FormatMemory() + ")", new Vector2(0, 176), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Texture Count: " + GorgonRenderStatistics.TextureCount.ToString() + " (" + GorgonRenderStatistics.TextureSize.FormatMemory() + ")", new Vector2(0, 192), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "RT Count: " + GorgonRenderStatistics.RenderTargetCount.ToString() + " (" + GorgonRenderStatistics.RenderTargetSize.FormatMemory() + ")", new Vector2(0, 208), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Depth Count: " + GorgonRenderStatistics.DepthBufferCount.ToString() + " (" + GorgonRenderStatistics.DepthBufferSize.FormatMemory() + ")", new Vector2(0, 224), Color.White);

			float time = ((GorgonTiming.SecondsSinceStart - _startTime) / 45.0f).Min(1);
			_angle = 360.0f * time;

			if (_showDecal)
				_2D.PixelShader.Current = _shader;
			_sprite.Angle = _angle;
			_sprite.Position = new Vector2(_swap.Settings.Width / 2.0f, _swap.Settings.Height / 2.0f);
			_sprite.Draw();
			if (_showDecal)
				_2D.PixelShader.Current = null;

			/*_2D.Drawing.DrawRectangle(_sprite.Collider.ColliderBoundaries, Color.Green);
			_2D.Drawing.DrawEllipse(_sprite.Collider.ColliderBoundaries, Color.Red);*/

			//_2D.Render(false);

			_text.Position = new Vector2(320, 240);
			_text.Draw();
			_2D.Drawing.DrawRectangle(_text.Collider.ColliderBoundaries, Color.Cyan);


			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Draw calls: " + GorgonRenderStatistics.DrawCallCount.ToString(), new Vector2(0, 128), Color.White);
			GorgonRenderStatistics.EndFrame();
			_2D.Render();

			if (time >= 1.0f)
				_startTime = GorgonTiming.SecondsSinceStart;

			return true;
		}

		private static void Initialize()
		{
			_form = new Form1();
			_form.Show();
			_form.FormClosing += new FormClosingEventHandler(_form_FormClosing);
			_form.KeyDown += new KeyEventHandler(_form_KeyDown);

			_graphics = new GorgonGraphics();
			_swap = _graphics.Output.CreateSwapChain("Screen", new GorgonSwapChainSettings()
			{
				Window = _form,
				Width = 1280,
				Height = 800,
				IsWindowed = true
			});
			_2D = _graphics.Output.Create2DRenderer(_swap);
			//_2D.IsLogoVisible = true;
			_startTime = GorgonTiming.SecondsSinceStart;

			_shader = _graphics.Shaders.CreateShader<GorgonPixelShader>("MyShader", "DualTex", Properties.Resources.Shader, true);

			_noDecal = _graphics.Textures.FromFile<GorgonTexture2D>("Image", @"..\..\..\..\Resources\Images\Ship.png",
				new GorgonTexture2DSettings()
				{
					FileFilter = ImageFilters.Point,
					FileMipFilter = ImageFilters.Point
				});

			_tex = _graphics.Textures.CreateTexture<GorgonTexture2D>("Texture", new GorgonTexture2DSettings()
			{
				Width = _noDecal.Settings.Width,
				Height = _noDecal.Settings.Height,
				Format = BufferFormat.R8G8B8A8_UIntNormal,
				Usage = BufferUsage.Default,
				ArrayCount = 2,
				MipCount = 1
			});
			_tex.CopySubResource(_noDecal, 0, 1);


			GorgonTexture2D image = _graphics.Textures.FromFile<GorgonTexture2D>("Image2", @"..\..\..\..\Resources\Images\Ship_Decal.png",
				new GorgonTexture2DSettings()
				{
					FileFilter = ImageFilters.Point,
					FileMipFilter = ImageFilters.Point,
					Usage = BufferUsage.Staging,
					ArrayCount = 1,
					MipCount = 1
				});

			_tex.CopySubResource(image, 0, 0);

			image.Dispose();

			_sprite = _2D.Renderables.CreateSprite("Test", _tex.Settings.Size, _tex);
			//_sprite.Collider = new Gorgon2DAABB();
			//((Gorgon2DAABB)_sprite.Collider).Location = new Vector2(_tex.Settings.Width / 4.0f, _tex.Settings.Height / 4.0f);
			//((Gorgon2DAABB)_sprite.Collider).Size = new Vector2(0.5f, 0.5f);
			//_sprite.Collider = new Gorgon2DBoundingCircle();
			//((Gorgon2DBoundingCircle)_sprite.Collider).Radius = 0.75f;

			_sprite.Anchor = new Vector2(_sprite.Size.X / 2.0f, _sprite.Size.Y / 2.0f);
			GorgonTiming.TimeScale = 0.1f;

			_text = _2D.Renderables.CreateText("Text", _graphics.Fonts.DefaultFont, "This is a line of text.");
			_text.Collider = new Gorgon2DAABB();
			_text.Angle = 45.0f;
		}

		static void _form_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.S)
			{
				_showDecal = !_showDecal;

				if (_showDecal)
					_sprite.Texture = _tex;
				else
					_sprite.Texture = _noDecal;
			}
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
				Initialize();
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