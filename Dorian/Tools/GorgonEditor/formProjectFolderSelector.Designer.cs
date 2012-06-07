namespace GorgonLibrary.GorgonEditor
{
	partial class formProjectFolderSelector
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formProjectFolderSelector));
			this.panel2 = new GorgonLibrary.GorgonEditor.panelEx();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.treeFolders = new System.Windows.Forms.TreeView();
			this.imageTree = new System.Windows.Forms.ImageList(this.components);
			this.labelPath = new System.Windows.Forms.Label();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.buttonOK);
			this.panel2.Controls.Add(this.buttonCancel);
			this.panel2.Controls.Add(this.statusStrip1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(0, 348);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(428, 51);
			this.panel2.TabIndex = 5;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.buttonOK.Image = global::GorgonLibrary.GorgonEditor.Properties.Resources.ok_16x16;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(236, 11);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(87, 28);
			this.buttonOK.TabIndex = 0;
			this.buttonOK.Text = "OK";
			this.buttonOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Image = global::GorgonLibrary.GorgonEditor.Properties.Resources.cancel_16x16;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(329, 11);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 28);
			this.buttonCancel.TabIndex = 1;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// statusStrip1
			// 
			this.statusStrip1.BackColor = System.Drawing.Color.White;
			this.statusStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
			this.statusStrip1.Location = new System.Drawing.Point(0, 29);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(428, 22);
			this.statusStrip1.TabIndex = 2;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// treeFolders
			// 
			this.treeFolders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeFolders.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeFolders.HideSelection = false;
			this.treeFolders.ImageIndex = 0;
			this.treeFolders.ImageList = this.imageTree;
			this.treeFolders.Location = new System.Drawing.Point(3, 3);
			this.treeFolders.Name = "treeFolders";
			this.treeFolders.PathSeparator = "/";
			this.treeFolders.SelectedImageIndex = 0;
			this.treeFolders.ShowLines = false;
			this.treeFolders.ShowNodeToolTips = true;
			this.treeFolders.ShowRootLines = false;
			this.treeFolders.Size = new System.Drawing.Size(422, 319);
			this.treeFolders.TabIndex = 6;
			this.treeFolders.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeFolders_BeforeCollapse);
			this.treeFolders.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeFolders_AfterSelect);
			// 
			// imageTree
			// 
			this.imageTree.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageTree.ImageStream")));
			this.imageTree.TransparentColor = System.Drawing.Color.Transparent;
			this.imageTree.Images.SetKeyName(0, "folder_16x16.png");
			// 
			// labelPath
			// 
			this.labelPath.AutoEllipsis = true;
			this.labelPath.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.labelPath.Location = new System.Drawing.Point(0, 325);
			this.labelPath.Name = "labelPath";
			this.labelPath.Size = new System.Drawing.Size(428, 23);
			this.labelPath.TabIndex = 7;
			this.labelPath.Text = "Selected path:";
			this.labelPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// formProjectFolderSelector
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(428, 399);
			this.Controls.Add(this.labelPath);
			this.Controls.Add(this.treeFolders);
			this.Controls.Add(this.panel2);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formProjectFolderSelector";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Select project folder...";
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private panelEx panel2;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.TreeView treeFolders;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ImageList imageTree;
		private System.Windows.Forms.Label labelPath;
	}
}