using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimMath;
using GorgonLibrary;
using GorgonLibrary.UI;
using GorgonLibrary.Graphics;
using GorgonLibrary.Animation;
using GorgonLibrary.Renderers;
using GorgonLibrary.Math;

namespace Tester_Anim
{
	public partial class Form1 : Form
	{
		struct StarSprite
		{
			public GorgonSprite Sprite;
			public GorgonAnimationController<GorgonSprite> Controller;
		}

		struct Star
		{
			public StarSprite Sprite;
			public Vector2 Position;
		}
		
		private StarSprite[] _sprites;
		private Star[] _stars;
		private GorgonGraphics _graphics = null;
		private Gorgon2D _2D = null;
		private GorgonTexture2D _texture = null;

		private bool Idle()
		{
			_2D.Clear(Color.Black);

			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "FPS: " + GorgonLibrary.Diagnostics.GorgonTiming.FPS.ToString("0.0"), Vector2.Zero, Color.White);

			for (int i = 0; i < _stars.Length; i++)
			{
				if ((_stars[i].Sprite.Sprite == _sprites[4].Sprite) || (_stars[i].Sprite.Sprite == _sprites[5].Sprite))
				{
					if ((_stars[i].Sprite.Controller.CurrentAnimation.Time - _stars[i].Sprite.Controller.CurrentAnimation.Tracks["Scale"].KeyFrames[1].Time).Abs() <= 0.5f)
						_stars[i].Position = new Vector2(GorgonRandom.RandomSingle(), GorgonRandom.RandomSingle());
				}

				if (_stars[i].Sprite.Sprite == _sprites[3].Sprite)
				{
					if ((_stars[i].Sprite.Controller.CurrentAnimation.Time - _stars[i].Sprite.Controller.CurrentAnimation.Tracks["Opacity"].KeyFrames[1].Time).Abs() <= 0.5f)
						_stars[i].Position = new Vector2(GorgonRandom.RandomSingle(), GorgonRandom.RandomSingle());
				}
				_stars[i].Sprite.Sprite.Position = new Vector2(_stars[i].Position.X * _2D.DefaultTarget.Settings.Width, _stars[i].Position.Y * _2D.DefaultTarget.Settings.Height);
				_stars[i].Sprite.Sprite.Draw();				
			}

			for (int i = 0; i < _sprites.Length; i++)
				_sprites[i].Controller.Update();

			_2D.Render();

			return true;
		}

		private void CreateScaler(GorgonAnimationController<GorgonSprite> controller)
		{
			controller.Add("Scale", GorgonRandom.RandomSingle(5000, 10000));
			controller["Scale"].Tracks["Scale"].KeyFrames.Add(new GorgonKeyVector2(0, new Vector2(1, 1)));
			controller["Scale"].Tracks["Scale"].KeyFrames.Add(new GorgonKeyVector2(controller["Scale"].Length * GorgonRandom.RandomSingle(0.25f, 0.5f), new Vector2(0.0125f, 0.0125f)));
			controller["Scale"].Tracks["Scale"].KeyFrames.Add(new GorgonKeyVector2(controller["Scale"].Length, new Vector2(1, 1)));
			controller["Scale"].IsLooped = true;
		}

		private void CreateSpinner(GorgonAnimationController<GorgonSprite> controller)
		{
			controller.Add("Spin", GorgonRandom.RandomSingle(8000, 16000));
			controller["Spin"].Tracks["Angle"].KeyFrames.Add(new GorgonKeySingle(0, 0));
			controller["Spin"].Tracks["Angle"].KeyFrames.Add(new GorgonKeySingle(controller["Spin"].Length * GorgonRandom.RandomSingle(0.25f, 0.5f), GorgonRandom.RandomSingle(360)));
			controller["Spin"].Tracks["Angle"].KeyFrames.Add(new GorgonKeySingle(controller["Spin"].Length, 0));
			controller["Spin"].Tracks["Angle"].InterpolationMode = TrackInterpolationMode.Spline;
			controller["Spin"].IsLooped = true;
		}

		private void CreateFader(GorgonAnimationController<GorgonSprite> controller)
		{
			controller.Add("Fader", GorgonRandom.RandomSingle(8000, 16000));
			controller["Fader"].Tracks["Opacity"].KeyFrames.Add(new GorgonKeySingle(0, 1.0f));
			controller["Fader"].Tracks["Opacity"].KeyFrames.Add(new GorgonKeySingle(controller["Fader"].Length * GorgonRandom.RandomSingle(0.25f, 0.5f), GorgonRandom.RandomSingle(0.25f)));
			controller["Fader"].Tracks["Opacity"].KeyFrames.Add(new GorgonKeySingle(controller["Fader"].Length, 1.0f));
			controller["Fader"].Tracks["Opacity"].InterpolationMode = TrackInterpolationMode.Spline;
			controller["Fader"].IsLooped = true;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				_graphics = new GorgonGraphics();
				_2D = _graphics.Output.Create2DRenderer(this);
				ClientSize = new System.Drawing.Size(1280, 800);

				_texture = _graphics.Textures.FromFile<GorgonTexture2D>("Stars", @"..\..\..\..\Resources\Images\Stars.png");
				_sprites = new StarSprite[6];
				_stars = new Star[2048];

				
				for (int i = 0; i < _sprites.Length; i++)
				{
					StarSprite star = new StarSprite();

					star.Sprite = _2D.Renderables.CreateSprite("Sprite", new Vector2(1, 1), _texture);
					star.Sprite.SmoothingMode = SmoothingMode.Smooth;
					star.Sprite.BlendingMode = BlendingMode.Additive;
					star.Controller = new GorgonAnimationController<GorgonSprite>();

					switch (i)
					{
						case 0:
							star.Sprite.TextureRegion = new RectangleF(_texture.ToTexel(new Vector2(2, 0)), _texture.ToTexel(new Vector2(5, 5)));
							star.Sprite.Size = new Vector2(5, 5);
							CreateSpinner(star.Controller);							
							break;
						case 1:
							star.Sprite.TextureRegion = new RectangleF(_texture.ToTexel(new Vector2(9, 0)), _texture.ToTexel(new Vector2(5, 5)));
							star.Sprite.Size = new Vector2(5, 5);
							CreateFader(star.Controller);
							break;
						case 2:
							star.Sprite.TextureRegion = new RectangleF(_texture.ToTexel(new Vector2(11, 9)), _texture.ToTexel(new Vector2(1, 1)));
							star.Sprite.Size = new Vector2(1, 1);
							break;
						case 5:
							star.Sprite.TextureRegion = new RectangleF(_texture.ToTexel(new Vector2(0, 13)), _texture.ToTexel(new Vector2(21, 24)));
							star.Sprite.Size = new Vector2(21, 24);
							CreateScaler(star.Controller);
							break;
						case 4:
							star.Sprite.TextureRegion = new RectangleF(_texture.ToTexel(new Vector2(35, 2)), _texture.ToTexel(new Vector2(10, 12)));
							star.Sprite.Size = new Vector2(10, 12);
							CreateScaler(star.Controller);
							break;
						case 3:
							star.Sprite.TextureRegion = new RectangleF(_texture.ToTexel(new Vector2(4, 9)), _texture.ToTexel(new Vector2(1, 1)));
							star.Sprite.Size = new Vector2(1, 1);
							CreateFader(star.Controller);
							break;
					}

					if (star.Controller.Count > 0)
						star.Controller.Play(star.Sprite, 0);
					star.Sprite.Anchor = star.Sprite.Size * 0.5f;
					_sprites[i] = star;
				}

				//_sprites[0].Controller[0].Save(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\AnimationSave.anim");
				//_sprites[0].Controller.Remove(0);
				//_sprites[0].Controller.FromFile(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\AnimationSave.anim");
				//_sprites[0].Controller.Play(_sprites[0].Sprite, 0);

				for (int i = 0; i < _stars.Length; i++)
				{
					Star star = new Star();
					star.Position = new Vector2(GorgonRandom.RandomSingle(), GorgonRandom.RandomSingle());

					int sprite = GorgonRandom.RandomInt32(256);
					if (sprite < 253)
						star.Sprite = _sprites[GorgonRandom.RandomInt32(4)];
					else
						star.Sprite = _sprites[GorgonRandom.RandomInt32(4, 6)];
					_stars[i] = star;
				}

				Gorgon.ApplicationIdleLoopMethod = Idle;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
				Gorgon.Quit();
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (_graphics != null)
				_graphics.Dispose();

			_graphics = null;
		}

		public Form1()
		{
			InitializeComponent();
		}
	}
}