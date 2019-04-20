namespace Gorgon.Editor.UI.Forms
{
    partial class FormPlugInManager
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPlugInManager));
			this.ButtonOK = new System.Windows.Forms.Button();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.ListPlugins = new System.Windows.Forms.ListView();
			this.ColumnPluginName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ColumnStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ColumnPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ToolStripButtons = new System.Windows.Forms.ToolStrip();
			this.ButtonAddPlugins = new System.Windows.Forms.ToolStripButton();
			this.ButtonRemovePlugins = new System.Windows.Forms.ToolStripButton();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.ToolStripButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// ButtonOK
			// 
			this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonOK.AutoSize = true;
			this.ButtonOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ButtonOK.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.ButtonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
			this.ButtonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.ButtonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonOK.Location = new System.Drawing.Point(544, 19);
			this.ButtonOK.MinimumSize = new System.Drawing.Size(100, 30);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(100, 30);
			this.ButtonOK.TabIndex = 2;
			this.ButtonOK.Text = "OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonCancel.AutoSize = true;
			this.ButtonCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.ButtonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
			this.ButtonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonCancel.Location = new System.Drawing.Point(650, 19);
			this.ButtonCancel.MinimumSize = new System.Drawing.Size(100, 30);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(100, 30);
			this.ButtonCancel.TabIndex = 3;
			this.ButtonCancel.Text = "Cancel";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// panel1
			// 
			this.panel1.AutoSize = true;
			this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panel1.BackColor = System.Drawing.Color.SteelBlue;
			this.panel1.Controls.Add(this.panel2);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 504);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(0, 1, 0, 0);
			this.panel1.Size = new System.Drawing.Size(753, 53);
			this.panel1.TabIndex = 4;
			// 
			// panel2
			// 
			this.panel2.AutoSize = true;
			this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.panel2.Controls.Add(this.ButtonCancel);
			this.panel2.Controls.Add(this.ButtonOK);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(0, 1);
			this.panel2.Name = "panel2";
			this.panel2.Padding = new System.Windows.Forms.Padding(0, 16, 0, 0);
			this.panel2.Size = new System.Drawing.Size(753, 52);
			this.panel2.TabIndex = 0;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.ListPlugins, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.ToolStripButtons, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(753, 504);
			this.tableLayoutPanel1.TabIndex = 5;
			// 
			// ListPlugins
			// 
			this.ListPlugins.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ListPlugins.CheckBoxes = true;
			this.ListPlugins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnPluginName,
            this.ColumnStatus,
            this.ColumnPath});
			this.ListPlugins.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ListPlugins.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.ListPlugins.Location = new System.Drawing.Point(3, 32);
			this.ListPlugins.MultiSelect = false;
			this.ListPlugins.Name = "ListPlugins";
			this.ListPlugins.Size = new System.Drawing.Size(747, 473);
			this.ListPlugins.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.ListPlugins.TabIndex = 0;
			this.ListPlugins.UseCompatibleStateImageBehavior = false;
			this.ListPlugins.View = System.Windows.Forms.View.Details;
			// 
			// ColumnPluginName
			// 
			this.ColumnPluginName.Text = "Plug in name";
			// 
			// ColumnStatus
			// 
			this.ColumnStatus.Text = "Status";
			// 
			// ColumnPath
			// 
			this.ColumnPath.Text = "Path";
			// 
			// ToolStripButtons
			// 
			this.ToolStripButtons.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.ToolStripButtons.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.ToolStripButtons.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ButtonAddPlugins,
            this.ButtonRemovePlugins});
			this.ToolStripButtons.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.ToolStripButtons.Location = new System.Drawing.Point(0, 0);
			this.ToolStripButtons.Name = "ToolStripButtons";
			this.ToolStripButtons.Size = new System.Drawing.Size(753, 29);
			this.ToolStripButtons.Stretch = true;
			this.ToolStripButtons.TabIndex = 1;
			this.ToolStripButtons.Text = "toolStrip1";
			// 
			// ButtonAddPlugins
			// 
			this.ButtonAddPlugins.Image = global::Gorgon.Editor.Properties.Resources.add_plugin_22x22;
			this.ButtonAddPlugins.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.ButtonAddPlugins.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ButtonAddPlugins.Name = "ButtonAddPlugins";
			this.ButtonAddPlugins.Size = new System.Drawing.Size(95, 26);
			this.ButtonAddPlugins.Text = "Add Plug in";
			this.ButtonAddPlugins.ToolTipText = "Adds new plug ins.";
			// 
			// ButtonRemovePlugins
			// 
			this.ButtonRemovePlugins.Image = global::Gorgon.Editor.Properties.Resources.remove_plugins_22x22;
			this.ButtonRemovePlugins.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.ButtonRemovePlugins.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ButtonRemovePlugins.Name = "ButtonRemovePlugins";
			this.ButtonRemovePlugins.Size = new System.Drawing.Size(129, 26);
			this.ButtonRemovePlugins.Text = "Remove plug in(s)";
			this.ButtonRemovePlugins.ToolTipText = "Removes the selected plug ins.  \r\n\r\nNote that this will not take effect until the" +
    " application is restarted.";
			// 
			// FormPlugInManager
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.ClientSize = new System.Drawing.Size(753, 557);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.panel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormPlugInManager";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Plug in manager";
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ToolStripButtons.ResumeLayout(false);
			this.ToolStripButtons.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ButtonOK;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ListView ListPlugins;
        private System.Windows.Forms.ColumnHeader ColumnPluginName;
        private System.Windows.Forms.ColumnHeader ColumnStatus;
        private System.Windows.Forms.ColumnHeader ColumnPath;
        private System.Windows.Forms.ToolStrip ToolStripButtons;
        private System.Windows.Forms.ToolStripButton ButtonAddPlugins;
        private System.Windows.Forms.ToolStripButton ButtonRemovePlugins;
    }
}