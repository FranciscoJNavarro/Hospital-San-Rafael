using HospitalSanRafael.Data;
using HospitalSanRafael.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalSanRafael.Business
{
    public class RegistroMedicamentoService
    {
        public const string MensajeConfirmacion =
            "¿Confirma que los datos del medicamento son correctos? Esta acción quedará registrada.";

        private readonly IRegistroMedicamentoRepository _repositorio;
        private readonly IReloj _reloj;
        private readonly ValidadorRegistroMedicamento _validador;

        public RegistroMedicamentoService(IRegistroMedicamentoRepository repositorio, IReloj reloj)
        {
            if (repositorio == null) throw new ArgumentNullException("repositorio");
            if (reloj == null) throw new ArgumentNullException("reloj");

            _repositorio = repositorio;
            _reloj = reloj;
            _validador = new ValidadorRegistroMedicamento(reloj);
        }

        /// <summary>Paso 1 (RF-02): valida y regresa el resumen para mostrarlo (RF-03).</summary>
        public ResultadoValidacion Validar(SolicitudRegistroMedicamento solicitud)
        {
            return _validador.Validar(solicitud);
        }

        /// <summary>Paso 2 (RF-03/RF-04): guarda solo si el usuario confirmó explícitamente.</summary>
        public ResultadoOperacion Registrar(SolicitudRegistroMedicamento solicitud, bool confirmadoPorUsuario)
        {
            if (!confirmadoPorUsuario)
            {
                var errores = new Dictionary<string, string>
                {
                    { CamposRegistro.General, "El registro requiere la confirmación explícita del usuario." }
                };
                return ResultadoOperacion.Fallido(errores, "El registro no fue confirmado.");
            }

            // Se vuelve a validar: entre el resumen y la confirmación pudo pasar tiempo
            // (por ejemplo, la fecha pudo salir de la ventana de 24 horas).
            ResultadoValidacion validacion = _validador.Validar(solicitud);
            if (!validacion.EsValido)
                return ResultadoOperacion.Fallido(validacion.Errores, "El registro contiene errores. Corríjalos e intente de nuevo.");

            RegistroMedicamento registro = validacion.RegistroValidado;
            registro.Id = Guid.NewGuid();
            registro.FechaHoraRegistro = _reloj.Ahora;

            try
            {
                _repositorio.Guardar(registro);
            }
            catch (Exception)
            {
                var errores = new Dictionary<string, string>
                {
                    { CamposRegistro.General, "No fue posible guardar el registro. NO se registró la administración; avise al área de sistemas." }
                };
                return ResultadoOperacion.Fallido(errores, "Error al guardar.");
            }

            return ResultadoOperacion.Exitoso(registro, "El registro de administración se guardó correctamente.");
        }
    }
}
