﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: April 18, 2019 9:04:47 AM
// 

using Gorgon.Core;

namespace Gorgon.Editor.PlugIns;

/// <summary>
/// Defines a button to display on the ribbon bar, in the tools area
/// </summary>
public class ToolPlugInRibbonButton
    : IGorgonNamedObject, IDisposable, IToolPlugInRibbonButton
{
    // Flag to indicate that we own the large image and are repsonsible for its lifetime.
    private bool _ownsLargeImage;
    // Flag to indicate that we own the small image and are repsonsible for its lifetime.
    private bool _ownsSmallImage;

    /// <inheritdoc/>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Property to set or return the action to perform when the button is clicked.
    /// </summary>
    public Action ClickCallback
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the function to determine if the button can be clicked.
    /// </summary>
    public Func<bool> CanExecute
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the display text for the button.
    /// </summary>
    public string DisplayText
    {
        get;
    }

    /// <summary>
    /// Property to return the 48x48 large icon.
    /// </summary>
    public Image LargeIcon
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the 16x16 small icon.
    /// </summary>
    public Image SmallIcon
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the group that owns this button.
    /// </summary>
    public string GroupName
    {
        get;
    }

    /// <summary>
    /// Property to set or return whether to use the small icon, or large icon on the ribbon.
    /// </summary>
    public bool IsSmall
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether this button should start a separator.
    /// </summary>
    public bool IsSeparator
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the description for the button.
    /// </summary>
    public string Description
    {
        get;
        set;
    }

    /// <summary>
    /// Function to validate the button to ensure it'll be displayed correctly on the ribbon.
    /// </summary>
    public void ValidateButton()
    {
        if ((LargeIcon.Width != 48) || (LargeIcon.Height != 48))
        {
            LargeIcon = new Bitmap(LargeIcon, new Size(48, 48));
            _ownsLargeImage = true;
        }

        if ((SmallIcon.Width != 16) || (SmallIcon.Height != 16))
        {
            SmallIcon = new Bitmap(SmallIcon, new Size(16, 16));
            _ownsSmallImage = true;
        }
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        CanExecute = null;
        ClickCallback = null;

        if (_ownsLargeImage)
        {
            LargeIcon.Dispose();
        }

        if (_ownsSmallImage)
        {
            SmallIcon.Dispose();
        }
    }

    /// <summary>Initializes a new instance of the <see cref="ToolPlugInRibbonButton"/> class.</summary>
    /// <param name="displayText">The display text.</param>
    /// <param name="largeIcon">The large icon.</param>
    /// <param name="smallIcon">The small icon.</param>
    /// <param name="groupName">Name of the group.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="displayText"/>, or the <paramref name="groupName"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="displayText"/>, or the <paramref name="groupName"/> parameter is empty.</exception>
    public ToolPlugInRibbonButton(string displayText, Image largeIcon, Image smallIcon, string groupName)
    {
        if (displayText is null)
        {
            throw new ArgumentNullException(nameof(displayText));
        }

        if (string.IsNullOrWhiteSpace(displayText))
        {
            throw new ArgumentEmptyException(nameof(displayText));
        }

        if (groupName is null)
        {
            throw new ArgumentNullException(nameof(groupName));
        }

        if (string.IsNullOrWhiteSpace(groupName))
        {
            throw new ArgumentEmptyException(nameof(groupName));
        }

        Name = Guid.NewGuid().ToString("N");
        DisplayText = displayText;
        LargeIcon = largeIcon ?? throw new ArgumentNullException(nameof(largeIcon));
        SmallIcon = smallIcon ?? throw new ArgumentNullException(nameof(smallIcon));
        GroupName = groupName;
    }
}
