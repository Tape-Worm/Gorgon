using System.ComponentModel;

namespace Gorgon.Examples;

    partial class Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
        ComponentResourceManager resources = new ComponentResourceManager(typeof(Form));
        SuspendLayout();
        // 
        // Form
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1264, 761);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "Form";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Folder File System of Power (v1.x Example)";
        ResumeLayout(false);
    }
}

