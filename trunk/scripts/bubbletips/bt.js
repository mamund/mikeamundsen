/* 
  bubble tooltips

	$Date: 2007-09-26 00:06:01 -0400 (Wed, 26 Sep 2007) $
	$Rev: 57 $
	$Author: mikeamundsen $
*/
var bt =
{
    opacity:"90",
    cssHRef:"bt.css",
    maxUrlLen:30,
    
    init:function(args)
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
            'opacity':bt.opacity,
            'cssHRef':bt.cssHRef,
            'maxUrlLen':bt.maxUrlLen
        };
        
        // fill in any missing values
        args = bt.handleArgs(args,defaults);
        
        // set shared properties
        bt.cssHRef=args.cssHRef;
        bt.maxUrlLen=args.maxUrlLen;
        bt.opacity=args.opacity;

        if(args.id==null)
        {
            links=document.getElementsByTagName("a");
        }
        else
        {
            links=document.getElementById(args.id).getElementsByTagName("a");
        }

        // add the css file link
        bt.addCssLink();
        
        // create the tooltip element
        h=document.createElement("span");
        h.id="btc";
        h.setAttribute("id","btc");
        h.style.position="absolute";
        document.getElementsByTagName("body")[0].appendChild(h);
        
        // add tooltips as needed
        for(i=0;i<links.length;i++)
        {
            bt.prepare(links[i]);
        }
    },

    handleArgs:function(args,defaults)
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
    },
    
    prepare:function(el)
    {
        var tooltip,t,b,s,l;
        
        t=el.getAttribute("title");
        
        if(t==null || t.length==0)
        {
            t="link:";
        }
        
        el.removeAttribute("title");
        tooltip=bt.addElement("span","tooltip");
        s=bt.addElement("span","top");
        s.appendChild(document.createTextNode(t));
        tooltip.appendChild(s);
        b=bt.addElement("b","bottom");
        l=el.getAttribute("href");
        
        if(l.length>bt.maxUrlLen)
        {
            l=l.substr(0,bt.maxUrlLen-3)+"...";
        }
        
        b.appendChild(document.createTextNode(l));
        tooltip.appendChild(b);
        bt.setOpacity(tooltip);
        
        el.tooltip=tooltip;
        el.onmouseover=bt.showTooltip;
        el.onmouseout=bt.hideTooltip;
        el.onmousemove=bt.positionTooltip;
    },
    
    showTooltip:function(e)
    {
        document.getElementById("btc").appendChild(this.tooltip);
        bt.positionTooltip(e);
    },
    
    hideTooltip:function(e)
    {
        var d=document.getElementById("btc");
        
        if(d.childNodes.length>0)
        {
            d.removeChild(d.firstChild);
        }
    },
    
    setOpacity:function(el)
    {
        el.style.filter="alpha(opacity:"+bt.opacity+")";
        el.style.KHTMLOpacity="0."+bt.opacity;
        el.style.MozOpacity="0."+bt.opacity;
        el.style.opacity="0."+bt.opacity;
    },
    
    addElement:function(t,c)
    {
        var x=document.createElement(t);
        
        x.className=c;
        x.style.display="block";
        
        return(x);
    },
    
    addCssLink:function()
    {
        var l=bt.addElement("link");
        
        l.setAttribute("type","text/css");
        l.setAttribute("rel","stylesheet");
        l.setAttribute("href",bt.cssHRef);
        l.setAttribute("media","screen");
        
        document.getElementsByTagName("head")[0].appendChild(l);
    },

    positionTooltip:function(e)
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
        document.getElementById("btc").style.top=(posy+10)+"px";
        document.getElementById("btc").style.left=(posx-20)+"px";
    }
};