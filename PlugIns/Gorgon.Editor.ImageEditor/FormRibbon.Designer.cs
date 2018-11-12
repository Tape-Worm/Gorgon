namespace Gorgon.Editor.ImageEditor
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
            this.RibbonImageContent = new ComponentFactory.Krypton.Ribbon.KryptonRibbon();
            this.TabImage = new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab();
            this.kryptonRibbonGroup1 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.kryptonRibbonGroupTriple1 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.ButtonSaveImage = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.ButtonCancelChanges = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            ((System.ComponentModel.ISupportInitialize)(this.RibbonImageContent)).BeginInit();
            this.SuspendLayout();
            // 
            // RibbonImageContent
            // 
            this.RibbonImageContent.AllowFormIntegrate = true;
            this.RibbonImageContent.InDesignHelperMode = true;
            this.RibbonImageContent.Name = "RibbonImageContent";
            this.RibbonImageContent.RibbonTabs.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab[] {
            this.TabImage});
            this.RibbonImageContent.SelectedTab = this.TabImage;
            this.RibbonImageContent.Size = new System.Drawing.Size(1161, 115);
            this.RibbonImageContent.TabIndex = 0;
            // 
            // TabImage
            // 
            this.TabImage.Groups.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup[] {
            this.kryptonRibbonGroup1});
            this.TabImage.KeyTip = "I";
            this.TabImage.Text = "Image";
            // 
            // kryptonRibbonGroup1
            // 
            this.kryptonRibbonGroup1.AllowCollapsed = false;
            this.kryptonRibbonGroup1.DialogBoxLauncher = false;
            this.kryptonRibbonGroup1.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
            this.kryptonRibbonGroupTriple1});
            this.kryptonRibbonGroup1.KeyTipGroup = "F";
            this.kryptonRibbonGroup1.TextLine1 = "File";
            // 
            // kryptonRibbonGroupTriple1
            // 
            this.kryptonRibbonGroupTriple1.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonSaveImage,
            this.ButtonCancelChanges});
            this.kryptonRibbonGroupTriple1.MinimumSize = ComponentFactory.Krypton.Ribbon.GroupItemSize.Large;
            // 
            // ButtonSaveImage
            // 
            this.ButtonSaveImage.Enabled = false;
            this.ButtonSaveImage.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.save_content_48x48;
            this.ButtonSaveImage.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.save_content_16x16;
            this.ButtonSaveImage.KeyTip = "S";
            this.ButtonSaveImage.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.ButtonSaveImage.TextLine1 = "Save";
            this.ButtonSaveImage.TextLine2 = " Image";
            this.ButtonSaveImage.ToolTipBody = "Updates the image file in the file system with the current changes.";
            this.ButtonSaveImage.ToolTipTitle = "Save Image";
            // 
            // ButtonCancelChanges
            // 
            this.ButtonCancelChanges.ImageLarge = global::Gorgon.Editor.ImageEditor.Properties.Resources.cancel_48x48;
            this.ButtonCancelChanges.ImageSmall = global::Gorgon.Editor.ImageEditor.Properties.Resources.cancel_16x16;
            this.ButtonCancelChanges.KeyTip = "C";
            this.ButtonCancelChanges.TextLine1 = "Cancel";
            this.ButtonCancelChanges.ToolTipBody = "Exit the image editor without saving any changes.";
            this.ButtonCancelChanges.ToolTipTitle = "Cancel Editing";
            this.ButtonCancelChanges.Click += new System.EventHandler(this.ButtonCancelChanges_Click);
            // 
            // FormRibbon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1161, 296);
            this.Controls.Add(this.RibbonImageContent);
            this.Name = "FormRibbon";
            this.Text = "FormRibbon";
            ((System.ComponentModel.ISupportInitialize)(this.RibbonImageContent)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonTab TabImage;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup kryptonRibbonGroup1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonSaveImage;
        internal ComponentFactory.Krypton.Ribbon.KryptonRibbon RibbonImageContent;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonCancelChanges;
    }
}