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
// Created: Wednesday, October 12, 2011 6:38:14 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.UI;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A swap chain used to display graphics to a window.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The swap chain is used to display graphics data into a window either through an exclusive full screen view of the rendering surface, or can be used to display the rendering data in the client area 
	/// of the window.
	/// </para> 
	/// <para>
	/// By default, if a window loses focus and the swap chain is in full screen, it will revert to windowed mode.  The swap chain will attempt to reacquire full screen mode when the window regains focus. 
	/// This functionality can be disabled with the <see cref="ExitFullScreenModeOnFocusLoss"/> property if it does not suit the needs of the developer.  Setting this value to <b>false</b> is mandatory in full 
	/// screen multi-monitor applications, if the <see cref="ExitFullScreenModeOnFocusLoss"/> flag is <b>false</b> in this scenario. Be aware that when this flag is set to <b>false</b>, the behaviour will be 
	/// unhandled and it will be the responsibility of the developer to handle application focus loss/restoration in multi-monitor environments.
	/// </para>
	/// <para>
	/// Multiple swap chains can be set to full screen on different video outputs.  When setting up for multiple video outputs in full screen, ensure that the window for the extra video output is located on 
	/// the monitor attached to that video output.  Failure to do so will keep the mode from switching.
	/// </para>	
	/// <para>
	/// <note type="note">
	/// <para>
	/// Note that due to a known limitation on Windows 7, it is not currently possible to switch to full screen on multiple outputs on multiple GPUs. One possible workaround is to create a full screen 
	/// borderless window on the secondary device and use that as a "fake" full screen mode.  If this workaround is applied, then it is suggested to disable the Desktop Windows Compositor. To disable the 
	/// compositor, set the <see cref="GorgonGraphics.IsDWMCompositionEnabled"/> property to <b>false</b> or see this link: 
	/// <a href="http://msdn.microsoft.com/en-us/library/aa969510.aspx">http://msdn.microsoft.com/en-us/library/aa969510.aspx</a> for instructions on doing this manually.
	/// </para>
	/// <para>
	/// This does not apply to full screen modes on multiple outputs for the same video device.
	/// </para>
	/// </note>
	/// </para>	
	/// </remarks>
	/// <seealso cref="EnterFullScreen"/>
	/// <seealso cref="ExitFullScreenModeOnFocusLoss"/>
	/// <seealso cref="GorgonGraphics.IsDWMCompositionEnabled"/>
	public sealed class GorgonSwapChain
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		// The log interface used for debug logging.
		private readonly IGorgonLog _log;
		// The back buffers for the swap chain.
		private GorgonTexture[] _backBufferTextures;
		// The parent form for the control bound to the swap chain.
		private readonly Form _parentForm;
		// Flag to indicate that the resize operation has completed.
		private bool _resized;
		// Flag to indicate that a screen state transition is in progress.
		private bool _screenStateTransition;
		// The previous output to use when restoring full screen mode.
		private IGorgonVideoOutputInfo _previousOutput;
		// The previous video mode to use when restoring full screen mode.
		private DXGI.ModeDescription1? _previousMode;
		// The backing store for the information used to create this swap chain.
		private readonly GorgonSwapChainInfo _info;
		#endregion

		#region Events.
		/// <summary>
		/// Event called before the swap chain has been resized.
		/// </summary>
		public event EventHandler<EventArgs> BeforeSwapChainResized;
		/// <summary>
		/// Event called after the swap chain has been resized.
		/// </summary>
		public event EventHandler<EventArgs> AfterSwapChainResized;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the DXGI swap chain interface.
		/// </summary>
		internal DXGI.SwapChain1 GISwapChain
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Property to return the video device used to create this swap chain.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
		}

		/// <summary>
		/// Property to return the textures used as back buffers for this swap chain.
		/// </summary>
		/// <remarks>
		/// For swap chains that were created with the flag <see cref="IGorgonSwapChainInfo.UseFlipMode"/> set to <b>true</b>, this value will return each texture for each back buffer. The first texture will 
		/// be read/write, but the subsequent textures will be read-only. Otherwise, this will only contain the first back buffer texture.
		/// </remarks>
		public IReadOnlyList<GorgonTexture> BackBufferTextures => _backBufferTextures;

		/// <summary>
		/// Property to return the render target view associated with this swap chain.
		/// </summary>
		public GorgonRenderTargetView RenderTargetView
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return whether the application should automatically resize the swap chain back buffers to match the <see cref="Window"/> client area size.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Developers may wish to use a smaller (or larger) back buffer size than is needed for the <see cref="Window"/>. This flag will enable developers to turn off Gorgon's automatic back buffer resizing 
		/// and allow users to do their own resize operations via the <see cref="ResizeBackBuffers"/> method when the window size changes.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// When setting this value to <b>true</b>, there will be a small performance penalty when the application calls the <see cref="Present"/> method because the driver will have to scale the contents of the 
		/// back buffer to fit the client area of the window (if the <see cref="IGorgonSwapChainInfo.StretchBackBuffer"/> property for the <see cref="IGorgonSwapChainInfo"/> passed in to the constructor is set to 
		/// <b>true</b>).
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public bool DoNotAutoResizeBackBuffer
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to exit back into windowed mode when the application loses focus.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This flag controls the behavior of a full screen swap chain when the application loses focus. When set to <b>true</b>, the swap chain will exit full screen mode upon focus loss, and when the application 
		/// regains focus, the swap chain will restore full screen mode.
		/// </para>
		/// <para>
		/// This flag only affects the swap chain when it is in full screen mode. If the swap chain is in windowed mode, no action is taken.
		/// </para>
		/// <para>
		/// The default value for this flag is <b>true</b>.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// When using multiple swap chains in a multi-monitor set up (i.e. a swap chain on each monitor in full screen mode), then this flag should be set to <b>false</b>, otherwise the application will reset one 
		/// of the swap chains back to windowed mode when the window on the other output gains focus. 
		/// </para>
		/// <para>
		/// Users of multi-monitor setups are responsible for handling their own screen state management when focus is lost or gained.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public bool ExitFullScreenModeOnFocusLoss
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the currently active full screen video mode.
		/// </summary>
		/// <remarks>
		/// When <see cref="IsWindowed"/> is <b>false</b>, this value will be <b>null</b>.
		/// </remarks>
		public DXGI.ModeDescription1? FullScreenVideoMode
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the output that is currently being used for full screen mode by this swap chain.
		/// </summary>
		/// <remarks>
		/// When <see cref="IsWindowed"/> is <b>false</b>, this value will be <b>null</b>.
		/// </remarks>
		public IGorgonVideoOutputInfo FullscreenOutput
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the handle of the window or child control that is bound with this swap chain.
		/// </summary>
		public Control Window
		{
			get;
		}

		/// <summary>
		/// Property to return whether the swap chain is in stand by mode.
		/// </summary>
		/// <remarks>
		/// Stand by mode is entered when the <see cref="Present"/> method detects that the window is occluded.
		/// </remarks>
		public bool IsInStandBy
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the settings used to create this swap chain.
		/// </summary>
		public IGorgonSwapChainInfo Info => _info;

		/// <summary>
		/// Property to return whether the swap chain is in windowed mode or not.
		/// </summary>
		/// <remarks>
		/// To enter or exit full screen mode on a swap chain, call the <see cref="EnterFullScreen"/> or <see cref="ExitFullScreen"/> methods.
		/// </remarks>
		/// <seealso cref="EnterFullScreen"/>
		/// <seealso cref="ExitFullScreen"/>
		public bool IsWindowed => FullscreenOutput == null || FullScreenVideoMode == null;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create any resources bound to the swap chain.
		/// </summary>
		private void CreateResources()
		{
			_backBufferTextures = !Info.UseFlipMode ? new GorgonTexture[1] : new GorgonTexture[3];

			_log.Print($"SwapChain '{Name}': Creating {_backBufferTextures.Length} D3D11 textures for back buffers...", LoggingLevel.Verbose);

			for (int i = 0; i < _backBufferTextures.Length; ++i)
			{
				_backBufferTextures[i] = new GorgonTexture(this, i, _log);
			}

			_log.Print($"SwapChain '{Name}': Creating D3D11 render target view...", LoggingLevel.Verbose);
			RenderTargetView = new GorgonRenderTargetView(_backBufferTextures[0], DXGI.Format.Unknown, 0, 0, -1, _log);
		}

		/// <summary>
		/// Function to release resources associated with the swap chain before performing a state change.
		/// </summary>
		private void ReleaseResources()
		{
			_log.Print($"SwapChain '{Name}': Releasing D3D11 render target view.", LoggingLevel.Verbose);
			RenderTargetView?.Dispose();

			_log.Print($"SwapChain '{Name}': Releasing {_backBufferTextures.Length} D3D11 textures.", LoggingLevel.Verbose);
			foreach (GorgonTexture texture in _backBufferTextures)
			{
				texture?.Dispose();
			}
		}

		/// <summary>
		/// Function to initialize the swap chain.
		/// </summary>
		private void Initialize()
		{
			// Ensure that we can use this format for display.
			if ((Graphics.VideoDevice.GetBufferFormatSupport(Info.Format) & D3D11.FormatSupport.Display) != D3D11.FormatSupport.Display)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FORMAT_NOT_SUPPORTED, Info.Format));
			}

			if ((Info.Width < 1) || (Info.Height < 1))
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_SWAP_BACKBUFFER_TOO_SMALL, Info.Width, Info.Height));
			}

			using (DXGI.Factory2 factory = Graphics.VideoDevice.DXGIAdapter().GetParent<DXGI.Factory2>())
			{
				DXGI.SwapChainDescription1 swapDesc = _info.ToSwapChainDesc();

				GISwapChain = new DXGI.SwapChain1(factory, Graphics.VideoDevice.D3DDevice(), Window.Handle, ref swapDesc)
				              {
					              DebugName = Name + " DXGISwapChain"
				              };

				// Due to an issue with winforms and DXGI, we have to manually handle transitions ourselves.
				factory.MakeWindowAssociation(Window.Handle, DXGI.WindowAssociationFlags.IgnoreAll);

				CreateResources();
				
				Window.Resize += Window_Resize;
				
				// We assign these events to the parent form so that a window resize is smooth, currently using the Resize event only introduces massive
				// lag when resizing the back buffers. This will counter that by only resizing when the resize operation ends.
				_parentForm.ResizeBegin += ParentForm_ResizeBegin;
				_parentForm.ResizeEnd += ParentForm_ResizeEnd;

				// Use these events to restore full screen or windowed state when the application regains or loses focus.
				_parentForm.Activated += ParentForm_Activated;
				_parentForm.Deactivate += ParentForm_Deactivated;
			}
		}

		/// <summary>
		/// Handles the Activated event of the ParentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ParentForm_Activated(object sender, EventArgs e)
		{
			if ((!ExitFullScreenModeOnFocusLoss) 
				|| (!IsWindowed)
				|| (_previousOutput == null) 
				|| (_previousMode == null))
			{
				return;
			}

			DXGI.ModeDescription1 mode = _previousMode.Value;

			try
			{
				EnterFullScreen(ref mode, _previousOutput);
			}
			catch (Exception ex)
			{
				_log.LogException(ex);
			}
			finally
			{
				_previousOutput = null;
				_previousMode = null;
			}
		}

		/// <summary>
		/// Handles the Deactivated event of the ParentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ParentForm_Deactivated(object sender, EventArgs e)
		{
			if ((!ExitFullScreenModeOnFocusLoss)
				|| (IsWindowed)
				|| (FullScreenVideoMode == null)
				|| (_previousOutput != null)
				|| (_previousMode != null))
			{
				return;
			}

			_previousMode = FullScreenVideoMode.Value;
			_previousOutput = FullscreenOutput;

			try
			{
				ExitFullScreen();
			}
			catch (Exception ex)
			{
				_log.LogException(ex);
			}
		}

		/// <summary>
		/// Handles the ResizeEnd event of the ParentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ParentForm_ResizeEnd(object sender, EventArgs e)
		{
			try
			{
				// When we're done, tell the system that the resize is complete.
				_resized = true;

				Window_Resize(this, e);
			}
			catch (Exception ex)
			{
				_log.LogException(ex);
			}
			finally
			{
				_resized = false;
			}
		}

		/// <summary>
		/// Handles the ResizeBegin event of the ParentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ParentForm_ResizeBegin(object sender, EventArgs e)
		{
			// Set the flag to indicate that we've started a resize operation.
			_resized = false;
		}

		/// <summary>
		/// Handles the Resize event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Window_Resize(object sender, EventArgs e)
		{
			try
			{
				// The user will handle the resizing, so do nothing.
				if (DoNotAutoResizeBackBuffer)
				{
					return;
				}

				// If we maximize, or restore, then we'll need to force a buffer resize.
				if ((!_resized) && 
					((_parentForm.WindowState == FormWindowState.Maximized) || (_parentForm.WindowState == FormWindowState.Normal)))
				{
					_resized = true;
				}

				// If we're entering/exiting full screen, or not at the end of a resize operation, or the window client size is invalid, or the window is in a minimized state, 
				// or the window size has not changed (can occur when the window is moved) then do nothing.
				if (!_screenStateTransition)
				{
					if ((Window.ClientSize.Width < 1)
					    || (Window.ClientSize.Height < 1)
						|| ((Window.ClientSize.Width == _info.Width) && (Window.ClientSize.Height == _info.Height))
					    || (_parentForm.WindowState == FormWindowState.Minimized)
						|| (!_resized))
					{
						return;
					}
				}
				
				ResizeBackBuffers(Window.ClientSize.Width, Window.ClientSize.Height);
			}
			catch (Exception ex)
			{
				_log.LogException(ex);
			}
			finally
			{
				_resized = _parentForm.WindowState == FormWindowState.Minimized;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (GISwapChain == null)
			{
				return;
			}

			// Unhook any event handlers subscribed to us.
			BeforeSwapChainResized = null;
			AfterSwapChainResized = null;

			if (Window != null)
			{
				Window.Resize -= Window_Resize;
			}

			if (_parentForm != null)
			{
				_parentForm.ResizeBegin -= ParentForm_ResizeBegin;
				_parentForm.ResizeEnd -= ParentForm_ResizeEnd;
				_parentForm.Activated -= ParentForm_Activated;
				_parentForm.Deactivate -= ParentForm_Deactivated;
			}
			
			ReleaseResources();

			if ((GISwapChain != null) && (!IsWindowed))
			{
				// Always go to windowed mode before destroying the swap chain.
				GISwapChain.SetFullscreenState(false, null);
			}

			GISwapChain?.Dispose();

			GISwapChain = null;
		}
		
		/// <summary>
		/// Function to resize the back buffers for the swap chain.
		/// </summary>
		/// <param name="newWidth">The new width of the swap chain back buffers.</param>
		/// <param name="newHeight">The new height of the swap chain back buffers.</param>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="newWidth"/>, or the <paramref name="newHeight"/> parameter is less than 1.</exception>
		/// <remarks>
		/// <para>
		/// This method will only resize the back buffers associated with the swap chain, and not the swap chain <see cref="Window"/> that it is bound with. 
		/// </para>
		/// <para>
		/// Developers who set the <see cref="DoNotAutoResizeBackBuffer"/> to <b>true</b> should use this method to resize the back buffers manually when a <see cref="Window"/> is resized. Otherwise, developers 
		/// should rarely, if ever, have to call this method.
		/// </para>
		/// </remarks>
		public void ResizeBackBuffers(int newWidth, int newHeight)
		{
			if (newWidth < 1)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_SWAP_BACKBUFFER_TOO_SMALL, newWidth, newHeight), nameof(newWidth));
			}

			if (newHeight < 1)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_SWAP_BACKBUFFER_TOO_SMALL, newWidth, newHeight), nameof(newHeight));
			}

			_log.Print($"SwapChain '{Name}': Resizing back buffers.", LoggingLevel.Verbose);
			// Tell the application that this swap chain is going to be resized.
			BeforeSwapChainResized?.Invoke(this, EventArgs.Empty);

			ReleaseResources();

			GISwapChain.ResizeBuffers(IsWindowed ? 2 : 3, newWidth, newHeight, Info.Format, DXGI.SwapChainFlags.AllowModeSwitch);

			_info.Width = newWidth;
			_info.Height = newHeight;

			CreateResources();

			AfterSwapChainResized?.Invoke(this, EventArgs.Empty);

			_log.Print($"SwapChain '{Name}': Back buffers resized.", LoggingLevel.Verbose);
		}

		/// <summary>
		/// Function to flip the buffers to the front buffer.
		/// </summary>
		/// <param name="interval">[Optional] The vertical blank interval.</param>
		/// <remarks>
		/// <para>
		/// If <paramref name="interval"/> parameter is greater than 0, then this method will synchronize to the vertical blank count specified by interval  Passing 0 will display the contents of the 
		/// back buffer as soon as possible.
		/// </para>
		/// <para>
		/// If the window that the swap chain is bound with is occluded and/or the swap chain is in between a mode switch, then this method will place the swap chain into stand by mode, and will recover 
		/// (i.e. turn off stand by) once the device is ready for rendering again.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the interval parameter is less than 0 or greater than 4. This is only thrown when Gorgon is compiled in <b>DEBUG</b> mode.</exception>
		/// <exception cref="GorgonException">Thrown when the method encounters an unrecoverable error.</exception>
		public void Present(int interval = 0)
		{
			DXGI.PresentFlags flags = !IsInStandBy ? DXGI.PresentFlags.None : DXGI.PresentFlags.Test;

			interval.ValidateRange(nameof(interval), 0, 4, true, true);

			try
			{
				IsInStandBy = false;
				GISwapChain.Present(interval, flags);
			}
			catch (DX.SharpDXException sdex)
			{
				if ((sdex.ResultCode == DXGI.ResultCode.DeviceReset)
					|| (sdex.ResultCode == DXGI.ResultCode.DeviceRemoved)
					|| (sdex.ResultCode == DXGI.ResultCode.ModeChangeInProgress)
					|| (sdex.ResultCode.Code == (int)DXGI.DXGIStatus.ModeChangeInProgress)
					|| (sdex.ResultCode.Code == (int)DXGI.DXGIStatus.Occluded))
				{
					IsInStandBy = true;
					return;
				}

				if (sdex.ResultCode == DX.Result.Ok)
				{
					return;
				}

				if (!sdex.ResultCode.Success)
				{
					throw new GorgonException(GorgonResult.DriverError, Resources.GORGFX_ERR_CATASTROPHIC);
				}
			}
		}

		/// <summary>
		/// Function to put the swap chain in full screen mode.
		/// </summary>
		/// <param name="desiredMode">The video mode to use when entering full screen.</param>
		/// <param name="output">The output that will be used for full screen mode.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="output"/> parameter is <b>null</b>.</exception>
		/// <exception cref="GorgonException">Thrown when the <see cref="Window"/> bound to this swap chain is not a Windows <see cref="Form"/>.</exception>
		/// <remarks>
		/// <para>
		/// This will transition the swap chain to full screen mode from windowed mode. If a render target view for this swap chain is bound to the pipeline, then it will be unbound before resetting its state.
		/// </para>
		/// <para>
		/// If the <paramref name="desiredMode"/> parameter does not match a supported video mode for the <paramref name="output"/>, then the closest available video mode will be used and the <paramref name="desiredMode"/> 
		/// parameter will be updated to reflect the video mode that was chosen.
		/// </para>
		/// <para>
		/// If the <paramref name="desiredMode"/> parameter is the same as the <see cref="FullScreenVideoMode"/> property and the <paramref name="output"/> parameter is the same as the 
		/// <see cref="FullscreenOutput"/> property, then this method will do nothing.
		/// </para>
		/// <para>
		/// When the swap chain is bound to a child control (e.g. a panel), then this method will throw an exception if called. Entering full screen is only supported on swap chains bound to a Windows 
		/// <see cref="Form"/>.
		/// </para>
		/// </remarks>
		/// <seealso cref="ExitFullScreen"/>
		public void EnterFullScreen(ref DXGI.ModeDescription1 desiredMode, IGorgonVideoOutputInfo output)
		{
			if (output == null)
			{
				throw new ArgumentNullException(nameof(output));
			}

			if (!(Window is Form))
			{
				throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_ERR_NEED_FORM_FOR_FULLSCREEN, Name));
			}

			if ((FullScreenVideoMode != null) && (FullScreenVideoMode.Value.Equals(desiredMode)) && (output == FullscreenOutput))
			{
				return;
			}

			DXGI.Output dxgiOutput = null;
			DXGI.Output1 dxgiOutput1 = null;

			try
			{
				_log.Print($"SwapChain '{Name}': Entering full screen mode.  Requested mode {desiredMode} on output {output.Name}.", LoggingLevel.Verbose);

				dxgiOutput = Graphics.VideoDevice.DXGIAdapter().GetOutput(output.Index);
				dxgiOutput1 = dxgiOutput.QueryInterface<DXGI.Output1>();

				DXGI.ModeDescription1 actualMode;

				// Try to find something resembling the video mode we asked for.
				dxgiOutput1.FindClosestMatchingMode1(ref desiredMode, out actualMode, Graphics.VideoDevice.D3DDevice());

				DXGI.ModeDescription resizeMode = actualMode.ToModeDesc();

				// Switch to the format we want so that ResizeBackBuffers will work correctly.
				_info.Format = desiredMode.Format;

				// Bring the control up before attempting to switch to full screen.
				// Otherwise things get real weird, real fast.
				if (!Window.Visible)
				{
					Window.Show();
				}

				// Before every call to ResizeTarget, we must indicate that we want to handle the resize event on the control.
				// Failure to do so will bring up warnings in the debug log output about presentation inefficiencies.
				_screenStateTransition = true;
				GISwapChain.ResizeTarget(ref resizeMode);

				DXGI.Rational refreshRate = resizeMode.RefreshRate;
				GISwapChain.SetFullscreenState(true, dxgiOutput1);

				// The MSDN documentation says to call resize targets again with a zeroed refresh rate after setting the mode: 
				// https://msdn.microsoft.com/en-us/library/windows/desktop/ee417025(v=vs.85).aspx.
				resizeMode = new DXGI.ModeDescription(resizeMode.Width, resizeMode.Height, new DXGI.Rational(0, 0), resizeMode.Format);
				GISwapChain.ResizeTarget(ref resizeMode);

				// Ensure that we have an up-to-date copy of the video mode information.
				resizeMode.RefreshRate = refreshRate;
				FullScreenVideoMode = desiredMode = resizeMode.ToModeDesc1();
				FullscreenOutput = output;
				_info.Width = desiredMode.Width;
				_info.Height = desiredMode.Height;
				_info.Format = desiredMode.Format;

				_log.Print($"SwapChain '{Name}': Full screen mode was set.  Final mode: {FullScreenVideoMode}.  Swap chain back buffer size: {_info.Width}x{_info.Height}, Format: {_info.Format}",
				           LoggingLevel.Verbose);
			}
			catch (DX.SharpDXException sdEx)
			{
				switch (sdEx.ResultCode.Code)
				{
					case (int)DXGI.DXGIStatus.ModeChangeInProgress:
						_log.Print($"SwapChain '{Name}': Could not switch to full screen mode because the device was busy switching to full screen on another output.",
								   LoggingLevel.All);
						break;
					default:
						if (sdEx.ResultCode != DXGI.ResultCode.NotCurrentlyAvailable)
						{
							throw;
						}

						GorgonApplication.Log.Print($"SwapChain '{Name}': Could not switch to full screen mode because the device is not currently available.",
													LoggingLevel.All);
						break;
				}
			}
			finally
			{
				dxgiOutput1?.Dispose();
				dxgiOutput?.Dispose();
				_screenStateTransition = false;
			}
		}

		/// <summary>
		/// Function to put the swap chain into windowed mode.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will restore the swap chain to windowed mode from full screen mode. If a render target view for this swap chain is bound to the pipeline, then it will be unbound before resetting its state.
		/// </para>
		/// <para>
		/// When the swap chain is already in windowed mode, then this method will do nothing.
		/// </para>
		/// </remarks>
		/// <seealso cref="EnterFullScreen"/>
		public void ExitFullScreen()
		{
			if ((IsWindowed)
				|| (FullScreenVideoMode == null)
				|| (FullscreenOutput == null))
			{
				return;
			}

			try
			{
				_log.Print($"SwapChain '{Name}': Restoring windowed mode.", LoggingLevel.Verbose);

				_screenStateTransition = true;
				DXGI.ModeDescription1 desc1 = FullScreenVideoMode.Value;
				DXGI.ModeDescription desc = desc1.ToModeDesc();

				GISwapChain.SetFullscreenState(false, null);

				// Resize to match the video mode.
				GISwapChain.ResizeTarget(ref desc);

				_info.Width = desc.Width;
				_info.Height = desc.Height;
				_info.Format = desc.Format;

				_log.Print($"SwapChain '{Name}': Windowed mode restored. Back buffer size: {_info.Width}x{_info.Height}, Format: {_info.Format}.", LoggingLevel.Verbose);
			}
			catch (Exception ex)
			{
				_log.LogException(ex);
				throw;
			}
			finally
			{
				FullScreenVideoMode = null;
				FullscreenOutput = null;
				_screenStateTransition = false;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSwapChain"/> class.
		/// </summary>
		/// <param name="name">The name of the swap chain.</param>
		/// <param name="graphics">The graphics interface used to create this swap chain.</param>
		/// <param name="window">The window that should be bound with the swap chain.</param>
		/// <param name="info">Settings for the swap chain.</param>
		/// <param name="log">[Optional] The log used for debug output.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="graphics"/>, <paramref name="window"/>, or the <paramref name="info"/> parameters are <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
		/// <exception cref="GorgonException">Thrown when the <see cref="IGorgonSwapChainInfo.Width"/>, or <see cref="IGorgonSwapChainInfo.Height"/> values of the <see cref="IGorgonSwapChainInfo"/> passed to the <paramref name="info"/> parameter 
		/// are less than 1.
		/// <para>-or-</para>
		/// <para>
		/// Thrown when the <see cref="IGorgonSwapChainInfo.Format"/> value of the <see cref="IGorgonSwapChainInfo"/> passed to the <paramref name="info"/> parameter is not a supported display format.
		/// </para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// Swap chains are used to send graphics data to the display <paramref name="window"/>. It does this by setting up a series of off screen buffers and rendering to each buffer and presenting that buffer 
		/// to the display window (<see cref="Present"/>). 
		/// </para>
		/// <para>
		/// Because the buffers are independent of the <paramref name="window"/>, they may be larger or smaller than the target <paramref name="window"/>. The <see cref="IGorgonSwapChainInfo.Width"/> and 
		/// <see cref="IGorgonSwapChainInfo.Height"/> properties of the <paramref name="info"/> parameter are used to define the size of these buffers. This buffer size is automatically set when the 
		/// <paramref name="window"/> is resized by the user unless the <see cref="DoNotAutoResizeBackBuffer"/> property is set to <b>true</b> and the <see cref="IGorgonSwapChainInfo.StretchBackBuffer"/> property 
		/// for the <see cref="IGorgonSwapChainInfo"/> passed to the <paramref name="info"/> parameter is set to <b>true</b> (the default).
		/// </para>
		/// <para>
		/// When choosing a buffer format in the <see cref="IGorgonSwapChainInfo.Format"/> passed by the <paramref name="info"/> property, it is important to choose a format that can be used as a display format. 
		/// Failure to do so will result in an exception. Users may determine if a format is supported for display by using the <see cref="IGorgonVideoDevice.GetBufferFormatSupport"/> method on the the 
		/// <see cref="GorgonGraphics.VideoDevice"/> property of the <see cref="GorgonGraphics"/> instance passed to this method.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// If the buffer sizes are inconsistent with the window size, there will be a small performance penalty as the driver resizes the the contents of the buffer when copying to the display. For best 
		/// performance, please use buffers that have the same size as the client area of the <paramref name="window"/>.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		/// <seealso cref="Present"/>
		/// <seealso cref="DoNotAutoResizeBackBuffer"/>
		/// <seealso cref="IGorgonSwapChainInfo"/>
		public GorgonSwapChain(string name, GorgonGraphics graphics, Control window, IGorgonSwapChainInfo info, IGorgonLog log = null)
			: base(name)
		{
			if (info == null)
			{
				throw new ArgumentNullException(Resources.GORGFX_ERR_PARAMETER_MUST_NOT_BE_NULL, nameof(info));
			}

			_log = log ?? GorgonLogDummy.DefaultInstance;

			_log.Print($"SwapChain '{name}': Creating D3D11 swap chain...", LoggingLevel.Simple);

			ExitFullScreenModeOnFocusLoss = true;
			Window = window ?? throw new ArgumentNullException(Resources.GORGFX_ERR_PARAMETER_MUST_NOT_BE_NULL, nameof(window));
			_parentForm = window as Form ?? window.FindForm();
			Graphics = graphics ?? throw new ArgumentNullException(Resources.GORGFX_ERR_PARAMETER_MUST_NOT_BE_NULL, nameof(graphics));
			
			// Clone the info so that changes to the source won't be reflected back here and cause us grief.
			_info = new GorgonSwapChainInfo(info);
			Initialize();
		}
		#endregion
	}
}