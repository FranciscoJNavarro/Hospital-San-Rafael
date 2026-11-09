using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalSanRafael.Entities
{
    public class ResultadoValidacion
    {
        private readonly Dictionary<string, string> _errores = new Dictionary<string, string>();

        public bool EsValido { get { return _errores.Count == 0; } }

        public IReadOnlyDictionary<string, string> Errores { get { return _errores; } }

        /// <summary>Entidad ya convertida a tipos reales. Solo tiene valor si EsValido = true.</summary>
        public RegistroMedicamento RegistroValidado { get; set; }

        public void AgregarError(string campo, string mensaje)
        {
            if (!_errores.ContainsKey(campo))
                _errores.Add(campo, mensaje);
        }
    }
}
