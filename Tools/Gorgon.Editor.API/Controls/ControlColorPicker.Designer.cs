namespace GorgonLibrary.Editor
{
	partial class ControlColorPicker
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlColorPicker));
            this.panelColor = new Fetze.WinFormsColor.ColorPanel();
            this.sliderColor = new Fetze.WinFormsColor.ColorSlider();
            this.sliderAlpha = new Fetze.WinFormsColor.ColorSlider();
            this.labelColorInfo = new System.Windows.Forms.Label();
            this.buttonExpando = new System.Windows.Forms.Button();
            this.tipControls = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // panelColor
            // 
            this.panelColor.BottomLeftColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.panelColor.BottomRightColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.panelColor.Location = new System.Drawing.Point(4, 4);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(150, 150);
            this.panelColor.TabIndex = 0;
            this.panelColor.TopLeftColor = System.Drawing.Color.White;
            this.panelColor.TopRightColor = System.Drawing.Color.White;
            this.panelColor.ValuePercentual = ((System.Drawing.PointF)(resources.GetObject("panelColor.ValuePercentual")));
            this.panelColor.ValueChanged += new System.EventHandler(this.sliderColor_ValueChanged);
            // 
            // sliderColor
            // 
            this.sliderColor.Location = new System.Drawing.Point(160, 4);
            this.sliderColor.Name = "sliderColor";
            this.sliderColor.PickerSize = 7;
            this.sliderColor.Size = new System.Drawing.Size(32, 150);
            this.sliderColor.TabIndex = 1;
            this.sliderColor.ValueChanged += new System.EventHandler(this.sliderColor_ValueChanged);
            // 
            // sliderAlpha
            // 
            this.sliderAlpha.Location = new System.Drawing.Point(198, 4);
            this.sliderAlpha.Maximum = System.Drawing.Color.White;
            this.sliderAlpha.Minimum = System.Drawing.Color.Empty;
            this.sliderAlpha.Name = "sliderAlpha";
            this.sliderAlpha.PickerSize = 7;
            this.sliderAlpha.Size = new System.Drawing.Size(32, 150);
            this.sliderAlpha.TabIndex = 2;
            this.sliderAlpha.ValueChanged += new System.EventHandler(this.sliderColor_ValueChanged);
            // 
            // labelColorInfo
            // 
            this.labelColorInfo.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelColorInfo.ForeColor = System.Drawing.Color.White;
            this.labelColorInfo.Location = new System.Drawing.Point(4, 161);
            this.labelColorInfo.Name = "labelColorInfo";
            this.labelColorInfo.Size = new System.Drawing.Size(150, 18);
            this.labelColorInfo.TabIndex = 3;
            // 
            // buttonExpando
            // 
            this.buttonExpando.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.buttonExpando.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.buttonExpando.FlatAppearance.MouseDownBackColor = System.Drawing.Color.RoyalBlue;
            this.buttonExpando.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.buttonExpando.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonExpando.ForeColor = System.Drawing.Color.White;
            this.buttonExpando.Image = global::GorgonLibrary.Editor.Properties.APIResources.Expando;
            this.buttonExpando.Location = new System.Drawing.Point(177, 156);
            this.buttonExpando.Name = "buttonExpando";
            this.buttonExpando.Size = new System.Drawing.Size(38, 23);
            this.buttonExpando.TabIndex = 4;
            this.tipControls.SetToolTip(this.buttonExpando, "Click for precise color control.");
            this.buttonExpando.UseVisualStyleBackColor = false;
            this.buttonExpando.Click += new System.EventHandler(this.buttonExpando_Click);
            // 
            // controlColorPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.Controls.Add(this.buttonExpando);
            this.Controls.Add(this.labelColorInfo);
            this.Controls.Add(this.sliderAlpha);
            this.Controls.Add(this.sliderColor);
            this.Controls.Add(this.panelColor);
            this.Name = "controlColorPicker";
            this.Size = new System.Drawing.Size(236, 183);
            this.ResumeLayout(false);

		}

		#endregion

		private Fetze.WinFormsColor.ColorPanel panelColor;
		private Fetze.WinFormsColor.ColorSlider sliderColor;
		private Fetze.WinFormsColor.ColorSlider sliderAlpha;
		private System.Windows.Forms.Label labelColorInfo;
		private System.Windows.Forms.Button buttonExpando;
		private System.Windows.Forms.ToolTip tipControls;
	}
}
