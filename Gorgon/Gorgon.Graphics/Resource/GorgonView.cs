namespace Gorgon.Graphics
{
	/// <summary>
	/// A resource view.
	/// </summary>
	/// <remarks>Use a resource view to allow a resource (or sub resource) to be bound to the pipeline.  When the resource is created with a typeless format, this will allow 
	/// the resource to be cast to any format within the same group.</remarks>
	public abstract class GorgonView
	{
		#region Variables.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the resource that the view is bound with.
		/// </summary>
		public GorgonResource Resource
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the format for the view.
		/// </summary>
		public BufferFormat Format
		{
			get;
		}

		/// <summary>
		/// Property to return information about the view format.
		/// </summary>
		public GorgonBufferFormatInfo.FormatData FormatInformation
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform clean up of the resources used by the view.
		/// </summary>
		protected abstract void OnCleanUp();

		/// <summary>
		/// Function to perform initialization of the view.
		/// </summary>
		protected abstract void OnInitialize();

		/// <summary>
		/// Function to clean up the resources used by the view.
		/// </summary>
		internal void CleanUp()
		{
			OnCleanUp();
		}

		/// <summary>
		/// Function to perform initialization of the shader view resource.
		/// </summary>
		internal void Initialize()
		{
			OnInitialize();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonView"/> class.
		/// </summary>
		/// <param name="resource">The resource to bind to the view.</param>
		/// <param name="format">The format of the view.</param>
		protected GorgonView(GorgonResource resource, BufferFormat format)
		{
			Resource = resource;
			Format = format;
			FormatInformation = GorgonBufferFormatInfo.GetInfo(Format);
		}
		#endregion
	}
}