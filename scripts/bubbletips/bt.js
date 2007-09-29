/* 
  bubble tooltips

	$Date: 2007-09-26 00:06:01 -0400 (Wed, 26 Sep 2007) $
	$Rev: 57 $
	$Author: mikeamundsen $
*/

function BubbleTips()
{
		// private 
		var that = this;

		// public properties
    this.opacity = "90";
    this.cssHRef = "bt.css";
    this.maxUrlLen = 30;
    this.tipId = "btc";
    this.cssTooltip = "tooltip";
    this.cssTop = "top";
    this.cssBottom = "bottom";
		
		// privileged
    this.init = function(args)
    {
			var links,i,h,defaults;
			
			// js must be off
			if(!document.getElementById || !document.getElementsByTagName)
			{
			    return;
			}
			
			// set defaults
			defaults =
			{
			    'id':null,
			    'opacity':that.opacity,
			    'cssHRef':that.cssHRef,
			    'maxUrlLen':that.maxUrlLen,
			    'tipId':that.tipId,
			    'cssTooltip':that.cssTooltip,
			    'cssTop':that.cssTop,
			    'cssBottom':that.cssBottom
			};
			
			// fill in any missing values
			args = handleArgs(args,defaults);
			
			// set shared properties
			this.cssHRef=args.cssHRef;
			this.maxUrlLen=args.maxUrlLen;
			this.opacity=args.opacity;
			
			if(args.id==null)
			{
			    links=document.getElementsByTagName("a");
			}
			else
			{
			    links=document.getElementById(args.id).getElementsByTagName("a");
			}
			
			// add the css file link
			addCssLink();
			
			// create the tooltip element
			h=document.createElement("span");
			h.id=that.tipId;
			h.setAttribute("id",that.tipId);
			h.style.position="absolute";
			document.getElementsByTagName("body")[0].appendChild(h);
			
			// add tooltips as needed
			for(i=0;i<links.length;i++)
			{
			    prepare(links[i]);
			}
    };

		this.showTooltip = function(e)
		{
		    document.getElementById(that.tipId).appendChild(this.tooltip);
		    that.positionTooltip(e);
		}
		
		this.hideTooltip = function(e)
		{
		    var d=document.getElementById(that.tipId);
		    
		    if(d.childNodes.length>0)
		    {
		        d.removeChild(d.firstChild);
		    }
		}
		
		this.positionTooltip = function(e)
		{
		    var posx=0,posy=0;
		    
		    if(e==null)
		    {
		        e=window.event;
		    }
		    
		    if(e.pageX || e.pageY)
		    {
		        posx=e.pageX; posy=e.pageY;
		    }
		    else if(e.clientX || e.clientY)
		    {
		        if(document.documentElement.scrollTop)
		        {
		            posx=e.clientX+document.documentElement.scrollLeft;
		            posy=e.clientY+document.documentElement.scrollTop;
		        }
		        else
		        {
		            posx=e.clientX+document.body.scrollLeft;
		            posy=e.clientY+document.body.scrollTop;
		        }
		    }
		    document.getElementById(that.tipId).style.top=(posy+10)+"px";
		    document.getElementById(that.tipId).style.left=(posx-20)+"px";
		}

		// private methods
		var prepare = function(el)
		{
		    var tooltip,t,b,s,l;
		    
		    t=el.getAttribute("title");
		    
		    if(t==null || t.length==0)
		    {
		        t="link:";
		    }
		    
		    el.removeAttribute("title");
		    tooltip=addElement("span",that.cssTooltip);
		    s=addElement("span",that.cssTop);
		    s.appendChild(document.createTextNode(t));
		    tooltip.appendChild(s);
		    b=addElement("b",that.cssBottom);
		    l=el.getAttribute("href");
		    
		    if(l.length>that.maxUrlLen)
		    {
		        l=l.substr(0,that.maxUrlLen-3)+"...";
		    }
		    
		    b.appendChild(document.createTextNode(l));
		    tooltip.appendChild(b);
		    setOpacity(tooltip);
		    
		    el.tooltip=tooltip;
		    el.onmouseover=that.showTooltip;
		    el.onmouseout=that.hideTooltip;
		    el.onmousemove=that.positionTooltip;
		}

		var addElement = function(t,c)
		{
		    var x=document.createElement(t);
		    
		    x.className=c;
		    x.style.display="block";
		    
		    return(x);
		}
		
		var addCssLink = function()
		{
		    var l=addElement("link");
		    
		    l.setAttribute("href",that.cssHRef);
		    l.setAttribute("type","text/css");
		    l.setAttribute("rel","stylesheet");
		    l.setAttribute("media","screen");
		    
		    document.getElementsByTagName("head")[0].appendChild(l);
		}

		var setOpacity = function(el)
		{
		    el.style.filter="alpha(opacity:"+that.opacity+")";
		    el.style.KHTMLOpacity="0."+that.opacity;
		    el.style.MozOpacity="0."+that.opacity;
		    el.style.opacity="0."+that.opacity;
		}
    
		var handleArgs = function(args,defaults)
		{
		    if(args==null)
		    {
		        args = defaults;
		    }
		    else
		    {
		        for(var i in defaults)
		        {
		            if(typeof args[i] === 'undefined')
		            {
		                args[i]=defaults[i];
		            }
		        }
		    }
		    
		    return args;
		};
};

