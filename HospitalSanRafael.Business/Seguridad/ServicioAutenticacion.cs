using System;
using System.Collections.Generic;
using HospitalSanRafael.Business.Logging;
using HospitalSanRafael.Data;
using HospitalSanRafael.Entities;

namespace HospitalSanRafael.Business.Seguridad
{
    /// <summary>
    /// Valida usuario y contraseña, y bloquea temporalmente la cuenta después de
    /// varios intentos fallidos para frenar ataques de fuerza bruta.
    /// Debe usarse una sola instancia en toda la aplicación (guarda los intentos en memoria).
    /// </summary>
    public class ServicioAutenticacion
    {
        // Mismo mensaje si el usuario no existe o si la contraseña es incorrecta: no revela qué usuarios existen
        private const string MensajeCredencialesInvalidas = "Usuario o contraseña incorrectos.";
        private const int LongitudMaximaUsuario = 50;

        private readonly IUsuarioRepository _repositorio;
        private readonly ConfiguracionSeguridad _configuracion;
        private readonly IReloj _reloj;
        private readonly ILogger _logger;

        private readonly object _candado = new object();
        private readonly Dictionary<string, ControlIntentos> _intentosPorUsuario =
            new Dictionary<string, ControlIntentos>(StringComparer.OrdinalIgnoreCase);

        public ServicioAutenticacion(IUsuarioRepository repositorio, ConfiguracionSeguridad configuracion,
                                     IReloj reloj, ILogger logger)
        {
            if (repositorio == null) throw new ArgumentNullException("repositorio");
            if (configuracion == null) throw new ArgumentNullException("configuracion");
            if (reloj == null) throw new ArgumentNullException("reloj");
            if (logger == null) throw new ArgumentNullException("logger");

            _repositorio = repositorio;
            _configuracion = configuracion;
            _reloj = reloj;
            _logger = logger;
        }

        public ResultadoAutenticacion Autenticar(string nombreUsuario, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrEmpty(contrasena))
                return ResultadoAutenticacion.Fallido("Capture su usuario y contraseña.");

            string usuarioCapturado = nombreUsuario.Trim();
            if (usuarioCapturado.Length > LongitudMaximaUsuario)
                return ResultadoAutenticacion.Fallido(MensajeCredencialesInvalidas);

            if (EstaBloqueado(usuarioCapturado))
            {
                _logger.Advertencia("Intento de inicio de sesión en cuenta bloqueada. Usuario=" + usuarioCapturado);
                return ResultadoAutenticacion.Fallido(
                    "La cuenta está bloqueada temporalmente por varios intentos fallidos. Intente de nuevo en "
                    + _configuracion.MinutosBloqueo + " minutos.");
            }

            Usuario usuario;
            try
            {
                usuario = _repositorio.ObtenerPorNombre(usuarioCapturado);
            }
            catch (Exception ex)
            {
                _logger.Error("Error al consultar el archivo de usuarios.", ex);
                return ResultadoAutenticacion.Fallido("No fue posible iniciar sesión. Avise al área de sistemas.");
            }

            if (!CredencialesSonValidas(usuario, contrasena))
            {
                RegistrarIntentoFallido(usuarioCapturado);
                return ResultadoAutenticacion.Fallido(MensajeCredencialesInvalidas);
            }

            ReiniciarIntentos(usuarioCapturado);
            _logger.Info("Inicio de sesión exitoso. Usuario=" + usuario.NombreUsuario + ", Rol=" + usuario.Rol);
            return ResultadoAutenticacion.Exitoso(usuario);
        }

        private static bool CredencialesSonValidas(Usuario usuario, string contrasena)
        {
            if (usuario == null)
            {
                HashContrasena.SimularVerificacion(contrasena);
                return false;
            }

            bool contrasenaCorrecta = HashContrasena.Verificar(contrasena, usuario.Sal, usuario.HashContrasena, usuario.Iteraciones);
            return contrasenaCorrecta && usuario.Activo;
        }

        private bool EstaBloqueado(string nombreUsuario)
        {
            lock (_candado)
            {
                ControlIntentos control;
                return _intentosPorUsuario.TryGetValue(nombreUsuario, out control)
                       && control.BloqueadoHasta.HasValue
                       && control.BloqueadoHasta.Value > _reloj.Ahora;
            }
        }

        private void RegistrarIntentoFallido(string nombreUsuario)
        {
            lock (_candado)
            {
                ControlIntentos control;
                if (!_intentosPorUsuario.TryGetValue(nombreUsuario, out control))
                {
                    control = new ControlIntentos();
                    _intentosPorUsuario[nombreUsuario] = control;
                }

                control.Fallidos++;
                _logger.Advertencia("Inicio de sesión fallido (" + control.Fallidos + " de " +
                                    _configuracion.MaxIntentosFallidos + "). Usuario=" + nombreUsuario);

                if (control.Fallidos >= _configuracion.MaxIntentosFallidos)
                {
                    control.BloqueadoHasta = _reloj.Ahora.AddMinutes(_configuracion.MinutosBloqueo);
                    control.Fallidos = 0;
                    _logger.Advertencia("Cuenta bloqueada por " + _configuracion.MinutosBloqueo +
                                        " minutos. Usuario=" + nombreUsuario);
                }
            }
        }

        private void ReiniciarIntentos(string nombreUsuario)
        {
            lock (_candado)
            {
                _intentosPorUsuario.Remove(nombreUsuario);
            }
        }

        /// <summary>Intentos fallidos y bloqueo de un usuario.</summary>
        private class ControlIntentos
        {
            public int Fallidos { get; set; }
            public DateTime? BloqueadoHasta { get; set; }
        }
    }
}
