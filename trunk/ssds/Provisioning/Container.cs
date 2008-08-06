using System;
using System.Web;
using System.Net;
using System.IO;
using System.Globalization;

using Amundsen.Utilities;

namespace Amundsen.SSDS.Provisioning
{
  /// <summary>
  /// Public Domain 2008 amundsen.com, inc.
  /// @author mike amundsen (mamund@yahoo.com)
  /// @version 1.4 (2008-08-05)
  /// @version 1.3 (2008-07-20)
  /// @version 1.2 (2008-07-13)
  /// @version 1.1 (2008-07-09)
  /// @version 1.0 (2008-07-03)
  /// </summary>
  class Container : IHttpHandler
  {
    private WebUtility wu;
    private HttpClient client;
    private HttpContext ctx;
    private CacheService cs;

    string ssdsUser = string.Empty;
    string ssdsPassword = string.Empty;
    bool showExpires = false;
    int maxAge = 60;
    string msft_request = string.Empty;

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
        string rtn = wu.ConfirmXmlMediaType(ctx.Request.AcceptTypes,Constants.SsdsType);
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
          case "delete":
            Delete();
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
      ctx.Response.AppendHeader("Allow", "GET,HEAD,POST,DELETE,OPTIONS");
      ctx.Response.AppendHeader("X-Acceptable", "application/xml,text/xml");
    }

    // get metadata for requested container or a list of containers
    private void Get()
    {
      Get(false);
    }
    private void Get(bool supressContent)
    {
      string rtn = string.Empty;
      string authority = string.Empty;
      string container = string.Empty;
      string url = string.Empty;
      string request_url = string.Empty;
      CacheItem item = null;

      // get args
      authority = (ctx.Request.QueryString["authority"] != null ? ctx.Request.QueryString["authority"] : string.Empty);
      if (authority.Length==0)
      {
        throw new HttpException(400, "Missing Authority ID");
      }

      container = (ctx.Request.QueryString["container"] != null ? ctx.Request.QueryString["container"] : string.Empty);

      request_url = ctx.Request.Url.ToString();
      string ifNoneMatch = wu.GetHeader(ctx, "if-none-match");

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
        url = string.Format(CultureInfo.CurrentCulture, "https://{0}.{1}{2}{3}", authority, Constants.SsdsRoot, container, (container.Length!=0?"":Constants.QueryAll));
        rtn = client.Execute(url, "get", Constants.SsdsType);
        msft_request = (client.ResponseHeaders[Constants.MsftRequestId] != null ? client.ResponseHeaders[Constants.MsftRequestId] : string.Empty);

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
      ctx.Response.ContentType = "text/xml";
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

    // create a new container
    private void Post()
    {
      string rtn = string.Empty;
      string data = string.Empty;
      string body = string.Empty;
      string authority = string.Empty;
      string container = string.Empty;
      string url = string.Empty;

      // hack to support isap rewrite utility
      // added to support proper returning location header in response
      string request_url = (ctx.Request.Headers["X-Rewrite-URL"] != null ? ctx.Request.Headers["X-Rewrite-URL"] : ctx.Request.Url.ToString());

      // get args
      authority = (ctx.Request.QueryString["authority"] != null ? ctx.Request.QueryString["authority"] : string.Empty);
      if (authority.Length==0)
      {
        throw new HttpException(400, "Missing Authority ID");
      }

      // get body (container to create)
      using (StreamReader sr = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
      {
        body = sr.ReadToEnd();
        sr.Close();
      }

      container = body.ToLower(CultureInfo.CurrentCulture).Replace("<container>", "").Replace("</container>", "");
      if (container.Length==0)
      {
        throw new HttpException(400, "Missing Container ID");
      }

      // handle request to remote server
      data = string.Format(CultureInfo.CurrentCulture, Constants.ContainerFormat, container, Constants.SitkaNS);
      url = string.Format(CultureInfo.CurrentCulture, "https://{0}.{1}", authority, Constants.SsdsRoot);
      rtn = client.Execute(url, "post", Constants.SsdsType, data);
      msft_request = (client.ResponseHeaders[Constants.MsftRequestId] != null ? client.ResponseHeaders[Constants.MsftRequestId] : string.Empty);

      // clear cache
      cs.RemoveItem(ctx.Request.Url.ToString());

      // add msft_header, if present
      if (msft_request.Length != 0)
      {
        ctx.Response.AppendToLog(string.Format(" [{0}={1}]", Constants.MsftRequestId, msft_request));
        ctx.Response.AddHeader(Constants.MsftRequestId, msft_request);
      }

      // compose response to client
      ctx.Response.StatusCode = 201;
      ctx.Response.ContentType = "text/xml";
      ctx.Response.StatusDescription = "Container has been created.";
      ctx.Response.RedirectLocation = string.Format("{0}{1}", request_url, container);
      ctx.Response.Write(rtn);
    }

    // delete an existing container
    private void Delete()
    {
      string rtn = string.Empty;
      string authority = string.Empty;
      string container = string.Empty;
      string url = string.Empty;
      string request_url = string.Empty;

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

      // handle request to remote server
      url = string.Format(CultureInfo.CurrentCulture, "https://{0}.{1}{2}", authority, Constants.SsdsRoot, container);
      rtn = client.Execute(url, "delete", Constants.SsdsType);
      msft_request = (client.ResponseHeaders[Constants.MsftRequestId] != null ? client.ResponseHeaders[Constants.MsftRequestId] : string.Empty);

      // clear cache
      cs.RemoveItem(ctx.Request.Url.ToString());
      cs.RemoveItem(ctx.Request.Url.ToString().Replace(container, ""));
      cs.RemoveItem(ctx.Request.Url.ToString().Replace("container.ssds", "entity.ssds")+"&entity=");

      // add msft_header, if present
      if (msft_request.Length != 0)
      {
        ctx.Response.AppendToLog(string.Format(" [{0}={1}]", Constants.MsftRequestId, msft_request));
        ctx.Response.AddHeader(Constants.MsftRequestId, msft_request);
      }

      // compose response to client
      ctx.Response.StatusCode = 200;
      ctx.Response.ContentType = "text/xml";
      ctx.Response.StatusDescription = "OK";
      ctx.Response.Write(rtn);
    }
  }
}
