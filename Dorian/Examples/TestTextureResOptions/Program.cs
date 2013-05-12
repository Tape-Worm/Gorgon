using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.Graphics;

namespace TestTextureResOptions
{
	static class Program
	{
		private static Form1 _form;

		private static void Init()
		{
			_form = new Form1();

			var gfx = new GorgonGraphics();

			var test1D = gfx.Textures.CreateTexture<GorgonTexture1D>("Test", new GorgonTexture1DSettings()
				{
					Width = 512,
					Format = BufferFormat.R8_Int,
					ArrayCount = 1,
					MipCount = 1,
					Usage = BufferUsage.Default,
					UnorderedAccessViewFormat = BufferFormat.R8_Int
				});

			test1D.Dispose();

			var test2D = gfx.Textures.CreateTexture<GorgonTexture2D>("Test2", new GorgonTexture2DSettings()
				{
					Width = 512,
					Height = 512,
                    Format = BufferFormat.R16G16,
					ArrayCount = 6,
					MipCount = 1,
					Usage = BufferUsage.Default,
					IsTextureCube = true,
					ShaderViewFormat = BufferFormat.R16G16_Int,
					UnorderedAccessViewFormat = BufferFormat.R32_UInt,
				});
			
			test2D.Dispose();

			var test3D = gfx.Textures.CreateTexture<GorgonTexture3D>("Test3", new GorgonTexture3DSettings()
			{
				Width = 512,
				Height = 512,
				Depth = 32,
				Format = BufferFormat.R8G8B8A8_Int,
				MipCount = 1,
				Usage = BufferUsage.Default,
				UnorderedAccessViewFormat = BufferFormat.R8G8B8A8_Int,
			});

			test3D.Dispose();

			var testDS = gfx.Output.CreateDepthStencil("DSTest", new GorgonDepthStencilSettings()
				{
					Width = 512,
					Height = 512,
					Format = BufferFormat.D24_UIntNormal_S8_UInt,
					ShaderViewFormat = BufferFormat.R32_Float
				});

			testDS.Dispose();
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			Init();

			Gorgon.Run(_form);
		}
	}
}
