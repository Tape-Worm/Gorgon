#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Wednesday, December 16, 2015 7:25:15 PM
// 
#endregion

using System;
using System.Linq;
using System.Threading;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using Gorgon.Native;
using System.Collections.Generic;
using Gorgon.Math;
using D3D12 = SharpDX.Direct3D12;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A swap chain.
	/// </summary>
	/// <remarks>
	/// // TODO: add more information.
	/// </remarks>
	public class GorgonSwapChain
		: GorgonNamedObject, IDisposable
	{
		#region Classes.
		/// <summary>
		/// Frame resource for a swap chain.
		/// </summary>
		private class SwapFrame
			: IDisposable
		{
			#region Properties.
			/// <summary>
			/// Property to return the allocator used for a command list on this frame.
			/// </summary>
			public D3D12.CommandAllocator Allocator
			{
				get;
			}

			/// <summary>
			/// Property to return the command used used for this frame.
			/// </summary>
			public D3D12.GraphicsCommandList CommandList
			{
				get;
			}

			/// <summary>
			/// Property to set or return the fence value for this frame.
			/// </summary>
			public long Fence
			{
				get;
				set;
			}

			/// <summary>
			/// Property to return the texture for this frame.
			/// </summary>
			public GorgonTexture2D Texture
			{
				get;
			}

			/// <summary>
			/// Property to return the index for this frame.
			/// </summary>
			public int Index
			{
				get;
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to reset the frame allocator and command list.
			/// </summary>
			/// <param name="state">Initial state for the frame.</param>
			public void Reset(D3D12.PipelineState state = null)
			{
				Allocator.Reset();
				CommandList.Reset(Allocator, state);
			}

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose()
			{
				Texture?.Dispose();
				CommandList?.Dispose();
				Allocator?.Dispose();	
			}
			#endregion

			#region Constructor/Finalizer.
			/// <summary>
			/// Initializes a new instance of the <see cref="SwapFrame"/> class.
			/// </summary>
			/// <param name="index">The index of the back buffer frame.</param>
			/// <param name="swapChain">The swap chain containing the back buffer.</param>
			public SwapFrame(int index, GorgonSwapChain swapChain)
			{
				Texture = new GorgonTexture2D(swapChain, index);
				Index = index;
				Tuple<D3D12.CommandAllocator, D3D12.GraphicsCommandList> commandTuple = swapChain.Graphics.GraphicsCommander.CreateCommandList();
				Allocator = commandTuple.Item1;
				CommandList = commandTuple.Item2;
			}
			#endregion
		}
		#endregion

		#region Variables.
		// The DXGI swap chain for this swap chain.
		private DXGI.SwapChain3 _swapChain;
		// Back buffer textures for this swap chain.
		private SwapFrame[] _backBuffers = new SwapFrame[0];
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the DXGI Swap chain wrapped by this object.
		/// </summary>
		internal DXGI.SwapChain3 DXGISwapChain => _swapChain;

		/// <summary>
		/// Property to return the <see cref="GorgonVideoOutputInfo"/> used for full screen output.
		/// </summary>
		/// <remarks>
		/// When the swap chain has its <see cref="IsWindowed"/> flag set to <b>false</b>, this will be set to <b>null</b> (<i>Nothing</i> in VB.Net), otherwise it will return the <see cref="GorgonVideoOutputInfo"/> 
		/// used to display the swap chain in full screen mode.
		/// </remarks>
		public GorgonVideoOutputInfo FullScreenOutput
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the current index of the back buffer that is in use.
		/// </summary>
		public int CurrentBackBufferIndex => _swapChain?.CurrentBackBufferIndex ?? 0;

		/// <summary>
		/// Property to return the current video mode used by the swap chain while in full screen mode.
		/// </summary>
		/// <remarks>
		/// When the swap chain has its <see cref="IsWindowed"/> flag set to <b>false</b>, this will be set to <b>null</b> (<i>Nothing</i> in VB.Net), otherwise it will return the <see cref="GorgonVideoMode"/> 
		/// used to display the swap chain in full screen mode.
		/// </remarks>
		public GorgonVideoMode? FullScreenMode
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return information about this swap chain.
		/// </summary>
		public GorgonSwapChainInfo Info
		{
			get;
		}

		/// <summary>
		/// Property to return the <see cref="GorgonGraphics"/> object linked to this object.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
		}

		/// <summary>
		/// Property to return whether the swap chain is using windowed mode or not.
		/// </summary>
		public bool IsWindowed => !_swapChain.IsFullScreen;
		#endregion

		#region Methods.
/*		/// <summary>
		/// Function to validate the settings for the swap chain.
		/// </summary>
		private void ValidateSettings()
		{
			bool isMultiSampled = !Info.MultiSampleInfo.Equals(GorgonMultiSampleInfo.NoMultiSampling);

			// Check for a valid window handle.
			if (Info.WindowHandle == IntPtr.Zero)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_SWAPCHAIN_WINDOW_REQUIRED);
			}

			if ((Info.BackBufferCount < 2) && (Info.PresentMode == PresentMode.Discard))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_SWAPCHAIN_DISCARD_NEEDS_2_OR_MORE);
			}

			if ((Info.PresentMode == PresentMode.Sequential)
				&& (Info.Format != BufferFormat.R8G8B8A8_UNorm)
				&& (Info.Format != BufferFormat.B8G8R8A8_UNorm)
				&& (Info.Format != BufferFormat.R16G16B16A16_Float))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_SWAPCHAIN_SEQ_FORMATSUPPORT);
			}

			// Ensure that we have at least 1 of these flags.
			if (((Info.Usage & SwapUsage.RenderTarget) != SwapUsage.RenderTarget)
			    && ((Info.Usage & SwapUsage.ShaderInput) != SwapUsage.ShaderInput))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_SWAPCHAIN_MUST_BE_TARGET_OR_SHADERINPUT);
			}

			// If we've specified UAV access, ensure that the feature level allows it.
			if ((Graphics.VideoDevice.RequestedFeatureLevel < DeviceFeatureLevel.FeatureLevel11_0)
				&& ((Info.Usage & SwapUsage.UnorderedAccess) == SwapUsage.UnorderedAccess))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_UAV_FEATURELEVEL_REQUIRED);
			}

			if ((Info.Width < 1) || (Info.Height < 1))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_SWAPCHAIN_SIZE_TOO_SMALL);
			}

			if ((!Graphics.VideoDevice.SupportsDisplayFormat(Info.Format))
				|| (!Graphics.VideoDevice.SupportsRenderTargetFormat(Info.Format, isMultiSampled)))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_SWAPCHAIN_FORMAT_INVALID);
			}

			// Multi sampling is only available for blt modes.
			if ((isMultiSampled) && (Info.PresentMode == PresentMode.Sequential))
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_PRESENTMODE_NO_MULTISAMPLE, Info.PresentMode));
			}

			// Ensure that the quality of the sampling does not exceed what the card can do.
			// While standard is supposed to guaranteed for all D3D 10+ devices, centered may not be. However, I cannot find a way to 
			// test for that, so we'll just have to assume it'll work.
			if ((!isMultiSampled) || (Info.MultiSampleInfo.Quality == GorgonMultiSampleInfo.StandardMultiSamplePatternQuality) ||
			    (Info.MultiSampleInfo.Quality == GorgonMultiSampleInfo.CenteredMultiSamplePatternQuality))
			{
				return;
			}

			int multiSampleQuality = Graphics.VideoDevice.GetMultiSampleQuality(Info.Format, Info.MultiSampleInfo.Count);
				
			if ((Info.MultiSampleInfo.Quality >= multiSampleQuality)
			    || (Info.MultiSampleInfo.Quality < 0))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
				                          string.Format(Resources.GORGFX_ERR_MULTISAMPLE_INVALID,
				                                        Info.MultiSampleInfo.Count,
				                                        Info.MultiSampleInfo.Quality,
				                                        Info.Format));
			}
		}

		/// <summary>
		/// Function to find the most suitable output based on the window.
		/// </summary>
		/// <returns>The output to use.</returns>
		private IGorgonVideoOutputInfo GetOutput(IGorgonVideoDeviceInfo deviceInfo)
		{
			IntPtr monitor = Win32API.MonitorFromWindow(Info.WindowHandle, MonitorFlags.MONITOR_DEFAULTTOPRIMARY);

			// Find the video output for the window.
			return (from videoOutput in deviceInfo.Outputs
			        where videoOutput.MonitorHandle == monitor
			        select videoOutput).SingleOrDefault();
		}*/

		/// <summary>
		/// Function to create the D3D 12 backing resources for this swap chain.
		/// </summary>
		private void CreateResources()
		{
			long[] fenceValues = null;

			// If we're recreating the resources (e.g after a resize, then dispose of them first).
			if (_backBuffers != null)
			{
				// Store the previous fence values for the buffers.
				fenceValues = _backBuffers.Select(item => item.Fence).ToArray();

				foreach (SwapFrame buffer in _backBuffers)
				{
					buffer.Dispose();
				}
			}

			_backBuffers = new SwapFrame[Info.BackBufferCount];

			// Restore the previous fences.
			if (fenceValues != null)
			{
				for (int i = 0; i < fenceValues.Length.Min(Info.BackBufferCount); ++i)
				{
					_backBuffers[i].Fence = fenceValues[i];
				}
			}

			for (int i = 0; i < _backBuffers.Length; ++i)
			{
				_backBuffers[i] = new SwapFrame(i, this);
			}
		}

		/// <summary>
		/// Function used to create the swap chain.
		/// </summary>
		private void CreateSwapChain()
		{
			using (DXGI.Factory4 factory4 = Graphics.DXGIAdapter.GetParent<DXGI.Factory4>())
			{
				factory4.MakeWindowAssociation(Info.WindowHandle, DXGI.WindowAssociationFlags.IgnoreAll);

				var desc = new DXGI.SwapChainDescription1
				           {
					           Format = (DXGI.Format)Info.Format,
					           AlphaMode = (DXGI.AlphaMode)Info.AlphaMode,
					           BufferCount = Info.BackBufferCount,
					           Usage = (DXGI.Usage)Info.Usage,
					           Scaling = (DXGI.Scaling)Info.ScalingBehavior,
					           SwapEffect = (DXGI.SwapEffect)Info.PresentMode,
					           Flags = DXGI.SwapChainFlags.AllowModeSwitch,
					           Height = Info.Height,
					           Width = Info.Width,
					           SampleDescription = GorgonMultiSampleInfo.NoMultiSampling.Convert(),
					           Stereo = false
				           };

				using (DXGI.SwapChain1 swap1 = new DXGI.SwapChain1(factory4, Graphics.GraphicsCommander.D3DCommandQueue, Info.WindowHandle, ref desc))
				{
					_swapChain = swap1.QueryInterface<DXGI.SwapChain3>();
					CreateResources();
				}
			}
		}

		/// <summary>
		/// Function to clear the back buffer to a specific color, and optionally the depth/stencil buffer to a specific value.
		/// </summary>
		/// <param name="color">The color to apply to the back buffer.</param>
		/// <param name="depth">[Optional] The value used to fill the depth buffer.</param>
		/// <param name="stencil">[Optional] The value used to fill the stencil buffer.</param>
		public void Clear(GorgonColor color, float depth = 1.0f, int stencil = 0)
		{
			SwapFrame frame = _backBuffers[CurrentBackBufferIndex];
			Graphics.GraphicsCommander.WaitForFence(frame.Fence);

			frame.Reset();

			frame.CommandList.ResourceBarrierTransition(frame.Texture.D3DResource, D3D12.ResourceStates.Present, D3D12.ResourceStates.RenderTarget);
			frame.CommandList.ClearRenderTargetView(frame.Texture.RTVHandle, color.ToRawColor4());
		}

		/// <summary>
		/// Function to present the contents of the current back buffer to the display.
		/// </summary>
		/// <param name="interval">VSync interval to use when presenting.</param>
		public void Present(int interval)
		{
			SwapFrame frame = _backBuffers[CurrentBackBufferIndex];

			frame.CommandList.ResourceBarrierTransition(frame.Texture.D3DResource, D3D12.ResourceStates.RenderTarget, D3D12.ResourceStates.Present);
			frame.CommandList.Close();

			frame.Fence = Graphics.GraphicsCommander.ExecuteCommandList(frame.CommandList);

			DXGISwapChain.Present(interval, 0);
		}

		/*/// <summary>
		/// Function to set the swap chain to use windowed mode.
		/// </summary>
		public void SetWindowed()
		{
			if ((IsWindowed) || (_output == null))
			{
				return;
			}
			
			if (_output != null)
			{
				_swapChain.Value.SetFullscreenState(false, null);
			}

			_output?.Dispose();
			_output = null;

			FullscreenMode = null;
			FullscreenOutput = null;
			IsWindowed = false;
		}

		/// <summary>
		/// Function to set the swap chain into full screen mode.
		/// </summary>
		/// <param name="mode">The video mode to use for full screen mode.</param>
		/// <param name="output">[Optional] A <see cref="IGorgonVideoOutputInfo"/> representing the output to use for full screen mode.</param>
		/// <remarks>
		/// <para>
		/// If the <paramref name="output"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net), then the output that contains the majority of the client area of the the bound window will be used as the 
		/// output for full screen mode.
		/// </para>
		/// </remarks>
		public void SetFullscreen(GorgonVideoMode mode, IGorgonVideoOutputInfo output = null)
		{
			DXGI.SwapChain2 swapChain = _swapChain.Value;

			// If no outputs are available or no video modes are available, do nothing.
			// This can happen on reference rasterizer or WARP devices, or if the application is running
			// in an RDP session.
			if ((Graphics.VideoDevice.Info.Outputs.Count == 0)
				|| (Graphics.VideoDevice.Info.Outputs.All(item => item.VideoModes.Count == 0)))
			{
				return;
			}

			if (output == null)
			{
				output = GetOutput(Graphics.VideoDevice.Info);
			}

			if (output == null)
			{
				throw new GorgonException(GorgonResult.NotInitialized, Resources.GORGFX_ERR_SWAPCHAIN_NO_OUTPUT);
			}

			// Reset to windowed mode first.
			SetWindowed();

			if (!Graphics.VideoDevice.SupportsDisplayFormat(Info.Format))
			{
				throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORGFX_ERR_SWAPCHAIN_FORMAT_INVALID, Info.Format));
			}

			// Get the DXGI output so we can force the swap chain to a specific output.
			using (DXGI.Output output0 = _adapter.GetOutput(output.Index))
			{
				_output = output0.QueryInterface<DXGI.Output3>();
			}

			DXGI.ModeDescription nearestMode;

			// If we can't find the mode in the output video mode list, then fall back to letting the card pick the nearest video 
			// mode.
			if (output.VideoModes.All(item => item.Width != mode.Width
			                                  && item.Height != mode.Height
			                                  && item.Format != mode.Format
			                                  && !item.RefreshRate.Equals(mode.RefreshRate)))
			{
				_output.GetClosestMatchingMode(_d3dDevice, mode.ToModeDesc(), out nearestMode);
			}
			else
			{
				nearestMode = mode.ToModeDesc();
			}

			swapChain.ResizeTarget(ref nearestMode);
			swapChain.SetFullscreenState(true, _output);
			// TODO: 
			//swapChain.ResizeBuffers(Info.BackBufferCount, nearestMode.Width, nearestMode.Height, nearestMode.Format, DXGI.SwapChainFlags.AllowModeSwitch);

			FullscreenOutput = output;
			FullscreenMode = new GorgonVideoMode(nearestMode.Width, nearestMode.Height, (BufferFormat)nearestMode.Format, nearestMode.RefreshRate.FromRational());
		}*/

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if ((DXGISwapChain != null) && (_backBuffers != null) && (_backBuffers.Length > 0))
			{
				Graphics?.GraphicsCommander.WaitForFence(_backBuffers[CurrentBackBufferIndex].Fence);
			}
/*
			// Set windowed mode before shutting everything down.
			SetWindowed();
*/

			if (_backBuffers != null)
			{
				foreach (SwapFrame frame in _backBuffers)
				{
					frame.Dispose();
				}
			}

			_swapChain?.Dispose();
			_swapChain = null;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSwapChain"/> class.
		/// </summary>
		/// <param name="name">The name for this swap chain.</param>
		/// <param name="graphics">The <see cref="GorgonGraphics"/> interface that is responsible for this swap chain.</param>
		/// <param name="info">A <see cref="GorgonSwapChainInfo"/> containing the necessary data used to create the swap chain.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="info"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		public GorgonSwapChain(string name, GorgonGraphics graphics, GorgonSwapChainInfo info)
			: base(name)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			if (graphics == null)
			{
				throw new ArgumentNullException(nameof(graphics));
			}

			Graphics = graphics;
			Info = info;

			CreateSwapChain();

			/*
			
			Info = info;
			
			

			if (GorgonGraphics.EnableDebug)
			{
				ValidateSettings();
			}

			_d3dDevice = ((ISdxVideoDevice)graphics.VideoDevice).GetD3DDevice();
			_adapter = ((ISdxVideoDevice)graphics.VideoDevice).GetDXGIAdapter();*/
		}
		#endregion
	}
}
