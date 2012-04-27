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
using GorgonLibrary.Math;
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
		private GorgonSwapChain _mainScreen = null;
		private GorgonRenderTarget _target = null;
		private GorgonRenderTarget _target2 = null;
		private GorgonRenderTarget _target3 = null;
		private GorgonTexture2D _texture = null;
		private GorgonTexture2D _textureNM = null;
		private GorgonOrthoCamera _cam1 = null;
		private GorgonConstantBuffer _waveEffectBuffer = null;
		private GorgonDataStream _waveEffectStream = null;
		private GorgonRenderTarget _outputTarget = null;
		private GorgonFont font = null;
		private Vector2 _mousePosition = Vector2.Zero;
		private GorgonSprite letter = null;
		private GorgonText _text = null;
		private GorgonText _fps = null;
		private string test = "the quick brown fox jumps over the lazy dog.\r\nTHE QUICK BROWN FOX JUMPS OVER ÃHE LAZY DOG.\r\nThe Quick Brown Fox Jumps Over The Lazy Dog.";
			//"~!@#$%^&*()_+`1234567890-=[]\\{}|,.<>/?abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";//

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			_mousePosition = e.Location;
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			if (e.Delta < 0)
				_2D.Effects.GaussianBlur.BlurAmount -= 0.125f;
			else
				_2D.Effects.GaussianBlur.BlurAmount += 0.125f;

			if (_2D.Effects.GaussianBlur.BlurAmount < 2.0f)
				_2D.Effects.GaussianBlur.BlurAmount = 2.0f;
			if (_2D.Effects.GaussianBlur.BlurAmount > 10.0f)
				_2D.Effects.GaussianBlur.BlurAmount = 10.0f;
		}
		
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{

				this.ClientSize = new Size(800, 600);

				using (GorgonVideoDeviceCollection devices = new GorgonVideoDeviceCollection(true, false))
				{
					//_graphics = new GorgonGraphics(devices[0], DeviceFeatureLevel.SM5);
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

				_texture = _graphics.Textures.FromFile<GorgonTexture2D>("File", @"..\..\..\..\Resources\Images\Ship.png", GorgonTexture2DSettings.FromFile);
				//_textureNM = _graphics.Textures.FromFile<GorgonTexture2D>("File", @"..\..\..\..\Resources\Images\Ship_DISP.png", GorgonTexture2DSettings.FromFile);

				_mainScreen = _graphics.Output.CreateSwapChain("MainScreen", new GorgonSwapChainSettings()
				{
					IsWindowed = true,
					Window = this,
					Flags = SwapChainUsageFlags.RenderTarget | SwapChainUsageFlags.ShaderInput
				});
				_2D = _graphics.Create2DRenderer(_mainScreen);
				//_2D.IsLogoVisible = true;

				GorgonSprite sprite = _2D.Renderables.CreateSprite("Sprite", new Vector2(178, 207), _texture, new RectangleF(0, 0, _texture.Settings.Width, _texture.Settings.Height));

				_target.Texture.Copy(Properties.Resources.Haiku);
				_target2.Texture.Copy(Properties.Resources.Haiku);
				//_texture.CopySubResource(_target.Texture, new Rectangle(0, 0, 256, 128), Vector2.Zero);

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
				string fps = string.Empty;

				GorgonTimer timer = new GorgonTimer();

				_2D.Effects.GaussianBlur.BlurAmount = 2.0f;
				GorgonOrthoCamera camera =_2D.CreateCamera("My Camera", new Vector2(640, 480), 1000.0f);
				_outputTarget = _graphics.Output.CreateRenderTarget("Output", new GorgonRenderTargetSettings()
				{
					Width = 800,
					Height = 600,
					Format = BufferFormat.R8G8B8A8_UIntNormal,
					DepthStencilFormat = BufferFormat.Unknown,
					MultiSample = new GorgonMultisampling(1, 0),
				});

				GorgonSprite window = _2D.Renderables.CreateSprite("Window", new Vector2(800, 600), _mainScreen.Texture);
				_2D.Effects.GaussianBlur.BlurAmount = 3.0f;

				_2D.Effects.GaussianBlur.BlurRenderTargetsSize = new System.Drawing.Size(256, 256);
				window.Texture = _outputTarget.Texture;
				window.BlendingMode = BlendingMode.None;
				window.Angle = 0.0f;
				window.Blending.DestinationAlphaBlend = BlendType.InverseSourceAlpha;

				font = _graphics.Textures.CreateFont("Bradley Hand ITC Regular", new GorgonFontSettings()
					{
						TextureSize = new System.Drawing.Size(512, 512),
						FontFamilyName = "Arial",
						FontStyle = FontStyle.Bold,
						PointSize = 10,
						TextContrast = 0,
						AntiAliasingMode = FontAntiAliasMode.AntiAliasHQ,
						PackingSpacing = 1,
						OutlineSize = 1
					});

				System.IO.StreamReader reader = null;
				try
				{
					reader = new System.IO.StreamReader(@"X:\Unpak\LZWEXP.TXT");
					test = reader.ReadToEnd();
				}
				finally
				{
					if (reader != null)
						reader.Dispose();
				}
				letter = _2D.Renderables.CreateSprite("Letter", new Vector2(1, 1), null, new RectangleF(0, 0, 1, 1));
				letter.Color = Color.Black;
				letter.Angle = 0.0f;
				_text = new GorgonText(_2D, "Text", font);
				_text.Text = test;
				//_text.TextRectangle = new RectangleF(0, 0, _mainScreen.Settings.Width, _mainScreen.Settings.Height);
				//_text.ClipToRectangle = true;
				//_text.WordWrap = true;
				_fps = new GorgonText(_2D, "FPS", font);
				_fps.ShadowEnabled = true;
				_fps.Color = Color.LightBlue;
				_text.LineSpacing = -1.0f;
				//_text.ShadowEnabled = true;
				//_text.UseKerning = false;
				//_text.Alignment = Alignment.Center;				
				float lineSpaceAngle = 90;
				Gorgon.ApplicationIdleLoopMethod = (GorgonFrameRate timing) =>
					{
						//Text = timing.AverageFPS.ToString("0.0");
						fps = timing.AverageFPS.ToString("0.0");
						_2D.Clear(Color.White);
//                        /*						if (!backForth)
//                                                    angle += 0.125f * timing.FrameDelta;
//                                                else
//                                                    angle -= 0.125f * timing.FrameDelta;

//                                                if ((angle > 0.05f) || (angle < -0.05f))
//                                                    backForth = !backForth;*/
							
//                        //_waveEffectStream.Position = 0;
//                        //_waveEffectStream.Write(1.0f / _target.Settings.Width);
//                        //_waveEffectStream.Write(1.0f / _target.Settings.Height);
//                        ////_waveEffectStream.Write(0.2f);
//                        ////_waveEffectStream.Write(0.8f); //(angle + 0.05f) * 2000.0f);
//                        ////_waveEffectStream.Write(1.0f);
//                        ////_waveEffectStream.Write(1.0f / _target.Settings.Height); //(angle + 0.05f) * 2000.0f);
//                        //offset += (10.0f * timing.FrameDelta) / sprite.Size.Y;
//                        ////_waveEffectStream.Write(offset);						
//                        //_waveEffectStream.Position = 0;
//                        //_waveEffectBuffer.Update(_waveEffectStream);

//                        _2D.Target = _target;
//                        _2D.Drawing.Blit(_target2, Vector2.Zero);
//                        //_2D.Clear(Color.Black);
//                        //_2D.Camera = camera;

//                        sprite.Opacity = 1.0f;
//                        sprite.Position = new Vector2(_mainScreen.Settings.Width - 128, _mainScreen.Settings.Height - 128);
//                        sprite.Blending.DestinationAlphaBlend = BlendType.InverseSourceAlpha;
//                        sprite.Anchor = new Vector2(89, 103);
//                        //sprite.Angle += 90.0f * timing.FrameDelta;
//                        angle += 94.0f * timing.FrameDelta;
//                        sprite.Angle = 0.0f;//angle;
//                        sprite.BlendingMode = BlendingMode.Modulate;
//                        sprite.Opacity = 0.26f;
//                        sprite.Texture = _texture;
//                        sprite.SmoothingMode = SmoothingMode.Smooth;
//                        sprite.Draw();
//                        _2D.Target = _outputTarget;
//                        _2D.Target.Clear(Color.White);
//                        sprite.Draw();
//                        //_2D.Effects.Wave.Amplitude = 5.0f / _target.Settings.Width;
//                        //_2D.Effects.Wave.Period += 0.125f * timing.FrameDelta;
//                        //_2D.Effects.Wave.Length = 45.0f;
//                        //_2D.Effects.Wave.Render((int passIndex) =>
//                        //{
//                        //    _2D.Drawing.Blit(_target, position);
//                        //});
//                        _2D.Drawing.Blit(_target, position);
//                        _2D.Target = null;

//                        _2D.Effects.Displacement.DisplacementRenderTargetSize = _outputTarget.Settings.Size;
//                        _2D.Effects.Displacement.Strength = 3.5f;
//                        _2D.Effects.Displacement.Render((int pass) =>
//                            {
//                                //sprite.Texture = _textureNM;
//                                if (pass == 0)
//                                {
//                                    sprite.Opacity = 1.0f;
//                                    sprite.BlendingMode = BlendingMode.Additive;
//                                    sprite.Draw();
//                                    sprite.Position = sprite.Position + new Vector2(50, 0);
//                                    sprite.Angle = -angle;
//                                    sprite.Draw();
//                                }
//                                else
//                                {
//                                    _2D.Drawing.Blit(_outputTarget, Vector2.Zero);

//                                    _2D.Target = _outputTarget;
//                                    _2D.Effects.SobelEdgeDetection.LineThickness = 2.0f / _mainScreen.Settings.Width;
//                                    _2D.Effects.SobelEdgeDetection.EdgeThreshold = 0.85f;
//                                    _2D.Effects.SobelEdgeDetection.Render((int passIndex) =>
//                                    {
//                                        _2D.Drawing.Blit(_mainScreen, Vector2.Zero);

//                                        _2D.Target = null;
//                                        _2D.Effects.Posterize.Bits = 15;
//                                        _2D.Effects.Posterize.Render((int posPass) =>
//                                        {
//                                            _2D.Drawing.Blit(_outputTarget, Vector2.Zero);
//                                        });
//                                    });
//                                }
//                            });


//                        window.Size = new Vector2(_mainScreen.Settings.Width / 2.0f, _mainScreen.Settings.Height / 2.0f);
//                        window.Scale = new Vector2((_2D.Effects.GaussianBlur.BlurRenderTargetsSize.Width * 2.0f) / _mainScreen.Settings.Width, (_2D.Effects.GaussianBlur.BlurRenderTargetsSize.Height * 2.0f) / _mainScreen.Settings.Height);
//                        window.TextureOffset = _mousePosition - new Vector2(_mainScreen.Settings.Width / 4.0f, _mainScreen.Settings.Height / 4.0f); ;
//                        window.TextureSize = window.Size;
//                        for (int i = 0; i < (int)((10.0f - _2D.Effects.GaussianBlur.BlurAmount) * 2); i++)
//                        {
//                            _2D.Target = null;
//                            _2D.Effects.GaussianBlur.Render((int passIndex) =>
//                            {
//                                if (passIndex == 0)
//                                    window.Draw();
//                                //_2D.Drawing.Blit(_mainScreen, Vector2.Zero, new Vector2(512.0f / _mainScreen.Settings.Width, 512.0f / _mainScreen.Settings.Height));
//                                else
//                                {
//                                    _2D.Drawing.Blit(_2D.Effects.GaussianBlur.BlurredTexture, window.TextureOffset, new Vector2(1.0f / window.Scale.X, 1.0f / window.Scale.Y));
//                                }
//                            });
//                            _2D.Target = _outputTarget;
//                            _2D.Drawing.Blit(_mainScreen, Vector2.Zero);
//                        }
//                        _2D.Target = null;
//                        _2D.Drawing.Blit(_outputTarget, Vector2.Zero);
//                        _2D.Drawing.DrawRectangle(new RectangleF(window.TextureOffset.X, window.TextureOffset.Y, window.Size.X, window.Size.Y), Color.Black);


////						_2D.Render();


//                        //_target.Texture.Save(@"X:\unpak\testfile.png", ImageFileFormat.PNG);
						

//                        //_2D.PixelShader.Current = tempShader;					
///*							
//                        for (int i = 0; i < blurPasses; i++)
//                        {
//                            if ((i % 2) == 0)
//                            {
//                                _2D.Target = _target2;
//                                _2D.Drawing.Blit(_target, Vector2.Zero);
//                            }
//                            else
//                            {
//                                _2D.Target = _target;
//                                _2D.Drawing.Blit(_target2, Vector2.Zero);
//                            }
//                        }

//                        if (timer.Milliseconds > 100)
//                        {
//                            timer.Reset();
//                            if (!backForth)
//                                blurPasses--;
//                            else
//                                blurPasses++;

//                            if ((blurPasses == 0) || (blurPasses == 128))
//                                backForth = !backForth;
//                        }

//                        if ((_2D.Target != _target) && (blurPasses > 0))
//                        {
//                            _2D.Target = _target;
//                            _2D.Drawing.Blit(_target2, Vector2.Zero);
//                        }*/

//                        _2D.Camera = null;
//                        _2D.Target = null;
//                        _2D.Viewport = null;
//                        //_2D.PixelShader.Current = null;						


//                        //_2D.Effects.Invert.Render((int passInvertIndex) =>
//                        //    {
//                        //        testEffect.OutputTarget = _target3;
//                        //        testEffect.FrameDelta = timing.FrameDelta;
//                        //        testEffect.WaveTarget = _target2;
//                        //        testEffect.Render((int passIndex) =>
//                        //            {
//                        //                if (passIndex == 0)
//                        //                    _2D.Drawing.Blit(_target, Vector2.Zero);
//                        //                else
//                        //                    _2D.Drawing.Blit(_target2, Vector2.Zero);
//                        //            });

//                        //        _2D.Target = null;
//                        //        _2D.Drawing.Blit(_target3, position);
//                        //    });

//                        /*_2D.Effects.BurnDodge.UseDodge = false;
//                        _2D.Effects.BurnDodge.UseLinear = true;
//                        _2D.Effects.BurnDodge.Render((int pass) =>
//                            {
//                                _2D.Drawing.Blit(_target, position);
//                            });*/

//                        //_2D.Target = _target2;
//                        //_2D.Effects.Wave.Amplitude = 0.01f;
//                        //_2D.Effects.Wave.Period += (50.0f / _target.Settings.Height) * timing.FrameDelta;
//                        //_2D.Effects.Wave.Length = 50.0f;
//                        //_2D.Effects.Wave.WaveType = WaveType.Both;
//                        //_2D.Effects.Wave.Render((int index) =>
//                        //    {
//                        //        _2D.Drawing.Blit(_target, Vector2.Zero);
//                        //        _2D.Target = null;
//                        //        _2D.Effects.SharpenEmboss.Amount = 2.0f;
//                        //        _2D.Effects.SharpenEmboss.Area = _target.Settings.Size;
//                        //        _2D.Effects.SharpenEmboss.UseEmbossing = true;
//                        //        _2D.Effects.SharpenEmboss.Render((int innerIndex) =>
//                        //        {
//                        //            _2D.Drawing.Blit(_target2, position);
//                        //        });
//                        //    }
//                        //);


						//position = new Vector2(position.X + 15.0f * timing.FrameDelta, position.Y + 15.0f * timing.FrameDelta);
						float y = 0;
						Vector2 pos = Vector2.Zero;
						//for (int i = 0; i < font.Textures.Count; i++)
						//{
						//    _2D.Drawing.FilledRectangle(new RectangleF(pos.X, pos.Y, font.Textures[i].Settings.Width, font.Textures[i].Settings.Height), Color.DarkBlue);
						//    _2D.Drawing.Blit(font.Textures[i], pos, new Vector2(2, 2));

						//    if ((int)(pos.X + font.Textures[i].Settings.Width + 1) > _mainScreen.Settings.Width)
						//    {
						//        pos.X = 0;
						//        pos.Y += font.Textures[i].Settings.Height + 1;
						//    }
						//    else
						//        pos.X += font.Textures[i].Settings.Width + 1;

						//}
						//test = "FPS: " + timing.AverageFPS.ToString("0.0");
						//pos = new Vector2(0, _mainScreen.Settings.Height / 2.0f);
						////letter.Angle += 2.0f * timing.FrameDelta;
						//for (int i = 0; i < test.Length; i++)
						//{
						//    GorgonGlyph glyph = font.Glyphs[test[i]];

						//    if (test[i] != ' ')
						//    {
						//        /*if (i > 0)
						//        {
						//            GorgonKerningPair kernPair = new GorgonKerningPair(test[i - 1], test[i]);

						//            if (font.KerningPairs.ContainsKey(kernPair))
						//                pos.X -= font.KerningPairs[kernPair];
						//        }*/

						//        letter.Texture = glyph.Texture;
						//        letter.TextureRegion = glyph.GlyphCoordinates;
						//        letter.Size = glyph.GlyphCoordinates.Size;
						//        //letter.Anchor = -(new Vector2(pos.X + glyph.Offset.X, glyph.Offset.Y)) + new Vector2(400, 28);								
						//        letter.Position = new Vector2(pos.X + glyph.Offset.X, glyph.Offset.Y + (_mainScreen.Settings.Height / 2.0f));
						//        letter.Draw();
						//        pos.X += glyph.Advance.X + glyph.Advance.Y;// +glyph.Advance.Z;
						//        if (i < test.Length - 1)
						//        {
						//            GorgonKerningPair kernPair = new GorgonKerningPair(test[i], test[i + 1]);

						//            if (font.KerningPairs.ContainsKey(kernPair))
						//                pos.X += font.KerningPairs[kernPair];
						//            else
						//                pos.X += glyph.Advance.Z;

						//            pos.X += font.Settings.OutlineSize;// / 2.0f;								
						//        }
						//        if (pos.X > _mainScreen.Settings.Width)
						//            break;
						//    }
						//    else
						//        pos.X += font.Glyphs[test[i]].GlyphCoordinates.Width - 1;
							
						//}

						//pos = new Vector2(0, (_mainScreen.Settings.Height / 2.0f) + font.FontHeight + font.Settings.OutlineSize);
						//for (int i = 0; i < test.Length; i++)
						//{
						//    GorgonGlyph glyph = font.Glyphs[test[i]];

						//    if (test[i] != ' ')
						//    {
						//        y = pos.Y + glyph.Offset.Y;
						//        letter.Texture = glyph.Texture;
						//        letter.TextureRegion = glyph.GlyphCoordinates;
						//        letter.Size = glyph.GlyphCoordinates.Size;
						//        letter.Anchor = Vector2.Zero;
						//        letter.Position = new Vector2(pos.X + glyph.Offset.X, y);
						//        letter.Draw();
						//        pos.X += glyph.GlyphCoordinates.Width;
						//        /*pos.X += glyph.Advance.X + glyph.Advance.Y;// +glyph.Advance.Z;
						//        if (i < test.Length - 1)
						//        {
						//            GorgonKerningPair kernPair = new GorgonKerningPair(test[i], test[i + 1]);

						//            if (font.KerningPairs.ContainsKey(kernPair))
						//                pos.X += font.KerningPairs[kernPair];
						//            else
						//                pos.X += glyph.Advance.Z;
						//        }*/
						//        if (pos.X > _mainScreen.Settings.Width)
						//            break;
						//    }
						//    else
						//        pos.X += font.Glyphs[test[i]].GlyphCoordinates.Width - 1;

						//}

						//pos = new Vector2(0, 0);
						////letter.Angle += 2.0f * timing.FrameDelta;
						//for (int i = 0; i < fps.Length; i++)
						//{
						//    GorgonGlyph glyph = font.Glyphs[fps[i]];

						//    if (fps[i] != ' ')
						//    {
						//        /*if (i > 0)
						//        {
						//            GorgonKerningPair kernPair = new GorgonKerningPair(fps[i - 1], fps[i]);

						//            if (font.KerningPairs.ContainsKey(kernPair))
						//                pos.X -= font.KerningPairs[kernPair];
						//        }*/

						//        letter.Texture = glyph.Texture;
						//        letter.TextureRegion = glyph.GlyphCoordinates;
						//        letter.Size = glyph.GlyphCoordinates.Size;
						//        //letter.Anchor = -(new Vector2(pos.X + glyph.Offset.X, glyph.Offset.Y)) + new Vector2(400, 28);								
						//        letter.Position = new Vector2(pos.X + glyph.Offset.X, glyph.Offset.Y);
						//        letter.Draw();
						//        pos.X += glyph.Advance.X + glyph.Advance.Y;// +glyph.Advance.Z;
						//        if (i < fps.Length - 1)
						//        {
						//            GorgonKerningPair kernPair = new GorgonKerningPair(fps[i], fps[i + 1]);

						//            if (font.KerningPairs.ContainsKey(kernPair))
						//                pos.X += font.KerningPairs[kernPair];
						//            else
						//                pos.X += glyph.Advance.Z;

						//            pos.X += font.Settings.OutlineSize;// / 2.0f;
						//        }
						//    }
						//    else
						//        pos.X += font.Glyphs[fps[i]].GlyphCoordinates.Width - 1;

						//}

						//_2D.Drawing.Blit(font.Textures[0], Vector2.Zero);
						//_text.Color = Color.FromArgb(96, 0, 0, 0);
						float rads = GorgonMathUtility.Radians(angle * 90.0f);
						float cos = GorgonMathUtility.Cos(rads);
						float sin = GorgonMathUtility.Sin(rads);

						_text.ShadowOffset = new Vector2(2 * cos - 2 * sin, 2 * sin + 2 * cos);
						_text.SmoothingMode = SmoothingMode.Smooth;
						//_text.Text = test;
						//_text.Color = Color.LightGreen;
						_text.Position = new Vector2(45, 45);
						//_text.Anchor = new Vector2(-1, -1);
						//_text.Scale = new Vector2(2, 2);
						_text.LineSpacing = -(((float)Math.Sin(GorgonMathUtility.Radians(lineSpaceAngle)) + 0.95f) * (1 / 1.95f));
						//_text.TextRectangle = new RectangleF(0, 0, _2D.Target.Viewport.Region.Width - _text.Position.X + _text.Anchor.X, _2D.Target.Viewport.Region.Height - _text.Position.Y + _text.Anchor.Y);
						//_2D.Drawing.DrawRectangle(new RectangleF(Vector2.Zero, _text.Size), Color.Blue);
						_text.Angle = angle * 5.0f;
						//_text.ClipToRectangle = true;
						//_text.Draw();						
						_text.Color = Color.LightGreen;
						//_text.Anchor = Vector2.Zero;
						//_text.Position = new Vector2(45, 45);
						_text.Draw();
						//_fps.Color = Color.FromArgb(96, 0, 0, 0);
						//_fps.Anchor = new Vector2(-1, -1);
						_fps.Text = timing.AverageFPS.ToString("0.0#") + " DT: " + (timing.AverageFrameDelta * 1000.0f).ToString("0.000") + " ms";
						_fps.Position = Vector2.Zero;
						//_fps.Draw();
						_fps.Color = Color.LightBlue;
						_fps.Anchor = Vector2.Zero;
						_fps.Draw();
						//_text.Color = Color.White;
						//_text.Position = Vector2.Subtract(_text.Position, new Vector2(2, 2));
						//_text.Draw();

						//Text = timing.AverageFPS.ToString("0.0#");
						_2D.Render();

						lineSpaceAngle += 180.0f * timing.FrameDelta;
						if (lineSpaceAngle > 360.0f)
							lineSpaceAngle = 360.0f - lineSpaceAngle;

						angle += 1.0f * timing.FrameDelta;
						if (angle > 360.0f)
							angle = 360.0f - angle;

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

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.SizeChanged"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			if (_graphics == null)
				return;

			if (_outputTarget != null)
				_outputTarget.Dispose();
			_outputTarget = _graphics.Output.CreateRenderTarget("Output", new GorgonRenderTargetSettings()
							{
								Size = ClientSize,
								Format = BufferFormat.R8G8B8A8_UIntNormal
							});
		}
	}
}
