using System.Collections.Generic;

namespace HospitalSanRafael.Entities
{
    /// <summary>Resultado final de intentar guardar un registro (RF-04).</summary>
    public class ResultadoOperacion
    {
        public bool Exito { get; private set; }
        public string Mensaje { get; private set; }
        public RegistroMedicamento Registro { get; private set; }
        public IReadOnlyDictionary<string, string> Errores { get; private set; }

        public static ResultadoOperacion Exitoso(RegistroMedicamento registro, string mensaje)
        {
            return new ResultadoOperacion
            {
                Exito = true,
                Mensaje = mensaje,
                Registro = registro,
                Errores = new Dictionary<string, string>()
            };
        }

        public static ResultadoOperacion Fallido(IReadOnlyDictionary<string, string> errores, string mensaje)
        {
            return new ResultadoOperacion
            {
                Exito = false,
                Mensaje = mensaje,
                Errores = errores ?? new Dictionary<string, string>()
            };
        }

        /// <summary>Atajo para un fallo con un solo mensaje general (no ligado a un campo).</summary>
        public static ResultadoOperacion FallidoGeneral(string mensajeParaUsuario, string mensaje)
        {
            var errores = new Dictionary<string, string> { { CamposRegistro.General, mensajeParaUsuario } };
            return Fallido(errores, mensaje);
        }
    }
}
