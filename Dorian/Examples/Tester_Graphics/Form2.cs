using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SlimMath;
using GorgonLibrary;
using GorgonLibrary.UI;
using GorgonLibrary.Graphics;

namespace Tester_Graphics
{
	public partial class Form2 : Form
	{
		private GorgonGraphics _graphics = null;
		private GorgonSwapChain _swap = null;

		private bool Idle()
		{
			_swap.Clear(Color.White);

			_swap.Flip();
			return true;
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
