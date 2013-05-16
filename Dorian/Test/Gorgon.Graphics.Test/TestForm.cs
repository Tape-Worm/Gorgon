using System.Windows.Forms;

namespace GorgonLibrary.Graphics.Test
{
    public partial class TestForm : Form
    {
        public DialogResult TestResult
        {
            get;
            private set;
        }

        public bool ShowTestPanel
        {
            get
            {
                return panelInput.Visible;
            }
            set
            {
                TestResult = DialogResult.None;
                panelInput.Visible = value;
            }
        }
        
        public TestForm()
        {
            InitializeComponent();
        }

        private void buttonYes_Click(object sender, System.EventArgs e)
        {
            TestResult = DialogResult.Yes;
            Close();
        }

        private void buttonWrong_Click(object sender, System.EventArgs e)
        {
            TestResult = DialogResult.No;
            Close();
        }
    }
}
