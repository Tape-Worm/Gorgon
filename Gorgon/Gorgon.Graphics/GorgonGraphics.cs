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
// Created: Tuesday, July 19, 2011 8:55:06 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Gorgon.Collections.Specialized;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.UI;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using GI = SharpDX.DXGI;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Flags used to determine how a subresource should be updated.
	/// </summary>
	public enum UpdateSubResourceFlags
	{
		/// <summary>
		/// Perform no special logic when updating the subresource.
		/// </summary>
		None = D3D11.CopyFlags.None,
		/// <summary>
		/// Do not overwrite the existing contents of the subresource.
		/// </summary>
		NoOverwrite = D3D11.CopyFlags.NoOverwrite,
		/// <summary>
		/// The existing data in the subresource is no longer valid and should be discarded.
		/// </summary>
		Discard = D3D11.CopyFlags.Discard
	}

	/// <summary>
	/// Operators used for comparison operations.
	/// </summary>
	public enum ComparisonOperator
	{
		/// <summary>
		/// Unknown.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Never pass the comparison.
		/// </summary>
		Never = 1,
		/// <summary>
		/// If the source data is less than the destination data, the comparison passes.
		/// </summary>
		Less = 2,
		/// <summary>
		/// If the source data is equal to the destination data, the comparison passes.
		/// </summary>
		Equal = 3,
		/// <summary>
		/// If the source data is less than or equal to the destination data, the comparison passes.
		/// </summary>
		LessEqual = 4,
		/// <summary>
		/// If the source data is greater than the destination data, the comparison passes.
		/// </summary>
		Greater = 5,
		/// <summary>
		/// If the source data is not equal to the destination data, the comparison passes.
		/// </summary>
		NotEqual = 6,
		/// <summary>
		/// If the source data is greater than or equal to the destination data, the comparison passes.
		/// </summary>
		GreaterEqual = 7,
		/// <summary>
		/// Always pass the comparison.
		/// </summary>
		Always = 8
	}

	/// <summary>
	/// The primary object for the graphics sub system.
	/// </summary>
	/// <remarks>This interface is used to create all objects (buffers, shaders, etc...) that are to be used for graphics.  An interface is tied to a single physical video device, to use 
	/// multiple video devices, create additional graphics interfaces and assign the device to the <see cref="Gorgon.Graphics.GorgonGraphics.VideoDevice">VideoDevice</see> property.
	/// <para>The constructor for this object can take a value known as a device feature level to specify the base line video device capabilities to use.  This feature level value specifies 
	/// what capabilities we have available. To have Gorgon use the best available feature level for your video device, you may call the GorgonGraphics constructor 
	/// without any parameters and it will use the best available feature level for your device.</para>
	/// <para>Along with the feature level, the graphics object can also take a <see cref="Gorgon.Graphics.VideoDevice">Video Device</see> object as a parameter.  Specifying a 
	/// video device will force Gorgon to use that video device for rendering. If a video device is not specified, then the first detected video device will be used.</para>
	/// <para>Please note that graphics objects cannot be shared between devices and must be duplicated.</para>
	/// <para>Objects created by this interface will be automatically tracked and disposed when this interface is disposed.  This is meant to help handle memory leak problems.  However, 
	/// it is important to note that this is not a good practice and the developer is responsible for calling Dispose on all objects that they create, graphics or otherwise.</para>
	/// <para>This object will enumerate video devices, monitor outputs (for multi-head adapters), and video modes for each of the video devices in the system upon creation.  These
    /// items are accessible from the <see cref="Gorgon.Graphics.GorgonVideoDeviceList">GorgonVideoDeviceEnumerator</see> class.</para>
    /// <para>These objects can also be used in a deferred context.  This means that when a graphics object is deferred, it can be used in a multi threaded environment to allow set up of 
    /// a scene by recording commands sent to the video device for execution later on the rendering process.  This is handy where multiple passes for the same scene are required (e.g. a deferred renderer).</para>
	/// <para>Please note that this object requires Direct3D 11 (but not necessarily a Direct3D 11 video card) and at least Windows Vista Service Pack 2 or higher.  
	/// Windows XP and operating systems before it will not work, and an exception will be thrown if this object is created on those platforms.</para>
    /// <para>Deferred graphics contexts require a video device with a feature level of SM5 or better.</para>
	/// </remarks>
    public sealed class GorgonGraphics
        : IDisposable
    {
		#region Variables.
		// Tracked objects.
		private GorgonDisposableObjectCollection _trackedObjects;
		// Flag to indicate that the desktop window manager compositor is enabled.
		private static bool _isDWMEnabled;                                                  
		// Flag to indicate that we should not enable the DWM.
		private static readonly bool _dontEnableDWM;                                        
		// The log interface used to log debug messages.
		private readonly IGorgonLog _log;
		// The video device to use for this graphics object.
		private IGorgonVideoDevice _videoDevice;
		// The currently active list of render targets.
		private List<GorgonRenderTargetView> _currentRenderTargets;
		// The currently active list of unordered access views.
		private List<GorgonUnorderedAccessView> _currentUavs;
		// Offsets for consume/append buffers.
		private List<int> _offsets;
		// The list of D3D render targets to use.
		private D3D11.RenderTargetView[] _D3DRenderTargets;
		// The list of D3D unordered access views to use.
		private D3D11.UnorderedAccessView[] _D3DUavs;
		// The currently active depth/stencil view.
		private GorgonDepthStencilView _currentDepthStencilView;
        #endregion

        #region Properties.
		/// <summary>
		/// Property to return the Direct 3D 11.1 device context for this graphics instance.
		/// </summary>
		internal D3D11.DeviceContext1 D3DDeviceContext => VideoDevice?.D3DDeviceContext();

		/// <summary>
		/// Property to set or return whether DWM composition is enabled or not.
		/// </summary>
		/// <remarks>This property will have no effect on systems that initially have the desktop window manager compositor disabled.</remarks>
		public static bool IsDWMCompositionEnabled
        {
            get
            {
                return _isDWMEnabled;
            }
            set
            {
                if (!value)
                {
	                if (!_isDWMEnabled)
	                {
		                return;
	                }

	                if (Win32API.DwmEnableComposition(0) != 0)
	                {
		                return;
	                }

	                _isDWMEnabled = false;
                }
                else
                {
	                if ((_isDWMEnabled) || (_dontEnableDWM))
	                {
		                return;
	                }

	                if (Win32API.DwmEnableComposition(1) != 0)
	                {
		                return;
	                }

	                _isDWMEnabled = true;
                }
            }
        }

		/// <summary>
        /// Property to return the input geometry interface.
        /// </summary>
        /// <remarks>
        /// The input interface covers items such as the vertex buffer, index buffer, bindings of the aforementioned buffers, the primitive type, etc...
        /// </remarks>
        public GorgonInputGeometry Input
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the interface for buffers.
        /// </summary>
        public GorgonBuffers Buffers
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the shader interface.
        /// </summary>
        /// <remarks>This is used to create shaders, create constant buffers and bind them to the pipeline.</remarks>
        public GorgonShaderBinding Shaders
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the current rasterizer states.
        /// </summary>
        public GorgonRasterizerRenderState Rasterizer
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the output merging interface.
        /// </summary>
        /// <remarks>This is responsible for setting blending states, depth/stencil states, creating render targets, etc...</remarks>
        public GorgonOutputMerger Output
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the textures interface.
        /// </summary>
        public GorgonTextures Textures
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the fonts interface.
        /// </summary>
        public GorgonFonts Fonts
        {
            get;
            private set;
        }

		/// <summary>
		/// Property to set or return the video device to use for this graphics interface.
		/// </summary>
		public IGorgonVideoDevice VideoDevice => _videoDevice;

		/// <summary>
		/// Property to set or return whether object tracking is disabled.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will enable SharpDX's object tracking to ensure references are destroyed upon application exit.
		/// </para>
		/// <para>
		/// The default value for DEBUG mode is <b>true</b>, and for RELEASE it is set to <b>false</b>.  Disabling object tracking will give a slight performance increase.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// This flag <i>must</i> be set prior to creating any <see cref="GorgonGraphics"/> object, or else the flag will not take effect.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public static bool IsObjectTrackingEnabled
        {
            get
            {
                return SharpDX.Configuration.EnableObjectTracking;
            }
            set
            {
                SharpDX.Configuration.EnableObjectTracking = value;
            }
        }

		/// <summary>
		/// Property to set or return whether debug output is enabled for the underlying graphics API.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will enable debug output for the underlying graphics API that Gorgon uses to render (Direct 3D 11 at this time). When this is enabled, all functionality will have debugging information that will 
		/// output to the debug output console (Output window in Visual Studio) if the <c>Debug -> Enable Native Debugging</c> is turned on in the application project settings <i>and</i> the DirectX control panel 
		/// is set up to debug the application under Direct 3D 10/11(/12 for Windows 10) application list.
		/// </para>
		/// <para>
		/// When Gorgon is compiled in DEBUG mode, this flag defaults to <b>true</b>, otherwise it defaults to <b>false</b>.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// This flag <i>must</i> be set prior to creating any <see cref="GorgonGraphics"/> object, or else the flag will not take effect.
		/// </para>
		/// <para>
		/// The D3D11 SDK Layers DLL must be installed in order for this flag to work. If it is not, then the application may crash.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public static bool IsDebugEnabled
		{
			get;
			set;
		}

        /// <summary>
        /// Property to set or return whether swap chains should reset their full screen setting on regaining focus.
        /// </summary>
        /// <remarks>
        /// This will control whether Gorgon will try to reacquire full screen mode when a full screen swap chain window regains focus.  When this is set to <b>false</b>, and the window 
        /// containing the full screen swap chain loses focus, it will revert to windowed mode and remain in windowed mode.  When set to <b>true</b>, it will try to reacquire full screen mode.
        /// <para>The default value for this is <b>true</b>.  However, for a full screen multimonitor scenario, this should be set to <b>false</b>.</para>
        /// </remarks>
        public bool ResetFullscreenOnFocus
        {
            get;
            set;
        }

		/// <summary>
		/// Property to return the active list of render targets.
		/// </summary>
		public IReadOnlyList<GorgonRenderTargetView> RenderTargets => _currentRenderTargets;

		/// <summary>
		/// Property to return the active depth/stencil view.
		/// </summary>
		public GorgonDepthStencilView DepthStencilView => _currentDepthStencilView;
        #endregion

        #region Methods.

		/// <summary>
		/// Function to validate the render target views and depth/stencil view being assigned.
		/// </summary>
		/// <param name="renderTargetViews">The render target views to evaluate.</param>
		/// <param name="depthStencilView">The depth/stencil view to evaluate.</param>
		private void ValidateRenderTargetDepthStencilViews(GorgonRenderTargetView[] renderTargetViews, GorgonDepthStencilView depthStencilView)
		{
#if DEBUG
			GorgonRenderTargetView startView = renderTargetViews?.FirstOrDefault(item => item != null);

			if (startView == null)
			{
				return;
			}

			IEnumerable<GorgonRenderTargetView> otherViews = renderTargetViews.Where(item => item != startView && item != null);

			// ReSharper disable PossibleMultipleEnumeration
			// If we don't have a render target view, we don't need to check anything, even if we have a depth/stencil.
			// Begin checking resource data.
			var rtvTexture = startView.Resource as GorgonTexture;

			Debug.Assert(rtvTexture != null, "Render target view resource is not a texture.");

			// Compare the depth/stencil view against the first view (all views and their resources should have the same properties).
			if (depthStencilView != null) 
			{
				// Ensure all resources are the same type.
				if (depthStencilView.Resource.ResourceType != startView.Resource.ResourceType)
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_TYPE_MISMATCH, depthStencilView.Resource.ResourceType),
					                            nameof(depthStencilView));
				}

				// Ensure the depth stencil array/depth counts match for all resources.
				if (depthStencilView.ArrayCount != startView.ArrayOrDepthCount)
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_ARRAYCOUNT_MISMATCH, depthStencilView.Resource.Name),
					                            nameof(depthStencilView));
				}

				var dsTexture = depthStencilView.Resource as GorgonTexture;

				Debug.Assert(dsTexture != null, "Depth/stencil view not bound to a texture.");

				// Check to ensure that multisample info matches.
				if (dsTexture.Info.MultiSampleInfo.Equals(rtvTexture.Info.MultiSampleInfo))
				{
					throw new ArgumentException(
						string.Format(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_MULTISAMPLE_MISMATCH, dsTexture.Info.MultiSampleInfo.Quality, dsTexture.Info.MultiSampleInfo.Count),
						nameof(depthStencilView));
				}

				if ((dsTexture.Info.Width != rtvTexture.Info.Width)
				    || (dsTexture.Info.Height != rtvTexture.Info.Height)
				    || ((dsTexture.Info.TextureType != TextureType.Texture3D) && (dsTexture.Info.ArrayCount != rtvTexture.Info.ArrayCount))
				    || ((dsTexture.Info.TextureType == TextureType.Texture3D) && (dsTexture.Info.Depth != rtvTexture.Info.Depth)))
				{
					throw new ArgumentException(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_RESOURCE_MISMATCH, nameof(depthStencilView));
				}
			}

			// Only check if we have more than 1 render target view being applied.
			if (renderTargetViews.Length < 2)
			{
				return;
			}

			// Check for duplicates.
			if (renderTargetViews.Count(item => item == startView) > 1)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_RTV_ALREADY_BOUND, startView.Resource.Name), nameof(renderTargetViews));
			}

			// Check for type mismatch.
			if (otherViews.Any(item => item.Resource.ResourceType != startView.Resource.ResourceType))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_RTV_NOT_SAME_TYPE, startView.Resource.Name), nameof(renderTargetViews));
			}

			IEnumerable<GorgonTexture> otherTextures = from otherRtv in otherViews
			                                           let otherTexture = otherRtv.Resource as GorgonTexture
			                                           where otherTexture != null && otherTexture != rtvTexture
			                                           select otherTexture;

			// Check multisampling info.
			if (otherTextures.Any(item => !item.Info.MultiSampleInfo.Equals(rtvTexture.Info.MultiSampleInfo)))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_RTV_MULTISAMPLE_MISMATCH,
				                                          rtvTexture.Info.MultiSampleInfo.Quality,
				                                          rtvTexture.Info.MultiSampleInfo.Count),
				                            nameof(renderTargetViews));
			}

			//  Check for mismatch in size, array or depth.
			otherTextures = from otherTexture in otherTextures
			                where ((rtvTexture.Info.TextureType != TextureType.Texture3D && otherTexture.Info.ArrayCount != rtvTexture.Info.ArrayCount)
			                       || ((rtvTexture.Info.TextureType == TextureType.Texture2D && otherTexture.Info.Depth != rtvTexture.Info.Depth))
			                       || (rtvTexture.Info.Width != otherTexture.Info.Width)
			                       || (rtvTexture.Info.Height != otherTexture.Info.Height))
			                select otherTexture;


			if (otherTextures.Any())
			{
				throw new ArgumentException(Resources.GORGFX_ERR_RTV_RESOURCE_MISMATCH, nameof(renderTargetViews));
			}
			// ReSharper restore PossibleMultipleEnumeration
#endif
		}

		/// <summary>
		/// Function to clean up the categorized interfaces.
		/// </summary>
		private void DestroyInterfaces()
        {
			Fonts?.CleanUp();
			Fonts = null;
			Textures?.CleanUp();
			Textures = null;
			Shaders?.CleanUp();
			Shaders = null;
			Output?.CleanUp();
			Output = null;
			Rasterizer?.CleanUp();
			Rasterizer = null;
        }

        /// <summary>
        /// Function to create and initialize the various state objects.
        /// </summary>
        private void CreateStates()
        {
			_currentRenderTargets = new List<GorgonRenderTargetView>(VideoDevice.MaxRenderTargetViewSlots);
			_D3DRenderTargets = new D3D11.RenderTargetView[VideoDevice.MaxRenderTargetViewSlots];
			_currentUavs = new List<GorgonUnorderedAccessView>(VideoDevice.MaxRenderTargetViewSlots);
			_D3DUavs = new D3D11.UnorderedAccessView[VideoDevice.MaxRenderTargetViewSlots];
			_offsets = new List<int>(VideoDevice.MaxRenderTargetViewSlots);

            // Create interfaces.
            Rasterizer = new GorgonRasterizerRenderState(this);
            Input = new GorgonInputGeometry(this);
            Shaders = new GorgonShaderBinding(this);
            Output = new GorgonOutputMerger(this);
            Textures = new GorgonTextures(this);
            Fonts = new GorgonFonts(this);
            Buffers = new GorgonBuffers(this);

            ClearState();
        }

        /// <summary>
        /// Function to retrieve a list of all swap chains that are currently full screen.
        /// </summary>
        /// <returns>The list of full screen swap chains.</returns>
        internal IEnumerable<GorgonSwapChain> GetFullScreenSwapChains()
        {
	        return new GorgonSwapChain[0];
	        /*
            return (from graphicsObj in _trackedObjects
                    let swap = graphicsObj as GorgonSwapChain
                    where (swap != null) && (!swap.Info.IsWindowed)
                    select swap);*/
        }

		public void UpdateSubResource(GorgonResource resource, GorgonPointerBase data, int destSubResourceIndex = 0, GorgonBox? destRegion = null, int srcRowPitch = 0, int srcDepthSlicePitch = 0, UpdateSubResourceFlags flags = UpdateSubResourceFlags.None)
		{
			D3D11.ResourceRegion? region = destRegion?.ToResourceRegion();

			D3DDeviceContext.UpdateSubresource1(resource.D3DResource, destSubResourceIndex, region, new IntPtr(data.Address), srcRowPitch, srcDepthSlicePitch, (int)flags);
		}

        /// <summary>
        /// Function to add an object for tracking by the main Gorgon interface.
        /// </summary>
        /// <param name="trackedObject">Object to add.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="trackedObject"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <remarks>This allows Gorgon to track objects and destroy them upon <see cref="GorgonApplication.Quit">termination</see>.</remarks>
        public void AddTrackedObject(IDisposable trackedObject)
        {
            if (trackedObject == null)
            {
                throw new ArgumentNullException(nameof(trackedObject));
            }

            _trackedObjects.Add(trackedObject);
        }

        /// <summary>
        /// Function to remove a tracked object from the Gorgon interface.
        /// </summary>
        /// <param name="trackedObject">Object to remove.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="trackedObject"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <remarks>This will -not- destroy the tracked object.</remarks>
        public void RemoveTrackedObject(IDisposable trackedObject)
        {
            if (trackedObject == null)
            {
                throw new ArgumentNullException(nameof(trackedObject));
            }

            _trackedObjects.Remove(trackedObject);
        }

        /// <summary>
        /// Function to a list of objects being tracked by a type value.
        /// </summary>
        /// <typeparam name="T">Type to search for.</typeparam>
        /// <returns>A list of objects that match the type.</returns>
        public IList<T> GetTrackedObjectsOfType<T>()
            where T : IDisposable
        {
            return (from trackedObject in _trackedObjects
                    where trackedObject is T
                    select (T)trackedObject).ToArray();
        }

		/// <summary>
		/// Function to bind the currently active render target views and any applicable depth/stencil view to the pipeline.
		/// </summary>
		/// <param name="renderTargetViews">An array of <see cref="GorgonRenderTargetView"/> objects to bind to the pipeline.</param>
		/// <param name="depthStencilView">[Optional] The current depth/stencil view to bind to the pipeline</param>
		/// <exception cref="ArgumentException">Thrown when a <see cref="GorgonRenderTargetView"/> is already bound to a slot. 
		/// <para>-or-</para>
		/// <para>Thrown when any of the render target views and/or depth stencil view do not have the same array/depth count.</para>
		/// <para>-or-</para>
		/// <para>Thrown when any render target and/or depth stencil view is not the same type.</para>
		/// <para>-or-</para>
		/// <para>Thrown when any render target and/or depth stencil has a width/height/depth (or array count) mismatch.</para>
		/// <para>-or-</para>
		/// <para>Thrown when any render target and/or depth stencil multisampling quality/count is not the same.</para>
		/// <para>This exception is only thrown when Gorgon is compiled as DEBUG.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// This will bind <see cref="GorgonRenderTargetView"/> objects and an optional <see cref="GorgonDepthStencilView"/> to the pipeline when rendering. Users should bind a minimum of a single render target 
		/// (e.g. a <see cref="GorgonSwapChain"/>) to be able to visualize graphical data. 
		/// </para>
		/// <para>
		/// The number of render targets passed to the <paramref name="renderTargetViews"/> parameter will be limited to the number of render target slots for the device. The total number of available slots 
		/// is available through the <see cref="IGorgonVideoDevice.MaxRenderTargetViewSlots"/> value on the <see cref="VideoDevice"/> property. If the number of render target views passed exceeds this value, 
		/// then Gorgon will only set up to the maximum value and no more. No exception will be thrown. If the number of render targets is less than the maximum, then the remaining slots will be set to 
		/// <b>null</b>.
		/// </para> 
		/// <para>
		/// If <b>null</b> is passed to the <paramref name="renderTargetViews"/> parameter, then all render targets are unbound. However, if the <paramref name="depthStencilView"/> is specified, it will be 
		/// bound regardless of whether there are render target views to bind or not.
		/// </para>
		/// <para>
		/// All resources bound to the views passed to <paramref name="renderTargetViews"/> (and <paramref name="depthStencilView"/>) must meet the following criteria:
		/// <list type="bullet">
		///		<item>
		///			<description>Share the same type. That is one of <see cref="TextureType.Texture1D"/>, <see cref="TextureType.Texture2D"/> or <see cref="TextureType.Texture3D"/>.</description>
		///		</item>
		///		<item>
		///			<description>The same array size, or depth in the case of <see cref="TextureType.Texture3D"/>.</description>
		///		</item>
		///		<item>
		///			<description>The same multisample quality and count.</description>
		///		</item>
		/// </list>
		/// If any of these conditions are not met, then an exception will be thrown (when Gorgon is compiled in DEBUG mode).
		/// </para>
		/// <para>
		/// The same <see cref="GorgonRenderTargetView"/> cannot be bound to multiple render target slots simultaneously. However, you may set multiple non-overlapping resource views of a single resource as 
		/// simultaneous multiple render targets. 
		/// </para>
		/// <para>
		/// If the <paramref name="renderTargetViews"/> (and <paramref name="depthStencilView"/>) uses an array count, then all views must have the same array count.
		/// </para>
		/// <para>
		/// Because unordered access views (UAVs) share the same slots as render target views, calling this method will unbind any existing unordered access views.
		/// </para>
		/// </remarks>
		public void SetRenderTargets(GorgonRenderTargetView[] renderTargetViews, GorgonDepthStencilView depthStencilView = null)
		{
#if DEBUG
			ValidateRenderTargetDepthStencilViews(renderTargetViews, depthStencilView);
#endif

			_currentRenderTargets.Clear();
			_currentUavs.Clear();
			_currentDepthStencilView = null;

			// If we've cleared the state, then clear our underlying state and leave.
			if (((renderTargetViews == null) || (renderTargetViews.Length == 0))
				&& (depthStencilView == null))
			{
				D3DDeviceContext.OutputMerger.SetTargets();
				return;
			}

			_currentDepthStencilView = depthStencilView;

			// If we've set a list of render targets, then record the state for those.
			if ((renderTargetViews != null) && (renderTargetViews.Length > 0))
			{
				for (int i = 0; i < renderTargetViews.Length.Min(_D3DRenderTargets.Length); ++i)
				{
					GorgonRenderTargetView rtv = renderTargetViews[i];

					_currentRenderTargets.Add(rtv);
					_D3DRenderTargets[i] = rtv?.D3DRenderTargetView;
				}
			}

			D3DDeviceContext.OutputMerger.SetTargets(_currentDepthStencilView?.D3DView, _currentRenderTargets.Count, _D3DRenderTargets);
		}

		/// <summary>
		/// Function to clear a specific render target view.
		/// </summary>
		/// <param name="view">The <see cref="GorgonRenderTargetView"/> to clear.</param>
		/// <param name="color">The color used to fill the view with.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="view"/> parameter is <b>null</b>. This is only thrown when Gorgon is compiled in DEBUG mode.</exception>
		public void ClearRenderTargetView(GorgonRenderTargetView view, GorgonColor color)
		{
			view.ValidateObject(nameof(view));

			D3DDeviceContext.ClearRenderTargetView(view.D3DRenderTargetView, color.ToRawColor4());
		}

        /// <summary>
        /// Function to clear the states for the graphics object.
        /// </summary>
        /// <param name="flush">[Optional] <b>true</b> to flush the queued graphics object commands, <b>false</b> to leave as is.</param>
        /// <remarks>If <paramref name="flush"/> is set to <b>true</b>, then a performance penalty is incurred.</remarks>
        public void ClearState(bool flush = false)
        {
            if (flush)
            {
				VideoDevice.D3DDeviceContext().Flush();
            }

	        _currentRenderTargets.Clear();
	        _currentDepthStencilView = null;
	        _currentUavs.Clear();

			for (int i = 0; i < _D3DRenderTargets.Length; ++i)
	        {
		        _D3DRenderTargets[i] = null;
	        }

	        for (int i = 0; i < _D3DUavs.Length; ++i)
	        {
		        _D3DUavs[i] = null;
	        }

            // Set default states.
            Input?.Reset();
            Rasterizer?.Reset();
            Output?.Reset();
            Shaders?.Reset();
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGraphics"/> class.
		/// </summary>
		/// <param name="videoDeviceInfo">[Optional] A <see cref="VideoDeviceInfo"/> to specify the video device to use for this instance.</param>
		/// <param name="featureLevel">[Optional] The requested feature level for the video device used with this object.</param>
		/// <param name="log">[Optional] The log to use for debugging.</param>
		/// <exception cref="GorgonException">Thrown when the <paramref name="featureLevel"/> is unsupported.</exception>
		/// <remarks>
		/// <para>
		/// When the <paramref name="videoDeviceInfo"/> is set to <b>null</b> (<i>Nothing</i> in VB.Net), Gorgon will use the first video device with feature level specified by <paramref name="featureLevel"/>  
		/// will be used. If the feature level requested is higher than what any device in the system can support, then the first device with the highest feature level will be used.
		/// </para>
		/// <para>
		/// When specifying a feature level, the device with the closest matching feature level will be used. If the <paramref name="videoDeviceInfo"/> is specified, then that device will be used at the 
		/// requested <paramref name="featureLevel"/>. If the requested <paramref name="featureLevel"/> is higher than what the <paramref name="videoDeviceInfo"/> will support, then Gorgon will use the 
		/// highest feature of the specified <paramref name="videoDeviceInfo"/>. 
		/// </para>
		/// <para>
		/// If Gorgon is compiled in DEBUG mode, and <see cref="VideoDeviceInfo"/> is <b>null</b>, then it will attempt to find the most appropriate hardware video device, and failing that, will fall 
		/// back to a software device (WARP).
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// The Gorgon Graphics library only works on Windows 7 (with the Platform Update for Direct 3D 11.1) or better. No other operating system is supported at this time.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		/// <example>
		/// <para>
		/// The following examples show the various ways the object can be configured:
		/// </para>
		/// <code lang="csharp">
		/// <![CDATA[
		/// // Create using the first video device with the highest feature level:
		/// var graphics = new GorgonGraphics();
		/// 
		/// // Create using a specific video device and use the highest feature level supported by that device:
		/// // Get a list of available video devices.
		/// IGorgonVideoDeviceList videoDevices = new GorgonVideoDeviceList(log);
		/// videoDevices.Enumerate(true);
		/// var graphics = new GorgonGraphics(videoDevices[0]);
		/// 
		/// // Create using the requested feature level and the first adapter that supports the nearest feature level requested:
		/// // If the device does not support 11.0, then the device with the nearest feature level (e.g. 10.1) will be used instead.
		/// var graphics = new GorgonGraphics(null, FeatureLevel.Level_11_0);
		/// 
		/// // Create using the requested device and the requested feature level:
		/// // If the device does not support 11.0, then the highest feature level supported by the device will be used (e.g. 10.1).
		/// IGorgonVideoDeviceList videoDevices = new GorgonVideoDeviceList(log);
		/// videoDevices.Enumerate(true);
		/// var graphics = new GorgonGraphics(videoDevices[0], FeatureLevel.Level_11_0); 
		/// ]]>
		/// </code>
		/// </example>
		/// <seealso cref="VideoDeviceInfo"/>
		public GorgonGraphics(IGorgonVideoDeviceInfo videoDeviceInfo, FeatureLevelSupport? featureLevel = null, IGorgonLog log = null)
		{
			if (!Win32API.IsWindows7SP1OrGreater())
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_INVALID_OS);
			}

			// If we've not specified a feature level, or the feature level exceeds the requested device feature level, then 
			// fall back to the device feature level.
			if ((featureLevel == null) || (videoDeviceInfo.SupportedFeatureLevel < featureLevel.Value))
			{
				featureLevel = videoDeviceInfo.SupportedFeatureLevel;
			}

			// We only support feature level 10 and greater.
			if (!Enum.IsDefined(typeof(FeatureLevelSupport), featureLevel.Value))
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FEATURE_LEVEL_INVALID, featureLevel));
			}

			_log = log ?? GorgonLogDummy.DefaultInstance;
			ResetFullscreenOnFocus = true;
			
			_log.Print("Gorgon Graphics initializing...", LoggingLevel.Simple);

            // Track our objects.
            _trackedObjects = new GorgonDisposableObjectCollection();

			_log.Print($"Using video device '{videoDeviceInfo.Name}' at feature level [{featureLevel.Value}] for Direct 3D 11.1.", LoggingLevel.Simple);

			_videoDevice = new VideoDevice(videoDeviceInfo, featureLevel.Value, _log);

			CreateStates();

			GorgonApplication.Log.Print("Gorgon Graphics initialized.", LoggingLevel.Simple);
		}

		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Gorgon.Native.Win32API.DwmIsCompositionEnabled(System.Boolean@)")]
		static GorgonGraphics()
		{
			// Only do this on Windows 7.  On Windows 8 this value could be wrong and is not needed.
			if ((GorgonComputerInfo.OperatingSystemVersion.Major == 6)
			    || (GorgonComputerInfo.OperatingSystemVersion.Minor == 1))
			{
				if (Win32API.DwmIsCompositionEnabled(out _isDWMEnabled) != 0) // S_OK
				{
					_isDWMEnabled = false;
				}
			}

			if (!_isDWMEnabled)
			{
				_dontEnableDWM = true;
			}

			DX.Configuration.ThrowOnShaderCompileError = false;

#if DEBUG
			IsDebugEnabled = true;
#endif
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			IGorgonVideoDevice device = Interlocked.Exchange(ref _videoDevice, null);
			
			if (device == null)
			{
				return;
			}

			// Reset the state for the context. This will ensure we don't have anything bound to the pipeline when we shut down.
			device.D3DDeviceContext().ClearState();

			_currentRenderTargets.Clear();
			_currentDepthStencilView = null;
			_currentUavs.Clear();
			_D3DRenderTargets = null;
			_D3DUavs = null;

			_trackedObjects.Clear();
			DestroyInterfaces();
			device.Dispose();
		}
#endregion
	}
}
