// (c)2007 mamund@yahoo.com

﻿
var md={ovl:null,dialogCss:'md_dialog',popupCss:'md_popup',overlayCss:'md_overlay',init:function()
{md.ovl=document.createElement('div');md.ovl.id=md.overlayCss;document.body.appendChild(md.ovl);},show:function(elementId,className,eventRef)
{var cssName=md.dialogCss;if(className)
cssName=className;var elm=document.getElementById(elementId);if(elm)
{if(eventRef)
eval(eventRef);md.cssjs('remove',elm,md.popupCss);md.cssjs('add',elm,cssName);md.ovl.appendChild(elm);md.toggleDialog();}
md.cancelClick(e);},hide:function(elementId,className,eventRef)
{var cssName=md.dialogCss;if(className)
cssName=className;var elm=document.getElementById(elementId);if(elm)
{if(eventRef)
eval(eventRef);md.cssjs('remove',elm,cssName);md.cssjs('add',elm,md.popupCss);md.toggleDialog();}
md.cancelClick(e);},toggleDialog:function()
{md.ovl.style.visibility=(md.ovl.style.visibility=="visible")?"hidden":"visible";},getTarget:function(e){var target=window.event?window.event.srcElement:e?e.target:null;if(!target){return false;}
while(!target.tohide&&target.nodeName.toLowerCase()!='body')
{target=target.parentNode;}
return target;},cancelClick:function(e){if(window.event){window.event.cancelBubble=true;window.event.returnValue=false;return;}
if(e){e.stopPropagation();e.preventDefault();}},addEvent:function(elm,evType,fn,useCapture){if(elm.addEventListener)
{elm.addEventListener(evType,fn,useCapture);return true;}else if(elm.attachEvent){var r=elm.attachEvent('on'+evType,fn);return r;}else{elm['on'+evType]=fn;}},cssjs:function(a,o,c1,c2){switch(a){case'swap':o.className=!md.cssjs('check',o,c1)?o.className.replace(c2,c1):o.className.replace(c1,c2);break;case'add':if(!md.cssjs('check',o,c1)){o.className+=o.className?' '+c1:c1;}
break;case'remove':var rep=o.className.match(' '+c1)?' '+c1:c1;o.className=o.className.replace(rep,'');break;case'check':return new RegExp("(^|\\s)"+c1+"(\\s|$)").test(o.className)
break;}}}
md.addEvent(window,'load',md.init,false);