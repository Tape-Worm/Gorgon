#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Friday, April 27, 2007 9:10:23 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Drawing = System.Drawing;
using SharpUtilities;
using SharpUtilities.Utility;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing the primary rendering window.
	/// </summary>
	public class PrimaryRenderWindow
		: RenderWindow
	{
		#region Structs.
		/// <summary>
		/// Structure to save the control/form settings.
		/// </summary>
		public struct WinSettings
		{
			#region Variables.
			/// <summary>
			/// Caption of the control.
			/// </summary>
			public string Caption;
			/// <summary>
			/// Minimize box present.
			/// </summary>
			public bool MinimizeBox;
			/// <summary>
			/// Maximize box present.
			/// </summary>
			public bool MaximizeBox;
			/// <summary>
			/// Maximize box present.
			/// </summary>
			public bool ControlBox;
			/// <summary>
			/// Top most form.
			/// </summary>
			public bool TopMost;
			/// <summary>
			/// Form border style.
			/// </summary>
			public FormBorderStyle Style;
			/// <summary>
			/// Form X position.
			/// </summary>
			public int Left;
			/// <summary>
			/// Form Y position.
			/// </summary>
			public int Top;
			/// <summary>
			/// Form width.
			/// </summary>
			public int Width;
			/// <summary>
			/// Form height.
			/// </summary>
			public int Height;
			#endregion
		}
		#endregion

		#region Variables.
		private D3D9.Device _device;						// Device object.
		private WinSettings _formSettings;					// Form settings.		
		private bool _windowedMode;							// Flag to indicate whether we're windowed or not.		
		private bool _deviceWasLost;						// Flag indicating the device is in a lost state.
		private bool _allowResizeEvent;						// Flag to allow the resize event.

		private static Font _defaultFont = null;			// Default font.
		private static Image _logoImage = null;				// Gorgon logo image.
		private static Sprite _logoSprite = null;			// Gorgon logo sprite.
		private static TextSprite _logoStats = null;		// Statistics.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return if the device has been reset yet or not.
		/// </summary>
		internal bool DeviceNotReset
		{
			get
			{
				return _deviceWasLost;
			}
		}

		/// <summary>
		/// Property to return the D3D device object.
		/// </summary>
		internal D3D9.Device Device
		{
			get
			{
				return _device;
			}
		}

		/// <summary>
		/// Property to set or return flags that indicate what should be cleared per frame.
		/// </summary>
		public override ClearTargets ClearEachFrame
		{
			get
			{
				if (InheritClearFlags)
					return (Gorgon.ClearEachFrame | ClearTargets.BackBuffer) & ~ClearTargets.None;

				return base.ClearEachFrame;
			}
			set
			{
				// For the primary window only, always clear the back buffer.
				if ((value & ClearTargets.None) == ClearTargets.None)
					base.ClearEachFrame = ClearTargets.BackBuffer;
				else
					base.ClearEachFrame = value | ClearTargets.BackBuffer;
			}
		}

		/// <summary>
		/// Property to return the stored form settings.
		/// </summary>
		public WinSettings FormSettings
		{
			get
			{
				return _formSettings;
			}
		}

		/// <summary>
		/// Property to set or return whether this render window is full screen or not.
		/// </summary>
		public bool Windowed
		{
			get
			{
				return _windowedMode;
			}
			set
			{
				// Force windowed mode if we're on a control.
				if ((!(_owner is Form)) && (!value))
				{
					Gorgon.Log.Print("RenderWindow", "Cannot go to full screen mode with render target bound to a child control.", LoggingLevel.Verbose);
					_windowedMode = true;
					return;
				}
				else
				{
					// If we have additional render target windows, then don't set
					// full screen mode.
					foreach (RenderTarget target in RenderTargetCache.Targets)
					{
						if (target is RenderWindow)
						{
							Gorgon.Log.Print("RenderWindow", "Cannot go to full screen mode with additional render target windows present.", LoggingLevel.Verbose);
							_windowedMode = true;
							return;
						}
					}
				}

				// Turn off resize handler, it will be re-enabled in the reset.
				_allowResizeEvent = false;

				DeviceLost();

				// HACK: For some reason when the window is maximized and then eventually
				// set to full screen, the mouse cursor screws up and we can click right through
				// our window into whatever is behind it.  Setting it to normal size prior to
				// resetting it seems to work.
				if ((_owner is Form) && (_ownerForm.WindowState != FormWindowState.Normal))
					_ownerForm.WindowState = FormWindowState.Normal;
				_windowedMode = value;

				ResetMode(false, 0, 0);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to draw frame statistics.
		/// </summary>
		private void DrawStats()
		{
			Viewport lastClip = null;	// Last clipping view.
			BlendingModes lastBlend;	// Last blending mode.

			if (_logoStats == null)
				return;

			// Draw background.
			if (!Gorgon.InvertFrameStatsTextColor)
			{
				Gorgon.CurrentRenderTarget.BeginDrawing();
				lastBlend = Gorgon.CurrentRenderTarget.BlendingMode;
				Gorgon.CurrentRenderTarget.BlendingMode = BlendingModes.Modulated;
				Gorgon.CurrentRenderTarget.FilledRectangle(0, 0, Gorgon.CurrentRenderTarget.Width, 80, Drawing.Color.FromArgb(160, Drawing.Color.Black));
				Gorgon.CurrentRenderTarget.HorizontalLine(0, 80, Gorgon.CurrentRenderTarget.Width, Drawing.Color.White);
				Gorgon.CurrentRenderTarget.BlendingMode = lastBlend;
				Gorgon.CurrentRenderTarget.EndDrawing();
			}

			// Reset the clipping view.			
			if (Gorgon.CurrentClippingViewport != DefaultView)
			{
				lastClip = Gorgon.CurrentClippingViewport;
				Gorgon.CurrentClippingViewport = null;
			}

			if (_logoStats.Color != Gorgon.FrameStatsTextColor)
				_logoStats.Color = Gorgon.FrameStatsTextColor;
			if (Gorgon.InvertFrameStatsTextColor)
				_logoStats.BlendingMode = BlendingModes.Inverted;
			else
				_logoStats.BlendingMode = BlendingModes.Modulated;
			_logoStats.Text = "Frame draw time: " + Convert.ToSingle(Gorgon.FrameStats.FrameDrawTime).ToString("0.0") + "ms\n" +
								"Current FPS: " + Gorgon.FrameStats.CurrentFps.ToString("0.0") + "\n" +
								"Average FPS: " + Gorgon.FrameStats.AverageFps.ToString("0.0") + "\n" +
								"Highest FPS: " + Gorgon.FrameStats.HighestFps.ToString("0.0") + "\n" +
								"Lowest FPS: " + Gorgon.FrameStats.LowestFps.ToString("0.0") + "\n";
			_logoStats.Draw();

			// Reset the clipper to the previous value.
			if (lastClip != null)
				Gorgon.CurrentClippingViewport = lastClip;
		}

		/// <summary>
		/// Function to draw the logo.
		/// </summary>
		private void DrawLogo()
		{
			Viewport lastClip = null;	// Last clipping view.

			if ((_logoSprite == null) || (_logoImage == null) || (this != Gorgon.Screen))
				return;

			// Reset the clipping view.
			if (Gorgon.CurrentClippingViewport != DefaultView)
			{
				lastClip = Gorgon.CurrentClippingViewport;
				Gorgon.CurrentClippingViewport = DefaultView;
			}

			_logoSprite.SetPosition(Width - _logoSprite.Width, Height - _logoSprite.Height);
			_logoSprite.AlphaMaskFunction = CompareFunctions.Always;
			_logoSprite.BlendingMode = BlendingModes.Modulated;
			_logoSprite.Opacity = 225;
			_logoSprite.Draw();

			// Reset the clipper to the previous value.
			if (lastClip != null)
				Gorgon.CurrentClippingViewport = lastClip;
		}

		/// <summary>
		/// Function to save the settings for a form.
		/// </summary>
		private void SaveFormSettings()
		{
			_formSettings.Caption = _ownerForm.Text;
			_formSettings.ControlBox = _ownerForm.ControlBox;
			_formSettings.MaximizeBox = _ownerForm.MaximizeBox;
			_formSettings.MinimizeBox = _ownerForm.MinimizeBox;
			_formSettings.Style = _ownerForm.FormBorderStyle;
			_formSettings.TopMost = _ownerForm.TopMost;
			_formSettings.Left = _ownerForm.Left;
			_formSettings.Top = _ownerForm.Top;
			_formSettings.Width = _ownerForm.Width;
			_formSettings.Height = _ownerForm.Height;
		}

		/// <summary>
		/// Function to restore settings for an owner form.
		/// </summary>
		private void RestoreFormSettings()
		{
			_ownerForm.Text = _formSettings.Caption;
			_ownerForm.ControlBox = _formSettings.ControlBox;
			_ownerForm.MaximizeBox = _formSettings.MaximizeBox;
			_ownerForm.MinimizeBox = _formSettings.MinimizeBox;
			_ownerForm.FormBorderStyle = _formSettings.Style;
			_ownerForm.TopMost = _formSettings.TopMost;
			_ownerForm.Left = _formSettings.Left;
			_ownerForm.Top = _formSettings.Top;
			_ownerForm.Width = _formSettings.Width;
			_ownerForm.Height = _formSettings.Height;
		}

		/// <summary>
		/// Function to toggle windowed or fullscreen mode.
		/// </summary>
		/// <param name="resize">TRUE if we're resizing, FALSE if not.</param>
		/// <param name="resizewidth">New width if we're resizing.</param>
		/// <param name="resizeheight">New height if we're resizing.</param>
		private void ResetMode(bool resize, int resizewidth, int resizeheight)
		{
			if (_device == null)
				throw new DeviceCannotResetException();

			// Don't reset if the device is not in a lost state.
			_deviceWasLost = true;

			// Turn off resizing handler.
			_allowResizeEvent = false;

			// Do not perform a reset while minimized.
			Gorgon.Log.Print("RenderWindow", "Resetting device...", LoggingLevel.Verbose);

			// Force required resources to free.
			Gorgon.OnDeviceLost();

			try
			{
				// Do not change parameters while minimized.
				if (_ownerForm.WindowState != FormWindowState.Minimized)
					SetPresentParams(_requestedVideoMode, _windowedMode, resize, resizewidth, resizeheight, false);

				// Reset device.
				_device.Reset(_presentParameters);

				// Device was successfully reset.
				_deviceWasLost = false;

				// Get the current width & height.
				SetDimensions(_currentVideoMode.Width, _currentVideoMode.Height);

				// Get the color buffer.				
				SetColorBuffer(_device.GetRenderTarget(0));

				// Get the depth buffer.
				if (_presentParameters.AutoDepthStencilFormat != D3D9.Format.Unknown)
					SetDepthBuffer(_device.DepthStencilSurface);
				else
					SetDepthBuffer(null);

				// Set the proper window dressing if windowed.
				if ((_presentParameters.Windowed) && (_owner is Form) && (!resize))
				{
					_ownerForm.Hide();
					_ownerForm = Gorgon.Screen.OwnerForm;
					_ownerForm.MinimizeBox = _formSettings.MinimizeBox;
					_ownerForm.MaximizeBox = _formSettings.MaximizeBox;
					_ownerForm.ControlBox = _formSettings.ControlBox;
					_ownerForm.TopMost = _formSettings.TopMost;
					_ownerForm.FormBorderStyle = _formSettings.Style;
					_ownerForm.SetDesktopLocation((Gorgon.DesktopVideoMode.Width / 2) - (_ownerForm.Width / 2), (Gorgon.DesktopVideoMode.Height / 2) - (_ownerForm.Height / 2));
					_ownerForm.Show();
				}

				Gorgon.Log.Print("RenderWindow", "Video mode {0}x{1}x{2} ({4}) Refresh Rate: {3}Hz has been reset.", LoggingLevel.Verbose, Gorgon.Screen.Mode.Width, Gorgon.Screen.Mode.Height, Gorgon.Screen.Mode.Bpp, Gorgon.Screen.Mode.RefreshRate, Gorgon.Screen.Mode.Format.ToString());
				Gorgon.Log.Print("RenderWindow", "Device reset.", LoggingLevel.Verbose);

				Gorgon.Renderer.RenderStates.CheckForWBuffer(Converter.ConvertDepthFormat(_presentParameters.AutoDepthStencilFormat));

				// Reset render states to their previous values.
				Gorgon.Renderer.RenderStates.SetStates();

				// Set default image layer states.
				for (int i = 0; i < Gorgon.CurrentDriver.MaximumTextureStages; i++)
					Gorgon.Renderer.ImageLayerStates[i].SetStates();
			}
			catch (D3D9.Direct3D9Exception d3dEx)
			{
                if (d3dEx.ResultCode == D3D9.Error.DeviceLost)
                {
                    // If we lose the device for some reason, flag it so on the next frame we can check for
                    // device availability.
                    _deviceWasLost = true;
                }
                else
                    throw d3dEx;
			}
			catch (Exception ex)
			{
				throw new DeviceCannotResetException(ex);
			}

			if (!_deviceWasLost)
			{
				// Set the view matrix.
				_device.SetTransform(D3D9.TransformState.View, DX.Matrix.Identity);

				// Restore device state if reset was successful.
				Gorgon.OnDeviceReset();

				_allowResizeEvent = true;
			}
		}

		/// <summary>
		/// Function to perform a flip of the D3D buffers.
		/// </summary>
		protected internal override void D3DFlip()
		{
            _device.Present();
		}

		/// <summary>
		/// Function to handle a resize of the owner.
		/// </summary>
		/// <param name="sender">Sender of this event.</param>
		/// <param name="e">Event arguments.</param>
		protected override void OnOwnerResized(object sender, EventArgs e)
		{
			// Ensure we need to intercept this event.
			if (_allowResizeEvent)
			{
				if ((!Gorgon.FastResize) && (_device != null))
				{
                    DX.Result level;                // Cooperative level.

                    DX.Configuration.ThrowOnError = false;
                    level = Device.TestCooperativeLevel();
                    DX.Configuration.ThrowOnError = true;

					// Ensure we -can- reset.
					if ((level == D3D9.Error.DeviceNotReset) || (level.IsSuccess))
					{
						DeviceLost();
						ResetMode(true, _owner.ClientSize.Width, _owner.ClientSize.Height);
					}

				}

				Refresh();
			}
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				// Clean up the default font.
				if (_defaultFont != null)
					_defaultFont.Dispose();

				// Remove resizing handler.
				_owner.Resize -= new EventHandler(OnOwnerResized);

				// Destroy the D3D device object.
				if (_device != null)
					_device.Dispose();

				Gorgon.Log.Print("RenderWindow", "Direct 3D device destroyed.", LoggingLevel.Intermediate);
				_deviceWasLost = true;

				if (_owner is Form)
					RestoreFormSettings();
			}

			_defaultFont = null;
			_device = null;

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to set the presentation parameters for Direct3D.
		/// </summary>
		/// <param name="mode">Video mode to use.</param>
		/// <param name="usewindow">TRUE to use windowed mode, FALSE to use fullscreen.</param>
		/// <param name="resize">TRUE if we're resizing the window, FALSE if not.</param>
		/// <param name="resizewidth">If resize is TRUE, take video mode width from this.</param>
		/// <param name="resizeheight">If resize is TRUE, take video mode height from this.</param>
		/// <param name="preserveBackBuffer">TRUE to preserve the contents of the back buffer when the target is updated, FALSE to discard.</param>
		protected override void SetPresentParams(VideoMode mode, bool usewindow, bool resize, int resizewidth, int resizeheight, bool preserveBackBuffer)
		{
			// Make sure the desktop can be used for rendering.
			if ((usewindow) && (!Gorgon.CurrentDriver.DesktopFormatSupported(mode)))
				throw new DeviceVideoModeNotValidException(Gorgon.DesktopVideoMode);

			if (!resize)
			{
				if (!usewindow)
					Gorgon.Log.Print("RenderWindow", "Setting fullscreen mode {0}x{1}x{2} ({4}) Refresh Rate: {3}Hz on '{5}'.", LoggingLevel.Simple, mode.Width, mode.Height, mode.Bpp, mode.RefreshRate, mode.Format.ToString(), _objectName);
				else
					Gorgon.Log.Print("RenderWindow", "Setting windowed mode {0}x{1}x{2} ({4}) Refresh Rate: {3}Hz on '{5}'.", LoggingLevel.Simple, mode.Width, mode.Height, mode.Bpp, mode.RefreshRate, mode.Format.ToString(), _objectName);
			}
			else
				Gorgon.Log.Print("RenderWindow", "Resizing windowed mode {0}x{1}x{2} ({4}) Refresh Rate: {3}Hz on '{5}'.", LoggingLevel.Simple, resizewidth, resizeheight, mode.Bpp, mode.RefreshRate, mode.Format.ToString(), _objectName);

			// Don't go too small.
			if (resizewidth < 32)
				resizewidth = 32;
			if (resizeheight < 32)
				resizeheight = 32;

			if (mode.Width < 32)
				mode.Width = 32;
			if (mode.Height < 32)
				mode.Height = 32;

			_presentParameters.Windowed = usewindow;
			_presentParameters.DeviceWindowHandle = _owner.Handle;
			_presentParameters.PresentFlags = D3D9.PresentFlags.None;
			_presentParameters.Multisample = D3D9.MultisampleType.None;
			if (!preserveBackBuffer)
				_presentParameters.SwapEffect = D3D9.SwapEffect.Discard;
			else
				_presentParameters.SwapEffect = D3D9.SwapEffect.Copy;

			if (_owner is Form)
			{
				if (!usewindow)
				{
#if !DEBUG
				_ownerForm.TopMost = true;
#else
					_ownerForm.TopMost = false;
#endif
					_ownerForm.MinimizeBox = false;
					_ownerForm.MaximizeBox = false;
					_ownerForm.ControlBox = false;
					_ownerForm.FormBorderStyle = FormBorderStyle.None;

					// Make sure we can reach this video mode.
					_currentVideoMode = Gorgon.Drivers[Gorgon.CurrentDriver.DriverName].VideoModes[mode];
				}
				else
				{
					if (!resize)
					{
						_currentVideoMode.Width = mode.Width;
						_currentVideoMode.Height = mode.Height;
					}
					else
					{
						_currentVideoMode.Width = resizewidth;
						_currentVideoMode.Height = resizeheight;
					}
					_currentVideoMode.RefreshRate = Gorgon.DesktopVideoMode.RefreshRate;
					_currentVideoMode.Format = mode.Format;
				}

				// Resize the owner form.				
				_ownerForm.ClientSize = new Drawing.Size(_currentVideoMode.Width, _currentVideoMode.Height);
				_presentParameters.BackBufferWidth = _currentVideoMode.Width;
				_presentParameters.BackBufferHeight = _currentVideoMode.Height;
			}
			else
			{
				// Size to control boundaries.
				if (!resize)
				{
					_presentParameters.BackBufferWidth = _owner.ClientSize.Width;
					_presentParameters.BackBufferHeight = _owner.ClientSize.Height;
					_currentVideoMode.Width = _owner.ClientSize.Width;
					_currentVideoMode.Height = _owner.ClientSize.Height;
				}
				else
				{
					_presentParameters.BackBufferWidth = resizewidth;
					_presentParameters.BackBufferHeight = resizeheight;
					_currentVideoMode.Width = resizewidth;
					_currentVideoMode.Height = resizeheight;
				}
				_currentVideoMode.RefreshRate = Gorgon.DesktopVideoMode.RefreshRate;
				_currentVideoMode.Format = mode.Format;
			}

			if (_presentParameters.SwapEffect == D3D9.SwapEffect.Copy)
				_presentParameters.BackBufferCount = 1;
			else
				_presentParameters.BackBufferCount = 3;

			_presentParameters.BackBufferFormat = Converter.Convert(_currentVideoMode.Format);

			if (!usewindow)
			{
				// Set up presentation delays.
				if (_VSyncInterval != VSyncIntervals.IntervalNone)
				{
					Gorgon.Log.Print("RenderWindow", "VSync enabled ({0}).", LoggingLevel.Verbose, _VSyncInterval.ToString());
					_presentParameters.PresentationInterval = Converter.Convert(_VSyncInterval);
					_presentParameters.FullScreenRefreshRateInHertz = _currentVideoMode.RefreshRate;
				}
				else
				{
					Gorgon.Log.Print("RenderWindow", "VSync disabled.", LoggingLevel.Verbose);
					_presentParameters.PresentationInterval = Converter.Convert(VSyncIntervals.IntervalNone);
					_presentParameters.FullScreenRefreshRateInHertz = 0;
				}
			}
			else
			{
				// Default to immediate presentation for windowed mode.
				Gorgon.Log.Print("RenderWindow", "VSync is disabled in windowed mode.", LoggingLevel.Verbose);
				_presentParameters.PresentationInterval = D3D9.PresentInterval.Immediate;
				_presentParameters.FullScreenRefreshRateInHertz = 0;
			}

			UpdateDepthStencilFormat(UseStencilBuffer, UseDepthBuffer);
		}

		/// <summary>
		/// Function to set the video mode for the render window.
		/// </summary>
		/// <param name="mode">Mode to use.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to use full screen</param>
		/// <param name="usedepth">TRUE to use a depth buffer, FALSE to exclude.</param>
		/// <param name="usestencil">TRUE to use a stencil buffer, FALSE to exclude.</param>		
		/// <param name="dontCreate">TRUE to force the device to reset, rather than recreate.</param>
		/// <param name="vSyncInterval">V-sync interval for presentation.</param>
		internal void SetMode(VideoMode mode, bool windowed, bool usedepth, bool usestencil, bool dontCreate, VSyncIntervals vSyncInterval)
		{
			D3D9.CreateFlags flags;		// Flags used in device creation.

			// Reset objects.
			DeviceLost();

			// Show the form if it's hidden.
			if (!Gorgon.Screen.OwnerForm.Visible)
				Gorgon.Screen.OwnerForm.Visible = true;

			// Turn off resizing handler.
			_allowResizeEvent = false;
			SetDepthStencil(usedepth, usestencil);

			// Copy the requested video mode.
			_requestedVideoMode = mode;
			_windowedMode = windowed;

			_VSyncInterval = vSyncInterval;

			// If we don't support the requested vsync setting, then default to immediate.
			if ((_VSyncInterval & Gorgon.CurrentDriver.PresentIntervalSupport) == 0)
			{
				_VSyncInterval = VSyncIntervals.IntervalNone;
				Gorgon.Log.Print("RenderWindow", "Requested interval not available.", LoggingLevel.Verbose);
			}

			Gorgon.Log.Print("RenderWindow", "Requested vertical sync interval: {0}.  Final interval: {1}", LoggingLevel.Verbose, vSyncInterval.ToString(), _VSyncInterval.ToString());

			if (!_windowedMode)
			{
				if (!(_owner is Form))
				{
					Gorgon.Log.Print("RenderWindow", "Cannot go to full screen mode with primary render target bound to a child control.", LoggingLevel.Verbose);
					_windowedMode = true;
				}
				else
				{
					// If we have additional render target windows, then don't set
					// full screen mode.
					foreach (RenderTarget target in RenderTargetCache.Targets)
					{
						if (target is RenderWindow)
						{
							Gorgon.Log.Print("RenderWindow", "Cannot go to full screen mode with additional render target windows present.", LoggingLevel.Verbose);
							_windowedMode = true;
							break;
						}
					}
				}
			}

			// Get D3D presentation parameters.
			SetPresentParams(mode, _windowedMode, false, 0, 0, false);

			// We may need to force a reset.
			if (!dontCreate)
			{
				try
				{
					Gorgon.Log.Print("RenderWindow", "Creating Direct 3D device object...", LoggingLevel.Intermediate);

					// Center on screen.
					if (_presentParameters.Windowed)
						_ownerForm.SetDesktopLocation((Gorgon.DesktopVideoMode.Width / 2) - (_ownerForm.Width / 2), (Gorgon.DesktopVideoMode.Height / 2) - (_ownerForm.Height / 2));

					flags = D3D9.CreateFlags.FpuPreserve | D3D9.CreateFlags.Multithreaded;

					// Determine processing.
					if (Gorgon.CurrentDriver.HardwareTransformAndLighting)
					{
						flags |= D3D9.CreateFlags.HardwareVertexProcessing;
						Gorgon.Log.Print("RenderWindow", "Using hardware vertex processing.", LoggingLevel.Verbose);
					}
					else
					{
						flags |= D3D9.CreateFlags.SoftwareVertexProcessing;
						Gorgon.Log.Print("RenderWindow", "Using software vertex processing.", LoggingLevel.Verbose);
					}

					// Create the device.
					_device = new D3D9.Device(Gorgon.CurrentDriver.DriverIndex, Driver.DeviceType, _ownerForm.Handle, flags, _presentParameters);
					_deviceWasLost = false;

					// Confirm if a W-buffer exists or not.					
					Gorgon.Renderer.RenderStates.CheckForWBuffer(Converter.ConvertDepthFormat(_presentParameters.AutoDepthStencilFormat));
					// Set default states.
					Gorgon.Renderer.RenderStates.SetStates();

					// Set default image layer states.
					for (int i = 0; i < Gorgon.CurrentDriver.MaximumTextureStages; i++)
						Gorgon.Renderer.ImageLayerStates[i].SetStates();

					Gorgon.Log.Print("RenderWindow", "Direct 3D device object created.", LoggingLevel.Intermediate);
				}
				catch (Exception ex)
				{
					throw new DeviceCreationFailureException(ex);
				}

				_allowResizeEvent = true;
				SetDimensions(_currentVideoMode.Width, _currentVideoMode.Height);

				// Get the color buffer.
				SetColorBuffer(_device.GetRenderTarget(0));

				// Get the depth buffer.
				if (this._presentParameters.AutoDepthStencilFormat != D3D9.Format.Unknown)
					SetDepthBuffer(_device.DepthStencilSurface);
				else
					SetDepthBuffer(null);

				// Set the view matrix.
				_device.SetTransform(D3D9.TransformState.View, DX.Matrix.Identity);

				// Load logo data.
				if (_logoImage == null)
					_logoImage = Image.FromResource("GorgonLogo3", Properties.Resources.ResourceManager);
				if (_logoSprite == null)
					_logoSprite = new Sprite("@LogoSprite", _logoImage, 0, 0, 200, 56);
				if (_defaultFont == null)
					_defaultFont = new Font("@InternalFont", "Arial", 9.0f, true, true);
				if (_logoStats == null)
					_logoStats = new TextSprite("@LogoStats", string.Empty, _defaultFont, 0, 0, Drawing.Color.Chocolate);
			}
			else
			{
				ResetMode(true, mode.Width, mode.Height);
				DeviceReset();
			}

			Gorgon.Log.Print("RenderWindow", "Video mode {0}x{1}x{2} ({4}) Refresh Rate: {3}Hz has been set.", LoggingLevel.Simple, Width, Height, mode.Bpp, mode.RefreshRate, mode.Format.ToString());
		}

		/// <summary>
		/// Function to reset a device that's in a lost state.
		/// </summary>
		internal void ResetLostDevice()
		{
            DX.Result coopLevel;                    // Cooperative level.

			// Get the cooperative level.
            DX.Configuration.ThrowOnError = false;
            coopLevel = _device.TestCooperativeLevel();
            DX.Configuration.ThrowOnError = true;

			_deviceWasLost = false;
            if (coopLevel == D3D9.Error.DeviceNotReset)
            {
                _deviceWasLost = true;
                ResetMode(false, 0, 0);
            }
            if (coopLevel == D3D9.Error.DriverInternalError)
                throw new DeviceCannotResetException(true);
		}

		/// <summary>
		/// Function to render the scene for this target.
		/// </summary>
		public override void Update()
		{
			// Draw the logo.
			if (Gorgon.LogoVisible)
				DrawLogo();
			if (Gorgon.FrameStatsVisible)
				DrawStats();

			base.Update();
		}

		/// <summary>
		/// Function called when device is reset.
		/// </summary>
		public override void DeviceReset()
		{
			if (_device != null)
			{
				if (_logoImage != null)
					_logoImage.Copy(Properties.Resources.GorgonLogo3);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Control that will own this render target window.</param>
		internal PrimaryRenderWindow(Control owner)
			: base("@PrimaryRenderWindow", owner)
		{
			_formSettings = new WinSettings();
			_owner.Resize += new EventHandler(OnOwnerResized);
			SaveFormSettings();

			_deviceWasLost = true;
		}
		#endregion
	}
}
