
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: May 14, 2019 11:11:34 AM
// 

using Gorgon.Diagnostics;
using Gorgon.Memory;
using Gorgon.Timing;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A factory for creating/retrieving render targets for temporary use
/// </summary>
/// <remarks>
/// <para>
/// During the lifecycle of an application, many render targets may be required. In most instances, creating a render target for a scene and disposing of it when done is all that is required. But, when 
/// dealing with effects, shaders, etc... render targets may need to be created and released during a frame and doing this several times in a frame can be costly
/// </para>
/// <para>
/// This factory allows applications to "rent" a render target and return it when done with only an initial cost when creating a target for the first time. This way, temporary render targets can be reused 
/// when needed
/// </para>
/// <para>
/// When the factory retrieves a target, it will check its internal pool and see if a render target already exists. If it exists, and is not in use, it will return the existing target. If the target 
/// does not exist, or is being used elsewhere, then a new target is created and added to the pool
/// </para>
/// <para>
/// Render targets allocated by the factory will be reclaimed over time so memory usage will not get out of control. The default is 2.5 minutes, which is adjustable via the <see cref="ExpiryTime"/> 
/// proeprty after each target is returned. This only affects targets that are not currently rented out, so there should be no danger of reclaiming a target that is in use
/// </para>
/// <para>
/// Targets retrieved by this factory must be returned when they are no longer needed, otherwise the purpose of the factory is defeated. 
/// </para>
/// </remarks>
/// <remarks>Initializes a new instance of the <see cref="RenderTargetFactory"/> class.</remarks>
/// <param name="graphics">The graphics interface that owns this factory.</param>
internal class RenderTargetFactory(GorgonGraphics graphics)
        : IDisposable, IGorgonRenderTargetFactory
{

    // The list of render targets handled by this factory.
    private readonly List<GorgonRenderTarget2DView> _renderTargets = [];
    // The list of preallocated default shader resource views.
    private readonly List<GorgonShaderResourceView> _srvs = [];
    // The list of indexes for previously rented targets.
    private readonly HashSet<GorgonRenderTarget2DView> _rented = [];
    // The graphics interface that owns this factory.
    private readonly GorgonGraphics _graphics = graphics;
    // The time at which a render target was last returned from rental.
    private readonly Dictionary<GorgonRenderTarget2DView, double> _expiryTime = [];
    // The list used to clean up expired targets.
    private readonly List<GorgonRenderTarget2DView> _cleanupList = [];
    // The timer used to expire the render targets.
    private readonly IGorgonTimer _expiryTimer = GorgonTimerQpc.SupportsQpc() ? new GorgonTimerQpc() : new GorgonTimerMultimedia();
    // An allocator for creating texture info objects.
    private readonly GorgonRingPool<TempTargetTextureInfo> _textureInfoAllocator = new(100, () => new TempTargetTextureInfo());

    /// <summary>
    /// Property to return the number of render targets that are currently in flight.
    /// </summary>
    public int RentedCount => _rented.Count;

    /// <summary>
    /// Property to return the number of free render targets available for rent.
    /// </summary>
    public int AvailableCount => _renderTargets.Count;

    /// <summary>
    /// Property to return the total number of render targets available in the factory.
    /// </summary>
    public int TotalCount => _rented.Count + _renderTargets.Count;

    /// <summary>
    /// Property to set or return the maximum lifetime for an unrented render target, in minutes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value applies only to newly created render targets via the <see cref="Rent"/> method. Any previously created targets will use whatever expiry time that was set previously.
    /// </para>
    /// <para>
    /// The default value is 0.5 minutes.
    /// </para>
    /// </remarks>
    public double ExpiryTime
    {
        get;
        set;
    } = 0.5;

    /// <summary>
    /// Function to expire any previously allocated targets after a certain amount of time.
    /// </summary>
    /// <param name="force"><b>true</b> to clear all unrented targets immediately, <b>false</b> to only remove targets that have expired past the <see cref="ExpiryTime"/>.</param>
    /// <remarks>
    /// <para>
    /// Applications should call this method to free up memory associated with the cached render targets.  This only affects targets that are not currently rented out.
    /// </para>
    /// </remarks>
    /// <seealso cref="ExpiryTime"/>
    public void ExpireTargets(bool force = false)
    {
        double currentMinutes = _expiryTimer.Minutes;
        _cleanupList.Clear();

        foreach (KeyValuePair<GorgonRenderTarget2DView, double> time in _expiryTime)
        {
            // If we're already rented, then do nothing.
            if ((_rented.Contains(time.Key)) || (!_renderTargets.Contains(time.Key)))
            {
                continue;
            }

            // If a target has been alive for more than time limit, then dump it.
            if ((!force) && ((currentMinutes - time.Value) < 0))
            {
                continue;
            }

            _cleanupList.Add(time.Key);
        }

        foreach (GorgonRenderTarget2DView target in _cleanupList)
        {
            GorgonShaderResourceView srv = target.GetShaderResourceView();

            if (_srvs.Contains(srv))
            {
                srv.Dispose();
                _srvs.Remove(srv);
            }

            target.OwnerFactory = null;
            _renderTargets.Remove(target);
            _expiryTime.Remove(target);
            target.Dispose();
        }

        _cleanupList.Clear();
    }

    /// <summary>
    /// Function to rent a render target from the factory.
    /// </summary>
    /// <param name="targetInfo">The information about the render target to retrieve.</param>
    /// <param name="name">A unique user defined name for a new render target.</param>
    /// <param name="clearOnRetrieve">[Optional] <b>true</b> to clear the render target when retrieved, or <b>false</b> to leave the contents as-is.</param>
    /// <returns>The requested render target.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="targetInfo"/>, or the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// All calls to this method should be paired with a call to the <see cref="Return"/> method.  Failure to do so may result in a leak.
    /// </para>
    /// <para>
    /// The optional <paramref name="clearOnRetrieve"/> parameter, if set to <b>true</b>,, will clear the contents of a render target that is being reused 
    /// prior to returning it.  In some cases this is not ideal, so setting it to <b>false</b> will preserve the contents.  New render targets will always 
    /// be cleared.
    /// </para>
    /// <note type="caution">
    /// <para>
    /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
    /// </para>
    /// </note>
    /// </remarks>
    public GorgonRenderTarget2DView Rent(IGorgonTexture2DInfo targetInfo, string name, bool clearOnRetrieve = true)
    {
        name.ValidateString(nameof(name));
        targetInfo.ValidateObject(nameof(targetInfo));

        ExpireTargets();

        // Ensure the information is valid.            
        TempTargetTextureInfo newInfo = _textureInfoAllocator.Allocate();
        newInfo.Initialize(targetInfo);
        newInfo.Name = name;

        for (int i = 0; i < _renderTargets.Count; ++i)
        {
            GorgonRenderTarget2DView rtv = _renderTargets[i];
            GorgonTexture2D target = _renderTargets[i].Texture;

            if ((!_rented.Contains(rtv)) && (target.Width == newInfo.Width) && (target.Height == newInfo.Height) && (target.MipLevels == newInfo.MipLevels)
                && (target.ArrayCount == newInfo.ArrayCount) && (target.Format == newInfo.Format) && (target.Binding == newInfo.Binding)
                && (target.MultisampleInfo.Equals(newInfo.MultisampleInfo)) && (newInfo.IsCubeMap == target.IsCubeMap)
                && (string.Equals(newInfo.Name, rtv.Texture.Name, StringComparison.OrdinalIgnoreCase)))
            {
                if (clearOnRetrieve)
                {
                    rtv.Clear(GorgonColors.BlackTransparent);
                }

                _renderTargets.Remove(rtv);
                _expiryTime.Remove(rtv);
                _rented.Add(rtv);
                return rtv;
            }
        }

        if (_renderTargets.Count == 0)
        {
            _expiryTimer.Reset();
        }

        GorgonRenderTarget2DView newRtv = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, newInfo);
        // Cache a default shader resource view (the texture holds the cache, we hold a separate one so we can clean it up later).
        _srvs.Add(newRtv.GetShaderResourceView());
        newRtv.OwnerFactory = this;
        _rented.Add(newRtv);
        newRtv.Clear(GorgonColors.BlackTransparent);
        return newRtv;
    }

    /// <summary>
    /// Function to return the render target to the factory.
    /// </summary>
    /// <param name="rtv">The render target to return.</param>
    /// <returns><b>true</b> if the target was returned successfully, <b>false</b> if not.</returns>
    public bool Return(GorgonRenderTarget2DView rtv)
    {
        ExpireTargets();

        if ((rtv is null) || (rtv.OwnerFactory != this))
        {
            return false;
        }

        if (!_rented.Remove(rtv))
        {
            return false;
        }

        _renderTargets.Add(rtv);
        _expiryTime[rtv] = _expiryTimer.Minutes + (ExpiryTime < 0 ? 0 : ExpiryTime);
        return true;
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        _expiryTime.Clear();

        foreach (GorgonShaderResourceView srv in _srvs)
        {
            srv.Dispose();
        }

        foreach (GorgonRenderTarget2DView rtv in _renderTargets)
        {
            rtv.OwnerFactory = null;
            rtv.Dispose();
        }

        // For currently rented targets, detach from the factory so we don't disrupt anyting. This should not happen, but better safe than sorry.
        foreach (GorgonRenderTarget2DView rtv in _rented)
        {
            rtv.OwnerFactory = null;
        }

        _rented.Clear();
        _srvs.Clear();
        _renderTargets.Clear();
    }
}
