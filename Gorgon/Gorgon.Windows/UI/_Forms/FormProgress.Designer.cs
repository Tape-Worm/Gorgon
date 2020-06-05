namespace Gorgon.UI
{
    partial class FormProgress
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProgress));
            this.Progress = new Gorgon.UI.GorgonProgressPanel();
            this.SuspendLayout();
            // 
            // Progress
            // 
            this.Progress.AllowCancellation = true;
            this.Progress.AutoSize = true;
            this.Progress.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Progress.BackColor = System.Drawing.Color.White;
            this.Progress.CurrentValue = 0F;
            this.Progress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Progress.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Progress.ForeColor = System.Drawing.Color.Black;
            this.Progress.Location = new System.Drawing.Point(1, 1);
            this.Progress.Margin = new System.Windows.Forms.Padding(0);
            this.Progress.MaximumSize = new System.Drawing.Size(512, 128);
            this.Progress.Name = "Progress";
            this.Progress.ProgressMessage = "Progress";
            this.Progress.ProgressMessageFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Progress.ProgressTitle = "Title";
            this.Progress.Size = new System.Drawing.Size(507, 126);
            this.Progress.TabIndex = 0;
            this.Progress.Resize += new System.EventHandler(this.Progress_Resize);
            // 
            // FormProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(509, 128);
            this.ControlBox = false;
            this.Controls.Add(this.Progress);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormProgress";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FormProgress";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        /// <summary>
        /// The progress panel embedded in the form.
        /// </summary>
        public GorgonProgressPanel Progress;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}