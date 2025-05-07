using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Examples;

partial class Form
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

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
        ComponentResourceManager resources = new ComponentResourceManager(typeof(Form));
        LabelFps = new Label();
        LabelInstructions = new Label();
        TableDisplay = new TableLayoutPanel();
        FlowInstructions = new FlowLayoutPanel();
        ImageKeyboardIcon = new PictureBox();
        PanelGraphics = new Panel();
        PanelFps = new Panel();
        TableDisplay.SuspendLayout();
        FlowInstructions.SuspendLayout();
        ((ISupportInitialize)ImageKeyboardIcon).BeginInit();
        PanelFps.SuspendLayout();
        SuspendLayout();
        // 
        // LabelFps
        // 
        LabelFps.AutoSize = true;
        LabelFps.Dock = DockStyle.Fill;
        LabelFps.ForeColor = System.Drawing.Color.FromArgb(255, 255, 128);
        LabelFps.Location = new System.Drawing.Point(0, 0);
        LabelFps.Margin = new Padding(3);
        LabelFps.Name = "LabelFps";
        LabelFps.Size = new System.Drawing.Size(38, 15);
        LabelFps.TabIndex = 0;
        LabelFps.Text = "FPS: 0";
        // 
        // LabelInstructions
        // 
        LabelInstructions.Anchor = AnchorStyles.Left;
        LabelInstructions.AutoSize = true;
        LabelInstructions.Location = new System.Drawing.Point(25, 3);
        LabelInstructions.Name = "LabelInstructions";
        LabelInstructions.Size = new System.Drawing.Size(220, 15);
        LabelInstructions.TabIndex = 1;
        LabelInstructions.Text = "Press the space bar to change idle loops.";
        // 
        // TableDisplay
        // 
        TableDisplay.AutoSize = true;
        TableDisplay.ColumnCount = 1;
        TableDisplay.ColumnStyles.Add(new ColumnStyle());
        TableDisplay.Controls.Add(FlowInstructions, 0, 2);
        TableDisplay.Dock = DockStyle.Bottom;
        TableDisplay.Location = new System.Drawing.Point(0, 778);
        TableDisplay.Name = "TableDisplay";
        TableDisplay.RowCount = 3;
        TableDisplay.RowStyles.Add(new RowStyle());
        TableDisplay.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        TableDisplay.RowStyles.Add(new RowStyle());
        TableDisplay.Size = new System.Drawing.Size(1280, 22);
        TableDisplay.TabIndex = 3;
        // 
        // FlowInstructions
        // 
        FlowInstructions.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        FlowInstructions.AutoSize = true;
        FlowInstructions.Controls.Add(ImageKeyboardIcon);
        FlowInstructions.Controls.Add(LabelInstructions);
        FlowInstructions.Location = new System.Drawing.Point(0, 0);
        FlowInstructions.Margin = new Padding(0);
        FlowInstructions.Name = "FlowInstructions";
        FlowInstructions.Size = new System.Drawing.Size(1280, 22);
        FlowInstructions.TabIndex = 4;
        // 
        // ImageKeyboardIcon
        // 
        ImageKeyboardIcon.Anchor = AnchorStyles.Left;
        ImageKeyboardIcon.Image = Properties.Resources.keyboardIcon;
        ImageKeyboardIcon.Location = new System.Drawing.Point(3, 3);
        ImageKeyboardIcon.MaximumSize = new System.Drawing.Size(16, 16);
        ImageKeyboardIcon.Name = "ImageKeyboardIcon";
        ImageKeyboardIcon.Size = new System.Drawing.Size(16, 16);
        ImageKeyboardIcon.SizeMode = PictureBoxSizeMode.CenterImage;
        ImageKeyboardIcon.TabIndex = 2;
        ImageKeyboardIcon.TabStop = false;
        // 
        // PanelGraphics
        // 
        PanelGraphics.Dock = DockStyle.Fill;
        PanelGraphics.Location = new System.Drawing.Point(0, 15);
        PanelGraphics.Name = "PanelGraphics";
        PanelGraphics.Size = new System.Drawing.Size(1280, 763);
        PanelGraphics.TabIndex = 3;
        // 
        // PanelFps
        // 
        PanelFps.AutoSize = true;
        PanelFps.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        PanelFps.Controls.Add(LabelFps);
        PanelFps.Dock = DockStyle.Top;
        PanelFps.Location = new System.Drawing.Point(0, 0);
        PanelFps.Name = "PanelFps";
        PanelFps.Size = new System.Drawing.Size(1280, 15);
        PanelFps.TabIndex = 0;
        // 
        // Form
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
        ClientSize = new System.Drawing.Size(1280, 800);
        Controls.Add(PanelGraphics);
        Controls.Add(PanelFps);
        Controls.Add(TableDisplay);
        DoubleBuffered = true;
        ForeColor = System.Drawing.Color.White;
        Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
        Name = "Form";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "More Idling - Advanced Idling Techniques";
        TableDisplay.ResumeLayout(false);
        TableDisplay.PerformLayout();
        FlowInstructions.ResumeLayout(false);
        FlowInstructions.PerformLayout();
        ((ISupportInitialize)ImageKeyboardIcon).EndInit();
        PanelFps.ResumeLayout(false);
        PanelFps.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private Label LabelFps;
    private Label LabelInstructions;
    private TableLayoutPanel TableDisplay;
    private PictureBox ImageKeyboardIcon;
    private Panel PanelGraphics;
    private FlowLayoutPanel FlowInstructions;
    private Panel PanelFps;
}