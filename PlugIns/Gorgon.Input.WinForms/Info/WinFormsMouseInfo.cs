using System;
using System.Windows.Forms;
using Gorgon.Input.WinForms.Properties;

namespace Gorgon.Input.WinForms
{
	/// <inheritdoc/>
	class WinFormsMouseInfo
		: IGorgonMouseInfo
	{
		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="WinFormsMouseInfo"/> class.
		/// </summary>
		public WinFormsMouseInfo()
		{
			UUID = Guid.Empty;
			Name = Resources.GORINP_WINFORMS_MOUSE_DESC;
			ClassName = "Mouse";
			HumanInterfaceDevicePath = "SystemMouse";

			ButtonCount = SystemInformation.MouseButtons;
			SamplingRate = 0;
			HasHorizontalWheel = false;
		}
		#endregion

		#region IGorgonMouseInfo Members
		/// <inheritdoc/>
		public int ButtonCount
		{
			get;
		}

		/// <inheritdoc/>
		public int SamplingRate
		{
			get;
		}

		/// <inheritdoc/>
		public bool HasHorizontalWheel
		{
			get;
		}
		#endregion

		#region IGorgonInputDeviceInfo Members
		/// <inheritdoc/>
		public Guid UUID
		{
			get;
		}

		/// <inheritdoc/>
		public string HumanInterfaceDevicePath
		{
			get;
		}

		/// <inheritdoc/>
		public string ClassName
		{
			get;
		}

		/// <inheritdoc/>
		public InputDeviceType InputDeviceType => InputDeviceType.Mouse;

		#endregion

		#region IGorgonNamedObject Members
		/// <inheritdoc/>
		public string Name
		{
			get;
		}
		#endregion
	}
}
