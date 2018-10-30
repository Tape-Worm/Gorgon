using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.Content;
using Gorgon.Editor.UI.Views;

namespace Gorgon.Editor.ImageEditor
{
    internal class TestContent
        : EditorContentCommon
    {
        protected override ContentBaseControl OnGetView()
        {
            return new TestView();
        }
    }
}
