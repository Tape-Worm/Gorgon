#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Monday, March 2, 2015 11:57:22 PM
// 
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Editor.Properties;
using Gorgon.Graphics;
using Gorgon.Math;
using Gorgon.Renderers;

namespace Gorgon.Editor
{
	/// <summary>
	/// A 2D renderer for the editor content.
	/// </summary>
	/// <remarks>
	/// For content that requires the use of Gorgon's 2D renderer interface, this abstract class will provide the ability to 
	/// perform per-content custom rendering.
	/// </remarks>
	public abstract class EditorContentRenderer2D 
		: IEditorContentRenderer
	{
		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		// Presentation interval.
		private int _interval;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the graphics interface for the renderer.
		/// </summary>
		protected GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the 2D renderer interface
		/// </summary>
		protected Gorgon2D Renderer
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the primary swap chain for the control.
		/// </summary>
		protected GorgonSwapChain SwapChain
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the BeforeResizeEvent event of the SwapChain control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void SwapChain_BeforeResizeEvent(object sender, EventArgs eventArgs)
		{
			OnBeforeSwapChainResize();
		}

		/// <summary>
		/// Handles the AfterResizeEvent event of the SwapChain control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="gorgonAfterSwapChainResizedEventArgs">The <see cref="GorgonAfterSwapChainResizedEventArgs"/> instance containing the event data.</param>
		private void SwapChain_AfterResizeEvent(object sender, GorgonAfterSwapChainResizedEventArgs gorgonAfterSwapChainResizedEventArgs)
		{
			OnAfterSwapChainResize(gorgonAfterSwapChainResizedEventArgs);
		}

		/// <summary>
		/// Function to clean up any resources provided to the renderer.
		/// </summary>
		protected abstract void OnCleanUpResources();

		/// <summary>
		/// Function to create any resources that may be required by the renderer.
		/// </summary>
		protected abstract void OnCreateResources();

		/// <summary>
		/// Function called before the swap chain is resized.
		/// </summary>
		/// <remarks>Use this method to unbind resources before the swap chain is resized.</remarks>
		protected virtual void OnBeforeSwapChainResize()
		{
			// Nothing here in base.
		}

		/// <summary>
		/// Function called after the swap chain is resized.
		/// </summary>
		/// <param name="args">Arguments for the resize event.</param>
		/// <remarks>Use this method to rebind and reinitialize (if necessary) items when the swap chain is finished resizing.</remarks>
		protected virtual void OnAfterSwapChainResize(GorgonAfterSwapChainResizedEventArgs args)
		{
			// Nothing here in base.
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorContentRenderer2D"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface for the renderer.</param>
		/// <param name="content">The content object.</param>
		protected EditorContentRenderer2D(GorgonGraphics graphics, IContentData content)
		{
			if (graphics == null)
			{
				throw new ArgumentNullException("graphics");
			}

			if (content == null)
			{
				throw new ArgumentNullException("content");
			}

			Graphics = graphics;
			Content = content;
			RenderInterval = 1;
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="EditorContentRenderer2D"/> class.
		/// </summary>
		~EditorContentRenderer2D()
		{
			Dispose(false);
		}
		#endregion

		#region IContentRenderer
		#region Properties.
		/// <summary>
		/// Property to return the content to be rendered.
		/// </summary>
		public IContentData Content
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the presentation interval for the renderer.
		/// </summary>
		/// <remarks>Use this to control the vsync for the rendering.</remarks>
		public int RenderInterval
		{
			get
			{
				return _interval;
			}
			set
			{
				_interval = value.Max(0).Min(4);
			}
		}

		/// <summary>
		/// Property to set or return the color to use when clearing the render surface.
		/// </summary>
		/// <remarks>Set this value to NULL (Nothing in VB.Net) to use the control background color.</remarks>
		public GorgonColor? ClearColor
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform the actual rendering of graphics to the control surface.
		/// </summary>
		public abstract void Draw();

		/// <summary>
		/// Function to clear all resources for this renderer.
		/// </summary>
		public void ClearResources()
		{
			StopRendering();

			if (Renderer != null)
			{
				Renderer.Dispose();
				Renderer = null;
			}

			if (SwapChain != null)
			{
				SwapChain.AfterSwapChainResized -= SwapChain_AfterResizeEvent;
				SwapChain.BeforeSwapChainResized -= SwapChain_BeforeResizeEvent;
				SwapChain.Dispose();
				SwapChain = null;
			}

			OnCleanUpResources();
		}

		/// <summary>
		/// Function to stop any rendering.
		/// </summary>
		public void StopRendering()
		{
			GorgonApplication.ApplicationIdleLoopMethod = null;
		}

		/// <summary>
		/// Function to start rendering.
		/// </summary>
		public void StartRendering()
		{
			GorgonApplication.ApplicationIdleLoopMethod = () =>
			                                   {
												   // Reset the render target.
				                                   Renderer.Target = SwapChain;

												   // Assume the background color for the control that we're rendering into.
				                                   Renderer.Clear(ClearColor ?? SwapChain.Settings.Window.BackColor);

				                                   Draw();

												   Renderer.Render(RenderInterval);
				                                   return true;
			                                   };
		}

		/// <summary>
		/// Function to reset the resources used by the renderer.
		/// </summary>
		/// <remarks>
		/// This method can be used to reset the state to reflect a potential content change, or just to restart the renderer. If 
		/// resetting due to a content change, be aware that this method carries a lot of overhead and will impair performance.
		/// </remarks>
		public void ResetResources()
		{
			StopRendering();
			OnCleanUpResources();
			OnCreateResources();
			StartRendering();
		}

		/// <summary>
		/// Function to create the necessary resources for the renderer.
		/// </summary>
		/// <param name="renderControl">Control to render into.</param>
		public void CreateResources(Control renderControl)
		{
			if (Graphics == null)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GOREDIT_ERR_NO_GFX_FOUND);
			}

			if (renderControl == null)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GOREDIT_ERR_CONTENT_UI_NOT_A_CONTROL);
			}

			if ((SwapChain != null)
			    && (Renderer != null))
			{
				ClearResources();
			}
			
			SwapChain = Graphics.Output.CreateSwapChain(Content.Name + " Swap Chain",
			                                            new GorgonSwapChainSettings
			                                            {
															Format = BufferFormat.R8G8B8A8_UIntNormal,
															Window = renderControl,
															IsWindowed = true
			                                            });
			
			Renderer = Graphics.Output.Create2DRenderer(SwapChain);

			// Force alpha blending on for primitive drawing.
			Renderer.Drawing.BlendingMode = BlendingMode.Modulate;

			SwapChain.AfterSwapChainResized += SwapChain_AfterResizeEvent;
			SwapChain.BeforeSwapChainResized += SwapChain_BeforeResizeEvent;

			OnCreateResources();
		}
		#endregion
		#endregion

		#region IDisposable Implementation.
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				ClearResources();
			}

			Renderer = null;
			SwapChain = null;
			_disposed = true;
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
