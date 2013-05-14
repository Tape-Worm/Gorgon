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
// Created: Wednesday, October 12, 2011 6:57:29 PM
// 
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using GI = SharpDX.DXGI;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Usage flags for the swap chain.
	/// </summary>
	[Flags()]
	public enum SwapChainUsageFlags
	{
		/// <summary>
		/// Specifies that the swap chain is used to display its data.
		/// </summary>
		RenderTarget = 1,
		/// <summary>
		/// Specifies that the swap chain is used as a shader input.
		/// </summary>
		ShaderInput = 2
	}

	/// <summary>
	/// Values to determine how data is displayed to the front buffer of a swap chain.
	/// </summary>
	public enum SwapEffect
	{
		/// <summary>
		/// Back buffer data will be discarded when the data is displayed.
		/// </summary>
		/// <remarks>The swap chain must have more than 1 back buffer to use this.</remarks>
		Discard = 0,
		/// <summary>
		/// Back buffer data will not be discard when the data is displayed.
		/// </summary>
		/// <remarks>Use this to display the back buffer data in order from the first buffer, to the last.  This cannot be used on swap chains that have multisampling enabled.</remarks>
		Sequential = 1
	}

    /// <summary>
    /// Settings for a render target.
    /// </summary>
    public interface IRenderTargetSettings
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the width of the render target.
        /// </summary>
        int Width
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the height of the render target.
        /// </summary>
        /// <remarks>For 2D and 3D render targets only.</remarks>
        int Height
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the depth of the render target.
        /// </summary>
        /// <returns>For 3D render targets only.  For other target types, this value will return 1.</returns>
        int Depth
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the format for the render target.
        /// </summary>
        BufferFormat Format
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the format used to access the render target in a shader.
        /// </summary>
        /// <remarks>If this value is set to Unknown, then the <see cref="GorgonLibrary.Graphics.IRenderTargetSettings.Format">Format</see> property will be used.
        /// <para>If the render target type is a <see cref="GorgonLibrary.Graphics.GorgonSwapChain">GorgonSwapChain</see>, then setting this value to anything other 
        /// than Unknown will enable the swap chain front buffer to be read in a shader.</para>
        /// <para>The default value is Unknown.</para>
        /// </remarks>
        BufferFormat ShaderViewFormat
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the un
        /// </summary>
        BufferFormat UnorderedAccessViewFormat
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the format used by the default depth/stencil buffer.
        /// </summary>
        /// <remarks>If this value is set to Unknown, then no default depth/stencil will be created for the render target.  
        /// <para>If a more complex depth/stencil (e.g. a depth/stencil with shader access) is required, then leave this value set to Unknown, 
        /// create a new <see cref="GorgonLibrary.Graphics.GorgonDepthStencil">GorgonDepthStencil</see> buffer and attach it to the render 
        /// targets <see cref="GorgonLibrary.Graphics.GorgonRenderTarget.DepthStencil">DepthStencil</see> property.</para>
        /// </remarks>
        BufferFormat DepthStencilFormat
        {
            get;
            set;
        }
        #endregion
    }

	/// <summary>
	/// Settings for defining a render target.
	/// </summary>
	public class GorgonRenderTargetSettings
	{
		#region Variables.
		private GorgonVideoMode _mode = default(GorgonVideoMode);			// Gorgon video mode.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the width and height of the target.
		/// </summary>
		public Size Size
		{
			get
			{
				return _mode.Size;
			}
			set
			{
				_mode.SetSize(value);
			}
		}

		/// <summary>
		/// Property to set or return the format of the back buffer for the target.
		/// </summary>
		public BufferFormat Format
		{
			get
			{
				return _mode.Format;
			}
			set
			{
				_mode.Format = value;
			}
		}

		/// <summary>
		/// Property to set or return the width of the target.
		/// </summary>
		public int Width
		{
			get
			{
				return _mode.Width;
			}
			set
			{
				_mode.SetSize(value, _mode.Height);
			}
		}

		/// <summary>
		/// Property to set or return the height of the target.
		/// </summary>
		public int Height
		{
			get
			{
				return _mode.Height;
			}
			set
			{
				_mode.SetSize(_mode.Width, value);
			}
		}

		/// <summary>
		/// Property to set or return the multisampling settings for the target.
		/// </summary>
		public GorgonMultisampling MultiSample
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the video mode to use.
		/// </summary>
		/// <remarks>For swap chains -ONLY-: leaving the width, height or format undefined (i.e. 0, 0, or Unknown) will tell Gorgon to find the best video mode based on the window dimensions and desktop format.</remarks>
		public GorgonVideoMode VideoMode
		{
			get
			{
				return _mode;
			}
			set
			{
				_mode = value;
			}
		}
		
		/// <summary>
		/// Property to set or return the depth and/or stencil buffer format.
		/// </summary>
		/// <remarks>Setting this value to Unknown will create the target without a depth buffer.  The default value is Unknown.</remarks>
		public BufferFormat DepthStencilFormat
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTargetSettings"/> class.
		/// </summary>
		public GorgonRenderTargetSettings()
		{
			Format = BufferFormat.Unknown;
			DepthStencilFormat = BufferFormat.Unknown;
			MultiSample = new GorgonMultisampling(1, 0);
		}
		#endregion
	}

	/// <summary>
	/// Settings for defining a swap chain.
	/// </summary>
	public class GorgonSwapChainSettings
		: GorgonRenderTargetSettings
	{
		#region Variables.
		private int _backBufferCount = 2;									// Number of back buffers.		
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether the client area of the window should stay in sync with the swap chain back buffer size.
		/// </summary>
		/// <remarks>
		/// Set this to TRUE to tell the swap chain to -not- resize the client window when the swap chain back buffer is not the same size.
		/// <para>This is only applied when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Window">Window</see> property is set to a windows form object, otherwise it is ignored.</para>
		/// <para>The default value is FALSE.</para></remarks>
		public bool NoClientResize
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the window that is bound with the swap chain.
		/// </summary>
		/// <remarks>Leaving this value as NULL (Nothing in VB.Net) will use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon default application window.</see></remarks>
		public Control Window
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to use windowed mode or not.
		/// </summary>
		public bool IsWindowed
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return usage flags for the swap chain.
		/// </summary>
		/// <remarks>This will default to SwapChainUsageFlags.RenderTarget if not defined.
		/// <para>If the current video device is a SM2_a_b video device, then this can only be set to RenderTarget, any other combination will not work and will throw an exception upon creation.</para>
		/// </remarks>
		public SwapChainUsageFlags Flags
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of back buffers to use.
		/// </summary>
		/// <remarks>This value defaults to 2 if not specified.</remarks>
		public int BufferCount
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
		/// Property to set or return the effect used when displaying data in the swap chain to the front buffer.
		/// </summary>
		/// <remarks>This value defaults to SwapEffect.Discard if not specified.
		/// <para>Note that multisampling cannot be used on the swap chain if SwapEffect.Sequential is used.</para>
		/// </remarks>
		public SwapEffect SwapEffect
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to convert a Gorgon swap effect into a D3D swap effect.
		/// </summary>
		/// <param name="swapEffect">Swap effect to convert.</param>
		/// <returns>The D3D swap effect.</returns>
		internal static GI.SwapEffect Convert(SwapEffect swapEffect)
		{
			return swapEffect == SwapEffect.Discard ? GI.SwapEffect.Discard : GI.SwapEffect.Sequential;
		}

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSwapChainSettings"/> class.
		/// </summary>
		public GorgonSwapChainSettings()
		{
			IsWindowed = true;			
			Flags = SwapChainUsageFlags.RenderTarget;
			SwapEffect = Graphics.SwapEffect.Discard;
			MultiSample = new GorgonMultisampling(1, 0);
			DepthStencilFormat = BufferFormat.Unknown;			
		}
		#endregion
	}
}
