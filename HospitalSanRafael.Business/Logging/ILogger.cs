using System;

namespace HospitalSanRafael.Business.Logging
{
    /// <summary>
    /// Contrato para registrar eventos del sistema. Permite cambiar el destino
    /// (archivo, base de datos, visor de eventos) sin tocar el resto del código.
    /// </summary>
    public interface ILogger
    {
        /// <summary>Eventos normales de operación (inicio de la app, registro guardado, inicio de sesión).</summary>
        void Info(string mensaje);

        /// <summary>Situaciones anómalas pero controladas (validación rechazada, acceso denegado).</summary>
        void Advertencia(string mensaje);

        /// <summary>Errores y excepciones.</summary>
        void Error(string mensaje, Exception excepcion = null);
    }
}
