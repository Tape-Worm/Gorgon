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
        private GorgonRenderTarget _target = null;
		private GorgonConstantBuffer _frameIndex = null;
		private GorgonDataStream _stream = null;
		private GorgonGraphics _graphics = null;
		private GorgonSwapChain _swap = null;
		private Gorgon2D _2D = null;
		private GorgonSprite _sprite = null;
        private GorgonSprite _targetSprite = null;
		private GorgonTexture2D _texture = null;
		private GorgonTimer _timer = null;
		private GorgonPixelShader _shader = null;
        private ushort[] _delays = null;
		private int _frame = 0;
		private GorgonFont _font = null;
		private GorgonText _text = null;
		private GorgonRectangle _rect = null;
		private GorgonRasterizerStates _wireFrame = new GorgonRasterizerStates()
		{
			CullingMode = CullingMode.Back,
			FillMode = FillMode.Wireframe,
			IsDepthClippingEnabled = true
		};

		private bool Idle()
		{
            if (_graphics.VideoDevice.SupportedFeatureLevel != DeviceFeatureLevel.SM2_a_b)
            {
                _2D.PixelShader.Current = _shader;
                _2D.PixelShader.ConstantBuffers[2] = _frameIndex;
            }
            _2D.Target = _target;
			
			_sprite.Draw();

            _2D.PixelShader.Current = null;
            _2D.Target = null;
            _2D.Clear(Color.White);

			float aspectRatio = 1.0f;

			if (_targetSprite.Size.X >= _targetSprite.Size.Y)
			{
				aspectRatio = ((_targetSprite.Size.Y / _targetSprite.Size.X) / ((float)_swap.Settings.Height / (float)_swap.Settings.Width));
				_targetSprite.ScaledSize = new Vector2(_swap.Settings.Width, _swap.Settings.Height * aspectRatio);
			}
			else
			{
				aspectRatio = ((_targetSprite.Size.X / _targetSprite.Size.Y) / ((float)_swap.Settings.Width / (float)_swap.Settings.Height));
				_targetSprite.ScaledSize = new Vector2(_swap.Settings.Width * aspectRatio, _swap.Settings.Height);
			}

            
			_targetSprite.Anchor = new Vector2(_targetSprite.Size.X / 2.0f, _targetSprite.Size.Y / 2.0f);
			
			_targetSprite.Position = new Vector2(_swap.Settings.Width / 2.0f, _swap.Settings.Height / 2.0f);
            _targetSprite.Opacity += (GorgonTiming.Delta * 0.5f);
            _targetSprite.Opacity = 1.0f.Min(_targetSprite.Opacity);
            _targetSprite.Draw();

			if (_timer == null)
			{
				_timer = new GorgonTimer();
			}

            if (_texture.Settings.ArrayCount > 1)
            {
                int delay = _delays[_frame] * 10;
                if (_timer.Milliseconds > delay)
                {
                    _timer.Reset();
                    _frame++;

                    if (_frame >= _texture.Settings.ArrayCount)
                    {
                        _frame = 0;
                    }

                    UpdateFrame(_frame);
                }
            }

			_rect.Draw();
			//_2D.ClipRegion = new Rectangle(0, 0, 64, 32);
			GorgonRasterizerStates prevstate = _graphics.Rasterizer.States;
//			_2D.Render(false);

//			_graphics.Rasterizer.States = _wireFrame;
			_text.Draw();			
//			_2D.Render(false);
//			_graphics.Rasterizer.States = prevstate;
			//_2D.ClipRegion = null;

			_2D.Render();
			return true;
		}

		private void UpdateFrame(int frame)
		{
			_stream.Write<float>(frame);
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
				int count = 8;
				int quality = GorgonVideoDeviceEnumerator.VideoDevices[0].GetMultiSampleQuality(BufferFormat.R8G8B8A8_UIntNormal, count) - 1;
				_graphics = new GorgonGraphics(DeviceFeatureLevel.SM5);
				_swap = _graphics.Output.CreateSwapChain("Screen", new GorgonSwapChainSettings()
				{
					IsWindowed = true,
					Width = 800,
					Height = 600,
					MultiSample = new GorgonMultisampling(count, quality),
					Format = BufferFormat.R8G8B8A8_UIntNormal
				});

				//GorgonImageCodecs.DDS.LegacyConversionFlags = DDSFlags.NoR10B10G10A2Fix;
                string fileName = @"c:\mike\unpak\rain_test.gif";
				//GorgonImageCodecs.TIFF.UseAllFrames = true;
				var codec = new GorgonCodecGIF();
				codec.Clip = true;	// Clip this image because animated gifs can have varying frame sizes and resizing the frames can cause issues.				
				codec.Format = BufferFormat.R8G8B8A8_UIntNormal;
                _delays = codec.GetFrameDelays(fileName);
				using (var imageData = GorgonImageData.FromFile(fileName, codec))
				{
					//imageData.Resize(64, 128, false, ImageFilter.Cubic);
					//_texture = _graphics.Textures.FromFile<GorgonTexture2D>("Test", fileName, codec);
					_texture = _graphics.Textures.CreateTexture<GorgonTexture2D>("Test", imageData);
				}

                //_texture.Save(@"d:\unpak\saveTest.png", new GorgonCodecPNG());

/*                using (var stream = File.Open(@"c:\mike\unpak\testStream.bin", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    stream.WriteByte(0x7F);
                    stream.WriteByte(0x7F);
                    stream.WriteByte(0x7F);
					_texture.Save(stream, new GorgonCodecPNG()
						{
						});
                }

				_texture.Dispose();
				
				using (var stream = File.Open(@"c:\mike\unpak\testStream.bin", FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					stream.ReadByte();
					stream.ReadByte();
					stream.ReadByte();

					_delays = codec.GetFrameDelays(stream);
					_texture = _graphics.Textures.FromStream<GorgonTexture2D>("Test", stream, (int)(stream.Length - stream.Position), new GorgonCodecGIF());
				}*/

				_font = _graphics.Fonts.CreateFont("Test", new GorgonFontSettings() {
					FontFamilyName = "Times New Roman", 
					Size = 24.0f, 
					FontStyle = FontStyle.Bold, 
					AntiAliasingMode = FontAntiAliasMode.AntiAliasHQ, 
					TextureSize = new System.Drawing.Size(256, 256),
					OutlineSize = 2
				});				

				_2D = _graphics.Output.Create2DRenderer(_swap);				

				_text = _2D.Renderables.CreateText("Test", _font, "Testing gradient\nTesting spacing.");
				_text.SmoothingMode = SmoothingMode.Smooth;
				_text.Color = Color.Blue;
				_text.SetCornerColor(RectangleCorner.LowerRight, Color.Cyan);
				_text.SetCornerColor(RectangleCorner.LowerLeft, Color.Cyan);
				_text.Angle = 40.0f;				
				_text.Position = new Vector2(0, 32);

				_rect = _2D.Renderables.CreateRectangle("Rect", new RectangleF(_text.Position, _text.Size), Color.DarkGray, true);
				_rect.Angle = _text.Angle;

				_sprite = _2D.Renderables.CreateSprite("Test", new Vector2(_texture.Settings.Width, _texture.Settings.Height), _texture);
                _sprite.Blending.SourceAlphaBlend = BlendType.One;
                _sprite.Blending.DestinationAlphaBlend = BlendType.One;

				_frameIndex = _graphics.Shaders.CreateConstantBuffer(sizeof(float) * 4, false);
				_stream = new GorgonDataStream(sizeof(float));
                UpdateFrame(0);

                if (_graphics.VideoDevice.SupportedFeatureLevel != DeviceFeatureLevel.SM2_a_b)
                {
                    _shader = _graphics.Shaders.CreateShader<GorgonPixelShader>("TArray", "DualTex", Properties.Resources.Shader, true);
                }

                _target = _graphics.Output.CreateRenderTarget("Name", new GorgonRenderTargetSettings()
                {
                    Width = (int)_sprite.Size.X,
                    Height = (int)_sprite.Size.Y,
                    Format = BufferFormat.R8G8B8A8_UIntNormal
                });

                _target.Clear(Color.Transparent);
                _targetSprite = _2D.Renderables.CreateSprite("Target", new Vector2(_target.Settings.Width, _target.Settings.Height), _target.Texture);
                _targetSprite.Opacity = 0.0f;
				_targetSprite.SmoothingMode = SmoothingMode.Smooth;
				_targetSprite.Anchor = new Vector2(_targetSprite.Size.X / 2.0f, _targetSprite.Size.Y / 2.0f);

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