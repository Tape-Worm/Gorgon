
namespace Gorgon.Examples;

partial class FormMain
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
        ButtonInformation = new Button();
        FlowButtons = new FlowLayoutPanel();
        ButtonWarning = new Button();
        ButtonWarningDetailed = new Button();
        ButtonError = new Button();
        ButtonErrorDetailed = new Button();
        ButtonConfirm = new Button();
        ButtonConfirmCancel = new Button();
        ButtonConfirmToAll = new Button();
        TipButton = new ToolTip(components);
        FlowButtons.SuspendLayout();
        SuspendLayout();
        // 
        // ButtonInformation
        // 
        ButtonInformation.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        ButtonInformation.AutoSize = true;
        ButtonInformation.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        ButtonInformation.Location = new Point(0, 0);
        ButtonInformation.Margin = new Padding(0);
        ButtonInformation.MinimumSize = new Size(640, 0);
        ButtonInformation.Name = "ButtonInformation";
        ButtonInformation.Size = new Size(640, 42);
        ButtonInformation.TabIndex = 0;
        ButtonInformation.Text = "Information";
        TipButton.SetToolTip(ButtonInformation, "Click to show an Information Dialog");
        ButtonInformation.UseVisualStyleBackColor = true;
        ButtonInformation.Click += ButtonInformation_Click;
        // 
        // FlowButtons
        // 
        FlowButtons.AutoSize = true;
        FlowButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        FlowButtons.Controls.Add(ButtonInformation);
        FlowButtons.Controls.Add(ButtonWarning);
        FlowButtons.Controls.Add(ButtonWarningDetailed);
        FlowButtons.Controls.Add(ButtonError);
        FlowButtons.Controls.Add(ButtonErrorDetailed);
        FlowButtons.Controls.Add(ButtonConfirm);
        FlowButtons.Controls.Add(ButtonConfirmCancel);
        FlowButtons.Controls.Add(ButtonConfirmToAll);
        FlowButtons.FlowDirection = FlowDirection.TopDown;
        FlowButtons.Location = new Point(0, 0);
        FlowButtons.Name = "FlowButtons";
        FlowButtons.Size = new Size(640, 336);
        FlowButtons.TabIndex = 1;
        // 
        // ButtonWarning
        // 
        ButtonWarning.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        ButtonWarning.AutoSize = true;
        ButtonWarning.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        ButtonWarning.Location = new Point(0, 42);
        ButtonWarning.Margin = new Padding(0);
        ButtonWarning.Name = "ButtonWarning";
        ButtonWarning.Size = new Size(640, 42);
        ButtonWarning.TabIndex = 1;
        ButtonWarning.Text = "Warning";
        TipButton.SetToolTip(ButtonWarning, "Click to show a Warning Dialog");
        ButtonWarning.UseVisualStyleBackColor = true;
        ButtonWarning.Click += ButtonWarning_Click;
        // 
        // ButtonWarningDetailed
        // 
        ButtonWarningDetailed.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        ButtonWarningDetailed.AutoSize = true;
        ButtonWarningDetailed.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        ButtonWarningDetailed.Location = new Point(0, 84);
        ButtonWarningDetailed.Margin = new Padding(0);
        ButtonWarningDetailed.Name = "ButtonWarningDetailed";
        ButtonWarningDetailed.Size = new Size(640, 42);
        ButtonWarningDetailed.TabIndex = 2;
        ButtonWarningDetailed.Text = "Warning w/Detail";
        TipButton.SetToolTip(ButtonWarningDetailed, "Click to show a Warning Dialog with details.");
        ButtonWarningDetailed.UseVisualStyleBackColor = true;
        ButtonWarningDetailed.Click += ButtonWarningDetailed_Click;
        // 
        // ButtonError
        // 
        ButtonError.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        ButtonError.AutoSize = true;
        ButtonError.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        ButtonError.Location = new Point(0, 126);
        ButtonError.Margin = new Padding(0);
        ButtonError.Name = "ButtonError";
        ButtonError.Size = new Size(640, 42);
        ButtonError.TabIndex = 3;
        ButtonError.Text = "Error";
        TipButton.SetToolTip(ButtonError, "Click to show an Error Dialog");
        ButtonError.UseVisualStyleBackColor = true;
        ButtonError.Click += ButtonError_Click;
        // 
        // ButtonErrorDetailed
        // 
        ButtonErrorDetailed.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        ButtonErrorDetailed.AutoSize = true;
        ButtonErrorDetailed.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        ButtonErrorDetailed.Location = new Point(0, 168);
        ButtonErrorDetailed.Margin = new Padding(0);
        ButtonErrorDetailed.Name = "ButtonErrorDetailed";
        ButtonErrorDetailed.Size = new Size(640, 42);
        ButtonErrorDetailed.TabIndex = 4;
        ButtonErrorDetailed.Text = "Error w/Detail";
        TipButton.SetToolTip(ButtonErrorDetailed, "Click to show an Error Dialog with details.");
        ButtonErrorDetailed.UseVisualStyleBackColor = true;
        ButtonErrorDetailed.Click += ButtonErrorDetailed_Click;
        // 
        // ButtonConfirm
        // 
        ButtonConfirm.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        ButtonConfirm.AutoSize = true;
        ButtonConfirm.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        ButtonConfirm.Location = new Point(0, 210);
        ButtonConfirm.Margin = new Padding(0);
        ButtonConfirm.Name = "ButtonConfirm";
        ButtonConfirm.Size = new Size(640, 42);
        ButtonConfirm.TabIndex = 5;
        ButtonConfirm.Text = "Confirmation";
        TipButton.SetToolTip(ButtonConfirm, "Click to show a Confirmation Dialog");
        ButtonConfirm.UseVisualStyleBackColor = true;
        ButtonConfirm.Click += ButtonConfirm_Click;
        // 
        // ButtonConfirmCancel
        // 
        ButtonConfirmCancel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        ButtonConfirmCancel.AutoSize = true;
        ButtonConfirmCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        ButtonConfirmCancel.Location = new Point(0, 252);
        ButtonConfirmCancel.Margin = new Padding(0);
        ButtonConfirmCancel.Name = "ButtonConfirmCancel";
        ButtonConfirmCancel.Size = new Size(640, 42);
        ButtonConfirmCancel.TabIndex = 6;
        ButtonConfirmCancel.Text = "Confirmation w/Cancel";
        TipButton.SetToolTip(ButtonConfirmCancel, "Click to show a Confirmation Dialog with a Cancel button.");
        ButtonConfirmCancel.UseVisualStyleBackColor = true;
        ButtonConfirmCancel.Click += ButtonConfirmCancel_Click;
        // 
        // ButtonConfirmToAll
        // 
        ButtonConfirmToAll.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        ButtonConfirmToAll.AutoSize = true;
        ButtonConfirmToAll.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        ButtonConfirmToAll.Location = new Point(0, 294);
        ButtonConfirmToAll.Margin = new Padding(0);
        ButtonConfirmToAll.Name = "ButtonConfirmToAll";
        ButtonConfirmToAll.Size = new Size(640, 42);
        ButtonConfirmToAll.TabIndex = 7;
        ButtonConfirmToAll.Text = "Confirmation w/To All";
        TipButton.SetToolTip(ButtonConfirmToAll, "Click to show a Confirmation Dialog with a To All checkbox.");
        ButtonConfirmToAll.UseVisualStyleBackColor = true;
        ButtonConfirmToAll.Click += ButtonConfirmToAll_Click;
        // 
        // FormMain
        // 
        AutoScaleDimensions = new SizeF(13F, 32F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        ClientSize = new Size(721, 457);
        Controls.Add(FlowButtons);
        Font = new Font("Segoe UI", 18F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Icon = (Icon)resources.GetObject("$this.Icon");
        Margin = new Padding(6);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "FormMain";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Dialogs - Windows Forms";
        FlowButtons.ResumeLayout(false);
        FlowButtons.PerformLayout();
        ResumeLayout(false);
        PerformLayout();

    }
    private Button ButtonInformation;
    private FlowLayoutPanel FlowButtons;
    private Button ButtonWarning;
    private Button ButtonWarningDetailed;
    private Button ButtonError;
    private Button ButtonErrorDetailed;
    private Button ButtonConfirm;
    private Button ButtonConfirmCancel;
    private Button ButtonConfirmToAll;
    private ToolTip TipButton;
}
