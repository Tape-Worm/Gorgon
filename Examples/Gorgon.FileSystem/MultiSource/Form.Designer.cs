using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Examples;

partial class Form
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new Container();
        var resources = new ComponentResourceManager(typeof(Form));
        splitFileSystem = new SplitContainer();
        treeFileSystem = new TreeView();
        imageTree = new ImageList(components);
        ((ISupportInitialize)splitFileSystem).BeginInit();
        splitFileSystem.Panel1.SuspendLayout();
        splitFileSystem.SuspendLayout();
        SuspendLayout();
        // 
        // splitFileSystem
        // 
        splitFileSystem.Dock = DockStyle.Fill;
        splitFileSystem.Location = new System.Drawing.Point(2, 2);
        splitFileSystem.Name = "splitFileSystem";
        // 
        // splitFileSystem.Panel1
        // 
        splitFileSystem.Panel1.Controls.Add(treeFileSystem);
        splitFileSystem.Size = new System.Drawing.Size(780, 557);
        splitFileSystem.SplitterDistance = 200;
        splitFileSystem.SplitterWidth = 5;
        splitFileSystem.TabIndex = 0;
        // 
        // treeFileSystem
        // 
        treeFileSystem.BorderStyle = BorderStyle.FixedSingle;
        treeFileSystem.Dock = DockStyle.Fill;
        treeFileSystem.ImageIndex = 0;
        treeFileSystem.ImageList = imageTree;
        treeFileSystem.Location = new System.Drawing.Point(0, 0);
        treeFileSystem.Name = "treeFileSystem";
        treeFileSystem.SelectedImageIndex = 0;
        treeFileSystem.ShowRootLines = false;
        treeFileSystem.Size = new System.Drawing.Size(200, 557);
        treeFileSystem.TabIndex = 0;
        treeFileSystem.BeforeCollapse += TreeFileSystem_BeforeCollapse;
        treeFileSystem.BeforeExpand += TreeFileSystem_BeforeExpand;
        treeFileSystem.NodeMouseDoubleClick += TreeFileSystem_NodeMouseDoubleClick;
        // 
        // imageTree
        // 
        imageTree.ColorDepth = ColorDepth.Depth32Bit;
        imageTree.ImageSize = new System.Drawing.Size(16, 16);
        imageTree.TransparentColor = System.Drawing.Color.Transparent;
        // 
        // Form
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(784, 561);
        Controls.Add(splitFileSystem);
        Font = new System.Drawing.Font("Segoe UI", 9F);
        Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
        Name = "Form";
        Padding = new Padding(2);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Gorgon Example - Multiple Filesystem Sources";
        splitFileSystem.Panel1.ResumeLayout(false);
        ((ISupportInitialize)splitFileSystem).EndInit();
        splitFileSystem.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private SplitContainer splitFileSystem;
    private TreeView treeFileSystem;
    private ImageList imageTree;
}

