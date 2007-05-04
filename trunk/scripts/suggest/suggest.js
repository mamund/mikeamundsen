/* 
    ajax suggest library
    TODO:
    - better positioning of the list div (xy co-ords reltive to input)
    - support for arrow up/down (like mouse rollovers)
    - support for more complex return data (multi-point data, etc.)
*/

var suggest = 
{
    // object properties
	xhr:null,
	input:null,
	list:null,
	url:null,
	cssHide: 'suggest_hide',
	cssShow: 'suggest_show',
	cssOver: 'suggest_over',
	cssOut: 'suggest_out',
	cssLink: 'suggest_link',

    // initialize
	init:function(inputId, listId, urlTemplate)
	{
	    // make sure we have xmlhttp available
		suggest.xhr = suggest.getXHR();
		if(suggest.xhr===null)
		{
			alert('*** ERROR:\n\nAjax not supported!');
			return;
		}
		
		// handle args
		suggest.input = document.getElementById(inputId);
		suggest.list = document.getElementById(listId);
		suggest.url = urlTemplate;
		
		// handle events
		suggest.addEvent(suggest.input,'keyup',suggest.execute,false);
		suggest.addEvent(document,'click',suggest.hideList,false);
	},
	
	// do the work
	execute:function()
	{
		var val = '';
		
		// if no call in progress...
		if (suggest.xhr.readyState === 4 || suggest.xhr.readyState === 0) 
		{
		    // get user input
			val = escape(suggest.input.value);
			try
			{
			    // try sending to the server
				suggest.xhr.open("GET", suggest.url.replace('{@search}',val), true);
				suggest.xhr.onreadystatechange = suggest.onComplete; 
				suggest.xhr.send(null);
			}
			catch(ex)
			{
			    // oh well...
				alert('*** ERROR:\n\n'+ex.message);
			}
		}		
	},
	
	// get xmlhttprequest object
	getXHR:function()
	{
	    // try for compliant object
		if (window.XMLHttpRequest!==null) 
		{
			return new XMLHttpRequest();
		}
		
		// fallback to activex
		if(window.ActiveXObject!==null)
		{
			return new ActiveXObject("Microsoft.XMLHTTP");
		}
		
		// just give up
		return null;
	},
	
	// upon return from server call
	onComplete:function()
	{
		var data,results,elm;
		
		// server done?
		if (suggest.xhr.readyState === 4) 
		{
		    // get an error?
		    if(suggest.xhr.status!==200)
		    {
		        alert('*** ERROR:\n\n'+suggest.xhr.status+'\n'+suggest.xhr.statusText);
		        return;
		    }
    		
    		// pull the data
			data = suggest.xhr.responseText.split("\n");
			
			// build the list
			results = '';
			suggest.list.innerHTML = '';
			for(i=0; i < data.length - 1; i++) 
			{
			    // note innerHTML is faster than DOM here
				results += '<div onmouseover="suggest.mouseOver(this);" ';
				results += 'onmouseout="suggest.mouseOut(this);" ';
				results += 'onclick="suggest.click(this.innerHTML);" ';
				results += 'class="'+suggest.cssLink+'">{@results}</div>'.replace('{@results}',data[i]);
			}
			
			// stuff results into list display
			suggest.list.innerHTML += results;
			suggest.list.className=suggest.cssShow;
		}
	},
	
	// rolling over an item
	mouseOver:function(elm)
	{
	    // update style and cursor
		elm.className = suggest.cssOver;
		elm.style.cursor = 'pointer';	
	},
	
	// leaving an item
	mouseOut:function(elm)
	{
	    // revert style and cursor
		elm.className = suggest.cssLink;
		elm.style.cursor = 'default';
	},
	
	// selecting an item
	click:function(data)
	{
	    // stuff the input element
		suggest.input.value = data;
		suggest.hideList();		
	},
	
	hideList:function()
	{
		// clean up the list element
		suggest.list.innerHTML = '';
		suggest.list.className = suggest.cssHide;
		suggest.list.style.cursor = 'default';
	},
	
	// generic event-registration
	addEvent:function(elm, evType, fn, useCapture)
	{
	    var r;
	    
	    // try w3c
        if (elm.addEventListener) 
        {
            elm.addEventListener(evType, fn, useCapture);
            return true;
        } 
        
        // try msie
        if (elm.attachEvent) 
        { 
            r = elm.attachEvent('on' + evType, fn);
            return r;
        } 

        // try other
        elm['on' + evType] = fn;
    }
};
