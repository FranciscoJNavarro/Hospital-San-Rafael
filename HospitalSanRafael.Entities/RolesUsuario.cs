namespace HospitalSanRafael.Entities
{
    /// <summary>
    /// Roles definidos para el sistema. En esta versión solo el rol Enfermero
    /// tiene acceso a la pantalla de registro.
    /// </summary>
    public static class RolesUsuario
    {
        public const string Enfermero = "Enfermero";
        public const string Medico = "Medico";
        public const string JefeEnfermeria = "JefeEnfermeria";
    }
}
