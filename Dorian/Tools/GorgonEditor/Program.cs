#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Monday, April 30, 2012 6:28:28 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using GorgonLibrary;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.UI;
using GorgonLibrary.FileSystem;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Main application interface.
	/// </summary>
	static class Program
	{
		#region Variables.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the settings for the application.
		/// </summary>
		public static GorgonEditorSettings Settings
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the list of cached fonts on the system.
		/// </summary>
		public static IDictionary<string, Font> CachedFonts
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the graphics interface.
		/// </summary>
		public static GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the renderer for the editor.
		/// </summary>
		public static Gorgon2D Renderer
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize the graphics interface.
		/// </summary>
		/// <param name="control">Main content control.</param>
		public static void InitializeGraphics(Control control)
		{
			if (Renderer != null)
			{
				Renderer.Dispose();
				Renderer = null;
			}

			if (Graphics != null)
			{
				Graphics.Dispose();
				Graphics = null;
			}

			Graphics = new GorgonGraphics(DeviceFeatureLevel.SM2_a_b);
			
			// Create the renderer with a default swap chain.  This is sized to 1x1 to keep from eating
			// video memory since we'll never use this particular swap chain.
			// The down side is that we'll end up having to manage our render targets manually.
			// i.e. setting Target = null won't work because it'll just flip to this 1x1 swap chain.
			Renderer = Graphics.Output.Create2DRenderer(
				Graphics.Output.CreateSwapChain("Content.SwapChain", new GorgonSwapChainSettings()
				{
					BufferCount = 2,
					DepthStencilFormat = BufferFormat.Unknown,
					Flags = SwapChainUsageFlags.RenderTarget,
					Format = BufferFormat.R8G8B8A8_UIntNormal,
					MultiSample = new GorgonMultisampling(1, 0),
					SwapEffect = SwapEffect.Discard,
					Window = control
				}));
		}

		/// <summary>
		/// Function to update the font cache.
		/// </summary>
		static void UpdateCachedFonts()
		{
			SortedDictionary<string, Font> fonts = null;

			// Clear the cached fonts.
			if (CachedFonts != null)
			{
				foreach (var font in CachedFonts)
					font.Value.Dispose();
			}

			fonts = new SortedDictionary<string,Font>();

			// Get font families.
			foreach (var family in FontFamily.Families)
			{
				Font newFont = null;

				if (!fonts.ContainsKey(family.Name))
				{
					if (family.IsStyleAvailable(FontStyle.Regular))
						newFont = new Font(family, 16.0f, FontStyle.Regular, GraphicsUnit.Pixel);
					else
					{
						if (family.IsStyleAvailable(FontStyle.Bold))
							newFont = new Font(family, 16.0f, FontStyle.Bold, GraphicsUnit.Pixel);
						else
						{
							if (family.IsStyleAvailable(FontStyle.Italic))
								newFont = new Font(family, 16.0f, FontStyle.Italic, GraphicsUnit.Pixel);
						}
					}

					// Only add if we could use the regular, bold or italic style.
					if (newFont != null)
						fonts.Add(family.Name, newFont);
				}
			}

			CachedFonts = fonts;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="Program"/> class.
		/// </summary>
		static Program()
		{
			Settings = new GorgonEditorSettings();
		}
		#endregion

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

				Settings.Load();
								
				Gorgon.Run(new AppContext());
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
			}
			finally
			{
				// Get rid of shared resources.
				if (Renderer != null)
				{
					Renderer.Dispose();
					Renderer = null;
				}

				// Shut down the graphics interface.
				if (Graphics != null)
				{
					Graphics.Dispose();
					Graphics = null;
				}

				// Clear the cached fonts.
				if (CachedFonts != null)
				{
					foreach (var font in CachedFonts)
						font.Value.Dispose();
				}
			}
		}
	}
}
