//#define MULTIMON

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
using GorgonLibrary.Diagnostics;
using GorgonLibrary.PlugIns;
using GorgonLibrary.Collections;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace Tester_Graphics
{	
	public partial class Form1 : Form
	{
		private GorgonGraphics _graphics = null;
		private GorgonSwapChain _swapChain = null;
		private Gorgon2D _2D = null;
		private GorgonRenderTarget _target = null;
		private GorgonTexture2D _texture = null;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				this.ClientSize = new Size(800, 600);

				_graphics = new GorgonGraphics(DeviceFeatureLevel.SM2_a_b);
				_swapChain = _graphics.Output.CreateSwapChain("My Swap Chain", new GorgonSwapChainSettings()
				{
					Window = this,
					IsWindowed = true
				});

				_target = _graphics.Output.CreateRenderTarget("My target", new GorgonRenderTargetSettings()
				{
					Width = 320,
					Height = 240,
					Format = BufferFormat.B8G8R8X8_UIntNormal
				});

				_texture = _graphics.Textures.FromFile("File", @"..\..\..\..\Resources\Images\TextureTest.png", GorgonTexture2DSettings.FromFile);

				_2D = _graphics.Create2DRenderer(_swapChain);
				_2D.IsLogoVisible = true;

				GorgonSprite sprite = _2D.Renderables.CreateSprite("Sprite", new Vector2(256.0f, 256.0f), _texture, new RectangleF(0, 0, _texture.Settings.Width, _texture.Settings.Height));

				Vector2 position = Vector2.Zero;

				Gorgon.ApplicationIdleLoopMethod = (GorgonFrameRate timing) =>
					{
						_2D.Target = _target;
						_2D.Clear(Color.Black);
						sprite.Position = new Vector2(0, 0);
						sprite.Angle += 90.0f * timing.FrameDelta;
						sprite.Draw();
						_2D.Render();
						
						_2D.Target = null;
						_2D.Clear(Color.White);
						_2D.Drawing.Blit(_target, position);

						position = new Vector2(position.X + 15.0f * timing.FrameDelta, position.Y + 15.0f * timing.FrameDelta);
						
						_2D.Render();
						return true;
					};				
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
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
