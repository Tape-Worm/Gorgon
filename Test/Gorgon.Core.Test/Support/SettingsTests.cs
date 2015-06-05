using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Configuration;

namespace Gorgon.Core.Test.Support
{
	public class Settings
		: GorgonApplicationSettings
	{
		[GorgonApplicationSetting("MySection", DefaultValue = 123)]
		public int Value
		{
			get;
			set;
		}

		[GorgonApplicationSetting("MySection", SettingName = "YetMore")]
		public int AnotherValue
		{
			get;
			set;
		}

		[GorgonApplicationSetting("MySection")]
		public string StrValue
		{
			get;
			set;
		}

		[GorgonApplicationSetting("Arrays")]
		public int[] IntArray
		{
			get;
			private set;
		}

		[GorgonApplicationSetting("Arrays")]
		public string[] NamesOfStuff
		{
			get;
			set;
		}

		[GorgonApplicationSetting("Lists")]
		public IList<float> Floats
		{
			get;
			private set;
		}

		[GorgonApplicationSetting("DateSection")]
		public DateTime TehDate
		{
			get;
			set;
		}

		[GorgonApplicationSetting("Dictionaries")]
		public IDictionary<string, DateTime> DateGuids
		{
			get;
			private set;
		}

		public Settings()
			: base("SettingsTests")
		{
			StrValue = "Abc";
			IntArray = new int[5];
			NamesOfStuff = null;
			Floats = new List<float>();
			DateGuids = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
		}
	}
}
