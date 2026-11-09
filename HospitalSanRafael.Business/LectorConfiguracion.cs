using System.Configuration;
using System.Globalization;

namespace HospitalSanRafael.Business
{
    /// <summary>
    /// Lee valores de &lt;appSettings&gt; del Web.config. Si una clave falta o tiene un
    /// formato inválido lanza una excepción clara (fail fast) en lugar de usar un valor equivocado.
    /// </summary>
    public static class LectorConfiguracion
    {
        public static string LeerTexto(string clave)
        {
            string valor = ConfigurationManager.AppSettings[clave];
            if (string.IsNullOrWhiteSpace(valor))
                throw new ConfigurationErrorsException("Falta la clave '" + clave + "' en <appSettings> del Web.config.");
            return valor.Trim();
        }

        public static int LeerEntero(string clave)
        {
            int resultado;
            if (!int.TryParse(LeerTexto(clave), NumberStyles.None, CultureInfo.InvariantCulture, out resultado))
                throw new ConfigurationErrorsException("La clave '" + clave + "' del Web.config debe ser un número entero.");
            return resultado;
        }

        public static double LeerDecimal(string clave)
        {
            double resultado;
            if (!double.TryParse(LeerTexto(clave), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out resultado))
                throw new ConfigurationErrorsException("La clave '" + clave + "' del Web.config debe ser un número decimal (use punto).");
            return resultado;
        }
    }
}
