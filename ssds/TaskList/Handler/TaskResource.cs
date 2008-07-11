using System;
using System.Web;
using System.Xml;
using System.Net;
using System.Globalization;

using Amundsen.Utilities;

namespace Amundsen.SSDS.TaskDemo
{
  /// <summary>
  /// Public Domain 2008 amundsen.com, inc.
  /// @author mike amundsen (mamund@yahoo.com)
  /// @version 1.1 (2008-07-03)
  /// </summary>
  public class TaskResource : IHttpHandler
  {
    private WebUtility wu = new WebUtility();
    private CacheService cs = new CacheService();
    private HttpClient client = new HttpClient();
    private HttpContext ctx;

    const string reply_type = "text/xml";
    const string queryAll = "?q=''";
    const string sitkaNS = "http://schemas.microsoft.com/sitka/2008/03/";
    const string errorFormat = "<s:Error xmlns:s='{2}'><s:Code>{0}</s:Code><s:Message>{1}</s:Message></s:Error>";

    DateTime ifModifiedSinceDate = DateTime.MaxValue;

    // set via config file <ssdsSettings>
    int maxAge;
    bool showExpires;
    string endPoint = string.Empty;
    string ssdsUser = string.Empty;
    string ssdsPassword = string.Empty;
    string queryFormat = string.Empty;

    bool IHttpHandler.IsReusable
    {
      get { return false; }
    }

    void IHttpHandler.ProcessRequest(HttpContext context)
    {
      ctx = context;

      try
      {
        string rtn = wu.ConfirmXmlMediaType(ctx.Request.AcceptTypes);
        if (rtn.Length == 0)
        {
          Options();
          throw new HttpException((int)HttpStatusCode.NotAcceptable, HttpStatusCode.NotAcceptable.ToString());
        }

        HandleConfigSettings();
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
          case "put":
            Put();
            break;
          case "delete":
            Delete();
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
        ctx.Response.ContentType = reply_type;
        ctx.Response.Write(string.Format(CultureInfo.CurrentCulture, errorFormat, hex.GetHttpCode(), hex.Message, sitkaNS));
        ctx.Response.Write(" ".PadRight(500));
        ctx.Response.StatusCode = hex.GetHttpCode();
        ctx.Response.StatusDescription = hex.Message;
      }
      catch (Exception ex)
      {
        ctx.Response.ContentType = reply_type;
        ctx.Response.Write(string.Format(CultureInfo.CurrentCulture, errorFormat, 500, ex.Message, sitkaNS));
        ctx.Response.Write(" ".PadRight(500));
        ctx.Response.StatusCode = 500;
        ctx.Response.StatusDescription = ex.Message;
      }
    }

    // reflect valid methods and mimetypes to client
    private void Options()
    {
      ctx.Response.AppendHeader("Allow", "GET,HEAD,POST,PUT,DELETE,OPTIONS");
      ctx.Response.AppendHeader("X-Acceptable", "application/xml,text/xml");
    }

    // get task list or single task
    private void Get()
    {
      Get(false);
    }
    private void Get(bool supressContent)
    {
      string rtn = string.Empty;
      CacheItem item = null;

      // parse query line
      string id = (ctx.Request.QueryString["id"] != null ? "/" + ctx.Request.QueryString["id"] : string.Empty);
      string q = (ctx.Request.QueryString["q"] != null && ctx.Request.QueryString["q"].Length!=0 ? string.Format(CultureInfo.CurrentCulture,queryFormat,ctx.Server.UrlEncode(ctx.Request.QueryString["q"]).Replace("+","%20")) : queryAll);
      string query = (id.Length!=0 ? id : q);
      string url = endPoint+query;

      // get request headers
      string ifNoneMatch = wu.GetHeader(ctx,"if-none-match");

      // check local cache first (if allowed)
      if (wu.CheckNoCache(ctx)==true)
      {
        cs.RemoveItem(url);
      }
      else
      {
        item = cs.GetItem(url);
      }

      // did our copy expire?
      if (item != null && (item.Expires < DateTime.UtcNow || ifModifiedSinceDate<item.LastModified))
      {
        cs.RemoveItem(url);
        item = null;
      }

      // ask server for new copy
      if (item == null)
      {
        rtn = client.Execute(url, "get", "application/xml");

        // fill local cache
        item = cs.PutItem(
          new CacheItem(
            url,
            rtn,
            string.Format(CultureInfo.CurrentCulture, "\"{0}\"", cs.MD5BinHex(rtn)),
            DateTime.UtcNow.AddSeconds(30),
            showExpires
          )
        );
      }

      // does client have good copy?
      if (ifNoneMatch == item.ETag)
      {
        throw new HttpException((int)HttpStatusCode.NotModified, HttpStatusCode.NotModified.ToString());
      }

      // handle response
      ctx.Response.ContentType = reply_type;
      ctx.Response.Write(item.Payload);
      ctx.Response.SuppressContent = supressContent;

      // validation caching
      ctx.Response.AddHeader("etag", item.ETag);
      ctx.Response.AppendHeader("Last-Modified", string.Format(CultureInfo.CurrentCulture, "{0:R}", item.LastModified));

      // expiration caching
      if (showExpires)
      {
        ctx.Response.AppendHeader("Expires", string.Format(CultureInfo.CurrentCulture, "{0:R}", item.Expires));
        ctx.Response.AppendHeader("cache-control", string.Format(CultureInfo.CurrentCulture, "max-age={0}", maxAge));
      }

      // hack to defeat custom IE caching
      if (ctx.Request.UserAgent != null && ctx.Request.UserAgent.IndexOf("IE", StringComparison.CurrentCultureIgnoreCase) != -1)
      {
        ctx.Response.AppendHeader("cache-control", "post-check=1,pre-check=2");
      }
    }

    // add a new task
    private void Post()
    {
      string id = string.Empty;

      // get the document from the stream
      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.Load(ctx.Request.InputStream);

      // get id from within the doc
      XmlNamespaceManager xmlNS = new XmlNamespaceManager(xmlDoc.NameTable);
      xmlNS.AddNamespace("s", sitkaNS);
      XmlNode node = xmlDoc.SelectSingleNode("//s:Id",xmlNS);
      if(node!=null)
      {
        id = node.InnerText;
      }
      if (id.Length==0)
      {
        throw new HttpException((int)HttpStatusCode.BadRequest, "missing id within document");
      }

      // send to the server
      client.Execute(endPoint, "POST", "application/xml", xmlDoc.OuterXml);

      // remove related local cache items
      cs.RemoveItem(endPoint + queryAll);

      // redirect client to the new location
      ctx.Response.ContentType = reply_type;
      ctx.Response.AddHeader("location", id);
      ctx.Response.StatusCode = (int)HttpStatusCode.RedirectMethod;
      ctx.Response.StatusDescription = HttpStatusCode.RedirectMethod.ToString();
      ctx.Response.Write("<moved href='" + id + "'/>");
    }

    // update an existing task
    private void Put()
    {
      string id = string.Empty;
      string pathId = (ctx.Request.QueryString["id"] != null ? ctx.Request.QueryString["id"] : string.Empty);

      // get the document from the stream
      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.Load(ctx.Request.InputStream);

      // get id from within the doc
      XmlNamespaceManager xmlNS = new XmlNamespaceManager(xmlDoc.NameTable);
      xmlNS.AddNamespace("s", sitkaNS);
      XmlNode node = xmlDoc.SelectSingleNode("//s:Id", xmlNS);
      if (node != null)
      {
        id = node.InnerText;
      }
      if (id.Length==0)
      {
        throw new HttpException((int)HttpStatusCode.BadRequest, "missing id");
      }
      // compare path id to doc id
      if (pathId != id)
      {
        throw new HttpException((int)HttpStatusCode.BadRequest, "path id and doc id mismatch");
      }

      // send to the server
      client.Execute(endPoint+"/"+id, "PUT", "application/xml", xmlDoc.OuterXml);

      // remove related local cache items
      cs.RemoveItem(endPoint + queryAll);
      cs.RemoveItem(endPoint + "/" + id);

      // handle response
      if (ctx.Request.UserAgent != null && ctx.Request.UserAgent.IndexOf("IE", StringComparison.CurrentCultureIgnoreCase) != -1)
      {
        // ie fails to redirect on PUT, so we just send OK
        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
        ctx.Response.StatusDescription = HttpStatusCode.OK.ToString();
      }
      else
      {
        // redirect to the updated record
        ctx.Response.ContentType = reply_type;
        ctx.Response.AddHeader("location", id);
        ctx.Response.StatusCode = (int)HttpStatusCode.RedirectMethod;
        ctx.Response.StatusDescription = HttpStatusCode.RedirectMethod.ToString();
        ctx.Response.Write("<moved href='" + id + "'/>");
      }
    }

    // delete an existing task
    private void Delete()
    {
      string id = (ctx.Request.QueryString["id"] != null ? ctx.Request.QueryString["id"] : string.Empty);
      if (id.Length==0)
      {
        throw new HttpException((int)HttpStatusCode.BadRequest, "missing id");
      }

      // send to the server
      client.Execute(endPoint + "/" + id, "DELETE", "application/xml");

      // remove related local cache items
      cs.RemoveItem(endPoint + queryAll);
      cs.RemoveItem(endPoint + "/" + id);

      // handle response
      if (ctx.Request.UserAgent!=null && ctx.Request.UserAgent.IndexOf("Safari", StringComparison.CurrentCultureIgnoreCase) != -1)
      {
        // safari 3 fails on 204, so we just send OK
        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
        ctx.Response.StatusDescription = HttpStatusCode.OK.ToString();
      }
      else
      {
        // send 204 (no content)
        throw new HttpException((int)HttpStatusCode.NoContent, HttpStatusCode.NoContent.ToString());
      }
    }

    private void HandleConfigSettings()
    {
      endPoint = wu.GetConfigSectionItem("ssdsSettings", "tasksEndPoint");
      queryFormat = wu.GetConfigSectionItem("ssdsSettings", "tasksQueryFormat");
      maxAge = Int32.Parse(wu.GetConfigSectionItem("ssdsSettings", "maxAge"),CultureInfo.CurrentCulture);
      showExpires = (wu.GetConfigSectionItem("ssdsSettings", "showExpires") == "true" ? true : false);
      ssdsUser = wu.GetConfigSectionItem("ssdsSettings", "ssdsUser");
      ssdsPassword = wu.GetConfigSectionItem("ssdsSettings", "ssdsPassword");
    }
  }
}
