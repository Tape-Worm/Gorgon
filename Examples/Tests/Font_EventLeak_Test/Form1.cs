using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.UI;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace Font_EventLeak_Test
{
	/// <summary>
	/// A test to ensure that the event leak for GorgonFont is fixed.
	/// 
	/// From issue https://github.com/Tape-Worm/Gorgon/issues/3
	/// </summary>
	public partial class Form1 : Form
	{
		private GorgonGraphics _graphics;
		private GorgonSwapChain _swap;
		private Gorgon2D _2d;
		private GorgonFont _font;
		private GorgonTextDebug _debug = new GorgonTextDebug();
		private int _textCount = 0;
				
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (_font != null)
			{
				_font.Dispose();
			}

			if (_2d != null)
			{
				_2d.Dispose();
			}

			if (_swap != null)
			{
				_swap.Dispose();
			}

			if (_graphics != null)
			{
				_graphics.Dispose();
			}
		}

		private bool Idle()
		{
			if (GorgonTiming.SecondsSinceStart > 30)
			{
				GorgonDialogs.InfoBox(this, string.Format("{0} text objects created.  None survived.", _textCount));
				return false;
			}

			++_textCount;
			// This is for testing, for the love of all that is good and holy, do NOT do this in your code.
			GorgonText text = _2d.Renderables.CreateText("Test", _font, string.Format("This is a test.  When object #{0} goes out of scope, the event should unbind because of WeakEventManager.", _textCount));
			

			_swap.Clear(Color.CornflowerBlue);

			text.Position = new SlimMath.Vector2(50, 50);
			text.Color = Color.Yellow;
			text.Draw();

			if (_debug.Objects.Count > 512)
			{
				_debug.GcTest();
			}


			_debug.Objects.Add(new WeakReference<GorgonText>(text));

			_2d.Render();
			_swap.Flip(1);

			return true;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			Visible = true;
			ClientSize = new Size(1280, 720);

			_graphics = new GorgonGraphics();
			_swap = _graphics.Output.CreateSwapChain("Swap", new GorgonSwapChainSettings
			{
				BufferCount = 2,
				IsWindowed = true,
				Format = BufferFormat.R8G8B8A8_UIntNormal,
				Width = 1280,
				Height = 720,
				Window = this
			});

			_font = _graphics.Fonts.CreateFont("FontTest", new GorgonFontSettings
			{
				FontFamilyName = "Segoe UI",
				FontHeightMode = FontHeightMode.Pixels,
				Size = 12.0f
			});

			_2d = _graphics.Output.Create2DRenderer(_swap);
			_2d.Begin2D();
			

			Gorgon.ApplicationIdleLoopMethod = Idle;
		}

		public Form1()
		{
			InitializeComponent();
		}
	}
}
