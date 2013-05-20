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
        public StructuredBufferType ViewType
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to perform initialization of the shader view resource.
        /// </summary>
        protected override void InitializeImpl()
        {
            var bufferType = SharpDX.Direct3D11.UnorderedAccessViewBufferFlags.None;

            switch (ViewType)
            {
                case StructuredBufferType.AppendConsume:
                    bufferType = SharpDX.Direct3D11.UnorderedAccessViewBufferFlags.Append;
                    break;
                case StructuredBufferType.Counter:
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
                    DebugName = "Gorgon Unordered Access View for " + Resource.GetType().FullName
                };
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonStructuredBufferUnorderedAccessView"/> class.
        /// </summary>
        /// <param name="resource">The buffer to bind to the view.</param>
        /// <param name="format">The format of the view.</param>
        /// <param name="firstElement">The first element in the buffer.</param>
        /// <param name="elementCount">The number of elements to view.</param>
        /// <param name="viewType">The type of unordered access view.</param>
        internal GorgonStructuredBufferUnorderedAccessView(GorgonResource resource, BufferFormat format, int firstElement, int elementCount, StructuredBufferType viewType)
            : base(resource, format, firstElement, elementCount)
        {
            ViewType = viewType;
        }
        #endregion
    }
}