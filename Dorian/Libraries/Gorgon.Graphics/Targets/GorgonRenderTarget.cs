#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Saturday, July 30, 2011 1:11:00 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A render target.
	/// </summary>
	/// <remarks>The render target will receive the graphical data for display.</remarks>
	public abstract class GorgonRenderTarget
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		private bool _disposed = false;				// Flag to indicate that the object was disposed.
		private bool _cleaned = false;				// Flag to indicate that the object was cleaned up.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the graphics interface that created this render target.
		/// </summary>
		protected GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the default background color for the target.
		/// </summary>
		/// <remarks>The default color is white (1.0f, 1.0f, 1.0f, 1.0f).</remarks>
		public GorgonColor DefaultBackgroundColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the surface object for this render target.
		/// </summary>
		public GorgonSurface Surface
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the depth/stencil buffer surface object for this render target.
		/// </summary>
		public GorgonSurface DepthStencilSurface
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return whether the target has a depth buffer attached to it.
		/// </summary>
		public abstract bool HasDepthBuffer
		{
			get;
		}

		/// <summary>
		/// Property to return whether the target has a stencil buffer attaached to it.
		/// </summary>
		public abstract bool HasStencilBuffer
		{
			get;
		}
		#endregion

		#region Methods.
		#region Remove this shit.
		/// <summary>
		/// 
		/// </summary>
		public abstract void SetupTest();

		/// <summary>
		/// 
		/// </summary>
		public abstract void RunTest(float dt);

		/// <summary>
		/// 
		/// </summary>
		public abstract void CleanUpTest();

		#endregion

		/// <summary>
		/// Function to clear out any outstanding resources when the object is disposed.
		/// </summary>
		/// <remarks>Implementors must call this method to clean up outstanding resources in the Dispose method.</remarks>
		protected virtual void CleanUpResources()
		{
			_cleaned = true;
		}

		/// <summary>
		/// Function to create any resources required by the render target.
		/// </summary>
		/// <remarks>Implementors must use this method to build the internal render target based on the rendering back end.</remarks>
		protected abstract void CreateResources();

		/// <summary>
		/// Function to perform an update on the resources required by the render target.
		/// </summary>
		protected abstract void UpdateResources();

		/// <summary>
		/// Function to initialize the render target.
		/// </summary>
		internal virtual void Initialize()
		{
			CreateResources();
		}

		/// <summary>
		/// Function to validate any settings for a multi-head device window.
		/// </summary>
		/// <param name="graphics">Graphics interface to use.</param>
		/// <param name="multiHeadSettings">Multi head settings.</param>
		internal static void ValidateDeviceWindowSettings(GorgonGraphics graphics, GorgonMultiHeadSettings multiHeadSettings)
		{
			IObjectTracker tracker = graphics as IObjectTracker;

			if (multiHeadSettings == null)
				throw new ArgumentNullException("multiHeadSettings");

			if (multiHeadSettings.Settings.Count < 1)
				throw new ArgumentException("There were no device settings in the multi-head settings.", "multiHeadSettings");

			// Force all settings to use the same windowed/full screen state as the first (master head) in the list.
			foreach (var setting in multiHeadSettings.Settings)
			{
				setting.IsWindowed = false;		// Force full-screen.

				// Ensure that the first setting is for the master video output.
				if ((setting == multiHeadSettings.Settings[0]) && (setting.Output != setting.Device.Outputs[0]))
					throw new ArgumentException("The video output for the first setting is not the master video output.", "multiHeadSettings");

				ValidateDeviceWindowSettings(graphics, setting);
			}
		}

		/// <summary>
		/// Function to validate the swap chain settings.
		/// </summary>
		/// <param name="graphics">Graphics interface to use.</param>
		/// <param name="deviceWindow">Device window creating the swap chain.</param>
		/// <param name="settings">Settings to validate.</param>
		internal static void ValidateSwapChainSettings(GorgonGraphics graphics, GorgonDeviceWindow deviceWindow, GorgonSwapChainSettings settings)
		{
			IObjectTracker tracker = graphics as IObjectTracker;

			if (settings == null)
				throw new ArgumentNullException("settings");

			// Ensure that we're not already using this window as a device window.
			var inUse = tracker.TrackedObjects.Count(item =>
			{
				GorgonWindowTarget<GorgonDeviceWindowSettings> targetItem = item as GorgonWindowTarget<GorgonDeviceWindowSettings>;
				GorgonMultiHeadDeviceWindow multiHeadItem = item as GorgonMultiHeadDeviceWindow;

				return (((multiHeadItem != null) && (multiHeadItem.Settings.Settings.Count(deviceSetting => deviceSetting.BoundWindow == settings.BoundWindow) > 0)) ||
					((targetItem != null) && (targetItem.Settings.BoundWindow == settings.BoundWindow)));
			}) > 0;

			if (inUse)
				throw new ArgumentException("The specified window is already assigned to a device window or swap chain.", "window");

			// If we haven't specified a size, or we're using a child control, then assume the size of the bound window.
			if (settings.Dimensions == System.Drawing.Size.Empty)
				settings.Dimensions = settings.BoundWindow.ClientSize;

			// Use the default desktop display format if we haven't picked a format.
			if (settings.Format == GorgonBufferFormat.Unknown)
				settings.Format = deviceWindow.Settings.Output.DefaultVideoMode.Format;

			// Check to see if back buffer is supported.
			if (!deviceWindow.Settings.Output.SupportsBackBufferFormat(settings.Format, true))
				throw new GorgonException(GorgonResult.FormatNotSupported, "Cannot use the specified format '" + settings.Format.ToString() + "' with the display format '" + deviceWindow.Settings.Output.DefaultVideoMode.Format.ToString() + "'.");

			// Set up the depth buffer if necessary.
			if (settings.DepthStencilFormat != GorgonBufferFormat.Unknown)
			{
				if (!deviceWindow.Settings.Output.SupportsDepthFormat(settings.Format, settings.DepthStencilFormat, true))
					throw new GorgonException(GorgonResult.FormatNotSupported, "Cannot use the specified depth/stencil format '" + settings.DepthStencilFormat.ToString() + "'.");
			}

			// We can only enable multi-sampling when we have a discard swap effect and have non-lockable depth buffers.
			if (settings.MSAAQualityLevel.Level != GorgonMSAALevel.None)
			{
				if ((settings.DisplayFunction == GorgonDisplayFunction.Discard) && (settings.DepthStencilFormat != GorgonBufferFormat.D16_UIntNormal_Lockable)
					&& (settings.DepthStencilFormat != GorgonBufferFormat.D32_Float_Lockable))
				{
					int? qualityLevel = deviceWindow.Settings.Device.GetMultiSampleQuality(settings.MSAAQualityLevel.Level, settings.Format, true);

					if ((qualityLevel == null) || (qualityLevel < settings.MSAAQualityLevel.Quality))
						throw new ArgumentException("The device cannot support the quality level '" + settings.MSAAQualityLevel.Level.ToString() + "' at a quality of '" + settings.MSAAQualityLevel.Quality.ToString() + "'");
				}
				else
					throw new ArgumentException("Cannot use multi sampling with device windows that don't have a Discard display function and/or use lockable depth/stencil buffers.");
			}
		}

		/// <summary>
		/// Function to validate any settings for a device window.
		/// </summary>
		/// <param name="graphics">Graphics interface to use.</param>
		/// <param name="settings">Settings to validate.</param>
		internal static void ValidateDeviceWindowSettings(GorgonGraphics graphics, GorgonDeviceWindowSettings settings)
		{
			IObjectTracker tracker = graphics as IObjectTracker;
			System.Windows.Forms.Form window = settings.BoundWindow as System.Windows.Forms.Form;
			IntPtr monitorHandle = IntPtr.Zero;

			if (settings == null)
				throw new ArgumentNullException("settings");
			
			// For child controls, do not go to full screen.
			if ((window == null) && (!settings.IsWindowed))
				throw new ArgumentException("Cannot switch to full screen with a child control.", "fullScreen");
						
			// Ensure that we're not already using this window as a device window.
			var inUse = tracker.TrackedObjects.Count(item =>
			{
				GorgonWindowTarget<GorgonDeviceWindowSettings> targetItem = item as GorgonWindowTarget<GorgonDeviceWindowSettings>;
				GorgonMultiHeadDeviceWindow multiHeadItem = item as GorgonMultiHeadDeviceWindow;

				return (((multiHeadItem != null) && (multiHeadItem.Settings.Settings.Count(deviceSetting => deviceSetting.BoundWindow == settings.BoundWindow) > 0)) ||
					((targetItem != null) && (targetItem.Settings.BoundWindow == settings.BoundWindow)));
			}) > 0;

			if (inUse)
				throw new ArgumentException("The specified window is already assigned to a device window or swap chain.", "window");

			// If we haven't specified a size, or we're using a child control, then assume the size of the bound window.
			if ((settings.Dimensions == System.Drawing.Size.Empty) || (window == null))
				settings.Dimensions = settings.BoundWindow.ClientSize;

			// Find out which device and output contain the window.
			if (settings.Output == null)
				settings.Output = graphics.VideoDevices.GetOutputFromControl(settings.BoundWindow);

			if ((settings.Device == null) || (!settings.Device.Outputs.Contains(settings.Output)))
				settings.Device = settings.Output.VideoDevice;

			// Use the default desktop display format if we haven't picked a format.
			if (settings.Format == GorgonBufferFormat.Unknown)
				settings.Format = settings.Output.DefaultVideoMode.Format;

			// If we're maximized, then use the desktop resolution.
			if ((window != null) && (window.WindowState == System.Windows.Forms.FormWindowState.Maximized))
				settings.Dimensions = new System.Drawing.Size(settings.Output.DefaultVideoMode.Width, settings.Output.DefaultVideoMode.Height);

			// If we are not windowed, don't allow an unknown video mode.
			if (!settings.IsWindowed)
			{
				int count = 0;

				// If we've not specified a refresh rate, then find the lowest refresh on the output.
				if ((settings.RefreshRateDenominator == 0) || (settings.RefreshRateNumerator == 0))
				{
					if (settings.Output.VideoModes.Count(item => item.Width == settings.Width &&
														item.Height == settings.Height &&
														item.Format == settings.Format) > 0)
					{
						var refresh = (from mode in settings.Output.VideoModes
									   where ((mode.Width == settings.Width) && (mode.Height == settings.Height) && (mode.Format == settings.Format))
									   select mode.RefreshRateNumerator).Min();
						settings.RefreshRateNumerator = refresh;
						settings.RefreshRateDenominator = 1;
					}
				}

				count = settings.Output.VideoModes.Count(item => item == settings.DisplayMode);

				if (count == 0)
					throw new GorgonException(GorgonResult.CannotCreate, "Unable to set the video mode.  The mode '" + settings.Width.ToString() + "x" +
						settings.Height.ToString() + " Format: " + settings.Format.ToString() + " Refresh Rate: " +
						settings.RefreshRateNumerator.ToString() + "/" + settings.RefreshRateDenominator.ToString() +
						"' is not a valid full screen video mode for this video output.");
			}

			// Check to see if back buffer is supported.
			if (!settings.Output.SupportsBackBufferFormat(settings.Format, settings.IsWindowed))
				throw new GorgonException(GorgonResult.FormatNotSupported, "Cannot use the specified format '" + settings.Format.ToString() + "' with the display format '" + settings.Output.DefaultVideoMode.Format.ToString() + "'.");

			// Set up the depth buffer if necessary.
			if (settings.DepthStencilFormat != GorgonBufferFormat.Unknown)
			{
				if (!settings.Output.SupportsDepthFormat(settings.Format, settings.DepthStencilFormat, settings.IsWindowed))
					throw new GorgonException(GorgonResult.FormatNotSupported, "Cannot use the specified depth/stencil format '" + settings.DepthStencilFormat.ToString() + "'.");
			}

			// We can only enable multi-sampling when we have a discard swap effect and have non-lockable depth buffers.
			if (settings.MSAAQualityLevel.Level != GorgonMSAALevel.None)
			{
				if ((settings.DisplayFunction == GorgonDisplayFunction.Discard) && (settings.DepthStencilFormat != GorgonBufferFormat.D16_UIntNormal_Lockable)
					&& (settings.DepthStencilFormat != GorgonBufferFormat.D32_Float_Lockable))
				{
					int? qualityLevel = settings.Device.GetMultiSampleQuality(settings.MSAAQualityLevel.Level, settings.Format, settings.IsWindowed);

					if ((qualityLevel == null) || (qualityLevel < settings.MSAAQualityLevel.Quality))
						throw new ArgumentException("The device cannot support the quality level '" + settings.MSAAQualityLevel.Level.ToString() + "' at a quality of '" + settings.MSAAQualityLevel.Quality.ToString() + "'");
				}
				else
					throw new ArgumentException("Cannot use multi sampling with device windows that don't have a Discard display function and/or use lockable depth/stencil buffers.");
			}
		}

		/// <summary>
		/// Function to update the render target after changes have been made to its settings.
		/// </summary>
		public abstract void UpdateSettings();

		/// <summary>
		/// Function to clear a target.
		/// </summary>
		/// <param name="color">Color to clear with.</param>
		/// <param name="depthValue">Depth buffer value to clear with.</param>
		/// <param name="stencilValue">Stencil value to clear with.</param>
		/// <remarks>This will only clear a depth/stencil buffer if one has been attached to the target, otherwise it will do nothing.
		/// <para>Pass NULL (Nothing in VB.Net) to <paramref name="color"/>, <paramref name="depthValue"/> or <paramref name="stencilValue"/> to exclude that buffer from being cleared.</para>
		/// </remarks>
		public abstract void Clear(GorgonColor? color, float? depthValue, int? stencilValue);

		/// <summary>
		/// Function to clear a target.
		/// </summary>
		/// <param name="color">Color to clear with.</param>
		/// <param name="depthValue">Depth buffer value to clear with.</param>
		/// <remarks>This will only clear a depth/stencil buffer if one has been attached to the target, otherwise it will do nothing.
		/// <para>Pass NULL (Nothing in VB.Net) to <paramref name="color"/> or <paramref name="depthValue"/> to exclude that buffer from being cleared.</para>
		/// </remarks>
		public void Clear(GorgonColor? color, float? depthValue)
		{
			Clear(color, depthValue, 0);
		}

		/// <summary>
		/// Function to clear a target.
		/// </summary>
		/// <param name="color">Color to clear with.</param>
		public void Clear(GorgonColor color)
		{
			Clear(color, 1.0f, 0);
		}

		/// <summary>
		/// Function to clear a target.
		/// </summary>
		/// <remarks>This will clear the target with the <see cref="P:GorgonLibrary.Graphics.GorgonRenderTarget.DefaultBackgroundColor">default background color</see> value.</remarks>
		public void Clear()
		{
			Clear(DefaultBackgroundColor, 1.0f, 0);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTarget"/> class.
		/// </summary>
		/// <param name="graphics">The graphics instance that owns this render target.</param>
		/// <param name="name">The name.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		protected GorgonRenderTarget(GorgonGraphics graphics, string name)
			: base(name)
		{
			if (graphics == null)
				throw new ArgumentNullException("graphics");

			Graphics = graphics;
			DefaultBackgroundColor = new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f);
		}
		#endregion
	
		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// Ensure that any resources that were allocated are released by this time.
					if (!_cleaned)
						throw new ObjectDisposedException(GetType().FullName, "The object was disposed without calling CleanUpResources().  This may cause memory leaks in native code.");
				}

				_disposed = true;
			}
		}
		
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void  Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
