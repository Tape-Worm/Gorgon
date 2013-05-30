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

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// An unordered access structured buffer view.
    /// </summary>
    /// <remarks>Use a resource view to allow a multiple threads inside of a shader access to the contents of a resource (or sub resource) at the same time.  
    /// <para>Ordered access views can be read/write in the shader if the format is set to one of R32_Uint, R32_Int or R32_Float.  Otherwise the view will be read-only.  An unordered access view must 
    /// have a format that is the same bit-depth and in the same group as its bound resource.</para>
    /// <para>Unlike a <see cref="GorgonLibrary.Graphics.GorgonBufferShaderView">GorgonBufferShaderView</see>, only one unordered access view may be applied to a resource.</para>
    /// <para>This view allows structured views to be viewed as append/consume or count buffers.</para>
    /// </remarks>
    public class GorgonStructuredBufferUnorderedAccessView
        : GorgonBufferUnorderedAccessView
    {
        #region Properties.
        /// <summary>
        /// Property to return the type of view.
        /// </summary>
        public UnorderedAccessViewType ViewType
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to perform initialization of the shader view resource.
        /// </summary>
        protected override void OnInitialize()
        {
            var bufferType = SharpDX.Direct3D11.UnorderedAccessViewBufferFlags.None;

            switch (ViewType)
            {
                case UnorderedAccessViewType.AppendConsume:
                    bufferType = SharpDX.Direct3D11.UnorderedAccessViewBufferFlags.Append;
                    break;
                case UnorderedAccessViewType.Counter:
                    bufferType = SharpDX.Direct3D11.UnorderedAccessViewBufferFlags.Counter;
                    break;
            }

            var desc = new SharpDX.Direct3D11.UnorderedAccessViewDescription
                {
                    Dimension = SharpDX.Direct3D11.UnorderedAccessViewDimension.Buffer,
                    Buffer =
                        {
                            FirstElement = ElementStart,
                            ElementCount = ElementCount,
                            Flags = bufferType
                        },
                    Format = (SharpDX.DXGI.Format)Format
                };

            D3DView = new SharpDX.Direct3D11.UnorderedAccessView(Resource.Graphics.D3DDevice, Resource.D3DResource, desc)
                {
                    DebugName = "Gorgon Unordered Access View for " + Resource.Name
                };
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
        }
        #endregion
    }
}