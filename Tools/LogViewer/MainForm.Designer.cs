namespace GorgonLibrary.Tools
{
	partial class MainForm
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.fileOpen = new System.Windows.Forms.OpenFileDialog();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textLog = new System.Windows.Forms.TextBox();
			this.comboSection = new System.Windows.Forms.ComboBox();
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
			this.menuOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.menuExit = new System.Windows.Forms.ToolStripMenuItem();
			this.timerTail = new System.Windows.Forms.Timer(this.components);
			this.menuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// fileOpen
			// 
			this.fileOpen.DefaultExt = "Log";
			this.fileOpen.Filter = "Gorgon Log Files (*.log)|*.log|All Files (*.*)|*.*";
			this.fileOpen.InitialDirectory = ".\\";
			this.fileOpen.Title = "Open Gorgon Log File.";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 34);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(46, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Section:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 74);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(28, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Log:";
			// 
			// textLog
			// 
			this.textLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textLog.BackColor = System.Drawing.Color.White;
			this.textLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textLog.ForeColor = System.Drawing.Color.Black;
			this.textLog.Location = new System.Drawing.Point(15, 90);
			this.textLog.MaxLength = 0;
			this.textLog.Multiline = true;
			this.textLog.Name = "textLog";
			this.textLog.ReadOnly = true;
			this.textLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textLog.Size = new System.Drawing.Size(520, 171);
			this.textLog.TabIndex = 2;
			this.textLog.WordWrap = false;
			// 
			// comboSection
			// 
			this.comboSection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.comboSection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboSection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.comboSection.FormattingEnabled = true;
			this.comboSection.Location = new System.Drawing.Point(15, 50);
			this.comboSection.Name = "comboSection";
			this.comboSection.Size = new System.Drawing.Size(520, 21);
			this.comboSection.Sorted = true;
			this.comboSection.TabIndex = 3;
			this.comboSection.SelectedIndexChanged += new System.EventHandler(this.comboSection_SelectedIndexChanged);
			// 
			// menuStrip
			// 
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile});
			this.menuStrip.Location = new System.Drawing.Point(0, 0);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size(547, 24);
			this.menuStrip.TabIndex = 4;
			this.menuStrip.Text = "menuStrip";
			// 
			// menuFile
			// 
			this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuOpen,
            this.toolStripMenuItem1,
            this.menuExit});
			this.menuFile.Name = "menuFile";
			this.menuFile.Size = new System.Drawing.Size(35, 20);
			this.menuFile.Text = "&File";
			// 
			// menuOpen
			// 
			this.menuOpen.Image = global::GorgonLibrary.Tools.Properties.Resources.folder_out;
			this.menuOpen.Name = "menuOpen";
			this.menuOpen.ShortcutKeyDisplayString = "Ctrl+O";
			this.menuOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.menuOpen.Size = new System.Drawing.Size(194, 22);
			this.menuOpen.Text = "&Open Log File...";
			this.menuOpen.Click += new System.EventHandler(this.menuOpen_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(191, 6);
			// 
			// menuExit
			// 
			this.menuExit.Image = global::GorgonLibrary.Tools.Properties.Resources.delete;
			this.menuExit.Name = "menuExit";
			this.menuExit.ShortcutKeyDisplayString = "Alt+F4";
			this.menuExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.menuExit.Size = new System.Drawing.Size(194, 22);
			this.menuExit.Text = "E&xit";
			this.menuExit.Click += new System.EventHandler(this.menuExit_Click);
			// 
			// timerTail
			// 
			this.timerTail.Tick += new System.EventHandler(this.timerTail_Tick);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(547, 273);
			this.Controls.Add(this.comboSection);
			this.Controls.Add(this.textLog);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.menuStrip);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Gorgon Log Viewer";
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.OpenFileDialog fileOpen;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textLog;
		private System.Windows.Forms.ComboBox comboSection;
		private System.Windows.Forms.MenuStrip menuStrip;
		private System.Windows.Forms.ToolStripMenuItem menuFile;
		private System.Windows.Forms.ToolStripMenuItem menuOpen;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem menuExit;
		private System.Windows.Forms.Timer timerTail;
	}
}

