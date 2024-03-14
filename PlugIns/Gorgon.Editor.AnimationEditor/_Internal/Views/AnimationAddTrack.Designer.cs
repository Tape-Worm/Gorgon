namespace Gorgon.Editor.AnimationEditor;

partial class AnimationAddTrack
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
        this.ListTracks = new System.Windows.Forms.ListBox();
        this.PanelBody.SuspendLayout();
        this.SuspendLayout();
        // 
        // PanelBody
        // 
        this.PanelBody.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.PanelBody.Controls.Add(this.ListTracks);
        this.PanelBody.Size = new System.Drawing.Size(364, 411);
        // 
        // ListTracks
        // 
        this.ListTracks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ListTracks.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.ListTracks.Dock = System.Windows.Forms.DockStyle.Fill;
        this.ListTracks.ForeColor = System.Drawing.Color.White;
        this.ListTracks.FormattingEnabled = true;
        this.ListTracks.HorizontalScrollbar = true;
        this.ListTracks.ItemHeight = 15;
        this.ListTracks.Location = new System.Drawing.Point(0, 0);
        this.ListTracks.Name = "ListTracks";
        this.ListTracks.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
        this.ListTracks.Size = new System.Drawing.Size(364, 411);
        this.ListTracks.Sorted = true;
        this.ListTracks.TabIndex = 0;
        this.ListTracks.SelectedValueChanged += new System.EventHandler(this.ListTracks_SelectedValueChanged);
        this.ListTracks.DoubleClick += new System.EventHandler(this.ListTracks_DoubleClick);
        // 
        // AnimationAddTrack
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Name = "AnimationAddTrack";
        this.Size = new System.Drawing.Size(364, 468);
        this.Text = "Add Track(s)";
        this.PanelBody.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();

    }



    private System.Windows.Forms.ListBox ListTracks;
}
