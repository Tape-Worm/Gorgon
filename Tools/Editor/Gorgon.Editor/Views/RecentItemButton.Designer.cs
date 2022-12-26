namespace Gorgon.Editor.Views;

partial class RecentItemButton
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
        this.components = new System.ComponentModel.Container();
        this.TableItem = new System.Windows.Forms.TableLayoutPanel();
        this.pictureBox1 = new System.Windows.Forms.PictureBox();
        this.LabelPath = new System.Windows.Forms.Label();
        this.LabelTitle = new System.Windows.Forms.Label();
        this.PanelFileDate = new System.Windows.Forms.Panel();
        this.LabelDateTime = new System.Windows.Forms.Label();
        this.ButtonDelete = new System.Windows.Forms.Button();
        this.TipButton = new System.Windows.Forms.ToolTip(this.components);
        this.TableItem.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
        this.PanelFileDate.SuspendLayout();
        this.SuspendLayout();
        // 
        // TableItem
        // 
        this.TableItem.AutoSize = true;
        this.TableItem.ColumnCount = 2;
        this.TableItem.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 48F));
        this.TableItem.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableItem.Controls.Add(this.pictureBox1, 0, 0);
        this.TableItem.Controls.Add(this.LabelPath, 1, 1);
        this.TableItem.Controls.Add(this.LabelTitle, 1, 0);
        this.TableItem.Cursor = System.Windows.Forms.Cursors.Hand;
        this.TableItem.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableItem.Location = new System.Drawing.Point(0, 0);
        this.TableItem.Name = "TableItem";
        this.TableItem.RowCount = 3;
        this.TableItem.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableItem.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableItem.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableItem.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableItem.Size = new System.Drawing.Size(384, 54);
        this.TableItem.TabIndex = 0;
        this.TableItem.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseDown);
        this.TableItem.MouseEnter += new System.EventHandler(this.RecentItemButton_MouseEnter);
        this.TableItem.MouseLeave += new System.EventHandler(this.RecentItemButton_MouseLeave);
        this.TableItem.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseMove);
        this.TableItem.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseUp);
        // 
        // pictureBox1
        // 
        this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
        this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pictureBox1.Image = global::Gorgon.Editor.Properties.Resources.recent_item_badge_48x48;
        this.pictureBox1.Location = new System.Drawing.Point(0, 0);
        this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
        this.pictureBox1.MinimumSize = new System.Drawing.Size(48, 48);
        this.pictureBox1.Name = "pictureBox1";
        this.TableItem.SetRowSpan(this.pictureBox1, 2);
        this.pictureBox1.Size = new System.Drawing.Size(48, 54);
        this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
        this.pictureBox1.TabIndex = 2;
        this.pictureBox1.TabStop = false;
        this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseDown);
        this.pictureBox1.MouseEnter += new System.EventHandler(this.RecentItemButton_MouseEnter);
        this.pictureBox1.MouseLeave += new System.EventHandler(this.RecentItemButton_MouseLeave);
        this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseMove);
        this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseUp);
        // 
        // LabelPath
        // 
        this.LabelPath.AutoEllipsis = true;
        this.LabelPath.AutoSize = true;
        this.LabelPath.Cursor = System.Windows.Forms.Cursors.Hand;
        this.LabelPath.Dock = System.Windows.Forms.DockStyle.Fill;
        this.LabelPath.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.LabelPath.Location = new System.Drawing.Point(51, 31);
        this.LabelPath.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
        this.LabelPath.Name = "LabelPath";
        this.LabelPath.Size = new System.Drawing.Size(330, 17);
        this.LabelPath.TabIndex = 1;
        this.LabelPath.Text = "Path to file here";
        this.LabelPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.LabelPath.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseDown);
        this.LabelPath.MouseEnter += new System.EventHandler(this.RecentItemButton_MouseEnter);
        this.LabelPath.MouseLeave += new System.EventHandler(this.RecentItemButton_MouseLeave);
        this.LabelPath.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseMove);
        this.LabelPath.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseUp);
        // 
        // LabelTitle
        // 
        this.LabelTitle.AutoSize = true;
        this.LabelTitle.Cursor = System.Windows.Forms.Cursors.Hand;
        this.LabelTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        this.LabelTitle.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.LabelTitle.Location = new System.Drawing.Point(51, 0);
        this.LabelTitle.Name = "LabelTitle";
        this.LabelTitle.Size = new System.Drawing.Size(330, 25);
        this.LabelTitle.TabIndex = 1;
        this.LabelTitle.Text = "Title goes here, or something like this.";
        this.LabelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.LabelTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseDown);
        this.LabelTitle.MouseEnter += new System.EventHandler(this.RecentItemButton_MouseEnter);
        this.LabelTitle.MouseLeave += new System.EventHandler(this.RecentItemButton_MouseLeave);
        this.LabelTitle.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseMove);
        this.LabelTitle.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseUp);
        // 
        // PanelFileDate
        // 
        this.PanelFileDate.AutoSize = true;
        this.PanelFileDate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.PanelFileDate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.PanelFileDate.Controls.Add(this.LabelDateTime);
        this.PanelFileDate.Cursor = System.Windows.Forms.Cursors.Hand;
        this.PanelFileDate.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.PanelFileDate.Location = new System.Drawing.Point(0, 54);
        this.PanelFileDate.Margin = new System.Windows.Forms.Padding(0);
        this.PanelFileDate.Name = "PanelFileDate";
        this.PanelFileDate.Size = new System.Drawing.Size(384, 28);
        this.PanelFileDate.TabIndex = 3;
        this.PanelFileDate.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseDown);
        this.PanelFileDate.MouseEnter += new System.EventHandler(this.RecentItemButton_MouseEnter);
        this.PanelFileDate.MouseLeave += new System.EventHandler(this.RecentItemButton_MouseLeave);
        this.PanelFileDate.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseMove);
        this.PanelFileDate.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseUp);
        // 
        // LabelDateTime
        // 
        this.LabelDateTime.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.LabelDateTime.AutoSize = true;
        this.LabelDateTime.Cursor = System.Windows.Forms.Cursors.Hand;
        this.LabelDateTime.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.LabelDateTime.ForeColor = System.Drawing.Color.Silver;
        this.LabelDateTime.Location = new System.Drawing.Point(268, 7);
        this.LabelDateTime.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
        this.LabelDateTime.Name = "LabelDateTime";
        this.LabelDateTime.Size = new System.Drawing.Size(113, 15);
        this.LabelDateTime.TabIndex = 2;
        this.LabelDateTime.Text = "1/1/1900 1:23:45 PM";
        this.LabelDateTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        this.LabelDateTime.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseDown);
        this.LabelDateTime.MouseEnter += new System.EventHandler(this.RecentItemButton_MouseEnter);
        this.LabelDateTime.MouseLeave += new System.EventHandler(this.RecentItemButton_MouseLeave);
        this.LabelDateTime.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseMove);
        this.LabelDateTime.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseUp);
        // 
        // ButtonDelete
        // 
        this.ButtonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonDelete.AutoSize = true;
        this.ButtonDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonDelete.Cursor = System.Windows.Forms.Cursors.Hand;
        this.ButtonDelete.FlatAppearance.BorderSize = 2;
        this.ButtonDelete.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
        this.ButtonDelete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
        this.ButtonDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonDelete.Font = new System.Drawing.Font("Marlett", 9F);
        this.ButtonDelete.Location = new System.Drawing.Point(350, 3);
        this.ButtonDelete.Name = "ButtonDelete";
        this.ButtonDelete.Size = new System.Drawing.Size(31, 26);
        this.ButtonDelete.TabIndex = 3;
        this.ButtonDelete.TabStop = false;
        this.ButtonDelete.Text = "r";
        this.TipButton.SetToolTip(this.ButtonDelete, "Delete this project");
        this.ButtonDelete.UseVisualStyleBackColor = false;
        this.ButtonDelete.Visible = false;
        this.ButtonDelete.Click += new System.EventHandler(this.ButtonDelete_Click);
        // 
        // RecentItemButton
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.AutoSize = true;
        this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.Controls.Add(this.ButtonDelete);
        this.Controls.Add(this.TableItem);
        this.Controls.Add(this.PanelFileDate);
        this.ForeColor = System.Drawing.Color.White;
        this.Name = "RecentItemButton";
        this.Size = new System.Drawing.Size(384, 82);
        this.MouseEnter += new System.EventHandler(this.RecentItemButton_MouseEnter);
        this.MouseLeave += new System.EventHandler(this.RecentItemButton_MouseLeave);
        this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RecentItemButton_MouseMove);
        this.TableItem.ResumeLayout(false);
        this.TableItem.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        this.PanelFileDate.ResumeLayout(false);
        this.PanelFileDate.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel TableItem;
    private System.Windows.Forms.Label LabelDateTime;
    private System.Windows.Forms.Label LabelPath;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.Label LabelTitle;
    private System.Windows.Forms.Panel PanelFileDate;
    private System.Windows.Forms.Button ButtonDelete;
    private System.Windows.Forms.ToolTip TipButton;
}
