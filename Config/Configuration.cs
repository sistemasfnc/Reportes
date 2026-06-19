using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Xml.Linq;


namespace Config
{
    public class Configuration
    {
        static string sXml = @"C:\www\Pedro_Romero\Proyectos_NET\BusDatos\Reportes\Config\bin\Debug\Config.dll.config";

        //static string sXml = @"E:\\oldconfig\\Config.dll.config";

        public static string GetStringValue(string Key)
        {
            return GetConfigValue(Key);
        }

        public static bool GetBoolValue(string Key)
        {
            return Convert.ToBoolean(GetConfigValue(Key));
        }

        public static int GetIntegerValue(string Key)
        {
            return Convert.ToInt32(GetConfigValue(Key));
        }

        private static string GetConfigValue(string Key)
        {            
            XElement xElement = XElement.Load(sXml);
            var x = from a in xElement.Element("applicationSettings").Elements("Config.Properties.Settings").Elements("setting") where a.Attribute("name").Value == Key select a.Value;
            return x.First().ToString();            
        }

        public static void UpdateKeyValue(string Key, string Value)
        {            
            XElement xElement = XElement.Load(sXml);
            xElement.Element("applicationSettings").Elements("Config.Properties.Settings").Elements("setting").First(x => x.Attribute("name").Value == Key).Value = Value;
            xElement.Save(sXml);
        }
    }
}
