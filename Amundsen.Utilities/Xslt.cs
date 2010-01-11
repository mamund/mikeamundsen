using System;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.IO;
using System.Web;

namespace Amundsen.Utilities
{
  /// <summary>
  /// Public Domain 2008-2010 amundsen.com, inc.
  /// @author mike amundsen (mamund@yahoo.com)
  /// @version 1.0 (2008-07-03)
  /// </summary>

  // Xslt helper class
  // - handles local XML/XSLT documents
  // - caches compiled XSLT and XML documents
  // - supports MemoryStream as a return
  // - uses XPathNavigator
  public class Xslt
  {
    // accept two file paths, return string
    public string Transform(string XmlFile, string XslFile)
    {
      XmlDocument XmlDoc = this.GetXmlDocument(XmlFile);
      return Transform(XmlDoc, XslFile, null);
    }
    public string Transform(string XmlFile, string XslFile, XsltArgumentList XsltArgs)
    {
      XmlDocument XmlDoc = this.GetXmlDocument(XmlFile);
      return Transform(XmlDoc, XslFile, XsltArgs);
    }

    // accept XmlDocument and file path, return string
    public string Transform(XmlDocument XmlDoc, string XslFile)
    {
      return Transform(XmlDoc, XslFile, null);
    }
    public string Transform(XmlDocument XmlDoc, string XslFile, XsltArgumentList XsltArgs)
    {
      string rtn = string.Empty;

      MemoryStream ms = new MemoryStream();
      XPathNavigator xpn = XmlDoc.CreateNavigator();
      XslCompiledTransform tr = this.GetTransform(XslFile);
      tr.Transform(xpn, XsltArgs, ms);

      // convert to string
      ms.Position = 0;
      using (StreamReader sr = new StreamReader(ms))
      {
        rtn = sr.ReadToEnd();
        sr.Close();
      }

      return rtn;
    }

    // accept two XmlDocuments, return string (no caching)
    public string Transform(XmlDocument XmlDoc, XmlDocument XslDoc)
    {
      return Transform(XmlDoc, XslDoc, null);
    }
    public string Transform(XmlDocument XmlDoc, XmlDocument XslDoc, XsltArgumentList XsltArgs)
    {
      string rtn = string.Empty;

      MemoryStream ms = new MemoryStream();
      XslCompiledTransform tr = this.GetTransform(XslDoc);
      XPathNavigator xpn = XmlDoc.CreateNavigator();
      tr.Transform(xpn, XsltArgs, ms);

      // return as a string
      ms.Position = 0;
      using (System.IO.StreamReader sr = new System.IO.StreamReader(ms))
      {
        rtn = sr.ReadToEnd();
        sr.Close();
      }

      return rtn;
    }

    // accept two file paths, update shared stream
    public void Transform(string XmlFile, string XslFile, MemoryStream ms)
    {
      XmlDocument XmlDoc = this.GetXmlDocument(XmlFile);
      Transform(XmlDoc, XslFile, null, ms);
    }
    public void Transform(string XmlFile, string XslFile, XsltArgumentList XsltArgs, MemoryStream ms)
    {
      XmlDocument XmlDoc = this.GetXmlDocument(XmlFile);
      Transform(XmlDoc, XslFile, XsltArgs, ms);
    }

    // accept XmlDocument, file path, update shared stream
    public void Transform(XmlDocument XmlDoc, string XslFile, XsltArgumentList XsltArgs, MemoryStream ms)
    {
      XslCompiledTransform tr = this.GetTransform(XslFile);
      XPathNavigator xpn = XmlDoc.CreateNavigator();
      tr.Transform(xpn, XsltArgs, ms);
      ms.Position = 0;
    }

    // accept two XmlDocuments, update shared stream (no caching)
    public void Transform(XmlDocument XmlDoc, XmlDocument XslDoc, XsltArgumentList XsltArgs, MemoryStream ms)
    {
      XslCompiledTransform tr = this.GetTransform(XslDoc);
      XPathNavigator xpn = XmlDoc.CreateNavigator();
      tr.Transform(xpn, XsltArgs, ms);
      ms.Position = 0;
    }

    // *** private methods to handle XslCompiledTransform (file version stores in cache)
    private XslCompiledTransform GetTransform(XmlDocument XslDoc)
    {
      XslCompiledTransform tr = new XslCompiledTransform();
      tr.Load(XslDoc);
      return tr;
    }
    private XslCompiledTransform GetTransform(string XslFile)
    {
      XslFile = new FileInfo(XslFile).FullName;
      XslCompiledTransform tr = (XslCompiledTransform)HttpRuntime.Cache.Get(XslFile);
      if (tr == null)
      {
        tr = new XslCompiledTransform();
        tr.Load(XslFile);
        HttpRuntime.Cache.Add(XslFile,
            tr,
            new System.Web.Caching.CacheDependency(XslFile),
            System.Web.Caching.Cache.NoAbsoluteExpiration,
            System.Web.Caching.Cache.NoSlidingExpiration,
            System.Web.Caching.CacheItemPriority.Normal,
            null
        );
      }
      return tr;
    }

    // private method to handle XmlDocument (stores in cache)
    private XmlDocument GetXmlDocument(string XmlFile)
    {
      XmlFile = new FileInfo(XmlFile).FullName;
      XmlDocument XmlDoc = (XmlDocument)HttpRuntime.Cache.Get(XmlFile);
      if (XmlDoc == null)
      {
        XmlDoc = new XmlDocument();
        XmlDoc.Load(XmlFile);
        HttpRuntime.Cache.Add(XmlFile,
          XmlDoc,
            new System.Web.Caching.CacheDependency(XmlFile),
            System.Web.Caching.Cache.NoAbsoluteExpiration,
            System.Web.Caching.Cache.NoSlidingExpiration,
            System.Web.Caching.CacheItemPriority.Normal,
            null
        );
      }
      return XmlDoc;
    }
  }
}