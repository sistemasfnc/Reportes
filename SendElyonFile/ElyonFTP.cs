using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using EventLog;
using Facade;
using Entity;
using System.Net;
using System.ComponentModel.Design;

namespace SendElyonFile
{
    class ElyonFTP
    {

        static List<Generic> lFiles { get; set; }
        
        static void Main(string[] args)
        {
            try
            {
                GetFiles();
                UploadFiles();
                UpdateFiles();
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
            }
            
        }

        static void GetFiles()
        {
            using (FacadeDesmaterializacion facade = new FacadeDesmaterializacion(Configuration.GetStringValue("FNCFacturacion")))
            {
                lFiles = facade.GetFilesForUpload(Configuration.GetStringValue("RelationshipsFolder"));
            }
        }

        static void UploadFiles()
        {
            for (int i = 0; i < lFiles.Count; i++)
            {
                if (File.Exists(lFiles[i].name))
                {
                    try
                    {
                        UploadFile(lFiles[i].name);
                        lFiles[i].id = 1;
                    }
                    catch (Exception)
                    {
                        lFiles[i].id = 0;
                    }
                }                
            }
        }

        static void UploadFile(string sfile)
        {
            string logname = @"d:\Temp\winscp.xml";
            StringBuilder strCommand = new StringBuilder("open ftps://");
            strCommand.Append(Configuration.GetStringValue("ElyonUser"));
            strCommand.Append(":");
            strCommand.Append(Configuration.GetStringValue("ElyonPassword"));
            strCommand.Append("@");
            strCommand.Append(Configuration.GetStringValue("ElyonFtp"));
            Process winscp = new Process();
            try
            {
                winscp.StartInfo.FileName = Configuration.GetStringValue("WinSCP");
                winscp.StartInfo.Arguments = "/xmllog=\"" + logname + "\"";
                winscp.StartInfo.UseShellExecute = false;
                winscp.StartInfo.RedirectStandardInput = true;
                winscp.StartInfo.RedirectStandardOutput = true;
                winscp.StartInfo.CreateNoWindow = true;
                winscp.Start();
                //winscp.StandardInput.WriteLine("open ftps://800180553:1YtRat3MZ0ty@ftp.elyon.com.co");
                winscp.StandardInput.WriteLine(strCommand.ToString());
                //winscp.StandardInput.WriteLine("cd /neumologica");
                winscp.StandardInput.WriteLine("put " + sfile);
                winscp.StandardInput.Close();
                winscp.WaitForExit();
                if (winscp.ExitCode != 0)
                {
                    LogError.WriteError("Facturacion", "Aplicacion", new ApplicationException("Error de conexión al FTP"));
                }
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                winscp.Dispose();
                winscp = null;
            }            
        }

        static void UpdateFiles()
        {
            using (FacadeDesmaterializacion facade = new FacadeDesmaterializacion(Configuration.GetStringValue("FNCFacturacion")))
            {
                facade.UpdateFileStatus(lFiles);
            }
        }

    }
}
