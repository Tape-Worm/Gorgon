namespace GorgonLibrary.Editor.ImageEditorPlugIn.Controls
{
	partial class PanelImagePreferences
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
			this.panel5 = new System.Windows.Forms.Panel();
			this.panel6 = new System.Windows.Forms.Panel();
			this.checkShowAnimations = new System.Windows.Forms.CheckBox();
			this.labelImageEditorSettings = new System.Windows.Forms.Label();
			this.checkShowActualSize = new System.Windows.Forms.CheckBox();
			this.panel5.SuspendLayout();
			this.panel6.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel5
			// 
			this.panel5.Controls.Add(this.panel6);
			this.panel5.Controls.Add(this.labelImageEditorSettings);
			this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel5.Location = new System.Drawing.Point(3, 3);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(623, 75);
			this.panel5.TabIndex = 2;
			// 
			// panel6
			// 
			this.panel6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.panel6.Controls.Add(this.checkShowActualSize);
			this.panel6.Controls.Add(this.checkShowAnimations);
			this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel6.Location = new System.Drawing.Point(0, 18);
			this.panel6.Name = "panel6";
			this.panel6.Size = new System.Drawing.Size(623, 57);
			this.panel6.TabIndex = 1;
			// 
			// checkShowAnimations
			// 
			this.checkShowAnimations.AutoSize = true;
			this.checkShowAnimations.Location = new System.Drawing.Point(7, 6);
			this.checkShowAnimations.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.checkShowAnimations.Name = "checkShowAnimations";
			this.checkShowAnimations.Size = new System.Drawing.Size(236, 19);
			this.checkShowAnimations.TabIndex = 0;
			this.checkShowAnimations.Text = "notlocalized show transition animations";
			this.checkShowAnimations.UseVisualStyleBackColor = true;
			// 
			// labelImageEditorSettings
			// 
			this.labelImageEditorSettings.Dock = System.Windows.Forms.DockStyle.Top;
			this.labelImageEditorSettings.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelImageEditorSettings.Location = new System.Drawing.Point(0, 0);
			this.labelImageEditorSettings.Name = "labelImageEditorSettings";
			this.labelImageEditorSettings.Size = new System.Drawing.Size(623, 18);
			this.labelImageEditorSettings.TabIndex = 0;
			this.labelImageEditorSettings.Text = "not localized image editor settings";
			// 
			// checkShowActualSize
			// 
			this.checkShowActualSize.AutoSize = true;
			this.checkShowActualSize.Location = new System.Drawing.Point(7, 31);
			this.checkShowActualSize.Name = "checkShowActualSize";
			this.checkShowActualSize.Size = new System.Drawing.Size(276, 19);
			this.checkShowActualSize.TabIndex = 1;
			this.checkShowActualSize.Text = "notlocalized show images as actual size on load";
			this.checkShowActualSize.UseVisualStyleBackColor = true;
			// 
			// PanelImagePreferences
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.Controls.Add(this.panel5);
			this.Name = "PanelImagePreferences";
			this.Size = new System.Drawing.Size(629, 442);
			this.panel5.ResumeLayout(false);
			this.panel6.ResumeLayout(false);
			this.panel6.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.Panel panel6;
		private System.Windows.Forms.CheckBox checkShowAnimations;
		private System.Windows.Forms.Label labelImageEditorSettings;
		private System.Windows.Forms.CheckBox checkShowActualSize;

	}
}
