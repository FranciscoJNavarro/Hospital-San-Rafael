using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace HospitalSanRafael.Business.Logging
{
    /// <summary>
    /// Escribe los eventos en un archivo de texto por día: bitacora_AAAA-MM-DD.log
    /// Formato de cada línea: fecha | nivel | mensaje
    /// </summary>
    public class ArchivoLogger : ILogger
    {
        private const string NivelInfo = "INFO";
        private const string NivelAdvertencia = "WARN";
        private const string NivelError = "ERROR";

        private static readonly object Candado = new object();
        private readonly string _carpetaLogs;

        /// <param name="carpetaLogs">Ruta física de la carpeta; se configura en el Web.config.</param>
        public ArchivoLogger(string carpetaLogs)
        {
            if (string.IsNullOrWhiteSpace(carpetaLogs))
                throw new ArgumentException("La carpeta de logs es obligatoria.", "carpetaLogs");
            _carpetaLogs = carpetaLogs;
        }

        public void Info(string mensaje)
        {
            Escribir(NivelInfo, mensaje, null);
        }

        public void Advertencia(string mensaje)
        {
            Escribir(NivelAdvertencia, mensaje, null);
        }

        public void Error(string mensaje, Exception excepcion = null)
        {
            Escribir(NivelError, mensaje, excepcion);
        }

        private void Escribir(string nivel, string mensaje, Exception excepcion)
        {
            try
            {
                DateTime ahora = DateTime.Now;
                var linea = new StringBuilder();
                linea.AppendFormat(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd HH:mm:ss.fff} | {1,-5} | {2}",
                                   ahora, nivel, QuitarSaltosDeLinea(mensaje));

                // El detalle técnico de la excepción solo va al log, nunca a la pantalla del usuario
                if (excepcion != null)
                    linea.AppendLine().Append("    ").Append(excepcion);

                string rutaArchivo = Path.Combine(_carpetaLogs,
                    "bitacora_" + ahora.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".log");

                lock (Candado)
                {
                    Directory.CreateDirectory(_carpetaLogs);
                    File.AppendAllText(rutaArchivo, linea.ToString() + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch (Exception errorAlEscribir)
            {
                // Una falla del log nunca debe detener el registro de un medicamento
                Trace.TraceError("No se pudo escribir en el log: " + errorAlEscribir.Message);
            }
        }

        /// <summary>
        /// Evita "log forging": que alguien meta saltos de línea en un dato
        /// para crear líneas falsas dentro de la bitácora.
        /// </summary>
        private static string QuitarSaltosDeLinea(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return string.Empty;
            return texto.Replace("\r", " ").Replace("\n", " ");
        }
    }
}
