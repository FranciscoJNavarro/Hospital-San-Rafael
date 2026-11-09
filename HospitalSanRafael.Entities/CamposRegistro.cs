namespace HospitalSanRafael.Entities
{
    /// <summary>
    /// Nombres de los campos del formulario. Se usan como llave para asociar
    /// cada mensaje de error con el campo que lo provocó (RF-04).
    /// </summary>
    public static class CamposRegistro
    {
        public const string NombreMedicamento = "NombreMedicamento";
        public const string DosisMg = "DosisMg";
        public const string FechaHoraAdministracion = "FechaHoraAdministracion";
        public const string FrecuenciaDiaria = "FrecuenciaDiaria";
        public const string RequiereReceta = "RequiereReceta";

        /// <summary>Errores que no pertenecen a un campo específico (por ejemplo, fallas al guardar).</summary>
        public const string General = "General";
    }
}
