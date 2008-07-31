/* 2008-06-23 (mca) for Tasklist SSDS demo */

var pg =
{
  id : '',
  tasks_url : '/ssds/tasks/',
  taskXml :
    '<task \
    xmlns:s="http://schemas.microsoft.com/sitka/2008/03/" \
    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" \
    xmlns:x="http://www.w3.org/2001/XMLSchema">\n \
    <s:Id>{@id}</s:Id>\n \
    <name xsi:type="x:string">{@name}</name>\n \
    <is-completed xsi:type="x:string">{@is-completed}</is-completed>\n \
    </task>',

  init : function()
  {
    pg.attachEvents();
    pg.id = pg.getIdArg();

    if(pg.id!='')
    {
      pg.getItem();
    }
    else
    {
      pg.getList();
    }
  },

  showAdd : function()
  {
    document.getElementById('add-id').value = '';
    document.getElementById('add-name').value = '';
    document.getElementById('add-completed').value = 'false';

    pg.toggleView('add');
  },

  addSave : function()
  {
    var xml;

    xml = pg.taskXml;
    xml = xml.replace('{@id}',document.getElementById('add-id').value);
    xml = xml.replace('{@name}',document.getElementById('add-name').value);
    xml = xml.replace('{@is-completed}',document.getElementById('add-completed').value);

    ajax.httpPost(pg.tasks_url,null,pg.onAjaxComplete,true,'addItem','application/x-ssds+xml',xml);
  },

  addCancel : function()
  {
    pg.refreshList();
  },

  editSave : function()
  {
    var xml;

    xml = pg.taskXml;
    xml = xml.replace('{@id}',document.getElementById('edit-id').value);
    xml = xml.replace('{@name}',document.getElementById('edit-name').value);
    xml = xml.replace('{@is-completed}',document.getElementById('edit-completed').value);

    ajax.httpPut(pg.tasks_url+pg.id,null,pg.onAjaxComplete,true,'updateItem','application/x-ssds+xml',xml);
  },

  editDelete:function()
  {
    if(confirm('Ready to delete this task?')==true)
    {
      ajax.httpDelete(pg.tasks_url+pg.id,null,pg.onAjaxComplete,true,'deleteItem',null);
    }
  },

  editCancel : function()
  {
    pg.refreshList();
  },

  refreshList:function()
  {
    location.href = location.pathname;
  },

  getIdArg : function()
  {
    var srch,id,rex,match;

    srch = location.search;
    rex = /\?id=([^&]*)/i;
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

  getItem : function()
  {
    pg.toggleView('loading');
    ajax.httpGet(pg.tasks_url+pg.id,null,pg.onAjaxComplete,true,'getItem');
  },

  getList : function()
  {
    pg.toggleView('loading');
    ajax.httpGet(pg.tasks_url,null,pg.onAjaxComplete,true,'getList');
  },

  onAjaxComplete : function(response,headers,context,status,msg)
  {
    switch(status)
    {
      case 0:
        // ie abort code
        break;
      case 200:   // OK
      case 201:   // Created
      case 202:   // Accepted
      case 204:   // NoContent
        break;
      case 301:
      case 302:
        if(headers["location"])
        {
          location.href=headers["location"];
        }
        return;
        break;
      default:    // 400 & 500 errors
        alert(status+'\n'+msg);
        return;
        break;
    }

    switch(context)
    {
      case 'addItem':
      case 'updateItem':
      case 'deleteItem':
        pg.processUpdateItem(status);
        break;
      case 'getItem':
        pg.processItem(response,headers);
        break;
      case 'getList':
        pg.processList(response,headers);
        break;
      default:
        alert('Unknown context\n'+context);
    }
  },

  processUpdateItem:function(status)
  {
    if(status<400)
    {
      pg.refreshList();
    }
  },

  processItem:function(response,headers)
  {
    var xml,view,coll,id,name,completed;

    xml = response.selectNodes('/task');
    id = xml[0].selectSingleNode('s:Id/text()').nodeValue;
    name = xml[0].selectSingleNode('name/text()').nodeValue;
    completed = xml[0].selectSingleNode('is-completed/text()').nodeValue;

    document.getElementById('edit-id').value = id;
    document.getElementById('edit-name').value = name;
    document.getElementById('edit-completed').value = completed;

    pg.toggleView('edit');
  },

  processList:function(response,headers)
  {
    var list,xml,li,a,i,id,name;

    list = document.getElementById('task-list');
    xml = response.selectNodes('//task');
    for(i=0;i<xml.length;i++)
    {
      id = xml[i].selectSingleNode('s:Id/text()').nodeValue;
      name = xml[i].selectSingleNode('name/text()').nodeValue;

      a = document.createElement('a');
      a.href = location.pathname+'?id='+id
      a.title='Edit Task';
      a.appendChild(document.createTextNode(name));

      li = document.createElement('li');
      li.appendChild(a);

      list.appendChild(li);
    }
    pg.toggleView('list')
  },

  attachEvents : function()
  {
    document.getElementById('add-new').onclick = pg.showAdd;
    document.getElementById('refresh-list').onclick = pg.refreshList;
    document.getElementById('add-save').onclick = pg.addSave;
    document.getElementById('add-cancel').onclick = pg.addCancel;
    document.getElementById('edit-save').onclick = pg.editSave;
    document.getElementById('edit-delete').onclick = pg.editDelete;
    document.getElementById('edit-cancel').onclick = pg.editCancel;
  },

    toggleView:function(view)
  {
    var vw,vs;
    vw = view || 'list';
    vs = {'list':'block','add':'none','edit':'none','loading':'none'};

    switch(vw)
    {
      case 'list':
        vs = {'list':'block','add':'none','edit':'none','loading':'none'};
        break;
      case 'add':
        vs = {'list':'none','add':'block','edit':'none','loading':'none'};
        break;
      case 'edit':
        vs = {'list':'none','add':'none','edit':'block','loading':'none'};
        break;
      case 'loading':
        vs = {'list':'none','add':'none','edit':'none','loading':'block'};
        break;
      default:
        vs = {'list':'block','add':'none','edit':'none','loading':'none'};
        break;
    }

    document.getElementById('list-view').style.display = vs.list;
    document.getElementById('add-view').style.display = vs.add;
    document.getElementById('edit-view').style.display = vs.edit;
    document.getElementById('loading').style.display = vs.loading;
  }
}

// execute on startup
window.onload = function()
{
  pg.init();
}
