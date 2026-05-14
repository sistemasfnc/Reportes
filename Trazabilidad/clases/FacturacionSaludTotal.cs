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


namespace Trazabilidad.clases
{
    
    public class FacturacionSaludTotal : IDisposable
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
                sfilename.Append("800180553_");
                sfilename.Append("SETT_");
                sfilename.Append(item.invoice);
                sfilename.Append("_");                            
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
            sfilename.Append(this.GetSuppotType("Factura"));
            sfilename.Append("_");
            sfilename.Append("1.PDF");
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
                foreach (var generic in lgeneric)
                {
                    supportpath = new string[] { Configuration.GetStringValue("SupportsPath"), generic.date.Year.ToString(), generic.date.Month.ToString(), generic.date.Day.ToString() };
                    lsupports = new List<string>();
                    lsupports = this.SearchFile(generic.name + "*.jpg", Path.Combine(supportpath), lsupports);
                    if (lsupports.Count > 0)
                    {
                        this.GeneratePDFSupport(generic.name, lsupports, generic.code, sname);
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
        /*private void GeneratePDFSupport(string sfile, List<string> lFiles, string stype, string sinvoicename)
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation };
            string spath = Path.Combine(paths);
            StringBuilder ssupportfile = new StringBuilder(sinvoicename);
            ssupportfile.Append(this.GetSuppotType(stype));
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
        }        
        */
        private void GeneratePDFSupport(string sfile, List<string> lFiles, string stype, string sinvoicename)
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation };
            string spath = Path.Combine(paths);
            StringBuilder ssupportfile = new StringBuilder(sinvoicename);
            ssupportfile.Append(this.GetSuppotType(stype));
            ssupportfile.Append("_1.PDF");
            paths = new string[] { spath, ssupportfile.ToString() };
            string pdffile = Path.Combine(paths);

            Document document = new Document(PageSize.LETTER);
            FileStream fs = null;
            PdfWriter writer = null;
            PdfReader reader = null;
            PdfStamper stamper = null;

            try
            {
                if (File.Exists(pdffile))
                {
                    reader = new PdfReader(pdffile);
                    fs = new FileStream(pdffile + "_temp", FileMode.Create);
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
                    catch (Exception)
                    {
                        LogError.WriteMessage("Trazabilidad", "Aplicacion", item + " factura: " + sinvoicename);
                        continue;  // Skip this iteration if the image fails to load
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
            }
            finally
            {
                if (stamper != null)
                {
                    stamper.Close();
                    reader.Close();
                    fs.Close();
                    File.Delete(pdffile); // Delete the original file
                    File.Move(pdffile + "_temp", pdffile); // Rename the temporary file to the original file
                }
                else if (document.IsOpen())
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

        private string GetSuppotType(string ssupport)
        {
            switch (ssupport)
            {
                case "Autorizacion": return "17";
                case "Orden Medica": return "5";
                case "Soporte Clinico": return "5";
                case "Detalle de Factura": return "2";
                default: return "1";
            }
        }

        public void UploadFiles()
        {
            string directory = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation);
            if (Directory.Exists(directory))
            {                                         
                FileInfo fileInfo = null;
                WebClient client = null;               
                foreach (string sfile in Directory.GetFiles(directory, "*.*"))
                {                                       
                    try
                    {
                        fileInfo = new FileInfo(sfile);
                        client = new WebClient();
                        client.Credentials = new NetworkCredential(Configuration.GetStringValue("SaludTotalUser"), Configuration.GetStringValue("SaludTotalPassword"));
                        client.UploadFile(Configuration.GetStringValue("SaludTotalFtp") + "FactElectronica/800180553/BOGOTA/" + fileInfo.Name, sfile);                        
                    }
                    catch (Exception ex)
                    {
                        LogError.WriteError("Trazabilidad", "Aplicacion", ex);
                        throw;
                    }
                    finally
                    {                       
                        fileInfo = null;
                        client = null;
                    }
                }
            }
        }

        public void Dispose()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}