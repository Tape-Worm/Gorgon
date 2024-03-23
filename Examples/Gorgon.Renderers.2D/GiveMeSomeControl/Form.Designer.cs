namespace Gorgon.Examples;

partial class Form
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form));
        SplitViews = new SplitContainer();
        GroupControl1 = new Panel();
        TrackSpeed = new TrackBar();
        ButtonAnimation = new Button();
        GroupControl2 = new Panel();
        ((System.ComponentModel.ISupportInitialize)SplitViews).BeginInit();
        SplitViews.Panel1.SuspendLayout();
        SplitViews.Panel2.SuspendLayout();
        SplitViews.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)TrackSpeed).BeginInit();
        SuspendLayout();
        // 
        // SplitViews
        // 
        SplitViews.Dock = DockStyle.Fill;
        SplitViews.Location = new Point(0, 0);
        SplitViews.Name = "SplitViews";
        // 
        // SplitViews.Panel1
        // 
        SplitViews.Panel1.Controls.Add(GroupControl1);
        SplitViews.Panel1MinSize = 160;
        // 
        // SplitViews.Panel2
        // 
        SplitViews.Panel2.Controls.Add(TrackSpeed);
        SplitViews.Panel2.Controls.Add(ButtonAnimation);
        SplitViews.Panel2.Controls.Add(GroupControl2);
        SplitViews.Panel2MinSize = 140;
        SplitViews.Size = new Size(1106, 571);
        SplitViews.SplitterDistance = 481;
        SplitViews.SplitterWidth = 5;
        SplitViews.TabIndex = 4;
        SplitViews.SplitterMoved += SplitViews_SplitterMoved;
        // 
        // GroupControl1
        // 
        GroupControl1.BorderStyle = BorderStyle.FixedSingle;
        GroupControl1.Dock = DockStyle.Fill;
        GroupControl1.Location = new Point(0, 0);
        GroupControl1.Name = "GroupControl1";
        GroupControl1.Size = new Size(481, 571);
        GroupControl1.TabIndex = 1;
        GroupControl1.Text = "Control 1";
        // 
        // TrackSpeed
        // 
        TrackSpeed.Dock = DockStyle.Bottom;
        TrackSpeed.LargeChange = 1;
        TrackSpeed.Location = new Point(0, 526);
        TrackSpeed.Name = "TrackSpeed";
        TrackSpeed.Size = new Size(620, 45);
        TrackSpeed.TabIndex = 5;
        TrackSpeed.TickStyle = TickStyle.TopLeft;
        TrackSpeed.Value = 5;
        // 
        // ButtonAnimation
        // 
        ButtonAnimation.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        ButtonAnimation.Location = new Point(3, 479);
        ButtonAnimation.Name = "ButtonAnimation";
        ButtonAnimation.Size = new Size(607, 27);
        ButtonAnimation.TabIndex = 4;
        ButtonAnimation.Text = "Start/Stop Animation";
        ButtonAnimation.UseVisualStyleBackColor = true;
        ButtonAnimation.Click += ButtonAnimation_Click;
        // 
        // GroupControl2
        // 
        GroupControl2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        GroupControl2.BorderStyle = BorderStyle.FixedSingle;
        GroupControl2.Location = new Point(0, 0);
        GroupControl2.Name = "GroupControl2";
        GroupControl2.Size = new Size(613, 472);
        GroupControl2.TabIndex = 2;
        GroupControl2.Text = "Control 2";
        // 
        // Form
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;
        ClientSize = new Size(1106, 571);
        Controls.Add(SplitViews);
        Font = new Font("Segoe UI", 9F);
        Icon = (Icon)resources.GetObject("$this.Icon");
        KeyPreview = true;
        Name = "Form";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Give Me Some Control";
        SplitViews.Panel1.ResumeLayout(false);
        SplitViews.Panel2.ResumeLayout(false);
        SplitViews.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)SplitViews).EndInit();
        SplitViews.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)TrackSpeed).EndInit();
        ResumeLayout(false);
    }

    private System.Windows.Forms.SplitContainer SplitViews;
        private System.Windows.Forms.Panel GroupControl1;
        private System.Windows.Forms.TrackBar TrackSpeed;
        private System.Windows.Forms.Button ButtonAnimation;
        private System.Windows.Forms.Panel GroupControl2;
}

