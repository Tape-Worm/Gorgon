namespace Gorgon.Editor.ImageEditor
{
    partial class ImageEditorView
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
            this.kryptonPanel1 = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            this.LabelZoom = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.MenuZoomItems = new ComponentFactory.Krypton.Toolkit.KryptonDropButton();
            this.ZoomMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ItemZoomToWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripMenuItem();
            this.PanelBar = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.RenderPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel1)).BeginInit();
            this.kryptonPanel1.SuspendLayout();
            this.ZoomMenu.SuspendLayout();
            this.PanelBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // RenderPanel
            // 
            this.RenderPanel.Size = new System.Drawing.Size(1211, 769);
            // 
            // kryptonPanel1
            // 
            this.kryptonPanel1.AutoSize = true;
            this.kryptonPanel1.Controls.Add(this.PanelBar);
            this.kryptonPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.kryptonPanel1.Location = new System.Drawing.Point(0, 769);
            this.kryptonPanel1.Name = "kryptonPanel1";
            this.kryptonPanel1.Size = new System.Drawing.Size(1211, 28);
            this.kryptonPanel1.TabIndex = 1;
            // 
            // LabelZoom
            // 
            this.LabelZoom.Location = new System.Drawing.Point(3, 3);
            this.LabelZoom.Name = "LabelZoom";
            this.LabelZoom.Size = new System.Drawing.Size(39, 18);
            this.LabelZoom.TabIndex = 0;
            this.LabelZoom.Values.Text = "Zoom:";
            // 
            // MenuZoomItems
            // 
            this.MenuZoomItems.AutoSize = true;
            this.MenuZoomItems.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MenuZoomItems.ContextMenuStrip = this.ZoomMenu;
            this.MenuZoomItems.DropDownOrientation = ComponentFactory.Krypton.Toolkit.VisualOrientation.Top;
            this.MenuZoomItems.Location = new System.Drawing.Point(48, 3);
            this.MenuZoomItems.Name = "MenuZoomItems";
            this.MenuZoomItems.Size = new System.Drawing.Size(87, 22);
            this.MenuZoomItems.TabIndex = 1;
            this.MenuZoomItems.Values.Text = "To Window";
            // 
            // ZoomMenu
            // 
            this.ZoomMenu.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ZoomMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ItemZoomToWindow,
            this.toolStripMenuItem1,
            this.toolStripMenuItem3,
            this.toolStripMenuItem4,
            this.toolStripMenuItem5,
            this.toolStripMenuItem7,
            this.toolStripMenuItem8,
            this.toolStripMenuItem9,
            this.toolStripMenuItem10,
            this.toolStripMenuItem11});
            this.ZoomMenu.Name = "ZoomMenu";
            this.ZoomMenu.Size = new System.Drawing.Size(135, 208);
            // 
            // ItemZoomToWindow
            // 
            this.ItemZoomToWindow.Checked = true;
            this.ItemZoomToWindow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ItemZoomToWindow.Name = "ItemZoomToWindow";
            this.ItemZoomToWindow.Size = new System.Drawing.Size(134, 22);
            this.ItemZoomToWindow.Text = "To Window";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(131, 6);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(134, 22);
            this.toolStripMenuItem3.Text = "12.5%";
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(134, 22);
            this.toolStripMenuItem4.Text = "25%";
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(134, 22);
            this.toolStripMenuItem5.Text = "50%";
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(134, 22);
            this.toolStripMenuItem7.Text = "100%";
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            this.toolStripMenuItem8.Size = new System.Drawing.Size(134, 22);
            this.toolStripMenuItem8.Text = "200%";
            // 
            // toolStripMenuItem9
            // 
            this.toolStripMenuItem9.Name = "toolStripMenuItem9";
            this.toolStripMenuItem9.Size = new System.Drawing.Size(134, 22);
            this.toolStripMenuItem9.Text = "400%";
            // 
            // toolStripMenuItem10
            // 
            this.toolStripMenuItem10.Name = "toolStripMenuItem10";
            this.toolStripMenuItem10.Size = new System.Drawing.Size(134, 22);
            this.toolStripMenuItem10.Text = "800%";
            // 
            // toolStripMenuItem11
            // 
            this.toolStripMenuItem11.Name = "toolStripMenuItem11";
            this.toolStripMenuItem11.Size = new System.Drawing.Size(134, 22);
            this.toolStripMenuItem11.Text = "1600%";
            // 
            // PanelBar
            // 
            this.PanelBar.AutoSize = true;
            this.PanelBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PanelBar.ColumnCount = 2;
            this.PanelBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.PanelBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.PanelBar.Controls.Add(this.MenuZoomItems, 1, 0);
            this.PanelBar.Controls.Add(this.LabelZoom, 0, 0);
            this.PanelBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelBar.Location = new System.Drawing.Point(0, 0);
            this.PanelBar.Name = "PanelBar";
            this.PanelBar.RowCount = 1;
            this.PanelBar.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelBar.Size = new System.Drawing.Size(1211, 28);
            this.PanelBar.TabIndex = 2;
            // 
            // ImageEditorView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.kryptonPanel1);
            this.Name = "ImageEditorView";
            this.Size = new System.Drawing.Size(1211, 797);
            this.Controls.SetChildIndex(this.kryptonPanel1, 0);
            this.Controls.SetChildIndex(this.RenderPanel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.RenderPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel1)).EndInit();
            this.kryptonPanel1.ResumeLayout(false);
            this.kryptonPanel1.PerformLayout();
            this.ZoomMenu.ResumeLayout(false);
            this.PanelBar.ResumeLayout(false);
            this.PanelBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonPanel kryptonPanel1;
        private ComponentFactory.Krypton.Toolkit.KryptonDropButton MenuZoomItems;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel LabelZoom;
        private System.Windows.Forms.ContextMenuStrip ZoomMenu;
        private System.Windows.Forms.ToolStripMenuItem ItemZoomToWindow;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem8;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem9;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem10;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem11;
        private System.Windows.Forms.TableLayoutPanel PanelBar;
    }
}
