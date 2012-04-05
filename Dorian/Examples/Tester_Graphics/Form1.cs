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
		private Random _rnd = new Random();
		private GorgonGraphics _graphics = null;
		private Gorgon2D _2D = null;
		private GorgonRenderTarget _target = null;
		private GorgonRenderTarget _target2 = null;
		private GorgonRenderTarget _target3 = null;
		private GorgonTexture2D _texture = null;
		private GorgonOrthoCamera _cam1 = null;
		private GorgonConstantBuffer _waveEffectBuffer = null;
		private GorgonDataStream _waveEffectStream = null;
		
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{

				this.ClientSize = new Size(800, 600);

				using (GorgonVideoDeviceCollection devices = new GorgonVideoDeviceCollection(true, false))
				{
					_graphics = new GorgonGraphics(devices[0], DeviceFeatureLevel.SM4);
					//_graphics = new GorgonGraphics(devices[0], DeviceFeatureLevel.SM2_a_b);
				}

				_target = _graphics.Output.CreateRenderTarget("My target", new GorgonRenderTargetSettings()
				{
					Width = 320,
					Height = 240,
					Format = BufferFormat.R8G8B8A8_UIntNormal
				});

				_target2 = _graphics.Output.CreateRenderTarget("My target 2", new GorgonRenderTargetSettings()
				{
					Width = 320,
					Height = 240,
					Format = BufferFormat.R8G8B8A8_UIntNormal
				});

				_target3 = _graphics.Output.CreateRenderTarget("My Target 3", _target2.Settings);

				//using (GorgonTexture1D _1D = _graphics.Textures.CreateTexture<GorgonTexture1D>("Test", new GorgonTexture1DSettings()
				//{
				//    Width = 512,
				//    ArrayCount = 1,
				//    MipCount = 1,
				//    Format = BufferFormat.A8_UIntNormal,
				//    Usage = BufferUsage.Default
				//}, null))
				//{
				//    byte[] data = _1D.Save(ImageFileFormat.DDS);
				//}

				_texture = _graphics.Textures.FromFile<GorgonTexture2D>("File", @".\TextureTest.png", GorgonTexture2DSettings.FromFile);

				_2D = _graphics.Create2DRenderer(this);
				_2D.IsLogoVisible = true;

				GorgonSprite sprite = _2D.Renderables.CreateSprite("Sprite", new Vector2(256.0f, 256.0f), _texture, new RectangleF(0, 0, _texture.Settings.Width, _texture.Settings.Height));

				_target.Texture.Copy(Properties.Resources.Haiku);
				_texture.CopySubResource(_target.Texture, new Rectangle(0, 0, 256, 128), Vector2.Zero);

				//byte[] data = new byte[_texture.SizeInBytes];

				////_rnd.NextBytes(data);
				//for (int i = 0; i < data.Length; i+=4)
				//{
				//    data[i] = 255;
				//    data[i + 1] = 0;
				//    data[i + 2] = 0;
				//    data[i + 3] = 255;
				//}

				//int dataposition = ((256 * 4) * 128) + (128 * 4);

				//for (int i = 0; i < 8; i++)
				//{
				//    data[dataposition] = 255;
				//    data[dataposition + 1] = 255;
				//    data[dataposition + 2] = 255;
				//    data[dataposition + 3] = 255;
				//    dataposition += 4;
				//}

				
/*				using (GorgonDataStream stream = new GorgonDataStream(data))
				{
					_texture.UpdateSubResource(new GorgonTexture2DData(stream, 1024), 0);
				}*/

				//data = new byte[64 * 4 * 64];
				////_rnd.NextBytes(data);

				//for (int i = 0; i < data.Length; i += 4)
				//{
				//    data[i] = 64;
				//    data[i + 1] = 128;
				//    data[i + 2] = (byte)((16384 - i) / 64);
				//    data[i + 3] = 255;
				//}
/*				using (GorgonDataStream stream = new GorgonDataStream(data))
				{
					_target.Texture.UpdateSubResource(new GorgonTexture2DData(stream, 256), 0, new Rectangle(128, 128, 64, 64));
				}*/

				//using (GorgonDataStream stream = _texture.Lock(0, BufferLockFlags.Write | BufferLockFlags.Discard))
				//{
				//    for (int y = 128; y < 128 + 64; y++)
				//    {
				//        stream.Position = ((256 * 4) * y) + (128 * 4);
				//        stream.Write(data, (y - 128) * 256, 256);
				//    }
				//}
				//_texture.Unlock();
								
				//_target.Texture.CopySubResource(_texture, new Rectangle(0, 0, 256, 240), Vector2.Zero);

				// TODO: Test this on the 6870, buggy ATI drivers seem to be choking on this, especially on SM2_a_b feature levels.
				//GorgonTexture2D normalTexture = _target.Texture.ConvertToNormalized32Bit();
				//System.IO.MemoryStream stream = new System.IO.MemoryStream();
				//_target.Texture.Save(stream, ImageFileFormat.DDS);
				//normalTexture.Save(stream, ImageFileFormat.DDS);
				//System.IO.FileStream fileSt = System.IO.File.Open(@"d:\unpak\testfile.dds", System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None);
				//for (int i = 0; i < stream.Length; i++)
				//{
				//    int data = stream.ReadByte();
				//    fileSt.WriteByte((byte)data);
				//}
				//_target.Texture.Save(@".\testfile.png", ImageFileFormat.PNG);
				//_texture.Save(@".\testfile.png", ImageFileFormat.PNG);
				//_target.Texture.Save(@".\rt.png", ImageFileFormat.PNG);
				//_texture.SaveSM2WorkAround();
				//normalTexture.Save(@".\testfile.png", ImageFileFormat.PNG);
				//fileSt.Close();
				//stream.Dispose();
				/*_texture.Dispose();
				_texture = normalTexture;
				sprite.Texture = _texture;*/

				Vector2 position = Vector2.Zero;
				//string shaderSource = "#GorgonInclude \"Gorgon2DShaders\"";
				//    "float4 PixEntry(GorgonSpriteVertex vertex) : SV_Target\n" +
				//    "{\n" +
				//    "return GorgonPixelShaderTextured(vertex);\n" +
				//    "float grey = color.r * 0.3f + color.g * 0.59f + color.b * 0.11f;\n" +
				//    "return float4(grey, grey, grey, color.a);\n" +
				//    "}";

				//GorgonPixelShader tempShader = _graphics.Shaders.CreateShader<GorgonPixelShader>("TempShader", "GorgonPixelShaderQuickBlur", shaderSource, true);

				////_2D.PixelShader.Current = tempShader;

				//_waveEffectBuffer = _graphics.Shaders.CreateConstantBuffer(16, false);
				//_waveEffectStream = new GorgonDataStream(16);
				//_graphics.Shaders.PixelShader.ConstantBuffers[2] = _waveEffectBuffer;

				float angle = 0.0f;
				float offset = 0.0f;
				bool backForth = false;
				int blurPasses = 128;

				GorgonTimer timer = new GorgonTimer();

				_2D.Effects.GaussianBlur.BlurAmount = 2.0f;
				GorgonOrthoCamera camera =_2D.CreateCamera("My Camera", new Vector2(640, 480), 1000.0f);
				Gorgon.ApplicationIdleLoopMethod = (GorgonFrameRate timing) =>
					{
/*						if (!backForth)
						    angle += 0.125f * timing.FrameDelta;
						else
						    angle -= 0.125f * timing.FrameDelta;

						if ((angle > 0.05f) || (angle < -0.05f))
						    backForth = !backForth;*/
							
						//_waveEffectStream.Position = 0;
						//_waveEffectStream.Write(1.0f / _target.Settings.Width);
						//_waveEffectStream.Write(1.0f / _target.Settings.Height);
						////_waveEffectStream.Write(0.2f);
						////_waveEffectStream.Write(0.8f); //(angle + 0.05f) * 2000.0f);
						////_waveEffectStream.Write(1.0f);
						////_waveEffectStream.Write(1.0f / _target.Settings.Height); //(angle + 0.05f) * 2000.0f);
						//offset += (10.0f * timing.FrameDelta) / sprite.Size.Y;
						////_waveEffectStream.Write(offset);						
						//_waveEffectStream.Position = 0;
						//_waveEffectBuffer.Update(_waveEffectStream);

						_2D.Target = _target;
						_2D.Clear(Color.Black);
						_2D.Camera = camera;
						_2D.Viewport = new GorgonViewport(0, 0, 160.0f, 120.0f, 0, 1);						

						sprite.Opacity = 1.0f;
						sprite.Position = new Vector2(160, 120);
						//sprite.Angle += 90.0f * timing.FrameDelta;
						sprite.Draw();

//						_2D.Render();

						//_target.Texture.Save(@"X:\unpak\testfile.png", ImageFileFormat.PNG);
						

						//_2D.PixelShader.Current = tempShader;					
/*							
						for (int i = 0; i < blurPasses; i++)
						{
							if ((i % 2) == 0)
							{
								_2D.Target = _target2;
								_2D.Drawing.Blit(_target, Vector2.Zero);
							}
							else
							{
								_2D.Target = _target;
								_2D.Drawing.Blit(_target2, Vector2.Zero);
							}
						}

						if (timer.Milliseconds > 100)
						{
							timer.Reset();
							if (!backForth)
								blurPasses--;
							else
								blurPasses++;

							if ((blurPasses == 0) || (blurPasses == 128))
								backForth = !backForth;
						}

						if ((_2D.Target != _target) && (blurPasses > 0))
						{
							_2D.Target = _target;
							_2D.Drawing.Blit(_target2, Vector2.Zero);
						}*/

						_2D.Camera = null;
						_2D.Target = null;
						_2D.Viewport = null;
						//_2D.PixelShader.Current = null;						

						_2D.Clear(Color.White);
						_2D.Drawing.BlendingMode = BlendingMode.None;

						//_2D.Effects.Invert.Render((int passInvertIndex) =>
						//    {
						//        testEffect.OutputTarget = _target3;
						//        testEffect.FrameDelta = timing.FrameDelta;
						//        testEffect.WaveTarget = _target2;
						//        testEffect.Render((int passIndex) =>
						//            {
						//                if (passIndex == 0)
						//                    _2D.Drawing.Blit(_target, Vector2.Zero);
						//                else
						//                    _2D.Drawing.Blit(_target2, Vector2.Zero);
						//            });

						//        _2D.Target = null;
						//        _2D.Drawing.Blit(_target3, position);
						//    });

						_2D.Target = _target2;
						_2D.Effects.Wave.Amplitude = 0.01f;
						_2D.Effects.Wave.Period += (50.0f / _target.Settings.Height) * timing.FrameDelta;
						_2D.Effects.Wave.Length = 50.0f;
						_2D.Effects.Wave.WaveType = WaveType.Both;
						_2D.Effects.Wave.Render((int index) =>
							{
								_2D.Drawing.Blit(_target, Vector2.Zero);
								if (_2D.Effects.GaussianBlur.BlurAmount < 10.0f)
									_2D.Target = _target3;
								else
									_2D.Target = null;
								_2D.Effects.SharpenEmboss.Amount = 2.0f;
								_2D.Effects.SharpenEmboss.Area = _target.Settings.Size;
								_2D.Effects.SharpenEmboss.UseEmbossing = true;
								_2D.Effects.SharpenEmboss.Render((int innerIndex) =>
								{
									if (_2D.Effects.GaussianBlur.BlurAmount < 10.0f)
										_2D.Drawing.Blit(_target2, Vector2.Zero);
									else
										_2D.Drawing.Blit(_target2, position);
								});

								if (_2D.Effects.GaussianBlur.BlurAmount < 10.0f)
								{
									_2D.Effects.GaussianBlur.BlurAmount += 0.5f * timing.FrameDelta;
									if (_2D.Effects.GaussianBlur.BlurAmount > 10.0f)
										_2D.Effects.GaussianBlur.BlurAmount = 10.0f;

									_2D.Effects.GaussianBlur.SourceTexture = _target3.Texture;
									_2D.Effects.GaussianBlur.Render(position);
								}
							}
						);


						position = new Vector2(position.X + 15.0f * timing.FrameDelta, position.Y + 15.0f * timing.FrameDelta);
						
						_2D.Render();
						return true;
					};

				//normalTexture.Dispose();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}		

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			switch (e.KeyCode)
			{
				case Keys.C:
					if (_2D.Camera == null)
						_2D.Camera = _cam1;
					else
						_2D.Camera = null;
					break;
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (_waveEffectStream != null)
				_waveEffectStream.Dispose();

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
