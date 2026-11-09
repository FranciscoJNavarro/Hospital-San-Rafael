using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Hospital_San_Rafael.Infraestructura;
using HospitalSanRafael.Business;
using HospitalSanRafael.Entities;

namespace Hospital_San_Rafael
{
    /// <summary>
    /// Pantalla del rol Enfermero(a): captura (RF-01), validación (RF-02),
    /// confirmación (RF-03) y resultado (RF-04) del registro de un medicamento.
    /// Esta clase solo se encarga de la presentación; las reglas viven en la capa Business.
    /// </summary>
    public partial class RegistrarMedicamento : Page
    {
        private const string ClaveSolicitudPendiente = "SolicitudPendiente";
        private const string FormatoFechaHtml = "yyyy-MM-ddTHH:mm";
        private const string FormatoFechaPantalla = "dd/MM/yyyy HH:mm";
        private const string FormatoFechaHoraRegistro = "dd/MM/yyyy HH:mm:ss";

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Liga el ViewState al usuario autenticado: un formulario falsificado desde otro sitio
            // (ataque CSRF) no puede enviarse en nombre de la enfermera.
            ViewStateUserKey = User.Identity.Name;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ConfigurarValidacionesDelFormulario();
            litUsuario.Text = User.Identity.Name;

            if (!IsPostBack)
                litMensajeConfirmacion.Text = RegistroMedicamentoService.MensajeConfirmacion;
        }

        // ---------- Paso 1: validar y mostrar resumen (RF-02, RF-03) ----------
        protected void btnRevisar_Click(object sender, EventArgs e)
        {
            LimpiarErrores();

            // Si el navegador tenía JavaScript deshabilitado, aquí se vuelven a evaluar los validadores
            if (!Page.IsValid)
                return;

            SolicitudRegistroMedicamento solicitud = LeerFormulario();
            ResultadoValidacion validacion = CrearServicio().Validar(solicitud);

            if (!validacion.EsValido)
            {
                MostrarErrores(validacion.Errores);
                return;
            }

            ViewState[ClaveSolicitudPendiente] = solicitud;
            MostrarDatosRegistro(validacion.RegistroValidado,
                                 litResNombre, litResDosis, litResFecha, litResFrecuencia, litResReceta);
            MostrarPaso(pnlConfirmacion);
        }

        // ---------- Paso 2: confirmar y guardar (RF-03, RF-04) ----------
        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            LimpiarErrores();
            var solicitud = ViewState[ClaveSolicitudPendiente] as SolicitudRegistroMedicamento;

            if (solicitud == null)
            {
                // Evita registros duplicados por doble clic o recarga de página
                MostrarErrorGeneral("No hay un registro pendiente de confirmar. Capture los datos nuevamente.");
                MostrarPaso(pnlFormulario);
                return;
            }

            ResultadoOperacion resultado = CrearServicio().Registrar(solicitud, true, User.Identity.Name);
            ViewState.Remove(ClaveSolicitudPendiente);

            if (!resultado.Exito)
            {
                MostrarErrores(resultado.Errores);
                MostrarPaso(pnlFormulario);
                return;
            }

            RegistroMedicamento registro = resultado.Registro;
            litMensajeExito.Text = resultado.Mensaje;
            litExFolio.Text = registro.Id.ToString().ToUpperInvariant();
            MostrarDatosRegistro(registro, litExNombre, litExDosis, litExFecha, litExFrecuencia, litExReceta);
            litExRegistradoPor.Text = registro.RegistradoPor;
            litExRegistro.Text = registro.FechaHoraRegistro.ToString(FormatoFechaHoraRegistro, CultureInfo.CurrentCulture);

            MostrarPaso(pnlExito);
        }

        protected void btnCorregir_Click(object sender, EventArgs e)
        {
            ViewState.Remove(ClaveSolicitudPendiente);
            MostrarPaso(pnlFormulario);   // los campos conservan lo capturado
        }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            Response.Redirect(Request.RawUrl, false);
        }

        protected void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            FabricaServicios.Logger.Info("Cierre de sesión. Usuario=" + User.Identity.Name);
            FormsAuthentication.SignOut();
            Session.Abandon();
            Response.Redirect(FormsAuthentication.LoginUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        // ---------- Configuración del formulario ----------

        /// <summary>
        /// Los límites del front-end salen de la misma fuente que usa el back-end (Web.config),
        /// así nunca quedan con valores distintos entre sí.
        /// </summary>
        private void ConfigurarValidacionesDelFormulario()
        {
            ReglasNegocio reglas = FabricaServicios.Reglas;

            string dosisMaxima = reglas.DosisMaximaMg.ToString(CultureInfo.InvariantCulture);
            string frecuenciaMinima = reglas.FrecuenciaMinimaDiaria.ToString(CultureInfo.InvariantCulture);
            string frecuenciaMaxima = reglas.FrecuenciaMaximaDiaria.ToString(CultureInfo.InvariantCulture);
            string ventanaHoras = reglas.VentanaHorasPasado.ToString(CultureInfo.InvariantCulture);

            txtNombre.MaxLength = reglas.LongitudMaximaNombre;
            revNombre.ValidationExpression = ValidadorRegistroMedicamento.PatronNombreMedicamento;

            txtDosis.Attributes["max"] = dosisMaxima;
            cvDosisMaxima.ValueToCompare = dosisMaxima;
            cvDosisMaxima.ErrorMessage = "❌ La dosis no puede exceder " + dosisMaxima + " mg (límite de seguridad).";

            txtFrecuencia.Attributes["min"] = frecuenciaMinima;
            txtFrecuencia.Attributes["max"] = frecuenciaMaxima;
            rvFrecuencia.MinimumValue = frecuenciaMinima;
            rvFrecuencia.MaximumValue = frecuenciaMaxima;
            rvFrecuencia.ErrorMessage = "❌ La frecuencia diaria debe ser un número entero entre "
                                        + frecuenciaMinima + " y " + frecuenciaMaxima + ".";

            // El calendario no deja elegir fechas futuras; la ventana de horas la usa validaciones.js
            txtFechaHora.Attributes["max"] = DateTime.Now.ToString(FormatoFechaHtml, CultureInfo.InvariantCulture);
            cvFechaHora.Attributes["data-ventana-horas"] = ventanaHoras;
            cvFechaHora.ErrorMessage = "❌ La fecha y hora no puede ser futura ni tener más de " + ventanaHoras + " horas en el pasado.";
        }

        private static RegistroMedicamentoService CrearServicio()
        {
            return FabricaServicios.CrearRegistroMedicamentoService();
        }

        private SolicitudRegistroMedicamento LeerFormulario()
        {
            return new SolicitudRegistroMedicamento
            {
                NombreMedicamento = txtNombre.Text,
                DosisMg = txtDosis.Text,
                FechaHoraAdministracion = txtFechaHora.Text,
                FrecuenciaDiaria = txtFrecuencia.Text,
                RequiereReceta = LeerOpcionReceta()
            };
        }

        /// <returns>true/false según la opción elegida, o null si no se eligió ninguna.</returns>
        private bool? LeerOpcionReceta()
        {
            bool valor;
            return bool.TryParse(rblReceta.SelectedValue, out valor) ? valor : (bool?)null;
        }

        // ---------- Utilidades de presentación ----------

        private void MostrarPaso(Panel panelVisible)
        {
            pnlFormulario.Visible = panelVisible == pnlFormulario;
            pnlConfirmacion.Visible = panelVisible == pnlConfirmacion;
            pnlExito.Visible = panelVisible == pnlExito;
        }

        /// <summary>Llena los datos del medicamento; lo usan tanto el resumen (RF-03) como el mensaje de éxito (RF-04).</summary>
        private static void MostrarDatosRegistro(RegistroMedicamento registro, Literal nombre, Literal dosis,
                                                 Literal fechaHora, Literal frecuencia, Literal receta)
        {
            nombre.Text = registro.NombreMedicamento;
            dosis.Text = FormatearDosis(registro.DosisMg);
            fechaHora.Text = registro.FechaHoraAdministracion.ToString(FormatoFechaPantalla, CultureInfo.CurrentCulture) + " hrs";
            frecuencia.Text = FormatearFrecuencia(registro.FrecuenciaDiaria);
            receta.Text = registro.RequiereReceta ? "Sí" : "No";
        }

        private void LimpiarErrores()
        {
            lblErrNombre.Text = lblErrDosis.Text = lblErrFechaHora.Text =
                lblErrFrecuencia.Text = lblErrReceta.Text = string.Empty;
            pnlErrorGeneral.Visible = false;
        }

        private void MostrarErrores(IReadOnlyDictionary<string, string> errores)
        {
            var etiquetasPorCampo = new Dictionary<string, Label>
            {
                { CamposRegistro.NombreMedicamento, lblErrNombre },
                { CamposRegistro.DosisMg, lblErrDosis },
                { CamposRegistro.FechaHoraAdministracion, lblErrFechaHora },
                { CamposRegistro.FrecuenciaDiaria, lblErrFrecuencia },
                { CamposRegistro.RequiereReceta, lblErrReceta }
            };

            foreach (KeyValuePair<string, string> error in errores)
            {
                Label etiqueta;
                if (etiquetasPorCampo.TryGetValue(error.Key, out etiqueta))
                    etiqueta.Text = "❌ " + Server.HtmlEncode(error.Value);   // Label no codifica HTML por sí sola
                else
                    MostrarErrorGeneral(error.Value);
            }

            if (!pnlErrorGeneral.Visible)
                MostrarErrorGeneral("El registro no se guardó. Revise los campos marcados.");
        }

        private void MostrarErrorGeneral(string mensaje)
        {
            litErrorGeneral.Text = mensaje;
            pnlErrorGeneral.Visible = true;
        }

        private static string FormatearDosis(double dosis)
        {
            return dosis.ToString("0.###", CultureInfo.InvariantCulture) + " mg";
        }

        private static string FormatearFrecuencia(int vecesAlDia)
        {
            return vecesAlDia == 1 ? "1 vez al día" : vecesAlDia + " veces al día";
        }
    }
}
