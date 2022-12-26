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
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form));
        this.splitFileSystem = new System.Windows.Forms.SplitContainer();
        this.treeFileSystem = new System.Windows.Forms.TreeView();
        this.imageTree = new System.Windows.Forms.ImageList(this.components);
        ((System.ComponentModel.ISupportInitialize)(this.splitFileSystem)).BeginInit();
        this.splitFileSystem.Panel1.SuspendLayout();
        this.splitFileSystem.SuspendLayout();
        this.SuspendLayout();
        // 
        // splitFileSystem
        // 
        this.splitFileSystem.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitFileSystem.Location = new System.Drawing.Point(0, 0);
        this.splitFileSystem.Name = "splitFileSystem";
        // 
        // splitFileSystem.Panel1
        // 
        this.splitFileSystem.Panel1.Controls.Add(this.treeFileSystem);
        this.splitFileSystem.Size = new System.Drawing.Size(780, 525);
        this.splitFileSystem.SplitterDistance = 200;
        this.splitFileSystem.SplitterWidth = 5;
        this.splitFileSystem.TabIndex = 0;
        // 
        // treeFileSystem
        // 
        this.treeFileSystem.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.treeFileSystem.Dock = System.Windows.Forms.DockStyle.Fill;
        this.treeFileSystem.ImageIndex = 0;
        this.treeFileSystem.ImageList = this.imageTree;
        this.treeFileSystem.Location = new System.Drawing.Point(0, 0);
        this.treeFileSystem.Name = "treeFileSystem";
        this.treeFileSystem.SelectedImageIndex = 0;
        this.treeFileSystem.ShowRootLines = false;
        this.treeFileSystem.Size = new System.Drawing.Size(200, 525);
        this.treeFileSystem.TabIndex = 0;
        this.treeFileSystem.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeFileSystem_BeforeCollapse);
        this.treeFileSystem.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeFileSystem_BeforeExpand);
        this.treeFileSystem.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeFileSystem_NodeMouseDoubleClick);
        // 
        // imageTree
        // 
        this.imageTree.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageTree.ImageStream")));
        this.imageTree.TransparentColor = System.Drawing.Color.Transparent;
        this.imageTree.Images.SetKeyName(0, "folder_16x16.png");
        this.imageTree.Images.SetKeyName(1, "document_text_16x16.png");
        this.imageTree.Images.SetKeyName(2, "packedfile_16x16.png");
        // 
        // formMain
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(784, 561);
        this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "formMain";
        this.Padding = new System.Windows.Forms.Padding(2);
		    this.Controls.Add(this.splitFileSystem);
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Gorgon Example - Multiple Filesystem Sources";
        this.splitFileSystem.Panel1.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitFileSystem)).EndInit();
        this.splitFileSystem.ResumeLayout(false);
        this.ResumeLayout(false);

		}

		#endregion

		private SplitContainer splitFileSystem;
		private TreeView treeFileSystem;
		private ImageList imageTree;
	}

