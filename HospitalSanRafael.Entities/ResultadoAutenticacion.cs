namespace HospitalSanRafael.Entities
{
    /// <summary>Resultado de un intento de inicio de sesión.</summary>
    public class ResultadoAutenticacion
    {
        public bool Exito { get; private set; }
        public Usuario Usuario { get; private set; }

        /// <summary>Mensaje para mostrar al usuario cuando el inicio de sesión falla.</summary>
        public string Mensaje { get; private set; }

        public static ResultadoAutenticacion Exitoso(Usuario usuario)
        {
            return new ResultadoAutenticacion { Exito = true, Usuario = usuario };
        }

        public static ResultadoAutenticacion Fallido(string mensaje)
        {
            return new ResultadoAutenticacion { Exito = false, Mensaje = mensaje };
        }
    }
}
