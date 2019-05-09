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
// Created: May 9, 2019 12:38:19 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics.Core;
using Gorgon.UI;

namespace Gorgon.Editor.UI.Views
{
    /// <summary>
    /// The base form used for krypton tool plug ins.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Developers who use to create tool plug ins for the editor should use this form as the base for their UI.  It provides functionality to make setting up and rendering easier, and will perform any 
    /// necessary clean up on behalf of the developer.
    /// </para>
    /// </remarks>
    public partial class EditorToolBaseForm
        : KryptonForm
    {
        #region Variables.
        // The control that will receive rendering output.
        private Control _renderControl;
        // Previous idle method.
        private WeakReference<Func<bool>> _oldIdle;
        // The previous flag for background operation.
        private bool _oldBackgroundState;
        // The swap chain for the render control.
        private GorgonSwapChain _swapChain;
        // The application graphics context.
        private IGraphicsContext _graphicsContext;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether the form is in designer mode or not.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected bool IsDesignTime
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the swap chain for this window.
        /// </summary>
        /// <remarks>
        /// This property will return <b>null</b> until the <see cref="SetupGraphics"/> method is called.
        /// </remarks>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected GorgonSwapChain SwapChain
        {
            get => _swapChain;
            private set => _swapChain = value;
        }

        /// <summary>
        /// Property to return the application graphics context for the window.
        /// </summary>
        /// <remarks>
        /// This property will return <b>null</b> until the <see cref="SetupGraphics"/> method is called.
        /// </remarks>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected IGraphicsContext GraphicsContext
        {
            get => _graphicsContext;
            private set => _graphicsContext = value;
        }

        /// <summary>
        /// Property to set or return the control that will receive the rendering output from the swap chain.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property can only be set through the designer.  Setting it at runtime will do nothing.
        /// </para>
        /// </remarks>
        [Browsable(true), Description("Sets the control to receive the rendering output from the swap chain."), Category("Rendering"), DefaultValue(null), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Control RenderControl
        {
            get => _renderControl;
            set
            {
                if ((_renderControl == value) && (!IsDesignTime))
                {
                    return;
                }

                _renderControl = value;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to perform idle time processing.
        /// </summary>
        /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
        private bool Idle()
        {
            Func<bool> oldIdle = null;

            SwapChain.RenderTargetView.Clear(BackColor);

            if (GraphicsContext.Graphics.RenderTargets[0] != SwapChain.RenderTargetView)
            {
                GraphicsContext.Graphics.SetRenderTarget(SwapChain.RenderTargetView);
            }

            OnRender();

            // Render on other controls as well.
            if (GorgonApplication.AllowBackground)
            {
                _oldIdle?.TryGetTarget(out oldIdle);
                oldIdle?.Invoke();
            }

            SwapChain.Present(1);

            return true;
        }

        /// <summary>
        /// Function to shut down the graphics interface for this window.
        /// </summary>
        private void ShutdownGraphics()
        {
            if (_graphicsContext == null)
            {
                return;
            }

            OnShutdownGraphics();

            GorgonSwapChain swapChain = Interlocked.Exchange(ref _swapChain, null);
            IGraphicsContext context = Interlocked.Exchange(ref _graphicsContext, null);

            Func<bool> oldIdle = null;

            _oldIdle?.TryGetTarget(out oldIdle);
            GorgonApplication.IdleMethod = oldIdle;
            GorgonApplication.AllowBackground = _oldBackgroundState;
            
            if (swapChain != null)
            {
                context?.ReturnSwapPresenter(ref swapChain);
            }
        }

        /// <summary>
        /// Function to perform custom rendering using the graphics functionality.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is called once per frame to allow for custom graphics drawing on the <see cref="RenderControl"/>.
        /// </para>
        /// <para>
        /// Tool plug in implementors need to override this in order to perform custom rendering with the <see cref="GraphicsContext"/>. 
        /// </para>
        /// </remarks>
        protected virtual void OnRender()
        {
        }

        /// <summary>
        /// Function to perform custom graphics set up.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method allows tool plug in implementors to setup additional functionality for custom graphics rendering.
        /// </para>
        /// <para>
        /// Resources created by this method should be cleaned up in the <see cref="OnShutdownGraphics"/> method.
        /// </para>
        /// <para>
        /// Implementors do not need to set up a <see cref="SwapChain"/> since one is already provided.
        /// </para>
        /// </remarks>
        /// <seealso cref="OnShutdownGraphics"/>
        protected virtual void OnSetupGraphics()
        {

        }

        /// <summary>
        /// Function to perform clean up when graphics operations are shutting down.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Any resources created in the <see cref="OnSetupGraphics"/> method must be disposed of here.
        /// </para>
        /// </remarks>
        protected virtual void OnShutdownGraphics()
        {

        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"/> event.</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"/> that contains the event data.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            ShutdownGraphics();
        }

        /// <summary>
        /// Function to set up the graphics interface for this window.
        /// </summary>
        /// <param name="context">The application graphics context provided by the plug in.</param>
        /// <param name="allowBackgroundRendering">[Optional] <b>true</b> to allow the graphics functionality to render even if the form does not have focus, <b>false</b> to pause rendering.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="context"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <see cref="RenderControl"/> property is not set to a non-<b>null</b> value.</exception>
        /// <remarks>
        /// <para>
        /// This method will initialize the graphics sub system for the control (if required) so that users may use the Gorgon drawing functionality in their tool plug in. The method should be called 
        /// immediately after creating the form to reduce any possibility of issues.
        /// </para>
        /// <para>
        /// Implementors who wish to perform their own graphics initialization may override the <see cref="OnSetupGraphics"/> method for setup, and <see cref="OnShutdownGraphics"/> method for clean up.
        /// </para>
        /// <para>
        /// Prior to calling this method, the <see cref="RenderControl"/> property <b>must</b> be set or an exception will be thrown.
        /// </para>
        /// </remarks>
        public void SetupGraphics(IGraphicsContext context, bool allowBackgroundRendering = true)
        {
            if (GraphicsContext != null)
            {
                return;
            }

            if (RenderControl == null)
            {
                throw new GorgonException(GorgonResult.NotInitialized, Resources.GOREDIT_ERR_NO_RENDER_CONTROL);
            }

            GraphicsContext = context ?? throw new ArgumentNullException(nameof(context));

            SwapChain = context.LeaseSwapPresenter(RenderControl);

            OnSetupGraphics();

            if (GorgonApplication.IdleMethod != null)
            {
                _oldIdle = new WeakReference<Func<bool>>(GorgonApplication.IdleMethod);
            }
            else
            {
                _oldIdle = null;
            }

            _oldBackgroundState = GorgonApplication.AllowBackground;

            GorgonApplication.AllowBackground = true;
            GorgonApplication.IdleMethod = Idle;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="EditorToolBaseForm"/> class.</summary>
        public EditorToolBaseForm()
        {
            IsDesignTime = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

            InitializeComponent();
        }
        #endregion
    }
}
