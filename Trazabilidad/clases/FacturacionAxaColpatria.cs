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
    public class FacturacionAxaColpatria : IDisposable
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
            directory = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, "FACTURAS");
            Directory.CreateDirectory(directory);
            directory = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, "SOPORTES");
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
            string sinvoicefile = Path.Combine(Configuration.GetStringValue("InvoicesPath"), sfile + ".pdf");                        
            List<Generic> lgeneric = null;
            string[] paths = null;            
            if (File.Exists(sinvoicefile))
            {
                paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, "FACTURAS", "SETT" + sinvoice + ".pdf" };
                File.Copy(sinvoicefile, Path.Combine(paths));                
            }
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
                lgeneric = this.GetInvoiceSupports(item.chapter);
                this.CreatePDFSupports(lgeneric, sinvoice);
            }            
        }

        /// <summary>
        /// Método que genera un archivo PDF con los soportes de la factura
        /// </summary>
        /// <param name="lgeneric">Lista genérica con los soportes encontrados</param>
        /// <param name="sinvoice">String número de factura</param>
        /// <returns>String ruta del archivo PDF de soportes</returns>
        
        private void CreatePDFSupports(List<Generic> lgeneric, string sinvoice)
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, "SOPORTES", "SETT" + sinvoice + "_SOP.pdf" };
            string pdffile = Path.Combine(paths);
            int ifilesnumber = 0;
            List<string> lsupports = null;
            Document document = new Document(PageSize.LETTER);
            PdfReader reader = null;
            PdfStamper stamper = null;
            FileStream fs = null;
            bool fileExists = File.Exists(pdffile);

            if (lgeneric.Count > 0)
            {
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
                    string[] supportPath = new string[] { Configuration.GetStringValue("SupportsPath"), item.date.Year.ToString(), item.date.Month.ToString(), item.date.Day.ToString() };
                    lsupports = new List<string>();
                    lsupports = this.SearchFile(item.name + "*.jpg", Path.Combine(supportPath), lsupports);
                    if (lsupports.Count > 0)
                    {
                        foreach (string support in lsupports)
                        {
                            iTextSharp.text.Image img;
                            try
                            {
                                img = iTextSharp.text.Image.GetInstance(support);
                                
                            }
                            catch (Exception ex)
                            {

                                LogError.WriteError("Trazabilidad", "Aplicacion", ex);
                                img = iTextSharp.text.Image.GetInstance(Configuration.GetStringValue("BlankImage"));
                            }
                            img.SetAbsolutePosition(0, 0); // set the position to bottom left corner of the pdf
                            img.ScaleToFit(PageSize.LETTER.Width, PageSize.LETTER.Height);
                            img.SetDpi(300, 300);
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
                            }
                            else
                            {
                                document.Add(img);
                                document.NewPage();
                            }
                            ifilesnumber++;

                        }
                    }
                    else
                    {
                        if (stamper == null) // Only add blank pages if we are creating a new document
                        {
                            document.NewPage();
                            ifilesnumber++;
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
                // Rename the final PDF file to include the count of files/pages added
                string finalPath = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, "SOPORTES", $"SETT{sinvoice}_SOP_{ifilesnumber}.pdf");
                if (File.Exists(finalPath))
                {
                    File.Delete(finalPath); // Ensure we do not have a naming conflict
                }
                File.Move(pdffile, finalPath);
            }
        }
        

        /*
        private void CreatePDFSupports(List<Generic> lgeneric, string sinvoice)
        {
            List<string> lsupports = null;
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, "SOPORTES", "SETT" + sinvoice + "_SOP.pdf" };
            string pdffile = Path.Combine(paths);
            FileStream fs = null;
            int ifilesnumber = 0;
            string[] supportpath = null;
            if (lgeneric.Count > 0)
            {
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
                            ifilesnumber++;
                        }
                    }
                    else
                    {
                        document.NewPage();
                        ifilesnumber++;
                    }
                }
                document.Close();
                paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, "SOPORTES", "SETT" + sinvoice + "_SOP_" + ifilesnumber.ToString() + " .pdf" };
                if (!File.Exists(Path.Combine(paths)))
                {
                    File.Move(pdffile, Path.Combine(paths));
                }
            }                     
        }
        */

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