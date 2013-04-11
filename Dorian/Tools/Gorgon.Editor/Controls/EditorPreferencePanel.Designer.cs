namespace GorgonLibrary.Editor
{
    partial class EditorPreferencePanel
    {
        private System.Windows.Forms.Button buttonScratch;
        private System.Windows.Forms.TextBox textScratchLocation;
        private System.Windows.Forms.Label label1;

        private void InitializeComponent()
        {
            this.textScratchLocation = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panelContentPlugIns = new System.Windows.Forms.Panel();
            this.listContentPlugIns = new System.Windows.Forms.ListView();
            this.columnContentPlugInDesc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnContentPlugInPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.stripContentPlugIns = new System.Windows.Forms.ToolStrip();
            this.buttonAddContentPlugIn = new System.Windows.Forms.ToolStripButton();
            this.buttonDeleteContentPlugIn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonContentPlugInUp = new System.Windows.Forms.ToolStripButton();
            this.buttonContentPlugInDown = new System.Windows.Forms.ToolStripButton();
            this.buttonScratch = new System.Windows.Forms.Button();
            this.panelContentPlugIns.SuspendLayout();
            this.stripContentPlugIns.SuspendLayout();
            this.SuspendLayout();
            // 
            // textScratchLocation
            // 
            this.textScratchLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textScratchLocation.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textScratchLocation.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.textScratchLocation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textScratchLocation.Location = new System.Drawing.Point(6, 21);
            this.textScratchLocation.Name = "textScratchLocation";
            this.textScratchLocation.Size = new System.Drawing.Size(596, 20);
            this.textScratchLocation.TabIndex = 5;
            this.textScratchLocation.Enter += new System.EventHandler(this.textScratchLocation_Enter);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Scratch data location:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Content Plug-Ins:";
            // 
            // panelContentPlugIns
            // 
            this.panelContentPlugIns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContentPlugIns.Controls.Add(this.listContentPlugIns);
            this.panelContentPlugIns.Controls.Add(this.stripContentPlugIns);
            this.panelContentPlugIns.Location = new System.Drawing.Point(6, 65);
            this.panelContentPlugIns.Name = "panelContentPlugIns";
            this.panelContentPlugIns.Size = new System.Drawing.Size(627, 142);
            this.panelContentPlugIns.TabIndex = 9;
            // 
            // listContentPlugIns
            // 
            this.listContentPlugIns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listContentPlugIns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnContentPlugInDesc,
            this.columnContentPlugInPath});
            this.listContentPlugIns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listContentPlugIns.FullRowSelect = true;
            this.listContentPlugIns.Location = new System.Drawing.Point(0, 25);
            this.listContentPlugIns.Name = "listContentPlugIns";
            this.listContentPlugIns.Size = new System.Drawing.Size(627, 117);
            this.listContentPlugIns.TabIndex = 0;
            this.listContentPlugIns.UseCompatibleStateImageBehavior = false;
            this.listContentPlugIns.View = System.Windows.Forms.View.Details;
            this.listContentPlugIns.SelectedIndexChanged += new System.EventHandler(this.listContentPlugIns_SelectedIndexChanged);
            // 
            // columnContentPlugInDesc
            // 
            this.columnContentPlugInDesc.Text = "Description";
            // 
            // columnContentPlugInPath
            // 
            this.columnContentPlugInPath.Text = "Path";
            // 
            // stripContentPlugIns
            // 
            this.stripContentPlugIns.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.stripContentPlugIns.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonAddContentPlugIn,
            this.buttonDeleteContentPlugIn,
            this.toolStripSeparator1,
            this.buttonContentPlugInUp,
            this.buttonContentPlugInDown});
            this.stripContentPlugIns.Location = new System.Drawing.Point(0, 0);
            this.stripContentPlugIns.Name = "stripContentPlugIns";
            this.stripContentPlugIns.Size = new System.Drawing.Size(627, 25);
            this.stripContentPlugIns.Stretch = true;
            this.stripContentPlugIns.TabIndex = 1;
            this.stripContentPlugIns.Text = "toolStrip1";
            // 
            // buttonAddContentPlugIn
            // 
            this.buttonAddContentPlugIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonAddContentPlugIn.Image = global::GorgonLibrary.Editor.Properties.Resources.add_16x16;
            this.buttonAddContentPlugIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonAddContentPlugIn.Name = "buttonAddContentPlugIn";
            this.buttonAddContentPlugIn.Size = new System.Drawing.Size(23, 22);
            this.buttonAddContentPlugIn.Text = "Add content plug-in...";
            // 
            // buttonDeleteContentPlugIn
            // 
            this.buttonDeleteContentPlugIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonDeleteContentPlugIn.Enabled = false;
            this.buttonDeleteContentPlugIn.Image = global::GorgonLibrary.Editor.Properties.Resources.remove_16x16;
            this.buttonDeleteContentPlugIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonDeleteContentPlugIn.Name = "buttonDeleteContentPlugIn";
            this.buttonDeleteContentPlugIn.Size = new System.Drawing.Size(23, 22);
            this.buttonDeleteContentPlugIn.Text = "Remove content plug-in...";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonContentPlugInUp
            // 
            this.buttonContentPlugInUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonContentPlugInUp.Enabled = false;
            this.buttonContentPlugInUp.Image = global::GorgonLibrary.Editor.Properties.Resources.up_16x16;
            this.buttonContentPlugInUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonContentPlugInUp.Name = "buttonContentPlugInUp";
            this.buttonContentPlugInUp.Size = new System.Drawing.Size(23, 22);
            this.buttonContentPlugInUp.Text = "Move plug-in up.";
            this.buttonContentPlugInUp.Click += new System.EventHandler(this.buttonContentPlugInUp_Click);
            // 
            // buttonContentPlugInDown
            // 
            this.buttonContentPlugInDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonContentPlugInDown.Enabled = false;
            this.buttonContentPlugInDown.Image = global::GorgonLibrary.Editor.Properties.Resources.down_16x16;
            this.buttonContentPlugInDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonContentPlugInDown.Name = "buttonContentPlugInDown";
            this.buttonContentPlugInDown.Size = new System.Drawing.Size(23, 22);
            this.buttonContentPlugInDown.Text = "Move content plug-in down.";
            this.buttonContentPlugInDown.Click += new System.EventHandler(this.buttonContentPlugInDown_Click);
            // 
            // buttonScratch
            // 
            this.buttonScratch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonScratch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonScratch.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonScratch.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonScratch.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
            this.buttonScratch.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonScratch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonScratch.ForeColor = System.Drawing.Color.White;
            this.buttonScratch.Image = global::GorgonLibrary.Editor.Properties.Resources.folder_open_16x16;
            this.buttonScratch.Location = new System.Drawing.Point(608, 19);
            this.buttonScratch.Name = "buttonScratch";
            this.buttonScratch.Size = new System.Drawing.Size(26, 26);
            this.buttonScratch.TabIndex = 7;
            this.buttonScratch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonScratch.UseVisualStyleBackColor = false;
            this.buttonScratch.Click += new System.EventHandler(this.buttonScratch_Click);
            // 
            // EditorPreferencePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.panelContentPlugIns);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonScratch);
            this.Controls.Add(this.textScratchLocation);
            this.Controls.Add(this.label1);
            this.Name = "EditorPreferencePanel";
            this.Size = new System.Drawing.Size(639, 383);
            this.Text = "Editor Preferences";
            this.panelContentPlugIns.ResumeLayout(false);
            this.panelContentPlugIns.PerformLayout();
            this.stripContentPlugIns.ResumeLayout(false);
            this.stripContentPlugIns.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panelContentPlugIns;
        private System.Windows.Forms.ToolStrip stripContentPlugIns;
        private System.Windows.Forms.ToolStripButton buttonAddContentPlugIn;
        private System.Windows.Forms.ToolStripButton buttonDeleteContentPlugIn;
        private System.Windows.Forms.ListView listContentPlugIns;
        private System.Windows.Forms.ColumnHeader columnContentPlugInPath;
        private System.Windows.Forms.ColumnHeader columnContentPlugInDesc;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton buttonContentPlugInUp;
        private System.Windows.Forms.ToolStripButton buttonContentPlugInDown;
    }
}
