/* cookie routines */

var cookies =
{
  create:function(name,value,days,path)
  {
    var expires,date,p;

    p = path || '/';

    if (days)
    {
      date = new Date();
      date.setTime(date.getTime()+(days*24*60*60*1000));
      expires = "; expires="+date.toGMTString();
    }
    else
    {
      expires = "";
    }
    document.cookie = name+"="+value+expires+"; path="+p;
  },

  read:function(name)
  {
    var c;
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for(var i=0;i < ca.length;i++)
    {
      c = ca[i];
      while (c.charAt(0)==' ')
      {
        c = c.substring(1,c.length);
      }
      if (c.indexOf(nameEQ) === 0)
      {
        return c.substring(nameEQ.length,c.length);
      }
    }
    return null;
  },

  erase:function(name)
  {
    cookies.create(name,"",-1);
  }
};