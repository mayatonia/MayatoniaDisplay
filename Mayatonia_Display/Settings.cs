using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace MayatoniaDisplay
{
    [Serializable]
    [XmlRoot("SETTINGS")]
    public class Settings
    {

        private static Settings _Instance;
        private List<DisplayConfiguration> _DisplayList = new List<DisplayConfiguration>();
        private static String _ConfigFileName = "Configuration.xml";
        private List<Connection> _ConnectionPoints = new List<Connection>();
        private int _AutoStartSeconds = 0; //Defaults to 0

        private String _Banner_Text = "Mayatonia Display - Washington D.C.";
        private String _Office_Abbreviation = "EST";
        private bool _DynamicallyAssignDisplays = false;

        [XmlArray("DISPLAYS")]
        [XmlArrayItem("DISPLAY")]
        public List<DisplayConfiguration> DisplayList
        {
            get
            {
                return _DisplayList;
            }

            set
            {
                _DisplayList = value;
            }
        }

        [XmlIgnore]
        public static Settings Instance
        {
            get
            {
                return _Instance;
            }

            set
            {
                _Instance = value;
            }
        }


        [XmlArray("CONNECTIONS")]
        [XmlArrayItem("CONNECTON")]
        public List<Connection> ConnectionPoints
        {
            get
            {
                return _ConnectionPoints;
            }

            set
            {
                _ConnectionPoints = value;
            }
        }


        [XmlIgnore]
        public String ConnectionsPointsString
        {
            get
            {
                String connString = String.Empty;
                foreach (Connection con in this._ConnectionPoints)
                {
                    connString += String.Format("{0}:{1}:{2}:{3}/", con.Title, con.Host, con.Port, con.Timeout);
                }

                return connString;
            }
            set
            {
                List<Connection> lstConnections = new List<Connection>();


                String ConnString = value.Trim();
                if (ConnString.Length > 0)
                { 
                //Parse the Connection String which is input in the following format, each entry seperated by
                // a forward / slash.
                //=========================================================
                //         Title:Host:Port:Timeout/Title:Host:Port:Timeout/...
                //=========================================================
                String[] arrConnections = ConnString.Split('/');
                int nInterations = 0;
                    foreach (String strConn in arrConnections)
                    {
                        nInterations++;
                        try
                        {
                            if (!String.IsNullOrEmpty(strConn))
                            {
                                String[] arrConnDetail = strConn.Split(':');
                                String Title = arrConnDetail[0];
                                String Host = arrConnDetail[1];
                                int Port = 80; //Port 80 i.e. HTTP
                                try
                                {
                                    Port = Convert.ToInt32(arrConnDetail[2]);
                                }
                                catch { } // Port is missing, since it is optional, it is set the default port

                                int Timeout = 2000; //2 Seconds
                                try
                                {
                                    Timeout = Convert.ToInt32(arrConnDetail[3]);
                                }
                                catch { } //Timeout is missing, it is an optional value therefore it is set to the default

                                Connection newConn = new Connection(Title, Host, Port, Timeout);
                                lstConnections.Add(newConn);
                            }
                        }
                        catch
                        {
                            //An error errored while parsing this connection string entry. Notify the user
                            //of the error and the ordinal value where the error is identified.

                            String Message = String.Format("An error has been identified. Connection #{0} has a malformed parameter, \r\nplease ensure the entry is properly inputted in the below format: \r\n\r\nTitle:Host:Port:Timeout/Title:Host:Port:Timeout/...", nInterations);
                            System.Windows.MessageBox.Show(Message, "Connection Parsing Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            return;
                        }
                    }

                    //Set the ConnectionPoints object to the new list (it can be empty if no string is specified
                    ConnectionPoints = lstConnections;

                }



            }
        }

        
        [XmlAttribute("AUTOSTART")]
        public int AutoStartSeconds
        {
            get
            {
                return _AutoStartSeconds;
            }

            set
            {
                _AutoStartSeconds = value;
            }
        }

        [XmlAttribute("BANNER_TEXT")]
        public string Banner_Text
        {
            get
            {
                return _Banner_Text;
            }

            set
            {
                _Banner_Text = value;
            }
        }

        [XmlAttribute("OFFICE_ABBRV")]
        public string Office_Abbreviation
        {
            get
            {
                return _Office_Abbreviation;
            }

            set
            {
                _Office_Abbreviation = value;
            }
        }

        [XmlAttribute("DYNAMIC_ASSIGNMENT")]
        public bool DynamicallyAssignDisplays
        {
            get
            {
                return _DynamicallyAssignDisplays;
            }

            set
            {
                _DynamicallyAssignDisplays = value;
            }
        }

        public Settings()
        {
    

        }

        /// <summary>
        /// Save the Settings
        /// </summary>
        /// <param name="settings"></param>
        public static void Save(Settings settings)
        {
            using (FileStream fs = new FileStream(_ConfigFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {                
                    XmlSerializer ser = new XmlSerializer(settings.GetType());
                    
                    ser.Serialize(fs, settings);                                                        

            }
        }

        /// <summary>
        /// Load the settings
        /// </summary>
        /// <returns></returns>
        public static Settings Load()
        {
            Settings sets = new Settings();

            try
            {
                if (File.Exists(_ConfigFileName))
                {
                    using (FileStream fs = new FileStream(_ConfigFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        XmlSerializer ser = new XmlSerializer(sets.GetType());
                        sets = ser.Deserialize(fs) as Settings;

                        if (sets.ConnectionPoints.Count == 0)
                        {
                            sets.ConnectionPoints.Add(new Connection("Internet", "www.google.com", 80, 2000));
                        }

                    }
                }
            }
            catch(Exception ex)
            {
                ex.ToString();
            }

            return sets;
                
        }


    }

    [Serializable]
    public class DisplayConfiguration
    {

        private int _DisplayIndex = 0;
        private List<DisplaySource> _Sources = new List<DisplaySource>();
        private int _DisplayTypeID = 1;
        private Boolean _DisplayEnabled = false;
        private Boolean _DisplayHeader = false;
        private Boolean _DisplayNetworkStatus = false;



        [XmlAttribute("TYPE")]
        /// <summary>
        /// 1 = Single Display on Monitor
        /// 2 = Four displays on Monitor
        /// 3 = Hybrid display consisting of 1 large Display and 3 small displays
        /// 4 = External Application -- The program will attempt to frame the external application's window
        /// </summary>        
        public int DisplayTypeID
        {
            get
            {
                return _DisplayTypeID;
            }

            set
            {
                _DisplayTypeID = value;
            }
        }

        [XmlArray("SOURCES")]
        [XmlArrayItem("SOURCE")]     
        public List<DisplaySource> Sources
        {
            get
            {
                return _Sources;
            }

            set
            {
                _Sources = value;
            }
        }


        ///// <summary>
        ///// ONLY FOR SERIALIZATION
        ///// </summary>
        //[XmlArray("URLSOURCES")]
        //[XmlArrayItem("URLSOURCE")]
        //public List<URLSource> URLSources
        //{
        //    get
        //    {
        //        List<URLSource> lst = new List<URLSource>();
        //        foreach (DisplaySource src in _Sources)
        //            if (src is DisplaySource)
        //                lst.Add(src as URLSource);

        //        return lst;
        //    }
        //    set
        //    {
        //        if (_Sources == null)
        //            _Sources = new List<DisplaySource>();

        //        foreach (URLSource url in value)
        //            _Sources.Add(url);
                    
        //    }
        //}

        ///// <summary>
        ///// ONLY FOR SERIALIZATION
        ///// </summary>
        //[XmlArray("APPSOURCES")]
        //[XmlArrayItem("APPSOURCE")]
        //public List<AppSource> APPSources
        //{
        //    get
        //    {
        //        List<AppSource> lst = new List<AppSource>();
        //        foreach (DisplaySource src in _Sources)
        //            if (src is AppSource)
        //                lst.Add(src as AppSource);

        //        return lst;
        //    }
        //    set
        //    {
        //        if (_Sources == null)
        //            _Sources = new List<DisplaySource>();

        //        foreach (AppSource app in value)
        //            _Sources.Add(app);
        //    }
        //}

        [XmlAttribute("INDEX")]
        public int DisplayIndex
        {
            get
            {
                return _DisplayIndex;
            }

            set
            {
                _DisplayIndex = value;
            }
        }

        [XmlAttribute("ENABLED")]
        public bool DisplayEnabled
        {
            get
            {
                return _DisplayEnabled;
            }

            set
            {
                _DisplayEnabled = value;
            }
        }

        [XmlAttribute("HEADER")]
        public bool DisplayHeader
        {
            get
            {
                return _DisplayHeader;
            }

            set
            {
                _DisplayHeader = value;
            }
        }

        [XmlAttribute("NETWORK")]
        public bool DisplayNetworkStatus
        {
            get
            {
                return _DisplayNetworkStatus;
            }

            set
            {
                _DisplayNetworkStatus = value;
            }
        }

        public DisplayConfiguration()
        {

         
        }

        public DisplayConfiguration Copy()
        {
            DisplayConfiguration copy = this.MemberwiseClone() as DisplayConfiguration;

            foreach (DisplaySource src in this.Sources)
                copy.Sources.Add(src.Copy());

            return copy;
        }


    }

    [Serializable]
    public class Connection
    {
        private String _Title = String.Empty;
        private String _Host = String.Empty;
        private int _Port = 0;
        private int _Timeout = 0;

        [XmlAttribute("TITLE")]
        public string Title
        {
            get
            {
                return _Title;
            }

            set
            {
                _Title = value;
            }
        }

        [XmlAttribute("HOST")]
        public string Host
        {
            get
            {
                return _Host;
            }

            set
            {
                _Host = value;
            }
        }

        [XmlAttribute("PORT")]
        public int Port
        {
            get
            {
                return _Port;
            }

            set
            {
                _Port = value;
            }
        }

        [XmlAttribute("TIMEOUT")]
        public int Timeout
        {
            get
            {
                return _Timeout;
            }

            set
            {
                _Timeout = value;
            }
        }

        public Connection()
        {


        }


        /// <summary>
        /// Create a connection object
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="Host"></param>
        /// <param name="Port"></param>
        /// <param name="Timeout"></param>
        public Connection(String Title, String Host, int Port, int Timeout)
        {
            this._Title = Title;
            this._Host = Host;
            this._Port = Port;
            this._Timeout = Timeout;
        }

    }

    [Serializable]
    [XmlInclude(typeof(DisplaySource)), XmlInclude(typeof(URLSource)), XmlInclude(typeof(AppSource))]
    public class DisplaySource
    {
        private int _RefreshInterval = 10; //10 Minutes
        private int _OnScreenDuration = 5; //5 Minutes
        private String _Name = String.Empty;
        private int _Height = 768; //Defualt to 768 pixels
        private int _Width = 1024; //Default to 1024 pixels
        private bool _DimensionsAutomatic = false;
        private Double _Zoom = 1.0;

        public DisplaySource()
        {

        }

        [XmlAttribute("REFRESHINTERVAL")]
        public int RefreshInterval
        {
            get
            {
                return _RefreshInterval;
            }

            set
            {
                _RefreshInterval = value;
            }
        }

        [XmlAttribute("PLAYINTERVAL")]
        public int OnPlayDuration
        {
            get
            {
                return _OnScreenDuration;
            }

            set
            {
                _OnScreenDuration = value;
            }
        }

        [XmlAttribute("NAME")]
        public string Name
        {
            get
            {
                return _Name;
            }

            set
            {
                _Name = value;
            }
        }

        [XmlAttribute("HEIGHT")]
        public int Height
        {
            get
            {
                return _Height;
            }

            set
            {
                _Height = value;
            }
        }

        [XmlAttribute("WIDTH")]
        public int Width
        {
            get
            {
                return _Width;
            }

            set
            {
                _Width = value;
            }
        }

        [XmlAttribute("AUTO_DIMENSIONS")]
        public bool DimensionsAutomatic
        {
            get
            {
                return _DimensionsAutomatic;
            }

            set
            {
                _DimensionsAutomatic = value;
            }
        }

        [XmlAttribute("ZOOM")]
        public double Zoom
        {
            get
            {
                return _Zoom;
            }

            set
            {
                _Zoom = value;
            }
        }

        public DisplaySource Copy()
        {
            return this.MemberwiseClone() as DisplaySource;
        }
    }

    [Serializable]
    public class URLSource : DisplaySource
    {
        private String _URL = String.Empty;

  

        public URLSource()
        {

        }

        
        [XmlAttribute("URL")]
        public string URL
        {
            get
            {
                return _URL;
            }

            set
            {
                _URL = value;
            }
        }



        public new URLSource Copy()
        {
            return this.MemberwiseClone() as URLSource;
        }


        public override string ToString()
        {
               return String.Format("Name: {0} - Type: URL - Duration: {1:##} - Refresh Rate: {2:##} - Source: {3}", this.Name, this.OnPlayDuration, this.RefreshInterval, this.URL);
        }

        public override bool Equals(object obj)
        {
            return this.ToString().Equals(obj.ToString());
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [Serializable]
    public class AppSource : DisplaySource
    {
        private String _Path = String.Empty;
        private String _Parameters = String.Empty;

        public AppSource()
        {

        }

        [XmlAttribute("PATH")]
        public string Path
        {
            get
            {
                return _Path;
            }

            set
            {
                _Path = value;
            }
        }

        [XmlAttribute("PARAMS")]
        public string Parameters
        {
            get
            {
                return _Parameters;
            }

            set
            {
                _Parameters = value;
            }
        }

        public new AppSource Copy()
        {
            return this.MemberwiseClone() as AppSource;
        }


        public override string ToString()
        {
            return String.Format("Name: {0} - Type: App - Path: {1} {2}", this.Name, this.Path, this.Parameters);
        }

        public override bool Equals(object obj)
        {
            return this.ToString().Equals(obj.ToString());
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }


}
