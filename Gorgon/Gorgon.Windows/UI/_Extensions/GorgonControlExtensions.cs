
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Wednesday, July 1, 2015 7:44:04 PM
// 



namespace System.Windows.Forms;

/// <summary>
/// Extension methods for the <see cref="Control"/> base class
/// </summary>
public static class GorgonControlExtensions
{
    /// <summary>
    /// Function to retrieve the first ancestor of this control that matches the type specifier.
    /// </summary>
    /// <typeparam name="T">The type of control to find. Must inherit from <see cref="Control"/>.</typeparam>
    /// <param name="control">The control to start the search from.</param>
    /// <returns>The very first ancestor of this control matching <typeparamref name="T"/>, or <b>null</b> if the control has no ancestor of the given type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="control"/> parameter is <b>null</b>.</exception>
    public static T GetFirstAncestor<T>(this Control control)
        where T : Control
    {
        if (control is null)
        {
            throw new ArgumentNullException(nameof(control));
        }

        Control parent = control.Parent;

        while (parent is not null)
        {
            if ((parent.Parent is null) && (parent is T))
            {
                break;
            }

            parent = parent.Parent;
        }

        return (T)parent;
    }

    /// <summary>
    /// Function to retrieve the ancestor for this control that matches the type specifier.
    /// </summary>
    /// <typeparam name="T">The type of control to find. Must inherit from <see cref="Control"/>.</typeparam>
    /// <param name="control">The control to start the search from.</param>
    /// <returns>The very first ancestor of this control matching <typeparamref name="T"/>, or <b>null</b> if the control has no ancestor of the given type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="control"/> parameter is <b>null</b>.</exception>
    public static T GetAncestor<T>(this Control control)
        where T : Control
    {
        if (control is null)
        {
            throw new ArgumentNullException(nameof(control));
        }

        Control parent = control.Parent;

        while (parent is not null)
        {
            if (parent.Parent is null)
            {
                return null;
            }

            if (parent is T)
            {
                break;
            }

            parent = parent.Parent;
        }

        return (T)parent;
    }

    /// <summary>
    /// Function to retrieve the form in which this control is contained.
    /// </summary>
    /// <typeparam name="T">Type of form. Must inherit from <see cref="Form"/>.</typeparam>
    /// <param name="control">The control to start searching from.</param>
    /// <returns>The <see cref="Form"/> of type <typeparamref name="T"/> if found, <b>null</b> if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="control"/> parameter is <b>null</b>.</exception>
    public static T GetForm<T>(this Control control)
        where T : Form => control is null ? throw new ArgumentNullException(nameof(control)) : control.FindForm() as T;
}
