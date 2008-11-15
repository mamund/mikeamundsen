using System;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Net;
using System.Globalization;
using System.IO;

using Amundsen.Utilities;
using Amundsen.SSDS.Provisioning;

namespace Amundsen.SSDS.PhotoDemo
{
    /// <summary>
    /// Public Domain 2008 amundsen.com, inc.
    /// @author mike amundsen (mamund@yahoo.com)
    /// @version 1.0 (2008-11-11)
    /// </summary>
    class Photos : IHttpHandler
    {
        private WebUtility wu = new WebUtility();
        private CacheService cs = new CacheService();
        private HttpClient client = new HttpClient();
        private HttpContext ctx;
        private Hashing h = new Hashing();

        // set via config file <ssdsSettings>
        int maxAge;
        bool showExpires;
        string ssdsProxy = string.Empty;
        string ssdsUser = string.Empty;
        string ssdsPassword = string.Empty;
        string msft_request = string.Empty;
        string accept_type = string.Empty;

        bool IHttpHandler.IsReusable
        {
            get { return false; }
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            ctx = context;

            HandleConfigSettings();
            client.RequestHeaders.Add("authorization", "Basic " + h.Base64Encode(string.Format("{0}:{1}", ssdsUser, ssdsPassword)));
            wu.SetCompression(ctx);

            try
            {
                switch (ctx.Request.HttpMethod.ToLower())
                {
                    case "get":
                        Get();
                        break;
                    case "head":
                        Get(false);
                        break;
                    default:
                        throw new HttpException((int)HttpStatusCode.MethodNotAllowed, "Method Not Allowed");
                        //break;
                }
            }
            catch (HttpException hex)
            {
                if (hex.GetHttpCode() != 304)
                {
                    ctx.Response.ContentType = "text/xml";
                    ctx.Response.Write(string.Format(CultureInfo.CurrentCulture, Constants.ErrorFormat, hex.GetHttpCode(), hex.Message, Constants.SitkaNS));
                    ctx.Response.Write(" ".PadRight(500));
                }
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

        private void Get()
        {
            Get(false);
        }
        private void Get(bool suppressContent)
        {
            CacheItem item = null;
            string sds_url = string.Empty;
            //string accept_type = string.Empty;
            string request_url = ctx.Request.Url.ToString();
            string ifNoneMatch = wu.GetHeader(ctx, "if-none-match");
            string authority = "mca-photos";
            string container = (ctx.Request["container"] != null ? ctx.Request["container"] : "home");
            string entity = (ctx.Request["entity"] != null ? ctx.Request["entity"] : string.Empty);

            // fix up for default page request
            if (container == "home" && entity == string.Empty)
            {
                container = "original";
            }

            // handle various file types
            switch (entity.ToLower())
            {
                case "":
                    accept_type = "text/html";
                    break;
                case "photos.js":
                    container = "files";
                    accept_type = "text/javascript";
                    break;
                case "photos.css":
                    container = "files";
                    accept_type = "text/css";
                    break;
                default:
                    accept_type = "image/jpeg";
                    break;

            }

            // check local cache first (if allowed)
            if (wu.CheckNoCache(ctx) == true)
            {
                ifNoneMatch = string.Empty;
                cs.RemoveItem(request_url + accept_type);
            }
            else
            {
                item = cs.GetItem(request_url + accept_type);
            }

            // did our copy expire?
            if (item != null && (item.Expires < DateTime.UtcNow))
            {
                cs.RemoveItem(request_url + accept_type);
                item = null;
            }

            // ok, we need to talk to SDS now
            if (item == null)
            {
                sds_url = string.Format(CultureInfo.CurrentCulture, "{0}{1}/{2}/{3}", ssdsProxy, authority, container, entity);
                if (container == "original" && entity == "")
                {
                    item = ProcessListPage(request_url, sds_url);
                }
                else
                {
                    item = ProcessFileRequest(request_url, sds_url);
                }
            }

            // finish processing request
            if (ifNoneMatch == item.ETag)
            {
                ctx.Response.ContentType = (accept_type != Constants.SsdsType ? accept_type : "text/xml");
                throw new HttpException((int)HttpStatusCode.NotModified, HttpStatusCode.NotModified.ToString());
            }

            // compose response to client
            ctx.Response.SuppressContent = suppressContent;
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = (accept_type != Constants.SsdsType ? accept_type : "text/xml");
            ctx.Response.StatusDescription = "OK";
            if (item.BinaryData != null)
            {
                ctx.Response.BinaryWrite(item.BinaryData);
            }
            else
            {
                ctx.Response.Write(item.Payload);
            }

            // add msft_header, if present (for debugging)
            if (msft_request.Length != 0)
            {
                ctx.Response.AppendToLog(string.Format(" [{0}={1}]", Constants.MsftRequestId, msft_request));
                ctx.Response.AddHeader(Constants.MsftRequestId, msft_request);
            }

            // validation caching
            ctx.Response.AddHeader("etag", item.ETag);
            ctx.Response.AppendHeader("Last-Modified", string.Format(CultureInfo.CurrentCulture, "{0:R}", item.LastModified));

            // expiration caching, if config'ed
            if (showExpires)
            {
                ctx.Response.AppendHeader("Expires", string.Format(CultureInfo.CurrentCulture, "{0:R}", item.Expires));
                ctx.Response.AppendHeader("cache-control", string.Format(CultureInfo.CurrentCulture, "max-age={0}, must-revalidate", maxAge));
            }
            else
            {
                ctx.Response.AppendHeader("cache-control", "must-revalidate");
            }

            // ie local cache hack
            if (ctx.Request.UserAgent != null && ctx.Request.UserAgent.IndexOf("IE", StringComparison.CurrentCultureIgnoreCase) != -1)
            {
                ctx.Response.AppendHeader("cache-control", "no-cache,post-check=1,pre-check=2");
            }

        }

        // handle request for list page (html)
        private CacheItem ProcessListPage(string request_url, string sds_url)
        {
            CacheItem item = null;
            string rtn = string.Empty;
            string transform = "photo-list.xsl";
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            client.UseBinaryStream = true;
            rtn = client.Execute(sds_url, "get", accept_type, string.Empty, ref ms);
            if (ms != null)
            {
                accept_type = client.ResponseHeaders["content-type"];
            }

            if (rtn.Length == 0 && ms != null)
            {
                System.Text.Encoding enc = System.Text.Encoding.UTF8;
                rtn = enc.GetString(ms.ToArray());
            }
            msft_request = (client.ResponseHeaders[Constants.MsftRequestId] != null ? client.ResponseHeaders[Constants.MsftRequestId] : string.Empty);

            // convert return into xml, load transform and execute
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(rtn);
            rtn = Transform(xmldoc, ctx.Server.MapPath(transform), null);
            accept_type = "text/html";

            // place into local cache
            item = cs.PutItem(
              new CacheItem(
                request_url + accept_type,
                rtn,
                string.Format(CultureInfo.CurrentCulture, "\"{0}\"", cs.MD5BinHex(rtn)),
                DateTime.UtcNow.AddSeconds(maxAge),
                showExpires,
                null
              )
            );

            return item;
        }

        // handle request for binary file (image, js, css)
        private CacheItem ProcessFileRequest(string request_url, string sds_url)
        {
            CacheItem item = null;
            string rtn = string.Empty;
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            client.UseBinaryStream = true;
            rtn = client.Execute(sds_url, "get", accept_type, string.Empty, ref ms);
            if (ms != null)
            {
                accept_type = client.ResponseHeaders["content-type"];
            }

            if (rtn.Length == 0 && ms != null)
            {
                System.Text.Encoding enc = System.Text.Encoding.UTF8;
                rtn = enc.GetString(ms.ToArray());
            }
            msft_request = (client.ResponseHeaders[Constants.MsftRequestId] != null ? client.ResponseHeaders[Constants.MsftRequestId] : string.Empty);
            
            // place into local cache
            item = cs.PutItem(
              new CacheItem(
                request_url + accept_type,
                rtn,
                string.Format(CultureInfo.CurrentCulture, "\"{0}\"", cs.MD5BinHex(rtn)),
                DateTime.UtcNow.AddSeconds(maxAge),
                showExpires,
                (ms != null && ms.Length != 0 ? ms.ToArray() : null)
              )
            );

            return item;
        }

        // handle xsl transform routines
        private string Transform(XmlDocument xmldoc, string sXSLFile, XsltArgumentList args)
        {
            XslCompiledTransform tr = (XslCompiledTransform)ctx.Cache.Get(sXSLFile);
            if (tr == null)
            {
                tr = new XslCompiledTransform();
                tr.Load(sXSLFile);
                ctx.Cache.Add(sXSLFile,
                    tr,
                    new System.Web.Caching.CacheDependency(sXSLFile),
                    System.Web.Caching.Cache.NoAbsoluteExpiration,
                    System.Web.Caching.Cache.NoSlidingExpiration,
                    System.Web.Caching.CacheItemPriority.Normal,
                    null
                );
            }

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            XPathNavigator xpn = xmldoc.CreateNavigator();
            tr.Transform(xpn, args, ms);

            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            return enc.GetString(ms.ToArray());
        }

        private void HandleConfigSettings()
        {
            ssdsProxy = wu.GetConfigSectionItem("ssdsSettings", "ssdsProxy");
            ssdsUser = wu.GetConfigSectionItem("ssdsSettings", "ssdsUser");
            ssdsPassword = wu.GetConfigSectionItem("ssdsSettings", "ssdsPassword");
            maxAge = Int32.Parse(wu.GetConfigSectionItem("ssdsSettings", "maxAge"), CultureInfo.CurrentCulture);
            showExpires = (wu.GetConfigSectionItem("ssdsSettings", "showExpires") == "true" ? true : false);
        }

    }

}
