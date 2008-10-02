<%@ WebHandler Language="C#" Class="Echo" %>

using System;
using System.Web;

public class Echo : IHttpHandler
{
  HttpContext ctx;
  
  public void ProcessRequest(HttpContext context)
  {
    string method = string.Empty;
    string media = string.Empty;
    string rtn = string.Empty;
    string kv_fmt = "{0}={1}";
    ctx = context; 

    string[] keys = ctx.Request.Form.AllKeys;
    for(int i=0;i<keys.Length;i++)
    {
      if (i != 0)
      {
        rtn += "&";
      }
      rtn += string.Format(kv_fmt, keys[i], ctx.Request.Form[keys[i]]);
    }
    
    media = ctx.Request.Headers["accept"];
    switch (media.ToLower())
    {
      case "text/xml":
        rtn = string.Format("<echo>{0}</echo>", rtn);
        break;
      case "application/json":
        rtn = string.Format("{{\"echo\":\"{0}\"}}", rtn.Replace("\"","\\\""));
        break;
      default:
        rtn = string.Format("<h1>echo</h1><p>{0}</p>", rtn);
        media = "text/html";
        break;
    }

    ctx.Response.ContentType = media;
    ctx.Response.Write(rtn);
  }

  public bool IsReusable
  {
    get
    {
      return false;
    }
  }
}