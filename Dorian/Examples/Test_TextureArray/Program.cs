using System;
using System.Drawing;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.Animation;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using GorgonLibrary.Renderers;
using SlimMath;

namespace Test_TextureArray
{
	static class Program
	{
		private static GorgonTexture2D _tex = null;
		private static GorgonTexture2D _noDecal = null;
		private static GorgonTexture2D _dyn = null;
		private static Form1 _form = null;
		private static GorgonGraphics _graphics = null;
		private static Gorgon2D _2D = null;
		private static GorgonSwapChain _swap = null;
		private static GorgonSprite _sprite = null;
		private static GorgonText _text = null;
		private static GorgonPixelShader _shader = null;
		private static float _startTime = 0;
		private static bool _showDecal = false;
		private static float _angle = 0.0f;
		private static GorgonAnimationController<GorgonSprite> _controller = null;
		private static GorgonAnimationController<GorgonText> _textController = null;
		private static GorgonStructuredBuffer _structBuffer = null;

		private static bool Idle()
		{
			_2D.Clear(Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Time since startup: " + GorgonTiming.SecondsSinceStart.ToString("0.0") + " seconds", new Vector2(0, 0), Color.Black);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "FPS: " + GorgonTiming.FPS.ToString("0.0"), new Vector2(0, 16), Color.Black);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Frame Delta: " + (GorgonTiming.Delta * 1000.0f).ToString("0.0##") + " ms", new Vector2(0, 32), Color.Black);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Avg. FPS: " + GorgonTiming.AverageFPS.ToString("0.0"), new Vector2(0, 48), Color.Black);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Avg. Frame Delta: " + (GorgonTiming.AverageDelta * 1000.0f).ToString("0.0##") + " ms", new Vector2(0, 64), Color.Black);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Frame Count: " + GorgonTiming.FrameCount.ToString("0"), new Vector2(0, 80), Color.Black);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Highest FPS: " + GorgonTiming.HighestFPS.ToString("0.0"), new Vector2(0, 96), Color.Black);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Lowest FPS: " + GorgonTiming.LowestFPS.ToString("0.0"), new Vector2(0, 112), Color.Black);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "VB Count: " + GorgonRenderStatistics.VertexBufferCount.ToString() + " (" + GorgonRenderStatistics.VertexBufferSize.FormatMemory() + ")", new Vector2(0, 144), Color.Black);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "IB Count: " + GorgonRenderStatistics.IndexBufferCount.ToString() + " (" + GorgonRenderStatistics.IndexBufferSize.FormatMemory() + ")", new Vector2(0, 160), Color.Black);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "CB Count: " + GorgonRenderStatistics.ConstantBufferCount.ToString() + " (" + GorgonRenderStatistics.ConstantBufferSize.FormatMemory() + ")", new Vector2(0, 176), Color.Black);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Texture Count: " + GorgonRenderStatistics.TextureCount.ToString() + " (" + GorgonRenderStatistics.TextureSize.FormatMemory() + ")", new Vector2(0, 192), Color.Black);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "RT Count: " + GorgonRenderStatistics.RenderTargetCount.ToString() + " (" + GorgonRenderStatistics.RenderTargetSize.FormatMemory() + ")", new Vector2(0, 208), Color.Black);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Depth Count: " + GorgonRenderStatistics.DepthBufferCount.ToString() + " (" + GorgonRenderStatistics.DepthBufferSize.FormatMemory() + ")", new Vector2(0, 224), Color.Black);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Sprite: " + _sprite.Position.ToString(), new Vector2(0, 240), Color.Blue);

			float time = ((GorgonTiming.SecondsSinceStart - _startTime) / 45.0f).Min(1);
			_angle = 360.0f * time;
						
			if (_showDecal)
				_2D.PixelShader.Current = _shader;
			_sprite.Angle = _angle;			
			_sprite.Draw();
			if (_showDecal)
				_2D.PixelShader.Current = null;

			/*_2D.Drawing.DrawRectangle(_sprite.Collider.ColliderBoundaries, Color.Green);
			_2D.Drawing.DrawEllipse(_sprite.Collider.ColliderBoundaries, Color.Red);*/

			//_2D.Render(false);

			_text.Color = Color.Black;
			//_text.Angle = _angle;
			//_text.Position = new Vector2(320, 240);
			_text.Draw();
			_2D.Drawing.DrawRectangle(_text.Collider.ColliderBoundaries, Color.Cyan);
			_2D.Drawing.DrawEllipse(_text.Collider.ColliderBoundaries, Color.Red);

			_2D.Drawing.FilledRectangle(new RectangleF(150, 150, 256, 256), GorgonColor.White, _dyn);

			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Draw calls: " + GorgonRenderStatistics.DrawCallCount.ToString(), new Vector2(0, 128), Color.Black);
			GorgonRenderStatistics.EndFrame();
			_2D.Render();

			if (time >= 1.0f)
				_startTime = GorgonTiming.SecondsSinceStart;

			_controller.Update();
			_textController.Update();
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

			_noDecal = _graphics.Textures.FromFile<GorgonTexture2D>("Image", @"..\..\..\..\Resources\Images\BallDemo.png", new GorgonCodecPNG());

			var image = _graphics.Textures.FromFile<GorgonTexture2D>("Image2", @"..\..\..\..\Resources\Images\Ship_Decal.png", new GorgonCodecPNG()
				{
					Usage = BufferUsage.Staging
				});
			/*_tex = _graphics.Textures.CreateTexture<GorgonTexture2D>("Texture", new GorgonTexture2DSettings()
			{
				Width = image.Settings.Width,
				Height = image.Settings.Height,
				Format = BufferFormat.R8G8B8A8_UIntNormal,
				Usage = BufferUsage.Default,
				ArrayCount = 2,
				MipCount = 1
			});

			_tex.CopySubResource(_noDecal, 0, 1);
			_tex.CopySubResource(image, 0, 0);*/
			_tex = _graphics.Textures.FromFile<GorgonTexture2D>("Texture", @"D:\unpak\textureUpload.dds", new GorgonCodecPNG());

			image.Dispose();

			_sprite = _2D.Renderables.CreateSprite("Test", new Vector2(32, 32), _noDecal, new RectangleF(0, 0, 1.0f, 1.0f));
			//_sprite = _2D.Renderables.CreateSprite("Test", new Vector2(64, 64), _noDecal, new RectangleF(0, 0, 0.5f, 0.5f));
			//_sprite.Collider = new Gorgon2DAABB();
			//((Gorgon2DAABB)_sprite.Collider).Location = new Vector2(_tex.Settings.Width / 4.0f, _tex.Settings.Height / 4.0f);
			//((Gorgon2DAABB)_sprite.Collider).Size = new Vector2(0.5f, 0.5f);
			//_sprite.Collider = new Gorgon2DBoundingCircle();
			//((Gorgon2DBoundingCircle)_sprite.Collider).Radius = 0.75f;

			_sprite.Anchor = new Vector2(_sprite.Size.X / 2.0f, _sprite.Size.Y / 2.0f);
			GorgonTiming.TimeScale = 0.1f;

			_text = _2D.Renderables.CreateText("Text", _graphics.Fonts.DefaultFont, "This is a line of text.");
			_text.Collider = new Gorgon2DBoundingCircle();
			_text.Angle = 45.0f;
			_text.Anchor = new Vector2(_text.Size.X / 2.0f, _text.Size.Y / 2.0f);
			_text.Position = new Vector2(320, 240);

			_controller = new GorgonAnimationController<GorgonSprite>();
			GorgonAnimation<GorgonSprite> anim = _controller.Add("Position", 3000.0f);            

			_sprite.Position = new Vector2(_swap.Settings.Width / 2.0f, _swap.Settings.Height / 2.0f);

			/*anim.Tracks["Position"].KeyFrames.Add(new GorgonKeyVector2(0.0f, new Vector2(_swap.Settings.Width / 2.0f, _swap.Settings.Height / 2.0f)));
			anim.Tracks["Position"].KeyFrames.Add(new GorgonKeyVector2(1000.0f, new Vector2(0, _swap.Settings.Height / 2.0f)));
			anim.Tracks["Position"].KeyFrames.Add(new GorgonKeyVector2(2000.0f, new Vector2(_swap.Settings.Width / 2.0f, 0)));
			anim.Tracks["Position"].KeyFrames.Add(new GorgonKeyVector2(3000.0f, new Vector2(_swap.Settings.Width / 2.0f, _swap.Settings.Height / 2.0f)));
			anim.Tracks["Color"].KeyFrames.Add(new GorgonKeyGorgonColor(0.0f, GorgonColor.Transparent));
			anim.Tracks["Color"].KeyFrames.Add(new GorgonKeyGorgonColor(1500.0f, new GorgonColor(1.0f, 0.0f, 0.0f, 1.0f)));
			anim.Tracks["Color"].KeyFrames.Add(new GorgonKeyGorgonColor(2000.0f, new GorgonColor(0.0f, 1.0f, 0.0f, 1.0f)));
			anim.Tracks["Color"].KeyFrames.Add(new GorgonKeyGorgonColor(2500.0f, new GorgonColor(0.0f, 0.0f, 1.0f, 1.0f)));
			anim.Tracks["Color"].KeyFrames.Add(new GorgonKeyGorgonColor(3000.0f, new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f)));			
			anim.Tracks["Color"].InterpolationMode = TrackInterpolationMode.Spline;
			anim.Tracks["Texture"].KeyFrames.Add(new GorgonKeyTexture2D(0.0f, _noDecal, new RectangleF(0, 0, 0.5f, 0.5f)));
			anim.Tracks["Texture"].KeyFrames.Add(new GorgonKeyTexture2D(1000.0f, _noDecal, new RectangleF(0.5f, 0, 0.5f, 0.5f)));
			anim.Tracks["Texture"].KeyFrames.Add(new GorgonKeyTexture2D(2000.0f, _noDecal, new RectangleF(0.0f, 0.5f, 0.5f, 0.5f)));
			anim.Tracks["Texture"].KeyFrames.Add(new GorgonKeyTexture2D(2250.0f, _tex, new RectangleF(0, 0, 1.0f, 1.0f)));
			anim.Tracks["Texture"].InterpolationMode = TrackInterpolationMode.None;*/
			anim.Tracks["Scale"].KeyFrames.Add(new GorgonKeyVector2(0.0f, new Vector2(1.0f, 1.0f)));
			anim.Tracks["Scale"].KeyFrames.Add(new GorgonKeyVector2(1500.0f, new Vector2(256.0f, 256.0f)));
			anim.Tracks["Scale"].KeyFrames.Add(new GorgonKeyVector2(3000.0f, new Vector2(1.0f, 1.0f)));

			anim.IsLooped = true;
			//anim.Speed = 0.5f;
			//anim.Time = anim.Length;
			anim.Tracks["Position"].InterpolationMode = TrackInterpolationMode.Linear;

			anim.Save(@"D:\unpak\anim.gorAnim");
						
			_controller.Clear();

			_controller.FromFile(@"D:\unpak\anim.gorAnim");
			
			_textController = new GorgonAnimationController<GorgonText>();
			var tanim = _textController.Add("Rotation", 3000.0f);
			tanim.Tracks["Angle"].KeyFrames.Add(new GorgonKeySingle(0.0f, 0.0f));
			tanim.Tracks["Angle"].KeyFrames.Add(new GorgonKeySingle(1500.0f, 180.0f));
			tanim.Tracks["Angle"].KeyFrames.Add(new GorgonKeySingle(3000.0f, 0.0f));
			tanim.Tracks["Angle"].InterpolationMode = TrackInterpolationMode.Spline;
			tanim.Tracks["Position"].KeyFrames.Add(new GorgonKeyVector2(0, new Vector2(320, 240)));
			tanim.Tracks["Position"].KeyFrames.Add(new GorgonKeyVector2(1500.0f, new Vector2(320, 240)));
			tanim.Tracks["Position"].KeyFrames.Add(new GorgonKeyVector2(1980.0f, new Vector2(800, 600)));
			tanim.Tracks["Position"].KeyFrames.Add(new GorgonKeyVector2(3000.0f, new Vector2(320, 240)));
			tanim.Tracks["Position"].InterpolationMode = TrackInterpolationMode.Spline;
			tanim.IsLooped = true;

			_controller.Play(_sprite, "Position");
			_textController.Play(_text, "Rotation");

			_dyn = _graphics.Textures.CreateTexture<GorgonTexture2D>("Dynamic", new GorgonTexture2DSettings()
					{
						Width = 256,
						Height = 256,
						Format = BufferFormat.R8G8B8A8_UIntNormal,
						ArrayCount = 1,
						MipCount = 1,
						Usage = BufferUsage.Dynamic
					});

			var data = _dyn.Lock<GorgonTexture2DData>(BufferLockFlags.Discard);

			for (int y = 0; y < 256; y++)
			{
				for (int x = 0; x < data.RowPitch; x+=4)
				{
					float value = (GorgonRandom.Perlin(new Vector2(x / GorgonRandom.RandomSingle(200, 256), y / GorgonRandom.RandomSingle(200, 256))) + 1.0f) * 0.5f;/* *0.7f +
								(GorgonRandom.Perlin(new Vector2(x / 128.0f, y / 128.0f)) + 1.0f) / 2.0f * 0.2f +
								(GorgonRandom.Perlin(new Vector2(x / 256.0f, y / 256.0f)) + 1.0f) / 2.0f * 0.1f;*/
					data.Data.WriteInt32(new GorgonColor(value, value, value, (value * 0.5f) + 0.5f).ToARGB());
				}
			}

			_dyn.Unlock();

			_structBuffer = _graphics.Shaders.CreateStructuredBuffer(new GorgonStructuredBufferSettings()
			    {
			        ElementCount = 1,
                    ElementSize = 4
			    });

			int structValue = 2;
			using (var thing = _structBuffer.Lock(BufferLockFlags.Discard | BufferLockFlags.Write))
			{
				thing.Write(structValue);
			}
			
			_structBuffer.Unlock();

            _2D.PixelShader.Resources.SetShaderBuffer<GorgonStructuredBuffer>(2, _structBuffer);
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

			if (e.KeyCode == Keys.T)
			{
				if (_controller["Position"].Tracks["Position"].InterpolationMode == TrackInterpolationMode.None)
					_controller["Position"].Tracks["Position"].InterpolationMode = TrackInterpolationMode.Linear;
				else if (_controller["Position"].Tracks["Position"].InterpolationMode == TrackInterpolationMode.Linear)
					_controller["Position"].Tracks["Position"].InterpolationMode = TrackInterpolationMode.Spline;
				else
					_controller["Position"].Tracks["Position"].InterpolationMode = TrackInterpolationMode.None;

			}

			if (e.KeyCode == Keys.P)
			{
				_controller["Position"].Reset();
				_controller.Play(_sprite, "Position");
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