
// 
// Gorgon
// Copyright (C) 2018 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 31, 2018 10:50:08 AM
// 


using System.ComponentModel;
using Gorgon.Windows.Properties;

namespace Gorgon.UI;

/// <summary>
/// A message panel to display on an overlay
/// </summary>
public partial class GorgonWaitMessagePanel
    : UserControl
{

    // The image used for the wait icon.
    private Image _waitIcon;



    /// <summary>
    /// Indicates the border style for the for wait message box.
    /// </summary>
    [Browsable(true), Category("Appearance"), Description("Set whether there is a border on the wait message box."),
    DefaultValue(true), RefreshProperties(RefreshProperties.Repaint)]
    public bool WaitBoxHasBorder
    {
        get => BorderStyle == BorderStyle.FixedSingle;
        // ReSharper disable once ValueParameterNotUsed
        set => BorderStyle = value ? BorderStyle.FixedSingle : BorderStyle.None;
    }

    /// <summary>Gets or sets the foreground color of wait message box.</summary>
    [Browsable(true),
    Category("Appearance"),
    Description("Sets the foreground color for the text in the wait message box"),
    DefaultValue(typeof(Color), "Black"),
     RefreshProperties(RefreshProperties.Repaint)]
    public Color WaitBoxForeColor
    {
        get => LabelWaitMessage.ForeColor;
        set => LabelWaitMessage.ForeColor = value;
    }

    /// <summary>Gets or sets the foreground color of wait message box.</summary>
    [Browsable(true),
     Category("Appearance"),
     Description("Sets the foreground color for the title text in the wait message box"),
     DefaultValue(typeof(Color), "DimGray"),
     RefreshProperties(RefreshProperties.Repaint)]
    public Color WaitBoxTitleForeColor
    {
        get => LabelWaitTitle.ForeColor;
        set => LabelWaitTitle.ForeColor = value;
    }

    /// <summary>
    /// Property to set or return the message to display in the wait box.
    /// </summary>
    [Browsable(true),
     Category("Appearance"),
     Description("Sets the message to display in the wait message box."),
    RefreshProperties(RefreshProperties.Repaint)]
    public string WaitMessage
    {
        get => LabelWaitMessage.Text;
        set
        {
            if (string.Equals(value, LabelWaitMessage.Text, StringComparison.CurrentCulture))
            {
                return;
            }

            LabelWaitMessage.Text = value;

            if (string.IsNullOrEmpty(LabelWaitMessage.Text))
            {
                PanelWait.RowStyles[1].SizeType = SizeType.Absolute;
                PanelWait.RowStyles[1].Height = 0;

                if (string.IsNullOrEmpty(LabelWaitTitle.Text))
                {
                    PanelWait.ColumnStyles[1].SizeType = SizeType.Absolute;
                    PanelWait.ColumnStyles[1].Width = 0;
                }
                else if (PanelWait.ColumnStyles[1].SizeType != SizeType.AutoSize)
                {
                    PanelWait.ColumnStyles[1].SizeType = SizeType.AutoSize;
                }
            }
            else
            {
                PanelWait.RowStyles[1].SizeType = SizeType.AutoSize;
                PanelWait.ColumnStyles[1].SizeType = SizeType.AutoSize;
            }

            Invalidate();
        }
    }

    /// <summary>
    /// Property to set or return the title to display in the wait box.
    /// </summary>
    [Browsable(true),
     Category("Appearance"),
     Description("Sets the title to display in the wait message box."),
     RefreshProperties(RefreshProperties.Repaint)]
    public string WaitTitle
    {
        get => LabelWaitTitle.Text;
        set
        {
            if (string.Equals(value, LabelWaitTitle.Text, StringComparison.CurrentCulture))
            {
                return;
            }

            LabelWaitTitle.Text = value;

            if (string.IsNullOrEmpty(LabelWaitTitle.Text))
            {
                LabelWaitMessage.Anchor = AnchorStyles.Left;
                PanelWait.RowStyles[0].SizeType = SizeType.Absolute;
                PanelWait.RowStyles[0].Height = 0;

                if (string.IsNullOrEmpty(LabelWaitMessage.Text))
                {
                    PanelWait.ColumnStyles[1].SizeType = SizeType.Absolute;
                    PanelWait.ColumnStyles[1].Width = 0;
                }
                else if (PanelWait.ColumnStyles[1].SizeType != SizeType.AutoSize)
                {
                    PanelWait.ColumnStyles[1].SizeType = SizeType.AutoSize;
                }
            }
            else
            {
                LabelWaitMessage.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                PanelWait.RowStyles[0].SizeType = SizeType.AutoSize;
                PanelWait.ColumnStyles[1].SizeType = SizeType.AutoSize;
            }

            Invalidate();
        }
    }

    /// <summary>
    /// Property to set or return the font to use for the wait message.
    /// </summary>
    [Browsable(true),
     Category("Appearance"),
     Description("Sets the font to use in the wait message box."),
     RefreshProperties(RefreshProperties.Repaint),
    DefaultValue(typeof(Font), "Segoe UI, 9pt")]
    public Font WaitMessageFont
    {
        get => LabelWaitMessage.Font;
        set
        {
            LabelWaitMessage.Font = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Property to set or return the font to use for the wait message title.
    /// </summary>
    [Browsable(true),
     Category("Appearance"),
     Description("Sets the font to use in the wait message box title."),
     RefreshProperties(RefreshProperties.Repaint),
     DefaultValue(typeof(Font), "Segoe UI, 12pt")]
    public Font WaitTitleFont
    {
        get => LabelWaitTitle.Font;
        set
        {
            LabelWaitTitle.Font = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Property to set or return the icon used for the wait message.
    /// </summary>
    /// <remarks>
    /// Setting this value to <b>null</b> will reset the image to the original image for the control.
    /// </remarks>
    [Browsable(true),
     Category("Appearance"),
     Description("Sets the image to use in the wait message box."),
     RefreshProperties(RefreshProperties.Repaint),
     DefaultValue(typeof(Image), null)]
    public Image WaitIcon
    {
        get => _waitIcon;
        set
        {
            if (_waitIcon == value)
            {
                return;
            }

            if (value is null)
            {
                _waitIcon = null;
                ImageWait.Image = Resources.wait_48x48;
            }
            else
            {
                ImageWait.Image = _waitIcon = value;
            }
        }
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonWaitMessagePanel"/> class.
    /// </summary>
    public GorgonWaitMessagePanel() => InitializeComponent();

}
