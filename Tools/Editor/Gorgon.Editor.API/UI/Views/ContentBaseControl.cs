﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: October 29, 2018 4:12:28 PM
// 

using System.ComponentModel;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.UI;
using Krypton.Ribbon;

namespace Gorgon.Editor.UI.Views;

/// <summary>
/// The base control used to render content
/// </summary>
public partial class ContentBaseControl
    : EditorBaseControl, IRendererControl
{

    // Synchronization objects for events.
    private readonly object _closeEventLock = new();
    private readonly object _dragEnterEventLock = new();
    private readonly object _dragOverEventLock = new();
    private readonly object _dragDropEventLock = new();
    // The swap chain for the control.
    private GorgonSwapChain _swapChain;
    // The data context for the editor context.
    private IEditorContent _dataContext;
    // A list of child panel views identified by name.
    private readonly Dictionary<string, Control> _panelViews = new(StringComparer.OrdinalIgnoreCase);

    // Event triggered when the content is closed.
    private event EventHandler ContentClosedEvent;
    // Event triggered when a drag enter operation is bubbled up to the parent.
    private event EventHandler<DragEventArgs> BubbleDragEnterEvent;
    // Event triggered when a drop operation is bubbled up to the parent.
    private event EventHandler<DragEventArgs> BubbleDragDropEvent;
    // Event triggered when a drag over operation is bubbled up to the parent.
    private event EventHandler<DragEventArgs> BubbleDragOverEvent;

    /// <summary>
    /// Event triggered when the content is closed.
    /// </summary>
    [Category("Behavior"), Description("Notifies when the content control is closed.")]
    public event EventHandler ContentClosed
    {
        add
        {
            lock (_closeEventLock)
            {
                if (value is null)
                {
                    ContentClosedEvent = null;
                    return;
                }

                ContentClosedEvent += value;
            }
        }
        remove
        {
            lock (_closeEventLock)
            {
                if (value is null)
                {
                    return;
                }

                ContentClosedEvent -= value;
            }
        }
    }

    /// <summary>
    /// Event triggered when a drag enter operation is to be bubbled up to its parent.
    /// </summary>
    [Category("Drag Drop"), Description("Notifies the parent control that the drag enter event has been passed to it from this control.")]
    public event EventHandler<DragEventArgs> BubbleDragEnter
    {
        add
        {
            lock (_dragEnterEventLock)
            {
                if (value is null)
                {
                    BubbleDragEnterEvent = null;
                    return;
                }

                BubbleDragEnterEvent += value;
            }
        }
        remove
        {
            lock (_dragEnterEventLock)
            {
                if (value is null)
                {
                    return;
                }

                BubbleDragEnterEvent -= value;
            }
        }
    }

    /// <summary>
    /// Event triggered when a drop operation is to be bubbled up to its parent.
    /// </summary>
    [Category("Drag Drop"), Description("Notifies the parent control that the drag over event has been passed to it from this control.")]
    public event EventHandler<DragEventArgs> BubbleDragOver
    {
        add
        {
            lock (_dragOverEventLock)
            {
                if (value is null)
                {
                    BubbleDragOverEvent = null;
                    return;
                }

                BubbleDragOverEvent += value;
            }
        }
        remove
        {
            lock (_dragOverEventLock)
            {
                if (value is null)
                {
                    return;
                }

                BubbleDragOverEvent -= value;
            }
        }
    }

    /// <summary>
    /// Event triggered when a drop operation is to be bubbled up to its parent.
    /// </summary>
    [Category("Drag Drop"), Description("Notifies the parent control that the drag drop event has been passed to it from this control.")]
    public event EventHandler<DragEventArgs> BubbleDragDrop
    {
        add
        {
            lock (_dragDropEventLock)
            {
                if (value is null)
                {
                    BubbleDragDropEvent = null;
                    return;
                }

                BubbleDragDropEvent += value;
            }
        }
        remove
        {
            lock (_dragDropEventLock)
            {
                if (value is null)
                {
                    return;
                }

                BubbleDragDropEvent -= value;
            }
        }
    }

    /// <summary>
    /// Property to return the currently hosted panel in the <see cref="HostPanelControls"/>.
    /// </summary>
    protected Control CurrentHostedPanel
    {
        get;
        private set;
    }

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
    public Panel PresentationPanel => PanelPresenter;

    /// <summary>
    /// Property to return the panel that hosts the <see cref="HostPanelControls"/>.
    /// </summary>
    [Browsable(false)]
    public Panel HostPanel => PanelHost;

    /// <summary>
    /// Property to return the panel that will be used for hosting panels for settings, parameters, etc...
    /// </summary>
    [Browsable(false)]
    public Panel HostPanelControls => PanelHostControls;

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

    /// <summary>Handles the Click event of the ButtonClose control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The [EventArgs] instance containing the event data.</param>
    private async void ButtonClose_Click(object sender, EventArgs e)
    {
        EventHandler handler = null;
        CloseContentArgs args = new(true);

        if ((_dataContext?.CloseContentCommand is not null) && (_dataContext.CloseContentCommand.CanExecute(args)))
        {
            await _dataContext.CloseContentCommand.ExecuteAsync(args);
        }

        if (args.Cancel)
        {
            return;
        }

        lock (_closeEventLock)
        {
            handler = ContentClosedEvent;
        }
        handler?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The [PropertyChangedEventArgs] instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IEditorContent.CloseContentCommand):
                ButtonClose.Visible = _dataContext.CloseContentCommand is not null;

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

        OnPropertyChanged(e.PropertyName);
    }

    /// <summary>Handles the PropertyChanging event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The [PropertyChangingEventArgs] instance containing the event data.</param>
    private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e) => OnPropertyChanging(e.PropertyName);

    /// <summary>
    /// Function to initialize the view from the current data context.
    /// </summary>
    /// <param name="dataContext">The data context being assigned.</param>
    private void InitializeFromDataContext(IEditorContent dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        ButtonClose.Visible = dataContext.CloseContentCommand is not null;
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
        if (_swapChain is not null)
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
            if (dataContext?.CloseContentCommand is null)
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
    /// Function to show a hosted panel.
    /// </summary>
    /// <param name="control">The control to show.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="control"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// Only a single control can be active in the host at a time. Adding another control will hide the previous control.
    /// </para>
    /// </remarks>
    protected void ShowHostedPanel(Control control)
    {
        if (control is null)
        {
            throw new ArgumentNullException(nameof(control));
        }

        Control hostParent = PanelHostControls;

        try
        {
            while (hostParent is not null)
            {
                hostParent.SuspendLayout();
                hostParent = hostParent.Parent;
            }

            control.SuspendLayout();

            foreach (Control hostControl in PanelHostControls.Controls.OfType<Control>().Where(item => item != control))
            {
                hostControl.Visible = false;
            }

            control.Left = 0;
            control.Top = 0;
            float dpiScale = 1;
            int maxSize = ClientSize.Width - (Margin.Left + Margin.Right);

            if (DeviceDpi != 96)
            {
                dpiScale = DeviceDpi / 96.0f;
            }

            // Our control width should be within this range.  
            if (control.Width > maxSize)
            {
                control.Width = maxSize;
            }

            if (control.Width < (int)(300 * dpiScale).FastFloor())
            {
                control.Width = (int)(300 * dpiScale).FastFloor();
            }

            int newWidth = control.Width + PanelHost.Padding.Left;
            if (control.Height > PanelHostControls.ClientSize.Height)
            {
                newWidth += SystemInformation.VerticalScrollBarWidth;
            }

            PanelHost.Width = newWidth;
            PanelHostControls.Visible = PanelHost.Visible = true;
            control.Visible = true;
            PanelHostControls.AutoScrollMinSize = new Size(0, control.Height);

            CurrentHostedPanel = control;
        }
        finally
        {
            hostParent = control;
            while (hostParent is not null)
            {
                hostParent.ResumeLayout();
                hostParent = hostParent.Parent;
            }
        }
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
    /// Function to retrieve content file data from a drag/drop operation.
    /// </summary>
    /// <typeparam name="T">The type of data to retrieve. Must implement <see cref="IContentFileDragData"/> and be a reference type.</typeparam>
    /// <param name="e">The event parameters for the drag/drop event.</param>
    /// <returns>The data in the drag operation, or <b>null</b> if the data cannot be dragged and dropped onto this control.</returns>
    /// <remarks>
    /// <para>
    /// This will return custom drag/drop data that implements the <see cref="IContentFileDragData"/> interface. Content editor developers should use this method to verify whether the data being dragged 
    /// into the control is valid or not, and use the returned data to perform the final drop operation.
    /// </para>
    /// <para>
    /// If the editor cannot support the data being dragged in, then the developer should call the <see cref="OnBubbleDragEnter(DragEventArgs)"/>, <see cref="OnBubbleDragOver(DragEventArgs)"/>,or the 
    /// <see cref="OnBubbleDragDrop(DragEventArgs)"/> methods (depending on the event being fired on the control).
    /// </para>
    /// </remarks>
    /// <seealso cref="IContentFileDragData"/>
    protected T GetContentFileDragDropData<T>(DragEventArgs e)
        where T : class, IContentFileDragData
    {
        Type contentFileDragDataType = typeof(IContentFileDragData);

        if (!e.Data.GetDataPresent(contentFileDragDataType.FullName, true))
        {
            e.Effect = DragDropEffects.None;
            return null;
        }

        if (e.Data.GetData(contentFileDragDataType.FullName, true) is not IContentFileDragData dragData)
        {
            e.Effect = DragDropEffects.None;
            return null;
        }

        e.Effect = DragDropEffects.Copy;
        return dragData as T;
    }

    /// <summary>
    /// Function to hide the host panel controls.
    /// </summary>
    protected void HideHostedPanels()
    {
        foreach (Control control in PanelHostControls.Controls)
        {
            control.Visible = false;
        }

        PanelHost.Visible = PanelHostControls.Visible = false;

        CurrentHostedPanel = null;
    }

    /// <summary>
    /// Function to assign a data context to the <see cref="CurrentHostedPanel"/>.
    /// </summary>
    /// <param name="dataContext">The data context to assign.</param>        
    /// <param name="overrideCurrentDc"><b>true</b> to replace any non-null data context if present, or <b>false</b> to leave the current data context in place.</param>
    /// <exception cref="InvalidCastException">Thrown when the <see cref="CurrentHostedPanel"/> does not accept the type of data context represented by <paramref name="dataContext"/>.
    /// <para>-or-</para>
    /// <para>Thrown if the <see cref="CurrentHostedPanel"/> does not implement <see cref="IDataContext{T}"/>.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is used to assign an instance of a data context to the hosted panel in the <see cref="HostPanelControls"/>. If the panel does not implement <see cref="IDataContext{T}"/>, or the type of data 
    /// context does not match the type of data context the control was built for, an exception will be thrown.
    /// </para>
    /// </remarks>
    protected void AssignDataContextToHostedPanel(IViewModel dataContext, bool overrideCurrentDc)
    {
        if (CurrentHostedPanel is null)
        {
            return;
        }

        Type dataContextType = typeof(IDataContext<>);
        Type controlType = CurrentHostedPanel.GetType();
        Type _ = controlType.GetInterface(dataContextType.FullName) ?? throw new InvalidCastException(string.Format(Resources.GOREDIT_ERR_HOSTED_CTL_NOT_DATACONTEXT, CurrentHostedPanel.Name));

        if ((!overrideCurrentDc) && (ViewFactory.GetViewModel(CurrentHostedPanel) is not null))
        {
            return;
        }

        ViewFactory.AssignViewModel(dataContext, CurrentHostedPanel);
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
        EventHandler<DragEventArgs> handler = null;

        lock (_dragEnterEventLock)
        {
            handler = BubbleDragEnterEvent;
        }

        handler?.Invoke(this, e);
    }

    /// <summary>
    /// Function to bubble up the drag drop event up to the main project window.
    /// </summary>
    /// <param name="e">The drag event parameters.</param>
    /// <para>
    /// Implementors can use this method to notify the parent of this control that a drag over event is being passed on from this control.
    /// </para>
    protected virtual void OnBubbleDragOver(DragEventArgs e)
    {
        EventHandler<DragEventArgs> handler = null;

        lock (_dragOverEventLock)
        {
            handler = BubbleDragOverEvent;
        }

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
        EventHandler<DragEventArgs> handler = null;

        lock (_dragDropEventLock)
        {
            handler = BubbleDragDropEvent;
        }

        handler?.Invoke(this, e);
    }

    /// <summary>
    /// Function called when a property is changing on the data context.
    /// </summary>
    /// <param name="propertyName">The name of the property that is updating.</param>
    /// <remarks>
    /// <para>
    /// Implementors should override this method in order to handle a property change notification from their data context.
    /// </para>
    /// </remarks>
    protected virtual void OnPropertyChanging(string propertyName)
    {

    }

    /// <summary>
    /// Function called when a property is changed on the data context.
    /// </summary>
    /// <param name="propertyName">The name of the property that is updated.</param>
    /// <remarks>
    /// <para>
    /// Implementors should override this method in order to handle a property change notification from their data context.
    /// </para>
    /// </remarks>
    protected virtual void OnPropertyChanged(string propertyName)
    {

    }

    /// <summary>
    /// Function to unassign events for the data context.
    /// </summary>
    protected virtual void UnassignEvents()
    {
        if (_dataContext is null)
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
        if (_dataContext is null)
        {
            return;
        }

        if (Disposing)
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
    protected void OnSetDataContext(IEditorContent dataContext)
    {
        UnassignEvents();

        InitializeFromDataContext(dataContext);
        _dataContext = dataContext;

        if (_dataContext is null)
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
    protected virtual void OnShutdown()
    {
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

        if (id is null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentEmptyException(nameof(id));
        }

        return (!_panelViews.TryGetValue(id, out Control result)) ? null : (T)result;

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
        if (id is null)
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
    /// <param name="id">The ID for the panel, this should be the view model fully qualified type name.</param>
    /// <param name="control">The control representing the child panel.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="id"/>, or the <paramref name="control"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="id"/> parameter is empty.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="id"/> is already registered.</exception>
    protected void RegisterChildPanel(string id, Control control)
    {
        if (id is null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        if (control is null)
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

        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // If we've made no change, then do nothing.
        if ((context == GraphicsContext) && (_swapChain is not null) && (_swapChain.Window == RenderControl))
        {
            return;
        }

        if (_swapChain is not null)
        {
            GraphicsContext.ReturnSwapPresenter(ref _swapChain);
        }

        GorgonSwapChain swapChain = null;

        // If we've defined a render control, then lease a swap chain from the swap chain pool.
        if ((context is not null) && (RenderControl is not null))
        {
            swapChain = context.LeaseSwapPresenter(RenderControl);
        }

        OnSetupGraphics(context, swapChain);
        GraphicsContext = context;
        _swapChain = swapChain;
    }

    /// <summary>Initializes a new instance of the ContentBaseControl class.</summary>
    public ContentBaseControl() => InitializeComponent();

}
