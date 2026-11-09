using System;

namespace HospitalSanRafael.Business
{
    /// <summary>
    /// Abstrae la hora actual. Permite probar las reglas de fecha (no futura, ventana de horas)
    /// con una hora fija en pruebas unitarias.
    /// </summary>
    public interface IReloj
    {
        DateTime Ahora { get; }
    }

    /// <summary>Reloj real del servidor.</summary>
    public class RelojSistema : IReloj
    {
        public DateTime Ahora { get { return DateTime.Now; } }
    }
}
