using Gorgon.Editor.UI.Controls;

namespace Gorgon.Editor.FontEditor;

partial class FontSolidBrushView
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

        if (disposing)
        {
            UnassignEvents();
            ViewModel?.Unload();
        }

        base.Dispose(disposing);
    }



    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.PickerSolidBrush = new Gorgon.Editor.UI.Controls.ColorPicker();
        this.TableControls = new System.Windows.Forms.TableLayoutPanel();
        this.LabelBrushColor = new System.Windows.Forms.Label();
        this.PanelBody.SuspendLayout();
        this.TableControls.SuspendLayout();
        this.SuspendLayout();
        // 
        // PanelBody
        // 
        this.PanelBody.Controls.Add(this.TableControls);
        this.PanelBody.Size = new System.Drawing.Size(364, 294);
        // 
        // PickerSolidBrush
        // 
        this.PickerSolidBrush.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PickerSolidBrush.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.PickerSolidBrush.Location = new System.Drawing.Point(3, 24);
        this.PickerSolidBrush.Name = "PickerSolidBrush";
        this.PickerSolidBrush.Size = new System.Drawing.Size(361, 303);
        this.PickerSolidBrush.TabIndex = 1;
        // 
        // TableControls
        // 
        this.TableControls.AutoSize = true;
        this.TableControls.ColumnCount = 1;
        this.TableControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableControls.Controls.Add(this.LabelBrushColor, 0, 0);
        this.TableControls.Controls.Add(this.PickerSolidBrush, 0, 1);
        this.TableControls.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableControls.Location = new System.Drawing.Point(0, 0);
        this.TableControls.Name = "TableControls";
        this.TableControls.RowCount = 2;
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.Size = new System.Drawing.Size(364, 294);
        this.TableControls.TabIndex = 0;
        // 
        // LabelBrushColor
        // 
        this.LabelBrushColor.AutoSize = true;
        this.LabelBrushColor.Location = new System.Drawing.Point(3, 3);
        this.LabelBrushColor.Margin = new System.Windows.Forms.Padding(3);
        this.LabelBrushColor.Name = "LabelBrushColor";
        this.LabelBrushColor.Size = new System.Drawing.Size(101, 15);
        this.LabelBrushColor.TabIndex = 0;
        this.LabelBrushColor.Text = "Solid Brush Color:";
        // 
        // FontSolidBrushView
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
        this.Name = "FontSolidBrushView";
        this.Size = new System.Drawing.Size(364, 351);
        this.Text = "Font Brush - Solid";
        this.PanelBody.ResumeLayout(false);
        this.PanelBody.PerformLayout();
        this.TableControls.ResumeLayout(false);
        this.TableControls.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }



    private ColorPicker PickerSolidBrush;
    private System.Windows.Forms.TableLayoutPanel TableControls;
    private System.Windows.Forms.Label LabelBrushColor;
}
