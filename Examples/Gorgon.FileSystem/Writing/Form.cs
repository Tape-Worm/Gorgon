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
// Created: Friday, January 18, 2013 8:47:30 AM
// 
#endregion

using System;
using System.IO;
using System.Text;
using Gorgon.Core;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Examples;

/// <summary>
/// Main application interface.
/// </summary>
/// <remarks>
/// In this example we will show how to write to the file system.  
/// 
/// Whenever we create a file system, it's created as a read-only file system.  This is fine for most uses, but we need to be
/// able to write data into it.  The problem is that the file system data could be on read only media like a DVD or CD.  Gorgon's 
/// virtual file system is based on PhysicsFS (http://icculus.org/physfs/), it uses the concept of a write directory.
/// This means when you update a file or add one, it will get rerouted to the write directory.  The write directory MUST be a 
/// physical directory on your hard drive and must be able to be written into.
/// 
/// Setting up a write directory is fairly simple: Create an instance of a GorgonFileSystemWriter and pass in the file system 
/// that you wish to set up for writing, and the physical location on the windows file system where the data will be written into. 
/// Once a writer is setup, the files in it will take precedenceover all files in the file system.  So, for example, if 
/// SomeText.txt exists in root of the mounted folder, and the same file name exists in the physical file system directory, then 
/// the version in the physical file system directory will be used for file I/O operations.
/// 
/// Here, we do exactly this.  We take the file from the root of the directory and read it in.  
/// </remarks>
public partial class Form
    : System.Windows.Forms.Form
{
    #region Variables.
    // Our file system.
    private GorgonFileSystem _fileSystem;
    // The file system writer.
    private GorgonFileSystemWriter _writer;
    // Original text.
    private string _originalText = string.Empty;
    // Changed text.
    private string _changedText = string.Empty;
    #endregion

    #region Properties.
    /// <summary>
    /// Handles the TextChanged event of the textDisplay control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void TextDisplay_TextChanged(object sender, EventArgs e)
    {
        buttonSave.Enabled = !string.Equals(_originalText, textDisplay.Text, StringComparison.CurrentCulture);
        itemLoadOriginal.Enabled = buttonSave.Enabled;
    }

    /// <summary>
    /// Handles the Click event of the buttonSave control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ButtonSave_Click(object sender, EventArgs e)
    {
        try
        {
            CommandEnable(false);
            textDisplay.Enabled = false;

            if (string.IsNullOrEmpty(textDisplay.Text))
            {
                _writer.DeleteFile("/SomeText.txt");
                LoadText();
                return;
            }

            using Stream stream = _writer.OpenStream("/SomeText.txt", FileMode.Create);
            byte[] data = Encoding.UTF8.GetBytes(textDisplay.Text);
            stream.Write(data, 0, data.Length);
            _changedText = textDisplay.Text;
        }
        catch (Exception ex)
        {
            ex.Catch(_ => GorgonDialogs.ErrorBox(this, _), Program.Log);
        }
        finally
        {
            textDisplay.Enabled = true;
            CommandEnable(!string.Equals(_originalText, _changedText, StringComparison.CurrentCulture));
            UpdateInfo();
        }
    }

    /// <summary>
    /// Handles the Click event of the itemLoadChanged control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ItemLoadChanged_Click(object sender, EventArgs e)
    {
        try
        {
            CommandEnable(false);
            LoadText();
        }
        catch (Exception ex)
        {
            ex.Catch(_ => GorgonDialogs.ErrorBox(this, _), Program.Log);
        }
        finally
        {
            CommandEnable(!string.Equals(_originalText, _changedText, StringComparison.CurrentCulture));
            UpdateInfo();
        }
    }

    /// <summary>
    /// Handles the Click event of the itemLoadOriginal control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ItemLoadOriginal_Click(object sender, EventArgs e)
    {
        try
        {
            CommandEnable(false);
            LoadText();
            textDisplay.Text = _originalText;
        }
        catch (Exception ex)
        {
            ex.Catch(except => GorgonDialogs.ErrorBox(this, except), Program.Log);
        }
        finally
        {
            CommandEnable(!string.Equals(_originalText, _changedText, StringComparison.CurrentCulture));
            UpdateInfo();
        }
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to enable or disable the command buttons.
    /// </summary>
    /// <param name="value"><b>true</b> to enable the command buttons, <b>false</b> to disable.</param>
    private void CommandEnable(bool value)
    {
        itemLoadChanged.Enabled = itemLoadOriginal.Enabled = value;
        buttonSave.Enabled = !string.Equals(textDisplay.Text, _originalText, StringComparison.CurrentCulture);
    }

    /// <summary>
        /// Function to update the information label.
        /// </summary>
        private void UpdateInfo() => labelInfo.Text = string.Equals(_originalText, textDisplay.Text, StringComparison.CurrentCulture)
                             ? $"Using original text from {GorgonExample.GetResourcePath(@"FileSystems\FolderSystem\").FullName.Ellipses(100, true)}"
                             : $"Using modified text from {Program.WriteDirectory.FullName.Ellipses(100, true)}";

    /// <summary>
    /// Function to load the text into the file system.
    /// </summary>
    private void LoadText()
    {
        DirectoryInfo physicalPath = GorgonExample.GetResourcePath(@"FileSystems\FolderSystem\");

        // Unload the mounted files.
        _writer.Unmount();
        _fileSystem.Unmount(physicalPath.FullName);

        _fileSystem.Mount(physicalPath.FullName);

        // Load the original before we mount the write directory.
        IGorgonVirtualFile file = _fileSystem.GetFile("/SomeText.txt");

        using (Stream stream = file.OpenStream())
        {
            byte[] textData = new byte[stream.Length];

            stream.Read(textData, 0, textData.Length);
            _originalText = Encoding.UTF8.GetString(textData);
        }

        // Set the write location to the users app data folder.
        _writer.Mount();

        // Load the modified version (if it exists, if it doesn't, the original will be loaded instead).
        file = _fileSystem.GetFile("/SomeText.txt");

        using (Stream stream = file.OpenStream())
        {
            byte[] textData = new byte[stream.Length];

            stream.Read(textData, 0, textData.Length);
            _changedText = Encoding.UTF8.GetString(textData);

            textDisplay.Text = string.Equals(_changedText, _originalText, StringComparison.CurrentCulture) ? _originalText : _changedText;
        }
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        try
        {
            GorgonExample.PlugInLocationDirectory = new DirectoryInfo(ExampleConfig.Default.PlugInLocation);
            GorgonExample.ResourceBaseDirectory = new DirectoryInfo(ExampleConfig.Default.ResourceLocation);

            // Create our virtual file system.
            _fileSystem = new GorgonFileSystem(Program.Log);
            _writer = new GorgonFileSystemWriter(_fileSystem, _fileSystem, Program.WriteDirectory.FullName);

            LoadText();

            labelFileSystem.Text = $"{GorgonExample.GetResourcePath(@"FileSystems\FolderSystem\").FullName.Ellipses(100, true)} mounted as '/'.";
            labelWriteLocation.Text = $"{Program.WriteDirectory.FullName.Ellipses(100, true)} mounted as '/'";
        }
        catch (Exception ex)
        {
            GorgonExample.HandleException(ex);
            GorgonApplication.Quit();
        }
        finally
        {
            CommandEnable(!string.Equals(_originalText, _changedText, StringComparison.CurrentCulture));
            UpdateInfo();
        }
    }
    #endregion

    #region Constructor/Destructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="Form" /> class.
    /// </summary>
    public Form() => InitializeComponent();
    #endregion
}
