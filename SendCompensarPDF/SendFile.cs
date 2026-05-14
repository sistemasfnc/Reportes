using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Facade;
using Entity;
using System.IO;
using Config;
using System.Collections;
using Tamir.SharpSsh;
using Tamir.SharpSsh.jsch;
using System.Data;
using System.Net;
using System.Threading;
using AlexPilotti.FTPS.Client;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Diagnostics;

namespace SendCompensarPDF
{
    class SendFile
    {
        private static List<PFP> lPFP { get; set; }

        private static int fSecuence { get; set; }
        
        
        static void Main(string[] args)
        {
            FacadeCompensar oFacade = new FacadeCompensar();
            DataTable dt = new DataTable();
            try
            {
                oFacade.sConnection = Configuration.GetStringValue("CompensarConnection");
                if (Configuration.GetBoolValue("IsCompensar"))
                {
                    lPFP = oFacade.GetCompensarList();
                    ProcessFiles();
                }
                else
                {
                    dt = oFacade.GetSanitasList();
                    ProcessFiles(dt);
                }                
            }
            catch (Exception ex)
            {
                EventLog.LogError.WriteError("PFPCompensar", "PFPCompensar", ex);
            }
            finally
            {
                oFacade.Dispose();
                oFacade = null;
                lPFP = null;
                dt.Dispose();
                dt = null;
            }
        }

        static string GetFileName()
        {
            fSecuence = Configuration.GetIntegerValue("FileSecuence");
            StringBuilder sfName = new StringBuilder("RIC_COMPENSAR_FNC_");            
            sfName.Append(DateTime.Now.ToString("yyyyMMdd"));
            sfName.Append("_");
            sfName.Append(fSecuence.ToString());
            sfName.Append(".txt");
            return sfName.ToString();
        }

        static void ProcessFiles(DataTable dt)
        {
            string sPath = string.Empty;
            string sTarget = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                sPath = GetPath(dr["archivo"].ToString());
                sTarget = GetTarget(dr["archivo"].ToString());
                if (!string.IsNullOrEmpty(sPath))
                {
                    if (!File.Exists(sTarget))
                        File.Copy(sPath, sTarget);
                }                
                //UseWinSCP(sPath);
            }
        }

        static void ProcessFiles()
        {
            if (lPFP.Count > 0)
            {
                string sFile = Path.Combine(Configuration.GetStringValue("TextPath"), GetFileName());
                DataTable dt = new DataTable();
                dt.Columns.Add("id");
                dt.Columns.Add("archivo");
                int j = Configuration.GetIntegerValue("DataBaseId");
                //for (int i = 0; i < 20; i++)
                for (int i = 0; i < lPFP.Count; i++)
                {
                    try
                    {
                        lPFP[i].ruta = GetPath(lPFP[i].archivo);
                        if (!string.IsNullOrEmpty(lPFP[i].ruta))
                        {
                            File.AppendAllText(sFile, string.Join(";", GetText(lPFP[i])) + Environment.NewLine);
                            UseWinSCP(lPFP[i].ruta);
                            //SendFTPSFile(lPFP[i].ruta);
                            dt.Rows.Add(new object[] { j, lPFP[i].archivo });
                            j++;
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLog.LogError.WriteError("PFPCompensar", "PFPCompensar", ex);
                    }
                }
                InsertData(dt);
                Configuration.UpdateKeyValue("DataBaseId", j.ToString());
                UseWinSCP(sFile);
                fSecuence++;
                Configuration.UpdateKeyValue("FileSecuence", fSecuence.ToString());
            }                    
        }

        static void InsertData(DataTable dt)
        {
            using (FacadeCompensar oFacade = new FacadeCompensar())
            {
                oFacade.sConnection = Configuration.GetStringValue("CompensarConnection");
                oFacade.BulkData(dt);
            }            
        }

        static string[] GetText(PFP oEntity)
        {
            return new string[] 
            { 
                oEntity.nit, 
                oEntity.autorizacion.Trim().Replace(";", string.Empty), 
                oEntity.tipodocumento, 
                oEntity.documento, 
                oEntity.nuip, 
                oEntity.cups,
                oEntity.fecha.ToString("dd/MM/yyyy"),
                oEntity.estado,
                oEntity.tiposervicio,
                oEntity.nit,
                oEntity.eps,
                oEntity.archivo,
            };
            //return ((IEnumerable)oEntity).Cast<object>().Select(x => x.ToString()).ToArray();
        }
       
        static string GetPath(string File)
        {            
            string[] aFiles = Directory.GetFiles(Configuration.GetStringValue("FilePath"), File);
            return (aFiles.Length > 0) ? aFiles[0] : string.Empty;
        }

        static string GetTarget(string File)
        {
            //string sFile = File.Substring(0, File.LastIndexOf("_")) + ".pdf";
            return Path.Combine(Configuration.GetStringValue("SanitasPath"), File);            
        }

        static void SendFileSFTP(string sFile)
        {
            Sftp oSFTP = new Sftp(Configuration.GetStringValue("SftpHost"), Configuration.GetStringValue("SftpUser"), Configuration.GetStringValue("SftpPassword"));            
            try
            {
                oSFTP.Connect(Configuration.GetIntegerValue("SftpPort"));
                if (Configuration.GetBoolValue("OnProduction")) oSFTP.Put(sFile);
                else oSFTP.Put(sFile, "/tmp/" + Utils.Tools.GetFileName(sFile));
                oSFTP.Close();
            }
            catch (Exception ex)
            {
                EventLog.LogError.WriteError("PFPCompensar", "PFPCompensar", ex);
                throw;
            }
            finally
            {
                oSFTP = null;
            }
        }

        private static bool ValidateTestServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {            
            return true;
        }

        static void SendFTPSFile(string sFile)
        {            
            FTPSClient oFTP = new FTPSClient();
            NetworkCredential oCredentials = new NetworkCredential(Configuration.GetStringValue("SftpUser"), Configuration.GetStringValue("SftpPassword"));
            try
            {                
                oFTP.Connect(Configuration.GetStringValue("SftpHost"), oCredentials, ESSLSupportMode.All, new RemoteCertificateValidationCallback(ValidateTestServerCertificate));
                oFTP.SetTransferMode(AlexPilotti.FTPS.Common.ETransferMode.Binary);
                oFTP.SetCurrentDirectory("neumologica");
                oFTP.PutFile(sFile, Utils.Tools.GetFileName(sFile));
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                oFTP.Close();
                oFTP.Dispose();
                oFTP = null;
            }                      
        }

        static void UseWinSCP(string sFile)
        {
            Process winscp = new Process();
            winscp.StartInfo.FileName = @"C:\Program Files (x86)\WinSCP\winscp.com";
            winscp.StartInfo.UseShellExecute = false;
            winscp.StartInfo.RedirectStandardInput = true;
            winscp.StartInfo.RedirectStandardOutput = true;
            winscp.StartInfo.CreateNoWindow = true;
            winscp.Start();
            winscp.StandardInput.WriteLine("open ftps://" + Configuration.GetStringValue("SftpUser") + ":" + Configuration.GetStringValue("SftpPassword") + "@" + Configuration.GetStringValue("SftpHost"));
            winscp.StandardInput.WriteLine("cd /neumologica");
            winscp.StandardInput.WriteLine("put " + sFile);
            winscp.StandardInput.Close();
            winscp.WaitForExit();
            if (winscp.ExitCode != 0)
            {
                EventLog.LogError.WriteError("PFPCompensar", "PFPCompensar", new ApplicationException(winscp.ExitCode.ToString()));
            }
            winscp.Dispose();
            winscp = null;            
        }        
    }
}
