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
			Name = Resources.GORINP_WINFORMS_MOUSE_NAME;
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
			private set;
		}

		/// <inheritdoc/>
		public int SamplingRate
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public bool HasHorizontalWheel
		{
			get;
			private set;
		}
		#endregion

		#region IGorgonInputDeviceInfo Members
		/// <inheritdoc/>
		public Guid UUID
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public string HumanInterfaceDevicePath
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public string ClassName
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public InputDeviceType InputDeviceType
		{
			get
			{
				return InputDeviceType.Mouse;
			}
		}
		#endregion

		#region IGorgonNamedObject Members
		/// <inheritdoc/>
		public string Name
		{
			get;
			private set;
		}
		#endregion
	}
}
