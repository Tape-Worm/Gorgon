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
                DataContext?.Unload();

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
            this.components = new System.ComponentModel.Container();
            this.PanelNew = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.PanelsControls = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ButtonCreate = new System.Windows.Forms.Button();
            this.panel6 = new System.Windows.Forms.Panel();
            this.PanelLocateText = new System.Windows.Forms.Panel();
            this.TextProjectPath = new Gorgon.UI.GorgonCueTextBox();
            this.PanelLocate = new System.Windows.Forms.Panel();
            this.ButtonSelect = new System.Windows.Forms.Button();
            this.PanelWorkspace = new System.Windows.Forms.FlowLayoutPanel();
            this.kryptonLabel1 = new System.Windows.Forms.Label();
            this.LabelDriveSpace = new System.Windows.Forms.Label();
            this.kryptonLabel2 = new System.Windows.Forms.Label();
            this.LabelActiveGpu = new System.Windows.Forms.Label();
            this.kryptonLabel4 = new System.Windows.Forms.Label();
            this.LabelRam = new System.Windows.Forms.Label();
            this.PanelTitle = new System.Windows.Forms.Panel();
            this.LabelProjectTitle = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.LabelHeader = new System.Windows.Forms.Label();
            this.TipError = new System.Windows.Forms.ToolTip(this.components);
            this.PanelNew.SuspendLayout();
            this.panel2.SuspendLayout();
            this.PanelsControls.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel6.SuspendLayout();
            this.PanelLocateText.SuspendLayout();
            this.PanelLocate.SuspendLayout();
            this.PanelWorkspace.SuspendLayout();
            this.PanelTitle.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
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
            this.panel2.Location = new System.Drawing.Point(0, 33);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.panel2.Size = new System.Drawing.Size(712, 554);
            this.panel2.TabIndex = 4;
            // 
            // PanelsControls
            // 
            this.PanelsControls.Controls.Add(this.tableLayoutPanel1);
            this.PanelsControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelsControls.Location = new System.Drawing.Point(0, 1);
            this.PanelsControls.Name = "PanelsControls";
            this.PanelsControls.Size = new System.Drawing.Size(712, 553);
            this.PanelsControls.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.ButtonCreate, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.panel6, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.PanelWorkspace, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.PanelTitle, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 66.66666F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(712, 553);
            this.tableLayoutPanel1.TabIndex = 9;
            // 
            // ButtonCreate
            // 
            this.ButtonCreate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCreate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.ButtonCreate.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ButtonCreate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonCreate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonCreate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonCreate.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.ButtonCreate.Location = new System.Drawing.Point(64, 273);
            this.ButtonCreate.Margin = new System.Windows.Forms.Padding(64, 16, 64, 0);
            this.ButtonCreate.Name = "ButtonCreate";
            this.ButtonCreate.Size = new System.Drawing.Size(584, 52);
            this.ButtonCreate.TabIndex = 5;
            this.ButtonCreate.Text = "Create Project";
            this.ButtonCreate.UseVisualStyleBackColor = false;
            this.ButtonCreate.Click += new System.EventHandler(this.ButtonCreate_Click);
            // 
            // panel6
            // 
            this.panel6.AutoSize = true;
            this.panel6.Controls.Add(this.PanelLocateText);
            this.panel6.Controls.Add(this.PanelLocate);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.Location = new System.Drawing.Point(3, 155);
            this.panel6.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(706, 25);
            this.panel6.TabIndex = 1;
            // 
            // PanelLocateText
            // 
            this.PanelLocateText.AutoSize = true;
            this.PanelLocateText.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PanelLocateText.Controls.Add(this.TextProjectPath);
            this.PanelLocateText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelLocateText.Location = new System.Drawing.Point(0, 0);
            this.PanelLocateText.Margin = new System.Windows.Forms.Padding(0);
            this.PanelLocateText.Name = "PanelLocateText";
            this.PanelLocateText.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.PanelLocateText.Size = new System.Drawing.Size(662, 25);
            this.PanelLocateText.TabIndex = 9;
            // 
            // TextProjectPath
            // 
            this.TextProjectPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextProjectPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.TextProjectPath.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextProjectPath.CueText = "Enter the directory where your project will be stored...";
            this.TextProjectPath.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.TextProjectPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.TextProjectPath.Location = new System.Drawing.Point(0, 0);
            this.TextProjectPath.Margin = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.TextProjectPath.MaxLength = 100;
            this.TextProjectPath.Name = "TextProjectPath";
            this.TextProjectPath.Size = new System.Drawing.Size(662, 22);
            this.TextProjectPath.TabIndex = 0;
            this.TextProjectPath.Enter += new System.EventHandler(this.TextName_Enter);
            this.TextProjectPath.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextProjectPath_KeyDown);
            this.TextProjectPath.Leave += new System.EventHandler(this.TextName_Leave);
            this.TextProjectPath.MouseEnter += new System.EventHandler(this.TextName_MouseEnter);
            this.TextProjectPath.MouseLeave += new System.EventHandler(this.TextName_MouseLeave);
            // 
            // PanelLocate
            // 
            this.PanelLocate.Controls.Add(this.ButtonSelect);
            this.PanelLocate.Dock = System.Windows.Forms.DockStyle.Right;
            this.PanelLocate.Location = new System.Drawing.Point(662, 0);
            this.PanelLocate.Margin = new System.Windows.Forms.Padding(0);
            this.PanelLocate.Name = "PanelLocate";
            this.PanelLocate.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.PanelLocate.Size = new System.Drawing.Size(44, 25);
            this.PanelLocate.TabIndex = 8;
            // 
            // ButtonSelect
            // 
            this.ButtonSelect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.ButtonSelect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonSelect.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ButtonSelect.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonSelect.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonSelect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonSelect.Location = new System.Drawing.Point(3, 0);
            this.ButtonSelect.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonSelect.Name = "ButtonSelect";
            this.ButtonSelect.Size = new System.Drawing.Size(38, 22);
            this.ButtonSelect.TabIndex = 7;
            this.ButtonSelect.Text = "...";
            this.ButtonSelect.UseVisualStyleBackColor = false;
            this.ButtonSelect.Click += new System.EventHandler(this.ButtonSelect_Click);
            // 
            // PanelWorkspace
            // 
            this.PanelWorkspace.AutoSize = true;
            this.PanelWorkspace.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PanelWorkspace.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.PanelWorkspace.Controls.Add(this.kryptonLabel1);
            this.PanelWorkspace.Controls.Add(this.LabelDriveSpace);
            this.PanelWorkspace.Controls.Add(this.kryptonLabel2);
            this.PanelWorkspace.Controls.Add(this.LabelActiveGpu);
            this.PanelWorkspace.Controls.Add(this.kryptonLabel4);
            this.PanelWorkspace.Controls.Add(this.LabelRam);
            this.PanelWorkspace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelWorkspace.Location = new System.Drawing.Point(3, 191);
            this.PanelWorkspace.Name = "PanelWorkspace";
            this.PanelWorkspace.Size = new System.Drawing.Size(706, 63);
            this.PanelWorkspace.TabIndex = 8;
            // 
            // kryptonLabel1
            // 
            this.kryptonLabel1.AutoSize = true;
            this.kryptonLabel1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.kryptonLabel1.Location = new System.Drawing.Point(0, 3);
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
            this.LabelDriveSpace.Location = new System.Drawing.Point(153, 3);
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
            this.kryptonLabel2.Location = new System.Drawing.Point(0, 24);
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
            this.LabelActiveGpu.Location = new System.Drawing.Point(69, 24);
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
            this.kryptonLabel4.Location = new System.Drawing.Point(0, 45);
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
            this.LabelRam.Location = new System.Drawing.Point(87, 45);
            this.LabelRam.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.LabelRam.Name = "LabelRam";
            this.LabelRam.Size = new System.Drawing.Size(97, 15);
            this.LabelRam.TabIndex = 13;
            this.LabelRam.Text = "64k (65536 bytes)";
            // 
            // PanelTitle
            // 
            this.PanelTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelTitle.AutoSize = true;
            this.PanelTitle.Controls.Add(this.LabelProjectTitle);
            this.PanelTitle.Location = new System.Drawing.Point(3, 112);
            this.PanelTitle.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.PanelTitle.Name = "PanelTitle";
            this.PanelTitle.Size = new System.Drawing.Size(706, 32);
            this.PanelTitle.TabIndex = 9;
            // 
            // LabelProjectTitle
            // 
            this.LabelProjectTitle.AutoSize = true;
            this.LabelProjectTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelProjectTitle.Font = new System.Drawing.Font("Segoe UI", 18F);
            this.LabelProjectTitle.Location = new System.Drawing.Point(0, 0);
            this.LabelProjectTitle.Name = "LabelProjectTitle";
            this.LabelProjectTitle.Size = new System.Drawing.Size(0, 32);
            this.LabelProjectTitle.TabIndex = 0;
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
            this.panel3.Size = new System.Drawing.Size(712, 33);
            this.panel3.TabIndex = 5;
            // 
            // panel4
            // 
            this.panel4.AutoSize = true;
            this.panel4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panel4.Controls.Add(this.LabelHeader);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(3, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(709, 32);
            this.panel4.TabIndex = 1;
            // 
            // LabelHeader
            // 
            this.LabelHeader.AutoSize = true;
            this.LabelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.LabelHeader.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelHeader.Location = new System.Drawing.Point(0, 0);
            this.LabelHeader.Name = "LabelHeader";
            this.LabelHeader.Size = new System.Drawing.Size(143, 32);
            this.LabelHeader.TabIndex = 0;
            this.LabelHeader.Text = "New Project";
            this.LabelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TipError
            // 
            this.TipError.BackColor = System.Drawing.Color.White;
            this.TipError.ForeColor = System.Drawing.Color.Black;
            this.TipError.ShowAlways = true;
            this.TipError.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Error;
            this.TipError.ToolTipTitle = "Invalid Path";
            // 
            // StageNew
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PanelNew);
            this.Name = "StageNew";
            this.Size = new System.Drawing.Size(712, 587);
            this.PanelNew.ResumeLayout(false);
            this.PanelNew.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.PanelsControls.ResumeLayout(false);
            this.PanelsControls.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.PanelLocateText.ResumeLayout(false);
            this.PanelLocateText.PerformLayout();
            this.PanelLocate.ResumeLayout(false);
            this.PanelWorkspace.ResumeLayout(false);
            this.PanelWorkspace.PerformLayout();
            this.PanelTitle.ResumeLayout(false);
            this.PanelTitle.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel PanelNew;
        private System.Windows.Forms.Label LabelHeader;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel PanelsControls;
        private Gorgon.UI.GorgonCueTextBox TextProjectPath;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button ButtonCreate;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel PanelWorkspace;
        private System.Windows.Forms.Label kryptonLabel1;
        private System.Windows.Forms.Label LabelDriveSpace;
        private System.Windows.Forms.Label kryptonLabel2;
        private System.Windows.Forms.Label LabelActiveGpu;
        private System.Windows.Forms.Label kryptonLabel4;
        private System.Windows.Forms.Label LabelRam;
        private Panel panel6;
        private System.Windows.Forms.Button ButtonSelect;
        private Panel PanelLocate;
        private Panel PanelLocateText;
        private Panel PanelTitle;
        private Label LabelProjectTitle;
        private ToolTip TipError;
    }
}
