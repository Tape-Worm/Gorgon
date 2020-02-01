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
// Created: March 21, 2019 11:21:46 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Editor.Properties;
using Gorgon.Math;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// Extension methods for the zoom levels.
    /// </summary>
    public static class ZoomLevelExtensions
    {
        // The list of zoom levels.
        private static readonly ZoomLevels[] _levels = (ZoomLevels[])Enum.GetValues(typeof(ZoomLevels));

        // The look up for zoom level names.
        private static readonly Dictionary<ZoomLevels, string> _names = new Dictionary<ZoomLevels, string>()
        {
            {
                ZoomLevels.Percent12,
                    Resources.GOREDIT_ZOOM_12
            },
            {
                ZoomLevels.Percent25,
                    Resources.GOREDIT_ZOOM_25
            },
              {  ZoomLevels.Percent50,
                    Resources.GOREDIT_ZOOM_50
            },
              {  ZoomLevels.Percent100,
                    Resources.GOREDIT_ZOOM_100
            },
              {  ZoomLevels.Percent200,
                    Resources.GOREDIT_ZOOM_200
            },
              {  ZoomLevels.Percent400,
                    Resources.GOREDIT_ZOOM_400
            },
              {  ZoomLevels.Percent800,
                    Resources.GOREDIT_ZOOM_800
            },
              {  ZoomLevels.Percent1600,
                    Resources.GOREDIT_ZOOM_1600
            },
              {  ZoomLevels.Percent3200,
                    Resources.GOREDIT_ZOOM_3200
            },
              {  ZoomLevels.Percent6400,
                    Resources.GOREDIT_ZOOM_6400
            },
            {  ZoomLevels.ToWindow,
                    Resources.GOREDIT_ZOOM_TO_WINDOW
            },
            {
                ZoomLevels.Custom,
                    Resources.GOREDIT_ZOOM_CUSTOM
            }
        };

        /// <summary>
        /// Function to convert a <see cref="ZoomLevels"/> to its associated friendly name.
        /// </summary>
        /// <param name="zoomLevel">The zoom level to retrieve the name for.</param>
        /// <returns>The friendly name of the zoom level.</returns>
        public static string GetName(this ZoomLevels zoomLevel) => !_names.TryGetValue(zoomLevel, out string name) ? string.Empty : name;

        /// <summary>
        /// Function to convert a friendly name to an associated zoom level.
        /// </summary>
        /// <param name="name">The name to look up.</param>
        /// <returns>The associated zoom level.</returns>
        public static ZoomLevels FromName(this string name)
        {
            foreach (KeyValuePair<ZoomLevels, string> item in _names)
            {
                if (string.Equals(name, item.Value, StringComparison.CurrentCultureIgnoreCase))
                {
                    return item.Key;
                }
            }

            return ZoomLevels.Percent100;
        }

        /// <summary>
        /// Function to convert a <see cref="ZoomLevels"/> to its associated scaling factor.
        /// </summary>
        /// <param name="zoomLevel">The zoom level to retrieve the scaling factor from.</param>
        /// <returns>The scaling factor.</returns>
        /// <remarks>
        /// <para>
        /// When the zoom level is set to <see cref="ZoomLevels.ToWindow"/>, then this method will return -1.
        /// </para>
        /// </remarks>
        public static float GetScale(this ZoomLevels zoomLevel)
        {
            switch (zoomLevel)
            {
                case ZoomLevels.Percent12:
                    return 0.12f;
                case ZoomLevels.Percent25:
                    return 0.25f;
                case ZoomLevels.Percent50:
                    return 0.5f;
                case ZoomLevels.Percent100:
                    return 1;
                case ZoomLevels.Percent200:
                    return 2;
                case ZoomLevels.Percent400:
                    return 4;
                case ZoomLevels.Percent800:
                    return 8;
                case ZoomLevels.Percent1600:
                    return 16;
                case ZoomLevels.Percent3200:
                    return 32;
                case ZoomLevels.Percent6400:
                    return 64;
                case ZoomLevels.Custom:
                case ZoomLevels.ToWindow:
                    return -1;
            }

            return 1;
        }

        /// <summary>
        /// Function to retrieve the best <see cref="ZoomLevels"/> for the value passed. 
        /// </summary>
        /// <param name="zoomScale">The scale value to evaluate.</param>
        /// <returns>The zoom level that best matches the scale.</returns>
        public static ZoomLevels GetZoomLevel(this float zoomScale)
        {
            ZoomLevels? result = null;

            for (int i = 0; i < _levels.Length; ++i)
            {
                float scale = GetScale(_levels[i]);

                if (scale.EqualsEpsilon(-1))
                {
                    continue;
                }

                if (scale.EqualsEpsilon(zoomScale))
                {
                    result =  _levels[i];
                    break;
                }
            }

            if (result != null)
            {
                return result.Value;
            }

            ZoomLevels nextZoom = GetNextNearest(zoomScale);
            ZoomLevels prevZoom = GetPrevNearest(zoomScale);
            float nextScale = (GetScale(nextZoom) - zoomScale).Abs();
            float prevScale = (zoomScale - GetScale(prevZoom)).Abs();

            return (nextScale.EqualsEpsilon(prevScale)) || (nextScale < prevScale) ? nextZoom : prevZoom;
        }

        /// <summary>
        /// Function to retrieve the nearest previous zoom scale based on the scaling value passed in.
        /// </summary>
        /// <param name="zoomScale">The zoom scaling factor to evalute.</param>
        /// <returns>The nearest previous zoom level.</returns>
        public static ZoomLevels GetNextNearest(this float zoomScale)
        {
            for (int i = 0; i < _levels.Length; ++i)
            {
                float actualScale = GetScale(_levels[i]);

                if (actualScale.EqualsEpsilon(-1))
                {
                    continue;
                }

                if (zoomScale >= actualScale)
                {
                    continue;
                }

                if (zoomScale < actualScale)
                {
                    return _levels[i];
                }
            }

            return ZoomLevels.Percent6400;
        }

        /// <summary>
        /// Function to retrieve the nearest previous zoom scale based on the scaling value passed in.
        /// </summary>
        /// <param name="zoomScale">The zoom scaling factor to evalute.</param>
        /// <returns>The nearest previous zoom level.</returns>
        public static ZoomLevels GetPrevNearest(this float zoomScale)
        {
            for (int i = _levels.Length - 1; i >= 0; --i)
            {
                float actualScale = GetScale(_levels[i]);

                if (actualScale.EqualsEpsilon(-1))
                {
                    continue;
                }

                if (zoomScale <= actualScale)
                {
                    continue;
                }

                if (zoomScale > actualScale)
                {
                    return _levels[i];
                }
            }

            return ZoomLevels.Percent12;
        }

    }
}

