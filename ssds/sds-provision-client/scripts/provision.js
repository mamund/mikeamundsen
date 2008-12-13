/* SDS Provisioning Client
 * mike amundsen - http://amundsen.com/blog/
 * 2008-09-08 (mca) : updated UI for better login, added support for viewing blobs, added support for listing authorities
 * 2008-07-21 (mca) : minor tweaks to the UI
 * 2008-07-16 (mca) : updated to use new ssds-proxy URIs
 * 2008-07-14 (mca) : refactored javascript
 * 2008-07-10 (mca) : added support for caching, improved login pattern
 * 2008-07-08 (mca) : added support for entity queries
 * 2008-07-04 (mca) : fixed IE entity display bug
 * 2008-06-23 (mca) : initial release
 * 2008-11-04 (mca) : updated to point to new SDS proxy
 * 2008-11-20 (mca) : updated to allow viewing full xml for lists and items
 * 2008-11-26 (mca) : added display of authority and container metrics
 */

// define provision object
var provision = function()
{
  // private members
  var userName = '';
  var etag = '';
  var noCache = '';
  var ssdsContentType = 'application/x-ssds+xml';
  var authCookie = 'x-form-authorization';
  var alertUrl = 'alert-text.html';
  var viewObjectText = 'Raw Response'

  var authority = {};
  authority.id = '';
  authority.addUrl = '/sds-proxy/';
  authority.getUrl = '/sds-proxy/';
  authority.addXml = '<authority>{@authority}</authority>';
  authority.metricsUrl = '/sds-proxy/{@authority}';

  var container = {};
  container.id = '';
  container.listUrl = '/sds-proxy/{@authority}/';
  container.deleteUrl = '/sds-proxy/{@authority}/{@container}';
  container.addUrl = '/sds-proxy/{@authority}/';
  container.addXml = '<container>{@container}</container>';
  container.metricsUrl = '/sds-proxy/{@authority}/{@container}';

  var entity = {};
  entity.id = '';
  entity.query = '';
  entity.queryUrl = '/sds-proxy/{@authority}/{@container}/?{@query}';
  entity.listUrl = '/sds-proxy/{@authority}/{@container}/';
  entity.itemUrl = '/sds-proxy/{@authority}/{@container}/{@entity}';
  entity.deleteUrl = '/sds-proxy/{@authority}/{@container}/{@entity}';
  entity.addUrl = '/sds-proxy/{@authority}/{@container}/';
  entity.addXml = '<{@kind} xmlns:s="http://schemas.microsoft.com/sitka/2008/03/" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:x="http://www.w3.org/2001/XMLSchema">\n<s:Id>$id$</s:Id>\n<name xsi:type="x:string"></name>\n</{@kind}>';

  var serverErrors = [];
  serverErrors[401] = 'Access denied.\nPlease login.';
  serverErrors[403] = 'Access denied.\nInvalid username or password.';
  serverErrors[409] = 'Add cancelled.\nAn item with that ID already exists.';
  serverErrors[412] = 'Update cancelled.\nSomeone else has modified this item.';

  function init()
  {
    // get state from url
    authority.id = getSearchArg(/\?aid=([^&]*)/i);
    container.id = getSearchArg(/\&cid=([^&]*)/i);
    entity.id = getSearchArg(/\&eid=([^&]*)/i);
    entity.query = getSearchArg(/\&qry=([^&]*)/i);
    noCache = getSearchArg(/\&nc=([^&]*)/i);

    attachEvents();
    checkAlert();
    toggleUser();
    updateUI();
    selectView();
  }

  function selectView()
  {
    if(authority.id!=='' && container.id!=='' && entity.id!=='')
    {
      showEntityItem();
      return;
    }
    if(authority.id!=='' && container.id!=='' && entity.query!=='')
    {
      entityQueryExecute();
      return;
    }
    if(authority.id!=='' && container.id!=='')
    {
      showEntityList();
      return;
    }
    if(authority.id!=='')
    {
      showContainerList();
      return;
    }
    else
    {
      // get list of authorities for the logged-in user
      ajax.httpGet(authority.getUrl,null,onAjaxComplete,true,'getAuthorityList',{'cache-control':'no-cache'})
      return;
    }
  }

  function checkAlert()
  {
    ajax.httpGet(alertUrl,null,onAjaxComplete,false,'checkAlert',{'cache-control':'no-cache'});
  }

  function showHome()
  {
    location.href = location.pathname;
  }

  function showWhatsNew()
  {
    var elm
    elm = document.getElementsByClassName('whats-new')[0];
    if(elm)
    {
      if(elm.style.display=='none')
      {
        elm.style.display='block';
        this.title = "Click to hide"
      }
      else
      {
        elm.style.display='none';
        this.title = "Click to view"
      }
    }
    return false;
  }
  
  function showAuthorityMetrics()
  {
    var elm
    elm = document.getElementsByClassName('show-authority-metrics')[0];
    if(elm)
    {
      if(elm.style.display=='none')
      {
        elm.style.display='block';
        this.title = "Click to hide"
      }
      else
      {
        elm.style.display='none';
        this.title = "Click to view"
      }
    }
    return false;
  }

  function showContainerMetrics()
  {
    var elm
    elm = document.getElementsByClassName('show-container-metrics')[0];
    if(elm)
    {
      if(elm.style.display=='none')
      {
        elm.style.display='block';
        this.title = "Click to hide"
      }
      else
      {
        elm.style.display='none';
        this.title = "Click to view"
      }
    }
    return false;
  }
  
  function showAuthentication()
  {
    toggleView('authenticateUser');
    document.getElementById('auth-name').focus();
  }

  function authenticateUserSubmit()
  {
    var user,pass;

    user = document.getElementById('auth-name').value;
    pass = document.getElementById('auth-pass').value;
    if(user!=='' && pass!=='')
    {
      cookies.create(authCookie,Base64.encode(user+':'+pass));
      location.href = location.href;
    }
    return false;
  }

  function authenticateUserBack()
  {
    location.href = location.pathname;
  }

  function logoutUser()
  {
    cookies.erase(authCookie);
    location.href = location.pathname;
  }

  function showAuthorityView()
  {
    var elm;

    if(cookies.read(authCookie)===null||cookies.read(authCookie)=='')
    {
      alert('You must login first.');
    }
    else
    {
      elm = document.getElementById('authority-name');
      toggleView('authorityView');
      elm.value='';
      elm.focus();
    }
    return false;
  }

  function authorityViewSubmit()
  {
    var elm,name,data,url;

    elm = document.getElementById('authority-name');
    if(elm && elm.value!=='')
    {
      name = elm.value.replace(/\s/g,'-').toLowerCase();
      if (name.match(/^[0-9a-z]+$/i))
      {
        url = authority.addUrl;
        data = authority.addXml.replace('{@authority}',name);
        ajax.httpPost(url,null,onAjaxComplete,true,'addAuthority',ssdsContentType,data);
      }
      else
      {
        alert('Invalid authority name.\nUse a-z and 0-9');
      }
    }
    return false;
  }

  function authorityViewBack()
  {
    location.href = location.pathname;
  }

  function showContainerFilter()
  {
    var elm;

    if(cookies.read(authCookie)===null||cookies.read(authCookie)=='')
    {
      alert('You must login first.');
    }
    else
    {
      elm = document.getElementById('containerFilter-authority');
      if(authority.id!=='')
      {
        elm.value = authority.id;
      }

      toggleView('containerFilter');
      elm.focus();
    }
    return false;
  }

  function containerFilterSubmit()
  {
    var elm;

    elm = document.getElementById('containerFilter-authority');
    if(elm && elm.value!=='')
    {
      location.href = location.pathname+'?aid='+elm.value;
    }
    return false;
  }

  function showContainerList()
  {
    var url;

    toggleView('loading');
    url = container.listUrl.replace('{@authority}',authority.id);
    if(noCache=='1')
    {
      ajax.httpGet(url,null,onAjaxComplete,true,'getContainerList',{'cache-control':'no-cache'});
    }
    else
    {
      ajax.httpGet(url,null,onAjaxComplete,true,'getContainerList');
    }
  }

  function containerFilterBack()
  {
    location.href = location.pathname;
  }

  function containerListRefresh()
  {
    location.href = location.pathname+'?aid='+authority.id+'&nc=1';
  }

  function containerListAdd()
  {
    var elm;

    elm = document.getElementById('container-name');
    if(elm)
    {
      elm.value='';
    }

    toggleView('containerAdd');
    elm.focus();
  }

  function containerListBack()
  {
    location.href = location.pathname;
  }

  function containerAddSubmit()
  {
    var elm,name,url,data;

    elm = document.getElementById('container-name');
    if(elm && elm.value!=='')
    {
      name = elm.value.replace(/\s/g,'-').toLowerCase();
      if (name.match(/^[0-9a-z\-]+$/i))
      {
        url = container.addUrl.replace('{@authority}',authority.id);
        data = container.addXml.replace('{@container}',name);
        ajax.httpPost(url,null,onAjaxComplete,true,'addContainer',ssdsContentType,data);
      }
      else
      {
        alert('Invalid container name.\nUse a-z, 0-9, and -');
      }
    }
    return false;
  }

  function containerAddBack()
  {
    location.href = location.href;
  }

  function showEntityList()
  {
    var url;

    toggleView('loading');
    url = entity.listUrl.replace('{@authority}',authority.id).replace('{@container}',container.id);
    if(noCache=='1')
    {
      ajax.httpGet(url,null,onAjaxComplete,true,'getEntityList',{'accept':ssdsContentType,'cache-control':'no-cache'});
    }
    else
    {
      ajax.httpGet(url,null,onAjaxComplete,true,'getEntityList',{'accept':ssdsContentType});
    }
  }

  function entityListRefresh()
  {
    location.href = location.pathname+'?aid='+authority.id+'&cid='+container.id+'&nc=1';
  }

  function entityListQuery()
  {
    var elm;
    elm = document.getElementById('entityQuery-text');
    if(elm)
    {
      if(entity.query!=='')
      {
        elm.value = unescape(entity.query);
      }
      else
      {
        elm.value='from e in entities where e.Id>"1" select e';
      }
    }
    toggleView('entityQuery');
    elm.focus();
  }

  function entityListClear()
  {
    location.href = location.pathname+'?aid='+authority.id+'&cid='+container.id;
  }

  function entityListAdd()
  {
    var elm;
    elm = document.getElementById('entityAdd-xml');
    if(elm)
    {
      elm.value=entity.addXml;
    }
    toggleView('entityAdd');
    elm.focus();
  }

  function entityListDelete()
  {
    var url;

    if(confirm('Ready to Delete the '+container.id+' Container?')===true)
    {
      url = container.deleteUrl.replace('{@authority}',authority.id).replace('{@container}',container.id);
      ajax.httpDelete(url,null,onAjaxComplete,true,'deleteContainer',{'content-type':ssdsContentType});
    }
  }

  function entityListBack()
  {
    location.href = location.pathname+'?aid='+authority.id;
  }

  function showEntityItem()
  {
    var url;

    toggleView('loading');
    url = entity.itemUrl.replace('{@authority}',authority.id).replace('{@container}',container.id).replace('{@entity}',encodeURIComponent(entity.id));
    ajax.httpGet(url,null,onAjaxComplete,true,'getEntityItem',{'accept':ssdsContentType});
  }

  function entityItemEdit()
  {
    var url;
    url =   entity.itemUrl.replace('{@authority}',authority.id).replace('{@container}',container.id).replace('{@entity}',encodeURIComponent(entity.id));
    ajax.httpGet(url,null,onAjaxComplete,false,'getEntityEdit',{'accept':ssdsContentType,'cache-control':'no-cache'});
  }

  function entityItemDelete()
  {
    var url;

    if(confirm('Ready to Delete '+container.id+' entity ['+entity.id+']?')===true)
    {
      url = entity.deleteUrl.replace('{@authority}',authority.id).replace('{@container}',container.id).replace('{@entity}',encodeURIComponent(entity.id));
      ajax.httpDelete(url,null,onAjaxComplete,true,'deleteEntity',{'accept':ssdsContentType});
    }
  }

  function entityItemBack()
  {
    location.href = location.pathname+'?aid='+authority.id+'&cid='+container.id;
  }

  function entityAddSubmit()
  {
    var elm,url,data;

    elm = document.getElementById('entityAdd-xml');
    if(elm)
    {
      data = elm.value;
      url = entity.addUrl.replace('{@authority}',authority.id).replace('{@container}',container.id);
      ajax.httpPost(url,null,onAjaxComplete,true,'addEntity',ssdsContentType,data);
    }
    return false;
  }

  function entityAddBack()
  {
    location.href = location.pathname+'?aid='+authority.id+'&cid='+container.id;
  }

  function entityEditUpdate()
  {
    var elm,url,data;

    elm = document.getElementById('entityEdit-xml');
    if(elm)
    {
      data = elm.value;
      url = entity.itemUrl.replace('{@authority}',authority.id).replace('{@container}',container.id).replace('{@entity}',encodeURIComponent(entity.id));
      ajax.httpPut(url,null,onAjaxComplete,true,'editEntity',ssdsContentType,data,{'if-match':etag});
    }
    return false;
  }

  function entityEditBack()
  {
    location.href = location.pathname+'?aid='+authority.id+'&cid='+container.id+'&eid='+encodeURIComponent(entity.id);
  }

  function entityQuerySubmit()
  {
    var elm,url,data;

    elm = document.getElementById('entityQuery-text');
    if(elm)
    {
      data = encodeURIComponent(elm.value);
      location.href = location.pathname+'?aid='+authority.id+'&cid='+container.id+'&qry='+data;
    }
    return false;
  }

  function entityQueryBack()
  {
    location.href = location.pathname+'?aid='+authority.id+'&cid='+container.id;
  }

  function entityQueryExecute()
  {
    var url = entity.queryUrl.replace('{@authority}',authority.id).replace('{@container}',container.id).replace('{@query}',encodeURIComponent(entity.query));
    ajax.httpGet(url,null,onAjaxComplete,true,'queryEntity',{'accept-type':ssdsContentType});
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
        if(serverErrors[status])
        {
          alert(serverErrors[status]);
        }
        else
        {
          alert(status+'\n'+msg);
        }
        break;
    }

    // get credentials
    if(status==401 || status==403)
    {
      showAuthentication();
      return;
    }

    // get etag for the current entity document
    etag='';
    try
    {
      if(headers.etag!==null && (context==='getEntityItem' || context==='getEntityEdit'))
      {
        etag = headers.etag;
      }
    }
    catch(ex) {}

    // process results
    switch(context)
    {
      case 'checkAlert':
        processCheckAlert(response,headers);
        break;
      case 'loginUser':
        processLoginUser(response,headers);
        break;
      case 'getContainerList':
        processGetContainerList(response,headers);
        break;
      case "queryEntity":
      case 'getEntityList':
        processGetEntityList(response,headers);
        break;
      case 'getEntityItem':
        processGetEntityItem(response,headers);
        break;
      case 'getEntityEdit':
        processGetEntityEdit(response,headers);
        break;
      case 'addEntity':
      case 'deleteEntity':
        processUpdateEntity(status);
        break;
      case 'editEntity':
        processEditEntity(status);
        break;
      case 'addContainer':
      case 'deleteContainer':
        processUpdateContainer(status);
        break;
      case 'addAuthority':
        processAddAuthority(status);
        break;
      case 'getAuthorityList':
        processGetAuthorityList(response,headers);
        break;
      case 'authorityMetrics':
        processAuthorityMetrics(response,headers);
        break;
      case 'containerMetrics':
        processContainerMetrics(response,headers);
        break;
      default:
        alert('Unknown context\n'+context);
    }
  }

  function processGetAuthorityList(response,headers)
  {
    var elm,list,xml,opt,i,id;

    xml = response.selectNodes('//s:Authority');

    list = document.getElementById('containerFilter-authority');
    list.options[0] = new Option('--Select--','');

    for(i=0;i<xml.length;i++)
    {
      id = xml[i].selectSingleNode('s:Id/text()').nodeValue;
      list.options[i] =new Option(id,id);
    }

    showAuthorityXml();

    toggleView('containerFilter');
  }

  function showAuthorityXml()
  {
    var elm,a,url;
    
    elm = document.getElementById('authority-xml');
    if(elm)
    {
        url = '/sds-proxy/';
        a = document.createElement('a');
        a.href = url;
        a.onclick = function(){var w = window.open(url,'xml_window','status=0,toolbar=0,location=0,menubar=0,directories=0,resizable=1,scrollbars=1,height=600,width=600');w.focus();return false;}
        a.target = "xml_window";
        a.className = 'xml-window';
        a.appendChild(document.createTextNode(viewObjectText));
        elm.appendChild(a);
    }
  }
  
  function processCheckAlert(response,headers)
  {
    var elm;
    elm = document.getElementById('alert');
    if(elm)
    {
      if(response && response.length!=0)
      {
        elm.innerHTML = response;
        elm.style.display='block';
      }
      else
      {
        elm.innerHTML = '';
        elm.style.display='none'
      }
    }
  }

  function processGetContainerList(response,headers)
  {
    var elm,list,xml,li,a,i,id;

    list = document.getElementById('container-items');
    xml = response.selectNodes('//s:Container');
    for(i=0;i<xml.length;i++)
    {
      id = xml[i].selectSingleNode('s:Id/text()').nodeValue;

      a = document.createElement('a');
      a.href = location.pathname+'?aid='+authority.id+'&cid='+escape(id);
      a.title='View Entities';
      a.appendChild(document.createTextNode(id));

      li = document.createElement('li');
      li.appendChild(a);

      list.appendChild(li);
    }
  
    showContainerXml();  
    getAuthorityMetrics();

    toggleView('containerList');
  }

  function getAuthorityMetrics()
  {
    var elm,url;
    
    elm = document.getElementById('authority-xml');
    if(elm)
    {
        url = authority.metricsUrl.replace('{@authority}',authority.id);
        ajax.httpGet(url,null,onAjaxComplete,true,'authorityMetrics',{'accept-type':ssdsContentType});
    }
  }
  
  function showContainerXml()
  {
    var elm,a,url;
    elm = document.getElementById('container-xml');
    if(elm)
    {
        url = '/sds-proxy/'+authority.id+'/';
        a = document.createElement('a');
        a.href = url;
        a.onclick = function(){var w = window.open(url,'xml_window','status=0,toolbar=0,location=0,menubar=0,directories=0,resizable=1,scrollbars=1,height=600,width=600');w.focus();return false;}
        a.target = "xml_window";
        a.className = 'xml-window';
        a.appendChild(document.createTextNode(viewObjectText));
        elm.appendChild(a);
    }
  }
  
  function processAuthorityMetrics(response,headers)
  {
    var xml,dl,dt,dd,i,name,value;
    
    dl = document.getElementById('authority-metrics');
    xml = response.selectNodes('//s:Authority/*');
    for(i=0;i<xml.length;i++)
    {
      name = xml[i].tagName;
      try{value = xml[i].firstChild.nodeValue.replace(/^\s+|\s+$/g, '');}catch(ex){value='';}
      if(value===''){value='.';}

      dt = document.createElement('dt');
      dt.appendChild(document.createTextNode(name));
      dd = document.createElement('dd');
      dd.appendChild(document.createTextNode(value));

      dl.appendChild(dt);
      dl.appendChild(dd);
    }
  }
  
  function processGetEntityList(response,headers)
  {
    var list,xml,a,li,i,id,kind,elm,qtext;

    list = document.getElementById('entityList-items');
    xml = response.selectNodes('//s:EntitySet/*');
    for(i=0;i<xml.length;i++)
    {
      aview = null;
      kind = xml[i].tagName;
      id = xml[i].selectSingleNode('s:Id/text()').nodeValue;

      a = document.createElement('a');
      a.href = location.pathname+'?aid='+authority.id+'&cid='+container.id+'&eid='+encodeURIComponent(id);
      a.title='Entity Detail';
      a.appendChild(document.createTextNode(kind+' ['+id+']'));

      li = document.createElement('li');
      li.appendChild(a);
      list.appendChild(li);
    }

    elm = document.getElementById('query-block');
    qtext = document.getElementById('query-text');
    if(elm && qtext)
    {
      if(entity.query!=='')
      {
        qtext.innerHTML = unescape(entity.query);
        elm.style.display='block';
      }
      else
      {
        elm.style.display = 'none';
      }
    }
    
    showEntityListXml();
    getContainerMetrics();
    
    toggleView('entityList');
  }

  function getContainerMetrics()
  {
    var elm,url;
    
    elm = document.getElementById('container-xml');
    if(elm)
    {
        url = container.metricsUrl.replace('{@authority}',authority.id).replace('{@container}',container.id);
        ajax.httpGet(url,null,onAjaxComplete,true,'containerMetrics',{'accept-type':ssdsContentType});
    }
  }
  
  function showEntityListXml()
  {
    var elm,a,url;
    elm = document.getElementById('list-xml');
    if(elm)
    {
        url = '/sds-proxy/'+authority.id+'/'+container.id+'/'+(entity.query!=''?'?'+entity.query:'');
        a = document.createElement('a');
        a.href = url;
        a.onclick = function(){var w = window.open(url,'xml_window','status=0,toolbar=0,location=0,menubar=0,directories=0,resizable=1,scrollbars=1,height=600,width=600');w.focus();return false;}
        a.target = "xml_window";
        a.appendChild(document.createTextNode(viewObjectText));
        elm.appendChild(a);
    }
  }

  function processContainerMetrics(response,headers)
  {
    var xml,dl,dt,dd,i,name,value;
    
    dl = document.getElementById('container-metrics');
    xml = response.selectNodes('//s:Container/*');
    for(i=0;i<xml.length;i++)
    {
      name = xml[i].tagName;
      try{value = xml[i].firstChild.nodeValue.replace(/^\s+|\s+$/g, '');}catch(ex){value='';}
      if(value===''){value='.';}

      dt = document.createElement('dt');
      dt.appendChild(document.createTextNode(name));
      dd = document.createElement('dd');
      dd.appendChild(document.createTextNode(value));

      dl.appendChild(dt);
      dl.appendChild(dd);
    }
  }
  
  function processGetEntityEdit(response,headers)
  {
    var xml,elm;

    xml = response;
    elm = document.getElementById('entityEdit-xml');
    if(elm)
    {
      elm.value = xml;
    }
    toggleView('entityEdit');
    elm.focus();
  }

  function processGetEntityItem(response,headers)
  {
    var xml,dl,dt,dd,name,value,url,a;

    dl = document.getElementById('entityItem-fields');
    xml = response.selectNodes('//*');
    for(i=0;i<xml.length;i++)
    {
      name = xml[i].tagName;
      if(name=='s:Content')
      {
        try{value = 'content-type="'+xml[i].getAttribute('content-type')+'" content-length="'+xml[i].getAttribute('content-length')+'"';}catch(ex){value='';}
        if(value===''){value='.';}
        url = entity.itemUrl.replace('{@authority}',authority.id).replace('{@container}',container.id).replace('{@entity}',encodeURIComponent(entity.id)+'&obj=y');
      }
      else
      {
        try{value = xml[i].firstChild.nodeValue.replace(/^\s+|\s+$/g, '');}catch(ex){value='';}
        if(value===''){value='.';}
      }

      dt = document.createElement('dt');
      dt.appendChild(document.createTextNode(name));
      dd = document.createElement('dd');
      dd.appendChild(document.createTextNode(value));

      dl.appendChild(dt);
      dl.appendChild(dd);
    }

    if(url)
    {
      dt = document.createElement('dt');
      dt.appendChild(document.createTextNode(''));

      a = document.createElement('a');
      a.href = url;
      a.onclick = function(){var w = window.open(url,'object','status=0,toolbar=0,location=0,menubar=0,directories=0,resizable=1,scrollbars=1,height=400,width=400');w.focus();return false;}
      a.title = 'view object';
      a.appendChild(document.createTextNode('view object'));

      dd = document.createElement('dd');
      dd.appendChild(a);

      dl.appendChild(dt);
      dl.appendChild(dd);

    }

    elm = document.getElementById('entityItem-edit');
    if(url)
    {
      elm.disabled=true;
    }
    else
    {
      elm.disabled=false;
    }
  
    showEntityItemXml();  
    
    toggleView('entityItem');
  }

  function showEntityItemXml()
  {
    var elm,a,url;
    elm = document.getElementById('item-xml');
    if(elm)
    {
        if(name=='s:Content')
        {
            elm.innerHTML='';
        }
        else
        {
            url = '/sds-proxy/'+authority.id+'/'+container.id+'/'+encodeURIComponent(entity.id);
            a = document.createElement('a');
            a.href = url;
            a.onclick = function(){var w = window.open(url,'xml_window','status=0,toolbar=0,location=0,menubar=0,directories=0,resizable=1,scrollbars=1,height=600,width=600');w.focus();return false;}
            a.target = "xml_window";
            a.appendChild(document.createTextNode(viewObjectText));
            elm.appendChild(a);
        }
    }
  }
  
  function processAddAuthority(status)
  {
    if(status<400)
    {
      alert('Authority has been added.');
      location.href = location.pathname;
    }
  }

  function processUpdateContainer(status)
  {
    if(status<400)
    {
      location.href = location.pathname+'?aid='+authority.id;
    }
  }

  function processUpdateEntity(status)
  {
    if(status<400)
    {
      location.href = location.pathname+'?aid='+authority.id+'&cid='+container.id;
    }
  }

  function processEditEntity(status)
  {
    if(status<400)
    {
      location.href = location.pathname+'?aid='+authority.id+'&cid='+container.id+'&eid='+encodeURIComponent(entity.id);
    }
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

  function attachEvents()
  {
    document.getElementById('home').onclick = showHome;
    document.getElementById('whats-new-link').onclick = showWhatsNew;
    document.getElementById('commands-logout').onclick = logoutUser;

    document.getElementById('authenticateUser-form').onsubmit = authenticateUserSubmit;
    document.getElementById('authenticateUser-back').onclick = authenticateUserBack;

    document.getElementById('authorityView-form').onsubmit = authorityViewSubmit;
    document.getElementById('authorityView-back').onclick = authorityViewBack;

    document.getElementById('containerFilter-form').onsubmit = containerFilterSubmit;
    document.getElementById('containerFilter-add').onclick = showAuthorityView;

    document.getElementById('containerList-add').onclick = containerListAdd;
    document.getElementById('containerList-refresh').onclick = containerListRefresh;
    document.getElementById('containerList-home').onclick = showHome;

    document.getElementById('containerAdd-form').onsubmit = containerAddSubmit;
    document.getElementById('containerAdd-back').onclick = containerAddBack;

    document.getElementById('entityList-query').onclick = entityListQuery;
    document.getElementById('entityList-clear').onclick = entityListClear;
    document.getElementById('entityList-refresh').onclick = entityListRefresh;
    document.getElementById('entityList-add').onclick = entityListAdd;
    document.getElementById('entityList-delete').onclick = entityListDelete;
    document.getElementById('entityList-back').onclick = entityListBack;
    document.getElementById('entityList-home').onclick = showHome;

    document.getElementById('entityItem-edit').onclick = entityItemEdit;
    document.getElementById('entityItem-delete').onclick = entityItemDelete;
    document.getElementById('entityItem-back').onclick = entityItemBack;

    document.getElementById('entityAdd-form').onsubmit = entityAddSubmit;
    document.getElementById('entityAdd-back').onclick = entityAddBack;

    document.getElementById('entityEdit-update').onclick = entityEditUpdate;
    document.getElementById('entityEdit-back').onclick = entityEditBack;

    document.getElementById('entityQuery-form').onsubmit = entityQuerySubmit;
    document.getElementById('entityQuery-back').onclick = entityQueryBack;

    document.getElementById('authority-metrics-link').onclick = showAuthorityMetrics;
    document.getElementById('container-metrics-link').onclick = showContainerMetrics;
    
  }

  function updateUI()
  {
    var coll,i;

    coll = document.getElementsByClassName('authority-name');
    for(i=0;i<coll.length;i++)
    {
      coll[i].innerHTML = authority.id;
    }

    coll = document.getElementsByClassName('container-name');
    for(i=0;i<coll.length;i++)
    {
      coll[i].innerHTML = container.id;
    }

    if(userName!=='')
    {
      coll = document.getElementsByClassName('user-name');
      for(i=0;i<coll.length;i++)
      {
        coll[i].innerHTML = "Logged in as: "+userName;
      }
    }
  }

  function toggleUser()
  {
    var auth,name,coll,i;

    auth = cookies.read(authCookie);
    if(auth)
    {
      document.getElementById('logout').style.display='inline';

      userName = Base64.decode(auth).split(':')[0];
    }
    else
    {
      userName = '';
      document.getElementById('logout').style.display='none';
    }
  }

  function toggleView(view)
  {
    var vw,vs;

    vw = view || 'commands';
    vs = {authenticateUser:'none',authorityView:'none',containerFilter:'none',containerList:'none',containerAdd:'none',entityList:'none',entityItem:'none',entityAdd:'none',entityEdit:'none',entityQuery:'none',loading:'none'};

    switch(vw)
    {
      case 'commands':
        vs = {authenticateUser:'none',authorityView:'none',containerFilter:'none',containerList:'none',containerAdd:'none',entityList:'none',entityItem:'none',entityAdd:'none',entityEdit:'none',entityQuery:'none',loading:'none'};
        break;
      case 'authenticateUser':
        vs = {authenticateUser:'block',authorityView:'none',containerFilter:'none',containerList:'none',containerAdd:'none',entityList:'none',entityItem:'none',entityAdd:'none',entityEdit:'none',entityQuery:'none',loading:'none'};
        break;
      case 'authorityView':
        vs = {authenticateUser:'none',authorityView:'block',containerFilter:'none',containerList:'none',containerAdd:'none',entityList:'none',entityItem:'none',entityAdd:'none',entityEdit:'none',entityQuery:'none',loading:'none'};
        break;
      case 'containerFilter':
        vs = {authenticateUser:'none',authorityView:'none',containerFilter:'block',containerList:'none',containerAdd:'none',entityList:'none',entityItem:'none',entityAdd:'none',entityEdit:'none',entityQuery:'none',loading:'none'};
        break;
      case 'containerList':
        vs = {authenticateUser:'none',authorityView:'none',containerFilter:'none',containerList:'block',containerAdd:'none',entityList:'none',entityItem:'none',entityAdd:'none',entityEdit:'none',entityQuery:'none',loading:'none'};
        break;
      case 'containerAdd':
        vs = {authenticateUser:'none',authorityView:'none',containerFilter:'none',containerList:'none',containerAdd:'block',entityList:'none',entityItem:'none',entityAdd:'none',entityEdit:'none',entityQuery:'none',loading:'none'};
        break;
      case 'entityList':
        vs = {authenticateUser:'none',authorityView:'none',containerFilter:'none',containerList:'none',containerAdd:'none',entityList:'block',entityItem:'none',entityAdd:'none',entityEdit:'none',entityQuery:'none',loading:'none'};
        break;
      case 'entityItem':
        vs = {authenticateUser:'none',authorityView:'none',containerFilter:'none',containerList:'none',containerAdd:'none',entityList:'none',entityItem:'block',entityAdd:'none',entityEdit:'none',entityQuery:'none',loading:'none'};
        break;
      case 'entityAdd':
        vs = {authenticateUser:'none',authorityView:'none',containerFilter:'none',containerList:'none',containerAdd:'none',entityList:'none',entityItem:'none',entityAdd:'block',entityEdit:'none',entityQuery:'none',loading:'none'};
        break;
      case 'entityEdit':
        vs = {authenticateUser:'none',authorityView:'none',containerFilter:'none',containerList:'none',containerAdd:'none',entityList:'none',entityItem:'none',entityAdd:'none',entityEdit:'block',entityQuery:'none',loading:'none'};
        break;
      case 'entityQuery':
        vs = {authenticateUser:'none',authorityView:'none',containerFilter:'none',containerList:'none',containerAdd:'none',entityList:'none',entityItem:'none',entityAdd:'none',entityEdit:'none',entityQuery:'block',loading:'none'};
        break;
      case 'loading':
        vs = {authenticateUser:'none',authorityView:'none',containerFilter:'none',containerList:'none',containerAdd:'none',entityList:'none',entityItem:'none',entityAdd:'none',entityEdit:'none',entityQuery:'none',loading:'block'};
        break;
      default:
        vs = {authenticateUser:'none',authorityView:'none',containerFilter:'none',containerList:'none',containerAdd:'none',entityList:'none',entityItem:'none',entityAdd:'none',entityEdit:'none',entityQuery:'none',loading:'none'};
        break;
    }

    document.getElementById('authenticateUser').style.display = vs.authenticateUser;
    document.getElementById('authorityView').style.display = vs.authorityView;
    document.getElementById('containerFilter').style.display = vs.containerFilter;
    document.getElementById('containerList').style.display = vs.containerList;
    document.getElementById('containerAdd').style.display = vs.containerAdd;
    document.getElementById('entityList').style.display = vs.entityList;
    document.getElementById('entityItem').style.display = vs.entityItem;
    document.getElementById('entityAdd').style.display = vs.entityAdd;
    document.getElementById('entityEdit').style.display = vs.entityEdit;
    document.getElementById('entityQuery').style.display = vs.entityQuery;
    document.getElementById('loading').style.display = vs.loading;
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
  var p = provision();
  p.init();
};
