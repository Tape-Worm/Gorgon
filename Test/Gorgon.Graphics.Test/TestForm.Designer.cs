using System.Windows.Forms;

namespace GorgonLibrary.Graphics.Test
{
    partial class TestForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
		private System.ComponentModel.IContainer components = null;
        private Button buttonWrong;
        private Button buttonYes;
        public Panel panelDisplay;

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
			this.panelDisplay = new System.Windows.Forms.Panel();
			this.panelInput = new System.Windows.Forms.Panel();
			this.buttonWrong = new System.Windows.Forms.Button();
			this.buttonYes = new System.Windows.Forms.Button();
			this.panelInput.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelDisplay
			// 
			this.panelDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelDisplay.Location = new System.Drawing.Point(0, 0);
			this.panelDisplay.Name = "panelDisplay";
			this.panelDisplay.Size = new System.Drawing.Size(484, 322);
			this.panelDisplay.TabIndex = 0;
			// 
			// panelInput
			// 
			this.panelInput.Controls.Add(this.buttonWrong);
			this.panelInput.Controls.Add(this.buttonYes);
			this.panelInput.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelInput.Location = new System.Drawing.Point(0, 322);
			this.panelInput.Name = "panelInput";
			this.panelInput.Size = new System.Drawing.Size(484, 38);
			this.panelInput.TabIndex = 1;
			// 
			// buttonWrong
			// 
			this.buttonWrong.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonWrong.Location = new System.Drawing.Point(379, 3);
			this.buttonWrong.Name = "buttonWrong";
			this.buttonWrong.Size = new System.Drawing.Size(93, 23);
			this.buttonWrong.TabIndex = 1;
			this.buttonWrong.Text = "Looks wrong";
			this.buttonWrong.UseVisualStyleBackColor = true;
			this.buttonWrong.Click += new System.EventHandler(this.buttonWrong_Click);
			// 
			// buttonYes
			// 
			this.buttonYes.Location = new System.Drawing.Point(13, 3);
			this.buttonYes.Name = "buttonYes";
			this.buttonYes.Size = new System.Drawing.Size(75, 23);
			this.buttonYes.TabIndex = 0;
			this.buttonYes.Text = "Looks right";
			this.buttonYes.UseVisualStyleBackColor = true;
			this.buttonYes.Click += new System.EventHandler(this.buttonYes_Click);
			// 
			// TestForm
			// 
			this.ClientSize = new System.Drawing.Size(484, 360);
			this.Controls.Add(this.panelDisplay);
			this.Controls.Add(this.panelInput);
			this.Name = "TestForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.panelInput.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

		public Panel panelInput;
 

    }
}