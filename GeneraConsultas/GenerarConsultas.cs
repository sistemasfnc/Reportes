using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using Facade;
using Config;
using DAC;

namespace GeneraConsultas
{
    class GenerarConsultas
    {
        static void Main(string[] args)
        {
            using (ConsultasDAC oDAC = new ConsultasDAC(Configuration.GetStringValue("FoxConnection")))
            {
                oDAC.Generate();
                /*string[,] slines = oDAC.Generate();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < slines.GetLength(0); i++)
                {
                    StringBuilder lineBuilder = new StringBuilder();
                    for (int j = 0; j < slines.GetLength(1); j++)
                    {
                        if (lineBuilder.Length > 0)
                            lineBuilder.Append(",");
                        lineBuilder.Append(slines[i, j]);
                    }
                    sb.AppendLine(lineBuilder.ToString());
                }
                File.AppendAllText(@"c:\Temp\consultas.csv", sHeader, Encoding.UTF8);
                File.AppendAllText(@"c:\Temp\consultas.csv", sb.ToString(), Encoding.UTF8);*/
            }
        }
    }
}
