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
    public class FacturacionCompensar : IDisposable
    {

        public string sRelation { get; set; }

        public List<string> lFiles { get; set; }

        public bool bSupports { get; set; }

        public void GenerateHeader(List<Desmaterializacion> ldesmaterializacion)
        {
            string[] scolumns = null;
            StringBuilder ssource = new StringBuilder("ENC_800180553_");
            ssource.Append(DateTime.Now.ToString("yyyyMMdd"));
            ssource.Append(".TXT");
            StringBuilder stringBuilder = new StringBuilder();
            var result = ldesmaterializacion.Select
            (
                x => new
                {
                    sinviove = x.sfactura,
                    ssource = x.sfuentefactura,
                    sdate = x.sfechafactura,
                    dvalue = (Convert.ToDecimal(x.svalorfacturado) - Convert.ToDecimal(x.scopago))
                }
            ).GroupBy
            (
                x => new
                {
                    x.sinviove,
                    x.ssource,
                    x.sdate
                }
            ).Select
            (
                g => new
                {
                    invoice = g.Key.sinviove,
                    source = g.Key.ssource,
                    date = g.Key.sdate,
                    total = g.Sum(x => x.dvalue)
                }
            ).OrderBy(y => y.invoice).ToList();
            foreach (var item in result)
            {
                scolumns = new string[] { "8", "800180553", item.source, item.invoice, item.date, item.total.ToString().Replace(".", ",") };
                stringBuilder.AppendLine(string.Join("|", scolumns));
            }
            this.WriteFile(stringBuilder.ToString(), ssource.ToString());
        }

        public void GenerateDetail(List<Desmaterializacion> ldesmaterializacion)
        {
            string[] scolumns = null;
            StringBuilder ssource = new StringBuilder("DET_800180553_");
            ssource.Append(DateTime.Now.ToString("yyyyMMdd"));
            ssource.Append(".TXT");
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in ldesmaterializacion)
            {
                scolumns = new string[]
                {
                    item.sfuentefactura,
                    item.sfactura,
                    item.sautorizacion,
                    item.scups,
                    item.sservicio,
                    item.scantidad,
                    item.svalorunitario.Replace(".", ","),
                    item.svalorfacturado.Replace(".", ","),
                    item.scopago,
                    item.sdiagnostico,
                    this.GetDocumentType(item.stipodocumento),
                    item.sdocumento,
                    item.sprimerapellido,
                    item.ssegundoapellido,
                    item.sprimernombre,
                    item.ssegundonombre,
                    item.sfechaingreso,
                    item.sfechaegreso,
                    item.scausaexterna,
                };
                stringBuilder.AppendLine(string.Join("|", scolumns));
            }
            this.WriteFile(stringBuilder.ToString(), ssource.ToString());
        }

        private string GetDocumentType(string sdocumenttype)
        {
            if (sdocumenttype == "TI")
            {
                return "3";
            }
            else if (sdocumenttype == "NI")
            {
                return "2";
            }
            else if (sdocumenttype == "CE")
            {
                return "4";
            }
            else if (sdocumenttype == "PA")
            {
                return "5";
            }
            else if (sdocumenttype == "RC")
            {
                return "7";
            }
            else if (sdocumenttype == "NU")
            {
                return "8";
            }
            return "1";
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

        private string GeneratePDFSupport(string sfile, List<string> lFiles, string stype)
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, "SOPORTES" + this.sRelation };
            string spath = Path.Combine(paths);
            if (!Directory.Exists(spath))
            {
                Directory.CreateDirectory(spath);
            }
            paths = new string[] { spath, sfile + ".pdf" };
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
                        img = iTextSharp.text.Image.GetInstance(Configuration.GetStringValue("BlankImage"));                        
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
            return pdffile;
        }

        public void GenerateFileIndex(List<Desmaterializacion> ldesmaterializacion)
        {
            string[] scolumns = null;
            StringBuilder ssource = new StringBuilder("INDEX_800180553_");
            ssource.Append(DateTime.Now.ToString("yyyyMMdd"));
            ssource.Append(".TXT");
            string sinvoicefile = string.Empty;
            string ssuportfile = string.Empty;
            List<string> lsupports = null;
            string sssuport = string.Empty;
            string[] supportpath = null;
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var item in ldesmaterializacion)
            {
                lsupports = new List<string>();
                var lfilesTmp = new List<string>();
                sinvoicefile = Path.Combine(Configuration.GetStringValue("InvoicesPath"), item.sarchivo + ".pdf");
                if (string.IsNullOrEmpty(this.lFiles.FirstOrDefault(x => x.Contains(sinvoicefile))))
                {
                    lfilesTmp.Add(sinvoicefile);
                }
                //sinvoicefile = Path.Combine(Configuration.GetStringValue("InvoicesPath"), item.sarchivo + ".xml");
                //lfilesTmp.Add(sinvoicefile);
                if (!string.IsNullOrEmpty(item.sarchivonc))
                {
                    sinvoicefile = Path.Combine(Configuration.GetStringValue("InvoicesPath"), item.sarchivonc + ".pdf");
                    if (string.IsNullOrEmpty(this.lFiles.FirstOrDefault(x => x.Contains(sinvoicefile))))
                    {
                        lfilesTmp.Add(sinvoicefile);
                        sinvoicefile = Path.Combine(Configuration.GetStringValue("InvoicesPath"), item.sarchivonc + ".xml");
                        lfilesTmp.Add(sinvoicefile);
                    }
                }
                if (!string.IsNullOrEmpty(item.sarchivond))
                {
                    sinvoicefile = Path.Combine(Configuration.GetStringValue("InvoicesPath"), item.sarchivond + ".pdf");
                    if (string.IsNullOrEmpty(this.lFiles.FirstOrDefault(x => x.Contains(sinvoicefile))))
                    {
                        lfilesTmp.Add(sinvoicefile);
                        sinvoicefile = Path.Combine(Configuration.GetStringValue("InvoicesPath"), item.sarchivond + ".xml");
                        lfilesTmp.Add(sinvoicefile);
                    }
                }
                if (this.bSupports)
                {
                    if (!string.IsNullOrEmpty(item.sepisodio))
                    {
                        List<Generic> lgeneric = this.GetInvoiceSupports(item.sepisodio);
                        foreach (var generic in lgeneric)
                        {
                            lsupports = new List<string>();
                            supportpath = new string[] { Configuration.GetStringValue("SupportsPath"), generic.date.Year.ToString(), generic.date.Month.ToString(), generic.date.Day.ToString() };
                            lsupports = this.SearchFile(generic.name + "*.jpg", Path.Combine(supportpath), lsupports);
                            if (lsupports.Count > 0)
                            {
                                sssuport = this.GeneratePDFSupport(generic.name, lsupports, generic.code);
                                if (!item.bmultiple)
                                {
                                    scolumns = new string[]
                                    {
                                        "8",
                                        "800180553",
                                        this.GetDocumentType(sssuport, item.sfuentefactura + item.sfactura, item.sepisodio, generic.code),
                                        item.sfuentefactura + "-" + item.sfactura,
                                        this.GetDocumentType(item.stipodocumento),
                                        item.sdocumento,
                                        this.GetFileName(sssuport).Replace(".jpg", ".pdf").Replace("FE", "SETT"),
                                        "SOPORTES" + this.sRelation,
                                    };
                                }
                                else
                                {
                                    scolumns = new string[]
                                    {
                                        "8",
                                        "800180553",
                                        this.GetDocumentType(sssuport, item.sfuentefactura + item.sfactura, item.sepisodio, generic.code),
                                        item.sfuentefactura + "-" + item.sfactura,
                                        item.sautorizacion,
                                        this.GetDocumentType(item.stipodocumento),
                                        item.sdocumento,
                                        this.GetFileName(sssuport).Replace(".jpg", ".pdf").Replace("FE", "SETT"),
                                        "SOPORTES" + this.sRelation,
                                    };
                                }
                                stringBuilder.AppendLine(string.Join("|", scolumns));
                                this.lFiles.Add(sssuport);
                            }

                        }
                        /*this.GetFiles("*.pdf", Configuration.GetStringValue("SupportsPath"), item.sepisodio, ref lfilesTmp);
                        this.GetFiles("*.jpg", Configuration.GetStringValue("SupportsPath"), item.sepisodio, ref lfilesTmp);*/
                    }
                    else if (!string.IsNullOrEmpty(item.singreso))
                    {
                        this.GetFiles("*.pdf", Configuration.GetStringValue("SupportsPath"), item.singreso, ref lfilesTmp);
                    }
                    lsupports = new List<string>();
                    if (Directory.Exists(Configuration.GetStringValue("ExtraPath")))
                    {
                        lsupports = this.SearchFile(item.sfactura + "_*.pdf", Configuration.GetStringValue("ExtraPath"), lsupports);
                    }                    
                    foreach (var support in lsupports)
                    {
                        string[] asparts = support.Split('_');
                        if (asparts.Length > 2)
                        {
                            scolumns = new string[]
                            {
                                "8",
                                "800180553",
                                asparts[2],
                                item.sfuentefactura + "-" + item.sfactura,
                                this.GetDocumentType(item.stipodocumento),
                                item.sdocumento,
                                this.GetFileName(support),
                                "SOPORTES" + this.sRelation,
                            };
                            stringBuilder.AppendLine(string.Join("|", scolumns));
                            this.lFiles.Add(support);
                        }
                    }

                }
                foreach (string sfile in lfilesTmp.Distinct())
                {
                    if (!item.bmultiple)
                    {
                        scolumns = new string[]
                        {
                            "8",
                            "800180553",
                            this.GetDocumentType(sfile.Replace("FE", "SETT"), item.sfuentefactura + item.sfactura, item.sepisodio),
                            item.sfuentefactura + "-" + item.sfactura,
                            this.GetDocumentType(item.stipodocumento),
                            item.sdocumento,
                            this.GetFileName(sfile).Replace(".jpg", ".pdf").Replace("FE", "SETT"),
                            "SOPORTES" + this.sRelation,
                        };
                    }
                    else
                    {
                        scolumns = new string[]
                        {
                            "8",
                            "800180553",
                            this.GetDocumentType(sfile.Replace("FE", "SETT"), item.sfuentefactura + item.sfactura, item.sepisodio),
                            item.sfuentefactura + "-" + item.sfactura,
                            item.sautorizacion,
                            this.GetDocumentType(item.stipodocumento),
                            item.sdocumento,
                            this.GetFileName(sfile).Replace(".jpg", ".pdf").Replace("FE", "SETT"),
                            "SOPORTES" + this.sRelation,
                        };
                    }
                    stringBuilder.AppendLine(string.Join("|", scolumns));
                    this.lFiles.Add(sfile);
                }
            }
            this.WriteFile(stringBuilder.ToString(), ssource.ToString());
        }

        public List<Generic> GetInvoiceSupports(string sepisode)
        {
            using (FacadeDesmaterializacion facade = new FacadeDesmaterializacion(Configuration.GetStringValue("FNCFacturacion")))
            {
                return facade.GetInvoiceSupports(sepisode);
            }
        }

        private string GetDocumentType(string sfile, string sinvoice, string ssource, string stype = "")
        {
            if (!string.IsNullOrEmpty(stype))
            {
                switch (stype)
                {
                    case "Autorizacion": return "5";
                    case "Orden Medica": return "7";
                    case "Soporte Clinico": return "6";
                    case "Detalle de Factura": return "4";
                    default: return "13";

                }
            }
            else
            {
                if (!string.IsNullOrEmpty(ssource) && sfile.Contains(ssource))
                {
                    return "13";
                }
                if (sfile.Contains(".xml") && sfile.Contains(sinvoice))
                {
                    return "14";
                }
                else if (sfile.Contains(".pdf") && sfile.Contains(sinvoice))
                {
                    return "1";
                }
                else if (sfile.Contains(".xml") && sfile.StartsWith("NC"))
                {
                    return "17";
                }
                else if (sfile.Contains(".xml") && sfile.StartsWith("ND"))
                {
                    return "16";
                }
                else if (sfile.Contains(".pdf") && sfile.StartsWith("NC"))
                {
                    return "2";
                }
                else if (sfile.Contains(".pdf") && sfile.StartsWith("ND"))
                {
                    return "3";
                }
                return "5";
            }
        }

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

        private void GetFiles(string sfiletype, string sdir, string sentry, ref List<string> lfilesTmp)
        {
            foreach (string f in Directory.GetFiles(sdir, sfiletype))
            {
                if (f.Contains("_" + sentry) || f.Contains("-" + sentry))
                {
                    if (string.IsNullOrEmpty(this.lFiles.FirstOrDefault(x => x.Contains(f))))
                    {
                        lfilesTmp.Add(f);
                    }
                }
            }
            foreach (string d in Directory.GetDirectories(sdir))
            {
                GetFiles(sfiletype, d, sentry, ref lfilesTmp);
            }
        }

        private string GetFileName(string sfile)
        {
            return new FileInfo(sfile).Name;
        }

        private void DeleteSupportDir()
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, "SOPORTES" };
            string spath = Path.Combine(paths);
            Directory.Delete(spath, true);
        }

        public void ZipHeader()
        {
            StringBuilder zipfile = new StringBuilder("FAC_800180553_");
            zipfile.Append(DateTime.Now.ToString("yyyyMMdd"));
            zipfile.Append(".zip");
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, zipfile.ToString() };
            string spath = Path.Combine(paths);
            StringBuilder sdetail = new StringBuilder("DET_800180553_");
            sdetail.Append(DateTime.Now.ToString("yyyyMMdd"));
            sdetail.Append(".TXT");
            paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, sdetail.ToString() };
            string sdetailfile = Path.Combine(paths);
            StringBuilder sheader = new StringBuilder("ENC_800180553_");
            sheader.Append(DateTime.Now.ToString("yyyyMMdd"));
            sheader.Append(".TXT");
            paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, sheader.ToString() };
            string sheaderfile = Path.Combine(paths);
            using (var zipArchive = new ZipFile())
            {
                if (File.Exists(sheaderfile)) zipArchive.AddFile(sheaderfile, string.Empty);
                if (File.Exists(sdetailfile)) zipArchive.AddFile(sdetailfile, string.Empty);
                zipArchive.Save(spath);
            }
        }
        public void CompressFiles()
        {
            string sfile = string.Empty;
            StringBuilder zipfile = new StringBuilder("SOPORTES");
            zipfile.Append(this.sRelation);
            string zipDirectory = zipfile.ToString();
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, zipfile.ToString() };
            string spath = Path.Combine(paths);
            if (!Directory.Exists(spath))
            {
                Directory.CreateDirectory(spath);
            }
            zipfile.Append(".zip");
            paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, zipfile.ToString() };
            string zipresult = Path.Combine(paths);
            if (File.Exists(zipresult))
            {
                File.Delete(zipresult);
            }
            //using (var zipArchive = ZipFile.Open(zipresult, ZipArchiveMode.Create))
            {
                foreach (var item in this.lFiles.Distinct())
                {
                    if (File.Exists(item))
                    {
                        sfile = Path.Combine(spath, this.GetFileName(item));
                        if (!File.Exists(sfile))
                        {
                            sfile = sfile.Replace("FE", "SETT");
                            File.Copy(item, sfile);
                        }

                        if (sfile.Contains(".jpg"))
                        {
                            this.CreatePDFSupport(spath, sfile);
                            File.Delete(sfile);
                        }
                    }
                }
            }
            using (var zip = new ZipFile())
            {
                zip.AddDirectory(spath, zipDirectory);
                zip.Save(zipresult);
            }
        }

        private void CreatePDFSupport(string spath, string sfile)
        {
            try
            {
                string[] paths = new string[] { spath, Path.GetFileNameWithoutExtension(sfile) + ".pdf" };
                string pdffile = Path.Combine(paths);
                FileStream fs = new FileStream(pdffile, FileMode.Create);
                Document document = new Document();
                PdfWriter.GetInstance(document, fs);
                document.Open();
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(sfile);
                img.SetAbsolutePosition(0, 0); // set the position to bottom left corner of pdf
                img.ScaleAbsolute(iTextSharp.text.PageSize.LETTER.Width, iTextSharp.text.PageSize.LETTER.Height);
                document.Add(img);
                document.Close();

            }
            catch (Exception ex)
            {
                LogError.WriteError("Trazabilidad", "Aplicacion", ex);
            }           
        }

        public void GenerateNotes(List<Desmaterializacion> ldesmaterializacion)
        {
            string[] scolumns = null;
            StringBuilder ssource = new StringBuilder("NC_800180553_");
            ssource.Append(DateTime.Now.ToString("yyyyMMdd"));
            ssource.Append(".TXT");
            StringBuilder stringBuilder = new StringBuilder();
            var result = ldesmaterializacion.Select
                                            (
                                                x => new
                                                {
                                                    sinviove = x.sfactura,
                                                    ssource = x.sfuentefactura,
                                                    sdate = x.sfechafactura,
                                                    stiponota = x.stiponota,
                                                    svalornota = x.svalornota,
                                                    snotadebito = x.snotadebito,
                                                    svalordebito = x.svalordebito,
                                                    dvalue = Convert.ToDecimal(x.svalorfacturado)
                                                }
                                            ).GroupBy
                                            (
                                                x => new
                                                {
                                                    x.sinviove,
                                                    x.ssource,
                                                    x.sdate,
                                                    x.svalornota,
                                                    x.stiponota,
                                                    x.svalordebito,
                                                    x.snotadebito,
                                                }
                                            ).Select
                                            (
                                                g => new
                                                {
                                                    invoice = g.Key.sinviove,
                                                    source = g.Key.ssource,
                                                    date = g.Key.sdate,
                                                    note = g.Key.stiponota,
                                                    notevalue = g.Key.svalornota,
                                                    debit = g.Key.snotadebito,
                                                    debitvalue = g.Key.svalordebito,
                                                    total = g.Sum(x => x.dvalue)
                                                }
                                            ).ToList().FindAll(y => y.note != string.Empty);
            foreach (var item in result)
            {
                if (!string.IsNullOrEmpty(item.note))
                {
                    scolumns = new string[]
                    {
                        "8",
                        "800180553",
                        item.source,
                        item.invoice,
                        item.date,
                        item.total.ToString(),
                        item.note,
                        item.notevalue
                    };
                    stringBuilder.AppendLine(string.Join("|", scolumns));
                }
                if (!string.IsNullOrEmpty(item.debit))
                {
                    scolumns = new string[]
                    {
                        "8",
                        "800180553",
                        item.source,
                        item.invoice,
                        item.date,
                        item.total.ToString(),
                        item.debit,
                        item.debitvalue
                    };
                    stringBuilder.AppendLine(string.Join("|", scolumns));
                }
            }
            if (stringBuilder.Length > 0)
            {
                this.WriteFile(stringBuilder.ToString(), ssource.ToString());
            }
        }

        public void Dispose()
        {
            lFiles = null;
            GC.SuppressFinalize(this);
            GC.Collect();            
        }
    }
}