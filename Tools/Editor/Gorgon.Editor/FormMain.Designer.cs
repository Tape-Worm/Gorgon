using Gorgon.Editor.Views;

namespace Gorgon.Editor
{
    partial class FormMain
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
            if (disposing)
            {
                UnassignEvents();
            }

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.ProgressScreen = new Gorgon.UI.GorgonProgressScreenPanel();
            this.WaitScreen = new Gorgon.UI.GorgonWaitScreenPanel();
            this.RibbonMain = new ComponentFactory.Krypton.Ribbon.KryptonRibbon();
            this.MainPalette = new ComponentFactory.Krypton.Toolkit.KryptonPalette(this.components);
            this.TabFileSystem = new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab();
            this.RibbonGroupFileSystemNew = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.GroupCreate = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.ButtonCreate = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.MenuCreate = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SepCreate = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator();
            this.kryptonRibbonGroupTriple5 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.ButtonOpenContent = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.ButtonFileSystemNewDirectory = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.kryptonRibbonGroupSeparator2 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator();
            this.kryptonRibbonGroupLines3 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
            this.ButtonImport = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.ButtonExport = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.GroupFileSystemEdit = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.kryptonRibbonGroupTriple3 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.ButtonFileSystemPaste = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.kryptonRibbonGroupLines2 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
            this.ButtonFileSystemCopy = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.ButtonFileSystemCut = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.RibbonGroupFileSystemOrganize = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.kryptonRibbonGroupTriple2 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.ButtonFileSystemDelete = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.ButtonFileSystemDeleteAll = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.kryptonRibbonGroupSeparator1 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator();
            this.kryptonRibbonGroupTriple4 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.ButtonFileSystemRename = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.RibbonTabEditorTools = new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab();
            this.Stage = new Gorgon.Editor.Views.Stage();
            this.PanelProject = new Gorgon.Editor.Views.ProjectContainer();
            this.PanelWorkSpace = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.RibbonMain)).BeginInit();
            this.PanelWorkSpace.SuspendLayout();
            this.SuspendLayout();
            // 
            // ProgressScreen
            // 
            this.ProgressScreen.CurrentValue = 0F;
            this.ProgressScreen.Location = new System.Drawing.Point(406, 220);
            this.ProgressScreen.Name = "ProgressScreen";
            this.ProgressScreen.ProgressMessage = resources.GetString("ProgressScreen.ProgressMessage");
            this.ProgressScreen.ProgressMessageFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ProgressScreen.ProgressTitle = resources.GetString("ProgressScreen.ProgressTitle");
            this.ProgressScreen.Size = new System.Drawing.Size(640, 200);
            this.ProgressScreen.TabIndex = 1;
            this.ProgressScreen.Visible = false;
            // 
            // WaitScreen
            // 
            this.WaitScreen.Location = new System.Drawing.Point(213, 173);
            this.WaitScreen.Name = "WaitScreen";
            this.WaitScreen.Size = new System.Drawing.Size(281, 145);
            this.WaitScreen.TabIndex = 0;
            this.WaitScreen.Visible = false;
            this.WaitScreen.WaitMessage = "Loading...";
            this.WaitScreen.WaitTitle = "Please wait";
            // 
            // RibbonMain
            // 
            this.RibbonMain.AllowFormIntegrate = true;
            this.RibbonMain.AllowMinimizedChange = false;
            this.RibbonMain.InDesignHelperMode = true;
            this.RibbonMain.Name = "RibbonMain";
            this.RibbonMain.Palette = this.MainPalette;
            this.RibbonMain.PaletteMode = ComponentFactory.Krypton.Toolkit.PaletteMode.Custom;
            this.RibbonMain.QATLocation = ComponentFactory.Krypton.Ribbon.QATLocation.Hidden;
            this.RibbonMain.QATUserChange = false;
            this.RibbonMain.RibbonAppButton.AppButtonShowRecentDocs = false;
            this.RibbonMain.RibbonStyles.BackInactiveStyle = ComponentFactory.Krypton.Toolkit.PaletteBackStyle.ButtonCalendarDay;
            this.RibbonMain.RibbonStyles.BackStyle = ComponentFactory.Krypton.Toolkit.PaletteBackStyle.ButtonCalendarDay;
            this.RibbonMain.RibbonTabs.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab[] {
            this.TabFileSystem,
            this.RibbonTabEditorTools});
            this.RibbonMain.SelectedContext = null;
            this.RibbonMain.SelectedTab = this.TabFileSystem;
            this.RibbonMain.ShowMinimizeButton = false;
            this.RibbonMain.Size = new System.Drawing.Size(1051, 115);
            this.RibbonMain.StateCheckedNormal.RibbonTab.BackColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCheckedNormal.RibbonTab.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCheckedNormal.RibbonTab.BackColor3 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCheckedNormal.RibbonTab.BackColor4 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCheckedNormal.RibbonTab.BackColor5 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCheckedTracking.RibbonTab.BackColor2 = System.Drawing.Color.SteelBlue;
            this.RibbonMain.StateCheckedTracking.RibbonTab.BackColor3 = System.Drawing.Color.SteelBlue;
            this.RibbonMain.StateCheckedTracking.RibbonTab.BackColor4 = System.Drawing.Color.SteelBlue;
            this.RibbonMain.StateCommon.RibbonGeneral.ContextTextFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RibbonMain.StateCommon.RibbonGeneral.DisabledDark = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.RibbonMain.StateCommon.RibbonGeneral.DisabledLight = System.Drawing.Color.Silver;
            this.RibbonMain.StateCommon.RibbonGeneral.DropArrowDark = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCommon.RibbonGeneral.DropArrowLight = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCommon.RibbonGeneral.GroupSeparatorDark = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCommon.RibbonGeneral.GroupSeparatorLight = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCommon.RibbonGeneral.RibbonShape = ComponentFactory.Krypton.Toolkit.PaletteRibbonShape.Office365;
            this.RibbonMain.StateCommon.RibbonGeneral.TabSeparatorColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCommon.RibbonGeneral.TextFont = new System.Drawing.Font("Segoe UI", 9F);
            this.RibbonMain.StateCommon.RibbonGroupArea.BackColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.RibbonMain.StateCommon.RibbonGroupArea.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.RibbonMain.StateCommon.RibbonGroupArea.BackColor3 = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.RibbonMain.StateCommon.RibbonGroupArea.BackColor4 = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.RibbonMain.StateCommon.RibbonGroupArea.BackColor5 = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.RibbonMain.StateCommon.RibbonGroupButtonText.TextColor = System.Drawing.Color.White;
            this.RibbonMain.StateCommon.RibbonGroupCheckBoxText.TextColor = System.Drawing.Color.White;
            this.RibbonMain.StateCommon.RibbonGroupLabelText.TextColor = System.Drawing.Color.White;
            this.RibbonMain.StateCommon.RibbonGroupNormalBorder.BackColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCommon.RibbonGroupNormalBorder.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCommon.RibbonGroupNormalBorder.BackColor3 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCommon.RibbonGroupNormalBorder.BackColor4 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCommon.RibbonGroupNormalBorder.BackColor5 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCommon.RibbonGroupNormalTitle.TextColor = System.Drawing.Color.White;
            this.RibbonMain.StateCommon.RibbonGroupRadioButtonText.TextColor = System.Drawing.Color.White;
            this.RibbonMain.StateCommon.RibbonTab.BackColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCommon.RibbonTab.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCommon.RibbonTab.BackColor3 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCommon.RibbonTab.BackColor4 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCommon.RibbonTab.BackColor5 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateCommon.RibbonTab.TextColor = System.Drawing.Color.White;
            this.RibbonMain.StateDisabled.RibbonGroupButtonText.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.RibbonMain.StateDisabled.RibbonGroupCheckBoxText.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.RibbonMain.StateDisabled.RibbonGroupLabelText.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.RibbonMain.StateDisabled.RibbonGroupRadioButtonText.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.RibbonMain.StateNormal.RibbonGroupButtonText.TextColor = System.Drawing.Color.White;
            this.RibbonMain.StateNormal.RibbonGroupCheckBoxText.TextColor = System.Drawing.Color.White;
            this.RibbonMain.StateNormal.RibbonGroupCollapsedText.TextColor = System.Drawing.Color.White;
            this.RibbonMain.StateNormal.RibbonGroupLabelText.TextColor = System.Drawing.Color.White;
            this.RibbonMain.StateNormal.RibbonGroupNormalBorder.BackColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateNormal.RibbonGroupNormalBorder.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateNormal.RibbonGroupNormalBorder.BackColor3 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateNormal.RibbonGroupNormalBorder.BackColor4 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateNormal.RibbonGroupNormalBorder.BackColor5 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateNormal.RibbonGroupNormalTitle.TextColor = System.Drawing.Color.White;
            this.RibbonMain.StateNormal.RibbonGroupRadioButtonText.TextColor = System.Drawing.Color.White;
            this.RibbonMain.StateNormal.RibbonTab.BackColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateNormal.RibbonTab.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateNormal.RibbonTab.BackColor3 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateNormal.RibbonTab.BackColor4 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateNormal.RibbonTab.BackColor5 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateNormal.RibbonTab.TextColor = System.Drawing.Color.White;
            this.RibbonMain.StateTracking.RibbonGroupNormalTitle.BackColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateTracking.RibbonGroupNormalTitle.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateTracking.RibbonGroupNormalTitle.BackColor3 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateTracking.RibbonGroupNormalTitle.BackColor4 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateTracking.RibbonGroupNormalTitle.BackColor5 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.RibbonMain.StateTracking.RibbonGroupNormalTitle.TextColor = System.Drawing.Color.White;
            this.RibbonMain.StateTracking.RibbonTab.BackColor3 = System.Drawing.Color.DarkOrange;
            this.RibbonMain.StateTracking.RibbonTab.BackColor4 = System.Drawing.Color.DarkOrange;
            this.RibbonMain.StateTracking.RibbonTab.BackColor5 = System.Drawing.Color.DarkOrange;
            this.RibbonMain.TabIndex = 0;
            this.RibbonMain.Visible = false;
            this.RibbonMain.AppButtonMenuOpening += new System.ComponentModel.CancelEventHandler(this.RibbonMain_AppButtonMenuOpening);
            // 
            // MainPalette
            // 
            this.MainPalette.BasePaletteMode = ComponentFactory.Krypton.Toolkit.PaletteMode.Office365Black;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.OverrideDefault.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.OverrideDefault.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.None;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.OverrideFocus.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.OverrideFocus.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.None;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateCheckedNormal.Back.Color1 = System.Drawing.Color.SteelBlue;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateCheckedNormal.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateCheckedNormal.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.None;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateCheckedPressed.Back.Color1 = System.Drawing.Color.Orange;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateCheckedPressed.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateCheckedPressed.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.None;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateCheckedTracking.Back.Color1 = System.Drawing.Color.SteelBlue;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateCheckedTracking.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateCheckedTracking.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.None;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateCommon.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateCommon.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateCommon.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.None;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateDisabled.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateDisabled.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.None;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateNormal.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateNormal.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.None;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StatePressed.Back.Color1 = System.Drawing.Color.DarkOrange;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StatePressed.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StatePressed.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.None;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateTracking.Back.Color1 = System.Drawing.Color.SteelBlue;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateTracking.Back.Color2 = System.Drawing.Color.SteelBlue;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateTracking.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateTracking.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.None;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateTracking.Border.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.None;
            this.MainPalette.ButtonStyles.ButtonButtonSpec.StateTracking.Border.Width = 0;
            this.MainPalette.CustomisedKryptonPaletteFilePath = null;
            // 
            // TabFileSystem
            // 
            this.TabFileSystem.Groups.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup[] {
            this.RibbonGroupFileSystemNew,
            this.GroupFileSystemEdit,
            this.RibbonGroupFileSystemOrganize});
            this.TabFileSystem.KeyTip = "Y";
            this.TabFileSystem.Text = "File System";
            // 
            // RibbonGroupFileSystemNew
            // 
            this.RibbonGroupFileSystemNew.AllowCollapsed = false;
            this.RibbonGroupFileSystemNew.DialogBoxLauncher = false;
            this.RibbonGroupFileSystemNew.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
            this.GroupCreate,
            this.SepCreate,
            this.kryptonRibbonGroupTriple5,
            this.kryptonRibbonGroupSeparator2,
            this.kryptonRibbonGroupLines3});
            this.RibbonGroupFileSystemNew.KeyTipGroup = "F";
            this.RibbonGroupFileSystemNew.TextLine1 = "File";
            // 
            // GroupCreate
            // 
            this.GroupCreate.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonCreate});
            this.GroupCreate.Visible = false;
            // 
            // ButtonCreate
            // 
            this.ButtonCreate.ButtonType = ComponentFactory.Krypton.Ribbon.GroupButtonType.DropDown;
            this.ButtonCreate.ContextMenuStrip = this.MenuCreate;
            this.ButtonCreate.ImageLarge = global::Gorgon.Editor.Properties.Resources.new_content_48x48;
            this.ButtonCreate.ImageSmall = global::Gorgon.Editor.Properties.Resources.new_content_16x16;
            this.ButtonCreate.KeyTip = "C";
            this.ButtonCreate.TextLine1 = "Create";
            this.ButtonCreate.ToolTipBody = "Creates new content items.\r\n\r\nNote that some plug ins do not allow the creation o" +
    "f content.";
            // 
            // MenuCreate
            // 
            this.MenuCreate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MenuCreate.Name = "MenuCreate";
            this.MenuCreate.Size = new System.Drawing.Size(61, 4);
            // 
            // SepCreate
            // 
            this.SepCreate.Visible = false;
            // 
            // kryptonRibbonGroupTriple5
            // 
            this.kryptonRibbonGroupTriple5.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonOpenContent,
            this.ButtonFileSystemNewDirectory});
            // 
            // ButtonOpenContent
            // 
            this.ButtonOpenContent.Enabled = false;
            this.ButtonOpenContent.ImageLarge = global::Gorgon.Editor.Properties.Resources.open_edit_48x48;
            this.ButtonOpenContent.ImageSmall = global::Gorgon.Editor.Properties.Resources.open_edit_16x16;
            this.ButtonOpenContent.KeyTip = "O";
            this.ButtonOpenContent.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.ButtonOpenContent.TextLine1 = "Open";
            this.ButtonOpenContent.ToolTipBody = "Opens the currently selected file in the editor.\r\n\r\nIf the file is not handled by" +
    " the editor because it lacks an appropriate plug in, then this button will be di" +
    "sabled.";
            this.ButtonOpenContent.Click += new System.EventHandler(this.ButtonOpenContent_Click);
            // 
            // ButtonFileSystemNewDirectory
            // 
            this.ButtonFileSystemNewDirectory.Enabled = false;
            this.ButtonFileSystemNewDirectory.ImageLarge = global::Gorgon.Editor.Properties.Resources.add_directory_48x48;
            this.ButtonFileSystemNewDirectory.ImageSmall = global::Gorgon.Editor.Properties.Resources.add_directory_16x16;
            this.ButtonFileSystemNewDirectory.KeyTip = "D";
            this.ButtonFileSystemNewDirectory.TextLine1 = "Create";
            this.ButtonFileSystemNewDirectory.TextLine2 = "Directory";
            this.ButtonFileSystemNewDirectory.ToolTipBody = "Create a new directory";
            this.ButtonFileSystemNewDirectory.Click += new System.EventHandler(this.ButtonFileSystemNewDirectory_Click);
            // 
            // kryptonRibbonGroupLines3
            // 
            this.kryptonRibbonGroupLines3.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonImport,
            this.ButtonExport});
            // 
            // ButtonImport
            // 
            this.ButtonImport.ImageLarge = global::Gorgon.Editor.Properties.Resources.import_48x48;
            this.ButtonImport.ImageSmall = global::Gorgon.Editor.Properties.Resources.import_16x16;
            this.ButtonImport.KeyTip = "I";
            this.ButtonImport.TextLine1 = "Import";
            this.ButtonImport.ToolTipBody = "Import a directory into the selected directory or root directory.";
            this.ButtonImport.Click += new System.EventHandler(this.ButtonImport_Click);
            // 
            // ButtonExport
            // 
            this.ButtonExport.Enabled = false;
            this.ButtonExport.ImageLarge = global::Gorgon.Editor.Properties.Resources.export_48x48;
            this.ButtonExport.ImageSmall = global::Gorgon.Editor.Properties.Resources.export_16x16;
            this.ButtonExport.KeyTip = "E";
            this.ButtonExport.TextLine1 = "Export";
            this.ButtonExport.ToolTipBody = "Export the selected directory or file system.";
            this.ButtonExport.Click += new System.EventHandler(this.ButtonExport_Click);
            // 
            // GroupFileSystemEdit
            // 
            this.GroupFileSystemEdit.AllowCollapsed = false;
            this.GroupFileSystemEdit.DialogBoxLauncher = false;
            this.GroupFileSystemEdit.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
            this.kryptonRibbonGroupTriple3,
            this.kryptonRibbonGroupLines2});
            this.GroupFileSystemEdit.KeyTipGroup = "E";
            this.GroupFileSystemEdit.TextLine1 = "Edit";
            // 
            // kryptonRibbonGroupTriple3
            // 
            this.kryptonRibbonGroupTriple3.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonFileSystemPaste});
            // 
            // ButtonFileSystemPaste
            // 
            this.ButtonFileSystemPaste.Enabled = false;
            this.ButtonFileSystemPaste.ImageLarge = global::Gorgon.Editor.Properties.Resources.paste_48x48;
            this.ButtonFileSystemPaste.ImageSmall = global::Gorgon.Editor.Properties.Resources.paste_16x16;
            this.ButtonFileSystemPaste.KeyTip = "V";
            this.ButtonFileSystemPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.ButtonFileSystemPaste.TextLine1 = "Paste";
            this.ButtonFileSystemPaste.ToolTipBody = "Paste the copied or cut file or directory from the editor.";
            this.ButtonFileSystemPaste.Click += new System.EventHandler(this.ButtonFileSystemPaste_Click);
            // 
            // kryptonRibbonGroupLines2
            // 
            this.kryptonRibbonGroupLines2.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonFileSystemCopy,
            this.ButtonFileSystemCut});
            // 
            // ButtonFileSystemCopy
            // 
            this.ButtonFileSystemCopy.Enabled = false;
            this.ButtonFileSystemCopy.ImageLarge = global::Gorgon.Editor.Properties.Resources.copy_48x48;
            this.ButtonFileSystemCopy.ImageSmall = global::Gorgon.Editor.Properties.Resources.copy_16x16;
            this.ButtonFileSystemCopy.KeyTip = "C";
            this.ButtonFileSystemCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.ButtonFileSystemCopy.TextLine1 = "Copy";
            this.ButtonFileSystemCopy.ToolTipBody = "Copy the selected file/directory to the clipboard.";
            this.ButtonFileSystemCopy.Click += new System.EventHandler(this.ButtonFileSystemCopy_Click);
            // 
            // ButtonFileSystemCut
            // 
            this.ButtonFileSystemCut.Enabled = false;
            this.ButtonFileSystemCut.ImageLarge = global::Gorgon.Editor.Properties.Resources.cut_48x48;
            this.ButtonFileSystemCut.ImageSmall = global::Gorgon.Editor.Properties.Resources.cut_16x16;
            this.ButtonFileSystemCut.KeyTip = "X";
            this.ButtonFileSystemCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.ButtonFileSystemCut.TextLine1 = "Cut";
            this.ButtonFileSystemCut.ToolTipBody = "Cut the selected file or directory into the clipboard.";
            this.ButtonFileSystemCut.Click += new System.EventHandler(this.ButtonFileSystemCut_Click);
            // 
            // RibbonGroupFileSystemOrganize
            // 
            this.RibbonGroupFileSystemOrganize.AllowCollapsed = false;
            this.RibbonGroupFileSystemOrganize.DialogBoxLauncher = false;
            this.RibbonGroupFileSystemOrganize.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
            this.kryptonRibbonGroupTriple2,
            this.kryptonRibbonGroupSeparator1,
            this.kryptonRibbonGroupTriple4});
            this.RibbonGroupFileSystemOrganize.KeyTipGroup = "O";
            this.RibbonGroupFileSystemOrganize.TextLine1 = "Organize";
            // 
            // kryptonRibbonGroupTriple2
            // 
            this.kryptonRibbonGroupTriple2.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonFileSystemDelete,
            this.ButtonFileSystemDeleteAll});
            // 
            // ButtonFileSystemDelete
            // 
            this.ButtonFileSystemDelete.Enabled = false;
            this.ButtonFileSystemDelete.ImageLarge = global::Gorgon.Editor.Properties.Resources.icons8_48x48;
            this.ButtonFileSystemDelete.ImageSmall = global::Gorgon.Editor.Properties.Resources.icons8_16x16;
            this.ButtonFileSystemDelete.KeyTip = "L";
            this.ButtonFileSystemDelete.TextLine1 = "Delete";
            this.ButtonFileSystemDelete.ToolTipBody = "Delete the selected file or directory.";
            this.ButtonFileSystemDelete.Click += new System.EventHandler(this.ButtonFileSystemDelete_Click);
            // 
            // ButtonFileSystemDeleteAll
            // 
            this.ButtonFileSystemDeleteAll.Enabled = false;
            this.ButtonFileSystemDeleteAll.ImageLarge = global::Gorgon.Editor.Properties.Resources.clear_fs_48x48;
            this.ButtonFileSystemDeleteAll.ImageSmall = global::Gorgon.Editor.Properties.Resources.clear_fs_16x16;
            this.ButtonFileSystemDeleteAll.KeyTip = "L";
            this.ButtonFileSystemDeleteAll.TextLine1 = "Delete all";
            this.ButtonFileSystemDeleteAll.ToolTipBody = "Delete all files and directories from the file system.";
            this.ButtonFileSystemDeleteAll.Visible = false;
            this.ButtonFileSystemDeleteAll.Click += new System.EventHandler(this.ButtonFileSystemDeleteAll_Click);
            // 
            // kryptonRibbonGroupTriple4
            // 
            this.kryptonRibbonGroupTriple4.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonFileSystemRename});
            // 
            // ButtonFileSystemRename
            // 
            this.ButtonFileSystemRename.Enabled = false;
            this.ButtonFileSystemRename.ImageLarge = global::Gorgon.Editor.Properties.Resources.rename_48x48;
            this.ButtonFileSystemRename.ImageSmall = global::Gorgon.Editor.Properties.Resources.rename_16x16;
            this.ButtonFileSystemRename.KeyTip = "R";
            this.ButtonFileSystemRename.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.ButtonFileSystemRename.TextLine1 = "Rename";
            this.ButtonFileSystemRename.ToolTipBody = "Rename the selected file or directory.";
            this.ButtonFileSystemRename.Click += new System.EventHandler(this.ButtonFileSystemRename_Click);
            // 
            // RibbonTabEditorTools
            // 
            this.RibbonTabEditorTools.Text = "Editor Tools";
            this.RibbonTabEditorTools.Visible = false;
            // 
            // Stage
            // 
            this.Stage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Stage.CanOpen = true;
            this.Stage.CanSaveAs = true;
            this.Stage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Stage.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Stage.ForeColor = System.Drawing.Color.White;
            this.Stage.IsStartup = true;
            this.Stage.Location = new System.Drawing.Point(0, 115);
            this.Stage.Name = "Stage";
            this.Stage.Size = new System.Drawing.Size(1051, 511);
            this.Stage.TabIndex = 1;
            this.Stage.BackClicked += new System.EventHandler(this.StageLive_BackClicked);
            this.Stage.BrowseClicked += new System.EventHandler(this.StageLive_BrowseClicked);
            this.Stage.OpenPackFileClicked += new System.EventHandler(this.Stage_OpenClicked);
            this.Stage.SaveClicked += new System.EventHandler<Gorgon.Editor.Views.SaveEventArgs>(this.StageLive_Save);
            // 
            // PanelProject
            // 
            this.PanelProject.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.PanelProject.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelProject.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.PanelProject.ForeColor = System.Drawing.Color.White;
            this.PanelProject.Location = new System.Drawing.Point(0, 0);
            this.PanelProject.Name = "PanelProject";
            this.PanelProject.Size = new System.Drawing.Size(1051, 626);
            this.PanelProject.TabIndex = 0;
            this.PanelProject.RibbonAdded += new System.EventHandler<Gorgon.Editor.ContentRibbonEventArgs>(this.PanelProject_RibbonAdded);
            this.PanelProject.RibbonRemoved += new System.EventHandler<Gorgon.Editor.ContentRibbonEventArgs>(this.PanelProject_RibbonRemoved);
            this.PanelProject.FileExplorerContextChanged += new System.EventHandler(this.PanelProject_FileExplorerContextChanged);
            this.PanelProject.FileExplorerIsRenaming += new System.EventHandler(this.PanelProject_FileExplorerIsRenaming);
            // 
            // PanelWorkSpace
            // 
            this.PanelWorkSpace.Controls.Add(this.PanelProject);
            this.PanelWorkSpace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelWorkSpace.Location = new System.Drawing.Point(0, 0);
            this.PanelWorkSpace.Name = "PanelWorkSpace";
            this.PanelWorkSpace.Size = new System.Drawing.Size(1051, 626);
            this.PanelWorkSpace.TabIndex = 0;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.ClientSize = new System.Drawing.Size(1051, 626);
            this.Controls.Add(this.Stage);
            this.Controls.Add(this.ProgressScreen);
            this.Controls.Add(this.WaitScreen);
            this.Controls.Add(this.RibbonMain);
            this.Controls.Add(this.PanelWorkSpace);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormMain";
            this.PaletteMode = ComponentFactory.Krypton.Toolkit.PaletteMode.Office365Black;
            this.ShadowValues.BlurDistance = 1D;
            this.ShadowValues.Colour = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ShadowValues.Offset = new System.Drawing.Point(0, 0);
            this.ShadowValues.Opacity = 76D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.StateCommon.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.StateCommon.Border.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.StateCommon.Border.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.StateCommon.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.StateCommon.Header.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.StateCommon.Header.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.StateCommon.Header.Back.Image = global::Gorgon.Editor.Properties.Resources.windo_deco_129x39;
            this.StateCommon.Header.Back.ImageAlign = ComponentFactory.Krypton.Toolkit.PaletteRectangleAlign.Local;
            this.StateCommon.Header.Back.ImageStyle = ComponentFactory.Krypton.Toolkit.PaletteImageStyle.BottomRight;
            this.StateCommon.Header.ButtonEdgeInset = 8;
            this.StateCommon.Header.ButtonPadding = new System.Windows.Forms.Padding(3);
            this.StateCommon.Header.Content.ShortText.Color1 = System.Drawing.Color.White;
            this.StateCommon.Header.Content.ShortText.Color2 = System.Drawing.Color.White;
            this.StateCommon.Header.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StateCommon.Header.Content.ShortText.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.StateInactive.Header.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.StateInactive.Header.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.StateInactive.Header.Border.Color1 = System.Drawing.Color.Black;
            this.StateInactive.Header.Border.Color2 = System.Drawing.Color.Black;
            this.StateInactive.Header.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.StateInactive.Header.Content.ShortText.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.StateInactive.Header.Content.ShortText.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.StateInactive.Header.Content.ShortText.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.Activated += new System.EventHandler(this.FormMain_Activated);
            ((System.ComponentModel.ISupportInitialize)(this.RibbonMain)).EndInit();
            this.PanelWorkSpace.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
        private ComponentFactory.Krypton.Ribbon.KryptonRibbon RibbonMain;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonTab TabFileSystem;
        private Gorgon.UI.GorgonWaitScreenPanel WaitScreen;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup RibbonGroupFileSystemNew;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup RibbonGroupFileSystemOrganize;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple2;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonFileSystemDelete;
        private Gorgon.UI.GorgonProgressScreenPanel ProgressScreen;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup GroupFileSystemEdit;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple3;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonFileSystemPaste;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines2;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonFileSystemCopy;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonFileSystemCut;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonFileSystemDeleteAll;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines3;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonImport;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonExport;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator SepCreate;
        private Stage Stage;
        private ProjectContainer PanelProject;
        private System.Windows.Forms.Panel PanelWorkSpace;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple4;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonFileSystemRename;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple5;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonOpenContent;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonFileSystemNewDirectory;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple GroupCreate;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonCreate;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupSeparator kryptonRibbonGroupSeparator2;
        private System.Windows.Forms.ContextMenuStrip MenuCreate;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonTab RibbonTabEditorTools;
        private ComponentFactory.Krypton.Toolkit.KryptonPalette MainPalette;
    }
}

