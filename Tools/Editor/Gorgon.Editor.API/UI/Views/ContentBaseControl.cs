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
using ComponentFactory.Krypton.Ribbon;
using Gorgon.Editor.Content;

namespace Gorgon.Editor.UI.Views
{
    /// <summary>
    /// The base control used to render content.
    /// </summary>
    public partial class ContentBaseControl 
        : EditorBaseControl, IRendererControl
    {
        #region Events.
        /// <summary>
        /// Event triggered when the control is closing.
        /// </summary>
        public event EventHandler ControlClosing;
        #endregion

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
        /// Property to return the ribbon for the content view.
        /// </summary>
        [Browsable(false)]
        public KryptonRibbon Ribbon
        {
            get;
            protected set;
        }

        /// <summary>
        /// Property to set or return the control that will be rendered into using a <see cref="GorgonSwapChain"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Plug in developers should set this in the IDE designer to set up a swap chain for rendering when this control is created.
        /// </para>
        /// <para>
        /// If this property is assigned after control creation, the <see cref="SetupGraphics(IGraphicsContext)"/> method must be called again for it to take effect.
        /// </para>
        /// <para>
        /// If this value is set to <b>null</b>, then no swap chain will be created and the <see cref="SwapChain"/> property will be set to <b>null</b>.
        /// </para>
        /// </remarks>
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("Rendering"), Description("Sets or returns the custom control to use for Gorgon rendering.")]
        public Control RenderControl
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the panel that will be used for presentation of the content.
        /// </summary>
        [Browsable(false)]
        public KryptonPanel PresentationPanel => PanelPresenter;

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
        /// <param name="swapChain">The swap chain assigned to the <see cref="RenderControl"/>.</param>
        protected virtual void OnSetupGraphics(IGraphicsContext context, GorgonSwapChain swapChain)
        {
        }

        /// <summary>
        /// Function called to shut down the view and perform any clean up required (including user defined graphics objects).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Plug in developers do not need to clean up the <see cref="SwapChain"/> as it will be returned to the swap chain pool automatically.
        /// </para>
        /// </remarks>
        protected virtual void OnShutdown()
        {
        }

        /// <summary>
        /// Function to close the control.
        /// </summary>
        public void Close()
        {
            EventHandler handler = ControlClosing;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function to shut down the view.
        /// </summary>
        public void Shutdown()
        {
            Stop();

            OnShutdown();            

            // Return the swap chain to the pool.
            if (_swapChain != null)
            {
                GraphicsContext.ReturnSwapPresenter(ref _swapChain);
            }
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
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="context"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// If the <see cref="RenderControl"/> property is assigned on control creation, then a primary swap chain will be created for that control and provided via the <see cref="SwapChain"/> property.
        /// </para>
        /// </remarks>
        public void SetupGraphics(IGraphicsContext context)
        {
            // This should not be executing when designing.
            if (IsDesignTime)
            {
                return;
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // If we've made no change, then do nothing.
            if ((context == GraphicsContext) && (_swapChain != null) && (_swapChain.Window == RenderControl))
            {
                return;
            }

            if (_swapChain != null)
            {
                GraphicsContext.ReturnSwapPresenter(ref _swapChain);
            }

            GorgonSwapChain swapChain = null;

            // If we've defined a render control, then lease a swap chain from the swap chain pool.
            if ((context != null) && (RenderControl != null))
            {
                swapChain = context.LeaseSwapPresenter(RenderControl);
            }            

            OnSetupGraphics(context, swapChain);
            GraphicsContext = context;
            _swapChain = swapChain;
        }

        /// <summary>
        /// Function to assign the current content name.
        /// </summary>
        /// <param name="contentName">The name of the content.</param>
        public void SetContentName(string contentName)
        {
            if (string.IsNullOrWhiteSpace(contentName))
            {
                PanelContentName.Visible = false;
                return;
            }

            LabelHeader.Text = contentName;
            PanelContentName.Visible = true;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ContentBaseControl class.</summary>
        public ContentBaseControl() => InitializeComponent();
        #endregion
    }
}
