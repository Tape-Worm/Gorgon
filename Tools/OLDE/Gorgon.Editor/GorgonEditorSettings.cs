#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, May 02, 2012 10:30:29 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Configuration;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Editor
{
	/// <summary>
	/// Configuration settings for the editor.
	/// </summary>
	public sealed class GorgonEditorSettings
		: GorgonApplicationSettings
	{
		#region Variables.
        // Animation rate.
		private float _animationRate = 0.25f;
        // List of plug-ins disabled by the user.
	    private List<string> _userDisabledPlugIns;          
        #endregion

		#region Properties.
        /// <summary>
        /// Property to set or return the directory that holds the plug-ins.
        /// </summary>
        [GorgonApplicationSetting("PlugInDirectory", typeof(string), "MainApplication")]
        public string PlugInDirectory
        {
            get;
            set;
        }

		/// <summary>
		/// Property to set or return the main form state.
		/// </summary>
		[GorgonApplicationSetting("FormState", FormWindowState.Maximized, typeof(FormWindowState), "MainApplication")]
		public FormWindowState FormState
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the window dimensions.
		/// </summary>
		[GorgonApplicationSetting("WindowDimensions", typeof(Rectangle), "MainApplication")]
		public Rectangle WindowDimensions
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the path to the scratch location for temporary data.
		/// </summary>
		[GorgonApplicationSetting("ScratchPath", typeof(string), "Options")]
		public string ScratchPath
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the logo on the starting page should be animated.
		/// </summary>
		[GorgonApplicationSetting("AnimateStartPageLogo", true, typeof(bool), "Options")]
		public bool AnimateStartPageLogo
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the rate of animation for the default start page.
		/// </summary>
		[GorgonApplicationSetting("StartPageAnimationRate", 0.1f, typeof(float), "Options")]
		public float StartPageAnimationPulseRate
		{
			get
			{
				return _animationRate;
			}
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				if (value > 1)
				{
					value = 1;
				}

				_animationRate = value;
			}
		}

        /// <summary>
        /// Property to set or return the path to the last editor file.
        /// </summary>
        [GorgonApplicationSetting("LastEditorFile", "", typeof(string), "MainApplication")]
        public string LastEditorFile
        {
            get;
            set;
        }

		/// <summary>
		/// Property to set or return the last import file path.
		/// </summary>
		[GorgonApplicationSetting("LastImportFilePath", "", typeof(string), "MainApplication")]
		public string ImportLastFilePath
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the last export file path.
		/// </summary>
        [GorgonApplicationSetting("LastExportFilePath", "", typeof(string), "MainApplication")]
		public string ExportLastFilePath
		{
			get;
			set;
		}

        /// <summary>
        /// Property to set or return the recent files.
        /// </summary>
        [GorgonApplicationSetting("RecentFiles", typeof(IList<string>), "Recent")]
        public IList<string> RecentFiles
        {
            get;
            private set;
        }

		/// <summary>
		/// Property to return the list of user disabled plug-ins.
		/// </summary>
		[GorgonApplicationSetting("DisabledPlugIns", typeof(IList<string>), "PlugIns")]
		public IList<string> DisabledPlugIns
		{
		    get
		    {
		        return _userDisabledPlugIns;
		    }
		    set
		    {
		        _userDisabledPlugIns.Clear();

		        if ((value == null)
                    || (value.Count == 0))
		        {
		            return;
		        }

                _userDisabledPlugIns.AddRange(value);
		    }
		}

        /// <summary>
        /// Property to set or return the default image editor plug-in to use when handling images in other plug-ins.
        /// </summary>
        [GorgonApplicationSetting("DefaultImageEditorPlugIn", typeof(string), "")]
	    public string DefaultImageEditor
	    {
	        get;
	        set;
	    }

            /// <summary>
        /// Property set or return whether to automatically load the last file opened by the editor on start up.
        /// </summary>
        [GorgonApplicationSetting("AutoLoadLastFile", true, typeof(bool), "Options")]        
        public bool AutoLoadLastFile
        {
            get;
            set;
        }
		#endregion

		#region Methods.
		/// <summary>
		/// Function to find the best drive on the computer for use as temporary storage.
		/// </summary>
		/// <returns>The root directory of the best drive on the system.</returns>
		private static DirectoryInfo GetBestDrive()
		{
			DriveInfo biggestDrive = null;		// Biggest writable drive on the system.

			// Find the drive with the most free space.
			var driveList = (from drive in DriveInfo.GetDrives()
							 where ((drive.DriveType == DriveType.Fixed) || (drive.DriveType == DriveType.Ram))
									&& (drive.AvailableFreeSpace >= 157286400) && (drive.IsReady)
							 orderby drive.AvailableFreeSpace descending
							 select drive);

			// Do not assume we can write to the root directory of the selected drive, fall back if we can't.
			if (driveList.Any())
			{
				foreach (var drive in driveList)
				{
					try
					{
						// This function will fail if the root directory is read-only or we don't have
						// permission to read the rights on the directory.
						drive.RootDirectory.GetAccessControl();

						// We made it this far, so we're good.
						biggestDrive = drive;
						break;
					}
					catch (UnauthorizedAccessException)
					{
						// We can't access this drive for whatever reason, so we move on.						
					}
				}

				if (biggestDrive == null)
				{
					throw new IOException(Resources.GOREDIT_ERR_NO_DRIVE_AVAILABLE);
				}
			}
			else
			{
				throw new IOException(Resources.GOREDIT_ERR_NO_DRIVE_AVAILABLE);
			}
			
			return biggestDrive.RootDirectory;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonEditorSettings"/> class.
		/// </summary>
		internal GorgonEditorSettings()
			: base("Gorgon.Editor", new Version(1, 0))
		{
			AnimateStartPageLogo = true;
			StartPageAnimationPulseRate = 0.125f;

			// Set the path for the application settings.
			Path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).FormatDirectory(System.IO.Path.DirectorySeparatorChar) 
					  + "Tape_Worm".FormatDirectory(System.IO.Path.DirectorySeparatorChar)
					  + ApplicationName.FormatDirectory(System.IO.Path.DirectorySeparatorChar)
#if !DEBUG
					  + "Gorgon.Editor.config.xml";
#else
                      + "Gorgon.Editor.DEBUG.config.xml";
#endif

			var baseSize = new Size(1280, 800);

			// Set the default size, but ensure that it fits within the primary monitor.
			// Do not go larger than 1280x800 by default.
			if (Screen.PrimaryScreen.WorkingArea.Width < 1280)
				baseSize.Width = Screen.PrimaryScreen.WorkingArea.Width;
			if (Screen.PrimaryScreen.WorkingArea.Height < 800)
				baseSize.Height = Screen.PrimaryScreen.WorkingArea.Height;
			
			WindowDimensions = new Rectangle(Screen.PrimaryScreen.WorkingArea.Width / 2 - baseSize.Width / 2, Screen.PrimaryScreen.WorkingArea.Height / 2 - baseSize.Height / 2, 1280, 800);

            RecentFiles = new List<string>();
			_userDisabledPlugIns = new List<string>();
            PlugInDirectory = GorgonApplication.StartupPath + "PlugIns";

			// Set the default scratch location.
			ScratchPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
									 .FormatDirectory(System.IO.Path.DirectorySeparatorChar)
						  + "Tape_Worm".FormatDirectory(System.IO.Path.DirectorySeparatorChar)
						  + ApplicationName.FormatDirectory(System.IO.Path.DirectorySeparatorChar);

			var dirInfo = new DirectoryInfo(ScratchPath);

			if (!dirInfo.Exists)
			{
				return;
			}

			try
			{
				// Attempt to find the directory and write to it.
				dirInfo.GetAccessControl();
			}
			catch (UnauthorizedAccessException)
			{
				ScratchPath = GetBestDrive().Name;
			}
		}
		#endregion
	}
}
