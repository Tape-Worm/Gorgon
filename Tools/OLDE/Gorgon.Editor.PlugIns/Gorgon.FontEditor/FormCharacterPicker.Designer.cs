using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Editor.FontEditorPlugIn
{
	partial class FormCharacterPicker
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCharacterPicker));
			this.panel1 = new System.Windows.Forms.Panel();
			this.buttonSelectAll = new System.Windows.Forms.Button();
			this.listRanges = new System.Windows.Forms.ListView();
			this.columnRange = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnRangeName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.panelCharacters = new System.Windows.Forms.Panel();
			this.scrollVertical = new System.Windows.Forms.VScrollBar();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.checkBox2 = new System.Windows.Forms.CheckBox();
			this.checkBox3 = new System.Windows.Forms.CheckBox();
			this.checkBox4 = new System.Windows.Forms.CheckBox();
			this.checkBox5 = new System.Windows.Forms.CheckBox();
			this.checkBox6 = new System.Windows.Forms.CheckBox();
			this.checkBox7 = new System.Windows.Forms.CheckBox();
			this.checkBox8 = new System.Windows.Forms.CheckBox();
			this.checkBox16 = new System.Windows.Forms.CheckBox();
			this.checkBox15 = new System.Windows.Forms.CheckBox();
			this.checkBox14 = new System.Windows.Forms.CheckBox();
			this.checkBox13 = new System.Windows.Forms.CheckBox();
			this.checkBox11 = new System.Windows.Forms.CheckBox();
			this.checkBox12 = new System.Windows.Forms.CheckBox();
			this.checkBox10 = new System.Windows.Forms.CheckBox();
			this.checkBox9 = new System.Windows.Forms.CheckBox();
			this.checkBox24 = new System.Windows.Forms.CheckBox();
			this.checkBox23 = new System.Windows.Forms.CheckBox();
			this.checkBox22 = new System.Windows.Forms.CheckBox();
			this.checkBox21 = new System.Windows.Forms.CheckBox();
			this.checkBox19 = new System.Windows.Forms.CheckBox();
			this.checkBox20 = new System.Windows.Forms.CheckBox();
			this.checkBox18 = new System.Windows.Forms.CheckBox();
			this.checkBox17 = new System.Windows.Forms.CheckBox();
			this.checkBox32 = new System.Windows.Forms.CheckBox();
			this.checkBox31 = new System.Windows.Forms.CheckBox();
			this.checkBox30 = new System.Windows.Forms.CheckBox();
			this.checkBox29 = new System.Windows.Forms.CheckBox();
			this.checkBox27 = new System.Windows.Forms.CheckBox();
			this.checkBox28 = new System.Windows.Forms.CheckBox();
			this.checkBox26 = new System.Windows.Forms.CheckBox();
			this.checkBox25 = new System.Windows.Forms.CheckBox();
			this.checkBox40 = new System.Windows.Forms.CheckBox();
			this.checkBox39 = new System.Windows.Forms.CheckBox();
			this.checkBox38 = new System.Windows.Forms.CheckBox();
			this.checkBox37 = new System.Windows.Forms.CheckBox();
			this.checkBox35 = new System.Windows.Forms.CheckBox();
			this.checkBox36 = new System.Windows.Forms.CheckBox();
			this.checkBox34 = new System.Windows.Forms.CheckBox();
			this.checkBox33 = new System.Windows.Forms.CheckBox();
			this.checkBox48 = new System.Windows.Forms.CheckBox();
			this.checkBox47 = new System.Windows.Forms.CheckBox();
			this.checkBox46 = new System.Windows.Forms.CheckBox();
			this.checkBox45 = new System.Windows.Forms.CheckBox();
			this.checkBox43 = new System.Windows.Forms.CheckBox();
			this.checkBox44 = new System.Windows.Forms.CheckBox();
			this.checkBox42 = new System.Windows.Forms.CheckBox();
			this.checkBox41 = new System.Windows.Forms.CheckBox();
			this.textCharacters = new System.Windows.Forms.TextBox();
			this.labelCharacters = new System.Windows.Forms.Label();
			this.tipChar = new System.Windows.Forms.ToolTip(this.components);
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.panelCharacters.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.buttonSelectAll);
			this.panel1.Controls.Add(this.listRanges);
			this.panel1.Controls.Add(this.panelCharacters);
			this.panel1.Controls.Add(this.textCharacters);
			this.panel1.Controls.Add(this.labelCharacters);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(1, 25);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(624, 372);
			this.panel1.TabIndex = 15;
			// 
			// buttonSelectAll
			// 
			this.buttonSelectAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonSelectAll.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonSelectAll.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonSelectAll.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonSelectAll.ForeColor = System.Drawing.Color.White;
			this.buttonSelectAll.Location = new System.Drawing.Point(372, 346);
			this.buttonSelectAll.Name = "buttonSelectAll";
			this.buttonSelectAll.Size = new System.Drawing.Size(243, 23);
			this.buttonSelectAll.TabIndex = 3;
			this.buttonSelectAll.Text = "select";
			this.buttonSelectAll.UseVisualStyleBackColor = false;
			this.buttonSelectAll.Click += new System.EventHandler(this.buttonSelectAll_Click);
			// 
			// listRanges
			// 
			this.listRanges.BackColor = System.Drawing.Color.White;
			this.listRanges.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listRanges.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnRange,
            this.columnRangeName});
			this.listRanges.ForeColor = System.Drawing.Color.Black;
			this.listRanges.FullRowSelect = true;
			this.listRanges.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listRanges.Location = new System.Drawing.Point(372, 6);
			this.listRanges.MultiSelect = false;
			this.listRanges.Name = "listRanges";
			this.listRanges.Size = new System.Drawing.Size(243, 334);
			this.listRanges.TabIndex = 2;
			this.listRanges.UseCompatibleStateImageBehavior = false;
			this.listRanges.View = System.Windows.Forms.View.Details;
			this.listRanges.SelectedIndexChanged += new System.EventHandler(this.listRanges_SelectedIndexChanged);
			// 
			// columnRange
			// 
			this.columnRange.Text = "Range";
			// 
			// columnRangeName
			// 
			this.columnRangeName.Text = "Name";
			// 
			// panelCharacters
			// 
			this.panelCharacters.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelCharacters.Controls.Add(this.scrollVertical);
			this.panelCharacters.Controls.Add(this.checkBox1);
			this.panelCharacters.Controls.Add(this.checkBox2);
			this.panelCharacters.Controls.Add(this.checkBox3);
			this.panelCharacters.Controls.Add(this.checkBox4);
			this.panelCharacters.Controls.Add(this.checkBox5);
			this.panelCharacters.Controls.Add(this.checkBox6);
			this.panelCharacters.Controls.Add(this.checkBox7);
			this.panelCharacters.Controls.Add(this.checkBox8);
			this.panelCharacters.Controls.Add(this.checkBox16);
			this.panelCharacters.Controls.Add(this.checkBox15);
			this.panelCharacters.Controls.Add(this.checkBox14);
			this.panelCharacters.Controls.Add(this.checkBox13);
			this.panelCharacters.Controls.Add(this.checkBox11);
			this.panelCharacters.Controls.Add(this.checkBox12);
			this.panelCharacters.Controls.Add(this.checkBox10);
			this.panelCharacters.Controls.Add(this.checkBox9);
			this.panelCharacters.Controls.Add(this.checkBox24);
			this.panelCharacters.Controls.Add(this.checkBox23);
			this.panelCharacters.Controls.Add(this.checkBox22);
			this.panelCharacters.Controls.Add(this.checkBox21);
			this.panelCharacters.Controls.Add(this.checkBox19);
			this.panelCharacters.Controls.Add(this.checkBox20);
			this.panelCharacters.Controls.Add(this.checkBox18);
			this.panelCharacters.Controls.Add(this.checkBox17);
			this.panelCharacters.Controls.Add(this.checkBox32);
			this.panelCharacters.Controls.Add(this.checkBox31);
			this.panelCharacters.Controls.Add(this.checkBox30);
			this.panelCharacters.Controls.Add(this.checkBox29);
			this.panelCharacters.Controls.Add(this.checkBox27);
			this.panelCharacters.Controls.Add(this.checkBox28);
			this.panelCharacters.Controls.Add(this.checkBox26);
			this.panelCharacters.Controls.Add(this.checkBox25);
			this.panelCharacters.Controls.Add(this.checkBox40);
			this.panelCharacters.Controls.Add(this.checkBox39);
			this.panelCharacters.Controls.Add(this.checkBox38);
			this.panelCharacters.Controls.Add(this.checkBox37);
			this.panelCharacters.Controls.Add(this.checkBox35);
			this.panelCharacters.Controls.Add(this.checkBox36);
			this.panelCharacters.Controls.Add(this.checkBox34);
			this.panelCharacters.Controls.Add(this.checkBox33);
			this.panelCharacters.Controls.Add(this.checkBox48);
			this.panelCharacters.Controls.Add(this.checkBox47);
			this.panelCharacters.Controls.Add(this.checkBox46);
			this.panelCharacters.Controls.Add(this.checkBox45);
			this.panelCharacters.Controls.Add(this.checkBox43);
			this.panelCharacters.Controls.Add(this.checkBox44);
			this.panelCharacters.Controls.Add(this.checkBox42);
			this.panelCharacters.Controls.Add(this.checkBox41);
			this.panelCharacters.ForeColor = System.Drawing.Color.White;
			this.panelCharacters.Location = new System.Drawing.Point(6, 6);
			this.panelCharacters.Name = "panelCharacters";
			this.panelCharacters.Size = new System.Drawing.Size(360, 252);
			this.panelCharacters.TabIndex = 0;
			// 
			// scrollVertical
			// 
			this.scrollVertical.Dock = System.Windows.Forms.DockStyle.Right;
			this.scrollVertical.LargeChange = 1;
			this.scrollVertical.Location = new System.Drawing.Point(342, 0);
			this.scrollVertical.Maximum = 0;
			this.scrollVertical.Name = "scrollVertical";
			this.scrollVertical.Size = new System.Drawing.Size(16, 250);
			this.scrollVertical.TabIndex = 18;
			this.scrollVertical.Scroll += new System.Windows.Forms.ScrollEventHandler(this.scrollVertical_Scroll);
			// 
			// checkBox1
			// 
			this.checkBox1.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox1.BackColor = System.Drawing.Color.White;
			this.checkBox1.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox1.ForeColor = System.Drawing.Color.Black;
			this.checkBox1.Location = new System.Drawing.Point(7, 3);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(35, 35);
			this.checkBox1.TabIndex = 0;
			this.checkBox1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox1.UseMnemonic = false;
			this.checkBox1.UseVisualStyleBackColor = false;
			// 
			// checkBox2
			// 
			this.checkBox2.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox2.BackColor = System.Drawing.Color.White;
			this.checkBox2.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox2.ForeColor = System.Drawing.Color.Black;
			this.checkBox2.Location = new System.Drawing.Point(49, 3);
			this.checkBox2.Name = "checkBox2";
			this.checkBox2.Size = new System.Drawing.Size(35, 35);
			this.checkBox2.TabIndex = 1;
			this.checkBox2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox2.UseMnemonic = false;
			this.checkBox2.UseVisualStyleBackColor = false;
			// 
			// checkBox3
			// 
			this.checkBox3.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox3.BackColor = System.Drawing.Color.White;
			this.checkBox3.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox3.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox3.ForeColor = System.Drawing.Color.Black;
			this.checkBox3.Location = new System.Drawing.Point(91, 3);
			this.checkBox3.Name = "checkBox3";
			this.checkBox3.Size = new System.Drawing.Size(35, 35);
			this.checkBox3.TabIndex = 2;
			this.checkBox3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox3.UseMnemonic = false;
			this.checkBox3.UseVisualStyleBackColor = false;
			// 
			// checkBox4
			// 
			this.checkBox4.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox4.BackColor = System.Drawing.Color.White;
			this.checkBox4.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox4.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox4.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox4.ForeColor = System.Drawing.Color.Black;
			this.checkBox4.Location = new System.Drawing.Point(133, 3);
			this.checkBox4.Name = "checkBox4";
			this.checkBox4.Size = new System.Drawing.Size(35, 35);
			this.checkBox4.TabIndex = 3;
			this.checkBox4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox4.UseMnemonic = false;
			this.checkBox4.UseVisualStyleBackColor = false;
			// 
			// checkBox5
			// 
			this.checkBox5.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox5.BackColor = System.Drawing.Color.White;
			this.checkBox5.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox5.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox5.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox5.ForeColor = System.Drawing.Color.Black;
			this.checkBox5.Location = new System.Drawing.Point(175, 3);
			this.checkBox5.Name = "checkBox5";
			this.checkBox5.Size = new System.Drawing.Size(35, 35);
			this.checkBox5.TabIndex = 4;
			this.checkBox5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox5.UseMnemonic = false;
			this.checkBox5.UseVisualStyleBackColor = false;
			// 
			// checkBox6
			// 
			this.checkBox6.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox6.BackColor = System.Drawing.Color.White;
			this.checkBox6.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox6.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox6.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox6.ForeColor = System.Drawing.Color.Black;
			this.checkBox6.Location = new System.Drawing.Point(217, 3);
			this.checkBox6.Name = "checkBox6";
			this.checkBox6.Size = new System.Drawing.Size(35, 35);
			this.checkBox6.TabIndex = 5;
			this.checkBox6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox6.UseMnemonic = false;
			this.checkBox6.UseVisualStyleBackColor = false;
			// 
			// checkBox7
			// 
			this.checkBox7.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox7.BackColor = System.Drawing.Color.White;
			this.checkBox7.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox7.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox7.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox7.ForeColor = System.Drawing.Color.Black;
			this.checkBox7.Location = new System.Drawing.Point(259, 3);
			this.checkBox7.Name = "checkBox7";
			this.checkBox7.Size = new System.Drawing.Size(35, 35);
			this.checkBox7.TabIndex = 6;
			this.checkBox7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox7.UseMnemonic = false;
			this.checkBox7.UseVisualStyleBackColor = false;
			// 
			// checkBox8
			// 
			this.checkBox8.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox8.BackColor = System.Drawing.Color.White;
			this.checkBox8.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox8.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox8.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox8.ForeColor = System.Drawing.Color.Black;
			this.checkBox8.Location = new System.Drawing.Point(301, 3);
			this.checkBox8.Name = "checkBox8";
			this.checkBox8.Size = new System.Drawing.Size(35, 35);
			this.checkBox8.TabIndex = 7;
			this.checkBox8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox8.UseMnemonic = false;
			this.checkBox8.UseVisualStyleBackColor = false;
			// 
			// checkBox16
			// 
			this.checkBox16.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox16.BackColor = System.Drawing.Color.White;
			this.checkBox16.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox16.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox16.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox16.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox16.ForeColor = System.Drawing.Color.Black;
			this.checkBox16.Location = new System.Drawing.Point(301, 45);
			this.checkBox16.Name = "checkBox16";
			this.checkBox16.Size = new System.Drawing.Size(35, 35);
			this.checkBox16.TabIndex = 15;
			this.checkBox16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox16.UseMnemonic = false;
			this.checkBox16.UseVisualStyleBackColor = false;
			// 
			// checkBox15
			// 
			this.checkBox15.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox15.BackColor = System.Drawing.Color.White;
			this.checkBox15.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox15.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox15.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox15.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox15.ForeColor = System.Drawing.Color.Black;
			this.checkBox15.Location = new System.Drawing.Point(259, 45);
			this.checkBox15.Name = "checkBox15";
			this.checkBox15.Size = new System.Drawing.Size(35, 35);
			this.checkBox15.TabIndex = 14;
			this.checkBox15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox15.UseMnemonic = false;
			this.checkBox15.UseVisualStyleBackColor = false;
			// 
			// checkBox14
			// 
			this.checkBox14.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox14.BackColor = System.Drawing.Color.White;
			this.checkBox14.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox14.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox14.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox14.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox14.ForeColor = System.Drawing.Color.Black;
			this.checkBox14.Location = new System.Drawing.Point(217, 45);
			this.checkBox14.Name = "checkBox14";
			this.checkBox14.Size = new System.Drawing.Size(35, 35);
			this.checkBox14.TabIndex = 13;
			this.checkBox14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox14.UseMnemonic = false;
			this.checkBox14.UseVisualStyleBackColor = false;
			// 
			// checkBox13
			// 
			this.checkBox13.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox13.BackColor = System.Drawing.Color.White;
			this.checkBox13.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox13.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox13.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox13.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox13.ForeColor = System.Drawing.Color.Black;
			this.checkBox13.Location = new System.Drawing.Point(175, 45);
			this.checkBox13.Name = "checkBox13";
			this.checkBox13.Size = new System.Drawing.Size(35, 35);
			this.checkBox13.TabIndex = 12;
			this.checkBox13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox13.UseMnemonic = false;
			this.checkBox13.UseVisualStyleBackColor = false;
			// 
			// checkBox11
			// 
			this.checkBox11.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox11.BackColor = System.Drawing.Color.White;
			this.checkBox11.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox11.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox11.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox11.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox11.ForeColor = System.Drawing.Color.Black;
			this.checkBox11.Location = new System.Drawing.Point(91, 45);
			this.checkBox11.Name = "checkBox11";
			this.checkBox11.Size = new System.Drawing.Size(35, 35);
			this.checkBox11.TabIndex = 10;
			this.checkBox11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox11.UseMnemonic = false;
			this.checkBox11.UseVisualStyleBackColor = false;
			// 
			// checkBox12
			// 
			this.checkBox12.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox12.BackColor = System.Drawing.Color.White;
			this.checkBox12.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox12.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox12.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox12.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox12.ForeColor = System.Drawing.Color.Black;
			this.checkBox12.Location = new System.Drawing.Point(133, 45);
			this.checkBox12.Name = "checkBox12";
			this.checkBox12.Size = new System.Drawing.Size(35, 35);
			this.checkBox12.TabIndex = 11;
			this.checkBox12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox12.UseMnemonic = false;
			this.checkBox12.UseVisualStyleBackColor = false;
			// 
			// checkBox10
			// 
			this.checkBox10.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox10.BackColor = System.Drawing.Color.White;
			this.checkBox10.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox10.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox10.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox10.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox10.ForeColor = System.Drawing.Color.Black;
			this.checkBox10.Location = new System.Drawing.Point(49, 45);
			this.checkBox10.Name = "checkBox10";
			this.checkBox10.Size = new System.Drawing.Size(35, 35);
			this.checkBox10.TabIndex = 9;
			this.checkBox10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox10.UseMnemonic = false;
			this.checkBox10.UseVisualStyleBackColor = false;
			// 
			// checkBox9
			// 
			this.checkBox9.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox9.BackColor = System.Drawing.Color.White;
			this.checkBox9.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox9.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox9.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox9.ForeColor = System.Drawing.Color.Black;
			this.checkBox9.Location = new System.Drawing.Point(7, 45);
			this.checkBox9.Name = "checkBox9";
			this.checkBox9.Size = new System.Drawing.Size(35, 35);
			this.checkBox9.TabIndex = 8;
			this.checkBox9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox9.UseMnemonic = false;
			this.checkBox9.UseVisualStyleBackColor = false;
			// 
			// checkBox24
			// 
			this.checkBox24.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox24.BackColor = System.Drawing.Color.White;
			this.checkBox24.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox24.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox24.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox24.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox24.ForeColor = System.Drawing.Color.Black;
			this.checkBox24.Location = new System.Drawing.Point(301, 87);
			this.checkBox24.Name = "checkBox24";
			this.checkBox24.Size = new System.Drawing.Size(35, 35);
			this.checkBox24.TabIndex = 23;
			this.checkBox24.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox24.UseMnemonic = false;
			this.checkBox24.UseVisualStyleBackColor = false;
			// 
			// checkBox23
			// 
			this.checkBox23.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox23.BackColor = System.Drawing.Color.White;
			this.checkBox23.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox23.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox23.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox23.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox23.ForeColor = System.Drawing.Color.Black;
			this.checkBox23.Location = new System.Drawing.Point(259, 87);
			this.checkBox23.Name = "checkBox23";
			this.checkBox23.Size = new System.Drawing.Size(35, 35);
			this.checkBox23.TabIndex = 22;
			this.checkBox23.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox23.UseMnemonic = false;
			this.checkBox23.UseVisualStyleBackColor = false;
			// 
			// checkBox22
			// 
			this.checkBox22.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox22.BackColor = System.Drawing.Color.White;
			this.checkBox22.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox22.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox22.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox22.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox22.ForeColor = System.Drawing.Color.Black;
			this.checkBox22.Location = new System.Drawing.Point(217, 87);
			this.checkBox22.Name = "checkBox22";
			this.checkBox22.Size = new System.Drawing.Size(35, 35);
			this.checkBox22.TabIndex = 21;
			this.checkBox22.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox22.UseMnemonic = false;
			this.checkBox22.UseVisualStyleBackColor = false;
			// 
			// checkBox21
			// 
			this.checkBox21.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox21.BackColor = System.Drawing.Color.White;
			this.checkBox21.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox21.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox21.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox21.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox21.ForeColor = System.Drawing.Color.Black;
			this.checkBox21.Location = new System.Drawing.Point(175, 87);
			this.checkBox21.Name = "checkBox21";
			this.checkBox21.Size = new System.Drawing.Size(35, 35);
			this.checkBox21.TabIndex = 20;
			this.checkBox21.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox21.UseMnemonic = false;
			this.checkBox21.UseVisualStyleBackColor = false;
			// 
			// checkBox19
			// 
			this.checkBox19.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox19.BackColor = System.Drawing.Color.White;
			this.checkBox19.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox19.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox19.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox19.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox19.ForeColor = System.Drawing.Color.Black;
			this.checkBox19.Location = new System.Drawing.Point(91, 87);
			this.checkBox19.Name = "checkBox19";
			this.checkBox19.Size = new System.Drawing.Size(35, 35);
			this.checkBox19.TabIndex = 18;
			this.checkBox19.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox19.UseMnemonic = false;
			this.checkBox19.UseVisualStyleBackColor = false;
			// 
			// checkBox20
			// 
			this.checkBox20.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox20.BackColor = System.Drawing.Color.White;
			this.checkBox20.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox20.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox20.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox20.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox20.ForeColor = System.Drawing.Color.Black;
			this.checkBox20.Location = new System.Drawing.Point(133, 87);
			this.checkBox20.Name = "checkBox20";
			this.checkBox20.Size = new System.Drawing.Size(35, 35);
			this.checkBox20.TabIndex = 19;
			this.checkBox20.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox20.UseMnemonic = false;
			this.checkBox20.UseVisualStyleBackColor = false;
			// 
			// checkBox18
			// 
			this.checkBox18.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox18.BackColor = System.Drawing.Color.White;
			this.checkBox18.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox18.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox18.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox18.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox18.ForeColor = System.Drawing.Color.Black;
			this.checkBox18.Location = new System.Drawing.Point(49, 87);
			this.checkBox18.Name = "checkBox18";
			this.checkBox18.Size = new System.Drawing.Size(35, 35);
			this.checkBox18.TabIndex = 17;
			this.checkBox18.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox18.UseMnemonic = false;
			this.checkBox18.UseVisualStyleBackColor = false;
			// 
			// checkBox17
			// 
			this.checkBox17.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox17.BackColor = System.Drawing.Color.White;
			this.checkBox17.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox17.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox17.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox17.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox17.ForeColor = System.Drawing.Color.Black;
			this.checkBox17.Location = new System.Drawing.Point(7, 87);
			this.checkBox17.Name = "checkBox17";
			this.checkBox17.Size = new System.Drawing.Size(35, 35);
			this.checkBox17.TabIndex = 16;
			this.checkBox17.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox17.UseMnemonic = false;
			this.checkBox17.UseVisualStyleBackColor = false;
			// 
			// checkBox32
			// 
			this.checkBox32.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox32.BackColor = System.Drawing.Color.White;
			this.checkBox32.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox32.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox32.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox32.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox32.ForeColor = System.Drawing.Color.Black;
			this.checkBox32.Location = new System.Drawing.Point(301, 128);
			this.checkBox32.Name = "checkBox32";
			this.checkBox32.Size = new System.Drawing.Size(35, 35);
			this.checkBox32.TabIndex = 31;
			this.checkBox32.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox32.UseMnemonic = false;
			this.checkBox32.UseVisualStyleBackColor = false;
			// 
			// checkBox31
			// 
			this.checkBox31.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox31.BackColor = System.Drawing.Color.White;
			this.checkBox31.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox31.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox31.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox31.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox31.ForeColor = System.Drawing.Color.Black;
			this.checkBox31.Location = new System.Drawing.Point(259, 128);
			this.checkBox31.Name = "checkBox31";
			this.checkBox31.Size = new System.Drawing.Size(35, 35);
			this.checkBox31.TabIndex = 30;
			this.checkBox31.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox31.UseMnemonic = false;
			this.checkBox31.UseVisualStyleBackColor = false;
			// 
			// checkBox30
			// 
			this.checkBox30.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox30.BackColor = System.Drawing.Color.White;
			this.checkBox30.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox30.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox30.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox30.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox30.ForeColor = System.Drawing.Color.Black;
			this.checkBox30.Location = new System.Drawing.Point(217, 128);
			this.checkBox30.Name = "checkBox30";
			this.checkBox30.Size = new System.Drawing.Size(35, 35);
			this.checkBox30.TabIndex = 29;
			this.checkBox30.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox30.UseMnemonic = false;
			this.checkBox30.UseVisualStyleBackColor = false;
			// 
			// checkBox29
			// 
			this.checkBox29.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox29.BackColor = System.Drawing.Color.White;
			this.checkBox29.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox29.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox29.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox29.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox29.ForeColor = System.Drawing.Color.Black;
			this.checkBox29.Location = new System.Drawing.Point(175, 128);
			this.checkBox29.Name = "checkBox29";
			this.checkBox29.Size = new System.Drawing.Size(35, 35);
			this.checkBox29.TabIndex = 28;
			this.checkBox29.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox29.UseMnemonic = false;
			this.checkBox29.UseVisualStyleBackColor = false;
			// 
			// checkBox27
			// 
			this.checkBox27.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox27.BackColor = System.Drawing.Color.White;
			this.checkBox27.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox27.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox27.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox27.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox27.ForeColor = System.Drawing.Color.Black;
			this.checkBox27.Location = new System.Drawing.Point(91, 128);
			this.checkBox27.Name = "checkBox27";
			this.checkBox27.Size = new System.Drawing.Size(35, 35);
			this.checkBox27.TabIndex = 26;
			this.checkBox27.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox27.UseMnemonic = false;
			this.checkBox27.UseVisualStyleBackColor = false;
			// 
			// checkBox28
			// 
			this.checkBox28.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox28.BackColor = System.Drawing.Color.White;
			this.checkBox28.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox28.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox28.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox28.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox28.ForeColor = System.Drawing.Color.Black;
			this.checkBox28.Location = new System.Drawing.Point(133, 128);
			this.checkBox28.Name = "checkBox28";
			this.checkBox28.Size = new System.Drawing.Size(35, 35);
			this.checkBox28.TabIndex = 27;
			this.checkBox28.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox28.UseMnemonic = false;
			this.checkBox28.UseVisualStyleBackColor = false;
			// 
			// checkBox26
			// 
			this.checkBox26.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox26.BackColor = System.Drawing.Color.White;
			this.checkBox26.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox26.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox26.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox26.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox26.ForeColor = System.Drawing.Color.Black;
			this.checkBox26.Location = new System.Drawing.Point(49, 128);
			this.checkBox26.Name = "checkBox26";
			this.checkBox26.Size = new System.Drawing.Size(35, 35);
			this.checkBox26.TabIndex = 25;
			this.checkBox26.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox26.UseMnemonic = false;
			this.checkBox26.UseVisualStyleBackColor = false;
			// 
			// checkBox25
			// 
			this.checkBox25.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox25.BackColor = System.Drawing.Color.White;
			this.checkBox25.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox25.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox25.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox25.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox25.ForeColor = System.Drawing.Color.Black;
			this.checkBox25.Location = new System.Drawing.Point(7, 128);
			this.checkBox25.Name = "checkBox25";
			this.checkBox25.Size = new System.Drawing.Size(35, 35);
			this.checkBox25.TabIndex = 24;
			this.checkBox25.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox25.UseMnemonic = false;
			this.checkBox25.UseVisualStyleBackColor = false;
			// 
			// checkBox40
			// 
			this.checkBox40.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox40.BackColor = System.Drawing.Color.White;
			this.checkBox40.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox40.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox40.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox40.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox40.ForeColor = System.Drawing.Color.Black;
			this.checkBox40.Location = new System.Drawing.Point(301, 170);
			this.checkBox40.Name = "checkBox40";
			this.checkBox40.Size = new System.Drawing.Size(35, 35);
			this.checkBox40.TabIndex = 39;
			this.checkBox40.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox40.UseMnemonic = false;
			this.checkBox40.UseVisualStyleBackColor = false;
			// 
			// checkBox39
			// 
			this.checkBox39.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox39.BackColor = System.Drawing.Color.White;
			this.checkBox39.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox39.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox39.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox39.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox39.ForeColor = System.Drawing.Color.Black;
			this.checkBox39.Location = new System.Drawing.Point(259, 170);
			this.checkBox39.Name = "checkBox39";
			this.checkBox39.Size = new System.Drawing.Size(35, 35);
			this.checkBox39.TabIndex = 38;
			this.checkBox39.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox39.UseMnemonic = false;
			this.checkBox39.UseVisualStyleBackColor = false;
			// 
			// checkBox38
			// 
			this.checkBox38.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox38.BackColor = System.Drawing.Color.White;
			this.checkBox38.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox38.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox38.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox38.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox38.ForeColor = System.Drawing.Color.Black;
			this.checkBox38.Location = new System.Drawing.Point(217, 170);
			this.checkBox38.Name = "checkBox38";
			this.checkBox38.Size = new System.Drawing.Size(35, 35);
			this.checkBox38.TabIndex = 37;
			this.checkBox38.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox38.UseMnemonic = false;
			this.checkBox38.UseVisualStyleBackColor = false;
			// 
			// checkBox37
			// 
			this.checkBox37.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox37.BackColor = System.Drawing.Color.White;
			this.checkBox37.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox37.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox37.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox37.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox37.ForeColor = System.Drawing.Color.Black;
			this.checkBox37.Location = new System.Drawing.Point(175, 170);
			this.checkBox37.Name = "checkBox37";
			this.checkBox37.Size = new System.Drawing.Size(35, 35);
			this.checkBox37.TabIndex = 36;
			this.checkBox37.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox37.UseMnemonic = false;
			this.checkBox37.UseVisualStyleBackColor = false;
			// 
			// checkBox35
			// 
			this.checkBox35.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox35.BackColor = System.Drawing.Color.White;
			this.checkBox35.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox35.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox35.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox35.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox35.ForeColor = System.Drawing.Color.Black;
			this.checkBox35.Location = new System.Drawing.Point(91, 170);
			this.checkBox35.Name = "checkBox35";
			this.checkBox35.Size = new System.Drawing.Size(35, 35);
			this.checkBox35.TabIndex = 34;
			this.checkBox35.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox35.UseMnemonic = false;
			this.checkBox35.UseVisualStyleBackColor = false;
			// 
			// checkBox36
			// 
			this.checkBox36.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox36.BackColor = System.Drawing.Color.White;
			this.checkBox36.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox36.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox36.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox36.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox36.ForeColor = System.Drawing.Color.Black;
			this.checkBox36.Location = new System.Drawing.Point(133, 170);
			this.checkBox36.Name = "checkBox36";
			this.checkBox36.Size = new System.Drawing.Size(35, 35);
			this.checkBox36.TabIndex = 35;
			this.checkBox36.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox36.UseMnemonic = false;
			this.checkBox36.UseVisualStyleBackColor = false;
			// 
			// checkBox34
			// 
			this.checkBox34.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox34.BackColor = System.Drawing.Color.White;
			this.checkBox34.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox34.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox34.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox34.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox34.ForeColor = System.Drawing.Color.Black;
			this.checkBox34.Location = new System.Drawing.Point(49, 170);
			this.checkBox34.Name = "checkBox34";
			this.checkBox34.Size = new System.Drawing.Size(35, 35);
			this.checkBox34.TabIndex = 33;
			this.checkBox34.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox34.UseMnemonic = false;
			this.checkBox34.UseVisualStyleBackColor = false;
			// 
			// checkBox33
			// 
			this.checkBox33.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox33.BackColor = System.Drawing.Color.White;
			this.checkBox33.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox33.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox33.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox33.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox33.ForeColor = System.Drawing.Color.Black;
			this.checkBox33.Location = new System.Drawing.Point(7, 170);
			this.checkBox33.Name = "checkBox33";
			this.checkBox33.Size = new System.Drawing.Size(35, 35);
			this.checkBox33.TabIndex = 32;
			this.checkBox33.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox33.UseMnemonic = false;
			this.checkBox33.UseVisualStyleBackColor = false;
			// 
			// checkBox48
			// 
			this.checkBox48.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox48.BackColor = System.Drawing.Color.White;
			this.checkBox48.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox48.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox48.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox48.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox48.ForeColor = System.Drawing.Color.Black;
			this.checkBox48.Location = new System.Drawing.Point(301, 211);
			this.checkBox48.Name = "checkBox48";
			this.checkBox48.Size = new System.Drawing.Size(35, 35);
			this.checkBox48.TabIndex = 47;
			this.checkBox48.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox48.UseMnemonic = false;
			this.checkBox48.UseVisualStyleBackColor = false;
			// 
			// checkBox47
			// 
			this.checkBox47.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox47.BackColor = System.Drawing.Color.White;
			this.checkBox47.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox47.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox47.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox47.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox47.ForeColor = System.Drawing.Color.Black;
			this.checkBox47.Location = new System.Drawing.Point(259, 211);
			this.checkBox47.Name = "checkBox47";
			this.checkBox47.Size = new System.Drawing.Size(35, 35);
			this.checkBox47.TabIndex = 46;
			this.checkBox47.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox47.UseMnemonic = false;
			this.checkBox47.UseVisualStyleBackColor = false;
			// 
			// checkBox46
			// 
			this.checkBox46.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox46.BackColor = System.Drawing.Color.White;
			this.checkBox46.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox46.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox46.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox46.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox46.ForeColor = System.Drawing.Color.Black;
			this.checkBox46.Location = new System.Drawing.Point(217, 211);
			this.checkBox46.Name = "checkBox46";
			this.checkBox46.Size = new System.Drawing.Size(35, 35);
			this.checkBox46.TabIndex = 45;
			this.checkBox46.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox46.UseMnemonic = false;
			this.checkBox46.UseVisualStyleBackColor = false;
			// 
			// checkBox45
			// 
			this.checkBox45.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox45.BackColor = System.Drawing.Color.White;
			this.checkBox45.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox45.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox45.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox45.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox45.ForeColor = System.Drawing.Color.Black;
			this.checkBox45.Location = new System.Drawing.Point(175, 211);
			this.checkBox45.Name = "checkBox45";
			this.checkBox45.Size = new System.Drawing.Size(35, 35);
			this.checkBox45.TabIndex = 44;
			this.checkBox45.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox45.UseMnemonic = false;
			this.checkBox45.UseVisualStyleBackColor = false;
			// 
			// checkBox43
			// 
			this.checkBox43.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox43.BackColor = System.Drawing.Color.White;
			this.checkBox43.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox43.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox43.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox43.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox43.ForeColor = System.Drawing.Color.Black;
			this.checkBox43.Location = new System.Drawing.Point(91, 211);
			this.checkBox43.Name = "checkBox43";
			this.checkBox43.Size = new System.Drawing.Size(35, 35);
			this.checkBox43.TabIndex = 42;
			this.checkBox43.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox43.UseMnemonic = false;
			this.checkBox43.UseVisualStyleBackColor = false;
			// 
			// checkBox44
			// 
			this.checkBox44.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox44.BackColor = System.Drawing.Color.White;
			this.checkBox44.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox44.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox44.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox44.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox44.ForeColor = System.Drawing.Color.Black;
			this.checkBox44.Location = new System.Drawing.Point(133, 211);
			this.checkBox44.Name = "checkBox44";
			this.checkBox44.Size = new System.Drawing.Size(35, 35);
			this.checkBox44.TabIndex = 43;
			this.checkBox44.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox44.UseMnemonic = false;
			this.checkBox44.UseVisualStyleBackColor = false;
			// 
			// checkBox42
			// 
			this.checkBox42.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox42.BackColor = System.Drawing.Color.White;
			this.checkBox42.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox42.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox42.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox42.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox42.ForeColor = System.Drawing.Color.Black;
			this.checkBox42.Location = new System.Drawing.Point(49, 211);
			this.checkBox42.Name = "checkBox42";
			this.checkBox42.Size = new System.Drawing.Size(35, 35);
			this.checkBox42.TabIndex = 41;
			this.checkBox42.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox42.UseMnemonic = false;
			this.checkBox42.UseVisualStyleBackColor = false;
			// 
			// checkBox41
			// 
			this.checkBox41.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox41.BackColor = System.Drawing.Color.White;
			this.checkBox41.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.checkBox41.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.checkBox41.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.checkBox41.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox41.ForeColor = System.Drawing.Color.Black;
			this.checkBox41.Location = new System.Drawing.Point(7, 211);
			this.checkBox41.Name = "checkBox41";
			this.checkBox41.Size = new System.Drawing.Size(35, 35);
			this.checkBox41.TabIndex = 40;
			this.checkBox41.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox41.UseMnemonic = false;
			this.checkBox41.UseVisualStyleBackColor = false;
			// 
			// textCharacters
			// 
			this.textCharacters.BackColor = System.Drawing.Color.White;
			this.textCharacters.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textCharacters.ForeColor = System.Drawing.Color.Black;
			this.textCharacters.Location = new System.Drawing.Point(6, 279);
			this.textCharacters.MaxLength = 65536;
			this.textCharacters.Multiline = true;
			this.textCharacters.Name = "textCharacters";
			this.textCharacters.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textCharacters.Size = new System.Drawing.Size(360, 90);
			this.textCharacters.TabIndex = 1;
			this.textCharacters.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textCharacters_KeyDown);
			this.textCharacters.Leave += new System.EventHandler(this.textCharacters_Leave);
			// 
			// labelCharacters
			// 
			this.labelCharacters.AutoSize = true;
			this.labelCharacters.ForeColor = System.Drawing.Color.White;
			this.labelCharacters.Location = new System.Drawing.Point(3, 261);
			this.labelCharacters.Name = "labelCharacters";
			this.labelCharacters.Size = new System.Drawing.Size(61, 15);
			this.labelCharacters.TabIndex = 16;
			this.labelCharacters.Text = "characters";
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonOK.ForeColor = System.Drawing.Color.White;
			this.buttonOK.Image = global::Gorgon.Editor.FontEditorPlugIn.Properties.Resources.ok_16x16;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(431, 403);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(87, 28);
			this.buttonOK.TabIndex = 0;
			this.buttonOK.Text = "ok";
			this.buttonOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonOK.UseVisualStyleBackColor = false;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.Image = global::Gorgon.Editor.FontEditorPlugIn.Properties.Resources.cancel_16x16;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(529, 403);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 28);
			this.buttonCancel.TabIndex = 1;
			this.buttonCancel.Text = "cancel";
			this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonCancel.UseVisualStyleBackColor = false;
			// 
			// FormCharacterPicker
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.Border = true;
			this.BorderColor = System.Drawing.Color.SteelBlue;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(626, 444);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ForeColor = System.Drawing.Color.Silver;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.InactiveBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormCharacterPicker";
			this.Padding = new System.Windows.Forms.Padding(1);
			this.Resizable = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Character Picker";
			this.Controls.SetChildIndex(this.buttonOK, 0);
			this.Controls.SetChildIndex(this.buttonCancel, 0);
			this.Controls.SetChildIndex(this.panel1, 0);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panelCharacters.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Button buttonCancel;
		private Button buttonOK;
		private Panel panel1;
		private VScrollBar scrollVertical;
		private Panel panelCharacters;
		private CheckBox checkBox1;
		private CheckBox checkBox2;
		private CheckBox checkBox3;
		private CheckBox checkBox4;
		private CheckBox checkBox5;
		private CheckBox checkBox6;
		private CheckBox checkBox7;
		private CheckBox checkBox8;
		private CheckBox checkBox16;
		private CheckBox checkBox15;
		private CheckBox checkBox14;
		private CheckBox checkBox13;
		private CheckBox checkBox11;
		private CheckBox checkBox12;
		private CheckBox checkBox10;
		private CheckBox checkBox9;
		private CheckBox checkBox24;
		private CheckBox checkBox23;
		private CheckBox checkBox22;
		private CheckBox checkBox21;
		private CheckBox checkBox19;
		private CheckBox checkBox20;
		private CheckBox checkBox18;
		private CheckBox checkBox17;
		private CheckBox checkBox32;
		private CheckBox checkBox31;
		private CheckBox checkBox30;
		private CheckBox checkBox29;
		private CheckBox checkBox27;
		private CheckBox checkBox28;
		private CheckBox checkBox26;
		private CheckBox checkBox25;
		private CheckBox checkBox40;
		private CheckBox checkBox39;
		private CheckBox checkBox38;
		private CheckBox checkBox37;
		private CheckBox checkBox35;
		private CheckBox checkBox36;
		private CheckBox checkBox34;
		private CheckBox checkBox33;
		private CheckBox checkBox48;
		private CheckBox checkBox47;
		private CheckBox checkBox46;
		private CheckBox checkBox45;
		private CheckBox checkBox43;
		private CheckBox checkBox44;
		private CheckBox checkBox42;
		private CheckBox checkBox41;
		private TextBox textCharacters;
		private Label labelCharacters;
		private ListView listRanges;
		private ColumnHeader columnRange;
		private ColumnHeader columnRangeName;
		private ToolTip tipChar;
		private Button buttonSelectAll;
	}
}