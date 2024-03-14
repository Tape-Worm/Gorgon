using System;
using System.Drawing;
using Gorgon.Core;

namespace Gorgon.Editor.PlugIns;

/// <summary>
/// Defines a button to display on the ribbon bar, in the tools area.
/// </summary>
public interface IToolPlugInRibbonButton
    : IGorgonNamedObject
{
    #region Properties.
    /// <summary>
    /// Property to return the action to perform when the button is clicked.
    /// </summary>
    Action ClickCallback
    {
        get;
    }

    /// <summary>
    /// Property to return the function to determine if the button can be clicked.
    /// </summary>
    Func<bool> CanExecute
    {
        get;
    }

    /// <summary>
    /// Property to return the display text for the button.
    /// </summary>
    string DisplayText
    {
        get;
    }

    /// <summary>
    /// Property to return the group that owns this button.
    /// </summary>
    string GroupName
    {
        get;
    }


    /// <summary>
    /// Property to return whether this button should start a separator.
    /// </summary>
    bool IsSeparator
    {
        get;
    }

    /// <summary>
    /// Property to return whether to use the small icon, or large icon on the ribbon.
    /// </summary>
    bool IsSmall
    {
        get;
    }

    /// <summary>
    /// Property to return the 48x48 large icon.
    /// </summary>
    Image LargeIcon
    {
        get;
    }

    /// <summary>
    /// Property to return the 16x16 small icon.
    /// </summary>
    Image SmallIcon
    {
        get;
    }

    /// <summary>
    /// Property to set or return the description for the button.
    /// </summary>
    string Description
    {
        get;
        set;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to validate the button to ensure it'll be displayed correctly on the ribbon.
    /// </summary>
    void ValidateButton();
    #endregion
}
