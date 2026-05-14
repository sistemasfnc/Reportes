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
    public class FacturacionNuevaEPS : IDisposable
    {
        public string sRelation { get; set; }

        public List<string> lFinalFiles { get; set; }
       

        public void GenerateFiles(List<Desmaterializacion> ldesmaterializacion)
        {
            this.lFinalFiles = new List<string>();
            string[] scolumns = null;
            StringBuilder ssource = new StringBuilder("CONTROL_");
            ssource.Append(sRelation);
            ssource.Append(".TXT");
            StringBuilder stringBuilder = new StringBuilder();
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
                    source = g.Key.ssource,
                    date = g.Key.sdate,
                    file = g.Key.sfile,
                    total = g.Sum(x => x.dvalue)
                }
            ).OrderBy(y => y.invoice).ToList();            
            StringBuilder sfilename = new StringBuilder();
            string directory = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
            Directory.CreateDirectory(directory);
            foreach (var item in result)
            {
                sfilename.Append("FVS_800180553_SETT");
                sfilename.Append(item.invoice);
                sfilename.Append(".pdf");
                this.GeneratePDF(ldesmaterializacion, item.invoice, item.file, sfilename.ToString());
                //this.CompressFiles(sfilename.ToString());
                sfilename.Clear();
            }
            this.CompressFiles();
            foreach (var item in this.lFinalFiles)
            {
                scolumns = new string[] { "800180553", "SETT", this.GetInvoiceNumber(item), this.GetFileName(item), string.Empty };
                stringBuilder.AppendLine(string.Join(";", scolumns));
            }
            this.WriteFile(stringBuilder.ToString(), ssource.ToString());
            this.DeletePDF();
        }

        private string GetFileName(string sfile)
        {
            return new FileInfo(sfile).Name;
        }

        private string GetInvoiceNumber(string sFile)
        {
            string sname = this.GetFileName(sFile);
            if (sname.Contains("_"))
            {
                string[] parts = sname.Split('_');
                if (parts.Length > 1)
                {
                    return parts[2].Replace("SETT", string.Empty).Replace(".pdf", string.Empty);
                }
            }
            return string.Empty;
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
            if (File.Exists(sinvoicefile))
            {
                string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, sname };
                File.Copy(sinvoicefile, Path.Combine(paths));
                this.lFinalFiles.Add(Path.Combine(paths));
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
                        this.GeneratePDFSupport(generic.name, lsupports, generic.code, sname, sinvoice);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sepisode"></param>
        /// <returns></returns>
        public List<Generic> GetInvoiceSupports(string sepisode)
        {
            using (FacadeDesmaterializacion facade = new FacadeDesmaterializacion(Configuration.GetStringValue("FNCFacturacion")))
            {
                return facade.GetInvoiceSupports(sepisode);
            }
        }

        private void DeletePDF()
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation };
            string path = Path.Combine(paths);
            try
            {
                string[] filesToDelete = Directory.GetFiles(path, "*.pdf");
                filesToDelete.ToList().ForEach(file => File.Delete(file));
            }
            catch (Exception ex)
            {
                LogError.WriteError("Trazabilidad", "Aplicacion", ex);
            }
        }

        /// <summary>
        /// Método para comprimir los archivos generados
        /// </summary>
        public void CompressFiles()
        {
            string sfile = string.Empty;
            StringBuilder zipfile = new StringBuilder("SOPORTES");
            zipfile.Append(this.sRelation);
            string zipDirectory = zipfile.ToString();
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, zipfile.ToString() };
            string spath = Path.Combine(paths);            
            zipfile.Append(".zip");
            paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, zipfile.ToString() };
            string zipresult = Path.Combine(paths);
            if (File.Exists(zipresult))
            {
                File.Delete(zipresult);
            }
            using (var zip = new ZipFile())
            {
                foreach (var item in this.lFinalFiles.Distinct())
                {
                    if (File.Exists(item))
                    {
                        zip.AddFile(item, "");
                    }
                }
                zip.Save(zipresult);
            }            
        }

        private string GetFileType(string stype)
        {
            switch (stype)
            {
                case "Autorizacion": return "OTR_800180553_SETT";
                case "Orden Medica": return "OPF_800180553_SETT";
                case "Soporte Clinico": return "RAA_800180553_SETT";
                case "Detalle de Factura": return "OTR_800180553_SETT";
                default: return "FVS_800180553_SETT";

            }
        }

        /// <summary>
        /// Método que genera el pdf para cada soporte
        /// </summary>
        /// <param name="sfile">String nombre del archivo</param>
        /// <param name="lFiles">Lista genérica con los archivos en JPG</param>
        /// <param name="stype">String tipo de archivo</param>
        /// <param name="sfolder">String carpeta destino</param>
        private void GeneratePDFSupport(string sfile, List<string> lFiles, string stype, string sfolder, string sinvoice)
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation };
            string spath = Path.Combine(paths);
            paths = new string[] { spath, this.GetFileType(stype) + sinvoice + ".pdf" };
            string pdffile = Path.Combine(paths);
            Document document = new Document(PageSize.LETTER);
            PdfWriter writer = null;
            FileStream fs = null;
            if (File.Exists(pdffile))
            {
                // Si el archivo existe, lee el documento existente
                PdfReader reader = new PdfReader(pdffile);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    writer = PdfWriter.GetInstance(document, memoryStream);
                    document.Open();
                    PdfContentByte cb = writer.DirectContent;

                    // Copiar las páginas existentes
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        document.NewPage();
                        PdfImportedPage page = writer.GetImportedPage(reader, i);
                        cb.AddTemplate(page, 0, 0);
                    }
                    // Agregar nuevas páginas con imágenes
                    this.AddNewImagesToDocument(lFiles, document);
                    document.Close();
                    reader.Close();
                    // Guarda el documento actualizado
                    File.WriteAllBytes(pdffile, memoryStream.ToArray());
                }
            }
            else
            {
                // Si el archivo no existe, crea uno nuevo y agrega imágenes
                fs = new FileStream(pdffile, FileMode.Create);
                writer = PdfWriter.GetInstance(document, fs);
                document.Open();
                this.AddNewImagesToDocument(lFiles, document);
                document.Close();
                fs.Close();
            }

            lFinalFiles.Add(pdffile);
        }

        private void AddNewImagesToDocument(List<string> lFiles, Document document)
        {
            iTextSharp.text.Image img = null;
            foreach (string item in lFiles)
            {
                document.NewPage();
                try
                {
                    img = iTextSharp.text.Image.GetInstance(item);
                }
                catch (Exception ex)
                {
                    LogError.WriteMessage("Trazabilidad", "Aplicacion", item);
                    img = iTextSharp.text.Image.GetInstance(Configuration.GetStringValue("BlankImage"));
                }
                img.SetAbsolutePosition(0, 0); // set the position to bottom left corner of pdf
                img.ScaleToFit(PageSize.LETTER.Width, PageSize.LETTER.Height);
                img.SetDpi(300, 300);
                document.Add(img);
            }
        }

        /*private void GeneratePDFSupport(string sfile, List<string> lFiles, string stype, string sfolder, string sinvoice)
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation };
            string spath = Path.Combine(paths);
            paths = new string[] { spath, this.GetFileType(stype) + sinvoice + ".pdf" };
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
            lFinalFiles.Add(pdffile);
            document.Close();
        }*/


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

        private void WriteFile(string scontent, string sfilesource)
        {
            string spath = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation);
            string sfiletarget = Path.Combine(spath, sfilesource);
            if (!Directory.Exists(spath))
            {
                Directory.CreateDirectory(spath);
            }
            Encoding utf8WithoutBom = new UTF8Encoding(false);
            File.WriteAllText(sfiletarget, scontent, utf8WithoutBom);
        }

        public void Dispose()
        {
           GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}