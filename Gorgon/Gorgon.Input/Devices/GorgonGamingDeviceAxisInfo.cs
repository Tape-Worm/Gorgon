#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Wednesday, August 12, 2015 9:25:53 PM
// 
#endregion

using Gorgon.Core;
using Gorgon.Input.Properties;

namespace Gorgon.Input
{
    /// <summary>
    /// Information about a gaming device axis.
    /// </summary>
    /// <remarks>
    /// This will provide information about the range of movement for the axis, as well as a default value for when the axis is centered or whatever the resting position may be. This default is set by 
    /// the <see cref="IGorgonGamingDeviceDriver"/> upon enumeration of the devices.
    /// </remarks>
    public class GorgonGamingDeviceAxisInfo
    {
        #region Variables.
        /// <summary>
        /// Property to return the identifier for the axis.
        /// </summary>
        public GamingDeviceAxis Axis
        {
            get;
        }

        /// <summary>
        /// Property to return the range of the axis.
        /// </summary>
        /// <remarks>
        /// This value will vary depending on the underlying provider used for the physical device.
        /// </remarks>
        public GorgonRange Range
        {
            get;
        }

        /// <summary>
        /// Property to return the default value for the resting position of the axis.
        /// </summary>
        /// <remarks>
        /// This is the value that will be returned when the device axis is stationary. For example, if the range of the axis is between 0 and 16384, then 8192 may be the default value, or it may be zero 
        /// depending on the type of axis.
        /// </remarks>
        public int DefaultValue
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString() => string.Format(Resources.GORINP_TOSTR_JOYSTICKAXIS, Axis, Range.Minimum, Range.Maximum);
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonGamingDeviceAxisInfo"/> struct.
        /// </summary>
        /// <param name="axis">The identifier for the axis.</param>
        /// <param name="range">The range of the axis.</param>
        /// <param name="defaultValue">The default value for the axis when in resting position.</param>
        public GorgonGamingDeviceAxisInfo(GamingDeviceAxis axis, GorgonRange range, int defaultValue)
        {
            Axis = axis;
            Range = range;
            DefaultValue = defaultValue;
        }
        #endregion

    }
}
