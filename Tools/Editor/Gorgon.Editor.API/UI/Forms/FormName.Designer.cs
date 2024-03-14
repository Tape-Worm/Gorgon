namespace Gorgon.Editor.UI.Forms;

partial class FormName
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormName));
        this.ButtonOK = new System.Windows.Forms.Button();
        this.ButtonCancel = new System.Windows.Forms.Button();
        this.TableName = new System.Windows.Forms.TableLayoutPanel();
        this.PanelButtons = new System.Windows.Forms.Panel();
        this.TextName = new Gorgon.UI.GorgonCueTextBox();
        this.PanelName = new System.Windows.Forms.Panel();
        this.PanelUnderline = new System.Windows.Forms.Panel();
        this.TableName.SuspendLayout();
        this.PanelButtons.SuspendLayout();
        this.PanelName.SuspendLayout();
        this.SuspendLayout();
        // 
        // ButtonOK
        // 
        this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonOK.AutoSize = true;
        this.ButtonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.ButtonOK.Enabled = false;
        this.ButtonOK.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Orange;
        this.ButtonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonOK.Location = new System.Drawing.Point(295, 3);
        this.ButtonOK.Name = "ButtonOK";
        this.ButtonOK.Size = new System.Drawing.Size(80, 29);
        this.ButtonOK.TabIndex = 0;
        this.ButtonOK.Text = "&OK";
        this.ButtonOK.UseVisualStyleBackColor = false;
        // 
        // ButtonCancel
        // 
        this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonCancel.AutoSize = true;
        this.ButtonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.ButtonCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Orange;
        this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonCancel.Location = new System.Drawing.Point(381, 3);
        this.ButtonCancel.Name = "ButtonCancel";
        this.ButtonCancel.Size = new System.Drawing.Size(80, 29);
        this.ButtonCancel.TabIndex = 1;
        this.ButtonCancel.Text = "&Cancel";
        this.ButtonCancel.UseVisualStyleBackColor = false;
        // 
        // TableName
        // 
        this.TableName.AutoSize = true;
        this.TableName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TableName.ColumnCount = 1;
        this.TableName.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableName.Controls.Add(this.PanelName, 0, 0);
        this.TableName.Controls.Add(this.PanelUnderline, 0, 1);
        this.TableName.Controls.Add(this.PanelButtons, 0, 2);
        this.TableName.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableName.Location = new System.Drawing.Point(0, 0);
        this.TableName.Margin = new System.Windows.Forms.Padding(0);
        this.TableName.Name = "TableName";
        this.TableName.RowCount = 3;
        this.TableName.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableName.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 3F));
        this.TableName.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableName.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableName.Size = new System.Drawing.Size(464, 62);
        this.TableName.TabIndex = 0;
        // 
        // PanelButtons
        // 
        this.PanelButtons.AutoSize = true;
        this.PanelButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.PanelButtons.Controls.Add(this.ButtonOK);
        this.PanelButtons.Controls.Add(this.ButtonCancel);
        this.PanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelButtons.Location = new System.Drawing.Point(0, 25);
        this.PanelButtons.Margin = new System.Windows.Forms.Padding(0);
        this.PanelButtons.Name = "PanelButtons";
        this.PanelButtons.Size = new System.Drawing.Size(464, 37);
        this.PanelButtons.TabIndex = 2;
        // 
        // TextName
        // 
        this.TextName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.TextName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TextName.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.TextName.CueText = "Enter a name for the {0}...";
        this.TextName.Font = new System.Drawing.Font("Segoe UI", 12F);
        this.TextName.ForeColor = System.Drawing.Color.White;
        this.TextName.Location = new System.Drawing.Point(0, 0);
        this.TextName.Margin = new System.Windows.Forms.Padding(0);
        this.TextName.MaxLength = 100;
        this.TextName.Name = "TextName";
        this.TextName.Size = new System.Drawing.Size(458, 22);
        this.TextName.TabIndex = 0;
        this.TextName.TextChanged += new System.EventHandler(this.TextName_TextChanged);
        this.TextName.Enter += new System.EventHandler(this.TextName_Enter);
        this.TextName.Leave += new System.EventHandler(this.TextName_Leave);
        this.TextName.MouseEnter += new System.EventHandler(this.TextName_MouseEnter);
        this.TextName.MouseLeave += new System.EventHandler(this.TextName_MouseLeave);
        // 
        // PanelName
        // 
        this.PanelName.AutoSize = true;
        this.PanelName.Controls.Add(this.TextName);
        this.PanelName.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelName.Location = new System.Drawing.Point(3, 0);
        this.PanelName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this.PanelName.Name = "PanelName";
        this.PanelName.Size = new System.Drawing.Size(458, 22);
        this.PanelName.TabIndex = 0;
        // 
        // PanelUnderline
        // 
        this.PanelUnderline.BackColor = System.Drawing.Color.Black;
        this.PanelUnderline.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelUnderline.Location = new System.Drawing.Point(3, 22);
        this.PanelUnderline.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this.PanelUnderline.Name = "PanelUnderline";
        this.PanelUnderline.Size = new System.Drawing.Size(458, 3);
        this.PanelUnderline.TabIndex = 1;
        // 
        // FormName
        // 
        this.AcceptButton = this.ButtonOK;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.AutoSize = true;
        this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.CancelButton = this.ButtonCancel;
        this.ClientSize = new System.Drawing.Size(464, 62);
        this.Controls.Add(this.TableName);
        
        this.ForeColor = System.Drawing.Color.White;
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "FormName";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Name";
        this.TableName.ResumeLayout(false);
        this.TableName.PerformLayout();
        this.PanelButtons.ResumeLayout(false);
        this.PanelButtons.PerformLayout();
        this.PanelName.ResumeLayout(false);
        this.PanelName.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private Gorgon.UI.GorgonCueTextBox TextName;
    private System.Windows.Forms.Button ButtonOK;
    private System.Windows.Forms.Button ButtonCancel;
    private System.Windows.Forms.TableLayoutPanel TableName;
    private System.Windows.Forms.Panel PanelButtons;
    private System.Windows.Forms.Panel PanelName;
    private System.Windows.Forms.Panel PanelUnderline;
}
