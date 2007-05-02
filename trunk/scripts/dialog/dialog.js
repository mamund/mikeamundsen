/*
  provide simple pop-up modal dialog 
  $Date
  $Rev
  $Author
  
  dependencies:
    - dialog.css  (default classes referred below)
    - dialog.png  (transparent overlay)
*/

var md = 
{
    ovl:        null,
    dialogCss:  'md_dialog',
    popupCss:   'md_popup',
    overlayCss: 'md_overlay',

    // initialize the library    
    init:function()
    {
        // set up overlay div on the document
        md.ovl = document.createElement('div');
        md.ovl.id = md.overlayCss;
        document.body.appendChild(md.ovl);
    },
    
    // show a modal dialog
    show:function(elementId,className,eventRef)
    {
        // handle default class, if needed
        var cssName=md.dialogCss;
        if(className)
            cssName=className;
        
        // try to get the dialog element    
        var elm = document.getElementById(elementId);
        if(elm)
        {
            // run event script, if passed
            if(eventRef)
                eval(eventRef);
            
            // switch the csss    
            md.cssjs('remove',elm,md.popupCss);
            md.cssjs('add',elm,cssName);
            
            // add the element to the overlay 
            md.ovl.appendChild(elm);
            
            // toggle the view to show the dialog
            md.toggleDialog();
        }
        // cancel any internal events
        md.cancelClick(e);    
    },
    
    // hide the modal dialog
    hide:function(elementId,className,eventRef)
    {
        // handle the default class, if needed
        var cssName=md.dialogCss;
        if(className)
            cssName=className;
        
        // get the element to hide
        var elm = document.getElementById(elementId);
        if(elm)
        {
            // run any script, if passed
            if(eventRef)
                eval(eventRef);
                
            // switch the css
            md.cssjs('remove',elm,cssName);
            md.cssjs('add',elm,md.popupCss);

            // toggle the view to hide the dialog
            md.toggleDialog();
        }    
        // cancel any internal events
        md.cancelClick(e);    
    },
    
    // handle toggling the dialog on/off
    toggleDialog:function()
    {
        md.ovl.style.visibility = (md.ovl.style.visibility == "visible") ? "hidden" : "visible";
    },
    
  /* helper methods (from chris heilmann http://wait-till-i-come.com */
  getTarget:function(e){
    var target = window.event ? window.event.srcElement : e ? e.target : null;
    if (!target){return false;}
    while(!target.tohide && target.nodeName.toLowerCase()!='body')
    {
      target=target.parentNode;
    }
    // if (target.nodeName.toLowerCase() != 'a'){target = target.parentNode;} Safari fix not needed here
    return target;
  },
  cancelClick:function(e){
    if (window.event){
      window.event.cancelBubble = true;
      window.event.returnValue = false;
      return;
    }
    if (e){
      e.stopPropagation();
      e.preventDefault();
    }
  },
  addEvent: function(elm, evType, fn, useCapture){
    if (elm.addEventListener) 
    {
      elm.addEventListener(evType, fn, useCapture);
      return true;
    } else if (elm.attachEvent) {
      var r = elm.attachEvent('on' + evType, fn);
      return r;
    } else {
      elm['on' + evType] = fn;
    }
  },
  cssjs:function(a,o,c1,c2){
    switch (a){
      case 'swap':
        o.className=!md.cssjs('check',o,c1)?o.className.replace(c2,c1):o.className.replace(c1,c2);
      break;
      case 'add':
        if(!md.cssjs('check',o,c1)){o.className+=o.className?' '+c1:c1;}
      break;
      case 'remove':
        var rep=o.className.match(' '+c1)?' '+c1:c1;
        o.className=o.className.replace(rep,'');
      break;
      case 'check':
        return new RegExp("(^|\\s)" + c1 + "(\\s|$)").test(o.className)
      break;
    }
  }
}
// register the entire script for the onload event
md.addEvent(window, 'load', md.init, false);


