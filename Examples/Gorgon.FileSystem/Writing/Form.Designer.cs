using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Examples
{
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form));
            this.imageTree = new System.Windows.Forms.ImageList(this.components);
            this.textDisplay = new System.Windows.Forms.TextBox();
            this.stripCommands = new System.Windows.Forms.ToolStrip();
            this.buttonSave = new System.Windows.Forms.ToolStripButton();
            this.buttonReload = new System.Windows.Forms.ToolStripDropDownButton();
            this.itemLoadChanged = new System.Windows.Forms.ToolStripMenuItem();
            this.itemLoadOriginal = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.panelWriteLocation = new System.Windows.Forms.Panel();
            this.labelWriteLocation = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panelFileSystem = new System.Windows.Forms.Panel();
            this.labelFileSystem = new System.Windows.Forms.Label();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.labelInfo = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.stripCommands.SuspendLayout();
            this.panelWriteLocation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel3.SuspendLayout();
            this.panelFileSystem.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // imageTree
            // 
            this.imageTree.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageTree.ImageStream")));
            this.imageTree.TransparentColor = System.Drawing.Color.Transparent;
            this.imageTree.Images.SetKeyName(0, "folder_16x16.png");
            this.imageTree.Images.SetKeyName(1, "document_text_16x16.png");
            this.imageTree.Images.SetKeyName(2, "packedfile_16x16.png");
            // 
            // textDisplay
            // 
            this.textDisplay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textDisplay.Location = new System.Drawing.Point(2, 29);
            this.textDisplay.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textDisplay.Multiline = true;
            this.textDisplay.Name = "textDisplay";
            this.textDisplay.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textDisplay.Size = new System.Drawing.Size(956, 503);
            this.textDisplay.TabIndex = 1;
            this.textDisplay.TextChanged += new System.EventHandler(this.TextDisplay_TextChanged);
            // 
            // stripCommands
            // 
            this.stripCommands.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.stripCommands.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.stripCommands.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.stripCommands.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonSave,
            this.buttonReload,
            this.toolStripSeparator1,
            this.toolStripLabel1});
            this.stripCommands.Location = new System.Drawing.Point(2, 2);
            this.stripCommands.Name = "stripCommands";
            this.stripCommands.Size = new System.Drawing.Size(956, 27);
            this.stripCommands.Stretch = true;
            this.stripCommands.TabIndex = 1;
            this.stripCommands.Text = "toolStrip1";
            // 
            // buttonSave
            // 
            this.buttonSave.Image = global::Gorgon.Examples.Properties.Resources.save_as_16x16;
            this.buttonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(55, 24);
            this.buttonSave.Text = "&Save";
            this.buttonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // buttonReload
            // 
            this.buttonReload.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemLoadChanged,
            this.itemLoadOriginal});
            this.buttonReload.Image = global::Gorgon.Examples.Properties.Resources.reload_filesystem_16x16;
            this.buttonReload.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonReload.Name = "buttonReload";
            this.buttonReload.Size = new System.Drawing.Size(76, 24);
            this.buttonReload.Text = "&Reload";
            // 
            // itemLoadChanged
            // 
            this.itemLoadChanged.Name = "itemLoadChanged";
            this.itemLoadChanged.Size = new System.Drawing.Size(190, 22);
            this.itemLoadChanged.Text = "&Load changed version";
            this.itemLoadChanged.Click += new System.EventHandler(this.ItemLoadChanged_Click);
            // 
            // itemLoadOriginal
            // 
            this.itemLoadOriginal.Name = "itemLoadOriginal";
            this.itemLoadOriginal.Size = new System.Drawing.Size(190, 22);
            this.itemLoadOriginal.Text = "Load &original version";
            this.itemLoadOriginal.Click += new System.EventHandler(this.ItemLoadOriginal_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(528, 24);
            this.toolStripLabel1.Text = "Change the text below and press the Save button.  Press the Reload button to load" +
    " the original text.";
            this.toolStripLabel1.ToolTipText = "Change the text below and press the Save button.  Press the Reload button to load" +
    " the original text.";
            // 
            // panelWriteLocation
            // 
            this.panelWriteLocation.BackColor = System.Drawing.SystemColors.Info;
            this.panelWriteLocation.Controls.Add(this.labelWriteLocation);
            this.panelWriteLocation.Controls.Add(this.pictureBox1);
            this.panelWriteLocation.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelWriteLocation.ForeColor = System.Drawing.SystemColors.InfoText;
            this.panelWriteLocation.Location = new System.Drawing.Point(0, 22);
            this.panelWriteLocation.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panelWriteLocation.Name = "panelWriteLocation";
            this.panelWriteLocation.Size = new System.Drawing.Size(956, 22);
            this.panelWriteLocation.TabIndex = 2;
            // 
            // labelWriteLocation
            // 
            this.labelWriteLocation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelWriteLocation.Location = new System.Drawing.Point(22, 0);
            this.labelWriteLocation.Name = "labelWriteLocation";
            this.labelWriteLocation.Size = new System.Drawing.Size(934, 22);
            this.labelWriteLocation.TabIndex = 0;
            this.labelWriteLocation.Text = "File system";
            this.labelWriteLocation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.pictureBox1.Image = global::Gorgon.Examples.Properties.Resources.file_system_16x16;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(22, 22);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // panel3
            // 
            this.panel3.AutoSize = true;
            this.panel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel3.Controls.Add(this.panelFileSystem);
            this.panel3.Controls.Add(this.panelWriteLocation);
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(2, 532);
            this.panel3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(956, 66);
            this.panel3.TabIndex = 3;
            // 
            // panelFileSystem
            // 
            this.panelFileSystem.BackColor = System.Drawing.SystemColors.Info;
            this.panelFileSystem.Controls.Add(this.labelFileSystem);
            this.panelFileSystem.Controls.Add(this.pictureBox3);
            this.panelFileSystem.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFileSystem.ForeColor = System.Drawing.SystemColors.InfoText;
            this.panelFileSystem.Location = new System.Drawing.Point(0, 0);
            this.panelFileSystem.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panelFileSystem.Name = "panelFileSystem";
            this.panelFileSystem.Size = new System.Drawing.Size(956, 22);
            this.panelFileSystem.TabIndex = 5;
            // 
            // labelFileSystem
            // 
            this.labelFileSystem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelFileSystem.Location = new System.Drawing.Point(22, 0);
            this.labelFileSystem.Name = "labelFileSystem";
            this.labelFileSystem.Size = new System.Drawing.Size(934, 22);
            this.labelFileSystem.TabIndex = 0;
            this.labelFileSystem.Text = "File system";
            this.labelFileSystem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Dock = System.Windows.Forms.DockStyle.Left;
            this.pictureBox3.Image = global::Gorgon.Examples.Properties.Resources.file_system_16x16;
            this.pictureBox3.Location = new System.Drawing.Point(0, 0);
            this.pictureBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(22, 22);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox3.TabIndex = 1;
            this.pictureBox3.TabStop = false;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.SystemColors.Info;
            this.panel4.Controls.Add(this.labelInfo);
            this.panel4.Controls.Add(this.pictureBox2);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.ForeColor = System.Drawing.SystemColors.InfoText;
            this.panel4.Location = new System.Drawing.Point(0, 44);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(956, 22);
            this.panel4.TabIndex = 3;
            // 
            // labelInfo
            // 
            this.labelInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelInfo.Location = new System.Drawing.Point(22, 0);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(934, 22);
            this.labelInfo.TabIndex = 0;
            this.labelInfo.Text = "Info";
            this.labelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Left;
            this.pictureBox2.Image = global::Gorgon.Examples.Properties.Resources.Info_16x16;
            this.pictureBox2.Location = new System.Drawing.Point(0, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(22, 22);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            // 
            // Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 600);
            this.Controls.Add(this.textDisplay);
            this.Controls.Add(this.stripCommands);
            this.Controls.Add(this.panel3);
            this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "Form";
            this.Padding = new System.Windows.Forms.Padding(2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Gorgon Example - Writing to a Virtual File System";
            this.stripCommands.ResumeLayout(false);
            this.stripCommands.PerformLayout();
            this.panelWriteLocation.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panelFileSystem.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.panel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private ImageList imageTree;
		private TextBox textDisplay;
		private Panel panelWriteLocation;
		private Label labelWriteLocation;
		private Panel panel3;
		private PictureBox pictureBox1;
		private ToolStrip stripCommands;
		private ToolStripButton buttonSave;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripLabel toolStripLabel1;
		private ToolStripDropDownButton buttonReload;
		private ToolStripMenuItem itemLoadChanged;
		private ToolStripMenuItem itemLoadOriginal;
		private Panel panel4;
		private PictureBox pictureBox2;
		private Label labelInfo;
		private Panel panelFileSystem;
		private Label labelFileSystem;
		private PictureBox pictureBox3;
	}
}

