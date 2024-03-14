using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Wpf.Properties;
using Gorgon.Math;
using Gorgon.Timing;
using Microsoft.Wpf.Interop.DirectX;

namespace Gorgon.Graphics.Wpf;

/// <summary>
/// A render target used to allow Gorgon to interoperate with WPF.
/// </summary>
/// <remarks>
/// <para>
/// This is a special render target type that is to be used in conjunction with Microsoft.Wpf.Interop.DirectX.D3D11Image (<a href="https://github.com/Microsoft/WPFDXInterop"/>). 
/// </para>
/// <para>
/// When rendering with this render target type, there is no need for a <see cref="GorgonSwapChain"/>. Applications will only need to set this render target as the current target and all rendering 
/// will route to the WPF surface. This makes using Gorgon with WPF fairly simple. However, there are some limitations to note: 
/// <list type="bullet">
///     <item>
///         <description>The included version of Microsoft.Wpf.Interop.DirectX.D3D11Image is for x64 <b>only</b>. This means that your application must be set to x64 in the project properties. </description>
///     </item>
///     <item>
///         <description>Due to limitations in WPF, the frame rate is locked to 60 FPS.</description>
///     </item>
///     <item>
///         <description>Exclusive full screen is <b>not</b> supported. However, windowed full screen can be enabled by setting up your WPF Window to maximized, with no window decoration (e.g. title bar, resize border, etc...).</description>
///     </item>
///     <item>
///         <description>The backbuffer format is fixed to <see cref="BufferFormat.B8G8R8A8_UNorm"/>.</description>
///     </item>
/// </list>
/// </para>
/// <para>
/// Because WPF works so very differently from Windows Forms, handling the idle time when rendering is supposed to happen is handled differently. Developers will no longer need to call 
/// <c>GorgonApplication.Run</c> in Program.cs and pass in their idle method callback. Instead, developers will pass in the idle method into the <see cref="Run"/> method on this target type. 
/// From that point forward, Gorgon will work the same as it always did.
/// </para>
/// </remarks>
/// <example>
///     <para>
///     Below is a small example showing how to initialize and use the WPF target.
///     </para>
///     <code lang="xaml">
///     <![CDATA[
///     <Window x:Class="Gorgon.Examples.MainWindow"
///         xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
///         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
///         xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
///         xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
///         xmlns:local="clr-namespace:WpfGorgon"
///         xmlns:DXExtensions="clr-namespace:Microsoft.Wpf.Interop.DirectX;assembly=Microsoft.Wpf.Interop.DirectX"
///         mc:Ignorable="d"
///         Title="WPF in Gorgon" Height="800" Width="1280"
///         Loaded="Window_Loaded"
///         Closing="Window_Closing">
///     
///         <!-- This is where we'll see our rendered data -->
///         <Image Stretch="Fill" x:Name="Wpf" Grid.ColumnSpan="2">
///             <Image.Source>
///                 <!-- This must be assigned as the image source -->
///                 <DXExtensions:D3D11Image />
///             </Image.Source>
///         </Image>
///     </Window>
///     ]]>
///     </code>
///     <code lang="csharp">
///     <![CDATA[
///     using System;
///     using System.Collections.Generic;
///     using System.Windows;
///     using System.Windows.Input;
///     using Gorgon.Graphics;
///     using Gorgon.Graphics.Core;
///     using Gorgon.Graphics.Wpf;
///     using Gorgon.Core;
///     
///     private GorgonGraphics _graphics;
///     private GorgonWpfTarget _target;
///     
///     private bool Idle()
///     {
///         // We'll just clear the screen here.  Exciting.
///         _graphics.SetRenderTarget(_target.RenderTargetView);
///         
///         _target.RenderTargetView(GorgonColor.CornFlowerBlue);
/// 
///         return true;
///     }
///     
///     // Handle the window shut down.
///     private void MainWindow_Closing(object sender, CancelEventArgs e)
///     {
///         // ALWAYS dispose your objects.
///         _target?.Dispose();
///         _graphics?.Dispose();
///     }
/// 
///     // In your MainWindow.xaml.cs file:
///     private void MainWindow_Loaded(object sender, RoutedEventArgs e)
///     {
///         // Initialize Gorgon as we have in the other examples.
///         // Find out which devices we have installed in the system.
///         IReadOnlyList<IGorgonVideoAdapterInfo> deviceList = GorgonGraphics.EnumerateAdapters();
///
///         if (deviceList.Count == 0)
///         {
///             MessageBox.Show("There are no suitable video adapters available in the system. This example is unable to continue and will now exit.", 
///                             "Error", 
///                             MessageBoxButton.OK, 
///                             MessageBoxImage.Error);
///
///             Close();
///             return;
///         }
///
///         // Create the graphics interface.
///         _graphics = new GorgonGraphics(deviceList[0]);
///
///         // Create the WPF render target.
///         // We pass in an Image component type here. This Image component MUST have its ImageSource property bound to a D3D11Image 
///         // object, otherwise this will not work.
///         _target = new GorgonWpfTarget(_graphics, new GorgonWpfTargetInfo(WpfImage, "WPF Render Target"));
///         
///         // Unlike our Windows Forms code, we do not use GorgonApplication.Run(), instead we pass the render method (Idle) into 
///         // the target itself.
///         _target.Run(Idle);
///     }
///     ]]>
///     </code>
/// </example>
public class GorgonWpfTarget
    : IGorgonWpfTargetInfo, IGorgonGraphicsObject, IDisposable
{
    #region Variables.
    // The window containing the D3DImage component.
    private Window _window;
    // The control that will receive rendering.
    private FrameworkElement _renderControl;
    // The interop image source.
    private D3D11Image _d3dImage;
    // The information used to create the target.
    private readonly GorgonWpfTargetInfo _info;
    // Flag to indicate rendering has started.
    private int _started;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the backing texture for the WPF surface to render into.
    /// </summary>
    public GorgonTexture2D Texture
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the render target view for the <see cref="Texture"/> resource.
    /// </summary>
    public GorgonRenderTarget2DView RenderTargetView
    {
        get;
        private set;
    }

    /// <summary>Property to return the graphics interface that built this object.</summary>
    public GorgonGraphics Graphics
    {
        get;                    
    }

    /// <summary>Property to return the function to call when rendering occurs.</summary>
    /// <remarks>This takes a method that returns a <see cref="bool" /> value. When the method returns <b>true</b>, rendering will continue, when it returns <b>false</b>, then rendering will stop.</remarks>
    public Func<bool> Idle
    {
        get;
        private set;
    }

    /// <summary>Property to return the image that will receive the rendered data.</summary>
    /// <remarks>This is a WPF image control that Gorgon will render into. The image source for this must be set to a <c>D3D11Image</c> control.</remarks>
    public Image RenderImage
    {
        get;
    }

    /// <summary>Property to return the name of this object.</summary>
    public string Name
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to build a render target from the WPF surface.
    /// </summary>
    private void BuildRenderTarget(nint surfacePtr)
    {
        RenderTargetView?.Dispose();
        Texture?.Dispose();

        RenderTargetView = GorgonRenderTarget2DView.CreateInteropRenderTarget(Graphics, surfacePtr, Name);
        Texture = RenderTargetView.Texture;            
    }

    /// <summary>
    /// Function called when WPF is ready to compose the view.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void CompositionTarget_Rendering(object sender, EventArgs e)
    {
        if (GorgonTiming.TimingStarted)
        {
            GorgonTiming.Update();
        }

        _d3dImage.RequestRender();
    }

    /// <summary>
    /// Function called when the host control is resized.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void OnResize(object sender, RoutedEventArgs e)
    {
        if ((_renderControl.ActualWidth <= 0) || (_renderControl.ActualHeight <= 0) || (_window.WindowState == WindowState.Minimized))
        {
            return;
        }

        _d3dImage.SetPixelSize((int)_renderControl.ActualWidth.FastCeiling(), (int)_renderControl.ActualHeight.FastCeiling());
    }

    /// <summary>
    /// Function to render to the render target when rendering is requested by WPF.
    /// </summary>
    /// <param name="surfacePtr">The pointer to the internal WPF surface.</param>
    /// <param name="needsNewRtv"><b>true</b> if the internal surface has been updated and the render target needs to be rebuilt, <b>false</b> if it does not.</param>
    private void Render(nint surfacePtr, bool needsNewRtv)
    {
        if (surfacePtr == IntPtr.Zero)
        {
            Stop();
            return;
        }

        if (needsNewRtv) 
        {
            BuildRenderTarget(surfacePtr);
        }

        if ((_window.WindowState == WindowState.Minimized) || (Idle is null))
        {
            return;
        }

        bool keepRunning = Idle();

        Graphics.Flush();

        if (!keepRunning)
        {
            Stop();
        }
    }

    /// <summary>
    /// Function to initialize the component.
    /// </summary>
    private void Initialize()
    {
        // Locate the render control for the D3D Image.
        _d3dImage = _info.RenderImage.Source as D3D11Image;

        if (_d3dImage is null)
        {
            throw new GorgonException(GorgonResult.CannotCreate, Resources.GORWPF_NOT_A_D3DIMAGE);
        }

        _renderControl = VisualTreeHelper.GetParent(_info.RenderImage) as FrameworkElement;

        if (_renderControl is null)
        {
            throw new GorgonException(GorgonResult.CannotCreate, Resources.GORWPF_CANNOT_FIND_PARENT);
        }

        // Locate the main window.
        _window = Window.GetWindow(_renderControl);

        if (_window is null)
        {
            throw new GorgonException(GorgonResult.CannotCreate, Resources.GORWPF_NO_APP_WINDOW);
        }

        var hwnd = new WindowInteropHelper(_window);
        _d3dImage.WindowOwner = hwnd.Handle;
        _d3dImage.OnRender = Render;

        OnResize(_renderControl, new RoutedEventArgs());

        WeakEventManager<FrameworkElement, SizeChangedEventArgs>.AddHandler(_renderControl, nameof(_renderControl.SizeChanged), OnResize);
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        WeakEventManager<Window, SizeChangedEventArgs>.RemoveHandler(_window, nameof(_window.SizeChanged), OnResize);
        CompositionTarget.Rendering -= CompositionTarget_Rendering;
        Interlocked.Exchange(ref _started, 0);
        Idle = null;

        RenderTargetView?.Dispose();
        RenderTargetView = null;
        Texture = null;
    }

    /// <summary>
    /// Function to begin rendering.
    /// </summary>
    /// <param name="renderMethod">The method to call when rendering occurs.</param>
    /// <remarks>
    /// <para>
    /// Applications must call this method to begin rendering.
    /// </para>
    /// </remarks>
    public void Run(Func<bool> renderMethod)
    {
        if ((_d3dImage is null) || (Interlocked.Exchange(ref _started, 1) == 1))
        {
            return;
        }

        Idle = renderMethod;

        if (Idle is null)
        {
            Interlocked.Exchange(ref _started, 0);
            return;
        }

        if (!GorgonTiming.TimingStarted)
        {
            GorgonTiming.StartTiming<GorgonTimerQpc>();
        }

        _d3dImage.RequestRender();
        CompositionTarget.Rendering += CompositionTarget_Rendering;
    }

    /// <summary>
    /// Function to stop rendering.
    /// </summary>
    public void Stop()
    {
        if (Interlocked.Exchange(ref _started, 0) == 0)
        {
            return;
        }

        CompositionTarget.Rendering -= CompositionTarget_Rendering;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="GorgonWpfTarget" /> class.</summary>
    /// <param name="graphics">The graphics object used to create the necessary resources.</param>
    /// <param name="info">The information used to build the object.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="info"/> parameter is <b>null</b>.</exception>
    public GorgonWpfTarget(GorgonGraphics graphics, IGorgonWpfTargetInfo info)
    {
        if (info is null)
        {
            throw new ArgumentNullException(nameof(info));
        }

        _info = new GorgonWpfTargetInfo(info);
        Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));

        Initialize();
    }
    #endregion
}
