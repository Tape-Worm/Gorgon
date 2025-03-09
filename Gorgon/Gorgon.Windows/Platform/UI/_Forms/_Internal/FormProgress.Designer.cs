namespace Gorgon.UI;

partial class FormProgress
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



    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        var resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProgress));
        Progress = new GorgonProgressPanel();
        SuspendLayout();
        // 
        // Progress
        // 
        Progress.AllowCancellation = true;
        Progress.AutoSize = true;
        Progress.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        Progress.BackColor = System.Drawing.Color.White;
        Progress.CurrentValue = 0F;
        Progress.Dock = System.Windows.Forms.DockStyle.Fill;
        Progress.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        Progress.ForeColor = System.Drawing.Color.Black;
        Progress.Location = new System.Drawing.Point(1, 1);
        Progress.Margin = new System.Windows.Forms.Padding(0);
        Progress.MaximumSize = new System.Drawing.Size(512, 128);
        Progress.Name = "Progress";
        Progress.ProgressMessage = "Progress";
        Progress.ProgressMessageFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        Progress.ProgressTitle = "Title";
        Progress.Size = new System.Drawing.Size(507, 126);
        Progress.TabIndex = 0;
        Progress.Resize += Progress_Resize;
        // 
        // FormProgress
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        AutoSize = true;
        AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        BackColor = System.Drawing.Color.Black;
        ClientSize = new System.Drawing.Size(509, 128);
        ControlBox = false;
        Controls.Add(Progress);
        ForeColor = System.Drawing.Color.White;
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
        KeyPreview = true;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "FormProgress";
        Padding = new System.Windows.Forms.Padding(1);
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "FormProgress";
        ResumeLayout(false);
        PerformLayout();
    }



#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// The progress panel embedded in the form.
    /// </summary>
    public GorgonProgressPanel Progress;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
