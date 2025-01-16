namespace Gorgon.Editor.AnimationEditor;

partial class AnimationCodecSettingsPanel
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
            /*DataContext?.OnUnload();
            UnassignEvents();*/
        }

        base.Dispose(disposing);
    }



    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.StripMenu = new System.Windows.Forms.ToolStrip();
        this.ButtonAddCodec = new System.Windows.Forms.ToolStripButton();
        this.ButtonRemoveCodecs = new System.Windows.Forms.ToolStripButton();
        this.ListCodecs = new System.Windows.Forms.ListView();
        this.ColumnCodec = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.ColumnPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.PanelBody.SuspendLayout();
        this.StripMenu.SuspendLayout();
        this.SuspendLayout();
        // 
        // PanelBody
        // 
        this.PanelBody.Controls.Add(this.ListCodecs);
        this.PanelBody.Controls.Add(this.StripMenu);
        // 
        // StripMenu
        // 
        this.StripMenu.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.StripMenu.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
        this.StripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.ButtonAddCodec,
        this.ButtonRemoveCodecs});
        this.StripMenu.Location = new System.Drawing.Point(0, 0);
        this.StripMenu.Name = "StripMenu";
        this.StripMenu.Size = new System.Drawing.Size(600, 29);
        this.StripMenu.Stretch = true;
        this.StripMenu.TabIndex = 0;
        this.StripMenu.Text = "toolStrip1";
        // 
        // ButtonAddCodec
        // 
        this.ButtonAddCodec.Image = global::Gorgon.Editor.AnimationEditor.Properties.Resources.add_plugin_22x22;
        this.ButtonAddCodec.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
        this.ButtonAddCodec.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.ButtonAddCodec.Name = "ButtonAddCodec";
        this.ButtonAddCodec.Size = new System.Drawing.Size(90, 26);
        this.ButtonAddCodec.Text = "Add codec";
        this.ButtonAddCodec.ToolTipText = "Adds a new codec plug-in to the list.";
        this.ButtonAddCodec.Click += new System.EventHandler(this.ButtonAddCodec_Click);
        // 
        // ButtonRemoveCodecs
        // 
        this.ButtonRemoveCodecs.Image = global::Gorgon.Editor.AnimationEditor.Properties.Resources.remove_plugins_22x22;
        this.ButtonRemoveCodecs.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
        this.ButtonRemoveCodecs.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.ButtonRemoveCodecs.Name = "ButtonRemoveCodecs";
        this.ButtonRemoveCodecs.Size = new System.Drawing.Size(111, 26);
        this.ButtonRemoveCodecs.Text = "Remove codec";
        this.ButtonRemoveCodecs.ToolTipText = "Removes the selected codec(s).";
        this.ButtonRemoveCodecs.Click += new System.EventHandler(this.ButtonRemoveCodecs_Click);
        // 
        // ListCodecs
        // 
        this.ListCodecs.BackColor = System.Drawing.Color.White;
        this.ListCodecs.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.ListCodecs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
        this.ColumnCodec,
        this.ColumnPath});
        this.ListCodecs.Dock = System.Windows.Forms.DockStyle.Fill;
        this.ListCodecs.ForeColor = System.Drawing.Color.Black;
        this.ListCodecs.FullRowSelect = true;
        this.ListCodecs.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
        this.ListCodecs.HideSelection = false;
        this.ListCodecs.Location = new System.Drawing.Point(0, 29);
        this.ListCodecs.Name = "ListCodecs";
        this.ListCodecs.Size = new System.Drawing.Size(600, 416);
        this.ListCodecs.TabIndex = 1;
        this.ListCodecs.UseCompatibleStateImageBehavior = false;
        this.ListCodecs.View = System.Windows.Forms.View.Details;
        this.ListCodecs.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.ListCodecs_ItemSelectionChanged);
        // 
        // ColumnCodec
        // 
        this.ColumnCodec.Text = "Codec";
        // 
        // ColumnPath
        // 
        this.ColumnPath.Text = "Path";
        // 
        // AnimationCodecSettingsPanel
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Name = "AnimationCodecSettingsPanel";
        this.Text = "Animation Codecs";
        this.PanelBody.ResumeLayout(false);
        this.PanelBody.PerformLayout();
        this.StripMenu.ResumeLayout(false);
        this.StripMenu.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }



    private System.Windows.Forms.ToolStrip StripMenu;
    private System.Windows.Forms.ListView ListCodecs;
    private System.Windows.Forms.ColumnHeader ColumnCodec;
    private System.Windows.Forms.ColumnHeader ColumnPath;
    private System.Windows.Forms.ToolStripButton ButtonAddCodec;
    private System.Windows.Forms.ToolStripButton ButtonRemoveCodecs;
}
