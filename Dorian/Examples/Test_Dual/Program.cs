using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using SlimMath;
using GorgonLibrary;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace Test_Dual
{
	static class Program
	{
		private static Form1 _form1;
		private static Form2 _form2;
		private static GorgonGraphics _graphics;
		private static Gorgon2D _2D;
		private static GorgonSwapChain _swap1;
		private static GorgonSwapChain _swap2;
		private static GorgonFont _font = null;
		private static bool _windowed = false;
		private static GorgonTexture2D _ball = null;
		private static GorgonSprite _sprite = null;
		private static RectangleF _bounds1 = RectangleF.Empty;
		private static RectangleF _bounds2 = RectangleF.Empty;
		private static bool _bounceH = false;
		private static bool _bounceV = false;
		private static Vector2 _spriteGlobal = Vector2.Zero;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="timing"></param>
		/// <returns></returns>
		static bool Idle()
		{
			_swap1.Clear(new GorgonColor(0.0f, 1.0f, 0.0f));
			_swap2.Clear(new GorgonColor(0.0f, 0.0f, 1.0f));

			_sprite.Position = _spriteGlobal;
			_2D.Target = _swap1;
			_2D.Drawing.DrawString(_font, "Monitor 1", Vector2.Zero, new GorgonColor(0, 0, 0));
			if (_bounds1.Contains(_spriteGlobal))
				_sprite.Draw();
			_2D.Render();

			_2D.Target = _swap2;
			_2D.Drawing.DrawString(_font, "Monitor 2", Vector2.Zero, new GorgonColor(0, 0, 0));
			if (_bounds2.Contains(_spriteGlobal))
			{
				_sprite.Position = new Vector2(_spriteGlobal.X - 800, _spriteGlobal.Y);
				_sprite.Draw();
			}
			_2D.Render();

			if (!_bounceH)
				_spriteGlobal.X += 300 * GorgonTiming.FrameDelta;
			else
				_spriteGlobal.X -= 300 * GorgonTiming.FrameDelta;

			if (!_bounceV)
				_spriteGlobal.Y += 300 * GorgonTiming.FrameDelta;
			else
				_spriteGlobal.Y -= 300 * GorgonTiming.FrameDelta;

			if (_spriteGlobal.Y > 600)
			{
				_spriteGlobal.Y = 600;
				_bounceV = !_bounceV;
			}

			if (_spriteGlobal.Y < 0)
			{
				_spriteGlobal.Y = 0;
				_bounceV = !_bounceV;
			}

			if (_spriteGlobal.X > 1600)
			{
				_spriteGlobal.X = 1600;
				_bounceH = !_bounceH;
			}

			if (_spriteGlobal.X < 0)
			{
				_spriteGlobal.X = 0;
				_bounceH = !_bounceH;
			}

			return true;
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			_form1 = new Form1();
			_form1.Show();
			_form2 = new Form2();
			_form2.Show();

			_form1.FormClosing += new FormClosingEventHandler(_form1_FormClosing);
			_form2.Owner = _form1;
			_form2.SetDesktopLocation(Screen.AllScreens[1].Bounds.Left, Screen.AllScreens[1].Bounds.Top);

			_graphics = new GorgonGraphics();
			_graphics.ResetFullscreenOnFocus = false;
			_swap1 = _graphics.Output.CreateSwapChain("Swap1", new GorgonSwapChainSettings()
				{
					Width = 800,
					Height = 600,
					BufferCount = 2,
					DepthStencilFormat = BufferFormat.Unknown,
					Flags = SwapChainUsageFlags.RenderTarget,
					Format = BufferFormat.R8G8B8A8_UIntNormal,
					IsWindowed = false,
					MultiSample = new GorgonMultisampling(1, 0),					
					Window = _form1
				});

			_swap2 = _graphics.Output.CreateSwapChain("Swap2", new GorgonSwapChainSettings()
			{
				Width = 800,
				Height = 600,
				BufferCount = 2,
				DepthStencilFormat = BufferFormat.Unknown,
				Flags = SwapChainUsageFlags.RenderTarget,
				Format = BufferFormat.R8G8B8A8_UIntNormal,
				IsWindowed = false,
				MultiSample = new GorgonMultisampling(1, 0),
				Window = _form2
			});

			_2D = _graphics.Output.Create2DRenderer(_swap1);
			_font = _graphics.Fonts.CreateFont("Font", _form1.Font, FontAntiAliasMode.AntiAliasHQ, new System.Drawing.Size(256, 256));
			_ball = _graphics.Textures.FromFile<GorgonTexture2D>("Ball", @"..\..\..\..\Resources\BallDemo\BallDemo.png");
			_sprite = _2D.Renderables.CreateSprite("Sprite", new Vector2(64, 64), _ball, new System.Drawing.RectangleF(64, 0, 64, 64));
			_sprite.Anchor = new Vector2(32, 32);
			_bounds1 = new RectangleF(-32, 0, 864, 600);
			_bounds2 = new RectangleF(800-64, 0, 800+128, 600);

			_form1.Focus();
			_form1.Activated += new EventHandler(_form1_Activated);
			_form1.Deactivate += new EventHandler(_form1_Deactivate);
			_form2.FormClosing += new FormClosingEventHandler(_form2_FormClosing);
			_form1.KeyDown += new KeyEventHandler(_form1_KeyDown);
			
			Gorgon.Run(Idle);
		}

		static void _form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.S)
			{
				GorgonTexture2D combinedTexture = _graphics.Textures.CreateTexture<GorgonTexture2D>("BigPicture", new GorgonTexture2DSettings()
					{
						Width = 1600,
						Height = 600,
						Format = BufferFormat.R8G8B8A8_UIntNormal,
						ArrayCount = 1,
						Usage = BufferUsage.Staging,
						Multisampling = new GorgonMultisampling(1, 0)
					});
				combinedTexture.CopySubResource(_swap1.Texture, 0, 0, new Rectangle(0, 0, 800, 600), Vector2.Zero);
				combinedTexture.CopySubResource(_swap2.Texture, 0, 0, new Rectangle(0, 0, 800, 600), new Vector2(800, 0));
				combinedTexture.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\DualMonitor.png", ImageFileFormat.PNG);
			}
		}

		static void QuitMe()
		{
			Gorgon.Quit();
		}

		/// <summary>
		/// Handles the FormClosing event of the _form2 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.FormClosingEventArgs"/> instance containing the event data.</param>
		static void _form2_FormClosing(object sender, FormClosingEventArgs e)
		{
			QuitMe();
		}

		/// <summary>
		/// Handles the Deactivate event of the _form1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		static void _form1_Deactivate(object sender, EventArgs e)
		{
			if (Form.ActiveForm == _form1)
			    return;

			_form1.WindowState = FormWindowState.Minimized;

			_windowed = true;
		}

		/// <summary>
		/// Handles the Activated event of the _form1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		static void _form1_Activated(object sender, EventArgs e)
		{
			if (_windowed)
			{
				_form1.Activated -= new EventHandler(_form1_Activated);
				_form1.Deactivate -= new EventHandler(_form1_Deactivate);

				_form1.WindowState = FormWindowState.Normal;
				_form2.WindowState = FormWindowState.Normal;

				_swap1.UpdateSettings(false);
				_swap2.UpdateSettings(false);
				_windowed = false;

				_form1.Activated += new EventHandler(_form1_Activated);
				_form1.Deactivate += new EventHandler(_form1_Deactivate);
			}
		}

		/// <summary>
		/// Handles the FormClosing event of the _form1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.FormClosingEventArgs"/> instance containing the event data.</param>
		static void _form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			QuitMe();
		}
	}
}
