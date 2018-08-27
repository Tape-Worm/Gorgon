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
            this.PanelInfo = new System.Windows.Forms.Panel();
            this.LabelInfo = new ComponentFactory.Krypton.Toolkit.KryptonWrapLabel();
            this.ButtonCreate = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.WorkspaceBrowser = new Gorgon.UI.GorgonFolderBrowser();
            this.TextName = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.LabelHeader = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.PanelNew)).BeginInit();
            this.PanelNew.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PanelsControls)).BeginInit();
            this.PanelsControls.SuspendLayout();
            this.PanelInfo.SuspendLayout();
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
            this.PanelsControls.Controls.Add(this.PanelInfo);
            this.PanelsControls.Controls.Add(this.ButtonCreate);
            this.PanelsControls.Controls.Add(this.WorkspaceBrowser);
            this.PanelsControls.Controls.Add(this.TextName);
            this.PanelsControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelsControls.Location = new System.Drawing.Point(0, 1);
            this.PanelsControls.Name = "PanelsControls";
            this.PanelsControls.Size = new System.Drawing.Size(706, 540);
            this.PanelsControls.TabIndex = 0;
            // 
            // PanelInfo
            // 
            this.PanelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelInfo.AutoScroll = true;
            this.PanelInfo.BackColor = System.Drawing.Color.Transparent;
            this.PanelInfo.Controls.Add(this.LabelInfo);
            this.PanelInfo.Location = new System.Drawing.Point(10, 415);
            this.PanelInfo.Name = "PanelInfo";
            this.PanelInfo.Size = new System.Drawing.Size(683, 69);
            this.PanelInfo.TabIndex = 7;
            this.PanelInfo.Resize += new System.EventHandler(this.PanelInfo_Resize);
            // 
            // LabelInfo
            // 
            this.LabelInfo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.LabelInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(57)))), ((int)(((byte)(91)))));
            this.LabelInfo.Location = new System.Drawing.Point(0, 0);
            this.LabelInfo.MaximumSize = new System.Drawing.Size(683, 69);
            this.LabelInfo.Name = "LabelInfo";
            this.LabelInfo.Size = new System.Drawing.Size(0, 15);
            // 
            // ButtonCreate
            // 
            this.ButtonCreate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCreate.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Custom2;
            this.ButtonCreate.Location = new System.Drawing.Point(10, 487);
            this.ButtonCreate.Name = "ButtonCreate";
            this.ButtonCreate.Size = new System.Drawing.Size(683, 39);
            this.ButtonCreate.TabIndex = 5;
            this.ButtonCreate.Values.Text = "Create Project";
            this.ButtonCreate.Click += new System.EventHandler(this.ButtonCreate_Click);
            // 
            // WorkspaceBrowser
            // 
            this.WorkspaceBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WorkspaceBrowser.BackColor = System.Drawing.Color.Transparent;
            this.WorkspaceBrowser.CaptionFont = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WorkspaceBrowser.CdRomImage = global::Gorgon.Editor.Properties.Resources.drive_cdrom_48x48;
            this.WorkspaceBrowser.DirectoryImage = global::Gorgon.Editor.Properties.Resources.folder_48x48;
            this.WorkspaceBrowser.DirectoryListFont = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WorkspaceBrowser.DirectoryNameFont = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WorkspaceBrowser.FileImage = global::Gorgon.Editor.Properties.Resources.file_48x48;
            this.WorkspaceBrowser.ForeColor = System.Drawing.Color.White;
            this.WorkspaceBrowser.HardDriveImage = global::Gorgon.Editor.Properties.Resources.drive_48x48;
            this.WorkspaceBrowser.Location = new System.Drawing.Point(10, 40);
            this.WorkspaceBrowser.Margin = new System.Windows.Forms.Padding(4);
            this.WorkspaceBrowser.Name = "WorkspaceBrowser";
            this.WorkspaceBrowser.NetworkDriveImage = global::Gorgon.Editor.Properties.Resources.drive_network_48x48;
            this.WorkspaceBrowser.RamDriveImage = global::Gorgon.Editor.Properties.Resources.drive_ram_48x48;
            this.WorkspaceBrowser.RemovableDriveImage = global::Gorgon.Editor.Properties.Resources.drive_remove_48x48;
            this.WorkspaceBrowser.Size = new System.Drawing.Size(683, 371);
            this.WorkspaceBrowser.TabIndex = 3;
            this.WorkspaceBrowser.Text = "Select a workspace directory";
            this.WorkspaceBrowser.FolderSelected += new System.EventHandler<Gorgon.UI.FolderSelectedArgs>(this.WorkspaceBrowser_FolderSelected);
            this.WorkspaceBrowser.FolderEntered += new System.EventHandler<Gorgon.UI.FolderSelectedArgs>(this.WorkspaceBrowser_FolderSelected);
            this.WorkspaceBrowser.Enter += new System.EventHandler(this.FolderBrowser_Enter);
            // 
            // TextName
            // 
            this.TextName.AlwaysActive = false;
            this.TextName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextName.InputControlStyle = ComponentFactory.Krypton.Toolkit.InputControlStyle.Custom1;
            this.TextName.Location = new System.Drawing.Point(10, 5);
            this.TextName.Name = "TextName";
            this.TextName.Size = new System.Drawing.Size(683, 28);
            this.TextName.TabIndex = 0;
            this.TextName.Text = "Untitled";
            this.TextName.TextChanged += new System.EventHandler(this.TextName_TextChanged);
            this.TextName.Enter += new System.EventHandler(this.TextName_Enter);
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
            this.PanelsControls.PerformLayout();
            this.PanelInfo.ResumeLayout(false);
            this.PanelInfo.PerformLayout();
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
        private ComponentFactory.Krypton.Toolkit.KryptonWrapLabel LabelInfo;
        private Gorgon.UI.GorgonFolderBrowser WorkspaceBrowser;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private ComponentFactory.Krypton.Toolkit.KryptonButton ButtonCreate;
        private System.Windows.Forms.Panel PanelInfo;
    }
}
