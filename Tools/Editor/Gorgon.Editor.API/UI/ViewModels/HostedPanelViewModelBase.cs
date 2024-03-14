using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI.Views;

namespace Gorgon.Editor.UI;

/// <summary>
/// A base view model for hosted panels within a <see cref="ContentBaseControl"/>
/// </summary>
/// <typeparam name="T">The type of view model parameters. Must inherit from <see cref="HostedPanelViewModelParameters"/>.</typeparam>
/// <remarks>
/// <para>
/// This view model is used when an operation requires more information from the user. Content plug ins that have functions that have user defined parameters can use this to present those parameters 
/// within the same view as the content as pop up panel. 
/// </para>
/// <para>
/// To use a hosted panel the plug in must define the panel by inheriting from this view model, and passing in parameters that inherit from <see cref="HostedPanelViewModelParameters"/>. A view must be 
/// registered with the <see cref="ContentBaseControl"/>.<see cref="ContentBaseControl.RegisterChildPanel(string, System.Windows.Forms.Control)"/> method
/// </para>
/// </remarks>
public abstract class HostedPanelViewModelBase<T>
    : ViewModelBase<T, IHostContentServices>, IHostedPanelViewModel
    where T : HostedPanelViewModelParameters
{

    // Flag to indicate that the editor is active.
    private bool _isActive;
    // The command to execute when the OK button is clicked.
    private IEditorCommand<object> _okCommand;



    /// <summary>Property to set or return whether the panel is active or not.</summary>
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value)
            {
                return;
            }

            OnPropertyChanging();
            _isActive = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return whether the panel is modal.</summary>
    public abstract bool IsModal
    {
        get;
    }

    /// <summary>Property to return the command used to cancel the operation.</summary>
    public IEditorCommand<object> CancelCommand
    {
        get;
    }

    /// <summary>
    /// Property to set or return the command used to apply the operation.
    /// </summary>
    public IEditorCommand<object> OkCommand
    {
        get => _okCommand;
        set
        {
            if (_okCommand == value)
            {
                return;
            }

            OnPropertyChanging();
            _okCommand = value;
            OnPropertyChanged();
        }
    }



    /// <summary>
    /// Function to cancel the dimension change operation.
    /// </summary>
    private void DoCancel()
    {
        try
        {
            if (!OnCancel())
            {
                IsActive = false;
            }
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GOREDIT_ERR_CANCEL);
        }
    }

    /// <summary>
    /// Function to allow custom functionality when canceling the operation.
    /// </summary>
    /// <returns><b>true</b> if the cancel operation was handled, or <b>false</b> if not.</returns>
    protected virtual bool OnCancel() => false;



    /// <summary>Initializes a new instance of the <see cref="HostedPanelViewModelBase{T}"/> class.</summary>
    protected HostedPanelViewModelBase() => CancelCommand = new EditorCommand<object>(DoCancel);

}
