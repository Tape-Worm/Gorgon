namespace Gorgon.Examples
{
    partial class FormExample
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormExample));
            this.LabelText = new System.Windows.Forms.Label();
            this.ButtonThemeColors = new System.Windows.Forms.Button();
            this.ContentArea.SuspendLayout();
            this.SuspendLayout();
            // 
            // ContentArea
            // 
            this.ContentArea.BackColor = System.Drawing.SystemColors.Window;
            this.ContentArea.Controls.Add(this.LabelText);
            this.ContentArea.Controls.Add(this.ButtonThemeColors);
            this.ContentArea.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ContentArea.ForeColor = System.Drawing.SystemColors.WindowText;
            this.ContentArea.Location = new System.Drawing.Point(2, 35);
            this.ContentArea.Size = new System.Drawing.Size(952, 568);
            // 
            // LabelText
            // 
            this.LabelText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelText.ForeColor = System.Drawing.Color.Black;
            this.LabelText.Location = new System.Drawing.Point(0, 0);
            this.LabelText.Margin = new System.Windows.Forms.Padding(3);
            this.LabelText.Name = "LabelText";
            this.LabelText.Size = new System.Drawing.Size(952, 532);
            this.LabelText.TabIndex = 0;
            this.LabelText.Text = resources.GetString("LabelText.Text");
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
            this.ButtonThemeColors.Location = new System.Drawing.Point(0, 532);
            this.ButtonThemeColors.Name = "ButtonThemeColors";
            this.ButtonThemeColors.Size = new System.Drawing.Size(952, 36);
            this.ButtonThemeColors.TabIndex = 1;
            this.ButtonThemeColors.Text = "Click to change theme colors.";
            this.ButtonThemeColors.UseVisualStyleBackColor = true;
            this.ButtonThemeColors.Click += new System.EventHandler(this.ButtonThemeColors_Click);
            // 
            // FormExample
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BorderSize = 2;
            this.ClientSize = new System.Drawing.Size(956, 605);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.MinimumSize = new System.Drawing.Size(320, 240);
            this.Name = "FormExample";
            this.Padding = new System.Windows.Forms.Padding(2);
            this.ShowBorder = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Gorgon Example #6 - Flat Form.";
            this.ContentArea.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label LabelText;
        private System.Windows.Forms.Button ButtonThemeColors;
    }
}

