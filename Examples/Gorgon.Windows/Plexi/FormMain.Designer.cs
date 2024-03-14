
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
        this.TableControls = new System.Windows.Forms.TableLayoutPanel();
        this.ButtonOverlayTextbox = new System.Windows.Forms.Button();
        this.LabelPlexi = new System.Windows.Forms.Label();
        this.ButtonOverlayForm = new System.Windows.Forms.Button();
        this.PanelPlexi = new System.Windows.Forms.Panel();
        this.CheckList = new System.Windows.Forms.CheckedListBox();
        this.DatePick = new System.Windows.Forms.DateTimePicker();
        this.TextPlexi = new System.Windows.Forms.TextBox();
        this.TableControls.SuspendLayout();
        this.PanelPlexi.SuspendLayout();
        this.SuspendLayout();
        // 
        // TableControls
        // 
        this.TableControls.AutoSize = true;
        this.TableControls.ColumnCount = 2;
        this.TableControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.TableControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.TableControls.Controls.Add(this.ButtonOverlayTextbox, 1, 1);
        this.TableControls.Controls.Add(this.LabelPlexi, 0, 0);
        this.TableControls.Controls.Add(this.ButtonOverlayForm, 0, 1);
        this.TableControls.Controls.Add(this.PanelPlexi, 1, 0);
        this.TableControls.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableControls.Location = new System.Drawing.Point(0, 0);
        this.TableControls.Name = "TableControls";
        this.TableControls.RowCount = 2;
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.Size = new System.Drawing.Size(1280, 800);
        this.TableControls.TabIndex = 0;
        // 
        // ButtonOverlayTextbox
        // 
        this.ButtonOverlayTextbox.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.ButtonOverlayTextbox.AutoSize = true;
        this.ButtonOverlayTextbox.Location = new System.Drawing.Point(908, 759);
        this.ButtonOverlayTextbox.Margin = new System.Windows.Forms.Padding(3, 16, 3, 16);
        this.ButtonOverlayTextbox.Name = "ButtonOverlayTextbox";
        this.ButtonOverlayTextbox.Size = new System.Drawing.Size(104, 25);
        this.ButtonOverlayTextbox.TabIndex = 3;
        this.ButtonOverlayTextbox.Text = "Overlay Textbox";
        this.ButtonOverlayTextbox.UseVisualStyleBackColor = true;
        this.ButtonOverlayTextbox.Click += new System.EventHandler(this.ButtonOverlayTextbox_Click);
        // 
        // LabelPlexi
        // 
        this.LabelPlexi.AutoSize = true;
        this.LabelPlexi.Location = new System.Drawing.Point(3, 3);
        this.LabelPlexi.Margin = new System.Windows.Forms.Padding(3);
        this.LabelPlexi.Name = "LabelPlexi";
        this.LabelPlexi.Size = new System.Drawing.Size(632, 510);
        this.LabelPlexi.TabIndex = 0;
        this.LabelPlexi.Text = resources.GetString("LabelPlexi.Text");
        // 
        // ButtonOverlayForm
        // 
        this.ButtonOverlayForm.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.ButtonOverlayForm.AutoSize = true;
        this.ButtonOverlayForm.Location = new System.Drawing.Point(268, 759);
        this.ButtonOverlayForm.Margin = new System.Windows.Forms.Padding(3, 16, 3, 16);
        this.ButtonOverlayForm.Name = "ButtonOverlayForm";
        this.ButtonOverlayForm.Size = new System.Drawing.Size(104, 25);
        this.ButtonOverlayForm.TabIndex = 2;
        this.ButtonOverlayForm.Text = "Overlay Window";
        this.ButtonOverlayForm.UseVisualStyleBackColor = true;
        this.ButtonOverlayForm.Click += new System.EventHandler(this.ButtonOverlayForm_Click);
        // 
        // PanelPlexi
        // 
        this.PanelPlexi.Controls.Add(this.CheckList);
        this.PanelPlexi.Controls.Add(this.DatePick);
        this.PanelPlexi.Controls.Add(this.TextPlexi);
        this.PanelPlexi.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelPlexi.Location = new System.Drawing.Point(643, 3);
        this.PanelPlexi.Name = "PanelPlexi";
        this.PanelPlexi.Size = new System.Drawing.Size(634, 737);
        this.PanelPlexi.TabIndex = 4;
        // 
        // CheckList
        // 
        this.CheckList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.CheckList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.CheckList.CheckOnClick = true;
        this.CheckList.FormattingEnabled = true;
        this.CheckList.Items.AddRange(new object[] {
        "Item 1",
        "Item 2",
        "Item 3",
        "Item 4",
        "Item 5",
        "Item 6",
        "Item 7",
        "Item 8",
        "Item 9",
        "Item 10"});
        this.CheckList.Location = new System.Drawing.Point(4, 391);
        this.CheckList.Name = "CheckList";
        this.CheckList.Size = new System.Drawing.Size(621, 326);
        this.CheckList.TabIndex = 4;
        this.CheckList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.CheckList_ItemCheck);
        // 
        // DatePick
        // 
        this.DatePick.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.DatePick.Location = new System.Drawing.Point(3, 361);
        this.DatePick.Name = "DatePick";
        this.DatePick.Size = new System.Drawing.Size(622, 23);
        this.DatePick.TabIndex = 3;
        this.DatePick.ValueChanged += new System.EventHandler(this.DatePick_ValueChanged);
        // 
        // TextPlexi
        // 
        this.TextPlexi.AcceptsReturn = true;
        this.TextPlexi.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.TextPlexi.Dock = System.Windows.Forms.DockStyle.Top;
        this.TextPlexi.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.TextPlexi.Location = new System.Drawing.Point(0, 0);
        this.TextPlexi.Multiline = true;
        this.TextPlexi.Name = "TextPlexi";
        this.TextPlexi.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.TextPlexi.Size = new System.Drawing.Size(634, 355);
        this.TextPlexi.TabIndex = 2;
        this.TextPlexi.Text = resources.GetString("TextPlexi.Text");
        this.TextPlexi.Leave += new System.EventHandler(this.TextPlexi_Leave);
        // 
        // FormMain
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(1280, 800);
        this.Controls.Add(this.TableControls);
        
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "FormMain";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Plexi";
        this.TableControls.ResumeLayout(false);
        this.TableControls.PerformLayout();
        this.PanelPlexi.ResumeLayout(false);
        this.PanelPlexi.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }



    private System.Windows.Forms.TableLayoutPanel TableControls;
    private System.Windows.Forms.Label LabelPlexi;
    private System.Windows.Forms.Button ButtonOverlayTextbox;
    private System.Windows.Forms.Button ButtonOverlayForm;
    private System.Windows.Forms.Panel PanelPlexi;
    private System.Windows.Forms.TextBox TextPlexi;
    private System.Windows.Forms.CheckedListBox CheckList;
    private System.Windows.Forms.DateTimePicker DatePick;
}
