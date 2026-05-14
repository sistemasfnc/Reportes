using Config;
using Entity;
using EventLog;
using Facade;
using FluentFTP;
using FluentFTP.Helpers;
using FNCUtils;
using Ionic.Zip;
using iText.Kernel.Utils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Web;
using System.Web.DynamicData;
using System.Xml.Linq;
using Utils;
using static iTextSharp.text.pdf.AcroFields;

namespace Trazabilidad.clases
{
    public class Facturacion2885 : IDisposable
    {

        public string sRelation { get; set; }

        public string stype { get; set; }

        public List<string> listfiles { get; set; }

        public string scompany { get; set; }

        private bool bisfaminasar { get; set; }

        private bool bunifiedsupport { get; set; }

        private bool bfileinzip { get; set; }

        private string sprocessid { get; set; }

        private bool bisbolivar { get; set; }

        private bool bisallianz { get; set; }

        private StringBuilder lerror { get; set; }

        private StringBuilder lcontrol { get; set; }

        public Facturacion2885()
        {
            //this.listfiles = new List<string>();
            this.bisfaminasar = false;
            this.bunifiedsupport = false;
            this.bfileinzip = true;
            this.listfiles = new List<string>();
            this.bisbolivar = this.bisallianz = false;
            this.sprocessid = string.Empty;
            this.lerror = new StringBuilder();
            this.lcontrol = new StringBuilder();
        }

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
                sfilename.Append(this.GetInvoiceName(item.invoice));
                sfilename.Append(".PDF");
                this.ProccessFiles(item.invoice, item.file, sfilename.ToString(), ldesmaterializacion);
                sfilename.Clear();
            }
            if (this.scompany.EqualsOfAny("44"))
            {
                this.listfiles.AddRange(Directory.GetFiles(directory, "SETT*.zip"));
                this.CompressFiles("RIPS");
            }
            else if (this.scompany.EqualsOfAny("114"))
            {
                CompressFoldersToZip(Path.Combine(directory, "SOPORTES"), directory, "SOPORTES");
                Directory.Delete(Path.Combine(directory, "SOPORTES"), true);
                //this.RemoveDirectories(Path.Combine(directory, "SOPORTES"), "SETT*");                
            }
            this.RemoveFiles();
            this.listfiles.Clear();
            this.GenerateErrorFile(directory);
            if (this.lcontrol.Length > 0)
            {
                this.GenerateControlFile(directory);
            }
        }

        private void GenerateControlFile(string directory)
        {
            string sfile = Path.Combine(directory, "control.txt");
            if (!File.Exists(sfile))
            {
                File.WriteAllText(sfile, this.lcontrol.ToString());
            }
            else
            {
                File.AppendAllText(sfile, this.lcontrol.ToString());
            }
        }

        private void RemoveDirectories(string directory, string pattern)
        {
            string[] folders = Directory.GetDirectories(directory, pattern);
            foreach (string folder in folders)
            {
                Directory.Delete(folder, true);
            }
        }

        public void CompressFoldersToZip(string sourceDirectory, string outputDirectory, string szipname, bool bfolders = true)
        {
            string zipFilePath = Path.Combine(outputDirectory, $"{szipname}.zip");
            using (ZipFile zip = new ZipFile())
            {
                if (bfolders)
                {
                    string[] folders = Directory.GetDirectories(sourceDirectory, "SETT*");
                    foreach (string folder in folders)
                    {
                        string folderName = Path.GetFileName(folder);
                        zip.AddDirectory(folder, folderName);
                    }
                }
                else
                {
                    string[] files = Directory.GetFiles(sourceDirectory);
                    foreach (string file in files)
                    {
                        zip.AddFile(file, string.Empty);
                    }
                }
                zip.Save(zipFilePath);
            }
        }

        private string GetInvoiceName(string sinvoice)
        {
            if (scompany.EqualsAnyOf("21", "260")) //Compensar
            {
                return $"FV_800180553_SETT{sinvoice}";
            }
            else if (scompany.EqualsAnyOf("19")) //Bolivar
            {
                return $"FAT_800180553_SETT{sinvoice}";
            }
            else if (scompany.EqualsAnyOf("83", "53")) //Sura, suramericana
            {
                return $"800180553_SETT_{sinvoice}";
            }
            else if (scompany.EqualsAnyOf("44")) //Positiva
            {
                return $"FAC_800180553_SETT{sinvoice}";
            }
            else if (scompany.EqualsAnyOf("25", "171", "09", "50", "02", "17", "23", "01", "16", "114", "255", "261")) //Sanitas, Viva 1, capital salud, Salud Total, Allianz, Colmena, Coomeva, Aliansalud, colmedica, Ecopetrol, Armada
            {
                return $"FEV_800180553_SETT{sinvoice}";
            }
            else if (scompany.EqualsAnyOf("04", "05", "215", "39", "18", "112", "40")) //Axa Colpatria, Medisanitas, Colsanitas, Policia, Medplus
            {
                return $"SETT{sinvoice}";
            }
            else if (scompany.EqualsAnyOf("64")) //Nueva EPS
            {
                return $"FEV_800180553_SETT{sinvoice}";
            }
            else if (scompany.EqualsAnyOf("36"))
            {
                return $"FEV_800180553_SETT{sinvoice}";
            }
            else //El resto
            {
                return $"FEV_800180553_SETT_{sinvoice}";
            }
        }

        private void ProcessPolicia(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            string scuvsdir = Path.Combine(directory, "CUV");
            if (!Directory.Exists(scuvsdir))
            {
                Directory.CreateDirectory(scuvsdir);
            }
            string sripsdir = Path.Combine(directory, "RIPS");
            if (!Directory.Exists(sripsdir))
            {
                Directory.CreateDirectory(sripsdir);
            }
            string sxmldir = Path.Combine(directory, "XML");
            if (!Directory.Exists(sxmldir))
            {
                Directory.CreateDirectory(sxmldir);
            }
            string ssuportpath = Path.Combine(directory, $"SETT{sinvoice}");
            if (!Directory.Exists(ssuportpath))
            {
                Directory.CreateDirectory(ssuportpath);
            }
            this.CopyExtraFiles(sinvoice, scuvsdir, $"CUV_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", $"RIPS_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", $"XML_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", false, sripsdir, false, sxmldir);
            this.bfileinzip = false;
            this.GenerateInvoiceFile(sinvoicepath, Path.Combine(directory, sinvoicename), false);
            this.GenerateSupports(ldesmaterializacion, sinvoice, ssuportpath);
        }
        private void ProcessEcopetrol(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            string sjsondir = Path.Combine(directory, "RIPS");
            if (!Directory.Exists(sjsondir))
            {
                Directory.CreateDirectory(sjsondir);
            }
            this.CopyExtraFiles(sinvoice, sjsondir, $"CUV_SETT{sinvoice}", $"RIPS_SETT{sinvoice}", $"FEV_SETT{sinvoice}", true);
            this.CompressFiles($"SETT{sinvoice}", sjsondir);
            this.RemoveFiles();
            this.listfiles.Clear();
            string[] paths = new string[] { directory, $"SOPORTES" };
            string ssupportdir = Path.Combine(paths);
            if (!Directory.Exists(ssupportdir))
            {
                Directory.CreateDirectory(ssupportdir);
            }
            ssupportdir = Path.Combine(ssupportdir, $"SETT{sinvoice}");
            if (!Directory.Exists(ssupportdir))
            {
                Directory.CreateDirectory(ssupportdir);
            }
            this.bfileinzip = false;
            this.GenerateInvoiceFile(sinvoicepath, Path.Combine(ssupportdir, sinvoicename), false);
            this.GenerateSupports(ldesmaterializacion, sinvoice, ssupportdir);
            //this.GenerateSanitasSupports(ldesmaterializacion, sinvoice, ssupportdir, true);
        }

        private void ProcessArmada(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            this.GenerateInvoiceFile(sinvoicepath, Path.Combine(directory, sinvoicename), true);
            this.CopyExtraFiles(sinvoice, directory, $"CUV_800180553_SETT{sinvoice}", $"RIPS_800180553_SETT{sinvoice}", $"XML_800180553_SETT{sinvoice}", true);
            //this.CompressFiles($"SETT{sinvoice}");
            //this.RemoveFiles();
            this.listfiles.Clear();
            this.bfileinzip = false;
            this.GenerateSupports(ldesmaterializacion, sinvoice);
        }

        private void ProcessCompensar(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            //string[] paths = new string[] { directory, "SOPORTES", this.sRelation };
            string[] paths = new string[] { directory, "SOPORTES" };
            string ssupportdir = Path.Combine(paths);
            string sjsondir = Path.Combine(directory, "JSON");
            if (!Directory.Exists(ssupportdir))
            {
                Directory.CreateDirectory(ssupportdir);
            }
            if (!Directory.Exists(sjsondir))
            {
                Directory.CreateDirectory(sjsondir);
            }
            this.CopyExtraFiles(sinvoice, sjsondir, $"CUV_SETT{sinvoice}", $"RIPS_SETT{sinvoice}", $"FEV_SETT{sinvoice}", true);
            this.CompressFiles($"SETT{sinvoice}", sjsondir);
            this.RemoveFiles();
            this.listfiles.Clear();
            //this.listfiles.AddRange(Directory.GetFiles(sjsondir, $"SETT{sinvoice}.zip"));
            //this.CompressFiles(this.sRelation, sjsondir, true);
            //this.RemoveFiles();
            //this.listfiles.Clear();
            this.GenerateInvoiceFile(sinvoicepath, Path.Combine(ssupportdir, sinvoicename));
            this.GenerateSupports(ldesmaterializacion, sinvoice, ssupportdir);
            //this.CompressFiles(this.sRelation, ssupportdir, true, true);
            this.CompressFiles($"SETT{sinvoice}", ssupportdir, true, false);
            this.RemoveFiles();
            this.listfiles.Clear();
        }

        private void ProcessBolivar(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            this.bisbolivar = true;
            this.CopyExtraFiles(sinvoice, directory, $"800180553_SETT{sinvoice}_cuv", $"FE-SETT{sinvoice}-01", string.Empty, true, string.Empty, true);
            this.GenerateInvoiceFile(sinvoicepath, Path.Combine(directory, sinvoicename));
            this.GenerateSupports(ldesmaterializacion, sinvoice);
            this.CompressFiles($"FAC_SETT{sinvoice}");
            this.RemoveFiles();
            this.listfiles.Clear();
        }

        private void ProcessSanitas(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            try
            {
                this.GenerateInvoiceFile(sinvoicepath, Path.Combine(directory, sinvoicename));
                bool tieneMultiplesDocumentos = ldesmaterializacion.Where(x => x.sfactura == sinvoice).Select(x => x.sdocumento).Distinct().Count() > 1;
                if (tieneMultiplesDocumentos)
                {
                    this.GenerateSanitasSupports(ldesmaterializacion, sinvoice, directory);
                }
                else
                {
                    this.GenerateSupports(ldesmaterializacion, sinvoice, directory);
                }
                this.CompressFiles($"800180553_SETT{sinvoice}");
                this.RemoveFiles();
                this.listfiles.Clear();
                string ssupportdir = Path.Combine(directory, "RIPS");
                string sjsondir = Path.Combine(directory, "CUV");
                if (!Directory.Exists(ssupportdir))
                {
                    Directory.CreateDirectory(ssupportdir);
                }
                if (!Directory.Exists(sjsondir))
                {
                    Directory.CreateDirectory(sjsondir);
                }
                //this.CopyExtraFiles(sinvoice, sjsondir, $"CUV_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", $"RIPS_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", string.Empty, false, ssupportdir);
                this.CopyExtraFiles(sinvoice, sjsondir, $"CUV_SETT{sinvoice}", $"RIPS_SETT{sinvoice}", string.Empty, false, ssupportdir);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Trazabilidad", "Trazabilidad", ex);
                throw;
            }
        }

        private void ProcessCoomeva(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            this.GenerateInvoiceFile(sinvoicepath, Path.Combine(directory, sinvoicename));
            this.GenerateSupports(ldesmaterializacion, sinvoice, directory);
            this.CompressFiles($"800180553_SETT{sinvoice}");
            string ssupportdir = Path.Combine(directory, "RIPS");
            string sjsondir = Path.Combine(directory, "CUV");
            if (!Directory.Exists(ssupportdir))
            {
                Directory.CreateDirectory(ssupportdir);
            }
            if (!Directory.Exists(sjsondir))
            {
                Directory.CreateDirectory(sjsondir);
            }
            this.SetProcessId(sinvoice);
            this.CopyExtraFiles(sinvoice, sjsondir, $"ResultadosMSPS_SETT{sinvoice}_ID{this.sprocessid}_A_CUV", string.Empty, string.Empty, false, ssupportdir, true);
            this.RemoveFiles();
            this.listfiles.Clear();
        }

        private void ProcessSura(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            this.GenerateInvoiceFile(sinvoicepath, Path.Combine(directory, sinvoicename), false);
            this.GenerateSupports(ldesmaterializacion, sinvoice, directory);
            this.CompressFiles($"800180553_SETT_{sinvoice}", string.Empty, false, true);
            string ssupportdir = Path.Combine(directory, $"800180553_SETT_{sinvoice}");
            if (!Directory.Exists(ssupportdir))
            {
                Directory.CreateDirectory(ssupportdir);
            }
            this.CopyExtraFiles(sinvoice, ssupportdir, $"800180553_SETT{sinvoice}-CUV", $"800180553_SETT{sinvoice}", string.Empty, false, string.Empty);
            this.RemoveFiles();
            this.listfiles.Clear();
        }

        private void ProcessFamisanar(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            this.bisfaminasar = true;
            this.bisbolivar = true;
            this.GenerateInvoiceFile(sinvoicepath, Path.Combine(directory, sinvoicename));
            this.GenerateSupports(ldesmaterializacion, sinvoice, directory);
            this.CopyExtraFiles(sinvoice, directory, $"800180553_SETT{sinvoice}_cuv", $"800180553_SETT{sinvoice}", string.Empty, true, string.Empty);
            this.CompressFiles($"800180553_SETT{sinvoice}");
            this.RemoveFiles();
            this.listfiles.Clear();
        }

        private void ProcessAlianzColmena(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            this.bunifiedsupport = true;
            this.bfileinzip = false;
            string ssupportdir = Path.Combine(directory, $"SOPORTES");
            string sinvoicedir = Path.Combine(directory, $"FACTURAS");
            if (!Directory.Exists(ssupportdir))
            {
                Directory.CreateDirectory(ssupportdir);
            }
            if (!Directory.Exists(sinvoicedir))
            {
                Directory.CreateDirectory(sinvoicedir);
            }
            this.GenerateInvoiceFile(sinvoicepath, Path.Combine(sinvoicedir, sinvoicename), false);
            this.GenerateSupports(ldesmaterializacion, sinvoice, ssupportdir);
            ssupportdir = Path.Combine(directory, "RIPS");
            string sjsondir = Path.Combine(directory, "CUV");
            if (!Directory.Exists(ssupportdir))
            {
                Directory.CreateDirectory(ssupportdir);
            }
            if (!Directory.Exists(sjsondir))
            {
                Directory.CreateDirectory(sjsondir);
            }
            this.SetProcessId(sinvoice);
            this.CopyExtraFiles(sinvoice, sjsondir, $"ResultadosMSPS_SETT{sinvoice}_ID{this.sprocessid}_A_CUV", $"FE-{sinvoice}-01", string.Empty, false, ssupportdir, true);
        }

        private void ProcessAlianz(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            this.bisallianz = true;
            this.bisbolivar = true;
            //this.bunifiedsupport = true;
            this.bfileinzip = false;
            string ssupportdir = Path.Combine(directory, "SOPORTES");
            string sinvoicedir = Path.Combine(directory, "FACTURAS");
            if (!Directory.Exists(ssupportdir))
            {
                Directory.CreateDirectory(ssupportdir);
            }
            if (!Directory.Exists(sinvoicedir))
            {
                Directory.CreateDirectory(sinvoicedir);
            }
            this.GenerateInvoiceFile(sinvoicepath, Path.Combine(sinvoicedir, sinvoicename), false);
            this.GenerateSupports(ldesmaterializacion, sinvoice, ssupportdir);
            ssupportdir = Path.Combine(directory, "RIPS");
            string sjsondir = Path.Combine(directory, "CUV");
            if (!Directory.Exists(ssupportdir))
            {
                Directory.CreateDirectory(ssupportdir);
            }
            if (!Directory.Exists(sjsondir))
            {
                Directory.CreateDirectory(sjsondir);
            }
            this.SetProcessId(sinvoice);
            this.CopyExtraFiles(sinvoice, sjsondir, $"800180553_SETT{sinvoice}_cuv", $"800180553_SETT{sinvoice}_rips", string.Empty, false, ssupportdir, true);
        }


        private void ProcessAxaColpatria(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            string sinvoicefolder = Path.Combine(directory, "FACTURAS");
            try
            {
                if (!Directory.Exists(sinvoicefolder))
                {
                    Directory.CreateDirectory(sinvoicefolder);
                }
                this.GenerateInvoiceFile(sinvoicepath, Path.Combine(sinvoicefolder, sinvoicename), false);
                string ssuportpath = Path.Combine(directory, "SOPORTES");
                if (!Directory.Exists(ssuportpath))
                {
                    Directory.CreateDirectory(ssuportpath);
                }
                this.bfileinzip = false;
                this.GenerateSupports(ldesmaterializacion, sinvoice, ssuportpath);
                string ssupportdir = Path.Combine(directory, "RIPS");
                string sjsondir = Path.Combine(directory, "CUV");
                if (!Directory.Exists(ssupportdir))
                {
                    Directory.CreateDirectory(ssupportdir);
                }
                if (!Directory.Exists(sjsondir))
                {
                    Directory.CreateDirectory(sjsondir);
                }
                this.SetProcessId(sinvoice);
                this.CopyExtraFiles(sinvoice, sjsondir, $"ResultadosMSPS_SETT{sinvoice}_ID{this.sprocessid}_A_CUV", $"SETT{sinvoice}_RIPS", string.Empty, false, ssupportdir);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Trazabilidad", "Trazabilidad", ex);
                throw;
            }

        }

        private void ProcessPositiva(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            this.bfileinzip = true;
            this.GenerateInvoiceFile(sinvoicepath, Path.Combine(directory, sinvoicename), true);
            this.GenerateSupports(ldesmaterializacion, sinvoice, directory);
            this.CompressFiles($"SOPORTES{this.sRelation}", string.Empty, true);
            this.RemoveFiles();
            this.listfiles.Clear();
            //string ssuportpath = Path.Combine(directory, $"SETT{sinvoice}");
            /*if (!Directory.Exists(ssuportpath))
            {
                Directory.CreateDirectory(ssuportpath);
            }*/
            this.CopyExtraFiles(sinvoice, directory, string.Empty, string.Empty, string.Empty, true, string.Empty, true);
            this.CompressFiles($"SETT{sinvoice}");
            this.RemoveFiles();
            this.listfiles.Clear();
        }

        private void ProcessColmedica(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            this.bfileinzip = false;
            string ssupportdir = Path.Combine(directory, "RIPS");
            if (!Directory.Exists(ssupportdir))
            {
                Directory.CreateDirectory(ssupportdir);
            }
            this.CopyExtraFiles(sinvoice, ssupportdir, $"CUV_SETT{sinvoice}", $"RIPS_SETT{sinvoice}", $"FEV_SETT{sinvoice}");
            string sinvoicedir = Path.Combine(directory, "FACTURAS");
            if (!Directory.Exists(sinvoicedir))
            {
                Directory.CreateDirectory(sinvoicedir);
            }
            this.GenerateInvoiceFile(sinvoicepath, Path.Combine(sinvoicedir, sinvoicename), false);
            this.bfileinzip = true;
            this.GenerateSupports(ldesmaterializacion, sinvoice);
            this.CompressFiles($"800180553_SETT{sinvoice}");
            this.RemoveFiles();
            this.listfiles.Clear();
        }

        private void ProcessAliansalud(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            this.ProcessColmedica(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
        }

        private void ProcessSanitasPrepagada(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            this.bfileinzip = false;
            //this.bunifiedsupport = true;
            string sinvoicefolder = Path.Combine(directory, "FACTURAS");
            if (!Directory.Exists(sinvoicefolder))
            {
                Directory.CreateDirectory(sinvoicefolder);
            }
            try
            {
                this.GenerateInvoiceFile(sinvoicepath, Path.Combine(sinvoicefolder, sinvoicename), false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            string ssuportpath = Path.Combine(directory, "SOPORTES");
            if (!Directory.Exists(ssuportpath))
            {
                Directory.CreateDirectory(ssuportpath);
            }
            //this.GenerateSupports(ldesmaterializacion, sinvoice, ssuportpath);
            try
            {
                this.GenerateSanitasSupports(ldesmaterializacion, sinvoice, ssuportpath, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            string sjsondir = Path.Combine(directory, "CUV");
            if (!Directory.Exists(sjsondir))
            {
                Directory.CreateDirectory(sjsondir);
            }
            string sripsdir = Path.Combine(directory, "RIPS");
            if (!Directory.Exists(sripsdir))
            {
                Directory.CreateDirectory(sripsdir);
            }
            try
            {
                this.SetProcessId(sinvoice);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            try
            {
                this.CopyExtraFiles(sinvoice, sjsondir, $"ResultadosMSPS_SETT{sinvoice}_ID{this.sprocessid}_A_cuv", $"SETT{sinvoice}_RIPS", string.Empty, false, sripsdir);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ProcessCapitalViva(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            this.bfileinzip = false;
            this.GenerateInvoiceFile(sinvoicepath, Path.Combine(directory, sinvoicename), false);
            this.GenerateSupports(ldesmaterializacion, sinvoice, directory);
            //string ssuportpath = Path.Combine(directory, "RIPS");
            this.CopyExtraFiles(sinvoice, directory, string.Empty, string.Empty, string.Empty, false);
        }

        private void ProcessNuevaEPS(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            this.bfileinzip = false;
            string ssuporpath = Path.Combine(directory, "SOPORTES");
            string sripspath = Path.Combine(directory, "RIPS");
            string sxmlpath = Path.Combine(directory, "XML");
            string scuvpath = Path.Combine(directory, "CUV");
            if (!Directory.Exists(ssuporpath))
            {
                Directory.CreateDirectory(ssuporpath);
            }
            if (!Directory.Exists(sripspath))
            {
                Directory.CreateDirectory(sripspath);
            }
            if (!Directory.Exists(sxmlpath))
            {
                Directory.CreateDirectory(sxmlpath);
            }
            if (!Directory.Exists(scuvpath))
            {
                Directory.CreateDirectory(scuvpath);
            }
            try
            {
                this.GenerateSupports(ldesmaterializacion, sinvoice, ssuporpath);
                this.GenerateInvoiceFile(sinvoicepath, Path.Combine(ssuporpath, sinvoicename), false);
                this.CopyExtraFiles(sinvoice, scuvpath, $"SETT{sinvoice}_CUV", $"SETT{sinvoice}", $"XML_SETT{sinvoice}", false, sripspath, false, sxmlpath);
            }
            catch (Exception ex)
            {

                LogError.WriteError("Trazabilidad", "Trazabilidad", ex);
                throw;
            }
        }

        private void ProcessSaludTotal(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            this.bfileinzip = false;
            this.GenerateInvoiceFile(sinvoicepath, Path.Combine(directory, sinvoicename), false);
            this.GenerateSupports(ldesmaterializacion, sinvoice, directory);
            string ssupportdir = Path.Combine(directory, "RIPS");
            if (!Directory.Exists(ssupportdir))
            {
                Directory.CreateDirectory(ssupportdir);
            }
            this.CopyExtraFiles(sinvoice, ssupportdir, $"800180553_SETT{sinvoice}_CUV", $"800180553_SETT{sinvoice}_RIPS", string.Empty, false, ssupportdir);
            listfiles.AddRange(Directory.GetFiles(ssupportdir));
            this.CompressFiles($"800180553_SETT{sinvoice}");
            listfiles.Clear();
            Directory.Delete(ssupportdir, true);
        }

        private void ProcessEquidad(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            this.bfileinzip = false;
            string ssuporpath = Path.Combine(directory, "SOPORTES");
            string sinvoicespath = Path.Combine(directory, "FACTURAS");
            string sripspath = Path.Combine(directory, "RIPS");
            string sxmlpath = Path.Combine(directory, "XML");
            string scuvpath = Path.Combine(directory, "CUV");
            if (!Directory.Exists(ssuporpath))
            {
                Directory.CreateDirectory(ssuporpath);
            }
            if (!Directory.Exists(sinvoicespath))
            {
                Directory.CreateDirectory(sinvoicespath);
            }
            if (!Directory.Exists(sripspath))
            {
                Directory.CreateDirectory(sripspath);
            }
            if (!Directory.Exists(sxmlpath))
            {
                Directory.CreateDirectory(sxmlpath);
            }
            if (!Directory.Exists(scuvpath))
            {
                Directory.CreateDirectory(scuvpath);
            }
            try
            {
                this.GenerateSupports(ldesmaterializacion, sinvoice, ssuporpath);
                this.GenerateInvoiceFile(sinvoicepath, Path.Combine(sinvoicespath, sinvoicename), false);
                this.CopyExtraFiles(sinvoice, scuvpath, $"CUV_800180553_SETT{sinvoice}", $"RIPS_800180553_SETT{sinvoice}", $"XML_SETT{sinvoice}", false, sripspath, false, sxmlpath);
            }
            catch (Exception ex)
            {

                LogError.WriteError("Trazabilidad", "Trazabilidad", ex);
                throw;
            }

        }

        private void ProcessMedplus(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            string sinvoicefolder = Path.Combine(directory, "FACTURAS");
            try
            {
                if (!Directory.Exists(sinvoicefolder))
                {
                    Directory.CreateDirectory(sinvoicefolder);
                }
                this.GenerateInvoiceFile(sinvoicepath, Path.Combine(sinvoicefolder, sinvoicename), false);
                string ssuportpath = Path.Combine(directory, "SOPORTES");
                if (!Directory.Exists(ssuportpath))
                {
                    Directory.CreateDirectory(ssuportpath);
                }
                this.bfileinzip = false;
                this.GenerateSupports(ldesmaterializacion, sinvoice, ssuportpath);
                string ssupportdir = Path.Combine(directory, "RIPS");
                string sjsondir = Path.Combine(directory, "CUV");
                if (!Directory.Exists(ssupportdir))
                {
                    Directory.CreateDirectory(ssupportdir);
                }
                if (!Directory.Exists(sjsondir))
                {
                    Directory.CreateDirectory(sjsondir);
                }
                this.SetProcessId(sinvoice);
                this.CopyExtraFiles(sinvoice, sjsondir, $"ResultadosMSPS_SETT{sinvoice}_ID{this.sprocessid}_A_CUV", $"SETT{sinvoice}_RIPS", string.Empty, false, ssupportdir);
            }
            catch (Exception ex)
            {
                LogError.WriteError("Trazabilidad", "Trazabilidad", ex);
                throw;
            }
        }

        private void ProcessPanAmericanLife(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            string sinvoicefolder = Path.Combine(directory, $"SETT{sinvoice}");
            try
            {
                if (!Directory.Exists(sinvoicefolder))
                {
                    Directory.CreateDirectory(sinvoicefolder);
                }
                this.GenerateInvoiceFile(sinvoicepath, Path.Combine(sinvoicefolder, sinvoicename), false);
                this.GenerateSupports(ldesmaterializacion, sinvoice, sinvoicefolder);
                this.SetProcessId(sinvoice);
                this.CopyExtraFiles(sinvoice, sinvoicefolder, $"CUV_SETT{sinvoice}", $"RIPS_SETT{sinvoice}", $"XML_SETT{sinvoice}", false, sinvoicefolder);
                listfiles.Clear();
            }
            catch (Exception ex)
            {
                LogError.WriteError("Trazabilidad", "Trazabilidad", ex);
                throw;
            }

        }

        private void ProcessMapfre(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                this.GenerateInvoiceFile(sinvoicepath, Path.Combine(directory, sinvoicename), false);
                this.GenerateSupports(ldesmaterializacion, sinvoice, directory);
                string fev = Directory.GetFiles(directory, "FEV_*.pdf").FirstOrDefault();
                string opf = Directory.GetFiles(directory, "OPF_*.pdf").FirstOrDefault();
                string pdx = Directory.GetFiles(directory, "PDX_*.pdf").FirstOrDefault();

                var pdfs = new List<string>();
                if (!string.IsNullOrWhiteSpace(fev)) pdfs.Add(fev);
                if (!string.IsNullOrWhiteSpace(opf)) pdfs.Add(opf);
                if (!string.IsNullOrWhiteSpace(pdx)) pdfs.Add(pdx);

                if (pdfs.Count > 0)
                {
                    string mergedPdf = Path.Combine(directory, $"SETT{sinvoice}.pdf");
                    MergePdfsITextSharp(pdfs, mergedPdf);

                    foreach (var f in pdfs) File.Delete(f);
                }
                this.SetProcessId(sinvoice);
                this.CopyExtraFiles(sinvoice, directory, $"CUV_SETT{sinvoice}", $"RIPS_SETT{sinvoice}", $"XML_SETT{sinvoice}", false, directory);
                listfiles.Clear();
            }
            catch (Exception ex)
            {
                LogError.WriteError("Trazabilidad", "Trazabilidad", ex);
                throw;
            }

        }


        private void ProcessSegurosAlfa(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                this.GenerateInvoiceFile(sinvoicepath, Path.Combine(directory, sinvoicename), false);
                this.GenerateSupports(ldesmaterializacion, sinvoice, directory);
                string fev = Directory.GetFiles(directory, "FEV_*.pdf").FirstOrDefault();
                string opf = Directory.GetFiles(directory, "OPF_*.pdf").FirstOrDefault();
                string pdx = Directory.GetFiles(directory, "PDX_*.pdf").FirstOrDefault();

                var pdfs = new List<string>();
                if (!string.IsNullOrWhiteSpace(fev)) pdfs.Add(fev);
                if (!string.IsNullOrWhiteSpace(opf)) pdfs.Add(opf);
                if (!string.IsNullOrWhiteSpace(pdx)) pdfs.Add(pdx);

                if (pdfs.Count > 0)
                {
                    string mergedPdf = Path.Combine(directory, $"SETT{sinvoice}.pdf");
                    MergePdfsITextSharp(pdfs, mergedPdf);

                    foreach (var f in pdfs) File.Delete(f);
                }
                this.SetProcessId(sinvoice);
                this.CopyExtraFiles(sinvoice, directory, $"CUV_SETT{sinvoice}", $"RIPS_SETT{sinvoice}", $"XML_SETT{sinvoice}", false, directory);
                string[] keywords = { $"CUV_SETT{sinvoice}", $"RIPS_SETT{sinvoice}", $"XML_SETT{sinvoice}" };
                listfiles.Clear();
                listfiles.AddRange(Directory.GetFiles(directory).Where(f => keywords.Any(k => Path.GetFileName(f).Contains(k))));
                this.CompressFiles($"SETT{sinvoice}");
                this.RemoveFiles();
                listfiles.Clear();
            }
            catch (Exception ex)
            {
                LogError.WriteError("Trazabilidad", "Trazabilidad", ex);
                throw;
            }

        }


        private void ProcessHDI(string directory, string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            try
            {
                this.GenerateInvoiceFile(sinvoicepath, Path.Combine(directory, sinvoicename));
                this.GenerateSupports(ldesmaterializacion, sinvoice);
                this.CopyExtraFiles(sinvoice, directory, string.Empty, string.Empty, string.Empty, true);
                this.CompressFiles($"SETT{sinvoice}");
                this.RemoveFiles();
                this.listfiles.Clear();
            }
            catch (Exception ex)
            {
                LogError.WriteError("Trazabilidad", "Trazabilidad", ex);
                throw;
            }

        }


        private void ProccessFiles(string sinvoice, string sinvoicepath, string sinvoicename, List<Desmaterializacion> ldesmaterializacion)
        {
            /*this.scompany = "64";
            this.sRelation = "00000";*/
            string directory = Path.Combine(Configuration.GetStringValue("RelationshipsFolder"), this.sRelation);
            if (scompany.EqualsAnyOf("21", "260", "259")) //Compensar
            {
                this.ProcessCompensar(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsAnyOf("19")) //Bolivar
            {
                this.ProcessBolivar(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsAnyOf("25")) //Sanitas
            {
                this.ProcessSanitas(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsAnyOf("23")) //Coomeva
            {
                this.ProcessCoomeva(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsAnyOf("83", "53")) //Sura, Suramericana, Sura ARL
            {
                this.ProcessSura(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsAnyOf("26")) //Famisanar
            {
                this.ProcessFamisanar(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsAnyOf("263")) //Famisanar Pac empresa
            {
                this.ProcessFamisanar(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsAnyOf("17")) //Colmena
            {
                this.ProcessAlianzColmena(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsAnyOf("02")) //Allianz
            {
                this.ProcessAlianz(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsAnyOf("04", "05", "215")) //Axa Colpatria
            {
                this.ProcessAxaColpatria(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsAnyOf("44")) //Positiva
            {
                this.ProcessPositiva(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsAnyOf("18", "39")) //Colsanitas y Medisanitas
            {
                this.ProcessSanitasPrepagada(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsAnyOf("171", "09")) //Capital Salud y Viva 1
            {
                this.ProcessCapitalViva(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsAnyOf("50", "261")) //Salud Total
            {
                this.ProcessSaludTotal(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsAnyOf("40")) //Medplus
            {
                this.ProcessMedplus(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsAnyOf("01", "16")) //Aliansalud, Colmedica
            {
                this.ProcessColmedica(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsAnyOf("114")) //Ecopetrol
            {
                this.ProcessEcopetrol(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsOfAny("112")) //Policia
            {
                this.ProcessPolicia(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsOfAny("64")) //Nueva EPS
            {
                this.ProcessNuevaEPS(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsOfAny("255")) //Armada Nacional
            {
                this.ProcessArmada(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsOfAny("58")) //La equidad seguros
            {
                this.ProcessEquidad(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsOfAny("43")) //Pan American Life
            {
                this.ProcessPanAmericanLife(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsOfAny("37")) //Mapfre
            {
                this.ProcessMapfre(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsOfAny("144")) //Seguros Alfa
            {
                this.ProcessSegurosAlfa(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else if (scompany.EqualsOfAny("36")) //HDI
            {
                this.ProcessHDI(directory, sinvoice, sinvoicepath, sinvoicename, ldesmaterializacion);
            }
            else //El resto
            {
                this.GenerateInvoiceFile(sinvoicepath, Path.Combine(directory, sinvoicename));
                this.GenerateSupports(ldesmaterializacion, sinvoice);
                this.CopyExtraFiles(sinvoice, directory, string.Empty, string.Empty, string.Empty, true);
                this.CompressFiles($"FAC_SETT{sinvoice}");
                this.RemoveFiles();
                this.listfiles.Clear();
            }
        }

        private string GetFileName(string sfilepath)
        {
            FileInfo fileInfo = new FileInfo(sfilepath);
            return fileInfo.Name;
        }

        private void CopyExtraFiles(string sinvoice, string scuvfolder, string scuvname, string sripsname, string sxmlname, bool bincludeinzip = false, string sripfolder = "", bool bcuvtxt = false, string sxmlpath = "")
        {
            // --- S3 config ---
            string bucketName = Configuration.GetStringValue("S3BucketName");
            string cuvPrefix = Configuration.GetStringValue("CuvPrefix");
            string cuvHistPrefix = Configuration.GetStringValue("CuvHistPrefix");

            string sfilecuvname = string.Empty, sfileripsname = string.Empty;
            string[] paths = new string[] { Configuration.GetStringValue("CuvPath"), $"SETT{sinvoice}" };
            string sdirectory = Path.Combine(paths);
            string shistorydirectory = Path.Combine(Configuration.GetStringValue("HistoryCuvPath"), $"SETT{sinvoice}");
            string sdestination = string.Empty, scuv = string.Empty, sxml = string.Empty, srips = string.Empty, stxtcuv = string.Empty;
            string ripfolder = string.IsNullOrEmpty(sripfolder) ? scuvfolder : sripfolder;

            // --- Init AWS ---
            var aws = new AWSConnector(
                Configuration.GetStringValue("AWSKey"),
                Configuration.GetStringValue("AWSSecret")
            );
            aws.Connect();

            string scuvKey = null, sxmlKey = null, sripsKey = null;
            /*
            // Validación y búsqueda de archivos con manejo de excepciones
            try
            {
                if (Directory.Exists(sdirectory))
                {
                    var jsonFiles = Directory.GetFiles(sdirectory, "*.json");
                    scuv = jsonFiles.FirstOrDefault(x => Path.GetFileName(x).Contains("CUV_"));

                    var allFiles = Directory.GetFiles(sdirectory);
                    sxml = allFiles.FirstOrDefault(x => x.EndsWith(".xml"));
                    srips = allFiles.FirstOrDefault(x => Path.GetFileName(x).Contains("RIPS_"));
                }
                else if (Directory.Exists(shistorydirectory))
                {
                    // CORRECCIÓN: Aquí estabas usando sdirectory en lugar de shistorydirectory
                    var jsonFiles = Directory.GetFiles(shistorydirectory, "*.json");
                    scuv = jsonFiles.FirstOrDefault(x => Path.GetFileName(x).Contains("CUV_"));

                    var allFiles = Directory.GetFiles(shistorydirectory);
                    sxml = allFiles.FirstOrDefault(x => x.EndsWith(".xml"));
                    srips = allFiles.FirstOrDefault(x => Path.GetFileName(x).Contains("RIPS_"));
                }
                else
                {
                    this.lerror.AppendLine($"No se encontró el directorio para la factura SETT{sinvoice}. Rutas verificadas: {sdirectory} y {shistorydirectory}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                this.lerror.AppendLine($"Sin permisos para acceder al directorio de la factura SETT{sinvoice}: {ex.Message}");
            }
            catch (Exception ex)
            {
                this.lerror.AppendLine($"Error al buscar archivos para la factura SETT{sinvoice}: {ex.Message}");
            }*/

            try
            {
                string invPrefix = $"{cuvPrefix}SETT{sinvoice}/";
                var keys = aws.ListKeys(bucketName, invPrefix);

                if (keys.Count == 0)
                {
                    string invHistPrefix = $"{cuvHistPrefix}SETT{sinvoice}/";
                    var histKeys = aws.ListKeys(bucketName, invHistPrefix);

                    if (histKeys.Count == 0)
                    {
                        var flatHist = aws.ListKeys(bucketName, cuvHistPrefix);
                        histKeys = flatHist.Where(k => k.IndexOf($"SETT{sinvoice}", StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                    }

                    keys = histKeys;
                }

                if (keys.Count == 0)
                {
                    this.lerror.AppendLine($"No se encontraron objetos en S3 para SETT{sinvoice} en prefijos {cuvPrefix} / {cuvHistPrefix}");
                }
                else
                {
                    // Buscar archivos dentro de esos keys
                    scuvKey = keys.FirstOrDefault(k =>
                        (
                            Path.GetFileName(k).IndexOf("CUV_", StringComparison.OrdinalIgnoreCase) >= 0
                            || Path.GetFileName(k).EndsWith("_cuv.json", StringComparison.OrdinalIgnoreCase)
                        )
                        && k.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                    );
                    sxmlKey = keys.FirstOrDefault(k => k.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));
                    sripsKey = keys.FirstOrDefault(k =>
                        (
                            Path.GetFileName(k).IndexOf("RIPS_", StringComparison.OrdinalIgnoreCase) >= 0
                            || Path.GetFileName(k).EndsWith("_rips.json", StringComparison.OrdinalIgnoreCase)
                        )
                        && k.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                    );
                }
            }
            catch (Exception ex)
            {
                this.lerror.AppendLine($"Error consultando S3 para SETT{sinvoice}: {ex.Message}");
            }

            // Creación de timestamp si no existe
            var consecutive = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.json";

            if (!string.IsNullOrEmpty(scuvKey))
            {
                try
                {
                    byte[] bytes = aws.DownloadFileAsync(scuvKey, bucketName);

                    if (bytes == null || bytes.Length == 0)
                    {
                        this.lerror.AppendLine($"Archivo CUV para SETT{sinvoice} es 0 bytes o no se pudo descargar. Key: {scuvKey}");
                    }
                    else
                    {
                        sfilecuvname = Path.GetFileName(scuvKey);
                        string sextension = (bcuvtxt) ? "txt" : "json";

                        if (this.scompany.EqualsAnyOf("19", "26", "02", "263"))
                        {
                            string[] names = sfilecuvname.Split('_');
                            if (names.Length > 1)
                            {
                                if (names[1].StartsWith("SETT") && this.scompany.Equals("02"))
                                {
                                    names[1] = consecutive;
                                }
                                sfilecuvname = $"{names[1].Replace(".json", "")}_{scuvname}.{sextension}";
                            }
                            sdestination = Path.Combine(scuvfolder, sfilecuvname);
                        }
                        else
                        {
                            sdestination = string.IsNullOrEmpty(scuvname) ? Path.Combine(scuvfolder, sfilecuvname) : Path.Combine(scuvfolder, $"{scuvname}.{sextension}");
                        }

                        sdestination = sdestination.Replace("json", sextension);

                        if (!File.Exists(sdestination))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(sdestination));
                            File.WriteAllBytes(sdestination, bytes);

                            if (bincludeinzip) listfiles.Add(sdestination);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.lerror.AppendLine($"Error al copiar archivo CUV para SETT{sinvoice} desde S3: {ex.Message}");
                }
            }
            else
            {
                this.lerror.AppendLine($"Archivo CUV para SETT{sinvoice} no ha podido ser encontrado en S3");
            }

            if (!string.IsNullOrEmpty(sxmlKey) && !this.scompany.EqualsAnyOf("50", "261"))
            {
                try
                {
                    byte[] bytes = aws.DownloadFileAsync(sxmlKey, bucketName);

                    if (bytes == null || bytes.Length == 0)
                    {
                        this.lerror.AppendLine($"Archivo XML para SETT{sinvoice} es 0 bytes o no se pudo descargar. Key: {sxmlKey}");
                    }
                    else
                    {
                        string localName = Path.GetFileName(sxmlKey);
                        string sxmlfinalpath = (string.IsNullOrEmpty(sxmlpath)) ? scuvfolder : sxmlpath;

                        sdestination = string.IsNullOrEmpty(sxmlname)
                            ? Path.Combine(sxmlfinalpath, localName)
                            : Path.Combine(sxmlfinalpath, $"{sxmlname}.xml");

                        if (this.scompany.EqualsOfAny("44", "26", "263", "23", "25", "43"))
                        {
                            sdestination = sdestination.Replace("FEV", "XML");
                            if (this.scompany.EqualsOfAny("23", "25", "43"))
                            {
                                var tmpconsecutive = sfilecuvname.Replace(".json", "");
                                tmpconsecutive = tmpconsecutive.Replace("CUV_", "");
                                sdestination = sdestination.Replace(tmpconsecutive, $"SETT{sinvoice}");
                            }
                        }

                        if (!File.Exists(sdestination))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(sdestination));
                            File.WriteAllBytes(sdestination, bytes);

                            if (bincludeinzip) listfiles.Add(sdestination);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.lerror.AppendLine($"Error al copiar archivo XML para SETT{sinvoice} desde S3: {ex.Message}");
                }
            }
            else
            {
                this.lerror.AppendLine($"Archivo XML para SETT{sinvoice} no ha podido ser encontrado en S3");
            }

            if (!string.IsNullOrEmpty(sripsKey))
            {
                try
                {
                    byte[] bytes = aws.DownloadFileAsync(sripsKey, bucketName);

                    if (bytes == null || bytes.Length == 0)
                    {
                        this.lerror.AppendLine($"Archivo RIPS para SETT{sinvoice} es 0 bytes o no se pudo descargar. Key: {sripsKey}");
                    }
                    else
                    {
                        sfileripsname = Path.GetFileName(sripsKey);

                        if (!this.bisallianz)
                        {
                            sdestination = string.IsNullOrEmpty(sripsname) ? Path.Combine(ripfolder, this.GetFileName(sfileripsname)) : Path.Combine(ripfolder, $"{sripsname}.json");
                            if (this.scompany.EqualsOfAny("23", "43"))
                            {
                                var tmpconsecutive = sfileripsname.Replace(".json", "");
                                tmpconsecutive = tmpconsecutive.Replace("RIPS_", "");
                                sdestination = sdestination.Replace(tmpconsecutive, $"SETT{sinvoice}");
                            }
                        }
                        else
                        {
                            string[] names = sfileripsname.Split('_');
                            if (names.Length > 1)
                            {
                                if (names[1].StartsWith("SETT") && this.scompany.Equals("02"))
                                {
                                    names[1] = consecutive;
                                }
                                sfileripsname = $"{names[1].Replace(".json", "")}_{sripsname}.json";
                            }
                            sdestination = Path.Combine(ripfolder, sfileripsname);
                        }

                        if (!File.Exists(sdestination))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(sdestination));
                            File.WriteAllBytes(sdestination, bytes);

                            if (bincludeinzip) listfiles.Add(sdestination);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.lerror.AppendLine($"Error al copiar archivo RIPS para SETT{sinvoice} desde S3: {ex.Message}");
                }
            }
            else
            {
                this.lerror.AppendLine($"Archivo RIPS para SETT{sinvoice} no ha podido ser encontrado en S3");
            }



            /*else
            {
                string sbasedir = $"\\\\LOKI\\RIPServinte\\NumeroEnvío_{this.sRelation}\\";
                if (Directory.Exists(sbasedir))
                {
                    srips = Directory.EnumerateFiles(sbasedir, "*.json", SearchOption.AllDirectories).FirstOrDefault(f => Path.GetFileName(f).Contains(sinvoice));
                    sxml = Directory.EnumerateFiles(sbasedir, "*.xml", SearchOption.AllDirectories).FirstOrDefault(f => Path.GetFileName(f).Contains(sinvoice));
                }                
            }*/
            /*
            // Procesamiento de archivo CUV
            if (!string.IsNullOrEmpty(scuv))
            {
                try
                {
                    if (File.Exists(scuv) && new FileInfo(scuv).Length == 0)
                    {
                        this.lerror.AppendLine($"Archivo CUV para la factura SETT{sinvoice} es de 0 bytes");
                    }
                    else if (File.Exists(scuv))
                    {
                        sfilecuvname = this.GetFileName(scuv);
                        string sextension = (bcuvtxt) ? "txt" : "json";

                        if (this.scompany.EqualsAnyOf("19", "26", "02", "263"))
                        {
                            string[] names = sfilecuvname.Split('_');
                            if (names.Length > 1)
                            {
                                if (names[1].StartsWith("SETT") && this.scompany.Equals("02"))
                                {
                                    names[1] = consecutive;
                                }
                                sfilecuvname = $"{names[1].Replace(".json", "")}_{scuvname}.{sextension}";
                            }
                            sdestination = Path.Combine(scuvfolder, sfilecuvname);
                        }
                        else
                        {
                            sdestination = string.IsNullOrEmpty(scuvname) ? Path.Combine(scuvfolder, sfilecuvname) : Path.Combine(scuvfolder, $"{scuvname}.{sextension}");
                        }

                        sdestination = sdestination.Replace("json", sextension);

                        if (!File.Exists(sdestination))
                        {
                            File.Copy(scuv, sdestination);
                            if (bincludeinzip)
                            {
                                listfiles.Add(sdestination);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.lerror.AppendLine($"Error al copiar archivo CUV para la factura SETT{sinvoice}: {ex.Message}");
                }
            }
            else
            {
                this.lerror.AppendLine($"Archivo CUV para la factura SETT{sinvoice} no ha podido ser encontrado");
            }

            // Procesamiento de archivo XML
            if (!string.IsNullOrEmpty(sxml) && !this.scompany.EqualsAnyOf("50", "261"))
            {
                try
                {
                    if (File.Exists(sxml) && new FileInfo(sxml).Length == 0)
                    {
                        this.lerror.AppendLine($"Archivo XML para la factura SETT{sinvoice} es de 0 bytes");
                    }
                    else if (File.Exists(sxml))
                    {
                        string sxmlfinalpath = (string.IsNullOrEmpty(sxmlpath)) ? scuvfolder : sxmlpath;
                        sdestination = string.IsNullOrEmpty(sxmlname) ? Path.Combine(sxmlfinalpath, this.GetFileName(sxml)) : Path.Combine(sxmlfinalpath, $"{sxmlname}.xml");

                        if (this.scompany.EqualsOfAny("44", "26", "263", "23", "25"))
                        {
                            sdestination = sdestination.Replace("FEV", "XML");
                            if (this.scompany.EqualsOfAny("23", "25"))
                            {
                                var tmpconsecutive = sfilecuvname.Replace(".json", "");
                                tmpconsecutive = tmpconsecutive.Replace("CUV_", "");
                                sdestination = sdestination.Replace(tmpconsecutive, $"SETT{sinvoice}");
                            }
                        }

                        if (!File.Exists(sdestination))
                        {
                            File.Copy(sxml, sdestination);
                            if (bincludeinzip)
                            {
                                listfiles.Add(sdestination);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.lerror.AppendLine($"Error al copiar archivo XML para la factura SETT{sinvoice}: {ex.Message}");
                }
            }
            else
            {
                this.lerror.AppendLine($"Archivo XML para la factura SETT{sinvoice} no ha podido ser encontrado");
            }

            // Procesamiento de archivo RIPS
            if (!string.IsNullOrEmpty(srips))
            {
                try
                {
                    if (File.Exists(srips) && new FileInfo(srips).Length == 0)
                    {
                        this.lerror.AppendLine($"Archivo RIPS para la factura SETT{sinvoice} es de 0 bytes");
                    }
                    else if (File.Exists(srips))
                    {
                        sfileripsname = this.GetFileName(srips);

                        if (!this.bisallianz)
                        {
                            sdestination = string.IsNullOrEmpty(sripsname) ? Path.Combine(ripfolder, this.GetFileName(srips)) : Path.Combine(ripfolder, $"{sripsname}.json");
                            if (this.scompany.EqualsOfAny("23"))
                            {
                                var tmpconsecutive = sfileripsname.Replace(".json", "");
                                tmpconsecutive = tmpconsecutive.Replace("RIPS_", "");
                                sdestination = sdestination.Replace(tmpconsecutive, $"SETT{sinvoice}");
                            }
                        }
                        else
                        {
                            string[] names = sfileripsname.Split('_');
                            if (names.Length > 1)
                            {
                                if (names[1].StartsWith("SETT") && this.scompany.Equals("02"))
                                {
                                    names[1] = consecutive;
                                }
                                sfileripsname = $"{names[1].Replace(".json", "")}_{sripsname}.json";
                            }
                            sdestination = Path.Combine(ripfolder, sfileripsname);
                        }

                        if (!File.Exists(sdestination))
                        {
                            File.Copy(srips, sdestination);
                            if (bincludeinzip)
                            {
                                listfiles.Add(sdestination);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.lerror.AppendLine($"Error al copiar archivo RIPS para la factura SETT{sinvoice}: {ex.Message}");
                }
            }
            else
            {
                this.lerror.AppendLine($"Archivo RIPS para la factura SETT{sinvoice} no ha podido ser encontrado");
            }*/
        }

        public void MergePdfsITextSharp(List<string> inputFiles, string outputFile)
        {
            FileStream stream = null;
            Document document = null;
            PdfCopy pdfCopy = null;

            try
            {
                stream = new FileStream(outputFile, FileMode.Create);
                document = new Document();
                pdfCopy = new PdfCopy(document, stream);

                document.Open();

                foreach (string file in inputFiles)
                {
                    PdfReader reader = null;
                    try
                    {
                        reader = new PdfReader(file);

                        for (int i = 1; i <= reader.NumberOfPages; i++)
                        {
                            pdfCopy.AddPage(pdfCopy.GetImportedPage(reader, i));
                        }
                    }
                    finally
                    {
                        reader?.Close();
                    }
                }
            }
            finally
            {
                if (document != null && document.IsOpen())
                    document.Close();

                pdfCopy?.Close();
                stream?.Close();
            }
        }



        private void SetProcessId(string sinvoice)
        {
            string[] paths = new string[] { Configuration.GetStringValue("CuvPath"), $"SETT{sinvoice}" };
            string sdirectory = Path.Combine(paths);
            string scuv = string.Empty;
            if (Directory.Exists(sdirectory))
            {
                //scuv = Directory.GetFiles(sdirectory, "*.json").FirstOrDefault(x => Path.GetFileName(x).Contains("CUV_"));
                scuv = Directory.GetFiles(sdirectory, "*.json")
                    .FirstOrDefault(x =>
                    {
                        var fileName = Path.GetFileName(x);

                        return fileName.IndexOf("CUV_", StringComparison.OrdinalIgnoreCase) >= 0
                               || fileName.EndsWith("_cuv.json", StringComparison.OrdinalIgnoreCase);
                    });
                if (!string.IsNullOrEmpty(scuv))
                {
                    string jsonContent = File.ReadAllText(scuv);
                    if (!string.IsNullOrEmpty(jsonContent))
                    {
                        JObject jsonObj = JObject.Parse(jsonContent);
                        if (jsonObj["ProcesoId"] != null)
                        {
                            this.sprocessid = jsonObj["ProcesoId"].ToString();
                        }
                    }
                }
            }
            else
            {
                paths = new string[] { Configuration.GetStringValue("HistoryCuvPath"), $"SETT{sinvoice}" };
                sdirectory = Path.Combine(paths);
                if (Directory.Exists(sdirectory))
                {
                    //scuv = Directory.GetFiles(sdirectory, "*.json").FirstOrDefault(x => Path.GetFileName(x).Contains("CUV_"));
                    scuv = Directory.GetFiles(sdirectory, "*.json")
                        .FirstOrDefault(x =>
                        {
                            var fileName = Path.GetFileName(x);

                            return fileName.IndexOf("CUV_", StringComparison.OrdinalIgnoreCase) >= 0
                                   || fileName.EndsWith("_cuv.json", StringComparison.OrdinalIgnoreCase);
                        });
                    if (!string.IsNullOrEmpty(scuv))
                    {
                        string jsonContent = File.ReadAllText(scuv);
                        if (!string.IsNullOrEmpty(jsonContent))
                        {
                            JObject jsonObj = JObject.Parse(jsonContent);
                            if (jsonObj["ProcesoId"] != null)
                            {
                                this.sprocessid = jsonObj["ProcesoId"].ToString();
                            }
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(this.sprocessid))
            {
                this.lerror.AppendLine($"Archivo CUV para la factura SETT{sinvoice} no ha podido ser leido");
            }
        }

        private void GenerateInvoiceFile(string invoicefile, string sresultfile, bool binclude = true, string sinvoice = "")
        {
            string sinvoicefile = Path.Combine(Configuration.GetStringValue("InvoicesPath"), invoicefile + ".pdf");
            if (File.Exists(sinvoicefile))
            {
                File.Copy(sinvoicefile, sresultfile);
                if (binclude)
                {
                    listfiles.Add(sresultfile);
                }
                if (this.scompany.EqualsAnyOf("64"))
                {
                    this.lcontrol.AppendLine($"800180553;SETT;{sinvoice};{new FileInfo(sresultfile).Name}");
                }
            }
            else
            {
                this.lerror.AppendLine($"Archivo de factura {sinvoicefile} no encontrado");
            }
        }

        private void GenerateErrorFile(string sdirectory)
        {
            string sfile = Path.Combine(sdirectory, "errorlog.txt");
            if (!File.Exists(sfile))
            {
                File.WriteAllText(sfile, this.lerror.ToString());
            }
            else
            {
                File.AppendAllText(sfile, this.lerror.ToString());
            }
        }

        private void CompressFolder(string szipname, string sfolder)
        {
            //string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, $"{szipname}.zip" };
            //string zipresult = Path.Combine(paths);
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, sfolder };
            string folderresult = Path.Combine(paths);
            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(folderresult, System.IO.Path.GetFileName(folderresult));
                zip.Save(szipname);
            }
        }

        private void CompressFiles(string szipname, string sdestination = "", bool bupdatezip = false, bool bcreatedirectory = false)
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation, $"{szipname}.zip" };
            string zipresult = string.IsNullOrEmpty(sdestination) ? Path.Combine(paths) : Path.Combine(sdestination, $"{szipname}.zip");

            using (ZipFile zip = File.Exists(zipresult) && bupdatezip ? ZipFile.Read(zipresult) : new ZipFile())
            {
                foreach (var archivo in listfiles)
                {
                    string directoryInZip = bcreatedirectory ? szipname : string.Empty;

                    // Verifica si el archivo ya existe en el ZIP
                    string entryName = Path.Combine(directoryInZip, Path.GetFileName(archivo));
                    if (zip.ContainsEntry(entryName))
                    {
                        zip.RemoveEntry(entryName); // Elimina la versión anterior
                    }
                    zip.AddFile(archivo, directoryInZip);
                }
                zip.Save(zipresult);
            }
        }

        private void RemoveFiles()
        {
            foreach (var file in listfiles)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sfile"></param>
        /// <param name="lFiles"></param>
        /// <param name="stype"></param>
        /// <param name="sinvoicename"></param>
        /// <param name="sinvoice"></param>
        private void GeneratePDFSupport(string sfile, List<string> lFiles, string stype, string sinvoicename, string sdestinationpath = "", bool bchangename = false)
        {
            string[] paths = new string[] { Configuration.GetStringValue("RelationshipsFolder"), this.sRelation };
            string spath = string.IsNullOrEmpty(sdestinationpath) ? Path.Combine(paths) : sdestinationpath;
            StringBuilder ssupportfile = new StringBuilder();
            if (this.scompany.EqualsOfAny("112") && !bchangename)
            {
                ssupportfile.Append(this.GetSuppotType(stype));
            }
            else if (!this.scompany.EqualsOfAny("112") && !bchangename)
            {
                ssupportfile.Append(this.GetSuppotType(stype));
                ssupportfile.Append("_800180553_SETT");
                ssupportfile.Append(sinvoicename);
                if (!string.IsNullOrEmpty(sfile))
                {
                    ssupportfile.Append($"_{sfile}");
                }
            }
            else
            {
                ssupportfile.Append(sfile);
            }
            ssupportfile.Append(".PDF");
            string pdfFileName = ssupportfile.ToString();
            string pdffile = Path.Combine(spath, pdfFileName);
            string tempFile = pdffile + ".temp";

            Document document = null;
            PdfSmartCopy pdfCopy = null;
            FileStream fs = null;
            try
            {
                fs = new FileStream(tempFile, FileMode.Create);
                if (File.Exists(pdffile))
                {
                    PdfReader reader = new PdfReader(pdffile);
                    document = new Document(reader.GetPageSizeWithRotation(1));
                    pdfCopy = new PdfSmartCopy(document, fs);
                    document.Open();

                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        pdfCopy.AddPage(pdfCopy.GetImportedPage(reader, i));
                    }

                    reader.Close();
                }
                else
                {
                    document = new Document(PageSize.LETTER);
                    pdfCopy = new PdfSmartCopy(document, fs);
                    document.Open();
                }

                foreach (string item in lFiles)
                {
                    try
                    {
                        MemoryStream ms = new MemoryStream();
                        Document tempDoc = new Document(PageSize.LETTER);
                        PdfWriter writer = PdfWriter.GetInstance(tempDoc, ms);
                        tempDoc.Open();

                        iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(item);
                        img.SetAbsolutePosition(0, 0);
                        img.ScaleToFit(PageSize.LETTER.Width, PageSize.LETTER.Height);
                        img.SetDpi(300, 300);
                        tempDoc.Add(img);

                        tempDoc.Close();
                        writer.Close();

                        PdfReader imgReader = new PdfReader(ms.ToArray());
                        pdfCopy.AddPage(pdfCopy.GetImportedPage(imgReader, 1));
                        imgReader.Close();
                        ms.Close();
                    }
                    catch (Exception ex)
                    {
                        LogError.WriteError("Trazabilidad", "Trazabilidad", ex);
                        this.lerror.AppendLine($"Archivo de soporte {item} no ha podido ser generado");
                        MemoryStream ms = new MemoryStream();
                        Document tempDoc = new Document(PageSize.LETTER);
                        PdfWriter writer = PdfWriter.GetInstance(tempDoc, ms);
                        tempDoc.Open();

                        iTextSharp.text.Image blankImg = iTextSharp.text.Image.GetInstance(Configuration.GetStringValue("BlankImage"));
                        blankImg.SetAbsolutePosition(0, 0);
                        blankImg.ScaleToFit(PageSize.LETTER.Width, PageSize.LETTER.Height);
                        tempDoc.Add(blankImg);

                        tempDoc.Close();
                        writer.Close();

                        PdfReader fallbackReader = new PdfReader(ms.ToArray());
                        pdfCopy.AddPage(pdfCopy.GetImportedPage(fallbackReader, 1));
                        fallbackReader.Close();
                        ms.Close();
                    }
                }
                document.Close();
                pdfCopy.Close();
                fs.Close();

                if (File.Exists(pdffile))
                {
                    File.Delete(pdffile);
                }
                File.Move(tempFile, pdffile);
                if (this.bfileinzip)
                {
                    listfiles.Add(pdffile);
                }
                if (this.scompany.EqualsAnyOf("64"))
                {
                    this.lcontrol.AppendLine($"800180553;SETT;{sinvoicename};{new FileInfo(pdffile).Name}");
                }
            }
            catch (Exception ex)
            {
                LogError.WriteError("Trazabilidad", "Trazabilidad", ex);
                try { document?.Close(); } catch { }
                try { pdfCopy?.Close(); } catch { }
                try { fs?.Close(); } catch { }
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }


        private void GenerateSanitasSupports(List<Desmaterializacion> ldesmaterializacion, string sinvoice, string sdestinationpath, bool bprepaid = false)
        {
            List<Generic> lgeneric = null;
            List<string> lsupports = null;
            string[] supportpath = null;
            string sfilename = string.Empty;
            var tmp = ldesmaterializacion.Where(z => z.sfactura == sinvoice).Select
            (
                x => new
                {
                    schapter = x.sepisodio,
                    sinvoive = x.sfactura,
                    sdocument = x.sdocumento,
                    sdocumenttype = x.stipodocumento
                }
            )
            .GroupBy
            (
                x => new
                {
                    x.schapter,
                    x.sinvoive,
                    x.sdocument,
                    x.sdocumenttype
                }
            )
            .Select
            (
                y => new
                {
                    chapter = y.Key.schapter,
                    document = y.Key.sdocument,
                    documenttype = y.Key.sdocumenttype
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
                        if (this.scompany == "114")
                        {
                            sfilename = $"SETT{sinvoice}_SOP";
                        }
                        else if (this.scompany.EqualsAnyOf("39", "18"))
                        {
                            if (generic.code == "Autorizacion")
                            {
                                sfilename = $"SETT{sinvoice}_SOP_1";
                            }
                            else if (generic.code == "Soporte Clinico")
                            {
                                sfilename = $"SETT{sinvoice}_SOP_2";
                            }
                        }
                        else
                        {
                            sfilename = !bprepaid ? $"{item.documenttype}{item.document}" : $"SETT{sinvoice}_SOP_1";
                        }
                        this.GeneratePDFSupport(sfilename, lsupports, generic.code, sinvoice, sdestinationpath, bprepaid);
                    }
                }
            }
        }

        private void GenerateSupports(List<Desmaterializacion> ldesmaterializacion, string sinvoice, string sdestinationpath = "")
        {
            List<Generic> lgeneric = null;
            List<string> lsupports = null;
            string[] supportpath = null;
            string scode = string.Empty;
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
                    scode = !this.bunifiedsupport ? generic.code : "Soporte";
                    if (lsupports.Count > 0)
                    {
                        this.GeneratePDFSupport(string.Empty, lsupports, scode, sinvoice, sdestinationpath);
                    }
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
            string sauthname = string.Empty;
            string sdetailname = string.Empty;
            string sordername = string.Empty;
            string ssupportname = string.Empty;
            if (this.scompany.EqualsAnyOf("26", "263"))
            {
                sauthname = "AUT";
            }
            else if (this.scompany.EqualsAnyOf("44"))
            {
                sauthname = "ADS";
                ssupportname = "EPI";
            }
            else if (this.scompany.EqualsAnyOf("04", "05", "215"))
            {
                sauthname = "OPF";
            }
            else if (this.scompany.EqualsAnyOf("50", "261", "02"))
            {
                sauthname = "OPF";
                ssupportname = "PDX";
            }
            else if (this.scompany.EqualsAnyOf("255"))
            {
                sauthname = "PDE";
                sordername = "OPF";
                ssupportname = "PDX";
            }
            else if (this.scompany.EqualsAnyOf("112"))
            {
                sauthname = "Autorizacion";
                ssupportname = "Soporte";
            }
            switch (ssupport)
            {
                case "Autorizacion": return (!string.IsNullOrEmpty(sauthname)) ? sauthname : "OPF";
                case "Detalle de Factura": return (!string.IsNullOrEmpty(sdetailname)) ? sdetailname : "HEV";
                case "Orden Medica": return (!string.IsNullOrEmpty(sauthname)) ? sordername : "OPF";
                default: return (!string.IsNullOrEmpty(ssupportname)) ? ssupportname : "PDX";
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}