using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Facade;
using Entity;
using Config;
using Utils;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Ionic.Zip;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.IO;
using EventLog;

namespace Trazabilidad.clases
{
    public class FacturacionSuramericana : IDisposable
    {
        public string sRelation { get; set; }

        /// <summary>
        /// Método que genera los archivos en PDF y ZIP de la relación de envío seleccionada
        /// </summary>
        /// <param name="ldesmaterializacion">Lista genérica con los detalles de la relación de envío</param>
        public void GenerateFiles(List<Desmaterializacion> ldesmaterializacion)
        {
            StringBuilder sfilename = new StringBuilder();
            string directory = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
            Directory.CreateDirectory(directory);
            var result = ldesmaterializacion.Select
            (
                x => new
                {
                    sinviove = x.sfactura,
                    ssource = x.sfuentefactura,
                    sdate = x.sfechafactura,
                    sfile = x.sarchivo,
                    dvalue = (Convert.ToDecimal(x.svalorfacturado) - Convert.ToDecimal(x.scopago))
                }
            ).GroupBy
            (
                x => new
                {
                    x.sinviove,
                    x.ssource,
                    x.sfile,
                    x.sdate
                }
            ).Select
            (
                g => new
                {
                    invoice = g.Key.sinviove,
                    file = g.Key.sfile,
                    source = g.Key.ssource,
                    date = g.Key.sdate,
                    total = g.Sum(x => x.dvalue)
                }
            ).OrderBy(y => y.invoice).ToList();
            List<string> lTmp = new List<string>();
            foreach (var item in result)
            {
                if (lTmp.FirstOrDefault(x => x.Contains(item.invoice)) == null)
                {
                    sfilename.Append("SETT");
                    sfilename.Append(item.invoice);
                    if (!Directory.Exists(sfilename.ToString()))
                    {
                        Directory.CreateDirectory(Path.Combine(directory, sfilename.ToString()));
                    }
                    this.GeneratePDF(ldesmaterializacion, item.invoice, item.file, sfilename.ToString());
                    sfilename.Clear();
                    lTmp.Add(item.invoice);
                }
            }
        }

        /// <summary>
        /// Método para generar el archivo PDF con la factura y sus soportes
        /// </summary>
        /// <param name="ldesmaterializacion">Lista genérica que contiene la información de la relación de envío</param>
        /// <param name="sinvoice">String número de factura</param>
        /// <param name="sfile">String nombre del archivo de la factura</param>
        /// <param name="sname">String nombre del archivo a generar tanto en PDF como en ZIP</param>
        private void GeneratePDF(List<Desmaterializacion> ldesmaterializacion, string sinvoice, string sfile, string sname)
        {
            Document document = new Document();
            string spdf = string.Empty;
            PdfImportedPage page = null;
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, sname, sname + ".pdf" };
            string sresultfile = Path.Combine(paths);
            if (File.Exists(Path.Combine(Configuration.GetStringValue("InvoicesPath"), sfile + ".pdf")))
            {
                using (FileStream newFileStream = new FileStream(sresultfile, FileMode.Create))
                {
                    PdfCopy writer = new PdfCopy(document, newFileStream);
                    document.Open();
                    PdfReader reader = new PdfReader(Path.Combine(Configuration.GetStringValue("InvoicesPath"), sfile + ".pdf"));
                    page = writer.GetImportedPage(reader, 1);
                    writer.AddPage(page);
                    writer.FreeReader(reader);
                    reader.Close();
                    var tmp = ldesmaterializacion.Where(z => z.sfactura == sinvoice).Select
                    (
                        x => new
                        {
                            schapter = x.sepisodio,
                            sinvoive = x.sfactura
                        }
                    )
                    .GroupBy
                    (
                        x => new
                        {
                            x.schapter,
                            x.sinvoive
                        }
                    )
                    .Select
                    (
                        y => new
                        {
                            chapter = y.Key.schapter
                        }
                    ).ToList();
                    foreach (var item in tmp)
                    {
                        List<Generic> lgeneric = this.GetInvoiceSupports(item.chapter);
                        if (lgeneric.Count > 0)
                        {
                            spdf = this.CreatePDFSupports(lgeneric, sinvoice);
                            reader = new PdfReader(spdf);
                            for (int i = 0; i < reader.NumberOfPages; i++)
                            {
                                page = writer.GetImportedPage(reader, i + 1);
                                writer.AddPage(page);
                            }
                            writer.FreeReader(reader);
                            reader.Close();
                            File.Delete(spdf);
                        }
                    }
                    document.Close();
                }
            }
        }


        /// <summary>
        /// Método que genera un archivo PDF con los soportes de la factura
        /// </summary>
        /// <param name="lgeneric">Lista genérica con los soportes encontrados</param>
        /// <param name="sinvoice">String número de factura</param>
        /// <returns>String ruta del archivo PDF de soportes</returns>
        /*private string CreatePDFSupports(List<Generic> lgeneric, string sinvoice)
        {
            List<string> lsupports = null;
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, "soportes" + sinvoice + ".pdf" };
            string pdffile = Path.Combine(paths);
            string[] supportpath = null;
            FileStream fs = null;
            if (!File.Exists(pdffile))
            {
                fs = new FileStream(pdffile, FileMode.Create);
            }
            else
            {
                fs = new FileStream(pdffile, FileMode.Append);
            }
            Document document = new Document();
            PdfWriter.GetInstance(document, fs);
            document.Open();
            foreach (var item in lgeneric)
            {
                supportpath = new string[] { Configuration.GetStringValue("SupportsPath"), item.date.Year.ToString(), item.date.Month.ToString(), item.date.Day.ToString() };
                lsupports = new List<string>();
                lsupports = this.SearchFile(item.name + "*.jpg", Path.Combine(supportpath), lsupports);
                if (lsupports.Count > 0)
                {
                    foreach (string support in lsupports)
                    {
                        iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(support);
                        img.SetAbsolutePosition(0, 0); // set the position to bottom left corner of pdf
                        document.SetPageSize(PageSize.LETTER);
                        img.ScaleToFit(PageSize.LETTER.Width, PageSize.LETTER.Height);
                        //img.ScaleAbsolute(iTextSharp.text.PageSize.LETTER.Width, iTextSharp.text.PageSize.LETTER.Height);
                        img.SetDpi(300, 300);
                        document.Add(img);
                        document.NewPage();
                    }
                }

            }
            document.Close();
            return pdffile;
        }*/
        private string CreatePDFSupports(List<Generic> lgeneric, string sinvoice)
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, "soportes" + sinvoice + ".pdf" };
            string pdffile = Path.Combine(paths);
            FileStream fs = null;
            Document document = new Document(PageSize.LETTER);
            PdfReader reader = null;
            PdfStamper stamper = null;
            bool fileExists = File.Exists(pdffile);
            List<string> lsupports = null;
            if (fileExists)
            {
                reader = new PdfReader(pdffile);
                fs = new FileStream(pdffile + "_temp", FileMode.Create);  // Create a temporary file
                stamper = new PdfStamper(reader, fs);
            }
            else
            {
                fs = new FileStream(pdffile, FileMode.Create);
                PdfWriter.GetInstance(document, fs);
                document.Open();
            }
            foreach (var item in lgeneric)
            {
                string[] supportpath = { Configuration.GetStringValue("SupportsPath"), item.date.Year.ToString(), item.date.Month.ToString(), item.date.Day.ToString() };
                lsupports = new List<string>();
                lsupports = this.SearchFile(item.name + "*.jpg", Path.Combine(supportpath), lsupports);
                foreach (string support in lsupports)
                {
                    try
                    {
                        iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(support);
                        img.SetAbsolutePosition(0, 0); // set the position to bottom left corner of the pdf
                        img.ScaleToFit(PageSize.LETTER.Width, PageSize.LETTER.Height);
                        img.SetDpi(300, 300);
                        if (stamper != null)
                        {
                            PdfContentByte content = stamper.GetOverContent(reader.NumberOfPages + 1);
                            if (content != null)
                            {
                                content.AddImage(img);
                            }
                        }
                        else
                        {
                            document.Add(img);
                            document.NewPage();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError.WriteMessage("Trazabilidad", "Aplicacion", support + " factura: " + sinvoice);
                        continue;
                    }
                    
                }
            }
            if (stamper != null)
            {
                stamper.Close();
                reader.Close();
                fs.Close();
                File.Delete(pdffile); // Delete the original file
                File.Move(pdffile + "_temp", pdffile); // Rename the temporary file to the original file
            }
            else
            {
                document.Close();
                fs.Close();
            }
            return pdffile;
        }

        /// <summary>
        /// Método que obtiene los nombres de los soportes de una factura por número de episio del ingreso
        /// </summary>
        /// <param name="sepisode">String número de episodio</param>
        /// <returns>Lista genérica con los soportes encontrados</returns>
        private List<Generic> GetInvoiceSupports(string sepisode)
        {
            using (FacadeDesmaterializacion facade = new FacadeDesmaterializacion(Configuration.GetStringValue("FNCFacturacion")))
            {
                return facade.GetInvoiceSupports(sepisode);
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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}