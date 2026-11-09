<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Hospital_San_Rafael.Login" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Iniciar sesión - Hospital San Rafael</title>
    <link href="Estilos/sitio.css" rel="stylesheet" />
</head>
<body>
    <form id="formLogin" runat="server">
        <div class="tarjeta tarjeta-angosta">
            <h1>Hospital Regional "San Rafael"<br /><small>Control de administración de medicamentos</small></h1>

            <asp:Panel ID="pnlMensaje" runat="server" CssClass="alerta-error" Visible="false">
                ❌ <asp:Literal ID="litMensaje" runat="server" Mode="Encode" />
            </asp:Panel>

            <div class="campo">
                <label class="titulo" for="txtUsuario">Usuario</label>
                <asp:TextBox ID="txtUsuario" runat="server" MaxLength="50" autocomplete="username" />
                <asp:RequiredFieldValidator ID="rfvUsuario" runat="server" ControlToValidate="txtUsuario"
                    CssClass="error" Display="Dynamic" ErrorMessage="❌ Capture su usuario." />
            </div>

            <div class="campo">
                <label class="titulo" for="txtContrasena">Contraseña</label>
                <asp:TextBox ID="txtContrasena" runat="server" TextMode="Password" MaxLength="128" autocomplete="current-password" />
                <asp:RequiredFieldValidator ID="rfvContrasena" runat="server" ControlToValidate="txtContrasena"
                    CssClass="error" Display="Dynamic" ErrorMessage="❌ Capture su contraseña." />
            </div>

            <asp:Button ID="btnIniciarSesion" runat="server" Text="Iniciar sesión" CssClass="boton primario"
                OnClick="btnIniciarSesion_Click" />
        </div>
    </form>
</body>
</html>
