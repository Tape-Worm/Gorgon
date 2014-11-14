namespace GorgonLibrary.Editor.Controls
{
    partial class PanelPlugIns
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			this.tabPlugIns = new KRBTabControl.KRBTabControl();
			this.pagePlugIns = new KRBTabControl.TabPageEx();
			this.panelContentPlugIns = new System.Windows.Forms.Panel();
			this.listContentPlugIns = new System.Windows.Forms.ListView();
			this.columnDesc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.popupPlugIns = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.itemDisablePlugIn = new System.Windows.Forms.ToolStripMenuItem();
			this.itemEnablePlugIn = new System.Windows.Forms.ToolStripMenuItem();
			this.pageDisabled = new KRBTabControl.TabPageEx();
			this.listDisabledPlugIns = new System.Windows.Forms.ListView();
			this.columnDisabledDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnDisabledReason = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnDisablePath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.imagePlugInHelp = new System.Windows.Forms.PictureBox();
			this.tabPlugIns.SuspendLayout();
			this.pagePlugIns.SuspendLayout();
			this.panelContentPlugIns.SuspendLayout();
			this.popupPlugIns.SuspendLayout();
			this.pageDisabled.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.imagePlugInHelp)).BeginInit();
			this.SuspendLayout();
			// 
			// tabPlugIns
			// 
			this.tabPlugIns.AllowDrop = true;
			this.tabPlugIns.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.tabPlugIns.BackgroundHatcher.HatchType = System.Drawing.Drawing2D.HatchStyle.DashedVertical;
			this.tabPlugIns.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.tabPlugIns.Controls.Add(this.pagePlugIns);
			this.tabPlugIns.Controls.Add(this.pageDisabled);
			this.tabPlugIns.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabPlugIns.IsCaptionVisible = false;
			this.tabPlugIns.IsDocumentTabStyle = true;
			this.tabPlugIns.IsDrawEdgeBorder = true;
			this.tabPlugIns.IsUserInteraction = false;
			this.tabPlugIns.ItemSize = new System.Drawing.Size(0, 24);
			this.tabPlugIns.Location = new System.Drawing.Point(3, 3);
			this.tabPlugIns.Name = "tabPlugIns";
			this.tabPlugIns.SelectedIndex = 0;
			this.tabPlugIns.Size = new System.Drawing.Size(665, 383);
			this.tabPlugIns.TabBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.tabPlugIns.TabGradient.ColorEnd = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.tabPlugIns.TabGradient.ColorStart = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.tabPlugIns.TabGradient.SelectedTabFontStyle = System.Drawing.FontStyle.Bold;
			this.tabPlugIns.TabGradient.TabPageSelectedTextColor = System.Drawing.Color.White;
			this.tabPlugIns.TabGradient.TabPageTextColor = System.Drawing.Color.White;
			this.tabPlugIns.TabIndex = 6;
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
			this.pagePlugIns.Size = new System.Drawing.Size(655, 348);
			this.pagePlugIns.TabIndex = 0;
			this.pagePlugIns.Text = "active";
			// 
			// panelContentPlugIns
			// 
			this.panelContentPlugIns.Controls.Add(this.listContentPlugIns);
			this.panelContentPlugIns.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelContentPlugIns.Location = new System.Drawing.Point(0, 0);
			this.panelContentPlugIns.Name = "panelContentPlugIns";
			this.panelContentPlugIns.Size = new System.Drawing.Size(655, 348);
			this.panelContentPlugIns.TabIndex = 9;
			// 
			// listContentPlugIns
			// 
			this.listContentPlugIns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listContentPlugIns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnDesc,
            this.columnType,
            this.columnPath});
			this.listContentPlugIns.ContextMenuStrip = this.popupPlugIns;
			this.listContentPlugIns.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listContentPlugIns.FullRowSelect = true;
			this.listContentPlugIns.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listContentPlugIns.Location = new System.Drawing.Point(0, 0);
			this.listContentPlugIns.Name = "listContentPlugIns";
			this.listContentPlugIns.Size = new System.Drawing.Size(655, 348);
			this.listContentPlugIns.TabIndex = 0;
			this.listContentPlugIns.UseCompatibleStateImageBehavior = false;
			this.listContentPlugIns.View = System.Windows.Forms.View.Details;
			// 
			// columnDesc
			// 
			this.columnDesc.Text = "description";
			// 
			// columnType
			// 
			this.columnType.Text = "type";
			// 
			// columnPath
			// 
			this.columnPath.Text = "path";
			// 
			// popupPlugIns
			// 
			this.popupPlugIns.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemDisablePlugIn,
            this.itemEnablePlugIn});
			this.popupPlugIns.Name = "contextMenuStrip1";
			this.popupPlugIns.Size = new System.Drawing.Size(154, 48);
			this.popupPlugIns.Opening += new System.ComponentModel.CancelEventHandler(this.popupPlugIns_Opening);
			// 
			// itemDisablePlugIn
			// 
			this.itemDisablePlugIn.Image = global::GorgonLibrary.Editor.Properties.Resources.remove_16x16;
			this.itemDisablePlugIn.Name = "itemDisablePlugIn";
			this.itemDisablePlugIn.Size = new System.Drawing.Size(153, 22);
			this.itemDisablePlugIn.Text = "disable plug-in";
			this.itemDisablePlugIn.Click += new System.EventHandler(this.itemDisablePlugIn_Click);
			// 
			// itemEnablePlugIn
			// 
			this.itemEnablePlugIn.Image = global::GorgonLibrary.Editor.Properties.Resources.add_16x16;
			this.itemEnablePlugIn.Name = "itemEnablePlugIn";
			this.itemEnablePlugIn.Size = new System.Drawing.Size(153, 22);
			this.itemEnablePlugIn.Text = "enable plug-in";
			this.itemEnablePlugIn.Click += new System.EventHandler(this.itemEnablePlugIn_Click);
			// 
			// pageDisabled
			// 
			this.pageDisabled.BackColor = System.Drawing.Color.White;
			this.pageDisabled.Controls.Add(this.listDisabledPlugIns);
			this.pageDisabled.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.pageDisabled.IsClosable = false;
			this.pageDisabled.Location = new System.Drawing.Point(5, 30);
			this.pageDisabled.Name = "pageDisabled";
			this.pageDisabled.Size = new System.Drawing.Size(655, 348);
			this.pageDisabled.TabIndex = 3;
			this.pageDisabled.Text = "disabled";
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
			this.listDisabledPlugIns.Size = new System.Drawing.Size(655, 348);
			this.listDisabledPlugIns.TabIndex = 1;
			this.listDisabledPlugIns.UseCompatibleStateImageBehavior = false;
			this.listDisabledPlugIns.View = System.Windows.Forms.View.Details;
			this.listDisabledPlugIns.DoubleClick += new System.EventHandler(this.listDisabledPlugIns_DoubleClick);
			// 
			// columnDisabledDescription
			// 
			this.columnDisabledDescription.Text = "description";
			// 
			// columnDisabledReason
			// 
			this.columnDisabledReason.Text = "reason";
			// 
			// columnDisablePath
			// 
			this.columnDisablePath.Text = "path";
			// 
			// imagePlugInHelp
			// 
			this.imagePlugInHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.imagePlugInHelp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.imagePlugInHelp.Image = global::GorgonLibrary.Editor.Properties.Resources.info_16x16;
			this.imagePlugInHelp.Location = new System.Drawing.Point(649, 6);
			this.imagePlugInHelp.Name = "imagePlugInHelp";
			this.imagePlugInHelp.Size = new System.Drawing.Size(16, 16);
			this.imagePlugInHelp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.imagePlugInHelp.TabIndex = 7;
			this.imagePlugInHelp.TabStop = false;
			// 
			// PanelPlugIns
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.imagePlugInHelp);
			this.Controls.Add(this.tabPlugIns);
			this.Name = "PanelPlugIns";
			this.Size = new System.Drawing.Size(671, 389);
			this.tabPlugIns.ResumeLayout(false);
			this.pagePlugIns.ResumeLayout(false);
			this.panelContentPlugIns.ResumeLayout(false);
			this.popupPlugIns.ResumeLayout(false);
			this.pageDisabled.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.imagePlugInHelp)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private KRBTabControl.KRBTabControl tabPlugIns;
        private KRBTabControl.TabPageEx pagePlugIns;
        private System.Windows.Forms.Panel panelContentPlugIns;
        private System.Windows.Forms.ListView listContentPlugIns;
        private System.Windows.Forms.ColumnHeader columnDesc;
        private System.Windows.Forms.ColumnHeader columnType;
        private System.Windows.Forms.ColumnHeader columnPath;
        private KRBTabControl.TabPageEx pageDisabled;
        private System.Windows.Forms.ListView listDisabledPlugIns;
        private System.Windows.Forms.ColumnHeader columnDisabledDescription;
        private System.Windows.Forms.ColumnHeader columnDisabledReason;
        private System.Windows.Forms.ColumnHeader columnDisablePath;
		private System.Windows.Forms.ContextMenuStrip popupPlugIns;
		private System.Windows.Forms.ToolStripMenuItem itemDisablePlugIn;
		private System.Windows.Forms.ToolStripMenuItem itemEnablePlugIn;
		private System.Windows.Forms.PictureBox imagePlugInHelp;

    }
}
