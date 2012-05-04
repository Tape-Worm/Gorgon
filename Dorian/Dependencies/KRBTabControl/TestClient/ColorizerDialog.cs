using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestClient
{
    public partial class ColorizerDialog : Form
    {
        private KRBTabControl.KRBTabControl.RandomizerCaption colorizer = null;
        
        public ColorizerDialog()
        {
            InitializeComponent();
        }

        public ColorizerDialog(KRBTabControl.KRBTabControl.RandomizerCaption colorizer)
            : this()
        {
            this.colorizer = colorizer;
        }

        public KRBTabControl.KRBTabControl.RandomizerCaption Colorizer
        {
            get
            {
                return colorizer = new KRBTabControl.KRBTabControl.RandomizerCaption((byte)redTrackBar.Value, (byte)greenTrackBar.Value,
                    (byte)blueTrackBar.Value, (byte)alphaTrackBar.Value, colorizer.IsRandomizerEnabled, colorizer.IsTransparencyEnabled);
            }
        }

        private void TRACKBAR_ValueChanged(object sender, EventArgs e)
        {
            if (sender is TrackBar)
            {
                string result = null;
                TrackBar ctrl = (TrackBar)sender;
                if (ctrl.Value == 0)
                    result = "Min";
                else if (ctrl.Value == 255)
                    result = "Max";

                switch (ctrl.Name)
                {
                    case "redTrackBar":
                        lblRed.Text = String.Format("Red: {0}", result ?? ctrl.Value.ToString());
                        break;
                    case "greenTrackBar":
                        lblGreen.Text = String.Format("Green: {0}", result ?? ctrl.Value.ToString());
                        break;
                    case "blueTrackBar":
                        lblBlue.Text = String.Format("Blue: {0}", result ?? ctrl.Value.ToString());
                        break;
                    default:
                        lblAlpha.Text = String.Format("Alpha: {0}", result ?? ctrl.Value.ToString());
                        break;
                }
            }
        }

        private void ColorizerDialog_Load(object sender, EventArgs e)
        {
            redTrackBar.Value = colorizer.Red;
            greenTrackBar.Value = colorizer.Green;
            blueTrackBar.Value = colorizer.Blue;
            alphaTrackBar.Value = colorizer.Transparency;
        }
    }
}
