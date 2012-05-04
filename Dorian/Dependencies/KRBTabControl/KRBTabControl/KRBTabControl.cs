using System;
using System.Text;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections.Generic;
using KRBTabControl.Win32;

namespace KRBTabControl
{
    [Designer(typeof(KRBTabControlDesigner))]
    public partial class KRBTabControl : TabControl
    {
        #region Events

        /// <summary>
        /// Event raised when the value of the IsDrawHeader property is changed.
        /// </summary>
        [Description("Event raised when the value of the IsDrawHeader property is changed")]
        public event EventHandler DrawHeaderChanged;

        /// <summary>
        /// Event raised when the value of the IsCaptionVisible property is changed.
        /// </summary>
        [Description("Event raised when the value of the IsCaptionVisible property is changed")]
        public event EventHandler CaptionVisibleChanged;

        /// <summary>
        /// Event raised when the value of the HeaderVisibility(StretchToParent) property is changed.
        /// </summary>
        [Description("Event raised when the value of the HeaderVisibility(StretchToParent) property is changed")]
        public event EventHandler StretchToParentChanged;

        /// <summary>
        /// Occurs when a tab page is being closed.
        /// </summary>
        [Description("Occurs when a tab page is being closed")]
        public event EventHandler<SelectedIndexChangingEventArgs> TabPageClosing;
        
        /// <summary>
        /// Occurs when a tab page is being selected.
        /// </summary>
        [Description("Occurs when a tab page is being selected")]
        public event EventHandler<SelectedIndexChangingEventArgs> SelectedIndexChanging;

        /// <summary>
        /// Occurs when a user clicks the drop-down icon on the caption if it's visible.
        /// </summary>
        [Description("Occurs when a user clicks the drop-down icon on the caption if it's visible")]
        public event EventHandler<ContextMenuShownEventArgs> ContextMenuShown;
        
        #endregion

        #region Enums

        public enum UpDown32Style
        {
            BlackGlass,
            OfficeBlue,
            OfficeOlive,
            OfficeSilver,
            Default,
            KRBBlue
        };

        public enum TabAlignments
        {
            Bottom,
            Top
        };

        public enum TabStyle
        {
            KRBStyle,
            OfficeXP,
            VS2010
        };

        public enum TabHeaderStyle
        {
            Solid,
            Hatch,
            Texture
        };

        public enum ControlBorderStyle
        {
            Solid,
            Dashed,
            Dotted
        };

        private enum ButtonState
        {
            Normal,
            Hover,
            Pressed
        };

        /// <summary>
        /// Specifies the four styles of raised or inset rectangles. Lines can have either
        /// Ridge or Groove appearance only.
        /// </summary>
        private enum ThreeDStyle
        {
            /// <summary>
            /// Inset, groove appearance.
            /// </summary>
            Groove,
            /// <summary>
            /// Inset appearance. Applies only to rectangles, not to lines.
            /// </summary>
            Inset,
            /// <summary>
            /// Raised appearance. Applies only to rectangles, not to lines.
            /// </summary>
            Raised,
            /// <summary>
            /// Raised, ridge appearance.
            /// </summary>
            Ridge,
        };

        #endregion

        #region Symbolic Constants

        private const int CAPTIONHEIGHT = 20;
        private static readonly int _value = SystemInformation.Border3DSize.Width;      // Sistemde kullanılan çerçeve kalınlığı

        #endregion

        #region Static Members Of The Class

        // [0:UpArrow, 1:DownArrow]
        private static ArrowWindow[] dragDropArrowArray = new ArrowWindow[] { null, null };
        private static Cursor _myCursor = null;

        #endregion

        #region Instance Members

        // IsDrawHeader, IsCaptionVisible, IsDrawEdgeBorder, IsUserInteraction, IsDocumentTabStyle, IsDrawTabSeparator, HeaderVisibility
        private bool[] conditionBooleanArray = { true, true, false, true, false, false, false };
        // BackgroundColor, BorderColor, TabBorderColor, TabPageCloseIconColor
        private Color[] colorArray = { SystemColors.Info, Color.Silver, Color.Silver, Color.Black };
        // TabPageCloseIconRectangle, CaptionDropDownRectangle, CaptionCloseRectangle
        private Rectangle[] conditionRectangleArray = new Rectangle[3];
        // TabPageCloseButtonState, CaptionDropDownButtonState, CaptionCloseButtonState
        private ButtonState[] conditionButtonStateArray = new ButtonState[] { ButtonState.Normal, ButtonState.Normal, ButtonState.Normal };

        private GradientTab _tabGradient;
        private Hatcher _hatcher;
        private CaptionGradient _gradientCaption;
        private ButtonsCaption _captionButtons;
        private RandomizerCaption _captionRandomizer;
        private TabPageExPool _tabPageExPool;
        private TabStyle _tabStyles = TabStyle.KRBStyle;                    //Initializer
        private Scroller _myScroller = null;                                //Initializer
        private UpDown32 _upDown32 = null;                                  //Initializer
        private int _tabHOffset = 0;                                        //Initializer
        private Image _backgroundImage = null;                              //Initializer
        private TabAlignments _alignments = TabAlignments.Top;              //Initializer
        private TabHeaderStyle _headerStyle = TabHeaderStyle.Solid;         //Initializer
        private UpDown32Style _upDownStyle = UpDown32Style.Default;         //Initializer
        private ControlBorderStyle _borderStyle = ControlBorderStyle.Solid; //Initializer

        #endregion

        #region Constructor

        public KRBTabControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint |
                 ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw |
                 ControlStyles.UserMouse, true);

            this.Size = new Size()  //Object Initializer
            {
                Width = 300,
                Height = 200
            };

            this.ItemSize = new Size()
            {
                Width = 0,
                Height = 26
            };

            _tabGradient = new GradientTab(Color.White, Color.Gainsboro, LinearGradientMode.Horizontal, Color.Black, Color.Black, FontStyle.Regular);    //Instantiate
            _tabGradient.GradientChanged += new EventHandler(CONTROL_INVALIDATE_UPDATE);

            _hatcher = new Hatcher(Color.White, Color.Gainsboro, HatchStyle.DashedVertical);
            _hatcher.HatchChanged += new EventHandler(CONTROL_INVALIDATE_UPDATE);

            _gradientCaption = new CaptionGradient();
            _gradientCaption.GradientChanged += new EventHandler(CONTROL_INVALIDATE_UPDATE);

            _captionButtons = new ButtonsCaption();
            _captionButtons.ButtonsColorChanged += new EventHandler(CONTROL_INVALIDATE_UPDATE);

            _captionRandomizer = new RandomizerCaption();
            _captionRandomizer.CaptionRandomizerChanged += new EventHandler(CONTROL_INVALIDATE_UPDATE);

            this.AllowDrop = true;  // For drag and drop tab pages.You can change this from control's property.
        }

        #endregion

        #region Destructor

        ~KRBTabControl()
        {
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Property

        /// <summary>
        /// Gets or Sets the tabPage style.
        /// </summary>
        [Description("Gets or Sets the tabPage style")]
        [DefaultValue(typeof(TabStyle), "KRBStyle")]
        [Browsable(true)]
        public TabStyle TabStyles
        {
            get { return _tabStyles; }
            set
            {
                if (!value.Equals(_tabStyles))
                {
                    _tabStyles = value;
                    this.UpdateStyles();
                }
            }
        }

        /// <summary>
        /// Gets or Sets the border color of the control.
        /// </summary>
        [Description("Gets or Sets the border color of the control")]
        [DefaultValue(typeof(Color), "Silver")]
        [Browsable(true)]
        public Color BorderColor
        {
            get { return colorArray[1]; }
            set
            {
                if (!value.Equals(colorArray[1]))
                {
                    colorArray[1] = value;

                    Invalidate();
                    Update();
                }
            }
        }

        /// <summary>
        /// Gets or Sets the border color of the tab page area.
        /// </summary>
        [Description("Gets or Sets the border color of the tab page area")]
        [DefaultValue(typeof(Color), "Silver")]
        [Browsable(true)]
        public Color TabBorderColor
        {
            get { return colorArray[2]; }
            set
            {
                if (!value.Equals(colorArray[2]))
                {
                    colorArray[2] = value;

                    Invalidate();
                    Update();
                }
            }
        }

        /// <summary>
        /// Gets or Sets the icon's color, selected tab page of the tab control.
        /// </summary>
        [Description("Gets or Sets the icon's color, selected tab page of the tab control")]
        [DefaultValue(typeof(Color), "Black")]
        [Browsable(true)]
        public Color TabPageCloseIconColor
        {
            get { return colorArray[3]; }
            set 
            {
                if (!value.Equals(colorArray[3]))
                {
                    colorArray[3] = value;

                    Invalidate();
                    Update();
                }
            }
        }

        /// <summary>
        /// Gets or Sets the distance, in pixels, between the left edge of the first tab page and the left edge of its container's client area, the value must be in the range of -2 to 5 pixel size.
        /// </summary>
        [Description("Gets or Sets the distance, in pixels, between the left edge of the first tab page and the left edge of its container's client area, the value must be in the range of -2 to 5 pixel size")]
        [DefaultValue(0)]
        [Browsable(true)]
        public int TabHOffset
        {
            get { return _tabHOffset; }
            set
            {
                if (!value.Equals(_tabHOffset))
                {
                    if (value < -2)
                        value = -2;
                    else if (value > 5)
                        value = 5;

                    _tabHOffset = value;

                    Invalidate();
                    Update();
                }
            }
        }

        /// <summary>
        /// Gets or Sets the background style of the tab page header.
        /// </summary>
        [Description("Gets or Sets the background style of the tab page header")]
        [RefreshProperties(System.ComponentModel.RefreshProperties.All)]
        [DefaultValue(typeof(TabHeaderStyle), "Solid")]
        [Browsable(true)]
        public TabHeaderStyle HeaderStyle
        {
            get { return _headerStyle; }
            set
            {
                if (!value.Equals(_headerStyle))
                {
                    _headerStyle = value;

                    if (conditionBooleanArray[0])
                    {
                        Invalidate();
                        Update();
                    }
                }
            }
        }

        /// <summary>
        /// You can determine a new border style for your tab control.
        /// </summary>
        [Description("You can determine a new border style for your tab control")]
        [DefaultValue(typeof(ControlBorderStyle), "Solid")]
        [Browsable(true)]
        public ControlBorderStyle BorderStyle
        {
            get { return _borderStyle; }
            set 
            {
                if (!value.Equals(_borderStyle))
                {
                    _borderStyle = value;
                    
                    Invalidate();
                    Update();
                }
            }
        }

        /// <summary>
        /// Tab Alingment.
        /// </summary>
        [Description("Tab Alingment")]
        [DefaultValue(typeof(TabAlignments), "Top")]
        [Browsable(true)]
        public TabAlignments Alignments
        {
            get { return _alignments; }
            set
            {
                if (!value.Equals(_alignments))
                {
                    _alignments = value;

                    if (Enum.GetName(typeof(TabAlignments), _alignments) == "Top")
                        base.Alignment = TabAlignment.Top;
                    else
                        base.Alignment = TabAlignment.Bottom;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the gradient colors of the selected tab page item.
        /// </summary>
        [Description("Gets or Sets the gradient colors of the selected tab page item")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Browsable(true)]
        public GradientTab TabGradient
        {
            get { return _tabGradient; }
            set
            {
                try
                {
                    if (!value.Equals(_tabGradient))
                    {
                        _tabGradient.GradientChanged -= CONTROL_INVALIDATE_UPDATE;
                        _tabGradient = value;
                        _tabGradient.GradientChanged += new EventHandler(CONTROL_INVALIDATE_UPDATE);

                        Invalidate();
                        Update();
                    }
                }
                catch (NullReferenceException)
                {
                    MessageBox.Show("Value cannot be null!, please enter a valid value.");
                }
            }
        }

        ///// <summary>
        ///// Gets or Sets the background hatch style and its sub properties of the tab page header.
        ///// </summary>
        [Description("Gets or Sets the background hatch style and its sub properties of the tab page header")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Browsable(true)]
        public Hatcher BackgroundHatcher
        {
            get { return _hatcher; }
            set
            {
                try
                {
                    if (!value.Equals(_hatcher))
                    {
                        _hatcher.HatchChanged -= CONTROL_INVALIDATE_UPDATE;
                        _hatcher = value;
                        _hatcher.HatchChanged += new EventHandler(CONTROL_INVALIDATE_UPDATE);

                        Invalidate();
                        Update();
                    }
                }
                catch (NullReferenceException)
                {
                    MessageBox.Show("Value cannot be null!, please enter a valid value.");
                }
            }
        }

        /// <summary>
        /// You can determine a new caption gradient style for your tab control.
        /// </summary>
        [Description("You can determine a new caption gradient style for your tab control")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Browsable(true)]
        public CaptionGradient GradientCaption
        {
            get { return _gradientCaption; }
            set
            {
                try
                {
                    if (!value.Equals(_gradientCaption))
                    {
                        _gradientCaption.GradientChanged -= CONTROL_INVALIDATE_UPDATE;
                        _gradientCaption = value;
                        _gradientCaption.GradientChanged += new EventHandler(CONTROL_INVALIDATE_UPDATE);

                        Invalidate();
                        Update();
                    }
                }
                catch (NullReferenceException)
                {
                    MessageBox.Show("Value cannot be null!, please enter a valid value.");
                }
            }
        }

        /// <summary>
        /// You can change your active or inactive caption buttons's color.
        /// </summary>
        [Description("You can change your active or inactive caption buttons's color")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Browsable(true)]
        public ButtonsCaption CaptionButtons
        {
            get { return _captionButtons; }
            set
            {
                try
                {
                    if (!value.Equals(_captionButtons))
                    {
                        _captionButtons.ButtonsColorChanged -= CONTROL_INVALIDATE_UPDATE;
                        _captionButtons = value;
                        _captionButtons.ButtonsColorChanged += new EventHandler(CONTROL_INVALIDATE_UPDATE);

                        Invalidate();
                        Update();
                    }
                }
                catch (NullReferenceException)
                {
                    MessageBox.Show("Value cannot be null!, please enter a valid value.");
                }
            }
        }

        /// <summary>
        /// You can change your active and inactive caption color components (Red, Green, Blue, Alpha).
        /// </summary>
        [Description("You can change your active and inactive caption color components (Red, Green, Blue, Alpha)")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Browsable(true)]
        public RandomizerCaption CaptionRandomizer
        {
            get { return _captionRandomizer; }
            set
            {
                try
                {
                    if (!value.Equals(_captionRandomizer))
                    {
                        _captionRandomizer.CaptionRandomizerChanged -= CONTROL_INVALIDATE_UPDATE;
                        _captionRandomizer = value;
                        _captionRandomizer.CaptionRandomizerChanged += new EventHandler(CONTROL_INVALIDATE_UPDATE);

                        Invalidate();
                        Update();
                    }
                }
                catch (NullReferenceException)
                {
                    MessageBox.Show("Value cannot be null!, please enter a valid value.");
                }
            }
        }

        /// <summary>
        /// Gets or Sets the background color of the tab page header.
        /// </summary>
        [Description("Gets or Sets the background color of the tab page header")]
        [DefaultValue(typeof(Color), "Info")]
        [Browsable(true)]
        public Color BackgroundColor
        {
            get { return colorArray[0]; }
            set
            {
                if (!value.Equals(colorArray[0]))
                {
                    colorArray[0] = value;

                    if (_headerStyle == TabHeaderStyle.Solid)
                    {
                        Invalidate();
                        Update();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or Sets the scroller(up/down) style of the tab control.
        /// </summary>
        [Description("Gets or Sets the scroller(up/down) style of the tab control")]
        [DefaultValue(typeof(UpDown32Style), "OfficeSilver")]
        [Browsable(true)]
        public UpDown32Style UpDownStyle
        {
            get { return _upDownStyle; }
            set
            {
                if (!value.Equals(_upDownStyle))
                {
                    _upDownStyle = value;

                    if (_upDown32 != null)
                    {
                        if (_myScroller != null)
                        {
                            _myScroller.ScrollLeft -= new EventHandler(ScrollLeft);
                            _myScroller.ScrollRight -= new EventHandler(ScrollRight);
                            _myScroller.Dispose();
                            _myScroller = null;
                        }

                        _myScroller = new Scroller(_upDownStyle);
                        _myScroller.ScrollLeft += new EventHandler(ScrollLeft);
                        _myScroller.ScrollRight += new EventHandler(ScrollRight);

                        IntPtr parentHwnd = User32.GetParent(_myScroller.Handle);

                        if (parentHwnd != this.Handle)
                            User32.SetParent(_myScroller.Handle, this.Handle);

                        this.OnResize(EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the active tab is stretched to its parent container or not.
        /// </summary>
        [Description("Determines whether the active tab is stretched to its parent container or not")]
        [RefreshProperties(System.ComponentModel.RefreshProperties.All)]
        [DefaultValue(false)]
        [Browsable(true), DisplayName("StretchToParent")]
        public bool HeaderVisibility
        {
            get { return conditionBooleanArray[6]; }
            set
            {
                if (!value.Equals(conditionBooleanArray[6]))
                {
                    conditionBooleanArray[6] = value;

                    if (value)
                        this.Multiline = true;
                    else
                        this.Multiline = false;

                    this.UpdateStyles();

                    // Fire a Notification Event.
                    OnStretchToParentChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Determines whether the tab header background is draw or not.
        /// </summary>
        [Description("Determines whether the tab header background is draw or not")]
        [RefreshProperties(System.ComponentModel.RefreshProperties.All)]
        [DefaultValue(true)]
        [Browsable(true)]
        public bool IsDrawHeader
        {
            get { return conditionBooleanArray[0]; }
            set
            {
                if (!value.Equals(conditionBooleanArray[0]))
                {
                    conditionBooleanArray[0] = value;

                    Invalidate();
                    Update();

                    // Fire a Notification Event.
                    OnDrawHeaderChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Determines whether the tab control caption is visible or not.
        /// </summary>
        [Description("Determines whether the tab control caption is visible or not")]
        [RefreshProperties(System.ComponentModel.RefreshProperties.All)]
        [DefaultValue(true)]
        [Browsable(true)]
        public bool IsCaptionVisible
        {
            get { return conditionBooleanArray[1]; }
            set
            {
                if (!value.Equals(conditionBooleanArray[1]))
                {
                    conditionBooleanArray[1] = value;
                    this.UpdateStyles();

                    // Fire a Notification Event.
                    OnCaptionVisibleChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Determines whether the tab control's edge border is draw or not, you must set the IsCaptionVisible's value to false for this change to take effect.
        /// </summary>
        [Description("Determines whether the tab control's edge border is draw or not, you must set the IsCaptionVisible's value to false for this change to take effect")]
        [DefaultValue(false)]
        [Browsable(true)]
        public bool IsDrawEdgeBorder
        {
            get { return conditionBooleanArray[2]; }
            set
            {
                if (!value.Equals(conditionBooleanArray[2]))
                {
                    conditionBooleanArray[2] = value;
                    this.UpdateStyles();
                }
            }
        }

        /// <summary>
        /// Provides keyboard support to the user for tab control operations[Ex:Insert, Delete, F1 Keys, Keyboard navigation].
        /// </summary>
        [Description("Provides keyboard support to the user for tab control operations[Ex:Insert, Delete, F1 Keys, Keyboard navigation]")]
        [DefaultValue(true)]
        [Browsable(true)]
        public bool IsUserInteraction
        {
            get { return conditionBooleanArray[3]; }
            set 
            {
                if (!value.Equals(conditionBooleanArray[3]))
                    conditionBooleanArray[3] = value;
            }
        }

        /// <summary>
        /// Determines whether the tab separator line is visible or not between the tab pages.
        /// </summary>
        [Description("Determines whether the tab separator line is visible or not between the tab pages")]
        [DefaultValue(false)]
        [Browsable(true)]
        public bool IsDrawTabSeparator
        {
            get { return conditionBooleanArray[5]; }
            set
            {
                if (!value.Equals(conditionBooleanArray[5]))
                {
                    conditionBooleanArray[5] = value;

                    Invalidate();
                    Update();
                }
            }
        }

        /// <summary>
        /// Determines whether the tab control like as document tab style or not.
        /// </summary>
        [Description("Determines whether the tab control like as document tab style or not")]
        [DefaultValue(false)]
        [Browsable(true)]
        public bool IsDocumentTabStyle
        {
            get { return conditionBooleanArray[4]; }
            set
            {
                if (!value.Equals(conditionBooleanArray[4]))
                {
                    conditionBooleanArray[4] = value;

                    if (this.TabCount == 1)
                        this.UpdateStyles();
                }
            }
        }

        [Browsable(true)]
        public new Image BackgroundImage
        {
            get
            {
                return _backgroundImage;
            }
            set
            {
                if (value != null)
                {
                    if (!value.Equals(_backgroundImage))
                    {
                        _backgroundImage = value;

                        if (_headerStyle == TabHeaderStyle.Texture)
                        {
                            Invalidate();
                            Update();
                        }
                    }
                }
                else
                {
                    _backgroundImage = null;

                    if (_headerStyle == TabHeaderStyle.Texture)
                    {
                        Invalidate();
                        Update();
                    }
                }
            }
        }

        [Editor(typeof(TabpageExCollectionEditor), typeof(UITypeEditor))]
        [Browsable(true)]
        public new TabPageCollection TabPages
        {
            get
            {
                return base.TabPages;
            }
        }

        /// <summary>
        /// The ItemSize's height value must be in the range of 22 to 80 size(HEIGHT), ItemSize's width value does not effect on the control.
        /// </summary>
        public new Size ItemSize
        {
            get { return base.ItemSize; }
            set
            {
                if (!value.Equals(base.ItemSize))
                {
                    if (value.Height < 22)
                        value.Height = 22;
                    else if (value.Height > 80)
                        value.Height = 80;

                    base.ItemSize = value;

                    Invalidate();
                    Update();
                }
            }
        }

        public override System.Drawing.Rectangle DisplayRectangle
        {
            get
            {
                if (conditionBooleanArray[6])
                    return new Rectangle(1, 1, this.Width - 2, this.Height - 2);
                else if (conditionBooleanArray[1] && !conditionBooleanArray[4] && this.TabCount <= 1)
                {
                    switch (_alignments)
                    {
                        case TabAlignments.Top:
                            return new Rectangle(1, 1, this.Width - 2, this.Height - (CAPTIONHEIGHT + 3));
                        default:
                            return new Rectangle(1, CAPTIONHEIGHT + 2, this.Width - 2, this.Height - (CAPTIONHEIGHT + 3));
                    }
                }
                else
                {
                    if (conditionBooleanArray[1])
                    {
                        switch (_alignments)
                        {
                            case TabAlignments.Top:
                                return new Rectangle(1, ItemSize.Height + 6, this.Width - 2, this.Height - (ItemSize.Height + CAPTIONHEIGHT + 8));
                            default:
                                return new Rectangle(1, CAPTIONHEIGHT + 2, this.Width - 2, this.Height - (ItemSize.Height + CAPTIONHEIGHT + 7));
                        }
                    }
                    else
                    {
                        if (conditionBooleanArray[2])
                        {
                            switch (_alignments)
                            {
                                case TabAlignments.Top:
                                    return new Rectangle(5, ItemSize.Height + 6, this.Width - 10, this.Height - (ItemSize.Height + 11));
                                default:
                                    return new Rectangle(5, 5, this.Width - 10, this.Height - (ItemSize.Height + 11));
                            }
                        }
                        else
                        {
                            switch (_alignments)
                            {
                                case TabAlignments.Top:
                                    if (_tabStyles != TabStyle.VS2010)
                                        return new Rectangle(1, ItemSize.Height + 6, this.Width - 2, this.Height - (ItemSize.Height + 7));
                                    else
                                        return new Rectangle(1, ItemSize.Height + 6, this.Width - 2, this.Height - (ItemSize.Height + 11));
                                default:
                                    if (_tabStyles != TabStyle.VS2010)
                                        return new Rectangle(1, 1, this.Width - 2, this.Height - (ItemSize.Height + 7));
                                    else
                                        return new Rectangle(1, 5, this.Width - 2, this.Height - (ItemSize.Height + 11));
                            }
                        }
                    }
                }
            }
        }

        #region Previous Hided Members

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Color BackColor
        {
            get
            {
                // Do not change on this value.
                return Color.Transparent;
            }
        }

        [DefaultValue(typeof(TabDrawMode), "OwnerDrawFixed")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new TabDrawMode DrawMode
        {
            get
            {
                return TabDrawMode.OwnerDrawFixed;
            }
        }

        [Browsable(false)]
        public new TabAlignment Alignment
        {
            get { return base.Alignment; }
            set
            {
                if (!value.Equals(base.Alignment))
                {
                    base.Alignment = value;

                    Invalidate();
                    Update();
                }
            }
        }

        [DefaultValue(false)]
        [Browsable(false)]
        public new bool Multiline
        {
            get { return base.Multiline; }
            set
            {
                if (!value.Equals(base.Multiline))
                {
                    if (conditionBooleanArray[6])
                        base.Multiline = true;
                    else
                        base.Multiline = false;
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new TabAppearance Appearance
        {
            get { return base.Appearance; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new TabSizeMode SizeMode
        {
            get { return base.SizeMode; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool HotTrack
        {
            get { return base.HotTrack; }
        }

        #endregion

        #endregion

        #region Override Methods

        protected override void OnResize(EventArgs e)
        {
            try
            {
                if (!this.Multiline && _upDown32 != null)
                {
                    Rectangle rctTabArea = this.DisplayRectangle;

                    if (this.Alignments == TabAlignments.Top)
                        _myScroller.Location = new Point(this.Width - _myScroller.Width - 4, rctTabArea.Top - _myScroller.Height - 8);
                    else
                        _myScroller.Location = new Point(this.Width - _myScroller.Width - 4, rctTabArea.Bottom + 8);
                }
            }
            catch (IndexOutOfRangeException) { }
            finally
            {
                base.OnResize(e);
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (Visible)
            {
                // Draw Caption Bar
                DrawCaption(pe.Graphics);

                if (conditionBooleanArray[6])
                {
                    if (this.Width <= 2 || this.Height <= 2)
                        return;
                }

                if (!conditionBooleanArray[1])
                {
                    if (this.Width <= 2 || (this.TabCount == 0 ? this.Height <= 2 : DisplayRectangle.Height <= -4))
                        return;
                }
                
                // If appropriate fill tab control's background and paint tab items.
                Draw(pe.Graphics);
            }
        }

        protected override void OnGiveFeedback(GiveFeedbackEventArgs gfbevent)
        {
            gfbevent.UseDefaultCursors = _myCursor == null ? true : false;

            if (gfbevent.Effect == DragDropEffects.Move)
                Cursor.Current = _myCursor;
            else
            {
                Cursor.Current = Cursors.No;

                if (dragDropArrowArray[0] != null && dragDropArrowArray[0].Visible)
                    dragDropArrowArray[0].Hide();

                if (dragDropArrowArray[1] != null && dragDropArrowArray[1].Visible)
                    dragDropArrowArray[1].Hide();
            }
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            if (!DesignMode)
            {
                if (drgevent.KeyState == 1 && drgevent.Data.GetDataPresent(typeof(TabPageEx)))
                {
                    TabPageEx over = OverTab();
                    if (over != null)
                    {
                        TabPageEx draggingTab = drgevent.Data.GetData(typeof(TabPageEx)) as TabPageEx;

                        int draggingTabIndex = ((KRBTabControl)draggingTab.Parent).TabPages.IndexOf(draggingTab);
                        int overringTabIndex = this.TabPages.IndexOf(over);

                        bool checkTabs = over.Parent == draggingTab.Parent;
                        if (!checkTabs)
                            draggingTabIndex = overringTabIndex;
                        
                        if (checkTabs ? overringTabIndex != draggingTabIndex : true)
                        {
                            drgevent.Effect = DragDropEffects.Move;
                            Rectangle rctOver = this.GetTabRect(overringTabIndex);

                            if (overringTabIndex == 0)
                            {
                                rctOver.X += _tabHOffset;
                            }

                            if (_alignments == TabAlignments.Top)
                            {
                                rctOver.Y += 3;
                            }
                            else
                            {
                                rctOver.Y -= 3;
                            }

                            if (dragDropArrowArray[0] == null)
                                dragDropArrowArray[0] = new ArrowWindow(Resources.UpArrow, 255);

                            if (dragDropArrowArray[1] == null)
                                dragDropArrowArray[1] = new ArrowWindow(Resources.DownArrow, 255);

                            Point upArrowPoint = Point.Empty;
                            Point downArrowPoint = Point.Empty;

                            if (overringTabIndex > draggingTabIndex)
                            {
                                if (_alignments == TabAlignments.Top)
                                {
                                    downArrowPoint = new Point(rctOver.Right - dragDropArrowArray[1].Width / 2 + 1, rctOver.Top + 2 - dragDropArrowArray[1].Height);
                                    upArrowPoint = new Point(rctOver.Right - dragDropArrowArray[0].Width / 2 + 1, rctOver.Bottom - 10);
                                }
                                else
                                {
                                    downArrowPoint = new Point(rctOver.Right - dragDropArrowArray[1].Width / 2 + 1, rctOver.Top + 11 - dragDropArrowArray[1].Height);
                                    upArrowPoint = new Point(rctOver.Right - dragDropArrowArray[0].Width / 2 + 1, rctOver.Bottom - 3);
                                }
                            }
                            else
                            {
                                if (_alignments == TabAlignments.Top)
                                {
                                    downArrowPoint = new Point(rctOver.Left - dragDropArrowArray[1].Width / 2 + 1, rctOver.Top + 2 - dragDropArrowArray[1].Height);
                                    upArrowPoint = new Point(rctOver.Left - dragDropArrowArray[0].Width / 2 + 1, rctOver.Bottom - 10);
                                }
                                else
                                {
                                    downArrowPoint = new Point(rctOver.Left - dragDropArrowArray[1].Width / 2 + 1, rctOver.Top + 11 - dragDropArrowArray[1].Height);
                                    upArrowPoint = new Point(rctOver.Left - dragDropArrowArray[0].Width / 2 + 1, rctOver.Bottom - 3);
                                }
                            }

                            dragDropArrowArray[0].Location = this.PointToScreen(upArrowPoint);
                            dragDropArrowArray[1].Location = this.PointToScreen(downArrowPoint);

                            if (!dragDropArrowArray[0].Visible)
                                dragDropArrowArray[0].Show();

                            if (!dragDropArrowArray[1].Visible)
                                dragDropArrowArray[1].Show();
                        }
                        else
                            drgevent.Effect = DragDropEffects.None;
                    }
                    else
                        drgevent.Effect = DragDropEffects.None;
                }
                else
                    drgevent.Effect = DragDropEffects.None;
            }

            base.OnDragOver(drgevent);
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            if (!DesignMode)
            {
                if (drgevent.Data.GetDataPresent(typeof(TabPageEx)))
                {
                    TabPageEx over = OverTab();
                    if (over != null)
                    {
                        TabPageEx draggingTab = drgevent.Data.GetData(typeof(TabPageEx)) as TabPageEx;

                        if (draggingTab != over)
                        {
                            int overringTabIndex = this.TabPages.IndexOf(over);
                            if (!draggingTab.Parent.Equals(over.Parent))
                            {
                                ((KRBTabControl)draggingTab.Parent).TabPages.Remove(draggingTab);
                                this.TabPages.Insert(overringTabIndex, draggingTab);
                            }
                            else
                            {
                                int draggingTabIndex = this.TabPages.IndexOf(draggingTab);
                                // Switching tab indexes, we don't remove the dragging tab, because; dragging tab already within the current tab container.
                                this.TabPages[overringTabIndex] = draggingTab;
                                this.TabPages[draggingTabIndex] = over;
                            }

                            // Select our dragging tab.
                            this.SelectedTab = draggingTab;
                            UpdateTabSelection(true);

                            dragDropArrowArray[0].Hide();
                            dragDropArrowArray[1].Hide();
                        }
                    }
                }
            }

            base.OnDragDrop(drgevent);
        }

        // TabItemCloseButton(0), CaptionDropDownButton(1), CaptionCloseButton(2)
        static byte[] processes = { 0, 0, 0 };
        // TabItemCloseButton(0), CaptionDropDownButton(1), CaptionCloseButton(2)
        static bool[] _mouseImgProcessing = { true, true, true };
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!conditionBooleanArray[6])
            {
                #region "/*** TabItemCloseButton ***/"

                if (conditionRectangleArray[0].Contains(e.Location))
                {
                    if (processes[0] != 1)
                    {
                        conditionButtonStateArray[0] = ButtonState.Hover;

                        _mouseImgProcessing[0] = false;
                        Invalidate();
                        processes[0]++;
                    }

                    goto Enough;
                }
                else
                {
                    if (!_mouseImgProcessing[0])
                    {
                        conditionButtonStateArray[0] = ButtonState.Normal;

                        _mouseImgProcessing[0] = true;
                        Invalidate(conditionRectangleArray[0]);
                        processes[0] = 0;
                    }
                }

                #endregion

                if (conditionBooleanArray[1])
                {
                    #region "/*** CaptionDropDownButton ***/"

                    if (conditionRectangleArray[1].Contains(e.Location))
                    {
                        if (processes[1] != 1)
                        {
                            conditionButtonStateArray[1] = ButtonState.Hover;

                            _mouseImgProcessing[1] = false;
                            Invalidate();
                            processes[1]++;
                        }

                        goto Enough;
                    }
                    else
                    {
                        if (!_mouseImgProcessing[1])
                        {
                            conditionButtonStateArray[1] = ButtonState.Normal;

                            _mouseImgProcessing[1] = true;
                            Invalidate();
                            processes[1] = 0;
                        }
                    }

                    #endregion

                    #region "/*** CaptionCloseButton ***/"

                    if (conditionRectangleArray[2].Contains(e.Location))
                    {
                        if (processes[2] != 1)
                        {
                            conditionButtonStateArray[2] = ButtonState.Hover;

                            _mouseImgProcessing[2] = false;
                            Invalidate();
                            processes[2]++;
                        }
                    }
                    else
                    {
                        if (!_mouseImgProcessing[2])
                        {
                            conditionButtonStateArray[2] = ButtonState.Normal;

                            _mouseImgProcessing[2] = true;
                            Invalidate();
                            processes[2] = 0;
                        }
                    }

                    #endregion
                }
            }

        Enough: ;
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (!conditionBooleanArray[6])
            {
                if (conditionButtonStateArray[0] == ButtonState.Hover || conditionButtonStateArray[0] == ButtonState.Pressed)
                {
                    processes[0] = 0;
                    conditionButtonStateArray[0] = ButtonState.Normal;
                    Invalidate(conditionRectangleArray[0]);
                }
                else if (conditionButtonStateArray[1] == ButtonState.Hover || conditionButtonStateArray[1] == ButtonState.Pressed)
                {
                    processes[1] = 0;
                    conditionButtonStateArray[1] = ButtonState.Normal;
                    Invalidate(conditionRectangleArray[1]);
                }
                else if (conditionButtonStateArray[2] == ButtonState.Hover || conditionButtonStateArray[2] == ButtonState.Pressed)
                {
                    processes[2] = 0;
                    conditionButtonStateArray[2] = ButtonState.Normal;
                    Invalidate(conditionRectangleArray[2]);
                }
            }

            base.OnMouseLeave(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            // Fix: re-paint caption bar.
            if (!conditionBooleanArray[6] && conditionBooleanArray[1])
                Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            // Fix: re-paint caption bar.
            if (!conditionBooleanArray[6] && conditionBooleanArray[1])
                Invalidate();
        }
        
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (dragDropArrowArray[0] != null)
                    dragDropArrowArray[0].Hide();
                if (dragDropArrowArray[1] != null)
                    dragDropArrowArray[1].Hide();
            }

            base.OnKeyUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs ke)
        {
            if (ke.KeyCode == Keys.Escape)
            {
                if (dragDropArrowArray[0] != null && dragDropArrowArray[0].Visible)
                    dragDropArrowArray[0].Hide();

                if (dragDropArrowArray[1] != null && dragDropArrowArray[1].Visible)
                    dragDropArrowArray[1].Hide();
            }

            base.OnKeyDown(ke);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!conditionBooleanArray[6] && e.Button == MouseButtons.Left)
            {
                if (conditionRectangleArray[0].Contains(e.Location))
                {
                    conditionButtonStateArray[0] = ButtonState.Pressed;
                    Invalidate(conditionRectangleArray[0]);
                }
                else if (conditionBooleanArray[1] && conditionRectangleArray[1].Contains(e.Location))
                {
                    conditionButtonStateArray[1] = ButtonState.Pressed;
                    Invalidate(conditionRectangleArray[1]);
                }
                else if (conditionBooleanArray[1] && conditionRectangleArray[2].Contains(e.Location))
                {
                    conditionButtonStateArray[2] = ButtonState.Pressed;
                    Invalidate(conditionRectangleArray[2]);
                }
                else if (!DesignMode && this.TabCount >= 1)
                {
                    Rectangle draggingTab = this.GetTabRect(this.SelectedIndex);

                    if (this.SelectedIndex == 0)
                    {
                        if (draggingTab.X + _tabHOffset > e.X)
                            goto Finalize;
                    }

                    if (_alignments == TabAlignments.Top)
                        draggingTab.Y += 3;
                    else
                        draggingTab.Y -= 3;

                    if (draggingTab.Contains(e.X, e.Y))
                    {
                        string filePath = System.IO.Path.Combine(Application.StartupPath, "Drag.cur");

                        if (_myCursor == null)
                            _myCursor = GetCustomCursor(filePath);

                        this.DoDragDrop((TabPageEx)this.SelectedTab, DragDropEffects.Move);
                    }
                }
            }

        Finalize: ;
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!conditionBooleanArray[6] && e.Button == MouseButtons.Left)
            {
                #region TabItemClose

                if (conditionRectangleArray[0].Contains(e.Location))
                {
                    conditionButtonStateArray[0] = ButtonState.Hover;
                    Invalidate(conditionRectangleArray[0]);
                    goto Finalize;
                }
                else
                {
                    conditionButtonStateArray[0] = ButtonState.Normal;
                    Invalidate(conditionRectangleArray[0]);
                }

                #endregion

                if (conditionBooleanArray[1])
                {
                    #region CaptionDropDown

                    if (conditionRectangleArray[1].Contains(e.Location))
                    {
                        conditionButtonStateArray[1] = ButtonState.Hover;
                        Invalidate(conditionRectangleArray[1]);
                        goto Finalize;
                    }
                    else
                    {
                        conditionButtonStateArray[1] = ButtonState.Normal;
                        Invalidate(conditionRectangleArray[1]);
                    }
                    
                    #endregion

                    #region CaptionClose
                    
                    if (conditionRectangleArray[2].Contains(e.Location))
                    {
                        conditionButtonStateArray[2] = ButtonState.Hover;
                        Invalidate(conditionRectangleArray[2]);
                    }
                    else
                    {
                        conditionButtonStateArray[2] = ButtonState.Normal;
                        Invalidate(conditionRectangleArray[2]);
                    }

                    #endregion
                }
            }

        Finalize: ;
            base.OnMouseUp(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (!conditionBooleanArray[6])
            {
                if (e.Button == MouseButtons.Left)
                {
                    // IsCaptionVisible?
                    if (conditionBooleanArray[1])
                    {
                        if (conditionRectangleArray[1].Contains(e.Location))
                        {
                            ContextMenuStrip contextMenu = new System.Windows.Forms.ContextMenuStrip();
                            Point dropDownLocation = PointToScreen(new Point(conditionRectangleArray[1].X, conditionRectangleArray[1].Bottom));

                            using (ContextMenuShownEventArgs ea = new ContextMenuShownEventArgs(contextMenu, dropDownLocation))
                            {
                                // Fire a Notification Event.
                                OnContextMenuShown(ea);
                            }

                            goto Finalize;
                        }
                        else if (this.TabCount > 0 && conditionRectangleArray[2].Contains(e.Location))
                        {
                            TabPageEx closingTabPage = (TabPageEx)this.SelectedTab;
                            bool flag = closingTabPage.IsClosable ? closingTabPage.Enabled : false;

                            if (flag)
                            {
                                using (SelectedIndexChangingEventArgs sea = new SelectedIndexChangingEventArgs(closingTabPage, this.SelectedIndex))
                                {
                                    // Fire a Notification Event.
                                    OnTabPageClosing(sea);

                                    if (!sea.Cancel)
                                    {
                                        this.TabPages.Remove(closingTabPage);
                                        SelectNextAvailableTabPage();
                                    }
                                    else
                                        MessageBox.Show("The operation was canceled by the user.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    
                                    Cursor.Position = PointToScreen(e.Location);
                                }
                            }

                            goto Finalize;
                        }
                    }

                    // IsDocumentTabStyle?
                    if (!conditionBooleanArray[4] ? this.TabCount > 1 : this.TabCount >= 1)
                    {
                        TabPageEx closingTabPage = (TabPageEx)this.SelectedTab;
                        if (conditionRectangleArray[0].Contains(e.Location))
                        {
                            bool flag = closingTabPage.IsClosable ? !closingTabPage.Enabled || closingTabPage.preventClosing : true;
                            if (!flag)
                            {
                                using (SelectedIndexChangingEventArgs te = new SelectedIndexChangingEventArgs(closingTabPage, this.SelectedIndex))
                                {
                                    // Fire a Notification Event.
                                    OnTabPageClosing(te);

                                    if (!te.Cancel)
                                    {
                                        this.SelectedTab.Dispose();
                                        if (DesignMode)
                                        {
                                            ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
                                            selectionService.SetSelectedComponents(new IComponent[] { this }, SelectionTypes.Auto);
                                        }
                                        else
                                            SelectNextAvailableTabPage();
                                    }

                                    Cursor.Position = PointToScreen(e.Location);
                                }
                            }
                            else
                                closingTabPage.preventClosing = false;
                        }
                        else
                            closingTabPage.preventClosing = false;
                    }
                }
            }

        Finalize: ;
            base.OnMouseClick(e);
        }
        
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (!conditionBooleanArray[6])
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (!conditionBooleanArray[4] ? this.TabCount > 1 : this.TabCount >= 1)
                    {
                        TabPageEx closingTabPage = (TabPageEx)this.SelectedTab;
                        if (conditionRectangleArray[0].Contains(e.Location))
                        {
                            bool flag = closingTabPage.IsClosable ? !closingTabPage.Enabled || closingTabPage.preventClosing : true;
                            if (!flag)
                            {
                                using (SelectedIndexChangingEventArgs te = new SelectedIndexChangingEventArgs(closingTabPage, this.SelectedIndex))
                                {
                                    // Fire a Notification Event.
                                    OnTabPageClosing(te);

                                    if (!te.Cancel)
                                    {
                                        this.SelectedTab.Dispose();
                                        if (DesignMode)
                                        {
                                            ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
                                            selectionService.SetSelectedComponents(new IComponent[] { this }, SelectionTypes.Auto);
                                        }
                                    }
                                    
                                    Cursor.Position = PointToScreen(e.Location);
                                }
                            }
                            else
                                closingTabPage.preventClosing = false;
                        }
                        else
                            closingTabPage.preventClosing = false;
                    }
                }
            }

            base.OnMouseDoubleClick(e);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            if (_upDown32 != null)
            {
                if (this.SelectedIndex == 0)
                {
                    _myScroller._leftScroller.Enabled = false;
                    _myScroller._rightScroller.Enabled = true;
                }
                else if (this.SelectedIndex == this.TabCount - 1)
                {
                    _myScroller._leftScroller.Enabled = true;
                    _myScroller._rightScroller.Enabled = false;
                }
                else
                {
                    _myScroller._leftScroller.Enabled = true;
                    _myScroller._rightScroller.Enabled = true;
                }
            }

            base.OnSelectedIndexChanged(e);
            this.Invalidate();
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            if (e.Control is TabPageEx && e.Control.Text == String.Empty)
            {
                TabPageEx adding = e.Control as TabPageEx;

                int value = 0;

                foreach (TabPage tab in this.TabPages)
                {
                    if (tab.Name.Contains("tabPageEx"))
                    {
                        try
                        {
                            int current = Convert.ToInt32(tab.Name.Substring(9, tab.Name.Length - 9));
                            value = Math.Max(value, current);
                        }
                        catch { }
                    }
                }

                adding.Name = "tabPageEx" + (value + 1).ToString();
                adding.Text = adding.Name;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData == (Keys.Tab | Keys.Control)) || (keyData == (Keys.Tab | Keys.Shift | Keys.Control)))
            {
                if (!OnNavigateTabPage((keyData & Keys.Shift) != Keys.Shift ? this.SelectedIndex + 1 : this.SelectedIndex - 1, true))
                {
                    msg.Result = new IntPtr(1);
                    return true;
                }
            }
            else
            {
                switch (keyData)
                {
                    // Selects Last TabPage
                    case Keys.End:
                        if (!OnNavigateTabPage(this.TabCount - 1, false))
                        {
                            msg.Result = new IntPtr(1);
                            return true;
                        }
                        break;
                    // Selects First TabPage
                    case Keys.Home:
                        if (!OnNavigateTabPage(0, false))
                        {
                            msg.Result = new IntPtr(1);
                            return true;
                        }
                        break;
                    // Selects the tab on the left side of the currently selected TabPage
                    case Keys.Left:
                        if (!OnNavigateTabPage(this.SelectedIndex - 1, false))
                        {
                            msg.Result = new IntPtr(1);
                            return true;
                        }
                        break;
                    // Selects the tab on the right side of the currently selected TabPage
                    case Keys.Right:
                        if (!OnNavigateTabPage(this.SelectedIndex + 1, false))
                        {
                            msg.Result = new IntPtr(1);
                            return true;
                        }
                        break;
                    case Keys.Insert:
                        if (conditionBooleanArray[3] && MessageBox.Show("Do you want to insert a new tab page here?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            TabPageEx tabPage = new TabPageEx();
                            this.Controls.Add(tabPage);
                        }
                        break;
                    case Keys.Delete:
                        if (conditionBooleanArray[3] && this.TabCount > 0)
                        {
                            if (MessageBox.Show("Do you want to remove the selected tab page?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                TabPageEx removingTabPage = this.SelectedTab as TabPageEx;
                                if (removingTabPage != null && removingTabPage.IsClosable && removingTabPage.Enabled)
                                {
                                    using (SelectedIndexChangingEventArgs e = new SelectedIndexChangingEventArgs(removingTabPage, this.SelectedIndex))
                                    {
                                        // Fire a Notification Event.
                                        OnTabPageClosing(e);

                                        if (!e.Cancel)
                                        {
                                            this.TabPages.Remove(removingTabPage);
                                            SelectNextAvailableTabPage();
                                        }
                                        else
                                            MessageBox.Show("The operation was canceled by the user.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("The selected tab page could not be deleted!!!, it may be due to the following reasons;\r\n\r\n1.Tab page might be null or disposed by the application.\r\n2.Tab page might not be closable.\r\n3.Tab page might be disable.",
                                        Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                        break;
                    case Keys.Escape:
                        break;
                    case Keys.F1:
                        break;
                }
            }

            return true;
        }

        protected override bool ProcessMnemonic(char charCode)
        {
            foreach (TabPageEx tabPage in this.TabPages)
            {
                if (IsMnemonic(charCode, tabPage.Text))
                {
                    if (!tabPage.Enabled)
                        return true;

                    using (SelectedIndexChangingEventArgs e = new SelectedIndexChangingEventArgs(tabPage, this.TabPages.IndexOf(tabPage)))
                    {
                        // Fire a Notification Event.
                        OnSelectedIndexChanging(e);

                        if (!e.Cancel)
                            this.SelectedTab = tabPage;

                        return true;
                    }
                }
            }

            return base.ProcessMnemonic(charCode);
        }

        protected override void WndProc(ref Message m)
        {
            #region Tab Control Notification Messages

            //if (m.Msg == (int)(User32.Msgs.WM_REFLECT | User32.Msgs.WM_NOTIFY))
            //{
            //    if (!DesignMode)
            //    {
            //        User32.NMTCKEYDOWN tcn = (User32.NMTCKEYDOWN)(System.Runtime.InteropServices.Marshal.PtrToStructure(m.LParam, typeof(User32.NMTCKEYDOWN)));
            //        switch (tcn.hdr.code)
            //        {
            //            // TCN_FIRST, handle virtual keys from tcn.wVKey parameters.
            //            case -550:
            //                break;
            //            // TCN_SELCHANGE
            //            case -551:
            //                break;
            //            // TCN_SELCHANGING, process virtual keys in here.
            //            case -552:
            //                break;
            //        }
            //    }
            //}

            #endregion
            
            base.WndProc(ref m);
            if (m.Msg == (int)User32.Msgs.WM_PARENTNOTIFY)
            {
                if ((ushort)(m.WParam.ToInt32() & 0xFFFF) == (int)User32.Msgs.WM_CREATE)
                {
                    IntPtr handle = FindUpDownControl();
                    if (handle != IntPtr.Zero)
                    {
                        if (_upDown32 != null)
                            _upDown32.ReleaseHandle();

                        _upDown32 = new UpDown32(handle);

                        if (_myScroller != null)
                        {
                            _myScroller.ScrollLeft -= new EventHandler(ScrollLeft);
                            _myScroller.ScrollRight -= new EventHandler(ScrollRight);
                            _myScroller.Dispose();
                            _myScroller = null;
                        }

                        _myScroller = new Scroller(_upDownStyle);
                        _myScroller.ScrollLeft += new EventHandler(ScrollLeft);
                        _myScroller.ScrollRight += new EventHandler(ScrollRight);

                        IntPtr parentHwnd = User32.GetParent(_myScroller.Handle);

                        if (parentHwnd != m.HWnd)
                            User32.SetParent(_myScroller.Handle, m.HWnd);

                        if (this.SelectedIndex == 0)
                            _myScroller._leftScroller.Enabled = false;
                        else if (this.SelectedIndex == this.TabCount - 1)
                            _myScroller._rightScroller.Enabled = false;

                        this.OnResize(EventArgs.Empty);
                    }
                }
            }
            else if (m.Msg == (int)User32.Msgs.WM_LBUTTONDOWN)
            {
                if (this.TabCount > 1)
                {
                    TabPageEx selectingTabPage = OverTab();
                    if (selectingTabPage != null)
                    {
                        int index = TabPages.IndexOf(selectingTabPage);
                        if (index != this.SelectedIndex)
                        {
                            if (!selectingTabPage.Enabled)
                                m.Result = new IntPtr(1);
                            else
                            {
                                using (SelectedIndexChangingEventArgs e = new SelectedIndexChangingEventArgs(selectingTabPage, index))
                                {
                                    // Fire a Notification Event.
                                    OnSelectedIndexChanging(e);

                                    if (e.Cancel)
                                        m.Result = new IntPtr(1);
                                    else
                                    {
                                        selectingTabPage.preventClosing = true;
                                        this.SelectedTab = selectingTabPage;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (m.Msg == (int)User32.Msgs.WM_PAINT)
            {
                if (_upDown32 != null && !this.Multiline)
                {
                    if (this.TabCount > 1)
                    {
                        int width = 0;

                        for (int i = 0; i < this.TabCount; i++)
                            width += GetTabRect(i).Width;

                        if (width <= _myScroller.Left)
                            _myScroller.Visible = false;
                        else
                            _myScroller.Visible = true;
                    }
                    else if (this.TabCount <= 1)
                        _myScroller.Visible = false;
                }
            }
            else if (m.Msg == (int)User32.Msgs.WM_NCHITTEST)
            {
                if (m.Result.ToInt32() == User32._HT_TRANSPARENT)
                    m.Result = (IntPtr)User32._HT_CLIENT;
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tabGradient.GradientChanged -= CONTROL_INVALIDATE_UPDATE;
                _tabGradient.Dispose();

                _hatcher.HatchChanged -= CONTROL_INVALIDATE_UPDATE;
                _hatcher.Dispose();

                _gradientCaption.GradientChanged -= CONTROL_INVALIDATE_UPDATE;
                _gradientCaption.Dispose();

                _captionButtons.ButtonsColorChanged -= CONTROL_INVALIDATE_UPDATE;
                _captionButtons.Dispose();

                _captionRandomizer.CaptionRandomizerChanged -= CONTROL_INVALIDATE_UPDATE;
                _captionRandomizer.Dispose();

                if (_tabPageExPool != null)
                    _tabPageExPool.Dispose();
                if (_upDown32 != null)
                    _upDown32.ReleaseHandle();
                if (_myCursor != null)
                {
                    _myCursor.Dispose();
                    _myCursor = null;
                }
                if (dragDropArrowArray[0] != null)
                {
                    dragDropArrowArray[0].Dispose();
                    dragDropArrowArray[0] = null;
                }
                if (dragDropArrowArray[1] != null)
                {
                    dragDropArrowArray[1].Dispose();
                    dragDropArrowArray[1] = null;
                }
                if (_myScroller != null)
                {
                    _myScroller.ScrollLeft -= ScrollLeft;
                    _myScroller.ScrollRight -= ScrollRight;
                    _myScroller.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Virtual Methods

        protected virtual TabPageEx OverTab()
        {
            TabPageEx over = null;

            Point pt = this.PointToClient(Cursor.Position);
            User32.TCHITTESTINFO mouseInfo = new User32.TCHITTESTINFO(pt, User32.TabControlHitTest.TCHT_ONITEM);
            int currentTabIndex = User32.SendMessage(this.Handle, User32._TCM_HITTEST, IntPtr.Zero, ref mouseInfo);

            if (currentTabIndex > -1)
            {
                Rectangle currentTabRct = this.GetTabRect(currentTabIndex);

                if (currentTabIndex == 0)
                    currentTabRct.X += _tabHOffset;

                if (_alignments == TabAlignments.Top)
                    currentTabRct.Y += 3;
                else
                    currentTabRct.Y -= 3;

                if (currentTabRct.Contains(pt))
                    over = this.TabPages[currentTabIndex] as TabPageEx;
            }

            return over;
        }

        /// <summary>
        /// Selects a tab page in the tab control.
        /// </summary>
        /// <param name="tabPageIndex">Index of the tab to select</param>
        /// <param name="wrap">Is command keys? (Control, Shift or Tab Keys.)</param>
        /// <returns>Returns TRUE to prevent the selection from changing, or FALSE to allow the selection to change.</returns>
        protected virtual bool OnNavigateTabPage(int tabPageIndex, bool wrap)
        {
            if ((tabPageIndex == this.SelectedIndex) || this.TabCount <= 1)
                return true;

            try
            {
                /* Control, Shift, Tab Keys */
                if (wrap)
                {
                    if (tabPageIndex > this.TabCount - 1)
                        tabPageIndex = 0;
                    else if (tabPageIndex < 0)
                        tabPageIndex = this.TabCount - 1;
                }

                TabPageEx selectingTabPage = this.TabPages[tabPageIndex] as TabPageEx;
                if (selectingTabPage == null)
                    return true;
                else
                {
                    if (!selectingTabPage.Enabled)
                        return true;

                    using (SelectedIndexChangingEventArgs e = new SelectedIndexChangingEventArgs(selectingTabPage, tabPageIndex))
                    {
                        // Fire a Notification Event.
                        OnSelectedIndexChanging(e);

                        if (!e.Cancel)
                        {
                            this.SelectedTab = selectingTabPage;
                            return false;
                        }

                        return true;
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                return true;
            }
        }

        protected virtual void Draw(Graphics gfx)
        {
            DrawBorder(gfx);

            // Draw Tab Header Background and Fill the tabpage border line.
            if (this.SelectedTab != null && !conditionBooleanArray[6] && (!conditionBooleanArray[4] ? (conditionBooleanArray[1] ? this.TabCount > 1 : true) : true))
            {
                Rectangle rctTabArea = this.DisplayRectangle;

                if (!conditionBooleanArray[1] && _tabStyles == TabStyle.VS2010)
                {
                    Rectangle bottomLine;
                    switch (_alignments)
                    {
                        case TabAlignments.Top:
                            bottomLine = new Rectangle(rctTabArea.Left, rctTabArea.Bottom, rctTabArea.Width, 4);
                            break;
                        default:
                            bottomLine = new Rectangle(rctTabArea.Left, 1, rctTabArea.Width, 4);
                            break;
                    }

                    using (LinearGradientBrush brush = new LinearGradientBrush(bottomLine, _tabGradient.ColorEnd, _tabGradient.ColorStart, _tabGradient.GradientStyle))
                    {
                        Blend bl = new Blend(2);
                        bl.Factors = new float[] { 0.2F, 1.0F };
                        bl.Positions = new float[] { 0.0F, 1.0F };
                        brush.Blend = bl;
                        gfx.FillRectangle(brush, bottomLine);
                    }
                }

                if (IsDrawHeader)
                {
                    Rectangle rct = this.ClientRectangle;
                    rct.Inflate(-1, -1);

                    Rectangle rctTabHeader;
                    switch (_alignments)
                    {
                        case TabAlignments.Top:
                            rctTabHeader = new Rectangle(rct.Left, rct.Top, rct.Width, rctTabArea.Top - 6);
                            break;
                        default:
                            rctTabHeader = new Rectangle(rct.Left, rctTabArea.Bottom + 5, rct.Width, rct.Bottom - rctTabArea.Bottom - 5);
                            break;
                    }

                    using (Bitmap overlay = new Bitmap(rctTabHeader.Width + 1, rctTabHeader.Height + 1))
                    {
                        // Make an associated Graphics object.
                        using (Graphics gr = Graphics.FromImage(overlay))
                        {
                            gr.SmoothingMode = SmoothingMode.HighQuality;

                            if (_headerStyle == TabHeaderStyle.Solid)
                            {
                                using (Brush brush = new SolidBrush(colorArray[0]))
                                    gr.FillRectangle(brush, 0, 0, overlay.Width, overlay.Height);
                            }
                            else if (_headerStyle == TabHeaderStyle.Hatch)
                            {
                                using (Brush brush = new HatchBrush(_hatcher.HatchType, _hatcher.ForeColor, _hatcher.BackColor))
                                    gr.FillRectangle(brush, 0, 0, overlay.Width, overlay.Height);
                            }
                            else
                            {
                                if (_backgroundImage != null)
                                {
                                    using (Brush brush = new TextureBrush(_backgroundImage, WrapMode.TileFlipXY))
                                        gr.FillRectangle(brush, 0, 0, overlay.Width, overlay.Height);
                                }
                            }
                        }

                        gfx.DrawImage(overlay, rctTabHeader, 1, 1, overlay.Width - 1, overlay.Height - 1, GraphicsUnit.Pixel);
                    }
                }

                //Draw Tabs
                for (int i = 0; i < this.TabCount; i++)
                    DrawTabs(gfx, i);
            }
        }

        protected virtual void DrawTabs(Graphics gfx, int nIndex)
        {
            Rectangle currentTab = this.GetTabRect(nIndex);

            /* Eğer ilk tabın indisi 0 ise Rectangle'ın Left location değerini belirtilen oranda artırıyoruz.
            Aynı zamanda Tab genişliğinide gene belirlenen oran kadar küçültüyoruz. */
            if (nIndex == 0)
            {
                currentTab.X += _tabHOffset;
                currentTab.Width -= _tabHOffset;
            }

            if (_alignments == TabAlignments.Top)
                currentTab.Y += 3;
            else
                currentTab.Y -= 4;

            if (nIndex == this.SelectedIndex)
            {
                LinearGradientBrush pathBrush = null;

                Rectangle rct = this.ClientRectangle;
                Rectangle rctTabArea = this.DisplayRectangle;

                rct.Inflate(-1, -1);
                rctTabArea.Inflate(_value - 1, _value - 1);
                
                DashStyle borderStyle;
                switch (_borderStyle)
                {
                    case ControlBorderStyle.Dashed:
                        borderStyle = DashStyle.Dash;
                        break;
                    case ControlBorderStyle.Dotted:
                        borderStyle = DashStyle.Dot;
                        break;
                    default:
                        borderStyle = DashStyle.Solid;
                        break;
                }

                Image closeImg = GetCloseTabImage();
                switch (_alignments)
                {
                    case TabAlignments.Top:

                        conditionRectangleArray[0] = new Rectangle()  // Object Initializer
                        {
                            X = currentTab.Right - (closeImg.Width + 5),
                            Y = (currentTab.Height - closeImg.Height) / 2 + 5,
                            Width = closeImg.Width,
                            Height = closeImg.Height,
                        };

                        using (Pen pen1 = new Pen(colorArray[2]))
                        using (GraphicsPath gp = new GraphicsPath())
                        {
                            pen1.DashStyle = borderStyle;
                            if (!conditionBooleanArray[1] && conditionBooleanArray[2])
                            {
                                // Create an open figure
                                gp.AddLine(rct.Left, rctTabArea.Top - 4, currentTab.Left, rctTabArea.Top - 4);
                                gp.AddLine(currentTab.Left, currentTab.Top, currentTab.Right - 1, currentTab.Top);
                                gp.AddLine(currentTab.Right - 1, rctTabArea.Top - 4, rct.Right, rctTabArea.Top - 4);
                                gp.AddLine(rct.Right, rct.Bottom, rct.Left, rct.Bottom);
                                gp.CloseFigure();

                                using (Region reg = new System.Drawing.Region(gp))
                                {
                                    reg.Exclude(rctTabArea);
                                    pathBrush = new LinearGradientBrush(gp.GetBounds(), _tabGradient.ColorStart, _tabGradient.ColorEnd, _tabGradient.GradientStyle);
                                    Blend bl = new Blend(2);
                                    bl.Factors = new float[] { 0.3F, 1.0F };
                                    bl.Positions = new float[] { 0.0F, 1.0F };
                                    pathBrush.Blend = bl;
                                    gfx.FillRegion(pathBrush, reg);
                                }
                            }
                            else
                            {
                                // Create an open figure
                                gp.AddLine(rct.Left, rctTabArea.Top, rct.Left, rctTabArea.Top - 4);
                                gp.AddLine(currentTab.Left, rctTabArea.Top - 4, currentTab.Left, currentTab.Top);
                                gp.AddLine(currentTab.Right - 1, currentTab.Top, currentTab.Right - 1, rctTabArea.Top - 4);
                                gp.AddLine(rct.Right, rctTabArea.Top - 4, rct.Right, rctTabArea.Top);
                                gp.CloseFigure();

                                pathBrush = new LinearGradientBrush(gp.GetBounds(), _tabGradient.ColorStart, _tabGradient.ColorEnd, _tabGradient.GradientStyle);
                                Blend bl = new Blend(2);
                                bl.Factors = new float[] { 0.3F, 1.0F };
                                bl.Positions = new float[] { 0.0F, 1.0F };
                                pathBrush.Blend = bl;
                                gfx.FillPath(pathBrush, gp);
                            }

                            if (_tabStyles == TabStyle.OfficeXP)
                            {
                                gp.Reset();
                                gp.AddLine(currentTab.Left, currentTab.Top + 3, currentTab.Left, currentTab.Top);
                                gp.AddLine(currentTab.Right - 1, currentTab.Top, currentTab.Right - 1, currentTab.Top + 3);
                                gp.CloseFigure();

                                SmoothingMode graphicsMode = gfx.SmoothingMode;
                                using (LinearGradientBrush brush = new LinearGradientBrush(gp.GetBounds(), Color.OrangeRed, Color.SandyBrown, LinearGradientMode.Vertical))
                                {
                                    gfx.SmoothingMode = SmoothingMode.AntiAlias;
                                    Blend bl = new Blend(2);
                                    bl.Factors = new float[] { 0.1F, 1.0F };
                                    bl.Positions = new float[] { 0.0F, 1.0F };
                                    brush.Blend = bl;

                                    gfx.FillPath(brush, gp);
                                }

                                gfx.SmoothingMode = graphicsMode;
                            }

                            gp.Reset();
                            if (nIndex == 0 && _tabHOffset <= -1)
                                gp.AddLine(currentTab.Left, rctTabArea.Top - 4, currentTab.Left, rctTabArea.Top);
                            else
                                gp.AddLine(rct.Left, rctTabArea.Top - 4, currentTab.Left, rctTabArea.Top - 4);

                            gp.AddLine(currentTab.Left, currentTab.Top, currentTab.Right - 1, currentTab.Top);
                            gp.AddLine(currentTab.Right - 1, rctTabArea.Top - 4, rct.Right - 1, rctTabArea.Top - 4);

                            // Create another figure
                            gp.StartFigure();
                            if (!conditionBooleanArray[1] && conditionBooleanArray[2])
                            {
                                Rectangle innerRct = rctTabArea;
                                innerRct.Width -= 1;
                                innerRct.Height -= 1;
                                gp.AddRectangle(innerRct);
                            }
                            else
                                gp.AddLine(rct.Left, rctTabArea.Top, rct.Right - 1, rctTabArea.Top);
                            gfx.DrawPath(pen1, gp);

                            // Draw tab close image.
                            if (conditionButtonStateArray[0] == ButtonState.Normal)
                            {
                                using (ImageAttributes attributes = new ImageAttributes())
                                {
                                    ColorMap[] map = new ColorMap[2];
                                    map[0] = new ColorMap();
                                    map[0].OldColor = Color.White;
                                    map[0].NewColor = Color.Transparent;
                                    map[1] = new ColorMap();
                                    map[1].OldColor = Color.Black;
                                    map[1].NewColor = colorArray[3];

                                    attributes.SetRemapTable(map);
                                    gfx.DrawImage(closeImg, conditionRectangleArray[0], 0, 0, closeImg.Width, closeImg.Height, GraphicsUnit.Pixel, attributes);
                                }
                            }
                            else
                                gfx.DrawImageUnscaled(closeImg, conditionRectangleArray[0]);

                            closeImg.Dispose();
                        }
                        break;
                    case TabAlignments.Bottom:

                        conditionRectangleArray[0] = new Rectangle()  // Object Initializer
                        {
                            X = currentTab.Right - (closeImg.Width + 5),
                            Y = currentTab.Top + (currentTab.Height - closeImg.Height) / 2 + 2,
                            Width = closeImg.Width,
                            Height = closeImg.Height,
                        };

                        using (Pen pen1 = new Pen(colorArray[2]))
                        using (GraphicsPath gp = new GraphicsPath())
                        {
                            pen1.DashStyle = borderStyle;
                            if (!conditionBooleanArray[1] && conditionBooleanArray[2])
                            {
                                // Create an open figure
                                gp.AddLine(rct.Left, rctTabArea.Bottom + 3, currentTab.Left, rctTabArea.Bottom + 3);
                                gp.AddLine(currentTab.Left, currentTab.Bottom, currentTab.Right - 1, currentTab.Bottom);
                                gp.AddLine(currentTab.Right - 1, rctTabArea.Bottom + 3, rct.Right, rctTabArea.Bottom + 3);
                                gp.AddLine(rct.Right, rct.Top, rct.Left, rct.Top);
                                gp.CloseFigure();

                                using (Region reg = new System.Drawing.Region(gp))
                                {
                                    reg.Exclude(rctTabArea);
                                    pathBrush = new LinearGradientBrush(gp.GetBounds(), _tabGradient.ColorEnd, _tabGradient.ColorStart, _tabGradient.GradientStyle);
                                    Blend bl = new Blend(2);
                                    bl.Factors = new float[] { 0.3F, 1.0F };
                                    bl.Positions = new float[] { 0.0F, 1.0F };
                                    pathBrush.Blend = bl;
                                    gfx.FillRegion(pathBrush, reg);
                                }
                            }
                            else
                            {
                                // Create an open figure
                                gp.AddLine(rct.Left, rctTabArea.Bottom - 1, rct.Left, rctTabArea.Bottom + 3);
                                gp.AddLine(currentTab.Left, rctTabArea.Bottom + 3, currentTab.Left, currentTab.Bottom);
                                gp.AddLine(currentTab.Right - 1, currentTab.Bottom, currentTab.Right - 1, rctTabArea.Bottom + 3);
                                gp.AddLine(rct.Right, rctTabArea.Bottom + 3, rct.Right, rctTabArea.Bottom - 1);
                                gp.CloseFigure();

                                pathBrush = new LinearGradientBrush(gp.GetBounds(), _tabGradient.ColorEnd, _tabGradient.ColorStart, _tabGradient.GradientStyle);
                                Blend bl = new Blend(2);
                                bl.Factors = new float[] { 0.3F, 1.0F };
                                bl.Positions = new float[] { 0.0F, 1.0F };
                                pathBrush.Blend = bl;
                                gfx.FillPath(pathBrush, gp);
                            }

                            if (_tabStyles == TabStyle.OfficeXP)
                            {
                                gp.Reset();
                                gp.AddLine(currentTab.Left, currentTab.Bottom - 3, currentTab.Left, currentTab.Bottom);
                                gp.AddLine(currentTab.Right - 1, currentTab.Bottom, currentTab.Right - 1, currentTab.Bottom - 3);
                                gp.CloseFigure();

                                SmoothingMode graphicsMode = gfx.SmoothingMode;
                                using (LinearGradientBrush brush = new LinearGradientBrush(gp.GetBounds(), Color.SandyBrown, Color.OrangeRed, LinearGradientMode.Vertical))
                                {
                                    gfx.SmoothingMode = SmoothingMode.AntiAlias;
                                    Blend bl = new Blend(2);
                                    bl.Factors = new float[] { 0.3F, 1.0F };
                                    bl.Positions = new float[] { 0.0F, 1.0F };
                                    brush.Blend = bl;
                                    gfx.FillPath(brush, gp);
                                }

                                gfx.SmoothingMode = graphicsMode;
                            }

                            gp.Reset();
                            if (nIndex == 0 && _tabHOffset <= -1)
                                gp.AddLine(currentTab.Left, rctTabArea.Bottom - 1, currentTab.Left, currentTab.Bottom - 1);
                            else
                                gp.AddLine(rct.Left, rctTabArea.Bottom + 3, currentTab.Left, rctTabArea.Bottom + 3);

                            gp.AddLine(currentTab.Left, currentTab.Bottom, currentTab.Right - 1, currentTab.Bottom);
                            gp.AddLine(currentTab.Right - 1, rctTabArea.Bottom + 3, rct.Right - 1, rctTabArea.Bottom + 3);

                            // Create another figure
                            gp.StartFigure();
                            if (!conditionBooleanArray[1] && conditionBooleanArray[2])
                            {
                                Rectangle innerRct = rctTabArea;
                                innerRct.Width -= 1;
                                innerRct.Height -= 1;
                                gp.AddRectangle(innerRct);
                            }
                            else
                                gp.AddLine(rct.Left, rctTabArea.Bottom - 1, rct.Right - 1, rctTabArea.Bottom - 1);

                            gfx.DrawPath(pen1, gp);

                            // Draw tab close image.
                            if (conditionButtonStateArray[0] == ButtonState.Normal)
                            {
                                using (ImageAttributes attributes = new ImageAttributes())
                                {
                                    ColorMap[] map = new ColorMap[2];
                                    map[0] = new ColorMap();
                                    map[0].OldColor = Color.White;
                                    map[0].NewColor = Color.Transparent;
                                    map[1] = new ColorMap();
                                    map[1].OldColor = Color.Black;
                                    map[1].NewColor = colorArray[3];

                                    attributes.SetRemapTable(map);
                                    gfx.DrawImage(closeImg, conditionRectangleArray[0], 0, 0, closeImg.Width, closeImg.Height, GraphicsUnit.Pixel, attributes);
                                }
                            }
                            else
                                gfx.DrawImageUnscaled(closeImg, conditionRectangleArray[0]);

                            closeImg.Dispose();
                        }
                        break;
                }

                pathBrush.Dispose();
            }
            else
            {
                if (conditionBooleanArray[5])
                {
                    // Draw 3D Border Line for between tab pages.
                    using (Custom3DBorder tabBorder = new Custom3DBorder(gfx))
                    {
                        if (nIndex > this.SelectedIndex && nIndex != this.TabCount - 1)
                        {
                            if (_alignments == TabAlignments.Top)
                                tabBorder.Draw3DLine(currentTab.Right, currentTab.Top + 3, currentTab.Right, currentTab.Bottom - 11, ThreeDStyle.Groove, 1);
                            else
                                tabBorder.Draw3DLine(currentTab.Right, currentTab.Bottom - 5, currentTab.Right, currentTab.Top + 13, ThreeDStyle.Groove, 1);
                        }
                        else if (nIndex < this.SelectedIndex && nIndex != 0)
                        {
                            if (_alignments == TabAlignments.Top)
                                tabBorder.Draw3DLine(currentTab.Left, currentTab.Top + 3, currentTab.Left, currentTab.Bottom - 11, ThreeDStyle.Groove, 1);
                            else
                                tabBorder.Draw3DLine(currentTab.Left, currentTab.Bottom - 5, currentTab.Left, currentTab.Top + 13, ThreeDStyle.Groove, 1);
                        }
                    }
                }
            }

            //Draw Tab's Icon Image at the ImageList.
            if (this.ImageList != null && !this.TabPages[nIndex].ImageIndex.Equals(-1))
            {
                if (this.TabPages[nIndex].ImageIndex <= this.ImageList.Images.Count - 1)
                {
                    Image img = this.ImageList.Images[this.TabPages[nIndex].ImageIndex];
                    Rectangle currentImgRct;

                    if (_alignments == TabAlignments.Top)
                    {
                        currentImgRct = new Rectangle() //Object Initializer
                        {
                            X = currentTab.X + 5,
                            Y = (currentTab.Height - img.Height) / 2 + 4,
                            Width = img.Width,
                            Height = img.Height
                        };

                        currentTab = new Rectangle()
                        {
                            X = currentTab.X + 5 + img.Width,
                            Y = currentTab.Y,
                            Width = currentTab.Width - (5 + img.Width),
                            Height = currentTab.Height
                        };
                    }
                    else
                    {
                        currentImgRct = new Rectangle() //Object Initializer
                        {
                            X = currentTab.X + 5,
                            Y = currentTab.Bottom - (img.Height + 6),
                            Width = img.Width,
                            Height = img.Height
                        };

                        currentTab = new Rectangle()
                        {
                            X = currentTab.X + 5 + img.Width,
                            Y = currentTab.Y + 5,
                            Width = currentTab.Width - (5 + img.Width),
                            Height = currentTab.Height - 5
                        };
                    }

                    gfx.DrawImageUnscaled(img, currentImgRct);
                    img.Dispose();
                }
            }
            else
            {
                if (_alignments == TabAlignments.Top)
                {
                    currentTab = new Rectangle()
                    {
                        X = currentTab.X + 5,
                        Y = currentTab.Y,
                        Width = currentTab.Width - 5,
                        Height = currentTab.Height
                    };
                }
                else
                {
                    currentTab = new Rectangle()
                    {
                        X = currentTab.X + 5,
                        Y = currentTab.Y + 5,
                        Width = currentTab.Width - 5,
                        Height = currentTab.Height - 5
                    };
                }
            }

            //Draw Text on the Tabs
            using (SolidBrush brush = new SolidBrush(this.TabPages[nIndex].Enabled ? (nIndex == this.SelectedIndex ? _tabGradient.TabPageSelectedTextColor : _tabGradient.TabPageTextColor) : Color.Gray))
            {
                using (StringFormat format = new StringFormat(StringFormatFlags.LineLimit))
                {
                    format.Trimming = StringTrimming.EllipsisCharacter;
                    format.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.Show;

                    if (nIndex == this.SelectedIndex)
                    {
                        currentTab.Width -= conditionRectangleArray[0].Width;
                        format.Alignment = StringAlignment.Near;
                    }
                    else
                    {
                        if (conditionBooleanArray[5] || nIndex == 0)
                        {
                            currentTab.X -= 5;
                            currentTab.Width += 5;
                            format.Alignment = StringAlignment.Center;
                        }
                        else
                        {
                            currentTab.X += 8;
                            currentTab.Width -= 8;
                        }
                    }

                    format.LineAlignment = StringAlignment.Center;

                    Font currentFont = this.TabPages[nIndex].Font;
                    if (nIndex == this.SelectedIndex)
                        currentFont = new Font(currentFont, _tabGradient.SelectedTabFontStyle);

                    gfx.DrawString(this.TabPages[nIndex].Text, currentFont, brush, currentTab, format);
                }
            }
        }

        protected virtual void DrawBorder(Graphics gfx)
        {
            Rectangle rct = this.ClientRectangle;

            if (conditionBooleanArray[0] || conditionBooleanArray[6] || this.TabCount == 0
                || (conditionBooleanArray[1] && this.TabCount == 1 && !conditionBooleanArray[4]))
            {
                ButtonBorderStyle borderStyle;
                switch (_borderStyle)
                {
                    case ControlBorderStyle.Dashed:
                        borderStyle = ButtonBorderStyle.Dashed;
                        break;
                    case ControlBorderStyle.Dotted:
                        borderStyle = ButtonBorderStyle.Dotted;
                        break;
                    default:
                        borderStyle = ButtonBorderStyle.Solid;
                        break;
                }

                ControlPaint.DrawBorder(gfx, rct, colorArray[1], borderStyle);
            }
            else
            {
                Rectangle rctTabArea = this.DisplayRectangle;

                using (Pen pen = new Pen(colorArray[1]), pen2 = new Pen(colorArray[2]))
                {
                    DashStyle borderStyle;
                    switch (_borderStyle)
                    {
                        case ControlBorderStyle.Dashed:
                            borderStyle = DashStyle.Dash;
                            pen.DashStyle = borderStyle; pen2.DashStyle = borderStyle;
                            break;
                        case ControlBorderStyle.Dotted:
                            borderStyle = DashStyle.Dot;
                            pen.DashStyle = borderStyle; pen2.DashStyle = borderStyle;
                            break;
                        default:
                            borderStyle = DashStyle.Solid;
                            pen.DashStyle = borderStyle; pen2.DashStyle = borderStyle;
                            break;
                    }

                    if (_alignments == TabAlignments.Top)
                    {
                        if (this.SelectedIndex != 0 || _tabHOffset >= 0)
                            gfx.DrawLine(pen2, rct.X, rctTabArea.Y - 5, rct.X, rctTabArea.Y - 1);
                        else
                        {
                            if (_tabHOffset == -1)
                                gfx.DrawLine(pen2, rct.X, rctTabArea.Y - 1, rct.X + 1, rctTabArea.Y - 1);
                        }
                        gfx.DrawLine(pen, rct.X, rctTabArea.Y, rct.X, rct.Bottom - 1);
                        gfx.DrawLine(pen, rct.X, rct.Bottom - 1, rct.Width - 1, rct.Bottom - 1);
                        gfx.DrawLine(pen, rct.Width - 1, rct.Bottom - 1, rct.Width - 1, rctTabArea.Y);
                        gfx.DrawLine(pen2, rct.Width - 1, rctTabArea.Y - 1, rct.Width - 1, rctTabArea.Y - 5);
                    }
                    else
                    {
                        if (this.SelectedIndex != 0 || _tabHOffset >= 0)
                            gfx.DrawLine(pen2, rct.X, rctTabArea.Bottom + 4, rct.X, rctTabArea.Bottom);
                        else
                        {
                            if (_tabHOffset == -1)
                                gfx.DrawLine(pen2, rct.X, rctTabArea.Bottom, rct.X + 1, rctTabArea.Bottom);
                        }
                        gfx.DrawLine(pen, rct.X, rctTabArea.Bottom - 1, rct.X, rct.Y);
                        gfx.DrawLine(pen, rct.X, rct.Y, rct.Width - 1, rct.Y);
                        gfx.DrawLine(pen, rct.Width - 1, rct.Y, rct.Width - 1, rctTabArea.Bottom - 1);
                        gfx.DrawLine(pen2, rct.Width - 1, rctTabArea.Bottom, rct.Width - 1, rctTabArea.Bottom + 4);
                    }
                }
            }
        }

        protected virtual void DrawCaption(Graphics gfx)
        {
            // If StretchToParent(HeaderVisibility) is false, draw tabcontrol caption.
            if (!conditionBooleanArray[6] && conditionBooleanArray[1])
            {
                Rectangle rct;
                switch (_alignments)
                {
                    case TabAlignments.Top:
                        rct = new Rectangle(DisplayRectangle.Left, DisplayRectangle.Bottom + 1, DisplayRectangle.Width, CAPTIONHEIGHT);
                        break;
                    default:
                        rct = new Rectangle(DisplayRectangle.Left, DisplayRectangle.Top - (CAPTIONHEIGHT + 1), DisplayRectangle.Width, CAPTIONHEIGHT);
                        break;
                }

                if (rct.Width <= 0 || (this.TabCount == 0 ? this.Height <= 17 : DisplayRectangle.Height <= -6))
                    return;

                // Create a new empty image for manipulations. If you use this constructor, you get a new Bitmap object that represents a bitmap in memory with a PixelFormat of Format32bppARGB.
                using (Bitmap overlay = new Bitmap(rct.Width + 1, rct.Height + 1))
                {
                    // Make an associated Graphics object.
                    using (Graphics gr = Graphics.FromImage(overlay))
                    {
                        gr.SmoothingMode = SmoothingMode.HighQuality;

                        // Fill Caption
                        using (LinearGradientBrush brush = (this.Focused || this.ContainsFocus) ? new LinearGradientBrush(new Rectangle(0, 0, overlay.Width, overlay.Height), _gradientCaption.ActiveCaptionColorStart, _gradientCaption.ActiveCaptionColorEnd, _gradientCaption.CaptionGradientStyle)
                            : new LinearGradientBrush(new Rectangle(0, 0, overlay.Width, overlay.Height), _gradientCaption.InactiveCaptionColorStart, _gradientCaption.InactiveCaptionColorEnd, _gradientCaption.CaptionGradientStyle))
                        {
                            Blend bl = new Blend(2);
                            bl.Factors = new float[] { 0.1F, 1.0F };
                            bl.Positions = new float[] { 0.0F, 1.0F };
                            brush.Blend = bl;
                            gr.FillRectangle(brush, 0, 0, overlay.Width, overlay.Height);
                        }
                    }

                    /* Create a new color matrix,
                       The value _captionRandomizer.Transparency in row 4, column 4 specifies the alpha value */
                    float[][] jaggedMatrix = new float[][]
                    {
                        // Red component   [from 0.0 to 1.0 increase red color component.]
                        new float[]{ _captionRandomizer.IsRandomizerEnabled ? _captionRandomizer.Red / 255f : 1.0f , 0.0f , 0.0f , 0.0f , 0.0f },                  
                        // Green component [from 0.0 to 1.0 increase green color component.]
                        new float[]{ 0.0f , _captionRandomizer.IsRandomizerEnabled ? _captionRandomizer.Green / 255f : 1.0f , 0.0f , 0.0f , 0.0f },                
                        // Blue component  [from 0.0 to 1.0 increase blue color component.]
                        new float[]{ 0.0f , 0.0f , _captionRandomizer.IsRandomizerEnabled ? _captionRandomizer.Blue / 255f : 1.0f , 0.0f , 0.0f },                 
                        // Alpha component [from 1.0 to 0.0 increase transparency bitmap.]
                        new float[]{ 0.0f , 0.0f , 0.0f , _captionRandomizer.IsTransparencyEnabled ? _captionRandomizer.Transparency / 255f : 1.0f , 0.0f },       
                        // White component [0.0: goes to Original color, 1.0: goes to white for all color component(Red, Green, Blue.)]
                        new float[]{ _captionRandomizer.IsRandomizerEnabled ? 0.2f : 0.0f , _captionRandomizer.IsRandomizerEnabled ? 0.2f : 0.0f , _captionRandomizer.IsRandomizerEnabled ? 0.2f : 0.0f , 0.0f , 1.0f }                                                                                           
                    };

                    ColorMatrix colorMatrix = new ColorMatrix(jaggedMatrix);

                    // Create an ImageAttributes object and set its color matrix
                    using (ImageAttributes attributes = new ImageAttributes())
                    {
                        attributes.SetColorMatrix(
                            colorMatrix,
                            ColorMatrixFlag.Default,
                            ColorAdjustType.Bitmap);

                        gfx.DrawImage(overlay, rct, 1, 1, overlay.Width - 1, overlay.Height - 1, GraphicsUnit.Pixel, attributes);
                    }
                }

                // Draw Caption Line
                using (Pen captionBorderPen = new Pen(colorArray[1]))
                {
                    DashStyle borderStyle;
                    switch (_borderStyle)
                    {
                        case ControlBorderStyle.Dashed:
                            borderStyle = DashStyle.Dash;
                            captionBorderPen.DashStyle = borderStyle;
                            break;
                        case ControlBorderStyle.Dotted:
                            borderStyle = DashStyle.Dot;
                            captionBorderPen.DashStyle = borderStyle;
                            break;
                        default:
                            borderStyle = DashStyle.Solid;
                            captionBorderPen.DashStyle = borderStyle;
                            break;
                    }

                    gfx.DrawLine(captionBorderPen, rct.Left, _alignments == TabAlignments.Top ? rct.Y - 1 : rct.Bottom, rct.Right - 1, _alignments == TabAlignments.Top ? rct.Y - 1 : rct.Bottom);
                }
                
                Image captionCloseImg = Resources.CaptionClose;
                Image captionDropDownImg = Resources.DropDown;

                using (ImageAttributes attributes = new ImageAttributes())
                {
                    ColorMap[] map = new ColorMap[2];
                    map[0] = new ColorMap();
                    map[0].OldColor = Color.White;
                    map[0].NewColor = Color.Transparent;
                    map[1] = new ColorMap();
                    map[1].OldColor = Color.Black;
                    map[1].NewColor = this.Focused || this.ContainsFocus ? _captionButtons.ActiveCaptionButtonsColor : _captionButtons.InactiveCaptionButtonsColor;

                    attributes.SetRemapTable(map);

                    int iconPos = rct.Y + rct.Height / 2 - 1;
                    if ((this.SelectedTab != null && !((TabPageEx)this.SelectedTab).IsClosable) || this.TabCount <= 0)
                    {
                        conditionRectangleArray[1] = new Rectangle(rct.Right - (3 + captionDropDownImg.Width), iconPos - captionDropDownImg.Height / 2, captionDropDownImg.Width, captionDropDownImg.Height);
                        if (conditionButtonStateArray[1] == ButtonState.Hover)
                        {
                            using (Brush buttonBrush = new SolidBrush(this.Focused || this.ContainsFocus ? Color.DarkBlue : Color.White))
                                gfx.FillRectangle(buttonBrush, conditionRectangleArray[1]);
                        }
                        else if (conditionButtonStateArray[1] == ButtonState.Pressed)
                        {
                            using (Brush buttonBrush = new LinearGradientBrush(conditionRectangleArray[1], Color.DarkBlue, Color.LightBlue, LinearGradientMode.Vertical))
                                gfx.FillRectangle(buttonBrush, conditionRectangleArray[1]);
                        }
                        gfx.DrawImage(captionDropDownImg, conditionRectangleArray[1], 0, 0, captionDropDownImg.Width, captionDropDownImg.Height, GraphicsUnit.Pixel, attributes);
                    }
                    else
                    {
                        conditionRectangleArray[2] = new Rectangle(rct.Right - (3 + captionCloseImg.Width), iconPos - captionCloseImg.Height / 2, captionCloseImg.Width, captionCloseImg.Height);
                        conditionRectangleArray[1] = new Rectangle(conditionRectangleArray[2].Left - (3 + captionDropDownImg.Width), iconPos - captionDropDownImg.Height / 2, captionDropDownImg.Width, captionDropDownImg.Height);

                        if (conditionButtonStateArray[2] == ButtonState.Hover)
                        {
                            using (Brush buttonBrush = new SolidBrush(this.Focused || this.ContainsFocus ? Color.DarkBlue : Color.White))
                                gfx.FillRectangle(buttonBrush, conditionRectangleArray[2]);
                        }
                        else if (conditionButtonStateArray[2] == ButtonState.Pressed)
                        {
                            using (Brush buttonBrush = new LinearGradientBrush(conditionRectangleArray[2], Color.DarkBlue, Color.LightBlue, LinearGradientMode.Vertical))
                                gfx.FillRectangle(buttonBrush, conditionRectangleArray[2]);
                        }

                        if (conditionButtonStateArray[1] == ButtonState.Hover)
                        {
                            using (Brush buttonBrush = new SolidBrush(this.Focused || this.ContainsFocus ? Color.DarkBlue : Color.White))
                                gfx.FillRectangle(buttonBrush, conditionRectangleArray[1]);
                        }
                        else if (conditionButtonStateArray[1] == ButtonState.Pressed)
                        {
                            using (Brush buttonBrush = new LinearGradientBrush(conditionRectangleArray[1], Color.DarkBlue, Color.LightBlue, LinearGradientMode.Vertical))
                                gfx.FillRectangle(buttonBrush, conditionRectangleArray[1]);
                        }

                        gfx.DrawImage(captionCloseImg, conditionRectangleArray[2], 0, 0, captionCloseImg.Width, captionCloseImg.Height, GraphicsUnit.Pixel, attributes);
                        gfx.DrawImage(captionDropDownImg, conditionRectangleArray[1], 0, 0, captionDropDownImg.Width, captionDropDownImg.Height, GraphicsUnit.Pixel, attributes);
                    }
                }

                // Draw String
                rct.Width = conditionRectangleArray[1].Left;
                rct.Inflate(-3, -1);

                if (rct.Width > 0)
                {
                    using (Font captionFont = new System.Drawing.Font(this.Font.Name, this.Font.Size > 12 ? 12 : this.Font.Size, this.Focused || this.ContainsFocus ? _gradientCaption.ActiveCaptionFontStyle : FontStyle.Regular, GraphicsUnit.Pixel))
                    {
                        bool isContainsTabPages = this.SelectedTab != null ? true : false;

                        using (SolidBrush captionTextBrush = new SolidBrush(this.Focused || this.ContainsFocus ? _gradientCaption.ActiveCaptionTextColor : _gradientCaption.InactiveCaptionTextColor))
                        using (StringFormat captionTextFormat = new StringFormat(StringFormatFlags.LineLimit))
                        {

                            if (isContainsTabPages)
                                captionTextFormat.Alignment = StringAlignment.Near;
                            else
                                captionTextFormat.Alignment = StringAlignment.Center;

                            captionTextFormat.Trimming = StringTrimming.EllipsisCharacter;
                            captionTextFormat.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.Hide;

                            gfx.DrawString(isContainsTabPages ? this.SelectedTab.Text : "Tab container is empty, drag or add a new tab page here!", captionFont,
                                captionTextBrush, rct, captionTextFormat);
                        }
                    }
                }
            }
        }

        protected virtual void OnDrawHeaderChanged(EventArgs e)
        {
            if (DrawHeaderChanged != null)
                DrawHeaderChanged(this, e);
        }

        protected virtual void OnCaptionVisibleChanged(EventArgs e)
        {
            if (CaptionVisibleChanged != null)
                CaptionVisibleChanged(this, e);
        }

        protected virtual void OnStretchToParentChanged(EventArgs e)
        {
            if (StretchToParentChanged != null)
                StretchToParentChanged(this, e);
        }

        protected virtual void OnTabPageClosing(SelectedIndexChangingEventArgs e)
        {
            if (TabPageClosing != null)
                TabPageClosing(this, e);
        }

        protected virtual void OnSelectedIndexChanging(SelectedIndexChangingEventArgs e)
        {
            if (SelectedIndexChanging != null)
                SelectedIndexChanging(this, e);
        }

        protected virtual void OnContextMenuShown(ContextMenuShownEventArgs e)
        {
            ToolStripMenuItem menuItem;
            if (this.TabCount > 0)
            {
                menuItem = new ToolStripMenuItem("Close", Resources.delete_12x12, null, Keys.Control | Keys.C);
                menuItem.ImageScaling = ToolStripItemImageScaling.None;
                TabPageEx closingTabPage = (TabPageEx)this.SelectedTab;
                menuItem.Enabled = closingTabPage.IsClosable ? closingTabPage.Enabled : false;
                if (menuItem.Enabled)
                {
                    menuItem.Click += (sender, ea) =>
                        {
                            using (SelectedIndexChangingEventArgs sea = new SelectedIndexChangingEventArgs(closingTabPage, this.SelectedIndex))
                            {
                                // Fire a Notification Event.
                                OnTabPageClosing(sea);

                                if (!sea.Cancel)
                                {
                                    this.TabPages.Remove(closingTabPage);
                                    SelectNextAvailableTabPage();
                                }
                                else
                                    MessageBox.Show("The operation was canceled by the user.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        };
                }

                e.ContextMenu.Items.Add(menuItem);

                if (this.TabCount > 1)
                {
                    menuItem = new ToolStripMenuItem("Close All But This", Resources.PushpinHS, null, Keys.Control | Keys.A);
                    menuItem.Click += (sender, ea) =>
                    {
                        // Iterate All TabPages in the collection.
                        for (int i = this.TabCount - 1; i >= 0; i--)
                        {
                            TabPageEx currentTabPage = this.TabPages[i] as TabPageEx;
                            if (currentTabPage == null || !currentTabPage.IsClosable || i == this.SelectedIndex)
                                continue;

                            using (SelectedIndexChangingEventArgs sea = new SelectedIndexChangingEventArgs(currentTabPage, i))
                            {
                                // Fire a Notification Event.
                                OnTabPageClosing(sea);

                                if (!sea.Cancel)
                                    this.TabPages.Remove(currentTabPage);
                            }
                        }
                    };

                    e.ContextMenu.Items.Add(menuItem);
                    e.ContextMenu.Items.Add(new ToolStripSeparator());
                }
            }

            List<ToolStripMenuItem> availableMenuItems = new List<ToolStripMenuItem>();
            foreach (TabPageEx tab in this.TabPages)
            {
                menuItem = new ToolStripMenuItem(tab.ToString(), this.ImageList != null ? (tab.ImageIndex != -1 ? this.ImageList.Images[tab.ImageIndex] : null) : null, null, tab.Name);
                menuItem.Checked = true;
                menuItem.Click += (sender, ea) =>
                    {
                        this.HideTab((TabPageEx)this.TabPages[((ToolStripItem)sender).Name]);
                    };

                availableMenuItems.Add(menuItem);
            }

            if (this._tabPageExPool != null)
            {
                foreach (TabPageEx tab in this._tabPageExPool)
                {
                    menuItem = new ToolStripMenuItem(tab.ToString(), this.ImageList != null ? (tab.ImageIndex != -1 ? this.ImageList.Images[tab.ImageIndex] : null) : null, null, tab.Name);
                    menuItem.Click += (sender, ea) =>
                    {
                        this.ShowTab(this._tabPageExPool[((ToolStripItem)sender).Name]);
                    };

                    availableMenuItems.Add(menuItem);
                }
            }

            if (availableMenuItems.Count > 0)
            {
                availableMenuItems.Sort((p1, p2) => p1.Text.CompareTo(p2.Text));
                menuItem = new ToolStripMenuItem("Available Tab Pages", Resources.InsertTabControlHS, availableMenuItems.ToArray());
                e.ContextMenu.Items.Add(menuItem);
                availableMenuItems.Clear();
            }

            if (ContextMenuShown != null)
                ContextMenuShown(this, e);
            
            e.ContextMenu.Show(e.MenuLocation);
        }

        #endregion

        #region Helper Methods

        private void SelectNextAvailableTabPage()
        {
            if (this.TabCount <= 1)
                return;

            using (SelectedIndexChangingEventArgs e = new SelectedIndexChangingEventArgs((TabPageEx)SelectedTab, 0))
            {
                // Fire a Notification Event.
                OnSelectedIndexChanging(e);

                if (e.Cancel)
                {
                    for (int i = 1; OnNavigateTabPage(i, false) && i < this.TabCount; i++) ;
                }
            }
        }

        private void CONTROL_INVALIDATE_UPDATE(object sender, EventArgs e)
        {
            Invalidate();
            Update();
        }

        private int ScrollPosition()
        {
            int value = -1;
            
            Rectangle tabRect;
            do
            {
                tabRect = GetTabRect(value + 1);
                value++;
            } while (tabRect.Left < 0 && value < this.TabCount);

            return value;
        }

        private void ScrollRight(object sender, EventArgs e)
        {
            if (this.TabCount <= 1)
                return;
            
            if (GetTabRect(this.TabCount - 1).Right <= this._myScroller.Left)
            {
                _myScroller._rightScroller.Enabled = false;
                return;
            }
            if (!_myScroller._leftScroller.Enabled)
                _myScroller._leftScroller.Enabled = true;

            int scrollPos = Math.Max(0, (ScrollPosition() + 1) * 0x10000);

            User32.SendMessage(this.Handle, (int)User32.Msgs.WM_HSCROLL, (IntPtr)(scrollPos | 0x4), IntPtr.Zero);
            User32.SendMessage(this.Handle, (int)User32.Msgs.WM_HSCROLL, (IntPtr)(scrollPos | 0x8), IntPtr.Zero);

            if (GetTabRect(this.TabCount - 1).Right <= this._myScroller.Left)
                _myScroller._rightScroller.Enabled = false;

            CONTROL_INVALIDATE_UPDATE(sender, e);
        }

        private void ScrollLeft(object sender, EventArgs e)
        {
            if (this.TabCount <= 1)
                return;
            
            if (!_myScroller._rightScroller.Enabled)
                _myScroller._rightScroller.Enabled = true;

            int scrollPos = Math.Max(0, (ScrollPosition() - 1) * 0x10000);

            if (scrollPos == 0)
                _myScroller._leftScroller.Enabled = false;

            User32.SendMessage(this.Handle, (int)User32.Msgs.WM_HSCROLL, (IntPtr)(scrollPos | 0x4), IntPtr.Zero);
            User32.SendMessage(this.Handle, (int)User32.Msgs.WM_HSCROLL, (IntPtr)(scrollPos | 0x8), IntPtr.Zero);

            CONTROL_INVALIDATE_UPDATE(sender, e);
        }

        private Cursor GetCustomCursor(string filename)
        {
            Cursor custom = null;

            IntPtr cPointer;
            try
            {
                cPointer = User32.LoadCursorFromFile(filename);
                if (!IntPtr.Zero.Equals(cPointer))
                    custom = new Cursor(cPointer);
            }
            catch { ;}

            return custom;
        }

        private Bitmap GetCloseTabImage()
        {
            Bitmap bmp;
            switch (conditionButtonStateArray[0])
            {
                case ButtonState.Hover:
                    bmp = Resources.TabHover;
                    break;
                case ButtonState.Pressed:
                    bmp = Resources.TabPressed;
                    break;
                default:
                    bmp = Resources.TabClose;
                    break;
            }

            return bmp;
        }

        private IntPtr FindUpDownControl()
        {
            return User32.FindWindowEx(this.Handle, IntPtr.Zero, "msctls_updown32", "\0");
        }

        #endregion

        #region General Methods

        public void HideTab(TabPageEx TabPage)
        {
            if (_tabPageExPool == null)
                _tabPageExPool = new TabPageExPool();

            if (this.TabPages.Contains(TabPage))
            {
                _tabPageExPool.Add(TabPage);
                this.TabPages.Remove(TabPage);
                SelectNextAvailableTabPage();
            }
        }

        public void ShowTab(TabPageEx TabPage)
        {
            if (_tabPageExPool != null)
            {
                if (_tabPageExPool.Contains(TabPage))
                {
                    this.TabPages.Add(TabPage);
                    _tabPageExPool.Remove(TabPage);
                }
            }
        }

        #endregion

        [Editor(typeof(GradientTabEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(GradientTabConverter))]
        public class GradientTab : IDisposable
        {
            #region Event
            
            /// <summary>
            /// Occurs when the sub properties changed of the TabGradient property.
            /// </summary>
            [Description("Occurs when the sub properties changed of the TabGradient property")]
            public event EventHandler GradientChanged;

            #endregion

            #region Instance Members

            private Color _colorStart = Color.White;
            private Color _colorEnd = Color.Gainsboro;
            private Color _tabPageTextColor = Color.Black;
            private Color _tabPageSelectedTextColor = Color.Black;
            private LinearGradientMode _gradientStyle = LinearGradientMode.Horizontal;
            private FontStyle _selectedTabFontStyle = FontStyle.Regular;

            #endregion

            #region Constructor

            public GradientTab() { }

            public GradientTab(Color first, Color second, LinearGradientMode lnrMode, Color textColor, Color selectedTextColor, FontStyle fontStyle)
            {
                _colorStart = first;
                _colorEnd = second;
                _gradientStyle = lnrMode;
                _tabPageTextColor = textColor;
                _tabPageSelectedTextColor = selectedTextColor;
                _selectedTabFontStyle = fontStyle;
            }

            #endregion

            #region Property

            /// <summary>
            /// Gets or Sets the first gradient color of the selected tab item.
            /// </summary>
            [Description("Gets or Sets the first gradient color of the selected tab item")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(Color), "White")]
            [Browsable(true)]
            public Color ColorStart
            {
                get { return _colorStart; }
                set
                {
                    if (!value.Equals(_colorStart))
                    {
                        _colorStart = value;
                        OnGradientChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Gets or Sets the second gradient color of the selected tab item.
            /// </summary>
            [Description("Gets or Sets the second gradient color of the selected tab item")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(Color), "LightBlue")]
            [Browsable(true)]
            public Color ColorEnd
            {
                get { return _colorEnd; }
                set
                {
                    if (!value.Equals(_colorEnd))
                    {
                        _colorEnd = value;
                        OnGradientChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Gets or Sets the gradient style of the selected tab item.
            /// </summary>
            [Description("Gets or Sets the gradient style of the selected tab item")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(LinearGradientMode), "Horizontal")]
            [Browsable(true)]
            public LinearGradientMode GradientStyle
            {
                get { return _gradientStyle; }
                set
                {
                    if (!value.Equals(_gradientStyle))
                    {
                        _gradientStyle = value;
                        OnGradientChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Gets or Sets the text color of the inactive tab item.
            /// </summary>
            [Description("Gets or Sets the text color of the inactive tab item")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(Color), "Black")]
            [Browsable(true)]
            public Color TabPageTextColor
            {
                get { return _tabPageTextColor; }
                set
                {
                    if (!value.Equals(_tabPageTextColor))
                    {
                        _tabPageTextColor = value;
                        OnGradientChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Gets or Sets the text color of the selected tab item.
            /// </summary>
            [Description("Gets or Sets the text color of the selected tab item")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(Color), "Black")]
            [Browsable(true)]
            public Color TabPageSelectedTextColor
            {
                get { return _tabPageSelectedTextColor; }
                set
                {
                    if (!value.Equals(_tabPageSelectedTextColor))
                    {
                        _tabPageSelectedTextColor = value;
                        OnGradientChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Gets or Sets the font style of the selected tab item.
            /// </summary>
            [Description("Gets or Sets the font style of the selected tab item")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(FontStyle), "Regular")]
            [Browsable(true)]
            public FontStyle SelectedTabFontStyle
            {
                get { return _selectedTabFontStyle; }
                set
                {
                    if (!value.Equals(_selectedTabFontStyle))
                    {
                        _selectedTabFontStyle = value;
                        OnGradientChanged(EventArgs.Empty);
                    }
                }
            }

            #endregion

            #region Virtual Methods

            protected virtual void OnGradientChanged(EventArgs e)
            {
                if (GradientChanged != null)
                    GradientChanged(this, e);
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            #endregion
        }

        class GradientTabConverter : ExpandableObjectConverter
        {
            #region Destructor

            ~GradientTabConverter()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Override Methods

            //All the CanConvertTo() method needs to is check that the target type is a string.
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return true;
                else
                    return base.CanConvertTo(context, destinationType);
            }

            //ConvertTo() simply checks that it can indeed convert to the desired type.
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return ToString(value);
                else
                    return base.ConvertTo(context, culture, value, destinationType);
            }

            /* The exact same process occurs in reverse when converting a GradientTab object to a string.
            First the Properties window calls CanConvertFrom(). If it returns true, the next step is to call
            the ConvertFrom() method. */
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;
                else
                    return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is string)
                    return FromString(value);
                else
                    return base.ConvertFrom(context, culture, value);
            }

            #endregion

            #region Helper Methods

            private string ToString(object value)
            {
                GradientTab gradient = value as GradientTab;    // Gelen object tipimizi GradientTab tipine dönüştürüyoruz ve ayıklama işlemine başlıyoruz.
                ColorConverter converter = new ColorConverter();
                return String.Format("{0}, {1}, {2}, {3}, {4}, {5}",
                    converter.ConvertToString(gradient.ColorStart), converter.ConvertToString(gradient.ColorEnd), gradient.GradientStyle, converter.ConvertToString(gradient.TabPageTextColor), converter.ConvertToString(gradient.TabPageSelectedTextColor), gradient.SelectedTabFontStyle);
            }

            private GradientTab FromString(object value)
            {
                string[] result = ((string)value).Split(',');
                if (result.Length != 6)
                    throw new ArgumentException("Could not convert to value");

                try
                {
                    GradientTab gradient = new GradientTab();

                    // Retrieve the colors
                    ColorConverter converter = new ColorConverter();
                    gradient.ColorStart = (Color)converter.ConvertFromString(result[0]);
                    gradient.ColorEnd = (Color)converter.ConvertFromString(result[1]);
                    gradient.GradientStyle = (LinearGradientMode)Enum.Parse(typeof(LinearGradientMode), result[2], true);
                    gradient.TabPageTextColor = (Color)converter.ConvertFromString(result[3]);
                    gradient.TabPageSelectedTextColor = (Color)converter.ConvertFromString(result[4]);
                    gradient.SelectedTabFontStyle = (FontStyle)Enum.Parse(typeof(FontStyle), result[5], true);

                    return gradient;
                }
                catch (Exception)
                {
                    throw new ArgumentException("Could not convert to value");
                }
            }

            #endregion
        }

        class GradientTabEditor : UITypeEditor
        {
            #region Destructor

            ~GradientTabEditor()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Override Methods

            public override bool GetPaintValueSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override void PaintValue(PaintValueEventArgs e)
            {
                GradientTab gradient = e.Value as GradientTab;
                using (LinearGradientBrush brush = new LinearGradientBrush(e.Bounds, gradient.ColorStart, gradient.ColorEnd, gradient.GradientStyle))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
            }

            #endregion
        }

        [Editor(typeof(HatcherEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(HatcherConverter))]
        public class Hatcher : IDisposable
        {
            #region Event

            /// <summary>
            /// Occurs when the sub properties changed of the BackgroundHatcher property.
            /// </summary>
            [Description("Occurs when the sub properties changed of the BackgroundHatcher property")]
            public event EventHandler HatchChanged;

            #endregion

            #region Instance Members

            private Color _foreColor = Color.White;
            private Color _backColor = Color.Gainsboro;
            private System.Drawing.Drawing2D.HatchStyle _hatchType = System.Drawing.Drawing2D.HatchStyle.DashedVertical;
            private string _hatchStyle = "DashedVertical";

            #endregion

            #region Constructor

            public Hatcher() { }

            public Hatcher(Color foreColor, Color backColor, HatchStyle type)
            {
                _foreColor = foreColor;
                _backColor = backColor;
                _hatchType = type;
            }

            #endregion

            #region Property

            /// <summary>
            /// Gets or Sets the fore-background color of the tab pages background area.
            /// </summary>
            [Description("Gets or Sets the fore-background color of the tab pages background area")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(Color), "White")]
            [Browsable(true)]
            public Color ForeColor
            {
                get { return _foreColor; }
                set
                {
                    if (!value.Equals(_foreColor))
                    {
                        _foreColor = value;
                        OnHatchChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Gets or Sets the back-background color of the tab pages background area.
            /// </summary>
            [Description("Gets or Sets the back-background color of the tab pages background area")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(Color), "Gainsboro")]
            [Browsable(true)]
            public Color BackColor
            {
                get { return _backColor; }
                set
                {
                    if (!value.Equals(_backColor))
                    {
                        _backColor = value;
                        OnHatchChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Hatch style.
            /// </summary>
            [Browsable(false)]
            public System.Drawing.Drawing2D.HatchStyle HatchType
            {
                get { return _hatchType; }
                set
                {
                    if (!value.Equals(_hatchType))
                    {
                        _hatchType = value;
                        OnHatchChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Gets or Sets the hatch style of the background area.
            /// </summary>
            [Description("Gets or Sets the hatch style of the background area")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue("DashedVertical")]
            [Browsable(true)]
            [TypeConverter(typeof(HatchStyleConverter))]
            public string HatchStyle
            {
                get { return _hatchStyle; }
                set
                {
                    if (!value.Equals(_hatchStyle))
                    {
                        _hatchStyle = value;
                        _hatchType = (HatchStyle)Enum.Parse(typeof(HatchStyle), _hatchStyle, true);
                        OnHatchChanged(EventArgs.Empty);
                    }
                }
            }

            #endregion

            #region Virtual Methods

            protected virtual void OnHatchChanged(EventArgs e)
            {
                if (HatchChanged != null)
                    HatchChanged(this, e);
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            #endregion
        }

        class HatcherConverter : ExpandableObjectConverter
        {
            #region Destructor

            ~HatcherConverter()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Override Methods

            //All the CanConvertTo() method needs to is check that the target type is a string.
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return true;
                else
                    return base.CanConvertTo(context, destinationType);
            }

            //ConvertTo() simply checks that it can indeed convert to the desired type.
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return ToString(value);
                else
                    return base.ConvertTo(context, culture, value, destinationType);
            }

            /* The exact same process occurs in reverse when converting a Hatcher object to a string.
            First the Properties window calls CanConvertFrom(). If it returns true, the next step is to call
            the ConvertFrom() method. */
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;
                else
                    return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is string)
                    return FromString(value);
                else
                    return base.ConvertFrom(context, culture, value);
            }

            #endregion

            #region Helper Methods

            private string ToString(object value)
            {
                Hatcher hatch = value as Hatcher;    // Gelen object tipimizi Hatcher tipine dönüştürüyoruz ve ayıklama işlemine başlıyoruz.

                ColorConverter converter = new ColorConverter();

                return String.Format("{0}, {1}, {2}",
                    converter.ConvertToString(hatch.ForeColor), converter.ConvertToString(hatch.BackColor), hatch.HatchStyle);
            }

            private Hatcher FromString(object value)
            {
                string[] result = ((string)value).Split(',');
                if (result.Length != 3)
                    throw new ArgumentException("Could not convert to value");

                try
                {
                    Hatcher hatch = new Hatcher();

                    // Retrieve the colors
                    ColorConverter converter = new ColorConverter();

                    hatch.ForeColor = (Color)converter.ConvertFromString(result[0]);
                    hatch.BackColor = (Color)converter.ConvertFromString(result[1]);
                    hatch.HatchStyle = result[2];

                    return hatch;
                }
                catch (Exception)
                {
                    throw new ArgumentException("Could not convert to value");
                }
            }

            #endregion
        }

        class HatcherEditor : UITypeEditor
        {
            #region Destructor

            ~HatcherEditor()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Override Methods

            public override bool GetPaintValueSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override void PaintValue(PaintValueEventArgs e)
            {
                Hatcher hatch = e.Value as Hatcher;
                using (HatchBrush brush = new HatchBrush(hatch.HatchType, hatch.ForeColor, hatch.BackColor))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
            }

            #endregion
        }

        class HatchStyleConverter : StringConverter // Bug! enumeration of HatchStyle, multiple values at the result.
        {
            #region Destructor

            ~HatchStyleConverter()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Instance Members

            // Cache the collection of values so you don't need to re-create it each time.
            private static StandardValuesCollection _collection;

            #endregion

            #region Override Methods

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;    // PropertyGrid shows a Combobox to us.
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return true;    // If you select the true,PropertyGrid  does not allow change the value.It means Select Only.
            }

            // Provide the list of HatchStyle standart values.
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                if (_collection == null)
                {
                    List<string> hatchStyle = new List<string>();

                    foreach (string current in Enum.GetNames(typeof(HatchStyle)))
                    {
                        hatchStyle.Add(current);
                    }
                    // Now wrap the real values in the StandarValuesCollection object.
                    _collection = new TypeConverter.StandardValuesCollection(hatchStyle);
                }

                return _collection;
            }

            #endregion
        }

        [Editor(typeof(CaptionGradientEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(CaptionGradientConverter))]
        public class CaptionGradient : IDisposable
        {
            #region Event

            /// <summary>
            /// Occurs when the sub properties changed of the GradientCaption property.
            /// </summary>
            [Description("Occurs when the sub properties changed of the GradientCaption property")]
            public event EventHandler GradientChanged;

            #endregion

            #region Instance Members

            private Color _activeCaptionColorStart = SystemColors.GradientActiveCaption;
            private Color _activeCaptionColorEnd = SystemColors.ActiveCaption;
            private Color _inactiveCaptionColorStart = SystemColors.GradientInactiveCaption;
            private Color _inactiveCaptionColorEnd = SystemColors.InactiveCaption;
            private Color _activeCaptionTextColor = SystemColors.ActiveCaptionText;
            private Color _inactiveCaptionTextColor = SystemColors.InactiveCaptionText;
            private LinearGradientMode _captionGradientStyle = LinearGradientMode.Vertical;
            private FontStyle _activeCaptionFontStyle = FontStyle.Regular;

            #endregion

            #region Constructor

            public CaptionGradient() { }

            public CaptionGradient(Color firstActive, Color secondActive, Color firstInactive, Color secondInactive, LinearGradientMode lnrMode,
                Color activeTextColor, Color inactiveTextColor, FontStyle fontStyle)
            {
                _activeCaptionColorStart = firstActive;
                _activeCaptionColorEnd = secondActive;
                _inactiveCaptionColorStart = firstInactive;
                _inactiveCaptionColorEnd = secondInactive;
                _captionGradientStyle = lnrMode;
                _activeCaptionTextColor = activeTextColor;
                _inactiveCaptionTextColor = inactiveTextColor;
                _activeCaptionFontStyle = fontStyle;
            }

            #endregion

            #region Property

            /// <summary>
            /// First Active Caption Gradient Color.
            /// </summary>
            [Description("First Active Caption Gradient Color")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(Color), "GradientActiveCaption")]
            [Browsable(true)]
            public Color ActiveCaptionColorStart
            {
                get { return _activeCaptionColorStart; }
                set
                {
                    if (!value.Equals(_activeCaptionColorStart))
                    {
                        _activeCaptionColorStart = value;
                        OnGradientChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Second Active Caption Gradient Color.
            /// </summary>
            [Description("Second Active Caption Gradient Color")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(Color), "ActiveCaption")]
            [Browsable(true)]
            public Color ActiveCaptionColorEnd
            {
                get { return _activeCaptionColorEnd; }
                set
                {
                    if (!value.Equals(_activeCaptionColorEnd))
                    {
                        _activeCaptionColorEnd = value;
                        OnGradientChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// First Inactive Caption Gradient Color.
            /// </summary>
            [Description("First Inactive Caption Gradient Color")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(Color), "GradientInactiveCaption")]
            [Browsable(true)]
            public Color InactiveCaptionColorStart
            {
                get { return _inactiveCaptionColorStart; }
                set
                {
                    if (!value.Equals(_inactiveCaptionColorStart))
                    {
                        _inactiveCaptionColorStart = value;
                        OnGradientChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Second Inactive Caption Gradient Color.
            /// </summary>
            [Description("Second Inactive Caption Gradient Color")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(Color), "InactiveCaption")]
            [Browsable(true)]
            public Color InactiveCaptionColorEnd
            {
                get { return _inactiveCaptionColorEnd; }
                set
                {
                    if (!value.Equals(_inactiveCaptionColorEnd))
                    {
                        _inactiveCaptionColorEnd = value;
                        OnGradientChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Caption Gradient Style.
            /// </summary>
            [Description("Caption Gradient Style")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(LinearGradientMode), "Vertical")]
            [Browsable(true)]
            public LinearGradientMode CaptionGradientStyle
            {
                get { return _captionGradientStyle; }
                set
                {
                    if (!value.Equals(_captionGradientStyle))
                    {
                        _captionGradientStyle = value;
                        OnGradientChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Inactive Caption Text Color.
            /// </summary>
            [Description("Inactive Caption Text Color")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(Color), "InactiveCaptionText")]
            [Browsable(true)]
            public Color InactiveCaptionTextColor
            {
                get { return _inactiveCaptionTextColor; }
                set
                {
                    if (!value.Equals(_inactiveCaptionTextColor))
                    {
                        _inactiveCaptionTextColor = value;
                        OnGradientChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Active Caption Text Color.
            /// </summary>
            [Description("Active Caption Text Color")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(Color), "ActiveCaptionText")]
            [Browsable(true)]
            public Color ActiveCaptionTextColor
            {
                get { return _activeCaptionTextColor; }
                set
                {
                    if (!value.Equals(_activeCaptionTextColor))
                    {
                        _activeCaptionTextColor = value;
                        OnGradientChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Active Caption Font Style.
            /// </summary>
            [Description("Active Caption Font Style")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(FontStyle), "Regular")]
            [Browsable(true)]
            public FontStyle ActiveCaptionFontStyle
            {
                get { return _activeCaptionFontStyle; }
                set
                {
                    if (!value.Equals(_activeCaptionFontStyle))
                    {
                        _activeCaptionFontStyle = value;
                        OnGradientChanged(EventArgs.Empty);
                    }
                }
            }

            #endregion

            #region Virtual Methods

            protected virtual void OnGradientChanged(EventArgs e)
            {
                if (GradientChanged != null)
                    GradientChanged(this, e);
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            #endregion
        }

        class CaptionGradientConverter : ExpandableObjectConverter
        {
            #region Destructor

            ~CaptionGradientConverter()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Override Methods

            //All the CanConvertTo() method needs to is check that the target type is a string.
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return true;
                else
                    return base.CanConvertTo(context, destinationType);
            }

            //ConvertTo() simply checks that it can indeed convert to the desired type.
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return ToString(value);
                else
                    return base.ConvertTo(context, culture, value, destinationType);
            }

            /* The exact same process occurs in reverse when converting a CaptionGradient object to a string.
            First the Properties window calls CanConvertFrom(). If it returns true, the next step is to call
            the ConvertFrom() method. */
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;
                else
                    return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is string)
                    return FromString(value);
                else
                    return base.ConvertFrom(context, culture, value);
            }

            #endregion

            #region Helper Methods

            private string ToString(object value)
            {
                CaptionGradient gradient = value as CaptionGradient;    // Gelen object tipimizi CaptionGradient tipine dönüştürüyoruz ve ayıklama işlemine başlıyoruz.
                ColorConverter converter = new ColorConverter();
                return String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}",
                    converter.ConvertToString(gradient.ActiveCaptionColorStart), converter.ConvertToString(gradient.ActiveCaptionColorEnd),
                    converter.ConvertToString(gradient.InactiveCaptionColorStart), converter.ConvertToString(gradient.InactiveCaptionColorEnd), gradient.CaptionGradientStyle,
                    converter.ConvertToString(gradient.InactiveCaptionTextColor), converter.ConvertToString(gradient.ActiveCaptionTextColor), gradient.ActiveCaptionFontStyle);
            }

            private CaptionGradient FromString(object value)
            {
                string[] result = ((string)value).Split(',');
                if (result.Length != 8)
                    throw new ArgumentException("Could not convert to value");

                try
                {
                    CaptionGradient gradient = new CaptionGradient();

                    // Retrieve the colors
                    ColorConverter converter = new ColorConverter();
                    gradient.ActiveCaptionColorStart = (Color)converter.ConvertFromString(result[0]);
                    gradient.ActiveCaptionColorEnd = (Color)converter.ConvertFromString(result[1]);
                    gradient.InactiveCaptionColorStart = (Color)converter.ConvertFromString(result[2]);
                    gradient.InactiveCaptionColorEnd = (Color)converter.ConvertFromString(result[3]);
                    gradient.CaptionGradientStyle = (LinearGradientMode)Enum.Parse(typeof(LinearGradientMode), result[4], true);
                    gradient.InactiveCaptionTextColor = (Color)converter.ConvertFromString(result[5]);
                    gradient.ActiveCaptionTextColor = (Color)converter.ConvertFromString(result[6]);
                    gradient.ActiveCaptionFontStyle = (FontStyle)Enum.Parse(typeof(FontStyle), result[7], true);

                    return gradient;
                }
                catch (Exception)
                {
                    throw new ArgumentException("Could not convert to value");
                }
            }

            #endregion
        }

        class CaptionGradientEditor : UITypeEditor
        {
            #region Destructor

            ~CaptionGradientEditor()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Override Methods

            public override bool GetPaintValueSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override void PaintValue(PaintValueEventArgs e)
            {
                CaptionGradient gradient = e.Value as CaptionGradient;
                using (LinearGradientBrush brush = new LinearGradientBrush(e.Bounds, gradient.InactiveCaptionColorStart, gradient.InactiveCaptionColorEnd, gradient.CaptionGradientStyle))
                {
                    Blend bl = new Blend(2);
                    bl.Factors = new float[] { 0.1F, 1.0F };
                    bl.Positions = new float[] { 0.0F, 1.0F };
                    brush.Blend = bl;
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
            }

            #endregion
        }

        [Editor(typeof(ButtonsCaptionEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ButtonsCaptionConverter))]
        public class ButtonsCaption : IDisposable
        {
            #region Event

            /// <summary>
            /// Occurs when the sub properties changed of the CaptionButtons property.
            /// </summary>
            [Description("Occurs when the sub properties changed of the CaptionButtons property")]
            public event EventHandler ButtonsColorChanged;

            #endregion

            #region Instance Members

            private Color _activeCaptionButtonsColor = Color.White;
            private Color _inactiveCaptionButtonsColor = Color.Black;

            #endregion

            #region Constructor

            public ButtonsCaption() { }

            public ButtonsCaption(Color activeColor, Color inactiveColor)
            {
                _activeCaptionButtonsColor = activeColor;
                _inactiveCaptionButtonsColor = inactiveColor;
            }

            #endregion

            #region Property

            /// <summary>
            /// Active Caption Buttons Color.
            /// </summary>
            [Description("Active Caption Buttons Color")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(Color), "White")]
            [Browsable(true)]
            public Color ActiveCaptionButtonsColor
            {
                get { return _activeCaptionButtonsColor; }
                set
                {
                    if (!value.Equals(_activeCaptionButtonsColor))
                    {
                        _activeCaptionButtonsColor = value;
                        OnButtonsColorChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Inactive Caption Buttons Color.
            /// </summary>
            [Description("Inactive Caption Buttons Color")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(Color), "Black")]
            [Browsable(true)]
            public Color InactiveCaptionButtonsColor
            {
                get { return _inactiveCaptionButtonsColor; }
                set
                {
                    if (!value.Equals(_inactiveCaptionButtonsColor))
                    {
                        _inactiveCaptionButtonsColor = value;
                        OnButtonsColorChanged(EventArgs.Empty);
                    }
                }
            }

            #endregion

            #region Virtual Methods

            protected virtual void OnButtonsColorChanged(EventArgs e)
            {
                if (ButtonsColorChanged != null)
                    ButtonsColorChanged(this, e);
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            #endregion
        }

        class ButtonsCaptionConverter : ExpandableObjectConverter
        {
            #region Destructor

            ~ButtonsCaptionConverter()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Override Methods

            //All the CanConvertTo() method needs to is check that the target type is a string.
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return true;
                else
                    return base.CanConvertTo(context, destinationType);
            }

            //ConvertTo() simply checks that it can indeed convert to the desired type.
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return ToString(value);
                else
                    return base.ConvertTo(context, culture, value, destinationType);
            }

            /* The exact same process occurs in reverse when converting a ButtonsCaption object to a string.
            First the Properties window calls CanConvertFrom(). If it returns true, the next step is to call
            the ConvertFrom() method. */
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;
                else
                    return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is string)
                    return FromString(value);
                else
                    return base.ConvertFrom(context, culture, value);
            }

            #endregion

            #region Helper Methods

            private string ToString(object value)
            {
                ButtonsCaption buttons = value as ButtonsCaption;    // Gelen object tipimizi ButtonsCaption tipine dönüştürüyoruz ve ayıklama işlemine başlıyoruz.
                ColorConverter converter = new ColorConverter();
                return String.Format("{0}, {1}",
                    converter.ConvertToString(buttons.ActiveCaptionButtonsColor), converter.ConvertToString(buttons.InactiveCaptionButtonsColor));
            }

            private ButtonsCaption FromString(object value)
            {
                string[] result = ((string)value).Split(',');
                if (result.Length != 2)
                    throw new ArgumentException("Could not convert to value");

                try
                {
                    ButtonsCaption buttons = new ButtonsCaption();

                    // Retrieve the colors
                    ColorConverter converter = new ColorConverter();
                    buttons.ActiveCaptionButtonsColor = (Color)converter.ConvertFromString(result[0]);
                    buttons.InactiveCaptionButtonsColor = (Color)converter.ConvertFromString(result[1]);

                    return buttons;
                }
                catch (Exception)
                {
                    throw new ArgumentException("Could not convert to value");
                }
            }

            #endregion
        }

        class ButtonsCaptionEditor : UITypeEditor
        {
            #region Destructor

            ~ButtonsCaptionEditor()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Override Methods

            public override bool GetPaintValueSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override void PaintValue(PaintValueEventArgs e)
            {
                if (e.Context.Instance is KRBTabControl)
                {
                    KRBTabControl parent = e.Context.Instance as KRBTabControl;
                    ButtonsCaption caption = e.Value as ButtonsCaption;

                    using (LinearGradientBrush brush = new LinearGradientBrush(e.Bounds, parent.GradientCaption.InactiveCaptionColorStart, parent.GradientCaption.InactiveCaptionColorEnd, parent.GradientCaption.CaptionGradientStyle))
                    {
                        Blend bl = new Blend(2);
                        bl.Factors = new float[] { 0.1F, 1.0F };
                        bl.Positions = new float[] { 0.0F, 1.0F };
                        brush.Blend = bl;
                        e.Graphics.FillRectangle(brush, e.Bounds);

                        Image captionDropDown = Resources.DropDown;
                        using (ImageAttributes attributes = new ImageAttributes())
                        {
                            ColorMap[] map = new ColorMap[2];
                            map[0] = new ColorMap();
                            map[0].OldColor = Color.White;
                            map[0].NewColor = Color.Transparent;
                            map[1] = new ColorMap();
                            map[1].OldColor = Color.Black;
                            map[1].NewColor = caption.InactiveCaptionButtonsColor;

                            attributes.SetRemapTable(map);

                            Rectangle rct = e.Bounds;
                            rct.Inflate(-3, 0);
                            e.Graphics.DrawImage(captionDropDown, rct, 0, 0, captionDropDown.Width, captionDropDown.Height, GraphicsUnit.Pixel, attributes);
                        }
                    }
                }
            }

            #endregion
        }

        [Editor(typeof(CaptionColorChooserEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(RandomizerCaptionConverter))]
        public class RandomizerCaption : ICaptionRandomizer
        {
            #region Event

            /// <summary>
            /// Occurs when the sub properties changed of the CaptionRandomizer property.
            /// </summary>
            [Description("Occurs when the sub properties changed of the CaptionRandomizer property")]
            public event EventHandler CaptionRandomizerChanged;

            #endregion

            #region Instance Members

            // Red, Green, Blue, Alpha
            private byte[] rgba = { 180, 180, 180, 180 };
            // IsRandomizerEnabled, IsTransparencyEnabled
            private bool[] options = { false, false };

            #endregion

            #region Constructor

            public RandomizerCaption() { }

            public RandomizerCaption(byte red, byte green, byte blue, byte transparency,
                bool isRandomizerEnabled, bool isTransparencyEnabled)
            {
                // Sets RGBA
                rgba[0] = red;
                rgba[1] = green;
                rgba[2] = blue;
                rgba[3] = transparency;

                // Sets Options
                options[0] = isRandomizerEnabled;
                options[1] = isTransparencyEnabled;
            }

            #endregion

            #region Virtual Methods

            protected virtual void OnCaptionRandomizerChanged(EventArgs e)
            {
                if (CaptionRandomizerChanged != null)
                    CaptionRandomizerChanged(this, e);
            }

            #endregion

            #region ICaptionRandomizer Members

            /// <summary>
            /// Determines whether the randomizer effect is enable or not for tab control caption.
            /// </summary>
            [Description("Determines whether the randomizer effect is enable or not for tab control caption")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(false)]
            [Browsable(true)]
            public bool IsRandomizerEnabled
            {
                get { return options[0]; }
                set
                {
                    if (!value.Equals(options[0]))
                    {
                        options[0] = value;
                        OnCaptionRandomizerChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Determines whether the transparency effect is visible or not for tab control caption.
            /// </summary>
            [Description("Determines whether the transparency effect is visible or not for tab control caption")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(false)]
            [Browsable(true)]
            public bool IsTransparencyEnabled
            {
                get { return options[1]; }
                set
                {
                    if (!value.Equals(options[1]))
                    {
                        options[1] = value;
                        OnCaptionRandomizerChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// The red color component property must be in the range of 0 to 255.
            /// </summary>
            [Description("The red color component property must be in the range of 0 to 255")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(Byte), "180")]
            [Browsable(true)]
            public byte Red
            {
                get { return rgba[0]; }
                set
                {
                    if (!value.Equals(rgba[0]))
                    {
                        rgba[0] = value;

                        if (IsRandomizerEnabled)
                            OnCaptionRandomizerChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// The green color component property must be in the range of 0 to 255.
            /// </summary>
            [Description("The green color component property must be in the range of 0 to 255")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(Byte), "180")]
            [Browsable(true)]
            public byte Green
            {
                get { return rgba[1]; }
                set
                {
                    if (!value.Equals(rgba[1]))
                    {
                        rgba[1] = value;

                        if (IsRandomizerEnabled)
                            OnCaptionRandomizerChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// The blue color component property must be in the range of 0 to 255.
            /// </summary>
            [Description("The blue color component property must be in the range of 0 to 255")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(Byte), "180")]
            [Browsable(true)]
            public byte Blue
            {
                get { return rgba[2]; }
                set
                {
                    if (!value.Equals(rgba[2]))
                    {
                        rgba[2] = value;

                        if (IsRandomizerEnabled)
                            OnCaptionRandomizerChanged(EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// This property must be in the range of 50 to 255.
            /// </summary>
            [Description("This property must be in the range of 50 to 255")]
            [RefreshProperties(RefreshProperties.Repaint)]
            [NotifyParentProperty(true)]
            [DefaultValue(typeof(Byte), "180")]
            [Browsable(true)]
            public byte Transparency
            {
                get { return rgba[3]; }
                set
                {
                    if (!value.Equals(rgba[3]))
                    {
                        if (value < 50)
                            value = 50;

                        rgba[3] = value;

                        if (IsTransparencyEnabled)
                            OnCaptionRandomizerChanged(EventArgs.Empty);
                    }
                }
            }

            #endregion
            
            #region IDisposable Members

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            #endregion
        }

        class RandomizerCaptionConverter : ExpandableObjectConverter
        {
            #region Destructor

            ~RandomizerCaptionConverter()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Override Methods

            //All the CanConvertTo() method needs to is check that the target type is a string.
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return true;
                else
                    return base.CanConvertTo(context, destinationType);
            }

            //ConvertTo() simply checks that it can indeed convert to the desired type.
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return ToString(value);
                else
                    return base.ConvertTo(context, culture, value, destinationType);
            }

            /* The exact same process occurs in reverse when converting a RandomizerCaption object to a string.
            First the Properties window calls CanConvertFrom(). If it returns true, the next step is to call
            the ConvertFrom() method. */
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;
                else
                    return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is string)
                    return FromString(value);
                else
                    return base.ConvertFrom(context, culture, value);
            }

            #endregion

            #region Helper Methods

            private string ToString(object value)
            {
                // Gelen object tipimizi ICaptionRandomizer tipine cast ediyoruz ve ayıklama işlemine başlıyoruz.
                ICaptionRandomizer randomizer = value as ICaptionRandomizer;

                return String.Format("{0}, {1}, {2}, {3}, {4}, {5}",
                    randomizer.Red, randomizer.Green, randomizer.Blue, randomizer.Transparency,
                    randomizer.IsRandomizerEnabled, randomizer.IsTransparencyEnabled);
            }

            private RandomizerCaption FromString(object value)
            {
                string[] result = ((string)value).Split(',');
                if (result.Length != 6)
                    throw new ArgumentException("Could not convert to value");

                try
                {
                    RandomizerCaption randomizer = new RandomizerCaption();

                    ByteConverter byteConverter = new ByteConverter();
                    BooleanConverter booleanConverter = new BooleanConverter();

                    // Retrieve the values of the object.
                    randomizer.Red = (byte)byteConverter.ConvertFromString(result[0]);
                    randomizer.Green = (byte)byteConverter.ConvertFromString(result[1]);
                    randomizer.Blue = (byte)byteConverter.ConvertFromString(result[2]);
                    randomizer.Transparency = (byte)byteConverter.ConvertFromString(result[3]);
                    randomizer.IsRandomizerEnabled = (bool)booleanConverter.ConvertFromString(result[4]);
                    randomizer.IsTransparencyEnabled = (bool)booleanConverter.ConvertFromString(result[5]);

                    return randomizer;
                }
                catch (Exception)
                {
                    throw new ArgumentException("Could not convert to value");
                }
            }

            #endregion
        }

        class TabpageExCollectionEditor : CollectionEditor
        {
            #region Constructor

            public TabpageExCollectionEditor(Type type)
                : base(type) { }

            #endregion

            #region Destructor

            ~TabpageExCollectionEditor()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Override Methods

            protected override Type CreateCollectionItemType()
            {
                return typeof(TabPageEx);
            }

            protected override void DestroyInstance(object instance)
            {
                base.DestroyInstance(instance);

                ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
                selectionService.SetSelectedComponents(new IComponent[] { (KRBTabControl)Context.Instance }, SelectionTypes.Auto);
            }

            #endregion
        }

        class TabPageExPool : System.Collections.CollectionBase, IDisposable
        {
            #region Indexer

            public TabPageEx this[int index]
            {
                get
                {
                    return (TabPageEx)this.List[index];
                }
                set
                {
                    this.List[index] = value;
                }
            }

            public TabPageEx this[string name]
            {
                get
                {
                    foreach (TabPageEx tabPage in this)
                    {
                        if (tabPage.Name.Equals(name))
                            return tabPage;
                    }
                    
                    return null;
                }
            }

            #endregion

            #region General Methods

            public void Add(TabPageEx TabPage)
            {
                if (this.List.Contains(TabPage))
                    throw new ArgumentException("Bu tab zaten var.");
                else
                    this.List.Add(TabPage);
            }

            public void Remove(TabPageEx TabPage)
            {
                if (this.List.Contains(TabPage))
                    this.List.Remove(TabPage);
            }

            public bool Contains(TabPageEx TabPage)
            {
                if (this.List.Contains(TabPage))
                    return true;
                else
                    return false;
            }

            public int IndexOf(TabPageEx TabPage)
            {
                return this.List.IndexOf(TabPage);
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                foreach (TabPageEx tab in this)
                    tab.Dispose();

                GC.SuppressFinalize(this);
            }

            #endregion
        }

        class Custom3DBorder : IDisposable
        {
            #region Instance Members

            /// <summary>
            /// This is a private member variable that contains the associated
            /// Graphics object. It is not part of the programming interface of
            /// the Custom3DBorder class.
            /// </summary>
            private Graphics _graphics;

            #endregion

            #region Constructor

            /// <summary>
            /// This constructor initializes a new instance of the Custom3DBorder class with the
            /// specified Graphics object. Grid points are based on a grid where the lines
            /// are between the pixels, instead of through the centers of the pixels.
            /// </summary>
            /// <param name="graphics">Graphics object that is the drawing surface on which
            /// this Custom3DBorder object draws.</param>
            public Custom3DBorder(Graphics graphics)
            {
                _graphics = graphics;
            }

            #endregion

            #region Helper Methods

            /// <summary>
            /// Draws a line connecting the two points specified by the coordinate pairs. If the line
            /// is horizontal, the pixels are drawn just below the specified grid line. If the line is
            /// vertical, the pixels are drawn just to the right of the the specified grid line. If the
            /// line is diagonal, the pixels are drawn from corner to corner of a rectangle where the
            /// corners are made up of the two points. The pixels will fall just inside the rectangle.
            /// </summary>
            /// <param name="pen">Pen object that determines the color, width, and style of the line.</param>
            /// <param name="p1">Point structure that specifies the first grid point to connect.</param>
            /// <param name="p2">Point structure that specifies the second grid point to connect.</param>
            private void DrawLine(Pen pen, Point p1, Point p2)
            {
                Point pp1 = new Point(p1.X, p1.Y);
                Point pp2 = new Point(p2.X, p2.Y);

                // to simplify the modifications that we have to make
                // if the second point is to the left of the first point,
                // switch the two points.
                if (pp2.X < pp1.X)
                {
                    Point tempPoint = pp2;
                    pp2 = pp1;
                    pp1 = tempPoint;
                }

                if (pp1.Y == pp2.Y)  // if line is horizontal
                {
                    pp2.X--;
                }
                else if (pp1.X == pp2.X)  // if line is vertical
                {
                    pp2.Y--;
                }
                else if (pp2.Y > pp1.Y) // if line is descending down and to right
                {
                    pp2.X--;
                    pp2.Y--;
                }
                else if (pp2.Y < pp1.Y) // if line is ascending up and to right
                {
                    pp1.Y--;
                    pp2.X--;
                }
                _graphics.DrawLine(pen, pp1, pp2);
            }

            #endregion

            #region General Methods

            /// <summary>
            /// Draws an inset or raised line connecting the two points specified by
            /// the coordinate pairs. This method draws only horizontal or vertical
            /// lines. If the line is not horizontal or vertical, this method throws an
            /// ArgumentException. If the line is horizontal, the pixels are
            /// drawn just below the specified grid line. If the line is
            /// vertical, the pixels are drawn just to the right of the
            /// the specified grid line.
            /// </summary>
            /// <param name="p1">Point structure that specifies the first grid point to connect.</param>
            /// <param name="p2">Point structure that specifies the second grid point to connect.</param>
            /// <param name="style">Specifies the raised or inset style.</param>
            /// <param name="depth">Specifies the depth of the raised or inset line or rectangle.
            /// Valid values are 1 and 2.</param>
            public void Draw3DLine(Point p1, Point p2, ThreeDStyle style, int depth)
            {
                if (p1.X != p2.X && p1.Y != p2.Y)
                    throw new ArgumentException();
                if (depth != 1 && depth != 2)
                    throw new ArgumentException();

                if (depth == 1)
                {
                    switch (style)
                    {
                        case ThreeDStyle.Inset:
                        case ThreeDStyle.Groove:
                            if (p1.Y == p2.Y)
                            {
                                DrawLine(SystemPens.ControlDark, p1, p2);
                                Point pl1 = new Point(p1.X, p1.Y + 1);
                                Point pl2 = new Point(p2.X, p2.Y + 1);
                                DrawLine(SystemPens.ControlLightLight, pl1, pl2);
                            }
                            else
                            {
                                DrawLine(SystemPens.ControlDark, p1, p2);
                                Point pl1 = new Point(p1.X + 1, p1.Y);
                                Point pl2 = new Point(p2.X + 1, p2.Y);
                                DrawLine(SystemPens.ControlLightLight, pl1, pl2);
                            }
                            break;
                        case ThreeDStyle.Raised:
                        case ThreeDStyle.Ridge:
                            if (p1.Y == p2.Y)
                            {
                                DrawLine(SystemPens.ControlLightLight, p1, p2);
                                Point pd1 = new Point(p1.X, p1.Y + 1);
                                Point pd2 = new Point(p2.X, p2.Y + 1);
                                DrawLine(SystemPens.ControlDark, pd1, pd2);
                            }
                            else
                            {
                                DrawLine(SystemPens.ControlLightLight, p1, p2);
                                Point pd1 = new Point(p1.X + 1, p1.Y);
                                Point pd2 = new Point(p2.X + 1, p2.Y);
                                DrawLine(SystemPens.ControlDark, pd1, pd2);
                            }
                            break;
                    }
                }
                else if (depth == 2)
                {
                    switch (style)
                    {
                        case ThreeDStyle.Inset:
                        case ThreeDStyle.Groove:
                            if (p1.Y == p2.Y)
                            {
                                DrawLine(SystemPens.ControlDarkDark, p1, p2);
                                Point pp1 = new Point(p1.X, p1.Y + 1);
                                Point pp2 = new Point(p2.X, p2.Y + 1);
                                DrawLine(SystemPens.ControlDark, pp1, pp2);
                                pp1.Y++;
                                pp2.Y++;
                                DrawLine(SystemPens.ControlLight, pp1, pp2);
                                pp1.Y++;
                                pp2.Y++;
                                DrawLine(SystemPens.ControlLightLight, pp1, pp2);
                            }
                            else
                            {
                                DrawLine(SystemPens.ControlDarkDark, p1, p2);
                                Point pp1 = new Point(p1.X + 1, p1.Y);
                                Point pp2 = new Point(p2.X + 1, p2.Y);
                                DrawLine(SystemPens.ControlDark, pp1, pp2);
                                pp1.X++;
                                pp2.X++;
                                DrawLine(SystemPens.ControlLight, pp1, pp2);
                                pp1.X++;
                                pp2.X++;
                                DrawLine(SystemPens.ControlLightLight, pp1, pp2);
                            }
                            break;
                        case ThreeDStyle.Raised:
                        case ThreeDStyle.Ridge:
                            if (p1.Y == p2.Y)
                            {
                                DrawLine(SystemPens.ControlLightLight, p1, p2);
                                Point pp1 = new Point(p1.X, p1.Y + 1);
                                Point pp2 = new Point(p2.X, p2.Y + 1);
                                DrawLine(SystemPens.ControlLight, pp1, pp2);
                                pp1.Y++;
                                pp2.Y++;
                                DrawLine(SystemPens.ControlDark, pp1, pp2);
                                pp1.Y++;
                                pp2.Y++;
                                DrawLine(SystemPens.ControlDarkDark, pp1, pp2);
                            }
                            else
                            {
                                DrawLine(SystemPens.ControlLightLight, p1, p2);
                                Point pp1 = new Point(p1.X + 1, p1.Y);
                                Point pp2 = new Point(p2.X + 1, p2.Y);
                                DrawLine(SystemPens.ControlLight, pp1, pp2);
                                pp1.X++;
                                pp2.X++;
                                DrawLine(SystemPens.ControlDark, pp1, pp2);
                                pp1.X++;
                                pp2.X++;
                                DrawLine(SystemPens.ControlDarkDark, pp1, pp2);
                            }
                            break;
                    }
                }
            }

            /// <summary>
            /// Draws an inset or raised line connecting the two points specified by
            /// the coordinate pairs. This method draws only horizontal or vertical
            /// lines. If the line is not horizontal or vertical, this method throws an
            /// ArgumentException. If the line is horizontal, the pixels are
            /// drawn just below the specified grid line. If the line is
            /// vertical, the pixels are drawn just to the right of the
            /// the specified grid line.
            /// </summary>
            /// <param name="x1">x coordinate of the first grid point.</param>
            /// <param name="y1">y coordinate of the first grid point.</param>
            /// <param name="x2">x coordinate of the second grid point.</param>
            /// <param name="y2">y coordinate of the second grid point.</param>
            /// <param name="style">Specifies the raised or inset style.</param>
            /// <param name="depth">Specifies the depth of the raised or inset line or rectangle.
            /// Valid values are 1 and 2.</param>
            public void Draw3DLine(int x1, int y1, int x2, int y2, ThreeDStyle style, int depth)
            {
                this.Draw3DLine(new Point(x1, y1), new Point(x2, y2), style, depth);
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            #endregion
        }

        class ArrowWindow : Form
        {
            #region Instance Members

            private byte opacity = 200;     // Initializer

            #endregion

            #region Constructor

            public ArrowWindow()
            {
                this.SetStyle(ControlStyles.Selectable, false);
                this.SetStyle(ControlStyles.UserMouse | ControlStyles.FixedWidth | ControlStyles.FixedHeight, true);

                this.Visible = false;
                this.ShowIcon = false;
                this.ControlBox = false;
                this.ShowInTaskbar = false;
                this.FormBorderStyle = FormBorderStyle.None;
                this.StartPosition = FormStartPosition.Manual;
            }

            public ArrowWindow(Bitmap bmp, byte opacity)
                : this()
            {
                if (bmp == null)
                    throw new ArgumentNullException();

                this.opacity = opacity;
                GDIWindow(bmp, opacity);
            }

            #endregion

            #region Destructor

            ~ArrowWindow()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Property

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;

                    // This form has to have the WS_EX_LAYERED extended style.
                    cp.ExStyle |= (int)User32.WindowExStyles.WS_EX_LAYERED;
                    // This form has to have the WS_EX_TOPMOST extended style.
                    cp.ExStyle |= (int)User32.WindowExStyles.WS_EX_TOPMOST;
                    // Hide from ALT+Tab menu, Turn on WS_EX_TOOLWINDOW style.
                    cp.ExStyle |= (int)User32.WindowExStyles.WS_EX_TOOLWINDOW;

                    return cp;
                }
            }

            protected override bool ShowWithoutActivation
            {
                // True if the window will not be activated when it is shown; otherwise, false.
                get { return true; }
            }

            #endregion

            #region Override Methods

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case (int)User32.Msgs.WM_NCHITTEST:
                        m.Result = (IntPtr)User32._HT_TRANSPARENT;
                        break;
                    case (int)User32.Msgs.WM_MOUSEACTIVATE:
                        break;
                    default:
                        base.WndProc(ref m);
                        break;
                }
            }

            #endregion

            #region Helper Methods

            private void GDIWindow(Bitmap bitmap, byte opacity)
            {
                IntPtr scrDc = User32.GetDC(IntPtr.Zero);
                IntPtr memDc = User32.CreateCompatibleDC(scrDc);
                IntPtr hBitmap = IntPtr.Zero;
                IntPtr oldBitmap = IntPtr.Zero;

                try
                {
                    hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                    oldBitmap = User32.SelectObject(memDc, hBitmap);

                    Size size = new Size(bitmap.Width, bitmap.Height);
                    Point pos = this.Location;
                    Point pointSource = new Point(0, 0);
                    User32.BLENDFUNCTION blend = new User32.BLENDFUNCTION();
                    blend.BlendOp = User32._AC_SRC_OVER;
                    blend.BlendFlags = 0;
                    blend.SourceConstantAlpha = opacity;
                    blend.AlphaFormat = User32._AC_SRC_ALPHA;

                    User32.UpdateLayeredWindow(Handle, scrDc, ref pos, ref size, memDc, ref pointSource, 0, ref blend, User32._LWA_ALPHA);
                }
                finally
                {
                    User32.ReleaseDC(IntPtr.Zero, scrDc);
                    if (hBitmap != IntPtr.Zero)
                    {
                        User32.SelectObject(memDc, oldBitmap);
                        User32.DeleteObject(hBitmap);
                    }
                    User32.DeleteDC(memDc);
                }
            }

            #endregion
        }

        class UpDown32 : NativeWindow
        {
            #region Constructor

            public UpDown32(IntPtr hwnd)
                : base()
            {
                this.AssignHandle(hwnd);
            }

            #endregion

            #region Destructor

            ~UpDown32()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Override Methods

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                switch (m.Msg)
                {
                    case (int)User32.Msgs.WM_DESTROY:
                        this.ReleaseHandle();
                        break;
                    case (int)User32.Msgs.WM_NCDESTROY:
                        this.ReleaseHandle();
                        break;
                    case (int)User32.Msgs.WM_NCPAINT:
                        int style = User32.GetWindowLong(m.HWnd, (int)User32.WindowExStyles.GWL_STYLE);
                        if ((style & (int)User32.WindowStyles.WS_VISIBLE) == (int)User32.WindowStyles.WS_VISIBLE)
                        {
                            User32.SetWindowPos(m.HWnd, new IntPtr(0), 0, 0, 0, 0, User32.FlagsSetWindowPos.SWP_NOMOVE | User32.FlagsSetWindowPos.SWP_NOSIZE | User32.FlagsSetWindowPos.SWP_NOREDRAW | User32.FlagsSetWindowPos.SWP_NOACTIVATE);
                            User32.SetWindowLong(m.HWnd, (int)User32.WindowExStyles.GWL_STYLE, style & ~(int)User32.WindowStyles.WS_VISIBLE);
                        }
                        m.Result = (IntPtr)1; // indicate msg has been processed
                        break;
                }
            }

            #endregion
        }

        class Scroller : Control
        {
            #region Event

            public event EventHandler ScrollLeft;
            public event EventHandler ScrollRight;

            #endregion

            #region Instance Members

            public RolloverUpDown _leftScroller;
            public RolloverUpDown _rightScroller;

            #endregion

            #region Constructor

            public Scroller()
                : base()
            {
                this.SetStyle(ControlStyles.Selectable, false);
                this.SetStyle(ControlStyles.FixedWidth | ControlStyles.FixedHeight, true);
            }

            public Scroller(UpDown32Style upDownStyle)
                : this()
            {
                _leftScroller = new RolloverUpDown(true);
                _rightScroller = new RolloverUpDown(true);

                switch (upDownStyle)
                {
                    case UpDown32Style.BlackGlass:
                        _leftScroller.NormalImage = Resources.LeftNormalBlackGlass;
                        _leftScroller.HoverImage = Resources.LeftHoverBlackGlass;
                        _leftScroller.DownImage = Resources.LeftDownBlackGlass;
                        _rightScroller.NormalImage = Resources.RightNormalBlackGlass;
                        _rightScroller.HoverImage = Resources.RightHoverBlackGlass;
                        _rightScroller.DownImage = Resources.RightDownBlackGlass;
                        break;
                    case UpDown32Style.KRBBlue:
                        _leftScroller.NormalImage = Resources.LeftNormalKRBBlue;
                        _leftScroller.HoverImage = Resources.LeftHoverKRBBlue;
                        _leftScroller.DownImage = Resources.LeftDownKRBBlue;
                        _rightScroller.NormalImage = Resources.RightNormalKRBBlue;
                        _rightScroller.HoverImage = Resources.RightHoverKRBBlue;
                        _rightScroller.DownImage = Resources.RightDownKRBBlue;
                        break;
                    case UpDown32Style.OfficeBlue:
                        _leftScroller.NormalImage = Resources.LeftNormalOfficeBlue;
                        _leftScroller.HoverImage = Resources.LeftHoverOfficeBlue;
                        _leftScroller.DownImage = Resources.LeftDownOfficeBlue;
                        _rightScroller.NormalImage = Resources.RightNormalOfficeBlue;
                        _rightScroller.HoverImage = Resources.RightHoverOfficeBlue;
                        _rightScroller.DownImage = Resources.RightDownOfficeBlue;
                        break;
                    case UpDown32Style.OfficeOlive:
                        _leftScroller.NormalImage = Resources.LeftNormalOfficeOlive;
                        _leftScroller.HoverImage = Resources.LeftHoverOfficeOlive;
                        _leftScroller.DownImage = Resources.LeftDownOfficeOlive;
                        _rightScroller.NormalImage = Resources.RightNormalOfficeOlive;
                        _rightScroller.HoverImage = Resources.RightHoverOfficeOlive;
                        _rightScroller.DownImage = Resources.RightDownOfficeOlive;
                        break;
                    case UpDown32Style.OfficeSilver:
                        _leftScroller.NormalImage = Resources.LeftNormalOfficeSilver;
                        _leftScroller.HoverImage = Resources.LeftHoverOfficeSilver;
                        _leftScroller.DownImage = Resources.LeftDownOfficeSilver;
                        _rightScroller.NormalImage = Resources.RightNormalOfficeSilver;
                        _rightScroller.HoverImage = Resources.RightHoverOfficeSilver;
                        _rightScroller.DownImage = Resources.RightDownOfficeSilver;
                        break;
                    default:
                        _leftScroller.NormalImage = Resources.leftArrow;
                        _leftScroller.HoverImage = Resources.leftArrowHover;
                        _leftScroller.DownImage = Resources.leftArrowDown;
                        _rightScroller.NormalImage = Resources.RightArrow;
                        _rightScroller.HoverImage = Resources.RightArrowHover;
                        _rightScroller.DownImage = Resources.RightArrowDown;
                        break;
                }

                this.Size = new Size()
                {
                    Width = _leftScroller.NormalImage.Width * 2 + 1,
                    Height = _leftScroller.NormalImage.Height
                };

                _leftScroller.Location = new Point(0, 0);
                _rightScroller.Location = new Point(this.Width / 2 + 1, 0);

                _leftScroller.MouseDown += new MouseEventHandler(OnLeftScroll);
                _rightScroller.MouseDown += new MouseEventHandler(OnRightScroll);

                this.Controls.Add(_leftScroller);
                this.Controls.Add(_rightScroller);
            }

            #endregion

            #region Destructor

            ~Scroller()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Property

            //protected override CreateParams CreateParams
            //{
            //    get
            //    {
            //        CreateParams cp = base.CreateParams;
            //        cp.ExStyle |= 0x20;

            //        return cp;
            //    }
            //}

            #endregion

            #region Override Methods

            protected override void OnPaintBackground(PaintEventArgs pevent)
            {
                if (BackColor == Color.Transparent)
                {
                    // Do Nothing.
                }
                else
                {
                    base.OnPaintBackground(pevent);
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _leftScroller.MouseDown -= new MouseEventHandler(OnLeftScroll);
                    _rightScroller.MouseDown -= new MouseEventHandler(OnRightScroll);

                    _leftScroller.Dispose();
                    _rightScroller.Dispose();
                }

                base.Dispose(disposing);
            }

            #endregion

            #region Helper Methods

            private void OnRightScroll(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (ScrollRight != null)
                        ScrollRight(this, EventArgs.Empty);
                }
            }

            private void OnLeftScroll(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (ScrollLeft != null)
                        ScrollLeft(this, EventArgs.Empty);
                }
            }

            #endregion
        }

        class RolloverUpDown : UpDownBase
        {
            #region Instance Members

            private Image _disabledImage;
            private Image _normalImage;
            private Image _hoverImage;
            private Image _downImage;

            #endregion

            #region Constructor

            public RolloverUpDown(bool value)
                : base()
            {
                this.Caching = value;
            }

            #endregion

            #region Destructor

            ~RolloverUpDown()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Property

            public Image DisabledImage
            {
                get { return _disabledImage; }
                set
                {
                    if (value != null)
                    {
                        if (!value.Equals(_disabledImage))
                            _disabledImage = value;
                    }
                    else
                    {
                        _disabledImage = null;
                    }
                }
            }

            public Image NormalImage
            {
                get { return _normalImage; }
                set
                {
                    if (value != null)
                    {
                        if (!value.Equals(_normalImage))
                        {
                            _normalImage = value;

                            this.Size = new Size(_normalImage.Width, _normalImage.Height);
                        }
                    }
                    else
                    {
                        _normalImage = null;
                    }
                }
            }

            public Image HoverImage
            {
                get { return _hoverImage; }
                set
                {
                    if (value != null)
                    {
                        if (!value.Equals(_hoverImage))
                            _hoverImage = value;
                    }
                    else
                    {
                        _hoverImage = null;
                    }
                }
            }

            public Image DownImage
            {
                get { return _downImage; }
                set
                {
                    if (value != null)
                    {
                        if (!value.Equals(_downImage))
                            _downImage = value;
                    }
                    else
                    {
                        _downImage = null;
                    }
                }
            }

            #endregion

            #region Override Methods

            //protected override CreateParams CreateParams
            //{
            //    get
            //    {
            //        CreateParams cp = base.CreateParams;
            //        cp.ExStyle |= 0x20;

            //        return cp;
            //    }
            //}

            protected override void PaintDisabled(Graphics g)
            {
                if (_disabledImage != null)
                    g.DrawImageUnscaled(_disabledImage, 0, 0);
                else
                {
                    if (_normalImage != null)
                        ControlPaint.DrawImageDisabled(g, _normalImage, 0, 0, this.BackColor);
                }
            }

            protected override void PaintNormal(Graphics g)
            {
                if (_normalImage != null)
                    g.DrawImageUnscaled(_normalImage, 0, 0);
            }

            protected override void PaintHover(Graphics g)
            {
                if (_hoverImage != null)
                    g.DrawImageUnscaled(_hoverImage, 0, 0);
            }

            protected override void PaintDown(Graphics g)
            {
                if (_downImage != null)
                    g.DrawImageUnscaled(_downImage, 0, 0);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_disabledImage != null)
                        _disabledImage.Dispose();
                    if (_normalImage != null)
                        _normalImage.Dispose();
                    if (_hoverImage != null)
                        _hoverImage.Dispose();
                    if (_downImage != null)
                        _downImage.Dispose();
                }

                base.Dispose(disposing);
            }

            #endregion
        }

        abstract class UpDownBase : Control
        {
            #region Enum

            private enum States
            {
                Disabled,
                Normal,
                Hover,
                Down
            };

            #endregion

            #region Instance Members

            private Image _disabledImage;
            private Image _normalImage;
            private Image _hoverImage;
            private Image _downImage;

            private bool _caching = true;               // Initializer
            private States _btnState = States.Normal;   // Initializer

            #endregion

            #region Constructor

            public UpDownBase()
            {
                this.SetStyle(ControlStyles.Selectable, false);
                this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.FixedWidth | ControlStyles.FixedHeight, true);
            }

            #endregion

            #region Destructor

            ~UpDownBase()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Property

            public bool Caching
            {
                get { return _caching; }
                set
                {
                    if (!value.Equals(_caching))
                        _caching = value;
                }
            }

            #endregion

            #region Override Methods

            protected override void OnPaint(PaintEventArgs e)
            {
                if (!_caching)
                {
                    switch (_btnState)
                    {
                        case States.Down:
                            PaintDown(e.Graphics);
                            break;
                        case States.Hover:
                            PaintHover(e.Graphics);
                            break;
                        case States.Normal:
                            PaintNormal(e.Graphics);
                            break;
                        case States.Disabled:
                            PaintDisabled(e.Graphics);
                            break;
                    }
                }
                else
                {
                    switch (_btnState)
                    {
                        case States.Down:
                            CreateAndPaintCachedImage(_downImage,
                                new ClientPaintMethod(PaintDown), e.Graphics);
                            break;
                        case States.Hover:
                            CreateAndPaintCachedImage(_hoverImage,
                                new ClientPaintMethod(PaintHover), e.Graphics);
                            break;
                        case States.Normal:
                            CreateAndPaintCachedImage(_normalImage,
                                new ClientPaintMethod(PaintNormal), e.Graphics);
                            break;
                        case States.Disabled:
                            CreateAndPaintCachedImage(_disabledImage,
                                new ClientPaintMethod(PaintDisabled), e.Graphics);
                            break;
                    }
                }
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                if (_btnState == States.Disabled)
                    return;

                _btnState = States.Hover;

                this.Invalidate();

                base.OnMouseEnter(e);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                if (_btnState == States.Disabled)
                    return;

                if (e.Button == MouseButtons.Left)
                {
                    _btnState = States.Down;
                    this.Invalidate();
                }

                base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                if (_btnState == States.Disabled)
                    return;

                if (e.Button == MouseButtons.Left)
                {
                    _btnState = States.Hover;
                    this.Invalidate();
                }

                base.OnMouseUp(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                if (_btnState == States.Disabled)
                    return;

                _btnState = States.Normal;

                this.Invalidate();

                base.OnMouseLeave(e);
            }

            protected override void OnEnabledChanged(EventArgs e)
            {
                if (!this.Enabled)
                    _btnState = States.Disabled;
                else if (this.Enabled && _btnState == States.Disabled)
                    _btnState = States.Normal;

                this.Invalidate();

                base.OnEnabledChanged(e);
            }

            #endregion

            #region Abstract Methods

            protected abstract void PaintDisabled(Graphics g);

            protected abstract void PaintNormal(Graphics g);

            protected abstract void PaintHover(Graphics g);

            protected abstract void PaintDown(Graphics g);

            #endregion

            #region Helper Methods

            private delegate void ClientPaintMethod(Graphics g);

            private void CreateAndPaintCachedImage(
                Image image, ClientPaintMethod paintMethod, Graphics g)
            {
                if (image == null)
                {
                    image = new Bitmap(Width, Height);
                    Graphics bufferedGraphics = Graphics.FromImage(image);
                    paintMethod(bufferedGraphics);
                    bufferedGraphics.Dispose();
                }
                g.DrawImageUnscaled(image, 0, 0);
            }

            #endregion

            #region General Methods

            protected void Flush()
            {
                _disabledImage = null;
                _normalImage = null;
                _hoverImage = null;
                _downImage = null;
            }

            #endregion
        }
    }

    public class KRBTabControlDesigner : System.Windows.Forms.Design.ParentControlDesigner
    {
        #region Instance Members

        private DesignerVerbCollection _verbs;
        private DesignerActionListCollection _actionLists;

        private IDesignerHost _designerHost;
        private IComponentChangeService _changeService;

        #endregion

        #region Constructor

        public KRBTabControlDesigner()
            : base() { }

        #endregion

        #region Destructor

        ~KRBTabControlDesigner()
        {
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Property

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (_actionLists == null)
                {
                    _actionLists = new DesignerActionListCollection();
                    _actionLists.Add(new KRBTabControlActionList((KRBTabControl)Control));
                }

                return _actionLists;
            }
        }

        public override DesignerVerbCollection Verbs
        {
            get
            {
                if (_verbs == null)
                {
                    DesignerVerb[] addVerbs = new DesignerVerb[]
                    {
                        new DesignerVerb("Add Tab", new EventHandler(OnAddTab)),
                        new DesignerVerb("Remove Tab", new EventHandler(OnRemoveTab))
                    };

                    _verbs = new DesignerVerbCollection();
                    _verbs.AddRange(addVerbs);

                    KRBTabControl parentControl = Control as KRBTabControl;
                    if (parentControl != null)
                    {
                        switch (parentControl.TabPages.Count)
                        {
                            case 0:
                                _verbs[1].Enabled = false;
                                break;
                            default:
                                _verbs[1].Enabled = true;
                                break;
                        }
                    }
                }

                return _verbs;
            }
        }

        #endregion

        #region Override Methods

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            _designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
            _changeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));

            // Update your designer verb whenever ComponentChanged event occurs.
            if (_changeService != null)
                _changeService.ComponentChanged += new ComponentChangedEventHandler(OnComponentChanged);
        }

        /*  As a general rule, always call the base method first in the PreFilterXxx() methods and last in the 
            PostFilterXxx() methods. This way, all designer classes are given the proper opportunity to apply their 
            changes. The ControlDesigner and ComponentDesigner use these methods to add properties like Visible, 
            Enabled, Name, and Locked. */

        /// <summary>
        /// Override this method to remove unused or inappropriate events.
        /// </summary>
        /// <param name="events">Events collection of the control.</param>
        protected override void PostFilterEvents(System.Collections.IDictionary events)
        {
            events.Remove("StyleChanged");
            events.Remove("MarginChanged");
            events.Remove("PaddingChanged");
            events.Remove("EnabledChanged");
            events.Remove("ImeModeChanged");
            events.Remove("LocationChanged");
            events.Remove("RightToLeftChanged");
            events.Remove("BindingContextChanged");
            events.Remove("RightToLeftLayoutChanged");

            base.PostFilterEvents(events);
        }

        /// <summary>
        /// Override this method to add some properties to the control or change the properties attributes for a dynamic user interface.
        /// </summary>
        /// <param name="properties">Properties collection of the control before than add a new property to the collection by user.</param>
        protected override void PreFilterProperties(System.Collections.IDictionary properties)
        {
            base.PreFilterProperties(properties);

            // We don't want to show the "Location" and "ShowToolTips" properties for our control at the design-time.
            properties["Location"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
              (PropertyDescriptor)properties["Location"], BrowsableAttribute.No);
            properties["ShowToolTips"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
              (PropertyDescriptor)properties["ShowToolTips"], BrowsableAttribute.No);

            /* After than, we don't want to see some properties at design-time for general reasons(Dynamic property attributes). */
            KRBTabControl parentControl = Control as KRBTabControl;

            if (parentControl != null)
            {
                if (parentControl.HeaderVisibility)
                {
                    properties["ItemSize"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["ItemSize"], BrowsableAttribute.No);
                    properties["TabStyles"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["TabStyles"], BrowsableAttribute.No);
                    properties["Alignments"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["Alignments"], BrowsableAttribute.No);
                    properties["UpDownStyle"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["UpDownStyle"], BrowsableAttribute.No);
                    properties["HeaderStyle"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["HeaderStyle"], BrowsableAttribute.No);
                    properties["TabGradient"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["TabGradient"], BrowsableAttribute.No);
                    properties["IsDrawHeader"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["IsDrawHeader"], BrowsableAttribute.No);
                    properties["CaptionButtons"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["CaptionButtons"], BrowsableAttribute.No);
                    properties["TabBorderColor"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["TabBorderColor"], BrowsableAttribute.No);
                    properties["GradientCaption"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["GradientCaption"], BrowsableAttribute.No);
                    properties["BackgroundColor"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["BackgroundColor"], BrowsableAttribute.No);
                    properties["BackgroundImage"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["BackgroundImage"], BrowsableAttribute.No);
                    properties["BackgroundHatcher"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["BackgroundHatcher"], BrowsableAttribute.No);
                    properties["CaptionRandomizer"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["CaptionRandomizer"], BrowsableAttribute.No);
                    properties["IsDrawTabSeparator"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["IsDrawTabSeparator"], BrowsableAttribute.No);

                    return;
                }

                if (!parentControl.IsCaptionVisible)
                {
                    properties["CaptionButtons"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                        (PropertyDescriptor)properties["CaptionButtons"], BrowsableAttribute.No);
                    properties["CaptionRandomizer"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                        (PropertyDescriptor)properties["CaptionRandomizer"], BrowsableAttribute.No);
                    properties["GradientCaption"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                        (PropertyDescriptor)properties["GradientCaption"], BrowsableAttribute.No);
                }

                if (parentControl.IsDrawHeader)
                {
                    switch (parentControl.HeaderStyle)
                    {
                        case KRBTabControl.TabHeaderStyle.Hatch:
                            properties["BackgroundColor"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                              (PropertyDescriptor)properties["BackgroundColor"], BrowsableAttribute.No);
                            properties["BackgroundImage"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                              (PropertyDescriptor)properties["BackgroundImage"], BrowsableAttribute.No);
                            break;
                        case KRBTabControl.TabHeaderStyle.Solid:
                            properties["BackgroundImage"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                              (PropertyDescriptor)properties["BackgroundImage"], BrowsableAttribute.No);
                            properties["BackgroundHatcher"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                              (PropertyDescriptor)properties["BackgroundHatcher"], BrowsableAttribute.No);
                            break;
                        default:
                            properties["BackgroundColor"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                              (PropertyDescriptor)properties["BackgroundColor"], BrowsableAttribute.No);
                            properties["BackgroundHatcher"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                              (PropertyDescriptor)properties["BackgroundHatcher"], BrowsableAttribute.No);
                            break;
                    }
                }
                else
                {
                    properties["HeaderStyle"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["HeaderStyle"], BrowsableAttribute.No);
                    properties["BackgroundColor"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["BackgroundColor"], BrowsableAttribute.No);
                    properties["BackgroundImage"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["BackgroundImage"], BrowsableAttribute.No);
                    properties["BackgroundHatcher"] = TypeDescriptor.CreateProperty(typeof(KRBTabControl),
                      (PropertyDescriptor)properties["BackgroundHatcher"], BrowsableAttribute.No);
                }
            }
        }

        /// <summary>
        /// Override this method to remove unused or inappropriate properties.
        /// </summary>
        /// <param name="properties">Properties collection of the control.</param>
        protected override void PostFilterProperties(System.Collections.IDictionary properties)
        {
            properties.Remove("Margin");
            properties.Remove("ImeMode");
            properties.Remove("Padding");
            properties.Remove("Enabled");
            properties.Remove("RightToLeft");
            properties.Remove("RightToLeftLayout");
            properties.Remove("ApplicationSettings");
            properties.Remove("DataBindings");

            base.PostFilterProperties(properties);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == (int)User32.Msgs.WM_NCHITTEST)
            {
                if (m.Result.ToInt32() == User32._HT_TRANSPARENT)
                    m.Result = (IntPtr)User32._HT_CLIENT;
            }
        }

        protected override bool GetHitTest(Point point)
        {
            ISelectionService _selectionService = (ISelectionService)GetService(typeof(ISelectionService));
            if (_selectionService != null)
            {
                object selectedObject = _selectionService.PrimarySelection;
                if (selectedObject != null && selectedObject.Equals(this.Control))
                {
                    Point p = this.Control.PointToClient(point);

                    User32.TCHITTESTINFO hti = new User32.TCHITTESTINFO(p, User32.TabControlHitTest.TCHT_ONITEM);

                    Message m = new Message();
                    m.HWnd = this.Control.Handle;
                    m.Msg = User32._TCM_HITTEST;

                    IntPtr lParam = System.Runtime.InteropServices.Marshal.AllocHGlobal(System.Runtime.InteropServices.Marshal.SizeOf(hti));
                    System.Runtime.InteropServices.Marshal.StructureToPtr(hti, lParam, false);
                    m.LParam = lParam;

                    base.WndProc(ref m);
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(lParam);

                    if (m.Result.ToInt32() != -1)
                        return hti.flags != User32.TabControlHitTest.TCHT_NOWHERE;
                }
            }

            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _changeService != null)
                _changeService.ComponentChanged -= OnComponentChanged;

            base.Dispose(disposing);
        }

        #endregion

        #region Helper Methods

        /*  When the designer modifies the KRBTabControl.TabPages collection, 
                the Properties window is not updated until the control is deselected and then reselected. To 
                correct this defect, you need to explicitly notify the IDE that a change has been made by using 
                the PropertyDescriptor for the property. */

        private void OnAddTab(Object sender, EventArgs e)
        {
            KRBTabControl parentControl = Control as KRBTabControl;

            TabControl.TabPageCollection oldTabs = parentControl.TabPages;

            // Notify the IDE that the TabPages collection property of the current tab control has changed.
            RaiseComponentChanging(TypeDescriptor.GetProperties(parentControl)["TabPages"]);
            TabPageEx newTab = (TabPageEx)_designerHost.CreateComponent(typeof(TabPageEx));
            newTab.Text = newTab.Name;
            parentControl.TabPages.Add(newTab);
            parentControl.SelectedTab = newTab;
            RaiseComponentChanged(TypeDescriptor.GetProperties(parentControl)["TabPages"], oldTabs, parentControl.TabPages);
        }

        private void OnRemoveTab(Object sender, EventArgs e)
        {
            KRBTabControl parentControl = Control as KRBTabControl;

            if (parentControl.SelectedIndex < 0)
                return;

            TabControl.TabPageCollection oldTabs = parentControl.TabPages;

            // Notify the IDE that the TabPages collection property of the current tab control has changed.
            RaiseComponentChanging(TypeDescriptor.GetProperties(parentControl)["TabPages"]);
            _designerHost.DestroyComponent(parentControl.SelectedTab);
            RaiseComponentChanged(TypeDescriptor.GetProperties(parentControl)["TabPages"], oldTabs, parentControl.TabPages);
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            KRBTabControl parentControl = e.Component as KRBTabControl;

            if (parentControl != null && e.Member.Name == "TabPages")
            {
                foreach (DesignerVerb verb in Verbs)
                {
                    if (verb.Text == "Remove Tab")
                    {
                        switch (parentControl.TabPages.Count)
                        {
                            case 0:
                                verb.Enabled = false;
                                break;
                            default:
                                verb.Enabled = true;
                                break;
                        }

                        break;
                    }
                }
            }
        }

        #endregion
    }

    public class KRBTabControlActionList : DesignerActionList
    {
        #region Instance Members

        private KRBTabControl _linkedControl;
        private IDesignerHost _host;
        private IComponentChangeService _changeService;
        private DesignerActionUIService _designerService;
        private bool _isSupportedAlphaColor = false;

        #endregion

        #region Constructor

        // The constructor associates the control to the smart tag action list.
        public KRBTabControlActionList(KRBTabControl control)
            : base(control)
        {
            _linkedControl = control;
            _host = (IDesignerHost)GetService(typeof(IDesignerHost));
            _changeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));
            _designerService = (DesignerActionUIService)GetService(typeof(DesignerActionUIService));

            this.AutoShow = true;   // When this control will be added to the design area, the smart tag panel will open automatically.
        }

        #endregion

        #region Destructor

        ~KRBTabControlActionList()
        {
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Property - DesignerActionPropertyItem

        /* Properties that are targets of DesignerActionPropertyItem entries. */

        public KRBTabControl.TabStyle TabStyles
        {
            get { return _linkedControl.TabStyles; }
            set
            {
                GetPropertyByName("TabStyles").SetValue(_linkedControl, value);
            }
        }

        public KRBTabControl.TabAlignments Alignments
        {
            get { return _linkedControl.Alignments; }
            set
            {
                GetPropertyByName("Alignments").SetValue(_linkedControl, value);
            }
        }

        public Color FirstColor
        {
            get { return _linkedControl.TabGradient.ColorStart; }
            set
            {
                if (!value.Equals(_linkedControl.TabGradient.ColorStart))
                {
                    KRBTabControl.GradientTab oldGradient = _linkedControl.TabGradient;

                    DesignerTransaction transaction = null;
                    try
                    {
                        // Start the transaction.
                        transaction = _host.CreateTransaction("First Color");

                        // Trigger a new ComponentChanging event.
                        _changeService.OnComponentChanging(_linkedControl, GetPropertyByName("TabGradient"));

                        // Set the current value to the control.
                        _linkedControl.TabGradient.ColorStart = value;

                        // Trigger a new ComponentChanged event.
                        _changeService.OnComponentChanged(_linkedControl, GetPropertyByName("TabGradient"), oldGradient, _linkedControl.TabGradient);

                        // Commit the transaction.
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception while changing the value of the FirstColor property, " + ex.Message);
                        if (transaction != null)
                            transaction.Cancel();
                    }
                }
            }
        }

        public Color SecondColor
        {
            get { return _linkedControl.TabGradient.ColorEnd; }
            set
            {
                if (!value.Equals(_linkedControl.TabGradient.ColorEnd))
                {
                    KRBTabControl.GradientTab oldGradient = _linkedControl.TabGradient;

                    DesignerTransaction transaction = null;
                    try
                    {
                        // Start the transaction.
                        transaction = _host.CreateTransaction("Second Color");

                        // Trigger a new ComponentChanging event.
                        _changeService.OnComponentChanging(_linkedControl, GetPropertyByName("TabGradient"));

                        // Set the current value to the control.
                        _linkedControl.TabGradient.ColorEnd = value;

                        // Trigger a new ComponentChanged event.
                        _changeService.OnComponentChanged(_linkedControl, GetPropertyByName("TabGradient"), oldGradient, _linkedControl.TabGradient);

                        // Commit the transaction.
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception while changing the value of the SecondColor property, " + ex.Message);
                        if (transaction != null)
                            transaction.Cancel();
                    }
                }
            }
        }

        public LinearGradientMode GradientMode
        {
            get { return _linkedControl.TabGradient.GradientStyle; }
            set
            {
                if (!value.Equals(_linkedControl.TabGradient.GradientStyle))
                {
                    KRBTabControl.GradientTab oldGradient = _linkedControl.TabGradient;

                    DesignerTransaction transaction = null;
                    try
                    {
                        // Start the transaction.
                        transaction = _host.CreateTransaction("Gradient Mode");

                        // Trigger a new ComponentChanging event.
                        _changeService.OnComponentChanging(_linkedControl, GetPropertyByName("TabGradient"));

                        // Set the current value to the control.
                        _linkedControl.TabGradient.GradientStyle = value;

                        // Trigger a new ComponentChanged event.
                        _changeService.OnComponentChanged(_linkedControl, GetPropertyByName("TabGradient"), oldGradient, _linkedControl.TabGradient);

                        // Commit the transaction.
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception while changing the value of the GradientMode property, " + ex.Message);
                        if (transaction != null)
                            transaction.Cancel();
                    }
                }
            }
        }

        public bool IsSupportedAlphaColor
        {
            get { return _isSupportedAlphaColor; }
            set
            {
                if (!value.Equals(_isSupportedAlphaColor))
                    _isSupportedAlphaColor = value;
            }
        }

        #endregion

        #region Override Methods

        /* Implementation of this abstract method creates smart tag 
               items, associates their targets, and collects into list. */

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            try
            {
                // Creating the action list static headers.
                items.Add(new DesignerActionHeaderItem("Commands"));
                items.Add(new DesignerActionHeaderItem("Appearance"));

                if (!_linkedControl.HeaderVisibility)
                {
                    // Creates other action list headers.
                    items.Add(new DesignerActionHeaderItem("Tab Item Appearance"));

                    items.Add(new DesignerActionPropertyItem("TabStyles", "Tab Styles", "Appearance",
                         "Tab Style"));

                    items.Add(new DesignerActionPropertyItem("Alignments", "Tab Alignments", "Appearance",
                        "Tab Alignment"));

                    items.Add(new DesignerActionPropertyItem("FirstColor", "First Color", "Tab Item Appearance",
                        "First TabItem Color"));

                    items.Add(new DesignerActionPropertyItem("SecondColor", "Second Color", "Tab Item Appearance",
                        "Second TabItem Color"));

                    items.Add(new DesignerActionPropertyItem("GradientMode", "Gradient Mode", "Tab Item Appearance",
                        "Gradient Style"));

                    items.Add(new DesignerActionPropertyItem("IsSupportedAlphaColor", "Support Alpha Color", "Tab Item Appearance",
                        "Supports alpha component for tab item background colors"));

                    items.Add(new DesignerActionMethodItem(this,
                        "RandomizeColors", "Randomize Colors", "Tab Item Appearance",
                        "Randomize TabItem Colors", false));
                }

                items.Add(new DesignerActionMethodItem(this,
                    "HeaderVisibility", "StretchToParent " + (_linkedControl.HeaderVisibility ? "ON" : "OFF"), "Appearance",
                    "Determines whether the active tab is stretched to its parent container or not", false));

                items.Add(new DesignerActionMethodItem(this,
                    "AddTab", "Add Tab", "Commands",
                    "Add a new tab page to the container", false));

                if (_linkedControl.TabCount > 0)
                {
                    DesignerActionMethodItem methodRemove = new DesignerActionMethodItem(this, "RemoveTab", "Remove Tab", "Commands",
                        "Removes the selected tab page from the container", false);

                    items.Add(methodRemove);
                }

                // Add a new static header and its items.
                items.Add(new DesignerActionHeaderItem("Information"));
                items.Add(new DesignerActionTextItem("X: " + _linkedControl.Location.X + ", " + "Y: " + _linkedControl.Location.Y, "Information"));
                items.Add(new DesignerActionTextItem("Width: " + _linkedControl.Size.Width + ", " + "Height: " + _linkedControl.Size.Height, "Information"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception while generating the action list panel for this KRBTabControl, " + ex.Message);
            }

            return items;
        }

        #endregion

        #region Helper Methods

        /* This helper method to retrieve control properties.
           GetProperties ensures undo and menu updates to work properly. */

        private PropertyDescriptor GetPropertyByName(String propName)
        {
            if (propName != null)
            {
                PropertyDescriptor prop = TypeDescriptor.GetProperties(_linkedControl)[propName];

                if (prop != null)
                    return prop;
                else
                    throw new ArgumentException("Property name not found.", propName);
            }
            else
                throw new ArgumentNullException("Property name must not blank.");
        }

        #endregion

        #region General Methods - DesignerActionMethodItem

        /* Methods that are targets of DesignerActionMethodItem entries. */

        public void RandomizeColors()
        {
            DesignerTransaction transaction = null;
            try
            {
                // Start the transaction.
                transaction = _host.CreateTransaction("Randomize Colors");

                // Trigger a new ComponentChanging event.
                _changeService.OnComponentChanging(_linkedControl, GetPropertyByName("TabGradient"));

                Color a, b;
                Random rand = new Random();

                if (!IsSupportedAlphaColor)
                {
                    a = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
                    b = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
                }
                else
                {
                    a = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255), rand.Next(255));
                    b = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255), rand.Next(255));
                }

                _linkedControl.TabGradient.ColorEnd = a;
                _linkedControl.TabGradient.ColorStart = b;

                // Trigger a new ComponentChanged event.
                _changeService.OnComponentChanged(_linkedControl, GetPropertyByName("TabGradient"), null, null);

                // Commit the transaction.
                transaction.Commit();

                // Update Smart Tag Panel.
                _designerService.Refresh(Component);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception while doing randomize action, " + ex.Message);
                if (transaction != null)
                    transaction.Cancel();
            }
        }

        public void HeaderVisibility()
        {
            try
            {
                GetPropertyByName("HeaderVisibility").SetValue(_linkedControl, !_linkedControl.HeaderVisibility);
                _designerService.Refresh(Component);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception while changing the value of the HeaderVisibility property, " + ex.Message);
            }
        }

        public void AddTab()
        {
            TabControl.TabPageCollection oldTabs = _linkedControl.TabPages;

            DesignerTransaction transaction = null;
            try
            {
                // Start the transaction.
                transaction = _host.CreateTransaction("Add Tab");

                // Trigger a new ComponentChanging event.
                _changeService.OnComponentChanging(_linkedControl, GetPropertyByName("TabPages"));

                // Create a new designer component and add it to the TabPages collection.
                TabPageEx newTab = (TabPageEx)_host.CreateComponent(typeof(TabPageEx));
                newTab.Text = newTab.Name;

                // Add it to the TabPages collection.
                _linkedControl.TabPages.Add(newTab);

                // After than, select the new adding component.
                _linkedControl.SelectedTab = newTab;

                // Trigger a new ComponentChanged event.
                _changeService.OnComponentChanged(_linkedControl, GetPropertyByName("TabPages"), oldTabs, _linkedControl.TabPages);

                // Commit the transaction.
                transaction.Commit();

                // Update Smart Tag Panel.
                _designerService.Refresh(Component);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception while adding a new TabPage to the tab container, " + ex.Message);
                if (transaction != null)
                    transaction.Cancel();
            }
        }

        public void RemoveTab()
        {
            if (_linkedControl.SelectedIndex < 0)
                return;

            TabControl.TabPageCollection oldTabs = _linkedControl.TabPages;

            DesignerTransaction transaction = null;
            try
            {
                // Start the transaction.
                transaction = _host.CreateTransaction("Remove Tab");

                // Trigger a new ComponentChanging event.
                _changeService.OnComponentChanging(_linkedControl, GetPropertyByName("TabPages"));

                // Remove the currently selected TabPage from the collection and designer host.
                _host.DestroyComponent(_linkedControl.SelectedTab);

                // Trigger a new ComponentChanged event.
                _changeService.OnComponentChanged(_linkedControl, GetPropertyByName("TabPages"), oldTabs, _linkedControl.TabPages);

                // Commit the transaction.
                transaction.Commit();

                // Update Smart Tag Panel.
                _designerService.Refresh(Component);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception while removing the currently selected TabPage from the tab container, " + ex.Message);
                if (transaction != null)
                    transaction.Cancel();
            }
        }

        #endregion
    }

    [Designer(typeof(System.Windows.Forms.Design.ScrollableControlDesigner))]
    public class TabPageEx : TabPage
    {
        #region Instance Members

        internal bool preventClosing = false;
        private bool _isClosable = true;
        private string _text = null;

        #endregion

        #region Constructor

        public TabPageEx()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ContainerControl, true);

            this.Font = new Font("Arial", 10f);
            this.BackColor = Color.White;
        }

        public TabPageEx(string text)
            : this()
        {
            this.Text = text;
        }

        #endregion

        #region Destructor

        ~TabPageEx()
        {
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Property

        /// <summary>
        /// Determines whether the tab page is closable or not.
        /// </summary>
        [Description("Determines whether the tab page is closable or not")]
        [DefaultValue(true)]
        [Browsable(true)]
        public bool IsClosable
        {
            get { return _isClosable; }
            set 
            {
                if (!value.Equals(_isClosable))
                {
                    _isClosable = value;

                    if (this.Parent != null)
                    {
                        this.Parent.Invalidate();
                        this.Parent.Update();
                    }
                }
            }
        }

        public new string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (value != null && !value.Equals(_text))
                {
                    base.Text = value;
                    base.Text = base.Text.Trim();
                    base.Text = base.Text.PadRight(base.Text.Length + 2);
                    _text = base.Text.TrimEnd();
                }
            }
        }

        [DefaultValue(true)]
        [Browsable(true)]
        public new bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;
            }
        }

        #endregion

        #region Override Methods

        public override string ToString()
        {
            return this.Text;
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            
            if (this.Parent != null)
                this.Parent.Invalidate();
        }

        #endregion
    }
}