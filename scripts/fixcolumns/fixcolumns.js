/*  
    $Date$
    $Rev$
    $Author$
*/

var fixColumns =
{
	containerCSS : 'fx-container',
	columnCSS : 'fx-column',
	headerCSS : 'fx-header',
	footerCSS : 'fx-footer',

	init:function ()
	{
		var height,coll,divs,i,j;
		
		divs = document.getElementsByTagName('div');

		for(i=0;i<divs.length;i++)
		{
			if(divs[i].className.indexOf(fixColumns.containerCSS)!=-1)
			{
				height = divs[i].offsetHeight;
				coll = divs[i].getElementsByTagName('div');

				for(j=0;j<coll.length;j++)
				{
					if(coll[j].className.indexOf(fixColumns.headerCSS)!=-1)
					{
						height=height-coll[j].offsetHeight;
					}
					if(coll[j].className.indexOf(fixColumns.footerCSS)!=-1)
					{
						height=height-coll[j].offsetHeight;
					}
				}

				for(j=0;j<coll.length;j++)
				{
					if(coll[j].className.indexOf(fixColumns.columnCSS)!=-1)
					{
						coll[j].style.height = height+'px';
					}
				}
			}
		}
	}
};

