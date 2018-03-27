using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Examples
{
    /// <summary>
    /// GorgonFlatForm example.
    /// 
    /// This is an example of the Gorgon "Flat Form" look.  This was originally built to emulate the look of the Microsoft Zune style window, however with the arrival of Windows 10, this look is now part 
    /// of the operating system, so the value of this is minimal and is left here merely as a curiosity.
    ///  
    /// However, unlike the Windows 10 window, this window can be themed to match a color scheme that better suits your application.  You can choose the window decoration color, and other element colors. 
    /// It will even theme any of the toolstrip controls that come with .NET (other controls however are left to the developer to override).  Application can override the theme via the Theme property. 
    /// Themes can also be saved to and loaded from an XML file containing the theme settings.
    /// 
    /// To use it:
    /// * Create a Windows Forms application.
    /// * Reference Gorgon.Core and Gorgon.Windows
    /// * Open the form code (e.g. form1.cs) and replace the inherited Form with GorgonFlatForm:
    ///   public class Form1 : Form -> public class Form1 : GorgonFlatForm
    /// 
    /// And that's it.  
    /// 
    /// The window is pretty much a standard window with an Icon, Minimize, Maximize and Close buttons (all of which can be disabled).  Below the title bar, there's a client area panel where you can drop 
    /// in any controls you'd like to flesh out the control.
    /// 
    /// The border is not visible by default (although the mouse will react as though there is a border, and this is affected by the ResizeHandleSize property). To show a border the user must set the 
    /// ShowBorder flag to TRUE, and increase the BorderSize and Padding properties to the desired size(s).
    /// </summary>
    public partial class FormExample 
        : GorgonFlatForm
    {
        #region Variables.
        // Flag used to switch form themes.
        private bool _isOriginalTheme = true;
        #endregion

        #region Methods.
        /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // This allows us the chance to update our size based on display DPI while keeping the designer from breaking everything.
            SuspendLayout();
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96, 96);
            ResumeLayout(true);

            // We'll need to reposition as our form size will change on higher DPI monitors.
            Location = new Point(Screen.FromControl(this).Bounds.Width / 2 - Size.Width / 2, Screen.FromControl(this).Bounds.Height / 2 - Size.Height / 2);
        }

        /// <summary>
        /// Function to load a theme from an XML file on disk and set it on the form.
        /// </summary>
        private void ChangeTheme()
        {
            // Pick which file we're going to open.
            string fileName = _isOriginalTheme ? "OriginalTheme.xml" : "DarkTheme.xml";
            
            // The FormatDirectory method is one of the extension methods available in Gorgon to help handle correct formatting and validation of paths.
            using (Stream stream = File.Open(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath) ?? throw new DirectoryNotFoundException(), "Themes")
                                                 .FormatDirectory(Path.DirectorySeparatorChar) + fileName, FileMode.Open, FileAccess.Read))
            {
                // Setting the theme is incredibly simple.
                Theme = GorgonFlatFormTheme.Load(stream);
            }

            // Unfortunately, some controls cannot have their colors changed automatically...
            ButtonThemeColors.BackColor = Theme.WindowBackground;
            ButtonThemeColors.FlatAppearance.MouseOverBackColor = _isOriginalTheme ? Color.FromKnownColor(KnownColor.ControlLight) : Theme.WindowBorderInactive;
            ButtonThemeColors.FlatAppearance.MouseDownBackColor = _isOriginalTheme ? Color.FromKnownColor(KnownColor.ControlDark) : Theme.WindowBorderActive;
        }

        /// <summary>
        /// Handles the Click event of the ButtonThemeColors control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonThemeColors_Click(object sender, EventArgs e)
        {
            _isOriginalTheme = !_isOriginalTheme;
            ChangeTheme();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="FormExample"/> class.
        /// </summary>
        public FormExample()
        {
            InitializeComponent();
        }
        #endregion
    }
}
