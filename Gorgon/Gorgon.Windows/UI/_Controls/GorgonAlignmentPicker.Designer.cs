namespace Gorgon.UI;

partial class GorgonAlignmentPicker
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
            AlignmentChanged = null;
        }

        base.Dispose(disposing);
    }



    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.panelAnchor = new System.Windows.Forms.Panel();
        this.radioBottomRight = new System.Windows.Forms.RadioButton();
        this.radioBottomCenter = new System.Windows.Forms.RadioButton();
        this.radioBottomLeft = new System.Windows.Forms.RadioButton();
        this.radioMiddleRight = new System.Windows.Forms.RadioButton();
        this.radioCenter = new System.Windows.Forms.RadioButton();
        this.radioMiddleLeft = new System.Windows.Forms.RadioButton();
        this.radioTopRight = new System.Windows.Forms.RadioButton();
        this.radioTopCenter = new System.Windows.Forms.RadioButton();
        this.radioTopLeft = new System.Windows.Forms.RadioButton();
        this.panelAnchor.SuspendLayout();
        this.SuspendLayout();
        // 
        // panelAnchor
        // 
        this.panelAnchor.Controls.Add(this.radioBottomRight);
        this.panelAnchor.Controls.Add(this.radioBottomCenter);
        this.panelAnchor.Controls.Add(this.radioBottomLeft);
        this.panelAnchor.Controls.Add(this.radioMiddleRight);
        this.panelAnchor.Controls.Add(this.radioCenter);
        this.panelAnchor.Controls.Add(this.radioMiddleLeft);
        this.panelAnchor.Controls.Add(this.radioTopRight);
        this.panelAnchor.Controls.Add(this.radioTopCenter);
        this.panelAnchor.Controls.Add(this.radioTopLeft);
        this.panelAnchor.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.panelAnchor.Location = new System.Drawing.Point(0, 0);
        this.panelAnchor.Name = "panelAnchor";
        this.panelAnchor.Size = new System.Drawing.Size(105, 105);
        this.panelAnchor.TabIndex = 13;
        // 
        // radioBottomRight
        // 
        this.radioBottomRight.Appearance = System.Windows.Forms.Appearance.Button;
        this.radioBottomRight.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
        this.radioBottomRight.FlatAppearance.BorderSize = 0;
        this.radioBottomRight.FlatAppearance.CheckedBackColor = System.Drawing.Color.SteelBlue;
        this.radioBottomRight.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.radioBottomRight.FlatAppearance.MouseOverBackColor = System.Drawing.Color.CornflowerBlue;
        this.radioBottomRight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.radioBottomRight.Image = global::Gorgon.Windows.Properties.Resources.arrow_se_24x24;
        this.radioBottomRight.Location = new System.Drawing.Point(73, 71);
        this.radioBottomRight.Margin = new System.Windows.Forms.Padding(0);
        this.radioBottomRight.Name = "radioBottomRight";
        this.radioBottomRight.Size = new System.Drawing.Size(29, 26);
        this.radioBottomRight.TabIndex = 8;
        this.radioBottomRight.UseVisualStyleBackColor = false;
        this.radioBottomRight.Click += new System.EventHandler(this.Radio_Click);
        // 
        // radioBottomCenter
        // 
        this.radioBottomCenter.Appearance = System.Windows.Forms.Appearance.Button;
        this.radioBottomCenter.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
        this.radioBottomCenter.FlatAppearance.BorderSize = 0;
        this.radioBottomCenter.FlatAppearance.CheckedBackColor = System.Drawing.Color.SteelBlue;
        this.radioBottomCenter.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.radioBottomCenter.FlatAppearance.MouseOverBackColor = System.Drawing.Color.CornflowerBlue;
        this.radioBottomCenter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.radioBottomCenter.Image = global::Gorgon.Windows.Properties.Resources.arrow_s_24x24;
        this.radioBottomCenter.Location = new System.Drawing.Point(38, 71);
        this.radioBottomCenter.Margin = new System.Windows.Forms.Padding(0);
        this.radioBottomCenter.Name = "radioBottomCenter";
        this.radioBottomCenter.Size = new System.Drawing.Size(29, 26);
        this.radioBottomCenter.TabIndex = 7;
        this.radioBottomCenter.UseVisualStyleBackColor = false;
        this.radioBottomCenter.Click += new System.EventHandler(this.Radio_Click);
        // 
        // radioBottomLeft
        // 
        this.radioBottomLeft.Appearance = System.Windows.Forms.Appearance.Button;
        this.radioBottomLeft.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
        this.radioBottomLeft.FlatAppearance.BorderSize = 0;
        this.radioBottomLeft.FlatAppearance.CheckedBackColor = System.Drawing.Color.SteelBlue;
        this.radioBottomLeft.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.radioBottomLeft.FlatAppearance.MouseOverBackColor = System.Drawing.Color.CornflowerBlue;
        this.radioBottomLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.radioBottomLeft.Image = global::Gorgon.Windows.Properties.Resources.arrow_sw_24x24;
        this.radioBottomLeft.Location = new System.Drawing.Point(3, 71);
        this.radioBottomLeft.Margin = new System.Windows.Forms.Padding(0);
        this.radioBottomLeft.Name = "radioBottomLeft";
        this.radioBottomLeft.Size = new System.Drawing.Size(29, 26);
        this.radioBottomLeft.TabIndex = 6;
        this.radioBottomLeft.UseVisualStyleBackColor = false;
        this.radioBottomLeft.Click += new System.EventHandler(this.Radio_Click);
        // 
        // radioMiddleRight
        // 
        this.radioMiddleRight.Appearance = System.Windows.Forms.Appearance.Button;
        this.radioMiddleRight.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
        this.radioMiddleRight.FlatAppearance.BorderSize = 0;
        this.radioMiddleRight.FlatAppearance.CheckedBackColor = System.Drawing.Color.SteelBlue;
        this.radioMiddleRight.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.radioMiddleRight.FlatAppearance.MouseOverBackColor = System.Drawing.Color.CornflowerBlue;
        this.radioMiddleRight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.radioMiddleRight.Image = global::Gorgon.Windows.Properties.Resources.arrow_e_24x24;
        this.radioMiddleRight.Location = new System.Drawing.Point(73, 39);
        this.radioMiddleRight.Margin = new System.Windows.Forms.Padding(0);
        this.radioMiddleRight.Name = "radioMiddleRight";
        this.radioMiddleRight.Size = new System.Drawing.Size(29, 26);
        this.radioMiddleRight.TabIndex = 5;
        this.radioMiddleRight.UseVisualStyleBackColor = false;
        this.radioMiddleRight.Click += new System.EventHandler(this.Radio_Click);
        // 
        // radioCenter
        // 
        this.radioCenter.Appearance = System.Windows.Forms.Appearance.Button;
        this.radioCenter.Checked = true;
        this.radioCenter.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
        this.radioCenter.FlatAppearance.BorderSize = 0;
        this.radioCenter.FlatAppearance.CheckedBackColor = System.Drawing.Color.SteelBlue;
        this.radioCenter.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.radioCenter.FlatAppearance.MouseOverBackColor = System.Drawing.Color.CornflowerBlue;
        this.radioCenter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.radioCenter.Image = global::Gorgon.Windows.Properties.Resources.center_24x24;
        this.radioCenter.Location = new System.Drawing.Point(38, 39);
        this.radioCenter.Margin = new System.Windows.Forms.Padding(0);
        this.radioCenter.Name = "radioCenter";
        this.radioCenter.Size = new System.Drawing.Size(29, 26);
        this.radioCenter.TabIndex = 4;
        this.radioCenter.TabStop = true;
        this.radioCenter.UseVisualStyleBackColor = false;
        this.radioCenter.Click += new System.EventHandler(this.Radio_Click);
        // 
        // radioMiddleLeft
        // 
        this.radioMiddleLeft.Appearance = System.Windows.Forms.Appearance.Button;
        this.radioMiddleLeft.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
        this.radioMiddleLeft.FlatAppearance.BorderSize = 0;
        this.radioMiddleLeft.FlatAppearance.CheckedBackColor = System.Drawing.Color.SteelBlue;
        this.radioMiddleLeft.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.radioMiddleLeft.FlatAppearance.MouseOverBackColor = System.Drawing.Color.CornflowerBlue;
        this.radioMiddleLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.radioMiddleLeft.Image = global::Gorgon.Windows.Properties.Resources.arrow_w_24x24;
        this.radioMiddleLeft.Location = new System.Drawing.Point(3, 39);
        this.radioMiddleLeft.Margin = new System.Windows.Forms.Padding(0);
        this.radioMiddleLeft.Name = "radioMiddleLeft";
        this.radioMiddleLeft.Size = new System.Drawing.Size(29, 26);
        this.radioMiddleLeft.TabIndex = 3;
        this.radioMiddleLeft.UseVisualStyleBackColor = false;
        this.radioMiddleLeft.Click += new System.EventHandler(this.Radio_Click);
        // 
        // radioTopRight
        // 
        this.radioTopRight.Appearance = System.Windows.Forms.Appearance.Button;
        this.radioTopRight.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
        this.radioTopRight.FlatAppearance.BorderSize = 0;
        this.radioTopRight.FlatAppearance.CheckedBackColor = System.Drawing.Color.SteelBlue;
        this.radioTopRight.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.radioTopRight.FlatAppearance.MouseOverBackColor = System.Drawing.Color.CornflowerBlue;
        this.radioTopRight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.radioTopRight.Image = global::Gorgon.Windows.Properties.Resources.arrow_ne_24x24;
        this.radioTopRight.Location = new System.Drawing.Point(73, 7);
        this.radioTopRight.Margin = new System.Windows.Forms.Padding(0);
        this.radioTopRight.Name = "radioTopRight";
        this.radioTopRight.Size = new System.Drawing.Size(29, 26);
        this.radioTopRight.TabIndex = 2;
        this.radioTopRight.UseVisualStyleBackColor = false;
        this.radioTopRight.Click += new System.EventHandler(this.Radio_Click);
        // 
        // radioTopCenter
        // 
        this.radioTopCenter.Appearance = System.Windows.Forms.Appearance.Button;
        this.radioTopCenter.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
        this.radioTopCenter.FlatAppearance.BorderSize = 0;
        this.radioTopCenter.FlatAppearance.CheckedBackColor = System.Drawing.Color.SteelBlue;
        this.radioTopCenter.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.radioTopCenter.FlatAppearance.MouseOverBackColor = System.Drawing.Color.CornflowerBlue;
        this.radioTopCenter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.radioTopCenter.Image = global::Gorgon.Windows.Properties.Resources.arrow_n_24x24;
        this.radioTopCenter.Location = new System.Drawing.Point(38, 7);
        this.radioTopCenter.Margin = new System.Windows.Forms.Padding(0);
        this.radioTopCenter.Name = "radioTopCenter";
        this.radioTopCenter.Size = new System.Drawing.Size(29, 26);
        this.radioTopCenter.TabIndex = 1;
        this.radioTopCenter.UseVisualStyleBackColor = false;
        this.radioTopCenter.Click += new System.EventHandler(this.Radio_Click);
        // 
        // radioTopLeft
        // 
        this.radioTopLeft.Appearance = System.Windows.Forms.Appearance.Button;
        this.radioTopLeft.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
        this.radioTopLeft.FlatAppearance.BorderSize = 0;
        this.radioTopLeft.FlatAppearance.CheckedBackColor = System.Drawing.Color.SteelBlue;
        this.radioTopLeft.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.radioTopLeft.FlatAppearance.MouseOverBackColor = System.Drawing.Color.CornflowerBlue;
        this.radioTopLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.radioTopLeft.Image = global::Gorgon.Windows.Properties.Resources.arrow_nw_24x24;
        this.radioTopLeft.Location = new System.Drawing.Point(3, 7);
        this.radioTopLeft.Margin = new System.Windows.Forms.Padding(0);
        this.radioTopLeft.Name = "radioTopLeft";
        this.radioTopLeft.Size = new System.Drawing.Size(29, 26);
        this.radioTopLeft.TabIndex = 0;
        this.radioTopLeft.UseVisualStyleBackColor = false;
        this.radioTopLeft.Click += new System.EventHandler(this.Radio_Click);
        // 
        // GorgonAlignmentPicker
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.Controls.Add(this.panelAnchor);
        this.Name = "GorgonAlignmentPicker";
        this.Size = new System.Drawing.Size(105, 105);
        this.panelAnchor.ResumeLayout(false);
        this.ResumeLayout(false);

    }



    private System.Windows.Forms.Panel panelAnchor;
    private System.Windows.Forms.RadioButton radioBottomRight;
    private System.Windows.Forms.RadioButton radioBottomCenter;
    private System.Windows.Forms.RadioButton radioBottomLeft;
    private System.Windows.Forms.RadioButton radioMiddleRight;
    private System.Windows.Forms.RadioButton radioCenter;
    private System.Windows.Forms.RadioButton radioMiddleLeft;
    private System.Windows.Forms.RadioButton radioTopRight;
    private System.Windows.Forms.RadioButton radioTopCenter;
    private System.Windows.Forms.RadioButton radioTopLeft;
}
