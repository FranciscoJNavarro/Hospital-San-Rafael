using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalSanRafael.Entities
{
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
    }
}
