using HospitalSanRafael.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HospitalSanRafael.Data
{
    public class RegistroMedicamentoXmlRepository : IRegistroMedicamentoRepository
    {
        // Evita que dos enfermeras guardando al mismo tiempo corrompan el archivo
        private static readonly object Candado = new object();
        private readonly string _rutaArchivo;
        private readonly XmlSerializer _serializador = new XmlSerializer(typeof(List<RegistroMedicamento>));

        public RegistroMedicamentoXmlRepository(string rutaArchivo)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo))
                throw new ArgumentException("La ruta del archivo es obligatoria.", "rutaArchivo");
            _rutaArchivo = rutaArchivo;
        }

        public void Guardar(RegistroMedicamento registro)
        {
            if (registro == null) throw new ArgumentNullException("registro");

            lock (Candado)
            {
                List<RegistroMedicamento> registros = Leer();
                registros.Add(registro);
                Escribir(registros);
            }
        }

        public List<RegistroMedicamento> ObtenerTodos()
        {
            lock (Candado)
            {
                return Leer();
            }
        }

        private List<RegistroMedicamento> Leer()
        {
            if (!File.Exists(_rutaArchivo))
                return new List<RegistroMedicamento>();

            using (var stream = File.OpenRead(_rutaArchivo))
            {
                return (List<RegistroMedicamento>)_serializador.Deserialize(stream);
            }
        }

        private void Escribir(List<RegistroMedicamento> registros)
        {
            string directorio = Path.GetDirectoryName(_rutaArchivo);
            if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
                Directory.CreateDirectory(directorio);

            // Se escribe primero en un temporal: si algo falla, no se pierde el archivo original
            string temporal = _rutaArchivo + ".tmp";
            using (var stream = File.Create(temporal))
            {
                _serializador.Serialize(stream, registros);
            }

            if (File.Exists(_rutaArchivo))
                File.Replace(temporal, _rutaArchivo, null);
            else
                File.Move(temporal, _rutaArchivo);
        }
    }
}
