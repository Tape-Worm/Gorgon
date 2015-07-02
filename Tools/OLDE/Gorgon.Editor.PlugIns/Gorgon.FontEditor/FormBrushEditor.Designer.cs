using System.ComponentModel;
using System.Windows.Forms;
using Gorgon.Editor.FontEditorPlugIn.Controls;

namespace Gorgon.Editor.FontEditorPlugIn
{
    partial class FormBrushEditor
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBrushEditor));
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.tabBrushEditor = new KRBTabControl.KRBTabControl();
			this.pageTexture = new KRBTabControl.TabPageEx();
			this.panelTextureEditor = new Gorgon.Editor.FontEditorPlugIn.Controls.PanelTexture();
			this.pageSolid = new KRBTabControl.TabPageEx();
			this.colorSolidBrush = new Fetze.WinFormsColor.ColorPickerPanel();
			this.pagePattern = new KRBTabControl.TabPageEx();
			this.panelHatchEditor = new Gorgon.Editor.FontEditorPlugIn.Controls.PanelHatch();
			this.pageGradient = new KRBTabControl.TabPageEx();
			this.panelGradEditor = new Gorgon.Editor.FontEditorPlugIn.Controls.PanelGradient();
			this.labelInfo = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.comboBrushType = new System.Windows.Forms.ComboBox();
			this.labelBrushType = new System.Windows.Forms.Label();
			this.imageFileBrowser = new Gorgon.Editor.EditorFileDialog();
			this.tabBrushEditor.SuspendLayout();
			this.pageTexture.SuspendLayout();
			this.pageSolid.SuspendLayout();
			this.pagePattern.SuspendLayout();
			this.pageGradient.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.Image = global::Gorgon.Editor.FontEditorPlugIn.Properties.Resources.cancel_16x16;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(511, 9);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 28);
			this.buttonCancel.TabIndex = 2;
			this.buttonCancel.Text = "cancel";
			this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonCancel.UseVisualStyleBackColor = false;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonOK.ForeColor = System.Drawing.Color.White;
			this.buttonOK.Image = global::Gorgon.Editor.FontEditorPlugIn.Properties.Resources.ok_16x16;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(418, 9);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(87, 28);
			this.buttonOK.TabIndex = 1;
			this.buttonOK.Text = "ok";
			this.buttonOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonOK.UseVisualStyleBackColor = false;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// tabBrushEditor
			// 
			this.tabBrushEditor.Alignment = System.Windows.Forms.TabAlignment.Bottom;
			this.tabBrushEditor.Alignments = KRBTabControl.KRBTabControl.TabAlignments.Bottom;
			this.tabBrushEditor.AllowDrop = true;
			this.tabBrushEditor.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
			this.tabBrushEditor.BackgroundHatcher.HatchType = System.Drawing.Drawing2D.HatchStyle.DashedVertical;
			this.tabBrushEditor.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
			this.tabBrushEditor.Controls.Add(this.pageTexture);
			this.tabBrushEditor.Controls.Add(this.pageSolid);
			this.tabBrushEditor.Controls.Add(this.pagePattern);
			this.tabBrushEditor.Controls.Add(this.pageGradient);
			this.tabBrushEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabBrushEditor.HeaderVisibility = true;
			this.tabBrushEditor.IsCaptionVisible = false;
			this.tabBrushEditor.IsDocumentTabStyle = true;
			this.tabBrushEditor.IsDrawHeader = false;
			this.tabBrushEditor.IsUserInteraction = false;
			this.tabBrushEditor.ItemSize = new System.Drawing.Size(0, 28);
			this.tabBrushEditor.Location = new System.Drawing.Point(1, 56);
			this.tabBrushEditor.Multiline = true;
			this.tabBrushEditor.Name = "tabBrushEditor";
			this.tabBrushEditor.SelectedIndex = 1;
			this.tabBrushEditor.Size = new System.Drawing.Size(610, 338);
			this.tabBrushEditor.TabBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
			this.tabBrushEditor.TabGradient.ColorEnd = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
			this.tabBrushEditor.TabGradient.ColorStart = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
			this.tabBrushEditor.TabGradient.TabPageSelectedTextColor = System.Drawing.Color.White;
			this.tabBrushEditor.TabGradient.TabPageTextColor = System.Drawing.Color.White;
			this.tabBrushEditor.TabHOffset = -1;
			this.tabBrushEditor.TabIndex = 0;
			this.tabBrushEditor.UpDownStyle = KRBTabControl.KRBTabControl.UpDown32Style.BlackGlass;
			this.tabBrushEditor.SelectedIndexChanged += new System.EventHandler(this.tabBrushEditor_SelectedIndexChanged);
			// 
			// pageTexture
			// 
			this.pageTexture.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
			this.pageTexture.Controls.Add(this.panelTextureEditor);
			this.pageTexture.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageTexture.ForeColor = System.Drawing.Color.White;
			this.pageTexture.IsClosable = false;
			this.pageTexture.Location = new System.Drawing.Point(1, 1);
			this.pageTexture.Name = "pageTexture";
			this.pageTexture.Size = new System.Drawing.Size(608, 336);
			this.pageTexture.TabIndex = 0;
			this.pageTexture.Text = "Texture";
			// 
			// panelTextureEditor
			// 
			this.panelTextureEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelTextureEditor.Location = new System.Drawing.Point(0, 0);
			this.panelTextureEditor.Name = "panelTextureEditor";
			this.panelTextureEditor.Size = new System.Drawing.Size(608, 336);
			this.panelTextureEditor.TabIndex = 0;
			this.panelTextureEditor.BrushChanged += new System.EventHandler(this.BrushChanged);
			// 
			// pageSolid
			// 
			this.pageSolid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.pageSolid.Controls.Add(this.colorSolidBrush);
			this.pageSolid.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.pageSolid.ForeColor = System.Drawing.Color.White;
			this.pageSolid.IsClosable = false;
			this.pageSolid.Location = new System.Drawing.Point(1, 1);
			this.pageSolid.Name = "pageSolid";
			this.pageSolid.Size = new System.Drawing.Size(608, 336);
			this.pageSolid.TabIndex = 1;
			this.pageSolid.Text = "Solid";
			// 
			// colorSolidBrush
			// 
			this.colorSolidBrush.AlphaEnabled = true;
			this.colorSolidBrush.Dock = System.Windows.Forms.DockStyle.Fill;
			this.colorSolidBrush.Location = new System.Drawing.Point(0, 0);
			this.colorSolidBrush.Name = "colorSolidBrush";
			this.colorSolidBrush.OldColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.colorSolidBrush.PrimaryAttribute = Fetze.WinFormsColor.ColorPickerPanel.PrimaryAttrib.Hue;
			this.colorSolidBrush.SelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.colorSolidBrush.Size = new System.Drawing.Size(608, 336);
			this.colorSolidBrush.TabIndex = 0;
			this.colorSolidBrush.ColorChanged += new System.EventHandler(this.colorSolidBrush_ColorChanged);
			// 
			// pagePattern
			// 
			this.pagePattern.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
			this.pagePattern.Controls.Add(this.panelHatchEditor);
			this.pagePattern.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.pagePattern.ForeColor = System.Drawing.Color.White;
			this.pagePattern.IsClosable = false;
			this.pagePattern.Location = new System.Drawing.Point(1, 1);
			this.pagePattern.Name = "pagePattern";
			this.pagePattern.Size = new System.Drawing.Size(608, 336);
			this.pagePattern.TabIndex = 3;
			this.pagePattern.Text = "Pattern";
			// 
			// panelHatchEditor
			// 
			this.panelHatchEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelHatchEditor.Location = new System.Drawing.Point(0, 0);
			this.panelHatchEditor.Name = "panelHatchEditor";
			this.panelHatchEditor.Size = new System.Drawing.Size(608, 336);
			this.panelHatchEditor.TabIndex = 0;
			this.panelHatchEditor.BrushChanged += new System.EventHandler(this.BrushChanged);
			// 
			// pageGradient
			// 
			this.pageGradient.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
			this.pageGradient.Controls.Add(this.panelGradEditor);
			this.pageGradient.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.pageGradient.ForeColor = System.Drawing.Color.White;
			this.pageGradient.IsClosable = false;
			this.pageGradient.Location = new System.Drawing.Point(1, 1);
			this.pageGradient.Name = "pageGradient";
			this.pageGradient.Size = new System.Drawing.Size(608, 336);
			this.pageGradient.TabIndex = 2;
			this.pageGradient.Text = "Gradient";
			// 
			// panelGradEditor
			// 
			this.panelGradEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelGradEditor.Location = new System.Drawing.Point(0, 0);
			this.panelGradEditor.Name = "panelGradEditor";
			this.panelGradEditor.Size = new System.Drawing.Size(608, 336);
			this.panelGradEditor.TabIndex = 0;
			this.panelGradEditor.BrushChanged += new System.EventHandler(this.BrushChanged);
			// 
			// labelInfo
			// 
			this.labelInfo.Location = new System.Drawing.Point(326, 117);
			this.labelInfo.Name = "labelInfo";
			this.labelInfo.Size = new System.Drawing.Size(259, 183);
			this.labelInfo.TabIndex = 7;
			this.labelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panel1
			// 
			this.panel1.AutoSize = true;
			this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panel1.Controls.Add(this.buttonOK);
			this.panel1.Controls.Add(this.buttonCancel);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(1, 394);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
			this.panel1.Size = new System.Drawing.Size(610, 44);
			this.panel1.TabIndex = 5;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.comboBrushType);
			this.panel2.Controls.Add(this.labelBrushType);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(1, 25);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(610, 31);
			this.panel2.TabIndex = 6;
			// 
			// comboBrushType
			// 
			this.comboBrushType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBrushType.FormattingEnabled = true;
			this.comboBrushType.Location = new System.Drawing.Point(75, 5);
			this.comboBrushType.Name = "comboBrushType";
			this.comboBrushType.Size = new System.Drawing.Size(165, 23);
			this.comboBrushType.TabIndex = 1;
			this.comboBrushType.SelectedIndexChanged += new System.EventHandler(this.comboBrushType_SelectedIndexChanged);
			// 
			// labelBrushType
			// 
			this.labelBrushType.AutoSize = true;
			this.labelBrushType.Location = new System.Drawing.Point(3, 8);
			this.labelBrushType.Name = "labelBrushType";
			this.labelBrushType.Size = new System.Drawing.Size(63, 15);
			this.labelBrushType.TabIndex = 0;
			this.labelBrushType.Text = "brush type";
			// 
			// imageFileBrowser
			// 
			this.imageFileBrowser.Filename = null;
			this.imageFileBrowser.StartDirectory = null;
			this.imageFileBrowser.Text = "Open Image";
			// 
			// FormBrushEditor
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.ShowBorder = true;
			this.BorderColor = System.Drawing.Color.SteelBlue;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(612, 439);
			this.Controls.Add(this.tabBrushEditor);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.ForeColor = System.Drawing.Color.Silver;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.InactiveBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormBrushEditor";
			this.Padding = new System.Windows.Forms.Padding(1);
			this.Resizable = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Brush Editor";
			this.Controls.SetChildIndex(this.panel1, 0);
			this.Controls.SetChildIndex(this.panel2, 0);
			this.Controls.SetChildIndex(this.tabBrushEditor, 0);
			this.tabBrushEditor.ResumeLayout(false);
			this.pageTexture.ResumeLayout(false);
			this.pageSolid.ResumeLayout(false);
			this.pagePattern.ResumeLayout(false);
			this.pageGradient.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		private Button buttonCancel;
        private Button buttonOK;
        private KRBTabControl.KRBTabControl tabBrushEditor;
        private KRBTabControl.TabPageEx pageSolid;
        private KRBTabControl.TabPageEx pageTexture;
		private Panel panel1;
		private Panel panel2;
		private ComboBox comboBrushType;
        private Label labelBrushType;
        private Fetze.WinFormsColor.ColorPickerPanel colorSolidBrush;
		private EditorFileDialog imageFileBrowser;
		private Label labelInfo;
		private KRBTabControl.TabPageEx pageGradient;
		private PanelGradient panelGradEditor;
		private KRBTabControl.TabPageEx pagePattern;
		private PanelHatch panelHatchEditor;
		private PanelTexture panelTextureEditor;
    }
}