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
// Created: Wednesday, May 29, 2013 11:08:29 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.UI;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace Gorgon.Graphics
{
	/// <summary>
	/// The type of structured buffer unordered access view.
	/// </summary>
	public enum UnorderedAccessViewType
	{
		/// <summary>
		/// A standard structured buffer.
		/// </summary>
		Standard = 0,
		/// <summary>
		/// An append/consume buffer.
		/// </summary>
		AppendConsume = 1,
		/// <summary>
		/// A counter buffer.
		/// </summary>
		Counter = 2
	}
	
	/// <summary>
    /// An unordered access structured buffer view.
    /// </summary>
    /// <remarks>Use a resource view to allow a multiple threads inside of a shader access to the contents of a resource (or sub resource) at the same time.  
    /// <para>Ordered access views can be read/write in the shader if the format is set to one of R32_Uint, R32_Int or R32_Float.  Otherwise the view will be read-only.  An unordered access view must 
    /// have a format that is the same bit-depth and in the same group as its bound resource.</para>
    /// <para>Unlike a <see cref="Gorgon.Graphics.GorgonBufferUnorderedAccessView">GorgonBufferUnorderedAccessView</see>, only one unordered access view may be applied to a resource.</para>
    /// <para>This view allows structured views to be viewed as append/consume or count buffers.</para>
    /// </remarks>
    public sealed class GorgonStructuredBufferUnorderedAccessView
        : GorgonBufferUnorderedAccessView
    {
        #region Variables.
        private GorgonStructuredBuffer _structuredBuffer;               // Structured buffer attached to this view.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the type of view.
        /// </summary>
        public UnorderedAccessViewType ViewType
        {
            get;
        }

        /// <summary>
        /// Property to set or return the initial count for the unordered access view.
        /// </summary>
        /// <remarks>This only applies to view AppendConsume/Counter view types.
        /// <para>The default value is -1.</para>
        /// </remarks>
        public int InitialCount
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to perform initialization of the shader view resource.
        /// </summary>
        protected override void OnInitialize()
        {
            var bufferType = UnorderedAccessViewBufferFlags.None;

			GorgonApplication.Log.Print("Creating structured buffer unordered access view for {0}.", LoggingLevel.Verbose, Resource.Name);

            switch (ViewType)
            {
                case UnorderedAccessViewType.AppendConsume:
                    bufferType = UnorderedAccessViewBufferFlags.Append;
                    break;
                case UnorderedAccessViewType.Counter:
                    bufferType = UnorderedAccessViewBufferFlags.Counter;
                    break;
            }

            var desc = new UnorderedAccessViewDescription
                {
                    Dimension = UnorderedAccessViewDimension.Buffer,
                    Buffer =
                        {
                            FirstElement = ElementStart,
                            ElementCount = ElementCount,
                            Flags = bufferType
                        },
                    Format = (Format)Format
                };

            D3DView = new UnorderedAccessView(Resource.Graphics.D3DDevice, Resource.D3DResource, desc)
                {
                    DebugName = "Gorgon Unordered Access View for " + Resource.Name
                };
        }

		/// <summary>
		/// Function to copy data from this view into a buffer.
		/// </summary>
		/// <param name="buffer">Buffer that will receive the data.</param>
		/// <param name="offset">DWORD aligned offset within the buffer to start writing at.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> parameter is less than 0, or larger than the size of the buffer minus 4 bytes.</exception>
		/// <remarks>This will copy the contents of a structured buffer into any other type of buffer.  The view must have been created using a ViewType of Counter or Append, otherwise an exception will be thrown.</remarks>
		public void CopyTo(GorgonBaseBuffer buffer, int offset)
		{
			buffer.ValidateObject("buffer");
			offset.ValidateRange("index", 0, buffer.SizeInBytes - 4);

#if DEBUG
			if (ViewType == UnorderedAccessViewType.Standard)
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_VIEW_UNORDERED_TYPE_NOT_VALID_FOR_COPY);
			}
#endif
			Resource.Graphics.Context.CopyStructureCount(buffer.D3DBuffer, offset, D3DView);
		}

        /// <summary>
        /// Explicit operator to convert this view into its attached structured buffer.
        /// </summary>
        /// <param name="view">View to convert.</param>
        /// <returns>The attached buffer.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when the buffer is not a structured buffer.</exception>
        public static explicit operator GorgonStructuredBuffer(GorgonStructuredBufferUnorderedAccessView view)
        {
            if (view._structuredBuffer == null)
            {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_BUFFER, "structured"));
            }

            return view._structuredBuffer;
        }

        /// <summary>
        /// Function to convert this view into its attached structured buffer.
        /// </summary>
        /// <param name="view">View to convert.</param>
        /// <returns>The attached buffer.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when the buffer is not a structured buffer.</exception>
        public static GorgonStructuredBuffer ToStructuredBuffer(GorgonStructuredBufferUnorderedAccessView view)
        {
            if (view._structuredBuffer == null)
            {
                throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_BUFFER, "structured"));
            }

            return view._structuredBuffer;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonStructuredBufferUnorderedAccessView"/> class.
        /// </summary>
        /// <param name="resource">The buffer to bind to the view.</param>
        /// <param name="firstElement">The first element in the buffer.</param>
        /// <param name="elementCount">The number of elements to view.</param>
        /// <param name="viewType">The type of unordered access view.</param>
        internal GorgonStructuredBufferUnorderedAccessView(GorgonResource resource, int firstElement, int elementCount, UnorderedAccessViewType viewType)
            : base(resource, BufferFormat.Unknown, firstElement, elementCount, false)
        {
            ViewType = viewType;
            InitialCount = -1;
            _structuredBuffer = (GorgonStructuredBuffer)resource;
        }
        #endregion
    }
}