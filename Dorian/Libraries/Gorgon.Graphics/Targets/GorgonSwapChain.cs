#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Saturday, July 30, 2011 1:15:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A swap chain render target that can be attached to a window.
	/// </summary>
	public abstract class GorgonSwapChain
		: GorgonRenderTarget
	{
		#region Variables.
		private bool _disposed = false;			// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the window that contains the <see cref="GorgonLibrary.Graphics.GorgonSwapChain.BoundWindow">BoundWindow</see>.
		/// </summary>
		/// <remarks>If the BoundWindow is a windows form, then this property will be the same as the BoundWindow property.</remarks>
		public Form ParentWindow
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the window that is bound to the swap chain.
		/// </summary>
		public Control BoundWindow
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the SizeChanged event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void BoundWindow_SizeChanged(object sender, EventArgs e)
		{
			RemoveEventHandlers();
			UpdateTargetInformation(new GorgonVideoMode(BoundWindow.ClientSize.Width, BoundWindow.ClientSize.Height, TargetInformation.Format, TargetInformation.RefreshRateNumerator, TargetInformation.RefreshRateDenominator), DepthStencilFormat);
			if (ParentWindow.WindowState != FormWindowState.Minimized)
				UpdateRenderTarget();
			AddEventHandlers();
		}

		/// <summary>
		/// Function to retrieve the parent window for the bound window.
		/// </summary>
		private void GetParentWindow()
		{
			Control parent = null;

			if (BoundWindow is Form)
			{
				ParentWindow = (Form)BoundWindow;
				return;
			}

			parent = BoundWindow.Parent;			

			while (parent != null)
			{
				ParentWindow = parent as Form;
				if (ParentWindow != null)
					break;

				parent = parent.Parent;
			}

			if (ParentWindow == null)
				throw new GorgonException(GorgonResult.CannotCreate, "Could not find the owner window for the bound window.");
		}

		/// <summary>
		/// Function to determine if the active device is ready to render.
		/// </summary>
		/// <returns>TRUE if the device is ready for rendering, FALSE if not.</returns>
		protected abstract bool CanRender();

		/// <summary>
		/// Function to remove any event handlers assigned to the bound window.
		/// </summary>
		protected void RemoveEventHandlers()
		{
			if (BoundWindow != null)
				BoundWindow.SizeChanged -= new EventHandler(BoundWindow_SizeChanged);
		}

		/// <summary>
		/// Function to add any event handlers that need to be assigned to the bound window.
		/// </summary>
		protected void AddEventHandlers()
		{
			if (BoundWindow != null)
				BoundWindow.SizeChanged += new EventHandler(BoundWindow_SizeChanged);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
					RemoveEventHandlers();

				_disposed = true;
			}
		}

		/// <summary>
		/// Function to display the contents of the swap chain.
		/// </summary>
		public abstract void Display();

		/// <summary>
		/// Function to initialize the render target.
		/// </summary>
		internal override void Initialize()
		{
			RemoveEventHandlers();
			base.Initialize();
			AddEventHandlers();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSwapChain"/> class.
		/// </summary>
		/// <param name="graphics">The graphics instance that owns this render target.</param>
		/// <param name="name">The name.</param>
		/// <param name="window">Window to bind the swap chain to.</param>
		/// <param name="mode">A video mode structure defining the width, height and format of the render target.</param>
		/// <param name="depthStencilFormat">The depth buffer format (if required) for the target.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		/// <remarks>Passing <see cref="E:GorgonLibrary.Graphics.GorgonBufferFormat.Unknown">GorgonBufferFormat.Unknown</see> will skip the creation of the depth/stencil buffer.</remarks>
		protected GorgonSwapChain(GorgonGraphics graphics, string name, Control window, GorgonVideoMode mode, GorgonBufferFormat depthStencilFormat)
			: base(graphics, name, mode, depthStencilFormat)	
		{
			if (window == null)
				throw new ArgumentNullException("window");

			BoundWindow = window;
			if (BoundWindow != Gorgon.ApplicationWindow)
				GetParentWindow();
			else
				ParentWindow = Gorgon.ParentWindow;
		}
		#endregion
	}
}
