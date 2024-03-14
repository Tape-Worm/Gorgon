
namespace Gorgon.Examples;

partial class Main
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
        this.Browser = new Gorgon.UI.GorgonFolderBrowser();
        this.SuspendLayout();
        // 
        // Browser
        // 
        this.Browser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
        this.Browser.DirectoryNameFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.Browser.Dock = System.Windows.Forms.DockStyle.Fill;
        this.Browser.ForeColor = System.Drawing.Color.Black;
        this.Browser.Location = new System.Drawing.Point(0, 0);
        this.Browser.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
        this.Browser.Name = "Browser";
        this.Browser.Size = new System.Drawing.Size(804, 480);
        this.Browser.TabIndex = 0;
        this.Browser.Text = "Double click on a folder to change it. Click the \'+\' to create a folder and \'-\' t" +
"o delete one.";
        this.Browser.FolderEntered += new System.EventHandler<Gorgon.UI.FolderSelectedArgs>(this.Browser_FolderEntered);
        // 
        // FormMain
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(804, 480);
        this.Controls.Add(this.Browser);
        
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "FormMain";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Folder Browser Example";
        this.ResumeLayout(false);

    }



    private UI.GorgonFolderBrowser Browser;
}
