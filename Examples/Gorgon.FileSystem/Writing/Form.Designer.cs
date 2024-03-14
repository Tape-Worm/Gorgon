using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Examples;

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

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        var resources = new ComponentResourceManager(typeof(Form));
        textDisplay = new TextBox();
        stripCommands = new ToolStrip();
        buttonSave = new ToolStripButton();
        buttonReload = new ToolStripDropDownButton();
        itemLoadChanged = new ToolStripMenuItem();
        itemLoadOriginal = new ToolStripMenuItem();
        toolStripSeparator1 = new ToolStripSeparator();
        toolStripLabel1 = new ToolStripLabel();
        panelWriteLocation = new Panel();
        labelWriteLocation = new Label();
        pictureBox1 = new PictureBox();
        panel3 = new Panel();
        panelFileSystem = new Panel();
        labelFileSystem = new Label();
        pictureBox3 = new PictureBox();
        panel4 = new Panel();
        labelInfo = new Label();
        pictureBox2 = new PictureBox();
        stripCommands.SuspendLayout();
        panelWriteLocation.SuspendLayout();
        ((ISupportInitialize)pictureBox1).BeginInit();
        panel3.SuspendLayout();
        panelFileSystem.SuspendLayout();
        ((ISupportInitialize)pictureBox3).BeginInit();
        panel4.SuspendLayout();
        ((ISupportInitialize)pictureBox2).BeginInit();
        SuspendLayout();
        // 
        // textDisplay
        // 
        textDisplay.BorderStyle = BorderStyle.FixedSingle;
        textDisplay.Dock = DockStyle.Fill;
        textDisplay.Location = new System.Drawing.Point(2, 29);
        textDisplay.Margin = new Padding(4, 3, 4, 3);
        textDisplay.Multiline = true;
        textDisplay.Name = "textDisplay";
        textDisplay.ScrollBars = ScrollBars.Both;
        textDisplay.Size = new System.Drawing.Size(956, 503);
        textDisplay.TabIndex = 1;
        textDisplay.TextChanged += TextDisplay_TextChanged;
        // 
        // stripCommands
        // 
        stripCommands.GripStyle = ToolStripGripStyle.Hidden;
        stripCommands.ImageScalingSize = new System.Drawing.Size(20, 20);
        stripCommands.Items.AddRange(new ToolStripItem[] { buttonSave, buttonReload, toolStripSeparator1, toolStripLabel1 });
        stripCommands.Location = new System.Drawing.Point(2, 2);
        stripCommands.Name = "stripCommands";
        stripCommands.Size = new System.Drawing.Size(956, 27);
        stripCommands.Stretch = true;
        stripCommands.TabIndex = 1;
        stripCommands.Text = "toolStrip1";
        // 
        // buttonSave
        // 
        buttonSave.Image = Properties.Resources.save_as_16x16;
        buttonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
        buttonSave.Name = "buttonSave";
        buttonSave.Size = new System.Drawing.Size(55, 24);
        buttonSave.Text = "&Save";
        buttonSave.Click += ButtonSave_Click;
        // 
        // buttonReload
        // 
        buttonReload.DropDownItems.AddRange(new ToolStripItem[] { itemLoadChanged, itemLoadOriginal });
        buttonReload.Image = Properties.Resources.reload_filesystem_16x16;
        buttonReload.ImageTransparentColor = System.Drawing.Color.Magenta;
        buttonReload.Name = "buttonReload";
        buttonReload.Size = new System.Drawing.Size(76, 24);
        buttonReload.Text = "&Reload";
        // 
        // itemLoadChanged
        // 
        itemLoadChanged.Name = "itemLoadChanged";
        itemLoadChanged.Size = new System.Drawing.Size(190, 22);
        itemLoadChanged.Text = "&Load changed version";
        itemLoadChanged.Click += ItemLoadChanged_Click;
        // 
        // itemLoadOriginal
        // 
        itemLoadOriginal.Name = "itemLoadOriginal";
        itemLoadOriginal.Size = new System.Drawing.Size(190, 22);
        itemLoadOriginal.Text = "Load &original version";
        itemLoadOriginal.Click += ItemLoadOriginal_Click;
        // 
        // toolStripSeparator1
        // 
        toolStripSeparator1.Name = "toolStripSeparator1";
        toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
        // 
        // toolStripLabel1
        // 
        toolStripLabel1.Alignment = ToolStripItemAlignment.Right;
        toolStripLabel1.Name = "toolStripLabel1";
        toolStripLabel1.Size = new System.Drawing.Size(528, 24);
        toolStripLabel1.Text = "Change the text below and press the Save button.  Press the Reload button to load the original text.";
        toolStripLabel1.ToolTipText = "Change the text below and press the Save button.  Press the Reload button to load the original text.";
        // 
        // panelWriteLocation
        // 
        panelWriteLocation.BackColor = System.Drawing.SystemColors.Info;
        panelWriteLocation.Controls.Add(labelWriteLocation);
        panelWriteLocation.Controls.Add(pictureBox1);
        panelWriteLocation.Dock = DockStyle.Bottom;
        panelWriteLocation.ForeColor = System.Drawing.SystemColors.InfoText;
        panelWriteLocation.Location = new System.Drawing.Point(0, 22);
        panelWriteLocation.Margin = new Padding(4, 3, 4, 3);
        panelWriteLocation.Name = "panelWriteLocation";
        panelWriteLocation.Size = new System.Drawing.Size(956, 22);
        panelWriteLocation.TabIndex = 2;
        // 
        // labelWriteLocation
        // 
        labelWriteLocation.Dock = DockStyle.Fill;
        labelWriteLocation.Location = new System.Drawing.Point(22, 0);
        labelWriteLocation.Name = "labelWriteLocation";
        labelWriteLocation.Size = new System.Drawing.Size(934, 22);
        labelWriteLocation.TabIndex = 0;
        labelWriteLocation.Text = "File system";
        labelWriteLocation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // pictureBox1
        // 
        pictureBox1.Dock = DockStyle.Left;
        pictureBox1.Image = Properties.Resources.file_system_16x16;
        pictureBox1.Location = new System.Drawing.Point(0, 0);
        pictureBox1.Margin = new Padding(4, 3, 4, 3);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Size = new System.Drawing.Size(22, 22);
        pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
        pictureBox1.TabIndex = 1;
        pictureBox1.TabStop = false;
        // 
        // panel3
        // 
        panel3.AutoSize = true;
        panel3.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        panel3.Controls.Add(panelFileSystem);
        panel3.Controls.Add(panelWriteLocation);
        panel3.Controls.Add(panel4);
        panel3.Dock = DockStyle.Bottom;
        panel3.Location = new System.Drawing.Point(2, 532);
        panel3.Margin = new Padding(4, 3, 4, 3);
        panel3.Name = "panel3";
        panel3.Size = new System.Drawing.Size(956, 66);
        panel3.TabIndex = 3;
        // 
        // panelFileSystem
        // 
        panelFileSystem.BackColor = System.Drawing.SystemColors.Info;
        panelFileSystem.Controls.Add(labelFileSystem);
        panelFileSystem.Controls.Add(pictureBox3);
        panelFileSystem.Dock = DockStyle.Bottom;
        panelFileSystem.ForeColor = System.Drawing.SystemColors.InfoText;
        panelFileSystem.Location = new System.Drawing.Point(0, 0);
        panelFileSystem.Margin = new Padding(4, 3, 4, 3);
        panelFileSystem.Name = "panelFileSystem";
        panelFileSystem.Size = new System.Drawing.Size(956, 22);
        panelFileSystem.TabIndex = 5;
        // 
        // labelFileSystem
        // 
        labelFileSystem.Dock = DockStyle.Fill;
        labelFileSystem.Location = new System.Drawing.Point(22, 0);
        labelFileSystem.Name = "labelFileSystem";
        labelFileSystem.Size = new System.Drawing.Size(934, 22);
        labelFileSystem.TabIndex = 0;
        labelFileSystem.Text = "File system";
        labelFileSystem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // pictureBox3
        // 
        pictureBox3.Dock = DockStyle.Left;
        pictureBox3.Image = Properties.Resources.file_system_16x16;
        pictureBox3.Location = new System.Drawing.Point(0, 0);
        pictureBox3.Margin = new Padding(4, 3, 4, 3);
        pictureBox3.Name = "pictureBox3";
        pictureBox3.Size = new System.Drawing.Size(22, 22);
        pictureBox3.SizeMode = PictureBoxSizeMode.CenterImage;
        pictureBox3.TabIndex = 1;
        pictureBox3.TabStop = false;
        // 
        // panel4
        // 
        panel4.BackColor = System.Drawing.SystemColors.Info;
        panel4.Controls.Add(labelInfo);
        panel4.Controls.Add(pictureBox2);
        panel4.Dock = DockStyle.Bottom;
        panel4.ForeColor = System.Drawing.SystemColors.InfoText;
        panel4.Location = new System.Drawing.Point(0, 44);
        panel4.Name = "panel4";
        panel4.Size = new System.Drawing.Size(956, 22);
        panel4.TabIndex = 3;
        // 
        // labelInfo
        // 
        labelInfo.Dock = DockStyle.Fill;
        labelInfo.Location = new System.Drawing.Point(22, 0);
        labelInfo.Name = "labelInfo";
        labelInfo.Size = new System.Drawing.Size(934, 22);
        labelInfo.TabIndex = 0;
        labelInfo.Text = "Info";
        labelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // pictureBox2
        // 
        pictureBox2.Dock = DockStyle.Left;
        pictureBox2.Image = Properties.Resources.Info_16x16;
        pictureBox2.Location = new System.Drawing.Point(0, 0);
        pictureBox2.Name = "pictureBox2";
        pictureBox2.Size = new System.Drawing.Size(22, 22);
        pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;
        pictureBox2.TabIndex = 1;
        pictureBox2.TabStop = false;
        // 
        // Form
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(960, 600);
        Controls.Add(textDisplay);
        Controls.Add(stripCommands);
        Controls.Add(panel3);
        Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
        Margin = new Padding(4, 3, 4, 3);
        Name = "Form";
        Padding = new Padding(2);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Gorgon Example - Writing to a Virtual File System";
        stripCommands.ResumeLayout(false);
        stripCommands.PerformLayout();
        panelWriteLocation.ResumeLayout(false);
        ((ISupportInitialize)pictureBox1).EndInit();
        panel3.ResumeLayout(false);
        panelFileSystem.ResumeLayout(false);
        ((ISupportInitialize)pictureBox3).EndInit();
        panel4.ResumeLayout(false);
        ((ISupportInitialize)pictureBox2).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }


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