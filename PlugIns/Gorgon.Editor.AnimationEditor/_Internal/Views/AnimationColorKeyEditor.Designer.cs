namespace Gorgon.Editor.AnimationEditor;

partial class AnimationColorKeyEditor
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
            DataContext?.Unload();
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
        this.PickerColor = new Gorgon.Editor.UI.Controls.ColorPicker();
        this.PickerAlpha = new Gorgon.Editor.UI.Controls.AlphaPicker();
        this.PanelBody.SuspendLayout();
        this.SuspendLayout();
        // 
        // PanelBody
        // 
        this.PanelBody.Controls.Add(this.PickerAlpha);
        this.PanelBody.Controls.Add(this.PickerColor);
        this.PanelBody.Size = new System.Drawing.Size(389, 411);
        // 
        // PickerColor
        // 
        this.PickerColor.AutoSize = true;
        this.PickerColor.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PickerColor.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.PickerColor.Location = new System.Drawing.Point(0, 0);
        this.PickerColor.Margin = new System.Windows.Forms.Padding(0);
        this.PickerColor.Name = "PickerColor";
        this.PickerColor.Size = new System.Drawing.Size(389, 411);
        this.PickerColor.TabIndex = 0;
        this.PickerColor.ColorChanged += new System.EventHandler<Gorgon.Editor.UI.Controls.ColorChangedEventArgs>(this.Picker_ColorChanged);
        // 
        // PickerAlpha
        // 
        this.PickerAlpha.AlphaValue = 0;
        this.PickerAlpha.AutoSize = true;
        this.PickerAlpha.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.PickerAlpha.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PickerAlpha.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.PickerAlpha.ForeColor = System.Drawing.Color.White;
        this.PickerAlpha.Location = new System.Drawing.Point(0, 0);
        this.PickerAlpha.Name = "PickerAlpha";
        this.PickerAlpha.Size = new System.Drawing.Size(389, 411);
        this.PickerAlpha.TabIndex = 1;
        this.PickerAlpha.AlphaValueChanged += new System.EventHandler(this.PickerAlpha_AlphaValueChanged);
        // 
        // AnimationColorKeyEditor
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Name = "AnimationColorKeyEditor";
        this.Size = new System.Drawing.Size(389, 468);
        this.Text = "Key Color";
        this.PanelBody.ResumeLayout(false);
        this.PanelBody.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private UI.Controls.ColorPicker PickerColor;
    private UI.Controls.AlphaPicker PickerAlpha;
}
