#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Tuesday, June 4, 2013 10:22:04 PM
// 
#endregion

using Gorgon.Core;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A render target bound to a buffer.
	/// </summary>
	public class GorgonRenderTargetBuffer
		: GorgonBuffer
    {
        #region Properties.
        /// <summary>
        /// Property to return the default render target view for the buffer.
        /// </summary>
	    public GorgonRenderTargetView DefaultRenderTargetView
	    {
	        get;
	        private set;
	    }

		/// <summary>
		/// Property to return the settings for this render target.
		/// </summary>
		public new GorgonRenderTargetBufferSettings Settings
		{
			get;
		}
		#endregion

        #region Methods.
        /// <summary>
        /// Function to create a default render target view.
        /// </summary>
	    protected override void OnCreateDefaultRenderTargetView()
	    {
            var info = new GorgonBufferFormatInfo(Settings.DefaultShaderViewFormat);
            DefaultRenderTargetView = OnGetRenderTargetView(Settings.Format, Settings.Format, 0, Settings.SizeInBytes / info.SizeInBytes);
        }

        /// <summary>
        /// Function to retrieve a render target view.
        /// </summary>
        /// <param name="format">Format of the new render target view.</param>
        /// <param name="firstElement">The first element in the buffer to map to the view.</param>
        /// <param name="elementCount">The number of elements in the buffer to map to the view.</param>
        /// <returns>A render target view.</returns>
        /// <remarks>Use this to create/retrieve a render target view that can bind a portion of the target to the pipeline as a render target.
        /// <para>The <paramref name="format"/> for the render target view does not have to be the same as the render target backing buffer, and if the format is set to Unknown, then it will 
        /// use the format from the buffer.</para>
        /// </remarks>
        /// <exception cref="GorgonException">Thrown when the view could not created or retrieved from the internal cache.</exception>
        public GorgonRenderTargetBufferView GetRenderTargetView(BufferFormat format, int firstElement, int elementCount)
        {
            return OnGetRenderTargetView(format, Settings.Format, firstElement, elementCount);
        }

	    /// <summary>
        /// Function to retrieve the render target view for a render target.
        /// </summary>
        /// <param name="target">Render target to evaluate.</param>
        /// <returns>The render target view for the swap chain.</returns>
        public static GorgonRenderTargetView ToRenderTargetView(GorgonRenderTargetBuffer target)
        {
            return target == null ? null : target.DefaultRenderTargetView;
        }

        /// <summary>
        /// Implicit operator to return the render target view for a render target
        /// </summary>
        /// <param name="target">Render target to evaluate.</param>
        /// <returns>The render target view for the swap chain.</returns>
        public static implicit operator GorgonRenderTargetView(GorgonRenderTargetBuffer target)
        {
            return target == null ? null : target.DefaultRenderTargetView;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTargetBuffer"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that created this object.</param>
		/// <param name="name">The name of the render target.</param>
		/// <param name="settings">Settings to apply to the render target.</param>
		internal GorgonRenderTargetBuffer(GorgonGraphics graphics, string name, GorgonRenderTargetBufferSettings settings)
			: base(graphics, name, settings)
		{
			Settings = settings;
            IsRenderTarget = true;
		}
		#endregion
	}
}
