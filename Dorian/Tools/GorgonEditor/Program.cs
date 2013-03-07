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
// Created: Monday, April 30, 2012 6:28:28 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using GorgonLibrary;
using GorgonLibrary.IO;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.UI;
using GorgonLibrary.FileSystem;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Main application interface.
	/// </summary>
	static class Program
	{
		#region Properties.
		/// <summary>
		/// Property to return the logging interface for the application.
		/// </summary>
		public static GorgonLogFile LogFile
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the file system for the scratch files.
		/// </summary>
		public static GorgonFileSystem ScratchFiles
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the currently loaded content.
		/// </summary>
		public static ContentObject CurrentContent
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to set or return the settings for the application.
		/// </summary>
		public static GorgonEditorSettings Settings
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the list of cached fonts on the system.
		/// </summary>
		public static IDictionary<string, Font> CachedFonts
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the graphics interface.
		/// </summary>
		public static GorgonGraphics Graphics
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to allow the user to select a new scratch file location.
		/// </summary>
		/// <returns>The new path for the scratch data, or NULL if canceled.</returns>
		public static string SetScratchLocation()
		{
			FolderBrowserDialog dialog = null;

			try
			{
				dialog = new FolderBrowserDialog();
				dialog.Description = "Select a new temporary scratch path.";
				if (Directory.Exists(Settings.ScratchPath))
				{
					dialog.SelectedPath = Settings.ScratchPath;
				}
				dialog.ShowNewFolderButton = true;

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					// Append the scratch directory.
					return dialog.SelectedPath.FormatDirectory(Path.DirectorySeparatorChar) + "Gorgon.Editor." + Guid.NewGuid().ToString("N").FormatDirectory(Path.DirectorySeparatorChar);
				}

				return null;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
				return null;
			}
			finally
			{
				if (dialog != null)
				{
					dialog.Dispose();
				}
			}
		}

		/// <summary>
		/// Function to clean up the scratch area when the program exits.
		/// </summary>
		public static void CleanUpScratchArea()
		{
			if (Program.Settings == null) 
			{
				throw new ApplicationException("The application settings were not loaded.");
			}

			// The scratch directory is gone, nothing to clean up.
			if (!Directory.Exists(Settings.ScratchPath))
			{
				return;
			}

			try
			{
				LogFile.Print("Cleaning up scratch area at \"{0}\".", LoggingLevel.Simple, Settings.ScratchPath);

				DirectoryInfo directory = new DirectoryInfo(Settings.ScratchPath);

				// Wipe out everything in this directory and the directory proper.
				directory.Delete(true);
			}
			catch (Exception ex)
			{
				GorgonException.Log = LogFile;
				GorgonException.Catch(ex);
				GorgonException.Log = Gorgon.Log;
			}
		}

		/// <summary>
		/// Function to initialize the graphics interface.
		/// </summary>
		/// <param name="control">Main content control.</param>
		public static void InitializeGraphics()
		{
			if (Graphics != null)
			{
				Graphics.Dispose();
				Graphics = null;
			}

			Graphics = new GorgonGraphics(DeviceFeatureLevel.SM2_a_b);			
		}

		/// <summary>
		/// Function to update the font cache.
		/// </summary>
		static void UpdateCachedFonts()
		{
			SortedDictionary<string, Font> fonts = null;

			// Clear the cached fonts.
			if (CachedFonts != null)
			{
				foreach (var font in CachedFonts)
					font.Value.Dispose();
			}

			fonts = new SortedDictionary<string,Font>();

			// Get font families.
			foreach (var family in FontFamily.Families)
			{
				Font newFont = null;

				if (!fonts.ContainsKey(family.Name))
				{
					if (family.IsStyleAvailable(FontStyle.Regular))
						newFont = new Font(family, 16.0f, FontStyle.Regular, GraphicsUnit.Pixel);
					else
					{
						if (family.IsStyleAvailable(FontStyle.Bold))
							newFont = new Font(family, 16.0f, FontStyle.Bold, GraphicsUnit.Pixel);
						else
						{
							if (family.IsStyleAvailable(FontStyle.Italic))
								newFont = new Font(family, 16.0f, FontStyle.Italic, GraphicsUnit.Pixel);
						}
					}

					// Only add if we could use the regular, bold or italic style.
					if (newFont != null)
						fonts.Add(family.Name, newFont);
				}
			}

			CachedFonts = fonts;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="Program"/> class.
		/// </summary>
		static Program()
		{
			Settings = new GorgonEditorSettings();
		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				Settings.Load();
								
				Gorgon.Run(new AppContext());
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
			}
			finally
			{
				// Destroy the current content.
				if (CurrentContent != null)
				{
					CurrentContent.Dispose();
					CurrentContent = null;
				}

				// Shut down the graphics interface.
				if (Graphics != null)
				{
					Graphics.Dispose();
					Graphics = null;
				}

				// Clean up temporary files in scratch area.
				if (Program.Settings != null)
				{
					CleanUpScratchArea();
				}

				// Close the logging file.
				if (Program.LogFile != null)
				{
					Program.LogFile.Close();
				}

				// Clear the cached fonts.
				if (CachedFonts != null)
				{
					foreach (var font in CachedFonts)
						font.Value.Dispose();
				}
			}
		}
	}
}
