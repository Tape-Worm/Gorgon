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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GI = SlimDX.DXGI;
using D3D = SlimDX.Direct3D11;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A swap chain used to display graphics to a window.
	/// </summary>
	public class GorgonSwapChain
		: GorgonNamedObject, IDisposable
	{
		#region Classes.
		/// <summary>
		/// Records the form state.
		/// </summary>
		private class FormStateRecord
		{
			#region Variables.
			private Point _location;					// Window location.
			private Size _size;							// Window size.
			private FormBorderStyle _border;			// Window border.
			private bool _topMost;						// Topmost flag.
			private bool _sysMenu;						// System menu flag.
			private bool _maximizeButton;				// Maxmimize button flag.
			private bool _minimizeButton;				// Minimize button flag.
			private bool _visible;						// Visible flag.
			private bool _enabled;						// Enabled flag.
			private Form _window;						// Window.
			#endregion

			#region Methods.
			/// <summary>
			/// Function to restore the original window state.
			/// </summary>
			/// <param name="keepSize">TRUE to keep the size of the window, FALSE to restore it.</param>
			/// <param name="dontMove">TRUE to keep the window from moving, FALSE to restore the last location.</param>
			public void Restore(bool keepSize, bool dontMove)
			{
				if (_window == null)
					return;

				if (!dontMove)
					_window.Location = _location;
				if (!keepSize)
					_window.Size = _size;
				_window.FormBorderStyle = _border;
				_window.TopMost = _topMost;
				_window.ControlBox = _sysMenu;
				_window.MaximizeBox = _maximizeButton;
				_window.MinimizeBox = _minimizeButton;
				_window.Enabled = _enabled;
				_window.Visible = _visible;
			}

			/// <summary>
			/// Function to update the form state.
			/// </summary>
			public void Update()
			{
				_location = _window.Location;
				_size = _window.Size;
				_border = _window.FormBorderStyle;
				_topMost = _window.TopMost;
				_sysMenu = _window.ControlBox;
				_maximizeButton = _window.MaximizeBox;
				_minimizeButton = _window.MinimizeBox;
				_enabled = _window.Enabled;
				_visible = _window.Visible;
			}
			#endregion

			#region Constructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="FormStateRecord"/> struct.
			/// </summary>
			/// <param name="window">The window.</param>
			public FormStateRecord(Form window)
			{
				_location = window.Location;
				_size = window.Size;
				_border = window.FormBorderStyle;
				_topMost = window.TopMost;
				_sysMenu = window.ControlBox;
				_maximizeButton = window.MaximizeBox;
				_minimizeButton = window.MinimizeBox;
				_enabled = window.Enabled;
				_visible = window.Visible;
				_window = window;
			}
			#endregion
		}
		#endregion

		#region Variables.
		private Form _parentForm = null;			// Parent form for our window.
		private bool _disposed = false;				// Flag to indicate that the object was disposed.
		private GI.SwapChain _swapChain = null;		// DXGI swap chain.
		private bool _wasWindowed = true;			// Flag to indicate that the window was in windowed mode because of a transition.
		private FormStateRecord _formState = null;	// Form state.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the video output that the swap chain is operating on.
		/// </summary>
		public GorgonVideoOutput VideoOutput
		{
			get
			{
				if (_swapChain == null)
					return null;

				GI.Output d3doutput = _swapChain.ContainingOutput;

				var videoOutput = (from output in Settings.VideoDevice.Outputs
								   where output.Handle == d3doutput.Description.MonitorHandle
								   select output).SingleOrDefault();

				return videoOutput;
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

		/// <summary>
		/// Property to return the graphics interface that owns this object.
		/// </summary>
		public GorgonGraphics Graphics
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
			if (Settings.IsWindowed)
				return;
		}

		/// <summary>
		/// Handles the Activated event of the _parentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _parentForm_Activated(object sender, EventArgs e)
		{
			if (!_wasWindowed)
			{
				Settings.IsWindowed = false;
				_wasWindowed = true;
			}
		}

		/// <summary>
		/// Handles the Resize event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Window_Resize(object sender, EventArgs e)
		{
			if (_parentForm.WindowState != FormWindowState.Minimized)
			{
				if (_swapChain != null)
				{
					GI.SwapChainFlags flags = GI.SwapChainFlags.AllowModeSwitch;

					if ((!Settings.IsWindowed) && (!Settings.AllowRotation))
						flags |= GI.SwapChainFlags.NonPrerotated;

					Settings.VideoMode = new GorgonVideoMode(Settings.Window.ClientSize.Width, Settings.Window.ClientSize.Height, Settings.VideoMode.Value.Format, Settings.VideoMode.Value.RefreshRateNumerator, Settings.VideoMode.Value.RefreshRateDenominator);
					_swapChain.ResizeBuffers(Settings.BufferCount, Settings.VideoMode.Value.Width, Settings.VideoMode.Value.Height, (GI.Format)Settings.VideoMode.Value.Format, flags);
				}
			}
		}
		
		/// <summary>
		/// Function to intialize the swap chain.
		/// </summary>
		internal void Initialize()
		{
			GI.SwapChainFlags flags = GI.SwapChainFlags.AllowModeSwitch;
			GI.SwapChainDescription d3dSettings = new GI.SwapChainDescription();

			d3dSettings.BufferCount = Settings.BufferCount;
			d3dSettings.Flags = flags;

			d3dSettings.IsWindowed = true;
			d3dSettings.ModeDescription = GorgonVideoMode.Convert(Settings.VideoMode.Value);
			d3dSettings.OutputHandle = Settings.Window.Handle;
			d3dSettings.SampleDescription = GorgonMultiSampling.Convert(Settings.MultiSamples);
			d3dSettings.SwapEffect = GorgonSwapChainSettings.Convert(Settings.SwapEffect);

			if ((Settings.Flags & SwapChainUsageFlags.RenderTarget) == SwapChainUsageFlags.RenderTarget)
				d3dSettings.Usage = GI.Usage.RenderTargetOutput;

			if ((Settings.Flags & SwapChainUsageFlags.ShaderInput) == SwapChainUsageFlags.ShaderInput)
				d3dSettings.Usage |= GI.Usage.ShaderInput;

			Gorgon.Log.Print("GorgonSwapChain '{0}': Creating D3D11 swap chain...", Diagnostics.GorgonLoggingLevel.Simple, Name);
			_swapChain = new GI.SwapChain(Graphics.GIFactory, Settings.VideoDevice.D3DDevice, d3dSettings);

			// Due to a bug with winforms and DXGI, we have to manually handle transitions ourselves.
			Graphics.GIFactory.SetWindowAssociation(Settings.Window.Handle, GI.WindowAssociationFlags.IgnoreAll);

			// We need to handle focus loss ourselves because of the aforementioned bug.
			_parentForm.Activated += new EventHandler(_parentForm_Activated);
			_parentForm.Deactivate += new EventHandler(_parentForm_Deactivate);

			if (Settings.IsWindowed)
				Settings.Window.Resize += new EventHandler(Window_Resize);

			if ((!Settings.IsWindowed) && (!Settings.AllowRotation))
				flags |= GI.SwapChainFlags.NonPrerotated;

			ModeStateUpdate(Settings.IsWindowed, d3dSettings.ModeDescription, d3dSettings.BufferCount, flags, true);
		}

		/// <summary>
		/// Function to update the fullscreen/windowed mode state.
		/// </summary>
		/// <param name="windowed">TRUE for windowed mode, FALSE for fullscreen.</param>
		/// <param name="mode">Video mode to use.</param>
		/// <param name="bufferCount">Number of back buffers.</param>
		/// <param name="flags">Flags for the swap chain.</param>
		/// <param name="overrideState">TRUE to force the mode switch, FALSE to let the swap chain determine if it needs to switch modes.</param>
		private void ModeStateUpdate(bool windowed, GI.ModeDescription mode, int bufferCount, GI.SwapChainFlags flags, bool overrideState)
		{
			_swapChain.ResizeTarget(mode);

			if ((overrideState) || (windowed != Settings.IsWindowed))
			{
				if (!windowed)
					_swapChain.SetFullScreenState(true, VideoOutput.GIOutput);
				else
					_swapChain.SetFullScreenState(false, null);
			}

			_swapChain.ResizeBuffers(bufferCount, mode.Width, mode.Height, mode.Format, flags);
		}

		/// <summary>
		/// Function to update the settings for the swap chain.
		/// </summary>
		/// <param name="settings">Settings to update.</param>
		/// <remarks>This will only update the format, buffer size, buffer count, windowed mode and the rotation settings.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Window">GorgonSwapChainSettings.Window</see> property is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonSwapChainSettings.VideoMode.Format</see> property cannot be used by the video device for displaying data.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.MultiSamples.Quality</see> property is higher than what the video device can support.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		public void UpdateSettings(GorgonSwapChainSettings settings)
		{
			GI.SwapChainFlags flags = GI.SwapChainFlags.AllowModeSwitch;

			if (_swapChain == null)
				return;

			Settings.Window.Resize -= new EventHandler(Window_Resize);

			// Use these previous settings.
			settings.Window = Settings.Window;
			settings.VideoDevice = Settings.VideoDevice;
			settings.Flags = Settings.Flags;
			settings.MultiSamples = Settings.MultiSamples;
			settings.SwapEffect = Settings.SwapEffect;

			// If we're going to full screen mode, remember the current form state.
			if (!settings.IsWindowed)
			{
				if (Settings.IsWindowed)
					_formState.Update();
			}

			// Validate and modify the settings as appropriate.
			Graphics.ValidateSwapChainSettings(settings, this);
			
			// Modify the buffers.
			if ((!settings.IsWindowed) && (!settings.AllowRotation))
				flags |= GI.SwapChainFlags.NonPrerotated;

			ModeStateUpdate(settings.IsWindowed, GorgonVideoMode.Convert(settings.VideoMode.Value), settings.BufferCount, flags, false);

			// If we're coming back from full screen mode, then restore the form state.
			if (settings.IsWindowed)
			{
				if (!Settings.IsWindowed)
					_formState.Restore(false, false);
			}

			Settings = settings;

			if (Settings.IsWindowed)
				Settings.Window.Resize += new EventHandler(Window_Resize);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSwapChain"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this swap chain.</param>
		/// <param name="name">The name of the swap chain.</param>
		/// <param name="settings">Settings for the swap chain.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		internal GorgonSwapChain(GorgonGraphics graphics, string name, GorgonSwapChainSettings settings)
			: base(name)
		{
			if (graphics == null)
				throw new ArgumentNullException("graphics");

			Settings = settings;
			Graphics = graphics;			

			// Get the parent form for our window.
			_parentForm = Gorgon.GetTopLevelForm(settings.Window);
			_formState = new FormStateRecord(_parentForm);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Settings.Window.Resize -= new EventHandler(Window_Resize);
					_parentForm.Activated -= new EventHandler(_parentForm_Activated);
					_parentForm.Deactivate -= new EventHandler(_parentForm_Deactivate);

					Gorgon.Log.Print("GorgonSwapChain '{0}': Removing D3D11 swap chain...", Diagnostics.GorgonLoggingLevel.Simple, Name);
					if (_swapChain != null)
					{
						if (!Settings.IsWindowed)
							_swapChain.SetFullScreenState(false, null);

						_swapChain.Dispose();
					}
					if (Graphics != null)
						Graphics.RemoveTrackedObject(this);					
				}

				_swapChain = null;
				Graphics = null;
				_disposed = true;
			}
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
