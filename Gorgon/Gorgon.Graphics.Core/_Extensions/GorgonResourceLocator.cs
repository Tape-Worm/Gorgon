
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
// Created: August 11, 2018 1:54:00 PM
// 


namespace Gorgon.Graphics.Core;

/// <summary>
/// The method to use when locating the resources by name
/// </summary>
public enum LocateFilterType
{
    /// <summary>
    /// The names should be equal.
    /// </summary>
    Equal,
    /// <summary>
    /// The name should start with the value specified.
    /// </summary>
    StartsWith,
    /// <summary>
    /// The name should end with the value specified.
    /// </summary>
    EndsWith,
    /// <summary>
    /// The name should contain the value specified.
    /// </summary>
    Contains,
    /// <summary>
    /// Return all with names that are not equal to the value specified.
    /// </summary>
    NotEqual,
    /// <summary>
    /// Return all with names that do not contain the value specified.
    /// </summary>
    NotContains,
    /// <summary>
    /// Return all with names that do not start with the value specified.
    /// </summary>
    NotStartsWith,
    /// <summary>
    /// Return all with names that do not end with the value specified.
    /// </summary>
    NotEndsWith
}

/// <summary>
/// A locator extension for <see cref="GorgonGraphicsResource"/> objects
/// </summary>
public static class GorgonResourceLocator
{

    /// <summary>
    /// Function to perform a comparison of an item name to the name specified, using the filter type specified.
    /// </summary>
    /// <param name="name">The name to look up.</param>
    /// <param name="itemName">The name of the item to evaluate.</param>
    /// <param name="filterType">The type of filter to apply.</param>
    /// <param name="comparisonType">The type of string comparer to use.</param>
    /// <returns><b>true</b> if the resource name matches the filter type, or <b>false</b> if not.</returns>
    private static bool NameComparison(string name, string itemName, LocateFilterType filterType, StringComparison comparisonType) => !string.IsNullOrWhiteSpace(itemName)
&& filterType switch
{
    LocateFilterType.StartsWith => itemName.StartsWith(name, comparisonType),
    LocateFilterType.EndsWith => itemName.EndsWith(name, comparisonType),
    LocateFilterType.Contains => itemName.IndexOf(name, comparisonType) != -1,
    LocateFilterType.NotContains => itemName.IndexOf(name, comparisonType) == -1,
    LocateFilterType.NotStartsWith => !itemName.StartsWith(name, comparisonType),
    LocateFilterType.NotEndsWith => !itemName.EndsWith(name, comparisonType),
    LocateFilterType.NotEqual => !string.Equals(name, itemName, comparisonType),
    _ => string.Equals(name, itemName, comparisonType),
};

    /// <summary>
    /// Function to locate a graphics resource by its name.
    /// </summary>
    /// <typeparam name="T">The type of graphics resource to look up. Must inherit from <see cref="GorgonGraphicsResource"/>.</typeparam>
    /// <param name="graphics">The graphics instance that was used to create the resource.</param>
    /// <param name="name">The name of the resource to find.</param>
    /// <param name="filterType">[Optional] The type of filter to apply.</param>
    /// <param name="comparisonType">[Optional] The type of string comparison to use for name comparison.</param>
    /// <returns>An enumerable containing the resources with names that match the filter type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// Each instance of the <seealso cref="GorgonGraphics"/> keeps a weak registration of each objected inheriting from <see cref="GorgonGraphicsResource"/> that was created during the lifetime of the
    /// application. Using this registration an application can look up any previously created resource (assuming it's not been collected, or disposed) should it be necessary.
    /// </para>
    /// <para>
    /// <note type="important">
    /// Resource names are not required to be unique. Therefore, searching for the name may result in multiple items being returned in the enumerable.
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsResource"/>
    public static IEnumerable<T> LocateResourcesByName<T>(this GorgonGraphics graphics,
                                                          string name,
                                                          LocateFilterType filterType = LocateFilterType.Equal,
                                                          StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase)
        where T : GorgonGraphicsResource
    {
        if (graphics is null)
        {
            throw new ArgumentNullException(nameof(graphics));
        }

#pragma warning disable IDE0046 // Convert to conditional expression
        if (string.IsNullOrWhiteSpace(name))
        {
            return [];
        }

        return graphics.GetDisposables()
                       .Select(item =>
                               {
                                   // If the object has been collected/disposed, then do nothing.
                                   if ((!item.TryGetTarget(out IDisposable disposable))
                                       || (disposable is not T resource)
                                       || (resource.IsDisposed))
                                   {
                                       return null;
                                   }

                                   return !NameComparison(name, resource.Name, filterType, comparisonType) ? null : resource;
                               })
                       .Where(item => item is not null);
#pragma warning restore IDE0046 // Convert to conditional expression
    }

    /// <summary>
    /// Function to locate a graphics resource by its type.
    /// </summary>
    /// <typeparam name="T">The type of graphics resource to look up. Must inherit from <see cref="GorgonGraphicsResource"/>.</typeparam>
    /// <param name="graphics">The graphics instance that was used to create the resource.</param>
    /// <returns>An enumerable containing the resources with names that match the filter type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// Each instance of the <seealso cref="GorgonGraphics"/> keeps a weak registration of each objected inheriting from <see cref="GorgonGraphicsResource"/> that was created during the lifetime of the
    /// application. Using this registration an application can look up any previously created resource (assuming it's not been collected, or disposed) should it be necessary.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsResource"/>
    public static IEnumerable<T> LocateResourcesByType<T>(this GorgonGraphics graphics)
        where T : GorgonGraphicsResource => graphics is null
            ? throw new ArgumentNullException(nameof(graphics))
            : graphics.GetDisposables()
                       .Select(item =>
                                   // If the object has been collected/disposed, then do nothing.
                                   ((!item.TryGetTarget(out IDisposable disposable))
                                       || (disposable is not T resource)
                                       || (resource.IsDisposed))
                                       ? null
                                       : resource)
                       .Where(item => item is not null);

}
