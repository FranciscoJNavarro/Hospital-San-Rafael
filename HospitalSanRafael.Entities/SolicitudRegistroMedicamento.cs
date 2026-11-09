using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalSanRafael.Entities
{
    [Serializable]
    public class SolicitudRegistroMedicamento
    {
        public string NombreMedicamento { get; set; }
        public string DosisMg { get; set; }
        public string FechaHoraAdministracion { get; set; }
        public string FrecuenciaDiaria { get; set; }
        public bool? RequiereReceta { get; set; }
    }
}