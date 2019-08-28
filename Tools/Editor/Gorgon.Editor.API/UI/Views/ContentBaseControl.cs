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
using System.Windows.Forms;
using ComponentFactory.Krypton.Ribbon;
using ComponentFactory.Krypton.Toolkit;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.GdiPlus;
using Gorgon.UI;

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
        // The data context for the editor context.
        private IEditorContent _dataContext;
        // A list of child panel views identified by name.
        private readonly Dictionary<string, Control> _panelViews = new Dictionary<string, Control>(StringComparer.OrdinalIgnoreCase);
        #endregion

        #region Events.
        /// <summary>
        /// Event triggered when a drag enter operation is to be bubbled up to its parent.
        /// </summary>
        [Category("Drag Drop"), Description("Notifies the parent control that the drag enter event has been passed to it from this control.")]
        public event EventHandler<DragEventArgs> BubbleDragEnter;
        /// <summary>
        /// Event triggered when a drag enter operation is to be bubbled up to its parent.
        /// </summary>
        [Category("Drag Drop"), Description("Notifies the parent control that the drag drop event has been passed to it from this control.")]
        public event EventHandler<DragEventArgs> BubbleDragDrop;
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
        [Browsable(true),
            DefaultValue(null),
            EditorBrowsable(EditorBrowsableState.Always),
            Category("Rendering"),
            Description("Sets or returns the custom control to use for rendering output."),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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

        /// <summary>
        /// Property to set or return the host for content panels.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPanelHostActive
        {
            get => PanelHost.Visible;
            set
            {
                PanelHost.Visible = value;
                PanelHost.BringToFront();
                PanelHost.Left = ClientSize.Width - PanelHost.Width;
                PanelHost.Top = 0;
                PanelHost.Height = ClientSize.Height;

                Invalidate(true);
            }
        }
        #endregion

        #region Methods.
        /// <summary>Handles the Click event of the ButtonClose control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void ButtonClose_Click(object sender, EventArgs e)
        {
            if ((_dataContext?.CloseContentCommand == null) || (!_dataContext.CloseContentCommand.CanExecute(null)))
            {
                return;
            }

            _dataContext.CloseContentCommand.Execute(new CloseContentArgs(true));
        }

        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [PropertyChangedEventArgs] instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IEditorContent.CloseContentCommand):
                    ButtonClose.Visible = _dataContext.CloseContentCommand != null;

                    if (!PanelContentName.Visible)
                    {
                        SetContentName(_dataContext);
                    }
                    return;
                case nameof(IEditorContent.ContentState):
                case nameof(IEditorContent.File):
                    SetContentName(_dataContext);
                    break;
            }

            OnPropertyChanged(e);
        }

        /// <summary>Handles the PropertyChanging event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [PropertyChangingEventArgs] instance containing the event data.</param>
        private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e) => OnPropertyChanging(e);

        /// <summary>
        /// Function to initialize the view from the current data context.
        /// </summary>
        /// <param name="dataContext">The data context being assigned.</param>
        private void InitializeFromDataContext(IEditorContent dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            ButtonClose.Visible = dataContext.CloseContentCommand != null;
            SetContentName(dataContext);
        }

        /// <summary>
        /// Function to shut down the view.
        /// </summary>
        private void Shutdown()
        {
            UnassignEvents();

            Stop();

            OnShutdown();

            // Return the swap chain to the pool.
            if (_swapChain != null)
            {
                GraphicsContext.ReturnSwapPresenter(ref _swapChain);
            }
        }

        /// <summary>
        /// Function to assign the current content name.
        /// </summary>
        /// <param name="dataContext">The data context to use.</param>
        private void SetContentName(IEditorContent dataContext)
        {
            if (string.IsNullOrWhiteSpace(dataContext?.File?.Name))
            {
                LabelHeader.Text = string.Empty;
                if (dataContext?.CloseContentCommand == null)
                {
                    PanelContentName.Visible = false;
                }
                else
                {
                    PanelContentName.Visible = true;
                }
                return;
            }

            LabelHeader.Text = $"{dataContext.File.Name}{(dataContext.ContentState == ContentState.Unmodified ? string.Empty : "*")}";
            PanelContentName.Visible = true;
        }

        /// <summary>
        /// Function to show the control keyboard focus state.
        /// </summary>
        /// <param name="isFocused"><b>true</b> if focused, <b>false</b> if not.</param>
        protected void ShowFocusState(bool isFocused)
        {
            if (isFocused)
            {
                panel4.BackColor = Color.FromArgb(104, 104, 104);
                PanelContentName.BackColor = Color.FromKnownColor(KnownColor.SteelBlue);
            }
            else
            {
                panel4.BackColor = Color.FromArgb(52, 52, 52);
                PanelContentName.BackColor = Color.FromKnownColor(KnownColor.SlateGray);
            }
        }

        /// <summary>
        /// Function to check to see if drag drop data is valid for this control.
        /// </summary>
        /// <param name="e">The event parameters for the drag/drop event.</param>
        /// <param name="handler">The drag/drop handler used to handle the drag drop information.</param>
        /// <returns>The data in the drag operation, or <b>null</b> if the data cannot be dragged and dropped onto this control.</returns>
        protected IContentFileDragData GetDragDropData(DragEventArgs e, IDragDropHandler<IContentFileDragData> handler)
        {
            if (handler == null)
            {
                e.Effect = DragDropEffects.None;
                return null;
            }

            e.Effect = DragDropEffects.Copy;

            Type contentFileDragDataType = typeof(IContentFileDragData);

            if (!e.Data.GetDataPresent(contentFileDragDataType.FullName, true))
            {
                return null;
            }

            if (!(e.Data.GetData(contentFileDragDataType.FullName, true) is IContentFileDragData dragData))
            {
                return null;
            }

            if (!handler.CanDrop(dragData))
            {
                if (dragData.Cancel)
                {
                    e.Effect = DragDropEffects.None;
                }

                return null;
            }

            return dragData;
        }

        /// <summary>
        /// Function to add a control to the panel host.
        /// </summary>
        /// <param name="control">The control to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="control"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// Adding a control does not guarantee that the host will be visible. To show the panel host, use the <see cref="IsPanelHostActive"/> property.
        /// </para>
        /// <para>
        /// Only a single control can be active in the host at a time. Adding another control will remove the previous control.
        /// </para>
        /// </remarks>
        protected void AddControlToPanelHost(Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }

            try
            {
                PanelHost.SuspendLayout();
                PanelHostControls.SuspendLayout();
                control.SuspendLayout();

                PanelHostControls.Controls.Clear();
                PanelHostControls.Controls.Add(control);
                control.Left = 0;
                control.Top = 0;

                // Our control width should be within this range.  
                if (control.Width > 640)
                {
                    control.Width = 640;
                }

                if (control.Width < 160)
                {
                    control.Width = 160;
                }

                PanelHost.Width = control.Width + PanelHost.Padding.Left;
                if (control.Height > PanelHostControls.ClientSize.Height)
                {
                    PanelHost.Width += SystemInformation.VerticalScrollBarWidth;
                }

                PanelHostControls.AutoScrollMinSize = new Size(0, control.Height);
            }
            finally
            {
                control.ResumeLayout(false);
                control.PerformLayout();
                PanelHostControls.ResumeLayout(false);
                PanelHostControls.PerformLayout();
                PanelHost.ResumeLayout(false);
                PanelHost.PerformLayout();
            }


        }

        /// <summary>
        /// Function to clear the host panel controls.
        /// </summary>
        protected void ClearPanelHost() => PanelHostControls.Controls.Clear();

        /// <summary>
        /// Function to render the swap chain contents to a GDI+ bitmap.
        /// </summary>
        /// <param name="graphics">The GDI+ graphics interface.</param>
        /// <remarks>
        /// <para>
        /// Use this method to send the backbuffer for the <see cref="SwapChain"/> to a GDI+ bitmap when the control is being rendered to a bitmap or printed. The <see cref="GorgonOverlayPanel"/> renders 
        /// the control to a bitmap to achieve the transparency effect it uses, and without this method anything on the swap chain will not be drawn under the overlay.
        /// </para>
        /// </remarks>
        protected void RenderSwapChainToBitmap(System.Drawing.Graphics graphics)
        {
            if ((IsDesignTime) || (SwapChain == null) || (IdleMethod == null))
            {
                return;
            }

            // This method is used to capture the D3D rendering when rendering to a GDI+ bitmap (as used by the overlay).
            // Without it, no image is rendered and only a dark grey background is visible.

            IGorgonImage swapBufferImage = null;
            Bitmap gdiBitmap = null;

            try
            {
                IdleMethod();

                swapBufferImage = SwapChain.CopyBackBufferToImage();
                gdiBitmap = swapBufferImage.Buffers[0].ToBitmap();
                swapBufferImage.Dispose();

                graphics.DrawImage(gdiBitmap, new Point(0, 0));
            }
            catch
            {
                // Empty on purpose.  Don't need to worry about exceptions here, if things fail, they fail and state should not be corrupted.
            }
            finally
            {
                gdiBitmap?.Dispose();
                swapBufferImage?.Dispose();
            }
        }

        /// <summary>
        /// Function to bubble up the drag enter event up to the main project window.
        /// </summary>
        /// <param name="e">The drag event parameters.</param>
        /// <remarks>
        /// <para>
        /// Implementors can use this method to notify the parent of this control that a drag enter event is being passed on from this control.
        /// </para>
        /// </remarks>
        protected virtual void OnBubbleDragEnter(DragEventArgs e)
        {
            EventHandler<DragEventArgs> handler = BubbleDragEnter;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Function to bubble up the drag drop event up to the main project window.
        /// </summary>
        /// <param name="e">The drag event parameters.</param>
        /// <remarks>
        /// <para>
        /// Implementors can use this method to notify the parent of this control that a drag drop event is being passed on from this control.
        /// </para>
        /// </remarks>
        protected virtual void OnBubbleDragDrop(DragEventArgs e)
        {
            EventHandler<DragEventArgs> handler = BubbleDragDrop;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Function called when a property is changing on the data context.
        /// </summary>
        /// <param name="e">The event parameters.</param>
        /// <remarks>
        /// <para>
        /// Implementors should override this method in order to handle a property change notification from their data context.
        /// </para>
        /// </remarks>
        protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
        {

        }

        /// <summary>
        /// Function called when a property is changed on the data context.
        /// </summary>
        /// <param name="e">The event parameters.</param>
        /// <remarks>
        /// <para>
        /// Implementors should override this method in order to handle a property change notification from their data context.
        /// </para>
        /// </remarks>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {

        }

        /// <summary>
        /// Function to unassign events for the data context.
        /// </summary>
        protected virtual void UnassignEvents()
        {
            if (_dataContext == null)
            {
                return;
            }

            _dataContext.PropertyChanging -= DataContext_PropertyChanging;
            _dataContext.PropertyChanged -= DataContext_PropertyChanged;
        }

        /// <summary>
        /// Function called when the view should be reset by a <b>null</b> data context.
        /// </summary>
        protected virtual void ResetDataContext()
        {
            if (_dataContext == null)
            {
                return;
            }

            ButtonClose.Visible = false;
            SetContentName(null);
        }

        /// <summary>
        /// Function to assign the data context to this object.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>
        /// <para>
        /// Applications must call this method when setting their own data context. Otherwise, some functionality will not work.
        /// </para>
        /// </remarks>
        protected void SetDataContext(IEditorContent dataContext)
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);
            _dataContext = dataContext;

            if (_dataContext == null)
            {
                return;
            }

            _dataContext.PropertyChanged += DataContext_PropertyChanged;
            _dataContext.PropertyChanging += DataContext_PropertyChanging;
        }

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
        /// Function to retrieve a registered child panel control.
        /// </summary>
        /// <typeparam name="T">The type of panel.</typeparam>
        /// <param name="type">The type of the panel view model.</param>
        /// <returns>The registered panel if found, or <b>null</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="type"/> parameter is <b>null</b>.</exception>
        protected T GetRegisteredPanel<T>(Type type)
            where T : Control
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (_panelViews.TryGetValue(type.FullName, out Control result))
            {
                return (T)result;
            }

            Control SearchInterfaces(Type implementingType)
            {
                Type[] interfaces = implementingType.GetInterfaces();

                if (interfaces == null)
                {
                    return null;
                }

                foreach (Type interfaceType in interfaces)
                {
                    if (_panelViews.TryGetValue(interfaceType.FullName, out Control interfaceResult))
                    {
                        return interfaceResult;
                    }
                }

                return null;
            }

            // Check interfaces.
            result = SearchInterfaces(type);

            if (result != null)
            {
                return (T)result;
            }

            Type baseType = type.BaseType;

            // Walk the hierarchy to see if we've descended from the specified type.
            while (baseType != null)
            {
                if (_panelViews.TryGetValue(baseType.FullName, out result))
                {
                    return (T)result;
                }

                result = SearchInterfaces(baseType);

                if (result != null)
                {
                    return (T)result;
                }

                baseType = baseType.BaseType;
            }

            return null;
        }

        /// <summary>
        /// Function to determine if a child panel is registered on this view or not.
        /// </summary>
        /// <typeparam name="T">The type of panel.</typeparam>
        /// <param name="id">The ID of the panel.</param>
        /// <returns>The registered panel if found, or <b>null</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="id"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="id"/> parameter is empty.</exception>
        protected T GetRegisteredPanel<T>(string id)
            where T : Control
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentEmptyException(nameof(id));
            }

            return !_panelViews.TryGetValue(id, out Control result) ? null : (T)result;
        }

        /// <summary>
        /// Function to register a child panel with this content control.
        /// </summary>
        /// <param name="id">The ID of the panel.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="id"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="id"/> parameter is empty.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="id"/> is not registered.</exception>
        protected void UnregisterChildPanel(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentEmptyException(nameof(id));
            }

            if (!_panelViews.ContainsKey(id))
            {
                throw new ArgumentException(string.Format(Resources.GOREDIT_ERR_PANEL_NOT_REGISTERED, id));
            }

            _panelViews.Remove(id);
        }

        /// <summary>
        /// Function to register a child panel with this content control.
        /// </summary>
        /// <param name="id">The ID of the panel.</param>
        /// <param name="control">The control representing the child panel.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="id"/>, or the <paramref name="control"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="id"/> parameter is empty.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="id"/> is already registered.</exception>
        protected void RegisterChildPanel(string id, Control control)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentEmptyException(nameof(id));
            }

            if (_panelViews.ContainsKey(id))
            {
                throw new ArgumentException(string.Format(Resources.GOREDIT_ERR_PANEL_ALREADY_REGISTERED, id));
            }

            _panelViews.Add(id, control);
        }

        /// <summary>
        /// Function to begin rendering on the control.
        /// </summary>
        public void Start()
        {
            GorgonApplication.AllowBackground = true;
            GorgonApplication.IdleMethod = IdleMethod;
        }

        /// <summary>
        /// Function to cease rendering on the control.
        /// </summary>
        public void Stop()
        {
            GorgonApplication.AllowBackground = false;
            GorgonApplication.IdleMethod = null;
        }

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
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ContentBaseControl class.</summary>
        public ContentBaseControl() => InitializeComponent();
        #endregion
    }
}
