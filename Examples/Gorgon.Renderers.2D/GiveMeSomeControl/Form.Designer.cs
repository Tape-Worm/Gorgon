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
        this.SplitViews = new System.Windows.Forms.SplitContainer();
        this.GroupControl1 = new System.Windows.Forms.Panel();
        this.TrackSpeed = new System.Windows.Forms.TrackBar();
        this.ButtonAnimation = new System.Windows.Forms.Button();
        this.GroupControl2 = new System.Windows.Forms.Panel();
        ((System.ComponentModel.ISupportInitialize)(this.SplitViews)).BeginInit();
        this.SplitViews.Panel1.SuspendLayout();
        this.SplitViews.Panel2.SuspendLayout();
        this.SplitViews.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.TrackSpeed)).BeginInit();
        this.SuspendLayout();
        // 
        // SplitViews
        // 
        this.SplitViews.Dock = System.Windows.Forms.DockStyle.Fill;
        this.SplitViews.Location = new System.Drawing.Point(0, 0);
        this.SplitViews.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        this.SplitViews.Name = "SplitViews";
        // 
        // SplitViews.Panel1
        // 
        this.SplitViews.Panel1.Controls.Add(this.GroupControl1);
        this.SplitViews.Panel1MinSize = 160;
        // 
        // SplitViews.Panel2
        // 
        this.SplitViews.Panel2.Controls.Add(this.TrackSpeed);
        this.SplitViews.Panel2.Controls.Add(this.ButtonAnimation);
        this.SplitViews.Panel2.Controls.Add(this.GroupControl2);
        this.SplitViews.Panel2MinSize = 140;
        this.SplitViews.Size = new System.Drawing.Size(1264, 761);
        this.SplitViews.SplitterDistance = 550;
        this.SplitViews.SplitterWidth = 6;
        this.SplitViews.TabIndex = 4;
        this.SplitViews.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.SplitViews_SplitterMoved);
        // 
        // GroupControl1
        // 
        this.GroupControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.GroupControl1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.GroupControl1.Location = new System.Drawing.Point(0, 0);
        this.GroupControl1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        this.GroupControl1.Name = "GroupControl1";
        this.GroupControl1.Size = new System.Drawing.Size(550, 761);
        this.GroupControl1.TabIndex = 1;
        this.GroupControl1.Text = "Control 1";
        // 
        // TrackSpeed
        // 
        this.TrackSpeed.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.TrackSpeed.LargeChange = 1;
        this.TrackSpeed.Location = new System.Drawing.Point(0, 705);
        this.TrackSpeed.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        this.TrackSpeed.Name = "TrackSpeed";
        this.TrackSpeed.Size = new System.Drawing.Size(708, 56);
        this.TrackSpeed.TabIndex = 5;
        this.TrackSpeed.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
        this.TrackSpeed.Value = 5;
        // 
        // ButtonAnimation
        // 
        this.ButtonAnimation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonAnimation.Location = new System.Drawing.Point(3, 639);
        this.ButtonAnimation.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        this.ButtonAnimation.Name = "ButtonAnimation";
        this.ButtonAnimation.Size = new System.Drawing.Size(696, 36);
        this.ButtonAnimation.TabIndex = 4;
        this.ButtonAnimation.Text = "Start/Stop Animation";
        this.ButtonAnimation.UseVisualStyleBackColor = true;
        this.ButtonAnimation.Click += new System.EventHandler(this.ButtonAnimation_Click);
        // 
        // GroupControl2
        // 
        this.GroupControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.GroupControl2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.GroupControl2.Location = new System.Drawing.Point(0, 0);
        this.GroupControl2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        this.GroupControl2.Name = "GroupControl2";
        this.GroupControl2.Size = new System.Drawing.Size(702, 628);
        this.GroupControl2.TabIndex = 2;
        this.GroupControl2.Text = "Control 2";
        // 
        // Form
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(1264, 761);
        this.Controls.Add(this.SplitViews);
        this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.KeyPreview = true;
        this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        this.Name = "Form";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Give Me Some Control";
        this.SplitViews.Panel1.ResumeLayout(false);
        this.SplitViews.Panel2.ResumeLayout(false);
        this.SplitViews.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.SplitViews)).EndInit();
        this.SplitViews.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.TrackSpeed)).EndInit();
        this.ResumeLayout(false);

        }
    

        private System.Windows.Forms.SplitContainer SplitViews;
        private System.Windows.Forms.Panel GroupControl1;
        private System.Windows.Forms.TrackBar TrackSpeed;
        private System.Windows.Forms.Button ButtonAnimation;
        private System.Windows.Forms.Panel GroupControl2;
}

