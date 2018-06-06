﻿namespace SobelComputeEngine
{
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.label1 = new System.Windows.Forms.Label();
            this.TextImagePath = new System.Windows.Forms.TextBox();
            this.ButtonImagePath = new System.Windows.Forms.Button();
            this.PanelDisplay = new System.Windows.Forms.Panel();
            this.DialogOpenPng = new System.Windows.Forms.OpenFileDialog();
            this.TrackThreshold = new System.Windows.Forms.TrackBar();
            this.TrackThickness = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.TableSliderControls = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ContentArea.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TrackThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrackThickness)).BeginInit();
            this.TableSliderControls.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ContentArea
            // 
            this.ContentArea.BackColor = System.Drawing.SystemColors.Window;
            this.ContentArea.Controls.Add(this.panel1);
            this.ContentArea.Controls.Add(this.TableSliderControls);
            this.ContentArea.ForeColor = System.Drawing.SystemColors.WindowText;
            this.ContentArea.Location = new System.Drawing.Point(1, 33);
            this.ContentArea.Size = new System.Drawing.Size(1278, 766);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 29);
            this.label1.TabIndex = 0;
            this.label1.Text = "Image Path:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TextImagePath
            // 
            this.TextImagePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextImagePath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextImagePath.Location = new System.Drawing.Point(79, 3);
            this.TextImagePath.Name = "TextImagePath";
            this.TextImagePath.ReadOnly = true;
            this.TextImagePath.Size = new System.Drawing.Size(651, 23);
            this.TextImagePath.TabIndex = 1;
            // 
            // ButtonImagePath
            // 
            this.ButtonImagePath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonImagePath.Image = global::Gorgon.Graphics.Example.Properties.Resources.loadimage_16x16;
            this.ButtonImagePath.Location = new System.Drawing.Point(736, 3);
            this.ButtonImagePath.Name = "ButtonImagePath";
            this.ButtonImagePath.Size = new System.Drawing.Size(26, 23);
            this.ButtonImagePath.TabIndex = 2;
            this.ButtonImagePath.UseVisualStyleBackColor = true;
            this.ButtonImagePath.Click += new System.EventHandler(this.ButtonImagePath_Click);
            // 
            // PanelDisplay
            // 
            this.PanelDisplay.BackColor = System.Drawing.Color.Black;
            this.PanelDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelDisplay.Location = new System.Drawing.Point(8, 8);
            this.PanelDisplay.Name = "PanelDisplay";
            this.PanelDisplay.Size = new System.Drawing.Size(1262, 677);
            this.PanelDisplay.TabIndex = 3;
            // 
            // DialogOpenPng
            // 
            this.DialogOpenPng.Filter = "Portable Network Graphics File (*.png)|*.png";
            this.DialogOpenPng.Title = "Load a PNG file as a texture";
            // 
            // TrackThreshold
            // 
            this.TrackThreshold.AutoSize = false;
            this.TrackThreshold.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TrackThreshold.Enabled = false;
            this.TrackThreshold.Location = new System.Drawing.Point(72, 3);
            this.TrackThreshold.Maximum = 90;
            this.TrackThreshold.Minimum = 1;
            this.TrackThreshold.Name = "TrackThreshold";
            this.TrackThreshold.Size = new System.Drawing.Size(289, 32);
            this.TrackThreshold.TabIndex = 4;
            this.TrackThreshold.TickFrequency = 10;
            this.TrackThreshold.Value = 25;
            this.TrackThreshold.ValueChanged += new System.EventHandler(this.TrackThreshold_ValueChanged);
            // 
            // TrackThickness
            // 
            this.TrackThickness.AutoSize = false;
            this.TrackThickness.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TrackThickness.Enabled = false;
            this.TrackThickness.Location = new System.Drawing.Point(435, 3);
            this.TrackThickness.Maximum = 5;
            this.TrackThickness.Minimum = 1;
            this.TrackThickness.Name = "TrackThickness";
            this.TrackThickness.Size = new System.Drawing.Size(289, 32);
            this.TrackThickness.TabIndex = 5;
            this.TrackThickness.Value = 2;
            this.TrackThickness.ValueChanged += new System.EventHandler(this.TrackThreshold_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 3);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 35);
            this.label2.TabIndex = 6;
            this.label2.Text = "Threshold:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(367, 3);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 35);
            this.label3.TabIndex = 7;
            this.label3.Text = "Thickness:";
            // 
            // TableSliderControls
            // 
            this.TableSliderControls.AutoSize = true;
            this.TableSliderControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TableSliderControls.ColumnCount = 4;
            this.TableSliderControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableSliderControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableSliderControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.TableSliderControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableSliderControls.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.TableSliderControls.Controls.Add(this.label1, 0, 0);
            this.TableSliderControls.Controls.Add(this.TextImagePath, 1, 0);
            this.TableSliderControls.Controls.Add(this.ButtonImagePath, 2, 0);
            this.TableSliderControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.TableSliderControls.Location = new System.Drawing.Point(0, 0);
            this.TableSliderControls.Name = "TableSliderControls";
            this.TableSliderControls.RowCount = 2;
            this.TableSliderControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableSliderControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableSliderControls.Size = new System.Drawing.Size(1278, 73);
            this.TableSliderControls.TabIndex = 8;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 4;
            this.TableSliderControls.SetColumnSpan(this.tableLayoutPanel2, 3);
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.TrackThreshold, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.TrackThickness, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.label3, 2, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 32);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(727, 38);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.PanelDisplay);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 73);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(8);
            this.panel1.Size = new System.Drawing.Size(1278, 693);
            this.panel1.TabIndex = 0;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 800);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "FormMain";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.ShowBorder = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sobel Shader - Compute Engine";
            this.ContentArea.ResumeLayout(false);
            this.ContentArea.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TrackThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrackThickness)).EndInit();
            this.TableSliderControls.ResumeLayout(false);
            this.TableSliderControls.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TextBox TextImagePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ButtonImagePath;
        private System.Windows.Forms.Panel PanelDisplay;
        private System.Windows.Forms.OpenFileDialog DialogOpenPng;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar TrackThickness;
        private System.Windows.Forms.TrackBar TrackThreshold;
        private System.Windows.Forms.TableLayoutPanel TableSliderControls;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel panel1;
    }
}
