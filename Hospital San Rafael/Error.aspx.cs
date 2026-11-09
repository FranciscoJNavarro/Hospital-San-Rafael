using System;
using System.Web.UI;

namespace Hospital_San_Rafael
{
    /// <summary>
    /// Página de error genérica configurada en customErrors del Web.config.
    /// El error ya fue registrado en el log por Global.asax (Application_Error).
    /// </summary>
    public partial class PaginaError : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Que el navegador sepa que hubo un error (no un 200 "todo bien") y que IIS no lo reemplace
            Response.StatusCode = 500;
            Response.TrySkipIisCustomErrors = true;
        }
    }
}
