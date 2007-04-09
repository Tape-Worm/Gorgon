#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Wednesday, August 03, 2005 12:52:59 AM
// 
#endregion

using System;
using Drawing = System.Drawing;
using System.Windows.Forms;
using SharpUtilities;
using SharpUtilities.Mathematics;
using SharpUtilities.Utility;
using DX = Microsoft.DirectX;
using D3D = Microsoft.DirectX.Direct3D;
using GorgonLibrary.Internal;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Fonts;

namespace GorgonLibrary
{
	/// <summary>
	/// A render window allows us to receive output.
	/// </summary>
	/// <remarks>
	/// This is where the final output of the rendering will be sent and displayed.  This can be a form or control.
	/// </remarks>
	public class RenderWindow 
		: RenderTarget, IDeviceStateObject
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
		private D3D.PresentParameters _presentParameters;	// Render target presentation parameters.
		private D3D.SwapChain _swapChain;				    // Swap chain.
		private D3D.Device _device;							// Device object.
		private WinSettings _formSettings;					// Form settings.		
		private bool _windowedMode;							// Flag to indicate whether we're windowed or not.		
		private VideoMode _requestedVideoMode;				// Video mode that we requested.		
		private VideoMode _currentVideoMode;				// Video mode for render window.		
		private Form _ownerForm;							// Form that owns the owning control.		
		private bool _backBuffer;							// Flag to indicate that this render window is the main back buffer.		
		private bool _deviceWasLost;						// Flag indicating the device is in a lost state.
		private Control _owner;								// Owning control of this render window.
		private bool _backBufferPreserved;					// Flag to indicate whether we'll preserve the backbuffer or not.
		private VSyncIntervals _VSyncInterval;				// Vertical sync intervals.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the device type.
		/// </summary>
		internal static D3D.DeviceType DeviceType
		{
			get
			{
#if INCLUDE_D3DREF
				return Gorgon.UseReferenceDevice ? D3D.DeviceType.Reference : D3D.DeviceType.Hardware;
#else
				return D3D.DeviceType.Hardware;
#endif
			}
		}

		/// <summary>
		/// Property to set or return if the device has been reset yet or not.
		/// </summary>
		internal bool DeviceNotReset
		{
			get
			{
				return _deviceWasLost;
			}
			set
			{
				_deviceWasLost = value;
			}
		}

		/// <summary>
		/// Property to return the D3D device object.
		/// </summary>
		internal D3D.Device Device
		{
			get
			{
				return _device;
			}
		}

		/// <summary>
		/// Property to return the swap chain.
		/// </summary>
		internal D3D.SwapChain SwapChain
		{
			get
			{
				return _swapChain;
			}
		}

		/// <summary>
		/// Property to return the requested video mode.
		/// </summary>
		internal VideoMode RequestedVideoMode
		{
			get
			{
				return _requestedVideoMode;
			}
		}

		/// <summary>
		/// Property to return the presentation interval.
		/// </summary>
		public VSyncIntervals VSyncInterval
		{
			get
			{
				return _VSyncInterval;
			}
		}

		/// <summary>
		/// Property to return whether the device has been put into a lost state.
		/// </summary>
		public bool DeviceIsInLostState
		{
			get
			{
				// If already lost, return.
				if (!_deviceWasLost)
					return false;

				// Check for device loss.
				return Gorgon.Renderer.DeviceIsLost;
			}
		}

		/// <summary>
		/// Property to return the format of the depth buffer.
		/// </summary>
		public DepthBufferFormats DepthBufferFormat
		{
			get
			{
				return Converter.Convert(_presentParameters.AutoDepthStencilFormat);
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
		/// Property to return the owning control of this swap chain.
		/// </summary>
		public Control Owner
		{
			get
			{
				return _owner;
			}
		}

		/// <summary>
		/// Property to return the owning form of the owning control.
		/// </summary>
		public Form OwnerForm
		{
			get
			{
				return _ownerForm;
			}
		}

		/// <summary>
		/// Property to return whether the swap chain has a depth buffer attached or not.
		/// </summary>
		public override bool UseDepthBuffer
		{
			get
			{
				return _useDepthBuffer;
			}
			set
			{
				throw new NotImplementedException(NotImplementedTypes.Property, "Set_UseDepthBuffer", null);
			}
		}

		/// <summary>
		/// Property to return whether the swap chain has a stencil buffer attached or not.
		/// </summary>
		public override bool UseStencilBuffer
		{
			get
			{
				return _useStencilBuffer;
			}
			set
			{
				throw new NotImplementedException(NotImplementedTypes.Property, "Set_UseStencilBuffer", null);
			}
		}

		/// <summary>
		/// Property to return the currently set video mode.
		/// </summary>
		public VideoMode Mode
		{
			get
			{
				return _currentVideoMode;
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
				if (_backBuffer)
				{
					// Force windowed mode if we're on a control.
					if ((!(_owner is Form)) && (!value))
					{
						Gorgon.Log.Print("RenderWindow", "Cannot go to full screen mode with render target bound to a child control.", LoggingLevel.Verbose);
						value = true;
					}
					else
					{
						// If we have additional render target windows, then don't set
						// full screen mode.
						foreach (RenderTarget target in Gorgon.RenderTargets)
						{
							if ((target is RenderWindow) && (target != this))
							{
								Gorgon.Log.Print("RenderWindow", "Cannot go to full screen mode with additional render target windows present.", LoggingLevel.Verbose);
								value = true;
								break;
							}
						}
					}

					// Turn off resize handler, it will be re-enabled in the reset.
					_owner.Resize -= new EventHandler(OnOwnerResized);

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
				else
					_windowedMode = true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to handle a resize of the owner.
		/// </summary>
		/// <param name="sender">Sender of this event.</param>
		/// <param name="e">Event arguments.</param>
		private void OnOwnerResized(object sender, EventArgs e)
		{
			// If we're not the primary backbuffer, then just destroy and recreate.
			if (_ownerForm.WindowState != FormWindowState.Minimized)
			{
				if ((!Gorgon.FastResize) || (!_backBuffer))
					DeviceLost();
				if (!_backBuffer)
					DeviceReset();
				else
				{
					if (!Gorgon.FastResize)
						ResetMode(true, _owner.ClientSize.Width, _owner.ClientSize.Height);
				}
			}
			
			// Update the render windows.
			Refresh();
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
		/// Function to determine if we can convert between a specified format and the desktop format.
		/// </summary>
		/// <param name="driverindex">Driver to use.</param>
		/// <param name="sourceformat">Format to convert from.</param>
		/// <returns>TRUE if supported, FALSE if not.</returns>
		private bool ValidDesktopFormat(int driverindex, D3D.Format sourceformat)
		{
			return D3D.Manager.CheckDeviceFormatConversion(driverindex, DeviceType, sourceformat, Converter.Convert(Gorgon.DesktopVideoMode.Format));
		}

		/// <summary>
		/// Function to return a viable stencil/depth buffer.
		/// </summary>
		/// <param name="usestencil">TRUE to use a stencil, FALSE to exclude.</param>
		/// <param name="usedepth">TRUE to use a depth buffer, FALSE to exclude.</param>
		protected override void UpdateDepthStencilFormat(bool usestencil, bool usedepth)
		{
			int i;		// Loop.						

			// List of depth/stencil formats.
			D3D.DepthFormat[][] dsFormats = new D3D.DepthFormat[][] {new D3D.DepthFormat[] {D3D.DepthFormat.D24SingleS8,D3D.DepthFormat.D24S8,D3D.DepthFormat.D24X4S4,D3D.DepthFormat.D15S1,D3D.DepthFormat.L16},
																		new D3D.DepthFormat[] {D3D.DepthFormat.D32,D3D.DepthFormat.D24X8,D3D.DepthFormat.D16}};

			_useStencilBuffer = false;
			_useDepthBuffer = false;
			_presentParameters.EnableAutoDepthStencil = false;
			_presentParameters.AutoDepthStencilFormat = D3D.DepthFormat.Unknown;

			// Force the window to be shown.
			if (!_owner.Visible)
				_owner.Visible = true;

			// Deny the stencil buffer if it's not supported.
			if ((!Gorgon.Driver.SupportStencil) && (usestencil))
			{
				usestencil = false;
				Gorgon.Log.Print("RenderWindow", "Stencil buffer was requested, but the driver doesn't support it.", LoggingLevel.Verbose);
			}

			// Do nothing if we don't want either.
			if ((!usestencil) && (!usedepth))
				return;

			if ((usestencil) && (usedepth))
			{
				// If we want a stencil buffer, find an appropriate format.
				for (i = 0; i < dsFormats[0].Length; i++)
				{
					if ((!_presentParameters.EnableAutoDepthStencil) && (CheckBackBuffer(Gorgon.Driver.DriverIndex, _presentParameters.BackBufferFormat, dsFormats[0][i])))
					{
						_presentParameters.EnableAutoDepthStencil = true;
						_presentParameters.AutoDepthStencilFormat = dsFormats[0][i];
						_useStencilBuffer = true;
						_useDepthBuffer = true;
						Gorgon.Log.Print("RenderWindow", "Stencil and depth buffer requested and found.  Using stencil buffer. ({0})", LoggingLevel.Verbose, Converter.Convert(dsFormats[0][i]).ToString());
						break;
					}
				}
			}

			// If we haven't set this yet, then we haven't selected a buffer format yet.
			if ((!_presentParameters.EnableAutoDepthStencil) && (usedepth))
			{
				// Find depth buffers without a stencil buffer.
				for (i = 0; i < dsFormats[1].Length; i++)
				{
					if ((!_presentParameters.EnableAutoDepthStencil) && (CheckBackBuffer(Gorgon.Driver.DriverIndex, _presentParameters.BackBufferFormat, dsFormats[1][i])))
					{
						_presentParameters.EnableAutoDepthStencil = true;
						_presentParameters.AutoDepthStencilFormat = dsFormats[1][i];
						_useDepthBuffer = true;
						Gorgon.Log.Print("RenderWindow", "Stencil buffer not requested or found.  Using depth buffer. ({0}).", LoggingLevel.Verbose, Converter.Convert(dsFormats[1][i]).ToString());
						break;
					}
				}
			}

			if (!_presentParameters.EnableAutoDepthStencil)
				Gorgon.Log.Print("RenderWindow", "No acceptable depth/stencil buffer found or requested.  Driver may use alternate form of HSR.", LoggingLevel.Verbose);
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
		private void SetPresentParams(VideoMode mode, bool usewindow, bool resize, int resizewidth, int resizeheight, bool preserveBackBuffer)
		{
			// Make sure the desktop can be used for rendering.
			if (usewindow)
			{
				if ((!ValidFormat(Gorgon.Driver.DriverIndex, Converter.Convert(Gorgon.DesktopVideoMode.Format), Converter.Convert(mode.Format), true)) || (!ValidDesktopFormat(Gorgon.Driver.DriverIndex, Converter.Convert(mode.Format))))
					throw new InvalidVideoModeException(Gorgon.DesktopVideoMode, null);
			}

			if (_backBuffer)
			{
				if (!resize)
				{
					if (!usewindow)
						Gorgon.Log.Print("RenderWindow", "Setting fullscreen mode {0}x{1}x{2} ({4}) Refresh Rate: {3}Hz on '{5}'.", LoggingLevel.Simple, mode.Width, mode.Height, mode.Bpp, mode.RefreshRate, mode.Format.ToString(), _objectName);
					else
						Gorgon.Log.Print("RenderWindow", "Setting windowed mode {0}x{1}x{2} ({4}) Refresh Rate: {3}Hz on '{5}'.", LoggingLevel.Simple, mode.Width, mode.Height, mode.Bpp, mode.RefreshRate, mode.Format.ToString(), _objectName);
				}
				else
					Gorgon.Log.Print("RenderWindow", "Resizing windowed mode {0}x{1}x{2} ({4}) Refresh Rate: {3}Hz on '{5}'.", LoggingLevel.Simple, resizewidth, resizeheight, mode.Bpp, mode.RefreshRate, mode.Format.ToString(), _objectName);
			}
			else
			{
				if (!resize)
					Gorgon.Log.Print("RenderWindow", "Setting swap chain to {0}x{1}x{2} ({3}) on '{4}'.", LoggingLevel.Intermediate, mode.Width, mode.Height, mode.Bpp, mode.Format.ToString(), _objectName);
				else
					Gorgon.Log.Print("RenderWindow", "Resizing swap chain to {0}x{1}x{2} ({3}) on '{4}'.", LoggingLevel.Intermediate, resizewidth, resizeheight, mode.Bpp, mode.Format.ToString(), _objectName);
			}

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
			_presentParameters.PresentFlag = D3D.PresentFlag.None;
			_presentParameters.MultiSample = D3D.MultiSampleType.None;
			if (!preserveBackBuffer)
				_presentParameters.SwapEffect = D3D.SwapEffect.Discard;
			else
				_presentParameters.SwapEffect = D3D.SwapEffect.Copy;
			_presentParameters.ForceNoMultiThreadedFlag = false;

			if (_owner is Form)
			{
				if (!usewindow)
				{
#if !DEBUG
					_ownerForm.TopMost = true;
#endif
					_ownerForm.TopMost = false;
					_ownerForm.MinimizeBox = false;
					_ownerForm.MaximizeBox = false;
					_ownerForm.ControlBox = false;
					_ownerForm.FormBorderStyle = FormBorderStyle.None;

					// Make sure we can reach this video mode.
					_currentVideoMode = Gorgon.Drivers[Gorgon.Driver.DriverName].VideoModes[mode];
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

			if (_presentParameters.SwapEffect == D3D.SwapEffect.Copy)
				_presentParameters.BackBufferCount = 1;
			else
				_presentParameters.BackBufferCount = 3;

			_presentParameters.BackBufferFormat = Converter.Convert(_currentVideoMode.Format);

			if (!usewindow)
			{
				// Set up presentation delays.
				if (_backBuffer)
				{
					if (_VSyncInterval != VSyncIntervals.IntervalNone)
					{
						Gorgon.Log.Print("RenderWindow", "VSync enabled ({0}).", LoggingLevel.Verbose, _VSyncInterval.ToString());
						_presentParameters.PresentationInterval = Converter.Convert(_VSyncInterval);
						_presentParameters.FullScreenRefreshRateInHz = _currentVideoMode.RefreshRate;
					}
					else
					{
						Gorgon.Log.Print("RenderWindow", "VSync disabled.", LoggingLevel.Verbose);
						_presentParameters.PresentationInterval = Converter.Convert(VSyncIntervals.IntervalNone);
						_presentParameters.FullScreenRefreshRateInHz = 0;
					}
				}
				
			}
			else
			{
				// Default to immediate presentation for windowed mode.
				if (_backBuffer)
					Gorgon.Log.Print("RenderWindow", "VSync is disabled in windowed mode.", LoggingLevel.Verbose);
				_presentParameters.PresentationInterval = D3D.PresentInterval.Immediate;
				_presentParameters.FullScreenRefreshRateInHz = 0;
			}

			UpdateDepthStencilFormat(_useStencilBuffer, _useDepthBuffer);
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
			D3D.CreateFlags flags;		// Flags used in device creation.

			// Reset objects.
			DeviceLost();

			// Turn off resizing handler.
			_owner.Resize -= new EventHandler(OnOwnerResized);
			_useStencilBuffer = usestencil;
			_useDepthBuffer = usedepth;

			// Copy the requested video mode.
			_requestedVideoMode = mode;
			_windowedMode = windowed;

			_VSyncInterval = vSyncInterval;

			// If we don't support the requested vsync setting, then default to immediate.
			if ((_VSyncInterval & Gorgon.Driver.PresentIntervalSupport) == 0)
			{
				_VSyncInterval = VSyncIntervals.IntervalNone;
				Gorgon.Log.Print("RenderWindow", "Requested interval not available.", LoggingLevel.Verbose);
			}

			Gorgon.Log.Print("RenderWindow", "Requested vertical sync interval: {0}.  Final interval: {1}", LoggingLevel.Verbose, vSyncInterval.ToString(), _VSyncInterval.ToString());

			if ((_backBuffer) && (!_windowedMode))
			{
				if (!(_owner is Form))
				{
					Gorgon.Log.Print("RenderWindow", "Cannot go to full screen mode with render target bound to a child control.", LoggingLevel.Verbose);
					_windowedMode = true;
				}
				else
				{
					// If we have additional render target windows, then don't set
					// full screen mode.
					foreach (RenderTarget target in Gorgon.RenderTargets)
					{
						if ((target is RenderWindow) && (target != this))
						{
							Gorgon.Log.Print("RenderWindow", "Cannot go to full screen mode with additional render target windows present.", LoggingLevel.Verbose);
							_windowedMode = true;
							break;
						}
					}
				}
			}

			// Get D3D presentation parameters.
			SetPresentParams(mode,_windowedMode,false,0,0, _backBufferPreserved);
			if (!dontCreate)
			{
				try
				{
					Gorgon.Log.Print("RenderWindow", "Creating Direct 3D device object...", LoggingLevel.Intermediate);

					// Center on screen.
					if (_presentParameters.Windowed)
						_ownerForm.SetDesktopLocation((Gorgon.DesktopVideoMode.Width / 2) - (_ownerForm.Width / 2), (Gorgon.DesktopVideoMode.Height / 2) - (_ownerForm.Height / 2));

					flags = D3D.CreateFlags.FpuPreserve;

					// Determine processing.
					if (Gorgon.Driver.TnL)
					{
						flags |= D3D.CreateFlags.HardwareVertexProcessing;
						Gorgon.Log.Print("RenderWindow", "Using hardware vertex processing.", LoggingLevel.Verbose);
					}
					else
					{
						flags |= D3D.CreateFlags.SoftwareVertexProcessing;
						Gorgon.Log.Print("RenderWindow", "Using software vertex processing.", LoggingLevel.Verbose);
					}

					// Create the device.
					_device = new D3D.Device(Gorgon.Driver.DriverIndex, DeviceType, _ownerForm.Handle, flags, _presentParameters);
					_deviceWasLost = false;

					// Confirm if a W-buffer exists or not.
					Gorgon.Renderer.RenderStates.CheckForWBuffer(Converter.Convert(_presentParameters.AutoDepthStencilFormat));
					// Set default states.
					Gorgon.Renderer.RenderStates.SetStates();
					// Set default image layer states.
					for (int i = 0;i < Gorgon.Driver.MaximumTextureStages;i++)
						Gorgon.Renderer.ImageLayerStates[i].SetStates();

					Gorgon.Log.Print("RenderWindow", "Direct 3D device object created.", LoggingLevel.Intermediate);
				}
				catch (Exception e)
				{
					throw new CannotCreateDeviceException(e);
				}

				_owner.Resize += new EventHandler(OnOwnerResized);
				_width = _currentVideoMode.Width;
				_height = _currentVideoMode.Height;

				_defaultView.Refresh();
				// Get the color buffer.
				_colorBuffer = _device.GetRenderTarget(0);
				// Get the depth buffer.
				if (this._presentParameters.AutoDepthStencilFormat != D3D.DepthFormat.Unknown)
					_depthBuffer = _device.DepthStencilSurface;
				else
					_depthBuffer = null;

				// Set the view matrix.
				_device.Transform.View = DX.Matrix.Identity;

				// Update windows.
				Refresh();

				// Load logo data.
				if (_logoImage == null)
					_logoImage = Gorgon.ImageManager.FromResource("GorgonLogo3", Properties.Resources.ResourceManager);
				if (_logoSprite == null)
					_logoSprite = new Sprite("@LogoSprite", _logoImage, 0, 0, 200, 56);
				if (_logoStats == null)
					_logoStats = new TextSprite("@LogoStats", string.Empty, FontManager.DefaultFont, 0, 0, Drawing.Color.Chocolate);
			}
			else
			{
				ResetMode(true, mode.Width, mode.Height);
				DeviceReset();

				// Set the view matrix.
				if (_device != null)
					_device.Transform.View = DX.Matrix.Identity;
			}

			Gorgon.Log.Print("RenderWindow", "Video mode {0}x{1}x{2} ({4}) Refresh Rate: {3}Hz has been set.", LoggingLevel.Simple, _width, _height, mode.Bpp, mode.RefreshRate, mode.Format.ToString());
		}

		/// <summary>
		/// Function to create a swap chain render target.
		/// </summary>
		/// <param name="mode">Mode to use.</param>
		/// <param name="usedepth">TRUE to use a depth buffer, FALSE to exclude.</param>
		/// <param name="usestencil">TRUE to use a stencil buffer, FALSE to exclude.</param>
		/// <param name="resize">TRUE if resizing, FALSE if not.</param>
		/// <param name="preserveBackBuffer">TRUE to preserve the contents of the back buffer when the target is updated, FALSE to discard.</param>
		internal void CreateSwapChain(VideoMode mode,bool usedepth,bool usestencil,bool resize, bool preserveBackBuffer)
		{
			// Ensure we have the device.
			if (Gorgon.Renderer.DeviceIsLost)
				return;

			_owner.Resize -= new EventHandler(OnOwnerResized);

			_backBufferPreserved = preserveBackBuffer;
			_requestedVideoMode = mode;
			// Always windowed.
			_windowedMode = true;
			_useStencilBuffer = usestencil;
			_useDepthBuffer = usedepth;

			SetPresentParams(mode, true, resize, _owner.ClientSize.Width, _owner.ClientSize.Height, preserveBackBuffer);

			// Try to create the swap chain.
			try
			{
				_swapChain = new D3D.SwapChain(Gorgon.Screen.Device, _presentParameters);
				Gorgon.Log.Print("RenderWindow", "Swap chain created.", LoggingLevel.Verbose);

				// Get the color buffer.
				_colorBuffer = _swapChain.GetBackBuffer(0, D3D.BackBufferType.Mono);

				// If we have a Z Buffer, then try to create it.
				if (_presentParameters.EnableAutoDepthStencil)
				{
					_depthBuffer = Gorgon.Screen.Device.CreateDepthStencilSurface(_currentVideoMode.Width, _currentVideoMode.Height, _presentParameters.AutoDepthStencilFormat, D3D.MultiSampleType.None, 0, false);
					Gorgon.Log.Print("RenderWindow", "Depth buffer created.", LoggingLevel.Verbose);
				}
				else
					_depthBuffer = null;

				// Update width and height to reflect the backbuffer dimensions.
				_width = _currentVideoMode.Width;
				_height = _currentVideoMode.Height;

				// Update windows.
				Refresh();

				_owner.Resize += new EventHandler(OnOwnerResized);

				Gorgon.Log.Print("RenderWindow", "Swap chain mode {0}x{1}x{2} ({3}) has been set.", LoggingLevel.Simple, _width, _height, mode.Bpp, mode.Format.ToString());
			}
			catch (Exception e)
			{
				throw new CannotCreateRenderTargetException(e);
			}
		}

		/// <summary>
		/// Function to toggle windowed or fullscreen mode.
		/// </summary>
		/// <param name="resize">TRUE if we're resizing, FALSE if not.</param>
		/// <param name="resizewidth">New width if we're resizing.</param>
		/// <param name="resizeheight">New height if we're resizing.</param>
		internal void ResetMode(bool resize, int resizewidth, int resizeheight)
		{
			// Only allow this on back buffers.
			if (!_backBuffer)
				return;

			if (_device == null)
				throw new CannotResetDeviceException("Video mode was not initially set, call SetMode() first.", null);

			_deviceWasLost = true;

			// Turn off resizing handler.
			_owner.Resize -= new EventHandler(OnOwnerResized);
			SetPresentParams(_requestedVideoMode, _windowedMode, resize, resizewidth, resizeheight, _backBufferPreserved);
			try
			{
				Gorgon.Log.Print("RenderWindow", "Resetting device...", LoggingLevel.Verbose);

				// Update the projection matrix.
				_projectionChanged = true;

				// Force required resources to free.
				Gorgon.DeviceLost();
				Gorgon.Renderer.DeviceLost();
				_device.Reset(_presentParameters);

				// Restore devices if reset was successful.
				Gorgon.Renderer.DeviceReset();
				_deviceWasLost = false;

				// Set the proper window dressing if windowed.
				if ((_presentParameters.Windowed) && (_owner is Form))
				{
					_ownerForm = Gorgon.Screen.OwnerForm;
					_ownerForm.MinimizeBox = Gorgon.Screen.FormSettings.MinimizeBox;
					_ownerForm.MaximizeBox = Gorgon.Screen.FormSettings.MaximizeBox;
					_ownerForm.ControlBox = Gorgon.Screen.FormSettings.ControlBox;
					_ownerForm.TopMost = Gorgon.Screen.FormSettings.TopMost;
					_ownerForm.FormBorderStyle = Gorgon.Screen.FormSettings.Style;
					if (!resize)
						_ownerForm.SetDesktopLocation((Gorgon.DesktopVideoMode.Width / 2) - (_ownerForm.Width / 2), (Gorgon.DesktopVideoMode.Height / 2) - (_ownerForm.Height / 2));
				}

				Gorgon.Log.Print("RenderWindow", "Video mode {0}x{1}x{2} ({4}) Refresh Rate: {3}Hz has been reset.", LoggingLevel.Verbose, Gorgon.Screen.Mode.Width, Gorgon.Screen.Mode.Height, Gorgon.Screen.Mode.Bpp, Gorgon.Screen.Mode.RefreshRate, Gorgon.Screen.Mode.Format.ToString());
				Gorgon.Log.Print("RenderWindow", "Device reset.", LoggingLevel.Verbose);

				Gorgon.Renderer.RenderStates.CheckForWBuffer(Converter.Convert(_presentParameters.AutoDepthStencilFormat));

				// Reset render states to their previous values.
				Gorgon.Renderer.RenderStates.SetStates();
				// Set default image layer states.
				for (int i = 0; i < Gorgon.Driver.MaximumTextureStages; i++)
					Gorgon.Renderer.ImageLayerStates[i].SetStates();
			}
			catch (D3D.DeviceLostException)
			{
				// If we lose the device for some reason, flag it so on the next frame we can check for
				// device availability.
				_deviceWasLost = true;
			}
			catch (Exception e)
			{
				throw new CannotResetDeviceException(e);
			}

			if (!_deviceWasLost)
			{
				_owner.Resize += new EventHandler(OnOwnerResized);
				_width = _currentVideoMode.Width;
				_height = _currentVideoMode.Height;
				_defaultView.Refresh();

				// Get the color buffer.
				_colorBuffer = _device.GetRenderTarget(0);
				// Get the depth buffer.
				if (_presentParameters.AutoDepthStencilFormat != D3D.DepthFormat.Unknown)
					_depthBuffer = _device.DepthStencilSurface;

				Gorgon.Renderer.SetRenderTarget(this);

				// Update windows.
				Refresh();

				// Reset the device.
				Gorgon.DeviceReset();
			}
		}

		
		/// <summary>
		/// Function to return whether this format can be used for hardware accelerated rendering.
		/// </summary>
		/// <param name="driverindex">Index of the driver we're using.</param>
		/// <param name="display">Format of the display to check.</param>
		/// <param name="backbuffer">Back buffer format.</param>
		/// <param name="windowed">TRUE if we're going to be windowed, FALSE if not.</param>
		/// <returns>TRUE if this format can be used, FALSE if not.</returns>
		internal bool ValidFormat(int driverindex, D3D.Format display, D3D.Format backbuffer, bool windowed)
		{
			return D3D.Manager.CheckDeviceType(driverindex, DeviceType, display, backbuffer, windowed);
		}

		/// <summary>
		/// Function to render the scene for this target.
		/// </summary>
		public override void Update()
		{
			base.Update();

			// Flip to front.
			Gorgon.Renderer.Flip();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <remarks>
		/// This constructor will create a new swap chain or set the primary video mode if not set.  
		/// Creating a swap chain will only work in windowed mode, if you are not in windowed mode then an 
		/// exception will be thrown.
		/// </remarks>
		/// <param name="name">Name of this swap chain.</param>
		/// <param name="owner">Owning window of this render target.</param>
		/// <param name="priority">Priority of the render target.</param>
		internal RenderWindow(string name,Control owner, int priority) : base(name,0,0,priority)
		{
			Control parent = null;		// Parent of the owning control.

			Gorgon.Log.Print("RenderWindow", "Creating rendering window '{0}' ...", LoggingLevel.Intermediate, name);

			_owner = owner;
			_backBufferPreserved = false;

			// Make sure we're in windowed mode if we've already set the primary window.
			if ((Gorgon.Screen != null) && (!Gorgon.Screen.Windowed))
				throw new NotLegalInFullScreenException(null);

			// Create presentation parameters.
			_presentParameters = new D3D.PresentParameters();

			// Determine the parent form.
			if (_owner is Form) 
				_ownerForm = (Form)_owner;
			else
			{
				parent = _owner.Parent;
				// Recurse until we find a form.
				while ((parent != null) && (!(parent is Form)))
					parent = parent.Parent;

				if (parent == null)
					throw new InvalidParentFormException(null);

				_ownerForm = (Form)parent;
			}

			_formSettings = new WinSettings();
			SaveFormSettings();

			_useStencilBuffer = false;
			_useDepthBuffer = false;
			_windowedMode = false;
			_deviceWasLost = true;

			// If there's no device created, then we need to create one.
			if (Gorgon.Screen == null)
			{
				_backBuffer = true;
				Gorgon.Log.Print("RenderWindow", "This window is the primary rendering window.", LoggingLevel.Verbose);
			}
			else
				_backBuffer = false;

			// Get settings from current desktop video mode.
			_requestedVideoMode = Gorgon.DesktopVideoMode;
			_currentVideoMode = Gorgon.DesktopVideoMode;

			Gorgon.Log.Print("RenderWindow", "Bound to control: {0} (0x{1:x8}) of type {2}.  Parent form: {3} (0x{4:x8})", LoggingLevel.Verbose, owner.Name, owner.Handle.ToInt32(), owner.GetType().ToString(), _ownerForm.Name, _ownerForm.Handle.ToInt32().ToString("x").PadLeft(8, '0'));
			Gorgon.Log.Print("RenderWindow", "Rendering window '{0}' created.", LoggingLevel.Intermediate, name);
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)			
			{
				Gorgon.Log.Print("RenderWindow", "Destroying render window '{0}'...", LoggingLevel.Intermediate, _objectName);

				// Remove resizing handler.
				_owner.Resize -= new EventHandler(OnOwnerResized);

				// Remove buffer hooks.
				if (_colorBuffer != null)
				{
					_colorBuffer.Dispose();
					Gorgon.Log.Print("RenderWindow", "Color buffer destroyed.", LoggingLevel.Verbose);
				}

				if (_depthBuffer != null)
				{
					_depthBuffer.Dispose();
					Gorgon.Log.Print("RenderWindow", "Depth buffer destroyed.", LoggingLevel.Verbose);
				}

				if (_backBuffer)
				{
					// Destroy the D3D device object.
					if (_device != null)					
						_device.Dispose();						
					
					_device = null;
					Gorgon.Log.Print("RenderWindow", "Direct 3D device destroyed.", LoggingLevel.Intermediate);
					_deviceWasLost = true;
				}				
				else
				{
					if (_swapChain != null)
					{
						_swapChain.Dispose();
						Gorgon.Log.Print("RenderWindow", "Swap chain destroyed.", LoggingLevel.Verbose);
					}
				}

				// Turn off the active render target.
				Gorgon.Renderer.SetRenderTarget(null);

				if (_owner is Form)
					RestoreFormSettings();				

				Gorgon.Log.Print("RenderWindow", "Render window '{0}' destroyed.", LoggingLevel.Intermediate, _objectName);
			}			

			// Do unmanaged clean up.
			_colorBuffer = null;
			_depthBuffer = null;
			_swapChain = null;			
		}
		#endregion

		#region IDeviceStateObject Members
		/// <summary>
		/// Function called when device is lost.
		/// </summary>
		public override void DeviceLost()
		{
			Gorgon.Renderer.SetRenderTarget(null);

			if (_colorBuffer != null)
				_colorBuffer.Dispose();

			if (_depthBuffer != null)
				_depthBuffer.Dispose();

			_depthBuffer = null;
			_colorBuffer = null;

			if ((!_backBuffer) && (_swapChain != null))
			{
				_swapChain.Dispose();
				_swapChain = null;
			}
		}

		/// <summary>
		/// Function to force the loss of data associated with this object.
		/// </summary>
		public override void ForceRelease()
		{
			DeviceLost();
		}

		/// <summary>
		/// Function called when device is reset.
		/// </summary>
		public override void DeviceReset()
		{
			if (!_backBuffer)
				CreateSwapChain(_requestedVideoMode,_useDepthBuffer,_useStencilBuffer,true, _backBufferPreserved);

			if (_logoImage != null)
				_logoImage.Copy(Properties.Resources.GorgonLogo3);
		}
		#endregion
	}
}
