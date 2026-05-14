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
    public class FacturacionPositiva : IDisposable
    {
        public string sRelation { get; set; }

        public void GenerateFiles(List<Desmaterializacion> ldesmaterializacion)
        {
            StringBuilder sfilename = new StringBuilder();
            string directory = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation);
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, "ARCHIVOS XML" };
            string sxmldir = Path.Combine(paths);
            string sinvoicedir = string.Empty;
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
            Directory.CreateDirectory(directory);
            Directory.CreateDirectory(sxmldir);
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
                    sinvoicedir = Path.Combine(directory, "SETT_" + item.invoice);
                    if (!Directory.Exists(sinvoicedir))
                    {
                        Directory.CreateDirectory(sinvoicedir);
                    }
                    this.GenerateFiles(ldesmaterializacion, item.invoice, sinvoicedir, item.file, sxmldir);
                    lTmp.Add(item.invoice);
                }
            }
            this.CompressFiles(ldesmaterializacion, sxmldir);
        }                

        private void GenerateFiles(List<Desmaterializacion> ldesmaterializacion, string sinvoice, string sinvoicedir, string sfile, string sxmldir)
        {
            string sinvoicefile = Path.Combine(Configuration.GetStringValue("InvoicesPath"), sfile + ".pdf");
            string sxmlfile = Path.Combine(Configuration.GetStringValue("InvoicesPath"), sfile + ".xml");
            string[] paths = null;
            List<string> lsupports = null;
            List<Generic> lgeneric = null;
            string[] supportpath = null;
            if (File.Exists(sinvoicefile))
            {
                paths = new string[] { sinvoicedir, "Factura.pdf" };
                File.Copy(sinvoicefile, Path.Combine(paths));
            }
            if (File.Exists(sxmlfile))
            {
                paths = new string[] { sxmldir, sfile + ".xml" };
                File.Copy(sxmlfile, Path.Combine(paths));
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
                        this.GeneratePDFSupport(generic.name, lsupports, generic.code, sinvoicedir);
                    }
                }
            }
        }

        private void GeneratePDFSupport(string sfile, List<string> lFiles, string stype, string sfolder)
        {
            if (stype == "Otras")
            {
                stype = "Anexo";
            }
            else if (stype == "Detalle de Factura")
            {
                stype = "Detallado";
            }
            string[] paths = new string[] { sfolder, stype + ".pdf" };                        
            string pdffile = Path.Combine(paths);
            if (!File.Exists(pdffile))
            {
                FileStream fs = new FileStream(pdffile, FileMode.Create);
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
                    img.SetAbsolutePosition(0, 0);
                    document.SetPageSize(PageSize.LETTER);
                    img.ScaleToFit(PageSize.LETTER.Width, PageSize.LETTER.Height);                    
                    img.SetDpi(300, 300);
                    document.Add(img);
                    document.NewPage();
                }
                document.Close();
            }
        }

        private void CompressFiles(List<Desmaterializacion> ldesmaterializacion, string sxmldir)
        {
            string directory = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation);
            string sinvoicedir = string.Empty;
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
            if (result.Count > 0)
            {
                StringBuilder zipfile = new StringBuilder("SETT_");
                zipfile.Append(result.FirstOrDefault().invoice);
                zipfile.Append(".zip");
                string zipresult = Path.Combine(new string[] { directory, zipfile.ToString() });
                try
                {
                    using (var zip = new ZipFile())
                    { 
                        foreach (var item in result)
                        {
                            sinvoicedir = Path.Combine(directory, "SETT_" + item.invoice);
                            if (Directory.Exists(sinvoicedir))
                            {                                
                                zip.AddDirectory(sinvoicedir, "SETT_" + item.invoice);
                                zip.Save(zipresult);
                                Directory.Delete(sinvoicedir, true);
                            }                                                                                                        
                        }                                                
                    }
                }
                catch (Exception ex)
                {
                    LogError.WriteError("Trazabilidad", "Aplicacion", ex);
                    throw;
                }
            }
            if (Directory.Exists(sxmldir) && result.Count > 0)
            {
                StringBuilder zipfile = new StringBuilder(result.FirstOrDefault().file);
                zipfile.Append(".zip");
                string zipresult = Path.Combine(new string[] { directory, zipfile.ToString() });
                using (var zip = new ZipFile())
                {
                    zip.AddDirectory(sxmldir);
                    zip.Save(zipresult);
                }
                Directory.Delete(sxmldir, true);
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
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}