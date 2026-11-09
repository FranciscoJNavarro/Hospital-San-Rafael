using System;

namespace HospitalSanRafael.Business.Seguridad
{
    /// <summary>Parámetros de seguridad del inicio de sesión, leídos del Web.config.</summary>
    public class ConfiguracionSeguridad
    {
        /// <summary>Intentos fallidos permitidos antes de bloquear temporalmente la cuenta.</summary>
        public int MaxIntentosFallidos { get; private set; }

        /// <summary>Minutos que la cuenta permanece bloqueada.</summary>
        public int MinutosBloqueo { get; private set; }

        public ConfiguracionSeguridad(int maxIntentosFallidos, int minutosBloqueo)
        {
            if (maxIntentosFallidos < 1)
                throw new ArgumentOutOfRangeException("maxIntentosFallidos", "Debe permitirse al menos un intento.");
            if (minutosBloqueo < 1)
                throw new ArgumentOutOfRangeException("minutosBloqueo", "El bloqueo debe durar al menos un minuto.");

            MaxIntentosFallidos = maxIntentosFallidos;
            MinutosBloqueo = minutosBloqueo;
        }

        public static ConfiguracionSeguridad CargarDesdeConfiguracion()
        {
            return new ConfiguracionSeguridad(
                LectorConfiguracion.LeerEntero("MaxIntentosFallidosLogin"),
                LectorConfiguracion.LeerEntero("MinutosBloqueoLogin"));
        }
    }
}
