using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Collections;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Used to hold the video device capability information.
	/// </summary>
	public abstract class GorgonVideoDeviceCapabilities
	{
		#region Value Types.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the pixel shader version.
		/// </summary>
		public Version PixelShaderVersion
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the vertex shader version.
		/// </summary>
		public Version VertexShaderVersion
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the available comparison flags for an alpha channel.
		/// </summary>
		public GorgonCompareFlags AlphaComparisonFlags
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the available comparison flags for a depth buffer.
		/// </summary>
		public GorgonCompareFlags DepthComparisonFlags
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the available VSync intervals.
		/// </summary>
		public GorgonVSyncInterval VSyncIntervals
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the maximum number of concurrent data slots available.
		/// </summary>
		public int MaxSlots
		{
			get;
			protected set;
		}

		/// <summary>
		/// A collection of renderer specific capabilities for the device.
		/// </summary>
		/// <remarks>This can be used to derive specific hardware functionality for a particular type of rendering back end.</remarks>
		public GorgonCapabilityCollection CustomCapabilities
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the device capabilities.
		/// </summary>
		/// <returns>A collection of custom renderer specific capabilities.</returns>
		protected abstract IEnumerable<KeyValuePair<string, object>> GetCaps();

		/// <summary>
		/// Function to retrieve the device capabilities.
		/// </summary>
		internal void EnumerateCapabilities()
		{
			CustomCapabilities = new GorgonCapabilityCollection(GetCaps());
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoDeviceCapabilities"/> class.
		/// </summary>
		protected GorgonVideoDeviceCapabilities()
		{
			AlphaComparisonFlags = GorgonCompareFlags.Always;
			DepthComparisonFlags = GorgonCompareFlags.Always;
			PixelShaderVersion = new Version(0, 0);
			VertexShaderVersion = new Version(0, 0);			
		}
		#endregion
	}
}
