namespace Gorgon.Editor.AnimationEditor;

partial class AnimationTrackContainer
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
        }

        base.Dispose(disposing);
    }



    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
        this.GridTrackKeys = new Gorgon.Editor.AnimationEditor.DataGridViewEx();
        this.Column0 = new System.Windows.Forms.DataGridViewImageColumn();
        ((System.ComponentModel.ISupportInitialize)(this.GridTrackKeys)).BeginInit();
        this.SuspendLayout();
        // 
        // GridTrackKeys
        // 
        this.GridTrackKeys.AllowCellDragging = true;
        this.GridTrackKeys.AllowDrop = true;
        this.GridTrackKeys.AllowUserToAddRows = false;
        this.GridTrackKeys.AllowUserToDeleteRows = false;
        this.GridTrackKeys.AllowUserToResizeColumns = false;
        this.GridTrackKeys.AllowUserToResizeRows = false;
        dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
        dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
        dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.SteelBlue;
        dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White;
        this.GridTrackKeys.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
        this.GridTrackKeys.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.GridTrackKeys.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.GridTrackKeys.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
        this.GridTrackKeys.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
        dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
        dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
        dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
        dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.SteelBlue;
        dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
        dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
        this.GridTrackKeys.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
        this.GridTrackKeys.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        this.GridTrackKeys.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
        this.Column0});
        dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
        dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F);
        dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
        dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.SteelBlue;
        dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
        dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
        this.GridTrackKeys.DefaultCellStyle = dataGridViewCellStyle3;
        this.GridTrackKeys.Dock = System.Windows.Forms.DockStyle.Fill;
        this.GridTrackKeys.EnableHeadersVisualStyles = false;
        this.GridTrackKeys.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.GridTrackKeys.Location = new System.Drawing.Point(0, 0);
        this.GridTrackKeys.Name = "GridTrackKeys";
        this.GridTrackKeys.NoDataMessage = "Add a new track to begin animating.";
        this.GridTrackKeys.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
        dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9F);
        dataGridViewCellStyle4.ForeColor = System.Drawing.Color.White;
        dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.SteelBlue;
        dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
        dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
        this.GridTrackKeys.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
        this.GridTrackKeys.RowHeadersWidth = 256;
        this.GridTrackKeys.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
        dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI", 9F);
        dataGridViewCellStyle5.ForeColor = System.Drawing.Color.White;
        dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.SteelBlue;
        dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.White;
        this.GridTrackKeys.RowsDefaultCellStyle = dataGridViewCellStyle5;
        this.GridTrackKeys.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
        this.GridTrackKeys.ShowCellErrors = false;
        this.GridTrackKeys.ShowEditingIcon = false;
        this.GridTrackKeys.Size = new System.Drawing.Size(648, 358);
        this.GridTrackKeys.TabIndex = 0;
        this.GridTrackKeys.VirtualMode = true;
        this.GridTrackKeys.CellsDrag += new System.EventHandler<Gorgon.Editor.AnimationEditor.CellsDragEventArgs>(this.GridTrackKeys_CellsDrag);
        this.GridTrackKeys.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridTrackKeys_CellValueChanged);
        this.GridTrackKeys.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.GridTrackKeys_CellValueNeeded);
        this.GridTrackKeys.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridTrackKeys_RowHeaderMouseClick);
        this.GridTrackKeys.SelectionChanged += new System.EventHandler(this.GridTrackKeys_SelectionChanged);
        this.GridTrackKeys.SizeChanged += new System.EventHandler(this.GridTrackKeys_SizeChanged);
        this.GridTrackKeys.DragDrop += new System.Windows.Forms.DragEventHandler(this.GridTrackKeys_DragDrop);
        this.GridTrackKeys.DragOver += new System.Windows.Forms.DragEventHandler(this.GridTrackKeys_DragOver);
        // 
        // Column0
        // 
        this.Column0.HeaderText = "0";
        this.Column0.Name = "Column0";
        this.Column0.Resizable = System.Windows.Forms.DataGridViewTriState.False;
        this.Column0.Width = 33;
        // 
        // AnimationTrackContainer
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.Controls.Add(this.GridTrackKeys);
        
        this.ForeColor = System.Drawing.Color.White;
        this.Name = "AnimationTrackContainer";
        this.Size = new System.Drawing.Size(648, 358);
        ((System.ComponentModel.ISupportInitialize)(this.GridTrackKeys)).EndInit();
        this.ResumeLayout(false);

    }



    private DataGridViewEx GridTrackKeys;
    private System.Windows.Forms.DataGridViewImageColumn Column0;
}
