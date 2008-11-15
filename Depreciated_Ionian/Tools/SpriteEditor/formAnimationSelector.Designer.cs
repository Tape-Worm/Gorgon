namespace GorgonLibrary.Graphics.Tools
{
	partial class formAnimationSelector
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formAnimationSelector));
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.checkedAnimations = new System.Windows.Forms.CheckedListBox();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.check;
			this.buttonOK.Location = new System.Drawing.Point(214, 229);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(26, 23);
			this.buttonOK.TabIndex = 1;
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete;
			this.buttonCancel.Location = new System.Drawing.Point(246, 229);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(26, 23);
			this.buttonCancel.TabIndex = 2;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// checkedAnimations
			// 
			this.checkedAnimations.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.checkedAnimations.CheckOnClick = true;
			this.checkedAnimations.Dock = System.Windows.Forms.DockStyle.Top;
			this.checkedAnimations.FormattingEnabled = true;
			this.checkedAnimations.Location = new System.Drawing.Point(0, 0);
			this.checkedAnimations.Name = "checkedAnimations";
			this.checkedAnimations.Size = new System.Drawing.Size(284, 225);
			this.checkedAnimations.TabIndex = 0;
			this.checkedAnimations.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedAnimations_ItemCheck);
			// 
			// formAnimationSelector
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(284, 264);
			this.Controls.Add(this.checkedAnimations);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formAnimationSelector";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Select animations to play.";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.CheckedListBox checkedAnimations;
	}
}