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
        ComponentResourceManager resources = new ComponentResourceManager(typeof(Form));
        PanelDisplay = new Panel();
        TableHelp = new TableLayoutPanel();
        Label18 = new Label();
        Label17 = new Label();
        label2 = new Label();
        label3 = new Label();
        label4 = new Label();
        label5 = new Label();
        label6 = new Label();
        label7 = new Label();
        label8 = new Label();
        label9 = new Label();
        label10 = new Label();
        label11 = new Label();
        label12 = new Label();
        label13 = new Label();
        label14 = new Label();
        label15 = new Label();
        Label16 = new Label();
        panel1 = new Panel();
        LabelMouse = new Label();
        pictureBox3 = new PictureBox();
        LabelKeyboard = new Label();
        pictureBox1 = new PictureBox();
        LabelGamingDevice = new Label();
        pictureBox2 = new PictureBox();
        TableJoysticks = new TableLayoutPanel();
        FlowJoystick = new FlowLayoutPanel();
        label1 = new Label();
        ListJoysticks = new ListBox();
        FlowDevices = new FlowLayoutPanel();
        FlowMouse = new FlowLayoutPanel();
        FlowKeyboard = new FlowLayoutPanel();
        FlowGamingDevices = new FlowLayoutPanel();
        pictureBox4 = new PictureBox();
        TableMain = new TableLayoutPanel();
        PanelDisplay.SuspendLayout();
        TableHelp.SuspendLayout();
        ((ISupportInitialize)pictureBox3).BeginInit();
        ((ISupportInitialize)pictureBox1).BeginInit();
        ((ISupportInitialize)pictureBox2).BeginInit();
        TableJoysticks.SuspendLayout();
        FlowJoystick.SuspendLayout();
        FlowDevices.SuspendLayout();
        FlowMouse.SuspendLayout();
        FlowKeyboard.SuspendLayout();
        FlowGamingDevices.SuspendLayout();
        ((ISupportInitialize)pictureBox4).BeginInit();
        TableMain.SuspendLayout();
        SuspendLayout();
        // 
        // PanelDisplay
        // 
        PanelDisplay.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        PanelDisplay.BackColor = Color.White;
        PanelDisplay.BorderStyle = BorderStyle.FixedSingle;
        PanelDisplay.Controls.Add(TableHelp);
        PanelDisplay.Location = new Point(6, 6);
        PanelDisplay.Margin = new Padding(6);
        PanelDisplay.Name = "PanelDisplay";
        PanelDisplay.Padding = new Padding(3);
        PanelDisplay.Size = new Size(1002, 628);
        PanelDisplay.TabIndex = 1;
        // 
        // TableHelp
        // 
        TableHelp.AutoSize = true;
        TableHelp.BackColor = Color.FromArgb(32, 32, 32);
        TableHelp.ColumnCount = 3;
        TableHelp.ColumnStyles.Add(new ColumnStyle());
        TableHelp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 2F));
        TableHelp.ColumnStyles.Add(new ColumnStyle());
        TableHelp.Controls.Add(Label18, 2, 1);
        TableHelp.Controls.Add(Label17, 0, 1);
        TableHelp.Controls.Add(label2, 0, 2);
        TableHelp.Controls.Add(label3, 2, 2);
        TableHelp.Controls.Add(label4, 0, 3);
        TableHelp.Controls.Add(label5, 2, 3);
        TableHelp.Controls.Add(label6, 0, 4);
        TableHelp.Controls.Add(label7, 2, 4);
        TableHelp.Controls.Add(label8, 0, 5);
        TableHelp.Controls.Add(label9, 2, 5);
        TableHelp.Controls.Add(label10, 0, 6);
        TableHelp.Controls.Add(label11, 2, 6);
        TableHelp.Controls.Add(label12, 0, 7);
        TableHelp.Controls.Add(label13, 2, 7);
        TableHelp.Controls.Add(label14, 0, 8);
        TableHelp.Controls.Add(label15, 2, 8);
        TableHelp.Controls.Add(Label16, 0, 0);
        TableHelp.Controls.Add(panel1, 1, 1);
        TableHelp.Dock = DockStyle.Top;
        TableHelp.Location = new Point(3, 3);
        TableHelp.Name = "TableHelp";
        TableHelp.RowCount = 9;
        TableHelp.RowStyles.Add(new RowStyle());
        TableHelp.RowStyles.Add(new RowStyle());
        TableHelp.RowStyles.Add(new RowStyle());
        TableHelp.RowStyles.Add(new RowStyle());
        TableHelp.RowStyles.Add(new RowStyle());
        TableHelp.RowStyles.Add(new RowStyle());
        TableHelp.RowStyles.Add(new RowStyle());
        TableHelp.RowStyles.Add(new RowStyle());
        TableHelp.RowStyles.Add(new RowStyle());
        TableHelp.Size = new Size(994, 348);
        TableHelp.TabIndex = 1;
        TableHelp.Visible = false;
        // 
        // Label18
        // 
        Label18.Anchor = AnchorStyles.Left;
        Label18.AutoSize = true;
        Label18.Location = new Point(254, 30);
        Label18.Margin = new Padding(4);
        Label18.Name = "Label18";
        Label18.Size = new Size(77, 15);
        Label18.TabIndex = 2;
        Label18.Text = "Exit program.";
        // 
        // Label17
        // 
        Label17.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        Label17.AutoSize = true;
        Label17.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Italic);
        Label17.Location = new Point(4, 27);
        Label17.Margin = new Padding(4);
        Label17.Name = "Label17";
        Label17.Padding = new Padding(3);
        Label17.Size = new Size(240, 21);
        Label17.TabIndex = 0;
        Label17.Text = "ESC";
        Label17.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // label2
        // 
        label2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        label2.AutoSize = true;
        label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Italic);
        label2.Location = new Point(4, 56);
        label2.Margin = new Padding(4);
        label2.Name = "label2";
        label2.Size = new Size(240, 60);
        label2.TabIndex = 3;
        label2.Text = "Mouse Button\r\nJoystick/Gamepad Button\r\n1-8 (or Left CTRL + 1-8)";
        label2.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // label3
        // 
        label3.Anchor = AnchorStyles.Left;
        label3.AutoSize = true;
        label3.Location = new Point(254, 56);
        label3.Margin = new Padding(4);
        label3.Name = "label3";
        label3.Size = new Size(471, 60);
        label3.TabIndex = 4;
        label3.Text = "Paint with various colors.\r\n\r\nColors: Black, Blue, Green ,Cyan, Red, Purple, Brown, 50% Gray \r\n30% Gray, Bright Blue, Bright Green, Bright Cyan, Bright Red, Bright Purple, Bright Yellow";
        label3.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // label4
        // 
        label4.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        label4.AutoSize = true;
        label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Italic);
        label4.Location = new Point(4, 124);
        label4.Margin = new Padding(4);
        label4.Name = "label4";
        label4.Size = new Size(240, 45);
        label4.TabIndex = 5;
        label4.Text = "+. -\r\nMouse Wheel Up/Down\r\nPOV Hat Up/Down\r\n";
        label4.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // label5
        // 
        label5.Anchor = AnchorStyles.Left;
        label5.AutoSize = true;
        label5.Location = new Point(254, 139);
        label5.Margin = new Padding(4);
        label5.Name = "label5";
        label5.Size = new Size(171, 15);
        label5.TabIndex = 6;
        label5.Text = "Increase or decrease brush size.";
        label5.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // label6
        // 
        label6.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        label6.AutoSize = true;
        label6.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Italic);
        label6.Location = new Point(4, 177);
        label6.Margin = new Padding(4);
        label6.Name = "label6";
        label6.Size = new Size(240, 15);
        label6.TabIndex = 7;
        label6.Text = "R";
        label6.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // label7
        // 
        label7.Anchor = AnchorStyles.Left;
        label7.AutoSize = true;
        label7.Location = new Point(254, 177);
        label7.Margin = new Padding(4);
        label7.Name = "label7";
        label7.Size = new Size(118, 15);
        label7.TabIndex = 8;
        label7.Text = "Random color brush.";
        label7.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // label8
        // 
        label8.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        label8.AutoSize = true;
        label8.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Italic);
        label8.Location = new Point(4, 200);
        label8.Margin = new Padding(4);
        label8.Name = "label8";
        label8.Size = new Size(240, 15);
        label8.TabIndex = 9;
        label8.Text = "C\r\n";
        label8.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // label9
        // 
        label9.Anchor = AnchorStyles.Left;
        label9.AutoSize = true;
        label9.Location = new Point(254, 200);
        label9.Margin = new Padding(4);
        label9.Name = "label9";
        label9.Size = new Size(73, 15);
        label9.TabIndex = 10;
        label9.Text = "Clear image.";
        label9.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // label10
        // 
        label10.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        label10.AutoSize = true;
        label10.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Italic);
        label10.Location = new Point(4, 223);
        label10.Margin = new Padding(4);
        label10.Name = "label10";
        label10.Size = new Size(240, 60);
        label10.TabIndex = 11;
        label10.Text = "Horizontal Mouse Scroll Wheel Left/Right\r\nThrottle Up/Down (Joystick)\r\nLeft Trigger Up/Down (Gamepad)\r\nPage Up/Down\r\n";
        label10.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // label11
        // 
        label11.Anchor = AnchorStyles.Left;
        label11.AutoSize = true;
        label11.Location = new Point(254, 245);
        label11.Margin = new Padding(4);
        label11.Name = "label11";
        label11.Size = new Size(226, 15);
        label11.TabIndex = 12;
        label11.Text = "Increase or decrease alpha value of brush.";
        label11.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // label12
        // 
        label12.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        label12.AutoSize = true;
        label12.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Italic);
        label12.Location = new Point(4, 291);
        label12.Margin = new Padding(4);
        label12.Name = "label12";
        label12.Size = new Size(240, 30);
        label12.TabIndex = 13;
        label12.Text = "END";
        label12.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // label13
        // 
        label13.Anchor = AnchorStyles.Left;
        label13.AutoSize = true;
        label13.Location = new Point(254, 291);
        label13.Margin = new Padding(4);
        label13.Name = "label13";
        label13.Size = new Size(283, 30);
        label13.TabIndex = 14;
        label13.Text = "Disable input system. \r\nSwitch away from the app and back again to restore.";
        label13.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // label14
        // 
        label14.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        label14.AutoSize = true;
        label14.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Italic);
        label14.Location = new Point(4, 329);
        label14.Margin = new Padding(4);
        label14.Name = "label14";
        label14.Size = new Size(240, 15);
        label14.TabIndex = 15;
        label14.Text = "F1";
        label14.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // label15
        // 
        label15.AutoSize = true;
        label15.Location = new Point(254, 329);
        label15.Margin = new Padding(4);
        label15.Name = "label15";
        label15.Size = new Size(124, 15);
        label15.TabIndex = 16;
        label15.Text = "Close this help screen.";
        label15.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // Label16
        // 
        Label16.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        Label16.AutoSize = true;
        Label16.BackColor = Color.FromArgb(45, 45, 48);
        TableHelp.SetColumnSpan(Label16, 3);
        Label16.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
        Label16.Location = new Point(4, 0);
        Label16.Margin = new Padding(4, 0, 0, 0);
        Label16.Name = "Label16";
        Label16.Padding = new Padding(0, 4, 0, 4);
        Label16.Size = new Size(990, 23);
        Label16.TabIndex = 17;
        Label16.Text = "Help";
        // 
        // panel1
        // 
        panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        panel1.BackColor = Color.FromArgb(45, 45, 48);
        panel1.Location = new Point(248, 23);
        panel1.Margin = new Padding(0);
        panel1.Name = "panel1";
        TableHelp.SetRowSpan(panel1, 8);
        panel1.Size = new Size(2, 325);
        panel1.TabIndex = 18;
        // 
        // LabelMouse
        // 
        LabelMouse.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        LabelMouse.AutoSize = true;
        LabelMouse.Location = new Point(27, 0);
        LabelMouse.Name = "LabelMouse";
        LabelMouse.Size = new Size(43, 24);
        LabelMouse.TabIndex = 0;
        LabelMouse.Text = "Mouse";
        LabelMouse.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // pictureBox3
        // 
        pictureBox3.BackColor = Color.FromArgb(47, 47, 48);
        pictureBox3.Image = Properties.Resources.device_mouse_16x16;
        pictureBox3.Location = new Point(0, 0);
        pictureBox3.Margin = new Padding(0);
        pictureBox3.Name = "pictureBox3";
        pictureBox3.Size = new Size(24, 24);
        pictureBox3.SizeMode = PictureBoxSizeMode.CenterImage;
        pictureBox3.TabIndex = 1;
        pictureBox3.TabStop = false;
        // 
        // LabelKeyboard
        // 
        LabelKeyboard.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        LabelKeyboard.AutoSize = true;
        LabelKeyboard.Location = new Point(27, 0);
        LabelKeyboard.Name = "LabelKeyboard";
        LabelKeyboard.Size = new Size(57, 24);
        LabelKeyboard.TabIndex = 0;
        LabelKeyboard.Text = "Keyboard";
        LabelKeyboard.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // pictureBox1
        // 
        pictureBox1.Anchor = AnchorStyles.Left;
        pictureBox1.BackColor = Color.FromArgb(47, 47, 48);
        pictureBox1.Image = Properties.Resources.device_keyboard_16x16;
        pictureBox1.Location = new Point(0, 0);
        pictureBox1.Margin = new Padding(0);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Size = new Size(24, 24);
        pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
        pictureBox1.TabIndex = 1;
        pictureBox1.TabStop = false;
        // 
        // LabelGamingDevice
        // 
        LabelGamingDevice.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        LabelGamingDevice.AutoSize = true;
        LabelGamingDevice.Location = new Point(27, 0);
        LabelGamingDevice.Name = "LabelGamingDevice";
        LabelGamingDevice.Size = new Size(87, 24);
        LabelGamingDevice.TabIndex = 2;
        LabelGamingDevice.Text = "Gaming Device";
        LabelGamingDevice.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // pictureBox2
        // 
        pictureBox2.Anchor = AnchorStyles.Left;
        pictureBox2.Image = Properties.Resources.device_gamepad_16x16;
        pictureBox2.Location = new Point(0, 0);
        pictureBox2.Margin = new Padding(0);
        pictureBox2.Name = "pictureBox2";
        pictureBox2.Size = new Size(24, 24);
        pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;
        pictureBox2.TabIndex = 3;
        pictureBox2.TabStop = false;
        // 
        // TableJoysticks
        // 
        TableJoysticks.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        TableJoysticks.AutoSize = true;
        TableJoysticks.ColumnCount = 1;
        TableJoysticks.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        TableJoysticks.Controls.Add(FlowJoystick, 0, 0);
        TableJoysticks.Controls.Add(ListJoysticks, 0, 1);
        TableJoysticks.Location = new Point(1014, 8);
        TableJoysticks.Margin = new Padding(0, 8, 8, 0);
        TableJoysticks.Name = "TableJoysticks";
        TableJoysticks.RowCount = 2;
        TableJoysticks.RowStyles.Add(new RowStyle());
        TableJoysticks.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        TableJoysticks.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        TableJoysticks.Size = new Size(258, 632);
        TableJoysticks.TabIndex = 0;
        // 
        // FlowJoystick
        // 
        FlowJoystick.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        FlowJoystick.AutoSize = true;
        FlowJoystick.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        FlowJoystick.BackColor = Color.FromArgb(45, 45, 48);
        FlowJoystick.Controls.Add(pictureBox2);
        FlowJoystick.Controls.Add(label1);
        FlowJoystick.Location = new Point(0, 0);
        FlowJoystick.Margin = new Padding(0, 0, 0, 3);
        FlowJoystick.Name = "FlowJoystick";
        FlowJoystick.Size = new Size(258, 24);
        FlowJoystick.TabIndex = 1;
        // 
        // label1
        // 
        label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        label1.AutoSize = true;
        label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
        label1.Location = new Point(24, 0);
        label1.Margin = new Padding(0);
        label1.Name = "label1";
        label1.Size = new Size(97, 24);
        label1.TabIndex = 4;
        label1.Text = "Gaming Devices";
        label1.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // ListJoysticks
        // 
        ListJoysticks.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        ListJoysticks.BackColor = Color.FromArgb(30, 30, 30);
        ListJoysticks.BorderStyle = BorderStyle.None;
        ListJoysticks.ForeColor = Color.White;
        ListJoysticks.FormattingEnabled = true;
        ListJoysticks.IntegralHeight = false;
        ListJoysticks.ItemHeight = 15;
        ListJoysticks.Location = new Point(0, 27);
        ListJoysticks.Margin = new Padding(0, 0, 0, 8);
        ListJoysticks.Name = "ListJoysticks";
        ListJoysticks.SelectionMode = SelectionMode.None;
        ListJoysticks.Size = new Size(258, 597);
        ListJoysticks.TabIndex = 1;
        ListJoysticks.TabStop = false;
        // 
        // FlowDevices
        // 
        FlowDevices.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        FlowDevices.AutoSize = true;
        FlowDevices.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        TableMain.SetColumnSpan(FlowDevices, 2);
        FlowDevices.Controls.Add(FlowMouse);
        FlowDevices.Controls.Add(FlowKeyboard);
        FlowDevices.Controls.Add(FlowGamingDevices);
        FlowDevices.FlowDirection = FlowDirection.TopDown;
        FlowDevices.Location = new Point(8, 640);
        FlowDevices.Margin = new Padding(8, 0, 8, 0);
        FlowDevices.Name = "FlowDevices";
        FlowDevices.Size = new Size(1264, 80);
        FlowDevices.TabIndex = 0;
        FlowDevices.WrapContents = false;
        // 
        // FlowMouse
        // 
        FlowMouse.AutoSize = true;
        FlowMouse.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        FlowMouse.Controls.Add(pictureBox3);
        FlowMouse.Controls.Add(LabelMouse);
        FlowMouse.Location = new Point(0, 0);
        FlowMouse.Margin = new Padding(0);
        FlowMouse.Name = "FlowMouse";
        FlowMouse.Size = new Size(73, 24);
        FlowMouse.TabIndex = 0;
        // 
        // FlowKeyboard
        // 
        FlowKeyboard.AutoSize = true;
        FlowKeyboard.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        FlowKeyboard.Controls.Add(pictureBox1);
        FlowKeyboard.Controls.Add(LabelKeyboard);
        FlowKeyboard.Location = new Point(0, 28);
        FlowKeyboard.Margin = new Padding(0, 4, 0, 4);
        FlowKeyboard.Name = "FlowKeyboard";
        FlowKeyboard.Size = new Size(87, 24);
        FlowKeyboard.TabIndex = 0;
        FlowKeyboard.WrapContents = false;
        // 
        // FlowGamingDevices
        // 
        FlowGamingDevices.AutoSize = true;
        FlowGamingDevices.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        FlowGamingDevices.Controls.Add(pictureBox4);
        FlowGamingDevices.Controls.Add(LabelGamingDevice);
        FlowGamingDevices.Location = new Point(0, 56);
        FlowGamingDevices.Margin = new Padding(0);
        FlowGamingDevices.Name = "FlowGamingDevices";
        FlowGamingDevices.Size = new Size(117, 24);
        FlowGamingDevices.TabIndex = 1;
        // 
        // pictureBox4
        // 
        pictureBox4.BackColor = Color.FromArgb(47, 47, 48);
        pictureBox4.Image = Properties.Resources.device_gamepad_16x16;
        pictureBox4.Location = new Point(0, 0);
        pictureBox4.Margin = new Padding(0);
        pictureBox4.Name = "pictureBox4";
        pictureBox4.Size = new Size(24, 24);
        pictureBox4.SizeMode = PictureBoxSizeMode.CenterImage;
        pictureBox4.TabIndex = 1;
        pictureBox4.TabStop = false;
        // 
        // TableMain
        // 
        TableMain.ColumnCount = 2;
        TableMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        TableMain.ColumnStyles.Add(new ColumnStyle());
        TableMain.Controls.Add(PanelDisplay, 0, 0);
        TableMain.Controls.Add(TableJoysticks, 1, 0);
        TableMain.Controls.Add(FlowDevices, 0, 1);
        TableMain.Dock = DockStyle.Fill;
        TableMain.Location = new Point(0, 0);
        TableMain.Margin = new Padding(0);
        TableMain.Name = "TableMain";
        TableMain.RowCount = 2;
        TableMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        TableMain.RowStyles.Add(new RowStyle());
        TableMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        TableMain.Size = new Size(1280, 720);
        TableMain.TabIndex = 0;
        // 
        // Form
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 30);
        ClientSize = new Size(1280, 720);
        Controls.Add(TableMain);
        ForeColor = Color.White;
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "Form";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Gorgon Example - Input Devices";
        Load += Form_Load;
        PanelDisplay.ResumeLayout(false);
        PanelDisplay.PerformLayout();
        TableHelp.ResumeLayout(false);
        TableHelp.PerformLayout();
        ((ISupportInitialize)pictureBox3).EndInit();
        ((ISupportInitialize)pictureBox1).EndInit();
        ((ISupportInitialize)pictureBox2).EndInit();
        TableJoysticks.ResumeLayout(false);
        TableJoysticks.PerformLayout();
        FlowJoystick.ResumeLayout(false);
        FlowJoystick.PerformLayout();
        FlowDevices.ResumeLayout(false);
        FlowDevices.PerformLayout();
        FlowMouse.ResumeLayout(false);
        FlowMouse.PerformLayout();
        FlowKeyboard.ResumeLayout(false);
        FlowKeyboard.PerformLayout();
        FlowGamingDevices.ResumeLayout(false);
        FlowGamingDevices.PerformLayout();
        ((ISupportInitialize)pictureBox4).EndInit();
        TableMain.ResumeLayout(false);
        TableMain.PerformLayout();
        ResumeLayout(false);
    }

    private Panel PanelDisplay;
    private Label LabelKeyboard;
    private PictureBox pictureBox1;
    private Label LabelGamingDevice;
    private PictureBox pictureBox2;
    private Label LabelMouse;
    private PictureBox pictureBox3;
    private TableLayoutPanel TableJoysticks;
    private ListBox ListJoysticks;
    private FlowLayoutPanel FlowDevices;
    private FlowLayoutPanel FlowMouse;
    private FlowLayoutPanel FlowKeyboard;
    private FlowLayoutPanel FlowJoystick;
    private TableLayoutPanel TableMain;
    private FlowLayoutPanel FlowGamingDevices;
    private PictureBox pictureBox4;
    private Label label1;
    private Label Label17;
    private TableLayoutPanel TableHelp;
    private Label Label18;
    private Label label2;
    private Label label3;
    private Label label4;
    private Label label5;
    private Label label6;
    private Label label7;
    private Label label8;
    private Label label9;
    private Label label10;
    private Label label11;
    private Label label12;
    private Label label13;
    private Label label14;
    private Label label15;
    private Label Label16;
    private Panel panel1;
}