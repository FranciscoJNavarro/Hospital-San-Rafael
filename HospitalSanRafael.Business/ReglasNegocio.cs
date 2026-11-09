using System;

namespace HospitalSanRafael.Business
{
    /// <summary>
    /// Valores de negocio que pueden cambiar con el tiempo (RF-02).
    /// Se leen del Web.config para no tenerlos escritos en el código.
    /// </summary>
    public class ReglasNegocio
    {
        public double DosisMaximaMg { get; private set; }
        public int FrecuenciaMinimaDiaria { get; private set; }
        public int FrecuenciaMaximaDiaria { get; private set; }
        public int VentanaHorasPasado { get; private set; }
        public int LongitudMaximaNombre { get; private set; }

        // Constructor público: permite crear reglas fijas en pruebas unitarias sin depender del Web.config
        public ReglasNegocio(double dosisMaximaMg, int frecuenciaMinimaDiaria, int frecuenciaMaximaDiaria,
                             int ventanaHorasPasado, int longitudMaximaNombre)
        {
            if (dosisMaximaMg <= 0)
                throw new ArgumentOutOfRangeException("dosisMaximaMg", "La dosis máxima debe ser mayor a 0.");
            if (frecuenciaMinimaDiaria < 1 || frecuenciaMaximaDiaria < frecuenciaMinimaDiaria)
                throw new ArgumentOutOfRangeException("frecuenciaMinimaDiaria", "El rango de frecuencia no es válido.");
            if (ventanaHorasPasado <= 0)
                throw new ArgumentOutOfRangeException("ventanaHorasPasado", "La ventana de horas debe ser mayor a 0.");
            if (longitudMaximaNombre <= 0)
                throw new ArgumentOutOfRangeException("longitudMaximaNombre", "La longitud máxima debe ser mayor a 0.");

            DosisMaximaMg = dosisMaximaMg;
            FrecuenciaMinimaDiaria = frecuenciaMinimaDiaria;
            FrecuenciaMaximaDiaria = frecuenciaMaximaDiaria;
            VentanaHorasPasado = ventanaHorasPasado;
            LongitudMaximaNombre = longitudMaximaNombre;
        }

        /// <summary>
        /// Lee las reglas del Web.config. Si falta una clave o tiene un valor inválido,
        /// la aplicación falla al iniciar en vez de validar con un valor equivocado.
        /// </summary>
        public static ReglasNegocio CargarDesdeConfiguracion()
        {
            return new ReglasNegocio(
                LectorConfiguracion.LeerDecimal("DosisMaximaMg"),
                LectorConfiguracion.LeerEntero("FrecuenciaMinimaDiaria"),
                LectorConfiguracion.LeerEntero("FrecuenciaMaximaDiaria"),
                LectorConfiguracion.LeerEntero("VentanaHorasPasado"),
                LectorConfiguracion.LeerEntero("LongitudMaximaNombre"));
        }
    }
}
