using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Gorgon.Graphics.Core;

namespace Gorgon.Graphics.Avalonia;

/// <summary>
/// The current state for the image.
/// </summary>
internal enum AvaloniaImageState
{
    /// <summary>
    /// The image is available for use.
    /// </summary>
    Available = 0,
    /// <summary>
    /// The image was presented, but failed to be shown.
    /// </summary>
    Error = 1,
    /// <summary>
    /// The image is currently in flight for presenting.
    /// </summary>
    InFlight = 2
}

/// <summary>
/// An individual image for an avalonia swap chain.
/// </summary>
internal class AvaloniaSwapChainImage
    : IGorgonGraphicsObject, IAsyncDisposable
{
    #region Variables.
    // The interop layer for the GPU.
    private readonly ICompositionGpuInterop _interop;
    // The surface that will receive the texture data.
    private readonly CompositionDrawingSurface _surface;
    // The size of the surface.
    private readonly PixelSize _pixelSize;
    // The render target texture that will receive rendering data.
    private GorgonTexture2D _texture;
    // The shared texture interface.
    private IGorgonSharedResource _sharedTexture;
    // The handle for a shared texture.
    private nint _textureHandle;
    // Image properties for the image when transferring to avalonia.
    private PlatformGraphicsExternalImageProperties _properties;
    // The image imported into the GPU.
    private ICompositionImportedGpuImage _imported;
    // The last presentation task.
    private Task _lastPresentation;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the current render target view for the image.
    /// </summary>
    public GorgonRenderTarget2DView RenderTargetView
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the graphics instance for this swap chain texture.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the state of the image.
    /// </summary>
    public AvaloniaImageState ImageState => _lastPresentation?.Status switch
    {
        TaskStatus.RanToCompletion => AvaloniaImageState.Available,
        TaskStatus.Faulted => AvaloniaImageState.Error,
        null => AvaloniaImageState.Available,
        _ => AvaloniaImageState.InFlight
    };

    /// <summary>
    /// Property to return the width of the image, in pixels.
    /// </summary>
    public int Width => _pixelSize.Width;

    /// <summary>
    /// Property to return the height of the image, in pixels.
    /// </summary>
    public int Height => _pixelSize.Height;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to initialize the swap chain image.
    /// </summary>
    private void Initialize()
    {
        _texture = new GorgonTexture2D(Graphics, new GorgonTexture2DInfo(Width, Height, BufferFormat.R8G8B8A8_UNorm)
        {
            Name = $"AvaloniaSwapChainTexture_{Guid.NewGuid():N}",
            Binding = TextureBinding.RenderTarget | TextureBinding.ShaderResource,
            Usage = ResourceUsage.Default,
            Shared = TextureSharingOptions.SharedKeyedMutex
        });

        _sharedTexture = _texture;
        _textureHandle = _sharedTexture.GetSharedHandle();

        _properties = new PlatformGraphicsExternalImageProperties
        {
            Width = Width,
            Height = Height,
            Format = PlatformGraphicsExternalImageFormat.B8G8R8A8UNorm,
            TopLeftOrigin = true
        };

        RenderTargetView = _texture.GetRenderTargetView();

        _imported = _interop.ImportImage(new PlatformHandle(_textureHandle, KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureGlobalSharedHandle), _properties);
    }

    /// <summary>
    /// Function to asynchronously dispose of the resources used by the image.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> for asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if ((_lastPresentation is not null)
            && (_lastPresentation.Status != TaskStatus.RanToCompletion))
        {
            try
            {
                await _lastPresentation;
            }
            catch
            {
            }
        }

        _lastPresentation = null;
        _sharedTexture = null;

        RenderTargetView?.Dispose();
        RenderTargetView = null;
        _texture?.Dispose();

        if (_imported is null)
        {
            return;
        }

        await _imported.DisposeAsync();
    }

    /// <summary>
    /// Function that needs to be called prior to rendering.
    /// </summary>
    public void Begin() => _sharedTexture?.Acquire(0, int.MaxValue);

    /// <summary>
    /// Function end rendering and send the rendering data to the Avalonia surface.
    /// </summary>
    public void Present()
    {
        _sharedTexture?.Release(1);
        _lastPresentation = _surface.UpdateWithKeyedMutexAsync(_imported, 1, 0);
    }
    #endregion

    #region Constructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaSwapChainImage"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface for the swap chain texture.</param>
    /// <param name="pixelSize">The size of the texture.</param>
    /// <param name="interop">The GPU interop layer.</param>
    /// <param name="surface">The avalonia surface that will receive the texture data.</param>
    public AvaloniaSwapChainImage(GorgonGraphics graphics, PixelSize pixelSize, ICompositionGpuInterop interop, CompositionDrawingSurface surface)
    {
        Graphics = graphics;
        _pixelSize = pixelSize;
        _interop = interop;
        _surface = surface;

        Initialize();
    }
    #endregion
}
