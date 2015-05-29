using System.ComponentModel;
using System.Windows.Forms;
using Gorgon.UI;

namespace Gorgon.Editor.FontEditorPlugIn.Controls
{
	partial class PanelHatch
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

		    if (disposing)
		    {
			    if (comboHatch != null)
			    {
				    comboHatch.SelectedIndexChanged -= comboHatch_SelectedIndexChanged;
			    }

		        if (_previewBitmap != null)
		        {
		            _previewBitmap.Dispose();
		            _previewBitmap = null;
		        }

		        if (_foreColorBitmap != null)
		        {
		            _foreColorBitmap.Dispose();
		            _foreColorBitmap = null;
		        }

		        if (_backColorBitmap != null)
		        {
		            _backColorBitmap.Dispose();
		            _backColorBitmap = null;
		        }
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
            this.panelPreview = new System.Windows.Forms.Panel();
            this.labelPatternType = new System.Windows.Forms.Label();
            this.labelForegroundColor = new System.Windows.Forms.Label();
            this.panelForegroundColor = new Gorgon.UI.GorgonSelectablePanel();
            this.panelBackgroundColor = new Gorgon.UI.GorgonSelectablePanel();
            this.labelBackgroundColor = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.comboHatch = new Gorgon.Editor.FontEditorPlugIn.ComboPatterns();
            this.panel4 = new System.Windows.Forms.Panel();
            this.labelPreview = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelPreview
            // 
            this.panelPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPreview.BackgroundImage = global::Gorgon.Editor.FontEditorPlugIn.Properties.Resources.Pattern;
            this.panelPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPreview.Location = new System.Drawing.Point(6, 23);
            this.panelPreview.Name = "panelPreview";
            this.panelPreview.Size = new System.Drawing.Size(196, 179);
            this.panelPreview.TabIndex = 0;
            this.panelPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.panelPreview_Paint);
            // 
            // labelPatternType
            // 
            this.labelPatternType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPatternType.AutoSize = true;
            this.labelPatternType.Location = new System.Drawing.Point(3, 6);
            this.labelPatternType.Name = "labelPatternType";
            this.labelPatternType.Size = new System.Drawing.Size(63, 13);
            this.labelPatternType.TabIndex = 1;
            this.labelPatternType.Text = "pattern type";
            // 
            // labelForegroundColor
            // 
            this.labelForegroundColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelForegroundColor.AutoSize = true;
            this.labelForegroundColor.Location = new System.Drawing.Point(3, 34);
            this.labelForegroundColor.Name = "labelForegroundColor";
            this.labelForegroundColor.Size = new System.Drawing.Size(58, 13);
            this.labelForegroundColor.TabIndex = 3;
            this.labelForegroundColor.Text = "foreground";
            // 
            // panelForegroundColor
            // 
            this.panelForegroundColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelForegroundColor.BackgroundImage = global::Gorgon.Editor.FontEditorPlugIn.Properties.Resources.Pattern;
            this.panelForegroundColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelForegroundColor.Location = new System.Drawing.Point(78, 30);
            this.panelForegroundColor.Name = "panelForegroundColor";
            this.panelForegroundColor.Size = new System.Drawing.Size(211, 21);
            this.panelForegroundColor.TabIndex = 1;
            this.panelForegroundColor.Paint += new System.Windows.Forms.PaintEventHandler(this.panelForegroundColor_Paint);
            this.panelForegroundColor.DoubleClick += new System.EventHandler(this.panelForegroundColor_DoubleClick);
            this.panelForegroundColor.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.panelForegroundColor_PreviewKeyDown);
            // 
            // panelBackgroundColor
            // 
            this.panelBackgroundColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBackgroundColor.BackgroundImage = global::Gorgon.Editor.FontEditorPlugIn.Properties.Resources.Pattern;
            this.panelBackgroundColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBackgroundColor.Location = new System.Drawing.Point(78, 57);
            this.panelBackgroundColor.Name = "panelBackgroundColor";
            this.panelBackgroundColor.Size = new System.Drawing.Size(211, 21);
            this.panelBackgroundColor.TabIndex = 2;
            this.panelBackgroundColor.Paint += new System.Windows.Forms.PaintEventHandler(this.panelBackgroundColor_Paint);
            this.panelBackgroundColor.DoubleClick += new System.EventHandler(this.panelBackgroundColor_DoubleClick);
            this.panelBackgroundColor.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.panelBackgroundColor_PreviewKeyDown);
            // 
            // labelBackgroundColor
            // 
            this.labelBackgroundColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelBackgroundColor.AutoSize = true;
            this.labelBackgroundColor.Location = new System.Drawing.Point(3, 61);
            this.labelBackgroundColor.Name = "labelBackgroundColor";
            this.labelBackgroundColor.Size = new System.Drawing.Size(64, 13);
            this.labelBackgroundColor.TabIndex = 5;
            this.labelBackgroundColor.Text = "background";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panelBackgroundColor);
            this.panel1.Controls.Add(this.labelPatternType);
            this.panel1.Controls.Add(this.labelBackgroundColor);
            this.panel1.Controls.Add(this.comboHatch);
            this.panel1.Controls.Add(this.panelForegroundColor);
            this.panel1.Controls.Add(this.labelForegroundColor);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(208, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(292, 208);
            this.panel1.TabIndex = 6;
            // 
            // comboHatch
            // 
            this.comboHatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboHatch.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboHatch.FormattingEnabled = true;
            this.comboHatch.Location = new System.Drawing.Point(78, 3);
            this.comboHatch.Name = "comboHatch";
            this.comboHatch.Size = new System.Drawing.Size(211, 21);
            this.comboHatch.Style = System.Drawing.Drawing2D.HatchStyle.BackwardDiagonal;
            this.comboHatch.TabIndex = 0;
            // 
            // panel4
            // 
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.labelPreview);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(6, 6);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(196, 18);
            this.panel4.TabIndex = 1;
            // 
            // labelPreview
            // 
            this.labelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelPreview.Location = new System.Drawing.Point(0, 0);
            this.labelPreview.Name = "labelPreview";
            this.labelPreview.Size = new System.Drawing.Size(194, 16);
            this.labelPreview.TabIndex = 0;
            this.labelPreview.Text = "preview";
            this.labelPreview.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.panelPreview);
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(6);
            this.panel3.Size = new System.Drawing.Size(208, 208);
            this.panel3.TabIndex = 7;
            // 
            // PanelHatch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Name = "PanelHatch";
            this.Size = new System.Drawing.Size(500, 208);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private Panel panelPreview;
		private Label labelPatternType;
		private ComboPatterns comboHatch;
		private Label labelForegroundColor;
		private GorgonSelectablePanel panelForegroundColor;
		private GorgonSelectablePanel panelBackgroundColor;
		private Label labelBackgroundColor;
		private Panel panel1;
		private Panel panel4;
		private Label labelPreview;
		private Panel panel3;
	}
}
