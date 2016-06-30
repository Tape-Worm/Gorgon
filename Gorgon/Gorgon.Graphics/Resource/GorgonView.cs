using System;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A resource view.
	/// </summary>
	/// <remarks>Use a resource view to allow a resource (or sub resource) to be bound to the pipeline.  When the resource is created with a typeless format, this will allow 
	/// the resource to be cast to any format within the same group.</remarks>
	public abstract class GorgonView
		: IDisposable
	{
		#region Properties.
		/// <summary>
		/// Property to return the log interface used for debug logging.
		/// </summary>
		protected IGorgonLog Log
		{
			get;
		}

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
			protected set;
		}

		/// <summary>
		/// Property to return information about the view format.
		/// </summary>
		public GorgonFormatInfo FormatInformation
		{
			get;
			protected set;
		}
		#endregion

		#region Methods.

		/// <summary>
		/// Function to perform clean up of the resources used by the view.
		/// </summary>
		protected virtual void OnCleanUp()
		{
			throw new NotSupportedException("YOU'RE SUPPOSED TO REMOVE THIS!");
		}

		/// <summary>
		/// Function to perform initialization of the view.
		/// </summary>
		protected virtual void OnInitialize()
		{
			throw new NotSupportedException("YOU'RE SUPPOSED TO REMOVE THIS!");
		}

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
		internal void InitializeOLDEN()
		{
			OnInitialize();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public abstract void Dispose();
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonView"/> class.
		/// </summary>
		/// <param name="resource">The resource to bind to the view.</param>
		/// <param name="format">The format of the view.</param>
		/// <param name="log">[Optional] The log interface used for debug logging.</param>
		protected GorgonView(GorgonResource resource, BufferFormat format, IGorgonLog log = null)
		{
			throw new NotSupportedException("DO NOT USE THIS FUCKING CONSTRUCTOR!");
			Log = log ?? GorgonLogDummy.DefaultInstance;
			Resource = resource;
			Format = format;
			FormatInformation = new GorgonFormatInfo(SharpDX.DXGI.Format.A8P8);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonView"/> class.
		/// </summary>
		/// <param name="resource">The resource to bind with the view.</param>
		/// <param name="log">The log interface used for debug logging.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="resource"/> parameter is <b>null</b>.</exception>
		protected GorgonView(GorgonResource resource, IGorgonLog log)
		{
			if (resource == null)
			{
				throw new ArgumentNullException(nameof(resource));
			}

			Log = log ?? GorgonLogDummy.DefaultInstance;
			Resource = resource;

		}
		#endregion
	}
}