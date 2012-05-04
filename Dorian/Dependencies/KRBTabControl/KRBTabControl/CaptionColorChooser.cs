using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Windows.Forms;

namespace KRBTabControl
{
    partial class KRBTabControl
    {
        internal partial class CaptionColorChooser : Form
        {
            #region Instance Members

            public KRBTabControl contextInstance;
            public ICaptionRandomizer Randomizer;

            #endregion

            #region Constructor

            public CaptionColorChooser()
            {
                InitializeComponent();
            }

            #endregion

            #region Destructor

            ~CaptionColorChooser()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Helper Methods

            private void Captions_Paint(object sender, PaintEventArgs e)
            {
                Rectangle rct = e.ClipRectangle;

                rct.Inflate(-8, -8);
                rct.Height = 23;
                rct.Width -= 1;

                // Draw Border Lines
                using (Pen captionBorderPen = new Pen(contextInstance.BorderColor))
                    e.Graphics.DrawRectangle(captionBorderPen, rct);

                // Create two new empty image for manipulations. If you use this constructor, you get a new Bitmap object that represents a bitmap in memory with a PixelFormat of Format32bppARGB.
                using (Bitmap overlay = new Bitmap(rct.Width + 1, rct.Height + 1),
                    overlay2 = new Bitmap(rct.Width + 1, rct.Height + 1))
                {
                    // Make an associated Graphics object.
                    using (Graphics gr = Graphics.FromImage(overlay), gr2 = Graphics.FromImage(overlay2))
                    {
                        gr.SmoothingMode = SmoothingMode.HighQuality;

                        // Fill Active Caption.
                        using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, overlay.Width, overlay.Height),
                            contextInstance.GradientCaption.ActiveCaptionColorStart, contextInstance.GradientCaption.ActiveCaptionColorEnd, contextInstance.GradientCaption.CaptionGradientStyle))
                        {
                            Blend bl = new Blend(2);
                            bl.Factors = new float[] { 0.1F, 1.0F };
                            bl.Positions = new float[] { 0.0F, 1.0F };
                            brush.Blend = bl;
                            gr.FillRectangle(brush, 0, 0, overlay.Width, overlay.Height);
                        }

                        gr2.SmoothingMode = SmoothingMode.HighQuality;

                        // Fill Inactive Caption.
                        using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, overlay2.Width, overlay2.Height),
                            contextInstance.GradientCaption.InactiveCaptionColorStart, contextInstance.GradientCaption.InactiveCaptionColorEnd, contextInstance.GradientCaption.CaptionGradientStyle))
                        {
                            Blend bl = new Blend(2);
                            bl.Factors = new float[] { 0.1F, 1.0F };
                            bl.Positions = new float[] { 0.0F, 1.0F };
                            brush.Blend = bl;
                            gr2.FillRectangle(brush, 0, 0, overlay2.Width, overlay2.Height);
                        }
                    }

                    /* Create a new color matrix,
                       The value prgAlpha in row 4, column 4 specifies the alpha value */
                    float[][] jaggedMatrix = new float[][]
                {
                    // Red component   [from 0.0 to 1.0 increase red color component.]
                    new float[]{ (byte)nmrRed.Value / 255f , 0.0f , 0.0f , 0.0f , 0.0f },                  
                    // Green component [from 0.0 to 1.0 increase green color component.]
                    new float[]{ 0.0f , (byte)nmrGreen.Value / 255f , 0.0f , 0.0f , 0.0f },                
                    // Blue component  [from 0.0 to 1.0 increase blue color component.]
                    new float[]{ 0.0f , 0.0f , (byte)nmrBlue.Value / 255f , 0.0f , 0.0f },                 
                    // Alpha component [from 1.0 to 0.0 increase transparency bitmap.]
                    new float[]{ 0.0f , 0.0f , 0.0f , (byte)nmrAlpha.Value / 255f , 0.0f },       
                    // White component [0.0: goes to Original color, 1.0: goes to white for all color component(Red, Green, Blue.)]
                    new float[]{ 0.2f , 0.2f , 0.2f , 0.0f , 1.0f }                                                                                           
                };

                    ColorMatrix colorMatrix = new ColorMatrix(jaggedMatrix);

                    // Create an ImageAttributes object and set its color matrix
                    using (ImageAttributes attributes = new ImageAttributes())
                    {
                        attributes.SetColorMatrix(
                            colorMatrix,
                            ColorMatrixFlag.Default,
                            ColorAdjustType.Bitmap);

                        using (Bitmap closeIcon = new Bitmap(Resources.CaptionClose), dropDownIcon = new Bitmap(Resources.DropDown))
                        {
                            using (ImageAttributes attributes2 = new ImageAttributes())
                            {
                                ColorMap[] map = new ColorMap[2];
                                map[0] = new ColorMap();
                                map[0].OldColor = Color.White;
                                map[0].NewColor = Color.Transparent;
                                map[1] = new ColorMap();
                                map[1].OldColor = Color.Black;
                                map[1].NewColor = contextInstance.CaptionButtons.ActiveCaptionButtonsColor;

                                attributes2.SetRemapTable(map);

                                // Shrink rectangle for drawing caption background.
                                rct.Inflate(-1, -1);
                                rct.Width += 1;
                                rct.Height += 1;
                                e.Graphics.DrawImage(overlay, rct, 1, 1, overlay.Width - 1, overlay.Height - 1, GraphicsUnit.Pixel, attributes);

                                Rectangle closeIconRct = new Rectangle(rct.Right - (3 + closeIcon.Width), rct.Y + 4, closeIcon.Width, closeIcon.Height);
                                Rectangle dropDownIconRct = new Rectangle(closeIconRct.Left - (3 + dropDownIcon.Width), rct.Y + 4, dropDownIcon.Width, dropDownIcon.Height);

                                e.Graphics.DrawImage(closeIcon, closeIconRct, 0, 0, closeIcon.Width, closeIcon.Height, GraphicsUnit.Pixel, attributes2);
                                e.Graphics.DrawImage(dropDownIcon, dropDownIconRct, 0, 0, dropDownIcon.Width, dropDownIcon.Height, GraphicsUnit.Pixel, attributes2);

                                using (Font captionFont = new System.Drawing.Font("Arial", 12, contextInstance.GradientCaption.ActiveCaptionFontStyle, GraphicsUnit.Pixel))
                                using (SolidBrush captionTextBrush = new SolidBrush(contextInstance.GradientCaption.ActiveCaptionTextColor))
                                {
                                    e.Graphics.DrawString("Active Caption", captionFont,
                                        captionTextBrush, new Point(rct.X + 3, rct.Y + 3));
                                }

                                rct.Y = rct.Bottom + 8;
                                rct.X -= 1;
                                rct.Width += 1;

                                // Draw Border Lines
                                using (Pen captionBorderPen = new Pen(contextInstance.BorderColor))
                                    e.Graphics.DrawRectangle(captionBorderPen, rct);

                                rct.Inflate(-1, -1);
                                rct.Width += 1;
                                rct.Height += 1;
                                e.Graphics.DrawImage(overlay2, rct, 1, 1, overlay2.Width - 1, overlay2.Height - 1, GraphicsUnit.Pixel, attributes);

                                map[1].NewColor = contextInstance.CaptionButtons.InactiveCaptionButtonsColor;
                                attributes2.SetRemapTable(map);

                                closeIconRct = new Rectangle(rct.Right - (3 + closeIcon.Width), rct.Y + 3, closeIcon.Width, closeIcon.Height);
                                dropDownIconRct = new Rectangle(closeIconRct.Left - (3 + dropDownIcon.Width), rct.Y + 3, dropDownIcon.Width, dropDownIcon.Height);

                                e.Graphics.DrawImage(closeIcon, closeIconRct, 0, 0, closeIcon.Width, closeIcon.Height, GraphicsUnit.Pixel, attributes2);
                                e.Graphics.DrawImage(dropDownIcon, dropDownIconRct, 0, 0, dropDownIcon.Width, dropDownIcon.Height, GraphicsUnit.Pixel, attributes2);

                                using (Font captionFont = new System.Drawing.Font("Arial", 12, FontStyle.Regular, GraphicsUnit.Pixel))
                                using (SolidBrush captionTextBrush = new SolidBrush(contextInstance.GradientCaption.InactiveCaptionTextColor))
                                {
                                    e.Graphics.DrawString("Inactive Caption", captionFont,
                                        captionTextBrush, new Point(rct.X + 3, rct.Y + 3));
                                }
                            }
                        }
                    }
                }
            }

            private void CaptionColorChooser_Load(object sender, EventArgs e)
            {
                nmrRed.Value = Randomizer.Red;
                nmrGreen.Value = Randomizer.Green;
                nmrBlue.Value = Randomizer.Blue;
                nmrAlpha.Value = Randomizer.Transparency;
            }

            private void numeric_ValueChanged(object sender, EventArgs e)
            {
                Captions.Invalidate();
                Captions.Update();
            }

            private void button1_Click(object sender, EventArgs e)
            {
                Randomizer = new KRBTabControl.RandomizerCaption((byte)nmrRed.Value, (byte)nmrGreen.Value, (byte)nmrBlue.Value, (byte)nmrAlpha.Value,
                    Randomizer.IsRandomizerEnabled, Randomizer.IsTransparencyEnabled);
            }

            #endregion

        }

        [ToolboxItem(false)]
        internal class MyPanel : Panel
        {
            #region Constructor

            public MyPanel()
                : base()
            {
                this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ContainerControl | ControlStyles.UserPaint | ControlStyles.FixedWidth | ControlStyles.FixedHeight, true);
            }

            #endregion

            #region Destructor

            ~MyPanel()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Override Methods

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                using (HatchBrush brush = new HatchBrush(HatchStyle.Percent90, Color.WhiteSmoke, Color.LightSteelBlue))
                    e.Graphics.FillRectangle(brush, e.ClipRectangle);

                using (Pen pen = new Pen(SystemColors.ControlDarkDark))
                {
                    pen.DashStyle = DashStyle.Dot;
                    e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                }
            }

            #endregion
        }
    }
}