using System.Windows.Forms;
using Gorgon.UI;

namespace Gorgon.Editor.Views
{
    partial class StageNew
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
            if (disposing)
            {
                DataContext?.OnUnload();

                UnassignEvents();
            }

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
            this.PanelNew = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.PanelsControls = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ButtonCreate = new System.Windows.Forms.Button();
            this.panel6 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.PanelUnderline = new System.Windows.Forms.Panel();
            this.TextName = new System.Windows.Forms.TextBox();
            this.PanelWorkspace = new System.Windows.Forms.FlowLayoutPanel();
            this.LabelWorkspace = new System.Windows.Forms.Label();
            this.LabelWorkspaceLocation = new System.Windows.Forms.Label();
            this.kryptonLabel1 = new System.Windows.Forms.Label();
            this.LabelDriveSpace = new System.Windows.Forms.Label();
            this.kryptonLabel2 = new System.Windows.Forms.Label();
            this.LabelActiveGpu = new System.Windows.Forms.Label();
            this.kryptonLabel4 = new System.Windows.Forms.Label();
            this.LabelRam = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.LabelHeader = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.PanelNew.SuspendLayout();
            this.panel2.SuspendLayout();
            this.PanelsControls.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel5.SuspendLayout();
            this.PanelWorkspace.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelNew
            // 
            this.PanelNew.Controls.Add(this.panel2);
            this.PanelNew.Controls.Add(this.panel3);
            this.PanelNew.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelNew.Location = new System.Drawing.Point(0, 0);
            this.PanelNew.Name = "PanelNew";
            this.PanelNew.Size = new System.Drawing.Size(712, 587);
            this.PanelNew.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.PanelsControls);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 35);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.panel2.Size = new System.Drawing.Size(712, 552);
            this.panel2.TabIndex = 4;
            // 
            // PanelsControls
            // 
            this.PanelsControls.Controls.Add(this.tableLayoutPanel1);
            this.PanelsControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelsControls.Location = new System.Drawing.Point(0, 1);
            this.PanelsControls.Name = "PanelsControls";
            this.PanelsControls.Size = new System.Drawing.Size(712, 551);
            this.PanelsControls.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.ButtonCreate, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel6, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.PanelWorkspace, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(712, 551);
            this.tableLayoutPanel1.TabIndex = 9;
            // 
            // ButtonCreate
            // 
            this.ButtonCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCreate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonCreate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonCreate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonCreate.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.ButtonCreate.Location = new System.Drawing.Point(64, 346);
            this.ButtonCreate.Margin = new System.Windows.Forms.Padding(64, 64, 64, 0);
            this.ButtonCreate.Name = "ButtonCreate";
            this.ButtonCreate.Size = new System.Drawing.Size(584, 52);
            this.ButtonCreate.TabIndex = 5;
            this.ButtonCreate.Text = "Create Project";
            this.ButtonCreate.Click += new System.EventHandler(this.ButtonCreate_Click);
            // 
            // panel6
            // 
            this.panel6.AutoSize = true;
            this.panel6.Controls.Add(this.panel5);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.Location = new System.Drawing.Point(3, 3);
            this.panel6.Name = "panel6";
            this.panel6.Padding = new System.Windows.Forms.Padding(0, 16, 0, 3);
            this.panel6.Size = new System.Drawing.Size(706, 44);
            this.panel6.TabIndex = 1;
            // 
            // panel5
            // 
            this.panel5.AutoSize = true;
            this.panel5.Controls.Add(this.PanelUnderline);
            this.panel5.Controls.Add(this.TextName);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(0, 16);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(706, 25);
            this.panel5.TabIndex = 1;
            // 
            // PanelUnderline
            // 
            this.PanelUnderline.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.PanelUnderline.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PanelUnderline.Location = new System.Drawing.Point(0, 23);
            this.PanelUnderline.Name = "PanelUnderline";
            this.PanelUnderline.Size = new System.Drawing.Size(706, 2);
            this.PanelUnderline.TabIndex = 1;
            // 
            // TextName
            // 
            this.TextName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.TextName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextName.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.TextName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.TextName.Location = new System.Drawing.Point(0, 0);
            this.TextName.MaxLength = 100;
            this.TextName.Name = "TextName";
            this.TextName.Size = new System.Drawing.Size(706, 22);
            this.TextName.TabIndex = 0;
            this.TextName.Text = "Untitled";
            this.TextName.TextChanged += new System.EventHandler(this.TextName_TextChanged);
            this.TextName.Enter += new System.EventHandler(this.TextName_Enter);
            this.TextName.Leave += new System.EventHandler(this.TextName_Leave);
            this.TextName.MouseEnter += new System.EventHandler(this.TextName_MouseEnter);
            this.TextName.MouseLeave += new System.EventHandler(this.TextName_MouseLeave);
            // 
            // PanelWorkspace
            // 
            this.PanelWorkspace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelWorkspace.AutoSize = true;
            this.PanelWorkspace.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PanelWorkspace.Controls.Add(this.LabelWorkspace);
            this.PanelWorkspace.Controls.Add(this.LabelWorkspaceLocation);
            this.PanelWorkspace.Controls.Add(this.kryptonLabel1);
            this.PanelWorkspace.Controls.Add(this.LabelDriveSpace);
            this.PanelWorkspace.Controls.Add(this.kryptonLabel2);
            this.PanelWorkspace.Controls.Add(this.LabelActiveGpu);
            this.PanelWorkspace.Controls.Add(this.kryptonLabel4);
            this.PanelWorkspace.Controls.Add(this.LabelRam);
            this.PanelWorkspace.Location = new System.Drawing.Point(3, 53);
            this.PanelWorkspace.Name = "PanelWorkspace";
            this.PanelWorkspace.Size = new System.Drawing.Size(706, 93);
            this.PanelWorkspace.TabIndex = 8;
            // 
            // LabelWorkspace
            // 
            this.LabelWorkspace.AutoSize = true;
            this.LabelWorkspace.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelWorkspace.Location = new System.Drawing.Point(0, 12);
            this.LabelWorkspace.Margin = new System.Windows.Forms.Padding(0, 12, 0, 3);
            this.LabelWorkspace.Name = "LabelWorkspace";
            this.LabelWorkspace.Size = new System.Drawing.Size(115, 15);
            this.LabelWorkspace.TabIndex = 6;
            this.LabelWorkspace.Text = "Workspace location:";
            // 
            // LabelWorkspaceLocation
            // 
            this.LabelWorkspaceLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelWorkspaceLocation.AutoEllipsis = true;
            this.LabelWorkspaceLocation.AutoSize = true;
            this.PanelWorkspace.SetFlowBreak(this.LabelWorkspaceLocation, true);
            this.LabelWorkspaceLocation.Location = new System.Drawing.Point(115, 12);
            this.LabelWorkspaceLocation.Margin = new System.Windows.Forms.Padding(0, 12, 0, 3);
            this.LabelWorkspaceLocation.Name = "LabelWorkspaceLocation";
            this.LabelWorkspaceLocation.Size = new System.Drawing.Size(92, 15);
            this.LabelWorkspaceLocation.TabIndex = 7;
            this.LabelWorkspaceLocation.Text = "c:\\temp\\sample";
            // 
            // kryptonLabel1
            // 
            this.kryptonLabel1.AutoSize = true;
            this.kryptonLabel1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.kryptonLabel1.Location = new System.Drawing.Point(0, 33);
            this.kryptonLabel1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.kryptonLabel1.Name = "kryptonLabel1";
            this.kryptonLabel1.Size = new System.Drawing.Size(153, 15);
            this.kryptonLabel1.TabIndex = 8;
            this.kryptonLabel1.Text = "Free workspace drive space:";
            // 
            // LabelDriveSpace
            // 
            this.LabelDriveSpace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelDriveSpace.AutoEllipsis = true;
            this.LabelDriveSpace.AutoSize = true;
            this.PanelWorkspace.SetFlowBreak(this.LabelDriveSpace, true);
            this.LabelDriveSpace.Location = new System.Drawing.Point(153, 33);
            this.LabelDriveSpace.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.LabelDriveSpace.Name = "LabelDriveSpace";
            this.LabelDriveSpace.Size = new System.Drawing.Size(213, 15);
            this.LabelDriveSpace.TabIndex = 9;
            this.LabelDriveSpace.Text = "8GB (8,000,000,000 bytes or something)";
            // 
            // kryptonLabel2
            // 
            this.kryptonLabel2.AutoSize = true;
            this.kryptonLabel2.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.kryptonLabel2.Location = new System.Drawing.Point(0, 54);
            this.kryptonLabel2.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.kryptonLabel2.Name = "kryptonLabel2";
            this.kryptonLabel2.Size = new System.Drawing.Size(69, 15);
            this.kryptonLabel2.TabIndex = 10;
            this.kryptonLabel2.Text = "Active GPU:";
            // 
            // LabelActiveGpu
            // 
            this.LabelActiveGpu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelActiveGpu.AutoEllipsis = true;
            this.LabelActiveGpu.AutoSize = true;
            this.PanelWorkspace.SetFlowBreak(this.LabelActiveGpu, true);
            this.LabelActiveGpu.Location = new System.Drawing.Point(69, 54);
            this.LabelActiveGpu.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.LabelActiveGpu.Name = "LabelActiveGpu";
            this.LabelActiveGpu.Size = new System.Drawing.Size(133, 15);
            this.LabelActiveGpu.TabIndex = 11;
            this.LabelActiveGpu.Text = "Nvidia bitchin\' fast 2000";
            // 
            // kryptonLabel4
            // 
            this.kryptonLabel4.AutoSize = true;
            this.kryptonLabel4.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.kryptonLabel4.Location = new System.Drawing.Point(0, 75);
            this.kryptonLabel4.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.kryptonLabel4.Name = "kryptonLabel4";
            this.kryptonLabel4.Size = new System.Drawing.Size(87, 15);
            this.kryptonLabel4.TabIndex = 12;
            this.kryptonLabel4.Text = "Available RAM:";
            // 
            // LabelRam
            // 
            this.LabelRam.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelRam.AutoEllipsis = true;
            this.LabelRam.AutoSize = true;
            this.LabelRam.Location = new System.Drawing.Point(87, 75);
            this.LabelRam.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.LabelRam.Name = "LabelRam";
            this.LabelRam.Size = new System.Drawing.Size(97, 15);
            this.LabelRam.TabIndex = 13;
            this.LabelRam.Text = "64k (65536 bytes)";
            // 
            // panel3
            // 
            this.panel3.AutoSize = true;
            this.panel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel3.BackColor = System.Drawing.Color.SteelBlue;
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(3, 0, 0, 1);
            this.panel3.Size = new System.Drawing.Size(712, 35);
            this.panel3.TabIndex = 5;
            // 
            // panel4
            // 
            this.panel4.AutoSize = true;
            this.panel4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.panel4.Controls.Add(this.LabelHeader);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(3, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(709, 34);
            this.panel4.TabIndex = 1;
            // 
            // LabelHeader
            // 
            this.LabelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.LabelHeader.Font = new System.Drawing.Font("Segoe UI", 13.5F);
            this.LabelHeader.Location = new System.Drawing.Point(0, 0);
            this.LabelHeader.Name = "LabelHeader";
            this.LabelHeader.Size = new System.Drawing.Size(709, 34);
            this.LabelHeader.TabIndex = 0;
            this.LabelHeader.Text = "New Project";
            this.LabelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.PanelNew);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(712, 587);
            this.panel1.TabIndex = 1;
            // 
            // StageNew
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Name = "StageNew";
            this.Size = new System.Drawing.Size(712, 587);
            this.PanelNew.ResumeLayout(false);
            this.PanelNew.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.PanelsControls.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.PanelWorkspace.ResumeLayout(false);
            this.PanelWorkspace.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel PanelNew;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label LabelHeader;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel PanelsControls;
        private System.Windows.Forms.TextBox TextName;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button ButtonCreate;
        private System.Windows.Forms.Label LabelWorkspace;
        private System.Windows.Forms.Label LabelWorkspaceLocation;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel PanelWorkspace;
        private System.Windows.Forms.Label kryptonLabel1;
        private System.Windows.Forms.Label LabelDriveSpace;
        private System.Windows.Forms.Label kryptonLabel2;
        private System.Windows.Forms.Label LabelActiveGpu;
        private System.Windows.Forms.Label kryptonLabel4;
        private System.Windows.Forms.Label LabelRam;
        private Panel PanelUnderline;
        private Panel panel6;
        private Panel panel5;
    }
}
