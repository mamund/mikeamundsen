// context window $Date:$ $Rev:$ $Author:$

var cw={width:300,height:200,elm:'',show:function(id,elm,right,horiz,vert,width,height)
{if(cw.elm==elm&&document.getElementById(id).style.display=='block')
{cw.hide(id);return;}
if(right==null||right==undefined)
right=true;if(width==null||right==undefined)
width=cw.width;if(height==null||right==undefined)
height=cw.height;if(right==true)
{if(horiz==null||right==undefined)
horiz=10;if(vert==null||right==undefined)
vert=10;}
else
{if(horiz==null||right==undefined)
horiz=cw.width+10;if(vert==null||right==undefined)
vert=10;}
var w=document.getElementById(id);if(w!=null)
{w.style.width=width+'px';w.style.height=height+'px';w.style.display='block';w.style.visibility='visible';var my_top=cw.findPos(elm)[1];var my_left=cw.findPos(elm)[0];w.style.top=my_top+parseInt(vert,10)+'px';if(my_top+parseInt(vert,10)<0)w.style.top='0px';if(right==true)
w.style.left=my_left+elm.offsetWidth+parseInt(horiz,10)+'px';else
{w.style.left=my_left-parseInt(horiz,10)+'px';if((my_left-parseInt(horiz,10))<0)w.style.left=my_left+elm.offsetWidth+parseInt(horiz,10)+'px';}
cw.elm=elm;}},hide:function(id)
{var w=document.getElementById(id);if(w!=null)
{w.style.display='none';w.style.visibility='hidden';w.top=-999;w.left=-999;cw.elm='';}},findPos:function(obj)
{var curleft=curtop=0;if(obj.offsetParent)
{curleft=obj.offsetLeft
curtop=obj.offsetTop
while(obj=obj.offsetParent)
{curleft+=obj.offsetLeft
curtop+=obj.offsetTop}}
return[curleft,curtop];}};