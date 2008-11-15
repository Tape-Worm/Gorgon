namespace GorgonLibrary.Graphics.Tools
{
	partial class formNewSprite
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formNewSprite));
			this.label1 = new System.Windows.Forms.Label();
			this.textName = new System.Windows.Forms.TextBox();
			this.groupImage = new System.Windows.Forms.GroupBox();
			this.checkRenderTarget = new System.Windows.Forms.CheckBox();
			this.comboImages = new System.Windows.Forms.ComboBox();
			this.buttonImageManager = new System.Windows.Forms.Button();
			this.labelImageName = new System.Windows.Forms.Label();
			this.tips = new System.Windows.Forms.ToolTip(this.components);
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.groupImage.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(38, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Name:";
			// 
			// textName
			// 
			this.textName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textName.Location = new System.Drawing.Point(56, 13);
			this.textName.Name = "textName";
			this.textName.Size = new System.Drawing.Size(287, 20);
			this.textName.TabIndex = 0;
			this.textName.TextChanged += new System.EventHandler(this.textName_TextChanged);
			// 
			// groupImage
			// 
			this.groupImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupImage.Controls.Add(this.checkRenderTarget);
			this.groupImage.Controls.Add(this.comboImages);
			this.groupImage.Controls.Add(this.buttonImageManager);
			this.groupImage.Controls.Add(this.labelImageName);
			this.groupImage.Location = new System.Drawing.Point(12, 45);
			this.groupImage.Name = "groupImage";
			this.groupImage.Size = new System.Drawing.Size(331, 70);
			this.groupImage.TabIndex = 2;
			this.groupImage.TabStop = false;
			this.groupImage.Text = "Image";
			// 
			// checkRenderTarget
			// 
			this.checkRenderTarget.AutoSize = true;
			this.checkRenderTarget.Location = new System.Drawing.Point(9, 47);
			this.checkRenderTarget.Name = "checkRenderTarget";
			this.checkRenderTarget.Size = new System.Drawing.Size(111, 17);
			this.checkRenderTarget.TabIndex = 2;
			this.checkRenderTarget.Text = "Use render target.";
			this.checkRenderTarget.UseVisualStyleBackColor = true;
			this.checkRenderTarget.CheckedChanged += new System.EventHandler(this.checkRenderTarget_CheckedChanged);
			// 
			// comboImages
			// 
			this.comboImages.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.comboImages.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.comboImages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboImages.FormattingEnabled = true;
			this.comboImages.Location = new System.Drawing.Point(46, 16);
			this.comboImages.Name = "comboImages";
			this.comboImages.Size = new System.Drawing.Size(245, 21);
			this.comboImages.TabIndex = 0;
			this.comboImages.SelectedIndexChanged += new System.EventHandler(this.comboImages_SelectedIndexChanged);
			// 
			// buttonImageManager
			// 
			this.buttonImageManager.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.photo_scenery;
			this.buttonImageManager.Location = new System.Drawing.Point(297, 15);
			this.buttonImageManager.Name = "buttonImageManager";
			this.buttonImageManager.Size = new System.Drawing.Size(26, 23);
			this.buttonImageManager.TabIndex = 1;
			this.tips.SetToolTip(this.buttonImageManager, "Click here to load an image.");
			this.buttonImageManager.UseVisualStyleBackColor = true;
			this.buttonImageManager.Click += new System.EventHandler(this.buttonImageManager_Click);
			// 
			// labelImageName
			// 
			this.labelImageName.AutoSize = true;
			this.labelImageName.Location = new System.Drawing.Point(6, 19);
			this.labelImageName.Name = "labelImageName";
			this.labelImageName.Size = new System.Drawing.Size(38, 13);
			this.labelImageName.TabIndex = 0;
			this.labelImageName.Text = "Name:";
			// 
			// tips
			// 
			this.tips.IsBalloon = true;
			this.tips.StripAmpersands = true;
			this.tips.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
			this.tips.ToolTipTitle = "What is this?";
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.check;
			this.buttonOK.Location = new System.Drawing.Point(285, 121);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(26, 23);
			this.buttonOK.TabIndex = 1;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete;
			this.buttonCancel.Location = new System.Drawing.Point(317, 121);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(26, 23);
			this.buttonCancel.TabIndex = 2;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// formNewSprite
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(355, 151);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.groupImage);
			this.Controls.Add(this.textName);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formNewSprite";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "New sprite";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.formNewSprite_KeyDown);
			this.groupImage.ResumeLayout(false);
			this.groupImage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textName;
		private System.Windows.Forms.GroupBox groupImage;
		private System.Windows.Forms.Button buttonImageManager;
		private System.Windows.Forms.Label labelImageName;
		private System.Windows.Forms.CheckBox checkRenderTarget;
		private System.Windows.Forms.ComboBox comboImages;
		private System.Windows.Forms.ToolTip tips;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
	}
}