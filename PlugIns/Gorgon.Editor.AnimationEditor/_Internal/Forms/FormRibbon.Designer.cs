namespace Gorgon.Editor.AnimationEditor;

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
                ContentRenderer.ZoomScaleChanged -= ContentRenderer_ZoomScale;
            }
        }

        base.Dispose(disposing);
    }



    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRibbon));
        this.RibbonAnimationContent = new Krypton.Ribbon.KryptonRibbon();
        this.ContextKeyEditor = new Krypton.Ribbon.KryptonRibbonContext();
        this.TabAnimation = new Krypton.Ribbon.KryptonRibbonTab();
        this.GroupAnimationFile = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple1 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonNewAnimation = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonSaveAnimation = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.GroupAnimationEdit = new Krypton.Ribbon.KryptonRibbonGroup();
        this.LineAnimationUndoRedo = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonAnimationUndo = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonAnimationRedo = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.GroupAnimation = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple6 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonAnimationProperties = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupSeparator6 = new Krypton.Ribbon.KryptonRibbonGroupSeparator();
        this.LineGroupBgImage = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonAnimationLoadBack = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonAnimationClearBack = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.LineGroupAnimation = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonAnimationSprite = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.CheckAnimationLoop = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.GroupTrack = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple4 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.CheckAnimationEditTrack = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupSeparator4 = new Krypton.Ribbon.KryptonRibbonGroupSeparator();
        this.kryptonRibbonGroupTriple3 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonAddTrack = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupLines3 = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonRemoveTrack = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonAnimationClear = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.GroupPreview = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple2 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonAnimPlay = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonAnimStop = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupSeparator3 = new Krypton.Ribbon.KryptonRibbonGroupSeparator();
        this.kryptonRibbonGroupLines1 = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonPrevKey = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonNextKey = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonFirstKey = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonLastKey = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupSeparator1 = new Krypton.Ribbon.KryptonRibbonGroupSeparator();
        this.LinesZoom2 = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonZoomAnimation = new Krypton.Ribbon.KryptonRibbonGroupButton();
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
        this.TabKeyEditor = new Krypton.Ribbon.KryptonRibbonTab();
        this.GroupKeyEditorAnimation = new Krypton.Ribbon.KryptonRibbonGroup();
        this.kryptonRibbonGroupTriple5 = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonAnimationGoBack = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.GroupAnimationKeys = new Krypton.Ribbon.KryptonRibbonGroup();
        this.TripleGroupKeyframes = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonAnimationSetKeyframe = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupSeparator2 = new Krypton.Ribbon.KryptonRibbonGroupSeparator();
        this.LineGroupKeyframes = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonAnimationRemoveKeyframes = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonAnimationClearKeyframes = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.GroupAnimationKeyEdit = new Krypton.Ribbon.KryptonRibbonGroup();
        this.TripleGroupAnimationEditPaste = new Krypton.Ribbon.KryptonRibbonGroupTriple();
        this.ButtonAnimationPaste = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.LineGroupAnimationCopyCut = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonAnimationCopy = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonAnimationCut = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.kryptonRibbonGroupSeparator5 = new Krypton.Ribbon.KryptonRibbonGroupSeparator();
        this.LineAnimationKeyUndoRedo = new Krypton.Ribbon.KryptonRibbonGroupLines();
        this.ButtonAnimationKeyUndo = new Krypton.Ribbon.KryptonRibbonGroupButton();
        this.ButtonAnimationKeyRedo = new Krypton.Ribbon.KryptonRibbonGroupButton();
        ((System.ComponentModel.ISupportInitialize)(this.RibbonAnimationContent)).BeginInit();
        this.MenuZoom.SuspendLayout();
        this.SuspendLayout();
        // 
        // RibbonAnimationContent
        // 
        this.RibbonAnimationContent.AllowFormIntegrate = true;
        this.RibbonAnimationContent.InDesignHelperMode = true;
        this.RibbonAnimationContent.Name = "RibbonAnimationContent";
        this.RibbonAnimationContent.RibbonContexts.AddRange(new Krypton.Ribbon.KryptonRibbonContext[] {
        this.ContextKeyEditor});
        this.RibbonAnimationContent.RibbonTabs.AddRange(new Krypton.Ribbon.KryptonRibbonTab[] {
        this.TabAnimation,
        this.TabKeyEditor});
        this.RibbonAnimationContent.SelectedContext = null;
        this.RibbonAnimationContent.SelectedTab = this.TabAnimation;
        this.RibbonAnimationContent.Size = new System.Drawing.Size(1594, 115);
        this.RibbonAnimationContent.TabIndex = 0;
        // 
        // ContextKeyEditor
        // 
        this.ContextKeyEditor.ContextColor = System.Drawing.Color.Blue;
        this.ContextKeyEditor.ContextName = "ContextKeyEditor";
        this.ContextKeyEditor.ContextTitle = "Edit Track";
        // 
        // TabAnimation
        // 
        this.TabAnimation.Groups.AddRange(new Krypton.Ribbon.KryptonRibbonGroup[] {
        this.GroupAnimationFile,
        this.GroupAnimationEdit,
        this.GroupAnimation,
        this.GroupTrack,
        this.GroupPreview});
        this.TabAnimation.KeyTip = "I";
        this.TabAnimation.Text = "Animation";
        // 
        // GroupAnimationFile
        // 
        this.GroupAnimationFile.AllowCollapsed = false;
        this.GroupAnimationFile.DialogBoxLauncher = false;
        this.GroupAnimationFile.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple1});
        this.GroupAnimationFile.KeyTipGroup = "F";
        this.GroupAnimationFile.TextLine1 = "File";
        // 
        // kryptonRibbonGroupTriple1
        // 
        this.kryptonRibbonGroupTriple1.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonNewAnimation,
        this.ButtonSaveAnimation});
        this.kryptonRibbonGroupTriple1.MinimumSize = Krypton.Ribbon.GroupItemSize.Large;
        // 
        // ButtonNewAnimation
        // 
        this.ButtonNewAnimation.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.new_content_48x48;
        this.ButtonNewAnimation.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.new_content_16x16;
        this.ButtonNewAnimation.KeyTip = "N";
        this.ButtonNewAnimation.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
        this.ButtonNewAnimation.TextLine1 = "New";
        this.ButtonNewAnimation.TextLine2 = "Animation";
        this.ButtonNewAnimation.ToolTipBody = "Creates a new animation based on the current animation being edited.\r\n\r\nAll anima" +
"tion properties (except the name) will be copied to the new \r\nanimation.";
        this.ButtonNewAnimation.ToolTipTitle = "New animation";
        this.ButtonNewAnimation.Click += new System.EventHandler(this.ButtonNewAnimation_Click);
        // 
        // ButtonSaveAnimation
        // 
        this.ButtonSaveAnimation.Enabled = false;
        this.ButtonSaveAnimation.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.save_48x48;
        this.ButtonSaveAnimation.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.save_16x16;
        this.ButtonSaveAnimation.KeyTip = "S";
        this.ButtonSaveAnimation.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
        this.ButtonSaveAnimation.TextLine1 = "Save";
        this.ButtonSaveAnimation.TextLine2 = "Animation";
        this.ButtonSaveAnimation.ToolTipBody = "Updates the animation file in the file system with the current changes.";
        this.ButtonSaveAnimation.ToolTipTitle = "Save Animation";
        this.ButtonSaveAnimation.Click += new System.EventHandler(this.ButtonSaveAnimation_Click);
        // 
        // GroupAnimationEdit
        // 
        this.GroupAnimationEdit.AllowCollapsed = false;
        this.GroupAnimationEdit.DialogBoxLauncher = false;
        this.GroupAnimationEdit.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.LineAnimationUndoRedo});
        this.GroupAnimationEdit.KeyTipGroup = "E";
        this.GroupAnimationEdit.TextLine1 = "Edit";
        // 
        // LineAnimationUndoRedo
        // 
        this.LineAnimationUndoRedo.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonAnimationUndo,
        this.ButtonAnimationRedo});
        // 
        // ButtonAnimationUndo
        // 
        this.ButtonAnimationUndo.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.undo_48x48;
        this.ButtonAnimationUndo.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.undo_16x16;
        this.ButtonAnimationUndo.KeyTip = "U";
        this.ButtonAnimationUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
        this.ButtonAnimationUndo.TextLine1 = "Undo";
        this.ButtonAnimationUndo.ToolTipBody = "Reverts the previous change made to the animation.\r\n.";
        this.ButtonAnimationUndo.ToolTipTitle = "Undo";
        this.ButtonAnimationUndo.Click += new System.EventHandler(this.ButtonAnimationUndo_Click);
        // 
        // ButtonAnimationRedo
        // 
        this.ButtonAnimationRedo.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.redo_48x48;
        this.ButtonAnimationRedo.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.redo_16x16;
        this.ButtonAnimationRedo.KeyTip = "R";
        this.ButtonAnimationRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
        this.ButtonAnimationRedo.TextLine1 = "Redo";
        this.ButtonAnimationRedo.ToolTipBody = "Restores the next change made to the animation.";
        this.ButtonAnimationRedo.ToolTipTitle = "Redo";
        this.ButtonAnimationRedo.Click += new System.EventHandler(this.ButtonAnimationRedo_Click);
        // 
        // GroupAnimation
        // 
        this.GroupAnimation.AllowCollapsed = false;
        this.GroupAnimation.DialogBoxLauncher = false;
        this.GroupAnimation.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple6,
        this.kryptonRibbonGroupSeparator6,
        this.LineGroupBgImage,
        this.LineGroupAnimation});
        this.GroupAnimation.KeyTipGroup = "A";
        this.GroupAnimation.TextLine1 = "Animation";
        // 
        // kryptonRibbonGroupTriple6
        // 
        this.kryptonRibbonGroupTriple6.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonAnimationProperties});
        // 
        // ButtonAnimationProperties
        // 
        this.ButtonAnimationProperties.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.anim_properties_48x48;
        this.ButtonAnimationProperties.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.anim_properties_16x16;
        this.ButtonAnimationProperties.KeyTip = "P";
        this.ButtonAnimationProperties.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.L)));
        this.ButtonAnimationProperties.TextLine1 = "Properties";
        this.ButtonAnimationProperties.ToolTipBody = "Updates properties for the animation such as the length and \r\nthe frames per seco" +
"nd.";
        this.ButtonAnimationProperties.ToolTipTitle = "Properties";
        this.ButtonAnimationProperties.Click += new System.EventHandler(this.ButtonAnimationProperties_Click);
        // 
        // LineGroupBgImage
        // 
        this.LineGroupBgImage.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonAnimationLoadBack,
        this.ButtonAnimationClearBack});
        // 
        // ButtonAnimationLoadBack
        // 
        this.ButtonAnimationLoadBack.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.load_image_48x48;
        this.ButtonAnimationLoadBack.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.load_image_16x16;
        this.ButtonAnimationLoadBack.KeyTip = "S";
        this.ButtonAnimationLoadBack.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
        this.ButtonAnimationLoadBack.TextLine1 = "Set Reference";
        this.ButtonAnimationLoadBack.TextLine2 = "Image";
        this.ButtonAnimationLoadBack.ToolTipBody = resources.GetString("ButtonAnimationLoadBack.ToolTipBody");
        this.ButtonAnimationLoadBack.ToolTipTitle = "Set reference image";
        this.ButtonAnimationLoadBack.Click += new System.EventHandler(this.ButtonAnimationLoadBack_Click);
        // 
        // ButtonAnimationClearBack
        // 
        this.ButtonAnimationClearBack.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.clear_image_48x48;
        this.ButtonAnimationClearBack.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.clear_image_16x16;
        this.ButtonAnimationClearBack.KeyTip = "C";
        this.ButtonAnimationClearBack.TextLine1 = "Clear Reference";
        this.ButtonAnimationClearBack.TextLine2 = "Image";
        this.ButtonAnimationClearBack.ToolTipBody = "Clears the reference image if one is loaded.\r\n\r\nNOTE: This is only for animation " +
"design purposes and will not be \r\nstored with the animation file.\r\n";
        this.ButtonAnimationClearBack.ToolTipTitle = "Clear reference image";
        this.ButtonAnimationClearBack.Click += new System.EventHandler(this.ButtonAnimationClearBack_Click);
        // 
        // LineGroupAnimation
        // 
        this.LineGroupAnimation.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonAnimationSprite,
        this.CheckAnimationLoop});
        // 
        // ButtonAnimationSprite
        // 
        this.ButtonAnimationSprite.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.sprite_48x48;
        this.ButtonAnimationSprite.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.sprite2_16x16;
        this.ButtonAnimationSprite.KeyTip = "P";
        this.ButtonAnimationSprite.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
        | System.Windows.Forms.Keys.P)));
        this.ButtonAnimationSprite.TextLine1 = "Set Main";
        this.ButtonAnimationSprite.TextLine2 = "Sprite";
        this.ButtonAnimationSprite.ToolTipBody = resources.GetString("ButtonAnimationSprite.ToolTipBody");
        this.ButtonAnimationSprite.ToolTipTitle = "Set main sprite";
        this.ButtonAnimationSprite.Click += new System.EventHandler(this.ButtonAnimationSprite_Click);
        // 
        // CheckAnimationLoop
        // 
        this.CheckAnimationLoop.ButtonType = Krypton.Ribbon.GroupButtonType.Check;
        this.CheckAnimationLoop.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.anim_loop_48x48;
        this.CheckAnimationLoop.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.anim_loop_16x16;
        this.CheckAnimationLoop.KeyTip = "L";
        this.CheckAnimationLoop.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
        | System.Windows.Forms.Keys.L)));
        this.CheckAnimationLoop.TextLine1 = "Looping";
        this.CheckAnimationLoop.ToolTipBody = "Enables or disables looping when animating.";
        this.CheckAnimationLoop.ToolTipTitle = "Looping";
        this.CheckAnimationLoop.Click += new System.EventHandler(this.CheckAnimationLoop_Click);
        // 
        // GroupTrack
        // 
        this.GroupTrack.AllowCollapsed = false;
        this.GroupTrack.DialogBoxLauncher = false;
        this.GroupTrack.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple4,
        this.kryptonRibbonGroupSeparator4,
        this.kryptonRibbonGroupTriple3,
        this.kryptonRibbonGroupLines3});
        this.GroupTrack.KeyTipGroup = "K";
        this.GroupTrack.TextLine1 = "Track";
        // 
        // kryptonRibbonGroupTriple4
        // 
        this.kryptonRibbonGroupTriple4.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.CheckAnimationEditTrack});
        // 
        // CheckAnimationEditTrack
        // 
        this.CheckAnimationEditTrack.ButtonType = Krypton.Ribbon.GroupButtonType.Check;
        this.CheckAnimationEditTrack.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.edit_track_48x48;
        this.CheckAnimationEditTrack.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.edit_track_16x16;
        this.CheckAnimationEditTrack.KeyTip = "E";
        this.CheckAnimationEditTrack.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
        this.CheckAnimationEditTrack.TextLine1 = "Edit Track";
        this.CheckAnimationEditTrack.ToolTipBody = "Opens the currently selected track for adding, updating and removing \r\nkey frames" +
" for the animation.";
        this.CheckAnimationEditTrack.ToolTipTitle = "Edit track";
        this.CheckAnimationEditTrack.Click += new System.EventHandler(this.CheckAnimationEditTrack_Click);
        // 
        // kryptonRibbonGroupTriple3
        // 
        this.kryptonRibbonGroupTriple3.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonAddTrack});
        // 
        // ButtonAddTrack
        // 
        this.ButtonAddTrack.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.add_track_48x48;
        this.ButtonAddTrack.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.add_track_16x16;
        this.ButtonAddTrack.KeyTip = "T";
        this.ButtonAddTrack.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
        this.ButtonAddTrack.TextLine1 = "Add";
        this.ButtonAddTrack.TextLine2 = "Track(s)";
        this.ButtonAddTrack.ToolTipBody = "Adds a new track or multiple tracks to the animation.";
        this.ButtonAddTrack.ToolTipTitle = "Add track";
        this.ButtonAddTrack.Click += new System.EventHandler(this.ButtonAddTrack_Click);
        // 
        // kryptonRibbonGroupLines3
        // 
        this.kryptonRibbonGroupLines3.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonRemoveTrack,
        this.ButtonAnimationClear});
        // 
        // ButtonRemoveTrack
        // 
        this.ButtonRemoveTrack.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.remove_track_48x48;
        this.ButtonRemoveTrack.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.remove_track_16x16;
        this.ButtonRemoveTrack.KeyTip = "R";
        this.ButtonRemoveTrack.TextLine1 = "Remove";
        this.ButtonRemoveTrack.TextLine2 = "Track";
        this.ButtonRemoveTrack.ToolTipBody = "Removes the selected track(s) from the animation.";
        this.ButtonRemoveTrack.ToolTipTitle = "Remove track";
        this.ButtonRemoveTrack.Click += new System.EventHandler(this.ButtonRemoveTrack_Click);
        // 
        // ButtonAnimationClear
        // 
        this.ButtonAnimationClear.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.clear_fs_48x48;
        this.ButtonAnimationClear.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.clear_fs_16x16;
        this.ButtonAnimationClear.KeyTip = "C";
        this.ButtonAnimationClear.TextLine1 = "Clear Track(s)";
        this.ButtonAnimationClear.ToolTipBody = "Clears all tracks and keys from the current animation.";
        this.ButtonAnimationClear.ToolTipTitle = "Clear";
        this.ButtonAnimationClear.Click += new System.EventHandler(this.ButtonAnimationClear_Click);
        // 
        // GroupPreview
        // 
        this.GroupPreview.AllowCollapsed = false;
        this.GroupPreview.DialogBoxLauncher = false;
        this.GroupPreview.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple2,
        this.kryptonRibbonGroupSeparator3,
        this.kryptonRibbonGroupLines1,
        this.kryptonRibbonGroupSeparator1,
        this.LinesZoom2});
        this.GroupPreview.KeyTipGroup = "V";
        this.GroupPreview.TextLine1 = "Preview";
        // 
        // kryptonRibbonGroupTriple2
        // 
        this.kryptonRibbonGroupTriple2.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonAnimPlay,
        this.ButtonAnimStop});
        // 
        // ButtonAnimPlay
        // 
        this.ButtonAnimPlay.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.anim_play_48x48;
        this.ButtonAnimPlay.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.anim_play_16x16;
        this.ButtonAnimPlay.KeyTip = "P";
        this.ButtonAnimPlay.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Space)));
        this.ButtonAnimPlay.TextLine1 = "Play";
        this.ButtonAnimPlay.ToolTipBody = "Plays the current animation from the current selected time.";
        this.ButtonAnimPlay.ToolTipTitle = "Play Animation";
        this.ButtonAnimPlay.Click += new System.EventHandler(this.ButtonAnimPlay_Click);
        // 
        // ButtonAnimStop
        // 
        this.ButtonAnimStop.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.anim_stop_48x48;
        this.ButtonAnimStop.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.anim_stop_16x16;
        this.ButtonAnimStop.KeyTip = "S";
        this.ButtonAnimStop.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Space)));
        this.ButtonAnimStop.TextLine1 = "Stop";
        this.ButtonAnimStop.ToolTipBody = "Stops a currently playing animation.";
        this.ButtonAnimStop.ToolTipTitle = "Stop animation";
        this.ButtonAnimStop.Visible = false;
        this.ButtonAnimStop.Click += new System.EventHandler(this.ButtonAnimStop_Click);
        // 
        // kryptonRibbonGroupLines1
        // 
        this.kryptonRibbonGroupLines1.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonPrevKey,
        this.ButtonNextKey,
        this.ButtonFirstKey,
        this.ButtonLastKey});
        // 
        // ButtonPrevKey
        // 
        this.ButtonPrevKey.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.anim_prev_key_48x48;
        this.ButtonPrevKey.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.anim_prev_key_16x16;
        this.ButtonPrevKey.KeyTip = "R";
        this.ButtonPrevKey.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Left)));
        this.ButtonPrevKey.TextLine1 = "Previous";
        this.ButtonPrevKey.ToolTipBody = "Moves to the previous key frame of the animation.";
        this.ButtonPrevKey.ToolTipTitle = "Previous key frame";
        this.ButtonPrevKey.Click += new System.EventHandler(this.ButtonPrevKey_Click);
        // 
        // ButtonNextKey
        // 
        this.ButtonNextKey.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.anim_next_key_48x48;
        this.ButtonNextKey.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.anim_next_key_16x16;
        this.ButtonNextKey.KeyTip = "N";
        this.ButtonNextKey.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Right)));
        this.ButtonNextKey.TextLine1 = "Next";
        this.ButtonNextKey.ToolTipBody = "Moves to the next key frame of the animation.";
        this.ButtonNextKey.ToolTipTitle = "Next key frame";
        this.ButtonNextKey.Click += new System.EventHandler(this.ButtonNextKey_Click);
        // 
        // ButtonFirstKey
        // 
        this.ButtonFirstKey.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.anim_first_key_48x48;
        this.ButtonFirstKey.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.anim_first_key_16x16;
        this.ButtonFirstKey.KeyTip = "F";
        this.ButtonFirstKey.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Home)));
        this.ButtonFirstKey.TextLine1 = "First";
        this.ButtonFirstKey.ToolTipBody = "Resets the animation to the first key frame.";
        this.ButtonFirstKey.ToolTipTitle = "First key frame";
        this.ButtonFirstKey.Click += new System.EventHandler(this.ButtonFirstKey_Click);
        // 
        // ButtonLastKey
        // 
        this.ButtonLastKey.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.anim_last_key_48x48;
        this.ButtonLastKey.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.anim_last_key_16x16;
        this.ButtonLastKey.KeyTip = "L";
        this.ButtonLastKey.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.End)));
        this.ButtonLastKey.TextLine1 = "Last";
        this.ButtonLastKey.ToolTipBody = "Sets the animation to the last key frame.";
        this.ButtonLastKey.ToolTipTitle = "Last key frame";
        this.ButtonLastKey.Click += new System.EventHandler(this.ButtonLastKey_Click);
        // 
        // LinesZoom2
        // 
        this.LinesZoom2.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonZoomAnimation});
        // 
        // ButtonZoomAnimation
        // 
        this.ButtonZoomAnimation.ButtonType = Krypton.Ribbon.GroupButtonType.DropDown;
        this.ButtonZoomAnimation.ContextMenuStrip = this.MenuZoom;
        this.ButtonZoomAnimation.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.zoom_48x48;
        this.ButtonZoomAnimation.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.zoom_16x16;
        this.ButtonZoomAnimation.KeyTip = "Z";
        this.ButtonZoomAnimation.TextLine1 = "Zoom";
        this.ButtonZoomAnimation.ToolTipBody = resources.GetString("ButtonZoomAnimation.ToolTipBody");
        this.ButtonZoomAnimation.ToolTipTitle = "Zoom";
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
        // TabKeyEditor
        // 
        this.TabKeyEditor.ContextName = "ContextKeyEditor";
        this.TabKeyEditor.Groups.AddRange(new Krypton.Ribbon.KryptonRibbonGroup[] {
        this.GroupKeyEditorAnimation,
        this.GroupAnimationKeys,
        this.GroupAnimationKeyEdit});
        this.TabKeyEditor.KeyTip = "K";
        this.TabKeyEditor.Text = "Key Editor";
        // 
        // GroupKeyEditorAnimation
        // 
        this.GroupKeyEditorAnimation.AllowCollapsed = false;
        this.GroupKeyEditorAnimation.DialogBoxLauncher = false;
        this.GroupKeyEditorAnimation.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.kryptonRibbonGroupTriple5});
        this.GroupKeyEditorAnimation.KeyTipGroup = "D";
        this.GroupKeyEditorAnimation.TextLine1 = "Editor";
        // 
        // kryptonRibbonGroupTriple5
        // 
        this.kryptonRibbonGroupTriple5.ItemAlignment = Krypton.Ribbon.RibbonItemAlignment.Center;
        this.kryptonRibbonGroupTriple5.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonAnimationGoBack});
        // 
        // ButtonAnimationGoBack
        // 
        this.ButtonAnimationGoBack.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.go_back_48x48;
        this.ButtonAnimationGoBack.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.go_back_16x16;
        this.ButtonAnimationGoBack.TextLine1 = "Go Back";
        this.ButtonAnimationGoBack.ToolTipBody = "Closes the key editor and returns back to the animation tab.";
        this.ButtonAnimationGoBack.ToolTipTitle = "Go back to animation";
        this.ButtonAnimationGoBack.Click += new System.EventHandler(this.CheckAnimationEditTrack_Click);
        // 
        // GroupAnimationKeys
        // 
        this.GroupAnimationKeys.AllowCollapsed = false;
        this.GroupAnimationKeys.DialogBoxLauncher = false;
        this.GroupAnimationKeys.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.TripleGroupKeyframes,
        this.kryptonRibbonGroupSeparator2,
        this.LineGroupKeyframes});
        this.GroupAnimationKeys.KeyTipGroup = "K";
        this.GroupAnimationKeys.TextLine1 = "Keyframes";
        // 
        // TripleGroupKeyframes
        // 
        this.TripleGroupKeyframes.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonAnimationSetKeyframe});
        // 
        // ButtonAnimationSetKeyframe
        // 
        this.ButtonAnimationSetKeyframe.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.add_key_frame_48x48;
        this.ButtonAnimationSetKeyframe.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.add_key_frame_16x16;
        this.ButtonAnimationSetKeyframe.KeyTip = "S";
        this.ButtonAnimationSetKeyframe.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
        | System.Windows.Forms.Keys.S)));
        this.ButtonAnimationSetKeyframe.TextLine1 = "Set";
        this.ButtonAnimationSetKeyframe.TextLine2 = "Keyframe";
        this.ButtonAnimationSetKeyframe.ToolTipBody = "Assigns the current value(s) to the currently selected key frame.";
        this.ButtonAnimationSetKeyframe.ToolTipTitle = "Set key frame";
        this.ButtonAnimationSetKeyframe.Click += new System.EventHandler(this.ButtonAnimationSetKeyframe_Click);
        // 
        // LineGroupKeyframes
        // 
        this.LineGroupKeyframes.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonAnimationRemoveKeyframes,
        this.ButtonAnimationClearKeyframes});
        // 
        // ButtonAnimationRemoveKeyframes
        // 
        this.ButtonAnimationRemoveKeyframes.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.remove_key_frame_48x48;
        this.ButtonAnimationRemoveKeyframes.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.remove_key_frame_16x16;
        this.ButtonAnimationRemoveKeyframes.KeyTip = "R";
        this.ButtonAnimationRemoveKeyframes.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
        this.ButtonAnimationRemoveKeyframes.TextLine1 = "Remove";
        this.ButtonAnimationRemoveKeyframes.TextLine2 = "Keyframes";
        this.ButtonAnimationRemoveKeyframes.ToolTipBody = "Removes the selected key frames from the track.";
        this.ButtonAnimationRemoveKeyframes.ToolTipTitle = "Remove key frames";
        this.ButtonAnimationRemoveKeyframes.Click += new System.EventHandler(this.ButtonAnimationRemoveKeyframes_Click);
        // 
        // ButtonAnimationClearKeyframes
        // 
        this.ButtonAnimationClearKeyframes.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.clear_fs_48x48;
        this.ButtonAnimationClearKeyframes.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.clear_fs_16x16;
        this.ButtonAnimationClearKeyframes.KeyTip = "C";
        this.ButtonAnimationClearKeyframes.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
        | System.Windows.Forms.Keys.Delete)));
        this.ButtonAnimationClearKeyframes.TextLine1 = "Clear All";
        this.ButtonAnimationClearKeyframes.TextLine2 = "Keyframes";
        this.ButtonAnimationClearKeyframes.ToolTipBody = "Removes all key frames from the tracks.";
        this.ButtonAnimationClearKeyframes.ToolTipTitle = "Clear all key frames";
        this.ButtonAnimationClearKeyframes.Click += new System.EventHandler(this.ButtonAnimationClearKeyframes_Click);
        // 
        // GroupAnimationKeyEdit
        // 
        this.GroupAnimationKeyEdit.AllowCollapsed = false;
        this.GroupAnimationKeyEdit.DialogBoxLauncher = false;
        this.GroupAnimationKeyEdit.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupContainer[] {
        this.TripleGroupAnimationEditPaste,
        this.LineGroupAnimationCopyCut,
        this.kryptonRibbonGroupSeparator5,
        this.LineAnimationKeyUndoRedo});
        this.GroupAnimationKeyEdit.KeyTipGroup = "E";
        this.GroupAnimationKeyEdit.TextLine1 = "Edit";
        // 
        // TripleGroupAnimationEditPaste
        // 
        this.TripleGroupAnimationEditPaste.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonAnimationPaste});
        // 
        // ButtonAnimationPaste
        // 
        this.ButtonAnimationPaste.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.paste_48x48;
        this.ButtonAnimationPaste.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.paste_16x16;
        this.ButtonAnimationPaste.KeyTip = "C";
        this.ButtonAnimationPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
        this.ButtonAnimationPaste.TextLine1 = "Paste";
        this.ButtonAnimationPaste.ToolTipBody = "Pastes copied/cut key frames to another location in a track.";
        this.ButtonAnimationPaste.ToolTipTitle = "Paste";
        this.ButtonAnimationPaste.Click += new System.EventHandler(this.ButtonAnimationPaste_Click);
        // 
        // LineGroupAnimationCopyCut
        // 
        this.LineGroupAnimationCopyCut.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonAnimationCopy,
        this.ButtonAnimationCut});
        // 
        // ButtonAnimationCopy
        // 
        this.ButtonAnimationCopy.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.copy_48x48;
        this.ButtonAnimationCopy.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.copy_16x16;
        this.ButtonAnimationCopy.KeyTip = "P";
        this.ButtonAnimationCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
        this.ButtonAnimationCopy.TextLine1 = "Copy";
        this.ButtonAnimationCopy.ToolTipBody = "Copy the currently selected key frame(s).";
        this.ButtonAnimationCopy.ToolTipTitle = "Copy";
        this.ButtonAnimationCopy.Click += new System.EventHandler(this.ButtonAnimationCopy_Click);
        // 
        // ButtonAnimationCut
        // 
        this.ButtonAnimationCut.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.cut_48x48;
        this.ButtonAnimationCut.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.cut_16x16;
        this.ButtonAnimationCut.KeyTip = "T";
        this.ButtonAnimationCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
        this.ButtonAnimationCut.TextLine1 = "Cut";
        this.ButtonAnimationCut.ToolTipBody = "Cuts the selected key frame(s) for moving key frames \r\nto another location on the" +
" track.";
        this.ButtonAnimationCut.ToolTipTitle = "Cut";
        this.ButtonAnimationCut.Click += new System.EventHandler(this.ButtonAnimationCut_Click);
        // 
        // LineAnimationKeyUndoRedo
        // 
        this.LineAnimationKeyUndoRedo.Items.AddRange(new Krypton.Ribbon.KryptonRibbonGroupItem[] {
        this.ButtonAnimationKeyUndo,
        this.ButtonAnimationKeyRedo});
        // 
        // ButtonAnimationKeyUndo
        // 
        this.ButtonAnimationKeyUndo.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.undo_48x48;
        this.ButtonAnimationKeyUndo.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.undo_16x16;
        this.ButtonAnimationKeyUndo.KeyTip = "U";
        this.ButtonAnimationKeyUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
        this.ButtonAnimationKeyUndo.TextLine1 = "Undo";
        this.ButtonAnimationKeyUndo.ToolTipBody = "Reverts the previous change made to the animation.\r\n.";
        this.ButtonAnimationKeyUndo.ToolTipTitle = "Undo";
        this.ButtonAnimationKeyUndo.Click += new System.EventHandler(this.ButtonAnimationUndo_Click);
        // 
        // ButtonAnimationKeyRedo
        // 
        this.ButtonAnimationKeyRedo.ImageLarge = global::Gorgon.Editor.AnimationEditor.Properties.Resources.redo_48x48;
        this.ButtonAnimationKeyRedo.ImageSmall = global::Gorgon.Editor.AnimationEditor.Properties.Resources.redo_16x16;
        this.ButtonAnimationKeyRedo.KeyTip = "R";
        this.ButtonAnimationKeyRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
        this.ButtonAnimationKeyRedo.TextLine1 = "Redo";
        this.ButtonAnimationKeyRedo.ToolTipBody = "Restores the next change made to the animation.";
        this.ButtonAnimationKeyRedo.ToolTipTitle = "Redo";
        this.ButtonAnimationKeyRedo.Click += new System.EventHandler(this.ButtonAnimationRedo_Click);
        // 
        // FormRibbon
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1594, 302);
        this.Controls.Add(this.RibbonAnimationContent);
        
        this.Name = "FormRibbon";
        this.Text = "FormRibbon";
        ((System.ComponentModel.ISupportInitialize)(this.RibbonAnimationContent)).EndInit();
        this.MenuZoom.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();

    }


    private Krypton.Ribbon.KryptonRibbonTab TabAnimation;
    private Krypton.Ribbon.KryptonRibbonGroup GroupAnimationFile;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple1;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonSaveAnimation;
    internal Krypton.Ribbon.KryptonRibbon RibbonAnimationContent;
    private Krypton.Ribbon.KryptonRibbonGroup GroupAnimationEdit;
    private Krypton.Ribbon.KryptonRibbonGroupLines LineAnimationUndoRedo;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAnimationUndo;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAnimationRedo;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonNewAnimation;
    private Krypton.Ribbon.KryptonRibbonGroup GroupTrack;
    internal Krypton.Ribbon.KryptonRibbonGroupButton ButtonZoomAnimation;
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
    private Krypton.Ribbon.KryptonRibbonGroup GroupPreview;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple2;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAnimPlay;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAnimStop;
    private Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines1;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonPrevKey;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonNextKey;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonFirstKey;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonLastKey;
    private Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator3;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple3;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAddTrack;
    private Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines3;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonRemoveTrack;
    private Krypton.Ribbon.KryptonRibbonGroupTriple TripleGroupAnimationEditPaste;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAnimationPaste;
    private Krypton.Ribbon.KryptonRibbonGroupLines LineGroupAnimationCopyCut;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAnimationCut;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAnimationCopy;
    private Krypton.Ribbon.KryptonRibbonGroupLines LinesZoom2;
    private Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator1;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAnimationClear;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple4;
    private Krypton.Ribbon.KryptonRibbonGroupButton CheckAnimationEditTrack;
    private Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator4;
    private Krypton.Ribbon.KryptonRibbonGroupButton CheckAnimationLoop;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAnimationLoadBack;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAnimationClearBack;
    private Krypton.Ribbon.KryptonRibbonGroup GroupAnimation;
    private Krypton.Ribbon.KryptonRibbonGroupLines LineGroupBgImage;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple6;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAnimationProperties;
    private Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator6;
    private Krypton.Ribbon.KryptonRibbonGroupLines LineGroupAnimation;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAnimationSprite;
    private Krypton.Ribbon.KryptonRibbonTab TabKeyEditor;
    private Krypton.Ribbon.KryptonRibbonGroup GroupAnimationKeyEdit;
    private Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator5;
    private Krypton.Ribbon.KryptonRibbonGroupLines LineAnimationKeyUndoRedo;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAnimationKeyUndo;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAnimationKeyRedo;
    private Krypton.Ribbon.KryptonRibbonContext ContextKeyEditor;
    private Krypton.Ribbon.KryptonRibbonGroup GroupAnimationKeys;
    private Krypton.Ribbon.KryptonRibbonGroupTriple TripleGroupKeyframes;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAnimationSetKeyframe;
    private Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator2;
    private Krypton.Ribbon.KryptonRibbonGroupLines LineGroupKeyframes;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAnimationRemoveKeyframes;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAnimationClearKeyframes;
    private Krypton.Ribbon.KryptonRibbonGroup GroupKeyEditorAnimation;
    private Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple5;
    private Krypton.Ribbon.KryptonRibbonGroupButton ButtonAnimationGoBack;
}
