namespace Gorgon.UI;

partial class FormWait
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormWait));
        this.Wait = new Gorgon.UI.GorgonWaitMessagePanel();
        this.SuspendLayout();
        // 
        // Wait
        // 
        this.Wait.AutoSize = true;
        this.Wait.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.Wait.BackColor = System.Drawing.Color.White;
        this.Wait.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.Wait.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.Wait.Location = new System.Drawing.Point(0, 0);
        this.Wait.Margin = new System.Windows.Forms.Padding(0);
        this.Wait.Name = "Wait";
        this.Wait.Size = new System.Drawing.Size(213, 66);
        this.Wait.TabIndex = 0;
        this.Wait.WaitMessage = "Loading...";
        this.Wait.WaitTitle = "Please Wait";
        this.Wait.Resize += new System.EventHandler(this.Wait_Resize);
        // 
        // FormWait
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.AutoSize = true;
        this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.ClientSize = new System.Drawing.Size(213, 66);
        this.ControlBox = false;
        this.Controls.Add(this.Wait);
        
        this.ForeColor = System.Drawing.Color.White;
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "FormWait";
        this.ShowIcon = false;
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "FormWait";
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    // The wait panel embedded in the form.
    internal Gorgon.UI.GorgonWaitMessagePanel Wait;
}
