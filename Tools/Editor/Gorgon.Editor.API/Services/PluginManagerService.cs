using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Editor.UI.Forms;
using Gorgon.UI;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A service used to allow the management of plug ins.
    /// </summary>
    public class PluginManagerService
		: IPlugInManagerService
    {
        #region Variables.

        #endregion

        #region Properties.

        #endregion

        #region Methods.
        /// <summary>Function to show the plug in manager interface.</summary>
        /// <returns>
        ///   <b>true</b> if the changes were accepted, <b>false</b> if not.</returns>
        public bool Show()
        {
            using (var form = new FormPlugInManager())
            {
                if (form.ShowDialog(GorgonApplication.MainForm) != DialogResult.OK)
                {
                    return false;
                }

                return true;
            }
        }
        #endregion

        #region Constructor/Finalizer.

        #endregion
    }
}
