using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using SlimMath;
using GorgonLibrary;
using GorgonLibrary.Math;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace Test_TextureArray
{
	static class Program
	{
		private static Form1 _form = null;
		private static GorgonGraphics _graphics = null;
		private static Gorgon2D _2D = null;
		private static GorgonSwapChain _swap = null;
		private static float _startTime = 0;

		private static bool Idle()
		{
			_2D.Clear(Color.Blue);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Time since startup: " + GorgonTiming.SecondsSinceStart.ToString("0.0") + " seconds", new Vector2(0, 0), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "FPS: " + GorgonTiming.FPS.ToString("0.0"), new Vector2(0, 16), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Frame Delta: " + (GorgonTiming.FrameDelta * 1000.0f).ToString("0.0##") + " ms", new Vector2(0, 32), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Avg. FPS: " + GorgonTiming.AverageFPS.ToString("0.0"), new Vector2(0, 48), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Avg. Frame Delta: " + (GorgonTiming.AverageFrameDelta * 1000.0f).ToString("0.0##") + " ms", new Vector2(0, 64), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Frame Count: " + GorgonTiming.FrameCount.ToString("0"), new Vector2(0, 80), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Highest FPS: " + GorgonTiming.HighestFPS.ToString("0.0"), new Vector2(0, 96), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Lowest FPS: " + GorgonTiming.LowestFPS.ToString("0.0"), new Vector2(0, 112), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "VB Count: " + GorgonRenderStatistics.VertexBufferCount.ToString() + " (" + GorgonRenderStatistics.VertexBufferSize.FormatMemory() + ")", new Vector2(0, 144), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "IB Count: " + GorgonRenderStatistics.IndexBufferCount.ToString() + " (" + GorgonRenderStatistics.IndexBufferSize.FormatMemory() + ")", new Vector2(0, 160), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "CB Count: " + GorgonRenderStatistics.ConstantBufferCount.ToString() + " (" + GorgonRenderStatistics.ConstantBufferSize.FormatMemory() + ")", new Vector2(0, 176), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Texture Count: " + GorgonRenderStatistics.TextureCount.ToString() + " (" + GorgonRenderStatistics.TextureSize.FormatMemory() + ")", new Vector2(0, 192), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "RT Count: " + GorgonRenderStatistics.RenderTargetCount.ToString() + " (" + GorgonRenderStatistics.RenderTargetSize.FormatMemory() + ")", new Vector2(0, 208), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Depth Count: " + GorgonRenderStatistics.DepthBufferCount.ToString() + " (" + GorgonRenderStatistics.DepthBufferSize.FormatMemory() + ")", new Vector2(0, 224), Color.White);

			_startTime = GorgonTiming.SecondsSinceStart;

			GorgonRenderStatistics.EndFrame();

			_2D.Render(false);

			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, "Draw calls: " + GorgonRenderStatistics.DrawCallCount.ToString(), new Vector2(0, 128), Color.White);
			_2D.Render();

			return true;
		}

		private static void Initialize()
		{
			_form = new Form1();
			_form.Show();
			_form.FormClosing += new FormClosingEventHandler(_form_FormClosing);

			_graphics = new GorgonGraphics();
			_swap = _graphics.Output.CreateSwapChain("Screen", new GorgonSwapChainSettings()
					{
						Window = _form,
						Width = 1280,
						Height = 800,
						DepthStencilFormat = BufferFormat.D24_UIntNormal_S8_UInt,
						IsWindowed = false
					});
			_2D = _graphics.Output.Create2DRenderer(_swap);
			_2D.IsLogoVisible = true;
			_startTime = GorgonTiming.SecondsSinceStart;
		}

		static void _form_FormClosing(object sender, FormClosingEventArgs e)
		{
			Gorgon.Quit();
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Initialize();
				Gorgon.Run(Idle);
			}
			finally
			{
				if (_graphics != null)
					_graphics.Dispose();
			}
		}
	}
}
