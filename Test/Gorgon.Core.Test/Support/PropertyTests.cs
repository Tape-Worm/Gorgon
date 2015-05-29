namespace Gorgon.Core.Test.Support
{
	class PropertyTests
	{
		public int MyProperty
		{
			get;
			set;
		}

		public int MyWriteOnlyProperty
		{
			private get;
			set;
		}

		public string MyReadOnlyProperty
		{
			get
			{
				return MyWriteOnlyProperty.ToString();
			}
		}

		public string MyReadOnlyAutoProperty
		{
			get;
			private set;
		}

		public PropertyTests()
		{
			MyReadOnlyAutoProperty = "MyAuto";
		}
	}
}
