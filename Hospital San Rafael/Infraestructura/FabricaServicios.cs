using System.Web.Hosting;
using HospitalSanRafael.Business;
using HospitalSanRafael.Business.Logging;
using HospitalSanRafael.Business.Seguridad;
using HospitalSanRafael.Data;

namespace Hospital_San_Rafael.Infraestructura
{
    /// <summary>
    /// Único lugar donde se crean y conectan los objetos de las capas (composition root).
    /// Las páginas piden aquí lo que necesitan en vez de construirlo ellas mismas.
    /// Si mañana se cambia el XML por SQL Server, solo se modifica esta clase.
    /// </summary>
    public static class FabricaServicios
    {
        // IMPORTANTE: el orden importa, cada campo puede usar los que están declarados arriba de él.
        private static readonly ILogger LoggerAplicacion = new ArchivoLogger(ObtenerRutaFisica("CarpetaLogs"));
        private static readonly IReloj Reloj = new RelojSistema();
        private static readonly ReglasNegocio ReglasCargadas = ReglasNegocio.CargarDesdeConfiguracion();

        // Una sola instancia: guarda en memoria los intentos fallidos de inicio de sesión
        private static readonly ServicioAutenticacion AutenticacionCompartida = new ServicioAutenticacion(
            new UsuarioXmlRepository(ObtenerRutaFisica("RutaArchivoUsuarios")),
            ConfiguracionSeguridad.CargarDesdeConfiguracion(),
            Reloj,
            LoggerAplicacion);

        public static ILogger Logger
        {
            get { return LoggerAplicacion; }
        }

        public static ReglasNegocio Reglas
        {
            get { return ReglasCargadas; }
        }

        public static ServicioAutenticacion Autenticacion
        {
            get { return AutenticacionCompartida; }
        }

        public static RegistroMedicamentoService CrearRegistroMedicamentoService()
        {
            var repositorio = new RegistroMedicamentoXmlRepository(ObtenerRutaFisica("RutaArchivoRegistros"));
            return new RegistroMedicamentoService(repositorio, Reloj, ReglasCargadas, LoggerAplicacion);
        }

        /// <summary>Convierte una ruta virtual del Web.config (~/App_Data/...) en la ruta física del servidor.</summary>
        private static string ObtenerRutaFisica(string claveConfiguracion)
        {
            return HostingEnvironment.MapPath(LectorConfiguracion.LeerTexto(claveConfiguracion));
        }
    }
}
