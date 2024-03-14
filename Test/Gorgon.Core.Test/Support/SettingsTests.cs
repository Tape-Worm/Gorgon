using System;
using System.Collections.Generic;
using System.Drawing;
using Gorgon.Configuration;

namespace Gorgon.Core.Test.Support
{
    public enum SettingEnum
    {
        Enum1 = 0,
        Enum2 = 1,
        Enum3 = 2
    }

    public class Settings
        : GorgonApplicationSettings
    {
        [GorgonApplicationSetting("ValueTypes")]
        public Color TehColor
        {
            get;
            set;
        }

        [GorgonApplicationSetting("ValueTypes")]
        public Rectangle Rectangle
        {
            get;
            set;
        }

        [GorgonApplicationSetting("MySection", DefaultValue = SettingEnum.Enum2)]
        public SettingEnum EnumValue
        {
            get;
            set;
        }

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

        public Settings(Version version = null)
            : base("SettingsTests", version, null)
        {
            StrValue = "Abc";
            IntArray = new int[5];
            NamesOfStuff = null;
            Floats = new List<float>();
            DateGuids = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
