/* SSDS Guestbook Client
 * mike amundsen - http://amundsen.com/blog/
 * 2008-07-29 (mca) : initial release
 */

var guestbook = function()
{
  var g_nickname = '';
  var g_password = '';
  var g_authCookie = 'x-form-authorization';

  var g_guests_url = '/guestbook/guests/';
  var g_messages_url = '/guestbook/messages/';
  var g_filter = '';

  var errors = [];
  errors[401] = 'Please login to continue.';
  errors[403] = 'Invalid login. Please try again.';
  errors[404] = 'No record(s) found.';
  errors[409] = 'Update cancelled. Someone else already updated this record.';

  function init()
  {
    toggleView('loading');
    g_filter = getSearchArg(/\?filter=([^&]*)/i);
    toggleUser();
    toggleButtons();
    attachEvents();
    getMessages(g_filter);
  }

  function showAbout()
  {
    toggleView('about');
  }

  function showLogin()
  {
    toggleView('guestLogin');
  }

  function loginSubmit()
  {
    var frm;

    frm = document.getElementById('guestLogin-form');
    g_nickname = frm.nickname.value;
    g_password = MD5(frm.password.value);
    ajax.httpGet(g_guests_url+g_nickname,null,onAjaxComplete,true,'getGuest')

    return false;
  }

  function loginBack()
  {
    toggleView('messages');
  }

  function logoutGuest()
  {
    cookies.erase(g_authCookie);
    location.href = location.pathname;
  }

  function showCreateGuest()
  {
    var elm;

    elm = document.getElementById('guestCreate-form');
    if(elm)
    {
      elm.nickname.value='';
      elm.password.value='';
      elm.email.value='';
    }
    toggleView('guestCreate');
  }

  function guestCreateBack()
  {
    location.href = location.pathname;
  }

  function guestCreateSubmit()
  {
    var elm,nick,pass,email,data;

    elm = document.getElementById('guestCreate-form');
    if(elm)
    {
      nick = elm.nickname.value;
      if(nick==null || nick=='')
      {
        alert('Must enter a nickname.');
        return false;
      }

      pass = elm.password.value;
      if(pass==null || pass=='')
      {
        alert('Must enter a password.');
        return false;
      }

      email = elm.email.value;
      if(email==null || email=='')
      {
        alert('Must enter an email address.');
        return false;
      }

      data = 'nickname='+escape(nick.toLowerCase())+'&password='+escape(MD5(pass))+'&email='+escape(email);
      ajax.httpPost(g_guests_url,null,onAjaxComplete,true,'addGuest','application/x-www-form-urlencoded',data);
    }
    return false;
  }

  function showDelete()
  {
    toggleView('guestDelete');
  }

  function showMessages()
  {
    location.href = location.pathname;
  }

  function getMessages(nick)
  {
    nick = nick || '';
    ajax.httpGet(g_messages_url+nick,null,onAjaxComplete,true,'getMessages');
  }

  function showCreateMessage()
  {
    var elm;

    if(g_nickname==null || g_nickname=='')
    {
      toggleView('guestLogin');
      return false;
    }

    elm = document.getElementById('messageCreate-form');
    if(elm)
    {
      elm.style.display='block';
      messageCounter();
    }
    return false;
  }

  function messageCreateSubmit()
  {
    var elm,msg;

    elm = document.getElementById('messageCreate-form');
    if(elm)
    {
      msg = elm.message.value;
      if(msg==null || msg=='')
      {
        alert('Must enter a message.');
      }
      else
      {
        ajax.httpPost(g_messages_url,null,onAjaxComplete,true,'addMessage','application/x-www-form-urlencoded','message='+escape(msg));
      }
    }
    return false;
  }

  function messageCreateBack()
  {
    var elm;
    elm = document.getElementById('messageCreate-form');
    if(elm)
    {
      elm.message.value = '';
      elm.style.display='none';
    }
    location.href = location.pathname;
  }

  function showGuests()
  {
    ajax.httpGet(g_guests_url,null,onAjaxComplete,true,'getGuests');
  }

  function onAjaxComplete(response,headers,context,status,msg)
  {
    switch(status)
    {
      case 0:     // ie abort code
        return;
        //break;
      case 200:   // OK
      case 201:   // Created
      case 202:   // Accepted
      case 204:   // NoContent
        break;
      case 301:   // redirects
      case 302:
        if(headers.location && headers.location!=='')
        {
          location.href=headers.location;
        }
        return;
        // break;
      default:    // 400 & 500 errors
        if(errors[status]!=null)
        {
          alert(errors[status]);
        }
        else
        {
          alert(status+'\n'+msg);
        }

        if(status==401 || status==403)
        {
          showLogin();
        }
        if(status!=400)
        {
          showMessages();
        }
        return;
        break;
    }

    // process results
    switch(context)
    {
      case 'getMessages':
        processGetMessages(response,headers);
        break;
      case 'addMessage':
        processAddMessage(status,msg);
        break;
      case 'getGuests':
        processGetGuests(response,headers);
        break;
      case 'getGuest':
        processGetGuest(response,headers);
        break;
      case 'addGuest':
        processAddGuest(status,msg);
        break;
      default:
        alert('Unknown context\n'+context);
    }
  }

  function processGetMessages(response,headers)
  {
    var list,xml,a,sp,dd,dt,i,id,created,msg,nick;

    list = document.getElementById('recent-messages');
    list.innerHTML = '';

    xml = response.selectNodes('//message');
    for(i=0;i<xml.length;i++)
    {
      id = xml[i].selectSingleNode('s:Id/text()').nodeValue;
      created = xml[i].selectSingleNode('date-created/text()').nodeValue;
      nick = xml[i].selectSingleNode('nickname/text()').nodeValue;
      msg = xml[i].selectSingleNode('body/text()').nodeValue;

      // fixup date display
      a = document.createElement('a');
      a.className='guest';
      a.href = location.pathname+'?filter='+encodeURIComponent(nick);
      a.title='View messages';
      a.appendChild(document.createTextNode(nick));

      sp = document.createElement('span');
      sp.className='date';
      sp.appendChild(document.createTextNode('('+modifyDate(created)+')'));

      dt = document.createElement('dt');
      dt.appendChild(a);
      dt.appendChild(sp);

      dd = document.createElement('dd');
      dd.appendChild(document.createTextNode(msg));

      list.appendChild(dt);
      list.appendChild(dd);
    }

    toggleView('messages');
  }

  function processAddMessage(status,msg)
  {
    if(status<400)
    {
      location.href = location.pathname;
    }
  }

  function processGetGuests(response,headers)
  {
    var list,xml,a,sp,li,i,id,created;

    list = document.getElementById('recent-guests');
    list.innerHTML = '';

    xml = response.selectNodes('//guest');
    for(i=0;i<xml.length;i++)
    {
      id = xml[i].selectSingleNode('s:Id/text()').nodeValue;
      created = xml[i].selectSingleNode('date-created/text()').nodeValue;

      a = document.createElement('a');
      a.className='guest';
      a.href = location.pathname+'?filter='+encodeURIComponent(id);
      a.title='View messages';
      a.appendChild(document.createTextNode(id));

      sp = document.createElement('span');
      sp.className='date';
      sp.appendChild(document.createTextNode('('+modifyDate(created)+')'));

      li = document.createElement('li');
      li.appendChild(a);
      li.appendChild(sp);

      list.appendChild(li);
    }

    toggleView('guests');
  }

  function processGetGuest(response,headers)
  {
    var nick,pass;

    nick = response.selectSingleNode('//s:Id/text()').nodeValue;
    pass = response.selectSingleNode('//password/text()').nodeValue;

    if(nick.toLowerCase()==g_nickname.toLowerCase() && pass==g_password)
    {
      cookies.create(g_authCookie,Base64.encode(nick+':'+pass));
      location.href = location.href;
    }
    else
    {
      toggleButtons();
      alert('Login failed.');
    }
  }

  function processAddGuest(status,msg)
  {
    if(status<400)
    {
      showLogin();
    }
  }
  function attachEvents()
  {
    var elm;

    document.getElementById('commands-guestLogin').onclick = showLogin;
    document.getElementById('commands-guestCreate').onclick = showCreateGuest;
    document.getElementById('commands-guestDelete').onclick = showDelete;
    document.getElementById('commands-messages').onclick = showMessages;
    document.getElementById('commands-guests').onclick = showGuests;
    document.getElementById('commands-guestLogout').onclick = logoutGuest;
    document.getElementById('commands-about').onclick = showAbout;

    document.getElementById('guestLogin-back').onclick = loginBack;
    document.getElementById('guestLogin-form').onsubmit = loginSubmit;

    document.getElementById('messageCreate-link').onclick = showCreateMessage;
    document.getElementById('messageCreate-back').onclick = messageCreateBack;
    document.getElementById('messageCreate-form').onsubmit = messageCreateSubmit

    document.getElementById('guestCreate-back').onclick = guestCreateBack;
    document.getElementById('guestCreate-form').onsubmit = guestCreateSubmit;

    document.getElementById('textMessage').onkeydown = messageCounter;
    document.getElementById('textMessage').onkeyup = messageCounter;
  }


  function messageCounter()
  {
    countChars(document.getElementById('textMessage'),document.getElementById('textCount'),140);
  }
  function countChars(inputElm, counterElm, maxChars)
  {
    if (inputElm.value.length > maxChars)
    {
      // if too long...trim it!
      inputElm.value = inputElm.value.substring(0, maxChars);
    }
    else
    {
      // otherwise, update 'characters left' counter
      counterElm.innerHTML = maxChars - inputElm.value.length;
    }
  }

  function modifyDate(dt)
  {
      var pd = Date.parse(dt).addMinutes(-1*(Date.parse(dt).getTimezoneOffset())).toString('yyyy-MM-ddTHH:mmZ');
      return (prettyDate(pd)!=null?prettyDate(pd):Date.parse(dt).toString('MMM dd yyyy @ hh:mmtt'));
  }

  function toggleUser()
  {
    var auth,elm;

    auth = cookies.read(g_authCookie);
    if(auth!=null)
    {
      g_nickname = Base64.decode(auth).split(':')[0];
      g_password = Base64.decode(auth).split(':')[1];

      elm = document.getElementsByClassName('nickname');
      for(i=0;i<elm.length;i++)
      {
        elm[0].innerHTML = 'Welcome, '+g_nickname;
      }
    }
    else
    {
      g_nickname='';
      g_password='';
      elm = document.getElementsByClassName('nickname');
      for(i=0;i<elm.length;i++)
      {
        elm[0].innerHTML = 'Not logged in.';
      }
    }
  }

  function toggleButtons()
  {
    if(g_nickname!='')
    {
      document.getElementById('commands-guestLogin').disabled = true;
      document.getElementById('commands-guestCreate').disabled = true;
      document.getElementById('commands-guestDelete').disabled = false;
      document.getElementById('commands-messages').disabled = false;
      document.getElementById('commands-guests').disabled = false;
      document.getElementById('commands-guestLogout').disabled = false;
    }
    else
    {
      document.getElementById('commands-guestLogin').disabled = false;
      document.getElementById('commands-guestCreate').disabled = false;
      document.getElementById('commands-guestDelete').disabled = true;
      document.getElementById('commands-messages').disabled = false;
      document.getElementById('commands-guests').disabled = false;
      document.getElementById('commands-guestLogout').disabled = true;
    }
  }

  function toggleView(view)
  {
    var vw,vs;

    vw = view || 'commands';
    vs = {messages:'block',guests:'none',guestLogin:'none',guestCreate:'none',guestDelete:'none',loading:'none',about:'none'};

    switch(vw)
    {
      case 'messages':
        vs = {messages:'block',guests:'none',guestLogin:'none',guestCreate:'none',guestDelete:'none',loading:'none',about:'none'};
        break;
      case 'guests':
        vs = {messages:'none',guests:'block',guestLogin:'none',guestCreate:'none',guestDelete:'none',loading:'none',about:'none'};
        break;
      case 'guestLogin':
        vs = {messages:'none',guests:'none',guestLogin:'block',guestCreate:'none',guestDelete:'none',loading:'none',about:'none'};
        break;
      case 'guestCreate':
        vs = {messages:'none',guests:'none',guestLogin:'none',guestCreate:'block',guestDelete:'none',loading:'none',about:'none'};
        break;
      case 'guestDelete':
        vs = {messages:'none',guests:'none',guestLogin:'none',guestCreate:'none',guestDelete:'block',loading:'none',about:'none'};
        break;
      case 'about':
        vs = {messages:'none',guests:'none',guestLogin:'none',guestCreate:'none',guestDelete:'none',loading:'none',about:'block'};
        break;
      case 'loading':
        vs = {messages:'none',guests:'none',guestLogin:'none',guestCreate:'none',guestDelete:'none',loading:'block',about:'none'};
        break;
      default:
        vs = {messages:'block',guests:'none',guestLogin:'none',guestCreate:'none',guestDelete:'none',loading:'none',about:'none'};
        break;
    }

    document.getElementById('messages').style.display = vs.messages;
    document.getElementById('guests').style.display = vs.guests;
    document.getElementById('guest-login').style.display = vs.guestLogin;
    document.getElementById('guest-create').style.display = vs.guestCreate;
    document.getElementById('guest-delete').style.display = vs.guestDelete;
    document.getElementById('about').style.display = vs.about;
    document.getElementById('loading').style.display = vs.loading;

    return;
  }

  function getSearchArg(rex)
  {
    var srch,id,match;

    srch = location.search;
    match = rex.exec(srch);
    if (match !== null)
    {
      id=match[1];
    }
    else
    {
      id='';
    }
    return id;
  }

  // publish members & return ref
  var that = {};
  that.init = init;
  return that;
};

// extend document object, if needed
if(!document.getElementsByClassName)
{
  document.getElementsByClassName = function(className, tag, elm)
  {
    var testClass = new RegExp("(^|\\s)" + className + "(\\s|$)");
    tag = tag || "*";
    elm = elm || document;
    var elements = (tag == "*" && elm.all)? elm.all : elm.getElementsByTagName(tag);
    var returnElements = [];
    var current;
    var length = elements.length;
    for(var i=0; i < length; i++){
      current = elements[i];
      if(testClass.test(current.className)){
        returnElements.push(current);
      }
    }
    return returnElements;
  };
}

// execute on startup
window.onload = function()
{
  var gb = guestbook();
  gb.init();
};
