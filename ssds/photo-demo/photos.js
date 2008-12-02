// 2008-11-10 (mca) : photo.js

window.onload = function()
{
    var p = photoList()
    p.init();
}	 

var photoList = function()
{
    function init()
    {
	    var elm,coll,i;
    	
	    coll = document.getElementsByTagName('a');
	    for(i=0;i<coll.length;i++)
	    {
		    if(coll[i].getAttribute('title') && coll[i].getAttribute('title').indexOf('.jpg')!=-1)
		    {
			    coll[i].onclick = showPreview;
		    }
	    }
    	
	    elm = document.getElementById('load-list');
	    if(elm)
	    {
	        elm.style.display='none';
	    }
	    elm = document.getElementById('list');
	    if(elm)
	    {
	        elm.className = 'border';
	    }
	    elm = document.getElementById('photos');
	    if(elm)
	    {
	        elm.style.display='block';
	    }
	    elm = document.getElementById('source');
	    if(elm)
	    {
	        elm.style.display='block';
	    }
    }
    
    function showPreview()
    {
	    var link,elm;
    	
	    link = document.getElementById('full-view');
	    if(link)
	    {
	        link.href = this.href.replace('preview','original');
	    }
    	
	    elm = document.getElementById('preview');
	    if(elm)
	    {
	        elm.src = this.href;
	        elm.style.display = 'inline';
	    }
	    
	    return false;
    }
    
    var that = {};
    that.init = init;
    return that;
}