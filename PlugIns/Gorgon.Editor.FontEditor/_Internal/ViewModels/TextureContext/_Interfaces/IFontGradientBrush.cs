using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Fonts;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// A view model for the font gradient brush interface.
/// </summary>
internal interface IFontGradientBrush
    : IHostedPanelViewModel
{
    /// <summary>
    /// Property to set or return the brush to edit.
    /// </summary>
    GorgonGlyphLinearGradientBrush Brush
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the angle of rotation (in degrees) for the gradient.
    /// </summary>
    float Angle
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether to use gamma correction on the gradient.
    /// </summary>
    bool UseGamma
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether to apply scaling based on the painted region for the <see cref="Angle"/>.
    /// </summary>
    bool ScaleAngle
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the currently selected node.
    /// </summary>
    WeightHandle SelectedNode
    {
        get;
    }

    /// <summary>
    /// Property to return the list of weighted nodes for the gradient.
    /// </summary>
    IReadOnlyList<WeightHandle> Nodes
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to add a node to the gradient node list.
    /// </summary>
    IEditorCommand<(float Weight, GorgonColor Color)> AddNodeCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to delete a node on the gradient.
    /// </summary>
    IEditorCommand<object> DeleteNodeCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to clear all nodes (except for the first and last) on the gradient.
    /// </summary>
    IEditorCommand<object> ClearNodesCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to duplicate the selected node.
    /// </summary>
    IEditorCommand<object> DuplicateNodeCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to select a node.
    /// </summary>
    IEditorCommand<WeightHandle> SelectNodeCommand
    {
        get;
    }

    /// <summary>
    /// Property to set the weight of the selected node.
    /// </summary>
    IEditorCommand<float> SetWeightCommand
    {
        get;
    }

    /// <summary>
    /// Property to set the color of the selected node.
    /// </summary>
    IEditorCommand<GorgonColor> SetColorCommand
    {
        get;
    }
}
