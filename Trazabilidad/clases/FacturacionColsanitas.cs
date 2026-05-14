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
using System.Net;
using System.ComponentModel.Design;
using System.Diagnostics;
using static iTextSharp.text.pdf.AcroFields;


namespace Trazabilidad.clases
{
    public class FacturacionColsanitas : IDisposable
    {
        public string sRelation { get; set; }

        public string stype { get; set; }

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
            foreach (var item in result)
            {                
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
        /*
        private void GeneratePDF(List<Desmaterializacion> ldesmaterializacion, string sinvoice, string sfile, string sname)
        {
            Document document = null;
            string spdf = string.Empty;
            StringBuilder sfilename = new StringBuilder(sname);          
            sfilename.Append(".PDF");
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
                    lsupports = this.SearchFile(generic.name + "*.jpg", Path.Combine(supportpath), lsupports);                    
                }
            }
            if (lsupports.Count > 0)
            {
                FileStream fs = null;
                string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation };
                string spath = Path.Combine(paths);
                StringBuilder ssupportfile = new StringBuilder(sname);
                ssupportfile.Append("_SOP_1.PDF");
                paths = new string[] { spath, ssupportfile.ToString() };
                string pdffile = Path.Combine(paths);

                if (!File.Exists(pdffile))
                {
                    fs = new FileStream(pdffile, FileMode.Create);
                }
                else
                {
                    fs = new FileStream(pdffile, FileMode.Append);
                }
                document = new Document();
                PdfWriter.GetInstance(document, fs);
                document.Open();
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
                document.Close();
            }
        }
        */

        
        private void GeneratePDF(List<Desmaterializacion> ldesmaterializacion, string sinvoice, string sfile, string sname)
        {
            string spdf = string.Empty;
            StringBuilder sfilename = new StringBuilder(sname);
            sfilename.Append(".PDF");
            string sinvoicefile = Path.Combine(Configuration.GetStringValue("InvoicesPath"), sfile + ".PDF");
            List<string> lsupports = new List<string>();
            List<Generic> lgeneric = new List<Generic>();
            string[] supportpath = null;

            if (File.Exists(sinvoicefile))
            {
                string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, sfilename.ToString() };
                File.Copy(sinvoicefile, Path.Combine(paths), true);
            }

            var tmp = ldesmaterializacion.Where(z => z.sfactura == sinvoice).Select(x => x.sepisodio)
                        .Distinct()
                        .ToList();

            foreach (var chapter in tmp)
            {
                lgeneric = this.GetInvoiceSupports(chapter);
                lsupports = new List<string>();
                foreach (var generic in lgeneric)
                {
                    supportpath = new string[] { Configuration.GetStringValue("SupportsPath"), generic.date.Year.ToString(), generic.date.Month.ToString(), generic.date.Day.ToString() };
                    lsupports = this.SearchFile(generic.name + "*.jpg", Path.Combine(supportpath), lsupports);
                }
            }

            if (lsupports.Count > 0)
            {
                FileStream fs = null;
                string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation };
                string spath = Path.Combine(paths);
                StringBuilder ssupportfile = new StringBuilder(sname);
                ssupportfile.Append("_SOP_1.PDF");
                paths = new string[] { spath, ssupportfile.ToString() };
                string pdffile = Path.Combine(paths);
                Document document = new Document(PageSize.LETTER);
                PdfReader reader = null;
                PdfStamper stamper = null;
                if (File.Exists(pdffile))
                {
                    reader = new PdfReader(pdffile);
                    fs = new FileStream(pdffile + "_temp", FileMode.Create);
                    stamper = new PdfStamper(reader, fs);
                }
                else
                {
                    fs = new FileStream(pdffile, FileMode.Create);
                    PdfWriter.GetInstance(document, fs);
                    document.Open();
                }
                foreach (string support in lsupports)
                {
                    iTextSharp.text.Image img = null;
                    try
                    {   
                        img = iTextSharp.text.Image.GetInstance(support);
                        img.SetAbsolutePosition(0, 0); // set the position to bottom left corner of the pdf
                        img.ScaleToFit(PageSize.LETTER.Width, PageSize.LETTER.Height);
                        img.SetDpi(300, 300);
                    }
                    catch (Exception)
                    {
                        //LogError.WriteError("Trazabilidad", "Aplicacion", ex);
                        LogError.WriteMessage("Trazabilidad", "Aplicacion", support + " factura: " + sinvoice);
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
                    File.Delete(pdffile);
                    File.Move(pdffile + "_temp", pdffile);
                }
                else
                {
                    document.Close();
                    fs.Close();
                }
            }
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
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}