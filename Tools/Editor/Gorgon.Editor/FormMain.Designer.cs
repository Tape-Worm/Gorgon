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
                StageLauncher.StageNewProject.ProjectCreated -= StageNewProject_ProjectCreated;

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
            this.AppThemeManager = new ComponentFactory.Krypton.Toolkit.KryptonManager(this.components);
            this.AppPalette = new ComponentFactory.Krypton.Toolkit.KryptonPalette(this.components);
            this.PanelWorkSpace = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            this.PanelProject = new Gorgon.Editor.Views.EditorProject();
            this.ProgressScreen = new Gorgon.UI.GorgonProgressScreenPanel();
            this.WaitScreen = new Gorgon.UI.GorgonWaitScreenPanel();
            this.RibbonMain = new ComponentFactory.Krypton.Ribbon.KryptonRibbon();
            this.TabFileSystem = new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab();
            this.RibbonGroupFileSystemNew = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.kryptonRibbonGroupTriple1 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.ButtonFileSystemNewDirectory = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.RibbonGroupFileSystemOrganize = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.kryptonRibbonGroupTriple2 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.ButtonFileSystemDelete = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.kryptonRibbonGroupLines1 = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines();
            this.ButtonFileSystemRename = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.StageLauncher = new Gorgon.Editor.Views.StageLaunch();
            ((System.ComponentModel.ISupportInitialize)(this.PanelWorkSpace)).BeginInit();
            this.PanelWorkSpace.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RibbonMain)).BeginInit();
            this.SuspendLayout();
            // 
            // AppThemeManager
            // 
            this.AppThemeManager.GlobalPalette = this.AppPalette;
            this.AppThemeManager.GlobalPaletteMode = ComponentFactory.Krypton.Toolkit.PaletteModeManager.Custom;
            // 
            // AppPalette
            // 
            this.AppPalette.BasePaletteMode = ComponentFactory.Krypton.Toolkit.PaletteMode.Office2010Black;
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
            this.AppPalette.Common.StateCommon.Content.LongText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.AppPalette.Common.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.White;
            this.AppPalette.Common.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.White;
            this.AppPalette.Common.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
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
            this.AppPalette.HeaderStyles.HeaderForm.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.AppPalette.HeaderStyles.HeaderForm.StateCommon.Back.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.AppPalette.HeaderStyles.HeaderForm.StateCommon.ButtonEdgeInset = 8;
            this.AppPalette.HeaderStyles.HeaderForm.StateCommon.ButtonPadding = new System.Windows.Forms.Padding(3);
            this.AppPalette.HeaderStyles.HeaderForm.StateCommon.Content.Padding = new System.Windows.Forms.Padding(3, -1, -1, -1);
            this.AppPalette.HeaderStyles.HeaderForm.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.AppPalette.HeaderStyles.HeaderForm.StateDisabled.Content.ShortText.Color1 = System.Drawing.Color.DimGray;
            this.AppPalette.HeaderStyles.HeaderForm.StateDisabled.Content.ShortText.Color2 = System.Drawing.Color.DimGray;
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
            this.AppPalette.LabelStyles.LabelCustom1.StateCommon.ShortText.Color1 = System.Drawing.Color.Black;
            this.AppPalette.LabelStyles.LabelCustom1.StateCommon.ShortText.Color2 = System.Drawing.Color.Black;
            this.AppPalette.LabelStyles.LabelCustom1.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.AppPalette.LabelStyles.LabelCustom2.StateCommon.ShortText.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.AppPalette.LabelStyles.LabelCustom2.StateCommon.ShortText.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.AppPalette.LabelStyles.LabelCustom2.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 16F);
            this.AppPalette.LabelStyles.LabelCustom3.StateCommon.ShortText.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.AppPalette.LabelStyles.LabelCustom3.StateCommon.ShortText.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.AppPalette.LabelStyles.LabelCustom3.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.AppPalette.LabelStyles.LabelNormalControl.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.AppPalette.LabelStyles.LabelTitleControl.StateCommon.DrawFocus = ComponentFactory.Krypton.Toolkit.InheritBool.False;
            this.AppPalette.LabelStyles.LabelTitleControl.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 18F);
            this.AppPalette.PanelStyles.PanelClient.StateCommon.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.AppPalette.PanelStyles.PanelClient.StateCommon.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.AppPalette.PanelStyles.PanelClient.StateCommon.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.True;
            this.AppPalette.PanelStyles.PanelClient.StateCommon.GraphicsHint = ComponentFactory.Krypton.Toolkit.PaletteGraphicsHint.None;
            this.AppPalette.PanelStyles.PanelCommon.StateCommon.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.False;
            this.AppPalette.PanelStyles.PanelCustom1.StateCommon.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.AppPalette.PanelStyles.PanelCustom1.StateCommon.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.AppPalette.PanelStyles.PanelCustom1.StateCommon.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.True;
            this.AppPalette.Ribbon.RibbonGeneral.GroupSeparatorDark = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.AppPalette.Ribbon.RibbonGeneral.GroupSeparatorLight = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
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
            // 
            // PanelWorkSpace
            // 
            this.PanelWorkSpace.Controls.Add(this.PanelProject);
            this.PanelWorkSpace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelWorkSpace.Location = new System.Drawing.Point(0, 0);
            this.PanelWorkSpace.Name = "PanelWorkSpace";
            this.PanelWorkSpace.Size = new System.Drawing.Size(1264, 761);
            this.PanelWorkSpace.TabIndex = 0;
            // 
            // PanelProject
            // 
            this.PanelProject.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.PanelProject.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelProject.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.PanelProject.Location = new System.Drawing.Point(0, 0);
            this.PanelProject.Name = "PanelProject";
            this.PanelProject.Size = new System.Drawing.Size(1264, 761);
            this.PanelProject.TabIndex = 0;
            this.PanelProject.RenameBegin += new System.EventHandler(this.PanelProject_RenameBegin);
            this.PanelProject.RenameEnd += new System.EventHandler(this.PanelProject_RenameEnd);
            // 
            // ProgressScreen
            // 
            this.ProgressScreen.AllowCancellation = true;
            this.ProgressScreen.CurrentValue = 0F;
            this.ProgressScreen.Location = new System.Drawing.Point(0, 0);
            this.ProgressScreen.MeterStyle = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.ProgressScreen.Name = "ProgressScreen";
            this.ProgressScreen.ProgressMessage = "Something";
            this.ProgressScreen.ProgressMessageFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ProgressScreen.ProgressTitle = "Title";
            this.ProgressScreen.Size = new System.Drawing.Size(1019, 424);
            this.ProgressScreen.TabIndex = 1;
            this.ProgressScreen.TabStop = true;
            // 
            // WaitScreen
            // 
            this.WaitScreen.Location = new System.Drawing.Point(971, 604);
            this.WaitScreen.Name = "WaitScreen";
            this.WaitScreen.Size = new System.Drawing.Size(281, 145);
            this.WaitScreen.TabIndex = 0;
            this.WaitScreen.TabStop = true;
            this.WaitScreen.Visible = false;
            this.WaitScreen.WaitMessage = "Loading...";
            this.WaitScreen.WaitTitle = "Please wait";
            // 
            // RibbonMain
            // 
            this.RibbonMain.AllowFormIntegrate = false;
            this.RibbonMain.AllowMinimizedChange = false;
            this.RibbonMain.InDesignHelperMode = true;
            this.RibbonMain.Name = "RibbonMain";
            this.RibbonMain.QATLocation = ComponentFactory.Krypton.Ribbon.QATLocation.Hidden;
            this.RibbonMain.QATUserChange = false;
            this.RibbonMain.RibbonAppButton.AppButtonShowRecentDocs = false;
            this.RibbonMain.RibbonTabs.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab[] {
            this.TabFileSystem});
            this.RibbonMain.SelectedContext = null;
            this.RibbonMain.SelectedTab = this.TabFileSystem;
            this.RibbonMain.ShowMinimizeButton = false;
            this.RibbonMain.Size = new System.Drawing.Size(1264, 115);
            this.RibbonMain.TabIndex = 0;
            this.RibbonMain.Visible = false;
            this.RibbonMain.AppButtonMenuOpening += new System.ComponentModel.CancelEventHandler(this.RibbonMain_AppButtonMenuOpening);
            // 
            // TabFileSystem
            // 
            this.TabFileSystem.Groups.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup[] {
            this.RibbonGroupFileSystemNew,
            this.RibbonGroupFileSystemOrganize});
            this.TabFileSystem.KeyTip = "F";
            this.TabFileSystem.Text = "File System";
            // 
            // RibbonGroupFileSystemNew
            // 
            this.RibbonGroupFileSystemNew.AllowCollapsed = false;
            this.RibbonGroupFileSystemNew.DialogBoxLauncher = false;
            this.RibbonGroupFileSystemNew.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
            this.kryptonRibbonGroupTriple1});
            this.RibbonGroupFileSystemNew.TextLine1 = "New";
            // 
            // kryptonRibbonGroupTriple1
            // 
            this.kryptonRibbonGroupTriple1.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonFileSystemNewDirectory});
            // 
            // ButtonFileSystemNewDirectory
            // 
            this.ButtonFileSystemNewDirectory.Enabled = false;
            this.ButtonFileSystemNewDirectory.ImageLarge = global::Gorgon.Editor.Properties.Resources.add_directory_48x48;
            this.ButtonFileSystemNewDirectory.ImageSmall = global::Gorgon.Editor.Properties.Resources.add_directory_16x16;
            this.ButtonFileSystemNewDirectory.TextLine1 = "Directory";
            this.ButtonFileSystemNewDirectory.Click += new System.EventHandler(this.ButtonFileSystemNewDirectory_Click);
            // 
            // RibbonGroupFileSystemOrganize
            // 
            this.RibbonGroupFileSystemOrganize.AllowCollapsed = false;
            this.RibbonGroupFileSystemOrganize.DialogBoxLauncher = false;
            this.RibbonGroupFileSystemOrganize.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
            this.kryptonRibbonGroupTriple2,
            this.kryptonRibbonGroupLines1});
            this.RibbonGroupFileSystemOrganize.KeyTipGroup = "O";
            this.RibbonGroupFileSystemOrganize.TextLine1 = "Organize";
            // 
            // kryptonRibbonGroupTriple2
            // 
            this.kryptonRibbonGroupTriple2.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonFileSystemDelete});
            // 
            // ButtonFileSystemDelete
            // 
            this.ButtonFileSystemDelete.Enabled = false;
            this.ButtonFileSystemDelete.ImageLarge = global::Gorgon.Editor.Properties.Resources.icons8_48x48;
            this.ButtonFileSystemDelete.ImageSmall = global::Gorgon.Editor.Properties.Resources.icons8_16x16;
            this.ButtonFileSystemDelete.KeyTip = "D";
            this.ButtonFileSystemDelete.TextLine1 = "Delete";
            this.ButtonFileSystemDelete.Click += new System.EventHandler(this.ButtonFileSystemDelete_Click);
            // 
            // kryptonRibbonGroupLines1
            // 
            this.kryptonRibbonGroupLines1.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
            this.ButtonFileSystemRename});
            // 
            // ButtonFileSystemRename
            // 
            this.ButtonFileSystemRename.Enabled = false;
            this.ButtonFileSystemRename.ImageLarge = global::Gorgon.Editor.Properties.Resources.rename_16x16;
            this.ButtonFileSystemRename.ImageSmall = global::Gorgon.Editor.Properties.Resources.rename_16x16;
            this.ButtonFileSystemRename.KeyTip = "R";
            this.ButtonFileSystemRename.TextLine1 = "Rename";
            this.ButtonFileSystemRename.ToolTipBody = "Rename the selected file system item.";
            this.ButtonFileSystemRename.Click += new System.EventHandler(this.ButtonFileSystemRename_Click);
            // 
            // StageLauncher
            // 
            this.StageLauncher.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.StageLauncher.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StageLauncher.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.StageLauncher.Location = new System.Drawing.Point(0, 115);
            this.StageLauncher.Name = "StageLauncher";
            this.StageLauncher.ShowBackButton = false;
            this.StageLauncher.Size = new System.Drawing.Size(1264, 646);
            this.StageLauncher.TabIndex = 0;
            this.StageLauncher.BackClicked += new System.EventHandler(this.StageLauncher_BackClicked);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(1264, 761);
            this.Controls.Add(this.ProgressScreen);
            this.Controls.Add(this.StageLauncher);
            this.Controls.Add(this.WaitScreen);
            this.Controls.Add(this.RibbonMain);
            this.Controls.Add(this.PanelWorkSpace);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Gorgon Editor";
            ((System.ComponentModel.ISupportInitialize)(this.PanelWorkSpace)).EndInit();
            this.PanelWorkSpace.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.RibbonMain)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonManager AppThemeManager;
        private ComponentFactory.Krypton.Toolkit.KryptonPalette AppPalette;
        private ComponentFactory.Krypton.Toolkit.KryptonPanel PanelWorkSpace;
        private Views.StageLaunch StageLauncher;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbon RibbonMain;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonTab TabFileSystem;
        private Gorgon.UI.GorgonWaitScreenPanel WaitScreen;
        private EditorProject PanelProject;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup RibbonGroupFileSystemNew;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonFileSystemNewDirectory;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup RibbonGroupFileSystemOrganize;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupLines kryptonRibbonGroupLines1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonFileSystemRename;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple kryptonRibbonGroupTriple2;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton ButtonFileSystemDelete;
        private Gorgon.UI.GorgonProgressScreenPanel ProgressScreen;
    }
}

