namespace GorgonLibrary.Editor.SpriteEditorPlugIn.Controls
{
	partial class PanelSpriteEditor
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
			this.panelSprite = new System.Windows.Forms.Panel();
			this.PanelDisplay.SuspendLayout();
			this.SuspendLayout();
			// 
			// PanelDisplay
			// 
			this.PanelDisplay.Controls.Add(this.panelSprite);
			this.PanelDisplay.Size = new System.Drawing.Size(806, 606);
			// 
			// panelSprite
			// 
			this.panelSprite.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.panelSprite.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelSprite.Location = new System.Drawing.Point(0, 0);
			this.panelSprite.Name = "panelSprite";
			this.panelSprite.Size = new System.Drawing.Size(806, 606);
			this.panelSprite.TabIndex = 0;
			// 
			// PanelSpriteEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.Name = "PanelSpriteEditor";
			this.Size = new System.Drawing.Size(806, 636);
			this.Text = "not localized sprite";
			this.PanelDisplay.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		internal System.Windows.Forms.Panel panelSprite;

	}
}
