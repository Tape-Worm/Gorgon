namespace GorgonLibrary.Editor.FontEditorPlugIn.Controls
{
	partial class panelHatch
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
		        if (_previewBitmap != null)
		        {
		            _previewBitmap.Dispose();
		            _previewBitmap = null;
		        }

		        if (_foreColorBitmap != null)
		        {
		            _foreColorBitmap.Dispose();
		            _foreColorBitmap = null;
		        }

		        if (_backColorBitmap != null)
		        {
		            _backColorBitmap.Dispose();
		            _backColorBitmap = null;
		        }
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
            this.panelPreview = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panelForegroundColor = new GorgonLibrary.UI.GorgonSelectablePanel();
            this.panelBackgroundColor = new GorgonLibrary.UI.GorgonSelectablePanel();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.comboHatch = new GorgonLibrary.Editor.FontEditorPlugIn.ComboPatterns();
            this.panel1.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelPreview
            // 
            this.panelPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPreview.BackgroundImage = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.Pattern;
            this.panelPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPreview.Location = new System.Drawing.Point(6, 23);
            this.panelPreview.Name = "panelPreview";
            this.panelPreview.Size = new System.Drawing.Size(196, 179);
            this.panelPreview.TabIndex = 0;
            this.panelPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.panelPreview_Paint);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Pattern type:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Foreground:";
            // 
            // panelForegroundColor
            // 
            this.panelForegroundColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelForegroundColor.BackgroundImage = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.Pattern;
            this.panelForegroundColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelForegroundColor.Location = new System.Drawing.Point(78, 30);
            this.panelForegroundColor.Name = "panelForegroundColor";
            this.panelForegroundColor.Size = new System.Drawing.Size(211, 21);
            this.panelForegroundColor.TabIndex = 1;
            this.panelForegroundColor.Paint += new System.Windows.Forms.PaintEventHandler(this.panelForegroundColor_Paint);
            // 
            // panelBackgroundColor
            // 
            this.panelBackgroundColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBackgroundColor.BackgroundImage = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.Pattern;
            this.panelBackgroundColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBackgroundColor.Location = new System.Drawing.Point(78, 57);
            this.panelBackgroundColor.Name = "panelBackgroundColor";
            this.panelBackgroundColor.Size = new System.Drawing.Size(211, 21);
            this.panelBackgroundColor.TabIndex = 2;
            this.panelBackgroundColor.Paint += new System.Windows.Forms.PaintEventHandler(this.panelBackgroundColor_Paint);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Background:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panelBackgroundColor);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.comboHatch);
            this.panel1.Controls.Add(this.panelForegroundColor);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(208, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(292, 208);
            this.panel1.TabIndex = 6;
            // 
            // panel4
            // 
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.label4);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(6, 6);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(196, 18);
            this.panel4.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(194, 16);
            this.label4.TabIndex = 0;
            this.label4.Text = "Preview";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.panelPreview);
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(6);
            this.panel3.Size = new System.Drawing.Size(208, 208);
            this.panel3.TabIndex = 7;
            // 
            // comboHatch
            // 
            this.comboHatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboHatch.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboHatch.FormattingEnabled = true;
            this.comboHatch.Location = new System.Drawing.Point(78, 3);
            this.comboHatch.Name = "comboHatch";
            this.comboHatch.Size = new System.Drawing.Size(211, 21);
            this.comboHatch.Style = System.Drawing.Drawing2D.HatchStyle.BackwardDiagonal;
            this.comboHatch.TabIndex = 0;
            this.comboHatch.SelectedIndexChanged += new System.EventHandler(this.comboHatch_SelectedIndexChanged);
            // 
            // panelHatch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Name = "panelHatch";
            this.Size = new System.Drawing.Size(500, 208);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelPreview;
		private System.Windows.Forms.Label label1;
		private ComboPatterns comboHatch;
		private System.Windows.Forms.Label label2;
		private UI.GorgonSelectablePanel panelForegroundColor;
		private UI.GorgonSelectablePanel panelBackgroundColor;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Panel panel3;
	}
}
