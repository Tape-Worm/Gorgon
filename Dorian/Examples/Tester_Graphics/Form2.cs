using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using SlimMath;
using GorgonLibrary;
using GorgonLibrary.Math;
using GorgonLibrary.UI;
using GorgonLibrary.IO;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace Tester_Graphics
{
	// KEEP THIS CODE!!!  Use in example for loading animated GIF files!
	public partial class Form2 : Form
	{
		private GorgonConstantBuffer _frameIndex = null;
		private GorgonDataStream _stream = null;
		private GorgonGraphics _graphics = null;
		private GorgonSwapChain _swap = null;
		private Gorgon2D _2D = null;
		private GorgonSprite _sprite = null;
		private GorgonTexture2D _texture = null;
		private GorgonTimer _timer = null;
		private GorgonPixelShader _shader = null;
		private int _frame = 1;

		private bool Idle()
		{
			_2D.Clear(Color.Black);

			_sprite.Opacity += 0.15f * GorgonTiming.Delta;
			_sprite.Opacity = 1.0f.Min(_sprite.Opacity);

			_sprite.Draw();

			if (_timer == null)
			{
				_timer = new GorgonTimer();
			}

			if (_timer.Milliseconds >= 70)
			{
				_timer.Reset();
				_frame++;

				if (_frame >= _texture.Settings.ArrayCount)
				{
					_frame = 1;
				}

				UpdateFrame();
			}

			_2D.Render();
			return true;
		}

		private void UpdateFrame()
		{
			_stream.Write<float>(_frame);
			_stream.Position = 0;
			_frameIndex.Update(_stream);
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (_stream != null)
			{
				_stream.Dispose();
				_stream = null;
			}
		}
		
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				_graphics = new GorgonGraphics();
				_swap = _graphics.Output.CreateSwapChain("Screen", new GorgonSwapChainSettings()
				{
					IsWindowed = true,
					Width = 800,
					Height = 600,
					Format = BufferFormat.R8G8B8A8_UIntNormal
				});

				//GorgonImageCodecs.DDS.LegacyConversionFlags = DDSFlags.NoR10B10G10A2Fix;
				GorgonImageCodecs.GIF.UseAllFrames = true;
				GorgonImageCodecs.GIF.Clip = true;	// Clip this image because animated gifs can have varying frame sizes and resizing the frames can cause issues.
				using (var data = GorgonImageData.FromFile(@"d:\images\rain_test.gif"))
				{
                    _texture = _graphics.Textures.CreateTexture<GorgonTexture2D>("Test", (GorgonTexture2DSettings)data.Settings, data);
				}

				_2D = _graphics.Output.Create2DRenderer(_swap);
				
				_sprite = _2D.Renderables.CreateSprite("Test", new Vector2(_texture.Settings.Width, _texture.Settings.Height), _texture);
				_sprite.Size = new Vector2(_swap.Settings.Width, _swap.Settings.Height);
				_sprite.Opacity = 0.0f;

				_frameIndex = _graphics.Shaders.CreateConstantBuffer(sizeof(float) * 4, false);
				_stream = new GorgonDataStream(sizeof(float));

				_shader = _graphics.Shaders.CreateShader<GorgonPixelShader>("TArray", "DualTex", Properties.Resources.Shader, true);

				_2D.PixelShader.Current = _shader;
				_2D.PixelShader.ConstantBuffers[0] = _frameIndex;

                /*using (var texture = _graphics.Textures.FromFile<GorgonTexture2D>("Test", @"d:\unpak\textureUpload.dds", new GorgonTexture2DSettings()
                {
                    MipCount = 0,
					Usage = BufferUsage.Default
                }))
                {
					using (var data = GorgonImageData.CreateFromTexture(texture))
					{
						data.Save(@"D:\unpak\testSave.dds", GorgonImageCodecs.DDS);
					}

                    var images = texture.ToImage();

                    for (int i = 0; i < images.Length; i++)
                    {
                        images[i].Save(@"d:\unpak\Test\toImage" + i.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    }
                }*/

                
				/*using (Image image = Image.FromFile(@"d:\images\OSUsers_512x512.gif"))
				{
					Image[] images = new Image[160];
					for (int i = 0; i < images.Length; i++)
					{
						images[i] = image.Clone() as Image;
					}
					using (var texture = _graphics.Textures.Create2DTextureFromGDIImage("Test", images, new GorgonGDIOptions()
						{
							MipCount = 10                            
						}))
					{
						texture.Save(@"d:\unpak\textureUpload.dds", ImageFileFormat.DDS);
					}
				}*/

				//Image[] images = new Image[

				/*using (GorgonTexture2D texture = _graphics.Textures.FromFile<GorgonTexture2D>("Test", @"D:\images\OSUsers.jpg"))
				{
					using (WICLoad wic = new WICLoad())
					{
						using (FileStream stream = File.Open(@"D:\unpak\wictest.png", FileMode.Create, FileAccess.Write, FileShare.None))
						{
							wic.SavePNGToStream(texture, stream);
						}
					}
				}*/

				Gorgon.ApplicationIdleLoopMethod = Idle;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

		public Form2()
		{
			InitializeComponent();
		}
	}
}
