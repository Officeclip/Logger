<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Logger.Net.Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<%= StackExchange.Profiling.MiniProfiler.RenderIncludes() %>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <div>
        This is the default page<br/>
        <asp:Literal runat="server" ID="literal1"></asp:Literal>
    </div>
    </form>
</body>
</html>
