using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using System.IO;
using Config;

namespace Utils
{
    public class SendMail
    {        
        private string MailBoddy
        {
            get
            {
                StringBuilder Mensaje = new StringBuilder("<p style='font-weight:bold; font-family:verdana'>Reciba un cordial saludo, </p>");                
                Mensaje.Append("<p style='font-family:verdana'>Adjunto encontrar&aacute; el archivo con los procedimientos solicitados por la FNC</p>");
                Mensaje.Append("<p style='font-family:verdana'>Cualquier inquietud con gusto ser&aacute; atendida</p>");
                Mensaje.Append("<p style='font-family:verdana'>Cordialmente</p><br />");                
                Mensaje.Append("<span style='font-family:verdana'>Ivonne Perlaza</span><br />");
                Mensaje.Append("<span style='font-family:verdana'>Consulta<span><br />");
                Mensaje.Append("<span style='font-family:verdana'>Fundaci&oacute;n Neumol&oacute;gica Colombiana</span><br />");
                return Mensaje.ToString();
            }            
        }

        public void Send(string recipient, string user, string password, string file, string message = "")
        {
            // Validaciones de entrada
            if (string.IsNullOrEmpty(recipient))
            {
                throw new ArgumentException("La dirección de correo electrónico del destinatario no puede estar vacía.");
            }
            if (string.IsNullOrEmpty(user))
            {
                throw new ArgumentException("La dirección de correo electrónico del remitente no puede estar vacía.");
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("La contraseña del remitente no puede estar vacía.");
            }
            if (!string.IsNullOrEmpty(file) && !File.Exists(file))
            {
                throw new ArgumentException("El archivo adjunto no existe.");
            }

            string messageBody = string.IsNullOrEmpty(message) ? this.MailBoddy : message;
            string subject = string.IsNullOrEmpty(message) ? "Autorizacion de Procedimientos" : "Lanzamiento nueva pagina web FNC";

            using (var smtpClient = new SmtpClient(Configuration.GetStringValue("MailServer"), Configuration.GetIntegerValue("MailPort")))
            using (var mailMessage = new MailMessage(user, recipient, subject, messageBody))
            {
                mailMessage.IsBodyHtml = true;
                mailMessage.Bcc.Add(user);
                if (!string.IsNullOrEmpty(file))
                {
                    mailMessage.Attachments.Add(GetAttachment(file));
                }

                try
                {
                    smtpClient.Credentials = new NetworkCredential(user, password);
                    smtpClient.Send(mailMessage);
                }
                catch (Exception ex)
                {
                    // Agregar manejo de excepciones aquí
                    throw new Exception("Error al enviar correo electrónico.", ex);
                }
            }
        }

        private Attachment GetAttachment(string File)
        {
            byte[] contentAsBytes = System.IO.File.ReadAllBytes(File);
            MemoryStream memStream = new MemoryStream(contentAsBytes);
            StreamWriter streamWriter = new StreamWriter(memStream);
            streamWriter.Flush();
            memStream.Position = 0;
            return new Attachment(memStream, "plantillasanitas.xls")
            {
                ContentType = new System.Net.Mime.ContentType("application/vnd.ms-excel"),
                Name = "plantillasanitas.xls",
                NameEncoding = Encoding.UTF8,
            };
        }        
    }
}
