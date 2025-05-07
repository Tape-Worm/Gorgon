using System.Windows;
using Gorgon.UI;
using Gorgon.UI.Wpf;

namespace Dialogs.Wpf;

/// <summary>
/// Main example window
/// </summary>
/// <remarks>
/// <para>
/// This example shows the custom dialogs available in Gorgon.  These are meant as replacements for the standard Windows dialogs shown when errors, warnings, confirmation 
/// or event standard information is shown to the user.
/// 
/// The warning and error dialogs allow applications to display a details panel, which can be used to relay more detailed error information for debugging purposes.
/// 
/// The confirmation dialog allows the application to display a yes/no dialog, with an optional cancel button. It can also show a checkbox that is used to indicate that 
/// the answer provided by the user should be applied to all objects.
/// 
/// Unlike the Windows Forms version of the Dialogs, the WPF version does not need a parent window.
/// </para>
/// </remarks>
public partial class MainWindow : Window
{
    // Big text to show.
    private const string Lorem = @"

Lorem ipsum odor amet, consectetuer adipiscing elit. Eget morbi tellus ornare phasellus per dui. Nisi dictum mattis blandit maximus scelerisque efficitur tellus! Cursus blandit ut feugiat vehicula diam nascetur dapibus per.Cursus sapien interdum mus fusce erat ut nulla gravida.Ultrices aliquet nisl ac tristique pretium aliquam.Porttitor tempus mattis semper blandit turpis donec. Maecenas rutrum malesuada proin facilisi ullamcorper. Gravida blandit mollis urna donec venenatis potenti neque. Odio habitant ad consequat elit sed condimentum rhoncus.

Auctor vitae mauris facilisi hac erat, vehicula sollicitudin gravida? Augue hendrerit luctus; posuere suspendisse mi rutrum. Nascetur ridiculus vitae sodales sodales ultrices lorem nulla.Felis diam sed quisque tincidunt nam habitant.Hendrerit penatibus tempus justo amet; elementum per.Nibh finibus porttitor laoreet elementum massa penatibus elementum laoreet amet. Tortor senectus proin curae mollis erat nisi dui. Tincidunt sollicitudin lectus est volutpat sed. Proin scelerisque purus neque quam nec vestibulum magnis varius elementum. Nam tristique quisque eleifend pretium; orci platea fermentum aenean.

Cubilia mi convallis tempus natoque donec molestie mollis pretium purus. Vehicula at lacinia habitant non class vulputate proin accumsan a. Sollicitudin ornare duis rhoncus ad lacinia phasellus adipiscing ullamcorper. Praesent ut nisi blandit rutrum blandit ligula commodo. Faucibus massa etiam pulvinar hac sodales quisque fringilla pharetra penatibus. Aliquet potenti phasellus vel neque elementum! Praesent fringilla a tristique egestas eget nascetur.

Imperdiet sit urna curabitur varius tortor. Penatibus per pretium integer facilisi ac. Vitae libero curabitur felis porta curabitur adipiscing natoque suscipit. Sit leo sed phasellus consectetur laoreet potenti.Nunc commodo senectus commodo imperdiet nunc dui.Elementum phasellus ut aptent euismod eros ipsum habitasse. Feugiat senectus eget molestie justo mollis morbi sit. Litora aptent dolor nulla ultrices est ac enim vivamus nibh. Dictum porttitor sed cubilia magna, velit rhoncus viverra molestie. Aptent vitae interdum ultrices; sit consequat fusce.";

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow() => InitializeComponent();

    /// <summary>
    /// Function called when a button is clicked.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void ButtonInformation_Click(object sender, RoutedEventArgs e) => GorgonDialogs.Information("This is an information dialog. It is used to convey information to the user of the application.\n\nClick OK to return." + Lorem);

    /// <summary>
    /// Function called when a button is clicked.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void ButtonWarning_Click(object sender, RoutedEventArgs e) => GorgonDialogs.Warning("This is a warning dialog. It is used to inform the user that an operation may have run into trouble.\n\nClick OK to return." + Lorem);

    /// <summary>
    /// Function called when a button is clicked.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void ButtonWarningDetailed_Click(object sender, RoutedEventArgs e) => GorgonDialogs.Warning("This is a warning dialog. It is used to inform the user that an operation may have run into trouble.\n\nIt contains a detail button for more information about the warning, click it to show the detail panel.\n\nClick OK to return.", details: Lorem);

    /// <summary>
    /// Function called when a button is clicked.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void ButtonError_Click(object sender, RoutedEventArgs e) => GorgonDialogs.Error("This is an error dialog. It is used to inform the user that something has gone wrong.\n\nClick OK to return." + Lorem);

    /// <summary>
    /// Function to throw an exception.
    /// </summary>
    private void ThrowException5() => throw new Exception("This is the fifth exception.");

    /// <summary>
    /// Function to throw an exception.
    /// </summary>
    private void ThrowException4()
    {
        try
        {
            ThrowException5();
        }
        catch (Exception ex)
        {
            throw new ObjectDisposedException("This is the fourth exception.", ex);
        }
    }

    /// <summary>
    /// Function to throw an exception.
    /// </summary>
    private void ThrowException3()
    {
        try
        {
            ThrowException4();
        }
        catch (Exception ex)
        {
            throw new AccessViolationException("This is the third exception.", ex);
        }
    }

    /// <summary>
    /// Function to throw an exception.
    /// </summary>
    private void ThrowException2()
    {
        try
        {
            ThrowException3();
        }
        catch (Exception ex)
        {
            throw new FormatException("This is the second exception.", ex);
        }
    }

    /// <summary>
    /// Function to throw an exception.
    /// </summary>
    private void ThrowException1()
    {
        try
        {
            ThrowException2();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("This is the first exception.", ex);
        }
    }

    /// <summary>
    /// Function called when a button is clicked.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void ButtonErrorDetails_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ThrowException1();
        }
        catch (Exception ex)
        {
            GorgonDialogs.Error(ex, "This is an error that was triggered by an Exception.\n\nClick the Details button to show information about the Exception.\n\nClick OK to return.");
        }
    }

    /// <summary>
    /// Function called when a button is clicked.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
    {
        if (GorgonDialogs.Confirm("This is a confirmation dialog. It is used to query the user about a decision that needs to be made.\n\nClick either Yes or No to return." + Lorem) == ConfirmationResult.Yes)
        {
            GorgonDialogs.Information("You clicked Yes.\n\nClick OK to return.");
        }
        else
        {
            GorgonDialogs.Information("You clicked No.\n\nClick OK to return.");
        }
    }

    /// <summary>
    /// Function called when a button is clicked.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void ButtonConfirmCancel_Click(object sender, RoutedEventArgs e)
    {
        ConfirmationResult result = GorgonDialogs.Confirm("This is a confirmation dialog. It is used to query the user about a decision that needs to be made.\n\nClick either Yes, No or Cancel to return.",
            allowCancel: true);

        switch (result)
        {
            case ConfirmationResult.Yes:
                GorgonDialogs.Information("You clicked Yes.\n\nClick OK to return.");
                return;
            case ConfirmationResult.No:
                GorgonDialogs.Information("You clicked No.\n\nClick OK to return.");
                return;
            case ConfirmationResult.Cancel:
                GorgonDialogs.Information("You clicked Cancel.\n\nClick OK to return.");
                return;
        }

    }

    /// <summary>
    /// Function called when a button is clicked.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void ButtonConfirmToAll_Click(object sender, RoutedEventArgs e)
    {
        ConfirmationResult result = GorgonDialogs.Confirm("This is a confirmation dialog. It is used to query the user about a decision that needs to be made.\n\nClick either Yes, No or Cancel to return.",
            allowCancel: true, allowToAll: true);

        string extraText = "\n\nThis only applies to one item.";

        if ((result & ConfirmationResult.ToAll) == ConfirmationResult.ToAll)
        {
            extraText = "\n\nThis applies to all items.";
        }

        // Remove the flag.
        result &= ~ConfirmationResult.ToAll;

        switch (result)
        {
            case ConfirmationResult.Yes:
                GorgonDialogs.Information($"You clicked Yes.{extraText}\n\nClick OK to return.");
                return;
            case ConfirmationResult.No:
                GorgonDialogs.Information($"You clicked No.{extraText}\n\nClick OK to return.");
                return;
            case ConfirmationResult.Cancel:
                GorgonDialogs.Information("You clicked Cancel.\n\nClick OK to return.");
                return;
        }
    }
}