
namespace Gorgon.Editor.FontEditor
{
    partial class CharacterPicker
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


            if (disposing)
            {
                _characterChangedEvent = null;
                _cancelClickedEvent = null;
                _okClickedEvent = null;

                DeselectFont();

                if (_graphics is not null)
                {
                    _graphics.Dispose();
                }
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
            this.ButtonSelectAll = new System.Windows.Forms.Button();
            this.ListCharacterRanges = new System.Windows.Forms.ListView();
            this.columnRange = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnRangeName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ScrollVertical = new System.Windows.Forms.VScrollBar();
            this.panelCharacters = new System.Windows.Forms.Panel();
            this.CheckBox1 = new System.Windows.Forms.CheckBox();
            this.CheckBox2 = new System.Windows.Forms.CheckBox();
            this.CheckBox3 = new System.Windows.Forms.CheckBox();
            this.CheckBox4 = new System.Windows.Forms.CheckBox();
            this.CheckBox5 = new System.Windows.Forms.CheckBox();
            this.CheckBox6 = new System.Windows.Forms.CheckBox();
            this.CheckBox7 = new System.Windows.Forms.CheckBox();
            this.CheckBox8 = new System.Windows.Forms.CheckBox();
            this.CheckBox16 = new System.Windows.Forms.CheckBox();
            this.CheckBox15 = new System.Windows.Forms.CheckBox();
            this.CheckBox14 = new System.Windows.Forms.CheckBox();
            this.CheckBox13 = new System.Windows.Forms.CheckBox();
            this.CheckBox11 = new System.Windows.Forms.CheckBox();
            this.CheckBox12 = new System.Windows.Forms.CheckBox();
            this.CheckBox10 = new System.Windows.Forms.CheckBox();
            this.CheckBox9 = new System.Windows.Forms.CheckBox();
            this.CheckBox24 = new System.Windows.Forms.CheckBox();
            this.CheckBox23 = new System.Windows.Forms.CheckBox();
            this.CheckBox22 = new System.Windows.Forms.CheckBox();
            this.CheckBox21 = new System.Windows.Forms.CheckBox();
            this.CheckBox19 = new System.Windows.Forms.CheckBox();
            this.CheckBox20 = new System.Windows.Forms.CheckBox();
            this.CheckBox18 = new System.Windows.Forms.CheckBox();
            this.CheckBox17 = new System.Windows.Forms.CheckBox();
            this.CheckBox32 = new System.Windows.Forms.CheckBox();
            this.CheckBox31 = new System.Windows.Forms.CheckBox();
            this.CheckBox30 = new System.Windows.Forms.CheckBox();
            this.CheckBox29 = new System.Windows.Forms.CheckBox();
            this.CheckBox27 = new System.Windows.Forms.CheckBox();
            this.CheckBox28 = new System.Windows.Forms.CheckBox();
            this.CheckBox26 = new System.Windows.Forms.CheckBox();
            this.CheckBox25 = new System.Windows.Forms.CheckBox();
            this.CheckBox40 = new System.Windows.Forms.CheckBox();
            this.CheckBox39 = new System.Windows.Forms.CheckBox();
            this.CheckBox38 = new System.Windows.Forms.CheckBox();
            this.CheckBox37 = new System.Windows.Forms.CheckBox();
            this.CheckBox35 = new System.Windows.Forms.CheckBox();
            this.CheckBox36 = new System.Windows.Forms.CheckBox();
            this.CheckBox34 = new System.Windows.Forms.CheckBox();
            this.CheckBox33 = new System.Windows.Forms.CheckBox();
            this.CheckBox48 = new System.Windows.Forms.CheckBox();
            this.CheckBox47 = new System.Windows.Forms.CheckBox();
            this.CheckBox46 = new System.Windows.Forms.CheckBox();
            this.CheckBox45 = new System.Windows.Forms.CheckBox();
            this.CheckBox43 = new System.Windows.Forms.CheckBox();
            this.CheckBox44 = new System.Windows.Forms.CheckBox();
            this.CheckBox42 = new System.Windows.Forms.CheckBox();
            this.CheckBox41 = new System.Windows.Forms.CheckBox();
            this.LabelSelectedCharacters = new System.Windows.Forms.Label();
            this.PanelButtons = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.ButtonOK = new System.Windows.Forms.Button();
            this.TipChar = new System.Windows.Forms.ToolTip(this.components);
            this.TextSelectedCharacters = new System.Windows.Forms.TextBox();
            this.LeftTable = new System.Windows.Forms.TableLayoutPanel();
            this.RightTable = new System.Windows.Forms.TableLayoutPanel();
            this.TableControls = new System.Windows.Forms.TableLayoutPanel();
            this.panelCharacters.SuspendLayout();
            this.PanelButtons.SuspendLayout();
            this.LeftTable.SuspendLayout();
            this.RightTable.SuspendLayout();
            this.TableControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // ButtonSelectAll
            // 
            this.ButtonSelectAll.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.ButtonSelectAll.AutoSize = true;
            this.ButtonSelectAll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonSelectAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.ButtonSelectAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ButtonSelectAll.FlatAppearance.CheckedBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonSelectAll.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonSelectAll.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonSelectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonSelectAll.Location = new System.Drawing.Point(83, 462);
            this.ButtonSelectAll.MinimumSize = new System.Drawing.Size(96, 0);
            this.ButtonSelectAll.Name = "ButtonSelectAll";
            this.ButtonSelectAll.Size = new System.Drawing.Size(96, 27);
            this.ButtonSelectAll.TabIndex = 1;
            this.ButtonSelectAll.Text = "Select All";
            this.ButtonSelectAll.UseVisualStyleBackColor = false;
            this.ButtonSelectAll.Click += new System.EventHandler(this.ButtonSelectAll_Click);
            // 
            // ListCharacterRanges
            // 
            this.ListCharacterRanges.BackColor = System.Drawing.Color.White;
            this.ListCharacterRanges.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ListCharacterRanges.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnRange,
            this.columnRangeName});
            this.ListCharacterRanges.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListCharacterRanges.ForeColor = System.Drawing.Color.Black;
            this.ListCharacterRanges.FullRowSelect = true;
            this.ListCharacterRanges.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.ListCharacterRanges.HideSelection = false;
            this.ListCharacterRanges.Location = new System.Drawing.Point(3, 3);
            this.ListCharacterRanges.MultiSelect = false;
            this.ListCharacterRanges.Name = "ListCharacterRanges";
            this.ListCharacterRanges.Size = new System.Drawing.Size(256, 453);
            this.ListCharacterRanges.TabIndex = 0;
            this.ListCharacterRanges.UseCompatibleStateImageBehavior = false;
            this.ListCharacterRanges.View = System.Windows.Forms.View.Details;
            this.ListCharacterRanges.SelectedIndexChanged += new System.EventHandler(this.ListCharacterRanges_SelectedIndexChanged);
            // 
            // columnRange
            // 
            this.columnRange.Text = "Range";
            // 
            // columnRangeName
            // 
            this.columnRangeName.Text = "Name";
            // 
            // ScrollVertical
            // 
            this.ScrollVertical.LargeChange = 1;
            this.ScrollVertical.Location = new System.Drawing.Point(392, 0);
            this.ScrollVertical.Maximum = 0;
            this.ScrollVertical.Name = "ScrollVertical";
            this.ScrollVertical.Size = new System.Drawing.Size(17, 251);
            this.ScrollVertical.TabIndex = 48;
            this.ScrollVertical.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ScrollVertical_Scroll);
            // 
            // panelCharacters
            // 
            this.panelCharacters.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.panelCharacters.AutoSize = true;
            this.panelCharacters.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCharacters.Controls.Add(this.CheckBox1);
            this.panelCharacters.Controls.Add(this.CheckBox2);
            this.panelCharacters.Controls.Add(this.CheckBox3);
            this.panelCharacters.Controls.Add(this.CheckBox4);
            this.panelCharacters.Controls.Add(this.CheckBox5);
            this.panelCharacters.Controls.Add(this.CheckBox6);
            this.panelCharacters.Controls.Add(this.CheckBox7);
            this.panelCharacters.Controls.Add(this.CheckBox8);
            this.panelCharacters.Controls.Add(this.CheckBox16);
            this.panelCharacters.Controls.Add(this.CheckBox15);
            this.panelCharacters.Controls.Add(this.CheckBox14);
            this.panelCharacters.Controls.Add(this.CheckBox13);
            this.panelCharacters.Controls.Add(this.CheckBox11);
            this.panelCharacters.Controls.Add(this.CheckBox12);
            this.panelCharacters.Controls.Add(this.CheckBox10);
            this.panelCharacters.Controls.Add(this.CheckBox9);
            this.panelCharacters.Controls.Add(this.CheckBox24);
            this.panelCharacters.Controls.Add(this.CheckBox23);
            this.panelCharacters.Controls.Add(this.CheckBox22);
            this.panelCharacters.Controls.Add(this.CheckBox21);
            this.panelCharacters.Controls.Add(this.CheckBox19);
            this.panelCharacters.Controls.Add(this.CheckBox20);
            this.panelCharacters.Controls.Add(this.CheckBox18);
            this.panelCharacters.Controls.Add(this.CheckBox17);
            this.panelCharacters.Controls.Add(this.CheckBox32);
            this.panelCharacters.Controls.Add(this.CheckBox31);
            this.panelCharacters.Controls.Add(this.CheckBox30);
            this.panelCharacters.Controls.Add(this.CheckBox29);
            this.panelCharacters.Controls.Add(this.CheckBox27);
            this.panelCharacters.Controls.Add(this.CheckBox28);
            this.panelCharacters.Controls.Add(this.CheckBox26);
            this.panelCharacters.Controls.Add(this.CheckBox25);
            this.panelCharacters.Controls.Add(this.CheckBox40);
            this.panelCharacters.Controls.Add(this.CheckBox39);
            this.panelCharacters.Controls.Add(this.CheckBox38);
            this.panelCharacters.Controls.Add(this.CheckBox37);
            this.panelCharacters.Controls.Add(this.CheckBox35);
            this.panelCharacters.Controls.Add(this.CheckBox36);
            this.panelCharacters.Controls.Add(this.CheckBox34);
            this.panelCharacters.Controls.Add(this.CheckBox33);
            this.panelCharacters.Controls.Add(this.CheckBox48);
            this.panelCharacters.Controls.Add(this.CheckBox47);
            this.panelCharacters.Controls.Add(this.CheckBox46);
            this.panelCharacters.Controls.Add(this.CheckBox45);
            this.panelCharacters.Controls.Add(this.CheckBox43);
            this.panelCharacters.Controls.Add(this.CheckBox44);
            this.panelCharacters.Controls.Add(this.CheckBox42);
            this.panelCharacters.Controls.Add(this.CheckBox41);
            this.panelCharacters.ForeColor = System.Drawing.Color.White;
            this.panelCharacters.Location = new System.Drawing.Point(0, 0);
            this.panelCharacters.Margin = new System.Windows.Forms.Padding(0);
            this.panelCharacters.MaximumSize = new System.Drawing.Size(392, 251);
            this.panelCharacters.MinimumSize = new System.Drawing.Size(392, 251);
            this.panelCharacters.Name = "panelCharacters";
            this.panelCharacters.Size = new System.Drawing.Size(392, 251);
            this.panelCharacters.TabIndex = 0;
            // 
            // CheckBox1
            // 
            this.CheckBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox1.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox1.BackColor = System.Drawing.Color.White;
            this.CheckBox1.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox1.ForeColor = System.Drawing.Color.Black;
            this.CheckBox1.Location = new System.Drawing.Point(31, 3);
            this.CheckBox1.Name = "CheckBox1";
            this.CheckBox1.Size = new System.Drawing.Size(35, 35);
            this.CheckBox1.TabIndex = 0;
            this.CheckBox1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox1.UseMnemonic = false;
            this.CheckBox1.UseVisualStyleBackColor = false;
            // 
            // CheckBox2
            // 
            this.CheckBox2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox2.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox2.BackColor = System.Drawing.Color.White;
            this.CheckBox2.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox2.ForeColor = System.Drawing.Color.Black;
            this.CheckBox2.Location = new System.Drawing.Point(73, 3);
            this.CheckBox2.Name = "CheckBox2";
            this.CheckBox2.Size = new System.Drawing.Size(35, 35);
            this.CheckBox2.TabIndex = 1;
            this.CheckBox2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox2.UseMnemonic = false;
            this.CheckBox2.UseVisualStyleBackColor = false;
            // 
            // CheckBox3
            // 
            this.CheckBox3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox3.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox3.BackColor = System.Drawing.Color.White;
            this.CheckBox3.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox3.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox3.ForeColor = System.Drawing.Color.Black;
            this.CheckBox3.Location = new System.Drawing.Point(115, 3);
            this.CheckBox3.Name = "CheckBox3";
            this.CheckBox3.Size = new System.Drawing.Size(35, 35);
            this.CheckBox3.TabIndex = 2;
            this.CheckBox3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox3.UseMnemonic = false;
            this.CheckBox3.UseVisualStyleBackColor = false;
            // 
            // CheckBox4
            // 
            this.CheckBox4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox4.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox4.BackColor = System.Drawing.Color.White;
            this.CheckBox4.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox4.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox4.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox4.ForeColor = System.Drawing.Color.Black;
            this.CheckBox4.Location = new System.Drawing.Point(157, 3);
            this.CheckBox4.Name = "CheckBox4";
            this.CheckBox4.Size = new System.Drawing.Size(35, 35);
            this.CheckBox4.TabIndex = 3;
            this.CheckBox4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox4.UseMnemonic = false;
            this.CheckBox4.UseVisualStyleBackColor = false;
            // 
            // CheckBox5
            // 
            this.CheckBox5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox5.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox5.BackColor = System.Drawing.Color.White;
            this.CheckBox5.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox5.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox5.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox5.ForeColor = System.Drawing.Color.Black;
            this.CheckBox5.Location = new System.Drawing.Point(199, 3);
            this.CheckBox5.Name = "CheckBox5";
            this.CheckBox5.Size = new System.Drawing.Size(35, 35);
            this.CheckBox5.TabIndex = 4;
            this.CheckBox5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox5.UseMnemonic = false;
            this.CheckBox5.UseVisualStyleBackColor = false;
            // 
            // CheckBox6
            // 
            this.CheckBox6.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox6.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox6.BackColor = System.Drawing.Color.White;
            this.CheckBox6.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox6.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox6.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox6.ForeColor = System.Drawing.Color.Black;
            this.CheckBox6.Location = new System.Drawing.Point(241, 3);
            this.CheckBox6.Name = "CheckBox6";
            this.CheckBox6.Size = new System.Drawing.Size(35, 35);
            this.CheckBox6.TabIndex = 5;
            this.CheckBox6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox6.UseMnemonic = false;
            this.CheckBox6.UseVisualStyleBackColor = false;
            // 
            // CheckBox7
            // 
            this.CheckBox7.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox7.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox7.BackColor = System.Drawing.Color.White;
            this.CheckBox7.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox7.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox7.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox7.ForeColor = System.Drawing.Color.Black;
            this.CheckBox7.Location = new System.Drawing.Point(283, 3);
            this.CheckBox7.Name = "CheckBox7";
            this.CheckBox7.Size = new System.Drawing.Size(35, 35);
            this.CheckBox7.TabIndex = 6;
            this.CheckBox7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox7.UseMnemonic = false;
            this.CheckBox7.UseVisualStyleBackColor = false;
            // 
            // CheckBox8
            // 
            this.CheckBox8.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox8.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox8.BackColor = System.Drawing.Color.White;
            this.CheckBox8.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox8.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox8.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox8.ForeColor = System.Drawing.Color.Black;
            this.CheckBox8.Location = new System.Drawing.Point(325, 3);
            this.CheckBox8.Name = "CheckBox8";
            this.CheckBox8.Size = new System.Drawing.Size(35, 35);
            this.CheckBox8.TabIndex = 7;
            this.CheckBox8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox8.UseMnemonic = false;
            this.CheckBox8.UseVisualStyleBackColor = false;
            // 
            // CheckBox16
            // 
            this.CheckBox16.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox16.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox16.BackColor = System.Drawing.Color.White;
            this.CheckBox16.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox16.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox16.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox16.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox16.ForeColor = System.Drawing.Color.Black;
            this.CheckBox16.Location = new System.Drawing.Point(325, 45);
            this.CheckBox16.Name = "CheckBox16";
            this.CheckBox16.Size = new System.Drawing.Size(35, 35);
            this.CheckBox16.TabIndex = 15;
            this.CheckBox16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox16.UseMnemonic = false;
            this.CheckBox16.UseVisualStyleBackColor = false;
            // 
            // CheckBox15
            // 
            this.CheckBox15.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox15.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox15.BackColor = System.Drawing.Color.White;
            this.CheckBox15.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox15.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox15.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox15.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox15.ForeColor = System.Drawing.Color.Black;
            this.CheckBox15.Location = new System.Drawing.Point(283, 45);
            this.CheckBox15.Name = "CheckBox15";
            this.CheckBox15.Size = new System.Drawing.Size(35, 35);
            this.CheckBox15.TabIndex = 14;
            this.CheckBox15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox15.UseMnemonic = false;
            this.CheckBox15.UseVisualStyleBackColor = false;
            // 
            // CheckBox14
            // 
            this.CheckBox14.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox14.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox14.BackColor = System.Drawing.Color.White;
            this.CheckBox14.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox14.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox14.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox14.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox14.ForeColor = System.Drawing.Color.Black;
            this.CheckBox14.Location = new System.Drawing.Point(241, 45);
            this.CheckBox14.Name = "CheckBox14";
            this.CheckBox14.Size = new System.Drawing.Size(35, 35);
            this.CheckBox14.TabIndex = 13;
            this.CheckBox14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox14.UseMnemonic = false;
            this.CheckBox14.UseVisualStyleBackColor = false;
            // 
            // CheckBox13
            // 
            this.CheckBox13.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox13.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox13.BackColor = System.Drawing.Color.White;
            this.CheckBox13.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox13.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox13.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox13.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox13.ForeColor = System.Drawing.Color.Black;
            this.CheckBox13.Location = new System.Drawing.Point(199, 45);
            this.CheckBox13.Name = "CheckBox13";
            this.CheckBox13.Size = new System.Drawing.Size(35, 35);
            this.CheckBox13.TabIndex = 12;
            this.CheckBox13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox13.UseMnemonic = false;
            this.CheckBox13.UseVisualStyleBackColor = false;
            // 
            // CheckBox11
            // 
            this.CheckBox11.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox11.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox11.BackColor = System.Drawing.Color.White;
            this.CheckBox11.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox11.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox11.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox11.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox11.ForeColor = System.Drawing.Color.Black;
            this.CheckBox11.Location = new System.Drawing.Point(115, 45);
            this.CheckBox11.Name = "CheckBox11";
            this.CheckBox11.Size = new System.Drawing.Size(35, 35);
            this.CheckBox11.TabIndex = 10;
            this.CheckBox11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox11.UseMnemonic = false;
            this.CheckBox11.UseVisualStyleBackColor = false;
            // 
            // CheckBox12
            // 
            this.CheckBox12.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox12.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox12.BackColor = System.Drawing.Color.White;
            this.CheckBox12.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox12.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox12.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox12.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox12.ForeColor = System.Drawing.Color.Black;
            this.CheckBox12.Location = new System.Drawing.Point(157, 45);
            this.CheckBox12.Name = "CheckBox12";
            this.CheckBox12.Size = new System.Drawing.Size(35, 35);
            this.CheckBox12.TabIndex = 11;
            this.CheckBox12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox12.UseMnemonic = false;
            this.CheckBox12.UseVisualStyleBackColor = false;
            // 
            // CheckBox10
            // 
            this.CheckBox10.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox10.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox10.BackColor = System.Drawing.Color.White;
            this.CheckBox10.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox10.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox10.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox10.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox10.ForeColor = System.Drawing.Color.Black;
            this.CheckBox10.Location = new System.Drawing.Point(73, 45);
            this.CheckBox10.Name = "CheckBox10";
            this.CheckBox10.Size = new System.Drawing.Size(35, 35);
            this.CheckBox10.TabIndex = 9;
            this.CheckBox10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox10.UseMnemonic = false;
            this.CheckBox10.UseVisualStyleBackColor = false;
            // 
            // CheckBox9
            // 
            this.CheckBox9.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox9.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox9.BackColor = System.Drawing.Color.White;
            this.CheckBox9.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox9.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox9.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox9.ForeColor = System.Drawing.Color.Black;
            this.CheckBox9.Location = new System.Drawing.Point(31, 45);
            this.CheckBox9.Name = "CheckBox9";
            this.CheckBox9.Size = new System.Drawing.Size(35, 35);
            this.CheckBox9.TabIndex = 8;
            this.CheckBox9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox9.UseMnemonic = false;
            this.CheckBox9.UseVisualStyleBackColor = false;
            // 
            // CheckBox24
            // 
            this.CheckBox24.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox24.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox24.BackColor = System.Drawing.Color.White;
            this.CheckBox24.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox24.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox24.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox24.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox24.ForeColor = System.Drawing.Color.Black;
            this.CheckBox24.Location = new System.Drawing.Point(325, 87);
            this.CheckBox24.Name = "CheckBox24";
            this.CheckBox24.Size = new System.Drawing.Size(35, 35);
            this.CheckBox24.TabIndex = 23;
            this.CheckBox24.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox24.UseMnemonic = false;
            this.CheckBox24.UseVisualStyleBackColor = false;
            // 
            // CheckBox23
            // 
            this.CheckBox23.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox23.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox23.BackColor = System.Drawing.Color.White;
            this.CheckBox23.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox23.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox23.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox23.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox23.ForeColor = System.Drawing.Color.Black;
            this.CheckBox23.Location = new System.Drawing.Point(283, 87);
            this.CheckBox23.Name = "CheckBox23";
            this.CheckBox23.Size = new System.Drawing.Size(35, 35);
            this.CheckBox23.TabIndex = 22;
            this.CheckBox23.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox23.UseMnemonic = false;
            this.CheckBox23.UseVisualStyleBackColor = false;
            // 
            // CheckBox22
            // 
            this.CheckBox22.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox22.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox22.BackColor = System.Drawing.Color.White;
            this.CheckBox22.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox22.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox22.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox22.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox22.ForeColor = System.Drawing.Color.Black;
            this.CheckBox22.Location = new System.Drawing.Point(241, 87);
            this.CheckBox22.Name = "CheckBox22";
            this.CheckBox22.Size = new System.Drawing.Size(35, 35);
            this.CheckBox22.TabIndex = 21;
            this.CheckBox22.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox22.UseMnemonic = false;
            this.CheckBox22.UseVisualStyleBackColor = false;
            // 
            // CheckBox21
            // 
            this.CheckBox21.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox21.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox21.BackColor = System.Drawing.Color.White;
            this.CheckBox21.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox21.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox21.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox21.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox21.ForeColor = System.Drawing.Color.Black;
            this.CheckBox21.Location = new System.Drawing.Point(199, 87);
            this.CheckBox21.Name = "CheckBox21";
            this.CheckBox21.Size = new System.Drawing.Size(35, 35);
            this.CheckBox21.TabIndex = 20;
            this.CheckBox21.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox21.UseMnemonic = false;
            this.CheckBox21.UseVisualStyleBackColor = false;
            // 
            // CheckBox19
            // 
            this.CheckBox19.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox19.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox19.BackColor = System.Drawing.Color.White;
            this.CheckBox19.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox19.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox19.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox19.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox19.ForeColor = System.Drawing.Color.Black;
            this.CheckBox19.Location = new System.Drawing.Point(115, 87);
            this.CheckBox19.Name = "CheckBox19";
            this.CheckBox19.Size = new System.Drawing.Size(35, 35);
            this.CheckBox19.TabIndex = 18;
            this.CheckBox19.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox19.UseMnemonic = false;
            this.CheckBox19.UseVisualStyleBackColor = false;
            // 
            // CheckBox20
            // 
            this.CheckBox20.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox20.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox20.BackColor = System.Drawing.Color.White;
            this.CheckBox20.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox20.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox20.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox20.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox20.ForeColor = System.Drawing.Color.Black;
            this.CheckBox20.Location = new System.Drawing.Point(157, 87);
            this.CheckBox20.Name = "CheckBox20";
            this.CheckBox20.Size = new System.Drawing.Size(35, 35);
            this.CheckBox20.TabIndex = 19;
            this.CheckBox20.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox20.UseMnemonic = false;
            this.CheckBox20.UseVisualStyleBackColor = false;
            // 
            // CheckBox18
            // 
            this.CheckBox18.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox18.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox18.BackColor = System.Drawing.Color.White;
            this.CheckBox18.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox18.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox18.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox18.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox18.ForeColor = System.Drawing.Color.Black;
            this.CheckBox18.Location = new System.Drawing.Point(73, 87);
            this.CheckBox18.Name = "CheckBox18";
            this.CheckBox18.Size = new System.Drawing.Size(35, 35);
            this.CheckBox18.TabIndex = 17;
            this.CheckBox18.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox18.UseMnemonic = false;
            this.CheckBox18.UseVisualStyleBackColor = false;
            // 
            // CheckBox17
            // 
            this.CheckBox17.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox17.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox17.BackColor = System.Drawing.Color.White;
            this.CheckBox17.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox17.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox17.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox17.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox17.ForeColor = System.Drawing.Color.Black;
            this.CheckBox17.Location = new System.Drawing.Point(31, 87);
            this.CheckBox17.Name = "CheckBox17";
            this.CheckBox17.Size = new System.Drawing.Size(35, 35);
            this.CheckBox17.TabIndex = 16;
            this.CheckBox17.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox17.UseMnemonic = false;
            this.CheckBox17.UseVisualStyleBackColor = false;
            // 
            // CheckBox32
            // 
            this.CheckBox32.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox32.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox32.BackColor = System.Drawing.Color.White;
            this.CheckBox32.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox32.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox32.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox32.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox32.ForeColor = System.Drawing.Color.Black;
            this.CheckBox32.Location = new System.Drawing.Point(325, 128);
            this.CheckBox32.Name = "CheckBox32";
            this.CheckBox32.Size = new System.Drawing.Size(35, 35);
            this.CheckBox32.TabIndex = 31;
            this.CheckBox32.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox32.UseMnemonic = false;
            this.CheckBox32.UseVisualStyleBackColor = false;
            // 
            // CheckBox31
            // 
            this.CheckBox31.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox31.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox31.BackColor = System.Drawing.Color.White;
            this.CheckBox31.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox31.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox31.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox31.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox31.ForeColor = System.Drawing.Color.Black;
            this.CheckBox31.Location = new System.Drawing.Point(283, 128);
            this.CheckBox31.Name = "CheckBox31";
            this.CheckBox31.Size = new System.Drawing.Size(35, 35);
            this.CheckBox31.TabIndex = 30;
            this.CheckBox31.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox31.UseMnemonic = false;
            this.CheckBox31.UseVisualStyleBackColor = false;
            // 
            // CheckBox30
            // 
            this.CheckBox30.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox30.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox30.BackColor = System.Drawing.Color.White;
            this.CheckBox30.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox30.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox30.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox30.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox30.ForeColor = System.Drawing.Color.Black;
            this.CheckBox30.Location = new System.Drawing.Point(241, 128);
            this.CheckBox30.Name = "CheckBox30";
            this.CheckBox30.Size = new System.Drawing.Size(35, 35);
            this.CheckBox30.TabIndex = 29;
            this.CheckBox30.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox30.UseMnemonic = false;
            this.CheckBox30.UseVisualStyleBackColor = false;
            // 
            // CheckBox29
            // 
            this.CheckBox29.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox29.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox29.BackColor = System.Drawing.Color.White;
            this.CheckBox29.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox29.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox29.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox29.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox29.ForeColor = System.Drawing.Color.Black;
            this.CheckBox29.Location = new System.Drawing.Point(199, 128);
            this.CheckBox29.Name = "CheckBox29";
            this.CheckBox29.Size = new System.Drawing.Size(35, 35);
            this.CheckBox29.TabIndex = 28;
            this.CheckBox29.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox29.UseMnemonic = false;
            this.CheckBox29.UseVisualStyleBackColor = false;
            // 
            // CheckBox27
            // 
            this.CheckBox27.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox27.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox27.BackColor = System.Drawing.Color.White;
            this.CheckBox27.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox27.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox27.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox27.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox27.ForeColor = System.Drawing.Color.Black;
            this.CheckBox27.Location = new System.Drawing.Point(115, 128);
            this.CheckBox27.Name = "CheckBox27";
            this.CheckBox27.Size = new System.Drawing.Size(35, 35);
            this.CheckBox27.TabIndex = 26;
            this.CheckBox27.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox27.UseMnemonic = false;
            this.CheckBox27.UseVisualStyleBackColor = false;
            // 
            // CheckBox28
            // 
            this.CheckBox28.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox28.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox28.BackColor = System.Drawing.Color.White;
            this.CheckBox28.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox28.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox28.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox28.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox28.ForeColor = System.Drawing.Color.Black;
            this.CheckBox28.Location = new System.Drawing.Point(157, 128);
            this.CheckBox28.Name = "CheckBox28";
            this.CheckBox28.Size = new System.Drawing.Size(35, 35);
            this.CheckBox28.TabIndex = 27;
            this.CheckBox28.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox28.UseMnemonic = false;
            this.CheckBox28.UseVisualStyleBackColor = false;
            // 
            // CheckBox26
            // 
            this.CheckBox26.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox26.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox26.BackColor = System.Drawing.Color.White;
            this.CheckBox26.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox26.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox26.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox26.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox26.ForeColor = System.Drawing.Color.Black;
            this.CheckBox26.Location = new System.Drawing.Point(73, 128);
            this.CheckBox26.Name = "CheckBox26";
            this.CheckBox26.Size = new System.Drawing.Size(35, 35);
            this.CheckBox26.TabIndex = 25;
            this.CheckBox26.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox26.UseMnemonic = false;
            this.CheckBox26.UseVisualStyleBackColor = false;
            // 
            // CheckBox25
            // 
            this.CheckBox25.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox25.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox25.BackColor = System.Drawing.Color.White;
            this.CheckBox25.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox25.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox25.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox25.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox25.ForeColor = System.Drawing.Color.Black;
            this.CheckBox25.Location = new System.Drawing.Point(31, 128);
            this.CheckBox25.Name = "CheckBox25";
            this.CheckBox25.Size = new System.Drawing.Size(35, 35);
            this.CheckBox25.TabIndex = 24;
            this.CheckBox25.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox25.UseMnemonic = false;
            this.CheckBox25.UseVisualStyleBackColor = false;
            // 
            // CheckBox40
            // 
            this.CheckBox40.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox40.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox40.BackColor = System.Drawing.Color.White;
            this.CheckBox40.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox40.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox40.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox40.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox40.ForeColor = System.Drawing.Color.Black;
            this.CheckBox40.Location = new System.Drawing.Point(325, 170);
            this.CheckBox40.Name = "CheckBox40";
            this.CheckBox40.Size = new System.Drawing.Size(35, 35);
            this.CheckBox40.TabIndex = 39;
            this.CheckBox40.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox40.UseMnemonic = false;
            this.CheckBox40.UseVisualStyleBackColor = false;
            // 
            // CheckBox39
            // 
            this.CheckBox39.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox39.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox39.BackColor = System.Drawing.Color.White;
            this.CheckBox39.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox39.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox39.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox39.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox39.ForeColor = System.Drawing.Color.Black;
            this.CheckBox39.Location = new System.Drawing.Point(283, 170);
            this.CheckBox39.Name = "CheckBox39";
            this.CheckBox39.Size = new System.Drawing.Size(35, 35);
            this.CheckBox39.TabIndex = 38;
            this.CheckBox39.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox39.UseMnemonic = false;
            this.CheckBox39.UseVisualStyleBackColor = false;
            // 
            // CheckBox38
            // 
            this.CheckBox38.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox38.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox38.BackColor = System.Drawing.Color.White;
            this.CheckBox38.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox38.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox38.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox38.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox38.ForeColor = System.Drawing.Color.Black;
            this.CheckBox38.Location = new System.Drawing.Point(241, 170);
            this.CheckBox38.Name = "CheckBox38";
            this.CheckBox38.Size = new System.Drawing.Size(35, 35);
            this.CheckBox38.TabIndex = 37;
            this.CheckBox38.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox38.UseMnemonic = false;
            this.CheckBox38.UseVisualStyleBackColor = false;
            // 
            // CheckBox37
            // 
            this.CheckBox37.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox37.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox37.BackColor = System.Drawing.Color.White;
            this.CheckBox37.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox37.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox37.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox37.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox37.ForeColor = System.Drawing.Color.Black;
            this.CheckBox37.Location = new System.Drawing.Point(199, 170);
            this.CheckBox37.Name = "CheckBox37";
            this.CheckBox37.Size = new System.Drawing.Size(35, 35);
            this.CheckBox37.TabIndex = 36;
            this.CheckBox37.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox37.UseMnemonic = false;
            this.CheckBox37.UseVisualStyleBackColor = false;
            // 
            // CheckBox35
            // 
            this.CheckBox35.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox35.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox35.BackColor = System.Drawing.Color.White;
            this.CheckBox35.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox35.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox35.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox35.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox35.ForeColor = System.Drawing.Color.Black;
            this.CheckBox35.Location = new System.Drawing.Point(115, 170);
            this.CheckBox35.Name = "CheckBox35";
            this.CheckBox35.Size = new System.Drawing.Size(35, 35);
            this.CheckBox35.TabIndex = 34;
            this.CheckBox35.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox35.UseMnemonic = false;
            this.CheckBox35.UseVisualStyleBackColor = false;
            // 
            // CheckBox36
            // 
            this.CheckBox36.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox36.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox36.BackColor = System.Drawing.Color.White;
            this.CheckBox36.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox36.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox36.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox36.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox36.ForeColor = System.Drawing.Color.Black;
            this.CheckBox36.Location = new System.Drawing.Point(157, 170);
            this.CheckBox36.Name = "CheckBox36";
            this.CheckBox36.Size = new System.Drawing.Size(35, 35);
            this.CheckBox36.TabIndex = 35;
            this.CheckBox36.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox36.UseMnemonic = false;
            this.CheckBox36.UseVisualStyleBackColor = false;
            // 
            // CheckBox34
            // 
            this.CheckBox34.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox34.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox34.BackColor = System.Drawing.Color.White;
            this.CheckBox34.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox34.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox34.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox34.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox34.ForeColor = System.Drawing.Color.Black;
            this.CheckBox34.Location = new System.Drawing.Point(73, 170);
            this.CheckBox34.Name = "CheckBox34";
            this.CheckBox34.Size = new System.Drawing.Size(35, 35);
            this.CheckBox34.TabIndex = 33;
            this.CheckBox34.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox34.UseMnemonic = false;
            this.CheckBox34.UseVisualStyleBackColor = false;
            // 
            // CheckBox33
            // 
            this.CheckBox33.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox33.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox33.BackColor = System.Drawing.Color.White;
            this.CheckBox33.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox33.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox33.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox33.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox33.ForeColor = System.Drawing.Color.Black;
            this.CheckBox33.Location = new System.Drawing.Point(31, 170);
            this.CheckBox33.Name = "CheckBox33";
            this.CheckBox33.Size = new System.Drawing.Size(35, 35);
            this.CheckBox33.TabIndex = 32;
            this.CheckBox33.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox33.UseMnemonic = false;
            this.CheckBox33.UseVisualStyleBackColor = false;
            // 
            // CheckBox48
            // 
            this.CheckBox48.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox48.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox48.BackColor = System.Drawing.Color.White;
            this.CheckBox48.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox48.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox48.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox48.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox48.ForeColor = System.Drawing.Color.Black;
            this.CheckBox48.Location = new System.Drawing.Point(325, 211);
            this.CheckBox48.Name = "CheckBox48";
            this.CheckBox48.Size = new System.Drawing.Size(35, 35);
            this.CheckBox48.TabIndex = 47;
            this.CheckBox48.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox48.UseMnemonic = false;
            this.CheckBox48.UseVisualStyleBackColor = false;
            // 
            // CheckBox47
            // 
            this.CheckBox47.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox47.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox47.BackColor = System.Drawing.Color.White;
            this.CheckBox47.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox47.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox47.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox47.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox47.ForeColor = System.Drawing.Color.Black;
            this.CheckBox47.Location = new System.Drawing.Point(283, 211);
            this.CheckBox47.Name = "CheckBox47";
            this.CheckBox47.Size = new System.Drawing.Size(35, 35);
            this.CheckBox47.TabIndex = 46;
            this.CheckBox47.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox47.UseMnemonic = false;
            this.CheckBox47.UseVisualStyleBackColor = false;
            // 
            // CheckBox46
            // 
            this.CheckBox46.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox46.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox46.BackColor = System.Drawing.Color.White;
            this.CheckBox46.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox46.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox46.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox46.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox46.ForeColor = System.Drawing.Color.Black;
            this.CheckBox46.Location = new System.Drawing.Point(241, 211);
            this.CheckBox46.Name = "CheckBox46";
            this.CheckBox46.Size = new System.Drawing.Size(35, 35);
            this.CheckBox46.TabIndex = 45;
            this.CheckBox46.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox46.UseMnemonic = false;
            this.CheckBox46.UseVisualStyleBackColor = false;
            // 
            // CheckBox45
            // 
            this.CheckBox45.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox45.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox45.BackColor = System.Drawing.Color.White;
            this.CheckBox45.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox45.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox45.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox45.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox45.ForeColor = System.Drawing.Color.Black;
            this.CheckBox45.Location = new System.Drawing.Point(199, 211);
            this.CheckBox45.Name = "CheckBox45";
            this.CheckBox45.Size = new System.Drawing.Size(35, 35);
            this.CheckBox45.TabIndex = 44;
            this.CheckBox45.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox45.UseMnemonic = false;
            this.CheckBox45.UseVisualStyleBackColor = false;
            // 
            // CheckBox43
            // 
            this.CheckBox43.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox43.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox43.BackColor = System.Drawing.Color.White;
            this.CheckBox43.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox43.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox43.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox43.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox43.ForeColor = System.Drawing.Color.Black;
            this.CheckBox43.Location = new System.Drawing.Point(115, 211);
            this.CheckBox43.Name = "CheckBox43";
            this.CheckBox43.Size = new System.Drawing.Size(35, 35);
            this.CheckBox43.TabIndex = 42;
            this.CheckBox43.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox43.UseMnemonic = false;
            this.CheckBox43.UseVisualStyleBackColor = false;
            // 
            // CheckBox44
            // 
            this.CheckBox44.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox44.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox44.BackColor = System.Drawing.Color.White;
            this.CheckBox44.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox44.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox44.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox44.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox44.ForeColor = System.Drawing.Color.Black;
            this.CheckBox44.Location = new System.Drawing.Point(157, 211);
            this.CheckBox44.Name = "CheckBox44";
            this.CheckBox44.Size = new System.Drawing.Size(35, 35);
            this.CheckBox44.TabIndex = 43;
            this.CheckBox44.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox44.UseMnemonic = false;
            this.CheckBox44.UseVisualStyleBackColor = false;
            // 
            // CheckBox42
            // 
            this.CheckBox42.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox42.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox42.BackColor = System.Drawing.Color.White;
            this.CheckBox42.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox42.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox42.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox42.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox42.ForeColor = System.Drawing.Color.Black;
            this.CheckBox42.Location = new System.Drawing.Point(73, 211);
            this.CheckBox42.Name = "CheckBox42";
            this.CheckBox42.Size = new System.Drawing.Size(35, 35);
            this.CheckBox42.TabIndex = 41;
            this.CheckBox42.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox42.UseMnemonic = false;
            this.CheckBox42.UseVisualStyleBackColor = false;
            // 
            // CheckBox41
            // 
            this.CheckBox41.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CheckBox41.Appearance = System.Windows.Forms.Appearance.Button;
            this.CheckBox41.BackColor = System.Drawing.Color.White;
            this.CheckBox41.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.CheckBox41.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.CheckBox41.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.CheckBox41.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckBox41.ForeColor = System.Drawing.Color.Black;
            this.CheckBox41.Location = new System.Drawing.Point(31, 211);
            this.CheckBox41.Name = "CheckBox41";
            this.CheckBox41.Size = new System.Drawing.Size(35, 35);
            this.CheckBox41.TabIndex = 40;
            this.CheckBox41.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CheckBox41.UseMnemonic = false;
            this.CheckBox41.UseVisualStyleBackColor = false;
            // 
            // LabelSelectedCharacters
            // 
            this.LabelSelectedCharacters.AutoSize = true;
            this.LabelSelectedCharacters.Location = new System.Drawing.Point(3, 254);
            this.LabelSelectedCharacters.Margin = new System.Windows.Forms.Padding(3);
            this.LabelSelectedCharacters.Name = "LabelSelectedCharacters";
            this.LabelSelectedCharacters.Size = new System.Drawing.Size(110, 15);
            this.LabelSelectedCharacters.TabIndex = 1;
            this.LabelSelectedCharacters.Text = "Selected Characters";
            // 
            // PanelButtons
            // 
            this.PanelButtons.AutoSize = true;
            this.PanelButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PanelButtons.Controls.Add(this.buttonCancel);
            this.PanelButtons.Controls.Add(this.ButtonOK);
            this.PanelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PanelButtons.Location = new System.Drawing.Point(0, 498);
            this.PanelButtons.Margin = new System.Windows.Forms.Padding(0);
            this.PanelButtons.Name = "PanelButtons";
            this.PanelButtons.Size = new System.Drawing.Size(683, 36);
            this.PanelButtons.TabIndex = 1;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.AutoSize = true;
            this.buttonCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.buttonCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.buttonCancel.FlatAppearance.CheckedBackColor = System.Drawing.Color.SteelBlue;
            this.buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.Location = new System.Drawing.Point(593, 6);
            this.buttonCancel.MinimumSize = new System.Drawing.Size(87, 0);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(87, 27);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = false;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // ButtonOK
            // 
            this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOK.AutoSize = true;
            this.ButtonOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.ButtonOK.Enabled = false;
            this.ButtonOK.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ButtonOK.FlatAppearance.CheckedBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonOK.Location = new System.Drawing.Point(500, 6);
            this.ButtonOK.MinimumSize = new System.Drawing.Size(87, 0);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(87, 27);
            this.ButtonOK.TabIndex = 0;
            this.ButtonOK.Text = "OK";
            this.ButtonOK.UseVisualStyleBackColor = false;
            this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // TipChar
            // 
            this.TipChar.BackColor = System.Drawing.Color.White;
            this.TipChar.ForeColor = System.Drawing.Color.Black;
            this.TipChar.IsBalloon = true;
            this.TipChar.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.TipChar.ToolTipTitle = "Character Info";
            // 
            // TextSelectedCharacters
            // 
            this.TextSelectedCharacters.BackColor = System.Drawing.Color.White;
            this.TextSelectedCharacters.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LeftTable.SetColumnSpan(this.TextSelectedCharacters, 2);
            this.TextSelectedCharacters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextSelectedCharacters.ForeColor = System.Drawing.Color.Black;
            this.TextSelectedCharacters.Location = new System.Drawing.Point(0, 272);
            this.TextSelectedCharacters.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.TextSelectedCharacters.MaxLength = 65536;
            this.TextSelectedCharacters.Multiline = true;
            this.TextSelectedCharacters.Name = "TextSelectedCharacters";
            this.TextSelectedCharacters.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TextSelectedCharacters.Size = new System.Drawing.Size(409, 217);
            this.TextSelectedCharacters.TabIndex = 2;
            this.TextSelectedCharacters.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextCharacters_KeyDown);
            this.TextSelectedCharacters.Leave += new System.EventHandler(this.TextCharacters_Leave);
            // 
            // LeftTable
            // 
            this.LeftTable.AutoSize = true;
            this.LeftTable.ColumnCount = 2;
            this.LeftTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LeftTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.LeftTable.Controls.Add(this.panelCharacters, 0, 0);
            this.LeftTable.Controls.Add(this.ScrollVertical, 1, 0);
            this.LeftTable.Controls.Add(this.LabelSelectedCharacters, 0, 1);
            this.LeftTable.Controls.Add(this.TextSelectedCharacters, 0, 2);
            this.LeftTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LeftTable.Location = new System.Drawing.Point(3, 3);
            this.LeftTable.Name = "LeftTable";
            this.LeftTable.RowCount = 3;
            this.LeftTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.LeftTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.LeftTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LeftTable.Size = new System.Drawing.Size(409, 492);
            this.LeftTable.TabIndex = 0;
            // 
            // RightTable
            // 
            this.RightTable.AutoSize = true;
            this.RightTable.ColumnCount = 1;
            this.RightTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.RightTable.Controls.Add(this.ListCharacterRanges, 0, 0);
            this.RightTable.Controls.Add(this.ButtonSelectAll, 0, 1);
            this.RightTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RightTable.Location = new System.Drawing.Point(418, 3);
            this.RightTable.Name = "RightTable";
            this.RightTable.RowCount = 2;
            this.RightTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.RightTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.RightTable.Size = new System.Drawing.Size(262, 492);
            this.RightTable.TabIndex = 1;
            // 
            // TableControls
            // 
            this.TableControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.TableControls.ColumnCount = 2;
            this.TableControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableControls.Controls.Add(this.LeftTable, 0, 0);
            this.TableControls.Controls.Add(this.RightTable, 1, 0);
            this.TableControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableControls.Location = new System.Drawing.Point(0, 0);
            this.TableControls.Name = "TableControls";
            this.TableControls.RowCount = 1;
            this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableControls.Size = new System.Drawing.Size(683, 498);
            this.TableControls.TabIndex = 0;
            // 
            // CharacterPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.Controls.Add(this.TableControls);
            this.Controls.Add(this.PanelButtons);
            this.Name = "CharacterPicker";
            this.Size = new System.Drawing.Size(683, 534);
            this.panelCharacters.ResumeLayout(false);
            this.PanelButtons.ResumeLayout(false);
            this.PanelButtons.PerformLayout();
            this.LeftTable.ResumeLayout(false);
            this.LeftTable.PerformLayout();
            this.RightTable.ResumeLayout(false);
            this.RightTable.PerformLayout();
            this.TableControls.ResumeLayout(false);
            this.TableControls.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button ButtonSelectAll;
        private System.Windows.Forms.ListView ListCharacterRanges;
        private System.Windows.Forms.ColumnHeader columnRange;
        private System.Windows.Forms.ColumnHeader columnRangeName;
        private System.Windows.Forms.VScrollBar ScrollVertical;
        private System.Windows.Forms.Panel panelCharacters;
        private System.Windows.Forms.CheckBox CheckBox1;
        private System.Windows.Forms.CheckBox CheckBox2;
        private System.Windows.Forms.CheckBox CheckBox3;
        private System.Windows.Forms.CheckBox CheckBox4;
        private System.Windows.Forms.CheckBox CheckBox5;
        private System.Windows.Forms.CheckBox CheckBox6;
        private System.Windows.Forms.CheckBox CheckBox7;
        private System.Windows.Forms.CheckBox CheckBox8;
        private System.Windows.Forms.CheckBox CheckBox16;
        private System.Windows.Forms.CheckBox CheckBox15;
        private System.Windows.Forms.CheckBox CheckBox14;
        private System.Windows.Forms.CheckBox CheckBox13;
        private System.Windows.Forms.CheckBox CheckBox11;
        private System.Windows.Forms.CheckBox CheckBox12;
        private System.Windows.Forms.CheckBox CheckBox10;
        private System.Windows.Forms.CheckBox CheckBox9;
        private System.Windows.Forms.CheckBox CheckBox24;
        private System.Windows.Forms.CheckBox CheckBox23;
        private System.Windows.Forms.CheckBox CheckBox22;
        private System.Windows.Forms.CheckBox CheckBox21;
        private System.Windows.Forms.CheckBox CheckBox19;
        private System.Windows.Forms.CheckBox CheckBox20;
        private System.Windows.Forms.CheckBox CheckBox18;
        private System.Windows.Forms.CheckBox CheckBox17;
        private System.Windows.Forms.CheckBox CheckBox32;
        private System.Windows.Forms.CheckBox CheckBox31;
        private System.Windows.Forms.CheckBox CheckBox30;
        private System.Windows.Forms.CheckBox CheckBox29;
        private System.Windows.Forms.CheckBox CheckBox27;
        private System.Windows.Forms.CheckBox CheckBox28;
        private System.Windows.Forms.CheckBox CheckBox26;
        private System.Windows.Forms.CheckBox CheckBox25;
        private System.Windows.Forms.CheckBox CheckBox40;
        private System.Windows.Forms.CheckBox CheckBox39;
        private System.Windows.Forms.CheckBox CheckBox38;
        private System.Windows.Forms.CheckBox CheckBox37;
        private System.Windows.Forms.CheckBox CheckBox35;
        private System.Windows.Forms.CheckBox CheckBox36;
        private System.Windows.Forms.CheckBox CheckBox34;
        private System.Windows.Forms.CheckBox CheckBox33;
        private System.Windows.Forms.CheckBox CheckBox48;
        private System.Windows.Forms.CheckBox CheckBox47;
        private System.Windows.Forms.CheckBox CheckBox46;
        private System.Windows.Forms.CheckBox CheckBox45;
        private System.Windows.Forms.CheckBox CheckBox43;
        private System.Windows.Forms.CheckBox CheckBox44;
        private System.Windows.Forms.CheckBox CheckBox42;
        private System.Windows.Forms.CheckBox CheckBox41;
        private System.Windows.Forms.Label LabelSelectedCharacters;
        private System.Windows.Forms.Panel PanelButtons;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button ButtonOK;
        private System.Windows.Forms.ToolTip TipChar;
        private System.Windows.Forms.TextBox TextSelectedCharacters;
        private System.Windows.Forms.TableLayoutPanel LeftTable;
        private System.Windows.Forms.TableLayoutPanel RightTable;
        private System.Windows.Forms.TableLayoutPanel TableControls;
    }
}
