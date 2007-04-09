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

using System;
using Forms = System.Windows.Forms;
using System.Reflection;
using System.Configuration;
using System.ComponentModel;
using SharpUtilities.Mathematics;
using Drawing = System.Drawing;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Shaders;
using GorgonLibrary.Timing;
using GorgonLibrary.FileSystems;

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
		private string _resourcePath = string.Empty;    // Path to the application resources.		
        private SetupDialog _setup = null;              // Setup dialog.
		private bool _useDepthBuffer = false;			// Flag to use a depth buffer or not.
		private bool _useStencilBuffer = false;			// Flag to use a stencil buffer or not.
		private string _plugInPath = string.Empty;		// Path to plug-ins.
		private Forms.Control _control = null;			// Control used for the main render window.
		private int _pluginCount = 0;					// Number of plug-ins available.
		private PreciseTimer _logoTimer = null;			// Logo timer.
		private Sprite _logoSprite = null;				// Logo sprite.
		private float _logoOpacity = 0;					// Logo opacity.
		private float _logoBlur = _maxBlur;				// Blur factor.
		private bool _logoFadeDir = false;				// Logo fade direction.
		private Shader _logoShader = null;				// Shader for the logo.
		private bool _logoDone = false;					// Logo is done animation?
		#endregion

        #region Properties.
		/// <summary>
		/// Property to set or return the main render window.
		/// </summary>
		protected Forms.Control RenderWindow
		{
			get
			{
				return _control;
			}
			set
			{
				if (value == null)
					value = this;
				_control = value;
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
				return _pluginCount;
			}
		}
		#endregion

		#region Methods.
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
        /// <param name="e">The <see cref="GorgonLibrary.Graphics.FrameEventArgs"/> instance containing the event data.</param>
		private void OnLogoBegin(object sender, FrameEventArgs e)
		{
			BlurLogo((float)(e.CurrentTarget.TimingData.FrameDrawTime / 1000));

			_logoSprite.SetPosition(Gorgon.VideoMode.Width / 2, Gorgon.VideoMode.Height / 2);
			_logoSprite.Draw();

			if (_logoTimer.Seconds > 7.25)
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
			int count = 0;							// Plug-in count.
			string plugInPath = string.Empty;		// Plug-in path.

			count = Convert.ToInt32(ConfigurationManager.AppSettings["PlugInCount"]);

			// Load each plug-in.
			for (int i = 0; i < count; i++)
			{
				if ((ConfigurationManager.AppSettings["PlugIn_Enabled_" + i.ToString()] == null) || (ConfigurationManager.AppSettings["PlugIn_Enabled_" + i.ToString()] != "0"))
				{
					plugInPath = _plugInPath + ConfigurationManager.AppSettings["PlugIn_" + i.ToString()];
					Gorgon.PlugIns.Load(plugInPath);
				}
			}
	
			_pluginCount = count;
		}

        /// <summary>
        /// Function to load the file systems.
        /// </summary>
        private void LoadFilesystems()
        {
            int count = 0;                      // File system count.
            string fsPath = string.Empty;       // File system path.
            string fsName = string.Empty;       // File system name.
            string fsRoot = string.Empty;       // File system root.
			FileSystemPlugIn fsPlugIn = null;	// File system plug-in.

            count = Convert.ToInt32(ConfigurationManager.AppSettings["FileSystemCount"]);

            // Load each file system.
            for (int i = 0; i < count; i++)
            {
                if ((ConfigurationManager.AppSettings["FileSystem_Enabled_" + i.ToString()] == null) || (ConfigurationManager.AppSettings["FS_Enabled_" + i.ToString()] != "0"))
                {
                    fsPath = _plugInPath + ConfigurationManager.AppSettings["FileSystem_" + i.ToString()];
                    fsName = ConfigurationManager.AppSettings["FileSystem_Name_" + i.ToString()];
                    fsRoot = ConfigurationManager.AppSettings["FileSystem_Root_" + i.ToString()];

                    // Load the file system.
                    fsPlugIn = Gorgon.FileSystems.LoadPlugIn(fsPath);

					Gorgon.FileSystems.Create(fsName, fsPlugIn);
					if ((fsRoot != string.Empty) && (fsRoot != null))
						Gorgon.FileSystems[fsName].RootPath = fsRoot;
                }
            }
        }

        /// <summary>
        /// Function to do initialization.
        /// </summary>
        protected virtual void Initialize()
		{
		}		

		/// <summary>
		/// Function called on the end of a frame.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Graphics.FrameEventArgs"/> instance containing the event data.</param>
		protected virtual void OnFrameEnd(object sender, FrameEventArgs e)
		{
		}

		/// <summary>
		/// Function called on the beginning of a frame.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Graphics.FrameEventArgs"/> instance containing the event data.</param>
		protected virtual void OnFrameBegin(object sender, FrameEventArgs e)
		{
		}

		/// <summary>
		/// Function called when the device is reset.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Graphics.FrameEventArgs"/> instance containing the event data.</param>
		protected virtual void OnReset(object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Function called when the device is lost.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void OnLost(object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(Forms.FormClosingEventArgs e)
		{
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

			if ((path == null) || (path == string.Empty))
				result = @".\";
			else
				result = path;

			if (result[result.Length - 1] != '\\')
				result += @"\";

			return result;
		}

		/// <summary>
		/// Function to display the logo.
		/// </summary>
		protected virtual void RunLogo()
		{
            Drawing.Color previousBack;         // Previous background color.
			SpriteManager sprites;				// Sprite manager.

			// Show the form.
			Show();

			_logoTimer = new PreciseTimer();

			// TODO: Remove this when things are finalized.
#if CREATE_LOGO
			Gorgon.Images.FromResource("Tape_Worm_Logo", Properties.Resources.ResourceManager);
			Gorgon.Shaders.FromResource("Blur", Properties.Resources.ResourceManager);
			_logoSprite = Gorgon.Sprites.Create("@LogoSprite", Gorgon.Images["Tape_Worm_Logo"]);
			_logoSprite.Shader = Gorgon.Shaders["Blur"];
			_logoSprite.SetAxis(128, 128);
			_logoSprite.Save(@"d:\unpak\logo.gorSprite");
#else
			// Create a sprite manager and load the sprite.
			sprites = new SpriteManager();
			_logoSprite = sprites.FromResource("logo", Properties.Resources.ResourceManager);
#endif

			// If we have a card with pixel shader 2.0 capability we're in for a treat.
			// Yes, that's sad, really.
			if (Gorgon.Driver.PixelShaderVersion >= new Version(2, 0))
			{
				_logoShader = _logoSprite.Shader;
				_logoShader.Parameters["sourceImage"].SetValue(_logoSprite.Image);
				_logoShader.Parameters["blurAmount"].SetValue(_logoBlur);
			}
            previousBack = Gorgon.Screen.BackgroundColor;
			Gorgon.Screen.BackgroundColor = Drawing.Color.Black;

			// Set up sprite.
			_logoSprite.Smoothing = Smoothing.Smooth;
			_logoSprite.UniformScale = (Gorgon.Screen.Width * 5.0f)/_logoSprite.Width;

			// Assign the logo.
			Gorgon.Screen.OnFrameBegin += new FrameEventHandler(OnLogoBegin);

			_logoTimer.Reset();
			Focus();			
			Gorgon.Go();

			// Pause until the timeout is up.
			while (Gorgon.IsRunning)
				Gorgon.ProcessMessages();

			// Clean up.
			if (Gorgon.Screen != null)
			{				
				Gorgon.Screen.OnFrameBegin -= new FrameEventHandler(OnLogoBegin);
				if (_logoShader != null)
					Gorgon.Shaders.Remove(_logoSprite.Shader.Name);
				Gorgon.ImageManager.Remove("tape_worm_logo");
				_logoShader = null;
				_logoTimer = null;
				_logoSprite = null;
				Gorgon.Screen.BackgroundColor = previousBack;
			}            

			// Clear out the message queue.
			Gorgon.ProcessMessages();
		}
        
		/// <summary>
		/// Function to do setup.
		/// </summary>
		/// <returns>FALSE if cancelled, TRUE if not.</returns>
		public virtual bool Setup()
		{
			bool cancel;			// Cancel flag.			

            // Get the resource path.
            _resourcePath = ConfigurationManager.AppSettings["ResourcePath"];

            // If null, make sure it defaults to the current directory.
			_resourcePath = ValidatePath(_resourcePath);
			_resourcePath += Assembly.GetCallingAssembly().GetName().Name + @"\";

			// Get plug-in path.
			_plugInPath = ConfigurationManager.AppSettings["PlugInPath"];

			// If null, make sure it defaults to the current directory.
			_plugInPath = ValidatePath(_plugInPath);
#if DEBUG
			_plugInPath += @"Debug\";
#else
			_plugInPath += @"Release\";
#endif
		
			// Create new gorgon object.
			Gorgon.Initialize(Gorgon.AllowScreenSaver, Gorgon.AllowBackgroundRendering);
			
			// Call up configuration options.
			_setup = new SetupDialog();
			
			if (_setup.Run(this) == Forms.DialogResult.OK)
			{
				Gorgon.AllowScreenSaver = _setup.AllowScreensaver;
				Gorgon.AllowBackgroundRendering = _setup.AllowBackgroundRendering;

                // Load any file systems.
                LoadFilesystems();

				// Begin loading plug-ins.
				LoadPlugins();                

				// Create new gorgon object.
				Gorgon.Driver = _setup.VideoDriver;
				Gorgon.SetMode(_control, _setup.VideoMode, _setup.WindowedFlag, 60, true, true,_setup.VSyncInterval);

				// Run the logo.				
				RunLogo();

				// Just quit if we haven't got a target.
				if (Gorgon.Screen == null)
					return false;

				// Initialize.
				Initialize();

				// Do on-idle.
				Gorgon.OnDeviceLost += new EventHandler(OnLost);
				Gorgon.OnDeviceReset += new EventHandler(OnReset);
				Gorgon.Screen.OnFrameBegin += new FrameEventHandler(OnFrameBegin);
				Gorgon.Screen.OnFrameEnd += new FrameEventHandler(OnFrameEnd);								
				cancel = true;				
			}
			else 
			{
				Gorgon.Terminate();
				cancel = false;
			}

			// Clean up.
			_setup.Dispose();
			_setup = null;

			return cancel;
		}
		#endregion

		#region Constructors/Destructors.
		/// <summary>
		/// Constructor.
		/// </summary>
		public GorgonApplicationWindow() 
		{
			// Default to this window.
			_control = this;
            InitializeComponent();            
		}
		#endregion
	}
}
