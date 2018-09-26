namespace Gorgon.Editor
{
    partial class FormWorkspaceLocator
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormWorkspaceLocator));
            this.AppThemeManager = new ComponentFactory.Krypton.Toolkit.KryptonManager(this.components);
            this.AppPalette = new ComponentFactory.Krypton.Toolkit.KryptonPalette(this.components);
            this.WorkspaceBrowser = new Gorgon.UI.GorgonFolderBrowser();
            this.panel1 = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            this.ButtonCancel = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.ButtonOK = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)(this.panel1)).BeginInit();
            this.panel1.SuspendLayout();
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
            this.AppPalette.LabelStyles.LabelCommon.StateCommon.ShortText.Color1 = System.Drawing.Color.White;
            this.AppPalette.LabelStyles.LabelCommon.StateCommon.ShortText.Color2 = System.Drawing.Color.White;
            this.AppPalette.LabelStyles.LabelCommon.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AppPalette.LabelStyles.LabelCustom1.StateCommon.ShortText.Color1 = System.Drawing.Color.Black;
            this.AppPalette.LabelStyles.LabelCustom1.StateCommon.ShortText.Color2 = System.Drawing.Color.Black;
            this.AppPalette.LabelStyles.LabelCustom1.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.AppPalette.LabelStyles.LabelCustom2.StateCommon.ShortText.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.AppPalette.LabelStyles.LabelCustom2.StateCommon.ShortText.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.AppPalette.LabelStyles.LabelCustom2.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 16F);
            this.AppPalette.LabelStyles.LabelCustom3.StateCommon.ShortText.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.AppPalette.LabelStyles.LabelCustom3.StateCommon.ShortText.Color2 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.AppPalette.LabelStyles.LabelCustom3.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.AppPalette.LabelStyles.LabelNormalControl.StateCommon.Padding = new System.Windows.Forms.Padding(0);
            this.AppPalette.LabelStyles.LabelNormalControl.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.AppPalette.LabelStyles.LabelTitleControl.StateCommon.DrawFocus = ComponentFactory.Krypton.Toolkit.InheritBool.False;
            this.AppPalette.LabelStyles.LabelTitleControl.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 18F);
            this.AppPalette.LabelStyles.LabelTitlePanel.StateCommon.ShortText.Color1 = System.Drawing.Color.Silver;
            this.AppPalette.LabelStyles.LabelTitlePanel.StateCommon.ShortText.Color2 = System.Drawing.Color.Silver;
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
            // WorkspaceBrowser
            // 
            this.WorkspaceBrowser.BackColor = System.Drawing.Color.Transparent;
            this.WorkspaceBrowser.CaptionFont = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WorkspaceBrowser.CdRomImage = global::Gorgon.Editor.Properties.Resources.drive_cdrom_48x48;
            this.WorkspaceBrowser.DirectoryImage = global::Gorgon.Editor.Properties.Resources.folder_48x48;
            this.WorkspaceBrowser.DirectoryListFont = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WorkspaceBrowser.DirectoryNameFont = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WorkspaceBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WorkspaceBrowser.FileImage = global::Gorgon.Editor.Properties.Resources.file_48x48;
            this.WorkspaceBrowser.ForeColor = System.Drawing.Color.White;
            this.WorkspaceBrowser.HardDriveImage = global::Gorgon.Editor.Properties.Resources.drive_48x48;
            this.WorkspaceBrowser.Location = new System.Drawing.Point(0, 0);
            this.WorkspaceBrowser.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.WorkspaceBrowser.Name = "WorkspaceBrowser";
            this.WorkspaceBrowser.NetworkDriveImage = global::Gorgon.Editor.Properties.Resources.drive_network_48x48;
            this.WorkspaceBrowser.RamDriveImage = global::Gorgon.Editor.Properties.Resources.drive_ram_48x48;
            this.WorkspaceBrowser.RemovableDriveImage = global::Gorgon.Editor.Properties.Resources.drive_remove_48x48;
            this.WorkspaceBrowser.Size = new System.Drawing.Size(624, 401);
            this.WorkspaceBrowser.TabIndex = 3;
            this.WorkspaceBrowser.FolderSelected += new System.EventHandler<Gorgon.UI.FolderSelectedArgs>(this.WorkspaceBrowser_FolderSelected);
            this.WorkspaceBrowser.FolderEntered += new System.EventHandler<Gorgon.UI.FolderSelectedArgs>(this.WorkspaceBrowser_FolderEntered);
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.ButtonCancel);
            this.panel1.Controls.Add(this.ButtonOK);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 401);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(624, 40);
            this.panel1.TabIndex = 1;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonCancel.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Custom2;
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(514, 5);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(98, 32);
            this.ButtonCancel.TabIndex = 7;
            this.ButtonCancel.Values.Text = "&Cancel";
            // 
            // ButtonOK
            // 
            this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonOK.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Custom2;
            this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOK.Enabled = false;
            this.ButtonOK.Location = new System.Drawing.Point(410, 5);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(98, 32);
            this.ButtonOK.TabIndex = 6;
            this.ButtonOK.Values.Text = "&OK";
            // 
            // FormWorkspaceLocator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.WorkspaceBrowser);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormWorkspaceLocator";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select a workspace directory...";
            ((System.ComponentModel.ISupportInitialize)(this.panel1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ComponentFactory.Krypton.Toolkit.KryptonManager AppThemeManager;
        private ComponentFactory.Krypton.Toolkit.KryptonPalette AppPalette;
        private Gorgon.UI.GorgonFolderBrowser WorkspaceBrowser;
        private ComponentFactory.Krypton.Toolkit.KryptonPanel panel1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton ButtonCancel;
        private ComponentFactory.Krypton.Toolkit.KryptonButton ButtonOK;
    }
}