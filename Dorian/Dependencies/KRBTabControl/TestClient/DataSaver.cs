using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace TestClient
{
    #region Enum

    public enum SerializerType
    {
        Xml, Binary
    };

    #endregion

    // If you choose xml serialization type, you should be create your color converter for color struct properties.
    [DataSaverAttribute("Settings", TypeSerializer = SerializerType.Binary)]
    class DataSaver
    {
        #region Destructor

        ~DataSaver()
        {
            GC.SuppressFinalize(this);
        }

        #endregion

        #region General Methods

        public static void SaveData(ApplicationSettings settings)
        {
            DataSaverAttribute attribute = GetAttribute();

            if (attribute != null && settings != null)
            {
                try
                {
                    string fileName = null;
                    switch (attribute.TypeSerializer)
                    {
                        case SerializerType.Binary:
                            fileName = String.Format("{0}.{1}", attribute.FileName, "dat");
                            using (Stream writeStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                            {
                                IFormatter serializer = new BinaryFormatter();
                                serializer.Serialize(writeStream, settings);
                            }
                            break;
                        default:
                            fileName = String.Format("{0}.{1}", attribute.FileName, "xml");
                            using (Stream writeStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(ApplicationSettings));
                                serializer.Serialize(writeStream, settings);
                            }
                            break;
                    }

                    MessageBox.Show("The data saving operation is successfully completed.");
                }
                catch
                {
                    MessageBox.Show("Failure: The data saving operation is not completed.");
                }
            }
            else
            {
                MessageBox.Show("Failure: The data saving option is not implemented.");
            }
        }

        public static ApplicationSettings LoadData()
        {
            ApplicationSettings settings = null;
            
            DataSaverAttribute attribute = GetAttribute();
            if (attribute != null)
            {
                try
                {
                    string fileName = null;
                    switch (attribute.TypeSerializer)
                    {
                        case SerializerType.Binary:
                            fileName = String.Format("{0}.{1}", attribute.FileName, "dat");
                            using (Stream readStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                            {
                                IFormatter serializer = new BinaryFormatter();
                                settings = (ApplicationSettings)serializer.Deserialize(readStream);
                            }
                            break;
                        default:
                            fileName = String.Format("{0}.{1}", attribute.FileName, "xml");
                            using (Stream readStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(ApplicationSettings));
                                settings = (ApplicationSettings)serializer.Deserialize(readStream);
                            }
                            break;
                    }
                }
                catch
                {
                    MessageBox.Show("Failure: The data reading operation is not completed.");
                }
            }
            else
            {
                MessageBox.Show("Failure: The data reading option is not implemented.");
            }

            return settings;
        }

        #endregion

        #region Helper Methods

        private static DataSaverAttribute GetAttribute()
        {
            DataSaverAttribute attribute = (DataSaverAttribute)Attribute.GetCustomAttribute(typeof(DataSaver), typeof(DataSaverAttribute));
            return attribute;
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    class DataSaverAttribute : Attribute
    {
        #region Destructor

        ~DataSaverAttribute()
        {
            GC.SuppressFinalize(this);
        }

        #endregion

        // Attribute constructor for positional parameters.
        public DataSaverAttribute(string fileName)
        {
            this.FileName = fileName;
        }

        // Accessor
        public string FileName { get; private set; }
        // Property for named parameter.
        public SerializerType TypeSerializer { get; set; }
    }

    [Serializable]
    public class ApplicationSettings
    {
        #region Constructor

        public ApplicationSettings()
        { }
        
        public ApplicationSettings(Point formLocation)
        {
            this.FormLocation = formLocation;
        }

        #endregion
        
        #region Destructor

        ~ApplicationSettings()
        {
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Property

        // Form location point.
        public Point FormLocation { get; set; }

        // First TabItem Color.
        public Color FirstTabItemColor { get; set; }
        
        // Second TabItem Color.
        public Color SecondTabItemColor { get; set; }

        // Selected TabItem Text Color.
        public Color SelectedTabItemTextColor { get; set; }

        // Value of the first tab item horizontal offset.
        public int HOffset { get; set; }

        // Selected tab page name.
        public string SelectedTabPageName { get; set; }

        // RGBA Values of CaptionRandomizer property.
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }
        public byte Alpha { get; set; }

        // Determines whether the tab header background is draw or not.
        public bool IsDrawHeader { get; set; }

        // Determines whether the tab control caption is visible or not.
        public bool IsCaptionVisible { get; set; }

        // Determines whether the tab control's edge border is draw or not, you must set the IsCaptionVisible's value to false for this change to take effect.
        public bool IsDrawEdgeBorder { get; set; }

        // Determines whether the active tab is stretched to its parent container or not.
        public bool IsStretchToParent { get; set; }

        // Determines whether the tab separator line is visible or not between the tab pages.
        public bool IsDrawTabSeparator { get; set; }

        // Determines whether the randomizer effect is enable or not for tab control caption.
        public bool IsRandomizerEnabled { get; set; }

        // Border Style.
        public KRBTabControl.KRBTabControl.ControlBorderStyle BorderStyles { get; set; }

        // TabPage Style.
        public KRBTabControl.KRBTabControl.TabStyle TabPageStyles { get; set; }

        // Tab Alingment.
        public KRBTabControl.KRBTabControl.TabAlignments TabAlingments { get; set; }

        // TabItem BackgrundStyle.
        public KRBTabControl.KRBTabControl.TabHeaderStyle HeaderStyles { get; set; }

        #endregion
    }
}