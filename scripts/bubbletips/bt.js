/*  
    $Date: 2007-09-26 00:06:01 -0400 (Wed, 26 Sep 2007) $
    $Rev: 57 $
    $Author: mikeamundsen $
*/

/**
 * @fileoverview
 * BubbleTooltips converts simple 'title' HTML attributes into bubble-shapped pop-up tips.
 * Inspired by Alessandro Fulciniti - http://pro.html.it - http://web-graphics.com
 * @version 1.0.0
 * @author Mike Amundsen - mamund@yahoo.com
 */

/**
 * BubbleTips is a class that provides pop-up 'bubbles' for tooltips
 * @constructor
 */ 
function BubbleTips() {
    
    // private reference to self
    var that = this;
    
    // public properties *********************************************
    /** the DOM ID of the HTML element to which the tooltips are to be applied. default = null (the entire document) @type string*/
    this.id = null;
    /** The opacity of the tooltip bubble. default = 90 @type integer */
    this.opacity = 90;
    
    /** The URI reference for the css stylesheet. default = "bt.css"  @type string */
    this.cssHRef = "bt.css";
    
    /** The maximum display length of the URL (if exists). default = 30 @type integer */
    this.maxUrlLen = 30;
    
    /** The HTML DOM ID used to generate the tooltip element. default = "btc" @type string */
    this.tipId = "btc";
    
    /** The CSS class that defines the bubble display. default = "tooltip" @type string */
    this.cssTooltip = "tooltip";
    
    /** The CSS class that defines the style of the title text in the bubble. default = "top" @type string */ 
    this.cssTop = "top";
    
    /** The CSS class that defines the style of the href text in the bubble. default = "bottom" @type string */
    this.cssBottom = "bottom";

    // privileged methods ********************************************
    /** This function initializes the tooltips within the HTML document.
     * Call this method once at the start of your page, usually in the load event.
     * If you change any of the public properties, you should call this method again.
     * Pass property values via the associative array in the form {[propertyname]:[value],...}
     * @param args (associative array)
     * @see BubbleTips#id
     * @see BubbleTips#opacity
     * @see BubbleTips#cssHRef
     * @see BubbleTips#maxUrlLen
     * @see BubbleTips#tipId
     * @see BubbleTips#cssTooltip
     * @see BubbleTips#cssTop
     * @see BubbleTips#cssBottom
     */
    this.init = function(args) {
        var coll, i, tt, defaults;
        
        // js must be off
        if(!document.getElementById || !document.getElementsByTagName) {
            return;
        }
            
        defaults = {
            id : null,
            opacity : that.opacity,
            cssHRef : that.cssHRef,
            maxUrlLen : that.maxUrlLen,
            tipId : that.tipId,
            cssTooltip : that.cssTooltip,
            cssTop : that.cssTop,
            cssBottom : that.cssBottom
        };
        
        // fill in any missing values
        args = handleArgs(args, defaults);
        
        // set shared properties
        this.cssHRef = args.cssHRef;
        this.maxUrlLen = args.maxUrlLen;
        this.opacity = args.opacity;
        
        if(args.id === null) {
            coll = document.getElementsByTagName("a");
        } else {
            coll = document.getElementById(args.id).getElementsByTagName("a");
        }
        
        // add the css file link
        addCssLink();
        
        // create the tooltip element
        tt = document.createElement("span");
        tt.id = that.tipId;
        tt.setAttribute("id", that.tipId);
        tt.style.position = "absolute";
        document.getElementsByTagName("body")[0].appendChild(tt);
        
        // add tooltips as needed
        for(i=0; i<coll.length; i++) {
            convert(coll[i]);
        }
    };

    /**
     * This method is used to display the tooltip on mouseover events.
     * Called via events and not via user scripts.
     * @param e (object) optional event argument
     */ 
    this.show = function (e) {
        document.getElementById(that.tipId).appendChild(this.tooltip);
        that.position(e);
    };

    /**
     * This method is used to hide the tooltip on mouseout events.
     * Called via events and not via user scripts.
     * @param e (object) optional event argument
     */
    this.hide = function (e) {
        var d = document.getElementById(that.tipId);
        
        if(d.childNodes.length>0) {
            d.removeChild(d.firstChild);
        }
    };
    
    /**
     * This method is used to properly position the tooltip at runtime.
     * Called via events and not via user scripts.
     * @param e (object) optional event argument
     */
    this.position = function (e) {
        var posx=0, posy=0;
        
        if(e === null || typeof e === 'undefined') {
            e = window.event;
        }
        
        if(e.pageX || e.pageY) {
            posx = e.pageX;
            posy = e.pageY;
        } else if(e.clientX || e.clientY) {
            if(document.documentElement.scrollTop) {
                posx = e.clientX+document.documentElement.scrollLeft;
                posy = e.clientY+document.documentElement.scrollTop;
            } else {
                posx = e.clientX+document.body.scrollLeft;
                posy = e.clientY+document.body.scrollTop;
            }
        }
        document.getElementById(that.tipId).style.top = (posy + 10) + "px";
        document.getElementById(that.tipId).style.left = (posx - 20) + "px";
    };

    // private methods ***********************************************
    var convert = function (el) {
        var tooltip, t, b, s, l;
        
        t = el.getAttribute("title");
        
        if(t === null || t.length === 0) {
            t = "link:";
        }
        
        el.removeAttribute("title");
        tooltip = addElement("span", that.cssTooltip);
        s = addElement("span", that.cssTop);
        s.appendChild(document.createTextNode(t));
        tooltip.appendChild(s);
        b = addElement("b", that.cssBottom);
        l = el.getAttribute("href");
        
        if(l.length > that.maxUrlLen) {
            l = l.substr(0,that.maxUrlLen-3) + "...";
        }
        
        b.appendChild(document.createTextNode(l));
        tooltip.appendChild(b);
        setOpacity(tooltip);
        
        el.tooltip = tooltip;
        el.onmouseover = that.show;
        el.onmouseout = that.hide;
        el.onmousemove = that.position;
    };

    var addElement = function (t, c) {
        var x = document.createElement(t);
        
        x.className = c;
        x.style.display = "block";
        
        return(x);
    };
    
    var addCssLink = function () {
        var lk = addElement("link");
        
        lk.setAttribute("href", that.cssHRef);
        lk.setAttribute("type", "text/css");
        lk.setAttribute("rel", "stylesheet");
        lk.setAttribute("media", "screen");
        
        document.getElementsByTagName("head")[0].appendChild(lk);
    };

    var setOpacity = function (el) {
        el.style.filter = "alpha(opacity:"+that.opacity+")";
        el.style.KHTMLOpacity = "0."+that.opacity;
        el.style.MozOpacity = "0."+that.opacity;
        el.style.opacity = "0."+that.opacity;
    };

    var handleArgs = function (args, defaults) {
        if(args===null || typeof args === 'undefined') {
            args = defaults;
        } else {
            for(var i in defaults) {
                if(typeof args[i] === 'undefined') {
                    args[i] = defaults[i];
                }
            }
        }
        return args;
    };
}

