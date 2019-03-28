namespace Gorgon.Editor.UI.Views
{
    partial class ContentBaseControl
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
                _panelViews.Clear();
                UnassignEvents();
                Shutdown();
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
            this.PanelPresenter = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            this.PanelContentName = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.LabelHeader = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ButtonClose = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.TipButton = new System.Windows.Forms.ToolTip(this.components);
            this.PanelHost = new System.Windows.Forms.Panel();
            this.PanelHostControls = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.PanelPresenter)).BeginInit();
            this.PanelContentName.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel1.SuspendLayout();
            this.PanelHost.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelPresenter
            // 
            this.PanelPresenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelPresenter.Location = new System.Drawing.Point(0, 21);
            this.PanelPresenter.Name = "PanelPresenter";
            this.PanelPresenter.Size = new System.Drawing.Size(600, 447);
            this.PanelPresenter.TabIndex = 0;
            // 
            // PanelContentName
            // 
            this.PanelContentName.AutoSize = true;
            this.PanelContentName.BackColor = System.Drawing.Color.SteelBlue;
            this.PanelContentName.Controls.Add(this.panel4);
            this.PanelContentName.Dock = System.Windows.Forms.DockStyle.Top;
            this.PanelContentName.Location = new System.Drawing.Point(0, 0);
            this.PanelContentName.Name = "PanelContentName";
            this.PanelContentName.Padding = new System.Windows.Forms.Padding(6, 0, 0, 1);
            this.PanelContentName.Size = new System.Drawing.Size(600, 21);
            this.PanelContentName.TabIndex = 7;
            this.PanelContentName.Visible = false;
            // 
            // panel4
            // 
            this.panel4.AutoSize = true;
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(104)))), ((int)(((byte)(104)))), ((int)(((byte)(104)))));
            this.panel4.Controls.Add(this.LabelHeader);
            this.panel4.Controls.Add(this.panel1);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.panel4.Location = new System.Drawing.Point(6, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(594, 20);
            this.panel4.TabIndex = 1;
            // 
            // LabelHeader
            // 
            this.LabelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.LabelHeader.LabelStyle = ComponentFactory.Krypton.Toolkit.LabelStyle.Custom3;
            this.LabelHeader.Location = new System.Drawing.Point(0, 0);
            this.LabelHeader.Name = "LabelHeader";
            this.LabelHeader.Size = new System.Drawing.Size(573, 20);
            this.LabelHeader.StateCommon.ShortText.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.LabelHeader.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelHeader.TabIndex = 0;
            this.LabelHeader.Values.Text = "(Content)";
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.ButtonClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(573, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(21, 20);
            this.panel1.TabIndex = 0;
            // 
            // ButtonClose
            // 
            this.ButtonClose.AutoSize = true;
            this.ButtonClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonClose.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.FormClose;
            this.ButtonClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonClose.Location = new System.Drawing.Point(0, 0);
            this.ButtonClose.Name = "ButtonClose";
            this.ButtonClose.Size = new System.Drawing.Size(21, 20);
            this.ButtonClose.StateCommon.Back.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.None;
            this.ButtonClose.StateCommon.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.ButtonClose.StateCommon.Border.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.None;
            this.ButtonClose.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Marlett", 9F);
            this.ButtonClose.StateCommon.Content.ShortText.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.ButtonClose.StateCommon.Content.ShortText.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.ButtonClose.StateNormal.Content.ShortText.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ButtonClose.TabIndex = 0;
            this.ButtonClose.TabStop = false;
            this.TipButton.SetToolTip(this.ButtonClose, "Closes this content.");
            this.ButtonClose.Values.Text = "r";
            this.ButtonClose.Click += new System.EventHandler(this.ButtonClose_Click);
            // 
            // TipButton
            // 
            this.TipButton.BackColor = System.Drawing.Color.White;
            this.TipButton.ToolTipTitle = "Close";
            // 
            // PanelHost
            // 
            this.PanelHost.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.PanelHost.Controls.Add(this.PanelHostControls);
            this.PanelHost.Dock = System.Windows.Forms.DockStyle.Right;
            this.PanelHost.ForeColor = System.Drawing.Color.White;
            this.PanelHost.Location = new System.Drawing.Point(240, 21);
            this.PanelHost.MinimumSize = new System.Drawing.Size(160, 0);
            this.PanelHost.Name = "PanelHost";
            this.PanelHost.Padding = new System.Windows.Forms.Padding(1, 0, 0, 1);
            this.PanelHost.Size = new System.Drawing.Size(360, 447);
            this.PanelHost.TabIndex = 1;
            this.PanelHost.Visible = false;
            // 
            // PanelHostControls
            // 
            this.PanelHostControls.AutoScroll = true;
            this.PanelHostControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.PanelHostControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelHostControls.Location = new System.Drawing.Point(1, 0);
            this.PanelHostControls.Name = "PanelHostControls";
            this.PanelHostControls.Size = new System.Drawing.Size(359, 446);
            this.PanelHostControls.TabIndex = 0;
            // 
            // ContentBaseControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PanelHost);
            this.Controls.Add(this.PanelPresenter);
            this.Controls.Add(this.PanelContentName);
            this.Name = "ContentBaseControl";
            ((System.ComponentModel.ISupportInitialize)(this.PanelPresenter)).EndInit();
            this.PanelContentName.ResumeLayout(false);
            this.PanelContentName.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.PanelHost.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        [System.Runtime.CompilerServices.AccessedThroughProperty("PresentationPanel")]
        private ComponentFactory.Krypton.Toolkit.KryptonPanel PanelPresenter;
        private System.Windows.Forms.Panel PanelContentName;
        private System.Windows.Forms.Panel panel4;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel LabelHeader;
        private System.Windows.Forms.Panel panel1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton ButtonClose;
        private System.Windows.Forms.ToolTip TipButton;
        private System.Windows.Forms.Panel PanelHost;
        private System.Windows.Forms.Panel PanelHostControls;
    }
}
