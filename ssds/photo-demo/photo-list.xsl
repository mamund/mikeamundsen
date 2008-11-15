<?xml version="1.0" encoding="utf-8"?>
<!-- 2008-11-10 (mca) : convert entity list into html page -->
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:s="http://schemas.microsoft.com/sitka/2008/03/" 
	xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
	xmlns:x="http://www.w3.org/2001/XMLSchema"
	extension-element-prefixes="s xsi x">
	
	<xsl:output method="xml" 
		omit-xml-declaration="yes" 
		doctype-public="-//W3C//DTD XHTML 1.0 Strict//EN" 
		doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd" 
		encoding="utf-16"
		media-type="text/html"
		standalone="yes"/>
	
	<xsl:template match="/">
		<html xmlns="http://www.w3.org/1999/xhtml">
			<head>
				<title>SDS Photo Demo</title>
				<link href="/photo-demo/files/photos.css" type="text/css" rel="stylesheet" />
			</head>
			<body>
				<h1>SDS Photo Demo</h1>
				
				<address>
					Version 1.0 (mca) : 2008-11-08<br />
					<a href="http://amundsen.com/ssds/" title="SSDS Examples">SDS Examples</a>
				</address>
				
				<h4>Click on an image in the list to see the preview. Click on the preview image to see the full image.</h4>
				
				<div id="page">
					<div id="detail">
						<a id="full-view" title="view full image" target="_blank">
							<img id="preview" style="display:none;" />
						</a>
					</div>
					<div id="list" style="display:block;">
						<ul>
							<xsl:apply-templates select="//s:Entity" />
						</ul>
					</div>
				</div>
			</body>
			<script type="text/javascript" src="/photo-demo/home/photos.js">// *na* </script>
		</html>
	</xsl:template>

	<xsl:template match="s:Entity">
		<li>
			<a href="/photo-demo/preview/{s:Id}" title="{s:Id}">
				<img src="/photo-demo/thumbnail/{s:Id}" alt="{s:Id}" title="{s:Id} ({s:Content/@content-length} bytes)" />
			</a>
		</li>
	</xsl:template>
	
</xsl:stylesheet>
