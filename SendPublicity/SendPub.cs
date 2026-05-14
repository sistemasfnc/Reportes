using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Facade;
using Entity;
using System.IO;
using Config;
using EventLog;
using Utils;
using System.ComponentModel.DataAnnotations;

namespace SendPublicity
{
    class SendPub
    {
        static List<Generic> lGeneric { get; set; }
        
        static void Main(string[] args)
        {
            GenerateSending();
        }

        static void GenerateSending()
        {
            lGeneric = new List<Generic>();
            try
            {
                lGeneric.AddRange(GetDBData());
                lGeneric.AddRange(GetFileData());
                SendMail();
            }
            catch (Exception ex)
            {
                LogError.WriteError("Publicidad", "Aplicacion", ex);
            }
        }

        static List<Generic> GetDBData()
        {
            using (FacadePublicidad oFacade = new FacadePublicidad())
            {
                return oFacade.GetList();
            }            
        }

        static void SendMail()
        {
            SendMail oMail = new SendMail();
            for (int i = 3000; i < lGeneric.Count; i++)
            {
                try
                {
                    if (!string.IsNullOrEmpty(lGeneric[i].code.Trim()) && new EmailAddressAttribute().IsValid(lGeneric[i].code.Trim()))
                    {
                        oMail.Send(lGeneric[i].code.Trim(), Configuration.GetStringValue("MailUser"), Configuration.GetStringValue("MailPassword"), string.Empty, GetMessage(lGeneric[i].name.Trim()));
                    }                    
                }
                catch (Exception ex)
                {
                    LogError.WriteError("Publicidad", "Aplicacion", ex);                    
                }                
            }
            oMail = null;
        }

        static string GetMessage(string sName)
        {
            StringBuilder sMessage = new StringBuilder("<table  background='http://www.neumologica.org/image002.png' width='688' height='688'>");
            sMessage.Append("<tr>");
            sMessage.Append("<td width='45%'>&nbsp;</td>");
            sMessage.Append("<td style='font-weight:bold;font-family:Verdana;color:blue;'>");
            sMessage.Append(sName);
            sMessage.Append("</td>");
            return sMessage.ToString();
        }

        static List<Generic> GetFileData()
        {
            List<Generic> lGeneric = new List<Generic>();
            if (File.Exists(Configuration.GetStringValue("DataFile")))
            {
                string[] sLines = File.ReadAllLines(Configuration.GetStringValue("DataFile"));
                string[] sData = null;
                for (int i = 0; i < sLines.Length; i++)
                {
                    sData = sLines[i].Split(',');
                    if (sData.Length > 1)
                    {
                        lGeneric.Add(new Generic() { code = sData[0], name = sData[1] });
                    }
                }
            }
            return lGeneric;
        }
    }
}
