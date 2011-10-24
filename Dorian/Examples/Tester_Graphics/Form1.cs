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
		GorgonTimer _timer = new GorgonTimer(true);
		Form2 form2 = null;
		

		private bool Idle(GorgonFrameRate timing)
		{			
			Text = "FPS: " + timing.FPS.ToString() + " DT:" + timing.FrameDelta.ToString();

			if (_test1 != null)
				_test1.Run();

			if (_test2 != null)
				_test2.Run();

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
				Gorgon.Initialize(this);

				Gorgon.PlugIns.SearchPaths.Add(@"..\..\..\..\PlugIns\bin\debug");

				GorgonFrameRate.UseHighResolutionTimer = false;

				Gorgon.UnfocusedSleepTime = 1;
				Gorgon.AllowBackground = true;

				this.Show();

				ClientSize = new System.Drawing.Size(640, 480);

#if MULTIMON
				form2 = new Form2();
				form2.FormClosing += new FormClosingEventHandler(form2_FormClosing);
				form2.Show();
#endif
				_graphics = new GorgonGraphics(DeviceFeatureLevel.Level10_1_SM4);
				//_graphics = new GorgonGraphics();
 
				mode1 = (from videoMode in _graphics.VideoDevices[0].Outputs[0].VideoModes
						 where videoMode.Width == 1024 && videoMode.Height == 768 && videoMode.Format == GorgonBufferFormat.B8G8R8A8_UIntNormal
						 orderby videoMode.RefreshRateNumerator descending, videoMode.RefreshRateDenominator descending
						 select videoMode).First();

				int count = 2;
				int quality = _graphics.VideoDevices[0].GetMultiSampleQuality(GorgonBufferFormat.B8G8R8A8_UIntNormal, count);
				GorgonMultiSampling multiSample = new GorgonMultiSampling(count, quality - 1);
				_swapChain = _graphics.CreateSwapChain("Swap", new GorgonSwapChainSettings() { IsWindowed = true, VideoMode = mode1, MultiSample = multiSample });
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
				Gorgon.Go(Idle);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
				Application.Exit();				
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
				Gorgon.Terminate();
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
			}
		}

		public Form1()
		{
			InitializeComponent();
		}
	}
}
