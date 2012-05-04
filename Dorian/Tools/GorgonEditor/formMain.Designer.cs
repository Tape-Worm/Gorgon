#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Monday, April 30, 2012 6:28:36 PM
// 
#endregion

namespace GorgonLibrary.GorgonEditor
{
	partial class formMain
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formMain));
			this.panelMainMenu = new System.Windows.Forms.Panel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonSave = new System.Windows.Forms.Button();
			this.buttonOpen = new System.Windows.Forms.Button();
			this.buttonNew = new System.Windows.Forms.Button();
			this.buttonExit = new System.Windows.Forms.Button();
			this.splitMain = new System.Windows.Forms.SplitContainer();
			this.panelDynamicFileMenu = new System.Windows.Forms.Panel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAnimationAction = new System.Windows.Forms.Button();
			this.buttonShaderAction = new System.Windows.Forms.Button();
			this.buttonSpriteAction = new System.Windows.Forms.Button();
			this.buttonFontAction = new System.Windows.Forms.Button();
			this.buttonProjectAction = new System.Windows.Forms.Button();
			this.splitEdit = new System.Windows.Forms.SplitContainer();
			this.panelEditor = new System.Windows.Forms.Panel();
			this.tabDocuments = new KRBTabControl.KRBTabControl();
			this.propertyItem = new System.Windows.Forms.PropertyGrid();
			this.panelMainMenu.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
			this.splitMain.Panel1.SuspendLayout();
			this.splitMain.Panel2.SuspendLayout();
			this.splitMain.SuspendLayout();
			this.panelDynamicFileMenu.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitEdit)).BeginInit();
			this.splitEdit.Panel1.SuspendLayout();
			this.splitEdit.Panel2.SuspendLayout();
			this.splitEdit.SuspendLayout();
			this.panelEditor.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelMainMenu
			// 
			this.panelMainMenu.AutoScroll = true;
			this.panelMainMenu.BackColor = System.Drawing.SystemColors.ControlDark;
			this.panelMainMenu.Controls.Add(this.tableLayoutPanel1);
			this.panelMainMenu.Dock = System.Windows.Forms.DockStyle.Left;
			this.panelMainMenu.Location = new System.Drawing.Point(0, 0);
			this.panelMainMenu.Name = "panelMainMenu";
			this.panelMainMenu.Size = new System.Drawing.Size(92, 613);
			this.panelMainMenu.TabIndex = 0;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.buttonSave, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.buttonOpen, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.buttonNew, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.buttonExit, 0, 5);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(92, 613);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// buttonSave
			// 
			this.buttonSave.Dock = System.Windows.Forms.DockStyle.Top;
			this.buttonSave.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonSave.FlatAppearance.CheckedBackColor = System.Drawing.Color.DodgerBlue;
			this.buttonSave.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.buttonSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.buttonSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonSave.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonSave.ForeColor = System.Drawing.Color.White;
			this.buttonSave.Location = new System.Drawing.Point(3, 69);
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.Size = new System.Drawing.Size(86, 27);
			this.buttonSave.TabIndex = 2;
			this.buttonSave.Text = "&Save";
			this.buttonSave.UseVisualStyleBackColor = true;
			this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
			// 
			// buttonOpen
			// 
			this.buttonOpen.Dock = System.Windows.Forms.DockStyle.Top;
			this.buttonOpen.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonOpen.FlatAppearance.CheckedBackColor = System.Drawing.Color.DodgerBlue;
			this.buttonOpen.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.buttonOpen.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.buttonOpen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonOpen.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonOpen.ForeColor = System.Drawing.Color.White;
			this.buttonOpen.Location = new System.Drawing.Point(3, 36);
			this.buttonOpen.Name = "buttonOpen";
			this.buttonOpen.Size = new System.Drawing.Size(86, 27);
			this.buttonOpen.TabIndex = 1;
			this.buttonOpen.Text = "&Open";
			this.buttonOpen.UseVisualStyleBackColor = true;
			this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
			// 
			// buttonNew
			// 
			this.buttonNew.Dock = System.Windows.Forms.DockStyle.Top;
			this.buttonNew.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonNew.FlatAppearance.CheckedBackColor = System.Drawing.Color.DodgerBlue;
			this.buttonNew.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.buttonNew.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.buttonNew.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonNew.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonNew.ForeColor = System.Drawing.Color.White;
			this.buttonNew.Location = new System.Drawing.Point(3, 3);
			this.buttonNew.Name = "buttonNew";
			this.buttonNew.Size = new System.Drawing.Size(86, 27);
			this.buttonNew.TabIndex = 0;
			this.buttonNew.Text = "&New";
			this.buttonNew.UseVisualStyleBackColor = true;
			this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
			// 
			// buttonExit
			// 
			this.buttonExit.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonExit.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonExit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.buttonExit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.buttonExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonExit.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonExit.ForeColor = System.Drawing.Color.White;
			this.buttonExit.Location = new System.Drawing.Point(3, 587);
			this.buttonExit.Name = "buttonExit";
			this.buttonExit.Size = new System.Drawing.Size(86, 23);
			this.buttonExit.TabIndex = 3;
			this.buttonExit.Text = "E&xit";
			this.buttonExit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonExit.UseVisualStyleBackColor = true;
			// 
			// splitMain
			// 
			this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitMain.IsSplitterFixed = true;
			this.splitMain.Location = new System.Drawing.Point(92, 0);
			this.splitMain.Name = "splitMain";
			// 
			// splitMain.Panel1
			// 
			this.splitMain.Panel1.BackColor = System.Drawing.SystemColors.ControlDark;
			this.splitMain.Panel1.Controls.Add(this.panelDynamicFileMenu);
			this.splitMain.Panel1.Padding = new System.Windows.Forms.Padding(3);
			// 
			// splitMain.Panel2
			// 
			this.splitMain.Panel2.Controls.Add(this.splitEdit);
			this.splitMain.Size = new System.Drawing.Size(852, 613);
			this.splitMain.SplitterDistance = 110;
			this.splitMain.SplitterWidth = 1;
			this.splitMain.TabIndex = 1;
			// 
			// panelDynamicFileMenu
			// 
			this.panelDynamicFileMenu.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelDynamicFileMenu.Controls.Add(this.tableLayoutPanel2);
			this.panelDynamicFileMenu.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelDynamicFileMenu.Location = new System.Drawing.Point(3, 3);
			this.panelDynamicFileMenu.Name = "panelDynamicFileMenu";
			this.panelDynamicFileMenu.Size = new System.Drawing.Size(104, 607);
			this.panelDynamicFileMenu.TabIndex = 0;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Controls.Add(this.buttonAnimationAction, 0, 5);
			this.tableLayoutPanel2.Controls.Add(this.buttonShaderAction, 0, 4);
			this.tableLayoutPanel2.Controls.Add(this.buttonSpriteAction, 0, 3);
			this.tableLayoutPanel2.Controls.Add(this.buttonFontAction, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.buttonProjectAction, 0, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 6;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(102, 605);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// buttonAnimationAction
			// 
			this.buttonAnimationAction.BackColor = System.Drawing.SystemColors.Control;
			this.buttonAnimationAction.Dock = System.Windows.Forms.DockStyle.Top;
			this.buttonAnimationAction.Enabled = false;
			this.buttonAnimationAction.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonAnimationAction.FlatAppearance.BorderSize = 0;
			this.buttonAnimationAction.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.buttonAnimationAction.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.buttonAnimationAction.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonAnimationAction.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonAnimationAction.ForeColor = System.Drawing.Color.Black;
			this.buttonAnimationAction.Location = new System.Drawing.Point(3, 139);
			this.buttonAnimationAction.Name = "buttonAnimationAction";
			this.buttonAnimationAction.Size = new System.Drawing.Size(96, 23);
			this.buttonAnimationAction.TabIndex = 9;
			this.buttonAnimationAction.Text = "&Animation...";
			this.buttonAnimationAction.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonAnimationAction.UseVisualStyleBackColor = false;
			// 
			// buttonShaderAction
			// 
			this.buttonShaderAction.BackColor = System.Drawing.SystemColors.Control;
			this.buttonShaderAction.Dock = System.Windows.Forms.DockStyle.Top;
			this.buttonShaderAction.Enabled = false;
			this.buttonShaderAction.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonShaderAction.FlatAppearance.BorderSize = 0;
			this.buttonShaderAction.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.buttonShaderAction.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.buttonShaderAction.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonShaderAction.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonShaderAction.ForeColor = System.Drawing.Color.Black;
			this.buttonShaderAction.Location = new System.Drawing.Point(3, 110);
			this.buttonShaderAction.Name = "buttonShaderAction";
			this.buttonShaderAction.Size = new System.Drawing.Size(96, 23);
			this.buttonShaderAction.TabIndex = 8;
			this.buttonShaderAction.Text = "S&hader...";
			this.buttonShaderAction.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonShaderAction.UseVisualStyleBackColor = false;
			// 
			// buttonSpriteAction
			// 
			this.buttonSpriteAction.BackColor = System.Drawing.SystemColors.Control;
			this.buttonSpriteAction.Dock = System.Windows.Forms.DockStyle.Top;
			this.buttonSpriteAction.Enabled = false;
			this.buttonSpriteAction.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonSpriteAction.FlatAppearance.BorderSize = 0;
			this.buttonSpriteAction.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.buttonSpriteAction.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.buttonSpriteAction.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonSpriteAction.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonSpriteAction.ForeColor = System.Drawing.Color.Black;
			this.buttonSpriteAction.Location = new System.Drawing.Point(3, 81);
			this.buttonSpriteAction.Name = "buttonSpriteAction";
			this.buttonSpriteAction.Size = new System.Drawing.Size(96, 23);
			this.buttonSpriteAction.TabIndex = 7;
			this.buttonSpriteAction.Text = "&Sprite...";
			this.buttonSpriteAction.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonSpriteAction.UseVisualStyleBackColor = false;
			// 
			// buttonFontAction
			// 
			this.buttonFontAction.BackColor = System.Drawing.SystemColors.Control;
			this.buttonFontAction.Dock = System.Windows.Forms.DockStyle.Top;
			this.buttonFontAction.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonFontAction.FlatAppearance.BorderSize = 0;
			this.buttonFontAction.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.buttonFontAction.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.buttonFontAction.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonFontAction.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonFontAction.ForeColor = System.Drawing.Color.Black;
			this.buttonFontAction.Location = new System.Drawing.Point(3, 52);
			this.buttonFontAction.Name = "buttonFontAction";
			this.buttonFontAction.Size = new System.Drawing.Size(96, 23);
			this.buttonFontAction.TabIndex = 6;
			this.buttonFontAction.Text = "&Font...";
			this.buttonFontAction.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonFontAction.UseVisualStyleBackColor = false;
			this.buttonFontAction.Click += new System.EventHandler(this.buttonFontAction_Click);
			// 
			// buttonProjectAction
			// 
			this.buttonProjectAction.BackColor = System.Drawing.SystemColors.Control;
			this.buttonProjectAction.Dock = System.Windows.Forms.DockStyle.Top;
			this.buttonProjectAction.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonProjectAction.FlatAppearance.BorderSize = 0;
			this.buttonProjectAction.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
			this.buttonProjectAction.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.buttonProjectAction.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonProjectAction.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonProjectAction.ForeColor = System.Drawing.Color.Black;
			this.buttonProjectAction.Location = new System.Drawing.Point(3, 3);
			this.buttonProjectAction.Name = "buttonProjectAction";
			this.buttonProjectAction.Size = new System.Drawing.Size(96, 23);
			this.buttonProjectAction.TabIndex = 5;
			this.buttonProjectAction.Text = "P&roject...";
			this.buttonProjectAction.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonProjectAction.UseVisualStyleBackColor = false;
			// 
			// splitEdit
			// 
			this.splitEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitEdit.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitEdit.Location = new System.Drawing.Point(0, 0);
			this.splitEdit.Name = "splitEdit";
			// 
			// splitEdit.Panel1
			// 
			this.splitEdit.Panel1.Controls.Add(this.panelEditor);
			this.splitEdit.Panel1.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			// 
			// splitEdit.Panel2
			// 
			this.splitEdit.Panel2.Controls.Add(this.propertyItem);
			this.splitEdit.Size = new System.Drawing.Size(741, 613);
			this.splitEdit.SplitterDistance = 512;
			this.splitEdit.TabIndex = 0;
			// 
			// panelEditor
			// 
			this.panelEditor.Controls.Add(this.tabDocuments);
			this.panelEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelEditor.Location = new System.Drawing.Point(3, 0);
			this.panelEditor.Name = "panelEditor";
			this.panelEditor.Size = new System.Drawing.Size(509, 613);
			this.panelEditor.TabIndex = 0;
			// 
			// tabDocuments
			// 
			this.tabDocuments.AllowDrop = true;
			this.tabDocuments.BackgroundColor = System.Drawing.SystemColors.ControlDarkDark;
			this.tabDocuments.BackgroundHatcher.HatchType = System.Drawing.Drawing2D.HatchStyle.DashedVertical;
			this.tabDocuments.BorderColor = System.Drawing.SystemColors.ControlDark;
			this.tabDocuments.CaptionButtons.InactiveCaptionButtonsColor = System.Drawing.SystemColors.ControlDark;
			this.tabDocuments.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabDocuments.GradientCaption.ActiveCaptionColorEnd = System.Drawing.SystemColors.ControlDark;
			this.tabDocuments.GradientCaption.ActiveCaptionColorStart = System.Drawing.SystemColors.ControlDark;
			this.tabDocuments.GradientCaption.ActiveCaptionFontStyle = System.Drawing.FontStyle.Bold;
			this.tabDocuments.GradientCaption.ActiveCaptionTextColor = System.Drawing.Color.White;
			this.tabDocuments.GradientCaption.InactiveCaptionColorEnd = System.Drawing.SystemColors.ControlDarkDark;
			this.tabDocuments.GradientCaption.InactiveCaptionColorStart = System.Drawing.SystemColors.ControlDarkDark;
			this.tabDocuments.GradientCaption.InactiveCaptionTextColor = System.Drawing.SystemColors.ControlDark;
			this.tabDocuments.IsCaptionVisible = false;
			this.tabDocuments.IsDocumentTabStyle = true;
			this.tabDocuments.IsDrawTabSeparator = true;
			this.tabDocuments.IsUserInteraction = false;
			this.tabDocuments.ItemSize = new System.Drawing.Size(0, 26);
			this.tabDocuments.Location = new System.Drawing.Point(0, 0);
			this.tabDocuments.Name = "tabDocuments";
			this.tabDocuments.Size = new System.Drawing.Size(509, 613);
			this.tabDocuments.TabGradient.ColorEnd = System.Drawing.SystemColors.Control;
			this.tabDocuments.TabGradient.ColorStart = System.Drawing.SystemColors.Control;
			this.tabDocuments.TabHOffset = 2;
			this.tabDocuments.TabIndex = 0;
			this.tabDocuments.UpDownStyle = KRBTabControl.KRBTabControl.UpDown32Style.BlackGlass;
			this.tabDocuments.ContextMenuShown += new System.EventHandler<KRBTabControl.KRBTabControl.ContextMenuShownEventArgs>(this.tabDocuments_ContextMenuShown);
			// 
			// propertyItem
			// 
			this.propertyItem.CategoryForeColor = System.Drawing.Color.White;
			this.propertyItem.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyItem.HelpBackColor = System.Drawing.SystemColors.ControlDark;
			this.propertyItem.LineColor = System.Drawing.Color.Black;
			this.propertyItem.Location = new System.Drawing.Point(0, 0);
			this.propertyItem.Name = "propertyItem";
			this.propertyItem.Size = new System.Drawing.Size(225, 613);
			this.propertyItem.TabIndex = 0;
			this.propertyItem.ToolbarVisible = false;
			this.propertyItem.ViewBackColor = System.Drawing.SystemColors.ControlDark;
			this.propertyItem.ViewForeColor = System.Drawing.Color.White;
			// 
			// formMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.ClientSize = new System.Drawing.Size(944, 613);
			this.Controls.Add(this.splitMain);
			this.Controls.Add(this.panelMainMenu);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "formMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Editor";
			this.panelMainMenu.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.splitMain.Panel1.ResumeLayout(false);
			this.splitMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
			this.splitMain.ResumeLayout(false);
			this.panelDynamicFileMenu.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.splitEdit.Panel1.ResumeLayout(false);
			this.splitEdit.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitEdit)).EndInit();
			this.splitEdit.ResumeLayout(false);
			this.panelEditor.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelMainMenu;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button buttonSave;
		private System.Windows.Forms.Button buttonOpen;
		private System.Windows.Forms.Button buttonNew;
		private System.Windows.Forms.Button buttonExit;
		private System.Windows.Forms.SplitContainer splitMain;
		private System.Windows.Forms.SplitContainer splitEdit;
		private System.Windows.Forms.PropertyGrid propertyItem;
		private System.Windows.Forms.Panel panelDynamicFileMenu;
		private System.Windows.Forms.Panel panelEditor;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Button buttonAnimationAction;
		private System.Windows.Forms.Button buttonShaderAction;
		private System.Windows.Forms.Button buttonSpriteAction;
		private System.Windows.Forms.Button buttonFontAction;
		private System.Windows.Forms.Button buttonProjectAction;
		private KRBTabControl.KRBTabControl tabDocuments;
	}
}

