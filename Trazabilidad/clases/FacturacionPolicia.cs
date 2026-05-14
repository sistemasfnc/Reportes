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
using EventLog;
using System.IO;


namespace Trazabilidad.clases
{
    public class FacturacionPolicia : IDisposable
    {
        public string sRelation { get; set; }

        public string stype { get; set; }

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
                if (!Directory.Exists(sfilename.ToString()))
                {
                    Directory.CreateDirectory(Path.Combine(directory, sfilename.ToString()));
                }
                this.GeneratePDF(ldesmaterializacion, item.invoice, item.file, sfilename.ToString());
                //this.CompressFiles(sfilename.ToString());
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
            string sinvoicefile = Path.Combine(Configuration.GetStringValue("InvoicesPath"), sfile + ".pdf");
            List<string> lsupports = null;
            List<Generic> lgeneric = null;
            string[] supportpath = null;
            string sfolder = string.Empty;
            if (File.Exists(sinvoicefile))
            {
                string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, sname + ".pdf" };
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
            if (tmp.Count > 0)
            {
                string[] ssuportpath = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, sname };
                sfolder = Path.Combine(ssuportpath);
                if (!Directory.Exists(sfolder)) Directory.CreateDirectory(sfolder);
            }
            foreach (var item in tmp)
            {
                lgeneric = this.GetInvoiceSupports(item.chapter);
                foreach (var generic in lgeneric)
                {
                    supportpath = new string[] { Configuration.GetStringValue("SupportsPath"), generic.date.Year.ToString(), generic.date.Month.ToString(), generic.date.Day.ToString() };
                    lsupports = new List<string>();
                    lsupports = this.SearchFile(generic.name + "*.jpg", Path.Combine(supportpath), lsupports);
                    if (lsupports.Count > 0)
                    {
                        this.GeneratePDFSupport(generic.name, lsupports, generic.code, sfolder);
                    }
                }
            }
        }


        /// <summary>
        /// Método que genera el pdf para cada soporte
        /// </summary>
        /// <param name="sfile">String nombre del archivo</param>
        /// <param name="lFiles">Lista genérica con los archivos en JPG</param>
        /// <param name="stype">String tipo de archivo</param>
        /// <param name="sfolder">String carpeta destino</param>
        private void GeneratePDFSupport(string sfile, List<string> lFiles, string stype, string sfolder)
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, sfolder };
            string spath = Path.Combine(paths);
            string sfilename = (stype == "Soporte Clinico") ? "Soporte" : "Autorizacion";
            paths = new string[] { spath, sfilename + ".pdf" };
            string pdffile = Path.Combine(paths);
            Document document = new Document(PageSize.LETTER);
            FileStream fs = null;
            PdfWriter writer = null;
            PdfReader reader = null;
            PdfStamper stamper = null;
            MemoryStream memoryStream = new MemoryStream();
            if (File.Exists(pdffile))
            {
                reader = new PdfReader(pdffile);
                stamper = new PdfStamper(reader, memoryStream);
            }
            else
            {
                fs = new FileStream(pdffile, FileMode.Create);
                writer = PdfWriter.GetInstance(document, fs);
                document.Open();
            }
            foreach (string item in lFiles)
            {
                iTextSharp.text.Image img;
                try
                {
                    img = iTextSharp.text.Image.GetInstance(item);
                }
                catch (Exception ex)
                {
                    LogError.WriteError("Trazabilidad", "Aplicacion", ex);
                    img = iTextSharp.text.Image.GetInstance(Configuration.GetStringValue("BlankImage"));
                }
                img.SetAbsolutePosition(0, 0); // set the position to bottom left corner of the page
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
            }
            if (stamper != null)
            {
                stamper.Close(); // close the stamper, this will finalize the new PDF with all pages
                reader.Close();
                File.WriteAllBytes(pdffile, memoryStream.ToArray()); // write the memory stream to file
            }
            else
            {
                document.Close(); // close the document, this finalizes the new PDF
                fs.Close();
            }
        }
        /*private void GeneratePDFSupport(string sfile, List<string> lFiles, string stype, string sfolder)
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, sfolder };
            string spath = Path.Combine(paths);
            string sfilename = (stype == "Soporte Clinico") ? "Soporte" : "Autorizacion";
            paths = new string[] { spath, sfilename + ".pdf" };
            string pdffile = Path.Combine(paths);
            FileStream fs = (File.Exists(pdffile)) ? new FileStream(pdffile, FileMode.Append) : new FileStream(pdffile, FileMode.Create);
            Document document = new Document();
            PdfWriter.GetInstance(document, fs);
            document.Open();
            iTextSharp.text.Image img = null;
            foreach (string item in lFiles)
            {
                try
                {
                    document.NewPage();
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
            }
            document.Close();
        }*/

        /// <summary>
        /// Método para comprimir el archivo PDF generado
        /// </summary>
        /// <param name="spdf">String ruta del archivo PDF</param>
        public void CompressFiles(string spdf)
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, spdf + ".pdf" }; ;
            string sfile = Path.Combine(paths);
            StringBuilder zipfile = new StringBuilder(spdf);
            zipfile.Append(".zip");
            paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, zipfile.ToString() };
            string zipresult = Path.Combine(paths);
            if (File.Exists(zipresult))
            {
                File.Delete(zipresult);
            }
            using (var zip = new ZipFile())
            {
                zip.AddFile(sfile, string.Empty);
                zip.Save(zipresult);
            }
        }

        /// <summary>
        /// Método que genera un archivo PDF con los soportes de la factura
        /// </summary>
        /// <param name="lgeneric">Lista genérica con los soportes encontrados</param>
        /// <param name="sinvoice">String número de factura</param>
        /// <returns>String ruta del archivo PDF de soportes</returns>
        private string CreatePDFSupports(List<Generic> lgeneric, string sinvoice)
        {
            List<string> lsupports = null;
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, "soportes" + sinvoice + ".pdf" };
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
            foreach (var item in lgeneric)
            {
                lsupports = new List<string>();
                lsupports = this.SearchFile(item.name + "*.jpg", Configuration.GetStringValue("SupportsPath"), lsupports);
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