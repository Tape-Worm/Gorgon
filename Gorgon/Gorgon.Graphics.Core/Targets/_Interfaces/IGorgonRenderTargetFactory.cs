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
// Created: May 14, 2019 11:50:37 AM
// 
#endregion

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A factory for creating/retrieving render targets for temporary use.
    /// </summary>
    /// <remarks>
    /// <para>
    /// During the lifecycle of an application, many render targets may be required. In most instances, creating a render target for a scene and disposing of it when done is all that is required. But, when 
    /// dealing with effects, shaders, etc... render targets may need to be created and released during a frame and doing this several times in a frame can be costly.
    /// </para>
    /// <para>
    /// This factory allows applications to "rent" a render target and return it when done with only an initial cost when creating a target for the first time. This way, temporary render targets can be reused 
    /// when needed.
    /// </para>
    /// <para>
    /// When the factory retrieves a target, it will check its internal pool and see if a render target already exists. If it exists, and is not in use, it will return the existing target. If the target 
    /// does not exist, or is being used elsewhere, then a new target is created and added to the pool.
    /// </para>
    /// <para>
    /// Targets retrieved by this factory must be returned when they are no longer needed, otherwise the purpose of the factory is defeated. As such, best practice is to rent and return a render target during 
    /// the lifetime of a single frame. If the <see cref="GorgonGraphics.IsDebugEnabled"/> property is set to <b>true</b>, then after a swap chain presentation, a warning will be sent to the application log 
    /// complaining about render targets still rented out at the end of the frame.
    /// </para>
    /// </remarks>
    public interface IGorgonRenderTargetFactory
    {
        #region Properties.
        /// <summary>
        /// Property to return the number of render targets that are currently in flight.
        /// </summary>
        int RentedCount
        {
            get;
        }

        /// <summary>
        /// Property to return the total number of render targets available in the factory.
        /// </summary>
        int TotalCount
        {
            get;
        }

        /// <summary>
        /// Property to set or return the maximum lifetime for an unrented render target, in minutes.
        /// </summary>
        double ExpiryTime
        {
            get;
            set;
        }
        #endregion

        #region Methods.
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
        void ExpireTargets(bool force = false);

        /// <summary>
        /// Function to rent a render target from the factory.
        /// </summary>
        /// <param name="targetInfo">The information about the render target to retrieve.</param>
        /// <param name="name">A unique user defined name for a new render target.</param>
        /// <param name="clearOnRetrieve">[Optional] <b>true</b> to clear the render target when retrieved, or <b>false</b> to leave the contents as-is.</param>
        /// <returns>The requested render target.</returns>
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
        GorgonRenderTarget2DView Rent(IGorgonTexture2DInfo targetInfo, string name, bool clearOnRetrieve = true);

        /// <summary>
        /// Function to return the render target to the factory.
        /// </summary>
        /// <param name="rtv">The render target to return.</param>
        /// <returns><b>true</b> if the target was returned successfully, <b>false</b> if not.</returns>
        bool Return(GorgonRenderTarget2DView rtv);
        #endregion
    }
}