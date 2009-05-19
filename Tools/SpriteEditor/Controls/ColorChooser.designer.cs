using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics.Tools
{
	internal partial class ColorChooser
		: System.Windows.Forms.Form
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.Label3 = new System.Windows.Forms.Label();
			this.nudSaturation = new System.Windows.Forms.NumericUpDown();
			this.Label7 = new System.Windows.Forms.Label();
			this.nudBrightness = new System.Windows.Forms.NumericUpDown();
			this.nudRed = new System.Windows.Forms.NumericUpDown();
			this.pnlColor = new System.Windows.Forms.Panel();
			this.Label6 = new System.Windows.Forms.Label();
			this.Label1 = new System.Windows.Forms.Label();
			this.Label5 = new System.Windows.Forms.Label();
			this.pnlBrightness = new System.Windows.Forms.Panel();
			this.nudBlue = new System.Windows.Forms.NumericUpDown();
			this.Label4 = new System.Windows.Forms.Label();
			this.nudGreen = new System.Windows.Forms.NumericUpDown();
			this.Label2 = new System.Windows.Forms.Label();
			this.nudHue = new System.Windows.Forms.NumericUpDown();
			this.labelAlpha = new System.Windows.Forms.Label();
			this.numericAlpha = new System.Windows.Forms.NumericUpDown();
			this.panelAlpha = new System.Windows.Forms.Panel();
			this.pnlSelectedColor = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.nudSaturation)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudBrightness)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudRed)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudBlue)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudGreen)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudHue)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericAlpha)).BeginInit();
			this.SuspendLayout();
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnCancel.Location = new System.Drawing.Point(290, 376);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(64, 24);
			this.btnCancel.TabIndex = 55;
			this.btnCancel.Text = "Cancel";
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnOK.Location = new System.Drawing.Point(218, 376);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(64, 24);
			this.btnOK.TabIndex = 54;
			this.btnOK.Text = "OK";
			// 
			// Label3
			// 
			this.Label3.AutoSize = true;
			this.Label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label3.Location = new System.Drawing.Point(123, 327);
			this.Label3.Name = "Label3";
			this.Label3.Size = new System.Drawing.Size(31, 13);
			this.Label3.TabIndex = 45;
			this.Label3.Text = "Blue:";
			this.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// nudSaturation
			// 
			this.nudSaturation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.nudSaturation.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nudSaturation.Location = new System.Drawing.Point(69, 297);
			this.nudSaturation.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.nudSaturation.Name = "nudSaturation";
			this.nudSaturation.Size = new System.Drawing.Size(48, 22);
			this.nudSaturation.TabIndex = 42;
			this.nudSaturation.ValueChanged += new System.EventHandler(this.HandleHSVChange);
			this.nudSaturation.TextChanged += new System.EventHandler(this.HandleTextChanged);
			// 
			// Label7
			// 
			this.Label7.AutoSize = true;
			this.Label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label7.Location = new System.Drawing.Point(4, 327);
			this.Label7.Name = "Label7";
			this.Label7.Size = new System.Drawing.Size(59, 13);
			this.Label7.TabIndex = 50;
			this.Label7.Text = "Brightness:";
			this.Label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// nudBrightness
			// 
			this.nudBrightness.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.nudBrightness.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nudBrightness.Location = new System.Drawing.Point(69, 323);
			this.nudBrightness.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.nudBrightness.Name = "nudBrightness";
			this.nudBrightness.Size = new System.Drawing.Size(48, 22);
			this.nudBrightness.TabIndex = 47;
			this.nudBrightness.ValueChanged += new System.EventHandler(this.HandleHSVChange);
			this.nudBrightness.TextChanged += new System.EventHandler(this.HandleTextChanged);
			// 
			// nudRed
			// 
			this.nudRed.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.nudRed.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nudRed.Location = new System.Drawing.Point(168, 272);
			this.nudRed.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.nudRed.Name = "nudRed";
			this.nudRed.Size = new System.Drawing.Size(48, 22);
			this.nudRed.TabIndex = 38;
			this.nudRed.ValueChanged += new System.EventHandler(this.HandleRGBChange);
			this.nudRed.TextChanged += new System.EventHandler(this.HandleTextChanged);
			// 
			// pnlColor
			// 
			this.pnlColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlColor.Location = new System.Drawing.Point(8, 8);
			this.pnlColor.Name = "pnlColor";
			this.pnlColor.Size = new System.Drawing.Size(256, 256);
			this.pnlColor.TabIndex = 51;
			this.pnlColor.Visible = false;
			this.pnlColor.MouseUp += new System.Windows.Forms.MouseEventHandler(this.frmMain_MouseUp);
			// 
			// Label6
			// 
			this.Label6.AutoSize = true;
			this.Label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label6.Location = new System.Drawing.Point(5, 301);
			this.Label6.Name = "Label6";
			this.Label6.Size = new System.Drawing.Size(58, 13);
			this.Label6.TabIndex = 49;
			this.Label6.Text = "Saturation:";
			this.Label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// Label1
			// 
			this.Label1.AutoSize = true;
			this.Label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label1.Location = new System.Drawing.Point(123, 276);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(30, 13);
			this.Label1.TabIndex = 43;
			this.Label1.Text = "Red:";
			this.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// Label5
			// 
			this.Label5.AutoSize = true;
			this.Label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label5.Location = new System.Drawing.Point(5, 276);
			this.Label5.Name = "Label5";
			this.Label5.Size = new System.Drawing.Size(30, 13);
			this.Label5.TabIndex = 48;
			this.Label5.Text = "Hue:";
			this.Label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// pnlBrightness
			// 
			this.pnlBrightness.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlBrightness.Location = new System.Drawing.Point(271, 8);
			this.pnlBrightness.Name = "pnlBrightness";
			this.pnlBrightness.Size = new System.Drawing.Size(16, 256);
			this.pnlBrightness.TabIndex = 52;
			this.pnlBrightness.Visible = false;
			// 
			// nudBlue
			// 
			this.nudBlue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.nudBlue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nudBlue.Location = new System.Drawing.Point(168, 323);
			this.nudBlue.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.nudBlue.Name = "nudBlue";
			this.nudBlue.Size = new System.Drawing.Size(48, 22);
			this.nudBlue.TabIndex = 40;
			this.nudBlue.ValueChanged += new System.EventHandler(this.HandleRGBChange);
			this.nudBlue.TextChanged += new System.EventHandler(this.HandleTextChanged);
			// 
			// Label4
			// 
			this.Label4.AutoSize = true;
			this.Label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label4.Location = new System.Drawing.Point(230, 276);
			this.Label4.Name = "Label4";
			this.Label4.Size = new System.Drawing.Size(34, 13);
			this.Label4.TabIndex = 46;
			this.Label4.Text = "Color:";
			this.Label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// nudGreen
			// 
			this.nudGreen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.nudGreen.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nudGreen.Location = new System.Drawing.Point(168, 297);
			this.nudGreen.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.nudGreen.Name = "nudGreen";
			this.nudGreen.Size = new System.Drawing.Size(48, 22);
			this.nudGreen.TabIndex = 39;
			this.nudGreen.ValueChanged += new System.EventHandler(this.HandleRGBChange);
			this.nudGreen.TextChanged += new System.EventHandler(this.HandleTextChanged);
			// 
			// Label2
			// 
			this.Label2.AutoSize = true;
			this.Label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label2.Location = new System.Drawing.Point(123, 301);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(39, 13);
			this.Label2.TabIndex = 44;
			this.Label2.Text = "Green:";
			this.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// nudHue
			// 
			this.nudHue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.nudHue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nudHue.Location = new System.Drawing.Point(69, 272);
			this.nudHue.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.nudHue.Name = "nudHue";
			this.nudHue.Size = new System.Drawing.Size(48, 22);
			this.nudHue.TabIndex = 41;
			this.nudHue.ValueChanged += new System.EventHandler(this.HandleHSVChange);
			this.nudHue.TextChanged += new System.EventHandler(this.HandleTextChanged);
			// 
			// labelAlpha
			// 
			this.labelAlpha.AutoSize = true;
			this.labelAlpha.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelAlpha.Location = new System.Drawing.Point(123, 353);
			this.labelAlpha.Name = "labelAlpha";
			this.labelAlpha.Size = new System.Drawing.Size(37, 13);
			this.labelAlpha.TabIndex = 57;
			this.labelAlpha.Text = "Alpha:";
			this.labelAlpha.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// numericAlpha
			// 
			this.numericAlpha.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericAlpha.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.numericAlpha.Location = new System.Drawing.Point(168, 349);
			this.numericAlpha.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.numericAlpha.Name = "numericAlpha";
			this.numericAlpha.Size = new System.Drawing.Size(48, 22);
			this.numericAlpha.TabIndex = 56;
			this.numericAlpha.ValueChanged += new System.EventHandler(this.numericAlpha_ValueChanged);
			// 
			// panelAlpha
			// 
			this.panelAlpha.BackgroundImage = global::GorgonLibrary.Graphics.Tools.Properties.Resources.alphaPicker;
			this.panelAlpha.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelAlpha.Enabled = false;
			this.panelAlpha.Location = new System.Drawing.Point(318, 8);
			this.panelAlpha.Name = "panelAlpha";
			this.panelAlpha.Size = new System.Drawing.Size(16, 256);
			this.panelAlpha.TabIndex = 58;
			// 
			// pnlSelectedColor
			// 
            this.pnlSelectedColor.BackgroundImage = global::GorgonLibrary.Graphics.Tools.Properties.Resources.buttonBack;
			this.pnlSelectedColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlSelectedColor.Location = new System.Drawing.Point(270, 266);
			this.pnlSelectedColor.Name = "pnlSelectedColor";
			this.pnlSelectedColor.Size = new System.Drawing.Size(84, 32);
			this.pnlSelectedColor.TabIndex = 53;
			this.pnlSelectedColor.Visible = false;
			// 
			// ColorChooser
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(360, 406);
			this.Controls.Add(this.panelAlpha);
			this.Controls.Add(this.labelAlpha);
			this.Controls.Add(this.numericAlpha);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.Label3);
			this.Controls.Add(this.nudSaturation);
			this.Controls.Add(this.Label7);
			this.Controls.Add(this.nudBrightness);
			this.Controls.Add(this.nudRed);
			this.Controls.Add(this.pnlColor);
			this.Controls.Add(this.Label6);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.Label5);
			this.Controls.Add(this.pnlSelectedColor);
			this.Controls.Add(this.pnlBrightness);
			this.Controls.Add(this.nudBlue);
			this.Controls.Add(this.Label4);
			this.Controls.Add(this.nudGreen);
			this.Controls.Add(this.Label2);
			this.Controls.Add(this.nudHue);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ColorChooser";
			this.ShowInTaskbar = false;
			this.Text = "Select Color";
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.ColorChooser1_Paint);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.frmMain_MouseUp);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouse);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HandleMouse);
			this.Load += new System.EventHandler(this.ColorChooser1_Load);
			((System.ComponentModel.ISupportInitialize)(this.nudSaturation)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudBrightness)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudRed)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudBlue)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudGreen)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudHue)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericAlpha)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		internal System.Windows.Forms.Button btnCancel;
		internal System.Windows.Forms.Button btnOK;
		internal System.Windows.Forms.Label Label3;
		internal System.Windows.Forms.NumericUpDown nudSaturation;
		internal System.Windows.Forms.Label Label7;
		internal System.Windows.Forms.NumericUpDown nudBrightness;
		internal System.Windows.Forms.NumericUpDown nudRed;
		internal System.Windows.Forms.Panel pnlColor;
		internal System.Windows.Forms.Label Label6;
		internal System.Windows.Forms.Label Label1;
		internal System.Windows.Forms.Label Label5;
		internal System.Windows.Forms.Panel pnlSelectedColor;
		internal System.Windows.Forms.Panel pnlBrightness;
		internal System.Windows.Forms.NumericUpDown nudBlue;
		internal System.Windows.Forms.Label Label4;
		internal System.Windows.Forms.NumericUpDown nudGreen;
		internal System.Windows.Forms.Label Label2;
		internal System.Windows.Forms.NumericUpDown nudHue;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ColorChooser()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Calculate the coordinates of the three
			// required regions on the form.
			Rectangle SelectedColorRectangle =
			  new Rectangle(pnlSelectedColor.Location, pnlSelectedColor.Size);
			Rectangle BrightnessRectangle =
			  new Rectangle(pnlBrightness.Location, pnlBrightness.Size);
			Rectangle ColorRectangle =
			  new Rectangle(pnlColor.Location, pnlColor.Size);

			// Create the new ColorWheel class, indicating
			// the locations of the color wheel itself, the
			// brightness area, and the position of the selected color.
			myColorWheel = new ColorWheel(
			  ColorRectangle, BrightnessRectangle,
			  SelectedColorRectangle, new Rectangle(panelAlpha.Location, panelAlpha.Size));
		}

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

		internal System.Windows.Forms.Label labelAlpha;
		internal System.Windows.Forms.NumericUpDown numericAlpha;
		internal System.Windows.Forms.Panel panelAlpha;

	}
}
