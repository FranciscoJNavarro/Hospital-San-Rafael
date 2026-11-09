using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalSanRafael.Entities
{
    [Serializable]
    public class RegistroMedicamento
    {
        public Guid Id { get; set; }
        public string NombreMedicamento { get; set; }
        public double DosisMg { get; set; }
        public DateTime FechaHoraAdministracion { get; set; }
        public int FrecuenciaDiaria { get; set; }
        public bool RequiereReceta { get; set; }
        public DateTime FechaHoraRegistro { get; set; }
    }
}
