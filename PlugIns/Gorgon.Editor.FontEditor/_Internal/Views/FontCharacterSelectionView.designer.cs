using Gorgon.Editor.UI.Controls;

namespace Gorgon.Editor.FontEditor
{
    partial class FontCharacterSelectionView
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

            if (disposing)
            {                
                UnassignEvents();
                DataContext?.Unload();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.CharPicker = new Gorgon.Editor.FontEditor.CharacterPicker();
            this.PanelBody.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelBody
            // 
            this.PanelBody.Controls.Add(this.CharPicker);
            this.PanelBody.Size = new System.Drawing.Size(689, 548);
            // 
            // CharPicker
            // 
            this.CharPicker.AutoSize = true;
            this.CharPicker.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.CharPicker.Characters = null;
            this.CharPicker.CurrentFont = null;
            this.CharPicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CharPicker.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.CharPicker.ForeColor = System.Drawing.Color.White;
            this.CharPicker.Location = new System.Drawing.Point(0, 0);
            this.CharPicker.Name = "CharPicker";
            this.CharPicker.ShowOkCancel = false;
            this.CharPicker.Size = new System.Drawing.Size(689, 548);
            this.CharPicker.TabIndex = 1;
            this.CharPicker.CharactersChanged += new System.EventHandler(this.CharPicker_CharactersChanged);
            // 
            // FontCharacterSelectionView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Name = "FontCharacterSelectionView";
            this.Size = new System.Drawing.Size(689, 605);
            this.Text = "Font Character Selection";
            this.PanelBody.ResumeLayout(false);
            this.PanelBody.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CharacterPicker CharPicker;
    }
}
