#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Tuesday, February 24, 2015 10:19:16 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.Core;

namespace Gorgon.Editor
{
	/// <summary>
	/// The settings for the application.
	/// </summary>
	interface IEditorSettings
	{
		/// <summary>
		/// Property to set or return the directory that holds the plug-ins.
		/// </summary>
		string PlugInDirectory
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the directory that holds theme files.
		/// </summary>
		string ThemeDirectory
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the main form state.
		/// </summary>
		FormWindowState FormState
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the window dimensions.
		/// </summary>
		Rectangle WindowDimensions
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the path to the scratch location for temporary data.
		/// </summary>
		/// <remarks>This value will check and format itself appropriately for directory paths.</remarks>
		string ScratchPath
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the rate of animation for the default start page.
		/// </summary>
		float StartPageAnimationPulseRate
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the path to the last editor file.
		/// </summary>
		string LastEditorFile
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the last import file path.
		/// </summary>
		string ImportLastFilePath
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the last export file path.
		/// </summary>
		string ExportLastFilePath
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the recent files.
		/// </summary>
		IList<string> RecentFiles
		{
			get;
		}

		/// <summary>
		/// Property to return the list of user disabled plug-ins.
		/// </summary>
		IList<string> DisabledPlugIns
		{
			get;
		}

		/// <summary>
		/// Property to set or return the default image editor plug-in to use when handling images in other plug-ins.
		/// </summary>
		string DefaultImageEditor
		{
			get;
			set;
		}

		/// <summary>
		/// Property set or return whether to automatically load the last file opened by the editor on start up.
		/// </summary>
		bool AutoLoadLastFile
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the name of the application that the settings are from.
		/// </summary>
		string ApplicationName
		{
			get;
		}

		/// <summary>
		/// Property to set or return the path to the configuration file.
		/// </summary>
		string Path
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the application settings version.
		/// </summary>
		/// <remarks>Assigning NULL (<i>Nothing</i> in VB.Net) will bypass version checking.</remarks>
		Version Version
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the position of the splitter on the main form.
		/// </summary>
		int SplitPosition
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the properties & files tabs are visible in the editor.
		/// </summary>
		bool PropertiesVisible
		{
			get;
			set;
		}

		/// <summary>
		/// Function to save the settings to a file.
		/// </summary>
		/// <remarks>No versioning will be applied to the settings file when the <see cref="P:Gorgon.Configuration.GorgonApplicationSettings.Version">Version</see> property is NULL (<i>Nothing</i> in VB.Net).</remarks>
		/// <exception cref="GorgonException">Thrown when the file being saved is not of the same format as an Gorgon application setting file.</exception>
		void Save();
	}
}