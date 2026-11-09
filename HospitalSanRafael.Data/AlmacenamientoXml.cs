using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace HospitalSanRafael.Data
{
    /// <summary>
    /// Lee y escribe una lista de objetos en un archivo XML.
    /// Lo comparten todos los repositorios XML para no repetir el mismo código.
    /// </summary>
    /// <typeparam name="T">Tipo de objeto que se guarda en el archivo.</typeparam>
    public class AlmacenamientoXml<T>
    {
        // Un candado por tipo de dato: evita que dos usuarios guardando al mismo tiempo corrompan el archivo
        private static readonly object Candado = new object();

        private readonly string _rutaArchivo;
        private readonly XmlSerializer _serializador = new XmlSerializer(typeof(List<T>));

        public AlmacenamientoXml(string rutaArchivo)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo))
                throw new ArgumentException("La ruta del archivo es obligatoria.", "rutaArchivo");
            _rutaArchivo = rutaArchivo;
        }

        public List<T> LeerTodos()
        {
            lock (Candado)
            {
                return LeerArchivo();
            }
        }

        /// <summary>
        /// Lee, modifica y vuelve a escribir la lista como una sola operación protegida,
        /// para que ningún otro hilo escriba entre la lectura y la escritura.
        /// </summary>
        public void Modificar(Action<List<T>> modificacion)
        {
            if (modificacion == null) throw new ArgumentNullException("modificacion");

            lock (Candado)
            {
                List<T> elementos = LeerArchivo();
                modificacion(elementos);
                EscribirArchivo(elementos);
            }
        }

        private List<T> LeerArchivo()
        {
            if (!File.Exists(_rutaArchivo))
                return new List<T>();

            using (var stream = File.OpenRead(_rutaArchivo))
            {
                return (List<T>)_serializador.Deserialize(stream);
            }
        }

        private void EscribirArchivo(List<T> elementos)
        {
            string directorio = Path.GetDirectoryName(_rutaArchivo);
            if (!string.IsNullOrEmpty(directorio))
                Directory.CreateDirectory(directorio);

            // Se escribe primero en un temporal: si algo falla a la mitad, el archivo original no se pierde
            string rutaTemporal = _rutaArchivo + ".tmp";
            using (var stream = File.Create(rutaTemporal))
            {
                _serializador.Serialize(stream, elementos);
            }

            if (File.Exists(_rutaArchivo))
                File.Replace(rutaTemporal, _rutaArchivo, null);
            else
                File.Move(rutaTemporal, _rutaArchivo);
        }
    }
}
