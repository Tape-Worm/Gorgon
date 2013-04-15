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
            this.panelContentPlugIns = new System.Windows.Forms.Panel();
            this.listContentPlugIns = new System.Windows.Forms.ListView();
            this.columnDesc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonScratch = new System.Windows.Forms.Button();
            this.tabPlugIns = new KRBTabControl.KRBTabControl();
            this.pagePlugIns = new KRBTabControl.TabPageEx();
            this.pageDisabled = new KRBTabControl.TabPageEx();
            this.listDisabledPlugIns = new System.Windows.Forms.ListView();
            this.columnDisabledDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnDisabledReason = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnDisablePath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.miniToolStrip = new System.Windows.Forms.ToolStrip();
            this.columnType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelContentPlugIns.SuspendLayout();
            this.tabPlugIns.SuspendLayout();
            this.pagePlugIns.SuspendLayout();
            this.pageDisabled.SuspendLayout();
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
            this.textScratchLocation.Size = new System.Drawing.Size(695, 23);
            this.textScratchLocation.TabIndex = 5;
            this.toolHelp.SetToolTip(this.textScratchLocation, "Selects a new working area for the content in the editor.\r\n\r\nChanging this value " +
        "will require a restart of the application.");
            this.textScratchLocation.Enter += new System.EventHandler(this.textScratchLocation_Enter);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Scratch data location:";
            // 
            // panelContentPlugIns
            // 
            this.panelContentPlugIns.Controls.Add(this.listContentPlugIns);
            this.panelContentPlugIns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContentPlugIns.Location = new System.Drawing.Point(0, 0);
            this.panelContentPlugIns.Name = "panelContentPlugIns";
            this.panelContentPlugIns.Size = new System.Drawing.Size(721, 232);
            this.panelContentPlugIns.TabIndex = 9;
            // 
            // listContentPlugIns
            // 
            this.listContentPlugIns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listContentPlugIns.CheckBoxes = true;
            this.listContentPlugIns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnDesc,
            this.columnType,
            this.columnPath});
            this.listContentPlugIns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listContentPlugIns.FullRowSelect = true;
            this.listContentPlugIns.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listContentPlugIns.Location = new System.Drawing.Point(0, 0);
            this.listContentPlugIns.Name = "listContentPlugIns";
            this.listContentPlugIns.Size = new System.Drawing.Size(721, 232);
            this.listContentPlugIns.TabIndex = 0;
            this.toolHelp.SetToolTip(this.listContentPlugIns, "Manages plug-ins that are responsible for creating/editing \r\nspecific types of co" +
        "ntent.");
            this.listContentPlugIns.UseCompatibleStateImageBehavior = false;
            this.listContentPlugIns.View = System.Windows.Forms.View.Details;
            // 
            // columnDesc
            // 
            this.columnDesc.Text = "Description";
            // 
            // columnPath
            // 
            this.columnPath.Text = "Path";
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
            this.buttonScratch.Location = new System.Drawing.Point(707, 20);
            this.buttonScratch.Name = "buttonScratch";
            this.buttonScratch.Size = new System.Drawing.Size(26, 26);
            this.buttonScratch.TabIndex = 7;
            this.buttonScratch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolHelp.SetToolTip(this.buttonScratch, "Selects a new working area for the content in the editor.\r\n\r\nChanging this value " +
        "will require a restart of the application.");
            this.buttonScratch.UseVisualStyleBackColor = false;
            this.buttonScratch.Click += new System.EventHandler(this.buttonScratch_Click);
            // 
            // tabPlugIns
            // 
            this.tabPlugIns.AllowDrop = true;
            this.tabPlugIns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabPlugIns.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.tabPlugIns.BackgroundHatcher.HatchType = System.Drawing.Drawing2D.HatchStyle.DashedVertical;
            this.tabPlugIns.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.tabPlugIns.Controls.Add(this.pagePlugIns);
            this.tabPlugIns.Controls.Add(this.pageDisabled);
            this.tabPlugIns.IsCaptionVisible = false;
            this.tabPlugIns.IsDocumentTabStyle = true;
            this.tabPlugIns.IsDrawEdgeBorder = true;
            this.tabPlugIns.IsUserInteraction = false;
            this.tabPlugIns.ItemSize = new System.Drawing.Size(0, 24);
            this.tabPlugIns.Location = new System.Drawing.Point(7, 52);
            this.tabPlugIns.Name = "tabPlugIns";
            this.tabPlugIns.SelectedIndex = 0;
            this.tabPlugIns.Size = new System.Drawing.Size(731, 267);
            this.tabPlugIns.TabBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.tabPlugIns.TabGradient.ColorEnd = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.tabPlugIns.TabGradient.ColorStart = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.tabPlugIns.TabGradient.SelectedTabFontStyle = System.Drawing.FontStyle.Bold;
            this.tabPlugIns.TabGradient.TabPageSelectedTextColor = System.Drawing.Color.White;
            this.tabPlugIns.TabGradient.TabPageTextColor = System.Drawing.Color.White;
            this.tabPlugIns.TabIndex = 14;
            this.tabPlugIns.TabPageCloseIconColor = System.Drawing.Color.White;
            this.tabPlugIns.UpDownStyle = KRBTabControl.KRBTabControl.UpDown32Style.Default;
            // 
            // pagePlugIns
            // 
            this.pagePlugIns.BackColor = System.Drawing.Color.White;
            this.pagePlugIns.Controls.Add(this.panelContentPlugIns);
            this.pagePlugIns.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pagePlugIns.IsClosable = false;
            this.pagePlugIns.Location = new System.Drawing.Point(5, 30);
            this.pagePlugIns.Name = "pagePlugIns";
            this.pagePlugIns.Size = new System.Drawing.Size(721, 232);
            this.pagePlugIns.TabIndex = 0;
            this.pagePlugIns.Text = "Plug-Ins";
            // 
            // pageDisabled
            // 
            this.pageDisabled.BackColor = System.Drawing.Color.White;
            this.pageDisabled.Controls.Add(this.listDisabledPlugIns);
            this.pageDisabled.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.pageDisabled.IsClosable = false;
            this.pageDisabled.Location = new System.Drawing.Point(5, 30);
            this.pageDisabled.Name = "pageDisabled";
            this.pageDisabled.Size = new System.Drawing.Size(721, 232);
            this.pageDisabled.TabIndex = 3;
            this.pageDisabled.Text = "Disabled Plug-Ins";
            // 
            // listDisabledPlugIns
            // 
            this.listDisabledPlugIns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listDisabledPlugIns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnDisabledDescription,
            this.columnDisabledReason,
            this.columnDisablePath});
            this.listDisabledPlugIns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listDisabledPlugIns.FullRowSelect = true;
            this.listDisabledPlugIns.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listDisabledPlugIns.Location = new System.Drawing.Point(0, 0);
            this.listDisabledPlugIns.Name = "listDisabledPlugIns";
            this.listDisabledPlugIns.Size = new System.Drawing.Size(721, 232);
            this.listDisabledPlugIns.TabIndex = 1;
            this.listDisabledPlugIns.UseCompatibleStateImageBehavior = false;
            this.listDisabledPlugIns.View = System.Windows.Forms.View.Details;
            this.listDisabledPlugIns.DoubleClick += new System.EventHandler(this.listDisabledPlugIns_DoubleClick);
            // 
            // columnDisabledDescription
            // 
            this.columnDisabledDescription.Text = "Description";
            // 
            // columnDisabledReason
            // 
            this.columnDisabledReason.Text = "Reason for disable";
            // 
            // columnDisablePath
            // 
            this.columnDisablePath.Text = "Path";
            // 
            // miniToolStrip
            // 
            this.miniToolStrip.AutoSize = false;
            this.miniToolStrip.CanOverflow = false;
            this.miniToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.miniToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.miniToolStrip.Location = new System.Drawing.Point(98, 3);
            this.miniToolStrip.Name = "miniToolStrip";
            this.miniToolStrip.Size = new System.Drawing.Size(627, 25);
            this.miniToolStrip.Stretch = true;
            this.miniToolStrip.TabIndex = 1;
            this.toolHelp.SetToolTip(this.miniToolStrip, "Manages plug-ins that write out the editor content items into a\r\nspecific file fo" +
        "rmat (e.g. Zip)");
            // 
            // columnType
            // 
            this.columnType.Text = "Type";
            // 
            // EditorPreferencePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.Controls.Add(this.tabPlugIns);
            this.Controls.Add(this.buttonScratch);
            this.Controls.Add(this.textScratchLocation);
            this.Controls.Add(this.label1);
            this.Name = "EditorPreferencePanel";
            this.Size = new System.Drawing.Size(745, 328);
            this.Text = "Editor Preferences";
            this.panelContentPlugIns.ResumeLayout(false);
            this.tabPlugIns.ResumeLayout(false);
            this.pagePlugIns.ResumeLayout(false);
            this.pageDisabled.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.Panel panelContentPlugIns;
        private System.Windows.Forms.ListView listContentPlugIns;
        private System.Windows.Forms.ColumnHeader columnPath;
        private System.Windows.Forms.ColumnHeader columnDesc;
        private System.ComponentModel.IContainer components;
        private KRBTabControl.KRBTabControl tabPlugIns;
        private KRBTabControl.TabPageEx pagePlugIns;
        private System.Windows.Forms.ToolStrip miniToolStrip;
        private KRBTabControl.TabPageEx pageDisabled;
        private System.Windows.Forms.ListView listDisabledPlugIns;
        private System.Windows.Forms.ColumnHeader columnDisabledDescription;
        private System.Windows.Forms.ColumnHeader columnDisabledReason;
        private System.Windows.Forms.ColumnHeader columnDisablePath;
        private System.Windows.Forms.ColumnHeader columnType;
    }
}
