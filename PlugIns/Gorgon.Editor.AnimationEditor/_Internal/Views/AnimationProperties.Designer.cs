namespace Gorgon.Editor.AnimationEditor
{
    partial class AnimationProperties
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
                DataContext?.OnUnload();
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
            this.components = new System.ComponentModel.Container();
            this.TableProperties = new System.Windows.Forms.TableLayoutPanel();
            this.NumericFps = new System.Windows.Forms.NumericUpDown();
            this.LabelLength = new System.Windows.Forms.Label();
            this.LabelFps = new System.Windows.Forms.Label();
            this.NumericLength = new System.Windows.Forms.NumericUpDown();
            this.CheckLoop = new System.Windows.Forms.CheckBox();
            this.LabelLooping = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.LabelFrameCount = new System.Windows.Forms.Label();
            this.LabelLoopCount = new System.Windows.Forms.Label();
            this.NumericLoopCount = new System.Windows.Forms.NumericUpDown();
            this.TipInfo = new System.Windows.Forms.ToolTip(this.components);
            this.PanelBody.SuspendLayout();
            this.TableProperties.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericFps)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericLength)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericLoopCount)).BeginInit();
            this.SuspendLayout();
            // 
            // PanelBody
            // 
            this.PanelBody.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.PanelBody.Controls.Add(this.TableProperties);
            this.PanelBody.Size = new System.Drawing.Size(364, 411);
            // 
            // TableProperties
            // 
            this.TableProperties.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.TableProperties.ColumnCount = 2;
            this.TableProperties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableProperties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableProperties.Controls.Add(this.NumericFps, 1, 1);
            this.TableProperties.Controls.Add(this.LabelLength, 0, 0);
            this.TableProperties.Controls.Add(this.LabelFps, 0, 1);
            this.TableProperties.Controls.Add(this.NumericLength, 1, 0);
            this.TableProperties.Controls.Add(this.CheckLoop, 1, 4);
            this.TableProperties.Controls.Add(this.LabelLooping, 0, 4);
            this.TableProperties.Controls.Add(this.panel1, 0, 2);
            this.TableProperties.Controls.Add(this.LabelLoopCount, 0, 5);
            this.TableProperties.Controls.Add(this.NumericLoopCount, 1, 5);
            this.TableProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableProperties.Location = new System.Drawing.Point(0, 0);
            this.TableProperties.Name = "TableProperties";
            this.TableProperties.RowCount = 7;
            this.TableProperties.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableProperties.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableProperties.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.TableProperties.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TableProperties.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableProperties.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableProperties.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TableProperties.Size = new System.Drawing.Size(364, 411);
            this.TableProperties.TabIndex = 0;
            // 
            // NumericFps
            // 
            this.NumericFps.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NumericFps.BackColor = System.Drawing.Color.White;
            this.NumericFps.DecimalPlaces = 6;
            this.NumericFps.ForeColor = System.Drawing.Color.Black;
            this.NumericFps.Location = new System.Drawing.Point(185, 32);
            this.NumericFps.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.NumericFps.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumericFps.Name = "NumericFps";
            this.NumericFps.Size = new System.Drawing.Size(176, 23);
            this.NumericFps.TabIndex = 3;
            this.NumericFps.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TipInfo.SetToolTip(this.NumericFps, "The number of animation frames that can be displayed \r\nin one second.");
            this.NumericFps.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.NumericFps.ValueChanged += new System.EventHandler(this.NumericFps_ValueChanged);
            // 
            // LabelLength
            // 
            this.LabelLength.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.LabelLength.AutoSize = true;
            this.LabelLength.Location = new System.Drawing.Point(3, 7);
            this.LabelLength.Margin = new System.Windows.Forms.Padding(3);
            this.LabelLength.Name = "LabelLength";
            this.LabelLength.Size = new System.Drawing.Size(101, 15);
            this.LabelLength.TabIndex = 0;
            this.LabelLength.Text = "Length (seconds):";
            this.LabelLength.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.TipInfo.SetToolTip(this.LabelLength, "The total running time of the animation, in seconds.");
            // 
            // LabelFps
            // 
            this.LabelFps.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.LabelFps.AutoSize = true;
            this.LabelFps.Location = new System.Drawing.Point(3, 36);
            this.LabelFps.Margin = new System.Windows.Forms.Padding(3);
            this.LabelFps.Name = "LabelFps";
            this.LabelFps.Size = new System.Drawing.Size(109, 15);
            this.LabelFps.TabIndex = 2;
            this.LabelFps.Text = "Frames per second:";
            this.LabelFps.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.TipInfo.SetToolTip(this.LabelFps, "The number of animation frames that can be displayed \r\nin one second.");
            // 
            // NumericLength
            // 
            this.NumericLength.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NumericLength.BackColor = System.Drawing.Color.White;
            this.NumericLength.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NumericLength.DecimalPlaces = 1;
            this.NumericLength.ForeColor = System.Drawing.Color.Black;
            this.NumericLength.Location = new System.Drawing.Point(185, 3);
            this.NumericLength.Maximum = new decimal(new int[] {
            7200,
            0,
            0,
            0});
            this.NumericLength.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.NumericLength.Name = "NumericLength";
            this.NumericLength.Size = new System.Drawing.Size(176, 23);
            this.NumericLength.TabIndex = 1;
            this.NumericLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TipInfo.SetToolTip(this.NumericLength, "The total running time of the animation, in seconds.");
            this.NumericLength.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumericLength.ValueChanged += new System.EventHandler(this.NumericLength_ValueChanged);
            // 
            // CheckLoop
            // 
            this.CheckLoop.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckLoop.AutoSize = true;
            this.CheckLoop.Location = new System.Drawing.Point(265, 105);
            this.CheckLoop.Margin = new System.Windows.Forms.Padding(0);
            this.CheckLoop.Name = "CheckLoop";
            this.CheckLoop.Size = new System.Drawing.Size(15, 14);
            this.CheckLoop.TabIndex = 6;
            this.TipInfo.SetToolTip(this.CheckLoop, "If checked, then the animation will repeat continuously, \r\nstarting over from the" +
        " beginning.\r\n\r\nIf unchecked, then the animation will only render once.");
            this.CheckLoop.UseVisualStyleBackColor = true;
            this.CheckLoop.Click += new System.EventHandler(this.CheckLoop_Click);
            // 
            // LabelLooping
            // 
            this.LabelLooping.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.LabelLooping.AutoSize = true;
            this.LabelLooping.Location = new System.Drawing.Point(3, 105);
            this.LabelLooping.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.LabelLooping.Name = "LabelLooping";
            this.LabelLooping.Size = new System.Drawing.Size(50, 15);
            this.LabelLooping.TabIndex = 5;
            this.LabelLooping.Text = "Looped:";
            this.LabelLooping.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.TipInfo.SetToolTip(this.LabelLooping, "If checked, then the animation will repeat continuously, \r\nstarting over from the" +
        " beginning.\r\n\r\nIf unchecked, then the animation will only render once.");
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.TableProperties.SetColumnSpan(this.panel1, 2);
            this.panel1.Controls.Add(this.LabelFrameCount);
            this.panel1.Location = new System.Drawing.Point(0, 58);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.panel1.Size = new System.Drawing.Size(364, 21);
            this.panel1.TabIndex = 7;
            // 
            // LabelFrameCount
            // 
            this.LabelFrameCount.AutoSize = true;
            this.LabelFrameCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelFrameCount.Location = new System.Drawing.Point(134, 3);
            this.LabelFrameCount.Name = "LabelFrameCount";
            this.LabelFrameCount.Size = new System.Drawing.Size(97, 15);
            this.LabelFrameCount.TabIndex = 4;
            this.LabelFrameCount.Text = "Frame count: 60";
            // 
            // LabelLoopCount
            // 
            this.LabelLoopCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.LabelLoopCount.AutoSize = true;
            this.LabelLoopCount.Location = new System.Drawing.Point(3, 133);
            this.LabelLoopCount.Margin = new System.Windows.Forms.Padding(3);
            this.LabelLoopCount.Name = "LabelLoopCount";
            this.LabelLoopCount.Size = new System.Drawing.Size(71, 15);
            this.LabelLoopCount.TabIndex = 7;
            this.LabelLoopCount.Text = "Loop count:";
            this.LabelLoopCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.TipInfo.SetToolTip(this.LabelLoopCount, "The number of times the animation can loop before ending.\r\n\r\nIf the value is set " +
        "to 0, then the animation will loop forever.");
            // 
            // NumericLoopCount
            // 
            this.NumericLoopCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NumericLoopCount.BackColor = System.Drawing.Color.White;
            this.NumericLoopCount.ForeColor = System.Drawing.Color.Black;
            this.NumericLoopCount.Location = new System.Drawing.Point(185, 129);
            this.NumericLoopCount.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.NumericLoopCount.Name = "NumericLoopCount";
            this.NumericLoopCount.Size = new System.Drawing.Size(176, 23);
            this.NumericLoopCount.TabIndex = 8;
            this.NumericLoopCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TipInfo.SetToolTip(this.NumericLoopCount, "The number of times the animation can loop before ending.\r\n\r\nIf the value is set " +
        "to 0, then the animation will loop forever.");
            this.NumericLoopCount.ValueChanged += new System.EventHandler(this.NumericLoopCount_ValueChanged);
            // 
            // TipInfo
            // 
            this.TipInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.TipInfo.ForeColor = System.Drawing.Color.White;
            this.TipInfo.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.TipInfo.ToolTipTitle = "Info";
            this.TipInfo.UseAnimation = false;
            this.TipInfo.UseFading = false;
            // 
            // AnimationProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "AnimationProperties";
            this.Size = new System.Drawing.Size(364, 468);
            this.Text = "Properties";
            this.PanelBody.ResumeLayout(false);
            this.TableProperties.ResumeLayout(false);
            this.TableProperties.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericFps)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericLength)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericLoopCount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel TableProperties;
        private System.Windows.Forms.NumericUpDown NumericFps;
        private System.Windows.Forms.Label LabelLength;
        private System.Windows.Forms.Label LabelFps;
        private System.Windows.Forms.NumericUpDown NumericLength;
        private System.Windows.Forms.Label LabelFrameCount;
        private System.Windows.Forms.CheckBox CheckLoop;
        private System.Windows.Forms.Label LabelLooping;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label LabelLoopCount;
        private System.Windows.Forms.NumericUpDown NumericLoopCount;
        private System.Windows.Forms.ToolTip TipInfo;
    }
}
