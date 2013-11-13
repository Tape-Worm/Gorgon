namespace GorgonLibrary.Editor.FontEditorPlugIn
{
    partial class formBrushEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formBrushEditor));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.tabDocumentManager = new KRBTabControl.KRBTabControl();
            this.pageGradient = new KRBTabControl.TabPageEx();
            this.pageTexture = new KRBTabControl.TabPageEx();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.radioNone = new System.Windows.Forms.RadioButton();
            this.radioGradient = new System.Windows.Forms.RadioButton();
            this.radioTexture = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.tabDocumentManager.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonCancel.ForeColor = System.Drawing.Color.White;
            this.buttonCancel.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.cancel_16x16;
            this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonCancel.Location = new System.Drawing.Point(513, 9);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(87, 28);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonCancel.UseVisualStyleBackColor = false;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Enabled = false;
            this.buttonOK.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonOK.ForeColor = System.Drawing.Color.White;
            this.buttonOK.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.ok_16x16;
            this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonOK.Location = new System.Drawing.Point(420, 9);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(87, 28);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonOK.UseVisualStyleBackColor = false;
            // 
            // tabDocumentManager
            // 
            this.tabDocumentManager.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabDocumentManager.Alignments = KRBTabControl.KRBTabControl.TabAlignments.Bottom;
            this.tabDocumentManager.AllowDrop = true;
            this.tabDocumentManager.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.tabDocumentManager.BackgroundHatcher.HatchType = System.Drawing.Drawing2D.HatchStyle.DashedVertical;
            this.tabDocumentManager.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.tabDocumentManager.Controls.Add(this.pageGradient);
            this.tabDocumentManager.Controls.Add(this.pageTexture);
            this.tabDocumentManager.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabDocumentManager.HeaderVisibility = true;
            this.tabDocumentManager.IsCaptionVisible = false;
            this.tabDocumentManager.IsDocumentTabStyle = true;
            this.tabDocumentManager.IsDrawHeader = false;
            this.tabDocumentManager.IsUserInteraction = false;
            this.tabDocumentManager.ItemSize = new System.Drawing.Size(0, 28);
            this.tabDocumentManager.Location = new System.Drawing.Point(113, 24);
            this.tabDocumentManager.Multiline = true;
            this.tabDocumentManager.Name = "tabDocumentManager";
            this.tabDocumentManager.SelectedIndex = 0;
            this.tabDocumentManager.Size = new System.Drawing.Size(499, 371);
            this.tabDocumentManager.TabBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
            this.tabDocumentManager.TabGradient.ColorEnd = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
            this.tabDocumentManager.TabGradient.ColorStart = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
            this.tabDocumentManager.TabGradient.TabPageSelectedTextColor = System.Drawing.Color.White;
            this.tabDocumentManager.TabGradient.TabPageTextColor = System.Drawing.Color.White;
            this.tabDocumentManager.TabHOffset = -1;
            this.tabDocumentManager.TabIndex = 0;
            this.tabDocumentManager.UpDownStyle = KRBTabControl.KRBTabControl.UpDown32Style.BlackGlass;
            // 
            // pageGradient
            // 
            this.pageGradient.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.pageGradient.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pageGradient.ForeColor = System.Drawing.Color.White;
            this.pageGradient.IsClosable = false;
            this.pageGradient.Location = new System.Drawing.Point(1, 1);
            this.pageGradient.Name = "pageGradient";
            this.pageGradient.Size = new System.Drawing.Size(497, 369);
            this.pageGradient.TabIndex = 1;
            this.pageGradient.Text = "Gradient";
            // 
            // pageTexture
            // 
            this.pageTexture.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.pageTexture.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pageTexture.IsClosable = false;
            this.pageTexture.Location = new System.Drawing.Point(1, 1);
            this.pageTexture.Name = "pageTexture";
            this.pageTexture.Size = new System.Drawing.Size(610, 369);
            this.pageTexture.TabIndex = 0;
            this.pageTexture.Text = "Texture";
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.buttonOK);
            this.panel1.Controls.Add(this.buttonCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 395);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.panel1.Size = new System.Drawing.Size(612, 44);
            this.panel1.TabIndex = 5;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 24);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(113, 371);
            this.panel2.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Brush Type:";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.radioButton1);
            this.panel3.Controls.Add(this.radioTexture);
            this.panel3.Controls.Add(this.radioGradient);
            this.panel3.Controls.Add(this.radioNone);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 15);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(113, 356);
            this.panel3.TabIndex = 1;
            // 
            // radioNone
            // 
            this.radioNone.AutoSize = true;
            this.radioNone.Checked = true;
            this.radioNone.Location = new System.Drawing.Point(3, 3);
            this.radioNone.Name = "radioNone";
            this.radioNone.Size = new System.Drawing.Size(74, 19);
            this.radioNone.TabIndex = 0;
            this.radioNone.Text = "No brush";
            this.radioNone.UseVisualStyleBackColor = true;
            // 
            // radioGradient
            // 
            this.radioGradient.AutoSize = true;
            this.radioGradient.Location = new System.Drawing.Point(3, 28);
            this.radioGradient.Name = "radioGradient";
            this.radioGradient.Size = new System.Drawing.Size(103, 19);
            this.radioGradient.TabIndex = 1;
            this.radioGradient.Text = "Gradient brush";
            this.radioGradient.UseVisualStyleBackColor = true;
            // 
            // radioTexture
            // 
            this.radioTexture.AutoSize = true;
            this.radioTexture.Location = new System.Drawing.Point(3, 53);
            this.radioTexture.Name = "radioTexture";
            this.radioTexture.Size = new System.Drawing.Size(97, 19);
            this.radioTexture.TabIndex = 2;
            this.radioTexture.Text = "Texture brush";
            this.radioTexture.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(3, 78);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(96, 19);
            this.radioButton1.TabIndex = 3;
            this.radioButton1.Text = "Pattern brush";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // formBrushEditor
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.BorderColor = System.Drawing.Color.SteelBlue;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(612, 439);
            this.Controls.Add(this.tabDocumentManager);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.Silver;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formBrushEditor";
            this.Resizable = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Brush Editor";
            this.Controls.SetChildIndex(this.panel1, 0);
            this.Controls.SetChildIndex(this.panel2, 0);
            this.Controls.SetChildIndex(this.tabDocumentManager, 0);
            this.tabDocumentManager.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private KRBTabControl.KRBTabControl tabDocumentManager;
        private KRBTabControl.TabPageEx pageGradient;
        private KRBTabControl.TabPageEx pageTexture;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioTexture;
        private System.Windows.Forms.RadioButton radioGradient;
        private System.Windows.Forms.RadioButton radioNone;
        private System.Windows.Forms.Label label1;
    }
}