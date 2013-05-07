using System;
using System.Drawing;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.Graphics;

namespace Tester_Graphics
{
	public partial class Form3 : Form
	{
		private GorgonGraphics _gfx = null;
		private GorgonSwapChain _swap = null;

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.KeyCode == Keys.F1)
			{
				_swap.UpdateSettings(!_swap.Settings.IsWindowed);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			_gfx = new GorgonGraphics();
			_swap = _gfx.Output.CreateSwapChain("Test", new GorgonSwapChainSettings()
					{
						Width = 1024,
						Height = 768,
						Format = BufferFormat.R8G8B8A8_UIntNormal,
						IsWindowed = false
					});

			Gorgon.ApplicationIdleLoopMethod = () =>
				{
					_swap.Clear(Color.Blue);

					_swap.Flip();
					return true;
				};

/*			try
			{
				GorgonVideoDeviceEnumerator.Enumerate(false, false);

			}
			catch (Exception ex)
			{
			}*/
		}

		public Form3()
		{
			InitializeComponent();
		}
	}
}
