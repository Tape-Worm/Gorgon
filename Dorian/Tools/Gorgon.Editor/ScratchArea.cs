#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Sunday, September 22, 2013 7:33:11 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.IO;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Accessibility for the scratch area.
	/// </summary>
	enum ScratchAccessibility
	{
		/// <summary>
		/// Scratch area is accessible and writable.
		/// </summary>
		Accessible = 0,
		/// <summary>
		/// Scratch is pointed at a system area.
		/// </summary>
		SystemArea = 1,
		/// <summary>
		/// Scratch area is read only or there are insufficient permissions to the scratch area.
		/// </summary>
		CannotWrite = 2,
		/// <summary>
		/// When changing the scratch area, this indicates that the user canceled the selection.
		/// </summary>
		Canceled = 3
	}

	/// <summary>
	/// The scratch area interface.
	/// </summary>
	/// <remarks>This interface is used to maintain the scratch area.</remarks>
	static class ScratchArea
	{
        #region Constants.
        /// <summary>
        /// Meta data file name.
        /// </summary>
        public const string MetaDataFile = ".gorgon.editor.metadata";
        /// <summary>
        /// Meta data file path.
        /// </summary>
        public const string MetaDataFilePath = "/" + MetaDataFile;
        #endregion

		#region Variables.
        private readonly static XElement _metaDataRootNode = new XElement("Gorgon.Editor.MetaData");
        private readonly static string[] _systemDirs;
	    private static XDocument _metaDataXML;
        private static Guid _scratchID = Guid.NewGuid();
		#endregion

		#region Properties.
        /// <summary>
		/// Property to return the file system for the scratch files.
		/// </summary>
		public static GorgonFileSystem ScratchFiles
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if the path is a root path or system location.
		/// </summary>
		/// <param name="path">Path to evaluate.</param>
		/// <returns>TRUE if a system location or root directory.</returns>
		private static bool IsSystemLocation(string path)
		{
			var sysRoot = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
			var info = new DirectoryInfo(Path.GetFullPath(path));

			// Don't allow the root of the system drive as a scratch area.
			if ((info.Parent == null) && (string.Equals(sysRoot, info.FullName, StringComparison.OrdinalIgnoreCase)))
			{
				return true;
			}

			// Ensure the system files are not accessible.
			return (_systemDirs.Any(item => string.Equals(path, item, StringComparison.OrdinalIgnoreCase)));
		}

        /// <summary>
        /// Function to retrieve the meta data for the scratch area files.
        /// </summary>
        /// <returns>The XML document containing the metadata, or NULL (Nothing in VB.Net) if no meta data was found.</returns>
	    public static XDocument GetMetaData()
        {
            if (_metaDataXML != null)
            {
                return _metaDataXML;
            }

            _metaDataXML = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), _metaDataRootNode);

            // If we have no files, then there's no metadata.
            if ((ScratchFiles == null)
                || ((ScratchFiles.RootDirectory.Directories.Count == 0)
                    && (ScratchFiles.RootDirectory.Files.Count == 0)))
            {
                return _metaDataXML;
            }

            var file = ScratchFiles.GetFile(MetaDataFilePath);

            if (file == null)
            {
                return _metaDataXML;
            }

            XDocument metaDataFile;

            using(var metaStream = file.OpenStream(false))
            {
                metaDataFile = XDocument.Load(metaStream);
            }

            // If this file is invalid, then do not return it.
            if (!metaDataFile.Descendants(_metaDataRootNode.Name).Any())
            {
                return _metaDataXML;
            }

            _metaDataXML = metaDataFile;

            return _metaDataXML;
        }

		/// <summary>
		/// Function to remove all old scratch area directories from the scratch path.
		/// </summary>
		public static void CleanOldScratchAreas()
		{
			if (string.IsNullOrWhiteSpace(Program.Settings.ScratchPath))
			{
				return;
			}

			var scratchInfo = new DirectoryInfo(Program.Settings.ScratchPath);

			if (!scratchInfo.Exists)
			{
				return;
			}

			// Get all the directories in the scratch area.
			var directories = scratchInfo.GetDirectories("Gorgon.Editor.*", SearchOption.TopDirectoryOnly)
				.Where(item => (((item.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
					&& ((item.Attributes & FileAttributes.NotContentIndexed) == FileAttributes.NotContentIndexed)));

			foreach (var directory in directories)
			{
				try
				{
					directory.Delete(true);
				}
				catch (Exception ex)
				{
					// If we can't delete, then something else is amiss and we'll have to try again later.
					// So just eat the exception and move on to the next directory.
					GorgonException.Catch(ex);
				}
			}

            _metaDataXML = null;
		    GetMetaData();
		}

		/// <summary>
		/// Function to destroy the current scratch area.
		/// </summary>
		/// <returns>TRUE if the clean up operation was successful, FALSE if not.</returns>
		public static bool DestroyScratchArea()
		{
			if (Program.Settings == null)
			{
				throw new ApplicationException(Resources.GOREDIT_NO_APP_SETTINGS);
			}

			try
			{
				var scratchLocation = new StringBuilder();

				scratchLocation.Append(Program.Settings.ScratchPath);
				scratchLocation.Append(_scratchID.ToString("N").FormatDirectory(Path.DirectorySeparatorChar));

				// Use the write location of the currently mounted file system if possible.
				if ((ScratchFiles != null) && (!string.IsNullOrWhiteSpace(ScratchFiles.WriteLocation)))
				{
					ScratchFiles.Clear();

					scratchLocation.Length = 0;
					scratchLocation.Append(ScratchFiles.WriteLocation);
				}

				string scratchPath = scratchLocation.ToString();

				// The scratch directory is gone, nothing to clean up.
				if (!Directory.Exists(scratchPath))
				{
					return true;
				}

				Program.LogFile.Print("Cleaning up scratch area at \"{0}\".", LoggingLevel.Simple, scratchPath);

				var directory = new DirectoryInfo(scratchPath);

				// Wipe out everything in this directory and the directory proper.				
				directory.Delete(true);

                // This will refresh the metadata.
			    _metaDataXML = null;
			    GetMetaData();
			}
			catch (Exception ex)
			{
				GorgonException.Log = Program.LogFile;
				GorgonException.Catch(ex);
				GorgonException.Log = Gorgon.Log;

				return false;
			}

			return true;
		}

		/// <summary>
		/// Function to allow the user to select a new scratch file location.
		/// </summary>
		/// <returns>A code indicating the accessibility to the scratch area on the drive.</returns>
		public static ScratchAccessibility SetScratchLocation()
		{
			FolderBrowserDialog dialog = null;

			try
			{
				dialog = new FolderBrowserDialog
				{
					Description = Resources.GOREDIT_SCRATCH_PATH_DLG_DESC
				};

				if (Directory.Exists(Program.Settings.ScratchPath))
				{
					dialog.SelectedPath = Program.Settings.ScratchPath;
				}
				dialog.ShowNewFolderButton = true;

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return ScratchAccessibility.Canceled;
				}

				// We chose poorly.
				string selectedPath = dialog.SelectedPath.FormatDirectory(Path.DirectorySeparatorChar);
				if (!IsSystemLocation(selectedPath))
				{
					Program.Settings.ScratchPath = selectedPath;
					return ScratchAccessibility.Accessible;
				}

				GorgonDialogs.ErrorBox(null, Resources.GOREDIT_CANNOT_USESYS_SCRATCH);

				return ScratchAccessibility.SystemArea;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
				return ScratchAccessibility.CannotWrite;
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
		/// Function to determine if the scratch area is accessible.
		/// </summary>
		/// <param name="scratchPath">Path to the scratch area.</param>
		/// <returns>A code indicating the level of accessibility to the scratch area on the drive.</returns>
		public static ScratchAccessibility CanAccessScratch(string scratchPath)
		{
			string directoryName = Path.GetFullPath(scratchPath).FormatDirectory(Path.DirectorySeparatorChar);

			// Ensure that the device exists or is ready.
			var root = Path.GetPathRoot(directoryName);

			if ((string.IsNullOrWhiteSpace(root)) || !Directory.Exists(root))
			{
				return ScratchAccessibility.CannotWrite;
			}

			try
			{
				// Do not allow access to a system location.
				if (IsSystemLocation(directoryName))
				{
					return ScratchAccessibility.SystemArea;
				}

				var directoryInfo = new DirectoryInfo(directoryName);
				if (!directoryInfo.Exists)
				{
					// If we created the directory, then hide it.
					directoryInfo.Create();
					return ScratchAccessibility.Accessible;
				}

				directoryInfo.GetAccessControl();

				// Ensure that we can actually write to this directory.
				var testWrite = new FileInfo(directoryName + "TestWrite.tst");
				using (Stream stream = testWrite.OpenWrite())
				{
					stream.WriteByte(127);
				}
				testWrite.Delete();
			}
			catch (Exception ex)
			{
				GorgonException.Log = Program.LogFile;
				GorgonException.Catch(ex);
				GorgonException.Log = Gorgon.Log;
				return ScratchAccessibility.CannotWrite;
			}

			return ScratchAccessibility.Accessible;
		}

		/// <summary>
		/// Function to intialize the scratch area.
		/// </summary>
		public static void InitializeScratch()
		{
			_scratchID = Guid.NewGuid();
			ScratchFiles.Clear();
			ScratchFiles.WriteLocation = Program.Settings.ScratchPath + ("Gorgon.Editor." + _scratchID.ToString("N")).FormatDirectory(Path.DirectorySeparatorChar);

			// Set the directory to hidden.  We don't want users really messing around in here if we can help it.
			var scratchDir = new DirectoryInfo(ScratchFiles.WriteLocation);
			if (((scratchDir.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
				|| ((scratchDir.Attributes & FileAttributes.NotContentIndexed) != FileAttributes.NotContentIndexed))
			{
				scratchDir.Attributes = FileAttributes.NotContentIndexed | FileAttributes.Hidden;
			}

		    _metaDataXML = null;
		    GetMetaData();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="ScratchArea"/> class.
		/// </summary>
		static ScratchArea()
		{
			_systemDirs = ((Environment.SpecialFolder[])Enum.GetValues(typeof(Environment.SpecialFolder)))
							.Select(Environment.GetFolderPath)
							.ToArray();
			ScratchFiles = new GorgonFileSystem();

            // Create default metadata.
            _metaDataXML = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), _metaDataRootNode);
		}
		#endregion
	}
}
