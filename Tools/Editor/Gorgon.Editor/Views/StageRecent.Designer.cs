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
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new System.Windows.Forms.ListViewItem.ListViewSubItem[] {
            new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "ASdf"),
            new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "3453", System.Drawing.Color.Gray, System.Drawing.Color.White, new System.Drawing.Font("Segoe UI", 10F))}, -1);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new System.Windows.Forms.ListViewItem.ListViewSubItem[] {
            new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "cxzcxz"),
            new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "1234", System.Drawing.Color.Gray, System.Drawing.Color.White, new System.Drawing.Font("Segoe UI", 10F))}, -1);
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem(new System.Windows.Forms.ListViewItem.ListViewSubItem[] {
            new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "gdfgdf"),
            new System.Windows.Forms.ListViewItem.ListViewSubItem(null, "1234", System.Drawing.Color.Gray, System.Drawing.Color.White, new System.Drawing.Font("Segoe UI", 10F))}, -1);
            this.PanelRecent = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            this.kryptonLabel1 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.PanelBorder = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ListFiles = new System.Windows.Forms.ListView();
            this.ColumnFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.PanelRecent)).BeginInit();
            this.PanelRecent.SuspendLayout();
            this.PanelBorder.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelRecent
            // 
            this.PanelRecent.Controls.Add(this.PanelBorder);
            this.PanelRecent.Controls.Add(this.kryptonLabel1);
            this.PanelRecent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelRecent.Location = new System.Drawing.Point(6, 6);
            this.PanelRecent.Name = "PanelRecent";
            this.PanelRecent.PanelBackStyle = ComponentFactory.Krypton.Toolkit.PaletteBackStyle.PanelCustom1;
            this.PanelRecent.Size = new System.Drawing.Size(588, 456);
            this.PanelRecent.TabIndex = 0;
            // 
            // kryptonLabel1
            // 
            this.kryptonLabel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.kryptonLabel1.LabelStyle = ComponentFactory.Krypton.Toolkit.LabelStyle.Custom2;
            this.kryptonLabel1.Location = new System.Drawing.Point(0, 0);
            this.kryptonLabel1.Name = "kryptonLabel1";
            this.kryptonLabel1.Size = new System.Drawing.Size(588, 34);
            this.kryptonLabel1.TabIndex = 0;
            this.kryptonLabel1.Values.Text = "Recent files";
            // 
            // PanelBorder
            // 
            this.PanelBorder.BackColor = System.Drawing.Color.DimGray;
            this.PanelBorder.Controls.Add(this.panel1);
            this.PanelBorder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelBorder.Location = new System.Drawing.Point(0, 34);
            this.PanelBorder.Name = "PanelBorder";
            this.PanelBorder.Padding = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.PanelBorder.Size = new System.Drawing.Size(588, 422);
            this.PanelBorder.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.ListFiles);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(588, 421);
            this.panel1.TabIndex = 0;
            // 
            // ListFiles
            // 
            this.ListFiles.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.ListFiles.BackColor = System.Drawing.Color.White;
            this.ListFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ListFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnFileName,
            this.ColumnDate});
            this.ListFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListFiles.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.ListFiles.FullRowSelect = true;
            this.ListFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.ListFiles.HideSelection = false;
            listViewItem1.UseItemStyleForSubItems = false;
            listViewItem2.UseItemStyleForSubItems = false;
            listViewItem3.UseItemStyleForSubItems = false;
            this.ListFiles.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3});
            this.ListFiles.Location = new System.Drawing.Point(0, 0);
            this.ListFiles.Name = "ListFiles";
            this.ListFiles.Size = new System.Drawing.Size(588, 421);
            this.ListFiles.TabIndex = 0;
            this.ListFiles.UseCompatibleStateImageBehavior = false;
            this.ListFiles.View = System.Windows.Forms.View.Details;
            // 
            // ColumnFileName
            // 
            this.ColumnFileName.Text = "FIleName";
            // 
            // ColumnDate
            // 
            this.ColumnDate.Text = "Date";
            // 
            // StageRecent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PanelRecent);
            this.Name = "StageRecent";
            this.Padding = new System.Windows.Forms.Padding(6);
            ((System.ComponentModel.ISupportInitialize)(this.PanelRecent)).EndInit();
            this.PanelRecent.ResumeLayout(false);
            this.PanelRecent.PerformLayout();
            this.PanelBorder.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonPanel PanelRecent;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabel1;
        private System.Windows.Forms.Panel PanelBorder;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView ListFiles;
        private System.Windows.Forms.ColumnHeader ColumnFileName;
        private System.Windows.Forms.ColumnHeader ColumnDate;
    }
}
