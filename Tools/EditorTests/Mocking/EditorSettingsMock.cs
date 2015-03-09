using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GorgonLibrary.Editor;
using GorgonLibrary.IO;

namespace EditorTests.Mocking
{
	/// <summary>
	/// Mock object for editor settings.
	/// </summary>
	public class EditorSettingsMock
		: IEditorSettings
	{
		#region Variables.
		// The scratch area location.

		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorSettingsMock"/> class.
		/// </summary>
		public EditorSettingsMock()
		{
			ScratchPath = @"A:\scratch_path\";
		}
		#endregion

		#region IEditorSettings Members
		/// <summary>
		/// Property to set or return the directory that holds the plug-ins.
		/// </summary>
		public string PlugInDirectory
		{
			get
			{
				return string.Empty;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the directory that holds theme files.
		/// </summary>
		public string ThemeDirectory
		{
			get
			{
				return string.Empty;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the main form state.
		/// </summary>
		public System.Windows.Forms.FormWindowState FormState
		{
			get
			{
				return FormWindowState.Normal;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the window dimensions.
		/// </summary>
		public System.Drawing.Rectangle WindowDimensions
		{
			get
			{
				return new Rectangle(0, 0, 100, 100);
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the path to the scratch location for temporary data.
		/// </summary>
		/// <remarks>
		/// This value will check and format itself appropriately for directory paths.
		/// </remarks>
		public string ScratchPath
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the logo on the starting page should be animated.
		/// </summary>
		public bool AnimateStartPageLogo
		{
			get
			{
				return true;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the rate of animation for the default start page.
		/// </summary>
		public float StartPageAnimationPulseRate
		{
			get
			{
				return 1.0f;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the path to the last editor file.
		/// </summary>
		public string LastEditorFile
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the last import file path.
		/// </summary>
		public string ImportLastFilePath
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the last export file path.
		/// </summary>
		public string ExportLastFilePath
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the recent files.
		/// </summary>
		public IList<string> RecentFiles
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Property to return the list of user disabled plug-ins.
		/// </summary>
		public IList<string> DisabledPlugIns
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Property to set or return the default image editor plug-in to use when handling images in other plug-ins.
		/// </summary>
		public string DefaultImageEditor
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property set or return whether to automatically load the last file opened by the editor on start up.
		/// </summary>
		public bool AutoLoadLastFile
		{
			get
			{
				return false;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to return the name of the application that the settings are from.
		/// </summary>
		public string ApplicationName
		{
			get
			{
				return "Mock";
			}
		}

		/// <summary>
		/// Property to set or return the path to the configuration file.
		/// </summary>
		public string Path
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the application settings version.
		/// </summary>
		/// <remarks>
		/// Assigning NULL (Nothing in VB.Net) will bypass version checking.
		/// </remarks>
		public Version Version
		{
			get
			{
				return new Version(1, 0, 0, 0);
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the position of the splitter on the main form.
		/// </summary>
		public int SplitPosition
		{
			get;
			set;
		}

		/// <summary>
		/// Function to save the settings to a file.
		/// </summary>
		/// <remarks>
		/// No versioning will be applied to the settings file when the <see cref="P:GorgonLibrary.Configuration.GorgonApplicationSettings.Version">Version</see> property is NULL (Nothing in VB.Net).
		/// </remarks>
		public void Save()
		{
			throw new NotSupportedException();
		}
		#endregion

		#region IEditorSettings Members
		/// <summary>
		/// Gets or sets a value indicating whether [properties visible].
		/// </summary>
		/// <value>
		///   <c>true</c> if [properties visible]; otherwise, <c>false</c>.
		/// </value>
		public bool PropertiesVisible
		{
			get;
			set;
		}
		#endregion
	}
}
