namespace Gorgon.Editor.UI.Controls;

partial class FormSaveDialog
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSaveDialog));
        this.label1 = new System.Windows.Forms.Label();
        this.TextFileName = new System.Windows.Forms.TextBox();
        this.FileExplorer = new Gorgon.Editor.UI.Controls.ContentFileExplorer();
        this.panel1 = new System.Windows.Forms.Panel();
        this.ButtonCancel = new System.Windows.Forms.Button();
        this.ButtonOK = new System.Windows.Forms.Button();
        this.TableControls = new System.Windows.Forms.TableLayoutPanel();
        this.panel1.SuspendLayout();
        this.TableControls.SuspendLayout();
        this.SuspendLayout();
        // 
        // label1
        // 
        this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(3, 446);
        this.label1.Margin = new System.Windows.Forms.Padding(3);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(57, 13);
        this.label1.TabIndex = 0;
        this.label1.Text = "File Name:";
        // 
        // TextFileName
        // 
        this.TextFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.TextFileName.BackColor = System.Drawing.Color.White;
        this.TextFileName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.TextFileName.ForeColor = System.Drawing.Color.Black;
        this.TextFileName.Location = new System.Drawing.Point(66, 443);
        this.TextFileName.Name = "TextFileName";
        this.TextFileName.Size = new System.Drawing.Size(533, 20);
        this.TextFileName.TabIndex = 1;
        this.TextFileName.TextChanged += new System.EventHandler(this.TextFileName_TextChanged);
        // 
        // FileExplorer
        // 
        this.FileExplorer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.FileExplorer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.TableControls.SetColumnSpan(this.FileExplorer, 2);
        this.FileExplorer.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.FileExplorer.ForeColor = System.Drawing.Color.White;
        this.FileExplorer.Location = new System.Drawing.Point(3, 3);
        this.FileExplorer.MultiSelect = false;
        this.FileExplorer.Name = "FileExplorer";
        this.FileExplorer.Size = new System.Drawing.Size(596, 434);
        this.FileExplorer.TabIndex = 0;
        this.FileExplorer.FileEntriesFocused += new System.EventHandler<Gorgon.Editor.UI.Controls.ContentFileEntriesFocusedArgs>(this.FileExplorer_FileEntriesFocused);
        // 
        // panel1
        // 
        this.panel1.AutoSize = true;
        this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.panel1.Controls.Add(this.ButtonCancel);
        this.panel1.Controls.Add(this.ButtonOK);
        this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panel1.Location = new System.Drawing.Point(0, 466);
        this.panel1.Name = "panel1";
        this.panel1.Size = new System.Drawing.Size(592, 31);
        this.panel1.TabIndex = 1;
        // 
        // ButtonCancel
        // 
        this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonCancel.AutoSize = true;
        this.ButtonCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.ButtonCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonCancel.Location = new System.Drawing.Point(518, 3);
        this.ButtonCancel.MinimumSize = new System.Drawing.Size(64, 23);
        this.ButtonCancel.Name = "ButtonCancel";
        this.ButtonCancel.Size = new System.Drawing.Size(64, 25);
        this.ButtonCancel.TabIndex = 1;
        this.ButtonCancel.Text = "Cancel";
        this.ButtonCancel.UseVisualStyleBackColor = false;
        // 
        // ButtonOK
        // 
        this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonOK.AutoSize = true;
        this.ButtonOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.ButtonOK.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonOK.Location = new System.Drawing.Point(448, 3);
        this.ButtonOK.MinimumSize = new System.Drawing.Size(64, 23);
        this.ButtonOK.Name = "ButtonOK";
        this.ButtonOK.Size = new System.Drawing.Size(64, 25);
        this.ButtonOK.TabIndex = 0;
        this.ButtonOK.Text = "OK";
        this.ButtonOK.UseVisualStyleBackColor = false;
        this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
        // 
        // TableControls
        // 
        this.TableControls.AutoSize = true;
        this.TableControls.ColumnCount = 2;
        this.TableControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableControls.Controls.Add(this.FileExplorer, 0, 0);
        this.TableControls.Controls.Add(this.label1, 0, 1);
        this.TableControls.Controls.Add(this.TextFileName, 1, 1);
        this.TableControls.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableControls.Location = new System.Drawing.Point(0, 0);
        this.TableControls.Name = "TableControls";
        this.TableControls.RowCount = 2;
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.Size = new System.Drawing.Size(592, 466);
        this.TableControls.TabIndex = 0;
        // 
        // FormSaveDialog
        // 
        this.AcceptButton = this.ButtonOK;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.CancelButton = this.ButtonCancel;
        this.ClientSize = new System.Drawing.Size(592, 497);
        this.Controls.Add(this.TableControls);
        this.Controls.Add(this.panel1);
        this.ForeColor = System.Drawing.Color.White;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MinimizeBox = false;
        this.Name = "FormSaveDialog";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Save";
        this.panel1.ResumeLayout(false);
        this.panel1.PerformLayout();
        this.TableControls.ResumeLayout(false);
        this.TableControls.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }



    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox TextFileName;
    private Gorgon.Editor.UI.Controls.ContentFileExplorer FileExplorer;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button ButtonCancel;
    private System.Windows.Forms.Button ButtonOK;
    private System.Windows.Forms.TableLayoutPanel TableControls;
}
