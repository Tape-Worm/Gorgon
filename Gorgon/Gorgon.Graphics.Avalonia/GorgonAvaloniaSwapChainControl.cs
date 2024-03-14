using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Rendering;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Timing;

namespace Gorgon.Graphics.Avalonia;

/// <summary>
/// A control used to display an Avalonia swap chain so that Gorgon can render into it.
/// </summary>
/// <remarks>
/// <para>
/// This control allows interoperability between Gorgon and Avalonia. 
/// </para>
/// <para>
/// To use it, create an Avalonia application and put this control in the main window. Then, in the initialization of the main window (e.g. Window_Loaded), execute the 
/// <see cref="RunAsync(GorgonGraphics, Func{GorgonRenderTarget2DView, bool})"/> method to begin the application loop. 
/// </para>
/// <para>
/// The application loop Idle method is different from the Windows Forms version. It receives a <see cref="GorgonRenderTarget2DView"/> which represents the render target from Avalonia. When this target 
/// receives rendering, it will appear in the Avalonia window. All other rendering functionality works as it does with other UI systems.
/// </para>
/// <para>
/// When finished, the application should destroy any resources it created in the <c>DetachedFromLogicalTree</c> event on the control. This, however, may vary depending on how things are utilized
/// throughout your application.
/// </para>
/// <para>
/// This control exposes a <see cref="Resized"/> event which users can use to handle a resize of the control and recalculate things like view size, and project matrices.
/// </para>
/// <para>
/// For an example on how to use the control properly, check the GlassCube.Avalonia example.
/// </para>
/// </remarks>
public class GorgonAvaloniaSwapChainControl
    : Control
{
    #region Variables.
    // The interop layer for the GPU.
    private ICompositionGpuInterop _gpuInterop;
    // The surface that will receive the rendering.
    private CompositionDrawingSurface _surface;
    // The visual for the surface.
    private CompositionSurfaceVisual _visual;
    // The compositor for the visual tree.
    private Compositor _compositor;
    // The internal swap chain object.
    private AvaloniaSwapChain _swapChain;
    // The idle method used to render to the swap chain.
    private Func<GorgonRenderTarget2DView, bool> _idle;
    // Flag to indicate that the next frame is queued for rendering.
    private bool _nextFrameQueued;
    #endregion

    #region Events.
    /// <summary>
    /// Event for a resized control.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ResizedEvent = RoutedEvent.Register<GorgonAvaloniaSwapChainControl, RoutedEventArgs>(nameof(Resized), RoutingStrategies.Bubble);

    /// <summary>
    /// Event triggered when the control is resized.
    /// </summary>
    public event EventHandler<RoutedEventArgs> Resized
    {
        add => AddHandler(ResizedEvent, value);
        remove => RemoveHandler(ResizedEvent, value);
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to extract the functionality for composition of the swap chain on the control.
    /// </summary>
    /// <param name="control">The control to evaluate.</param>
    private async Task GetCompositionFunctionalityAsync(Control control)
    {
        CompositionVisual visual = ElementComposition.GetElementVisual(control);

        _compositor = visual.Compositor;
        _surface = _compositor.CreateDrawingSurface();
        _visual = _compositor.CreateSurfaceVisual();
        _visual.Size = new Vector2((float)control.Bounds.Width, (float)control.Bounds.Height);
        _visual.Surface = _surface;

        ElementComposition.SetElementChildVisual(control, _visual);

        _gpuInterop = await _compositor.TryGetCompositionGpuInterop();

        if (_gpuInterop is null)
        {
            throw new GorgonException(GorgonResult.CannotCreate, Properties.Resources.GORAVA_ERR_CANNOT_RETRIEVE_INTEROP_LAYER);
        }
    }

    /// <summary>
    /// Function to clean up the resources used by the swap chain.
    /// </summary>
    private async Task CleanupAsync()
    {
        AvaloniaSwapChain swapChain = Interlocked.Exchange(ref _swapChain, null);
        CompositionDrawingSurface surface = Interlocked.Exchange(ref _surface, null);

        Interlocked.Exchange(ref _visual, null);

        if (swapChain is not null)
        {
            await swapChain.DisposeAsync();
        }

        surface?.Dispose();
    }

    /// <summary>
    /// Function called when the control is resized.
    /// </summary>
    /// <param name="e">The event parameters.</param>
    protected virtual void OnResize(RoutedEventArgs e) => RaiseEvent(e);

    /// <summary>
    /// Called when the styled element is removed from a rooted logical tree.
    /// </summary>
    /// <param name="e">The event args.</param>
    protected async override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _idle = null;

        await CleanupAsync();

        base.OnDetachedFromLogicalTree(e);
    }

    /// <summary>
    /// Function to perform an update during the idle loop.
    /// </summary>
    private void Update()
    {
        _nextFrameQueued = false;

        if ((_idle is null) || (_swapChain is null))
        {
            return;
        }

        IRenderRoot root = this.GetVisualRoot();

        if (root is null)
        {
            return;
        }

        Vector2 oldSize = new((float)_visual.Size.X, (float)_visual.Size.Y);
        Vector2 newSize = new((float)Bounds.Width, (float)Bounds.Height);

        _visual.Size = newSize;

        if ((!newSize.X.EqualsEpsilon(oldSize.X)) || (!newSize.Y.EqualsEpsilon(oldSize.Y)))
        {
            OnResize(new RoutedEventArgs(ResizedEvent));
        }

        GorgonRenderTarget2DView target = _swapChain.BeginRendering(PixelSize.FromSize(Bounds.Size, root.RenderScaling));

        // Reset the swap chain to nothing on next present (otherwise avalonia might set it for us and we'll lose track of it).
        // We don't set it to the image target right away because it might be redundant to do so since the user should be able to 
        // pick and chose when to set the target during their call.
        _swapChain.Graphics.SetRenderTarget(null);

        if (!_idle(target))
        {
            _swapChain?.Present();
            return;
        }

        _swapChain?.Present();

        _nextFrameQueued = true;
        _compositor?.RequestCompositionUpdate(Update);

        GorgonTiming.Update();
    }

    /// <summary>
    /// Function that acts as the entry point for the main loop of the application.
    /// </summary>
    /// <param name="graphics">The graphics interface used to communicate with Avalonia.</param>
    /// <param name="idle">The method to call during idle time.</param>
    /// <returns>A task for asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="graphics"/>, or the <paramref name="idle"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// Calling this method will start the rendering process for Gorgon. All rendering will take place in the <paramref name="idle"/> function which provides the main render target for the application as a parameter. 
    /// The <paramref name="idle"/> method will then return <b>true</b> to continue rendering, or <b>false</b> to exit the idle loop.
    /// </para>
    /// <para>
    /// Because this method is async, it should be awaited so that any code that comes after will be called when the idle loop is terminated.
    /// </para>
    /// </remarks>
    public async Task RunAsync(GorgonGraphics graphics, Func<GorgonRenderTarget2DView, bool> idle)
    {
        if (graphics is null)
        {
            throw new ArgumentNullException(nameof(graphics));
        }

        if (idle is null)
        {
            throw new ArgumentNullException(nameof(idle));
        }

        await GetCompositionFunctionalityAsync(this);

        _swapChain ??= new AvaloniaSwapChain(graphics, _gpuInterop, _surface);

        _idle = idle;

        if (!GorgonTiming.TimingStarted)
        {
            GorgonTiming.StartTiming<GorgonTimerQpc>();
        }

        if (_nextFrameQueued)
        {
            return;
        }

        _nextFrameQueued = true;
        _compositor?.RequestCompositionUpdate(Update);
    }
    #endregion
}
