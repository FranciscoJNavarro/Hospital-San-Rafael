using HospitalSanRafael.Business;
using HospitalSanRafael.Data;
using HospitalSanRafael.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Hospital_San_Rafael
{
    public partial class RegistrarMedicamento : Page
    {
        private const string ClaveSolicitud = "SolicitudPendiente";
        private static readonly CultureInfo CulturaMx = new CultureInfo("es-MX");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // El calendario no deja elegir fechas futuras (la validación real está en Business)
                txtFechaHora.Attributes["max"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm");
                litMensajeConfirmacion.Text = RegistroMedicamentoService.MensajeConfirmacion;
            }
        }

        private RegistroMedicamentoService CrearServicio()
        {
            string ruta = Server.MapPath("~/App_Data/registros_medicamentos.xml");
            return new RegistroMedicamentoService(new RegistroMedicamentoXmlRepository(ruta), new RelojSistema());
        }

        private SolicitudRegistroMedicamento LeerFormulario()
        {
            bool? receta = null;
            if (rblReceta.SelectedValue == "true") receta = true;
            else if (rblReceta.SelectedValue == "false") receta = false;

            return new SolicitudRegistroMedicamento
            {
                NombreMedicamento = txtNombre.Text,
                DosisMg = txtDosis.Text,
                FechaHoraAdministracion = txtFechaHora.Text,
                FrecuenciaDiaria = txtFrecuencia.Text,
                RequiereReceta = receta
            };
        }

        // ---------- Paso 1: validar y mostrar resumen ----------
        protected void btnRevisar_Click(object sender, EventArgs e)
        {
            LimpiarErrores();
            SolicitudRegistroMedicamento solicitud = LeerFormulario();
            ResultadoValidacion validacion = CrearServicio().Validar(solicitud);

            if (!validacion.EsValido)
            {
                MostrarErrores(validacion.Errores);
                return;
            }

            ViewState[ClaveSolicitud] = solicitud;
            RegistroMedicamento r = validacion.RegistroValidado;

            litResNombre.Text = r.NombreMedicamento;
            litResDosis.Text = FormatearDosis(r.DosisMg);
            litResFecha.Text = FormatearFecha(r.FechaHoraAdministracion);
            litResFrecuencia.Text = r.FrecuenciaDiaria + " vez/veces al día";
            litResReceta.Text = r.RequiereReceta ? "Sí" : "No";

            MostrarPaso(pnlConfirmacion);
        }

        // ---------- Paso 2: confirmar y guardar ----------
        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            LimpiarErrores();
            var solicitud = ViewState[ClaveSolicitud] as SolicitudRegistroMedicamento;

            if (solicitud == null)
            {
                // Evita registros duplicados por doble clic o recarga de página
                MostrarErrorGeneral("No hay un registro pendiente de confirmar. Capture los datos nuevamente.");
                MostrarPaso(pnlFormulario);
                return;
            }

            ResultadoOperacion resultado = CrearServicio().Registrar(solicitud, true);
            ViewState.Remove(ClaveSolicitud);

            if (!resultado.Exito)
            {
                MostrarErrores(resultado.Errores);
                MostrarPaso(pnlFormulario);
                return;
            }

            RegistroMedicamento r = resultado.Registro;
            litMensajeExito.Text = resultado.Mensaje;
            litExFolio.Text = r.Id.ToString().ToUpperInvariant();
            litExNombre.Text = r.NombreMedicamento;
            litExDosis.Text = FormatearDosis(r.DosisMg);
            litExFecha.Text = FormatearFecha(r.FechaHoraAdministracion);
            litExFrecuencia.Text = r.FrecuenciaDiaria + " vez/veces al día";
            litExReceta.Text = r.RequiereReceta ? "Sí" : "No";
            litExRegistro.Text = r.FechaHoraRegistro.ToString("dd/MM/yyyy HH:mm:ss", CulturaMx);

            MostrarPaso(pnlExito);
        }

        protected void btnCorregir_Click(object sender, EventArgs e)
        {
            ViewState.Remove(ClaveSolicitud);
            MostrarPaso(pnlFormulario);   // los campos conservan lo capturado
        }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            Response.Redirect(Request.RawUrl, false);
        }

        // ---------- Utilidades de presentación ----------
        private void MostrarPaso(Panel panel)
        {
            pnlFormulario.Visible = panel == pnlFormulario;
            pnlConfirmacion.Visible = panel == pnlConfirmacion;
            pnlExito.Visible = panel == pnlExito;
        }

        private void LimpiarErrores()
        {
            lblErrNombre.Text = lblErrDosis.Text = lblErrFechaHora.Text =
                lblErrFrecuencia.Text = lblErrReceta.Text = string.Empty;
            pnlErrorGeneral.Visible = false;
        }

        private void MostrarErrores(IReadOnlyDictionary<string, string> errores)
        {
            var etiquetas = new Dictionary<string, Label>
            {
                { CamposRegistro.NombreMedicamento, lblErrNombre },
                { CamposRegistro.DosisMg, lblErrDosis },
                { CamposRegistro.FechaHoraAdministracion, lblErrFechaHora },
                { CamposRegistro.FrecuenciaDiaria, lblErrFrecuencia },
                { CamposRegistro.RequiereReceta, lblErrReceta }
            };

            foreach (var error in errores)
            {
                Label etiqueta;
                if (etiquetas.TryGetValue(error.Key, out etiqueta))
                    etiqueta.Text = "❌ " + Server.HtmlEncode(error.Value);
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

        private static string FormatearFecha(DateTime fecha)
        {
            return fecha.ToString("dd/MM/yyyy HH:mm", CulturaMx) + " hrs";
        }
    }
}