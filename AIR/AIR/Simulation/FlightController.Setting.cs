using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;

namespace AIR.Simulation
{
    /// <summary>
    /// Define Flight Gear XML Schema
    /// </summary>
    public abstract partial class FlightController
    {
        #region Static Reference

        /// <summary>
        /// Symbol map
        /// </summary>
        public static readonly Dictionary<char, string> SympolMap = new Dictionary<char, string>()
        {
            {'\n', "newline"},
            {'\t', "tab"},
            {'\f', "formfeed"},
            {'\r', "carriagereturn"},
            {'\v', "verticaltab"}
        };

        /// <summary>
        /// Format map
        /// </summary>
        public static readonly Dictionary<Type, string> FormatMap = new Dictionary<Type, string>()
        {
            {typeof(string),"%s"},
            {typeof(Int64),"%d"},
            {typeof(UInt64),"%d"},
            {typeof(Int32),"%d"},
            {typeof(UInt32),"%d"},
            {typeof(Int16),"%d"},
            {typeof(UInt16),"%d"},
            {typeof(char),"%d"},
            {typeof(byte),"%d"},
            {typeof(double),"%f"},
            {typeof(float),"%f"},
        };

        /// <summary>
        /// Type map
        /// </summary>
        public static readonly Dictionary<string, string> TypeMap = new Dictionary<string, string>()
        {
            {"%s", "string"},
            {"%d", "integer"},
            {"%f", "float"},
        };


        #endregion Static Reference

        #region Settings

        /// <summary>
        /// Settings of flight controller
        /// </summary>
        public class Setting
        {
            #region Structs

            /// <summary>
            /// Chunk of data
            /// </summary>
            public struct Chunk
            {
                /// <summary>
                /// Node directory
                /// </summary>
                public string Node;
                /// <summary>
                /// Name of variable
                /// </summary>
                public string Name;
                /// <summary>
                /// Name of format
                /// </summary>
                public string Format;
                /// <summary>
                /// Name of type
                /// </summary>
                public string Type;
            }
            /// <summary>
            /// Output Settings of schema
            /// </summary>
            public struct OutputSettings
            {
                /// <summary>
                /// Header of package
                /// </summary>
                public string Preamble;
                /// <summary>
                /// Trailer of package
                /// </summary>
                public string Postamble;
                /// <summary>
                /// Token to separate lines
                /// </summary>
                public char LineSeparator;
                /// <summary>
                /// Token to separate varaibles
                /// </summary>
                public char VarSeparator;
                /// <summary>
                /// If output is in binary mode
                /// </summary>
                public bool BinaryMode;
                /// <summary>
                /// Output Chunks
                /// </summary>
                public List<Chunk> Chucks;
            }
            /// <summary>
            /// Input Settings of schema
            /// </summary>
            public struct InputSettings
            {
                /// <summary>
                /// Token to separate lines
                /// </summary>
                public char LineSeparator;
                /// <summary>
                /// Token to separate varaibles
                /// </summary>
                public char VarSeparator;
                /// <summary>
                /// Input Chunks
                /// </summary>
                public List<Chunk> Chucks;
            }

            #endregion Structs

            /// <summary>
            /// Input settings
            /// </summary>
            public InputSettings Input;

            /// <summary>
            /// Output settings
            /// </summary>
            public OutputSettings Output;

            /// <summary>
            /// Installation path of flight gear
            /// </summary>
            public string InstallationPath;

            /// <summary>
            /// Generic Name for output
            /// </summary>
            public string GenericNameOut;

            /// <summary>
            /// Generic Name for input
            /// </summary>
            public string GenericNameIn;

            /// <summary>
            /// Interval of control in ms
            /// </summary>
            public int ControlInterval;

            /// <summary>
            /// IP end point of software
            /// </summary>
            public IPEndPoint HostEndpoint;

            /// <summary>
            /// IP end point of flight gear
            /// </summary>
            public IPEndPoint FlightGearEndPoint;

            /// <summary>
            /// Get protocol path out
            /// </summary>
            public string ProtocolPathOut
            {
                get
                {
                    return InstallationPath + @"\data\Protocol\" + GenericNameOut;
                }
            }

            /// <summary>
            /// Get protocol path out
            /// </summary>
            public string ProtocolPathIn
            {
                get
                {
                    return InstallationPath + @"\data\Protocol\" + GenericNameIn;
                }
            }

            /// <summary>
            /// Cosntructor
            /// </summary>
            public Setting()
            {
                //get program folder path
                string prog = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
                prog = prog.Replace("(x86)", "").Trim();
                InstallationPath = prog + @"\FlightGear";
                GenericNameOut = "generic_cs_out.xml";
                GenericNameIn = "generic_cs_in.xml";
                Input.LineSeparator = '\n';
                Input.VarSeparator = '|';
                Input.Chucks = new List<Chunk>();
                Output.LineSeparator = '\n';
                Output.VarSeparator = '|';
                Output.Chucks = new List<Chunk>();
                HostEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5510);
                FlightGearEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5511);
                ControlInterval = 100;
            }

            /// <summary>
            /// Read a Fligher Gear XML Schema form xml
            /// </summary>
            /// <param name="XML"></param>
            public void ReadSchema(string XML)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(XML);
                    if (doc["PropertyList"] != null)
                    {
                        XmlNode PropertyList = doc["PropertyList"];
                        if (PropertyList["generic"] != null)
                        {
                            XmlNode generic = PropertyList["generic"];
                            //get output setting
                            if (generic["output"] != null)
                            {
                                XmlNode output = generic["output"];
                                //get binary mode
                                if (output["binary_mode"] != null)
                                {
                                    Output.BinaryMode = output["binary_mode"].InnerText.ToLower() == "true";
                                }
                            }
                            //get input setting
                            if (generic["input"] != null)
                            {
                                XmlNode input = generic["input"];
                            }
                        }
                    }
                }
                catch (Exception)
                { }
            }

            /// <summary>
            /// Create an xml from setting and flight controller
            /// </summary>
            /// <param name="controller"></param>
            /// <returns></returns>
            public string WriteSchema(bool isInput, bool isOutput)
            {
                XmlDocument doc = new XmlDocument();
                XmlNode elem = doc.AppendChild(doc.CreateElement("PropertyList"));
                XmlNode generic = elem.AppendChild(doc.CreateElement("generic"));
                if (isOutput)
                {
                    XmlNode output = generic.AppendChild(doc.CreateElement("output"));
                    //handle output
                    XmlNode oLineSplit = output.AppendChild(doc.CreateElement("line_separator"));
                    oLineSplit.InnerText = SympolMap.ContainsKey(this.Output.LineSeparator) ?
                        SympolMap[this.Output.LineSeparator] : this.Output.LineSeparator.ToString();
                    XmlNode oVarSplit = output.AppendChild(doc.CreateElement("var_separator"));
                    oVarSplit.InnerText = SympolMap.ContainsKey(this.Output.VarSeparator) ?
                        SympolMap[this.Output.VarSeparator] : this.Output.VarSeparator.ToString();
                    XmlNode binMode = output.AppendChild(doc.CreateElement("binary_mode"));
                    binMode.InnerText = this.Output.BinaryMode.ToString().ToLower();
                    XmlNode preamble = output.AppendChild(doc.CreateElement("preamble"));
                    preamble.InnerText = this.Output.Preamble;
                    XmlNode postamble = output.AppendChild(doc.CreateElement("postamble"));
                    postamble.InnerText = this.Output.Postamble;
                    foreach (var c in this.Output.Chucks)
                    {
                        XmlNode chunk = output.AppendChild(doc.CreateElement("chunk"));
                        XmlNode node = chunk.AppendChild(doc.CreateElement("node"));
                        node.InnerText = c.Node;
                        XmlNode name = chunk.AppendChild(doc.CreateElement("name"));
                        name.InnerText = c.Name;
                        XmlNode type = chunk.AppendChild(doc.CreateElement("type"));
                        type.InnerText = c.Type;
                        XmlNode format = chunk.AppendChild(doc.CreateElement("format"));
                        format.InnerText = c.Format;
                    }
                }
                if (isInput)
                {
                    XmlNode input = generic.AppendChild(doc.CreateElement("input"));
                    //handle input
                    XmlNode iLineSplit = input.AppendChild(doc.CreateElement("line_separator"));
                    iLineSplit.InnerText = SympolMap.ContainsKey(this.Input.LineSeparator) ?
                        SympolMap[this.Input.LineSeparator] : this.Input.LineSeparator.ToString();
                    XmlNode iVarSplit = input.AppendChild(doc.CreateElement("var_separator"));
                    iVarSplit.InnerText = SympolMap.ContainsKey(this.Input.VarSeparator) ?
                        SympolMap[this.Input.VarSeparator] : this.Input.VarSeparator.ToString();
                    foreach (var c in this.Input.Chucks)
                    {
                        XmlNode chunk = input.AppendChild(doc.CreateElement("chunk"));
                        XmlNode node = chunk.AppendChild(doc.CreateElement("node"));
                        node.InnerText = c.Node;
                        XmlNode name = chunk.AppendChild(doc.CreateElement("name"));
                        name.InnerText = c.Name;
                        XmlNode type = chunk.AppendChild(doc.CreateElement("type"));
                        type.InnerText = c.Type;
                        XmlNode format = chunk.AppendChild(doc.CreateElement("format"));
                        format.InnerText = c.Format;
                    }
                }
                return TabXML(doc);
            }

            /// <summary>
            /// Make sure XML has tab
            /// </summary>
            /// <param name="doc"></param>
            /// <returns></returns>
            static string TabXML(XmlDocument doc)
            {
                StringBuilder sb = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "  ";
                settings.NewLineChars = "\r\n";
                settings.NewLineHandling = NewLineHandling.Replace;
                using (XmlWriter writer = XmlWriter.Create(sb, settings))
                {
                    doc.Save(writer);
                }
                return sb.ToString().Replace("encoding=\"utf-16\"", "");
            }
        }

        #endregion Settings
    }
}
