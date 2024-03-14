using Gorgon.Diagnostics;
using Gorgon.Editor.FontEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Fonts;
using Gorgon.Math;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// The view model for the font gradient brush interface
/// </summary>
internal class FontGradientBrush
    : HostedPanelViewModelBase<HostedPanelViewModelParameters>, IFontGradientBrush, IFontBrush
{

    // The brush to edit.
    private GorgonGlyphLinearGradientBrush _brush = DefaultBrush;
    // The list of nodes for weighting the gradient.
    private readonly List<WeightHandle> _weightNodes = [];
    private readonly IComparer<WeightHandle> _weightNodeSorter = new WeightHandleComparer();
    // The currently selected node.
    private WeightHandle _selected;
    // Angle of rotation for the gradient.
    private float _angle;
    // Flag to use gamma adjustment.
    private bool _useGamma;
    // Flag to scale the gradient.
    private bool _scaling;

    /// <summary>
    /// The default brush.
    /// </summary>
    public static readonly GorgonGlyphLinearGradientBrush DefaultBrush = new()
    {
        Angle = 0,
        GammaCorrection = false,
        ScaleAngle = false
    };



    /// <summary>Property to return whether the panel is modal.</summary>
    public override bool IsModal => true;

    /// <summary>
    /// Property to set or return the angle of rotation (in degrees) for the gradient.
    /// </summary>
    public float Angle
    {
        get => _angle;
        set
        {
            if (_angle.EqualsEpsilon(value))
            {
                return;
            }

            OnPropertyChanging();
            _angle = value.Max(0).Min(360);
            UpdateBrush();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return whether to use gamma correction on the gradient.
    /// </summary>
    public bool UseGamma
    {
        get => _useGamma;
        set
        {
            if (_useGamma == value)
            {
                return;
            }

            OnPropertyChanging();
            _useGamma = value;
            UpdateBrush();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return whether to apply scaling based on the painted region for the <see cref="Angle"/>.
    /// </summary>
    public bool ScaleAngle
    {
        get => _scaling;
        set
        {
            if (_scaling == value)
            {
                return;
            }

            OnPropertyChanging();
            _scaling = value;
            UpdateBrush();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the currently selected node.
    /// </summary>
    public WeightHandle SelectedNode
    {
        get => _selected;
        private set
        {
            if (_selected == value)
            {
                return;
            }

            OnPropertyChanging();
            _selected = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to set or return the brush to edit.</summary>
    public GorgonGlyphLinearGradientBrush Brush
    {
        get => _brush;
        set
        {
            value ??= DefaultBrush;

            if ((_brush == value) || (_brush.Equals(value)))
            {
                return;
            }

            OnPropertyChanging();
            _brush = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the list of weighted nodes for the gradient.
    /// </summary>
    public IReadOnlyList<WeightHandle> Nodes => _weightNodes;

    /// <summary>Gets the brush.</summary>
    GorgonGlyphBrush IFontBrush.Brush => Brush;

    /// <summary>Property to return the command used to add a node to the gradient node list.</summary>
    public IEditorCommand<(float Weight, GorgonColor Color)> AddNodeCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to delete a node on the gradient.
    /// </summary>
    public IEditorCommand<object> DeleteNodeCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to clear all nodes (except for the first and last) on the gradient.
    /// </summary>
    public IEditorCommand<object> ClearNodesCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to select a node.
    /// </summary>
    public IEditorCommand<WeightHandle> SelectNodeCommand
    {
        get;
    }

    /// <summary>Property to set the weight of the selected node.</summary>
    public IEditorCommand<float> SetWeightCommand
    {
        get;
    }

    /// <summary>
    /// Property to set the color of the selected node.
    /// </summary>
    public IEditorCommand<GorgonColor> SetColorCommand
    {
        get;
    }

    /// <summary>Property to return the command used to duplicate the selected node.</summary>
    public IEditorCommand<object> DuplicateNodeCommand
    {
        get;
    }



    /// <summary>
    /// Function to sort the node list.
    /// </summary>
    private void SortNodes()
    {
        _weightNodes.Sort(_weightNodeSorter);

        for (int i = 0; i < _weightNodes.Count; ++i)
        {
            _weightNodes[i].Index = i;
        }
    }

    /// <summary>
    /// Function to extract all data related the gradient brush.
    /// </summary>
    /// <param name="brush">The brush to extract data from.</param>
    private void ExtractBrushData(GorgonGlyphLinearGradientBrush brush)
    {
        NotifyPropertyChanging(nameof(Nodes));

        _weightNodes.Clear();

        int index = 0;
        foreach (GorgonGlyphBrushInterpolator interpolator in brush.Interpolation.OrderBy(item => item.Weight))
        {
            _weightNodes.Add(new WeightHandle(interpolator.Weight, interpolator.Color, index++));
        }

        NotifyPropertyChanged(nameof(Nodes));

        Angle = brush.Angle;
    }

    /// <summary>
    /// Function to handle rebuilding the brush.
    /// </summary>
    private void UpdateBrush()
    {
        try
        {
            GorgonGlyphLinearGradientBrush brush = new()
            {
                Angle = _angle,
                GammaCorrection = _useGamma,
                ScaleAngle = _scaling
            };

            brush.Interpolation.Clear();
            for (int i = 0; i < Nodes.Count; ++i)
            {
                brush.Interpolation.Add(new GorgonGlyphBrushInterpolator(Nodes[i].Weight, Nodes[i].Color));
            }

            Brush = brush;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_BUILD_BRUSH);
        }
    }

    /// <summary>
    /// Function to determine if the node can be added at a specified location.
    /// </summary>
    /// <param name="nodeValues">The node values.</param>
    /// <returns><b>true</b> if node can be added, <b>false</b> if not.</returns>
    private bool CanAddNode((float Weight, GorgonColor) nodeValues) => Nodes.All(item => !item.Weight.EqualsEpsilon(nodeValues.Weight));

    /// <summary>
    /// Function to add a node to the gradient node list.
    /// </summary>
    /// <param name="nodeValues">Values for the node.</param>
    private void DoAddNode((float Weight, GorgonColor Color) nodeValues)
    {
        HostServices.BusyService.SetBusy();

        try
        {
            WeightHandle node = new(nodeValues.Weight, nodeValues.Color, -1);
            NotifyPropertyChanging(nameof(Nodes));
            _weightNodes.Add(node);
            SortNodes();
            NotifyPropertyChanged(nameof(Nodes));

            UpdateBrush();

            SelectedNode = node;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_ADD_NODE);
        }
        finally
        {
            HostServices.BusyService.SetIdle();
        }
    }

    /// <summary>
    /// Function to determine if a node can be deleted.
    /// </summary>
    /// <returns><b>true</b> if the node can be deleted, <b>false</b> if not.</returns>
    private bool CanDeleteNode() => (SelectedNode is not null) && (SelectedNode.Index > 0) && (SelectedNode.Index < Nodes.Count - 1);

    /// <summary>
    /// Function to delete a node.
    /// </summary>
    private void DoDeleteNode()
    {
        try
        {
            WeightHandle node = SelectedNode;

            if (HostServices.MessageDisplay.ShowConfirmation(Resources.GORFNT_CONFIRM_BRUSH_GRAD_DELETE_NODE) == Services.MessageResponse.No)
            {
                return;
            }

            HostServices.BusyService.SetBusy();

            int nodeIndex = _weightNodes.IndexOf(node);

            if ((nodeIndex == -1) || (_weightNodes.Count < 2))
            {
                SelectedNode = null;
            }
            else
            {
                --nodeIndex;
                SelectedNode = nodeIndex >= 0 ? _weightNodes[nodeIndex] : null;
            }

            _weightNodes.Remove(node);
            SortNodes();

            UpdateBrush();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_REMOVE_NODE);
        }
        finally
        {
            HostServices.BusyService.SetIdle();
        }
    }

    /// <summary>
    /// Function to determine if the nodes can be cleared.
    /// </summary>
    /// <returns><b>true</b> if the nodes can be cleared, <b>false</b> if not.</returns>
    private bool CanClearNodes() => Nodes.Count > 2;

    /// <summary>
    /// Function to clear all nodes.
    /// </summary>
    private void DoClearNodes()
    {
        try
        {
            WeightHandle node = SelectedNode;

            if (HostServices.MessageDisplay.ShowConfirmation(Resources.GORFNT_CONFIRM_BRUSH_NODES_CLEAR) == Services.MessageResponse.No)
            {
                return;
            }

            HostServices.BusyService.SetBusy();

            SelectedNode = null;
            _weightNodes.RemoveRange(1, _weightNodes.Count - 2);
            SortNodes();

            UpdateBrush();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_REMOVE_NODE);
        }
        finally
        {
            HostServices.BusyService.SetIdle();
        }
    }

    /// <summary>
    /// Function to determine if the selected node can be duplicated or not.
    /// </summary>
    /// <returns><b>true</b> if the node can be duplicated, <b>false</b> if not.</returns>
    private bool CanDuplicateNode() => SelectedNode is not null;

    private void DoDuplicateNode()
    {
        HostServices.BusyService.SetBusy();

        try
        {
            NotifyPropertyChanging(nameof(Nodes));
            WeightHandle node = new(SelectedNode.Weight, SelectedNode.Color, -1);
            _weightNodes.Add(node);

            SortNodes();

            NotifyPropertyChanged(nameof(Nodes));

            SelectedNode = node;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_DUPE_NODE);
        }
        finally
        {
            HostServices.BusyService.SetIdle();
        }
    }

    /// <summary>
    /// Function to select a node.
    /// </summary>
    /// <param name="node">The node to select.</param>
    private void DoSelectNode(WeightHandle node)
    {
        try
        {
            SelectedNode = node;
        }
        catch (Exception ex)
        {
            HostServices.Log.Print("Error selected the gradient node.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if the weight can be assigned to the selected node.
    /// </summary>
    /// <param name="value">The weight to assign.</param>
    /// <returns><b>true</b> if the weight can be assigned, <b>false</b> if not.</returns>
    private bool CanSetWeight(float value) => SelectedNode is not null;

    /// <summary>
    /// Function to set the weight for the selected node.
    /// </summary>
    /// <param name="value">The value to assign.</param>
    private void DoSetWeight(float value)
    {
        try
        {
            SelectedNode.Weight = value;
            SortNodes();

            UpdateBrush();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_SET_NODE_WEIGHT);
        }
    }

    /// <summary>
    /// Function to determine if the color can be assigned to the selected node.
    /// </summary>
    /// <param name="value">The color to assign.</param>
    /// <returns><b>true</b> if the color can be assigned, <b>false</b> if not.</returns>
    private bool CanSetColor(GorgonColor value) => SelectedNode is not null;

    /// <summary>
    /// Function to set the color for the selected node.
    /// </summary>
    /// <param name="value">The value to assign.</param>
    private void DoSetColor(GorgonColor value)
    {
        try
        {
            SelectedNode.Color = value;
            UpdateBrush();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_SET_NODE_COLOR);
        }
    }

    /// <summary>
    /// Function called when the associated view is loaded.
    /// </summary>
    /// <seealso cref="Initialize(T)" />
    /// <seealso cref="Unload" />
    /// <remarks><para>
    /// This method should be overridden when there is a need to set up functionality (e.g. events) when the UI first loads with the view model attached. Unlike the <see cref="Initialize(T)" /> method, this
    /// method may be called multiple times during the lifetime of the application.
    /// </para>
    /// <para>
    /// Anything that requires tear down should have their tear down functionality in the accompanying <see cref="Unload" /> method.
    /// </para></remarks>
    protected override void OnLoad()
    {
        base.OnLoad();

        ExtractBrushData(_brush ?? DefaultBrush);
    }

    /// <summary>Function called when the associated view is unloaded.</summary>
    /// <remarks>This method is used to perform tear down and clean up of resources.</remarks>
    protected override void OnUnload()
    {
        _brush = DefaultBrush;

        base.OnUnload();
    }

    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    protected override void OnInitialize(HostedPanelViewModelParameters injectionParameters) => ExtractBrushData(DefaultBrush);



    /// <summary>Initializes a new instance of the <see cref="FontGradientBrush" /> class.</summary>
    public FontGradientBrush()
    {
        AddNodeCommand = new EditorCommand<(float, GorgonColor)>(DoAddNode, CanAddNode);
        DeleteNodeCommand = new EditorCommand<object>(DoDeleteNode, CanDeleteNode);
        ClearNodesCommand = new EditorCommand<object>(DoClearNodes, CanClearNodes);
        DuplicateNodeCommand = new EditorCommand<object>(DoDuplicateNode, CanDuplicateNode);
        SelectNodeCommand = new EditorCommand<WeightHandle>(DoSelectNode);
        SetWeightCommand = new EditorCommand<float>(DoSetWeight, CanSetWeight);
        SetColorCommand = new EditorCommand<GorgonColor>(DoSetColor, CanSetColor);
    }

}
