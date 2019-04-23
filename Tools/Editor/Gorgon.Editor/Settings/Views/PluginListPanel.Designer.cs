namespace Gorgon.Editor.Views
{
    partial class PlugInListPanel
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlugInListPanel));
			this.TableBody = new System.Windows.Forms.TableLayoutPanel();
			this.ListPlugIns = new System.Windows.Forms.ListView();
			this.ColumnPlugInName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ColumnPlugInType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ColumnPlugInStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ColumnPlugInPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label2 = new System.Windows.Forms.Label();
			this.TextStatus = new System.Windows.Forms.TextBox();
			this.PanelBody.SuspendLayout();
			this.TableBody.SuspendLayout();
			this.SuspendLayout();
			// 
			// PanelBody
			// 
			this.PanelBody.Controls.Add(this.TableBody);
			this.PanelBody.Size = new System.Drawing.Size(600, 358);
			// 
			// TableBody
			// 
			this.TableBody.AutoSize = true;
			this.TableBody.ColumnCount = 1;
			this.TableBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableBody.Controls.Add(this.ListPlugIns, 0, 1);
			this.TableBody.Controls.Add(this.label2, 0, 0);
			this.TableBody.Controls.Add(this.TextStatus, 0, 2);
			this.TableBody.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableBody.Location = new System.Drawing.Point(0, 0);
			this.TableBody.Name = "TableBody";
			this.TableBody.RowCount = 3;
			this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 75F));
			this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.TableBody.Size = new System.Drawing.Size(600, 358);
			this.TableBody.TabIndex = 0;
			// 
			// ListPlugIns
			// 
			this.ListPlugIns.BackColor = System.Drawing.Color.White;
			this.ListPlugIns.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ListPlugIns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnPlugInName,
            this.ColumnPlugInType,
            this.ColumnPlugInStatus,
            this.ColumnPlugInPath});
			this.ListPlugIns.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ListPlugIns.ForeColor = System.Drawing.Color.Black;
			this.ListPlugIns.FullRowSelect = true;
			this.ListPlugIns.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.ListPlugIns.HideSelection = false;
			this.ListPlugIns.Location = new System.Drawing.Point(3, 69);
			this.ListPlugIns.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.ListPlugIns.MultiSelect = false;
			this.ListPlugIns.Name = "ListPlugIns";
			this.ListPlugIns.Size = new System.Drawing.Size(594, 216);
			this.ListPlugIns.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.ListPlugIns.TabIndex = 1;
			this.ListPlugIns.UseCompatibleStateImageBehavior = false;
			this.ListPlugIns.View = System.Windows.Forms.View.Details;
			this.ListPlugIns.SelectedIndexChanged += new System.EventHandler(this.ListPlugIns_SelectedIndexChanged);
			// 
			// ColumnPlugInName
			// 
			this.ColumnPlugInName.Text = "Name";
			// 
			// ColumnPlugInType
			// 
			this.ColumnPlugInType.Text = "Type";
			// 
			// ColumnPlugInStatus
			// 
			this.ColumnPlugInStatus.Text = "Status";
			// 
			// ColumnPlugInPath
			// 
			this.ColumnPlugInPath.Text = "Path";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 3);
			this.label2.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(594, 60);
			this.label2.TabIndex = 2;
			this.label2.Text = resources.GetString("label2.Text");
			// 
			// TextStatus
			// 
			this.TextStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.TextStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.TextStatus.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TextStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.TextStatus.Location = new System.Drawing.Point(3, 288);
			this.TextStatus.Multiline = true;
			this.TextStatus.Name = "TextStatus";
			this.TextStatus.ReadOnly = true;
			this.TextStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.TextStatus.Size = new System.Drawing.Size(594, 67);
			this.TextStatus.TabIndex = 3;
			this.TextStatus.Text = "Plug in\r\nloaded \r\nsuccessfully";
			// 
			// PlugInListPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Name = "PlugInListPanel";
			this.Size = new System.Drawing.Size(600, 381);
			this.Text = "Plug ins";
			this.PanelBody.ResumeLayout(false);
			this.PanelBody.PerformLayout();
			this.TableBody.ResumeLayout(false);
			this.TableBody.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel TableBody;
        private System.Windows.Forms.ListView ListPlugIns;
        private System.Windows.Forms.ColumnHeader ColumnPlugInName;
        private System.Windows.Forms.ColumnHeader ColumnPlugInType;
        private System.Windows.Forms.ColumnHeader ColumnPlugInPath;
        private System.Windows.Forms.ColumnHeader ColumnPlugInStatus;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TextStatus;
    }
}
