#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: July 5, 2020 1:30:01 AM
// 
#endregion

using System.Collections.Generic;
using System.Linq;
using Gorgon.Core;
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Math;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// Metadata to pass to the <see cref="IKeyValueEditor"/>.
/// </summary>
/// <remarks>
/// <para>
/// This is used to update the number of values and the names and limits for each value.
/// </para>
/// </remarks>
internal class KeyValueMetadata
{
    #region Variables.
    // The value count.
    private int _valueCount = 4;
    // The metadata for the values.
    private readonly MetadataValues[] _valueData = new MetadataValues[4];

    /// <summary>
    /// A default copy of the metadata.
    /// </summary>
    public static readonly KeyValueMetadata Default;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to set or return the number of values to use.
    /// </summary>
    public int ValueCount
    {
        get => _valueCount;
        set
        {
            if (_valueCount == value)
            {
                return;
            }

            _valueCount = value.Max(1).Min(4);

        }
    }

    /// <summary>
    /// Property to return the value metadata.
    /// </summary>
    public IReadOnlyList<MetadataValues> ValueMetadata => _valueData;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to set the metadata for a specific value.
    /// </summary>
    /// <param name="valueIndex">The index of the value, between 0 and 3.</param>
    /// <param name="displayName">The display name for the value.</param>
    /// <param name="decimalCount">The number of decimal places allowed in the value.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    public void SetData(int valueIndex, string displayName, int decimalCount, float min, float max)
    {
        if ((valueIndex < 0) || (valueIndex >= _valueData.Length))
        {
            return;
        }

        _valueData[valueIndex] = new MetadataValues
        {
            DisplayName = (string.IsNullOrWhiteSpace(displayName) ? string.Format(Resources.GORANM_TEXT_DEFAULT_VALUE_NAME, valueIndex + 1) : displayName),   
            DecimalCount = decimalCount,
            MinMax = new GorgonRangeF(min, max)
        };
    }
    #endregion

    #region Constructor
    /// <summary>Initializes static members of the <see cref="KeyValueMetadata"/> class.</summary>
    static KeyValueMetadata()
    {
        Default = new KeyValueMetadata
        {
            ValueCount = 4
        };
        for (int i = 0; i < Default.ValueCount; ++i)
        {
            Default.SetData(i, string.Format(Resources.GORANM_TEXT_DEFAULT_VALUE_NAME, i + 1), 6, short.MinValue, short.MaxValue);
        }
    }
    #endregion
}
