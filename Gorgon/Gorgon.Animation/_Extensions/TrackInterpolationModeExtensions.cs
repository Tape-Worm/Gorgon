using System;
using System.Collections.Generic;
using Gorgon.Animation.Properties;

namespace Gorgon.Animation
{
    /// <summary>
    /// Extension methods for the <see cref="TrackInterpolationMode"/> values.
    /// </summary>
    public static class TrackInterpolationModeExtensions
    {
        // The declared values.
        private static readonly TrackInterpolationMode[] _values = (TrackInterpolationMode[])Enum.GetValues(typeof(TrackInterpolationMode));

        /// <summary>
        /// Function to get a culture sensitive string representing a <see cref="TrackInterpolationMode"/> value.
        /// </summary>
        /// <param name="mode">The mode to evaluate.</param>
        /// <returns>The string representation.</returns>
        public static string GetDescription(this TrackInterpolationMode mode)
        {
            switch(mode)
            {
                case TrackInterpolationMode.Linear:
                    return Resources.GORANM_DESC_INTERP_LINEAR;
                case TrackInterpolationMode.Spline:
                    return Resources.GORANM_DESC_INTERP_SPLINE;
                default:
                    return Resources.GORANM_DESC_INTERP_NONE;
            }
        }

        /// <summary>
        /// Function to retrieve a list of explicit values in the <see cref="TrackInterpolationMode"/> passed in.
        /// </summary>
        /// <returns>The list of explicitly declared values.</returns>
        public static IEnumerable<TrackInterpolationMode> GetExplicitValues(this TrackInterpolationMode mode)
        {
            for (int i = 0; i < _values.Length; ++i)
            {
                TrackInterpolationMode modeItem = _values[i];

                if ((mode & modeItem) == modeItem)
                {
                    yield return modeItem;
                }
            }
        }

        /// <summary>
        /// Function to get a the <see cref="TrackInterpolationMode"/> from a culture sensitive string.
        /// </summary>
        /// <param name="mode">The string value to evaluate.</param>
        /// <returns>The enum value.</returns>
        public static TrackInterpolationMode GetTrackInterpolationMode(this string mode)
        {
#pragma warning disable IDE0046 // Convert to conditional expression
            if (string.IsNullOrWhiteSpace(mode))
            {
                return TrackInterpolationMode.None;
            }

            if (string.Equals(mode, Resources.GORANM_DESC_INTERP_LINEAR, StringComparison.CurrentCultureIgnoreCase))
            {
                return TrackInterpolationMode.Linear;
            }

            if (string.Equals(mode, Resources.GORANM_DESC_INTERP_SPLINE, StringComparison.CurrentCultureIgnoreCase))
            {
                return TrackInterpolationMode.Spline;
            }
#pragma warning restore IDE0046 // Convert to conditional expression

            return TrackInterpolationMode.None;
        }
    }
}
