namespace Gorgon.Editor.SpriteEditor
{
    partial class FormManualRectInput
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormManualRectInput));
            this.ManualInput = new Gorgon.Editor.SpriteEditor.ManualRectInput();
            this.SuspendLayout();
            // 
            // ManualInput
            // 
            this.ManualInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ManualInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ManualInput.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ManualInput.Location = new System.Drawing.Point(0, 0);
            this.ManualInput.Name = "ManualInput";
            this.ManualInput.Size = new System.Drawing.Size(349, 263);
            this.ManualInput.TabIndex = 0;
            // 
            // FormManualRectInput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(349, 263);
            this.Controls.Add(this.ManualInput);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormManualRectInput";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Manual Sprite Clipping";
            this.ResumeLayout(false);

        }

        #endregion

        private ManualRectInput ManualInput;
    }
}