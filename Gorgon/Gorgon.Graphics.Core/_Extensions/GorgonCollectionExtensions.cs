
// 
// Gorgon
// Copyright (C) 2018 Michael Winsor
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
// Created: April 8, 2018 1:44:13 PM
// 


using SharpDX.Direct3D;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Extension methods for collections
/// </summary>
public static class GorgonCollectionExtensions
{
    // A null video mode reference.
    private static readonly GorgonVideoMode _invalidMode = GorgonVideoMode.InvalidMode;

    /// <summary>
    /// Function to find a display mode supported by the Gorgon.
    /// </summary>
    /// <param name="videoModes">The list of video modes to evaluate.</param>
    /// <param name="output">The output to use when looking for a video mode.</param>
    /// <param name="videoMode">The <see cref="GorgonVideoMode"/> used to find the closest match.</param>
    /// <param name="suggestedMode">A <see cref="GorgonVideoMode"/> that is the nearest match for the provided video mode.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="output"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// Users may leave the <see cref="GorgonVideoMode"/> values at unspecified (either 0, or default enumeration values) to indicate that these values should not be used in the search.
    /// </para>
    /// <para>
    /// The following members in <see cref="GorgonVideoMode"/> may be skipped (if not listed, then this member must be specified):
    /// <list type="bullet">
    ///		<item>
    ///			<description><see cref="GorgonVideoMode.Width"/> and <see cref="GorgonVideoMode.Height"/>.  Both values must be set to 0 if not filtering by width or height.</description>
    ///		</item>
    ///		<item>
    ///			<description><see cref="GorgonVideoMode.RefreshRate"/> should be set to empty in order to skip filtering by refresh rate.</description>
    ///		</item>
    ///		<item>
    ///			<description><see cref="GorgonVideoMode.Scaling"/> should be set to <see cref="ModeScaling.Unspecified"/> in order to skip filtering by the scaling mode.</description>
    ///		</item>
    ///		<item>
    ///			<description><see cref="GorgonVideoMode.ScanlineOrder"/> should be set to <see cref="ModeScanlineOrder.Unspecified"/> in order to skip filtering by the scanline order.</description>
    ///		</item>
    /// </list>
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The <see cref="GorgonVideoMode.Format"/> member must be one of the UNorm format types and cannot be set to <see cref="BufferFormat.Unknown"/>.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public static void FindNearestVideoMode(this IReadOnlyList<GorgonVideoMode> videoModes, IGorgonVideoOutputInfo output, ref readonly GorgonVideoMode videoMode, out GorgonVideoMode suggestedMode)
    {
        ref readonly GorgonVideoMode result = ref _invalidMode;

        if (videoModes is null)
        {
            suggestedMode = result;
            return;
        }

        if (GorgonVideoMode.Equals(in videoMode, in _invalidMode))
        {
            suggestedMode = result;
            return;
        }

        using Factory1 factory = new();
        using Adapter1 adapter = factory.GetAdapter1(output.Adapter.Index);
        using Output giOutput = adapter.GetOutput(output.Index);
        using Output1 giOutput1 = giOutput.QueryInterface<Output1>();
        using D3D11.Device device = new(adapter,
                                                        GorgonGraphics.IsDebugEnabled
                                                            ? D3D11.DeviceCreationFlags.Debug
                                                            : D3D11.DeviceCreationFlags.None,
                                                        FeatureLevel.Level_12_1,
                                                        FeatureLevel.Level_12_0);
        ModeDescription1 matchMode = videoMode.ToModeDesc1();

        giOutput1.FindClosestMatchingMode1(ref matchMode, out ModeDescription1 mode, device);

        mode.ToGorgonVideoMode(out suggestedMode);
    }
}
