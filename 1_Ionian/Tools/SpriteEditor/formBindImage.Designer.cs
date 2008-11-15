#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Friday, May 18, 2007 2:48:50 PM
// 
#endregion

namespace GorgonLibrary.Graphics.Tools
{
	partial class formBindImage
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formBindImage));
			this.comboImages = new System.Windows.Forms.ComboBox();
			this.buttonImageManager = new System.Windows.Forms.Button();
			this.labelImageName = new System.Windows.Forms.Label();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.checkRenderTarget = new System.Windows.Forms.CheckBox();
			this.tips = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// comboImages
			// 
			this.comboImages.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.comboImages.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.comboImages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboImages.FormattingEnabled = true;
			this.comboImages.Location = new System.Drawing.Point(53, 12);
			this.comboImages.Name = "comboImages";
			this.comboImages.Size = new System.Drawing.Size(245, 21);
			this.comboImages.TabIndex = 4;
			this.comboImages.SelectedIndexChanged += new System.EventHandler(this.comboImages_SelectedIndexChanged);
			// 
			// buttonImageManager
			// 
			this.buttonImageManager.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.photo_scenery;
			this.buttonImageManager.Location = new System.Drawing.Point(304, 12);
			this.buttonImageManager.Name = "buttonImageManager";
			this.buttonImageManager.Size = new System.Drawing.Size(26, 23);
			this.buttonImageManager.TabIndex = 6;
			this.buttonImageManager.UseVisualStyleBackColor = true;
			this.buttonImageManager.Click += new System.EventHandler(this.buttonImageManager_Click);
			// 
			// labelImageName
			// 
			this.labelImageName.AutoSize = true;
			this.labelImageName.Location = new System.Drawing.Point(9, 15);
			this.labelImageName.Name = "labelImageName";
			this.labelImageName.Size = new System.Drawing.Size(38, 13);
			this.labelImageName.TabIndex = 3;
			this.labelImageName.Text = "Name:";
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.check;
			this.buttonOK.Location = new System.Drawing.Point(272, 62);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(26, 23);
			this.buttonOK.TabIndex = 5;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete;
			this.buttonCancel.Location = new System.Drawing.Point(304, 62);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(26, 23);
			this.buttonCancel.TabIndex = 8;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// checkRenderTarget
			// 
			this.checkRenderTarget.AutoSize = true;
			this.checkRenderTarget.Location = new System.Drawing.Point(12, 39);
			this.checkRenderTarget.Name = "checkRenderTarget";
			this.checkRenderTarget.Size = new System.Drawing.Size(111, 17);
			this.checkRenderTarget.TabIndex = 7;
			this.checkRenderTarget.Text = "Use render target.";
			this.checkRenderTarget.UseVisualStyleBackColor = true;
			this.checkRenderTarget.Click += new System.EventHandler(this.checkRenderTarget_Click);
			// 
			// tips
			// 
			this.tips.IsBalloon = true;
			this.tips.StripAmpersands = true;
			this.tips.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
			this.tips.ToolTipTitle = "What is this?";
			// 
			// formBindImage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(341, 93);
			this.Controls.Add(this.comboImages);
			this.Controls.Add(this.buttonImageManager);
			this.Controls.Add(this.labelImageName);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.checkRenderTarget);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formBindImage";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Bind image to sprite.";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox comboImages;
		private System.Windows.Forms.Button buttonImageManager;
		private System.Windows.Forms.Label labelImageName;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.CheckBox checkRenderTarget;
		private System.Windows.Forms.ToolTip tips;
	}
}