using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Entity;
using System.IO;
using Utils;

namespace WServices
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "Service1" en el código, en svc y en el archivo de configuración.
    // NOTE: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione Service1.svc o Service1.svc.cs en el Explorador de soluciones e inicie la depuración.
    public class Service1 : IService1
    {        
        public string SendSMS(Sms oEntity)
        {
            string sMessage = this.GetSMSText(oEntity);
            StringBuilder sParameters = new StringBuilder("user=");
            sParameters.Append(Config.Configuration.GetStringValue("SMSUser"));
            sParameters.Append("&password=");
            sParameters.Append(Config.Configuration.GetStringValue("SMSPassword"));
            sParameters.Append("&sender=Neumologica&SMSText=");
            sParameters.Append(sMessage);
            sParameters.Append("&GSM=");
            sParameters.Append(oEntity.cellphone);
            return Tools.SetHttpPostVar(Config.Configuration.GetStringValue("SMSUrl"), sParameters.ToString());
            //return string.Empty;
        }
      
        private string GetSMSText(Sms oEntity)
        {
            string sText = string.Empty;
            string[] args = new string[] { oEntity.name, oEntity.schedule.ToString("dd/MM/yyyy"), oEntity.schedule.ToString("hh:mm") };
            if (File.Exists(Config.Configuration.GetStringValue("SMSTemplate")))
            {
                sText = File.ReadAllText(Config.Configuration.GetStringValue("SMSTemplate"));
                sText = string.Format(sText, args);
            }
            return sText;
        }
    }
}
