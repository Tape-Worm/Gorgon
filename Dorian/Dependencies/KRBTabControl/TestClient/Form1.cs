using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace TestClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            ToolStripProfessionalRenderer rnd = toolStrip1.Renderer as ToolStripProfessionalRenderer;
            if (rnd != null)
                rnd.RoundedEdges = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "Server is started by the user.";
        }

        #region ON/OFF

        private void toolMenuHeaderDraw_Click(object sender, EventArgs e)
        {
            krbTabControl1.IsDrawHeader = !krbTabControl1.IsDrawHeader;
            toolMenuHeaderDraw.Text = String.Format("Header Draw: {0}", krbTabControl1.IsDrawHeader ? "ON" : "OFF");
            toolMenuHeaderDraw.Checked = krbTabControl1.IsDrawHeader ? true : false;
        }

        ToolStripMenuItem tabPageWindows = null;
        private void toolMenuStretch_Click(object sender, EventArgs e)
        {
            krbTabControl1.HeaderVisibility = !krbTabControl1.HeaderVisibility;
            toolMenuStretch.Text = String.Format("Stretch To Parent: {0}", krbTabControl1.HeaderVisibility ? "ON" : "OFF");
            toolMenuStretch.Checked = krbTabControl1.HeaderVisibility ? true : false;
            this.Text = String.Format("Test Client{0}", krbTabControl1.HeaderVisibility ? String.Format(" [{0}]", krbTabControl1.SelectedTab) : null);

            if (krbTabControl1.HeaderVisibility)
            {
                tabPageWindows = new ToolStripMenuItem("Window");
                menuStrip1.Items.Insert(2, tabPageWindows);
                
                // Add each tabpage window in the collection of the window menu item.
                foreach (KRBTabControl.TabPageEx tabPage in krbTabControl1.TabPages)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem(tabPage.ToString());
                    item.Name = tabPage.Name;
                    if (krbTabControl1.SelectedTab.Equals(tabPage))
                        item.Checked = true;

                    item.Click += (thrower, ea) =>
                    {
                        if (krbTabControl1.SelectedTab.Name != item.Name)
                        {
                            krbTabControl1.SelectTab(item.Name);
                            item.Checked = true;
                            this.Text = String.Format("Test Client [{0}]", item.Text);

                            ToolStripItemCollection collection = tabPageWindows.DropDownItems;
                            foreach (ToolStripMenuItem menuItem in collection)
                            {
                                if (!item.Equals(menuItem))
                                {
                                    menuItem.Checked = false;
                                }
                            }
                        }
                    };
                    
                    tabPageWindows.DropDownItems.Add(item);
                }
            }
            else
            {
                if (tabPageWindows == null)
                    return;

                tabPageWindows.Visible = false;
            }
        }

        private void toolMenuCaption_Click(object sender, EventArgs e)
        {
            krbTabControl1.IsCaptionVisible = !krbTabControl1.IsCaptionVisible;
            toolMenuCaption.Text = String.Format("Caption Visible: {0}", krbTabControl1.IsCaptionVisible ? "ON" : "OFF");
            toolMenuCaption.Checked = krbTabControl1.IsCaptionVisible ? true : false;

            if (krbTabControl1.IsCaptionVisible)
            {
                krbTabControl1.IsDrawEdgeBorder = false;
                toolMenuEdgeBorder.Text = "Edge Border Drawing: OFF";
                toolMenuEdgeBorder.Checked = false;
            }
        }

        private void toolMenuSeparator_Click(object sender, EventArgs e)
        {
            krbTabControl1.IsDrawTabSeparator = !krbTabControl1.IsDrawTabSeparator;
            toolMenuSeparator.Text = String.Format("TabItem Separator: {0}", krbTabControl1.IsDrawTabSeparator ? "ON" : "OFF");
            toolMenuSeparator.Checked = krbTabControl1.IsDrawTabSeparator ? true : false;
        }

        private void toolMenuColorizer_Click(object sender, EventArgs e)
        {
            krbTabControl1.CaptionRandomizer.IsRandomizerEnabled = !krbTabControl1.CaptionRandomizer.IsRandomizerEnabled;
            toolMenuColorizer.Text = String.Format("Caption RGBA Colorizer: {0}", krbTabControl1.CaptionRandomizer.IsRandomizerEnabled ? "ON" : "OFF");
            toolMenuColorizer.Checked = krbTabControl1.CaptionRandomizer.IsRandomizerEnabled ? true : false;
        }

        private void toolMenuEdgeBorder_Click(object sender, EventArgs e)
        {
            krbTabControl1.IsDrawEdgeBorder = !krbTabControl1.IsDrawEdgeBorder;
            toolMenuEdgeBorder.Text = String.Format("Edge Border Drawing: {0}", krbTabControl1.IsDrawEdgeBorder ? "ON" : "OFF");
            toolMenuEdgeBorder.Checked = krbTabControl1.IsDrawEdgeBorder ? true : false;

            if (krbTabControl1.IsDrawEdgeBorder)
            {
                krbTabControl1.IsCaptionVisible = false;
                toolMenuCaption.Text = "Caption Visible: OFF";
                toolMenuCaption.Checked = false;
            }
        }

        #endregion

        #region Tab-Alignments

        private void menuTop_Click(object sender, EventArgs e)
        {
            if (!menuTop.Checked)
            {
                krbTabControl1.Alignments = KRBTabControl.KRBTabControl.TabAlignments.Top;
                menuTop.Checked = true;
                menuBottom.Checked = false;
                // Replace color positions.
                Color first = krbTabControl1.TabGradient.ColorStart;
                Color second = krbTabControl1.TabGradient.ColorEnd;
                krbTabControl1.TabGradient.ColorStart = second;
                krbTabControl1.TabGradient.ColorEnd = first;
            }
        }

        private void menuBottom_Click(object sender, EventArgs e)
        {
            if (!menuBottom.Checked)
            {
                krbTabControl1.Alignments = KRBTabControl.KRBTabControl.TabAlignments.Bottom;
                menuBottom.Checked = true;
                menuTop.Checked = false;
                // Replace color positions.
                Color first = krbTabControl1.TabGradient.ColorStart;
                Color second = krbTabControl1.TabGradient.ColorEnd;
                krbTabControl1.TabGradient.ColorStart = second;
                krbTabControl1.TabGradient.ColorEnd = first;
            }
        }

        #endregion

        #region TabHOffset

        private void toolMenuOffset_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
            {
                int offset;
                if (Int32.TryParse(toolMenuOffset.Text, out offset))
                {
                    if (offset < -2 || offset > 5)
                    {
                        MessageBox.Show("This property must be in the range of -2 to 5.");
                        toolMenuOffset.Text = "-2";
                        krbTabControl1.TabHOffset = -2;
                        return;
                    }

                    krbTabControl1.TabHOffset = offset;
                }
            }
        }

        #endregion

        #region TabItemStylesUI

        private void menuKRBStyle_Click(object sender, EventArgs e)
        {
            if (!menuKRBStyle.Checked)
            {
                krbTabControl1.TabStyles = KRBTabControl.KRBTabControl.TabStyle.KRBStyle;
                menuKRBStyle.Checked = true;
                menuOfficeXP.Checked = false;
                menu2010.Checked = false;
            }
        }

        private void menuOfficeXP_Click(object sender, EventArgs e)
        {
            if (!menuOfficeXP.Checked)
            {
                krbTabControl1.TabStyles = KRBTabControl.KRBTabControl.TabStyle.OfficeXP;
                menuOfficeXP.Checked = true;
                menuKRBStyle.Checked = false;
                menu2010.Checked = false;
            }
        }

        private void menu2010_Click(object sender, EventArgs e)
        {
            if (!menu2010.Checked)
            {
                krbTabControl1.TabStyles = KRBTabControl.KRBTabControl.TabStyle.VS2010;
                menu2010.Checked = true;
                menuKRBStyle.Checked = false;
                menuOfficeXP.Checked = false;
            }
        }

        #endregion

        #region Paint TabItem Background Header

        private void menuTexture_Click(object sender, EventArgs e)
        {
            if (!menuTexture.Checked)
            {
                krbTabControl1.HeaderStyle = KRBTabControl.KRBTabControl.TabHeaderStyle.Texture;
                menuTexture.Checked = true;
                menuSolid.Checked = false;
                menuHatch.Checked = false;
            }
        }

        private void menuSolid_Click(object sender, EventArgs e)
        {
            if (!menuSolid.Checked)
            {
                krbTabControl1.HeaderStyle = KRBTabControl.KRBTabControl.TabHeaderStyle.Solid;
                menuSolid.Checked = true;
                menuHatch.Checked = false;
                menuTexture.Checked = false;
            }
        }

        private void menuHatch_Click(object sender, EventArgs e)
        {
            if (!menuHatch.Checked)
            {
                krbTabControl1.BackgroundHatcher.ForeColor = Color.LightPink;
                krbTabControl1.BackgroundHatcher.HatchType = HatchStyle.DottedGrid;
                krbTabControl1.HeaderStyle = KRBTabControl.KRBTabControl.TabHeaderStyle.Hatch;
                menuHatch.Checked = true;
                menuSolid.Checked = false;
                menuTexture.Checked = false;
            }
        }

        #endregion

        private void menuColorizer_Click(object sender, EventArgs e)
        {
            using (ColorizerDialog dialog = new ColorizerDialog(krbTabControl1.CaptionRandomizer))
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    krbTabControl1.CaptionRandomizer = dialog.Colorizer;
                    if (krbTabControl1.CaptionRandomizer.Transparency != 255)
                        krbTabControl1.CaptionRandomizer.IsTransparencyEnabled = true;
                }
            }
        }

        #region Add a new TabPage

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            KRBTabControl.TabPageEx newTabPage = new KRBTabControl.TabPageEx();
            newTabPage.ImageIndex = 2;
            krbTabControl1.TabPages.Add(newTabPage);
            Label newLabel = new Label();
            newLabel.Text = newTabPage.Text;
            newLabel.Font = new Font("Tahoma", 9.75f, FontStyle.Bold);
            newLabel.Location = new Point(10, 10);
            newTabPage.Controls.Add(newLabel);
        }

        #endregion

        private void menuTabItemColors_Click(object sender, EventArgs e)
        {
            myColorDialog1.Title = "Select first tab item color";
            if (myColorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                krbTabControl1.TabGradient.ColorStart = myColorDialog1.Color;
            myColorDialog1.Title = "Select second tab item color";
            if (myColorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                krbTabControl1.TabGradient.ColorEnd = myColorDialog1.Color;
            myColorDialog1.Title = "Selected tab item text color";
            myColorDialog1.Color = krbTabControl1.TabGradient.TabPageSelectedTextColor;
            if (myColorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                krbTabControl1.TabGradient.TabPageSelectedTextColor = myColorDialog1.Color;
        }

        #region IsSelectable

        private void krbTabControl1_SelectedIndexChanging(object sender, KRBTabControl.KRBTabControl.SelectedIndexChangingEventArgs e)
        {
            if (e.TabPage.Text == "IsSelectable?")
                e.Cancel = true;
        }

        #endregion

        #region Border StylesUI

        private void menuBorderSolid_Click(object sender, EventArgs e)
        {
            if (!menuBorderSolid.Checked)
            {
                krbTabControl1.BorderStyle = KRBTabControl.KRBTabControl.ControlBorderStyle.Solid;
                menuBorderSolid.Checked = true;
                menuBorderDotted.Checked = false;
                menuBorderDashed.Checked = false;
            }
        }

        private void menuBorderDashed_Click(object sender, EventArgs e)
        {
            if (!menuBorderDashed.Checked)
            {
                krbTabControl1.BorderStyle = KRBTabControl.KRBTabControl.ControlBorderStyle.Dashed;
                menuBorderDashed.Checked = true;
                menuBorderDotted.Checked = false;
                menuBorderSolid.Checked = false;
            }
        }

        private void menuBorderDotted_Click(object sender, EventArgs e)
        {
            if (!menuBorderDotted.Checked)
            {
                krbTabControl1.BorderStyle = KRBTabControl.KRBTabControl.ControlBorderStyle.Dotted;
                menuBorderDotted.Checked = true;
                menuBorderDashed.Checked = false;
                menuBorderSolid.Checked = false;
            }
        }

        #endregion

        #region Load and Save

        private void SaveSettings(bool isCloseApp)
        {
            ApplicationSettings settings = new ApplicationSettings(this.Location);
            settings.HOffset = krbTabControl1.TabHOffset;
            settings.SelectedTabPageName = krbTabControl1.SelectedTab != null ? krbTabControl1.SelectedTab.Name : null;
            settings.IsStretchToParent = krbTabControl1.HeaderVisibility;
            settings.FirstTabItemColor = krbTabControl1.TabGradient.ColorStart;
            settings.SecondTabItemColor = krbTabControl1.TabGradient.ColorEnd;
            settings.SelectedTabItemTextColor = krbTabControl1.TabGradient.TabPageSelectedTextColor;
            settings.Red = krbTabControl1.CaptionRandomizer.Red;
            settings.Green = krbTabControl1.CaptionRandomizer.Green;
            settings.Blue = krbTabControl1.CaptionRandomizer.Blue;
            settings.Alpha = krbTabControl1.CaptionRandomizer.Transparency;
            settings.IsDrawHeader = krbTabControl1.IsDrawHeader;
            settings.IsDrawEdgeBorder = krbTabControl1.IsDrawEdgeBorder;
            settings.IsCaptionVisible = krbTabControl1.IsCaptionVisible;
            settings.IsRandomizerEnabled = krbTabControl1.CaptionRandomizer.IsRandomizerEnabled;
            settings.IsDrawTabSeparator = krbTabControl1.IsDrawTabSeparator;
            settings.BorderStyles = krbTabControl1.BorderStyle;
            settings.HeaderStyles = krbTabControl1.HeaderStyle;
            settings.TabPageStyles = krbTabControl1.TabStyles;
            settings.TabAlingments = krbTabControl1.Alignments;

            DataSaver.SaveData(settings);
            if (isCloseApp)
                Application.Exit();
        }
        
        private void menuSaveSettings_Click(object sender, EventArgs e)
        {
            SaveSettings(false);
        }

        private void menuSaveAndExit_Click(object sender, EventArgs e)
        {
            SaveSettings(true);
        }

        private void menuLoadSettings_Click(object sender, EventArgs e)
        {
            ApplicationSettings settings = DataSaver.LoadData();
            if (settings != null)
            {
                this.krbTabControl1.SuspendLayout();
                this.SuspendLayout();
                
                this.Location = settings.FormLocation;
                // ************ //
                krbTabControl1.TabHOffset = settings.HOffset;
                toolMenuOffset.Text = krbTabControl1.TabHOffset.ToString();
                // ************ //
                foreach (TabPage tabPage in krbTabControl1.TabPages)
                {
                    if (tabPage.Name.Equals(settings.SelectedTabPageName))
                    {
                        krbTabControl1.SelectedTab = tabPage;
                        break;
                    }
                }
                // ************ //
                krbTabControl1.HeaderVisibility = settings.IsStretchToParent;
                toolMenuStretch.Text = String.Format("Stretch To Parent: {0}", krbTabControl1.HeaderVisibility ? "ON" : "OFF");
                toolMenuStretch.Checked = krbTabControl1.HeaderVisibility ? true : false;
                // ************ //
                krbTabControl1.TabGradient.ColorStart = settings.FirstTabItemColor;
                krbTabControl1.TabGradient.ColorEnd = settings.SecondTabItemColor;
                krbTabControl1.TabGradient.TabPageSelectedTextColor = settings.SelectedTabItemTextColor;
                // ************ //
                krbTabControl1.CaptionRandomizer = new KRBTabControl.KRBTabControl.RandomizerCaption()
                {
                    Red = settings.Red,
                    Green = settings.Green,
                    Blue = settings.Blue,
                    Transparency = settings.Alpha,
                    IsRandomizerEnabled = settings.IsRandomizerEnabled,
                    IsTransparencyEnabled = settings.Alpha == 255 ? false : true
                };
                // ************ //
                krbTabControl1.IsDrawHeader = settings.IsDrawHeader;
                toolMenuHeaderDraw.Text = String.Format("Header Draw: {0}", krbTabControl1.IsDrawHeader ? "ON" : "OFF");
                toolMenuHeaderDraw.Checked = krbTabControl1.IsDrawHeader ? true : false;
                // ************ //
                krbTabControl1.IsDrawEdgeBorder = settings.IsDrawEdgeBorder;
                toolMenuEdgeBorder.Text = String.Format("Edge Border Drawing: {0}", krbTabControl1.IsDrawEdgeBorder ? "ON" : "OFF");
                toolMenuEdgeBorder.Checked = krbTabControl1.IsDrawEdgeBorder ? true : false;
                // ************ //
                krbTabControl1.IsCaptionVisible = settings.IsCaptionVisible;
                toolMenuCaption.Text = String.Format("Caption Visible: {0}", krbTabControl1.IsCaptionVisible ? "ON" : "OFF");
                toolMenuCaption.Checked = krbTabControl1.IsCaptionVisible ? true : false;
                // ************ //
                toolMenuColorizer.Text = String.Format("Caption RGBA Colorizer: {0}", krbTabControl1.CaptionRandomizer.IsRandomizerEnabled ? "ON" : "OFF");
                toolMenuColorizer.Checked = krbTabControl1.CaptionRandomizer.IsRandomizerEnabled ? true : false;
                // ************ //
                krbTabControl1.IsDrawTabSeparator = settings.IsDrawTabSeparator;
                toolMenuSeparator.Text = String.Format("TabItem Separator: {0}", krbTabControl1.IsDrawTabSeparator ? "ON" : "OFF");
                toolMenuSeparator.Checked = krbTabControl1.IsDrawTabSeparator ? true : false;
                // ************ //
                krbTabControl1.BorderStyle = settings.BorderStyles;
                switch (krbTabControl1.BorderStyle)
                {
                    case KRBTabControl.KRBTabControl.ControlBorderStyle.Dashed:
                        menuBorderDashed.Checked = true;
                        menuBorderDotted.Checked = false;
                        menuBorderSolid.Checked = false;
                        break;
                    case KRBTabControl.KRBTabControl.ControlBorderStyle.Dotted:
                        menuBorderDotted.Checked = true;
                        menuBorderDashed.Checked = false;
                        menuBorderSolid.Checked = false;
                        break;
                    default:
                        menuBorderSolid.Checked = true;
                        menuBorderDashed.Checked = false;
                        menuBorderDotted.Checked = false;
                        break;
                }
                // ************ //
                krbTabControl1.HeaderStyle = settings.HeaderStyles;
                switch (krbTabControl1.HeaderStyle)
                {
                    case KRBTabControl.KRBTabControl.TabHeaderStyle.Texture:
                        menuTexture.Checked = true;
                        menuHatch.Checked = false;
                        menuSolid.Checked = false;
                        break;
                    case KRBTabControl.KRBTabControl.TabHeaderStyle.Hatch:
                        menuHatch.Checked = true;
                        menuTexture.Checked = false;
                        menuSolid.Checked = false;
                        break;
                    default:
                        menuSolid.Checked = true;
                        menuTexture.Checked = false;
                        menuHatch.Checked = false;
                        break;
                }
                // ************* //
                krbTabControl1.TabStyles = settings.TabPageStyles;
                switch (krbTabControl1.TabStyles)
                {
                    case KRBTabControl.KRBTabControl.TabStyle.VS2010:
                        menu2010.Checked = true;
                        menuOfficeXP.Checked = false;
                        menuKRBStyle.Checked = false;
                        break;
                    case KRBTabControl.KRBTabControl.TabStyle.OfficeXP:
                        menuOfficeXP.Checked = true;
                        menu2010.Checked = false;
                        menuKRBStyle.Checked = false;
                        break;
                    default:
                        menuKRBStyle.Checked = true;
                        menu2010.Checked = false;
                        menuOfficeXP.Checked = false;
                        break;
                }
                // *************** //
                krbTabControl1.Alignments = settings.TabAlingments;
                switch (krbTabControl1.Alignments)
                {
                    case KRBTabControl.KRBTabControl.TabAlignments.Top:
                        menuTop.Checked = true;
                        menuBottom.Checked = false;
                        break;
                    default:
                        menuBottom.Checked = true;
                        menuTop.Checked = false;
                        break;
                }

                this.krbTabControl1.ResumeLayout(false);
                this.ResumeLayout(false);
                this.PerformLayout();
            }
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            SaveSettings(false);
        }

        #endregion

        #region IsClosable

        private void krbTabControl1_TabPageClosing(object sender, KRBTabControl.KRBTabControl.SelectedIndexChangingEventArgs e)
        {
            if (e.TabPage.Text == "Schedules")
            {
                if (MessageBox.Show("Do you want to remove the Schedules tab page?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        #endregion

        #region Customize ContextMenuStrip

        private void krbTabControl1_ContextMenuShown(object sender, KRBTabControl.KRBTabControl.ContextMenuShownEventArgs e)
        {
            ToolStripMenuItem menuItem = new ToolStripMenuItem()
            {
                Text = "Another menu item",
                ShortcutKeys = Keys.Control | Keys.N
            };

            menuItem.Click += (thrower, ea) =>
            {
                MessageBox.Show("Hello World!!!");
            };

            e.ContextMenu.Items.Insert(0, new ToolStripSeparator());
            e.ContextMenu.Items.Insert(0, menuItem);

            menuItem = new ToolStripMenuItem()
            {
                Text = "Open a new tab",
                Image = this.ımageList1.Images[2],
                ShortcutKeys = Keys.Control | Keys.O
            };

            menuItem.Click += (thrower, ea) =>
            {
                KRBTabControl.TabPageEx newTabPage = new KRBTabControl.TabPageEx();
                newTabPage.ImageIndex = 2;
                krbTabControl1.TabPages.Add(newTabPage);
                Label newLabel = new Label();
                newLabel.AutoSize = true;
                newLabel.Text = String.Format("I've created by the drop-down menu, {0}", newTabPage.Text);
                newLabel.Font = new Font("Tahoma", 9.75f, FontStyle.Bold);
                newLabel.Location = new Point(10, 10);
                newTabPage.Controls.Add(newLabel);
            };

            e.ContextMenu.Items.Insert(0, new ToolStripSeparator());
            e.ContextMenu.Items.Insert(0, menuItem);
            e.ContextMenu.Items.Add(new ToolStripSeparator());
            
            menuItem = new ToolStripMenuItem("My Menu Item");
            if (krbTabControl1.SelectedTab != null)
            {
                menuItem.Click += (thrower, ea) =>
                {
                    MessageBox.Show(String.Format("Selected Tab Page: {0}", krbTabControl1.SelectedTab));
                };
            }
            e.ContextMenu.Items.Add(menuItem);
        }

        #endregion
    }
}