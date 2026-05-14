using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;
using Utils;
using System.IO;

namespace DAC
{
    public class ConsultasDAC : IDisposable
    {
        private string sConnection { get; set; }

        public ConsultasDAC(string Connection)
        {
            this.sConnection = Connection;
        }

        private DataTable GetDiagnosis(DateTime iDate)
        {
            DataTable dt = new DataTable();
            StringBuilder sDiagnosis = new StringBuilder();
            StringBuilder sQuery = new StringBuilder("SELECT hicdetdiag.CREGIDPAC, hicdetdiag.DREGHIC, RipsDiag.CDIAGCOD, RipsDiag.CDIAGDES FROM hicdetdiag, RipsDiag WHERE RipsDiag.CDIAGCOD = hicdetdiag.CDIAGCOD");
            sQuery.Append(" AND hicdetdiag.DREGHIC <= ");
            sQuery.Append(Tools.FormatFoxDate(iDate));
            using (OLEDBDAC oDAC = new OLEDBDAC(sConnection))
            {
                return oDAC.GetDataTable(sQuery.ToString(), null, false);                
            }            
        }
        
        private string GetDiagnosis(DataTable dt, DateTime iDate, string Patient)
        {
            var list = dt.AsEnumerable().Where(x => x["CREGIDPAC"].ToString() == Patient && Convert.ToDateTime(x["DREGHIC"]).CompareTo(iDate) == 0);
            StringBuilder sDiagnosis = new StringBuilder();
            sDiagnosis.Append("\"");
            foreach (var item in list)
            {                
                sDiagnosis.Append(item[2].ToString().Trim());
                sDiagnosis.Append(" ");
                sDiagnosis.Append(item[3].ToString().Trim().Replace("\"", "'"));
                sDiagnosis.Append("\n");                
            }
            sDiagnosis.Append("\"");
            return sDiagnosis.ToString();
        }

        public void Generate()
        {
            //DateTime dDate = new DateTime(2015, 09, 21);
            DateTime dDate = new DateTime(2015, 12, 5);
            StringBuilder sQuery = new StringBuilder("SELECT lectura.id AS AppointmentId, '005o0000001hAIk' AS Owner, (hictipocita.CTIPCITDES + ' ' + DTOC(Dreghic)) AS AppointmentName, '012o0000000pebrAAA' AS AppointmentType, '' AS Companion");
            sQuery.Append(", hicmaestra.CREGIDPAC AS Patient, IIF(hicprofesional.cregidpro = '0081', '0080',  IIF(hicprofesional.cregidpro = '0300', '0301', hicprofesional.cregidpro)) AS Professional, '' AS Support, hicmaestra.Dreghic AS Fecha, '' AS Email, '' AS Reason");
            sQuery.Append(", '' AS Disease, '' AS Perinatal, '' AS Pahologic, '' AS Pharmacologic, '' AS Clinical, '' AS UpperSymtoms, '' AS LowerSymtoms, '' AS Recurrent");
            sQuery.Append(", '' AS AppointmentForeign, 'true' AS IsClosed, hicmaestra.Dreghic AS CloseDate, '' AS Physical, '' AS Diagnosis, '' AS Percentil, '' AS Lpm, '' AS Rpm");
            sQuery.Append(", IIF(hicprofesional.cregidpro = '0081', '0080',  IIF(hicprofesional.cregidpro = '0300', '0301', hicprofesional.cregidpro)) AS ProfessionalId, '' AS Appointment, '' AS Weight, '' AS Size, '' AS Bmi, '' AS Condition, IIF(EMPTY(empresa.cempcodgst), empresa.cempcod, empresa.cempcodgst) AS CompanyId, '' AS Medication");
            sQuery.Append(", '' AS Attention, IIF(hicprofesional.cregidpro = '0081', '0080',  IIF(hicprofesional.cregidpro = '0300', '0301', hicprofesional.cregidpro)) AS ClosedBy, '' AS AuthCode, '' AS Formulation, '' AS Referal, 'true' AS IsMigrated, 'Médico FNC' AS ViaEntry, 'Ambulatorio' AS CareType");
            sQuery.Append(", '' AS ABG, '' AS Fio2, '' AS Ph, '' AS Paco, '' AS HC, '' AS Sato, '' AS Pao, '' AS Hemo, '' AS HemoInt, '' AS Spirometry, '' AS Cvf, '' AS Vefl1, '' AS Vefl2, '' AS Cvf2");
            sQuery.Append(", '' AS Vefl, '' AS Cvf3, '' AS Electro, '' AS Eco, '' AS EcoInt, '' AS AppointmentStatus, '' AS PastDisease, '' AS Digestive, '' AS Skin, '' AS Neuro, '' AS Paraclinic, '' AS Analysis");
            sQuery.Append(", '' AS Education, TRIM(empresa.cempdes) AS Convenio, TRIM(programa.cprogdes) AS Plan, hicmaestra.cclave");
            //sQuery.Append(" FROM 1paccarg AS t1, hictipocita, hicmaestra, hicprofesional, hicpaciente LEFT JOIN empresa ON empresa.cempcod = hicpaciente.cempcod LEFT JOIN programa ON hicpaciente.cprogcod = programa.cprogcod");
            sQuery.Append(" FROM lectura, hictipocita, hicmaestra, hicprofesional, hicpaciente LEFT JOIN empresa ON empresa.cempcod = hicpaciente.cempcod LEFT JOIN programa ON hicpaciente.cprogcod = programa.cprogcod");
            sQuery.Append(" WHERE hicmaestra.ctipcitcod = hictipocita.ctipcitcod AND hicprofesional.cregidpro = hicmaestra.cregidpro AND hicmaestra.CREGIDPAC = hicpaciente.CREGIDPAC");
            //sQuery.Append(" FROM paccondel AS t1, hictipocita, hicmaestra, hicprofesional, hicpaciente LEFT JOIN empresa ON empresa.cempcod = hicpaciente.cempcod LEFT JOIN programa ON hicpaciente.cprogcod = programa.cprogcod");
            //sQuery.Append(" WHERE hicmaestra.ctipcitcod = hictipocita.ctipcitcod AND hicprofesional.cregidpro = hicmaestra.cregidpro AND hicmaestra.CREGIDPAC = hicpaciente.CREGIDPAC");            
            //sQuery.Append(" AND hicmaestra.dreghic BETWEEN {^2015-10-31} AND ");            
            //sQuery.Append(" AND hicmaestra.dreghic BETWEEN {^2015-12-01} AND ");    
            //sQuery.Append(" AND hicmaestra.dreghic <= ");            
            //sQuery.Append(Tools.FormatFoxDate(dDate));
            //sQuery.Append(" AND hicmaestra.nregidhic = 310818");
            //sQuery.Append(" AND t1.idfield BETWEEN 155521 AND 158815 AND hictipocita.lactivo = .T.");
            sQuery.Append(" AND hicmaestra.nregidhic = lectura.idassessment ORDER BY lectura.idassessment");


            //sQuery.Append(" AND t1.cregidpac = '0210542' AND hictipocita.lactivo = .T.");                        
            //sQuery.Append(" AND hicpaciente.cregidpac = t1.cregidpac ORDER BY t1.idfield");            
            //sQuery.Append(" AND hicpaciente.cregidpac = t1.cregidpac AND hicmaestra.Dreghic = t1.Dreghic");                        
            //sQuery.Append(" AND hicpaciente.cregidpac = t1.cregidpac ORDER BY t1.idfield");
            DataTable dt = new DataTable();
            //DataTable dt1 = new DataTable();            
            string[,] slines = null;
            int val = 0;
            int j = 94;
            int k = 0;
            using (OLEDBDAC oDAC = new OLEDBDAC(sConnection))
            {
                //dt1 = this.GetDiagnosis(dDate);
                dt = oDAC.GetDataTable(sQuery.ToString(), null, false);
                slines = new string[5001, 76];
                //slines = new string[dt.Rows.Count, 76];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                   
                    slines[k, 0] = dt.Rows[i][0].ToString().Trim();
                    slines[k, 1] = dt.Rows[i][1].ToString();
                    slines[k, 2] = dt.Rows[i][2].ToString().Trim();
                    slines[k, 3] = dt.Rows[i][3].ToString();
                    slines[k, 4] = dt.Rows[i][4].ToString();
                    slines[k, 5] = dt.Rows[i][5].ToString();
                    slines[k, 6] = dt.Rows[i][6].ToString().Trim();
                    slines[k, 7] = dt.Rows[i][7].ToString();
                    slines[k, 8] = Convert.ToDateTime(dt.Rows[i][8]).ToString("yyyy-MM-dd");
                    slines[k, 9] = dt.Rows[i][9].ToString();
                    slines[k, 10] = dt.Rows[i][10].ToString();
                    slines[k, 11] = dt.Rows[i][11].ToString();
                    slines[k, 12] = dt.Rows[i][12].ToString();
                    slines[k, 13] = dt.Rows[i][13].ToString();
                    slines[k, 14] = dt.Rows[i][14].ToString();
                    slines[k, 15] = dt.Rows[i][15].ToString();
                    slines[k, 16] = dt.Rows[i][16].ToString();
                    slines[k, 17] = dt.Rows[i][17].ToString();
                    slines[k, 18] = dt.Rows[i][18].ToString();
                    slines[k, 19] = dt.Rows[i][19].ToString();
                    slines[k, 20] = dt.Rows[i][20].ToString();
                    slines[k, 21] = Convert.ToDateTime(dt.Rows[i][21]).ToString("yyyy-MM-dd");
                    slines[k, 22] = dt.Rows[i][22].ToString();
                    slines[k, 23] = dt.Rows[i][23].ToString();
                    slines[k, 24] = dt.Rows[i][24].ToString();
                    slines[k, 25] = dt.Rows[i][25].ToString();
                    slines[k, 26] = dt.Rows[i][26].ToString();
                    slines[k, 27] = dt.Rows[i][27].ToString();
                    slines[k, 28] = dt.Rows[i][28].ToString();
                    slines[k, 29] = dt.Rows[i][29].ToString();
                    slines[k, 30] = dt.Rows[i][30].ToString();
                    slines[k, 31] = dt.Rows[i][31].ToString();
                    slines[k, 32] = dt.Rows[i][32].ToString();
                    if (int.TryParse(dt.Rows[i][33].ToString(), out val))
                    {
                        slines[k, 33] = Convert.ToInt32(dt.Rows[i][33]).ToString().PadLeft(2, '0');
                    }
                    else
                    {
                        slines[k, 33] = dt.Rows[i][33].ToString();
                    }
                    slines[k, 34] = dt.Rows[i][34].ToString();
                    slines[k, 35] = dt.Rows[i][35].ToString();
                    slines[k, 36] = dt.Rows[i][36].ToString();
                    slines[k, 37] = dt.Rows[i][37].ToString();
                    slines[k, 38] = dt.Rows[i][38].ToString();
                    slines[k, 39] = dt.Rows[i][39].ToString();
                    slines[k, 40] = this.GetDiagnosis(dt.Rows[i]["cclave"].ToString());
                    //slines[k, 40] = this.GetDiagnosis(dt1, Convert.ToDateTime(dt.Rows[i][8]), dt.Rows[i][5].ToString());
                    slines[k, 41] = this.GetServices(dt.Rows[i]["cclave"].ToString());
                    slines[k, 42] = dt.Rows[i][40].ToString();
                    slines[k, 43] = dt.Rows[i][41].ToString();
                    slines[k, 44] = dt.Rows[i][42].ToString();
                    slines[k, 45] = dt.Rows[i][43].ToString();
                    slines[k, 46] = dt.Rows[i][44].ToString();
                    slines[k, 47] = dt.Rows[i][45].ToString();
                    slines[k, 48] = dt.Rows[i][46].ToString();
                    slines[k, 49] = dt.Rows[i][47].ToString();
                    slines[k, 50] = dt.Rows[i][48].ToString();
                    slines[k, 51] = dt.Rows[i][49].ToString();
                    slines[k, 52] = dt.Rows[i][50].ToString();
                    slines[k, 53] = dt.Rows[i][51].ToString();
                    slines[k, 54] = dt.Rows[i][52].ToString();
                    slines[k, 55] = dt.Rows[i][53].ToString();
                    slines[k, 56] = dt.Rows[i][54].ToString();
                    slines[k, 57] = dt.Rows[i][55].ToString();
                    slines[k, 58] = dt.Rows[i][56].ToString();
                    slines[k, 59] = dt.Rows[i][57].ToString();
                    slines[k, 60] = dt.Rows[i][58].ToString();
                    slines[k, 61] = dt.Rows[i][59].ToString();
                    slines[k, 62] = dt.Rows[i][60].ToString();
                    slines[k, 63] = dt.Rows[i][61].ToString();
                    slines[k, 64] = dt.Rows[i][62].ToString();
                    slines[k, 65] = dt.Rows[i][63].ToString();
                    slines[k, 66] = dt.Rows[i][64].ToString();
                    slines[k, 67] = dt.Rows[i][65].ToString();
                    slines[k, 68] = dt.Rows[i][66].ToString();
                    slines[k, 69] = dt.Rows[i][67].ToString();
                    slines[k, 70] = dt.Rows[i][68].ToString();
                    slines[k, 71] = dt.Rows[i][69].ToString();
                    slines[k, 72] = this.GetPrescriptions(dt.Rows[i]["cclave"].ToString());
                    slines[k, 73] = this.GetDocumentation(dt.Rows[i]["cclave"].ToString());
                    slines[k, 74] = dt.Rows[i][70].ToString();
                    slines[k, 75] = dt.Rows[i][71].ToString();
                    k++;                                      
                    if ((i % 5000) == 0 && i > 0)
                    {
                        this.WriteFile(slines, j.ToString());
                        slines = null;
                        slines = new string[5001, 76];
                        j++;
                        k = 0;
                    }                  
                }
                this.WriteFile(slines, j.ToString());
                slines = null;                
            }
            dt.Dispose();
            dt = null;
            //return slines;
        }

        private void WriteFile(string[,] sLines, string sNumber)
        {
            string sFile = @"c:\Temp\consultas " + sNumber + ".csv";
            string sHeader = "idAssessment,OWNERID,NAME,RECORDTYPEID,COMPANION__C,PATIENTID__C,PROFESSIONALID__C,SUPPORTPROFESSIONALID__C,ASSESMENTDATE__C,COMPANIONEMAIL__C,ASSESMENTREASON__C,CURRENTDISEASE__C,PERINATAL__C,PATHOLOGICAL__C,PHARMACOLOGICAL__C,CLINICALS__C,UPPERRESPSYMTOMS__C,LOWERRESPSYMTOMS__C,RECURRENTINFECHISTORY__C,FOREIGNBODYHISTORY__C,ISCLOSED__C,CLOSEDATE__C,DESCRIPTIONPHYSICALEXAMINATION__C,DESCRIPCION_DIAGNOSTICO__C,PERCENTILE__C,FC_LPM__C,FR_RPM__C,PROFESSIONALIDENTIFICATION__C,APPOINMENTCODE__C,WEIGHT__C,SIZE__C,BMI__C,BMI_CONDITION__C,COMPANYID__C,MEDICATIONQTY__C,ATTENTIONTIME__C,CLOSEDBYUSER__C,AUTHORIZATIONCODE__C,FORMULATION__C,REFERALTEST__C,DIAGNOSISTEXT__C,ORDERTEST__C,ISMIGRATED__C,VIAENTRY__C,TYPEOFCARE__C,ABG__C,FIO2__C,PH__C,PACO2_MMHG__C,HCO3_MMOL_L__C,SATO2__C,PAAO2__C,HEMOGLOBIN__C,HEMOINTERPRETATION__C,SPIROMETRY__C,CVF_PREB2__C,VEFL_PREB2__C,VEFL_CVF_PREB2__C,CVF_POSTB2__C,VEFL_CVF_POSTB2__C,CVF_VEFL_INTERP__C,ELECTROCARDIOGRAM__C,ECHOCARDIOGRAM__C,ECHOCARDINTERP__C,STATUS__C,PASTDISEASE__C,DIGESTIVESYMPTOMS__C,SYMPTOMSSKIN__C,NEURODEVELOPMENTAL__C,PARACLINICOS__C,ANALYSIS__C,EDUCATIONSUGESTION__C,PrescriptionText__c,AlejusHistory__c,AgreementName__c,PlanName__c\n";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < sLines.GetLength(0); i++)
            {
                StringBuilder lineBuilder = new StringBuilder();
                for (int j = 0; j < sLines.GetLength(1); j++)
                {
                    if (lineBuilder.Length > 0)
                        lineBuilder.Append(",");
                    lineBuilder.Append(sLines[i, j]);
                }
                sb.AppendLine(lineBuilder.ToString());
            }
            File.AppendAllText(sFile, sHeader, Encoding.UTF8);
            File.AppendAllText(sFile, sb.ToString(), Encoding.UTF8);
        }

        //private string GetDiagnosis(DateTime iDate, string Patient)
        private string GetDiagnosis(string Clave)
        {
            DataTable dt = new DataTable();
            StringBuilder sDiagnosis = new StringBuilder();
            StringBuilder sQuery = new StringBuilder("SELECT RipsDiag.CDIAGCOD, hicdetdiag.mdiagdes FROM hicdetdiag, RipsDiag WHERE RipsDiag.CDIAGCOD = hicdetdiag.CDIAGCOD AND hicdetdiag.CCLAVE = '");
            sQuery.Append(Clave);
            sQuery.Append("'");            
            using (OLEDBDAC oDAC = new OLEDBDAC(sConnection))
            {
                dt = oDAC.GetDataTable(sQuery.ToString(), null, false);
                sDiagnosis.Append("\"");
                for (int i = 0; i < dt.Rows.Count; i++)
                {                    
                    sDiagnosis.Append(dt.Rows[i][0].ToString().Trim());
                    sDiagnosis.Append(" ");
                    sDiagnosis.Append(dt.Rows[i][1].ToString().Trim().Replace("\"", "'"));
                    sDiagnosis.Append("\n");
                }
                sDiagnosis.Append("\"");
            }
            dt.Dispose();
            dt = null;
            sQuery = null;
            GC.Collect();
            return sDiagnosis.ToString();
        }
        
        //private string GetServices(DateTime iDate, string Patient)
        private string GetServices(string Clave)
        {
            DataTable dt = new DataTable();
            StringBuilder sServices = new StringBuilder();
            StringBuilder sQuery = new StringBuilder("SELECT srvservicios.CSRVCOD, srvservicios.CSRVDES, hicdetservicio.MRECOMENDA FROM srvservicios, hicdetservicio WHERE srvservicios.CSRVCOD = hicdetservicio.CSRVCOD AND hicdetservicio.CCLAVE = '");
            sQuery.Append(Clave);
            sQuery.Append("'");            
            using (OLEDBDAC oDAC = new OLEDBDAC(sConnection))
            {
                dt = oDAC.GetDataTable(sQuery.ToString(), null, false);
                sServices.Append("\"");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sServices.Append(dt.Rows[i][0].ToString().Trim());
                    sServices.Append(" ");
                    sServices.Append(dt.Rows[i][1].ToString().Trim());
                    if (!string.IsNullOrEmpty(dt.Rows[i][2].ToString().Trim()))
                    {
                        sServices.Append(" RECOMENDACIONES: ");
                        sServices.Append(dt.Rows[i][2].ToString().Trim());
                    }                    
                    sServices.Append("\n");
                }
                sServices.Append("\"");
            }
            dt.Dispose();
            dt = null;
            sQuery = null;
            GC.Collect();
            return sServices.ToString();
        }

        //private string GetDocumentation(DateTime iDate, string Patient)
        private string GetDocumentation(string Clave)
        {
            DataTable dt = new DataTable();
            StringBuilder sDocumentation = new StringBuilder();
            StringBuilder sQuery = new StringBuilder("SELECT hicetapa.cetades, hicdetalle.mtexto FROM hicetapa, hicdetalle WHERE hicdetalle.cetacod = hicetapa.cetacod AND hicdetalle.cclave = '");
            sQuery.Append(Clave);
            sQuery.Append("'");            
            sQuery.Append(" AND hicdetalle.ctipcitcod = hicetapa.ctipcitcod ORDER BY norden");
            using (OLEDBDAC oDAC = new OLEDBDAC(sConnection))
            {
                dt = oDAC.GetDataTable(sQuery.ToString(), null, false);                
                sDocumentation.Append("\"");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sDocumentation.Append(dt.Rows[i][0].ToString().Trim());
                    sDocumentation.Append("\n");
                    sDocumentation.Append(dt.Rows[i][1].ToString().Trim().Replace("\"","'"));                    
                    sDocumentation.Append("\n");
                }
                sDocumentation.Append(this.GetParametersDocumentation(Clave));
                sDocumentation.Append("\"");
            }            
            dt.Dispose();
            dt = null;
            sQuery = null;
            GC.Collect();
            return sDocumentation.ToString();                                    
        }

        private string GetParametersDocumentation(string Clave)
        {
            DataTable dt = new DataTable();
            StringBuilder sDocumentation = new StringBuilder();
            StringBuilder sQuery = new StringBuilder("SELECT cetapardes, mvalor from hicetapardef, hicdetparam WHERE hicdetparam.cetaparcod = hicetapardef.cetaparcod");
            sQuery.Append(" AND hicdetparam.cclave = '");
            sQuery.Append(Clave);
            sQuery.Append("'");
            using (OLEDBDAC oDAC = new OLEDBDAC(sConnection))
            {
                dt = oDAC.GetDataTable(sQuery.ToString(), null, false);                
                if (dt.Rows.Count > 0)
                {
                    sDocumentation.Append("VARIABLES: \n");
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {                        
                        sDocumentation.Append(dt.Rows[i][0].ToString().Trim().Replace("\"", "'") + ": " + dt.Rows[i][1].ToString());
                        sDocumentation.Append("\n");
                    }                
                }                
            }
            dt.Dispose();
            dt = null;
            sQuery = null;
            GC.Collect();
            return sDocumentation.ToString();     
        }

        //private string GetPrescriptions(DateTime iDate, string Patient)
        private string GetPrescriptions(string Clave)
        {
            DataTable dt = new DataTable();
            StringBuilder sPrescriptions = new StringBuilder();
            StringBuilder sQuery = new StringBuilder("SELECT hictipoprescr.ctippredes, hicdetprescr.mprescripcion from hictipoprescr, hicdetprescr WHERE hicdetprescr.ctipprecod = hictipoprescr.ctipprecod AND hicdetprescr.cclave = '");
            sQuery.Append(Clave);
            sQuery.Append("'");            
            using (OLEDBDAC oDAC = new OLEDBDAC(sConnection))
            {
                dt = oDAC.GetDataTable(sQuery.ToString(), null, false);
                sPrescriptions.Append("\"");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sPrescriptions.Append(dt.Rows[i][0].ToString().Trim());
                    sPrescriptions.Append("\n");
                    sPrescriptions.Append(dt.Rows[i][1].ToString().Trim().Replace("\"", "'"));
                    sPrescriptions.Append("\n");
                }
                sPrescriptions.Append("\"");
            }
            dt.Dispose();
            dt = null;
            sQuery = null;
            GC.Collect();
            return sPrescriptions.ToString();
        }


        public void Dispose()
        {            
            this.sConnection = string.Empty;
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
