<%@ Page Language="C#" %>
<script runat="server">
    
    void Page_Load(object sender, EventArgs args)
    {
        string list = "test1\ntest2\ntest3\ntest4\ntest5";
        string search = Request["search"];

        if (search != null && search != string.Empty)
        {
            Response.Write(list);
        }
        else
            Response.Write("");
    }
</script>