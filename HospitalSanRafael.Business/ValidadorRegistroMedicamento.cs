using HospitalSanRafael.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HospitalSanRafael.Business
{
    public class ValidadorRegistroMedicamento
    {
        public const double DosisMaximaMg = 5000;
        public const int FrecuenciaMinima = 1;
        public const int FrecuenciaMaxima = 24;
        public const int VentanaHorasPasado = 24;
        public const int LongitudMaximaNombre = 100;

        // Solo letras (incluye acentos, ñ, ü) separadas por un espacio
        private static readonly Regex RegexNombre =
            new Regex(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü]+( [A-Za-zÁÉÍÓÚáéíóúÑñÜü]+)*$");

        // Solo dígitos con punto decimal opcional: rechaza negativos, comas, notación científica, etc.
        private static readonly Regex RegexDecimal = new Regex(@"^\d+(\.\d+)?$");
        private static readonly Regex RegexEntero = new Regex(@"^\d+$");

        private static readonly string[] FormatosFecha =
        {
            "yyyy-MM-ddTHH:mm",      // formato del control datetime-local de HTML5
            "yyyy-MM-ddTHH:mm:ss",
            "dd/MM/yyyy HH:mm"
        };

        private readonly IReloj _reloj;

        public ValidadorRegistroMedicamento(IReloj reloj)
        {
            if (reloj == null) throw new ArgumentNullException("reloj");
            _reloj = reloj;
        }

        public ResultadoValidacion Validar(SolicitudRegistroMedicamento solicitud)
        {
            var resultado = new ResultadoValidacion();

            if (solicitud == null)
            {
                resultado.AgregarError(CamposRegistro.General, "No se recibieron datos del formulario.");
                return resultado;
            }

            string nombre;
            double dosis;
            DateTime fecha;
            int frecuencia;
            bool receta;

            string error;
            if ((error = ValidarNombre(solicitud.NombreMedicamento, out nombre)) != null)
                resultado.AgregarError(CamposRegistro.NombreMedicamento, error);

            if ((error = ValidarDosis(solicitud.DosisMg, out dosis)) != null)
                resultado.AgregarError(CamposRegistro.DosisMg, error);

            if ((error = ValidarFechaHora(solicitud.FechaHoraAdministracion, out fecha)) != null)
                resultado.AgregarError(CamposRegistro.FechaHoraAdministracion, error);

            if ((error = ValidarFrecuencia(solicitud.FrecuenciaDiaria, out frecuencia)) != null)
                resultado.AgregarError(CamposRegistro.FrecuenciaDiaria, error);

            if ((error = ValidarReceta(solicitud.RequiereReceta, out receta)) != null)
                resultado.AgregarError(CamposRegistro.RequiereReceta, error);

            if (resultado.EsValido)
            {
                resultado.RegistroValidado = new RegistroMedicamento
                {
                    NombreMedicamento = nombre,
                    DosisMg = dosis,
                    FechaHoraAdministracion = fecha,
                    FrecuenciaDiaria = frecuencia,
                    RequiereReceta = receta
                };
            }

            return resultado;
        }

        /// <returns>Mensaje de error, o null si es válido.</returns>
        public string ValidarNombre(string valor, out string nombreNormalizado)
        {
            nombreNormalizado = null;

            if (string.IsNullOrWhiteSpace(valor))
                return "El nombre del medicamento es obligatorio.";

            string normalizado = Regex.Replace(valor.Trim(), @"\s+", " ");

            if (normalizado.Length > LongitudMaximaNombre)
                return "El nombre del medicamento no puede exceder " + LongitudMaximaNombre + " caracteres.";

            if (!RegexNombre.IsMatch(normalizado))
                return "El nombre del medicamento solo puede contener letras y espacios (sin números ni caracteres especiales).";

            nombreNormalizado = normalizado;
            return null;
        }

        public string ValidarDosis(string valor, out double dosis)
        {
            dosis = 0;

            if (string.IsNullOrWhiteSpace(valor))
                return "La dosis es obligatoria.";

            string texto = valor.Trim();

            if (texto.Length > 10 || !RegexDecimal.IsMatch(texto) ||
                !double.TryParse(texto, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out dosis))
            {
                dosis = 0;
                return "La dosis debe ser un número válido usando punto decimal (ejemplo: 250.5).";
            }

            if (double.IsNaN(dosis) || double.IsInfinity(dosis) || dosis <= 0)
                return "La dosis debe ser mayor a 0 mg.";

            if (dosis > DosisMaximaMg)
                return "La dosis no puede exceder " + DosisMaximaMg.ToString("N0", CultureInfo.InvariantCulture) +
                       " mg (límite de seguridad).";

            return null;
        }

        public string ValidarFechaHora(string valor, out DateTime fecha)
        {
            fecha = DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(valor))
                return "La fecha y hora de administración es obligatoria.";

            if (!DateTime.TryParseExact(valor.Trim(), FormatosFecha, CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out fecha))
            {
                fecha = DateTime.MinValue;
                return "La fecha y hora no tiene un formato válido.";
            }

            DateTime ahora = _reloj.Ahora;

            if (fecha > ahora)
                return "La fecha y hora de administración no puede ser futura.";

            if (fecha < ahora.AddHours(-VentanaHorasPasado))
                return "La fecha y hora no puede tener más de " + VentanaHorasPasado + " horas en el pasado.";

            return null;
        }

        public string ValidarFrecuencia(string valor, out int frecuencia)
        {
            frecuencia = 0;

            if (string.IsNullOrWhiteSpace(valor))
                return "La frecuencia diaria es obligatoria.";

            string texto = valor.Trim();

            if (!RegexEntero.IsMatch(texto) ||
                !int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out frecuencia))
            {
                frecuencia = 0;
                return "La frecuencia diaria debe ser un número entero (sin decimales).";
            }

            if (frecuencia < FrecuenciaMinima || frecuencia > FrecuenciaMaxima)
                return "La frecuencia diaria debe estar entre " + FrecuenciaMinima + " y " + FrecuenciaMaxima + ".";

            return null;
        }

        public string ValidarReceta(bool? valor, out bool requiereReceta)
        {
            requiereReceta = false;

            if (!valor.HasValue)
                return "Debe indicar explícitamente si el medicamento requiere receta médica.";

            requiereReceta = valor.Value;
            return null;
        }
    }
}
