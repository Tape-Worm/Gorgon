
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
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
// Created: February 6, 2020 9:57:02 PM
// 

using System.ComponentModel;
using System.Numerics;
using Gorgon.Core;
using Gorgon.Editor.Tools;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers;

namespace Gorgon.Editor.Rendering;

/// <summary>
/// A default implementation of a <see cref="IToolRenderer"/>
/// </summary>
/// <typeparam name="T">The type of view model for the renderer. Must implement the <see cref="IEditorTool"/> interface, and be a reference type.</typeparam>
/// <remarks>
/// <para>
/// This renderer does the bare minimum to present content on the view. Tool plug-in UI developers should inherit from this class to take advantage of the default functionality it provides. 
/// </para>
/// <para>
/// The default renderer provides basic support for rendering tool specific content
/// </para>
/// <para>
/// Renderers will also receive access to the view model applied to the view, so the renderer can respond to changes on the tool UI and adjust the visuals appropriately. The view model must implement 
/// the <see cref="IEditorTool"/> interface before they can be used with a renderer
/// </para>
/// </remarks>
/// <seealso cref="IToolRenderer"/>
/// <seealso cref="IEditorTool"/>
/// <seealso cref="Gorgon2D"/>
public class DefaultToolRenderer<T>
    : IGorgonNamedObject, IToolRenderer
    where T : class, IEditorTool
{
    // Flag to indicate that the resources are loaded.
    private int _resourcesLoading;
    // The swap chain for the content view.
    private readonly GorgonSwapChain _swapChain;

    /// <summary>
    /// Property to return the default texture used to draw the background.
    /// </summary>
    protected GorgonTexture2DView BackgroundPattern
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the primary render target.
    /// </summary>
    /// <remarks>
    /// Developers can use this property to reset the render target back to the original target after rendering to another target.
    /// </remarks>
    protected GorgonRenderTarget2DView MainRenderTarget => _swapChain?.RenderTargetView;

    /// <summary>
    /// Property to return the 2D renderer used to draw onto the content view.
    /// </summary>
    protected Gorgon2D Renderer
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the graphics interface used to create graphics objects.
    /// </summary>
    protected GorgonGraphics Graphics => Renderer?.Graphics;

    /// <summary>
    /// Property to return the pixel format for the view.
    /// </summary>
    protected BufferFormat PixelFormat => _swapChain.Format;

    /// <inheritdoc/>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Property to return the size of the view client area.
    /// </summary>
    public GorgonPoint ClientSize
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to set or return whether the renderer is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the color to use when clearing the swap chain.
    /// </summary>
    /// <remarks>
    /// This value defaults to the background color of the view.
    /// </remarks>
    public GorgonColor BackgroundColor
    {
        get;
        set;
    }

    /// <summary>Property to return the data context assigned to this view.</summary>
    public T DataContext
    {
        get;
        private set;
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    private void SetDataContext(T dataContext)
    {
        if (DataContext is not null)
        {
            DataContext.PropertyChanging -= DataContext_PropertyChanging;
            DataContext.PropertyChanged -= DataContext_PropertyChanged;
        }

        DataContext = dataContext;

        if (DataContext is null)
        {
            return;
        }

        DataContext.PropertyChanging += DataContext_PropertyChanging;
        DataContext.PropertyChanged += DataContext_PropertyChanged;
    }

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (IsEnabled)
        {
            OnPropertyChanged(e.PropertyName);
        }
    }

    /// <summary>Handles the PropertyChanging event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e)
    {
        if (IsEnabled)
        {
            OnPropertyChanging(e.PropertyName);
        }
    }

    /// <summary>Handles the AfterSwapChainResized event of the SwapChain control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SwapChainResizedEventArgs"/> instance containing the event data.</param>
    private void SwapChain_AfterSwapChainResized(object sender, SwapChainResizedEventArgs e)
    {
        // Because we have multiple swap chains, we need to reset it back to our swap chain.
        // Otherwise the camera will use the correct swap chain.
        if (Graphics.RenderTargets[0] != _swapChain.RenderTargetView)
        {
            Graphics.SetRenderTarget(_swapChain.RenderTargetView);
        }

        ClientSize = e.Size;
        OnResizeEnd();
    }

    /// <summary>Handles the BeforeSwapChainResized event of the SwapChain control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SwapChainResizingEventArgs"/> instance containing the event data.</param>
    private void SwapChain_BeforeSwapChainResized(object sender, SwapChainResizingEventArgs e) => OnResizeBegin();

    /// <summary>
    /// Function to calculate scaling to the specified size, bounded by the client area of the rendering control.
    /// </summary>
    /// <param name="size">The size of the area to zoom into.</param>
    /// <param name="windowSize">The size of the window.</param>
    /// <returns>The scaling factor to apply.</returns>
    protected float CalculateScaling(Vector2 size, Vector2 windowSize)
    {
        Vector2 scaling = new(windowSize.X / size.X, windowSize.Y / size.Y);

        return scaling.X.Min(scaling.Y);
    }

    /// <summary>
    /// Function called when a property on the <see cref="DataContext"/> is changing.
    /// </summary>
    /// <param name="propertyName">The name of the property that is changing.</param>
    /// <remarks>
    /// <para>
    /// Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.
    /// </para>
    /// </remarks>
    protected virtual void OnPropertyChanging(string propertyName)
    {

    }

    /// <summary>
    /// Function called when a property on the <see cref="DataContext"/> has been changed.
    /// </summary>
    /// <param name="propertyName">The name of the property that was changed.</param>
    /// <remarks>
    /// <para>
    /// Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.
    /// </para>
    /// </remarks>
    protected virtual void OnPropertyChanged(string propertyName)
    {

    }

    /// <summary>
    /// Function called when the view is about to be resized.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Developers can override this method to handle cases where the view window is resized and the content has size dependent data (e.g. render targets).
    /// </para>
    /// </remarks>
    protected virtual void OnResizeBegin()
    {
    }

    /// <summary>
    /// Function called when the view has been resized.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Developers can override this method to handle cases where the view window is resized and the content has size dependent data (e.g. render targets).
    /// </para>
    /// </remarks>
    protected virtual void OnResizeEnd()
    {
    }

    /// <summary>
    /// Function called when the renderer needs to load any resource data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Developers can override this method to set up their own resources specific to their renderer. Any resources set up in this method should be cleaned up in the associated 
    /// <see cref="OnUnload"/> method.
    /// </para>
    /// </remarks>
    protected virtual void OnLoad()
    {

    }

    /// <summary>
    /// Function called when the renderer needs to clean up any resource data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Developers should always override this method if they've overridden the <see cref="OnLoad"/> method. Failure to do so can cause memory leakage.
    /// </para>
    /// </remarks>
    protected virtual void OnUnload()
    {
    }

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing">
    ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        UnloadResources();

        if (DataContext is not null)
        {
            SetDataContext(null);
        }
    }

    /// <summary>
    /// Function to render the background.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Developers can override this method to render a custom background.
    /// </para>
    /// </remarks>
    protected virtual void OnRenderBackground()
    {
        GorgonRectangleF textureSize = new(0, 0, ClientSize.X / (float)BackgroundPattern.Width, ClientSize.Y / (float)BackgroundPattern.Height);

        Renderer.Begin();
        Renderer.DrawFilledRectangle(new GorgonRectangleF(0, 0, ClientSize.X, ClientSize.Y), GorgonColors.White, BackgroundPattern, textureSize);
        Renderer.End();
    }

    /// <summary>
    /// Function to render the content.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the method that developers should override in order to draw their content to the view. 
    /// </para>
    /// </remarks>
    protected virtual void OnRenderContent()
    {
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Function to load resources for the renderer.</summary>
    /// <remarks>
    /// This method is used to load any required temporary resources for the renderer prior to rendering content. This must be paired with a call to <see cref="UnloadResources"/> when the renderer is
    /// no longer in use to ensure efficient memory usage.
    /// </remarks>
    public void LoadResources()
    {
        if (Interlocked.Exchange(ref _resourcesLoading, 1) == 1)
        {
            return;
        }

        Graphics.SetRenderTarget(_swapChain.RenderTargetView);

        BackgroundPattern = GorgonTexture2DView.CreateTexture(Graphics, new GorgonTexture2DInfo(CommonEditorResources.CheckerBoardPatternImage.Width,
                                                                                                     CommonEditorResources.CheckerBoardPatternImage.Height,
                                                                                                     CommonEditorResources.CheckerBoardPatternImage.Format)
        {
            Name = "Tool_Bg_Pattern",
            Usage = ResourceUsage.Immutable
        }, CommonEditorResources.CheckerBoardPatternImage);

        ClientSize = new GorgonPoint(_swapChain.Width, _swapChain.Height);

        _swapChain.SwapChainResizing += SwapChain_BeforeSwapChainResized;
        _swapChain.SwapChainResized += SwapChain_AfterSwapChainResized;

        // Let the custom renderer set up its own stuff.
        OnLoad();
    }

    /// <summary>Function to render the content.</summary>
    /// <remarks>This method is called by the view to render the content.</remarks>
    public void Render()
    {
        if (!IsEnabled)
        {
            return;
        }

        _swapChain.RenderTargetView.Clear(BackgroundColor);
        Graphics.SetRenderTarget(_swapChain.RenderTargetView);

        OnRenderBackground();

        OnRenderContent();

        _swapChain.Present(1);
    }

    /// <summary>Function to unload resources from the renderer.</summary>
    /// <remarks>This method is used to unload temporary resources for the renderer when it is no longer needed. Failure to call this may result in memory leakage.</remarks>
    public void UnloadResources()
    {
        if (Interlocked.Exchange(ref _resourcesLoading, 0) == 0)
        {
            return;
        }

        OnUnload();

        _swapChain.SwapChainResized -= SwapChain_AfterSwapChainResized;
        _swapChain.SwapChainResizing -= SwapChain_BeforeSwapChainResized;

        BackgroundPattern?.Dispose();
        BackgroundPattern = null;
    }

    /// <summary>Initializes a new instance of the <see cref="DefaultToolRenderer{T}"/> class.</summary>
    /// <param name="name">The name of the renderer.</param>
    /// <param name="renderer">The main renderer for the content view.</param>
    /// <param name="swapChain">The swap chain for the content view.</param>
    /// <param name="dataContext">The view model to assign to the renderer.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    protected internal DefaultToolRenderer(string name, Gorgon2D renderer, GorgonSwapChain swapChain, T dataContext)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        Name = name;
        Renderer = renderer;
        _swapChain = swapChain;
        ClientSize = new GorgonPoint(swapChain.Width, swapChain.Height);

        SetDataContext(dataContext);
    }
}
