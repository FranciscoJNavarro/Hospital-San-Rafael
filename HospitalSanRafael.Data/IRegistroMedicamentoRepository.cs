using System.Collections.Generic;
using HospitalSanRafael.Entities;

namespace HospitalSanRafael.Data
{
    /// <summary>
    /// Contrato de almacenamiento de registros. El resto del sistema depende de esta
    /// interfaz y no del XML, así se puede cambiar a SQL Server sin tocar otras capas.
    /// </summary>
    public interface IRegistroMedicamentoRepository
    {
        void Guardar(RegistroMedicamento registro);
        List<RegistroMedicamento> ObtenerTodos();
    }
}
