namespace Gorgon.Editor
{
	partial class ContentPanel
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
				if (Renderer != null)
				{
					Renderer.Dispose();
				}

				Renderer = null;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ContentPanel));
			this.panelCaption = new System.Windows.Forms.Panel();
			this.labelCaption = new System.Windows.Forms.Label();
			this.labelClose = new System.Windows.Forms.Label();
			this._panelContentDisplay = new System.Windows.Forms.Panel();
			this.panelCaption.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelCaption
			// 
			this.panelCaption.Controls.Add(this.labelCaption);
			this.panelCaption.Controls.Add(this.labelClose);
			resources.ApplyResources(this.panelCaption, "panelCaption");
			this.panelCaption.Name = "panelCaption";
			// 
			// labelCaption
			// 
			resources.ApplyResources(this.labelCaption, "labelCaption");
			this.labelCaption.Name = "labelCaption";
			// 
			// labelClose
			// 
			resources.ApplyResources(this.labelClose, "labelClose");
			this.labelClose.Name = "labelClose";
			this.labelClose.Click += new System.EventHandler(this.labelClose_Click);
			this.labelClose.MouseEnter += new System.EventHandler(this.labelClose_MouseEnter);
			this.labelClose.MouseLeave += new System.EventHandler(this.labelClose_MouseLeave);
			this.labelClose.MouseHover += new System.EventHandler(this.labelClose_MouseEnter);
			this.labelClose.MouseMove += new System.Windows.Forms.MouseEventHandler(this.labelClose_MouseMove);
			// 
			// _panelContentDisplay
			// 
			resources.ApplyResources(this._panelContentDisplay, "_panelContentDisplay");
			this._panelContentDisplay.Name = "_panelContentDisplay";
			// 
			// ContentPanel
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._panelContentDisplay);
			this.Controls.Add(this.panelCaption);
			this.Name = "ContentPanel";
			this.panelCaption.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelCaption;
		private System.Windows.Forms.Label labelCaption;
		private System.Windows.Forms.Label labelClose;
	}
}
