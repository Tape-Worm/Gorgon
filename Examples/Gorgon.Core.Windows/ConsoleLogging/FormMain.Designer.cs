namespace Gorgon.Examples;

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
        ButtonWarning = new Button();
        ButtonError = new Button();
        ButtonException = new Button();
        ButtonCustom = new Button();
        TextLogMessage = new TextBox();
        ButtonToLog = new Button();
        SuspendLayout();
        // 
        // ButtonWarning
        // 
        ButtonWarning.Image = Properties.Resources.warning_24x24;
        ButtonWarning.Location = new Point(12, 12);
        ButtonWarning.Name = "ButtonWarning";
        ButtonWarning.Size = new Size(102, 50);
        ButtonWarning.TabIndex = 0;
        ButtonWarning.Text = "Warning!";
        ButtonWarning.TextAlign = ContentAlignment.MiddleRight;
        ButtonWarning.TextImageRelation = TextImageRelation.ImageBeforeText;
        ButtonWarning.UseVisualStyleBackColor = true;
        ButtonWarning.Click += ButtonWarning_Click;
        // 
        // ButtonError
        // 
        ButtonError.Image = Properties.Resources.error_24x24;
        ButtonError.Location = new Point(120, 12);
        ButtonError.Name = "ButtonError";
        ButtonError.Size = new Size(102, 50);
        ButtonError.TabIndex = 1;
        ButtonError.Text = "Error!";
        ButtonError.TextAlign = ContentAlignment.MiddleRight;
        ButtonError.TextImageRelation = TextImageRelation.ImageBeforeText;
        ButtonError.UseVisualStyleBackColor = true;
        ButtonError.Click += ButtonError_Click;
        // 
        // ButtonException
        // 
        ButtonException.Image = Properties.Resources.error_24x24;
        ButtonException.Location = new Point(228, 12);
        ButtonException.Name = "ButtonException";
        ButtonException.Size = new Size(102, 50);
        ButtonException.TabIndex = 2;
        ButtonException.Text = "Exception!";
        ButtonException.TextAlign = ContentAlignment.MiddleRight;
        ButtonException.TextImageRelation = TextImageRelation.ImageBeforeText;
        ButtonException.UseVisualStyleBackColor = true;
        ButtonException.Click += ButtonException_Click;
        // 
        // ButtonCustom
        // 
        ButtonCustom.Image = Properties.Resources.info_24x24;
        ButtonCustom.Location = new Point(336, 12);
        ButtonCustom.Name = "ButtonCustom";
        ButtonCustom.Size = new Size(102, 50);
        ButtonCustom.TabIndex = 3;
        ButtonCustom.Text = "Custom";
        ButtonCustom.TextAlign = ContentAlignment.MiddleRight;
        ButtonCustom.TextImageRelation = TextImageRelation.ImageBeforeText;
        ButtonCustom.UseVisualStyleBackColor = true;
        ButtonCustom.Click += ButtonCustom_Click;
        // 
        // TextLogMessage
        // 
        TextLogMessage.AcceptsReturn = true;
        TextLogMessage.BorderStyle = BorderStyle.FixedSingle;
        TextLogMessage.Enabled = false;
        TextLogMessage.Location = new Point(14, 68);
        TextLogMessage.MaxLength = 1024;
        TextLogMessage.Multiline = true;
        TextLogMessage.Name = "TextLogMessage";
        TextLogMessage.ScrollBars = ScrollBars.Vertical;
        TextLogMessage.Size = new Size(424, 85);
        TextLogMessage.TabIndex = 4;
        TextLogMessage.TextChanged += TextLogMessage_TextChanged;
        // 
        // ButtonToLog
        // 
        ButtonToLog.Enabled = false;
        ButtonToLog.Image = Properties.Resources.save_16x16;
        ButtonToLog.Location = new Point(14, 159);
        ButtonToLog.Name = "ButtonToLog";
        ButtonToLog.Size = new Size(424, 26);
        ButtonToLog.TabIndex = 5;
        ButtonToLog.Text = "Send to log";
        ButtonToLog.TextAlign = ContentAlignment.MiddleRight;
        ButtonToLog.TextImageRelation = TextImageRelation.ImageBeforeText;
        ButtonToLog.UseVisualStyleBackColor = true;
        ButtonToLog.Click += ButtonToLog_Click;
        // 
        // FormMain
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        ClientSize = new Size(453, 198);
        Controls.Add(ButtonToLog);
        Controls.Add(TextLogMessage);
        Controls.Add(ButtonCustom);
        Controls.Add(ButtonException);
        Controls.Add(ButtonError);
        Controls.Add(ButtonWarning);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Icon = (Icon)resources.GetObject("$this.Icon");
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "FormMain";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Log Controls";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Button ButtonWarning;
    private Button ButtonError;
    private Button ButtonException;
    private Button ButtonCustom;
    private TextBox TextLogMessage;
    private Button ButtonToLog;
}