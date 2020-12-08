namespace Gorgon.UI
{
    partial class GorgonProgressPanel
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
                OperationCancelledEvent = null;
                ButtonCancel.Click -= ButtonCancel_Click;
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
            this.ProgressMeter = new System.Windows.Forms.ProgressBar();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.LabelTitle = new System.Windows.Forms.Label();
            this.LabelProgressMessage = new System.Windows.Forms.Label();
            this.PanelLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.PanelLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // ProgressMeter
            // 
            this.ProgressMeter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.ProgressMeter.ForeColor = System.Drawing.Color.Lime;
            this.ProgressMeter.Location = new System.Drawing.Point(3, 67);
            this.ProgressMeter.MarqueeAnimationSpeed = 15;
            this.ProgressMeter.MaximumSize = new System.Drawing.Size(500, 23);
            this.ProgressMeter.MinimumSize = new System.Drawing.Size(500, 23);
            this.ProgressMeter.Name = "ProgressMeter";
            this.ProgressMeter.Size = new System.Drawing.Size(500, 23);
            this.ProgressMeter.Step = 1;
            this.ProgressMeter.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.ProgressMeter.TabIndex = 2;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.AutoSize = true;
            this.ButtonCancel.ForeColor = System.Drawing.Color.Black;
            this.ButtonCancel.Location = new System.Drawing.Point(414, 96);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(89, 25);
            this.ButtonCancel.TabIndex = 3;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Visible = false;
            // 
            // LabelTitle
            // 
            this.LabelTitle.AutoEllipsis = true;
            this.LabelTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.LabelTitle.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.LabelTitle.ForeColor = System.Drawing.Color.DimGray;
            this.LabelTitle.Location = new System.Drawing.Point(0, 0);
            this.LabelTitle.Margin = new System.Windows.Forms.Padding(0);
            this.LabelTitle.MaximumSize = new System.Drawing.Size(506, 64);
            this.LabelTitle.MinimumSize = new System.Drawing.Size(506, 32);
            this.LabelTitle.Name = "LabelTitle";
            this.LabelTitle.Size = new System.Drawing.Size(506, 32);
            this.LabelTitle.TabIndex = 0;
            this.LabelTitle.Text = "Title";
            this.LabelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LabelProgressMessage
            // 
            this.LabelProgressMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelProgressMessage.AutoEllipsis = true;
            this.LabelProgressMessage.ForeColor = System.Drawing.Color.Black;
            this.LabelProgressMessage.Location = new System.Drawing.Point(3, 32);
            this.LabelProgressMessage.MaximumSize = new System.Drawing.Size(500, 32);
            this.LabelProgressMessage.MinimumSize = new System.Drawing.Size(500, 32);
            this.LabelProgressMessage.Name = "LabelProgressMessage";
            this.LabelProgressMessage.Size = new System.Drawing.Size(500, 32);
            this.LabelProgressMessage.TabIndex = 1;
            this.LabelProgressMessage.Text = "Progress";
            this.LabelProgressMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PanelLayout
            // 
            this.PanelLayout.AutoSize = true;
            this.PanelLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PanelLayout.Controls.Add(this.LabelTitle);
            this.PanelLayout.Controls.Add(this.LabelProgressMessage);
            this.PanelLayout.Controls.Add(this.ProgressMeter);
            this.PanelLayout.Controls.Add(this.ButtonCancel);
            this.PanelLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.PanelLayout.Location = new System.Drawing.Point(0, 0);
            this.PanelLayout.Name = "PanelLayout";
            this.PanelLayout.Size = new System.Drawing.Size(506, 124);
            this.PanelLayout.TabIndex = 4;
            // 
            // GorgonProgressPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.PanelLayout);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Black;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MaximumSize = new System.Drawing.Size(512, 128);
            this.Name = "GorgonProgressPanel";
            this.Size = new System.Drawing.Size(511, 127);
            this.PanelLayout.ResumeLayout(false);
            this.PanelLayout.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.FlowLayoutPanel PanelLayout;
        private System.Windows.Forms.ProgressBar ProgressMeter;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Label LabelTitle;
        private System.Windows.Forms.Label LabelProgressMessage;
    }
}
