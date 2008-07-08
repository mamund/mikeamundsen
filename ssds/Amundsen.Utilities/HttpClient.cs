using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Globalization;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;
using System.Threading;
using System.Text.RegularExpressions;
using System.Xml;

namespace Amundsen.Utilities
{
  /// <summary>
  /// Public Domain 2008 amundsen.com, inc.
  /// @author mike amundsen (mamund@yahoo.com)
  /// @version 1.0 (2008-07-03)
  /// </summary>
  public class HttpClient
  {
    // properties
    private WebHeaderCollection _RequestHeaders = new WebHeaderCollection();
    public WebHeaderCollection RequestHeaders
    {
      get { return _RequestHeaders; }
      set { _RequestHeaders = value; }
    }

    private WebHeaderCollection _ResponseHeaders = new WebHeaderCollection();
    public WebHeaderCollection ResponseHeaders
    {
      get { return _ResponseHeaders; }
      set { _ResponseHeaders = value; }
    }

    private CookieContainer _CookieCollection = new CookieContainer();
    public CookieContainer CookieCollection
    {
      get { return _CookieCollection; }
      set { _CookieCollection = value; }
    }

    private HttpStatusCode _ResponseStatusCode = HttpStatusCode.OK;
    public HttpStatusCode ResponseStatusCode
    {
      get { return _ResponseStatusCode; }
      set { _ResponseStatusCode = value; }
    }

    private string _ResponseDescription = string.Empty;
    public string ResponseDescription
    {
      get { return _ResponseDescription; }
      set { _ResponseDescription = value; }
    }

    private DateTime _ResponseLastModified = System.DateTime.MaxValue;
    public DateTime ResponseLastModified
    {
      get { return _ResponseLastModified; }
      set { _ResponseLastModified = value; }
    }

    private NetworkCredential _Credentials = new NetworkCredential();
    public NetworkCredential Credentials
    {
      get { return _Credentials; }
      set { _Credentials = value; }
    }

    private long _ResponseLength;
    public long ResponseLength
    {
      get { return _ResponseLength; }
      set { _ResponseLength = value; }
    }
    private string _UserAgent = string.Empty;

    public string UserAgent
    {
      get { return _UserAgent; }
      set { _UserAgent = value; }
    }

    private bool _PreAuthenticate = true;

    public bool PreAuthenticate
    {
      get { return _PreAuthenticate; }
      set { _PreAuthenticate = value; }
    }
    private string _HttpVersion = "1.1";

    public string HttpVersion
    {
      get { return _HttpVersion; }
      set { _HttpVersion = value; }
    }
    private bool _FollowRedirects = true;

    public bool FollowRedirects
    {
      get { return _FollowRedirects; }
      set { _FollowRedirects = value; }
    }

    public HttpClient() { }
    public HttpClient(NetworkCredential credentials)
    {
      this.Credentials = credentials;
    }
    public HttpClient(string user, string password)
    {
      this.Credentials = new NetworkCredential(user, password);
    }
    // method that makes the call
    public string Execute(string url)
    {
      return Execute(url, "get", "text/xml", string.Empty);
    }
    public string Execute(string url, string method)
    {
      return Execute(url, method, "text/xml", string.Empty);
    }
    public string Execute(string url, string method, string contentType)
    {
      return Execute(url, method, contentType, string.Empty);
    }
    public string Execute(string url, string method, string contentType, string body)
    {
      HttpWebRequest req = null;
      HttpWebResponse resp = null;
      string rtnBody = string.Empty;

      try
      {
        // build request object
        req = (HttpWebRequest)WebRequest.Create(url);
        req.UserAgent = (this.UserAgent.Length != 0 ? this.UserAgent : "amundsen-ssds/1.0");
        req.Method = method.ToUpper(CultureInfo.CurrentCulture);
        req.ContentType = contentType;
        req.Accept = contentType;
        req.ContentLength = body.Length;
        req.PreAuthenticate = this.PreAuthenticate;
        if (this.Credentials.UserName.Length!=0)
        {
          req.Credentials = this.Credentials;
        }

        // set headers
        if (this.RequestHeaders != null)
        {
          for (int i = 0; i < this.RequestHeaders.Count; i++)
          {
            // some headers must be set as properties only
            string key = this.RequestHeaders.GetKey(i);
            string value = this.RequestHeaders[i];
            switch (key.ToLower(CultureInfo.CurrentCulture))
            {
              case "user-agent":
                req.UserAgent = value;
                break;
              case "if-modified-since":
                req.IfModifiedSince = DateTime.Parse(value,CultureInfo.CurrentCulture);
                break;
              case "accept":
                req.Accept = value;
                break;
              default:
                req.Headers.Set(key, value);
                break;
            }
          }
        }

        // set cookies
        if (this.CookieCollection != null)
          req.CookieContainer = this.CookieCollection;
        if (HttpContext.Current != null &&
            HttpContext.Current.Request != null &&
            HttpContext.Current.Request.Headers != null &&
            HttpContext.Current.Request.Headers["cookie"] != null
            )
          req.CookieContainer.SetCookies(new Uri(url), HttpContext.Current.Request.Headers["cookie"]);

        // set body
        if (body != null && body.Trim().Length!=0)
        {
          using (StreamWriter sw = new StreamWriter(req.GetRequestStream()))
          {
            sw.Write(body);
            sw.Close();
          }
        }

        // tweak request
        req.ProtocolVersion = new Version(this.HttpVersion);
        req.AllowAutoRedirect = this.FollowRedirects;

        // now use request obj to populate response obj
        resp = (HttpWebResponse)req.GetResponse();

        // get properties
        this.ResponseLength = resp.ContentLength;
        this.ResponseStatusCode = resp.StatusCode;
        this.ResponseDescription = resp.StatusDescription;
        this.ResponseLastModified = resp.LastModified;

        // get headers
        this.ResponseHeaders = resp.Headers;

        // get cookies
        this.CookieCollection = new CookieContainer();
        foreach (Cookie ck in resp.Cookies)
          this.CookieCollection.Add(ck);

        // get body
        if (resp.ContentLength != 0)
        {
          using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
          {
            rtnBody = sr.ReadToEnd();
            sr.Close();
          }
        }

        // clean up
        if (resp != null)
          resp.Close();

        resp = null;
        req = null;

        // return the results (if any)
        return rtnBody;
      }
      catch (HttpException hex)
      {
        throw new HttpException(hex.GetHttpCode(), hex.Message);
      }
      catch (WebException wex)
      {
        // typical http error
        if (wex.Status == WebExceptionStatus.ProtocolError)
        {
          string msg = string.Empty;
          string code = string.Empty;
          HttpWebResponse wrsp = (HttpWebResponse)wex.Response;

          msg = wrsp.StatusDescription;
          code = ((int)wrsp.StatusCode).ToString();

          // hack to check for SSDS custom error msg (should be in the StatusDescription...)
          try
          {
            XmlNode node = null;
            XmlDocument xml_doc = new XmlDocument();
            xml_doc.Load(wrsp.GetResponseStream());
            XmlNamespaceManager xml_ns = new XmlNamespaceManager(xml_doc.NameTable);
            xml_ns.AddNamespace("s", "http://schemas.microsoft.com/sitka/2008/03/");
            node = xml_doc.SelectSingleNode("//s:Message", xml_ns);
            if (node != null)
            {
              msg = node.InnerText;
            }
            node = xml_doc.SelectSingleNode("//s:Code", xml_ns);
            if (node != null)
            {
              code = node.InnerText;
            }
            msg = msg + "[" + code + "]";
          }
          catch (XmlException xex)
          {
            throw new HttpException(Int32.Parse(code), msg);
          }
          throw new HttpException(Int32.Parse(code), msg);
        }
        else
        {
          throw new HttpException(500, wex.Message);
        }
      }
      catch (Exception ex)
      {
        throw new HttpException(500, ex.Message);
      }
    }
  }
}
