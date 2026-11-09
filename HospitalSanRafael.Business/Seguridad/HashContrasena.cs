using System;
using System.Security.Cryptography;

namespace HospitalSanRafael.Business.Seguridad
{
    /// <summary>
    /// Hash de contraseñas con PBKDF2-SHA256 + sal aleatoria.
    /// Es un algoritmo lento a propósito: hace muy costoso adivinar contraseñas por fuerza bruta
    /// aunque alguien llegue a robar el archivo de usuarios.
    /// </summary>
    public static class HashContrasena
    {
        public const int IteracionesRecomendadas = 100000;
        private const int TamanoSalBytes = 16;
        private const int TamanoHashBytes = 32;

        /// <summary>Verifica una contraseña contra el hash guardado.</summary>
        public static bool Verificar(string contrasena, string salBase64, string hashBase64, int iteraciones)
        {
            if (contrasena == null || string.IsNullOrEmpty(salBase64) || string.IsNullOrEmpty(hashBase64) || iteraciones <= 0)
                return false;

            byte[] sal;
            byte[] hashEsperado;
            try
            {
                sal = Convert.FromBase64String(salBase64);
                hashEsperado = Convert.FromBase64String(hashBase64);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] hashCalculado = CalcularHash(contrasena, sal, iteraciones, hashEsperado.Length);
            return SonIgualesEnTiempoConstante(hashCalculado, hashEsperado);
        }

        /// <summary>Genera sal y hash para una contraseña nueva (útil para dar de alta usuarios).</summary>
        public static void GenerarHash(string contrasena, int iteraciones, out string salBase64, out string hashBase64)
        {
            if (string.IsNullOrEmpty(contrasena)) throw new ArgumentException("La contraseña es obligatoria.", "contrasena");

            var sal = new byte[TamanoSalBytes];
            using (var generador = new RNGCryptoServiceProvider())
            {
                generador.GetBytes(sal);
            }

            salBase64 = Convert.ToBase64String(sal);
            hashBase64 = Convert.ToBase64String(CalcularHash(contrasena, sal, iteraciones, TamanoHashBytes));
        }

        /// <summary>
        /// Se ejecuta cuando el usuario no existe, para que la respuesta tarde lo mismo
        /// y no se pueda adivinar qué usuarios existen midiendo el tiempo.
        /// </summary>
        public static void SimularVerificacion(string contrasena)
        {
            CalcularHash(contrasena ?? string.Empty, new byte[TamanoSalBytes], IteracionesRecomendadas, TamanoHashBytes);
        }

        private static byte[] CalcularHash(string contrasena, byte[] sal, int iteraciones, int tamanoBytes)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(contrasena, sal, iteraciones, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(tamanoBytes);
            }
        }

        /// <summary>Compara todos los bytes siempre, para no filtrar información por el tiempo de respuesta.</summary>
        private static bool SonIgualesEnTiempoConstante(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;

            int diferencia = 0;
            for (int i = 0; i < a.Length; i++)
                diferencia |= a[i] ^ b[i];

            return diferencia == 0;
        }
    }
}
