#region LGPL.
//
// Gorgon.
// Copyright (C) 2004 Michael Winsor
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
// Created: Saturday, May 08, 2004 11:59:54 AM
//
#endregion

//#define CREATE_LOGO

using System;
using System.Data;
using System.Collections.Generic;
using Forms = System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;
using SharpUtilities.Utility;
using SharpUtilities.Mathematics;
using Drawing = System.Drawing;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.FileSystems;
using GorgonLibrary.InputDevices;
using GorgonLibrary.PlugIns;


namespace GorgonLibrary.Framework
{
	/// <summary>
	/// Object representing a base application framework to use with Gorgon.
	/// </summary>
	public partial class GorgonApplicationWindow 
		: Forms.Form
	{
		#region Constants.
		private const float _maxBlur = 0.03999f;		// Maximum blur.
		#endregion

		#region Variables.
		private string _resourcePath = string.Empty;			// Path to the application resources.		
        private SetupDialog _setup = null;						// Setup dialog.
		private bool _useDepthBuffer;							// Flag to use a depth buffer or not.
		private bool _useStencilBuffer;							// Flag to use a stencil buffer or not.
		private string _plugInPath = string.Empty;				// Path to plug-ins.
		private Forms.Control _renderControl = null;			// Control used for the main render window.
		private PreciseTimer _logoTimer = null;					// Logo timer.
		private Sprite _logoSprite = null;						// Logo sprite.
		private float _logoOpacity;								// Logo opacity.
		private float _logoBlur = _maxBlur;						// Blur factor.
		private bool _logoFadeDir;								// Logo fade direction.
		private Shader _logoShader = null;						// Shader for the logo.
		private bool _logoDone;									// Logo is done animation?
		private double _logoSwitchTime = 0;						// Time value for switching logo animations.
		private SortedList<string, FileSystem> _fileSystems;	// File systems.
		private string _applicationName = "GorgonApplication";	// Application name.
		private string _configPath = string.Empty;				// Configuration path.
		private Input _input;									// Input plug-in.
		private Sprite _cursorSprite;							// Cursor sprite.
		private Image _cursorImage;								// Cursor image.
		private bool _cursorVisible;							// Flag to indicate that the cursor is visible.
		private bool _showLogo = true;							// Flag to show the logo.
		private bool _escapeCloses = true;						// Flag to indicate that the escape key will close the application.
		private Font _defaultFont = null;						// Default font.
		#endregion

        #region Properties.
		/// <summary>
		/// Property to return the cursor sprite.
		/// </summary>
		[Browsable(false)]
		protected Sprite CursorSprite
		{
			get
			{
				return _cursorSprite;
			}
		}

		/// <summary>
		/// Property to return the default font.
		/// </summary>
		protected Font FrameworkFont
		{
			get
			{
				return _defaultFont;
			}
		}

		/// <summary>
		/// Property to set or return whether the escape key will close the application or not.
		/// </summary>
		[Browsable(true), Category("Framework"), Description("Sets whether the escape key will close the application."), DefaultValue(true)]
		public bool EscapeCloses
		{
			get
			{
				return _escapeCloses;
			}
			set
			{
				_escapeCloses = value;
			}
		}

		/// <summary>
		/// Property to set or return whether to invert the frame stats text color.
		/// </summary>
		[Browsable(true), Category("Gorgon"), Description("Sets whether the frame statistics text color is inverted, or whether it shows up as the intended color over a tinted background.  The latter is a slower method."), DefaultValue(true)]
		public bool InvertFrameStatsColor
		{
			get
			{
				return Gorgon.InvertFrameStatsTextColor;
			}
			set
			{
				Gorgon.InvertFrameStatsTextColor = value;
			}
		}

		/// <summary>
		/// Property to set or return whether fast resizing is enabled or not.
		/// </summary>
		[Browsable(true), Category("Gorgon"), Description("Sets whether the system will reset the device when a resize event occurs, or just stretch the rendering image."), DefaultValue(false)]
		public bool FastResize
		{
			get
			{
				return Gorgon.FastResize;
			}
			set
			{
				Gorgon.FastResize = value;
			}
		}

		/// <summary>
		/// Property to set or return whether to invert the frame stats text color.
		/// </summary>
		[Browsable(true), Category("Gorgon"), Description("Sets whether to show the Gorgon logo on the primary rendering surface or not."), DefaultValue(true)]
		public bool LogoBadgeVisible
		{
			get
			{
				return Gorgon.LogoVisible;
			}
			set
			{
				Gorgon.LogoVisible = value;
			}
		}

		/// <summary>
		/// Property to set or return whether to show or hide the animated logo on startup.
		/// </summary>
		/// <remarks>Users can make a custom logo by overriding the <see cref="GorgonLibrary.Framework.GorgonApplicationWindow.RunLogo"/> method.</remarks>
		[Browsable(true), Category("Framework"), Description("Sets whether to show a logo on application startup."), DefaultValue(true)]
		public bool ShowLogo
		{
			get
			{
				return _showLogo;
			}
			set
			{
				_showLogo = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the cursor sprite is visible.
		/// </summary>
		[Browsable(true), Category("Framework"), Description("Shows or hides the built-in cursor sprite.")]
		public bool CursorSpriteVisible
		{
			get
			{
				return _cursorVisible;
			}
			set
			{
				_cursorVisible = value;
			}
		}

		/// <summary>
		/// Property to set or return the main render window.
		/// </summary>
		[Browsable(true), Category("Framework"), Description("The control which will receive the rendering information.")]
		public Forms.Control RenderWindow
		{
			get
			{
				return _renderControl;
			}
			set
			{
				_renderControl = value;
			}
		}

		/// <summary>
		/// Property to set or return the application name.
		/// </summary>
		[Browsable(true), Category("Framework"), DefaultValue("GorgonApplication"), Description("Defines the human readable application name.")]
		public string ApplicationName
		{
			get
			{
				return _applicationName;
			}
			set
			{
				if (value == null)
					value = string.Empty;
				_applicationName = value;
			}
		}

		/// <summary>
		/// Property to return any loaded input plug-ins.
		/// </summary>
		[Browsable(false)]
		public Input Input
		{
			get
			{
				return _input;
			}
		}

		/// <summary>
		/// Property to return the loaded file systems.
		/// </summary>
		[Browsable(false)]
		public SortedList<string, FileSystem> FileSystems
		{
			get
			{
				return _fileSystems;
			}
		}

        /// <summary>
        /// Gets or sets the size of the client area of the form.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Drawing.Size"></see> that represents the size of the form's client area.</returns>
        /// <PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        [Category("Layout"), Browsable(true), Description("Sets the size of the client area for the application window."), RefreshProperties(RefreshProperties.All)]
        public new Drawing.Size ClientSize
        {
            get
            {
                return base.ClientSize;
            }
            set
            {
                SetClientSizeCore(value.Width, value.Height);
            }
        }

        /// <summary>
        /// Property to return the resource path.
        /// </summary>
        [Category("Framework"), Browsable(true), Description("Path to the resources for the application.")]
        public string ResourcePath
        {
            get
            {
                return _resourcePath;
            }
            set
            {
                if (_resourcePath == null)
                    _resourcePath = string.Empty;

                _resourcePath = value;
            }
        }

		/// <summary>
		/// Property to return the plug-in path.
		/// </summary>
        [Category("Framework"), Browsable(true), Description("Path to the plug-ins for the application.")]
		public string PlugInPath
		{
			get
			{
				return _plugInPath;
			}
            set
            {
                if (_plugInPath == null)
                    _plugInPath = string.Empty;
                _plugInPath = value;
            }
		}

        /// <summary>
        /// Property to return the setup dialog interface.
        /// </summary>
        [Browsable(false)]
        public SetupDialog SetupDialog
        {
            get
            {
                return _setup;
            }
        }

		/// <summary>
		/// Property to set or return whether we should use a depth buffer or not, call before calling Setup()
		/// </summary>
        [Category("Gorgon"), Browsable(true), Description("Flag to use a depth buffer with the application.")]
		public bool UseDepthBuffer
		{
			get
			{
				return _useDepthBuffer;
			}
			set
			{
				_useDepthBuffer = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we should use a stencil buffer or not, call before calling Setup()
		/// </summary>
        [Category("Gorgon"), Browsable(true), Description("Flag to use a stencil buffer with the application.")]
		public bool UseStencilBuffer
		{
			get
			{
				return _useStencilBuffer;
			}
			set
			{
				_useStencilBuffer = value;
			}
		}

#if INCLUDE_D3DREF
		/// <summary>
		/// Property to set or return whether we should use the D3D reference device (use this only for testing).
		/// </summary>
        [Category("Gorgon"), Browsable(true), Description("Flag to use the D3D reference device with the application.")]
		public bool UseD3DReferenceDevice
		{
			get
			{
				return Gorgon.UseReferenceDevice;
			}
			set
			{
				Gorgon.UseReferenceDevice = value;
			}
		}
#endif

		/// <summary>
		/// Property to return the number of loaded plug-ins.
		/// </summary>
        [Browsable(false)]
		public int LoadedPlugInCount
		{
			get
			{
				return ConfigurationSettings.PlugIns.Count;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to do setup.
		/// </summary>
		/// <returns>FALSE if cancelled, TRUE if not.</returns>
		private bool Setup()
		{
			bool noCancel;					// Cancel flag.
			bool showBadge;					// Show logo badge flag.
			DataRow[] rows;					// Data rows.
			Forms.Control boundControl;		// Control to bind.

			try
			{
				// Use this window if we didn't specify a target.
				if (RenderWindow == null)
					boundControl = this;
				else
					boundControl = RenderWindow;

				// Get the resource path.
				if (ResourcePath == string.Empty)
				{
					rows = ConfigurationSettings.Paths.Select("PathName='ResourcePath'");
					if (rows.Length > 0)
						_resourcePath = rows[0]["Path"].ToString();

					if (!_resourcePath.EndsWith(@"\"))
						_resourcePath += @"\";

					if (ApplicationName != string.Empty)
						_resourcePath += ApplicationName + @"\";

					// If null, make sure it defaults to the current directory.
					_resourcePath = ValidatePath(_resourcePath);
				}

				// Create new gorgon object.
				Gorgon.Initialize(Gorgon.AllowBackgroundRendering, Gorgon.AllowScreenSaver);

				// Get plug-in path.
				if (PlugInPath == string.Empty)
				{
					if (Gorgon.Platform == PlatformID.x86)
						rows = ConfigurationSettings.Paths.Select("PathName='PlugInPathX86'");
					else
						rows = ConfigurationSettings.Paths.Select("PathName='PlugInPathX64'");

					if (rows.Length > 0)
						_plugInPath = rows[0]["Path"].ToString();

					_plugInPath = System.IO.Path.GetFullPath(ValidatePath(_plugInPath));
				}

				// Call up configuration options.
				_setup = new SetupDialog();

				if (_setup.ShowDialog(this) == Forms.DialogResult.OK)
				{
					Gorgon.AllowScreenSaver = _setup.AllowScreensaver;
					Gorgon.AllowBackgroundRendering = _setup.AllowBackgroundRendering;

					// Load any file systems.
					LoadFilesystems();

					// Load input plug-ins.
					LoadInputPlugIns();

					// Begin loading plug-ins.
					LoadPlugins();

					// Create new gorgon object.
					Gorgon.CurrentDriver = _setup.VideoDriver;
					Gorgon.SetMode(boundControl, _setup.VideoMode.Width, _setup.VideoMode.Height, _setup.VideoMode.Format, _setup.WindowedFlag, false, false, _setup.VideoMode.RefreshRate, _setup.VSyncInterval);

					// Run the logo.
					if (_showLogo)
					{
						showBadge = Gorgon.LogoVisible;
						Gorgon.LogoVisible = false;
						RunLogo();
						Gorgon.LogoVisible = showBadge;
					}
					_logoDone = true;

					// Just quit if we haven't got a target.
					if (!(Gorgon.IsInitialized) || (Gorgon.Screen == null))
						return false;

					// Load cursor.
					_cursorImage = Image.FromResource("__Cursor", Properties.Resources.ResourceManager);
					_cursorSprite = new Sprite("@@Cursor", _cursorImage);
					_cursorSprite.SetAxis(1, 1);

					if (_input != null)
					{
						_input.Bind(boundControl);

						// Set up input defaults.
						if (_input.Keyboard != null)
						{
							_input.Keyboard.Enabled = true;
							_input.Keyboard.Exclusive = false;
							_input.Keyboard.KeyDown += new KeyboardInputEvent(Keyboard_KeyDown);
							_input.Keyboard.KeyUp += new KeyboardInputEvent(Keyboard_KeyUp);
						}

						if (_input.Mouse != null)
						{
							_input.Mouse.Enabled = true;
							_input.Mouse.Exclusive = true;

							// Default to the screen width and height for the range.
							_input.Mouse.PositionRange = new Drawing.RectangleF(0, 0, Gorgon.Screen.Width, Gorgon.Screen.Height);

							_input.Mouse.MouseDown += new MouseInputEvent(Mouse_MouseDown);
							_input.Mouse.MouseUp += new MouseInputEvent(Mouse_MouseUp);
							_input.Mouse.MouseMove += new MouseInputEvent(Mouse_MouseMove);
							_input.Mouse.MouseWheelMove += new MouseInputEvent(Mouse_MouseWheelMove);
						}
					}

					// If we haven't assigned a keyboard interface, then use the standard form handler.
					if ((_input == null) || (_input.Keyboard == null))
						this.KeyDown += new System.Windows.Forms.KeyEventHandler(GorgonApplicationWindow_KeyDown);

					// Set up default font.
					_defaultFont = new Font("DefaultFrameworkFont", this.Font);

					// Inherit the form background color.
					Gorgon.Screen.BackgroundColor = BackColor;

					// Initialize.
					Initialize();

					// Do on-idle.
					Gorgon.DeviceLost += new EventHandler(Gorgon_DeviceLost);
					Gorgon.DeviceReset += new EventHandler(Gorgon_DeviceReset);
					Gorgon.Idle += new FrameEventHandler(Gorgon_Idle);
					noCancel = true;

					// Begin rendering.
					Gorgon.Go();
				}
				else
				{
					Gorgon.Terminate();
					noCancel = false;
				}
			}
			catch(Exception ex)
			{
				Gorgon.CloseMode();
				// Turn off input handler.
				if (_input != null)
				{
					if (_input.Keyboard != null)
					{
						_input.Keyboard.KeyDown -= new KeyboardInputEvent(Keyboard_KeyDown);
						_input.Keyboard.KeyUp -= new KeyboardInputEvent(Keyboard_KeyUp);
					}

					if (_input.Mouse != null)
					{
						_input.Mouse.MouseDown -= new MouseInputEvent(Mouse_MouseDown);
						_input.Mouse.MouseUp -= new MouseInputEvent(Mouse_MouseUp);
						_input.Mouse.MouseMove -= new MouseInputEvent(Mouse_MouseMove);
						_input.Mouse.MouseWheelMove -= new MouseInputEvent(Mouse_MouseWheelMove);

						if ((!_input.Mouse.CursorVisible) && (_input.Mouse.Exclusive))
						{
							_input.Mouse.Exclusive = false;
							_input.Mouse.CursorVisible = true;
						}
					}
				}

				UI.ErrorBox(this, ex);
				noCancel = false;
			}
			finally
			{
				// Clean up.
				if (_setup != null)
					_setup.Dispose();
				_setup = null;
			}

			return noCancel;
		}

		/// <summary>
		/// Handles the KeyUp event of the Keyboard control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.InputDevices.KeyboardInputEventArgs"/> instance containing the event data.</param>
		private void Keyboard_KeyUp(object sender, KeyboardInputEventArgs e)
		{
			OnKeyboardKeyUp(e);
		}

		/// <summary>
		/// Handles the MouseDown event of the Mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.InputDevices.MouseInputEventArgs"/> instance containing the event data.</param>
		private void Mouse_MouseDown(object sender, MouseInputEventArgs e)
		{
			OnMouseButtonDown(e);
		}

		/// <summary>
		/// Handles the MouseUp event of the Mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.InputDevices.MouseInputEventArgs"/> instance containing the event data.</param>
		private void Mouse_MouseUp(object sender, MouseInputEventArgs e)
		{
			OnMouseButtonUp(e);
		}

		/// <summary>
		/// Handles the MouseMove event of the Mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.InputDevices.MouseInputEventArgs"/> instance containing the event data.</param>
		private void Mouse_MouseMove(object sender, MouseInputEventArgs e)
		{
			OnMouseMovement(e);
		}

		/// <summary>
		/// Handles the MouseWheelMove event of the Mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.InputDevices.MouseInputEventArgs"/> instance containing the event data.</param>
		private void Mouse_MouseWheelMove(object sender, MouseInputEventArgs e)
		{
			OnMouseWheelScrolled(e);
		}

		/// <summary>
		/// Handles the KeyDown event of the GorgonApplicationWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void GorgonApplicationWindow_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if ((e.Alt) && (e.KeyCode == Forms.Keys.Enter))
				Gorgon.Screen.Windowed = !Gorgon.Screen.Windowed;

			if ((e.Control) && (e.KeyCode == Forms.Keys.F))
				Gorgon.FrameStatsVisible = !Gorgon.FrameStatsVisible;

			if ((e.KeyCode == Forms.Keys.Escape) && (_escapeCloses))
				Close();
		}

		/// <summary>
		/// Handles the KeyDown event of the Keyboard control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.InputDevices.KeyboardInputEventArgs"/> instance containing the event data.</param>
		private void Keyboard_KeyDown(object sender, KeyboardInputEventArgs e)
		{
			if ((e.Alt) && (e.Key == KeyboardKeys.Enter))
				Gorgon.Screen.Windowed = !Gorgon.Screen.Windowed;

			if ((e.Ctrl) && (e.Key == KeyboardKeys.F))
				Gorgon.FrameStatsVisible = !Gorgon.FrameStatsVisible;

			if ((e.Key == KeyboardKeys.Escape) && (_escapeCloses))
				Close();

			OnKeyboardKeyDown(e);
		}

		/// <summary>
		/// Function to blur the logo if we support PS 2.0.
		/// </summary>
		/// <param name="frameTime">Frame draw time.</param>
		private void BlurLogo(float frameTime)
		{			
			if (!_logoFadeDir)
			{
				if (_logoSprite.UniformScale < ((Gorgon.Screen.Width * 3.5f) / _logoSprite.Width))
					_logoBlur -= 0.0132f * frameTime;
				_logoOpacity += 95.0f * frameTime;
				_logoSprite.UniformScale -= 8.0f * frameTime;
				if (_logoSprite.UniformScale < 1.0f)
					_logoSprite.UniformScale = 1.0f;
				if (_logoBlur < 0.00001f) 
					_logoBlur = 0.00001f;
				if (_logoOpacity > 255.0f)
					_logoOpacity = 255.0f;				
			}
			else
			{
				Vector2D scale = _logoSprite.Scale;		// Sprite scale.
				if (scale.Y > 0.0001f)
					scale.X += 22.5f * frameTime;
				else
					scale.X -= 15.0f * frameTime;
				scale.Y -= 6.5f * frameTime;
				
				if (scale.Y < 0.0001f)
					scale.Y = 0.0001f;
				if (scale.X <= 0.0001f)
					_logoDone = true;
				_logoSprite.Scale = scale;
			}

			// Set the blur factor.			
			if (_logoShader != null)
				_logoShader.Parameters["blurAmount"].SetValue(_logoBlur);
			_logoSprite.Opacity = (byte)_logoOpacity;
		}		

		/// <summary>
		/// Function called on the beginning of a frame.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void LogoRender(object sender, FrameEventArgs e)
		{
			Gorgon.Screen.Clear();

			BlurLogo(e.FrameDeltaTime);

			_logoSprite.SetPosition(Gorgon.CurrentVideoMode.Width / 2, Gorgon.CurrentVideoMode.Height / 2);
			_logoSprite.Draw();

			if (_logoTimer.Seconds >= 0.01)
			{
				_logoSwitchTime += 0.01;
				_logoTimer.Reset();
			}

			if (_logoSwitchTime > 4.0)
			{
				if ((_logoDone) && (_logoFadeDir))
					Gorgon.Stop();
				else
					_logoFadeDir = true;
			}
		}

		/// <summary>
		/// Function to load the plug-ins.
		/// </summary>
		private void LoadPlugins()
		{
			// Load each plug-in.
			foreach (GorgonSettings.PlugInsRow row in ConfigurationSettings.PlugIns)
			{
				if ((row.PlugInEnabled) && (row.PlugInDLL != string.Empty))
				{
					if (row.PlugInName == string.Empty)
						PlugInFactory.Load(row.PlugInDLL);
					else
						PlugInFactory.Load(row.PlugInDLL, row.PlugInName);
				}
			}
		}

        /// <summary>
        /// Function to load the file systems.
        /// </summary>
        private void LoadFilesystems()
        {
			FileSystemProvider fsPlugIn = null;	// File system plug-in.
			FileSystem fileSystem = null;		// File system.
			string rootPath = string.Empty;		// Root path.

			// Go through each file system and mount it.
			foreach (GorgonSettings.FileSystemsRow row in ConfigurationSettings.FileSystems)
			{
				if ((row.FileSystemEnabled) && (row.FileSystemProviderDLL != string.Empty))
				{
					fsPlugIn = FileSystemProvider.Load(_plugInPath + row.FileSystemProviderDLL, row.FileSystemProviderName);
					fileSystem = FileSystem.Create(row.FileSystemName, fsPlugIn);

					if (row.FileSystemRoot != string.Empty)
					{
						rootPath = ResourcePath;
						if (System.IO.Path.GetDirectoryName(row.FileSystemRoot) != string.Empty)
							rootPath += ValidatePath(System.IO.Path.GetDirectoryName(row.FileSystemRoot));
						rootPath += System.IO.Path.GetFileName(row.FileSystemRoot);
						fileSystem.AssignRoot(rootPath);
					}

					_fileSystems.Add(fileSystem.Name, fileSystem);
				}
			}
        }

		/// <summary>
		/// Function to load input plug-ins.
		/// </summary>
		private void LoadInputPlugIns()
		{			
			foreach (GorgonSettings.InputPlugInsRow row in ConfigurationSettings.InputPlugIns)
			{
				if ((row.InputEnabled) && (row.InputPlugInDLL != string.Empty) && (row.InputPlugInName != string.Empty))
				{
					_input = Input.LoadInputPlugIn(_plugInPath + row.InputPlugInDLL, row.InputPlugInName);
					return;
				}
			}
		}

		/// <summary>
		/// Function called on the beginning of a frame.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Gorgon_Idle(object sender, FrameEventArgs e)
		{
			Gorgon.Screen.Clear();

			OnLogicUpdate(e);
			OnFrameUpdate(e);

			// Draw the cursor.
			if ((_cursorSprite != null) && (_cursorVisible))
			{
				if ((_input != null) && (_input.Mouse != null))
					_cursorSprite.SetPosition(_input.Mouse.Position.X, _input.Mouse.Position.Y);
				_cursorSprite.Draw();
			}
		}

		/// <summary>
		/// Function called when the device is reset.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Gorgon_DeviceReset(object sender, EventArgs e)
		{
			if ((_input != null) && (_input.Mouse != null))
				_input.Mouse.PositionRange = new Drawing.RectangleF(0, 0, Gorgon.Screen.Width, Gorgon.Screen.Height);

			OnDeviceReset();
		}

		/// <summary>
		/// Function called when the device is lost.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Gorgon_DeviceLost(object sender, EventArgs e)
		{
			OnDeviceLost();
		}

		/// <summary>
		/// Function called when a keyboard key is pushed down.
		/// </summary>
		/// <param name="e">Keyboard event parameters.</param>
		protected virtual void OnKeyboardKeyDown(KeyboardInputEventArgs e)
		{
			// User code goes here.
		}

		/// <summary>
		/// Function called when a keyboard key is released.
		/// </summary>
		/// <param name="e">Keyboard event parameters.</param>
		protected virtual void OnKeyboardKeyUp(KeyboardInputEventArgs e)
		{
			// User code goes here.
		}

		/// <summary>
		/// Function called when a mouse button is pushed down.
		/// </summary>
		/// <param name="e">Mouse event parameters.</param>
		protected virtual void OnMouseButtonDown(MouseInputEventArgs e)
		{
			// User code goes here.
		}

		/// <summary>
		/// Function called when a mouse button is release.
		/// </summary>
		/// <param name="e">Mouse event parameters.</param>
		protected virtual void OnMouseButtonUp(MouseInputEventArgs e)
		{
			// User code goes here.
		}

		/// <summary>
		/// Function called when the mouse is moved.
		/// </summary>
		/// <param name="e">Mouse event parameters.</param>
		protected virtual void OnMouseMovement(MouseInputEventArgs e)
		{
			// User code goes here.
		}

		/// <summary>
		/// Function called when a mouse scroll wheel is scrolled.
		/// </summary>
		/// <param name="e">Mouse event parameters.</param>
		protected virtual void OnMouseWheelScrolled(MouseInputEventArgs e)
		{
			// User code goes here.
		}

		/// <summary>
		/// Function called when the video device has recovered from a lost state.
		/// </summary>
		protected virtual void OnDeviceReset()
		{
			// User code goes here.
		}

		/// <summary>
		/// Function called when the video device is set to a lost state.
		/// </summary>
		protected virtual void OnDeviceLost()
		{
			// User code goes here.
		}

		/// <summary>
        /// Function to do initialization.
        /// </summary>
        protected virtual void Initialize()
		{
			// User code goes here.
		}

		/// <summary>
		/// Function to perform logic updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected virtual void OnLogicUpdate(FrameEventArgs e)
		{
			// User controlled logic goes here.
		}

		/// <summary>
		/// Function to perform rendering updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected virtual void OnFrameUpdate(FrameEventArgs e)
		{
			// User rendering goes here.
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignMode)
			{
				try
				{
					Cursor = Forms.Cursors.WaitCursor;

					// Default to this window.
					_fileSystems = new SortedList<string, FileSystem>();

					if (!string.IsNullOrEmpty(_configPath))
						ConfigurationSettings.ReadXml(_configPath, System.Data.XmlReadMode.IgnoreSchema);

					if (!Setup())
					{
						Forms.Application.Exit();
						return;
					}
				}
				catch (Exception ex)
				{
					if (IsDisposed)
						UI.ErrorBox(null, ex);
					else
						UI.ErrorBox(this, ex);
					Forms.Application.Exit();
				}
				finally
				{
					Cursor = Forms.Cursors.Default;
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(Forms.FormClosingEventArgs e)
		{
			// If we cancel, then don't destroy anything.
			if (e.Cancel)
			{
				base.OnFormClosing(e);
				return;
			}

			// Remove handlers.
			Gorgon.DeviceLost -= new EventHandler(Gorgon_DeviceLost);
			Gorgon.DeviceReset -= new EventHandler(Gorgon_DeviceLost);
			Gorgon.Idle -= new FrameEventHandler(Gorgon_Idle);

			// Clean up input events.
			if (_input != null)
			{
				if (_input.Keyboard != null)
				{
					_input.Keyboard.KeyDown -= new KeyboardInputEvent(Keyboard_KeyDown);
					_input.Keyboard.KeyUp -= new KeyboardInputEvent(Keyboard_KeyUp);
				}

				if (_input.Mouse != null)
				{
					_input.Mouse.MouseDown -= new MouseInputEvent(Mouse_MouseDown);
					_input.Mouse.MouseUp -= new MouseInputEvent(Mouse_MouseUp);
					_input.Mouse.MouseMove -= new MouseInputEvent(Mouse_MouseMove);
					_input.Mouse.MouseWheelMove -= new MouseInputEvent(Mouse_MouseWheelMove);
				}

				// Destroy the input interface.
				_input.Dispose();
				_input = null;
			}

			Gorgon.Terminate();

			base.OnFormClosing(e);
		}

		/// <summary>
		/// Function to validate a path.
		/// </summary>
		/// <param name="path">Path to validate</param>
		/// <returns>Updated path.</returns>
		protected string ValidatePath(string path)
		{
			string result = string.Empty;		// Result.

			if (string.IsNullOrEmpty(path))
				result = @".\";
			else
				result = path;

			if (!result.EndsWith(@"\"))
				result += @"\";

			// Remove invalid characters.
			foreach (char c in System.IO.Path.GetInvalidPathChars())
				result = result.Replace(c, '_');

			return result;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"></see> that contains the event data.</param>
		protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
		{
			// Allow us to skip the logo animation.
			if (!_logoDone)
			{
				e.Handled = true;
				if (e.KeyCode == Forms.Keys.Escape)
				{
					Gorgon.Stop();
					_logoDone = true;					
					return;
				}

				if (e.KeyCode == Forms.Keys.F2)
				{
					Gorgon.FrameStatsVisible = !Gorgon.FrameStatsVisible;
					return;
				}

				// Switch between windowed and fullscreen mode.
				if ((e.KeyCode == Forms.Keys.Enter) && (e.Alt))
				{
					Gorgon.Screen.Windowed = !Gorgon.Screen.Windowed;
					return;
				}
			}

			base.OnKeyDown(e);
		}

		/// <summary>
		/// Function to display the logo.
		/// </summary>
		protected virtual void RunLogo()
		{
            Drawing.Color previousBack;         // Previous background color.

			// Show the form.
			Show();

			Forms.Cursor.Hide();
			try
			{
				_logoTimer = new PreciseTimer();

				// Create the sprite logo.
				Image.FromResource("Tape_Worm_Logo", Properties.Resources.ResourceManager);
				_logoSprite = new Sprite("@LogoSprite", ImageCache.Images["Tape_Worm_Logo"]);
				Shader.FromResource("Blur", Properties.Resources.ResourceManager, false);
				_logoSprite.Shader = ShaderCache.Shaders["Blur"];
				_logoSprite.SetAxis(128, 128);

				// If we have a card with pixel shader 2.0 capability we're in for a treat.
				// Yes, that's sad, really.
				if (Gorgon.CurrentDriver.PixelShaderVersion >= new Version(2, 0))
				{
					_logoShader = _logoSprite.Shader;
					_logoShader.Parameters["sourceImage"].SetValue(_logoSprite.Image);
					_logoShader.Parameters["blurAmount"].SetValue(_logoBlur);
				}
				previousBack = Gorgon.Screen.BackgroundColor;
				Gorgon.Screen.BackgroundColor = Drawing.Color.Black;

				// Set up sprite.
				_logoSprite.Smoothing = Smoothing.Smooth;
				_logoSprite.UniformScale = (Gorgon.Screen.Width * 5.0f) / _logoSprite.Width;

				// Assign the logo.
				Gorgon.Idle += new FrameEventHandler(LogoRender);

				_logoTimer.Reset();
				Focus();
				Gorgon.Go();

				// Pause until the timeout is up.
				while (Gorgon.IsRunning)
					Gorgon.ProcessMessages();

				// Clean up.
				if ((Gorgon.IsInitialized) && (Gorgon.Screen != null))
				{
					Gorgon.Idle -= new FrameEventHandler(LogoRender);
					if (_logoShader != null)
						ShaderCache.Shaders[_logoSprite.Shader.Name].Dispose();
					ImageCache.Images["Tape_Worm_Logo"].Dispose();
					_logoShader = null;
					_logoTimer = null;
					_logoSprite = null;
					Gorgon.Screen.BackgroundColor = previousBack;
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Error trying to display the logo.", ex);
			}
			finally
			{
				Forms.Cursor.Show();
				// Clear out the message queue.
				Gorgon.ProcessMessages();
			}
		}
		#endregion

		#region Constructors/Destructors.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonApplicationWindow"/> class.
		/// </summary>
		/// <param name="configPath">The config file path.</param>
		protected GorgonApplicationWindow(string configPath) 
		{
			_configPath = configPath;
			Gorgon.LogoVisible = true;

			// Load the configuration.
			InitializeComponent();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonApplicationWindow"/> class.
		/// </summary>
		public GorgonApplicationWindow()
			: this(string.Empty)
		{
		}
		#endregion
	}
}

