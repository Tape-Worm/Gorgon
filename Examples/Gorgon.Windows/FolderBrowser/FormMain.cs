#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: December 7, 2020 3:17:59 PM
// 
#endregion

using System;
using System.IO;
using System.Windows.Forms;
using Gorgon.Examples.Properties;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Examples
{
    /// <summary>
    /// Main example window.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This example shows the Gorgon folder browser control. This is meant as a replacement for the awful folder selector that has been included with WinForms since the dawn 
    /// of .NET.  
    /// 
    /// With this browser you can not only select a folder, but rename, add and delete folders. It also supports setting a virtual root so that the browser cannot go above a 
    /// certain directory level.
    /// 
    /// Finally the directory selector can be configured to use the Gorgon File System by setting the root directory to the physical file system path and the directory separator 
    /// character ('/'), and then handling any events triggered to translate back and forth.
    /// </para>
    /// </remarks>
    public partial class FormMain
        : Form
    {
        #region Variables.
        // The area where we can make changes (our resource area).
        private DirectoryInfo _writeArea;
        #endregion

        #region Methods.
        /// <summary>Function called when the browser enters a new folder.</summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private void Browser_FolderEntered(object sender, FolderSelectedArgs e)
        {
            // If we are under the writable area, then allow us to add/remove/rename directories.
            if ((_writeArea.Exists) && (e.FolderPath.StartsWith(_writeArea.FullName.FormatDirectory(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase)))
            {
                Browser.IsReadOnly = false;
                Browser.Text = "Double click on a folder to change it. Click the '+' to create a folder and '-' to delete one.";
            }
            else
            {
                // Otherwise, be safe and don't allow any changes.
                Browser.IsReadOnly = true;
                Browser.Text = "Double click on a folder to change it.";
            }
        }

        /// <summary>Raises the <see cref="Form.Load">Load</see> event.</summary>
        /// <param name="e">An <see cref="EventArgs">EventArgs</see> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _writeArea = new DirectoryInfo(Path.Combine(Path.GetFullPath(Settings.Default.ResourceLocation), "FolderBrowser"));

            // Set the root to the example folder so we can keep things safe.
            if (_writeArea.Exists)
            {
                Browser.AssignInitialDirectory(_writeArea);
            }
            else
            {
                // If we didn't get the resource directory, then we'll show the entire file system, but with our 
                // add/remove directory functionality disabled - We don't want to mess up your drive right?
                Browser.IsReadOnly = true;
                Browser.Text = "Double click on a folder to change it.";
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FormMain" /> class.</summary>
        public FormMain() => InitializeComponent();
        #endregion
    }
}
