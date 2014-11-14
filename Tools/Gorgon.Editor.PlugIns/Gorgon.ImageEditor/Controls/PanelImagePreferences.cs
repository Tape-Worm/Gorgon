#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Saturday, November 1, 2014 1:00:52 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using GorgonLibrary.Editor.ImageEditorPlugIn.Properties;
using GorgonLibrary.IO;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor.ImageEditorPlugIn.Controls
{
	/// <summary>
	/// Panel for image editor preferences.
	/// </summary>
	public partial class PanelImagePreferences 
		: PreferencePanel
	{
		#region Classes.
		/// <summary>
		/// Image codec descriptor.
		/// </summary>
		private class CodecDescriptor
		{
			/// <summary>
			/// Property to set or return the fully qualified type name of the plug-in.
			/// </summary>
			public string CodecTypeName
			{
				get;
				set;
			}

			/// <summary>
			/// Property to return the short type name of the plug-in.
			/// </summary>
			public string CodecName
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the description for the plug-in.
			/// </summary>
			public string CodecDesc
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the path to the codec plug-in DLL.
			/// </summary>
			public string CodecPath
			{
				get;
				set;
			}
		}
		#endregion

		#region Variables.
		// A list of external codecs.
		private List<CodecDescriptor> _codecs = new List<CodecDescriptor>();
        // Filter type list.
	    private Dictionary<ImageFilter, string> _filterTypes = new Dictionary<ImageFilter, string>
	                                                           {
	                                                               {
	                                                                   ImageFilter.Point,
	                                                                   Resources.GORIMG_TEXT_FILTER_POINT
	                                                               },
	                                                               {
	                                                                   ImageFilter.Linear,
	                                                                   Resources.GORIMG_TEXT_FILTER_LINEAR
	                                                               },
	                                                               {
	                                                                   ImageFilter.Cubic,
	                                                                   Resources.GORIMG_TEXT_FILTER_CUBIC
	                                                               },
	                                                               {
	                                                                   ImageFilter.Fant,
	                                                                   Resources.GORIMG_TEXT_FILTER_FANT
	                                                               },
	                                                           };
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to load the codec plug-in DLL.
		/// </summary>
		/// <param name="codecPath">Path to the codec plug-in DLL.</param>
		/// <returns>The list of codec descriptors in the plug-in.</returns>
		private CodecDescriptor[] LoadCodec(string codecPath)
		{
			if (!File.Exists(codecPath))
			{
				GorgonDialogs.ErrorBox(ParentForm, string.Format(Resources.GORIMG_ERR_CODEC_FILE_NOT_FOUND, codecPath));
				return new CodecDescriptor[0];
			}

			// Ensure that this DLL is a valid .NET assembly.
			if (!Gorgon.PlugIns.IsPlugInAssembly(codecPath))
			{
				GorgonDialogs.ErrorBox(ParentForm, string.Format(Resources.GORIMG_ERR_DLL_NOT_ASSEMBLY, codecPath));
				return new CodecDescriptor[0];
			}

			// Load the assembly so we can enumerate the plug-ins from it.
			AssemblyName assemblyName = Gorgon.PlugIns.LoadPlugInAssembly(codecPath);

			// Get a list of codec plug-ins.
			GorgonCodecPlugIn[] plugIns = Gorgon.PlugIns.EnumeratePlugIns(assemblyName).OfType<GorgonCodecPlugIn>().ToArray();

			if (plugIns.Length == 0)
			{
				return new CodecDescriptor[0];
			}

			return plugIns.Select(item => new CodecDescriptor
			                              {
				                              CodecTypeName = item.Name,
											  CodecName = item.GetType().Name,
											  CodecDesc = item.Description,
											  CodecPath = codecPath
			                              }).ToArray();
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the listCodecs control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void listCodecs_SelectedIndexChanged(object sender, EventArgs e)
		{
			ValidateControls();
		}

		/// <summary>
		/// Handles the Click event of the buttonAddCodec control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonAddCodec_Click(object sender, EventArgs e)
		{
			try
			{
				dialogOpen.InitialDirectory = GorgonImageEditorPlugIn.Settings.LastCodecPath;

				if (dialogOpen.ShowDialog(ParentForm) == DialogResult.Cancel)
				{
					return;
				}

				Cursor.Current = Cursors.WaitCursor;

				CodecDescriptor[] codecs = LoadCodec(dialogOpen.FileName);

				if (codecs.Length == 0)
				{
					GorgonDialogs.ErrorBox(ParentForm, string.Format(Resources.GORIMG_ERR_NO_CODEC_PLUGINS, dialogOpen.FileName));
					return;
				}

				listCodecs.BeginUpdate();
				foreach (CodecDescriptor codec in
						codecs.Where(item => _codecs.All(codecType => !string.Equals(codecType.CodecTypeName, item.CodecTypeName, StringComparison.OrdinalIgnoreCase))))
				{
					// Add to our list view.
					var item = new ListViewItem
					           {
						           Text = codec.CodecName,
						           Name = codec.CodecTypeName,
						           Tag = codec
					           };
					item.SubItems.Add(codec.CodecDesc);
					item.SubItems.Add(dialogOpen.FileName.Ellipses(80, true));

					listCodecs.Items.Add(item);

					_codecs.Add(codec);

					// Now remove from the active plug-in list because we don't want them loaded just yet.
					Gorgon.PlugIns.Unload(codec.CodecTypeName);
				}

				// Remember the last codec path.
				GorgonImageEditorPlugIn.Settings.LastCodecPath = Path.GetDirectoryName(dialogOpen.FileName).FormatDirectory(Path.DirectorySeparatorChar);
				GorgonImageEditorPlugIn.Settings.Save();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				listCodecs.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				listCodecs.EndUpdate();
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonRemoveCodec control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonRemoveCodec_Click(object sender, EventArgs e)
		{
			var result = ConfirmationResult.None;

			try
			{
				if (listCodecs.SelectedItems.Count == 0)
				{
					return;
				}

				// Get a list of paths from the selected items.
				var codecPaths = (from listItem in listCodecs.SelectedItems.Cast<ListViewItem>()
				                  let codec = listItem.Tag as CodecDescriptor
				                  where codec != null
				                  select codec.CodecPath).Distinct().ToArray();

				// Find all codecs that belong on the same path as the selected codecs.
				var groupedCodecs = (from listItem in listCodecs.Items.Cast<ListViewItem>()
				                     let codec = listItem.Tag as CodecDescriptor
				                     where codec != null && codecPaths.Any(item => string.Equals(item, codec.CodecPath, StringComparison.OrdinalIgnoreCase))
				                     group codec by codec.CodecPath).ToArray();

				foreach (var codecGroup in groupedCodecs)
				{
					var codecs = codecGroup.ToArray();

					if (codecs.Length > 1)
					{
						if ((result & ConfirmationResult.ToAll) != ConfirmationResult.ToAll)
						{
							result = GorgonDialogs.ConfirmBox(ParentForm,
							                                  string.Format(Resources.GORIMG_DLG_REMOVE_CODECS, codecs[0].CodecName, codecGroup.Key),
							                                  null,
															  groupedCodecs.Length > 1,
							                                  groupedCodecs.Length > 1);

							if (result == ConfirmationResult.Cancel)
							{
								return;
							}
						}

						if ((result & ConfirmationResult.Yes) == ConfirmationResult.Yes)
						{
							// Remove the group of codecs.
							foreach (CodecDescriptor codecDesc in codecs)
							{
								_codecs.Remove(codecDesc);
								if (Gorgon.PlugIns.Contains(codecDesc.CodecTypeName))
								{
									Gorgon.PlugIns.Unload(codecDesc.CodecTypeName);
								}
							}
						}

						continue;
					}

					// If we only have one codec under this group, then just remove the one.
					if ((result & ConfirmationResult.ToAll) != ConfirmationResult.ToAll)
					{
						result = GorgonDialogs.ConfirmBox(ParentForm,
						                                  string.Format(Resources.GORIMG_DLG_REMOVE_CODEC, codecs[0].CodecName),
						                                  null,
														  groupedCodecs.Length > 1,
						                                  groupedCodecs.Length > 1);
					}

					if (result == ConfirmationResult.Cancel)
					{
						return;
					}

					if ((result & ConfirmationResult.Yes) != ConfirmationResult.Yes)
					{
						continue;
					}

					_codecs.Remove(codecs[0]);
					if (!Gorgon.PlugIns.Contains(codecs[0].CodecTypeName))
					{
						continue;
					}

					Gorgon.PlugIns.Unload(codecs[0].CodecTypeName);
				}

				// Refresh the codec list.
				FillCodecList();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
			}
		}

		/// <summary>
		/// Function to validate the controls on the form.
		/// </summary>
		private void ValidateControls()
		{
			panelCannotEdit.Visible = Content is GorgonImageContent;
			listCodecs.Enabled = buttonAddCodec.Enabled = !(Content is GorgonImageContent);
			buttonRemoveCodec.Enabled = listCodecs.SelectedItems.Count > 0 && !(Content is GorgonImageContent);
		}

		/// <summary>
		/// Function to fill the codec list with the plug-ins.
		/// </summary>
		private void FillCodecList()
		{
			listCodecs.BeginUpdate();

			try
			{
				listCodecs.Items.Clear();

				foreach (CodecDescriptor codec in _codecs)
				{
					var codecItem = new ListViewItem
					{
						Text = codec.CodecName,
						Name = codec.CodecTypeName,
						Tag = codec
					};

					codecItem.SubItems.Add(codec.CodecDesc);
					codecItem.SubItems.Add(codec.CodecPath.Ellipses(80, true));

					listCodecs.Items.Add(codecItem);
				}
			}
			finally
			{
				listCodecs.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				listCodecs.EndUpdate();
			}
		}

		/// <summary>
		/// Function to localize the text on the controls for the panel.
		/// </summary>
		/// <remarks>
		/// Override this method to supply localized text for any controls on the panel.
		/// </remarks>
		protected override void LocalizeControls()
		{
			labelImageEditorSettings.Text = Resources.GORIMG_TEXT_IMAGE_EDITOR_SETTINGS;
			checkShowAnimations.Text = Resources.GORIMG_TEXT_SHOW_ANIMATIONS;
			checkShowActualSize.Text = Resources.GORIMG_TEXT_SHOW_ACTUALSIZE_ON_LOAD;
			columnName.Text = Resources.GORIMG_TEXT_CODEC_NAME;
			columnDesc.Text = Resources.GORIMG_TEXT_CODEC_DESC;
			columnPath.Text = Resources.GORIMG_TEXT_CODEC_PATH;
			buttonAddCodec.Text = Resources.GORIMG_ACC_ADD_CODEC;
			buttonRemoveCodec.Text = Resources.GORIMG_ACC_REMOVE_CODEC;
			dialogOpen.Filter = string.Format("{0} (*.dll)|*.dll", Resources.GORIMG_DLG_CODEC_FILTER);
			dialogOpen.Title = Resources.GORIMG_DLG_LOAD_CODEC;
			labelImageCodecs.Text = Resources.GORIMG_TEXT_IMAGE_CODEC_PLUGINS;
			labelCannotEdit.Text = Resources.GORIMG_TEXT_CANNOT_EDIT;
		    labelScalingFilters.Text = Resources.GORIMG_TEXT_IMAGE_FILTERING;
		    labelScaleFilter.Text = Resources.GORIMG_TEXT_FILTER_RESIZE;
		    labelMipMapFilter.Text = Resources.GORIMG_TEXT_FILTER_MIPS;

		    comboMipMapFilter.Items.Add(Resources.GORIMG_TEXT_FILTER_POINT);
            comboMipMapFilter.Items.Add(Resources.GORIMG_TEXT_FILTER_LINEAR);
            comboMipMapFilter.Items.Add(Resources.GORIMG_TEXT_FILTER_CUBIC);
            comboMipMapFilter.Items.Add(Resources.GORIMG_TEXT_FILTER_FANT);
            comboResizeFilter.Items.Add(Resources.GORIMG_TEXT_FILTER_POINT);
            comboResizeFilter.Items.Add(Resources.GORIMG_TEXT_FILTER_LINEAR);
            comboResizeFilter.Items.Add(Resources.GORIMG_TEXT_FILTER_CUBIC);
            comboResizeFilter.Items.Add(Resources.GORIMG_TEXT_FILTER_FANT);
        }

		/// <summary>
		/// Function to commit any settings.
		/// </summary>
		public override void CommitSettings()
		{
			GorgonImageEditorPlugIn.Settings.UseAnimations = checkShowAnimations.Checked;
			GorgonImageEditorPlugIn.Settings.StartWithActualSize = checkShowActualSize.Checked;

		    GorgonImageEditorPlugIn.Settings.MipFilter =
		        _filterTypes.First(item =>
		                           string.Equals(item.Value, comboMipMapFilter.Text, StringComparison.CurrentCultureIgnoreCase))
		                    .Key;

		    GorgonImageEditorPlugIn.Settings.ResizeImageFilter =
		        _filterTypes.First(item => string.Equals(item.Value, comboResizeFilter.Text, StringComparison.CurrentCulture))
		                    .Key;

            // If we have an image open, then do not save the codec information.
		    if (!(Content is GorgonImageContent))
		    {
		        GorgonImageEditorPlugIn.Settings.CustomCodecs.Clear();
		        foreach (CodecDescriptor codec in _codecs)
		        {
		            GorgonImageEditorPlugIn.Settings.CustomCodecs.Add(codec.CodecPath);
		        }

                var imagePlugIn = (GorgonImageEditorPlugIn)PlugIn;

                // Refresh the codec list.
                imagePlugIn.GetCodecs();
		    }


			GorgonImageEditorPlugIn.Settings.Save();
		}

		/// <summary>
		/// Function to read the current settings into their respective controls.
		/// </summary>
		public override void InitializeSettings()
		{
			checkShowAnimations.Checked = GorgonImageEditorPlugIn.Settings.UseAnimations;
			checkShowActualSize.Checked = GorgonImageEditorPlugIn.Settings.StartWithActualSize;

		    comboMipMapFilter.Text = _filterTypes[GorgonImageEditorPlugIn.Settings.MipFilter];
		    comboResizeFilter.Text = _filterTypes[GorgonImageEditorPlugIn.Settings.ResizeImageFilter];
            
			// Get the list of already loaded image codecs.
			_codecs = (from plugIn in Gorgon.PlugIns
			           let codec = plugIn as GorgonCodecPlugIn
			           where codec != null
			           orderby codec.Name
			           select new CodecDescriptor
			                  {
				                  CodecTypeName = codec.Name,
								  CodecName = codec.GetType().Name,
								  CodecDesc = codec.Description,
								  CodecPath = codec.PlugInPath
			                  }).ToList();

			// Look through the list of codecs and load them as necessary.
			foreach (string codecPath in GorgonImageEditorPlugIn.Settings.CustomCodecs)
			{
				// If we've already loaded this guy, then do nothing.
				if (_codecs.Any(item => string.Equals(codecPath, item.CodecPath, StringComparison.OrdinalIgnoreCase)))
				{
					continue;
				}

				_codecs.AddRange(LoadCodec(codecPath));
			}

			FillCodecList();

			ValidateControls();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="PanelImagePreferences"/> class.
		/// </summary>
		public PanelImagePreferences()
		{
			InitializeComponent();
		}
		#endregion
	}
}
