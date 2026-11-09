using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using Hospital_San_Rafael.Infraestructura;
using HospitalSanRafael.Entities;

namespace Hospital_San_Rafael
{
    /// <summary>
    /// Inicio de sesión. Valida las credenciales con la capa Business y crea un ticket
    /// de autenticación cifrado que incluye el rol del usuario.
    /// </summary>
    public partial class Login : Page
    {
        private const string ParametroUrlDeRegreso = "ReturnUrl";

        protected void Page_Load(object sender, EventArgs e)
        {
            // Un usuario que YA inició sesión llega aquí cuando su rol no tiene permiso para la página que pidió
            bool fueRedirigidoPorFaltaDePermiso = !IsPostBack && Request.IsAuthenticated
                                                  && !string.IsNullOrEmpty(Request.QueryString[ParametroUrlDeRegreso]);
            if (fueRedirigidoPorFaltaDePermiso)
            {
                FabricaServicios.Logger.Advertencia("Acceso denegado por rol. Usuario=" + User.Identity.Name +
                                                    ", Página=" + Request.QueryString[ParametroUrlDeRegreso]);
                MostrarMensaje("Su usuario no tiene permiso para acceder a esa página. Inicie sesión con una cuenta autorizada.");
            }
        }

        protected void btnIniciarSesion_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            ResultadoAutenticacion resultado = FabricaServicios.Autenticacion.Autenticar(txtUsuario.Text, txtContrasena.Text);

            if (!resultado.Exito)
            {
                MostrarMensaje(resultado.Mensaje);
                return;
            }

            CrearCookieDeAutenticacion(resultado.Usuario);

            // GetRedirectUrl solo permite regresar a páginas de esta misma aplicación (evita "open redirect")
            Response.Redirect(FormsAuthentication.GetRedirectUrl(resultado.Usuario.NombreUsuario, false), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// El rol se guarda dentro del ticket cifrado (UserData). Global.asax lo lee en cada petición
        /// para aplicar las reglas de autorización del Web.config.
        /// </summary>
        private void CrearCookieDeAutenticacion(Usuario usuario)
        {
            DateTime ahora = DateTime.Now;
            var ticket = new FormsAuthenticationTicket(
                1,
                usuario.NombreUsuario,
                ahora,
                ahora.Add(FormsAuthentication.Timeout),
                false,
                usuario.Rol,
                FormsAuthentication.FormsCookiePath);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL,
                Path = FormsAuthentication.FormsCookiePath
            };

            Response.Cookies.Add(cookie);
        }

        private void MostrarMensaje(string mensaje)
        {
            litMensaje.Text = mensaje;
            pnlMensaje.Visible = true;
        }
    }
}
