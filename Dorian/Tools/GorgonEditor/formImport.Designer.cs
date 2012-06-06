namespace GorgonLibrary.GorgonEditor
{
	partial class formImport
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formImport));
			this.label1 = new System.Windows.Forms.Label();
			this.textDestination = new System.Windows.Forms.TextBox();
			this.buttonSelectDestination = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.buttonImportFile = new System.Windows.Forms.Button();
			this.textImportFile = new System.Windows.Forms.TextBox();
			this.dialogFile = new System.Windows.Forms.OpenFileDialog();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.panel2 = new GorgonLibrary.GorgonEditor.panelEx();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 55);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(63, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Destination:";
			// 
			// textDestination
			// 
			this.textDestination.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textDestination.Enabled = false;
			this.textDestination.Location = new System.Drawing.Point(12, 71);
			this.textDestination.Name = "textDestination";
			this.textDestination.Size = new System.Drawing.Size(413, 20);
			this.textDestination.TabIndex = 2;
			this.textDestination.TextChanged += new System.EventHandler(this.textDestination_TextChanged);
			// 
			// buttonSelectDestination
			// 
			this.buttonSelectDestination.Enabled = false;
			this.buttonSelectDestination.Image = global::GorgonLibrary.GorgonEditor.Properties.Resources.folder_16x16;
			this.buttonSelectDestination.Location = new System.Drawing.Point(431, 69);
			this.buttonSelectDestination.Name = "buttonSelectDestination";
			this.buttonSelectDestination.Size = new System.Drawing.Size(33, 24);
			this.buttonSelectDestination.TabIndex = 3;
			this.buttonSelectDestination.UseVisualStyleBackColor = true;
			this.buttonSelectDestination.Click += new System.EventHandler(this.buttonSelectDestination_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 9);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(26, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "File:";
			// 
			// buttonImportFile
			// 
			this.buttonImportFile.Image = global::GorgonLibrary.GorgonEditor.Properties.Resources.folder_16x16;
			this.buttonImportFile.Location = new System.Drawing.Point(431, 23);
			this.buttonImportFile.Name = "buttonImportFile";
			this.buttonImportFile.Size = new System.Drawing.Size(33, 24);
			this.buttonImportFile.TabIndex = 1;
			this.buttonImportFile.UseVisualStyleBackColor = true;
			this.buttonImportFile.Click += new System.EventHandler(this.buttonImportFile_Click);
			// 
			// textImportFile
			// 
			this.textImportFile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textImportFile.Location = new System.Drawing.Point(12, 25);
			this.textImportFile.Name = "textImportFile";
			this.textImportFile.ReadOnly = true;
			this.textImportFile.Size = new System.Drawing.Size(413, 20);
			this.textImportFile.TabIndex = 0;
			this.textImportFile.TextChanged += new System.EventHandler(this.textImportFile_TextChanged);
			// 
			// dialogFile
			// 
			this.dialogFile.Filter = "All Files (*.*)|*.*";
			this.dialogFile.Multiselect = true;
			this.dialogFile.Title = "Open file for import...";
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.buttonOK.Image = global::GorgonLibrary.GorgonEditor.Properties.Resources.ok_16x16;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(286, 16);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(87, 28);
			this.buttonOK.TabIndex = 0;
			this.buttonOK.Text = "OK";
			this.buttonOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Image = global::GorgonLibrary.GorgonEditor.Properties.Resources.cancel_16x16;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(379, 16);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 28);
			this.buttonCancel.TabIndex = 1;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.buttonOK);
			this.panel2.Controls.Add(this.buttonCancel);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(0, 100);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(478, 56);
			this.panel2.TabIndex = 4;
			// 
			// formImport
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(478, 156);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.buttonImportFile);
			this.Controls.Add(this.textImportFile);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.buttonSelectDestination);
			this.Controls.Add(this.textDestination);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formImport";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Import";
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textDestination;
		private System.Windows.Forms.Button buttonSelectDestination;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button buttonImportFile;
		private System.Windows.Forms.TextBox textImportFile;
		private panelEx panel2;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.OpenFileDialog dialogFile;
	}
}