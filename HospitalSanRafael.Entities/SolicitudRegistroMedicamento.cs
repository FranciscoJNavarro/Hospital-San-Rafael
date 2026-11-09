using System;

namespace HospitalSanRafael.Entities
{
    /// <summary>
    /// Datos tal como llegan del formulario, todavía como texto y sin validar.
    /// La capa de negocio es la encargada de convertirlos a sus tipos reales.
    /// </summary>
    [Serializable]
    public class SolicitudRegistroMedicamento
    {
        public string NombreMedicamento { get; set; }
        public string DosisMg { get; set; }
        public string FechaHoraAdministracion { get; set; }
        public string FrecuenciaDiaria { get; set; }

        /// <summary>null = el usuario no eligió ninguna opción (no hay valor por defecto ambiguo).</summary>
        public bool? RequiereReceta { get; set; }
    }
}
