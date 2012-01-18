//#define MULTIMON

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.UI;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.PlugIns;
using GorgonLibrary.Collections;
using GorgonLibrary.Graphics;

namespace Tester_Graphics
{
	public partial class Form1 : Form
	{
		GorgonVideoMode mode1 = default(GorgonVideoMode);
		GorgonVideoMode mode2 = default(GorgonVideoMode);
		Test _test1 = null;
		Test _test2 = null;
		GorgonGraphics _graphics = null;
		GorgonSwapChain _swapChain = null;
		GorgonSwapChain _swapChain2 = null;
		GorgonRasterizerStates _multiSample = GorgonRasterizerStates.DefaultStates;
		GorgonTimer _timer = new GorgonTimer(true);
		Form2 form2 = null;
		PointF pos = PointF.Empty;
		GorgonBlendStates blend1 = GorgonBlendStates.DefaultStates;
		GorgonBlendStates blend2 = GorgonBlendStates.DefaultStates;
		

		int frameCount = 0;
		float frameDepth = 0.0f;
		bool _pause = false;
		Random _rnd = new Random();

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (e.KeyCode == Keys.Pause)			
				_pause = !_pause;
		}

		private bool Idle(GorgonFrameRate timing)
		{			
			Text = "FPS: " + timing.FPS.ToString() + " DT:" + (timing.FrameDelta * 1000).ToString() + " msec.";

			try
			{
				//_graphics.SetViewport(_rnd.Next(0, ClientSize.Width - 1), _rnd.Next(ClientSize.Height - 1), _rnd.Next(ClientSize.Width - 1) + 1, _rnd.Next(ClientSize.Height - 1) + 1, 0, 1.0f);

				if (!_swapChain.IsInStandBy)
				{
					//_graphics.Draw();
					//_graphics.ApplyStates();
					//_graphics.ApplyViewports();

					if (_swapChain.DepthStencil != null)
						_swapChain.DepthStencil.Clear(1.0f, 0);

					//if (frameCount == 0)
					{
						_swapChain.Clear(Color.Black);
						frameCount = 0;
					}

					if (_test1 != null)
					{
						//if (!_pause)
						//    _test1.Transform(timing.FrameDelta);
						_test1.Draw();
					}


					//_graphics.ApplyViewport(1);
					//_test1.Draw();

					if (_test2 != null)
					{
						if (_swapChain2.DepthStencil != null)
							_swapChain2.DepthStencil.Clear(1.0f, 0);
						_swapChain2.Clear(Color.Black);
						if (!_pause)
							_test2.Transform(timing.FrameDelta);
						_test2.Draw();
					}
				}

				_swapChain.Flip();

#if MULTIMON
				if (_swapChain2 != null)
					_swapChain2.Flip();
#endif
				//System.Threading.Thread.Sleep(10);
				frameDepth += 0.05f * timing.FrameDelta;

				if (frameDepth > 1.0f)
					frameDepth = 0.0f;

				frameCount++;
			}
			catch (Exception ex)
			{
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
				_graphics.Blending.States = blend1;
			}

			if (e.KeyCode == Keys.D2)
			{
				_graphics.Blending.States = blend2;
			}

			if (e.KeyCode == Keys.F1) //((e.Alt) && (e.KeyCode == Keys.Enter))
			{
				_swapChain.UpdateSettings(!_swapChain.Settings.IsWindowed);
				if (_swapChain2 != null)
					_swapChain2.UpdateSettings(!_swapChain2.Settings.IsWindowed);
			}
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
				_graphics = new GorgonGraphics(DeviceFeatureLevel.SM4_1);
				//_graphics.IsObjectTrackingEnabled = false;
				//_graphics = new GorgonGraphics();
				//_graphics.ResetFullscreenOnFocus = false;

				_multiSample.IsMultisamplingEnabled = false;
				//_multiSample.CullingMode =  GorgonCullingMode.None;
				_graphics.Rasterizer.States = _multiSample;
				blend1.RenderTarget0.IsBlendingEnabled = true;
				blend1.RenderTarget0.SourceBlend = BlendType.SourceAlpha;
				blend1.RenderTarget0.DestinationBlend = BlendType.InverseSourceAlpha;
				//blend2.RenderTargetStates[0].IsBlendingEnabled = true;
				//blend2.RenderTargetStates[0].SourceBlend = BlendType.SourceAlpha;
				//blend2.RenderTargetStates[0].DestinationBlend = BlendType.InverseSourceAlpha;
				blend2 = blend1;
				blend2.RenderTarget0.DestinationBlend = BlendType.One;
				//blend2.RenderTarget0.WriteMask = ColorWriteMaskFlags.Blue | ColorWriteMaskFlags.Green | ColorWriteMaskFlags.Alpha;
				_graphics.Blending.States = blend1;

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
				int quality = _graphics.VideoDevices[0].GetMultiSampleQuality(mode1.Format, count);
				GorgonMultiSampling multiSample = new GorgonMultiSampling(count, quality - 1);
				_swapChain = _graphics.CreateSwapChain("Swap", new GorgonSwapChainSettings() { Window = this, IsWindowed = true, VideoMode = mode1, MultiSample = multiSample, DepthStencilFormat = BufferFormat.Unknown});
				_graphics.Rasterizer.SetViewport(_swapChain.Viewport);
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
				
				_test1 = new Test(_swapChain);
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

		void form2_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (_test2 != null)
				_test2.Dispose();
			_test2 = null;
			if (_swapChain2 != null)
				_swapChain2.Dispose();
			_swapChain2 = null;
			form2 = null;
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			try
			{
				if (form2 != null)
				{
					form2.Close();
					form2 = null;
				}

				if (_test1 != null)
				{
					_test1.Dispose();
					_test1 = null;
				}
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
			}
		}

		public Form1()
		{
			//this.SetStyle(ControlStyles.AllPaintingInWmPaint, false);
			//this.SetStyle(ControlStyles.UserPaint, false);
			InitializeComponent();
		}
	}
}
