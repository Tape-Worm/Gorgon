namespace Gorgon.Examples;

partial class TextContentView
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
            UnregisterChildPanel(typeof(TextColor).FullName);
            _formRibbon?.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.TextColorPicker = new Gorgon.Examples.TextColorView();
        this.HostPanel.SuspendLayout();
        this.HostPanelControls.SuspendLayout();
        this.SuspendLayout();
        // 
        // StatusPanel
        // 
        this.StatusPanel.Size = new System.Drawing.Size(653, 24);
        this.StatusPanel.Visible = false;
        // 
        // PresentationPanel
        // 
        this.PresentationPanel.Location = new System.Drawing.Point(0, 21);
        this.PresentationPanel.Size = new System.Drawing.Size(653, 684);
        // 
        // HostPanel
        // 
        this.HostPanel.Location = new System.Drawing.Point(653, 21);
        this.HostPanel.Size = new System.Drawing.Size(368, 708);
        // 
        // HostPanelControls
        // 
        this.HostPanelControls.Controls.Add(this.TextColorPicker);
        this.HostPanelControls.Size = new System.Drawing.Size(367, 707);
        // 
        // TextColorPicker
        // 
        this.TextColorPicker.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.TextColorPicker.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.TextColorPicker.ForeColor = System.Drawing.Color.White;
        this.TextColorPicker.Location = new System.Drawing.Point(0, 0);
        this.TextColorPicker.Name = "TextColorPicker";
        this.TextColorPicker.Size = new System.Drawing.Size(364, 468);
        this.TextColorPicker.TabIndex = 0;
        this.TextColorPicker.Text = "Text Color";
        // 
        // TextContentView
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Name = "TextContentView";
        this.HostPanel.ResumeLayout(false);
        this.HostPanel.PerformLayout();
        this.HostPanelControls.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();

    }



    private TextColorView TextColorPicker;
}
