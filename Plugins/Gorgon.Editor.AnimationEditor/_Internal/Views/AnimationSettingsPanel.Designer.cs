namespace Gorgon.Editor.AnimationEditor;

partial class AnimationSettingsPanel
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



    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnimationSettingsPanel));
        this.CheckAnimatePrimaryBg = new System.Windows.Forms.CheckBox();
        this.CheckOnionSkin = new System.Windows.Forms.CheckBox();
        this.panel1 = new System.Windows.Forms.Panel();
        this.TableRes = new System.Windows.Forms.TableLayoutPanel();
        this.label1 = new System.Windows.Forms.Label();
        this.label2 = new System.Windows.Forms.Label();
        this.NumericXRes = new System.Windows.Forms.NumericUpDown();
        this.NumericYRes = new System.Windows.Forms.NumericUpDown();
        this.TipSettings = new System.Windows.Forms.ToolTip(this.components);
        this.CheckAddTextureTrack = new System.Windows.Forms.CheckBox();
        this.CheckUnsupported = new System.Windows.Forms.CheckBox();
        this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
        this.PanelBody.SuspendLayout();
        this.TableRes.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericXRes)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericYRes)).BeginInit();
        this.tableLayoutPanel1.SuspendLayout();
        this.SuspendLayout();
        // 
        // PanelBody
        // 
        this.PanelBody.Controls.Add(this.tableLayoutPanel1);
        // 
        // CheckAnimatePrimaryBg
        // 
        this.CheckAnimatePrimaryBg.AutoSize = true;
        this.CheckAnimatePrimaryBg.Location = new System.Drawing.Point(3, 3);
        this.CheckAnimatePrimaryBg.Name = "CheckAnimatePrimaryBg";
        this.CheckAnimatePrimaryBg.Size = new System.Drawing.Size(321, 19);
        this.CheckAnimatePrimaryBg.TabIndex = 0;
        this.CheckAnimatePrimaryBg.Text = "Animate background when no primary sprite is present?";
        this.TipSettings.SetToolTip(this.CheckAnimatePrimaryBg, resources.GetString("CheckAnimatePrimaryBg.ToolTip"));
        this.CheckAnimatePrimaryBg.UseVisualStyleBackColor = true;
        this.CheckAnimatePrimaryBg.Click += new System.EventHandler(this.CheckAnimatePrimaryBg_Click);
        // 
        // CheckOnionSkin
        // 
        this.CheckOnionSkin.AutoSize = true;
        this.CheckOnionSkin.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
        this.CheckOnionSkin.Location = new System.Drawing.Point(3, 53);
        this.CheckOnionSkin.Name = "CheckOnionSkin";
        this.CheckOnionSkin.Size = new System.Drawing.Size(310, 34);
        this.CheckOnionSkin.TabIndex = 2;
        this.CheckOnionSkin.Text = "Use onion skinning overlays when editing keyframes? \r\n(Note: Some editors do not " +
"support onion skinning)";
        this.CheckOnionSkin.TextAlign = System.Drawing.ContentAlignment.TopLeft;
        this.TipSettings.SetToolTip(this.CheckOnionSkin, resources.GetString("CheckOnionSkin.ToolTip"));
        this.CheckOnionSkin.UseVisualStyleBackColor = true;
        this.CheckOnionSkin.Click += new System.EventHandler(this.CheckOnionSkin_Click);
        // 
        // panel1
        // 
        this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
        this.panel1.Location = new System.Drawing.Point(6, 115);
        this.panel1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.panel1.Name = "panel1";
        this.panel1.Size = new System.Drawing.Size(588, 2);
        this.panel1.TabIndex = 2;
        // 
        // TableRes
        // 
        this.TableRes.AutoSize = true;
        this.TableRes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.TableRes.ColumnCount = 3;
        this.TableRes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableRes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableRes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableRes.Controls.Add(this.label1, 0, 0);
        this.TableRes.Controls.Add(this.label2, 1, 1);
        this.TableRes.Controls.Add(this.NumericXRes, 0, 1);
        this.TableRes.Controls.Add(this.NumericYRes, 2, 1);
        this.TableRes.Location = new System.Drawing.Point(3, 120);
        this.TableRes.Name = "TableRes";
        this.TableRes.RowCount = 3;
        this.TableRes.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableRes.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableRes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
        this.TableRes.Size = new System.Drawing.Size(271, 52);
        this.TableRes.TabIndex = 3;
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(3, 0);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(107, 15);
        this.label1.TabIndex = 0;
        this.label1.Text = "Default Resolution:";
        this.TipSettings.SetToolTip(this.label1, resources.GetString("label1.ToolTip"));
        // 
        // label2
        // 
        this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.label2.AutoSize = true;
        this.label2.Location = new System.Drawing.Point(129, 22);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(13, 15);
        this.label2.TabIndex = 2;
        this.label2.Text = "x";
        this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        this.TipSettings.SetToolTip(this.label2, resources.GetString("label2.ToolTip"));
        // 
        // NumericXRes
        // 
        this.NumericXRes.BackColor = System.Drawing.Color.White;
        this.NumericXRes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericXRes.ForeColor = System.Drawing.Color.Black;
        this.NumericXRes.Increment = new decimal(new int[] {
        256,
        0,
        0,
        0});
        this.NumericXRes.Location = new System.Drawing.Point(3, 18);
        this.NumericXRes.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericXRes.Minimum = new decimal(new int[] {
        256,
        0,
        0,
        0});
        this.NumericXRes.Name = "NumericXRes";
        this.NumericXRes.Size = new System.Drawing.Size(120, 23);
        this.NumericXRes.TabIndex = 1;
        this.NumericXRes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.TipSettings.SetToolTip(this.NumericXRes, resources.GetString("NumericXRes.ToolTip"));
        this.NumericXRes.Value = new decimal(new int[] {
        256,
        0,
        0,
        0});
        this.NumericXRes.ValueChanged += new System.EventHandler(this.NumericXRes_ValueChanged);
        // 
        // NumericYRes
        // 
        this.NumericYRes.BackColor = System.Drawing.Color.White;
        this.NumericYRes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericYRes.ForeColor = System.Drawing.Color.Black;
        this.NumericYRes.Increment = new decimal(new int[] {
        256,
        0,
        0,
        0});
        this.NumericYRes.Location = new System.Drawing.Point(148, 18);
        this.NumericYRes.Maximum = new decimal(new int[] {
        16384,
        0,
        0,
        0});
        this.NumericYRes.Minimum = new decimal(new int[] {
        256,
        0,
        0,
        0});
        this.NumericYRes.Name = "NumericYRes";
        this.NumericYRes.Size = new System.Drawing.Size(120, 23);
        this.NumericYRes.TabIndex = 3;
        this.NumericYRes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.TipSettings.SetToolTip(this.NumericYRes, resources.GetString("NumericYRes.ToolTip"));
        this.NumericYRes.Value = new decimal(new int[] {
        256,
        0,
        0,
        0});
        this.NumericYRes.ValueChanged += new System.EventHandler(this.NumericXRes_ValueChanged);
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
        // CheckAddTextureTrack
        // 
        this.CheckAddTextureTrack.AutoSize = true;
        this.CheckAddTextureTrack.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
        this.CheckAddTextureTrack.Location = new System.Drawing.Point(3, 28);
        this.CheckAddTextureTrack.Name = "CheckAddTextureTrack";
        this.CheckAddTextureTrack.Size = new System.Drawing.Size(350, 19);
        this.CheckAddTextureTrack.TabIndex = 1;
        this.CheckAddTextureTrack.Text = "Add default texture track when the primary sprite is assigned?";
        this.CheckAddTextureTrack.TextAlign = System.Drawing.ContentAlignment.TopLeft;
        this.TipSettings.SetToolTip(this.CheckAddTextureTrack, "When this is checked, a texture track is added to a new animation \r\nwhen a primar" +
    "y sprite is assigned for the first time.\r\n");
        this.CheckAddTextureTrack.UseVisualStyleBackColor = true;
        this.CheckAddTextureTrack.Click += new System.EventHandler(this.CheckAddTextureTrack_Click);
        // 
        // CheckUnsupported
        // 
        this.CheckUnsupported.AutoSize = true;
        this.CheckUnsupported.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
        this.CheckUnsupported.Location = new System.Drawing.Point(3, 93);
        this.CheckUnsupported.Name = "CheckUnsupported";
        this.CheckUnsupported.Size = new System.Drawing.Size(238, 19);
        this.CheckUnsupported.TabIndex = 2;
        this.CheckUnsupported.Text = "Warn on unsupported animation tracks?";
        this.CheckUnsupported.TextAlign = System.Drawing.ContentAlignment.TopLeft;
        this.TipSettings.SetToolTip(this.CheckUnsupported, resources.GetString("CheckUnsupported.ToolTip"));
        this.CheckUnsupported.UseVisualStyleBackColor = true;
        this.CheckUnsupported.Click += new System.EventHandler(this.CheckUnsupported_Click);
        // 
        // tableLayoutPanel1
        // 
        this.tableLayoutPanel1.AutoSize = true;
        this.tableLayoutPanel1.ColumnCount = 1;
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel1.Controls.Add(this.TableRes, 0, 5);
        this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 4);
        this.tableLayoutPanel1.Controls.Add(this.CheckAnimatePrimaryBg, 0, 0);
        this.tableLayoutPanel1.Controls.Add(this.CheckAddTextureTrack, 0, 1);
        this.tableLayoutPanel1.Controls.Add(this.CheckOnionSkin, 0, 2);
        this.tableLayoutPanel1.Controls.Add(this.CheckUnsupported, 0, 3);
        this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
        this.tableLayoutPanel1.Name = "tableLayoutPanel1";
        this.tableLayoutPanel1.RowCount = 6;
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.tableLayoutPanel1.Size = new System.Drawing.Size(600, 445);
        this.tableLayoutPanel1.TabIndex = 0;
        // 
        // AnimationSettingsPanel
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Name = "AnimationSettingsPanel";
        this.Text = "Animation Editor Settings";
        this.PanelBody.ResumeLayout(false);
        this.PanelBody.PerformLayout();
        this.TableRes.ResumeLayout(false);
        this.TableRes.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericXRes)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericYRes)).EndInit();
        this.tableLayoutPanel1.ResumeLayout(false);
        this.tableLayoutPanel1.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }


    private System.Windows.Forms.CheckBox CheckAnimatePrimaryBg;
    private System.Windows.Forms.CheckBox CheckOnionSkin;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.TableLayoutPanel TableRes;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ToolTip TipSettings;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.NumericUpDown NumericXRes;
    private System.Windows.Forms.NumericUpDown NumericYRes;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.CheckBox CheckAddTextureTrack;
    private System.Windows.Forms.CheckBox CheckUnsupported;
}
