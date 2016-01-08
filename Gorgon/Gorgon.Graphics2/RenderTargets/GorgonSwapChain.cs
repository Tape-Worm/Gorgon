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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using Gorgon.Math;
using Gorgon.Native;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A swap chain.
	/// </summary>
	/// <remarks>
	/// // TODO: add more information.
	/// </remarks>
	public class GorgonSwapChain
		: IGorgonGraphicsObject, IDisposable
	{
		#region Variables.
		// The DXGI swap chain for this swap chain.
		private Lazy<DXGI.SwapChain2> _swapChain;
		// The D3D11 device for the video device.
		private readonly D3D11.Device2 _d3dDevice;
		// The DXGI output used for full screen mode.
		private DXGI.Output3 _output;
		// The DXGI adapter used by the video device.
		private readonly DXGI.Adapter2 _adapter;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the <see cref="IGorgonVideoOutputInfo"/> used for full screen output.
		/// </summary>
		public IGorgonVideoOutputInfo FullscreenOutput
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the current video mode used by the swap chain while in full screen mode.
		/// </summary>
		public GorgonVideoMode? FullscreenMode
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return information about this swap chain.
		/// </summary>
		public IGorgonSwapChainInfo Info
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
		public bool IsWindowed
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
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
		}

		/// <summary>
		/// Function used to create the swap chain.
		/// </summary>
		/// <returns>A new DXGI swap chain.</returns>
		private DXGI.SwapChain2 CreateSwapChain()
		{
			using (DXGI.Factory3 factory3 = _adapter.GetParent<DXGI.Factory3>())
			{
				factory3.MakeWindowAssociation(Info.WindowHandle, DXGI.WindowAssociationFlags.IgnoreAll);

				var desc = new DXGI.SwapChainDescription1
				           {
					           Format = (DXGI.Format)Info.Format,
					           AlphaMode = (DXGI.AlphaMode)Info.AlphaMode,
					           BufferCount = Info.BackBufferCount,
					           Usage = (DXGI.Usage)Info.Usage,
					           Scaling = (DXGI.Scaling)Info.ScalingBehavior,
					           SwapEffect = (DXGI.SwapEffect)Info.PresentMode,
					           Flags = DXGI.SwapChainFlags.None,
					           Height = Info.Height,
					           Width = Info.Width,
					           SampleDescription = Info.MultiSampleInfo.Convert(),
					           Stereo = false
				           };

				using (DXGI.SwapChain1 swap1 = new DXGI.SwapChain1(factory3, _d3dDevice, Info.WindowHandle, ref desc))
				{
					return swap1.QueryInterface<DXGI.SwapChain2>();
				}
			}
		}

		/// <summary>
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
		}

		/// <summary>
		/// Function to copy the contents of a swap chain back buffer to the front buffer using the technique described in the <see cref="PresentMode"/>.
		/// </summary>
		public void Present()
		{
			DXGI.SwapChain2 test = _swapChain.Value;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (!_swapChain.IsValueCreated)
			{
				return;
			}

			// Set windowed mode before shutting everything down.
			SetWindowed();

			_swapChain.Value.Dispose();
			_swapChain = null;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSwapChain"/> class.
		/// </summary>
		/// <param name="graphics">The <see cref="GorgonGraphics"/> interface that is responsible for this swap chain.</param>
		/// <param name="info">A <see cref="IGorgonSwapChainInfo"/> containing the necessary data used to create the swap chain.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="info"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		public GorgonSwapChain(GorgonGraphics graphics, IGorgonSwapChainInfo info)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			if (graphics == null)
			{
				throw new ArgumentNullException(nameof(graphics));
			}

			IsWindowed = true;
			Info = info;
			Graphics = graphics;
			_swapChain = new Lazy<DXGI.SwapChain2>(CreateSwapChain, true);

			if (GorgonGraphics.EnableDebug)
			{
				ValidateSettings();
			}

			_d3dDevice = ((ISdxVideoDevice)graphics.VideoDevice).GetD3DDevice();
			_adapter = ((ISdxVideoDevice)graphics.VideoDevice).GetDXGIAdapter();
		}
		#endregion
	}
}
