using System;
using System.Collections.Generic;
using System.Linq;
using HospitalSanRafael.Business.Logging;
using HospitalSanRafael.Data;
using HospitalSanRafael.Entities;

namespace HospitalSanRafael.Business
{
    /// <summary>
    /// Coordina el flujo de registro: validar (RF-02), confirmar (RF-03) y guardar (RF-04).
    /// </summary>
    public class RegistroMedicamentoService
    {
        public const string MensajeConfirmacion =
            "¿Confirma que los datos del medicamento son correctos? Esta acción quedará registrada.";

        private readonly IRegistroMedicamentoRepository _repositorio;
        private readonly IReloj _reloj;
        private readonly ILogger _logger;
        private readonly ValidadorRegistroMedicamento _validador;

        public RegistroMedicamentoService(IRegistroMedicamentoRepository repositorio, IReloj reloj,
                                          ReglasNegocio reglas, ILogger logger)
        {
            if (repositorio == null) throw new ArgumentNullException("repositorio");
            if (reloj == null) throw new ArgumentNullException("reloj");
            if (logger == null) throw new ArgumentNullException("logger");

            _repositorio = repositorio;
            _reloj = reloj;
            _logger = logger;
            _validador = new ValidadorRegistroMedicamento(reloj, reglas);
        }

        /// <summary>Paso 1 (RF-02): valida y regresa el registro convertido para mostrar el resumen (RF-03).</summary>
        public ResultadoValidacion Validar(SolicitudRegistroMedicamento solicitud)
        {
            ResultadoValidacion validacion = _validador.Validar(solicitud);

            // Solo se registran los NOMBRES de los campos con error, no lo que capturó el usuario
            if (!validacion.EsValido)
                _logger.Advertencia("Validación rechazada. Campos con error: " + UnirNombresDeCampos(validacion.Errores));

            return validacion;
        }

        /// <summary>Paso 2 (RF-03/RF-04): guarda solo si el usuario confirmó explícitamente.</summary>
        /// <param name="usuarioQueRegistra">Usuario autenticado; queda guardado en el registro para trazabilidad.</param>
        public ResultadoOperacion Registrar(SolicitudRegistroMedicamento solicitud, bool confirmadoPorUsuario,
                                            string usuarioQueRegistra)
        {
            if (!confirmadoPorUsuario)
            {
                _logger.Advertencia("Intento de registro sin confirmación. Usuario=" + usuarioQueRegistra);
                return ResultadoOperacion.FallidoGeneral(
                    "El registro requiere la confirmación explícita del usuario.", "El registro no fue confirmado.");
            }

            if (string.IsNullOrWhiteSpace(usuarioQueRegistra))
            {
                _logger.Advertencia("Intento de registro sin usuario identificado.");
                return ResultadoOperacion.FallidoGeneral(
                    "No se pudo identificar al usuario. Inicie sesión nuevamente.", "Usuario no identificado.");
            }

            // Se vuelve a validar: entre el resumen y la confirmación pudo pasar tiempo
            // (por ejemplo, la fecha pudo salir de la ventana de horas permitida).
            ResultadoValidacion validacion = Validar(solicitud);
            if (!validacion.EsValido)
                return ResultadoOperacion.Fallido(validacion.Errores, "El registro contiene errores. Corríjalos e intente de nuevo.");

            RegistroMedicamento registro = validacion.RegistroValidado;
            registro.Id = Guid.NewGuid();
            registro.FechaHoraRegistro = _reloj.Ahora;
            registro.RegistradoPor = usuarioQueRegistra;

            try
            {
                _repositorio.Guardar(registro);
            }
            catch (Exception ex)
            {
                // Al usuario se le da un mensaje claro; el detalle técnico solo va al log
                _logger.Error("Error al guardar el registro con folio " + registro.Id + ". Usuario=" + usuarioQueRegistra, ex);
                return ResultadoOperacion.FallidoGeneral(
                    "No fue posible guardar el registro. NO se registró la administración; avise al área de sistemas.",
                    "Error al guardar.");
            }

            _logger.Info(string.Format("Registro guardado. Folio={0}, Medicamento={1}, Dosis={2} mg, Usuario={3}",
                                       registro.Id, registro.NombreMedicamento, registro.DosisMg, usuarioQueRegistra));

            return ResultadoOperacion.Exitoso(registro, "El registro de administración se guardó correctamente.");
        }

        private static string UnirNombresDeCampos(IReadOnlyDictionary<string, string> errores)
        {
            return string.Join(", ", errores.Keys.ToArray());
        }
    }
}
