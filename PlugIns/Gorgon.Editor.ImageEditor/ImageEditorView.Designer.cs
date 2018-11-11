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
            this.components = new System.ComponentModel.Container();
            this.kryptonPanel1 = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            this.PanelBar = new System.Windows.Forms.TableLayoutPanel();
            this.MenuZoomItems = new ComponentFactory.Krypton.Toolkit.KryptonDropButton();
            this.MenuZoom = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ItemZoomToWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.Item12Percent = new System.Windows.Forms.ToolStripMenuItem();
            this.Item25Percent = new System.Windows.Forms.ToolStripMenuItem();
            this.Item50Percent = new System.Windows.Forms.ToolStripMenuItem();
            this.Item100Percent = new System.Windows.Forms.ToolStripMenuItem();
            this.Item200Percent = new System.Windows.Forms.ToolStripMenuItem();
            this.Item400Percent = new System.Windows.Forms.ToolStripMenuItem();
            this.Item800Percent = new System.Windows.Forms.ToolStripMenuItem();
            this.Item1600Percent = new System.Windows.Forms.ToolStripMenuItem();
            this.LabelZoom = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.TableViewControls = new System.Windows.Forms.TableLayoutPanel();
            this.ScrollHorizontal = new System.Windows.Forms.HScrollBar();
            this.ScrollVertical = new System.Windows.Forms.VScrollBar();
            this.PanelImage = new System.Windows.Forms.Panel();
            this.ButtonCenter = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)(this.PresentationPanel)).BeginInit();
            this.PresentationPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel1)).BeginInit();
            this.kryptonPanel1.SuspendLayout();
            this.PanelBar.SuspendLayout();
            this.MenuZoom.SuspendLayout();
            this.TableViewControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // PresentationPanel
            // 
            this.PresentationPanel.Controls.Add(this.TableViewControls);
            this.PresentationPanel.Size = new System.Drawing.Size(1211, 761);
            // 
            // kryptonPanel1
            // 
            this.kryptonPanel1.AutoSize = true;
            this.kryptonPanel1.Controls.Add(this.PanelBar);
            this.kryptonPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.kryptonPanel1.Location = new System.Drawing.Point(0, 761);
            this.kryptonPanel1.Name = "kryptonPanel1";
            this.kryptonPanel1.Size = new System.Drawing.Size(1211, 36);
            this.kryptonPanel1.TabIndex = 1;
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
            this.PanelBar.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.PanelBar.RowCount = 1;
            this.PanelBar.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelBar.Size = new System.Drawing.Size(1211, 36);
            this.PanelBar.TabIndex = 2;
            // 
            // MenuZoomItems
            // 
            this.MenuZoomItems.AutoSize = true;
            this.MenuZoomItems.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MenuZoomItems.ContextMenuStrip = this.MenuZoom;
            this.MenuZoomItems.DropDownOrientation = ComponentFactory.Krypton.Toolkit.VisualOrientation.Top;
            this.MenuZoomItems.Location = new System.Drawing.Point(54, 6);
            this.MenuZoomItems.Name = "MenuZoomItems";
            this.MenuZoomItems.Size = new System.Drawing.Size(85, 24);
            this.MenuZoomItems.Splitter = false;
            this.MenuZoomItems.TabIndex = 1;
            this.MenuZoomItems.Values.Text = "To Window";
            // 
            // MenuZoom
            // 
            this.MenuZoom.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MenuZoom.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ItemZoomToWindow,
            this.toolStripMenuItem1,
            this.Item12Percent,
            this.Item25Percent,
            this.Item50Percent,
            this.Item100Percent,
            this.Item200Percent,
            this.Item400Percent,
            this.Item800Percent,
            this.Item1600Percent});
            this.MenuZoom.Name = "ZoomMenu";
            this.MenuZoom.Size = new System.Drawing.Size(135, 208);
            // 
            // ItemZoomToWindow
            // 
            this.ItemZoomToWindow.Checked = true;
            this.ItemZoomToWindow.CheckOnClick = true;
            this.ItemZoomToWindow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ItemZoomToWindow.Name = "ItemZoomToWindow";
            this.ItemZoomToWindow.Size = new System.Drawing.Size(134, 22);
            this.ItemZoomToWindow.Tag = "ToWindow";
            this.ItemZoomToWindow.Text = "To Window";
            this.ItemZoomToWindow.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(131, 6);
            // 
            // Item12Percent
            // 
            this.Item12Percent.CheckOnClick = true;
            this.Item12Percent.Name = "Item12Percent";
            this.Item12Percent.Size = new System.Drawing.Size(134, 22);
            this.Item12Percent.Tag = "Percent12";
            this.Item12Percent.Text = "12.5%";
            this.Item12Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item25Percent
            // 
            this.Item25Percent.CheckOnClick = true;
            this.Item25Percent.Name = "Item25Percent";
            this.Item25Percent.Size = new System.Drawing.Size(134, 22);
            this.Item25Percent.Tag = "Percent25";
            this.Item25Percent.Text = "25%";
            this.Item25Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item50Percent
            // 
            this.Item50Percent.CheckOnClick = true;
            this.Item50Percent.Name = "Item50Percent";
            this.Item50Percent.Size = new System.Drawing.Size(134, 22);
            this.Item50Percent.Tag = "Percent50";
            this.Item50Percent.Text = "50%";
            this.Item50Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item100Percent
            // 
            this.Item100Percent.CheckOnClick = true;
            this.Item100Percent.Name = "Item100Percent";
            this.Item100Percent.Size = new System.Drawing.Size(134, 22);
            this.Item100Percent.Tag = "Percent100";
            this.Item100Percent.Text = "100%";
            this.Item100Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item200Percent
            // 
            this.Item200Percent.CheckOnClick = true;
            this.Item200Percent.Name = "Item200Percent";
            this.Item200Percent.Size = new System.Drawing.Size(134, 22);
            this.Item200Percent.Tag = "Percent200";
            this.Item200Percent.Text = "200%";
            this.Item200Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item400Percent
            // 
            this.Item400Percent.CheckOnClick = true;
            this.Item400Percent.Name = "Item400Percent";
            this.Item400Percent.Size = new System.Drawing.Size(134, 22);
            this.Item400Percent.Tag = "Percent400";
            this.Item400Percent.Text = "400%";
            this.Item400Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item800Percent
            // 
            this.Item800Percent.CheckOnClick = true;
            this.Item800Percent.Name = "Item800Percent";
            this.Item800Percent.Size = new System.Drawing.Size(134, 22);
            this.Item800Percent.Tag = "Percent800";
            this.Item800Percent.Text = "800%";
            this.Item800Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item1600Percent
            // 
            this.Item1600Percent.CheckOnClick = true;
            this.Item1600Percent.Name = "Item1600Percent";
            this.Item1600Percent.Size = new System.Drawing.Size(134, 22);
            this.Item1600Percent.Tag = "Percent1600";
            this.Item1600Percent.Text = "1600%";
            this.Item1600Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // LabelZoom
            // 
            this.LabelZoom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.LabelZoom.Location = new System.Drawing.Point(3, 6);
            this.LabelZoom.Name = "LabelZoom";
            this.LabelZoom.Size = new System.Drawing.Size(45, 24);
            this.LabelZoom.TabIndex = 0;
            this.LabelZoom.Values.Text = "Zoom:";
            // 
            // TableViewControls
            // 
            this.TableViewControls.ColumnCount = 2;
            this.TableViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableViewControls.Controls.Add(this.ScrollHorizontal, 0, 1);
            this.TableViewControls.Controls.Add(this.ScrollVertical, 1, 0);
            this.TableViewControls.Controls.Add(this.PanelImage, 0, 0);
            this.TableViewControls.Controls.Add(this.ButtonCenter, 1, 1);
            this.TableViewControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableViewControls.Location = new System.Drawing.Point(0, 0);
            this.TableViewControls.Name = "TableViewControls";
            this.TableViewControls.RowCount = 2;
            this.TableViewControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableViewControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableViewControls.Size = new System.Drawing.Size(1211, 761);
            this.TableViewControls.TabIndex = 0;
            // 
            // ScrollHorizontal
            // 
            this.ScrollHorizontal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ScrollHorizontal.Enabled = false;
            this.ScrollHorizontal.Location = new System.Drawing.Point(0, 739);
            this.ScrollHorizontal.Maximum = 110;
            this.ScrollHorizontal.Minimum = -100;
            this.ScrollHorizontal.Name = "ScrollHorizontal";
            this.ScrollHorizontal.Size = new System.Drawing.Size(1189, 22);
            this.ScrollHorizontal.TabIndex = 0;
            this.ScrollHorizontal.ValueChanged += new System.EventHandler(this.ScrollVertical_ValueChanged);
            // 
            // ScrollVertical
            // 
            this.ScrollVertical.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ScrollVertical.Enabled = false;
            this.ScrollVertical.Location = new System.Drawing.Point(1189, 0);
            this.ScrollVertical.Maximum = 110;
            this.ScrollVertical.Minimum = -100;
            this.ScrollVertical.Name = "ScrollVertical";
            this.ScrollVertical.Size = new System.Drawing.Size(22, 739);
            this.ScrollVertical.TabIndex = 1;
            this.ScrollVertical.ValueChanged += new System.EventHandler(this.ScrollVertical_ValueChanged);
            // 
            // PanelImage
            // 
            this.PanelImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelImage.Location = new System.Drawing.Point(3, 3);
            this.PanelImage.Name = "PanelImage";
            this.PanelImage.Size = new System.Drawing.Size(1183, 733);
            this.PanelImage.TabIndex = 2;
            // 
            // ButtonCenter
            // 
            this.ButtonCenter.AutoSize = true;
            this.ButtonCenter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonCenter.Location = new System.Drawing.Point(1189, 739);
            this.ButtonCenter.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonCenter.Name = "ButtonCenter";
            this.ButtonCenter.Size = new System.Drawing.Size(22, 22);
            this.ButtonCenter.TabIndex = 3;
            this.ButtonCenter.Values.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.center_16x16;
            this.ButtonCenter.Values.Text = "";
            // 
            // ImageEditorView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.kryptonPanel1);
            this.Name = "ImageEditorView";
            this.RenderControl = this.PanelImage;
            this.Size = new System.Drawing.Size(1211, 797);
            this.Controls.SetChildIndex(this.kryptonPanel1, 0);
            this.Controls.SetChildIndex(this.PresentationPanel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.PresentationPanel)).EndInit();
            this.PresentationPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel1)).EndInit();
            this.kryptonPanel1.ResumeLayout(false);
            this.kryptonPanel1.PerformLayout();
            this.PanelBar.ResumeLayout(false);
            this.PanelBar.PerformLayout();
            this.MenuZoom.ResumeLayout(false);
            this.TableViewControls.ResumeLayout(false);
            this.TableViewControls.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonPanel kryptonPanel1;
        private ComponentFactory.Krypton.Toolkit.KryptonDropButton MenuZoomItems;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel LabelZoom;
        private System.Windows.Forms.ContextMenuStrip MenuZoom;
        private System.Windows.Forms.ToolStripMenuItem ItemZoomToWindow;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem Item12Percent;
        private System.Windows.Forms.ToolStripMenuItem Item25Percent;
        private System.Windows.Forms.ToolStripMenuItem Item50Percent;
        private System.Windows.Forms.ToolStripMenuItem Item100Percent;
        private System.Windows.Forms.ToolStripMenuItem Item200Percent;
        private System.Windows.Forms.ToolStripMenuItem Item400Percent;
        private System.Windows.Forms.ToolStripMenuItem Item800Percent;
        private System.Windows.Forms.ToolStripMenuItem Item1600Percent;
        private System.Windows.Forms.TableLayoutPanel PanelBar;
        private System.Windows.Forms.TableLayoutPanel TableViewControls;
        private System.Windows.Forms.HScrollBar ScrollHorizontal;
        private System.Windows.Forms.VScrollBar ScrollVertical;
        private System.Windows.Forms.Panel PanelImage;
        private ComponentFactory.Krypton.Toolkit.KryptonButton ButtonCenter;
    }
}
