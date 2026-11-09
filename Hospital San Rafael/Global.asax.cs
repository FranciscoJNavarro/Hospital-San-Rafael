using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using Hospital_San_Rafael.Infraestructura;

namespace Hospital_San_Rafael
{
    /// <summary>Eventos globales de la aplicación: arranque, roles del usuario y errores no controlados.</summary>
    public class Global : HttpApplication
    {
        private const string Desconocido = "(desconocido)";

        protected void Application_Start(object sender, EventArgs e)
        {
            FabricaServicios.Logger.Info("Aplicación iniciada.");
        }

        protected void Application_End(object sender, EventArgs e)
        {
            FabricaServicios.Logger.Info("Aplicación detenida.");
        }

        /// <summary>
        /// El rol del usuario viaja cifrado dentro del ticket de autenticación (UserData).
        /// Aquí se lo asignamos al usuario para que funcionen las reglas &lt;allow roles="..."&gt; del Web.config.
        /// </summary>
        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var identidad = Context.User != null ? Context.User.Identity as FormsIdentity : null;
            if (identidad == null || !identidad.IsAuthenticated)
                return;

            string[] roles = (identidad.Ticket.UserData ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            Context.User = new GenericPrincipal(identidad, roles);
        }

        /// <summary>
        /// Atrapa cualquier excepción que no se haya controlado en las páginas y la registra en el log.
        /// Después, el Web.config (customErrors) muestra la página de error genérica sin detalles técnicos.
        /// </summary>
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception excepcion = Server.GetLastError();
            if (excepcion == null)
                return;

            string url = Context != null ? Request.RawUrl : Desconocido;
            string usuario = Context != null && Context.User != null ? Context.User.Identity.Name : Desconocido;

            try
            {
                var excepcionHttp = excepcion as HttpException;
                if (excepcionHttp != null && excepcionHttp.GetHttpCode() == 404)
                    FabricaServicios.Logger.Advertencia("Página no encontrada: " + url + ". Usuario=" + usuario);
                else
                    FabricaServicios.Logger.Error("Error no controlado en " + url + ". Usuario=" + usuario, excepcion);
            }
            catch (Exception errorAlRegistrar)
            {
                // Si la configuración del log está dañada, al menos queda rastro en el Trace de .NET
                Trace.TraceError("No se pudo registrar el error: " + errorAlRegistrar.Message);
            }
        }
    }
}
