#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: March 14, 2019 11:34:27 AM
// 
#endregion

namespace Gorgon.Editor.AnimationEditor
{
    partial class AnimationEditorView
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
            if (disposing)
            {
                SetDataContext(null);
                Tracks.SetDataContext(null);
                AddTracks.SetDataContext(null);
            }

            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (disposing)
            {
                UnregisterChildPanel(typeof(ColorValueEditor).FullName);
                UnregisterChildPanel(typeof(KeyValueEditor).FullName);
                UnregisterChildPanel(typeof(AddTrack).FullName);
                UnregisterChildPanel(typeof(AnimProperties).FullName);
                _ribbonForm?.Dispose();
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
            this.PanelTrackingControls = new System.Windows.Forms.Panel();
            this.Tracks = new Gorgon.Editor.AnimationEditor.AnimationTrackContainer();
            this.SplitTrack = new System.Windows.Forms.Splitter();
            this.AddTracks = new Gorgon.Editor.AnimationEditor.AnimationAddTrack();
            this.AnimationProperties = new Gorgon.Editor.AnimationEditor.AnimationProperties();
            this.FloatValuesEditor = new Gorgon.Editor.AnimationEditor.AnimationFloatKeyEditor();
            this.ColorValuesEditor = new Gorgon.Editor.AnimationEditor.AnimationColorKeyEditor();
            this.HostPanel.SuspendLayout();
            this.HostPanelControls.SuspendLayout();
            this.PanelTrackingControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // StatusPanel
            // 
            this.StatusPanel.Location = new System.Drawing.Point(0, 569);
            this.StatusPanel.Size = new System.Drawing.Size(844, 0);
            this.StatusPanel.Visible = false;
            // 
            // PresentationPanel
            // 
            this.PresentationPanel.Location = new System.Drawing.Point(0, 21);
            this.PresentationPanel.Size = new System.Drawing.Size(844, 548);
            // 
            // HostPanel
            // 
            this.HostPanel.Location = new System.Drawing.Point(844, 21);
            this.HostPanel.Size = new System.Drawing.Size(393, 548);
            // 
            // HostPanelControls
            // 
            this.HostPanelControls.Controls.Add(this.ColorValuesEditor);
            this.HostPanelControls.Controls.Add(this.FloatValuesEditor);
            this.HostPanelControls.Controls.Add(this.AnimationProperties);
            this.HostPanelControls.Controls.Add(this.AddTracks);
            this.HostPanelControls.Size = new System.Drawing.Size(392, 547);
            // 
            // PanelTrackingControls
            // 
            this.PanelTrackingControls.AutoScroll = true;
            this.PanelTrackingControls.Controls.Add(this.Tracks);
            this.PanelTrackingControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PanelTrackingControls.Location = new System.Drawing.Point(0, 573);
            this.PanelTrackingControls.Name = "PanelTrackingControls";
            this.PanelTrackingControls.Size = new System.Drawing.Size(1237, 211);
            this.PanelTrackingControls.TabIndex = 3;
            // 
            // Tracks
            // 
            this.Tracks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.Tracks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tracks.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Tracks.ForeColor = System.Drawing.Color.White;
            this.Tracks.Location = new System.Drawing.Point(0, 0);
            this.Tracks.Name = "Tracks";
            this.Tracks.Size = new System.Drawing.Size(1237, 211);
            this.Tracks.TabIndex = 0;
            // 
            // SplitTrack
            // 
            this.SplitTrack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.SplitTrack.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.SplitTrack.Location = new System.Drawing.Point(0, 569);
            this.SplitTrack.MinExtra = 100;
            this.SplitTrack.MinSize = 100;
            this.SplitTrack.Name = "SplitTrack";
            this.SplitTrack.Size = new System.Drawing.Size(1237, 4);
            this.SplitTrack.TabIndex = 4;
            this.SplitTrack.TabStop = false;
            this.SplitTrack.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.SplitTrack_SplitterMoved);
            // 
            // AddTracks
            // 
            this.AddTracks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.AddTracks.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.AddTracks.ForeColor = System.Drawing.Color.White;
            this.AddTracks.Location = new System.Drawing.Point(0, 0);
            this.AddTracks.Name = "AddTracks";
            this.AddTracks.Size = new System.Drawing.Size(389, 548);
            this.AddTracks.TabIndex = 0;
            this.AddTracks.Text = "Add new track(s)";
            this.AddTracks.Visible = false;
            // 
            // AnimationProperties
            // 
            this.AnimationProperties.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.AnimationProperties.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.AnimationProperties.ForeColor = System.Drawing.Color.White;
            this.AnimationProperties.Location = new System.Drawing.Point(0, 24);
            this.AnimationProperties.Name = "AnimationProperties";
            this.AnimationProperties.Size = new System.Drawing.Size(389, 468);
            this.AnimationProperties.TabIndex = 1;
            this.AnimationProperties.Text = "Properties";
            this.AnimationProperties.Visible = false;
            // 
            // FloatValuesEditor
            // 
            this.FloatValuesEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.FloatValuesEditor.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FloatValuesEditor.ForeColor = System.Drawing.Color.White;
            this.FloatValuesEditor.IsModal = false;
            this.FloatValuesEditor.Location = new System.Drawing.Point(0, 48);
            this.FloatValuesEditor.Name = "FloatValuesEditor";
            this.FloatValuesEditor.Size = new System.Drawing.Size(389, 201);
            this.FloatValuesEditor.TabIndex = 2;
            this.FloatValuesEditor.Text = "Value Editor";
            // 
            // ColorValuesEditor
            // 
            this.ColorValuesEditor.AutoSize = true;
            this.ColorValuesEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.ColorValuesEditor.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ColorValuesEditor.ForeColor = System.Drawing.Color.White;
            this.ColorValuesEditor.IsModal = false;
            this.ColorValuesEditor.Location = new System.Drawing.Point(0, 72);
            this.ColorValuesEditor.Name = "ColorValuesEditor";
            this.ColorValuesEditor.Size = new System.Drawing.Size(389, 406);
            this.ColorValuesEditor.TabIndex = 3;
            this.ColorValuesEditor.Text = "Color Editor";
            // 
            // AnimationEditorView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SplitTrack);
            this.Controls.Add(this.PanelTrackingControls);
            this.Name = "AnimationEditorView";
            this.Size = new System.Drawing.Size(1237, 784);
            this.Controls.SetChildIndex(this.PanelTrackingControls, 0);
            this.Controls.SetChildIndex(this.SplitTrack, 0);
            this.Controls.SetChildIndex(this.HostPanel, 0);
            this.Controls.SetChildIndex(this.StatusPanel, 0);
            this.Controls.SetChildIndex(this.PresentationPanel, 0);
            this.HostPanel.ResumeLayout(false);
            this.HostPanel.PerformLayout();
            this.HostPanelControls.ResumeLayout(false);
            this.HostPanelControls.PerformLayout();
            this.PanelTrackingControls.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel PanelTrackingControls;
        private System.Windows.Forms.Splitter SplitTrack;
        private AnimationTrackContainer Tracks;
        private AnimationAddTrack AddTracks;
        private AnimationProperties AnimationProperties;
        private AnimationFloatKeyEditor FloatValuesEditor;
        private AnimationColorKeyEditor ColorValuesEditor;
    }
}
