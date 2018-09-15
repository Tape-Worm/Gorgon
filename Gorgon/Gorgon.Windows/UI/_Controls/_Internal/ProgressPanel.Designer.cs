namespace Gorgon.UI
{
    partial class ProgressPanel
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
            this.ProgressMeter = new System.Windows.Forms.ProgressBar();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.LabelTitle = new System.Windows.Forms.Label();
            this.LabelProgressMessage = new System.Windows.Forms.Label();
            this.TableProgress = new System.Windows.Forms.TableLayoutPanel();
            this.TableProgress.SuspendLayout();
            this.SuspendLayout();
            // 
            // ProgressMeter
            // 
            this.ProgressMeter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.ProgressMeter.ForeColor = System.Drawing.Color.Lime;
            this.ProgressMeter.Location = new System.Drawing.Point(3, 49);
            this.ProgressMeter.MarqueeAnimationSpeed = 33;
            this.ProgressMeter.MaximumSize = new System.Drawing.Size(500, 23);
            this.ProgressMeter.Name = "ProgressMeter";
            this.ProgressMeter.Size = new System.Drawing.Size(474, 23);
            this.ProgressMeter.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.ProgressMeter.TabIndex = 2;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.AutoSize = true;
            this.ButtonCancel.ForeColor = System.Drawing.Color.Black;
            this.ButtonCancel.Location = new System.Drawing.Point(395, 78);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(82, 25);
            this.ButtonCancel.TabIndex = 3;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Visible = false;
            // 
            // LabelTitle
            // 
            this.LabelTitle.AutoEllipsis = true;
            this.LabelTitle.AutoSize = true;
            this.LabelTitle.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.LabelTitle.ForeColor = System.Drawing.Color.DimGray;
            this.LabelTitle.Location = new System.Drawing.Point(0, 0);
            this.LabelTitle.Margin = new System.Windows.Forms.Padding(0);
            this.LabelTitle.MaximumSize = new System.Drawing.Size(506, 64);
            this.LabelTitle.Name = "LabelTitle";
            this.LabelTitle.Size = new System.Drawing.Size(48, 25);
            this.LabelTitle.TabIndex = 0;
            this.LabelTitle.Text = "Title";
            // 
            // LabelProgressMessage
            // 
            this.LabelProgressMessage.AutoEllipsis = true;
            this.LabelProgressMessage.AutoSize = true;
            this.LabelProgressMessage.ForeColor = System.Drawing.Color.Black;
            this.LabelProgressMessage.Location = new System.Drawing.Point(3, 28);
            this.LabelProgressMessage.Margin = new System.Windows.Forms.Padding(3);
            this.LabelProgressMessage.MaximumSize = new System.Drawing.Size(506, 120);
            this.LabelProgressMessage.Name = "LabelProgressMessage";
            this.LabelProgressMessage.Size = new System.Drawing.Size(52, 15);
            this.LabelProgressMessage.TabIndex = 1;
            this.LabelProgressMessage.Text = "Progress";
            this.LabelProgressMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TableProgress
            // 
            this.TableProgress.AutoSize = true;
            this.TableProgress.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TableProgress.ColumnCount = 1;
            this.TableProgress.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableProgress.Controls.Add(this.LabelTitle, 0, 0);
            this.TableProgress.Controls.Add(this.ButtonCancel, 0, 3);
            this.TableProgress.Controls.Add(this.LabelProgressMessage, 0, 1);
            this.TableProgress.Controls.Add(this.ProgressMeter, 0, 2);
            this.TableProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableProgress.Location = new System.Drawing.Point(0, 0);
            this.TableProgress.Name = "TableProgress";
            this.TableProgress.RowCount = 4;
            this.TableProgress.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableProgress.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableProgress.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableProgress.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableProgress.Size = new System.Drawing.Size(480, 106);
            this.TableProgress.TabIndex = 0;
            // 
            // ProgressPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.TableProgress);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Black;
            this.MaximumSize = new System.Drawing.Size(512, 200);
            this.MinimumSize = new System.Drawing.Size(320, 32);
            this.Name = "ProgressPanel";
            this.Size = new System.Drawing.Size(480, 106);
            this.TableProgress.ResumeLayout(false);
            this.TableProgress.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.ProgressBar ProgressMeter;
        internal System.Windows.Forms.Button ButtonCancel;
        internal System.Windows.Forms.Label LabelTitle;
        internal System.Windows.Forms.Label LabelProgressMessage;
        internal System.Windows.Forms.TableLayoutPanel TableProgress;
    }
}
