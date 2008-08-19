using System;
using System.Web;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Globalization;
using System.Text.RegularExpressions;

using Amundsen.Utilities;

namespace Amundsen.SSDS.GuestBook
{
  /// <summary>
  /// Public Domain 2008 amundsen.com, inc.
  /// @author mike amundsen (mamund@yahoo.com)
  /// @version 1.3 (2008-08-10)
  /// @version 1.2 (2008-08-06)
  /// @version 1.1 (2008-08-05)
  /// @version 1.0 (2008-08-01)
  /// 
  /// uses SSDS Proxy server (http://amundsen.com/ssds-proxy/)
  /// GET, HEAD, POST, OPTIONS  /guestbook/messages/
  /// GET, HEAD                 /guestbook/messages/{nickname}
  /// </summary>
  class Messages : IHttpHandler
  {
    private WebUtility wu;
    private HttpClient client;
    private HttpContext ctx;
    private CacheService cs;
    private Hashing h;
    private Xslt x = new Xslt();

    static string ssdsUser = string.Empty;
    static string ssdsPassword = string.Empty;
    static string ssdsProxy = string.Empty;

    bool showExpires = false;
    int maxAge = 60;
    string msft_request = string.Empty;

    string authority = "mikeamundsen";
    string container = "guestbook";
    string message_list_query = @"from e in entities where e.Kind==""message"" select e";
    string message_nick_query = @"from e in entities where e.Kind==""message"" %2526%2526 e[""nickname""]==""{0}"" select e";

    bool IHttpHandler.IsReusable
    {
      get { return false; }
    }

    public void ProcessRequest(HttpContext context)
    {
      ctx = context;
      client = new HttpClient();
      wu = new WebUtility();
      cs = new CacheService();
      h = new Hashing();

      // process request
      try
      {
        string rtn = wu.ConfirmXmlMediaType(ctx.Request.AcceptTypes, Constants.SsdsType);
        if (rtn.Length == 0)
        {
          Options();
          throw new HttpException((int)HttpStatusCode.NotAcceptable, HttpStatusCode.NotAcceptable.ToString());
        }

        HandleConfigSettings();
        client.RequestHeaders.Add("authorization", "Basic " + h.Base64Encode(string.Format("{0}:{1}", ssdsUser, ssdsPassword)));

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
      ctx.Response.AppendHeader("X-Acceptable", "application/x-ssds+xml,application/xml,text/xml");
    }

    // get metadata for requested authority
    private void Get()
    {
      Get(false);
    }
    private void Get(bool supressContent)
    {
      string rtn = string.Empty;
      string entity = string.Empty;
      string url = string.Empty;
      string query = string.Empty;
      string request_url = string.Empty;
      string rss = string.Empty;
      string filter = string.Empty;
      CacheItem item = null;

      // look for args
      entity = (ctx.Request.QueryString["entity"] != null ? ctx.Request.QueryString["entity"] : string.Empty);
      // make sure you only return a message document
      if (entity.Length != 0)
      {
        // return messages for passed in nickname
        filter = entity;
        entity = "?" + ctx.Server.UrlPathEncode(string.Format(message_nick_query, entity));
      }
      else
      {
        // just return list of messages
        entity = "?" + ctx.Server.UrlPathEncode(message_list_query);
      }

      // see if rss flag appears
      rss = (ctx.Request.QueryString["rss"] != null ? ctx.Request.QueryString["rss"] : string.Empty);

      request_url = ctx.Request.Url.ToString();
      string ifNoneMatch = wu.GetHeader(ctx, "if-none-match");
      bool noCache = (wu.GetHeader(ctx, "cache-control").IndexOf("no-cache") != -1 ? true : false);

      // check local cache first (if allowed)
      if (wu.CheckNoCache(ctx) == true)
      {
        ifNoneMatch = string.Empty;
        cs.RemoveItem(request_url);
      }
      else
      {
        item = cs.GetItem(request_url);
      }

      // did our copy expire?
      if (item != null && (item.Expires < DateTime.UtcNow))
      {
        cs.RemoveItem(request_url);
        item = null;
      }

      if (item == null)
      {
        // handle query
        url = string.Format(CultureInfo.CurrentCulture, "{0}{1}/{2}/{3}", ssdsProxy,authority, container, entity);
        if (noCache)
        {
          client.RequestHeaders.Add("cache-control", "no-cache");
        }
        rtn = client.Execute(url, "get", Constants.SsdsType);
        msft_request = (client.ResponseHeaders[Constants.MsftRequestId] != null ? client.ResponseHeaders[Constants.MsftRequestId] : string.Empty);

        // make sure we got at least one message document
        XmlDocument xmldoc = new XmlDocument();
        xmldoc.LoadXml(rtn);
        XmlNodeList nodes = xmldoc.SelectNodes("//message");
        if (nodes.Count == 0)
        {
          throw new HttpException(404, string.Format("Message record not found [{0}]", entity));
        }

        // generate RSS, if requested
        if (rss != string.Empty)
        {
          XmlDocument xsldoc = new XmlDocument();
          xsldoc.Load(ctx.Server.MapPath("~/transforms/messages-rss.xsl"));
          XsltArgumentList args = new XsltArgumentList();
          args.AddParam("max-rows", "", (filter.Length==0?"25":"10"));
          args.AddParam("build-date", "", DateTime.UtcNow.ToString("r"));
          args.AddParam("title", "", string.Format("{0}'s SSDS Guestbook Messages",(filter.Length!=0?filter:"Everyone")));
          rtn = x.Transform(xmldoc, xsldoc, args);
          rtn = rtn.Replace(@"encoding=""utf-16""", @"encoding=""utf-8""");
        }

        // fill local cache
        item = cs.PutItem(
          new CacheItem(
            request_url,
            rtn,
            string.Format(CultureInfo.CurrentCulture, "\"{0}\"", cs.MD5BinHex(rtn)),
            DateTime.UtcNow.AddSeconds(maxAge),
            showExpires
          )
        );
      }

      // if client has same copy, just send 304
      if (ifNoneMatch == item.ETag)
      {
        throw new HttpException((int)HttpStatusCode.NotModified, HttpStatusCode.NotModified.ToString());
      }

      // compose response to client
      ctx.Response.SuppressContent = supressContent;
      ctx.Response.StatusCode = 200;
      ctx.Response.ContentType = (rss=="yes"?"application/rss+xml":"text/xml");
      ctx.Response.StatusDescription = "OK";
      ctx.Response.Write(item.Payload);

      // add msft_header, if present
      if (msft_request.Length != 0)
      {
        ctx.Response.AppendToLog(string.Format(" [{0}={1}]", Constants.MsftRequestId, msft_request));
        ctx.Response.AddHeader(Constants.MsftRequestId, msft_request);
      }

      // validation caching
      ctx.Response.AddHeader("etag", item.ETag);
      ctx.Response.AppendHeader("Last-Modified", string.Format(CultureInfo.CurrentCulture, "{0:R}", item.LastModified));

      // expiration caching
      if (showExpires)
      {
        ctx.Response.AppendHeader("Expires", string.Format(CultureInfo.CurrentCulture, "{0:R}", item.Expires));
        ctx.Response.AppendHeader("cache-control", string.Format(CultureInfo.CurrentCulture, "max-age={0}", maxAge));
      }

      // ie local cache hack
      if (ctx.Request.UserAgent != null && ctx.Request.UserAgent.IndexOf("IE", StringComparison.CurrentCultureIgnoreCase) != -1)
      {
        ctx.Response.AppendHeader("cache-control", "no-cache,post-check=1,pre-check=2");
      }
    }

    private void Post()
    {
      string rtn = string.Empty;
      string data = string.Empty;
      string body = string.Empty;
      string entityId = string.Empty;

      string valid_password = string.Empty;
      string nickname = string.Empty;
      string password = string.Empty;
      string message = string.Empty;
      string url = string.Empty;

      // hack to support isapi rewrite utility
      // added to support proper returning location header in response
      string request_url = (ctx.Request.Headers["X-Rewrite-URL"] != null ? ctx.Request.Headers["X-Rewrite-URL"] : ctx.Request.Url.ToString());

      // handle local login stuff
      try
      {
        wu.GetBasicAuthCredentials(ctx, ref nickname, ref password, "guestbook_auth");

        rtn = client.Execute(string.Format("{0}{1}/{2}/{3}", ssdsProxy, authority, container, nickname), "get", Constants.SsdsType);
        XmlDocument xmldoc = new XmlDocument();
        xmldoc.LoadXml(rtn);
        XmlNode node = xmldoc.SelectSingleNode("//password");
        if (node != null)
        {
          valid_password = node.InnerText;
        }
        if (valid_password != h.MD5BinHex(password) && valid_password!=password)
        {
          throw new HttpException(403, "Not Authorized");
        }
      }
      catch (HttpException hex)
      {
        if (hex.GetHttpCode() > 399)
        {
          throw new HttpException(403, string.Format("Not Authorized [{0}]", hex.GetHttpCode()));
        }
      }

      // get form arguments
      message = (ctx.Request.Form["message"] != null ? ctx.Request.Form["message"].Trim() : string.Empty);
      if (message.Length == 0)
      {
        throw new HttpException(400, "Missing Message");
      }

      // compose valid SSDS Entity
      entityId = wu.MakeDescendingId();
      data = EntityTemplate.Replace("$id$", entityId).Replace("$nickname$", nickname).Replace("$message$", message).Replace("$date$", DateTime.UtcNow.ToString());
      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.LoadXml(data);

      // handle request to remote server
      url = string.Format(CultureInfo.CurrentCulture, "{0}{1}/{2}/", ssdsProxy, authority, container);
      rtn = client.Execute(url, "post", Constants.SsdsType, xmlDoc.OuterXml);

      // refresh cache
      cs.RemoveItem(ctx.Request.Url.ToString());
      client.RequestHeaders.Add("cache-control", "no-cache");
      rtn = client.Execute(string.Format(CultureInfo.CurrentCulture, "{0}{1}/{2}/?{3}&rss=yes", ssdsProxy, authority, container, message_list_query), "get", Constants.SsdsType);
      rtn = client.Execute(string.Format(CultureInfo.CurrentCulture, "{0}{1}/{2}/?{3}", ssdsProxy, authority, container, message_list_query), "get", Constants.SsdsType);
      msft_request = (client.ResponseHeaders[Constants.MsftRequestId] != null ? client.ResponseHeaders[Constants.MsftRequestId] : string.Empty);

      // add msft_header, if present
      if (msft_request.Length != 0)
      {
        ctx.Response.AppendToLog(string.Format(" [{0}={1}]", Constants.MsftRequestId, msft_request));
        ctx.Response.AddHeader(Constants.MsftRequestId, msft_request);
      }

      // compose response to client
      ctx.Response.StatusCode = 201;
      ctx.Response.ContentType = "text/xml";
      ctx.Response.StatusDescription = "Entity has been created.";
      ctx.Response.RedirectLocation = string.Format("{0}{1}", request_url, entityId);
      ctx.Response.Write(rtn);
    }

    private static void HandleConfigSettings()
    {
      WebUtility wu = new WebUtility();
      ssdsUser = wu.GetConfigSectionItem("ssdsSettings", "ssdsUser");
      ssdsPassword = wu.GetConfigSectionItem("ssdsSettings", "ssdsPassword");
      ssdsProxy = wu.GetConfigSectionItem("ssdsSettings", "ssdsProxy");
    }

    string EntityTemplate = @"<message xmlns:s=""http://schemas.microsoft.com/sitka/2008/03/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:x=""http://www.w3.org/2001/XMLSchema"">
  <s:Id>$id$</s:Id>
  <date-created xsi:type=""x:dateTime"">$date$</date-created>
  <nickname xsi:type=""x:string"">$nickname$</nickname>
  <body xsi:type=""x:string"">$message$</body>
</message>";

  }
}
