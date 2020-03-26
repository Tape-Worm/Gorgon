namespace Gorgon.Editor.UI.Controls
{
    partial class EditorSubPanelCommon
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
            this.PanelCaptionBorder = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.LabelCaption = new System.Windows.Forms.Label();
            this.PanelConfirmCancel = new System.Windows.Forms.Panel();
            this.ButtonOK = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.PanelBody = new System.Windows.Forms.Panel();
            this.PanelCaptionBorder.SuspendLayout();
            this.panel2.SuspendLayout();
            this.PanelConfirmCancel.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelCaptionBorder
            // 
            this.PanelCaptionBorder.AutoSize = true;
            this.PanelCaptionBorder.BackColor = System.Drawing.Color.SteelBlue;
            this.PanelCaptionBorder.Controls.Add(this.panel2);
            this.PanelCaptionBorder.Dock = System.Windows.Forms.DockStyle.Top;
            this.PanelCaptionBorder.Location = new System.Drawing.Point(0, 0);
            this.PanelCaptionBorder.Name = "PanelCaptionBorder";
            this.PanelCaptionBorder.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.PanelCaptionBorder.Size = new System.Drawing.Size(539, 21);
            this.PanelCaptionBorder.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.AutoSize = true;
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panel2.Controls.Add(this.LabelCaption);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(2, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(537, 21);
            this.panel2.TabIndex = 0;
            // 
            // LabelCaption
            // 
            this.LabelCaption.AutoSize = true;
            this.LabelCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelCaption.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.LabelCaption.Location = new System.Drawing.Point(0, 0);
            this.LabelCaption.Name = "LabelCaption";
            this.LabelCaption.Size = new System.Drawing.Size(0, 21);
            this.LabelCaption.TabIndex = 0;
            // 
            // PanelConfirmCancel
            // 
            this.PanelConfirmCancel.AutoSize = true;
            this.PanelConfirmCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PanelConfirmCancel.Controls.Add(this.ButtonOK);
            this.PanelConfirmCancel.Controls.Add(this.ButtonCancel);
            this.PanelConfirmCancel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PanelConfirmCancel.Location = new System.Drawing.Point(0, 457);
            this.PanelConfirmCancel.Name = "PanelConfirmCancel";
            this.PanelConfirmCancel.Size = new System.Drawing.Size(539, 36);
            this.PanelConfirmCancel.TabIndex = 9999;
            // 
            // ButtonOK
            // 
            this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOK.AutoSize = true;
            this.ButtonOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOK.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ButtonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonOK.Location = new System.Drawing.Point(364, 3);
            this.ButtonOK.MinimumSize = new System.Drawing.Size(83, 30);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(83, 30);
            this.ButtonOK.TabIndex = 0;
            this.ButtonOK.Text = "OK";
            this.ButtonOK.UseVisualStyleBackColor = false;
            this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.AutoSize = true;
            this.ButtonCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ButtonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonCancel.Location = new System.Drawing.Point(453, 3);
            this.ButtonCancel.MinimumSize = new System.Drawing.Size(83, 30);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(83, 30);
            this.ButtonCancel.TabIndex = 1;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = false;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // PanelBody
            // 
            this.PanelBody.AutoSize = true;
            this.PanelBody.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.PanelBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelBody.Location = new System.Drawing.Point(0, 21);
            this.PanelBody.Name = "PanelBody";
            this.PanelBody.Size = new System.Drawing.Size(539, 436);
            this.PanelBody.TabIndex = 1;
            // 
            // EditorSubPanelCommon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PanelBody);
            this.Controls.Add(this.PanelConfirmCancel);
            this.Controls.Add(this.PanelCaptionBorder);
            this.Name = "EditorSubPanelCommon";
            this.Size = new System.Drawing.Size(539, 493);
            this.PanelCaptionBorder.ResumeLayout(false);
            this.PanelCaptionBorder.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.PanelConfirmCancel.ResumeLayout(false);
            this.PanelConfirmCancel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel PanelCaptionBorder;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label LabelCaption;
        private System.Windows.Forms.Panel PanelConfirmCancel;
        private System.Windows.Forms.Button ButtonOK;
        private System.Windows.Forms.Button ButtonCancel;
    }
}
