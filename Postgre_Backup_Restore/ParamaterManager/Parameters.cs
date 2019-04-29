using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Postgre_Backup_Restore.ParamaterManager
{
    public class Parameters
    {
        private static Parameters _parameters = null;
        public static Parameters Params
        {
            get
            {
                if (_parameters == null)
                {
                    GetPrams();
                }
                return _parameters;
            }
        }
        public string PgDump
        {
            get;
            set;
        }
        public string Base
        {
            get;
            set;
        }
        public string Host
        {
            get;
            set;
        }
        public string Port
        {
            get;
            set;
        }
        public string User
        {
            get;
            set;
        }
        public string Password
        {
            get;
            set;
        }
        private static Parameters GetPrams()
        {
            if (_parameters == null)
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(Parameters));
                TextReader reader = null;
                try
                {
                    reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "/Parameters.xml");//@"TesaParam.xml");
                    object obj = deserializer.Deserialize(reader);
                    _parameters = (Parameters)obj;
                    reader.Close();
                }
                catch (Exception e)
                {
                    Trace.WriteLine("StreamReaderErr : (" + e.Message + ")");
                    _parameters = new Parameters();
                    SaveToFile();
                    Trace.WriteLine("Parameters.xml Created");
                }
            }
            return _parameters;
        }

        private Parameters()
        {

        }

        public static void SavePrams()
        {
            GetPrams();
            SaveToFile();
        }

        private static void SaveToFile()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Parameters));
            using (TextWriter writer = new StreamWriter(@"Parameters.xml"))
            {
                serializer.Serialize(writer, _parameters);
            }
        }
    }
}
