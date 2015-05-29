using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Editor
{
    partial class PreferencePanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolHelp = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // toolHelp
            // 
            this.toolHelp.BackColor = System.Drawing.Color.White;
            this.toolHelp.ForeColor = System.Drawing.Color.Black;
            this.toolHelp.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolHelp.ToolTipTitle = "Help";
            // 
            // PreferencePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "PreferencePanel";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Size = new System.Drawing.Size(591, 442);
            this.ResumeLayout(false);

        }

        #endregion

		/// <summary>
		/// Tool tip for controls on the panel.
		/// </summary>
        protected ToolTip toolHelp;

    }
}
