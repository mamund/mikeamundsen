/* 
	simple stylesheet switcher

	$Date$
	$Rev$
	$Author$
*/

var ss =
{
	cookieName:'_myStyle',
	cookieLife:365,
	defaultStyle:'default',

	selectStyle:function(title) 
	{
		if(!title || title==='undefined' || title==='')
		{
			title=ss.defaultStyle;
		}
		
		var lnks = document.getElementsByTagName('link');
		for (var i = lnks.length - 1; i >= 0; i--) 
		{
			if (lnks[i].getAttribute('rel').indexOf('style')> -1 && lnks[i].getAttribute('title')) 
			{
				lnks[i].disabled = true;
				if (lnks[i].getAttribute('title') == title) 
				{
					lnks[i].disabled = false;
				}
			}
		}
	},
	
	getCurrentStyle:function()
	{
		var lnks = document.getElementsByTagName('link');
	  var rtn = '';
	  
		for (var i = lnks.length - 1; i >= 0; i--) 
		{
			if (lnks[i].getAttribute('rel').indexOf('style')> -1 && lnks[i].disabled===false) 
			{
				rtn = lnks[i].getAttribute('title');
				break;
			}
		}
		
		return rtn;
	},
	
	saveStyle:function()
	{
		var cstyle = ss.getCurrentStyle();
		if(cstyle!=='undefined' && cstyle!=='')
		{
			ss.createCookie(ss.cookieName,cstyle,ss.cookieLife);
		}
	},
	
	recallStyle:function()
	{
		ss.selectStyle(ss.readCookie(ss.cookieName));
	},
	
	forgetStyle:function()
	{
		ss.eraseCookie(ss.cookieName);
		ss.selectStyle(ss.defaultStyle);
	},
	
  createCookie:function(name,value,days)
  {
  		var dte, expires;
  		
      if (days) 
      {
        dte = new Date();
        dte.setTime(dte.getTime()+(days*24*60*60*1000));
        expires = "; expires="+dte.toGMTString();
      }
      else 
      {
      	expires = "";
      }
      document.cookie = name+"="+value+expires+"; path=/";
  },

  readCookie:function(name)
  {
  		var nmeq,ca,c;
  		
      nmeq = name + "=";
      ca = document.cookie.split(';');
      for(var i=0;i < ca.length;i++) 
      {
        c = ca[i];
        while (c.charAt(0)==' ') 
        {
        	c = c.substring(1,c.length);
        }
        if (c.indexOf(nmeq) === 0) 
        {
        	return c.substring(nmeq.length,c.length);
        }
      }
      return null;
  },

  eraseCookie:function(name)
  {
    ss.createCookie(name,"",-1);
  }
};