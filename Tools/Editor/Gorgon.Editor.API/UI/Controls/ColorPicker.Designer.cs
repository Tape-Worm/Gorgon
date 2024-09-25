namespace Gorgon.Editor.UI.Controls;

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
            ColorChangedEvent = null;
        }

        base.Dispose(disposing);
    }



    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        Picker = new Fetze.WinFormsColor.ColorPickerPanel();
        SuspendLayout();
        // 
        // Picker
        // 
        Picker.AlphaEnabled = true;
        Picker.AutoSize = true;
        Picker.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Picker.Dock = DockStyle.Fill;
        Picker.Location = new Point(0, 0);
        Picker.MinimumSize = new Size(360, 288);
        Picker.Name = "Picker";
        Picker.OldColor = Color.FromArgb(255, 0, 0);
        Picker.PrimaryAttribute = Fetze.WinFormsColor.ColorPickerPanel.PrimaryAttrib.Hue;
        Picker.SelectedColor = Color.FromArgb(255, 0, 0);
        Picker.Size = new Size(360, 288);
        Picker.TabIndex = 0;
        Picker.ColorChanged += Picker_ColorChanged;
        Picker.OldColorChanged += Picker_ColorChanged;
        // 
        // ColorPicker
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Controls.Add(Picker);
        Name = "ColorPicker";
        Size = new Size(360, 288);
        ResumeLayout(false);
        PerformLayout();
    }

    private Fetze.WinFormsColor.ColorPickerPanel Picker;
}
