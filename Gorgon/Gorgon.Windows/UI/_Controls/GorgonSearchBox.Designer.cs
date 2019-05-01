namespace Gorgon.UI
{
    partial class GorgonSearchBox
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ButtonClearSearch = new System.Windows.Forms.Button();
            this.TextSearch = new Gorgon.UI.GorgonCueTextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // ButtonClearSearch
            // 
            this.ButtonClearSearch.AutoSize = true;
            this.ButtonClearSearch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonClearSearch.BackColor = System.Drawing.Color.Transparent;
            this.ButtonClearSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonClearSearch.FlatAppearance.BorderSize = 0;
            this.ButtonClearSearch.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonClearSearch.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(86)))), ((int)(((byte)(86)))), ((int)(((byte)(86)))));
            this.ButtonClearSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonClearSearch.Font = new System.Drawing.Font("Marlett", 9F);
            this.ButtonClearSearch.Location = new System.Drawing.Point(0, 0);
            this.ButtonClearSearch.Name = "ButtonClearSearch";
            this.ButtonClearSearch.Size = new System.Drawing.Size(27, 24);
            this.ButtonClearSearch.TabIndex = 1;
            this.ButtonClearSearch.Text = "r";
            this.ButtonClearSearch.UseVisualStyleBackColor = false;
            this.ButtonClearSearch.Visible = false;
            this.ButtonClearSearch.Click += new System.EventHandler(this.ButtonClearSearch_Click);
            // 
            // TextSearch
            // 
            this.TextSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextSearch.CueText = "Search...";
            this.TextSearch.Location = new System.Drawing.Point(3, 4);
            this.TextSearch.Name = "TextSearch";
            this.TextSearch.Size = new System.Drawing.Size(253, 16);
            this.TextSearch.TabIndex = 0;
            this.TextSearch.TextChanged += new System.EventHandler(this.TextSearch_TextChanged);
            this.TextSearch.Enter += new System.EventHandler(this.TextSearch_Enter);
            this.TextSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextSearch_KeyUp);
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.TextSearch);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(262, 24);
            this.panel1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.AutoSize = true;
            this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel2.Controls.Add(this.ButtonClearSearch);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(262, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(27, 24);
            this.panel2.TabIndex = 1;
            // 
            // GorgonSearchBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "GorgonSearchBox";
            this.Size = new System.Drawing.Size(289, 24);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GorgonCueTextBox TextSearch;
        private System.Windows.Forms.Button ButtonClearSearch;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
    }
}
