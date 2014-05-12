using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIR.IO;

namespace AIR.Simulation
{
    public abstract partial class FlightController
    {
        #region Public Properties

        /// <summary>
        /// Settings of controller
        /// </summary>
        public Setting Settings = new Setting();

        /// <summary>
        /// Behavior
        /// </summary>
        public Action Behavior;

        #endregion Public Properties

        #region Private Properties

        /// <summary>
        /// Udp client
        /// </summary>
        UdpClientSocket Udp;

        /// <summary>
        /// Timer to send control
        /// </summary>
        System.Timers.Timer ControlTimer;

        #endregion Private Properties

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public FlightController()
        {
            InitializeVariables();
        }

        /// <summary>
        /// Initiallize all xml variables
        /// </summary>
        void InitializeVariables()
        {
            //acquire members
            FieldInfo[] fs = this.GetType().GetFields();
            PropertyInfo[] ps = this.GetType().GetProperties();
            List<MemberInfo> members = new List<MemberInfo>();
            members.AddRange(fs);
            members.AddRange(ps);
            //process members
            foreach (var m in members.ToArray())
            {
                //first consider its currect name as name
                string name = m.Name;
                Type rawType = m.MemberType == MemberTypes.Field ? ((FieldInfo)m).FieldType : ((PropertyInfo)m).PropertyType;
                string format = FormatMap.ContainsKey(rawType) ? FormatMap[rawType] : "%s";
                string type = TypeMap[format];
                string node = "";
                //read attribute name if exist
                var nodeAttrs = m.GetCustomAttributes(typeof(NodeAttribute), true);
                var nodeAttr = nodeAttrs.Length > 0 ? (NodeAttribute)nodeAttrs[0] : null;
                node = nodeAttr == null ? node : nodeAttr.Node;
                //read inbound
                if (m.GetCustomAttributes(typeof(ToSendAttribute), true).Length > 0)
                {
                    //define reference
                    Setting.Chunk chunk = new Setting.Chunk();
                    chunk.Name = name;
                    chunk.Format = format;
                    chunk.Node = node;
                    chunk.Type = type;
                    this.Settings.Input.Chucks.Add(chunk);
                    continue;
                }
                //read outbound
                if (m.GetCustomAttributes(typeof(ToReceiveAttribute), true).Length > 0)
                {
                    //define reference
                    Setting.Chunk chunk = new Setting.Chunk();
                    chunk.Name = name;
                    chunk.Format = format;
                    chunk.Node = node;
                    chunk.Type = type;
                    this.Settings.Output.Chucks.Add(chunk);
                    continue;
                }
            }
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Start Controller
        /// </summary>
        public void Start()
        {
            ReadGeneric();
            //udp
            Udp = new UdpClientSocket(Settings.HostEndpoint.Address, Settings.HostEndpoint.Port,
                Settings.FlightGearEndPoint.Address, Settings.FlightGearEndPoint.Port);
            Udp.PackageReceived = Udp_Received;
            Udp.Start();
            //timer
            ControlTimer = new System.Timers.Timer();
            ControlTimer.Interval = Settings.ControlInterval;
            ControlTimer.Elapsed += Timer_Tick;
            ControlTimer.Start();
            //start behavior
            System.Threading.ThreadPool.QueueUserWorkItem((obj) => Behavior());
        }

        /// <summary>
        /// Write generic setting of controller class
        /// </summary>
        /// <returns></returns>
        public bool TryWriteGeneric()
        {
            //make sure path exist
            if (!Directory.Exists(Settings.InstallationPath))
                return false;
            try
            {
                string xml_out = Settings.WriteSchema(false, true);
                string xml_in = Settings.WriteSchema(true, false);
                //delete local and target copy
                File.Delete(Settings.GenericNameOut);
                File.Delete(Settings.GenericNameIn);
                //write local copy
                File.WriteAllText(Settings.GenericNameOut, xml_out);
                File.WriteAllText(Settings.GenericNameIn, xml_in);
                //copy over localcopy
                File.Copy(Settings.GenericNameOut, Settings.ProtocolPathOut, true);
                File.Copy(Settings.GenericNameIn, Settings.ProtocolPathIn, true);
                //delete local copy
                File.Delete(Settings.GenericNameOut);
                File.Delete(Settings.GenericNameIn);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Read generic form file
        /// </summary>
        bool ReadGeneric()
        {
            //make sure path exist
            if (!File.Exists(Settings.ProtocolPathOut))
                return false;
            string xml = File.ReadAllText(Settings.ProtocolPathOut);
            Settings.ReadSchema(xml);
            return true;
        }

        /// <summary>
        /// Action to do when inbound udp package received
        /// </summary>
        /// <param name="bytes"></param>
        void Udp_Received(byte[] bytes)
        {
            string[] receive = Encoding.ASCII.GetString(bytes).Split(Settings.Output.VarSeparator);
            for (int i = 0; i < receive.Length && i < Settings.Output.Chucks.Count; i++)
            {
                //check field
                FieldInfo field = this.GetType().GetField(Settings.Output.Chucks[i].Name);
                if (field != null)
                {
                    field.SetValue(this, Convert.ChangeType(receive[i], field.FieldType));
                    continue;
                }
                //check property
                PropertyInfo prop = this.GetType().GetProperty(Settings.Output.Chucks[i].Name);
                if (prop != null)
                {
                    prop.SetValue(this, Convert.ChangeType(receive[i], prop.PropertyType), null);
                    continue;
                }
            }
        }

        /// <summary>
        /// Action to do when timer tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Timer_Tick(object sender, System.Timers.ElapsedEventArgs args)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var chunk in Settings.Input.Chucks)
            {
                //check field
                FieldInfo field = this.GetType().GetField(chunk.Name);
                if (field != null)
                {
                    string value = field.GetValue(this).ToString();
                    if (field.FieldType == typeof(bool))
                        value = value.ToLower();
                    if (!first)
                        sb.Append(Settings.Input.VarSeparator);
                    sb.Append(value);
                    first = false;
                    continue;
                }
                //check property
                PropertyInfo prop = this.GetType().GetProperty(chunk.Name);
                if (prop != null)
                {
                    string value = prop.GetValue(this, null).ToString();
                    if (prop.PropertyType == typeof(bool))
                        value = value.ToLower();
                    if (!first)
                        sb.Append(Settings.Input.VarSeparator);
                    sb.Append(value);
                    first = false;
                    continue;
                }
            }
            sb.Append(Settings.Input.LineSeparator);
            Udp.Send(Encoding.ASCII.GetBytes(sb.ToString()));
        }

        #endregion Private Methods

        #region Attributes

        /// <summary>
        /// Define overrided node for xml
        /// </summary>
        public class NodeAttribute : Attribute
        {
            /// <summary>
            /// Node to use in xml
            /// </summary>
            public string Node { get; private set; }

            /// <summary>
            /// Constructor of XML node attribute
            /// </summary>
            /// <param name="Name"></param>
            public NodeAttribute(string Node)
            {
                this.Node = Node;
            }
        }

        /// <summary>
        /// Indicate the variable as out bound control
        /// </summary>
        public class ToSendAttribute : Attribute
        { }

        /// <summary>
        /// Indicate the variable as in bound sensor or feedback
        /// </summary>
        public class ToReceiveAttribute : Attribute
        { }

        #endregion Attributes
    }
}
