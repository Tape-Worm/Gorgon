using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Editor.ImageEditorPlugIn
{
	partial class FormCodecs
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCodecs));
			this.panel1 = new System.Windows.Forms.Panel();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.containerCodecs = new System.Windows.Forms.ToolStripContainer();
			this.stripCodecs = new System.Windows.Forms.ToolStrip();
			this.buttonAddCodec = new System.Windows.Forms.ToolStripButton();
			this.buttonRemoveCodec = new System.Windows.Forms.ToolStripButton();
			this.listCodecs = new System.Windows.Forms.ListView();
			this.columnName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnDesc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.panel1.SuspendLayout();
			this.containerCodecs.ContentPanel.SuspendLayout();
			this.containerCodecs.TopToolStripPanel.SuspendLayout();
			this.containerCodecs.SuspendLayout();
			this.stripCodecs.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.buttonOK);
			this.panel1.Controls.Add(this.buttonCancel);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(1, 410);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(654, 35);
			this.panel1.TabIndex = 5;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonOK.ForeColor = System.Drawing.Color.White;
			this.buttonOK.Image = global::Gorgon.Editor.ImageEditorPlugIn.Properties.Resources.ok_16x16;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(471, 3);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(87, 28);
			this.buttonOK.TabIndex = 2;
			this.buttonOK.Text = "OK";
			this.buttonOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonOK.UseVisualStyleBackColor = false;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.Image = global::Gorgon.Editor.ImageEditorPlugIn.Properties.Resources.cancel_16x16;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(564, 3);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 28);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonCancel.UseVisualStyleBackColor = false;
			// 
			// containerCodecs
			// 
			// 
			// containerCodecs.ContentPanel
			// 
			this.containerCodecs.ContentPanel.Controls.Add(this.listCodecs);
			this.containerCodecs.ContentPanel.Size = new System.Drawing.Size(654, 360);
			this.containerCodecs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerCodecs.Location = new System.Drawing.Point(1, 25);
			this.containerCodecs.Name = "containerCodecs";
			this.containerCodecs.Size = new System.Drawing.Size(654, 385);
			this.containerCodecs.TabIndex = 6;
			this.containerCodecs.Text = "toolStripContainer1";
			// 
			// containerCodecs.TopToolStripPanel
			// 
			this.containerCodecs.TopToolStripPanel.Controls.Add(this.stripCodecs);
			// 
			// stripCodecs
			// 
			this.stripCodecs.Dock = System.Windows.Forms.DockStyle.None;
			this.stripCodecs.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripCodecs.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonAddCodec,
            this.buttonRemoveCodec});
			this.stripCodecs.Location = new System.Drawing.Point(0, 0);
			this.stripCodecs.Name = "stripCodecs";
			this.stripCodecs.Size = new System.Drawing.Size(654, 25);
			this.stripCodecs.Stretch = true;
			this.stripCodecs.TabIndex = 0;
			// 
			// buttonAddCodec
			// 
			this.buttonAddCodec.Image = global::Gorgon.Editor.ImageEditorPlugIn.Properties.Resources.image_add_16x16;
			this.buttonAddCodec.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonAddCodec.Name = "buttonAddCodec";
			this.buttonAddCodec.Size = new System.Drawing.Size(154, 22);
			this.buttonAddCodec.Text = "Not localized add codec";
			// 
			// buttonRemoveCodec
			// 
			this.buttonRemoveCodec.Enabled = false;
			this.buttonRemoveCodec.Image = global::Gorgon.Editor.ImageEditorPlugIn.Properties.Resources.remove_image_16x16;
			this.buttonRemoveCodec.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonRemoveCodec.Name = "buttonRemoveCodec";
			this.buttonRemoveCodec.Size = new System.Drawing.Size(174, 22);
			this.buttonRemoveCodec.Text = "Not localized remove codec";
			// 
			// listCodecs
			// 
			this.listCodecs.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listCodecs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnName,
            this.columnDesc,
            this.columnPath});
			this.listCodecs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listCodecs.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listCodecs.Location = new System.Drawing.Point(0, 0);
			this.listCodecs.Name = "listCodecs";
			this.listCodecs.Size = new System.Drawing.Size(654, 360);
			this.listCodecs.TabIndex = 0;
			this.listCodecs.UseCompatibleStateImageBehavior = false;
			this.listCodecs.View = System.Windows.Forms.View.Details;
			// 
			// columnName
			// 
			this.columnName.Text = "Not localized name";
			// 
			// columnDesc
			// 
			this.columnDesc.Text = "Not localized desc";
			// 
			// columnPath
			// 
			this.columnPath.Text = "Not localized path";
			// 
			// FormCodecs
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.Border = true;
			this.BorderColor = System.Drawing.Color.SteelBlue;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(656, 446);
			this.Controls.Add(this.containerCodecs);
			this.Controls.Add(this.panel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ForeColor = System.Drawing.Color.Silver;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormCodecs";
			this.Padding = new System.Windows.Forms.Padding(1);
			this.Resizable = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Not localized codecs";
			this.Controls.SetChildIndex(this.panel1, 0);
			this.Controls.SetChildIndex(this.containerCodecs, 0);
			this.panel1.ResumeLayout(false);
			this.containerCodecs.ContentPanel.ResumeLayout(false);
			this.containerCodecs.TopToolStripPanel.ResumeLayout(false);
			this.containerCodecs.TopToolStripPanel.PerformLayout();
			this.containerCodecs.ResumeLayout(false);
			this.containerCodecs.PerformLayout();
			this.stripCodecs.ResumeLayout(false);
			this.stripCodecs.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Panel panel1;
		private Button buttonOK;
		private Button buttonCancel;
		private ToolStripContainer containerCodecs;
		private ToolStrip stripCodecs;
		private ToolStripButton buttonAddCodec;
		private ToolStripButton buttonRemoveCodec;
		private ListView listCodecs;
		private ColumnHeader columnName;
		private ColumnHeader columnDesc;
		private ColumnHeader columnPath;
	}
}