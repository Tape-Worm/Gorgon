namespace Gorgon.Editor.UI.Views;

partial class SettingsBaseControl
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
        this.PanelCaption = new System.Windows.Forms.Panel();
        this.LabelCaption = new System.Windows.Forms.Label();
        this.PanelBody = new System.Windows.Forms.Panel();
        this.PanelCaption.SuspendLayout();
        this.SuspendLayout();
        // 
        // PanelCaption
        // 
        this.PanelCaption.AutoSize = true;
        this.PanelCaption.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.PanelCaption.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.PanelCaption.Controls.Add(this.LabelCaption);
        this.PanelCaption.Dock = System.Windows.Forms.DockStyle.Top;
        this.PanelCaption.Location = new System.Drawing.Point(0, 0);
        this.PanelCaption.Name = "PanelCaption";
        this.PanelCaption.Size = new System.Drawing.Size(600, 23);
        this.PanelCaption.TabIndex = 0;
        // 
        // LabelCaption
        // 
        this.LabelCaption.AutoSize = true;
        this.LabelCaption.Font = new System.Drawing.Font("Segoe UI Semibold", 11F);
        this.LabelCaption.Location = new System.Drawing.Point(0, 0);
        this.LabelCaption.Margin = new System.Windows.Forms.Padding(3);
        this.LabelCaption.Name = "LabelCaption";
        this.LabelCaption.Size = new System.Drawing.Size(99, 20);
        this.LabelCaption.TabIndex = 1;
        this.LabelCaption.Text = "Caption Here";
        // 
        // PanelBody
        // 
        this.PanelBody.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelBody.Location = new System.Drawing.Point(0, 23);
        this.PanelBody.Name = "PanelBody";
        this.PanelBody.Size = new System.Drawing.Size(600, 445);
        this.PanelBody.TabIndex = 1;
        // 
        // SettingsBaseControl
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.PanelBody);
        this.Controls.Add(this.PanelCaption);
        this.ForeColor = System.Drawing.Color.White;
        this.Name = "SettingsBaseControl";
        this.PanelCaption.ResumeLayout(false);
        this.PanelCaption.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Panel PanelCaption;
    private System.Windows.Forms.Label LabelCaption;        
}
