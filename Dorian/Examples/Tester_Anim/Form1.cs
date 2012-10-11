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

namespace Tester_Anim
{
	public partial class Form1 : Form
	{
		struct Star
		{
			public GorgonSprite Sprite;
			public GorgonAnimationController<GorgonSprite> Animation;
		}

		private GorgonGraphics _graphics = null;
		private Gorgon2D _2D = null;
		private GorgonTexture2D _texture = null;
		private Star[] _stars = null;
		private Random _rnd = new Random();

		private bool Idle()
		{
			bool isDropping = false;
			_2D.Clear(Color.Black);

			for (int i = 0; i < _stars.Length; i++)
			{
				if (_stars[i].Sprite.Position.Y >= _2D.DefaultTarget.Settings.Height)
					continue;

				if (!isDropping)
					isDropping = (_stars[i].Animation.CurrentAnimation != null) && (_stars[i].Animation.CurrentAnimation.Name == "Drop");
				_stars[i].Sprite.Anchor = _stars[i].Sprite.Size / 2.0f;
				_stars[i].Sprite.Draw();

				if (_stars[i].Animation != null)
				{
					if ((!isDropping) && ((_stars[i].Animation.CurrentAnimation == null) || (_stars[i].Animation.CurrentAnimation.Name != "Drop")))
					{
						_stars[i].Animation.Play(_stars[i].Sprite, "Drop");
						isDropping = true;
					}
					_stars[i].Animation.Update();
				}
			}

			_2D.Render();

			return true;
		}

		private void CreateSpinner(ref Star star)
		{
			star.Animation = new GorgonAnimationController<GorgonSprite>();
			star.Animation.Add("Spin", (float)(_rnd.NextDouble() * 15000.0f + 15000));
			star.Animation["Spin"].Tracks["Angle"].KeyFrames.Add(new GorgonKeySingle(0, 0));
			star.Animation["Spin"].Tracks["Angle"].KeyFrames.Add(new GorgonKeySingle(star.Animation["Spin"].Length * (float)(_rnd.NextDouble() * 0.25 + 0.25), (float)(_rnd.NextDouble() * 360.0f)));
			star.Animation["Spin"].Tracks["Angle"].KeyFrames.Add(new GorgonKeySingle(star.Animation["Spin"].Length, 0));
			star.Animation["Spin"].Tracks["Angle"].InterpolationMode = TrackInterpolationMode.Spline;
			star.Animation["Spin"].IsLooped = true;
		}

		private void CreateFader(ref Star star)
		{
			star.Animation = new GorgonAnimationController<GorgonSprite>();
			star.Animation.Add("Fader", (float)(_rnd.NextDouble() * 8000.0 + 8000));
			star.Animation["Fader"].Tracks["Opacity"].KeyFrames.Add(new GorgonKeySingle(0, 1.0f));
			star.Animation["Fader"].Tracks["Opacity"].KeyFrames.Add(new GorgonKeySingle(star.Animation["Fader"].Length * (float)(_rnd.NextDouble() * 0.25 + 0.25) , (float)_rnd.NextDouble() * 0.25f));
			star.Animation["Fader"].Tracks["Opacity"].KeyFrames.Add(new GorgonKeySingle(star.Animation["Fader"].Length, 1.0f));
			star.Animation["Fader"].Tracks["Opacity"].InterpolationMode = TrackInterpolationMode.Spline;
			star.Animation["Fader"].IsLooped = true;
		}

		private void CreateDropper(ref Star star)
		{
			star.Animation.Add("Drop", _rnd.Next(7000) + 2000);
			star.Animation["Drop"].Tracks["Position"].KeyFrames.Add(new GorgonKeyVector2(0, star.Sprite.Position));
			star.Animation["Drop"].Tracks["Position"].KeyFrames.Add(new GorgonKeyVector2(star.Animation["Drop"].Length, new Vector2(star.Sprite.Position.X, _2D.DefaultTarget.Settings.Height + 5)));
			star.Animation["Drop"].Tracks["Position"].InterpolationMode = TrackInterpolationMode.Spline;
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
				_stars = new Star[512];				
				
				for (int i = 0; i < _stars.Length; i++)
				{
					Star star = new Star();
					star.Sprite = _2D.Renderables.CreateSprite("Sprite", new Vector2(1, 1), _texture);
					star.Sprite.Position = new Vector2((float)(_rnd.NextDouble() * _2D.DefaultTarget.Settings.Width), (float)(_rnd.NextDouble() * _2D.DefaultTarget.Settings.Height));
					star.Sprite.SmoothingMode = SmoothingMode.Smooth;
					star.Sprite.BlendingMode = BlendingMode.Additive;

					int sprite = _rnd.Next(256);

					if (sprite < 43)
					{
						star.Sprite.TextureRegion = new RectangleF(_texture.ToTexel(new Vector2(2, 0)), _texture.ToTexel(new Vector2(5, 5)));
						star.Sprite.Size = new Vector2(5, 5);
						if (_rnd.Next(100) < 50)
							CreateSpinner(ref star);
						else
							CreateFader(ref star);
						star.Animation.Play(star.Sprite, 0);
					}
					if ((sprite >= 43) && (sprite < 86))
					{
						star.Sprite.TextureRegion = new RectangleF(_texture.ToTexel(new Vector2(9, 0)), _texture.ToTexel(new Vector2(5, 5)));
						star.Sprite.Size = new Vector2(5, 5);
						if (_rnd.Next(100) < 50)
							CreateSpinner(ref star);
						else
							CreateFader(ref star);

						star.Animation.Play(star.Sprite, 0);
					}
					if ((sprite >= 86) && (sprite < 129))
					{
						star.Sprite.TextureRegion = new RectangleF(_texture.ToTexel(new Vector2(4, 9)), _texture.ToTexel(new Vector2(1, 1)));
						star.Sprite.Size = new Vector2(1, 1);
						CreateFader(ref star);
						star.Animation.Play(star.Sprite, 0);
					}
					if ((sprite >= 129) && (sprite < 253))
					{
						star.Sprite.TextureRegion = new RectangleF(_texture.ToTexel(new Vector2(11, 9)), _texture.ToTexel(new Vector2(1, 1)));
						star.Sprite.Size = new Vector2(1, 1);
						CreateFader(ref star);
						star.Animation.Play(star.Sprite, 0);
					}
					if ((sprite >= 253) && (sprite < 254))
					{
						star.Sprite.TextureRegion = new RectangleF(_texture.ToTexel(new Vector2(0, 13)), _texture.ToTexel(new Vector2(21, 24)));
						star.Sprite.Size = new Vector2(21, 24);
						if (_rnd.Next(100) > 75)
						{
							CreateSpinner(ref star);
							star.Animation.Play(star.Sprite, 0);
						}
					}
					if ((sprite >= 254) && (sprite < 256))
					{
						star.Sprite.TextureRegion = new RectangleF(_texture.ToTexel(new Vector2(35, 2)), _texture.ToTexel(new Vector2(10, 12)));
						star.Sprite.Size = new Vector2(10, 12);
						if (_rnd.Next(100) > 75)
						{
							CreateSpinner(ref star);
							star.Animation.Play(star.Sprite, 0);
						}
					}
					if (star.Animation == null)
						star.Animation = new GorgonAnimationController<GorgonSprite>();
					CreateDropper(ref star);
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