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
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using GorgonLibrary.IO;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Configuration;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Configuration settings for the editor.
	/// </summary>
	public class GorgonEditorSettings
		: GorgonApplicationSettings
	{
		#region Variables.
		private float _animationRate = 0.25f;		// Animation rate.
		#endregion

		#region Properties.
        /// <summary>
        /// Property to set or return the directory that holds the plug-ins.
        /// </summary>
        [ApplicationSetting("PlugInDirectory", typeof(string), "MainApplication")]
        public string PlugInDirectory
        {
            get;
            set;
        }

		/// <summary>
		/// Property to set or return the main form state.
		/// </summary>
		[ApplicationSetting("FormState", FormWindowState.Maximized, typeof(FormWindowState), "MainApplication")]
		public FormWindowState FormState
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the window dimensions.
		/// </summary>
		[ApplicationSetting("WindowDimensions", typeof(Rectangle), "MainApplication")]
		public Rectangle WindowDimensions
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the path to the scratch location for temporary data.
		/// </summary>
		[ApplicationSetting("ScratchPath", typeof(string), "Options")]
		public string ScratchPath
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to animate the start page for the editor.
		/// </summary>
		[ApplicationSetting("StartPageAnimation", true, typeof(bool), "Options")]
		public bool AnimateStartPage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the rate of animation for the default start page.
		/// </summary>
		[ApplicationSetting("StartPageAnimationRate", 0.1f, typeof(float), "Options")]
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
        [ApplicationSetting("LastEditorFile", "", typeof(string), "MainApplication")]
        public string LastEditorFile
        {
            get;
            set;
        }

		/// <summary>
		/// Property to set or return the last import file path.
		/// </summary>
		[ApplicationSetting("LastImportFilePath", "", typeof(string), "MainApplication")]
		public string ImportLastFilePath
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the last export file path.
		/// </summary>
        [ApplicationSetting("LastExportFilePath", "", typeof(string), "MainApplication")]
		public string ExportLastFilePath
		{
			get;
			set;
		}

        /// <summary>
        /// Property to set or return the recent files.
        /// </summary>
        [ApplicationSetting("RecentFiles", typeof(IList<string>), "Recent")]
        public IList<string> RecentFiles
        {
            get;
            private set;
        }

        /// <summary>
        /// Property set or return whether to automatically load the last file opened by the editor on start up.
        /// </summary>
        [ApplicationSetting("AutoLoadLastFile", true, typeof(bool), "Options")]        
        public bool AutoLoadLastFile
        {
            get;
            set;
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonEditorSettings"/> class.
		/// </summary>
		internal GorgonEditorSettings()
			: base("Gorgon.Editor", new Version(1, 0))
		{
			// Set the path for the application settings.
			this.Path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).FormatDirectory(System.IO.Path.DirectorySeparatorChar) 
					  + "Tape_Worm".FormatDirectory(System.IO.Path.DirectorySeparatorChar)
					  + this.ApplicationName.FormatDirectory(System.IO.Path.DirectorySeparatorChar)
#if !DEBUG
					  + "Gorgon.Editor.config.xml";
#else
                      + "Gorgon.Editor.DEBUG.config.xml";
#endif

			Size baseSize = new Size(1280, 800);

			// Set the default size, but ensure that it fits within the primary monitor.
			// Do not go larger than 1280x800 by default.
			if (Screen.PrimaryScreen.WorkingArea.Width < 1280)
				baseSize.Width = Screen.PrimaryScreen.WorkingArea.Width;
			if (Screen.PrimaryScreen.WorkingArea.Height < 800)
				baseSize.Height = Screen.PrimaryScreen.WorkingArea.Height;
			
			WindowDimensions = new Rectangle(Screen.PrimaryScreen.WorkingArea.Width / 2 - baseSize.Width / 2, Screen.PrimaryScreen.WorkingArea.Height / 2 - baseSize.Height / 2, 1280, 800);

            RecentFiles = new List<string>();
            PlugInDirectory = Gorgon.ApplicationDirectory + "PlugIns";

			DriveInfo biggestDrive = null;		// Biggest writable drive on the system.

			// Find the drive with the most free space.
			var driveList = (from drive in DriveInfo.GetDrives()
							 where ((drive.DriveType == DriveType.Fixed) || (drive.DriveType == DriveType.Ram))
									&& (drive.AvailableFreeSpace >= 157286400) && (drive.IsReady)
							 orderby drive.AvailableFreeSpace descending
							 select drive);
			
			// Do not assume we can write to the root directory of the selected drive, fall back if we can't.
			if ((driveList != null) && (driveList.Count() > 0))
			{
				foreach (var drive in driveList)
				{
					try
					{
						// This function will fail if the root directory is read-only or we don't have
						// permission to read the rights on the directory.
						var accessControl = drive.RootDirectory.GetAccessControl();

						// We made it this far, so we're good.
						biggestDrive = drive;
						break;
					}
					catch (UnauthorizedAccessException)
					{
						// We can't access this drive for whatever reason, so we move on.						
					}
				}

                // If the drive is the same as the drive for our profile, then just stick the temp dir in there.
                if (biggestDrive != null) 
                {
                    var rootPath = System.IO.Path.GetPathRoot(biggestDrive.RootDirectory.Name);
                    var userPath = System.IO.Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                                        
                    if (rootPath == userPath)
                    {
                        biggestDrive = null;
                    }
                }
			}
			else
			{
				throw new System.IO.IOException("There are no drives on the system with enough free space.  The Gorgon Editor requires a minimum of 150 MB of free space for temporary files.");
			}

			// Can't find a writable root directory for some reason, so make do with the user directory (if that's not writable, then you're shit out of luck).
			if (biggestDrive == null)
			{
				// Get the default scratch location.
				ScratchPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).FormatDirectory(System.IO.Path.DirectorySeparatorChar)
								+ "Tape_Worm".FormatDirectory(System.IO.Path.DirectorySeparatorChar)
								+ this.ApplicationName.FormatDirectory(System.IO.Path.DirectorySeparatorChar)
								+ "Gorgon.Editor." + Guid.NewGuid().ToString("N").FormatDirectory(System.IO.Path.DirectorySeparatorChar);
			}
			else
			{
				ScratchPath = biggestDrive.RootDirectory.Name + "Gorgon.Editor." + Guid.NewGuid().ToString("N").FormatDirectory(System.IO.Path.DirectorySeparatorChar);
			}
		}
		#endregion
	}
}
