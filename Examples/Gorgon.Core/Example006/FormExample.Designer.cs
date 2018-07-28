using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Examples
{
    partial class FormExample
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormExample));
            this.ButtonThemeColors = new System.Windows.Forms.Button();
            this.TextText = new System.Windows.Forms.TextBox();
            this.ContentArea.SuspendLayout();
            this.SuspendLayout();
            // 
            // ContentArea
            // 
            this.ContentArea.BackColor = System.Drawing.SystemColors.Window;
            this.ContentArea.Controls.Add(this.TextText);
            this.ContentArea.Controls.Add(this.ButtonThemeColors);
            this.ContentArea.ForeColor = System.Drawing.SystemColors.WindowText;
            this.ContentArea.Location = new System.Drawing.Point(1, 33);
            this.ContentArea.Size = new System.Drawing.Size(638, 446);
            // 
            // ButtonThemeColors
            // 
            this.ButtonThemeColors.BackColor = System.Drawing.SystemColors.Control;
            this.ButtonThemeColors.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ButtonThemeColors.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.ButtonThemeColors.FlatAppearance.BorderSize = 0;
            this.ButtonThemeColors.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
            this.ButtonThemeColors.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlLight;
            this.ButtonThemeColors.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonThemeColors.Location = new System.Drawing.Point(0, 410);
            this.ButtonThemeColors.Name = "ButtonThemeColors";
            this.ButtonThemeColors.Size = new System.Drawing.Size(638, 36);
            this.ButtonThemeColors.TabIndex = 1;
            this.ButtonThemeColors.Text = "Click to change theme colors.";
            this.ButtonThemeColors.UseVisualStyleBackColor = true;
            this.ButtonThemeColors.Click += new System.EventHandler(this.ButtonThemeColors_Click);
            // 
            // TextText
            // 
            this.TextText.BackColor = System.Drawing.Color.White;
            this.TextText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextText.Location = new System.Drawing.Point(0, 0);
            this.TextText.Multiline = true;
            this.TextText.Name = "TextText";
            this.TextText.ReadOnly = true;
            this.TextText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TextText.Size = new System.Drawing.Size(638, 410);
            this.TextText.TabIndex = 1;
            this.TextText.Text = resources.GetString("TextText.Text");
            // 
            // FormExample
            // 
            this.BorderSize = 2;
            this.ClientSize = new System.Drawing.Size(640, 480);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.MinimumSize = new System.Drawing.Size(320, 240);
            this.Name = "FormExample";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.ShowBorder = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Gorgon Example #6 - Flat Form.";
            this.ContentArea.ResumeLayout(false);
            this.ContentArea.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private Button ButtonThemeColors;
        private TextBox TextText;
    }
}

