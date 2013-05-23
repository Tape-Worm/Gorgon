using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// An unordered access buffer view.
	/// </summary>
	/// <remarks>Use a resource view to allow a multiple threads inside of a shader access to the contents of a resource (or sub resource) at the same time.  
	/// <para>Ordered access views can be read/write in the shader if the format is set to one of R32_Uint, R32_Int or R32_Float.  Otherwise the view will be read-only.  An unordered access view must 
	/// have a format that is the same bit-depth and in the same group as its bound resource.</para>
	/// <para>Unlike a <see cref="GorgonLibrary.Graphics.GorgonBufferShaderView">GorgonBufferShaderView</see>, only one unordered access view may be applied to a resource.</para>
	/// </remarks>
	public class GorgonBufferUnorderedAccessView
		: GorgonUnorderedAccessView
	{
		#region Properties.
		/// <summary>
		/// Property to return the offset of the view from first element in the buffer.
		/// </summary>
		public int ElementStart
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of elements in the view.
		/// </summary>
		public int ElementCount
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
            var desc = new SharpDX.Direct3D11.UnorderedAccessViewDescription
				{
					Dimension = SharpDX.Direct3D11.UnorderedAccessViewDimension.Buffer,
					Buffer =
						{
							FirstElement = ElementStart,
							ElementCount = ElementCount,
							Flags = D3D.UnorderedAccessViewBufferFlags.None
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
		/// Initializes a new instance of the <see cref="GorgonBufferUnorderedAccessView"/> class.
		/// </summary>
		/// <param name="resource">The buffer to bind to the view.</param>
		/// <param name="format">The format of the view.</param>
		/// <param name="firstElement">The first element in the buffer.</param>
		/// <param name="elementCount">The number of elements to view.</param>
		internal GorgonBufferUnorderedAccessView(GorgonResource resource, BufferFormat format, int firstElement, int elementCount)
			: base(resource, format)
		{
			ElementStart = firstElement;
			ElementCount = elementCount;
		}
		#endregion
	}
}