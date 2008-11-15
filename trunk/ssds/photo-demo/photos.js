// 2008-11-10 (mca) : photo.js

window.onload = function()
{
	var coll,i;
	document.getElementById("list").style.display="block";
	
	coll = document.getElementsByTagName('a');
	for(i=0;i<coll.length;i++)
	{
		if(coll[i].getAttribute('title').indexOf('.jpg')!=-1)
		{
			coll[i].onclick = showPreview;
		}
	}
}	 

function showPreview()
{
	var link,elm;
	
	link = document.getElementById('full-view');
	link.href = this.href.replace('preview','original');
	
	elm = document.getElementById('preview');
	elm.src = this.href;
	elm.style.display = "inline";
	
	return false;
}
