namespace Gorgon.Editor.TextureAtlasTool
{
    partial class FormSpriteSelector
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSpriteSelector));
			this.TableFileSelection = new System.Windows.Forms.TableLayoutPanel();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.ButtonLoad = new System.Windows.Forms.Button();
			this.ContentFileExplorer = new Gorgon.Editor.UI.Controls.ContentFileExplorer();
			this.ButtonLabelMultiSprite = new System.Windows.Forms.Button();
			this.TableFileSelection.SuspendLayout();
			this.SuspendLayout();
			// 
			// TableFileSelection
			// 
			this.TableFileSelection.AutoSize = true;
			this.TableFileSelection.ColumnCount = 2;
			this.TableFileSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableFileSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TableFileSelection.Controls.Add(this.ButtonCancel, 1, 2);
			this.TableFileSelection.Controls.Add(this.ButtonLoad, 0, 2);
			this.TableFileSelection.Controls.Add(this.ContentFileExplorer, 0, 0);
			this.TableFileSelection.Controls.Add(this.ButtonLabelMultiSprite, 0, 1);
			this.TableFileSelection.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableFileSelection.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
			this.TableFileSelection.Location = new System.Drawing.Point(0, 0);
			this.TableFileSelection.Name = "TableFileSelection";
			this.TableFileSelection.RowCount = 3;
			this.TableFileSelection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableFileSelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableFileSelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableFileSelection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TableFileSelection.Size = new System.Drawing.Size(784, 561);
			this.TableFileSelection.TabIndex = 1;
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.ButtonCancel.AutoSize = true;
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.ButtonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
			this.ButtonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonCancel.Location = new System.Drawing.Point(675, 527);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(106, 31);
			this.ButtonCancel.TabIndex = 2;
			this.ButtonCancel.Text = "&Cancel";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
			// 
			// ButtonLoad
			// 
			this.ButtonLoad.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.ButtonLoad.AutoSize = true;
			this.ButtonLoad.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ButtonLoad.Enabled = false;
			this.ButtonLoad.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.ButtonLoad.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
			this.ButtonLoad.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.ButtonLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonLoad.Location = new System.Drawing.Point(563, 527);
			this.ButtonLoad.Name = "ButtonLoad";
			this.ButtonLoad.Size = new System.Drawing.Size(106, 31);
			this.ButtonLoad.TabIndex = 1;
			this.ButtonLoad.Text = "&Load";
			this.ButtonLoad.UseVisualStyleBackColor = true;
			this.ButtonLoad.Click += new System.EventHandler(this.ButtonLoad_Click);
			// 
			// ContentFileExplorer
			// 
			this.ContentFileExplorer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.TableFileSelection.SetColumnSpan(this.ContentFileExplorer, 2);
			this.ContentFileExplorer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContentFileExplorer.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.ContentFileExplorer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.ContentFileExplorer.Location = new System.Drawing.Point(3, 3);
			this.ContentFileExplorer.Name = "ContentFileExplorer";
			this.ContentFileExplorer.ShowSearch = true;
			this.ContentFileExplorer.Size = new System.Drawing.Size(778, 493);
			this.ContentFileExplorer.TabIndex = 0;
			this.ContentFileExplorer.Search += new System.EventHandler<Gorgon.UI.GorgonSearchEventArgs>(this.ContentFileExplorer_Search);
			// 
			// ButtonLabelMultiSprite
			// 
			this.ButtonLabelMultiSprite.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.ButtonLabelMultiSprite.AutoSize = true;
			this.ButtonLabelMultiSprite.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TableFileSelection.SetColumnSpan(this.ButtonLabelMultiSprite, 2);
			this.ButtonLabelMultiSprite.FlatAppearance.BorderSize = 0;
			this.ButtonLabelMultiSprite.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.ButtonLabelMultiSprite.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.ButtonLabelMultiSprite.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonLabelMultiSprite.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
			this.ButtonLabelMultiSprite.Image = global::Gorgon.Editor.TextureAtlasTool.Properties.Resources.info_16x16;
			this.ButtonLabelMultiSprite.Location = new System.Drawing.Point(558, 499);
			this.ButtonLabelMultiSprite.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.ButtonLabelMultiSprite.Name = "ButtonLabelMultiSprite";
			this.ButtonLabelMultiSprite.Size = new System.Drawing.Size(223, 25);
			this.ButtonLabelMultiSprite.TabIndex = 8;
			this.ButtonLabelMultiSprite.TabStop = false;
			this.ButtonLabelMultiSprite.Text = "More than 1 sprite must be selected.";
			this.ButtonLabelMultiSprite.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.ButtonLabelMultiSprite.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.ButtonLabelMultiSprite.UseMnemonic = false;
			this.ButtonLabelMultiSprite.UseVisualStyleBackColor = true;
			// 
			// FormSpriteSelector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(784, 561);
			this.Controls.Add(this.TableFileSelection);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(800, 600);
			this.Name = "FormSpriteSelector";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Load Sprites";
			this.TableFileSelection.ResumeLayout(false);
			this.TableFileSelection.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel TableFileSelection;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button ButtonLoad;
        private UI.Controls.ContentFileExplorer ContentFileExplorer;
        private System.Windows.Forms.Button ButtonLabelMultiSprite;
    }
}