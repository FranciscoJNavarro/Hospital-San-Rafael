// Validaciones del lado del cliente (front-end).
// Solo mejoran la experiencia: la validación definitiva SIEMPRE se repite en el servidor (capa Business).

/**
 * Validador personalizado de ASP.NET (CustomValidator) para la fecha y hora de administración.
 * La ventana de horas permitida llega desde el servidor en el atributo data-ventana-horas,
 * que a su vez sale del Web.config (no está escrita aquí).
 */
function validarFechaAdministracion(origen, argumentos) {
    var fecha = new Date(argumentos.Value);
    if (isNaN(fecha.getTime())) {
        argumentos.IsValid = false;
        return;
    }

    var ventanaHoras = parseInt(origen.getAttribute('data-ventana-horas'), 10);
    var ahora = new Date();
    var limiteInferior = new Date(ahora.getTime() - ventanaHoras * 60 * 60 * 1000);

    argumentos.IsValid = fecha <= ahora && fecha >= limiteInferior;
}
