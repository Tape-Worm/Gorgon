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
using GorgonLibrary.Graphics.Renderers;

namespace Tester_Graphics
{	
	public partial class Form1 : Form
	{
		private Gorgon2D _graphics2D = null;
		private GorgonSprite _sprite = null;
		private GorgonSprite _sprite2 = null;

		GorgonVideoMode mode1 = default(GorgonVideoMode);
#if MULTIMON
		GorgonVideoMode mode2 = default(GorgonVideoMode);
#endif
		GorgonGraphics _graphics = null;
		GorgonSwapChain _swapChain = null;
		GorgonRasterizerStates _multiSample = GorgonRasterizerStates.DefaultStates;
		GorgonTimer _timer = new GorgonTimer(true);
		PointF pos = PointF.Empty;
		GorgonBlendStates blend1 = GorgonBlendStates.DefaultStates;
		GorgonBlendStates blend2 = GorgonBlendStates.DefaultStates;
		

		int frameCount = 0;
		//float frameDepth = 0.0f;
		bool _pause = false;
		Random _rnd = new Random();
		float accum = 0.0f;
		float target = (float)GorgonFrameRate.FpsToMilliseconds(60) / 1000.0f;

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (e.KeyCode == Keys.Pause)			
				_pause = !_pause;
		}

		Vector2 testMove = Vector2.Zero;
		float angle = 0.0f;
		float alphaTest = 0.0f;
		GorgonTexture2D _texture = null;
		private bool Idle(GorgonFrameRate timing)
		{			

			try
			{
				//_graphics.SetViewport(_rnd.Next(0, ClientSize.Width - 1), _rnd.Next(ClientSize.Height - 1), _rnd.Next(ClientSize.Width - 1) + 1, _rnd.Next(ClientSize.Height - 1) + 1, 0, 1.0f);

				if (!_swapChain.IsInStandBy)
				{
					_swapChain.Clear(Color.Black);
					//_sprite.BlendingMode = BlendingMode.None;
					//if (alphaTest > 0.25f)
					//    _sprite.AlphaTestValues = new GorgonMinMaxF(alphaTest - 0.25f, alphaTest);
					//else
					//    _sprite.AlphaTestValues = new GorgonMinMaxF(0, alphaTest);

					//_graphics2D.ViewMatrix = Matrix.Translation(-400.0f, -300.0f, 0) * Matrix.RotationZ(GorgonLibrary.Math.GorgonMathUtility.Sin(GorgonLibrary.Math.GorgonMathUtility.Radians(-angle * 2.0f))) * Matrix.Translation(400, 300, 0);
					//_sprite.Scale = new Vector2(4.0f, 4.0f);
					//_sprite.TextureOffset = testMove;
					//_sprite.Opacity = 1.0f;
					//_sprite.Angle = new Vector3(angle, 0, angle);					
					//_sprite.Position = new Vector3((_swapChain.Settings.Width / 2) - (_sprite.Size.X / 2), _swapChain.Settings.Height / 2 - (_sprite.Size.Y / 2), 0);
					//_sprite.Position = new Vector3(_swapChain.Settings.Width / 2.0f, _swapChain.Settings.Height / 2.0f, 0);
					_sprite.Position = Vector2.Zero;
					_sprite.Draw();

					//_sprite.Opacity = 0.75f;
					//_sprite.BlendingMode = BlendingMode.Additive;
					_sprite2.Position = Vector2.Subtract(_sprite.Position, new Vector2(128, 128));
					//_sprite.Angle = Vector3.Zero;
					//_sprite.Position = Vector3.Subtract(_sprite.Position, new Vector3(128, 128, 0));
					_sprite2.Draw();

					_graphics2D.Render();

					angle += 90.0f * timing.FrameDelta;
					if (angle > 360.0f)
						angle = 360.0f - angle;

					testMove.X += 30.0f * timing.FrameDelta;
					testMove.Y += 30.0f * timing.FrameDelta;

					alphaTest += 0.02f * timing.FrameDelta;

					if (alphaTest > 1.0f)
						alphaTest = 1.0f;
					//_graphics.Draw();
					//_graphics.ApplyStates();
					//_graphics.ApplyViewports();

				//    if (_swapChain.DepthStencil != null)
				//        _swapChain.DepthStencil.Clear(1.0f, 0);

				//    //if (frameCount == 0)
				//    {
				//        _swapChain.Clear(Color.Black);
				//        //frameCount = 0;
				//    }

				//    if (_test1 != null)
				//    {
				//        //if (!_pause) 
				//        {
				//            if (timing.FrameDelta < 0.16666666666666666666666666666667f)
				//                accum += timing.FrameDelta;
				//            else
				//                accum += 0.16666666666666666666666666666667f;
				//            while (accum >= target)
				//            {								
				//                //_test1.Transform(timing.FrameDelta);
				//                if (!_pause)
				//                    _test1.Transform(target);
				//                frameCount = 0;
				//                accum -= target;

				//                if (accum <= target)
				                    Text = "FPS: " + timing.FPS.ToString() + " DT:" + (timing.FrameDelta * 1000).ToString() + " msec.  AT:" + alphaTest.ToString("####0.0####");
				//            }
				//        }

				//        _test1.Draw();
				//        _graphics.DrawIndexed(0, 0, _sprites.Length * 6);
				//    }


				//    //_graphics.ApplyViewport(1);
				//    //_test1.Draw();

				//    if (_test2 != null)
				//    {
				//        if (_swapChain2.DepthStencil != null)
				//            _swapChain2.DepthStencil.Clear(1.0f, 0);
				//        _swapChain2.Clear(Color.Black);
				//        if (!_pause)
				//            _test2.Transform(timing.FrameDelta);
				//        _test2.Draw();
				//    }
				}
								
				frameCount++;

#if MULTIMON
				if (_swapChain2 != null)
					_swapChain2.Flip();
#endif
				//System.Threading.Thread.Sleep(1);
			}
			catch (Exception ex)
			{
				Gorgon.AllowBackground = false;
				GorgonException.Catch(ex, () => GorgonLibrary.UI.GorgonDialogs.ErrorBox(this, ex));
				return false;
			}

			return true;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.KeyCode == Keys.D1)
			{
				_graphics.Output.BlendingState.States = blend1;
			}

			if (e.KeyCode == Keys.D2)
			{
				_graphics.Output.BlendingState.States = blend2;
			}

			if (e.KeyCode == Keys.F1) //((e.Alt) && (e.KeyCode == Keys.Enter))
				_swapChain.UpdateSettings(!_swapChain.Settings.IsWindowed);
		}

		private bool _deactivated = false;

		protected override void OnDeactivate(EventArgs e)
		{
			base.OnDeactivate(e);
#if MULTIMON
			if ((Form.ActiveForm == this) || (Form.ActiveForm == form2))
				return;
			_deactivated = true;
#endif
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);

#if MULTIMON
			if ((Form.ActiveForm == form2) && (!_swapChain.Settings.IsWindowed))
				return;

			if ((_swapChain2 != null) && (_swapChain != null) && (_deactivated))
			{
				_swapChain.UpdateSettings(false);
				_swapChain2.UpdateSettings(false);
				_deactivated = false;
			}
#endif
		}


		void form2_Deactivate(object sender, EventArgs e)
		{

		}

		void form2_Activated(object sender, EventArgs e)
		{
			if (_deactivated)
				OnActivated(e);
		}

		
		protected override void OnLoad(EventArgs e)
		{			
			base.OnLoad(e);

			try
			{
				this.panelDX.Visible = false;

				GorgonFrameRate.UseHighResolutionTimer = true;

				Gorgon.UnfocusedSleepTime = 10;
				Gorgon.AllowBackground = true;

				this.Show();

				ClientSize = new System.Drawing.Size(640, 480);

#if MULTIMON
				form2 = new Form2();
				form2.FormClosing += new FormClosingEventHandler(form2_FormClosing);
				form2.Activated += new EventHandler(form2_Activated);
				form2.Deactivate += new EventHandler(form2_Deactivate);
				form2.ShowInTaskbar = false;
				form2.Show();
#endif				
				//GorgonVideoDeviceCollection devices = new GorgonVideoDeviceCollection(false, true);
				//_graphics = new GorgonGraphics(devices[1], DeviceFeatureLevel.SM2_a_b);				
				//devices.Dispose();				
				//_graphics = new GorgonGraphics(DeviceFeatureLevel.SM2_a_b);				
				_graphics = new GorgonGraphics();

				////_graphics.IsObjectTrackingEnabled = false;
				////_graphics.ResetFullscreenOnFocus = false;

				//_multiSample.IsMultisamplingEnabled = true;
				//_multiSample.IsScissorTestingEnabled = false;
				////_multiSample.CullingMode =  GorgonCullingMode.None;
				//_graphics.Rasterizer.States = _multiSample;
				//blend1.RenderTarget0.IsBlendingEnabled = true;
				//blend1.RenderTarget0.SourceBlend = BlendType.SourceAlpha;
				//blend1.RenderTarget0.DestinationBlend = BlendType.InverseSourceAlpha;

				//blend2 = blend1;
				//blend2.RenderTarget0.DestinationBlend = BlendType.One;
				////blend2.RenderTarget0.WriteMask = ColorWriteMaskFlags.Blue | ColorWriteMaskFlags.Green | ColorWriteMaskFlags.Alpha;
				//_graphics.Output.BlendingState.States = blend1;
				
				//GorgonGraphics.IsDWMCompositionEnabled = false;
				//this.TopMost = true;
				//this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
				//this.WindowState = FormWindowState.Maximized;
				mode1 = (from videoMode in _graphics.VideoDevice.Outputs[0].VideoModes
						 where videoMode.Width == 800 && videoMode.Height == 600 && 
							(videoMode.Format == BufferFormat.R8G8B8A8_UIntNormal_sRGB || videoMode.Format == BufferFormat.R8G8B8A8_UIntNormal || videoMode.Format == BufferFormat.B8G8R8A8_UIntNormal || videoMode.Format == BufferFormat.B8G8R8A8_UIntNormal_sRGB)
							&& (_graphics.VideoDevice.SupportsDisplayFormat(videoMode.Format))
						 orderby videoMode.Format, videoMode.RefreshRateNumerator descending, videoMode.RefreshRateDenominator descending
						 select videoMode).First();

				int count = 4;
				int quality = _graphics.VideoDevice.GetMultiSampleQuality(mode1.Format, count);
				GorgonMultiSampling multiSample = new GorgonMultiSampling(count, quality - 1);
				multiSample = new GorgonMultiSampling(1, 0);
				_swapChain = _graphics.Output.CreateSwapChain("Swap", new GorgonSwapChainSettings() { Window = this, IsWindowed = true, VideoMode = mode1, MultiSample = multiSample, DepthStencilFormat = BufferFormat.Unknown});
				_swapChain.Resized += new EventHandler(_swapChain_Resized);
				
				_graphics2D = _graphics.Create2DRenderer(_swapChain);
				float aspect = (float)(_swapChain.Settings.VideoMode.Width) / (float)(_swapChain.Settings.VideoMode.Height);
				_graphics2D.ProjectionMatrix = Matrix.PerspectiveFovLH(GorgonLibrary.Math.GorgonMathUtility.Radians(75.0f), aspect, 0.01f, 10000.0f);
				_graphics2D.ViewMatrix = Matrix.LookAtLH(new Vector3(-0.0f, -0.0f, 0.1f), new Vector3(0, 0, -0.1f), -Vector3.UnitY);
				_sprite = _graphics2D.Renderables.CreateSprite("Test", 100.0f, 100.0f);
				_texture = _graphics.Textures.FromFile("Test", @"..\..\..\..\Resources\BallDemo\BallDemo.png", GorgonTexture2DSettings.FromFile);
				_sprite.Texture = _texture;
				
				_sprite.TextureOffset = new Vector2(0.5f, 0);
				_sprite.Size = new Vector2(0.5f, 0.5f);
				_sprite.Anchor = new Vector2(0.25f, 0.25f);
				_sprite.Scale = new Vector2(0.03125f, 0.03125f);
				//_sprite.CullingMode = CullingMode.None;

				// TODO: Make a new sprite type for perspective correct sprites.
				//       Allow the user to set the texture coordinates manually and don't use sprite size to determine region.

				_sprite2 = _graphics2D.Renderables.CreateSprite("Test2", 100.0f, 100.0f);				
				
				//_sprite.HorizontalWrapping = TextureAddressing.Border;
				//_sprite.BorderColor = Color.Blue;
				//_sprite.Opacity = 0.25f;				
				_sprite2.TextureOffset = new Vector2(64, 0);
				_sprite2.Size = new Vector2(64, 64);
				_sprite2.Anchor = new Vector2(32, 32);
				_sprite2.Scale = new Vector2(4, 4);

				_sprite2.Texture = _texture;
				//_sprite2.SetVertexColor(SpriteCorner.UpperRight, new GorgonColor(0.0f, 1.0f, 1.0f, 1.0f));
				//_sprite2.SetVertexColor(SpriteCorner.LowerLeft, new GorgonColor(0.0f, 0.0f, 1.0f, 0.0f));
				//_sprite2.Angle = new Vector3(0, 0, 45.0f);
				//_sprite.SetVertexOffset(SpriteCorner.UpperLeft, new Vector3(-10.0f, -20.0f, 0.0f));
				//_sprite.SetVertexOffset(SpriteCorner.LowerRight, new Vector3(10.0f, 20.0f, 0.0f));
				//_graphics2D.ViewMatrix = Matrix.LookAtLH(new Vector3(0.0f, 0.0f, -5.0f), new Vector3(0, 0, 1.0f), Vector3.UnitY);				
				//_sprite.TextureSize = new Vector2(128, 128);
				//_graphics.Rasterizer.SetViewport(_swapChain.Viewport);
				//_graphics.Viewports.Add(new GorgonViewport(640, 400, 640, 400));

#if MULTIMON
				form2.Location = _graphics.VideoDevices[0].Outputs[1].OutputBounds.Location;

				mode2 = (from videoMode in _graphics.VideoDevices[0].Outputs[1].VideoModes
						 where videoMode.Width == 640 && videoMode.Height == 480 &&
							(videoMode.Format == BufferFormat.R8G8B8A8_UIntNormal_sRGB || videoMode.Format == BufferFormat.R8G8B8A8_UIntNormal || videoMode.Format == BufferFormat.B8G8R8A8_UIntNormal || videoMode.Format == BufferFormat.B8G8R8A8_UIntNormal_sRGB)
							&& (_graphics.VideoDevice.SupportsDisplayFormat(videoMode.Format))
						 orderby videoMode.Format, videoMode.RefreshRateNumerator, videoMode.RefreshRateDenominator
						 select videoMode).First();

				_swapChain2 = _graphics.CreateSwapChain("Swap2", new GorgonSwapChainSettings() { IsWindowed = true, Window = form2, VideoMode = mode2, DepthStencilFormat = BufferFormat.D24_UIntNormal_S8_UInt });
				this.Focus();
#endif

				//_swapChain.UpdateSettings(false);
#if MULTIMON
				//_swapChain2.UpdateSettings(false);				
#endif				
				
				//_test1 = new Test(_swapChain);
#if MULTIMON
				_test2 = new Test(_swapChain2);
#endif
				
				Gorgon.ApplicationIdleLoopMethod = Idle;
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
				Gorgon.Quit();		
			}

		}

		void _swapChain_Resized(object sender, EventArgs e)
		{
			float aspect = (float)(_swapChain.Settings.VideoMode.Width) / (float)(_swapChain.Settings.VideoMode.Height);
			_graphics2D.ProjectionMatrix = Matrix.PerspectiveFovLH(GorgonLibrary.Math.GorgonMathUtility.Radians(75.0f), aspect, 0.01f, 10000.0f);
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			try
			{
				if (_graphics2D != null)
					_graphics2D.Dispose();
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
			}
		}

		public Form1()
		{
			//this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			//this.SetStyle(ControlStyles.UserPaint, true);
			InitializeComponent();
		}
	}
}
