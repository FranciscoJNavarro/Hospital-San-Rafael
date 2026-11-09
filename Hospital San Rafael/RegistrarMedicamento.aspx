<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RegistrarMedicamento.aspx.cs" Inherits="Hospital_San_Rafael.RegistrarMedicamento" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Registro de administración de medicamentos</title>
    <style>
        body { font-family: Segoe UI, Arial, sans-serif; background: #f4f6f8; margin: 0; padding: 24px; color: #1f2933; }
        .tarjeta { max-width: 640px; margin: auto; background: #fff; border-radius: 8px; padding: 24px; box-shadow: 0 1px 4px rgba(0,0,0,.1); }
        h1 { font-size: 22px; margin-top: 0; }
        .campo { margin-bottom: 16px; }
        .campo label.titulo { display: block; font-weight: 600; margin-bottom: 4px; }
        .campo input[type=text], .campo input[type=number], .campo input[type=datetime-local] { width: 100%; padding: 8px; font-size: 16px; box-sizing: border-box; }
        .error { color: #b91c1c; font-size: 14px; display: block; margin-top: 4px; }
        .alerta-error { background: #fee2e2; color: #991b1b; padding: 12px; border-radius: 6px; margin-bottom: 16px; }
        .alerta-exito { background: #dcfce7; color: #166534; padding: 12px; border-radius: 6px; margin-bottom: 16px; }
        .confirmacion { background: #fef9c3; padding: 12px; border-radius: 6px; font-weight: 600; margin: 16px 0; }
        table.resumen { width: 100%; border-collapse: collapse; }
        table.resumen td { border-bottom: 1px solid #e5e7eb; padding: 8px; }
        table.resumen td:first-child { font-weight: 600; width: 45%; }
        .boton { padding: 10px 18px; font-size: 16px; border: 0; border-radius: 6px; cursor: pointer; margin-right: 8px; }
        .primario { background: #1d4ed8; color: #fff; }
        .secundario { background: #e5e7eb; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="tarjeta">
            <h1>Hospital Regional "San Rafael"<br /><small>Registro de administración de medicamentos</small></h1>

            <asp:Panel ID="pnlErrorGeneral" runat="server" CssClass="alerta-error" Visible="false">
                ❌ <asp:Literal ID="litErrorGeneral" runat="server" Mode="Encode" />
            </asp:Panel>

            <%-- ===== RF-01: FORMULARIO ===== --%>
            <asp:Panel ID="pnlFormulario" runat="server">
                <div class="campo">
                    <label class="titulo" for="txtNombre">Nombre del medicamento *</label>
                    <asp:TextBox ID="txtNombre" runat="server" MaxLength="100" autocomplete="off" />
                    <asp:Label ID="lblErrNombre" runat="server" CssClass="error" />
                </div>

                <div class="campo">
                    <label class="titulo" for="txtDosis">Dosis administrada (mg) *</label>
                    <asp:TextBox ID="txtDosis" runat="server" TextMode="Number" step="any" min="0.001" max="5000" />
                    <asp:Label ID="lblErrDosis" runat="server" CssClass="error" />
                </div>

                <div class="campo">
                    <label class="titulo" for="txtFechaHora">Fecha y hora de administración *</label>
                    <asp:TextBox ID="txtFechaHora" runat="server" TextMode="DateTimeLocal" />
                    <asp:Label ID="lblErrFechaHora" runat="server" CssClass="error" />
                </div>

                <div class="campo">
                    <label class="titulo" for="txtFrecuencia">Frecuencia diaria indicada (veces al día) *</label>
                    <asp:TextBox ID="txtFrecuencia" runat="server" TextMode="Number" step="1" min="1" max="24" />
                    <asp:Label ID="lblErrFrecuencia" runat="server" CssClass="error" />
                </div>

                <div class="campo">
                    <label class="titulo">¿Requiere receta médica? *</label>
                    <%-- Sin opción seleccionada por defecto (RF-02) --%>
                    <asp:RadioButtonList ID="rblReceta" runat="server" RepeatDirection="Horizontal">
                        <asp:ListItem Text="Sí" Value="true" />
                        <asp:ListItem Text="No" Value="false" />
                    </asp:RadioButtonList>
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
                <asp:Button ID="btnConfirmar" runat="server" Text="Sí, confirmar registro" CssClass="boton primario"
                    OnClick="btnConfirmar_Click" UseSubmitBehavior="false" OnClientClick="this.disabled=true;" />
                <asp:Button ID="btnCorregir" runat="server" Text="No, corregir datos" CssClass="boton secundario" OnClick="btnCorregir_Click" />
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
                    <tr><td>Registrado el</td><td><asp:Literal ID="litExRegistro" runat="server" Mode="Encode" /></td></tr>
                </table>
                <br />
                <asp:Button ID="btnNuevo" runat="server" Text="Registrar otro medicamento" CssClass="boton primario" OnClick="btnNuevo_Click" />
            </asp:Panel>
        </div>
    </form>
</body>
</html>