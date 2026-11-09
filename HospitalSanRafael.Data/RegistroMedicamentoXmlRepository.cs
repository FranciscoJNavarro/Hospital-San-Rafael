using System;
using System.Collections.Generic;
using HospitalSanRafael.Entities;

namespace HospitalSanRafael.Data
{
    /// <summary>Guarda los registros de administración en un archivo XML.</summary>
    public class RegistroMedicamentoXmlRepository : IRegistroMedicamentoRepository
    {
        private readonly AlmacenamientoXml<RegistroMedicamento> _almacenamiento;

        /// <param name="rutaArchivo">Ruta física del archivo; se configura en el Web.config.</param>
        public RegistroMedicamentoXmlRepository(string rutaArchivo)
        {
            _almacenamiento = new AlmacenamientoXml<RegistroMedicamento>(rutaArchivo);
        }

        public void Guardar(RegistroMedicamento registro)
        {
            if (registro == null) throw new ArgumentNullException("registro");
            _almacenamiento.Modificar(registros => registros.Add(registro));
        }

        public List<RegistroMedicamento> ObtenerTodos()
        {
            return _almacenamiento.LeerTodos();
        }
    }
}
