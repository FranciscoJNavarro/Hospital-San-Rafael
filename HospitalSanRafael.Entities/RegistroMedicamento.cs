using System;

namespace HospitalSanRafael.Entities
{
    /// <summary>Administración de un medicamento ya validada y lista para guardarse.</summary>
    [Serializable]
    public class RegistroMedicamento
    {
        /// <summary>Folio único del registro.</summary>
        public Guid Id { get; set; }

        public string NombreMedicamento { get; set; }
        public double DosisMg { get; set; }
        public DateTime FechaHoraAdministracion { get; set; }
        public int FrecuenciaDiaria { get; set; }
        public bool RequiereReceta { get; set; }

        /// <summary>Momento en que se confirmó el registro en el sistema.</summary>
        public DateTime FechaHoraRegistro { get; set; }

        /// <summary>Usuario autenticado que confirmó el registro (trazabilidad).</summary>
        public string RegistradoPor { get; set; }
    }
}
