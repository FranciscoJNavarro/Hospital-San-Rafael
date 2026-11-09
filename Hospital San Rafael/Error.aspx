<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="Hospital_San_Rafael.PaginaError" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Ocurrió un problema - Hospital San Rafael</title>
    <link href="Estilos/sitio.css" rel="stylesheet" />
</head>
<body>
    <%-- Página genérica: NO muestra detalles técnicos del error (esos quedan solo en el log) --%>
    <div class="tarjeta tarjeta-angosta">
        <h1>Hospital Regional "San Rafael"</h1>
        <div class="alerta-error">
            ❌ Ocurrió un problema inesperado. El incidente quedó registrado para el área de sistemas.
        </div>
        <p><strong>Si estaba registrando un medicamento, verifique si el registro se guardó antes de volver a capturarlo.</strong></p>
        <a class="boton primario" href="RegistrarMedicamento.aspx">Volver al inicio</a>
    </div>
</body>
</html>
