using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// The primary render target for a window.
	/// </summary>
	/// <remarks>
	/// These objects are used to as the primary render target for a window.
	/// </remarks>
	public abstract class GorgonDeviceWindow
		: GorgonSwapChain
	{
		#region Variables.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the video device that this device window is bound with.
		/// </summary>
		public GorgonVideoDevice VideoDevice
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return which output on the device is being used with this device window.
		/// </summary>
		public GorgonVideoOutput VideoOutput
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether this device window is full screen or windowed.
		/// </summary>
		public bool IsWindowed
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the details of the selected video mode.
		/// </summary>
		public GorgonVideoMode Mode
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDeviceWindow"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="device">Video device to use.</param>
		/// <param name="output">Video output on the device to use.</param>
		/// <param name="mode">A video mode structure defining the width, height and format of the render target.</param>
		/// <param name="depthStencilFormat">The depth buffer format (if required) for the target.</param>
		/// <param name="fullScreen">TRUE to go full screen, FALSE to stay windowed.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <param name="device"/>, <param name="output"/> or <param name="window"> parameters are NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		/// <remarks>When passing TRUE to <paramref name="fullScreen"/>, then the <paramref name="window"/> parameter must be a Windows Form object.
		/// <para>The <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateNominator">RefreshRateNominator</see> and the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateDenominator">RefreshRateDenominator</see> 
		/// of the <see cref="GorgonLibrary.Graphics.GorgonVideoMode">GorgonVideoMode</see> type are not relevant when <param name="fullScreen"/> is set to FALSE.</para>
		/// </remarks>
		protected GorgonDeviceWindow(string name, GorgonVideoDevice device, GorgonVideoOutput output,  Control window, GorgonVideoMode mode, GorgonBufferFormat depthStencilFormat, bool fullScreen)
			: base(name, window, mode, depthStencilFormat)
		{
			if (device == null)
				throw new ArgumentNullException("device");
			if (output == null)
				throw new ArgumentNullException("output");

			// For child controls, do not go to full screen.
			if (!(window is Form))
				fullScreen = false;

			VideoDevice = device;
			VideoOutput = output;
			IsWindowed = !fullScreen;
			Mode = mode;
		}
		#endregion
	}
}
