// bubble tooltips
// (c) 2007 mike amundsen mamund@yahoo.com

function BubbleTips()
{var that=this;this.id=null;this.opacity=90;this.cssHRef="bt.css";this.maxUrlLen=30;this.tipId="btc";this.cssTooltip="tooltip";this.cssTop="top";this.cssBottom="bottom";this.init=function(args)
{var coll,i,tt,defaults;if(!document.getElementById||!document.getElementsByTagName)
{return;}
var defaults={id:null,opacity:that.opacity,cssHRef:that.cssHRef,maxUrlLen:that.maxUrlLen,tipId:that.tipId,cssTooltip:that.cssTooltip,cssTop:that.cssTop,cssBottom:that.cssBottom};args=handleArgs(args,defaults);this.cssHRef=args.cssHRef;this.maxUrlLen=args.maxUrlLen;this.opacity=args.opacity;if(args.id===null)
{coll=document.getElementsByTagName("a");}
else
{coll=document.getElementById(args.id).getElementsByTagName("a");}
addCssLink();tt=document.createElement("span");tt.id=that.tipId;tt.setAttribute("id",that.tipId);tt.style.position="absolute";document.getElementsByTagName("body")[0].appendChild(tt);for(i=0;i<coll.length;i++)
{convert(coll[i]);}};this.show=function(e)
{document.getElementById(that.tipId).appendChild(this.tooltip);that.position(e);};this.hide=function(e)
{var d=document.getElementById(that.tipId);if(d.childNodes.length>0)
{d.removeChild(d.firstChild);}};this.position=function(e)
{var posx=0,posy=0;if(e===null||typeof e=='undefined')
{e=window.event;}
if(e.pageX||e.pageY)
{posx=e.pageX;posy=e.pageY;}
else if(e.clientX||e.clientY)
{if(document.documentElement.scrollTop)
{posx=e.clientX+document.documentElement.scrollLeft;posy=e.clientY+document.documentElement.scrollTop;}
else
{posx=e.clientX+document.body.scrollLeft;posy=e.clientY+document.body.scrollTop;}}
document.getElementById(that.tipId).style.top=(posy+10)+"px";document.getElementById(that.tipId).style.left=(posx-20)+"px";};var convert=function(el)
{var tooltip,t,b,s,l;t=el.getAttribute("title");if(t===null||t.length===0)
{t="link:";}
el.removeAttribute("title");tooltip=addElement("span",that.cssTooltip);s=addElement("span",that.cssTop);s.appendChild(document.createTextNode(t));tooltip.appendChild(s);b=addElement("b",that.cssBottom);l=el.getAttribute("href");if(l.length>that.maxUrlLen)
{l=l.substr(0,that.maxUrlLen-3)+"...";}
b.appendChild(document.createTextNode(l));tooltip.appendChild(b);setOpacity(tooltip);el.tooltip=tooltip;el.onmouseover=that.show;el.onmouseout=that.hide;el.onmousemove=that.position;};var addElement=function(t,c)
{var x=document.createElement(t);x.className=c;x.style.display="block";return(x);};var addCssLink=function()
{var l=addElement("link");l.setAttribute("href",that.cssHRef);l.setAttribute("type","text/css");l.setAttribute("rel","stylesheet");l.setAttribute("media","screen");document.getElementsByTagName("head")[0].appendChild(l);};var setOpacity=function(el)
{el.style.filter="alpha(opacity:"+that.opacity+")";el.style.KHTMLOpacity="0."+that.opacity;el.style.MozOpacity="0."+that.opacity;el.style.opacity="0."+that.opacity;};var handleArgs=function(args,defaults)
{if(args===null||typeof args==='undefined')
{args=defaults;}
else
{for(var i in defaults)
{if(typeof args[i]==='undefined')
{args[i]=defaults[i];}}}
return args;};}