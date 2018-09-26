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
            this.PanelNew = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.PanelsControls = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ButtonCreate = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.TextName = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
            this.PanelWorkspace = new System.Windows.Forms.FlowLayoutPanel();
            this.LabelWorkspaceLocation = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.LabelWorkspace = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.kryptonLabel1 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.LabelDriveSpace = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.kryptonLabel2 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.LabelActiveGpu = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.kryptonLabel4 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.LabelRam = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.LabelHeader = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.PanelNew)).BeginInit();
            this.PanelNew.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PanelsControls)).BeginInit();
            this.PanelsControls.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
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
            this.PanelNew.Location = new System.Drawing.Point(0, 6);
            this.PanelNew.Name = "PanelNew";
            this.PanelNew.PanelBackStyle = ComponentFactory.Krypton.Toolkit.PaletteBackStyle.PanelCustom1;
            this.PanelNew.Size = new System.Drawing.Size(706, 575);
            this.PanelNew.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.PanelsControls);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 34);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.panel2.Size = new System.Drawing.Size(706, 541);
            this.panel2.TabIndex = 4;
            // 
            // PanelsControls
            // 
            this.PanelsControls.Controls.Add(this.tableLayoutPanel1);
            this.PanelsControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelsControls.Location = new System.Drawing.Point(0, 1);
            this.PanelsControls.Name = "PanelsControls";
            this.PanelsControls.Size = new System.Drawing.Size(706, 540);
            this.PanelsControls.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.ButtonCreate, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.PanelWorkspace, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(706, 540);
            this.tableLayoutPanel1.TabIndex = 9;
            // 
            // ButtonCreate
            // 
            this.ButtonCreate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCreate.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Custom2;
            this.ButtonCreate.Location = new System.Drawing.Point(64, 230);
            this.ButtonCreate.Margin = new System.Windows.Forms.Padding(64, 64, 64, 0);
            this.ButtonCreate.Name = "ButtonCreate";
            this.ButtonCreate.Size = new System.Drawing.Size(578, 28);
            this.ButtonCreate.TabIndex = 5;
            this.ButtonCreate.Values.Text = "Create Project";
            this.ButtonCreate.Click += new System.EventHandler(this.ButtonCreate_Click);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.TextName, 0, 1);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 16);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0, 16, 0, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(706, 28);
            this.tableLayoutPanel2.TabIndex = 6;
            // 
            // TextName
            // 
            this.TextName.AlwaysActive = false;
            this.TextName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextName.InputControlStyle = ComponentFactory.Krypton.Toolkit.InputControlStyle.Custom1;
            this.TextName.Location = new System.Drawing.Point(3, 0);
            this.TextName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.TextName.Name = "TextName";
            this.TextName.Size = new System.Drawing.Size(700, 28);
            this.TextName.TabIndex = 0;
            this.TextName.Text = "Untitled";
            this.TextName.TextChanged += new System.EventHandler(this.TextName_TextChanged);
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
            this.PanelWorkspace.Location = new System.Drawing.Point(3, 50);
            this.PanelWorkspace.Name = "PanelWorkspace";
            this.PanelWorkspace.Size = new System.Drawing.Size(700, 113);
            this.PanelWorkspace.TabIndex = 8;
            // 
            // LabelWorkspaceLocation
            // 
            this.LabelWorkspaceLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelWorkspace.SetFlowBreak(this.LabelWorkspaceLocation, true);
            this.LabelWorkspaceLocation.LabelStyle = ComponentFactory.Krypton.Toolkit.LabelStyle.TitlePanel;
            this.LabelWorkspaceLocation.Location = new System.Drawing.Point(114, 12);
            this.LabelWorkspaceLocation.Margin = new System.Windows.Forms.Padding(0, 12, 0, 3);
            this.LabelWorkspaceLocation.Name = "LabelWorkspaceLocation";
            this.LabelWorkspaceLocation.Size = new System.Drawing.Size(96, 20);
            this.LabelWorkspaceLocation.TabIndex = 7;
            this.LabelWorkspaceLocation.Values.Text = "c:\\temp\\sample";
            // 
            // LabelWorkspace
            // 
            this.LabelWorkspace.Location = new System.Drawing.Point(0, 12);
            this.LabelWorkspace.Margin = new System.Windows.Forms.Padding(0, 12, 0, 3);
            this.LabelWorkspace.Name = "LabelWorkspace";
            this.LabelWorkspace.Size = new System.Drawing.Size(114, 18);
            this.LabelWorkspace.TabIndex = 6;
            this.LabelWorkspace.Values.Text = "Workspace location:";
            // 
            // kryptonLabel1
            // 
            this.kryptonLabel1.Location = new System.Drawing.Point(0, 38);
            this.kryptonLabel1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.kryptonLabel1.Name = "kryptonLabel1";
            this.kryptonLabel1.Size = new System.Drawing.Size(156, 18);
            this.kryptonLabel1.TabIndex = 8;
            this.kryptonLabel1.Values.Text = "Free workspace drive space:";
            // 
            // LabelDriveSpace
            // 
            this.LabelDriveSpace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelWorkspace.SetFlowBreak(this.LabelDriveSpace, true);
            this.LabelDriveSpace.LabelStyle = ComponentFactory.Krypton.Toolkit.LabelStyle.TitlePanel;
            this.LabelDriveSpace.Location = new System.Drawing.Point(156, 38);
            this.LabelDriveSpace.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.LabelDriveSpace.Name = "LabelDriveSpace";
            this.LabelDriveSpace.Size = new System.Drawing.Size(227, 20);
            this.LabelDriveSpace.TabIndex = 9;
            this.LabelDriveSpace.Values.Text = "8GB (8,000,000,000 bytes or something)";
            // 
            // kryptonLabel2
            // 
            this.kryptonLabel2.Location = new System.Drawing.Point(0, 64);
            this.kryptonLabel2.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.kryptonLabel2.Name = "kryptonLabel2";
            this.kryptonLabel2.Size = new System.Drawing.Size(68, 18);
            this.kryptonLabel2.TabIndex = 10;
            this.kryptonLabel2.Values.Text = "Active GPU:";
            // 
            // LabelActiveGpu
            // 
            this.LabelActiveGpu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelWorkspace.SetFlowBreak(this.LabelActiveGpu, true);
            this.LabelActiveGpu.LabelStyle = ComponentFactory.Krypton.Toolkit.LabelStyle.TitlePanel;
            this.LabelActiveGpu.Location = new System.Drawing.Point(68, 64);
            this.LabelActiveGpu.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.LabelActiveGpu.Name = "LabelActiveGpu";
            this.LabelActiveGpu.Size = new System.Drawing.Size(142, 20);
            this.LabelActiveGpu.TabIndex = 11;
            this.LabelActiveGpu.Values.Text = "Nvidia bitchin\' fast 2000";
            // 
            // kryptonLabel4
            // 
            this.kryptonLabel4.Location = new System.Drawing.Point(0, 90);
            this.kryptonLabel4.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.kryptonLabel4.Name = "kryptonLabel4";
            this.kryptonLabel4.Size = new System.Drawing.Size(86, 18);
            this.kryptonLabel4.TabIndex = 12;
            this.kryptonLabel4.Values.Text = "Available RAM:";
            // 
            // LabelRam
            // 
            this.LabelRam.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelRam.LabelStyle = ComponentFactory.Krypton.Toolkit.LabelStyle.TitlePanel;
            this.LabelRam.Location = new System.Drawing.Point(86, 90);
            this.LabelRam.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.LabelRam.Name = "LabelRam";
            this.LabelRam.Size = new System.Drawing.Size(107, 20);
            this.LabelRam.TabIndex = 13;
            this.LabelRam.Values.Text = "64k (65536 bytes)";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.SteelBlue;
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.panel3.Size = new System.Drawing.Size(706, 34);
            this.panel3.TabIndex = 5;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.panel4.Controls.Add(this.LabelHeader);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(6, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(700, 100);
            this.panel4.TabIndex = 1;
            // 
            // LabelHeader
            // 
            this.LabelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.LabelHeader.LabelStyle = ComponentFactory.Krypton.Toolkit.LabelStyle.Custom2;
            this.LabelHeader.Location = new System.Drawing.Point(0, 0);
            this.LabelHeader.Name = "LabelHeader";
            this.LabelHeader.Size = new System.Drawing.Size(700, 34);
            this.LabelHeader.TabIndex = 0;
            this.LabelHeader.Values.Text = "New Project";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.PanelNew);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(0, 6, 6, 6);
            this.panel1.Size = new System.Drawing.Size(712, 587);
            this.panel1.TabIndex = 1;
            // 
            // StageNew
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "StageNew";
            this.Size = new System.Drawing.Size(712, 587);
            ((System.ComponentModel.ISupportInitialize)(this.PanelNew)).EndInit();
            this.PanelNew.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PanelsControls)).EndInit();
            this.PanelsControls.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.PanelWorkspace.ResumeLayout(false);
            this.PanelWorkspace.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonPanel PanelNew;
        private System.Windows.Forms.Panel panel1;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel LabelHeader;
        private System.Windows.Forms.Panel panel2;
        private ComponentFactory.Krypton.Toolkit.KryptonPanel PanelsControls;
        private ComponentFactory.Krypton.Toolkit.KryptonTextBox TextName;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private ComponentFactory.Krypton.Toolkit.KryptonButton ButtonCreate;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel LabelWorkspace;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel LabelWorkspaceLocation;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel PanelWorkspace;
        private TableLayoutPanel tableLayoutPanel2;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabel1;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel LabelDriveSpace;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabel2;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel LabelActiveGpu;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabel4;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel LabelRam;
    }
}
