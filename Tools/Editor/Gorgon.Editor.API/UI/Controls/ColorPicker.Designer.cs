namespace Gorgon.Editor.UI.Controls
{
    partial class ColorPicker
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
			this.Picker = new Fetze.WinFormsColor.ColorPickerPanel();
			this.SuspendLayout();
			// 
			// Picker
			// 
			this.Picker.AlphaEnabled = true;
			this.Picker.AutoSize = true;
			this.Picker.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Picker.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Picker.Location = new System.Drawing.Point(0, 0);
			this.Picker.MinimumSize = new System.Drawing.Size(360, 288);
			this.Picker.Name = "Picker";
			this.Picker.OldColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Picker.PrimaryAttribute = Fetze.WinFormsColor.ColorPickerPanel.PrimaryAttrib.Hue;
			this.Picker.SelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Picker.Size = new System.Drawing.Size(418, 306);
			this.Picker.TabIndex = 0;
			this.Picker.ColorChanged += new System.EventHandler(this.Picker_ColorChanged);
			this.Picker.OldColorChanged += new System.EventHandler(this.Picker_ColorChanged);
			// 
			// ColorPicker
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.Picker);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Name = "ColorPicker";
			this.Size = new System.Drawing.Size(418, 306);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private Fetze.WinFormsColor.ColorPickerPanel Picker;
    }
}
