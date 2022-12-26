namespace Gorgon.Examples;

partial class FormTextEditor
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTextEditor));
        this.TextContent = new System.Windows.Forms.TextBox();
        this.panel1 = new System.Windows.Forms.Panel();
        this.ButtonCancel = new System.Windows.Forms.Button();
        this.ButtonOK = new System.Windows.Forms.Button();
        this.panel1.SuspendLayout();
        this.SuspendLayout();
        // 
        // TextContent
        // 
        this.TextContent.AcceptsReturn = true;
        this.TextContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.TextContent.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.TextContent.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TextContent.ForeColor = System.Drawing.Color.White;
        this.TextContent.Location = new System.Drawing.Point(0, 0);
        this.TextContent.MaxLength = 8192;
        this.TextContent.Multiline = true;
        this.TextContent.Name = "TextContent";
        this.TextContent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
        this.TextContent.Size = new System.Drawing.Size(784, 528);
        this.TextContent.TabIndex = 0;
        this.TextContent.WordWrap = false;
        this.TextContent.TextChanged += new System.EventHandler(this.TextContent_TextChanged);
        // 
        // panel1
        // 
        this.panel1.AutoSize = true;
        this.panel1.Controls.Add(this.ButtonCancel);
        this.panel1.Controls.Add(this.ButtonOK);
        this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panel1.Location = new System.Drawing.Point(0, 528);
        this.panel1.Name = "panel1";
        this.panel1.Size = new System.Drawing.Size(784, 33);
        this.panel1.TabIndex = 1;
        // 
        // ButtonCancel
        // 
        this.ButtonCancel.AutoSize = true;
        this.ButtonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.ButtonCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonCancel.Location = new System.Drawing.Point(697, 3);
        this.ButtonCancel.Name = "ButtonCancel";
        this.ButtonCancel.Size = new System.Drawing.Size(75, 27);
        this.ButtonCancel.TabIndex = 1;
        this.ButtonCancel.Text = "&Cancel";
        this.ButtonCancel.UseVisualStyleBackColor = false;
        // 
        // ButtonOK
        // 
        this.ButtonOK.AutoSize = true;
        this.ButtonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.ButtonOK.Enabled = false;
        this.ButtonOK.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonOK.Location = new System.Drawing.Point(616, 3);
        this.ButtonOK.Name = "ButtonOK";
        this.ButtonOK.Size = new System.Drawing.Size(75, 27);
        this.ButtonOK.TabIndex = 0;
        this.ButtonOK.Text = "&OK";
        this.ButtonOK.UseVisualStyleBackColor = false;
        // 
        // FormTextEditor
        // 
        this.AcceptButton = this.ButtonOK;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.CancelButton = this.ButtonCancel;
        this.ClientSize = new System.Drawing.Size(784, 561);
        this.Controls.Add(this.TextContent);
        this.Controls.Add(this.panel1);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.ForeColor = System.Drawing.Color.White;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "FormTextEditor";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Change Text";
        this.panel1.ResumeLayout(false);
        this.panel1.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox TextContent;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button ButtonOK;
    private System.Windows.Forms.Button ButtonCancel;
}