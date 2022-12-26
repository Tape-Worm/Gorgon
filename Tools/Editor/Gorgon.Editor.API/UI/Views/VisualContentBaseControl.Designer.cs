namespace Gorgon.Editor.UI.Views;

partial class VisualContentBaseControl
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.TableViewControls = new System.Windows.Forms.TableLayoutPanel();
        this.ScrollHorizontal = new System.Windows.Forms.HScrollBar();
        this.ScrollVertical = new System.Windows.Forms.VScrollBar();
        this.PanelRenderWindow = new Gorgon.UI.GorgonSelectablePanel();
        this.ButtonCenter = new System.Windows.Forms.Button();
        this.PanelStatus = new System.Windows.Forms.Panel();
        this.PresentationPanel.SuspendLayout();
        this.TableViewControls.SuspendLayout();
        this.SuspendLayout();
        // 
        // PresentationPanel
        // 
        this.PresentationPanel.Controls.Add(this.TableViewControls);
        this.PresentationPanel.Location = new System.Drawing.Point(0, 21);
        this.PresentationPanel.Size = new System.Drawing.Size(861, 684);
        // 
        // HostPanel
        // 
        this.HostPanelControls.Size = new System.Drawing.Size(159, 707);
        // 
        // TableViewControls
        // 
        this.TableViewControls.ColumnCount = 2;
        this.TableViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableViewControls.Controls.Add(this.ScrollHorizontal, 0, 1);
        this.TableViewControls.Controls.Add(this.ScrollVertical, 1, 0);
        this.TableViewControls.Controls.Add(this.PanelRenderWindow, 0, 0);
        this.TableViewControls.Controls.Add(this.ButtonCenter, 1, 1);
        this.TableViewControls.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableViewControls.Location = new System.Drawing.Point(0, 0);
        this.TableViewControls.Name = "TableViewControls";
        this.TableViewControls.RowCount = 2;
        this.TableViewControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.TableViewControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableViewControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableViewControls.Size = new System.Drawing.Size(861, 684);
        this.TableViewControls.TabIndex = 1;
        // 
        // ScrollHorizontal
        // 
        this.ScrollHorizontal.Dock = System.Windows.Forms.DockStyle.Fill;
        this.ScrollHorizontal.Enabled = false;
        this.ScrollHorizontal.Location = new System.Drawing.Point(0, 662);
        this.ScrollHorizontal.Maximum = 110;
        this.ScrollHorizontal.Minimum = -100;
        this.ScrollHorizontal.Name = "ScrollHorizontal";
        this.ScrollHorizontal.Size = new System.Drawing.Size(839, 22);
        this.ScrollHorizontal.TabIndex = 0;
        // 
        // ScrollVertical
        // 
        this.ScrollVertical.Dock = System.Windows.Forms.DockStyle.Fill;
        this.ScrollVertical.Enabled = false;
        this.ScrollVertical.Location = new System.Drawing.Point(839, 0);
        this.ScrollVertical.Maximum = 110;
        this.ScrollVertical.Minimum = -100;
        this.ScrollVertical.Name = "ScrollVertical";
        this.ScrollVertical.Size = new System.Drawing.Size(22, 662);
        this.ScrollVertical.TabIndex = 1;
        // 
        // PanelRenderWindow
        // 
        this.PanelRenderWindow.AllowDrop = true;
        this.PanelRenderWindow.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelRenderWindow.Location = new System.Drawing.Point(3, 3);
        this.PanelRenderWindow.Name = "PanelRenderWindow";
        this.PanelRenderWindow.ShowFocus = false;
        this.PanelRenderWindow.Size = new System.Drawing.Size(833, 656);
        this.PanelRenderWindow.TabIndex = 2;
        this.PanelRenderWindow.DragDrop += new System.Windows.Forms.DragEventHandler(this.PanelRenderWindow_DragDrop);
        this.PanelRenderWindow.DragEnter += new System.Windows.Forms.DragEventHandler(this.PanelRenderWindow_DragEnter);
        this.PanelRenderWindow.DragOver += new System.Windows.Forms.DragEventHandler(this.PanelRenderWindow_DragOver);
        this.PanelRenderWindow.Resize += new System.EventHandler(this.PanelRenderWindow_Resize);
        // 
        // ButtonCenter
        // 
        this.ButtonCenter.AutoSize = true;
        this.ButtonCenter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonCenter.Dock = System.Windows.Forms.DockStyle.Fill;
        this.ButtonCenter.FlatAppearance.BorderSize = 0;
        this.ButtonCenter.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonCenter.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonCenter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonCenter.Image = global::Gorgon.Editor.Properties.Resources.center_16x16;
        this.ButtonCenter.Location = new System.Drawing.Point(839, 662);
        this.ButtonCenter.Margin = new System.Windows.Forms.Padding(0);
        this.ButtonCenter.Name = "ButtonCenter";
        this.ButtonCenter.Size = new System.Drawing.Size(22, 22);
        this.ButtonCenter.TabIndex = 3;
        this.ButtonCenter.Click += new System.EventHandler(this.ButtonCenter_Click);
        // 
        // PanelStatus
        // 
        this.PanelStatus.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.PanelStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.PanelStatus.Location = new System.Drawing.Point(0, 705);
        this.PanelStatus.Name = "PanelStatus";
        this.PanelStatus.Size = new System.Drawing.Size(861, 24);
        this.PanelStatus.TabIndex = 0;
        // 
        // VisualContentBaseControl
        // 
        this.AllowDrop = true;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.PanelStatus);
        this.Name = "VisualContentBaseControl";
        this.RenderControl = this.PanelRenderWindow;
        this.Size = new System.Drawing.Size(1021, 729);
        this.Controls.SetChildIndex(this.PanelStatus, 0);
        this.Controls.SetChildIndex(this.PresentationPanel, 0);
        this.PresentationPanel.ResumeLayout(false);
        this.TableViewControls.ResumeLayout(false);
        this.TableViewControls.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel TableViewControls;
    private System.Windows.Forms.HScrollBar ScrollHorizontal;
    [System.Runtime.CompilerServices.AccessedThroughProperty("RenderWindow")]
    private Gorgon.UI.GorgonSelectablePanel PanelRenderWindow;
    private System.Windows.Forms.Button ButtonCenter;
    private System.Windows.Forms.VScrollBar ScrollVertical;
    [System.Runtime.CompilerServices.AccessedThroughProperty("StatusPanel")]
    private System.Windows.Forms.Panel PanelStatus;
}
