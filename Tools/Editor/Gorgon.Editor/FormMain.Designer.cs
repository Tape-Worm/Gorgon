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
			ComponentFactory.Krypton.Toolkit.Values.PopupPositionValues popupPositionValues1 = new ComponentFactory.Krypton.Toolkit.Values.PopupPositionValues();
			this.AppThemeManager = new ComponentFactory.Krypton.Toolkit.KryptonManager(this.components);
			this.AppPalette = new ComponentFactory.Krypton.Toolkit.KryptonPalette(this.components);
			this.ProgressScreen = new Gorgon.UI.GorgonProgressScreenPanel();
			this.WaitScreen = new Gorgon.UI.GorgonWaitScreenPanel();
			this.RibbonMain = new ComponentFactory.Krypton.Ribbon.KryptonRibbon();
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
			this.kryptonRibbonGroupLines1 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
			this.ButtonExpand = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
			this.ButtonCollapse = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
			this.Stage = new Gorgon.Editor.Views.Stage();
			this.PanelProject = new Gorgon.Editor.Views.EditorProject();
			this.PanelWorkSpace = new System.Windows.Forms.Panel();
			this.RibbonTabEditorTools = new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab();
			((System.ComponentModel.ISupportInitialize)(this.RibbonMain)).BeginInit();
			this.PanelWorkSpace.SuspendLayout();
			this.SuspendLayout();
			// 
			// AppThemeManager
			// 
			this.AppThemeManager.GlobalPalette = this.AppPalette;
			this.AppThemeManager.GlobalPaletteMode = ComponentFactory.Krypton.Toolkit.PaletteModeManager.Custom;
			// 
			// AppPalette
			// 
			this.AppPalette.BasePaletteMode = ComponentFactory.Krypton.Toolkit.PaletteMode.Office365Black;
			this.AppPalette.ButtonStyles.ButtonAlternate.OverrideDefault.Back.Color1 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonAlternate.OverrideDefault.Back.Color2 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonAlternate.OverrideDefault.Back.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
			this.AppPalette.ButtonStyles.ButtonAlternate.OverrideDefault.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonAlternate.OverrideDefault.Border.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
			this.AppPalette.ButtonStyles.ButtonAlternate.OverrideDefault.Border.Rounding = 6;
			this.AppPalette.ButtonStyles.ButtonAlternate.OverrideDefault.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonAlternate.OverrideDefault.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonAlternate.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ButtonStyles.ButtonAlternate.StateCommon.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ButtonStyles.ButtonAlternate.StateCommon.Back.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.None;
			this.AppPalette.ButtonStyles.ButtonAlternate.StateCommon.Border.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ButtonStyles.ButtonAlternate.StateCommon.Border.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ButtonStyles.ButtonAlternate.StateCommon.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonAlternate.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.AppPalette.ButtonStyles.ButtonCommon.OverrideDefault.Back.Color1 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCommon.StateCheckedNormal.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(130)))), ((int)(((byte)(40)))));
			this.AppPalette.ButtonStyles.ButtonCommon.StateCheckedNormal.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(130)))), ((int)(((byte)(40)))));
			this.AppPalette.ButtonStyles.ButtonCommon.StateCheckedNormal.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.ButtonStyles.ButtonCommon.StateCheckedNormal.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCommon.StateCheckedNormal.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.AppPalette.ButtonStyles.ButtonCommon.StateCheckedPressed.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(148)))), ((int)(((byte)(100)))));
			this.AppPalette.ButtonStyles.ButtonCommon.StateCheckedPressed.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(148)))), ((int)(((byte)(100)))));
			this.AppPalette.ButtonStyles.ButtonCommon.StateCheckedPressed.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.ButtonStyles.ButtonCommon.StateCheckedPressed.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCommon.StateCheckedTracking.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(146)))), ((int)(((byte)(121)))));
			this.AppPalette.ButtonStyles.ButtonCommon.StateCheckedTracking.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(146)))), ((int)(((byte)(121)))));
			this.AppPalette.ButtonStyles.ButtonCommon.StateCheckedTracking.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.ButtonStyles.ButtonCommon.StateCheckedTracking.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCommon.StateCommon.Back.Color1 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCommon.StateCommon.Back.Color2 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCommon.StateCommon.Border.Color1 = System.Drawing.Color.Black;
			this.AppPalette.ButtonStyles.ButtonCommon.StateCommon.Border.Color2 = System.Drawing.Color.Black;
			this.AppPalette.ButtonStyles.ButtonCommon.StateCommon.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.ButtonStyles.ButtonCommon.StateCommon.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCommon.StateCommon.Border.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
			this.AppPalette.ButtonStyles.ButtonCommon.StateCommon.Border.Rounding = 0;
			this.AppPalette.ButtonStyles.ButtonCommon.StateCommon.Content.LongText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCommon.StateCommon.Content.LongText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCommon.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCommon.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCommon.StateDisabled.Border.Color1 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCommon.StateDisabled.Border.Color2 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCommon.StateDisabled.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCommon.StateDisabled.Border.Rounding = 0;
			this.AppPalette.ButtonStyles.ButtonCommon.StateDisabled.Content.LongText.Color1 = System.Drawing.Color.DimGray;
			this.AppPalette.ButtonStyles.ButtonCommon.StateDisabled.Content.LongText.Color2 = System.Drawing.Color.DimGray;
			this.AppPalette.ButtonStyles.ButtonCommon.StateDisabled.Content.ShortText.Color1 = System.Drawing.Color.DimGray;
			this.AppPalette.ButtonStyles.ButtonCommon.StateDisabled.Content.ShortText.Color2 = System.Drawing.Color.DimGray;
			this.AppPalette.ButtonStyles.ButtonCommon.StatePressed.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(109)))), ((int)(((byte)(131)))));
			this.AppPalette.ButtonStyles.ButtonCommon.StatePressed.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(109)))), ((int)(((byte)(131)))));
			this.AppPalette.ButtonStyles.ButtonCommon.StatePressed.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.ButtonStyles.ButtonCommon.StatePressed.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCommon.StateTracking.Back.Color1 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonCommon.StateTracking.Back.Color2 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonCommon.StateTracking.Content.LongText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCommon.StateTracking.Content.LongText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCommon.StateTracking.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCommon.StateTracking.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom1.OverrideDefault.Back.Color1 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom1.OverrideDefault.Back.Color2 = System.Drawing.Color.Silver;
			this.AppPalette.ButtonStyles.ButtonCustom1.OverrideDefault.Content.ShortText.Color1 = System.Drawing.Color.LightSkyBlue;
			this.AppPalette.ButtonStyles.ButtonCustom1.OverrideDefault.Content.ShortText.Color2 = System.Drawing.Color.LightSkyBlue;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedNormal.Back.Color1 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedNormal.Back.Color2 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedNormal.Back.ColorAlign = ComponentFactory.Krypton.Toolkit.PaletteRectangleAlign.Local;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedNormal.Back.ColorAngle = 0F;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedNormal.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Switch90;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedNormal.Border.Color1 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedNormal.Border.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(109)))), ((int)(((byte)(131)))));
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedNormal.Border.ColorAlign = ComponentFactory.Krypton.Toolkit.PaletteRectangleAlign.Local;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedNormal.Border.ColorAngle = 0F;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedNormal.Border.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.SolidInside;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedNormal.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.True;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedNormal.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedNormal.Border.Rounding = 6;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedNormal.Border.Width = 2;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedNormal.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedNormal.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedPressed.Back.Color1 = System.Drawing.Color.Gray;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedPressed.Back.Color2 = System.Drawing.Color.Gray;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedPressed.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedPressed.Border.Rounding = 10;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedPressed.Border.Width = 2;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedPressed.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedPressed.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedTracking.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(146)))), ((int)(((byte)(121)))));
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedTracking.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(146)))), ((int)(((byte)(121)))));
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedTracking.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedTracking.Border.Rounding = 6;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedTracking.Border.Width = 2;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedTracking.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCheckedTracking.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCommon.Back.Color2 = System.Drawing.Color.Silver;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCommon.Border.Color1 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCommon.Border.Color2 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCommon.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCommon.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCommon.Border.Rounding = 6;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCommon.Border.Width = 2;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCommon.Content.Image.ImageH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCommon.Content.Image.ImageV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 12F);
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCommon.Content.ShortText.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateCommon.Content.ShortText.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Far;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateDisabled.Back.Color1 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateDisabled.Back.Color2 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateDisabled.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom1.StateDisabled.Border.Rounding = 6;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateDisabled.Border.Width = 2;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateDisabled.Content.ShortText.Color1 = System.Drawing.Color.DimGray;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateDisabled.Content.ShortText.Color2 = System.Drawing.Color.DimGray;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateNormal.Back.Color1 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateNormal.Back.Color2 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateNormal.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom1.StateNormal.Border.Rounding = 6;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateNormal.Border.Width = 2;
			this.AppPalette.ButtonStyles.ButtonCustom1.StatePressed.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(109)))), ((int)(((byte)(131)))));
			this.AppPalette.ButtonStyles.ButtonCustom1.StatePressed.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(109)))), ((int)(((byte)(131)))));
			this.AppPalette.ButtonStyles.ButtonCustom1.StatePressed.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom1.StatePressed.Border.Rounding = 10;
			this.AppPalette.ButtonStyles.ButtonCustom1.StatePressed.Border.Width = 2;
			this.AppPalette.ButtonStyles.ButtonCustom1.StatePressed.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom1.StatePressed.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateTracking.Back.Color1 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateTracking.Back.Color2 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateTracking.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom1.StateTracking.Border.Rounding = 6;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateTracking.Border.Width = 2;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateTracking.Content.ShortText.Color1 = System.Drawing.Color.Silver;
			this.AppPalette.ButtonStyles.ButtonCustom1.StateTracking.Content.ShortText.Color2 = System.Drawing.Color.Silver;
			this.AppPalette.ButtonStyles.ButtonCustom2.OverrideDefault.Back.Color1 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom2.OverrideDefault.Back.Color2 = System.Drawing.Color.Silver;
			this.AppPalette.ButtonStyles.ButtonCustom2.OverrideDefault.Border.Color1 = System.Drawing.Color.LightSkyBlue;
			this.AppPalette.ButtonStyles.ButtonCustom2.OverrideDefault.Border.Color2 = System.Drawing.Color.LightSkyBlue;
			this.AppPalette.ButtonStyles.ButtonCustom2.OverrideDefault.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom2.OverrideDefault.Content.ShortText.Color1 = System.Drawing.Color.LightSkyBlue;
			this.AppPalette.ButtonStyles.ButtonCustom2.OverrideDefault.Content.ShortText.Color2 = System.Drawing.Color.LightSkyBlue;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedNormal.Back.Color1 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedNormal.Back.Color2 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedNormal.Back.ColorAlign = ComponentFactory.Krypton.Toolkit.PaletteRectangleAlign.Local;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedNormal.Back.ColorAngle = 0F;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedNormal.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Switch90;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedNormal.Border.Color1 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedNormal.Border.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(109)))), ((int)(((byte)(131)))));
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedNormal.Border.ColorAlign = ComponentFactory.Krypton.Toolkit.PaletteRectangleAlign.Local;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedNormal.Border.ColorAngle = 0F;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedNormal.Border.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.SolidInside;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedNormal.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.True;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedNormal.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedNormal.Border.Rounding = 6;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedNormal.Border.Width = 2;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedNormal.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedNormal.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedPressed.Back.Color1 = System.Drawing.Color.Gray;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedPressed.Back.Color2 = System.Drawing.Color.Gray;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedPressed.Border.Color1 = System.Drawing.Color.Gray;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedPressed.Border.Color2 = System.Drawing.Color.Gray;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedPressed.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedPressed.Border.Rounding = 10;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedPressed.Border.Width = 2;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedPressed.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedPressed.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedTracking.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(146)))), ((int)(((byte)(121)))));
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedTracking.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(146)))), ((int)(((byte)(121)))));
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedTracking.Border.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(146)))), ((int)(((byte)(121)))));
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedTracking.Border.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(146)))), ((int)(((byte)(121)))));
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedTracking.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedTracking.Border.Rounding = 6;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedTracking.Border.Width = 2;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedTracking.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCheckedTracking.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCommon.Back.Color2 = System.Drawing.Color.Silver;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCommon.Border.Color1 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCommon.Border.Color2 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCommon.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.True;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCommon.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCommon.Border.Rounding = 6;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCommon.Border.Width = 2;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCommon.Content.Image.ImageH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Near;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCommon.Content.Image.ImageV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCommon.Content.ShortText.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateCommon.Content.ShortText.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateDisabled.Back.Color1 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateDisabled.Back.Color2 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateDisabled.Border.Color1 = System.Drawing.Color.DimGray;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateDisabled.Border.Color2 = System.Drawing.Color.DimGray;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateDisabled.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom2.StateDisabled.Border.Rounding = 6;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateDisabled.Border.Width = 2;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateDisabled.Content.ShortText.Color1 = System.Drawing.Color.DimGray;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateDisabled.Content.ShortText.Color2 = System.Drawing.Color.DimGray;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateNormal.Back.Color1 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateNormal.Back.Color2 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateNormal.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom2.StateNormal.Border.Rounding = 6;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateNormal.Border.Width = 2;
			this.AppPalette.ButtonStyles.ButtonCustom2.StatePressed.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(109)))), ((int)(((byte)(131)))));
			this.AppPalette.ButtonStyles.ButtonCustom2.StatePressed.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(109)))), ((int)(((byte)(131)))));
			this.AppPalette.ButtonStyles.ButtonCustom2.StatePressed.Border.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(109)))), ((int)(((byte)(131)))));
			this.AppPalette.ButtonStyles.ButtonCustom2.StatePressed.Border.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(109)))), ((int)(((byte)(131)))));
			this.AppPalette.ButtonStyles.ButtonCustom2.StatePressed.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom2.StatePressed.Border.Rounding = 10;
			this.AppPalette.ButtonStyles.ButtonCustom2.StatePressed.Border.Width = 2;
			this.AppPalette.ButtonStyles.ButtonCustom2.StatePressed.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom2.StatePressed.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateTracking.Back.Color1 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateTracking.Back.Color2 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateTracking.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom2.StateTracking.Border.Rounding = 6;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateTracking.Border.Width = 2;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateTracking.Content.ShortText.Color1 = System.Drawing.Color.Silver;
			this.AppPalette.ButtonStyles.ButtonCustom2.StateTracking.Content.ShortText.Color2 = System.Drawing.Color.Silver;
			this.AppPalette.ButtonStyles.ButtonCustom3.OverrideDefault.Back.Color1 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom3.OverrideDefault.Back.Color2 = System.Drawing.Color.Silver;
			this.AppPalette.ButtonStyles.ButtonCustom3.OverrideDefault.Content.ShortText.Color1 = System.Drawing.Color.LightSkyBlue;
			this.AppPalette.ButtonStyles.ButtonCustom3.OverrideDefault.Content.ShortText.Color2 = System.Drawing.Color.LightSkyBlue;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCheckedNormal.Back.Color1 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCheckedNormal.Back.Color2 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCheckedNormal.Back.ColorAlign = ComponentFactory.Krypton.Toolkit.PaletteRectangleAlign.Local;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCheckedNormal.Back.ColorAngle = 0F;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCheckedNormal.Border.ColorAngle = 0F;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCheckedNormal.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCheckedNormal.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCheckedNormal.Border.Width = 0;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCheckedNormal.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCheckedNormal.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCheckedPressed.Back.Color1 = System.Drawing.Color.Gray;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCheckedPressed.Back.Color2 = System.Drawing.Color.Gray;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCheckedPressed.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCheckedPressed.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCheckedTracking.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(146)))), ((int)(((byte)(121)))));
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCheckedTracking.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(146)))), ((int)(((byte)(121)))));
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCheckedTracking.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCheckedTracking.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCommon.Back.Color2 = System.Drawing.Color.Silver;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCommon.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCommon.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCommon.Border.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.None;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCommon.Border.Rounding = 2;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCommon.Border.Width = 0;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCommon.Content.Image.ImageH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Near;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCommon.Content.Image.ImageV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCommon.Content.ShortText.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateCommon.Content.ShortText.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateDisabled.Back.Color1 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateDisabled.Back.Color2 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateDisabled.Content.ShortText.Color1 = System.Drawing.Color.DimGray;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateDisabled.Content.ShortText.Color2 = System.Drawing.Color.DimGray;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateNormal.Back.Color1 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateNormal.Back.Color2 = System.Drawing.Color.Transparent;
			this.AppPalette.ButtonStyles.ButtonCustom3.StatePressed.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(109)))), ((int)(((byte)(131)))));
			this.AppPalette.ButtonStyles.ButtonCustom3.StatePressed.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(109)))), ((int)(((byte)(131)))));
			this.AppPalette.ButtonStyles.ButtonCustom3.StatePressed.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom3.StatePressed.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateTracking.Back.Color1 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateTracking.Back.Color2 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateTracking.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonCustom3.StateTracking.Border.Rounding = 3;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateTracking.Content.ShortText.Color1 = System.Drawing.Color.Silver;
			this.AppPalette.ButtonStyles.ButtonCustom3.StateTracking.Content.ShortText.Color2 = System.Drawing.Color.Silver;
			this.AppPalette.ButtonStyles.ButtonListItem.OverrideDefault.Back.Color1 = System.Drawing.Color.Gray;
			this.AppPalette.ButtonStyles.ButtonListItem.OverrideDefault.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonListItem.OverrideDefault.Border.Rounding = 6;
			this.AppPalette.ButtonStyles.ButtonListItem.OverrideDefault.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonListItem.OverrideDefault.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonListItem.StateCheckedNormal.Back.Color1 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonListItem.StateCheckedNormal.Back.Color2 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonListItem.StateCheckedNormal.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonListItem.StateCheckedNormal.Border.Rounding = 3;
			this.AppPalette.ButtonStyles.ButtonListItem.StateCheckedNormal.Border.Width = 0;
			this.AppPalette.ButtonStyles.ButtonListItem.StateCheckedNormal.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonListItem.StateCheckedNormal.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonListItem.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ButtonStyles.ButtonListItem.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonListItem.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ButtonStyles.ButtonListItem.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.AppPalette.ButtonStyles.ButtonListItem.StateDisabled.Content.ShortText.Color1 = System.Drawing.Color.DimGray;
			this.AppPalette.ButtonStyles.ButtonListItem.StateDisabled.Content.ShortText.Color2 = System.Drawing.Color.DimGray;
			this.AppPalette.ButtonStyles.ButtonListItem.StateTracking.Back.Color1 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonListItem.StateTracking.Back.Color2 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ButtonStyles.ButtonListItem.StateTracking.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ButtonStyles.ButtonListItem.StateTracking.Border.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
			this.AppPalette.ButtonStyles.ButtonListItem.StateTracking.Border.Rounding = 6;
			this.AppPalette.Common.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(104)))), ((int)(((byte)(104)))), ((int)(((byte)(104)))));
			this.AppPalette.Common.StateCommon.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(104)))), ((int)(((byte)(104)))), ((int)(((byte)(104)))));
			this.AppPalette.Common.StateCommon.Back.ColorAlign = ComponentFactory.Krypton.Toolkit.PaletteRectangleAlign.Form;
			this.AppPalette.Common.StateCommon.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
			this.AppPalette.Common.StateCommon.Back.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.True;
			this.AppPalette.Common.StateCommon.Back.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
			this.AppPalette.Common.StateCommon.Content.LongText.Color1 = System.Drawing.Color.White;
			this.AppPalette.Common.StateCommon.Content.LongText.Color2 = System.Drawing.Color.White;
			this.AppPalette.Common.StateCommon.Content.LongText.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.AppPalette.Common.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.Common.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.Common.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.AppPalette.Common.StateDisabled.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(104)))), ((int)(((byte)(104)))), ((int)(((byte)(104)))));
			this.AppPalette.Common.StateDisabled.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(104)))), ((int)(((byte)(104)))), ((int)(((byte)(104)))));
			this.AppPalette.Common.StateDisabled.Back.ColorAlign = ComponentFactory.Krypton.Toolkit.PaletteRectangleAlign.Form;
			this.AppPalette.Common.StateDisabled.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
			this.AppPalette.Common.StateDisabled.Back.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.True;
			this.AppPalette.Common.StateDisabled.Back.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
			this.AppPalette.Common.StateDisabled.Content.LongText.Color1 = System.Drawing.Color.Silver;
			this.AppPalette.Common.StateDisabled.Content.LongText.Color2 = System.Drawing.Color.Silver;
			this.AppPalette.Common.StateDisabled.Content.LongText.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.AppPalette.Common.StateDisabled.Content.ShortText.Color1 = System.Drawing.Color.Silver;
			this.AppPalette.Common.StateDisabled.Content.ShortText.Color2 = System.Drawing.Color.Silver;
			this.AppPalette.Common.StateDisabled.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.AppPalette.ContextMenu.StateChecked.ItemImage.Back.Color1 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ContextMenu.StateChecked.ItemImage.Back.Color2 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ContextMenu.StateChecked.ItemImage.Border.Color1 = System.Drawing.Color.CornflowerBlue;
			this.AppPalette.ContextMenu.StateChecked.ItemImage.Border.Color2 = System.Drawing.Color.CornflowerBlue;
			this.AppPalette.ContextMenu.StateChecked.ItemImage.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.ContextMenu.StateChecked.ItemImage.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ContextMenu.StateChecked.ItemImage.Border.Rounding = 2;
			this.AppPalette.ContextMenu.StateChecked.ItemImage.Content.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.ContextMenu.StateChecked.ItemImage.Content.Image.Effect = ComponentFactory.Krypton.Toolkit.PaletteImageEffect.LightLight;
			this.AppPalette.ContextMenu.StateChecked.ItemImage.Content.Image.ImageColorMap = System.Drawing.Color.White;
			this.AppPalette.ContextMenu.StateChecked.ItemImage.Content.Image.ImageColorTo = System.Drawing.Color.Fuchsia;
			this.AppPalette.ContextMenu.StateCommon.ControlInner.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ContextMenu.StateCommon.ControlInner.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ContextMenu.StateCommon.ControlInner.Back.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.ContextMenu.StateCommon.ControlInner.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ContextMenu.StateCommon.ControlOuter.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ContextMenu.StateCommon.ControlOuter.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ContextMenu.StateCommon.ControlOuter.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.ContextMenu.StateCommon.ControlOuter.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ContextMenu.StateCommon.Heading.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ContextMenu.StateCommon.Heading.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ContextMenu.StateCommon.Heading.Border.Color1 = System.Drawing.Color.Silver;
			this.AppPalette.ContextMenu.StateCommon.Heading.Border.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
			this.AppPalette.ContextMenu.StateCommon.Heading.Border.ColorAngle = 180F;
			this.AppPalette.ContextMenu.StateCommon.Heading.Border.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Rounded;
			this.AppPalette.ContextMenu.StateCommon.Heading.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom;
			this.AppPalette.ContextMenu.StateCommon.Heading.Border.Width = 1;
			this.AppPalette.ContextMenu.StateCommon.Heading.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ContextMenu.StateCommon.Heading.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.AppPalette.ContextMenu.StateCommon.ItemHighlight.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ContextMenu.StateCommon.ItemHighlight.Back.Color2 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ContextMenu.StateCommon.ItemHighlight.Back.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.None;
			this.AppPalette.ContextMenu.StateCommon.ItemHighlight.Border.Color1 = System.Drawing.Color.SteelBlue;
			this.AppPalette.ContextMenu.StateCommon.ItemHighlight.Border.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ContextMenu.StateCommon.ItemHighlight.Border.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
			this.AppPalette.ContextMenu.StateCommon.ItemHighlight.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ContextMenu.StateCommon.ItemHighlight.Border.Rounding = 2;
			this.AppPalette.ContextMenu.StateCommon.ItemHighlight.Border.Width = 2;
			this.AppPalette.ContextMenu.StateCommon.ItemImageColumn.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ContextMenu.StateCommon.ItemImageColumn.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ContextMenu.StateCommon.ItemImageColumn.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.ContextMenu.StateCommon.ItemImageColumn.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ContextMenu.StateCommon.ItemImageColumn.Border.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.None;
			this.AppPalette.ContextMenu.StateCommon.Separator.Back.Color1 = System.Drawing.Color.Silver;
			this.AppPalette.ContextMenu.StateCommon.Separator.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ContextMenu.StateCommon.Separator.Back.ColorAlign = ComponentFactory.Krypton.Toolkit.PaletteRectangleAlign.Local;
			this.AppPalette.ContextMenu.StateCommon.Separator.Back.ColorAngle = 180F;
			this.AppPalette.ContextMenu.StateCommon.Separator.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Linear50;
			this.AppPalette.ContextMenu.StateCommon.Separator.Back.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.None;
			this.AppPalette.ContextMenu.StateCommon.Separator.Border.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ContextMenu.StateCommon.Separator.Border.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ContextMenu.StateCommon.Separator.Border.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
			this.AppPalette.ContextMenu.StateCommon.Separator.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ContextMenu.StateCommon.Separator.Border.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.None;
			this.AppPalette.ContextMenu.StateNormal.ItemShortcutText.ShortText.Color1 = System.Drawing.Color.Silver;
			this.AppPalette.ContextMenu.StateNormal.ItemShortcutText.ShortText.Color2 = System.Drawing.Color.Silver;
			this.AppPalette.ContextMenu.StateNormal.ItemShortcutText.ShortText.Font = new System.Drawing.Font("Segoe UI", 7.25F, System.Drawing.FontStyle.Bold);
			this.AppPalette.ContextMenu.StateNormal.ItemTextStandard.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.ContextMenu.StateNormal.ItemTextStandard.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.ContextMenu.StateNormal.ItemTextStandard.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.AppPalette.ControlStyles.ControlAlternate.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ControlStyles.ControlAlternate.StateCommon.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ControlStyles.ControlAlternate.StateCommon.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.ControlStyles.ControlAlternate.StateCommon.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ControlStyles.ControlAlternate.StateCommon.Border.Rounding = 0;
			this.AppPalette.ControlStyles.ControlAlternate.StateCommon.Border.Width = 0;
			this.AppPalette.ControlStyles.ControlCommon.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ControlStyles.ControlCommon.StateCommon.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ControlStyles.ControlCommon.StateCommon.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.ControlStyles.ControlCommon.StateCommon.Border.Width = 0;
			this.AppPalette.ControlStyles.ControlCustom1.StateCommon.Back.Color1 = System.Drawing.Color.Silver;
			this.AppPalette.ControlStyles.ControlCustom1.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
			this.AppPalette.ControlStyles.ControlCustom1.StateCommon.Border.Color2 = System.Drawing.Color.Gray;
			this.AppPalette.ControlStyles.ControlCustom1.StateCommon.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
			this.AppPalette.ControlStyles.ControlCustom1.StateCommon.Border.Rounding = 0;
			this.AppPalette.ControlStyles.ControlCustom1.StateCommon.Border.Width = 1;
			this.AppPalette.ControlStyles.ControlCustom1.StateDisabled.Border.Color1 = System.Drawing.Color.DimGray;
			this.AppPalette.ControlStyles.ControlCustom1.StateDisabled.Border.Color2 = System.Drawing.Color.DimGray;
			this.AppPalette.ControlStyles.ControlCustom1.StateDisabled.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
			this.AppPalette.CustomisedKryptonPaletteFilePath = null;
			this.AppPalette.FormStyles.FormCommon.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.FormStyles.FormCommon.StateCommon.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.FormStyles.FormCommon.StateCommon.Border.Color1 = System.Drawing.Color.SteelBlue;
			this.AppPalette.FormStyles.FormCommon.StateCommon.Border.Color2 = System.Drawing.Color.SteelBlue;
			this.AppPalette.FormStyles.FormCommon.StateCommon.Border.ColorAlign = ComponentFactory.Krypton.Toolkit.PaletteRectangleAlign.Form;
			this.AppPalette.FormStyles.FormCommon.StateCommon.Border.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
			this.AppPalette.FormStyles.FormCommon.StateCommon.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.FormStyles.FormCommon.StateCommon.Border.Rounding = 0;
			this.AppPalette.FormStyles.FormCommon.StateCommon.Border.Width = 2;
			this.AppPalette.FormStyles.FormCommon.StateInactive.Border.Color1 = System.Drawing.Color.Black;
			this.AppPalette.FormStyles.FormCommon.StateInactive.Border.Color2 = System.Drawing.Color.Black;
			this.AppPalette.FormStyles.FormCommon.StateInactive.Border.ColorAlign = ComponentFactory.Krypton.Toolkit.PaletteRectangleAlign.Form;
			this.AppPalette.FormStyles.FormCommon.StateInactive.Border.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
			this.AppPalette.FormStyles.FormCommon.StateInactive.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.True;
			this.AppPalette.FormStyles.FormCommon.StateInactive.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.FormStyles.FormCommon.StateInactive.Border.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
			this.AppPalette.FormStyles.FormCommon.StateInactive.Border.Rounding = 0;
			this.AppPalette.FormStyles.FormCommon.StateInactive.Border.Width = 1;
			this.AppPalette.HeaderStyles.HeaderCommon.StateCommon.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.HeaderStyles.HeaderCommon.StateCommon.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.HeaderStyles.HeaderForm.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.HeaderStyles.HeaderForm.StateCommon.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.HeaderStyles.HeaderForm.StateCommon.Back.Image = global::Gorgon.Editor.Properties.Resources.windo_deco_129x39;
			this.AppPalette.HeaderStyles.HeaderForm.StateCommon.Back.ImageStyle = ComponentFactory.Krypton.Toolkit.PaletteImageStyle.BottomRight;
			this.AppPalette.HeaderStyles.HeaderForm.StateCommon.ButtonEdgeInset = 8;
			this.AppPalette.HeaderStyles.HeaderForm.StateCommon.ButtonPadding = new System.Windows.Forms.Padding(3);
			this.AppPalette.HeaderStyles.HeaderForm.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 14F);
			this.AppPalette.HeaderStyles.HeaderForm.StateDisabled.Content.ShortText.Color1 = System.Drawing.Color.DimGray;
			this.AppPalette.HeaderStyles.HeaderForm.StateDisabled.Content.ShortText.Color2 = System.Drawing.Color.DimGray;
			this.AppPalette.HeaderStyles.HeaderPrimary.StateCommon.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.HeaderStyles.HeaderPrimary.StateCommon.Border.DrawBorders = ((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders)((((ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left) 
            | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.AppPalette.InputControlStyles.InputControlCommon.StateActive.Back.Color1 = System.Drawing.Color.White;
			this.AppPalette.InputControlStyles.InputControlCommon.StateActive.Back.Color2 = System.Drawing.Color.White;
			this.AppPalette.InputControlStyles.InputControlCommon.StateActive.Border.Color1 = System.Drawing.Color.Gray;
			this.AppPalette.InputControlStyles.InputControlCommon.StateActive.Border.Color2 = System.Drawing.Color.Gray;
			this.AppPalette.InputControlStyles.InputControlCommon.StateActive.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom;
			this.AppPalette.InputControlStyles.InputControlCommon.StateActive.Content.ShortText.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.InputControlStyles.InputControlCommon.StateActive.Content.ShortText.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.InputControlStyles.InputControlCommon.StateActive.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.AppPalette.InputControlStyles.InputControlCommon.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.InputControlStyles.InputControlCommon.StateCommon.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.InputControlStyles.InputControlCommon.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
			this.AppPalette.InputControlStyles.InputControlCommon.StateCommon.Border.Color2 = System.Drawing.Color.Transparent;
			this.AppPalette.InputControlStyles.InputControlCommon.StateCommon.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom;
			this.AppPalette.InputControlStyles.InputControlCommon.StateCommon.Border.Rounding = 2;
			this.AppPalette.InputControlStyles.InputControlCommon.StateCommon.Border.Width = 1;
			this.AppPalette.InputControlStyles.InputControlCommon.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.InputControlStyles.InputControlCommon.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.InputControlStyles.InputControlCommon.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.AppPalette.InputControlStyles.InputControlCommon.StateDisabled.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
			this.AppPalette.InputControlStyles.InputControlCommon.StateDisabled.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
			this.AppPalette.InputControlStyles.InputControlCommon.StateDisabled.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.InputControlStyles.InputControlCommon.StateDisabled.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.None;
			this.AppPalette.InputControlStyles.InputControlCommon.StateDisabled.Content.ShortText.Color1 = System.Drawing.Color.DimGray;
			this.AppPalette.InputControlStyles.InputControlCommon.StateDisabled.Content.ShortText.Color2 = System.Drawing.Color.DimGray;
			this.AppPalette.InputControlStyles.InputControlCustom1.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 12F);
			this.AppPalette.InputControlStyles.InputControlCustom1.StateDisabled.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.LabelStyles.LabelBoldControl.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.AppPalette.LabelStyles.LabelCommon.StateCommon.ShortText.Color1 = System.Drawing.Color.White;
			this.AppPalette.LabelStyles.LabelCommon.StateCommon.ShortText.Color2 = System.Drawing.Color.White;
			this.AppPalette.LabelStyles.LabelCommon.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.AppPalette.LabelStyles.LabelCustom1.StateCommon.ShortText.Color1 = System.Drawing.Color.Black;
			this.AppPalette.LabelStyles.LabelCustom1.StateCommon.ShortText.Color2 = System.Drawing.Color.Black;
			this.AppPalette.LabelStyles.LabelCustom1.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.AppPalette.LabelStyles.LabelCustom2.StateCommon.ShortText.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.LabelStyles.LabelCustom2.StateCommon.ShortText.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.LabelStyles.LabelCustom2.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 16F);
			this.AppPalette.LabelStyles.LabelCustom3.StateCommon.ShortText.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.LabelStyles.LabelCustom3.StateCommon.ShortText.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.LabelStyles.LabelCustom3.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 12F);
			this.AppPalette.LabelStyles.LabelKeyTip.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AppPalette.LabelStyles.LabelNormalControl.StateCommon.Padding = new System.Windows.Forms.Padding(0);
			this.AppPalette.LabelStyles.LabelNormalControl.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.AppPalette.LabelStyles.LabelSuperTip.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AppPalette.LabelStyles.LabelTitleControl.StateCommon.DrawFocus = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.LabelStyles.LabelTitleControl.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 18F);
			this.AppPalette.LabelStyles.LabelTitlePanel.StateCommon.ShortText.Color1 = System.Drawing.Color.Silver;
			this.AppPalette.LabelStyles.LabelTitlePanel.StateCommon.ShortText.Color2 = System.Drawing.Color.Silver;
			this.AppPalette.LabelStyles.LabelToolTip.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AppPalette.PanelStyles.PanelClient.StateCommon.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.PanelStyles.PanelClient.StateCommon.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.PanelStyles.PanelClient.StateCommon.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.True;
			this.AppPalette.PanelStyles.PanelClient.StateCommon.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.None;
			this.AppPalette.PanelStyles.PanelCommon.StateCommon.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			this.AppPalette.PanelStyles.PanelCustom1.StateCommon.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
			this.AppPalette.PanelStyles.PanelCustom1.StateCommon.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
			this.AppPalette.PanelStyles.PanelCustom1.StateCommon.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.True;
			this.AppPalette.Ribbon.RibbonGeneral.GroupSeparatorDark = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.Ribbon.RibbonGeneral.GroupSeparatorLight = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
			this.AppPalette.Ribbon.RibbonGeneral.MinimizeBarDarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.Ribbon.RibbonGeneral.MinimizeBarLightColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.Ribbon.RibbonGeneral.TextFont = new System.Drawing.Font("Segoe UI", 9F);
			this.AppPalette.Ribbon.RibbonGroupArea.StateCommon.BackColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
			this.AppPalette.Ribbon.RibbonGroupArea.StateCommon.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
			this.AppPalette.Ribbon.RibbonGroupArea.StateCommon.BackColor3 = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
			this.AppPalette.Ribbon.RibbonGroupArea.StateCommon.BackColor4 = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
			this.AppPalette.Ribbon.RibbonGroupArea.StateCommon.BackColor5 = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
			this.AppPalette.Ribbon.RibbonGroupButtonText.StateCommon.TextColor = System.Drawing.Color.White;
			this.AppPalette.Ribbon.RibbonGroupButtonText.StateDisabled.TextColor = System.Drawing.Color.Black;
			this.AppPalette.Ribbon.RibbonGroupCheckBoxText.StateCommon.TextColor = System.Drawing.Color.White;
			this.AppPalette.Ribbon.RibbonGroupCheckBoxText.StateDisabled.TextColor = System.Drawing.Color.DimGray;
			this.AppPalette.Ribbon.RibbonGroupLabelText.StateCommon.TextColor = System.Drawing.Color.White;
			this.AppPalette.Ribbon.RibbonGroupLabelText.StateDisabled.TextColor = System.Drawing.Color.DimGray;
			this.AppPalette.Ribbon.RibbonGroupNormalBorder.StateCommon.BackColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
			this.AppPalette.Ribbon.RibbonGroupNormalBorder.StateCommon.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
			this.AppPalette.Ribbon.RibbonGroupNormalBorder.StateCommon.BackColor3 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.Ribbon.RibbonGroupNormalBorder.StateCommon.BackColor4 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.Ribbon.RibbonGroupNormalBorder.StateCommon.BackColor5 = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
			this.AppPalette.Ribbon.RibbonGroupNormalTitle.StateCommon.TextColor = System.Drawing.Color.White;
			this.AppPalette.Ribbon.RibbonGroupRadioButtonText.StateCommon.TextColor = System.Drawing.Color.White;
			this.AppPalette.Ribbon.RibbonGroupRadioButtonText.StateDisabled.TextColor = System.Drawing.Color.DimGray;
			this.AppPalette.Ribbon.RibbonTab.StateCommon.BackColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
			this.AppPalette.Ribbon.RibbonTab.StateCommon.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
			this.AppPalette.Ribbon.RibbonTab.StateCommon.BackColor3 = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
			this.AppPalette.Ribbon.RibbonTab.StateCommon.BackColor4 = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
			this.AppPalette.Ribbon.RibbonTab.StateCommon.BackColor5 = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
			this.AppPalette.Ribbon.RibbonTab.StateCommon.TextColor = System.Drawing.Color.White;
			this.AppPalette.Ribbon.RibbonTab.StateTracking.BackColor1 = System.Drawing.Color.SteelBlue;
			this.AppPalette.Ribbon.RibbonTab.StateTracking.BackColor2 = System.Drawing.Color.SteelBlue;
			this.AppPalette.Ribbon.RibbonTab.StateTracking.BackColor3 = System.Drawing.Color.SteelBlue;
			this.AppPalette.Ribbon.RibbonTab.StateTracking.BackColor4 = System.Drawing.Color.SteelBlue;
			this.AppPalette.Ribbon.RibbonTab.StateTracking.BackColor5 = System.Drawing.Color.SteelBlue;
			this.AppPalette.Ribbon.RibbonTab.StateTracking.TextColor = System.Drawing.Color.White;
			this.AppPalette.ToolMenuStatus.Button.ButtonCheckedGradientBegin = System.Drawing.Color.SteelBlue;
			this.AppPalette.ToolMenuStatus.Button.ButtonCheckedGradientEnd = System.Drawing.Color.SteelBlue;
			this.AppPalette.ToolMenuStatus.Button.ButtonCheckedGradientMiddle = System.Drawing.Color.SteelBlue;
			this.AppPalette.ToolMenuStatus.Button.ButtonCheckedHighlight = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			this.AppPalette.ToolMenuStatus.Button.ButtonCheckedHighlightBorder = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			this.AppPalette.ToolMenuStatus.Button.ButtonPressedBorder = System.Drawing.Color.CornflowerBlue;
			this.AppPalette.ToolMenuStatus.Button.ButtonPressedGradientBegin = System.Drawing.Color.CornflowerBlue;
			this.AppPalette.ToolMenuStatus.Button.ButtonPressedGradientEnd = System.Drawing.Color.CornflowerBlue;
			this.AppPalette.ToolMenuStatus.Button.ButtonPressedGradientMiddle = System.Drawing.Color.CornflowerBlue;
			this.AppPalette.ToolMenuStatus.Button.ButtonPressedHighlight = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			this.AppPalette.ToolMenuStatus.Button.ButtonPressedHighlightBorder = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			this.AppPalette.ToolMenuStatus.Button.ButtonSelectedBorder = System.Drawing.Color.SteelBlue;
			this.AppPalette.ToolMenuStatus.Button.ButtonSelectedGradientBegin = System.Drawing.Color.SteelBlue;
			this.AppPalette.ToolMenuStatus.Button.ButtonSelectedGradientEnd = System.Drawing.Color.SteelBlue;
			this.AppPalette.ToolMenuStatus.Button.ButtonSelectedGradientMiddle = System.Drawing.Color.SteelBlue;
			this.AppPalette.ToolMenuStatus.Button.ButtonSelectedHighlight = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			this.AppPalette.ToolMenuStatus.Button.ButtonSelectedHighlightBorder = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			this.AppPalette.ToolMenuStatus.Button.CheckBackground = System.Drawing.Color.SteelBlue;
			this.AppPalette.ToolMenuStatus.Button.CheckPressedBackground = System.Drawing.Color.CornflowerBlue;
			this.AppPalette.ToolMenuStatus.Button.CheckSelectedBackground = System.Drawing.Color.SteelBlue;
			this.AppPalette.ToolMenuStatus.Button.OverflowButtonGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.Button.OverflowButtonGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.Button.OverflowButtonGradientMiddle = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.Grip.GripDark = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.Grip.GripLight = System.Drawing.Color.Silver;
			this.AppPalette.ToolMenuStatus.Menu.ImageMarginGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.Menu.ImageMarginGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.Menu.ImageMarginGradientMiddle = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.Menu.ImageMarginRevealedGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.Menu.ImageMarginRevealedGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.Menu.ImageMarginRevealedGradientMiddle = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.Menu.MenuBorder = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
			this.AppPalette.ToolMenuStatus.Menu.MenuItemBorder = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.Menu.MenuItemPressedGradientBegin = System.Drawing.Color.CornflowerBlue;
			this.AppPalette.ToolMenuStatus.Menu.MenuItemPressedGradientEnd = System.Drawing.Color.CornflowerBlue;
			this.AppPalette.ToolMenuStatus.Menu.MenuItemPressedGradientMiddle = System.Drawing.Color.CornflowerBlue;
			this.AppPalette.ToolMenuStatus.Menu.MenuItemSelected = System.Drawing.Color.SteelBlue;
			this.AppPalette.ToolMenuStatus.Menu.MenuItemSelectedGradientBegin = System.Drawing.Color.SteelBlue;
			this.AppPalette.ToolMenuStatus.Menu.MenuItemSelectedGradientEnd = System.Drawing.Color.SteelBlue;
			this.AppPalette.ToolMenuStatus.Menu.MenuItemText = System.Drawing.Color.White;
			this.AppPalette.ToolMenuStatus.MenuStrip.MenuStripFont = new System.Drawing.Font("Segoe UI", 9F);
			this.AppPalette.ToolMenuStatus.MenuStrip.MenuStripGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.MenuStrip.MenuStripGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.MenuStrip.MenuStripText = System.Drawing.Color.White;
			this.AppPalette.ToolMenuStatus.Rafting.RaftingContainerGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.Rafting.RaftingContainerGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.Separator.SeparatorDark = System.Drawing.Color.Silver;
			this.AppPalette.ToolMenuStatus.Separator.SeparatorLight = System.Drawing.Color.Silver;
			this.AppPalette.ToolMenuStatus.ToolStrip.ToolStripBorder = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.ToolStrip.ToolStripContentPanelGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.ToolStrip.ToolStripContentPanelGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.ToolStrip.ToolStripDropDownBackground = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.ToolStrip.ToolStripFont = new System.Drawing.Font("Segoe UI", 9F);
			this.AppPalette.ToolMenuStatus.ToolStrip.ToolStripGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.ToolStrip.ToolStripGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.ToolStrip.ToolStripGradientMiddle = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.ToolStrip.ToolStripPanelGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.ToolStrip.ToolStripPanelGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.AppPalette.ToolMenuStatus.ToolStrip.ToolStripText = System.Drawing.Color.White;
			this.AppPalette.ToolMenuStatus.UseRoundedEdges = ComponentFactory.Krypton.Toolkit.InheritBool.False;
			// 
			// ProgressScreen
			// 
			this.ProgressScreen.AllowCancellation = true;
			this.ProgressScreen.CurrentValue = 0F;
			this.ProgressScreen.Location = new System.Drawing.Point(521, 307);
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
			this.WaitScreen.Location = new System.Drawing.Point(328, 260);
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
			this.RibbonMain.QATUserChange = false;
			this.RibbonMain.RibbonAppButton.AppButtonShowRecentDocs = false;
			this.RibbonMain.RibbonTabs.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab[] {
            this.TabFileSystem,
            this.RibbonTabEditorTools});
			this.RibbonMain.SelectedTab = this.TabFileSystem;
			this.RibbonMain.ShowMinimizeButton = false;
			this.RibbonMain.Size = new System.Drawing.Size(1280, 115);
			this.RibbonMain.StateCommon.RibbonGeneral.ContextTextFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RibbonMain.StateCommon.RibbonGeneral.RibbonShape = ComponentFactory.Krypton.Toolkit.PaletteRibbonShape.Office2010;
			this.RibbonMain.StateCommon.RibbonGeneral.TextFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RibbonMain.TabIndex = 0;
			popupPositionValues1.PlacementMode = ComponentFactory.Krypton.Toolkit.PlacementMode.Bottom;
			this.RibbonMain.ToolTipValues.ToolTipPosition = popupPositionValues1;
			this.RibbonMain.Visible = false;
			this.RibbonMain.AppButtonMenuOpening += new System.ComponentModel.CancelEventHandler(this.RibbonMain_AppButtonMenuOpening);
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
            this.kryptonRibbonGroupTriple4,
            this.kryptonRibbonGroupLines1});
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
			this.ButtonFileSystemRename.ToolTipBody = "Rename the selected file system item.";
			this.ButtonFileSystemRename.Click += new System.EventHandler(this.ButtonFileSystemRename_Click);
			// 
			// kryptonRibbonGroupLines1
			// 
			this.kryptonRibbonGroupLines1.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonExpand,
            this.ButtonCollapse});
			// 
			// ButtonExpand
			// 
			this.ButtonExpand.Enabled = false;
			this.ButtonExpand.ImageLarge = global::Gorgon.Editor.Properties.Resources.expand_48x48;
			this.ButtonExpand.ImageSmall = global::Gorgon.Editor.Properties.Resources.expand_16x16;
			this.ButtonExpand.KeyTip = "P";
			this.ButtonExpand.TextLine1 = "Expand";
			this.ButtonExpand.ToolTipBody = "Expand the currently selected file/directory.";
			this.ButtonExpand.Click += new System.EventHandler(this.ButtonExpand_Click);
			// 
			// ButtonCollapse
			// 
			this.ButtonCollapse.Enabled = false;
			this.ButtonCollapse.ImageLarge = global::Gorgon.Editor.Properties.Resources.collapse_48x48;
			this.ButtonCollapse.ImageSmall = global::Gorgon.Editor.Properties.Resources.collapse_16x16;
			this.ButtonCollapse.KeyTip = "A";
			this.ButtonCollapse.TextLine1 = "Collapse";
			this.ButtonCollapse.ToolTipBody = "Collapse the currently selected file/directory.";
			this.ButtonCollapse.Click += new System.EventHandler(this.ButtonCollapse_Click);
			// 
			// Stage
			// 
			this.Stage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.Stage.CanOpen = true;
			this.Stage.CanSaveAs = true;
			this.Stage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Stage.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Stage.IsStartup = true;
			this.Stage.Location = new System.Drawing.Point(0, 115);
			this.Stage.Name = "Stage";
			this.Stage.Size = new System.Drawing.Size(1280, 685);
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
			this.PanelProject.Location = new System.Drawing.Point(0, 0);
			this.PanelProject.Name = "PanelProject";
			this.PanelProject.Size = new System.Drawing.Size(1280, 800);
			this.PanelProject.TabIndex = 0;
			this.PanelProject.RibbonAdded += new System.EventHandler<Gorgon.Editor.ContentRibbonEventArgs>(this.PanelProject_RibbonAdded);
			this.PanelProject.RibbonRemoved += new System.EventHandler<Gorgon.Editor.ContentRibbonEventArgs>(this.PanelProject_RibbonRemoved);
			this.PanelProject.RenameBegin += new System.EventHandler(this.PanelProject_RenameBegin);
			this.PanelProject.RenameEnd += new System.EventHandler(this.PanelProject_RenameEnd);
			// 
			// PanelWorkSpace
			// 
			this.PanelWorkSpace.Controls.Add(this.PanelProject);
			this.PanelWorkSpace.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PanelWorkSpace.Location = new System.Drawing.Point(0, 0);
			this.PanelWorkSpace.Name = "PanelWorkSpace";
			this.PanelWorkSpace.Size = new System.Drawing.Size(1280, 800);
			this.PanelWorkSpace.TabIndex = 0;
			// 
			// RibbonTabEditorTools
			// 
			this.RibbonTabEditorTools.Text = "Editor Tools";
			this.RibbonTabEditorTools.Visible = false;
			// 
			// FormMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.ClientSize = new System.Drawing.Size(1280, 800);
			this.Controls.Add(this.ProgressScreen);
			this.Controls.Add(this.Stage);
			this.Controls.Add(this.WaitScreen);
			this.Controls.Add(this.RibbonMain);
			this.Controls.Add(this.PanelWorkSpace);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Activated += new System.EventHandler(this.FormMain_Activated);
			((System.ComponentModel.ISupportInitialize)(this.RibbonMain)).EndInit();
			this.PanelWorkSpace.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonManager AppThemeManager;
        private ComponentFactory.Krypton.Toolkit.KryptonPalette AppPalette;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbon RibbonMain;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonTab TabFileSystem;
        private Gorgon.UI.GorgonWaitScreenPanel WaitScreen;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup RibbonGroupFileSystemNew;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup RibbonGroupFileSystemOrganize;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple2;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonFileSystemDelete;
        private Gorgon.UI.GorgonProgressScreenPanel ProgressScreen;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonExpand;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonCollapse;
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
        private EditorProject PanelProject;
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
    }
}

