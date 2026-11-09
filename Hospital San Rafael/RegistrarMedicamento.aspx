<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RegistrarMedicamento.aspx.cs" Inherits="Hospital_San_Rafael.RegistrarMedicamento" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Registro de administración de medicamentos</title>
    <link href="Estilos/sitio.css" rel="stylesheet" />
    <script src="Scripts/validaciones.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="tarjeta">
            <div class="barra-usuario">
                <span>Usuario: <strong><asp:Literal ID="litUsuario" runat="server" Mode="Encode" /></strong> (Enfermero/a)</span>
                <asp:Button ID="btnCerrarSesion" runat="server" Text="Cerrar sesión" CssClass="boton-enlace"
                    CausesValidation="false" OnClick="btnCerrarSesion_Click" />
            </div>

            <h1>Hospital Regional "San Rafael"<br /><small>Registro de administración de medicamentos</small></h1>

            <asp:Panel ID="pnlErrorGeneral" runat="server" CssClass="alerta-error" Visible="false">
                ❌ <asp:Literal ID="litErrorGeneral" runat="server" Mode="Encode" />
            </asp:Panel>

            <%-- ===== RF-01: FORMULARIO =====
                 Validación en dos niveles:
                 1) Front-end: validadores de ASP.NET (avisan al instante en el navegador).
                 2) Back-end: ValidadorRegistroMedicamento en la capa Business (siempre se ejecuta).
                 Los límites (dosis máxima, frecuencia, horas) se asignan desde el code-behind leyendo el Web.config. --%>
            <asp:Panel ID="pnlFormulario" runat="server">
                <div class="campo">
                    <label class="titulo" for="txtNombre">Nombre del medicamento *</label>
                    <asp:TextBox ID="txtNombre" runat="server" autocomplete="off" />
                    <asp:RequiredFieldValidator ID="rfvNombre" runat="server" ControlToValidate="txtNombre"
                        CssClass="error" Display="Dynamic" ErrorMessage="❌ El nombre del medicamento es obligatorio." />
                    <asp:RegularExpressionValidator ID="revNombre" runat="server" ControlToValidate="txtNombre"
                        CssClass="error" Display="Dynamic"
                        ErrorMessage="❌ Solo se permiten letras y espacios (sin números ni caracteres especiales)." />
                    <asp:Label ID="lblErrNombre" runat="server" CssClass="error" />
                </div>

                <div class="campo">
                    <label class="titulo" for="txtDosis">Dosis administrada (mg) *</label>
                    <asp:TextBox ID="txtDosis" runat="server" TextMode="Number" step="any" min="0.001" />
                    <asp:RequiredFieldValidator ID="rfvDosis" runat="server" ControlToValidate="txtDosis"
                        CssClass="error" Display="Dynamic" ErrorMessage="❌ La dosis es obligatoria." />
                    <asp:CompareValidator ID="cvDosisMinima" runat="server" ControlToValidate="txtDosis"
                        Type="Double" Operator="GreaterThan" ValueToCompare="0"
                        CssClass="error" Display="Dynamic" ErrorMessage="❌ La dosis debe ser un número mayor a 0 mg." />
                    <asp:CompareValidator ID="cvDosisMaxima" runat="server" ControlToValidate="txtDosis"
                        Type="Double" Operator="LessThanEqual" CssClass="error" Display="Dynamic" />
                    <asp:Label ID="lblErrDosis" runat="server" CssClass="error" />
                </div>

                <div class="campo">
                    <label class="titulo" for="txtFechaHora">Fecha y hora de administración *</label>
                    <asp:TextBox ID="txtFechaHora" runat="server" TextMode="DateTimeLocal" />
                    <asp:RequiredFieldValidator ID="rfvFechaHora" runat="server" ControlToValidate="txtFechaHora"
                        CssClass="error" Display="Dynamic" ErrorMessage="❌ La fecha y hora de administración es obligatoria." />
                    <asp:CustomValidator ID="cvFechaHora" runat="server" ControlToValidate="txtFechaHora"
                        ClientValidationFunction="validarFechaAdministracion" CssClass="error" Display="Dynamic" />
                    <asp:Label ID="lblErrFechaHora" runat="server" CssClass="error" />
                </div>

                <div class="campo">
                    <label class="titulo" for="txtFrecuencia">Frecuencia diaria indicada (veces al día) *</label>
                    <asp:TextBox ID="txtFrecuencia" runat="server" TextMode="Number" step="1" />
                    <asp:RequiredFieldValidator ID="rfvFrecuencia" runat="server" ControlToValidate="txtFrecuencia"
                        CssClass="error" Display="Dynamic" ErrorMessage="❌ La frecuencia diaria es obligatoria." />
                    <asp:RangeValidator ID="rvFrecuencia" runat="server" ControlToValidate="txtFrecuencia"
                        Type="Integer" CssClass="error" Display="Dynamic" />
                    <asp:Label ID="lblErrFrecuencia" runat="server" CssClass="error" />
                </div>

                <div class="campo">
                    <label class="titulo">¿Requiere receta médica? *</label>
                    <%-- Sin opción seleccionada por defecto (RF-02) --%>
                    <asp:RadioButtonList ID="rblReceta" runat="server" RepeatDirection="Horizontal">
                        <asp:ListItem Text="Sí" Value="true" />
                        <asp:ListItem Text="No" Value="false" />
                    </asp:RadioButtonList>
                    <asp:RequiredFieldValidator ID="rfvReceta" runat="server" ControlToValidate="rblReceta"
                        CssClass="error" Display="Dynamic"
                        ErrorMessage="❌ Debe indicar explícitamente si el medicamento requiere receta médica." />
                    <asp:Label ID="lblErrReceta" runat="server" CssClass="error" />
                </div>

                <asp:Button ID="btnRevisar" runat="server" Text="Revisar datos" CssClass="boton primario" OnClick="btnRevisar_Click" />
            </asp:Panel>

            <%-- ===== RF-03: RESUMEN Y CONFIRMACIÓN ===== --%>
            <asp:Panel ID="pnlConfirmacion" runat="server" Visible="false">
                <h2>Resumen de los datos capturados</h2>
                <table class="resumen">
                    <tr><td>Medicamento</td><td><asp:Literal ID="litResNombre" runat="server" Mode="Encode" /></td></tr>
                    <tr><td>Dosis</td><td><asp:Literal ID="litResDosis" runat="server" Mode="Encode" /></td></tr>
                    <tr><td>Fecha y hora</td><td><asp:Literal ID="litResFecha" runat="server" Mode="Encode" /></td></tr>
                    <tr><td>Frecuencia diaria</td><td><asp:Literal ID="litResFrecuencia" runat="server" Mode="Encode" /></td></tr>
                    <tr><td>Requiere receta</td><td><asp:Literal ID="litResReceta" runat="server" Mode="Encode" /></td></tr>
                </table>
                <div class="confirmacion"><asp:Literal ID="litMensajeConfirmacion" runat="server" Mode="Encode" /></div>
                <%-- Se deshabilita al primer clic para evitar registros duplicados --%>
                <asp:Button ID="btnConfirmar" runat="server" Text="Sí, confirmar registro" CssClass="boton primario"
                    CausesValidation="false" UseSubmitBehavior="false" OnClientClick="this.disabled=true;"
                    OnClick="btnConfirmar_Click" />
                <asp:Button ID="btnCorregir" runat="server" Text="No, corregir datos" CssClass="boton secundario"
                    CausesValidation="false" OnClick="btnCorregir_Click" />
            </asp:Panel>

            <%-- ===== RF-04: ÉXITO ===== --%>
            <asp:Panel ID="pnlExito" runat="server" Visible="false">
                <div class="alerta-exito">✅ <asp:Literal ID="litMensajeExito" runat="server" Mode="Encode" /></div>
                <table class="resumen">
                    <tr><td>Folio</td><td><asp:Literal ID="litExFolio" runat="server" Mode="Encode" /></td></tr>
                    <tr><td>Medicamento</td><td><asp:Literal ID="litExNombre" runat="server" Mode="Encode" /></td></tr>
                    <tr><td>Dosis</td><td><asp:Literal ID="litExDosis" runat="server" Mode="Encode" /></td></tr>
                    <tr><td>Fecha y hora de administración</td><td><asp:Literal ID="litExFecha" runat="server" Mode="Encode" /></td></tr>
                    <tr><td>Frecuencia diaria</td><td><asp:Literal ID="litExFrecuencia" runat="server" Mode="Encode" /></td></tr>
                    <tr><td>Requiere receta</td><td><asp:Literal ID="litExReceta" runat="server" Mode="Encode" /></td></tr>
                    <tr><td>Registrado por</td><td><asp:Literal ID="litExRegistradoPor" runat="server" Mode="Encode" /></td></tr>
                    <tr><td>Registrado el</td><td><asp:Literal ID="litExRegistro" runat="server" Mode="Encode" /></td></tr>
                </table>
                <br />
                <asp:Button ID="btnNuevo" runat="server" Text="Registrar otro medicamento" CssClass="boton primario"
                    CausesValidation="false" OnClick="btnNuevo_Click" />
            </asp:Panel>
        </div>
    </form>
</body>
</html>
