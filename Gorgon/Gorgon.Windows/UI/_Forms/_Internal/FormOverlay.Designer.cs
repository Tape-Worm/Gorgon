
namespace Gorgon.UI;

partial class FormOverlay
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormOverlay));
        this.SuspendLayout();
        // 
        // FormOverlay
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.BackColor = System.Drawing.Color.Black;
        this.ClientSize = new System.Drawing.Size(904, 670);
        this.ControlBox = false;
        this.DoubleBuffered = true;
        
        this.ForeColor = System.Drawing.Color.Black;
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "FormOverlay";
        this.ShowIcon = false;
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        this.Text = "OverlayForm";
        this.ResumeLayout(false);

    }

    #endregion
}
