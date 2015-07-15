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
// Created: Saturday, July 4, 2015 2:20:46 PM
// 
#endregion

namespace Gorgon.Input
{
	/// <summary>
	/// Contains information for a pointing input (e.g. a mouse, track pad, etc...) device.
	/// </summary>
	public interface IGorgonMouseInfo
		: IGorgonInputDeviceInfo
	{
		/// <summary>
		/// Property to return the number of buttons on the pointing device.
		/// </summary> 
		int ButtonCount
		{
			get;
		}

		/// <summary>
		/// Property to return the number of data points per second.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Some input plug-ins may not be able to query this information. When this is the case, this value should return 0.
		/// </para>
		/// <para>
		/// This information may not be applicable for every pointing device.
		/// </para>
		/// </remarks>
		int SamplingRate
		{
			get;
		}

		/// <summary>
		/// Property to return whether the pointing device has a horizontal scrolling wheel.
		/// </summary>
		/// <remarks>
		/// Some input plug-ins may not be able to query this information. When this is the case, this value should return <b>false</b>.
		/// </remarks>
		bool HasHorizontalWheel
		{
			get;
		}
	}
}
