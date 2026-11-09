using System;
using System.Linq;
using HospitalSanRafael.Entities;

namespace HospitalSanRafael.Data
{
    /// <summary>Consulta los usuarios guardados en un archivo XML (solo hashes, nunca contraseñas).</summary>
    public class UsuarioXmlRepository : IUsuarioRepository
    {
        private readonly AlmacenamientoXml<Usuario> _almacenamiento;

        /// <param name="rutaArchivo">Ruta física del archivo; se configura en el Web.config.</param>
        public UsuarioXmlRepository(string rutaArchivo)
        {
            _almacenamiento = new AlmacenamientoXml<Usuario>(rutaArchivo);
        }

        public Usuario ObtenerPorNombre(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                return null;

            return _almacenamiento.LeerTodos().FirstOrDefault(usuario =>
                string.Equals(usuario.NombreUsuario, nombreUsuario, StringComparison.OrdinalIgnoreCase));
        }
    }
}
