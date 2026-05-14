using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using Config;
using Facade;
using EventLog;
using System.IO;
using System.Xml.Linq;
using Utils;

namespace SendMessage
{
    class SendSMS
    {
        static string sMessage = string.Empty;
        
        static void Main(string[] args)
        {
            /*if (DateTime.Now.Hour == 8)
            {
                SendTextMessage();
            }
            else*/
            {
                SendSanitasMail();
            }            
        }        

        static void SendTextMessage()
        {
            List<Sms> lSMS = new List<Sms>();
            FacadeSMS oFacade = new FacadeSMS();
            string sResult = string.Empty;
            try
            {
                sMessage = GetSMSText();
                lSMS = oFacade.GetSchedule();
                for (int i = 0; i < lSMS.Count; i++)                
                {                    
                    sResult = SendMessage(lSMS[i]);
                    lSMS[i].message = String.Format(sMessage, new string[] { lSMS[i].name, lSMS[i].schedule.ToString("dd/MM/yyyy"), lSMS[i].schedule.ToString("hh:mm") });
                    lSMS[i].result = ParseResult(sResult);
                    lSMS[i].senddate = DateTime.Now;
                    oFacade.InsertLog(lSMS[i]);
                }
            }
            catch (Exception ex)
            {
                LogError.WriteError("SendSMS", "Aplicacion", ex);
            }
            finally
            {
                oFacade.Dispose();
                oFacade = null;
                lSMS = null;
            }
        }

        static void SendSanitasMail()
        {
            try
            {                
                GenerateExcel();                
                //SendMail();
            }
            catch (Exception ex)
            {
                LogError.WriteError("SendSMS", "Sanitas", ex);
            }
        }

        static void SendMail()
        {
            SendMail oMail = new SendMail();
            oMail.Send(Config.Configuration.GetStringValue("SanitasRecipient"), Config.Configuration.GetStringValue("MailUser"), Config.Configuration.GetStringValue("MailPassword"), Config.Configuration.GetStringValue("ExcelFile"));
        }

        static void ValidateFile()
        {
            if (File.Exists(Config.Configuration.GetStringValue("ExcelFile")))
            {
                try
                {
                    File.Delete(Config.Configuration.GetStringValue("ExcelFile"));                    
                }
                catch (IOException ex)
                {
                    LogError.WriteError("SendSMS", "Aplicacion", ex);
                    return;
                }                
            }
            File.Copy(Config.Configuration.GetStringValue("ExcelFile1"), Config.Configuration.GetStringValue("ExcelFile"));                    
        }

        static void GenerateExcel()
        {            
            FacadeSanitas oFacade = new FacadeSanitas();            
            try
            {
                ValidateFile();
                oFacade.GenerateExcelFile(oFacade.GetProcedures());
            }
            catch (Exception ex)
            {
                LogError.WriteError("SendSMS", "Aplicacion", ex);
                throw;
            }            
            finally
            {
                oFacade.Dispose();
                oFacade = null;
            }
        }

        static string SendMessage(Sms oSMS)
        {
            string sResult = string.Empty;
            WService.Service1Client oService = new WService.Service1Client();
            try
            {
                sResult = oService.SendSMS(oSMS);
            }
            catch (Exception ex)
            {
                LogError.WriteError("SendSMS", "Aplicacion", ex);
                sResult = "-4";
            }
            finally
            {
                oService.Close();
                oService = null;
            }
            return sResult;
        }

        static string ParseResult(string sResult)
        {
            string sCode = (sResult != "-4") ? GetResultCode(sResult) : sResult;
            XElement oXML = XElement.Load(Config.Configuration.GetStringValue("XMLError"));
            var x = from a in oXML.Descendants("error") where a.Attribute("code").Value == sCode select a.Value;
            return x.First().ToString();                        
        }

        private static string GetResultCode(string sResult)
        {
            XElement oXML = XElement.Parse(sResult);
            var x = from a in oXML.Descendants("result") select new { Valor = a.Element("status").Value };
            return x.FirstOrDefault().Valor;
        }

        static string GetSMSText()
        {                        
            if (File.Exists(Config.Configuration.GetStringValue("SMSTemplate")))
            {
                return File.ReadAllText(Config.Configuration.GetStringValue("SMSTemplate"));                
            }
            return string.Empty;
        }

    }
}
