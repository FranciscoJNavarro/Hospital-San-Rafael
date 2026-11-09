using System;

namespace HospitalSanRafael.Entities
{
    /// <summary>
    /// Usuario del sistema. La contraseña NUNCA se guarda en texto plano:
    /// solo se guarda su hash PBKDF2 junto con la sal y el número de iteraciones.
    /// </summary>
    [Serializable]
    public class Usuario
    {
        public string NombreUsuario { get; set; }
        public string NombreCompleto { get; set; }

        /// <summary>Uno de los valores de <see cref="RolesUsuario"/>.</summary>
        public string Rol { get; set; }

        /// <summary>Hash PBKDF2-SHA256 de la contraseña, en Base64.</summary>
        public string HashContrasena { get; set; }

        /// <summary>Sal aleatoria usada para el hash, en Base64.</summary>
        public string Sal { get; set; }

        public int Iteraciones { get; set; }

        /// <summary>Permite desactivar una cuenta sin borrarla.</summary>
        public bool Activo { get; set; }
    }
}
