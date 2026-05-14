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
    public class FacturacionBolivar : IDisposable
    {
        public string sRelation { get; set; }

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
            foreach (var item in result)
            {
                sfilename.Append("FAC_");
                sfilename.Append("SETT");
                sfilename.Append(item.invoice);                
                this.GeneratePDF(ldesmaterializacion, item.invoice, item.file, sfilename.ToString());
                sfilename.Clear();
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
            StringBuilder sfilename = new StringBuilder(sname);           
            sfilename.Append("_");
            sfilename.Append("0.PDF");
            string sinvoicefile = Path.Combine(Configuration.GetStringValue("InvoicesPath"), sfile.ToString() + ".PDF");
            List<string> lsupports = null;
            List<Generic> lgeneric = null;
            string[] supportpath = null;
            if (File.Exists(sinvoicefile))
            {
                string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, sfilename.ToString() };
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
                lsupports = new List<string>();
                foreach (var generic in lgeneric)
                {
                    supportpath = new string[] { Configuration.GetStringValue("SupportsPath"), generic.date.Year.ToString(), generic.date.Month.ToString(), generic.date.Day.ToString() };
                    lsupports.AddRange(this.SearchFile(generic.name + "*.jpg", Path.Combine(supportpath), lsupports));                    
                }                
                if (lsupports.Count > 0)
                {
                    lsupports = lsupports.Distinct().ToList();
                    this.GeneratePDFSupport(lsupports, sname);
                }
            }
        }


        /// <summary>
        /// Método que genera el pdf para cada soporte
        /// </summary>
        /// <param name="lFiles">Lista genérica con los archivos en JPG</param>
        /// <param name="sfolder">String carpeta destino</param>
        private void GeneratePDFSupport(List<string> lFiles, string sinvoicename)
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation };
            string spath = Path.Combine(paths);
            StringBuilder ssupportfile = new StringBuilder(sinvoicename);
            ssupportfile.Append("_1.PDF");
            paths = new string[] { spath, ssupportfile.ToString() };
            string pdffile = Path.Combine(paths);

            Document document = new Document(PageSize.LETTER);
            FileStream fs = null;
            PdfWriter writer = null;
            PdfReader reader = null;
            PdfStamper stamper = null;

            if (File.Exists(pdffile))
            {
                reader = new PdfReader(pdffile);
                fs = new FileStream(pdffile + "_temp", FileMode.Create);  // Create a temporary file
                stamper = new PdfStamper(reader, fs);
            }
            else
            {
                fs = new FileStream(pdffile, FileMode.Create);
                writer = PdfWriter.GetInstance(document, fs);
                document.Open();
            }

            foreach (string item in lFiles)
            {
                iTextSharp.text.Image img = null;
                try
                {
                    img = iTextSharp.text.Image.GetInstance(item);
                    img.SetAbsolutePosition(0, 0); // set the position to bottom left corner of the pdf
                    img.ScaleToFit(PageSize.LETTER.Width, PageSize.LETTER.Height);
                    img.SetDpi(300, 300);
                }
                catch (Exception ex)
                {
                    LogError.WriteMessage("Trazabilidad", "Aplicacion", item + " factura: " + sinvoicename);
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
                }
                else
                {
                    document.Add(img);
                    document.NewPage();
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
        }

        /*private void GeneratePDFSupport(List<string> lFiles, string sinvoicename)
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation };
            string spath = Path.Combine(paths);
            StringBuilder ssupportfile = new StringBuilder(sinvoicename);            
            ssupportfile.Append("_");
            ssupportfile.Append("1.PDF");
            paths = new string[] { spath, ssupportfile.ToString() };
            string pdffile = Path.Combine(paths);
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
            iTextSharp.text.Image img = null;
            foreach (string item in lFiles)
            {
                try
                {
                    img = iTextSharp.text.Image.GetInstance(item);
                }
                catch (Exception ex)
                {
                    LogError.WriteError("Trazabilidad", "Aplicacion", ex);
                    img = iTextSharp.text.Image.GetInstance("~/images/blank.png");
                }
                img.SetAbsolutePosition(0, 0); // set the position to bottom left corner of pdf
                document.SetPageSize(PageSize.LETTER);
                img.ScaleToFit(PageSize.LETTER.Width, PageSize.LETTER.Height);
                //img.ScaleAbsolute(iTextSharp.text.PageSize.LETTER.Width, iTextSharp.text.PageSize.LETTER.Height);
                img.SetDpi(300, 300);
                document.Add(img);
                document.NewPage();
            }
            document.Close();
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
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}