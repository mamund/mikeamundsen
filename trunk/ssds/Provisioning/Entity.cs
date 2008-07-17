using System;
using System.Web;
using System.Xml;
using System.Net;
using System.Globalization;

using Amundsen.Utilities;

namespace Amundsen.SSDS.Provisioning
{
  /// <summary>
  /// Public Domain 2008 amundsen.com, inc.
  /// @author mike amundsen (mamund@yahoo.com)
  /// @version 1.2 (2008-07-13)
  /// @version 1.1 (2008-07-09)
  /// @version 1.0 (2008-07-03)
  /// </summary>
  class Entity : IHttpHandler
  {
    private WebUtility wu;
    private HttpClient client;
    private HttpContext ctx;
    private CacheService cs;

    string ssdsUser = string.Empty;
    string ssdsPassword = string.Empty;
    bool showExpires = false;
    int maxAge = 60;

    bool IHttpHandler.IsReusable
    {
      get { return false; }
    }

    void IHttpHandler.ProcessRequest(HttpContext context)
    {
      ctx = context;
      client = new HttpClient();
      wu = new WebUtility();
      cs = new CacheService();

      // process request
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
      ctx.Response.AppendHeader("Allow", "GET,HEAD,POST,PUT,DELETE,OPTIONS");
      ctx.Response.AppendHeader("X-Acceptable", "application/xml,text/xml");
    }

    // get entity or list of entities
    private void Get()
    {
      Get(false);
    }
    private void Get(bool supressContent)
    {
      string rtn = string.Empty;
      string authority = string.Empty;
      string container = string.Empty;
      string entity = string.Empty;
      string url = string.Empty;
      string query = string.Empty;
      string request_url = string.Empty;
      CacheItem item = null;

      // get args
      authority = (ctx.Request.QueryString["authority"] != null ? ctx.Request.QueryString["authority"] : string.Empty);
      if (authority.Trim().Length==0)
      {
        throw new HttpException(400, "Missing Authority ID");
      }

      container = (ctx.Request.QueryString["container"] != null ? ctx.Request.QueryString["container"] : string.Empty);
      if (container.Trim().Length == 0)
      {
        throw new HttpException(400, "Missing Container ID");
      }

      // look for passed query
      query = (ctx.Request.QueryString["query"] != null ? ctx.Request.QueryString["query"] : string.Empty);

      entity = (ctx.Request.QueryString["entity"] != null ? ctx.Request.QueryString["entity"] : string.Empty);
      if (entity.Trim().Length == 0)
      {
        if (query.Trim().Length == 0)
        {
          entity = Constants.QueryAll;
        }
        else
        {
          entity = string.Format("?q='{0}'", query.Trim());
        }
      }
      else
      {
        entity = "/" + entity.Trim();
      }

      // check local cache first (if allowed)
      request_url = ctx.Request.Url.ToString();
      string ifNoneMatch = wu.GetHeader(ctx, "if-none-match");
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
        url = string.Format(CultureInfo.CurrentCulture, "https://{0}.{1}{2}{3}", authority, Constants.SsdsRoot, container, entity);
        rtn = client.Execute(url, "get", Constants.SsdsType);

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

      // does client have good copy?
      if (ifNoneMatch == item.ETag)
      {
        throw new HttpException((int)HttpStatusCode.NotModified, HttpStatusCode.NotModified.ToString());
      }

      // compose response to client
      ctx.Response.SuppressContent = supressContent;
      ctx.Response.StatusCode = 200;
      ctx.Response.ContentType = "text/xml";
      ctx.Response.StatusDescription = "OK";
      ctx.Response.Write(item.Payload);

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

    // create a new entity
    private void Post()
    {
      string rtn = string.Empty;
      string data = string.Empty;
      string body = string.Empty;
      string authority = string.Empty;
      string container = string.Empty;
      string entityId = string.Empty;

      string url = string.Empty;

      // get args
      authority = (ctx.Request.QueryString["authority"] != null ? ctx.Request.QueryString["authority"] : string.Empty);
      if (authority.Length==0)
      {
        throw new HttpException(400, "Missing Authority ID");
      }

      container = (ctx.Request.QueryString["container"] != null ? ctx.Request.QueryString["container"] : string.Empty);
      if (container.Length == 0)
      {
        throw new HttpException(400, "Missing Container ID");
      }

      // get request body
      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.Load(ctx.Request.InputStream);

      // get id from within the doc
      XmlNamespaceManager xmlNS = new XmlNamespaceManager(xmlDoc.NameTable);
      xmlNS.AddNamespace("s", Constants.SitkaNS);
      XmlNode node = xmlDoc.SelectSingleNode("//s:Id", xmlNS);
      if (node != null)
      {
        entityId = node.InnerText;
      }
      if (entityId.Length == 0)
      {
        throw new HttpException((int)HttpStatusCode.BadRequest, "Missing ID within document");
      }

      // parse entity macro, if needed
      switch (entityId)
      {
        case "$id$":
        case "$id-asc$":
          xmlDoc.SelectSingleNode("//s:Id", xmlNS).InnerText = wu.MakeAscendingId(); 
          break;
        case "$id-desc$":
          xmlDoc.SelectSingleNode("//s:Id", xmlNS).InnerText = wu.MakeDescendingId();
          break;
        case "$guid$":
          xmlDoc.SelectSingleNode("//s:Id", xmlNS).InnerText = Guid.NewGuid().ToString();
          break;
      }

      // handle request to remote server
      url = string.Format(CultureInfo.CurrentCulture, "https://{0}.{1}{2}", authority, Constants.SsdsRoot, container);
      rtn = client.Execute(url, "post", Constants.SsdsType, xmlDoc.OuterXml);

      // clear cache
      cs.RemoveItem(ctx.Request.Url.ToString());

      // compose response to client
      ctx.Response.StatusCode = 201;
      ctx.Response.ContentType = "text/xml";
      ctx.Response.StatusDescription = "Entity has been created.";
      ctx.Response.Write(rtn);
    }

    // replace existing entity
    private void Put()
    {
      string rtn = string.Empty;
      string data = string.Empty;
      string body = string.Empty;
      string authority = string.Empty;
      string container = string.Empty;
      string entity = string.Empty;
      string entityId = string.Empty;
      string url = string.Empty;
      string request_url = string.Empty;
      string etag = string.Empty;
      string if_match = string.Empty;

      // get copy of complete url
      request_url = ctx.Request.Url.ToString();

      // get args
      authority = (ctx.Request.QueryString["authority"] != null ? ctx.Request.QueryString["authority"] : string.Empty);
      if (authority.Length == 0)
      {
        throw new HttpException(400, "Missing Authority ID");
      }

      container = (ctx.Request.QueryString["container"] != null ? ctx.Request.QueryString["container"] : string.Empty);
      if (container.Length == 0)
      {
        throw new HttpException(400, "Missing Container ID");
      }

      entity = (ctx.Request.QueryString["entity"] != null ? ctx.Request.QueryString["entity"] : string.Empty);
      if (container.Length == 0)
      {
        throw new HttpException(400, "Missing Entity ID");
      }

      // compose url for remote server call
      url = string.Format(CultureInfo.CurrentCulture, "https://{0}.{1}{2}/{3}", authority, Constants.SsdsRoot, container, entity);

      // make sure we have an if-match header
      if_match = wu.GetHeader(ctx,"if-match");
      if (if_match.Length == 0)
      {
        throw new HttpException((int)HttpStatusCode.PreconditionFailed, "Missing If-Match header");
      }

      // now get the current record's etag
      rtn = client.Execute(request_url, "head", Constants.SsdsType);
      etag = client.ResponseHeaders["etag"];
      if (etag != if_match)
      {
        throw new HttpException((int)HttpStatusCode.PreconditionFailed, "ETag and If-Match headers do not match");
      }

      // get passed in request body
      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.Load(ctx.Request.InputStream);

      // validate id from within the doc
      XmlNamespaceManager xmlNS = new XmlNamespaceManager(xmlDoc.NameTable);
      xmlNS.AddNamespace("s", Constants.SitkaNS);
      XmlNode node = xmlDoc.SelectSingleNode("//s:Id", xmlNS);
      if (node != null)
      {
        entityId = node.InnerText;
      }
      if (entityId.Length == 0)
      {
        throw new HttpException((int)HttpStatusCode.BadRequest, "Missing ID within document");
      }
      if (entityId != entity)
      {
        throw new HttpException((int)HttpStatusCode.BadRequest, "Path ID and Document ID mismatch");
      }

      // handle PUT to remote server
      rtn = client.Execute(url, "put", Constants.SsdsType, xmlDoc.OuterXml);

      // clear local cache
      cs.RemoveItem(request_url);
      cs.RemoveItem(request_url.Replace(entity, ""));

      // update local copy w/ new document
      // (cuz ssds returns wrong s:Version for PUTs!)
      rtn = client.Execute(url, "get", Constants.SsdsType);

      CacheItem item = cs.PutItem(
        new CacheItem(
          request_url,
          rtn,
          string.Format(CultureInfo.CurrentCulture, "\"{0}\"", cs.MD5BinHex(rtn)),
          DateTime.UtcNow.AddSeconds(maxAge),
          showExpires
        )
      );

      // compose response to client
      ctx.Response.StatusCode = 200;
      ctx.Response.ContentType = "text/xml";
      ctx.Response.StatusDescription = "OK";
      ctx.Response.Write(item.Payload);
    }

    // delete an existing entity
    private void Delete()
    {
      string rtn = string.Empty;
      string authority = string.Empty;
      string container = string.Empty;
      string entity = string.Empty;
      string url = string.Empty;

      // get args
      authority = (ctx.Request.QueryString["authority"] != null ? ctx.Request.QueryString["authority"] : string.Empty);
      if (authority.Length==0)
      {
        throw new HttpException(400, "Missing Authority ID");
      }

      container = (ctx.Request.QueryString["container"] != null ? ctx.Request.QueryString["container"] : string.Empty);
      if (container.Length==0)
      {
        throw new HttpException(400, "Missing Container ID");
      }

      entity = (ctx.Request.QueryString["entity"] != null ? ctx.Request.QueryString["entity"] : string.Empty);
      if (entity.Length == 0)
      {
        throw new HttpException(400, "Missing Entity ID");
      }

      // handle request to remote server
      url = string.Format(CultureInfo.CurrentCulture, "https://{0}.{1}{2}/{3}", authority, Constants.SsdsRoot, container,entity);
      rtn = client.Execute(url, "delete", Constants.SsdsType);

      // clear cache
      cs.RemoveItem(ctx.Request.Url.ToString());
      cs.RemoveItem(ctx.Request.Url.ToString().Replace(entity, ""));

      // compose response to client
      ctx.Response.StatusCode = 200;
      ctx.Response.ContentType = "text/xml";
      ctx.Response.StatusDescription = "OK";
      ctx.Response.Write(rtn);
    }
  }
}
