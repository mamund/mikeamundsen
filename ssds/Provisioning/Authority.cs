using System;
using System.Web;
using System.Xml;
using System.Net;
using System.IO;
using System.Globalization;

using Amundsen.Utilities;

namespace Amundsen.SSDS.Provisioning
{
  /// <summary>
  /// Public Domain 2008 amundsen.com, inc.
  /// @author mike amundsen (mamund@yahoo.com)
  /// @version 1.0 (2008-07-03)
  /// </summary>
  class Authority : IHttpHandler
  {
    private WebUtility wu;
    private HttpClient client;
    private HttpContext ctx;

    string ssdsUser = string.Empty;
    string ssdsPassword = string.Empty;

    bool IHttpHandler.IsReusable
    {
      get { return false; }
    }

    void IHttpHandler.ProcessRequest(HttpContext context)
    {
      ctx = context;
      client = new HttpClient();
      wu = new WebUtility();

      try
      {
        string rtn = wu.ConfirmXmlMediaType(ctx.Request.AcceptTypes);
        if (rtn.Length == 0)
        {
          Options();
          throw new HttpException((int)HttpStatusCode.NotAcceptable, HttpStatusCode.NotAcceptable.ToString());
        }

        wu.GetBasicAuthCredentials(ctx, ref ssdsUser, ref ssdsPassword);
        client.Credentials = new NetworkCredential(ssdsUser, ssdsPassword);

        wu.SetCompression(ctx);

        // process request
        switch (ctx.Request.HttpMethod.ToLower(CultureInfo.CurrentCulture))
        {
          case "get":
            Get();
            break;
          case "head":
            Get(true);
            break;
          case "post":
            Post();
            break;
          case "options":
            Options();
            break;
          default:
            Options();
            throw new HttpException((int)HttpStatusCode.MethodNotAllowed, HttpStatusCode.MethodNotAllowed.ToString());
        }
      }
      catch (HttpException hex)
      {
        ctx.Response.ContentType = "text/xml";
        ctx.Response.Write(string.Format(CultureInfo.CurrentCulture, Constants.ErrorFormat, hex.GetHttpCode(), hex.Message, Constants.SitkaNS));
        ctx.Response.Write(" ".PadRight(500));
        ctx.Response.StatusCode = hex.GetHttpCode();
        ctx.Response.StatusDescription = hex.Message;

      }
      catch (Exception ex)
      {
        ctx.Response.ContentType = "text/xml";
        ctx.Response.Write(string.Format(CultureInfo.CurrentCulture, Constants.ErrorFormat, 500, ex.Message, Constants.SitkaNS));
        ctx.Response.Write(" ".PadRight(500));
        ctx.Response.StatusCode = 500;
        ctx.Response.StatusDescription = ex.Message;
      }
    }

    // reflect valid methods and mimetypes to client
    private void Options()
    {
      ctx.Response.AppendHeader("Allow", "GET,HEAD,POST,OPTIONS");
      ctx.Response.AppendHeader("X-Acceptable", "application/xml,text/xml");
    }

    // get metadata for requested authority
    private void Get()
    {
      Get(false);
    }
    private void Get(bool supressContent)
    {
      string rtn = string.Empty;
      string authority = string.Empty;

      // get authority from query string
      authority = (ctx.Request.QueryString["authority"] != null ? ctx.Request.QueryString["authority"] : string.Empty);
      if (authority.Length==0)
      {
        throw new HttpException(400, "Missing Authority ID");
      }

      // make call to remote server
      rtn = client.Execute(string.Format(CultureInfo.CurrentCulture, "https://{0}.{1}", authority, Constants.SsdsRoot), "get", Constants.SsdsType);

      if (ctx.Request.UserAgent!=null && ctx.Request.UserAgent.IndexOf("IE", StringComparison.CurrentCultureIgnoreCase) != -1)
      {
        ctx.Response.AppendHeader("cache-control", "post-check=1,pre-check=2");
      }

      // compose response to client
      ctx.Response.SuppressContent = supressContent;
      ctx.Response.StatusCode = 200;
      ctx.Response.ContentType = "text/xml";
      ctx.Response.StatusDescription = "OK";
      ctx.Response.Write(rtn);

    }

    // create a new authority
    private void Post()
    {
      string rtn = string.Empty;
      string body = string.Empty;
      string authority = string.Empty;
      string url = string.Empty;
      string data = string.Empty;

      // get body
      using (StreamReader sr = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
      {
        body = sr.ReadToEnd();
        sr.Close();
      }

      authority = body.ToLower(CultureInfo.CurrentCulture).Replace("<authority>", string.Empty).Replace("</authority>", string.Empty);

      // handle post to ssds
      data = string.Format(CultureInfo.CurrentCulture, Constants.AuthorityFormat, authority, Constants.SitkaNS);
      url = string.Format(CultureInfo.CurrentCulture, "https://{0}", Constants.SsdsRoot);
      rtn = client.Execute(url, "post", Constants.SsdsType, data);

      // compose response to client
      ctx.Response.StatusCode = 201;
      ctx.Response.ContentType = "text/xml";
      ctx.Response.StatusDescription = "Authority has been created.";
      ctx.Response.Write(rtn);
    }
  }
}
