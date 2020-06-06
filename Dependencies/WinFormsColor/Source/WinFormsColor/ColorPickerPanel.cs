using System;
using System.CodeDom.Compiler;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Fetze.WinFormsColor
{
	public partial class ColorPickerPanel 
		: UserControl
	{
		public enum PrimaryAttrib
		{
			Hue,
			Saturation,
			Brightness,
			Red,
			Green,
			Blue
		}

		private struct InternalColor
		{
			public float H;
			public float S;
			public float V;
			public float A;

			public InternalColor(Color c)
			{
				H = c.GetHSVHue();
				S = c.GetHSVSaturation();
				V = c.GetHSVBrightness();
				A = c.A / 255.0f;
			}

            public Color ToColor() => Color.FromArgb((int)Math.Round(A * 255.0f), ExtMethodsSystemDrawingColor.ColorFromHSV(H, S, V));
        }

		private bool _alphaEnabled = true;
		private InternalColor _oldColor = new InternalColor(Color.Red);
		private InternalColor _selColor = new InternalColor(Color.Red);
		private PrimaryAttrib _primAttrib = PrimaryAttrib.Hue;
		private bool _suspendTextEvents;

        /// <summary>
        /// Event fired when the color has changed.
        /// </summary>
	    public event EventHandler ColorChanged;

        /// <summary>
        /// Event fired when the old color has changed.
        /// </summary>
	    public event EventHandler OldColorChanged;

		public bool AlphaEnabled
        {
            get => _alphaEnabled;
            set
            {
                _alphaEnabled = value;
                alphaSlider.Enabled = _alphaEnabled;
                numAlpha.Enabled = _alphaEnabled;
            }
        }
        public Color OldColor
        {
            get => _oldColor.ToColor();
            set
            {
                _oldColor = new InternalColor(value);
                UpdateColorShowBox();
                OldColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public Color SelectedColor
        {
            get => _selColor.ToColor();
            set
            {
                _selColor = new InternalColor(value);
                UpdateColorControls();
                ColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public PrimaryAttrib PrimaryAttribute
        {
            get => _primAttrib;
            set
            {
                _primAttrib = value;
                UpdateColorControls();
            }
        }

        public ColorPickerPanel() => InitializeComponent();

        private void UpdateColorControls()
		{
			UpdatePrimaryAttributeRadioBox();
			UpdateText();
			UpdateColorShowBox();
			UpdateColorPanelGradient();
			UpdateColorSliderGradient();
			UpdateAlphaSliderGradient();
			UpdateColorPanelValue();
			UpdateColorSliderValue();
			UpdateAlphaSliderValue();
		}
		private void UpdatePrimaryAttributeRadioBox()
		{
			switch (_primAttrib)
			{
				default:
					radioHue.Checked = true;
					break;
				case PrimaryAttrib.Saturation:
					radioSaturation.Checked = true;
					break;
				case PrimaryAttrib.Brightness:
					radioValue.Checked = true;
					break;
				case PrimaryAttrib.Red:
					radioRed.Checked = true;
					break;
				case PrimaryAttrib.Green:
					radioGreen.Checked = true;
					break;
				case PrimaryAttrib.Blue:
					radioBlue.Checked = true;
					break;
			}
		}
		private void UpdateText()
		{
			var tmp = _selColor.ToColor();
			_suspendTextEvents = true;

			textBoxHex.Text = string.Format("{0:X}", tmp.ToArgb());

			numRed.Value = tmp.R;
			numGreen.Value = tmp.G;
			numBlue.Value = tmp.B;
			numAlpha.Value = tmp.A;

			numHue.Value = (decimal)(_selColor.H * 360.0f);
			numSaturation.Value = (decimal)(_selColor.S * 100.0f);
			numValue.Value = (decimal)(_selColor.V * 100.0f);

			_suspendTextEvents = false;
		}
		private void UpdateColorShowBox()
		{
			colorShowBox.UpperColor = _alphaEnabled ? _oldColor.ToColor() : Color.FromArgb(255, _oldColor.ToColor());
			colorShowBox.LowerColor = _alphaEnabled ? _selColor.ToColor() : Color.FromArgb(255, _selColor.ToColor());
		}
		private void UpdateColorPanelGradient()
		{
			Color tmp;
			switch (_primAttrib)
			{
				default:
					colorPanel.SetupXYGradient(
						Color.White,
						ExtMethodsSystemDrawingColor.ColorFromHSV(_selColor.H, 1.0f, 1.0f),
						Color.Black,
						Color.Transparent);
					break;
				case PrimaryAttrib.Saturation:
					colorPanel.SetupHueBrightnessGradient(_selColor.S);
					break;
				case PrimaryAttrib.Brightness:
					colorPanel.SetupHueSaturationGradient(_selColor.V);
					break;
				case PrimaryAttrib.Red:
					tmp = _selColor.ToColor();
					colorPanel.SetupGradient(
						Color.FromArgb(tmp.A, tmp.R, 255, 0),
                        Color.FromArgb(tmp.A, tmp.R, 255, 255),
                        Color.FromArgb(tmp.A, tmp.R, 0, 0),
                        Color.FromArgb(tmp.A, tmp.R, 0, 255),
						32);
					break;
				case PrimaryAttrib.Green:
					tmp = _selColor.ToColor();
					colorPanel.SetupGradient(
                        Color.FromArgb(tmp.A, 255, tmp.G, 0),
                        Color.FromArgb(tmp.A, 255, tmp.G, 255),
                        Color.FromArgb(tmp.A, 0, tmp.G, 0),
                        Color.FromArgb(tmp.A, 0, tmp.G, 255),
						32);
					break;
				case PrimaryAttrib.Blue:
					tmp = _selColor.ToColor();
					colorPanel.SetupGradient(
                        Color.FromArgb(tmp.A, 255, 0, tmp.B),
                        Color.FromArgb(tmp.A, 255, 255, tmp.B),
                        Color.FromArgb(tmp.A, 0, 0, tmp.B),
                        Color.FromArgb(tmp.A, 0, 255, tmp.B),
						32);
					break;
			}
		}
		private void UpdateColorPanelValue()
		{
			Color tmp;
			switch (_primAttrib)
			{
				default:
					colorPanel.ValuePercentual = new PointF(
						_selColor.S,
						_selColor.V);
					break;
				case PrimaryAttrib.Saturation:
					colorPanel.ValuePercentual = new PointF(
						_selColor.H,
						_selColor.V);
					break;
				case PrimaryAttrib.Brightness:
					colorPanel.ValuePercentual = new PointF(
						_selColor.H,
						_selColor.S);
					break;
				case PrimaryAttrib.Red:
					tmp = _selColor.ToColor();
					colorPanel.ValuePercentual = new PointF(
						tmp.B / 255.0f,
						tmp.G / 255.0f);
					break;
				case PrimaryAttrib.Green:
					tmp = _selColor.ToColor();
					colorPanel.ValuePercentual = new PointF(
						tmp.B / 255.0f,
						tmp.R / 255.0f);
					break;
				case PrimaryAttrib.Blue:
					tmp = _selColor.ToColor();
					colorPanel.ValuePercentual = new PointF(
						tmp.G / 255.0f,
						tmp.R / 255.0f);
					break;
			}
		}
		private void UpdateColorSliderGradient()
		{
			Color tmp;
			switch (_primAttrib)
			{
				default:
					colorSlider.SetupHueGradient(/*this.selColor.GetHSVSaturation(), this.selColor.GetHSVBrightness()*/);
					break;
				case PrimaryAttrib.Saturation:
					colorSlider.SetupGradient(
						ExtMethodsSystemDrawingColor.ColorFromHSV(_selColor.H, 0.0f, _selColor.V),
						ExtMethodsSystemDrawingColor.ColorFromHSV(_selColor.H, 1.0f, _selColor.V));
					break;
				case PrimaryAttrib.Brightness:
					colorSlider.SetupGradient(
						ExtMethodsSystemDrawingColor.ColorFromHSV(_selColor.H, _selColor.S, 0.0f),
						ExtMethodsSystemDrawingColor.ColorFromHSV(_selColor.H, _selColor.S, 1.0f));
					break;
				case PrimaryAttrib.Red:
					tmp = _selColor.ToColor();
					colorSlider.SetupGradient(
						Color.FromArgb(255, 0, tmp.G, tmp.B),
						Color.FromArgb(255, 255, tmp.G, tmp.B));
					break;
				case PrimaryAttrib.Green:
					tmp = _selColor.ToColor();
					colorSlider.SetupGradient(
						Color.FromArgb(255, tmp.R, 0, tmp.B),
						Color.FromArgb(255, tmp.R, 255, tmp.B));
					break;
				case PrimaryAttrib.Blue:
					tmp = _selColor.ToColor();
					colorSlider.SetupGradient(
						Color.FromArgb(255, tmp.R, tmp.G, 0),
						Color.FromArgb(255, tmp.R, tmp.G, 255));
					break;
			}
		}
		private void UpdateColorSliderValue()
		{
			Color tmp;
			switch (_primAttrib)
			{
				default:
					colorSlider.ValuePercentual = _selColor.H;
					break;
				case PrimaryAttrib.Saturation:
					colorSlider.ValuePercentual = _selColor.S;
					break;
				case PrimaryAttrib.Brightness:
					colorSlider.ValuePercentual = _selColor.V;
					break;
				case PrimaryAttrib.Red:
					tmp = _selColor.ToColor();
					colorSlider.ValuePercentual = tmp.R / 255.0f;
					break;
				case PrimaryAttrib.Green:
					tmp = _selColor.ToColor();
					colorSlider.ValuePercentual = tmp.G / 255.0f;
					break;
				case PrimaryAttrib.Blue:
					tmp = _selColor.ToColor();
					colorSlider.ValuePercentual = tmp.B / 255.0f;
					break;
			}
		}
        private void UpdateAlphaSliderGradient() => alphaSlider.SetupGradient(Color.Transparent, Color.FromArgb(255, _selColor.ToColor()));
        private void UpdateAlphaSliderValue() => alphaSlider.ValuePercentual = _selColor.A;

        private void UpdateSelectedColorFromSliderValue()
		{
			Color tmp;
			switch (_primAttrib)
			{
				default:
					_selColor.H = colorSlider.ValuePercentual;
					break;
				case PrimaryAttrib.Saturation:
					_selColor.S = colorSlider.ValuePercentual;
					break;
				case PrimaryAttrib.Brightness:
					_selColor.V = colorSlider.ValuePercentual;
					break;
				case PrimaryAttrib.Red:
					tmp = _selColor.ToColor();
					_selColor = new InternalColor(Color.FromArgb(
						tmp.A, 
						(int)Math.Round(colorSlider.ValuePercentual * 255.0f), 
						tmp.G, 
						tmp.B));
					break;
				case PrimaryAttrib.Green:
					tmp = _selColor.ToColor();
					_selColor = new InternalColor(Color.FromArgb(
						tmp.A, 
						tmp.R, 
						(int)Math.Round(colorSlider.ValuePercentual * 255.0f), 
						tmp.B));
					break;
				case PrimaryAttrib.Blue:
					tmp = _selColor.ToColor();
					_selColor = new InternalColor(Color.FromArgb(
						tmp.A, 
						tmp.R, 
						tmp.G, 
						(int)Math.Round(colorSlider.ValuePercentual * 255.0f)));
					break;
			}

            ColorChanged?.Invoke(this, EventArgs.Empty);
        }
		private void UpdateSelectedColorFromPanelValue()
		{
			Color tmp;
			switch (_primAttrib)
			{
				default:
					_selColor.S = colorPanel.ValuePercentual.X;
					_selColor.V = colorPanel.ValuePercentual.Y;
					break;
				case PrimaryAttrib.Saturation:
					_selColor.H = colorPanel.ValuePercentual.X;
					_selColor.V = colorPanel.ValuePercentual.Y;
					break;
				case PrimaryAttrib.Brightness:
					_selColor.H = colorPanel.ValuePercentual.X;
					_selColor.S = colorPanel.ValuePercentual.Y;
					break;
				case PrimaryAttrib.Red:
					tmp = _selColor.ToColor();
					_selColor = new InternalColor(Color.FromArgb(
						tmp.A, 
						tmp.R, 
						(int)Math.Round(colorPanel.ValuePercentual.Y * 255.0f), 
						(int)Math.Round(colorPanel.ValuePercentual.X * 255.0f)));
					break;
				case PrimaryAttrib.Green:
					tmp = _selColor.ToColor();
					_selColor = new InternalColor(Color.FromArgb(
						tmp.A, 
						(int)Math.Round(colorPanel.ValuePercentual.Y * 255.0f), 
						tmp.G, 
						(int)Math.Round(colorPanel.ValuePercentual.X * 255.0f)));
					break;
				case PrimaryAttrib.Blue:
					tmp = _selColor.ToColor();
					_selColor = new InternalColor(Color.FromArgb(
						tmp.A, 
						(int)Math.Round(colorPanel.ValuePercentual.Y * 255.0f), 
						(int)Math.Round(colorPanel.ValuePercentual.X * 255.0f),
						tmp.B));
					break;
			}

            ColorChanged?.Invoke(this, EventArgs.Empty);
        }
		private void UpdateSelectedColorFromAlphaValue()
		{
			_selColor.A = alphaSlider.ValuePercentual;

            ColorChanged?.Invoke(this, EventArgs.Empty);
        }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			_selColor = _oldColor;
			UpdateColorControls();
		}

        private void SetStates(RadioButton button)
        {
            radioHue.Checked = button == radioHue;
            radioSaturation.Checked = button == radioSaturation;
            radioValue.Checked = button == radioValue;

            radioRed.Checked = button == radioRed;
            radioGreen.Checked = button == radioGreen;
            radioBlue.Checked = button == radioBlue;
        }

		private void RadioHue_CheckedChanged(object sender, EventArgs e)
		{
            if (radioHue.Checked)
            {
                PrimaryAttribute = PrimaryAttrib.Hue;
                SetStates(radioHue);
            }
		}
		private void RadioSaturation_CheckedChanged(object sender, EventArgs e)
		{
            if (radioSaturation.Checked)
            {
                PrimaryAttribute = PrimaryAttrib.Saturation;
                SetStates(radioSaturation);
            }
        }
		private void RadioValue_CheckedChanged(object sender, EventArgs e)
		{
            if (radioValue.Checked)
            {
                PrimaryAttribute = PrimaryAttrib.Brightness;
                SetStates(radioValue);
            }
        }
		private void RadioRed_CheckedChanged(object sender, EventArgs e)
		{
            if (radioRed.Checked)
            {
                PrimaryAttribute = PrimaryAttrib.Red;
                SetStates(radioRed);
            }
        }
		private void RadioGreen_CheckedChanged(object sender, EventArgs e)
		{
            if (radioGreen.Checked)
            {
                PrimaryAttribute = PrimaryAttrib.Green;
                SetStates(radioGreen);
            }
        }
		private void RadioBlue_CheckedChanged(object sender, EventArgs e)
		{
            if (radioBlue.Checked)
            {
                PrimaryAttribute = PrimaryAttrib.Blue;
                SetStates(radioBlue);
            }
        }

		private void ColorPanel_PercentualValueChanged(object sender, EventArgs e)
		{
			if (ContainsFocus)
            {
                UpdateSelectedColorFromPanelValue();
            }

            UpdateColorSliderGradient();
			UpdateAlphaSliderGradient();
			UpdateColorShowBox();
			UpdateText();
		}
		private void ColorSlider_PercentualValueChanged(object sender, EventArgs e)
		{
			if (ContainsFocus)
            {
                UpdateSelectedColorFromSliderValue();
            }

            UpdateColorPanelGradient();
			UpdateAlphaSliderGradient();
			UpdateColorShowBox();
			UpdateText();
		}
		private void AlphaSlider_PercentualValueChanged(object sender, EventArgs e)
		{
			if (ContainsFocus)
            {
                UpdateSelectedColorFromAlphaValue();
            }

            UpdateColorSliderGradient();
			UpdateColorPanelGradient();
			UpdateColorShowBox();
			UpdateText();
		}

		private void NumHue_ValueChanged(object sender, EventArgs e)
		{
			if (_suspendTextEvents)
            {
                return;
            }

            _selColor.H = (float)numHue.Value / 360.0f;
			UpdateColorControls();
		}
		private void NumSaturation_ValueChanged(object sender, EventArgs e)
		{
			if (_suspendTextEvents)
            {
                return;
            }

            _selColor.S = (float)numSaturation.Value / 100.0f;
			UpdateColorControls();
		}
		private void NumValue_ValueChanged(object sender, EventArgs e)
		{
			if (_suspendTextEvents)
            {
                return;
            }

            _selColor.V = (float)numValue.Value / 100.0f;
			UpdateColorControls();
		}
		private void NumRed_ValueChanged(object sender, EventArgs e)
		{
			if (_suspendTextEvents)
            {
                return;
            }

            var tmp = _selColor.ToColor();
			_selColor = new InternalColor(Color.FromArgb(
				tmp.A,
				(byte)numRed.Value,
				tmp.G,
				tmp.B));
			UpdateColorControls();

            ColorChanged?.Invoke(this, EventArgs.Empty);
        }
		private void NumGreen_ValueChanged(object sender, EventArgs e)
		{
			if (_suspendTextEvents)
            {
                return;
            }

            var tmp = _selColor.ToColor();
			_selColor = new InternalColor(Color.FromArgb(
				tmp.A,
				tmp.R,
				(byte)numGreen.Value,
				tmp.B));
			UpdateColorControls();

            ColorChanged?.Invoke(this, EventArgs.Empty);
        }
		private void NumBlue_ValueChanged(object sender, EventArgs e)
		{
			if (_suspendTextEvents)
            {
                return;
            }

            var tmp = _selColor.ToColor();
			_selColor = new InternalColor(Color.FromArgb(
				tmp.A,
				tmp.R,
				tmp.G,
				(byte)numBlue.Value));
			UpdateColorControls();

            ColorChanged?.Invoke(this, EventArgs.Empty);
        }
		private void NumAlpha_ValueChanged(object sender, EventArgs e)
		{
			if (_suspendTextEvents)
            {
                return;
            }

            var tmp = _selColor.ToColor();
			_selColor = new InternalColor(Color.FromArgb(
				(byte)numAlpha.Value,
				tmp.R,
				tmp.G,
				tmp.B));
			UpdateColorControls();

            ColorChanged?.Invoke(this, EventArgs.Empty);
        }
		private void TextBoxHex_TextChanged(object sender, EventArgs e)
		{
			if (_suspendTextEvents)
			{
				return;
			}


            if (!int.TryParse(textBoxHex.Text, NumberStyles.HexNumber, CultureInfo.CurrentUICulture, out int argb))
            {
                return;
            }

            var tmp = Color.FromArgb(argb);
			_selColor = new InternalColor(tmp);
			UpdateColorControls();

            ColorChanged?.Invoke(this, EventArgs.Empty);
        }

		private void ColorShowBox_UpperClick(object sender, EventArgs e)
		{
			_selColor = _oldColor;
			UpdateColorControls();
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }
	}
}
