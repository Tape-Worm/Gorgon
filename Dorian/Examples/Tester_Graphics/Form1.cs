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
		GorgonRasterizerState _multiSample = null;
		GorgonTimer _timer = new GorgonTimer(true);
		Graphics g = null;
		Form2 form2 = null;
		Image backBuffer = null;
		PointF pos = PointF.Empty;
		

		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnDoubleClick(e);
			if (Gorgon.ApplicationIdleLoopMethod == Idle)
			{
				backBuffer = new Bitmap(this.ClientSize.Width, this.ClientSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

				float aspect = (float)Properties.Resources.Haiku.Height / (float)Properties.Resources.Haiku.Width;
				pos = this.PointToScreen(Point.Round(new PointF(backBuffer.Width / 4, (backBuffer.Height / 2) - (((backBuffer.Height / 2) * aspect) / 2))));

				Gorgon.ApplicationIdleLoopMethod = Idle2;
			}
			else
			{
				backBuffer.Dispose();
				Gorgon.ApplicationIdleLoopMethod = Idle;
			}
		}

		private bool Idle2(GorgonFrameRate timing)
		{
			Text = "FPS: " + timing.FPS.ToString() + " DT:" + timing.FrameDelta.ToString();

			g = Graphics.FromImage(backBuffer);
			g.Clear(Color.Purple);
			g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
			g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			float aspect = (float)Properties.Resources.Haiku.Height / (float)Properties.Resources.Haiku.Width;
			g.DrawEllipse(Pens.Yellow, new Rectangle(backBuffer.Width / 2 - backBuffer.Width / 4, backBuffer.Height / 2 - backBuffer.Height / 4, backBuffer.Width / 2, backBuffer.Height / 2));
			g.DrawImage(Properties.Resources.Haiku, new RectangleF(Point.Round(pos).X - Location.X, Point.Round(pos).Y - Location.Y, backBuffer.Width / 2, (backBuffer.Height / 2) * aspect));
			g.Dispose();

			g = this.CreateGraphics();
			g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
			g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			g.DrawImage(backBuffer, new RectangleF(0, 0, ClientSize.Width, ClientSize.Height), new RectangleF(0, 0, backBuffer.Width, backBuffer.Height), GraphicsUnit.Pixel);			
			g.Dispose();			
			
			return true;
		}

		int frameCount = 0;
		float frameDepth = 0.0f;

		private bool Idle(GorgonFrameRate timing)
		{			
			Text = "FPS: " + timing.FPS.ToString() + " DT:" + timing.FrameDelta.ToString();

			try
			{
				if (!_swapChain.IsInStandBy)
				{
					_graphics.Draw();
					//_graphics.ApplyStates();
					//_graphics.ApplyViewports();

					_swapChain.DepthStencil.Clear(1.0f, 0);

					//if (frameCount == 0)
					{
						_swapChain.Clear(Color.Black);
						frameCount = 0;
					}

					if (_test1 != null)
					{
						_test1.Transform(timing.FrameDelta);
						_test1.Draw();
					}


					//_graphics.ApplyViewport(1);
					//_test1.Draw();

					if (_test2 != null)
						_test2.Draw();
				}

				_swapChain.Flip();

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

			if (e.KeyCode == Keys.F1) //((e.Alt) && (e.KeyCode == Keys.Enter))
			{
				_swapChain.UpdateSettings(!_swapChain.Settings.IsWindowed);
				if (_swapChain2 != null)
					_swapChain2.UpdateSettings(!_swapChain2.Settings.IsWindowed);
			}
		}			

		protected override void OnLoad(EventArgs e)
		{			
			base.OnLoad(e);

			try
			{
				this.panelDX.Visible = false;

				GorgonFrameRate.UseHighResolutionTimer = false;

				Gorgon.UnfocusedSleepTime = 1000;
				Gorgon.AllowBackground = true;

				this.Show();

				ClientSize = new System.Drawing.Size(640, 480);

#if MULTIMON
				form2 = new Form2();
				form2.FormClosing += new FormClosingEventHandler(form2_FormClosing);
				form2.Show();
#endif
				_graphics = new GorgonGraphics(DeviceFeatureLevel.SM2_a_b);
				//_graphics = new GorgonGraphics();

				_multiSample = _graphics.CreateRasterizerState();
				_multiSample.IsMultisamplingEnabled = true;
				_graphics.RasterizerState = _multiSample;
 
				mode1 = (from videoMode in _graphics.VideoDevice.Outputs[0].VideoModes
						 where videoMode.Width == 1280 && videoMode.Height == 800 && videoMode.Format == GorgonBufferFormat.R8G8B8A8_UIntNormal 
						 orderby videoMode.RefreshRateNumerator descending, videoMode.RefreshRateDenominator descending
						 select videoMode).First();

				int count = 2;
				int quality = _graphics.VideoDevices[0].GetMultiSampleQuality(GorgonBufferFormat.R8G8B8A8_UIntNormal, count);
				GorgonMultiSampling multiSample = new GorgonMultiSampling(count, quality - 1);
				_swapChain = _graphics.CreateSwapChain("Swap", new GorgonSwapChainSettings() { Window = this, IsWindowed = true, VideoMode = mode1, MultiSample = multiSample, DepthStencilFormat = GorgonBufferFormat.D24_UIntNormal_S8_UInt });
				//_graphics.RasterizerState.IsMultisamplingEnabled = true;
				_graphics.Viewports.Add(_swapChain.Viewport);
				//_graphics.Viewports.Add(new GorgonViewport(640, 400, 640, 400));
#if MULTIMON
				form2.Location = _graphics.VideoDevices[0].Outputs[1].OutputBounds.Location;

				mode2 = (from videoMode in _graphics.VideoDevices[0].Outputs[1].VideoModes
						 where videoMode.Width == 1024 && videoMode.Height == 768 && videoMode.Format == GorgonBufferFormat.R8G8B8A8_UIntNormal_sRGB
						 orderby videoMode.RefreshRateNumerator, videoMode.RefreshRateDenominator
						 select videoMode).First();

				_swapChain2 = _graphics.CreateSwapChain("Swap2", new GorgonSwapChainSettings() { IsWindowed = true, Window = form2, VideoMode = mode2 });
				this.Focus();
#endif

				//_swapChain.UpdateSettings(false);
#if MULTIMON
				_swapChain2.UpdateSettings(false);				
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
