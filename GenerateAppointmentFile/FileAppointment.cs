using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAC;
using System.Data;
using System.Configuration;
using System.IO;

namespace GenerateAppointmentFile
{
    class FileAppointment
    {
        static DataTable dt = null;

        static void Main(string[] args)
        {
            string[] asDays = Properties.Settings.Default.Festivos.Split(',');
            DateTime dtInitial = DateTime.Now.AddDays(1);
            DateTime dtFinal = DateTime.Now;
            int iDay = (int)dtFinal.DayOfWeek;            
            if (iDay == 5)
            {
                if (!string.IsNullOrEmpty(asDays.FirstOrDefault(x => x == dtFinal.AddDays(3).DayOfYear.ToString())))
                {
                    dtFinal = dtFinal.AddDays(4);
                }
                else
                {
                    dtFinal = dtFinal.AddDays(3);
                }
            }
            else
            {
                dtFinal = dtFinal.AddDays(1);
            }
            try
            {
                GetData(dtInitial, dtFinal);
                if (dt.Rows.Count > 0)
                {
                    SaveFile();
                }                
            }
            catch (Exception)
            {
                throw;
            }            
        }

        static void GetData(DateTime dtInitial, DateTime dtFinal)
        {
            using (CitasDAC oDAC = new CitasDAC())
            {
                oDAC.sqlConnection = Properties.Settings.Default.FNCStats;
                dt = oDAC.GetData(dtInitial, dtFinal);
            }
        }

        static void SaveFile()
        {
            string sFile = Path.Combine(Properties.Settings.Default.Ruta, Properties.Settings.Default.Archivo);
            StringBuilder sb = new StringBuilder();            
            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }
            File.WriteAllText(sFile, sb.ToString());
        }
    }
}
