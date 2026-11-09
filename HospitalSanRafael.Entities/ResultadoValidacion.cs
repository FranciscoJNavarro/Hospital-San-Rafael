using System.Collections.Generic;

namespace HospitalSanRafael.Entities
{
    /// <summary>Resultado de validar una solicitud: errores por campo o el registro ya convertido.</summary>
    public class ResultadoValidacion
    {
        private readonly Dictionary<string, string> _errores = new Dictionary<string, string>();

        public bool EsValido { get { return _errores.Count == 0; } }

        public IReadOnlyDictionary<string, string> Errores { get { return _errores; } }

        /// <summary>Entidad convertida a tipos reales. Solo tiene valor si EsValido = true.</summary>
        public RegistroMedicamento RegistroValidado { get; set; }

        /// <summary>Agrega un error. Si el campo ya tiene uno, se conserva el primero.</summary>
        public void AgregarError(string campo, string mensaje)
        {
            if (!_errores.ContainsKey(campo))
                _errores.Add(campo, mensaje);
        }
    }
}
