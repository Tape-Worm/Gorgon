namespace Gorgon.Editor.SpriteEditor;

partial class SpriteColor
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
            UnassignEvents();
            ViewModel?.Unload();
        }

        base.Dispose(disposing);
    }



    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        Picker = new UI.Controls.ColorPicker();
        PanelBody.SuspendLayout();
        SuspendLayout();
        // 
        // PanelBody
        // 
        PanelBody.Controls.Add(Picker);
        PanelBody.Size = new Size(360, 288);
        // 
        // Picker
        // 
        Picker.AutoSize = true;
        Picker.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Picker.Dock = DockStyle.Fill;
        Picker.Font = new Font("Segoe UI", 9F);
        Picker.Location = new Point(0, 0);
        Picker.Name = "Picker";
        Picker.Size = new Size(360, 288);
        Picker.TabIndex = 0;
        Picker.ColorChanged += Picker_ColorChanged;
        // 
        // SpriteColor
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Name = "SpriteColor";
        Size = new Size(360, 345);
        Text = "Sprite Color";
        PanelBody.ResumeLayout(false);
        PanelBody.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private UI.Controls.ColorPicker Picker;
}
