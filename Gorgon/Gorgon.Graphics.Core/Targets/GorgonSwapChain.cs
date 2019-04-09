#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: April 7, 2018 11:45:40 PM
// 
#endregion

using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using Gorgon.Native;
using SharpDX.DXGI;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Graphics.Imaging;

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
    /// If the swap chain is currently assigned to the <see cref="GorgonGraphics.RenderTargets"/> property, and it is resized, it will do its best to ensure it stays bound to the active render target list 
    /// (this also includes the current <see cref="GorgonGraphics.DepthStencilView"/>. This only applies to the default <see cref="RenderTargetView"/> associated with the swap chain. If a user has created a
    /// custom <see cref="GorgonRenderTarget2DView"/> object for the swap chain, and assigned that view to the <see cref="GorgonGraphics.RenderTargets"/> list, then it is their responsibility to ensure that the
    /// view is rebuilt and reassigned. Users may intercept a swap chain back buffer resize by hooking the <see cref="BeforeSwapChainResized"/> and the <see cref="AfterSwapChainResized"/> events.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphics"/>
    /// <seealso cref="GorgonRenderTarget2DView"/>
    public sealed class GorgonSwapChain
        : IGorgonGraphicsObject, IGorgonSwapChainInfo, IDisposable
    {
        #region Classes.
        /// <summary>
        /// Captures state for the window prior to entering borderless full screen mode and is used to restore it upon exit.
        /// </summary>
        private class WindowState
        {
            /// <summary>
            /// Property to set or return the previous border style for the form.
            /// </summary>
            public FormBorderStyle BorderStyle
            {
                get;
                set;
            }

            /// <summary>
            /// Property to set or return the previous form window state.
            /// </summary>
            public FormWindowState FormWindowState
            {
                get;
                set;
            }

            /// <summary>
            /// Property to set or return the previous client size.
            /// </summary>
            public Size ClientSize
            {
                get;
                set;
            }
        }

        /// <summary>
        /// State information used when resizing back buffers and performing screen state transitions.
        /// </summary>
        private class ResizeState
        {
            #region Variables.
            // Previous video mode.
            private GorgonVideoMode? _prevVideoMode;
            #endregion

            #region Properties.
            /// <summary>
            /// Property to set or return that the window was resized.
            /// </summary>
            public bool Resized
            {
                get;
                set;
            }

            /// <summary>
            /// Property to set or return that we're performing a full screen state transition.
            /// </summary>
            public bool IsScreenStateTransition
            {
                get;
                set;
            }

            /// <summary>
            /// Property to set or return the previous adapter output when transitioning back to windowed mode.
            /// </summary>
            public IGorgonVideoOutputInfo PreviousOutput
            {
                get;
                set;
            }

            /// <summary>
            /// The previous video mode when transitioning back to windowed mode.
            /// </summary>
            public ref GorgonVideoMode? PreviousVideoMode => ref _prevVideoMode;
            #endregion;
        }
        #endregion

        #region Variables.
        // The DXGI swap chain that this object will wrap.
        private SwapChain4 _swapChain;
        // The textures used by the back buffer.
        private readonly GorgonTexture2D[] _backBufferTextures;
        // The information used to create the swap chain.
        private readonly GorgonSwapChainInfo _info;
        // Information used when resizing the back buffers or performing a state transition.
        private readonly ResizeState _resizeState = new ResizeState();
        // The current full screen video mode.
        private GorgonVideoMode? _fullScreenVideoMode;
        // Flag to indicate that the application is in full screen borderless mode.
        private bool _isFullScreenBorderless;
        // The state to reset when exiting full screen borderless mode.
        private readonly WindowState _fullScreenBordlessState = new WindowState();
        // The render target view.
        private GorgonRenderTarget2DView _targetView;
        // The previously assigned render target views captured when using flip mode.
        private readonly GorgonRenderTargetView[] _previousViews = new GorgonRenderTargetView[D3D11.OutputMergerStage.SimultaneousRenderTargetCount];
        // Flag to indicate that tearing support is enabled (for flip mode).
        private readonly int _supportsTearing = 1;
        #endregion

        #region Events.
        /// <summary>
        /// Event called before the swap chain has been resized.
        /// </summary>
        public event EventHandler<BeforeSwapChainResizedEventArgs> BeforeSwapChainResized;
        /// <summary>
        /// Event called after the swap chain has been resized.
        /// </summary>
        public event EventHandler<AfterSwapChainResizedEventArgs> AfterSwapChainResized;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the internal DXGI swap chain.
        /// </summary>
        internal SwapChain4 DXGISwapChain => _swapChain;

        /// <summary>
        /// Property to return the back buffer texture to use as a render target.
        /// </summary>
        internal GorgonTexture2D InternalBackBufferTexture => ((_backBufferTextures?.Length ?? 0) > 0) ? _backBufferTextures?[0] : null;

        /// <summary>
        /// Property to return the default render target view for this swap chain.
        /// </summary>
        public GorgonRenderTarget2DView RenderTargetView => _targetView;

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
        /// Property to return the graphics interface that built this object.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get;
        }

        /// <summary>
        /// Property to return the window associated with the swap chain.
        /// </summary>
        public Control Window
        {
            get;
        }

        /// <summary>
        /// Property to return the parent form of the <see cref="Window"/>.
        /// </summary>
        /// <remarks>
        /// If the <see cref="Window"/> is a Form, then this property will return the same value as <see cref="Window"/>.
        /// </remarks>
        public Form ParentForm
        {
            get;
        }

        /// <summary>
        /// Property to return the name of this object.
        /// </summary>
        public string Name => _info.Name;

        /// <summary>
        /// Property to return whether the back buffer contents will be stretched to fit the size of the presentation target area (typically the client area of the window).
        /// </summary>
        public bool StretchBackBuffer => _info.StretchBackBuffer;

        /// <summary>
        /// Property to return whether to use flip mode rather than a bitblt mode to present the back buffer to the presentation target.
        /// </summary>
        public bool UseFlipMode => _info.UseFlipMode;

        /// <summary>
        /// Property to return the format of the swap chain back buffer.
        /// </summary>
        public BufferFormat Format => _info.Format;

        /// <summary>
        /// Property to return the width of the swap chain back buffer.
        /// </summary>
        public int Width => _info.Width;

        /// <summary>
        /// Property to return the height of the swap chain back buffer.
        /// </summary>
        public int Height => _info.Height;

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
        } = true;

        /// <summary>
        /// Property to return the currently active full screen video mode.
        /// </summary>
        /// <remarks>
        /// When <see cref="IsWindowed"/> is <b>false</b>, this value will be <b>null</b>.
        /// </remarks>
        public ref readonly GorgonVideoMode? FullScreenVideoMode => ref _fullScreenVideoMode;

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
        /// Property to return whether the swap chain is in windowed mode or not.
        /// </summary>
        /// <remarks>
        /// To enter or exit full screen mode on a swap chain, call the <see cref="O:Gorgon.Graphics.Core.GorgonSwapChain.EnterFullScreen"/> or <see cref="ExitFullScreen"/> methods.
        /// </remarks>
        /// <seealso cref="O:Gorgon.Graphics.Core.GorgonSwapChain.EnterFullScreen"/>
        /// <seealso cref="ExitFullScreen"/>
        public bool IsWindowed => FullscreenOutput == null || _fullScreenVideoMode == null;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to locate the form for the owning control.
        /// </summary>
        /// <param name="control">The control owned by the form.</param>
        /// <returns>The form that owns the control.</returns>
        private Form FindForm(Control control)
        {
            if (control is Form result)
            {
                return result;
            }

            result = control.FindForm();

            if (result == null)
            {
                result = control.TopLevelControl as Form;
            }

            if (result == null)
            {
                result = Form.ActiveForm;

                if ((result != null) && (result.DisplayRectangle.IntersectsWith(control.RectangleToScreen(control.ClientRectangle))))
                {
                    return result;
                }
            }

            if ((result == null) && (Application.OpenForms.Count > 0))
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (!form.DisplayRectangle.IntersectsWith(control.RectangleToScreen(control.ClientRectangle)))
                    {
                        continue;
                    }

                    result = form;
                    break;
                }
            }

            return result;
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
				|| (_resizeState.PreviousOutput == null) 
				|| (_resizeState.PreviousVideoMode == null))
			{
				return;
			}

			GorgonVideoMode mode = _resizeState.PreviousVideoMode.Value;

			try
			{
				EnterFullScreen(in mode, _resizeState.PreviousOutput);
			}
			catch (Exception ex)
			{
				Graphics.Log.LogException(ex);
			}
			finally
			{
                _resizeState.PreviousOutput = null;
                _resizeState.PreviousVideoMode = null;
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
				|| (_fullScreenVideoMode == null)
			    || (_resizeState.PreviousOutput == null) 
			    || (_resizeState.PreviousVideoMode == null))
			{
				return;
			}

		    _resizeState.PreviousVideoMode = _fullScreenVideoMode;
		    _resizeState.PreviousOutput = FullscreenOutput;

			try
			{
				ExitFullScreen();
			}
			catch (Exception ex)
			{
				Graphics.Log.LogException(ex);
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
                _resizeState.Resized = true;

				Window_Resize(this, e);
			}
			catch (Exception ex)
			{
				Graphics.Log.LogException(ex);
			}
			finally
			{
				_resizeState.Resized = false;
			}
		}

        /// <summary>
        /// Handles the ResizeBegin event of the ParentForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ParentForm_ResizeBegin(object sender, EventArgs e) =>
            // Set the flag to indicate that we've started a resize operation.
            _resizeState.Resized = false;

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
				if ((!_resizeState.Resized) && 
					((ParentForm.WindowState == FormWindowState.Maximized) || (ParentForm.WindowState == FormWindowState.Normal)))
				{
					_resizeState.Resized = true;
				}

				// If we're entering/exiting full screen, or not at the end of a resize operation, or the window client size is invalid, or the window is in a minimized state, 
				// or the window size has not changed (can occur when the window is moved) then do nothing.
				if (!_resizeState.IsScreenStateTransition)
				{
					if ((Window.ClientSize.Width < 1)
					    || (Window.ClientSize.Height < 1)
						|| ((Window.ClientSize.Width == _info.Width) && (Window.ClientSize.Height == _info.Height))
					    || (ParentForm.WindowState == FormWindowState.Minimized)
						|| (!_resizeState.Resized))
					{
						return;
					}
				}
				
				ResizeBackBuffers(Window.ClientSize.Width, Window.ClientSize.Height);
			}
			catch (Exception ex)
			{
				Graphics.Log.LogException(ex);
			    throw;
			}
			finally
			{
				_resizeState.Resized = ParentForm.WindowState == FormWindowState.Minimized;
			}
		}

        /// <summary>
        /// Function to destroy the resources for the swap chain.
        /// </summary>
        /// <param name="disposing"><b>true</b> if the object is being disposed, <b>false</b> if not.</param>
        /// <returns>The previous index of this swap chain's render target view.</returns>
        private int DestroyResources(bool disposing)
        {
            Graphics.Log.Print($"Destroying {Name} swap chain DXGI/D3D11 resources.", LoggingLevel.Simple);
            int renderTargetViewIndex = Graphics.RenderTargets.IndexOf(RenderTargetView);

            // Unbind this render target before removing the resources.
            if ((renderTargetViewIndex != -1) && (!disposing))
            {
                GorgonRenderTargetView[] currentViews = Graphics.RenderTargets.Select(item => item == RenderTargetView ? null : item).ToArray();
                Graphics.SetRenderTargets(currentViews, Graphics.DepthStencilView);
            }

            GorgonRenderTarget2DView rtv = Interlocked.Exchange(ref _targetView, null);
            rtv?.Dispose();

            if (_backBufferTextures == null)
            {
                return renderTargetViewIndex;
            }

            for (int i = 0; i < _backBufferTextures.Length; ++i)
            {
                GorgonTexture2D backBufferTexture = Interlocked.Exchange(ref _backBufferTextures[i], null);
                backBufferTexture?.Dispose();
            }

            return renderTargetViewIndex;
        }

        /// <summary>
        /// Function called after a back buffer resize to refresh the resources
        /// </summary>
        /// <param name="targetIndex">The previous index of the swap chain's render target view.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="targetIndex"/> parameter indicates the index this swap chain's render target view was assigned to in the output stage prior to a resize event. If this swap chain was not 
        /// assigned to the output stage, then this value will be -1.  
        /// </para>
        /// </remarks>
        private void CreateResources(int targetIndex)
        {
            for (int i = 0; i < _backBufferTextures.Length; ++i)
            {
                _backBufferTextures[i] = new GorgonTexture2D(this, i);
            }

            Graphics.Log.Print($"SwapChain '{Name}': Created {_backBufferTextures.Length} D3D11 textures for back buffers.", LoggingLevel.Verbose);

            _targetView = new GorgonRenderTarget2DView(_backBufferTextures[0],
                                                       _backBufferTextures[0].Format,
                                                       new GorgonFormatInfo(_backBufferTextures[0].Format),
                                                       0,
                                                       0,
                                                       _backBufferTextures[0].ArrayCount);
            _targetView.CreateNativeView();

            Graphics.Log.Print($"SwapChain '{Name}': Created swap chain render target view.", LoggingLevel.Verbose);

            if (targetIndex == -1)
            {
                return;
            }

            // Restore the render target if we resize.
            GorgonDepthStencil2DView dsv = null;
            if (Graphics.DepthStencilView != null)
            {
                dsv = ((_info.Width == Graphics.DepthStencilView.Width) 
                       && (_info.Height == Graphics.DepthStencilView.Height) 
                       && (Graphics.DepthStencilView.ArrayCount == 1) 
                       && (Graphics.DepthStencilView.ArrayIndex == 0)
                       && (Graphics.DepthStencilView.MultisampleInfo == GorgonMultisampleInfo.NoMultiSampling))
                          ? Graphics.DepthStencilView
                          : null;

                if (dsv == null)
                {
                    // Log a warning here because we didn't unbind our depth/stencil.
                    Graphics.Log.Print($"Warning: Depth/Stencil view for resource '{Graphics.DepthStencilView.Texture.Name}' ({Graphics.DepthStencilView.Width}x{Graphics.DepthStencilView.Height}) does not match the size of the swap chain ({_info.Width}x{_info.Height}). Therefore, the depth/stencil view will be unbound from the pipeline.",
                                       LoggingLevel.Verbose);
                }
            }

            GorgonRenderTargetView[] rtvs = Graphics.RenderTargets.ToArray();
            rtvs[targetIndex] = _targetView;
            Graphics.SetRenderTargets(rtvs, dsv);
        }

        /// <summary>
        /// Function to retrieve which targets are set when in flip mode.
        /// </summary>
        /// <returns>A tuple containing the first target index, and the total number of targets.</returns>
        private (int FirstIndex, int TargetCount) GetCurrentTargets()
        {
            // Record our current render targets so we can restore them -after- the present flip.
            bool thisTargetIsSet = false;
            int firstTarget = -1;
            int targetCount = 0;

            for (int i = 0; i < Graphics.RenderTargets.Count; ++i)
            {
                GorgonRenderTargetView view = Graphics.RenderTargets[i];
                _previousViews[i] = view;

                // Skip null entries, we don't care about these.
                if (view == null)
                {
                    continue;
                }

                if (view == RenderTargetView)
                {
                    thisTargetIsSet = true;
                }

                // Make note if which render target index was first used so we can be a little more efficent below.
                if (firstTarget == -1)
                {
                    firstTarget = i;
                }
                ++targetCount;
            }
            
            // Unbind the render targets.
            return thisTargetIsSet ? (firstTarget == -1 ? 0 : firstTarget, targetCount) : (0, 0);
        }

        /// <summary>
        /// Function to ensure that the settings passed to the swap chain are valid.
        /// </summary>
        /// <param name="info">Information about the swap chain.</param>
        private void ValidateSwapChainInfo(GorgonSwapChainInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (!Graphics.FormatSupport[info.Format].IsDisplayFormat)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FORMAT_NOT_SUPPORTED, info.Format));
            }

            // Constrain sizes.
            info.Width = info.Width.Min(Graphics.VideoAdapter.MaxTextureWidth).Max(1);
            info.Height = info.Height.Min(Graphics.VideoAdapter.MaxTextureHeight).Max(1);
        }

        /// <summary>
        /// Function to enter full screen mode on the swap chain.
        /// </summary>
        /// <exception cref="GorgonException">Thrown if the <see cref="Window"/> is not the same as the <see cref="ParentForm"/>.</exception>
        /// <remarks>
        /// <para>
        /// This method will set the application up for full screen borderless windowed mode. This allows us to have a full screen experience, but without exclusively taking over the device (which can lead 
        /// to issues in some cases - notably when Alt+Tabbing). To exit full screen mode, call <see cref="ExitFullScreen"/>.
        /// </para>
        /// <para>
        /// When this method enters full screen, the <see cref="ParentForm"/> is resized to fit the entire screen, and the <see cref="ParentForm"/> is brought forth to cover the task bar. Any border 
        /// decoration on the form is also removed.
        /// </para>
        /// <para>
        /// If full screen exclusive mode is required, users can call the <see cref="M:EnterFullScreen(in GorgonVideoMode, IGorgonVideoOutputInfo)"/> overload instead.
        /// </para>
        /// </remarks>
        /// <seealso cref="M:EnterFullScreen(GorgonVideoMode, IGorgonVideoOutputInfo)"/>
        public void EnterFullScreen()
        {
            if (ParentForm != Window)
            {
                throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_ERR_NEED_FORM_FOR_FULLSCREEN, Name));
            }

            if ((_isFullScreenBorderless) || (Graphics.VideoAdapter.Outputs.Count == 0))
            {
                return;
            }

			try
			{
			    IntPtr monitor = Win32API.MonitorFromWindow(ParentForm.Handle, MonitorFlags.MONITOR_DEFAULTTONEAREST);
			    IGorgonVideoOutputInfo output = null;

			    if (monitor != IntPtr.Zero)
			    {
			        output = Graphics.VideoAdapter.Outputs.FirstOrDefault(item => item.MonitorHandle == monitor);
			    }

			    if (output == null)
			    {
			        monitor = Win32API.MonitorFromWindow(ParentForm.Handle, MonitorFlags.MONITOR_DEFAULTTOPRIMARY);
			        output = Graphics.VideoAdapter.Outputs.FirstOrDefault(item => item.MonitorHandle == monitor);

                    Debug.Assert(output != null, "Cannot find a suitable output for full screen borderless window mode.");
			    }

			    var videoMode = new GorgonVideoMode(output.DesktopBounds.Width, output.DesktopBounds.Height, BufferFormat.R8G8B8A8_UNorm);
			    _fullScreenBordlessState.BorderStyle = ParentForm.FormBorderStyle;
			    _fullScreenBordlessState.FormWindowState = ParentForm.WindowState;
			    _fullScreenBordlessState.ClientSize = ParentForm.ClientSize;
                
				Graphics.Log.Print($"SwapChain '{Name}': Entering full screen borderless windowed mode.  Requested mode {videoMode} on output {output.Name}.", LoggingLevel.Verbose);

			    // Bring the control up before attempting to switch to full screen.
			    // Otherwise things get real weird, real fast.
			    if (!ParentForm.Visible)
			    {
			        ParentForm.Show();
			    }

			    // Switch to the format we want so that ResizeBackBuffers will work correctly.
			    _info.Format = videoMode.Format;
			    ParentForm.WindowState = FormWindowState.Normal;
			    ParentForm.FormBorderStyle = FormBorderStyle.None;
			    ParentForm.WindowState = FormWindowState.Maximized;
                ParentForm.Activate();

				// Before every call to ResizeTarget, we must indicate that we want to handle the resize event on the control.
				// Failure to do so will bring up warnings in the debug log output about presentation inefficiencies.
                _resizeState.IsScreenStateTransition = true;
			    ModeDescription modeDesc = videoMode.ToModeDesc();
				DXGISwapChain.ResizeTarget(ref modeDesc);

				modeDesc = new ModeDescription(modeDesc.Width, modeDesc.Height, new Rational(0, 0), modeDesc.Format);
				DXGISwapChain.ResizeTarget(ref modeDesc);

				// Ensure that we have an up-to-date copy of the video mode information.
				_fullScreenVideoMode = modeDesc.ToGorgonVideoMode();
				FullscreenOutput = output;
				_info.Width = _fullScreenVideoMode.Value.Width;
				_info.Height = _fullScreenVideoMode.Value.Height;
				_info.Format = _fullScreenVideoMode.Value.Format;
			    _isFullScreenBorderless = true;

				Graphics.Log.Print($"SwapChain '{Name}': Full screen borderless windowed mode was set.  Final mode: {FullScreenVideoMode}.  Swap chain back buffer size: {_info.Width}x{_info.Height}, Format: {_info.Format}",
				           LoggingLevel.Verbose);
			}
			catch (DX.SharpDXException sdEx)
			{
				switch (sdEx.ResultCode.Code)
				{
					case (int)DXGIStatus.ModeChangeInProgress:
						Graphics.Log.Print($"SwapChain '{Name}': Could not switch to full screen borderless windowed mode because the device was busy switching to full screen on another output.",
								   LoggingLevel.All);
						break;
					default:
						if (sdEx.ResultCode != ResultCode.NotCurrentlyAvailable)
						{
							throw;
						}

					    Graphics.Log.Print($"SwapChain '{Name}': Could not switch to full screen boderless windowed rmode because the device is not currently available.",
					               LoggingLevel.All);
						break;
				}
			}
			finally
			{
                _resizeState.IsScreenStateTransition = false;
			}
        }

		/// <summary>
		/// Function to put the swap chain in full screen mode.
		/// </summary>
		/// <param name="desiredMode">The video mode to use when entering full screen.</param>
		/// <param name="output">The output that will be used for full screen mode.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="output"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="output"/> is from a different video adapter than the one specified by <see cref="GorgonGraphics.VideoAdapter"/> on the <see cref="GorgonGraphics"/> interface.</exception>
		/// <exception cref="GorgonException">Thrown when the <see cref="Window"/> bound to this swap chain is not the <see cref="ParentForm"/>.</exception>
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
		public void EnterFullScreen(in GorgonVideoMode desiredMode, IGorgonVideoOutputInfo output)
		{
			if (output == null)
			{
				throw new ArgumentNullException(nameof(output));
			}

		    if (output.Adapter != Graphics.VideoAdapter)
		    {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_OUTPUT_ADAPTER_MISMATCH, output.Adapter.Name, Graphics.VideoAdapter.Name), nameof(output));
		    }

			if (ParentForm != Window)
			{
				throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_ERR_NEED_FORM_FOR_FULLSCREEN, Name));
			}

			if (((_fullScreenVideoMode != null) && (_fullScreenVideoMode.Value.Equals(in desiredMode)) && (output == FullscreenOutput))
                || (_isFullScreenBorderless))
			{
				return;
			}

			Output dxgiOutput = null;
			Output1 dxgiOutput6 = null;

			try
			{
				Graphics.Log.Print($"SwapChain '{Name}': Entering full screen mode.  Requested mode {desiredMode} on output {output.Name}.", LoggingLevel.Verbose);

				dxgiOutput = Graphics.DXGIAdapter.GetOutput(output.Index);
				dxgiOutput6 = dxgiOutput.QueryInterface<Output6>();

				// Try to find something resembling the video mode we asked for.
			    ModeDescription1 desiredDxGiMode = desiredMode.ToModeDesc1();
				dxgiOutput6.FindClosestMatchingMode1(ref desiredDxGiMode, out ModeDescription1 actualMode, Graphics.D3DDevice);

				ModeDescription resizeMode = actualMode.ToModeDesc();

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
                _resizeState.IsScreenStateTransition = true;
				DXGISwapChain.ResizeTarget(ref resizeMode);

				Rational refreshRate = resizeMode.RefreshRate;
				DXGISwapChain.SetFullscreenState(true, dxgiOutput6);

				// The MSDN documentation says to call resize targets again with a zeroed refresh rate after setting the mode: 
				// https://msdn.microsoft.com/en-us/library/windows/desktop/ee417025(v=vs.85).aspx.
				resizeMode = new ModeDescription(resizeMode.Width, resizeMode.Height, new Rational(0, 0), resizeMode.Format);
				DXGISwapChain.ResizeTarget(ref resizeMode);

				// Ensure that we have an up-to-date copy of the video mode information.
				resizeMode.RefreshRate = refreshRate;
				_fullScreenVideoMode = resizeMode.ToGorgonVideoMode();
				FullscreenOutput = output;
				_info.Width = _fullScreenVideoMode.Value.Width;
				_info.Height = _fullScreenVideoMode.Value.Height;
				_info.Format = _fullScreenVideoMode.Value.Format;

				Graphics.Log.Print($"SwapChain '{Name}': Full screen mode was set.  Final mode: {FullScreenVideoMode}.  Swap chain back buffer size: {_info.Width}x{_info.Height}, Format: {_info.Format}",
				           LoggingLevel.Verbose);
			}
			catch (DX.SharpDXException sdEx)
			{
				switch (sdEx.ResultCode.Code)
				{
					case (int)DXGIStatus.ModeChangeInProgress:
						Graphics.Log.Print($"SwapChain '{Name}': Could not switch to full screen mode because the device was busy switching to full screen on another output.",
								   LoggingLevel.All);
						break;
					default:
						if (sdEx.ResultCode != ResultCode.NotCurrentlyAvailable)
						{
							throw;
						}

					    Graphics.Log.Print($"SwapChain '{Name}': Could not switch to full screen mode because the device is not currently available.",
					               LoggingLevel.All);
						break;
				}
			}
			finally
			{
                dxgiOutput6?.Dispose();
				dxgiOutput?.Dispose();
                _resizeState.IsScreenStateTransition = false;
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
		/// <seealso cref="EnterFullScreen()"/>
		/// <seealso cref="M:EnterFullScreen(in GorgonVideoMode, IGorgonVideoOutputInfo)"/>
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
                Graphics.Log.Print($"SwapChain '{Name}': Restoring windowed mode.", LoggingLevel.Verbose);
                
                _resizeState.IsScreenStateTransition = true;

                ModeDescription desc = FullScreenVideoMode.Value.ToModeDesc();

				if (_isFullScreenBorderless)
			    {
			        desc.Width = _fullScreenBordlessState.ClientSize.Width;
			        desc.Height = _fullScreenBordlessState.ClientSize.Height;

			        ParentForm.WindowState = _fullScreenBordlessState.FormWindowState;
			        ParentForm.ClientSize = _fullScreenBordlessState.ClientSize;
			        ParentForm.FormBorderStyle = _fullScreenBordlessState.BorderStyle;
			        _isFullScreenBorderless = false;
			    }
				else
				{
				    DXGISwapChain.SetFullscreenState(false, null);
				}

				// Resize to match the video mode.
				DXGISwapChain.ResizeTarget(ref desc);

				_info.Width = desc.Width;
				_info.Height = desc.Height;
				_info.Format = (BufferFormat)desc.Format;

				Graphics.Log.Print($"SwapChain '{Name}': Windowed mode restored. Back buffer size: {_info.Width}x{_info.Height}, Format: {_info.Format}.", LoggingLevel.Verbose);
			}
			catch (Exception ex)
			{
				Graphics.Log.LogException(ex);
				throw;
			}
			finally
			{
				_fullScreenVideoMode = null;
				FullscreenOutput = null;
                _resizeState.IsScreenStateTransition = false;
			}
		}

		/// <summary>
		/// Function to resize the back buffers for the swap chain.
		/// </summary>
		/// <param name="newWidth">The new width of the swap chain back buffers.</param>
		/// <param name="newHeight">The new height of the swap chain back buffers.</param>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="newWidth"/>, or the <paramref name="newHeight"/> parameter is less than 1.</exception>
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

			Graphics.Log.Print($"SwapChain '{Name}': Resizing back buffers.", LoggingLevel.Verbose);

			// Tell the application that this swap chain is going to be resized.
			BeforeSwapChainResized?.Invoke(this, new BeforeSwapChainResizedEventArgs(new DX.Size2(_info.Width, _info.Height), new DX.Size2(newWidth, newHeight)));

			int rtvIndex = DestroyResources(false);

            SwapChainFlags flags = SwapChainFlags.AllowModeSwitch;

            if ((_supportsTearing != 0) && (UseFlipMode))
            {
                flags |= SwapChainFlags.AllowTearing;
            }

			DXGISwapChain.ResizeBuffers(IsWindowed ? 2 : 3, newWidth, newHeight, (Format)Format, flags);

            var oldSize = new DX.Size2(_info.Width, _info.Height);
			_info.Width = newWidth;
			_info.Height = newHeight;

            CreateResources(rtvIndex);

			AfterSwapChainResized?.Invoke(this, new AfterSwapChainResizedEventArgs(new DX.Size2(newWidth, newHeight), oldSize));

			Graphics.Log.Print($"SwapChain '{Name}': Back buffers resized.", LoggingLevel.Verbose);
		}

        /// <summary>
        /// Function to capture the back buffer data and place it into a new texture.
        /// </summary>
        /// <param name="usage">The intended usage for the texture.</param>
        /// <param name="binding">The binding mode for the texture.</param>
        /// <returns>The new texture.</returns>
        /// <exception cref="GorgonException">Thrown when the <paramref name="usage"/> parameter is not <see cref="ResourceUsage.Default"/>, or <see cref="ResourceUsage.Staging"/>.</exception>
        /// <remarks>
        /// <para>
        /// This method creates a copy of whatever is in the back buffer for the swap chain, and places that data into a new <see cref="GorgonTexture2D"/>. 
        /// </para>
        /// <para>
        /// Applications can define the <paramref name="usage"/> and <paramref name="binding"/> of the resulting texture so that the texture data can be displayed as a texture when rendering, or used as 
        /// a staging texture.  If the <paramref name="usage"/> parameter is not <see cref="ResourceUsage.Default"/>, or <see cref="ResourceUsage.Staging"/>, then an exception will be thrown.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonTexture2D"/>
        public GorgonTexture2D CopyBackBufferToTexture(ResourceUsage usage, TextureBinding binding)
        {
            if ((usage != ResourceUsage.Default) && (usage != ResourceUsage.Staging))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_BACKBUFFER_USAGE_INVALID);
            }

            var texture = new GorgonTexture2D(Graphics, new GorgonTexture2DInfo(_backBufferTextures[0], $"{Name} - Backbuffer copy")
            {
                Usage = usage,
                Binding = binding
            });

            _backBufferTextures[0].CopyTo(texture);

            return texture;
        }

        /// <summary>
        /// Function to capture the back buffer data and place it into a <see cref="IGorgonImage"/>.
        /// </summary>
        /// <returns>A new <see cref="IGorgonImage"/> that will contain the back buffer data.</returns>
        /// <remarks>
        /// <para>
        /// This method is similar to the <see cref="CopyBackBufferToTexture"/> method, but will instead copy the back buffer data into a <see cref="IGorgonImage"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="IGorgonImage"/>
        public IGorgonImage CopyBackBufferToImage()
        {
            using (GorgonTexture2D texture = CopyBackBufferToTexture(ResourceUsage.Staging, TextureBinding.None))
            {
                return texture.ToImage();
            }
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
			PresentFlags flags = !IsInStandBy ? PresentFlags.None : PresentFlags.Test;

			interval.ValidateRange(nameof(interval), 0, 4, true, true);

            if ((_supportsTearing != 0) && (interval == 0) && (!IsInStandBy))
            {
                flags |= PresentFlags.AllowTearing;
            }

			try
			{
				IsInStandBy = false;
			    
			    (int FirstIndex, int TargetCount) prevTargetRange = (0, 0);
			    GorgonDepthStencil2DView prevDepthStencil = null;

                // In flip modes, we have to unbind the render targets before presenting.
			    if (_info.UseFlipMode)
			    {
			        prevDepthStencil = Graphics.DepthStencilView;
			        prevTargetRange = GetCurrentTargets();

                    // If we had previous targets (and we are part of that list), then reset the targets before presenting (the runtime will do it for us anyway, but this will just 
                    // get rid of that annoying warning in the debug spew).
			        if (prevTargetRange.TargetCount != 0)
			        {
			            Graphics.SetRenderTarget(null);
			        }
			    }

			    _swapChain.Present(interval, flags);

			    if (prevTargetRange.TargetCount == 0)
			    {
			        return;
			    }

                // The typical use case is that we have only a single rtv set when we present.  So, rather than restoring everything
                // we should just restore the single rtv.
			    Graphics.SetRenderTargets(_previousViews, prevDepthStencil);
                
                // Remove all items from the list so we don't hang on to them.
			    Array.Clear(_previousViews, prevTargetRange.FirstIndex, prevTargetRange.TargetCount);
			}
			catch (DX.SharpDXException sdex)
			{
				if ((sdex.ResultCode == ResultCode.DeviceReset)
					|| (sdex.ResultCode == ResultCode.DeviceRemoved)
					|| (sdex.ResultCode == ResultCode.ModeChangeInProgress)
					|| (sdex.ResultCode.Code == (int)DXGIStatus.ModeChangeInProgress)
					|| (sdex.ResultCode.Code == (int)DXGIStatus.Occluded))
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
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            SwapChain4 swapChain = Interlocked.Exchange(ref _swapChain, null);

            BeforeSwapChainResized = null;
            AfterSwapChainResized = null;

            if (ParentForm != null)
            {
                // We assign these events to the parent form so that a window resize is smooth, currently using the Resize event only introduces massive
                // lag when resizing the back buffers. This will counter that by only resizing when the resize operation ends.
                ParentForm.ResizeBegin -= ParentForm_ResizeBegin;
                ParentForm.ResizeEnd -= ParentForm_ResizeEnd;

                // Use these events to restore full screen or windowed state when the application regains or loses focus.
                ParentForm.Activated -= ParentForm_Activated;
                ParentForm.Deactivate -= ParentForm_Deactivated;

            }

            if (Window != null)
            {
                Window.Resize -= Window_Resize;
            }

            if (swapChain == null)
            {
                this.UnregisterDisposable(Graphics);
                return;
            }

            if (!IsWindowed)
            {
                // Always go to windowed mode before destroying the swap chain.
                swapChain.SetFullscreenState(false, null);
                FullscreenOutput = null;
                _fullScreenVideoMode = null;
            }

            DestroyResources(true);
            this.UnregisterDisposable(Graphics);
            swapChain.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonSwapChain"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface that created this object.</param>
        /// <param name="control">The control bound to the swap chain.</param>
        /// <param name="info">The information used to create the object.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="info"/> or the <paramref name="control"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <see cref="IGorgonSwapChainInfo.Format"/> is not a display format.</exception>
        public GorgonSwapChain(GorgonGraphics graphics,
                               Control control,
                               GorgonSwapChainInfo info)
        {
            Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            ValidateSwapChainInfo(info);
            Window = control ?? throw new ArgumentNullException(nameof(control));
            ParentForm = FindForm(control);

            Debug.Assert(ParentForm != null, "No parent form found for control.");

            SwapChainDescription1 desc = info.ToSwapChainDesc();

            this.RegisterDisposable(graphics);

            if (info.UseFlipMode)
            {
                unsafe
                {
                    fixed (int* supportsTearing = &_supportsTearing)
                    {
                        Graphics.DXGIFactory.CheckFeatureSupport(Feature.PresentAllowTearing, new IntPtr(&supportsTearing), sizeof(int));
                    }
                }
                
                if (_supportsTearing != 0)
                {
                    desc.Flags |= SwapChainFlags.AllowTearing;
                }
            }

            using (var dxgiSwapChain = new SwapChain1(Graphics.DXGIFactory, Graphics.D3DDevice, control.Handle, ref desc)
                                                   {
                                                       DebugName = $"{info.Name}_DXGISwapChain4"
                                                   })
            {
                _swapChain = dxgiSwapChain.QueryInterface<SwapChain4>();
            }

            _info = new GorgonSwapChainInfo(in desc);
            _backBufferTextures = new GorgonTexture2D[_info.UseFlipMode ? 2 : 1];

            Graphics.DXGIFactory.MakeWindowAssociation(control.Handle, WindowAssociationFlags.IgnoreAll);
            
            CreateResources(-1);

            Window.Resize += Window_Resize;

            // We assign these events to the parent form so that a window resize is smooth, currently using the Resize event only introduces massive
            // lag when resizing the back buffers. This will counter that by only resizing when the resize operation ends.
            ParentForm.ResizeBegin += ParentForm_ResizeBegin;
            ParentForm.ResizeEnd += ParentForm_ResizeEnd;

            // Use these events to restore full screen or windowed state when the application regains or loses focus.
            ParentForm.Activated += ParentForm_Activated;
            ParentForm.Deactivate += ParentForm_Deactivated;
            
        }
        #endregion
    }
}
