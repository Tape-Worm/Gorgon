﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Fetze.WinFormsColor;

public class ColorSlider : UserControl
{
    private Bitmap srcImage = null;
    private int pickerSize = 5;
    private float pickerPos = 0.5f;
    private Color min = Color.Transparent;
    private Color max = Color.Transparent;
    private bool designSerializeColor = false;
    private int _dragState;

    public event EventHandler ValueChanged = null;
    public event EventHandler PercentualValueChanged = null;


    [Browsable(false)]
    public bool IsDragging => _dragState == 1;

    [Browsable(false)]
    public Rectangle ColorAreaRectangle => new Rectangle(
            ClientRectangle.X + pickerSize + 2,
            ClientRectangle.Y + pickerSize + 2,
            ClientRectangle.Width - pickerSize * 2 - 4,
            ClientRectangle.Height - pickerSize * 2 - 4);
    [DefaultValue(5)]
    public int PickerSize
    {
        get => pickerSize;
        set
        {
            pickerSize = value;
            Invalidate();
        }
    }
    [DefaultValue(true)]
    public bool ShowInnerPicker
    {
        get;
        set;
    }
    [DefaultValue(0.5f)]
    public float ValuePercentual
    {
        get => pickerPos;
        set
        {
            var lastVal = pickerPos;
            pickerPos = Math.Min(1.0f, Math.Max(0.0f, value));
            if ((pickerPos != lastVal) || (_dragState == 2))
            {
                OnPercentualValueChanged();
                UpdateColorValue();
                Invalidate();
            }
        }
    }

    [Browsable(false)]
    public Color Value
    {
        get;
        private set;
    }
    public Color Minimum
    {
        get => min;
        set
        {
            if (min != value)
            {
                SetupGradient(value, max);
                designSerializeColor = true;
            }
        }
    }
    public Color Maximum
    {
        get => max;
        set
        {
            if (max != value)
            {
                SetupGradient(min, value);
                designSerializeColor = true;
            }
        }
    }


    public ColorSlider()
    {
        SetStyle(ControlStyles.UserPaint, true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        SetStyle(ControlStyles.Selectable, true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        SetStyle(ControlStyles.ResizeRedraw, true);
        SetupHueGradient();
    }

    public void SetupGradient(Color min, Color max, int accuracy = 256)
    {
        accuracy = Math.Max(1, accuracy);

        this.min = min;
        this.max = max;
        srcImage = new Bitmap(1, accuracy);
        using (var g = Graphics.FromImage(srcImage))
        {
            var gradient = new LinearGradientBrush(
                new Point(0, srcImage.Height - 1),
                new Point(0, 0),
                min,
                max);
            g.FillRectangle(gradient, g.ClipBounds);
        }
        UpdateColorValue();
        Invalidate();
    }
    public void SetupGradient(ColorBlend blend, int accuracy = 256)
    {
        accuracy = Math.Max(1, accuracy);

        srcImage = new Bitmap(1, accuracy);
        using (var g = Graphics.FromImage(srcImage))
        {
            var gradient = new LinearGradientBrush(
                new Point(0, srcImage.Height - 1),
                new Point(0, 0),
                Color.Transparent,
                Color.Transparent)
            {
                InterpolationColors = blend
            };
            g.FillRectangle(gradient, g.ClipBounds);
        }
        min = srcImage.GetPixel(0, srcImage.Height - 1);
        max = srcImage.GetPixel(0, 0);
        UpdateColorValue();
        Invalidate();
    }
    public void SetupHueGradient(float saturation = 1.0f, float brightness = 1.0f, int accuracy = 256)
    {
        var blend = new ColorBlend
        {
            Colors = new Color[] {
                ExtMethodsSystemDrawingColor.ColorFromHSV(0.0f, saturation, brightness),
                ExtMethodsSystemDrawingColor.ColorFromHSV(1.0f / 6.0f, saturation, brightness),
                ExtMethodsSystemDrawingColor.ColorFromHSV(2.0f / 6.0f, saturation, brightness),
                ExtMethodsSystemDrawingColor.ColorFromHSV(3.0f / 6.0f, saturation, brightness),
                ExtMethodsSystemDrawingColor.ColorFromHSV(4.0f / 6.0f, saturation, brightness),
                ExtMethodsSystemDrawingColor.ColorFromHSV(5.0f / 6.0f, saturation, brightness),
                ExtMethodsSystemDrawingColor.ColorFromHSV(1.0f, saturation, brightness) },
            Positions = new float[] {
                0.0f,
                1.0f / 6.0f,
                2.0f / 6.0f,
                3.0f / 6.0f,
                4.0f / 6.0f,
                5.0f / 6.0f,
                1.0f}
        };
        SetupGradient(blend, accuracy);
    }

    protected void UpdateColorValue()
    {
        var oldVal = Value;
        Value = srcImage.GetPixel(0, (int)Math.Round((srcImage.Height - 1) * (1.0f - pickerPos)));
        if ((oldVal != Value) && (_dragState != 1))
        {
            OnValueChanged();
        }
    }

    protected void OnValueChanged() => ValueChanged?.Invoke(this, null);
    protected void OnPercentualValueChanged() => PercentualValueChanged?.Invoke(this, null);

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Invalidate();
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var colorBoxOuter = new Rectangle(
            ClientRectangle.X + pickerSize,
            ClientRectangle.Y + pickerSize,
            ClientRectangle.Width - pickerSize * 2 - 1,
            ClientRectangle.Height - pickerSize * 2 - 1);
        var colorBoxInner = new Rectangle(
            colorBoxOuter.X + 1,
            colorBoxOuter.Y + 1,
            colorBoxOuter.Width - 2,
            colorBoxOuter.Height - 2);
        var colorArea = ColorAreaRectangle;
        var pickerVisualPos = colorArea.Y + (int)Math.Round((1.0f - pickerPos) * colorArea.Height);

        if (min.A < 255 || max.A < 255)
        {
            e.Graphics.FillRectangle(new HatchBrush(HatchStyle.LargeCheckerBoard, Color.LightGray, Color.Gray), colorArea);
        }

        var colorAreaImageAttr = new System.Drawing.Imaging.ImageAttributes();
        colorAreaImageAttr.SetWrapMode(WrapMode.TileFlipXY);
        e.Graphics.DrawImage(srcImage, colorArea, 0, 0, srcImage.Width, srcImage.Height - 1, GraphicsUnit.Pixel, colorAreaImageAttr);

        e.Graphics.DrawRectangle(SystemPens.ControlDark, colorBoxOuter);
        e.Graphics.DrawRectangle(SystemPens.ControlLightLight, colorBoxInner);

        using (var brush = new SolidBrush(Enabled ? ForeColor : Color.FromKnownColor(KnownColor.DimGray)))
        {
            e.Graphics.FillPolygon(brush, new Point[] {
                new Point(0, pickerVisualPos - pickerSize),
                new Point(pickerSize, pickerVisualPos),
                new Point(0, pickerVisualPos + pickerSize),
                new Point(0, pickerVisualPos - pickerSize)});
            e.Graphics.FillPolygon(brush, new Point[] {
                new Point(colorBoxOuter.Right + pickerSize, pickerVisualPos - pickerSize),
                new Point(colorBoxOuter.Right, pickerVisualPos),
                new Point(colorBoxOuter.Right + pickerSize, pickerVisualPos + pickerSize),
                new Point(colorBoxOuter.Right + pickerSize, pickerVisualPos - pickerSize)});
        }

        if (ShowInnerPicker)
        {
            var innerPickerPen = Value.GetLuminance() > 0.5f ? Pens.Black : Pens.White;

            e.Graphics.DrawLine(innerPickerPen,
                new Point(colorArea.Left, pickerVisualPos),
                new Point(colorArea.Left + 2, pickerVisualPos));
            e.Graphics.DrawLine(innerPickerPen,
                new Point(colorArea.Right - 1, pickerVisualPos),
                new Point(colorArea.Right - 1 - 2, pickerVisualPos));
        }

        if (!Enabled)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(128, SystemColors.Control)), colorArea);
        }
    }
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _dragState = 1;
            Focus();
            ValuePercentual = 1.0f - (e.Y - ColorAreaRectangle.Y) / (float)ColorAreaRectangle.Height;
        }
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_dragState != 1)
        {
            return;
        }

        ValuePercentual = 1.0f - (e.Y - ColorAreaRectangle.Y) / (float)ColorAreaRectangle.Height;
    }
    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        _dragState = 2;
        ValuePercentual = 1.0f - (e.Y - ColorAreaRectangle.Y) / (float)ColorAreaRectangle.Height;
        _dragState = 0;
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        _dragState = 0;
        Invalidate();
    }
    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    private void ResetMinimum()
    {
        SetupHueGradient();
        designSerializeColor = false;
    }
    private void ResetMaximum()
    {
        SetupHueGradient();
        designSerializeColor = false;
    }
    private bool ShouldSerializeMinimum() => designSerializeColor;
    private bool ShouldSerializeMaximum() => designSerializeColor;
}


