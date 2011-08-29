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
// Created: Sunday, August 28, 2011 5:04:39 PM
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
	/// Values for setting up a <see cref="GorgonLibrary.Graphics.GorgonSwapChain">Gorgon Swap Chain</see>.
	/// </summary>
	public class GorgonSwapChainSettings
		: GorgonRenderTargetSettings
	{
		#region Variables.
		private int _backBufferCount = 0;		// Back buffer count.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the window that the swap chain is bound with.
		/// </summary>
		public Control BoundWindow
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the top-level form for the BoundWindow.
		/// </summary>
		/// <remarks>This will return the form containing the control that the swap chain is bound with.</remarks>
		public Form BoundForm
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the number of back buffers in the swap chain.
		/// </summary>
		public int BackBufferCount
		{
			get
			{
				return _backBufferCount;
			}
			set
			{
				if (value < 1)
					value = 1;

				_backBufferCount = value;
			}
		}

		/// <summary>
		/// Property to set or return the display function used when displaying the contents of the swap chain.
		/// </summary>
		public GorgonDisplayFunction DisplayFunction
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the vertical sync interval.
		/// </summary>
		/// <remarks>This value will GorgonVSyncInterval.None in windowed mode regardless of what's been set here.</remarks>
		public GorgonVSyncInterval VSyncInterval
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the flag to indicate that we will be using this device window for video.
		/// </summary>
		public bool WillUseVideo
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSwapChainSettings"/> class.
		/// </summary>
		protected GorgonSwapChainSettings()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSwapChainSettings"/> class.
		/// </summary>
		/// <param name="boundWindow">The window bound to the swap chain.</param>
		public GorgonSwapChainSettings(Control boundWindow)
		{
			if (boundWindow == null)
				throw new ArgumentNullException("The settings for a device window or swap chain require a window.", "boundWindow");

			if (BackBufferCount < 1)
				BackBufferCount = 2;
			BoundWindow = boundWindow;
			BoundForm = BoundWindow as Form;

			if (BoundForm == null)
				BoundForm = Gorgon.GetTopLevelForm(BoundWindow);
		
		
		}
		#endregion
	}
}
