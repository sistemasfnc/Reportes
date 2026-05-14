using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAC;
using Entity;
using System.Data;

namespace Facade
{
    /// <summary>
    /// Objeto fachada para manejar la capa de datos de las glosas
    /// </summary>
    public class FacadeGlosa : IDisposable
    {
        /// <summary>
        /// Cadena de conexión a la base de datos
        /// </summary>
        public string sConnection { get; set; }

        /// <summary>
        /// Constructor del objeto
        /// </summary>
        public FacadeGlosa()
        {

        }

        /// <summary>
        /// Método para insertar o editar la glosa y sus conceptos en la base de datos
        /// </summary>
        /// <param name="oEntity">Objeto que contiene la información de la glosa</param>
        /// <returns>ID de la glosa</returns>
        public int CreateOrEdit(Glosa oEntity, bool isUpdate = true)
        {
            //Inicializa objeto de glosa para la capa de datos
            using (GlosaDAC oDAC = new GlosaDAC(this.sConnection))
            {
                //Realizada llamado de método Insert para crear la glosa
                if (oEntity.id == 0)
                {
                    return oDAC.Insert(oEntity);
                }
                //Realiza llamado de método Update para actualizar la glosa
                else
                {
                    oDAC.Update(oEntity, isUpdate);
                    return oEntity.id;
                }                    
            }
        }

        /// <summary>
        /// Método que retorna los conceptos de devolución de las glosas
        /// </summary>
        /// <param name="sGroup">Parámetro opcional grupo de concepto</param>
        /// <returns>Lista genérica de conceptos</returns>
        public List<ConceptoGlosa> GetConcepts(string sGroup = "")
        {
            //Inicializa objeto de glosa para la capa de datos
            using (GlosaDAC oDAC = new GlosaDAC(this.sConnection))
            {
                //Realizada llamado de método obtener conceptos para retornar el listado de conceptos
                return oDAC.GetConcepts(sGroup);
            }
        }

        /// <summary>
        /// Método que retorna las glosas
        /// </summary>
        /// <param name="oEntity">Objeto que contiene información de la glosa</param>
        /// <returns>Lista genérica con las glosas</returns>
        public List<Glosa> GetComments(Glosa oEntity)
        {
            //Inicializa objeto de glosa para la capa de datos
            using (GlosaDAC oDAC = new GlosaDAC(this.sConnection))
            {
                //Realizada llamado de método obtener glosas para retornar el listado de glosas
                return oDAC.GetComments(oEntity);
            }
        }

        /// <summary>
        /// Método que obtiene una glosa
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        public Glosa GetComment(Glosa oEntity)
        {
            //Inicializa objeto de glosa para la capa de datos
            using (GlosaDAC oDAC = new GlosaDAC(this.sConnection))
            {
                //Realizada llamado de método obtener la glosa
                return oDAC.GetComment(oEntity);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oEntity"></param>
        /// <returns></returns>
        public DataTable GetCommentViewData(Glosa oEntity)
        {
            //Inicializa objeto de glosa para la capa de datos
            using (GlosaDAC oDAC = new GlosaDAC(this.sConnection))
            {
                //Realizada llamado de método obtener la vista de glosas
                return oDAC.GetCommentViewData(oEntity);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iComment"></param>
        /// <returns></returns>
        public List<ConceptoGlosa> GetCoceptsByComment(int iComment = 0)
        {
            //Inicializa objeto de glosa para la capa de datos
            using (GlosaDAC oDAC = new GlosaDAC(this.sConnection))
            {
                //Realizada llamado de método obtener la vista de glosas
                return oDAC.GetCoceptsByComment(iComment);
            }
        }

        /// <summary>
        /// Método que obtiene la lista de respuestas por concepto por glosa
        /// </summary>
        /// <param name="oResponse"></param>
        /// <returns></returns>
        public List<RespuestaGlosa> GetConceptResponse(RespuestaGlosa oResponse)
        {
            //Inicializa objeto de glosa para la capa de datos
            using (GlosaDAC oDAC = new GlosaDAC(this.sConnection))
            {
                //Realizada llamado de método obtener la lista de respuestas por concepto
                return oDAC.GetConceptResponse(oResponse);
            }
        }

        /// <summary>
        /// Método que inserta la respuesta a los conceptos por glosa
        /// </summary>
        /// <param name="oResponse"></param>
        public void InsertConceptResponse(RespuestaGlosa oResponse)
        {
            using (GlosaDAC oDAC = new GlosaDAC(this.sConnection))
            {
                oDAC.DeleteResponses(oResponse);
                oDAC.InsertConceptResponse(oResponse);
            }
        }

        /// <summary>
        /// Método Dispose libera objetos de la memoria
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
