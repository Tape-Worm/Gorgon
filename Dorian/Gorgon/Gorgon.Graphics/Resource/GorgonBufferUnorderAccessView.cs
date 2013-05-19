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
	public class GorgonBufferUnorderAccessView
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

		/// <summary>
		/// Property to return whether the buffer is using raw access or not.
		/// </summary>
		public bool IsRaw
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
			var structBuffer = Resource as GorgonStructuredBuffer;
			
			if (structBuffer != null)
			{
				switch (structBuffer.Settings.StructuredBufferType)
				{
					case StructuredBufferType.AppendConsume:
						bufferType = SharpDX.Direct3D11.UnorderedAccessViewBufferFlags.Append;
						break;
					case StructuredBufferType.Counter:
						bufferType = SharpDX.Direct3D11.UnorderedAccessViewBufferFlags.Append;
						break;
				}
			}
			else
			{
				if (IsRaw)
				{
					bufferType = SharpDX.Direct3D11.UnorderedAccessViewBufferFlags.Raw;
				}
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
		/// Initializes a new instance of the <see cref="GorgonBufferUnorderAccessView"/> class.
		/// </summary>
		/// <param name="resource">The buffer to bind to the view.</param>
		/// <param name="format">The format of the view.</param>
		/// <param name="firstElement">The first element in the buffer.</param>
		/// <param name="elementCount">The number of elements to view.</param>
		/// <param name="isRaw">TRUE if the view is a raw view, FALSE if not.</param>
		internal GorgonBufferUnorderAccessView(GorgonResource resource, BufferFormat format, int firstElement, int elementCount, bool isRaw)
			: base(resource, format)
		{
			ElementStart = firstElement;
			ElementCount = elementCount;
			IsRaw = isRaw;
		}
		#endregion
	}
}