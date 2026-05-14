using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Facade;
using Entity;
using Utils;
using EventLog;
using Config;
using System.IO;
using System.Text;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Microsoft.Ajax.Utilities;
using System.Diagnostics;
using Ionic.Zip;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace Trazabilidad.clases
{
    public class FacturacionColmenaARL : IDisposable
    {
        public string sRelation { get; set; }

        public void GenerateFiles(List<Desmaterializacion> ldesmaterializacion)
        {
            FacturacionCompensar facturacionCompensar = new FacturacionCompensar();
            List<string> lTmp = new List<string>();
            List<Generic> lsupports = null;
            int i = 0;
            string invoicedirectory = string.Empty;
            string directory = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
            Directory.CreateDirectory(directory);
            foreach (var item in ldesmaterializacion)
            {
                if (lTmp.FirstOrDefault(x => x.Contains(item.sfactura)) == null)
                {
                    invoicedirectory = Path.Combine(directory, item.sfactura);
                    if (!Directory.Exists(invoicedirectory))
                    {
                        Directory.CreateDirectory(invoicedirectory);
                    }
                    this.CopyInvoice(item.sarchivo, item.sfactura);
                }
                lsupports = facturacionCompensar.GetInvoiceSupports(item.sepisodio);
                i = 1;
                foreach (var support in lsupports)
                {
                    this.GeneratePDFSupport("SOPORTE" + i.ToString(), support.name, support.date, item.sfactura);
                    i++;
                }
                lTmp.Add(item.sfactura);
            }
        }

        /// <summary>
        /// Método que copia el archivo de la factura desde la carpeta de facturación hacia la carpeta de la relación y su factura
        /// </summary>
        /// <param name="sinvoicefile">String nombre del archivo de la factura</param>
        /// <param name="sinvoice">String número de factura</param>
        private void CopyInvoice(string sinvoicefile, string sinvoice)
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, sinvoice, "SETT" + sinvoice + ".PDF" };
            string[] invoicepath = new string[] { Configuration.GetStringValue("InvoicesPath"), sinvoicefile + ".pdf" };
            if (File.Exists(Path.Combine(invoicepath)) && !File.Exists(Path.Combine(paths)))
            {
                File.Copy(Path.Combine(invoicepath), Path.Combine(paths));
            }
        }

        /// <summary>
        /// Método que busca de manera recursiva archivos en un directorio
        /// </summary>
        /// <param name="sfile">String patrón de búsqueda</param>
        /// <param name="sdir">String directorio raíz</param>
        /// <param name="lsupports">Lista genérica con las rutas de los archivos encontrados</param>
        /// <returns>Lista genérica con las rutas de los archivos encontrados</returns>
        private List<string> SearchFile(string sfile, string sdir, List<string> lsupports)
        {
            foreach (string f in Directory.GetFiles(sdir, sfile))
            {
                lsupports.Add(f);
            }
            foreach (string d in Directory.GetDirectories(sdir))
            {
                SearchFile(sfile, d, lsupports);
            }
            return lsupports;
        }

        /// <summary>
        /// Método para generar los soportes de la factura
        /// </summary>
        /// <param name="stype"></param>
        /// <param name="ssupport"></param>
        /// <param name="date"></param>
        /// <param name="sinvoice"></param>
        /// <returns></returns>
        /*private int GeneratePDFSupport(string stype, string ssupport, DateTime date, string sinvoice)
        {
            int ipages = 0;
            FileStream fs = null;
            List<string> lsupports = new List<string>();
            Document document = new Document();
            string[] foldersupport = new string[] { Configuration.GetStringValue("SupportsPath"), date.Year.ToString(), date.Month.ToString(), date.Day.ToString() };
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, sinvoice, stype + ".PDF" };
            string pdffile = Path.Combine(paths);
            lsupports = this.SearchFile(ssupport + "*.jpg", Path.Combine(foldersupport), lsupports);
            fs = new FileStream(pdffile, FileMode.Create);          
            PdfWriter.GetInstance(document, fs);
            document.Open();
            foreach (string item in lsupports)
            {
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(item);
                img.SetAbsolutePosition(0, 0); // set the position to bottom left corner of pdf
                document.SetPageSize(PageSize.LETTER);
                img.ScaleToFit(PageSize.LETTER.Width, PageSize.LETTER.Height);
                img.SetDpi(300, 300);
                document.Add(img);
                document.NewPage();
                ipages++;
            }
            document.Close();
            return ipages;
        }*/

        private int GeneratePDFSupport(string stype, string ssupport, DateTime date, string sinvoice)
        {
            int ipages = 0;
            List<string> lsupports = new List<string>();
            FileStream fs = null;
            Document document = new Document(PageSize.LETTER);
            string folderPath = Path.Combine(Configuration.GetStringValue("SupportsPath"), date.Year.ToString(), date.Month.ToString(), date.Day.ToString());
            string pdfFilePath = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, sinvoice, stype + ".PDF");
            lsupports = this.SearchFile(ssupport + "*.jpg", folderPath, lsupports);
            PdfReader reader = null;
            PdfStamper stamper = null;
            bool fileExists = File.Exists(pdfFilePath);

            if (fileExists)
            {
                reader = new PdfReader(pdfFilePath);
                fs = new FileStream(pdfFilePath + "_temp", FileMode.Create);
                stamper = new PdfStamper(reader, fs);
            }
            else
            {
                fs = new FileStream(pdfFilePath, FileMode.Create);
                PdfWriter writer = PdfWriter.GetInstance(document, fs);
                document.Open();
            }
            foreach (string item in lsupports)
            {
                iTextSharp.text.Image img = null;
                try
                {
                    img = iTextSharp.text.Image.GetInstance(item);
                    img.SetAbsolutePosition(0, 0); // set the position to bottom left corner of the pdf
                    img.ScaleToFit(PageSize.LETTER.Width, PageSize.LETTER.Height);
                    img.SetDpi(300, 300);
                }
                catch (Exception)
                {
                    LogError.WriteMessage("Trazabilidad", "Aplicacion", item + " factura: " + sinvoice);
                    img = iTextSharp.text.Image.GetInstance(Configuration.GetStringValue("BlankImage"));
                }
                if (stamper != null)
                {
                    // Check if we need to add a new page
                    if (reader.NumberOfPages < 1)
                    {
                        document.NewPage(); // Document part is just for creating space
                    }
                    stamper.InsertPage(reader.NumberOfPages + 1, PageSize.LETTER);
                    PdfContentByte content = stamper.GetOverContent(reader.NumberOfPages);
                    content.AddImage(img);
                    ipages++;
                }
                else
                {
                    document.Add(img);
                    document.NewPage();
                    ipages++;
                }               
            }
            if (stamper != null)
            {
                stamper.Close();
                reader.Close();
                fs.Close();
                File.Delete(pdfFilePath); // Delete the original file
                File.Move(pdfFilePath + "_temp", pdfFilePath); // Rename the temporary file to the original file
            }
            else
            {
                document.Close();
                fs.Close();
            }
            return ipages;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}