namespace GorgonLibrary.Graphics.Tools
{
	partial class formPreferences
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formPreferences));
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.buttonPath = new System.Windows.Forms.Button();
			this.textEditorPath = new System.Windows.Forms.TextBox();
			this.textEditorName = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.tipButtons = new System.Windows.Forms.ToolTip(this.components);
			this.buttonBGColor = new System.Windows.Forms.Button();
			this.dialogEditorPath = new System.Windows.Forms.OpenFileDialog();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.checkShowLogo = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.pictureColor = new System.Windows.Forms.PictureBox();
			this.dialogColor = new System.Windows.Forms.ColorDialog();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.checkShowBoundingBox = new System.Windows.Forms.CheckBox();
			this.checkShowBoundingCircle = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureColor)).BeginInit();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.buttonPath);
			this.groupBox1.Controls.Add(this.textEditorPath);
			this.groupBox1.Controls.Add(this.textEditorName);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Location = new System.Drawing.Point(12, 13);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(260, 107);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Image editor";
			// 
			// buttonPath
			// 
			this.buttonPath.Enabled = false;
			this.buttonPath.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.folder;
			this.buttonPath.Location = new System.Drawing.Point(227, 76);
			this.buttonPath.Name = "buttonPath";
			this.buttonPath.Size = new System.Drawing.Size(26, 23);
			this.buttonPath.TabIndex = 2;
			this.tipButtons.SetToolTip(this.buttonPath, "Select an editor.");
			this.buttonPath.UseVisualStyleBackColor = true;
			this.buttonPath.Click += new System.EventHandler(this.buttonPath_Click);
			// 
			// textEditorPath
			// 
			this.textEditorPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textEditorPath.Location = new System.Drawing.Point(10, 78);
			this.textEditorPath.Name = "textEditorPath";
			this.textEditorPath.ReadOnly = true;
			this.textEditorPath.Size = new System.Drawing.Size(211, 20);
			this.textEditorPath.TabIndex = 1;
			// 
			// textEditorName
			// 
			this.textEditorName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textEditorName.Location = new System.Drawing.Point(10, 37);
			this.textEditorName.Name = "textEditorName";
			this.textEditorName.Size = new System.Drawing.Size(243, 20);
			this.textEditorName.TabIndex = 0;
			this.textEditorName.TextChanged += new System.EventHandler(this.textEditorName_TextChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(7, 62);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(61, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Editor path:";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(7, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(66, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Editor name:";
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.check;
			this.buttonOK.Location = new System.Drawing.Point(214, 287);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(26, 23);
			this.buttonOK.TabIndex = 2;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete;
			this.buttonCancel.Location = new System.Drawing.Point(246, 287);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(26, 23);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonBGColor
			// 
			this.buttonBGColor.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.Color;
			this.buttonBGColor.Location = new System.Drawing.Point(227, 16);
			this.buttonBGColor.Name = "buttonBGColor";
			this.buttonBGColor.Size = new System.Drawing.Size(26, 23);
			this.buttonBGColor.TabIndex = 0;
			this.tipButtons.SetToolTip(this.buttonBGColor, "Select the background color.");
			this.buttonBGColor.UseVisualStyleBackColor = true;
			this.buttonBGColor.Click += new System.EventHandler(this.buttonBGColor_Click);
			// 
			// dialogEditorPath
			// 
			this.dialogEditorPath.Filter = "Executable files (*.exe)|*.exe";
			this.dialogEditorPath.InitialDirectory = ".\\";
			this.dialogEditorPath.Title = "Select an image editor.";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.checkShowLogo);
			this.groupBox2.Controls.Add(this.buttonBGColor);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.pictureColor);
			this.groupBox2.Location = new System.Drawing.Point(12, 127);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(260, 73);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Background";
			// 
			// checkShowLogo
			// 
			this.checkShowLogo.AutoSize = true;
			this.checkShowLogo.Location = new System.Drawing.Point(10, 42);
			this.checkShowLogo.Name = "checkShowLogo";
			this.checkShowLogo.Size = new System.Drawing.Size(153, 17);
			this.checkShowLogo.TabIndex = 1;
			this.checkShowLogo.Text = "Show logo in sprite display.";
			this.checkShowLogo.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(7, 21);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(34, 13);
			this.label3.TabIndex = 1;
			this.label3.Text = "Color:";
			// 
			// pictureColor
			// 
			this.pictureColor.BackColor = System.Drawing.Color.White;
			this.pictureColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureColor.Location = new System.Drawing.Point(47, 20);
			this.pictureColor.Name = "pictureColor";
			this.pictureColor.Size = new System.Drawing.Size(174, 16);
			this.pictureColor.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureColor.TabIndex = 0;
			this.pictureColor.TabStop = false;
			// 
			// dialogColor
			// 
			this.dialogColor.AnyColor = true;
			this.dialogColor.FullOpen = true;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.checkShowBoundingCircle);
			this.groupBox3.Controls.Add(this.checkShowBoundingBox);
			this.groupBox3.Location = new System.Drawing.Point(12, 206);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(260, 73);
			this.groupBox3.TabIndex = 4;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Bounding Areas";
			// 
			// checkShowBoundingBox
			// 
			this.checkShowBoundingBox.AutoSize = true;
			this.checkShowBoundingBox.Location = new System.Drawing.Point(10, 19);
			this.checkShowBoundingBox.Name = "checkShowBoundingBox";
			this.checkShowBoundingBox.Size = new System.Drawing.Size(171, 17);
			this.checkShowBoundingBox.TabIndex = 1;
			this.checkShowBoundingBox.Text = "Show bounding box for sprites.";
			this.checkShowBoundingBox.UseVisualStyleBackColor = true;
			// 
			// checkShowBoundingCircle
			// 
			this.checkShowBoundingCircle.AutoSize = true;
			this.checkShowBoundingCircle.Location = new System.Drawing.Point(10, 42);
			this.checkShowBoundingCircle.Name = "checkShowBoundingCircle";
			this.checkShowBoundingCircle.Size = new System.Drawing.Size(179, 17);
			this.checkShowBoundingCircle.TabIndex = 2;
			this.checkShowBoundingCircle.Text = "Show bounding circle for sprites.";
			this.checkShowBoundingCircle.UseVisualStyleBackColor = true;
			// 
			// formPreferences
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 322);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formPreferences";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Preferences";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.formPreferences_KeyDown);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureColor)).EndInit();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox textEditorName;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonPath;
		private System.Windows.Forms.TextBox textEditorPath;
		private System.Windows.Forms.ToolTip tipButtons;
		private System.Windows.Forms.OpenFileDialog dialogEditorPath;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.PictureBox pictureColor;
		private System.Windows.Forms.CheckBox checkShowLogo;
		private System.Windows.Forms.Button buttonBGColor;
		private System.Windows.Forms.ColorDialog dialogColor;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.CheckBox checkShowBoundingBox;
		private System.Windows.Forms.CheckBox checkShowBoundingCircle;
	}
}