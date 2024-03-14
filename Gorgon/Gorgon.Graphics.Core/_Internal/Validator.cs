#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: January 2, 2021 4:02:14 PM
// 
#endregion

using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides validation functionality.
/// </summary>
internal class Validator
{
    /// <summary>
    /// Function to validate the depth/stencil view.
    /// </summary>
    /// <param name="dsv">The depth/stencil view to evaluate.</param>
    /// <param name="firstTarget">The first non-null target.</param>
    /// <param name="currentTargets">The current list of render targets.</param>
    public static void ValidateRtvAndDsv(GorgonDepthStencil2DView dsv, GorgonRenderTargetView firstTarget, ReadOnlySpan<GorgonRenderTargetView> currentTargets)
    {
        if ((firstTarget is null)
            && (dsv is null))
        {
            return;
        }

        if (firstTarget is not null)
        {
            // Ensure our dimensions match, and multi-sample settings match.
            for (int i = 0; i < currentTargets.Length; ++i)
            {
                GorgonRenderTargetView rtv = currentTargets[i];
                if ((rtv is null) || (rtv == firstTarget))
                {
                    continue;
                }

                for (int j = 0; j < currentTargets.Length; ++j)
                {
                    if (j == i)
                    {
                        continue;
                    }

                    if (currentTargets[j] == rtv)
                    {
                        throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_ALREADY_BOUND);
                    }
                }

                if (rtv.Resource.ResourceType != firstTarget.Resource.ResourceType)
                {
                    throw new GorgonException(GorgonResult.CannotBind,
                                              string.Format(Resources.GORGFX_ERR_RTV_NOT_SAME_TYPE, firstTarget.Resource.ResourceType));
                }

                switch (firstTarget.Resource.ResourceType)
                {
                    case GraphicsResourceType.Texture2D:
                        var left2D = (GorgonRenderTarget2DView)firstTarget;
                        var right2D = (GorgonRenderTarget2DView)rtv;

                        if ((left2D.Width != right2D.Width) && (left2D.Height != right2D.Height) && (left2D.ArrayCount != right2D.ArrayCount))
                        {
                            throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_RESOURCE_MISMATCH);
                        }

                        if (!left2D.MultisampleInfo.Equals(right2D.MultisampleInfo))
                        {
                            throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_MULTISAMPLE_MISMATCH);
                        }

                        break;
                    case GraphicsResourceType.Texture3D:
                        var left3D = (GorgonRenderTarget3DView)firstTarget;
                        var right3D = (GorgonRenderTarget3DView)rtv;

                        if ((left3D.Width != right3D.Width) && (left3D.Height != right3D.Height) && (left3D.Depth != right3D.Depth))
                        {
                            throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_RESOURCE_MISMATCH);
                        }

                        break;
                    default:
                        throw new GorgonException(GorgonResult.CannotBind,
                                                  string.Format(Resources.GORGFX_ERR_RTV_UNSUPPORTED_RESOURCE, firstTarget.Resource.ResourceType));
                }
            }
        }

        if ((firstTarget is null)
            || (dsv is null))
        {
            return;
        }

        // Ensure all resources are the same type.
        if (dsv.Texture.ResourceType != firstTarget.Resource.ResourceType)
        {
            throw new GorgonException(GorgonResult.CannotBind,
                                      string.Format(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_TYPE_MISMATCH, dsv.Texture.ResourceType));
        }

        var rtv2D = (GorgonRenderTarget2DView)firstTarget;

        // Ensure the depth stencil array/depth counts match for all resources.
        if (dsv.ArrayCount != rtv2D.ArrayCount)
        {
            throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_ARRAYCOUNT_MISMATCH, dsv.Texture.Name));
        }

        // Check to ensure that multisample info matches.
        if (!dsv.Texture.MultisampleInfo.Equals(rtv2D.MultisampleInfo))
        {
            throw new GorgonException(GorgonResult.CannotBind,
                                      string.Format(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_MULTISAMPLE_MISMATCH,
                                                    dsv.MultisampleInfo.Quality,
                                                    dsv.MultisampleInfo.Count));
        }

        if ((dsv.Width != rtv2D.Width)
            || (dsv.Height != rtv2D.Height))
        {
            throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_RESOURCE_MISMATCH);
        }
    }
}
