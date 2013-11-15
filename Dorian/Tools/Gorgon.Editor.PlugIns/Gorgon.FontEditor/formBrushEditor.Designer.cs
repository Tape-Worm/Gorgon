﻿namespace GorgonLibrary.Editor.FontEditorPlugIn
{
    partial class formBrushEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formBrushEditor));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.tabBrushEditor = new KRBTabControl.KRBTabControl();
            this.pageSolid = new KRBTabControl.TabPageEx();
            this.pageTexture = new KRBTabControl.TabPageEx();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.comboBrushType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.colorSolidBrush = new Fetze.WinFormsColor.ColorPickerPanel();
            this.tabBrushEditor.SuspendLayout();
            this.pageSolid.SuspendLayout();
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
            this.buttonCancel.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.cancel_16x16;
            this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonCancel.Location = new System.Drawing.Point(511, 9);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(87, 28);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
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
            this.buttonOK.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.ok_16x16;
            this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonOK.Location = new System.Drawing.Point(418, 9);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(87, 28);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonOK.UseVisualStyleBackColor = false;
            // 
            // tabBrushEditor
            // 
            this.tabBrushEditor.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabBrushEditor.Alignments = KRBTabControl.KRBTabControl.TabAlignments.Bottom;
            this.tabBrushEditor.AllowDrop = true;
            this.tabBrushEditor.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.tabBrushEditor.BackgroundHatcher.HatchType = System.Drawing.Drawing2D.HatchStyle.DashedVertical;
            this.tabBrushEditor.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.tabBrushEditor.Controls.Add(this.pageSolid);
            this.tabBrushEditor.Controls.Add(this.pageTexture);
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
            this.tabBrushEditor.SelectedIndex = 0;
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
            // pageSolid
            // 
            this.pageSolid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.pageSolid.Controls.Add(this.colorSolidBrush);
            this.pageSolid.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pageSolid.ForeColor = System.Drawing.Color.White;
            this.pageSolid.IsClosable = false;
            this.pageSolid.Location = new System.Drawing.Point(1, 1);
            this.pageSolid.Name = "pageSolid";
            this.pageSolid.Size = new System.Drawing.Size(608, 336);
            this.pageSolid.TabIndex = 1;
            this.pageSolid.Text = "Solid";
            // 
            // pageTexture
            // 
            this.pageTexture.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.pageTexture.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pageTexture.IsClosable = false;
            this.pageTexture.Location = new System.Drawing.Point(1, 1);
            this.pageTexture.Name = "pageTexture";
            this.pageTexture.Size = new System.Drawing.Size(608, 336);
            this.pageTexture.TabIndex = 0;
            this.pageTexture.Text = "Texture";
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
            this.panel2.Controls.Add(this.label1);
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
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Brush type:";
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
            // 
            // formBrushEditor
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.Border = true;
            this.BorderColor = System.Drawing.Color.SteelBlue;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(612, 439);
            this.Controls.Add(this.tabBrushEditor);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.Silver;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formBrushEditor";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.Resizable = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Brush Editor";
            this.Controls.SetChildIndex(this.panel1, 0);
            this.Controls.SetChildIndex(this.panel2, 0);
            this.Controls.SetChildIndex(this.tabBrushEditor, 0);
            this.tabBrushEditor.ResumeLayout(false);
            this.pageSolid.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private KRBTabControl.KRBTabControl tabBrushEditor;
        private KRBTabControl.TabPageEx pageSolid;
        private KRBTabControl.TabPageEx pageTexture;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.ComboBox comboBrushType;
        private System.Windows.Forms.Label label1;
        private Fetze.WinFormsColor.ColorPickerPanel colorSolidBrush;
    }
}