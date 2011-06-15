#region MIT.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Wednesday, August 03, 2005 12:52:59 AM
// 
#endregion

using System;
using Drawing = System.Drawing;
using System.Windows.Forms;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Internal;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Graphics
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
		#region Variables.
		/// <summary>Render target presentation parameters.</summary>
		protected D3D9.PresentParameters _presentParameters;
		/// <summary>Form that owns the owning control.</summary>
		protected Form _ownerForm = null;
		/// <summary>Video mode that we requested.</summary>
		protected VideoMode _requestedVideoMode;
		/// <summary>Video mode for render window.</summary>		
		protected VideoMode _currentVideoMode;
		/// <summary>Owning control of this render window.</summary>
		protected Control _owner;
		/// <summary>Vertical sync intervals.</summary>
		protected VSyncIntervals _VSyncInterval;
		
		private D3D9.SwapChain _swapChain;				    // Swap chain.		
		private bool _backBufferPreserved;					// Flag to indicate whether we'll preserve the backbuffer or not.		
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the swap chain.
		/// </summary>
		internal D3D9.SwapChain SwapChain
		{
			get
			{
				return _swapChain;
			}
		}

		/// <summary>
		/// Property to return whether this render target is valid for post pixel shader blending.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// If the driver supports post pixel shader blending of render targets (<see cref="GorgonLibrary.Driver.SupportMRTPostPixelShaderBlending">Driver.SupportMRTPostPixelShaderBlending</see> = True)
		/// then this property needs to be queried to find out if the particular render target can support post pixel shader blending.
		/// </remarks>
		public override bool IsValidForMRTPostPixelShaderBlending
		{
			get 
			{
				return Gorgon.Direct3D.CheckDeviceFormat(Gorgon.CurrentDriver.DriverIndex, Driver.DeviceType, Converter.Convert(_currentVideoMode.Format), 
					D3D9.Usage.QueryPostPixelShaderBlending | D3D9.Usage.RenderTarget, D3D9.ResourceType.Surface, Converter.Convert(_currentVideoMode.Format));
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
		/// Property to return the format of the depth buffer.
		/// </summary>
		public DepthBufferFormats DepthBufferFormat
		{
			get
			{
				return Converter.ConvertDepthFormat(_presentParameters.AutoDepthStencilFormat);
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
				return base.UseDepthBuffer;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Property to return whether the swap chain has a stencil buffer attached or not.
		/// </summary>
		public override bool UseStencilBuffer
		{
			get
			{
				return base.UseStencilBuffer;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Property to set or return the height of the render target.
		/// </summary>
		public override int Height
		{
			get
			{
				return base.Height;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Property to set or return the width of the render target.
		/// </summary>
		public override int Width
		{
			get
			{
				return base.Width;
			}
			set
			{
				throw new NotImplementedException();
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
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a swap chain render target.
		/// </summary>
		/// <param name="mode">Mode to use.</param>
		/// <param name="usedepth">TRUE to use a depth buffer, FALSE to exclude.</param>
		/// <param name="usestencil">TRUE to use a stencil buffer, FALSE to exclude.</param>
		/// <param name="resize">TRUE if resizing, FALSE if not.</param>
		/// <param name="preserveBackBuffer">TRUE to preserve the contents of the back buffer when the target is updated, FALSE to discard.</param>
		private void CreateSwapChain(VideoMode mode, bool usedepth, bool usestencil, bool resize, bool preserveBackBuffer)
		{
			// Ensure we have the device.
			if (Gorgon.Screen.DeviceNotReset)
				return;

			_owner.Resize -= new EventHandler(OnOwnerResized);

			_backBufferPreserved = preserveBackBuffer;
			_requestedVideoMode = mode;
			base.UseDepthBuffer = usedepth;
			base.UseStencilBuffer = usestencil;

			SetPresentParams(mode, true, resize, _owner.ClientSize.Width, _owner.ClientSize.Height, preserveBackBuffer);

			// Try to create the swap chain.
			try
			{
				_swapChain = new D3D9.SwapChain(Gorgon.Screen.Device, _presentParameters);
			}
			catch (Exception ex)
			{
				throw new GorgonException(GorgonErrors.CannotCreate, "Error creating the D3D render target object.", ex);
			}

			Gorgon.Log.Print("RenderWindow", "Swap chain created.", LoggingLevel.Verbose);

			// Get the color buffer.
			SetColorBuffer(_swapChain.GetBackBuffer(0));

			// If we have a Z Buffer, then try to create it.
			if (_presentParameters.EnableAutoDepthStencil)
			{
				SetDepthBuffer(D3D9.Surface.CreateDepthStencil(Gorgon.Screen.Device, _currentVideoMode.Width, _currentVideoMode.Height, _presentParameters.AutoDepthStencilFormat, D3D9.MultisampleType.None, 0, false));
				Gorgon.Log.Print("RenderWindow", "Depth buffer created.", LoggingLevel.Verbose);
			}
			else
				SetDepthBuffer(null);

			// Update width and height to reflect the backbuffer dimensions.
			base.Width = _currentVideoMode.Width;
			base.Height = _currentVideoMode.Height;

			// Update windows.
			Refresh();

			_owner.Resize += new EventHandler(OnOwnerResized);

			Gorgon.Log.Print("RenderWindow", "Swap chain mode {0}x{1}x{2} ({3}) has been set.", LoggingLevel.Simple, Width, Height, mode.Bpp, mode.Format.ToString());
		}

		/// <summary>
		/// Function to set whether to use a depth and/or stencil buffer.
		/// </summary>
		/// <param name="useDepth">TRUE to use a depth buffer, FALSE to exclude.</param>
		/// <param name="useStencil">TRUE to use a stencil buffer, FALSE to exclude.  If the depth buffer is FALSE, then this is automatically FALSE.</param>
		protected void SetDepthStencil(bool useDepth, bool useStencil)
		{
			base.UseDepthBuffer = useDepth;
			base.UseStencilBuffer = useStencil;
		}

		/// <summary>
		/// Function to set the width and height of the target.
		/// </summary>
		/// <param name="width">Width to set.</param>
		/// <param name="height">Height to set.</param>
		protected void SetDimensions(int width, int height)
		{
			base.Width = width;
			base.Height = height;
		}

		/// <summary>
		/// Function to perform a flip of the D3D buffers.
		/// </summary>
		/// <returns>A result code if the device is in a lost state.</returns>
		protected internal virtual DX.Result D3DFlip()
		{
			return _swapChain.Present(D3D9.Present.DoNotWait);
		}

		/// <summary>
		/// Function to handle a resize of the owner.
		/// </summary>
		/// <param name="sender">Sender of this event.</param>
		/// <param name="e">Event arguments.</param>
		protected virtual void OnOwnerResized(object sender, EventArgs e)
		{
			// If we're not the primary backbuffer, then just destroy and recreate.
			if (_ownerForm.WindowState != FormWindowState.Minimized)
			{
				DeviceLost();
				DeviceReset();
			}
			
			// Update the render windows.
			Refresh();
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
			D3D9.Format[][] dsFormats = new D3D9.Format[][] {new D3D9.Format[] {D3D9.Format.D24SingleS8,D3D9.Format.D24S8,D3D9.Format.D24X4S4,D3D9.Format.D15S1,D3D9.Format.L16},
																		new D3D9.Format[] {D3D9.Format.D32,D3D9.Format.D24X8,D3D9.Format.D16}};

			base.UseStencilBuffer = false;
			base.UseDepthBuffer = false;
			_presentParameters.EnableAutoDepthStencil = false;
			_presentParameters.AutoDepthStencilFormat = D3D9.Format.Unknown;

			// Force the window to be shown.
			if (!_owner.Visible)
				_owner.Visible = true;

			// Deny the stencil buffer if it's not supported.
			if ((!Gorgon.CurrentDriver.SupportStencil) && (usestencil))
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
					if ((!_presentParameters.EnableAutoDepthStencil) && (Gorgon.CurrentDriver.DepthFormatSupported(Converter.Convert(_presentParameters.BackBufferFormat), Converter.ConvertDepthFormat(dsFormats[0][i]))))
					{
						_presentParameters.EnableAutoDepthStencil = true;
						_presentParameters.AutoDepthStencilFormat = dsFormats[0][i];
						base.UseStencilBuffer = true;
						base.UseDepthBuffer = true;
						Gorgon.Log.Print("RenderWindow", "Stencil and depth buffer requested and found.  Using stencil buffer. ({0})", LoggingLevel.Verbose, Converter.ConvertDepthFormat(dsFormats[0][i]).ToString());
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
					if ((!_presentParameters.EnableAutoDepthStencil) && (Gorgon.CurrentDriver.DepthFormatSupported(Converter.Convert(_presentParameters.BackBufferFormat), Converter.ConvertDepthFormat(dsFormats[1][i]))))
					{
						_presentParameters.EnableAutoDepthStencil = true;
						_presentParameters.AutoDepthStencilFormat = dsFormats[1][i];
						base.UseDepthBuffer = true;
						Gorgon.Log.Print("RenderWindow", "Stencil buffer not requested or found.  Using depth buffer. ({0}).", LoggingLevel.Verbose, Converter.ConvertDepthFormat(dsFormats[1][i]).ToString());
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
		protected virtual void SetPresentParams(VideoMode mode, bool usewindow, bool resize, int resizewidth, int resizeheight, bool preserveBackBuffer)
		{
			// Make sure the desktop can be used for rendering.
			if ((usewindow) && (!Gorgon.CurrentDriver.DesktopFormatSupported(mode)))
				throw new ArgumentException("The desktop video mode (" + mode.ToString() + ") will not support the device object.");

			if (!resize)
				Gorgon.Log.Print("RenderWindow", "Setting swap chain to {0}x{1}x{2} ({3}) on '{4}'.", LoggingLevel.Intermediate, mode.Width, mode.Height, mode.Bpp, mode.Format.ToString(), Name);
			else
				Gorgon.Log.Print("RenderWindow", "Resizing swap chain to {0}x{1}x{2} ({3}) on '{4}'.", LoggingLevel.Intermediate, resizewidth, resizeheight, mode.Bpp, mode.Format.ToString(), Name);

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

			// Size to control boundaries.
			if (!resize)
			{
				if (_owner.ClientSize.Width > mode.Width)
					_owner.ClientSize = new Drawing.Size(mode.Width, _owner.ClientSize.Height);

				if (_owner.ClientSize.Height > mode.Height)
					_owner.ClientSize = new Drawing.Size(_owner.ClientSize.Width, mode.Height);

				_presentParameters.BackBufferHeight = mode.Height;
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

			if (_presentParameters.SwapEffect == D3D9.SwapEffect.Copy)
				_presentParameters.BackBufferCount = 1;
			else
				_presentParameters.BackBufferCount = 3;

			_presentParameters.BackBufferFormat = Converter.Convert(_currentVideoMode.Format);

			_presentParameters.PresentationInterval = D3D9.PresentInterval.Immediate;
			_presentParameters.FullScreenRefreshRateInHertz = 0;

			UpdateDepthStencilFormat(UseStencilBuffer, UseDepthBuffer);
		}

		/// <summary>
		/// Function to render the scene for this target.
		/// </summary>
		public override void Update()
		{
			Gorgon.Renderer.Render();
			Gorgon.Renderer.Flip(this);
		}

		/// <summary>
		/// Function to copy the render target into a render target image.
		/// </summary>
		/// <param name="destination">The render image that will receive the data.</param>
		public override void CopyToImage(RenderImage destination)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			ConvertToImageData(destination.Image, Converter.ConvertToImageFormat(Mode.Format));
		}

		/// <summary>
		/// Function to copy the render target into an image.
		/// </summary>
		/// <param name="image">Image that will receive the data.</param>
		public override void CopyToImage(Image image)
		{
			ConvertToImageData(image, Converter.ConvertToImageFormat(Mode.Format));
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="RenderWindow"/> class.
		/// </summary>
		/// <param name="name">Name of this swap chain.</param>
		/// <param name="owner">Owning window of this render target.</param>
		protected RenderWindow(string name, Control owner)
			: base(name, 0, 0)
		{
			Control parent = null;		// Parent of the owning control.

			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (owner == null)
				throw new ArgumentNullException("owner");

			if (RenderTargetCache.Targets.Contains(name))
				throw new ArgumentException("'" + name + "' already exists.");

			Gorgon.Log.Print("RenderWindow", "Creating rendering window '{0}' ...", LoggingLevel.Intermediate, name);

			_owner = owner;

			// Make sure we're in windowed mode if we've already set the primary window.
			if ((Gorgon.Screen != null) && (!Gorgon.Screen.Windowed))
				throw new InvalidOperationException("Cannot create multiple swapchains while in full-screen mode.");

			// Create presentation parameters.
			_presentParameters = new D3D9.PresentParameters();

			// Determine the parent form.
			_ownerForm = _owner as Form;

			if (_ownerForm == null)
			{
				parent = _owner.Parent;
				// Recurse until we find a form.
				while ((parent != null) && (!(parent is Form)))
					parent = parent.Parent;

				if (parent == null)
					throw new Exception("This shouldn't happen - I can't find a parent form for this control!!!");

				_ownerForm = parent as Form;
			}

			// Get settings from current desktop video mode.
			_requestedVideoMode = Gorgon.DesktopVideoMode;
			_currentVideoMode = Gorgon.DesktopVideoMode;

			if (IntPtr.Size == 4)
				Gorgon.Log.Print("RenderWindow", "Bound to control: {0} (0x{1:x8}) of type {2}.  Parent form: {3} (0x{4:x8})", LoggingLevel.Verbose, owner.Name, owner.Handle.ToInt32(), owner.GetType().ToString(), _ownerForm.Name, _ownerForm.Handle.ToInt32().ToString("x").PadLeft(8, '0'));
			else
				Gorgon.Log.Print("RenderWindow", "Bound to control: {0} (0x{1:x16}) of type {2}.  Parent form: {3} (0x{4:x16})", LoggingLevel.Verbose, owner.Name, owner.Handle.ToInt64(), owner.GetType().ToString(), _ownerForm.Name, _ownerForm.Handle.ToInt64().ToString("x").PadLeft(16, '0'));
			Gorgon.Log.Print("RenderWindow", "Rendering window '{0}' created.", LoggingLevel.Intermediate, name);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RenderWindow"/> class.
		/// </summary>
		/// <param name="name">Name of this swap chain.</param>
		/// <param name="owner">Owning window of this render target.</param>
		/// <param name="width">Width of the render window.</param>
		/// <param name="height">Height of the render window.</param>
		/// <param name="format">Back buffer format of the render window.</param>
		/// <param name="useDepth">TRUE to use a depth buffer, FALSE to exclude.</param>
		/// <param name="useStencil">TRUE to use a stencil buffer, FALSE to exclude.</param>
		/// <param name="preserveBackBuffer">TRUE to preserve the contents of the back buffer when the target is updated, FALSE to discard.</param>
		public RenderWindow(string name, Control owner, int width, int height, BackBufferFormats format, bool useDepth, bool useStencil, bool preserveBackBuffer) 
			: base(name, width, height)
		{
			Control parent = null;		// Parent of the owning control.

			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (owner == null)
				throw new ArgumentNullException("owner");

			if (RenderTargetCache.Targets.Contains(name))
				throw new ArgumentException("'" + name + "' already exists.");

			Gorgon.Log.Print("RenderWindow", "Creating rendering window '{0}' ...", LoggingLevel.Intermediate, name);

			_owner = owner;

			// Make sure we're in windowed mode if we've already set the primary window.
			if ((Gorgon.Screen != null) && (!Gorgon.Screen.Windowed))
				throw new InvalidOperationException("Cannot create multiple swapchains while in full-screen mode.");

			// Create presentation parameters.
			_presentParameters = new D3D9.PresentParameters();

			// Determine the parent form.
			_ownerForm = _owner as Form;

			if (_ownerForm == null)
			{
				parent = _owner.Parent;
				// Recurse until we find a form.
				while ((parent != null) && (!(parent is Form)))
					parent = parent.Parent;

				if (parent == null)
					throw new Exception("This shouldn't happen - Can't find a parent form for the control!!");

				_ownerForm = parent as Form;
			}

			// Get settings from current desktop video mode.
			_requestedVideoMode = Gorgon.DesktopVideoMode;
			_currentVideoMode = Gorgon.DesktopVideoMode;

			if (IntPtr.Size == 4)
				Gorgon.Log.Print("RenderWindow", "Bound to control: {0} (0x{1:x8}) of type {2}.  Parent form: {3} (0x{4:x8})", LoggingLevel.Verbose, owner.Name, owner.Handle.ToInt32(), owner.GetType().ToString(), _ownerForm.Name, _ownerForm.Handle.ToInt32().ToString("x").PadLeft(8, '0'));
			else
				Gorgon.Log.Print("RenderWindow", "Bound to control: {0} (0x{1:x16}) of type {2}.  Parent form: {3} (0x{4:x16})", LoggingLevel.Verbose, owner.Name, owner.Handle.ToInt64(), owner.GetType().ToString(), _ownerForm.Name, _ownerForm.Handle.ToInt64().ToString("x").PadLeft(16, '0'));
			Gorgon.Log.Print("RenderWindow", "Rendering window '{0}' created.", LoggingLevel.Intermediate, name);

			// Create the swap chain.
			CreateSwapChain(new VideoMode(width, height, 0, format), useDepth, useStencil, false, preserveBackBuffer);

			// Add to cache.
			RenderTargetCache.Targets.Add(this);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RenderWindow"/> class.
		/// </summary>
		/// <param name="name">Name of this swap chain.</param>
		/// <param name="owner">Owning window of this render target.</param>
		/// <param name="width">Width of the render window.</param>
		/// <param name="height">Height of the render window.</param>
		/// <param name="format">Back buffer format of the render window.</param>
		/// <param name="useDepth">TRUE to use a depth buffer, FALSE to exclude.</param>
		/// <param name="preserveBackBuffer">TRUE to preserve the contents of the back buffer when the target is updated, FALSE to discard.</param>
		public RenderWindow(string name, Control owner, int width, int height, BackBufferFormats format, bool useDepth, bool preserveBackBuffer)
			: this(name, owner, width, height, format, useDepth, false, preserveBackBuffer)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RenderWindow"/> class.
		/// </summary>
		/// <param name="name">Name of this swap chain.</param>
		/// <param name="owner">Owning window of this render target.</param>
		/// <param name="width">Width of the render window.</param>
		/// <param name="height">Height of the render window.</param>
		/// <param name="format">Back buffer format of the render window.</param>
		/// <param name="preserveBackBuffer">TRUE to preserve the contents of the back buffer when the target is updated, FALSE to discard.</param>
		public RenderWindow(string name, Control owner, int width, int height, BackBufferFormats format, bool preserveBackBuffer)
			: this(name, owner, width, height, format, false, false, preserveBackBuffer)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RenderWindow"/> class.
		/// </summary>
		/// <param name="name">Name of this swap chain.</param>
		/// <param name="owner">Owning window of this render target.</param>
		/// <param name="width">Width of the render window.</param>
		/// <param name="height">Height of the render window.</param>
		/// <param name="preserveBackBuffer">TRUE to preserve the contents of the back buffer when the target is updated, FALSE to discard.</param>
		public RenderWindow(string name, Control owner, int width, int height, bool preserveBackBuffer)
			: this(name, owner, width, height, Gorgon.DesktopVideoMode.Format, false, false, preserveBackBuffer)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RenderWindow"/> class.
		/// </summary>
		/// <param name="name">Name of this swap chain.</param>
		/// <param name="owner">Owning window of this render target.</param>
		/// <param name="preserveBackBuffer">TRUE to preserve the contents of the back buffer when the target is updated, FALSE to discard.</param>
		public RenderWindow(string name, Control owner, bool preserveBackBuffer)
			: this(name, owner, owner.Width, owner.Height, Gorgon.DesktopVideoMode.Format, false, false, preserveBackBuffer)
		{
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
				Gorgon.Log.Print("RenderWindow", "Destroying render window '{0}'...", LoggingLevel.Intermediate, Name);

				// Remove resizing handler.
				_owner.Resize -= new EventHandler(OnOwnerResized);

				// Remove buffer hooks.
				SetColorBuffer(null);
				Gorgon.Log.Print("RenderWindow", "Color buffer destroyed.", LoggingLevel.Verbose);

				SetDepthBuffer(null);
				Gorgon.Log.Print("RenderWindow", "Depth buffer destroyed.", LoggingLevel.Verbose);

				if (_swapChain != null)
				{
					_swapChain.Dispose();
					Gorgon.Log.Print("RenderWindow", "Swap chain destroyed.", LoggingLevel.Verbose);
				}

				// Turn off the active render target.
				if (Gorgon.CurrentRenderTarget == this)
					Gorgon.CurrentRenderTarget = null;

				Gorgon.Log.Print("RenderWindow", "Render window '{0}' destroyed.", LoggingLevel.Intermediate, Name);
			}			

			// Do unmanaged clean up.
			_swapChain = null;			
		}
		#endregion

		#region IDeviceStateObject Members
		/// <summary>
		/// Function called when device is lost.
		/// </summary>
		public override void DeviceLost()
		{
			base.DeviceLost();
			Gorgon.CurrentRenderTarget = null;

			SetDepthBuffer(null);
			SetColorBuffer(null);

			if (_swapChain != null)
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
			CreateSwapChain(_requestedVideoMode, UseDepthBuffer, UseStencilBuffer, true, _backBufferPreserved);
		}
		#endregion
	}
}
