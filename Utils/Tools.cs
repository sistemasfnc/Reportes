using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using EventLog;
using System.Net.Mail;
using Entity;
using System.Data;
using System.ComponentModel;
using System.Globalization;

namespace Utils
{
    public static class Tools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string SHA256Crypt(string text)
        {            
            UTF8Encoding encoder = new UTF8Encoding();
            SHA256Managed sha256hasher = new SHA256Managed();
            byte[] hashedDataBytes = sha256hasher.ComputeHash(encoder.GetBytes(text));
            return byteArrayToString(hashedDataBytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns></returns>
        private static string byteArrayToString(byte[] inputArray)
        {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < inputArray.Length; i++)
            {
                output.Append(inputArray[i].ToString("X2"));
            }
            return output.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public static string SendHttpPostRequest(string url, string parameters)
        {
            try
            {
                var request = CreateHttpPostRequest(url, parameters);
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            return reader.ReadToEnd().Trim();
                        }
                    }
                    else
                    {
                        return null; // or throw an exception
                    }
                }
            }
            catch (Exception ex)
            {
                // handle the exception and return an error message
                return "Error: " + ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static HttpWebRequest CreateHttpPostRequest(string url, string parameters)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = parameters.Length;
            using (var stream = request.GetRequestStream())
            {
                var data = Encoding.UTF8.GetBytes(parameters);
                stream.Write(data, 0, data.Length);
            }
            return request;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sDate"></param>
        /// <param name="sTime"></param>
        /// <returns></returns>
        public static DateTime FormatDate(string sDate, string sTime)
        {
            int iHour = Convert.ToInt32(sTime.Substring(0, 2));
            int iMinute = Convert.ToInt32(sTime.Substring(2, 2));
            try
            {
                return Convert.ToDateTime(sDate).AddHours(iHour).AddMinutes(iMinute - 10);
            }
            catch (Exception ex)
            {
                LogError.WriteError(Config.Configuration.GetStringValue("ErrorLog"), "DAC", ex);
                throw new ApplicationException("Error al formatear la fecha");
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string FormatFoxDate(DateTime date)
        {
            StringBuilder sDate = new StringBuilder("{^");
            sDate.Append(date.ToString("yyyy-MM-dd"));
            sDate.Append("}");
            return sDate.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sNumber"></param>
        /// <returns></returns>
        public static string FormatPhone(string sNumber)
        {
            StringBuilder sPhone = new StringBuilder("57");
            return sPhone.Append(sNumber).ToString();
        }        

        public static string GetStatus(int iStatus)
        {
            switch (iStatus)
	        {
                case 1: return "Sin diligenciar y sin enviar a central de cuentas";
                case 2: return "En caja con pendientes sin enviar a central de cuentas";
                case 3: return "En caja sin recibir en central de cuentas";
                case 4: return "En central de cuentas recibido y auditado";
                case 5: return "Listo para facturar sin pendientes";
                case 6: return "Devuelto a caja sin recibir";
                case 7: return "Recibido Devuelto de central de cuentas";
                case 8: return "Con pendientes para facturar";
                case 9: return "Listo para facturar con pendientes";
                default: return "Sin diligenciar";            
	        }
        }

        public static string GetStatus(int iStatus, bool flag)
        {
            switch (iStatus)
            {
                case 1: return (flag) ? "En caja sin diligenciar" : "Sin diligenciar y sin enviar a central de cuentas";
                case 2: return (flag) ? "En caja con pendientes sin enviar a central de cuentas" : "En caja con pendientes sin enviar a central de cuentas";
                case 3: return (flag) ? "En caja sin recibir en central de cuentas" : "En caja sin recibir en central de cuentas";
                case 4: return (flag) ? "En central de cuentas recibido y auditado" : "En central de cuentas recibido y auditado";
                case 5: return (flag) ? "Listo para facturar sin pendientes" : "Listo para facturar sin pendientes";
                case 6: return (flag) ? "Devuelto a caja sin recibir" : "Devuelto a caja sin recibir";
                case 7: return (flag) ? "Recibido Devuelto de central de cuentas" : "Recibido Devuelto de central de cuentas";
                case 8: return (flag) ? "Con pendientes para facturar" : "Con pendientes para facturar";
                case 9: return (flag) ? "Listo para facturar con pendientes" : "Listo para facturar con pendientes";
                case 0: return "En caja sin diligenciar y sin enviar";
                default: return "En caja sin diligenciar y sin enviar";
            }
        }

        public static bool HaveAccess(List<Security> lSecurity, int access)
        {
            Security oSecurity = lSecurity.Find(x => x.idaccess == access);
            return (oSecurity != null);
        }

        public static string GetChargeStatus(int status)
        {
            return (status == 1) ? "Anulado" : "Activo";
        }

        public static string GetInability(string sCode)
        {
            switch (sCode)
            {
                case "1110": return "INCAPACIDAD ARL ENF. PROFESION";
                case "1111": return "INCAP. ASUMIDA POR LA EMPRESA";
                case "1112": return "INCAP.POR ENFERMEDAD GRAL";
                case "1113": return "MATERNIDAD Y PATERNIDAD";
                case "1195": return "INCAPACIDAD ARP APRENDIZ";
                default: return "INCAPACIDAD SALUD APRENDIZ";                    
            }
        }

        public static string GetDocumentType(string DocumentType)
        {
            switch (DocumentType.ToUpper())
            {
                case "CC": return "1";
                case "NIT": return "2";
                case "TI": return "3";
                case "CE": return "4";
                case "RC": return "7";
                case "NUIP": return "8";
                default: return "9";
            }
        }

        public static string GetFileName(string sFile)
        {
            FileInfo oFile = new FileInfo(sFile);
            return oFile.Name;
        }

        public static bool IsDate(string sDate)
        {
            DateTime dt = new DateTime();
            return (string.IsNullOrEmpty(sDate)) ? false : DateTime.TryParse(sDate, out dt);
        }

        public static string GetCosCenter(string sCost)
        {
            return (sCost.ToUpper().Contains("AIREPOC")) ? "COAD" : (sCost.ToUpper().Contains("ASMAIRE ADULTO") ? "COAD" : "COPE");
        }

        public static string GetColumnPart(string sValue, int iPart)
        {
            string[] result = sValue.Split(' ');
            if (result.Length > 1)
            {
                return (iPart == 0) ? result[iPart] : result[iPart] + " " + result[iPart + 1];
            }
            return string.Empty;
        }

        public static string GetColum(string sValue, int iColumn)
        {
            string[] result = sValue.Split(' ');
            if (result[iColumn] != null) return result[iColumn];
            return string.Empty;            
        }

        /// <summary>
        /// Valida si un número es entero
        /// </summary>
        /// <param name="snumber">Número en texto</param>
        /// <returns>Boleano</returns>
        public static bool IsNumeric(string input)
        {
            long result;
            return !string.IsNullOrWhiteSpace(input) && long.TryParse(input, out result);
        }

        /// <summary>
        /// Método que convierte una lista genérica en DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iList">Lista genérica</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable<T>(this List<T> iList)
        {
            DataTable dataTable = new DataTable();
            PropertyDescriptorCollection propertyDescriptorCollection = TypeDescriptor.GetProperties(typeof(T));
            for (int i = 0; i < propertyDescriptorCollection.Count; i++)
            {
                PropertyDescriptor propertyDescriptor = propertyDescriptorCollection[i];
                Type type = propertyDescriptor.PropertyType;
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    type = Nullable.GetUnderlyingType(type);
                dataTable.Columns.Add(propertyDescriptor.Name, type);
            }
            object[] values = new object[propertyDescriptorCollection.Count];
            foreach (T iListItem in iList)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = propertyDescriptorCollection[i].GetValue(iListItem);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }        

        public static int CastPercent(string sPercent)
        {
            if (string.IsNullOrEmpty(sPercent))
            {
                return 0;
            }
            char sSeparator = (sPercent.Contains(".")) ? '.' : ',';
            sPercent = sPercent.Substring(0, sPercent.IndexOf(sSeparator));
            return Convert.ToInt32(sPercent);
        }

        public static Dictionary<int, int> GetYears()
        {
            Dictionary<int, int> dcYears = new Dictionary<int, int>();
            for (int i = 2017; i <= DateTime.Now.Year; i++)
            {
                dcYears.Add(i, i);
            }
            return dcYears;
        }

        public static string GetMonthName(int iMonth)
        {
            System.Globalization.DateTimeFormatInfo dtFormat = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dtFormat.GetMonthName(iMonth));       
        }

        public static Dictionary<int, string> GetMonths()
        {
            Dictionary<int, string> dcMonths = new Dictionary<int, string>();
            for (int i = 1; i < 13; i++)
            {
                dcMonths.Add(i, Tools.ProperCase(Tools.GetMonthName(i)));
            }
            return dcMonths;
        }

        public static string ProperCase(string stext)
        {
            TextInfo myTI = new CultureInfo("es-CO", false).TextInfo;
            return myTI.ToTitleCase(stext.ToLower());
        }

        public static bool IsDocumentType(string sDocument)
        {
            return sDocument.EqualsOfAny("RC", "CC", "TI", "CE", "PA", "PE", "PT", "NU", "MS");
        }

        public static bool EqualsOfAny(this string value, params string[] targets)
        {
            return targets.Any(target => value.Equals(target, StringComparison.Ordinal));
        }

        public static T ConvertToDerived<T>(object baseObj) where T : new()
        {
            var derivedObj = new T();
            var props = baseObj.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.CanWrite)
                {
                    var val = prop.GetValue(baseObj);
                    if (val != null)
                        prop.SetValue(derivedObj, val);
                }
            }
            return derivedObj;
        }

        public static bool ValidateDates(DateTime dateTime1, DateTime dateTime2)
        {
            int age = Age(dateTime1, dateTime2);

            // Verificar que la fecha de nacimiento no sea mayor a la fecha actual (dateTime2) 
            // y que la edad no sea mayor a 105 años
            return (dateTime1 <= dateTime2 && age <= 105);
        }

        public static int Age(DateTime birthDate, DateTime laterDate)
        {
            int age = laterDate.Year - birthDate.Year;

            // Si no ha llegado al cumpleaños este año, restar 1 año de la edad
            if (laterDate.Date < birthDate.Date.AddYears(age))
            {
                age--;
            }

            return age;
        }

        public static string ReplaceChars(string sText)
        {
            if (!string.IsNullOrEmpty(sText))
            {
                char[] chars = new char[] { ';', ',', '#', '?', '*', '\n', '\r', '@' };
                string sresult = chars.Aggregate(sText, (c1, c2) => c1.Replace(c2, ' '));
                return sresult.Trim();
            }
            return string.Empty;
        }
    }
}
