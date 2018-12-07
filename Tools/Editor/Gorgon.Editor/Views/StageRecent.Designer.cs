namespace Gorgon.Editor.Views
{
    partial class StageRecent
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

            if (disposing)
            {
                UnassignEvents();
                _subItemFont.Dispose();
                _itemFont.Dispose();
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
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem(new System.Windows.Forms.ListViewItem.ListViewSubItem[] {
            new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "ASdf"),
            new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "3453", System.Drawing.Color.Gray, System.Drawing.Color.White, new System.Drawing.Font("Segoe UI", 10F))}, -1);
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem(new System.Windows.Forms.ListViewItem.ListViewSubItem[] {
            new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "cxzcxz"),
            new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "1234", System.Drawing.Color.Gray, System.Drawing.Color.White, new System.Drawing.Font("Segoe UI", 10F))}, -1);
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem(new System.Windows.Forms.ListViewItem.ListViewSubItem[] {
            new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "gdfgdf"),
            new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "1234", System.Drawing.Color.Gray, System.Drawing.Color.White, new System.Drawing.Font("Segoe UI", 10F))}, -1);
            this.PanelRecent = new System.Windows.Forms.Panel();
            this.PanelBorder = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ListFiles = new System.Windows.Forms.ListView();
            this.ColumnFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.LabelHeader = new System.Windows.Forms.Label();
            this.PanelRecent.SuspendLayout();
            this.PanelBorder.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelRecent
            // 
            this.PanelRecent.Controls.Add(this.PanelBorder);
            this.PanelRecent.Controls.Add(this.panel2);
            this.PanelRecent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelRecent.Location = new System.Drawing.Point(6, 0);
            this.PanelRecent.Name = "PanelRecent";
            this.PanelRecent.Size = new System.Drawing.Size(594, 468);
            this.PanelRecent.TabIndex = 0;
            // 
            // PanelBorder
            // 
            this.PanelBorder.BackColor = System.Drawing.Color.DimGray;
            this.PanelBorder.Controls.Add(this.panel1);
            this.PanelBorder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelBorder.Location = new System.Drawing.Point(0, 35);
            this.PanelBorder.Name = "PanelBorder";
            this.PanelBorder.Padding = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.PanelBorder.Size = new System.Drawing.Size(594, 433);
            this.PanelBorder.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.ListFiles);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(594, 432);
            this.panel1.TabIndex = 0;
            // 
            // ListFiles
            // 
            this.ListFiles.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.ListFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ListFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ListFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnFileName,
            this.ColumnDate});
            this.ListFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListFiles.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.ListFiles.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ListFiles.FullRowSelect = true;
            this.ListFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.ListFiles.HideSelection = false;
            this.ListFiles.HotTracking = true;
            this.ListFiles.HoverSelection = true;
            this.ListFiles.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem4,
            listViewItem5,
            listViewItem6});
            this.ListFiles.Location = new System.Drawing.Point(0, 0);
            this.ListFiles.Name = "ListFiles";
            this.ListFiles.Size = new System.Drawing.Size(594, 432);
            this.ListFiles.TabIndex = 0;
            this.ListFiles.UseCompatibleStateImageBehavior = false;
            this.ListFiles.View = System.Windows.Forms.View.Details;
            this.ListFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.ListFiles_ColumnClick);
            this.ListFiles.ItemActivate += new System.EventHandler(this.ListFiles_ItemActivate);
            // 
            // ColumnFileName
            // 
            this.ColumnFileName.Text = "Path";
            // 
            // ColumnDate
            // 
            this.ColumnDate.Text = "Last Used";
            // 
            // panel2
            // 
            this.panel2.AutoSize = true;
            this.panel2.BackColor = System.Drawing.Color.SteelBlue;
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(3, 0, 0, 1);
            this.panel2.Size = new System.Drawing.Size(594, 35);
            this.panel2.TabIndex = 2;
            // 
            // panel3
            // 
            this.panel3.AutoSize = true;
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.panel3.Controls.Add(this.LabelHeader);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.panel3.Location = new System.Drawing.Point(3, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(591, 34);
            this.panel3.TabIndex = 1;
            // 
            // LabelHeader
            // 
            this.LabelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.LabelHeader.Font = new System.Drawing.Font("Segoe UI", 13.5F);
            this.LabelHeader.Location = new System.Drawing.Point(0, 0);
            this.LabelHeader.Name = "LabelHeader";
            this.LabelHeader.Size = new System.Drawing.Size(591, 34);
            this.LabelHeader.TabIndex = 1;
            this.LabelHeader.Text = "Recent files";
            this.LabelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // StageRecent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PanelRecent);
            this.Name = "StageRecent";
            this.PanelRecent.ResumeLayout(false);
            this.PanelRecent.PerformLayout();
            this.PanelBorder.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel PanelRecent;
        private System.Windows.Forms.Panel PanelBorder;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView ListFiles;
        private System.Windows.Forms.ColumnHeader ColumnFileName;
        private System.Windows.Forms.ColumnHeader ColumnDate;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label LabelHeader;
    }
}
