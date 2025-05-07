namespace Gorgon.UI.WindowsForms;

partial class FormDialog
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDialog));
        PictureIcon = new PictureBox();
        CheckDetail = new CheckBox();
        TextDetails = new TextBox();
        ButtonOK = new Button();
        CheckToAll = new CheckBox();
        ButtonNo = new Button();
        ButtonCancel = new Button();
        ButtonYes = new Button();
        LabelMessage = new Label();
        PanelMessage = new Panel();
        PanelDetails = new Panel();
        PanelButtonsBorder = new Panel();
        TableButtons = new TableLayoutPanel();
        ((System.ComponentModel.ISupportInitialize)PictureIcon).BeginInit();
        PanelMessage.SuspendLayout();
        PanelDetails.SuspendLayout();
        PanelButtonsBorder.SuspendLayout();
        TableButtons.SuspendLayout();
        SuspendLayout();
        // 
        // PictureIcon
        // 
        PictureIcon.Image = Properties.Resources.Info_48x48;
        PictureIcon.Location = new Point(0, 0);
        PictureIcon.Name = "PictureIcon";
        PictureIcon.Size = new Size(48, 48);
        PictureIcon.SizeMode = PictureBoxSizeMode.Zoom;
        PictureIcon.TabIndex = 0;
        PictureIcon.TabStop = false;
        // 
        // CheckDetail
        // 
        CheckDetail.Appearance = Appearance.Button;
        CheckDetail.FlatAppearance.BorderColor = Color.Black;
        CheckDetail.FlatAppearance.BorderSize = 2;
        CheckDetail.FlatAppearance.CheckedBackColor = Color.FromArgb(224, 224, 224);
        CheckDetail.FlatAppearance.MouseDownBackColor = Color.FromArgb(224, 224, 224);
        CheckDetail.FlatAppearance.MouseOverBackColor = Color.FromArgb(224, 224, 224);
        CheckDetail.ImageAlign = ContentAlignment.MiddleLeft;
        CheckDetail.Location = new Point(68, 3);
        CheckDetail.Name = "CheckDetail";
        CheckDetail.Size = new Size(69, 25);
        CheckDetail.TabIndex = 4;
        CheckDetail.Text = "&Details";
        CheckDetail.TextAlign = ContentAlignment.MiddleCenter;
        CheckDetail.Visible = false;
        CheckDetail.Click += CheckDetail_Click;
        // 
        // TextDetails
        // 
        TextDetails.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        TextDetails.BackColor = Color.FromArgb(224, 224, 224);
        TextDetails.BorderStyle = BorderStyle.FixedSingle;
        TextDetails.Font = new Font("Consolas", 9F);
        TextDetails.Location = new Point(0, 0);
        TextDetails.MaxLength = 2097152;
        TextDetails.Multiline = true;
        TextDetails.Name = "TextDetails";
        TextDetails.ReadOnly = true;
        TextDetails.ScrollBars = ScrollBars.Both;
        TextDetails.Size = new Size(1382, 197);
        TextDetails.TabIndex = 5;
        TextDetails.WordWrap = false;
        // 
        // ButtonOK
        // 
        ButtonOK.DialogResult = DialogResult.OK;
        ButtonOK.FlatAppearance.BorderColor = Color.Black;
        ButtonOK.FlatAppearance.MouseDownBackColor = Color.Silver;
        ButtonOK.FlatAppearance.MouseOverBackColor = Color.Silver;
        ButtonOK.ImageAlign = ContentAlignment.MiddleLeft;
        ButtonOK.Location = new Point(1235, 3);
        ButtonOK.Name = "ButtonOK";
        ButtonOK.Size = new Size(69, 25);
        ButtonOK.TabIndex = 0;
        ButtonOK.Text = "&OK";
        // 
        // CheckToAll
        // 
        CheckToAll.Anchor = AnchorStyles.None;
        CheckToAll.AutoSize = true;
        CheckToAll.Font = new Font("Segoe UI", 9F);
        CheckToAll.ImeMode = ImeMode.NoControl;
        CheckToAll.Location = new Point(3, 6);
        CheckToAll.Name = "CheckToAll";
        CheckToAll.Size = new Size(59, 19);
        CheckToAll.TabIndex = 3;
        CheckToAll.Text = "&To all?";
        CheckToAll.UseVisualStyleBackColor = false;
        // 
        // ButtonNo
        // 
        ButtonNo.DialogResult = DialogResult.No;
        ButtonNo.FlatAppearance.BorderColor = Color.Black;
        ButtonNo.FlatAppearance.MouseDownBackColor = Color.FromArgb(224, 224, 224);
        ButtonNo.FlatAppearance.MouseOverBackColor = Color.FromArgb(224, 224, 224);
        ButtonNo.ImageAlign = ContentAlignment.MiddleLeft;
        ButtonNo.Location = new Point(1160, 3);
        ButtonNo.Name = "ButtonNo";
        ButtonNo.Size = new Size(69, 25);
        ButtonNo.TabIndex = 1;
        ButtonNo.Text = "&No";
        // 
        // ButtonCancel
        // 
        ButtonCancel.DialogResult = DialogResult.Cancel;
        ButtonCancel.FlatAppearance.BorderColor = Color.Black;
        ButtonCancel.FlatAppearance.MouseDownBackColor = Color.FromArgb(224, 224, 224);
        ButtonCancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(224, 224, 224);
        ButtonCancel.ImageAlign = ContentAlignment.MiddleLeft;
        ButtonCancel.Location = new Point(1310, 3);
        ButtonCancel.Name = "ButtonCancel";
        ButtonCancel.Size = new Size(69, 25);
        ButtonCancel.TabIndex = 2;
        ButtonCancel.Text = "&Cancel";
        // 
        // ButtonYes
        // 
        ButtonYes.DialogResult = DialogResult.Yes;
        ButtonYes.FlatAppearance.BorderColor = Color.Black;
        ButtonYes.FlatAppearance.MouseDownBackColor = Color.Silver;
        ButtonYes.FlatAppearance.MouseOverBackColor = Color.Silver;
        ButtonYes.ImageAlign = ContentAlignment.MiddleLeft;
        ButtonYes.Location = new Point(1085, 3);
        ButtonYes.Name = "ButtonYes";
        ButtonYes.Size = new Size(69, 25);
        ButtonYes.TabIndex = 0;
        ButtonYes.Text = "&Yes";
        // 
        // LabelMessage
        // 
        LabelMessage.AutoSize = true;
        LabelMessage.Location = new Point(54, 9);
        LabelMessage.MinimumSize = new Size(0, 90);
        LabelMessage.Name = "LabelMessage";
        LabelMessage.Size = new Size(53, 90);
        LabelMessage.TabIndex = 6;
        LabelMessage.Text = "Message";
        // 
        // PanelMessage
        // 
        PanelMessage.AutoSize = true;
        PanelMessage.Controls.Add(LabelMessage);
        PanelMessage.Controls.Add(PictureIcon);
        PanelMessage.Dock = DockStyle.Fill;
        PanelMessage.Location = new Point(0, 0);
        PanelMessage.Name = "PanelMessage";
        PanelMessage.Size = new Size(1382, 598);
        PanelMessage.TabIndex = 7;
        // 
        // PanelDetails
        // 
        PanelDetails.AutoSize = true;
        PanelDetails.Controls.Add(TextDetails);
        PanelDetails.Dock = DockStyle.Bottom;
        PanelDetails.Location = new Point(0, 630);
        PanelDetails.Margin = new Padding(0);
        PanelDetails.Name = "PanelDetails";
        PanelDetails.Size = new Size(1382, 200);
        PanelDetails.TabIndex = 7;
        PanelDetails.Visible = false;
        // 
        // PanelButtonsBorder
        // 
        PanelButtonsBorder.AutoSize = true;
        PanelButtonsBorder.BackColor = Color.Black;
        PanelButtonsBorder.Controls.Add(TableButtons);
        PanelButtonsBorder.Dock = DockStyle.Bottom;
        PanelButtonsBorder.Location = new Point(0, 598);
        PanelButtonsBorder.Name = "PanelButtonsBorder";
        PanelButtonsBorder.Padding = new Padding(0, 1, 0, 0);
        PanelButtonsBorder.Size = new Size(1382, 32);
        PanelButtonsBorder.TabIndex = 7;
        // 
        // TableButtons
        // 
        TableButtons.AutoSize = true;
        TableButtons.BackColor = Color.White;
        TableButtons.ColumnCount = 7;
        TableButtons.ColumnStyles.Add(new ColumnStyle());
        TableButtons.ColumnStyles.Add(new ColumnStyle());
        TableButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        TableButtons.ColumnStyles.Add(new ColumnStyle());
        TableButtons.ColumnStyles.Add(new ColumnStyle());
        TableButtons.ColumnStyles.Add(new ColumnStyle());
        TableButtons.ColumnStyles.Add(new ColumnStyle());
        TableButtons.Controls.Add(CheckToAll, 0, 0);
        TableButtons.Controls.Add(CheckDetail, 1, 0);
        TableButtons.Controls.Add(ButtonYes, 3, 0);
        TableButtons.Controls.Add(ButtonNo, 4, 0);
        TableButtons.Controls.Add(ButtonOK, 5, 0);
        TableButtons.Controls.Add(ButtonCancel, 6, 0);
        TableButtons.Dock = DockStyle.Bottom;
        TableButtons.Location = new Point(0, 1);
        TableButtons.Name = "TableButtons";
        TableButtons.RowCount = 1;
        TableButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        TableButtons.Size = new Size(1382, 31);
        TableButtons.TabIndex = 7;
        // 
        // FormDialog
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        ClientSize = new Size(1382, 830);
        Controls.Add(PanelMessage);
        Controls.Add(PanelButtonsBorder);
        Controls.Add(PanelDetails);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Icon = (Icon)resources.GetObject("$this.Icon");
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(450, 169);
        Name = "FormDialog";
        StartPosition = FormStartPosition.CenterParent;
        ((System.ComponentModel.ISupportInitialize)PictureIcon).EndInit();
        PanelMessage.ResumeLayout(false);
        PanelMessage.PerformLayout();
        PanelDetails.ResumeLayout(false);
        PanelDetails.PerformLayout();
        PanelButtonsBorder.ResumeLayout(false);
        PanelButtonsBorder.PerformLayout();
        TableButtons.ResumeLayout(false);
        TableButtons.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private PictureBox PictureIcon;
    private CheckBox CheckDetail;
    private TextBox TextDetails;
    private CheckBox CheckToAll;
    private Button ButtonOK;
    private Button ButtonNo;
    private Button ButtonCancel;
    private Button ButtonYes;
    private Label LabelMessage;
    private Panel PanelMessage;
    private Panel PanelDetails;
    private Panel PanelButtonsBorder;
    private TableLayoutPanel TableButtons;
}