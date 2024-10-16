﻿namespace Gorgon.Editor.Views;

partial class StageRecent
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
            ClearItems();
        }
        base.Dispose(disposing);
    }



    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.PanelRecent = new System.Windows.Forms.Panel();
        this.PanelBorder = new System.Windows.Forms.Panel();
        this.panel1 = new System.Windows.Forms.Panel();
        this.PanelRecentItems = new System.Windows.Forms.FlowLayoutPanel();
        this.panel2 = new System.Windows.Forms.Panel();
        this.panel3 = new System.Windows.Forms.Panel();
        this.LabelHeader = new System.Windows.Forms.Label();
        this.PanelRecent.SuspendLayout();
        this.PanelBorder.SuspendLayout();
        this.panel1.SuspendLayout();
        this.panel2.SuspendLayout();
        this.panel3.SuspendLayout();
        this.SuspendLayout();
        // 
        // PanelRecent
        // 
        this.PanelRecent.Controls.Add(this.PanelBorder);
        this.PanelRecent.Controls.Add(this.panel2);
        this.PanelRecent.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelRecent.Location = new System.Drawing.Point(0, 0);
        this.PanelRecent.Name = "PanelRecent";
        this.PanelRecent.Size = new System.Drawing.Size(600, 468);
        this.PanelRecent.TabIndex = 0;
        // 
        // PanelBorder
        // 
        this.PanelBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.PanelBorder.Controls.Add(this.panel1);
        this.PanelBorder.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelBorder.Location = new System.Drawing.Point(0, 33);
        this.PanelBorder.Name = "PanelBorder";
        this.PanelBorder.Padding = new System.Windows.Forms.Padding(0, 1, 0, 0);
        this.PanelBorder.Size = new System.Drawing.Size(600, 435);
        this.PanelBorder.TabIndex = 1;
        // 
        // panel1
        // 
        this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.panel1.Controls.Add(this.PanelRecentItems);
        this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panel1.Location = new System.Drawing.Point(0, 1);
        this.panel1.Name = "panel1";
        this.panel1.Size = new System.Drawing.Size(600, 434);
        this.panel1.TabIndex = 0;
        // 
        // PanelRecentItems
        // 
        this.PanelRecentItems.AutoScroll = true;
        this.PanelRecentItems.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelRecentItems.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.PanelRecentItems.Location = new System.Drawing.Point(0, 0);
        this.PanelRecentItems.Name = "PanelRecentItems";
        this.PanelRecentItems.Size = new System.Drawing.Size(600, 434);
        this.PanelRecentItems.TabIndex = 2;
        this.PanelRecentItems.WrapContents = false;
        this.PanelRecentItems.Layout += new System.Windows.Forms.LayoutEventHandler(this.PanelRecentItems_Layout);
        // 
        // panel2
        // 
        this.panel2.AutoSize = true;
        this.panel2.BackColor = System.Drawing.Color.SteelBlue;
        this.panel2.Controls.Add(this.panel3);
        this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
        this.panel2.Location = new System.Drawing.Point(0, 0);
        this.panel2.Name = "panel2";
        this.panel2.Padding = new System.Windows.Forms.Padding(3, 0, 0, 1);
        this.panel2.Size = new System.Drawing.Size(600, 33);
        this.panel2.TabIndex = 2;
        // 
        // panel3
        // 
        this.panel3.AutoSize = true;
        this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.panel3.Controls.Add(this.LabelHeader);
        this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panel3.ForeColor = System.Drawing.Color.White;
        this.panel3.Location = new System.Drawing.Point(3, 0);
        this.panel3.Name = "panel3";
        this.panel3.Size = new System.Drawing.Size(597, 32);
        this.panel3.TabIndex = 1;
        // 
        // LabelHeader
        // 
        this.LabelHeader.AutoSize = true;
        this.LabelHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this.LabelHeader.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.LabelHeader.Location = new System.Drawing.Point(0, 0);
        this.LabelHeader.Name = "LabelHeader";
        this.LabelHeader.Size = new System.Drawing.Size(177, 32);
        this.LabelHeader.TabIndex = 1;
        this.LabelHeader.Text = "Recent Projects";
        this.LabelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // StageRecent
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.PanelRecent);
        this.Name = "StageRecent";
        this.PanelRecent.ResumeLayout(false);
        this.PanelRecent.PerformLayout();
        this.PanelBorder.ResumeLayout(false);
        this.panel1.ResumeLayout(false);
        this.panel2.ResumeLayout(false);
        this.panel2.PerformLayout();
        this.panel3.ResumeLayout(false);
        this.panel3.PerformLayout();
        this.ResumeLayout(false);

    }



    private System.Windows.Forms.Panel PanelRecent;
    private System.Windows.Forms.Panel PanelBorder;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.Label LabelHeader;
    private System.Windows.Forms.FlowLayoutPanel PanelRecentItems;
}
