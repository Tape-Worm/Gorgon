namespace Tester2
{
    partial class TestForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestForm));
			((System.ComponentModel.ISupportInitialize)(this.ConfigurationSettings)).BeginInit();
			this.SuspendLayout();
			// 
			// TestForm
			// 
			this.ApplicationName = "Tester2";
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(640, 480);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(160, 120);
			this.Name = "TestForm";
			this.Text = "Tester";
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this._control_MouseMove);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TestForm_KeyDown);
			((System.ComponentModel.ISupportInitialize)(this.ConfigurationSettings)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion


	}
}