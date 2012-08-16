<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="KeypicExample._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        div#login
        {
            background: #eee;
            width: 300px;
            padding: 10px;
            border: 1px solid #ddd;
            margin: 100px auto;
            position: relative;
        }
        .keypic
        {
            background: #fff;
            border: 2px solid #ddd;
            margin:5px 0;
            width:150px
        }
        .keypic img
        {
            margin: 5px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div id="login">
        <span style="color: Red">
            <asp:Literal ID="ltlErrorMessage" EnableViewState="false" runat="server"></asp:Literal></span><br />
        <table cellpadding="5" cellspacing="0">
            <tr>
                <td>
                    <label>
                        Username</label>
                </td>
                <td>
                    <asp:TextBox ID="txtUsername" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    <label>
                        Email</label>
                </td>
                <td>
                    <asp:TextBox ID="txtEmail" runat="server"></asp:TextBox>
                </td>
            </tr>
        </table>
        Message<br />
        <asp:TextBox ID="txtMessage" TextMode="MultiLine" Width="250px" Height="100px" runat="server"></asp:TextBox>

        <div align="center" class="keypic">
            <%=keypicClient.GetImage(150,150) %></div>
        <asp:HiddenField ID="hdnToken" runat="server" />
        <asp:Button ID="btnLogin" runat="server" Text="Login" OnClick="btnLogin_Click" />
    </div>
    </form>
</body>
</html>
