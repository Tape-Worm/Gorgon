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
            this.columnContentPlugInDesc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnContentPlugInPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.stripContentPlugIns = new System.Windows.Forms.ToolStrip();
            this.buttonAddContentPlugIn = new System.Windows.Forms.ToolStripButton();
            this.buttonDeleteContentPlugIn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonContentPlugInUp = new System.Windows.Forms.ToolStripButton();
            this.buttonContentPlugInDown = new System.Windows.Forms.ToolStripButton();
            this.buttonScratch = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.listReaderPlugIns = new System.Windows.Forms.ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.stripReaderPlugIns = new System.Windows.Forms.ToolStrip();
            this.buttonAddReaderPlugIn = new System.Windows.Forms.ToolStripButton();
            this.buttonDeleteReaderPlugIn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonReaderPlugInUp = new System.Windows.Forms.ToolStripButton();
            this.buttonReaderPlugInDown = new System.Windows.Forms.ToolStripButton();
            this.tabPlugIns = new KRBTabControl.KRBTabControl();
            this.pageContentPlugIns = new KRBTabControl.TabPageEx();
            this.pageWriterPlugIns = new KRBTabControl.TabPageEx();
            this.panel1 = new System.Windows.Forms.Panel();
            this.listWriterPlugIns = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.stripWriterPlugIns = new System.Windows.Forms.ToolStrip();
            this.buttonAddWriterPlugIn = new System.Windows.Forms.ToolStripButton();
            this.buttonDeleteWriterPlugIn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonWriterPlugInUp = new System.Windows.Forms.ToolStripButton();
            this.buttonWriterPlugInDown = new System.Windows.Forms.ToolStripButton();
            this.pageReaderPlugIns = new KRBTabControl.TabPageEx();
            this.pageDisabled = new KRBTabControl.TabPageEx();
            this.listDisabledPlugIns = new System.Windows.Forms.ListView();
            this.columnDisabledDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnDisabledReason = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnDisablePath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.miniToolStrip = new System.Windows.Forms.ToolStrip();
            this.panelContentPlugIns.SuspendLayout();
            this.stripContentPlugIns.SuspendLayout();
            this.panel2.SuspendLayout();
            this.stripReaderPlugIns.SuspendLayout();
            this.tabPlugIns.SuspendLayout();
            this.pageContentPlugIns.SuspendLayout();
            this.pageWriterPlugIns.SuspendLayout();
            this.panel1.SuspendLayout();
            this.stripWriterPlugIns.SuspendLayout();
            this.pageReaderPlugIns.SuspendLayout();
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
            this.panelContentPlugIns.Controls.Add(this.stripContentPlugIns);
            this.panelContentPlugIns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContentPlugIns.Location = new System.Drawing.Point(0, 0);
            this.panelContentPlugIns.Name = "panelContentPlugIns";
            this.panelContentPlugIns.Size = new System.Drawing.Size(721, 232);
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
            this.listContentPlugIns.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listContentPlugIns.Location = new System.Drawing.Point(0, 25);
            this.listContentPlugIns.Name = "listContentPlugIns";
            this.listContentPlugIns.Size = new System.Drawing.Size(721, 207);
            this.listContentPlugIns.TabIndex = 0;
            this.toolHelp.SetToolTip(this.listContentPlugIns, "Manages plug-ins that are responsible for creating/editing \r\nspecific types of co" +
        "ntent.");
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
            this.stripContentPlugIns.Size = new System.Drawing.Size(721, 25);
            this.stripContentPlugIns.Stretch = true;
            this.stripContentPlugIns.TabIndex = 1;
            this.stripContentPlugIns.Text = "toolStrip1";
            this.toolHelp.SetToolTip(this.stripContentPlugIns, "Manages plug-ins that are responsible for creating/editing \r\nspecific types of co" +
        "ntent.");
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
            this.buttonContentPlugInDown.Text = "Move plug-in down.";
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
            // panel2
            // 
            this.panel2.Controls.Add(this.listReaderPlugIns);
            this.panel2.Controls.Add(this.stripReaderPlugIns);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(721, 232);
            this.panel2.TabIndex = 13;
            // 
            // listReaderPlugIns
            // 
            this.listReaderPlugIns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listReaderPlugIns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
            this.listReaderPlugIns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listReaderPlugIns.FullRowSelect = true;
            this.listReaderPlugIns.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listReaderPlugIns.Location = new System.Drawing.Point(0, 25);
            this.listReaderPlugIns.Name = "listReaderPlugIns";
            this.listReaderPlugIns.Size = new System.Drawing.Size(721, 207);
            this.listReaderPlugIns.TabIndex = 0;
            this.toolHelp.SetToolTip(this.listReaderPlugIns, "Manages plug-ins that read in the editor content items from a\r\nspecific file form" +
        "at (e.g. Zip)");
            this.listReaderPlugIns.UseCompatibleStateImageBehavior = false;
            this.listReaderPlugIns.View = System.Windows.Forms.View.Details;
            this.listReaderPlugIns.SelectedIndexChanged += new System.EventHandler(this.listContentPlugIns_SelectedIndexChanged);
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Description";
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Path";
            // 
            // stripReaderPlugIns
            // 
            this.stripReaderPlugIns.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.stripReaderPlugIns.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonAddReaderPlugIn,
            this.buttonDeleteReaderPlugIn,
            this.toolStripSeparator3,
            this.buttonReaderPlugInUp,
            this.buttonReaderPlugInDown});
            this.stripReaderPlugIns.Location = new System.Drawing.Point(0, 0);
            this.stripReaderPlugIns.Name = "stripReaderPlugIns";
            this.stripReaderPlugIns.Size = new System.Drawing.Size(721, 25);
            this.stripReaderPlugIns.Stretch = true;
            this.stripReaderPlugIns.TabIndex = 1;
            this.stripReaderPlugIns.Text = "toolStrip1";
            this.toolHelp.SetToolTip(this.stripReaderPlugIns, "Manages plug-ins that read in the editor content items from a\r\nspecific file form" +
        "at (e.g. Zip)");
            // 
            // buttonAddReaderPlugIn
            // 
            this.buttonAddReaderPlugIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonAddReaderPlugIn.Image = global::GorgonLibrary.Editor.Properties.Resources.add_16x16;
            this.buttonAddReaderPlugIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonAddReaderPlugIn.Name = "buttonAddReaderPlugIn";
            this.buttonAddReaderPlugIn.Size = new System.Drawing.Size(23, 22);
            this.buttonAddReaderPlugIn.Text = "Add reader plug-in...";
            // 
            // buttonDeleteReaderPlugIn
            // 
            this.buttonDeleteReaderPlugIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonDeleteReaderPlugIn.Enabled = false;
            this.buttonDeleteReaderPlugIn.Image = global::GorgonLibrary.Editor.Properties.Resources.remove_16x16;
            this.buttonDeleteReaderPlugIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonDeleteReaderPlugIn.Name = "buttonDeleteReaderPlugIn";
            this.buttonDeleteReaderPlugIn.Size = new System.Drawing.Size(23, 22);
            this.buttonDeleteReaderPlugIn.Text = "Remove reader plug-in...";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonReaderPlugInUp
            // 
            this.buttonReaderPlugInUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonReaderPlugInUp.Enabled = false;
            this.buttonReaderPlugInUp.Image = global::GorgonLibrary.Editor.Properties.Resources.up_16x16;
            this.buttonReaderPlugInUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonReaderPlugInUp.Name = "buttonReaderPlugInUp";
            this.buttonReaderPlugInUp.Size = new System.Drawing.Size(23, 22);
            this.buttonReaderPlugInUp.Text = "Move plug-in up.";
            this.buttonReaderPlugInUp.Click += new System.EventHandler(this.buttonContentPlugInUp_Click);
            // 
            // buttonReaderPlugInDown
            // 
            this.buttonReaderPlugInDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonReaderPlugInDown.Enabled = false;
            this.buttonReaderPlugInDown.Image = global::GorgonLibrary.Editor.Properties.Resources.down_16x16;
            this.buttonReaderPlugInDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonReaderPlugInDown.Name = "buttonReaderPlugInDown";
            this.buttonReaderPlugInDown.Size = new System.Drawing.Size(23, 22);
            this.buttonReaderPlugInDown.Text = "Move plug-in down.";
            this.buttonReaderPlugInDown.Click += new System.EventHandler(this.buttonContentPlugInDown_Click);
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
            this.tabPlugIns.Controls.Add(this.pageContentPlugIns);
            this.tabPlugIns.Controls.Add(this.pageWriterPlugIns);
            this.tabPlugIns.Controls.Add(this.pageReaderPlugIns);
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
            // pageContentPlugIns
            // 
            this.pageContentPlugIns.BackColor = System.Drawing.Color.White;
            this.pageContentPlugIns.Controls.Add(this.panelContentPlugIns);
            this.pageContentPlugIns.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pageContentPlugIns.IsClosable = false;
            this.pageContentPlugIns.Location = new System.Drawing.Point(5, 30);
            this.pageContentPlugIns.Name = "pageContentPlugIns";
            this.pageContentPlugIns.Size = new System.Drawing.Size(721, 232);
            this.pageContentPlugIns.TabIndex = 0;
            this.pageContentPlugIns.Text = "Content Plug-Ins";
            // 
            // pageWriterPlugIns
            // 
            this.pageWriterPlugIns.BackColor = System.Drawing.Color.White;
            this.pageWriterPlugIns.Controls.Add(this.panel1);
            this.pageWriterPlugIns.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.pageWriterPlugIns.IsClosable = false;
            this.pageWriterPlugIns.Location = new System.Drawing.Point(5, 30);
            this.pageWriterPlugIns.Name = "pageWriterPlugIns";
            this.pageWriterPlugIns.Size = new System.Drawing.Size(721, 232);
            this.pageWriterPlugIns.TabIndex = 1;
            this.pageWriterPlugIns.Text = "Writer Plug-Ins";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.listWriterPlugIns);
            this.panel1.Controls.Add(this.stripWriterPlugIns);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(721, 232);
            this.panel1.TabIndex = 11;
            // 
            // listWriterPlugIns
            // 
            this.listWriterPlugIns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listWriterPlugIns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listWriterPlugIns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listWriterPlugIns.FullRowSelect = true;
            this.listWriterPlugIns.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listWriterPlugIns.Location = new System.Drawing.Point(0, 25);
            this.listWriterPlugIns.Name = "listWriterPlugIns";
            this.listWriterPlugIns.Size = new System.Drawing.Size(721, 207);
            this.listWriterPlugIns.TabIndex = 0;
            this.toolHelp.SetToolTip(this.listWriterPlugIns, "Manages plug-ins that write out the editor content items into a\r\nspecific file fo" +
        "rmat (e.g. Zip)");
            this.listWriterPlugIns.UseCompatibleStateImageBehavior = false;
            this.listWriterPlugIns.View = System.Windows.Forms.View.Details;
            this.listWriterPlugIns.SelectedIndexChanged += new System.EventHandler(this.listContentPlugIns_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Description";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Path";
            // 
            // stripWriterPlugIns
            // 
            this.stripWriterPlugIns.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.stripWriterPlugIns.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonAddWriterPlugIn,
            this.buttonDeleteWriterPlugIn,
            this.toolStripSeparator2,
            this.buttonWriterPlugInUp,
            this.buttonWriterPlugInDown});
            this.stripWriterPlugIns.Location = new System.Drawing.Point(0, 0);
            this.stripWriterPlugIns.Name = "stripWriterPlugIns";
            this.stripWriterPlugIns.Size = new System.Drawing.Size(721, 25);
            this.stripWriterPlugIns.Stretch = true;
            this.stripWriterPlugIns.TabIndex = 1;
            this.stripWriterPlugIns.Text = "toolStrip1";
            this.toolHelp.SetToolTip(this.stripWriterPlugIns, "Manages plug-ins that write out the editor content items into a\r\nspecific file fo" +
        "rmat (e.g. Zip)");
            // 
            // buttonAddWriterPlugIn
            // 
            this.buttonAddWriterPlugIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonAddWriterPlugIn.Image = global::GorgonLibrary.Editor.Properties.Resources.add_16x16;
            this.buttonAddWriterPlugIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonAddWriterPlugIn.Name = "buttonAddWriterPlugIn";
            this.buttonAddWriterPlugIn.Size = new System.Drawing.Size(23, 22);
            this.buttonAddWriterPlugIn.Text = "Add writer plug-in...";
            // 
            // buttonDeleteWriterPlugIn
            // 
            this.buttonDeleteWriterPlugIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonDeleteWriterPlugIn.Enabled = false;
            this.buttonDeleteWriterPlugIn.Image = global::GorgonLibrary.Editor.Properties.Resources.remove_16x16;
            this.buttonDeleteWriterPlugIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonDeleteWriterPlugIn.Name = "buttonDeleteWriterPlugIn";
            this.buttonDeleteWriterPlugIn.Size = new System.Drawing.Size(23, 22);
            this.buttonDeleteWriterPlugIn.Text = "Remove writer plug-in...";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonWriterPlugInUp
            // 
            this.buttonWriterPlugInUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonWriterPlugInUp.Enabled = false;
            this.buttonWriterPlugInUp.Image = global::GorgonLibrary.Editor.Properties.Resources.up_16x16;
            this.buttonWriterPlugInUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonWriterPlugInUp.Name = "buttonWriterPlugInUp";
            this.buttonWriterPlugInUp.Size = new System.Drawing.Size(23, 22);
            this.buttonWriterPlugInUp.Text = "Move plug-in up.";
            this.buttonWriterPlugInUp.Click += new System.EventHandler(this.buttonContentPlugInUp_Click);
            // 
            // buttonWriterPlugInDown
            // 
            this.buttonWriterPlugInDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonWriterPlugInDown.Enabled = false;
            this.buttonWriterPlugInDown.Image = global::GorgonLibrary.Editor.Properties.Resources.down_16x16;
            this.buttonWriterPlugInDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonWriterPlugInDown.Name = "buttonWriterPlugInDown";
            this.buttonWriterPlugInDown.Size = new System.Drawing.Size(23, 22);
            this.buttonWriterPlugInDown.Text = "Move plug-in down.";
            this.buttonWriterPlugInDown.Click += new System.EventHandler(this.buttonContentPlugInDown_Click);
            // 
            // pageReaderPlugIns
            // 
            this.pageReaderPlugIns.BackColor = System.Drawing.Color.White;
            this.pageReaderPlugIns.Controls.Add(this.panel2);
            this.pageReaderPlugIns.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.pageReaderPlugIns.IsClosable = false;
            this.pageReaderPlugIns.Location = new System.Drawing.Point(5, 30);
            this.pageReaderPlugIns.Name = "pageReaderPlugIns";
            this.pageReaderPlugIns.Size = new System.Drawing.Size(721, 232);
            this.pageReaderPlugIns.TabIndex = 2;
            this.pageReaderPlugIns.Text = "Reader (File System) Plug-Ins";
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
            this.panelContentPlugIns.PerformLayout();
            this.stripContentPlugIns.ResumeLayout(false);
            this.stripContentPlugIns.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.stripReaderPlugIns.ResumeLayout(false);
            this.stripReaderPlugIns.PerformLayout();
            this.tabPlugIns.ResumeLayout(false);
            this.pageContentPlugIns.ResumeLayout(false);
            this.pageWriterPlugIns.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.stripWriterPlugIns.ResumeLayout(false);
            this.stripWriterPlugIns.PerformLayout();
            this.pageReaderPlugIns.ResumeLayout(false);
            this.pageDisabled.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
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
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ListView listReaderPlugIns;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ToolStrip stripReaderPlugIns;
        private System.Windows.Forms.ToolStripButton buttonAddReaderPlugIn;
        private System.Windows.Forms.ToolStripButton buttonDeleteReaderPlugIn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton buttonReaderPlugInUp;
        private System.Windows.Forms.ToolStripButton buttonReaderPlugInDown;
        private System.ComponentModel.IContainer components;
        private KRBTabControl.KRBTabControl tabPlugIns;
        private KRBTabControl.TabPageEx pageContentPlugIns;
        private KRBTabControl.TabPageEx pageWriterPlugIns;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView listWriterPlugIns;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ToolStrip stripWriterPlugIns;
        private System.Windows.Forms.ToolStripButton buttonAddWriterPlugIn;
        private System.Windows.Forms.ToolStripButton buttonDeleteWriterPlugIn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton buttonWriterPlugInUp;
        private System.Windows.Forms.ToolStripButton buttonWriterPlugInDown;
        private KRBTabControl.TabPageEx pageReaderPlugIns;
        private System.Windows.Forms.ToolStrip miniToolStrip;
        private KRBTabControl.TabPageEx pageDisabled;
        private System.Windows.Forms.ListView listDisabledPlugIns;
        private System.Windows.Forms.ColumnHeader columnDisabledDescription;
        private System.Windows.Forms.ColumnHeader columnDisabledReason;
        private System.Windows.Forms.ColumnHeader columnDisablePath;
    }
}
