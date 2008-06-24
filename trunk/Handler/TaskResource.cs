using System;
using System.Web;
using System.Xml;
using System.Net;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Configuration;
using System.Collections.Specialized;


namespace amundsen.ssds
{
  public class TaskResource : IHttpHandler
  {
    private CacheService cs = new CacheService();
    private HTTPClient client = new HTTPClient();
    private HttpContext ctx = null;
    private string method = string.Empty;
    private string[] media = null;
    private string ctype = string.Empty;

    const string all_recs = "?q=''";
    const string sitka_ns = "http://schemas.microsoft.com/sitka/2008/03/";
    DateTime if_modified_since_date = DateTime.MaxValue;
    const string error_format = "<s:Error xmlns:s='{2}'><s:Code>{0}</s:Code><s:Message>{1}</s:Message></s:Error>";

    // set via config file <ssdsSettings>
    string query_format = string.Empty;
    string endpoint = string.Empty;
    int max_age = 0;
    bool show_expires = false;
    string username = string.Empty;
    string password = string.Empty;

    bool IHttpHandler.IsReusable
    {
      get { return false; }
    }

    void IHttpHandler.ProcessRequest(HttpContext context)
    {
      // set up state info
      ctx = context;

      method = ctx.Request.HttpMethod.ToLower();
      media = ctx.Request.AcceptTypes;
      ctype = CheckMediaType();

      HandleConfigSettings();

      // set remote server credentials 
      client.Credentials = new NetworkCredential(username, password);

      // set compression (if supported)
      SetCompression(ctx);

      // process request
      try
      {
        switch (method)
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
          default:
            throw new HttpException((int)HttpStatusCode.MethodNotAllowed, HttpStatusCode.MethodNotAllowed.ToString());
        }
      }
      catch (HttpException hex)
      {
        ctx.Response.ContentType = "text/xml";
        ctx.Response.Write(string.Format(error_format, hex.GetHttpCode(), hex.Message,sitka_ns));
        ctx.Response.Write(" ".PadRight(500));
        ctx.Response.StatusCode = hex.GetHttpCode();
        ctx.Response.StatusDescription = hex.Message;

      }
      catch (Exception ex)
      {
        ctx.Response.ContentType = "text/xml";
        ctx.Response.Write(string.Format(error_format, 500, ex.Message,sitka_ns));
        ctx.Response.Write(" ".PadRight(500));
        ctx.Response.StatusCode = 500;
        ctx.Response.StatusDescription = ex.Message;
      }
    }

    private void Get()
    {
      Get(false);
    }
    private void Get(bool suppress_content)
    {
      string rtn = string.Empty;
      CacheItem item = null;

      // parse query line
      string id = (ctx.Request.QueryString["id"] != null ? "/" + ctx.Request.QueryString["id"] : string.Empty);
      string q = (ctx.Request.QueryString["q"] != null && ctx.Request.QueryString["q"]!=string.Empty ? string.Format(query_format,ctx.Server.UrlEncode(ctx.Request.QueryString["q"]).Replace("+","%20")) : all_recs);
      string query = (id != string.Empty ? id : q);
      string url = endpoint+query;

      // get request headers
      string if_none_match = GetHeader("if-none-match");
      string if_modified_since = GetHeader("if-modified-since");

      // check local cache first (if allowed)
      if (CheckNoCache()==true)
      {
        cs.RemoveItem(url);
      }
      else
      {
        item = cs.GetItem(url);
      }

      // did our copy expire?
      if (item != null && (item.Expires < DateTime.UtcNow || if_modified_since_date<item.LastModified))
      {
        cs.RemoveItem(url);
        item = null;
      }

      // ask server for new copy
      if (item == null)
      {
        rtn = client.Execute(url, "get", ctype);

        // fill local cache
        item = cs.PutItem(
          new CacheItem(
            url,
            rtn,
            string.Format("\"{0}\"", cs.MD5BinHex(rtn)),
            DateTime.UtcNow.AddSeconds(30),
            show_expires
          )
        );
      }
      else
      {
        // does client need new copy?
        if (if_none_match != item.ETag)
        {
          rtn = item.Payload;
        }
        else
        {
          throw new HttpException((int)HttpStatusCode.NotModified, HttpStatusCode.NotModified.ToString());
        }
      }

      // send response
      ctx.Response.ContentType = ctype;
      ctx.Response.Write(item.Payload);
      ctx.Response.SuppressContent = suppress_content;

      // validation caching
      ctx.Response.AddHeader("etag", item.ETag);
      ctx.Response.AppendHeader("Last-Modified", string.Format("{0:R}", item.LastModified));

      // expiration caching
      if (show_expires)
      {
        ctx.Response.AppendHeader("Expires", string.Format("{0:R}", item.Expires));
        ctx.Response.AppendHeader("cache-control", string.Format("max-age={0}", max_age));
      }

      // hack to force IE to refresh
      if (ctx.Request.UserAgent.IndexOf("IE") != -1)
      {
        ctx.Response.AppendHeader("cache-control", "post-check=1,pre-check=2");
      }
    }

    private void Post()
    {
      string rtn = string.Empty;
      string id = string.Empty;

      // get the document from the stream
      XmlDocument xml_doc = new XmlDocument();
      xml_doc.Load(ctx.Request.InputStream);

      // get id from within the doc
      XmlNamespaceManager xml_ns = new XmlNamespaceManager(xml_doc.NameTable);
      xml_ns.AddNamespace("s", sitka_ns);
      XmlNode node = xml_doc.SelectSingleNode("//s:Id",xml_ns);
      if(node!=null)
      {
        id = node.InnerText;
      }
      if (id == string.Empty)
      {
        throw new HttpException((int)HttpStatusCode.BadRequest, "missing id within document");
      }

      // send to the server
      rtn = client.Execute(endpoint, "POST", "application/xml", xml_doc.OuterXml);

      // remove related local cache item
      cs.RemoveItem(endpoint + all_recs);

      // redirect client to the new location
      ctx.Response.AddHeader("location", id);
      ctx.Response.StatusCode = (int)HttpStatusCode.RedirectMethod;
      ctx.Response.StatusDescription = HttpStatusCode.RedirectMethod.ToString();
      ctx.Response.Write("<moved href='" + id + "'/>");
    }

    private void Put()
    {
      string rtn = string.Empty;
      string id = string.Empty;
      string path_id = (ctx.Request.QueryString["id"] != null ? ctx.Request.QueryString["id"] : string.Empty);

      // get the document from the stream
      XmlDocument xml_doc = new XmlDocument();
      xml_doc.Load(ctx.Request.InputStream);

      // get id from within teh doc
      XmlNamespaceManager xml_ns = new XmlNamespaceManager(xml_doc.NameTable);
      xml_ns.AddNamespace("s", sitka_ns);
      XmlNode node = xml_doc.SelectSingleNode("//s:Id", xml_ns);
      if (node != null)
      {
        id = node.InnerText;
      }
      if (id == string.Empty)
      {
        throw new HttpException((int)HttpStatusCode.BadRequest, "missing id");
      }
      // compare path id to doc id
      if (path_id != id)
      {
        throw new HttpException((int)HttpStatusCode.BadRequest, "path id and doc id mismatch");
      }

      // send to the server
      rtn = client.Execute(endpoint+"/"+id, "PUT", "application/xml", xml_doc.OuterXml);

      // remove related local cache items
      cs.RemoveItem(endpoint+all_recs);
      cs.RemoveItem(endpoint + "/" + id);

      if (ctx.Request.UserAgent.IndexOf("IE") != -1)
      {
        // ie fails to redirect on PUT, so we just send OK
        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
        ctx.Response.StatusDescription = HttpStatusCode.OK.ToString();
      }
      else
      {
        ctx.Response.AddHeader("location", id);
        ctx.Response.StatusCode = (int)HttpStatusCode.RedirectMethod;
        ctx.Response.StatusDescription = HttpStatusCode.RedirectMethod.ToString();
        ctx.Response.Write("<moved href='" + id + "'/>");
      }
    }

    private void Delete()
    {
      string rtn = string.Empty;
      string id = (ctx.Request.QueryString["id"] != null ? ctx.Request.QueryString["id"] : string.Empty);

      if (id==string.Empty)
      {
        throw new HttpException((int)HttpStatusCode.BadRequest, "missing id");
      }

      // send to the server
      rtn = client.Execute(endpoint + "/" + id, "DELETE", "application/xml");

      // remove related local cache items
      cs.RemoveItem(endpoint + all_recs);
      cs.RemoveItem(endpoint + "/" + id);

      // nothing to pass along here
      if (ctx.Request.UserAgent.IndexOf("Safari") != -1)
      {
        // safari 3 fails on 204, so we just send OK
        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
        ctx.Response.StatusDescription = HttpStatusCode.OK.ToString();
      }
      else
      {
        throw new HttpException((int)HttpStatusCode.NoContent, HttpStatusCode.NoContent.ToString());
      }
    }

    private string CheckMediaType()
    {
      string rtn = string.Empty;

      if (method == "get" || method=="head" || method=="options")
      {
        for (int i = 0; i < media.Length; i++)
        {
          if (media[i] == "*/*")
          {
            rtn = "application/xml";
            goto done;
          }
          if (media[i].ToLower().IndexOf("xml") != -1)
          {
            rtn = "application/xml";
            goto done;
          }
        }
      }
      else
      {
        if (method=="delete" || ctx.Request.ContentType.ToLower().IndexOf("xml") != -1)
        {
          rtn = "application/xml";
          goto done;
        }
      }

      done:
      if (rtn == string.Empty)
      {
        throw new HttpException((int)HttpStatusCode.NotAcceptable, HttpStatusCode.NotAcceptable.ToString());
      }

      return rtn;
    }

    private string GetHeader(string key)
    {
      string rtn = string.Empty;

      if (ctx.Request.Headers[key] != null)
        return ctx.Request.Headers[key];
      else
        return string.Empty;
    }

    private bool CheckNoCache()
    {
      string pragma = GetHeader("pragma");
      string cache_control = GetHeader("cache-control");
      return (pragma.ToLower().IndexOf("no-cache") != -1 || cache_control.ToLower().IndexOf("no-cache") != -1);
    }

    private void SetCompression(HttpContext ctx)
    {
      string enc_header = GetHeader("accept-encoding");
      if (enc_header.Contains("gzip"))
      {
        ctx.Response.Filter = new GZipStream(ctx.Response.Filter, CompressionMode.Compress);
        ctx.Response.AppendHeader("Content-Encoding", "gzip");
      }
      else if (enc_header.Contains("deflate"))
      {
        ctx.Response.Filter = new DeflateStream(ctx.Response.Filter, CompressionMode.Compress);
        ctx.Response.AppendHeader("Content-Encoding", "deflate");
      }
    }

    private void HandleConfigSettings()
    {
      endpoint = GetConfigSectionItem("ssdsSettings", "endPoint");
      query_format = GetConfigSectionItem("ssdsSettings", "queryFormat");
      max_age = Int32.Parse(GetConfigSectionItem("ssdsSettings", "maxAge"));
      show_expires = (GetConfigSectionItem("ssdsSettings", "showExpires")=="true"?true:false);
      username = GetConfigSectionItem("ssdsSettings", "userName");
      password = GetConfigSectionItem("ssdsSettings", "password");
    }

    // get item from selected section in config file
    private string GetConfigSectionItem(string section, string key)
    {
      return GetConfigSectionItem(section, key, "");
    }
    private string GetConfigSectionItem(string section, string key, string defaultValue)
    {
      if (ConfigurationManager.GetSection(section) != null)
      {
        NameValueCollection coll = (NameValueCollection)ConfigurationManager.GetSection(section);
        if (coll[key] != null)
          return coll[key].ToString();
        else
          return defaultValue;
      }
      else
        return defaultValue;
    }
  }
}
