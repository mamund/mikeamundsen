
namespace Amundsen.SSDS.Provisioning
{
  /// <summary>
  /// Public Domain 2008 amundsen.com, inc.
  /// @author mike amundsen (mamund@yahoo.com)
  /// @version 1.6 (2008-10-29) - changed to sds endpoint
  /// @version 1.5 (2008-09-08)
  /// @version 1.4 (2008-08-05)
  /// @version 1.3 (2008-07-20)
  /// @version 1.2 (2008-07-13)
  /// @version 1.1 (2008-07-09)
  /// @version 1.0 (2008-07-03)
  /// </summary>
  static class Constants
  {
    static public string SsdsRoot = "data.database.windows.net/v1/"; //"data.beta.mssds.com/v1/";
    static public string SsdsType = "application/x-ssds+xml";
    static public string QueryAll = "?q=";
    static public string SitkaNS = "http://schemas.microsoft.com/sitka/2008/03/";
    static public string MsftRequestId = "x-msft-request-id";

    static public string ErrorFormat = "<s:Error xmlns:s='{2}'><s:Code>{0}</s:Code><s:Message>{1}</s:Message></s:Error>";
    static public string AuthorityFormat = "<s:Authority xmlns:s='{1}'> <s:Id>{0}</s:Id> </s:Authority>";
    static public string ContainerFormat = "<s:Container xmlns:s='{1}'> <s:Id>{0}</s:Id> </s:Container>";

  }
}