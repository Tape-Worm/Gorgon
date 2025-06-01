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

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new Container();
        ComponentResourceManager resources = new ComponentResourceManager(typeof(Form));
        splitFileSystem = new SplitContainer();
        treeFileSystem = new TreeView();
        imageTree = new ImageList(components);
        LabelInstructions = new Label();
        TextDisplay = new TextBox();
        Picture = new PictureBox();
        ((ISupportInitialize)splitFileSystem).BeginInit();
        splitFileSystem.Panel1.SuspendLayout();
        splitFileSystem.Panel2.SuspendLayout();
        splitFileSystem.SuspendLayout();
        ((ISupportInitialize)Picture).BeginInit();
        SuspendLayout();
        // 
        // splitFileSystem
        // 
        splitFileSystem.Dock = DockStyle.Fill;
        splitFileSystem.Location = new Point(2, 2);
        splitFileSystem.Name = "splitFileSystem";
        // 
        // splitFileSystem.Panel1
        // 
        splitFileSystem.Panel1.Controls.Add(treeFileSystem);
        // 
        // splitFileSystem.Panel2
        // 
        splitFileSystem.Panel2.Controls.Add(LabelInstructions);
        splitFileSystem.Panel2.Controls.Add(TextDisplay);
        splitFileSystem.Panel2.Controls.Add(Picture);
        splitFileSystem.Size = new Size(780, 557);
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
        treeFileSystem.Location = new Point(0, 0);
        treeFileSystem.Name = "treeFileSystem";
        treeFileSystem.SelectedImageIndex = 0;
        treeFileSystem.ShowRootLines = false;
        treeFileSystem.Size = new Size(200, 557);
        treeFileSystem.TabIndex = 0;
        treeFileSystem.BeforeCollapse += TreeFileSystem_BeforeCollapse;
        treeFileSystem.BeforeExpand += TreeFileSystem_BeforeExpand;
        treeFileSystem.NodeMouseDoubleClick += TreeFileSystem_NodeMouseDoubleClick;
        // 
        // imageTree
        // 
        imageTree.ColorDepth = ColorDepth.Depth32Bit;
        imageTree.ImageSize = new Size(16, 16);
        imageTree.TransparentColor = Color.Transparent;
        // 
        // LabelInstructions
        // 
        LabelInstructions.Dock = DockStyle.Fill;
        LabelInstructions.Location = new Point(0, 0);
        LabelInstructions.Name = "LabelInstructions";
        LabelInstructions.Size = new Size(575, 557);
        LabelInstructions.TabIndex = 0;
        LabelInstructions.Text = "Double click on a file node in the tree to display it.";
        LabelInstructions.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // TextDisplay
        // 
        TextDisplay.BorderStyle = BorderStyle.None;
        TextDisplay.Font = new Font("Consolas", 10F);
        TextDisplay.Location = new Point(111, 286);
        TextDisplay.Multiline = true;
        TextDisplay.Name = "TextDisplay";
        TextDisplay.ReadOnly = true;
        TextDisplay.ScrollBars = ScrollBars.Both;
        TextDisplay.Size = new Size(100, 23);
        TextDisplay.TabIndex = 2;
        TextDisplay.Visible = false;
        TextDisplay.WordWrap = false;
        // 
        // Picture
        // 
        Picture.Location = new Point(471, 113);
        Picture.Name = "Picture";
        Picture.Size = new Size(100, 50);
        Picture.SizeMode = PictureBoxSizeMode.Zoom;
        Picture.TabIndex = 1;
        Picture.TabStop = false;
        Picture.Visible = false;
        // 
        // Form
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(784, 561);
        Controls.Add(splitFileSystem);
        Font = new Font("Segoe UI", 9F);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "Form";
        Padding = new Padding(2);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Gorgon Example - Multiple Filesystem Sources";
        splitFileSystem.Panel1.ResumeLayout(false);
        splitFileSystem.Panel2.ResumeLayout(false);
        splitFileSystem.Panel2.PerformLayout();
        ((ISupportInitialize)splitFileSystem).EndInit();
        splitFileSystem.ResumeLayout(false);
        ((ISupportInitialize)Picture).EndInit();
        ResumeLayout(false);
    }



    private SplitContainer splitFileSystem;
    private TreeView treeFileSystem;
    private ImageList imageTree;
    private Label LabelInstructions;
    private PictureBox Picture;
    private TextBox TextDisplay;
}

