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
                SuspendLayout();
                ContentClosedEvent = null;
                BubbleDragDropEvent = null;
                BubbleDragOverEvent = null;
                BubbleDragEnterEvent = null;
                _panelViews.Clear();
                UnassignEvents();
                Shutdown();
                ResumeLayout(false);
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
            this.PanelPresenter = new System.Windows.Forms.Panel();
            this.PanelContentName = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.LabelHeader = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ButtonClose = new System.Windows.Forms.Button();
            this.TipButton = new System.Windows.Forms.ToolTip(this.components);
            this.PanelHost = new System.Windows.Forms.Panel();
            this.PanelHostControls = new System.Windows.Forms.Panel();
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
            this.PanelPresenter.Size = new System.Drawing.Size(440, 447);
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
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.panel4.Controls.Add(this.LabelHeader);
            this.panel4.Controls.Add(this.panel1);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(6, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(594, 20);
            this.panel4.TabIndex = 1;
            // 
            // LabelHeader
            // 
            this.LabelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.LabelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.LabelHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelHeader.Location = new System.Drawing.Point(0, 0);
            this.LabelHeader.Name = "LabelHeader";
            this.LabelHeader.Size = new System.Drawing.Size(567, 20);
            this.LabelHeader.TabIndex = 0;
            this.LabelHeader.Text = "(Content)";
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.ButtonClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(567, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(27, 20);
            this.panel1.TabIndex = 0;
            // 
            // ButtonClose
            // 
            this.ButtonClose.AutoSize = true;
            this.ButtonClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ButtonClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonClose.FlatAppearance.BorderSize = 0;
            this.ButtonClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Maroon;
            this.ButtonClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
            this.ButtonClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonClose.Font = new System.Drawing.Font("Marlett", 9F);
            this.ButtonClose.Location = new System.Drawing.Point(0, 0);
            this.ButtonClose.Name = "ButtonClose";
            this.ButtonClose.Size = new System.Drawing.Size(27, 20);
            this.ButtonClose.TabIndex = 0;
            this.ButtonClose.TabStop = false;
            this.ButtonClose.Text = "r";
            this.TipButton.SetToolTip(this.ButtonClose, "Closes this content.");
            this.ButtonClose.UseVisualStyleBackColor = false;
            this.ButtonClose.Click += new System.EventHandler(this.ButtonClose_Click);
            // 
            // TipButton
            // 
            this.TipButton.BackColor = System.Drawing.Color.White;
            this.TipButton.ToolTipTitle = "Close";
            // 
            // PanelHost
            // 
            this.PanelHost.AutoSize = true;
            this.PanelHost.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PanelHost.Controls.Add(this.PanelHostControls);
            this.PanelHost.Dock = System.Windows.Forms.DockStyle.Right;
            this.PanelHost.Location = new System.Drawing.Point(440, 21);
            this.PanelHost.MinimumSize = new System.Drawing.Size(160, 0);
            this.PanelHost.Name = "PanelHost";
            this.PanelHost.Padding = new System.Windows.Forms.Padding(1, 0, 0, 1);
            this.PanelHost.Size = new System.Drawing.Size(160, 447);
            this.PanelHost.TabIndex = 1;
            this.PanelHost.Visible = false;
            // 
            // PanelHostControls
            // 
            this.PanelHostControls.AutoScroll = true;
            this.PanelHostControls.AutoSize = true;
            this.PanelHostControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PanelHostControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelHostControls.Location = new System.Drawing.Point(1, 0);
            this.PanelHostControls.Name = "PanelHostControls";
            this.PanelHostControls.Size = new System.Drawing.Size(159, 446);
            this.PanelHostControls.TabIndex = 0;
            // 
            // ContentBaseControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PanelPresenter);
            this.Controls.Add(this.PanelHost);
            this.Controls.Add(this.PanelContentName);
            this.Name = "ContentBaseControl";
            this.PanelContentName.ResumeLayout(false);
            this.PanelContentName.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.PanelHost.ResumeLayout(false);
            this.PanelHost.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        [System.Runtime.CompilerServices.AccessedThroughProperty(nameof(PresentationPanel))]
        private System.Windows.Forms.Panel PanelPresenter;
        private System.Windows.Forms.Panel PanelContentName;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label LabelHeader;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button ButtonClose;
        private System.Windows.Forms.ToolTip TipButton;
        [System.Runtime.CompilerServices.AccessedThroughProperty(nameof(HostPanel))]
        private System.Windows.Forms.Panel PanelHost;
        [System.Runtime.CompilerServices.AccessedThroughProperty(nameof(HostPanelControls))]
        private System.Windows.Forms.Panel PanelHostControls;
    }
}
