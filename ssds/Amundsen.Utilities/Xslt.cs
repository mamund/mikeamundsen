using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.IO;
using System.Web;

namespace Amundsen.Utilities
{
  public class Xslt
  {
      // get xslt transform for this object, mimetype
      public string GetMimeTypeTransform(string name, string mimetype)
      {
          WebUtility wu = new WebUtility();
          return wu.GetConfigSectionItem("ssdsTransforms", string.Format("{0}:{1}",name,mimetype));
      }

    // handle xsl transform routines
      public string Transform(string sXMLDocument, string sXSLFile)
      {
          XmlDocument xmldoc = new XmlDocument();
          xmldoc.Load(sXMLDocument);
          return Transform(xmldoc, sXSLFile, null);
      }
      public string Transform(string sXMLDocument, string sXSLFile, XsltArgumentList args)
      {
          XmlDocument xmldoc = new XmlDocument();
          xmldoc.Load(sXMLDocument);
          return Transform(xmldoc, sXSLFile, args);
      }
      public string Transform(XmlDocument xmldoc, string sXSLFile)
      {
          return Transform(xmldoc, sXSLFile, null);
      }
      public string Transform(XmlDocument xmldoc, string sXSLFile, XsltArgumentList args)
      {
          XslCompiledTransform tr = (XslCompiledTransform)HttpContext.Current.Cache.Get(sXSLFile);
          if (tr == null)
          {
              tr = new XslCompiledTransform();
              tr.Load(sXSLFile);
              HttpContext.Current.Cache.Add(sXSLFile,
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
      public string Transform(XmlDocument xmldoc, XmlDocument xsldoc)
      {
          return Transform(xmldoc, xsldoc, null);
      }
      public string Transform(XmlDocument xmldoc, XmlDocument xsldoc, XsltArgumentList args)
      {
          XslCompiledTransform tr = new XslCompiledTransform();
          tr.Load(xsldoc);

          System.IO.MemoryStream ms = new System.IO.MemoryStream();
          XPathNavigator xpn = xmldoc.CreateNavigator();
          tr.Transform(xpn, args, ms);

          System.Text.Encoding enc = System.Text.Encoding.UTF8;
          return enc.GetString(ms.ToArray());
      }
  }
}
