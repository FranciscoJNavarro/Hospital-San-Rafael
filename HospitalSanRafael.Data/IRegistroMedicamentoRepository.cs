using HospitalSanRafael.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalSanRafael.Data
{
    public interface IRegistroMedicamentoRepository
    {
        void Guardar(RegistroMedicamento registro);
        List<RegistroMedicamento> ObtenerTodos();
    }
}
