using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text;
using System.IO;
using System.Collections;
using Facade;
using Entity;
using EventLog;
using Config;

namespace Trazabilidad
{
    public partial class GenerarReporte : System.Web.UI.Page
    {
        /// <summary>
        /// 
        /// </summary>
        private int iComment
        {
            get { return (ViewState["iComment"] != null) ? Convert.ToInt32(ViewState["iComment"]) : 0; }
            set { ViewState["iComment"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["iComment"] != null)
            {
                this.iComment = Convert.ToInt32(Request.QueryString["iComment"]);
                this.CreatePDF();
            }
        }

        /// <summary>
        /// Método que genera el PDF de respuesta de la glosa
        /// </summary>
        private void CreatePDF()
        {
            MemoryStream outputStream = new MemoryStream();
            Glosa oGlosa = null;
            Invoice oFactura = null;
            FacadeGlosa oFacade = null;
            FacadeFactura oFacadeFactura = null;
            string sFile = string.Empty;            
            decimal dAceptado = 0, dGlosa = 0;
            try
            {
                oGlosa = new Glosa() { id = this.iComment };
                oFacadeFactura = new FacadeFactura(Configuration.GetStringValue("FNCFacturacion"));
                oFacade = new FacadeGlosa();
                oFacade.sConnection = Configuration.GetStringValue("FNCFacturacion");
                oGlosa = oFacade.GetComment(oGlosa);
                oFactura = new Invoice() { invoice = oGlosa.invoice };
                oFactura = oFacadeFactura.GetList(oFactura)[0];
                if (oGlosa.lConcept != null)
                {                    
                    Document oDocument = new Document();
                    Font fntNormal = FontFactory.GetFont(FontFactory.COURIER, 8);
                    Font fntBold = FontFactory.GetFont(FontFactory.COURIER_BOLD, 8);
                    iTextSharp.text.Image oImage = null;
                    oDocument.SetPageSize(iTextSharp.text.PageSize.LETTER);
                    PdfWriter.GetInstance(oDocument, outputStream);
                    oDocument.Open();
                    oImage = iTextSharp.text.Image.GetInstance(Configuration.GetStringValue("ImageSource"));
                    oImage.Alignment = 6;
                    oDocument.Add(oImage);
                    oDocument.Add(new Paragraph("Bogotá," + DateTime.Now.ToString("dd/MM/yyyy"), fntNormal));
                    oDocument.Add(new Paragraph("Señores", fntNormal));
                    oDocument.Add(new Paragraph(oFactura.eps, fntBold));
                    oDocument.Add(new Paragraph("Ciudad", fntNormal));
                    oDocument.Add(new Paragraph("Estimados señores:", fntNormal));
                    oDocument.Add(Chunk.NEWLINE);
                    oDocument.Add(new Paragraph("A continuación damos respuesta a la notificación de glosa recibida.", fntNormal));
                    oDocument.Add(Chunk.NEWLINE);
                    string[] asHeader = new string[] { "FACTURA", "CONCEPTO", "OBSERVACION", "RESPUESTA", "ANALISIS", "V. NETO FACTURA", "V. GLOSA", "V. NO ACEPTADO", "V. ACEPTADO" };
                    PdfPTable pdfTable = new PdfPTable(asHeader.Length);
                    pdfTable.WidthPercentage = 100;
                    PdfPCell pdfCell = null;
                    for (int i = 0; i < asHeader.Length; i++)
                    {
                        pdfTable.AddCell(new PdfPCell(new Paragraph(asHeader[i], fntBold)));
                    }
                    for (int i = 0; i < oGlosa.lConcept.Count; i++)
                    {
                        pdfTable.AddCell(new PdfPCell(new Paragraph(oGlosa.invoice, fntNormal)));
                        pdfTable.AddCell(new PdfPCell(new Paragraph(oGlosa.lConcept[i].conceptcode + ". " + oGlosa.lConcept[i].conceptname, fntNormal)));
                        pdfTable.AddCell(new PdfPCell(new Paragraph(oGlosa.lConcept[i].conceptobservations, fntNormal)));
                        pdfTable.AddCell(new PdfPCell(new Paragraph(oGlosa.lConcept[i].oResponse.responsecode + ". " + oGlosa.lConcept[i].oResponse.responsename, fntNormal)));
                        pdfTable.AddCell(new PdfPCell(new Paragraph(oGlosa.lConcept[i].oResponse.observations, fntNormal)));
                        pdfCell = new PdfPCell(new Paragraph(oFactura.value.ToString("C"), fntNormal));
                        pdfCell.HorizontalAlignment = 2;
                        pdfCell.NoWrap = true;
                        pdfTable.AddCell(pdfCell);
                        pdfCell = new PdfPCell(new Paragraph(oGlosa.lConcept[i].conceptvalue.ToString("C"), fntNormal));
                        pdfCell.NoWrap = true;
                        pdfCell.HorizontalAlignment = 2;
                        pdfTable.AddCell(pdfCell);
                        pdfCell = new PdfPCell(new Paragraph((oGlosa.lConcept[i].conceptvalue - oGlosa.lConcept[i].oResponse.acceptedvalue).ToString("C"), fntNormal));
                        pdfCell.NoWrap = true;
                        pdfCell.HorizontalAlignment = 2;
                        pdfTable.AddCell(pdfCell);
                        pdfCell = new PdfPCell(new Paragraph(oGlosa.lConcept[i].oResponse.acceptedvalue.ToString("C"), fntNormal));
                        pdfCell.NoWrap = true;
                        pdfCell.HorizontalAlignment = 2;
                        pdfTable.AddCell(pdfCell);
                        dAceptado += oGlosa.lConcept[i].oResponse.acceptedvalue;
                        dGlosa += oGlosa.lConcept[i].conceptvalue;
                    }
                    pdfCell = new PdfPCell(new Paragraph("TOTAL", fntBold));
                    pdfCell.Colspan = 5;
                    pdfCell.HorizontalAlignment = 1;
                    pdfTable.AddCell(pdfCell);
                    pdfCell = new PdfPCell(new Paragraph(oFactura.value.ToString("C"), fntNormal));
                    pdfCell.HorizontalAlignment = 2;
                    pdfCell.NoWrap = true;
                    pdfTable.AddCell(pdfCell);
                    pdfCell = new PdfPCell(new Paragraph(dGlosa.ToString("C"), fntNormal));
                    pdfCell.HorizontalAlignment = 2;
                    pdfCell.NoWrap = true;
                    pdfTable.AddCell(pdfCell);
                    pdfCell = new PdfPCell(new Paragraph((dGlosa - dAceptado).ToString("C"), fntNormal));
                    pdfCell.HorizontalAlignment = 2;
                    pdfCell.NoWrap = true;
                    pdfTable.AddCell(pdfCell);
                    pdfCell = new PdfPCell(new Paragraph(dAceptado.ToString("C"), fntNormal));
                    pdfCell.HorizontalAlignment = 2;
                    pdfCell.NoWrap = true;
                    pdfTable.AddCell(pdfCell);
                    oDocument.Add(pdfTable);
                    oDocument.Add(Chunk.NEWLINE);
                    oDocument.Add(new Paragraph("De antemano agradecemos que ante cualquier inquietud se comunique al telefono 7428900 ext 2410-2400", fntNormal));
                    oDocument.Add(Chunk.NEWLINE);
                    oDocument.Add(new Paragraph("Cordialmente,", fntNormal));
                    oDocument.Add(Chunk.NEWLINE);
                    oDocument.Add(Chunk.NEWLINE);
                    oDocument.Add(Chunk.NEWLINE);
                    pdfTable = new PdfPTable(2);
                    pdfTable.SetWidths(new float[] { 2f, 2f });
                    pdfCell = new PdfPCell(new Paragraph("LINA MARIA GARZON AUZA", fntBold));
                    pdfCell.Border = 0;
                    pdfTable.AddCell(pdfCell);
                    pdfCell = new PdfPCell(new Paragraph(" ", fntBold));
                    pdfCell.Border = 0;
                    pdfTable.AddCell(pdfCell);
                    pdfCell = new PdfPCell(new Paragraph("COORDINADORA DE FACTURACION", fntBold));
                    pdfCell.Border = 0;
                    pdfTable.AddCell(pdfCell);
                    pdfCell = new PdfPCell(new Paragraph("AUXILIAR DE FACTURACION", fntBold));
                    pdfCell.Border = 0;
                    pdfTable.AddCell(pdfCell);
                    oDocument.Add(pdfTable);
                    oDocument.Close();
                    sFile = "RespuestaGlosa" + oGlosa.invoice + ".pdf";
                }
            }
            catch (Exception ex)
            {
                LogError.WriteError("Facturacion", "Aplicacion", ex);
                throw;
            }
            finally
            {
                oFacade.Dispose();
                oFacade = null;
                oGlosa = null;
                oFacadeFactura.Dispose();
                oFacadeFactura = null;
            }
            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("Expires", "0");
            Response.AddHeader("Cache-Control", "");
            Response.AddHeader("Content-Disposition", "attachment; filename=" + sFile);
            Response.AddHeader("Content-length", outputStream.GetBuffer().Length.ToString());
            Response.OutputStream.Write(outputStream.GetBuffer(), 0, outputStream.GetBuffer().Length);
            Response.End();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetConceptsString(bool isResponse, List<ConceptoGlosa> lConcept)
        {
            string[] aConcept = null;
            List<ConceptoGlosa> lConcepto = null;
            if (lConcept != null)
            {
                lConcepto = (isResponse) ? lConcept.FindAll(x => x.conceptgroup == "9") : lConcept.FindAll(x => x.conceptgroup != "9");
                aConcept = new string[lConcepto.Count];
                for (int i = 0; i < lConcepto.Count; i++)
                {
                    aConcept[i] = lConcepto[i].conceptcode;
                }
            }
            return String.Join(",", aConcept);
        }

        /// <summary>
        /// 
        /// </summary>        
        /// <param name="sFile"></param>
        /*private void GeneratePDF(string sFile)
        {
            MemoryStream outputStream = new MemoryStream();
            Document oDocument = new Document();
            iTextSharp.text.Image oImage = null;
            oDocument.SetPageSize(iTextSharp.text.PageSize.LETTER);            
            PdfWriter.GetInstance(oDocument, outputStream);            
            oDocument.Open();
            ArrayList htmlarraylist = iTextSharp.text.html.simpleparser.HTMLWorker.ParseToList(new StringReader(sHTML), null);
            oImage = iTextSharp.text.Image.GetInstance(Configuration.GetStringValue("ImageSource"));
            oImage.Alignment = 6;
            oDocument.Add(oImage);            
            Paragraph mypara = new Paragraph();
            mypara.Font.Size = 6;
            mypara.Alignment = 36;
            mypara.InsertRange(0, htmlarraylist);
            oDocument.Add(mypara);            
            oImage = null;
            oDocument.Close();
           
        }*/
    }
}