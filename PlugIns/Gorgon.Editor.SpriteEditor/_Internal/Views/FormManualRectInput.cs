#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: March 23, 2019 9:42:34 PM
// 
#endregion

using ComponentFactory.Krypton.Toolkit;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The window for the manual rectangle input interface.
    /// </summary>
    internal partial class FormManualRectInput 
        : KryptonForm, IDataContext<IManualRectInputVm>
    {
        #region Variables.

        #endregion

        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        public IManualRectInputVm DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to reset the data context.
        /// </summary>
        private void ResetDataContext() => ManualInput.SetDataContext(null);

        /// <summary>
        /// Function to initialize the view based on the data context.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        private void InitializeDataContext(IManualRectInputVm dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }            
        }        

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IManualRectInputVm dataContext)
        {
            InitializeDataContext(dataContext);
            DataContext = dataContext;

            ManualInput.SetDataContext(dataContext);            
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.FormManualRectInput"/> class.</summary>
        public FormManualRectInput() => InitializeComponent();        
        #endregion
    }
}
