// (c)2007 mca
// ajax suggest library

var suggest={xhr:null,input:null,list:null,url:null,cssHide:'suggest_hide',cssShow:'suggest_show',cssOver:'suggest_over',cssOut:'suggest_out',cssLink:'suggest_link',init:function(inputId,listId,urlTemplate)
{suggest.xhr=suggest.getXHR();if(suggest.xhr===null)
{alert('*** ERROR:\n\nAjax not supported!');return;}
suggest.input=document.getElementById(inputId);suggest.list=document.getElementById(listId);suggest.url=urlTemplate;suggest.addEvent(suggest.input,'keyup',suggest.execute,false);suggest.addEvent(document,'click',suggest.hideList,false);},execute:function()
{var val='';if(suggest.xhr.readyState===4||suggest.xhr.readyState===0)
{val=escape(suggest.input.value);try
{suggest.xhr.open("GET",suggest.url.replace('{@search}',val),true);suggest.xhr.onreadystatechange=suggest.onComplete;suggest.xhr.send(null);}
catch(ex)
{alert('*** ERROR:\n\n'+ex.message);}}},getXHR:function()
{if(window.XMLHttpRequest!==null)
{return new XMLHttpRequest();}
if(window.ActiveXObject!==null)
{return new ActiveXObject("Microsoft.XMLHTTP");}
return null;},onComplete:function()
{var data,results,elm;if(suggest.xhr.readyState===4)
{if(suggest.xhr.status!==200)
{alert('*** ERROR:\n\n'+suggest.xhr.status+'\n'+suggest.xhr.statusText);return;}
data=suggest.xhr.responseText.split("\n");results='';suggest.list.innerHTML='';for(i=0;i<data.length-1;i++)
{results+='<div onmouseover="suggest.mouseOver(this);" ';results+='onmouseout="suggest.mouseOut(this);" ';results+='onclick="suggest.click(this.innerHTML);" ';results+='class="'+suggest.cssLink+'">{@results}</div>'.replace('{@results}',data[i]);}
suggest.list.innerHTML+=results;suggest.list.className=suggest.cssShow;}},mouseOver:function(elm)
{elm.className=suggest.cssOver;elm.style.cursor='pointer';},mouseOut:function(elm)
{elm.className=suggest.cssLink;elm.style.cursor='default';},click:function(data)
{suggest.input.value=data;suggest.hideList();},hideList:function()
{suggest.list.innerHTML='';suggest.list.className=suggest.cssHide;suggest.list.style.cursor='default';},addEvent:function(elm,evType,fn,useCapture)
{var r;if(elm.addEventListener)
{elm.addEventListener(evType,fn,useCapture);return true;}
if(elm.attachEvent)
{r=elm.attachEvent('on'+evType,fn);return r;}
elm['on'+evType]=fn;}};