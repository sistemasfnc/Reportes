using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentFTP;
using System.Net;
using FluentFTP.Helpers;
using System.IO;

namespace Trazabilidad.clases
{
    public class FTPManage : IDisposable
    {
        private string host { get; set; }

        private string user { get; set; }

        private string password { get; set; }

        private string remotedir { get; set; }

        private int port { get; set; }

        public FTPManage(string shost, string suser, string spassword, string sremotefolder, int iport) 
        { 
            this.host = shost;
            this.user = suser;
            this.password = spassword;
            this.remotedir = sremotefolder;
            this.port = iport;
        }

        public void UploadFile(string file)
        {
            try
            {
                // Crear una nueva instancia de FtpClient
                using (FtpClient client = new FtpClient(host))
                {
                    client.Credentials = new NetworkCredential(this.user, this.password);

                    // Configurar FTPS con TLS implícito en el puerto 990
                    client.Config.EncryptionMode = FtpEncryptionMode.Implicit;
                    client.Config.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    client.ValidateCertificate += Client_ValidateCertificate;

                    // Conectar al servidor
                    client.Connect();

                    // Subir el archivo
                    client.UploadFile(file, new FileInfo(file).Name);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Client_ValidateCertificate(FluentFTP.Client.BaseClient.BaseFtpClient control, FtpSslValidationEventArgs e)
        {
            e.Accept = true;
        }
        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}