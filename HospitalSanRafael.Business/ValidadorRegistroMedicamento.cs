using System;
using System.Globalization;
using System.Text.RegularExpressions;
using HospitalSanRafael.Entities;

namespace HospitalSanRafael.Business
{
    /// <summary>
    /// Implementa las reglas de RF-02. Es la validación definitiva del sistema:
    /// se ejecuta siempre en el servidor, aunque el navegador ya haya validado.
    /// Cada campo tiene su propio método público para poder probarlo por separado.
    /// </summary>
    public class ValidadorRegistroMedicamento
    {
        /// <summary>
        /// Solo letras (incluye acentos, ñ y ü) separadas por espacios.
        /// Es público para que el front-end use exactamente el mismo patrón.
        /// </summary>
        public const string PatronNombreMedicamento =
            @"^\s*[A-Za-zÁÉÍÓÚáéíóúÑñÜü]+(\s+[A-Za-zÁÉÍÓÚáéíóúÑñÜü]+)*\s*$";

        // Límite técnico (no de negocio) para no procesar textos numéricos absurdamente largos
        private const int LongitudMaximaTextoNumerico = 10;

        private static readonly Regex RegexNombre = new Regex(PatronNombreMedicamento);

        // Solo dígitos con punto decimal opcional: rechaza negativos, comas, notación científica, etc.
        private static readonly Regex RegexDecimal = new Regex(@"^\d+(\.\d+)?$");
        private static readonly Regex RegexEntero = new Regex(@"^\d+$");

        private static readonly string[] FormatosFechaAceptados =
        {
            "yyyy-MM-ddTHH:mm",      // formato del control datetime-local de HTML5
            "yyyy-MM-ddTHH:mm:ss",
            "dd/MM/yyyy HH:mm"
        };

        private readonly IReloj _reloj;
        private readonly ReglasNegocio _reglas;

        public ValidadorRegistroMedicamento(IReloj reloj, ReglasNegocio reglas)
        {
            if (reloj == null) throw new ArgumentNullException("reloj");
            if (reglas == null) throw new ArgumentNullException("reglas");
            _reloj = reloj;
            _reglas = reglas;
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
            DateTime fechaHora;
            int frecuencia;
            bool requiereReceta;

            AgregarSiHayError(resultado, CamposRegistro.NombreMedicamento, ValidarNombre(solicitud.NombreMedicamento, out nombre));
            AgregarSiHayError(resultado, CamposRegistro.DosisMg, ValidarDosis(solicitud.DosisMg, out dosis));
            AgregarSiHayError(resultado, CamposRegistro.FechaHoraAdministracion, ValidarFechaHora(solicitud.FechaHoraAdministracion, out fechaHora));
            AgregarSiHayError(resultado, CamposRegistro.FrecuenciaDiaria, ValidarFrecuencia(solicitud.FrecuenciaDiaria, out frecuencia));
            AgregarSiHayError(resultado, CamposRegistro.RequiereReceta, ValidarReceta(solicitud.RequiereReceta, out requiereReceta));

            if (resultado.EsValido)
            {
                resultado.RegistroValidado = new RegistroMedicamento
                {
                    NombreMedicamento = nombre,
                    DosisMg = dosis,
                    FechaHoraAdministracion = fechaHora,
                    FrecuenciaDiaria = frecuencia,
                    RequiereReceta = requiereReceta
                };
            }

            return resultado;
        }

        /// <returns>Mensaje de error, o null si el valor es válido.</returns>
        public string ValidarNombre(string valor, out string nombreNormalizado)
        {
            nombreNormalizado = null;

            if (string.IsNullOrWhiteSpace(valor))
                return "El nombre del medicamento es obligatorio.";

            // Se quitan espacios sobrantes para guardar siempre el mismo formato
            string normalizado = Regex.Replace(valor.Trim(), @"\s+", " ");

            if (normalizado.Length > _reglas.LongitudMaximaNombre)
                return "El nombre del medicamento no puede exceder " + _reglas.LongitudMaximaNombre + " caracteres.";

            if (!RegexNombre.IsMatch(normalizado))
                return "El nombre del medicamento solo puede contener letras y espacios (sin números ni caracteres especiales).";

            nombreNormalizado = normalizado;
            return null;
        }

        /// <returns>Mensaje de error, o null si el valor es válido.</returns>
        public string ValidarDosis(string valor, out double dosis)
        {
            dosis = 0;

            if (string.IsNullOrWhiteSpace(valor))
                return "La dosis es obligatoria.";

            string texto = valor.Trim();

            if (texto.Length > LongitudMaximaTextoNumerico || !RegexDecimal.IsMatch(texto) ||
                !double.TryParse(texto, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out dosis))
            {
                dosis = 0;
                return "La dosis debe ser un número válido usando punto decimal (ejemplo: 250.5).";
            }

            if (double.IsNaN(dosis) || double.IsInfinity(dosis) || dosis <= 0)
                return "La dosis debe ser mayor a 0 mg.";

            if (dosis > _reglas.DosisMaximaMg)
                return "La dosis no puede exceder " + _reglas.DosisMaximaMg.ToString("N0", CultureInfo.InvariantCulture) +
                       " mg (límite de seguridad).";

            return null;
        }

        /// <returns>Mensaje de error, o null si el valor es válido.</returns>
        public string ValidarFechaHora(string valor, out DateTime fechaHora)
        {
            fechaHora = DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(valor))
                return "La fecha y hora de administración es obligatoria.";

            if (!DateTime.TryParseExact(valor.Trim(), FormatosFechaAceptados, CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out fechaHora))
            {
                fechaHora = DateTime.MinValue;
                return "La fecha y hora no tiene un formato válido.";
            }

            DateTime ahora = _reloj.Ahora;

            if (fechaHora > ahora)
                return "La fecha y hora de administración no puede ser futura.";

            if (fechaHora < ahora.AddHours(-_reglas.VentanaHorasPasado))
                return "La fecha y hora no puede tener más de " + _reglas.VentanaHorasPasado + " horas en el pasado.";

            return null;
        }

        /// <returns>Mensaje de error, o null si el valor es válido.</returns>
        public string ValidarFrecuencia(string valor, out int frecuencia)
        {
            frecuencia = 0;

            if (string.IsNullOrWhiteSpace(valor))
                return "La frecuencia diaria es obligatoria.";

            string texto = valor.Trim();

            if (texto.Length > LongitudMaximaTextoNumerico || !RegexEntero.IsMatch(texto) ||
                !int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out frecuencia))
            {
                frecuencia = 0;
                return "La frecuencia diaria debe ser un número entero (sin decimales).";
            }

            if (frecuencia < _reglas.FrecuenciaMinimaDiaria || frecuencia > _reglas.FrecuenciaMaximaDiaria)
                return "La frecuencia diaria debe estar entre " + _reglas.FrecuenciaMinimaDiaria +
                       " y " + _reglas.FrecuenciaMaximaDiaria + ".";

            return null;
        }

        /// <returns>Mensaje de error, o null si el valor es válido.</returns>
        public string ValidarReceta(bool? valor, out bool requiereReceta)
        {
            requiereReceta = false;

            if (!valor.HasValue)
                return "Debe indicar explícitamente si el medicamento requiere receta médica.";

            requiereReceta = valor.Value;
            return null;
        }

        private static void AgregarSiHayError(ResultadoValidacion resultado, string campo, string mensajeError)
        {
            if (mensajeError != null)
                resultado.AgregarError(campo, mensajeError);
        }
    }
}
