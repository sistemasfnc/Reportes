using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventLog;
using Config;
using OfficeOpenXml;

namespace SendCompensarReport
{
    class SendExcelFile
    {
        static void Main(string[] args)
        {
        }


        static void GenerateExcel()
        {

        }

        static void UseWinSCP(string sFile)
        {
            Process winscp = new Process();
            winscp.StartInfo.FileName = @"c:\Winscp\winscp.com";
            winscp.StartInfo.UseShellExecute = false;
            winscp.StartInfo.RedirectStandardInput = true;
            winscp.StartInfo.RedirectStandardOutput = true;
            winscp.StartInfo.CreateNoWindow = true;
            winscp.Start();
            winscp.StandardInput.WriteLine("open ftps//" + Configuration.GetStringValue("FtpUser") + ":" + Configuration.GetStringValue("FtpPassword") + "@" + Configuration.GetStringValue("FtpHost"));
            winscp.StandardInput.WriteLine("cd /neumologica");
            winscp.StandardInput.WriteLine("put " + sFile);
            winscp.StandardInput.Close();
            winscp.WaitForExit();
            if (winscp.ExitCode != 0)
            {
                EventLog.LogError.WriteError("FTPCompensar", "FTPCompensar", new ApplicationException(winscp.ExitCode.ToString()));
            }
            winscp.Dispose();
            winscp = null;
        }
    }
}
