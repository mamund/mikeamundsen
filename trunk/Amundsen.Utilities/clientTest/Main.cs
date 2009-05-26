// Main.cs created with MonoDevelop
// User: mca at 11:49 PM 3/25/2009
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
using System;

using Amundsen.Utilities;

namespace clientTest
{
  class MainClass
  {
    public static void Main(string[] args)
    {
      HttpClient c = new HttpClient();
      System.IO.MemoryStream ms = new System.IO.MemoryStream();
      string rtn = c.Execute("http://amundsen.com/","get","text/html","",ref ms);
      Console.Out.Write(rtn);
    }
  }
}