#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: October 29, 2018 4:12:28 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics.Core;
using Gorgon.UI;
using ComponentFactory.Krypton.Toolkit;

namespace Gorgon.Editor.UI.Views
{
    /// <summary>
    /// The base control used to render content.
    /// </summary>
    public partial class ContentBaseControl 
        : EditorBaseControl, IRendererControl
    {
        #region Variables.
        // The swap chain for the control.
        private GorgonSwapChain _swapChain;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the idle method for rendering on the control.
        /// </summary>
        /// <remarks>
        /// The <see cref="Stop"/> method must be called prior to switching idle methods.
        /// </remarks>
        protected Func<bool> IdleMethod
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the panel that will be used for rendering.
        /// </summary>
        [Browsable(false)]
        public KryptonPanel RenderPanel => PanelRenderer;

        /// <summary>Property to return the graphics context.</summary>
        [Browsable(false)]
        public IGraphicsContext GraphicsContext
        {
            get;
            private set;
        }

        /// <summary>Property to return the swap chain assigned to the control.</summary>
        [Browsable(false)]
        public GorgonSwapChain SwapChain => _swapChain;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to allow user defined setup of the graphics context with this control.
        /// </summary>
        /// <param name="context">The context being assigned.</param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="context"/> is <b>null</b>, then applications should use this method to shut down/dispose of any created graphics items.
        /// </para>
        /// </remarks>
        protected virtual void OnSetupGraphics(IGraphicsContext context)
        {

        }

        /// <summary>
        /// Function to begin rendering on the control.
        /// </summary>
        public void Start() => GorgonApplication.IdleMethod = IdleMethod;

        /// <summary>
        /// Function to cease rendering on the control.
        /// </summary>
        public void Stop() => GorgonApplication.IdleMethod = null;

        /// <summary>
        /// Function to initialize the graphics context for the control.
        /// </summary>
        /// <param name="context">The graphics context to use.</param>
        /// <remarks>
        /// <para>
        /// This method assigns the swap chain to the control, so the user does not need to set one up on control creation.
        /// </para>
        /// </remarks>
        public void SetupGraphics(IGraphicsContext context)
        {
            // This should not be executing when designing.
            if (IsDesignTime)
            {
                return;
            }

            if ((GraphicsContext != null) && (_swapChain != null))
            {
                GraphicsContext.ReturnSwapPresenter(ref _swapChain);
            }

            if (context != null)
            {
                _swapChain = context.LeaseSwapPresenter(PanelRenderer);
            }            

            OnSetupGraphics(context);

            GraphicsContext = context;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ContentBaseControl class.</summary>
        public ContentBaseControl() => InitializeComponent();
        #endregion
    }
}
