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
    public class FacturacionFamisanar : IDisposable
    {
        public string sRelation { get; set; }
        
        public void GenerateFiles(List<Desmaterializacion> ldesmaterializacion)
        {
            FacturacionCompensar facturacionCompensar = new FacturacionCompensar();            
            string[] scolumns = null;            
            StringBuilder stringBuilder = new StringBuilder();
            List<Generic> lsupports = null;
            List<string> lTmp = new List<string>();
            string directory = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
            Directory.CreateDirectory(directory);                       
            foreach (var item in ldesmaterializacion)
            {                                
                if (lTmp.FirstOrDefault(x => x.Contains(item.sfactura)) == null)
                {
                    lTmp.Add(item.sfactura);
                    scolumns = new string[]
                    {
                        "SETT" + item.sfactura,
                        "110011025101",
                        item.stipodocumento,
                        item.sdocumento,
                        this.GetDocumentType(String.Empty),
                        item.sfuentefactura + item.sfactura + ".PDF",
                        "800180553",
                        "1"
                    };
                    stringBuilder.AppendLine(string.Join(",", scolumns));
                    this.CopyInvoice(item.sarchivo, item.sfuentefactura + item.sfactura);
                }                
                lsupports = facturacionCompensar.GetInvoiceSupports(item.sepisodio);                
                foreach (var support in lsupports)
                {                    
                    scolumns = new string[] 
                    {
                        "SETT" + item.sfactura, 
                        "110011025101", 
                        item.stipodocumento, 
                        item.sdocumento,
                        this.GetDocumentType(support.code),
                        support.code.Replace(" ", "_") + "_" + item.singreso + ".PDF", 
                        "800180553", 
                        this.GeneratePDFSupport(support.code.Replace(" ", "_") + "_" + item.singreso, support.name, support.date).ToString() 
                    };
                    stringBuilder.AppendLine(string.Join(",", scolumns));
                }
            }
            this.WriteFile(stringBuilder.ToString(), "IM" + this.sRelation + ".TXT");            
            facturacionCompensar.Dispose();
            facturacionCompensar = null;
            stringBuilder = null;
            lTmp = null;
            lsupports = null;
        }

        private void WriteFile(string scontent, string ssource)
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, ssource };
            string sfiletarget = Path.Combine(paths);                        
            File.WriteAllText(sfiletarget, scontent);            
            
        }

        private string GetDocumentType(string stype)
        {
            switch (stype)
            {
                case "Autorizacion": return "3";
                case "Orden Medica": return "5";
                case "Soporte Clinico": return "13";
                case "Detalle de Factura": return "1";
                default: return "2";
            }
        }      
        
        private void CopyInvoice(string sinvoicefile, string sinvoice)
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, sinvoice + ".PDF" };
            string[] invoicepath = new string[] { Configuration.GetStringValue("InvoicesPath"), sinvoicefile + ".pdf" };
            if (File.Exists(Path.Combine(invoicepath)))
            {
                File.Copy(Path.Combine(invoicepath), Path.Combine(paths));
            }
        }

        private int GeneratePDFSupport(string stype, string ssupport, DateTime date)
        {
            int ipages = 0;
            FileStream fs = null;
            List<string> lsupports = new List<string>();
            Document document = new Document();            
            string[] foldersupport = new string[] { Configuration.GetStringValue("SupportsPath"), date.Year.ToString(), date.Month.ToString(), date.Day.ToString() };
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, stype + ".PDF"  };            
            string pdffile = Path.Combine(paths);
            lsupports = this.SearchFile(ssupport + "*.jpg", Path.Combine(foldersupport), lsupports);
            try
            {
                fs = new FileStream(pdffile, FileMode.Create);
                PdfWriter.GetInstance(document, fs);
                document.Open();
                foreach (string item in lsupports)
                {
                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(item);
                    img.SetAbsolutePosition(0, 0); // set the position to bottom left corner of pdf
                    document.SetPageSize(PageSize.LETTER);
                    img.ScaleToFit(PageSize.LETTER.Width, PageSize.LETTER.Height);
                    //img.ScaleAbsolute(iTextSharp.text.PageSize.LETTER.Width, iTextSharp.text.PageSize.LETTER.Height);
                    img.SetDpi(300, 300);
                    document.Add(img);
                    document.NewPage();
                    ipages++;
                }
                document.Close();
                return ipages;
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Facturacion", ex);
                return 1;
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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}