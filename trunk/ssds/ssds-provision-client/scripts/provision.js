/* 2008-06-23 (mca) for SSDS provisioning client */

var pg =
{
  user_name : '',
  authority : '',
  container : '',
  entity : '',

  authority_add_url : '/ssds/authority/',
  authority_add_xml : '<authority>{@authority}</authority>',

  container_list_url : '/ssds/container/{@authority}/',
  container_delete_url : '/ssds/container/{@authority}/{@container}',
  container_add_url : '/ssds/container/{@authority}/',
  container_add_xml : '<container>{@container}</container>',

  entity_list_url : '/ssds/container/{@authority}/{@container}',
  entity_item_url : '/ssds/entity/{@authority}/{@container}/{@entity}',
  entity_delete_url : '/ssds/entity/{@authority}/{@container}/{@entity}',
  entity_add_url : '/ssds/entity/{@authority}/{@container}/',
  entity_add_xml : '<{@kind} xmlns:s="http://schemas.microsoft.com/sitka/2008/03/" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:x="http://www.w3.org/2001/XMLSchema"> \n \
<s:Id>$id$</s:Id> \n \
<name xsi:type="x:string"></name> \n \
</{@kind}>',

  init : function()
  {
    pg.authority = pg.getAuthorityArg();
    pg.container = pg.getContainerArg();
    pg.entity = pg.getEntityArg();

    pg.attachEvents();
    pg.toggleUser();
    pg.updateUI();

    pg.loadAuthorities();

    if(pg.authority!='' && pg.container!='' && pg.entity!='')
    {
      pg.showEntityItem();
    }
    else
    {
      if(pg.authority!='' && pg.container!='')
      {
        pg.showEntityList();
      }
      else
      {
        if(pg.authority!='')
        {
          pg.showContainerList();
        }
        else
        {
          pg.toggleView('commands');
        }
      }
    }
  },

  loadAuthorities:function()
  {
    var elm,i;

    elm = document.getElementById('containerFilter-authority');
    if(elm && (elm.tagName.toLowerCase()=='select'))
    {
      elm.options.length=0;
      elm.options[0] = new Option('--Select--','');
      for(i=0;i<pg.authority_list.length;i++)
      {
        elm.options[i+1] = new Option(pg.authority_list[i],pg.authority_list[i]);
      }
    }
  },

  showAuthentication:function()
  {
    pg.toggleView('authenticateUser');
    document.getElementById('auth-name').focus();
  },

  authenticateUserSubmit:function()
  {
    var user,pass;

    user = document.getElementById('auth-name').value;
    pass = document.getElementById('auth-pass').value;
    if(user!='' && pass!='')
    {
      cookies.create('x-form-authorization',Base64.encode(user+':'+pass));
      pg.authenticateUserBack();
    }
    return false;
  },

  authenticateUserBack:function()
  {
    location.href = location.pathname;
  },

  logoutUser:function()
  {
    cookies.erase('x-form-authorization');
    location.href = location.pathname;
  },

  showAuthorityView:function()
  {
    var elm;
    elm = document.getElementById('authority-name')
    pg.toggleView('authorityView');
    elm.value='';
    elm.focus();

    return false;
  },

  authorityViewSubmit:function()
  {
    var elm,name,data,url;

    elm = document.getElementById('authority-name');
    if(elm && elm.value!='')
    {
      name = elm.value.replace(/\s/g,'-').toLowerCase();
      url = pg.authority_add_url;
      data = pg.authority_add_xml.replace('{@authority}',name);
      ajax.httpPost(url,null,pg.onAjaxComplete,true,'addAuthority','application/xml',data);
    }
    return false;
  },

  authorityViewBack:function()
  {
    location.href = location.pathname;
  },

  showContainerFilter:function()
  {
    var elm;

    elm = document.getElementById('containerFilter-authority');
    if(pg.authority!='')
    {
      elm.value = pg.authority;
    }

    pg.toggleView('containerFilter');
    elm.focus();

    return false;
  },

  containerFilterSubmit:function()
  {
    var elm;

    elm = document.getElementById('containerFilter-authority');
    if(elm && elm.value!='')
    {
      location.href = location.pathname+'?aid='+elm.value;
    }

    return false;
  },

  showContainerList:function()
  {
    var url;

    pg.toggleView('loading');
    url = pg.container_list_url.replace('{@authority}',pg.authority);
    ajax.httpGet(url,null,pg.onAjaxComplete,true,'getContainerList');
  },

  containerFilterBack:function()
  {
    location.href = location.pathname;
  },

  containerListAdd:function()
  {
    var elm;

    elm = document.getElementById('container-name');
    if(elm)
    {
      elm.value='';
    }

    pg.toggleView('containerAdd');
    elm.focus();
  },

  containerListBack:function()
  {
    pg.showContainerFilter();
  },

  containerAddSubmit:function()
  {
    var elm,name,url,data;

    elm = document.getElementById('container-name');
    if(elm && elm.value!='')
    {
      name = elm.value.replace(/\s/g,'-').toLowerCase();
      if (name.match(/^[a-z\-]+$/i))
      {
        url = pg.container_add_url.replace('{@authority}',pg.authority);
        data = pg.container_add_xml.replace('{@container}',name);
        ajax.httpPost(url,null,pg.onAjaxComplete,true,'addContainer','application/xml',data);
      }
      else
      {
        alert('Invalid container name.\nUse a-z, 0-9, and -');
      }
    }

    return false;
  },

  containerAddBack:function()
  {
    location.href = location.href;
  },

  showEntityList:function()
  {
    var url;

    pg.toggleView('loading');
    url = pg.entity_list_url.replace('{@authority}',pg.authority).replace('{@container}',pg.container);
    ajax.httpGet(url,null,pg.onAjaxComplete,true,'getEntityList',{'content-type':'application/xml'});
  },

  entityListAdd:function()
  {
    var elm;
    elm = document.getElementById('entityAdd-xml');
    if(elm)
    {
      elm.value=pg.entity_add_xml;
    }
    pg.toggleView('entityAdd');
    elm.focus();
  },

  entityListDelete:function()
  {
    var url;

    if(confirm('Ready to Delete the '+pg.container+' Container?')==true)
    {
      url = pg.container_delete_url.replace('{@authority}',pg.authority).replace('{@container}',pg.container);
      ajax.httpDelete(url,null,pg.onAjaxComplete,true,'deleteContainer',{'content-type':'application/xml'});
    }
  },

  entityListBack:function()
  {
    location.href = location.pathname+'?aid='+pg.authority;
  },

  showEntityItem:function()
  {
    var url;

    pg.toggleView('loading');
    url = pg.entity_item_url.replace('{@authority}',pg.authority).replace('{@container}',pg.container).replace('{@entity}',encodeURIComponent(pg.entity));
    ajax.httpGet(url,null,pg.onAjaxComplete,true,'getEntityItem',{'content-type':'application/xml'});
  },

  entityItemEdit:function()
  {
    var url;
    url =   pg.entity_item_url.replace('{@authority}',pg.authority).replace('{@container}',pg.container).replace('{@entity}',encodeURIComponent(pg.entity));
    ajax.httpGet(url,null,pg.onAjaxComplete,false,'getEntityEdit',{'content-type':'application/xml'});
  },

  entityItemDelete:function()
  {
    var url;

    if(confirm('Ready to Delete '+pg.container+' entity ['+pg.entity+']?')==true)
    {
      url = pg.entity_delete_url.replace('{@authority}',pg.authority).replace('{@container}',pg.container).replace('{@entity}',encodeURIComponent(pg.entity));
      ajax.httpDelete(url,null,pg.onAjaxComplete,true,'deleteEntity',{'content-type':'application/xml'});
    }
  },

  entityItemBack:function()
  {
    location.href = location.pathname+'?aid='+pg.authority+'&cid='+pg.container
  },

  entityAddSubmit:function()
  {
    var elm,url,data;

    elm = document.getElementById('entityAdd-xml');
    if(elm)
    {
      data = elm.value;
      url = pg.entity_add_url.replace('{@authority}',pg.authority).replace('{@container}',pg.container);
      ajax.httpPost(url,null,pg.onAjaxComplete,true,'addEntity','application/xml',data);
    }
    return false;
  },

  entityAddBack:function()
  {
    location.href = location.pathname+'?aid='+pg.authority+'&cid='+pg.container
  },

  entityEditUpdate:function()
  {
    var elm,url,data;

    elm = document.getElementById('entityEdit-xml');
    if(elm)
    {
      data = elm.value;
      url = pg.entity_item_url.replace('{@authority}',pg.authority).replace('{@container}',pg.container).replace('{@entity}',encodeURIComponent(pg.entity));
      ajax.httpPut(url,null,pg.onAjaxComplete,true,'editEntity','application/xml',data);
    }
    return false;
  },

  entityEditBack:function()
  {
    location.href = location.pathname+'?aid='+pg.authority+'&cid='+pg.container+'&eid='+encodeURIComponent(pg.entity);
  },

  onAjaxComplete : function(response,headers,context,status,msg)
  {
    switch(status)
    {
      case 0:
        // ie abort code
        return;
        break;
      case 200:   // OK
      case 201:   // Created
      case 202:   // Accepted
      case 204:   // NoContent
        break;
      case 301:
      case 302:
        if(headers["location"] && headers['location']!='')
        {
          location.href=headers["location"];
        }
        return;
        break;
      default:    // 400 & 500 errors
        alert(status+'\n'+msg);
        //location.href = location.pathname;
        break;
    }

    switch(context)
    {
      case 'loginUser':
        pg.processLoginUser(response,headers);
        break;
      case 'getContainerList':
        pg.processGetContainerList(response,headers);
        break;
      case 'getEntityList':
        pg.processGetEntityList(response,headers);
        break;
      case 'getEntityItem':
        pg.processGetEntityItem(response,headers);
        break;
      case 'getEntityEdit':
        pg.processGetEntityEdit(response,headers);
        break;
      case 'addEntity':
      case 'deleteEntity':
        pg.processUpdateEntity(status);
        break;
      case 'editEntity':
        pg.processEditEntity(status);
        break;
      case 'addContainer':
      case 'deleteContainer':
        pg.processUpdateContainer(status);
        break;
      case 'addAuthority':
        pg.processAddAuthority(status);
        break;
      default:
        alert('Unknown context\n'+context);
    }
  },

  processGetContainerList:function(response,headers)
  {
    var list,xml,li,a,i,id;

    list = document.getElementById('container-items');
    xml = response.selectNodes('//s:Container');
    for(i=0;i<xml.length;i++)
    {
      id = xml[i].selectSingleNode('s:Id/text()').nodeValue;

      a = document.createElement('a');
      a.href = location.pathname+'?aid='+pg.authority+'&cid='+escape(id);
      a.title='View Entities';
      a.appendChild(document.createTextNode(id));

      li = document.createElement('li');
      li.appendChild(a);

      list.appendChild(li);
    }
    pg.toggleView('containerList');
  },

  processGetEntityList:function(response,headers)
  {
    var list,xml,a,li,i,id,kind;

    list = document.getElementById('entityList-items');
    xml = response.selectNodes('//s:EntitySet/*');
    for(i=0;i<xml.length;i++)
    {
      kind = xml[i].tagName;
      id = xml[i].selectSingleNode('s:Id/text()').nodeValue;

      a = document.createElement('a');
      a.href = location.pathname+'?aid='+pg.authority+'&cid='+pg.container+'&eid='+encodeURIComponent(id);
      a.title='Entity Detail';
      a.appendChild(document.createTextNode(kind+' ['+id+']'));

      li = document.createElement('li');
      li.appendChild(a);

      list.appendChild(li);
    }
    pg.toggleView('entityList');
  },

  processGetEntityEdit:function(response,headers)
  {
    var xml,elm;

    xml = response;
    elm = document.getElementById('entityEdit-xml');
    if(elm)
    {
      elm.value = xml;
    }
    pg.toggleView('entityEdit');
    elm.focus();
  },

  processGetEntityItem:function(response,headers)
  {
    var xml,dl,dt,dd,name,value;

    dl = document.getElementById('entityItem-fields');
    xml = response.selectNodes('descendant::*');
    for(i=0;i<xml.length;i++)
    {
      name = xml[i].tagName;
      try{value = xml[i].firstChild.nodeValue.replace(/^\s+|\s+$/g, '');}catch(ex){value='';}
      if(value==''){value='.'};

      dt = document.createElement('dt');
      dt.appendChild(document.createTextNode(name));
      dd = document.createElement('dd');
      dd.appendChild(document.createTextNode(value));

      dl.appendChild(dt);
      dl.appendChild(dd);
    }
    pg.toggleView('entityItem');
  },

  processAddAuthority:function(status)
  {
    if(status<400)
    {
      alert('Authority has been added.')
      location.href = location.pathname;
    }
  },

  processUpdateContainer:function(status)
  {
    if(status<400)
    {
      location.href = location.pathname+'?aid='+pg.authority;
    }
  },

  processUpdateEntity:function(status)
  {
    if(status<400)
    {
      location.href = location.pathname+'?aid='+pg.authority+'&cid='+pg.container;
    }
  },

  processEditEntity:function(status)
  {
    if(status<400)
    {
      location.href = location.pathname+'?aid='+pg.authority+'&cid='+pg.container+'&eid='+encodeURIComponent(pg.entity);
    }
  },

  getAuthorityArg : function()
  {
    var srch,id,rex,match;

    srch = location.search;
    rex = /\?aid=([^&]*)/i;
    match = rex.exec(srch);
    if (match != null)
    {
      id=match[1];
    }
    else
    {
      id='';
    }
    return id;
  },

  getContainerArg : function()
  {
    var srch,id,rex,match;

    srch = location.search;
    rex = /\&cid=([^&]*)/i;
    match = rex.exec(srch);
    if (match != null)
    {
      id=match[1];
    }
    else
    {
      id='';
    }
    return id;
  },

  getEntityArg : function()
  {
    var srch,id,rex,match;

    srch = location.search;
    rex = /\&eid=([^&]*)/i;
    match = rex.exec(srch);
    if (match != null)
    {
      id=match[1];
    }
    else
    {
      id='';
    }
    return id;
  },

  attachEvents : function()
  {
    document.getElementById('commands-createAuthority').onclick = pg.showAuthorityView;
    document.getElementById('commands-manageContainers').onclick = pg.showContainerFilter;
    document.getElementById('commands-authenticateUser').onclick = pg.showAuthentication;
    document.getElementById('commands-logout').onclick = pg.logoutUser;

    document.getElementById('authenticateUser-form').onsubmit = pg.authenticateUserSubmit;
    document.getElementById('authenticateUser-back').onclick = pg.authenticateUserBack;

    document.getElementById('authorityView-form').onsubmit = pg.authorityViewSubmit;
    document.getElementById('authorityView-back').onclick = pg.authorityViewBack;

    document.getElementById('containerFilter-form').onsubmit = pg.containerFilterSubmit;
    document.getElementById('containerFilter-back').onclick = pg.containerFilterBack;

    document.getElementById('containerList-add').onclick = pg.containerListAdd;
    document.getElementById('containerList-back').onclick = pg.containerListBack;

    document.getElementById('containerAdd-form').onsubmit = pg.containerAddSubmit;
    document.getElementById('containerAdd-back').onclick = pg.containerAddBack;

    document.getElementById('entityList-add').onclick = pg.entityListAdd;
    document.getElementById('entityList-delete').onclick = pg.entityListDelete;
    document.getElementById('entityList-back').onclick = pg.entityListBack;

    document.getElementById('entityItem-edit').onclick = pg.entityItemEdit;
    document.getElementById('entityItem-delete').onclick = pg.entityItemDelete;
    document.getElementById('entityItem-back').onclick = pg.entityItemBack;

    document.getElementById('entityAdd-form').onsubmit = pg.entityAddSubmit;
    document.getElementById('entityAdd-back').onclick = pg.entityAddBack;

    document.getElementById('entityEdit-update').onclick = pg.entityEditUpdate;
    document.getElementById('entityEdit-back').onclick = pg.entityEditBack;
  },

  updateUI:function()
  {
    var coll,i;

    coll = document.getElementsByClassName('authority-name');
    for(i=0;i<coll.length;i++)
    {
      coll[i].innerHTML = pg.authority;
    }

    coll = document.getElementsByClassName('container-name');
    for(i=0;i<coll.length;i++)
    {
      coll[i].innerHTML = pg.container;
    }

    if(pg.user_name!='')
    {
      coll = document.getElementsByClassName('user-name');
      for(i=0;i<coll.length;i++)
      {
        coll[i].innerHTML = "Logged in as: "+pg.user_name;
      }
    }

  },

  toggleUser:function()
  {
    var auth,name,coll,i;

    auth = cookies.read('x-form-authorization');
    if(auth)
    {
      document.getElementById('login').style.display='none';
      document.getElementById('logout').style.display='inline';

      pg.user_name = Base64.decode(auth).split(':')[0];
    }
    else
    {
      pg.user_name = '';
      document.getElementById('login').style.display='inline';
      document.getElementById('logout').style.display='none';
    }
  },

  toggleView:function(view)
  {
    var vw,vs;

    vw = view || 'commands';
    vs = {'commands':'block','authenticateUser':'none','authorityView':'none','containerFilter':'none','containerList':'none','containerAdd':'none','entityList':'none','entityItem':'none','entityAdd':'none','entityEdit':'none','loading':'none'};

    switch(vw)
    {
      case 'commands':
        vs = {'commands':'block','authenticateUser':'none','authorityView':'none','containerFilter':'none','containerList':'none','containerAdd':'none','entityList':'none','entityItem':'none','entityAdd':'none','entityEdit':'none','loading':'none'};
        break;
      case 'authenticateUser':
        vs = {'commands':'none','authenticateUser':'block','authorityView':'none','containerFilter':'none','containerList':'none','containerAdd':'none','entityList':'none','entityItem':'none','entityAdd':'none','entityEdit':'none','loading':'none'};
        break;
      case 'authorityView':
        vs = {'commands':'none','authenticateUser':'none','authorityView':'block','containerFilter':'none','containerList':'none','containerAdd':'none','entityList':'none','entityItem':'none','entityAdd':'none','entityEdit':'none','loading':'none'};
        break;
      case 'containerFilter':
        vs = {'commands':'none','authenticateUser':'none','authorityView':'none','containerFilter':'block','containerList':'none','containerAdd':'none','entityList':'none','entityItem':'none','entityAdd':'none','entityEdit':'none','loading':'none'};
        break;
      case 'containerList':
        vs = {'commands':'none','authenticateUser':'none','authorityView':'none','containerFilter':'none','containerList':'block','containerAdd':'none','entityList':'none','entityItem':'none','entityAdd':'none','entityEdit':'none','loading':'none'};
        break;
      case 'containerAdd':
        vs = {'commands':'none','authenticateUser':'none','authorityView':'none','containerFilter':'none','containerList':'none','containerAdd':'block','entityList':'none','entityItem':'none','entityAdd':'none','entityEdit':'none','loading':'none'};
        break;
      case 'entityList':
        vs = {'commands':'none','authenticateUser':'none','authorityView':'none','containerFilter':'none','containerList':'none','containerAdd':'none','entityList':'block','entityItem':'none','entityAdd':'none','entityEdit':'none','loading':'none'};
        break;
      case 'entityItem':
        vs = {'commands':'none','authenticateUser':'none','authorityView':'none','containerFilter':'none','containerList':'none','containerAdd':'none','entityList':'none','entityItem':'block','entityAdd':'none','entityEdit':'none','loading':'none'};
        break;
      case 'entityAdd':
        vs = {'commands':'none','authenticateUser':'none','authorityView':'none','containerFilter':'none','containerList':'none','containerAdd':'none','entityList':'none','entityItem':'none','entityAdd':'block','entityEdit':'none','loading':'none'};
        break;
      case 'entityEdit':
        vs = {'commands':'none','authenticateUser':'none','authorityView':'none','containerFilter':'none','containerList':'none','containerAdd':'none','entityList':'none','entityItem':'none','entityAdd':'none','entityEdit':'block','loading':'none'};
        break;
      case 'loading':
        vs = {'commands':'none','authenticateUser':'none','authorityView':'none','containerFilter':'none','containerList':'none','containerAdd':'none','entityList':'none','entityItem':'none','entityAdd':'none','entityEdit':'none','loading':'block'};
        break;
      default:
        vs = {'commands':'none','authenticateUser':'none','authorityView':'none','containerFilter':'none','containerList':'none','containerAdd':'none','entityList':'none','entityItem':'none','entityAdd':'none','entityEdit':'none','loading':'none'};
        break;
    }

    document.getElementById('commands').style.display = vs.commands;
    document.getElementById('authenticateUser').style.display = vs.authenticateUser;
    document.getElementById('authorityView').style.display = vs.authorityView;
    document.getElementById('containerFilter').style.display = vs.containerFilter;
    document.getElementById('containerList').style.display = vs.containerList;
    document.getElementById('containerAdd').style.display = vs.containerAdd;
    document.getElementById('entityList').style.display = vs.entityList;
    document.getElementById('entityItem').style.display = vs.entityItem;
    document.getElementById('entityAdd').style.display = vs.entityAdd;
    document.getElementById('entityEdit').style.display = vs.entityEdit;
    document.getElementById('loading').style.display = vs.loading;
  }
}

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
  pg.init();
}
