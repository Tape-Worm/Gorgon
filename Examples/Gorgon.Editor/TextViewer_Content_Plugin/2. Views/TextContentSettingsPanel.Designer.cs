namespace Gorgon.Examples;

partial class TextContentSettingsPanel
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
            ViewModel?.Unload();
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
        this.TipSettings = new System.Windows.Forms.ToolTip(this.components);
        this.label1 = new System.Windows.Forms.Label();
        this.RadioArial = new System.Windows.Forms.RadioButton();
        this.RadioTimesNewRoman = new System.Windows.Forms.RadioButton();
        this.RadioPapyrus = new System.Windows.Forms.RadioButton();
        this.PanelBody.SuspendLayout();
        this.SuspendLayout();
        // 
        // PanelBody
        // 
        this.PanelBody.Controls.Add(this.RadioPapyrus);
        this.PanelBody.Controls.Add(this.RadioTimesNewRoman);
        this.PanelBody.Controls.Add(this.RadioArial);
        this.PanelBody.Controls.Add(this.label1);
        // 
        // TipSettings
        // 
        this.TipSettings.AutoPopDelay = 10000;
        this.TipSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TipSettings.ForeColor = System.Drawing.Color.White;
        this.TipSettings.InitialDelay = 500;
        this.TipSettings.ReshowDelay = 100;
        this.TipSettings.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
        this.TipSettings.ToolTipTitle = "Info";
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Dock = System.Windows.Forms.DockStyle.Top;
        this.label1.Location = new System.Drawing.Point(0, 0);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(75, 15);
        this.label1.TabIndex = 0;
        this.label1.Text = "Default Font:";
        // 
        // RadioArial
        // 
        this.RadioArial.AutoSize = true;
        this.RadioArial.Dock = System.Windows.Forms.DockStyle.Top;
        this.RadioArial.Location = new System.Drawing.Point(0, 15);
        this.RadioArial.Name = "RadioArial";
        this.RadioArial.Size = new System.Drawing.Size(600, 19);
        this.RadioArial.TabIndex = 1;
        this.RadioArial.TabStop = true;
        this.RadioArial.Text = "Arial";
        this.RadioArial.UseVisualStyleBackColor = true;
        this.RadioArial.CheckedChanged += new System.EventHandler(this.Radio_CheckedChanged);
        // 
        // RadioTimesNewRoman
        // 
        this.RadioTimesNewRoman.AutoSize = true;
        this.RadioTimesNewRoman.Dock = System.Windows.Forms.DockStyle.Top;
        this.RadioTimesNewRoman.Location = new System.Drawing.Point(0, 34);
        this.RadioTimesNewRoman.Name = "RadioTimesNewRoman";
        this.RadioTimesNewRoman.Size = new System.Drawing.Size(600, 19);
        this.RadioTimesNewRoman.TabIndex = 2;
        this.RadioTimesNewRoman.TabStop = true;
        this.RadioTimesNewRoman.Text = "Times New Roman";
        this.RadioTimesNewRoman.UseVisualStyleBackColor = true;
        this.RadioTimesNewRoman.Click += new System.EventHandler(this.Radio_CheckedChanged);
        // 
        // RadioPapyrus
        // 
        this.RadioPapyrus.AutoSize = true;
        this.RadioPapyrus.Dock = System.Windows.Forms.DockStyle.Top;
        this.RadioPapyrus.Location = new System.Drawing.Point(0, 53);
        this.RadioPapyrus.Name = "RadioPapyrus";
        this.RadioPapyrus.Size = new System.Drawing.Size(600, 19);
        this.RadioPapyrus.TabIndex = 3;
        this.RadioPapyrus.TabStop = true;
        this.RadioPapyrus.Text = "Papyrus";
        this.RadioPapyrus.UseVisualStyleBackColor = true;
        this.RadioPapyrus.Click += new System.EventHandler(this.Radio_CheckedChanged);
        // 
        // TextContentSettingsPanel
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Name = "TextContentSettingsPanel";
        this.Text = "Example Text Content Editor Settings";
        this.PanelBody.ResumeLayout(false);
        this.PanelBody.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.ToolTip TipSettings;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.RadioButton RadioPapyrus;
    private System.Windows.Forms.RadioButton RadioTimesNewRoman;
    private System.Windows.Forms.RadioButton RadioArial;
}
