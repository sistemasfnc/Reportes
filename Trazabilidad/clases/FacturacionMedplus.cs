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
using System.Xml.Linq;

namespace Trazabilidad.clases
{
    public class FacturacionMedplus : IDisposable
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
                this.GenerateFiles(ldesmaterializacion, item.invoice, item.file);               
            }
        }

        private void GenerateFiles(List<Desmaterializacion> ldesmaterializacion, string sinvoice, string sfile)
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, "SETT" + sinvoice };
            string sdirectory = Path.Combine(paths);
            if (!Directory.Exists(sdirectory))
            {
                Directory.CreateDirectory(sdirectory);
            }
            Document document = new Document();
            string spdf = string.Empty;
            StringBuilder sfilename = new StringBuilder("FEV_800180553_SETT");
            sfilename.Append(sinvoice);
            sfilename.Append(".PDF");
            string sinvoicefile = Path.Combine(Configuration.GetStringValue("InvoicesPath"), sfile.ToString() + ".PDF");
            string sxmlfile = Path.Combine(Configuration.GetStringValue("InvoicesPath"), sfile.ToString() + ".XML");
            List<Generic> lgeneric = null;
            string[] supportpath = null;
            List<string> lsupports = null;
            if (File.Exists(sinvoicefile))
            {
                paths = new string[] { sdirectory, sfilename.ToString() };
                File.Copy(sinvoicefile, Path.Combine(paths));
            }
            if (File.Exists(sxmlfile))
            {
                paths = new string[] { sdirectory, sfilename.ToString().Replace(".PDF", ".XML").Replace("FEV", "XML") };
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
                        this.GeneratePDFSupport(generic.name, lsupports, generic.code, sinvoice, sdirectory);
                    }
                }
            }
            paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, "RIPS" };
            string sripsdir = Path.Combine(paths);
            if (!Directory.Exists(sripsdir))
            {
                Directory.CreateDirectory(sripsdir);
            }
            this.CopyExtraFiles(sinvoice, sripsdir);
        }
        private string GetFileName(string sfilepath)
        {
            FileInfo fileInfo = new FileInfo(sfilepath);
            return fileInfo.Name;
        }

        private void CopyExtraFiles(string sinvoice, string sdestination)
        {
            string[] paths = new string[] { Configuration.GetStringValue("CuvPath"), $"SETT{sinvoice}" };
            string sdirectory = Path.Combine(paths);
            if (Directory.Exists(sdirectory))
            {
                string scuv = Directory.GetFiles(sdirectory).FirstOrDefault(x => Path.GetFileName(x).Contains("CUV_"));
                string sxml = Directory.GetFiles(sdirectory).FirstOrDefault(x => x.EndsWith(".xml"));
                string srips = Directory.GetFiles(sdirectory).FirstOrDefault(x => Path.GetFileName(x).Contains("RIPS_"));
                if (!string.IsNullOrEmpty(scuv))
                {
                    string sfinalcuv = Path.Combine(sdestination, this.GetFileName(scuv));
                    File.Copy(scuv, sfinalcuv);    
                }
                if (!string.IsNullOrEmpty(sxml))
                {
                    string sfinalxml = Path.Combine(sdestination, this.GetFileName(sxml)); 
                    File.Copy(sxml, sfinalxml);                    
                }
                if (!string.IsNullOrEmpty(srips))
                {
                    string sfinalrip = Path.Combine(sdestination, this.GetFileName(srips));
                    File.Copy(srips, sfinalrip);                    
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
        private void GeneratePDFSupport(string sfile, List<string> lFiles, string stype, string sinvoicename, string sinvoice)
        {
            string[] paths = new string[] { sinvoice };
            string spath = Path.Combine(paths);
            StringBuilder ssupportfile = new StringBuilder();
            ssupportfile.Append(this.GetSuppotType(stype));
            ssupportfile.Append("_800180553_SETT");
            ssupportfile.Append(sinvoicename);
            ssupportfile.Append(".PDF");
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
                    img = iTextSharp.text.Image.GetInstance(Configuration.GetStringValue("BlankImage"));
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
                case "Autorizacion": return "OPF";
                case "Detalle de Factura": return "DTC";
                default: return "PDX";
            }
        }

        public void Dispose()
        {
           GC.SuppressFinalize(this);
           GC.Collect();
        }
    }
}