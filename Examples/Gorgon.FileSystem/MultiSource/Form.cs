﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Thursday, January 17, 2013 11:07:02 PM
// 

using System.Text;
using Gorgon.Core;
using Gorgon.IO;
using Gorgon.IO.Providers;
using Gorgon.PlugIns;
using Gorgon.UI;

namespace Gorgon.Examples;

/// <summary>
/// Main application interface
/// </summary>
/// <remarks>
/// In this example we will mount two different data sources into a virtual file system.  
/// 
/// The file system is able to mount different data sources into the same file system which will allow for access
/// to multiple sets of files as a unified file system.  Here we mount a physical file system folder as the root
/// of the file system and a zip file as a virtual subdirectory in the virtual file system.  Accessing these
/// files is just a matter of getting the file entry and loading the data as a stream.  
/// 
/// You may mount multiple data sources into the same virtual directory (i.e. the root, or a sub directory) and the
/// files from each data source will be merged.  However, the caveat here is the order in which the file systems
/// are mounted.  If 2 or more data sources contain the same file names, then the data source that was mounted
/// last will take precedence over the previous file systems.  For example, if D:\directory\filename.txt exists in
/// the virtual root as "/filename.txt" and we mount a zip file that has filename.txt in the root of the zip file,
/// then when we open filename.txt from the virtual file system we will be opening the file from the zip file
/// 
/// We begin the example by loading the file system provider for zip files, and then mounting a physical folder
/// and then the zip file into a virtual subdirectory.  From there we enumerate the files and virtual sub directories
/// into the tree view.  Opening a file can be done either from the file entry as shown in the example, or it can be
/// opened via the file system interface (allowing the user to pass a full path to the file).  Files can be returned
/// as a stream (OpenStream) or an array of bytes (ReadFile).  Please note that writing to these file systems is not
/// supported and can only be done when a write directory is set.  This will be covered in another example
/// </remarks>
public partial class Form
    : System.Windows.Forms.Form
{

    // Our file system.
    private GorgonFileSystem _fileSystem;
    // Our picture box.
    private PictureBox _picture;
    // Loaded image.
    private Image _image;
    // Loaded text/binary info.
    private TextBox _textDisplay;
    // Textbox font.
    private Font _textFont;
    // Instructions label.
    private Label _instructions;
    // File system plug-in assembly cache.
    private GorgonMefPlugInCache _cache;

    /// <summary>
    /// Handles the NodeMouseDoubleClick event of the treeFileSystem control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="TreeNodeMouseClickEventArgs" /> instance containing the event data.</param>
    private void TreeFileSystem_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
    {
        if (splitFileSystem.Panel2.Controls.Count > 0)
        {
            splitFileSystem.Panel2.Controls.RemoveAt(0);
        }

        try
        {
            if (e.Node?.Tag is not IGorgonVirtualFile)
            {
                splitFileSystem.Panel2.Controls.Add(_instructions);
                return;
            }

            _picture.Image = null;
            _textDisplay.Text = string.Empty;
            IGorgonVirtualFile file = (IGorgonVirtualFile)e.Node.Tag;

            // Here we load the image from the file system.
            // Note that we don't care if it's from the zip file
            // or the folder.  It's all the same to us.
            using Stream fileStream = file.OpenStream();
            // If it's a picture, then load it.
            switch (file.Extension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                case ".png":
                    if (_image is not null)
                    {
                        _image.Dispose();
                        _image = null;
                    }

                    _image = Image.FromStream(fileStream);
                    _picture.Image = _image;
                    _picture.SizeMode = PictureBoxSizeMode.Zoom;

                    // Add to control.
                    splitFileSystem.Panel2.Controls.Add(_picture);
                    _picture.Dock = DockStyle.Fill;
                    break;
                default:
                    // Get data in the file stream.
                    byte[] textData = new byte[fileStream.Length];
                    fileStream.Read(textData, 0, textData.Length);

                    // Convert to a string.
                    _textDisplay.Text = Encoding.UTF8.GetString(textData);
                    _textDisplay.Multiline = true;
                    _textDisplay.ReadOnly = true;
                    _textDisplay.ScrollBars = ScrollBars.Both;
                    _textDisplay.Dock = DockStyle.Fill;
                    splitFileSystem.Panel2.Controls.Add(_textDisplay);
                    break;
            }
        }
        catch (Exception ex)
        {
            ex.Handle(e => GorgonDialogs.ErrorBox(this, e), Program.Log);
            splitFileSystem.Panel2.Controls.Add(_instructions);
        }
    }

    /// <summary>
    /// Handles the BeforeExpand event of the treeFileSystem control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="TreeViewCancelEventArgs" /> instance containing the event data.</param>
    private void TreeFileSystem_BeforeExpand(object sender, TreeViewCancelEventArgs e)
    {

        try
        {
            if (e.Node.Tag is not IGorgonVirtualDirectory directory)
            {
                e.Cancel = true;
                return;
            }

            FillTree(directory);
        }
        catch (Exception ex)
        {
            ex.Handle(e => GorgonDialogs.ErrorBox(this, e), Program.Log);
        }
    }

    /// <summary>
    /// Handles the BeforeCollapse event of the treeFileSystem control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="TreeViewCancelEventArgs" /> instance containing the event data.</param>
    private void TreeFileSystem_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
    {
        IGorgonVirtualDirectory directory = e.Node.Tag as IGorgonVirtualDirectory;

        try
        {
            // Do not expand or collapse
            if (directory == _fileSystem.RootDirectory)
            {
                e.Cancel = true;
            }
        }
        catch (Exception ex)
        {
            ex.Handle(e => GorgonDialogs.ErrorBox(this, e), Program.Log);
        }
    }

    /// <summary>
    /// Function to load the zip file system provider.
    /// </summary>
    private void LoadZipFileSystemProvider()
    {
        // Name of our zip provider plugin.
        const string zipProviderPlugInName = "Gorgon.IO.Zip.ZipProvider";

        // We can load the objects we need and discard the plugin system after.
        // This works because we keep the references to the objects that our 
        // plugin creates, even after the plugin is gone.
        _cache = new GorgonMefPlugInCache(Program.Log);

        GorgonFileSystemProviderFactory providerFactory = new(_cache, Program.Log);
        IGorgonFileSystemProvider provider = providerFactory.CreateProvider(Path.Combine(GorgonExample.GetPlugInPath().FullName, "Gorgon.FileSystem.Zip.DLL"), zipProviderPlugInName);

        _fileSystem = new GorgonFileSystem(provider, Program.Log);
    }

    /// <summary>
    /// Function to fill the file system tree view.
    /// </summary>
    /// <param name="directory">Parent directory to fill, or <b>null</b> to fill the root directory.</param>
    private void FillTree(IGorgonVirtualDirectory directory)
    {
        TreeNodeCollection nodes;
        TreeNode parentNode;

        if (directory is null)
        {
            directory = _fileSystem.RootDirectory;
            nodes = treeFileSystem.Nodes;

            // Set root node.
            parentNode = new TreeNode("/") { Name = "/" };
            parentNode.SelectedImageIndex = parentNode.ImageIndex = 0;
        }
        else
        {
            // Find the node with the directory.
            TreeNode[] searchNodes = treeFileSystem.Nodes.Find(directory.FullPath, true);

            if (searchNodes.Length > 0)
            {
                parentNode = searchNodes[0];
                nodes = parentNode.Nodes;
            }
            else
            {
                GorgonDialogs.ErrorBox(this, "Could not find the virtual directory '" + directory.FullPath + "'");
                return;
            }
        }

        parentNode.Tag = directory;

        // Turn off the expand event.
        treeFileSystem.BeforeExpand -= TreeFileSystem_BeforeExpand;

        try
        {
            // Clean up the tree node.
            treeFileSystem.BeginUpdate();
            nodes.Clear();

            // Add the root node if necessary.
            if (parentNode.Tag == _fileSystem.RootDirectory)
            {
                nodes.Add(parentNode);
            }

            // Enumerate the data.  For the purposed of this example, we will filter out known binary files from our file system.				
            IEnumerable<IGorgonVirtualDirectory> directories = directory.Directories.Select(d => d.Value)
                                                                                    .OrderBy(item => item.Name);
            IEnumerable<IGorgonVirtualFile> files = directory.Files.Select(f => f.Value)
                                                                   .Where(item => item.Extension is not ".gorSprite" and not ".gal")
                                                                   .OrderBy(item => item.Name);

            // Get directories.
            foreach (IGorgonVirtualDirectory subDirectory in directories)
            {
                TreeNode directoryNode = new(subDirectory.Name)
                {
                    Name = subDirectory.FullPath,
                    Tag = subDirectory
                };

                // Put a special icon on the zip file so we have a visual representation
                // of where it is in our VFS.
                // The VFS does not care if the data is in a zip file or folder, and Gorgon
                // does very little to differentiate it.  After all, the whole point of
                // have a unified file system is to abstract away the differences.
                if (subDirectory.Name != "ZipFile")
                {
                    directoryNode.SelectedImageIndex = directoryNode.ImageIndex = 0;
                }
                else
                {
                    directoryNode.SelectedImageIndex = directoryNode.ImageIndex = 2;
                }

                // Add a dummy node if there are files or sub directories.
                if ((subDirectory.Directories.Count > 0) || (subDirectory.Files.Count(item => item.Value.Extension is not ".gorSprite" and not ".gal") > 0))
                {
                    directoryNode.Nodes.Add(new TreeNode("This is a dummy node."));
                }

                parentNode.Nodes.Add(directoryNode);
            }

            // Get files.
            foreach (IGorgonVirtualFile file in files)
            {
                if (file.Extension == ".gorSprite")
                {
                    continue;
                }

                TreeNode fileNode = new(file.Name)
                {
                    Name = file.FullPath,
                    Tag = file
                };
                fileNode.SelectedImageIndex = fileNode.ImageIndex = 1;
                parentNode.Nodes.Add(fileNode);
            }

            parentNode.Expand();
        }
        finally
        {
            treeFileSystem.EndUpdate();
            treeFileSystem.BeforeExpand += TreeFileSystem_BeforeExpand;
        }
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
    /// </summary>
    /// <param name="e">A <see cref="FormClosingEventArgs" /> that contains the event data.</param>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        _cache?.Dispose();
        _picture?.Dispose();
        _image?.Dispose();
        _textDisplay?.Dispose();
        _textFont?.Dispose();
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
            GorgonExample.ResourceBaseDirectory = new DirectoryInfo(ExampleConfig.Default.ResourceLocation);
            GorgonExample.PlugInLocationDirectory = new DirectoryInfo(ExampleConfig.Default.PlugInLocation);

            // Picture box.
            _picture = new PictureBox
            {
                Name = "pictureImage"
            };

            // Text display.
            _textDisplay = new TextBox
            {
                Name = "textDisplay"
            };
            _textFont = new Font("Consolas", 10.0f, FontStyle.Regular, GraphicsUnit.Point);
            _textDisplay.Font = _textFont;

            _instructions = new Label
            {
                Name = "labelInstructions",
                Text = "Double click on a file node in the tree to display it.",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = Font
            };

            // Add the instructions.
            splitFileSystem.Panel2.Controls.Add(_instructions);

            // Get the zip file provider.
            LoadZipFileSystemProvider();

            // Mount the physical file system directory.
            _fileSystem.Mount(GorgonExample.GetResourcePath(@"\FileSystems\VFSRoot\").FullName);

            // Mount the zip file into a sub directory.
            _fileSystem.Mount(Path.Combine(GorgonExample.GetResourcePath(@"\FileSystems").FullName, "VFSRoot.zip"), "/ZipFile");

            // Fill the root of the tree.
            FillTree(null);
        }
        catch (Exception ex)
        {
            ex.Handle(e => GorgonDialogs.ErrorBox(this, e), Program.Log);
            GorgonApplication.Quit();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Form" /> class.
    /// </summary>
    public Form()
    {
        InitializeComponent();

        imageTree.ImageSize = new Size(16, 16);
        imageTree.Images.Add(Properties.Resources.folder_16x16);
        imageTree.Images.Add(Properties.Resources.document_text_16x16);
        imageTree.Images.Add(Properties.Resources.packed_file_16x16);
    }
}
