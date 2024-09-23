namespace Gorgon.Editor.FontEditor;

    partial class FormCharacterPicker
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCharacterPicker));
        this.CharPicker = new Gorgon.Editor.FontEditor.CharacterPicker();
        this.SuspendLayout();
        // 
        // CharPicker
        // 
        this.CharPicker.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.CharPicker.Characters = null;
        this.CharPicker.CurrentFont = null;
        this.CharPicker.Dock = System.Windows.Forms.DockStyle.Fill;
        this.CharPicker.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.CharPicker.ForeColor = System.Drawing.Color.White;
        this.CharPicker.Location = new System.Drawing.Point(1, 1);
        this.CharPicker.Name = "CharPicker";
        this.CharPicker.Size = new System.Drawing.Size(685, 442);
        this.CharPicker.TabIndex = 0;
        this.CharPicker.OkClicked += new System.EventHandler(this.CharPicker_OkClicked);
        this.CharPicker.CancelClicked += new System.EventHandler(this.CharPicker_CancelClicked);
        // 
        // FormCharacterPicker
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.ClientSize = new System.Drawing.Size(687, 444);
        this.Controls.Add(this.CharPicker);
        
        this.ForeColor = System.Drawing.Color.White;
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "FormCharacterPicker";
        this.Padding = new System.Windows.Forms.Padding(1);
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Character Picker";
        this.ResumeLayout(false);

        }



    private CharacterPicker CharPicker;
}
