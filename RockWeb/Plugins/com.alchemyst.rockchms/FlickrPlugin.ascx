<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FlickrPlugin.ascx.cs" Inherits="com.alchemyst.rockchms.FlickrPlugin" %>

<asp:Label ID="lblTest" runat="server" />
<asp:Button ID="btnLogin" runat="server" Text="Authenticate" OnClick="btnLogin_Click" />

<div>
    User ID: 
        <asp:TextBox runat="server" ID="UserIdTextBox" Text=""
            AutoPostBack="True"></asp:TextBox>
</div>
<div>

    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False">
        <Columns>
            <asp:BoundField DataField="PhotoId" HeaderText="PhotoId"
                SortExpression="PhotoId" />
            <asp:HyperLinkField DataNavigateUrlFields="WebUrl" DataTextField="Title" />
            <asp:ImageField DataAlternateTextField="Title"
                DataImageUrlField="LargeSquareThumbnailUrl">
            </asp:ImageField>
        </Columns>
    </asp:GridView>

</div>
<!-- Carousel
    ================================================== -->
<div id="myCarousel" class="carousel slide" style="width:640px;">
    <!-- Indicators -->
    <asp:Repeater ID="rptIndicators" runat="server">
        <HeaderTemplate>
            <ol class="carousel-indicators">
        </HeaderTemplate>
        <ItemTemplate>
            <li data-target="#myCarousel" data-slide-to="<%# Eval("RowNum") %>" <%#Eval("IndicatorClass") %>></li>
        </ItemTemplate>
        <FooterTemplate>
            </ol>
        </FooterTemplate>
    </asp:Repeater>

    <asp:Repeater ID="rptInnerCarousel" runat="server">
        <HeaderTemplate>
            <div class="carousel-inner">
        </HeaderTemplate>
        <ItemTemplate>
        <div class="<%#Eval("InnerClass") %>">
            <img src="<%#Eval("Medium640Url") %>" data-src="" alt="<%#Eval("Title") %>">
            <div class="container">
                <div class="carousel-caption">
                    <h1><%#Eval("Title") %></h1>
                    <p></p>
                    <p></p>
                </div>
            </div>
        </div>
        </ItemTemplate>
        <FooterTemplate>
            </div>
        </FooterTemplate>
    </asp:Repeater>
    <a class="left carousel-control" href="#myCarousel" data-slide="prev"><span class="glyphicon glyphicon-chevron-left"></span></a>
    <a class="right carousel-control" href="#myCarousel" data-slide="next"><span class="glyphicon glyphicon-chevron-right"></span></a>
</div>
<!-- /.carousel -->

<!--
<script type="text/javascript">
    $('.carousel').carousel({
        interval: 2000
    })
</script>
-->
