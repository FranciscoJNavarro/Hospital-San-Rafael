using HospitalSanRafael.Entities;

namespace HospitalSanRafael.Data
{
    /// <summary>Contrato para consultar los usuarios del sistema.</summary>
    public interface IUsuarioRepository
    {
        /// <returns>El usuario, o null si no existe.</returns>
        Usuario ObtenerPorNombre(string nombreUsuario);
    }
}
