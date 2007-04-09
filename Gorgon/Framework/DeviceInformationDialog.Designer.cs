namespace GorgonLibrary.Framework
{
	internal partial class DeviceInformationDialog
	{
		private System.Windows.Forms.Button OK;
		private System.Windows.Forms.ImageList imgIcons;
		private System.ComponentModel.IContainer components;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeviceInformationDialog));
			this.imgIcons = new System.Windows.Forms.ImageList(this.components);
			this.driverName = new SharpUtilities.Controls.RoundBox();
			this.driverDescription = new System.Windows.Forms.Label();
			this.fadeTimer = new System.Windows.Forms.Timer(this.components);
			this.OK = new System.Windows.Forms.Button();
			this.cardInfo = new SharpUtilities.Controls.InfoGrid();
			this.driverName.SuspendLayout();
			this.SuspendLayout();
			// 
			// imgIcons
			// 
			this.imgIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgIcons.ImageStream")));
			this.imgIcons.TransparentColor = System.Drawing.Color.Transparent;
			this.imgIcons.Images.SetKeyName(0, "");
			this.imgIcons.Images.SetKeyName(1, "");
			// 
			// driverName
			// 
			this.driverName.BorderColor = System.Drawing.Color.Black;
			this.driverName.Controls.Add(this.driverDescription);
			this.driverName.DisabledGradient1 = System.Drawing.Color.Gray;
			this.driverName.DisabledGradient2 = System.Drawing.Color.DarkGray;
			this.driverName.GradientAngle = 270F;
			this.driverName.GradientColor1 = System.Drawing.Color.Gray;
			this.driverName.GradientColor2 = System.Drawing.Color.Gray;
			this.driverName.Location = new System.Drawing.Point(0, 0);
			this.driverName.Name = "driverName";
			this.driverName.Size = new System.Drawing.Size(482, 37);
			this.driverName.TabIndex = 10;
			// 
			// driverDescription
			// 
			this.driverDescription.BackColor = System.Drawing.Color.Transparent;
			this.driverDescription.Font = new System.Drawing.Font("Georgia", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.driverDescription.ForeColor = System.Drawing.Color.White;
			this.driverDescription.Location = new System.Drawing.Point(3, 0);
			this.driverDescription.Name = "driverDescription";
			this.driverDescription.Size = new System.Drawing.Size(476, 37);
			this.driverDescription.TabIndex = 0;
			this.driverDescription.Text = "Bitchin\' Fast 3D 2000";
			this.driverDescription.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// fadeTimer
			// 
			this.fadeTimer.Interval = 10;
			this.fadeTimer.Tick += new System.EventHandler(this.fadeTimer_Tick);
			// 
			// OK
			// 
			this.OK.BackColor = System.Drawing.SystemColors.Control;
			this.OK.Cursor = System.Windows.Forms.Cursors.Hand;
			this.OK.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.OK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.OK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.OK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.OK.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.ForeColor = System.Drawing.Color.Black;
			this.OK.Image = global::GorgonLibrary.Properties.Resources.check;
			this.OK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.OK.Location = new System.Drawing.Point(422, 318);
			this.OK.Name = "OK";
			this.OK.Size = new System.Drawing.Size(57, 24);
			this.OK.TabIndex = 8;
			this.OK.Text = "OK";
			this.OK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.OK.UseVisualStyleBackColor = false;
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// cardInfo
			// 
			this.cardInfo.BorderStyle = SharpUtilities.Controls.InfoGridBorderStyles.FixedSingle;
			this.cardInfo.CaptionBackgroundGradient1 = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this.cardInfo.CaptionBackgroundStyle = SharpUtilities.Controls.InfoGridBackgroundStyles.LeftDiagonal;
			this.cardInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cardInfo.Location = new System.Drawing.Point(6, 43);
			this.cardInfo.Name = "cardInfo";
			this.cardInfo.Size = new System.Drawing.Size(473, 269);
			this.cardInfo.TabIndex = 11;
			this.cardInfo.Text = "Bitchin\' Fast 3D 2000 capabilities";
			// 
			// DeviceInformationDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(482, 346);
			this.Controls.Add(this.cardInfo);
			this.Controls.Add(this.driverName);
			this.Controls.Add(this.OK);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(6)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DeviceInformationDialog";
			this.Opacity = 0.01;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Device Info";
			this.driverName.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private SharpUtilities.Controls.RoundBox driverName;
		private System.Windows.Forms.Label driverDescription;
		private System.Windows.Forms.Timer fadeTimer;
		private SharpUtilities.Controls.InfoGrid cardInfo;
	}
}
