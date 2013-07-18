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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics.Properties;
using GorgonLibrary.Native;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Render target after resized event arguments.
	/// </summary>
	public class GorgonAfterSwapChainResizedEventArgs
		: EventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to return the new width of the target.
		/// </summary>
		public int Width
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the new height of the target.
		/// </summary>
		public int Height
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the swap chain is in full screen or windowed mode.
		/// </summary>
		public bool IsWindowed
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonAfterSwapChainResizedEventArgs" /> class.
		/// </summary>
		/// <param name="target">Render target that was resized.</param>
		public GorgonAfterSwapChainResizedEventArgs(GorgonSwapChain target)
		{
			Width = target.Settings.Width;
			Height = target.Settings.Height;
			IsWindowed = target.Settings.IsWindowed;
		}
		#endregion
	}

	/// <summary>
	/// Event parameters for a full screen/windowed state change.
	/// </summary>
	public class GorgonBeforeStateTransitionEventArgs
		: GorgonCancelEventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to return whether the swap chain was previously windowed.
		/// </summary>
		public bool WasWindowed
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBeforeStateTransitionEventArgs"/> class.
		/// </summary>
		/// <param name="wasWindowed">TRUE if the swap chain was previously windowed, FALSE if it was full screen.</param>
		public GorgonBeforeStateTransitionEventArgs(bool wasWindowed)
			: base(false)
		{
			WasWindowed = wasWindowed;
		}
		#endregion
	}

	/// <summary>
	/// A swap chain used to display graphics to a window.
	/// </summary>
	/// <remarks>The swap chain is used to display data to the <see cref="GorgonLibrary.Graphics.GorgonVideoOutput">video output</see>, or it can be used as a shader input.
	/// <para>Swap chains embedded into child controls (a panel, group box, etc...) will not be able to switch to full screen mode and will automatically revert to windowed mode.</para>
	/// <para>Multiple swap chains can be set to full screen on different video outputs.  When setting up for multiple video outputs in full screen, ensure that the window
	/// for the extra video output is located on the monitor attached to that video output.  Failure to do so will keep the mode from switching.
	/// </para>	
	/// <para>
	/// Note that due to a known limitation on Windows 7, it is not currently possible to switch to full screen on multiple outputs on <see cref="GorgonLibrary.Graphics.GorgonVideoDevice">multiple video devices</see>.  
	/// One possible workaround is to create a full screen borderless window on the secondary device and use that as a "fake" full screen mode.  If this workaround
	/// is applied, then it is suggested to disable the Desktop Windows Compositor.  To disable the compositor, see this link http://msdn.microsoft.com/en-us/library/aa969510.aspx.
	/// </para>	
	/// <para>If the window loses focus and the swap chain is in full screen, it will revert to windowed mode.  The swap chain will attempt to reacquire full screen mode when it regains focus.  
	/// This functionality can be disabled with the <see cref="P:GorgonLibrary.Graphics.GorgonGraphics.ResetFullscreenOnFocus">GorgonGraphics.ResetFullscreenOnFocus</see> property if it does not suit the needs of the 
	/// developer.  This is mandatory in full screen multi-monitor applications, if the ResetFullscreenOnFocus flag is FALSE in this scenario, then the behaviour when switching between applications will be undefined.  
	/// It is the responsibility of the developer to handle task switching in multi-monitor environments.</para>
	/// </remarks>
	public sealed class GorgonSwapChain
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		private GorgonRenderTarget2D _renderTarget;			// The render target bound to this swap chain.
		private bool _disposed;								// Flag to indicate that the object was disposed.
		private Control _topLevelControl;					// Top level control.
		private Form _parentForm;							// Parent form for our window.
		private bool _isInResize;							// Flag to indicate that we're in the middle of a resizing operation.
		#endregion

		#region Events.
        /// <summary>
        /// Event called before the swap chain has been resized.
        /// </summary>
	    public event EventHandler<EventArgs> BeforeSwapChainResized;
		/// <summary>
		/// Event called after the swap chain has been resized.
		/// </summary>
		public event EventHandler<GorgonAfterSwapChainResizedEventArgs> AfterSwapChainResized;
		/// <summary>
		/// Event called before the swap chain transitions to full screen or windowed mode.
		/// </summary>
		public event EventHandler<GorgonBeforeStateTransitionEventArgs> BeforeStateTransition;
		/// <summary>
		/// Event called after the swap chain transiitons to full screen or windowed mode.
		/// </summary>
		public event EventHandler<GorgonAfterSwapChainResizedEventArgs> AfterStateTransition;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the DXGI swap chain interface.
		/// </summary>
		internal GI.SwapChain GISwapChain
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the default depth/stencil buffer for this swap chain.
		/// </summary>
		public GorgonDepthStencil2D DepthStencilBuffer
		{
			get
			{
				return _renderTarget == null ? null : _renderTarget.DepthStencilBuffer;
			}
		}

		/// <summary>
		/// Property to return the graphics interface that owns this object.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return whether the swap chain resizes with its parent control.
		/// </summary>
		public bool AutoResize
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return whether we're in stand by mode.
		/// </summary>
		/// <remarks>Stand by mode is entered when the <see cref="M:GorgonLibrary.Graphics.GorgonSwapChain.Flip">Flip</see> method detects that the window is occluded.</remarks>
		public bool IsInStandBy
		{
			get;
			private set;
		}

		/// <summary>
		/// Function to return the video output that the swap chain is operating on.
		/// </summary>
		public GorgonVideoOutput VideoOutput
		{
			get
			{
				if (GISwapChain == null)
				{
					return null;
				}

				IntPtr handle = Win32API.GetMonitor(Settings.Window);

				for (int i = 0; i < Graphics.VideoDevice.Outputs.Count; i++)
				{
					if (Graphics.VideoDevice.Outputs[i].Handle == handle)
					{
						return Graphics.VideoDevice.Outputs[i];
					}
				}

				return null;
			}
		}

		/// <summary>
		/// Property to return the default viewport associated with this render target.
		/// </summary>
		public GorgonViewport Viewport
		{
			get
			{
				return _renderTarget == null ? new GorgonViewport(0, 0, 0, 0, 0, 0) : _renderTarget.Viewport;
			}
		}

		/// <summary>
		/// Property to return the settings for this swap chain.
		/// </summary>
		public GorgonSwapChainSettings Settings
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Deactivate event of the _parentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _parentForm_Deactivate(object sender, EventArgs e)
		{
		    try
		    {
		        if ((GISwapChain == null)
		            || (!Graphics.ResetFullscreenOnFocus))
		            return;

		        Graphics.GetFullScreenSwapChains();

		        // Reset the video mode to windowed.
		        // Note:  For some reason, this is different than it was on SlimDX.  I never had to do this before, but with
		        // SharpDX I now have to handle the transition from full screen to windowed manually.
		        if (GISwapChain.IsFullScreen)
		        {
		            GISwapChain.SetFullscreenState(false, null);
		            Settings.IsWindowed = true;
		            ((Form)Settings.Window).WindowState = FormWindowState.Minimized;
		        }
		    }
            catch (Exception ex)
            {
#if DEBUG
                GorgonException.Catch(ex,
                                      () =>
                                      UI.GorgonDialogs.ErrorBox(_parentForm,
                                                                string.Format(Resources.GORGFX_CATASTROPHIC_ERROR, Gorgon.Log.LogPath),
                                                                ex));

                // If we fail in here, then we have a terminal error in Gorgon, don't risk further corruption.
                Gorgon.Quit();
#else
		        GorgonException.Catch(ex);
#endif
            }
		}

		/// <summary>
		/// Handles the Activated event of the _parentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _parentForm_Activated(object sender, EventArgs e)
		{
		    try
		    {
		        if ((GISwapChain == null)
		            || (!Graphics.ResetFullscreenOnFocus))
		            return;

		        Graphics.GetFullScreenSwapChains();

		        if (!GISwapChain.IsFullScreen)
		        {
		            ((Form)Settings.Window).WindowState = FormWindowState.Normal;
		            // Get the current video output.				
		            GISwapChain.SetFullscreenState(true, null);
		            Settings.IsWindowed = false;
		        }
		    }
            catch (Exception ex)
            {
#if DEBUG
                GorgonException.Catch(ex,
                                      () =>
                                      UI.GorgonDialogs.ErrorBox(_parentForm,
                                                                string.Format(Resources.GORGFX_CATASTROPHIC_ERROR, Gorgon.Log.LogPath),
                                                                ex));

                // If we fail in here, then we have a terminal error in Gorgon, don't risk further corruption.
                Gorgon.Quit();
#else
		        GorgonException.Catch(ex);
#endif
            }
		}

		/// <summary>
		/// Function to create any resources bound to the swap chain.
		/// </summary>
		/// <param name="targetReseat">The swap chain needs to be re-seated in slot 0 of the render target list.</param>
		/// <param name="depthReseat">The depth/stencil view needs to be re-seated.</param>
		private void CreateResources(bool targetReseat = false, bool depthReseat = false)
		{
			Gorgon.Log.Print("GorgonSwapChain '{0}': Creating D3D11 render target view...", Diagnostics.LoggingLevel.Intermediate, Name);

			if (_renderTarget == null)
			{
				_renderTarget = new GorgonRenderTarget2D(Graphics, Name + "_Internal_Render_Target_" + Guid.NewGuid(), new GorgonRenderTarget2DSettings
					{
						AllowUnorderedAccessViews = (Settings.Flags & SwapChainUsageFlags.AllowUnorderedAccessView) == SwapChainUsageFlags.AllowUnorderedAccessView,
						ArrayCount = 1,
						DepthStencilFormat = Settings.DepthStencilFormat,
						Width = Settings.Width,
						Height = Settings.Height,
						Format = Settings.Format,
						TextureFormat = Settings.Format,
						Multisampling = Settings.Multisampling,
						IsTextureCube = false,
						MipCount = 1,
						ShaderViewFormat = BufferFormat.Unknown
					});
			}
			else
			{
				// Readjust target settings.
                _renderTarget.Settings.Width = Settings.Width;
                _renderTarget.Settings.Height = Settings.Height;
                _renderTarget.Settings.DepthStencilFormat = Settings.DepthStencilFormat;
                _renderTarget.Settings.Format = Settings.Format;
                _renderTarget.Settings.Multisampling = Settings.Multisampling;
			}

			// Initialize (or reinitialize) the target.
            _renderTarget.InitializeSwapChain(this);

			// Re-seat the target/depth stencil.
			if ((targetReseat) && (depthReseat))
			{
				Graphics.Output.SetRenderTarget(_renderTarget, DepthStencilBuffer);
			}
			else if (depthReseat)
			{
				Graphics.Output.DepthStencilView = DepthStencilBuffer;
			}
			else if (targetReseat)
			{
                Graphics.Output.SetRenderTarget(_renderTarget, Graphics.Output.DepthStencilView);
			}
		}

		/// <summary>
		/// Function to resize the back buffers.
		/// </summary>
		private void ResizeBuffers()
		{
            if (BeforeSwapChainResized != null)
            {
                BeforeSwapChainResized(this, EventArgs.Empty);
            }

			var targetReseat = _renderTarget.OnSwapChainResize();
			var depthReseat = DepthStencilBuffer != null && DepthStencilBuffer.OnDepthStencilResize();

		    GISwapChain.ResizeBuffers(Settings.BufferCount, Settings.VideoMode.Width, Settings.VideoMode.Height, (GI.Format)Settings.VideoMode.Format, GI.SwapChainFlags.AllowModeSwitch);
			CreateResources(targetReseat, depthReseat);
            
			if (AfterSwapChainResized != null)
			{
				AfterSwapChainResized(this, new GorgonAfterSwapChainResizedEventArgs(this));
			}
		}

		/// <summary>
		/// Handles the ParentChanged event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Window_ParentChanged(object sender, EventArgs e)
		{
		    try
		    {
		        // If the actual control has changed parents, update the top level control.
		        if (sender == Settings.Window)
		        {
		            var newTopLevelParent = Gorgon.GetTopLevelControl(Settings.Window);

		            if (newTopLevelParent != _topLevelControl)
		            {
		                _topLevelControl.ParentChanged -= Window_ParentChanged;

		                // If we're not at the top of the chain, then find out which window is and set it up
		                // to handle changes to its hierarchy.
		                if (newTopLevelParent != Settings.Window)
		                {
		                    _topLevelControl = newTopLevelParent;
		                    _topLevelControl.ParentChanged += Window_ParentChanged;
		                }
		            }
		        }

		        if (_parentForm != null)
		        {
		            _parentForm.ResizeBegin -= _parentForm_ResizeBegin;
		            _parentForm.ResizeEnd -= _parentForm_ResizeEnd;
		        }

		        _parentForm = Gorgon.GetTopLevelForm(Settings.Window);

		        if (_parentForm == null)
		        {
		            return;
		        }

		        _parentForm.ResizeBegin += _parentForm_ResizeBegin;
		        _parentForm.ResizeEnd += _parentForm_ResizeEnd;
		    }
            catch (Exception ex)
            {
#if DEBUG
                GorgonException.Catch(ex,
                                      () =>
                                      UI.GorgonDialogs.ErrorBox(_parentForm,
                                                                string.Format(Resources.GORGFX_CATASTROPHIC_ERROR, Gorgon.Log.LogPath),
                                                                ex));

                // If we fail in here, then we have a terminal error in Gorgon, don't risk further corruption.
                Gorgon.Quit();
#else
		        GorgonException.Catch(ex);
#endif
            }
		}

		/// <summary>
		/// Handles the ResizeBegin event of the _parentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void _parentForm_ResizeBegin(object sender, EventArgs e)
		{
			_isInResize = true;
		}

		/// <summary>
		/// Handles the ResizeEnd event of the _parentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void _parentForm_ResizeEnd(object sender, EventArgs e)
		{
			_isInResize = false;

			// Only attempt a resize if the window has actually changed size.
			if ((Settings.Window.ClientSize.Width != Settings.Width) || (Settings.Window.ClientSize.Height != Settings.Height))
			{
				Window_Resize(sender, e);
			}
		}

		/// <summary>
		/// Handles the Resize event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Window_Resize(object sender, EventArgs e)
		{
			// If we're in a manual resize operation, then don't call this method just yet.
			if (_isInResize)
			{
				return;
			}

			// Attempt to get the parent form if we don't have one yet.
			if (_parentForm == null)
			{
				_parentForm = Gorgon.GetTopLevelForm(Settings.Window);
				_parentForm.ResizeBegin += _parentForm_ResizeBegin;
				_parentForm.ResizeEnd += _parentForm_ResizeEnd;
			}

			if ((!AutoResize) || ((_parentForm != null) && (_parentForm.WindowState == FormWindowState.Minimized)))
				return;

			// Only do this if the size has changed, if we're just restoring the window, then don't bother.
			if (((GISwapChain == null)) || (Settings.Window.ClientSize.Width <= 0) || (Settings.Window.ClientSize.Height <= 0))
			{
				return;
			}

		    try
		    {
		        // Resize the video mode.
		        Settings.VideoMode = new GorgonVideoMode(Settings.Window.ClientSize.Width,
		                                                 Settings.Window.ClientSize.Height,
		                                                 Settings.VideoMode.Format,
		                                                 Settings.VideoMode.RefreshRateNumerator,
		                                                 Settings.VideoMode.RefreshRateDenominator);
		        ResizeBuffers();
		    }
		    catch (Exception ex)
		    {
#if DEBUG
                GorgonException.Catch(ex,
                                      () =>
                                      UI.GorgonDialogs.ErrorBox(_parentForm,
                                                                string.Format(Resources.GORGFX_CATASTROPHIC_ERROR, Gorgon.Log.LogPath),
                                                                ex));

                // If we fail in here, then we have a terminal error in Gorgon, don't risk further corruption.
                Gorgon.Quit();
#else
                // Log the exception.
		        GorgonException.Catch(ex);
#endif
		    }
		}

		/// <summary>
		/// Function to update the fullscreen/windowed mode state.
		/// </summary>
		private void ModeStateUpdate()
		{
			GorgonBeforeStateTransitionEventArgs e = null;

			GI.ModeDescription mode = GorgonVideoMode.Convert(Settings.VideoMode);

			try
			{
				GISwapChain.ResizeTarget(ref mode);

				if (!Settings.IsWindowed)
				{
					e = new GorgonBeforeStateTransitionEventArgs(true);
					if (BeforeStateTransition != null)
						BeforeStateTransition(this, new GorgonBeforeStateTransitionEventArgs(true));

					if (!e.Cancel)
					{
						// We don't need to force an output.  We'll just let DXGI figure it out from the 
						// window area on the monitor.  Currently SharpDX's ContainingOutput property is
						// buggy in 2.4.2 and will return multiple ref counts for each time the property is 
						// read.  v2.5.0 is in dev, but now has the problem of not returning the correct output.						
						GISwapChain.SetFullscreenState(true, null);
						if (_parentForm != null)
						{
							_parentForm.Activated += _parentForm_Activated;
							_parentForm.Deactivate += _parentForm_Deactivate;
						}
					}
				}
				else
				{
					e = new GorgonBeforeStateTransitionEventArgs(false);

					if (BeforeStateTransition != null)
						BeforeStateTransition(this, new GorgonBeforeStateTransitionEventArgs(false));

					if (!e.Cancel)
						GISwapChain.SetFullscreenState(false, null);
				}
			}
			catch (SharpDX.SharpDXException sdEx)
			{
				if (sdEx.ResultCode.Code == (int)GI.DXGIStatus.ModeChangeInProgress)
				{
					Gorgon.Log.Print("GorgonSwapChain '{0}': Could not switch to full screen mode because the device was busy switching to full screen on another output.", LoggingLevel.All, Name);
				}
				else
				{
					if (sdEx.ResultCode == GI.ResultCode.NotCurrentlyAvailable)
					{
						Gorgon.Log.Print(
							"GorgonSwapChain '{0}': Could not switch to full screen mode because the device is not currently available.  Possible causes are:  .",
							LoggingLevel.All, Name);
					}
					else
					{
						throw;
					}
				}
			}

			ResizeBuffers();

			if ((!e.Cancel) && (AfterStateTransition != null))
			{
				AfterStateTransition(this, new GorgonAfterSwapChainResizedEventArgs(this));
			}
		}

		/// <summary>
		/// Function to perform updating of the swap chain settings.
		/// </summary>
		/// <param name="graphics">Gorgon graphics interface to use.</param>
		/// <param name="settings">Settings to change.</param>
		internal static void ValidateSwapChainSettings(GorgonGraphics graphics, GorgonSwapChainSettings settings)
		{
			D3D.Device d3dDevice = null;
			IntPtr monitor = IntPtr.Zero;
			GorgonVideoOutput output = null;

            // Define as render target if we didn't specify the flags.
            if (settings.Flags == SwapChainUsageFlags.None)
            {
                settings.Flags = SwapChainUsageFlags.RenderTarget;
            }

			if (graphics.VideoDevice == null)
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the swap chain, no video device was selected.");

			if ((graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) &&
			    (settings.Flags != SwapChainUsageFlags.RenderTarget))
			{
			    throw new GorgonException(GorgonResult.CannotCreate,
			                              string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM4));
			}

            // Ensure that we're using SM5 or better hardware if we want an unordered access view to our backbuffer.
            if ((graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
                && ((settings.Flags & SwapChainUsageFlags.AllowUnorderedAccessView)
                    == SwapChainUsageFlags.AllowUnorderedAccessView))
            {
                throw new GorgonException(GorgonResult.CannotCreate,
                                          string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM5));
            }

			// Default to using the default Gorgon application window.
			if (settings.Window == null)
			{
				settings.Window = Gorgon.ApplicationForm;

				// No application window, then we're out of luck.
				if (settings.Window == null)
					throw new ArgumentException("No window to bind with the swap chain.", "settings");
			}

			// Force to windowed mode if we're binding to a child control on a form.
			if (!(settings.Window is Form))
				settings.IsWindowed = true;

			monitor = Win32API.GetMonitor(settings.Window);		// Get the monitor that the window is on.

			// Find the video output for the window.
			output = (from videoOutput in graphics.VideoDevice.Outputs
					  where videoOutput.Handle == monitor
					  select videoOutput).SingleOrDefault();

			if (output == null)
			{
				throw new GorgonException(GorgonResult.CannotCreate, "Could not find the video output for the specified window.");
			}

			// Get the Direct 3D device instance.
			d3dDevice = graphics.D3DDevice;

			// If we've not defined a video mode, determine the best mode to use.
			GorgonVideoMode stagedMode = settings.VideoMode;

			// Fill in any missing settings.
		    if (stagedMode.Width == 0)
		    {
		        stagedMode = new GorgonVideoMode(settings.Window.ClientSize.Width,
		                                         stagedMode.Height,
		                                         stagedMode.Format,
		                                         stagedMode.RefreshRateNumerator,
		                                         stagedMode.RefreshRateDenominator);
		    }
		    if (stagedMode.Height == 0)
		    {
                stagedMode = new GorgonVideoMode(stagedMode.Width,
                                                 settings.Window.ClientSize.Height,
                                                 stagedMode.Format,
                                                 stagedMode.RefreshRateNumerator,
                                                 stagedMode.RefreshRateDenominator);
            }
		    if (stagedMode.Format == BufferFormat.Unknown)
		    {
                stagedMode = new GorgonVideoMode(stagedMode.Width,
                                                 stagedMode.Height,
                                                 output.DefaultVideoMode.Format,
                                                 stagedMode.RefreshRateNumerator,
                                                 stagedMode.RefreshRateDenominator);
		    }
		    if ((stagedMode.RefreshRateDenominator == 0) || (stagedMode.RefreshRateNumerator == 0))
			{
                stagedMode = new GorgonVideoMode(stagedMode.Width,
                                                 stagedMode.Height,
                                                 stagedMode.Format,
                                                 output.DefaultVideoMode.RefreshRateNumerator,
                                                 output.DefaultVideoMode.RefreshRateDenominator);
			}

			// If the device does not support different full screen modes (e.g. WARP/Refrast on Windows 8), then reset the windowed switch.
			if ((output.VideoModes.Count == 0) && (settings.IsWindowed))
			{
				settings.IsWindowed = true;
			}

			// If going full screen, ensure that whatever mode we've chosen can be used, otherwise go to the closest match.
			if (!settings.IsWindowed)
			{
				// Check to ensure that no other swap chain is on the video output if we're going to full screen mode.
			    if (graphics.GetTrackedObjectsOfType<GorgonSwapChain>()
			                .Any(
			                    item =>
			                    (item.VideoOutput == output) && (!item.Settings.IsWindowed)
			                    && (item.Settings.Window != settings.Window)))
			    {
			        throw new GorgonException(GorgonResult.CannotCreate,
			                                  "There is already a full screen swap chain active on the video output '"
			                                  + output.Name + "'.");
			    }

			    var modeCount = (from mode in output.VideoModes
								 where mode == stagedMode
								 select mode).Count();

				// We couldn't find the mode in the list, find the nearest match.
				if (modeCount == 0)
				{
					stagedMode = output.FindMode(stagedMode);
				}
			}
					
			// Ensure that the selected video format can be used.
			if (!graphics.VideoDevice.SupportsDisplayFormat(stagedMode.Format))
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot use the format '" + stagedMode.Format.ToString() + "' for display on the video device '" + graphics.VideoDevice.Name + "'.");

			settings.VideoMode = stagedMode;

			// Check multi sampling levels.
			if (settings.SwapEffect == SwapEffect.Sequential)
				settings.Multisampling = new GorgonMultisampling(1, 0);
			
			int quality = graphics.VideoDevice.GetMultiSampleQuality(settings.VideoMode.Format, settings.Multisampling.Count);

			// Ensure that the quality of the sampling does not exceed what the card can do.
			if ((settings.Multisampling.Quality >= quality) || (settings.Multisampling.Quality < 0))
				throw new GorgonException(GorgonResult.CannotCreate, "Video device '" + graphics.VideoDevice.Name + "' does not support multisampling with a count of '" + settings.Multisampling.Count.ToString() + "' and a quality of '" + settings.Multisampling.Quality.ToString() + " with a format of '" + settings.VideoMode.Format + "'");

			// Force 2 buffers for discard.
			if ((settings.BufferCount < 2) && (settings.SwapEffect == SwapEffect.Discard))
				settings.BufferCount = 2;

			// Perform window handling.
			settings.Window.Visible = true;
			settings.Window.Enabled = true;			
		}

		/// <summary>
		/// Function to intialize the swap chain.
		/// </summary>
		internal void Initialize()
		{
			var flags = GI.SwapChainFlags.AllowModeSwitch;
			var d3dSettings = new GI.SwapChainDescription();

			// Resize the window to match requested mode size.
			if ((_parentForm == Settings.Window) && (Settings.IsWindowed) && (!Settings.NoClientResize))
				_parentForm.ClientSize = new Size(Settings.VideoMode.Width, Settings.VideoMode.Height);

			AutoResize = !Settings.NoClientResize;
			Graphics.GetFullScreenSwapChains();

			d3dSettings.BufferCount = Settings.BufferCount;
			d3dSettings.Flags = flags;

			d3dSettings.IsWindowed = true;
			d3dSettings.ModeDescription = GorgonVideoMode.Convert(Settings.VideoMode);
			d3dSettings.OutputHandle = Settings.Window.Handle;
			d3dSettings.SampleDescription = GorgonMultisampling.Convert(Settings.Multisampling);
			d3dSettings.SwapEffect = GorgonSwapChainSettings.Convert(Settings.SwapEffect);

		    if ((Settings.Flags & SwapChainUsageFlags.RenderTarget) == SwapChainUsageFlags.RenderTarget)
		    {
		        d3dSettings.Usage = GI.Usage.RenderTargetOutput;
		    }

		    if ((Settings.Flags & SwapChainUsageFlags.AllowShaderView) == SwapChainUsageFlags.AllowShaderView)
		    {
		        d3dSettings.Usage |= GI.Usage.ShaderInput;
		    }

		    if ((Settings.Flags & SwapChainUsageFlags.AllowUnorderedAccessView)
                == SwapChainUsageFlags.AllowUnorderedAccessView)
            {
                d3dSettings.Usage |= GI.Usage.UnorderedAccess;
            }

			Gorgon.Log.Print("GorgonSwapChain '{0}': Creating D3D11 swap chain...", Diagnostics.LoggingLevel.Simple, Name);
			GISwapChain = new GI.SwapChain(Graphics.GIFactory, Graphics.D3DDevice, d3dSettings);
			GISwapChain.DebugName = Name + " DXGISwapChain";

			// Due to an issue with winforms and DXGI, we have to manually handle transitions ourselves.
			Graphics.GIFactory.MakeWindowAssociation(Settings.Window.Handle, GI.WindowAssociationFlags.IgnoreAll);

			if (!Settings.IsWindowed)
			{
				ModeStateUpdate();
			}

			CreateResources();

			Settings.Window.Resize += Window_Resize;
			
			if (_parentForm == null)
			{
				return;
			}

			_parentForm.ResizeBegin += _parentForm_ResizeBegin;
			_parentForm.ResizeEnd += _parentForm_ResizeEnd;
		}

        /// <summary>
        /// Function to clear the render target.
        /// </summary>
        /// <param name="color">Color used to clear the render target.</param>
        /// <param name="deferred">[Optional] A deferred context to use when clearing the render target.</param>
        /// <remarks>
        /// This will only clear the default render target view. Any attached depth/stencil buffer will remain untouched.
        /// <para>
        /// If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net), the immediate context will be used to clear the render target.  If it is non-NULL, then it 
        /// will use the specified deferred context to clear the render target.
        /// <para>If you are using a deferred context, it is necessary to use that context to clear the render target because 2 threads may not access the same resource at the same time.  
        /// Passing a separate deferred context will alleviate that.</para>
        /// </para>
        /// </remarks>
        public void Clear(GorgonColor color, GorgonGraphics deferred = null)
        {
            _renderTarget.Clear(color, deferred);
        }

        /// <summary>
        /// Function to clear the render target and an attached depth buffer.
        /// </summary>
        /// <param name="color">Color used to clear the render target.</param>
        /// <param name="depthValue">Value used to clear the depth buffer.</param>
        /// <param name="deferred">[Optional] A deferred context to use when clearing the render target.</param>
        /// <remarks>
        /// This will only clear the default render target view. Any stencil buffer will remain untouched.
        /// <para>
        /// If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net), the immediate context will be used to clear the render target.  If it is non-NULL, then it 
        /// will use the specified deferred context to clear the render target.
        /// <para>If you are using a deferred context, it is necessary to use that context to clear the render target because 2 threads may not access the same resource at the same time.  
        /// Passing a separate deferred context will alleviate that.</para>
        /// </para>
        /// </remarks>
        public void Clear(GorgonColor color, float depthValue, GorgonGraphics deferred = null)
        {
            _renderTarget.Clear(color, depthValue, deferred);
        }

        /// <summary>
        /// Function to clear the render target and an attached depth buffer.
        /// </summary>
        /// <param name="color">Color used to clear the render target.</param>
        /// <param name="stencilValue">Value used to clear the stencil buffer.</param>
        /// <param name="deferred">[Optional] A deferred context to use when clearing the render target.</param>
        /// <remarks>
        /// This will only clear the default render target view.  Any depth buffer will remain untouched.
        /// <para>
        /// If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net), the immediate context will be used to clear the render target.  If it is non-NULL, then it 
        /// will use the specified deferred context to clear the render target.
        /// <para>If you are using a deferred context, it is necessary to use that context to clear the render target because 2 threads may not access the same resource at the same time.  
        /// Passing a separate deferred context will alleviate that.</para>
        /// </para>
        /// </remarks>
        public void Clear(GorgonColor color, byte stencilValue, GorgonGraphics deferred = null)
        {
            _renderTarget.Clear(color, stencilValue, deferred);
        }

        /// <summary>
        /// Function to clear the render target and an attached depth/stencil buffer.
        /// </summary>
        /// <param name="color">Color used to clear the render target.</param>
        /// <param name="depthValue">Value used to clear the depth buffer.</param>
        /// <param name="stencilValue">Value used to clear the stencil buffer.</param>
        /// <param name="deferred">[Optional] A deferred context to use when clearing the render target.</param>
        /// <remarks>
        /// This will only clear the default render target view.
        /// <para>
        /// If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net), the immediate context will be used to clear the render target.  If it is non-NULL, then it 
        /// will use the specified deferred context to clear the render target.
        /// <para>If you are using a deferred context, it is necessary to use that context to clear the render target because 2 threads may not access the same resource at the same time.  
        /// Passing a separate deferred context will alleviate that.</para>
        /// </para>
        /// </remarks>
        public void Clear(GorgonColor color, float depthValue, byte stencilValue, GorgonGraphics deferred = null)
        {
            _renderTarget.Clear(color, depthValue, stencilValue, deferred);
        }

		/// <summary>
		/// Function to update the settings for the swap chain.
		/// </summary>
		/// <param name="mode">New video mode to use.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Window">GorgonSwapChainSettings.Window</see> property is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonSwapChainSettings.VideoMode.Format</see> property cannot be used by the video device for displaying data.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.Multisamplings.Quality</see> property is higher than what the video device can support.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		public void UpdateSettings(GorgonVideoMode mode)
		{
			UpdateSettings(mode, Settings.IsWindowed, Settings.DepthStencilFormat, Settings.BufferCount);
		}

		/// <summary>
		/// Function to update the settings for the swap chain.
		/// </summary>
		/// <param name="isWindowed">TRUE to use windowed mode, FALSE to use full screen mode.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Window">GorgonSwapChainSettings.Window</see> property is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonSwapChainSettings.VideoMode.Format</see> property cannot be used by the video device for displaying data.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.Multisamplings.Quality</see> property is higher than what the video device can support.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		public void UpdateSettings(bool isWindowed)
		{
			UpdateSettings(Settings.VideoMode, isWindowed, Settings.DepthStencilFormat, Settings.BufferCount);
		}

		/// <summary>
		/// Function to update the settings for the swap chain.
		/// </summary>
		/// <param name="mode">New video mode to use.</param>
		/// <param name="isWindowed">TRUE to use windowed mode, FALSE to use full screen mode.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Window">GorgonSwapChainSettings.Window</see> property is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonSwapChainSettings.VideoMode.Format</see> property cannot be used by the video device for displaying data.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.Multisamplings.Quality</see> property is higher than what the video device can support.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		public void UpdateSettings(GorgonVideoMode mode, bool isWindowed)
		{
			UpdateSettings(Settings.VideoMode, isWindowed, Settings.DepthStencilFormat, Settings.BufferCount);
		}

		/// <summary>
		/// Function to update the settings for the render target.
		/// </summary>
		/// <param name="mode">New video mode to use.</param>
		/// <param name="depthStencilFormat">The format of the internal depth/stencil buffer.</param>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonRenderTargetSettings.VideoMode.Format</see> property cannot be used by the render target.
		///   <para>-or-</para>
		///   <para>The width and height are not valid for the render target.</para>
		///   </exception>
		public void UpdateSettings(GorgonVideoMode mode, BufferFormat depthStencilFormat)
		{
			UpdateSettings(mode, Settings.IsWindowed, depthStencilFormat, Settings.BufferCount);
		}

		/// <summary>
		/// Function to update the settings for the swap chain.
		/// </summary>
		/// <param name="mode">New video mode to use.</param>
		/// <param name="depthStencilFormat">The format of the internal depth/stencil buffer.</param>
		/// <param name="isWindowed">TRUE to switch to windowed mode, FALSE to switch to full screen.</param>
		/// <param name="bufferCount">Number of back buffers.</param>
		/// <remarks>If the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.SwapEffect">SwapEffect</see> for the swap chain is set to discard, then the <paramref name="bufferCount"/> must be greater than 1.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Window">GorgonSwapChainSettings.Window</see> property is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonSwapChainSettings.VideoMode.Format</see> property cannot be used by the video device for displaying data.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		public void UpdateSettings(GorgonVideoMode mode, bool isWindowed, BufferFormat depthStencilFormat, int bufferCount)
		{
			if (GISwapChain == null)
				return;

			if (_parentForm != null)
			{
				_parentForm.Activated -= _parentForm_Activated;
				_parentForm.Deactivate -= _parentForm_Deactivate;
			}

			// Assign the new settings.	
			Settings.IsWindowed = isWindowed;			
			Settings.VideoMode = mode;
			Settings.BufferCount = bufferCount;
			Settings.DepthStencilFormat = depthStencilFormat;

			// Validate and modify the settings as appropriate.
			ValidateSwapChainSettings(Graphics, Settings);

			ModeStateUpdate();			
			
			// Ensure our window is the proper size.
			if ((_parentForm == Settings.Window) && (Settings.IsWindowed) && ((Settings.VideoMode.Width != Settings.Window.ClientSize.Width) || (Settings.VideoMode.Height != Settings.Window.ClientSize.Height)))
				Settings.Window.ClientSize = new Size(Settings.VideoMode.Width, Settings.VideoMode.Height);
		}

		/// <summary>
		/// Function to flip the buffers to the front buffer.
		/// </summary>
		/// <param name="interval">Vertical blank interval.</param>
		/// <remarks>If <paramref name="interval"/> parameter is greater than 0, then this method will synchronize to the vertical blank count specified by interval  Passing 0 will display immediately.
		/// <para>If the window that the swap chain is bound with is occluded and/or the swap chain is in between a mode switch, then this method will place the swap chain into stand by mode, and will recover (i.e. turn off stand by) once the device is ready for rendering again.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the interval parameter is less than 0 or greater than 4.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the method encounters an unrecoverable error.</exception>
		public void Flip(int interval)
		{
			var flags = GI.PresentFlags.None;
			SharpDX.Result result = SharpDX.Result.Ok;

			GorgonDebug.AssertParamRange(interval, 0, 4, true, true, "interval");

		    if (GISwapChain == null)
		    {
		        return;
		    }

		    if (IsInStandBy)
		    {
		        flags = GI.PresentFlags.Test;
		    }

		    try
			{
				IsInStandBy = false;
				GISwapChain.Present(interval, flags);
			}
			catch (SharpDX.SharpDXException sdex)
			{
			    if (sdex.ResultCode == SharpDX.Result.Ok)
			    {
			        return;
			    }

			    if (!result.Success)
			    {
                    throw new GorgonException(GorgonResult.CannotWrite, "Cannot update the swap chain front buffer.\nAn unrecoverable error has occurred:\n" + D3DErrors.GetError(result.Code).Description + " (" + D3DErrors.GetError(result.Code).Code + ")");
			    }

                IsInStandBy = true;
			}
		}

		/// <summary>
		/// Function to flip the buffers to the front buffer.
		/// </summary>
		/// <remarks>If the window that the swap chain is bound with is occluded and/or the swap chain is in between a mode switch, then this method will place the swap chain into stand by mode, and will recover (i.e. turn off stand by) once the device is ready for rendering again.
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the method encounters an unrecoverable error.</exception>
		public void Flip()
		{
			Flip(0);
		}

        /// <summary>
        /// Explicit operator to retrieve the 2D texture used as the render target for a swap chain.
        /// </summary>
        /// <param name="swapChain">Swap chain to evaluate.</param>
        /// <returns>The 2D texture used as the render target for a swap chain.</returns>
        public static explicit operator GorgonTexture2D(GorgonSwapChain swapChain)
        {
            return swapChain == null ? null : swapChain._renderTarget;
        }

        /// <summary>
        /// Implicit operator to retrieve the 2D render target for a swap chain.
        /// </summary>
        /// <param name="swapChain">Swap chain to evaluate.</param>
        /// <returns>The 2D render target for a swap chain.</returns>
        public static explicit operator GorgonRenderTarget2D(GorgonSwapChain swapChain)
        {
            return swapChain == null ? null : swapChain._renderTarget;
        }

		/// <summary>
		/// Function to retrieve the render target view for a swap chain.
		/// </summary>
		/// <param name="swapChain">Swap chain to evaluate.</param>
		/// <returns>The render target view for the swap chain.</returns>
		public static GorgonRenderTargetView ToRenderTargetView(GorgonSwapChain swapChain)
		{
			return swapChain == null ? null : swapChain._renderTarget;
		}

		/// <summary>
		/// Implicit operator to return the render target view for a swap chain.
		/// </summary>
		/// <param name="swapChain">Swap chain to evaluate.</param>
		/// <returns>The render target view for the swap chain.</returns>
		public static implicit operator GorgonRenderTargetView(GorgonSwapChain swapChain)
		{
			return swapChain == null ? null : swapChain._renderTarget;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSwapChain"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this swap chain.</param>
		/// <param name="name">The name of the swap chain.</param>
		/// <param name="settings">Settings for the swap chain.</param>
		internal GorgonSwapChain(GorgonGraphics graphics, string name, GorgonSwapChainSettings settings)
			: base(name)
		{
			Graphics = graphics;
			Settings = settings;

			// Get the parent form for our window.
			_parentForm = Gorgon.GetTopLevelForm(settings.Window);
			_topLevelControl = Gorgon.GetTopLevelControl(settings.Window);
			settings.Window.ParentChanged += Window_ParentChanged;

			if (_topLevelControl != settings.Window)
			{
				_topLevelControl.ParentChanged += Window_ParentChanged;
			}

			AutoResize = true;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				Settings.Window.ParentChanged -= Window_ParentChanged;

				if (_topLevelControl != null)
				{
					_topLevelControl.ParentChanged -= Window_ParentChanged;
				}

				if (_parentForm != null)
				{
					_parentForm.ResizeBegin -= _parentForm_ResizeBegin;
					_parentForm.ResizeEnd -= _parentForm_ResizeEnd;
					_parentForm.Activated -= _parentForm_Activated;
					_parentForm.Deactivate -= _parentForm_Deactivate;
				}

				Settings.Window.Resize -= Window_Resize;

				if (_renderTarget != null)
				{
					_renderTarget.Dispose();
					_renderTarget = null;
				}

				Gorgon.Log.Print("GorgonSwapChain '{0}': Removing D3D11 swap chain...", Diagnostics.LoggingLevel.Simple, Name);
				if (GISwapChain != null)
				{
					// Always go to windowed mode before destroying the swap chain.
					GISwapChain.SetFullscreenState(false, null);
					GISwapChain.Dispose();
				}
				if (Graphics != null)
					Graphics.RemoveTrackedObject(this);

				GISwapChain = null;
			}

			Graphics = null;
			_disposed = true;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
    }
}