using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace TestClient
{
    public class MyColorDialog : ColorDialog
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetWindowText(IntPtr hWnd, string lpString);

        private string title = string.Empty;
        private bool titleSet = false;

        /// <summary>
        /// Gets or sets the title that will be displayed on the dialog when it's shown.
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        [Description("The title that will be displayed on the dialog when it's shown.")]
        public string Title
        {
            get { return title; }
            set
            {
                if (value != null && value != title)
                {
                    title = value;
                    titleSet = false;
                }
            }
        }

        protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            if (!titleSet)
            {
                SetWindowText(hWnd, title);
                titleSet = true;
            }

            return base.HookProc(hWnd, msg, wparam, lparam);
        }
    }
}