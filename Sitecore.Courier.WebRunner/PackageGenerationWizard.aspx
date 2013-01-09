<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PackageGenerationWizard.aspx.cs" Inherits="Sitecore.Courier.WebRunner.PackageGenerationWizard" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Update Generation Wizard</title>
    <link href="/sitecore/shell/themes/standard/default/WebFramework.css" rel="Stylesheet" />
    <link href="/sitecore/admin/Wizard/UpdateInstallationWizard.css" rel="Stylesheet" />
    <link type="text/css" href="/sitecore/admin/Wizard/PackageGenerationWizard/css/black-tie/jquery-ui-1.8.24.custom.css" rel="stylesheet" />
    <script type="text/javascript" src="/sitecore/admin/Wizard/PackageGenerationWizard/js/jquery-1.8.2.min.js"></script>
    <script type="text/javascript" src="/sitecore/admin/Wizard/PackageGenerationWizard/js/jquery-ui-1.8.24.custom.min.js"></script>
    <script>
        $(function () {
            $("#tabs").tabs();
        });
    </script>
    <script type="text/javascript">
        $(function () {
            $("input:submit").button();
        });
    </script>
</head>
<body>
    <form id="form1" runat="server" class="wf-container">
    <div class="wf-content">
        <h1 id="lblHeader">
            Welcome to Sitecore Courier update generation wizard</h1>
        <p class="wf-subtitle">
            This wizard helps you to generate a package for deployment.</p>
        <div>
            <div class="wf-statebox wf-statebox-warning" style="margin: 2em 0">
                <p>
                    If you press "Analyze" first - you can select the changes you want to package. Otherwise - all differences will be packaged.
                </p>
            </div>
            <table style="width: 500px;">
                <tr>
                    <td>
                        <b>Source</b>
                    </td>
                    <td>
                        <asp:TextBox ID="SourcePath" Width="400px" runat="server" Text="F:\serialization1\"></asp:TextBox>
                    </td>
                    <td>
                        &nbsp;
                    </td>
                </tr>
                <tr>
                    <td>
                        <b>Target</b>
                    </td>
                    <td>
                        <asp:TextBox ID="TargetPath" Width="400px" runat="server" Text="F:\serialization2\"></asp:TextBox>
                    </td>
                </tr>
            </table>
            <br />
            <asp:Button ID="Analyze" runat="server" Text="Analyze" OnClick="Analyze_Click" />
            &nbsp;&nbsp;
            <asp:Button ID="Generate" runat="server" Text="Generate" OnClick="Generate_Click" />
            <br />
            <br />
            <asp:HyperLink ID="DownloadLink" runat="server" Visible="False" Target="_Blank">Download Package</asp:HyperLink>
            <asp:Panel ID="AnalyzeResults" runat="server" Visible="False">
                <div id="tabs">
                    <ul>
                        <li><a href="#tabs-1">Added (<asp:Literal ID="AddedCount" runat="server"></asp:Literal>)</a></li>
                        <li><a href="#tabs-2">Deleted (<asp:Literal ID="DeletedCount" runat="server"></asp:Literal>)</a></li>
                        <li><a href="#tabs-3">Changed (<asp:Literal ID="ChangedCount" runat="server"></asp:Literal>)</a></li>
                    </ul>
                    <div id="tabs-1">
                        <asp:CheckBoxList ID="Added" runat="server">
                        </asp:CheckBoxList>
                    </div>
                    <div id="tabs-2">
                        <asp:CheckBoxList ID="Deleted" runat="server">
                        </asp:CheckBoxList>
                    </div>
                    <div id="tabs-3">
                        <asp:CheckBoxList ID="Changed" runat="server">
                        </asp:CheckBoxList>
                    </div>
                </div>
            </asp:Panel>
        </div>
    </div>
    <br />
    <div class="wf-footer">
        Alexander Doroshenko - <a href="http://siteocresnippets.blogspot.com">http://siteocresnippets.blogspot.com</a>
    </div>
    </form>
</body>
</html>
