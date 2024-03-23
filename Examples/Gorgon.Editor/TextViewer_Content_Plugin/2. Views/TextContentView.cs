
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 3, 2020 3:45:05 PM
// 

using System.ComponentModel;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Graphics.Core;

namespace Gorgon.Examples;

/// <summary>
/// This is our actual view used to display and edit the content
/// </summary>
/// <remarks>
/// This is the main UI that will display our content. It provides functionality suited for editing content that requires Gorgon's 2D 
/// rendering capabilities
/// 
/// For creating an editor view, we need to inherit from the VisualContentBaseControl (if rendering is required, otherwise the 
/// EditorBaseControl can be used for more basic/customizable view functionality), and implement the IDataContext{T} interface. This 
/// provides us with the properties/methods for assigning a view model as the data context for the UI
/// 
/// As mentioned in the plug in class, the UI in the content plug in (and everywhere else in the editor really) uses the MVVM pattern
/// (Model-View-View Model), or, rather, a somewhat bastardized version of it. This allows us to provide a separation between the user 
/// interface and the data that we're working on. In MVVM, the view never works directly on the model data, we use the view model to 
/// act as a conduit to interact with the data and UI. The view model acts as a presenter by sending the model data to the view, and 
/// a controller by receiving commands from the view to act upon the data
/// 
/// This particular content view's base class has functionality that already includes the ability to scroll around the content if it 
/// doesn't fit within the client space of the control, host custom rendering for content using Gorgon's 2D graphics API, and drag 
/// and drop functionality. 
/// </remarks>
internal partial class TextContentView
    : VisualContentBaseControl, IDataContext<ITextContent>
{

    // The renderer for our text data.
    // 
    // We use this to display our content data using Gorgon's 2D functionality. It also has built-in functionality for panning and 
    // zooming in/out on the content data. 

    private TextRenderer _renderer;

    // The form containing the ribbon used to merge with the application ribbon.
    private readonly FormRibbon _formRibbon;

    /// <summary>Property to return the data context assigned to this view.</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // <-- So we don't show up in the IDE.
    public ITextContent ViewModel
    {
        get;
        private set;
    }

    /// <summary>
    /// Function to initialize the view from the data context.
    /// </summary>
    /// <param name="dataContext">The data context to use.</param>
    private void InitializeFromDataContext(ITextContent dataContext)
    {
        // This method is used to transfer the data from the view model into the view before the view actually loads (prior to the 
        // Load event - most of the time...) If we have no data context, we call ResetDataContext to reset the view back to its 
        // default state.

        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }
    }

    /// <summary>Function called when the renderer is switched.</summary>
    /// <param name="renderer">The current renderer.</param>
    /// <param name="resetZoom">
    ///   <b>true</b> if the zoom should be reset, <b>false</b> if not.</param>
    protected override void OnSwitchRenderer(IContentRenderer renderer, bool resetZoom)
    {
        // This method is called when we call SwitchRenderer so that renderer specific functionality can be called after the 
        // renderer is switched.

        // If we have a ribbon to merge into the editor ribbon, we should pass in the current renderer.
        _formRibbon.ContentRenderer = renderer;

        if (renderer is null)
        {
            return;
        }

        // Calling this will reset the panning/zooming to the default for the currently active renderer. The DefaultZoom 
        // method is not defined on the base class for the renderer, so that if users don't require this can skip it.
        if (resetZoom)
        {
            _renderer.DefaultZoom();
        }
    }

    /// <summary>Raises the <see cref="UserControl.Load"/> event.</summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // We don't want to trigger any of our code in the designer, so we skip out while editing the IDE.
        if (IsDesignTime)
        {
            return;
        }

        // If we have a ribbon, we can force it's handle creation here as a precaution (I honestly can't remember why 
        // this here, probably to deal with a bug in the components?)
        if (!_formRibbon.IsHandleCreated)
        {
            _formRibbon.CreateControl();
        }

        // Always call the OnLoad for the data context here so the view model can perform any required initialization
        // after the control is created and loaded.
        ViewModel?.Load();

        // The control that we render our content into is selectable, so we should default selection into it.
        RenderControl?.Select();
    }

    /// <summary>Function called to shut down the view and perform any clean up required (including user defined graphics objects).</summary>
    protected override void OnShutdown()
    {
        // Always call unload when the control is shutting down. This will enable us to perform any necessary clean up 
        // on the view model.
        ViewModel?.Unload();
        base.OnShutdown();
    }

    /// <summary>Function to allow applications to set up rendering for their specific use cases.</summary>
    /// <param name="context">The graphics context from the host application.</param>
    /// <param name="swapChain">The current swap chain for the rendering panel.</param>
    protected override void OnSetupContentRenderer(IGraphicsContext context, GorgonSwapChain swapChain)
    {
        // This method is used to set up rendering for our content using the Gorgon Graphics/Gorgon 2D 
        // functionality.
        //
        // It provides us with the graphics context from the editor application that contains instances 
        // of our graphics and 2D renderer interfaces. A default swap chain is also provided so that 
        // we can render into our view.
        _renderer = new TextRenderer(context.Renderer2D, swapChain, context.FontFactory, ViewModel);
        // Create any necessary resources for the renderer here.
        _renderer.CreateResources();

        // Any renderers we create must be added to the internal renderer list so they can be managed by 
        // the content system.
        AddRenderer(_renderer.Name, _renderer);

        // We use this method to change renderers. In this case, we set the only renderer as the default.
        SwitchRenderer(_renderer.Name, true);
    }

    /// <summary>
    /// Function to assign a data context to the view as a view model.
    /// </summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>
    /// <para>
    /// Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.
    /// </para>
    /// </remarks>
    public void SetDataContext(ITextContent dataContext)
    {
        // This method is where we bind our view model to the view. This allows us to capture any property changes on the 
        // view model and update the view accordingly.
        //
        // We can capture the changes by overriding the OnPropertyChanging method to handle any changes prior to the property 
        // actually changing. And OnPropertyChanged to handle any changes after the property is updated.
        //
        // This example doesn't really need to update the control (just its renderer), so we don't use these methods here.

        OnSetDataContext(dataContext);

        InitializeFromDataContext(dataContext);

        ViewModel = dataContext;

        // If we have any controls embedded on the control (or associated with the control in the case of the ribbon) that have 
        // view model support, we need to assign their view models as well. These can be child view models, or the current view 
        // model for the control.
        //
        // In this case we have a color picker UI panel that appears when we change the text color, and it will need a view 
        // model. This view model is a child view model off of our main view model.
        _formRibbon.SetDataContext(dataContext);
        TextColorPicker.SetDataContext(dataContext?.TextColor);
    }

    /// <summary>Initializes a new instance of the <see cref="TextContentView"/> class.</summary>
    public TextContentView()
    {
        InitializeComponent();

        // In order to get our content ribbon to merge with our main application ribbon, we have to create an instance of it.
        // Due to how Krypton handles the ribbon, it can only be placed on a form, so we need to instatiate the form holding 
        // the ribbon, and then assign the ribbon from the form to our ribbon property.
        //
        // One the editor is opened, this ribbon will be picked up and merged into the main ribbon, and unmerged when the 
        // editor closes. 
        //
        // NOTE: ALWAYS dispose of this object in the Dipose method (look in the TextContentView.Designer.cs file).
        _formRibbon = new FormRibbon();
        Ribbon = _formRibbon.RibbonTextContent;

        // Our view can use sub panels that pop in and out depending on the context for editing.
        // In this instance, we've planted a color picker on the view (in the host panel on the right hand side of the view).
        // And now we have to register it with the system. When we assign the view model for the panel to the CurrentPanel 
        // property of this views view model, it will automatically spring into place and when we clear the property by 
        // setting it to NULL, it will go away again.
        //
        // To register a sub panel, we pass in the fully qualified type name of the view model for the control, and the 
        // instance of the sub panel control.
        RegisterChildPanel(typeof(TextColor).FullName, TextColorPicker);
    }
}
