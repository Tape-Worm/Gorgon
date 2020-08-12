namespace Gorgon.Examples
{
    partial class FormRibbon
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

                if (_contentRenderer != null)
                {
                    _contentRenderer.ZoomScaleChanged -= ContentRenderer_ZoomScale;
                }

                DataContext = null;
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
            this.components = new System.ComponentModel.Container();
            this.RibbonTextContent = new ComponentFactory.Krypton.Ribbon.KryptonRibbon();
            this.TabText = new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab();
            this.GroupImageFile = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.kryptonRibbonGroupTriple1 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.ButtonSaveText = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.GroupTextEdit = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.kryptonRibbonGroupLines2 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
            this.ButtonTextUndo = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.ButtonTextRedo = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.GroupText = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.kryptonRibbonGroupTriple2 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.ButtonFont = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.MenuFonts = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ItemArial = new System.Windows.Forms.ToolStripMenuItem();
            this.ItemTimesNewRoman = new System.Windows.Forms.ToolStripMenuItem();
            this.ItemPapyrus = new System.Windows.Forms.ToolStripMenuItem();
            this.kryptonRibbonGroupLines1 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
            this.ButtonTextColor = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.ButtonChangeText = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.kryptonRibbonGroupSeparator3 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator();
            this.kryptonRibbonGroupLines3 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
            this.ButtonZoom = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.MenuZoom = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ItemZoomToWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.Item12Percent = new System.Windows.Forms.ToolStripMenuItem();
            this.Item25Percent = new System.Windows.Forms.ToolStripMenuItem();
            this.Item50Percent = new System.Windows.Forms.ToolStripMenuItem();
            this.Item100Percent = new System.Windows.Forms.ToolStripMenuItem();
            this.Item200Percent = new System.Windows.Forms.ToolStripMenuItem();
            this.Item400Percent = new System.Windows.Forms.ToolStripMenuItem();
            this.Item800Percent = new System.Windows.Forms.ToolStripMenuItem();
            this.Item1600Percent = new System.Windows.Forms.ToolStripMenuItem();
            this.Item3200Percent = new System.Windows.Forms.ToolStripMenuItem();
            this.Item6400Percent = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.RibbonTextContent)).BeginInit();
            this.MenuFonts.SuspendLayout();
            this.MenuZoom.SuspendLayout();
            this.SuspendLayout();
            // 
            // RibbonTextContent
            // 
            this.RibbonTextContent.AllowFormIntegrate = true;
            this.RibbonTextContent.InDesignHelperMode = true;
            this.RibbonTextContent.Name = "RibbonTextContent";
            this.RibbonTextContent.RibbonTabs.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab[] {
            this.TabText});
            this.RibbonTextContent.SelectedContext = null;
            this.RibbonTextContent.SelectedTab = this.TabText;
            this.RibbonTextContent.Size = new System.Drawing.Size(1293, 115);
            this.RibbonTextContent.TabIndex = 0;
            // 
            // TabText
            // 
            this.TabText.Groups.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup[] {
            this.GroupImageFile,
            this.GroupTextEdit,
            this.GroupText});
            this.TabText.Text = "Text";
            // 
            // GroupImageFile
            // 
            this.GroupImageFile.AllowCollapsed = false;
            this.GroupImageFile.DialogBoxLauncher = false;
            this.GroupImageFile.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
            this.kryptonRibbonGroupTriple1});
            this.GroupImageFile.KeyTipGroup = "F";
            this.GroupImageFile.TextLine1 = "File";
            // 
            // kryptonRibbonGroupTriple1
            // 
            this.kryptonRibbonGroupTriple1.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonSaveText});
            this.kryptonRibbonGroupTriple1.MinimumSize = ComponentFactory.Krypton.Ribbon.GroupItemSize.Large;
            // 
            // ButtonSaveText
            // 
            this.ButtonSaveText.Enabled = false;
            this.ButtonSaveText.ImageLarge = global::Gorgon.Examples.Properties.Resources.save_content_48x48;
            this.ButtonSaveText.ImageSmall = global::Gorgon.Examples.Properties.Resources.save_content_16x16;
            this.ButtonSaveText.KeyTip = "S";
            this.ButtonSaveText.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.ButtonSaveText.TextLine1 = "Save";
            this.ButtonSaveText.TextLine2 = "Text";
            this.ButtonSaveText.ToolTipBody = "Updates the text file in the file system with the current changes.";
            this.ButtonSaveText.ToolTipTitle = "Save";
            this.ButtonSaveText.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // GroupTextEdit
            // 
            this.GroupTextEdit.AllowCollapsed = false;
            this.GroupTextEdit.DialogBoxLauncher = false;
            this.GroupTextEdit.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
            this.kryptonRibbonGroupLines2});
            this.GroupTextEdit.KeyTipGroup = "E";
            this.GroupTextEdit.TextLine1 = "Edit";
            // 
            // kryptonRibbonGroupLines2
            // 
            this.kryptonRibbonGroupLines2.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonTextUndo,
            this.ButtonTextRedo});
            // 
            // ButtonTextUndo
            // 
            this.ButtonTextUndo.ImageLarge = global::Gorgon.Examples.Properties.Resources.undo_48x48;
            this.ButtonTextUndo.ImageSmall = global::Gorgon.Examples.Properties.Resources.undo_16x16;
            this.ButtonTextUndo.KeyTip = "Z";
            this.ButtonTextUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.ButtonTextUndo.TextLine1 = "Undo";
            this.ButtonTextUndo.ToolTipBody = "Restores the text back to the original text before it was changed.";
            this.ButtonTextUndo.ToolTipTitle = "Undo";
            this.ButtonTextUndo.Click += new System.EventHandler(this.ButtonTextUndo_Click);
            // 
            // ButtonTextRedo
            // 
            this.ButtonTextRedo.ImageLarge = global::Gorgon.Examples.Properties.Resources.redo_48x48;
            this.ButtonTextRedo.ImageSmall = global::Gorgon.Examples.Properties.Resources.redo_16x16;
            this.ButtonTextRedo.KeyTip = "Y";
            this.ButtonTextRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.ButtonTextRedo.TextLine1 = "Redo";
            this.ButtonTextRedo.ToolTipBody = "Resets the text to the changed text.";
            this.ButtonTextRedo.ToolTipTitle = "Redo";
            this.ButtonTextRedo.Click += new System.EventHandler(this.ButtonTextRedo_Click);
            // 
            // GroupText
            // 
            this.GroupText.AllowCollapsed = false;
            this.GroupText.DialogBoxLauncher = false;
            this.GroupText.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
            this.kryptonRibbonGroupTriple2,
            this.kryptonRibbonGroupLines1,
            this.kryptonRibbonGroupSeparator3,
            this.kryptonRibbonGroupLines3});
            this.GroupText.KeyTipGroup = "T";
            this.GroupText.TextLine1 = "Text";
            // 
            // kryptonRibbonGroupTriple2
            // 
            this.kryptonRibbonGroupTriple2.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonFont});
            // 
            // ButtonFont
            // 
            this.ButtonFont.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.DropDown;
            this.ButtonFont.ContextMenuStrip = this.MenuFonts;
            this.ButtonFont.ImageLarge = global::Gorgon.Examples.Properties.Resources.font_48x48;
            this.ButtonFont.ImageSmall = global::Gorgon.Examples.Properties.Resources.font_16x16;
            this.ButtonFont.KeyTip = "F";
            this.ButtonFont.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.ButtonFont.TextLine1 = "Font";
            this.ButtonFont.ToolTipBody = "Sets the font used to render the text.";
            this.ButtonFont.ToolTipTitle = "Font";
            // 
            // MenuFonts
            // 
            this.MenuFonts.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MenuFonts.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ItemArial,
            this.ItemTimesNewRoman,
            this.ItemPapyrus});
            this.MenuFonts.Name = "MenuImageType";
            this.MenuFonts.Size = new System.Drawing.Size(174, 70);
            // 
            // ItemArial
            // 
            this.ItemArial.Checked = true;
            this.ItemArial.CheckOnClick = true;
            this.ItemArial.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ItemArial.Name = "ItemArial";
            this.ItemArial.Size = new System.Drawing.Size(173, 22);
            this.ItemArial.Text = "Arial";
            this.ItemArial.Click += new System.EventHandler(this.Font_Click);
            // 
            // ItemTimesNewRoman
            // 
            this.ItemTimesNewRoman.CheckOnClick = true;
            this.ItemTimesNewRoman.Name = "ItemTimesNewRoman";
            this.ItemTimesNewRoman.Size = new System.Drawing.Size(173, 22);
            this.ItemTimesNewRoman.Text = "Times New Roman";
            this.ItemTimesNewRoman.Click += new System.EventHandler(this.Font_Click);
            // 
            // ItemPapyrus
            // 
            this.ItemPapyrus.CheckOnClick = true;
            this.ItemPapyrus.Name = "ItemPapyrus";
            this.ItemPapyrus.Size = new System.Drawing.Size(173, 22);
            this.ItemPapyrus.Text = "Papyrus";
            this.ItemPapyrus.Click += new System.EventHandler(this.Font_Click);
            // 
            // kryptonRibbonGroupLines1
            // 
            this.kryptonRibbonGroupLines1.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonTextColor,
            this.ButtonChangeText});
            // 
            // ButtonTextColor
            // 
            this.ButtonTextColor.ImageLarge = global::Gorgon.Examples.Properties.Resources.color_48x48;
            this.ButtonTextColor.ImageSmall = global::Gorgon.Examples.Properties.Resources.color_16x16;
            this.ButtonTextColor.KeyTip = "C";
            this.ButtonTextColor.TextLine1 = "Text";
            this.ButtonTextColor.TextLine2 = "Color";
            this.ButtonTextColor.ToolTipBody = "Changes the color of the text.";
            this.ButtonTextColor.ToolTipTitle = "Color";
            this.ButtonTextColor.Click += new System.EventHandler(this.ButtonTextColor_Click);
            // 
            // ButtonChangeText
            // 
            this.ButtonChangeText.ImageLarge = global::Gorgon.Examples.Properties.Resources.change_text_48x48;
            this.ButtonChangeText.ImageSmall = global::Gorgon.Examples.Properties.Resources.change_text_16x16;
            this.ButtonChangeText.KeyTip = "T";
            this.ButtonChangeText.TextLine1 = "Change";
            this.ButtonChangeText.TextLine2 = "Text";
            this.ButtonChangeText.ToolTipBody = "Opens up the text input panel so the user can input new text.";
            this.ButtonChangeText.ToolTipTitle = "Change Text";
            this.ButtonChangeText.Click += new System.EventHandler(this.ButtonChangeText_Click);
            // 
            // kryptonRibbonGroupLines3
            // 
            this.kryptonRibbonGroupLines3.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonZoom});
            // 
            // ButtonZoom
            // 
            this.ButtonZoom.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.DropDown;
            this.ButtonZoom.ContextMenuStrip = this.MenuZoom;
            this.ButtonZoom.ImageLarge = global::Gorgon.Examples.Properties.Resources.zoom_48x48;
            this.ButtonZoom.ImageSmall = global::Gorgon.Examples.Properties.Resources.zoom_16x16;
            this.ButtonZoom.KeyTip = "Z";
            this.ButtonZoom.TextLine1 = "Zoom";
            this.ButtonZoom.ToolTipBody = "Zooms in or out on the content by the specified percentage.";
            this.ButtonZoom.ToolTipTitle = "Zoom";
            // 
            // MenuZoom
            // 
            this.MenuZoom.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MenuZoom.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ItemZoomToWindow,
            this.toolStripMenuItem1,
            this.Item12Percent,
            this.Item25Percent,
            this.Item50Percent,
            this.Item100Percent,
            this.Item200Percent,
            this.Item400Percent,
            this.Item800Percent,
            this.Item1600Percent,
            this.Item3200Percent,
            this.Item6400Percent});
            this.MenuZoom.Name = "ZoomMenu";
            this.MenuZoom.Size = new System.Drawing.Size(134, 252);
            // 
            // ItemZoomToWindow
            // 
            this.ItemZoomToWindow.Checked = true;
            this.ItemZoomToWindow.CheckOnClick = true;
            this.ItemZoomToWindow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ItemZoomToWindow.Name = "ItemZoomToWindow";
            this.ItemZoomToWindow.Size = new System.Drawing.Size(133, 22);
            this.ItemZoomToWindow.Tag = "ToWindow";
            this.ItemZoomToWindow.Text = "To Window";
            this.ItemZoomToWindow.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(130, 6);
            // 
            // Item12Percent
            // 
            this.Item12Percent.CheckOnClick = true;
            this.Item12Percent.Name = "Item12Percent";
            this.Item12Percent.Size = new System.Drawing.Size(133, 22);
            this.Item12Percent.Tag = "Percent12";
            this.Item12Percent.Text = "12.5%";
            this.Item12Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item25Percent
            // 
            this.Item25Percent.CheckOnClick = true;
            this.Item25Percent.Name = "Item25Percent";
            this.Item25Percent.Size = new System.Drawing.Size(133, 22);
            this.Item25Percent.Tag = "Percent25";
            this.Item25Percent.Text = "25%";
            this.Item25Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item50Percent
            // 
            this.Item50Percent.CheckOnClick = true;
            this.Item50Percent.Name = "Item50Percent";
            this.Item50Percent.Size = new System.Drawing.Size(133, 22);
            this.Item50Percent.Tag = "Percent50";
            this.Item50Percent.Text = "50%";
            this.Item50Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item100Percent
            // 
            this.Item100Percent.CheckOnClick = true;
            this.Item100Percent.Name = "Item100Percent";
            this.Item100Percent.Size = new System.Drawing.Size(133, 22);
            this.Item100Percent.Tag = "Percent100";
            this.Item100Percent.Text = "100%";
            this.Item100Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item200Percent
            // 
            this.Item200Percent.CheckOnClick = true;
            this.Item200Percent.Name = "Item200Percent";
            this.Item200Percent.Size = new System.Drawing.Size(133, 22);
            this.Item200Percent.Tag = "Percent200";
            this.Item200Percent.Text = "200%";
            this.Item200Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item400Percent
            // 
            this.Item400Percent.CheckOnClick = true;
            this.Item400Percent.Name = "Item400Percent";
            this.Item400Percent.Size = new System.Drawing.Size(133, 22);
            this.Item400Percent.Tag = "Percent400";
            this.Item400Percent.Text = "400%";
            this.Item400Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item800Percent
            // 
            this.Item800Percent.CheckOnClick = true;
            this.Item800Percent.Name = "Item800Percent";
            this.Item800Percent.Size = new System.Drawing.Size(133, 22);
            this.Item800Percent.Tag = "Percent800";
            this.Item800Percent.Text = "800%";
            this.Item800Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item1600Percent
            // 
            this.Item1600Percent.CheckOnClick = true;
            this.Item1600Percent.Name = "Item1600Percent";
            this.Item1600Percent.Size = new System.Drawing.Size(133, 22);
            this.Item1600Percent.Tag = "Percent1600";
            this.Item1600Percent.Text = "1600%";
            this.Item1600Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item3200Percent
            // 
            this.Item3200Percent.CheckOnClick = true;
            this.Item3200Percent.Name = "Item3200Percent";
            this.Item3200Percent.Size = new System.Drawing.Size(133, 22);
            this.Item3200Percent.Tag = "Percent3200";
            this.Item3200Percent.Text = "3200%";
            this.Item3200Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // Item6400Percent
            // 
            this.Item6400Percent.CheckOnClick = true;
            this.Item6400Percent.Name = "Item6400Percent";
            this.Item6400Percent.Size = new System.Drawing.Size(133, 22);
            this.Item6400Percent.Tag = "Percent6400";
            this.Item6400Percent.Text = "6400%";
            this.Item6400Percent.Click += new System.EventHandler(this.ItemZoom_Click);
            // 
            // FormRibbon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1293, 399);
            this.Controls.Add(this.RibbonTextContent);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FormRibbon";
            this.Text = "FormRibbon";
            ((System.ComponentModel.ISupportInitialize)(this.RibbonTextContent)).EndInit();
            this.MenuFonts.ResumeLayout(false);
            this.MenuZoom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonTab TabText;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup GroupImageFile;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonSaveText;
        internal ComponentFactory.Krypton.Ribbon.KryptonRibbon RibbonTextContent;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup GroupText;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple2;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonFont;
        private System.Windows.Forms.ContextMenuStrip MenuFonts;
        private System.Windows.Forms.ToolStripMenuItem ItemArial;
        private System.Windows.Forms.ToolStripMenuItem ItemTimesNewRoman;
        private System.Windows.Forms.ToolStripMenuItem ItemPapyrus;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator3;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines3;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonZoom;
        private System.Windows.Forms.ContextMenuStrip MenuZoom;
        private System.Windows.Forms.ToolStripMenuItem ItemZoomToWindow;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem Item12Percent;
        private System.Windows.Forms.ToolStripMenuItem Item25Percent;
        private System.Windows.Forms.ToolStripMenuItem Item50Percent;
        private System.Windows.Forms.ToolStripMenuItem Item100Percent;
        private System.Windows.Forms.ToolStripMenuItem Item200Percent;
        private System.Windows.Forms.ToolStripMenuItem Item400Percent;
        private System.Windows.Forms.ToolStripMenuItem Item800Percent;
        private System.Windows.Forms.ToolStripMenuItem Item1600Percent;
        private System.Windows.Forms.ToolStripMenuItem Item3200Percent;
        private System.Windows.Forms.ToolStripMenuItem Item6400Percent;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonTextColor;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonChangeText;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup GroupTextEdit;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines2;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonTextUndo;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonTextRedo;
    }
}