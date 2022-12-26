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
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Tools;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Timing;
using Gorgon.UI;

namespace Gorgon.Editor.UI.Views;

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
    : Form
{
    #region Variables.
    // A list of renderers used to draw our content to the UI.
    private readonly Dictionary<string, IToolRenderer> _renderers = new(StringComparer.OrdinalIgnoreCase);
    // The control that will receive rendering output.
    private Control _renderControl;
    // Previous idle method.
    private Func<bool> _oldIdle;
    // The previous flag for background operation.
    private bool _oldBackgroundState;
    // The swap chain for the render control.
    private GorgonSwapChain _swapChain;
    // The application graphics context.
    private IGraphicsContext _graphicsContext;
    // The data context for this window.
    private IEditorTool _dataContext;
    // The state for the close procedure.
    private int _closeState;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the currently active tool renderer.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    protected IToolRenderer Renderer
    {
        get;
        private set;
    }

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
    /// Property to return the graphics context for the application.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    protected IGraphicsContext GraphicsContext => _graphicsContext;

    /// <summary>
    /// Property to return the swap chain for the tool render panel.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    protected GorgonSwapChain SwapChain => _swapChain;

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
    /// <summary>Handles the PropertyChanging event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e) => OnPropertyChanging(e.PropertyName);

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e) => OnPropertyChanged(e.PropertyName);

    /// <summary>Handles the DragDrop event of the PanelRenderWindow control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
    private void PanelRenderWindow_DragDrop(object sender, DragEventArgs e) => OnRenderWindowDragDrop(e);

    /// <summary>Handles the DragEnter event of the PanelRenderWindow control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
    private void PanelRenderWindow_DragEnter(object sender, DragEventArgs e) => OnRenderWindowDragEnter(e);

    /// <summary>Handles the DragOver event of the PanelRenderWindow control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
    private void PanelRenderWindow_DragOver(object sender, DragEventArgs e) => OnRenderWindowDragOver(e);

    /// <summary>
    /// Function to handle idle time and rendering.
    /// </summary>
    /// <returns><b>true</b> to continue rendering, <b>false</b> to stop.</returns>
    private bool Idle()
    {
        OnBeforeRender();
        Renderer?.Render();
        OnAfterRender();

        // Render on other controls as well.
        if ((GorgonApplication.AllowBackground) && (_oldIdle is not null))
        {
            if (!_oldIdle())
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Function to shut down the graphics interface for this window.
    /// </summary>
    private void ShutdownGraphics()
    {
        if (_graphicsContext is null)
        {
            return;
        }

        GorgonApplication.IdleMethod = Interlocked.Exchange(ref _oldIdle, null);

        OnShutdownGraphics();

        foreach (IToolRenderer renderer in _renderers.Values)
        {                
            renderer.Dispose();
        }

        _renderers.Clear();

        GorgonSwapChain swapChain = Interlocked.Exchange(ref _swapChain, null);
        IGraphicsContext context = Interlocked.Exchange(ref _graphicsContext, null);            

        GorgonApplication.AllowBackground = _oldBackgroundState;

        if (swapChain is not null)
        {
            context?.ReturnSwapPresenter(ref swapChain);
        }
    }

    /// <summary>
    /// Function called when the view should be reset by a <b>null</b> data context.
    /// </summary>
    private void ResetDataContext()
    {
        if ((_dataContext is null) || (Disposing))
        {
            return;
        }

        OnResetDataContext();
    }

    /// <summary>
    /// Function to unassign events for the data context.
    /// </summary>
    private void UnassignEvents()
    {
        if (_dataContext is null)
        {
            return;
        }

        OnUnassignEvents();

        _dataContext.PropertyChanging -= DataContext_PropertyChanging;
        _dataContext.PropertyChanged -= DataContext_PropertyChanged;
    }


    /// <summary>
    /// Function to handle a drag enter event on the render control.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    /// <remarks>
    /// <para>
    /// Tool plug in developers can override this method to handle a drag enter event when an item is dragged into the rendering area on the view.
    /// </para>
    /// </remarks>
    protected virtual void OnRenderWindowDragEnter(DragEventArgs e)
    {
    }

    /// <summary>
    /// Function to handle a drag over event on the render control.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    /// <remarks>
    /// <para>
    /// Tool plug in developers can override this method to handle a drag over event when an item is dragged over the rendering area on the view.
    /// </para>
    /// </remarks>
    protected virtual void OnRenderWindowDragOver(DragEventArgs e)
    {
    }

    /// <summary>
    /// Function to handle a drag drop event on the render control.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    /// <remarks>
    /// <para>
    /// Tool plug in developers can override this method to handle a drop event when an item is dropped into the rendering area on the view.
    /// </para>
    /// </remarks>
    protected virtual void OnRenderWindowDragDrop(DragEventArgs e)
    {        
    }

    /// <summary>
    /// Function called after rendering ends.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Developers can use this method to perform operations immediately after rendering the tool UI.
    /// </para>
    /// </remarks>
    protected virtual void OnAfterRender()
    {

    }

    /// <summary>
    /// Function called before rendering begins.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Developers can use this method to perform last minute updates prior to rendering the tool UI.
    /// </para>
    /// </remarks>
    protected virtual void OnBeforeRender()
    {

    }

    /// <summary>
    /// Function called when the renderer is switched.
    /// </summary>
    /// <param name="renderer">The current renderer.</param>
    protected virtual void OnSwitchRenderer(IToolRenderer renderer)
    {

    }

    /// <summary>
    /// Function called when a property is changing on the data context.
    /// </summary>
    /// <param name="propertyName">The name of the property that is changing.</param>
    /// <remarks>
    /// <para>
    /// Developers should override this method when detecting property changes on the data context instead of assigning their own event handlers.
    /// </para>
    /// </remarks>
    protected virtual void OnPropertyChanging(string propertyName)
    {
    
    }

    /// <summary>
    /// Function called when a property was changed on the data context.
    /// </summary>
    /// <param name="propertyName">The name of the property that was changed.</param>
    /// <remarks>
    /// <para>
    /// Developers should override this method when detecting property changes on the data context instead of assigning their own event handlers.
    /// </para>
    /// </remarks>
    protected virtual void OnPropertyChanged(string propertyName)
    {

    }

    /// <summary>
    /// Function to register a new renderer with the view.
    /// </summary>
    /// <param name="name">The name of the renderer.</param>
    /// <param name="renderer">The renderer to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, or the <paramref name="renderer"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <exception cref="ArgumentException">Thrown if a renderer is already registered with the same <paramref name="name"/>.</exception>
    /// <remarks>
    /// <para>
    /// Some tool plug ins will require the rendered view to change depending on state. For example, a specific tool is selected and the view needs to switch to a portion of the tool UI. In such 
    /// cases it is not practical to have a single renderer performing all manner of state changes, so this method provides a means of passing different renderer instances to the view.
    /// </para>
    /// <para>
    /// Developers of the tool plug ins should register these renderers in the <see cref="OnSetupGraphics(IGraphicsContext, GorgonSwapChain)"/> method so that the renderers are available right 
    /// away. Failure to do so can lead to broken rendering.
    /// </para>
    /// </remarks>
    /// <seealso cref="IToolRenderer"/>
    protected void AddRenderer(string name, IToolRenderer renderer)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        if (_renderers.ContainsKey(name))
        {
            throw new ArgumentException(string.Format(Resources.GOREDIT_ERR_TOOL_RENDERER_EXISTS, name), nameof(name));
        }

        _renderers[name] = renderer;
    }

    /// <summary>
    /// Function to determine if a renderer with the specified name is registered.
    /// </summary>
    /// <param name="name">The name of the renderer to evaluate.</param>
    /// <returns><b>true</b> if the renderer is registered, <b>false</b> if not.</returns>
    protected bool HasRenderer(string name)
    {
#pragma warning disable IDE0046 // Convert to conditional expression
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        return string.IsNullOrWhiteSpace(name) ? throw new ArgumentEmptyException(nameof(name)) : _renderers.ContainsKey(name);
#pragma warning restore IDE0046 // Convert to conditional expression
    }

    /// <summary>
    /// Function to switch to another registered renderer.
    /// </summary>
    /// <param name="name">The name of the renderer to switch to.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <exception cref="ArgumentException">Thrown if no renderer with the specified <paramref name="name"/> was found.</exception>
    /// <remarks>
    /// <para>
    /// Developers will this method to switch between renderer types (if multiple renderers are used) to denote editor state. 
    /// </para>
    /// </remarks>
    protected void SwitchRenderer(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        if (!_renderers.TryGetValue(name, out IToolRenderer newRenderer))
        {
            throw new ArgumentException(string.Format(Resources.GOREDIT_ERR_CONTENT_RENDERER_NOT_FOUND, name), nameof(name));
        }

        // Blow away any temporary resources registered to the renderer.
        if (Renderer is not null)
        {
            Renderer.UnloadResources();
            OnSwitchRenderer(null);
        }
        Renderer = null;

        // Ensure temporary resources for the new renderer are available.
        newRenderer.LoadResources();
        Renderer = newRenderer;
        Renderer.IsEnabled = true;

        OnSwitchRenderer(Renderer);

        // Reset the timing for the idle loop so we don't end up with stupid delta values because it takes time to switch.
        GorgonTiming.Reset();
    }

    /// <summary>
    /// Function to perform custom graphics set up.
    /// </summary>
    /// <param name="graphicsContext">The graphics context for the application.</param>
    /// <param name="swapChain">The swap chain used to render into the UI.</param>
    /// <remarks>
    /// <para>
    /// This method allows tool plug in implementors to setup additional functionality for custom graphics rendering.
    /// </para>
    /// <para>
    /// Resources created by this method should be cleaned up in the <see cref="OnShutdownGraphics"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="OnShutdownGraphics"/>
    protected virtual void OnSetupGraphics(IGraphicsContext graphicsContext, GorgonSwapChain swapChain)
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

    /// <summary>
    /// Function to unassign events for the data context.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the developer assigns events to the data context, child data contexts, or other event capable controls, they should unassign the events here to avoid event leakage.
    /// </para>
    /// </remarks>
    protected virtual void OnUnassignEvents()
    {
    
    }

    /// <summary>
    /// Function called when the view should be reset by a <b>null</b> data context.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Developers should override this method to return the form back to its original state.
    /// </para>
    /// </remarks>
    protected virtual void OnResetDataContext()
    {
        
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
    protected void OnSetDataContext(IEditorTool dataContext)
    {
        UnassignEvents();
        ResetDataContext();

        _dataContext = dataContext;

        if (_dataContext is null)
        {
            return;
        }

        _dataContext.PropertyChanged += DataContext_PropertyChanged;
        _dataContext.PropertyChanging += DataContext_PropertyChanging;
    }

    /// <summary>Raises the <see cref="Form.FormClosing"/> event.</summary>
    /// <param name="e">A <see cref="FormClosingEventArgs"/> that contains the event data.</param>
    protected override async void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        if (_closeState > 0)
        {
            // Cancel our request if we're in the processing of shutting everything down.
            e.Cancel = _closeState == 1;
            return;
        }

        Interlocked.Exchange(ref _closeState, 1);
        e.Cancel = true;

        var args = new CloseToolArgs(true);

        if ((_dataContext?.CloseToolCommand is not null) && (_dataContext.CloseToolCommand.CanExecute(args)))
        {
            await _dataContext.CloseToolCommand.ExecuteAsync(args);
        }

        if (args.Cancel)
        {
            // Reset our state if we've cancelled, otherwise calling close will break down again.
            Interlocked.Exchange(ref _closeState, 0);
            return;
        }

        await Task.Yield();

        try
        {
            _dataContext?.Unload();
            ShutdownGraphics();
        }
        finally
        {
            // This will ensure that the form actually disposes.
            Interlocked.Exchange(ref _closeState, 2);
            Close();
        }
    }

    /// <summary>Raises the <see cref="Form.Load"/> event.</summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (IsDesignTime)
        {
            return;
        }

        _dataContext?.Load();
        RenderControl?.Select();
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
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (RenderControl is null)
        {
            throw new GorgonException(GorgonResult.NotInitialized, Resources.GOREDIT_ERR_NO_RENDER_CONTROL);
        }

        if (IsDesignTime)
        {
            return;
        }

        // If we've made no change, then do nothing.
        if ((context == _graphicsContext) && (_swapChain is not null) && (_swapChain.Window == RenderControl))
        {
            return;
        }

        if (_swapChain is not null)
        {
            _graphicsContext?.ReturnSwapPresenter(ref _swapChain);
            _graphicsContext = null;
        }
                    
        GorgonSwapChain swapChain = context.LeaseSwapPresenter(RenderControl);

        // Always insert the default renderer.
        _renderers["null"] = new DefaultToolRenderer<IEditorTool>("null", context.Renderer2D, swapChain, _dataContext);

        foreach (IToolRenderer renderer in _renderers.Values)
        {
            renderer.BackgroundColor = RenderControl?.BackColor ?? GorgonColor.White;
        }

        // If we didn't register any renderers, then do not switch to anything (nothing will be rendered).
        if (_renderers.Count == 0)
        {
            return;
        }

        OnSetupGraphics(context, swapChain);

        if (Renderer is null)
        {
            _renderers["null"].IsEnabled = true;
            SwitchRenderer("null");
        }
        else
        {
            _renderers["null"].IsEnabled = false;
        }

        _oldIdle = GorgonApplication.IdleMethod;

        _oldBackgroundState = GorgonApplication.AllowBackground;

        GorgonApplication.AllowBackground = allowBackgroundRendering;
        GorgonApplication.IdleMethod = Idle;

        _graphicsContext = context;
        _swapChain = swapChain;
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
