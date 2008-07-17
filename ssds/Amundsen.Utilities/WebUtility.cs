using System;
using System.Collections.Specialized;
using System.Web;
using System.Net;
using System.IO.Compression;
using System.Configuration;
using System.Globalization;

namespace Amundsen.Utilities
{
  /// <summary>
  /// Public Domain 2008 amundsen.com, inc.
  /// @author mike amundsen (mamund@yahoo.com)
  /// @version 1.0 (2008-07-03)
  /// </summary>
  public class WebUtility
  {
    public string ConfirmXmlMediaType(string[] media)
    {
      return ConfirmXmlMediaType(media, "application/xml");
    }
    public string ConfirmXmlMediaType(string[] media, string returnType)
    {
      string rtn = string.Empty;
      string mt = string.Empty;
      for (int i = 0; i < media.Length; i++)
      {
        mt = media[i];
        if (mt.IndexOf("/*", StringComparison.CurrentCultureIgnoreCase) != -1)
        {
          rtn = returnType;
          break;
        }
        if (mt.IndexOf("xml", StringComparison.CurrentCultureIgnoreCase) != -1)
        {
          rtn = returnType;
          break;
        }

      }
      return rtn;
    }

    public bool CheckNoCache(HttpContext context)
    {
      string pragma = GetHeader(context, "pragma");
      string cache_control = GetHeader(context, "cache-control");
      return (pragma.IndexOf("no-cache", StringComparison.CurrentCultureIgnoreCase) != -1 || cache_control.IndexOf("no-cache", StringComparison.CurrentCultureIgnoreCase) != -1);
    }

    public string GetHeader(HttpContext context, string key)
    {
      if (context.Request.Headers[key] != null)
        return context.Request.Headers[key];
      else
        return string.Empty;
    }

    public void SetCompression(HttpContext context)
    {
      string encodingHeader = GetHeader(context,"accept-encoding");
      if (encodingHeader.Contains("gzip"))
      {
        context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
        context.Response.AppendHeader("Content-Encoding", "gzip");
      }
      else if (encodingHeader.Contains("deflate"))
      {
        context.Response.Filter = new DeflateStream(context.Response.Filter, CompressionMode.Compress);
        context.Response.AppendHeader("Content-Encoding", "deflate");
      }
    }

    // get item from selected section in config file
    public string GetConfigSectionItem(string section, string key)
    {
      return GetConfigSectionItem(section, key, "");
    }
    public string GetConfigSectionItem(string section, string key, string defaultValue)
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

    public string CookieRead(HttpContext ctx, string key)
    {
      if (ctx.Request.Cookies[key] != null)
        return ctx.Request.Cookies[key].Value;
      else
        return string.Empty;
    }
    public void CookieClear(HttpContext ctx, string key)
    {
      if (ctx.Request.Cookies[key] != null)
      {
        HttpCookie aCookie = new HttpCookie(key);
        aCookie.Expires = DateTime.UtcNow.AddDays(-1);
        ctx.Response.Cookies.Add(aCookie);
      }
    }
    public void CookieWrite(HttpContext ctx, string key, string value)
    { 
      CookieWrite(ctx, key, value, 0, "");
    }
    public void CookieWrite(HttpContext ctx, string key, string value, string path)
    {
      CookieWrite(ctx, key, value, 0, path);
    }
    public void CookieWrite(HttpContext ctx, string key, string value, double days, string path)
    {
      HttpCookie aCookie = new HttpCookie(key);
      aCookie.Value = value;
      if (days > 0)
        aCookie.Expires = DateTime.UtcNow.AddDays(days);
      if (path.Length != 0)
        aCookie.Path = path;

      ctx.Response.Cookies.Add(aCookie);
    }

    public string MakeAscendingId()
    {
      DateTimePrecise dtp = new DateTimePrecise(10);
      DateTime dt = dtp.UtcNow;
      return dt.Ticks.ToString();
    }

    public string MakeDescendingId()
    {
      DateTimePrecise dtp = new DateTimePrecise(10);
      DateTime future = new DateTime(2100, 1, 1, 0, 0, 1);
      return Convert.ToString(future.Ticks - dtp.UtcNow.Ticks);
    }

    public void GetBasicAuthCredentials(HttpContext ctx, ref string username, ref string password)
    {
      Hashing h = new Hashing();
      string authorizationHash = string.Empty;
      string authorizationInfo = string.Empty;
      string decoded = string.Empty;

      authorizationInfo = GetHeader(ctx, "authorization");
      if (authorizationInfo.Length == 0)
      {
        authorizationInfo = CookieRead(ctx, "x-form-authorization");
      }

      try
      {
        decoded = h.Base64Decode(authorizationInfo.Replace("Basic ", ""));
        username = decoded.Split(':')[0];
        password = decoded.Split(':')[1];
      }
      catch (Exception aex)
      {
        username = string.Empty;
        password = string.Empty;
        //ctx.Response.Headers.Add("WWW-Authenticate", "Basic");
        //throw new HttpException(401, HttpStatusCode.Unauthorized.ToString());
      }

    }

  }
}
