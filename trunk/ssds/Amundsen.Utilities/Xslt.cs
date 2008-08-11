using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.IO;

namespace Amundsen.Utilities
{
  public class Xslt
  {
    // handle xsl transform routines
    public string Transform(string sXMLDocument, string sXSLFile)
    {
      return Transform(sXMLDocument, sXSLFile, null);
    }
    public string Transform(string sXMLDocument, string sXSLFile, XsltArgumentList args)
    {

      XmlDocument xd = new XmlDocument();
      xd.Load(sXMLDocument);
      XPathNavigator xdNav = xd.CreateNavigator();
      XslCompiledTransform tr = new XslCompiledTransform();
      tr.Load(sXSLFile);
      StringWriter sw = new StringWriter();
      tr.Transform(xdNav, args, sw);
      return sw.ToString();

    }
    public string Transform(XmlDocument xmldoc, XmlDocument xsldoc)
    {
      return Transform(xmldoc, xsldoc, null);
    }
    public string Transform(XmlDocument xmldoc, XmlDocument xsldoc, XsltArgumentList args)
    {

      XPathNavigator xdNav = xmldoc.CreateNavigator();
      XslCompiledTransform tr = new XslCompiledTransform();
      tr.Load(xsldoc);
      StringWriter sw = new StringWriter();
      tr.Transform(xdNav, args, sw);
      return sw.ToString();

    }
  }
}
