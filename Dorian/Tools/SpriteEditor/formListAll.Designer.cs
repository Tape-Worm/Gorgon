namespace GorgonLibrary.Graphics.Tools
{
	partial class formListAll
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formListAll));
			this.listSprites = new System.Windows.Forms.ListView();
			this.columnFrameName = new System.Windows.Forms.ColumnHeader();
			this.columnDimensions = new System.Windows.Forms.ColumnHeader();
			this.label1 = new System.Windows.Forms.Label();
			this.numericDelay = new System.Windows.Forms.NumericUpDown();
			this.pictureSprite = new System.Windows.Forms.PictureBox();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.numericDelay)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureSprite)).BeginInit();
			this.SuspendLayout();
			// 
			// listSprites
			// 
			this.listSprites.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.listSprites.CheckBoxes = true;
			this.listSprites.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnFrameName,
            this.columnDimensions});
			this.listSprites.FullRowSelect = true;
			this.listSprites.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listSprites.Location = new System.Drawing.Point(12, 12);
			this.listSprites.Name = "listSprites";
			this.listSprites.Size = new System.Drawing.Size(303, 303);
			this.listSprites.TabIndex = 10;
			this.listSprites.UseCompatibleStateImageBehavior = false;
			this.listSprites.View = System.Windows.Forms.View.Details;
			this.listSprites.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listSprites_ItemChecked);
			this.listSprites.SelectedIndexChanged += new System.EventHandler(this.listSprites_SelectedIndexChanged);
			// 
			// columnFrameName
			// 
			this.columnFrameName.Text = "Frame Name";
			// 
			// columnDimensions
			// 
			this.columnDimensions.Text = "Dimensions";
			this.columnDimensions.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(321, 143);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 13);
			this.label1.TabIndex = 12;
			this.label1.Text = "Delay interval (ms):";
			// 
			// numericDelay
			// 
			this.numericDelay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.numericDelay.DecimalPlaces = 4;
			this.numericDelay.Location = new System.Drawing.Point(324, 159);
			this.numericDelay.Maximum = new decimal(new int[] {
            -727379969,
            232,
            0,
            327680});
			this.numericDelay.Name = "numericDelay";
			this.numericDelay.Size = new System.Drawing.Size(128, 20);
			this.numericDelay.TabIndex = 13;
			this.numericDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// pictureSprite
			// 
			this.pictureSprite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureSprite.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureSprite.Location = new System.Drawing.Point(324, 12);
			this.pictureSprite.Name = "pictureSprite";
			this.pictureSprite.Size = new System.Drawing.Size(128, 128);
			this.pictureSprite.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureSprite.TabIndex = 11;
			this.pictureSprite.TabStop = false;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.check;
			this.buttonOK.Location = new System.Drawing.Point(394, 292);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(26, 23);
			this.buttonOK.TabIndex = 8;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete;
			this.buttonCancel.Location = new System.Drawing.Point(426, 292);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(26, 23);
			this.buttonCancel.TabIndex = 9;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// formListAll
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(464, 327);
			this.Controls.Add(this.numericDelay);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.pictureSprite);
			this.Controls.Add(this.listSprites);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonCancel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "formListAll";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Import sprite frames.";
			((System.ComponentModel.ISupportInitialize)(this.numericDelay)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureSprite)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.ListView listSprites;
		private System.Windows.Forms.PictureBox pictureSprite;
		private System.Windows.Forms.ColumnHeader columnFrameName;
		private System.Windows.Forms.ColumnHeader columnDimensions;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown numericDelay;
	}
}